using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 5) throw new ArgumentException("controls.json AK LK engine.json separation.json required");
byte[] acoustic = File.ReadAllBytes(args[1]);
var profile = LegacyCursiveCategoryProfile.Load(acoustic.AsSpan(188222), 188222, out _);
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0, traces = 0, heapSteps = 0;
foreach (var test in document.RootElement.GetProperty("cases").EnumerateArray())
{
    byte[] current = Bytes(test, "current"), anchor = Bytes(test, "anchor"), residue = Bytes(test, "residue");
    var contexts = new LegacySymbolContextFactory();
    if (test.GetProperty("suffix").ValueKind != JsonValueKind.Null)
        BinaryPrimitives.WriteUInt64LittleEndian(anchor.AsSpan(8), contexts.ResolveInitial(test.GetProperty("suffix").GetUInt16()).Identity);
    var rows = test.GetProperty("rows").EnumerateArray().Select(r => Stored(Convert.FromHexString(r.GetString()!))).ToArray();
    uint[] headers = test.GetProperty("headers").EnumerateArray().Select(h => h.GetUInt32()).ToArray();
    var scripts = test.GetProperty("scripts").EnumerateArray().ToArray();
    var expectedTraces = test.GetProperty("traces").EnumerateArray().ToArray();
    int trace = 0;
    void Check(int kind, LegacyNativeWordRecombinationRequest request)
    {
        Require(trace < expectedTraces.Length, "Unexpected context callback.");
        var expected = expectedTraces[trace++];
        Require(expected.GetProperty("kind").GetInt32() == kind && request.GetSnapshot().Span.SequenceEqual(Bytes(expected, "bytes")) &&
            request.Cost.Value == expected.GetProperty("cell").GetInt32(), "Independent full80 context packet/cell/encounter order differs.");
        traces++;
    }
    JsonElement Script(LegacyNativeWordRecombinationRequest request) => scripts[request.ReadUInt16(16) - 100];
    var operations = new LegacyNativeHigherArcTransitionOperations((request, _) =>
    {
        Check(118, request);
        var script = Script(request);
        request.Cost.Value = unchecked(request.Cost.Value + script.GetProperty("d118").GetInt32());
        request.WriteByte(59, script.GetProperty("flag").GetByte());
        return script.GetProperty("a118").GetBoolean();
    }, (request, _) =>
    {
        Check(140, request);
        var script = Script(request);
        request.Cost.Value = unchecked(request.Cost.Value + script.GetProperty("d140").GetInt32());
        request.StorageForProbeIncrement(script.GetProperty("dinc").GetInt32());
        return script.GetProperty("a140").GetBoolean();
    }, (request, _) =>
    {
        Check(0, request);
        return Script(request).GetProperty("successors").GetBoolean() ? new ushort[] { 27 } : new ushort[] { 26 };
    });
    var policy = Policy(test.GetProperty("policy"));
    var result = LegacyNativeLanguageContextRecombination.Collect(current, 33,
        (node, ordinal) => { Require(node == BinaryPrimitives.ReadInt32LittleEndian(current.AsSpan(56)) && ordinal == 2, "Anchor ownership changed."); return Stored(anchor); },
        node => { Require(node == BinaryPrimitives.ReadInt32LittleEndian(current.AsSpan(56)) - (sbyte)anchor[32] - 1,
            "Previous-node ownership changed."); return rows; }, id => headers[id], profile, contexts, new(1, 2, 3, 4), policy, operations, 1001, 1002,
        residue, test.GetProperty("cap").GetInt32());
    Require(result.InitialGeometryAccepted == test.GetProperty("initial").GetBoolean() &&
        result.Candidates16.Span.SequenceEqual(Bytes(test, "output")) && result.LastRequest80.Span.SequenceEqual(Bytes(test, "last")) &&
        result.MatchedRows == test.GetProperty("matched").GetInt32() && result.GeometryCalls == test.GetProperty("calls").GetInt32() &&
        trace == expectedTraces.Length, "Independent context filtering, deltas, literal wraps, full16 heap or last request differs.");
    cases++;
}
foreach (var stream in document.RootElement.GetProperty("streams").EnumerateArray())
{
    var heap = new LegacyNativeLanguageContextCandidateHeap(stream.GetProperty("cap").GetInt32());
    foreach (var step in stream.GetProperty("steps").EnumerateArray())
    {
        bool accepted = heap.TryInsert(step.GetProperty("cost").GetInt32(), step.GetProperty("inc").GetInt32(),
            step.GetProperty("previous").GetUInt16(), step.GetProperty("delta").GetInt32(), step.GetProperty("current").GetUInt16());
        Require(accepted == step.GetProperty("accepted").GetBoolean() && heap.GetSnapshot().Span.SequenceEqual(Bytes(step, "output")),
            "Independent full16 heap movement, equal-root tie or impossible sentinel differs.");
        heapSteps++;
    }
}
Console.WriteLine($"PASS {cases} full80 context cases/{traces} exact packet/cell callbacks and1024 heap streams/{heapSteps} full16 transitions.");
int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new InvalidDataException("Invalid context/heap ownership accepted.");
}
var guardCase = document.RootElement.GetProperty("cases")[0];
byte[] guardCurrent = Bytes(guardCase, "current"), guardAnchor = Bytes(guardCase, "anchor"), guardResidue = Bytes(guardCase, "residue");
var guardRows = guardCase.GetProperty("rows").EnumerateArray().Select(row => Stored(Convert.FromHexString(row.GetString()!))).ToArray();
uint[] guardHeaders = guardCase.GetProperty("headers").EnumerateArray().Select(h => h.GetUInt32()).ToArray();
var guardPolicy = new LegacyNativeLanguageContextPolicy(false, 0, false, false, 0, 2, (ushort)guardRows.Length);
var guardOperations = new LegacyNativeHigherArcTransitionOperations((_, _) => true, (_, _) => true, (_, _) => new ushort[] { 27 });
LegacyNativeLanguageContextRecombinationResult Guard(byte[]? current = null, byte[]? anchor = null,
    LegacyNativeLanguageContextPolicy? policy = null, LegacyNativeHigherArcTransitionOperations? operations = null,
    ulong firstIdentity = 1001, ulong nextIdentity = 1002, int capacity = 8, byte[]? residue = null,
    LegacyNativeWordParentRowLookup? lookup = null, Func<byte, uint>? header = null,
    Func<int, IReadOnlyList<LegacyNativeStoredWordRecord>>? nodeLookup = null, CancellationToken token = default)
    => LegacyNativeLanguageContextRecombination.Collect(current ?? guardCurrent, 33,
        lookup ?? ((_, _) => Stored(anchor ?? guardAnchor)), nodeLookup ?? (_ => guardRows), header ?? (id => guardHeaders[id]),
        profile, new(), new(1, 2, 3, 4), policy ?? guardPolicy, operations ?? guardOperations,
        firstIdentity, nextIdentity, residue ?? guardResidue, capacity, token);
Reject(() => Guard(current: new byte[87]));
Reject(() => Guard(residue: new byte[79]));
Reject(() => Guard(firstIdentity: 0));
Reject(() => Guard(nextIdentity: 1001));
Reject(() => Guard(capacity: -1));
Reject(() => Guard(capacity: 65536));
Reject(() => Guard(policy: guardPolicy with { PrimaryLimit230a = 10 }));
Reject(() => Guard(policy: guardPolicy with { SecondaryEnd23f0 = 10 }));
var invalidAnchor = guardAnchor.ToArray();
BinaryPrimitives.WriteUInt16LittleEndian(invalidAnchor.AsSpan(30), ushort.MaxValue);
Reject(() => Guard(anchor: invalidAnchor));
invalidAnchor = guardAnchor.ToArray();
BinaryPrimitives.WriteUInt16LittleEndian(invalidAnchor.AsSpan(26), 63);
Reject(() => Guard(anchor: invalidAnchor));
Reject(() => Guard(lookup: (_, _) => null!));
Reject(() => Guard(nodeLookup: _ => null!));
Reject(() => Guard(operations: guardOperations with { Calculate118 = null! }));
for (int stage = 0; stage < 6; stage++)
{
    using var canceled = new CancellationTokenSource();
    int selectedStage = stage;
    var cancelOperations = new LegacyNativeHigherArcTransitionOperations((_, _) =>
    { if (selectedStage == 2) canceled.Cancel(); return true; }, (_, _) =>
    { if (selectedStage == 3) canceled.Cancel(); return true; }, (_, _) =>
    { if (selectedStage == 4) canceled.Cancel(); return new ushort[] { 27 }; });
    Reject(() => Guard(operations: cancelOperations, token: canceled.Token,
        lookup: (_, _) => { if (selectedStage == 0) canceled.Cancel(); return Stored(guardAnchor); },
        nodeLookup: _ => { if (selectedStage == 5) canceled.Cancel(); return guardRows; },
        header: id => { if (selectedStage == 1) canceled.Cancel(); return guardHeaders[id]; }));
}
for (int stage = 0; stage < 3; stage++)
{
    using var canceled = new CancellationTokenSource();
    int selectedStage = stage;
    var cancelFalseOperations = new LegacyNativeHigherArcTransitionOperations((_, _) =>
    { if (selectedStage == 0) { canceled.Cancel(); return false; } return true; }, (_, _) =>
    { if (selectedStage == 1) { canceled.Cancel(); return false; } return true; }, (_, _) =>
    { canceled.Cancel(); return ReadOnlyMemory<ushort>.Empty; });
    Reject(() => Guard(operations: cancelFalseOperations, token: canceled.Token));
}
Reject(() => new LegacyNativeLanguageContextCandidateHeap(-1));
Reject(() => new LegacyNativeLanguageContextCandidateHeap(65536));
Reject(() => new LegacyNativeLanguageContextCandidateHeap(0).TryInsert(0, 0, 0, 0, 0, new CancellationToken(true)));
var baseline = Guard();
byte[] mutableCurrent = guardCurrent.ToArray(), mutableResidue = guardResidue.ToArray();
var snapshotResult = Guard(current: mutableCurrent, residue: mutableResidue, lookup: (_, _) =>
{ Array.Clear(mutableCurrent); Array.Clear(mutableResidue); return Stored(guardAnchor); });
Require(baseline.Candidates16.Span.SequenceEqual(snapshotResult.Candidates16.Span) &&
    baseline.LastRequest80.Span.SequenceEqual(snapshotResult.LastRequest80.Span), "Context input/residue changed during owner callback.");
var mutableRows = guardCase.GetProperty("rows").EnumerateArray().Select(row => Stored(Convert.FromHexString(row.GetString()!))).ToArray();
bool mutateOnce = true;
var rowSnapshotResult = Guard(nodeLookup: _ => mutableRows, header: id =>
{
    if (mutateOnce)
    {
        mutateOnce = false;
        foreach (var row in mutableRows)
            if (System.Runtime.InteropServices.MemoryMarshal.TryGetArray(row.Record, out ArraySegment<byte> segment))
                Array.Clear(segment.Array!, segment.Offset, segment.Count);
    }
    return guardHeaders[id];
});
Require(baseline.Candidates16.Span.SequenceEqual(rowSnapshotResult.Candidates16.Span) &&
    baseline.LastRequest80.Span.SequenceEqual(rowSnapshotResult.LastRequest80.Span), "Previous-node row bytes changed during header callback.");
Console.WriteLine($"PASS {guards} structural/callback cancellation guards and current/residue ownership snapshots.");
ActualMainLanguageContext.Run(args);

static byte[] Bytes(JsonElement test, string property) => Convert.FromHexString(test.GetProperty(property).GetString()!);
static LegacyNativeStoredWordRecord Stored(byte[] row)
{
    byte[] slot = new byte[128];
    row.CopyTo(slot, 0);
    return LegacyNativeWordRowMaterializer.Materialize(slot, 0, 0, 1, 1);
}
static LegacyNativeLanguageContextPolicy Policy(JsonElement value) => new(value.GetProperty("count").GetBoolean(),
    value.GetProperty("limit").GetByte(), value.GetProperty("flag").GetBoolean(), value.GetProperty("reuse").GetBoolean(),
    value.GetProperty("permission").GetByte(), value.GetProperty("primary").GetUInt16(), value.GetProperty("end").GetUInt16());
static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }

internal static class RequestProbeExtensions
{
    public static void StorageForProbeIncrement(this LegacyNativeWordRecombinationRequest request, int delta)
    {
        byte[] output = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(output, unchecked(request.ReadInt32(48) + delta));
        for (int offset = 0; offset < 4; offset++) request.WriteByte(48 + offset, output[offset]);
    }
}
