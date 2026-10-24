import base64,copy,json,struct,sys
from pathlib import Path

def b(x):return base64.b64encode(bytes(x)).decode()
def expected():
 stores=[]
 for seed in range(64):
  for marker in [-2147483648,-65536,-1,0,1,127,128,4095,65535,2147483647]:
   before=bytes((seed*13+i*17)&255 for i in range(8));known=bytes(255 if seed>>i&1 else 0 for i in range(8))
   stores.append(dict(Seed=seed,Index=marker,Before=b(before),KnownBefore=b(known),After=b(before[:4]+struct.pack('<i',marker)),KnownAfter=b(known[:4]+b'\xff'*4)))
 order=[]
 for ids in [[1,0,1,1],[0,0,0],[2,1,0],[]]:
  flags=[0,0x01004000,0x10000000];values=[-1]*3;marks=[-1 if i==1 else i+7 for i in range(len(ids))]
  for i,m in zip(ids,marks,strict=True):flags[i]|=0x08000000;values[i]=m
  order.append(dict(Ids=ids,Markers=marks,Flags=flags,Indices=values))
 folds=[]
 for seed in range(128):
  prior=seed*0x0010101;meta=[0x11004000 if seed>>i&1 else 0 for i in range(3)]
  folds.append(dict(Prior=prior,Metadata=meta,Actual=(prior&0xfeffffff)|(0x01000000 if all(v&0x11004000 for v in meta)else 0)))
 snap=[dict(Count=n,ConcreteType=0xc49,SourceIndex=0,ViewIndex=0,FirstPoint=0,LastPoint=n-1,FirstFraction=0,LastFraction=0,FirstClosed=True,LastClosed=True,IsKnownText=False,OriginalDOM=10,SelectedDOM=10,OriginalNext=1,SelectedNext=1,NoGraphParentIssued=True,ReportedKindQueried=False)for n in [3,4]]
 return dict(Scope='MODEL_FREE_DWORD44_SHARED_SCALARS_REAL_BOOTSTRAP_SNAPSHOTS_NOT_GRAPH_OWNER',Stores=stores,SharedNumericOrders=order,SharedNumericFolds=folds,RawSnapshots=snap,DefensiveCopies=4,Refusals=['field-short','field-long','mask-short','mask-long','same-storage','overlap-storage'],ChronologyIL=dict(Scope='READONLY_OWN_LIBRARY_IL_NOT_PRIVATE_METHOD_EXECUTION',IndexWriteCalls=2,WordFlagsStores=2,WordMetadataBeforeCharLoop=True,FirstIndexInsideCharBackedge=True,WordOrAndIndexAfterCharBackedge=True,CompleteSelectedSourcesInvoked=False,GraphParentConstructed=False),ENOwningExecuted=False,NN=False,DocumentApplied=False)
def strict(a,b):
 assert type(a)is type(b),(type(a),type(b))
 if isinstance(b,dict):
  assert a.keys()==b.keys()
  for k in b:strict(a[k],b[k])
 elif isinstance(b,list):
  assert len(a)==len(b)
  for x,y in zip(a,b,strict=True):strict(x,y)
 else:assert a==b,(a,b)
x=json.loads(Path(sys.argv[1]).read_bytes());want=expected();strict(x,want)
changes=[('wrong-word-order',lambda q:q['ChronologyIL'].update(WordOrAndIndexAfterCharBackedge=False)),('wrong-fold-order',lambda q:q['ChronologyIL'].update(WordMetadataBeforeCharLoop=False)),('claim-private-execution',lambda q:q['ChronologyIL'].update(CompleteSelectedSourcesInvoked=True)),('lower-data',lambda q:q['Stores'][0].update(After=b(bytes(8)))),('promote-lower-mask',lambda q:q['Stores'][0].update(KnownAfter=b(b'\xff'*8))),('upper-mask',lambda q:q['Stores'][0].update(KnownAfter=b(bytes(8)))),('missing-store',lambda q:q['Stores'].pop()),('duplicate-store',lambda q:q['Stores'].append(q['Stores'][0])),('wrong-index-type',lambda q:q['Stores'][0].update(Index=False)),('last-marker',lambda q:q['SharedNumericOrders'][0]['Indices'].__setitem__(1,7)),('wrong-word-all',lambda q:q['SharedNumericFolds'][7].update(Actual=0)),('raw-kind',lambda q:q['RawSnapshots'][1].update(ConcreteType=0xc25)),('fake-reported-query',lambda q:q['RawSnapshots'][1].update(ReportedKindQueried=True)),('snapshot-copy',lambda q:q.update(DefensiveCopies=0)),('missing-refusal',lambda q:q['Refusals'].pop()),('fake-graph-owner',lambda q:q.update(ENOwningExecuted=True)),('NN',lambda q:q.update(NN=True)),('apply',lambda q:q.update(DocumentApplied=True))]
refused=[]
for name,change in changes:
 q=copy.deepcopy(x);change(q)
 try:strict(q,want)
 except AssertionError:refused.append(name)
 else:raise AssertionError(name)
report=dict(Passed=True,ActualCSharpIndexStores=640,SharedNumericOrderCases=4,SharedNumericWordFolds=128,RealBootstrapSnapshots=2,DefensiveCopies=4,ExactRefusals=6,TamperRefused=refused,ActualPrivateBodyILClassified=True,PrivateGraphOwnerExercised=False,ReportedKindLookupExercised=False,NN=False)
p=Path(sys.argv[2]);assert not p.exists();p.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
