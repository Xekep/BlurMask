using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("LegacyMainGlobalPublishedCleanup.Probe")]
namespace InkRecognition;

/// <summary>Typed proof of the fresh21F320 F4==0 return for the actual global
/// published first alternative. No enabled glyph-rewrite body is executed.</summary>
public sealed class LegacyFreshMainGlobalPublishedGlyphNormalizationNoOp
{
    public LegacyFreshMainGlobalResultGlyphNormalization Policy { get; }
    public LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot Snapshot { get; }
    public bool ExecutedGlyphRewriteBody => false;
    internal LegacyFreshMainGlobalPublishedGlyphNormalizationNoOp(LegacyFreshMainGlobalResultGlyphNormalization policy,
        LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot snapshot) { Policy = policy; Snapshot = snapshot; }
}

/// <summary>Fresh NEW20E/2B3 global controller only, without copied properties
/// or caller-supplied F4.00C860 clearsF4 and00CAB0 binds that same configuration
/// to controller18. Source218 descriptor processing does not own this field.</summary>
public sealed class LegacyFreshMainGlobalResultGlyphNormalization
{
    public LegacyFreshMainGlobalWordControllerPolicy SourcePolicy { get; }
    public uint NativePropertyId => 0x012d0017;
    public byte ProvenFreshConfigurationF4 => 0;
    private LegacyFreshMainGlobalResultGlyphNormalization(LegacyFreshMainGlobalWordControllerPolicy policy) => SourcePolicy = policy;

    public static LegacyFreshMainGlobalResultGlyphNormalization Prepare(LegacyFreshMainGlobalWordControllerPolicy actualPolicy,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualPolicy); token.ThrowIfCancellationRequested();
        var owner = actualPolicy.Owner;
        //This private-constructor policy prepares NEW20E/fresh00C860 limits
        //and exactly one fresh292 session; neither factory accepts an external
        //configuration/property-copy input. Model/source bytes are pinned by it.
        if (owner.Model.SourceKind != 0x135 || owner.Symbols.SelectedSources.Count != 1 ||
            owner.Symbols.ResourcePrefix.Identifier != "ru_RU-lk-text.lite.lex" ||
            owner.Symbols.ResourcePrefix.Field68 != 1 ||
            owner.Graph.Source.Source.CategoryOwner.SelectedSourceOffset != 0 ||
            !owner.Graph.Source.Source.SourceFingerprint.Equals(
                "7A5F08809E81FB4A2D4C78B3EFD251D0986503B0D30D21DE3695DAB1F8A89978",
                StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Exact fresh global Russian source/controller/model required.");
        if (actualPolicy.HasAssociatedSource || actualPolicy.Configuration.Mode22f6 != 0 ||
            actualPolicy.Configuration.Trace2368 != 0 || actualPolicy.Configuration.SecondaryAlternativeCount2404 != 1 ||
            actualPolicy.SecondaryScaleEc != 0)
            throw new NotSupportedException("Only the checked fresh global no-property ordinary main source is proved.");
        token.ThrowIfCancellationRequested(); return new(actualPolicy);
    }

    /// <summary>Represent21F320's first return without reading or writing any
    /// WORD/glyph, scratch or score storage. This proof does not imply the prior
    /// quote operation ran; an owning caller preserves that stage separately.</summary>
    public LegacyFreshMainGlobalPublishedGlyphNormalizationNoOp ApplyPublished(
        LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot actualSnapshot, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualSnapshot); token.ThrowIfCancellationRequested();
        var terminal = actualSnapshot.Terminal;
        var session = actualSnapshot.Session;
        var ownership = session.Ownership;
        terminal.Publication.Validate(token);
        if (!ReferenceEquals(actualSnapshot.SourcePolicy, SourcePolicy) ||
            !ReferenceEquals(terminal, ownership.PublishedTerminal) ||
            !ReferenceEquals(actualSnapshot.BeamOwner20, terminal.Publication) ||
            !ReferenceEquals(ownership.Geometry.Policy, SourcePolicy) ||
            !ReferenceEquals(ownership.Geometry.Graph, SourcePolicy.Owner.Graph) ||
            actualSnapshot.SourceNodeCount2310 != SourcePolicy.Owner.Graph.State.NodeCount)
            throw new ArgumentException("The published snapshot and no-op policy must share the exact actual source/result owner.");
        if (!session.IsComplete || session.IsStopped ||
            !ReferenceEquals(actualSnapshot.WordHead18, ownership.WordPublisher.Head))
            throw new InvalidOperationException("Only the unchanged completed actual publication list is supported.");
        terminal.Publication.Validate(token);
        return new(this, actualSnapshot);
    }
}
