using InkRecognition;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

if (args.Length != 3 || args[2] is not ("--prepared" or "--actual" or "--score-controls"))
    throw new ArgumentException("MATH_AK OUTPUT (--prepared|--actual|--score-controls)");
if (File.Exists(args[1])) throw new IOException("Output must be fresh.");
byte[] resource = File.ReadAllBytes(args[0]);
string inputSha = Convert.ToHexStringLower(SHA256.HashData(resource));
var forward = LegacyMathAkSymbolSequence.Read(resource, false);
var backward = LegacyMathAkSymbolSequence.Read(resource, true);
if (args[2] == "--score-controls")
{
    Type helper = typeof(LegacyMathAkSymbolSequence).Assembly.GetType("InkRecognition.LegacyMathSymbolSequenceCost", true)!;
    MethodInfo accumulate = helper.GetMethod("AccumulateToken", BindingFlags.Static | BindingFlags.NonPublic)!;
    float Token(float cost, float probability, int target, float factor) =>
        (float)accumulate.Invoke(null, new object[] { cost, probability, target, factor })!;
    float factor = forward.IgnoreFactor;
    var cases = new List<object>();
    foreach (var x in new (float Running, float Probability, int Target)[] {
        (0,1,0), (0,.5f,0), (0,.5f,239), (0,.5f,240), (0,0,0),
        (0,BitConverter.Int32BitsToSingle(0x00800000),0),
        (0,BitConverter.Int32BitsToSingle(0x007fffff),0),
        (0,BitConverter.Int32BitsToSingle(0x00800000),239), (0,-0f,0),
        (0,BitConverter.Int32BitsToSingle(0x00800000)*64,239), (16777216,.5f,0), (0,1,239) })
        cases.Add(new { running = x.Running, probability = x.Probability, target = x.Target,
            result = Token(x.Running, x.Probability, x.Target, factor) });
    var guards = new List<string>();
    void Refuse(string name, Action action)
    {
        try { action(); }
        catch (TargetInvocationException e) when (e.InnerException?.GetType() == typeof(ArgumentException))
        { guards.Add(name); return; }
        throw new InvalidDataException(name);
    }
    Refuse("negative-probability", () => Token(0,-.1f,0,factor));
    Refuse("probability-above-one", () => Token(0,1.1f,0,factor));
    Refuse("nan-probability", () => Token(0,float.NaN,0,factor));
    Refuse("infinite-probability", () => Token(0,float.PositiveInfinity,0,factor));
    Refuse("target-below-zero", () => Token(0,.5f,-1,factor));
    Refuse("target-above240", () => Token(0,.5f,241,factor));
    Refuse("zero-factor", () => Token(0,.5f,239,0));
    Refuse("nan-factor", () => Token(0,.5f,239,float.NaN));
    MethodInfo same = typeof(LegacyMathAkSymbolSequence.PreparedSymbols).GetMethod("SameSourceOrdinals", BindingFlags.Instance | BindingFlags.NonPublic)!;
    var aliasA = forward.Prepare(new[] { 40 }); var aliasB = backward.Prepare(new[] { 118 });
    if (!aliasA.CopyIds().SequenceEqual(aliasB.CopyIds()) || (bool)same.Invoke(aliasA,new object[] { aliasB })!)
        throw new InvalidDataException("Different ordinals must not bind merely by equal mapped IDs.");
    guards.Add("same-mapped-ids-not-source-identity");
    if (!(bool)same.Invoke(aliasA,new object[] { backward.Prepare(new[] { 40 }) })!)
        throw new InvalidDataException("Same source ordinals rejected.");
    guards.Add("exact-source-ordinals-match");
    float first = 0, second = 0;
    for (int i = 0; i < 4096; i++) first = Token(first,0,0,factor);
    float continuous = first;
    for (int i = 0; i < 4096; i++) { continuous = Token(continuous,.9999f,0,factor); second = Token(second,.9999f,0,factor); }
    float jointMean = continuous / 2f, separateMean = (first + second) / 2f;
    if (jointMean == separateMean) throw new InvalidDataException("Synthetic order witness failed.");
    File.WriteAllText(args[1], JsonSerializer.Serialize(new { inputSha, mode = "pure-rescore-controls",
        actualNetworkForwards = 0, forward.IgnoreOrdinalCount, forward.IgnoreFactor,
        backwardFactor = backward.IgnoreFactor, cases, guards,
        ordered = new { countPerRecord = 4096, firstProbability = 0f, secondProbability = .9999f,
            first, second, jointMean, separateMean } }, Options()));
    Console.WriteLine($"PASS {cases.Count} scalar cases/{guards.Count} guards/ordered accumulation witness; 0 forwards.");
    return;
}
if (args[2] == "--actual")
{
    // Fixed source ordinals, not labels. No expected answer or selected output is fed back.
    int[] sourceOrdinals = [0, 1, 2];
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
    var results = new List<object>();
    var predictions = new List<LegacyMathAkSymbolSequence.Prediction>();
    foreach (var model in new[] { forward, backward })
    {
        var prepared = model.Prepare(sourceOrdinals, timeout.Token);
        var prediction = model.Evaluate(prepared, timeout.Token);
        predictions.Add(prediction);
        results.Add(new { model.ReverseInterior, ids = prepared.CopyIds(), prepared.FrameCount,
            targets = Enumerable.Range(0, prepared.FrameCount).Select(prepared.TargetId).ToArray(),
            probabilities = prediction.CopyProbabilities(),
            selectedTargetProbabilities = Enumerable.Range(0, prediction.FrameCount)
                .Select(i => prediction.Probability(i, prediction.TargetId(i))).ToArray(),
            recordCost = model.ScoreRecord(prediction, timeout.Token) });
    }
    File.WriteAllText(args[1], JsonSerializer.Serialize(new { inputSha, mode = "actual-next-symbol-not-recognition",
        actualNetworkForwards = 2, formulaConfidence = (float?)null, sourceOrdinals, results,
        pairCost = LegacyMathAkSymbolSequence.ScorePair(predictions[0], predictions[1], timeout.Token) }, Options()));
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
