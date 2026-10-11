using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using InkRecognition;

if (args.Length is not (1 or 2)) throw new ArgumentException("Usage: controls.json [ru_RU-ak-cur.lite.res]");
byte[] ownPayload = BuildAlphabet();
var alphabet = LegacyInkAlphabet.Load(ownPayload, 0x103);
var producer = new LegacyOrdinarySymbolMaskProducer(alphabet, ownPayload.AsSpan(0, 16));
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int checkedCases = 0;
foreach (var test in controls.RootElement.GetProperty("cases").EnumerateArray())
{
    byte[] Bytes(string name) => test.GetProperty(name).EnumerateArray().Select(v => v.GetByte()).ToArray();
    var groups = test.GetProperty("groups").EnumerateArray().Select(v => new LegacySymbolMaskKnowledgeGroup(
        v[0].EnumerateArray().Select(x => x.GetUInt16()).ToArray(), v[1].GetSingle())).ToArray();
    var forms = test.GetProperty("forms").EnumerateArray().Select(v => new LegacyContextualSymbolForms(v[0].GetUInt16(), v[1].GetUInt16(), v[2].GetUInt16())).ToArray();
    var pairs = test.GetProperty("pairs").EnumerateArray().Select(v => new LegacyAcousticCachePair(v[0].GetUInt16(), v[1].GetUInt16())).ToArray();
    var result = producer.Build((LegacySymbolMaskControllerOutcome)test.GetProperty("outcome").GetInt32(), Bytes("seed"), groups, Bytes("restriction"), forms, pairs);
    Require(result.Primary.Span.SequenceEqual(Bytes("expectedPrimary")) && result.Secondary.Span.SequenceEqual(Bytes("expectedSecondary")), "Independent native ordered mask output differs.");
    var working = result.CopyWithoutClassPreselectionForSources(test.GetProperty("sourceIds").EnumerateArray().Select(v => v.GetInt32()).ToArray(), test.GetProperty("templates").GetInt32(), Bytes("lower"), Bytes("upper"));
    Require(working.Primary.Span.SequenceEqual(Bytes("expectedWorkingPrimary")) && working.Secondary.Span.SequenceEqual(Bytes("expectedWorkingSecondary")), "Source count producer -> working mask -> range filter differs.");
    Require(result.Primary.Span.SequenceEqual(Bytes("expectedPrimary")), "Working copy mutated base controller mask.");
    checkedCases++;
}

// Restriction closure is asymmetric: primary-only wide fill, no Latin/Cyrillic folding.
byte[] seed = new byte[27], restriction = new byte[4]; seed[15] = 1; restriction[15 >> 3] |= (byte)(1 << (15 & 7));
var wide = producer.Build(LegacySymbolMaskControllerOutcome.Succeeded, seed, [], restriction, [], [new(15, 16)]);
Require(wide.Primary.Span[16] == 1 && wide.Secondary.Span[16] == 0 && wide.Primary.Span[18] == 0 && wide.Primary.Span[20] == 0, "Wide expansion must not become secondary/script alias expansion.");
// Ordered groups are one pass, not a fixed-point closure; newly active late labels do not revisit earlier groups.
seed.AsSpan().Clear(); seed[17] = 1;
var ordered = producer.Build(LegacySymbolMaskControllerOutcome.Succeeded, seed, [new(new ushort[] { 18, 20 }, 1), new(new ushort[] { 17, 18 }, 1)], [], [], []);
Require(ordered.Primary.Span[18] == 1 && ordered.Primary.Span[20] == 0, "Ordered groups unexpectedly use transitive fixed-point closure.");
// Mask producer feeds actual cache/scorer APIs using an original synthetic alphabet/profile/glyph.
var profile = new LegacyOrdinaryAcousticCacheProfile(0, 1, 1, 1, 1, 1, 65536, 1, 27);
var cache = new LegacyOrdinaryAcousticClassCache(profile, alphabet, ownPayload.AsSpan(0, 16));
int[] neural = Enumerable.Repeat(1024, 27).ToArray(); neural[20] = 0;
var context = new LegacyOrdinaryAcousticCacheContext(false, true, true, false, false, null, false, ReadOnlyMemory<ushort>.Empty);
var cached = cache.Build(neural, [new LegacyInkStroke([new LegacyInkPoint(0, 0), new LegacyInkPoint(10, 0)])], wide.Primary.Span, context, []);
var scorer = new LegacyPreparedAcousticCandidateScorer(new(65536, 65536, 65536), [65535, 65535, 65535, 65535, 65535]);
Require(cached.Costs.Span[20] == 0x4000000 && !scorer.Evaluate(cached.Costs.Span, 20, [0], 0, 1, 0, 65536).Accepted,
    "Disabled Cyrillic label with best neural score must not reach candidate search.");
Require(scorer.Evaluate(cached.Costs.Span, 15, [0], 0, 1, 0, 65536).Accepted, "Enabled Latin candidate rejected by mask bridge.");

Reject(() => producer.Build((LegacySymbolMaskControllerOutcome)3, [], [], [], [], []));
Reject(() => producer.Build(LegacySymbolMaskControllerOutcome.Succeeded, [], [], [], [], []));
Reject(() => producer.Build(LegacySymbolMaskControllerOutcome.Failed, seed, [], [], [], []));
Reject(() => producer.Build(LegacySymbolMaskControllerOutcome.UnboundOrBypassed, [], [], [1], [], []));
Reject(() => producer.Build(LegacySymbolMaskControllerOutcome.UnboundOrBypassed, [], [new(new ushort[] { 27 }, 1)], [], [], []));
Reject(() => producer.Build(LegacySymbolMaskControllerOutcome.UnboundOrBypassed, [], [new(new ushort[] { 1 }, float.NaN)], [], [], []));
Reject(() => producer.Build(LegacySymbolMaskControllerOutcome.UnboundOrBypassed, [], [], [], [new(1, 2, 3)], []));
Reject(() => producer.Build(LegacySymbolMaskControllerOutcome.UnboundOrBypassed, [], [], [], [], [new(1, 65535)]));
seed[0] = 2; Reject(() => producer.Build(LegacySymbolMaskControllerOutcome.Succeeded, seed, [], [], [], []));
Reject(() => producer.Build(LegacySymbolMaskControllerOutcome.UnboundOrBypassed, [], [], [], [], [], new CancellationToken(true)));
Reject(() => new LegacyOrdinarySymbolMaskProducer(alphabet, []));
Console.WriteLine($"PASS {checkedCases} independent ordered controller/group/restriction/contextual/wide/virtual/primitive/source-count masks; final-cache/candidate integration and defensive guards.");

if (args.Length == 2)
{
    byte[] resource = File.ReadAllBytes(args[1]);
    var actualAlphabet = LegacyInkAlphabet.Load(resource.AsSpan(99), 0x103);
    var actualProducer = new LegacyOrdinarySymbolMaskProducer(actualAlphabet, resource.AsSpan(99, 16));
    var actualGroups = LegacyOrdinarySymbolMaskProducer.ReadV110KnowledgeGroups(resource.AsSpan(189734), actualAlphabet.LabelCount, out int consumed);
    Require(actualAlphabet.LabelCount == 192 && actualGroups.Length == 0, "Observed RU192/v110 knowledge groups differ.");
    var baseline = actualProducer.Build(LegacySymbolMaskControllerOutcome.UnboundOrBypassed, [], actualGroups, [], [], LegacyWideSymbolPairs.CreateUnrestricted(actualAlphabet));
    foreach (string text in new[] { "a", "а", "p", "р", "м" })
        if (actualAlphabet.TryGetLabel(text, out ushort id)) Require(baseline.Primary.Span[id] == 1, "Unbound Russian controller must not infer an alias restriction.");
    // Check the group reader against inserted own data within the observed checked envelope.
    int nodeLength = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(resource.AsSpan(189738)) + 8);
    byte[] node = resource.AsSpan(189734, nodeLength).ToArray();
    byte[] ownTail;
    using (var stream = new MemoryStream())
    { using var writer = new BinaryWriter(stream); writer.Write((short)2); foreach (ushort x in new ushort[] { 0, 2, 3, 15, 18, 20 }) writer.Write(x); writer.Write(.25f); ownTail = stream.ToArray(); }
    byte[] altered = [.. node.AsSpan(0, consumed - 2), .. ownTail, .. node.AsSpan(consumed)];
    BinaryPrimitives.WriteUInt32LittleEndian(altered.AsSpan(4), (uint)(altered.Length - 8));
    var decoded = LegacyOrdinarySymbolMaskProducer.ReadV110KnowledgeGroups(altered, 192, out int read);
    Require(decoded.Length == 2 && decoded[0].Symbols.Span.SequenceEqual(new ushort[] { 15, 18 }) && decoded[1].Symbols.Span.SequenceEqual(new ushort[] { 20 }) && decoded.All(g => g.Weight == .25f) && read == consumed - 2 + ownTail.Length, "v110 common-weight group decoding differs.");
    Reject(() => LegacyOrdinarySymbolMaskProducer.ReadV110KnowledgeGroups(altered.AsSpan(0, read - 1), 192, out _));
    BinaryPrimitives.WriteUInt16LittleEndian(altered.AsSpan(consumed + 2), 4);
    Reject(() => LegacyOrdinarySymbolMaskProducer.ReadV110KnowledgeGroups(altered, 192, out _));
    Console.WriteLine("PASS actual RU192/v110 group reader:0 groups; explicit unbound controller keeps Latin/Cyrillic distinct enabled classes. Own inserted two-group coefficient/truncation/range controls PASS; no native language binding or word-accuracy claim.");
}

static byte[] BuildAlphabet()
{
    string[] text = ["d0", "dash", "circle", "fallback", "v4", "v5", "v6", "-", "_", "\U000f0002", "\U000f0003", "\u30fc", "\u4e00", "\U000f0001", "\u00ad", "A", "\uff21", "a", "\u0430", "p", "\u0440", "context-base", "context-initial", "context-medial", "context-final", "virtual-extra", "z"];
    using var stream = new MemoryStream(); using var w = new BinaryWriter(stream); int count = text.Length;
    for (ushort i = 0; i < 7; i++) w.Write(i); w.Write((ushort)count); w.Write((ushort)1); w.Write((ushort)0);
    foreach (int n in new[] { 32, 64 }) for (int i = 0; i < n; i++) w.Write(i == n - 1 ? (ushort)0 : ushort.MaxValue);
    w.Write((ushort)0); w.Write((ushort)count); uint offset = 0;
    for (int i = 0; i < count; i++) { w.Write(offset); w.Write((uint)i); offset += (uint)Encoding.UTF8.GetByteCount(text[i]); }
    w.Write(offset); w.Write(65535u); foreach (string value in text) w.Write(Encoding.UTF8.GetBytes(value));
    for (int i = 0; i < count; i++) w.Write(i < 7 || i == 25 ? ushort.MaxValue : (ushort)i);
    w.Write((byte)0); w.Write((ushort)0); return stream.ToArray();
}
static void Require(bool ok, string message) { if (!ok) throw new InvalidDataException(message); }
static void Reject(Action action)
{ try { action(); } catch (Exception e) when (e is ArgumentException or InvalidDataException or OperationCanceledException) { return; } throw new InvalidDataException("Malformed/canceled input accepted."); }
