using InkRecognition;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;

if (args.Length != 2 || File.Exists(args[1])) throw new ArgumentException("REPOSITORY FRESH_CAPTURE");
byte[] resource = File.ReadAllBytes(Path.Combine(args[0], "models/math/math-ak.res"));
var profile = LegacyMathAkFeatureProfile.PrepareActual(resource);
var inputs = new (string Name, Vector2[] Points)[] {
    ("triangle", [new(0,0),new(2,1),new(4,0)]),
    ("horizontal", [new(0,0),new(40,0)]),
    ("vertical", [new(0,0),new(0,30)]),
    ("nonsquare", [new(0,0),new(3,8),new(4,2)]),
    ("turning", [new(0,0),new(5,2),new(1,6),new(8,3),new(2,0)]),
    ("duplicate", [new(0,0),new(0,0),new(2,1),new(4,0)]),
    ("negative", [new(8,8),new(6,4),new(2,3)])
};
var cases = new List<object>(); var guards = new List<string>();
void Need(bool value, string name) { if (!value) throw new Exception(name); guards.Add(name); }
foreach (var (name, xy) in inputs)
{
    var source = profile.CreateGeometry(new[] { xy });
    var bundle = source.PrepareUnguidedSingleStrokeDirectionalBundle();
    if (!source.OwnsDirectionalFeatureBundle(bundle) || !ReferenceEquals(source, bundle.Source)
        || !ReferenceEquals(bundle.Temporal.Source, source) || !ReferenceEquals(bundle.Spatial.Source, source)
        || !ReferenceEquals(bundle.Upper.Source, source)) throw new Exception("Owner mismatch");
    var temporal = bundle.Temporal.CopyValues(); var spatial = bundle.Spatial.CopyValues(); var upper = bundle.Upper.CopyValues();
    if (!temporal.SequenceEqual(source.PrepareUnguidedSingleStrokeDirectionalTemporal().CopyValues())
        || !spatial.SequenceEqual(source.PrepareUnguidedSingleStrokeDirectionalSpatial().CopyValues())
        || !upper.SequenceEqual(source.PrepareUnguidedSingleStrokeDirectionalUpper().CopyValues())) throw new Exception("Individual mismatch");
    if (bundle.Temporal.PreparedPointCount != bundle.Spatial.PreparedPointCount
        || bundle.Temporal.PreparedPointCount != bundle.Upper.PreparedPointCount) throw new Exception("Frame mismatch");
    var detached = bundle.Upper.CopyValues(); detached[0] ^= 1;
    if (!upper.SequenceEqual(bundle.Upper.CopyValues())) throw new Exception("Output alias");
    cases.Add(new { name, original = xy.Select(p => new[] { p.X, p.Y }).ToArray(), temporal, spatial, upper,
        preparedPointCount = bundle.Temporal.PreparedPointCount, bundle.HasCandidateAuthority, bundle.HasClassifierOutput });
}
var own = profile.CreateGeometry(new[] { inputs[0].Points });
var actual = own.PrepareUnguidedSingleStrokeDirectionalBundle();
var foreign = profile.CreateGeometry(new[] { inputs[0].Points });
Need(!foreign.OwnsDirectionalFeatureBundle(actual), "same-profile-foreign-source-refused");
Need(!LegacyMathAkFeatureProfile.PrepareActual(resource).CreateGeometry(new[] { inputs[0].Points }).OwnsDirectionalFeatureBundle(actual), "equal-data-foreign-profile-refused");
Need(!own.OwnsDirectionalFeatureBundle(null!), "null-bundle-refused");
Need(!actual.HasCandidateAuthority && !actual.HasClassifierOutput, "no-classifier-authority");
using (var cancel = new CancellationTokenSource())
{
    cancel.Cancel(); bool caught = false;
    try { own.PrepareUnguidedSingleStrokeDirectionalBundle(cancel.Token); }
    catch (OperationCanceledException e) { caught = e.GetType() == typeof(OperationCanceledException) && e.CancellationToken == cancel.Token; }
    Need(caught, "pre-cancel-exact-token");
}
foreach (var (name, strokes) in new (string, Vector2[][])[] {
    ("single-point-refused", [[new(0,0)]]),
    ("degenerate-refused", [[new(0,0),new(0,0)]]),
    ("multistroke-refused", [inputs[0].Points,inputs[0].Points]),
    ("over4096-refused", [Enumerable.Repeat(new Vector2(0,0),4097).ToArray()]) })
{
    bool caught = false;
    try { profile.CreateGeometry(strokes).PrepareUnguidedSingleStrokeDirectionalBundle(); }
    catch (NotSupportedException) { caught = true; }
    Need(caught, name);
}
resource.AsSpan().Clear();
Need(own.PrepareUnguidedSingleStrokeDirectionalBundle().Upper.CopyValues().SequenceEqual(actual.Upper.CopyValues()), "shared-profile-after-resource-clear");
Need(typeof(LegacyMathAkSourceGeometry.DirectionalFeatureBundle).GetConstructors().Length == 0, "no-public-bundle-constructor");

// Inspect call edges of the compiled implementation, without invoking private
// arithmetic or constructing a source/result through reflection.
var opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
    .Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null)!)
    .ToDictionary(op => unchecked((ushort)op.Value));
List<string> Calls(MethodInfo method)
{
    var result = new List<string>(); byte[] il = method.GetMethodBody()!.GetILAsByteArray()!;
    for (int p = 0; p < il.Length;)
    {
        ushort code = il[p++]; if (code == 0xfe) code = (ushort)(0xfe00 | il[p++]);
        var op = opcodes[code];
        if (op.OperandType == OperandType.InlineMethod)
        {
            var called = method.Module.ResolveMethod(BitConverter.ToInt32(il, p))!;
            result.Add(called.DeclaringType!.Name + "." + called.Name);
        }
        p += op.OperandType switch {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineI8 or OperandType.InlineR => 8,
            OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, p),
            _ => 4 };
    }
    return result;
}
const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
var type = typeof(LegacyMathAkSourceGeometry);
var entryCalls = Calls(type.GetMethod("PrepareUnguidedSingleStrokeDirectionalBundle", flags)!);
Need(entryCalls.Count(c => c.EndsWith(".PrepareDirectionalFrame")) == 1, "bundle-one-frame");
foreach (string family in new[] { "Temporal", "Spatial", "Upper" })
{
    string core = "PrepareDirectional" + family + "Core";
    Need(entryCalls.Count(c => c.EndsWith("." + core)) == 1, "bundle-one-" + family.ToLowerInvariant());
    var coreCalls = Calls(type.GetMethod(core, flags)!);
    Need(!coreCalls.Any(c => c.EndsWith(".PrepareDirectionalFrame") || c.StartsWith("LegacyDirectionalResampling.")
        || c.StartsWith("LegacyDirectionalIntersections.") || c.StartsWith("LegacyDirectional2B1Landmarks.")
        || c.StartsWith("LegacyDirectionalStatistics.")), "no-reprepare-" + family.ToLowerInvariant());
}
var frameCalls = Calls(type.GetMethod("PrepareDirectionalFrame", flags)!);
foreach (string call in new[] { "LegacyDirectionalResampling.Prepare", "LegacyDirectional2B1Landmarks.Extract", "LegacyDirectionalIntersections.Extract", "LegacyDirectionalStatistics.Extract" })
    Need(frameCalls.Count(c => c == call) == 1, "frame-once-" + call);
File.WriteAllText(args[1], JsonSerializer.Serialize(new { cases, guards, entryCalls, frameCalls, modelExecutions = 0,
    scope = "Shared directional preparation, source ownership and exact inputs only" }, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"PASS seven shared directional bundles /{guards.Count} guards /0NN");
