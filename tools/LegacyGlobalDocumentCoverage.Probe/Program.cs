using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length!=3)throw new ArgumentException("MODELS RECORDED-MANIFEST CAPTURE");
string m=args[0];byte[] R(string n)=>File.ReadAllBytes(Path.Combine(m,n));
byte[] ak=R("ru-RU/ru_RU-ak-cur.lite.res");
var models=LegacyFreshMainRecognitionContext.Load(ak,R("ru-RU/ru_RU-lk-text.lite.res"),R("ru-RU/legacy-engine-profile.json"),R("line-geometry/line-2e3-30.res"),R("line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(R("ru-RU/legacy-separation-profile.json"));
using var cs=File.OpenRead(Path.Combine(m,"text-unicode/legacy-case-data.json"));using var ps=File.OpenRead(Path.Combine(m,"text-unicode/legacy-text-publication-data.json"));
using var ns=File.OpenRead(Path.Combine(m,"text-unicode/legacy-normalization-data.json"));using var cis=File.OpenRead(Path.Combine(m,"text-unicode/legacy-circled-payload-data.json"));
var cd=LegacyNativeCaseData.Load(cs);var pd=LegacyNativeTextPublicationData.Load(ps);var norm=LegacyNativeUnicodeNormalizationData.Load(ns);var circles=LegacyNativeCircledPayloadData.Load(cis);
InkStroke M()=>new(new InkPoint[]{new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)});
var samples=new List<(string Name,InkStroke[] Ink)>{("M",[M()]),("MIR",[M(),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})]),("I",[new(new InkPoint[]{new(0,10),new(0,90)})]),("II",[new(new InkPoint[]{new(0,10),new(0,90)}),new(new InkPoint[]{new(40,10),new(40,90)})])};
var recorded=RecordedInputs.Load(args[1]);for(int i=0;i<recorded.Samples.Length;i++)samples.Add((recorded.Names[i],recorded.Samples[i]));


using var file=File.Create(args[2]);using var writer=new Utf8JsonWriter(file);writer.WriteStartObject();writer.WriteStartArray("samples");
int completed=0,refused=0,guards=0;
foreach(var(name,raw)in samples)
{
 using var scope=Scope(raw);var parent=Parent(scope);
 Reject(()=>LegacyFreshMainGlobalDocumentCoverage.Prepare(parent));
 parent.Publish(Enumerable.Range(0,40).Select(i=>(byte)(i*29+7)).ToArray(),new byte[8160]);
 var before=parent.CopyPendingSelections().ToArray();byte[] lines=parent.CopySourceLines56(),word=parent.WordSource.CopyChoiceScalar48();
 var coverageSeed=scope.Seed;var immutableBefore=CoverageSnapshot(parent);
 void Unchanged(){if(!ReferenceEquals(scope.Seed,coverageSeed)||CoverageSnapshot(parent)!=immutableBefore||!parent.CopyPendingSelections().SequenceEqual(before)||!parent.CopySourceLines56().SequenceEqual(lines)||!parent.WordSource.CopyChoiceScalar48().SequenceEqual(word))throw new Exception("Coverage mutated the actual pending/line/WORD state.");}
 writer.WriteStartObject();writer.WriteString("name",name);
 writer.WriteStartArray("counts");foreach(var s in scope.Document.Strokes)writer.WriteNumberValue(s.Points.Count);writer.WriteEndArray();
 Ranges("word",parent.OriginalWordSource.CopyOriginalKey());Ranges("text",parent.Text.CopyRanges());
 using var stop=new CancellationTokenSource();stop.Cancel();Reject(()=>LegacyFreshMainGlobalDocumentCoverage.Prepare(parent,stop.Token));Unchanged();
 try
 {
  var result=LegacyFreshMainGlobalDocumentCoverage.Prepare(parent);completed++;
  writer.WriteBoolean("completed",true);writer.WriteNumber("lineCount",result.LineCount);writer.WriteNumber("last",result.LastReferencedLine);
  writer.WriteNumber("wordVisits",result.WordVisits);writer.WriteNumber("helpers",result.LineHelperCalls);writer.WriteNumber("adjacent",result.AdjacentCharacterChecks);
  writer.WriteBoolean("ordinary",result.UsesOrdinaryWordRanges);writer.WriteNumber("subtractions",result.CharacterSubtractions);
  Ranges("normalizedWord",result.CopyNormalizedWordCoverageRanges());
  if(!result.CopyWordRanges().SequenceEqual(parent.OriginalWordSource.CopyOriginalKey()))throw new Exception("Raw WORD identity changed.");
  var normalizedCopy=result.CopyNormalizedWordCoverageRanges();normalizedCopy.AsSpan().Clear();if(result.CopyNormalizedWordCoverageRanges().SequenceEqual(normalizedCopy))throw new Exception("Normalized union aliases caller.");
  Ranges("initial",result.CopyInitialCharacterCoverage());Ranges("remaining",result.CopyRemainingCharacterCoverage());
  writer.WriteStartArray("keys");foreach(var key in result.CopyCharacterKeys()){writer.WriteStartArray();foreach(var r in key){writer.WriteStartArray();writer.WriteNumberValue(r.First.Value);writer.WriteNumberValue(r.Last.Value);writer.WriteEndArray();}writer.WriteEndArray();}writer.WriteEndArray();
  var copy=result.CopyCharacterKeys();copy[0].AsSpan().Fill(default);var bytes=result.CopySourceLines56();bytes.AsSpan().Fill(255);
  if(result.CopyCharacterKeys()[0].SequenceEqual(copy[0])||result.CopySourceLines56().SequenceEqual(bytes))throw new Exception("Diagnostic arrays alias retained coverage.");
  result.Validate();Unchanged();scope.Dispose();Reject(()=>result.Validate());Reject(()=>result.CopyWordRanges());Reject(()=>result.CopyCharacterKeys());Reject(()=>result.CopyNormalizedWordCoverageRanges());
 }
 catch(NotSupportedException e)when(e.Message.StartsWith("Unequal actual WORD/TEXT")){Unchanged();refused++;writer.WriteBoolean("completed",false);writer.WriteString("refusal","238B0");}
 // Scope can have been deliberately closed after successful diagnostics.
 if(!parent.IsComplete||parent.IsFaulted)throw new Exception("Read-only coverage changed parent publication.");
 writer.WriteEndObject();
}
writer.WriteEndArray();
{
 using var scope=Scope([M()]);var parent=Parent(scope);parent.Publish(new byte[40],new byte[8160]);var result=LegacyFreshMainGlobalDocumentCoverage.Prepare(parent);
 var pending=parent.CopyPendingSelections().ToArray();long next=scope.SharedState.Selections.NextSelectionId,output=-1;
 if(scope.SharedState.Selections.CreatePrepared(parent.Text.CopyRanges(),0,ref output,1)is not null||!scope.SharedState.Selections.Failed||scope.SharedState.Selections.NextSelectionId!=next+1||!parent.CopyPendingSelections().SequenceEqual(pending))throw new Exception("Prepared registration refusal did not retain counter-only failure.");
 Reject(()=>result.Validate());Reject(()=>result.CopySourceLines56());Reject(()=>LegacyFreshMainGlobalDocumentCoverage.Prepare(parent));
}
if(LegacyFreshMainGlobalDocumentCoverage.PreflightUnionWork(0)!=0||LegacyFreshMainGlobalDocumentCoverage.PreflightUnionWork(4096)!=16_777_216)throw new Exception("Union budget inclusive boundary changed.");
Reject(()=>LegacyFreshMainGlobalDocumentCoverage.PreflightUnionWork(-1));Reject(()=>LegacyFreshMainGlobalDocumentCoverage.PreflightUnionWork(4097));Reject(()=>LegacyFreshMainGlobalDocumentCoverage.PreflightUnionWork(int.MaxValue));
writer.WriteNumber("unionBudgetGuards",3);
writer.WriteBoolean("poisonedCounterWithoutCommand",true);writer.WriteNumber("completed",completed);writer.WriteNumber("refused",refused);writer.WriteNumber("guards",guards);writer.WriteEndObject();writer.Flush();
if(completed!=13||refused!=0)throw new Exception("Actual equality branch coverage changed.");
Console.WriteLine($"PASS13 global post-TEXT owners: {completed} exact coverage/{refused} explicit238B0 refusals/{guards} guards.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected refusal.");}
void Ranges(string name,IEnumerable<LegacyNativeInkPositionRange> ranges){writer.WriteStartArray(name);foreach(var r in ranges){writer.WriteStartArray();writer.WriteNumberValue(r.First.Value);writer.WriteNumberValue(r.Last.Value);writer.WriteEndArray();}writer.WriteEndArray();}
LegacyLibraryGlobalTextPublicationScope Scope(InkStroke[] ink){var d=LegacyOwnedRawInkDocument.Create(ink,default);var original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(d),["a","b"]);original.Run();var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,original.State.Inputs[1]);selected.Run();return LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,selected,0,table,cd,pd);}
LegacyFreshMainGlobalWordTextPublication Parent(LegacyLibraryGlobalTextPublicationScope scope){scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);var c=scope.CompleteSources(new byte[400],new byte[4],new byte[4]);return LegacyFreshMainGlobalWordTextPublication.Prepare(LegacyFreshMainGlobalTextSourceWrapper.Prepare(scope,c,ak,pd,norm,circles));}

string CoverageSnapshot(LegacyFreshMainGlobalWordTextPublication p)=>string.Join("|",new[]{p.Wrapper.Choices.StringPool.Count.ToString(),p.Scope.SharedState.Selections.NextSelectionId.ToString(),p.Scope.SharedState.Selections.Failed.ToString(),p.WordSource.Flags.ToString(),Convert.ToHexString(p.WordSource.CopyChoiceScalar48()),Convert.ToHexString(p.CopySourceLines56()),string.Join(";",p.CopyPendingSelections().Select(c=>$"{c.SelectionId}:{c.Name}:"+string.Join(",",c.CopyRanges().Select(x=>$"{x.First.Value:X16}-{x.Last.Value:X16}")))),string.Join(";",p.WordSource.CopyCharacterLinks().Select(l=>$"{l.Source.Node.Identity}:{l.Source.CopyAssignedScalars()}:"+Convert.ToHexString(l.Source.Node.CopyRanges16())+":"+string.Join(",",l.Source.CopyRecords().Select(x=>Convert.ToHexString(x.CopyStorage40())+":"+(x.Text is{}t?Convert.ToHexString(t.ReadBytes()):"null"))))) });
