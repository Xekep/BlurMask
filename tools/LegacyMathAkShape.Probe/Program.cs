using System.Numerics;
using System.Text.Json;
using InkRecognition;

if (args.Length != 2) throw new ArgumentException("REPOSITORY FRESH_CAPTURE");
if (File.Exists(args[1])) throw new IOException("Fresh capture required.");
byte[] raw = File.ReadAllBytes(Path.Combine(args[0], "models", "math", "math-ak.res"));
var models = LegacyMathAkSelectedModels.PrepareActual(raw);
var profile = models.FeatureProfile; var prototypes = profile.ShapePrototypes;
var guards = new List<string>();
void Check(string name, bool pass) { if (!pass) throw new Exception(name); guards.Add(name); }
void Refuse<T>(string name, Action action) where T : Exception
{ try { action(); } catch (Exception e) when (e.GetType() == typeof(T)) { guards.Add(name); return; } throw new Exception(name); }
Vector2[][] Make(float[][][] xy) => xy.Select(s => s.Select(p => new Vector2(p[0], p[1])).ToArray()).ToArray();
var cases = new (string Name, float[][][] Xy)[]
{
    ("point", [[[3,4]]]),
    ("horizontal", [[[0,0],[10,0]]]),
    ("vertical", [[[0,0],[0,10]]]),
    ("triangle", [[[0,0],[2,1],[4,0]]]),
    ("reverse-triangle", [[[4,0],[2,1],[0,0]]]),
    ("corner", [[[0,0],[0,1],[1,1]]]),
    ("signed", [[[-4,-2],[-1,3],[5,-1]]]),
    ("repeated", [[[0,0],[0,0],[1,2],[1,2],[3,0]]]),
    ("two-strokes", [[[0,0],[2,2]],[[4,0],[4,3]]]),
    ("short-last-stroke", [[[0,0],[100,0]],[[0,1],[0,2]]]),
    ("point-strokes", [[[0,0]],[[3,0]],[[3,4]]]),
    ("all-identical", [[[2,2],[2,2]],[[2,2]]]),
    ("asymmetric", [[[0,0],[1,7],[2,4],[8,1],[10,10]]])
};
var captured = new List<object>();
foreach (var item in cases)
{
    var input = Make(item.Xy); var source = models.CreateGeometry(input);
    object result;
    try
    {
        var features = source.PrepareShapeFeatures();
        Check("owned-" + item.Name, source.OwnsShapeFeatures(features));
        var distances = Enumerable.Range(0, 259).Where(i => prototypes.GetAlternativeCount(i) != 0)
            .Select(i => new { label = i, values = features.MeasurePrototypeDistances(i) }).ToArray();
        result = new { values = features.CopyValues(), features.ResampledPointCount, features.ResampledStrokeCount, distances };
    }
    catch (NotSupportedException e) { result = new { refusal = e.Message }; }
    captured.Add(new { name = item.Name, xy = item.Xy, result });
}
var stable = models.CreateGeometry(Make(cases[3].Xy)); var feature = stable.PrepareShapeFeatures();
var expected = feature.CopyValues(); var changed = feature.CopyValues(); changed[0] ^= 1;
Check("feature-copy-owned", feature.CopyValues().SequenceEqual(expected));
var equal = models.CreateGeometry(Make(cases[3].Xy));
Check("equal-geometry-foreign", !equal.OwnsShapeFeatures(feature));
var foreign = LegacyMathAkFeatureProfile.PrepareActual(raw).CreateGeometry(Make(cases[3].Xy));
Check("equal-profile-foreign", !foreign.OwnsShapeFeatures(feature));
Check("same-shared-profile", ReferenceEquals(stable.FeatureProfile, profile));
Check("same-prototype-owner", ReferenceEquals(stable.FeatureProfile.ShapePrototypes, prototypes));
Check("payload-accounting", prototypes.RetainedPayloadBytes == 17896);
var copy = prototypes.CopyPrototypeValues(0, 0); short first = copy[0]; copy[0] ^= 1;
Check("prototype-copy-owned", prototypes.CopyPrototypeValues(0, 0)[0] == first);
var targets = prototypes.CopyTargets(0, 0); ushort target = targets[0]; targets[0] ^= 1;
Check("targets-copy-owned", prototypes.CopyTargets(0, 0)[0] == target);
float firstDistance = feature.MeasurePrototypeDistances(0)[0]; var ds = feature.MeasurePrototypeDistances(0); ds[0] = -1;
Check("distance-copy-owned", feature.MeasurePrototypeDistances(0)[0] == firstDistance);
Refuse<ArgumentOutOfRangeException>("negative-label", () => prototypes.GetAlternativeCount(-1));
Refuse<ArgumentOutOfRangeException>("label259", () => prototypes.GetAlternativeCount(259));
Refuse<ArgumentOutOfRangeException>("missing-alternative", () => prototypes.CopyPrototypeValues(1, 0));
Refuse<ArgumentOutOfRangeException>("negative-alternative", () => prototypes.CopyPrototypeValues(0, -1));
using (var cts = new CancellationTokenSource())
{
    cts.Cancel();
    try { stable.PrepareShapeFeatures(cts.Token); throw new Exception("precancel-feature"); }
    catch (OperationCanceledException e) { Check("precancel-feature-token", e.GetType() == typeof(OperationCanceledException) && e.CancellationToken == cts.Token); }
    try { feature.MeasurePrototypeDistances(0, cts.Token); throw new Exception("precancel-distance"); }
    catch (OperationCanceledException e) { Check("precancel-distance-token", e.GetType() == typeof(OperationCanceledException) && e.CancellationToken == cts.Token); }
}
var parallel = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() => stable.PrepareShapeFeatures().CopyValues())));
Check("parallel-private-inspection", parallel.All(p => p.SequenceEqual(expected)));
var foreignBytes = (byte[])raw.Clone(); foreignBytes[1772560] ^= 1;
Refuse<InvalidDataException>("foreign-source-resource", () => LegacyMathAkFeatureProfile.PrepareActual(foreignBytes));
Array.Clear(raw);
Check("cleared-caller-resource", stable.PrepareShapeFeatures().CopyValues().SequenceEqual(expected));
var protoRows = Enumerable.Range(0, 259).Select(label => new { label, entries = Enumerable.Range(0, prototypes.GetAlternativeCount(label))
    .Select(ordinal => new { ordinal, values = prototypes.CopyPrototypeValues(label, ordinal), targets = prototypes.CopyTargets(label, ordinal), factor = prototypes.GetFactor(label, ordinal) }).ToArray() }).ToArray();
var document = new { scope = "actual299 same-source geometry and immutable13A6 data only", modelExecutions = 0,
    cases = captured, prototypes = protoRows, guards, publicClassifierAuthority = false };
File.WriteAllText(args[1], JsonSerializer.Serialize(document));
Console.WriteLine($"PASS {captured.Count} cases/{guards.Count} guards/39 prototypes/0 NN");
