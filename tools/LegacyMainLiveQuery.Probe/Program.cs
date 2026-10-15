using InkRecognition;
using System.Text.Json;
if(args.Length!=7)throw new ArgumentException("AK LK engine separation case publication capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var sampled=MainFixture.Create(ak,engine,table);int guards=0;
using(var file=File.Create(args[6]))using(var w=new Utf8JsonWriter(file))
{
    w.WriteStartObject();w.WriteStartArray("branches");
    foreach(bool caseAware in new[]{false,true})
    {
        var (span,owned)=Create(caseAware);
        var reference=ReadAll(span,(tag,q)=>span.Read(owned,tag,q));
        var live=span.CaptureLivePublicationEpoch(owned,default);
        var actual=ReadAll(span,(tag,q)=>span.ReadLivePublicationQuery(live,tag,q,false,default));
        if(actual.Length==0||!actual.AsSpan().SequenceEqual(reference))throw new Exception("Live query changed actual backend output");
        span.ValidateLivePublicationEpoch(live,default);
        using(var cancel=new CancellationTokenSource())
        {cancel.Cancel();Reject(()=>span.ValidateLivePublicationEpoch(live,cancel.Token));}
        span.ValidateLivePublicationEpoch(live,default);
        //A normal query touches the same slots even though D0 did not change.
        span.Read(owned,8UL<<40,Q(span));Reject(()=>span.ValidateLivePublicationEpoch(live,default));
        var (endSpan,endOwned)=Create(caseAware);var ended=endSpan.CaptureLivePublicationEpoch(endOwned,default);
        endSpan.EndLivePublicationEpoch(ended,default);Reject(()=>endSpan.ValidateLivePublicationEpoch(ended,default));
        //Ending publication preserves the original span and does not reset it.
        endSpan.Read(endOwned,8UL<<40,Q(endSpan));
        var (epochSpan,epochOwned)=Create(caseAware);var stale=epochSpan.CaptureLivePublicationEpoch(epochOwned,default);
        epochSpan.AdvanceNodeEpoch(0);Reject(()=>epochSpan.ValidateLivePublicationEpoch(stale,default));
        var (mapSpan,mapOwned)=Create(caseAware);var oldMap=mapSpan.CaptureLivePublicationEpoch(mapOwned,default);
        mapSpan.Prepare(0,0);Reject(()=>mapSpan.ValidateLivePublicationEpoch(oldMap,default));
        var (recapSpan,recapOwned)=Create(caseAware);var replaced=recapSpan.CaptureLivePublicationEpoch(recapOwned,default);
        var current=recapSpan.CaptureLivePublicationEpoch(recapOwned,default);
        Reject(()=>recapSpan.ValidateLivePublicationEpoch(replaced,default));
        recapSpan.ValidateLivePublicationEpoch(current,default);
        Reject(()=>recapSpan.ValidateLivePublicationEpoch(live,default));
        recapSpan.ValidateLivePublicationEpoch(current,default);recapSpan.EndLivePublicationEpoch(current,default);
        w.WriteStartObject();w.WriteBoolean("case",caseAware);w.WriteNumber("rows",actual.Length/32);
        w.WriteString("reference",Convert.ToHexString(reference));w.WriteString("actual",Convert.ToHexString(actual));w.WriteEndObject();
    }
    w.WriteEndArray();w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
}
Console.WriteLine($"PASS actual DIRECT/CASE pre-V80 same-backend live queries;{guards} cancel/externalquery/end/V80/D0/recapture/foreign guards. No intermediate materialization claim.");
(LegacyOwnedMainWordSpanPreparation Span,LegacyOwnedMainWordSpan Owned) Create(bool caseAware)
{
    LegacyOwnedMainWordSpanPreparation span;
    if(caseAware)
    {
        var dictionary=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,sampled,cases,publication);
        span=new(ak,dictionary,sampled,table);
    }
    else span=new(ak,lk,engine,sampled,table);
    span.AdvanceNodeEpoch(-1);return(span,span.Prepare(0,0));
}
LegacyPreparedDictionaryQuery Q(LegacyOwnedMainWordSpanPreparation span)=>new()
{Capacity=7,ContextRoot=span.Contexts.ContainerIdentity,Parameter1c=0,Parameter20=0,Flags24=0x100};
byte[] ReadAll(LegacyOwnedMainWordSpanPreparation span,Func<ulong,LegacyPreparedDictionaryQuery,LegacyAggregateDictionaryQueryPage> read)
{
    var q=Q(span);using var bytes=new MemoryStream();
    for(int page=0;page<100;page++)
    {var p=read(8UL<<40,q);if(p.ReturnedCount==0)return bytes.ToArray();if(p.ReturnedCount<0||p.Records.Length!=p.ReturnedCount*32)throw new Exception("Invalid page");bytes.Write(p.Records.Span);q.Skip+=p.ReturnedCount;}
    throw new Exception("Unexpected pagination extent");
}
void Reject(Action action){try{action();}catch(Exception e)when(e is InvalidOperationException or ArgumentException or OperationCanceledException){guards++;return;}throw new Exception("Stale live query lease accepted");}
