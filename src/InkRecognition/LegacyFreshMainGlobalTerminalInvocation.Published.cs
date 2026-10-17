namespace InkRecognition;

public sealed partial class LegacyFreshMainGlobalTerminalInvocation
{
    /// <summary>Select2257C0 from the actual post207CB0 rows. Absence of a
    /// winner is an explicit result, not a fabricated selected compact index.</summary>
    public LegacyFreshMainGlobalPublishedTerminalResult SelectPublished(LegacyFreshMainGlobalFinalPublication publication,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(publication); token.ThrowIfCancellationRequested();
        if (!ReferenceEquals(publication.Policy, Policy))
            throw new ArgumentException("The completed global publication must retain this exact controller owner.");
        publication.Validate(token);
        var winner = LegacyNativeMainTerminalSelection.SelectSingleAlternativeWithoutRescue(publication.FinalRecords,
            publication.FinalSourceEnd, sources, Options, publication.FinalCumulativeCost, Policy.Owner.Contexts, token);
        int? index = null;
        byte[] selected = [];
        if (winner.HasWinner)
        {
            int candidate = checked(publication.FinalNodeStart + (int)winner.RowField22);
            if ((uint)candidate >= (uint)publication.RecordCount2470)
                throw new InvalidDataException("Actual selected postpublication row22 must name its owned compact pool.");
            index = candidate;
            selected = publication.FinalRecords.Slice(checked(winner.Ordinal * 88), 88).ToArray();
        }
        publication.Validate(token);
        return new(this, publication, winner, index, selected);
    }
}

/// <summary>Actual postpublication numeric selection or no-winner. This is not
/// a273 alternative, WORD list, decoded text or calibrated acceptance.</summary>
public sealed class LegacyFreshMainGlobalPublishedTerminalResult
{
    private readonly byte[] selected;
    public LegacyFreshMainGlobalTerminalInvocation Invocation { get; }
    public LegacyFreshMainGlobalFinalPublication Publication { get; }
    public LegacyPreparedMainTerminalWinner Winner { get; }
    public int? SelectedRecordIndex { get; }
    internal LegacyFreshMainGlobalPublishedTerminalResult(LegacyFreshMainGlobalTerminalInvocation invocation,
        LegacyFreshMainGlobalFinalPublication publication, LegacyPreparedMainTerminalWinner winner,
        int? index, byte[] selected)
    { Invocation = invocation; Publication = publication; Winner = winner; SelectedRecordIndex = index; this.selected = selected; }
    public byte[] CopySelectedRecord() => selected.ToArray();
}
