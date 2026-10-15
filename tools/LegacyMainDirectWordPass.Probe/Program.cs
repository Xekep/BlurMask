using System.Buffers.Binary;
using InkRecognition;

if (args.Length is not (4 or 8)) throw new ArgumentException("AK LK engine separation [line30 line50 case-data publication-data] required");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]);
var fixture = MainFixture.Create(ak, lk, File.ReadAllBytes(args[2]), File.ReadAllBytes(args[3]));
var owner = new LegacyOwnedMainWordSpanPreparation(ak, lk, File.ReadAllBytes(args[2]), fixture.Sampled, fixture.Table);
var policy = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner);
var allocations = new List<LegacyMainDirectStorageRequest>();
ulong identity = 10;
LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
{
    allocations.Add(request);
    byte[] bytes = new byte[request.RequiredBytes];
    request.PreviousStorage.Span.CopyTo(bytes);
    return new(bytes, identity++);
}
var pass = new LegacyOwnedMainDirectWordPass(ak, policy, fixture.Table, Allocate);
var nodes = pass.Run();
var oracleOwner = new LegacyOwnedMainWordSpanPreparation(ak, lk, File.ReadAllBytes(args[2]), fixture.Sampled, fixture.Table);
var expectedNodes = OriginalDirectSchedulerOracle.Run(ak, oracleOwner, fixture.Table);
Require(nodes.Count == owner.SourceGraph.NodeCount && nodes.Count == 4, "Owning main graph extent changed.");
int comparedBytes = 0, comparedContexts = 0;
foreach (var node in nodes)
{
    var expected = expectedNodes[node.EndingNode];
    Require((node.RootQueries, node.ParentQueries, node.FlagAllocations, node.PrimaryCandidates,
        node.LanguageRan, node.LanguageQueries, node.LanguageAllocations, node.SourceEnd23f0, node.RetainedEnd23f8) ==
        (expected.RootQueries, expected.ParentQueries, expected.FlagAllocations, expected.PrimaryCandidates,
        expected.LanguageRan, expected.LanguageQueries, expected.LanguageAllocations, expected.SourceEnd23f0, expected.RetainedEnd23f8),
        "Original native procedural scheduler step/count oracle differs.");
    Require(node.CumulativePathCost2520 == expected.CumulativePathCost2520 &&
        node.AppliedPeriodicOffset == expected.AppliedPeriodicOffset, "Original cumulative/periodic controller state differs.");
    CompareStorage(node.Records, expected.Records, 88, [8]);
    CompareStorage(node.PrimaryAlternatives, expected.PrimaryAlternatives, 40, [8, 16]);
    Require(node.Records.Length == 156 * 88 && node.PrimaryAlternatives.Length == 125 * 80 &&
        node.SourceEnd23f0 >= 125 && node.RetainedEnd23f8 <= node.SourceEnd23f0,
        "Direct loop lost allocated primary sentinels or separate source/retained counts.");
    int finite = 0;
    for (int ordinal = 0; ordinal < 125; ordinal++)
        if (BinaryPrimitives.ReadInt32LittleEndian(node.Records.Span[(ordinal * 88 + 16)..]) < 0x4000000) finite++;
    Require(finite == node.PrimaryCandidates, "Source125 extent was confused with finite primary count.");
    Console.WriteLine($"NODE{node.EndingNode} root{node.RootQueries} parent{node.ParentQueries} flags{node.FlagAllocations} primary{node.PrimaryCandidates} language{node.LanguageRan}/{node.LanguageQueries}/{node.LanguageAllocations} ends{node.SourceEnd23f0}/{node.RetainedEnd23f8}");
}
Require(allocations.Count(request => request.Kind == LegacyMainDirectStorageKind.HeapRecords) == 1,
    "Per-node reset discarded the retained native descriptor backing allocation.");
int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new InvalidDataException("Invalid owning main pass accepted.");
}
Reject(() => pass.Run());
Reject(() => new LegacyOwnedMainDirectWordPass(ak, policy, fixture.Table,
    request => new(new byte[request.RequiredBytes + 1], 1)));
using (var canceled = new CancellationTokenSource())
{
    Reject(() => new LegacyOwnedMainDirectWordPass(ak, policy, fixture.Table,
        request => { canceled.Cancel(); return new(new byte[request.RequiredBytes], 1); }, canceled.Token));
}
using (var canceled = new CancellationTokenSource())
{
    var canceledOwner = new LegacyOwnedMainWordSpanPreparation(ak, lk, File.ReadAllBytes(args[2]), fixture.Sampled, fixture.Table);
    var canceledPolicy = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, canceledOwner);
    var canceledPass = new LegacyOwnedMainDirectWordPass(ak, canceledPolicy, fixture.Table, request =>
    {
        byte[] bytes = new byte[request.RequiredBytes]; request.PreviousStorage.Span.CopyTo(bytes);
        if (request.Kind == LegacyMainDirectStorageKind.NodeRecords) canceled.Cancel();
        return new(bytes, 71);
    }, canceled.Token);
    Reject(() => canceledPass.Run(canceled.Token));
    Reject(() => canceledPass.Run());
}
Console.WriteLine($"PASS owning ordered main DIRECT loop over{nodes.Count} actual nodes/{nodes.Sum(node => node.RootQueries)} root/{nodes.Sum(node => node.ParentQueries)} parent queries; independent procedural oracle {comparedBytes} full storage bytes/{comparedContexts} symbolic owner fields; persistent descriptor+290 caches and {guards} ownership/cancel guards. Logical allocation remains explicit; no first-proposal/final-winner/acceptance claim.");

if (args.Length == 8)
{
    using var caseStream = File.OpenRead(args[6]);
    using var publicationStream = File.OpenRead(args[7]);
    var caseData = LegacyNativeCaseData.Load(caseStream);
    var publicationData = LegacyNativeTextPublicationData.Load(publicationStream);
    const int physicalCount = 14;
    var line = MainLineWordFixture.Create(ak, File.ReadAllBytes(args[2]), File.ReadAllBytes(args[3]),
        File.ReadAllBytes(args[4]), File.ReadAllBytes(args[5]), caseData, publicationData, physicalCount);
    var lineOwner = new LegacyOwnedMainWordSpanPreparation(ak, lk, File.ReadAllBytes(args[2]), line.Source, line.Context, line.Table);
    var linePolicy = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, lineOwner);
    var linePass = new LegacyOwnedMainDirectWordPass(ak, linePolicy, lineOwner.MainLineAcoustic, line.Table, Allocate);
    var lineNodes = linePass.Run();
    var lineOracleOwner = new LegacyOwnedMainWordSpanPreparation(ak, lk, File.ReadAllBytes(args[2]), line.Source, line.Context, line.Table);
    var lineExpected = OriginalDirectSchedulerOracle.Run(ak, lineOracleOwner, line.Table, lineSource: true);
    for (int node = 0; node < lineNodes.Count; node++)
    {
        var actual = lineNodes[node]; var expected = lineExpected[node];
        Require((actual.RootQueries, actual.ParentQueries, actual.FlagAllocations, actual.PrimaryCandidates,
            actual.LanguageRan, actual.LanguageQueries, actual.LanguageAllocations, actual.SourceEnd23f0, actual.RetainedEnd23f8,
            actual.CumulativePathCost2520, actual.AppliedPeriodicOffset) ==
            (expected.RootQueries, expected.ParentQueries, expected.FlagAllocations, expected.PrimaryCandidates,
            expected.LanguageRan, expected.LanguageQueries, expected.LanguageAllocations, expected.SourceEnd23f0, expected.RetainedEnd23f8,
            expected.CumulativePathCost2520, expected.AppliedPeriodicOffset), "Full multi-physical independent scheduler/periodic state differs.");
        CompareStorage(actual.Records, expected.Records, 88, [8], lineOwner.Contexts, lineOracleOwner.Contexts);
        CompareStorage(actual.PrimaryAlternatives, expected.PrimaryAlternatives, 40, [8, 16], lineOwner.Contexts, lineOracleOwner.Contexts);
    }
    Require(lineNodes.Count == physicalCount * 4 && lineNodes.Count(node => node.LanguageRan) == physicalCount &&
        lineNodes[^1].LanguageAllocations > 0, "Actual multi-physical pass did not consume its complete source extent.");
    if (physicalCount == 14) Require(line.Source.SourceRanges.Graph.Groups.Count == 420, "Actual fourteen-physical group resource changed.");
    Require(lineNodes.All(node => node.SourceEnd23f0 is >= 125 and <= 156 && node.RetainedEnd23f8 <= node.SourceEnd23f0),
        "Multi-physical loop lost source/retained node extents.");
    var periodic = lineNodes.Single(node => node.AppliedPeriodicOffset > 0);
    Require(periodic.EndingNode == 49 && lineNodes.Take(41).All(node => node.CumulativePathCost2520 == 0) &&
        lineNodes.Skip(41).All(node => node.CumulativePathCost2520 == periodic.AppliedPeriodicOffset),
        "Actual50-node normalization must own precisely nodes49..41 plus copied future cumulative values.");
    Console.WriteLine($"PASS actual{physicalCount} physical/{line.Source.SourceRanges.Graph.Groups.Count} NN groups/{lineNodes.Count} ordered nodes, {lineNodes.Sum(node => node.RootQueries)} root/{lineNodes.Sum(node => node.ParentQueries)} parent queries, {lineNodes.Sum(node => node.FlagAllocations)} flag allocations, {lineNodes.Sum(node => node.LanguageQueries)} LM queries/{lineNodes.Sum(node => node.LanguageAllocations)} allocations; last{lineNodes[^1].SourceEnd23f0}/{lineNodes[^1].RetainedEnd23f8}, node49offset{periodic.AppliedPeriodicOffset}; full independent storage oracle cumulative{comparedBytes} bytes/{comparedContexts} context fields. No expected text or winner supplied.");
}

void CompareStorage(ReadOnlyMemory<byte> actualMemory, ReadOnlyMemory<byte> expectedMemory, int stride, int[] contextOffsets,
    LegacySymbolContextFactory? actualContexts = null, LegacySymbolContextFactory? expectedContexts = null)
{
    byte[] actual = actualMemory.ToArray(), expected = expectedMemory.ToArray();
    Require(actual.Length == expected.Length && actual.Length % stride == 0, "Oracle allocation extent differs.");
    for (int row = 0; row < actual.Length / stride; row++)
        foreach (int offset in contextOffsets)
        {
            int at = row * stride + offset;
            ulong actualIdentity = BinaryPrimitives.ReadUInt64LittleEndian(actual.AsSpan(at));
            ulong expectedIdentity = BinaryPrimitives.ReadUInt64LittleEndian(expected.AsSpan(at));
            Require(Path(actualContexts ?? owner.Contexts, actualIdentity).SequenceEqual(Path(expectedContexts ?? oracleOwner.Contexts, expectedIdentity)),
                "Actual/oracle symbolic lineage differs across separately owned context factories.");
            actual.AsSpan(at, 8).Clear(); expected.AsSpan(at, 8).Clear(); comparedContexts++;
        }
    Require(actual.AsSpan().SequenceEqual(expected), "Complete main stored-row/alternative scalar bytes differ from original scheduler oracle.");
    comparedBytes += actual.Length;
}

static ushort[] Path(LegacySymbolContextFactory contexts, ulong identity)
{
    var result = new List<ushort>();
    while (identity != 0)
    {
        Require(result.Count < 4096, "Unexpected cyclic context in actual result.");
        var node = contexts.ReadNode(identity); result.Add(node.Symbol); identity = node.ParentLink;
    }
    result.Reverse(); return result.ToArray();
}

static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
