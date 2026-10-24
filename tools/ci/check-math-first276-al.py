"""Build + hash-only validate-plan + stored AL DATA audit. Never runs --actual SOURCE14."""
from pathlib import Path
import argparse,json,os,subprocess,sys
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
root=Path(__file__).resolve().parents[2];out=a.output.resolve();out.mkdir(parents=True,exist_ok=False)
env=os.environ.copy();env.update(DOTNET_PROCESSOR_COUNT='2',MSBUILDDISABLENODEREUSE='1',DOTNET_CLI_USE_MSBUILD_SERVER='0');results=[]
def run(name,cmd,timeout):
 with(out/(name+'.log')).open('wb')as log:r=subprocess.run(cmd,cwd=root,env=env,stdout=log,stderr=subprocess.STDOUT,timeout=timeout)
 results.append(dict(stage=name,exit=r.returncode));(out/'results.json').write_bytes((json.dumps(results,indent=2)+chr(10)).encode('utf-8'))
 if r.returncode:raise SystemExit(name+' failed; inspect '+str(out/(name+'.log')))
name='LegacyMathAkFirstAlphabet.Probe';tool=root/'tools'/name;art=out/'build'
run('source-contract-controls',[sys.executable,'-I','-B',str(tool/'audit_source_contract.py'),str(root),str(out/'source-contract-controls.json')],45)
run('build',['dotnet','build',str(tool/(name+'.csproj')),'-c','Release','--artifacts-path',str(art),'--disable-build-servers','-p:UseSharedCompilation=false','-m:1','-nodeReuse:false'],360)
run('validate-plan',['dotnet',str(art/'bin'/name/'release'/(name+'.dll')),'--validate-plan',str(root/'models/math/math-ak.res'),str(tool/'fixtures.json'),str(out/'NOT-AN-OWNING-CAPTURE.json')],45)
assert not(out/'NOT-AN-OWNING-CAPTURE.json').exists()
run('stored-DATA-audit',[sys.executable,'-I','-B',str(tool/'audit_output.py'),str(root),str(out/'data-audit.json'),'--selftest'],45)
print('PASS build/hash-only plan/stored AL SOURCE14 DATA; no fresh profile/source/FIRST/NN')
