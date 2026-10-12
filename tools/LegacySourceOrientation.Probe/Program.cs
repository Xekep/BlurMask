using InkRecognition;
using System.Text.Json;

if (args.Length is < 1 or > 2) throw new ArgumentException("Usage: LegacySourceOrientation.Probe controls.json [ru-ak.res]");
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0])); int cases = 0, different = 0;
foreach (var item in document.RootElement.EnumerateArray())
{
    var raw = Read(item.GetProperty("strokes"));
    var source = LegacyOrdinarySourceInk.Prepare(raw, new(0x135, false, 0, 0));
    var metrics = LegacySourceOrientationPreparation.ExtractMetrics(source);
    Near(metrics.OrthogonalFraction, item.GetProperty("metrics")[0].GetSingle()); Near(metrics.CornerFraction, item.GetProperty("metrics")[1].GetSingle());
    var tolerant = LegacyOrientationStrokePreparation.ExtractMetrics(source.Strokes);
    if (metrics != tolerant) different++;
    //Own actual producer composition, not averaging per-stroke metadata.
    var orientation = new LegacyLineOrientationProfile(-90, 90, 15, .95f, null, null, .5f, 9, 2.5f, .2f, .3f, false, true);
    if (item.GetProperty("compose").GetBoolean())
    {
        var prepared = LegacySourceOrientationPreparation.Prepare(source, orientation, null);
        if (!ReferenceEquals(prepared.Source, source) || !ReferenceEquals(prepared.Prepared.Strokes, source.Strokes) || prepared.SourceMetrics != metrics)
            throw new Exception("Original source/chronology scope changed.");
    }
    cases++;
}
if (different < 16) throw new Exception("Near-equal source/fallback distinction not covered.");
using var canceled = new CancellationTokenSource(); canceled.Cancel();
try { LegacySourceOrientationPreparation.ExtractMetrics(null!, canceled.Token); throw new Exception("Null input accepted."); } catch (ArgumentNullException) { }
var validSource = LegacyOrdinarySourceInk.Prepare([new(new InkPoint[] { new(0,0), new(0,10) })], new(0x135,false,0,0));
try { LegacySourceOrientationPreparation.ExtractMetrics(validSource,canceled.Token); throw new Exception("Metric cancellation ignored."); } catch(OperationCanceledException) { }
var guardProfile = new LegacyLineOrientationProfile(-90,90,15,.95f,null,null,.5f,9,2.5f,.2f,.3f,false,true);
try { LegacySourceOrientationPreparation.Prepare(validSource,guardProfile,null,canceled.Token); throw new Exception("Preparation cancellation ignored."); } catch(OperationCanceledException) { }
try { LegacySourceOrientationPreparation.Prepare(validSource,guardProfile with { SecondaryProcessorOffset=6021 },null); throw new Exception("Missing enabled smoothing profile accepted."); } catch(ArgumentException) { }
Console.WriteLine($"PASS source15abe0 {cases} independent raw-ingress→exact metrics cases, {different} differing tolerant fallback cases.");
if (args.Length == 2)
{
    var bytes = File.ReadAllBytes(args[1]); var orientation = LegacyLineOrientationProfile.ReadResourceNode(bytes, 5444);
    var smoothing = LegacyOrientationSmoothingProfile.ReadV102Node(bytes, orientation.SecondaryProcessorOffset!.Value);
    InkStroke[] own = [new(new InkPoint[] { new(0, 90), new(0, 10), new(25, 60), new(50, 10), new(50, 90) }), new(new InkPoint[] { new(65, 90), new(65, 10) }), new(new InkPoint[] { new(65, 90), new(115, 10) }), new(new InkPoint[] { new(115, 90), new(115, 10) }), new(new InkPoint[] { new(130, 90), new(130, 10), new(175, 10), new(185, 20), new(185, 45), new(175, 55), new(130, 55) })];
    foreach (float rotate in new float[] { 0, 90, -90 })
    {
        var raw = LegacyLineOrientationProjection.Rotate(own, rotate);
        var source = LegacyOrdinaryLineScaleSource.Prepare(raw, bytes, 96);
        var prepared = LegacySourceOrientationPreparation.Prepare(source.Source, orientation, smoothing);
        if (!ReferenceEquals(prepared.Source,source.Source) || prepared.Prepared.Strokes.Sum(s=>s.Points.Count)!=54 ||
            -90+prepared.Projections.Span[0].AngleIndex*15 != -rotate)
            throw new Exception("Own rotated source producer/smoothing/projection diagnostic changed.");
        Console.WriteLine($"Actual RU source→exactmetrics→265→2d0 own MIR rotated{rotate}:scale={prepared.Scale.Value},metrics={prepared.SourceMetrics},weight={prepared.Prepared.SmoothingWeight},points={prepared.Prepared.Strokes.Sum(s => s.Points.Count)},topangles={string.Join(',', prepared.Projections.Span.ToArray().Take(3).Select(v => -90 + v.AngleIndex * 15))}");
    }
}
static InkStroke[] Read(JsonElement strokes) => strokes.EnumerateArray().Select(s => new InkStroke(s.EnumerateArray().Select(p => new InkPoint(p[0].GetSingle(), p[1].GetSingle())).ToArray())).ToArray();
static void Near(float a, float b) { if (MathF.Abs(a - b) > 1e-6f) throw new Exception($"Metric mismatch {a} != {b}."); }
