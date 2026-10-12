"""Original aggregate stream oracle; no vendor execution or copied fixture data."""
import argparse, hashlib, json, random, struct
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('output',type=Path);args=p.parse_args();rng=random.Random(0x236910)
def record(seed,node,index): return hashlib.sha256(struct.pack('<III',seed,node,index)).digest()
traces=[]
for t in range(384):
 children=[dict(seed=rng.randrange(1,2**32),normal=[rng.randrange(0,12) for _ in range(5)],aux=[rng.randrange(0,8) for _ in range(5)],owners=[bool(rng.randrange(3)) for _ in range(4)]) for _ in range(rng.randrange(0,9))]
 current=0;bases=[-1]*len(children);global_skip=0;ops=[]
 for step in range(32):
  startup=not children or rng.randrange(3)!=0
  node=0 if startup else rng.randrange(0,5);sid=0 if startup else rng.randrange(1,len(children)+1)
  header=8 if startup else rng.randrange(0,256);owner=rng.randrange(4);aux=bool(rng.randrange(2));clear=bool(rng.randrange(2));cap=rng.randrange(1,9)
  skip=global_skip if startup else rng.randrange(0,16)
  if startup and (step==0 or rng.randrange(4)==0):skip=0;global_skip=0;current=0;bases=[-1]*len(children)
  if startup and skip==0:current=0;bases=[-1]*len(children)
  calls=[];elig=[];ordinal=-1;count=0;out=b''
  def read_child(i,offset):
   calls.append(dict(child=i,node=node,skip=offset,aux=aux,clear=clear,owner=owner))
   total=children[i]['aux' if aux else 'normal'][node]
   n=min(cap,total-offset)
   payload=b''.join(record(children[i]['seed'],node,j) for j in range(offset,offset+max(0,n)))
   return n,payload
  if startup:
   while current<len(children):
    if bases[current]<0:bases[current]=skip
    elig.append(current)
    if children[current]['owners'][owner]:
     count,out=read_child(current,skip-bases[current])
     if count!=0:ordinal=current;break
    current+=1
   if ordinal<0:count=0
  else:
   ordinal=sid-1;elig.append(ordinal)
   if children[ordinal]['owners'][owner]:count,out=read_child(ordinal,skip)
   else:count=-1
  stamped=bytearray(out)
  for i in range(max(0,count)):stamped[i*32+4]=ordinal+1
  ops.append(dict(node=node,sid=sid,header=header,owner=owner,aux=aux,clear=clear,skip=skip,capacity=cap,count=count,ordinal=ordinal,current=current,bases=list(bases),calls=calls,eligibility=elig,records=bytes(stamped).hex(),costs=(bytes(max(0,count)*12) if clear else b'').hex()))
  if startup and count>0:global_skip=skip+count
 traces.append(dict(children=children,ops=ops))
args.output.parent.mkdir(parents=True,exist_ok=True);args.output.write_text(json.dumps(traces,separators=(',',':')),encoding='utf8')
print(f'Generated {len(traces)} independent stream traces / {sum(len(t["ops"]) for t in traces)} dispatch pages')
