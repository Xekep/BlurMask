import base64, json, random, struct, sys
from pathlib import Path

def f(x): return struct.unpack('<f', struct.pack('<f', x))[0]
def profile():
    b=bytearray(struct.pack('<HH',0x201,8))
    def ids(values):
        b.append(len(values)); b.extend(bytes((-len(b))&3))
        b.extend(struct.pack('<'+'i'*len(values),*values))
    ids([0,1,2]); b.extend(bytes([0,1,2,0,1,2,0,1,2,0,1]))
    ids([0,1,2]); start=[0,1,2,-1,1,0,2,0,1,2,-1]; end=[1,2,-1,0,2,1,0,-1,2,0,1]
    b.extend(bytes(x&255 for x in start)); b.extend(bytes(x&255 for x in end))
    prototypes=[(0.,1.,2.),(.25,.75,3.),(-.5,1.5,4.)]
    for p in prototypes: b.extend(struct.pack('<fffB',*p,0))
    return b,start,end,prototypes

def main(path):
    rng=random.Random(157030); p,start,end,prototypes=profile(); cases=[]
    for test in range(512):
        n=rng.randrange(1,16); strokes=[[[f(rng.randrange(-100,101)/4),f(rng.randrange(-100,101)/4)] for _ in range(rng.randrange(1,12))] for _ in range(n)]
        mapping=sorted(rng.sample(range(n),rng.randrange(1,n+1)))
        ranges=[]; projected=[]
        for _ in range(rng.randrange(1,8)):
            a=rng.randrange(len(mapping)); z=rng.randrange(a,len(mapping)); ap=rng.randrange(len(strokes[mapping[a]])); zp=rng.randrange(ap if a==z else 0,len(strokes[mapping[z]]))
            ranges.append([[0,a,ap],[0,z,zp]])
            # Independent membership partition: enumerate mapped physical
            # strokes, collect maximal contiguous parent runs.
            run=[]
            for s in range(a,z+1):
                if run and mapping[s]!=run[-1][0]+1:
                    projected.append([[0,run[0][0],run[0][1]],[0,run[-1][0],run[-1][2]]]);run=[]
                run.append((mapping[s],ap if s==a else 0,zp if s==z else len(strokes[mapping[s]])-1))
            projected.append([[0,run[0][0],run[0][1]],[0,run[-1][0],run[-1][2]]])
        alts=[]; altbytes=bytearray()
        for i in range(rng.randrange(1,45)):
            label=rng.randrange(11); prob=f(rng.randrange(1,17)/16); score=f(rng.randrange(-100,101)/8);cost=rng.randrange(-100000,100000)
            alts.append((label,prob,score,cost));row=bytearray(40)
            struct.pack_into('<H',row,8,label);struct.pack_into('<ffi',row,12,prob,score,cost);altbytes.extend(row)
        selected=rng.randrange(len(alts)); first=alts[selected]; labels=[first]
        for a in alts:
            if not any(a[0]==old[0] for old in labels) and len(labels)<20: labels.append(a)
        result=bytearray(720);struct.pack_into('<qi',result,8,len(projected),-1)
        points=[]
        for a,b in projected:
            for s in range(a[1],b[1]+1): points.extend(strokes[s][a[2] if s==a[1] else 0:(b[2]+1) if s==b[1] else len(strokes[s])])
        struct.pack_into('<ffff',result,24,min(x for x,y in points),max(x for x,y in points),min(y for x,y in points),max(y for x,y in points))
        for i in range(20):
            if i>=len(labels): struct.pack_into('<H',result,40+i*2,65535);struct.pack_into('<i',result,240+i*4,0x4000000);continue
            label,prob,score,cost=labels[i];struct.pack_into('<H',result,40+i*2,label);struct.pack_into('<ff',result,80+i*8,f(prob**2) if i==0 else prob,score);struct.pack_into('<i',result,240+i*4,cost)
            if start[label]==-1:continue
            proto=prototypes[start[label]]; lower=f(-1-f(proto[0]*2));upper=f(f(proto[1]*2)+lower)
            struct.pack_into('<f',result,560+i*8,lower);struct.pack_into('<f',result,400+i*8,upper)
            struct.pack_into('<i',result,320+i*4,1 if end[label]==-1 else 2)
            if end[label]!=-1: struct.pack_into('<f',result,564+i*8,lower);struct.pack_into('<f',result,404+i*8,upper)
        cases.append(dict(strokes=strokes,mapping=mapping,ranges=ranges,alternatives=base64.b64encode(altbytes).decode(),selected=selected,count=len(labels),projected=projected,expected=base64.b64encode(result).decode()))
    output=Path(path);output.parent.mkdir(parents=True,exist_ok=True)
    output.write_text(json.dumps(dict(profile=base64.b64encode(p).decode(),cases=cases),separators=(',',':')))
    print(f'Generated {len(cases)} independent source row controls')
if __name__=='__main__':main(sys.argv[1])
