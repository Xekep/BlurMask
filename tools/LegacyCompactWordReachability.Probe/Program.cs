using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length is not (5 or 9))
    throw new ArgumentException("AK LK engine separation output.json [line30 line50 case-data publication-data]");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]);
byte[] engine = File.ReadAllBytes(args[2]), tableBytes = File.ReadAllBytes(args[3]);
var fixture = MainFixture.Create(ak, lk, engine, tableBytes);
var owner = new LegacyOwnedMainWordSpanPreparation(ak, lk, engine, fixture.Sampled, fixture.Table);
var policy = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner);
ulong identity = 1;
LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
{
    byte[] bytes = new byte[request.RequiredBytes];
    request.PreviousStorage.Span.CopyTo(bytes);
    return new(bytes, identity++);
}
int guards = 0;
var completed = new LegacyOwnedMainDirectWordPass(ak, policy, fixture.Table, Allocate).RunOwned();
var terminal = LegacyOwnedFreshMainTerminalInvocation.Prepare(policy).SelectOwnedPass(completed);
var prepared = LegacyOwnedMainCompactWordReachability.Prepare(ak, terminal);
Require(prepared.FinalizedPrefixEnd == -1 && prepared.QueryChecks == 0 && prepared.BackendCalls == 0,
    "Four-node source must not invent an already-finalized prefix query.");
Require(ReferenceEquals(prepared.PassResult, completed), "Reachability lost its producing pass.");
CheckSnapshots(completed, prepared);

Reject(() => LegacyOwnedMainCompactWordReachability.Prepare(ak, null!));
Reject(() => LegacyOwnedMainCompactWordReachability.Prepare(ak.AsSpan(0, 100), terminal));
Reject(() => LegacyOwnedMainCompactWordReachability.Prepare(new byte[ak.Length], terminal));
var afterInvalidResource = LegacyOwnedMainCompactWordReachability.Prepare(ak, terminal);
CheckSnapshots(completed, afterInvalidResource);
Require(afterInvalidResource.CopySelectedNodes().AsSpan().SequenceEqual(prepared.CopySelectedNodes()),
    "Rejecting foreign resource bytes consumed or changed the producing query state.");
Reject(() => LegacyOwnedMainCompactWordReachability.Prepare(ak, terminal, new(true)));
Reject(() => prepared.GetNodeSnapshot(-1));
Reject(() => prepared.GetNodeSnapshot(completed.NodeCount));

using var output = File.Create(args[4]);
using var writer = new Utf8JsonWriter(output);
writer.WriteStartArray();
Capture(completed, prepared, policy.Configuration.PrimaryArcLimit230a);
if (args.Length == 9)
{
    using var caseFile = File.OpenRead(args[7]);
    using var publicationFile = File.OpenRead(args[8]);
    var line = MainLineWordFixture.Create(ak, engine, tableBytes,
        File.ReadAllBytes(args[5]), File.ReadAllBytes(args[6]),
        LegacyNativeCaseData.Load(caseFile), LegacyNativeTextPublicationData.Load(publicationFile));
    var lineOwner = new LegacyOwnedMainWordSpanPreparation(ak, lk, engine, line.Source, line.Context, line.Table);
    var linePolicy = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, lineOwner);
    var lineCompleted = new LegacyOwnedMainDirectWordPass(ak, linePolicy, lineOwner.MainLineAcoustic,
        line.Table, Allocate).RunOwned();
    var lineTerminal = LegacyOwnedFreshMainTerminalInvocation.Prepare(linePolicy).SelectOwnedPass(lineCompleted);
    var linePrepared = LegacyOwnedMainCompactWordReachability.Prepare(ak, lineTerminal);
    Require(lineCompleted.NodeCount == 56 && linePrepared.FinalizedPrefixEnd == 47,
        "Actual graph lookahead must own the56-node frontier47.");
    //This actual first-word-only scheduler has no older active cached-word
    //node. A nonempty finalized prefix does not itself imply a207780 call.
    CheckSnapshots(lineCompleted, linePrepared);
    Capture(lineCompleted, linePrepared, linePolicy.Configuration.PrimaryArcLimit230a);
}
writer.WriteEndArray();
writer.Flush();

//A result lease cannot silently replay permissions after the next sourceV80.
owner.AdvanceNodeEpoch(completed.NodeCount - 1);
Reject(() => LegacyOwnedMainCompactWordReachability.Prepare(ak, terminal));
Console.WriteLine($"PASS source-owned compact reachability, whole allocated88/40-byte storage/copy checks and {guards} source/cancellation guards. Compact publication remains separate.");

void CheckSnapshots(LegacyOwnedMainDirectWordPassResult input, LegacyOwnedMainCompactWordReachability marked)
{
    int beamMarks = 0, alternativeMarks = 0;
    for (int node = 0; node < input.NodeCount; node++)
    {
        var original = input.GetNodeSnapshot(node);
        var actual = marked.GetNodeSnapshot(node);
        Require(original.SourceEnd23f0 == actual.SourceEnd23f0 && original.RetainedEnd23f8 == actual.RetainedEnd23f8,
            "Reachability changed source/retained extents.");
        byte[] records = actual.Records.ToArray(), alternatives = actual.PrimaryAlternatives.ToArray();
        for (int at = 0; at < records.Length; at++)
        {
            byte before = original.Records.Span[at];
            if (at % 88 == 6 && at / 88 < original.SourceEnd23f0)
            {
                Require((before & 0xfd) == (records[at] & 0xfd), "Beam marking changed unrelated bits.");
                if ((records[at] & 2) != 0) beamMarks++;
            }
            else Require(before == records[at], "Beam marking changed an unassigned byte.");
        }
        for (int at = 0; at < alternatives.Length; at++)
        {
            byte before = original.PrimaryAlternatives.Span[at];
            if (at % 40 == 0x1b)
            {
                Require((before & 0x7f) == (alternatives[at] & 0x7f), "Alternative marking changed its parent ordinal.");
                if ((alternatives[at] & 0x80) != 0) alternativeMarks++;
            }
            else Require(before == alternatives[at], "Alternative marking changed an unassigned byte.");
        }
        //Every snapshot getter owns a fresh backing buffer.
        Array.Clear(records); Array.Clear(alternatives);
        var current = marked.GetNodeSnapshot(node);
        Require(current.Records.Span.SequenceEqual(actual.Records.Span) &&
            current.PrimaryAlternatives.Span.SequenceEqual(actual.PrimaryAlternatives.Span), "Snapshot storage leaked.");
    }
    Require(beamMarks > 0 && alternativeMarks > 0, "Actual source produced no marked predecessor paths.");
    byte[] selection = marked.CopySelectedNodes();
    Array.Clear(selection);
    Require(marked.CopySelectedNodes()[^1] == 1, "Node-selection getter leaked private storage.");
    Console.WriteLine($"ACTUAL {input.NodeCount} nodes: {beamMarks} beam marks/{alternativeMarks} alternatives, {marked.QueryChecks} query checks/{marked.BackendCalls} backend calls.");
}
void Capture(LegacyOwnedMainDirectWordPassResult input, LegacyOwnedMainCompactWordReachability marked, int primary)
{
    writer.WriteStartObject(); writer.WriteNumber("primary", primary);
    writer.WriteNumber("prefixEnd", marked.FinalizedPrefixEnd);
    writer.WriteNumber("checks", marked.QueryChecks); writer.WriteNumber("backendCalls", marked.BackendCalls);
    writer.WriteBase64String("selection", marked.CopySelectedNodes()); writer.WriteStartArray("nodes");
    for (int node = 0; node < input.NodeCount; node++)
    {
        var original = input.GetNodeSnapshot(node); var actual = marked.GetNodeSnapshot(node);
        writer.WriteStartObject(); writer.WriteNumber("sourceEnd", original.SourceEnd23f0);
        writer.WriteNumber("retainedEnd", original.RetainedEnd23f8);
        writer.WriteBase64String("inputRows", original.Records.Span);
        writer.WriteBase64String("inputAlternatives", original.PrimaryAlternatives.Span);
        writer.WriteBase64String("rows", actual.Records.Span);
        writer.WriteBase64String("alternatives", actual.PrimaryAlternatives.Span); writer.WriteEndObject();
    }
    writer.WriteEndArray(); writer.WriteEndObject();
}
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new InvalidDataException("Unsupported reachability/lease scope accepted.");
}
static void Require(bool value, string message)
{ if (!value) throw new InvalidDataException(message); }
