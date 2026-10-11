namespace InkRecognition.Math;

public sealed record MathRecognitionResult(string Text, string? LaTex, IReadOnlyList<string> Tokens,
    float Confidence, int Frames, bool Completed);

/// <summary>Independent ordinary-pen Math2 preprocessing and greedy decoding.</summary>
public sealed class StrokeMathRecognizer : IDisposable
{
    private readonly object _gate = new();
    private readonly string[] _labels;
    private readonly OrtRuntime _runtime;
    private readonly OrtSession _encoder, _initial, _decoder;
    private bool _disposed;
    public string RuntimeVersion => _runtime.Version;

    private StrokeMathRecognizer(MathModel model, string? runtimePath)
    {
        _labels = model.Labels; _runtime = new(runtimePath);
        OrtSession? encoder = null, initial = null;
        try
        {
            encoder = _runtime.CreateSession(model.Graphs[0], ["src", "downsampling", "strk_ends"],
                ["output", "output_len", "output_seg", "counting_output", "max_pred_counts"]);
            initial = _runtime.CreateSession(model.Graphs[1], ["memory", "batch_size", "memory_seg"],
                ["cross_k", "cross_v", "proj_key_seg_in"]);
            _decoder = _runtime.CreateSession(model.Graphs[2], ["tgt", "offset", "memory_seg", "prev_attn_seg", "proj_key_seg_in", "state_in_k", "state_in_v", "cross_k", "cross_v"],
                ["reco_output", "attn_seg", "state_out_k", "state_out_v"]);
            _encoder = encoder; _initial = initial;
        }
        catch { initial?.Dispose(); encoder?.Dispose(); _runtime.Dispose(); throw; }
    }

    public static StrokeMathRecognizer Load(Stream model, string? nativeRuntimePath = null) => new(MathModel.Read(model), nativeRuntimePath);

    public MathRecognitionResult Recognize(IReadOnlyList<InkStroke> strokes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(strokes);
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            cancellationToken.ThrowIfCancellationRequested();
            var features = MathPreprocessor.Process(strokes);
            if (features.Frames == 0) return new("", "", Array.Empty<string>(), 0, 0, true);
            cancellationToken.ThrowIfCancellationRequested();
            try { return Decode(features, cancellationToken); }
            catch (InvalidOperationException) when (cancellationToken.IsCancellationRequested) { throw new OperationCanceledException(cancellationToken); }
        }
    }

    private MathRecognitionResult Decode(MathFeatures features, CancellationToken token)
    {
        using var run = _runtime.CreateRun(token);
        using var src = _runtime.Tensor(features.Values, features.Frames, 1, 26);
        using var downsampling = _runtime.Tensor(Enumerable.Repeat(1L, features.Frames).ToArray(), features.Frames, 1);
        using var ends = _runtime.Tensor(features.StrokeEnds, 1, features.StrokeEnds.Length);
        var memory = _encoder.Run(run, src, downsampling, ends);
        OrtTensor[]? initialization = null;
        OrtTensor? key = null, value = null;
        try
        {
            var seg = memory[2].Floats().Shape;
            if (seg.Length != 3 || seg[1] != 1 || seg[2] != 256 || seg[0] < 1 || seg[0] > 6000) throw new InvalidDataException("Invalid encoder segments.");
            int segments = (int)seg[0];
            using var batch = _runtime.Tensor([1f], 1);
            initialization = _initial.Run(run, memory[0], batch, memory[2]);
            key = _runtime.Tensor(Array.Empty<float>(), 2, 4, 0, 64);
            value = _runtime.Tensor(Array.Empty<float>(), 2, 4, 0, 64);
            float[] previousAttention = [];
            var strings = new List<string>(); double logConfidence = 0;
            long previous = 0; bool completed = false;
            for (int offset = 0; offset < 128; offset++)
            {
                token.ThrowIfCancellationRequested();
                using var target = _runtime.Tensor([previous], 1, 1);
                using var position = _runtime.Tensor([(long)offset], 1);
                using var attention = _runtime.Tensor(previousAttention, segments, 1, offset);
                var outputs = _decoder.Run(run, target, position, memory[2], attention, initialization[2], key, value, initialization[0], initialization[1]);
                try
                {
                    var (logits, shape) = outputs[0].Floats();
                    if (!shape.AsSpan().SequenceEqual(new long[] { 1, 1, 297 })) throw new InvalidDataException("Invalid decoder vocabulary shape.");
                    int chosen = 0;
                    for (int i = 1; i < logits.Length; i++) if (logits[i] > logits[chosen]) chosen = i;
                    if (chosen == 1) { completed = true; break; }
                    if (chosen == 0) throw new InvalidDataException("Decoder emitted BOS after initialization.");
                    double denominator = 0;
                    foreach (float x in logits) denominator += System.Math.Exp((double)x - logits[chosen]);
                    logConfidence -= System.Math.Log(denominator);
                    strings.Add(_labels[chosen]); previous = chosen;
                    var (currentAttention, attentionShape) = outputs[1].Floats();
                    if (!attentionShape.AsSpan().SequenceEqual(new long[] { segments, 1, 1 })) throw new InvalidDataException("Invalid decoder attention shape.");
                    var nextAttention = new float[checked(segments * (offset + 1))];
                    for (int i = 0; i < segments; i++)
                    {
                        previousAttention.AsSpan(i * offset, offset).CopyTo(nextAttention.AsSpan(i * (offset + 1)));
                        nextAttention[i * (offset + 1) + offset] = currentAttention[i];
                    }
                    previousAttention = nextAttention;
                    key.Dispose(); value.Dispose(); key = outputs[2]; value = outputs[3];
                    outputs[2] = null!; outputs[3] = null!;
                }
                finally { foreach (var output in outputs) output?.Dispose(); }
            }
            var text = string.Concat(strings);
            return new(text, completed ? MathLatexFormatter.Format(strings) : null, strings.AsReadOnly(),
                strings.Count == 0 ? 0 : (float)System.Math.Exp(logConfidence / strings.Count), features.Frames, completed);
        }
        finally
        {
            value?.Dispose(); key?.Dispose();
            if (initialization != null) foreach (var output in initialization) output.Dispose();
            foreach (var output in memory) output.Dispose();
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed) return;
            _decoder.Dispose(); _initial.Dispose(); _encoder.Dispose(); _runtime.Dispose(); _disposed = true;
        }
    }
}
