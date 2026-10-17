"""Independent raw vocabulary and force-column checks; no model execution."""
import base64,hashlib,importlib.util,json,struct,sys
from pathlib import Path
if len(sys.argv)!=3:raise SystemExit('capture rawMath2.res')
r=json.loads(Path(sys.argv[1]).read_bytes());raw=Path(sys.argv[2]).read_bytes()
spec=importlib.util.spec_from_file_location('wire',Path(__file__).resolve().parents[1]/'export-ink-math-model.py')
m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
labels,_=m.parse(raw)
b=b''.join(struct.pack('<I',len(s.encode()))+s.encode()for s in labels)
assert hashlib.sha256(b).hexdigest()==r['VocabularyHash']
assert r['Guards']==10 and r['Force']is True and r['Offset']==1 and r['SecondOffset']==0
shape=r['AttentionShape'];assert len(shape)==3 and shape[0]>0 and shape[1:]==[1,1]
assert base64.b64decode(r['Attention'],validate=True)==struct.pack('<I',0xff7fffff)*shape[0]
assert r['MaximumPredictionCount']>=0
print('PASS independent raw vocabulary and actual cold force attention bytes;10 guards')
