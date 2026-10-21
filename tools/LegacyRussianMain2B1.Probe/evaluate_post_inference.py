"""Post-inference only: fixed capture/oracle hashes BEFORE target access.
No recognition is performed and no target is fed to model/decoder code.
V3 fixed-dataset replay: targets were previously seen after V1 inference.
This is not a new unseen/blind test; V3 source/captures/oracles freeze first.
"""
import ast,hashlib,json,sys,collections
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
def sha(p):return hashlib.file_digest(p.open('rb'),'sha256').hexdigest()
def distance(a,b):
 row=list(range(len(b)+1))
 for i,x in enumerate(a,1):
  nextrow=[i]
  for j,y in enumerate(b,1):nextrow.append(min(row[j]+1,nextrow[-1]+1,row[j-1]+(x!=y)))
  row=nextrow
 return row[-1]
def stream(path):
 script=ROOT/'tools/LegacyRussianMainAcoustic.Probe/audit_output.py';t=ast.parse(script.read_text());t.body=[n for n in t.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))];g={'__file__':str(script)};exec(compile(t,str(script),'exec'),g);return g['samples'](path)
o=Path(sys.argv[1]);target=Path(sys.argv[2]);oldraw=Path(sys.argv[3]);oldhost=Path(sys.argv[4]);capture=o/'capture-managed.json';assert sha(capture)=='31ad0ebe0fcea0c143f7770402a28ef19034bf41b6097e4a74a10e7e3959dae7';assert sha(o/'capture-native.json')==sha(capture)
for name in ['audit-managed.json','geometry-audit.json','cache-audit.json']:
 d=json.loads((o/name).read_bytes());assert d.get('samples',261)==261 and d.get('groups',27866)==27866
rows=[dict(id=s['id'],text=''.join(w['text']for w in s['words']))for s in stream(capture)];assert len(rows)==261
host=json.loads((o/'host-managed.json').read_bytes());assert host['guards']==11 and len(host['samples'])==261;assert sha(o/'host-native.json')==sha(o/'host-managed.json')
before=json.loads(oldraw.read_bytes())['samples'];oldadmission=json.loads(oldhost.read_bytes())['samples'];assert len(before)==len(oldadmission)==261
frozen=dict(captureSha256=sha(capture),nativeCaptureSha256=sha(o/'capture-native.json'),hostSha256=sha(o/'host-managed.json'),nativeHostSha256=sha(o/'host-native.json'),oracleSha256={name:sha(o/name)for name in ['audit-managed.json','geometry-audit.json','cache-audit.json']},sourceSha256={name:sha(ROOT/name)for name in ['src/InkRecognition/LegacyRussianModelBundle.cs','src/InkRecognition/LegacyDirectionalGlyphClassifier.cs','src/InkRecognition/LegacySaturatedDirectionalFeatures.cs','src/InkRecognition/LegacyDirectional2B1Landmarks.cs','tools/LegacyDirectional2B1.Probe/reference_landmarks.py','src/InkRecognition/LegacySaturatedDirectionalFeatureAssembly.cs','src/InkRecognition/LegacyDirectional2B1DefaultCalibration.cs','tools/LegacyRussianMain2B1.Probe/Program.cs','tools/LegacyRussianMain2B1.Probe/reference_features.py','tools/LegacyRussianMain2B1.Probe/audit_output.py','tools/LegacyRussianMain2B1.Probe/audit_controls.py','tools/LegacyRussianMainAcoustic.Probe/audit_output.py']},targetsPreviouslySeenAfterV1=True,targetsReopenedForV3=False)
(o/'inference-before-targets.json').write_text(json.dumps(frozen,indent=2)+'\n')
# FIRST target access in this program is below the capture/oracle freeze.
expected=json.loads(target.read_bytes())['samples'];assert len(expected)==261
results=[]
for a,b,h,oh,t in zip(rows,before,host['samples'],oldadmission,expected):
 assert a['id']==b['id']==h['id']==oh['id']==t['id'];oldtext=''.join(w['text']for w in b['words']);truth=t['expected']
 results.append(dict(id=a['id'],expected=truth,old=oldtext,new=a['text'],hostOld=oh['text'],hostNew=h['text'],oldEdits=distance(oldtext,truth),newEdits=distance(a['text'],truth)))
summary=dict(scope='V3 post-inference fixed dataset replay, targets previously seen after V1; no tuning, vendor-training overlap unknown. Supplied point lists omit physical pen-up markers.',samples=261,referenceCharacters=sum(len(r['expected'])for r in results),oldRawExact=sum(r['old']==r['expected']for r in results),newRawExact=sum(r['new']==r['expected']for r in results),oldEdits=sum(r['oldEdits']for r in results),newEdits=sum(r['newEdits']for r in results),changedOutputs=sum(r['old']!=r['new']for r in results),improvedEdits=sum(r['newEdits']<r['oldEdits']for r in results),worseEdits=sum(r['newEdits']>r['oldEdits']for r in results),oldHostAccepted=sum(bool(r['hostOld'])for r in results),oldHostCorrect=sum(bool(r['hostOld'])and r['hostOld']==r['expected']for r in results),newHostAccepted=sum(bool(r['hostNew'])for r in results),newHostCorrect=sum(bool(r['hostNew'])and r['hostNew']==r['expected']for r in results))
assert(summary['oldRawExact'],summary['oldEdits'],summary['oldHostAccepted'],summary['oldHostCorrect'])==(137,325,122,93)
summary['oldHostWrong']=summary['oldHostAccepted']-summary['oldHostCorrect'];summary['newHostWrong']=summary['newHostAccepted']-summary['newHostCorrect'];summary['targetSha256']=sha(target);summary['inferenceFreezeSha256']=sha(o/'inference-before-targets.json')
errors=collections.defaultdict(lambda:dict(samples=0,wrong=0,edits=0))
for r in results:
 q=errors[r['expected']];q['samples']+=1;q['wrong']+=r['new']!=r['expected'];q['edits']+=r['newEdits']
summary['remainingErrorsByExpected']=[dict(expected=k,**v)for k,v in sorted(errors.items(),key=lambda item:-item[1]['edits'])]
(o/'post-inference-summary.json').write_text(json.dumps(summary,ensure_ascii=True,indent=2)+'\n');(o/'post-inference-rows.json').write_text(json.dumps(results,ensure_ascii=True,indent=2)+'\n');print(json.dumps(summary,ensure_ascii=True))
