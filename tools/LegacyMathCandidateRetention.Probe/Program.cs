using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
using InkRecognition.Math;
if(args.Length!=4)throw new ArgumentException("pack res runtime capture.json");
using var runtime=new OrtRuntime(Path.GetFullPath(args[2]));using var run=runtime.CreateRun(default);
using var stream=File.OpenRead(args[0]);using var bundle=Math2Mode0ColdBundle.Read(runtime,stream,File.ReadAllBytes(args[1]));
var contexts=new List<RetentionContext>();
foreach(int width in new[]{1,3,16})contexts.Add(Create(runtime,run,bundle,width));
GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();
var rows=new List<Row>();
foreach(var context in contexts)
{
    string after=Digest(context.Source);
    if(after!=context.Before)throw new InvalidDataException("GC altered actual completed records.");
    int alive=DistinctAlive(context.Weak);
    if(alive>1)throw new InvalidDataException("Past completed steps retain candidate score owners.");
    string processed=Process(context.Source);
    using var reference=Prepare(runtime,run,bundle,context.Width);
    reference.RunToInitialOrder();
    if(Process(reference)!=processed)throw new InvalidDataException("GC changed EF4/final-sort payloads.");
    rows.Add(new(context.Width,context.Weak.Length,alive,context.Before,processed));
    GC.KeepAlive(context.Source);context.Source.Dispose();
}
var features=MathPreprocessor.Process([new([new InkPoint(0,30),new(0,0),new(10,18),new(20,0),new(20,30)]),new([new InkPoint(35,0),new(35,30)])]);
using var input=runtime.Tensor(features.Values,features.Frames,1,26);
using var downsampling=runtime.Tensor(Enumerable.Repeat(1L,features.Frames).ToArray(),features.Frames,1);
using var ends=runtime.Tensor(features.StrokeEnds,1,features.StrokeEnds.Length);
int activeCases=0;
foreach(int mode in new[]{0,1,2,3})
{
    var active=CreateActive(bundle,run,input,downsampling,ends);
    Collect();
    if(!active.Candidate.IsAlive)throw new InvalidDataException("Pending EOS capture lost its candidate owner.");
    if(mode==2)active.Owner.Dispose();
    else if(mode==3)FailRecording(active);
    else
    {
        AppendAndCancel(active);Collect();
        if(!active.Candidate.IsAlive)throw new InvalidDataException("Pre-entry cancellation released an active candidate owner.");
        Apply(active,mode==1);
    }
    Collect();
    if(active.Candidate.IsAlive)throw new InvalidDataException("Consumed, failed or disposed pending step retains candidate owner.");
    active.Owner.Dispose();activeCases++;
}
File.WriteAllText(args[3],JsonSerializer.Serialize(new Capture(rows.ToArray(),activeCases),CaptureJson.Default.Capture));
Console.WriteLine($"Actual candidate retention: {rows.Sum(r=>r.Records)} records/{rows.Sum(r=>r.DistinctCandidatesAlive)} distinct candidate owners alive after GC.");
[MethodImpl(MethodImplOptions.NoInlining)]
static RetentionContext Create(OrtRuntime runtime,OrtRun run,Math2Mode0ColdBundle bundle,int width)
{
    var source=Prepare(runtime,run,bundle,width);
    source.RunToInitialOrder();var initial=source.ReadInitialForDiagnostic();int count=initial.CopyRecords().Length;
    WeakReference[] weak=Enumerable.Range(0,count).Select(i=>initial.ReadOwnedSource(i).CandidateReferenceForRetentionProbe()).ToArray();
    return new(width,source,weak,Digest(source));
}
static Math2Mode0RawOnlySource Prepare(OrtRuntime runtime,OrtRun run,Math2Mode0ColdBundle bundle,int width)
{
    InkStroke[] ink=[new([new InkPoint(0,30),new(0,0),new(10,18),new(20,0),new(20,30)]),new([new InkPoint(35,0),new(35,30)])];
    return width==1?Math2Mode0RawOnlySource.PrepareColdDefault(runtime,bundle,run,ink):Math2Mode0RawOnlySource.PrepareWithPreparedSearch(runtime,bundle,run,ink,width);
}
static string Process(Math2Mode0RawOnlySource source)
{
    source.ApplyEf4();var final=source.OrderFinal();
    using var output=new MemoryStream();using var writer=new BinaryWriter(output);
    foreach(var row in final.CopyRecords())
    {
        writer.Write(BitConverter.SingleToInt32Bits(row.After));writer.Write(row.Segments);
        writer.Write(row.Primary.Length);foreach(var x in row.Primary)writer.Write(x);
        writer.Write(row.Units.Length);foreach(var x in row.Units)writer.Write(x);
        writer.Write(row.Assignments.Length);foreach(var x in row.Assignments)writer.Write(x);
        writer.Write(row.Runs.Length);foreach(var x in row.Runs){writer.Write(x.PrimaryIndex);writer.Write(x.EndSegmentExclusive);}
        writer.Write(row.InitialMask.Length);writer.Write(row.InitialMask);
        writer.Write(row.DeletionMask.Length);writer.Write(row.DeletionMask);
    }
    writer.Flush();return Convert.ToHexString(SHA256.HashData(output.ToArray()));
}
[MethodImpl(MethodImplOptions.NoInlining)]
static int DistinctAlive(WeakReference[] weak)
{var alive=new HashSet<object>(ReferenceEqualityComparer.Instance);foreach(var reference in weak)if(reference.Target is{} target)alive.Add(target);return alive.Count;}
static void Collect(){GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();}
[MethodImpl(MethodImplOptions.NoInlining)]
static Active CreateActive(Math2Mode0ColdBundle bundle,OrtRun run,OrtTensor input,OrtTensor downsampling,OrtTensor ends)
{
    var owner=Math2Mode0PrefixContext.Prepare(bundle,run,input,downsampling,ends,3);
    var recorder=owner.CreateCompletedRecords();
    for(int iteration=0;iteration<10;iteration++)
    {
        var step=owner.RunStep();var candidates=step.CreateCandidates(1f,0f);
        int count=candidates.CopyCompletedScoreFacts().Length;
        if(count>0)
        {
            var leases=Enumerable.Range(0,count).Select(i=>recorder.Capture(candidates,i)).ToArray();
            return new(owner,recorder,step,leases,new(candidates));
        }
        owner.ApplySelected(candidates.Select());
    }
    owner.Dispose();throw new InvalidDataException("Actual EOS capture fixture did not occur.");
}
[MethodImpl(MethodImplOptions.NoInlining)]
static void AppendAndCancel(Active active)
{
    var candidate=(Math2Mode0PrefixCandidates?)active.Candidate.Target??throw new InvalidDataException("Missing active owner.");
    foreach(var lease in active.Leases)active.Recorder.Append(lease);
    using var canceled=new CancellationTokenSource();canceled.Cancel();
    try{active.Owner.ApplySelected(candidate.Select(),canceled.Token);}
    catch(OperationCanceledException){return;}
    throw new InvalidDataException("Pre-entry cancellation accepted.");
}
[MethodImpl(MethodImplOptions.NoInlining)]
static void Apply(Active active,bool fail)
{
    var candidate=(Math2Mode0PrefixCandidates?)active.Candidate.Target??throw new InvalidDataException("Missing active owner.");
    if(!fail){active.Owner.ApplySelected(candidate.Select());return;}
    try{active.Owner.ApplySelectedDiagnostic(candidate.Select(),()=>throw new IOException("Prepared consuming fault."));}
    catch(IOException){if(active.Owner.IsFailed)return;}
    throw new InvalidDataException("Consuming failure did not poison context.");
}
[MethodImpl(MethodImplOptions.NoInlining)]
static void FailRecording(Active active)
{
    try{active.Recorder.AppendDiagnostic(active.Leases[0],(_,_)=>throw new IOException("Prepared recording fault."));}
    catch(IOException){if(active.Owner.IsFailed)return;}
    throw new InvalidDataException("Recording failure did not poison context.");
}
static string Digest(Math2Mode0RawOnlySource source)
{
    var ordered=source.ReadInitialForDiagnostic();using var output=new MemoryStream();using var writer=new BinaryWriter(output);
    var records=ordered.CopyRecords();
    for(int i=0;i<records.Length;i++)
    {
        var lease=ordered.ReadOwnedSource(i);writer.Write(lease.CopyRawRecord());
        ushort[] primary=lease.CopyPrimary(),secondary=lease.CopySecondary();
        writer.Write(primary.Length);foreach(ushort x in primary)writer.Write(x);
        writer.Write(secondary.Length);foreach(ushort x in secondary)writer.Write(x);
        foreach(int[] row in lease.CopyAncestry()){writer.Write(row.Length);foreach(int x in row)writer.Write(x);}
    }
    writer.Flush();return Convert.ToHexString(SHA256.HashData(output.ToArray()));
}
record RetentionContext(int Width,Math2Mode0RawOnlySource Source,WeakReference[] Weak,string Before);
record Row(int Width,int Records,int DistinctCandidatesAlive,string ContentSha256,string PostprocessingSha256);
record Active(Math2Mode0PrefixContext Owner,Math2Mode0CompletedRecords Recorder,Math2Mode0PrefixStep Step,Math2Mode0CompletedEventLease[] Leases,WeakReference Candidate);
record Capture(Row[] Rows,int ActiveLifetimeCases);
[JsonSerializable(typeof(Capture))]
internal partial class CaptureJson:JsonSerializerContext;
