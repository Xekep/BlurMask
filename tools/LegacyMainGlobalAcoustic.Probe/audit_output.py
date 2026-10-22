"""Independent raw-resource case masks, affine geometry, four NNs and finalcache."""
import ast,hashlib,json,struct,sys
from pathlib import Path
import numpy as np
ROOT=Path(__file__).resolve().parents[2]
def definitions(path):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));tree.body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))]
 scope={};exec(compile(tree,str(path),'exec'),scope);return scope
old=definitions(ROOT/'tools/LegacyMainSingleLineIngress.Probe/audit_output.py');cache=old['cache'];labels=old['labels']
nn=definitions(ROOT/'tools/LegacyFreshMainAcoustic.Probe/audit_output.py');network=nn['network'];exp_prob=nn['exp_prob']
dynamic=definitions(ROOT/'tools/LegacyDynamicWeights.Probe/reference_controls.py')['reference']
costs=definitions(ROOT/'tools/LegacyCostEnsemble.Probe/reference_controls.py')['reference']
read=definitions(ROOT/'tools/LegacyCaseAlphabetRanges.Probe/reference_controls.py')['read']
def f(x):return float(np.float32(x))
ak=Path(sys.argv[1]).read_bytes();lk=Path(sys.argv[2]).read_bytes();engine=json.loads(Path(sys.argv[3]).read_text());cases=json.loads(Path(sys.argv[4]).read_text())
actual_inputs=old['independent_actual2b1_inputs'](ROOT,engine)
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert hashlib.sha256(lk).hexdigest()=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
#Independent raw AL/LK maps and232C80 triplet callbacks, including duplicates.
raw=ak[99:];a=read(raw);lex=read(lk[607074:]);bytext={v:k for k,v in a['text'].items()}
control={'\0':65533,' ':65531,'\n':65529,'\u00a0':65532,'\ufffd':65535,'\U000f000c':65530}
primary=[65535 if lex['reverse'][i]==65535 else control.get(lex['text'][i],bytext.get(lex['text'][i],65535))for i in range(lex['count'])]
at=a['positions'][0]-4;flags=struct.unpack_from('<I',raw,at)[0];forms=[list(struct.unpack_from('<3H',raw,at-a['forms']*6+i*6))for i in range(a['forms'])];maps={};it=iter(a['maps'])
for mode in range(3):
 if flags&(1<<mode):maps[mode]=next(it)
def related(label,mode):
 m=maps.get(mode,maps.get(0))
 if m is None or m[label]<0:return None
 out=forms[m[label]].copy()
 if label not in out:
  for i in (0,1):
   if out[i]==65535:out[i]=label;break
 return out
callbacks=[]
for label in primary:
 first=related(label,0)if label<a['count']else None
 if first is None:callbacks.append(label);continue
 for mode in range(3):
  out=related(label,mode)
  if out is not None:callbacks.extend(v for v in out if v!=65535)
mask=[0]*a['count']
for v in callbacks:
 if 65507<=v<=65514:mask=[1]*len(mask)
 elif v<len(mask):mask[v]=1
#Exact v110 knowledge tail after the independently pinned four-family prefix.
pos=566330
def take(fmt):
 global pos
 v=struct.unpack_from('<'+fmt,ak,pos)[0];pos+=struct.calcsize('<'+fmt);return v
pos+=14
for _ in range(take('H')):
 n=take('H');pos+=n*2
pos+=24;assert take('B')==0;pos+=8;assert take('B')==0;count=take('h');assert 0<=count<=512
if count:
 ends=[take('H')for _ in range(count+1)];members=[take('H')for _ in range(ends[-1])];weight=take('f')
 if weight>0:
  for i in range(count):
   group=members[ends[i]:ends[i+1]]
   if any(mask[v]for v in group):
    for v in group:mask[v]=1
for text in ['-','\u00ad']:
 if text in bytext:mask[bytext[text]]=1
for i in range(len(mask)):
 if a['reverse'][i]==65535:mask[i]=0
header,label=labels(ak)
mask[header[1]]=int(any(mask[bytext[t]]for t in ['-','_','\U000f0002','\U000f0003','\u30fc','\u4e00']if t in bytext))
if header[2]!=65535:mask[header[2]]=mask[bytext['\U000f0001']]
for slot in [3,0,4,5,6]:
 if header[slot]!=65535:mask[header[slot]]=1
assert sum(mask)==145
weights_profile=[struct.unpack_from('<H',ak,195771)[0]]+list(struct.unpack_from('<8f',ak,195773));groups=0
for case in cases:
 assert case['mask']==mask and case['target']==100
 points=[[[f(v)for v in p]for p in s]for s in case['points']];cache_profile=case['cacheProfile']+case['cacheCaps']
 for g in case['groups']:
  baseline=f(g['primaryBaseline']);height=f(g['primaryHeight'])
  if 'secondaryBaseline'in g:
   baseline=max(baseline,f(g['secondaryBaseline']));top=min(f(g['primaryBaseline']-g['primaryHeight']),f(g['secondaryBaseline']-g['secondaryHeight']));height=f(baseline-top)
  assert f(g['baseline'])==baseline and f(g['height'])==height
  assert g['startKeys']==sorted(g['startKeys'])
  rawstrokes=[points[s][a:z]for s,a,z in g['ranges']];minimum=min(p[0]for s in rawstrokes for p in s);assert minimum==f(g['minX'])
  expected=[[[min(32767,int(f(f(f(p[0]-minimum)*case['target'])/g['height']))),min(32767,int(f(f(f(p[1]-g['baseline'])*case['target'])/g['height'])))]for p in s]for s in rawstrokes]
  assert expected==g['normalized'],('normalize',groups)
  for vector in g['inputs']:assert vector[-30:-2]==g['context']and vector[-2:]==g['boundaries']
  reconstructed=actual_inputs(expected,g['context'],g['boundaries'])
  assert list(map(len,reconstructed))==[256,238,313,313]
  assert reconstructed==g['inputs'],('actual2B1_features',groups)
  probabilities=[exp_prob(network(ak,n,v,engine['activations']['hidden']))for n,v in zip([195895,283697,367899,467101],reconstructed)]
  assert probabilities==g['probabilities'],('NN',groups)
  dyn=dynamic(rawstrokes,weights_profile);np.testing.assert_allclose(g['weights'][:4],[dyn[2]]*4,rtol=1e-6,atol=1e-6)
  integer=costs([dict(probabilities=p,weight=f(g['weights'][i]))for i,p in enumerate(probabilities)]+[None]*3,1,mask[:180]);assert integer==g['integerCosts']
  expected_cache,candidates=cache(integer,g['normalized'],mask,header,label,cache_profile,g['whole'],g['zero']);assert expected_cache==g['cacheCosts'],('cache',groups)
  assert candidates==sorted(g['candidates']);groups+=1
assert len(cases)==8 and groups==631
print(f'PASS independent145 rawcase-mask labels/{len(callbacks)} callbacks,{groups} rawfloat-normalized groups/{groups*4} actualNNs/dynamicweights/costs/final195cache; noexpected text')
