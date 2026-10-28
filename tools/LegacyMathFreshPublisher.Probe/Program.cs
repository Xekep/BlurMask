using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
using InkRecognition.Math;

if(args.Length!=4)throw new ArgumentException("pack rawMath2.res official-runtime capture.json");
using var runtime=new OrtRuntime(Path.GetFullPath(args[2]));using var run=runtime.CreateRun(default);
using var stream=File.OpenRead(args[0]);using var bundle=Math2Mode0ColdBundle.Read(runtime,stream,File.ReadAllBytes(args[1]));
int guards=0;void Reject(Action action)
{try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Missing Math publication guard.");}
// Native0336C0/033870 use different sentinels. These are scalar reader
// controls; actual owning publication below still supplies all positive tags.
void RejectSentinel(Action action)
{try{action();}catch(NotSupportedException){return;}throw new InvalidDataException("Missing native numeric sentinel refusal.");}
RejectSentinel(()=>Math2DomValueConversion.RegistrationTag(long.MaxValue));
RejectSentinel(()=>Math2DomValueConversion.CollectedTag(-1));
foreach(long id in new[]{long.MinValue,-2L,0L,1L})
 if(Math2DomValueConversion.RegistrationTag(id)!=id||Math2DomValueConversion.CollectedTag(id)!=id)
  throw new InvalidDataException("Native typed numeric reader changed non-sentinel tag.");
if(Math2DomValueConversion.RegistrationTag(-1)!=-1||Math2DomValueConversion.CollectedTag(long.MaxValue)!=long.MaxValue)
 throw new InvalidDataException("CB8 numeric reader sentinels were conflated.");
using var canceled=new CancellationTokenSource();canceled.Cancel();
var actual=new List<Actual>();var events=new List<Stage>();
foreach(int width in new[]{1,3,16})foreach(bool reversed in new[]{false,true})
{
 using var prepared=Prepare(runtime,run,bundle,width,reversed);
 var before=prepared.Field.CopySnapshot();Reject(()=>Math2Mode0FreshMathPublisher.Create(prepared.Receipt,canceled.Token));
 Reject(()=>Math2Mode0FreshMathPublisher.CreateDiagnostic(prepared.Receipt,0,null));
 using var publication=Math2Mode0FreshMathPublisher.CreateDiagnostic(prepared.Receipt,Math2Mode0FreshMathPublisher.DefaultPayloadBudget,
  (stage,index,target)=>{if(width==1&&!reversed)events.Add(new(stage.ToString(),index));});
 var result=publication.CopyResult();
 if(!Same(before,prepared.Field.CopySnapshot()))throw new InvalidDataException("Candidate publication mutated original selected registry/field.");
 Reject(()=>Math2Mode0FreshMathPublisher.Create(prepared.Receipt));
 var detached=publication.CopyResult();detached.Registry[0].Ranges.AsSpan().Fill(default);detached.Candidate.AsSpan().Fill(null!);
 if(publication.CopyResult().Registry[0].Ranges[0]==default||publication.CopyResult().Candidate[0]==null)throw new InvalidDataException("Inspection aliases actual publication data.");
 prepared.Source.Dispose();if(!SamePublication(result,publication.CopyResult()))throw new InvalidDataException("Source disposal invalidated detached Math publisher.");
 actual.Add(new(width==1?"ColdDefault":"PreparedSearch",width,reversed,prepared.Evidence,result));
 prepared.Field.Dispose();Reject(()=>publication.CopyResult());
 publication.Dispose();publication.Dispose();if(publication.CopySnapshot().CleanupCount!=1)throw new InvalidDataException("Math resources released twice.");
 Reject(()=>publication.CopyResult());
}
var failures=new List<Failure>();
foreach(var step in events)foreach(bool cancel in new[]{false,true})
{
 using var p=Prepare(runtime,run,bundle,1,false);using var stop=new CancellationTokenSource();
 Math2Mode0FreshMathPublisher? retained=null;Math2Mode0PublishSnapshot? before=null;
 try
 {
  Math2Mode0FreshMathPublisher.CreateDiagnostic(p.Receipt,Math2Mode0FreshMathPublisher.DefaultPayloadBudget,(stage,index,target)=>
  {if(stage.ToString()!=step.Name||index!=step.Index)return;retained=target;before=target.CopySnapshot();if(cancel)stop.Cancel();else throw new IOException("Prepared logical stage failure.");},token:stop.Token);
  throw new InvalidDataException("Unreached publication prefix.");
 }
 catch(Exception e)when(e is IOException or OperationCanceledException){}
 if(retained is null||before is null||!retained.CopySnapshot().Failed)throw new InvalidDataException("Lost publication failure prefix.");
 failures.Add(new(step.Name,step.Index,cancel,before,retained.CopySnapshot()));
 Reject(()=>retained.CopyResult());Reject(()=>Math2Mode0FreshMathPublisher.Create(p.Receipt));retained.Dispose();retained.Dispose();
 if(retained.CopySnapshot().CleanupCount!=1)throw new InvalidDataException("Fault cleanup was not exactly once.");
}
using(var p=Prepare(runtime,run,bundle,1,false))
{
 using var normal=Math2Mode0FreshMathPublisher.Create(p.Receipt);long exact=normal.CopySnapshot().Reservation;
 using var q=Prepare(runtime,run,bundle,1,false);
 Reject(()=>Math2Mode0FreshMathPublisher.CreateDiagnostic(q.Receipt,exact-1,null));
 using var accepted=Math2Mode0FreshMathPublisher.CreateDiagnostic(q.Receipt,exact,null);
 if(accepted.CopyResult().Reservation!=exact)throw new InvalidDataException("Exact envelope mismatch.");
 using var foreign=PrepareWithView(runtime,run,bundle,1,false,p.View);
 if(!ReferenceEquals(p.Field.Document,foreign.Field.Document))throw new InvalidDataException("Foreign same-document control missing.");
 Reject(()=>p.Field.WithFreshPublicationInput(foreign.Receipt,_=>0,default));
 Reject(()=>Math2Mode0FreshMathPublisher.Create(null!));
 Reject(()=>Math2Mode0FreshMathPublisher.CreateDiagnostic(foreign.Receipt,-1,null));
 Reject(()=>Math2Mode0FreshMathPublisher.CreateDiagnostic(foreign.Receipt,Math2Mode0FreshMathPublisher.DefaultPayloadBudget+1,null));
 Reject(()=>Math2Mode0FreshMathPublisher.CreateDiagnostic(foreign.Receipt,Math2Mode0FreshMathPublisher.DefaultPayloadBudget,null,-2));
}
Math2Mode0PublishSnapshot? refused=null;
using(var p=Prepare(runtime,run,bundle,1,false))
{
 Math2Mode0FreshMathPublisher? target=null;
 Reject(()=>Math2Mode0FreshMathPublisher.CreateDiagnostic(p.Receipt,Math2Mode0FreshMathPublisher.DefaultPayloadBudget,(s,i,t)=>target=t,0));
 if(target is null)throw new InvalidDataException();refused=target.CopySnapshot();
 if(refused.Pending.Length!=1||refused.Pending[0].HasSelection||refused.TemporaryNext!=3||refused.RegistryNext!=1)throw new InvalidDataException("CB8 failed C22 must consume/link temporary ID without final registration.");
 Reject(()=>Math2Mode0FreshMathPublisher.Create(p.Receipt));target.Dispose();
}
foreach(bool closeField in new[]{false,true})
{
 using var p=Prepare(runtime,run,bundle,1,false);Math2Mode0FreshMathPublisher? target=null;
 Reject(()=>Math2Mode0FreshMathPublisher.CreateDiagnostic(p.Receipt,Math2Mode0FreshMathPublisher.DefaultPayloadBudget,(s,i,t)=>
 {target=t;if(s==Math2Mode0PublishStage.ActionQueued&&i==0){if(closeField)p.Field.Dispose();else p.Receipt.Dispose();}}));
 if(target is null||!target.CopySnapshot().Failed)throw new InvalidDataException("Closing borrowed source must poison active publication.");target.Dispose();
}
var positive=Detached(runtime,run,bundle,false);var negative=Detached(runtime,run,bundle,true);Collect();
if(positive.Source.IsAlive||!negative.Source.IsAlive)throw new InvalidDataException("Publisher retained disposed decoder or negative control failed.");
var gc=positive.Publication.CopyResult();positive.Publication.Dispose();
//Publisher borrows and must never dispose the receipt's first-copy pool.
positive.Prepared.Receipt.Validate(positive.Prepared.Receipt.Copy,positive.Prepared.Field);
positive.Prepared.Dispose();negative.Publication.Dispose();negative.Prepared.Dispose();GC.KeepAlive(negative.Keep);
int successes=0,refusals=0;
using(var p=Prepare(runtime,run,bundle,1,false))
{
 Parallel.For(0,2,_=>
 {
  try{using var publication=Math2Mode0FreshMathPublisher.Create(p.Receipt);publication.CopyResult();Interlocked.Increment(ref successes);}
  catch(InvalidOperationException){Interlocked.Increment(ref refusals);}
 });
 if(successes!=1||refusals!=1)throw new InvalidDataException("Concurrent factories must share actual receipt one-use claim.");
 p.Receipt.Validate(p.Receipt.Copy,p.Field);
}
File.WriteAllText(args[3],JsonSerializer.Serialize(new Capture(Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(args[1]))),actual.ToArray(),events.ToArray(),failures.ToArray(),refused!,new(true,true,gc),new(successes,refusals,true),guards),CaptureJson.Default.Capture));
Console.WriteLine($"PASS{actual.Count} actual NN Math publishers(cold1/preparedsearch3/16);{failures.Count} whole-stage prefixes/{guards} guards; actual3op2/thirdregistry/late tags/borrowed-copy GC.");

static InkStroke[] Ink(bool reverse)
{InkStroke[] a=[new([new InkPoint(0,30),new(0,0),new(10,18),new(20,0),new(20,30)]),new([new InkPoint(35,0),new(35,30)])];return reverse?a.Reverse().ToArray():a;}
static Prepared Prepare(OrtRuntime runtime,OrtRun run,Math2Mode0ColdBundle bundle,int width,bool reverse)
{
 var features=Math2Mode0RawSourcePositions.PrepareFeatures(Ink(reverse),default);
 return PrepareWithView(runtime,run,bundle,width,reverse,LegacyFreshRawInkCurrentView.Prepare(features.Positions.Document));
}
static Prepared PrepareWithView(OrtRuntime runtime,OrtRun run,Math2Mode0ColdBundle bundle,int width,bool reverse,LegacyFreshRawInkCurrentView view)
{
 var field=Math2Mode0OriginalMathFieldSource.Prepare(view,"formula");
 var features=Math2Mode0RawSourcePositions.PrepareFeatures(field.Document,default);
 var source=width==1?Math2Mode0RawOnlySource.PrepareColdWithSelectedMathField(runtime,bundle,run,field):
  Math2Mode0RawOnlySource.PrepareWithPreparedSearchAndSelectedMathField(runtime,bundle,run,field,width);
 source.RunToInitialOrder();source.ApplyEf4();var output=source.OrderFinal().FormatFullCold().AttachSourcePositions();var raw=source.Loop.CopyRawAttentionStatistics();
 var receipt=output.CopySelectedMathField(field);var positions=source.ReadSourcePositions();
 var rows=receipt.Copy.CopyRecord();var evidence=new Evidence(Ink(reverse),field.CopySnapshot(),features.Features.Frames,features.Features.Values.Select(BitConverter.SingleToInt32Bits).ToArray(),
  positions.CopyPositions(),positions.CopySeparators(),positions.CopySourceOrder(),positions.PayloadReservation,raw.RawFrameHashes,
  rows.Primary,rows.Secondary,rows.Units,rows.Runs,rows.SourceRanges.Select(a=>a.Select(r=>new Range(r.First.Value,r.Last.Value)).ToArray()).ToArray(),
  rows.UnitRows.Select(r=>new Unit(r.Unit,r.Text.Identity,r.ByteCount,Convert.ToBase64String(r.Text.ReadBytes()))).ToArray(),output.CopyStatistics().PayloadReservation,receipt.Copy.CopyStatistics().PayloadReservation);
 return new(source,field,receipt,view,evidence,output);
}
[MethodImpl(MethodImplOptions.NoInlining)]
static DetachedEvidence Detached(OrtRuntime runtime,OrtRun run,Math2Mode0ColdBundle bundle,bool keep)
{
 var p=Prepare(runtime,run,bundle,1,false);var publication=Math2Mode0FreshMathPublisher.Create(p.Receipt);var weak=new WeakReference(p.Source);
 object? retained=keep?p.Output:null;p.Output=null;p.Source.Dispose();p.Source=null!;return new(p,publication,weak,retained);
}
static void Collect(){GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();}
static bool Same(Math2Mode0MathFieldSnapshot a,Math2Mode0MathFieldSnapshot b)=>JsonSerializer.Serialize(a,CaptureJson.Default.Math2Mode0MathFieldSnapshot)==JsonSerializer.Serialize(b,CaptureJson.Default.Math2Mode0MathFieldSnapshot);
static bool SamePublication(Math2Mode0PublishSnapshot a,Math2Mode0PublishSnapshot b)=>JsonSerializer.Serialize(a,CaptureJson.Default.Math2Mode0PublishSnapshot)==JsonSerializer.Serialize(b,CaptureJson.Default.Math2Mode0PublishSnapshot);
sealed class Prepared(Math2Mode0RawOnlySource source,Math2Mode0OriginalMathFieldSource field,Math2Mode0SelectedMathFieldCopy receipt,LegacyFreshRawInkCurrentView view,Evidence evidence,Math2Mode0SourceBoundOutput output):IDisposable
{
 internal Math2Mode0RawOnlySource Source=source;internal readonly Math2Mode0OriginalMathFieldSource Field=field;internal readonly Math2Mode0SelectedMathFieldCopy Receipt=receipt;
 internal readonly LegacyFreshRawInkCurrentView View=view;internal readonly Evidence Evidence=evidence;internal Math2Mode0SourceBoundOutput? Output=output;
 public void Dispose(){Source?.Dispose();Receipt.Dispose();Field.Dispose();}
}
record Range(ulong First,ulong Last);
record Unit(uint Word,int Identity,int ByteCount,string Bytes);
record Evidence(InkStroke[] Ink,Math2Mode0MathFieldSnapshot Field,int Frames,int[] FeatureBits,ulong[] Positions,int[] Separators,int[] Order,long PositionReservation,string[] RawHashes,ushort[] Primary,ushort[] Secondary,uint[] Units,MathSegmentRun[] Runs,Range[][] Ranges,Unit[] Rows,long AttachmentReservation,long CopyReservation);
record Actual(string Policy,int Width,bool Reversed,Evidence Evidence,Math2Mode0PublishSnapshot Publication);
record Stage(string Name,int Index);
record Failure(string Stage,int Index,bool Canceled,Math2Mode0PublishSnapshot Before,Math2Mode0PublishSnapshot After);
record GcEvidence(bool DetachedAllowsSourceCollection,bool SourceBackedRetainsSource,Math2Mode0PublishSnapshot Publication);
record ConcurrentEvidence(int Completed,int Refused,bool BorrowedCopyLive);
record Capture(string Resource,Actual[] Actual,Stage[] Events,Failure[] Failures,Math2Mode0PublishSnapshot RefusedSelection,GcEvidence Gc,ConcurrentEvidence Concurrent,int Guards);
record DetachedEvidence(Prepared Prepared,Math2Mode0FreshMathPublisher Publication,WeakReference Source,object? Keep);
[JsonSerializable(typeof(Capture))][JsonSerializable(typeof(Math2Mode0MathFieldSnapshot))][JsonSerializable(typeof(Math2Mode0PublishSnapshot))]
internal partial class CaptureJson:JsonSerializerContext;
