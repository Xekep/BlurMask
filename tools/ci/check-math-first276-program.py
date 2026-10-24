"""Build + immutable profile/resource loading + raw DATA audit. No geometry/source/FIRST/NN."""
from pathlib import Path
import argparse,json,os,subprocess,sys
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
root=Path(__file__).resolve().parents[2];out=a.output.resolve();out.mkdir(parents=True,exist_ok=False)
env=os.environ.copy();env.update(DOTNET_PROCESSOR_COUNT='2',MSBUILDDISABLENODEREUSE='1',DOTNET_CLI_USE_MSBUILD_SERVER='0');results=[]
def run(name,cmd,timeout):
 with(out/(name+'.log')).open('wb')as log:r=subprocess.run(cmd,cwd=root,env=env,stdout=log,stderr=subprocess.STDOUT,timeout=timeout)
 results.append(dict(stage=name,exit=r.returncode));(out/'results.json').write_text(json.dumps(results,indent=2)+'\n',encoding='utf-8')
 if r.returncode:raise SystemExit(name+' failed; inspect '+str(out/(name+'.log')))
name='LegacyMathAkFirst276Program.Probe';tool=root/'tools'/name;art=out/'build';resource=root/'models/math/math-ak.res'
run('build',['dotnet','build',str(tool/(name+'.csproj')),'-c','Release','--artifacts-path',str(art),'--disable-build-servers','-p:UseSharedCompilation=false','-m:1','-nodeReuse:false'],360)
dll=art/'bin'/name/'release'/(name+'.dll')
run('validate-plan',['dotnet',str(dll),'--validate-plan',str(resource),str(out/'plan.json')],45)
run('immutable-profile-resource-load',['dotnet',str(dll),'--actual-profile',str(resource),str(out/'capture.json')],45)
run('independent-raw-DATA-audit',[sys.executable,'-I','-B',str(tool/'audit_output.py'),str(root),str(out/'capture.json'),str(out/'data-audit.json'),'--selftest'],90)
print('PASS immutable profile/resource data load and strict raw audit; 0geometry/0source/0FIRST/0NN')
