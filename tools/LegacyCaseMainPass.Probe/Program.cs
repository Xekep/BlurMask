using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
using System.Runtime.InteropServices;
if(args.Length is not (7 or 9 or 10))throw new ArgumentException("AK LK engine separation case-data publication-data capture [line30 line50 [physicalCount]]");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);
var caseData=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var sampled=MainFixture.Create(ak,engine,table);
var dictionary=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,sampled,caseData,publication);
var owner=new LegacyOwnedMainWordSpanPreparation(ak,dictionary,sampled,table);
var controller=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,owner);
var policy=LegacyFreshCaseMainSourcePolicy.Prepare(ak,lk,controller);
ulong identity=0x100;int allocations=0;
LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
{
    allocations++;byte[] storage=new byte[request.RequiredBytes];request.PreviousStorage.Span.CopyTo(storage);
    return new(storage,++identity);
}
try{_=new LegacyOwnedMainDirectWordPass(ak,controller,table,Allocate);throw new Exception("DIRECT accepted case factory.");}catch(NotSupportedException){}
var pass=new LegacyOwnedMainCaseWordPass(ak,policy,table,Allocate);var ownedResult=pass.RunOwned();
IReadOnlyList<LegacyMainDirectWordNodeResult> result=Enumerable.Range(0,ownedResult.NodeCount).Select(ownedResult.GetNodeSnapshot).ToArray();
Require(ReferenceEquals(ownedResult.SourcePolicy,policy),"Case owned result lost factory binding.");
var copy=ownedResult.GetNodeSnapshot(0);Require(MemoryMarshal.TryGetArray(copy.Records,out var copiedStorage),"Expected portable record array.");
byte before=ownedResult.GetNodeSnapshot(0).Records.Span[0];copiedStorage.Array![copiedStorage.Offset]^=255;
Require(ownedResult.GetNodeSnapshot(0).Records.Span[0]==before,"Public snapshot mutated owned case storage.");
Require(result.Count==owner.SourceGraph.NodeCount&&result.Count>0,"Case scheduler did not cover actual graph.");
int root=0,parent=0,language=0;
foreach(var node in result)
{
    Require(node.Records.Length%88==0&&node.SourceEnd23f0<=node.Records.Length/88,"Actual source row extent differs.");
    root+=node.RootQueries;parent+=node.ParentQueries;language+=node.LanguageAllocations;
    for(int ordinal=0;ordinal<node.SourceEnd23f0;ordinal++)
    {
        var row=node.Records.Span.Slice(ordinal*88,88);
        if(BinaryPrimitives.ReadInt32LittleEndian(row[16..])>=0x4000000)continue;
        Require(row[4]==1,"Finite case row escaped selected source1.");
    }
}
Require(root>0&&parent>0&&language>0,"Case dictionary queries failed to reach language allocations.");
try{pass.Run();throw new Exception("Pass reused.");}catch(InvalidOperationException){}
Console.WriteLine($"PASS actual fresh292 main pass: {result.Count} nodes, {root} root queries, {parent} parent queries, {language} language allocations, {allocations} owned allocations. No text winner claim.");
using var file=File.Create(args[6]);using var writer=new Utf8JsonWriter(file);writer.WriteStartArray();Capture("single",result,controller.Configuration.PrimaryArcLimit230a);
if(args.Length>=9)
{
    var line=MainLineWordFixture.Create(ak,engine,File.ReadAllBytes(args[3]),File.ReadAllBytes(args[7]),File.ReadAllBytes(args[8]),caseData,publication,args.Length==10?int.Parse(args[9]):14);
    var lineDictionary=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,line.Source,caseData,publication);
    var lineOwner=new LegacyOwnedMainWordSpanPreparation(ak,lineDictionary,line.Source,line.Context,table);
    var lineController=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,lineOwner);
    var linePolicy=LegacyFreshCaseMainSourcePolicy.Prepare(ak,lk,lineController);
    var linePass=new LegacyOwnedMainCaseWordPass(ak,linePolicy,lineOwner.MainLineAcoustic,table,Allocate);
    var lineResult=linePass.Run();
    Require(lineResult.Count==lineOwner.SourceGraph.NodeCount,"Main line did not cover graph.");
    Capture("line",lineResult,lineController.Configuration.PrimaryArcLimit230a);
    Console.WriteLine($"PASS actual multiline292: {lineResult.Count} nodes, {lineResult.Sum(n=>n.RootQueries)} root/{lineResult.Sum(n=>n.ParentQueries)} parent queries, {lineResult.Sum(n=>n.LanguageAllocations)} language allocations.");
}
writer.WriteEndArray();writer.Flush();
void Capture(string scope,IReadOnlyList<LegacyMainDirectWordNodeResult> results,ushort primary)
{
    writer.WriteStartObject();writer.WriteString("scope",scope);writer.WriteNumber("primary",primary);writer.WriteStartArray("nodes");
    foreach(var n in results)
    {
        writer.WriteStartObject();writer.WriteNumber("node",n.EndingNode);writer.WriteNumber("sourceEnd",n.SourceEnd23f0);writer.WriteNumber("retainedEnd",n.RetainedEnd23f8);
        writer.WriteNumber("cumulative",n.CumulativePathCost2520);writer.WriteNumber("offset",n.AppliedPeriodicOffset);writer.WriteBoolean("language",n.LanguageRan);
        writer.WriteBase64String("records",n.Records.Span);writer.WriteBase64String("alternatives",n.PrimaryAlternatives.Span);writer.WriteEndObject();
    }
    writer.WriteEndArray();writer.WriteEndObject();
}
static void Require(bool value,string message){if(!value)throw new Exception(message);}
