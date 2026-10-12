using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if (args.Length is < 1 or > 2) throw new ArgumentException("Usage: LegacyLineOrientation.Probe controls.json [ru-ak.res]");
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0, bins = 0, children = 0;
foreach (var item in document.RootElement.EnumerateArray())
{
    var strokes = item.GetProperty("strokes").EnumerateArray().Select(s => new InkStroke(s.EnumerateArray().Select(p => new InkPoint(p[0].GetSingle(), p[1].GetSingle())).ToArray())).ToArray();
    var p = item.GetProperty("profile").EnumerateArray().Select(v => v.GetSingle()).ToArray();
    var profile = new LegacyLineOrientationProfile(p[0], p[1], p[2], p[3], null, null, p[4], (byte)p[5], p[6], p[7], p[8], p[9] != 0, true);
    var scale = LegacyLineOrientationProjection.ExtractScale(strokes, profile.ScaleQuantile);
    Near(scale.Value, item.GetProperty("scale").GetSingle());
    ArrayNear(scale.HorizontalSamples.Span, item.GetProperty("samplesX")); ArrayNear(scale.VerticalSamples.Span, item.GetProperty("samplesY"));
    var angles = LegacyLineOrientationProjection.CreateAngles(profile); ArrayNear(angles, item.GetProperty("angles"));
    var scores = LegacyLineOrientationProjection.Score(strokes, angles, scale.Value);
    var expected = item.GetProperty("ranked");
    if (scores.Length != expected.GetArrayLength()) throw new Exception("Projection length mismatch.");
    for (int i = 0; i < scores.Length; i++) { if (scores[i].AngleIndex != expected[i][0].GetInt32()) throw new Exception($"Projection tie/order mismatch case{cases}."); Near(scores[i].Score, expected[i][1].GetSingle()); bins++; }
    foreach (var rotated in item.GetProperty("rotated").EnumerateArray())
    {
        var actual = LegacyLineOrientationProjection.Rotate(strokes, rotated.GetProperty("angle").GetSingle());
        var expectedStrokes = rotated.GetProperty("strokes");
        for (int s = 0; s < actual.Length; s++) for (int i = 0; i < actual[s].Points.Count; i++) { Near(actual[s].Points[i].X, expectedStrokes[s][i][0].GetSingle()); Near(actual[s].Points[i].Y, expectedStrokes[s][i][1].GetSingle()); }
        Near(LegacyLineOrientationPolicy.Coverage(actual, scale.Value), rotated.GetProperty("coverage").GetSingle());
        if (LegacyLineOrientationPolicy.RejectOrder(actual, scale.Value, profile) != rotated.GetProperty("reject").GetBoolean()) throw new Exception("Moment/order rejection mismatch.");
    }
    var decision = LegacyLineOrientationPolicy.Choose(strokes, profile, scale.Value, OwnChild);
    Near(decision.Degrees, item.GetProperty("winner").GetSingle()); Near(decision.Coverage, item.GetProperty("decisionCoverage").GetSingle());
    if (!decision.RetainedAngleIndices.Span.SequenceEqual(item.GetProperty("retained").EnumerateArray().Select(v => v.GetInt32()).ToArray())) throw new Exception($"Retained shortlist mismatch case{cases}: {string.Join(',', decision.RetainedAngleIndices.ToArray())} vs {item.GetProperty("retained")}");
    var costList = item.GetProperty("costs");
    if (decision.ChildCosts.Length != costList.GetArrayLength()) throw new Exception("Child shortlist count mismatch.");
    for (int i = 0; i < decision.ChildCosts.Length; i++)
    {
        var actual = decision.ChildCosts.Span[i]; var want = costList[i];
        if (actual.AngleIndex != want[0].GetInt32() || actual.WeightedCost != want[1].GetInt32()) throw new Exception("Exact weighted child cost mismatch."); children++;
    }
    cases++;
}
// Explicit integer edge controls retain unsigned quotient, signed cast and CVTT semantics.
if (LegacyLineOrientationPolicy.WeightChild(new(uint.MaxValue, 1, 1), .5f) != -1 ||
    LegacyLineOrientationPolicy.WeightChild(new(100, 3, 4), .5f) != 66 ||
    LegacyLineOrientationPolicy.WeightChild(new(1, 0, 4), .5f) != int.MinValue ||
    LegacyLineOrientationPolicy.WeightChild(new(7, 2, 0), 1) != 0) throw new Exception("Child unsigned/CVTT controls failed.");
var sampleProfile = new LegacyLineOrientationProfile(-90, 90, 15, .95f, null, null, .5f, 9, 2.5f, .2f, .3f, false, true);
var accumulatedAngles = LegacyLineOrientationProjection.CreateAngles(sampleProfile with { MinimumDegrees=-.2f, MaximumDegrees=1f, StepDegrees=.1f });
if (accumulatedAngles.Length != 13 || accumulatedAngles[^1] != 1f)
    throw new Exception("Native approximate endpoint rejected a float accumulation slightly past maximum.");
byte[] ownNode = OwnProfile(sampleProfile);
if (LegacyLineOrientationProfile.ReadResourceNode(ownNode, 0) != sampleProfile) throw new Exception("Own profile reader mismatch.");
for (int i = 0; i < ownNode.Length; i++) ExpectInvalid(() => LegacyLineOrientationProfile.ReadResourceNode(ownNode.AsSpan(0, i), 0));
var bad = ownNode.ToArray(); bad[^1] = 2; ExpectInvalid(() => LegacyLineOrientationProfile.ReadResourceNode(bad, 0));
ExpectInvalid(() => LegacyLineOrientationProjection.ExtractScale([new(new InkPoint[] { new(3, 4), new(3, 4) })], .95f));
ExpectInvalid(() => LegacyLineOrientationProjection.CreateAngles(sampleProfile with { StepDegrees = 17 }));
using var canceled = new CancellationTokenSource(); canceled.Cancel();
try { LegacyLineOrientationProjection.CreateAngles(sampleProfile, canceled.Token); throw new Exception("Cancellation ignored."); } catch (OperationCanceledException) { }
Console.WriteLine($"PASS orientation {cases} independent cases, {bins} projections, {children} child policy costs, resource/malformed/CVTT/cancellation.");
if (args.Length == 2)
{
    var resource = File.ReadAllBytes(args[1]); var profile = LegacyLineOrientationProfile.ReadResourceNode(resource, 5444);
    if (profile != sampleProfile with { PrimaryProcessorOffset = 5477, SecondaryProcessorOffset = 6021 }) throw new Exception("Actual RU2d0 profile mismatch.");
    Console.WriteLine("PASS actual RU2d0 reader:249@5477/265@6021,13 angles,.95scale,.5cardinality,9shortlist.");
    // Actual loaded configuration drives numerical stages over our own printed source.
    InkStroke[] ownMir = [new(new InkPoint[] { new(0, 90), new(0, 10), new(25, 60), new(50, 10), new(50, 90) }), new(new InkPoint[] { new(65, 90), new(65, 10) }), new(new InkPoint[] { new(65, 90), new(115, 10) }), new(new InkPoint[] { new(115, 90), new(115, 10) }), new(new InkPoint[] { new(130, 90), new(130, 10), new(175, 10), new(185, 20), new(185, 45), new(175, 55), new(130, 55) })];
    var source = LegacyOrdinaryLineScaleSource.Prepare(ownMir, resource, 96);
    var scale = LegacyLineOrientationProjection.ExtractScale(source.Source.Strokes, profile.ScaleQuantile);
    var projections = LegacyLineOrientationProjection.Score(source.Source.Strokes, LegacyLineOrientationProjection.CreateAngles(profile), scale.Value);
    Console.WriteLine($"Actual profile diagnostic (own ink, before child265/249):scale={scale.Value},topangles={string.Join(',', projections.Take(5).Select(v => -90 + v.AngleIndex * 15))}");
}

static LegacyOrientationChildCost OwnChild(IReadOnlyList<InkStroke> strokes, float scale, CancellationToken cancellationToken)
{
    cancellationToken.ThrowIfCancellationRequested(); float sum = 0; uint count = 0;
    foreach (var stroke in strokes) foreach (var p in stroke.Points) { sum += MathF.Abs(p.X) * .25f + MathF.Abs(p.Y) * .75f; count++; }
    return new((uint)MathF.Round(sum, MidpointRounding.AwayFromZero), count, (ulong)strokes.Count);
}
static void Near(float actual, float expected) { if (!float.IsFinite(actual) || MathF.Abs(actual - expected) > 3e-5f * MathF.Max(1, MathF.Abs(expected))) throw new Exception($"Numerical mismatch {actual} != {expected}"); }
static void ArrayNear(ReadOnlySpan<float> actual, JsonElement expected) { if (actual.Length != expected.GetArrayLength()) throw new Exception("Array count mismatch."); for (int i = 0; i < actual.Length; i++) Near(actual[i], expected[i].GetSingle()); }
static void ExpectInvalid(Action action) { try { action(); } catch (Exception e) when (e is ArgumentException or InvalidDataException) { return; } throw new Exception("Malformed input accepted."); }
static byte[] OwnProfile(LegacyLineOrientationProfile p)
{
    using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
    writer.Write(0x0101fe90u); writer.Write(0u); writer.Write(0x2d0u); writer.Write((ushort)0); writer.Write((ushort)0x100);
    writer.Write(p.MinimumDegrees); writer.Write(p.MaximumDegrees); writer.Write(p.StepDegrees); writer.Write(p.ScaleQuantile); writer.Write((byte)0); writer.Write((byte)0);
    writer.Write(p.CostCardinalityExponent); writer.Write(p.ShortlistCount); writer.Write(p.CoverageThreshold); writer.Write(p.MinimumForwardFraction); writer.Write(p.MaximumRowJumpFraction); writer.Write(p.ReverseHorizontalOrder); writer.Write(p.FinalFlag);
    byte[] bytes = stream.ToArray(); BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), (uint)bytes.Length - 8); return bytes;
}
