"""SOURCE/PURE replay of immutable prior sampler/segment DATA, never a live issuer."""
from pathlib import Path
import argparse,hashlib,json,struct,importlib.util,copy
D=Path(__file__).resolve().parent;ROOT=D.parents[1]
H=lambda b:hashlib.sha256(b).hexdigest()
def load(p):
 def obj(pairs):
  d={}
  for k,v in pairs:
   assert k not in d,('duplicate key',k);d[k]=v
  return d
 return json.loads(p.read_bytes(),object_pairs_hook=obj,parse_constant=lambda x:(_ for _ in ()).throw(AssertionError(x)))
def expected(repo_root=None):
 root=Path(repo_root).resolve() if repo_root else ROOT
 for row in load(D/'readset.json'):
  path=(D/row['path']) if row['root']=='probe' else (root/row['path'])
  actual=(path.stat().st_size,H(path.read_bytes()))
  variants=row['variants'] if 'variants'in row else [row]
  assert any(actual==(v['bytes'],v['sha256'])for v in variants),row['path']
 sampler=load(D/'inputs/sampler14.json');segments=load(D/'inputs/segments14.json')
 fixtures=load(D/'inputs/fixtures.json');oldAL=load(D/'inputs/old-al14.json');oldHeader=load(D/'inputs/old-header14.json')
 names=[x['name'] for x in fixtures['cases']]
 assert len(names)==14 and len(set(names))==14
 assert all([x['name']for x in d['cases']]==names for d in [sampler,segments,oldAL,oldHeader])
 assert sampler['fixtureSHA']==segments['fixtureSHA']=='ce3505ffeb0014a62eacde7c2d4841a23dda5f704fb127820fac4c937134eacb'
 sp=importlib.util.spec_from_file_location('independent_scalar_features',D/'oracle_parts.py');parts=importlib.util.module_from_spec(sp);parts.REPO_ROOT=root;sp.loader.exec_module(parts)
 out=[]
 for s,g,al in zip(sampler['cases'],segments['cases'],oldAL['cases']):
  sd=s['data'];gd=g['data'];assert gd['Sampler']==sd
  bits=sd['PointBits'];ends=sd['Ends'];triples=gd['RangeTriples'];assert len(bits)%2==0 and ends[-1]*2==len(bits)
  assert len(triples)==3*gd['SegmentCount'] and gd['SegmentCount']>=1
  # Actual producer stops on endpoint0/suffix1: the first raw segment only.
  oid,start,end=triples[:3];assert 0<=oid<len(ends) and 0<=start<=end
  first=0 if oid==0 else ends[oid-1];length=ends[oid]-first;assert end<length
  decode=lambda i:struct.unpack('<f',struct.pack('<I',bits[i]&0xffffffff))[0]
  rows=[[[decode(2*i),decode(2*i+1)] for i in range(first+start,first+end+1)]]
  item=dict(name=s['name'],firstRequest=dict(endNode=0,suffixCount=1,selectedCount=1,rangeCount=1,range=[oid,start,end],sampledStrokeLength=length),workWords=(gd['SegmentCount']>>5)+1,priorPositiveDescriptor8Count=al['data']['Header']['PositiveDescriptor8Count'],priorPositive1390Count=al['data']['Header']['Positive1390Count'])
  if end-start+1>4096:item.update(featureEligible=False,reason='Existing directional source-point domain exceeded.',values=None)
  else:
   calc=parts.calculate(rows,start,end,length)
   item.update(featureEligible=True,values=calc['values'],valuesSHA256=calc['valuesSHA256'],preparedPointCount=calc['preparedPointCount'])
   assert len(item['values'])==276 and item['values'][0]==1024
   assert item['values'][-2:]==[1024 if start==0 else -1024,1024 if end+1==length else -1024]
  out.append(item)
 return dict(schema='first276-input14-independent-expectations-v1',cases=out,actualManagedRuns=0,liveMultiRangeCoverage=False,oldALHeaderDataUsedOnlyForVisibleCounterExpectations=True)
def check(c,e):
 assert type(c)is dict and set(c)=={'schema','resourceSHA','fixtureSHA','cases','successfulProfiles','owningSourceCalls','completeInputBoundaries','earlierRefusals','modelEvaluations','evaluatorSuccesses','fullOldALHeaderProjectionVerified','liveMultiRangeCoverage','physicalPaddingExported','exportedShortPayloadBytes','maximumCaptureBytes','capMeaning'}
 assert c['schema']=='first276-input14-boundary-v1'
 assert c['resourceSHA']=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
 assert c['fixtureSHA']=='ce3505ffeb0014a62eacde7c2d4841a23dda5f704fb127820fac4c937134eacb'
 for k,v in dict(owningSourceCalls=14,successfulProfiles=1,modelEvaluations=0,evaluatorSuccesses=0,maximumCaptureBytes=262144).items():assert type(c[k])is int and c[k]==v
 for k in ['completeInputBoundaries','earlierRefusals','exportedShortPayloadBytes']:assert type(c[k])is int and c[k]>=0
 assert c['capMeaning']=='accepted serialized UTF8 extent; not allocator/RSS bound'
 for k in ['fullOldALHeaderProjectionVerified','liveMultiRangeCoverage','physicalPaddingExported']:assert c[k] is False
 assert len(c['cases'])==14;complete=earlier=0;refusals=[]
 for got,want in zip(c['cases'],e['cases']):
  assert type(got)is dict and set(got)=={'name','outcome','exceptionType','message','data'}
  assert got['name']==want['name']
  if got['outcome']=='earlier-refusal-no-input':
   assert want['featureEligible'] is False and got['name'] in {'raw-8191-smooth','raw-8192-smooth'}
   assert got['message']=='Existing directional source-point domain exceeded.'
   assert got['data'] is None and got['exceptionType']=='System.NotSupportedException' and type(got['message'])is str and 0<len(got['message'])<=1024
   earlier+=1;refusals.append(dict(name=got['name'],message=got['message']));continue
  assert got['outcome']=='complete-input-missing-evaluator' and want['featureEligible']
  assert got['message']=='Actual FIRST evaluator/shortlist continuation remains unported.'
  assert got['exceptionType']=='InkRecognition.LegacyMathAkOriginalInputContext+First276InputBoundaryException'
  d=got['data'];assert set(d)=={'EndNode','SuffixCount','SelectedCount','RangeCount','WorkWords','CacheEntries','Descriptor108Count','PositiveDescriptor8Count','Positive1390Count','InitializedInputCount','ChargedPayloadBytes','ChargedCopyBytes','ChargedSourceWork','HasInitializedFeatures','HasEvaluatorOutput','InitializedInput'}
  for k,v in dict(EndNode=0,SuffixCount=1,SelectedCount=1,RangeCount=1,WorkWords=want['workWords'],CacheEntries=1,Descriptor108Count=1,PositiveDescriptor8Count=want['priorPositiveDescriptor8Count'],Positive1390Count=want['priorPositive1390Count'],InitializedInputCount=276).items():assert type(d[k])is int and d[k]==v,(got['name'],k)
  assert d['HasInitializedFeatures'] is True and d['HasEvaluatorOutput'] is False
  for k in ['ChargedPayloadBytes','ChargedCopyBytes','ChargedSourceWork']:assert type(d[k])is int and 0<=d[k]<2**63
  assert type(d['InitializedInput'])is list and len(d['InitializedInput'])==276
  assert all(type(x)is int and -32768<=x<=32767 for x in d['InitializedInput'])
  assert d['InitializedInput']==want['values'],got['name'];complete+=1
 assert complete==12 and earlier==2 and sum(w['featureEligible']for w in e['cases'])==12
 assert {r['name']for r in refusals}=={'raw-8191-smooth','raw-8192-smooth'}
 assert c['completeInputBoundaries']==complete and c['earlierRefusals']==earlier and c['exportedShortPayloadBytes']==552*complete
 return dict(exactCompleteInputCases=complete,earlierRefusals=refusals,full14FeatureEquality=complete==14,fullOldALHeaderProjectionVerified=False,liveMultiRangeCoverage=False,evaluatorSuccesses=0)
if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--repo-root',type=Path,default=ROOT);p.add_argument('--expected-output',type=Path);p.add_argument('--capture',type=Path);p.add_argument('--report',type=Path);a=p.parse_args();e=expected(a.repo_root)
 if a.expected_output:
  assert not a.expected_output.exists();a.expected_output.write_text(json.dumps(e,indent=2)+'\n',encoding='utf-8')
 if a.capture:
  assert a.report and not a.report.exists() and a.capture.stat().st_size<=262144
  result=check(load(a.capture),e);a.report.write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
 print('PURE independent expectations',len(e['cases']),'eligible',sum(x['featureEligible']for x in e['cases']),'managed runs 0')
