"""Strict scalar source13A0 audit; never executes C#, inference, or a vendor DLL.
CLI: REPO MODELS FIXTURES CAPTURE [REPORT]. Requires NumPy for independent dense
small-system algebra; large horizontal controls use zero-curvature identity.
"""
from pathlib import Path
import copy,hashlib,importlib.util,json,math,struct,sys
R=Path(sys.argv[1]);D=Path(__file__).resolve().parent
for item in json.loads((D/'reference-dependencies.json').read_text(encoding='utf8')):
 data=(R/item['relative']).read_bytes().replace(b'\r\n',b'\n')
 assert hashlib.sha256(data).hexdigest()==item['lfSHA256'],item['relative']
s=importlib.util.spec_from_file_location('ref13a0',D/'reference_13a0.py');ref=importlib.util.module_from_spec(s);s.loader.exec_module(ref)
raw=(Path(sys.argv[2])/'math/math-ak.res').read_bytes()
assert hashlib.sha256(raw).hexdigest()=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
policy=struct.unpack_from('<8f',raw,7317)
fixture=json.loads(Path(sys.argv[3]).read_text(encoding='utf-8-sig'));specs=fixture['cases'];expected=[ref.execute(c,policy)for c in specs]
def integer(x,lo=-(1<<31),hi=(1<<32)-1):assert type(x)is int and lo<=x<=hi

def ints(x,depth,lo=-(1<<31),hi=(1<<31)-1):
 assert type(x)is list
 if depth==1:
  for v in x:integer(v,lo,hi)
 else:
  for v in x:ints(v,depth-1,lo,hi)

def bits(p):return struct.unpack('<i',struct.pack('<f',p))[0]
def rowsbits(rows):return [[[bits(v)for v in p]for p in row]for row in rows]
def unbits(v):return struct.unpack('<f',struct.pack('<i',v))[0]
names=["quota-one-byte-over","quota-exact-one-output","quota-one-output-over","quota-refusal-count-unchanged","quota-one-node-over","quota-count-fill-exact"]
for c in specs:
 n=c['name'];names.extend(p+n for p in ['same-frame-','same-profile-','no-authority-','detached-points-','detached-map-'])
 if c is specs[0]:names.extend(['prepare-precancel-exact-token','point-copy-precancel-exact-token','map-copy-precancel-exact-token','null-original-refused','equal-data-distinct-frame','equal-data-distinct-document'])
excNames=['prepare-precancel-exact-token','point-copy-precancel-exact-token','map-copy-precancel-exact-token']
diagnosticCases=[(stage,mode)for stage in ['After13B6','AfterCountAndReservation','AfterFillBeforePublication']for mode in ['cancel','fault']]
for stage,mode in diagnosticCases:
 names.extend('diagnostic-'+prefix+'-'+stage+'-'+mode for prefix in ['throw','poison','foreign-thread','four-snapshots','fifth-refused'])

def check(capture):
 assert set(capture)=={'resourceSHA','cases','guards','exceptions','diagnosticResults','networkForwards'}
 integer(capture['networkForwards'],0,0)
 assert capture['resourceSHA']==hashlib.sha256(raw).hexdigest()
 assert capture['guards']==names and all(type(n)is str for n in capture['guards'])
 assert type(capture['cases'])is list and len(capture['cases'])==len(specs)
 assert type(capture['exceptions'])is list and len(capture['exceptions'])==3
 for e,n in zip(capture['exceptions'],excNames):
  assert set(e)=={'name','reached','exactType','tokenMatches','requested'}
  assert e['name']==n and e['exactType']=='System.OperationCanceledException'
  assert all(e[k]is True for k in ['reached','tokenMatches','requested'])
 for got,spec,want in zip(capture['cases'],specs,expected):
  assert set(got)=={'name','originalPointCounts','currentPrefixPointCounts','prefixPointBits','prefixOriginalIndices','pointBits','originalIndices','processedWords','records','descriptorCount','rangeResultCount'}
  assert got['name']==spec['name']
  for key in ['originalPointCounts','currentPrefixPointCounts']:ints(got[key],1,1,16640)
  for key in ['prefixPointBits','pointBits']:ints(got[key],3)
  for key in ['prefixOriginalIndices','originalIndices']:ints(got[key],2,0,16383)
  ints(got['processedWords'],1,0,(1<<32)-1)
  for key in ['descriptorCount','rangeResultCount']:integer(got[key],0,64)
  assert got['originalPointCounts']==list(map(len,ref.original(spec)))
  assert got['currentPrefixPointCounts']==list(map(len,want['prefix']['points']))
  assert got['prefixPointBits']==rowsbits(want['prefix']['points'])
  assert got['prefixOriginalIndices']==want['prefix']['originalPointIndices']
  assert got['originalIndices']==want['originalIndices']
  assert len(got['pointBits'])==len(want['points'])
  for actual,wanted,mode in zip(got['pointBits'],want['points'],want['modes']):
   assert len(actual)==len(wanted)
   for aa,bb in zip(actual,wanted):
    assert len(aa)==2
    for a,b in zip(aa,bb):
     value=unbits(a);assert math.isfinite(value)
     if mode!='spline':assert a==bits(b)
     else:assert abs(value-b)<=5e-5*max(1,abs(b))
  for k in ['processedWords','records','descriptorCount','rangeResultCount']:assert got[k]==want['before'][k]
 assert type(capture['diagnosticResults'])is list and len(capture['diagnosticResults'])==6
 diagnosticWant=expected[next(i for i,c in enumerate(specs)if c['name']=='prior-range-owner-retained')]
 for got,(stage,mode)in zip(capture['diagnosticResults'],diagnosticCases):
  assert set(got)=={'stage','mode','reached','sameView','stages','before','after','exceptionType','message','tokenMatches','tokenRequested'}
  assert got['stage']==stage and got['mode']==mode and got['reached']is True and got['sameView']is True
  assert got['stages']==['After13B6','AfterCountAndReservation','AfterFillBeforePublication'][:1+['After13B6','AfterCountAndReservation','AfterFillBeforePublication'].index(stage)]
  assert got['exceptionType']==('System.OperationCanceledException'if mode=='cancel'else'System.InvalidOperationException')
  assert got['message']==(''if mode=='cancel'else'bounded diagnostic fault')
  assert got['tokenMatches']is(mode=='cancel')and got['tokenRequested']is(mode=='cancel')
  for snap,phase in [(got['before'],2),(got['after'],-1)]:
   assert set(snap)=={'Phase','Claim','OwningThreadMatches','SourcePublished','DescriptorCount','RangeResultCount','OriginalPointCount','PrefixPointCount','ProcessedWordCount','Word0','Word1','Word2'}
   for key in ['Phase','Claim','DescriptorCount','RangeResultCount','OriginalPointCount','PrefixPointCount','ProcessedWordCount']:integer(snap[key],-1,16640)
   for key in ['Word0','Word1','Word2']:integer(snap[key],0,(1<<32)-1)
   assert snap['Phase']==phase and snap['Claim']==1 and snap['OwningThreadMatches']is True and snap['SourcePublished']is False
   assert snap['DescriptorCount']==diagnosticWant['before']['descriptorCount']and snap['RangeResultCount']==diagnosticWant['before']['rangeResultCount']
   assert snap['OriginalPointCount']==11 and snap['PrefixPointCount']==sum(map(len,diagnosticWant['prefix']['points']))
   words=diagnosticWant['before']['processedWords'];assert snap['ProcessedWordCount']==len(words)
   assert [snap['Word0'],snap['Word1'],snap['Word2']]==words+[0]*(3-len(words))
 #Current-prefix counts, never an assumption about original row count.
 by={c['name']:c for c in capture['cases']}
 for n in [8191,8192,8193,16384]:
  key=next(k for k in by if k.startswith('raw-'+str(n)+'-'))
  assert by[key]['currentPrefixPointCounts']==[n]
 return dict(cases=len(specs),guards=len(names),exceptions=3,diagnosticOwners=6,networkForwards=0,outputs=sum(sum(map(len,c['pointBits']))for c in capture['cases']),scope='Portable scalar source; spline coordinates tolerant, discrete counts/maps/range bytes exact; no CRT/FP-environment identity')

def negatives(original):
 controls=[]
 def reject(name,change):
  bad=copy.deepcopy(original);change(bad)
  try:check(bad)
  except(AssertionError,KeyError,TypeError,ValueError,OverflowError,struct.error):controls.append(name);return
  raise AssertionError('Accepted negative: '+name)
 reject('point-coordinate',lambda c:c['cases'][1]['pointBits'][0][0].__setitem__(0,bits(12345.0)))
 reject('original-key',lambda c:c['cases'][1]['originalIndices'][0].__setitem__(0,16383))
 reject('prefix-count',lambda c:c['cases'][0]['currentPrefixPointCounts'].__setitem__(0,5))
 reject('bitmap',lambda c:c['cases'][0]['processedWords'].__setitem__(0,1))
 reject('descriptor-counter',lambda c:c['cases'][0].__setitem__('descriptorCount',1))
 reject('network-bool',lambda c:c.__setitem__('networkForwards',False))
 reject('point-bool',lambda c:c['cases'][1]['pointBits'][0][0].__setitem__(1,False))
 reject('key-bool',lambda c:c['cases'][1]['originalIndices'][0].__setitem__(0,False))
 reject('missing-guard',lambda c:c['guards'].pop())
 reject('token-mismatch',lambda c:c['exceptions'][0].__setitem__('tokenMatches',False))
 reject('unreached-cancel',lambda c:c['exceptions'][0].__setitem__('reached',False))
 reject('missing-case',lambda c:c['cases'].pop())
 reject('diagnostic-unpoisoned-phase',lambda c:c['diagnosticResults'][0]['after'].__setitem__('Phase',2))
 reject('diagnostic-restored-claim',lambda c:c['diagnosticResults'][0]['after'].__setitem__('Claim',0))
 reject('diagnostic-corrupted-prefix',lambda c:c['diagnosticResults'][0]['after'].__setitem__('Word0',0))
 reject('diagnostic-claim-bool',lambda c:c['diagnosticResults'][0]['after'].__setitem__('Claim',True))
 reject('diagnostic-other-view',lambda c:c['diagnosticResults'][0].__setitem__('sameView',False))
 reject('diagnostic-wrong-fault-message',lambda c:c['diagnosticResults'][1].__setitem__('message','other fault'))
 return controls
capture=json.loads(Path(sys.argv[4]).read_text(encoding='utf-8-sig'));report=check(capture);report['negativeControls']=negatives(capture)
if len(sys.argv)>5:Path(sys.argv[5]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(json.dumps(report))
