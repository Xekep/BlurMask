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




var output=new List<object>();int guards=0,complete=0,refused=0,matrices=0;
foreach(var(name,raw)in samples)
{
 using var scope=Scope(raw);var seed=scope.Seed;var initial=seed.CurrentTransforms;var seed20=seed.CopyInitialScalarStorage20();var parent=Parent(scope);parent.Publish(new byte[40],new byte[8160]);
 LegacyFreshMainGlobalDocumentCoverage coverage;
 try{coverage=LegacyFreshMainGlobalDocumentCoverage.Prepare(parent);}
 catch(NotSupportedException e)when(e.Message.StartsWith("Unequal actual WORD/TEXT")){output.Add(D(("name",name),("completed",false),("refusal","238B0")));refused++;continue;}
 var state=LegacyFreshMainGlobalDocumentSelectedSources.Prepare(coverage);var index=LegacyFreshMainGlobalDocumentSourceIndices.Prepare(state).Run();var cleanup=LegacyFreshMainGlobalControlCleanup.Prepare(index);
 Reject(()=>LegacyFreshMainGlobalInputTransforms.Prepare(cleanup));cleanup.Run();Reject(()=>LegacyFreshMainGlobalInputTransforms.Prepare(cleanup,new(true)));
 var stage=LegacyFreshMainGlobalInputTransforms.Prepare(cleanup);Reject(()=>stage.ValidateCurrent());Reject(()=>stage.Run(new(true)));Reject(()=>LegacyFreshMainGlobalInputTransforms.Prepare(cleanup));
 var pending=parent.CopyPendingSelections().ToArray();stage.Run();stage.ValidateCurrent();Reject(()=>stage.Run());
 if(!ReferenceEquals(stage.Seed,seed)||!stage.ReleasedInitialOwner||ReferenceEquals(seed.CurrentTransforms,initial)||!ReferenceEquals(seed.InitialTransforms,initial)||!seed20.AsSpan().SequenceEqual(seed.CopyInitialScalarStorage20())||!pending.SequenceEqual(parent.CopyPendingSelections())||stage.TransformCount110!=0||!stage.VectorWasSwapped||!stage.ExportSucceeded||!stage.FollowingStage026EF0Executed)throw new Exception("Actual seed/vector lifetime or prior snapshot changed.");
 var geometry=stage.SourceGeometry;matrices+=geometry.MatricesResolved;complete++;
 output.Add(D(("name",name),("completed",true),("seed20",Convert.ToHexString(seed20)),("tag",seed.SourceTag30),("alternatives",stage.DocumentAlternativeCountD8),("lines",parent.LineCount),("transforms",stage.TransformCount110),("daf",seed.CopyDafInputRows32().Length/32),("swapped",stage.VectorWasSwapped),("following",stage.FollowingStage026EF0Executed),("matrixCount",geometry.MatricesResolved),("matrices",Convert.ToHexString(geometry.CopyResolvedMatrices())),("matches",geometry.CopyMatchedViewIndices()),("ranges",Ranges(geometry.CopyRanges())),("registry",stage.OriginalSelectedSourceInput.RegistryId),("fragments",geometry.CopyFragments().Select(f=>new long[]{f.SourceIndex,f.PointCount,f.FirstPoint,f.LastPoint,BitConverter.SingleToInt32Bits(f.FirstFraction),BitConverter.SingleToInt32Bits(f.LastFraction),f.FirstClosed?1:0,f.LastClosed?1:0}).ToArray()),("prefixes",Prefixes(stage))));
 scope.Dispose();Reject(()=>stage.ValidateCurrent());
}
var failures=new List<object>();
foreach(var at in Enum.GetValues<LegacyGlobalInputTransformStage>())
{
 using var scope=Scope([M()]);var cleanup=Cleanup(scope);var stage=LegacyFreshMainGlobalInputTransforms.Prepare(cleanup);using var cancel=new CancellationTokenSource();
 Reject(()=>stage.RunDiagnostic(s=>{if(s==at)cancel.Cancel();},cancel.Token));if(!stage.IsFaulted||stage.IsComplete)throw new Exception("Cancellation did not poison transform stage.");Reject(()=>stage.Run());Reject(()=>stage.ValidateCurrent());Reject(()=>LegacyFreshMainGlobalInputTransforms.Prepare(cleanup));
 failures.Add(D(("stage",at.ToString()),("prefixes",Prefixes(stage)),("retainedCount",scope.Seed.CurrentTransforms.Count)));
}
var failedExports=new List<object>();
for(int i=0;i<3;i++)
{
 using var scope=Scope([M()]);var cleanup=Cleanup(scope);var stage=LegacyFreshMainGlobalInputTransforms.Prepare(cleanup);byte[] bytes=Enumerable.Range(0,i*64).Select(n=>(byte)(n*17+3)).ToArray();var prefix=LegacyDocumentTransformVector.Prepared(bytes,Enumerable.Range(0,i).Select(_=>scope.Seed.InputBinding.CopyRanges()).ToArray());stage.RunPreparedExport(prefix,false);
 if(stage.IsComplete||!stage.IsFaulted||stage.ExportSucceeded||stage.FollowingStage026EF0Executed||!stage.VectorWasSwapped||!ReferenceEquals(scope.Seed.CurrentTransforms,prefix)||!stage.CopyTransformScalarStorage64().AsSpan().SequenceEqual(bytes))throw new Exception("Failed export prefix not retained before status gate.");
 Reject(()=>stage.Run());Reject(()=>stage.ValidateCurrent());Reject(()=>LegacyFreshMainGlobalInputTransforms.Prepare(cleanup));
 failedExports.Add(D(("count",i),("scalars",Convert.ToHexString(stage.CopyTransformScalarStorage64())),("prefixes",Prefixes(stage))));
}
var invalidations=new List<object>();
foreach(bool dispose in new[]{false,true})
{
 using var scope=Scope([M()]);var cleanup=Cleanup(scope);var stage=LegacyFreshMainGlobalInputTransforms.Prepare(cleanup);
 Reject(()=>stage.RunDiagnostic(s=>{if(s==LegacyGlobalInputTransformStage.Complete){if(dispose)scope.Dispose();else{long id=-1;_=scope.SharedState.Selections.CreatePrepared([],0,ref id,1);}}}));if(stage.IsComplete||!stage.IsFaulted)throw new Exception("Late source invalidation escaped.");Reject(()=>stage.Run());Reject(()=>stage.ValidateCurrent());invalidations.Add(D(("disposed",dispose),("prefixes",Prefixes(stage))));
}
var preSwap=new List<object>();
for(int kind=0;kind<3;kind++)
{
 using var scope=Scope([M()]);var cleanup=Cleanup(scope);var stage=LegacyFreshMainGlobalInputTransforms.Prepare(cleanup);var initial=scope.Seed.CurrentTransforms;var foreign=LegacyDocumentTransformVector.Fresh();
 Reject(()=>stage.RunDiagnostic(s=>{if(s==LegacyGlobalInputTransformStage.Exported){if(kind==0)scope.Dispose();else if(kind==1){long id=-1;_=scope.SharedState.Selections.CreatePrepared([],0,ref id,1);}else scope.Seed.SwapTransforms(foreign,true);}}));
 if(stage.VectorWasSwapped||stage.IsComplete||!stage.IsFaulted||!ReferenceEquals(scope.Seed.CurrentTransforms,kind==2?foreign:initial))throw new Exception("Invalid owner mutated seed at export boundary.");Reject(()=>stage.Run());Reject(()=>stage.ValidateCurrent());preSwap.Add(D(("kind",kind),("prefixes",Prefixes(stage))));
}
using(var f=File.Create(args[2]))using(var j=new Utf8JsonWriter(f))WriteJson(j,D(("samples",output),("completed",complete),("refused",refused),("matrices",matrices),("failures",failures),("failedExports",failedExports),("invalidations",invalidations),("preSwap",preSwap),("guards",guards)));
if(complete!=11||refused!=2||matrices!=11)throw new Exception("Actual transform coverage changed.");
Console.WriteLine($"PASS13 genuine global INPUT transforms/{complete} complete/{refused} coverage refusals/{matrices} resolved matrices/5 cancellation prefixes/3 failed exports/2 invalidations/{guards} guards; no final acceptance.");
LegacyFreshMainGlobalControlCleanup Cleanup(LegacyLibraryGlobalTextPublicationScope scope){var p=Parent(scope);p.Publish(new byte[40],new byte[8160]);return LegacyFreshMainGlobalControlCleanup.Prepare(LegacyFreshMainGlobalDocumentSourceIndices.Prepare(LegacyFreshMainGlobalDocumentSelectedSources.Prepare(LegacyFreshMainGlobalDocumentCoverage.Prepare(p))).Run()).Run();}
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected transform refusal.");}
ulong[][] Ranges(IEnumerable<LegacyNativeInkPositionRange> r)=>r.Select(x=>new[]{x.First.Value,x.Last.Value}).ToArray();
object[] Prefixes(LegacyFreshMainGlobalInputTransforms c)=>c.CopyPrefixes().Select(p=>(object)D(("stage",p.Stage),("swapped",p.VectorSwapped),("exported",p.ExportSucceeded),("following",p.InputSpansCompleted),("count",p.TransformCount))).ToArray();
LegacyLibraryGlobalTextPublicationScope Scope(InkStroke[] ink){var d=LegacyOwnedRawInkDocument.Create(ink,default);var original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(d),["a","b"]);original.Run();var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,original.State.Inputs[1]);selected.Run();return LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,selected,0,table,cd,pd);}
LegacyFreshMainGlobalWordTextPublication Parent(LegacyLibraryGlobalTextPublicationScope scope){scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);var c=scope.CompleteSources(new byte[400],new byte[4],new byte[4]);return LegacyFreshMainGlobalWordTextPublication.Prepare(LegacyFreshMainGlobalTextSourceWrapper.Prepare(scope,c,ak,pd,norm,circles));}

Dictionary<string,object?> D(params (string Name,object? Value)[] fields)=>fields.ToDictionary(p=>p.Name,p=>p.Value);
void WriteJson(Utf8JsonWriter w,object? value)
{
 switch(value)
 {
  case null:w.WriteNullValue();break;
  case string text:w.WriteStringValue(text);break;
  case bool flag:w.WriteBooleanValue(flag);break;
  case int number:w.WriteNumberValue(number);break;
  case uint number:w.WriteNumberValue(number);break;
  case long number:w.WriteNumberValue(number);break;
  case ulong number:w.WriteNumberValue(number);break;
  case Dictionary<string,object?> values:w.WriteStartObject();foreach(var pair in values){w.WritePropertyName(pair.Key);WriteJson(w,pair.Value);}w.WriteEndObject();break;
  case System.Collections.IEnumerable values:w.WriteStartArray();foreach(var item in values)WriteJson(w,item);w.WriteEndArray();break;
  default:throw new Exception("Untracked diagnostic JSON type "+value.GetType());
 }
}
