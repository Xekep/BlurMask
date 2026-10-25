"""Managed FIRST276 input14 build/capture/check. No native/AOT or evaluator/NN work."""
from pathlib import Path
import argparse,json,os,subprocess,sys
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
root=Path(__file__).resolve().parents[2];out=a.output.resolve();out.mkdir(parents=True,exist_ok=False)
tool=root/'tools/LegacyMathAkFirst276Input14.Probe';art=out/'build';results=[]
env=os.environ.copy();env.update(DOTNET_PROCESSOR_COUNT='2',MSBUILDDISABLENODEREUSE='1',DOTNET_CLI_USE_MSBUILD_SERVER='0',PYTHONDONTWRITEBYTECODE='1')
def run(name,cmd,timeout):
 with(out/(name+'.log')).open('wb')as log:r=subprocess.run(cmd,cwd=root,env=env,stdout=log,stderr=subprocess.STDOUT,timeout=timeout)
 results.append(dict(stage=name,exit=r.returncode));(out/'results.json').write_text(json.dumps(results,indent=2)+'\n',encoding='utf-8')
 if r.returncode:raise SystemExit(name+' failed: '+str(out/(name+'.log')))
run('historical-strict-DATA',[sys.executable,'-I','-B',str(tool/'audit_output.py'),str(root),str(out/'historical-audit.json'),'--selftest'],45)
run('input14-build',['dotnet','build',str(tool/'LegacyMathAkFirst276Input14.Probe.csproj'),'-c','Release','--artifacts-path',str(art),'--disable-build-servers','-p:UseSharedCompilation=false','-m:1','-nodeReuse:false'],360)
dll=art/'bin/LegacyMathAkFirst276Input14.Probe/release/LegacyMathAkFirst276Program.Probe.dll'
plan=out/'hash-plan-NOT-CAPTURE.json';inputs=[str(root/'models/math/math-ak.res'),str(tool/'inputs/fixtures.json')]
run('hash-only-plan',['dotnet',str(dll),'--validate-plan',*inputs,str(plan)],45);assert not plan.exists()
capture=out/'capture.json';run('actual-input14',['dotnet',str(dll),'--actual',*inputs,str(capture)],45)
run('actual-strict-oracle',[sys.executable,'-I','-B',str(tool/'audit_output.py'),str(root),str(out/'actual-audit.json'),'--capture',str(capture)],45)
print('PASS managed input14 regression:12 exact276 inputs,2 explicit source-point-domain refusals;0NN/0evaluator successes. No native/AOT claim.')
