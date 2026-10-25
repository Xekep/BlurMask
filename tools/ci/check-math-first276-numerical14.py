"""Managed FIRST276 partial numerical regression. No native/AOT or full recognition claim."""
from pathlib import Path
import argparse,json,os,subprocess,sys
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
root=Path(__file__).resolve().parents[2];out=a.output.resolve();out.mkdir(parents=True,exist_ok=False)
tool=root/'tools/LegacyMathAkFirst276Numerical14.Probe';art=out/'build';results=[]
env=os.environ.copy();env.update(DOTNET_PROCESSOR_COUNT='2',MSBUILDDISABLENODEREUSE='1',DOTNET_CLI_USE_MSBUILD_SERVER='0',PYTHONDONTWRITEBYTECODE='1')
def run(name,cmd,timeout):
 with(out/(name+'.log')).open('wb')as log:r=subprocess.run(cmd,cwd=root,env=env,stdout=log,stderr=subprocess.STDOUT,timeout=timeout)
 results.append(dict(stage=name,exit=r.returncode));(out/'results.json').write_text(json.dumps(results,indent=2)+'\n',encoding='utf-8')
 if r.returncode:raise SystemExit(name+' failed: '+str(out/(name+'.log')))
run('source-cohort-controls',[sys.executable,'-I','-B',str(tool/'audit_source_cohort.py'),str(root),str(out/'source-cohort.json')],45)
run('numerical14-build',['dotnet','build',str(tool/'LegacyMathAkFirst276Numerical14.Probe.csproj'),'-c','Release','--artifacts-path',str(art),'--disable-build-servers','-p:UseSharedCompilation=false','-m:1','-nodeReuse:false'],360)
dll=art/'bin/LegacyMathAkFirst276Numerical14.Probe/release/LegacyMathAkFirst276Program.Probe.dll'
plan=out/'hash-plan-NOT-CAPTURE.json';inputs=[str(root/'models/math/math-ak.res'),str(root/'tools/LegacyMathAkFirst276Input14.Probe/inputs/fixtures.json')]
run('hash-only-plan',['dotnet',str(dll),'--validate-plan',*inputs,str(plan)],45);assert not plan.exists()
capture=out/'capture.json';run('actual-numerical14',['dotnet',str(dll),'--actual',*inputs,str(capture)],45)
run('actual-strict-oracle',[sys.executable,'-I','-B',str(tool/'independent_numeric_oracle.py'),'--resource',str(root/'models/math/math-ak.res'),'--capture',str(capture),'--report',str(out/'actual-audit.json'),'--selftest'],45)
print('PASS twelve exact partial numerical prefixes and two original refusals; no normalization or recognition-quality claim.')
