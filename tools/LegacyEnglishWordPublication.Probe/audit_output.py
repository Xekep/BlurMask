import base64,copy,json,struct,sys
from pathlib import Path
values=[0,1,0x100000,0x200000,0x800000,0x1000000,0x80000000,0xffffffff]
def b64(b):return base64.b64encode(b).decode()
def strict(a,b):
 assert type(a)is type(b)
 if type(a)is dict:
  assert set(a)==set(b)
  for k in b:strict(a[k],b[k])
 elif type(a)is list:
  assert len(a)==len(b)
  for x,y in zip(a,b,strict=True):strict(x,y)
 else:assert a==b
folds=[]
for prior in values:
 for default in values:
  for marker in values:
   result=prior|default
   if marker&0x100000:result|=0x100000
   folds.append(dict(Prior=prior,Default=default,Marker=marker,Result=result))
rows=[]
for count in [0,1,2,128,4096]:
 for flags in values:
  b=bytearray(48);struct.pack_into('<I',b,8,flags);struct.pack_into('<ii',b,24,count,count)
  rows.append(dict(Count=count,Flags=flags,Scalar48=b64(b)))
tree=[dict(Identity=1,Left=0,Right=0,Balance=0,PreviousInserted=0,NextInserted=0)]
expected=dict(Scope='PREPARED_NEUTRAL_WORD_SCALARS_REGISTRATION_NOT_LIVE_EN',Folds=folds,WordFlags=[dict(Prior=p,A=a,B=b,Result=p|a|b)for p in values for a in values for b in values],Rows=rows,CharacterNodeIds=[1,1],WordNodeId=1,CharTree=tree,WordTree=tree,Pending=[dict(SelectionId=1,Name='CHAR',CommandIdentity=1,PreviousCommandIdentity=0,Pairs=[3,196357]),dict(SelectionId=2,Name='WORD',CommandIdentity=2,PreviousCommandIdentity=1,Pairs=[3,196357])],Trace=['word-intern','word-choice48:0','char-intern:new','word-choice-complete:0','word-choice48:1','char-intern:reuse','word-choice-complete:1','word-register'],Next=3,GuardRefusals=4,NN=False,LiveOwnerExecuted=False,TEXTCreated=False)
x=json.loads(Path(sys.argv[1]).read_bytes());strict(x,expected)
mutations=[lambda a:a['WordFlags'][8].update(Result=a['WordFlags'][8]['Result']^1),lambda a:a['WordFlags'].pop(),lambda a:a.update(Scope='ACTUAL'),lambda a:a.update(NN=True),lambda a:a.update(LiveOwnerExecuted=True),lambda a:a.update(TEXTCreated=True),lambda a:a.update(Next=True),lambda a:a.update(Next=3.0),lambda a:a['Folds'][1].update(Result=a['Folds'][1]['Result']^1),lambda a:a['Rows'][0].update(Scalar48=b64(bytes([1])+bytes(47))),lambda a:a.update(CharacterNodeIds=[1,2]),lambda a:a['Pending'][1].update(SelectionId=3),lambda a:a['Pending'][1].update(Name='TEXT'),lambda a:a['Pending'][1].update(Pairs=[0,196357]),lambda a:a['CharTree'][0].update(Left=1),lambda a:a['Trace'].reverse(),lambda a:a['Trace'].pop(),lambda a:a['Folds'].pop()]
for mutate in mutations:
 y=copy.deepcopy(x);mutate(y)
 try:strict(y,expected)
 except AssertionError:pass
 else:raise AssertionError('Mutation accepted')
r=dict(Status='PASS_NEUTRAL_PREPARED_WORD_SCALARS_COUNTERS_ONLY',FoldCases=len(folds),AllChoiceFlagCases=512,Full48Rows=len(rows),ScalarBytes=len(rows)*48,SharedCharacterKeyHits=1,SeparateWordTree=True,PendingCommands=2,CounterNext=3,TamperRefusals=len(mutations),NN=False,LiveAll275OwnerExecuted=False,TEXT=False)
out=Path(sys.argv[2]);assert not out.exists();out.write_bytes((json.dumps(r,indent=2)+'\n').encode());print(r)
