using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
using InkRecognition.Math;

if(args.Length!=4)throw new ArgumentException("pack rawMath2.res official-runtime capture.json");
using var runtime=new OrtRuntime(Path.GetFullPath(args[2]));using var run=runtime.CreateRun(default);
using var stream=File.OpenRead(args[0]);using var bundle=Math2Mode0ColdBundle.Read(runtime,stream,File.ReadAllBytes(args[1]));
int guards=0;
void Reject(Action action)
{try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Required Math field source guard missing.");}
using var canceled=new CancellationTokenSource();canceled.Cancel();
InkStroke[] ink=[new([new InkPoint(0,30),new(0,0),new(10,18),new(20,0),new(20,30)]),new([new InkPoint(35,0),new(35,30)])];
var actual=new List<Actual>();
foreach(bool reversed in new[]{false,true})
{
 var input=reversed?ink.Reverse().ToArray():ink;
 var prepared=Math2Mode0RawSourcePositions.PrepareFeatures(input,default);
 var view=LegacyFreshRawInkCurrentView.Prepare(prepared.Positions.Document);
 using var field=Math2Mode0OriginalMathFieldSource.Prepare(view,"formula");
 using var foreign=Math2Mode0OriginalMathFieldSource.Prepare(view,"formula");
 if(!ReferenceEquals(field.Document,foreign.Document)||ReferenceEquals(field.FieldIdentity,foreign.FieldIdentity))throw new InvalidDataException("Same-document foreign field control missing.");
 if(ReferenceEquals(field.OriginalNameOwner,field.SelectedNameOwner)||
  ReferenceEquals(field.ReadCanonicalName(false,7),field.ReadCanonicalName(true,7))||
  !ReferenceEquals(field.ReadCanonicalName(true,7).Owner,field.SelectedNameOwner)||
  !ReferenceEquals(field.ReadCanonicalName(true,7),field.ReadCanonicalName(true,7)))throw new InvalidDataException("Selected BEE names lost local ownership.");
 if(ReferenceEquals(field.OriginalInkIdentity,field.SelectedInkIdentity)||ReferenceEquals(field.OriginalSelectionIdentity,field.SelectedSelectionIdentity))throw new InvalidDataException("Original/selected owners were collapsed.");
 Reject(()=>Math2Mode0RawOnlySource.PrepareColdWithSelectedMathField(runtime,bundle,run,field,canceled.Token));
 if(field.CopySnapshot().InferenceClaimed)throw new InvalidDataException("Preentry cancellation consumed field.");
 using var source=Math2Mode0RawOnlySource.PrepareColdWithSelectedMathField(runtime,bundle,run,field);
 if(!ReferenceEquals(source.ReadSelectedMathField(field).C23Identity,field.C23Identity)||ReferenceEquals(field.C23Identity,foreign.C23Identity))throw new InvalidDataException("Source packet lost actual pre-NN C23 identity.");
 if(!ReferenceEquals(source.ReadSourcePositions().Document,field.Document))throw new InvalidDataException("NN source recreated original Document.");
 Reject(()=>Math2Mode0RawOnlySource.PrepareColdWithSelectedMathField(runtime,bundle,run,field));
 Reject(()=>source.ReadSelectedMathField(foreign));
 var forged=new Math2Mode0OriginalMathFieldSource.Binding(field);Reject(()=>forged.Validate(field));
 var foreignDoc=Math2Mode0RawSourcePositions.PrepareFeatures(input,default).Positions.Document;
 Reject(()=>Math2Mode0RawSourcePositions.PrepareFeatures(foreignDoc,default,source.ReadSelectedMathField(field)));
 source.RunToInitialOrder();source.ApplyEf4();var rawHistory=source.Loop.CopyRawAttentionStatistics();var output=source.OrderFinal().FormatFullCold();Collect();var attached=output.AttachSourcePositions();
 if(!ReferenceEquals(attached.Document,field.Document))throw new InvalidDataException("Formatted source Document changed.");
 var before=ProjectOriginal(output.CopyRecords()[0],attached.CopyRanges(0));
 Reject(()=>attached.CopySelectedMathField(foreign));Reject(()=>attached.CopySelectedMathField(field,canceled.Token));
 using var receipt=attached.CopySelectedMathField(field);receipt.Validate(receipt.Copy,field);
 var after=Project(receipt.Copy.CopyRecord());if(!Same(before,after))throw new InvalidDataException("Scoped first-copy changed numeric rows.");
 Reject(()=>receipt.Validate(receipt.Copy,foreign));Reject(()=>receipt.Validate(receipt.Copy,field,canceled.Token));
 Reject(()=>attached.CopySelectedMathField(field));
 using var outer=Math2Mode0OuterSymbolVector.CreateSingle(receipt.Copy);
 outer.ValidateDocument(field.Document);
 var positions=source.ReadSourcePositions();
 var selection=field.CopySnapshot();selection.C23Ranges.AsSpan().Fill(default);selection.OriginalNodes.AsSpan().Fill(null!);
 if(field.CopySnapshot().C23Ranges[0]==default||field.CopySnapshot().OriginalNodes[0]==null)throw new InvalidDataException("Math source inspection aliases live storage.");
 actual.Add(new(reversed,input,field.CopySnapshot(),prepared.Features.Frames,prepared.Features.Values.Select(BitConverter.SingleToInt32Bits).ToArray(),
  positions.CopyPositions(),positions.CopySeparators(),positions.CopySourceOrder(),positions.PayloadReservation,
  rawHistory.RawFrameCount,rawHistory.RawFrameHashes,before,after,ProjectOuter(outer.CopySymbols()),receipt.Copy.CopyStatistics().PayloadReservation,outer.CopyStatistics().PayloadReservation));
 if(!source.Loop.CopyRawAttentionStatistics().RawFrameHashes.SequenceEqual(rawHistory.RawFrameHashes))throw new InvalidDataException("Math field/copy join reevaluated NN.");
 source.Dispose();receipt.Validate(receipt.Copy,field);if(!Same(after,Project(receipt.Copy.CopyRecord())))throw new InvalidDataException("Disposed NN invalidated detached field copy.");
 field.Dispose();Reject(()=>receipt.Validate(receipt.Copy,field));Reject(()=>source.ReadSelectedMathField(field));
 if(!Same(after,Project(receipt.Copy.CopyRecord())))throw new InvalidDataException("Scope close should revoke authorization, not immutable copy storage.");
}
var originalPreparation=Math2Mode0RawSourcePositions.PrepareFeatures(ink,default);var commonView=LegacyFreshRawInkCurrentView.Prepare(originalPreparation.Positions.Document);
Reject(()=>Math2Mode0OriginalMathFieldSource.Prepare(null!,"formula"));Reject(()=>Math2Mode0OriginalMathFieldSource.Prepare(commonView,"a/b"));
Reject(()=>Math2Mode0OriginalMathFieldSource.Prepare(commonView,"formula",canceled.Token));
Reject(()=>Math2Mode0OriginalMathFieldSource.PrepareDiagnostic(commonView,"formula",0,Math2Mode0PreparedMathInput.Ordinary,null));
using(var budget=Math2Mode0OriginalMathFieldSource.Prepare(commonView,"formula"))
{
 long exact=budget.AdditionalPayloadReservation;
 Reject(()=>Math2Mode0OriginalMathFieldSource.PrepareDiagnostic(commonView,"formula",exact-1,Math2Mode0PreparedMathInput.Ordinary,null));
 using var same=Math2Mode0OriginalMathFieldSource.PrepareDiagnostic(commonView,"formula",exact,Math2Mode0PreparedMathInput.Ordinary,null);
 if(same.AdditionalPayloadReservation!=exact)throw new InvalidDataException("Exact preflight budget mismatch.");
}
var preparedSelections=new List<PreparedSelection>();
foreach(var mode in Enum.GetValues<Math2Mode0PreparedMathInput>().Where(m=>m!=Math2Mode0PreparedMathInput.Ordinary))
{
 using var field=Math2Mode0OriginalMathFieldSource.PrepareDiagnostic(commonView,"formula",Math2Mode0OriginalMathFieldSource.DefaultPayloadBudget,mode,null);
 var snapshot=field.CopySnapshot();if(snapshot.CanInfer)throw new InvalidDataException("Prepared unsupported selection became an actual NN source.");
 Reject(()=>Math2Mode0RawOnlySource.PrepareColdWithSelectedMathField(runtime,bundle,run,field));
 if(field.CopySnapshot().InferenceClaimed)throw new InvalidDataException("Unsupported shape claimed inference.");
 preparedSelections.Add(new(mode.ToString(),snapshot));
}
var failures=new List<Failure>();
foreach(var stage in Enum.GetValues<Math2Mode0MathFieldStage>())foreach(bool cancel in new[]{false,true})
{
 Math2Mode0OriginalMathFieldSource? retained=null;Math2Mode0MathFieldSnapshot? prefix=null;using var stop=new CancellationTokenSource();
 try
 {
  Math2Mode0OriginalMathFieldSource.PrepareDiagnostic(commonView,"formula",Math2Mode0OriginalMathFieldSource.DefaultPayloadBudget,
   Math2Mode0PreparedMathInput.Ordinary,(current,target)=>{retained=target;if(current!=stage)return;prefix=target.CopySnapshot();if(cancel)stop.Cancel();else throw new IOException("Prepared Math source failure.");},stop.Token);
  throw new InvalidDataException("Prepared construction fault was not reached.");
 }
 catch(Exception e)when(e is IOException or OperationCanceledException){}
 if(retained is null||prefix is null||!retained.CopySnapshot().Failed)throw new InvalidDataException("Math field prefix lost poison.");
 Reject(()=>Math2Mode0RawOnlySource.PrepareColdWithSelectedMathField(runtime,bundle,run,retained));
 failures.Add(new(stage.ToString(),cancel,prefix,retained.CopySnapshot()));retained.Dispose();retained.Dispose();
}
foreach(var closeStage in new[]{Math2Mode0MathFieldStage.OriginalField,Math2Mode0MathFieldStage.Ready})
{
 Math2Mode0OriginalMathFieldSource? retained=null;
 Reject(()=>Math2Mode0OriginalMathFieldSource.PrepareDiagnostic(commonView,"formula",Math2Mode0OriginalMathFieldSource.DefaultPayloadBudget,
  Math2Mode0PreparedMathInput.Ordinary,(stage,target)=>{retained=target;if(stage==closeStage)target.Dispose();}));
 if(retained is null||!retained.CopySnapshot().Closed||!retained.CopySnapshot().Failed)throw new InvalidDataException("Scope close during callback must preserve and poison prefix.");
 Reject(()=>Math2Mode0RawOnlySource.PrepareColdWithSelectedMathField(runtime,bundle,run,retained));
}
//Disposed official-ORT run fails after source claim. This is a portable
//external-lifetime fault, not a vendor allocator-order simulation.
using(var field=Math2Mode0OriginalMathFieldSource.Prepare(commonView,"failure"))
using(var dead=runtime.CreateRun(default))
{
 dead.Dispose();Reject(()=>Math2Mode0RawOnlySource.PrepareColdWithSelectedMathField(runtime,bundle,dead,field));
 if(!field.CopySnapshot().Failed||!field.CopySnapshot().InferenceClaimed)throw new InvalidDataException("Failed inference preparation must poison claimed source.");
 Reject(()=>Math2Mode0RawOnlySource.PrepareColdWithSelectedMathField(runtime,bundle,run,field));
}
using(var field=Math2Mode0OriginalMathFieldSource.Prepare(commonView,"closed"))
{
 field.Dispose();Reject(()=>Math2Mode0RawOnlySource.PrepareColdWithSelectedMathField(runtime,bundle,run,field));
}
//A Math field created AFTER old raw-only inference on the SAME Document
//cannot retroactively authenticate that output. Refusal leaves generic copy usable.
using(var old=Math2Mode0RawOnlySource.PrepareColdWithSourcePositions(runtime,bundle,run,ink))
{
 old.RunToInitialOrder();old.ApplyEf4();var attached=old.OrderFinal().FormatFullCold().AttachSourcePositions();
 using var late=Math2Mode0OriginalMathFieldSource.Prepare(LegacyFreshRawInkCurrentView.Prepare(attached.Document),"formula");
 Reject(()=>attached.CopySelectedMathField(late));
 if(late.CopySnapshot().InferenceClaimed)throw new InvalidDataException("Late metadata was claimed as original NN provenance.");
 using var copy=Math2Mode0FirstResultCopy.Create(attached);copy.ValidateForOuterCopy(default);
}
var positive=Detached(runtime,run,bundle,commonView,false);var negative=Detached(runtime,run,bundle,commonView,true);Collect();
if(positive.Source.IsAlive||!negative.Source.IsAlive)throw new InvalidDataException("Math receipt retained NN/source or GC control failed.");
positive.Receipt.Validate(positive.Receipt.Copy,positive.Field);
Reject(()=>positive.Receipt.Validate(negative.Receipt.Copy,positive.Field));
Reject(()=>negative.Receipt.Validate(positive.Receipt.Copy,negative.Field));
var gcRow=Project(positive.Receipt.Copy.CopyRecord());
if(!Same(gcRow,positive.Before))throw new InvalidDataException("GC changed copied fields.");
positive.Field.Dispose();Reject(()=>positive.Receipt.Validate(positive.Receipt.Copy,positive.Field));
positive.Receipt.Dispose();positive.Receipt.Dispose();negative.Receipt.Dispose();negative.Field.Dispose();GC.KeepAlive(negative.Keep);
File.WriteAllText(args[3],JsonSerializer.Serialize(new Capture(Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(args[1]))),actual.ToArray(),preparedSelections.ToArray(),failures.ToArray(),
 new GcEvidence(!positive.Source.IsAlive,negative.Source.IsAlive,gcRow),guards),CaptureJson.Default.Capture));
Console.WriteLine($"PASS {actual.Count} actual original/selected Math INPUT102 -> same-document NN/EE4/copy/outer; {preparedSelections.Count} prepared selection branches/{failures.Count} prefixes/{guards} guards; scoped receipt GC.");

[MethodImpl(MethodImplOptions.NoInlining)]
static DetachedEvidence Detached(OrtRuntime runtime,OrtRun run,Math2Mode0ColdBundle bundle,LegacyFreshRawInkCurrentView view,bool keep)
{
 var field=Math2Mode0OriginalMathFieldSource.Prepare(view,"detached");
 using var source=Math2Mode0RawOnlySource.PrepareColdWithSelectedMathField(runtime,bundle,run,field);
 source.RunToInitialOrder();source.ApplyEf4();var attached=source.OrderFinal().FormatFullCold().AttachSourcePositions();
 var receipt=attached.CopySelectedMathField(field);return new(receipt,field,new(source),keep?attached:null,Project(receipt.Copy.CopyRecord()));
}
static void Collect(){GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();}
static Row ProjectOriginal(Math2Mode0OutputRecord row,LegacyNativeInkPositionRange[][] ranges)=>new(BitConverter.SingleToInt32Bits(row.Score),row.Source.Frontier.Value,
 row.Source.CompletedSourceParent,row.Source.CopyPrimary(),row.Source.CopySecondary(),row.Units,row.Runs,
 row.UnitRows.Select(u=>new Unit(u.Unit,u.Text.Identity,u.ByteCount,u.ScalarBits,u.Flags,u.Metadata,Convert.ToBase64String(u.Text.ReadBytes()))).ToArray(),ProjectRanges(ranges));
static Row Project(Math2Mode0FirstCopyRecord row)=>new(BitConverter.SingleToInt32Bits(row.Score),row.Frontier,row.Parent,row.Primary,row.Secondary,row.Units,row.Runs,
 row.UnitRows.Select(u=>new Unit(u.Unit,u.Text.Identity,u.ByteCount,u.ScalarBits,u.Flags,u.Metadata,Convert.ToBase64String(u.Text.ReadBytes()))).ToArray(),ProjectRanges(row.SourceRanges));
static Range[][] ProjectRanges(LegacyNativeInkPositionRange[][] ranges)=>ranges.Select(row=>row.Select(r=>new Range(r.First.Value,r.Last.Value)).ToArray()).ToArray();
static OuterRow[] ProjectOuter(Math2Mode0OuterSymbol[] rows)=>rows.Select(r=>new OuterRow(r.Flags,r.Metadata,r.Alternatives.Select(a=>new Alternative(a.Text.Identity,a.ByteCount,a.ScalarBits,Convert.ToBase64String(a.Text.ReadBytes()))).ToArray(),r.Ranges.Select(v=>new Range(v.First.Value,v.Last.Value)).ToArray())).ToArray();
static bool Same(Row a,Row b)=>JsonSerializer.Serialize(a,CaptureJson.Default.Row)==JsonSerializer.Serialize(b,CaptureJson.Default.Row);
record Unit(uint Word,int Identity,int Length,uint ScalarBits,byte Flags,uint Metadata,string Bytes);
record Range(ulong First,ulong Last);
record Row(int ScoreBits,long Frontier,int Parent,ushort[] Primary,ushort[] Secondary,uint[] Units,MathSegmentRun[] Runs,Unit[] Rows,Range[][] Ranges);
record Alternative(int Identity,int ByteCount,uint ScalarBits,string Bytes);
record OuterRow(byte Flags,uint Metadata,Alternative[] Alternatives,Range[] Ranges);
record Actual(bool Reversed,InkStroke[] Ink,Math2Mode0MathFieldSnapshot Field,int Frames,int[] FeatureBits,ulong[] Positions,int[] Separators,int[] Order,long PositionReservation,int RawSteps,string[] RawHashes,Row Before,Row After,OuterRow[] Outer,long CopyReservation,long OuterReservation);
record PreparedSelection(string Mode,Math2Mode0MathFieldSnapshot Snapshot);
record Failure(string Stage,bool Canceled,Math2Mode0MathFieldSnapshot Before,Math2Mode0MathFieldSnapshot After);
record GcEvidence(bool DetachedAllowsSourceCollection,bool SourceBackedRetainsSource,Row Row);
record Capture(string Resource,Actual[] Actual,PreparedSelection[] Prepared,Failure[] Failures,GcEvidence Gc,int Guards);
record DetachedEvidence(Math2Mode0SelectedMathFieldCopy Receipt,Math2Mode0OriginalMathFieldSource Field,WeakReference Source,object? Keep,Row Before);
[JsonSerializable(typeof(Capture))][JsonSerializable(typeof(Row))]
internal partial class CaptureJson:JsonSerializerContext;
