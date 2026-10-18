namespace InkRecognition.Math;

internal enum Math2Mode0SourceAttachmentStage { Claimed, AfterRun, BeforeCompletion }
internal sealed record Math2Mode0SourceAttachmentStatistics(bool Complete, bool Failed, int Runs,
    long PayloadReservation, LegacyNativeInkPositionRange[][][] RangePrefix);

/// <summary>Actual046AE0 range attachment after full EE4 formatting. It does
/// not manufacture an SDK page, DOM node, source tag or accepted formula.</summary>
internal sealed partial class Math2Mode0SourceBoundOutput
{
    internal const long DefaultPayloadBudget = 16 * 1024 * 1024;
    internal Math2Mode0RawOnlyOutput Output { get; }
    internal LegacyOwnedRawInkDocument Document => positions.Document;
    private readonly Math2Mode0RawSourcePositions positions;
    private readonly LegacyOwnedRawInkRangeOperations ranges;
    private readonly LegacyNativeInkPositionRange[][][] attached;
    private readonly object[] records;
    private readonly long reservation;
    private bool complete, failed;
    private int runs;
    internal Math2Mode0SourceBoundOutput(Math2Mode0RawOnlyOutput output, Math2Mode0RawSourcePositions positions,
        Math2Mode0OutputRecord?[] actualRows, long reservation)
    {
        Output = output; this.positions = positions; this.reservation = reservation;
        ranges = LegacyOwnedRawInkRangeOperations.Prepare(positions.Document);
        records = new object[actualRows.Length]; attached = new LegacyNativeInkPositionRange[actualRows.Length][][];
        for (int at = 0; at < actualRows.Length; at++)
        {
            var row = actualRows[at] ?? throw new InvalidDataException("Complete actual output required.");
            records[at] = row.RecordIdentity;
            attached[at] = Enumerable.Range(0, row.UnitRows.Length).Select(_ => Array.Empty<LegacyNativeInkPositionRange>()).ToArray();
        }
    }
    internal void Run(Math2Mode0OutputRecord?[] actualRows,
        Action<Math2Mode0SourceAttachmentStage, int, Math2Mode0SourceBoundOutput>? observer, CancellationToken token)
    {
        void Observe(Math2Mode0SourceAttachmentStage stage, int at)
        { observer?.Invoke(stage, at, this); Output.ValidateSourceAttachment(this); token.ThrowIfCancellationRequested(); }
        try
        {
            Observe(Math2Mode0SourceAttachmentStage.Claimed, -1);
            for (int at = 0; at < actualRows.Length; at++)
            {
                var row = actualRows[at] ?? throw new InvalidDataException("Missing actual output.");
                int previous = 0;
                foreach (var run in row.Runs)
                {
                    token.ThrowIfCancellationRequested();
                    var interval = positions.ReadRange(previous, run.EndSegmentExclusive, token);
                    if (run.PrimaryIndex >= 0)
                        attached[at][run.PrimaryIndex] = ranges.Union(attached[at][run.PrimaryIndex], interval, token);
                    previous = run.EndSegmentExclusive; runs++;
                    Observe(Math2Mode0SourceAttachmentStage.AfterRun, at);
                }
            }
            Observe(Math2Mode0SourceAttachmentStage.BeforeCompletion, actualRows.Length); complete = true;
        }
        catch { failed = true; Output.Owner.Ef4.Source.Loop.FailRawAttentionHistory(); throw; }
    }
    internal void ValidateForCopy(Math2Mode0RawOnlyOutput actual, CancellationToken token = default)
    {
        if (!ReferenceEquals(actual, Output)) throw new ArgumentException("The exact same formatted output is required.");
        Output.WithSourceAttachment(this, () =>
        { token.ThrowIfCancellationRequested(); if (!complete || failed) throw new InvalidOperationException("Successful complete source attachment required."); return true; });
    }
    internal LegacyNativeInkPositionRange[][] CopyRanges(int record, CancellationToken token = default)
        => Output.WithSourceAttachment(this, () =>
        {
            token.ThrowIfCancellationRequested();
            if (!complete || failed) throw new InvalidOperationException("Complete actual source attachment required.");
            if ((uint)record >= (uint)attached.Length) throw new ArgumentOutOfRangeException(nameof(record));
            return attached[record].Select(row => row.ToArray()).ToArray();
        });
    internal object ReadRecordIdentity(int record) => Output.WithSourceAttachment(this, () =>
    {
        if (!complete || failed) throw new InvalidOperationException("Complete actual source attachment required.");
        return records[record];
    });
    // Detached logical source-field prefix, never accepted output.
    internal Math2Mode0SourceAttachmentStatistics CopyStatistics() => new(complete, failed, runs, reservation,
        attached.Select(record => record.Select(row => row.ToArray()).ToArray()).ToArray());
}

internal sealed partial class Math2Mode0RawOnlyOutput
{
    private bool sourceAttachmentClaimed;
    private Math2Mode0SourceBoundOutput? sourceAttachment;
    internal Math2Mode0SourceBoundOutput AttachSourcePositions(CancellationToken token = default)
        => AttachSourcePositionsDiagnostic(Math2Mode0SourceBoundOutput.DefaultPayloadBudget, null, token);
    internal Math2Mode0SourceBoundOutput AttachSourcePositionsDiagnostic(long budget,
        Action<Math2Mode0SourceAttachmentStage, int, Math2Mode0SourceBoundOutput>? observer,
        CancellationToken token = default) => owner.WithOutput(this, () =>
    {
        token.ThrowIfCancellationRequested();
        if (!complete || failed || sourceAttachmentClaimed) throw new InvalidOperationException("Complete unconsumed source attachment owner required.");
        if (budget < 0 || budget > Math2Mode0SourceBoundOutput.DefaultPayloadBudget) throw new ArgumentOutOfRangeException(nameof(budget));
        var positions = owner.Ef4.Source.ReadSourcePositions();
        long payload = checked(reservation + positions.PayloadReservation + output.Length * 8L);
        long peak = 0;
        foreach (var row in output)
        {
            token.ThrowIfCancellationRequested();
            if (row == null) throw new InvalidDataException("Complete actual output required.");
            int previous = 0;
            foreach (var run in row.Runs)
            {
                if (run.PrimaryIndex >= row.UnitRows.Length || run.EndSegmentExclusive < previous ||
                    run.EndSegmentExclusive > positions.InputFrameCount)
                    throw new InvalidDataException("Actual mapped run/source-position extent required.");
                previous = run.EndSegmentExclusive;
            }
            //Conservative source intervals per run plus peak010850/union work.
            long rangeRows = checked(row.Runs.LongLength * positions.Document.ObjectCount);
            payload = checked(payload + rangeRows * 16L);
            peak = System.Math.Max(peak, checked(rangeRows * 32L));
        }
        payload = checked(payload + peak);
        if (payload > budget) throw new NotSupportedException("Combined history/EF4/output/source payload envelope exceeds budget before claim.");
        token.ThrowIfCancellationRequested(); sourceAttachmentClaimed = true;
        try
        {
            sourceAttachment = new(this, positions, output, payload);
            sourceAttachment.Run(output, observer, token); return sourceAttachment;
        }
        catch { owner.Ef4.Source.Loop.FailRawAttentionHistory(); throw; }
    });
    internal void ValidateSourceAttachment(Math2Mode0SourceBoundOutput candidate) => WithSourceAttachment(candidate, () => true);
    internal T WithSourceAttachment<T>(Math2Mode0SourceBoundOutput candidate, Func<T> action) => owner.WithOutput(this, () =>
    {
        if (!sourceAttachmentClaimed || !ReferenceEquals(sourceAttachment, candidate))
            throw new ArgumentException("Exact source attachment owner required.");
        return action();
    });
}
