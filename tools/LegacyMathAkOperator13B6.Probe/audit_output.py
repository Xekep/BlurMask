from pathlib import Path
import copy,hashlib,importlib.util,json,sys
D=Path(__file__).resolve().parent;R=Path(sys.argv[1]);C=Path(sys.argv[2]);O=Path(sys.argv[3]);assert not O.exists()
PINS={}
for relative,sha in json.loads((D/'reference-pins.json').read_text()).items():assert hashlib.sha256((R/relative).read_bytes().replace(b'\r\n',b'\n')).hexdigest()==sha
s=importlib.util.spec_from_file_location('connected',D/'reference_connected.py');m=importlib.util.module_from_spec(s);s.loader.exec_module(m)
fixtures=json.loads((D/'fixtures.json').read_text());expected=[dict(name=x['name'],**m.execute(x['input']))for x in fixtures['cases']]
SHA='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1';assert hashlib.sha256((R/'models/math/math-ak.res').read_bytes()).hexdigest()==SHA
GUARDS=[]
for i,x in enumerate(expected,1):
 GUARDS += [f'same-prefix-frame-{i}',f'no-authority-{i}']
 if i==1:GUARDS+=['precancel-exact-token','copy-precancel-exact-token','null-original-refused']
 GUARDS += [f'processed-copy-detached-{i}']
 for j in range(x['rangeResultCount']):GUARDS += [f'ranges-detached-{i}-{j}',f'foreign-range-context-{i}-{j}']
def exact(a,b):
 if type(a)!=type(b):return False
 if type(b)is list:return len(a)==len(b)and all(exact(x,y)for x,y in zip(a,b))
 return a==b
def check(c):
 assert set(c)=={'resourceSHA','cases','guards','networkForwards'}and c['resourceSHA']==SHA
 assert type(c['networkForwards'])is int and c['networkForwards']==0 and exact(c['guards'],GUARDS)
 assert type(c['cases'])is list and len(c['cases'])==len(expected)
 for got,w in zip(c['cases'],expected):
  assert set(got)=={'name','processedWords','records','descriptorCount','rangeResultCount'}
  for key in got:assert exact(got[key],w[key]),(w['name'],key,got[key],w[key])
 return True
c=json.loads(C.read_text());assert check(c)
mutations=[('processed-bitmap',lambda x:x['cases'][-1]['processedWords'].__setitem__(0,0)),('range-current-id',lambda x:x['cases'][-1]['records'][0].__setitem__(0,'000000000000000000000041')),('range-endpoint',lambda x:x['cases'][-1]['records'][0].__setitem__(0,'01000000000000000000e040')),('range-byte-order',lambda x:x['cases'][-1]['records'][0].reverse()),('range-owner-deleted',lambda x:x['cases'][-1]['records'].clear()),('descriptor-count',lambda x:x['cases'][-1].__setitem__('descriptorCount',0)),('bool-processed',lambda x:x['cases'][0]['processedWords'].__setitem__(0,False)),('bool-zero-NN',lambda x:x.__setitem__('networkForwards',False)),('guard-deleted',lambda x:x['guards'].pop())]
negative=[]
for name,mutate in mutations:
 x=copy.deepcopy(c);mutate(x)
 try:check(x)
 except AssertionError:negative.append(name)
 else:raise AssertionError('accepted '+name)
O.write_text(json.dumps(dict(cases=len(expected),ranges=sum(x['rangeResultCount']for x in expected),guards=len(GUARDS),negatives=negative,networkForwards=0,captureSHA=hashlib.sha256(C.read_bytes()).hexdigest()),indent=2)+'\n',encoding='utf-8')
print('PASS connected scalar raw-input reference, exact record bytes, strict schema, guards,9 tamper;0NN')
