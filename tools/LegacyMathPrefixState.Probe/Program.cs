using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
using InkRecognition.Math;

if (args.Length != 4) throw new ArgumentException("model.ikrmath rawMath2.res official-runtime-library capture.json");
if (!BitConverter.IsLittleEndian) throw new NotSupportedException("Little endian capture required.");
byte[] resource = File.ReadAllBytes(args[1]);
using var runtime = new OrtRuntime(Path.GetFullPath(args[2]));
using var run = runtime.CreateRun(CancellationToken.None);
using var stream = File.OpenRead(args[0]);
using var bundle = Math2Mode0ColdBundle.Read(runtime, stream, resource);
using var modelStream = File.OpenRead(args[0]);
var model = MathModel.Read(modelStream);
var features = MathPreprocessor.Process([
    new InkStroke([new InkPoint(0, 30), new(0, 0), new(10, 18), new(20, 0), new(20, 30)]),
    new InkStroke([new InkPoint(35, 0), new(35, 30)])]);
using var source = runtime.Tensor(features.Values, features.Frames, 1, 26);
using var downsampling = runtime.Tensor(Enumerable.Repeat(1L, features.Frames).ToArray(), features.Frames, 1);
using var ends = runtime.Tensor(features.StrokeEnds, 1, features.StrokeEnds.Length);
int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or InvalidOperationException or InvalidDataException or OperationCanceledException or NotSupportedException)
    { guards++; return; }
    throw new InvalidDataException("Required prefix/Step lifecycle guard did not reject.");
}
TensorCapture Tensor(string name, int type, long[] shape, float[]? floats, long[]? longs)
{
    byte[] bytes = new byte[(floats?.Length ?? longs!.Length) * (type == 1 ? 4 : 8)];
    Buffer.BlockCopy((Array?)floats ?? longs!, 0, bytes, 0, bytes.Length);
    return new(name, type, shape, Convert.ToBase64String(bytes));
}
TensorCapture[] Inputs(Math2Mode0PrefixContext owner) => owner.CopyTensorInputs()
    .Select(x => Tensor(x.Name, x.Type, x.Shape, x.Floats, x.Int64s)).ToArray();
TensorCapture[] Outputs(Math2Mode0PrefixStep step) => Enumerable.Range(0, 4).Select(i =>
{
    var value = step.CopyOutput(i);
    return Tensor(i.ToString(), 1, value.Shape, value.Values, null);
}).ToArray();
GenerationCapture Generation(Math2Mode0PrefixGeneration generation)
    => new(generation.Offset, generation.ActiveCount, generation.CopyPrimary(),
        generation.CopySecondary(), generation.CopyAncestry());
Math2Mode0PrefixContext Create(int width = 3)
    => Math2Mode0PrefixContext.Prepare(bundle, run, source, downsampling, ends, width);
Reject(() => Math2Mode0PrefixContext.Prepare(bundle, run, source, downsampling, ends, 0));
Reject(() => Math2Mode0PrefixContext.Prepare(bundle, run, source, downsampling, ends, 17));
using (var canceledPreparation = new CancellationTokenSource())
{
    canceledPreparation.Cancel();
    Reject(() => Math2Mode0PrefixContext.Prepare(bundle, run, source, downsampling, ends, 3, canceledPreparation.Token));
}

var runs = new List<RunCapture>();
foreach (var settings in new[] { (Width: 1, Delta: -1f), (Width: 3, Delta: 1000f), (Width: 3, Delta: -1f) })
{
    using var owner = Create(settings.Width);
    var initial = Inputs(owner); var initialGeneration = Generation(owner.CopyGeneration());
    var steps = new List<StepCapture>();
    Math2Mode0PrefixCandidates? oldCandidates = null;
    Math2Mode0PrefixSelection? oldSelection = null;
    for (int iteration = 0; iteration < 5; iteration++)
    {
        var before = Inputs(owner); var prefixBefore = Generation(owner.CopyGeneration());
        var step = owner.RunStep(); var outputs = Outputs(step);
        Reject(() => owner.RunStep());
        if (oldCandidates != null) Reject(() => oldCandidates.Select());
        if (oldSelection != null) Reject(() => owner.ApplySelected(oldSelection));
        var candidates = step.CreateCandidates(1f, settings.Delta);
        var selected = candidates.Select();
        if (selected.IsDiagnostic) throw new InvalidDataException("Actual sidecar provenance lost.");
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        Reject(() => owner.ApplySelected(selected, canceled.Token));
        var rows = selected.CopyRecords(); selected.CopyRecords().AsSpan().Fill(255);
        owner.CopyGeneration().CopyPrimary()[0].AsSpan().Fill(ushort.MaxValue);
        owner.ApplySelected(selected);
        Reject(() => step.CopyOutput(0));
        steps.Add(new(false, before, outputs, Convert.ToBase64String(rows), Inputs(owner), prefixBefore,
            Generation(owner.CopyGeneration()), Tensor("prior", 1, [prefixBefore.ActiveCount],
                iteration == 0 ? new float[1] : steps[^1].SelectedCosts(), null),
            Tensor("logs", 1, [prefixBefore.ActiveCount, 297], candidates.CopyLogProbabilities(), null),
            Tensor("costs", 1, [prefixBefore.ActiveCount, 297], candidates.CopyUnmaskedCosts(), null),
            Convert.ToBase64String(candidates.CopyInformation()), Convert.ToBase64String(candidates.CopyConditional()),
            Convert.ToBase64String(candidates.CopyHeapStorage()), Convert.ToBase64String(candidates.CopySortedStorage()),
            Convert.ToBase64String(candidates.CopyAfterEos()), Convert.ToBase64String(candidates.CopyAfterResort()),
            candidates.CopyCompletedScoreFacts().Select(x => new EosCapture(x.Rank, x.SourceBeam,
                x.CandidateCost, x.CompletedScore, x.Prefix)).ToArray()));
        oldCandidates = candidates; oldSelection = selected;
    }
    runs.Add(new("actual-logits/native-prepared-candidate-kernels", settings.Width, settings.Delta,
        initial, initialGeneration, steps.ToArray()));
}

//Explicit diagnostic parent/label selection over actual NN steps. Sidecars
//and costs still come from those steps; these are not observed beam winners.
using (var owner = Create())
{
    var initial = Inputs(owner); var initialGeneration = Generation(owner.CopyGeneration());
    var steps = new List<StepCapture>();
    foreach (int[] parents in new[] { new[] { 0, 0, 0 }, new[] { 2, 0, 2 }, new[] { 2, 1 }, new[] { 1, 0 }, new[] { 0 } })
    {
        var before = Inputs(owner); var prefixBefore = Generation(owner.CopyGeneration());
        var step = owner.RunStep(); var outputs = Outputs(step); var prior = step.CopyPriorCosts();
        var actual = step.CreateCandidates(1f, 1000f);
        var labels = parents.Select((_, i) => (ushort)(i == 0 ? 262 : 2 + i)).ToArray();
        Reject(() => step.BindDiagnosticParents([-1], [2]));
        Reject(() => step.BindDiagnosticParents([prefixBefore.ActiveCount], [2]));
        Reject(() => step.BindDiagnosticParents([0], [297]));
        Reject(() => step.BindDiagnosticParents([0, 0], [2]));
        var selected = step.BindDiagnosticParents(parents, labels);
        if (!selected.IsDiagnostic) throw new InvalidDataException("Diagnostic selection must remain explicit.");
        var rows = selected.CopyRecords();
        owner.ApplySelected(selected);
        steps.Add(new(true, before, outputs, Convert.ToBase64String(rows), Inputs(owner), prefixBefore,
            Generation(owner.CopyGeneration()), Tensor("prior", 1, [prior.Length], prior, null),
            Tensor("logs", 1, [prefixBefore.ActiveCount, 297], actual.CopyLogProbabilities(), null),
            Tensor("costs", 1, [prefixBefore.ActiveCount, 297], actual.CopyUnmaskedCosts(), null),
            Convert.ToBase64String(actual.CopyInformation()), Convert.ToBase64String(actual.CopyConditional()),
            "", "", "", "", Array.Empty<EosCapture>()));
    }
    runs.Add(new("explicit-diagnostic-parent-selections", 3, 1000f, initial, initialGeneration, steps.ToArray()));
}

using (var a = Create())
using (var b = Create())
{
    using var canceled = new CancellationTokenSource(); canceled.Cancel();
    Reject(() => a.RunStep(canceled.Token));
    var stepA = a.RunStep(); var stepB = b.RunStep();
    var candidatesA = stepA.CreateCandidates(1f, 1000f);
    var selectedA = candidatesA.Select(); var selectedB = stepB.CreateCandidates(1f, 1000f).Select();
    Reject(() => a.ApplySelected(selectedB)); Reject(() => b.ApplySelected(selectedA));
    Reject(() => stepA.CreateCandidates(1f, 1000f, canceled.Token));
    Reject(bundle.Dispose); Reject(runtime.Dispose); Reject(run.Dispose);
    a.ApplySelected(selectedA); b.ApplySelected(selectedB);
    Reject(() => candidatesA.Select()); Reject(() => a.ApplySelected(selectedA));
}

var failures = new List<FailureCapture>();
foreach (bool cancelAfterPrefix in new[] { false, true })
{
    using var owner = Create(); using var cancel = new CancellationTokenSource();
    var step = owner.RunStep(); var selected = step.CreateCandidates(1f, 1000f).Select();
    var before = Generation(owner.CopyGeneration()); var rows = selected.CopyRecords();
    Reject(() => owner.ApplySelectedDiagnostic(selected, () =>
    {
        if (cancelAfterPrefix) cancel.Cancel();
        else throw new InvalidDataException("Explicit fault after prefix writes.");
    }, cancel.Token));
    if (!owner.IsFailed) throw new InvalidDataException("A partial prefix update remained reusable.");
    failures.Add(new(cancelAfterPrefix, before, Convert.ToBase64String(rows), Generation(owner.CopyGeneration())));
    Reject(() => owner.RunStep()); Reject(() => owner.ApplySelected(selected)); Reject(() => owner.CopyTensorInputs());
}
using (var termination = new CancellationTokenSource())
using (var terminatedRun = runtime.CreateRun(termination.Token))
{
    using var owner = Math2Mode0PrefixContext.Prepare(bundle, terminatedRun, source, downsampling, ends, 3);
    termination.Cancel();
    Reject(() => owner.RunStep());
    if (!owner.IsFailed) throw new InvalidDataException("Failed actual ORT run remained reusable.");
    Reject(() => owner.RunStep()); Reject(() => owner.CopyTensorInputs());
    Reject(terminatedRun.Dispose);
    owner.Dispose(); terminatedRun.Dispose();
    Reject(() => owner.RunStep());
}

//Pure prepared row-layout checks expose alternate-sentinel/U16 semantics;
//they are not configured-alias inference or tensor selections.
var prepared = new List<PreparedCapture>();
var generation = Math2Mode0PrefixGeneration.Cold(3);
foreach (int[] parents in new[] { new[] { 0, 0, 0 }, new[] { 2, 0, 2 }, new[] { 1, 0 }, new[] { 1 } })
{
    byte[] rows = new byte[parents.Length * 48];
    for (int i = 0; i < parents.Length; i++)
    {
        var row = rows.AsSpan(i * 48, 48);
        BinaryPrimitives.WriteUInt64LittleEndian(row[8..], (ulong)(0x1002a + i));
        BinaryPrimitives.WriteUInt64LittleEndian(row[16..], (ulong)(0x100c8 + i));
        BinaryPrimitives.WriteUInt64LittleEndian(row[32..], i == 0 ? ulong.MaxValue : 0xffffffff0000ffffUL);
        BinaryPrimitives.WriteInt32LittleEndian(row[40..], parents[i]);
    }
    var before = Generation(generation); generation = generation.Append(rows, default);
    prepared.Add(new(before, Convert.ToBase64String(rows), Generation(generation)));
}
Reject(() => Math2Mode0PrefixGeneration.Cold(0)); Reject(() => Math2Mode0PrefixGeneration.Cold(17));
Reject(() => generation.Append(new byte[47], default));
byte[] invalidParent = new byte[48]; BinaryPrimitives.WriteInt32LittleEndian(invalidParent.AsSpan(40), generation.ActiveCount);
Reject(() => generation.Append(invalidParent, default));

var argmax = new List<ArgmaxCapture>();
uint random = 12345;
for (int i = 0; i < 320; i++)
{
    float[] values = new float[1 + i % 297];
    for (int j = 0; j < values.Length; j++)
    {
        random = unchecked(random * 1664525 + 1013904223);
        values[j] = (i % 8) switch { 0 => -float.MaxValue, 1 => float.NaN,
            2 => float.NegativeInfinity, 3 => 0f, _ => (int)(random % 2000) - 1000f };
    }
    if (i % 8 == 4) values[^1] = float.PositiveInfinity;
    byte[] bytes = new byte[values.Length * 4]; Buffer.BlockCopy(values, 0, bytes, 0, bytes.Length);
    argmax.Add(new(Convert.ToBase64String(bytes), Math2Mode0ActualSidecars.FirstStrictMaximum(values)));
}

var capture = new Capture(runtime.Version, model.Graphs.Select(x => Convert.ToHexStringLower(SHA256.HashData(x.Span))).ToArray(),
    Convert.ToHexStringLower(SHA256.HashData(resource)), features.Frames, runs.ToArray(), failures.ToArray(),
    prepared.ToArray(), argmax.ToArray(), guards);
File.WriteAllText(args[3], JsonSerializer.Serialize(capture, CaptureJson.Default.Capture));
Console.WriteLine($"PASS20 actual decoder steps/derived sidecars +owned prefix/tensor generations; {guards} guards; no full search loop");

record TensorCapture(string Name, int Type, long[] Shape, string Bytes);
record GenerationCapture(long Offset, int ActiveCount, ushort[][] Primary, ushort[][] Secondary, int[][] Ancestry);
record StepCapture(bool Diagnostic, TensorCapture[] Before, TensorCapture[] Outputs, string Selected48,
    TensorCapture[] After, GenerationCapture PrefixBefore, GenerationCapture PrefixAfter,
    TensorCapture Prior, TensorCapture Logs, TensorCapture Costs, string Information64, string Conditional32,
    string Heap, string Sorted, string AfterEos, string AfterResort, EosCapture[] CompletedScores)
{
    internal float[] SelectedCosts()
    {
        byte[] rows = Convert.FromBase64String(Selected48);
        return Enumerable.Range(0, rows.Length / 48).Select(i => BinaryPrimitives.ReadSingleLittleEndian(rows.AsSpan(i * 48))).ToArray();
    }
}
record RunCapture(string Kind, int Width, float Delta, TensorCapture[] Initial, GenerationCapture InitialGeneration, StepCapture[] Steps);
record FailureCapture(bool Canceled, GenerationCapture Before, string Selected48, GenerationCapture After);
record PreparedCapture(GenerationCapture Before, string Selected48, GenerationCapture After);
record ArgmaxCapture(string ValuesF32, long Index);
record EosCapture(int Rank, int SourceBeam, float CandidateCost, float CompletedScore, ushort[] Prefix);
record Capture(string Runtime, string[] Models, string Resource, int Frames, RunCapture[] Runs,
    FailureCapture[] Failures, PreparedCapture[] PreparedPlanes, ArgmaxCapture[] PreparedArgmax, int Guards);
[JsonSerializable(typeof(Capture))]
partial class CaptureJson : JsonSerializerContext;
