"""Strict independent immutable-profile/resource DATA audit; no .NET execution."""
from pathlib import Path
import argparse,hashlib,json,importlib.util
D=Path(__file__).resolve().parent
spec=importlib.util.spec_from_file_location('first276_raw_oracle',D/'oracle.py')
oracle=importlib.util.module_from_spec(spec);spec.loader.exec_module(oracle)
def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('root',type=Path);p.add_argument('capture',type=Path);p.add_argument('report',type=Path);p.add_argument('--selftest',action='store_true');a=p.parse_args()
 assert not a.report.exists()
 # AL is an exact authenticated semantic addition; old capture/raw oracle unchanged.
 contractPath=a.root/'tools/LegacyMathAkFirstAlphabet.Probe/source_contract.py'
 spec=importlib.util.spec_from_file_location('first276_al_source_contract',contractPath)
 contract=importlib.util.module_from_spec(spec);spec.loader.exec_module(contract);contract.authenticate(a.root)
 pins=json.loads((D/'source-pins.json').read_text('utf-8'))
 for name,sha in pins.items():assert hashlib.sha256((a.root/'src/InkRecognition'/name).read_bytes().replace(b'\r\n',b'\n')).hexdigest()==sha,name
 raw=(a.root/'models/math/math-ak.res').read_bytes();b=a.capture.read_bytes();assert len(b)<=262144
 def pairs(ps):
  d={}
  for k,v in ps:
   assert k not in d,('duplicate-key',k);d[k]=v
  return d
 actual=json.loads(b.decode('utf-8'),object_pairs_hook=pairs,parse_constant=lambda x:(_ for _ in()).throw(AssertionError(x)))
 oracle.audit(actual,raw)
 controls=oracle.controls(raw)if a.selftest else None
 report=dict(PASS=True,scope='Immutable authenticated profile/resource DATA only; no geometry/source/FIRST/model evaluation',resourceSha256=oracle.SHA,captureSha256=hashlib.sha256(b).hexdigest(),firstProgramBytes=109,firstCalibrationShorts=552,oldTailCalibrationShorts=2386,oldTailProgramFields=24,observations=2,guards=10,geometryCalls=0,sourceCalls=0,firstCalls=0,nnCalls=0,negativeControls=controls)
 a.report.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print('PASS immutable profile DATA / tail unchanged / no source or evaluator')
if __name__=='__main__':main()
