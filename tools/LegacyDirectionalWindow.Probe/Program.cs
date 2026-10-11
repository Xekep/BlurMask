using InkRecognition;
using System.Text.Json;
if (args.Length > 1) throw new ArgumentException("Supply at most one own window-control JSON.");
if (args.Length == 1)
{
    if (new FileInfo(args[0]).Length > 8 * 1024 * 1024) throw new ArgumentException("Control file exceeds 8 MiB.");
    using var document = JsonDocument.Parse(File.ReadAllBytes(args[0])); int count = 0;
    foreach (var item in document.RootElement.EnumerateArray())
    {
        if (item.TryGetProperty("global", out var global))
        {
            int[] Integers(JsonElement p) => p.EnumerateArray().Select(p => p.GetInt32()).ToArray();
            short[] Shorts(string key) => item.GetProperty(key).EnumerateArray().Select(p => p.GetInt16()).ToArray();
            var geometry = LegacyDirectionalFeatureAssembly.TransformGeometry(Integers(global),
                item.GetProperty("windows").EnumerateArray().Select(Integers).ToArray(), Shorts("minima"), Shorts("maxima"));
            var combined = LegacyDirectionalFeatureAssembly.AppendInput(geometry, Integers(item.GetProperty("baseline")), Shorts("context"),
                item.GetProperty("start").GetInt16(), item.GetProperty("end").GetInt16());
            Require(combined.SequenceEqual(Shorts("expected")), $"Channel transform/calibration/order mismatch {count}."); count++; continue;
        }
        var strokes = item.GetProperty("strokes").EnumerateArray().Select(s => new LegacyInkStroke(s.EnumerateArray().Select(p => new LegacyInkPoint(p[0].GetInt16(), p[1].GetInt16())).ToArray())).ToArray();
        byte[] Bytes(string key) => item.GetProperty(key).EnumerateArray().Select(p => p.GetByte()).ToArray();
        var actual = LegacyDirectionalWindowStatistics.Extract(strokes, item.GetProperty("width").GetInt32(), item.GetProperty("height").GetInt32(),
            item.GetProperty("shift").GetInt32(), Bytes("members"), Bytes("flags"), Bytes("intersections"), item.GetProperty("unit").GetBoolean());
        Require(actual.SequenceEqual(item.GetProperty("expected").EnumerateArray().Select(p => p.GetInt32())), $"Local window statistics mismatch {count}."); count++;
    }
    Require(count > 0, "Empty controls."); Console.WriteLine($"PASS {count} independent local-window16 and calibrated313-channel comparisons.");
}
var stroke = new LegacyInkStroke[] { new([new LegacyInkPoint(0, 0), new(10, 0)]) };
var line = LegacyDirectionalWindowStatistics.Extract(stroke, 11, 1, 0, new byte[] { 0, 1 }, new byte[] { 0x41, 0x82 }, new byte[2]);
Require(line.SequenceEqual(new int[] { 465, 0, 1024, 1024, 1, 1, 1024, 0, 0, 1, 1, 0, 0, 0, 0, 0 }), "Analytic local line mismatch.");
var negativeShift = LegacyDirectionalWindowStatistics.Extract(stroke, 11, 1, -1, new byte[] { 0 }, new byte[] { 0xc0, 0 }, new byte[] { 1, 0 });
Require(negativeShift[2] == 0 && negativeShift[3] == 0 && negativeShift[4] == 1 && negativeShift[5] == 1 && negativeShift[15] == 512,
    "Negative-shift local unit, singleton boundaries or whole-glyph interior denominator mismatch.");
Require(LegacyDirectionalWindowStatistics.Extract(stroke, 11, 1, 0, [], new byte[2], new byte[2]).All(x => x == 0), "Empty window must emit zero raw statistics.");
Throws<ArgumentException>(() => LegacyDirectionalWindowStatistics.Extract(stroke, 11, 1, 0, new byte[] { 2 }, new byte[2], new byte[2]));
Throws<OperationCanceledException>(() => LegacyDirectionalWindowStatistics.Extract(stroke, 11, 1, 0, [], new byte[2], new byte[2], cancellationToken: new CancellationToken(true)));
var globals = new int[17]; globals[2] = 1; globals[0] = 512; globals[8] = -128;
var windows = Enumerable.Range(0, 16).Select(_ => new int[16]).ToArray(); windows[0][4] = 1; windows[1][9] = 2;
var minima = Enumerable.Repeat((short)-1024, 274).ToArray(); var maxima = Enumerable.Repeat((short)1024, 274).ToArray();
minima[0] = maxima[0] = 1024;
var transformed = LegacyDirectionalFeatureAssembly.TransformGeometry(globals, windows, minima, maxima);
Require(transformed.Length == 274 && transformed[0] == 1024 && transformed[1] == 0 && transformed[3] == -1024 && transformed[9] == -256 &&
    transformed[18 + 4] == 1024 && transformed[18 + 16 + 9] == 3072, "Global/local masks or covariance/count transforms mismatch.");
var baseline = new int[9]; baseline[3] = 0;
var assembled = LegacyDirectionalFeatureAssembly.AppendInput(transformed, baseline, Enumerable.Range(0, 28).Select(i => (short)i).ToArray(), 123, -456);
Require(assembled.Length == 313 && assembled[274] == -1024 && assembled[277] == 0 && assembled[283] == 0 && assembled[310] == 27 &&
    assembled[311] == 123 && assembled[312] == -456, "Baseline/context/boundary order mismatch.");
Throws<ArgumentException>(() => LegacyDirectionalFeatureAssembly.TransformGeometry(globals, windows, minima.AsSpan(1), maxima));
Throws<OperationCanceledException>(() => LegacyDirectionalFeatureAssembly.AppendInput(transformed, baseline, new short[28], 0, 0, new CancellationToken(true)));
Console.WriteLine("PASS own local statistics, global/window calibration, baseline/context/boundary channel order, negative shift, bounds and cancellation.313 recognition accuracy is not tested.");
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}."); }
