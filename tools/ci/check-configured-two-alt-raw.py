"""Build-only configured RAW entry plus stored/PURE DATA; NEVER invokes its DLL."""
from pathlib import Path
import argparse,json,os,subprocess,sys
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
root=Path(__file__).resolve().parents[2];output=a.output.resolve();output.mkdir(parents=True,exist_ok=False)
tool=root/'tools/LegacyConfiguredTwoAltAugmentation.Probe';artifacts=output/'build'
env=os.environ.copy();env.update(DOTNET_PROCESSOR_COUNT='2',DOTNET_CLI_TELEMETRY_OPTOUT='1',MSBUILDDISABLENODEREUSE='1')
steps=[]
def run(name,command,timeout):
    with (output/(name+'.log')).open('wb') as log:
        result=subprocess.run(command,cwd=root,env=env,stdout=log,stderr=subprocess.STDOUT,timeout=timeout)
    steps.append(dict(stage=name,exitCode=result.returncode))
    (output/'results.json').write_text(json.dumps(dict(scope='Build + SOURCE/PURE/stored DATA only; no target/model/NN',steps=steps),indent=2)+'\n')
    if result.returncode:raise SystemExit(name+' failed')
run('build-only',['dotnet','build',str(tool/'LegacyConfiguredTwoAltAugmentation.Probe.csproj'),'-c','Release','--disable-build-servers','--artifacts-path',str(artifacts),'-m:1','-p:UseSharedCompilation=false','-nodeReuse:false'],360)
run('source-contracts',[sys.executable,'-I','-B',str(tool/'audit_source_contracts.py'),str(root),str(output/'source-contracts.json')],45)
run('pure-data',[sys.executable,'-I','-B',str(tool/'test_raw.py'),str(output/'pure-data.json')],45)
run('stored-raw',[sys.executable,'-I','-B',str(tool/'audit_stored.py'),str(output/'stored-raw.json')],45)
run('metadata-bound',[sys.executable,'-I','-B',str(tool/'audit_debit_metadata.py'),str(output/'metadata-bound.json')],45)
run('historical-d0-source',[sys.executable,'-I','-B',str(root/'tools/handwriting/audit_linguistic_fst28c_number_initial_d0_budget.py'),'--repo',str(root),'--output',str(output/'historical-d0-source.json')],45)
print('PASS build-only/source/PURE/stored RAW; NO target or new inference')
