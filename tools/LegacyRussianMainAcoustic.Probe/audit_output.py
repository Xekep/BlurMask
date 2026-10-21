"""Streaming oracle: original MAIN D0 NN/cost snapshots, recomputed pure inputs.
No targets/native code execution. Featureproducer equality is a separate stage.
"""
import ast,hashlib,json,math,struct,sys
from pathlib import Path
import numpy as np
ROOT=Path(__file__).resolve().parents[2]
def defs(path):
 t=ast.parse(path.read_text(encoding='utf-8-sig'));t.body=[n for n in t.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))];g={'__file__':str(path)};exec(compile(t,str(path),'exec'),g);return g
refs=defs(ROOT/'tools/LegacyFreshMainAcoustic.Probe/audit_output.py');exp_prob=refs['exp_prob'];i32=refs['i32'];f=refs['f']
weights_ref=defs(ROOT/'tools/LegacyDynamicWeights.Probe/reference_controls.py')['reference'];cost_ref=defs(ROOT/'tools/LegacyCostEnsemble.Probe/reference_controls.py')['reference']
models=Path(sys.argv[1]);path=Path(sys.argv[2]);data=(models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes();assert hashlib.sha256(data).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
profile=json.loads((models/'ru-RU/legacy-engine-profile.json').read_bytes());activation=profile['activations']['hidden']
networks=[]
for offset in [195895,283697,367899,467101]:
 end=offset+8+struct.unpack_from('<I',data,offset+4)[0];raw=data[offset:end];assert struct.unpack_from('<HH',raw)==(0xfea0,0x106) and raw[8]==0x12
 ni,nh=struct.unpack_from('<II',raw,11);no=struct.unpack_from('<I',raw,27)[0];pos=33
 def matrix(columns,rows):
  global pos
  n=columns*(rows-1);m=np.frombuffer(raw[pos:pos+n],dtype=np.int8).astype(np.int64).reshape(rows-1,columns);pos+=n;s=list(raw[pos:pos+rows]);pos+=rows;assert s[0]==1;return m,s[1:]
 w1,s1=matrix(ni,nh);w2,s2=matrix(nh,no);assert pos==len(raw);networks.append((ni,w1,s1,w2,s2))
def evaluate(n,inputs):
 ni,w1,s1,w2,s2=n;assert len(inputs)==ni;hidden=[1024]
 for dot,scale in zip(w1@np.array(inputs,dtype=np.int64),s1):
  value=i32(i32(dot)*scale);index=abs(value)>>16;v=activation[index] if index<len(activation) else 1018;hidden.append(-v if value<=0 else v)
 return exp_prob([i32(i32(dot)*scale) for dot,scale in zip(w2@np.array(hidden,dtype=np.int64),s2)])
dynamic=[struct.unpack_from('<H',data,195771)[0]]+list(struct.unpack_from('<8f',data,195773))
def samples(path):
 decoder=json.JSONDecoder()
 with path.open(encoding='utf8') as stream:
  buffer=stream.read(65536);marker='"samples":[';assert marker in buffer;pos=buffer.index(marker)+len(marker)
  assert '"expectedLabelsRead":false' in buffer[:pos]
  assert 'c8fe150f3ca960cf7216da88c1dd90e1045aed2ef4652a8fa66c9e7a85353dce' in buffer[:pos]
  while True:
   buffer=buffer[pos:].lstrip(' ,\r\n\t');pos=0
   while not buffer:
    chunk=stream.read(65536)
    if not chunk:raise ValueError("Truncated capture before closing samples array")
    buffer+=chunk
   if buffer[0]==']':return
   while True:
    try:obj,pos=decoder.raw_decode(buffer);break
    except json.JSONDecodeError:
     chunk=stream.read(65536)
     if not chunk:raise
     buffer+=chunk
   yield obj
counts=dict(samples=0,originalGroups=0,originalProbabilities=0,rawCosts=0,recomputedInputShorts=0,dynamicExactGroups=0,dynamicMaxUlps=0,independentDynamicCostDifferences=0)
baseline=json.loads(Path(sys.argv[3]).read_bytes());assert len(baseline['samples'])==261
baseline_rows={s['id']:s for s in baseline['samples']}
for sample in samples(path):
 assert sample['supported'] and sample['publicationCompleted'];old=baseline_rows[sample['id']];assert {k:v for k,v in sample.items() if k not in ('mask','mainGroups')}==old
 assert len(sample['mainGroups'])==sample['nn'];counts['samples']+=1
 for group in sample['mainGroups']:
  assert list(map(len,group['inputs']))==[256,238,313,313]
  for family,inputs in enumerate(group['inputs']):
   assert inputs[-30:-2]==group['context'] and inputs[-2:]==group['boundaries']
   assert evaluate(networks[family],inputs)==group['probabilities'][family],('NN',sample['id'],group['id'],family)
   counts['originalProbabilities']+=180;counts['recomputedInputShorts']+=len(inputs)
  expected=cost_ref([dict(probabilities=p,weight=f(group['weights'][i]))for i,p in enumerate(group['probabilities'])]+[None]*3,1,sample['mask'][:180])
  assert expected==group['rawCosts'],('RAW_COST',sample['id'],group['id'])
  points=[[[f(v)for v in p]for p in s]for s in group['rawRanges']];dyn=weights_ref(points,dynamic)
  def bits(v):return struct.unpack('<I',struct.pack('<f',v))[0]
  ulp=max(abs(bits(v)-bits(dyn[2]))for v in group['weights'][:4]);counts['dynamicMaxUlps']=max(counts['dynamicMaxUlps'],ulp);counts['dynamicExactGroups']+=ulp==0
  independent=cost_ref([dict(probabilities=p,weight=dyn[2])for p in group['probabilities']]+[None]*3,1,sample['mask'][:180])
  counts['independentDynamicCostDifferences']+=sum(a!=b for a,b in zip(independent,group['rawCosts']))
  counts['originalGroups']+=1;counts['rawCosts']+=len(expected)
 if counts['samples']%20==0:print(counts,flush=True)
assert counts['samples']==261 and counts['originalGroups']==27866
counts.update(expectedLabelsRead=False,originalNNAndRawCostsExact=True,privateInputsCaptured=False,scope='Original D0 vectors/costs matched using same-owned-source recomputed featureinputs. Final-cache and independent313feature extraction remain separate.')
if len(sys.argv)>4:Path(sys.argv[4]).write_text(json.dumps(counts,indent=2)+'\n')
print('PASS',json.dumps(counts))
