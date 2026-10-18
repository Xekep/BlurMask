using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathOrdinaryOutput.Probe")]

namespace InkRecognition.Math;

internal enum Math2Mode0OutputStage { Claimed, BeforeFormatter, AfterUnits, AfterRunMap,
    AfterDecimal, BeforeRows, AfterIntern, AfterRow, BeforeCompletion }
internal sealed record Math2Mode0OutputStatistics(long PayloadReservation, int StartedRecords,
    int WrittenRows, bool Complete, bool Failed, string[] Phases, uint[][] UnitPrefix,
    MathSegmentRun[][] RunPrefix, int?[][] AlternativeIdentityPrefix);
internal sealed record Math2Mode0OutputUnit(uint Unit, LegacyNativeInternedUtf8 Text,
    int ByteCount, uint ScalarBits, byte Flags, uint Metadata);
internal sealed record Math2Mode0OutputRecord(object RecordIdentity, Math2Mode0CompletedEventLease Source,
    Math2Mode0EmptyLibrary Library, float Score, uint[] Units, ushort[] OldToNewMap,
    MathSegmentRun[] Runs, Math2Mode0OutputUnit[] UnitRows);

/// <summary>Ordinary rich-record output only. UTF8 alternatives remain separate;
/// this does not transport a formula tree or assert recognition acceptance.</summary>
internal sealed class Math2Mode0RawOnlyOutput
{
    private readonly Math2Mode0RawOnlyFinalOrdering owner;
    private readonly Math2Mode0RawOnlyOutputPolicy policy;
    private readonly object[] records;
    private readonly Math2Mode0OutputRecord?[] output;
    private readonly string[] phases;
    private readonly uint[][] unitPrefix;
    private readonly MathSegmentRun[][] runPrefix;
    private readonly Math2Mode0OutputUnit[][] rowPrefix;
    private readonly long reservation;
    private bool complete, failed;
    private int started, rows;
    internal Math2Mode0RawOnlyFinalOrdering Owner => owner;
    internal Math2Mode0RawOnlyOutputPolicy Policy => policy;
    internal Math2Mode0RawOnlyOutput(Math2Mode0RawOnlyFinalOrdering owner,
        Math2Mode0RawOnlyOutputPolicy policy, object[] records, long reservation)
    { this.owner = owner; this.policy = policy; this.records = records; this.reservation = reservation;
      output = new Math2Mode0OutputRecord?[records.Length]; phases = Enumerable.Repeat("unstarted", records.Length).ToArray();
      unitPrefix = Enumerable.Range(0, records.Length).Select(_ => Array.Empty<uint>()).ToArray();
      runPrefix = Enumerable.Range(0, records.Length).Select(_ => Array.Empty<MathSegmentRun>()).ToArray();
      rowPrefix = Enumerable.Range(0, records.Length).Select(_ => Array.Empty<Math2Mode0OutputUnit>()).ToArray(); }
    internal void Run(Action<Math2Mode0OutputStage, int, Math2Mode0RawOnlyOutput>? observer, CancellationToken token)
    {
        void Observe(Math2Mode0OutputStage stage, int index)
        { observer?.Invoke(stage, index, this); owner.ValidateOutput(this); token.ThrowIfCancellationRequested(); }
        try
        {
            Observe(Math2Mode0OutputStage.Claimed, -1);
            for (int at = 0; at < records.Length; at++)
            {
                Observe(Math2Mode0OutputStage.BeforeFormatter, at); started++;
                var row = owner.Ef4.CopyFinalRecord(owner, records[at]);
                ushort[] map = policy.CopyIdentityMapPrepared(row.Units);
                uint[] units = row.Units.ToArray();
                owner.Ef4.StoreOutputUnits(owner, this, row.RecordIdentity, units);
                unitPrefix[at] = units;
                phases[at] = "units50"; Observe(Math2Mode0OutputStage.AfterUnits, at);
                MathSegmentRun[] runs = row.Runs.Select(run => new MathSegmentRun(
                    run.PrimaryIndex < 0 ? run.PrimaryIndex : map[run.PrimaryIndex], run.EndSegmentExclusive)).ToArray();
                owner.Ef4.StoreOutputRuns(owner, this, row.RecordIdentity, runs);
                runPrefix[at] = runs;
                phases[at] = "runs68"; Observe(Math2Mode0OutputStage.AfterRunMap, at);
                units = policy.NormalizeDecimalPrepared(units, token);
                owner.Ef4.StoreOutputUnits(owner, this, row.RecordIdentity, units);
                unitPrefix[at] = units;
                phases[at] = "decimal50"; Observe(Math2Mode0OutputStage.AfterDecimal, at);
                Observe(Math2Mode0OutputStage.BeforeRows, at);
                var unitRows = new Math2Mode0OutputUnit[units.Length];
                rowPrefix[at] = unitRows;
                for (int index = 0; index < units.Length; index++)
                {
                    token.ThrowIfCancellationRequested();
                    var text = row.Library.InternOutput(policy.CopyUtf8((ushort)units[index]), token);
                    phases[at] = $"intern88:{index}"; Observe(Math2Mode0OutputStage.AfterIntern, at);
                    unitRows[index] = new(units[index], text, text.ByteCount, 0, 0, uint.MaxValue);
                    rows++; phases[at] = $"rows78:{index}"; Observe(Math2Mode0OutputStage.AfterRow, at);
                }
                output[at] = new(row.RecordIdentity, row.Source, row.Library, row.After, units, map, runs, unitRows);
                phases[at] = "rows78-complete";
            }
            Observe(Math2Mode0OutputStage.BeforeCompletion, records.Length); complete = true;
        }
        catch { failed = true; owner.Ef4.Source.Loop.FailRawAttentionHistory(); throw; }
    }
    internal Math2Mode0OutputRecord[] CopyRecords() => owner.WithOutput(this, () =>
    {
        if (!complete || failed) throw new InvalidOperationException("Complete ordinary output required.");
        return output.Select(row => row is null ? throw new InvalidDataException("Missing output record.") :
            row with { Units = row.Units.ToArray(), OldToNewMap = row.OldToNewMap.ToArray(),
                Runs = row.Runs.ToArray(), UnitRows = row.UnitRows.ToArray() }).ToArray();
    });
    // Explicit detached logical failure-prefix inspection, never accepted output.
    internal Math2Mode0OutputStatistics CopyStatistics() => new(reservation, started, rows, complete, failed,
        phases.ToArray(), unitPrefix.Select(units => units.ToArray()).ToArray(), runPrefix.Select(runs => runs.ToArray()).ToArray(),
        rowPrefix.Select(units => units.Select(unit => unit is null ? (int?)null : unit.Text.Identity).ToArray()).ToArray());
}

internal sealed partial class Math2Mode0RawOnlyFinalOrdering
{
    private bool outputClaimed;
    private Math2Mode0RawOnlyOutput? output;
    internal Math2Mode0RawOnlyOutput FormatOrdinary(CancellationToken token = default)
        => FormatOrdinaryDiagnostic(DefaultPayloadBudget, null, token);
    internal Math2Mode0RawOnlyOutput FormatOrdinaryDiagnostic(long budget,
        Action<Math2Mode0OutputStage, int, Math2Mode0RawOnlyOutput>? observer, CancellationToken token = default)
        => Ef4.WithFinalOrdering(this, () =>
        {
            token.ThrowIfCancellationRequested();
            if (!complete || failed || outputClaimed) throw new InvalidOperationException("Successful unconsumed final owner required.");
            if (budget < 0 || budget > DefaultPayloadBudget) throw new ArgumentOutOfRangeException(nameof(budget));
            // Metadata/immutable AL inspection only, before payload allocation.
            var (policy, payload) = Ef4.PlanOrdinaryOutput(this, retained, reservation, token);
            if (payload > budget) throw new NotSupportedException("Combined history/EF4/order/output payload envelope exceeds budget before claim.");
            token.ThrowIfCancellationRequested();
            outputClaimed = true;
            try
            {
                output = new(this, policy, retained.ToArray(), payload);
                output.Run(observer, token); return output;
            }
            catch { Ef4.Source.Loop.FailRawAttentionHistory(); throw; }
        });
    internal void ValidateOutput(Math2Mode0RawOnlyOutput candidate) => WithOutput(candidate, () => true);
    internal T WithOutput<T>(Math2Mode0RawOnlyOutput candidate, Func<T> action)
        => Ef4.WithFinalOrdering(this, () =>
        {
            if (!outputClaimed || !ReferenceEquals(output, candidate)) throw new ArgumentException("Exact ordinary output owner required.");
            return action();
        });
}

internal sealed partial class Math2Mode0RawOnlyEf4
{
    internal (Math2Mode0RawOnlyOutputPolicy Policy, long Payload) PlanOrdinaryOutput(
        Math2Mode0RawOnlyFinalOrdering final, object[] retained, long reservation, CancellationToken token)
        => WithFinalOrdering(final, () =>
        {
            if (retained.Length == 0) throw new NotSupportedException("Empty vector has no actual AL owner; no formula is invented.");
            var gate = FindFinalCell(retained[0]).Record.Source.ReadAttentionAlphabet();
            var policy = Math2Mode0RawOnlyOutputPolicy.Bind(this, gate);
            long payload = reservation, peak = 0;
            foreach (object record in retained)
            {
                token.ThrowIfCancellationRequested();
                var row = FindFinalCell(record); row.Record.Validate();
                if (row.Record.Library.OutputStringCount != 0)
                    throw new InvalidOperationException("The actual fresh unconsumed record Library is required.");
                if (!ReferenceEquals(gate, row.Record.Source.ReadAttentionAlphabet()))
                    throw new ArgumentException("Every retained record must use the SAME source AL.");
                if (row.Units.Length > 1000 || row.Runs.Any(run => run.PrimaryIndex < -1 || run.PrimaryIndex >= row.Units.Length))
                    throw new InvalidDataException("Actual formatter map/run coordinates required.");
                var labels = new HashSet<ushort>(); long strings = 0;
                foreach (uint unit in row.Units)
                {
                    token.ThrowIfCancellationRequested(); policy.ValidateUnit(unit);
                    ushort label = (ushort)unit;
                    // Pool byte interval plus UTF16 hexadecimal dictionary key
                    // payload. CLR object headers/dictionary nodes are excluded.
                    if (labels.Add(label)) strings = checked(strings + 5L * policy.ByteCount(label) + 1);
                    if (label == policy.Dot && labels.Add(policy.Period)) strings = checked(strings + 5L * policy.ByteCount(policy.Period) + 1);
                    peak = System.Math.Max(peak, 5L * policy.ByteCount(label));
                }
                // Existing EF4/order envelope plus output/map, row/alternative,
                // detached per-record workspace and temporary UTF8 conversion.
                payload = checked(payload + strings + row.Units.LongLength * (4 + 2 + 40 + 16 + 8) + row.Runs.LongLength * 16 +
                    row.Record.PrimaryCount * 2L + row.Assignments.LongLength * 4 + row.Initial.LongLength + row.Deleted.LongLength);
            }
            return (policy, checked(payload + peak));
        });
    internal void StoreOutputUnits(Math2Mode0RawOnlyFinalOrdering final, Math2Mode0RawOnlyOutput output, object record, uint[] units)
        => final.WithOutput(output, () => { FindFinalCell(record).Record.Validate(); FindFinalCell(record).Units = units; return true; });
    internal void StoreOutputRuns(Math2Mode0RawOnlyFinalOrdering final, Math2Mode0RawOnlyOutput output, object record, MathSegmentRun[] runs)
        => final.WithOutput(output, () => { FindFinalCell(record).Record.Validate(); FindFinalCell(record).Runs = runs; return true; });
}

internal sealed partial class Math2Mode0EmptyLibrary
{
    private LegacyNativeUtf8StringPool? outputPool;
    internal LegacyNativeInternedUtf8 InternOutput(ReadOnlySpan<byte> bytes, CancellationToken token)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        _ = PoolIdentity; token.ThrowIfCancellationRequested();
        return (outputPool ??= new()).Intern(bytes, token);
    }
    internal int OutputStringCount => outputPool?.Count ?? 0;
    private void ReleaseOutputPool() { outputPool?.Dispose(); outputPool = null; }
}
