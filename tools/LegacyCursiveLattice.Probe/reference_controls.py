"""Independent ordinary21b graph interpreter, with mutable reciprocal arc lists."""
import json,random,sys
rng=random.Random(193690)
cases=[]
for seed in range(768):
 n=rng.randrange(1,48);limit=rng.randrange(1,16)
 ranges=[];stroke=0;point=0
 for i in range(n):
  if rng.random()<.17:stroke+=1;point=0
  length=rng.randrange(1,8) if i==n-1 else rng.randrange(2,8);ranges.append([stroke,point,point+length]);point+=length-1
 # Native line order may differ from physical ink order.
 if seed%3==0:rng.shuffle(ranges)
 ids=[];last=-1
 for i in range(n):
  if rng.random()<.15:last=rng.choice([-1,0,1,3,8])
  ids.append(last)
 nodes=[]
 for i in range(n):
  flags=4
  if i and ids[i]!=-1:
   flags|=8 if ids[i]!=ids[i-1] else 0
   flags|=16 if ids[i-1]+1<ids[i] else 0
  nodes.append([dict(id=i,first=i,flags=flags,atoms=[i])])
 def composed_for(indices):
  composed=[]
  for j in sorted(indices,key=lambda j:ranges[j][:2]):
   r=ranges[j]
   if composed and composed[-1][0]==r[0] and composed[-1][2]==r[1]+1:composed[-1][2]=r[2]
   else:composed.append(r.copy())
  return composed
 intern={}
 next_id=n
 for left in range(n-1):
  for arc in list(nodes[left]):
   right=nodes[left+1][0]
   if right['flags']&8 or left+1-arc['first']>=limit:continue
   combined=arc['atoms']+right['atoms'];key=tuple(map(tuple,composed_for(combined)))
   if key not in intern:intern[key]=next_id;next_id+=1
   nodes[left+1].append(dict(id=intern[key],first=arc['first'],flags=arc['flags'],atoms=combined))
 out=[]
 for node,arcs in enumerate(nodes):
  result=[]
  for arc in arcs:
   order=sorted(arc['atoms'],key=lambda j:ranges[j][:2]);composed=[]
   for j in order:
    r=ranges[j]
    if composed and composed[-1][0]==r[0] and composed[-1][2]==r[1]+1:composed[-1][2]=r[2]
    else:composed.append(r.copy())
   successors=[]
   for dest in range(node+1,n):
    for a,candidate in enumerate(nodes[dest]):
     if candidate['first']==node+1:successors.append([dest-node,a])
   first=arc['first'];pred=[-1] if first==0 else list(range(len(nodes[first-1])))
   result.append([arc['id'],first,node-first,arc['flags'],order,composed,pred,successors])
  out.append(result)
 cases.append(dict(ranges=ranges,ids=ids,limit=limit,nodes=out))
json.dump(cases,open(sys.argv[1],'w'),separators=(',',':'))
print('Wrote',len(cases),'independent ordinary candidate graph controls')
