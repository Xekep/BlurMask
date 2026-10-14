using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InkRecognition;

if (args.Length is not (1 or 2)) throw new ArgumentException("Pass original token controls JSON and optional pinned RU acoustic resource.");
using var doc = JsonDocument.Parse(File.ReadAllBytes(args[0]));
var alphabet = LegacyInkAlphabet.Load(Convert.FromHexString(doc.RootElement.GetProperty("alphabet").GetString()!), 0x103);
int descriptors = 0, undefined = 0, emitted = 0;
foreach (var c in doc.RootElement.GetProperty("descriptors").EnumerateArray())
{
    var d = new LegacyNativeGlyphSymbolDescriptor(c[0].GetUInt16(), c[1].GetUInt16()); var e = c[2];
    if (e.ValueKind == JsonValueKind.Null) { Reject(() => LegacyNativeGlyphUtf8Tokens.Expand(d, Lookup)); undefined++; continue; }
    var tokens = LegacyNativeGlyphUtf8Tokens.Expand(d, Lookup);
    if (tokens.Length != e[0].GetInt32() || tokens.Any(t => t.Label != e[1].GetUInt16() || t.UseAlternateAlphabet != e[2].GetBoolean()))
        throw new Exception("Native descriptor repetition/label/alternate flag mismatch.");
    descriptors++;
}
for (int symbol = 0; symbol <= 65535; symbol++)
    for (int kind = 0; kind < 8; kind++)
    {
        int expected = kind is 1 or 3 ? symbol >> 2 & 0x1fff : 1;
        if (LegacyNativeGlyphUtf8Tokens.ReadTokenCount(new((ushort)kind, (ushort)symbol)) != expected)
            throw new Exception("Native count mask/shift mismatch.");
    }
foreach (var c in doc.RootElement.GetProperty("byteCases").EnumerateArray())
{
    var tokens = c.GetProperty("tokens").EnumerateArray().Select(t => new LegacyNativeGlyphUtf8Token(t[0].GetUInt16(), t[1].GetBoolean())).ToArray();
    LegacyNativeGlyphAlternateUtf8Table? alt = null;
    if (c.GetProperty("alternate").ValueKind != JsonValueKind.Null)
    {
        var segments = c.GetProperty("alternate").EnumerateArray().Select(s => Convert.FromHexString(s.GetString()!)).ToArray();
        var bytes = segments.SelectMany(s => s).ToArray(); int end = 0;
        var offsets = new List<int> { 0 }; foreach (var s in segments) { end += s.Length; offsets.Add(end); }
        alt = new(bytes, offsets.ToArray()); bytes.AsSpan().Clear();
    }
    var actual = LegacyNativeGlyphUtf8Tokens.EmitBeforeCanonicalComposition(tokens, alphabet, alt);
    if (!actual.AsSpan().SequenceEqual(Convert.FromHexString(c.GetProperty("expected").GetString()!)))
        throw new Exception("Native pre-canonical UTF8 byte emission mismatch.");
    actual.AsSpan().Clear();
    if (!LegacyNativeGlyphUtf8Tokens.EmitBeforeCanonicalComposition(tokens, alphabet, alt).AsSpan()
        .SequenceEqual(Convert.FromHexString(c.GetProperty("expected").GetString()!))) throw new Exception("Byte output aliases caller or source table.");
    emitted++;
}
var decomposed = LegacyNativeGlyphUtf8Tokens.EmitBeforeCanonicalComposition([new(3, false)], alphabet);
if (!decomposed.AsSpan().SequenceEqual("e\u0301"u8)) throw new Exception("A framework normalization was silently substituted for the native unfinished stage.");
Reject(() => LegacyNativeGlyphUtf8Tokens.Expand(new(3, 4)));
Reject(() => new LegacyNativeGlyphAlternateUtf8Table([1], [0]));
Reject(() => new LegacyNativeGlyphAlternateUtf8Table([1], [0, 2]));
Reject(() => new LegacyNativeGlyphAlternateUtf8Table([1], [1, 0]));
Reject(() => LegacyNativeGlyphUtf8Tokens.EmitBeforeCanonicalComposition([new(1, true)], alphabet, new([1], [0, 1])));
Reject(() => LegacyNativeGlyphUtf8Tokens.EmitBeforeCanonicalComposition(new LegacyNativeGlyphUtf8Token[65537], alphabet));
Reject(() => LegacyNativeGlyphUtf8Tokens.EmitBeforeCanonicalComposition(Enumerable.Repeat(new LegacyNativeGlyphUtf8Token(13, false), 513).ToArray(), alphabet));
using var canceled = new CancellationTokenSource(); canceled.Cancel();
Canceled(() => LegacyNativeGlyphUtf8Tokens.Expand(new(0, 0), cancellationToken: canceled.Token));
Canceled(() => LegacyNativeGlyphUtf8Tokens.EmitBeforeCanonicalComposition([], alphabet, cancellationToken: canceled.Token));
Console.WriteLine($"PASS {descriptors} descriptor token cases/{undefined} undefined kind refusals +524288 count controls +{emitted} exact byte/alternate ownership cases; budgets/cancellation. Canonical composition and winner remain separate.");
if (args.Length == 2)
{
    byte[] resource = File.ReadAllBytes(args[1]);
    if (Convert.ToHexString(SHA256.HashData(resource)).ToLowerInvariant() != "7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978")
        throw new InvalidDataException("Pinned actual RU resource required.");
    var ru = LegacyInkAlphabet.Load(resource.AsSpan(99), 0x103); int actualLabels = 0;
    for (int label = 0; label < ru.LabelCount; label++)
    {
        if (!ru.TryGetText((ushort)label, out string expected)) continue;
        var tokens = LegacyNativeGlyphUtf8Tokens.Expand(new(0, (ushort)label));
        if (!LegacyNativeGlyphUtf8Tokens.EmitBeforeCanonicalComposition(tokens, ru).AsSpan().SequenceEqual(Encoding.UTF8.GetBytes(expected)))
            throw new Exception("Actual RU stored alphabet emission mismatch.");
        actualLabels++;
    }
    foreach (string text in new[] { "м", "и", "р", "ё", "й", "Ё", "Й" })
    {
        if (!ru.TryGetLabel(text, out var label)) throw new Exception("Pinned alphabet fixture absent.");
        var raw = LegacyNativeGlyphUtf8Tokens.EmitBeforeCanonicalComposition(LegacyNativeGlyphUtf8Tokens.Expand(new(0, label)), ru);
        if (!raw.AsSpan().SequenceEqual(Encoding.UTF8.GetBytes(text))) throw new Exception("Actual ordinary Russian byte producer mismatch.");
    }
    Console.WriteLine($"PASS actual RU {actualLabels} stored-label ordinary glyph expansions/UTF8 bytes including Ё/Й. Prepared label records only; no OCR accuracy claim.");
}

static ushort Lookup(string text) => text switch { "\n" => 8, " " => 9, "\u00a0" => 10, _ => ushort.MaxValue };
static void Reject(Action action) { try { action(); } catch (ArgumentException) { return; } catch (InvalidDataException) { return; } catch (NotSupportedException) { return; }
    throw new Exception("Invalid/out-of-scope token producer accepted."); }
static void Canceled(Action action) { try { action(); } catch (OperationCanceledException) { return; } throw new Exception("Cancellation ignored."); }
