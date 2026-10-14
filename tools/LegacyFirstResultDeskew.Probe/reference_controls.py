"""Original chronological-membership controls and binary32 affine oracle for16F170."""
import base64,json,math,random,struct,sys
from pathlib import Path
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def add(a,b):return f(a+b)
def mul(a,b):return f(a*b)
def sub(a,b):return f(a-b)
def main(path):
    rng=random.Random(0x16f170);cases=[]
    for k in range(384):
        n=rng.randrange(1,9);physical=[[[f(rng.randrange(-160,161)/8),f(rng.randrange(-160,161)/8)]for _ in range(rng.randrange(0 if k%4==0 else 1,13))]for _ in range(n)]
        first=rng.randrange(n);last=rng.randrange(first,n);start=rng.randrange(3);end=rng.randrange(start,5)
        source=[];ranges=[]
        nonempty=[s for s in range(n)if physical[s]]
        for rowid in range(5):
            row=bytearray(rng.randbytes(720));spans=[]
            if nonempty and (k+rowid)%3:
                for j in range(rng.randrange(1,4)):
                    a=rng.choice(nonempty);z=rng.choice([s for s in nonempty if s>=a]);ap=rng.randrange(len(physical[a]));zp=rng.randrange(ap if a==z else 0,len(physical[z]));spans.append([[0,a,ap],[0,z,zp]])
            struct.pack_into('<q',row,8,len(spans));struct.pack_into('<ffff',row,24,-10,20,-15,25);source.append(row);ranges.append(spans)
        baseline=f(rng.randrange(-100,101)/4);height=f(rng.randrange(-10,101)/4);slope=rng.choice([0,f(-0.5773503),f(.5773503),f(.05),f(-.05),f(1e-6)])
        guide=bytearray(rng.randbytes(40));struct.pack_into('<fff',guide,0,baseline,height,slope);originalguide=guide[:];minimum=rng.choice([0,f(1e-6),f(2.8346457)]);preserve=k%2==0
        cosine=f(1/f(math.sqrt(mul(slope,slope)+1)));sine=f(math.sqrt(sub(1,mul(cosine,cosine))));sine=-sine if slope>0 else sine
        allpoints=[p for s in physical[first:last+1]for p in s];xmin=min((p[0]for p in allpoints),default=0);xmax=max((p[0]for p in allpoints),default=0)
        cx=mul(add(xmin,xmax),.5);cy=add(mul(cx,slope),baseline);tx=add(sub(mul(cy,sine),mul(cx,cosine)),cx);ty=add(sub(mul(-sine,cx),mul(cy,cosine)),cy)
        if not preserve:
            b=add(add(mul(baseline,cosine),mul(sine,0)),ty);h=sub(add(add(mul(add(baseline,height),cosine),mul(sine,0)),ty),b)
            h=minimum if minimum>h else h;struct.pack_into('<ff',guide,0,b,h)
        def point(p):return [add(add(mul(-sine,p[1]),mul(p[0],cosine)),tx),add(add(mul(p[1],cosine),mul(p[0],sine)),ty)]
        copied=[[p[:]for p in s]for s in physical]
        if cosine!=1 or -sine!=0 or tx!=0 or sine!=0 or ty!=0:
            for s in range(first,last+1):copied[s]=[point(p)for p in copied[s]]
        expected=[]
        for rowid,row in enumerate(source):
            out=row[:]
            if start<=rowid<end:
                if not ranges[rowid]:
                    a=point([-10,-15]);z=point([20,25]);struct.pack_into('<ffff',out,24,a[0],z[0],a[1],z[1])
                else:
                    p=[]
                    for a,z in ranges[rowid]:
                        for s in range(a[1],z[1]+1):p.extend(copied[s][a[2]if s==a[1]else 0:z[2]+1 if s==z[1]else len(copied[s])])
                    struct.pack_into('<ffff',out,24,min(q[0]for q in p),max(q[0]for q in p),min(q[1]for q in p),max(q[1]for q in p))
            expected.append(base64.b64encode(out).decode())
        cases.append(dict(physical=physical,first=first,last=last,start=start,end=end,source=[base64.b64encode(r).decode()for r in source],ranges=ranges,guide=base64.b64encode(originalguide).decode(),minimum=minimum,preserve=preserve,expectedGuide=base64.b64encode(guide).decode(),expectedRows=expected,expectedPoints=copied,affine=[cosine,sine,tx,ty]))
    out=Path(path);out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes((json.dumps(dict(cases=cases),separators=(',',':'))+'\n').encode());print(len(cases),'original affine/source-range controls')
if __name__=='__main__':main(sys.argv[1])
