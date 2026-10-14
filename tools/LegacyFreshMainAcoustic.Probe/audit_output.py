"""Independent NumPy int8 NN, EXP, dynamic-weight/integer/cache audit.
Feature extraction is separately tested by its own scalar probes. Here the
recorded actual feature vectors drive a separately implemented NN evaluator.
No vendor DLL or expected text label is executed or used for correction.
"""
import ast, hashlib, json, math, struct, sys
from pathlib import Path
import numpy as np

ROOT=Path(__file__).resolve().parents[2]
def definitions(path):
 tree=ast.parse(path.read_text());tree.body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))]
 scope={};exec(compile(tree,str(path),'exec'),scope);return scope
weights_ref=definitions(ROOT/'tools/LegacyDynamicWeights.Probe/reference_controls.py')['reference']
cost_ref=definitions(ROOT/'tools/LegacyCostEnsemble.Probe/reference_controls.py')['reference']
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def i32(x):return (int(x)+2**31)%2**32-2**31
def network(data,offset,inputs,activation):
 size=struct.unpack_from('<I',data,offset+4)[0]+8;b=data[offset:offset+size]
 assert struct.unpack_from('<HH',b)==(0xfea0,0x106) and b[8]==0x12
 ni,nh=struct.unpack_from('<II',b,11);no=struct.unpack_from('<I',b,27)[0]
 assert ni==len(inputs)
 cursor=33
 def matrix(columns,rows):
  nonlocal cursor
  n=columns*(rows-1)
  m=np.frombuffer(b[cursor:cursor+n],dtype=np.int8).astype(np.int64).reshape(rows-1,columns)
  cursor+=n;scales=list(b[cursor:cursor+rows]);cursor+=rows;assert scales[0]==1
  return m,scales[1:]
 w1,s1=matrix(ni,nh);w2,s2=matrix(nh,no);assert cursor==len(b)
 dots=w1@np.array(inputs,dtype=np.int64)
 hidden=[1024]
 for dot,scale in zip(dots,s1):
  value=i32(i32(dot)*scale);index=abs(value)>>16
  h=activation[index] if index<len(activation) else 1018
  hidden.append(-h if value<=0 else h)
 return [i32(i32(dot)*scale) for dot,scale in zip(w2@np.array(hidden,dtype=np.int64),s2)]
def exp_prob(raw):
 threshold=(351<<16)-4*1024*1024;bias=4*1024*1024
 shift=max(max(raw),threshold)-threshold
 table=[int(25*math.exp(-4+22*i/351)) for i in range(352)]
 values=[0 if x-shift+bias<0 else table[min(351,(x-shift+bias)>>16)] for x in raw]
 while sum(values)>2**31-1:values=[x>>1 for x in values]
 total=sum(values);maximum=max(values)
 if maximum==0:return [0]*len(raw)
 sum_shift=max(0,total.bit_length()-1-6);divisor=total>>sum_shift
 value_shift=30-(maximum.bit_length()-1)
 return [min(32767,((x<<value_shift)&0xffffffff)//divisor>>((value_shift+sum_shift-15)&31)) for x in values]
def round_native(x):return int(x+(.5 if x>0 else -.5))
IMP=0x4000000
def cache_reference(raw,points,mask,labels,profile,whole,zero):
 fallback,longdash,cjk,negative_div,positive_div,closed,threshold,minimum,capacity=profile
 costs=[v if mask[i] else IMP for i,v in enumerate(raw)]+[IMP]*(195-len(raw))
 costs[179]=IMP
 def assign(text,v):
  if text in labels and mask[labels[text]]:costs[labels[text]]=min(IMP,v)
 if costs[140]<IMP:
  base=costs[140];assign('-',base);assign('_',base)
  for text,p in [('ー',longdash),('一',cjk)]:assign(text,IMP if p<=0 else base+round_native(-math.log(p)*8192))
  negative=positive=0.
  for a,b in zip(points,points[1:]):
   dx=b[0]-a[0];dy=b[1]-a[1];length=f(math.hypot(dx,dy))
   if dx<0:negative=f(negative+length)
   elif dx>0:positive=f(positive+length)
   else:negative=f(negative+f(length*.5));positive=f(positive+f(length*.5))
  total=f(negative+positive)
  if abs(total)<=max(f(abs(total)*f(2.3841858e-7)),f(1.1920929e-7)):negative=positive=.5
  else:negative=f(negative/total);positive=f(positive/total)
  for text,p,d in [('\U000f0002',negative,negative_div),('\U000f0003',positive,positive_div)]:
   assign(text,IMP if p<=f(.1) else base+round_native(-math.log(f(p/d))*8192))
  costs[140]=IMP
 if whole:assign('\U000f0001',IMP) #actual owned singleton2AD probability0
 assign('�',IMP)
 costs[192]=costs[178]-int(f(fallback*-8192));costs[178]=IMP
 if whole and zero and '.' in labels and mask[labels['.']]:costs[labels['.']]=min(costs[:192])-1
 for text,index in [("'",193),('"',194)]:
  if text in labels:costs[index]=costs[labels[text]]
 #Independently retain sorted cost classes. Exact-cap ties are asserted absent
 #for this actual corpus; existing maxheap probe covers native identity ties.
 eligible=sorted((v,i) for i,v in enumerate(costs) if i!=192 and v<min(threshold,85173))
 assert len(eligible)>=minimum
 if len(eligible)>capacity:assert eligible[capacity-1][0]!=eligible[capacity][0]
 retained=eligible[:int(capacity)];saved=costs[192]
 out=[IMP]*195;out[192]=saved
 for v,i in retained:out[i]=v
 if '\u00ad' in labels and '-' in labels:out[labels['\u00ad']]=out[labels['-']]
 return out,sorted([i,v] for v,i in retained if i<192)

if len(sys.argv)!=4:raise SystemExit('usage: audit_output.py acoustic.res engine-profile.json output.json')
data=Path(sys.argv[1]).read_bytes();assert hashlib.sha256(data).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
engine=json.loads(Path(sys.argv[2]).read_text());p=json.loads(Path(sys.argv[3]).read_text())
assert p['mainPolicy']==dict(flags=0x21400,arcs=125,aux=0,alternatives=20,penup=10,prune=True)
assert p['target']==100 and p['height']==40 and p['baseline']==90 and p['sourceProbability']==0
#These configuration bytes are independently taken from the serialized prefix,
#rather than trusting the scorer's emitted dynamic weights/profile fields.
dynamic_at=195771;count=struct.unpack_from('<H',data,dynamic_at)[0]
dynamic=[count]+list(struct.unpack_from('<8f',data,dynamic_at+2))
networks=[195895,283697,367899,467101]
for index,g in enumerate(p['acousticGroups']):
 vector_rows=g['inputs'];assert list(map(len,vector_rows))==[256,238,313,313]
 for family,vector in enumerate(vector_rows):
  #1F is the full256-channel stroke vector, without an invented leading
  #bias. The other three feature contracts do include their1024 first slot.
  assert (family==0 or vector[0]==1024) and vector[-30:-2]==[-1024]*28 and vector[-2:]==g['boundaries']
 probabilities=[exp_prob(network(data,n,inputs,engine['activations']['hidden'])) for n,inputs in zip(networks,vector_rows)]
 assert probabilities==g['probabilities'],('NN',index)
 range0=p['normalized'][index]['ranges'][0];start,end=range0[1:]
 dyn=weights_ref([[list(map(f,row[:2])) for row in p['cutRows'][start:end]]],dynamic)
 assert g['weights'][:4]==[dyn[2]]*4 or all(abs(v-dyn[2])<1e-7 for v in g['weights'][:4]),('weights',index,dyn,g['weights'])
 slots=[dict(probabilities=family,weight=f(g['weights'][i])) for i,family in enumerate(probabilities)]+[None]*3
 integers=cost_ref(slots,1,p['mask'][:180]);assert integers==g['integerCosts'],('integer',index)
 confidence=[]
 for label in range(180):
  contributions=[f(1/(1+math.exp(-(f(probabilities[k][label]/32768)-f(engine['centers'][k][label]))*f(engine['slopes'][k][label])))) for k in range(4)]
  value=f(f(f(contributions[1]+contributions[0])+contributions[2])+contributions[3]);confidence.append(f(value/4))
 assert all(f(a)==f(b) for a,b in zip(g['confidence'],confidence)),('confidence',index)
 costs,candidates=cache_reference(integers,p['normalized'][index]['points'],p['mask'],p['specialLabels'],p['cacheProfile'],g['whole'],g['zeroEndpoints'])
 assert costs==g['cacheCosts'],('cache',index,[(i,a,b) for i,(a,b) in enumerate(zip(costs,g['cacheCosts'])) if a!=b])
 assert candidates==sorted(g['candidates']),('candidates',index)
print(f'PASS independent40 actual NNs/7200probabilities,10 raw-float dynamic groups,1800 integer+calibrated channels,1950 cache entries and shortlists')
