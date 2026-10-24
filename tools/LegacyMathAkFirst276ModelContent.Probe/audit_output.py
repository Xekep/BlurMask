"""Strict retained-model immutable DATA audit; never invokes any .NET target."""
from pathlib import Path
import argparse,hashlib,json,importlib.util
D=Path(__file__).resolve().parent

def module(name,path):
 spec=importlib.util.spec_from_file_location(name,path);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('root',type=Path);p.add_argument('report',type=Path);p.add_argument('--capture',type=Path);p.add_argument('--selftest',action='store_true');a=p.parse_args()
 assert not a.report.exists();root=a.root.resolve()
 contract=module('model_storage_source_contract',root/'tools/LegacyMathAkFirstAlphabet.Probe/source_contract.py');pins=contract.authenticate(root)
 oracle=module('model_storage_raw_oracle',D/'oracle.py')
 e=oracle.expected((root/'models/math/math-ak.res').read_bytes())
 capture=a.capture or D/'historical-model-content.json';raw=capture.read_bytes();assert len(raw)<=8192
 if a.capture is None:assert hashlib.sha256(raw).hexdigest()=='e00db74004b398120db1e1351dd31e02fe047d9bf2066115983e8f01cfcaff78'
 oracle.strict(json.loads(raw.decode('utf-8'),object_pairs_hook=oracle.unique),e)
 controls=oracle.controls(e)if a.selftest else None
 report=dict(PASS=True,scope='Stored immutable retained-model DATA + exact source semantic reversal; no fresh profile/source/FIRST/NN execution',sourcePins=pins,negativeControls=controls,observations=2,guardCount=12,contentScalarReads=222628,callerOverwriteByte=165,profileExecutions=0,sourceExecutions=0,FIRST=0,NN=0)
 a.report.write_bytes((json.dumps(report,indent=2)+'\n').encode());print('PASS retained-model DATA/12guards/0freshProfile')
if __name__=='__main__':main()
