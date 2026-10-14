"""Original independent native-caller vector controls; no vendor execution or golden OCR labels."""
import argparse,json,pathlib,random,struct

TEXT=['a',' ','\u2639','\u263a','\u2639\u263a','x','\n']
ALT=['','\u2639','\u263a','\U000f0001','\U000f0000','x']
def alphabet():
    data=bytearray(struct.pack('<8H',0,0,0,2,6,1,0,len(TEXT)))
    data+=struct.pack('<2H',1,0)
    for n in [32,64]:data+=struct.pack('<'+'H'*n,0,*([65535]*(n-1)))
    data+=struct.pack('<2H',0,len(TEXT));offset=0
    for i,t in enumerate(TEXT):data+=struct.pack('<II',offset,i);offset+=len(t.encode())
    data+=struct.pack('<II',offset,65535)+''.join(TEXT).encode()
    data+=struct.pack('<'+'H'*len(TEXT),*range(len(TEXT)))+b'\0\0\0'
    return data

def main():
    p=argparse.ArgumentParser();p.add_argument('output');a=p.parse_args();rng=random.Random(213000)
    rows=[];records=bytearray();descs=[]
    for kind,symbol in [(0,i) for i in range(7)]+[(2,2),(2,3),(1,0),(1,8),(4,0),(4,1),(4,2),(4,3),(4,4),(4,5)]:
        flags=rng.randrange(65536);pub=rng.randrange(65536);r=bytearray(rng.randbytes(40));struct.pack_into('<3H',r,4,(flags&~7)|kind,pub,symbol)
        records+=r;descs.append((kind,pub,symbol))
    for test in range(768):
        prepared=test>=512;indices=[rng.randrange(len(descs)-(0 if prepared else 2)) for _ in range(rng.randrange(0,36))]
        if not prepared:indices=[i for i in indices if i!=15] #actualF0000 path is not claimed by ordinaryhelper
        selected=b''.join(struct.pack('<HH',i,rng.randrange(65536)) for i in indices)
        filter=bool(rng.randrange(2));marker=rng.randrange(-3,len(indices)+4);cur=0;kept=[];emitted=[];expectedMarker=marker
        for j,i in enumerate(indices):
            kind,pub,sym=descs[i];remove=filter and kind==0 and sym==2
            if not remove:
                text=ALT[sym] if kind==4 else (('\n'*(sym>>2)) if kind==1 else TEXT[sym])
                remove=not text
                if not remove:
                    if text=='\u2639':text='\U0001f641'
                    elif text=='\u263a':text='\U0001f642'
                    emitted.append([text.encode().hex(),(pub&1023)|(0x100000 if text in ['\U000f0000','\U000f0001'] else 0)])
            if remove:
                if cur<expectedMarker:expectedMarker-=1
            else:cur+=1;kept.append(selected[j*4:j*4+4])
        rows.append(dict(prepared=prepared,selection=selected.hex(),filter=filter,marker=marker,expectedMarker=expectedMarker,
                         retained=b''.join(kept).hex(),expected=emitted))
    out=dict(alphabet=alphabet().hex(),records=records.hex(),packedCount=(len(descs)<<2)|3,
             alternate=[t.encode().hex() for t in ALT],cases=rows)
    pathlib.Path(a.output).write_bytes((json.dumps(out,separators=(',',':'))+'\n').encode());print(len(rows))
if __name__=='__main__':main()
