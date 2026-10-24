from pathlib import Path
import copy,hashlib,importlib.util,json,math,sys
D=Path(__file__).resolve().parent;R=Path(sys.argv[1]);C=Path(sys.argv[2]);O=Path(sys.argv[3]);assert not O.exists()
PREFIX_LF_PINS={'reference_prefix.py': '61fd185b05adb330bc933cddfe8b092ab30ad73c0b1dd335f9d75181a67447ab', 'reference_bootstrap.py': 'b879c0204031276a8a213b141f3db424eba70a8edd170d7daecb27dd7bb9f3ce'}
for name,sha in PREFIX_LF_PINS.items():
 assert hashlib.sha256((R/'tools/LegacyMathAkProcessedPrefix.Probe'/name).read_bytes().replace(b'\r\n',b'\n')).hexdigest()==sha
spec=importlib.util.spec_from_file_location('geometry_reference',D/'reference_geometry.py');m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);ref=m.controls()
GUARDS=['retains-genuine-prefix-1','actual-returned-copies-detached','no-processed-graph-authority','precancel-exact-type-token','copy-cancel-exact-type-token','foreign-profile-keeps-own-prefix','null-prefix-refused']+[f'retains-genuine-prefix-{i}'for i in range(2,8)]
SHA='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1';assert hashlib.sha256((R/'models/math/math-ak.res').read_bytes()).hexdigest()==SHA
maxerr=0
def exact(a,b):
 if type(a)is not type(b):return False
 if isinstance(b,list):return len(a)==len(b)and all(exact(x,y)for x,y in zip(a,b))
 return a==b

def coords(a,b):
 global maxerr
 assert type(a)is list and len(a)==len(b)
 for row,want in zip(a,b):
  assert type(row)is list and len(row)==2
  for x,y in zip(row,want):
   assert type(x)in(int,float)and math.isfinite(x)
   e=abs(x-y);maxerr=max(maxerr,e);assert e<=2e-6+2e-6*abs(y),(x,y)

def check(c):
 assert set(c)=={'resourceSHA','cases','preparedJoinData','guards','portableDoubleTranscendentals','networkForwards'}and c['resourceSHA']==SHA
 assert type(c['networkForwards'])is int and c['networkForwards']==0 and c['portableDoubleTranscendentals']is True and exact(c['guards'],GUARDS)
 assert type(c['cases'])is list and len(c['cases'])==7
 for got,w in zip(c['cases'],ref['cases']):
  assert set(got)=={'name','selectedSourceIndices','polylines'}and got['name']==w['name'];rows=w['result']['rows'];assert type(got['selectedSourceIndices'])is list and type(got['polylines'])is list and len(got['selectedSourceIndices'])==len(rows)==len(got['polylines'])
  for ids,poly,row in zip(got['selectedSourceIndices'],got['polylines'],rows):
   assert exact(ids,row['indices'])
   if row['skippedSmallBounds']:assert poly is None and ids is None
   else:coords(poly,row['points'])
 assert type(c['preparedJoinData'])is list and len(c['preparedJoinData'])==2
 for got,w in zip(c['preparedJoinData'],ref['preparedJoinData']):
  assert set(got)=={'name','points','numericDataOnly'}and got['name']==w['name']and got['numericDataOnly']is True;coords(got['points'],w['result'][0])
 return True
c=json.loads(C.read_text());assert check(c);actual_max=maxerr
mutations=[('source-index',lambda z:z['cases'][0]['selectedSourceIndices'][0].__setitem__(0,1)),('coordinate',lambda z:z['cases'][3]['polylines'][0][1].__setitem__(0,99)),('missing-vs-empty',lambda z:z['cases'][-1]['polylines'].__setitem__(0,[])),('join-branch',lambda z:z['preparedJoinData'][0]['points'][1].__setitem__(0,99)),('nan-coordinate',lambda z:z['cases'][0]['polylines'][0][0].__setitem__(0,float('nan'))),('bool-index',lambda z:z['cases'][0]['selectedSourceIndices'][0].__setitem__(0,False)),('guard-deleted',lambda z:z['guards'].pop()),('bool-NN-zero',lambda z:z.__setitem__('networkForwards',False))]
neg=[]
for name,mutate in mutations:
 z=copy.deepcopy(c);mutate(z)
 try:check(z)
 except(AssertionError,ValueError,KeyError,IndexError):neg.append(name)
 else:raise AssertionError('Accepted '+name)
O.write_text(json.dumps(dict(sourceCases=7,numericJoinCases=2,guards=13,negativeControls=neg,maxAbsoluteCoordinateError=actual_max,absoluteTolerance=2e-6,relativeTolerance=2e-6,indicesAndMissingRowsExact=True,portableCRTPolicy=True,networkForwards=0,captureSHA=hashlib.sha256(C.read_bytes()).hexdigest()),indent=2)+'\n');print('PASS7 source geometries/2numeric joins/13guards/8tamper/0NN, maxerr',actual_max)
