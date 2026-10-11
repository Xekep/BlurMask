"""Original independent set-union oracle; no model/vendor code is executed."""
import argparse,json,random,struct
from pathlib import Path

def f32(v): return struct.unpack('<f',struct.pack('<f',v))[0]
p=argparse.ArgumentParser();p.add_argument('output',type=Path);a=p.parse_args()
r=random.Random(0x1c01b0); out=[]
for case in range(512):
 n=12;flags=0x800 if case&1 else 0xc00
 reserved=[0,1,2,65535,4,65535];rows=[];sets=[set() for _ in range(n)]
 for i in reserved:
  if i!=65535:sets[i].update((1,2))
 state=False
 for j in range(6):
  scale=f32(r.choice([0,.000025,.5,.999975,1,1.000025,12,1677.722]))
  threshold=int(f32(scale*40000))
  kind=r.randrange(3); recompute=bool(r.randrange(2)); present=bool(r.randrange(2)); bits=r.randrange(256)
  # First two rows force stale-state behavior, including an ordinary successor.
  if j==0:kind,recompute,present,bits=1,True,True,7
  if j==1:kind,recompute,present,bits=0,True,False,0
  costs=[r.choice([-1,0,threshold-1,threshold,threshold+1,0x3ffffff,0x4000000,0x4000001,2147483647,-2147483648]) for _ in range(n)]
  rows.append(dict(costs=costs,scale=scale,recomputed=recompute,kind=kind,present=present,bits=bits))
  if recompute and kind==1:state=present and bool(bits&7)
  for i,cost in enumerate(costs):
   if cost<0x4000000:sets[i].add(1);sets[i].update(() if state else (2,))
   if cost<threshold:sets[i].add(4)
 permission=[sum(bits) for bits in sets]
 original=list(range(n))+[0xfffb,0xffff]
 need={1,4} if flags==0x800 else {1}
 working=[v if v>=n or need.issubset(sets[v]) else 65535 for v in original]
 out.append(dict(count=n,flags=flags,reserved=reserved,original=original,rows=rows,permission=permission,working=working))
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(out,separators=(',',':')),encoding='utf8');print(f'Generated {len(out)} independent permission/working-map controls')
