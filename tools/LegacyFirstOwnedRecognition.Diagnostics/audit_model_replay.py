from pathlib import Path
import ast,json,hashlib,importlib.util,struct,sys
R=Path(sys.argv[1]);I=json.loads(Path(sys.argv[2]).read_text());D=json.loads(Path(sys.argv[3]).read_text());AK=Path(I['ak.res']['path']).read_bytes();profile=json.loads(Path(I['engine.json']['path']).read_text())
for n,p in I.items():assert hashlib.sha256(Path(p['path']).read_bytes()).hexdigest()==p['sha256'],n
def require_coverage(data):
 if len(data)!=4 or [d['fixture']for d in data]!=[1,2,3,4]:raise ValueError('Exact four ordered actual fixture identities required')
 if [d['groups']for d in data]!=[42,3,6,6]:raise ValueError('Exact declared owning coverage required')
 for d in data:
  if len(d['modelGroups'])!=d['groups']:raise ValueError('Actual model group count differs')
  if [g['ordinal']for g in d['modelGroups']]!=list(range(d['groups'])):raise ValueError('Unique complete ordered group identities required')
 if sum(len(d['modelGroups'])for d in data)!=57:raise ValueError('Actual57 group coverage required')
require_coverage(D)

def defs(p):
 t=ast.parse(p.read_text(encoding='utf8'));t.body=[n for n in t.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))];s={'__file__':str(p)};exec(compile(t,str(p),'exec'),s);return s
feature=defs(R/'tools/LegacyEnglishNoInitialGlyph.Probe/feature_oracle.py')['extract'];nn=defs(R/'tools/LegacyFreshMainAcoustic.Probe/audit_output.py');cost=defs(R/'tools/LegacyCostEnsemble.Probe/reference_controls.py')['reference'];spec=importlib.util.spec_from_file_location('mixed',R/'tools/handwriting/ru_first_mixed_probability.py');mixed=importlib.util.module_from_spec(spec);spec.loader.exec_module(mixed);prob=mixed.for_russian_first(AK);report=[]
for d in D:
 for g in d['modelGroups']:
  pts=[tuple(p)for s in g['merged']for p in s]
  if not pts:raise ValueError('Missing actual merged glyph')
  if all(p==pts[0]for p in pts):
   #Native00A9F0 exactfirst-source assignment, independently authenticated by
   #existing degenerate audit; not a zero-placeholder normalizer fallback.
   trajectory=[0]*217;trajectory[6:36]=[1024]*30;stats=[1024,-768,0,0,-768,1024,-1024];trajectory[210:217]=stats;raster=stats+[-1024]*64+[1024]*128
  else:trajectory,raster=feature(g['merged'])
  assert len(trajectory)==217 and len(raster)==199 and len(g['context'])==28
  raw=[nn['network'](AK,off,v+g['context']+g['boundaries'],profile['activations']['hidden'])for off,v in [(19195,trajectory),(105197,raster)]];q=[prob(v)for v in raw]
  assert q[0]==g['trajectoryProbabilities'] and q[1]==g['rasterProbabilities'],(d['fixture'],g['ordinal'],'native mixedQ15')
  integer=cost([dict(probabilities=v,weight=1)for v in q]+[None]*5,1,[]);assert integer==g['integerCosts'],(d['fixture'],g['ordinal'],'integer ensemble')
  assert len(g['cacheCosts'])==195
 report.append({'fixture':d['fixture'],'groups':len(d['modelGroups']),'networks':2*len(d['modelGroups'])})
assert sum(r['groups']for r in report)==57 and sum(r['networks']for r in report)==114
print('PASS independent actualOWNED all57groups/114 trajectory+raster networks/Q15/ensemble; context28 are actualsource-derived captured diagnostic inputs',report)
