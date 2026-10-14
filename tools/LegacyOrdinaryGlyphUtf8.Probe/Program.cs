using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InkRecognition;

if (args.Length != 3) throw new ArgumentException("Pass ordinary controls, frozen glyph-token controls and pinned RU resource.");
using var own = JsonDocument.Parse(File.ReadAllBytes(args[0])); int accepted = 0, rejected = 0;
foreach (var c in own.RootElement.GetProperty("cases").EnumerateArray())
{
    var bytes = Convert.FromHexString(c.GetProperty("bytes").GetString()!);
    if (!c.GetProperty("accepted").GetBoolean()) { Reject(() => LegacyNativeOrdinaryGlyphUtf8.CompleteWithoutCircledStart(bytes)); rejected++; continue; }
    var result = LegacyNativeOrdinaryGlyphUtf8.CompleteWithoutCircledStart(bytes);
    if (!result.AsSpan().SequenceEqual(bytes)) throw new Exception("Native ordinary bytes changed by a framework conversion.");
    result.AsSpan().Clear();
    if (!LegacyNativeOrdinaryGlyphUtf8.CompleteWithoutCircledStart(bytes).AsSpan().SequenceEqual(bytes)) throw new Exception("Ordinary output aliases source storage.");
    accepted++;
}
using var tokenControls = JsonDocument.Parse(File.ReadAllBytes(args[1]));
var alphabet = LegacyInkAlphabet.Load(Convert.FromHexString(tokenControls.RootElement.GetProperty("alphabet").GetString()!), 0x103);
int byteCases = 0, byteRefusals = 0;
foreach (var c in tokenControls.RootElement.GetProperty("byteCases").EnumerateArray())
{
    var tokens = c.GetProperty("tokens").EnumerateArray().Select(t => new LegacyNativeGlyphUtf8Token(t[0].GetUInt16(), t[1].GetBoolean())).ToArray();
    LegacyNativeGlyphAlternateUtf8Table? alt = null;
    if (c.GetProperty("alternate").ValueKind != JsonValueKind.Null)
    {
        var segments = c.GetProperty("alternate").EnumerateArray().Select(v => Convert.FromHexString(v.GetString()!)).ToArray();
        var offsets = new List<int> { 0 }; int end = 0;
        foreach (var segment in segments) { end += segment.Length; offsets.Add(end); }
        alt = new(segments.SelectMany(s => s).ToArray(), offsets.ToArray());
    }
    var raw = LegacyNativeGlyphUtf8Tokens.EmitBeforeCanonicalComposition(tokens, alphabet, alt);
    bool valid = true; int cursor = 0;
    while (cursor < raw.Length)
    {
        if (Rune.DecodeFromUtf8(raw.AsSpan(cursor), out Rune rune, out int used) != OperationStatus.Done || rune.Value == 0xf0000)
        { valid = false; break; }
        cursor += used;
    }
    if (!valid) { Reject(() => LegacyNativeOrdinaryGlyphUtf8.CompleteWithoutCircledStart(raw)); byteRefusals++; continue; }
    var actual = LegacyNativeOrdinaryGlyphUtf8.CompleteWithoutCircledStart(raw);
    if (!actual.AsSpan().SequenceEqual(Convert.FromHexString(c.GetProperty("expected").GetString()!))) throw new Exception("Glyph token->helper ordinary composition mismatch.");
    byteCases++;
}
for (int kind = 0; kind < 8; kind++)
    for (int label = 0; label < 14; label++)
    {
        var desc = new LegacyNativeGlyphSymbolDescriptor((ushort)kind, (ushort)label);
        if (kind == 3 && (label & 3) == 1 && label >> 2 != 0) continue;
        var raw = LegacyNativeGlyphUtf8Tokens.EmitBeforeCanonicalComposition(LegacyNativeGlyphUtf8Tokens.Expand(desc, Lookup), alphabet);
        if (!LegacyNativeOrdinaryGlyphUtf8.Emit(desc, alphabet, actualControlLabelLookup: Lookup).AsSpan().SequenceEqual(raw))
            throw new Exception("Descriptor->ordinary UTF8 emitter differs from the pinned ordinary byte policy.");
    }
byte[] resource = File.ReadAllBytes(args[2]);
if (Convert.ToHexString(SHA256.HashData(resource)).ToLowerInvariant() != "7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978")
    throw new InvalidDataException("Pinned actual RU resource required.");
var ru = LegacyInkAlphabet.Load(resource.AsSpan(99), 0x103); int actualLabels = 0, actualMarkers = 0;
for (int label = 0; label < ru.LabelCount; label++)
{
    if (!ru.TryGetText((ushort)label, out string expected)) continue;
    if (expected.Contains("\U000f0000", StringComparison.Ordinal))
    { Reject(() => LegacyNativeOrdinaryGlyphUtf8.Emit(new(0, (ushort)label), ru)); actualMarkers++; continue; }
    var actual = LegacyNativeOrdinaryGlyphUtf8.Emit(new(0, (ushort)label), ru);
    if (!actual.AsSpan().SequenceEqual(Encoding.UTF8.GetBytes(expected))) throw new Exception("Actual ordinary RU glyph emission mismatch.");
    actualLabels++;
}
Reject(() => LegacyNativeOrdinaryGlyphUtf8.Emit(new(0, 0xfff6), ru));
using var canceled = new CancellationTokenSource(); canceled.Cancel();
try { LegacyNativeOrdinaryGlyphUtf8.CompleteWithoutCircledStart([], canceled.Token); throw new Exception("Cancellation ignored."); } catch (OperationCanceledException) { }
Reject(() => LegacyNativeOrdinaryGlyphUtf8.CompleteWithoutCircledStart(new byte[1_048_577]));
Console.WriteLine($"PASS {accepted} exact ordinary identity/{rejected} scope refusals, {byteCases} glyph-byte/helper compositions/{byteRefusals} restricted alternatives, {actualLabels} actual RU stored labels/{actualMarkers} genuine circled-start token refused. No winner/source-record acceptance claim.");

static ushort Lookup(string text) => text switch { "\n" => 8, " " => 9, "\u00a0" => 10, _ => ushort.MaxValue };
static void Reject(Action action) { try { action(); } catch (ArgumentException) { return; } catch (InvalidDataException) { return; } catch (NotSupportedException) { return; }
    throw new Exception("Invalid/out-of-scope ordinary emitter accepted."); }
