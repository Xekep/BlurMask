import json,copy,sys
from pathlib import Path
EXPECTED={"Scope":"PREPARED_CACHE_NUMERIC_ONLY_NOT_OWNER_AUTHORITY","FreshHit":False,"AliasHit":True,"CallbackCalls":2,"ReadyOwners":2,"Guards":["foreign pool","marker key","duplicate","capacity","zero capacity","oversize capacity","malformed receipt","exact cancellation"],"LiveOwnerExecuted":False,"NN":False}
def strict(a,b):
 assert type(a)is type(b)
 if type(b)is dict:
  assert set(a)==set(b)
  for k in b:strict(a[k],b[k])
 elif type(b)is list:
  assert len(a)==len(b)
  for x,y in zip(a,b,strict=True):strict(x,y)
 else:assert a==b
x=json.loads(Path(sys.argv[1]).read_bytes());strict(x,EXPECTED)
tests=[lambda q:q.update(CallbackCalls=3),lambda q:q.update(CallbackCalls=True),lambda q:q.update(ReadyOwners=1),lambda q:q.update(Guards=q['Guards'][:-1]),lambda q:q.update(Guards=q['Guards'][::-1]),lambda q:q.update(LiveOwnerExecuted=True)]
for mutation in tests:
 q=copy.deepcopy(x);mutation(q)
 try:strict(q,EXPECTED)
 except AssertionError:continue
 raise AssertionError('Tamper accepted')
report=dict(Status='PASS_PREPARED_DISPATCH_ONLY',ExactGuards=8,TamperRefusals=len(tests),OwnerAuthorityNotTested=True,NN=False);Path(sys.argv[2]).write_bytes(json.dumps(report,indent=2).encode());print(json.dumps(report))
