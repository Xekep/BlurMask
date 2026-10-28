"""Actual raw math diagnostic oracle: geometry, AL byte provenance, syntax and
Giac-only equality gating checks. Not an NN accuracy or native semantic claim.
Only tracked stdlib/NumPy helpers and data bytes; no vendor execution.
"""
from pathlib import Path
import ast,base64,hashlib,importlib.util,json,re,runpy,struct,sys
import numpy as np
root=Path(__file__).resolve().parents[2]
procedural_only='--procedural-only'in sys.argv
sys.argv=[a for a in sys.argv if a!='--procedural-only']
# Reuse independent native-coordinate helpers before their fixture assertions.
source=root/'tools/LegacyMathOriginalFieldSource.Probe/audit_output.py'
setup=source.read_text(encoding='utf-8-sig').split('data = json.loads')[0]
exec(compile(setup,str(source),'exec'),globals())
data=json.loads(Path(sys.argv[1]).read_bytes());raw=Path(sys.argv[2]).read_bytes()
assert data['resourceSHA256']==hashlib.sha256(raw).hexdigest()
reader=runpy.run_path(str(root/'tools/export-ink-math-model.py'))
r=reader['Reader'](raw,106,reader['header'](raw,90)[3]);assert r.read('H',8)==[65535]*7+[299]
n=r.read('H')
for inc in(32,64,1):table=r.read('H',n);n=next(x+inc for x in reversed(table)if x!=65535)
spans=r.read('H',n+1);entries=[r.read('II')for _ in range(spans[-1]+1)];blob=r.bytes(entries[-1][0])
intervals={label&65535:blob[offset:entries[i+1][0]]for i,(offset,label)in enumerate(entries[:-1])}
maximum=0.;records=complete=0;stages={};first=[]
for sample in data['samples']:
 e=ns['expected'](sample['ink']);want=features26(e['sampled'],297).ravel();assert len(sample['runs'])==3
 for width,row in zip((1,3,16),sample['runs'],strict=True):
  assert row['width']==width and row['policy']==('ColdDefault'if width==1 else'PreparedSearchDiagnostic')
  actual=np.asarray(row['featureBits'],dtype=np.int64).astype(np.uint32).view(np.float32)
  assert len(actual)==len(want)==row['frames']*26
  error=float(np.max(np.abs(actual-want)));maximum=max(maximum,error);assert error<1e-6
  stages[row['lastStage']]=stages.get(row['lastStage'],0)+1
  if row.get('publisherComplete'):complete+=1;assert row['rawFrameHashes'] and'mathResult'in row['publishedNames']
  for index,record in enumerate(row.get('formattedRecords',[])):
   records+=1;tokens=[]
   for unit,text in zip(record['units'],record['utf8'],strict=True):
    value=base64.b64decode(text);assert value==intervals[unit&65535],(sample['id'],width,index,unit)
    tokens.append(value.rstrip(b'\0').decode('utf8'))
   latex=record['diagnosticLatex'];requested=latex is not None and latex.rstrip().endswith('=')
   assert requested==record['diagnosticTrailingEqualsRequested']
   assert record['diagnosticEqualsGatedResult']==(record['diagnosticGiac']if requested else None)
   if index==0:
    first.append(dict(id=sample['id'],width=width,actual=latex,expectedPostInference=sample['expectedPostInference'],equals=requested,result=record['diagnosticEqualsGatedResult']))
 # Independent-label comparison is intentionally post-inference and report-only.
sample_count=17 if procedural_only else 31
assert len(data['samples'])==sample_count and complete==sample_count*3
if procedural_only:assert all(s['id'].startswith('procedural-')for s in data['samples'])
by={(r['id'],r['width']):r for r in first}
for width in(1,3,16):
 assert by['procedural-one-plus-two',width]['result']is None
 assert by['procedural-one-plus-two-equals',width]['result']=='3'
 assert by['procedural-definite-integral-1-10-x-squared-dx',width]['result']is None
 assert by['procedural-definite-integral-1-10-x-squared-dx-equals',width]['result']=='333'
if len(sys.argv)>3:assert Path(sys.argv[1]).read_bytes()==Path(sys.argv[3]).read_bytes()
summary=dict(samples=len(data['samples']),runs=sample_count*3,formattedRecords=records,featureMaximumError=maximum,stages=stages,firstRecords=first,scope='Independent geometry/AL/exact polynomial/equality gate diagnostics; no NN expected-label feedback, accuracy claim or native semantic recovery.')
Path(str(sys.argv[1])+'.audit.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf8')
print(f'PASS{sample_count} raw inputs/{sample_count*3} actual NN runs/{records} AL records;featureerror{maximum};exact polynomial/equality gates;no accuracy claim.')
