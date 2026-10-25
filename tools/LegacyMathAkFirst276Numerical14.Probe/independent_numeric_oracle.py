"""Independent integer model replay and strict partial numerical capture audit."""
from pathlib import Path
import hashlib,json,struct,argparse,copy
D=Path(__file__).resolve().parent
H=lambda b:hashlib.sha256(b).hexdigest()
def loads(raw):
 def pairs(rows):
  result={}
  for k,v in rows:assert k not in result,('duplicate key',k);result[k]=v
  return result
 return json.loads(raw,object_pairs_hook=pairs,parse_constant=lambda x:(_ for _ in ()).throw(AssertionError(x)))
def signed32(x):return (x+2**31)%2**32-2**31
def run_layers(input276,first,second,scales,hidden_lut,exp_lut):
 """Independent modular scalar equations; reads only known276/201 lanes with physical strides."""
 assert len(input276)==276 and input276[0]==1024
 assert all(type(x)is int and -32768<=x<=32767 for x in input276)
 assert len(first)==57600 and len(second)==53456 and len(scales)==258
 assert len(hidden_lut)==48 and len(exp_lut)==352
 assert all(first[r*288+i]==0 for r in range(200)for i in range(276,288))
 assert all(second[r*208+i]==0 for r in range(257)for i in range(201,208))
 hidden=[None]*201
 for row in range(200):
  # Single full precision sum followed by mod32 is independent from C# lane accumulation.
  value=signed32(sum(input276[i]*first[row*288+i]for i in range(276)))
  if value==-(2**31):raise ValueError('INT_MIN hidden lookup domain.')
  index=abs(value)//65536;magnitude=hidden_lut[index]if index<48 else 1018
  hidden[row+1]=-magnitude if value<=0 else magnitude
 hidden[0]=1024
 raw=[signed32(sum(hidden[i]*second[row*208+i]for i in range(201))*scales[row+1])for row in range(257)]
 c0=max(18808832,*raw)-18808832
 assert 0<=c0<=2**31-1-18808832
 exp=[]
 for value in raw:
  shifted=signed32(value-c0+4194304)
  exp.append(0 if shifted<0 else exp_lut[min(351,shifted//65536)])
 return dict(Hidden201=hidden,Exponential257=exp,StabilizationOffsetC0=c0)
def root_expected(resource):
 """Replay authenticated weights; no managed calls or resource reads on import."""
 pins=loads((D/'oracle-input-pins.json').read_bytes());raw=resource.read_bytes()
 assert len(raw)==pins['rawResourceBytes']and H(raw)==pins['rawResourceSHA256']
 input_raw=(D/'inputs/ROOT-reviewed-independent-input-expectations.json').read_bytes();assert H(input_raw)==pins['independentInputsSHA256']
 hraw=(D/'inputs/native-hidden-LUT.bin').read_bytes();eraw=(D/'inputs/native-exp-LUT.bin').read_bytes()
 assert H(hraw)==pins['hiddenLUTSHA256']and H(eraw)==pins['expLUTSHA256']
 assert struct.unpack_from('<II',raw,8702)==(0x0107fea0,168940)
 assert struct.unpack_from('<IIIIIH',raw,8713)==(276,201,0,0,258,10)
 first=struct.unpack_from('<57600h',raw,8736);second=struct.unpack_from('<53456b',raw,123936);scales=raw[177392:177650]
 assert scales[0]==1 and all(scales)
 hidden_lut=struct.unpack('<48i',hraw);exp_lut=struct.unpack('<352i',eraw)
 features=loads(input_raw);assert len(features['cases'])==14
 # Re-derive features from independent sampler/segment recordings. The checked
 # cached vectors are not accepted merely because C# produced the same values.
 import importlib.util
 prior=D.parent/'LegacyMathAkFirst276Input14.Probe/independent_oracle.py'
 spec=importlib.util.spec_from_file_location('prior_independent_features',prior)
 module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
 strict(features,module.expected(D.parents[1]))
 rows=[]
 for case in features['cases']:
  if not case['featureEligible']:rows.append(dict(name=case['name'],featureEligible=False,reason=case['reason'],numerical=None));continue
  rows.append(dict(name=case['name'],featureEligible=True,initializedInput=case['values'],numerical=run_layers(case['values'],first,second,scales,hidden_lut,exp_lut)))
 assert sum(x['featureEligible']for x in rows)==12
 return dict(scope='Independent actual-weight numerical expectations from prior ROOT-reviewed input vectors, not new capture self-expectations',cases=rows)
def strict(actual,want,path='/'):
 assert type(actual)is type(want),(path,'type')
 if type(want)is dict:
  assert actual.keys()==want.keys(),(path,'keys')
  for k in want:strict(actual[k],want[k],path+k+'/')
 elif type(want)is list:
  assert len(actual)==len(want),(path,'length')
  for i,(a,b)in enumerate(zip(actual,want)):strict(a,b,path+str(i)+'/')
 else:assert actual==want,(path,actual,want)
def audit_capture(capture,expected):
 assert type(capture)is dict and set(capture)=={'schema','resourceSHA','fixtureSHA','cases','successfulProfiles','owningSourceCalls','partialNumericalPrefixes','earlierRefusals','normalizationCalls','fullEvaluatorSuccesses','fullOldALHeaderProjectionVerified','liveMultiRangeCoverage','physicalPaddingExported','exportedShortPayloadBytes','exportedNumericalArrayBytes','maximumCaptureBytes','capMeaning'}
 assert capture['schema']=='first276-numerical14-partial-v1'
 assert capture['resourceSHA']=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
 assert capture['fixtureSHA']=='ce3505ffeb0014a62eacde7c2d4841a23dda5f704fb127820fac4c937134eacb'
 assert capture['owningSourceCalls']==14 and capture['successfulProfiles']==1
 assert capture['fullEvaluatorSuccesses']==capture['normalizationCalls']==0
 assert capture['liveMultiRangeCoverage']is False and capture['physicalPaddingExported']is False
 assert len(capture['cases'])==14;complete=refused=0
 for got,want in zip(capture['cases'],expected['cases']):
  assert type(got)is dict and set(got)=={'name','outcome','exceptionType','message','data','numerical'}
  assert got['name']==want['name']
  if not want['featureEligible']:
   assert got['outcome']=='earlier-refusal-no-input' and got['message']==want['reason']and got['data']is None and got['numerical']is None
   refused+=1;continue
  # Required all12 eligible cases: all-refusal captures cannot pass this gate.
  assert got['outcome']=='partial-numerical-missing-normalization'
  assert got['exceptionType']=='InkRecognition.LegacyMathAkOriginalInputContext+First276PartialNumericalBoundaryException'
  assert got['message']=='Actual FIRST numerical prefix complete; normalization/shortlist continuation remains unported.'
  strict(got['numerical'],want['numerical'],want['name']+'/numerical/')
  d=got['data'];assert set(d)=={'EndNode','SuffixCount','SelectedCount','RangeCount','WorkWords','CacheEntries','Descriptor108Count','PositiveDescriptor8Count','Positive1390Count','InitializedInputCount','ChargedPayloadBytes','ChargedCopyBytes','ChargedSourceWork','HasInitializedFeatures','HasEvaluatorOutput','InitializedInput'}
  strict(d['InitializedInput'],want['initializedInput'])
  for k,v in dict(EndNode=0,SuffixCount=1,SelectedCount=1,RangeCount=1,CacheEntries=1,Descriptor108Count=1,InitializedInputCount=276).items():assert type(d[k])is int and d[k]==v
  assert d['HasInitializedFeatures']is True and d['HasEvaluatorOutput']is False
  assert len(got['numerical']['Hidden201'])==201 and len(got['numerical']['Exponential257'])==257
  complete+=1
 assert complete==12 and refused==2 and capture['partialNumericalPrefixes']==12 and capture['earlierRefusals']==2
 assert capture['exportedShortPayloadBytes']==552*12 and capture['exportedNumericalArrayBytes']==1430*12
 return dict(PASS=True,exactHiddenAndPostExpCases=12,expectedPointDomainRefusals=2,normalizationVerified=False,evaluatorSuccesses=0,physicalPaddingKnown=False)
def synthetic_controls():
 # Deliberately fake weights and fake LUT values. No real resource or original
 # feature vector is loaded/evaluated in author controls.
 first=[0]*57600;second=[0]*53456;scales=[1]*258;features=[1024]+[0]*275
 hidden_lut=list(range(48));exp_lut=list(range(352))
 e=run_layers(features,first,second,scales,hidden_lut,exp_lut)
 assert e==dict(Hidden201=[1024]+[0]*200,Exponential257=[64]*257,StabilizationOffsetC0=0)
 negatives=[]
 for label,mutation in [('hidden',lambda x:x['Hidden201'].__setitem__(1,1)),('exp',lambda x:x['Exponential257'].__setitem__(0,65)),('C0',lambda x:x.__setitem__('StabilizationOffsetC0',1)),('padding',lambda x:x.__setitem__('slot0',0)),('type',lambda x:x['Hidden201'].__setitem__(0,True))]:
  bad=copy.deepcopy(e);mutation(bad)
  try:strict(bad,e)
  except AssertionError:negatives.append(label)
  else:raise AssertionError(label)
 return dict(scope='Fake zero weights/fake LUT synthetic only; no actual-resource evaluation',syntheticRows=1,negativeControls=negatives,actualModelEvaluations=0)
if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--synthetic-controls',action='store_true');p.add_argument('--resource',type=Path);p.add_argument('--capture',type=Path);p.add_argument('--expected-output',type=Path);p.add_argument('--report',type=Path);p.add_argument('--selftest',action='store_true');a=p.parse_args()
 if a.synthetic_controls:
  assert not a.resource and not a.capture and not a.expected_output
  report=synthetic_controls()
 else:
  assert a.resource,'ROOT-only actual-weight replay requires explicit --resource'
  expected=root_expected(a.resource)
  if a.expected_output:assert not a.expected_output.exists();a.expected_output.write_text(json.dumps(expected,indent=2)+'\n',encoding='utf-8')
  report=audit_capture(loads(a.capture.read_bytes()),expected)if a.capture else dict(scope='ROOT prepared actual-weight expectations only; no native/managed call',cases=12)
  if a.selftest:
   assert a.capture
   good=loads(a.capture.read_bytes());mutants=[]
   for i,row in enumerate(good['cases']):
    if row['outcome']!='partial-numerical-missing-normalization':continue
    for lane in ['Hidden201','Exponential257']:
     bad=copy.deepcopy(good);bad['cases'][i]['numerical'][lane][1]+=1;mutants.append(bad)
    bad=copy.deepcopy(good);bad['cases'][i]['numerical']['StabilizationOffsetC0']+=1;mutants.append(bad)
   bad=copy.deepcopy(good)
   for row in bad['cases']:
    row.update(outcome='earlier-refusal-no-input',data=None,numerical=None)
   mutants.append(bad)
   for bad in mutants:
    try:audit_capture(bad,expected)
    except AssertionError:pass
    else:raise AssertionError('Numerical corruption or all-refusal accepted')
   report['rejectedMutations']=len(mutants)
 if a.report:assert not a.report.exists();a.report.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
 print(json.dumps(report))
