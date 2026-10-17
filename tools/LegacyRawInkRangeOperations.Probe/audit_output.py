"""Independent linear active-range oracle; genuine decoded-source captures contain no expected labels."""
import sys,json,importlib.util
from pathlib import Path
spec=importlib.util.spec_from_file_location('c21_reference',Path(__file__).with_name('reference_controls.py'));m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
controls=json.loads(Path(sys.argv[1]).read_text());capture=json.loads(Path(sys.argv[2]).read_text())
assert len(capture['numeric'])==len(controls['tests'])
for wanted,actual in zip(controls['tests'],capture['numeric']):
 for name in ['union','intersect','subtract']:assert actual[name]==wanted[name],name
for actual in capture['actual']:
 assert actual['union']==m.union(actual['target'],actual['extra'])
 assert actual['intersect']==m.intersect(actual['target'],actual['filters'])
 assert actual['subtract']==m.subtract(actual['target'],actual['cuts'],actual['counts'])
print('PASS independent numeric/actual active-range arrays:',len(capture['numeric']),len(capture['actual']))
