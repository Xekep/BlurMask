using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

[assembly: InternalsVisibleTo("LegacyMathTensorState.Probe")]

namespace InkRecognition.Math;

/// <summary>Fingerprint-bound sessions; the caller retains the official ORT
/// runtime and run until all dependent state/step/tensor owners are disposed.</summary>
internal sealed class Math2Mode0TensorModels : IDisposable
{
    private static readonly string[] Hashes =
    [
        "3c1a5b691e34385cdaf18f4bdbdb8c8f8cd2cb757f4faed081858f17ea69f286",
        "1393a3c67eafbee9f0bf80a6c3442119bf78b26901986587a2442b1c2d4a0c4a",
        "925a3302b612e634a226ddc907d6ac52c8e908b3d931040cd49b63f4875e4638"
    ];
    internal OrtRuntime Runtime { get; }
    private OrtSession Encoder { get; }
    private OrtSession Initializer { get; }
    private OrtSession Decoder { get; }
    private readonly object gate = new();
    private int states;
    private readonly IDisposable runtimeLease;
    private bool disposed;

    internal Math2Mode0TensorModels(OrtRuntime runtime, MathModel model)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        ArgumentNullException.ThrowIfNull(model);
        if (model.Graphs.Length != 3 || model.Graphs.Where((x, i) =>
            Convert.ToHexStringLower(SHA256.HashData(x.Span)) != Hashes[i]).Any())
            throw new NotSupportedException("The three exact current Math2 mode0 graphs are required.");
        Runtime = runtime;
        runtimeLease = runtime.RetainMode0Owner();
        OrtSession? encoder = null, initializer = null;
        try
        {
            encoder = runtime.CreateSession(model.Graphs[0], ["src", "downsampling", "strk_ends"],
                ["output", "output_len", "output_seg", "counting_output", "max_pred_counts"]);
            initializer = runtime.CreateSession(model.Graphs[1], ["memory", "batch_size", "memory_seg"],
                ["cross_k", "cross_v", "proj_key_seg_in"]);
            Decoder = runtime.CreateSession(model.Graphs[2], Math2Mode0TensorState.CopyInputNames(),
                ["reco_output", "attn_seg", "state_out_k", "state_out_v"]);
            Encoder = encoder;
            Initializer = initializer;
        }
        catch { initializer?.Dispose(); encoder?.Dispose(); runtimeLease.Dispose(); throw; }
    }

    internal Math2Mode0EncoderOutputs RunEncoder(OrtRun run, OrtTensor source, OrtTensor downsampling,
        OrtTensor strokeEnds, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(downsampling);
        ArgumentNullException.ThrowIfNull(strokeEnds);
        using var operation = RetainOperation();
        Check(run);
        token.ThrowIfCancellationRequested();
        if (!source.IsOwnedBy(Runtime) || !downsampling.IsOwnedBy(Runtime) || !strokeEnds.IsOwnedBy(Runtime))
            throw new ArgumentException("Same runtime input tensors required.");
        var s = source.Describe();
        if (s.Type != 1 || s.Shape.Length != 3 || s.Shape[0] < 1 || s.Shape[0] > 6000 ||
            s.Shape[1] != 1 || s.Shape[2] != 26)
            throw new ArgumentException("Bounded actual encoder input [N,1,26] required.");
        Math2Mode0TensorState.Require(downsampling, 7, [s.Shape[0], 1]);
        var e = strokeEnds.Describe();
        if (e.Type != 7 || e.Shape.Length != 2 || e.Shape[0] != 1 || e.Shape[1] < 1 || e.Shape[1] > 6000)
            throw new ArgumentException("Bounded actual stroke-end input [1,K] required.");
        using var activeRun = run.RetainMode0Owner(Runtime);
        var outputs = Encoder.Run(run, source, downsampling, strokeEnds);
        try
        {
            token.ThrowIfCancellationRequested();
            return new(this, run, outputs);
        }
        catch { Math2Mode0TensorState.DisposeAll(outputs); throw; }
    }

    internal OrtTensor[] RunInitializer(OrtRun run, OrtTensor memory, OrtTensor batch, OrtTensor segments)
    {
        Check(run);
        return Initializer.Run(run, memory, batch, segments);
    }
    internal OrtTensor[] RunDecoder(OrtRun run, OrtTensor[] inputs)
    {
        Check(run);
        return Decoder.Run(run, inputs);
    }
    internal void Check(OrtRun run)
    {
        ArgumentNullException.ThrowIfNull(run);
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!run.IsOwnedBy(Runtime)) throw new ArgumentException("Same live ORT run required.");
    }
    internal void AddState() { lock (gate) { ObjectDisposedException.ThrowIf(disposed, this); states++; } }
    internal void ReleaseState() { lock (gate) states--; }
    internal IDisposable RetainOperation() { var lease = new OperationLease(this); AddState(); return lease; }
    private sealed class OperationLease(Math2Mode0TensorModels models) : IDisposable
    {
        private Math2Mode0TensorModels? owner = models;
        public void Dispose() { Interlocked.Exchange(ref owner, null)?.ReleaseState(); }
    }
    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            if (states != 0) throw new InvalidOperationException("Dispose live tensor states and encoder tuples before their model sessions.");
            Decoder.Dispose(); Initializer.Dispose(); Encoder.Dispose(); disposed = true; runtimeLease.Dispose();
        }
    }
}

/// <summary>Actual encoder handle ownership. Prepare consumes it exactly once,
/// including failure, after managed owner/policy validation.</summary>
internal sealed class Math2Mode0EncoderOutputs : IDisposable
{
    internal Math2Mode0TensorModels Models { get; }
    internal OrtRun Run { get; }
    private readonly object gate = new();
    private OrtTensor[]? outputs;
    private IDisposable? runLease;
    internal Math2Mode0EncoderOutputs(Math2Mode0TensorModels models, OrtRun run, OrtTensor[] tensors)
    {
        Models = models; Run = run; outputs = tensors;
        runLease = run.RetainMode0Owner(models.Runtime);
        try { models.AddState(); }
        catch { runLease.Dispose(); runLease = null; throw; }
    }
    internal (OrtTensor[] Outputs, IDisposable RunLease) Take()
    {
        lock (gate)
        {
            var taken = outputs ?? throw new InvalidOperationException("Encoder outputs already consumed or disposed.");
            outputs = null;
            Models.ReleaseState();
            var lease = runLease!; runLease = null;
            return (taken, lease);
        }
    }
    internal (int Type, long[] Shape, float[]? Floats, long[]? Int64s) CopyOutput(int ordinal)
    {
        lock (gate)
        {
            var tensor = (outputs ?? throw new InvalidOperationException("Consumed encoder tuple."))[ordinal];
            var metadata = tensor.Describe();
            return (metadata.Type, metadata.Shape, metadata.Type == 1 ? tensor.Floats().Values : null,
                metadata.Type == 7 ? tensor.Int64s().Values : null);
        }
    }
    public void Dispose()
    {
        lock (gate)
        {
            if (outputs is { } taken) { outputs = null; Math2Mode0TensorState.DisposeAll(taken); Models.ReleaseState(); }
            runLease?.Dispose(); runLease = null;
        }
    }
}

/// <summary>The no-prefix/default-zero initialization branch is proven; force
/// nonobservable positions remains an explicit prepared configuration input.</summary>
internal sealed class Math2Mode0PreparedPolicy
{
    internal LegacyMath2ObservableTokens Gate { get; }
    internal bool ForceNonobservablePositions { get; }
    internal Math2Mode0PreparedPolicy(LegacyMath2ObservableTokens gate, bool preparedForceNonobservablePositions)
    { ArgumentNullException.ThrowIfNull(gate); Gate = gate; ForceNonobservablePositions = preparedForceNonobservablePositions; }
}

/// <summary>CPU-copy numerical adaptation of the checked EF1 mode0 tensor
/// lifecycle. No EEF score producer, full beam, SDK allocation/failure order or
/// final recognition/acceptance is implemented.</summary>
internal sealed class Math2Mode0TensorState : IDisposable
{
    private static readonly string[] InputNames = ["tgt", "offset", "memory_seg", "prev_attn_seg",
        "proj_key_seg_in", "state_in_k", "state_in_v", "cross_k", "cross_v"];
    internal static string[] CopyInputNames() => InputNames.ToArray();
    private readonly object gate = new();
    private readonly Math2Mode0TensorModels models;
    private readonly OrtRun run;
    private readonly IDisposable runLease;
    private readonly Math2Mode0PreparedPolicy policy;
    private OrtTensor[] inputs;
    private Math2Mode0DecoderStep? pending;
    private bool failed, disposed;
    private float[] priorCosts = [0f];
    internal int BeamCount { get; private set; } = 1;
    internal long Offset { get; private set; }
    internal long MaximumPredictionCount { get; }
    private readonly long segments, memoryLength;

    private Math2Mode0TensorState(Math2Mode0TensorModels models, OrtRun run, Math2Mode0PreparedPolicy policy,
        OrtTensor[] inputs, long segments, long memoryLength, long maxCount, IDisposable runLease)
    {
        this.models = models; this.run = run; this.policy = policy; this.inputs = inputs; this.runLease = runLease;
        this.segments = segments; this.memoryLength = memoryLength; MaximumPredictionCount = maxCount;
        models.AddState();
    }

    internal static Math2Mode0TensorState Prepare(Math2Mode0TensorModels models,
        Math2Mode0EncoderOutputs encoder, Math2Mode0PreparedPolicy policy, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(models); ArgumentNullException.ThrowIfNull(encoder);
        ArgumentNullException.ThrowIfNull(policy);
        using var operation = models.RetainOperation();
        models.Check(encoder.Run);
        if (!ReferenceEquals(models, encoder.Models)) throw new ArgumentException("Actual same model/encoder owner required.");
        token.ThrowIfCancellationRequested();
        var taken = encoder.Take();
        var output = taken.Outputs;
        IDisposable? runLease = taken.RunLease;
        var runtime = models.Runtime;
        var made = new List<OrtTensor>();
        OrtTensor[]? initialized = null;
        try
        {
            if (output.Length != 5 || output.Any(x => !x.IsOwnedBy(runtime)) || output.Distinct().Count() != 5)
                throw new ArgumentException("Unique owned five-output encoder tuple required.");
            var memory = output[0].Describe(); var seg = output[2].Floats();
            if (memory.Type != 1 || memory.Shape.Length != 3 || memory.Shape[0] < 1 || memory.Shape[0] > 6000 ||
                memory.Shape[1] != 1 || memory.Shape[2] != 256 || seg.Shape.Length != 3 ||
                seg.Shape[0] < 1 || seg.Shape[0] > 6000 || seg.Shape[1] != 1 || seg.Shape[2] != 256)
                throw new InvalidDataException("Actual current encoder memory/segment geometry required.");
            var length = output[1].Int64s(); var max = output[4].Int64s();
            Require(output[1], 7, [1]); Require(output[3], 1, [1, 1]); Require(output[4], 7, [1]);
            if (length.Values[0] < 0 || max.Values[0] < 0)
                throw new NotSupportedException("Nonnegative actual encoder counts required in this bounded adaptation.");
            using var batch = runtime.Tensor(new float[1], 1); // Native default calloc: values zero, extent one.
            initialized = models.RunInitializer(encoder.Run, output[0], batch, output[2]);
            token.ThrowIfCancellationRequested();
            if (initialized.Length != 3) throw new InvalidDataException("Initializer tuple extent differs.");
            Require(initialized[0], 1, [2, 4, memory.Shape[0], 64]);
            Require(initialized[1], 1, [2, 4, memory.Shape[0], 64]);
            Require(initialized[2], 1, [1, seg.Shape[0], 256]);
            made.Add(runtime.Tensor(new long[1], 1, 1));
            made.Add(runtime.Tensor(new long[1], 1));
            made.Add(runtime.Tensor(seg.Values, seg.Shape)); // Separate decoder copy; initializer borrows original.
            made.Add(runtime.Tensor(Array.Empty<float>(), seg.Shape[0], 1, 0));
            made.Add(initialized[2]); initialized[2] = null!;
            made.Add(runtime.Tensor(Array.Empty<float>(), 2, 4, 0, 64));
            made.Add(runtime.Tensor(Array.Empty<float>(), 2, 4, 0, 64));
            made.Add(initialized[0]); initialized[0] = null!;
            made.Add(initialized[1]); initialized[1] = null!;
            token.ThrowIfCancellationRequested();
            var state = new Math2Mode0TensorState(models, encoder.Run, policy, made.ToArray(),
                seg.Shape[0], memory.Shape[0], max.Values[0], runLease);
            runLease = null;
            return state;
        }
        catch { DisposeAll(made); throw; }
        finally { DisposeAll(output); if (initialized != null) DisposeAll(initialized); runLease?.Dispose(); }
    }

    internal Math2Mode0DecoderStep RunStep(CancellationToken token = default)
    {
        lock (gate)
        {
            Check();
            if (pending != null) throw new InvalidOperationException("Apply the pending step before running another.");
            OrtTensor[]? outputs = null;
            try
            {
                token.ThrowIfCancellationRequested(); models.Check(run);
                outputs = models.RunDecoder(run, inputs);
                token.ThrowIfCancellationRequested();
                if (outputs.Length != 4) throw new InvalidDataException("Decoder tuple extent differs.");
                Require(outputs[0], 1, [1, BeamCount, 297]);
                Require(outputs[1], 1, [segments, BeamCount, 1]);
                Require(outputs[2], 1, [2, BeamCount * 4L, Offset + 1, 64]);
                Require(outputs[3], 1, [2, BeamCount * 4L, Offset + 1, 64]);
                pending = new(this, outputs, BeamCount, Offset);
                return pending;
            }
            catch { failed = true; if (outputs != null) DisposeAll(outputs); throw; }
        }
    }

    internal void ApplySelected(Math2Mode0SelectedRows selected, CancellationToken token = default)
    {
        lock (gate)
        {
            ArgumentNullException.ThrowIfNull(selected);
            Check();
            if (pending == null || !ReferenceEquals(selected.Step, pending))
                throw new ArgumentException("Selected rows must belong to this exact pending decoder step.");
            var next = new List<OrtTensor>();
            try
            {
                token.ThrowIfCancellationRequested(); models.Check(run);
                selected.Check();
                int n = selected.Count;
                int[] parents = selected.CopyParents();
                long[] labels = selected.CopyLabels();
                int[] staticMap = Enumerable.Range(0, n).Select(i => i < BeamCount ? i : 0).ToArray();
                var runtime = models.Runtime;
                next.Add(runtime.Tensor(labels, 1, n));
                next.Add(runtime.Tensor([checked(Offset + 1)], 1));
                next.Add(GatherTensor(inputs[2], 1, staticMap));
                var previous = inputs[3].Floats(); var latest = pending.Output(1).Floats();
                int t = checked((int)Offset), s = checked((int)segments), old = BeamCount;
                int attentionCount = checked(s * old * (t + 1));
                if (attentionCount > 64 * 1024 * 1024) throw new NotSupportedException("Attention extent exceeds the supported copy limit.");
                var concatenated = new float[attentionCount];
                for (int segment = 0; segment < s; segment++)
                    for (int beam = 0; beam < old; beam++)
                    {
                        previous.Values.AsSpan((segment * old + beam) * t, t)
                            .CopyTo(concatenated.AsSpan((segment * old + beam) * (t + 1)));
                        concatenated[(segment * old + beam) * (t + 1) + t] = latest.Values[segment * old + beam];
                    }
                var attention = Gather(concatenated, [s, old, t + 1], 1, parents);
                if (policy.ForceNonobservablePositions)
                    for (int beam = 0; beam < n; beam++)
                        if (!policy.Gate.IsObservable((ushort)labels[beam]))
                            for (int segment = 0; segment < s; segment++)
                                attention.Values[(segment * n + beam) * (t + 1) + t] = -float.MaxValue;
                next.Add(runtime.Tensor(attention.Values, attention.Shape));
                next.Add(GatherTensor(inputs[4], 0, staticMap));
                next.Add(GatherKv(pending.Output(2), parents, Offset + 1));
                next.Add(GatherKv(pending.Output(3), parents, Offset + 1));
                next.Add(GatherKv(inputs[7], staticMap, memoryLength));
                next.Add(GatherKv(inputs[8], staticMap, memoryLength));
                token.ThrowIfCancellationRequested();
                DisposeAll(inputs);
                inputs = next.ToArray();
                next.Clear();
                pending.Consume(); pending = null;
                BeamCount = n; Offset++; priorCosts = selected.CopyCosts();
            }
            catch { failed = true; DisposeAll(next); pending?.Consume(); pending = null; throw; }
        }
    }

    private OrtTensor GatherTensor(OrtTensor source, int axis, int[] indices)
    {
        var tensor = source.Floats(); var gathered = Gather(tensor.Values, tensor.Shape, axis, indices);
        return models.Runtime.Tensor(gathered.Values, gathered.Shape);
    }
    private OrtTensor GatherKv(OrtTensor source, int[] indices, long time)
    {
        Require(source, 1, [2, BeamCount * 4L, time, 64]);
        var tensor = source.Floats();
        var gathered = Gather(tensor.Values, [2, BeamCount, 4, time, 64], 1, indices);
        return models.Runtime.Tensor(gathered.Values, 2, indices.Length * 4L, time, 64);
    }
    internal static (float[] Values, long[] Shape) Gather(float[] values, long[] shape, int axis, int[] indices)
    {
        if (axis < 0 || axis >= shape.Length || indices.Length > 16 || indices.Any(i => i < 0 || i >= shape[axis]))
            throw new ArgumentException("Bounded tensor gather indices required.");
        int before = Product(shape.AsSpan(0, axis)), after = Product(shape.AsSpan(axis + 1));
        if (values.Length != checked(before * (int)shape[axis] * after)) throw new ArgumentException("Gather extent differs.");
        long outputCount = checked((long)before * indices.Length * after);
        if (outputCount > 64 * 1024 * 1024) throw new ArgumentException("Gather output exceeds supported copy limit.");
        var output = new float[(int)outputCount];
        for (int outer = 0; outer < before; outer++)
            for (int i = 0; i < indices.Length; i++)
                values.AsSpan(checked((outer * (int)shape[axis] + indices[i]) * after), after)
                    .CopyTo(output.AsSpan((outer * indices.Length + i) * after));
        var resultShape = shape.ToArray(); resultShape[axis] = indices.Length;
        return (output, resultShape);
    }
    private static int Product(ReadOnlySpan<long> dims)
    {
        long count = 1;
        foreach (long dim in dims) { if (dim < 0) throw new ArgumentException("Negative gather dimension."); count = checked(count * dim); }
        if (count > 64 * 1024 * 1024) throw new ArgumentException("Gather extent exceeds supported limit.");
        return (int)count;
    }
    internal static void Require(OrtTensor tensor, int type, long[] shape)
    {
        var actual = tensor.Describe();
        if (actual.Type != type || !actual.Shape.AsSpan().SequenceEqual(shape))
            throw new InvalidDataException("Tensor type or geometry differs from the actual current mode0 contract.");
    }
    internal void Check()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (failed) throw new InvalidOperationException("Failed tensor state cannot be reused.");
    }
    internal LegacyMath2ObservableTokens ObservableGate => policy.Gate;
    internal float[] CopyPriorCosts(Math2Mode0DecoderStep step) { CheckStep(step); return priorCosts.ToArray(); }
    internal T WithStep<T>(Math2Mode0DecoderStep step, Func<T> action)
    {
        lock (gate) { CheckStep(step); return action(); }
    }
    internal void CheckStep(Math2Mode0DecoderStep step)
    {
        Check();
        if (!ReferenceEquals(pending, step)) throw new InvalidOperationException("Step no longer belongs to the active state generation.");
    }
    internal (string Name, int Type, long[] Shape, float[]? Floats, long[]? Int64s)[] CopyInputs()
    {
        lock (gate)
        {
            Check();
            return inputs.Select((x, i) =>
            {
                var d = x.Describe();
                return (InputNames[i], d.Type, d.Shape, d.Type == 1 ? x.Floats().Values : null,
                    d.Type == 7 ? x.Int64s().Values : null);
            }).ToArray();
        }
    }
    internal static void DisposeAll(IEnumerable<OrtTensor> tensors)
    { foreach (var tensor in tensors) tensor?.Dispose(); }
    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            pending?.Consume(); pending = null;
            DisposeAll(inputs); inputs = [];
            disposed = true; runLease.Dispose(); models.ReleaseState();
        }
    }
}

internal sealed class Math2Mode0DecoderStep
{
    internal Math2Mode0TensorState Owner { get; }
    private OrtTensor[]? outputs;
    internal int BeamCount { get; }
    internal long Offset { get; }
    internal Math2Mode0DecoderStep(Math2Mode0TensorState owner, OrtTensor[] outputs, int count, long offset)
    { Owner = owner; this.outputs = outputs; BeamCount = count; Offset = offset; }
    internal OrtTensor Output(int ordinal)
    {
        Owner.CheckStep(this);
        return (outputs ?? throw new InvalidOperationException("Consumed decoder step."))[ordinal];
    }
    internal (float[] Values, long[] Shape) CopyOutput(int ordinal) => Owner.WithStep(this, () => Output(ordinal).Floats());
    internal Math2Mode0ScoredCandidates CreatePortableCandidates(int preparedBeamWidth,
        float preparedTemperature, float preparedDelta, ReadOnlySpan<byte> preparedSourceInformation64,
        ReadOnlySpan<byte> preparedConditional32, IReadOnlyList<ushort[]> preparedPrefixes,
        CancellationToken token = default)
    {
        if (preparedSourceInformation64.Length != BeamCount * 8 || preparedConditional32.Length != BeamCount * 4)
            throw new ArgumentException("Complete bounded prepared sidecars required.");
        var source = preparedSourceInformation64.ToArray(); var conditional = preparedConditional32.ToArray();
        return Owner.WithStep(this, () => Math2Mode0ScoredCandidates.Create(this, preparedBeamWidth,
            preparedTemperature, preparedDelta, source, conditional, preparedPrefixes, token));
    }
    /// <summary>Prepared selection over this actual step; this does not prove
    /// an EEF score or native per-beam sidecar producer.</summary>
    internal Math2Mode0SelectedRows BindPreparedRows(ReadOnlySpan<byte> records)
    {
        Owner.CheckStep(this);
        return new(this, records, prepared: true);
    }
    internal void Consume() { if (outputs is { } taken) { outputs = null; Math2Mode0TensorState.DisposeAll(taken); } }
}

internal sealed class Math2Mode0SelectedRows
{
    internal Math2Mode0DecoderStep Step { get; }
    internal int Count { get; }
    internal bool IsExplicitPreparedSelection { get; }
    private int[] Parents { get; }
    private long[] Labels { get; }
    private readonly byte[] records;
    internal Math2Mode0SelectedRows(Math2Mode0DecoderStep step, ReadOnlySpan<byte> records, bool prepared)
    {
        if (records.Length < 48 || records.Length > 16 * 48 || records.Length % 48 != 0)
            throw new ArgumentException("One to sixteen complete selected48 rows required.");
        IsExplicitPreparedSelection = prepared; Step = step; this.records = records.ToArray(); Count = records.Length / 48;
        Parents = new int[Count]; Labels = new long[Count];
        for (int i = 0; i < Count; i++)
        {
            var row = records.Slice(i * 48, 48);
            Parents[i] = BinaryPrimitives.ReadInt32LittleEndian(row[40..]);
            ulong label = BinaryPrimitives.ReadUInt64LittleEndian(row[8..]);
            if (Parents[i] < 0 || Parents[i] >= step.BeamCount || label >= 297)
                throw new ArgumentException("Selected parent/target outside this current actual tensor domain.");
            Labels[i] = (long)label;
        }
    }
    internal byte[] CopyRecords() => records.ToArray();
    internal int[] CopyParents() => Parents.ToArray();
    internal long[] CopyLabels() => Labels.ToArray();
    internal float[] CopyCosts() => Enumerable.Range(0, Count)
        .Select(i => BinaryPrimitives.ReadSingleLittleEndian(records.AsSpan(i * 48))).ToArray();
    internal void Check() => Step.Owner.CheckStep(Step);
}

/// <summary>Same-step actual logits -> portable F32 scores -> native prepared
/// heap/sort/EOS kernels. Width, temperature, delta, prefixes and per-beam
/// metadata remain explicitly prepared; this is not the native outer driver.</summary>
internal sealed class Math2Mode0ScoredCandidates
{
    private readonly Math2Mode0DecoderStep step;
    private readonly LegacyPortableMathLogProbabilities scores;
    private readonly LegacyPreparedMathEosHandoff handoff;
    private Math2Mode0ScoredCandidates(Math2Mode0DecoderStep step,
        LegacyPortableMathLogProbabilities scores, LegacyPreparedMathEosHandoff handoff)
    { this.step = step; this.scores = scores; this.handoff = handoff; }
    internal static Math2Mode0ScoredCandidates Create(Math2Mode0DecoderStep step, int width,
        float temperature, float delta, ReadOnlySpan<byte> sourceInfo, ReadOnlySpan<byte> conditional,
        IReadOnlyList<ushort[]> prefixes, CancellationToken token)
    {
        step.Owner.CheckStep(step); token.ThrowIfCancellationRequested();
        var logits = step.CopyOutput(0);
        var scores = LegacyPortableMathLogProbabilities.TransformLastFrame(logits.Values, 1,
            step.BeamCount, 297, temperature, step.Owner.CopyPriorCosts(step), token);
        var costs = scores.CopyCumulativeCosts();
        //Actual bundled EEF03C690 masks BOS0; B2/B4 space branch is absent
        //because the checked bundle's space sentinel is FFFF.
        for (int beam = 0; beam < step.BeamCount; beam++) costs[beam * 297] = float.MaxValue;
        byte[] bytes = new byte[costs.Length * 4];
        for (int i = 0; i < costs.Length; i++) BinaryPrimitives.WriteSingleLittleEndian(bytes.AsSpan(i * 4), costs[i]);
        var candidates = LegacyPreparedMathUnrestrictedCandidates.Expand(bytes, step.BeamCount, 297,
            width, sourceInfo, conditional, token);
        var handoff = LegacyPreparedMathEosHandoff.Apply(candidates, delta, step.Owner.ObservableGate, prefixes, token);
        token.ThrowIfCancellationRequested(); step.Owner.CheckStep(step);
        return new(step, scores, handoff);
    }
    internal Math2Mode0SelectedRows Select()
    {
        return step.Owner.WithStep(step, () => new Math2Mode0SelectedRows(step,
            handoff.CopyAfterResort().AsSpan(0, handoff.ActiveCount * 48), prepared: false));
    }
    internal float[] CopyLogProbabilities() => scores.CopyLogProbabilities();
    internal float[] CopyUnmaskedCosts() => scores.CopyCumulativeCosts();
    internal byte[] CopyHeapStorage() => handoff.Source.CopyHeapStorage();
    internal byte[] CopySortedStorage() => handoff.Source.CopySortedStorage();
    internal byte[] CopyAfterEos() => handoff.CopyAfterEos();
    internal byte[] CopyAfterResort() => handoff.CopyAfterResort();
}
