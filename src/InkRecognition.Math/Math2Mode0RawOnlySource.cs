using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathRawOnlyEf4.Probe")]

namespace InkRecognition.Math;

/// <summary>Library-owned pen strokes only. C25, imported glyphs, external
/// properties and a supplied existing-expression vector have no entry here.</summary>
internal sealed class Math2Mode0RawOnlySource : IDisposable
{
    private static readonly object Factory = new();
    private readonly object gate = new();
    private readonly object factory = Factory;
    private readonly long[] strokeEnds;
    private readonly Math2Mode0OriginalMathFieldSource.Binding? selectedField;
    private bool disposed;
    private Math2Mode0RawAttentionHistory? history;
    private Math2Mode0InitiallyOrderedCompleted? ordered;
    private Math2Mode0RawOnlyEf4? processed;
    private readonly Math2Mode0RawSourcePositions? sourcePositions;
    internal Math2Mode0ColdRecordingLoop Loop { get; }
    internal Math2Mode0RawOnlyEf4Policy Policy { get; }
    internal int InputFrameCount { get; }
    internal int StrokeCount => strokeEnds.Length;
    internal object SourceFactoryIdentity => factory;

    private Math2Mode0RawOnlySource(Math2Mode0ColdRecordingLoop loop,
        Math2Mode0RawOnlyEf4Policy policy, int frames, long[] ownedEnds,
        Math2Mode0RawSourcePositions? sourcePositions = null,
        Math2Mode0OriginalMathFieldSource.Binding? selectedField = null)
    { Loop = loop; Policy = policy; InputFrameCount = frames; strokeEnds = ownedEnds; this.sourcePositions = sourcePositions; this.selectedField = selectedField; }

    internal static Math2Mode0RawOnlySource PrepareColdDefault(OrtRuntime runtime,
        Math2Mode0ColdBundle bundle, OrtRun run, IReadOnlyList<InkStroke> strokes,
        CancellationToken token = default) => PrepareCore(runtime, bundle, run, strokes, null, false, token);

    internal static Math2Mode0RawOnlySource PrepareColdWithSourcePositions(OrtRuntime runtime,
        Math2Mode0ColdBundle bundle, OrtRun run, IReadOnlyList<InkStroke> strokes,
        CancellationToken token = default) => PrepareCore(runtime, bundle, run, strokes, null, true, token);

    // Explicit diagnostic search policy; no claim that a fresh EEB selected width > 1.
    internal static Math2Mode0RawOnlySource PrepareWithPreparedSearch(OrtRuntime runtime,
        Math2Mode0ColdBundle bundle, OrtRun run, IReadOnlyList<InkStroke> strokes,
        int width, CancellationToken token = default)
        => PrepareCore(runtime, bundle, run, strokes, width, false, token);

    internal static Math2Mode0RawOnlySource PrepareWithPreparedSearchAndSourcePositions(OrtRuntime runtime,
        Math2Mode0ColdBundle bundle, OrtRun run, IReadOnlyList<InkStroke> strokes,
        int width, CancellationToken token = default)
        => PrepareCore(runtime, bundle, run, strokes, width, true, token);

    internal static Math2Mode0RawOnlySource PrepareColdWithSelectedMathField(OrtRuntime runtime,
        Math2Mode0ColdBundle bundle, OrtRun run, Math2Mode0OriginalMathFieldSource actualField,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualField);
        return PrepareCore(runtime, bundle, run, actualField.Document.Strokes, null, true, actualField, token);
    }
    private static Math2Mode0RawOnlySource PrepareCore(OrtRuntime runtime,
        Math2Mode0ColdBundle bundle, OrtRun run, IReadOnlyList<InkStroke> strokes,
        int? width, bool sourceBound, CancellationToken token)
        => PrepareCore(runtime, bundle, run, strokes, width, sourceBound, null, token);

    private static Math2Mode0RawOnlySource PrepareCore(OrtRuntime runtime,
        Math2Mode0ColdBundle bundle, OrtRun run, IReadOnlyList<InkStroke> strokes,
        int? width, bool sourceBound, Math2Mode0OriginalMathFieldSource? actualField, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(runtime); ArgumentNullException.ThrowIfNull(run);
        ArgumentNullException.ThrowIfNull(strokes); token.ThrowIfCancellationRequested();
        var policy = Math2Mode0RawOnlyEf4Policy.Bind(bundle);
        if (width is < 1 or > 16) throw new ArgumentOutOfRangeException(nameof(width));
        MathFeatures features; Math2Mode0RawSourcePositions? positions = null;
        Math2Mode0OriginalMathFieldSource.Binding? field = null;
        if (actualField != null)
        {
            //Metadata envelope before claim; shared Document XY/positions is
            //reserved once by the existing source-position owner. No NN yet.
            long raw = actualField.Document.Strokes.Sum(s => (long)s.Points.Count);
            long upper = checked(actualField.AdditionalPayloadReservation + raw * 8 + MathPreprocessor.MaxFrames * 8L +
                actualField.Document.ObjectCount * 128L + 32);
            if (upper > Math2Mode0SourceBoundOutput.DefaultPayloadBudget)
                throw new NotSupportedException("Combined Math field/source-position payload exceeds budget before inference claim.");
            field = actualField.ClaimForInference(token);
        }
        try
        {
            if (sourceBound)
            {
                // Metadata is checked before cloning or feature/NN preparation.
                var prepared = field == null ? Math2Mode0RawSourcePositions.PrepareFeatures(strokes, token)
                    : Math2Mode0RawSourcePositions.PrepareFeatures(field.Document, token, field);
                features = prepared.Features; positions = prepared.Positions; positions.ValidateFeatures(features);
            }
            else features = MathPreprocessor.Process(strokes);
            if (features.Frames < 1) throw new ArgumentException("Nonempty raw pen input required.");
            field?.Validate(field.Owner, token);
            using var source = runtime.Tensor(features.Values, features.Frames, 1, 26);
            using var downsampling = runtime.Tensor(Enumerable.Repeat(1L, features.Frames).ToArray(), features.Frames, 1);
            using var ends = runtime.Tensor(features.StrokeEnds, 1, features.StrokeEnds.Length);
            token.ThrowIfCancellationRequested();
            var loop = width.HasValue
                ? Math2Mode0ColdRecordingLoop.PrepareWithPreparedPolicy(bundle, run, source, downsampling, ends,
                    width.Value, 1f, 1000f, token)
                : Math2Mode0ColdRecordingLoop.PrepareColdDefault(bundle, run, source, downsampling, ends, token);
            try
            {
                var result = new Math2Mode0RawOnlySource(loop, policy, features.Frames, features.StrokeEnds.ToArray(), positions, field);
                if (field != null) actualField!.SourcePrepared(field);
                return result;
            }
            catch { loop.Dispose(); throw; }
        }
        catch { if (field != null) actualField!.Fail(field); throw; }
    }
    internal Math2Mode0OriginalMathFieldSource.Binding ReadSelectedMathField(Math2Mode0OriginalMathFieldSource actualField)
        => WithOwner(() =>
        {
            var field = selectedField ?? throw new NotSupportedException("This source has no selected Math field provenance.");
            field.Validate(actualField); field.ValidatePrepared(); return field;
        });

    internal void RunToInitialOrder(CancellationToken token = default)
    {
        lock (gate)
        {
            Check(); token.ThrowIfCancellationRequested();
            if (history != null || ordered != null) throw new InvalidOperationException("Raw source recording is single-use.");
            history = Loop.RunWithRawAttentionHistory(token);
            ordered = Loop.OrderInitialCompleted(token);
        }
    }
    internal Math2Mode0RawOnlyEf4 ApplyEf4(CancellationToken token = default)
        => ApplyCore(Math2Mode0RawOnlyEf4.DefaultPayloadBudget, null, token);
    internal Math2Mode0RawOnlyFinalOrdering OrderFinal(CancellationToken token = default)
        => WithOwner(() => (processed ?? throw new InvalidOperationException("Successful EF4 required."))
            .OrderFinal(Math2Mode0RawOnlyFinalOrdering.DefaultPayloadBudget, null, token));
    internal Math2Mode0RawOnlyFinalOrdering OrderFinalDiagnostic(long payloadBudget,
        Action<Math2Mode0FinalOrderingStage, int, Math2Mode0RawOnlyFinalOrdering>? observer,
        CancellationToken token = default)
        => WithOwner(() => (processed ?? throw new InvalidOperationException("Successful EF4 required."))
            .OrderFinal(payloadBudget, observer, token));
    internal Math2Mode0RawOnlyEf4 ApplyWithPreparedPayloadBudget(long budget, CancellationToken token = default)
        => ApplyCore(budget, null, token);
    internal Math2Mode0RawOnlyEf4 ApplyDiagnostic(long budget,
        Action<Math2Mode0RawOnlyEf4Stage, int, Math2Mode0RawOnlyEf4> observer,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(observer);
        return ApplyCore(budget, observer, token);
    }
    private Math2Mode0RawOnlyEf4 ApplyCore(long budget,
        Action<Math2Mode0RawOnlyEf4Stage, int, Math2Mode0RawOnlyEf4>? observer, CancellationToken token)
    {
        lock (gate)
        {
            Check(); token.ThrowIfCancellationRequested();
            if (history == null || ordered == null) throw new InvalidOperationException("Same successful raw recording/order required.");
            if (processed != null) throw new InvalidOperationException("Raw EF4 is single-use, including failure.");
            if (budget < 0 || budget > Math2Mode0RawOnlyEf4.DefaultPayloadBudget)
                throw new ArgumentOutOfRangeException(nameof(budget));
            processed = Math2Mode0RawOnlyEf4.Prepare(this, ordered, history, strokeEnds, budget, token);
            processed.Run(observer, token);
            return processed;
        }
    }
    internal T WithOwner<T>(Func<T> action)
    { lock (gate) { Check(); return action(); } }
    private void Check()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        selectedField?.ValidatePrepared();
        if (!ReferenceEquals(factory, Factory)) throw new ArgumentException("Foreign raw source factory.");
    }
    // Detached raw input metadata is inspection, not a replacement-source API.
    internal long[] CopyStrokeEnds() => WithOwner(() => strokeEnds.ToArray());
    internal Math2Mode0RawSourcePositions ReadSourcePositions() => WithOwner(() => sourcePositions ??
        throw new NotSupportedException("This original source was not prepared by the named source-position factory."));
    internal Math2Mode0InitiallyOrderedCompleted ReadInitialForDiagnostic()
        => WithOwner(() => ordered ?? throw new InvalidOperationException("Not recorded."));
    internal Math2Mode0RawAttentionHistory ReadHistoryForDiagnostic()
        => WithOwner(() => history ?? throw new InvalidOperationException("Not recorded."));
    internal void ValidatePreparedOwners(Math2Mode0InitiallyOrderedCompleted initial,
        Math2Mode0RawAttentionHistory attention)
    {
        Check();
        if (!ReferenceEquals(initial, ordered) || !ReferenceEquals(attention, history))
            throw new ArgumentException("The exact same raw source order and history are required.");
    }
    internal void ValidateProcessing(Math2Mode0RawOnlyEf4 actual)
    {
        Check();
        if (!ReferenceEquals(processed, actual)) throw new ArgumentException("Foreign raw-only EF4 owner.");
    }
    internal void ValidateStrokeEndOwner(long[] actual)
    {
        Check();
        if (!ReferenceEquals(actual, strokeEnds)) throw new ArgumentException("Same private raw stroke-end owner required.");
    }
    internal Math2Mode0RawOnlyEf4Statistics CopyProcessingStatistics()
        => WithOwner(() => (processed ?? throw new InvalidOperationException("No processing owner.")).CopyStatistics());
    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            processed?.ReleasePayload();
            Loop.Dispose(); disposed = true;
        }
    }
}
