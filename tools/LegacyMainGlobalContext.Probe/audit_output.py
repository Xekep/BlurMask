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
ak=Path(sys.argv[1]).read_bytes();engine=json.loads(Path(sys.argv[2]).read_text());cases=json.loads(Path(sys.argv[3]).read_text())
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
offset,lv,rv,reverse,maxwindow,blank,cutoff,retain,selector,pointmean,extended=struct.unpack_from('<3fBBHf4B',ak,195746)
assert (reverse,maxwindow,blank,retain,selector,pointmean,extended)==(0,3,28,1,2,0,1)
window_count=group_count=accepted_count=0;descriptor_diff=False
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
  target=f(total/count);context=[0.]*28
  for descriptor,center,probability in accepted:
   if descriptor!=g['descriptor']:continue
   distance=f(f(center-target)/d['height'])
   if distance>=0:exponent=f(f(f(distance*distance)*f(-.5))/rv)
   else:
    distance=f(distance+offset);exponent=f(f(f(distance*distance)*f(-.5))/lv)if distance<=0 else 0.
   if exponent< -75:continue
   weight=f(math.exp(exponent));context=[max(a,f(weight*b))for a,b in zip(context,probability)]
  context=[f(f(v+v)-1)for v in context];context=[-1. if v<f(-.99)else v for v in context]
  assert fixed(context)==g['context'];group_count+=1
assert len(cases)==8 and descriptor_diff and window_count==47 and accepted_count==21 and group_count==631
print(f'PASS independent8 shared sources/{window_count} rawfloat227 windows/{accepted_count} accepted/{group_count} weightedcontexts; exactNN/probabilities/fixedchannels, descriptor IDs distinct fromline IDs')
