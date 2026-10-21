"""Independent raw107 matrices, float257 weights, actual217 AL/case and MAIN cache oracle.
Recorded vectors are audited NN inputs, not an assertion of whole feature/native decoder equivalence.
No targets or vendor executable code are read or run.
"""
import ast,hashlib,json,math,struct,sys
from pathlib import Path
import numpy as np
ROOT=Path(__file__).resolve().parents[2]
def defs(path):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));tree.body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))];scope={};exec(compile(tree,str(path),'exec'),scope);return scope
cost=defs(ROOT/'tools/LegacyCostEnsemble.Probe/reference_controls.py')['cost']
dynamic=defs(ROOT/'tools/LegacyDynamicWeights.Probe/reference_controls.py')['reference']
read=defs(ROOT/'tools/LegacyCaseAlphabetRanges.Probe/reference_controls.py')['read']
exp_prob=defs(ROOT/'tools/LegacyFreshMainAcoustic.Probe/audit_output.py')['exp_prob']
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def i32(v):return (int(v)+2**31)%2**32-2**31
def rnd(x):return int(x+(.5 if x>0 else -.5))
IMP=0x4000000
ak=Path(sys.argv[1]).read_bytes();ref=json.loads(Path(sys.argv[2]).read_text());capture=json.loads(Path(sys.argv[3]).read_text());assert hashlib.sha256(ak).hexdigest()==ref['resourceSha256']=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
assert capture['TargetsRead'] is False
al=read(ak[99:]);labels={t:i for i,t in al['text'].items()};count=al['count'];header=struct.unpack_from('<8H',ak,99);assert count==217
forms_at=al['positions'][0]-4-al['forms']*6;forms=[list(struct.unpack_from('<3H',ak,99+forms_at+i*6))for i in range(al['forms'])];flag_at=forms_at-2-(count+7)//8;flagbits=ak[99+flag_at:99+forms_at-2];relation=al['maps'][0];mask=ref['maskDiagnostic']['primary'];v=ref['values']
weights=[struct.unpack_from('<H',ak,184181)[0]]+list(struct.unpack_from('<8f',ak,184183))
#257 dynamic prefix starts immediately after actual MAIN optional40 flag.
assert weights[0]==30
nets=[]
for n in ref['networks']:
 layers=[]
 for m in n['matrices']:
  a=np.frombuffer(ak,dtype=np.int8,count=m['rows']*m['stride'],offset=m['weights']).reshape(m['rows'],m['stride'])[:,:m['columns']].astype(np.int64)
  scales=np.frombuffer(ak,dtype=np.uint8,count=m['rows'],offset=m['scales']+1).astype(np.int64);layers.append((a,scales))
 nets.append(layers)
lut=[int(math.floor(1024*math.tanh(3*i/47)))for i in range(48)]
def network(layers,vector):
 w,s=layers[0];dots=[i32(i32(x)*scale)for x,scale in zip(w@np.array(vector,dtype=np.int64),s)];hidden=[1024]+[(1 if x>0 else -1)*(lut[abs(x)>>16]if abs(x)>>16<48 else 1018)for x in dots]
 w,s=layers[1];raw=[i32(i32(x)*scale)for x,scale in zip(w@np.array(hidden,dtype=np.int64),s)]
 ordinary=raw[:180]+raw[181:];prob=exp_prob(ordinary);arg=f(f(f(raw[180])*-2)/1048576);special=0 if arg>75 else int(32768/(math.exp(arg)+1)+.5);prob=prob[:180]+[special]+prob[180:]
 return dots,hidden,raw,prob

def ensemble(probabilities,weight):
 total=0.;out=[0]*182
 for probs in probabilities:
  total=f(total+weight)
  for i,p in enumerate(probs):
   if mask[i]:out[i]+=cost(p)if weight==1 else int(f(f(cost(p))*weight))
 best=-1;bestcost=IMP
 for i,c in enumerate(out):
  if i not in (180,181)and mask[i]and c<bestcost:best,bestcost=i,c
 if best>=0 and flagbits[best>>3]&(1<<(best&7))and relation[best]>=0:
  triple=forms[relation[best]].copy()
  if best not in triple:
   for i in (0,1):
    if triple[i]==65535:triple[i]=best;break
  a,b=triple[:2]
  if a<182 and b<182 and mask[a]and mask[b]:
   pa=rnd(f(f(total*v['84'])*8192));pb=rnd(f(f(total*v['88'])*8192))
   if out[a]+pb<out[b]:out[b]=out[a]+pb
   elif out[b]+pa<out[a]:out[a]=out[b]+pa
 inv=f(1/total)if total>0 else 0
 out=[IMP if not mask[i]or total<=0 else min(IMP,c if total==1 else c>>1 if total==2 else int(f(f(c)*inv)))for i,c in enumerate(out)];out[181]=IMP
 return out

def cache(neural,g):
 out=neural.copy()+[IMP]*(220-len(neural))
 def enabled(t):return t in labels and mask[labels[t]]
 def assign(t,c):
  if enabled(t):out[labels[t]]=min(IMP,c)
 def prob(c):return f(math.exp(f(f(-c)*f(1/8192))))if c<IMP else 0.
 dash=header[1]
 if out[dash]<IMP:
  base=out[dash];assign('-',base);assign('_',base)
  for t,weight in [('\u30fc',v['a4']),('\u4e00',v['a8'])]:assign(t,IMP if weight<=0 else base+rnd(-math.log(weight)*8192))
  if enabled('\U000f0002')or enabled('\U000f0003'):
   neg=pos=0.
   for stroke in g['Merged']:
    for a,b in zip(stroke,stroke[1:]):
     dx,dy=f(b[0]-a[0]),f(b[1]-a[1]);length=f(math.hypot(dx,dy))
     if dx<0:neg=f(neg+length)
     elif dx>0:pos=f(pos+length)
     else:neg=f(neg+f(length*.5));pos=f(pos+f(length*.5))
   total=f(pos+neg)
   if abs(total)<=max(f(abs(total)*f(2.3841858e-7)),f(1.1920929e-7)):neg=pos=.5
   else:neg=f(neg/total);pos=f(pos/total)
   for t,p,d in [('\U000f0002',neg,v['ac']),('\U000f0003',pos,v['b0'])]:assign(t,IMP if p<=f(.1)else min(IMP,base+rnd(-math.log(f(p/d))*8192)))
  out[dash]=IMP
 assert header[2]==65535
 if len(g['Merged'])==1 and g['WholePhysicalStroke']and enabled('\U000f0001'):
  p=g['SourceProbability'];assert p is not None;assign('\U000f0001',IMP if p<=0 else rnd(-math.log(p)*v['b4']*8192))
 if '\ufffd'in labels:out[labels['\ufffd']]=IMP
 out[217]=out[180]-int(f(v['7C']*-8192));out[180]=IMP
 points=[p for stroke in g['Merged']for p in stroke];extent=max(max(p[i]for p in points)-min(p[i]for p in points)for i in(0,1))
 for text,threshold in [('.',f(100*v['BC']))]+[(t,f(100*v['C0']))for t in [',',"'",'"','\u3001']]:
  if extent>threshold and text in labels:out[labels[text]]+=rnd(f(f(f(extent/threshold)-1)*8192))
 if len(g['Merged'])==1 and g['WholePhysicalStroke']and g['EndpointKeysZero']and enabled('.'):out[labels['.']]=min(out[:217])-1
 for group in ref['groups']:
  weight=group['weight'];maximum=max([prob(out[i])for i in group['symbols']if mask[i]]or[0.])
  if weight>0 and maximum>0:
   for i in group['symbols']:
    if mask[i]:mix=f(f(f(1-weight)*prob(out[i]))+f(weight*maximum));out[i]=rnd(-math.log(mix)*8192)
 for t,i in [("'",218),('"',219)]:
  if t in labels:out[i]=out[labels[t]]
 def fill(upper):
  heap=[];cap=v['EE']
  for i,c in enumerate(out):
   if i==217 or c>=upper:continue
   item=[i,c]
   if len(heap)==cap:
    if heap[0][1]<=c:continue
    p=0
    while p*2<len(heap)-1:
     child=p*2+1
     if child<len(heap)-1 and heap[child][1]<=heap[child+1][1]:child+=1
     if heap[child][1]<=c:break
     heap[p]=heap[child];p=child
   else:
    p=len(heap);heap.append(None)
    while p>0:
     parent=(p-1)>>1
     if c<=heap[parent][1]:break
     heap[p]=heap[parent];p=parent
   heap[p]=item
  return heap
 retained=fill(min(v['E8'],cost(1)));rewrite=True
 if len(retained)<v['EC']:
  retained=fill(cost(1)*2)
  if len(retained)<v['EC']:retained=fill(IMP)
  rewrite=len(retained)>=v['EC']
 if rewrite:
  fallback,special=out[217],out[180];out=[IMP]*220;out[217]=fallback;out[180]=special
  for i,c in retained:out[i]=c
 count=g['Span']+1;power=1. if count==1 else f(math.pow(count,v['100']));fallbackpower=math.pow(count,1.2000000476837158)
 if count!=1:
  for i,c in enumerate(out):
   if c<IMP:out[i]=min(IMP,int(c*fallbackpower)if i==217 else int(f(f(c)*power)))
 candidates=[[i,out[i]]for i,c in retained if i<217]
 def sort(lo,hi):
  left,right=lo,hi;pivot=candidates[(lo+hi)//2][1]
  while left<=right:
   while candidates[left][1]<pivot:left+=1
   while candidates[right][1]>pivot:right-=1
   if left>right:break
   candidates[left],candidates[right]=candidates[right],candidates[left];left+=1;right-=1
  if lo<right:sort(lo,right)
  if left<hi:sort(left,hi)
 if len(candidates)>1:sort(0,len(candidates)-1)
 if '\u00ad'in labels and '-'in labels:out[labels['\u00ad']]=out[labels['-']]
 return out,candidates,power

import importlib.util
feature_path=Path(__file__).with_name('reference_features.py');spec=importlib.util.spec_from_file_location('main_features_independent',feature_path);feature=importlib.util.module_from_spec(spec);spec.loader.exec_module(feature);audit_features=feature.audit;assembly=feature.assembly
for control in capture.get("PreparedAssemblyControls",[]):
 assert assembly(control["Global"],control["Windows"],control["Baseline"],False)==control["Temporal"]
 assert assembly(control["Global"],control["Windows"],control["Baseline"],True)==control["Spatial"]
assert capture['CalibrationGuards']==6
for name,key in [('TemporalMin','temporalMin'),('TemporalMax','temporalMax'),('SpatialMin','spatialMin'),('SpatialMax','spatialMax')]:
 assert capture['CalibrationCatalog'][name]==feature.CALIBRATION[key]['values']
for control in capture['PreparedCalibrationControls']:
 for spatial,key in [(False,'Temporal'),(True,'Spatial')]:
  assert assembly(control['Global'],control['Windows'],control['Baseline'],spatial,control['Minima'],control['Maxima'])==control[key]
assert len(capture['PreparedCalibrationControls'])==4
checked=0
for row in capture['Rows']:
 for g in row['Groups']:
  audit_features(row,g);identity=(row['Name'],g['Group']);assert list(map(len,g['Inputs']))==[228,210,285,285]
  for slot,(layers,vector,recorded)in enumerate(zip(nets,g['Inputs'],g['Networks'])):
   expected=network(layers,vector)
   for key,value in zip(['HiddenDots','Hidden','Raw','Probabilities'],expected):assert value==recorded[key],(identity,slot,key)
  stored=[[[f(v)for v in row['CutInk'][s][i]]for i in range(a,z)]for s,a,z in g['Stored']];dyn=dynamic(stored,weights)
  assert all(abs(a-b)<=1e-7 for a,b in zip(dyn[:3],g['Dynamic'])),(identity,'dynamic',dyn,g['Dynamic'])
  neural=ensemble([n['Probabilities']for n in g['Networks']],f(g['Dynamic'][2]));assert neural==g['NeuralCosts'],(identity,'ensemble')
  final,candidates,power=cache(neural,g);assert final==g['Costs'],(identity,'cache',[(i,a,b)for i,(a,b)in enumerate(zip(final,g['Costs']))if a!=b])
  assert candidates==g['Candidates'],(identity,'ranking');assert power==f(g['PowerScale']);assert int(f(f(-cost(1))*power))==g['NegativeBaselineCost']
  checked+=1
 group_spans={g['Group']:g['Span']for g in row['Groups']};assert len(group_spans)==len(row['Groups']);assert all(group in group_spans and span==group_spans[group]for node in row['Graph']for group,span in node)
assert checked==capture['GroupCount']
print('PASS independent raw MAIN107 x4, storedfloat257, actualAL217/case80, primitives/B8/orderedgroups/prune/spanpower/ranking;',len(capture['Rows']),'owners',checked,'groups',checked*4,'NNs; no targets/native code')
