namespace InkRecognition.Math;

internal enum Math2Mode0RawOnlyEf4Stage { Claimed, BeforeUnits, AfterUnits, AfterAssignment, AfterInsertionMask, AfterPruning, AfterScore, BeforeCompletion }
internal sealed record Math2Mode0RawOnlyEf4Statistics(long Budget, long PlannedPayloadEnvelope,
    int StartedRecords, int ScoredRecords, int SkippedEmptyRecords, bool Complete, bool Failed,
    string[] LastStages, float[] StoredScores, int[] UnitCounts, int[] AssignmentCounts,
    int[] InitialMaskCounts, int[] DeletionMaskCounts, int[] RunCounts);
internal sealed record Math2Mode0RawOnlyEf4RecordCopy(Math2Mode0CompletedEventLease Source,
    Math2Mode0EmptyLibrary Library, object RecordIdentity, object SourceFactoryIdentity,
    ushort[] Primary, int Segments, bool SkippedEmpty, uint[] Units, int[] Assignments, MathSegmentRun[] Runs,
    byte[] InitialMask, byte[] DeletionMask, float Before, float Penalty, float After);

/// <summary>Same actual records after the restricted library raw-only byte1
/// EF4 branch. No final ordering, truncation, formula or accepted output.</summary>
internal sealed class Math2Mode0RawOnlyEf4
{
    internal const long DefaultPayloadBudget = 16L * 1024 * 1024;
    private sealed class Cell(Math2Mode0CompletedRecords.RawOnlyRecord record)
    {
        internal readonly Math2Mode0CompletedRecords.RawOnlyRecord Record = record;
        internal uint[] Units = [];
        internal int[] Assignments = [];
        internal MathSegmentRun[] Runs = [];
        internal byte[] Initial = [], Deleted = [];
        internal string Phase = "unstarted";
        internal int Segments;
        internal float Before, Penalty;
        internal void Release() { Units = []; Assignments = []; Runs = []; Initial = []; Deleted = []; }
    }
    private readonly Math2Mode0InitiallyOrderedCompleted ordered;
    private readonly Math2Mode0RawAttentionHistory history;
    private readonly long[] strokeEnds;
    private readonly Cell[] cells;
    private readonly long budget, planned;
    private bool claimed, complete, failed, released;
    internal Math2Mode0RawOnlySource Source { get; }
    internal bool IsFinalOrder => false;
    internal bool IsAcceptedFormula => false;
    private Math2Mode0RawOnlyEf4(Math2Mode0RawOnlySource source,
        Math2Mode0InitiallyOrderedCompleted ordered, Math2Mode0RawAttentionHistory history,
        long[] ends, Cell[] cells, long budget, long planned)
    { Source = source; this.ordered = ordered; this.history = history; strokeEnds = ends;
      this.cells = cells; this.budget = budget; this.planned = planned; }

    internal static Math2Mode0RawOnlyEf4 Prepare(Math2Mode0RawOnlySource source,
        Math2Mode0InitiallyOrderedCompleted ordered, Math2Mode0RawAttentionHistory history,
        long[] ownedEnds, long budget, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(ordered);
        ArgumentNullException.ThrowIfNull(history); ArgumentNullException.ThrowIfNull(ownedEnds);
        token.ThrowIfCancellationRequested();
        source.ValidatePreparedOwners(ordered, history);
        source.ValidateStrokeEndOwner(ownedEnds);
        if (budget < 0 || budget > DefaultPayloadBudget) throw new ArgumentOutOfRangeException(nameof(budget));
        var records = source.Loop.PrepareRawOnlyRecords(source, ordered, history, token);
        var stats = source.Loop.CopyRawAttentionStatistics();
        if (!stats.Complete || stats.Failed || stats.SnapshotCount != 0)
            throw new NotSupportedException("Raw-only EF4 owns first reconstruction of its complete history.");
        long retained = checked(stats.RawPayloadBytes + ownedEnds.LongLength * 8), peakWorkspace = 0;
        foreach (var record in records)
        {
            token.ThrowIfCancellationRequested(); record.Validate();
            int n = record.PrimaryCount;
            if (!ShouldProcessPrimaryPrepared(n))
            {
                if (record.Source.Offset != 0) throw new InvalidDataException("Empty record/source prefix mismatch.");
                continue; // Native036FAC skips BEFORE attention and finite score processing.
            }
            int s = record.Source.RawOnlySegmentCount;
            if (s is < 1 or > 6000 || (long)n * s > 1_000_000 ||
                n != record.Source.Offset || !float.IsFinite(record.Score) ||
                !ReferenceEquals(record.RawSource, source))
                throw new NotSupportedException("Complete finite actual old-prefix geometry required.");
            var alphabet = record.Source.ReadAttentionAlphabet();
            bool observable = false;
            for (int i = 0; i < n; i++)
            {
                ushort label = record.PrimaryAt(i);
                if (label >= 297) throw new NotSupportedException("Only actual unrestricted decoder labels are supported.");
                observable |= alphabet.IsObservable(label);
            }
            if (!observable) throw new NotSupportedException("Actual recorded observable old-prefix gate required.");
            for (int i = 0; i < ownedEnds.Length; i++)
                if (ownedEnds[i] < 1 || ownedEnds[i] > s || i > 0 && ownedEnds[i] <= ownedEnds[i - 1])
                    throw new NotSupportedException("The same actual stroke-end coordinates must address encoder segments.");
            // Snapshot cache is shared, counted once per distinct actual Source.
            // The conservative workspace includes raw copy, activated+filtered
            // tensors and all numerical helper payloads; not CLR/RSS overhead.
            retained = checked(retained + 4L * n * s + 32L * n + 16L * s);
            peakWorkspace = System.Math.Max(peakWorkspace,
                checked(16L * n * s + 64L * (n + s + ownedEnds.Length)));
        }
        token.ThrowIfCancellationRequested();
        return new(source, ordered, history, ownedEnds, records.Select(record => new Cell(record)).ToArray(),
            budget, checked(retained + peakWorkspace));
    }

    internal void Run(Action<Math2Mode0RawOnlyEf4Stage, int, Math2Mode0RawOnlyEf4>? observer,
        CancellationToken token)
    {
        if (claimed) throw new InvalidOperationException("EF4 postprocessing is single-use.");
        token.ThrowIfCancellationRequested();
        Source.Loop.ClaimRawOnlyPostprocessing(this, ordered, history, token);
        claimed = true;
        Source.Loop.WithRawOnlyPostprocessing(this, () =>
        { ExecuteCore(observer, token); return true; });
    }
    private void ExecuteCore(Action<Math2Mode0RawOnlyEf4Stage, int, Math2Mode0RawOnlyEf4>? observer,
        CancellationToken token)
    {
        void Observe(Math2Mode0RawOnlyEf4Stage stage, int index)
        {
            observer?.Invoke(stage, index, this);
            Source.Loop.WithRawOnlyPostprocessing(this, () => true);
            token.ThrowIfCancellationRequested();
        }
        try
        {
            if (planned > budget) throw new NotSupportedException("Total history/EF4 payload envelope exceeds the explicit budget.");
            Observe(Math2Mode0RawOnlyEf4Stage.Claimed, -1);
            for (int index = 0; index < cells.Length; index++)
            {
                var cell = cells[index]; var record = cell.Record;
                record.Validate();
                if (!ShouldProcessPrimaryPrepared(record.PrimaryCount))
                {
                    cell.Before = record.Score; cell.Phase = "skipped-empty";
                    continue; // No snapshot, EF4 callbacks, output writes or Score store.
                }
                Observe(Math2Mode0RawOnlyEf4Stage.BeforeUnits, index);
                var attention = history.CaptureCompleted(record.Source, token);
                if (!ReferenceEquals(attention.Source, record.Source) || attention.Symbols != record.PrimaryCount ||
                    attention.Segments != record.Source.RawOnlySegmentCount)
                    throw new InvalidDataException("Foreign actual historical attention geometry.");
                float[] raw = attention.CopyAttention(token);
                float[] activated = LegacyPortableMathSegmentation.ActivatePreparedSigmoid(raw,
                    attention.Symbols, attention.Segments, token);
                int n = attention.Symbols, s = attention.Segments;
                cell.Segments = s;
                var alphabet = record.Source.ReadAttentionAlphabet();
                int[] visible = Enumerable.Range(0, n).Where(i => alphabet.IsObservable(record.PrimaryAt(i))).ToArray();
                float[] filtered = new float[checked(visible.Length * s)];
                for (int i = 0; i < visible.Length; i++)
                    activated.AsSpan(visible[i] * s, s).CopyTo(filtered.AsSpan(i * s));
                cell.Before = record.Score;
                cell.Units = new uint[n];
                for (int i = 0; i < n; i++) cell.Units[i] = 0x01000000u | record.PrimaryAt(i);
                cell.Phase = "units50";
                Observe(Math2Mode0RawOnlyEf4Stage.AfterUnits, index);
                cell.Assignments = LegacyPreparedFreshMathSegments.AssignPreparedProbabilities(filtered, visible, s, token);
                if (cell.Assignments.Any(value => value < 0))
                    throw new NotSupportedException("Unassigned insertion branch is outside this actual finite observable scope.");
                //041240 has no -1 entry to insert;041EC0 has no inserted bits
                // and this actual recorder owns no auxiliary98 lanes.
                cell.Phase = "assignment";
                Observe(Math2Mode0RawOnlyEf4Stage.AfterAssignment, index);
                cell.Initial = new byte[n]; //041240 ->0401B0 explicitly appends zeros.
                cell.Phase = "insertion-mask";
                Observe(Math2Mode0RawOnlyEf4Stage.AfterInsertionMask, index);
                var pruned = LegacyPreparedMathUnusedUnitPruning.EvaluatePrepared(cell.Units,
                    cell.Assignments, strokeEnds, alphabet, token);
                cell.Units = pruned.CopyUnits(); cell.Assignments = pruned.CopyAssignments();
                cell.Deleted = pruned.CopyDeletionMask();
                if (!pruned.PreservesInsertedUnitsPrepared(cell.Initial, token))
                    throw new InvalidDataException("Native inserted-position preservation guard failed.");
                var separators = LegacyPreparedMathSeparatorCompaction.EvaluatePrepared(cell.Units,
                    Source.Policy.Separator, cell.Assignments, token);
                cell.Units = separators.CopyUnits(); cell.Assignments = separators.CopyAssignments();
                cell.Runs = CompressRuns(cell.Assignments, token);
                cell.Phase = "pruned50/runs68";
                Observe(Math2Mode0RawOnlyEf4Stage.AfterPruning, index);
                // Deliberately ORIGINAL activated tensor + INITIAL insertion
                // mask, with assignments already remapped by041660/041BA0.
                var score = EvaluateLiteralScorePrepared(cell.Initial,
                    activated, s, cell.Assignments, cell.Before, Source.Policy.Alpha, token);
                cell.Penalty = score.Penalty;
                token.ThrowIfCancellationRequested(); record.StoreScore(this, score.After);
                cell.Phase = "score";
                Observe(Math2Mode0RawOnlyEf4Stage.AfterScore, index);
            }
            Observe(Math2Mode0RawOnlyEf4Stage.BeforeCompletion, cells.Length);
            complete = true;
        }
        catch { failed = true; Source.Loop.FailRawAttentionHistory(); throw; }
    }
    internal static bool ShouldProcessPrimaryPrepared(int primaryCount)
    {
        if (primaryCount is < 0 or > 1000) throw new ArgumentOutOfRangeException(nameof(primaryCount));
        return primaryCount != 0;
    }
    // Numerical seam only: explicit arrays do not create a Source, Record or
    // accepted result. The owning caller supplies its authenticated phase data.
    internal static LegacyPreparedMaskedMathSegmentScore EvaluateLiteralScorePrepared(
        ReadOnlySpan<byte> initialMask, ReadOnlySpan<float> originalActivated, int segments,
        ReadOnlySpan<int> remappedAssignments, float before, float alpha, CancellationToken token = default)
        => LegacyPreparedMaskedMathSegmentScore.EvaluatePrepared(initialMask, originalActivated,
            segments, remappedAssignments, before, alpha, token);
    //041A30 also emits absent(-1) runs after stroke-boundary suppression.
    private static MathSegmentRun[] CompressRuns(int[] assignments, CancellationToken token)
    {
        var runs = new List<MathSegmentRun>();
        if (assignments.Length == 0) return [];
        int previous = assignments[0];
        for (int i = 1; i < assignments.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            if (assignments[i] == previous) continue;
            runs.Add(new(previous, i)); previous = assignments[i];
        }
        runs.Add(new(previous, assignments.Length)); token.ThrowIfCancellationRequested();
        return runs.ToArray();
    }
    internal Math2Mode0RawOnlyEf4RecordCopy[] CopyRecords()
        => Source.WithOwner(() => Source.Loop.WithRawOnlyPostprocessing(this, () =>
        {
            if (!complete || failed || released) throw new InvalidOperationException("Complete successful EF4 stage required.");
            return cells.Select(cell => new Math2Mode0RawOnlyEf4RecordCopy(cell.Record.Source,
                cell.Record.Library, cell.Record.RecordIdentity, Source.SourceFactoryIdentity,
                cell.Record.Source.CopyPrimary(), cell.Segments, cell.Phase == "skipped-empty",
                cell.Units.ToArray(), cell.Assignments.ToArray(), cell.Runs.ToArray(), cell.Initial.ToArray(),
                cell.Deleted.ToArray(), cell.Before, cell.Penalty, cell.Record.Score)).ToArray();
        }));
    internal Math2Mode0RawOnlyEf4Statistics CopyStatistics()
        => new(budget, planned, cells.Count(cell => cell.Phase != "unstarted" && cell.Phase != "skipped-empty"),
            cells.Count(cell => cell.Phase == "score"), cells.Count(cell => cell.Phase == "skipped-empty"), complete, failed,
            cells.Select(cell => cell.Phase).ToArray(), cells.Select(cell => cell.Record.Score).ToArray(),
            cells.Select(cell => cell.Units.Length).ToArray(), cells.Select(cell => cell.Assignments.Length).ToArray(),
            cells.Select(cell => cell.Initial.Length).ToArray(), cells.Select(cell => cell.Deleted.Length).ToArray(),
            cells.Select(cell => cell.Runs.Length).ToArray());
    internal void ReleasePayload()
    { released = true; foreach (var cell in cells) cell.Release(); }
}
