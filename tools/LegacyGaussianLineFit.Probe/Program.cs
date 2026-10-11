using InkRecognition;
using System.Text.Json;
if(args.Length != 1) throw new ArgumentException("Supply independent Gaussian line controls.");
if(new FileInfo(args[0]).Length > 16 * 1024 * 1024) throw new ArgumentException("Controls exceed 16 MiB.");
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0])); int fits = 0, extremaCases = 0, extremaCount = 0;
foreach(var item in document.RootElement.EnumerateArray())
{
    var lower = Points(item.GetProperty("lower")); var upper = Points(item.GetProperty("upper"));
    if(item.TryGetProperty("strokes", out var raw))
    {
        var strokes = raw.EnumerateArray().Select(Points).ToArray(); bool endpoints = item.GetProperty("endpoints").GetBoolean();
        var actual = LegacyLineExtrema.Extract(strokes, endpoints);
        Require(actual.Lower.SequenceEqual(lower) && actual.Upper.SequenceEqual(upper), "Extremum membership and order.");
        extremaCases++; extremaCount += lower.Length + upper.Length;
        Check(LegacyGaussianLineFit.Fit(strokes, endpoints), item);
    }
    Check(LegacyGaussianLineFit.FitExtrema(lower, upper), item); fits++;
}
Throws<ArgumentException>(() => LegacyGaussianLineFit.FitExtrema([], []));
Throws<ArgumentException>(() => LegacyGaussianLineFit.FitExtrema([new(float.NaN, 0)], []));
Throws<ArgumentException>(() => LegacyGaussianLineFit.FitExtrema(new InkPoint[LegacyGaussianLineFit.MaximumExtrema + 1], []));
Throws<ArgumentException>(() => LegacyLineExtrema.Extract([]));
Throws<ArgumentException>(() => LegacyLineExtrema.Extract([[]]));
Throws<ArgumentException>(() => LegacyLineExtrema.Extract([[new(0, float.PositiveInfinity)]]));
Throws<OperationCanceledException>(() => LegacyGaussianLineFit.FitExtrema([new(0, 0)], [], new(true)));
Throws<OperationCanceledException>(() => LegacyLineExtrema.Extract([[new(0, 0)]], cancellationToken: new(true)));
Console.WriteLine($"PASS {fits} independent Gaussian/ridge fits, {extremaCases} extremum populations ({extremaCount} observations); bounds, finite validation, cancellation.");
static InkPoint[] Points(JsonElement source) => source.EnumerateArray().Select(p => new InkPoint(p[0].GetSingle(), p[1].GetSingle())).ToArray();
static void Check(LegacyGaussianLineEstimate a, JsonElement item)
{
    var e = item.GetProperty("fit"); double[] values = [a.Guideline0, a.Guideline1, a.Guideline2, a.Guideline3, a.Slope];
    for(int i = 0; i < values.Length; i++)
    {
        double expected = e[i].GetDouble();
        Require(double.IsFinite(values[i]) && Math.Abs(values[i] - expected) <= 2e-7 * Math.Max(1, Math.Abs(expected)), $"Gaussian channel {i}: {values[i]} vs {expected}");
    }
    Require(a.Iterations is >= 1 and <= 20 && a.ExtremumCount == item.GetProperty("lower").GetArrayLength() + item.GetProperty("upper").GetArrayLength(), "Bounded iteration and extremum counts.");
}
static void Require(bool value, string message) { if(!value) throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T : Exception { try { action(); } catch(T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}"); }
