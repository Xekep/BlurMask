"""Full-source B3 raw-profile/request/ordered-callback/40B oracle.
Only standard Python; shared pure numeric reference functions never transfer lite ownership.
All expected requests are reconstructed from raw full model plus independently audited B2.
"""
from pathlib import Path
import argparse,ast,base64,hashlib,json,math,struct
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--b2',type=Path,required=True);p.add_argument('--inputs',type=Path,required=True);p.add_argument('--capture',type=Path,required=True);p.add_argument('--output',type=Path,required=True);args=p.parse_args();root=args.repo;models=args.models
def definitions(path,names=None):
 t=ast.parse(path.read_text(encoding='utf-8-sig'));nodes=[n for n in t.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))and(names is None or isinstance(n,(ast.Import,ast.ImportFrom))or n.name in names)]
 g={'__file__':str(path)};exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),g);return g
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def bits(x):return struct.pack('<f',x)
INF=0x4000000;d=(models/'en-US/en_US-ak-cur.res').read_bytes();assert hashlib.sha256(d).hexdigest()=='355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927'
wire=definitions(root/'tools/LegacySource247Wire.Probe/audit_output.py');sampler,error=wire['sampler'](d,dict(Offset=16952,End=18340,Version=0x108),217);assert error is None
fields={x['destination']:base64.b64decode(x['raw'])for x in sampler['fields']};F=lambda name:struct.unpack('<f',fields[name])[0];I=lambda name:struct.unpack('<i',fields[name])[0]
assert(F('6C'),F('70'),F('74'),F('108'),F('54'),F('58'),F('5C'),F('60'))==(1.5,-.5,f(.4),3.,1.,.5,1.5,0.)and fields['10C']==b'\1'
at=sampler['categoryOffset']+4
def ids():
 global at
 n=d[at];at=(at+4)&~3;v=list(struct.unpack_from('<'+'i'*n,d,at));at+=n*4;return v
first=ids();fm=list(struct.unpack_from('<220b',d,at));at+=220;geometry=ids();sm=list(struct.unpack_from('<220b',d,at));at+=220;em=list(struct.unpack_from('<220b',d,at));at+=220
prototypes=[struct.unpack_from('<fffB',d,at+i*13)for i in range(len(geometry))]
composites=[];start=[];end=[]
for a,b,c in zip(fm,sm,em):
 maps=[]
 for x,y in((a,b),(a,c),(first.index(0),b),(first.index(0),c)):
  if x==-1 or y==-1:maps.append(-1);continue
  item=(first[x],y)
  if item not in composites:composites.append(item)
  maps.append(composites.index(item))
 start.append(maps[0]);end.append(maps[1])
assert len(composites)==42 and composites[start[217]]==(6,7)
header,labels=definitions(root/'tools/LegacyMainSingleLineIngress.Probe/audit_output.py',{'labels'})['labels'](d[8843:373957])
assert header==[181,178,65535,180,184,182,183,217]
bypass={labels[t]for t in('\U000f0001','\U000f0002','\U000f0003','\U000f0004','\U000f0008')if t in labels}|set(header[4:7]);bypass.discard(65535)
assert bypass=={179,182,183,184,189,190,211}
assert hashlib.sha256(d[8843:373957]).hexdigest()=='511b3f4e562cdd1a15661b7f0426255bc7d8675b76cd444e1f58c78adf2d380c'
table=[f(v)for v in json.loads((models/'ru-RU/legacy-separation-profile.json').read_bytes())['values']]
assert hashlib.sha256(struct.pack('<256f',*table)).hexdigest()=='b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf'
roundFloat=definitions(root/'tools/LegacyCursiveSeparation.Probe/reference_controls.py',{'native_round_float'})['native_round_float']
def logcost(x,nearest=True):
 x=f(x);v=0 if x>4 else table[int(f(x+4)*255+.5)>>3]if x>=-4 else x;v=f(-v*8192)
 return int(roundFloat(v))if nearest else int(f(v+(.5 if v>0 else -.5)))
margins=[v for r in range(6)for v in struct.unpack('<6f',fields[f'{0x78+r*24:X}.matrixRow'])]
def environment(row):
 nodes=row['Nodes'];points=row['Cut'];height=f(row['Height']);boxes={};whole={};future={};prefix={};join={};blank={};matrix={}
 for g,ranges in row['GroupRanges'].items():
  ps=[p for r in ranges for p in points[r['StrokeIndex']][r['StartPoint']:r['EndPointExclusive']]]
  boxes[g]=[min(f(p[0])for p in ps),min(f(p[1])for p in ps),max(f(p[0])for p in ps),max(f(p[1])for p in ps)];whole[g]=all(r['StartPoint']==0 for r in ranges)
 for n in reversed(range(len(nodes))):
  for a,arc in enumerate(nodes[n]):
   low=boxes[arc['Group']][0]
   if n!=arc['Span']and n+1<len(nodes):
    for b,nextArc in enumerate(nodes[n+1]):
     if nextArc['Span']==0 and a in nextArc['Predecessors']:low=min(low,future[n+1,b])
   future[n,a]=low
 for n,arcs in enumerate(nodes):
  for a,arc in enumerate(arcs):
   box=boxes[arc['Group']];parent=n-arc['Span']-1
   if parent<0:prefix[n,a]=box[2];join[n,a]=blank[n,a]=INF;matrix[n,a]=[INF]*36
   else:
    prior=max((prefix[parent,p]for p in arc['Predecessors']),default=box[2]);prefix[n,a]=max(box[2],prior);gap=f(f(future[n,a]-prior)/height)
    join[n,a]=min(logcost(f(f(gap-F('6C'))*-F('108'))),I('120'))
    raw=f(f(f(f(box[2]-prior)/height)-F('70'))*-F('108'));blank[n,a]=min(logcost(raw),INF)
    matrix[n,a]=[]
    for i,margin in enumerate(margins):
     cost=logcost(f(f(gap-margin)*F('108')),False);matrix[n,a].append(cost-1 if i//6==5 else cost if i%6==5 else min(cost,I('124')))
   state=next(s for s in row['States']if(s['Node'],s['Arc'])==(n,a));b=state['Bounds']
   assert boxes[arc['Group']]==[f(b[k])for k in('MinimumX','MinimumY','MaximumX','MaximumY')]
   assert(bits(state['Future']),bits(state['Prefix']),state['Join'],state['Blank'],state['Matrix'])==(bits(future[n,a]),bits(prefix[n,a]),join[n,a],blank[n,a],matrix[n,a]),('state',n,a)
 def correct(stage,n,a,s,c,kind,gn,garc,gs,gc,incoming,on,oarc):
  if kind==6:return incoming,True
  if stage==140:
   delta=0
   if s!=209 and gs!=209:
    cp=prototypes[composites[c][1]];pp=prototypes[composites[gc][1]];current=boxes[nodes[n][a]['Group']];previous=boxes[nodes[gn][garc]['Group']];inverse=f(1/height)
    #Native1ACFA0 ordinary140 unions SOURCE with original ENDING(kind0).
    if (on,oarc)!=(gn,garc):
     ending=boxes[nodes[on][oarc]['Group']];previous=[min(previous[0],ending[0]),min(previous[1],ending[1]),max(previous[2],ending[2]),max(previous[3],ending[3])]
    bottom=f(cp[0]-pp[0]);top=f(bottom-f(cp[1]-pp[1]));residual=f(abs(f(f(f(current[1]-previous[1])*inverse)-top))+abs(f(f(f(current[3]-previous[3])*inverse)-bottom)))
    value=f(min(f(f(residual*F('54'))-F('58')),F('5C'))*8192);delta=int(f(value+(.5 if value>0 else -.5)))
   return incoming+delta,incoming+delta<INF
  if kind==5:
   possible=whole[nodes[n][a]['Group']]and(n+1==len(nodes)or whole[nodes[n+1][0]['Group']]);value=incoming+blank[n,a]if possible else INF;return value,possible and value<INF
  xor=(s==209)!=(gs==209)
  if kind==0 and xor or kind==2 and not whole[nodes[n][a]['Group']]:return INF,False
  if kind==2 and xor:return incoming,True
  delta=join[n,a]if kind==0 else matrix[n,a][composites[c][0]+composites[gc][0]*6]
  current=boxes[nodes[n][a]['Group']];previous=boxes[nodes[gn][garc]['Group']];displacement=f(max(f(current[0]-previous[0]),f(current[2]-previous[2]))/height)
  translate=f(f(displacement-F('74'))*F('108'))if displacement<=F('74')else 0
  value=incoming+delta-int(translate)*8192;return value,value<INF
 return correct
def replay(row,requests,trace):
 correct=environment(row);nodes=row['Nodes'];rows=[[]for _ in nodes];raw=[bytearray(200)for _ in nodes]
 for b in raw:
  for i in range(5):struct.pack_into('<i',b,i*40,INF)
 ti=0;stream=hashlib.sha256();witness={x['Index']:x['Data']for x in trace['Witnesses']}
 def call(stage,n,a,s,c,span,kind,on,oa,o,gn,ga,g,skipped,incoming):
  nonlocal ti
  cost,ok=correct(stage,n,a,s,c,kind,gn,g[3],g[5],g[7],incoming,on,o[3])
  expected=[n,a,s,c,span,kind,on,oa,o[3],o[5],o[7],o[4],o[2],o[0],o[1],int(o[6]),gn,ga,g[3],g[5],g[7],g[4],g[2],g[0],g[1],int(g[6]),skipped,incoming,cost,int(ok),stage]
  stream.update(struct.pack('<31i',*expected))
  if ti in witness:assert witness[ti]==expected,('ordered witness',ti,witness[ti],expected)
  ti+=1;return cost,ok
 for n,proposals in enumerate(requests):
  for proposal in proposals:
   a,span,s,c,cost,likelihood=[proposal[k]for k in('ArcOrdinal','Span','Symbol','CompositeCategory','GroupCost','LikelihoodByte')]
   if cost>=INF:continue
   parent=n-span-1
   if parent<0:
    if s==211 or -1 not in nodes[n][a]['Predecessors']:continue
    result=[cost,cost,255,a,span,s,True,c,likelihood]
   else:
    best=INF;result=None
    for oa,o in enumerate(rows[parent]):
     if o[3]not in nodes[n][a]['Predecessors']:continue
     gn,ga,g,skipped=parent,oa,o,0
     while g[5]in(217,65535,184,182,183):
      previous=gn-g[4]-1
      if previous==-1:break
      ga=g[2];gn=previous;g=rows[gn][ga];skipped+=1
     def correction(kind):
      value,ok=call(118,n,a,s,c,span,kind,parent,oa,o,gn,ga,g,skipped,0)
      if not ok:return INF
      value,ok=call(140,n,a,s,c,span,kind,parent,oa,o,gn,ga,g,skipped,value);return value if ok else INF
     first=second=INF
     if s not in(184,182,183)and o[5]not in(184,182,183)and(s==209)==(o[5]==209):first=correction(5 if s==217 else 6 if s==211 else 0)
     if s not in(217,65535):second=correction(2)
     delta=min(first,second)
     if delta>=INF or o[0]+delta>=INF:continue
     value=o[0]+delta+cost
     if value<best:best=value;result=[value,o[1]+cost,oa,a,span,s,second<first,c,likelihood]
    if result is None:continue
   dest=len(rows[n])
   while dest and rows[n][dest-1][0]>result[0]:dest-=1
   if dest>=5:continue
   count=min(5,len(rows[n])+1)
   for j in range(count-1,dest,-1):raw[n][j*40:(j+1)*40]=raw[n][(j-1)*40:j*40]
   off=dest*40;struct.pack_into('<ii',raw[n],off,result[0],result[1]);raw[n][off+9]=result[2];raw[n][off+10]=0
   struct.pack_into('<HHH',raw[n],off+12,result[5],int(result[6])|(result[4]<<1)|(result[7]<<4),result[3]);struct.pack_into('<Q',raw[n],off+24,0);raw[n][off+32]&=224
   rows[n].insert(dest,result);del rows[n][5:]
  for j,r in enumerate(rows[n]):raw[n][j*40+8]=next(x['LikelihoodByte']for x in proposals if x['ArcOrdinal']==r[3]and x['Symbol']==r[5])
 assert ti==trace['CallbackCount']and stream.hexdigest()==trace['CallbackSha256'],(ti,trace['CallbackCount'],stream.hexdigest(),trace['CallbackSha256'])
 return rows,[dict(Count=len(rows[n]),Bytes=base64.b64encode(b).decode())for n,b in enumerate(raw)]

o=json.loads(args.capture.read_bytes());base=json.loads(args.b2.read_bytes());assert o['TargetsRead']is False and base['TargetsRead']is False
assert hashlib.sha256(args.b2.read_bytes()).hexdigest()in{'9ac5e75e68bd25073bc4aba1c89ab74c6d8692a43042652585d862e0d8294804','d0c75b15e6c6455dc664e41e1ccb584eea4ee12c914947092140b4c3794b3c27'}
inputs=json.loads(args.inputs.read_bytes());inputs=inputs if isinstance(inputs,list)else inputs['samples'];assert len(inputs)==len(o['Actual'])==len(base['Actual'])
weights=definitions(root/'tools/LegacyFullEnglishFirstController.Probe/audit_output.py',{'weights'})['weights'];weights.__globals__.update(f=f,math=math)
callbacks=proposals=records=children=0
for item,row,prior in zip(inputs,o['Actual'],base['Actual']):
 assert row['Name']==prior['Name']==item.get('Name',item.get('id'))
 assert row['Status']=='AcceptedPrimary'and prior['Status']=='AcceptedInitializer'
 assert row['NetworkCount']==prior['NetworkCount']and row['LogicalAdditional']==prior['LogicalAdditional']and row['LogicalCombined']==prior['LogicalCombined']
 assert len(row['Children'])==len(prior['Children'])
 for c,b in zip(row['Children'],prior['Children']):
  for key in('Descriptor','Height','Raw','Groups','Nodes'):assert c[key]==b[key],(row['Name'],'B2 changed',key)
  groups={g['Group']:g for g in b['Groups']};requests=[];fallbacks=0
  for n,node in enumerate(b['Nodes']):
   req=[]
   for a,arc in sorted(enumerate(node),key=lambda x:(x[1]['Span'],x[0])):
    g=groups[arc['Group']];es=g['Entries'];ws=weights([e[1]for e in es]);assert all(bits(e[2])==bits(w)for e,w in zip(es,ws));top=es[:20];space=any(e[0]==header[5]for e in top)
    def add(symbol,category,ordinal):
     assert composites[category][0]==6 if symbol==217 else 0<=composites[category][0]<=5
     cost=g['Fallback']if ordinal==65535 else es[ordinal][1]
     if cost<INF and symbol not in bypass:cost+=g['Geometry']
     w=0. if ordinal==65535 else ws[ordinal];likelihood=max(1,min(255,int(f(w*255))))
     req.append(dict(ArcOrdinal=a,Span=arc['Span'],Symbol=symbol,CompositeCategory=category,GroupCost=cost,LikelihoodByte=likelihood,Ordinal=ordinal))
    for ordinal,e in enumerate(top):
     s=e[0]
     if s in(header[4],header[5])or s==header[6]and space or n==arc['Span']and s==labels['\U000f0001']:continue
     for category in(start[s],end[s]):
      if category>=0:add(s,category,ordinal)
    if header[3]!=65535 and arc['Span']<n:add(217,start[217],65535);fallbacks+=1
   requests.append(req)
  assert c['Requests']==[[[r[k]for k in('ArcOrdinal','Span','Symbol','CompositeCategory','GroupCost','LikelihoodByte','Ordinal')]for r in rs]for rs in requests],('requests',row['Name'],c['Descriptor'])
  assert c['RequestCount']==sum(map(len,requests))and c['FallbackProposals']==fallbacks
  env=dict(Nodes=[[dict(Group=a['Group'],Span=a['Span'],Predecessors=a['Pred'])for a in node]for node in b['Nodes']],Cut=b['Raw'],Height=b['Height'],GroupRanges={g['Group']:[dict(StrokeIndex=s,StartPoint=a,EndPointExclusive=z)for s,a,z in g['Stored']]for g in b['Groups']},States=[])
  for n,node in enumerate(b['Nodes']):
   for a,x in enumerate(node):env['States'].append(dict(Node=n,Arc=a,Bounds=dict(zip(('MinimumX','MinimumY','MaximumX','MaximumY'),x['Bounds'])),Future=x['Future'],Prefix=x['Prefix'],Join=x['Join'],Blank=x['Blank'],Matrix=x['Matrix']))
  rows,raw=replay(env,requests,c);assert raw==c['Primary'],('40B rows',row['Name'],c['Descriptor'])
  callbacks+=c['CallbackCount'];proposals+=c['RequestCount'];records+=sum(r['Count']for r in raw);children+=1
assert o['Accepted']==len(inputs)and o['Refused']==0 and o['Guards']>=25 and o['Cancellations']>=6
report=dict(status='PASS',actual=len(inputs),accepted=o['Accepted'],refused=0,children=children,requests=proposals,orderedCallbacks=callbacks,primaryRecords=records,guards=o['Guards'],cancellations=o['Cancellations'],captureSha256=hashlib.sha256(args.capture.read_bytes()).hexdigest(),scope='Raw full source profile and B2 exact goldens; every ordered request/callback and40B row independently reconstructed; primary only, no WORD',vendorExecuted=False,targetsRead=False);args.output.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
