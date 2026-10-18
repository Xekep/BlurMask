using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathCompletedRecords.Probe")]

namespace InkRecognition.Math;

internal sealed partial class Math2Mode0PrefixContext
{
    private Math2Mode0CompletedRecords? completedRecords;

    internal Math2Mode0CompletedRecords CreateCompletedRecords(CancellationToken token = default)
    {
        lock (gate)
        {
            Check(); token.ThrowIfCancellationRequested();
            if (completedRecords != null)
                throw new InvalidOperationException("The context already owns its completed vector.");
            completedRecords = new(this);
            return completedRecords;
        }
    }

    internal bool OwnsCompletedRecords(Math2Mode0CompletedRecords candidate)
        => ReferenceEquals(completedRecords, candidate);

    internal T WithCompletedStorage<T>(Func<T> action)
    {
        lock (gate) { return action(); }
    }

    internal void FailCompletedRecording()
    {
        // Called while the same reentrant context gate is held. Native outer
        // failure stops decoding; retained vector/prefix writes are not erased.
        failed = true; pending = null;
    }
}

internal sealed partial class Math2Mode0PrefixStep
{
    private Math2Mode0PrefixCandidates? completedCandidates;
    private Math2Mode0CommonSecondaryFrontier? completedFrontier;
    internal Math2Mode0PrefixContext PrefixOwner => owner;

    internal Math2Mode0CommonSecondaryFrontier ReadCompletedFrontier(CancellationToken token)
        => WithActive(() =>
        {
            token.ThrowIfCancellationRequested();
            return completedFrontier ??= Math2Mode0CommonSecondaryFrontier.FromStep(this, token);
        });

    internal void BindCompletedCandidates(Math2Mode0PrefixCandidates candidates)
    {
        if (completedCandidates != null && !ReferenceEquals(completedCandidates, candidates))
            throw new ArgumentException("A completed-event Step already belongs to another candidate generation.");
        completedCandidates = candidates;
    }

    internal void ValidateCompletedSelection(Math2Mode0PrefixSelection selection)
    {
        if (completedCandidates != null && !ReferenceEquals(completedCandidates, selection.ActualCandidates))
            throw new ArgumentException("After EOS capture, selection must use those exact candidate rows.");
        completedCandidates?.ValidateCompletedConsumption();
    }
}

/// <summary>Same-Step frontier from old secondary prefixes and raw logits.
/// Contiguous copied tensors are a CPU-memory adaptation. This helper drops
/// the time axis as EEF.V30 does; it does not use last-frame log probabilities.</summary>
internal sealed class Math2Mode0CommonSecondaryFrontier
{
    private readonly int[] argmax;
    internal long Value { get; }
    internal bool Common { get; }
    internal int[] CopyArgmax() => argmax.ToArray();
    private Math2Mode0CommonSecondaryFrontier(long value, bool common, int[] argmax)
    { Value = value; Common = common; this.argmax = argmax; }

    internal static Math2Mode0CommonSecondaryFrontier FromStep(Math2Mode0PrefixStep step,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var generation = step.Generation;
        var secondary = generation.CopySecondary().Take(generation.ActiveCount).ToArray();
        if (secondary.Any(row => row.Length != generation.Offset))
            throw new NotSupportedException("Only the exact cold no-import prefix generations are supported.");
        var logits = step.CopyOutput(0);
        if (logits.Shape.Length != 3 || logits.Shape[0] < 1 ||
            logits.Shape[1] != generation.ActiveCount || logits.Shape[2] != 297 ||
            logits.Values.LongLength != checked(logits.Shape[0] * logits.Shape[1] * logits.Shape[2]))
            throw new InvalidDataException("The same actual F32[T,B,297] decoder output is required.");
        // Nonempty cold histories always visit at least one position from0;
        // empty history is exclusively the initial generation with saved0.
        return EvaluatePrepared(secondary, logits.Values.AsSpan(0, generation.ActiveCount * 297),
            297, previousFrontier: 0, token);
    }

    internal static Math2Mode0CommonSecondaryFrontier EvaluatePrepared(ushort[][] secondary,
        ReadOnlySpan<float> frameZero, int vocabulary, long previousFrontier,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (secondary.Length < 1 || secondary.Length > 16 || vocabulary < 1 ||
            secondary.Any(row => row == null || row.Length != secondary[0].Length) ||
            frameZero.Length != checked(secondary.Length * vocabulary))
            throw new ArgumentException("Complete aligned prepared rows required.");
        long frontier = previousFrontier;
        bool common = true;
        for (int at = 0; at < secondary[0].Length; at++)
        {
            token.ThrowIfCancellationRequested();
            frontier = at;
            int first = unchecked((short)secondary[0][at]);
            for (int beam = 1; beam < secondary.Length; beam++)
                if (first != secondary[beam][at]) { common = false; break; }
            if (!common) break;
        }
        int[] maxima = new int[secondary.Length];
        for (int beam = 0; beam < maxima.Length; beam++)
            maxima[beam] = FirstElementArgmax(frameZero.Slice(beam * vocabulary, vocabulary));
        if (common && maxima.Length > 1)
            common = maxima.All(index => index == maxima[0]);
        token.ThrowIfCancellationRequested();
        return new(checked(frontier + (common ? 1 : 0)), common, maxima);
    }

    private static int FirstElementArgmax(ReadOnlySpan<float> values)
    {
        int index = 0; float best = values[0];
        for (int i = 1; i < values.Length; i++)
            if (values[i] > best) { index = i; best = values[i]; }
        return index;
    }
}

internal sealed partial class Math2Mode0PrefixCandidates
{
    private readonly Dictionary<int, Math2Mode0CompletedEventLease> completedLeases = new();
    internal sealed record CompletedData(Math2Mode0PrefixStep Step, int Rank, byte[] Row,
        Math2Mode0PrefixGeneration Generation,
        Math2Mode0CommonSecondaryFrontier Frontier);

    internal void ValidateCompletedConsumption()
    {
        ValidateFallbackConsumption();
        if (completedLeases.Values.Any(lease => !lease.IsConsumed))
            throw new InvalidOperationException("Captured EOS events must be recorded before selection.");
    }

    internal Math2Mode0CompletedEventLease CaptureCompletedEvent(Math2Mode0CompletedRecords recorder,
        int eventOrdinal, CancellationToken token)
        => Math2Mode0CompletedEventLease.FromActual(recorder, this, eventOrdinal, token);

    internal Math2Mode0CompletedEventLease RegisterCompletedEvent(Math2Mode0CompletedRecords recorder,
        int eventOrdinal, Func<CompletedData, Math2Mode0CompletedEventLease> create,
        CancellationToken token)
        => step.WithActive(() =>
        {
            ArgumentNullException.ThrowIfNull(recorder); ArgumentNullException.ThrowIfNull(create);
            token.ThrowIfCancellationRequested();
            recorder.CheckOwner(step.PrefixOwner);
            if ((uint)eventOrdinal >= (uint)handoff.Scores.Length)
                throw new ArgumentOutOfRangeException(nameof(eventOrdinal));
            if (completedLeases.ContainsKey(eventOrdinal))
                throw new InvalidOperationException("An actual EOS event has already been leased.");
            var fact = handoff.Scores[eventOrdinal];
            byte[] rows = handoff.CopyAfterPruning();
            byte[] row = rows.AsSpan(checked(fact.Rank * 48), 48).ToArray();
            int parent = BinaryPrimitives.ReadInt32LittleEndian(row.AsSpan(40));
            if (parent != fact.SourceBeam || parent < 0 || parent >= step.Generation.ActiveCount ||
                fact.Rank >= step.PreparedWidth || BinaryPrimitives.ReadUInt64LittleEndian(row.AsSpan(8)) != 1 ||
                BinaryPrimitives.ReadSingleLittleEndian(row) == float.MaxValue)
                throw new InvalidDataException("The actual pruned-before-EOS row lost its owner.");
            var data = new CompletedData(step, fact.Rank, row,
                step.Generation, step.ReadCompletedFrontier(token));
            var lease = create(data);
            lease.ValidateCapture(recorder, step, this, fact.Rank);
            token.ThrowIfCancellationRequested();
            step.BindCompletedCandidates(this);
            completedLeases.Add(eventOrdinal, lease);
            return lease;
        });
}

/// <summary>A one-use event bound to actual prefix context, Step, candidate
/// generation and pruned raw48 rank. Numerical EOS facts cannot create it.</summary>
internal sealed partial class Math2Mode0CompletedEventLease
{
    private readonly Math2Mode0CompletedRecords recorder;
    private readonly Math2Mode0PrefixStep step;
    private readonly Math2Mode0PrefixCandidates candidates;
    private readonly byte[] row;
    private readonly Math2Mode0PrefixGeneration generation;
    private readonly int sourceParent;
    private bool consumed;
    internal bool IsConsumed => consumed;
    internal int Rank { get; }
    internal long Offset => step.Generation.Offset;
    internal Math2Mode0CommonSecondaryFrontier Frontier { get; }
    private Math2Mode0CompletedEventLease(Math2Mode0CompletedRecords recorder,
        Math2Mode0PrefixStep step, Math2Mode0PrefixCandidates candidates, int rank,
        byte[] row, Math2Mode0PrefixGeneration generation,
        Math2Mode0CommonSecondaryFrontier frontier)
    {
        this.recorder = recorder; this.step = step; this.candidates = candidates; Rank = rank;
        this.row = row;
        sourceParent = BinaryPrimitives.ReadInt32LittleEndian(row.AsSpan(40));
        this.generation = step.BindCompletedGeneration(generation, sourceParent);
        Frontier = frontier;
    }
    internal static Math2Mode0CompletedEventLease FromActual(Math2Mode0CompletedRecords recorder,
        Math2Mode0PrefixCandidates candidates, int eventOrdinal, CancellationToken token)
        => candidates.RegisterCompletedEvent(recorder, eventOrdinal, data =>
            new(recorder, data.Step, candidates, data.Rank, data.Row,
                data.Generation, data.Frontier), token);
    internal void ValidateCapture(Math2Mode0CompletedRecords target, Math2Mode0PrefixStep source,
        Math2Mode0PrefixCandidates actualCandidates, int rank)
    {
        if (!ReferenceEquals(recorder, target) || !ReferenceEquals(step, source) ||
            !ReferenceEquals(candidates, actualCandidates) || Rank != rank || consumed)
            throw new ArgumentException("The exact newly created event owner is required.");
    }
    internal byte[] CopyRawRecord() => row.ToArray();
    internal ushort[] CopyPrimary() => generation.CopyCompletedPrimary(sourceParent);
    internal ushort[] CopySecondary() => generation.CopyCompletedSecondary(sourceParent);
    internal int[][] CopyAncestry() => generation.CopyAncestry();

    internal void Consume(Math2Mode0CompletedRecords target, Action action, CancellationToken token)
        => step.WithActive(() =>
        {
            token.ThrowIfCancellationRequested();
            if (!ReferenceEquals(recorder, target)) throw new ArgumentException("Foreign completed-vector owner.");
            target.CheckOwner(step.PrefixOwner);
            if (consumed) throw new InvalidOperationException("EOS event was already consumed.");
            step.BindCompletedCandidates(candidates);
            ValidateFallbackConsumer(target);
            consumed = true;
            try { action(); }
            catch { step.PrefixOwner.FailCompletedRecording(); throw; }
            return true;
        });
}

internal enum Math2Mode0CompletedStage
{
    BeforeVectorGrowth, AfterVectorGrowth, BeforeLibraryBase, BeforeLibraryPool,
    BeforeLibraryAdoption, AfterCountIncrement, AfterHeaderWrites, AfterScoreWrite,
    AfterPrimaryReserve, AfterPrimaryElement, BeforeSecondaryReserve, AfterSecondaryReserve,
    AfterSecondaryCopy, ContentCleanup
}

/// <summary>Distinct typed empty Engine3E owner. Managed object/allocation
/// lifetime is an adaptation: no native pointer bytes or pool padding claimed.</summary>
internal sealed partial class Math2Mode0EmptyLibrary
{
    private sealed class EmptyPool
    {
        // Known initializer stores at00,08,18,20,28,30,38,40. Padding14..17
        // is deliberately absent from this semantic representation.
        internal readonly ulong[] Qwords = new ulong[8];
        internal bool Released;
    }
    private EmptyPool? pool;
    internal int LogicalIdentity { get; }
    internal bool IsDisposed { get; private set; }
    internal bool IsPoolReleased => pool?.Released ?? false;
    internal int ReleaseCount { get; private set; }
    internal object PoolIdentity => pool ?? throw new InvalidOperationException("Library pool not initialized.");
    internal ulong[] CopyInitializedPoolQwords()
        => pool?.Qwords.ToArray() ?? throw new InvalidOperationException("Library pool not initialized.");
    internal uint InitializedPoolDword10 => 0;
    internal int ObjectBytes => 24;
    internal int PoolBytes => 72;
    internal Math2Mode0EmptyLibrary(int identity) { LogicalIdentity = identity; }
    internal void InitializeEmptyPool()
    {
        if (pool != null || IsDisposed) throw new InvalidOperationException("Library initialized or released.");
        pool = new();
    }
    internal void Release()
    {
        if (IsDisposed) return;
        IsDisposed = true; ReleaseCount++;
        ReleaseOutputPool();
        if (pool != null) pool.Released = true;
    }
}

internal sealed record Math2Mode0CompletedRecordSnapshot(float Score, long Frontier, int Parent,
    ushort[] Primary, int PrimaryCapacity, ushort[] Secondary, int SecondaryCapacity,
    Math2Mode0EmptyLibrary Library, int NestedTensorCount);

/// <summary>Ordinary037430 logical record owner. No diagnostics, optional
/// sidecars, constraint model, ABI pointer/padding capture or final search.</summary>
internal sealed partial class Math2Mode0CompletedRecords : IDisposable
{
    private sealed class Record
    {
        internal float Score;
        internal long Frontier;
        internal int Parent = -1;
        internal readonly List<ushort> Primary = new(), Secondary = new();
        internal int PrimaryCapacity, SecondaryCapacity, NestedTensorCount;
        internal readonly Math2Mode0EmptyLibrary Library;
        internal readonly Math2Mode0CompletedEventLease Source;
        internal Record(Math2Mode0EmptyLibrary library, Math2Mode0CompletedEventLease source)
        { Library = library; Source = source; }
        internal Math2Mode0CompletedRecordSnapshot Copy() => new(Score, Frontier, Parent,
            Primary.ToArray(), PrimaryCapacity, Secondary.ToArray(), SecondaryCapacity, Library, NestedTensorCount);
    }
    private readonly Math2Mode0PrefixContext owner;
    private readonly List<Record> records = new();
    private int capacity, createdLibraries, releasedUnadoptedLibraries;
    private bool failed, disposed;
    internal Math2Mode0CompletedRecords(Math2Mode0PrefixContext owner) { this.owner = owner; }
    internal int CreatedLibraries => owner.WithCompletedStorage(() => createdLibraries);
    internal int ReleasedUnadoptedLibraries => owner.WithCompletedStorage(() => releasedUnadoptedLibraries);
    internal int Count => owner.WithCompletedStorage(() => records.Count);
    internal int Capacity => owner.WithCompletedStorage(() => capacity);
    internal bool IsFailed => owner.WithCompletedStorage(() => failed);
    // Provenance is out of band, not a new native168 field or saved attention tensor.
    internal Math2Mode0CompletedEventLease ReadSourceEvent(int ordinal)
        => owner.WithCompletedStorage(() =>
        {
            var source = records[ordinal].Source;
            if (source.IsFallback) throw new InvalidOperationException("This record has a typed last-position fallback source.");
            return source;
        });
    internal Math2Mode0CompletedRecordSnapshot[] CopyRecords()
        => owner.WithCompletedStorage(() => records.Select(record => record.Copy()).ToArray());
    internal void CheckOwner(Math2Mode0PrefixContext candidate)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (failed) throw new InvalidOperationException("A failed completed recorder cannot resume.");
        if (initialOrderClaimed) throw new InvalidOperationException("Initial ordering seals completed-record mutation.");
        if (!ReferenceEquals(owner, candidate) || !owner.OwnsCompletedRecords(this))
            throw new ArgumentException("Foreign or unregistered prefix context/vector.");
    }
    internal Math2Mode0CompletedEventLease Capture(Math2Mode0PrefixCandidates candidates,
        int eventOrdinal, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        return candidates.CaptureCompletedEvent(this, eventOrdinal, token);
    }
    internal void Append(Math2Mode0CompletedEventLease lease, CancellationToken token = default)
        => AppendCore(lease, null, token);
    // Explicit friend-only failure/cancellation observations at logical native
    // boundaries. Production owns its allocations and has no callbacks.
    internal void AppendDiagnostic(Math2Mode0CompletedEventLease lease,
        Action<Math2Mode0CompletedStage, int> observer, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(observer);
        AppendCore(lease, observer, token);
    }
    private void AppendCore(Math2Mode0CompletedEventLease lease,
        Action<Math2Mode0CompletedStage, int>? observer, CancellationToken token, bool allowFallback = false)
    {
        ArgumentNullException.ThrowIfNull(lease);
        if (lease.IsFallback != allowFallback)
            throw new ArgumentException("Use the matching ordinary-EOS or typed fallback append entry.");
        lease.Consume(this, () =>
        {
            Record? counted = null;
            Math2Mode0EmptyLibrary? unadopted = null;
            void Observe(Math2Mode0CompletedStage stage, int index = -1)
            { observer?.Invoke(stage, index); token.ThrowIfCancellationRequested(); }
            try
            {
                if (capacity < checked(records.Count + 1))
                {
                    Observe(Math2Mode0CompletedStage.BeforeVectorGrowth);
                    records.EnsureCapacity(records.Count + 1);
                    capacity = records.Count + 1;
                    Observe(Math2Mode0CompletedStage.AfterVectorGrowth);
                }
                Observe(Math2Mode0CompletedStage.BeforeLibraryBase);
                unadopted = new(records.Count + 1);
                Observe(Math2Mode0CompletedStage.BeforeLibraryPool);
                unadopted.InitializeEmptyPool(); createdLibraries++;
                Observe(Math2Mode0CompletedStage.BeforeLibraryAdoption);
                counted = new(unadopted, lease);
                records.Add(counted); unadopted = null;
                Observe(Math2Mode0CompletedStage.AfterCountIncrement);
                byte[] row = lease.CopyRawRecord();
                counted.Parent = BinaryPrimitives.ReadInt32LittleEndian(row.AsSpan(40));
                counted.Frontier = lease.Frontier.Value;
                Observe(Math2Mode0CompletedStage.AfterHeaderWrites);
                ushort[] primary = lease.CopyPrimary(), secondary = lease.CopySecondary();
                counted.Score = LegacyPreparedMathCompletionScore.WithoutConstraintModel(
                    BinaryPrimitives.ReadSingleLittleEndian(row), primary.Length, token);
                Observe(Math2Mode0CompletedStage.AfterScoreWrite);
                counted.Primary.EnsureCapacity(primary.Length);
                counted.PrimaryCapacity = primary.Length;
                Observe(Math2Mode0CompletedStage.AfterPrimaryReserve);
                for (int i = 0; i < primary.Length; i++)
                {
                    counted.Primary.Add(primary[i]);
                    Observe(Math2Mode0CompletedStage.AfterPrimaryElement, i);
                }
                Observe(Math2Mode0CompletedStage.BeforeSecondaryReserve);
                counted.Secondary.EnsureCapacity(secondary.Length);
                counted.SecondaryCapacity = secondary.Length;
                Observe(Math2Mode0CompletedStage.AfterSecondaryReserve);
                counted.Secondary.AddRange(secondary);
                Observe(Math2Mode0CompletedStage.AfterSecondaryCopy);
                // Native common epilogue destroys nested tensors and writes
                // count38=0; none exist in this no-diagnostics ordinary route.
                counted.NestedTensorCount = 0;
                Observe(Math2Mode0CompletedStage.ContentCleanup);
            }
            catch
            {
                failed = true;
                if (unadopted != null) { unadopted.Release(); releasedUnadoptedLibraries++; }
                if (counted != null) counted.NestedTensorCount = 0;
                throw;
            }
        }, token);
    }
    public void Dispose()
        => owner.WithCompletedStorage(() =>
        {
            if (!disposed)
            {
                disposed = true;
                foreach (var record in records) record.Library.Release();
            }
            return true;
        });
}
