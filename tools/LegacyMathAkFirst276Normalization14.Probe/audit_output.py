"""Strict DATA audit against independent wide-integer normalization expectations."""
from pathlib import Path
import argparse,copy,json,hashlib
D=Path(__file__).resolve().parent
def load(path):
 def unique(rows):
  out={}
  for k,v in rows:assert k not in out;out[k]=v
  return out
 return json.loads(Path(path).read_bytes(),object_pairs_hook=unique,parse_constant=lambda x:(_ for _ in ()).throw(AssertionError(x)))
def equal(a,w):
 assert type(a)is type(w)
 if type(w)is dict:
  assert a.keys()==w.keys()
  for k in w:equal(a[k],w[k])
 elif type(w)is list:
  assert len(a)==len(w)
  for x,y in zip(a,w):equal(x,y)
 else:assert a==w
def normalize(values):
 assert type(values)is list and len(values)==257 and all(type(x)is int and 0<=x<=1641499228 for x in values)
 work=values.copy();halves=0
 while sum(work)>2147483647:work=[x//2 for x in work];halves+=1
 total=sum(work);maximum=max(work)
 if maximum==0:return dict(written=False,zeroMaximumNoOp=True,C4=None,A0=None,Q90=None,halvings=halves)
 k=max(0,total.bit_length()-7);divisor=total>>k;shift=30-(maximum.bit_length()-1)
 probability=[min(32767,(((x<<shift)&0xffffffff)//divisor)>>((shift+k-15)&31))for x in work]
 return dict(written=True,zeroMaximumNoOp=False,C4=divisor<<k,A0=probability,Q90=[x>>5 for x in probability],halvings=halves)

def audit(c,e):
 assert set(c)=={'schema','resourceSHA','fixtureSHA','cases','successfulProfiles','owningSourceCalls','normalizationBoundaries','earlierRefusals','fullEvaluatorSuccesses','shortlistSuccesses','physicalPaddingExported','liveMultiRangeCoverage','maximumCaptureBytes','capMeaning'}
 assert c['schema']=='first276-normalization14-partial-v1'
 assert c['resourceSHA']=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'and c['fixtureSHA']=='ce3505ffeb0014a62eacde7c2d4841a23dda5f704fb127820fac4c937134eacb'
 equal({k:c[k]for k in ['successfulProfiles','owningSourceCalls','normalizationBoundaries','earlierRefusals','fullEvaluatorSuccesses','shortlistSuccesses','physicalPaddingExported','liveMultiRangeCoverage','maximumCaptureBytes']},dict(successfulProfiles=1,owningSourceCalls=14,normalizationBoundaries=12,earlierRefusals=2,fullEvaluatorSuccesses=0,shortlistSuccesses=0,physicalPaddingExported=False,liveMultiRangeCoverage=False,maximumCaptureBytes=262144))
 assert type(c['cases'])is list and len(c['cases'])==14
 good=bad=0
 for actual,want in zip(c['cases'],e['cases']):
  assert set(actual)=={'name','outcome','exceptionType','message','data','normalized'}and actual['name']==want['name']
  if not want['eligible']:
   equal(actual,dict(name=want['name'],outcome='earlier-refusal-no-input',exceptionType='System.NotSupportedException',message=want['reason'],data=None,normalized=None));bad+=1;continue
  assert actual['outcome']=='normalized-missing-shortlist'and actual['exceptionType']=='InkRecognition.LegacyMathAkOriginalInputContext+First276NormalizedBoundaryException'
  assert actual['message']=='Actual FIRST normalization boundary; AL scoring/shortlist continuation remains unported.'
  n=want['normalization'];equal(actual['normalized'],dict(NativeC0=want['NativeC0'],NativeC4=n['C4'],HasNormalizedOutputs=n['written'],ZeroMaximumNoOp=n['zeroMaximumNoOp'],Quantized90=n['Q90'],ProbabilityA0=n['A0']))
  data=actual['data'];assert set(data)=={'EndNode','SuffixCount','SelectedCount','RangeCount','WorkWords','CacheEntries','Descriptor108Count','PositiveDescriptor8Count','Positive1390Count','InitializedInputCount','ChargedPayloadBytes','ChargedCopyBytes','ChargedSourceWork','HasInitializedFeatures','HasEvaluatorOutput','InitializedInput'}
  equal({k:data[k]for k in ['EndNode','SuffixCount','SelectedCount','RangeCount','CacheEntries','Descriptor108Count','InitializedInputCount','HasInitializedFeatures','HasEvaluatorOutput','InitializedInput']},dict(EndNode=0,SuffixCount=1,SelectedCount=1,RangeCount=1,CacheEntries=1,Descriptor108Count=1,InitializedInputCount=276,HasInitializedFeatures=True,HasEvaluatorOutput=False,InitializedInput=None))
  for k in ['ChargedPayloadBytes','ChargedCopyBytes','ChargedSourceWork']:assert type(data[k])is int and 0<data[k]<=67108864
  good+=1
 assert good==12 and bad==2
 return dict(PASS=True,exactNormalizedCases=12,probabilityValuesCompared=257*12,quantizedValuesCompared=257*12,originalDomainRefusals=2,shortlistSuccesses=0)
if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('capture',type=Path);p.add_argument('report',type=Path);p.add_argument('--selftest',action='store_true');p.add_argument('--resource',type=Path,required=True);a=p.parse_args();assert not a.report.exists()
 import importlib.util
 spec=importlib.util.spec_from_file_location('independent_numeric',D.parent/'LegacyMathAkFirst276Numerical14.Probe/independent_numeric_oracle.py')
 numeric=importlib.util.module_from_spec(spec);spec.loader.exec_module(numeric)
 base=numeric.root_expected(a.resource)
 expected={'cases':[]}
 for case in base['cases']:
  if case['featureEligible']:
   expected['cases'].append(dict(name=case['name'],eligible=True,NativeC0=case['numerical']['StabilizationOffsetC0'],normalization=normalize(case['numerical']['Exponential257'])))
  else:expected['cases'].append(dict(name=case['name'],eligible=False,reason=case['reason']))
 assert a.capture.stat().st_size<=262144;capture=load(a.capture);report=audit(capture,expected)
 if a.selftest:
  count=0
  def reject(bad):
   global count
   try:audit(bad,expected)
   except AssertionError:count+=1
   else:raise AssertionError('Corrupt normalization or all-refusal accepted')
  for i,row in enumerate(capture['cases']):
   if row['outcome']=='earlier-refusal-no-input':continue
   for key in ['Quantized90','ProbabilityA0']:
    bad=copy.deepcopy(capture);bad['cases'][i]['normalized'][key][0]+=1;reject(bad)
   bad=copy.deepcopy(capture);bad['cases'][i]['normalized']['NativeC4']+=1;reject(bad)
  bad=copy.deepcopy(capture)
  for row in bad['cases']:row.update(outcome='earlier-refusal-no-input',data=None,normalized=None)
  reject(bad);report['rejectedMutations']=count
 a.report.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
