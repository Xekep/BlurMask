namespace InkRecognition;

/// <summary>Immutable root document for this library's raw-stroke entrypoint.
/// Source object IDs are the original caller stroke ordinals, before filtering
/// or physical reorder. This is a new owned root, not an inferred SDK page ID.</summary>
public sealed class LegacyOwnedRawInkDocument
{
    private readonly InkStroke[] strokes;
    private readonly LegacyNativeSelectedInkRange[] selections;
    private readonly LegacyNativeTextSourceInputMap map;
    private readonly LegacyNativeInkPositionRange originalRange;
    public IReadOnlyList<InkStroke> Strokes { get; }
    public int ObjectCount => strokes.Length;
    private LegacyOwnedRawInkDocument(InkStroke[] strokes, LegacyNativeSelectedInkRange[] selections,
        LegacyNativeInkPositionRange originalRange)
    { this.strokes = strokes; Strokes = Array.AsReadOnly(strokes); this.selections = selections;
      map = new(selections, [], 0); this.originalRange = originalRange; }

    internal static LegacyOwnedRawInkDocument Create(IReadOnlyList<InkStroke> raw, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(raw); token.ThrowIfCancellationRequested();
        if (raw.Count is < 1 or > 256) throw new ArgumentException("One to256 raw source objects required.");
        var copy = new InkStroke[raw.Count]; int points = 0;
        var objects = new LegacyNativeSourceInkObject[raw.Count];
        for (int i = 0; i < raw.Count; i++)
        {
            token.ThrowIfCancellationRequested(); var input = raw[i]?.Points ?? throw new ArgumentException("Null raw source object.");
            if (input.Count is < 1 or > 4096 || input.Count > 4096 - points)
                throw new ArgumentException("Bounded complete raw source objects required.");
            var values = input.ToArray();
            foreach (var point in values)
                if (!float.IsFinite(point.X) || !float.IsFinite(point.Y) || MathF.Abs(point.X) > 1_000_000 || MathF.Abs(point.Y) > 1_000_000)
                    throw new ArgumentException("Finite bounded raw document points required.");
            points += values.Length; copy[i] = new(Array.AsReadOnly(values)); objects[i] = new(values.Length);
        }
        var first = LegacyNativeInkPosition.Pack(0, 0, 3, copy[0].Points.Count);
        var last = LegacyNativeInkPosition.Pack((uint)(copy.Length - 1), copy[^1].Points.Count - 1, 5, copy[^1].Points.Count);
        var selections = LegacyNativeSelectedInkProducer.Select([new(first, last)], objects, token);
        return new(copy, selections, new(first, last));
    }

    public LegacyNativeSelectedInkRange[] CopySelectedInputs() => selections.ToArray();

    /// <summary>05AFA0 ordinary projection into this exact raw root document.
    /// Ranges retain native C21.V80 append order;024F30 source-node
    /// interning remains a later stage. Sampled ordinals are never used as raw document IDs.</summary>
    public LegacyOwnedMainGlyphDocumentPositions Project(LegacyFreshMainSingleLinePlainRecognitionResult actualResult,
        LegacyOwnedMainGlyphSourceMetadataResult actualSource, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualResult); ArgumentNullException.ThrowIfNull(actualSource);
        token.ThrowIfCancellationRequested();
        if (!ReferenceEquals(actualResult.Owner.SourceDocument, this) ||
            !ReferenceEquals(actualSource.Owner.Ownership, actualResult.Session.Ownership))
            throw new ArgumentException("Actual source metadata and raw result must belong to this same original document.");
        var references = new LegacyNativeTextSourceReference[actualSource.ReferenceCount];
        for (int i = 0; i < references.Length; i++)
        {
            token.ThrowIfCancellationRequested(); var source = actualSource.References[i];
            if (source.First.First != 0 || source.Last.First != 0)
                throw new NotSupportedException("This raw root has the actual ordinary ingress group0, not an external document group table.");
            references[i] = new(new(source.First.First, source.First.Second, source.First.Third),
                new(source.Last.First, source.Last.Second, source.Last.Third), source.FirstResidual, source.LastResidual);
        }
        var projected = LegacyNativeTextSourcePositionPublication.Project(map, references, true, [originalRange], token);
        return new(this, actualResult, actualSource, null, projected);
    }

    /// <summary>The same ordinary05AFA0 projection for a genuine completed
    /// count1 or configured candidate. Its exact selected session, source and
    /// original raw document remain typed owners; sampled IDs are not reused.</summary>
    public LegacyOwnedMainGlyphDocumentPositions ProjectCandidate(LegacyOwnedMainCandidateSource actualCandidate,
        LegacyOwnedMainGlyphSourceMetadataResult actualSource, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualCandidate); ArgumentNullException.ThrowIfNull(actualSource);
        actualCandidate.Validate(token);
        if (!ReferenceEquals(actualCandidate.SourceDocument, this) ||
            !ReferenceEquals(actualSource.Owner.Ownership, actualCandidate.Ownership))
            throw new ArgumentException("The actual selected candidate/source metadata must belong to this exact raw document.");
        var references = new LegacyNativeTextSourceReference[actualSource.ReferenceCount];
        for (int i = 0; i < references.Length; i++)
        {
            token.ThrowIfCancellationRequested(); var source = actualSource.References[i];
            if (source.First.First != 0 || source.Last.First != 0)
                throw new NotSupportedException("This owned root supports actual ordinary ingress group0 only.");
            references[i] = new(new(source.First.First, source.First.Second, source.First.Third),
                new(source.Last.First, source.Last.Second, source.Last.Third), source.FirstResidual, source.LastResidual);
        }
        var projected = LegacyNativeTextSourcePositionPublication.Project(map, references, true, [originalRange], token);
        return new(this, actualCandidate, actualSource, projected);
    }

    /// <summary>Negative-span insertion projection for the same genuine selected
    /// candidate and original document. A negative span is an insertion source,
    /// not an ordinary closed glyph interval and has no residual allocation.</summary>
    public LegacyOwnedMainGlyphDocumentPositions ProjectSpanCandidate(LegacyOwnedMainCandidateSource actualCandidate,
        LegacyOwnedMainSpanGlyphSourceResult actualSource, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualCandidate); ArgumentNullException.ThrowIfNull(actualSource);
        actualCandidate.Validate(token);
        var sourceCandidate = actualSource.Owner.CandidateSource;
        if (!ReferenceEquals(actualCandidate.SourceDocument, this) ||
            (actualCandidate.IsConfigured
                ? sourceCandidate is null || !ReferenceEquals(sourceCandidate.CandidateIdentity, actualCandidate.CandidateIdentity) ||
                  !ReferenceEquals(sourceCandidate.Ownership, actualCandidate.Ownership) ||
                  !ReferenceEquals(sourceCandidate.SourceOverlay, actualCandidate.SourceOverlay)
                : sourceCandidate is not null || !ReferenceEquals(actualSource.Owner.RawResult, actualCandidate.RawResult)))
            throw new ArgumentException("Actual selected span, candidate and original document must have the same ownership.");
        var references = new LegacyNativeTextSourceReference[actualSource.ReferenceCount];
        for (int i = 0; i < references.Length; i++)
        {
            token.ThrowIfCancellationRequested(); var source = actualSource.References[i];
            if (source.First.First != 0 || source.Last.First != 0)
                throw new NotSupportedException("This raw root owns ordinary ingress group0 only.");
            references[i] = new(new(source.First.First, source.First.Second, source.First.Third),
                new(source.Last.First, source.Last.Second, source.Last.Third));
        }
        var projected = LegacyNativeTextSourcePositionPublication.Project(map, references, false, [originalRange], token);
        return new(this, actualCandidate, actualSource, projected);
    }

    /// <summary>05AFA0 negative-span insertion positions. These mode4 positions
    /// are not ordinary closed glyph ranges and have no residual tail.</summary>
    public LegacyOwnedMainGlyphDocumentPositions Project(LegacyFreshMainSingleLinePlainRecognitionResult actualResult,
        LegacyOwnedMainSpanGlyphSourceResult actualSource, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualResult); ArgumentNullException.ThrowIfNull(actualSource);
        token.ThrowIfCancellationRequested();
        if (!ReferenceEquals(actualResult.Owner.SourceDocument, this) || !ReferenceEquals(actualSource.Owner.RawResult, actualResult))
            throw new ArgumentException("Actual span and raw result must belong to this same original document.");
        var references = new LegacyNativeTextSourceReference[actualSource.ReferenceCount];
        for (int i = 0; i < references.Length; i++)
        {
            token.ThrowIfCancellationRequested(); var source = actualSource.References[i];
            if (source.First.First != 0 || source.Last.First != 0)
                throw new NotSupportedException("This raw root owns ordinary ingress group0 only.");
            references[i] = new(new(source.First.First, source.First.Second, source.First.Third),
                new(source.Last.First, source.Last.Second, source.Last.Third));
        }
        var projected = LegacyNativeTextSourcePositionPublication.Project(map, references, false, [originalRange], token);
        return new(this, actualResult, null, actualSource, projected);
    }
}

public sealed class LegacyOwnedMainGlyphDocumentPositions
{
    private readonly LegacyFreshMainSingleLinePlainRecognitionResult? result;
    public LegacyOwnedRawInkDocument Document { get; }
    public LegacyFreshMainSingleLinePlainRecognitionResult Result => result ??
        throw new InvalidOperationException("Configured source positions have no count1 raw result.");
    public LegacyOwnedMainCandidateSource? CandidateSource { get; }
    public LegacyOwnedMainGlyphFlushSession SelectedSession => CandidateSource?.Session ?? Result.Session;
    public LegacyOwnedMainGlyphSourceMetadataResult? OrdinarySource { get; }
    public LegacyOwnedMainSpanGlyphSourceResult? SpanSource { get; }
    public IReadOnlyList<LegacyNativeInkPositionRange> Ranges { get; }
    internal LegacyOwnedMainGlyphDocumentPositions(LegacyOwnedRawInkDocument document,
        LegacyFreshMainSingleLinePlainRecognitionResult result, LegacyOwnedMainGlyphSourceMetadataResult? source,
        LegacyOwnedMainSpanGlyphSourceResult? spanSource,
        LegacyNativeInkPositionRange[] ranges)
    { Document = document; this.result = result; OrdinarySource = source; SpanSource = spanSource;
      Ranges = Array.AsReadOnly(ranges); }
    internal LegacyOwnedMainGlyphDocumentPositions(LegacyOwnedRawInkDocument document,
        LegacyOwnedMainCandidateSource candidate, LegacyOwnedMainGlyphSourceMetadataResult source,
        LegacyNativeInkPositionRange[] ranges)
    {
        Document = document; CandidateSource = candidate; OrdinarySource = source;
        result = candidate.IsConfigured ? null : candidate.RawResult;
        Ranges = Array.AsReadOnly(ranges);
    }
    internal LegacyOwnedMainGlyphDocumentPositions(LegacyOwnedRawInkDocument document,
        LegacyOwnedMainCandidateSource candidate, LegacyOwnedMainSpanGlyphSourceResult source,
        LegacyNativeInkPositionRange[] ranges)
    {
        Document = document; CandidateSource = candidate; SpanSource = source;
        result = candidate.IsConfigured ? null : candidate.RawResult;
        Ranges = Array.AsReadOnly(ranges);
    }
}
