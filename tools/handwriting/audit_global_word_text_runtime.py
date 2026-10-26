"""Read-only native proof composition and bounded global WORD/TEXT contracts.

Never loads the vendor DLL. Numerical output is checked separately by the
portable LegacyGlobalWordText.Probe oracle, not by these source assertions.
"""
import argparse,hashlib,json,runpy,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
from global_wordtext_source_bridge import authenticate_word_text_cohort
wordCohortAuthentication=authenticate_word_text_cohort(root)
dependency=a.output.with_name('gwtr-'+hashlib.sha256(str(a.output.resolve()).encode()).hexdigest()[:12]+'.json')
saved=sys.argv
try:
 sys.argv=['audit_global_word_text_join_plan.py',str(dependency)]
 runpy.run_path(str(Path(__file__).with_name('audit_global_word_text_join_plan.py')),run_name='__main__')
finally:sys.argv=saved
contracts={
 'LegacyFreshMainGlobalWordTextPublication.cs':[
  'completion.Snapshot.ActiveAlternativeCount60!=1','selectedWordIndex!=0','Scope.SharedState.Words.Count!=0',
  'ReferenceEquals(Scope.Seed,Scope.Context.Result.PublicationSeed)',
  'ReferenceEquals(Scope.Seed.PendingSelections,Scope.SharedState.Selections)',
  'ReferenceEquals(Scope.Seed.StringPool,wrapper.Choices.StringPool)',
  'input.IsKnownText||input.SourceIndex!=(uint)i',
  'wordStorage=completion.Word.Word.CopyStorage();wordReferences=completion.Word.Word.FlatReferences.ToArray()',
  'OriginalWordSource=new(this)','actualLines.Owner.Controller.SourceOffset!=0',
  'LegacyNativeSourceLinePublication.ReadPreparedLineInfo(guide,0)',
  'alphabet.TryGetText((ushort)i,out string value)',
  'ProveScalarBytes(Encoding.UTF8.GetBytes(value),wrapper.Choices.Normalization,token)',
  'raw.Length>4||Encoding.UTF8.GetString(raw).EnumerateRunes().Take(2).Count()>1',
  'LegacyNativeCanonicalScratch.PrepareUtf8Scratch(raw,normalization,token)',
  'scratch.AsSpan(begin+4).IndexOf("\\U000f0001"u8)>=0',
  '(kind&7)==1&&(run&0x7ffc)!=0&&(run&3)==3',
  'The bounded parent admits ordinary scalar alternatives only.',
  'budget.AddSource(owner.Records.Length/40,compactCount,source.ReferenceCount,Scope.Document.ObjectCount)',
  'resolved.ExternalVariant!=0||resolved.EmittedSymbol is>=0xfff9 and<=0xfffc',
  'addedRows>65536-Rows||addedRanges>1_048_576-Ranges',
  'physicalInputs is<1 or>256','long addedRows=(long)existingRows+compactRows',
  '!c.Word.Word.CopyStorage().AsSpan().SequenceEqual(wordStorage)',
  '!c.Source.Selection.Publication.CopyCompactStorage().AsSpan().SequenceEqual(compactStorage)',
  'current.PackedCount!=before.PackedCount||!current.Records.Span.SequenceEqual(before.Records.Span)',
  'Wrapper.ReserveForWord(this,token);entered=true',
  'documentChoiceAllocated=true;Record("document-choice32")',
  'payload.ChoiceCount=1;Record("word-choice48")',
  'payload.Flags=(payload.Flags&~0x800000u)|0x200000',
  'payload!.Flags&=~0x200000u',
  'payload.Flags|=payload.ChoiceFlags','payload.SelectionId=-1',
  'Scope.SharedState.Selections.CreateGlobalWordSelection(payload,fault.Word,token)',
  'bool same=key.Length==1&&SameCoordinate(key[0])',
  'WrapperLineOffset=append;Record("source-line-allocation-refused")',
  'LegacyNativeSourceLinePublication.Append(lines,0,lineInputs,guides,0,token)',
  'text=new(this,textKey)','Scope.SharedState.Selections.CreateGlobalTextSelection(text,fault.Text,token)',
  'catch{faulted=true;stage="fault";Wrapper.FaultReservedWord(this)',
  'if(documentLink){BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(8),WordSource.ChoiceFlags&1023)',
 ],
 'LegacyFreshMainGlobalTextSourceWrapper.cs':[
  'Choices.ClaimSourceWrapper(this,token);reservedWordParent=actual',
  'Choices.FaultSourceWrapper(this)','Choices.ValidateWrapper(this,token)',
  'wordParent?.VisitPositiveSource(this,i,token)','wordParent?.AppendCharacter(this,output[^1],token)',
 ],
 'LegacyOwnedMainTextSourceInterner.cs':[
  'actual.Owner.RequireWordIntern(actual,token)',
  'InternCore(actual.CopyOriginalKey(),null,token,globalWordSource:actual)',
  'ReferenceEquals(value.Node.FirstGlobalWordSource,actual)',
 ],
 'LegacyOwnedFreshTextSourceContext.GlobalWordText.cs':[
  'ReferenceEquals(Document,parent.Scope.Document)',
  'CreateCore(actual.OriginalSource.CopyOriginalKey(),1,ref output,failure,token)',
  'long output=-1;return CreateCore(actual.CopyRanges(),2,ref output,failure,token)',
 ],
 'LegacyFreshMainGlobalWordSourceReceipt.cs':[
  'ranges=owner.Wrapper.Completion.CopyRanges()',
  'SelectionId{get;internal set;}=long.MaxValue',
  'ReferenceEquals(actual.Payload.Node.FirstGlobalSource,actual.Payload.Source)',
  'ordinary=parent.RangeOperations.Union(ordinary,actual.Payload.Source.CopyRanges(token),token)',
  'ChoiceFlags|=rows[actual.Marker].Flags',
 ],
}
texts={}
wordSourceBridge=None
def require_contract(name,text,needle):
 global wordSourceBridge
 if needle in text:return
 from global_wordtext_source_bridge import verify_word_text_source
 wordSourceBridge=verify_word_text_source(root)
 assert needle in wordSourceBridge['ReviewedChangedContracts'].get(name,{}),(name,needle)
for name,needles in contracts.items():
 if name=='LegacyFreshMainGlobalWordTextPublication.cs':
  from publication_compile_default_source import read_default_text
  text,compileDefaultSource=read_default_text(root,name)
 else:text=(root/'src/InkRecognition'/name).read_text(encoding='utf-8-sig')
 texts[name]=text
 for needle in needles:require_contract(name,text,needle)
# Check native ordering independently of probe trace labels.
parent=texts['LegacyFreshMainGlobalWordTextPublication.cs']
sequence=['documentChoiceAllocated=true','stage="word-intern";var node=',
 'payload=new(OriginalWordSource,node.Node)','payload.ChoiceCount=1;',
 'stage="characters";characters=','payload.Flags|=payload.ChoiceFlags;',
 'CreateGlobalWordSelection(payload','documentLink=true;',
 'LegacyNativeSourceLinePublication.Append(lines','text=new(this,textKey)',
 'CreateGlobalTextSelection(text']
if all(s in parent for s in sequence):
 offsets=[parent.index(s)for s in sequence];assert offsets==sorted(offsets)
else:
 from global_wordtext_source_bridge import verify_word_text_source
 wordSourceBridge=verify_word_text_source(root)
 assert wordSourceBridge['CurrentSelectedOnlyOrderVerified']
# Compare the old numerical AVL body after removing only the approved typed
# constructor sidecar argument. No comparator/balancing/coercion edits allowed.
interner=texts['LegacyOwnedMainTextSourceInterner.cs']
start=interner.index('    {',interner.index('private LegacyOwnedMainTextSourceInternResult InternCore'))
depth=0
for end in range(start,len(interner)):
 if interner[end]=='{':depth+=1
 elif interner[end]=='}':
  depth-=1
  if depth==0:break
body=interner[start:end+1].lstrip().replace(',globalSource,globalWordSource)',',globalSource)')
body_hash=hashlib.sha256(body.encode()).hexdigest()
assert body_hash=='fe312e01172a6989f9f9117df9b87910f144d9494c56dda91d9744edd146790a',body_hash
report=dict(status='PASS bounded runtime contracts; not full document/recognition completion',
 vendorExecution=False,ChangedMatchingBoundSourceView=wordCohortAuthentication.get('CurrentMatchingBoundPolicies',[]),PreflightRuntimeEquivalenceClaim=False,compileDefaultSource=compileDefaultSource,wordCohortAuthentication=wordCohortAuthentication,wordSourceBridge=wordSourceBridge,dependencySha256=hashlib.sha256(dependency.read_bytes()).hexdigest(),
 sourceContracts=sum(map(len,contracts.values())),nativeOrderStages=len(sequence),
 oldInternCoreNormalizedSha256=body_hash,
 admitted='fresh exact global1/oneWORD ordinary scalar outputs; prepared skip labelled separately',
 missing=['238B0 real geometric owner','025940/024A90 split/insertion','existing document/DOM bridge','Russian cursive quality'])
a.output.write_text(json.dumps(report,indent=2)+'\n')
print('PASS bounded global WORD/TEXT static contracts, native-order composition, unchanged old AVL numerical body.')
