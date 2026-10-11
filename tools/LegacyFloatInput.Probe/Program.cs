using InkRecognition;
using System.Text.Json;
if (args.Length > 1) throw new ArgumentException("Supply at most one own float-control JSON.");
if (args.Length == 1)
{
    if (new FileInfo(args[0]).Length > 8 * 1024 * 1024) throw new ArgumentException("Controls exceed8MiB.");
    using var document = JsonDocument.Parse(File.ReadAllBytes(args[0])); int count = 0;
    foreach (var item in document.RootElement.EnumerateArray())
    {
        float[] values = item.GetProperty("values").EnumerateArray().Select(p => p.GetSingle()).ToArray();
        var expected = item.GetProperty("expected").EnumerateArray().Select(p => p.GetInt16());
        Require(LegacyFloatInputQuantization.Quantize(values, item.GetProperty("exponent").GetInt32()).SequenceEqual(expected), $"Float quantization comparison failed{count}."); count++;
    }
    Require(count > 0, "Empty controls."); Console.WriteLine($"PASS {count} independent float32 multiply/truncate/short-wrap comparisons.");
}
Require(LegacyFloatInputQuantization.Quantize([-1, -.99f, -.001f, 0, .001f, .99f, 1]).SequenceEqual(new short[] { -1024, -1013, -1, 0, 1, 1013, 1024 }), "Context float conversion must truncate, not round.");
Require(LegacyFloatInputQuantization.Quantize([32, 64, -32, -64]).SequenceEqual(new short[] { -32768, 0, -32768, 0 }), "Native signed-short wrapping failed.");
Require(LegacyFloatInputQuantization.Quantize([-2097152f])[0] == 0, "Representable int32 minimum failed.");
Throws<ArgumentException>(() => LegacyFloatInputQuantization.Quantize([]));
Throws<ArgumentException>(() => LegacyFloatInputQuantization.Quantize(new float[4097]));
foreach (float bad in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity, 2097152f, -2097153f })
    Throws<ArgumentException>(() => LegacyFloatInputQuantization.Quantize([bad]));
foreach (int badExponent in new[] { -1, 15, 31 }) Throws<ArgumentOutOfRangeException>(() => LegacyFloatInputQuantization.Quantize([1], badExponent));
Throws<OperationCanceledException>(() => LegacyFloatInputQuantization.Quantize([1], cancellationToken: new CancellationToken(true)));
Console.WriteLine("PASS own context truncation, short wrapping, int32 boundary, invalid float/exponent/size and cancellation. No model assets or handwriting-accuracy claims.");
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected{typeof(T).Name}."); }
