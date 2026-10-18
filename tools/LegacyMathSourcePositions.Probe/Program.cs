using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
using InkRecognition.Math;

if(args.Length!=5)throw new ArgumentException("model.ikrmath rawMath2.res official-runtime prepared-position-controls.json capture.json");
int guards=0;
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or InvalidDataException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Required source-position guard missing.");}
var controls=JsonDocument.Parse(File.ReadAllBytes(args[3]));
var scalar=new List<ulong[]>();
foreach(var row in controls.RootElement.GetProperty("scalar").EnumerateArray())
{
 var values=row.EnumerateArray().Select(x=>x.GetUInt64()).ToArray();
 ulong result=Math2Mode0SourcePositionCoordinates.Compose(values[0],values[1],checked((byte)values[2]));
 if(result!=values[3])throw new InvalidDataException("Independent V18 scalar mismatch.");
 scalar.Add([values[0],values[1],values[2],result]);
}
InkStroke[] baseInk=[new([new InkPoint(0,30),new(0,0),new(10,18),new(20,0),new(20,30)]),new([new InkPoint(35,0),new(35,30)])];
Feature ProjectFeatures(InkStroke[] ink,MathFeatures feature,Math2Mode0RawSourcePositions positions)
{
 var queries=new List<RangeQuery>();
 for(int n=0;n<64;n++)
 {
  int a=n==0?0:n==1?feature.Frames:(n*137+17)%(feature.Frames+1);
  int b=n==0?feature.Frames:n==1?a:(n*53+31)%(feature.Frames+1);
  if(a>b)(a,b)=(b,a);
  queries.Add(new(a,b,positions.ReadRange(a,b,default).Select(p=>new[]{p.First.Value,p.Last.Value}).ToArray()));
 }
 return new(ink,BitConverter.SingleToUInt32Bits(feature.Scale),feature.Frames,feature.StrokeEnds,
  feature.Values.Select(BitConverter.SingleToUInt32Bits).ToArray(),positions.CopyPositions(),positions.CopySeparators(),
  positions.CopySourceOrder(),positions.PayloadReservation,queries.ToArray());
}
var featureCases=new List<Feature>();
var inputs=new List<InkStroke[]>{baseInk,baseInk.Reverse().ToArray(),new InkStroke[]{new([new InkPoint(1,2)])},
 new InkStroke[]{new([new InkPoint(0,0),new(.05f,.07f),new(3,2),new(3.1f,2.2f),new(9,0)])}};
foreach(int count in new[]{19,20,21,22,41,42,43,256})inputs.Add(Enumerable.Range(0,count).Select(i=>new InkStroke([new InkPoint(0,i*3)])).ToArray());
for(int n=0;n<8;n++)inputs.Add(Enumerable.Range(0,1+n%4).Select(i=>new InkStroke(Enumerable.Range(0,3+n).Select(p=>new InkPoint((i%2==0?50:0)+p*(n+1)%13,p%2==0?0:12+n)).ToArray())).ToArray());
foreach(var ink in inputs)
{
 var before=MathPreprocessor.Process(ink);var prepared=Math2Mode0RawSourcePositions.PrepareFeatures(ink,default);
 if(before.Frames!=prepared.Features.Frames||!before.StrokeEnds.SequenceEqual(prepared.Features.StrokeEnds)||
 !before.Values.Select(BitConverter.SingleToUInt32Bits).SequenceEqual(prepared.Features.Values.Select(BitConverter.SingleToUInt32Bits)))throw new InvalidDataException("Opt-in source mapping changed actual feature bytes.");
 prepared.Positions.ValidateFeatures(prepared.Features);Reject(()=>prepared.Positions.ValidateFeatures(before));
 featureCases.Add(ProjectFeatures(ink,prepared.Features,prepared.Positions));
 var copy=prepared.Positions.CopyPositions();copy[0]^=1;
 if(copy.SequenceEqual(prepared.Positions.CopyPositions()))throw new InvalidDataException("Position inspection aliases owner.");
}
// Explicit prepared producer failures over a genuine original document. They
// are not decoder hypotheses or actual NN output rows.
{
 var prepared=Math2Mode0RawSourcePositions.PrepareFeatures(baseInk,default);
 var document=prepared.Positions.Document;
 Reject(()=>new Math2Mode0RawSourcePositions.Builder(document,[0,0],default));
 Reject(()=>new Math2Mode0RawSourcePositions.Builder(document,[0,2],default));
 var builder=new Math2Mode0RawSourcePositions.Builder(document,[0,1],default);
 Reject(()=>builder.AppendCoordinate(1,0,0));
 Reject(()=>builder.AppendCoordinate(0,baseInk[0].Points.Count,0));
 Reject(()=>builder.AppendCoordinate(0,baseInk[0].Points.Count-1,1));
 Reject(()=>builder.CompleteStroke(0));
 Reject(()=>builder.Finish(prepared.Features,default));
 var single=Math2Mode0RawSourcePositions.PrepareFeatures([baseInk[0]],default);
 var duplicate=new Math2Mode0RawSourcePositions.Builder(single.Positions.Document,[0],default);
 duplicate.AppendCoordinate(0,0,0);duplicate.AppendCoordinate(0,0,0);duplicate.AppendCoordinate(0,1,0);duplicate.CompleteStroke(0);
 Reject(()=>duplicate.Finish(new MathFeatures(new float[78],3,[3],1),default));
}
using var canceled=new CancellationTokenSource();canceled.Cancel();
Reject(()=>Math2Mode0RawSourcePositions.PrepareFeatures(baseInk,canceled.Token));
Reject(()=>Math2Mode0RawSourcePositions.PrepareFeatures([],default));
Reject(()=>Math2Mode0RawSourcePositions.PrepareFeatures(new InkStroke[]{new([])},default));
Reject(()=>Math2Mode0RawSourcePositions.PrepareFeatures([new([new InkPoint(float.NaN,0)])],default));
Reject(()=>Math2Mode0RawSourcePositions.PrepareFeatures([new(Enumerable.Repeat(new InkPoint(0,0),4097).ToArray())],default));
using var runtime=new OrtRuntime(Path.GetFullPath(args[2]));using var run=runtime.CreateRun(default);
byte[] resource=File.ReadAllBytes(args[1]);using var stream=File.OpenRead(args[0]);using var bundle=Math2Mode0ColdBundle.Read(runtime,stream,resource);
Math2Mode0RawOnlySource Prepare(int width,bool sourceBound=true,bool reversed=false)
{
 var ink=reversed?baseInk.Reverse().ToArray():baseInk;
 var source=sourceBound?(width==1?Math2Mode0RawOnlySource.PrepareColdWithSourcePositions(runtime,bundle,run,ink):Math2Mode0RawOnlySource.PrepareWithPreparedSearchAndSourcePositions(runtime,bundle,run,ink,width)):
 Math2Mode0RawOnlySource.PrepareColdDefault(runtime,bundle,run,ink);
 try{source.RunToInitialOrder();source.ApplyEf4();return source;}catch{source.Dispose();throw;}
}
Row ProjectRow(Math2Mode0OutputRecord row,LegacyNativeInkPositionRange[][] attached)=>new(row.Units,row.OldToNewMap,row.Runs,
 row.UnitRows.Select(u=>new Unit(u.Unit,u.ByteCount,u.ScalarBits,u.Flags,u.Metadata,Convert.ToBase64String(u.Text.ReadBytes()))).ToArray(),
 BitConverter.SingleToUInt32Bits(row.Score),attached.Select(r=>r.Select(p=>new[]{p.First.Value,p.Last.Value}).ToArray()).ToArray());
var cases=new List<Actual>();long exactBudget=0;
foreach(int width in new[]{1,3,16})
{
 using var source=Prepare(width,reversed:width==3);var output=source.OrderFinal().FormatFullCold();
 var positions=source.ReadSourcePositions();GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();var history=source.Loop.CopyRawAttentionStatistics();var original=output.CopyRecords();
 Reject(()=>output.AttachSourcePositions(canceled.Token));Reject(()=>output.AttachSourcePositionsDiagnostic(0,null));
 Reject(()=>output.AttachSourcePositionsDiagnostic(-1,null));Reject(()=>output.AttachSourcePositionsDiagnostic(Math2Mode0SourceBoundOutput.DefaultPayloadBudget+1,null));
 if(source.Loop.IsFailed)throw new InvalidDataException("Preflight consumed source.");
 var stages=new List<string>();var attached=output.AttachSourcePositionsDiagnostic(Math2Mode0SourceBoundOutput.DefaultPayloadBudget,(stage,index,_)=>stages.Add($"{stage}:{index}"));
 attached.ValidateForCopy(output);if(width==1)exactBudget=attached.CopyStatistics().PayloadReservation;
 var rows=output.CopyRecords();
 for(int at=0;at<rows.Length;at++)if(!ReferenceEquals(rows[at].RecordIdentity,attached.ReadRecordIdentity(at))||!ReferenceEquals(rows[at].Source,original[at].Source)||!ReferenceEquals(rows[at].Library,original[at].Library))throw new InvalidDataException("Actual source/record/library identity changed.");
 var after=source.Loop.CopyRawAttentionStatistics();if(!after.RawFrameHashes.SequenceEqual(history.RawFrameHashes))throw new InvalidDataException("New NN or raw attention mutation.");
 Reject(()=>output.AttachSourcePositions());
 var rangeCopy=attached.CopyRanges(0);if(rangeCopy.Any(r=>r.Length>0)){var row=Array.FindIndex(rangeCopy,r=>r.Length>0);rangeCopy[row][0]=default;if(attached.CopyRanges(0)[row][0]==default)throw new InvalidDataException("Range copy aliases owner.");}
 GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();
 var captured=rows.Select((row,at)=>ProjectRow(row,attached.CopyRanges(at))).ToArray();
 cases.Add(new(width,width==3,history.RawFrameCount,history.RawFrameHashes,positions.CopyPositions(),positions.CopySeparators(),positions.CopySourceOrder(),positions.PayloadReservation,
 output.CopyStatistics().PayloadReservation,attached.CopyStatistics(),captured,stages.ToArray()));
 using var foreign=Prepare(1);var other=foreign.OrderFinal().FormatFullCold();Reject(()=>attached.ValidateForCopy(other));
 source.Dispose();Reject(()=>attached.CopyRanges(0));Reject(()=>attached.ValidateForCopy(output));
 if(original.Any(r=>r.Library.ReleaseCount!=1))throw new InvalidDataException("Library not released once.");
}
using(var old=Prepare(1,false)){var output=old.OrderFinal().FormatFullCold();Reject(()=>output.AttachSourcePositions());if(old.Loop.IsFailed||output.CopyRecords().Length!=1)throw new InvalidDataException("Old opaque-source output must remain reusable.");}
using(var budget=Prepare(1)){var output=budget.OrderFinal().FormatFullCold();Reject(()=>output.AttachSourcePositionsDiagnostic(exactBudget-1,null));var attached=output.AttachSourcePositionsDiagnostic(exactBudget,null);if(attached.CopyStatistics().PayloadReservation!=exactBudget)throw new InvalidDataException("Exact source envelope mismatch.");}
var failures=new List<Failure>();
foreach(var target in Enum.GetValues<Math2Mode0SourceAttachmentStage>())foreach(bool cancel in new[]{false,true})
{
 using var source=Prepare(3);var output=source.OrderFinal().FormatFullCold();Math2Mode0SourceBoundOutput? seen=null;using var stop=new CancellationTokenSource();bool reached=false;
 Reject(()=>output.AttachSourcePositionsDiagnostic(Math2Mode0SourceBoundOutput.DefaultPayloadBudget,(stage,index,current)=>{seen=current;if(stage!=target||target==Math2Mode0SourceAttachmentStage.AfterRun&&index!=1)return;reached=true;if(cancel)stop.Cancel();else throw new InvalidDataException("Prepared attachment fault.");},stop.Token));
 if(!reached||seen==null||!seen.CopyStatistics().Failed||!source.Loop.IsFailed)throw new InvalidDataException("Source failure did not poison owner.");
 Reject(()=>seen.CopyRanges(0));Reject(()=>output.CopyRecords());Reject(()=>output.AttachSourcePositions());
 failures.Add(new(target.ToString(),cancel,seen.CopyStatistics()));
}
var capture=new Capture(Convert.ToHexStringLower(SHA256.HashData(resource)),scalar.ToArray(),featureCases.ToArray(),cases.ToArray(),failures.ToArray(),guards);
File.WriteAllText(args[4],JsonSerializer.Serialize(capture,CaptureJson.Default.Capture));
Console.WriteLine($"PASS{cases.Sum(c=>c.Rows.Length)} actual source-bound records/{featureCases.Count} feature inputs/{scalar.Count} prepared coordinates;{failures.Count} prefix faults/{guards} guards; no DOM/formula acceptance");
record RangeQuery(int First,int Last,ulong[][] Ranges);
record Feature(InkStroke[] Ink,uint ScaleBits,int Frames,long[] StrokeEnds,uint[] Values,ulong[] Positions,int[] Separators,int[] Order,long PayloadReservation,RangeQuery[] Queries);
record Unit(uint RawUnit,int ByteCount,uint ScalarBits,byte Flags,uint Metadata,string Utf8);
record Row(uint[] Units,ushort[] Map,MathSegmentRun[] Runs,Unit[] UnitRows,uint ScoreBits,ulong[][][] Ranges);
record Actual(int Width,bool Reversed,int RawSteps,string[] RawHashes,ulong[] Positions,int[] Separators,int[] Order,long SourceReservation,long OutputReservation,Math2Mode0SourceAttachmentStatistics Statistics,Row[] Rows,string[] Stages);
record Failure(string Stage,bool Cancel,Math2Mode0SourceAttachmentStatistics Statistics);
record Capture(string Resource,ulong[][] Scalar,Feature[] Features,Actual[] Cases,Failure[] Failures,int Guards);
[JsonSerializable(typeof(Capture))]partial class CaptureJson:JsonSerializerContext{}
