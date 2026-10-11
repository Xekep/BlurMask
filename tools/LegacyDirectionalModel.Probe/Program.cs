using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

var minima = Enumerable.Repeat((short)-1024, 274).ToArray();
var maxima = Enumerable.Repeat((short)1024, 274).ToArray();
var context = Enumerable.Repeat((short)-1024, 28).ToArray();
var ownActivations = new LegacyInkActivations(new int[48], Enumerable.Repeat(512, 128).ToArray());
var ownNetwork = LegacyInkNetwork.Load(BuildNetwork());
foreach (var glyph in new[] { "И", "П" })
{
    var first = Features(glyph, 1, 0, 0);
    var translated = Features(glyph, 1, -100, 300);
    Require(first.SequenceEqual(translated), "Composed source translation/baseline invariance failed.");
    Require(first.Length == 313 && first[0] == 1024 && first.AsSpan(283, 28).SequenceEqual(context) &&
        first[^2] == 1024 && first[^1] == 1024, "Composed context/boundary layout failed.");
    var probabilities = LegacyClassScores.Convert(ownNetwork.Evaluate(first, ownActivations)).Probability32768.Span;
    Require(probabilities.SequenceEqual(new[] { 16384, 16384 }), "Own zero-weight end-to-end network probabilities failed.");
}
Throws<ArgumentException>(() => LegacyTemporalDirectionalFeatures.Extract(OwnGlyph("И", 1, 0, 0), new(40, 40), [], maxima, context, 1024, 1024));
Throws<ArgumentException>(() => LegacyTemporalDirectionalFeatures.Extract(OwnGlyph("И", 1, 0, 0), new(40, 0), minima, maxima, context, 1024, 1024));
Throws<OperationCanceledException>(() => LegacyTemporalDirectionalFeatures.Extract(OwnGlyph("И", 1, 0, 0), new(40, 40), minima, maxima, context, 1024, 1024, new CancellationToken(true)));
Console.WriteLine("PASS own composed313 translation/baseline mapping, context/boundaries, zero-weight classifier probabilities, malformed inputs and cancellation. No vendor assets required.");
if (args.Length == 0) return;
if (args.Length != 3) throw new ArgumentException("Usage: LegacyDirectionalModel.Probe [acoustic.res activations.json calibration.json]");
foreach (string path in args) if (new FileInfo(path).Length > 8 * 1024 * 1024) throw new ArgumentException("Caller file exceeds8MiB.");
byte[] acoustic = File.ReadAllBytes(args[0]);
using var tables = JsonDocument.Parse(File.ReadAllBytes(args[1]));
using var calibration = JsonDocument.Parse(File.ReadAllBytes(args[2]));
int[] Values(JsonElement element) => element.EnumerateArray().Select(p => p.GetInt32()).ToArray();
var activations = new LegacyInkActivations(Values(tables.RootElement.GetProperty("tanh").GetProperty("values")),
    Values(tables.RootElement.GetProperty("sigmoid").GetProperty("values")));
minima = calibration.RootElement.GetProperty("minima").EnumerateArray().Select(p => p.GetInt16()).ToArray();
maxima = calibration.RootElement.GetProperty("maxima").EnumerateArray().Select(p => p.GetInt16()).ToArray();
// Checked offsets of the inspected legacy RU lite resource only. Other
// resources require their own verified profile and class correspondence.
var network = LegacyInkNetwork.Load(acoustic.AsSpan(367899, 467094 - 367899));
var alphabet = LegacyInkAlphabet.Load(acoustic.AsSpan(99), 0x103);
Require(network.Kind == 0x3f && network.InputWidth == 313 && network.OutputWidth == 180, "Unexpected inspected directional profile.");
foreach (string glyph in new[] { "И", "П" })
    foreach (var (scale, x, y) in new[] { (1, 0, 0), (1, -100, 300), (2, 0, 0), (3, 200, -100) })
    {
        var features = Features(glyph, scale, x, y);
        var scores = LegacyClassScores.Convert(network.Evaluate(features, activations)).Probability32768.ToArray();
        var top = Enumerable.Range(0, scores.Length).OrderByDescending(i => scores[i]).ThenBy(i => i).Take(5).ToArray();
        using var buffer = new MemoryStream(); using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject(); writer.WriteString("ownGlyph", glyph); writer.WriteNumber("scale", scale);
            writer.WriteNumber("x", x); writer.WriteNumber("y", y); writer.WriteString("sourceBaseline", $"Y={y + 40 * scale};height={40 * scale}");
            writer.WriteString("context", "explicit caller choice:28 x -1024;boundaries1024;not SDK inferred");
            writer.WriteStartArray("top"); foreach (int index in top)
            {
                writer.WriteStartObject(); writer.WriteNumber("class", index);
                if (alphabet.TryGetText((ushort)index, out string text)) writer.WriteString("text", text);
                writer.WriteNumber("probability32768", scores[index]); writer.WriteEndObject();
            }
            writer.WriteEndArray(); writer.WriteEndObject();
        }
        Console.WriteLine(System.Text.Encoding.UTF8.GetString(buffer.ToArray()));
    }
Console.WriteLine("Completed eight own printed directional313 diagnostics. Class accuracy is reported, not required to pass; joined cursive, inferred baseline/context and full SDK ensemble are not tested.");

short[] Features(string glyph, int scale, int x, int y) => LegacyTemporalDirectionalFeatures.Extract(OwnGlyph(glyph, scale, x, y),
    new LegacyInkBaseline(checked((short)(y + 40 * scale)), checked((short)(40 * scale))), minima, maxima, context, 1024, 1024);
static LegacyInkStroke[] OwnGlyph(string glyph, int scale, int x, int y)
{
    (int X, int Y)[] points = glyph == "И" ? [(0, 0), (0, 40), (24, 0), (24, 40)] : [(0, 40), (0, 0), (24, 0), (24, 40)];
    return [new(points.Select(p => new LegacyInkPoint(checked((short)(x + p.X * scale)), checked((short)(y + p.Y * scale)))).ToArray())];
}
static byte[] BuildNetwork()
{
    using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
    writer.Write(0x0106fea0u); writer.Write(0u); writer.Write((byte)0x12); writer.Write((ushort)0x3f);
    writer.Write(313u); writer.Write(2u); writer.Write(0u); writer.Write(0u); writer.Write(3u); writer.Write((ushort)10);
    writer.Write(new byte[313]); writer.Write(new byte[] { 1, 1 });
    writer.Write(new byte[4]); writer.Write(new byte[] { 1, 1, 1 });
    var data = stream.ToArray(); BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(4), (uint)data.Length - 8); return data;
}
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected{typeof(T).Name}."); }
