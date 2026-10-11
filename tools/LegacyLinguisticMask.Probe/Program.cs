using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using InkRecognition;

if (args.Length is not (1 or 3)) throw new ArgumentException("controls.json [ru_RU-ak-cur.lite.res ru_RU-lk-text.lite.res]");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0;
foreach (var item in controls.RootElement.EnumerateArray())
{
    var sources = item.GetProperty("sources").EnumerateArray().Select(s => new LegacyLinguisticSymbolSource(
        s.GetProperty("tokens").EnumerateArray().Select(x => x.GetUInt32()).ToArray(),
        s.GetProperty("map").EnumerateArray().Select(x => x.GetUInt16()).ToArray(),
        s.GetProperty("completed").GetBoolean())).ToArray();
    var actual = LegacyLinguisticSymbolMaskController.Build(item.GetProperty("count").GetInt32(), sources, item.GetProperty("prepared").GetBoolean());
    var expected = item.GetProperty("expected").EnumerateArray().Select(x => x.GetByte()).ToArray();
    Require((int)actual.Outcome == item.GetProperty("outcome").GetInt32() && actual.Mask.Span.SequenceEqual(expected), $"Linguistic source control {cases} differs.");
    cases++;
}

string[] acousticTexts = ["A", "a", "\u0410", "\u0430", "p", "\u0440", "-", "\u00ad", "\u200d", "virtual"];
byte[] ownPayload = BuildAlphabet(acousticTexts, [9]);
var acoustic = LegacyInkAlphabet.Load(ownPayload, 0x103);
var lexical = LegacyInkAlphabet.Load(BuildAlphabet(["\u0440", "p", "\u0430", "a", " ", "missing", "virtual"], [6]), 0x103);
ushort[] map = LegacyLinguisticSymbolMaskController.CreateExactDictionaryMap(acoustic, lexical);
Require(map.AsSpan().SequenceEqual(new ushort[] { 5, 4, 3, 1, 0xfffb, 65535, 65535 }), "Exact lexical/acoustic IDs, space and virtual mapping differ.");
var source = LegacyLinguisticSymbolMaskController.CreateDictionarySource(acoustic, lexical);
Require(source.Tokens.Span.SequenceEqual(new uint[] { 0, 1, 2, 3, 4, 5, 6 }), "Dictionary enumeration must include every label in source order.");
var seed = LegacyLinguisticSymbolMaskController.Build(10, [source], true);
Require(seed.Mask.Span.SequenceEqual(new byte[] { 0, 1, 0, 1, 1, 1, 0, 0, 0, 0 }), "Dictionary seed differs.");

// Native special-token constants are checked independently as Unicode scalars.
(int Codepoint, ushort Token)[] special = [
    (0, 0xfffd), (32, 0xfffb), (10, 0xfff9), (160, 0xfffc), (0xfffd, 0xffff),
    (0xf000c, 0xfffa), (0xf0009, 0xffff), (0xf000a, 0xffff), (0xf000b, 0xffff),
    (0xf0000, 0xfff6), (0xf0006, 0xfff8), (0xf0007, 0xfff7),
    (0xf0010, 0xfff3), (0xf0011, 0xfff2), (0xf0012, 0xfff1), (0xf0013, 0xfff0),
    (0xf0014, 0xffef), (0xf0015, 0xffee), (0xf0016, 0xffed), (0xf0017, 0xffec), (0xf0018, 0xffeb),
    (0xf0005, 0xffea), (0xf0020, 0xffe9), (0xf0021, 0xffe8), (0xf0022, 0xffe7),
    (0xf0023, 0xffe6), (0xf0024, 0xffe5), (0xf0025, 0xffe4), (0xf0026, 0xffe3)];
foreach (var (codepoint, token) in special)
    Require(LegacyLinguisticSymbolMaskController.ResolveExactDictionaryToken(char.ConvertFromUtf32(codepoint), acoustic) == token, $"Control U+{codepoint:X} differs.");
Require(LegacyLinguisticSymbolMaskController.ResolveExactDictionaryToken("\u0410", acoustic) == 2 &&
    LegacyLinguisticSymbolMaskController.ResolveExactDictionaryToken("A", acoustic) == 0, "Latin and Cyrillic classes must retain distinct IDs.");

// Real producer→consumer path: restrict a prepared source to Cyrillic р, with
// the best acoustic NN score deliberately assigned to excluded Latin p.
var restricted = LegacyLinguisticSymbolMaskController.Build(10, [new(new uint[] { 0 }, map)], true);
var producer = new LegacyOrdinarySymbolMaskProducer(acoustic, ownPayload.AsSpan(0, 16));
var masks = producer.Build(restricted.Outcome, restricted.Mask.Span, [], [], [], []);
var profile = new LegacyOrdinaryAcousticCacheProfile(0, 1, 1, 1, 1, 1, 65536, 1, 10);
var cache = new LegacyOrdinaryAcousticClassCache(profile, acoustic, ownPayload.AsSpan(0, 16));
int[] neural = Enumerable.Repeat(1024, 10).ToArray(); neural[4] = 0;
var context = new LegacyOrdinaryAcousticCacheContext(false, true, true, false, false, null, false, ReadOnlyMemory<ushort>.Empty);
var cached = cache.Build(neural, [new LegacyInkStroke([new LegacyInkPoint(0, 0), new LegacyInkPoint(10, 0)])], masks.Primary.Span, context, []);
Require(cached.Costs.Span[4] == 0x4000000 && cached.Costs.Span[5] < 0x4000000, "Linguistic mask must reach the final acoustic class cache.");
var failed = LegacyLinguisticSymbolMaskController.Build(10, [new(new uint[] { 0 }, map, false)], true);
Require(failed.Outcome == LegacySymbolMaskControllerOutcome.Failed && failed.Mask.IsEmpty &&
    producer.Build(failed.Outcome, failed.Mask.Span, [], [], [], []).Primary.Span[4] == 1, "Failed source must restore native fallback, not accept partial mask.");

Reject(() => LegacyLinguisticSymbolMaskController.Build(0, [], true));
Reject(() => LegacyLinguisticSymbolMaskController.Build(65536, [], true));
Reject(() => LegacyLinguisticSymbolMaskController.Build(10, [new(new uint[] { 3 }, new ushort[] { 1 })], true));
Reject(() => LegacyLinguisticSymbolMaskController.Build(10, [new(new uint[1_048_577], ReadOnlyMemory<ushort>.Empty)], true));
Reject(() => LegacyLinguisticSymbolMaskController.Build(10, [], true, new CancellationToken(true)));
Console.WriteLine($"PASS {cases} independent linguistic source masks;27 control tokens, exact dictionary map/virtual labels, source failure, final acoustic cache and defensive controls.");

if (args.Length == 3)
{
    byte[] akBytes = File.ReadAllBytes(args[1]), lkBytes = File.ReadAllBytes(args[2]);
    Require(Convert.ToHexStringLower(SHA256.HashData(akBytes)) == "7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978" &&
        Convert.ToHexStringLower(SHA256.HashData(lkBytes)) == "d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e", "Expected pinned observed RU resources.");
    // The observed pinned RU resource offsets are diagnostic inputs, not discovery defaults.
    var ak = LegacyInkAlphabet.Load(akBytes.AsSpan(99), 0x103);
    var lk = LegacyInkAlphabet.Load(lkBytes.AsSpan(607074), 0x103);
    Require(ak.LabelCount == 192 && lk.LabelCount == 139, "Observed RU alphabets differ.");
    var actualSource = LegacyLinguisticSymbolMaskController.CreateDictionarySource(ak, lk);
    var actualSeed = LegacyLinguisticSymbolMaskController.Build(192, [actualSource], true);
    Require(actualSeed.Mask.Span.Count((byte)1) == 138 && actualSource.DictionaryMap.Span.Count((ushort)0xfffb) == 1 &&
        !actualSource.DictionaryMap.Span.Contains(ushort.MaxValue), "Actual RU exact map/seed differs.");
    var groups = LegacyOrdinarySymbolMaskProducer.ReadV110KnowledgeGroups(akBytes.AsSpan(189734), 192, out _);
    var actualProducer = new LegacyOrdinarySymbolMaskProducer(ak, akBytes.AsSpan(99, 16));
    var final = actualProducer.Build(actualSeed.Outcome, actualSeed.Mask.Span, groups, [], [], LegacyWideSymbolPairs.CreateUnrestricted(ak));
    foreach (string text in new[] { "a", "\u0430", "p", "\u0440", "m", "\u043c" })
        Require(ak.TryGetLabel(text, out ushort id) && final.Primary.Span[id] == 1, "Observed Russian lexical source must retain its Latin and Cyrillic labels.");
    Console.WriteLine($"PASS RU192 acoustic/RU139 lexical exact source:138 seed labels,{final.Primary.Span.Count((byte)1)} final primary labels. Both Latin and Cyrillic retained; runtime binding/decorator setup/word accuracy not claimed.");
}

static byte[] BuildAlphabet(string[] text, HashSet<int> virtualLabels)
{
    using var stream = new MemoryStream(); using var w = new BinaryWriter(stream); int count = text.Length;
    for (int i = 0; i < 7; i++) w.Write(ushort.MaxValue);
    w.Write((ushort)count); w.Write((ushort)1); w.Write((ushort)0);
    foreach (int n in new[] { 32, 64 }) for (int i = 0; i < n; i++) w.Write(i == n - 1 ? (ushort)0 : ushort.MaxValue);
    w.Write((ushort)0); w.Write((ushort)count); uint offset = 0;
    for (int i = 0; i < count; i++) { w.Write(offset); w.Write((uint)i); offset += (uint)Encoding.UTF8.GetByteCount(text[i]); }
    w.Write(offset); w.Write(65535u); foreach (string value in text) w.Write(Encoding.UTF8.GetBytes(value));
    for (int i = 0; i < count; i++) w.Write(virtualLabels.Contains(i) ? ushort.MaxValue : (ushort)i);
    w.Write((byte)0); w.Write((ushort)0); return stream.ToArray();
}
static void Require(bool ok, string message) { if (!ok) throw new InvalidDataException(message); }
static void Reject(Action action)
{ try { action(); } catch (Exception e) when (e is ArgumentException or InvalidDataException or OperationCanceledException) { return; } throw new InvalidDataException("Malformed/canceled input accepted."); }
