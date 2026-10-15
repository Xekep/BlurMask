"""Independent float-context geometry/7001/four-family NN/cost/cache audit."""
import ast,hashlib,json,math,struct,sys
from pathlib import Path
import numpy as np
ROOT=Path(__file__).resolve().parents[2]
def definitions(path):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));tree.body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))]
 scope={};exec(compile(tree,str(path),'exec'),scope);return scope
old=definitions(ROOT/'tools/LegacyFreshMainAcoustic.Probe/audit_output.py');network=old['network'];exp_prob=old['exp_prob']
geometry=definitions(ROOT/'tools/LegacyFirstRowContext.Probe/reference_controls.py')['reference']
dynamic=definitions(ROOT/'tools/LegacyDynamicWeights.Probe/reference_controls.py')['reference']
costs=definitions(ROOT/'tools/LegacyCostEnsemble.Probe/reference_controls.py')['reference']
def f(x):return float(np.float32(x))
def fixed(values):return [((int(f(v*1024))+32768)&65535)-32768 for v in values]
def labels(data):
 at=99
 def u(fmt):
  nonlocal at
  v=struct.unpack_from('<'+fmt,data,at)[0];at+=struct.calcsize('<'+fmt);return v
 header=[u('H') for _ in range(8)];n=u('H')
 for inc in [32,64,1]:n=next(v for v in reversed([u('H') for _ in range(n)]) if v!=65535)+inc
 spans=[u('H') for _ in range(n+1)];entries=[(u('I'),u('I')&65535) for _ in range(spans[-1]+1)];blob=data[at:at+entries[-1][0]]
 return header,{blob[a:b].decode('utf8'):label for (a,label),(b,_) in zip(entries,entries[1:])}
def cache(raw,strokes,mask,header,label,p,whole,zero):
 IMP=0x4000000;fallback,longdash,cjk,nd,pd,closed,threshold,minimum,capacity=p;width=len(mask)
 result=[v if mask[i] else IMP for i,v in enumerate(raw)]+[IMP]*(width+3-len(raw));result[header[0]]=IMP
 def enabled(text):return text in label and mask[label[text]]
 def assign(text,value):
  if enabled(text):result[label[text]]=min(IMP,value)
 def rounded(v):return int(v+(.5 if v>0 else -.5))
 def logarithm(base,p):return IMP if p<=0 else min(IMP,base+rounded(-math.log(p)*8192))
 if result[header[1]]<IMP:
  base=result[header[1]];assign('-',base);assign('_',base);assign('\u30fc',logarithm(base,longdash));assign('\u4e00',logarithm(base,cjk))
  negative=positive=0.
  for points in strokes:
   for a,b in zip(points,points[1:]):
    dx=f(b[0]-a[0]);dy=f(b[1]-a[1]);length=f(math.hypot(dx,dy))
    if dx<0:negative=f(negative+length)
    elif dx>0:positive=f(positive+length)
    else:negative=f(negative+f(length*.5));positive=f(positive+f(length*.5))
  total=f(positive+negative)
  if abs(total)<=max(f(abs(total)*f(2.3841858e-7)),f(1.1920929e-7)):negative=positive=.5
  else:negative=f(negative/total);positive=f(positive/total)
  assign('\U000f0002',IMP if negative<=f(.1) else logarithm(base,f(negative/nd)))
  assign('\U000f0003',IMP if positive<=f(.1) else logarithm(base,f(positive/pd)));result[header[1]]=IMP
 if whole:
  #All source2AD probabilities for this actual nonenclosing corpus are0.
  assign('\U000f0001',IMP)
 assign('\ufffd',IMP);result[width]=result[header[3]]-int(f(fallback*-8192));result[header[3]]=IMP
 if whole and zero and enabled('.'):assign('.',min(result[:width])-1)
 if "'" in label:result[width+1]=result[label["'"]]
 if '"' in label:result[width+2]=result[label['"']]
 #Independent bounded priority queue, retaining native equal-cost identities.
 def queue(upper):
  heap=[]
  for symbol,value in enumerate(result):
   if symbol==width or value>=upper:continue
   item=(symbol,value)
   if len(heap)<int(capacity):
    heap.append(item);index=len(heap)-1
    while index:
     parent=(index-1)//2
     if heap[parent][1]>=value:break
     heap[index]=heap[parent];index=parent
   else:
    if value>=heap[0][1]:continue
    index=0
    while index*2+1<len(heap):
     child=index*2+1
     if child+1<len(heap)and heap[child][1]<=heap[child+1][1]:child+=1
     if heap[child][1]<=value:break
     heap[index]=heap[child];index=child
   heap[index]=item
  return heap
 heap=queue(min(threshold,85173));rewrite=True
 if len(heap)<minimum:
  heap=queue(170346)
  if len(heap)<minimum:heap=queue(IMP)
  rewrite=len(heap)>=minimum
 if rewrite:
  saved=result[width];result=[IMP]*(width+3);result[width]=saved
  for symbol,value in heap:result[symbol]=value
 candidates=sorted([i,result[i]] for i,_ in heap if i<width)
 if '\u00ad' in label and '-' in label:result[label['\u00ad']]=result[label['-']]
 return result,candidates
if len(sys.argv)!=4:raise ValueError('AK.res engine-profile.json output.json')
ak=Path(sys.argv[1]).read_bytes();engine=json.loads(Path(sys.argv[2]).read_text());data=json.loads(Path(sys.argv[3]).read_text())
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
points=[[[f(v) for v in p] for p in stroke] for stroke in data['points']];activation=engine['activations']['hidden'];accepted=[];reference=-np.finfo(np.float32).max
offset,lv,rv,maximum,maxwindow,blank,cutoff,retain,selector,pointmean,extended=struct.unpack_from('<3fBBHf4B',ak,195746)
assert (maximum,maxwindow,blank,retain,selector,pointmean,extended)==(0,3,28,1,2,0,1)
for window in data['windows']:
 start,end=window['start'],window['end'];reference=max(p[0] for stroke in points[:start] for p in stroke)
 features=geometry(points[start:end+1],data['height'],reference,data['baseline'],False,False)
 np.testing.assert_allclose(window['features'],features,rtol=2e-6,atol=2e-6,err_msg=f'contextgeometry{start}/{end}')
 assert fixed([f(v) for v in window['features']])==window['fixed']
 probability=exp_prob(network(ak,189755,window['fixed'],activation));assert probability==window['probabilities']
 mass=f(0.)
 for i,p in enumerate(probability):
  if i!=blank-1:mass=f(mass+f(p/32768))
 take=cutoff<mass;assert take==window['accepted'];center=f(0.);count=0
 for stroke in points[start:end+1]:
  for p in stroke:center=f(center+p[0]);count+=1
 center=f(center/count);assert center==f(window['center'])
 if take:accepted.append((center,[f(p/32768) for p in probability]))
header,label=labels(ak);assert label==data['labels'];profile=data['cacheProfile'];profile_dynamic=[struct.unpack_from('<H',ak,195771)[0]]+list(struct.unpack_from('<8f',ak,195773))
for index,g in enumerate(data['groups']):
 ranges=g['ranges'];raw=[points[s][a:z] for s,a,z in ranges];minimum=min(p[0] for stroke in raw for p in stroke)
 expected_normal=[[[min(32767,int(f(f(p[0]-minimum)*data['target']/data['height']))),min(32767,int(f(f(p[1]-data['baseline'])*data['target']/data['height'])))] for p in stroke] for stroke in raw]
 assert expected_normal==g['normalized'],('normalize',index)
 sum_x=f(0.);count=0
 #This actual graph keeps physical order; chronology order is separatelychecked.
 for stroke in raw:
  for p in stroke:sum_x=f(sum_x+p[0]);count+=1
 target=f(sum_x/count);context=[0.]*28
 for center,probability in accepted:
  distance=f(f(center-target)/data['height'])
  if distance>=0:exponent=f(f(f(distance*distance)*f(-.5))/rv)
  else:
   distance=f(distance+offset);exponent=f(f(f(distance*distance)*f(-.5))/lv) if distance<=0 else 0.
  if exponent< -75:continue
  weight=f(math.exp(exponent));context=[max(a,f(weight*b)) for a,b in zip(context,probability)]
 context=[f(f(v+v)-1) for v in context];context=[-1. if v< f(-.99)else v for v in context]
 assert fixed(context)==g['context'],('context28',index)
 for vector in g['inputs']:assert vector[-30:-2]==g['context']and vector[-2:]==g['boundaries']
 prob=[exp_prob(network(ak,n,inputs,activation)) for n,inputs in zip([195895,283697,367899,467101],g['inputs'])]
 assert prob==g['probabilities'],('familyNN',index)
 dyn=dynamic(raw,profile_dynamic);np.testing.assert_allclose(g['weights'][:4],[dyn[2]]*4,rtol=1e-6,atol=1e-6)
 integers=costs([dict(probabilities=p,weight=f(g['weights'][i]))for i,p in enumerate(prob)]+[None]*3,1,data['mask'][:180])
 assert integers==g['integerCosts'],('integercost',index)
 expected_cache,candidates=cache(integers,g['normalized'],data['mask'],header,label,profile,g['whole'],g['zero'])
 assert expected_cache==g['cacheCosts'],('cache',index,[(i,a,b)for i,(a,b)in enumerate(zip(expected_cache,g['cacheCosts']))if a!=b][:12])
 assert candidates==sorted(g['candidates']),('candidates',index)
print(f'PASS {len(data["windows"])} actual float context/7001 windows/{len(accepted)} accepted, {len(data["groups"])} normalized/context28/dynamic groups, {len(data["groups"])*4} actual NNs and all integer195cache channels; no expected OCR labels.')
