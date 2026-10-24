"""Strict historical AL/header plus fresh-storage allocation DATA; no .NET target."""
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
 e=oracle.expected((D/'historical-al-source14.json').read_bytes())
 capture=a.capture or D/'historical-storage14.json';raw=capture.read_bytes();assert len(raw)<=131072
 if a.capture is None:assert hashlib.sha256(raw).hexdigest()=='893d49144c77fb0bf9a234118bda373b6c9bd1366c88f1a9b62afe36800f8f39'
 oracle.strict(json.loads(raw.decode('utf-8'),object_pairs_hook=oracle.unique),e)
 assert hashlib.sha256((D/'fixtures.json').read_bytes()).hexdigest()=='ce3505ffeb0014a62eacde7c2d4841a23dda5f704fb127820fac4c937134eacb'
 controls=oracle.controls(e)if a.selftest else None
 report=dict(PASS=True,scope='Stored original14 genuine SOURCE allocation DATA, not fresh source execution; three actual array-length getters, four-output/nonalias facts from source only',sourcePins=pins,negativeControls=controls,cases=14,oldALGuards=93,positive1390=1,profileExecutions=0,sourceExecutions=0,FIRST=0,NN=0)
 a.report.write_bytes((json.dumps(report,indent=2)+'\n').encode());print('PASS storage14 DATA/93oldguards/0freshSource')
if __name__=='__main__':main()
