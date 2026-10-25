"""Strict frozen SOURCE14 DATA audit. Never runs a source stage or model."""
from pathlib import Path
import argparse,copy,hashlib,json,importlib.util
D=Path(__file__).resolve().parent
SUMMARY_SHA='064cb3c99e3c1c493fefc59d77a8e7c9bcbf2a90c77bffe1b59e9f4090009b2c'
CAPTURE_SHA='89fdc6a8822cad6bfc45ca9f331c8ec7e5eb556d825a89b0b1c78261348d9d06'
FIXTURE_SHA='ce3505ffeb0014a62eacde7c2d4841a23dda5f704fb127820fac4c937134eacb'
RESOURCE_SHA='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
SOURCES={'LegacyMathAkFeatureProfile.cs':'3b74b423aba2e4269418ac87e293373e4057426650a5c520a7ad9295e2b21592','LegacyMathAkFreshCommonWorkspace.cs':'8fb6e2c23c229d7c45e02f79c1fb7e885a99accac03dfeb2c25d866a424b8b2b','LegacyMathAkSource13A2Segments.cs':'c98562ad2c165888343e597e2c2966440787de77ce82eacc10967fbe6fec1ed6'}
def H(b):return hashlib.sha256(b).hexdigest()
def pairs(xs):
 r={}
 for k,v in xs:
  assert k not in r,('duplicate-key',k);r[k]=v
 return r
def load(p):return json.loads(p.read_bytes().decode('utf-8'),object_pairs_hook=pairs,parse_constant=lambda x:(_ for _ in()).throw(AssertionError(x)))
def exact(a,b,path='/'):
 assert type(a)is type(b),(path,'type')
 if isinstance(a,dict):
  assert a.keys()==b.keys(),(path,'keys')
  for k in a:exact(a[k],b[k],path+k+'/')
 elif isinstance(a,list):
  assert len(a)==len(b),(path,'length')
  for i,(x,y)in enumerate(zip(a,b,strict=True)):exact(x,y,path+str(i)+'/')
 else:assert a==b,(path,a,b)

# Reviewed immutable FIRST retention successor. Historical SOURCE14 was NOT rerun.
# Root profile DATA proves raw OLD tail unchanged; exact reversal below proves all
# prior profile operators unchanged. AL is reversed separately; Header partial
# declaration restores its exact old pin. Segments remain byte-exact.
PROFILE_SUCCESSOR_SHA='68675aead36585873cbb65035f1133802d9d0801fd82c91fe8c44170b4105e68'
FIRST_OWNER_SHA='558b0555e122529afbe3ad050db03dc02814edb0f6bc4b86229c2502b1886638'
PROFILE_RESTORE=[['public sealed partial class LegacyMathAkFeatureProfile\n', 'public sealed class LegacyMathAkFeatureProfile\n'], ['    private LegacyMathAkFeatureProfile(short[][] minimum, short[][] maximum, ProgramDescriptor program, LegacyMathAkShapePrototypes shapes, LegacyMathAkOriginalSmoothingPolicy smoothing, LegacyMathAkSource13A2Policy sampler, ushort freshCommonHeapCapacity, First276Program first276)\n    { this.minimum = minimum; this.maximum = maximum; this.program = program; ShapePrototypes = shapes; OriginalSmoothing = smoothing; Source13A2Policy = sampler; FreshCommonHeapCapacity = freshCommonHeapCapacity; this.first276 = first276; }\n', '    private LegacyMathAkFeatureProfile(short[][] minimum, short[][] maximum, ProgramDescriptor program, LegacyMathAkShapePrototypes shapes, LegacyMathAkOriginalSmoothingPolicy smoothing, LegacyMathAkSource13A2Policy sampler, ushort freshCommonHeapCapacity)\n    { this.minimum = minimum; this.maximum = maximum; this.program = program; ShapePrototypes = shapes; OriginalSmoothing = smoothing; Source13A2Policy = sampler; FreshCommonHeapCapacity = freshCommonHeapCapacity; }\n'], ['            //Separate FIRST owner, from THIS private authenticated capture.\n            //No caller-buffer reread, selected-tail calibration or model copy.\n            var first276 = First276Program.ReadAuthenticated(captured, token);\n', ''], ['            return new(minimum, maximum, program, shapes, smoothing, sampler, freshCommonHeapCapacity, first276);\n', '            return new(minimum, maximum, program, shapes, smoothing, sampler, freshCommonHeapCapacity);\n']]
def al_contract(root):
 path=root/'tools/LegacyMathAkFirstAlphabet.Probe/source_contract.py'
 spec=importlib.util.spec_from_file_location('workspace_al_contract',path)
 module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module);module.authenticate(root)
 return module

def authenticate_profile(root,raw):
 # Restore ONLY the reviewed AL retention hook, then use the previous exact
 # FIRST owner reversal down to historical3b74. No old-source pin is waived.
 raw=al_contract(root).restore_profile(raw)
 assert H(raw)==PROFILE_SUCCESSOR_SHA,'Profile successor bytes'
 assert H(al_contract(root).restore_feature_policy('LegacyMathAkFirst276Program.cs',(root/'src/InkRecognition/LegacyMathAkFirst276Program.cs').read_bytes()))==FIRST_OWNER_SHA
 text=raw.decode('utf-8')
 for old,new in PROFILE_RESTORE:
  assert old and text.count(old)==1,'exact reviewed Profile reversal'
  text=text.replace(old,new,1)
 assert H(text.encode('utf-8'))==SOURCES['LegacyMathAkFeatureProfile.cs'],'historical Profile restored exactly'

def reference(root):
 p=D/'baseline-counts.json';assert H(p.read_bytes())==SUMMARY_SHA;s=load(p)
 assert s['resourceSHA']==RESOURCE_SHA and s['fixtureSHA']==FIXTURE_SHA
 p=root/'tools/LegacyMathAkOperator13A0.Probe/fixtures.json';assert H(p.read_bytes())==FIXTURE_SHA
 names=[x['name']for x in load(p)['cases']];assert len(names)==len(set(names))==14
 for name,sha in SOURCES.items():
  raw=(root/'src/InkRecognition'/name).read_bytes().replace(b'\r\n',b'\n')
  if name=='LegacyMathAkFeatureProfile.cs':authenticate_profile(root,raw)
  elif name=='LegacyMathAkFreshCommonWorkspace.cs':
   assert H(al_contract(root).restore_header(raw))==sha,name
  else:assert H(raw)==sha,name
 rows=[];guards=['pre-cancel-exact-token','null-original-refused']
 assert len(s['rows'])==14
 for name,row in zip(names,s['rows'],strict=True):
  assert row['name']==name and type(row['markerCount'])is int and type(row['segmentCount'])is int
  assert row['markerCount']==row['segmentCount']>=0;m=row['markerCount']
  data=dict(Version=1,WorkWords=(m>>5)+1,HeapCapacity=10,Descriptor108Count=0,TemporaryHeapCount=0,ResultSlot16Count=0,Hypothesis24Count=0,FreeSlotCount=0,PositiveDescriptor8Count=row['descriptor8Count'],Positive1390Count=row['range1390Count'],SegmentCount=m,ChargedPayloadBytes=182,ChargedCopyBytes=296,ChargedStructuralWork=898,HasFirstRequest=False,HasCandidateAuthority=False)
  rows.append(dict(name=name,data=data));guards += [x+'-'+name for x in ['version','actual-W','parsed-tail-capacity','fresh-pool-only','ledger','no-FIRST']]
 assert sum(x['data']['Positive1390Count']for x in rows)==1
 return dict(resourceSHA=RESOURCE_SHA,fixtureSHA=FIXTURE_SHA,cases=rows,guards=guards,owningSourceCalls=14,modelEvaluations=0,processedGraphAuthority=False,matcherRequestAuthority=False)
def verify_originals(paths):
 expected=['82e35c3398885f3b07d6d834dff5331e1044df03b4673a1b937cf1bd051dd64c','7f0f020fa3ae6745946a41f68de211a94d6205b4284e2d04876ed0111b4e069b','13165d436aaa37a75a949bd8c8fa17a826e34d3cbaa5876a17a6283b69985df4']
 for p,sha in zip(paths,expected,strict=True):assert H(p.read_bytes())==sha
 a,b,c=map(load,paths);rows=[]
 for x,y,z in zip(a['cases'],b['cases'],c['cases'],strict=True):
  assert x['name']==y['name']==z['name']and y['data']['Sampler']==x['data']
  m=len(x['data']['MarkerFields'])//3;assert len(x['data']['MarkerFields'])%3==0 and y['data']['SegmentCount']==m
  rows.append(dict(name=x['name'],markerCount=m,segmentCount=y['data']['SegmentCount'],descriptor8Count=z['descriptorCount'],range1390Count=z['rangeResultCount']))
 exact(load(D/'baseline-counts.json')['rows'],rows)
def selftest(r):
 names=[]
 def no(name,mutate):
  q=copy.deepcopy(r);mutate(q)
  try:exact(r,q)
  except AssertionError:names.append(name)
  else:raise AssertionError('tamper-admitted '+name)
 no('drop-case',lambda x:x['cases'].pop());no('reorder',lambda x:x['cases'].reverse())
 no('bool-width',lambda x:x['cases'][0]['data'].__setitem__('WorkWords',True));no('null-data',lambda x:x['cases'][0].__setitem__('data',None))
 for k in r['cases'][0]['data']:
  no('field-'+k,lambda x,k=k:x['cases'][0]['data'].__setitem__(k,not x['cases'][0]['data'][k]if type(x['cases'][0]['data'][k])is bool else x['cases'][0]['data'][k]+1))
 positive=next(i for i,c in enumerate(r['cases'])if c['data']['Positive1390Count'])
 no('positive1390-erased',lambda x:x['cases'][positive]['data'].__setitem__('Positive1390Count',0));no('guard-missing',lambda x:x['guards'].pop());no('guard-reordered',lambda x:x['guards'].reverse())
 no('resource',lambda x:x.__setitem__('resourceSHA','0'*64));no('fixture',lambda x:x.__setitem__('fixtureSHA','0'*64));no('model-forward',lambda x:x.__setitem__('modelEvaluations',1));no('source-calls',lambda x:x.__setitem__('owningSourceCalls',13))
 try:json.loads('{"a":1,"a":2}',object_pairs_hook=pairs)
 except AssertionError:names.append('duplicate-json-key')
 else:raise AssertionError('duplicate-json-key')
 return names
def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('root',type=Path);p.add_argument('report',type=Path);p.add_argument('--capture',type=Path);p.add_argument('--selftest',action='store_true');p.add_argument('--verify-originals',nargs=3,type=Path);a=p.parse_args()
 assert not a.report.exists();r=reference(a.root.resolve());capture=a.capture or D/'historical-source14.json'
 if a.capture is None:assert H(capture.read_bytes())==CAPTURE_SHA
 exact(r,load(capture));negative=selftest(r)if a.selftest else []
 if a.verify_originals:verify_originals(a.verify_originals)
 report=dict(PASS=True,scope='Frozen SOURCE-owning14 scalar DATA only; no fresh source execution or FIRST proof',cases=14,guards=86,segments=sum(x['data']['SegmentCount']for x in r['cases']),positive1390Cases=1,captureSHA=H(capture.read_bytes()),summarySHA=SUMMARY_SHA,sourcePins=len(SOURCES),negativeControls=negative,freshSourceExecutions=0,modelEvaluations=0)
 a.report.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print('PASS frozenSOURCE14 DATA/86guards/',len(negative),'tampers;0freshSource')
if __name__=='__main__':main()
