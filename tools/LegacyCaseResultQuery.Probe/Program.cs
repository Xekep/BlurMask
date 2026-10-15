using InkRecognition;
using System.Text.Json;
if(args.Length!=7)throw new ArgumentException("AK LK engine separation case-data publication-data capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);int guards=0;
var owned=Create();var session=owned.Dictionary.Session;Reject(()=>session.CaptureResultQueryLease(default));
owned.Pass.Run();
var reference=ReadAll((tag,q)=>session.Read(tag,q));
Reject(()=>session.CaptureResultQueryLease(default));
session.PrepareControllerNode(session.Graph.NodeCount-1);
var lease=session.CaptureResultQueryLease(default);
var actual=ReadAll((tag,q)=>session.ReadResultQuery(lease,tag,q,false,default));
Require(actual.Length>0&&actual.AsSpan().SequenceEqual(reference),"Result lease changed retained-map case query output.");
session.ValidateResultQueryLease(lease,default);
Reject(()=>session.ReadResultQuery(lease,8UL<<40,Q(),false,new(true)));
session.ValidateResultQueryLease(lease,default);
Reject(()=>session.CaptureResultQueryLease(default));
//An external read changes the shared native query slots, even without D0.
session.Read(8UL<<40,Q());Reject(()=>session.ValidateResultQueryLease(lease,default));
session.PrepareControllerNode(session.Graph.NodeCount-1);
var second=session.CaptureResultQueryLease(default);
Require(ReferenceEquals(lease.Map,second.Map),"V80 replaced the actual permission map.");
Reject(()=>session.ValidateResultQueryLease(lease,default));
var foreign=Create();foreign.Pass.Run();var foreignLease=foreign.Dictionary.Session.CaptureResultQueryLease(default);
Reject(()=>session.ValidateResultQueryLease(foreignLease,default));
Reject(()=>session.ReadResultQuery(foreignLease,8UL<<40,Q(),false,default));
session.ValidateResultQueryLease(second,default);
var repeated=ReadAll((tag,q)=>session.ReadResultQuery(second,tag,q,false,default));
Require(repeated.AsSpan().SequenceEqual(reference),"Result reconstruction did not retain the same normal50 map.");
session.RefreshPermissions(Enumerable.Repeat((byte)1,model.AcousticAlphabet.LabelCount).ToArray());
Reject(()=>session.ValidateResultQueryLease(second,default));
Reject(()=>session.CaptureResultQueryLease(default));
using(var file=File.Create(args[6]))using(var w=new Utf8JsonWriter(file))
{w.WriteStartObject();w.WriteNumber("guards",guards);w.WriteNumber("rows",actual.Length/32);w.WriteString("reference",Convert.ToHexString(reference));w.WriteString("actual",Convert.ToHexString(actual));w.WriteEndObject();w.Flush();}
Console.WriteLine($"PASS post-final-V80 same292 normal50 queries {actual.Length/32} rows; unchanged map across resets; {guards} capture/epoch/query/map/foreign/cancel guards; no extra D0 or new backend.");
LegacyPreparedDictionaryQuery Q()=>new(){Capacity=7,ContextRoot=session.Contexts.ContainerIdentity,Parameter1c=(uint)(session.Graph.NodeCount-1),Parameter20=0,Flags24=0x100};
byte[] ReadAll(Func<ulong,LegacyPreparedDictionaryQuery,LegacyAggregateDictionaryQueryPage> read)
{
    var q=Q();using var bytes=new MemoryStream();
    for(int page=0;page<100;page++)
    {
        var result=read(8UL<<40,q);Require(result.ReturnedCount>=0&&result.Records.Length==result.ReturnedCount*32,"Actual case query extent differs.");
        if(result.ReturnedCount==0)return bytes.ToArray();bytes.Write(result.Records.Span);q.Skip+=result.ReturnedCount;
    }
    throw new Exception("Query budget exhausted.");
}
(LegacyFreshCaseRussianDictionaryPreparation Dictionary,LegacyOwnedMainCaseWordPass Pass) Create()
{
    var source=MainFixture.Create(ak,engine,table);
    var dictionary=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,source,cases,publication);
    var span=new LegacyOwnedMainWordSpanPreparation(ak,dictionary,source,table);
    var policy=LegacyFreshCaseMainSourcePolicy.Prepare(ak,lk,LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,span));ulong id=0x1000;
    return(dictionary,new(ak,policy,table,r=>{byte[] b=new byte[r.RequiredBytes];r.PreviousStorage.Span.CopyTo(b);return new(b,++id);}));
}
void Reject(Action action){try{action();}catch(Exception e)when(e is InvalidOperationException or ArgumentException or OperationCanceledException){guards++;return;}throw new Exception("Invalid result source lease accepted.");}
static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
