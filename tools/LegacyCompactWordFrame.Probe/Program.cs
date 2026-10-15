using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length is not (1 or 5)) throw new ArgumentException("controls [AK LK engine separation] required.");
using var parsed = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int controls = 0, guards = 0, bytesChecked = 0;
foreach (var test in parsed.RootElement.EnumerateArray())
{
    var contexts = new LegacySymbolContextFactory();
    ulong current = Build(contexts, test.GetProperty("current"));
    ulong previous = Build(contexts, test.GetProperty("previous"));
    var policy = new LegacyPreparedCompactWordContextPolicy(test.GetProperty("associated").GetBoolean(),
        test.GetProperty("quotes").GetBoolean(), test.GetProperty("sentinels").GetBoolean(),
        test.GetProperty("apostrophe").GetUInt16(), test.GetProperty("quote").GetUInt16(),
        test.GetProperty("special1").GetUInt16(), test.GetProperty("special2").GetUInt16(),
        test.GetProperty("knowledge").GetUInt16(), test.GetProperty("specialde").GetUInt16());
    var calls = new List<ushort>();
    byte Category(ushort symbol)
    {
        calls.Add(symbol);
        return test.GetProperty(symbol == policy.Special251c ? "category1" : "category2").GetByte();
    }
    byte[] row = Convert.FromBase64String(test.GetProperty("row").GetString()!);
    byte[] originalRow = (byte[])row.Clone();
    var actual = LegacyPreparedCompactWordFrameProducer.CreateOrdinary(contexts, current, previous,
        row, test.GetProperty("node").GetUInt32(), test.GetProperty("glyph").GetByte(), policy, Category);
    byte[] expected = Convert.FromBase64String(test.GetProperty("expected").GetString()!);
    byte[] frame = actual.CopyFrame();
    Require(frame.AsSpan().SequenceEqual(expected), $"Full2075E0 frame{controls} differs.");
    Require(calls.SequenceEqual(test.GetProperty("calls").EnumerateArray().Select(value => value.GetUInt16())), "207420 category callback order differs.");
    Require(row.AsSpan().SequenceEqual(originalRow), "Frame producer mutated source row.");
    Require(ReferenceEquals(actual.ContextOwner, contexts), "Frame lost source context factory identity.");
    Require(actual.RewrittenCurrentContext == Build(contexts, test.GetProperty("rewritten")), "Context rewrite identity differs.");
    Array.Fill(frame, (byte)0xcc);
    Require(actual.CopyFrame().AsSpan().SequenceEqual(expected), "Public frame copy leaks private backing storage.");
    bytesChecked += expected.Length;
    controls++;
}

var emptyRow = new byte[88];
var prepared = new LegacyPreparedCompactWordContextPolicy(true, true, false, 10, 11, 193, 194, 178, 183);
var factory = new LegacySymbolContextFactory();
ulong root = BuildSymbols(factory, [4]);
ulong currentLeaf = BuildSymbols(factory, [4, 5]);
var matchingRow = new byte[88];
W16(matchingRow, 36, 10);
matchingRow[26] = 7;
int callsMade = 0;
byte MatchingCategory(ushort symbol) { callsMade++; return 7; }
ulong rewritten = LegacyPreparedCompactWordFrameProducer.RewriteCurrentContext(factory, currentLeaf, matchingRow, 0, prepared, MatchingCategory);
Require(factory.ReadNode(rewritten).ParentLink == root && factory.ReadNode(rewritten).Symbol == 193, "New replacement child was not linked to the actual parent.");
int nodeCount = factory.NodeCount;
Require(LegacyPreparedCompactWordFrameProducer.RewriteCurrentContext(factory, currentLeaf, matchingRow, 0, prepared, MatchingCategory) == rewritten &&
    factory.NodeCount == nodeCount && callsMade == 2, "Repeated context rewrite must reuse actual identity.");

//Native links only new nodes. An existing unlinked child is not repaired.
var preexistingFactory = new LegacySymbolContextFactory();
ulong preexistingRoot = BuildSymbols(preexistingFactory, [4]);
ulong existingLeaf = BuildSymbols(preexistingFactory, [4, 5]);
ulong unlinked = preexistingFactory.ResolveChild(preexistingRoot, 193).Identity;
Require(LegacyPreparedCompactWordFrameProducer.RewriteCurrentContext(preexistingFactory, existingLeaf, matchingRow, 0,
    prepared, _ => 7) == unlinked && preexistingFactory.ReadNode(unlinked).ParentLink == 0,
    "Existing context was relinked despite native Created=false.");

var full = new LegacySymbolContextFactory(1);
ulong only = BuildSymbols(full, [1]);
var mismatch = new byte[88];
mismatch[5] = 128;
W16(mismatch, 24, 1);
var nullAllocation = LegacyPreparedCompactWordFrameProducer.CreateOrdinary(full, only, 0, mismatch, 0, 0, prepared, _ => 0);
Require(nullAllocation.RewrittenCurrentContext == 0 && full.NodeCount == 1 &&
    I32(nullAllocation.CopyFrame(), 0) == 1 && U16(nullAllocation.CopyFrame(), 4) == prepared.KnowledgeSymbol0e,
    "Allocation failure must propagate null context, while2067C0 keeps its own mismatch branch.");

Reject(() => LegacyPreparedCompactWordFrameProducer.RewriteCurrentContext(factory, only, mismatch, 0, prepared, _ => 0));
Reject(() => LegacyPreparedCompactWordFrameProducer.CreateOrdinary(factory, 0, 0, new byte[87], 0, 0, prepared, _ => 0));
Reject(() => LegacyPreparedCompactWordFrameProducer.RewriteCurrentContext(null!, 0, emptyRow, 0, prepared, _ => 0));
using (var cancellation = new CancellationTokenSource())
{
    cancellation.Cancel();
    Reject(() => LegacyPreparedCompactWordFrameProducer.CreateOrdinary(factory, 0, 0, emptyRow, 0, 0, prepared, _ => 0, cancellation.Token));
}
using (var cancellation = new CancellationTokenSource())
{
    int before = factory.NodeCount;
    Reject(() => LegacyPreparedCompactWordFrameProducer.RewriteCurrentContext(factory, currentLeaf, matchingRow, 0,
        prepared, _ => { cancellation.Cancel(); return 7; }, cancellation.Token));
    Require(factory.NodeCount == before, "Canceled category callback inserted a context node.");
}
//Category callbacks are external: the prepared source snapshot must survive
//mutation of caller storage before downstream arc-coordinate production.
W16(matchingRow, 28, 37);
var snapshotFrame = LegacyPreparedCompactWordFrameProducer.CreateOrdinary(factory, currentLeaf, 0, matchingRow, 0, 0,
    prepared, _ => { W16(matchingRow, 28, 49); return 7; });
Require(BinaryPrimitives.ReadUInt64LittleEndian(snapshotFrame.CopyFrame().AsSpan(0x260)) == (37UL << 24),
    "Category callback mutation changed the captured source arc.");

if (args.Length == 5)
{
    byte[] ak = File.ReadAllBytes(args[1]), lk = File.ReadAllBytes(args[2]), engine = File.ReadAllBytes(args[3]);
    var fixture = MainFixture.Create(ak, lk, engine, File.ReadAllBytes(args[4]));
    var owner = new LegacyOwnedMainWordSpanPreparation(ak, lk, engine, fixture.Sampled, fixture.Table);
    var controller = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner);
    var alphabet = owner.CommonModel.AcousticAlphabet;
    var actualPolicy = LegacyPreparedCompactWordContextPolicy.FromAlphabet(alphabet, false, false, false);
    Require(actualPolicy.KnowledgeSymbol0e == U16(alphabet.CopySerializedHeader(), 6) && actualPolicy.Special251c == alphabet.LabelCount + 1 &&
        actualPolicy.Special251e == alphabet.LabelCount + 2, "Alphabet-derived native header/count symbols differ.");
    ulong identity = 1;
    var pass = new LegacyOwnedMainDirectWordPass(ak, controller, fixture.Table, request =>
    {
        byte[] bytes = new byte[request.RequiredBytes];
        request.PreviousStorage.Span.CopyTo(bytes);
        return new(bytes, identity++);
    });
    var result = pass.RunOwned();
    int realRows = 0;
    for (int node = 0; node < result.NodeCount; node++)
    {
        var snapshot = result.GetNodeSnapshot(node);
        for (int ordinal = 0; ordinal < snapshot.SourceEnd23f0; ordinal++)
        {
            byte[] row = snapshot.Records.Slice(ordinal * 88, 88).ToArray();
            ulong context = BinaryPrimitives.ReadUInt64LittleEndian(row.AsSpan(8));
            if (context == 0 || I32(row, 16) >= 0x4000000 || U16(row, 24) != node && (sbyte)row[5] < 0) continue;
            var labels = new List<ushort>();
            for (ulong at = context; at != 0; at = owner.Contexts.ReadNode(at).ParentLink) labels.Add(owner.Contexts.ReadNode(at).Symbol);
            labels.Reverse();
            if (labels.Any(label => label >= 0xffeb || label == actualPolicy.Special24de)) continue;
            var produced = LegacyPreparedCompactWordFrameProducer.CreateOrdinary(owner.Contexts, context, 0, row, (uint)node,
                0, actualPolicy, _ => throw new InvalidDataException("Prepared no-associated row called category service."));
            byte[] frame = produced.CopyFrame();
            byte[] expected = new byte[944];
            W32(expected, 0, labels.Count);
            for (int index = 0; index < labels.Count; index++) W16(expected, 4 + index * 12, labels[index]);
            W32(expected, 0x25c, 1);
            BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(0x260), (uint)node | ((ulong)U16(row, 28) << 24));
            Require(produced.RewrittenCurrentContext == context && frame.AsSpan().SequenceEqual(expected), "Actual resource ordinary complete frame differs.");
            realRows++;
        }
    }
    Require(realRows > 0, "Actual resources must exercise nonempty prepared frames.");
    Console.WriteLine($"PASS actual NN/DIRECT {realRows} ordinary complete frames, alphabet-bound symbols; preparedassociated/sentinel flags remain explicit.");
}
Console.WriteLine($"PASS {controls} full2075E0 frame/context/callback controls/{bytesChecked} bytes, {guards} guards, Created-only linkage, allocation0 propagation and callback snapshot.");

void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or OperationCanceledException)
    { guards++; return; }
    throw new InvalidDataException("Invalid prepared frame input accepted.");
}
static ulong Build(LegacySymbolContextFactory contexts, JsonElement symbols)
    => BuildSymbols(contexts, symbols.EnumerateArray().Select(value => value.GetUInt16()));
static ulong BuildSymbols(LegacySymbolContextFactory contexts, IEnumerable<ushort> symbols)
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
static int I32(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadInt32LittleEndian(data[offset..]);
static ushort U16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
static void W16(Span<byte> data, int offset, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(data[offset..], value);
static void W32(Span<byte> data, int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(data[offset..], value);
