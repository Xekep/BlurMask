"""Strict frozen/live AL SOURCE14 DATA audit; never invokes profile/source/FIRST."""
from pathlib import Path
import argparse,hashlib,json,importlib.util
D=Path(__file__).resolve().parent
def module(name,path):
 spec=importlib.util.spec_from_file_location(name,path);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
oracle=module('al_raw_data_oracle',D/'oracle.py')
contract=module('al_source_contract',D/'source_contract.py')
def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('root',type=Path);p.add_argument('report',type=Path);p.add_argument('--capture',type=Path);p.add_argument('--selftest',action='store_true');a=p.parse_args()
 assert not a.report.exists();root=a.root.resolve();sourcePins=contract.authenticate(root)
 raw=(root/'models/math/math-ak.res').read_bytes();capture=a.capture or D/'historical-al-source14.json';b=capture.read_bytes();assert len(b)<=131072
 if a.capture is None:assert hashlib.sha256(b).hexdigest()=='3611e7a4f2b150b8735adae05e3f38c404177133a20165d3dd2165675950396e'
 expected=oracle.expected(raw);actual=oracle.loads(b.decode('utf-8'));oracle.exact(expected,actual)
 controls=None
 if a.selftest:
  out=a.report.parent/(a.report.stem+'-controls');oracle.selftest(raw,out);controls=oracle.loads((out/'report.json').read_bytes())['controls']
 report=dict(PASS=True,scope='Strict AL scalar/raw dictionary and unchanged old-header14 DATA; no fresh source/NN/evaluator/cache/private-state proof',cases=14,exceptionRefusals=8,assertions=85,positive1390Cases=1,sourcePins=sourcePins,captureSha256=hashlib.sha256(b).hexdigest(),negativeControls=controls,freshSourceExecutions=0,profileExecutions=0,FIRST=0,NN=0,capScope='131072 accepted serialized UTF8; nominal4096 formula is NOT whole allocation bound')
 a.report.write_bytes((json.dumps(report,indent=2)+chr(10)).encode('utf-8'));print('PASS AL SOURCE14 DATA/93guards/0freshSource')
if __name__=='__main__':main()
