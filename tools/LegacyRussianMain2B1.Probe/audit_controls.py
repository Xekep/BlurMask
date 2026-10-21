"""Independent whole/local raw88=1 controls and actual-model NN witnesses.
The explicitly wrong V1 whole-unit branch is a negative reference, never a
runtime fallback or candidate choice. No expected text/quality labels.
"""
import ast,hashlib,importlib.util,json,struct,sys
from pathlib import Path
import numpy as np
ROOT=Path(__file__).resolve().parents[2]
def definitions(path):
 t=ast.parse(path.read_text(encoding='utf-8-sig'));t.body=[n for n in t.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))];g={'__file__':str(path)};exec(compile(t,str(path),'exec'),g);return g
spec=importlib.util.spec_from_file_location('features',Path(__file__).with_name('reference_features.py'));m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);profile=json.loads((Path(sys.argv[1])/'ru-RU/legacy-engine-profile.json').read_bytes());reference=m.prepare(profile);wrong_v1=m.prepare(profile,True);wrong_v2=m.prepare(profile,False,True)
spec=importlib.util.spec_from_file_location('scalar_geometry',ROOT/'tools/LegacyEnglishMainAcoustic.Probe/reference_features.py');scalar=importlib.util.module_from_spec(spec);spec.loader.exec_module(scalar)
raw=(Path(sys.argv[1])/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes();assert hashlib.sha256(raw).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978';networks=[]
for offset in [367899,467101]:
 end=offset+8+struct.unpack_from('<I',raw,offset+4)[0];network=raw[offset:end];ni,nh=struct.unpack_from('<II',network,11);no=struct.unpack_from('<I',network,27)[0];pos=33
 def matrix(columns,rows):
  global pos
  n=columns*(rows-1);mat=np.frombuffer(network[pos:pos+n],dtype=np.int8).astype(np.int64).reshape(rows-1,columns);pos+=n;scale=list(network[pos:pos+rows]);pos+=rows;assert scale[0]==1;return mat,scale[1:]
 w1,s1=matrix(ni,nh);w2,s2=matrix(nh,no);assert pos==len(network);networks.append((ni,w1,s1,w2,s2))
refs=definitions(ROOT/'tools/LegacyFreshMainAcoustic.Probe/audit_output.py');exp_prob=refs['exp_prob'];i32=lambda x:(int(x)+2**31)%2**32-2**31
activation=profile['activations']['hidden']
def evaluate(n,inputs):
 ni,w1,s1,w2,s2=n;assert len(inputs)==ni;hidden=[1024]
 for dot,scale in zip(w1@np.array(inputs,dtype=np.int64),s1):
  value=i32(i32(dot)*scale);index=abs(value)>>16;v=activation[index]if index<len(activation)else 1018;hidden.append(-v if value<=0 else v)
 return exp_prob([i32(i32(dot)*scale)for dot,scale in zip(w2@np.array(hidden,dtype=np.int64),s2)])
data=json.loads(Path(sys.argv[2]).read_bytes());assert len(data['guards'])==19 and len(set(data['guards']))==19;assert len(data['cases'])==9;channels=0;rows=[]
for case in data['cases']:
 first,second=reference(case['strokes']);vectors=[first+case['context']+[1024,-1024],second+case['context']+[1024,-1024]];assert vectors==case['inputs'],('actual2B1',case['id']);channels+=626
 normalized=scalar.coord['norm'](case['strokes']);samples=scalar.coord['resample'](normalized,100);assert samples['strokes']==case['prepared']and case['shift']==normalized['shift']and case['step']==samples['step']and case['recomputedSamplePointSequenceSame']is True
 maximum=[max(point[axis]for stroke in samples['strokes']for point in stroke)for axis in(0,1)];unit=1 if normalized['shift']<0 else 1<<normalized['shift'];assert [max(1,v)for v in maximum]==case['dimensions'];assert [v+unit for v in maximum]==case['oldUnitDimensions']
 probabilities=[evaluate(n,v)for n,v in zip(networks,vectors)];assert probabilities==case['probabilities'],('actual_owned_model_NN',case['id'])
 old=[v+case['context']+[1024,-1024]for v in wrong_v1(case['strokes'])];wrong_probabilities=[evaluate(n,v)for n,v in zip(networks,old)];changed=sum(a!=b for x,y in zip(vectors,old)for a,b in zip(x,y));nnchanged=sum(a!=b for x,y in zip(probabilities,wrong_probabilities)for a,b in zip(x,y));rows.append(dict(id=case['id'],shift=case['shift'],dimensions=case['dimensions'],oldUnitDimensions=case['oldUnitDimensions'],differentShortsFromAddedWholeUnit=changed,differentProbabilitiesFromAddedWholeUnit=nnchanged))
 old2=[v+case['context']+[1024,-1024]for v in wrong_v2(case['strokes'])];old2probs=[evaluate(n,v)for n,v in zip(networks,old2)];d2=sum(a!=b for x,y in zip(vectors,old2)for a,b in zip(x,y));p2=sum(a!=b for x,y in zip(probabilities,old2probs)for a,b in zip(x,y));rows[-1].update(differentShortsFromLegacyLandmarks=d2,differentProbabilitiesFromLegacyLandmarks=p2)
 if case['id']==8:assert d2>0 and p2>0
 if case['id']in(6,7):assert normalized['shift']>0 and all(v>0 for v in maximum)and changed>0 and nnchanged>0
report=dict(guards=19,cases=9,independent2B1Shorts=channels,actualOwnedModelProbabilities=3240,positiveShiftBothExtentsWitnesses=2,negativeWholeUnitBranchRejected=True,negativeV2LandmarkBranchRejected=True,witnesses=rows,scope='Nine synthetic explicit geometry/actual-model numerical cases; no recognition quality labels.')
if len(sys.argv)>3:Path(sys.argv[3]).write_text(json.dumps(report,indent=2)+'\n')
print('PASS',json.dumps(report))
