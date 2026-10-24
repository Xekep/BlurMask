"""Strict DATA comparison against independently regenerated raw-source sampler expectations.
CLI: DEPENDENCY_ROOT MODELS FIXTURES CAPTURE FRESH_REPORT
No C#, vendor library or neural network is executed.
"""
from pathlib import Path
import sys,json,copy,struct,math,importlib.util,hashlib
D=Path(__file__).resolve().parent
spec=importlib.util.spec_from_file_location('sampler_reference',D/'reference_sampler.py');ref=importlib.util.module_from_spec(spec);spec.loader.exec_module(ref)
ARRAYS=['BeforePointBits','BeforeOriginalIndices','BeforeEnds','PointBits','OriginalIndices','Ends','MarkerFields','RangeFields','MetricBits']
COSTS=['SamplerChargedBytes','SamplerChargedWork','DiagnosticChargedBytes','DiagnosticChargedWork']
def integer(v,lo=-(1<<31),hi=(1<<31)-1):assert type(v)is int and lo<=v<=hi

def bitsfloat(v):return struct.unpack('<f',struct.pack('<i',v))[0]

def comparison(actual,want,modes,ends,stride):
 assert len(actual)==len(want);start=0
 for mode,end in zip(modes,ends):
  for a,b in zip(actual[stride*start:stride*end],want[stride*start:stride*end]):
   if mode!='spline':assert a==b
   else:
    x,y=bitsfloat(a),bitsfloat(b);assert math.isfinite(x)and math.isfinite(y)and abs(x-y)<=5e-5*max(1,abs(y))
  start=end
 assert stride*start==len(actual)

def names(expected):
 n=['fresh-precancel-exact-token','null-source-refused']
 for e in expected['cases']:
  n.extend(prefix+e['name']for prefix in ['version-','same-source-shape-','current-counts-','marker-range-counts-','diagnostic-ledger-','sampler-ledger-'])
 return n

def check(capture,expected):
 assert type(capture)is dict and set(capture)=={'resourceSHA','fixtureSHA','cases','guards','retainedDiagnosticBytes','networkForwards','processedGraphAuthority','matcherRequestAuthority'}
 integer(capture['networkForwards'],0,0);assert capture['processedGraphAuthority']is False and capture['matcherRequestAuthority']is False
 assert capture['resourceSHA']==expected['resourceSHA']and capture['fixtureSHA']==expected['fixtureSHA']
 assert capture['guards']==names(expected)and all(type(x)is str for x in capture['guards'])
 assert type(capture['cases'])is list and len(capture['cases'])==14;retained=0
 for got,want in zip(capture['cases'],expected['cases']):
  assert type(got)is dict and set(got)=={'name','data'}and got['name']==want['name'];data=got['data']
  assert type(data)is dict and set(data)==set(ARRAYS+COSTS+['Version','HeightBits'])
  integer(data['Version'],1,1);integer(data['HeightBits']);assert data['HeightBits']==want['HeightBits']
  for k in ARRAYS:
   assert type(data[k])is list
   for v in data[k]:integer(v)
  for k in ['BeforeOriginalIndices','BeforeEnds','OriginalIndices','Ends','MarkerFields','RangeFields']:assert data[k]==want[k]
  comparison(data['BeforePointBits'],want['BeforePointBits'],want['upstreamModes'],want['BeforeEnds'],2)
  comparison(data['PointBits'],want['PointBits'],want['upstreamModes'],want['Ends'],2)
  comparison(data['MetricBits'],want['MetricBits'],want['upstreamModes'],list(range(1,len(want['Ends'])+1)),6)
  for k in COSTS:integer(data[k],0,(1<<63)-1)
  before=len(want['BeforeOriginalIndices']);after=len(want['OriginalIndices']);m=len(want['MarkerFields'])//3;s=len(want['Ends'])
  b=256+12*before+12*after+24*m+32*s;w=6*before+6*after+12*m+12*s
  assert data['DiagnosticChargedBytes']==b<=64*1024*1024 and data['DiagnosticChargedWork']==w<=64*1024*1024
  # Bounds only; complete sampler site accounting is source/AST evidence, not
  # falsely claimed reconstructed from final numerical outputs.
  assert data['SamplerChargedBytes']<=128*1024*1024 and data['SamplerChargedWork']<=1_000_000_000
  retained+=b
 integer(capture['retainedDiagnosticBytes'],0,128*1024*1024);assert capture['retainedDiagnosticBytes']==retained
 return dict(cases=14,guards=len(names(expected)),points=sum(len(x['OriginalIndices'])for x in expected['cases']),markers=sum(len(x['MarkerFields'])//3 for x in expected['cases']),networkForwards=0,scope='Full numerical fields checked; inherited13A0 spline/transcendental coordinate tolerance only, discrete maps/markers/ranges/counts exact. Sampler site totals bounded, not independently reconstructed.')

def negatives(capture,expected):
 out=[]
 def reject(name,edit):
  c=copy.deepcopy(capture);edit(c)
  try:check(c,expected)
  except(AssertionError,KeyError,TypeError,ValueError):out.append(name)
  else:raise AssertionError('Accepted corruption '+name)
 first=lambda c:c['cases'][0]['data']
 reject('network-bool',lambda c:c.__setitem__('networkForwards',False))
 reject('network-one',lambda c:c.__setitem__('networkForwards',1))
 reject('resource-foreign',lambda c:c.__setitem__('resourceSHA','0'*64))
 reject('fixture-foreign',lambda c:c.__setitem__('fixtureSHA','0'*64))
 reject('graph-promotion',lambda c:c.__setitem__('processedGraphAuthority',True))
 reject('request-promotion',lambda c:c.__setitem__('matcherRequestAuthority',True))
 reject('guard-delete',lambda c:c['guards'].pop())
 reject('guard-order',lambda c:c['guards'].reverse())
 reject('case-delete',lambda c:c['cases'].pop())
 reject('case-order',lambda c:c['cases'].reverse())
 reject('coordinate-bool',lambda c:first(c)['PointBits'].__setitem__(0,False))
 reject('coordinate-corrupt',lambda c:first(c)['PointBits'].__setitem__(0,struct.unpack('<i',struct.pack('<f',12345.))[0]))
 reject('before-coordinate-corrupt',lambda c:first(c)['BeforePointBits'].__setitem__(0,0))
 reject('source-key-bool',lambda c:first(c)['OriginalIndices'].__setitem__(0,False))
 reject('source-key-foreign',lambda c:first(c)['OriginalIndices'].__setitem__(0,16383))
 reject('end-count',lambda c:first(c)['Ends'].__setitem__(0,0))
 reject('marker-tag',lambda c:first(c)['MarkerFields'].__setitem__(0,2))
 reject('range-end',lambda c:first(c)['RangeFields'].__setitem__(2,0))
 reject('metric-corrupt',lambda c:first(c)['MetricBits'].__setitem__(0,0))
 reject('height-bool',lambda c:first(c).__setitem__('HeightBits',False))
 reject('diagnostic-byte-undercharge',lambda c:first(c).__setitem__('DiagnosticChargedBytes',first(c)['DiagnosticChargedBytes']-1))
 reject('diagnostic-work-undercharge',lambda c:first(c).__setitem__('DiagnosticChargedWork',first(c)['DiagnosticChargedWork']-1))
 reject('sampler-byte-overrun',lambda c:first(c).__setitem__('SamplerChargedBytes',128*1024*1024+1))
 reject('sampler-work-overrun',lambda c:first(c).__setitem__('SamplerChargedWork',1_000_000_001))
 reject('retained-bool',lambda c:c.__setitem__('retainedDiagnosticBytes',False))
 return out

def prepared(expected):
 # Ordinary numerical DATA only, explicitly not evidence of owning execution.
 cases=[];retained=0
 for e in expected['cases']:
  b=len(e['BeforeOriginalIndices']);a=len(e['OriginalIndices']);m=len(e['MarkerFields'])//3;s=len(e['Ends']);bytes_=256+12*b+12*a+24*m+32*s;work=6*b+6*a+12*m+12*s
  data={k:e[k]for k in ARRAYS+['HeightBits']};data.update(Version=1,SamplerChargedBytes=0,SamplerChargedWork=0,DiagnosticChargedBytes=bytes_,DiagnosticChargedWork=work);cases.append(dict(name=e['name'],data=data));retained+=bytes_
 return dict(resourceSHA=expected['resourceSHA'],fixtureSHA=expected['fixtureSHA'],cases=cases,guards=names(expected),retainedDiagnosticBytes=retained,networkForwards=0,processedGraphAuthority=False,matcherRequestAuthority=False)

if __name__=='__main__':
 expected=ref.main(Path(sys.argv[1]),Path(sys.argv[2]),Path(sys.argv[3]));capture=json.loads(Path(sys.argv[4]).read_text(encoding='utf-8-sig'))if sys.argv[4]!='--prepared-controls'else prepared(expected)
 result=check(capture,expected);result.update(negativeControls=negatives(capture,expected),preparedNumericDataOnly=sys.argv[4]=='--prepared-controls',heapEquivalentControls=expected['heapEquivalentControls'],limiterExceptionalControls=expected['limiterExceptionalControls']);report=Path(sys.argv[5]);assert not report.exists();report.write_text(json.dumps(result,indent=2)+'\n',encoding='utf8');print(json.dumps(result))
