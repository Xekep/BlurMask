"""Independent per-descriptor227 geometry/network and context weighting audit."""
import ast,hashlib,json,math,struct,sys
from pathlib import Path
import numpy as np
ROOT=Path(__file__).resolve().parents[2]
def definitions(path):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));tree.body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))]
 scope={};exec(compile(tree,str(path),'exec'),scope);return scope
old=definitions(ROOT/'tools/LegacyFreshMainAcoustic.Probe/audit_output.py');network=old['network'];exp_prob=old['exp_prob']
geometry=definitions(ROOT/'tools/LegacyFirstRowContext.Probe/reference_controls.py')['reference']
def f(x):return float(np.float32(x))
def fixed(v):return [((int(f(x*1024))+32768)&65535)-32768 for x in v]
native=definitions(ROOT/'tools/LegacyMainGlobalContext.Probe/audit_output.py')
ak=Path(sys.argv[1]).read_bytes();engine=json.loads(Path(sys.argv[2]).read_text());cases=json.loads(Path(sys.argv[3]).read_bytes())
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
offset,lv,rv,reverse,maxwindow,blank,cutoff,retain,selector,pointmean,extended=struct.unpack_from('<3fBBHf4B',ak,195746)
assert (reverse,maxwindow,blank,retain,selector,pointmean,extended)==(0,3,28,1,2,0,1)
combine=native['combine'];combine.__globals__.update(offset=offset,lv=lv,rv=rv)
window_count=group_count=accepted_count=nonneutral_cross=0;descriptor_diff=False
assert all(c['expectedLabelsRead'] is False for c in cases)
for case in cases:
 points=[[[f(v)for v in p]for p in s]for s in case['points']];owners={d['ordinal']:d for d in case['descriptors']};accepted=[];expected_windows=[]
 for d in case['descriptors']:
  descriptor_diff|=d['ordinal']!=d['line']
  for start in range(d['first']+1,d['last']+1):
   for end in range(start,min(start+maxwindow,d['last']+1)):expected_windows.append((d['ordinal'],start,end))
 assert [(w['descriptor'],w['start'],w['end'])for w in case['windows']]==expected_windows
 for w in case['windows']:
  d=owners[w['descriptor']];start,end=w['start'],w['end'];assert w['line']==d['line']
  reference=max(p[0]for s in points[d['first']:start]for p in s)
  features=geometry(points[start:end+1],d['height'],reference,d['baseline'],False,False)
  np.testing.assert_allclose(w['features'],features,rtol=2e-6,atol=2e-6)
  assert fixed([f(x)for x in features])==w['fixed']
  assert fixed([f(x)for x in w['features']])==w['fixed']
  probabilities=exp_prob(network(ak,189755,w['fixed'],engine['activations']['hidden']));assert probabilities==w['probabilities']
  mass=f(0.)
  for i,p in enumerate(probabilities):
   if i!=blank-1:mass=f(mass+f(p/32768))
  take=cutoff<mass;assert take==w['accepted'];center=f(0.);count=0
  for s in points[start:end+1]:
   for p in s:center=f(center+p[0]);count+=1
  center=f(center/count);assert center==f(w['center'])
  if take:accepted.append((d['ordinal'],center,[f(x/32768)for x in probabilities]));accepted_count+=1
  window_count+=1
 for g in case['groups']:
  d=owners[g['descriptor']];total=f(0.);count=0
  for s,a,z in g['ranges']:
   for p in points[s][a:z]:total=f(total+p[0]);count+=1
  target=f(total/count);observations=[(center,probability)for descriptor,center,probability in accepted if owners[descriptor]['line']==d['line']]
  context=combine(observations,target,d['height']);assert context==g['context'];group_count+=1
  if g['secondary']>=0:
   second=owners[g['secondary']];base=max(d['baseline'],second['baseline']);height=f(base-min(f(d['baseline']-d['height']),f(second['baseline']-second['height'])))
   wrong=combine(observations,target,height)
   nonneutral_cross+=any(v!=-1024 for v in context)and context!=wrong

report={'scope':'Actual PluColl Latin physical trajectories through Russian227 owner; no labels or Russian accuracy claim',
'captureSha256':hashlib.sha256(Path(sys.argv[3]).read_bytes()).hexdigest(),'sources':len(cases),'windows':window_count,'acceptedWindows':accepted_count,'groups':group_count,'crossFrameDiscriminators':nonneutral_cross,'descriptorLineDifference':descriptor_diff}
print(json.dumps(report,indent=2))
if len(sys.argv)>4: Path(sys.argv[4]).write_text(json.dumps(report,indent=2)+'\n')
