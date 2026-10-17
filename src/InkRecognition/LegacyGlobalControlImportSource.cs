using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("LegacyPublishedControlImport.Probe")]
namespace InkRecognition;

//Actual old/new first snapshot ownership, never a reconstructed old terminal.
//All conversion/projection consumers retain the same ControlImport instance;
//only this immutable private source view dispatches its source validation.
internal sealed class LegacyGlobalControlImportSource
{
    private readonly LegacyFreshMainGlobalFirstAlternativeSnapshot? old;
    private readonly LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot? published;
    internal LegacyFreshMainGlobalGlyphExecutionSession Session => old?.Session ?? published!.Session;
    internal LegacyFreshMainGlobalWordControllerPolicy Policy => old?.SourcePolicy ?? published!.SourcePolicy;
    internal LegacyNativePublishedWordNode? Head => old is not null ? old.WordHead18 : published!.WordHead18;
    internal int PublishedWordCount => old?.PublishedWordCount ?? published!.PublishedWordCount;
    internal int ActiveAlternativeCount => old?.ActiveAlternativeCount60 ?? published!.ActiveAlternativeCount60;
    private LegacyGlobalControlImportSource(LegacyFreshMainGlobalFirstAlternativeSnapshot value) => old = value;
    private LegacyGlobalControlImportSource(LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot value) => published = value;
    internal static LegacyGlobalControlImportSource FromOld(LegacyFreshMainGlobalFirstAlternativeSnapshot value)
    { ArgumentNullException.ThrowIfNull(value); return new(value); }
    internal static LegacyGlobalControlImportSource FromPublished(LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot value)
    { ArgumentNullException.ThrowIfNull(value); return new(value); }
    internal LegacyFreshMainGlobalFirstAlternativeSnapshot RequireOld() => old ??
        throw new InvalidOperationException("Published control import does not own an old first-alternative snapshot.");
    internal LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot RequirePublished() => published ??
        throw new InvalidOperationException("Old control import does not own a published first-alternative snapshot.");

    internal void ValidateSelection(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var ownership = Session.Ownership;
        LegacyGlobalSelectedCompactOwner actual;
        if (old is not null)
        {
            old.Terminal.PassResult.ValidateResultQueryLease(token);
            if (!ReferenceEquals(old.Terminal, ownership.Materialization.TerminalResult) ||
                !ReferenceEquals(old.BeamOwner20, ownership.Materialization))
                throw new ArgumentException("Actual old snapshot terminal/materialization identity required.");
            actual = LegacyGlobalSelectedCompactOwner.FromOld(old.BeamOwner20);
        }
        else
        {
            published!.Terminal.Publication.Validate(token);
            if (!ReferenceEquals(published.Terminal, ownership.PublishedTerminal) ||
                !ReferenceEquals(published.BeamOwner20, published.Terminal.Publication))
                throw new ArgumentException("Actual published snapshot terminal/publication identity required.");
            actual = LegacyGlobalSelectedCompactOwner.FromPublished(published.Terminal, token);
        }
        if (!actual.IsSameSelection(ownership.CompactOwner) ||
            !ReferenceEquals(actual.Policy, Policy) ||
            !ReferenceEquals(ownership.Geometry.Policy, Policy) ||
            !ReferenceEquals(ownership.Geometry.Graph, Policy.Owner.Graph))
            throw new ArgumentException("Actual selected source/controller/pool lineage required.");
        actual.Validate(token);
    }
}
