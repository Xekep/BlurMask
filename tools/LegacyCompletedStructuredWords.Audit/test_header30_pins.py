"""PURE source corruption controls; never builds/loads recognition code."""
from pathlib import Path
import json,shutil,subprocess,sys
if len(sys.argv)!=3:raise SystemExit('REPO FRESH_OUTPUT')
root=Path(sys.argv[1]).resolve();out=Path(sys.argv[2]).resolve();assert not out.exists();out.mkdir(parents=True)
t=out/'case-root';folder='tools/LegacyCompletedStructuredWords.Audit';audit=root/folder
shutil.copytree(audit,t/folder);(t/'src/InkRecognition').mkdir(parents=True)
pins=json.loads((audit/'source-pins.json').read_text());script=t/folder/'audit_source.py';cases=[]
for name in list(pins['lfSuccessors'])+list(pins['header30CurrentFiles']):shutil.copyfile(root/'src/InkRecognition'/name,t/'src/InkRecognition'/name)
def refuse(name,path,change):
 old=path.read_bytes();path.write_bytes(change(old))
 try:
  p=subprocess.run([sys.executable,'-I','-B',str(script),str(t)],stdout=subprocess.PIPE,stderr=subprocess.STDOUT,check=False)
  assert p.returncode!=0,name;cases.append(dict(Name=name,ExitCode=p.returncode))
 finally:path.write_bytes(old)
for name in pins['lfSuccessors']:refuse('current-corruption-'+name,t/'src/InkRecognition'/name,lambda b:b+b'//CORRUPTION\n')
for name in pins['header30CurrentFiles']:refuse('new-producer-corruption-'+name,t/'src/InkRecognition'/name,lambda b:b+b'//CORRUPTION\n')
for name,x in pins['header30HistoricalPreimages'].items():
 path=t/folder/x['Path'];refuse('historical-corruption-'+name,path,lambda b:b+b'//CORRUPTION\n')
 refuse('old-current-not-equivalent-'+name,t/'src/InkRecognition'/name,lambda b,p=path:p.read_bytes())
refuse('unknown-pin-schema',t/folder/'source-pins.json',lambda b:json.dumps(dict(json.loads(b),Unexpected=0)).encode())
p=subprocess.run([sys.executable,'-I','-B',str(script),str(t)],stdout=subprocess.PIPE,stderr=subprocess.STDOUT,check=True)
assert len(cases)==14
(out/'report.json').write_text(json.dumps(dict(Passed=True,CorruptionRefusals=len(cases),Cases=cases,FinalUnchangedSourcePass=p.stdout.decode(),CSharpExecuted=False,NN=False),indent=2)+'\n',encoding='utf-8',newline='\n')
print('PASS14 source corruption refusals +unmodified source pass; no current semantic reversal')
