"""Audit isolated actual-encoder tuples, or authenticate their compiled sources."""
from pathlib import Path
import argparse,hashlib,json,math,re

def verify_stage(stage):
 manifest_path=stage.parent/'source-manifest.json';manifest=json.loads(manifest_path.read_bytes())
 assert manifest['Schema']==1 and manifest['Scope']=='ISOLATED_ENCODER_OBSERVATION_NOT_MAIN_ADMISSION_OR_NEW_INFERENCE'
 assert manifest['SourceCounts']=={'Core':951,'Math':88,'Probe':3}
 files={}
 for folder in ['src/InkRecognition','src/InkRecognition.Math','tools/LegacyMathCurrentSelectedIngress.Probe']:
  for path in(stage/folder).rglob('*.cs'):
   if not {'obj','bin'}&set(path.relative_to(stage).parts):files[path.relative_to(stage).as_posix()]=hashlib.sha256(path.read_bytes()).hexdigest()
 assert files==manifest['DiagnosticSourcePins'] and len(files)==1042,'Diagnostic source set/body changed'
 modified={'src/InkRecognition.Math/Math2Mode0TensorState.cs','tools/LegacyMathCurrentSelectedIngress.Probe/Program.cs'}
 for name,expected in manifest['MainInputPins'].items():
  if name not in modified:assert hashlib.sha256((stage/name).read_bytes()).hexdigest()==expected,name
 assert files['src/InkRecognition.Math/Math2EncoderDiagnostic.cs']==manifest['TemplateSha256']
 return hashlib.sha256(manifest_path.read_bytes()).hexdigest()

def audit(trace_path,capture_path):
 trace=json.loads(trace_path.read_bytes());capture=json.loads(capture_path.read_bytes())
 assert trace['Schema']==1 and trace['Scope']=='ISOLATED_ACTUAL_ENCODER_INPUT_OUTPUT_HASHES_NO_EXTRA_INFERENCE'
 assert trace['Complete']and not trace['Pending']and trace['Runtime']==capture['Runtime']
 assert trace['CaptureSha256']==hashlib.sha256(capture_path.read_bytes()).hexdigest()
 calls=trace['Calls'];assert len(calls)==18
 byte_count=0
 for ordinal,call in enumerate(calls):
  assert call['Ordinal']==ordinal and len(call['Inputs'])==3 and len(call['Outputs'])==5
  for tensor in call['Inputs']+call['Outputs']:
   assert type(tensor['Type'])is int and tensor['Type']in(1,7)
   shape=tensor['Shape'];assert len(shape)<=8 and all(type(v)is int and v>0 for v in shape)
   assert tensor['Elements']==math.prod(shape)<=4*1024*1024
   assert tensor['Bytes']==tensor['Elements']*(4 if tensor['Type']==1 else 8)
   assert re.fullmatch('[0-9a-f]{64}',tensor['Sha256'])
   byte_count+=tensor['Bytes']
  case,route=divmod(ordinal,3);nn=capture['Actual'if route==2 else'Unchanged'][case]['NewNn']
  source,downsampling,ends=call['Inputs'];n=nn['Frames']
  assert(source['Type'],source['Shape'])==(1,[n,1,26])
  assert(downsampling['Type'],downsampling['Shape'])==(7,[n,1])
  assert ends['Type']==7 and len(ends['Shape'])==2 and ends['Shape'][0]==1
 assert trace['ReadBytes']==byte_count<=128*1024*1024
 for case in range(6):
  fresh,unchanged=calls[case*3:case*3+2]
  assert fresh['Inputs']==unchanged['Inputs']and fresh['Outputs']==unchanged['Outputs'],('Fresh/unchanged encoder tuples differ',case)
 return trace

def prove(stage,artifacts,report):
 from publication_portable_pdb import audit_pdb
 manifest_sha=verify_stage(stage)
 rows={}
 for folder,name,assembly,count in [('src','InkRecognition','InkRecognition',951),('src','InkRecognition.Math','InkRecognition.Math',88),('tools','LegacyMathCurrentSelectedIngress.Probe','LegacyMathFreshPublisher.Probe',3)]:
  authors={p.resolve()for p in(stage/folder/name).glob('*.cs')};assert len(authors)==count
  dll=artifacts/'bin'/name/'release'/(assembly+'.dll')
  result=audit_pdb(dll.with_suffix('.pdb'),dll,authors,artifacts/'obj'/name);assert result['authoredDocuments']==count;rows[name]=result
 report.write_text(json.dumps(dict(PASS=True,Scope='Isolated diagnostic build; not production source admission',SourceManifestSha256=manifest_sha,Projects=rows),indent=2)+'\n',encoding='utf8')

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__)
 p.add_argument('trace',type=Path,nargs='?');p.add_argument('capture',type=Path,nargs='?');p.add_argument('others',type=Path,nargs='*')
 p.add_argument('--prove-build',type=Path,nargs=3,metavar=('STAGE','ARTIFACTS','REPORT'));a=p.parse_args()
 if a.prove_build:
  prove(*(q.resolve()for q in a.prove_build));print('PASS isolated Core951/Math88/probe3 authored PE/PDB proof')
 else:
  if a.trace is None or a.capture is None:p.error('trace and original capture are required')
  primary=audit(a.trace,a.capture)
  for path in a.others:
   assert path.name.endswith('.encoder.json');other=audit(path,Path(str(path)[:-len('.encoder.json')]))
   assert other==primary,'Same-platform encoder evidence differs'
  print(json.dumps(dict(PASS=True,Calls=18,FreshUnchangedExact=6,ExactOtherRuns=len(a.others),ObservedTensorPayloadBytes=primary['ReadBytes'],Scope='Actual tensor shape/SHA consistency; not independent neural recomputation or numerical tolerance')))
