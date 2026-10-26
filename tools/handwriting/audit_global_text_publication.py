"""Static-only bounded global pre-NN and TOP.V68 composition. No SDK run."""
import hashlib,json,runpy,sys,subprocess
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
target=Path(sys.argv[1]);target.parent.mkdir(parents=True,exist_ok=True);saved=sys.argv;components=[]
for name in ('audit_global_text_publication_plan.py','audit_main_word_ranking.py','audit_main_span_score_publication.py','audit_original_selected_publication.py'):
 p=target.with_name(target.stem+'.'+name[6:-3]+'.json');path=Path(__file__).with_name(name);sys.argv=[str(path),str(p)];runpy.run_path(str(path));components.append(dict(name=name,output=str(p),sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
sys.argv=saved
raw=b.data[b.off(0x18000c943):b.off(0x18000c951)];assert raw.hex()=='c74714ffffffffc74718ffff0d02'
pins={i.address:(i.mnemonic,i.op_str)for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,0x18000c943)}
assert pins=={0x18000c943:('mov','dword ptr [rdi + 0x14], 0xffffffff'),0x18000c94a:('mov','dword ptr [rdi + 0x18], 0x20dffff')}
#The no-property fresh model owner already pins the NEW2B3 factory lineage.
#These two stores prove quota fields16 and18 areFFFF, not zero or configured2.
base='82fd36a67f26e030325d6cd3407f897f359c9273'
path='src/InkRecognition/LegacyFreshMainGlobalPlainRecognition.cs'
old=subprocess.check_output(['git','show',base+':'+path],cwd=root).decode('utf-8')
from publication_compile_default_source import read_default_text
new,compileDefaultSource=read_default_text(root,Path(path).name)
def body(s):return s[s.index('    private LegacyFreshMainGlobalPlainRecognitionResult RunCore('):s.index('\n}\n',s.index('    private LegacyFreshMainGlobalPlainRecognitionResult RunCore('))].replace('\r','')
assert body(old)==body(new),'The owned global numerical/controller body changed'
scope=(root/'src/InkRecognition/LegacyLibraryGlobalTextPublicationScope.cs').read_text(encoding='utf-8')
ordered=['LegacyLibraryTextProcessingContext.CreateForOriginalSelected(actualSelectedInput,token)','context.PrepareInput(token);lease=context.RetainForPublication()','scope=new(context,lease,models)','models.PrepareGlobalDocument(context.Document,resolution,table,caseData,publication,token)']
assert [scope.index(v)for v in ordered]==sorted(scope.index(v)for v in ordered)
completion=(root/'src/InkRecognition/LegacyFreshMainGlobalNoCandidateSourceCompletion.cs').read_text(encoding='utf-8')
orderedRun=['LegacyFreshMainGlobalPublishedQuoteNormalization.ApplyPublished','LegacyFreshMainGlobalResultGlyphNormalization.Prepare','ExportSource(token)','SourcesMarkedComplete=true','LegacyPreparedMainWordRanking.Rank','ApplySpanRanking','LegacyPreparedMainSpanScorePublication.Normalize','ApplySpanScores','CheckboxGuardVerified=true','IsTopV68Completed=true']
assert [completion.index(v)for v in orderedRun]==sorted(completion.index(v)for v in orderedRun)
for guard in ['cuts.Steps.Count!=1','cuts.Steps[0].Kind!=0','cuts.CopyTrailingPendingDescriptorOrdinals().Length!=0','cuts.Steps[0].CopyPendingDescriptorOrdinals().Length!=0','graph.Source.State.Contexts.Count!=0','word.Next is not null','word.Span.Last<word.Span.First','span.Words.Count!=1','span.SelectedOrdinals[0]!=0','(compact[i*40+32]&8)!=0']:
 assert guard in completion,guard
report=dict(vendorExecution=False,compileDefaultSource=compileDefaultSource,portablePreNnOrder=ordered,portableSourceStageOrder=orderedRun,portableScopeGuards=10,components=components,pins={hex(a):' '.join(v)for a,v in pins.items()},quotaProperties16and18='FFFF from actual00C860; scoped NEW/no external properties',base=base,unchangedGlobalRunCoreSha256=hashlib.sha256(body(new).encode()).hexdigest(),scope='preNN same Document/selected INPUT/2DA/shared3E/Seed ->actual global callback ->snapshot/quote/freshF4 ->one ordinary descriptor/one275/one279/no pending/no bit8 count-fill26N/ranking/275scores/absent checkbox. No imported2A, CHAR/TEXT publication, threshold or acceptance.')
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print('PASS global publication:4 static compositions +2 fresh quota stores +unchanged actual RunCore; static SDK bytes only')
