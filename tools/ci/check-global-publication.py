"""Build/PDB-gated default937 and test-only939 publication comparison; no quality claim."""
from pathlib import Path
import argparse,hashlib,importlib.util,json,os,re,subprocess,sys
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);p.add_argument('default_capture',type=Path);a=p.parse_args()
root=Path(__file__).resolve().parents[2];out=a.output.resolve();capture=a.default_capture.resolve();assert not out.exists()and not capture.exists();out.mkdir(parents=True);capture.parent.mkdir(parents=True,exist_ok=True)
env=os.environ.copy();env.update(DOTNET_PROCESSOR_COUNT='2',MSBUILDDISABLENODEREUSE='1',DOTNET_CLI_USE_MSBUILD_SERVER='0',PYTHONDONTWRITEBYTECODE='1')
def sha(path):return hashlib.sha256(Path(path).read_bytes()).hexdigest()
def pin(path):path=Path(path);return dict(path=str(path),bytes=path.stat().st_size,sha256=sha(path))
def load(name,path):
 spec=importlib.util.spec_from_file_location(name,path);module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module);return module
compiled_inputs={}
report=dict(PASS=False,Scope='COMPILED_DEFAULT_AND_TEST_ONLY_DIAGNOSTIC_PAIRED_PUBLICATION_NOT_FULL_PRODUCER_OR_QUALITY',ActualInvocations=0,Runtime='10.0.12',DOTNET_PROCESSOR_COUNT=2,AffinityClaim=False,Stages=[],Modes={})
def save(): (out/'report.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
def verify_execution_inputs():
 for row in inputs+list(compiled_inputs.values())+report.get('Captures',[]):assert pin(row['path'])==row,'Execution input changed after proof'
def run(name,argv,timeout):
 log=out/(name+'.log')
 with log.open('xb')as f:
  result=subprocess.run(argv,cwd=root,env=env,stdin=subprocess.DEVNULL,stdout=f,stderr=subprocess.STDOUT,timeout=timeout,creationflags=subprocess.CREATE_NO_WINDOW if os.name=='nt'else 0)
 report['Stages'].append(dict(Name=name,Command=list(map(str,argv)),ExitCode=result.returncode,TimeoutSeconds=timeout,Log=pin(log)));save()
 if result.returncode:raise RuntimeError('FIRST failure '+name+'; preserved '+str(log))
 return log
MODELS={'ru-RU/ru_RU-ak-cur.lite.res': {'bytes': 572763, 'sha256': '7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'}, 'ru-RU/ru_RU-lk-text.lite.res': {'bytes': 6200992, 'sha256': 'd26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'}, 'ru-RU/legacy-engine-profile.json': {'bytes': 53639, 'sha256': 'c339047fecc25a3f8244f40a5b8389db86bf4020c489aa3ec0e6d79da9679abc'}, 'line-geometry/line-2e3-30.res': {'bytes': 85812, 'sha256': '10f4b43cd02873f1995f543dece70e70c1f22cbdb7dcbf63e1c5c205966ac70e'}, 'line-geometry/line-2e3-50.res': {'bytes': 161332, 'sha256': '01f9fd0bf4b5c0c3c1acc170ee82938e24a132696acbfbcfb34573a9d0550c7e'}, 'ru-RU/legacy-separation-profile.json': {'bytes': 6812, 'sha256': '5efd16a324730bc450613c9208eeffee4c303aff6b7ad5ddae0b5458cbe4ba36'}, 'text-unicode/legacy-case-data.json': {'bytes': 35482, 'sha256': '0a7a5dbf1f6e5c97c860e4b13a5f7533b6e766be92487fa43142a8817e14c3e9'}, 'text-unicode/legacy-text-publication-data.json': {'bytes': 19024, 'sha256': '6daddbc9ce3d1d3d2e9e179ce008db60887f2c6a13c52ab8bb47dcb215890673'}}
files=set((root/'src/InkRecognition').glob('*.cs'))
for folder in ['LegacyGlobalTextPublication.Probe','LegacyGlobalTextPublication.ScoreDiagnostic']:
 files.update(q for q in(root/'tools'/folder).rglob('*.cs')if not{'obj','bin'}&set(q.relative_to(root/'tools'/folder).parts));files.add(root/'tools'/folder/(folder+'.csproj'))
files.add(root/'src/InkRecognition/InkRecognition.csproj');files.add(root/'global.json');files.update((root/'tools/handwriting').glob('*.py'));files.update((root/'tools/handwriting').glob('*.json'));files.update([Path(__file__),Path(__file__).with_name('publication_portable_pdb.py'),Path(__file__).with_name('publication_core_source_census.py'),Path(__file__).with_name('publication-core-source-admissions.json'),Path(__file__).with_name('test_publication_source_census.py')])
for name,pinvalue in MODELS.items():
 path=root/'models'/name;assert path.stat().st_size==pinvalue['bytes']and sha(path)==pinvalue['sha256'];files.add(path)
inputs=[pin(path)for path in sorted(files)];report['InputPins']=inputs;save()
try:
 runtime_log=run('required-runtime',['dotnet','--list-runtimes'],45)
 assert re.search(r'^Microsoft\.NETCore\.App 10\.0\.12 \[',runtime_log.read_text(encoding='utf8'),re.M),'Provision SDK10.0.401 test runtime before this gate; no roll-forward substitution'
 proof=load('publication_portable_pdb',Path(__file__).with_name('publication_portable_pdb.py'))
 contract=load('publication_flavor_source_contract',root/'tools/handwriting/publication_diagnostic_flavor_source_contract.py');report['SourceFlavor']=contract.verify(root,require_meter_v2=True)
 census=load('publication_core_source_census',Path(__file__).with_name('publication_core_source_census.py'));report['CoreSourceCensus']=census.verify(root)
 run('core-source-census-controls',[sys.executable,'-B',str(Path(__file__).with_name('test_publication_source_census.py')),'--root',str(root)],45)
 ordinary_authors={p.resolve()for p in(root/'src/InkRecognition').glob('*.cs')};assert len(ordinary_authors)==census.DEFAULT_CORE_COUNT
 compiled={}
 for mode,enabled,project,assembly in [('default','false','LegacyGlobalTextPublication.Probe','LegacyGlobalTextPublication.Probe'),('diagnostic','true','LegacyGlobalTextPublication.ScoreDiagnostic','InkRecognition.Probe')]:
  art=out/mode;assert not art.exists();flags=['-c','Release','--artifacts-path',str(art),'-m:1','--disable-build-servers','-nodeReuse:false','-p:UseSharedCompilation=false','-p:DebugType=portable','-p:DebugSymbols=true','-p:Optimize=true','-p:ContinuousIntegrationBuild=false','-p:DeterministicSourcePaths=false','-p:PathMap=','-p:RuntimeFrameworkVersion=10.0.12','-p:RollForward=Disable','-p:BlurMaskPublicationDiagnostics='+enabled]
  mode_report={};report['Modes'][mode]=mode_report
  for name,dllname in [('InkRecognition','InkRecognition'),(project,assembly)]:
   src=root/('src/InkRecognition'if name=='InkRecognition'else'tools/'+name)
   run('build-'+mode+'-'+name,['dotnet','build',str(src/(name+'.csproj'))]+flags+([]if name=='InkRecognition'else['-p:BuildProjectReferences=false']),600)
   dll=art/'bin'/name/'release'/(dllname+'.dll');authors=ordinary_authors.copy()if name=='InkRecognition'else{q.resolve()for q in src.glob('*.cs')}
   if name=='InkRecognition'and enabled=='true':authors.update(q.resolve()for q in(root/'tools/LegacyGlobalTextPublication.ScoreDiagnostic/Source').glob('*.cs'))
   expected=(census.DIAGNOSTIC_CORE_COUNT if enabled=='true'else census.DEFAULT_CORE_COUNT)if name=='InkRecognition'else 1;assert len(authors)==expected
   result=proof.audit_pdb(dll.with_suffix('.pdb'),dll,authors,art/'obj'/name);assert result['authoredDocuments']==expected;mode_report[name]=result
   for q in dll.parent.iterdir():
    if q.is_file():compiled_inputs[str(q)]=pin(q)
   for q in result['documents']:
    if q['kind']=='generated':compiled_inputs[q['path']]={k:q[k]for k in('path','bytes','sha256')}
   if name=='InkRecognition':
    assert (b'LegacyPublicationScoreTrace'in dll.read_bytes())==(enabled=='true'),'Compiled flavor metadata does not match source selection'
    compiled[mode]=dll
   else:
    assert dll.with_name('InkRecognition.dll').read_bytes()==compiled[mode].read_bytes(),'Entry/library flavor mismatch'
    opts=json.loads(dll.with_suffix('.runtimeconfig.json').read_bytes())['runtimeOptions'];assert opts['tfm']=='net10.0'and opts['framework']=={'name':'Microsoft.NETCore.App','version':'10.0.12'}and opts['rollForward']=='Disable'
    mode_report['EntryDLL']=str(dll);mode_report['RuntimeConfig']=pin(dll.with_suffix('.runtimeconfig.json'))
  save()
 # No model target executes until BOTH libraries and entries pass authored PE/PDB/runtime/flavor checks.
 verify_execution_inputs();report['CompiledPins']=list(compiled_inputs.values());report['PdbGateComplete']=True;save()
 calls=[('default',capture,[]),('diagnostic',out/'diagnostic-no-trace.json',[]),('diagnostic',out/'diagnostic-trace.json',['--score-trace'])]
 for i,(mode,target,extra)in enumerate(calls,1):
  verify_execution_inputs();assert not target.exists();report['ActualInvocations']+=1;save();run('actual-'+str(i)+'-'+mode,['dotnet','--fx-version','10.0.12','--roll-forward','Disable',report['Modes'][mode]['EntryDLL'],str(root/'models'),str(target)]+extra,45);assert target.is_file();report.setdefault('Captures',[]).append(pin(target));save()
 verify_execution_inputs()
 run('paired-independent-audit',[sys.executable,'-B',str(root/'tools/LegacyGlobalTextPublication.Probe/audit_output.py'),str(root/'models/ru-RU/ru_RU-ak-cur.lite.res'),str(capture),str(out/'diagnostic-trace.json'),str(out/'diagnostic-no-trace.json')],45)
 report['PASS']=True
finally:
 verify_execution_inputs()
 report['InputsUnchanged']=True;save()
print('PASS default937/test-only939 PDB/runtime gates and3 paired publication calls; full producer operands remain explicit; no quality claim.')
