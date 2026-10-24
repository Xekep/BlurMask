import ast,copy,hashlib,json,sys
from pathlib import Path
HERE=Path(__file__).resolve().parent
b=(HERE/'reference_range_operations.py').read_bytes();assert hashlib.sha256(b).hexdigest()=='dc5e998a42e4dcedbfda714d97dd58d06136af608c5f4aa5bc03bf70b89fb750'
g={'MASK':2**64-1};tree=ast.parse(b.decode());exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,ast.FunctionDef)],type_ignores=[]),'pinnedC21','exec'),g)
def strict(a,b):
 assert type(a)is type(b),(type(a),type(b))
 if type(b)is dict:
  assert set(a)==set(b)
  for k in b:strict(a[k],b[k])
 elif type(b)is list:
  assert len(a)==len(b),(len(a),len(b))
  for x,y in zip(a,b,strict=True):strict(x,y)
 else:assert a==b,(a,b)
def same(a,b):return a>>32==b>>32 and (a>>16)&65535==(b>>16)&65535 and ((a>>8)&255==(b>>8)&255 or (a>>8)&255==255 or (b>>8)&255==255)
def process(t):
 ranges=copy.deepcopy(t['Ranges']);steps=[];nodes={};root=None;inserted=[]
 def height(n):return 0 if n is None else n['h']
 def update(n):n['h']=1+max(height(n['l']),height(n['r']))
 def left(n):r=n['r'];n['r']=r['l'];r['l']=n;update(n);update(r);return r
 def right(n):l=n['l'];n['l']=l['r'];l['r']=n;update(n);update(l);return l
 def insert(n,fresh):
  if n is None:return fresh
  side='l'if fresh['key']<n['key']else'r';n[side]=insert(n[side],fresh);update(n)
  balance=height(n['r'])-height(n['l'])
  if balance>1:
   if height(n['r']['l'])>height(n['r']['r']):n['r']=right(n['r'])
   return left(n)
  if balance< -1:
   if height(n['l']['r'])>height(n['l']['l']):n['l']=left(n['l'])
   return right(n)
  return n
 for i in range(len(ranges)):
  ordinary=t['Positive'][i];key=ranges[i]
  if ordinary:
   if len(key)==1 and same(*key[0]) and not any(g['n'](a)<=g['n'](key[0][0])<=g['n'](b)and key[0][1]<=b for a,b in t['Original']):ordinary=False;key[0]=[(key[0][0]&~255)|4,(key[0][1]&~255)|4]
   elif t['Joiner'][i]:
    assert key;end=key[0][1]
    if(end>>8)&255==255:end&=~0xff00
    end=(end&~255)|4;key=[[end,end]];ordinary=False
   else:
    for j in range(i+1,len(ranges)):
     if not t['Positive'][j]:continue
     overlap=g['intersect'](g['union']([],ranges[j]),key)
     if not overlap:continue
     if key!=overlap:key=g['subtract'](key,overlap,t['Counts'])
     else:ranges[j]=g['subtract'](ranges[j],overlap,t['Counts'])
     break
  ranges[i]=key;kt=tuple(tuple(p)for p in key);new=kt not in nodes
  if new:
   n={'key':kt,'id':len(inserted)+1,'l':None,'r':None,'h':1};nodes[kt]=n;inserted.append(n);root=insert(root,n)
  steps.append(dict(Ordinary=ordinary,Key=key,Node=nodes[kt]['id'],Inserted=new))
 tree=[]
 for i,n in enumerate(inserted):tree.append(dict(Identity=n['id'],Left=0 if n['l']is None else n['l']['id'],Right=0 if n['r']is None else n['r']['id'],Balance=height(n['r'])-height(n['l']),PreviousInserted=i,NextInserted=i+2 if i+1<len(inserted)else 0))
 return dict(Id=t['Id'],Steps=steps,Tree=tree,Root=0 if root is None else root['id'])
def flags(t):
 # Literal1FAE0 bit instructions, deliberately not the C# folded mask.
 edx=(t['Aggregate']&~(1<<21))|(0 if t['Ordinary']else 1<<21)
 deg=t['Ordinary']and len(t['Key'])==1 and same(*t['Key'][0])
 edx=(edx&~(1<<23))|((1<<23)if deg else 0);edx&=0xcfbfffff;edx&=~(1<<24)
 return((((t['Prior']&1)<<24)^edx)&~(1<<25))&0xffffffff
controls=json.loads(Path(sys.argv[1]).read_bytes());sources=json.loads(Path(sys.argv[2]).read_bytes());actual=json.loads(Path(sys.argv[3]).read_bytes())
cs=controls['cases'];appends=sum(c['expected']['appended']for c in cs);fail=sum(not c['expected']['ok']for c in cs);reuses=0
for c in cs:
 e=c['expected'];incomplete=int(not e['ok']and e['visited']<len(c['incoming'])and c['incoming'][e['visited']]['failure']in(4,5));reuses+=e['visited']-e['appended']+incomplete
expected=dict(Scope='PREPARED_NEUTRAL_SOURCE_PROCESS_INTERN_REUSE_NOT_ACTUAL_CHAR',ReuseCases=len(cs),Appends=appends,Reuses=reuses,FailedPrefixes=fail,FullRecordRows=sum(len(c['expected']['records'])for c in cs),Flags=[flags(t)for t in sources['Flags']],SourceCases=[process(t)for t in sources['Cases']],ActualOwnerExecuted=False,NN=False)
strict(actual,expected)
mutations=[lambda x:x.update(ReuseCases=1),lambda x:x.update(ActualOwnerExecuted=True),lambda x:x['Flags'].__setitem__(0,x['Flags'][0]^1),lambda x:x['SourceCases'][0]['Steps'][0].update(Node=True),lambda x:x['SourceCases'][0]['Steps'][0].update(Ordinary=False),lambda x:x['SourceCases'][0]['Steps'][0].update(Key=[]),lambda x:x['SourceCases'][0].update(Root=99),lambda x:x['SourceCases'][0]['Tree'][0].update(Balance=5),lambda x:x.update(FullRecordRows=0),lambda x:x.update(Scope='ACTUAL')]
for mutate in mutations:
 x=copy.deepcopy(actual);mutate(x)
 try:strict(x,expected)
 except AssertionError:continue
 raise AssertionError('Tamper accepted')
report=dict(Status='PASS_PREPARED_NEUTRAL_CHAR_COMPONENTS_ONLY',ReuseCases=len(cs),FullRecordRows=expected['FullRecordRows'],ScalarBytes=expected['FullRecordRows']*40,SourceCases=len(sources['Cases']),Flags=len(sources['Flags']),FailedPrefixes=fail,TamperRefusals=len(mutations),NoActualOwner=True,NN=False)
Path(sys.argv[4]).write_bytes(json.dumps(report,indent=2).encode());print(json.dumps(report))
