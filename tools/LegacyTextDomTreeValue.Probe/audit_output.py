"""Owner-free value/tree equality and actual GC negative control, stdlib only."""
import base64,hashlib,json
from pathlib import Path
import sys
if len(sys.argv)!=2:raise SystemExit('capture.json')
d=json.loads(Path(sys.argv[1]).read_bytes())
assert d['valueScopeCollected'] is True and d['originalRetainsScope'] is True and d['guards']==11
assert len(d['rows'])==2
total=payload=0
def inspect(node):
 global total,payload
 assert set(node)=={'id','kind','name','nameId','tag','value','attributes','children'}
 assert isinstance(node['id'],int)and node['id']>0
 total+=1;payload+=len(base64.b64decode(node['value'],validate=True))
 for child in node['attributes']+node['children']:inspect(child)
for row in d['rows']:
 assert row['original']==row['value']
 assert row['value']['kind']==4 and len(row['value']['children'])==1
 inspect(row['value'])
assert total>50 and payload>100
def stable(x):
 if isinstance(x,list):return[stable(v)for v in x]
 if not isinstance(x,dict):return x
 result={k:stable(v)for k,v in x.items()}
 if result.get('name')=='gen':result['value']='LIBRARY-UUID'
 return result
digest=hashlib.sha256(json.dumps(stable(d),sort_keys=True,separators=(',',':')).encode()).hexdigest()
Path(str(sys.argv[1])+'.stable.sha256').write_text(digest+'\n',encoding='ascii')
print(f'PASS2 actual owner-free trees/{total}nodes/{payload}value bytes; exact metadata+payload equality, scope collectability/retaining control,11 guards; stableSHA={digest}')
