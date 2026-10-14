"""Independent ordinary167210 start/end records and168e10 prepared metadata oracle."""
import json,random,struct,sys
from pathlib import Path
rng=random.Random(0x168e10)
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def different(a,b):return abs(float(a))+abs(float(b))<abs(float(f(a-b)))*8388608
cases=[]
for test in range(512):
    raw=[];keys=[]
    for physical in range(1+test%7):
        points=[]
        for p in range(1+rng.randrange(16)):
            value=[f(rng.randrange(-64,65)/4),f(rng.randrange(-64,65)/4)]
            if points and rng.randrange(3)==0:value=points[-1].copy()
            elif points and rng.randrange(5)==0:
                value=[f(points[-1][0]+rng.choice([-1,1])*1e-7),points[-1][1]]
            points.append(value)
        raw.append(points);retained=[]
        for p,value in enumerate(points):
            if p==0 or different(value[0],points[p-1][0])or different(value[1],points[p-1][1]):retained.append(p)
        for j,start in enumerate(retained):keys.append([0,physical,start,(retained[j+1]if j+1<len(retained)else len(points))-1])
    queries=[]
    for _ in range(len(keys)*3):
        ordinal=rng.randrange(len(keys));remainder=rng.choice([0,1,50,199])if ordinal+1<len(keys)else 0
        index=ordinal+(remainder!=0);key=keys[index]
        queries.append([ordinal*200+remainder,key[0],key[1],key[3],index,(200-remainder)%256 if remainder else 0])
    cases.append(dict(raw=raw,keys=keys,queries=queries))
Path(sys.argv[1]).write_text(json.dumps(dict(cases=cases)),encoding='utf-8')
print('generated512 independent raw source/dual ending metadata controls')
