using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InkRecognition;

if (args.Length is not (1 or 3)) throw new ArgumentException("controls.json [RU acoustic.res RU lexical.res]");
byte[] payload = Alphabet(["A", "a", "\u0410", "\u0430", "p", "\u0440", "-", "\u00ad", "virtual"], 8);
var alphabet = LegacyInkAlphabet.Load(payload, 0x103);
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0;
foreach (var item in document.RootElement.EnumerateArray())
{
    var sources = item.GetProperty("sources").EnumerateArray().Select(s => new LegacyLinguisticSymbolSource(
        s.GetProperty("tokens").EnumerateArray().Select(x => x.GetUInt32()).ToArray(),
        s.GetProperty("map").EnumerateArray().Select(x => x.GetUInt16()).ToArray(),
        s.GetProperty("completed").GetBoolean())).ToArray();
    var binding = new LegacyPreparedOrdinaryLinguisticMaskBinding(alphabet, payload.AsSpan(0, 16), sources);
    var actual = binding.Build(item.GetProperty("bypass").GetBoolean(), item.GetProperty("prepared").GetBoolean(), [],
        item.GetProperty("permission").EnumerateArray().Select(x => x.GetByte()).ToArray(), [], []);
    Require((int)actual.ControllerOutcome == item.GetProperty("outcome").GetInt32() &&
        actual.Masks.Primary.Span.SequenceEqual(Bytes(item, "primary")) &&
        actual.Masks.Secondary.Span.SequenceEqual(Bytes(item, "secondary")), $"Binding control {cases} differs.");
    cases++;
}

// Caller-owned buffers/list can change after attachment; an attached snapshot cannot.
uint[] tokens = [0]; ushort[] map = [5];
var mutable = new List<LegacyLinguisticSymbolSource> { new(tokens, map) };
var snapshot = new LegacyPreparedOrdinaryLinguisticMaskBinding(alphabet, payload.AsSpan(0, 16), mutable);
tokens[0] = 0xffff; map[0] = 4; mutable.Clear();
var frozen = snapshot.Build(false, true, [], [], [], []);
Require(frozen.Masks.Primary.Span[5] == 1 && frozen.Masks.Primary.Span[4] == 0, "Attached prepared sources must be snapshots.");
// Failure and bypass deliberately differ from accepting a partial successful enumeration.
var cache = new LegacyOrdinaryAcousticClassCache(new(0, 1, 1, 1, 1, 1, 65536, 1, 9), alphabet, payload.AsSpan(0, 16));
int[] neural = Enumerable.Repeat(1024, 9).ToArray(); neural[4] = 0;
var glyph = new[] { new LegacyInkStroke([new LegacyInkPoint(0, 0), new LegacyInkPoint(10, 0)]) };
var context = new LegacyOrdinaryAcousticCacheContext(false, true, true, false, false, null, false, ReadOnlyMemory<ushort>.Empty);
foreach (var (bypass, prepared, expectedOutcome, allowedLatin) in new[]
    { (false, true, 2, false), (false, false, 1, true), (true, false, 0, true), (true, true, 0, true) })
{
    var result = snapshot.Build(bypass, prepared, [], [], [], []);
    var final = cache.Build(neural, glyph, result.Masks.Primary.Span, context, []);
    Require((int)result.ControllerOutcome == expectedOutcome &&
        (final.Costs.Span[4] < 0x4000000) == allowedLatin && final.Costs.Span[5] < 0x4000000,
        "Explicit prepared/bypass outcomes must reach the final cache.");
}
var incomplete = new LegacyPreparedOrdinaryLinguisticMaskBinding(alphabet, payload.AsSpan(0, 16), [new(new uint[] { 5 }, ReadOnlyMemory<ushort>.Empty, false)]);
Require(incomplete.Build(false, true, [], [], [], []).ControllerOutcome == LegacySymbolMaskControllerOutcome.Failed,
    "Failed enumeration must not accept its partial mask.");
Reject(() => new LegacyPreparedOrdinaryLinguisticMaskBinding(alphabet, payload.AsSpan(0, 16), [new(new uint[] { 1 }, new ushort[] { 5 })]));
Reject(() => new LegacyPreparedOrdinaryLinguisticMaskBinding(alphabet, payload.AsSpan(0, 16), [new(new uint[1_048_577], ReadOnlyMemory<ushort>.Empty)]));
Reject(() => snapshot.Build(true, false, [], [], [], [], new CancellationToken(true)));
Console.WriteLine($"PASS {cases} independent prepared-binding controls; snapshot ownership, explicit preparation/bypass, failed enumeration and final-cache integration.");

if (args.Length == 3)
{
    byte[] akBytes = File.ReadAllBytes(args[1]), lkBytes = File.ReadAllBytes(args[2]);
    Require(Convert.ToHexStringLower(SHA256.HashData(akBytes)) == "7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978" &&
        Convert.ToHexStringLower(SHA256.HashData(lkBytes)) == "d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e", "Expected pinned RU resources.");
    var ak = LegacyInkAlphabet.Load(akBytes.AsSpan(99), 0x103);
    var lk = LegacyInkAlphabet.Load(lkBytes.AsSpan(607074), 0x103);
    var binding = new LegacyPreparedOrdinaryLinguisticMaskBinding(ak, akBytes.AsSpan(99, 16),
        [LegacyLinguisticSymbolMaskController.CreateDictionarySource(ak, lk)]);
    var groups = LegacyOrdinarySymbolMaskProducer.ReadV110KnowledgeGroups(akBytes.AsSpan(189734), ak.LabelCount, out _);
    var actual = binding.Build(false, true, groups, [], [], LegacyWideSymbolPairs.CreateUnrestricted(ak));
    Require(actual.Masks.Primary.Span.Count((byte)1) == 145, "Actual prepared RU source mask differs.");
    var actualCache = new LegacyOrdinaryAcousticClassCache(LegacyOrdinaryAcousticCacheProfile.ReadV110Node(akBytes.AsSpan(189734)), ak, akBytes.AsSpan(99, 16));
    int excluded = Enumerable.Range(0, 180).First(i => actual.Masks.Primary.Span[i] == 0 && !ak.IsVirtual((ushort)i));
    var actualCosts = Enumerable.Repeat(8192, 180).ToArray(); actualCosts[excluded] = 0;
    var cached = actualCache.Build(actualCosts, glyph, actual.Masks.Primary.Span, context, []);
    Require(cached.Costs.Length == 195 && cached.Costs.Span[excluded] == 0x4000000, "Prepared RU source restriction must reach actual v110 final cache.");
    Require(ak.TryGetLabel("p", out ushort latin) && ak.TryGetLabel("\u0440", out ushort cyrillic) &&
        latin != cyrillic && actual.Masks.Primary.Span[latin] == 1 && actual.Masks.Primary.Span[cyrillic] == 1,
        "Native Russian dictionary does not collapse Latin/Cyrillic labels.");
    Console.WriteLine($"PASS pinned RU source -> v110 final cache:145/192 primary labels,180 controlled acoustic inputs,195 cache costs; best excluded class {excluded} rejected. No OCR/word accuracy claim.");
}

static byte[] Bytes(JsonElement item, string key) => item.GetProperty(key).EnumerateArray().Select(x => x.GetByte()).ToArray();
static byte[] Alphabet(string[] text, int virtualLabel)
{
    using var stream = new MemoryStream(); using var w = new BinaryWriter(stream);
    for (int i = 0; i < 7; i++) w.Write(ushort.MaxValue);
    w.Write((ushort)text.Length); w.Write((ushort)1); w.Write((ushort)0);
    foreach (int n in new[] { 32, 64 }) for (int i = 0; i < n; i++) w.Write(i == n - 1 ? (ushort)0 : ushort.MaxValue);
    w.Write((ushort)0); w.Write((ushort)text.Length); uint offset = 0;
    for (int i = 0; i < text.Length; i++) { w.Write(offset); w.Write((uint)i); offset += (uint)Encoding.UTF8.GetByteCount(text[i]); }
    w.Write(offset); w.Write(65535u); foreach (string value in text) w.Write(Encoding.UTF8.GetBytes(value));
    for (int i = 0; i < text.Length; i++) w.Write(i == virtualLabel ? ushort.MaxValue : (ushort)i);
    w.Write((byte)0); w.Write((ushort)0); return stream.ToArray();
}
static void Require(bool ok, string message) { if (!ok) throw new InvalidDataException(message); }
static void Reject(Action action)
{ try { action(); } catch (Exception e) when (e is ArgumentException or InvalidDataException or OperationCanceledException) { return; } throw new InvalidDataException("Malformed/canceled binding accepted."); }
