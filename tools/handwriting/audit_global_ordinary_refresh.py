"""Static global217670 ownership and mechanical old compact-loop extraction."""
import hashlib,json,re,runpy,subprocess,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('report.json')
root=Path(__file__).resolve().parents[2];target=Path(sys.argv[1]);target.parent.mkdir(parents=True,exist_ok=True)
from global_facade_owner_source_bridge import authenticate_owner_cohort
ownerCohortAuthentication=authenticate_owner_cohort(root)
baseline='532cdcf9d4defdf4b0c133407b35ff90a8494000'
old=subprocess.check_output(['git','show',baseline+':src/InkRecognition/LegacyOwnedMainOrdinaryGlyphRefresh.cs'],cwd=root).decode('utf-8')
def loop(code):
    start=code.index('for (int index = first; index < end; index++)');brace=code.index('{',start);depth=1;i=brace+1
    while depth:
        depth+=(code[i]=='{')-(code[i]=='}');i+=1
    return code[start:i]
old=loop(old)
kernel=(root/'src/InkRecognition/LegacyNativeOrdinaryGlyphRefreshKernel.cs').read_text(encoding='utf-8')
current=loop(kernel)
old=old.replace('compact.AsSpan(index * 40, 40)','compact.Slice(index * 40, 40)').replace('state.Source.BaseSourceKey','baseSourceKey')
start=old.index('ushort emitted, normalized; byte variant = 0;');end=old.index('if (!LegacyNativeGlyphSymbolUpdate.Apply',start)
old=old[:start]+'''var resolved = operations.ResolveSymbol(symbol, token);
                ushort emitted = resolved.EmittedSymbol, normalized = resolved.NormalizedOriginal;
                byte variant = resolved.ExternalVariant;
                '''+old[end:]
old=old.replace('Scoring.ReadSymbolScoreBits(state.Source, normalized, token)','operations.ReadSymbolScoreBits(normalized, token)')
old=old.replace('Scoring.ReadPreparedSymbol(state.Source, normalized, (traits & 0x40) != 0, token)','operations.ReadPreparedSymbol(normalized, (traits & 0x40) != 0, token)')
canonical=lambda code:re.sub(r'\s+','',code)
assert canonical(old)==canonical(current),'Only owner/delegate/Span access substitutions are permitted in the old numeric loop'
digest=hashlib.sha256(canonical(current).encode()).hexdigest()
saved=sys.argv;components=[]
for name in ('audit_global_ordinary_refresh_plan.py','audit_main_global_ordinary_glyph.py'):
    output=target.with_name(target.stem+'.'+name[6:-3]+'.json');path=Path(__file__).with_name(name)
    sys.argv=[str(path)]+(['--output',str(output)]if name=='audit_main_global_ordinary_glyph.py'else[str(output)])
    runpy.run_path(str(path),run_name='__main__')
    components.append(dict(audit=name,report=str(output),sha256=hashlib.sha256(output.read_bytes()).hexdigest()))
sys.argv=saved
refresh=(root/'src/InkRecognition/LegacyFreshMainGlobalOrdinaryGlyphRefresh.cs').read_text(encoding='utf-8')
emission=(root/'src/InkRecognition/LegacyFreshMainGlobalGlyphUtf8Emission.cs').read_text(encoding='utf-8')
producer=(root/'src/InkRecognition/LegacyFreshMainGlobalOrdinaryGlyphProducer.cs').read_text(encoding='utf-8')
session=(root/'src/InkRecognition/LegacyFreshMainGlobalGlyphExecutionSession.cs').read_text(encoding='utf-8')
for text in ('pool=session.Ownership.GlyphPool;producer=session.OrdinaryProducer;',
             'SourceMetadata=session.Associations.Metadata;',
             'compact=source.Selection.Publication.CopyCompactStorage();starts=source.Selection.Publication.CopyNodeStarts2478();',
             'progress.Add(canonicalOwnerKey,state);','state.Count>65536-retainedReferences',
             'Failed global refresh prefix cannot retry.',
             'producer.ResolveForRefresh(count.BaseNode,count.BaseArc,symbol,ct)',
             'Scoring.ReadFirstCandidateScore(count.BaseNode,count.BaseArc,token)',
             'Scoring.CreateRequest(count.BaseNode,count.BaseArc,symbol,end,ct)',
             'Scoring.ReadRequestCost(request,ct)'):
    assert text in refresh,text
order=['progress.Add(canonicalOwnerKey,state);','var count=SourceMetadata.Read(',
       'state.AllocationRequestedBytes=','var fill=SourceMetadata.Read(',
       'state.Allocation=fill.CopySourceAllocation26();','state.FirstScoreBits=',
       'LegacyNativeOrdinaryGlyphRefreshKernel.Apply(','state.Completed=new(']
positions=[refresh.index(v)for v in order];assert positions==sorted(positions)
assert 'internal LegacyFreshMainGlobalOrdinaryGlyphProducer OrdinaryProducer=>ordinary;'in session
assert 'return ResolveOwnedSymbol(symbol,0,node,arc,token);'in producer
ownerSourceBridge=None
if 'PrimaryAlphabet=>Completion.Snapshot.SourcePolicy.Owner.Model.AcousticAlphabet' in emission:
 assert 'PrimaryAlphabet=>Completion.Snapshot.SourcePolicy.Owner.Model.AcousticAlphabet' in emission
else:
 from global_facade_owner_source_bridge import verify_owner_routes
 ownerSourceBridge=verify_owner_routes(root)
 assert 'PrimaryAlphabet=>ActualCompletion.Snapshot.SourcePolicy.Owner.Model.AcousticAlphabet' in emission
assert 'if((glyph.Symbol.KindFlags&7)==4)'in emission
assert 'TopOwner.GlyphPool.CopyRecord(glyph.Reference,token).AsSpan().SequenceEqual(glyph.ReadRecord())'in emission
assert 'actualControlLabelLookup:text=>ReadControlLabel(text,token)'in emission
assert 'return ushort.MaxValue;'in emission
target.write_text(json.dumps(dict(noVendorExecution=True,ownerCohortAuthentication=ownerCohortAuthentication,ownerSourceBridge=ownerSourceBridge,baseline=baseline,
    mechanicalCompactLoopSha256=digest,substitutions=['Span.AsSpan to Span.Slice','bound base source key',
    'scalar resolver delegate','score and cost delegates'],components=components,
    globalOrderedStages=order,scope='same actual postV68 global pool/acoustic AL/session/overlay; empty control2A/non-null empty alternate',
    failure='Native4000 attempted bit is represented only by a poisoned typed progress owner, not invented raw64 header bytes or native failed-refresh replay',
    remaining=['WORD glyph source choices','source-ready CHAR/WORD/TEXT','application acceptance']),indent=2)+'\n',encoding='utf-8')
print('PASS mechanical old compact-loop identity '+digest+'; actual global owner/scorer/emitter bindings and composed PE pins.')
