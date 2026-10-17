using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
using InkRecognition.Math;

if (args.Length != 4) throw new ArgumentException("model.ikrmath rawMath2.res official-runtime-library capture.json");
if (!BitConverter.IsLittleEndian) throw new NotSupportedException("Little endian capture required.");
using var stream = File.OpenRead(args[0]);
var model = MathModel.Read(stream);
var resource = File.ReadAllBytes(args[1]);
var gate = LegacyMath2ObservableTokens.BindBundledResource(resource);
using var runtime = new OrtRuntime(Path.GetFullPath(args[2]));
using var run = runtime.CreateRun(CancellationToken.None);
using var models = new Math2Mode0TensorModels(runtime, model);
var strokes = new InkStroke[]
{
    new(new InkPoint[] { new(0, 30), new(0, 0), new(10, 18), new(20, 0), new(20, 30) }),
    new(new InkPoint[] { new(35, 0), new(35, 30) })
};
var features = MathPreprocessor.Process(strokes);
using var src = runtime.Tensor(features.Values, features.Frames, 1, 26);
using var downsampling = runtime.Tensor(Enumerable.Repeat(1L, features.Frames).ToArray(), features.Frames, 1);
using var ends = runtime.Tensor(features.StrokeEnds, 1, features.StrokeEnds.Length);
int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or InvalidOperationException or InvalidDataException or OperationCanceledException or NotSupportedException)
    { guards++; return; }
    throw new InvalidDataException("Required owner/lifecycle guard did not reject.");
}
TensorCapture Capture(string name, int type, long[] shape, float[]? floats, long[]? longs)
{
    byte[] bytes = new byte[checked((floats?.Length ?? longs!.Length) * (type == 1 ? 4 : 8))];
    Buffer.BlockCopy((Array?)floats ?? longs!, 0, bytes, 0, bytes.Length);
    return new(name, type, shape, Convert.ToBase64String(bytes));
}
TensorCapture[] Inputs(Math2Mode0TensorState state) => state.CopyInputs()
    .Select(x => Capture(x.Name, x.Type, x.Shape, x.Floats, x.Int64s)).ToArray();
TensorCapture[] Outputs(Math2Mode0DecoderStep step) => Enumerable.Range(0, 4).Select(i =>
{
    var x = step.CopyOutput(i);
    return Capture(new[] { "reco_output", "attn_seg", "state_out_k", "state_out_v" }[i], 1, x.Shape, x.Values, null);
}).ToArray();
Math2Mode0TensorState Create(bool force)
{
    using var encoder = models.RunEncoder(run, src, downsampling, ends);
    return Math2Mode0TensorState.Prepare(models, encoder, new(gate, force));
}
byte[] Rows(Math2Mode0DecoderStep step, int[] parents, bool forceOneStructural)
{
    var logits = step.CopyOutput(0).Values;
    byte[] records = new byte[parents.Length * 48];
    for (int i = 0; i < parents.Length; i++)
    {
        int parent = parents[i], chosen = 2;
        for (int symbol = 3; symbol < 297; symbol++)
            if (logits[parent * 297 + symbol] > logits[parent * 297 + chosen]) chosen = symbol;
        //Explicit prepared structural coverage, not a forced transcription.
        if (forceOneStructural && i == 0) chosen = 262;
        var row = records.AsSpan(i * 48, 48);
        BinaryPrimitives.WriteUInt64LittleEndian(row[8..], (ulong)chosen);
        BinaryPrimitives.WriteUInt64LittleEndian(row[32..], ulong.MaxValue);
        BinaryPrimitives.WriteInt32LittleEndian(row[40..], parent);
    }
    return records;
}
var captures = new List<StateRun>();
foreach (bool force in new[] { false, true })
{
    using var encoder = models.RunEncoder(run, src, downsampling, ends);
    var encoded = Enumerable.Range(0, 5).Select(i =>
    {
        var x = encoder.CopyOutput(i);
        return Capture(i.ToString(), x.Type, x.Shape, x.Floats, x.Int64s);
    }).ToArray();
    using var canceled = new CancellationTokenSource(); canceled.Cancel();
    Reject(() => Math2Mode0TensorState.Prepare(models, encoder, new(gate, force), canceled.Token));
    using var state = Math2Mode0TensorState.Prepare(models, encoder, new(gate, force));
    Reject(() => encoder.CopyOutput(0));
    Reject(() => Math2Mode0TensorState.Prepare(models, encoder, new(gate, force)));
    Reject(models.Dispose);
    Reject(runtime.Dispose);
    Reject(run.Dispose);
    var initial = Inputs(state);
    var steps = new List<StepCapture>();
    Math2Mode0SelectedRows? stale = null;
    foreach (var parents in new[] { new[] { 0, 0, 0 }, new[] { 2, 0, 2 }, new[] { 2, 1 }, new[] { 1, 0 }, new[] { 0 } })
    {
        var before = Inputs(state);
        var step = state.RunStep();
        var outputs = Outputs(step);
        Reject(() => state.RunStep());
        if (stale != null) Reject(() => state.ApplySelected(stale));
        byte[] records = Rows(step, parents, true);
        var bad = records.ToArray(); BinaryPrimitives.WriteInt32LittleEndian(bad.AsSpan(40), state.BeamCount);
        Reject(() => step.BindPreparedRows(bad));
        bad = records.ToArray(); BinaryPrimitives.WriteUInt64LittleEndian(bad.AsSpan(8), 297);
        Reject(() => step.BindPreparedRows(bad));
        Reject(() => step.BindPreparedRows(new byte[49]));
        var selected = step.BindPreparedRows(records);
        //Copies cannot rewrite the selected row ownership or parsed fields.
        selected.CopyRecords().AsSpan().Fill(255);
        selected.CopyParents().AsSpan().Fill(-1);
        state.ApplySelected(selected);
        stale = selected;
        Reject(() => step.CopyOutput(0));
        steps.Add(new(step.Offset, step.BeamCount, state.BeamCount, before, outputs,
            Convert.ToBase64String(records), Inputs(state)));
    }
    captures.Add(new(force, state.MaximumPredictionCount, encoded, initial, steps.ToArray()));
}

//Actual step -> separately proven portable score adapter -> native prepared
//heap/sort/EOS -> same-state cache update. Controller/sidecars remain prepared.
var portableSteps = new List<PortableStepCapture>();
TensorCapture[] portableInitial;
using (var state = Create(true))
{
    portableInitial = Inputs(state);
    ushort[][] prefixes = [Array.Empty<ushort>()];
    for (int iteration = 0; iteration < 4; iteration++)
    {
        var before = Inputs(state);
        var step = state.RunStep();
        var outputs = Outputs(step);
        var prior = state.CopyPriorCosts(step);
        var candidates = step.CreatePortableCandidates(3, 1f, 1000f,
            new byte[state.BeamCount * 8], new byte[state.BeamCount * 4], prefixes);
        var selected = candidates.Select();
        if (selected.IsExplicitPreparedSelection) throw new InvalidDataException("Actual score provenance lost.");
        var records = selected.CopyRecords();
        var nextPrefixes = selected.CopyParents().Select((parent, i) =>
            prefixes[parent].Concat(new[] { (ushort)selected.CopyLabels()[i] }).ToArray()).ToArray();
        var stage = new PortableStepCapture(new(step.Offset, step.BeamCount, selected.Count, before, outputs,
            Convert.ToBase64String(records), Array.Empty<TensorCapture>()),
            Capture("prior", 1, [prior.Length], prior, null),
            Capture("logs", 1, [state.BeamCount, 297], candidates.CopyLogProbabilities(), null),
            Capture("costs", 1, [state.BeamCount, 297], candidates.CopyUnmaskedCosts(), null),
            Convert.ToBase64String(candidates.CopyHeapStorage()), Convert.ToBase64String(candidates.CopySortedStorage()),
            Convert.ToBase64String(candidates.CopyAfterEos()), Convert.ToBase64String(candidates.CopyAfterResort()));
        state.ApplySelected(selected);
        portableSteps.Add(stage with { Step = stage.Step with { After = Inputs(state) } });
        Reject(() => candidates.Select()); Reject(() => state.ApplySelected(selected));
        prefixes = nextPrefixes;
    }
}
using (var first = Create(false))
using (var second = Create(false))
{
    var a = first.RunStep(); var b = second.RunStep();
    var selected = b.BindPreparedRows(Rows(b, [0], false));
    Reject(() => first.ApplySelected(selected));
    var portable = a.CreatePortableCandidates(1, 1f, 1000f, new byte[8], new byte[4], new ushort[][] { [] });
    Reject(() => second.ApplySelected(portable.Select()));
    first.ApplySelected(portable.Select());
    second.ApplySelected(selected);
}
using (var encoder = models.RunEncoder(run, src, downsampling, ends))
using (var foreignModels = new Math2Mode0TensorModels(runtime, model))
{
    Reject(models.Dispose); Reject(run.Dispose);
    Reject(() => Math2Mode0TensorState.Prepare(foreignModels, encoder, new(gate, false)));
    using var valid = Math2Mode0TensorState.Prepare(models, encoder, new(gate, false));
}
using (var foreignRuntime = new OrtRuntime(Path.GetFullPath(args[2])))
using (var wrong = foreignRuntime.Tensor([0f], 1))
    Reject(() => models.RunEncoder(run, wrong, downsampling, ends));
using (var tensor = runtime.Tensor([0f], 1)) Reject(() => tensor.Int64s());
using (var tensor = runtime.Tensor([42L], 1))
{
    Reject(() => tensor.Floats());
    if (tensor.Int64s().Values[0] != 42) throw new InvalidDataException("I64 reader differs.");
    tensor.Dispose(); Reject(() => tensor.Describe());
}
using (var state = Create(false))
using (var cancel = new CancellationTokenSource())
{
    var step = state.RunStep(); var selected = step.BindPreparedRows(Rows(step, [0], false));
    cancel.Cancel(); Reject(() => state.ApplySelected(selected, cancel.Token));
    Reject(() => state.RunStep()); Reject(() => state.CopyInputs()); Reject(() => step.CopyOutput(0));
}
using (var state = Create(false))
using (var cancel = new CancellationTokenSource())
{
    cancel.Cancel(); Reject(() => state.RunStep(cancel.Token)); Reject(() => state.RunStep());
}
using (var state = Create(false))
{
    state.Dispose(); state.Dispose(); Reject(() => state.RunStep());
}
using (var failureCancellation = new CancellationTokenSource())
using (var failureRun = runtime.CreateRun(failureCancellation.Token))
using (var failureModels = new Math2Mode0TensorModels(runtime, model))
using (var encoder = failureModels.RunEncoder(failureRun, src, downsampling, ends))
{
    failureCancellation.Cancel();
    Reject(() => Math2Mode0TensorState.Prepare(failureModels, encoder, new(gate, false)));
    Reject(() => encoder.CopyOutput(0));
    //Failure consumed/released the encoder's run/model leases as well as all
    //temporary initializer handles. Early disposal now succeeds.
    failureModels.Dispose(); failureRun.Dispose();
}
using (var failureCancellation = new CancellationTokenSource())
using (var failureRun = runtime.CreateRun(failureCancellation.Token))
using (var encoder = models.RunEncoder(failureRun, src, downsampling, ends))
using (var state = Math2Mode0TensorState.Prepare(models, encoder, new(gate, false)))
{
    failureCancellation.Cancel();
    Reject(() => state.RunStep()); Reject(() => state.RunStep()); Reject(() => state.CopyInputs());
}
using (var closedRun = runtime.CreateRun(CancellationToken.None))
{
    closedRun.Dispose(); Reject(() => models.RunEncoder(closedRun, src, downsampling, ends));
}
using (var closedRuntime = new OrtRuntime(Path.GetFullPath(args[2])))
{
    closedRuntime.Dispose(); Reject(() => new Math2Mode0TensorModels(closedRuntime, model));
}
var mutatedGraphs = model.Graphs.ToArray(); var corruptedGraph = model.Graphs[0].ToArray();
corruptedGraph[^1] ^= 1; mutatedGraphs[0] = corruptedGraph;
Reject(() => new Math2Mode0TensorModels(runtime, new MathModel(model.Labels, mutatedGraphs)));
Reject(() => Math2Mode0TensorState.Gather([0f], [1], 0, new int[17]));
Reject(() => Math2Mode0TensorState.Gather([0f], [2, int.MaxValue], 0, [0]));
Reject(() => Math2Mode0TensorState.Gather([0f], [1], 0, [1]));
var gathers = new List<GatherCapture>();
foreach (var (shape, axis, indices) in new (long[], int, int[])[]
{
    (new long[] { 2, 3, 4, 2, 3 }, 1, new[] { 2, 0, 2 }),
    (new long[] { 2, 3, 4 }, 1, new[] { 1, 1 }),
    (new long[] { 3, 2, 4 }, 0, new[] { 2, 0, 2, 1 }),
    (new long[] { 2, 3, 0, 64 }, 1, new[] { 2, 0 }),
    (new long[] { 2, 3, 4 }, 1, new[] { 0, 1, 2, 0 })
})
{
    int count = (int)shape.Aggregate(1L, (a, b) => a * b);
    var values = Enumerable.Range(0, count).Select(x => (x % 2 == 0 ? 1 : -1) * (float)(x + .25)).ToArray();
    var actual = Math2Mode0TensorState.Gather(values, shape, axis, indices);
    gathers.Add(new(Capture("source", 1, shape, values, null), axis, indices,
        Capture("gathered", 1, actual.Shape, actual.Values, null)));
}
var result = new StateCapture(runtime.Version, features.Frames, features.StrokeEnds,
    model.Graphs.Select(x => Convert.ToHexStringLower(SHA256.HashData(x.Span))).ToArray(),
    Convert.ToHexStringLower(SHA256.HashData(resource)), captures.ToArray(), portableInitial, portableSteps.ToArray(), gathers.ToArray(), guards);
File.WriteAllText(args[3], JsonSerializer.Serialize(result, CaptureJson.Default.StateCapture) + "\n");
Console.WriteLine($"PASS actual encoder/initializer +14 captured decoder steps, B1->3/repeat/shrink/permutation, {guards} guards; actual portable-score/heap/EOS composition separately; officialORT {runtime.Version}");

internal sealed record TensorCapture(string Name, int Type, long[] Shape, string Bytes);
internal sealed record StepCapture(long Offset, int BeforeBeams, int AfterBeams, TensorCapture[] Before,
    TensorCapture[] Outputs, string Selected48, TensorCapture[] After);
internal sealed record StateRun(bool PreparedForce, long MaximumPredictionCount,
    TensorCapture[] Encoder, TensorCapture[] Initial, StepCapture[] Steps);
internal sealed record StateCapture(string Runtime, int Frames, long[] StrokeEnds,
    string[] Models, string Resource, StateRun[] Runs, TensorCapture[] PortableInitial,
    PortableStepCapture[] PortableSteps, GatherCapture[] PreparedGathers, int Guards);
internal sealed record PortableStepCapture(StepCapture Step, TensorCapture Prior,
    TensorCapture Logs, TensorCapture Costs, string Heap, string Sorted, string AfterEos, string AfterResort);
internal sealed record GatherCapture(TensorCapture Source, int Axis, int[] Indices, TensorCapture Output);
[JsonSerializable(typeof(StateCapture))]
internal partial class CaptureJson : JsonSerializerContext;
