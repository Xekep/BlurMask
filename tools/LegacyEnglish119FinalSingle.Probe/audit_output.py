"""Independent actual EN main108 whole-cut/deferred path/geometry/source-key oracle.
No vendor execution, expected labels, NN or language inference.
"""
from pathlib import Path
import ast,base64,hashlib,itertools,json,math,struct,sys
sys.dont_write_bytecode=True
if len(sys.argv)!=4:raise SystemExit('models-directory capture.json prior-audited-v2.json')
repo=Path(__file__).resolve().parents[2];models=Path(sys.argv[1]);got=json.loads(Path(sys.argv[2]).read_bytes());prior=json.loads(Path(sys.argv[3]).read_bytes())
def defs(path):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));g={'__file__':str(path)};exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))],type_ignores=[]),str(path),'exec'),g);return g
src=repo/'tools/LegacyCursiveProducer.Probe/reference_controls.py';text=src.read_text();cut={'__file__':str(src)};exec(compile(text[:text.index('rng=random.Random')],str(src),'exec'),cut)
body=text[text.index(' for stroke,raw in enumerate(strokes):'):text.index('\nrng=random.Random')]
exec('def compute(strokes,height,baseline,confidence):\n tags=[1 if i==0 else 0 for s in strokes for i in range(len(s))]\n state=Stage(strokes,tags,0);whole=False;cap=None;previous=f(-3.4028234663852886e38)\n'+body,cut)
f=cut['f'];bits=cut['bits'];enclosure=defs(repo/'tools/LegacySourceEnclosure.Probe/reference_controls.py')['evaluate']
wire=defs(repo/'tools/LegacySource247Wire.Probe/audit_output.py');d=(models/'en-US/en_US-ak-cur.lite.res').read_bytes();assert hashlib.sha256(d).hexdigest()=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
sampler,refused=wire['sampler'](d,dict(Offset=182412,End=183800,Version=0x108),217);assert refused is None
fields={x['destination']:base64.b64decode(x['raw'])for x in sampler['fields']};assert fields['134.present']==fields['1D0.present']==b'\0'
assert d[183816:183818]==b'\x08\x01'and d[184106:184109]==b'\1\1\0'
gap=struct.unpack('<f',fields['68'])[0];slope=struct.unpack('<f',fields['114'])[0];threshold=struct.unpack('<i',fields['11C'])[0]
helpers=defs(repo/'tools/LegacyEnglish119Candidates.Probe/audit_output.py');helpers.update(f=f,table=json.loads((models/'ru-RU/legacy-separation-profile.json').read_text())['values']if False else [],gap=gap,slope=slope)
#Actual portable global table, checked independently from JSON schema.
profile=json.loads((models/'ru-RU/legacy-separation-profile.json').read_text());table=profile.get('values',profile.get('table'))
if table is None:
 print(profile.keys());raise AssertionError('table schema')
assert hashlib.sha256(struct.pack('<256f',*table)).hexdigest()=='b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf'
helpers['table']=list(map(f,table));compose=helpers['compose'];cost=helpers['cost']
#Independent original interval-distance rule; replace only the explicit actual
#profile fraction .6 with actual main raw .7 in the prepared generic reference.
native=repo/'tools/LegacyNativeSeed.Probe/reference_controls.py';tree=ast.parse(native.read_text());nodes=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))]
class Fraction(ast.NodeTransformer):
 def visit_Constant(self,n):return ast.copy_location(ast.Constant(struct.unpack_from('<f',d,184145)[0]),n)if isinstance(n.value,float)and n.value==.6 else n
ng={};exec(compile(ast.fix_missing_locations(Fraction().visit(ast.Module(body=nodes,type_ignores=[]))),str(native),'exec'),ng)
oldposts={x['Name']:x for x in prior['Post']};oldraw={x['Name']:x for x in prior['Actual']};count=records=deferred=arcs=removed=0
for case in got['Rows']:
 name=case['Name'];old=oldposts[name];orig=oldraw[name];last=old['Snapshots'][-1];count+=1
 assert len(last['Descriptors'])==len(last['Guides'])==1
 assert case['Physical']==last['Points']and case['Descriptor']==last['Descriptors'][0]and case['Guide']==last['Guides'][0]and case['SourceRows']==last['Rows']
 assert case['InputMetadata']==old['Metadata']
 guide=bytes.fromhex(case['Guide']);baseline,height=struct.unpack_from('<2f',guide);assert struct.unpack_from('<ii',guide,32)==(-1,-2147483648)
 assert case['FinalTrace']==['enter','169C30:single','161D20:no-pair','15D360:fresh','15D920:fresh-identity','prior130:cleared','complete']
 assert case['Trace']==['enter','108:cuts','21B:deferred-seeds','21B:local-ranges','108:local-pruning','complete']
 retained=[]
 for stroke in case['Raw']:
  retained.append([p for i,p in enumerate(stroke)if i==0 or any(abs(p[j])+abs(stroke[i-1][j])<abs(f(p[j]-stroke[i-1][j]))*2**23 for j in(0,1))])
 for i,v in enumerate(case['Probabilities']):assert abs(v-enclosure(retained,orig['Order'][i]))<3e-6
 expected=cut['compute']([[[f(v)for v in p]for p in st]for st in case['Physical']],height,baseline,list(map(f,case['Probabilities'])));enc=expected['expected'];assert len(enc)==len(case['CutRecords'])
 observed=[[bits(r[k])for k in ['X','Y','DX','DY','Length']]+[r[k]for k in['Tag','Physical','Original']]for r in case['CutRecords']];assert enc==observed,(name,'cut record arithmetic');records+=len(enc)
 assert case['Markers']==expected['markers'];counts=expected['counts'];assert counts==list(map(len,case['Cut']))
 points=[p for st in case['Cut']for p in st];assert [[bits(v)for v in p]for p in points]==[r[:2]for r in enc]
 flatInput=[p for st in case['InputMetadata']for p in st];flatOutput=[p for st in case['Metadata']for p in st]
 for record,meta in zip(enc,flatOutput):assert meta==flatInput[record[7]],(name,'inserted chronology')
 atoms=[]
 for i,(_,s,a)in enumerate(case['Markers']):atoms.append([s,a,case['Markers'][i+1][2]+1 if i+1<len(case['Markers'])and case['Markers'][i+1][1]==s else counts[s]])
 assert atoms==case['Atoms'];markers=[(s,a)for s,a,b in atoms];order=ng['expected']([[[f(v)for v in p]for p in st]for st in case['Cut']],markers,height,True)
 assert [[s['Atom'],s['Deferred'],s['Slot'],s['Displacement']]for s in case['Seeds']]==order,(name,'main seed ordering')
 seedRanges=[atoms[i]for i,_,_,_ in order];N=len(seedRanges);assert N==len(case['Nodes']);deferredIds=[i for i,s in enumerate(case['Seeds'])if s['Deferred']];deferred+=len(deferredIds);assert len(deferredIds)<=1
 paths=[list(range(N))]
 if deferredIds:
  dd=deferredIds[0];paths=[]
  for pos in range(max(0,dd-2),dd+1):
   path=list(range(N));path.pop(dd);path.insert(pos,dd);paths.append(path)
 #Native arc encounter order is independently defined by the allowed stable
 #placements, not reproduced with the library's linked insertion algorithm.
 variants=[]
 for node in range(N):
  groups=[node]if not deferredIds or node!=deferredIds[0]else[]
  if deferredIds:
   dd=deferredIds[0]
   if dd-2<=node<=dd:groups.append(dd)
   if dd-1<=node<=dd and node>0:groups.append(node-1)
  variants.append(groups)
 seedarcs=[]
 for node,gg in enumerate(variants):
  rr=[]
  for g in gg:
   pp=[p for p in paths if p[node]==g];pred=sorted({-1 if node==0 else variants[node-1].index(p[node-1])for p in pp});succ=sorted({variants[node+1].index(p[node+1])for p in pp if node+1<N});rr.append(dict(group=g,pred=pred,succ=succ,paths=pp))
  left={};right={};nextid=1
  for i,a in enumerate(rr):
   key=tuple(a['pred'])
   if key not in left:left[key]=node*128 if i==0 else node*128+2*nextid;nextid+=i!=0
   a['start']=left[key]
  for a in rr:
   key=tuple(a['succ'])
   if key not in right:right[key]=node*128+2*nextid+1;nextid+=1
   a['end']=right[key]
  seedarcs.append(rr)
 allocated={tuple(map(tuple,[rr]))for rr in seedRanges};expectedArcs=set();pathSegments={}
 for pathno,path in enumerate(paths):
  for end in range(N):
   for span in range(min(8,end+1)):
    start=end-span;rs=compose([seedRanges[g]for g in path[start:end+1]]);key=tuple(map(tuple,rs));allocated.add(key)
    if span and cost(case['Cut'],rs,height)>threshold:continue
    sa=seedarcs[start][variants[start].index(path[start])];ea=seedarcs[end][variants[end].index(path[end])]
    signature=(end,span,sa['start'],ea['end'],key);expectedArcs.add(signature);pathSegments.setdefault(signature,set()).add(pathno)
 assert {tuple(map(tuple,g))for g in case['Groups']}==allocated,(name,'allocation memberships')
 observedArcs=[]
 for node,aa in enumerate(case['Nodes']):
  for a in aa:
   sig=(node,a['Span'],a['Start'],a['End'],tuple(map(tuple,case['Groups'][a['Group']])));observedArcs.append(sig)
 assert set(observedArcs)==expectedArcs,(name,'retained deferred candidate memberships',set(observedArcs)^expectedArcs)
 arcs+=len(observedArcs);removed+=case['Removed']
 #Every reported adjacency is reciprocal and connects genuine compatible
 #retained path segments; root predecessors and removed targets stay exact.
 for node,aa in enumerate(case['Nodes']):
  for index,a in enumerate(aa):
   first=node-a['Span'];sig=(node,a['Span'],a['Start'],a['End'],tuple(map(tuple,case['Groups'][a['Group']])))
   prevnode=first-1
   if prevnode<0:assert a['Previous']==[-1]
   else:
    for pi in a['Previous']:
     pa=case['Nodes'][prevnode][pi];psig=(prevnode,pa['Span'],pa['Start'],pa['End'],tuple(map(tuple,case['Groups'][pa['Group']])));assert pathSegments[sig]&pathSegments[psig];assert[node-prevnode,index]in pa['Next']
   for off,ni in a['Next']:
    target=case['Nodes'][node+off][ni];assert target['Span']+1==off and index in target['Previous']
assert count==134 and len(got['MultilineRefused'])==7 and deferred==4 and arcs==4083 and removed>0
assert len(got['Failures'])==11
for fail in got['Failures']:assert fail['Trace'][-1]=='fault'and'complete'not in fail['Trace']
assert got['MaximumFinalInputCopyBytes']==1024+4096*8+256*160+4096*8<=128*1024

control=got['PreparedBudgetControls'];assert control['Scope']=='PREPARED; not genuine inference'
assert control['AdmittedSeedContextUnionWork']==20*4096*12<=1048576<control['RefusedSeedContextUnionWork']==20*4096*13
assert control['DeferredSeeds']==3 and control['DeferredCandidates']==148 and control['InitialDeferredEdges']==63 and control['DefaultPathExact'] is True
assert got['Guards']==1267
print('PASS actual134/7 raw main108/cuts',records,'records/4 deferred/4083 retained arcs; exact parent post265/source720/key lineage;11fault prefixes/bounded copies')
