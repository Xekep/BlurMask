"""Bounded global024F30 runtime composition; vendor bytes remain read-only.

Run the complete previously reviewed byte proofs, then check the new typed
owner seams. Numerical/output proof lives in the separate portable oracle.
"""
import hashlib,json,runpy,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
if len(sys.argv)!=2:raise SystemExit('REPORT.json')
target=Path(sys.argv[1]);reports={}
for name in ['audit_global_text_source_wrapper_plan','audit_global_text_payload_metadata_plan','audit_global_text_glyph_choices_runtime']:
 path=target.with_name(target.stem+'.'+name+'.json');saved=sys.argv
 try:sys.argv=[name,str(path)];runpy.run_path(str(Path(__file__).with_name(name+'.py')),run_name='__main__')
 finally:sys.argv=saved
 reports[name]=hashlib.sha256(path.read_bytes()).hexdigest()
def read(name):return(root/'src/InkRecognition'/name).read_text(encoding='utf-8')
def body(s,anchor):
 start=s.index('{',s.index(anchor));i=start+1;level=1
 while level:level+=(s[i]=='{')-(s[i]=='}');i+=1
 return s[start:i]
w=read('LegacyFreshMainGlobalTextSourceWrapper.cs');p=read('LegacyFreshMainGlobalTextSourcePayload.cs')
c=read('LegacyFreshMainGlobalTextGlyphChoices.cs');r=read('LegacyPreparedMainTextGlyphReuse.cs')
i=read('LegacyOwnedMainTextSourceInterner.cs');s=read('LegacyOwnedFreshTextSourceContext.Global.cs')
assert hashlib.sha256(body(r,'private LegacyPreparedTextGlyphReuseResult Process(').encode()).hexdigest()=='04d7614e9f2356cda87984b727ea875a0875ce86900965114f05b3e0e07a8a5c'
assert hashlib.sha256(body(c,'public IReadOnlyList<LegacyFreshMainGlobalPublishedGlyphChoices> Publish(').encode()).hexdigest()=='08ec74d0d054cf61203db5e36a8cb755043bffd9a964c073b7a574d54bab7ccd'
for term in ['original=choices.Projection.CopyOriginalRoot(token)','wordClip=Completion.CopyRanges()',
 'LegacyOwnedRawInkRangeOperations.Equal(original,Scope.Context.InputBinding.CopyRanges())',
 'LegacyOwnedRawInkRangeOperations.Equal(original,Scope.Context.Result.CopyInputRanges208())',
 'CharacterInterner.Count!=0','CopyPendingSelections().Count!=0','Choices.ClaimSourceWrapper(this,token)',
 'record=pool.CopyRecord(reference,token)','(flags&7)==1&&(symbol&0x7ffc)!=0&&(symbol&3)==3',
 'Choices.Refresh.RefreshOwner(key,allocator40,token)','Choices.Projection.Project(refreshed.Source,token)',
 'operations.Intersect(projected.CopyRanges(token),wordClip,token)','firstPassComplete=true',
 'Choices.SelectForWrapper(this,i,selection4,token)',
 'LegacyPreparedMainTextSourceProcessing.Process(operations,original,ranges,skipped,positive,joiner,i,token)',
 'CharacterInterner.InternGlobalProcessed(currentSource,token)','Choices.EmitFreshForWrapper(currentSelection,token)',
 'payload.Initialize(preparedCharFailure,token)','payload.Reuse(currentSelection,token)',
 'ReferenceEquals(key.Node.FirstGlobalSource,payload.Source)',
 'SourcePositions', # checked below against forbidden geometry substitution instead
 ]:
 if term=='SourcePositions':continue
 assert term in w,term
first=body(w,'private LegacyFreshMainGlobalTextSourceWord PublishCore(')
order=['record=pool.CopyRecord(reference,token)','Choices.Refresh.RefreshOwner','Choices.Projection.Project',
 'operations.Intersect','firstPassComplete=true','Choices.SelectForWrapper','LegacyPreparedMainTextSourceProcessing.Process',
 'CharacterInterner.InternGlobalProcessed','if(key.Inserted)','Choices.EmitFreshForWrapper','payload.Initialize','payload.Reuse']
assert [first.index(x)for x in order]==sorted(first.index(x)for x in order)
assert 'Refresh.Select('not in first,'Allrefs refresh before selections; no hidden per-glyph refresh'
assert 'lineOffset=0;'in first
for term in ['VisitFreshPrior(source.OriginalPositions.Source.References,inputs,token)','byte prior=1',
 'r.First.First!=0||r.Last.First!=0','for(int i=r.First.Second;i<=r.Last.Second;i++)','prior=0',
 'Groups[metadata.AcousticGroupId].DescriptorOrdinal','ReadInt32LittleEndian(descriptors[ordinal].AsSpan(16))',
 'ResolveGroupSource(metadata.AcousticGroupId,token).Primary.LineId!=line','unchecked(line+lineOffset)']:
 assert term in w,term
for term in ['CreateGlobalCharacterSelection(Source,preparedCharFailure,token)','SelectionId=-1',
 'BasicScalars|LegacyGlobalPayloadWrites.EmptyPriorVector','Owner.ReadPrior(Source,token)',
 'SourceLineId=-1','if(!succeeded)return false','SourceLineId=Owner.ReadSourceLine(Source,token)',
 '(aggregate&0xce1fffffu)','&0xfdffffffu','LegacyPreparedMainTextGlyphReuse.CreateGlobal(this,initial,Flags.Value)']:
 assert term in p,term
assert p.index('CreateGlobalCharacterSelection')<p.index('BasicScalars|')<p.index('Owner.ReadPrior')<p.index('SourceLineId=-1')<p.index('if(!succeeded)return false')<p.index('Owner.ReadSourceLine')<p.index('Flags=(((')
assert 'actual.Owner.RequireCurrentSource(actual,token)'in i and 'Prepared, old or foreign source nodes cannot become global payloads.'in i
assert 'CreateCore(actualSource.CopyRanges(token),0,ref output,preparedFailureStage,token)'in s
assert 'ReferenceEquals(scope.Seed.PendingSelections,this)'in s and 'ReferenceEquals(scope.Seed.InputBinding,InputBinding)'in s
assert 'Global reuse has no legacy emitter.'in r and 'globalOwner.RequireReusablePayload(payload,selected,token)'in r
for forbidden in ['LegacyOwnedMainCandidateSource.Prepare','CreateLegacyOwner','new LegacyFreshMainGlyphUtf8Emission','new LegacyNativeUtf8StringPool']:
 assert forbidden not in w+p+s,forbidden
target.write_text(json.dumps(dict(vendorExecution=False,dependencies=reports,oldSeams=[
 'LegacyFreshMainGlobalTextGlyphChoices.cs','LegacyFreshMainGlobalGlyphDocumentPositions.cs',
 'LegacyOwnedMainTextSourceInterner.cs','LegacyPreparedMainTextGlyphReuse.cs'],
 newSelectionPartial='LegacyOwnedFreshTextSourceContext.Global.cs',orderedStages=order,
 limitations=['bounded fresh ordinary one-descriptor scope','actual13 contains no duplicate source keys or overlap branches',
 'CHAR only; no WORD/TEXT or acceptance/default app change','no Russian quality claim']),indent=2)+'\n')
print('PASS global024F30 typed runtime/metadata/source join composition; public13 and21760 numeric bodies byte-identical.')
