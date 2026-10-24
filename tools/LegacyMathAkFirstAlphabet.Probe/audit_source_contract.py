"""Pure exact source-contract mutation controls; no target/model execution."""
from pathlib import Path
import argparse,json,importlib.util,tempfile
D=Path(__file__).resolve().parent
spec=importlib.util.spec_from_file_location('al_contract_controls',D/'source_contract.py');m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
p=argparse.ArgumentParser(description=__doc__);p.add_argument('root',type=Path);p.add_argument('report',type=Path);a=p.parse_args()
assert not a.report.exists();m.authenticate(a.root);controls=[]
files={n:(a.root/'src/InkRecognition'/n).read_bytes()for n in m.EXPECTED_SOURCES}
def refuse(name,target,change):
 with tempfile.TemporaryDirectory(prefix='first-al-source-',dir=a.report.parent)as temp:
  root=Path(temp);base=root/'src/InkRecognition';base.mkdir(parents=True)
  for n,b in files.items():(base/n).write_bytes(change(b)if n==target else b)
  try:m.authenticate(root)
  except AssertionError:controls.append(name)
  else:raise AssertionError('source tamper admitted '+name)
for name in files:refuse('extra-source-'+name,name,lambda b:b+bytes([10])+b'// altered')
refuse('old-profile-without-AL','LegacyMathAkFeatureProfile.cs',m.restore_profile)
refuse('old-header-without-partial','LegacyMathAkFreshCommonWorkspace.cs',m.restore_header)
refuse('new-prefix-friend-changed','LegacyMathAkFirst276Alphabet.cs',lambda b:b.replace(b'LegacyMathAkFirstAlphabet.Probe',b'Foreign.Probe',1))
refuse('false-FIRST-status-changed','LegacyMathAkFirst276SourceBinding.cs',lambda b:b.replace(b'17,7,false,false',b'17,7,true,false',1))
assert len(controls)==9
a.report.write_bytes((json.dumps(dict(PASS=True,scope='Pure exact source addition/refusal controls; no profile/source/NN execution',sourcePins=5,controls=controls,profileCalls=0,sourceCalls=0,NN=0),indent=2)+chr(10)).encode('utf-8'));print('PASS9 exact source-contract refusals')
