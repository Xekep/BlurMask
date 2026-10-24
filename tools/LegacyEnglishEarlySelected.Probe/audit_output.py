import base64,copy,json,struct,sys
from pathlib import Path
NS='http://www.myscript.com/ns/myscript/page'
counts_cases=[[1],[3],[1,2],[2,1,3],[3]*16,[1]*256,[4096],[16]*256]
guards=['foreign-chosen-input','original-once','incomplete-binding','selected-once','no-fields','duplicate-fields','invalid-field-path','raw-object-bound','raw-point-bound','view-prepare-cancel','original-prepare-cancel','selected-prepare-cancel','original-preentry-cancel','selected-preentry-cancel']
def b64(b):return base64.b64encode(b).decode()
def nodes(selected):
 names=['','page','','version','content','layout','textField','id','input','tag'];kinds=[4,3,2,0,3,3,3,0,3,0]
 parent=[0,1,2,2,2,2,5,7,7,9];children={1:[2],2:[3,5,6],5:[7],7:[9]};attrs={2:[4],7:[8],9:[10]};out=[]
 for i in range(1,11):
  value=struct.pack('<i',5)if i==4 else b'english-field'if i==8 else struct.pack('<q',0)if i==10 else b''
  row=dict(Identity=i,Name=names[i-1],NativeValueTag=7 if i==4 else 2 if i==8 else 4 if i==10 else 0,
   Value=b64(value),ParentIdentity=parent[i-1],Children=children.get(i,[]),Attributes=attrs.get(i,[]),Namespace=NS if i==3 else None,Prefix=''if i==3 else None)
  if selected:row['OriginalNodeIdentity']=i if i>=7 else 0
  else:row['Kind']=kinds[i-1]
  out.append(row)
 return out
def expected():
 out=[]
 for counts in counts_cases:
  n=len(counts);first=3|(0xff00 if counts[0]==1 else 0);last=((n-1)<<32)|((counts[-1]-1)<<16)|0xff05
  raw=[b64(b''.join(struct.pack('<ff',float(s*100+i),float(i*2))for i in range(k)))for s,k in enumerate(counts)]
  out.append(dict(Counts=counts,OriginalAttempts=19,SelectedAttempts=17+n,OriginalNext=1,SelectedNext=1,InputId=0,PendingNext=1,PendingCount=0,
   OriginalHistory=5,OriginalObservers=10,OriginalTrace=33,SelectedTrace=27+n,Raw=raw,Original=nodes(False),Selected=nodes(True),
   Registry=[dict(RegistryId=0,Name='INPUT',Category=100,Pairs=[first,last])],SameDocument=True,SameCurrentView=True,NamesHaveDistinctOwners=True))
 types=['ArgumentException','InvalidOperationException','NotSupportedException','InvalidOperationException','ArgumentException','NotSupportedException','NotSupportedException','ArgumentException','ArgumentException']+['OperationCanceledException']*5
 proofs=[dict(Name=n,ExceptionType='System.'+t,Reached=True,ExpectedTokenSupplied=i>=9,TokenMatches=i>=9,TokenRequested=i>=9)for i,(n,t)in enumerate(zip(guards,types,strict=True))]
 return dict(Scope='MODEL_FREE_REAL_BOOTSTRAP_HELPERS_NOT_EN_GRAPH_ISSUER',Cases=out,Guards=guards,GuardProofs=proofs,DefensiveCopyChecks=16,BootstrapHelpersExecuted=True,ENOwningExecuted=False,NN=False,DocumentApplied=False)
def strict(a,b):
 assert type(a)is type(b),(type(a),type(b))
 if isinstance(b,dict):
  assert a.keys()==b.keys(),(a.keys(),b.keys())
  for k in b:strict(a[k],b[k])
 elif isinstance(b,list):
  assert len(a)==len(b)
  for x,y in zip(a,b,strict=True):strict(x,y)
 else:assert a==b,(a,b)
want=expected();x=json.loads(Path(sys.argv[1]).read_bytes());strict(x,want)
changes=[]
for k,v in [('OriginalAttempts',18),('SelectedAttempts',17),('OriginalNext',True),('SelectedNext',2),('InputId',1),('PendingNext',2),('PendingCount',1),('OriginalHistory',4),('OriginalObservers',9),('OriginalTrace',32),('SelectedTrace',27),('SameDocument',False),('SameCurrentView',False),('NamesHaveDistinctOwners',False)]:changes.append((k,lambda q,k=k,v=v:q['Cases'][0].update({k:v})))
changes.append(('selected-origin',lambda q:q['Cases'][0]['Selected'][6].update(OriginalNodeIdentity=0)))
changes.append(('old-tag-value',lambda q:q['Cases'][0]['Original'][9].update(Value=b64(struct.pack('<q',1)))))
changes.append(('copied-raw',lambda q:q['Cases'][0].update(Raw=[''])))
changes.append(('registry-id',lambda q:q['Cases'][0]['Registry'][0].update(RegistryId=1)))
changes.append(('missing-case',lambda q:q['Cases'].pop()))
changes.append(('extra-case',lambda q:q['Cases'].append(copy.deepcopy(q['Cases'][0]))))
changes.append(('reorder-guards',lambda q:q['Guards'].reverse()))
changes.append(('claim-NN',lambda q:q.update(NN=True)))
changes.append(('claim-apply',lambda q:q.update(DocumentApplied=True)))
changes.append(('claim-EN-owner',lambda q:q.update(ENOwningExecuted=True)))
changes.append(('wrong-exception-type',lambda q:q['GuardProofs'][0].update(ExceptionType='System.Exception')))
changes.append(('unreached',lambda q:q['GuardProofs'][0].update(Reached=False)))
changes.append(('missing-guard-proof',lambda q:q['GuardProofs'].pop()))
for key in ['ExpectedTokenSupplied','TokenMatches','TokenRequested']:
 changes.append(('cancel-'+key,lambda q,k=key:q['GuardProofs'][9].update({k:False})))
names=[]
for name,mut in changes:
 q=copy.deepcopy(x);mut(q)
 try:strict(q,want)
 except AssertionError:names.append(name)
 else:raise AssertionError('accepted '+name)
report=dict(Passed=True,RealModelFreeHelperCases=8,DOMRows=160,RawBytes=sum(sum(c)*8 for c in counts_cases),ExactHelperRefusals=len(guards),DefensiveCopies=16,TamperRefused=names,NN=False,ENOwningExecuted=False)
Path(sys.argv[2]).write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
