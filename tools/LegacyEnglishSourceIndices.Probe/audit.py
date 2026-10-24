"""DATA only: independent pinned C21 linear union/recursive-height AVL replay."""
from pathlib import Path
import ast,json,hashlib,base64,copy,sys,math
D=Path(__file__).resolve().parent
PRIOR_SHA='530dfb0a0d59b18d23e51b7a89677ba1ab6cb237b3ea07550bb36d08d1828818'
SCOPE='SAME_EN_PRIVATE_POST020000_020920_NO_DOCUMENT_APPLY_NO_FRONTEND_C20_OR_GUIDE_CLAIM'
def h(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load_defs(name,extra):
 p=D/'dependencies'/name
 pins=json.loads((D/'dependency-pins.json').read_bytes());pin=next(x for x in pins if Path(x['Path']).name==name)
 assert h(p)==pin['Sha256']and p.stat().st_size==pin['Bytes']
 ns=dict(extra);tree=ast.parse(p.read_text(encoding='utf-8-sig'))
 nodes=[n for n in tree.body if isinstance(n,(ast.ClassDef,ast.FunctionDef))]
 exec(compile(ast.Module(body=nodes,type_ignores=[]),str(p),'exec'),ns);return ns
c21=load_defs('c21.py',{'MASK':2**64-1});avl=load_defs('avl.py',{})
def pairs(a):
 assert type(a)is list and len(a)%2==0 and len(a)<=131072
 assert all(type(x)is int and 0<=x<2**64 for x in a)
 return [a[i:i+2]for i in range(0,len(a),2)]
def flatten(a):return [v for r in a for v in r]
def integer(x,lo=0,hi=2**63-1):assert type(x)is int and lo<=x<=hi;return x
def equal(a,b):
 assert type(a)is type(b),(type(a),type(b))
 if type(b)is dict:
  assert a.keys()==b.keys(),(a.keys(),b.keys())
  for k in b:equal(a[k],b[k])
 elif type(b)is list:
  assert len(a)==len(b)
  for x,y in zip(a,b,strict=True):equal(x,y)
 else:assert a==b,(a,b)
def strict_tree(x):
 if type(x)is float:assert math.isfinite(x)
 elif type(x)is int:assert -2**63<=x<2**64
 elif type(x)is dict:
  for y in x.values():strict_tree(y)
 elif type(x)is list:
  for y in x:strict_tree(y)
 else:assert x is None or type(x)in(str,bool)
def read(p,pin=None):
 p=Path(p);assert p.stat().st_size<=32*1024*1024
 if pin:assert h(p)==pin
 def obj(kv):
  out={}
  for k,v in kv:assert k not in out;out[k]=v
  return out
 def bad(x):raise AssertionError('Nonfinite JSON')
 data=json.loads(p.read_bytes(),object_pairs_hook=obj,parse_constant=bad);strict_tree(data);return data

def index(sequence):
 raw=avl['replay']([(s['SourceNode'],pairs(s['Pairs']))for s in sequence])[0]
 def rows(values,vector):
  out=[]
  for x in values:
   i,page,slot,left,right,balance,head,source=x['fields']
   out.append(dict(Identity=i,Page=page,Slot=slot,Left=left,Right=right,Balance=balance,MembershipHead=head,CurrentSource=source,CachedCount=0 if vector else None,Pairs=flatten(x['key'])))
  return out
 return dict(IntervalRoot=raw['intervalRoot'],VectorRoot=raw['vectorRoot'],Dirty=raw['dirty'],SourcesAdded=raw['added'],Intervals=rows(raw['intervals'],False),Vectors=rows(raw['vectors'],True),Memberships=[dict(Identity=i,Page=p,Slot=s,SourceNode=source,Next=n)for i,p,s,source,n in raw['members']])

def expected(prior):
 selected=prior['SelectedSources'];word=prior['Word'];tree=selected['Tree'];n=len(tree)
 assert 1<=n<=4096 and len(selected['Characters'])==n
 shape={integer(x['Identity'],1,n):x for x in tree};assert len(shape)==n
 meta={integer(x['SourceNode'],1,n):x for x in selected['Characters']};assert len(meta)==n
 payload={integer(x['SourceNode'],1,n):x for x in word['FinalCharacters']};assert len(payload)==n
 start=[i for i,x in shape.items()if x['PreviousInserted']==0];assert len(start)==1
 order=[];cur=start[0];previous=0
 while cur:
  assert cur in shape and cur not in order
  row=shape[cur];assert row['PreviousInserted']==previous
  order.append(cur);previous=cur;cur=integer(row['NextInserted'],0,n)
 assert len(order)==n
 chars=[]
 for i in order:
  m,p=meta[i],payload[i];flags=integer(m['Flags'],0,2**32-1)
  assert base64.b64decode(m['FlagsKnown4'],validate=True)==b'\xff'*4
  assert integer(p['Written'],0,127)&4 #real EmptyPriorVector assignment
  assert p['AssignedDefaults']['Field40']==-1
  key=pairs(p['SourcePairs'])
  if not flags&0x200000:chars.append(dict(SourceNode=i,Flags=flags,HeaderOffset=32,Pairs=flatten(key)))
 aggregate=[]
 for ordinal,choice in enumerate(word['Choices']):
  assert choice['Ordinal']==ordinal and len(choice['Links'])==len(choice['Characters'])
  local=[]
  for link,source in zip(choice['Links'],choice['Characters'],strict=True):
   i=integer(link['SourceNode'],1,n);assert i==source['SourceNode']and link['Marker']==source['Marker']
   assert type(source['Ordinary'])is bool
   #Compiled append rejects a different canonical key; derive from original
   #per-reference source observations, never captured SourceIndices channels.
   key=pairs(source['SourcePairs']);assert key==pairs(payload[i]['SourcePairs'])
   if source['Ordinary']:
    local=c21['union'](local,key);aggregate=c21['union'](aggregate,key)
  equal(flatten(local),choice['OrdinaryPairs'])
 flags=integer(selected['WordFlags'],0,2**32-1);words=[]
 assert base64.b64decode(selected['WordFlagsKnown4'],validate=True)==b'\xff'*4
 if not flags&0x200000:
  key=aggregate or pairs(word['OriginalPairs'])
  words=[dict(SourceNode=integer(word['SourceNode'],1,65536),Flags=flags,HeaderOffset=48 if aggregate else 32,Pairs=flatten(key))]
 return dict(Characters=chars,Words=words,CharacterIndex=index(chars),WordIndex=index(words),PendingNext=integer(selected['PendingNext']),PendingCount=integer(selected['PendingCount']),Ready=True,DocumentApplied=False,Scope=SCOPE)

def audit_actual(actual,prior):
 assert type(actual)is dict and actual.keys()==prior.keys()|{'SourceIndices'}
 for k in prior:equal(actual[k],prior[k])
 wanted=expected(prior);equal(actual['SourceIndices'],wanted)
 return dict(CharacterSources=len(wanted['Characters']),WordSources=len(wanted['Words']),IntervalNodes=sum(len(wanted[k]['Intervals'])for k in('CharacterIndex','WordIndex')),VectorNodes=sum(len(wanted[k]['Vectors'])for k in('CharacterIndex','WordIndex')),Memberships=sum(len(wanted[k]['Memberships'])for k in('CharacterIndex','WordIndex')),WholePriorMatched=True)

def cost(t,a):
 assert type(t)is int and type(a)is int and 0<=t<=65536 and 0<=a<=65536
 m=t+a;arrays=128+16*t+80*m
 return dict(T=t,A=a,Payload=64+arrays,Copies=128+32*t+16*a+64*m+48*a*m,Visits=256+128*(a+1)*(m+1),Arrays=arrays)
def audit_prepared(actual):
 #Derived from fixed neutral inputs/source operations, not compiled capture.
 key0=[[3,(2<<16)|0xff05]];key2=[[(2<<32)|3,(2<<32)|(2<<16)|0xff05]]
 parent=[];unions=[]
 for key in[key0,key2,key0]:
  before=copy.deepcopy(parent);bound=cost(len(parent),len(key));parent=c21['union'](parent,key)
  unions.append(dict(Before=flatten(before),Additional=flatten(key),After=flatten(parent),**{k:v for k,v in bound.items()if k not in('T','A')}))
 wanted=dict(Scope='REAL_NEUTRAL_C21_UNION_AND_SOURCE_INDEX_KERNEL_NO_OWNING',Unions=unions,DuplicateMemberships=65537,DuplicateIntervals=1,DuplicateVectors=1,LatestOwnerMatched=True,OrderedVectors=2,EmptyVectorSourceMatched=True,Costs=[cost(t,a)for t,a in[(0,0),(0,1),(1,1),(65535,1),(65536,0),(4096,4096)]],Refusals=5,CacheZeroRuntimeFieldObserved=False,CacheZeroContract='SOURCE_AND_NATIVE_V78_COMPLETED_ADD_NO_CACHE_FILL_NOT_A_RUNTIME_CACHE_FIELD_TEST',OwnerCalls=0,ModelsLoaded=0,NN=0,DocumentApplied=False)
 equal(actual,wanted);return dict(Unions=3,CostBoundaries=6,DuplicateMemberships=65537,PreparedFullTreesExported=False,CacheZeroRuntimeFieldObserved=False)

def mutate_leaves(x,path=()):
 if type(x)is dict:
  for k,v in x.items():yield from mutate_leaves(v,path+(k,))
 elif type(x)is list:
  for i,v in enumerate(x):yield from mutate_leaves(v,path+(i,))
 else:yield path,x

def tamper(data,check,limit=64):
 count=0
 for path,value in mutate_leaves(data):
  if count==limit:break
  x=copy.deepcopy(data);t=x
  for k in path[:-1]:t=t[k]
  t[path[-1]]=not value if type(value)is bool else value+1 if type(value)in(int,float)else 'CORRUPTED' if type(value)is str else 1
  try:check(x)
  except(AssertionError,KeyError,TypeError,ValueError,IndexError):count+=1;continue
  raise AssertionError(('Tamper accepted',path))
 return count

def main():
 mode=sys.argv[1]
 if mode=='--prepared':
  actual=read(sys.argv[2]);details=audit_prepared(actual);negative=tamper(actual,audit_prepared)if '--tamper'in sys.argv else 0;report=Path(sys.argv[3])
 elif mode in('--actual','--self-test'):
  prior=read(sys.argv[2],PRIOR_SHA)
  if mode=='--actual':actual=read(sys.argv[3]);report=Path(sys.argv[4]);synthetic=False
  else:actual=copy.deepcopy(prior);actual['SourceIndices']=expected(prior);report=Path(sys.argv[3]);synthetic=True
  details=audit_actual(actual,prior)
  #New-image corruptions plus old-channel corruption coverage are separate.
  def check_new(x):z=copy.deepcopy(prior);z['SourceIndices']=x;return audit_actual(z,prior)
  negative=tamper(actual['SourceIndices'],check_new,128)if '--tamper'in sys.argv else 0
  if '--tamper'in sys.argv:
   for key in ['DocumentApplied','Metrics','Word','SelectedSources']:
    z=copy.deepcopy(actual)
    if type(z[key])is bool:z[key]=not z[key]
    else:z[key]={}
    try:audit_actual(z,prior)
    except(AssertionError,KeyError,TypeError,ValueError,IndexError):negative+=1
    else:raise AssertionError('Old channel accepted')
  details['SyntheticNotOwning']=synthetic
 else:raise AssertionError('Unknown mode')
 assert not report.exists();report.write_text(json.dumps(dict(Passed=True,Mode=mode,Details=details,TampersRefused=negative,NNReplay=False,VendorExecuted=False,DocumentApplyClaim=False,SourceGuideProven=False,FrontendC20OriginProven=False,PreparedUnexportedChannels=['full trees','full memberships','full-cap result arrays','runtime cache field']),indent=2)+'\n',encoding='utf-8')
 print(json.dumps(dict(Passed=True,Mode=mode,TampersRefused=negative)))
if __name__=='__main__':main()
