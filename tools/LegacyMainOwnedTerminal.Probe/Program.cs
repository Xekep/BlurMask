using System.Buffers.Binary;
using System.Runtime.InteropServices;
using InkRecognition;

if (args.Length is not (4 or 8))
    throw new ArgumentException("AK LK engine separation [line30 line50 case-data publication-data] required.");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]);
byte[] engine = File.ReadAllBytes(args[2]), separation = File.ReadAllBytes(args[3]);
int controls = 0, guards = 0;
ulong identity = 1;
LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
{
    byte[] bytes = new byte[request.RequiredBytes];
    request.PreviousStorage.Span.CopyTo(bytes);
    return new(bytes, identity++);
}

var fixture = MainFixture.Create(ak, lk, engine, separation);
var owner = new LegacyOwnedMainWordSpanPreparation(ak, lk, engine, fixture.Sampled, fixture.Table);
var policy = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner);
var pass = new LegacyOwnedMainDirectWordPass(ak, policy, fixture.Table, Allocate);
var result = pass.RunOwned();
CheckResult(result, policy);
Reject(() => pass.Run());
Reject(() => pass.RunOwned());

var foreignPolicy = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner);
Reject(() => LegacyOwnedFreshMainTerminalInvocation.Prepare(foreignPolicy).SelectOwnedPass(result));
var foreignOwner = new LegacyOwnedMainWordSpanPreparation(ak, lk, engine, fixture.Sampled, fixture.Table);
var foreignSourcePolicy = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, foreignOwner);
Reject(() => LegacyOwnedFreshMainTerminalInvocation.Prepare(foreignSourcePolicy).SelectOwnedPass(result));
var invocation = LegacyOwnedFreshMainTerminalInvocation.Prepare(policy);
Reject(() => invocation.SelectOwnedPass(null!));
using (var canceled = new CancellationTokenSource())
{
    canceled.Cancel();
    Reject(() => invocation.SelectOwnedPass(result, canceled.Token));
    var retry = new LegacyOwnedMainDirectWordPass(ak, foreignSourcePolicy, fixture.Table, Allocate);
    Reject(() => retry.RunOwned(canceled.Token));
    // Cancellation before entry does not consume the shared single-use guard.
    var afterCancellation = retry.RunOwned();
    CheckResult(afterCancellation, foreignSourcePolicy);
    Reject(() => retry.Run());
}
Reject(() => result.GetNodeSnapshot(-1));
Reject(() => result.GetNodeSnapshot(result.NodeCount));

// The existing Run API also consumes the same guard; no owning result can be
// manufactured afterward from its externally writable record snapshots.
var oldOwner = new LegacyOwnedMainWordSpanPreparation(ak, lk, engine, fixture.Sampled, fixture.Table);
var oldPolicy = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, oldOwner);
var oldPass = new LegacyOwnedMainDirectWordPass(ak, oldPolicy, fixture.Table, Allocate);
Require(oldPass.Run().Count == 4, "Original Run extent changed.");
Reject(() => oldPass.RunOwned());

if (args.Length == 8)
{
    using var caseStream = File.OpenRead(args[6]);
    using var publicationStream = File.OpenRead(args[7]);
    var line = MainLineWordFixture.Create(ak, engine, separation, File.ReadAllBytes(args[4]), File.ReadAllBytes(args[5]),
        LegacyNativeCaseData.Load(caseStream), LegacyNativeTextPublicationData.Load(publicationStream));
    var lineOwner = new LegacyOwnedMainWordSpanPreparation(ak, lk, engine, line.Source, line.Context, line.Table);
    var linePolicy = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, lineOwner);
    var linePass = new LegacyOwnedMainDirectWordPass(ak, linePolicy, lineOwner.MainLineAcoustic, line.Table, Allocate);
    var lineResult = linePass.RunOwned();
    Require(lineResult.NodeCount == 56, "Actual multi-physical node count changed.");
    Require(lineResult.CopyCumulativePathCosts()[^1] == 346098, "Actual periodic offset was lost before terminal selection.");
    CheckResult(lineResult, linePolicy);
}
Console.WriteLine($"PASS {controls} actual owned terminal/copy controls and {guards} ownership/cancellation/lifecycle guards; complete selected88 bytes and2520 cost, no text/confidence inference.");

void CheckResult(LegacyOwnedMainDirectWordPassResult completed, LegacyOwnedFreshMainWordControllerPolicy actualPolicy)
{
    var terminal = LegacyOwnedFreshMainTerminalInvocation.Prepare(actualPolicy);
    var selected = terminal.SelectOwnedPass(completed);
    var last = completed.GetNodeSnapshot(completed.NodeCount - 1);
    var expected = ScanActualInvocation(last, terminal.Options);
    Require(selected.Winner == expected, "Independent actual-invocation scalar scan differs.");
    Require(ReferenceEquals(selected.PassResult, completed), "Terminal lost its producing pass identity.");
    byte[] expectedPrimary = last.PrimaryAlternatives.ToArray();
    var expectedRow = expected.HasWinner ? last.Records.Slice(expected.Ordinal * 88, 88).ToArray() : [];
    Require(selected.CopySelectedRecord().AsSpan().SequenceEqual(expectedRow), "Selected full88-byte row differs.");
    Require(terminal.SelectPreparedRows(last.Records.Span, last.SourceEnd23f0, last.CumulativePathCost2520) == expected,
        "Existing prepared selection API changed.");
    Require(last.SourceEnd23f0 >= last.RetainedEnd23f8, "Source and retained row extents were confused.");
    controls += 5;

    // ReadOnlyMemory is not immutable: callers can recover its backing array.
    // Every public snapshot must therefore own a fresh copy of both buffers.
    Require(MemoryMarshal.TryGetArray(last.Records, out ArraySegment<byte> recordArray) && recordArray.Array is not null,
        "Test needs accessible public snapshot backing storage.");
    Array.Fill(recordArray.Array!, (byte)0xa5);
    Require(MemoryMarshal.TryGetArray(last.PrimaryAlternatives, out ArraySegment<byte> primaryArray) && primaryArray.Array is not null,
        "Test needs accessible primary snapshot backing storage.");
    Array.Fill(primaryArray.Array!, (byte)0x5a);
    var cumulative = completed.CopyCumulativePathCosts();
    cumulative[^1] = int.MinValue;
    var chosen = selected.CopySelectedRecord();
    Array.Fill(chosen, (byte)0xcc);
    Require(terminal.SelectOwnedPass(completed).Winner == expected, "External snapshot mutation altered owned terminal selection.");
    Require(selected.CopySelectedRecord().AsSpan().SequenceEqual(expectedRow), "Chosen record getter leaked private backing storage.");
    var fresh = completed.GetNodeSnapshot(completed.NodeCount - 1);
    Require(fresh.CumulativePathCost2520 == last.CumulativePathCost2520 &&
        completed.CopyCumulativePathCosts()[^1] == fresh.CumulativePathCost2520,
        "Cumulative getter leaked private backing storage.");
    Require(expectedRow.Length == 0 || fresh.Records.Slice(expected.Ordinal * 88, 88).Span.SequenceEqual(expectedRow),
        "Node getter leaked private row storage.");
    Require(fresh.PrimaryAlternatives.Span.SequenceEqual(expectedPrimary), "Node getter leaked private alternative storage.");
    controls += 5;
    Console.WriteLine($"OWNED nodes{completed.NodeCount} ends{fresh.SourceEnd23f0}/{fresh.RetainedEnd23f8} winner{expected.Ordinal} relative{expected.RelativeCost} cumulative{fresh.CumulativePathCost2520} total{expected.TotalCost}");
}

LegacyPreparedMainTerminalWinner ScanActualInvocation(LegacyMainDirectWordNodeResult last,
    LegacyPreparedMainTerminalOptions options)
{
    // Derived owning invocation is Filter4=false, no sentinel or required flag,
    // zero initial result flag. Thus all inspected primary/fallback rows are
    // rejected, while secondary rows retain their literal cost, no penalty.
    Require(!options.Filter4 && !options.ContextSentinels24b8 && !options.RequireFlag27Bit0_6 &&
        options.InputResultFlag8 == 0, "Independent bounded oracle invocation scope changed.");
    int best = 0x4000000, ordinal = -1, cost14 = 0x4000000;
    for (int at = 0; at < last.SourceEnd23f0; at++)
    {
        var row = last.Records.Span.Slice(at * 88, 88);
        if (at < options.PrimaryLimit230a || (row[5] & 4) != 0) continue;
        int cost = BinaryPrimitives.ReadInt32LittleEndian(row[16..]);
        if (cost >= best) continue;
        best = cost;
        ordinal = at;
        cost14 = BinaryPrimitives.ReadInt32LittleEndian(row[20..]);
    }
    if (ordinal < 0) return new(false, -1, 0x4000000, 0x4000000, 0x4000000, uint.MaxValue, 0, 0);
    var chosen = last.Records.Span.Slice(ordinal * 88, 88);
    return new(true, ordinal, best, unchecked(best + last.CumulativePathCost2520), cost14,
        BinaryPrimitives.ReadUInt16LittleEndian(chosen[34..]), 0, 0);
}

void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or OperationCanceledException)
    { guards++; return; }
    throw new InvalidDataException("Invalid owned terminal/lifecycle input accepted.");
}
static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidDataException(message);
}
