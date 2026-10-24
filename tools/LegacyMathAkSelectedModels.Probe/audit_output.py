import sys,json,hashlib,struct,copy
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]/'handwriting'))
from math_legacy_mixed107_reference import ACTUAL,parse_envelope,RESOURCE_SHA
GUARDS=['prepare-pre-cancel-token','bind-pre-cancel-token','null-source-refused','all-bindings-same-owner','equal-data-foreign-features-refused-before-forward','all-fifteen-weight-array-identities-stable','descriptor-view-not-array','descriptor-view-readonly','foreign-resource-refused','caller-clear-does-not-replace-arrays','parallel-immutable-inspection']
def check(repo,c):
 data=(repo/'models/math/math-ak.res').read_bytes();assert hashlib.sha256(data).hexdigest()==RESOURCE_SHA
 assert c['modelExecutions']==0 and c['vendorExecution'] is False and c['bindingCount']==256 and c['guards']==GUARDS
 assert len(c['models'])==5
 retained=0
 for row,(start,end,kind,width,parent) in zip(c['models'],ACTUAL[1:]):
  m=parse_envelope(data[start:end],start);a,b=m.matrices
  assert row['OriginalStart']==start and row['Kind']==kind and row['InputWidth']==width
  assert (row['FirstStride'],row['SecondStride'])==(a.stride,b.stride)
  payload=[a.weights,b.weights,b.scales]
  assert row['hashes']==[hashlib.sha256(x).hexdigest() for x in payload]
  assert row['lengths']==[len(a.weights)//2,len(b.weights),len(b.scales)]
  first=struct.unpack('<'+'h'*(len(a.weights)//2),a.weights)
  assert all(first[r*a.stride+j]==0 for r in range(a.rows) for j in range(a.columns,a.stride))
  assert all(b.weights[r*b.stride+j]==0 for r in range(b.rows) for j in range(b.columns,b.stride))
  retained+=sum(map(len,payload))
 assert retained==c['RetainedWeightBytes']==761370
 return retained
repo=Path(sys.argv[1]);capture=Path(sys.argv[2]);c=json.loads(capture.read_text());retained=check(repo,c)
neg=0
for field in ['hashes','lengths','OriginalStart','Kind','InputWidth','FirstStride','SecondStride']:
 bad=copy.deepcopy(c)
 if isinstance(bad['models'][0][field],list):bad['models'][0][field][0]='bad' if field=='hashes' else -1
 else:bad['models'][0][field]=-1
 try:check(repo,bad)
 except (AssertionError,TypeError):neg+=1
 else:raise AssertionError(field)
print(json.dumps({'models':5,'matrices':10,'weightBytes':retained,'guards':len(GUARDS),'tamperControls':neg,'modelExecutions':0},sort_keys=True))
