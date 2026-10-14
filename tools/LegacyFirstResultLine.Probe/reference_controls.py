"""Original binary-transfer and NumPy fit-completion controls; no vendor execution."""
import base64,importlib.util,json,random,struct,sys
from pathlib import Path

def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def transfer(row,markers):
    b=bytearray(584);flags=struct.unpack_from('<I',row,20)[0];label=struct.unpack_from('<H',row,40)[0]
    b[0]=bool(flags and not flags&8192);b[1]=label in [v for v in markers[:6] if v!=65535];b[2]=label==markers[6] and markers[6]!=65535
    b[8:24]=row[24:40];count=0
    for i in range(20):
        if struct.unpack_from('<H',row,40+i*2)[0]==65535:break
        for src,dst,size in [(80,24,8),(320,184,4),(400,264,8),(560,424,8)]:b[dst+i*size:dst+(i+1)*size]=row[src+i*size:src+(i+1)*size]
        count+=1
    struct.pack_into('<I',b,4,count);return b

def main(path):
    rng=random.Random(170400);cases=[]
    for k in range(512):
        row=bytearray(720);struct.pack_into('<I',row,20,rng.choice([0,1,8192,8193,0xffffffff]))
        count=rng.randrange(21);sentinel=rng.randrange(20) if k%7==0 else count
        labels=[rng.randrange(192) for _ in range(20)]
        if sentinel<20:labels[sentinel]=65535
        markers=[rng.choice([65535,rng.randrange(192),labels[0]]) for _ in range(7)]
        struct.pack_into('<ffff',row,24,-10,25,-20,100)
        for i,label in enumerate(labels):
            struct.pack_into('<H',row,40+i*2,label)
            struct.pack_into('<ff',row,80+i*8,f(rng.randrange(-32,33)/16),f(rng.randrange(-32,33)/16))
            struct.pack_into('<i',row,320+i*4,rng.randrange(4))
            struct.pack_into('<ff',row,400+i*8,f(rng.randrange(-32,33)/16),f(rng.randrange(-32,33)/16))
            struct.pack_into('<ff',row,560+i*8,f(rng.randrange(-32,33)/16),f(rng.randrange(-32,33)/16))
        cases.append(dict(source=base64.b64encode(row).decode(),markers=markers,expected=base64.b64encode(transfer(row,markers)).decode()))
    spec=importlib.util.spec_from_file_location('original_irls',Path(__file__).resolve().parent.parent/'LegacyCharacterLineFit.Probe/reference_controls.py');m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
    fits=[]
    for c in m.create()['cases'][:128]:
        out=c['expected']['output'];allb=[i['bounds'] for i in c['items']]
        b=[min(v[0] for v in allb),min(v[2] for v in allb),max(v[1] for v in allb),max(v[3] for v in allb)]
        if len(fits)%3==0:b[1]=f(b[1]-10000);b[3]=f(b[3]+10000)
        frac=f(f(b[3]-b[1])*.05);accept=frac<=out[1]
        result=out if accept else [f(f(b[3]+b[1])*.5),max(frac,c['profile'][4]),0]
        fits.append(dict(case=c,bounds=b,output=result,basis=0 if accept else 2))
    fallbacks=[]
    for k in range(128):
        minimum=f((k%17+1)*.25);x=f((k%13)*.125);y=f((k%11)*.125)
        width=minimum if k%3==0 else x;height=minimum if k%4==0 else y
        b=[-4.,7.,f(-4+width),f(7+height)];w=f(b[2]-b[0]);h=f(b[3]-b[1]);frac=f(h*.05)
        out=[b[3],minimum,0] if w<minimum and h<minimum else [f(f(b[3]+b[1])*.5),max(frac,minimum),0]
        fallbacks.append(dict(bounds=b,minimum=minimum,output=out))
    profiles=[]
    for dpi in [0,1,96,300]:
        for n,line in [(0,[]),(1,[1,0,100]),(1,[2,0,0]),(1,[2,0,120]),(1,[2,0,0xffffffff]),(2,[])]:
            profiles.append(dict(dpi=dpi,count=n,line=line,minimum=1e-6 if dpi==0 else f(f(dpi/f(25.4))*.75),width=f(f(line[2])*.25) if n==1 and line[0]>=2 and line[2]>0 else f(3.4028234663852886e38)))
    out=Path(path);out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes((json.dumps(dict(cases=cases,fits=fits,fallbacks=fallbacks,profiles=profiles),separators=(',',':'))+'\n').encode());print(len(cases),'transfers,',len(fits),'NumPy completions,',len(fallbacks),'empty primary fallbacks,',len(profiles),'actual config input variants')
if __name__=='__main__':main(sys.argv[1])
