"""Independent source geometry/native temporal222 check; never evaluates networks."""
from pathlib import Path
import argparse,hashlib,json,struct,importlib.util,copy,math
p=argparse.ArgumentParser();p.add_argument('repo',type=Path);p.add_argument('capture',type=Path);p.add_argument('output',type=Path);a=p.parse_args()
s=importlib.util.spec_from_file_location('literal',Path(__file__).with_name('reference_temporal.py'));ref=importlib.util.module_from_spec(s);s.loader.exec_module(ref)
b=(a.repo/'models/math/math-ak.res').read_bytes();assert hashlib.sha256(b).hexdigest()=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
assert struct.unpack_from('<H',b,1006005)[0]==0x1211 and struct.unpack_from('<I',b,1006007)[0]==0x3f
assert struct.unpack_from('<4H',b,1006011)==(30,16,8,10)
assert struct.unpack_from('<H',b,1006083)[0]==412
mi=struct.unpack_from('<412h',b,1006085);ma=struct.unpack_from('<412h',b,1006909)
r=json.loads(a.capture.read_text())
NAMES=['horizontal-half','vertical','nonsquare','fractional','duplicates','zigzag','closed-loop']
GUARDS=['equal-data-foreign-issuer','values-detached','points-detached','point-degenerate','multistroke-outside-subset','feature-point-cap','cancel-exact-token']
def check(r):
 assert r['modelExecutions']==0 and r['vendorExecution']is False and r['guards']==GUARDS
 assert [x['name']for x in r['rows']]==NAMES
 assert [q['value']for q in r['numericNorm']]==[16383,16384,32000,65535,4294967295]
 assert all(q['result']==ref.norm(q['value'])for q in r['numericNorm'])
 for row in r['rows']:
  normalized=ref.normalize(row['original']);assert normalized==row['normalized']
  samples,values=ref.calculate(normalized,mi,ma)
  assert row['samples']==samples and row['values']==values
  assert row['FeatureCount']==222 and row['ModelStart']==1007739 and row['guidance']=='absent'
  assert row['HasCandidateAuthority']is False and row['HasClassifierOutput']is False
check(r);neg=[]
for name,mutate in [('source-point',lambda z:z['rows'][5]['original'][1].__setitem__(0,2)),('sample-point',lambda z:z['rows'][0]['samples'][0].__setitem__(0,123)),('feature-value',lambda z:z['rows'][0]['values'].__setitem__(0,123)),('baseline-value',lambda z:z['rows'][0]['values'].__setitem__(217,123)),('boundary-value',lambda z:z['rows'][0]['values'].__setitem__(221,-1024)),('foreign-model',lambda z:z['rows'][0].__setitem__('ModelStart',8702)),('authority',lambda z:z['rows'][0].__setitem__('HasCandidateAuthority',True)),('norm-policy',lambda z:z['numericNorm'][2].__setitem__('result',178)),('guard',lambda z:z['guards'].pop())]:
 z=copy.deepcopy(r);mutate(z)
 try:check(z)
 except AssertionError:neg.append(name)
 else:raise AssertionError(name)
#Literal norm branch has a minimum large-input shift2. Keep a negative-policy
#witness independent of the managed output rather than assuming generic sqrt.
witness=[]
for x in range(128):
 for y in range(128):
  square=(x*16)**2+(y*16)**2;shift=0
  while square>>(shift*2)>=16384:shift+=1
  wrong=int(math.sqrt(square>>(shift*2))+.5)<<shift
  if ref.norm(square)!=wrong:witness.append(dict(dx=x,dy=y,square=square,actual=ref.norm(square),generic=wrong))
assert witness
out=dict(captureSHA256=hashlib.sha256(a.capture.read_bytes()).hexdigest(),rows=7,features=1554,guards=7,numericNormCases=5,corruptions=neg,normPolicyDiscriminators=len(witness),firstNormDiscriminator=witness[0],calibrationSHA256=hashlib.sha256(b[1006085:1007733]).hexdigest(),modelExecutions=0,scope='One genuine AK program/single whole source/explicit absent-guide temporal222 prefix; no classification or candidate authority')
a.output.write_text(json.dumps(out,indent=2)+'\n',encoding='utf8');print('PASS 7 vectors / 1554 features / 7 guards / 9 corruptions / 0NN')
