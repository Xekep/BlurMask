"""Independent raw108 profile/geometry, exact ordered callbacks and40B rows.
Only Python standard library. Prepared numeric controls are not NN proposals.
"""
from pathlib import Path
import ast,base64,hashlib,json,math,struct,sys
if len(sys.argv)!=6:raise SystemExit('models audited-candidates.json audited-controller.json controls.json primary.json')
root=Path(__file__).resolve().parents[2];models=Path(sys.argv[1]);source=json.loads(Path(sys.argv[2]).read_bytes());costs=json.loads(Path(sys.argv[3]).read_bytes());controls=json.loads(Path(sys.argv[4]).read_bytes());out=json.loads(Path(sys.argv[5]).read_bytes())
def definitions(path,names=None):
 t=ast.parse(path.read_text(encoding='utf-8-sig'));nodes=[n for n in t.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))and(names is None or isinstance(n,(ast.Import,ast.ImportFrom))or n.name in names)]
 g={'__file__':str(path)};exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),g);return g
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def bits(x):return struct.pack('<f',x)
INF=0x4000000;d=(models/out['Path']).read_bytes();manifest={x['path']:x['sha256']for x in json.loads((models/'manifest.json').read_bytes())['resources']}
assert out['Path']==source['Path']==costs['Path']=='en-US/en_US-ak-cur.lite.res'and hashlib.sha256(d).hexdigest()==manifest[out['Path']]
assert out['Fingerprint'].lower()==source['Fingerprint'].lower()==costs['Fingerprint'].lower()==hashlib.sha256(d[8843:182187]).hexdigest()
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
header,labels=definitions(root/'tools/LegacyMainSingleLineIngress.Probe/audit_output.py',{'labels'})['labels'](d[8843:182187])
assert header==[181,178,65535,180,184,182,183,217]
bypass={labels[t]for t in('\U000f0001','\U000f0002','\U000f0003','\U000f0004','\U000f0008')if t in labels}|set(header[4:7]);bypass.discard(65535)
assert bypass=={179,182,183,184,189,190,211}
def classcost(group,symbol,ordinal):
 #Native S16 category request is already resolved to a prototype before this.
 cost=group['Fallback']if ordinal==65535 and symbol==217 else group['Entries'][ordinal]['Cost']
 return cost if cost>=INF or symbol in bypass else cost+group['GeometryCost']
table=[f(v)for v in json.loads((models/'ru-RU/legacy-separation-profile.json').read_bytes())['values']]
assert hashlib.sha256(struct.pack('<256f',*table)).hexdigest()=='b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf'
roundFloat=definitions(root/'tools/LegacyCursiveSeparation.Probe/reference_controls.py',{'native_round_float'})['native_round_float']
def logcost(x,nearest=True):
 x=f(x);v=0 if x>4 else table[int(f(x+4)*255+.5)>>3]if x>=-4 else x;v=f(-v*8192)
 return int(roundFloat(v))if nearest else int(f(v+(.5 if v>0 else -.5)))
margins=[v for r in range(6)for v in struct.unpack('<6f',fields[f'{0x78+r*24:X}.matrixRow'])]
def environment(row):
 nodes=row['Nodes'];points=row['Cut'];height=f(row['Height']);boxes={};whole={};future={};prefix={};join={};blank={};matrix={}
 for g,ranges in enumerate(row['GroupRanges']):
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
 ti=0
 def call(stage,n,a,s,c,span,kind,on,oa,o,gn,ga,g,skipped,incoming):
  nonlocal ti
  cost,ok=correct(stage,n,a,s,c,kind,gn,g[3],g[5],g[7],incoming,on,o[3])
  expected=[n,a,s,c,span,kind,on,oa,o[3],o[5],o[7],o[4],o[2],o[0],o[1],int(o[6]),gn,ga,g[3],g[5],g[7],g[4],g[2],g[0],g[1],int(g[6]),skipped,incoming,cost,int(ok),stage]
  assert ti<len(trace)and trace[ti]['Data']==expected,('ordered callback',ti,trace[ti]if ti<len(trace)else None,expected);ti+=1;return cost,ok
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
 assert ti==len(trace)
 return rows,[dict(Count=len(rows[n]),Bytes=base64.b64encode(b).decode())for n,b in enumerate(raw)]
totalCalls=totalRows=blankRows=skippedCalls=proposals=0
assert len(out['Actual'])==len(costs['Rows'])==len(source['Cases'])==124
for row,reference,src in zip(out['Actual'],costs['Rows'],source['Cases']):
 assert row['Raw']==reference['Raw']==src['Raw']and row['Cut']==reference['Cut']and bits(row['Height'])==bits(reference['Height'])
 assert len(row['Nodes'])==len(src['Nodes'])==row['NodesCount']and row['CachedGroups']==len(reference['Groups'])
 assert row['GroupRanges']==[[dict(StrokeIndex=s,StartPoint=a,EndPointExclusive=b)for s,a,b in rs]for rs in src['AllocatedGroups']]
 expected=[];expectedClass=[];groups={g['Id']:g for g in reference['Groups']}
 for n,node in enumerate(row['Nodes']):
  assert[(a['Group'],a['Span'],a['Predecessors'])for a in node]==[(a['GroupId'],a['Span'],a['Previous'])for a in src['Nodes'][n]]
  requests=[];classRequests=[]
  for a,arc in sorted(enumerate(node),key=lambda x:(x[1]['Span'],x[0])):
   group=groups[arc['Group']];top=group['Entries'][:20];containsSpace=any(x['Symbol']==182 for x in top)
   for ordinal,entry in enumerate(top):
    s=entry['Symbol']
    if s in(184,182)or s==183 and containsSpace or n==arc['Span']and s==211:continue
    for category in(start[s],end[s]):
     if category<0:continue
     assert composites[category][0]<=5
     cost=classcost(group,s,ordinal);assert cost==group['Top'][s]
     assert 0<=composites[category][1]<len(prototypes)
     requests.append(dict(ArcOrdinal=a,Span=arc['Span'],Symbol=s,CompositeCategory=category,GroupCost=cost,LikelihoodByte=base64.b64decode(group['Bytes'])[s]))
     classRequests.append([a,arc['Span'],s,ordinal,category,composites[category][1],cost])
   if arc['Span']<n:
    cost=classcost(group,217,65535);assert cost==group['Top'][217]
    requests.append(dict(ArcOrdinal=a,Span=arc['Span'],Symbol=217,CompositeCategory=start[217],GroupCost=cost,LikelihoodByte=1))
    classRequests.append([a,arc['Span'],217,65535,start[217],composites[start[217]][1],cost])
  expected.append(requests);expectedClass.append(classRequests)
 assert expected==row['Requests']and sum(map(len,expected))==row['RequestCount']
 assert expectedClass==row['ClassRequests'],'actual shortlist ordinal/prototype/FFFF blank request provenance'
 rows,storage=replay(row,expected,row['Calls']);assert storage==row['Primary'],'full primary40B storage'
 totalCalls+=len(row['Calls']);totalRows+=sum(map(len,rows));blankRows+=sum(r[5]==217 for rs in rows for r in rs);skippedCalls+=sum(c['Data'][26]>0 for c in row['Calls']);proposals+=sum(map(len,expected))
preparedSkipped=preparedRoots=0;assert len(out['Prepared'])==len(controls)==512
for row,control in zip(out['Prepared'],controls):
 for n,requests in enumerate(row['Requests']):
  assert len(requests)==1;expected=182 if n==0 and control['rootHeader']else 217 if n==1 and control['blank1']or n==2 and control['blank2']else 10
  assert requests[0]['Symbol']==expected and requests[0]['GroupCost']==control['costs'][n]
 rows,storage=replay(out['PreparedOwner'],row['Requests'],row['Calls']);assert storage==row['Primary']
 preparedSkipped+=sum(c['Data'][26]>0 for c in row['Calls']);preparedRoots+=sum(c['Data'][16]==0 and c['Data'][19]==182 for c in row['Calls'])
assert(totalCalls,totalRows,blankRows,skippedCalls,preparedSkipped,preparedRoots,out['Guards'])==(out['ActualCallbacks'],out['PrimaryRows'],out['ActualBlankRows'],out['ActualSkipped'],out['PreparedSkipped'],out['PreparedRootStops'],714)
assert(totalCalls,totalRows,blankRows,skippedCalls,preparedSkipped,preparedRoots)==(534740,3850,1,46,1280,256)
print(f'PASS actual124/{proposals}proposals/{totalRows}full40B records/{totalCalls}ORDERED geometrycallbacks/{blankRows}realblank/{skippedCalls}realancestor; prepared512/{preparedSkipped}skips/{preparedRoots}retainedroots;714guards. NoB0/B8/terminal/text.')
