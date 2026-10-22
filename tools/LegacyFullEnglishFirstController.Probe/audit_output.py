from pathlib import Path
import argparse,ast,base64,struct,hashlib,json,math
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--capture',type=Path,required=True);p.add_argument('--a-capture',type=Path,required=True);p.add_argument('--b1-capture',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
def definitions(path,names=None):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));nodes=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))and(names is None or isinstance(n,(ast.Import,ast.ImportFrom))or n.name in names)];g={'__file__':str(path)};exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),g);return g
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
bits=lambda x:struct.pack('<f',x)
INF=0x4000000;d=(a.models/'en-US/en_US-ak-cur.res').read_bytes();assert hashlib.sha256(d).hexdigest()=='355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927'
header,labels=definitions(a.repo/'tools/LegacyMainSingleLineIngress.Probe/audit_output.py',{'labels'})['labels'](d[8843:373957]);assert header==[181,178,65535,180,184,182,183,217]
sampler,error=definitions(a.repo/'tools/LegacySource247Wire.Probe/audit_output.py')['sampler'](d,dict(Offset=16952,End=18340,Version=0x108),217);assert error is None
fields={x['destination']:base64.b64decode(x['raw'])for x in sampler['fields']};F=lambda n:struct.unpack('<f',fields[n])[0];I=lambda n:struct.unpack('<i',fields[n])[0]
assert(F('54'),F('58'),F('5C'),F('6C'),F('70'),F('74'),F('108'))==(1.,.5,1.5,1.5,-.5,f(.4),3.)and fields['10C']==b'\1'and sampler['categoryCompositeCount']==42
margins=[v for r in range(6)for v in struct.unpack('<6f',fields[f'{0x78+r*24:X}.matrixRow'])]
table=[f(v)for v in json.loads((a.models/'ru-RU/legacy-separation-profile.json').read_bytes())['values']];assert hashlib.sha256(struct.pack('<256f',*table)).hexdigest()=='b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf'
roundFloat=definitions(a.repo/'tools/LegacyCursiveSeparation.Probe/reference_controls.py',{'native_round_float'})['native_round_float']
def logcost(x,nearest=True):
 x=f(x);v=0 if x>4 else table[int(f(x+4)*255+.5)>>3]if x>=-4 else x;v=f(-v*8192)
 return int(roundFloat(v))if nearest else int(f(v+(.5 if v>0 else -.5)))
def geometry(points,ranges,height):
 chunks=[points[r[0]][r[1]:r[2]]for r in ranges];total=0
 for b in range(1,len(chunks)):
  left=[f(p[0])for s in chunks[:b]for p in s];right=[f(p[0])for s in chunks[b:]for p in s];distance=max(f(min(right)-max(left)),f(min(left)-max(right)))
  cost=logcost(f(f(f(distance/height)-F('68'))*-F('108')),len(chunks)<9)
  total=total+cost if len(chunks)>=9 or f(f(total)+f(cost))<INF else INF
 return min(total,I('118'))
def weights(costs):
 if len(costs)==1:return[1.]
 values=[f(math.exp(f(-f(c)/8192)))for c in costs[:20]];total=0.
 for x in values:total=f(total+x)
 return[f(x/total)for x in values]+[0.]*max(0,len(costs)-20)
bypass={labels[t]for t in('\U000f0001','\U000f0002','\U000f0003','\U000f0004','\U000f0008')if t in labels}|set(header[4:7]);bypass.discard(65535)
assert bypass=={179,182,183,184,189,190,211}
o=json.loads(a.capture.read_bytes());old=json.loads(a.a_capture.read_bytes());b1=json.loads(a.b1_capture.read_bytes());assert not o['TargetsRead']and o['Guards']==26 and o['Cancellations']==6
pe=o['PreparedEntries'];by={x[0]:i for i,x in enumerate(pe)}
for s,ordinal,geo,top,primitive,cost in o['PreparedQueries']:
 if primitive:expected=40000
 elif ordinal==65535 and s==217:expected=o['PreparedFallback']
 else:
  i=by.get(s,len(pe))if ordinal==65535 else ordinal;expected=INF if i>=len(pe)or top and i>=20 else pe[i][1]
 if expected<INF and s not in bypass:expected+=o['PreparedGeometry']
 assert cost==expected
assert len(o['Actual'])==len(old['Actual'])==len(b1['Actual']);groups=arcs=positive=weights_checked=states=links=distinct=0;accepted=refused=0
for row,arow,brow in zip(o['Actual'],old['Actual'],b1['Actual']):
 assert row['Name']==arow['Name']==brow['Name']
 if row['Status']=='Refused':refused+=1;continue
 accepted+=1;assert arow['Status']=='AcceptedAcoustic'and brow['Status']=='AcceptedConfidence'and row['NetworkCount']==arow['NetworkCount']==brow['NetworkCount']
 assert len(row['Children'])==len(arow['Children'])==len(brow['Children']);G=A=N=E=P=0
 for child,ac,bc in zip(row['Children'],arow['Children'],brow['Children']):
  assert child['Descriptor']==ac['Descriptor']==bc['Descriptor']and bits(child['Height'])==bits(ac['Height'])and child['Raw']==ac['CutInk'];points=child['Raw'];height=f(child['Height']);nodes=child['Nodes'];N+=len(nodes);P+=sum(map(len,points));bygroup={};boxes={}
  assert len(child['Groups'])==len(ac['Groups'])==len(bc['Groups'])
  for g,ag,bg in zip(child['Groups'],ac['Groups'],bc['Groups']):
   assert g['Group']==ag['Group']==bg['Group']and g['Costs']==ag['Final']and g['Fallback']==ag['Fallback']and g['Stored']==ag['Stored']and g['Confidence']==bg['Values'];bygroup[g['Group']]=g
   expected=geometry(points,g['Stored'],height);assert g['Geometry']==expected,(row['Name'],g['Group'],g['Geometry'],expected);positive+=expected>0
   es=g['Entries'];assert[[e[0],e[1]]for e in es]==ag['Candidates'];ws=weights([e[1]for e in es]);assert all(bits(e[2])==bits(w)and e[3]==max(1,min(255,int(f(w*255))))for e,w in zip(es,ws));weights_checked+=len(ws);distinct+=sum(bits(e[2])!=bits(c)for e,c in zip(es,g['Confidence']))
   ps=[p for s,start,end in g['Stored']for p in points[s][start:end]];boxes[g['Group']]=[min(f(p[0])for p in ps),min(f(p[1])for p in ps),max(f(p[0])for p in ps),max(f(p[1])for p in ps)];groups+=1;G+=1
  future={};prefix={}
  for n in reversed(range(len(nodes))):
   for ai,arc in enumerate(nodes[n]):
    assert 0<=arc['Span']<=min(7,n);expectedlinks=[]
    for t in range(n+1,min(len(nodes),n+9)):
     for j,target in enumerate(nodes[t]):
      if t-target['Span']-1==n and ai in target['Pred']:expectedlinks.append([t-n,j])
    assert arc['Successors']==expectedlinks;E+=len(expectedlinks);links+=len(expectedlinks)
    low=boxes[arc['Group']][0]
    if n!=arc['Span']and n+1<len(nodes):
     for j,target in enumerate(nodes[n+1]):
      if target['Span']==0 and ai in target['Pred']:low=min(low,future[n+1,j])
    future[n,ai]=low
  for n in range(len(nodes)):
   for ai,arc in enumerate(nodes[n]):
    box=boxes[arc['Group']];parent=n-arc['Span']-1;assert [f(x)for x in arc['Bounds']]==box
    if parent<0:
     assert all(p==-1 for p in arc['Pred']);prefix[n,ai]=box[2];join=blank=INF;matrix=[INF]*36;E+=1
    else:
     assert all(0<=p<len(nodes[parent])for p in arc['Pred']);prior=max((prefix[parent,p]for p in arc['Pred']),default=box[2]);prefix[n,ai]=max(box[2],prior);gap=f(f(future[n,ai]-prior)/height)
     join=min(logcost(f(f(gap-F('6C'))*-F('108'))),I('120'));blank=min(logcost(f(f(f(f(box[2]-prior)/height)-F('70'))*-F('108'))),INF);matrix=[]
     for i,m in enumerate(margins):
      cost=logcost(f(f(gap-m)*F('108')),False);matrix.append(cost-1 if i//6==5 else cost if i%6==5 else min(cost,I('124')))
    assert(bits(arc['Future']),bits(arc['Prefix']),arc['Join'],arc['Blank'],arc['Matrix'])==(bits(future[n,ai]),bits(prefix[n,ai]),join,blank,matrix),(row['Name'],n,ai)
    arcs+=1;A+=1;states+=36
 assert G<=4096 and A<=4096 and N<=4096 and E<=1048576 and P<=8192
 assert row['LogicalAdditional']==524288+2048*G+1024*A+256*N+16*E+64*P and row['LogicalCombined']==64*1024*1024+row['LogicalAdditional']
assert(accepted,refused,groups,arcs)==(o['Accepted'],o['Refused'],o['Groups'],o['Arcs'])
report=dict(status='PASS',scope='Independent actual full raw sampler/geometry/ranked initializer; no request/scheduler/word authority',actual=len(o['Actual']),accepted=accepted,refused=refused,groups=groups,arcs=arcs,positiveGeometry=positive,rankedWeights=weights_checked,marginStates=states,ownedSuccessorLinks=links,weightVsConfidence=distinct,preparedQueries=len(o['PreparedQueries']),guards=o['Guards'],cancellations=o['Cancellations'],captureSha256=hashlib.sha256(a.capture.read_bytes()).hexdigest(),vendorExecuted=False,targetsRead=False);a.output.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
