from pathlib import Path
import argparse,copy,hashlib,json,importlib.util
HERE=Path(__file__).resolve().parent
_spec=importlib.util.spec_from_file_location('bootstrap_preimage',HERE/'bootstrap_preimage.py')
_module=importlib.util.module_from_spec(_spec);_spec.loader.exec_module(_module)
restore_bootstrap_source=_module.restore
TABLE_LF_SHA='6a356fff98abd80e2b060fedf5388c8ad92bc36706feb6b8850c7ab1c61df6ab'
def digest(b):return hashlib.sha256(b).hexdigest()
def require(x):
 if not x:raise ValueError('reviewed original-input SOURCE contract mismatch')
def lf(b):return b.replace(b'\r\n',b'\n')
def restore(rec,raw):
 b=lf(raw);require(rec['Kind']=='OLD')
 if digest(b)==rec['PreimageLF']:return b
 require(digest(b)==rec['SuccessorLF'])
 for step in reversed(rec['Reverse']):
  a,z=step['Start'],step['End'];require(b[a:z]==step['SuccessorSlice'].encode('utf8'));b=b[:a]+step['OldSlice'].encode('utf8')+b[z:]
 require(digest(b)==rec['PreimageLF']);return b
p=argparse.ArgumentParser();p.add_argument('repo',type=Path);p.add_argument('output',type=Path);a=p.parse_args()
raw=(HERE/'source-contracts.json').read_bytes();require(digest(lf(raw))==TABLE_LF_SHA);table=json.loads(raw);restored=0;neg=0
for rec in table['Sources']:
 b=restore_bootstrap_source(rec['Relative'],(a.repo/rec['Relative']).read_bytes(),rec['SuccessorLF']);require(digest(lf(b))==rec['SuccessorLF'])
 if rec['Kind']=='OLD':
  reference=lf((HERE/rec['HistoricalReference']).read_bytes());require(digest(reference)==rec['PreimageLF']);require(restore(rec,b)==reference);require(restore(rec,reference)==reference);restored+=1
  for malformed in (b'!'+b[1:],b[:-1],b+b'!'):
   try:restore(rec,malformed)
   except ValueError:neg+=1
   else:raise AssertionError('unreviewed source accepted')
 else:
  for malformed in (b'!'+b[1:],b[:-1],b+b'!'):
   require(digest(lf(malformed))!=rec['SuccessorLF']);neg+=1
for rec in table['Fixtures']:
 b=(HERE/rec['Relative']).read_bytes();require(len(b)==rec['Bytes'] and digest(b)==rec['RawSHA256'])
# These are the exact reviewed SOURCE branches; no target/owner constructed.
s=(a.repo/'src/InkRecognition/LegacyEnglishOriginalInputContext.cs').read_text(encoding='utf-8-sig');require(s.index('PrechargeCompleteRawDomain(token)')<s.index('Snapshot(raw,token)')<s.index('var parent=Prepare(model,input.Document.Strokes'));require('Binding=processing.InputBinding' in s and 'lease.Validate()' in s and 'actual.RequireOriginalInputPoints(Document,token)' in s)
with a.output.open('x',encoding='utf8') as f:json.dump(dict(Status='PASS_EXACT_SOURCE_ONLY',CurrentPins=len(table['Sources']),HistoricalDefaultPreimages=restored,UnknownSourceRefusals=neg,RawFixturePins=len(table['Fixtures']),TargetExecuted=False,OwningCalls=0,NNCalls=0,NativeSDKSelectedReaderOriginProven=False,Scope='exact old3 default-preimage restoration; optinruntime proof separate single frozen owner'),f,indent=2);f.write('\n')
