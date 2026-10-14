"""Independent scalar1941B0 float chain/lookup/round/cap controls."""
from pathlib import Path
import json,math,random,struct,sys
def f(x):
 try:return struct.unpack('<f',struct.pack('<f',x))[0]
 except OverflowError:return math.copysign(math.inf,x)
table=[f(-math.log1p(math.exp(-(-4+8*i/255)))) for i in range(256)]
r=random.Random(0x1941b010);cases=[]
for i in range(2048):
 scale=10**r.randint(-2,12);lo=f(r.uniform(-5,5)*scale);hi=f(lo+r.uniform(0,8)*scale);prior=f(r.uniform(-5,5)*scale)
 height=f(10**r.uniform(-2,5));threshold=f(r.uniform(-3,3));slope=f(r.uniform(-40,40));maximum=bool(i%2)
 x=hi if maximum else lo;transformed=f(f(f(f(x-prior)/height)-threshold)*-slope)
 if transformed>4:cost=0
 else:
  log=transformed if transformed<-4 else table[int(float(f(transformed+4))*255+.5)>>3]
  value=f(-log*8192);cost=0x4000000 if value>=0x4000000 else math.floor(value+.5)
 cases.append(dict(low=lo,high=hi,prior=prior,height=height,threshold=threshold,slope=slope,maximum=maximum,cost=cost))
if len(sys.argv)!=2 or Path(sys.argv[1]).suffix.lower()!='.json':raise ValueError('Supply one output.json')
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(dict(table=table,cases=cases),separators=(',',':')),encoding='utf-8');print('Generated2048 independent blank cache10 controls.')
