namespace InkRecognition;

/// <summary>Typed proof of21F320's first, zero-write return. This does not
/// execute its enabled glyph-rewrite body or decide whether text is accepted.</summary>
public sealed class LegacyFreshMainResultGlyphNormalizationNoOp
{
    public LegacyFreshMainResultGlyphNormalization Policy { get; }
    public LegacyOwnedMainFirstAlternativeSnapshot Snapshot { get; }
    public bool ExecutedGlyphRewriteBody => false;
    internal LegacyFreshMainResultGlyphNormalizationNoOp(LegacyFreshMainResultGlyphNormalization policy,
        LegacyOwnedMainFirstAlternativeSnapshot snapshot) { Policy = policy; Snapshot = snapshot; }
}

/// <summary>Fresh NEW20E/2B3 configuration only, without copied properties or
/// caller-supplied flags.00C860 clearsF4;00CAB0 binds that same container to
/// controller18, which21F320 reads before any WORD/pool dereference.</summary>
public sealed class LegacyFreshMainResultGlyphNormalization
{
    public LegacyFreshCaseMainSourcePolicy SourcePolicy { get; }
    public uint NativePropertyId => 0x012d0017;
    public byte ProvenFreshConfigurationF4 => 0;
    private LegacyFreshMainResultGlyphNormalization(LegacyFreshCaseMainSourcePolicy source) => SourcePolicy = source;

    public static LegacyFreshMainResultGlyphNormalization Prepare(LegacyFreshCaseMainSourcePolicy actualSource,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualSource); token.ThrowIfCancellationRequested();
        var controller = actualSource.Controller;
        var owner = controller.Owner;
        var dictionary = actualSource.DictionaryOwner;
        //The accepted type's private construction is the fresh no-property
        //factory. Do not widen this to a generic prepared configuration bool:
        //00EB00 and generic00EBA0 can change/copyF4 in other native lifecycles.
        if (!ReferenceEquals(owner.CaseDictionaryOwner, dictionary) ||
            !ReferenceEquals(owner.CommonModel, actualSource.Model) ||
            !ReferenceEquals(owner.SourceGraph, actualSource.Graph) ||
            !ReferenceEquals(dictionary.Graph, actualSource.Graph) ||
            actualSource.Model.SourceKind != 0x135 ||
            actualSource.AcousticFingerprint != "7A5F08809E81FB4A2D4C78B3EFD251D0986503B0D30D21DE3695DAB1F8A89978")
            throw new ArgumentException("Exact fresh owned Russian main source/controller/model required.");
        if (actualSource.HasAssociatedSource || controller.Configuration.Mode22f6 != 0 ||
            controller.Configuration.Trace2368 != 0 || controller.Configuration.SecondaryAlternativeCount2404 != 1 ||
            controller.SecondaryScaleEc != 0 || dictionary.ContainerCaseMode90 != uint.MaxValue ||
            dictionary.IgnoreCaseC4 || dictionary.CaptureSideCosts2368)
            throw new NotSupportedException("Only the existing fresh no-property CASE main source is proved.");
        token.ThrowIfCancellationRequested(); return new(actualSource);
    }

    /// <summary>Validate the completed exact-owner candidate, then represent
    /// the native F4==0 return. No glyph, scratch, WORD, score or query is read
    /// or written by the normalization body. Quote order is a separate caller
    /// stage; this proof does not imply21FBF0 or final273 assembly ran.</summary>
    public LegacyFreshMainResultGlyphNormalizationNoOp Apply(LegacyOwnedMainFirstAlternativeSnapshot actualSnapshot,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualSnapshot); token.ThrowIfCancellationRequested();
        var session = actualSnapshot.Session;
        if (!ReferenceEquals(actualSnapshot.SourcePolicy, SourcePolicy) ||
            !ReferenceEquals(actualSnapshot.Terminal, session.Ownership.Materialization.TerminalResult) ||
            !ReferenceEquals(actualSnapshot.BeamOwner20, session.Ownership.Materialization) ||
            !ReferenceEquals(session.Ownership.Geometry.Graph, SourcePolicy.Graph) ||
            actualSnapshot.SourceNodeCount2310 != SourcePolicy.Graph.NodeCount)
            throw new ArgumentException("The snapshot and normalization policy must share the exact actual source/result owner.");
        if (!session.IsComplete || session.IsStopped ||
            !ReferenceEquals(actualSnapshot.WordHead18, session.Ownership.WordPublisher.Head))
            throw new InvalidOperationException("Only the unchanged completed actual publication list is supported.");
        token.ThrowIfCancellationRequested(); return new(this, actualSnapshot);
    }
}
