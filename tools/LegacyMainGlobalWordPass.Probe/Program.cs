using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
using System.Runtime.InteropServices;

if(args.Length!=9)throw new ArgumentException("AK engine separation case publication line30 line50 capture LK");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[8]),engine=File.ReadAllBytes(args[1]);
using var ed=JsonDocument.Parse(engine);var activation=ed.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),activation.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);byte[] line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
InkStroke M(float y)=>new(new InkPoint[]{new(0,y+90),new(0,y+10),new(25,y+60),new(50,y+10),new(50,y+90)});
InkStroke V(float y)=>new(new InkPoint[]{new(0,y+10),new(0,y+90)});
var mir=new InkStroke[]{M(0),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})};
var rows=new[]{V(0),new InkStroke(V(0).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray()),V(140),new InkStroke(V(140).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray())};
int owners=0,completed=0,refused=0,nodes=0;long comparedBytes=0;int comparedContexts=0,guardCalls=0,guardPositive=0,ownershipGuards=0;ulong identity=0x100000;
using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartArray();
foreach(var raw in new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows,Enumerable.Range(0,14).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray()})
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table);
    var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global);var source=LegacyFreshMainGlobalSourceRanges.Prepare(graph);var context=LegacyFreshMainGlobalContext.Prepare(ak,source,activations);var geometry=LegacyFreshMainGlobalGlyphGeometry.Prepare(ak,context);
    var owner=LegacyFreshMainGlobalCaseSession.Prepare(ak,lk,model,geometry,cd,pd);var cache=LegacyFreshMainGlobalTransitionCache.Prepare(ak,owner,table);var transitions=LegacyFreshMainGlobalWordTransitions.Prepare(ak,cache);var operations=transitions.CreateOperations();
    var costs=LegacyFreshMainGlobalGroupCosts.Prepare(ak,owner,table);var policy=LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak,owner,costs,transitions);
    int allocationCount=0;var pass=new LegacyFreshMainGlobalWordPass(ak,policy,Allocate);
    w.WriteStartObject();w.WriteNumber("owner",owners);w.WriteNumber("descriptorCount",cuts.Steps.Count(s=>!s.IsPendingSeparator));w.WriteNumber("nodes",graph.State.NodeCount);w.WriteNumber("primaryLimit",policy.Configuration.PrimaryArcLimit230a);
    try
    {
        var result=pass.RunOwned();
        var oracleSession=LegacyFreshMainGlobalCaseSession.Prepare(ak,lk,model,geometry,cd,pd);
        var oracleCosts=LegacyFreshMainGlobalGroupCosts.Prepare(ak,oracleSession,table);
        var oracleCache=LegacyFreshMainGlobalTransitionCache.Prepare(ak,oracleSession,table);
        var oracleTransitions=LegacyFreshMainGlobalWordTransitions.Prepare(ak,oracleCache);
        var oraclePolicy=LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak,oracleSession,oracleCosts,oracleTransitions);
        var expected=OriginalGlobalSchedulerOracle.Run(ak,oraclePolicy);
        if(expected.Count!=result.NodeCount)throw new Exception("Independent global scheduler node extent differs.");
        for(int n=0;n<result.NodeCount;n++)
        {
            var actual=result.GetNodeSnapshot(n);var e=expected[n];
            if((actual.EndingNode,actual.RootQueries,actual.ParentQueries,actual.FlagAllocations,actual.PrimaryCandidates,actual.LanguageRan,actual.LanguageQueries,actual.LanguageAllocations,actual.SourceEnd23f0,actual.RetainedEnd23f8,actual.CumulativePathCost2520,actual.AppliedPeriodicOffset)!=
                (e.EndingNode,e.RootQueries,e.ParentQueries,e.FlagAllocations,e.PrimaryCandidates,e.LanguageRan,e.LanguageQueries,e.LanguageAllocations,e.SourceEnd23f0,e.RetainedEnd23f8,e.CumulativePathCost2520,e.AppliedPeriodicOffset))throw new Exception($"Independent global node{n} scheduling/state differs.");
            Compare(actual.Records,e.Records,88,[8],owner.Contexts,oracleSession.Contexts);
            Compare(actual.PrimaryAlternatives,e.PrimaryAlternatives,40,[8,16],owner.Contexts,oracleSession.Contexts);
        }
        if(!ReferenceEquals(result.Graph,graph)||!ReferenceEquals(result.Policy,policy))throw new Exception("Global pass changed source identity.");
        Reject(()=>pass.RunOwned());
        Reject(()=>LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak,owner,oracleCosts,transitions));
        Reject(()=>LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak,owner,costs,oracleTransitions));
        byte[] badResource=ak.ToArray();badResource[0]^=1;Reject(()=>LegacyFreshMainGlobalWordControllerPolicy.Prepare(badResource,owner,costs,transitions));
        Reject(()=>new LegacyFreshMainGlobalWordPass(ak,policy,Allocate,new(true)));
        var snapshot=result.GetNodeSnapshot(0);if(!MemoryMarshal.TryGetArray(snapshot.Records,out var copied))throw new Exception("Portable snapshot missing backing array.");
        byte original=result.GetNodeSnapshot(0).Records.Span[0];copied.Array![copied.Offset]^=255;
        if(result.GetNodeSnapshot(0).Records.Span[0]!=original)throw new Exception("Public global snapshot changed private rows.");
        if(owners==0)
        {
            var checkSession=LegacyFreshMainGlobalCaseSession.Prepare(ak,lk,model,geometry,cd,pd);var checkCosts=LegacyFreshMainGlobalGroupCosts.Prepare(ak,checkSession,table);var checkCache=LegacyFreshMainGlobalTransitionCache.Prepare(ak,checkSession,table);var checkTransitions=LegacyFreshMainGlobalWordTransitions.Prepare(ak,checkCache);var checkPolicy=LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak,checkSession,checkCosts,checkTransitions);
            checkPolicy.Services.AdvanceNodeEpoch(-1,default);var lease=checkPolicy.Services.Prepare(0,0,default);
            _=LegacyNativeFirstDictionaryWordCompletion.ForGlobalSpan(checkPolicy,lease,default);_=LegacyNativeLaterDictionaryWordCompletion.ForGlobalSpan(checkPolicy,lease,default);
            Reject(()=>LegacyNativeFirstDictionaryWordCompletion.ForGlobalSpan(policy,lease,default));Reject(()=>LegacyNativeLaterDictionaryWordCompletion.ForGlobalSpan(policy,lease,default));
            Reject(()=>checkPolicy.Services.Read(lease,0,new(){Capacity=1,ContextRoot=owner.Contexts.ContainerIdentity},false,default));
            checkPolicy.Services.AdvanceNodeEpoch(0,default);
            Reject(()=>LegacyNativeFirstDictionaryWordCompletion.ForGlobalSpan(checkPolicy,lease,default));Reject(()=>LegacyNativeLaterDictionaryWordCompletion.ForGlobalSpan(checkPolicy,lease,default));
        }
        w.WriteString("status","completed");w.WriteStartArray("rows");
        for(int n=0;n<result.NodeCount;n++)
        {
            var r=result.GetNodeSnapshot(n);w.WriteStartObject();w.WriteNumber("node",n);w.WriteNumber("rootQueries",r.RootQueries);w.WriteNumber("parentQueries",r.ParentQueries);w.WriteNumber("flags",r.FlagAllocations);w.WriteNumber("primary",r.PrimaryCandidates);w.WriteBoolean("language",r.LanguageRan);w.WriteNumber("languageQueries",r.LanguageQueries);w.WriteNumber("languageAllocations",r.LanguageAllocations);w.WriteNumber("sourceEnd",r.SourceEnd23f0);w.WriteNumber("retainedEnd",r.RetainedEnd23f8);w.WriteString("records",Convert.ToHexString(r.Records.Span));w.WriteString("alternatives",Convert.ToHexString(r.PrimaryAlternatives.Span));w.WriteNumber("cumulative",r.CumulativePathCost2520);w.WriteEndObject();nodes++;
        }
        w.WriteEndArray();completed++;Console.WriteLine($"Actual global owner{owners} descriptors{cuts.Steps.Count(s=>!s.IsPendingSeparator)} nodes{result.NodeCount} PASS; NN{owner.AcousticEvaluations}.");
    }
    catch(NotSupportedException e)
    {w.WriteString("status","refused");w.WriteString("reason",e.Message);refused++;Console.WriteLine($"Actual global owner{owners} REFUSED: {e.Message}");}
    w.WriteStartArray("descriptors");foreach(var d in cuts.Steps.Where(d=>!d.IsPendingSeparator)){w.WriteStartObject();w.WriteNumber("ordinal",d.DescriptorOrdinal);w.WriteBase64String("raw",d.CopyDescriptor());w.WriteEndObject();}w.WriteEndArray();
    w.WriteStartArray("groupDescriptors");foreach(var g in graph.State.Groups)w.WriteNumberValue(g.DescriptorOrdinal);w.WriteEndArray();
    w.WriteStartArray("arcs");for(int n=0;n<graph.State.NodeCount;n++){w.WriteStartArray();foreach(var a in graph.State.GetNode(n))w.WriteNumberValue(a.GroupId);w.WriteEndArray();}w.WriteEndArray();
    var boundaryGuard=new LegacyGlobalOrdinaryWordPathGuard(policy.Services);w.WriteStartArray("markedGuards");
    for(int previous=0;previous<graph.State.NodeCount-1;previous++)for(ushort a=0;a<graph.State.GetNode(previous).Count;a++)foreach(byte kind in new byte[]{0,2})
    {
        byte[] bytes=new byte[80];BinaryPrimitives.WriteInt32LittleEndian(bytes,graph.State.NodeCount-1);BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4),previous);BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(14),a);bytes[28]=kind;bytes[60]=1;BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(68),policy.Services.Descriptor.Flags>>9&15);
        bool allowed=boundaryGuard.AllowsMarked(new(bytes,new(1,0)),default);w.WriteStartArray();w.WriteNumberValue(graph.State.NodeCount-1);w.WriteNumberValue(previous);w.WriteNumberValue(a);w.WriteNumberValue(kind);w.WriteBooleanValue(allowed);w.WriteEndArray();guardCalls++;if(allowed)guardPositive++;
    }w.WriteEndArray();
    w.WriteNumber("allocations",allocationCount);w.WriteNumber("acousticEvaluations",owner.AcousticEvaluations);w.WriteEndObject();owners++;
    LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
    {
        byte[] bytes=new byte[request.RequiredBytes];request.PreviousStorage.Span.CopyTo(bytes);allocationCount++;
        return new(bytes,++identity);
    }
}
w.WriteEndArray();w.Flush();Console.WriteLine($"DIAGNOSTIC {owners} actual global owners/{completed} completed/{refused} explicit refusals/{nodes} storednodes/{comparedBytes} independentlycomparedbytes/{comparedContexts} symbolicfields/{guardCalls} markedguards/{guardPositive} allowed/{ownershipGuards} ownershipguards; not frozen or recognized text.");
if(refused!=0)throw new Exception("Global pass still has reachable unsupported branches; do not freeze.");

void Compare(ReadOnlyMemory<byte> left,ReadOnlyMemory<byte> right,int stride,int[] offsets,LegacySymbolContextFactory lc,LegacySymbolContextFactory rc)
{
    byte[] a=left.ToArray(),b=right.ToArray();if(a.Length!=b.Length||a.Length%stride!=0)throw new Exception("Independent global storage extent differs.");
    for(int row=0;row<a.Length/stride;row++)foreach(int offset in offsets)
    {
        int at=row*stride+offset;ulong x=BinaryPrimitives.ReadUInt64LittleEndian(a.AsSpan(at)),y=BinaryPrimitives.ReadUInt64LittleEndian(b.AsSpan(at));
        if(!Path(lc,x).SequenceEqual(Path(rc,y)))throw new Exception("Independent global symbolic lineage differs.");
        a.AsSpan(at,8).Clear();b.AsSpan(at,8).Clear();comparedContexts++;
    }
    if(!a.AsSpan().SequenceEqual(b))throw new Exception("Independent global complete scalar storage differs.");comparedBytes+=a.Length;
}
static ushort[] Path(LegacySymbolContextFactory c,ulong id)
{
    var result=new List<ushort>();while(id!=0){if(result.Count>=4096)throw new Exception("Context cycle.");var row=c.ReadNode(id);result.Add(row.Symbol);id=row.ParentLink;}result.Reverse();return result.ToArray();
}

void Reject(Action action)
{
    try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){ownershipGuards++;return;}
    throw new Exception("Unsupported global controller ownership/reuse accepted.");
}
