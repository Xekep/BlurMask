using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;

if(args.Length!=9)throw new ArgumentException("AK LK engine separation line30 line50 case-data publication-data output.json required");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]),separation=File.ReadAllBytes(args[3]);
using var caseFile=File.OpenRead(args[6]);using var publicationFile=File.OpenRead(args[7]);
var line=MainLineWordFixture.Create(ak,engine,separation,File.ReadAllBytes(args[4]),File.ReadAllBytes(args[5]),
    LegacyNativeCaseData.Load(caseFile),LegacyNativeTextPublicationData.Load(publicationFile),15);
var owner=new LegacyOwnedMainWordSpanPreparation(ak,lk,engine,line.Source,line.Context,line.Table);
var policy=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,owner);
var allocations=new List<LegacyMainDirectStorageRequest>();ulong storageIdentity=1;
var pass=new LegacyOwnedMainDirectWordPass(ak,policy,owner.MainLineAcoustic,line.Table,Allocate);
var actual=pass.RunOwned();
var oracleOwner=new LegacyOwnedMainWordSpanPreparation(ak,lk,engine,line.Source,line.Context,line.Table);
var expected=OriginalDirectSchedulerOracle.Run(ak,oracleOwner,line.Table,lineSource:true);
Require(actual.NodeCount==60&&actual.NodeCount==owner.SourceGraph.NodeCount&&expected.Count==60,"Exact15-physical60-node source required.");
using var digest=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
int bytesCompared=0,contextFields=0,roots=0,parents=0,flags=0,lmQueries=0,lmAllocations=0,languageNodes=0;
var snapshots=new LegacyMainDirectWordNodeResult[actual.NodeCount];
foreach(int node in Enumerable.Range(0,actual.NodeCount))
{
    var got=actual.GetNodeSnapshot(node);var reference=expected[node];snapshots[node]=got;
    Require((got.EndingNode,got.RootQueries,got.ParentQueries,got.FlagAllocations,got.PrimaryCandidates,got.LanguageRan,
        got.LanguageQueries,got.LanguageAllocations,got.SourceEnd23f0,got.RetainedEnd23f8,got.CumulativePathCost2520,got.AppliedPeriodicOffset)==
        (reference.EndingNode,reference.RootQueries,reference.ParentQueries,reference.FlagAllocations,reference.PrimaryCandidates,reference.LanguageRan,
        reference.LanguageQueries,reference.LanguageAllocations,reference.SourceEnd23f0,reference.RetainedEnd23f8,reference.CumulativePathCost2520,reference.AppliedPeriodicOffset),
        "Independent scheduler counts/extents/cumulative state differ atnode"+node);
    Compare(got.Records,reference.Records,88,[8]);Compare(got.PrimaryAlternatives,reference.PrimaryAlternatives,40,[8,16]);
    Require(got.SourceEnd23f0>=policy.Configuration.PrimaryArcLimit230a&&got.RetainedEnd23f8<=got.SourceEnd23f0&&
        got.SourceEnd23f0<=got.Records.Length/88,"Native source-end/retained/allocation domains changed.");
    roots+=got.RootQueries;parents+=got.ParentQueries;flags+=got.FlagAllocations;lmQueries+=got.LanguageQueries;lmAllocations+=got.LanguageAllocations;
    if(got.LanguageRan)languageNodes++;
}
var periodic=snapshots.Single(row=>row.AppliedPeriodicOffset>0);
int firstAffected=49-Math.Min(50,owner.SourceGraph.MaximumAtoms);
Require(periodic.EndingNode==49&&firstAffected==41,"Actual firstperiodic backward extent changed.");
int offset49=periodic.AppliedPeriodicOffset;
int[] costs=actual.CopyCumulativePathCosts();
Require(costs.SequenceEqual(expected.Select(x=>x.CumulativePathCost2520))&&
    costs.Take(firstAffected).All(x=>x==0)&&costs.Skip(firstAffected).All(x=>x==offset49)&&
    snapshots.Skip(50).All(x=>x.AppliedPeriodicOffset==0&&x.CumulativePathCost2520==offset49),
    "Native node49 offset must apply49..41 andcopyforward through59.");
Require(languageNodes==15&&lmAllocations>0&&allocations.Count(x=>x.Kind==LegacyMainDirectStorageKind.HeapRecords)==1,
    "Complete language/singlepersistentdescriptor stress scope was not exercised.");
var invocation=LegacyOwnedFreshMainTerminalInvocation.Prepare(policy);
var terminal=invocation.SelectOwnedPass(actual);var last=snapshots[^1];
var literal=Scan(last,invocation.Options.PrimaryLimit230a);
Require(terminal.Winner==literal,"Independent first-invocation terminal scan differs.");
byte[] expectedChosen=literal.HasWinner?last.Records.Slice(literal.Ordinal*88,88).ToArray():[];
Require(terminal.CopySelectedRecord().AsSpan().SequenceEqual(expectedChosen),"Complete selected pre207CB0 row differs.");
int guards=0;
Reject(()=>pass.Run());Reject(()=>pass.RunOwned());
var foreignPolicy=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,owner);
Reject(()=>LegacyOwnedFreshMainTerminalInvocation.Prepare(foreignPolicy).SelectOwnedPass(actual));
Reject(()=>invocation.SelectOwnedPass(actual,new CancellationToken(true)));
costs[^1]=int.MinValue;Array.Fill(terminal.CopySelectedRecord(),(byte)0xaa);
Require(actual.CopyCumulativePathCosts()[^1]==offset49&&invocation.SelectOwnedPass(actual).Winner==literal&&
    terminal.CopySelectedRecord().AsSpan().SequenceEqual(expectedChosen),"Public result copies leaked mutable owned storage.");
using(var file=File.Create(args[8]))using(var w=new Utf8JsonWriter(file))
{
    w.WriteStartObject();w.WriteNumber("physicalCount",15);w.WriteNumber("nodes",actual.NodeCount);w.WriteNumber("groups",owner.SourceGraph.Groups.Count);
    w.WriteNumber("rootQueries",roots);w.WriteNumber("parentQueries",parents);w.WriteNumber("flagAllocations",flags);w.WriteNumber("languageQueries",lmQueries);w.WriteNumber("languageAllocations",lmAllocations);
    w.WriteNumber("bytesCompared",bytesCompared);w.WriteNumber("symbolicFieldsCompared",contextFields);w.WriteString("canonicalStorageSha256",Convert.ToHexString(digest.GetHashAndReset()).ToLowerInvariant());
    w.WriteNumber("periodicNode",49);w.WriteNumber("periodicFirstAffected",firstAffected);w.WriteNumber("periodicOffset",offset49);
    w.WriteStartArray("cumulative2520");foreach(int cost in actual.CopyCumulativePathCosts())w.WriteNumberValue(cost);w.WriteEndArray();
    w.WriteNumber("sourceEnd",last.SourceEnd23f0);w.WriteNumber("retainedEnd",last.RetainedEnd23f8);
    w.WriteBoolean("hasNumericWinner",literal.HasWinner);w.WriteNumber("winnerOrdinal",literal.Ordinal);w.WriteNumber("relativeCost",literal.RelativeCost);w.WriteNumber("totalCost",literal.TotalCost);
    w.WriteString("selectedPre207Cb0Record",Convert.ToHexString(expectedChosen));w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
}
Console.WriteLine($"PASS actual15 physical/{owner.SourceGraph.Groups.Count} NN groups/{actual.NodeCount} owning nodes: {bytesCompared} complete scalar storage bytes/{contextFields} symbolic context fields; {roots}root/{parents}parent/{flags}flag/{lmQueries}LM queries/{lmAllocations}LM allocations.");
Console.WriteLine($"PASS node49 offset{offset49} affects49..41 andcopies50..59; owned pre207CB0 ordinal{literal.Ordinal} relative{literal.RelativeCost}+cumulative{offset49}=total{literal.TotalCost}; {guards} lifecycle/owner/cancel guards. No expected text supplied.");

LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
{
    allocations.Add(request);byte[] storage=new byte[request.RequiredBytes];request.PreviousStorage.Span.CopyTo(storage);
    return new(storage,storageIdentity++);
}
void Compare(ReadOnlyMemory<byte> actualBytes,ReadOnlyMemory<byte> referenceBytes,int stride,int[] contextOffsets)
{
    byte[] got=actualBytes.ToArray(),wanted=referenceBytes.ToArray();
    Require(got.Length==wanted.Length&&got.Length%stride==0,"Full allocation extent differs.");
    Span<byte> scalar=stackalloc byte[4];
    for(int row=0;row<got.Length/stride;row++)foreach(int offset in contextOffsets)
    {
        int at=row*stride+offset;var a=Path(owner.Contexts,BinaryPrimitives.ReadUInt64LittleEndian(got.AsSpan(at)));
        var b=Path(oracleOwner.Contexts,BinaryPrimitives.ReadUInt64LittleEndian(wanted.AsSpan(at)));
        Require(a.AsSpan().SequenceEqual(b),"Independent symbolic owner/parent path differs.");
        BinaryPrimitives.WriteInt32LittleEndian(scalar,a.Length);digest.AppendData(scalar);
        foreach(ushort symbol in a){BinaryPrimitives.WriteInt32LittleEndian(scalar,symbol);digest.AppendData(scalar);}
        got.AsSpan(at,8).Clear();wanted.AsSpan(at,8).Clear();contextFields++;
    }
    Require(got.AsSpan().SequenceEqual(wanted),"Full stored row/alternative scalar bytes differ.");
    digest.AppendData(got);bytesCompared+=got.Length;
}
static ushort[] Path(LegacySymbolContextFactory contexts,ulong id)
{
    var symbols=new List<ushort>();while(id!=0)
    {Require(symbols.Count<4096,"Cyclic/oversized symbolic context.");var node=contexts.ReadNode(id);symbols.Add(node.Symbol);id=node.ParentLink;}
    symbols.Reverse();return symbols.ToArray();
}
static LegacyPreparedMainTerminalWinner Scan(LegacyMainDirectWordNodeResult node,ushort primary)
{
    int best=0x4000000,winner=-1,row14=0x4000000;
    for(int ordinal=0;ordinal<node.SourceEnd23f0;ordinal++)
    {
        var row=node.Records.Span.Slice(ordinal*88,88);if(ordinal<primary||(row[5]&4)!=0)continue;
        int cost=BinaryPrimitives.ReadInt32LittleEndian(row[16..]);if(cost>=best)continue;
        best=cost;winner=ordinal;row14=BinaryPrimitives.ReadInt32LittleEndian(row[20..]);
    }
    return winner<0?new(false,-1,0x4000000,0x4000000,0x4000000,uint.MaxValue,0,0):
        new(true,winner,best,unchecked(best+node.CumulativePathCost2520),row14,
            BinaryPrimitives.ReadUInt16LittleEndian(node.Records.Span[(winner*88+34)..]),0,0);
}
void Reject(Action action)
{try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Expected lifecycle/owner refusal.");}
static void Require(bool value,string message){if(!value)throw new InvalidDataException(message);}
