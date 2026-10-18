using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
using InkRecognition.Math;
if(args.Length!=4)throw new ArgumentException("pack res official-runtime capture.json");
using var runtime=new OrtRuntime(Path.GetFullPath(args[2]));using var run=runtime.CreateRun(default);
using var stream=File.OpenRead(args[0]);using var bundle=Math2Mode0ColdBundle.Read(runtime,stream,File.ReadAllBytes(args[1]));
int guards=0;
void Reject(Action action)
{try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Required first-copy guard accepted.");}
using var canceled=new CancellationTokenSource();canceled.Cancel();
var actual=new List<Actual>();
foreach(int width in new[]{1,3,16})
{
    var(source,output)=Prepare(runtime,run,bundle,width);
    var sourceEvidence=ReadSource(source,output);
    var original=output.Output.CopyRecords()[0];var before=ProjectOriginal(original,output.CopyRanges(0));
    Reject(()=>Math2Mode0FirstResultCopy.CreateDiagnostic(output,-1,null));
    Reject(()=>Math2Mode0FirstResultCopy.CreateDiagnostic(output,Math2Mode0FirstResultCopy.DefaultPayloadBudget+1,null));
    Reject(()=>Math2Mode0FirstResultCopy.CreateDiagnostic(output,0,null));
    Reject(()=>Math2Mode0FirstResultCopy.Create(output,canceled.Token));
    var fake=new Math2Mode0SourceBoundOutput(output.Output,source.ReadSourcePositions(),[original],0);
    Reject(()=>Math2Mode0FirstResultCopy.Create(fake));
    if(source.Loop.IsFailed)throw new InvalidDataException("Preflight failure consumed decoder.");
    using var copy=Math2Mode0FirstResultCopy.Create(output);
    if(!ReferenceEquals(copy.Document,output.Document))throw new InvalidDataException("Copied range document domain differs.");
    var row=copy.CopyRecord();var after=Project(row);
    if(!Same(before,after))throw new InvalidDataException("First copy changes data.");
    for(int i=0;i<row.UnitRows.Length;i++)
    {
        if(ReferenceEquals(row.UnitRows[i].Text,original.UnitRows[i].Text))throw new InvalidDataException("Copied text aliases source pool.");
        for(int j=0;j<i;j++)if(row.UnitRows[i].Text.ReadBytes().SequenceEqual(row.UnitRows[j].Text.ReadBytes())&&
            !ReferenceEquals(row.UnitRows[i].Text,row.UnitRows[j].Text))throw new InvalidDataException("Same copied interval has two identities.");
    }
    Reject(()=>Math2Mode0FirstResultCopy.Create(output));
    if(row.Primary.Length>0)row.Primary[0]^=123;
    row.Units.AsSpan().Fill(0xdeadbeef);row.Runs.AsSpan().Fill(new(1234,5678));
    foreach(var rangeRow in row.SourceRanges)rangeRow.AsSpan().Fill(new(new(0),new(0)));
    if(!Same(after,Project(copy.CopyRecord())))throw new InvalidDataException("Inspection exposed copied storage.");
    source.Dispose();Reject(()=>output.Output.CopyRecords());Reject(()=>output.CopyRanges(0));
    if(!Same(after,Project(copy.CopyRecord())))throw new InvalidDataException("Decoder disposal invalidated copy.");
    var stats=copy.CopyStatistics();var texts=copy.CopyRecord().UnitRows.Select(r=>r.Text).ToArray();
    var(exactSource,exactOutput)=Prepare(runtime,run,bundle,width);using(exactSource)
    {
        Reject(()=>Math2Mode0FirstResultCopy.CreateDiagnostic(exactOutput,stats.PayloadReservation-1,null));
        using var exact=Math2Mode0FirstResultCopy.CreateDiagnostic(exactOutput,stats.PayloadReservation,null);
        if(exact.CopyStatistics().PayloadReservation!=stats.PayloadReservation||!Same(after,Project(exact.CopyRecord())))
            throw new InvalidDataException("Exact combined budget changed first copy.");
    }
    copy.Dispose();copy.Dispose();Reject(()=>copy.CopyRecord());
    foreach(var text in texts)Reject(()=>text.ReadBytes());
    if(copy.CopyStatistics().ReleaseCount!=1)throw new InvalidDataException("Target pool release is not exact once.");
    actual.Add(new(width,sourceEvidence,before,after,stats));
}
var(aliasSource,aliasOutput)=Prepare(runtime,run,bundle,1,true);
int actualAliasPairs=0;
using(aliasSource)
using(var aliasCopy=Math2Mode0FirstResultCopy.Create(aliasOutput))
{
    var original=aliasOutput.Output.CopyRecords()[0];var copied=aliasCopy.CopyRecord();
    if(!Same(ProjectOriginal(original,aliasOutput.CopyRanges(0)),Project(copied)))throw new InvalidDataException("Repeated-ink actual copy differs.");
    for(int i=0;i<copied.UnitRows.Length;i++)
        if(ReferenceEquals(copied.UnitRows[i].Text,original.UnitRows[i].Text))throw new InvalidDataException("Repeated copy aliases source pool.");
    for(int i=0;i<copied.UnitRows.Length;i++)for(int j=0;j<i;j++)
        if(copied.UnitRows[i].Text.ReadBytes().SequenceEqual(copied.UnitRows[j].Text.ReadBytes()))
        {
            if(!ReferenceEquals(copied.UnitRows[i].Text,copied.UnitRows[j].Text))throw new InvalidDataException("Repeated actual interval lost identity.");
            actualAliasPairs++;
        }
    actual.Add(new(1,ReadSource(aliasSource,aliasOutput),ProjectOriginal(original,aliasOutput.CopyRanges(0)),Project(copied),aliasCopy.CopyStatistics()));
}
var failures=new List<Failure>();
foreach(var stage in Enum.GetValues<Math2Mode0FirstCopyStage>())
foreach(bool cancel in new[]{false,true})
{
    var(source,output)=Prepare(runtime,run,bundle,3);using(source)
    {
        Math2Mode0FirstResultCopy? retained=null;Math2Mode0FirstCopyStatistics? prefix=null;
        using var token=new CancellationTokenSource();
        try
        {
            Math2Mode0FirstResultCopy.CreateDiagnostic(output,Math2Mode0FirstResultCopy.DefaultPayloadBudget,(current,index,target)=>
            {
                retained=target;if(current!=stage||index>0)return;prefix=target.CopyStatistics();
                if(cancel)token.Cancel();else throw new IOException("Prepared first-copy failure.");
            },token.Token);
            throw new InvalidDataException("Required consuming copy failure did not occur.");
        }
        catch(Exception e)when(e is IOException or OperationCanceledException){}
        if(retained is null||prefix is null||!source.Loop.IsFailed||!retained.CopyStatistics().Failed)
            throw new InvalidDataException("Consuming failure lost poison/prefix.");
        Reject(()=>retained.CopyRecord());Reject(()=>Math2Mode0FirstResultCopy.Create(output));
        failures.Add(new(stage.ToString(),cancel,prefix));retained.Dispose();retained.Dispose();
        if(retained.CopyStatistics().ReleaseCount!=(stage==Math2Mode0FirstCopyStage.Claimed?0:1))
            throw new InvalidDataException("Failure target pool release is not exact once.");
    }
}
var positive=CreateDetached(runtime,run,bundle,false);var negative=CreateDetached(runtime,run,bundle,true);
Collect();
if(positive.Source.IsAlive||!negative.Source.IsAlive)throw new InvalidDataException("Detached/source-backed GC controls did not discriminate.");
if(!Same(positive.Before,Project(positive.Copy.CopyRecord())))throw new InvalidDataException("GC altered detached output.");
var gc=new GcEvidence(!positive.Source.IsAlive,negative.Source.IsAlive,positive.Before);
positive.Copy.Dispose();negative.Copy.Dispose();GC.KeepAlive(negative.Keep);
File.WriteAllText(args[3],JsonSerializer.Serialize(new Capture(actual.ToArray(),failures.ToArray(),gc,guards,actualAliasPairs),CaptureJson.Default.Capture));
Console.WriteLine($"PASS first result copy {actual.Count} actual sources/{actualAliasPairs} repeated interval pairs/{failures.Count} consuming prefixes/{guards} guards; detached GC and live-source negative control.");

static (Math2Mode0RawOnlySource Source,Math2Mode0SourceBoundOutput Output) Prepare(OrtRuntime runtime,OrtRun run,Math2Mode0ColdBundle bundle,int width,bool repeated=false)
{
    InkStroke[] ink=[new([new InkPoint(0,30),new(0,0),new(10,18),new(20,0),new(20,30)]),new([new InkPoint(35,0),new(35,30)])];
    if(repeated)ink=[new([new InkPoint(0,30),new(0,0),new(10,18),new(20,0),new(20,30)]),
        new([new InkPoint(35,30),new(35,0),new(45,18),new(55,0),new(55,30)])];
    if(width==3)ink=ink.Reverse().ToArray();
    var source=width==1?Math2Mode0RawOnlySource.PrepareColdWithSourcePositions(runtime,bundle,run,ink):Math2Mode0RawOnlySource.PrepareWithPreparedSearchAndSourcePositions(runtime,bundle,run,ink,width);
    try{source.RunToInitialOrder();source.ApplyEf4();return(source,source.OrderFinal().FormatFullCold().AttachSourcePositions());}
    catch{source.Dispose();throw;}
}
[MethodImpl(MethodImplOptions.NoInlining)]
static Detached CreateDetached(OrtRuntime runtime,OrtRun run,Math2Mode0ColdBundle bundle,bool keep)
{
    var(source,output)=Prepare(runtime,run,bundle,3);var copy=Math2Mode0FirstResultCopy.Create(output);
    var before=Project(copy.CopyRecord());source.Dispose();return new(copy,new(source),keep?output:null,before);
}
static SourceEvidence ReadSource(Math2Mode0RawOnlySource source,Math2Mode0SourceBoundOutput output)
{
    var positions=source.ReadSourcePositions();
    return new(output.Document.Strokes.Select(s=>new InkStroke(s.Points.ToArray())).ToArray(),
        positions.CopyPositions(),positions.CopySeparators(),positions.CopySourceOrder(),output.CopyStatistics().PayloadReservation);
}
static void Collect(){GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();}
static Row ProjectOriginal(Math2Mode0OutputRecord row,LegacyNativeInkPositionRange[][] ranges)=>new(BitConverter.SingleToInt32Bits(row.Score),row.Source.Frontier.Value,
    row.Source.CompletedSourceParent,row.Source.CopyPrimary(),row.Source.CopySecondary(),row.Units,row.Runs,
    row.UnitRows.Select(u=>new Unit(u.Unit,u.Text.Identity,u.ByteCount,u.ScalarBits,u.Flags,u.Metadata,Convert.ToBase64String(u.Text.ReadBytes()))).ToArray(),
    ranges.Select(values=>values.Select(r=>new Range(r.First.Value,r.Last.Value)).ToArray()).ToArray());
static Row Project(Math2Mode0FirstCopyRecord row)=>new(BitConverter.SingleToInt32Bits(row.Score),row.Frontier,row.Parent,
    row.Primary.ToArray(),row.Secondary.ToArray(),row.Units.ToArray(),row.Runs.ToArray(),row.UnitRows.Select(u=>new Unit(u.Unit,u.Text.Identity,u.ByteCount,u.ScalarBits,u.Flags,u.Metadata,Convert.ToBase64String(u.Text.ReadBytes()))).ToArray(),
    row.SourceRanges.Select(values=>values.Select(r=>new Range(r.First.Value,r.Last.Value)).ToArray()).ToArray());
static bool Same(Row a,Row b)=>JsonSerializer.Serialize(a,CaptureJson.Default.Row)==JsonSerializer.Serialize(b,CaptureJson.Default.Row);
record Unit(uint Word,int Identity,int Length,uint ScalarBits,byte Flags,uint Metadata,string Bytes);
record Range(ulong First,ulong Last);
record Row(int ScoreBits,long Frontier,int Parent,ushort[] Primary,ushort[] Secondary,uint[] Units,MathSegmentRun[] Runs,Unit[] Rows,Range[][] Ranges);
record SourceEvidence(InkStroke[] Ink,ulong[] Positions,int[] Separators,int[] Order,long AttachmentReservation);
record Actual(int Width,SourceEvidence Source,Row Before,Row After,Math2Mode0FirstCopyStatistics Statistics);
record Failure(string Stage,bool Canceled,Math2Mode0FirstCopyStatistics Prefix);
record GcEvidence(bool DetachedAllowsSourceCollection,bool SourceBackedRetainsSource,Row Row);
record Capture(Actual[] Actual,Failure[] Failures,GcEvidence Gc,int Guards,int ActualAliasPairs);
record Detached(Math2Mode0FirstResultCopy Copy,WeakReference Source,object? Keep,Row Before);
[JsonSerializable(typeof(Capture))][JsonSerializable(typeof(Row))]
internal partial class CaptureJson:JsonSerializerContext;
