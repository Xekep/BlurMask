"""One exact CI134 InputSha256 provenance adapter; no data acceptance fallback.

Baseline is NEVER parsed/reserialized to manufacture authority. Only the known
64-byte input digest literal is replaced after exact source-byte hashes and the
historical RAW input's compact+CRLF byte identity have been proved. The final
ENTIRE baseline must still equal its original historical SHA256.
"""
import hashlib,json,math
HISTORICAL_BASELINE='d1ffe91e88cf7e48f7d68cda029790c2d73f9e8c9f9d8c5640890c3ff1c712d0'
KNOWN_CI_BASELINE='45dc92c8990c508e4ea7a95a2875cba94e74ed39205ca806a1a4a4f5637b2c4a'
HISTORICAL_RAW='d1c9284dbe4f79301f2a0d860e14b3c00e5a0ba02430b98a6bac5f0c43fa6b99'
KNOWN_CI_RAW='39c3dd68210744184928a1a187e7fc82248d17c9b477a1fdd9e70f400ff98469'
sha=lambda data:hashlib.sha256(data).hexdigest()
def _pairs(values):
 out={}
 for k,v in values:
  assert k not in out,'Duplicate raw input field';out[k]=v
 return out
def _invalid_constant(value):raise AssertionError('Nonfinite raw JSON constant')
def strict_json(raw):return json.loads(raw,object_pairs_hook=_pairs,parse_constant=_invalid_constant)
def typed_equal(a,b):
 if type(a) is not type(b):return False
 if isinstance(a,dict):return list(a)==list(b) and all(typed_equal(a[k],b[k]) for k in a)
 if isinstance(a,list):return len(a)==len(b) and all(typed_equal(x,y) for x,y in zip(a,b,strict=True))
 return a==b
def recover_historical_raw(source):
 assert type(source) is bytes and sha(source)==KNOWN_CI_RAW,'Only exact known CI raw source permitted'
 data=strict_json(source);assert type(data) is list and len(data)==141
 names=[]
 for row in data:
  assert type(row) is dict and list(row)==['Name','Raw','Resolution']
  assert type(row['Name']) is str;names.append(row['Name'])
  assert type(row['Resolution']) in (int,float) and math.isfinite(row['Resolution'])
  assert type(row['Raw']) is list and row['Raw']
  for stroke in row['Raw']:
   assert type(stroke) is list and stroke
   for point in stroke:
    assert type(point) is list and len(point)==2 and all(type(x) in (int,float) and math.isfinite(x) for x in point)
 assert len(set(names))==141
 #Explicit fixed historical RAW serialization, not baseline reserialization.
 historical=(json.dumps(data,separators=(',',':'))+'\r\n').encode('utf-8')
 assert len(historical)==30488 and sha(historical)==HISTORICAL_RAW,'Historical raw bytes must be recovered exactly'
 assert typed_equal(data,strict_json(historical)),'Full typed raw tree must be identical'
 return historical

def adapt_known_134(baseline,source):
 assert type(baseline) is bytes and len(baseline)==8737918 and sha(baseline)==KNOWN_CI_BASELINE,'Only exact known CI134 baseline permitted'
 recover_historical_raw(source)
 old=b'"InputSha256":"'+KNOWN_CI_RAW.encode('ascii')+b'"'
 new=b'"InputSha256":"'+HISTORICAL_RAW.encode('ascii')+b'"'
 assert baseline.count(old)==1 and baseline.count(KNOWN_CI_RAW.encode('ascii'))==1
 result=baseline.replace(old,new,1)
 assert len(result)==len(baseline) and sha(result)==HISTORICAL_BASELINE,'Entire historical baseline hash remains mandatory'
 return result
