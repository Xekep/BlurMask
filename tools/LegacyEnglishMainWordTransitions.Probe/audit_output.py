"""Independent actual EN108 ordered-cache/source/scalar oracle. No targets."""
import ast,base64,hashlib,json,math,struct,sys
from pathlib import Path
if len(sys.argv)!=4:raise ValueError('models capture output')
root=Path(__file__).resolve().parents[2];models=Path(sys.argv[1]);capture=Path(sys.argv[2]);out=Path(sys.argv[3])
def definitions(path):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));ns={};exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))],type_ignores=[]),str(path),'exec'),ns);return ns
num=definitions(root/'tools/LegacyMainWordTransition.Probe/reference_controls.py');f=num['f'];logcost=num['logcost'];bits=lambda value:struct.pack('<f',value)
wire=definitions(root/'tools/LegacySource247Wire.Probe/audit_output.py')
b=(models/'en-US/en_US-ak-cur.lite.res').read_bytes();assert hashlib.sha256(b).hexdigest()=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
source,_=wire['source'](b);child=next(x for x in source.children if x['destination']=='247.130');sampler,error=wire['sampler'](b,child,217);assert error is None
fields={x['destination']:base64.b64decode(x['raw'])for x in sampler['fields']}
F=lambda n:struct.unpack('<f',fields[n])[0];I=lambda n:struct.unpack('<i',fields[n])[0]
margins=[v for i in range(6)for v in struct.unpack('<6f',fields[f'{0x78+i*24:X}.matrixRow'])]
table=[f(v)for v in json.loads((models/'ru-RU/legacy-separation-profile.json').read_text())['values']]
assert hashlib.sha256(struct.pack('<256f',*table)).hexdigest()=='b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf'
data=json.loads(capture.read_bytes(),parse_int=lambda s:-0.0 if s=='-0' else int(s));assert data['TargetsRead']is False
# Independent raw category201 prototypes and composite encounter order.
at=sampler['categoryOffset'];version,count=struct.unpack_from('<HH',b,at);assert(version,count)==(0x201,217);at+=4
entries=count+3
def ids():
 global at
 n=b[at];at=(at+4)&~3;v=struct.unpack_from('<'+'i'*n,b,at);at+=4*n;return v
def mapping(n):
 global at
 v=struct.unpack_from('<'+'b'*entries,b,at);at+=entries;assert all(-1<=x<n for x in v);return v
first=ids();fm=mapping(len(first));geo=ids();sm=mapping(len(geo));em=mapping(len(geo));prototypes=[]
for i in geo:prototypes.append(struct.unpack_from('<fffB',b,at));at+=13
composites=[];zero=first.index(0)
for x,y,z in zip(fm,sm,em):
 for c,g in((x,y),(x,z),(zero,y),(zero,z)):
  if c!=-1 and g!=-1 and(first[c],g)not in composites:composites.append((first[c],g))
assert len(prototypes)==24 and len(composites)==42
expectedControls={};wrap=num['wrap'];requests=unionDiscriminators=initials=wrapDiscriminators=0
arcs=points=boxcount=matrixcount=livegroups=0
for case in data['Rows']:
 profile=case['Profile'];mapping={'PairScale':'54','PairOffset':'58','PairMaximum':'5C','GapOffset':'68','DefaultGap':'6C','BlankThreshold':'70','TranslationThreshold':'74','GapSlope':'108','SourceGapScale':'110','SourceMatrixScale':'114'}
 for key,name in mapping.items():assert bits(f(profile[key]))==bits(F(name)),(case['Name'],key)
 for key,name in {'CrossLineCost':'60','JoinMaximumCost':'120','MatrixMaximumCost':'124','BoundaryMaximum118':'118','BoundaryThreshold11C':'11C'}.items():assert profile[key]==I(name)
 assert [bits(f(v))for v in profile['Matrix']]==[bits(v)for v in margins] and profile['PairEnabled']is True
 policy=case['Policy'];assert (policy['AcousticSourceFlags'],policy['MaximumSourceArcs'],policy['AuxiliarySourceArcs'],policy['MaximumParentDistance54'],policy['FlagPrepassDistance58'],policy['Permission2309'],policy['Graph1Dc'],policy['KnowledgeF5'])==(0x21400,125,0,120,20,0,False,False)
 assert policy['Summary']=={'PackedSource22e8':3,'SubwordSourceIndex22ec':-1,'OverrideSourceIndex22f0':-1}
 assert policy['Refresh']=={'PackedSource22e8':3,'ConsecutiveBlank22f7':0,'KnowledgeOption22f8':0,'SourceFormat22f9':0,'MaximumWordLength68':120,'KnowledgeMarkerA0':0}
 c=policy['Configuration'];assert (c['HeaderCount2400'],c['PrimaryAlternativeCount2402'],c['SecondaryAlternativeCount2404'],c['PrimaryArcLimit230a'])==(2,2,1,125)
 descriptor=base64.b64decode(case['Descriptor']);guide=base64.b64decode(case['Guide']);assert len(descriptor)==88 and len(guide)==40 and struct.unpack_from('<i',descriptor,16)[0]>=0
 height=struct.unpack_from('<f',guide,4)[0];assert height>0;counts=case['Counts'];pts=case['Cuts'];assert sum(counts)==len(pts)
 offsets=[0]
 for count in counts:offsets.append(offsets[-1]+count)
 boxes={}
 for key,ranges in case['Groups'].items():
  selected=[pts[offsets[r['StrokeIndex']]+p]for r in ranges for p in range(r['StartPoint'],r['EndPointExclusive'])]
  boxes[int(key)]=[min(f(p['X'])for p in selected),min(f(p['Y'])for p in selected),max(f(p['X'])for p in selected),max(f(p['Y'])for p in selected)];boxcount+=1
 assert len(case['PreparedGroupBounds'])==len(boxes)
 for group,bounds in enumerate(case['PreparedGroupBounds']):assert [bits(f(bounds[k]))for k in ('MinimumX','MinimumY','MaximumX','MaximumY')]==[bits(v)for v in boxes[group]]
 graph=case['Graph'];future=[[None]*len(row)for row in graph];prefix=[[None]*len(row)for row in graph]
 for n in range(len(graph)-1,-1,-1):
  for a,arc in enumerate(graph[n]):
   low=boxes[arc['GroupId']][0]
   if n!=arc['Span'] and n+1<len(graph):
    for next in arc['Next']:
     if next['NodeOffset']==1:low=min(low,future[n+1][next['ArcIndex']])
   future[n][a]=low
 sourcegap=f(case['Gap']);gapDefault=f(sourcegap*F('110'))if sourcegap>=0 else F('6C');scale=1 if sourcegap<0 or margins[0]==0 else f(f(sourcegap*F('114'))/margins[0])
 for n,row in enumerate(graph):
  for a,arc in enumerate(row):
   box=boxes[arc['GroupId']];costs=[0x4000000]*3;m=[0x4000000]*36
   if n==arc['Span']:high=box[2];prefix[n][a]=high
   else:
    previous=n-arc['Span']-1;values=[prefix[previous][p]for p in arc['Previous']];high=max(values)if values else box[2];prefix[n][a]=max(box[2],high);gap=f(f(future[n][a]-high)/height)
    costs[0]=min(I('120'),logcost(f(f(gap-gapDefault)*f(-F('108'))),table,True))
    costs[1]=min(I('120'),logcost(f(f(gap-f(gapDefault*3))*f(-F('108'))),table,True))
    costs[2]=min(0x4000000,logcost(f(f(f(f(box[0]-high)/height)-F('70'))*f(-F('108'))),table,True))
    for i in range(36):
     cost=logcost(f(f(gap-f(scale*margins[i]))*F('108')),table,False);m[i]=cost-1 if i//6==5 else cost if i%6==5 else min(cost,I('124'))
   state=case['States'][n][a];actualbox=state['Bounds'];assert [bits(f(actualbox[k]))for k in ('MinimumX','MinimumY','MaximumX','MaximumY')]==[bits(v)for v in box]
   assert (bits(f(state['FutureMinimumX'])),bits(f(state['PrefixMaximumX'])),state['JoinCost'],state['TripleJoinCost'],state['BlankCost'],state['CategoryMatrix'])==(bits(future[n][a]),bits(prefix[n][a]),*costs,m),(case['Name'],n,a)
   arcs+=1;matrixcount+=36
 # PREPARED numeric requests, not actual dictionary selected rows/winners.
 def box(n,a):return boxes[graph[n][a]['GroupId']]
 for call in case['Requests']:
  assert call['Prepared']is True;before=bytearray(base64.b64decode(call['Before']));after=bytearray(base64.b64decode(call['After']));assert len(before)==len(after)==80
  i32=lambda at:struct.unpack_from('<i',before,at)[0];u16=lambda at:struct.unpack_from('<H',before,at)[0]
  n=i32(0);a=u16(12);prev=i32(4);pa=u16(14);current=box(n,a);incoming=call['Incoming'];kind=before[28]
  if call['Pair']:
   cg=struct.unpack_from('<b',before,18)[0];cp=prototypes[cg]
   if u16(38)==65535:
    inverse=f(1/height);baseline=struct.unpack_from('<f',guide,0)[0]
    residual=f(abs(f(cp[1]-f(f(current[3]-current[1])*inverse)))+abs(f(cp[0]-f(f(current[3]-baseline)*inverse))))
    value=f(min(f(residual*F('54')),F('5C'))*8192);delta=math.trunc(f(value-.5 if value<=0 else value+.5));initials+=1
   else:
    pg=struct.unpack_from('<b',before,19)[0];pp=prototypes[pg];prior=box(prev,pa);ending=i32(8);ea=u16(16);original=prior[:]
    if prev!=ending or pa!=ea:
     endingbox=box(ending,ea);prior=[min(prior[0],endingbox[0]),min(prior[1],endingbox[1]),max(prior[2],endingbox[2]),max(prior[3],endingbox[3])]
    def pair(previous):
     bottom=f(cp[0]-pp[0]);top=f(bottom-f(cp[1]-pp[1]));inverse=f(1/height)
     residual=f(abs(f(f(f(current[1]-previous[1])*inverse)-top))+abs(f(f(f(current[3]-previous[3])*inverse)-bottom)))
     value=f(min(f(f(residual*F('54'))-F('58')),F('5C'))*8192)
     return math.trunc(f(value-.5 if value<=0 else value+.5))
    delta=pair(prior);unionDiscriminators+=delta!=pair(original)
   cost=wrap(incoming+delta);wrapDiscriminators+=cost!=incoming+delta
  elif kind==2 and any(r['StartPoint']!=0 for r in case['Groups'][str(graph[n][a]['GroupId'])]):cost=0x4000000
  else:
   state=case['States'][n][a]
   correction=state['JoinCost'] if kind==0 else state['CategoryMatrix'][i32(20)+i32(24)*6]
   # Earlier full-cache independent equality lets this consume checked state.
   previous=box(prev,pa);movement=f(max(f(current[0]-previous[0]),f(current[2]-previous[2]))/height)
   value=f(f(movement-F('74'))*F('108'))if movement<=F('74')else 0
   cost=wrap(incoming-math.trunc(value)*8192+correction)
  accepted=cost<i32(52);accumulator=wrap(i32(48)+cost-incoming)if accepted else 0x4000000
  expected=before[:];struct.pack_into('<i',expected,48,accumulator)
  assert(after,cost,accepted)==(expected,call['Outgoing'],call['Accepted']),(case['Name'],call['Control'],n,a,cost,call['Outgoing'])
  expectedControls[case['Name'],call['Before'],call['Incoming'],call['Pair']]=(expected,cost)
  requests+=1
 livegroups+=len({arc['GroupId']for row in graph for arc in row})
 points+=len(pts)
for prefix in data['CanceledPrefixes']:
 assert prefix['Prepared']is True and prefix['Faulted']is True
 before=bytearray(base64.b64decode(prefix['Before']));after=bytearray(base64.b64decode(prefix['After']));expected,cost=expectedControls[prefix['Name'],prefix['Before'],prefix['Incoming'],True]
 stage=prefix['Stage']
 if stage in('transition:entered','transition:validated'):want=before;wantCost=prefix['Incoming']
 elif stage=='transition:cost':want=before;wantCost=cost
 elif stage=='transition:accumulated':want=expected;wantCost=cost
 else:raise AssertionError('Unknown cancellation stage')
 assert (after,prefix['Outgoing'])==(want,wantCost),(stage,prefix['Outgoing'],wantCost)
assert len(data['CanceledPrefixes'])==4 and data['Cancellations']==4
report=dict(status='PASS independent actual raw EN108 policy/source/ordered cache oracle',cases=len(data['Rows']),refused=len(data['Refused']),arcs=arcs,allocatedGroups=boxcount,liveGroups=livegroups,currentPoints=points,matrixChannels=matrixcount,requestTransitionsChecked=True,preparedRequests=requests,initialPairRequests=initials,sourceEndingUnionDiscriminators=unionDiscriminators,signedWrapDiscriminators=wrapDiscriminators,guards=data['Guards'],cancellations=data['Cancellations'],targetsRead=False)
out.write_text(json.dumps(report,indent=2)+'\n');print(report)
