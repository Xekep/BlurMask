import copy,json,sys
from pathlib import Path
D=Path(__file__).resolve().parent
def flat(a):return [x for r in a for x in r]
controls=json.loads(Path(sys.argv[1]).read_bytes())['Cases'];x=json.loads(Path(sys.argv[2]).read_bytes())
want=dict(Scope='PREPARED_C21_POSTTEXT_NUMERIC_NO_OWNING',Cases=[dict(Union=flat(c['Union']),Remaining=flat(c['Remaining']),Partial=flat(c['Partial']),MultipleChoiceRemovalRequired=c['MultipleChoiceRemovalRequired'],SingleChoiceKept=c['SingleChoiceKept'])for c in controls],RequestedTokenGuards=3*len(controls),NN=False,OwningExecuted=False,DocumentApplied=False)
def strict(a,b):
 assert type(a)is type(b)
 if isinstance(b,dict):
  assert a.keys()==b.keys()
  for k in b:strict(a[k],b[k])
 elif isinstance(b,list):
  assert len(a)==len(b)
  for p,q in zip(a,b,strict=True):strict(p,q)
 else:assert a==b
strict(x,want);negatives=0
for name,value in [('Cases',[]),('RequestedTokenGuards',0),('NN',True),('DocumentApplied',True),('OwningExecuted',True)]:
 q=copy.deepcopy(x);q[name]=value
 try:strict(q,want)
 except AssertionError:negatives+=1
 else:raise AssertionError(name)
for name,value in [('Union',[]),('Remaining',[3]),('Partial',[]),('MultipleChoiceRemovalRequired',False),('SingleChoiceKept',False)]:
 q=copy.deepcopy(x);q['Cases'][0][name]=value
 try:strict(q,want)
 except AssertionError:negatives+=1
 else:raise AssertionError(name)
out=dict(Passed=True,PureCases=len(controls),ExactRequestedTokenGuards=3*len(controls),TamperRefused=negatives,NN=False,OwningExecuted=False,DocumentApplied=False)
Path(sys.argv[3]).write_text(json.dumps(out,indent=2)+'\n',encoding='utf-8');print(json.dumps(out))
