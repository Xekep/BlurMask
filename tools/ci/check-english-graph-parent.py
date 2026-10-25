"""Owning graph2 compatibility and graph3 predecessor regression."""
from pathlib import Path
import argparse,json,os,subprocess,sys
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
root=Path(__file__).resolve().parents[2];out=a.output.resolve();out.mkdir(parents=True,exist_ok=False)
tool=root/'tools/LegacyEnglishGraphParentPredecessor.ActualEntry';art=out/'build';results=[]
env=os.environ.copy();env.update(DOTNET_PROCESSOR_COUNT='2',MSBUILDDISABLENODEREUSE='1',DOTNET_CLI_USE_MSBUILD_SERVER='0',PYTHONDONTWRITEBYTECODE='1')
def run(name,cmd,timeout):
 with(out/(name+'.log')).open('wb')as log:r=subprocess.run(cmd,cwd=root,env=env,stdout=log,stderr=subprocess.STDOUT,timeout=timeout)
 results.append(dict(stage=name,exit=r.returncode));(out/'results.json').write_text(json.dumps(results,indent=2)+'\n',encoding='utf-8')
 if r.returncode:raise SystemExit(name+' failed: '+str(out/(name+'.log')))
for name,project in [('graph2','LegacyEnglishGraphLoop.ActualEntry'),('graph3','LegacyEnglishGraphParentPredecessor.ActualEntry')]:
 run(name+'-build',['dotnet','build',str(root/'tools'/project/(project+'.csproj')),'-c','Release','--artifacts-path',str(art),'--disable-build-servers','-p:UseSharedCompilation=false','-m:1','-nodeReuse:false'],360)
 dll=art/('bin/'+project+'/release/LegacyEnglishGraphLoop.ActualEntry.dll');capture=out/(name+'.json')
 run(name+'-actual',['dotnet',str(dll),'--actual-entry',str(root/'models'),str(tool/'fixtures/original-141.json'),str(capture)],45)
 if name=='graph2':run('graph2-unchanged-audit',[sys.executable,'-I','-B',str(root/'tools/LegacyEnglishGraphLoop.ActualEntry/audit_graph2.py'),str(capture),str(tool/'fixtures/historical-graph2.json'),str(out/'graph2-audit.json')],45)
 else:run('graph3-structural-audit',[sys.executable,'-I','-B',str(tool/'audit_graph3.py'),str(capture),str(out/'graph3-audit.json'),'--selftest'],45)
print('PASS original graph2 exact compatibility and original graph3 owning completion; no final-word or accuracy claim.')
