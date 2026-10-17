"""Independent ordinary cross geometry/allocation + mutable boundary oracle.
Local snapshots are separately validated inputs, not fabricated actual NN rows.
"""
import base64,copy,json,pathlib,struct,sys
j=json.loads(pathlib.Path(sys.argv[1]).read_bytes())
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def bits(x):return struct.unpack('<i',struct.pack('<f',x))[0]
def frombits(x):return struct.unpack('<f',struct.pack('<i',x))[0]
def i(b,n):return struct.unpack_from('<i',b,n)[0]
def metric(b,n):return struct.unpack_from('<f',b,n)[0]
def quick(rows):
 a=copy.deepcopy(rows)
 def sort(lo,hi):
  x,y=lo,hi;p=a[(lo+hi)//2][0][:2]
  while x<=y:
   while a[x][0][:2]<p:x+=1
   while a[y][0][:2]>p:y-=1
   if x<=y:a[x],a[y]=a[y],a[x];x+=1;y-=1
  if lo<y:sort(lo,y)
  if x<hi:sort(x,hi)
 if len(a)>1:sort(0,len(a)-1)
 return a
def compose(history):
 out=[]
 for row in history:
  for r in row:
   if out and out[-1][0]==r[0] and out[-1][2]-1==r[1]:out[-1][2]=r[2]
   else:out.append(r[:])
 return out
def append(state,local):
 out=copy.deepcopy(state);old=len(out['nodes']);base=len(out['groups']);aux=len(out['contexts']);cap=out['cap'];mapping={}
 for n,g in enumerate(local['groups']):
  if not g['fields'][3]:continue
  mapping[n]=len(out['groups']);g=copy.deepcopy(g)
  if g['fields'][2]!=65535:g['fields'][2]+=aux
  keep=any((node<cap or node==len(local['nodes'])-1) and any(a['header'][0]==n for a in row) for node,row in enumerate(local['nodes']))
  if keep:g['fields'][4]=1
  else:g['history']=[]
  out['groups'].append(g)
 out['contexts']+=copy.deepcopy(local['contexts'])
 for row in local['nodes']:
  row=copy.deepcopy(row)
  for a in row:
   h=a['header'];h[0]=mapping[h[0]];h[4]=(h[4]+((old&0xffffff)<<7))&0xffffffff;h[5]=(h[5]+((old&0xffffff)<<7))&0xffffffff
   if h[6]!=65535:h[6]+=aux
  out['nodes'].append(row)
 if not old:out['root']=copy.deepcopy(local['root'])
 else:
  width=len(out['nodes'][old-1])
  for selector in range(min(cap,len(local['nodes']))):
   roots=[n for n,a in enumerate(local['nodes'][selector]) if a['previous']==[-1]]
   for n in roots:out['nodes'][old+selector][n]['previous']=list(range(width))
   for n in range(width):out['nodes'][old-1][n]['next'][selector]+=roots
 return out
def geometry(a,b,ga,gb,left,right):
 if i(a,8)!=-1 and i(b,8)!=-1 and i(a,8)!=i(b,8):return False
 if i(a,4)!=-2147483648 and i(b,4)!=-2147483648 and i(a,4)!=i(b,4):return False
 la,lb=i(a,16),i(b,16)
 if la!=-1 and lb!=-1 and lb-la>1:return False
 if la==-1 or lb==-1 or i(a,40)-i(a,36)+1<=1 and i(b,40)-i(b,36)+1<=1:return True
 l=[frombits(v) for v in left['boundsBits']];r=[frombits(v) for v in right['boundsBits']]
 lowx,lowy,highx,highy=min(l[0],r[0]),min(l[1],r[1]),max(l[2],r[2]),max(l[3],r[3])
 delta=abs(f(metric(gb,12)-metric(ga,12)));height=max(metric(ga,4),metric(gb,4));allow=f(max(f(l[3]-l[1]),height)+delta)
 if f(highx-lowx)<=allow:
  gap=f(l[0]-r[2])
  if gap<0:gap=f(r[0]-l[2])
  if gap<=0 or f(gap+gap)<=f(r[1]-l[3]):return True
 return not(f(height*6)<f(highy-lowy) or f(height*6)<f(highx-lowx))
def cross(s,leftnode,required,rightnode,descriptors,guides,events):
 groups=s['groups'];nodes=s['nodes'];cap=s['cap']
 if required+rightnode-leftnode>=cap:return
 for ln,left in enumerate(nodes[leftnode]):
  h=left['header']
  if h[1]!=required:continue
  lg=groups[h[0]];a=descriptors[lg['fields'][0]]
  if i(a,0)&15 not in (0,6) or i(a,32)==0x2000000:continue
  for rn in left['next'][rightnode-leftnode-1][:]:
   right=nodes[rightnode][rn];rh=right['header'];rg=groups[rh[0]];b=descriptors[rg['fields'][0]]
   if rh[3]&8 or lg['fields'][0]==rg['fields'][0] or i(b,0)&15 not in (0,6) or i(b,32)==0x2000000:continue
   if not geometry(a,b,guides[i(a,16)] if i(a,16)!=-1 else None,guides[i(b,16)] if i(b,16)!=-1 else None,lg,rg):continue
   history=quick(lg['history']+rg['history']);ranges=compose(history)
   group=next((g for g,v in enumerate(groups) if v['ranges']==ranges),None)
   if group is None:
    group=len(groups);ba=list(map(frombits,lg['boundsBits']));bb=list(map(frombits,rg['boundsBits']))
    groups.append(dict(fields=[lg['fields'][0],rg['fields'][0],lg['fields'][2],0,0,0,int(bool(lg['fields'][6] or rg['fields'][6])),0],
     boundsBits=list(map(bits,[min(ba[0],bb[0]),min(ba[1],bb[1]),max(ba[2],bb[2]),max(ba[3],bb[3])])),ranges=ranges,history=history))
    events.append([0,leftnode,ln,group,rightnode,rn])
   existing=next((v for v in nodes[leftnode+1] if v['header'][0]==group and all(p in v['previous'] for p in left['previous']) and all(p in v['next'][0] for p in right['next'][0])),None)
   if existing is not None:
    if not(h[3]&rh[3]&1):existing['header'][3]&=254
    continue
   ordinal=len(nodes[rightnode]);span=h[1]+rightnode-leftnode
   new=dict(header=[group,span,0,(h[3]&30)|(h[3]&rh[3]&1),h[4],rh[5],h[6],0,0],previous=left['previous'][:],next=[[] for _ in range(cap)])
   nodes[rightnode].append(new);groups[group]['fields'][3]=min(200,groups[group]['fields'][3]+1)
   for selector in range(min(cap,len(nodes)-rightnode-1)):
    new['next'][selector]=right['next'][selector][:]
    for v in right['next'][selector]:nodes[rightnode+selector+1][v]['previous'].append(ordinal)
   previous=leftnode-h[1]-1
   for v in left['previous']:(s['root'] if previous<0 else nodes[previous][v])['next'][span].append(ordinal)
   events.append([1,rightnode,ordinal,group,leftnode,ln])
def rounded(x):
 v=struct.unpack('<I',struct.pack('<f',x))[0];m=v&0x7fffffff
 if m>=0x4b000000:return x
 if m<0x3f800000:v=(v&0x80000000)|(0x3f800000 if m>=0x3f000000 else 0)
 else:
  shift=150-(m>>23);v=((v>>shift)+((v>>(shift-1))&1))<<shift
 return struct.unpack('<f',struct.pack('<I',v))[0]
def reject(group,span,c,descriptors,guides):
 if span==0 or len(group['ranges'])<2:return False
 a=descriptors[group['fields'][0]]
 if i(a,24)==0x2000000:return False
 if i(a,16)==-1:return c['threshold']<0x4000000
 ga=guides[i(a,16)];height=metric(ga,4)
 if group['fields'][1]!=65535:
  b=descriptors[group['fields'][1]];assert i(b,16)!=-1
  gb=guides[i(b,16)];height=f(max(metric(ga,0),metric(gb,0))-min(f(metric(ga,0)-metric(ga,4)),f(metric(gb,0)-metric(gb,4))))
 boxes=[]
 for stroke,start,end in group['ranges']:
  pts=c['strokes'][stroke][start:end];assert pts;boxes.append((min(p[0] for p in pts),max(p[0] for p in pts)))
 total=0
 for n in range(1,len(boxes)):
  left,right=boxes[:n],boxes[n:]
  gap=max(f(min(p[0] for p in right)-max(p[1] for p in left)),f(min(p[0] for p in left)-max(p[1] for p in right)))
  x=f(f(f(gap/height)-c['gap'])*f(-c['slope']))
  value=0 if x>4 else j['table'][int(f(f(f(x+4)*255)+.5))>>3] if x>=-4 else x
  cost=int(rounded(f(f(-value)*8192)));total=min(total+cost,0x4000000)
 return total>c['threshold']
def prune(s,counts,c,descriptors,guides,events):
 nodes=s['nodes'];cap=s['cap']
 def visit(node,boundary):
  for ordinal in range(len(nodes[node])-1,-1,-1):
   arc=nodes[node][ordinal];group=s['groups'][arc['header'][0]];ranges=group['ranges'];drop=None
   if boundary:
    if group['fields'][1]==65535:continue
    a,b=descriptors[group['fields'][0]],descriptors[group['fields'][1]]
    first=next((n for n,r in enumerate(ranges) if r[0]==i(a,36)),None)
    if first is not None and any(r[0]==i(a,40) for r in ranges[first:]):continue
    first=next((n for n,r in enumerate(ranges) if r[0]==i(b,36)),None);complete=first is not None;physical=i(b,36)
    if complete:
     for r in ranges[first:]:
      if r[0]!=physical or r[1]!=0 or physical==i(b,40) and r[2]!=len(c['strokes'][physical]):complete=False;break
      physical+=1
    drop=reject(group,arc['header'][1],c,descriptors,guides) if complete and physical-1>=i(b,40) else True
   else:drop=reject(group,arc['header'][1],c,descriptors,guides)
   if not drop:continue
   for nn,row in enumerate(nodes):
    for other in row:
     if nn-other['header'][1]-1==node:other['previous']=[p-(p>ordinal) for p in other['previous'] if p!=ordinal]
   for previous in range(-1,len(nodes)):
    selector=node-previous-1
    if not 0<=selector<cap:continue
    for other in [s['root']] if previous<0 else nodes[previous]:other['next'][selector]=[p-(p>ordinal) for p in other['next'][selector] if p!=ordinal]
   refs=group['fields'][3]
   if refs<200:assert refs>0;group['fields'][3]-=1
   del nodes[node][ordinal];events.append([2,node,ordinal,arc['header'][0],int(boundary),0])
 start=0
 for count in counts:
  for node in range(start,min(len(nodes),start+cap)):visit(node,True)
  visit(start+count-1,False);start+=count
 assert start==len(nodes)
totalgroups=totalremoved=actualgroups=0
for c in j['cases']+j['boundaryFixtures']+j['actual']:
 descriptors=list(map(base64.b64decode,c['descriptors']));guides=list(map(base64.b64decode,c['guides']));cap=c['locals'][0]['cap'];events=[]
 if 'pruneOnly' in c:state=copy.deepcopy(c['locals'][0]);counts=c['pruneOnly']
 else:
  state=dict(cap=cap,groups=[],contexts=[],nodes=[],root=dict(header=[0,0,0,4,0,0,65535,0,0],previous=[],next=[[] for _ in range(cap)]));previous=-1
  for local in c['locals']:
   old=len(state['nodes'])-1;state=append(state,local)
   if previous>=0:cross(state,old,previous-1,len(state['nodes'])-1,descriptors,guides,events)
   previous=len(local['nodes'])
  counts=[len(v['nodes']) for v in c['locals']]
 prune(state,counts,c,descriptors,guides,events)
 assert events==c['events'],('event stream',c['actual'],events,c['events'])
 assert state==c['output'],('full snapshot',c['actual'])
 created=sum(e[0]==0 for e in events);totalgroups+=created;totalremoved+=sum(e[0]==2 for e in events)
 if c['actual']:actualgroups+=created
assert len(j['cases'])==j['prepared']==96 and len(j['actual'])==j['actualCount']==7
assert totalgroups==j['created'] and totalremoved==j['removed'] and actualgroups==j['actualCreated'] and actualgroups>0
assert len(j['boundaryFixtures'])==8
print(f'PASS independent111 full cross/prune states,{totalgroups} allocations/{actualgroups} actual cross groups,{totalremoved} ordered boundary/final removals; all fields/ranges/history/adjacency/events match')
