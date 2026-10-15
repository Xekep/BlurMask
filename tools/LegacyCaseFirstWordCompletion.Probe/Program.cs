using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if(args.Length!=7)throw new ArgumentException("AK LK engine separation case-data publication-data output.json");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);
var caseData=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var sampled=MainFixture.Create(ak,engine,table);
var dictionary=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,sampled,caseData,publication);
var owner=new LegacyOwnedMainWordSpanPreparation(ak,dictionary,sampled,table);
var controller=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,owner);
var policy=LegacyFreshCaseMainSourcePolicy.Prepare(ak,lk,controller);
var initial=new LegacyNativeMainInitialSpecialCandidates(ak,owner,new(1,0),new byte[80]);
var captures=new List<CompletionCapture>();int actualCount=0,preparedCount=0,guards=0;
LegacyNativeWordHeapRouter Router(LegacyNativeWordRecordHeap heap)=>new([heap,new(256)],new byte[]{0,0},-1,uint.MaxValue);
byte[]? chosenParent=null,chosenQuery=null;int chosenNode=-1;ushort chosenOrdinal=0;
for(int node=0;node<owner.SourceGraph.NodeCount;node++)
{
    if(owner.GetSpanExtent(node)<=node)continue;
    owner.AdvanceNodeEpoch(node-1);var span=owner.Prepare(node,(byte)node);
    var heap=new LegacyNativeWordRecordHeap(256);var query=new LegacyPreparedDictionaryQuery{Capacity=512,ContextRoot=owner.Contexts.ContainerIdentity};
    while(true)
    {
        var page=owner.Read(span,8UL<<40,query);
        if(page.ReturnedCount==0)break;
        Require(page.ReturnedCount>0,"Actual case root query failed.");
        initial.ProcessPreparedPage(span,page.Records.Span,0,Router(heap),controller.SecondaryScaleEc,
            controller.Refresh.ConsecutiveBlank22f7!=0,controller.Configuration.SuppressPreviousContext2609!=0);
        query.Skip+=page.ReturnedCount;
    }
    var parents=Enumerable.Range(0,heap.Count).Select(i=>LegacyNativeWordRowMaterializer.Materialize(heap.GetSlotSnapshot(i).Span,
        (ushort)i,1,1,1).Record.ToArray()).ToArray();
    for(ushort i=0;i<parents.Length;i++)
    {
        var injected=LegacyNativeFirstDictionaryWordCompletion.CreateAlreadyTerminalQuery(parents[i],new byte[32]);
        if(!injected.HasValue)continue;
        Run(span,injected.Value.ToArray(),parents[i],i,parents,"actual-injected");actualCount++;
        chosenParent=parents[i];chosenQuery=injected.Value.ToArray();chosenNode=node;chosenOrdinal=i;
    }
}
Require(actualCount>0&&chosenParent is not null&&chosenQuery is not null,"No real case startup terminal reached completion.");
//Separate prepared state/punctuation controls reuse one real terminal and
//actual source owner. They do not pretend these modified tags were OCR winners.
owner.AdvanceNodeEpoch(chosenNode-1);var preparedSpan=owner.Prepare(chosenNode,(byte)chosenNode);
var labels=new[]{"a",".",","}.Select(text=>model.AcousticAlphabet.TryGetLabel(text,out ushort label)?label:throw new Exception("Actual AL label missing.")).ToArray();
for(int state=0;state<28;state++)foreach(ushort symbol in labels)foreach(bool suppress in new[]{false,true})
{
    byte[] q=chosenQuery!.ToArray(),p=chosenParent!.ToArray();
    ulong tag=U64(q,0)&~((255UL<<56)|(7UL<<40));tag|=(ulong)(state<<2)<<56;
    tag|=(2UL|(suppress?4UL:0UL)|(symbol==labels[2]?1UL:0UL))<<40;
    W64(q,0,tag);W16(q,20,symbol);W16(p,36,symbol);
    var rows=new byte[chosenOrdinal+1][];for(int i=0;i<rows.Length;i++)rows[i]=p;
    Run(preparedSpan,q,p,chosenOrdinal,rows,"prepared-state-punctuation");preparedCount++;
}
Require(preparedCount==168,"Prepared state coverage changed.");

LegacyNativeFirstDictionaryWordCompletion New()=>LegacyNativeFirstDictionaryWordCompletion.ForFreshCaseSpan(policy,preparedSpan);
void Invoke(LegacyNativeFirstDictionaryWordCompletion target,int? node=null,bool? f5=null,bool? suppress=null,Func<int,ushort,ReadOnlyMemory<byte>>? lookup=null)
    =>target.ProcessCandidate(chosenQuery!,node??chosenNode,chosenOrdinal,chosenParent!,Router(new(256)),
        f5??controller.KnowledgeF5,new byte[40],lookup??((_,_)=>chosenParent!),suppress??(controller.Configuration.SuppressPreviousContext2609!=0));
Reject(()=>LegacyNativeFirstDictionaryWordCompletion.ForFreshCaseSpan(null!,preparedSpan));
Reject(()=>LegacyNativeFirstDictionaryWordCompletion.ForFreshCaseSpan(policy,null!));
Reject(()=>Invoke(New(),node:chosenNode+1));Reject(()=>Invoke(New(),f5:true));Reject(()=>Invoke(New(),suppress:true));
var wrongSource=chosenQuery!.ToArray();wrongSource[4]=2;
Reject(()=>New().ProcessCandidate(wrongSource,chosenNode,chosenOrdinal,chosenParent!,Router(new(256)),false,new byte[40],(_,_)=>chosenParent!,false));
var canceled=New();Reject(()=>canceled.ProcessCandidate(chosenQuery!,chosenNode,chosenOrdinal,chosenParent!,Router(new(256)),false,new byte[40],(_,_)=>chosenParent!,false,new(true)));Reject(()=>Invoke(canceled));
var invalidated=New();Reject(()=>Invoke(invalidated,lookup:(_,_)=>{owner.AdvanceNodeEpoch(chosenNode);return chosenParent!;}));Reject(()=>Invoke(invalidated));
Reject(()=>LegacyNativeFirstDictionaryWordCompletion.ForFreshCaseSpan(policy,preparedSpan));
var foreign=new LegacyOwnedMainWordSpanPreparation(ak,dictionary,sampled,table);foreign.AdvanceNodeEpoch(chosenNode-1);var foreignSpan=foreign.Prepare(chosenNode,(byte)chosenNode);
Reject(()=>LegacyNativeFirstDictionaryWordCompletion.ForFreshCaseSpan(policy,foreignSpan));
//A refreshed shared case backend invalidates the installed map even if this
//span owner's local epoch did not advance.
owner.AdvanceNodeEpoch(chosenNode-1);var last=owner.Prepare(chosenNode,(byte)chosenNode);var stale=LegacyNativeFirstDictionaryWordCompletion.ForFreshCaseSpan(policy,last);
dictionary.Session.RefreshPermissions(new byte[192]);Reject(()=>Invoke(stale));
using(var file=File.Create(args[6]))using(var w=new Utf8JsonWriter(file))
{
    w.WriteStartObject();w.WriteNumber("actualCount",actualCount);w.WriteNumber("preparedCount",preparedCount);w.WriteNumber("guards",guards);
    w.WriteStartArray("captures");foreach(var c in captures)
    {
        w.WriteStartObject();w.WriteString("kind",c.Kind);w.WriteNumber("node",c.Node);w.WriteNumber("ordinal",c.Ordinal);
        w.WriteString("query",c.Query);w.WriteString("parent",c.Parent);w.WriteString("token",c.Token);w.WriteString("residue",c.Residue);
        w.WriteString("storage",c.Storage);w.WriteString("language",c.Language);w.WriteString("slot",c.Slot);w.WriteNumber("restart",c.Restart);w.WriteNumber("path",c.Path);
        w.WriteStartObject("score");w.WriteNumber("wordClass",c.Score.WordClass);w.WriteNumber("wordCost",c.Score.WordCost);
        w.WriteNumber("classCost",c.Score.ClassCost);w.WriteNumber("categoryCost",c.Score.CategoryCost);w.WriteNumber("boundaryCost",c.Score.BoundaryCost);
        w.WriteNumber("lexicalSubtotal",c.Score.LexicalSubtotal);w.WriteEndObject();w.WriteEndObject();
    }
    w.WriteEndArray();w.WriteEndObject();w.Flush();
}
Console.WriteLine($"PASS {actualCount} actual case query/startup/materialized terminal completions +{preparedCount} explicitly prepared state/punctuation cases; {guards} owner/node/flags/map/epoch/cancel guards. No fullcase scheduler or OCR winner claim.");

void Run(LegacyOwnedMainWordSpan span,byte[] query,byte[] parent,ushort ordinal,byte[][] rows,string kind)
{
    byte[] residue=Enumerable.Range(0,40).Select(i=>(byte)(i*37+19)).ToArray();
    byte[] storage=Enumerable.Range(0,256*128).Select(i=>(byte)(i*17+23)).ToArray();
    var output=new LegacyNativeWordRecordHeap(256,storage);var target=LegacyNativeFirstDictionaryWordCompletion.ForFreshCaseSpan(policy,span);
    ulong tag=U64(query,0);ushort symbol=U16(query,20);
    var token=owner.ReadWordTerminal(span,tag,symbol,0).ToArray();
    var result=target.ProcessCandidate(query,span.EndingNode,ordinal,parent,Router(output),controller.KnowledgeF5,residue,
        (node,arc)=>{Require(node==span.EndingNode&&arc<rows.Length,"Unknown actual row owner.");return rows[arc];},controller.Configuration.SuppressPreviousContext2609!=0);
    Require(result.LanguageAccepted&&result.Allocation.Accepted&&output.Count==1,"Bounded finite completion unexpectedly rejected.");
    captures.Add(new(kind,span.EndingNode,ordinal,Convert.ToHexString(query),Convert.ToHexString(parent),
        Convert.ToHexString(token),Convert.ToHexString(residue),Convert.ToHexString(storage.AsSpan(0,128)),
        Convert.ToHexString(result.Language.Record40.Span),Convert.ToHexString(output.GetSlotSnapshot(0).Span),
        result.RestartTag28,result.PreLexicalCost,result.Language.Score));
}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Foreign/stale/unsupported completion accepted.");}
static ushort U16(byte[] bytes,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(at));
static ulong U64(byte[] bytes,int at)=>BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(at));
static void W16(byte[] bytes,int at,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(at),value);
static void W64(byte[] bytes,int at,ulong value)=>BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(at),value);
static void Require(bool value,string text){if(!value)throw new Exception(text);}
internal sealed record CompletionCapture(string Kind,int Node,ushort Ordinal,string Query,string Parent,string Token,
    string Residue,string Storage,string Language,string Slot,ulong Restart,int Path,LegacyWordLanguageScore Score);
