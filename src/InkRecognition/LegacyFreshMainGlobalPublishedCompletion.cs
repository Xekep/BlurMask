namespace InkRecognition;

/// <summary>One owned global result through first-alternative scores, quote
/// cleanup, the proven fresh-F4 return, dynamic control import and per-glyph
/// default UTF8 completion. This does not accept or replace document text.</summary>
public sealed class LegacyFreshMainGlobalPublishedCompletion
{
    private int started;
    private readonly LegacyNativeUnicodeNormalizationData normalization;
    private readonly LegacyNativeCircledPayloadData payloads;
    public LegacyFreshMainGlobalPlainRecognitionResult Source { get; }
    public LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot? Snapshot { get; private set; }
    public bool QuoteCleanupCompleted { get; private set; }
    public LegacyFreshMainGlobalPublishedGlyphNormalizationNoOp? GlyphNormalization { get; private set; }
    public LegacyFreshMainGlobalControlImport? Import { get; private set; }
    public bool IsComplete { get; private set; }
    public IReadOnlyList<LegacyFreshMainImportedWordEmission> Words { get; private set; } =
        Array.Empty<LegacyFreshMainImportedWordEmission>();

    private LegacyFreshMainGlobalPublishedCompletion(LegacyFreshMainGlobalPlainRecognitionResult source,
        LegacyNativeUnicodeNormalizationData normalization, LegacyNativeCircledPayloadData payloads)
    { Source = source; this.normalization = normalization; this.payloads = payloads; }

    /// <summary>Only a positive, fully completed raw global result is eligible.
    /// No-winner remains represented by the raw result, without a fake empty
    /// completion. The input's actual session and selected owner are retained.</summary>
    public static LegacyFreshMainGlobalPublishedCompletion Prepare(
        LegacyFreshMainGlobalPlainRecognitionResult source,
        LegacyNativeUnicodeNormalizationData normalization,
        LegacyNativeCircledPayloadData payloads, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(normalization);
        ArgumentNullException.ThrowIfNull(payloads);
        token.ThrowIfCancellationRequested();
        ValidateSource(source, token);
        return new(source, normalization, payloads);
    }

    /// <summary>Preflight does not consume the invocation. Once capture starts,
    /// failure or cancellation cannot restart this owner. Completed prefixes
    /// remain available; Words is populated only after every glyph completes.
    /// The shared quote-session claim also prevents replay through a new owner.</summary>
    public IReadOnlyList<LegacyFreshMainImportedWordEmission> Run(
        ReadOnlySpan<byte> alternativeAllocation400,
        Span<byte> leadingScratch4, Span<byte> trailingScratch4,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (alternativeAllocation400.Length != 400 || leadingScratch4.Length != 4 ||
            trailingScratch4.Length != 4 || leadingScratch4.Overlaps(trailingScratch4))
            throw new ArgumentException("Complete alternative400 and two distinct caller scratch4 slots required.");
        ValidateSource(Source, token);
        if (Interlocked.CompareExchange(ref started, 1, 0) != 0)
            throw new InvalidOperationException("This global completion already started, including failed prefixes.");
        var session = Source.Session!;
        int evaluations = Source.CachedMainGroupEvaluations;
        Snapshot = LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(
            Source.Selection, session, Source.Publications, alternativeAllocation400, token);
        LegacyFreshMainGlobalPublishedQuoteNormalization.ApplyPublished(Snapshot,
            leadingScratch4, trailingScratch4, token);
        QuoteCleanupCompleted = true;
        GlyphNormalization = LegacyFreshMainGlobalResultGlyphNormalization.Prepare(
            Snapshot.SourcePolicy, token).ApplyPublished(Snapshot, token);
        Import = LegacyFreshMainGlobalControlImport.PreparePublished(Snapshot, token);
        Import.Run(token);
        var words = LegacyFreshMainImportedWordEmission.Complete(Import, normalization, payloads, token);
        ValidateSource(Source, token);
        if (Source.Selection.Publication.Policy.Owner.AcousticEvaluations != evaluations)
            throw new InvalidOperationException("Result completion cannot reevaluate main NN groups.");
        token.ThrowIfCancellationRequested();
        Words = words;
        IsComplete = true;
        return words;
    }

    private static void ValidateSource(LegacyFreshMainGlobalPlainRecognitionResult source, CancellationToken token)
    {
        source.Selection.Publication.Validate(token);
        if (!source.HasWinner || !source.PublicationCompleted || source.Session is not { } session ||
            session.IsStopped || !ReferenceEquals(session.Ownership.PublishedTerminal, source.Selection) ||
            source.Publications.Count != session.CompletedFlushCount ||
            source.Selection.Publication.Policy.Owner.AcousticEvaluations != source.CachedMainGroupEvaluations)
            throw new InvalidOperationException("The same positive global owner and complete original callback sequence required.");
    }
}
