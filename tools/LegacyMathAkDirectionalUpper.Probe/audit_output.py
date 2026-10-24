from pathlib import Path
import sys,json,hashlib,copy,struct
sys.path.insert(0,str(Path(__file__).resolve().parent))
from reference_upper import calculate,literal,parts
R,C,O=map(Path,sys.argv[1:]);assert not O.exists();H=lambda b:hashlib.sha256(b).hexdigest();ak=(R/'models/math/math-ak.res').read_bytes();assert H(ak)=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
CASES=[('triangle',[[0,0],[2,1],[4,0]]),('horizontal',[[0,0],[40,0]]),('vertical',[[0,0],[0,30]]),('nonsquare',[[0,0],[3,8],[4,2]]),('turning',[[0,0],[5,2],[1,6],[8,3],[2,0]]),('duplicate',[[0,0],[0,0],[2,1],[4,0]]),('negative',[[8,8],[6,4],[2,3]])]
GUARDS=['same-profile-foreign-geometry-refused','equal-data-foreign-profile-refused','values-defensive-copy','pre-cancel-exact-token','single-point-refused','degenerate-refused','multistroke-refused','source-over4096-refused','shared-profile-after-caller-resource-clear','exact5F-feature-scope','absent-guide-whole-boundaries']
expected=[calculate(p,ak)for _,p in CASES];uniform=calculate(CASES[3][1],ak,True)
lo=struct.unpack_from('<223h',ak,1628214);hi=struct.unpack_from('<223h',ak,1628660)
global_=[i*40000-50000 for i in range(17)];cells=[[(r*8+c-100)*100000 for c in range(8)]for r in range(25)]
def transform(v,d):return 0 if d==0 else parts.sat(parts.i32(parts.div(parts.i32(v<<11),d)-1024))
def compose(arc):
 v=[1024]
 for i,x in enumerate(global_):v.append(parts.sat(parts.div(parts.i32(x*2048-2048),5)-1024 if i==2 else parts.i32(x*2)if i==8 else parts.div(parts.i32(x*2048),3072 if i==3 else 3 if 9<=i<=15 else 1024)-1024))
 v += [transform(x,arc//20)for cell in cells for x in cell];assert len(v)==218
 for i in range(1,218):
  if hi[i]!=lo[i]:v[i]=parts.sat(parts.div(parts.i32((v[i]-lo[i])*2048),hi[i]-lo[i])-1024)
 return v+[-1024]*3+[1024]*2
def ints(value,n):assert type(value)is list and len(value)==n and all(type(v)is int for v in value)
def check(r):
 assert type(r['modelExecutions'])is int and r['modelExecutions']==0 and r['vendorExecution']is False and r['guards']==GUARDS
 assert len(r['cases'])==7
 for row,(name,original),want in zip(r['cases'],CASES,expected):
  assert row['name']==name and row['original']==original and row['normalized']==want['normalized']
  assert type(row['PreparedPointCount'])is int and row['PreparedPointCount']==want['preparedCount']
  assert type(row['ModelStart'])is int and row['ModelStart']==1629111 and row['FeatureCount']==223
  assert row['HasCandidateAuthority']is False and row['HasClassifierOutput']is False
  ints(row['values'],223);assert row['values']==want['values']
 assert len(r['octants'])==1225
 for row,(dx,dy)in zip(r['octants'],((dx,dy)for dx in range(-17,18)for dy in range(-17,18))):
  assert type(row['dx'])is int and type(row['dy'])is int and row['dx']==dx and row['dy']==dy
  ints(row['values'],8);assert row['values']==literal(dx,dy)
 pairs=[(v,d)for v in [-2147483648,-1048577,-1048576,-1,0,1,1048575,1048576,2147483647]for d in [0,1,3,20,1000]];assert len(r['massControls'])==45
 for row,(v,d)in zip(r['massControls'],pairs):assert type(row['mass'])is int and type(row['denominator'])is int and type(row['value'])is int and (row['mass'],row['denominator'],row['value'])==(v,d,transform(v,d))
 assert r['global']==global_ and r['cells']==cells
 assert r['kernelControls']==[dict(name=name,values=([0]*8 if i<4 else [0,14,0,0,0,0,0,0]))for i,name in enumerate(['empty-members','end2-skip','singleton3-skip','last-point-skip','start1-next-outside-member'])]
 assert len(r['composition'])==3
 for row,arc in zip(r['composition'],[0,20,2000]):assert type(row['arc'])is int and row['arc']==arc;ints(row['values'],223);assert row['values']==compose(arc)
r=json.loads(C.read_text(encoding='utf-8-sig'));check(r);controls=[]
for name,mut in [('value',lambda q:q['cases'][0]['values'].__setitem__(0,0)),('baseline',lambda q:q['cases'][0]['values'].__setitem__(218,0)),('model-scope',lambda q:q['cases'][0].__setitem__('ModelStart',1459255)),('prepared-count',lambda q:q['cases'][0].__setitem__('PreparedPointCount',1)),('authority',lambda q:q['cases'][0].__setitem__('HasCandidateAuthority',True)),('value-type',lambda q:q['cases'][0]['values'].__setitem__(0,1024.0)),('uniform-grid',lambda q:q['cases'][3].__setitem__('values',uniform['values'])),('guard-order',lambda q:q['guards'].reverse()),('octant',lambda q:q['octants'][0]['values'].__setitem__(5,0)),('mass-shift-wrap',lambda q:q['massControls'][36].__setitem__('value',-32768)),('end-flag',lambda q:q['kernelControls'][1]['values'].__setitem__(1,14)),('composition-calibration',lambda q:q['composition'][1]['values'].__setitem__(1,0))]:
 bad=copy.deepcopy(r);mut(bad)
 try:check(bad)
 except AssertionError:controls.append(name)
 else:raise AssertionError('Corruption accepted: '+name)
O.write_text(json.dumps(dict(captureSHA256=H(C.read_bytes()),vectors=7,features=1561,guards=11,octantPairs=1225,massControls=45,kernelControls=5,compositionControls=3,compositionChannels=669,tamperControls=controls,uniformGridDiscriminators=[sum(a!=b for a,b in zip(e['values'],calculate(p,ak,True)['values']))for e,(_,p)in zip(expected,CASES)],modelExecutions=0),indent=2)+'\n',encoding='utf-8')
print('PASS7upper223/1561features/1225octants/45mass/5endpoint/3composition/11guards/12tamper;0NN')
