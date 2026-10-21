from pathlib import Path
import ast,hashlib,json,sys,struct
import numpy as np
repo=Path(sys.argv[1]);cap=Path(sys.argv[2]);p=json.loads(cap.read_bytes())
def definitions(path):
 t=ast.parse(path.read_text());t.body=[n for n in t.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))];s={'__file__':str(path)};exec(compile(t,str(path),'exec'),s);return s
f=definitions(repo/'tools/LegacyEnglishNoInitialGlyph.Probe/feature_oracle.py')['extract'];nn=definitions(repo/'tools/LegacyFreshMainAcoustic.Probe/audit_output.py');cost=definitions(repo/'tools/LegacyCostEnsemble.Probe/reference_controls.py')['reference']
ak=(repo/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes();profile=json.loads((repo/'models/ru-RU/legacy-engine-profile.json').read_bytes());assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert p['inputSha256']=='c8fe150f3ca960cf7216da88c1dd90e1045aed2ef4652a8fa66c9e7a85353dce'and p['expectedLabelsRead'] is False
count=degen=points=0;smallest=32767;largest=-32768
for sample in p['samples']:
 assert sample['target']==100 and sample['height']>0 and set(bytes.fromhex(sample['mask']))=={1}
 offsets=[0]
 for n in sample['counts']:offsets.append(offsets[-1]+n)
 prepared=sample['prepared'];assert offsets[-1]==len(prepared)
 for group in sample['groups']:
  ranges=group['ranges'];source=[prepared[offsets[s]+start:offsets[s]+end]for s,start,end in ranges];minx=min(x for stroke in source for x,y in stroke);expected=[]
  for stroke in source:
   result=[]
   for xy in stroke:
    coords=[]
    for v,base in zip(xy,[minx,sample['baseline']]):
     v=np.float32(np.float32(np.float32(np.float32(v)-np.float32(base))*np.float32(100))/np.float32(sample['height']));assert np.isfinite(v)and v>=-32768
     coords.append(min(32767,int(v)))
    result.append(coords);points+=1;smallest=min(smallest,*coords);largest=max(largest,*coords)
   expected.append(result)
  assert expected==group['normalized'],('normalization',sample['id'],group['ordinal'])
  #All recorded samples are one continuous physical track; each candidate is
  #one chronological contiguous source range, so no invented pen-up or merge.
  assert len(ranges)==len(expected)==1 and group['retained']==[]and group['merged']==expected
  signs=[1024 if ranges[0][1]==0 else-1024,1024 if ranges[-1][2]==sample['counts'][ranges[-1][0]]else-1024]
  assert signs==group['boundaries']and group['context']==[-1024]*28
  t,r=f(expected);degen+=int(len({tuple(xy)for stroke in expected for xy in stroke})==1)
  prob=[nn['exp_prob'](nn['network'](ak,at,v+group['context']+signs,profile['activations']['hidden']))for at,v in [(19195,t),(105197,r)]]
  assert prob[0]==group['trajectoryProbabilities']and prob[1]==group['rasterProbabilities'],('NN',sample['id'],group['ordinal'])
  assert group['costs']==cost([dict(probabilities=x,weight=1)for x in prob]+[None]*5,1,[]),('cost',sample['id'],group['ordinal'])
  count+=1
 print('oracle',sample['id'],count,flush=True)if int(sample['id'].split('-')[-1])%20==0 else None
assert len(p['samples'])==p['samplesCount']==261 and count==p['groupsCount']
report=dict(status='PASS',scope='Independent full FIRST arithmetic/complete native feature/probability/cost audit on actual261 candidate streams; no targets',captureSha256=hashlib.sha256(cap.read_bytes()).hexdigest(),samples=261,groups=count,normalizedPoints=points,collapsedGroups=degen,coordinateMin=smallest,coordinateMax=largest,probabilities=count*360,costs=count*180,features=count*416,expectedLabelsRead=False,limitations=['No running vendor SDK oracle','Context is source one-track native empty prior-window branch; no multi-stroke context coverage','No native-model quality or complete engine equivalence claim'])
cap.with_name('arithmetic-audit.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
