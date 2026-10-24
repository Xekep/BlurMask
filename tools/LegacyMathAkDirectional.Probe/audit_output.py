from pathlib import Path
import sys,json,hashlib,copy
sys.path.insert(0,str(Path(__file__).resolve().parent))
from reference_directional import calculate
R,C,O=map(Path,sys.argv[1:]);H=lambda d:hashlib.sha256(d).hexdigest();resource=(R/'models/math/math-ak.res').read_bytes();assert H(resource)=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
CASES=[('triangle',[[0,0],[2,1],[4,0]]),('horizontal',[[0,0],[40,0]]),('vertical',[[0,0],[0,30]]),('nonsquare',[[0,0],[3,8],[4,2]]),('turning',[[0,0],[5,2],[1,6],[8,3],[2,0]]),('duplicate',[[0,0],[0,0],[2,1],[4,0]]),('negative',[[8,8],[6,4],[2,3]])]
GUARDS=['same-profile-foreign-geometry-refused','equal-data-foreign-profile-refused','values-defensive-copy','pre-cancel-exact-token','single-point-refused','degenerate-refused','multistroke-refused','source-over4096-refused','shared-profile-after-caller-resource-clear']
expected=[calculate(original,resource)for _,original in CASES]
def check(r):
 assert r['modelExecutions']==0 and type(r['modelExecutions'])is int and r['vendorExecution']is False and r['guards']==GUARDS
 assert len(r['cases'])==7
 for row,(name,original),want in zip(r['cases'],CASES,expected):
  assert row['name']==name and row['original']==original and row['normalized']==want['normalized']
  assert type(row['PreparedPointCount'])is int and row['PreparedPointCount']==want['preparedCount']
  assert type(row['ModelStart'])is int and row['ModelStart']==1289175 and row['FeatureCount']==279
  assert row['HasCandidateAuthority']is False and row['HasClassifierOutput']is False
  assert len(row['values'])==279 and all(type(v)is int for v in row['values']) and row['values']==want['values']
  assert row['values'][274:277]==[-1024]*3 and want['defaultDifferences']>0
r=json.loads(C.read_text());check(r);neg=[]
for name,mut in [('value',lambda q:q['cases'][0]['values'].__setitem__(0,0)),('uncalibrated-baseline',lambda q:q['cases'][0]['values'].__setitem__(274,0)),('wrong-model',lambda q:q['cases'][0].__setitem__('ModelStart',8702)),('prepared-count',lambda q:q['cases'][0].__setitem__('PreparedPointCount',1)),('geometry',lambda q:q['cases'][3]['normalized'][1].__setitem__(0,0)),('guard-order',lambda q:q['guards'].reverse()),('authority',lambda q:q['cases'][0].__setitem__('HasCandidateAuthority',True)),('numeric-type',lambda q:q['cases'][0]['values'].__setitem__(0,1024.0))]:
 bad=copy.deepcopy(r);mut(bad)
 try:check(bad)
 except AssertionError:neg.append(name)
 else:raise AssertionError(name)
O.write_text(json.dumps(dict(captureSHA256=H(C.read_bytes()),vectors=7,features=1953,guards=9,tamperControls=neg,defaultCalibrationDiscriminators=[e['defaultDifferences']for e in expected],modelExecutions=0),indent=2)+'\n')
print('PASS7 source-bound279 vectors/1953features/9guards/8corruptions/0NN')
