"""Build + hash-only plans + stored model/storage DATA. No fresh profile/source/FIRST/NN."""
from pathlib import Path
import argparse,json,os,subprocess,sys
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
root=Path(__file__).resolve().parents[2];out=a.output.resolve();out.mkdir(parents=True,exist_ok=False)
env=os.environ.copy();env.update(DOTNET_PROCESSOR_COUNT='2',MSBUILDDISABLENODEREUSE='1',DOTNET_CLI_USE_MSBUILD_SERVER='0');results=[]
def run(name,cmd,timeout):
 with(out/(name+'.log')).open('wb')as log:r=subprocess.run(cmd,cwd=root,env=env,stdout=log,stderr=subprocess.STDOUT,timeout=timeout)
 results.append(dict(stage=name,exit=r.returncode));(out/'results.json').write_bytes((json.dumps(results,indent=2)+'\n').encode())
 if r.returncode:raise SystemExit(name+' failed: '+str(out/(name+'.log')))
run('source-contract-controls',[sys.executable,'-I','-B',str(root/'tools/LegacyMathAkFirstAlphabet.Probe/audit_source_contract.py'),str(root),str(out/'source-controls.json')],45)
for short,name in [('content','LegacyMathAkFirst276ModelContent.Probe'),('storage14','LegacyMathAkFirst276Storage14.Probe')]:
 tool=root/'tools'/name;art=out/'build'
 run(short+'-build',['dotnet','build',str(tool/(name+'.csproj')),'-c','Release','--artifacts-path',str(art),'--disable-build-servers','-p:UseSharedCompilation=false','-m:1','-nodeReuse:false'],360)
 # Both intentional friend assembly names are identical; project-name bin trees differ. Shared library artifacts permit incremental reuse.
 dll=art/'bin'/name/'release'/'LegacyMathAkFirst276ModelContent.Probe.dll'
 args=['dotnet',str(dll),'--validate-plan',str(root/'models/math/math-ak.res')]
 if short=='storage14':args.append(str(tool/'fixtures.json'))
 forbidden=out/(short+'-NOT-AN-ACTUAL-CAPTURE.json');args.append(str(forbidden))
 run(short+'-hash-only-plan',args,45);assert not forbidden.exists()
 run(short+'-stored-DATA',[sys.executable,'-I','-B',str(tool/'audit_output.py'),str(root),str(out/(short+'-audit.json')),'--selftest'],45)
print('PASS build/hash-only plans/stored strict DATA;0freshProfile/0source/0FIRST/0NN')
