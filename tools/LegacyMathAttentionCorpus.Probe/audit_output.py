"""Independent report/vocabulary/format comparison; no accuracy golden or NN."""
from pathlib import Path
import hashlib,importlib.util,json,re,sys
if len(sys.argv)!=4:raise SystemExit('controlReport rawMath2 corpusManifest')
report=Path(sys.argv[1]);r=json.loads(report.read_bytes());baseline=json.loads(Path(str(report)+'.baseline.json').read_bytes())
manifest=Path(sys.argv[3]);assert hashlib.sha256(manifest.read_bytes()).hexdigest()==r['manifestSHA256']==baseline['manifestSha256']
truth=json.loads(manifest.read_bytes());expected={s['id']:s['expected']for s in truth['samples']};old={s['id']:s for s in baseline['samples']}
spec=importlib.util.spec_from_file_location('wire',Path(__file__).resolve().parents[1]/'export-ink-math-model.py');m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
labels,_=m.parse(Path(sys.argv[2]).read_bytes())
def canonical(t):
 if t is None:return None
 t=re.sub(r'\\(?:left|right)(?![A-Za-z])','',t);t=re.sub(r'\s+','',t)
 for a,z in [(r'\dfrac',r'\frac'),(r'\tfrac',r'\frac'),(r'\longrightarrow',r'\to'),(r'\rightarrow',r'\to'),('\u039b',r'\Lambda'),('||',r'\Vert'),(r'\\\end{matrix}',r'\end{matrix}')]:t=t.replace(a,z)
 return t
assert canonical(r'\int_{\gamma}x')!=canonical(r'\int_{y}x')
assert canonical('F(x)')!=canonical('f(x)')
assert canonical(r'\rightarrow0')==canonical(r'\to0')
counts=[0,0];changed=[];offmatches=0
assert len(r['samples'])==len(expected)==len(old)and len(expected)<=128
for sample in r['samples']:
 key=sample['id'];assert key in expected and old[key]['expected']==expected[key]
 assert [c['force']for c in sample['controls']]==[False,True]
 assert sample['frames']==old[key]['frames']
 for i,c in enumerate(sample['controls']):
  assert all(isinstance(k,int)and 2<=k<297 for k in c['symbols'])
  assert c['rawText']==''.join(labels[k]for k in c['symbols'])
  assert len(c['symbols'])<=128 and(c['completed']or c['latex']is None)
  counts[i]+=canonical(c['latex'])==canonical(expected[key])
 off,on=sample['controls'];offmatches+=off['rawText']==old[key]['rawText']and off['completed']==old[key]['completed']
 if off['symbols']!=on['symbols']:changed.append(key)
stats=dict(samples=len(expected),forceOffMatchesBaseline=offmatches,formatNormalizedMatches=dict(forceOff=counts[0],forceOn=counts[1]),changedSamples=changed,
 scope='Diagnostic argmax128 controls only; no full search/recognition acceptance or quality threshold')
report.with_suffix('.summary.json').write_text(json.dumps(stats,indent=2)+'\n',encoding='utf8')
print(json.dumps(stats))
