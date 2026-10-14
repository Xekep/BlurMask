"""Original controls: explicit cluster examples and independently calculated scalar flags; not captured vendor outputs."""
import argparse,json,pathlib,random

def main():
    p=argparse.ArgumentParser();p.add_argument('model');p.add_argument('output');a=p.parse_args()
    d=json.loads(pathlib.Path(a.model).read_text())
    rows=[(bytes.fromhex(r[0])+b'\0',r[1:]) for r in d['literalRows']]
    def literal(raw):
        raw=raw.split(b'\0')[0]+b'\0';left,right,depth=0,len(rows),0
        while left<right:
            m=(left+right)//2;s,f=rows[m];x=s[depth];y=raw[depth]
            if y<x:right=m
            elif y>x:left=m+1
            elif depth<2 and y:left,right,depth=m+f[1+2*depth],m+f[2+2*depth]+1,depth+1
            else:return f[0] if s[depth:]==raw[depth:] else -1
        return -1
    def script(cp):
        return next((k for x,y,k in d['scriptRanges'] if x<=cp<=y),0)
    def flags(text,n,seed,glyph):
        out=(seed&~1023)|(glyph&1023)
        values=list(map(ord,text[1:] if text.startswith('\ufeff') else text))
        special=n==0
        if n:
            cp=values[0];sc=script(cp)
            if sc in [7,8,9] or (sc!=10 and 0x3000<=cp<=0x303f):out|=0x40000
            elif sc==10:out|=0x80000
            bit=0
            if n==2 and values[:2]==[13,10]:bit=0x2000
            elif n==1:
                if cp in [13,10,0x2028,0x2029]:bit=0x2000
                elif cp in [9,11,12,32,160]:bit=0x1000
                elif cp==0xad:bit=0x4000
                elif cp==0x200c:bit=0x8000
                elif cp in [0x61c,0x200e,0x200f,*range(0x202a,0x202f),*range(0x2066,0x206a)]:bit=0x20000
            if bit:out|=bit;special=True
        raw=text.encode()
        if not special:out=(out&~0x10000)|(0x10000 if literal(raw)>=0 else 0)
        return out
    cases=[];rng=random.Random(61000)
    def add(text,n):
        seed=rng.randrange(2**32);glyph=rng.randrange(65536)
        cases.append(dict(bytes=text.encode().hex(),count=n,seed=seed,glyph=glyph,flags=flags(text,n,seed,glyph)))
    for cp in list(range(256))+[0x61c,0x200c,0x200e,0x200f,*range(0x2028,0x202f),*range(0x2066,0x206a),0x3000,0x303f,0x3042,0x30ab,0xac00,0xd7a3,0x4e00,0x10400,0x1f1e6,0x10ffff]:
        add(chr(cp),1);add(chr(cp)+'x',1 if cp!=0x83 else 2)
    for h,*_ in d['literalRows']:
        s=bytes.fromhex(h).decode()
        add(s,len(s));add(s+'\0abc',len(s))
    for text,count in [('',0),('\ufeff',0),('\ufeffa',1),('\ufeff\ufeffa',1),('\r\nabc',2),('a\u0301b',2),('a\u0301\u0308b',3),
                       ('\u1100\u1161\u11a8x',3),('\uac00\u11a8x',2),('\uac01\u11a8x',2),
                       ('\U0001f1e6\U0001f1e7\U0001f1e8',2),('\u0644\u0627x',2),
                       ('\u0627\u0651x',1),('a\u200cx',1),('\u0600ax',2),('a\u200dx',1),
                       ('\U0001f469\u200d\U0001f467x',3),('\u0915\u094d\u0915x',3),
                       ('\u0a95\u0acd\u0a95x',3),('\u0b95\u0bcd\u0b95x',3),('a\u0083b',3),
                       ('\u0e01\u0e33x',1),('!\0x',1),('\nabc',1)]:add(text,count)
    pathlib.Path(a.output).write_bytes((json.dumps(dict(cases=cases),separators=(',',':'))+'\n').encode());print(len(cases))

if __name__=='__main__':main()
