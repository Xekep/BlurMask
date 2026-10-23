using InkRecognition;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

if (args.Length != 3 || args[2] is not ("--prepared" or "--actual"))
    throw new ArgumentException("MATH_AK OUTPUT (--prepared|--actual)");
if (File.Exists(args[1])) throw new IOException("Output must be fresh.");
byte[] resource = File.ReadAllBytes(args[0]);
string inputSha = Convert.ToHexStringLower(SHA256.HashData(resource));
var forward = LegacyMathAkSymbolSequence.Read(resource, false);
var backward = LegacyMathAkSymbolSequence.Read(resource, true);
if (args[2] == "--actual")
{
    // Fixed source ordinals, not labels. No expected answer or selected output is fed back.
    int[] sourceOrdinals = [0, 1, 2];
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
    var results = new List<object>();
    foreach (var model in new[] { forward, backward })
    {
        var prepared = model.Prepare(sourceOrdinals, timeout.Token);
        var prediction = model.Evaluate(prepared, timeout.Token);
        results.Add(new { model.ReverseInterior, ids = prepared.CopyIds(), prepared.FrameCount,
            targets = Enumerable.Range(0, prepared.FrameCount).Select(prepared.TargetId).ToArray(),
            probabilities = prediction.CopyProbabilities(),
            selectedTargetProbabilities = Enumerable.Range(0, prediction.FrameCount)
                .Select(i => prediction.Probability(i, prediction.TargetId(i))).ToArray() });
    }
    File.WriteAllText(args[1], JsonSerializer.Serialize(new { inputSha, mode = "actual-next-symbol-not-recognition",
        actualNetworkForwards = 2, formulaConfidence = (float?)null, sourceOrdinals, results }, Options()));
    Console.WriteLine("Actual two-network primitive capture completed.");
    return;
}

var checks = new List<string>();
void Check(string name, bool condition) { if (!condition) throw new InvalidDataException(name); checks.Add(name); }
void Reject<T>(string name, Action action) where T : Exception
{
    try { action(); } catch (T e) when (e.GetType() == typeof(T)) { checks.Add(name); return; }
    throw new InvalidDataException("Expected exact " + typeof(T).Name + ": " + name);
}
using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
Check("empty-boundary-frame-target", forward.Prepare([]).CopyIds().SequenceEqual(new[] { 240, 240 }));
int[] ordinals = [0, 1, 2];
var owned = forward.Prepare(ordinals); var reversed = backward.Prepare(ordinals);
Check("forward-three-ids", owned.CopyIds().SequenceEqual(new[] { 240, 0, 1, 2, 240 }));
Check("reverse-interior-only", reversed.CopyIds().SequenceEqual(new[] { 240, 2, 1, 0, 240 }));
Check("four-input-frames", owned.FrameCount == 4 && owned.InputId(3) == 2);
Check("next-position-targets", Enumerable.Range(0, 4).Select(owned.TargetId).SequenceEqual(new[] { 0, 1, 2, 240 }));
ordinals[0] = 289; Check("caller-ordinal-snapshot", owned.InputId(1) == 0);
int[] detached = owned.CopyIds(); detached[0] = -1; Check("output-id-snapshot", owned.InputId(0) == 240);
Check("maximum4095", forward.Prepare(new int[4095]).FrameCount == 4096);
Reject<ArgumentException>("above4095", () => forward.Prepare(new int[4096]));
Reject<ArgumentException>("negative-ordinal", () => forward.Prepare(new[] { -1 }));
Reject<ArgumentException>("ordinal290", () => forward.Prepare(new[] { 290 }));
Reject<ArgumentOutOfRangeException>("input-negative", () => owned.InputId(-1));
Reject<ArgumentOutOfRangeException>("target-end", () => owned.TargetId(4));
Reject<ArgumentException>("foreign-record-owner-no-forward", () => backward.Evaluate(owned));
Reject<OperationCanceledException>("read-pre-cancel", () => LegacyMathAkSymbolSequence.Read(resource, false, cancelled.Token));
Reject<OperationCanceledException>("prepare-pre-cancel", () => forward.Prepare(new[] { 0 }, cancelled.Token));
Reject<OperationCanceledException>("evaluate-pre-cancel-no-forward", () => forward.Evaluate(owned, cancelled.Token));
byte[] corrupt = (byte[])resource.Clone(); corrupt[0] ^= 1;
Reject<InvalidDataException>("resource-sha", () => LegacyMathAkSymbolSequence.Read(corrupt, false));
Reject<InvalidDataException>("resource-truncated", () => LegacyMathAkSymbolSequence.Read(resource.AsSpan(1), false));
Array.Clear(resource);
Check("retained-model-after-input-mutation", forward.Prepare(new[] { 0, 1, 2 }).CopyIds().SequenceEqual(owned.CopyIds()));
int[][] parallel = Enumerable.Range(0, 8).AsParallel().Select(_ => forward.Prepare(new[] { 0, 1, 2 }).CopyIds()).ToArray();
Check("parallel-immutable-preparation", parallel.All(x => x.SequenceEqual(owned.CopyIds())));

// Synthetic-only executor regression: zero inputs and hand-written finite FP32 weights.
// Reflection does not expose model storage or install a production test seam.
Assembly assembly = typeof(LegacyMathAkSymbolSequence).Assembly;
Type networkType = assembly.GetType("InkRecognition.NativeNetwork", true)!;
Type affineType = assembly.GetType("InkRecognition.AffineWeights", true)!;
Type layerType = assembly.GetType("InkRecognition.LstmLayer", true)!;
Type policyType = assembly.GetType("InkRecognition.NativeNetworkNumericalPolicy", true)!;
ParameterInfo policyParameter = networkType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
    .Single().GetParameters()[4];
Check("historical-default-policy-metadata", policyParameter.HasDefaultValue && Convert.ToInt32(policyParameter.DefaultValue) == 0);
float[] weights = new float[241 * 8];
for (int i = 1; i < 241; i++) weights[i * 8 + 1] = -17.32868f;
object output = Activator.CreateInstance(affineType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
    null, new object?[] { 241, 1, 8, 1, weights, null, null, Array.Empty<float>(), Array.Empty<float>(), Array.Empty<float>() }, null)!;
float[] Synthetic(string policy)
{
    object network = Activator.CreateInstance(networkType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
        null, new object?[] { 1, Array.CreateInstance(layerType, 0), output, null, Enum.Parse(policyType, policy) }, null)!;
    return (float[])networkType.GetMethod("Run", BindingFlags.Instance | BindingFlags.NonPublic)!
        .Invoke(network, new object[] { new float[] { 0 }, 1, CancellationToken.None })!;
}
float[] historical = Synthetic("ExistingPortable"), scalar = Synthetic("SymbolSequenceScalar");
Check("historical-versus-sequential-different", historical[0] != scalar[0]);
Check("historical-normalized-finite", historical.All(float.IsFinite) && historical[0] > 0 && historical[0] < 1);
Check("scalar-sequential-leading-one", scalar[0] == 1f);
MethodInfo oldSigmoid = networkType.GetMethod("Sigmoid", BindingFlags.Static | BindingFlags.NonPublic)!;
Type scalarMath = assembly.GetType("InkRecognition.NativeSymbolSequenceScalarMath", true)!;
MethodInfo newSigmoid = scalarMath.GetMethod("Sigmoid", BindingFlags.Static | BindingFlags.NonPublic)!;
float Old(float x) => (float)oldSigmoid.Invoke(null, new object[] { x })!;
float New(float x) => (float)newSigmoid.Invoke(null, new object[] { x })!;
Check("unclamped-minus90-versus-old", New(-90) == 0 && Old(-90) > 0);
Check("signed-zero", New(0f) == .5f && New(-0f) == .5f);
Check("positive-negative-infinity", New(float.PositiveInfinity) == 1 && New(float.NegativeInfinity) == 0);
Check("nan-remains-nan", float.IsNaN(New(float.NaN)));
File.WriteAllText(args[1], JsonSerializer.Serialize(new { inputSha, mode = "prepared-and-synthetic-only",
    actualNetworkForwards = 0, syntheticNetworkForwards = 2, checks,
    prepared = new { forwardIds = owned.CopyIds(), reverseIds = reversed.CopyIds() },
    synthetic = new { historical, scalar, oldMinus90 = Old(-90), unclampedMinus90 = New(-90) } }, Options()));
Console.WriteLine($"PASS {checks.Count} prepared/synthetic checks; 0 actual network forwards.");

static JsonSerializerOptions Options() => new() { WriteIndented = true };
