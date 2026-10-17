using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathPrefixState.Probe")]

namespace InkRecognition.Math;

/// <summary>Cold no-import/no-detection prefix and tensor ownership. Width,
/// temperature and delta remain prepared controller inputs; this is not the
/// EEE search scheduler or a final-hypothesis/acceptance implementation.</summary>
internal sealed partial class Math2Mode0PrefixContext : IDisposable
{
    private readonly object gate = new();
    private readonly Math2Mode0TensorState state;
    private Math2Mode0PrefixGeneration generation;
    private Math2Mode0PrefixStep? pending;
    private bool failed, disposed;
    internal int BeamWidth { get; }
    internal long InputFrameCount { get; }
    internal long EncoderMaximumPredictionCount => state.MaximumPredictionCount;
    //The exact cold numeric bound only. RunStep remains a diagnostic step API;
    //this property does not implement outer termination or maxstep fallback.
    internal ulong ColdMaximumPosition { get; }

    private Math2Mode0PrefixContext(Math2Mode0TensorState state, int width, long inputFrameCount)
    {
        this.state = state; BeamWidth = width;
        InputFrameCount = inputFrameCount;
        ulong maximum = System.Math.Min(1000UL, checked(4UL * (ulong)inputFrameCount + 1UL));
        if (state.MaximumPredictionCount != 0)
            maximum = System.Math.Min(maximum, checked((ulong)state.MaximumPredictionCount));
        ColdMaximumPosition = maximum;
        generation = Math2Mode0PrefixGeneration.Cold(width);
    }

    internal static Math2Mode0PrefixContext Prepare(Math2Mode0ColdBundle bundle, OrtRun run,
        OrtTensor source, OrtTensor downsampling, OrtTensor strokeEnds,
        int preparedBeamWidth, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        token.ThrowIfCancellationRequested();
        if (preparedBeamWidth < 1 || preparedBeamWidth > 16)
            throw new ArgumentOutOfRangeException(nameof(preparedBeamWidth));
        var state = bundle.PrepareState(run, source, downsampling, strokeEnds, token);
        //PrepareState already validated this same source as F32[N,1,26].
        //Keep its original N, never the encoder memory length or stroke count.
        try { return new(state, preparedBeamWidth, source.Describe().Shape[0]); }
        catch { state.Dispose(); throw; }
    }

    internal Math2Mode0PrefixStep RunStep(CancellationToken token = default)
    {
        lock (gate)
        {
            Check(); token.ThrowIfCancellationRequested();
            if (pending != null) throw new InvalidOperationException("Consume the exact pending prefix step first.");
            if (generation.Offset >= 1000) throw new NotSupportedException("Bounded prefix history is limited to1000 updates; no native maxstep policy is inferred.");
            try
            {
                CheckStateGeneration();
                pending = new(this, state.RunStep(token), generation);
                return pending;
            }
            catch { failed = true; throw; }
        }
    }

    internal void ApplySelected(Math2Mode0PrefixSelection selected, CancellationToken token = default)
        => ApplyCore(selected, null, token);

    // Friend-only fault observation; production has no mid-update callback.
    internal void ApplySelectedDiagnostic(Math2Mode0PrefixSelection selected,
        Action afterPrefixWrites, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(afterPrefixWrites);
        ApplyCore(selected, afterPrefixWrites, token);
    }

    private void ApplyCore(Math2Mode0PrefixSelection selected, Action? afterPrefixWrites,
        CancellationToken token)
    {
        lock (gate)
        {
            ArgumentNullException.ThrowIfNull(selected);
            Check(); token.ThrowIfCancellationRequested();
            CheckStep(selected.Step);
            selected.Step.ValidateCompletedSelection(selected);
            selected.Rows.Check();
            if (selected.Rows.Count > BeamWidth)
                throw new ArgumentException("Selected rows exceed this exact prefix context's width.");
            // All shape/owner checks above precede a consuming operation.
            // Later failure is not rolled back to a reusable generation.
            try
            {
                ObserveAttentionSelection(selected);
                var next = generation.Append(selected.Rows.CopyRecords(), token);
                generation = next; // Native prefix writes precede EF1 reindex.
                afterPrefixWrites?.Invoke();
                token.ThrowIfCancellationRequested();
                state.ApplySelected(selected.Rows, token);
                pending = null;
                CheckStateGeneration();
            }
            catch { failed = true; pending = null; throw; }
        }
    }

    internal T WithStep<T>(Math2Mode0PrefixStep step, Func<T> action)
    {
        lock (gate) { CheckStep(step); return action(); }
    }
    private void CheckStep(Math2Mode0PrefixStep step)
    {
        Check(); CheckStateGeneration();
        if (!ReferenceEquals(pending, step) || !ReferenceEquals(step.Generation, generation))
            throw new ArgumentException("The exact active prefix/Step generation is required.");
        state.CheckStep(step.TensorStep);
    }
    private void CheckStateGeneration()
    {
        if (state.Offset != generation.Offset || state.BeamCount != generation.ActiveCount)
            throw new InvalidOperationException("Prefix and tensor generations no longer agree.");
    }
    private void Check()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (failed) throw new InvalidOperationException("A failed combined prefix context cannot be resumed.");
        state.Check();
    }
    internal Math2Mode0PrefixGeneration CopyGeneration()
    {
        lock (gate) { ObjectDisposedException.ThrowIf(disposed, this); return generation.Copy(); }
    }
    internal bool IsFailed { get { lock (gate) return failed; } }
    internal (string Name, int Type, long[] Shape, float[]? Floats, long[]? Int64s)[] CopyTensorInputs()
    { lock (gate) { Check(); return state.CopyInputs(); } }
    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            disposed = true; pending = null; completedRecords?.Dispose(); state.Dispose();
        }
    }
}

/// <summary>Immutable owned copies. The rank/time ancestry is intentionally
/// separate from the parent-gathered U16 primary/secondary histories.</summary>
internal sealed class Math2Mode0PrefixGeneration
{
    private readonly ushort[][] primary, secondary;
    private readonly int[][] ancestry;
    internal long Offset { get; }
    internal int ActiveCount { get; }
    internal int Width => primary.Length;
    private Math2Mode0PrefixGeneration(long offset, int active, ushort[][] primary,
        ushort[][] secondary, int[][] ancestry)
    { Offset = offset; ActiveCount = active; this.primary = primary; this.secondary = secondary; this.ancestry = ancestry; }
    internal static Math2Mode0PrefixGeneration Cold(int width)
    {
        if (width < 1 || width > 16) throw new ArgumentOutOfRangeException(nameof(width));
        return new(0, 1, Enumerable.Range(0, width).Select(_ => Array.Empty<ushort>()).ToArray(),
            Enumerable.Range(0, width).Select(_ => Array.Empty<ushort>()).ToArray(),
            Enumerable.Range(0, width).Select(_ => new[] { -1 }).ToArray());
    }
    internal ushort[][] CopyPrimary() => primary.Select(x => x.ToArray()).ToArray();
    internal ushort[][] CopySecondary() => secondary.Select(x => x.ToArray()).ToArray();
    internal int[][] CopyAncestry() => ancestry.Select(x => x.ToArray()).ToArray();
    internal Math2Mode0PrefixGeneration Copy() => new(Offset, ActiveCount,
        CopyPrimary(), CopySecondary(), CopyAncestry());
    internal Math2Mode0PrefixGeneration Append(ReadOnlySpan<byte> records, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (records.Length == 0 || records.Length % 48 != 0 || records.Length / 48 > Width)
            throw new ArgumentException("Complete selected rows within the retained width required.");
        int count = records.Length / 48;
        var nextPrimary = new ushort[Width][]; var nextSecondary = new ushort[Width][];
        var nextAncestry = new int[Width][];
        for (int rank = 0; rank < Width; rank++)
        {
            token.ThrowIfCancellationRequested();
            int parent = -1;
            if (rank < count)
            {
                var row = records.Slice(rank * 48, 48);
                parent = BinaryPrimitives.ReadInt32LittleEndian(row[40..]);
                if (parent < 0 || parent >= ActiveCount)
                    throw new ArgumentException("Parent outside the exact preceding active generation.");
                ulong alternate = BinaryPrimitives.ReadUInt64LittleEndian(row[32..]);
                ulong label = alternate == ulong.MaxValue ? BinaryPrimitives.ReadUInt64LittleEndian(row[8..]) : alternate;
                nextPrimary[rank] = [.. primary[parent], unchecked((ushort)label)];
                nextSecondary[rank] = [.. secondary[parent], BinaryPrimitives.ReadUInt16LittleEndian(row[16..])];
            }
            else { nextPrimary[rank] = []; nextSecondary[rank] = []; }
            nextAncestry[rank] = [.. ancestry[rank], parent];
        }
        token.ThrowIfCancellationRequested();
        return new(checked(Offset + 1), count, nextPrimary, nextSecondary, nextAncestry);
    }
}

internal sealed partial class Math2Mode0PrefixStep
{
    private readonly Math2Mode0PrefixContext owner;
    internal Math2Mode0DecoderStep TensorStep { get; }
    internal Math2Mode0PrefixGeneration Generation { get; }
    internal int PreparedWidth => owner.BeamWidth;
    internal Math2Mode0PrefixStep(Math2Mode0PrefixContext owner, Math2Mode0DecoderStep step,
        Math2Mode0PrefixGeneration generation)
    { this.owner = owner; TensorStep = step; Generation = generation; }
    internal (float[] Values, long[] Shape) CopyOutput(int ordinal)
        => owner.WithStep(this, () => TensorStep.CopyOutput(ordinal));
    internal float[] CopyPriorCosts()
        => owner.WithStep(this, () => TensorStep.Owner.CopyPriorCosts(TensorStep));
    internal Math2Mode0PrefixCandidates CreateCandidates(float preparedTemperature, float preparedDelta,
        CancellationToken token = default)
        => Math2Mode0PrefixCandidates.Create(this, preparedTemperature, preparedDelta, token);
    internal Math2Mode0PrefixSelection BindDiagnosticParents(int[] preparedParents,
        ushort[] preparedSymbols, float preparedTemperature = 1f, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(preparedParents); ArgumentNullException.ThrowIfNull(preparedSymbols);
        return owner.WithStep(this, () =>
        {
            token.ThrowIfCancellationRequested();
            if (preparedParents.Length < 1 || preparedParents.Length > owner.BeamWidth ||
                preparedParents.Length != preparedSymbols.Length || preparedParents.Any(x => x < 0 || x >= TensorStep.BeamCount) ||
                preparedSymbols.Any(x => x >= 297))
                throw new ArgumentException("Explicit diagnostic parents/labels must fit this generation.");
            var scores = Math2Mode0ActualSidecars.Create(TensorStep, preparedTemperature, token);
            var costs = scores.CopyMaskedCosts(); var information = scores.CopyInformation();
            byte[] rows = new byte[preparedParents.Length * 48];
            for (int i = 0; i < preparedParents.Length; i++)
            {
                var row = rows.AsSpan(i * 48, 48); int parent = preparedParents[i];
                BinaryPrimitives.WriteSingleLittleEndian(row, costs[parent * 297 + preparedSymbols[i]]);
                BinaryPrimitives.WriteUInt64LittleEndian(row[8..], preparedSymbols[i]);
                information.AsSpan(parent * 8, 8).CopyTo(row[16..]);
                BinaryPrimitives.WriteUInt32LittleEndian(row[24..], uint.MaxValue);
                BinaryPrimitives.WriteUInt64LittleEndian(row[32..], ulong.MaxValue);
                BinaryPrimitives.WriteInt32LittleEndian(row[40..], parent);
            }
            token.ThrowIfCancellationRequested();
            return Math2Mode0PrefixSelection.FromDiagnostic(this, rows);
        });
    }
    internal T WithActive<T>(Func<T> action) => owner.WithStep(this, action);
}

internal sealed class Math2Mode0PrefixSelection
{
    internal Math2Mode0PrefixStep Step { get; }
    internal Math2Mode0SelectedRows Rows { get; }
    internal Math2Mode0PrefixCandidates? ActualCandidates { get; }
    internal bool IsDiagnostic => Rows.IsExplicitPreparedSelection;
    private Math2Mode0PrefixSelection(Math2Mode0PrefixStep step, Math2Mode0SelectedRows rows,
        Math2Mode0PrefixCandidates? actualCandidates = null)
    {
        if (!ReferenceEquals(step.TensorStep, rows.Step))
            throw new ArgumentException("Selected tensor rows must have this exact Step identity.");
        Step = step; Rows = rows; ActualCandidates = actualCandidates;
    }
    internal static Math2Mode0PrefixSelection FromActual(Math2Mode0PrefixCandidates candidates)
    {
        var owned = candidates.CreateSelectedRows();
        return new(owned.Step, owned.Rows, candidates);
    }
    internal static Math2Mode0PrefixSelection FromDiagnostic(Math2Mode0PrefixStep step, ReadOnlySpan<byte> records)
        => new(step, new Math2Mode0SelectedRows(step.TensorStep, records, prepared: true));
    internal byte[] CopyRecords() => Rows.CopyRecords();
}

internal sealed class Math2Mode0ActualSidecars
{
    private readonly LegacyPortableMathLogProbabilities scores;
    private readonly byte[] information, conditional;
    private Math2Mode0ActualSidecars(LegacyPortableMathLogProbabilities scores, byte[] information,
        byte[] conditional)
    { this.scores = scores; this.information = information; this.conditional = conditional; }
    internal static Math2Mode0ActualSidecars Create(Math2Mode0DecoderStep step, float temperature, CancellationToken token)
    {
        step.Owner.CheckStep(step); token.ThrowIfCancellationRequested();
        var logits = step.CopyOutput(0);
        var scores = LegacyPortableMathLogProbabilities.TransformLastFrame(logits.Values, 1,
            step.BeamCount, 297, temperature, step.Owner.CopyPriorCosts(step), token);
        var logs = scores.CopyLogProbabilities();
        byte[] information = new byte[step.BeamCount * 8], conditional = new byte[step.BeamCount * 4];
        for (int beam = 0; beam < step.BeamCount; beam++)
        {
            token.ThrowIfCancellationRequested();
            BinaryPrimitives.WriteInt64LittleEndian(information.AsSpan(beam * 8),
                FirstStrictMaximum(logs.AsSpan(beam * 297, 297)));
            BinaryPrimitives.WriteUInt32LittleEndian(conditional.AsSpan(beam * 4), uint.MaxValue);
        }
        return new(scores, information, conditional);
    }
    internal static long FirstStrictMaximum(ReadOnlySpan<float> values)
    {
        float best = -float.MaxValue; long index = -1;
        for (int i = 0; i < values.Length; i++)
            if (values[i] > best) { best = values[i]; index = i; }
        return index;
    }
    internal float[] CopyLogProbabilities() => scores.CopyLogProbabilities();
    internal float[] CopyUnmaskedCosts() => scores.CopyCumulativeCosts();
    internal float[] CopyMaskedCosts()
    {
        var costs = scores.CopyCumulativeCosts();
        //Derive the masked offer copy after argmax without retaining another
        //full score buffer in the candidate owner.
        for (int beam = 0; beam < information.Length / 8; beam++) costs[beam * 297] = float.MaxValue;
        return costs;
    }
    internal byte[] CopyInformation() => information.ToArray();
    internal byte[] CopyConditional() => conditional.ToArray();
}

internal sealed partial class Math2Mode0PrefixCandidates
{
    private readonly Math2Mode0PrefixStep step;
    private readonly Math2Mode0ActualSidecars sidecars;
    private readonly LegacyPreparedMathEosHandoff handoff;
    private Math2Mode0PrefixCandidates(Math2Mode0PrefixStep step, Math2Mode0ActualSidecars sidecars,
        LegacyPreparedMathEosHandoff handoff)
    { this.step = step; this.sidecars = sidecars; this.handoff = handoff; }
    internal static Math2Mode0PrefixCandidates Create(Math2Mode0PrefixStep step,
        float temperature, float delta, CancellationToken token)
        => step.WithActive(() => CreateCore(step, temperature, delta, token));
    private static Math2Mode0PrefixCandidates CreateCore(Math2Mode0PrefixStep step,
        float temperature, float delta, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        int width = step.PreparedWidth;
        var sidecars = Math2Mode0ActualSidecars.Create(step.TensorStep, temperature, token);
        var costs = sidecars.CopyMaskedCosts(); byte[] bytes = new byte[costs.Length * 4];
        for (int i = 0; i < costs.Length; i++) BinaryPrimitives.WriteSingleLittleEndian(bytes.AsSpan(i * 4), costs[i]);
        var candidates = LegacyPreparedMathUnrestrictedCandidates.Expand(bytes, step.TensorStep.BeamCount,
            297, width, sidecars.CopyInformation(), sidecars.CopyConditional(), token);
        var prefixes = step.Generation.CopyPrimary().Take(step.TensorStep.BeamCount).ToArray();
        var handoff = LegacyPreparedMathEosHandoff.Apply(candidates, delta,
            step.TensorStep.Owner.ObservableGate, prefixes, token);
        token.ThrowIfCancellationRequested();
        return new(step, sidecars, handoff);
    }
    internal Math2Mode0PrefixSelection Select() => Math2Mode0PrefixSelection.FromActual(this);
    internal (Math2Mode0PrefixStep Step, Math2Mode0SelectedRows Rows) CreateSelectedRows()
        => step.WithActive(() => (step, new Math2Mode0SelectedRows(step.TensorStep,
            handoff.CopyAfterResort().AsSpan(0, handoff.ActiveCount * 48), prepared: false)));
    internal float[] CopyLogProbabilities() => sidecars.CopyLogProbabilities();
    internal float[] CopyUnmaskedCosts() => sidecars.CopyUnmaskedCosts();
    internal byte[] CopyInformation() => sidecars.CopyInformation();
    internal byte[] CopyConditional() => sidecars.CopyConditional();
    internal byte[] CopyHeapStorage() => handoff.Source.CopyHeapStorage();
    internal byte[] CopySortedStorage() => handoff.Source.CopySortedStorage();
    internal byte[] CopyAfterEos() => handoff.CopyAfterEos();
    internal byte[] CopyAfterResort() => handoff.CopyAfterResort();
    internal LegacyPreparedMathEosScore[] CopyCompletedScoreFacts() => handoff.Scores
        .Select(x => x with { Prefix = x.Prefix.ToArray() }).ToArray();
}
