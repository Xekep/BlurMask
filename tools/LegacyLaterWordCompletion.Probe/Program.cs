using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 5) throw new ArgumentException("controls.json AK LK engine.json separation.json required");
byte[] acoustic = File.ReadAllBytes(args[1]);
var alphabet = LegacyInkAlphabet.Load(acoustic.AsSpan(99), 0x103);
Require(alphabet.TryGetLabel("-", out ushort dash), "Actual20c1c0 dash label missing.");
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0, callbacks = 0;
foreach (var test in document.RootElement.GetProperty("cases").EnumerateArray())
{
    var contexts = new LegacySymbolContextFactory();
    ulong suffix = Path(contexts, test.GetProperty("path").EnumerateArray().Select(v => v.GetUInt16()).ToArray());
    byte[] anchor = Bytes(test, "anchor"), previous = Bytes(test, "previous");
    BinaryPrimitives.WriteUInt64LittleEndian(anchor.AsSpan(8), suffix);
    BinaryPrimitives.WriteUInt16LittleEndian(previous.AsSpan(36),
        BinaryPrimitives.ReadUInt16LittleEndian(previous.AsSpan(36)) == 65534 ? dash : (ushort)0);
    uint[] headers = test.GetProperty("headers").EnumerateArray().Select(v => v.GetUInt32()).ToArray();
    int[] lines = test.GetProperty("lines").EnumerateArray().Select(v => v.GetInt32()).ToArray();
    var expectedCallbacks = test.GetProperty("callbacks").EnumerateArray().ToArray();
    int call = 0;
    void Check(string kind, params int[] values)
    {
        Require(call < expectedCallbacks.Length, "Unexpected history selector callback.");
        var expected = expectedCallbacks[call++];
        Require(expected[0].GetString() == kind && expected.EnumerateArray().Skip(1).Select(v => v.GetInt32()).SequenceEqual(values),
            "Independent F5/descriptor-line/owner encounter order differs.");
        callbacks++;
    }
    var result = LegacyNativeWordHistoryPreparation.Prepare(Bytes(test, "token"), test.GetProperty("currentId").GetByte(), previous, 5,
        anchor, test.GetProperty("difference").GetInt32(), test.GetProperty("subword").GetInt32(), test.GetProperty("extended").GetBoolean(),
        test.GetProperty("f5").GetBoolean(), dash, lines.Length, contexts,
        (node, ordinal) => { Check("anchor", node, ordinal); return Stored(Bytes(test, "priorAnchor")); },
        id => { Check("header", id); return headers[id]; },
        (node, ordinal) => { Check("line", node, ordinal); return lines[node]; });
    Require(result.UsesPreviousHistory == test.GetProperty("use").GetBoolean() &&
        result.ClearsFirstCategoryTag == test.GetProperty("clear").GetBoolean() &&
        result.History.Span.SequenceEqual(test.GetProperty("expectedHistory").EnumerateArray().Select(v => v.GetUInt32()).ToArray()) &&
        result.SourceToken16.Span.SequenceEqual(Bytes(test, "expectedToken")) &&
        result.IncludesBoundaryCost == (Bytes(test, "expectedToken")[12] != 0) && call == expectedCallbacks.Length,
        "Independent full16 token/history/F5/source/line/marker predicate differs.");
    cases++;
}
Console.WriteLine($"PASS {cases} independent history/token/source/F5/line/oldest-marker controls/{callbacks} owned callback checks.");
var selectedModel = LegacyRussianModelBundle.Load(acoustic, File.ReadAllBytes(args[2]), File.ReadAllBytes(args[3]));
var prefix = LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(File.ReadAllBytes(args[2]).AsSpan(606964, 4565174 - 606964));
var metadata = new LegacyPreparedDictionaryWordMetadata(selectedModel.Lexicon, selectedModel.WordIndex);
var language = new LegacyNativeKnownWordLanguageRecord(selectedModel.LanguageModel,
    selectedModel.LanguageProfiles[selectedModel.ResourceLanguageProfileIndex], selectedModel.AuxiliaryProfiles[selectedModel.ResourceLanguageProfileIndex],
    prefix.RuntimeLanguageScale);
Require(selectedModel.AcousticAlphabet.TryGetLabel("a", out ushort plain), "Actual formatter plain label missing.");
int formatters = 0;
foreach (var test in document.RootElement.GetProperty("formatters").EnumerateArray())
{
    var contexts = new LegacySymbolContextFactory();
    ulong suffix = Path(contexts, test.GetProperty("suffix").EnumerateArray().Select(v => v.GetUInt16()).ToArray());
    byte[] current = Bytes(test, "current");
    BinaryPrimitives.WriteUInt16LittleEndian(current.AsSpan(36), plain);
    BinaryPrimitives.WriteUInt64LittleEndian(current.AsSpan(8), suffix);
    var currentRows = new[] { Stored(current) };
    var previousRows = test.GetProperty("rows").EnumerateArray().Select(value =>
    {
        var bytes = Convert.FromHexString(value.GetString()!);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(36),
            BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(36)) == 65533 ? dash : (ushort)0);
        return Stored(bytes);
    }).ToArray();
    uint[] headers = test.GetProperty("headers").EnumerateArray().Select(v => v.GetUInt32()).ToArray();
    int[] lines = test.GetProperty("lines").EnumerateArray().Select(v => v.GetInt32()).ToArray();
    var heap = new LegacyNativeWordRecordHeap(1, Bytes(test, "storage"));
    int rootCost = test.GetProperty("budgetRoot").GetInt32();
    Require(heap.TryAllocate(rootCost, slot => BinaryPrimitives.WriteInt32LittleEndian(slot[16..], rootCost)), "Formatter budget seed failed.");
    var router = new LegacyNativeWordHeapRouter([heap, new(1)], new byte[] { 0, 0 }, 0, uint.MaxValue);
    var completion = new LegacyNativeLaterDictionaryWordCompletion(selectedModel, metadata, language,
        LegacyPreparedDictionaryRestartTags.CreateFresh(), 1, headers[1], false, 0, false, false);
    var packet = new LegacyNativeLanguageContextRecombinationResult(true, Bytes(test, "context"), Bytes(test, "packet"), 1, 2);
    var result = completion.ProcessCandidate(packet, 0, 8, 0, 10,
        owner => owner == 8 ? currentRows : owner == 6 ? previousRows : throw new InvalidDataException("Unknown formatter node owner."),
        id => headers[id], (node, ordinal) => { Require(ordinal == 0, "E8 ordinal changed."); return lines[node]; },
        contexts, router, test.GetProperty("f5").GetBoolean(), test.GetProperty("subword").GetInt32(), false,
        new byte[32], Bytes(test, "residue"), true);
    var score = result.Language.Score;
    Require(result.LanguageAccepted == test.GetProperty("accepted").GetBoolean() && result.Allocation.Accepted == test.GetProperty("allocated").GetBoolean() &&
        result.PreLexicalCost == test.GetProperty("path").GetInt32() && result.RestartTag28 == test.GetProperty("restart").GetUInt64() &&
        result.ContextualLineage5c == test.GetProperty("lineage").GetUInt64() &&
        result.History.UsesPreviousHistory == test.GetProperty("use").GetBoolean() &&
        result.History.ClearsFirstCategoryTag == test.GetProperty("clear").GetBoolean() &&
        result.History.IncludesBoundaryCost == test.GetProperty("boundary").GetBoolean(), "Independent history/lineage/restart/budget ownership differs.");
    Require(score.WordClass == test.GetProperty("wordClass").GetInt32() &&
        score.PreviousClass == (test.GetProperty("previousClass").ValueKind == JsonValueKind.Null ? null : test.GetProperty("previousClass").GetInt32()) &&
        score.WordCost == test.GetProperty("wordCost").GetInt32() && score.ClassCost == test.GetProperty("classCost").GetInt32() &&
        score.CategoryCost == test.GetProperty("categoryCost").GetInt32() && score.BoundaryCost == test.GetProperty("boundaryCost").GetInt32() &&
        score.LexicalSubtotal == test.GetProperty("subtotal").GetInt32(), "Independent actual class/word/bigram/category/boundary numeric costs differ.");
    Require(result.LanguageAccepted ? result.Language.Record40.Span.SequenceEqual(Bytes(test, "output")) : result.Language.Record40.IsEmpty,
        "Independent full40 model history/residue output differs.");
    byte[] expectedSlot = Bytes(test, "expectedSlot");
    if (result.Allocation.Accepted) BinaryPrimitives.WriteUInt16LittleEndian(expectedSlot.AsSpan(36), plain);
    Require(heap.GetSlotSnapshot(0).Span.SequenceEqual(expectedSlot), "Independent full128 cache record, inherited bytes, allocator tail or budget rejection differs.");
    formatters++;
}
Console.WriteLine($"PASS {formatters} independent real-resource numeric/full40/full128 terminal-context formatter controls.");
int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidDataException or InvalidOperationException or NotSupportedException or OperationCanceledException or OverflowException)
    { guards++; return; }
    throw new InvalidDataException("Invalid later-word history ownership accepted.");
}
var guardCase = document.RootElement.GetProperty("formatters")[2];
byte[] guardCurrent = Bytes(guardCase, "current"), guardPacket = Bytes(guardCase, "packet"), guardCandidate = Bytes(guardCase, "context");
BinaryPrimitives.WriteUInt16LittleEndian(guardCurrent.AsSpan(36), plain);
var guardRows = guardCase.GetProperty("rows").EnumerateArray().Select(value =>
{
    var row = Convert.FromHexString(value.GetString()!);
    BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(36), BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(36)) == 65533 ? dash : (ushort)0);
    return Stored(row);
}).ToArray();
var guardCurrentRows = new[] { Stored(guardCurrent) };
uint[] guardHeaders = guardCase.GetProperty("headers").EnumerateArray().Select(value => value.GetUInt32()).ToArray();
int[] guardLines = guardCase.GetProperty("lines").EnumerateArray().Select(value => value.GetInt32()).ToArray();
var guardContexts = new LegacySymbolContextFactory();
var guardResult = new LegacyNativeLanguageContextRecombinationResult(true, guardCandidate, guardPacket, 1, 2);
LegacyNativeLaterDictionaryWordCompletion NewGuard() => new(selectedModel, metadata, language,
    LegacyPreparedDictionaryRestartTags.CreateFresh(), 1, guardHeaders[1], false, 0, false, false);
LegacyNativeWordHeapRouter NewRouter() => new([new(64), new(32)], new byte[] { 0, 0 }, 0, uint.MaxValue);
IReadOnlyList<LegacyNativeStoredWordRecord> GuardNode(int owner) => owner == 8 ? guardCurrentRows : owner == 6 ? guardRows : throw new InvalidDataException("Foreign guard node.");
LegacyNativeLaterDictionaryWordCompletionResult Guard(LegacyNativeLaterDictionaryWordCompletion? target = null,
    LegacyNativeLanguageContextRecombinationResult? context = null, int index = 0, int ending = 8, ushort ordinal = 0,
    Func<int, IReadOnlyList<LegacyNativeStoredWordRecord>>? lookup = null, Func<byte, uint>? header = null,
    Func<int, int, int>? line = null, byte[]? query = null, byte[]? residue = null,
    CancellationToken token = default)
    => (target ?? NewGuard()).ProcessCandidate(context ?? guardResult, index, ending, ordinal, 10,
        lookup ?? GuardNode, header ?? (id => guardHeaders[id]), line ?? ((node, _) => guardLines[node]),
        guardContexts, NewRouter(), false, -1, false, query ?? new byte[32], residue ?? new byte[40], true, token);
Reject(() => new LegacyNativeLaterDictionaryWordCompletion(null!, metadata, language, default, 1, guardHeaders[1], false, 0, false, false));
Reject(() => new LegacyNativeLaterDictionaryWordCompletion(selectedModel, null!, language, default, 1, guardHeaders[1], false, 0, false, false));
Reject(() => new LegacyNativeLaterDictionaryWordCompletion(selectedModel, metadata, null!, default, 1, guardHeaders[1], false, 0, false, false));
var foreignIndex = new LegacyPreparedDictionaryWordMetadata(selectedModel.Lexicon, new(selectedModel.Lexicon));
Reject(() => new LegacyNativeLaterDictionaryWordCompletion(selectedModel, foreignIndex, language, default, 1, guardHeaders[1], false, 0, false, false));
var modelProfile = selectedModel.LanguageProfiles[selectedModel.ResourceLanguageProfileIndex];
var copiedProfile = new LegacyWordLanguageCostProfile(modelProfile.WordSlope, modelProfile.WordOffset, modelProfile.UnigramSlope,
    modelProfile.UnigramOffset, modelProfile.BigramSlope, modelProfile.BigramOffset);
var foreignLanguage = new LegacyNativeKnownWordLanguageRecord(selectedModel.LanguageModel, copiedProfile,
    selectedModel.AuxiliaryProfiles[selectedModel.ResourceLanguageProfileIndex], prefix.RuntimeLanguageScale);
Reject(() => new LegacyNativeLaterDictionaryWordCompletion(selectedModel, metadata, foreignLanguage, default, 1, guardHeaders[1], false, 0, false, false));
foreach (var flags in new[] { (Mode: true, Ec: 0f, Front: false, Trace: false), (Mode: false, Ec: 1f, Front: false, Trace: false),
    (Mode: false, Ec: 0f, Front: true, Trace: false), (Mode: false, Ec: 0f, Front: false, Trace: true) })
    Reject(() => new LegacyNativeLaterDictionaryWordCompletion(selectedModel, metadata, language, default, 1, guardHeaders[1], flags.Mode, flags.Ec, flags.Front, flags.Trace));
Reject(() => Guard(index: -1));
Reject(() => Guard(index: 1));
Reject(() => Guard(ending: 10));
Reject(() => Guard(ordinal: 1));
Reject(() => Guard(query: new byte[31]));
Reject(() => Guard(residue: new byte[39]));
Reject(() => Guard(context: guardResult with { LastRequest80 = new byte[79] }));
Reject(() => Guard(context: guardResult with { Candidates16 = new byte[15] }));
byte[] badPacket = guardPacket.ToArray();
BinaryPrimitives.WriteInt32LittleEndian(badPacket.AsSpan(8), 5);
Reject(() => Guard(context: guardResult with { LastRequest80 = badPacket }));
byte[] badCandidate = guardCandidate.ToArray();
BinaryPrimitives.WriteUInt16LittleEndian(badCandidate.AsSpan(12), 9);
Reject(() => Guard(context: guardResult with { Candidates16 = badCandidate }));
badCandidate = guardCandidate.ToArray();
BinaryPrimitives.WriteUInt16LittleEndian(badCandidate.AsSpan(14), 1);
Reject(() => Guard(context: guardResult with { Candidates16 = badCandidate }));
Reject(() => Guard(header: id => guardHeaders[id] ^ 1));
Reject(() => Guard(lookup: _ => null!));
Reject(() => NewGuard().ProcessCandidate(guardResult, 0, 8, 0, 10, GuardNode, id => guardHeaders[id], null!,
    guardContexts, NewRouter(), false, -1, false, new byte[32], new byte[40], true));
var mismatchRows = guardCase.GetProperty("rows").EnumerateArray().Select(value => Stored(Convert.FromHexString(value.GetString()!))).ToArray();
if (System.Runtime.InteropServices.MemoryMarshal.TryGetArray(mismatchRows[3].Record, out ArraySegment<byte> mismatch))
    mismatch.Array![mismatch.Offset + 40] ^= 1;
var mismatchContext = guardCandidate.ToArray();
BinaryPrimitives.WriteUInt16LittleEndian(mismatchContext.AsSpan(12), 3);
Reject(() => Guard(context: guardResult with { Candidates16 = mismatchContext }, lookup: owner => owner == 8 ? guardCurrentRows : mismatchRows));
for (int stage = 0; stage < 3; stage++)
{
    using var canceled = new CancellationTokenSource();
    int selected = stage;
    var target = NewGuard();
    Reject(() => Guard(target, token: canceled.Token,
        lookup: owner => { if (selected == 0) canceled.Cancel(); return GuardNode(owner); },
        header: id => { if (selected == 1) canceled.Cancel(); return guardHeaders[id]; },
        line: (node, _) => { if (selected == 2) canceled.Cancel(); return guardLines[node]; }));
    Reject(() => Guard(target));
}
var baseline = Guard();
byte[] mutableContext = guardCandidate.ToArray(), mutableRequest = guardPacket.ToArray(), mutableQuery = new byte[32],
    mutableResidue = Enumerable.Repeat((byte)0xa7, 40).ToArray();
var snapshot = Guard(context: guardResult with { Candidates16 = mutableContext, LastRequest80 = mutableRequest }, query: mutableQuery,
    residue: mutableResidue, lookup: owner =>
    { Array.Clear(mutableContext); Array.Clear(mutableRequest); Array.Clear(mutableQuery); Array.Clear(mutableResidue); return GuardNode(owner); });
Require(snapshot.LanguageAccepted && snapshot.ContextualLineage5c == baseline.ContextualLineage5c &&
    snapshot.Language.Record40.Span[9] == 0xa7, "Later-word context/request/query/lexical residue changed during node callback.");
Console.WriteLine($"PASS {guards} later-word model/profile/context/node/cancellation/lifecycle guards and input snapshots.");
int helperGuardsBefore = guards;
byte[] helperToken = new byte[16], helperPrevious = guardRows[2].Record.ToArray(), helperAnchor = guardCurrent.ToArray();
LegacyNativePreparedWordHistory Helper(byte[]? token = null, byte[]? previous = null, byte[]? anchor = null,
    int difference = 0, int subword = -1, int previousNode = 6, int available = 10,
    LegacyNativeWordParentRowLookup? lookup = null, Func<byte, uint>? header = null,
    Func<int, int, int>? line = null, bool f5 = true, CancellationToken cancellation = default)
    => LegacyNativeWordHistoryPreparation.Prepare(token ?? helperToken, 1, previous ?? helperPrevious, previousNode,
        anchor ?? helperAnchor, difference, subword, false, f5, dash, available, guardContexts,
        lookup ?? ((node, ordinal) => GuardNode(node)[ordinal]), header ?? (id => guardHeaders[id]),
        line ?? ((node, _) => guardLines[node]), cancellation);
Reject(() => Helper(token: new byte[15]));
Reject(() => Helper(previous: new byte[87]));
Reject(() => Helper(anchor: new byte[87]));
Reject(() => Helper(previousNode: -1));
Reject(() => Helper(available: 0));
Reject(() => Helper(subword: 256));
Reject(() => Helper(subword: 1)); //Selected subword source cannot dereference a missing suffix context.
byte[] excessiveHistory = helperPrevious.ToArray();
excessiveHistory[64] = 2;
Reject(() => Helper(previous: excessiveHistory));
Require(!Helper(previous: excessiveHistory, difference: 2).UsesPreviousHistory,
    "Unused foreign/fresh history must not parse unsupported old payload counts.");
var invalidF5Anchor = helperPrevious.ToArray();
BinaryPrimitives.WriteUInt16LittleEndian(invalidF5Anchor.AsSpan(36), dash);
BinaryPrimitives.WriteUInt16LittleEndian(invalidF5Anchor.AsSpan(60), ushort.MaxValue);
Reject(() => Helper(previous: invalidF5Anchor));
for (int stage = 0; stage < 4; stage++)
{
    using var canceled = new CancellationTokenSource();
    int selected = stage, lineCalls = 0;
    var prior = helperPrevious.ToArray();
    BinaryPrimitives.WriteUInt16LittleEndian(prior.AsSpan(36), dash);
    var priorAnchor = guardRows[0].Record.ToArray();
    priorAnchor[32] = 4; //End6, start2: both E8 reads are required.
    Reject(() => Helper(previous: prior, cancellation: canceled.Token,
        header: id => { if (selected == 0) canceled.Cancel(); return guardHeaders[id]; },
        lookup: (_, _) => { if (selected == 1) canceled.Cancel(); return Stored(priorAnchor); },
        line: (node, _) => { if (selected == 2 + lineCalls++) canceled.Cancel(); return 0; }));
}
byte[] mutablePrevious = helperPrevious.ToArray(), mutableToken = helperToken.ToArray();
mutablePrevious[64] = 1;
BinaryPrimitives.WriteUInt32LittleEndian(mutablePrevious.AsSpan(68), 0x0700000c);
var helperSnapshot = Helper(token: mutableToken, previous: mutablePrevious, header: id =>
{ Array.Clear(mutablePrevious); Array.Clear(mutableToken); return guardHeaders[id]; });
Require(helperSnapshot.History.Span.SequenceEqual(new uint[] { 0x0700000c }), "History payload changed during source-header callback.");
Console.WriteLine($"PASS {guards - helperGuardsBefore} history token/count/source/suffix/F5 callback guards and prior-payload snapshot.");
ActualLaterWordCompletion.Run(args);

static byte[] Bytes(JsonElement test, string property) => Convert.FromHexString(test.GetProperty(property).GetString()!);
static LegacyNativeStoredWordRecord Stored(byte[] row)
{
    byte[] slot = new byte[128]; row.CopyTo(slot, 0);
    return LegacyNativeWordRowMaterializer.Materialize(slot, 0, 0, 1, 1);
}
static ulong Path(LegacySymbolContextFactory contexts, ReadOnlySpan<ushort> symbols)
{
    ulong value = 0;
    foreach (ushort symbol in symbols)
    {
        var next = value == 0 ? contexts.ResolveInitial(symbol) : contexts.ResolveChild(value, symbol);
        if (next.Created && value != 0) contexts.LinkCreatedContext(next.Identity, value);
        value = next.Identity;
    }
    return value;
}
static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
