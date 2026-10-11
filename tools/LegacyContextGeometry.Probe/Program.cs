using InkRecognition;
using System.Text.Json;
if (args.Length > 3) throw new ArgumentException("Usage: LegacyContextGeometry.Probe [own-controls.json] [acoustic.res activations.json]");
if (args.Length is 1 or 3)
{
    if (new FileInfo(args[0]).Length > 8 * 1024 * 1024) throw new ArgumentException("Control file exceeds8MiB.");
    using var document = JsonDocument.Parse(File.ReadAllBytes(args[0])); int count = 0;
    foreach (var item in document.RootElement.EnumerateArray())
    {
        var strokes = item.GetProperty("strokes").EnumerateArray().Select(s => new LegacyInkStroke(s.EnumerateArray()
            .Select(p => new LegacyInkPoint(p[0].GetInt16(), p[1].GetInt16())).ToArray())).ToArray();
        var actual = LegacyContextGeometry.Extract(strokes, item.GetProperty("height").GetSingle(),
            item.GetProperty("xReference").GetSingle(), item.GetProperty("yReference").GetSingle(),
            item.GetProperty("maximumX").GetBoolean(), item.GetProperty("pointMeanY").GetBoolean());
        var expected = item.GetProperty("expected").EnumerateArray().Select(p => p.GetSingle()).ToArray();
        Require(expected.Length == 30 && actual.Zip(expected).All(pair => Math.Abs(pair.First - pair.Second) <= 3e-7f), $"Context30 mismatch{count}."); count++;
    }
    Require(count > 0, "Empty controls."); Console.WriteLine($"PASS {count} independent context30 geometry/raster comparisons (absolute tolerance3e-7).");
}
var horizontal = new LegacyInkStroke[] { new([new LegacyInkPoint(0, 0), new(10, 0)]) };
var features = LegacyContextGeometry.Extract(horizontal, 10, 0, 0);
Require(features.Length == 30 && features[0] == 1 && features[1] == 0 && features[2] == 0 &&
    features[5] == -.75f && features[6] == -.5f && features[7] == -.5f && features[11] == 1 && features[13] == .5f &&
    features.AsSpan(15, 3).SequenceEqual(new float[] { 1, -1, -1 }), "Own horizontal feature layout failed.");
var translated = new LegacyInkStroke[] { new([new LegacyInkPoint(50, 100), new(60, 100)]) };
Require(features.SequenceEqual(LegacyContextGeometry.Extract(translated, 10, 50, 100)), "Own single-stroke reference translation failed.");
var doubleLine = LegacyContextGeometry.Extract([horizontal[0], horizontal[0]], 10, 0, 0);
Require(doubleLine[6] == -.5f && doubleLine[7] == -.5f && doubleLine[11] == 1 && doubleLine[5] == -.5f,
    "Raster must merge overlapping marks while physical arc accumulates.");
var sentinel = LegacyContextGeometry.Extract([horizontal[0], new([new LegacyInkPoint(0, 0), new(0, 5)])], 10, 0, 20);
Require(sentinel[18] == -.5f && sentinel[22] == 0, "Later zero top must preserve native sentinel while first top subtracts reference.");
var quantized = LegacyFloatInputQuantization.Quantize(features);
Require(quantized.Length == 30 && quantized[0] == 1024 && quantized[13] == 512, "Geometry→fixed input composition failed.");
Throws<ArgumentException>(() => LegacyContextGeometry.Extract([], 10, 0, 0));
Throws<ArgumentException>(() => LegacyContextGeometry.Extract([new([new LegacyInkPoint(0, 0)])], 10, 0, 0));
Throws<ArgumentException>(() => LegacyContextGeometry.Extract(horizontal, 0, 0, 0));
Throws<ArgumentException>(() => LegacyContextGeometry.Extract(horizontal, 10, float.NaN, 0));
Throws<ArgumentException>(() => LegacyContextGeometry.Extract(Enumerable.Repeat(horizontal[0], 16).ToArray(), 10, 0, 0));
Throws<OperationCanceledException>(() => LegacyContextGeometry.Extract(horizontal, 10, 0, 0, cancellationToken: new CancellationToken(true)));
Console.WriteLine("PASS own context30 layout, reference translation, overlap raster/physical arc, zero-top sentinel, fixed conversion, malformed/degenerate rejection and cancellation. Prior selection and handwriting accuracy are not tested.");
if (args.Length >= 2)
{
    int offset = args.Length == 3 ? 1 : 0;
    foreach (string path in args.Skip(offset)) if (new FileInfo(path).Length > 8 * 1024 * 1024) throw new ArgumentException("Caller asset exceeds8MiB.");
    byte[] acoustic = File.ReadAllBytes(args[offset]);
    using var tables = JsonDocument.Parse(File.ReadAllBytes(args[offset + 1]));
    int[] Values(JsonElement element) => element.EnumerateArray().Select(p => p.GetInt32()).ToArray();
    var activations = new LegacyInkActivations(Values(tables.RootElement.GetProperty("tanh").GetProperty("values")),
        Values(tables.RootElement.GetProperty("sigmoid").GetProperty("values")));
    var network = LegacyInkNetwork.Load(acoustic.AsSpan(189755, 195746 - 189755));
    Require(network.Kind == 0x7001 && network.InputWidth == 30 && network.OutputWidth == 28, "Unexpected inspected RU initial context network.");
    foreach (string letter in new[] { "И", "П" })
    {
        LegacyInkPoint[] points = letter == "И" ? [new(0,0), new(0,40), new(24,0), new(24,40)] : [new(0,40), new(0,0), new(24,0), new(24,40)];
        var input = LegacyFloatInputQuantization.Quantize(LegacyContextGeometry.Extract([new(points)], 40, 0, 40));
        var scores = LegacyClassScores.Convert(network.Evaluate(input, activations)).Probability32768.ToArray();
        var top = Enumerable.Range(0, scores.Length).OrderByDescending(i => scores[i]).ThenBy(i => i).Take(5);
        using var buffer = new MemoryStream(); using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject(); writer.WriteString("ownShape", letter);
            writer.WriteString("callerGeometry", "height40;Xreference0;Yreference40;maxXfalse;pointMeanYfalse;not automatic line fitting");
            writer.WriteString("model", "inspected initial7001 context30, classes intentionally unlabeled");
            writer.WriteStartArray("top"); foreach (int index in top)
            { writer.WriteStartObject(); writer.WriteNumber("class", index); writer.WriteNumber("probability32768", scores[index]); writer.WriteEndObject(); }
            writer.WriteEndArray(); writer.WriteEndObject();
        }
        Console.WriteLine(System.Text.Encoding.UTF8.GetString(buffer.ToArray()));
    }
    Console.WriteLine("Completed own context30 initial-network diagnostics; prior candidate selection, line fitting and text accuracy are not tested.");
}
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected{typeof(T).Name}."); }
