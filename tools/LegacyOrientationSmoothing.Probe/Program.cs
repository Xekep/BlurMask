using InkRecognition;
using System.Text.Json;

if (args.Length is < 1 or > 2) throw new ArgumentException("Usage: LegacyOrientationSmoothing.Probe controls.json [ru-ak.res]");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0])); int fits = 0, preparations = 0, points = 0;
foreach (var item in controls.RootElement.GetProperty("fits").EnumerateArray())
{
    var actual = LegacyCubicStrokeSmoothing.Fit(item.GetProperty("values").EnumerateArray().Select(v => v.GetSingle()).ToArray(),
        item.GetProperty("lengths").EnumerateArray().Select(v => v.GetDouble()).ToArray(), item.GetProperty("penalty").GetDouble());
    var expected = item.GetProperty("coefficients");
    for (int i = 0; i < actual.Length; i++)
    {
        var c = actual[i]; double[] coefficients = [c.Value, c.Linear, c.Quadratic, c.Cubic];
        for (int k = 0; k < 4; k++) Near(coefficients[k], expected[i][k].GetDouble(), 2e-8);
    }
    fits++;
}
foreach (var item in controls.RootElement.GetProperty("preparations").EnumerateArray())
{
    var strokes = ReadStrokes(item.GetProperty("strokes")); var p = item.GetProperty("profile").EnumerateArray().Select(v => v.GetSingle()).ToArray();
    var profile = new LegacyOrientationSmoothingProfile(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7]);
    var metrics = item.GetProperty("metrics"); var sourceMetrics = new LegacyOrientationStrokeMetrics(metrics[0].GetSingle(), metrics[1].GetSingle());
    var actual = LegacyOrientationStrokePreparation.Prepare(strokes, profile, item.GetProperty("scale").GetSingle(), sourceMetrics);
    Near(actual.SmoothingWeight, item.GetProperty("weight").GetDouble(), 3e-6);
    Near(actual.Metrics.OrthogonalFraction, item.GetProperty("derived")[0].GetDouble(), 3e-6); Near(actual.Metrics.CornerFraction, item.GetProperty("derived")[1].GetDouble(), 3e-6);
    var expected = ReadStrokes(item.GetProperty("prepared"));
    for (int s = 0; s < actual.Strokes.Count; s++)
    {
        if (actual.Strokes[s].Points.Count != expected[s].Points.Count) throw new Exception($"Flattening count mismatch case{preparations},stroke{s}.");
        for (int i = 0; i < actual.Strokes[s].Points.Count; i++)
        { Near(actual.Strokes[s].Points[i].X, expected[s].Points[i].X, 5e-5); Near(actual.Strokes[s].Points[i].Y, expected[s].Points[i].Y, 5e-5); points++; }
    }
    preparations++;
}
ExpectInvalid(() => LegacyCubicStrokeSmoothing.Fit([0, 1, 2], [1, 1], 0));
ExpectInvalid(() => LegacyCubicStrokeSmoothing.Fit([0, 1, 2, 3], [1, 0, 1], 0));
ExpectInvalid(() => LegacyCubicStrokeSmoothing.Fit([0, float.NaN, 2, 3], [1, 1, 1], 0));
using var canceled = new CancellationTokenSource(); canceled.Cancel();
try { LegacyCubicStrokeSmoothing.Fit([0, 1, 2, 3], [1, 1, 1], 0, canceled.Token); throw new Exception("Cancellation ignored."); } catch (OperationCanceledException) { }
Console.WriteLine($"PASS child265 {fits} independent dense spline references, {preparations} full stroke preparations, {points} flattened points.");
if (args.Length == 2)
{
    var resource = File.ReadAllBytes(args[1]); var profile = LegacyOrientationSmoothingProfile.ReadV102Node(resource, 6021);
    var expected = new LegacyOrientationSmoothingProfile(.05f, .075f, .075f, .15f, .35f, .1f, .15f, .0001f);
    if (profile != expected) throw new Exception("Actual RU265 profile mismatch.");
    InkStroke[] own = [new(new InkPoint[] { new(0, 90), new(0, 10), new(25, 60), new(50, 10), new(50, 90) }), new(new InkPoint[] { new(65, 90), new(65, 10) }), new(new InkPoint[] { new(65, 90), new(115, 10) }), new(new InkPoint[] { new(115, 90), new(115, 10) }), new(new InkPoint[] { new(130, 90), new(130, 10), new(175, 10), new(185, 20), new(185, 45), new(175, 55), new(130, 55) })];
    var source = LegacyOrdinaryLineScaleSource.Prepare(own, resource, 96);
    float scale = LegacyLineOrientationProjection.ExtractScale(source.Source.Strokes, .95f).Value;
    //Both explicit modes are diagnostic; source219's actual metric writers remain separate evidence.
    foreach (var metrics in new LegacyOrientationStrokeMetrics[] { new(0, 0), new(-1, -1) })
    {
        var prepared = LegacyOrientationStrokePreparation.Prepare(source.Source.Strokes, profile, scale, metrics);
        var orientation = LegacyLineOrientationProfile.ReadResourceNode(resource, 5444);
        var ranked = LegacyLineOrientationProjection.Score(prepared.Strokes, LegacyLineOrientationProjection.CreateAngles(orientation), scale);
        Console.WriteLine($"Actual RU265→2d0 diagnostic own MIR (supplied metrics{metrics}):{prepared.Strokes.Sum(s => s.Points.Count)} points,weight={prepared.SmoothingWeight},topangles={string.Join(',', ranked.Take(5).Select(r => -90 + r.AngleIndex * 15))}");
    }
}
static InkStroke[] ReadStrokes(JsonElement element) => element.EnumerateArray().Select(s => new InkStroke(s.EnumerateArray().Select(p => new InkPoint(p[0].GetSingle(), p[1].GetSingle())).ToArray())).ToArray();
static void Near(double actual, double expected, double tolerance) { if (!double.IsFinite(actual) || Math.Abs(actual - expected) > tolerance * Math.Max(1, Math.Abs(expected))) throw new Exception($"Mismatch {actual:R} != {expected:R}"); }
static void ExpectInvalid(Action action) { try { action(); } catch (Exception e) when (e is ArgumentException or InvalidDataException) { return; } throw new Exception("Malformed accepted."); }
