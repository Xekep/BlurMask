import json,math,struct,random,sys
from pathlib import Path
r=random.Random(0x1be530)
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
controls=[]
for k in range(512):
 n=k%31;scale=f([.5,1,1.5,2,4][k%5]);costs=sorted(r.randrange(-2000,100000) for _ in range(n));weights=[];s=f(0)
 if n==1:weights=[1.0]
 else:
  for cost in costs[:20]:
   v=f(math.exp(f(-f(cost)/f(scale*8192))));weights.append(v);s=f(s+v)
  weights=[f(v/s) for v in weights]+[0.0]*max(0,n-20)
 controls.append(dict(costs=costs,scale=scale,weights=weights,bytes=[max(1,min(255,int(f(v*255)))) for v in weights]))
target=Path(sys.argv[1]);target.parent.mkdir(parents=True,exist_ok=True);target.write_text(json.dumps(controls))
