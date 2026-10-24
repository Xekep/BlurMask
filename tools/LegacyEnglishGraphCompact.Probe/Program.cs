using InkRecognition;
using System.Buffers.Binary;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

if(args is not [var akPath,var fixturesPath,var output]||File.Exists(output))throw new ArgumentException("AK prepared.json fresh-output.json");
byte[] Read(string path,int cap){var n=new FileInfo(path).Length;if(n<1||n>cap)throw new InvalidDataException("Bounded input required.");return File.ReadAllBytes(path);}
byte[] ak=Read(akPath,1024*1024);if(Convert.ToHexString(SHA256.HashData(ak)).ToLowerInvariant()!="ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657")throw new InvalidDataException("Actual AL217 resource required.");
byte[] fixtures=Read(fixturesPath,32*1024*1024);if(Convert.ToHexString(SHA256.HashData(fixtures)).ToLowerInvariant()!="74e2e74d49e902e1a2e84180826d8563e8ef4c459cd94301382680a6ca1b2ba7")throw new InvalidDataException("Exact prepared fixtures required.");
using var doc=JsonDocument.Parse(fixtures);if(doc.RootElement.GetProperty("Scope").GetString()!="PREPARED_MARKED207CB0_ONLY")throw new Exception("Prepared only.");
var source=LegacySource247WireLayout.Read(ak);var alphabet=source.Alphabet;
var categories=LegacyCursiveCategoryProfile.Load(ak.AsSpan(182477),182477,out _);
var configuration=new LegacyNativeWordControllerConfiguration(2,2,1,125,.25f,0,3,0,0,uint.MaxValue);
var ctor=typeof(LegacyMainCompactGroupScores).GetConstructor(BindingFlags.Instance|BindingFlags.NonPublic,null,
    [typeof(ushort[]),typeof(Func<ushort,CancellationToken,float>)],null)??throw new Exception("Prepared private numeric constructor changed.");
using var file=File.Create(output);using var writer=new Utf8JsonWriter(file);
writer.WriteStartObject();writer.WriteString("Scope","PREPARED_MARKED207CB0_2257_21FFD0_NUMERIC_NOT_LIVE_OWNER");writer.WriteNumber("NNCalls",0);writer.WriteStartArray("Cases");
foreach(var sample in doc.RootElement.GetProperty("Cases").EnumerateArray())
{
    int id=sample.GetProperty("Id").GetInt32();var contexts=new LegacySymbolContextFactory(256);var ids=new List<ulong>();
    foreach(var path in sample.GetProperty("ContextPaths").EnumerateArray())
    {
        ulong parent=0;foreach(var entry in path.EnumerateArray())
        {ushort symbol=entry.GetUInt16();var next=parent==0?contexts.ResolveInitial(symbol):contexts.ResolveChild(parent,symbol);if(parent!=0&&next.Created)contexts.LinkCreatedContext(next.Identity,parent);parent=next.Identity;}
        ids.Add(parent);
    }
    if(ids.Count!=5||ids.Distinct().Count()!=5)throw new Exception("Prepared symbolic mapping changed.");
    ulong Map(ulong v)=>v is >=1 and <=5?ids[checked((int)v-1)]:throw new Exception("Unknown prepared logical context.");
    var owned=new LegacyMainDirectWordNodeResult[2];int node=0;
    foreach(var n in sample.GetProperty("Nodes").EnumerateArray())
    {
        var rows=Convert.FromHexString(n.GetProperty("Rows").GetString()!);var primary=Convert.FromHexString(n.GetProperty("Primary").GetString()!);
        if(rows.Length!=156*88||primary.Length!=125*80)throw new Exception("Whole prepared extents required.");
        for(int o=0;o<126;o++)
        {
            if(BinaryPrimitives.ReadInt32LittleEndian(rows.AsSpan(o*88+16))>=0x4000000)continue;
            BinaryPrimitives.WriteUInt64LittleEndian(rows.AsSpan(o*88+8),Map(BinaryPrimitives.ReadUInt64LittleEndian(rows.AsSpan(o*88+8))));
            if(o>=125)continue;
            for(int k=0;k<2;k++)
            {
                int at=o*80+k*40;if(BinaryPrimitives.ReadInt32LittleEndian(primary.AsSpan(at))>=0x4000000)break;
                BinaryPrimitives.WriteUInt64LittleEndian(primary.AsSpan(at+8),Map(BinaryPrimitives.ReadUInt64LittleEndian(primary.AsSpan(at+8))));
                BinaryPrimitives.WriteUInt64LittleEndian(primary.AsSpan(at+16),Map(BinaryPrimitives.ReadUInt64LittleEndian(primary.AsSpan(at+16))));
            }
        }
        owned[node]=new(node,0,0,0,0,true,0,0,126,126,rows,primary);node++;
    }
    LegacyOrdinaryCursiveCandidateArc[][] arcs=[[new(0,0,0,0,1,new int[]{-1},[])],[new(1,0,0,1,2,new int[]{0},[])]];
    LegacyOrdinaryCursiveCandidateInk[] groups=[new(0,new int[]{0},new LegacyInkRange[]{new(0,0,1)}),new(1,new int[]{1},new LegacyInkRange[]{new(0,0,1)})];
    var graph=LegacyOrdinaryRecognitionGraph.FromSameSource(new(arcs,groups,8));
    var scores=new LegacyMainCompactGroupScores[2];var weightsOutput=new List<int[]>();
    for(int group=0;group<2;group++)
    {
        var input=sample.GetProperty("Candidates")[group].EnumerateArray().ToArray();var symbols=input.Select(x=>x[0].GetUInt16()).ToArray();var costs=input.Select(x=>x[1].GetInt32()).ToArray();
        if(!costs.SequenceEqual(costs.OrderBy(x=>x)))throw new Exception("Prepared ordered cache required.");
        var weights=LegacyOwnedRussianMainGlyphConfidenceGroup.CalculateCandidateWeights(costs,sample.GetProperty("Scales")[group].GetSingle());
        weightsOutput.Add(weights.Select(BitConverter.SingleToInt32Bits).ToArray());
        Func<ushort,CancellationToken,float> lookup=(symbol,ct)=>{ct.ThrowIfCancellationRequested();for(int i=0;i<symbols.Length;i++)if(symbols[i]==symbol)return weights[i];return 0;};
        scores[group]=(LegacyMainCompactGroupScores)ctor.Invoke([symbols,lookup]);
        if(!scores[group].CandidateSymbols.Span.SequenceEqual(symbols))throw new Exception("Symbol-only adapter changed order.");
    }
    var probes=new List<int[]>();bool Probe(int n,ushort ordinal,CancellationToken ct)
    {ct.ThrowIfCancellationRequested();var row=owned[n].Records.Span.Slice(ordinal*88,88);bool result=(row[5]&2)!=0||sample.GetProperty("Replies")[ordinal%2].GetBoolean();probes.Add([n,ordinal,result?1:0]);return result;}
    var residue=Enumerable.Repeat((byte)0xa7,4096*40).ToArray();var result=LegacyPreparedMainCompactPublication.Run(configuration,graph,contexts,alphabet,categories,false,sample.GetProperty("Penalty").GetInt32(),false,0,1,
        owned,Probe,g=>scores[g],residue,0,new int[3],new int[3],ct=>ct.ThrowIfCancellationRequested(),default,[0,0xc0e]);
    var final=result.Nodes[1];var winner=LegacyNativeMainTerminalSelection.SelectSingleAlternativeWithoutRescue(final.Records.Span,final.SourceEnd23f0,[new(0,0),new(0xc0e,sample.GetProperty("Penalty").GetInt32())],new(125,false,false,false,false,0),0,contexts);
    int selected=winner.HasWinner?checked(result.Starts[1]+(int)winner.RowField22):-1;
    var backtrack=winner.HasWinner?LegacyPreparedMainGlyphBacktrackRows.Run(graph,alphabet,BinaryPrimitives.ReadUInt16LittleEndian(alphabet.CopySerializedHeader().AsSpan(6)),
        result.Storage,result.Starts,result.Count,selected,65535,Enumerable.Repeat((byte)0x5d,72).ToArray(),default):null;
    JsonSerializer.Serialize(writer,new{Id=id,ContextMap=ids,Contexts=contexts.CopyNodes(),WeightsBits=weightsOutput,ProbeCalls=probes,Count=result.Count,Starts=result.Starts,PrefixOwners=result.PrefixOwners,
        Storage=Convert.ToHexString(result.Storage),Nodes=result.Nodes.Select(x=>new{Rows=Convert.ToHexString(x.Records.Span),Primary=Convert.ToHexString(x.PrimaryAlternatives.Span)}),Winner=winner,Selected=selected,
        Backtrack=backtrack is null?null:new{Rows=Convert.ToHexString(backtrack.Rows),Visited=backtrack.Visited,Residuals=backtrack.Residuals}});
}
writer.WriteEndArray();writer.WriteStartArray("Budgets");
foreach(var b in doc.RootElement.GetProperty("Budgets").EnumerateArray())
{
    var p=LegacyEnglishGraphCompactBudget.FromCounts(b.GetProperty("Nodes").GetInt32(),b.GetProperty("Arcs").GetInt32(),b.GetProperty("Records").GetInt32());
    var payload=Boundary(p.AllocationBytes,false);var copies=Boundary(p.CopyBytes,true);
    JsonSerializer.Serialize(writer,new{p.RecordCapacity,p.AllocationBytes,p.CopyBytes,Payload=payload,Copies=copies});
}
writer.WriteEndArray();writer.WriteEndObject();
static object Boundary(long required,bool copy)
{
    var x=LegacyEnglishCaseNode0Ledger.Create(128L*1024*1024);
    long cap=copy?LegacyEnglishCaseNode0Ledger.CopyCeiling:LegacyEnglishCaseNode0Ledger.ContinuationCeiling;
    long current=copy?x.Copies:x.Payload;bool over=required>cap-current-(copy?16:0);
    if(over)
    {
        long before=copy?x.Copies:x.Payload;string? exception=null;
        try{if(copy)x.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,required);else x.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,required);}
        catch(InvalidDataException e){exception=e.GetType().FullName;}
        long after=copy?x.Copies:x.Payload;if(exception is null||before!=after)throw new Exception("Over-cap did not refuse before counters changed.");
        return new{OverCap=true,ExactAccepted=false,OneShortRefused=false,Before=before,After=after,Exception=exception};
    }
    long pad=cap-current-required-(copy?16:0);
    if(copy){x.Copy(LegacyEnglishCaseNode0Ledger.Site.Reserved,pad);x.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,required);}
    else{x.Allocate(LegacyEnglishCaseNode0Ledger.Site.Reserved,pad);x.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,required);}
    if((copy?x.Copies:x.Payload)!=cap)throw new Exception("Exact budget did not land at cap.");
    var y=LegacyEnglishCaseNode0Ledger.Create(128L*1024*1024);
    if(copy)y.Copy(LegacyEnglishCaseNode0Ledger.Site.Reserved,pad+1);else y.Allocate(LegacyEnglishCaseNode0Ledger.Site.Reserved,pad+1);
    long old=copy?y.Copies:y.Payload;string? error=null;
    try{if(copy)y.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,required);else y.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,required);}
    catch(InvalidDataException e){error=e.GetType().FullName;}
    long now=copy?y.Copies:y.Payload;if(error is null||old!=now)throw new Exception("One-short mutated counters or admitted.");
    return new{OverCap=false,ExactAccepted=true,OneShortRefused=true,Before=old,After=now,Exception=error};
}
