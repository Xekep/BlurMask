"""Independent stable-interleaving DP and context-class/source-boundary proof. No NN/vendor."""
from pathlib import Path
import argparse,ast,json
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--capture',type=Path,required=True);p.add_argument('--b4',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
t=ast.parse((a.repo/'tools/LegacyFullEnglishFirstContext.Probe/audit_output.py').read_text(encoding='utf-8-sig'));g={};exec(compile(ast.Module(body=[n for n in t.body if isinstance(n,(ast.Import,ast.ImportFrom))or isinstance(n,ast.ClassDef)and n.name=='Cases'],type_ignores=[]),'reader','exec'),g)
prior=iter((row['Name'],c)for row in g['Cases'](a.b4)for c in row.get('Children',[]));rows=json.loads(a.capture.read_bytes());counts=dict(children=0,seeds=0,seedArcs=0,mergedArcs=0,endpointPairs=0,multiplePairs=0);maximumStates=0;projection=[]
for case in rows:
 name,old=next(prior);assert name==case['Name']and old['Descriptor']==case['Descriptor'];assert case['NNEvaluated']is False and case['RawLine']==-1 and case['DerivedAssignedLine']==0;assert old['Raw']==case['Cut']and old['Height']==case['Height'];seeds=case['Seeds'];nodes=case['SeedNodes'];N=len(seeds);assert len(nodes)==len(case['Nodes'])==len(old['Nodes'])==N
 ordinary=[i for i,s in enumerate(seeds)if not s['Deferred']];deferred=[i for i,s in enumerate(seeds)if s['Deferred']];D=len(deferred)
 #Every path preserves ordinary/deferred orders and allows a deferred item to
 #move at most two positions left. Sparse DP, no exponential path enumeration.
 layers=[{0}];edges=[]
 for pos in range(N):
  ee=[];reach=set()
  for j in layers[-1]:
   k=pos-j
   if 0<=k<len(ordinary):ee.append((j,j,ordinary[k]));reach.add(j)
   if j<D and deferred[j]-2<=pos<=deferred[j]:ee.append((j,j+1,deferred[j]));reach.add(j+1)
  edges.append(ee);layers.append(reach)
 assert D in layers[-1];back=[set()for _ in range(N+1)];back[-1]={D}
 for pos in reversed(range(N)):
  edges[pos]=[(j,k,z)for j,k,z in edges[pos]if k in back[pos+1]];back[pos]={j for j,k,z in edges[pos]}
 maximumStates=max(maximumStates,max(len(s)for s in back));assert maximumStates<=8
 for n,arcs in enumerate(nodes):
  assert len({ar['Group']for ar in arcs})==len(arcs)and {ar['Group']for ar in arcs}=={z for j,k,z in edges[n]}
  for ai,arc in enumerate(arcs):
   choices=[(j,k)for j,k,z in edges[n]if z==arc['Group']]
   pred={-1}if n==0 else {i for i,x in enumerate(nodes[n-1])if any(z==x['Group']and k==jj for j,k,z in edges[n-1]for jj,kk in choices)}
   succ=set()if n+1==N else {i for i,x in enumerate(nodes[n+1])if any(z==x['Group']and j==kk for j,k,z in edges[n+1]for jj,kk in choices)}
   assert set(arc['Pred'])==pred and set(arc['Succ'])==succ,('stable-path adjacency',case['Name'],n,ai)
  #Native first-equal encounter selection over independently validated
  #adjacency classes; IDs are not group/physical ordinals.
  left={};right={};nextid=1
  for ai,arc in enumerate(arcs):
   cls=tuple(sorted(arc['Pred']))
   if cls not in left:left[cls]=n*128 if ai==0 else n*128+2*nextid;nextid+=ai!=0
   assert arc['Start']==left[cls],('start class',n,ai)
  for arc in arcs:
   cls=tuple(sorted(arc['Succ']))
   if cls not in right:right[cls]=n*128+2*nextid+1;nextid+=1
   assert arc['End']==right[cls],('end class',n,arc['Group'])
 #Actual source physical overlap flag from all valid path prefixes/suffixes.
 before=[[set()for x in ar]for ar in nodes];after=[[set()for x in ar]for ar in nodes]
 for n,arcs in enumerate(nodes):
  for i,arc in enumerate(arcs):
   for pr in arc['Pred']:
    if pr>=0:before[n][i]|=before[n-1][pr]|{seeds[nodes[n-1][pr]['Group']]['Range'][0]}
 for n in reversed(range(N)):
  for i,arc in enumerate(nodes[n]):
   after[n][i].add(seeds[arc['Group']]['Range'][0])
   for su in arc['Succ']:after[n][i]|=after[n+1][su]
   seed=seeds[arc['Group']];flags=(0 if before[n][i]&after[n][i]else 4)|(int(seed['Deferred']and n-arc['Group']!=seed['Displacement']))
   assert arc['Flags']==flags,('seed flags',n,i)
 groups={gg['Id']:gg for gg in case['Groups']};oldgroups={gg['Group']:gg for gg in old['Groups']}
 for gg in groups.values():assert gg['Ranges']==oldgroups[gg['Id']]['Stored']
 for n,arcs in enumerate(case['Nodes']):
  assert len(arcs)==len(old['Nodes'][n])
  for arc,oo in zip(arcs,old['Nodes'][n]):
   assert(arc['Group'],arc['Span'],arc['Pred'])==(oo['Group'],oo['Span'],oo['Pred'])
   ss=groups[arc['Group']]['Seeds'];assert len(ss)==arc['Span']+1 and len(set(ss))==len(ss)<=7
   ids={s:i for i,s in enumerate(ss)};begin=n-arc['Span'];active={}
   for i,x in enumerate(nodes[begin]):
    if x['Group']in ids:active[i,1<<ids[x['Group']]]={x['Start']}
   for k in range(begin+1,n+1):
    following={}
    for i,x in enumerate(nodes[k]):
     if x['Group']not in ids:continue
     bit=1<<ids[x['Group']]
     for(pr,mask),starts in active.items():
      if not mask&bit and pr in x['Pred']:following.setdefault((i,mask|bit),set()).update(starts)
    active=following
   pairs={(start,nodes[n][i]['End'])for(i,mask),starts in active.items()if mask==(1<<len(ss))-1 for start in starts};assert(arc['Start'],arc['End'])in pairs,('merged actual source-context pair',case['Name'],n,arc['Group']);counts['endpointPairs']+=len(pairs);counts['multiplePairs']+=len(pairs)>1;counts['mergedArcs']+=1
 counts['children']+=1;counts['seeds']+=N;counts['seedArcs']+=sum(map(len,nodes));projection.append(dict(name=case['Name'],descriptor=case['Descriptor'],nodes=[[dict(group=ar['Group'],start=ar['Start'],end=ar['End'],flags=ar['Flags'])for ar in node]for node in case['Nodes']]))
assert next(prior,None)is None
r=dict(status='PASS',**counts,maximumDPStates=maximumStates,vendorExecuted=False,NNCalls=0,scope='Independent stable interleaving adjacency/first-equal classes and each retained candidate source-path endpoint pair; no candidate-completeness or nativeABI claim',projection=projection);a.output.write_text(json.dumps(r,separators=(',',':'))+'\n');print(json.dumps({k:v for k,v in r.items()if k!='projection'}))
