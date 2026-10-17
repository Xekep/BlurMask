namespace InkRecognition;

/// <summary>One completed actual279 list and its genuine raw source. The
/// configured variant retains its real selected2410 session; it is never
/// converted to the historical count1 RawResult/candidate type.</summary>
public sealed class LegacyOwnedMainCandidateSource
{
    private readonly LegacyFreshMainCompletedFirstCandidate? first;
    private readonly LegacyFreshMainConfiguredSingleLinePlainRecognitionResult? configured;
    private readonly LegacyOwnedMainConfiguredAlternative? alternative;
    private readonly LegacyOwnedMainWordPublicationResult[] publications;
    public bool IsConfigured => configured is not null;
    public LegacyFreshMainCompletedFirstCandidate FirstCandidate => first ??
        throw new InvalidOperationException("Configured ownership has no historical first-candidate object.");
    public LegacyFreshMainSingleLinePlainRecognitionResult RawResult => first?.RawResult ??
        throw new InvalidOperationException("Configured ownership has no count1 raw result.");
    public LegacyFreshMainConfiguredSingleLinePlainRecognitionResult ConfiguredResult => configured ??
        throw new InvalidOperationException("The historical first candidate has no configured raw result.");
    public LegacyOwnedMainConfiguredAlternative ConfiguredAlternative => alternative ??
        throw new InvalidOperationException("The historical first candidate has no selected2410 alternative.");
    public LegacyFreshMainSingleLinePlainRecognition InputOwner => first?.RawResult.Owner ?? configured!.InputOwner;
    public LegacyOwnedRawInkDocument SourceDocument => InputOwner.SourceDocument;
    public LegacyFreshMainOwnedTextPreprocessedInput Preprocessed => first?.RawResult.Preprocessed ?? configured!.Preprocessed;
    public LegacyFreshMainSingleLineSampled Sampled => first?.RawResult.Sampled ?? configured!.Sampled;
    public LegacyOwnedMainGlyphFlushSession Session { get; }
    public LegacyFreshMainPlainResultOwnership Ownership => Session.Ownership;
    public LegacyFreshMainPlainResultOwnership TopOwner => Ownership.TopOwner;
    public LegacyFreshCaseMainSourcePolicy SourcePolicy => Session.Plan.SourcePolicy;
    public LegacyOwnedRussianMainGlyphConfidence ConfidenceOwner => Ownership.Geometry.Owner;
    public LegacyOwnedRussianMainResidualSource SourceOverlay => Session.SourceOverlay;
    public LegacyNativePublishedWordNode? WordHead18 { get; }
    /// <summary>Stable source-invocation cache key shared by configured
    /// alternatives, distinct from each selected279 candidate identity.</summary>
    public object SourceIdentity => (object?)first?.RawResult ?? configured!;
    public object CandidateIdentity => (object?)first ?? alternative!;

    private LegacyOwnedMainCandidateSource(LegacyFreshMainCompletedFirstCandidate? first,
        LegacyFreshMainConfiguredSingleLinePlainRecognitionResult? configured,
        LegacyOwnedMainConfiguredAlternative? alternative, LegacyOwnedMainGlyphFlushSession session,
        LegacyOwnedMainWordPublicationResult[] publications)
    {
        this.first = first; this.configured = configured; this.alternative = alternative;
        Session = session; this.publications = publications; WordHead18 = session.Ownership.WordPublisher.Head;
    }

    public static LegacyOwnedMainCandidateSource FromFirst(LegacyFreshMainCompletedFirstCandidate actualCandidate,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualCandidate); token.ThrowIfCancellationRequested();
        if (!ReferenceEquals(actualCandidate.GlyphNormalization.Snapshot, actualCandidate.Snapshot) ||
            !ReferenceEquals(actualCandidate.GlyphNormalization.Policy.SourcePolicy, actualCandidate.Snapshot.SourcePolicy) ||
            !ReferenceEquals(actualCandidate.Snapshot.Session, actualCandidate.RawResult.Session) ||
            !ReferenceEquals(actualCandidate.Snapshot.WordHead18, actualCandidate.RawResult.Session.Ownership.WordPublisher.Head))
            throw new ArgumentException("The exact completed first-candidate snapshot and current native head required.");
        var result = new LegacyOwnedMainCandidateSource(actualCandidate, null, null,
            actualCandidate.RawResult.Session, actualCandidate.RawResult.Publications.ToArray());
        result.Validate(token); return result;
    }

    public static LegacyOwnedMainCandidateSource FromConfigured(
        LegacyFreshMainConfiguredSingleLinePlainRecognitionResult actualResult,
        LegacyOwnedMainConfiguredAlternative actualAlternative, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualResult); ArgumentNullException.ThrowIfNull(actualAlternative);
        token.ThrowIfCancellationRequested();
        if (!actualResult.Alternatives.IsComplete || actualResult.Alternatives.IsFaulted ||
            !actualResult.Alternatives.CopyActiveAlternatives().Any(a => ReferenceEquals(a, actualAlternative)) ||
            !ReferenceEquals(actualResult.Compact, actualAlternative.Terminal.Materialization) ||
            !ReferenceEquals(actualResult.Pass, actualAlternative.Terminal.Materialization.PassResult) ||
            !ReferenceEquals(actualResult.Alternatives.TopOwner, actualAlternative.Ownership.TopOwner))
            throw new ArgumentException("A retained genuine alternative of this identical configured raw pass/TOP required.");
        var result = new LegacyOwnedMainCandidateSource(null, actualResult, actualAlternative,
            actualAlternative.Session, actualAlternative.CopyPublications());
        result.Validate(token); return result;
    }

    /// <summary>Check source/candidate lineage and an unchanged actual WORD
    /// list. This does not refresh dictionary query state or rewrite glyphs.</summary>
    public void Validate(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (!Session.IsComplete || Session.IsStopped || alternative?.IsReleased == true ||
            !ReferenceEquals(WordHead18, Ownership.WordPublisher.Head) ||
            !ReferenceEquals(Sampled.SourceRanges.Graph, SourcePolicy.Graph) ||
            !ReferenceEquals(ConfidenceOwner.Graph, SourcePolicy.Graph) ||
            !ReferenceEquals(ConfidenceOwner.CaseDictionaryOwner, SourcePolicy.DictionaryOwner) ||
            !ReferenceEquals(Ownership.Controller.Owner.CaseDictionaryOwner, SourcePolicy.DictionaryOwner) ||
            !ReferenceEquals(SourcePolicy.Controller, Ownership.Controller) ||
            !ReferenceEquals(Preprocessed.Owner, InputOwner.Input) ||
            SourcePolicy.HasAssociatedSource || SourcePolicy.Controller.Configuration.Mode22f6 != 0)
            throw new InvalidOperationException("Completed unchanged same-source/TOP/candidate ownership required.");
        if (configured is not null && (!configured.Alternatives.IsComplete || configured.Alternatives.IsFaulted ||
            !ReferenceEquals(configured.Alternatives.TopOwner, TopOwner)))
            throw new InvalidOperationException("The genuine configured assembly must remain complete and healthy.");
        var head = WordHead18;
        for (int ordinal = publications.Length - 1; ordinal >= 0; ordinal--)
        {
            token.ThrowIfCancellationRequested(); var publication = publications[ordinal];
            if (!publication.Succeeded || !ReferenceEquals(publication.Association.Owner, Session) ||
                publication.Association.Ordinal != ordinal)
                throw new InvalidDataException("Exact successful callback tokens in native order required.");
            if (publication.PublishedWord is not { } word) continue;
            if (!ReferenceEquals(head, word) || !ReferenceEquals(word.Word.Pool, Ownership.GlyphPool))
                throw new InvalidDataException("Actual279 list contains a foreign, missing or reordered WORD.");
            head = head.Next;
        }
        if (head is not null || publications.Length != Session.CompletedFlushCount)
            throw new InvalidDataException("The complete actual callback prefix must own this entire279 list.");
        token.ThrowIfCancellationRequested();
    }

    public LegacyOwnedMainWordPublicationResult[] CopyPublications() => publications.ToArray();

    public LegacyOwnedMainWordUtf8Projection[] CopyRawWordsInNativeHeadOrder(CancellationToken token = default)
    {
        Validate(token);
        var result = new List<LegacyOwnedMainWordUtf8Projection>();
        for (int ordinal = publications.Length - 1; ordinal >= 0; ordinal--)
            if (publications[ordinal].PublishedWord is not null)
                result.Add(LegacyOwnedMainWordUtf8Projection.Project(publications[ordinal], token));
        token.ThrowIfCancellationRequested(); return result.ToArray();
    }
}
