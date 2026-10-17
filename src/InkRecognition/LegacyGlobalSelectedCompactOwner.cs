using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("LegacyMainGlobalPublishedGlyphs.Probe")]
namespace InkRecognition;

//Private ownership sum: actual old materialization OR actual published
//selection. No old terminal, pass, source, pool or classifier is manufactured.
internal sealed class LegacyGlobalSelectedCompactOwner
{
    private readonly LegacyFreshMainGlobalCompactWordMaterialization? old;
    private readonly LegacyFreshMainGlobalPublishedTerminalResult? published;
    internal object Identity => (object?)old ?? published!;
    internal LegacyFreshMainGlobalWordPassResult Pass => old?.TerminalResult.PassResult ?? published!.Publication.PassResult;
    internal LegacyFreshMainGlobalWordControllerPolicy Policy => Pass.Policy;
    internal LegacyFreshMainGlobalGlyphConfidence Confidence => old?.ConfidenceOwner ?? published!.Publication.ConfidenceOwner;
    internal LegacyPreparedMainTerminalWinner Winner => old?.TerminalResult.Winner ?? published!.Winner;
    internal int RecordCount => old?.RecordCount ?? published!.Publication.RecordCount2470;
    internal int SelectedRecordIndex => old?.SelectedRecordIndex ?? published!.SelectedRecordIndex!.Value;
    private LegacyGlobalSelectedCompactOwner(LegacyFreshMainGlobalCompactWordMaterialization value) { old = value; }
    private LegacyGlobalSelectedCompactOwner(LegacyFreshMainGlobalPublishedTerminalResult value) { published = value; }
    internal static LegacyGlobalSelectedCompactOwner FromOld(LegacyFreshMainGlobalCompactWordMaterialization value)
    { ArgumentNullException.ThrowIfNull(value); return new(value); }
    internal static LegacyGlobalSelectedCompactOwner FromPublished(LegacyFreshMainGlobalPublishedTerminalResult value,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(value); token.ThrowIfCancellationRequested();
        value.Publication.Validate(token);
        if (!value.Winner.HasWinner || value.SelectedRecordIndex is not int index)
            throw new ArgumentException("Actual positive published global selection required; noWinner cannot create glyphs/WORD.");
        if (!ReferenceEquals(value.Invocation.Policy, value.Publication.Policy) ||
            (uint)index >= (uint)value.Publication.RecordCount2470)
            throw new ArgumentException("Actual selected compact index/publication/controller ownership required.");
        return new(value);
    }
    internal LegacyFreshMainGlobalCompactWordMaterialization RequireOld() => old ??
        throw new InvalidOperationException("Published global selection does not own an old selected-terminal materialization.");
    internal LegacyFreshMainGlobalPublishedTerminalResult RequirePublished() => published ??
        throw new InvalidOperationException("Old global materialization does not own a published-terminal result.");
    internal byte[] CopyCompactStorage() => old?.CopyCompactStorage() ?? published!.Publication.CopyCompactStorage();
    internal int[] CopyNodeStarts2478() => old?.CopyNodeStarts2478() ?? published!.Publication.CopyNodeStarts2478();
    internal void Validate(CancellationToken token) => Pass.ValidateResultQueryLease(token);
    internal bool IsSameSelection(LegacyGlobalSelectedCompactOwner other) => ReferenceEquals(Identity, other.Identity);
}
