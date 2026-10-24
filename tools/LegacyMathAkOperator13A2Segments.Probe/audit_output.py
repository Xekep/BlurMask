"""Independent segment DATA audit. No library, vendor or neural execution.
CLI: DEPENDENCY_ROOT MODELS FIXTURES CAPTURE_OR_--prepared-controls FRESH_REPORT
"""
import sys,json,struct,math,base64,copy,importlib.util
from pathlib import Path
D=Path(__file__).resolve().parent
spec=importlib.util.spec_from_file_location('segment_reference',D/'reference_segments.py');ref=importlib.util.module_from_spec(spec);spec.loader.exec_module(ref)
ARRAYS=['BeforePointBits','BeforeOriginalIndices','BeforeEnds','PointBits','OriginalIndices','Ends','MarkerFields','RangeFields','MetricBits']
COSTS=['SamplerChargedBytes','SamplerChargedWork','DiagnosticChargedBytes','DiagnosticChargedWork']
SCALARS=['Version','SegmentCount','TotalMembership','RequiredWorkWords','BridgeChargedBytes','BridgeChargedWork','DiagnosticChargedBytes','DiagnosticChargedWork']
def integer(v,lo=-(1<<31),hi=(1<<31)-1):assert type(v)is int and lo<=v<=hi

def names(expected):
 n=['fresh-precancel-exact-token','null-source-refused']
 for e in expected['cases']:n.extend(p+e['name']for p in ['version-','segment-shape-','original-membership-','membership-order-','required-word-count-','known-mask-and-header-','finite-range-metrics-','bridge-ledger-','diagnostic-ledger-'])
 return n

def fbits(v):return struct.unpack('<f',struct.pack('<i',v))[0]
def compare(a,b,modes,ends,stride):
 assert len(a)==len(b);start=0
 for mode,end in zip(modes,ends):
  for x,y in zip(a[stride*start:stride*end],b[stride*start:stride*end]):
   if mode!='spline':assert x==y
   else:assert math.isfinite(fbits(x))and abs(fbits(x)-fbits(y))<=5e-5*max(1,abs(fbits(y)))
  start=end
 assert start*stride==len(a)

def sampler(data,want):
 assert type(data)is dict and set(data)==set(ARRAYS+COSTS+['Version','HeightBits'])
 integer(data['Version'],1,1);integer(data['HeightBits']);assert data['HeightBits']==want['HeightBits']
 for k in ARRAYS:
  assert type(data[k])is list
  for v in data[k]:integer(v)
 for k in ['BeforeOriginalIndices','BeforeEnds','OriginalIndices','Ends','MarkerFields','RangeFields']:assert data[k]==want[k]
 compare(data['BeforePointBits'],want['BeforePointBits'],want['upstreamModes'],want['BeforeEnds'],2)
 compare(data['PointBits'],want['PointBits'],want['upstreamModes'],want['Ends'],2)
 compare(data['MetricBits'],want['MetricBits'],want['upstreamModes'],list(range(1,len(want['Ends'])+1)),6)
 for k in COSTS:integer(data[k],0,(1<<63)-1)
 before=len(want['BeforeOriginalIndices']);after=len(want['OriginalIndices']);m=len(want['MarkerFields'])//3;s=len(want['Ends'])
 b=256+12*before+12*after+24*m+32*s;w=6*before+6*after+12*m+12*s
 assert data['DiagnosticChargedBytes']==b<=64*1024*1024 and data['DiagnosticChargedWork']==w<=64*1024*1024
 assert data['SamplerChargedBytes']<=128*1024*1024 and data['SamplerChargedWork']<=1_000_000_000
 return b

def check(capture,expected):
 assert type(capture)is dict and set(capture)=={'resourceSHA','fixtureSHA','cases','guards','retainedDiagnosticBytes','networkForwards','processedGraphAuthority','matcherRequestAuthority','workspaceAuthority'}
 integer(capture['networkForwards'],0,0)
 assert all(capture[k]is False for k in ['processedGraphAuthority','matcherRequestAuthority','workspaceAuthority'])
 assert capture['resourceSHA']==expected['resourceSHA']and capture['fixtureSHA']==expected['fixtureSHA']
 assert capture['guards']==names(expected)and all(type(x)is str for x in capture['guards'])
 assert type(capture['cases'])is list and len(capture['cases'])==14;retained=0;segments=0
 for got,want in zip(capture['cases'],expected['cases']):
  assert type(got)is dict and set(got)=={'name','data'}and got['name']==want['name'];d=got['data']
  assert type(d)is dict and set(d)==set(SCALARS+['Sampler','RecordBytes','DefinedMasks','RangeTriples','OriginalOffsets'])
  retained+=sampler(d['Sampler'],want['sampler'])
  for k in SCALARS:integer(d[k],0,(1<<63)-1)
  assert d['Version']==1
  for k in ['RangeTriples','OriginalOffsets']:
   assert type(d[k])is list
   for v in d[k]:integer(v,0)
   assert d[k]==want[k[0].lower()+k[1:]]
  for key in ['RecordBytes','DefinedMasks']:assert type(d[key])is str
  raw=base64.b64decode(d['RecordBytes'],validate=True);wr=base64.b64decode(want['recordBytes'],validate=True)
  mask=base64.b64decode(d['DefinedMasks'],validate=True);assert mask==base64.b64decode(want['definedMasks'],validate=True)
  m=want['segmentCount'];s=len(want['sampler']['Ends']);assert len(raw)==96*m and len(mask)==12*m
  # Only six defined metric F32 words inherit upstream spline tolerance. Every
  # other defined byte and every unknown zero placeholder is exact DATA.
  for i in range(m):
   a=raw[96*i:96*(i+1)];b=wr[96*i:96*(i+1)];assert a[:0x24]==b[:0x24]and a[0x3c:]==b[0x3c:]
   oid=d['RangeTriples'][3*i];mode=want['sampler']['upstreamModes'][oid]
   compare(list(struct.unpack_from('<6i',a,0x24)),list(struct.unpack_from('<6i',b,0x24)),[mode],[1],6)
  for k,w in [('SegmentCount','segmentCount'),('TotalMembership','totalMembership'),('RequiredWorkWords','requiredWorkWords'),('BridgeChargedBytes','bridgeBytes'),('BridgeChargedWork','bridgeWork'),('DiagnosticChargedBytes','diagnosticBytes'),('DiagnosticChargedWork','diagnosticWork')]:assert d[k]==want[w]
  assert d['BridgeChargedBytes']<=64*1024*1024 and d['BridgeChargedWork']<=64*1024*1024 and d['DiagnosticChargedBytes']<=64*1024*1024 and d['DiagnosticChargedWork']<=64*1024*1024
  retained+=d['DiagnosticChargedBytes'];segments+=m
 integer(capture['retainedDiagnosticBytes'],0,128*1024*1024);assert capture['retainedDiagnosticBytes']==retained
 return dict(cases=14,segments=segments,guards=len(names(expected)),knownBytesPerRecord=49,networkForwards=0,scope='Numerical DATA only; no workspace/request/graph authority. Exact discrete fields; inherited13A0 spline tolerance on coordinates/metrics only. Sampler totals bounded, not reconstructed.')

def prepared(expected):
 cases=[];retained=0
 for e in expected['cases']:
  w=e['sampler'];before=len(w['BeforeOriginalIndices']);after=len(w['OriginalIndices']);m=len(w['MarkerFields'])//3;s=len(w['Ends']);b=256+12*before+12*after+24*m+32*s
  sd={k:w[k]for k in ARRAYS+['HeightBits']};sd.update(Version=1,SamplerChargedBytes=0,SamplerChargedWork=0,DiagnosticChargedBytes=b,DiagnosticChargedWork=6*before+6*after+12*m+12*s)
  d=dict(Version=1,Sampler=sd,RecordBytes=e['recordBytes'],DefinedMasks=e['definedMasks'],RangeTriples=e['rangeTriples'],OriginalOffsets=e['originalOffsets'],SegmentCount=e['segmentCount'],TotalMembership=e['totalMembership'],RequiredWorkWords=e['requiredWorkWords'],BridgeChargedBytes=e['bridgeBytes'],BridgeChargedWork=e['bridgeWork'],DiagnosticChargedBytes=e['diagnosticBytes'],DiagnosticChargedWork=e['diagnosticWork']);cases.append(dict(name=e['name'],data=d));retained+=b+e['diagnosticBytes']
 return dict(resourceSHA=expected['resourceSHA'],fixtureSHA=expected['fixtureSHA'],cases=cases,guards=names(expected),retainedDiagnosticBytes=retained,networkForwards=0,processedGraphAuthority=False,matcherRequestAuthority=False,workspaceAuthority=False)

def negatives(capture,expected):
 out=[]
 def reject(name,edit):
  c=copy.deepcopy(capture);edit(c)
  try:check(c,expected)
  except(AssertionError,KeyError,TypeError,ValueError):out.append(name)
  else:raise AssertionError('Accepted corruption '+name)
 first=lambda c:c['cases'][0]['data']
 def byte(c,key,index):
  d=first(c);v=bytearray(base64.b64decode(d[key]));v[index]^=1;d[key]=base64.b64encode(v).decode()
 reject('network-bool',lambda c:c.__setitem__('networkForwards',False))
 reject('resource-foreign',lambda c:c.__setitem__('resourceSHA','0'*64))
 reject('fixture-foreign',lambda c:c.__setitem__('fixtureSHA','0'*64))
 for key in ['processedGraphAuthority','matcherRequestAuthority','workspaceAuthority']:reject(key+'-promotion',lambda c,k=key:c.__setitem__(k,True))
 reject('guard-delete',lambda c:c['guards'].pop());reject('guard-order',lambda c:c['guards'].reverse())
 reject('case-delete',lambda c:c['cases'].pop());reject('case-order',lambda c:c['cases'].reverse())
 reject('count-bool',lambda c:first(c).__setitem__('SegmentCount',False))
 reject('source-index-bool',lambda c:first(c)['Sampler']['OriginalIndices'].__setitem__(0,False))
 reject('source-coordinate-bool',lambda c:first(c)['Sampler']['PointBits'].__setitem__(0,False))
 reject('source-coordinate-foreign',lambda c:first(c)['Sampler']['PointBits'].__setitem__(0,0x46f12000))
 reject('range-bool',lambda c:first(c)['RangeTriples'].__setitem__(0,False))
 reject('range-inclusive-end',lambda c:first(c)['RangeTriples'].__setitem__(2,first(c)['RangeTriples'][2]+1))
 reject('membership-offset',lambda c:first(c)['OriginalOffsets'].__setitem__(-1,0))
 reject('required-word-count',lambda c:first(c).__setitem__('RequiredWorkWords',first(c)['RequiredWorkWords']+1))
 reject('mask-pointer-defined',lambda c:byte(c,'DefinedMasks',2))
 reject('opaque-placeholder',lambda c:byte(c,'RecordBytes',0x10))
 reject('candidate-count',lambda c:byte(c,'RecordBytes',8))
 reject('previous-end-flag',lambda c:byte(c,'RecordBytes',0x20))
 reject('bbox-corrupt',lambda c:byte(c,'RecordBytes',0x27))
 reject('centroid-corrupt',lambda c:byte(c,'RecordBytes',0x37))
 reject('invalid-base64',lambda c:first(c).__setitem__('RecordBytes','!'))
 for k in ['BridgeChargedBytes','BridgeChargedWork','DiagnosticChargedBytes','DiagnosticChargedWork']:reject(k+'-undercharge',lambda c,k=k:first(c).__setitem__(k,first(c)[k]-1))
 reject('retained-bool',lambda c:c.__setitem__('retainedDiagnosticBytes',False))
 return out

if __name__=='__main__':
 expected=ref.expected(Path(sys.argv[1]),Path(sys.argv[2]),Path(sys.argv[3]));prepared_only=sys.argv[4]=='--prepared-controls';capture=prepared(expected)if prepared_only else json.loads(Path(sys.argv[4]).read_text(encoding='utf-8-sig'))
 result=check(capture,expected);result.update(negativeControls=negatives(capture,expected),preparedNumericDataOnly=prepared_only,segmentControls=ref.controls());report=Path(sys.argv[5]);assert not report.exists();report.write_text(json.dumps(result,indent=2)+'\n',encoding='utf8');print(json.dumps(result))
