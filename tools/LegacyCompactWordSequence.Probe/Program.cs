using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length is not (1 or 5)) throw new ArgumentException("controls [AK LK engine separation] required.");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int normalCount = 0, appendCount = 0, byteCount = 0, guards = 0;
foreach (var test in controls.RootElement.GetProperty("normalize").EnumerateArray())
{
    byte[] input = Convert.FromBase64String(test.GetProperty("input").GetString()!);
    byte[] original = (byte[])input.Clone();
    byte[] expected = Convert.FromBase64String(test.GetProperty("expected").GetString()!);
    byte[] actual = LegacyPreparedCompactWordSequence.Normalize(input, test.GetProperty("sentinels").GetBoolean());
    CheckBytes(actual, expected, $"normalization{normalCount}");
    Require(input.AsSpan().SequenceEqual(original), "Prepared normalization mutated its input.");
    normalCount++;
}
foreach (var test in controls.RootElement.GetProperty("append").EnumerateArray())
{
    var contexts = new LegacySymbolContextFactory();
    ulong current = Build(contexts, test.GetProperty("current").EnumerateArray().Select(item => item.GetUInt16()));
    ulong previous = Build(contexts, test.GetProperty("previous").EnumerateArray().Select(item => item.GetUInt16()));
    byte[] input = Convert.FromBase64String(test.GetProperty("input").GetString()!);
    byte[] original = (byte[])input.Clone();
    byte[] row = Convert.FromBase64String(test.GetProperty("row").GetString()!);
    byte[] actual = LegacyPreparedCompactWordSequence.AppendContextDifference(input, contexts, current, previous,
        row, test.GetProperty("node").GetUInt32(), test.GetProperty("knowledge").GetUInt16(),
        test.GetProperty("sentinels").GetBoolean(), test.GetProperty("special").GetUInt16());
    CheckBytes(actual, Convert.FromBase64String(test.GetProperty("expected").GetString()!), $"context{appendCount}");
    Require(input.AsSpan().SequenceEqual(original), "Prepared context extraction mutated its input.");
    appendCount++;
}

var empty = new byte[944];
var plainRow = new byte[88];
var own = new LegacySymbolContextFactory();
var foreign = new LegacySymbolContextFactory();
ulong foreignId = Build(foreign, new ushort[] { 1, 2 });
Reject(() => LegacyPreparedCompactWordSequence.Normalize(new byte[943], false));
Reject(() => LegacyPreparedCompactWordSequence.AppendContextDifference(empty, own, 0, 0, new byte[87], 0, 0, false, 0));
Reject(() => LegacyPreparedCompactWordSequence.AppendContextDifference(empty, own, foreignId, 0, plainRow, 0, 0, false, 0));
Reject(() => LegacyPreparedCompactWordSequence.AppendContextDifference(empty, own, 0, foreignId, plainRow, 0, 0, false, 0));
var excessive = (byte[])empty.Clone();
W32(excessive, 0, 51);
Reject(() => LegacyPreparedCompactWordSequence.Normalize(excessive, false));
excessive = (byte[])empty.Clone();
W32(excessive, 0x3a0, 20);
Reject(() => LegacyPreparedCompactWordSequence.Normalize(excessive, false));
excessive = (byte[])empty.Clone();
W32(excessive, 0x3a8, -1);
Reject(() => LegacyPreparedCompactWordSequence.Normalize(excessive, false));
excessive = (byte[])empty.Clone();
W32(excessive, 0, 50);
W32(excessive, 0x3a8, 1);
excessive[0x268] = 1;
Reject(() => LegacyPreparedCompactWordSequence.Normalize(excessive, false));
W32(excessive, 0x3a8, 0);
W32(excessive, 0x3a0, 2);
for (int index = 0; index < 50; index++) excessive[6 + index * 12] = 3;
Reject(() => LegacyPreparedCompactWordSequence.Normalize(excessive, false));
W32(excessive, 0x3a0, 0);
plainRow[5] = 128;
BinaryPrimitives.WriteUInt16LittleEndian(plainRow.AsSpan(24), 1);
Reject(() => LegacyPreparedCompactWordSequence.AppendContextDifference(excessive, own, 0, 0, plainRow, 0, 0, false, 0));
using (var canceled = new CancellationTokenSource())
{
    canceled.Cancel();
    Reject(() => LegacyPreparedCompactWordSequence.Normalize(empty, false, canceled.Token));
    Reject(() => LegacyPreparedCompactWordSequence.AppendContextDifference(empty, own, 0, 0, plainRow, 0, 0, false, 0, canceled.Token));
}

// Equal leaf symbols under different parents must not be treated as one
// context. This tests identity, not a string suffix/common-symbol heuristic.
var identityContexts = new LegacySymbolContextFactory();
ulong a = Build(identityContexts, new ushort[] { 4, 9 });
ulong b = Build(identityContexts, new ushort[] { 5, 9 });
var identityRow = new byte[88];
byte[] difference = LegacyPreparedCompactWordSequence.AppendContextDifference(empty, identityContexts, a, b,
    identityRow, 0, 0, false, 65535);
Require(I32(difference, 0) == 2 && U16(difference, 4) == 4 && U16(difference, 16) == 9,
    "Context difference confused equal symbols with equal context identities.");

if (args.Length == 5)
{
    byte[] ak = File.ReadAllBytes(args[1]), lk = File.ReadAllBytes(args[2]), engine = File.ReadAllBytes(args[3]);
    var fixture = MainFixture.Create(ak, lk, engine, File.ReadAllBytes(args[4]));
    var source = new LegacyOwnedMainWordSpanPreparation(ak, lk, engine, fixture.Sampled, fixture.Table);
    var policy = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, source);
    ulong id = 1;
    var pass = new LegacyOwnedMainDirectWordPass(ak, policy, fixture.Table, request =>
    {
        byte[] bytes = new byte[request.RequiredBytes];
        request.PreviousStorage.Span.CopyTo(bytes);
        return new(bytes, id++);
    });
    var result = pass.RunOwned();
    int actualContexts = 0, actualSymbols = 0;
    for (int node = 0; node < result.NodeCount; node++)
    {
        var snapshot = result.GetNodeSnapshot(node);
        for (int ordinal = 0; ordinal < snapshot.SourceEnd23f0; ordinal++)
        {
            byte[] row = snapshot.Records.Slice(ordinal * 88, 88).ToArray();
            ulong context = BinaryPrimitives.ReadUInt64LittleEndian(row.AsSpan(8));
            if (I32(row, 16) >= 0x4000000 || context == 0 ||
                U16(row, 24) != (uint)node && (sbyte)row[5] < 0) continue;
            var path = new List<ushort>();
            for (ulong cursor = context; cursor != 0; cursor = source.Contexts.ReadNode(cursor).ParentLink)
                path.Add(source.Contexts.ReadNode(cursor).Symbol);
            path.Reverse();
            if (path.Any(symbol => symbol >= 0xffeb)) continue;
            //Only context extraction is resource-owned here. The runtime
            //symbols remain explicit and are not reached on these rows.
            byte[] frame = LegacyPreparedCompactWordSequence.AppendContextDifference(empty, source.Contexts,
                context, 0, row, checked((uint)node), 65535, false, 65535);
            Require(I32(frame, 0) == path.Count, "Actual owned context extent differs.");
            for (int index = 0; index < path.Count; index++)
                Require(U16(frame, 4 + index * 12) == path[index] && frame[6 + index * 12] == 0 && I32(frame, 8 + index * 12) == -1,
                    "Actual resource context extraction changed symbol/kind/index.");
            byte[] normalized = LegacyPreparedCompactWordSequence.Normalize(frame, false);
            Require(I32(normalized, 0) == path.Count, "Actual ordinary zero-prefix-coordinate normalization changed count.");
            for (int index = 0; index < path.Count; index++)
                Require(U16(normalized, 4 + index * 12) == path[index] && I32(normalized, 8 + index * 12) == 0,
                    "Actual ordinary normalization changed symbol or coordinate index.");
            ulong prior = source.Contexts.ReadNode(context).ParentLink;
            byte[] suffix = LegacyPreparedCompactWordSequence.AppendContextDifference(empty, source.Contexts,
                context, prior, row, checked((uint)node), 65535, false, 65535);
            Require(I32(suffix, 0) == 1 && U16(suffix, 4) == path[^1], "Actual owned ancestor difference changed.");
            actualContexts++;
            actualSymbols += path.Count;
        }
    }
    Require(actualContexts > 0 && actualSymbols > 0, "Actual resource test must exercise nonempty owned contexts.");
    var terminal = LegacyOwnedFreshMainTerminalInvocation.Prepare(policy).SelectOwnedPass(result);
    Console.WriteLine($"PASS actual NN/DIRECT rows: {actualContexts} nonempty owned contexts/{actualSymbols} symbols, ancestor differences+normalization; selected row context may be empty ({BinaryPrimitives.ReadUInt64LittleEndian(terminal.CopySelectedRecord().AsSpan(8)) == 0}). Runtime symbols explicit; no207CB0 orchestration/text claim.");
}
Console.WriteLine($"PASS {normalCount} normalizations/{appendCount} context differences/{byteCount} whole944-byte oracle bytes +{guards} guards +equal-label/different-context control.");

void CheckBytes(byte[] actual, byte[] expected, string label)
{
    if (!actual.AsSpan().SequenceEqual(expected))
    {
        int first = Enumerable.Range(0, expected.Length).First(index => actual[index] != expected[index]);
        throw new InvalidDataException($"{label} differs at{first:X}:actual{actual[first]:X2}/expected{expected[first]:X2}");
    }
    byteCount += actual.Length;
}
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or OperationCanceledException)
    { guards++; return; }
    throw new InvalidDataException("Invalid prepared sequence input accepted.");
}
static ulong Build(LegacySymbolContextFactory contexts, IEnumerable<ushort> symbols)
{
    ulong current = 0;
    foreach (ushort symbol in symbols)
    {
        var next = current == 0 ? contexts.ResolveInitial(symbol) : contexts.ResolveChild(current, symbol);
        if (current != 0 && next.Created) contexts.LinkCreatedContext(next.Identity, current);
        current = next.Identity;
    }
    return current;
}
static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidDataException(message);
}
static int I32(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadInt32LittleEndian(bytes[offset..]);
static ushort U16(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..]);
static void W32(Span<byte> bytes, int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(bytes[offset..], value);
