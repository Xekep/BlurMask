"""Independent immutable-graph and object-buffer cache oracle; no vendor/model execution."""
import argparse,json,random
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('output',type=Path);a=p.parse_args();r=random.Random(0x1520b0);tests=[]
for case in range(256):
 n=r.randint(2,6);cap=r.randint(2,5)
 arcs=[];next_group=n
 for node in range(n):
  row=[(node,0)]
  for span in range(1,min(node,cap-1)+1):row.append((next_group,span));next_group+=1
  arcs.append(row)
 pool=[];record={};calls=0;ops=[]
 for step in range(24):
  node=r.randrange(n);scope=r.randrange(cap+1);allow=bool(r.randrange(2))
  # Guaranteed adjacent duplicate and empty selection regressions.
  if step==0:node,scope,allow=n-1,0,False
  if step==1:node,scope,allow=n-1,0,False
  if step==2:node,scope,allow=n-1,cap,True
  present={g for g,s in arcs[node]}
  for entry in pool:
   if entry['scope']==scope and entry['id'] not in present:entry['id']=-1
  emitted=[];new=reused=0;sets=[set(),set(),{1,2}]
  for ordinal,(group,span) in enumerate(arcs[node]):
   if span!=scope:continue
   prior=record.get(group)
   needs=not(allow and prior is not None) and all(e['id']!=group for e in pool)
   if needs:
    entry=next((e for e in pool if e['id']==-1),None)
    if entry is None:entry={'id':-1,'scope':scope,'costs':[]};pool.append(entry)
    calls+=1;new+=1;entry['id']=group;entry['scope']=scope
    entry['costs']=[(0x4000000,39999,40000,40001,-1,100,0x4000001)[(group*13+calls*5+symbol*11)%7] for symbol in range(6)]
    record[group]=entry
   else:reused+=1;entry=record[group]
   costs=entry['costs'][:];emitted.append(dict(node=node,ordinal=ordinal,group=group,costs=costs,recomputed=needs))
   for i,cost in enumerate(costs[:3]):
    if cost<0x4000000:sets[i].update((1,2))
    if cost<40000:sets[i].add(4)
  permission=[sum(v) for v in sets]
  ops.append(dict(node=node,scope=scope,allow=allow,new=new,reused=reused,calls=calls,emitted=emitted,permission=permission))
 tests.append(dict(n=n,cap=cap,ops=ops))
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(tests,separators=(',',':')),encoding='utf8');print(f'Generated {len(tests)} independent lifecycle traces / {sum(len(t["ops"]) for t in tests)} cache passes')
