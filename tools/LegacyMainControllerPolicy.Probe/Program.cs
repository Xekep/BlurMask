using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 5) throw new ArgumentException("controls AK LK engine separation required");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int count = 0;
foreach (var row in controls.RootElement.EnumerateArray())
{
    var input = row.GetProperty("input").EnumerateArray().ToArray();
    var expected = row.GetProperty("expected").EnumerateArray().ToArray();
    var result = LegacyNativeWordScalarRefresh.Apply(input[0].GetByte(), input[1].GetUInt32(),
        input[2].GetBoolean(), input[3].GetByte(), input[4].GetByte(), input[5].GetByte(),
        input[6].GetInt32(), input[7].GetByte(), input[8].GetBoolean());
    Require(result == new LegacyNativeWordScalarRefreshResult(expected[0].GetUInt32(),
        expected[1].GetByte(), expected[2].GetByte(), expected[3].GetByte(), expected[4].GetInt32(),
        expected[5].GetByte()), "Independent20C8C0 scalar stores differ.");
    count++;
}
Console.WriteLine($"PASS{count} independent mode/packed-bit/associated/source-byte/signed-limit scalar controls.");

byte[] ak = File.ReadAllBytes(args[1]);
byte[] lk = File.ReadAllBytes(args[2]);
byte[] engine = File.ReadAllBytes(args[3]);
byte[] separation = File.ReadAllBytes(args[4]);
var fixture = MainFixture.Create(ak, lk, engine, separation);
var owner = new LegacyOwnedMainWordSpanPreparation(ak, lk, engine, fixture.Sampled, fixture.Table);
var policy = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner);
Require(ReferenceEquals(policy.Owner, owner), "Policy lost its actual source/session owner.");
var config = policy.Configuration;
Require(policy.MaximumParentDistance54 == 120 && policy.FlagPrepassDistance58 == 20 &&
    policy.Refresh.MaximumWordLength68 == 120 && config.HeaderCount2400 == 2 &&
    config.PrimaryAlternativeCount2402 == 2 && config.SecondaryAlternativeCount2404 == 1 &&
    config.PrimaryArcLimit230a == 125 && config.Mode22f6 == 0 && config.PackedSource22e8 == 2 &&
    config.SuppressPreviousContext2609 == 0 && config.Trace2368 == 0 &&
    policy.Refresh.ConsecutiveBlank22f7 == 0 && policy.Refresh.SourceFormat22f9 == 0 &&
    policy.Refresh.KnowledgeOption22f8 == 0 && !policy.KnowledgeF5 && policy.SecondaryScaleEc == 0,
    "Actual247.V48 ->151BF0 ->20C8C0 ->2144F0 scalar preparation differs.");

foreach (var row in controls.RootElement.EnumerateArray())
{
    var parent = row.GetProperty("parent").EnumerateArray().ToArray();
    Require(policy.AllowsParent(unchecked((int)parent[0].GetUInt32()), unchecked((int)parent[1].GetUInt32()),
        parent[2].GetByte()) == row.GetProperty("allowed").GetBoolean(),
        "Independent signed/wrapped201270 parent distance differs.");
}

//No synthetic query/filter/mask: consume every original root page with the
//selected source's real22F7 and EC. Limits125 and query storage128 are distinct.
var graph = owner.Source.SourceRanges.Graph;
var cell = new LegacyNativeWordTransitionCostCell(0x8877665544332211, 0);
var initial = new LegacyNativeMainInitialSpecialCandidates(ak, owner, cell, new byte[80]);
int emitted = 0, queries = 0, parentChecks = 0;
for (int node = 0; node < graph.NodeCount; node++)
{
    if (owner.GetSpanExtent(node) <= node) continue;
    var span = owner.Prepare(node, checked((byte)node));
    var query = new LegacyPreparedDictionaryQuery { Node = 0, Capacity = 128,
        Parameter1c = (uint)node, Parameter20 = (uint)node, ContextRoot = owner.Contexts.ContainerIdentity };
    var page = owner.Read(span, 8UL << 40, query, clearExtraCosts: true);
    var heap = new LegacyNativeWordRecordHeap(config.PrimaryArcLimit230a);
    var router = new LegacyNativeWordHeapRouter([heap, new(0)], new byte[] { 0, 0 },
        config.SuppressPreviousContext2609, config.OverrideHeap2458);
    var result = initial.ProcessPreparedPage(span, page.Records.Span, 0, router,
        policy.SecondaryScaleEc, policy.Refresh.ConsecutiveBlank22f7 != 0, config.SuppressPreviousContext2609 != 0);
    Require(result.VisitedQueries == page.ReturnedCount, "Actual full source page was filtered before its native consumer.");
    for (int ordinal = 0; ordinal < heap.Count; ordinal++)
    {
        var record = heap.GetSlotSnapshot(ordinal);
        int anchor = BinaryPrimitives.ReadInt32LittleEndian(record.Span[0x38..]);
        int current = node + 1;
        bool expected = unchecked((sbyte)record.Span[0x21]) >= 0 && unchecked(current - anchor) <= 120;
        Require(policy.AllowsParent(current, anchor, record.Span[0x21]) == expected,
            "Actual complete initial128/native54 guard differs.");
        parentChecks++;
    }
    queries += page.ReturnedCount;
    emitted += result.PrimaryAllocations;
}
Require(queries == 39 && emitted == 43 && parentChecks == 43, "Full actual source root/NN candidate regression changed.");
Console.WriteLine($"PASS actual selected main54=120/58=20/68=120, limits2/2/1 and125; {queries} full source queries ->{emitted} owned NN/native128 rows ->{parentChecks} real parent guards. DIRECT branch; full scheduler/decorators remain separate.");

int guards = 0;
Reject(() => LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, null!)); guards++;
Reject(() => LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner, new CancellationToken(true))); guards++;
byte[] foreign = ak.ToArray(); foreign[0] ^= 1;
Reject(() => LegacyOwnedFreshMainWordControllerPolicy.Prepare(foreign, owner)); guards++;
Reject(() => LegacyOwnedFreshMainWordControllerPolicy.Prepare([], owner)); guards++;
Console.WriteLine($"PASS{guards} cancellation/null/foreign/truncated resource guards.");

static void Require(bool condition, string message)
{ if (!condition) throw new InvalidDataException(message); }
static void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or OperationCanceledException)
    { return; }
    throw new InvalidDataException("Expected owning policy refusal.");
}
