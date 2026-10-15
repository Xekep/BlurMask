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
uint[] headers=owner.CopySourceHeaders();
LegacyNativeWordHeapRouter Router(LegacyNativeWordRecordHeap heap)=>new([heap,new(256)],new byte[]{0,0},-1,uint.MaxValue);
var initial=new LegacyNativeMainInitialSpecialCandidates(ak,owner,new(1,0),new byte[80]);
byte[]? actualQuery=null,actualParent=null,actualLanguageRow=null;int selectedNode=-1,actualTerminals=0;
for(int node=0;node<owner.SourceGraph.NodeCount;node++)
{
    if(node<1||owner.GetSpanExtent(node)<=node)continue;
    owner.AdvanceNodeEpoch(node-1);var span=owner.Prepare(node,(byte)node);
    var input=new LegacyNativeWordRecordHeap(256);
    var q=new LegacyPreparedDictionaryQuery{Capacity=512,ContextRoot=owner.Contexts.ContainerIdentity,Parameter1c=(uint)node,Parameter20=(uint)node};
    while(true)
    {
        var page=owner.Read(span,8UL<<40,q);if(page.ReturnedCount==0)break;
        Require(page.ReturnedCount>0,"Actual case startup query failed.");
        initial.ProcessPreparedPage(span,page.Records.Span,0,Router(input),controller.SecondaryScaleEc,false,false);
        q.Skip+=page.ReturnedCount;
    }
    var rows=Enumerable.Range(0,input.Count).Select(i=>LegacyNativeWordRowMaterializer.Materialize(input.GetSlotSnapshot(i).Span,(ushort)i,1,1,1).Record.ToArray()).ToArray();
    for(ushort i=0;i<rows.Length;i++)
    {
        var injected=LegacyNativeFirstDictionaryWordCompletion.CreateAlreadyTerminalQuery(rows[i],new byte[32]);if(!injected.HasValue)continue;
        var output=new LegacyNativeWordRecordHeap(256);
        var first=LegacyNativeFirstDictionaryWordCompletion.ForFreshCaseSpan(policy,span).ProcessCandidate(injected.Value.Span,node,i,rows[i],Router(output),
            false,new byte[40],(n,a)=>{Require(n==node&&a<rows.Length,"First anchor owner differs.");return rows[a];},false);
        Require(first.LanguageAccepted&&first.Allocation.Accepted,"Real first case language completion failed.");
        actualQuery=injected.Value.ToArray();actualParent=rows[i];actualLanguageRow=output.GetSlotSnapshot(0).Span[..88].ToArray();selectedNode=node;actualTerminals++;
    }
}
Require(actualTerminals>0&&actualQuery is not null&&actualParent is not null&&actualLanguageRow is not null,"Need real case startup/first-language payload.");
owner.AdvanceNodeEpoch(selectedNode-1);var live=owner.Prepare(selectedNode,(byte)selectedNode);
Require(live.Span!=0,"Fixture must distinguish current map span from later anchor span.");
int previousNode=selectedNode-1;
byte[] current=actualParent!.ToArray();current[6]&=254;current[7]&=254;current[32]=0;
W32(current,56,selectedNode);W16(current,60,0);W16(current,30,0);W16(current,24,selectedNode);
var candidateArc=owner.SourceGraph.GetNode(selectedNode).First(a=>a.Span==0);
ushort arc=checked((ushort)owner.SourceGraph.GetNode(selectedNode).ToList().IndexOf(candidateArc));W16(current,28,arc);
byte[] primaryPrevious=actualParent!.ToArray(),secondaryPrevious=actualLanguageRow!.ToArray();
primaryPrevious[4]=secondaryPrevious[4]=1;primaryPrevious[33]&=127;secondaryPrevious[33]&=127;
W64(primaryPrevious,40,U64(secondaryPrevious,40));
//These row placements/context16 are explicitly PREPARED HISTORY. The class
//payload is the actual first-language output; no later NN/winner is claimed.
byte[] request=new byte[80];W32(request,0,selectedNode);W32(request,8,previousNode);W32(request,32,0);W16(request,12,arc);
byte[] context=new byte[16];W32(context,0,100000);W32(context,4,321);W32(context,8,-47);W16(context,12,controller.Configuration.PrimaryArcLimit230a);W16(context,14,65535);
var packet=new LegacyNativeLanguageContextRecombinationResult(true,context,request,1,2);
var captures=new List<Capture>();int prepared=0,guards=0,snapshots=0;
ushort[] symbols=new[]{"a",".",","}.Select(text=>model.AcousticAlphabet.TryGetLabel(text,out ushort label)?label:throw new Exception("Actual ordinary AL symbol missing.")).ToArray();
for(int state=0;state<28;state++)foreach(ushort symbol in symbols)foreach(bool suppress in new[]{false,true})foreach(bool history in new[]{false,true})
{
    byte[] query=actualQuery!.ToArray();ulong tag=U64(query,0)&~((255UL<<56)|(7UL<<40));
    tag|=(ulong)(state<<2)<<56;tag|=(2UL|(suppress?4UL:0UL)|(symbol==symbols[2]?1UL:0UL))<<40;
    W64(query,0,tag);W16(query,20,symbol);W32(query,16,17);
    Run(query,history,false,"prepared-history-state");prepared++;
}
//The fresh RU auxiliary map has17 edges and zero terminals. This branch is a
//prepared actual-terminal-query consumer control, NOT a claimed V48 output.
Run(actualQuery!,true,true,"prepared-nonterminal-current-terminal-query");prepared++;
Run(actualQuery!,true,false,"actual-query-prepared-history");prepared++;
byte[] withContext=actualQuery!.ToArray();W64(withContext,8,owner.Contexts.ResolveInitial(U16(withContext,20)).Identity);
Run(withContext,true,false,"prepared-owned-query-context");prepared++;

LegacyNativeLaterDictionaryWordCompletion New()=>LegacyNativeLaterDictionaryWordCompletion.ForFreshCaseSpan(policy,live);
IReadOnlyList<LegacyNativeStoredWordRecord> Rows(int node,byte[] cur,byte[] prior,byte[] secondary)
{
    if(node==selectedNode)return [Stored(cur)];
    if(node!=previousNode)throw new ArgumentException("Unknown prepared history node.");
    //The collection extent is prepared SourceEnd23F0 = primary limit+1.
    var rows=Enumerable.Range(0,controller.Configuration.PrimaryArcLimit230a).Select(_=>Stored(prior)).ToList();
    rows.Add(Stored(secondary));return rows;
}
LegacyNativeLaterDictionaryWordCompletionResult Invoke(LegacyNativeLaterDictionaryWordCompletion target,byte[]? query=null,
    LegacySymbolContextFactory? contexts=null,int? node=null,int? available=null,bool f5=false,int subword=-1,bool extended=false,bool suppress=false,
    Func<int,IReadOnlyList<LegacyNativeStoredWordRecord>>? rows=null,Func<byte,uint>? header=null,Func<int,int,int>? line=null,CancellationToken token=default,
    LegacyNativeLanguageContextRecombinationResult? result=null)
    =>target.ProcessActualQuery(query??actualQuery!,result??packet,0,node??selectedNode,0,available??owner.SourceGraph.NodeCount,
        rows??(n=>Rows(n,current,primaryPrevious,secondaryPrevious)),header??(id=>headers[id]),line??sampled.GetSourceLineIdForArc,
        contexts??owner.Contexts,Router(new(256)),f5,subword,extended,new byte[40],suppress,token);
Reject(()=>LegacyNativeLaterDictionaryWordCompletion.ForFreshCaseSpan(null!,live));
Reject(()=>LegacyNativeLaterDictionaryWordCompletion.ForFreshCaseSpan(policy,null!));
Reject(()=>Invoke(New(),query:new byte[31]));Reject(()=>Invoke(New(),contexts:new()));Reject(()=>Invoke(New(),node:selectedNode+1));
Reject(()=>Invoke(New(),available:owner.SourceGraph.NodeCount+1));Reject(()=>Invoke(New(),f5:true));Reject(()=>Invoke(New(),subword:1));
Reject(()=>Invoke(New(),extended:true));Reject(()=>Invoke(New(),suppress:true));Reject(()=>Invoke(New(),header:_=>0));
byte[] primaryContext=context.ToArray();W16(primaryContext,12,0);
Reject(()=>Invoke(New(),result:packet with{Candidates16=primaryContext}));
byte[] outsideContext=context.ToArray();W16(outsideContext,12,controller.Configuration.PrimaryArcLimit230a+1);
Reject(()=>Invoke(New(),result:packet with{Candidates16=outsideContext}));
byte[] bad=actualQuery!.ToArray();bad[4]=2;Reject(()=>Invoke(New(),query:bad));
bad=actualQuery!.ToArray();W64(bad,8,new LegacySymbolContextFactory().ResolveInitial(1).Identity);Reject(()=>Invoke(New(),query:bad));
bad=actualQuery!.ToArray();bad[23]=3;Reject(()=>Invoke(New(),query:bad));
bad=actualQuery!.ToArray();bad[24]=1;Reject(()=>Invoke(New(),query:bad));
bad=actualQuery!.ToArray();Require(model.AcousticAlphabet.TryGetLabel("\ufffd",out ushort replacement),"Replacement absent.");W16(bad,20,replacement);Reject(()=>Invoke(New(),query:bad));
var canceled=New();Reject(()=>Invoke(canceled,token:new(true)));Reject(()=>Invoke(canceled));
byte[] mutable=actualQuery!.ToArray();var baseline=Invoke(New());
var snapshot=Invoke(New(),query:mutable,rows:n=>{Array.Fill(mutable,(byte)255);return Rows(n,current,primaryPrevious,secondaryPrevious);});
Require(snapshot.Language.Record40.Span.SequenceEqual(baseline.Language.Record40.Span)&&snapshot.RestartTag28==baseline.RestartTag28,"Actual query was not snapshotted before callbacks.");snapshots++;
for(int stage=0;stage<3;stage++)
{
    using var cts=new CancellationTokenSource();var target=New();
    Reject(()=>Invoke(target,token:cts.Token,rows:n=>{if(stage==0)cts.Cancel();return Rows(n,current,primaryPrevious,secondaryPrevious);},
        header:id=>{if(stage==1)cts.Cancel();return headers[id];},line:(n,a)=>{if(stage==2)cts.Cancel();return sampled.GetSourceLineIdForArc(n,a);}));
    Reject(()=>Invoke(target));
}
for(int stage=0;stage<2;stage++)
{
    var target=New();
    Reject(()=>Invoke(target,header:id=>{if(stage==0)owner.AdvanceNodeEpoch(selectedNode);return headers[id];},
        line:(n,a)=>{if(stage==1)owner.AdvanceNodeEpoch(selectedNode);return sampled.GetSourceLineIdForArc(n,a);}));
    Reject(()=>Invoke(target));
    owner.AdvanceNodeEpoch(selectedNode-1);live=owner.Prepare(selectedNode,(byte)selectedNode);
}
var stale=New();Reject(()=>Invoke(stale,rows:n=>{owner.AdvanceNodeEpoch(selectedNode);return Rows(n,current,primaryPrevious,secondaryPrevious);}));Reject(()=>Invoke(stale));
Reject(()=>New());
owner.AdvanceNodeEpoch(selectedNode-1);live=owner.Prepare(selectedNode,(byte)selectedNode);
var staleMap=New();dictionary.Session.RefreshPermissions(new byte[192]);Reject(()=>Invoke(staleMap));
owner.AdvanceNodeEpoch(selectedNode-1);live=owner.Prepare(selectedNode,(byte)selectedNode);
var foreign=new LegacyOwnedMainWordSpanPreparation(ak,dictionary,sampled,table);foreign.AdvanceNodeEpoch(selectedNode-1);var foreignSpan=foreign.Prepare(selectedNode,(byte)selectedNode);
Reject(()=>LegacyNativeLaterDictionaryWordCompletion.ForFreshCaseSpan(policy,foreignSpan));
using(var file=File.Create(args[6]))using(var w=new Utf8JsonWriter(file))
{
    w.WriteStartObject();w.WriteNumber("actualFirstTerminals",actualTerminals);w.WriteNumber("preparedHistoryCases",prepared);w.WriteNumber("guards",guards);w.WriteNumber("snapshotChecks",snapshots);
    w.WriteNumber("primaryLimit",controller.Configuration.PrimaryArcLimit230a);w.WriteNumber("previousSourceEnd",controller.Configuration.PrimaryArcLimit230a+1);w.WriteNumber("mapSpan",selectedNode);w.WriteNumber("anchorSpan",0);w.WriteStartArray("captures");
    foreach(var c in captures){w.WriteStartObject();w.WriteString("kind",c.Kind);w.WriteString("query",c.Query);w.WriteString("current",c.Current);w.WriteString("previous",c.Previous);
        w.WriteString("context",c.Context);w.WriteString("token",c.Token);w.WriteString("historyToken",c.HistoryToken);w.WriteString("residue",c.Residue);w.WriteString("storage",c.Storage);
        w.WriteString("language",c.Language);w.WriteString("slot",c.Slot);w.WriteNumber("restart",c.Restart);w.WriteNumber("path",c.Path);w.WriteNumber("lineage",c.Lineage);
        w.WriteBoolean("useHistory",c.UseHistory);w.WriteEndObject();}
    w.WriteEndArray();w.WriteEndObject();w.Flush();
}
Console.WriteLine($"PASS {actualTerminals} actual owned case startup/first-language terminals -> {prepared} explicitly PREPARED later-history cases (full query/metadata/language/128 records), {guards} guards/{snapshots} query snapshot. No native later scheduling/winner claim.");

void Run(byte[] query,bool useHistory,bool nonterminal,string kind)
{
    byte[] cur=current.ToArray(),prior=primaryPrevious.ToArray(),secondary=secondaryPrevious.ToArray();
    if(nonterminal)cur[5]&=253;
    if(!useHistory)prior[4]=secondary[4]=0;
    secondary[5]|=4; //Prepared previous boundary channel independent of case category.
    byte[] residue=Enumerable.Range(0,40).Select(i=>(byte)(i*37+19)).ToArray();
    byte[] storage=Enumerable.Range(0,256*128).Select(i=>(byte)(i*17+23)).ToArray();
    var heap=new LegacyNativeWordRecordHeap(256,storage);
    var token=owner.ReadWordTerminal(live,U64(query,0),U16(query,20),0).ToArray();
    var result=New().ProcessActualQuery(query,packet,0,selectedNode,0,owner.SourceGraph.NodeCount,n=>Rows(n,cur,prior,secondary),id=>headers[id],
        sampled.GetSourceLineIdForArc,owner.Contexts,Router(heap),false,-1,false,residue,false);
    Require(result.LanguageAccepted&&result.Allocation.Accepted&&result.History.UsesPreviousHistory==useHistory,"Prepared finite later completion failed.");
    captures.Add(new(kind,Convert.ToHexString(query),Convert.ToHexString(cur),Convert.ToHexString(secondary),Convert.ToHexString(context),
        Convert.ToHexString(token),Convert.ToHexString(result.History.SourceToken16.Span),Convert.ToHexString(residue),Convert.ToHexString(storage.AsSpan(0,128)),
        Convert.ToHexString(result.Language.Record40.Span),Convert.ToHexString(heap.GetSlotSnapshot(0).Span),result.RestartTag28,result.PreLexicalCost,result.ContextualLineage5c,useHistory));
}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException or InvalidDataException){guards++;return;}throw new Exception("Invalid case later ownership accepted.");}
static LegacyNativeStoredWordRecord Stored(byte[] row){byte[] slot=new byte[128];row.CopyTo(slot,0);return LegacyNativeWordRowMaterializer.Materialize(slot,0,0,1,1);}
static ushort U16(byte[] b,int p)=>BinaryPrimitives.ReadUInt16LittleEndian(b.AsSpan(p));
static ulong U64(byte[] b,int p)=>BinaryPrimitives.ReadUInt64LittleEndian(b.AsSpan(p));
static void W16(byte[] b,int p,int v)=>BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(p),(ushort)v);
static void W32(byte[] b,int p,int v)=>BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(p),v);
static void W64(byte[] b,int p,ulong v)=>BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(p),v);
static void Require(bool value,string text){if(!value)throw new Exception(text);}
internal sealed record Capture(string Kind,string Query,string Current,string Previous,string Context,string Token,string HistoryToken,
    string Residue,string Storage,string Language,string Slot,ulong Restart,int Path,ulong Lineage,bool UseHistory);
