"""Independent fresh C51 registry/0210C0 command oracle. No native execution."""
import random,json,sys,hashlib
from pathlib import Path
r=random.Random(0x210c0)
streams=[]
for i in range(1024):
 n=1+r.randrange(256)
 ids=list(range(n));r.shuffle(ids)
 registry={};next_id=0
 for key in ids:
  assert key not in registry
  registry[key]=True
  if next_id<=key:next_id=key+1
 assert next_id==n
 req=[];pending=[]
 for j in range(1+r.randrange(20)):
  pairs=[[r.getrandbits(64),r.getrandbits(64)]for _ in range(r.randrange(8))]
  if j%3==0:pairs=[[0xffffffffffffffff,0],[0,0xffffffffffffffff],[0,0xffffffffffffffff]]
  kind=r.randrange(3);stage=(1+r.randrange(4))if j>0 and r.randrange(10)==0 else 0
  out=-123456789-j
  used=next_id;next_id+=1
  if not stage:
   out=used
   pending.append(dict(id=used,name=['CHAR','WORD','TEXT'][kind],ranges=pairs))
  req.append(dict(kind=kind,stage=stage,residue=-123456789-j,ranges=pairs,
   output=out,next=next_id,commandCount=len(pending)))
  if stage:break
 streams.append(dict(objects=n,requests=req,pending=pending))
out=Path(sys.argv[1]);data=(json.dumps(dict(streams=streams),separators=(',',':'))+'\n').encode();out.write_bytes(data)
print('PASS1024 independent registry/transaction streams SHA256',hashlib.sha256(data).hexdigest())
