namespace InkRecognition;

/// <summary>Actual selected WORD reference and its refreshed, filtered,
/// classified UTF8 alternatives. Document source endpoints are separate.</summary>
public sealed class LegacyOwnedMainTextGlyphChoices
{
    public ulong SelectedReference { get; }
    public LegacyOwnedMainRefreshedGlyphAlternatives? OrdinaryRefreshed { get; }
    public LegacyOwnedMainRefreshedSpanGlyphOwner? SpanRefreshed { get; }
    public LegacyPreparedGlyphAlternativeSelection Selection { get; }
    public LegacyNativeTextGlyphCallerResult Published { get; }
    public LegacyOwnedMainGlyphDocumentPositions SourcePositions { get; }
    internal LegacyOwnedMainTextGlyphChoices(ulong reference, LegacyOwnedMainRefreshedGlyphAlternatives? refreshed,
        LegacyOwnedMainRefreshedSpanGlyphOwner? span, LegacyPreparedGlyphAlternativeSelection selection,
        LegacyNativeTextGlyphCallerResult published, LegacyOwnedMainGlyphDocumentPositions positions)
    { SelectedReference = reference; OrdinaryRefreshed = refreshed; SpanRefreshed = span; Selection = selection;
      Published = published; SourcePositions = positions; }
    /// <summary>No fallback to a different alternative when the native selected
    /// marker was removed or is unavailable.</summary>
    public byte[]? CopySelectedUtf8() => (uint)Published.AdjustedMarkerIndex < (uint)Published.Records.Count
        ? Published.Records[Published.AdjustedMarkerIndex].Text.ReadBytes() : null;
}

public sealed class LegacyOwnedMainTextWordChoices
{
    public LegacyNativePublishedWordNode Word { get; }
    public IReadOnlyList<LegacyOwnedMainTextGlyphChoices> Glyphs { get; }
    internal LegacyOwnedMainTextWordChoices(LegacyNativePublishedWordNode word, LegacyOwnedMainTextGlyphChoices[] glyphs)
    { Word = word; Glyphs = Array.AsReadOnly(glyphs); }
}

/// <summary>Actual first-candidate source refresh ->273.V50 ->213A0 ->21000 join.
/// This owns the fresh string pool and proves wrapperHeaderEFilter=false from
/// the private fresh no-properties lifecycle. It does not implement024F30's
/// source-node geometry/dedup,021760 reuse,0283B0 postprocessing or acceptance.</summary>
public sealed class LegacyFreshMainTextGlyphChoices : IDisposable
{
    private readonly LegacyNativeUtf8StringPool strings = new();
    private readonly LegacyNativeTextPublicationData publication;
    private readonly LegacyNativeUnicodeNormalizationData normalization;
    private readonly byte[] header;
    private int started;
    private bool disposed;
    public LegacyFreshMainCompletedFirstCandidate Candidate { get; }
    public LegacyOwnedMainOrdinaryGlyphRefresh Refresh { get; }
    public LegacyOwnedMainSpanGlyphRefresh SpanRefresh { get; }
    public LegacyFreshMainGlyphUtf8Emission Emitter { get; }

    private LegacyFreshMainTextGlyphChoices(ReadOnlySpan<byte> resource, LegacyFreshMainCompletedFirstCandidate candidate,
        LegacyCursiveSeparationTable table, LegacyNativeTextPublicationData publication,
        LegacyNativeUnicodeNormalizationData normalization, LegacyNativeCircledPayloadData circles, CancellationToken token)
    {
        Candidate = candidate; this.publication = publication; this.normalization = normalization;
        var source = candidate.Snapshot.SourcePolicy; var raw = candidate.RawResult;
        if (!ReferenceEquals(candidate.GlyphNormalization.Snapshot, candidate.Snapshot) ||
            !ReferenceEquals(candidate.GlyphNormalization.Policy.SourcePolicy, source) ||
            !ReferenceEquals(candidate.Snapshot.Session, raw.Session) ||
            !ReferenceEquals(candidate.Snapshot.WordHead18, raw.Session.Ownership.WordPublisher.Head) ||
            source.HasAssociatedSource || source.Controller.Configuration.Mode22f6 != 0)
            throw new ArgumentException("Completed actual fresh first candidate and unchanged source/head ownership required.");
        Refresh = LegacyOwnedMainOrdinaryGlyphRefresh.Prepare(resource, raw, table, token);
        SpanRefresh = LegacyOwnedMainSpanGlyphRefresh.Prepare(raw, token);
        Emitter = LegacyFreshMainGlyphUtf8Emission.Prepare(raw, normalization, circles, token);
        header = resource.Slice(99, 16).ToArray();
    }

    public static LegacyFreshMainTextGlyphChoices Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainCompletedFirstCandidate candidate, LegacyCursiveSeparationTable table,
        LegacyNativeTextPublicationData publication, LegacyNativeUnicodeNormalizationData normalization,
        LegacyNativeCircledPayloadData circles,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(candidate); ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(publication); ArgumentNullException.ThrowIfNull(normalization);
        ArgumentNullException.ThrowIfNull(circles);
        token.ThrowIfCancellationRequested(); return new(resource, candidate, table, publication, normalization, circles, token);
    }

    public IReadOnlyList<LegacyOwnedMainTextWordChoices> Publish(ReadOnlySpan<byte> allocatedRecordStorage40,
        ReadOnlySpan<byte> selectionAllocation4, CancellationToken token = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this); token.ThrowIfCancellationRequested();
        if (allocatedRecordStorage40.Length != 40 || selectionAllocation4.Length % 4 != 0 || selectionAllocation4.Length > 65536 * 4)
            throw new ArgumentException("Explicit complete row and bounded whole selection allocation required.");
        if (Interlocked.CompareExchange(ref started, 1, 0) != 0)
            throw new InvalidOperationException("This text-choice publication has already started, including failed prefixes.");
        var raw = Candidate.RawResult; var pool = raw.Session.Ownership.GlyphPool;
        var words = new List<LegacyOwnedMainTextWordChoices>();
        int referenceCount = 0, recordCount = 0, byteCount = 0;
        foreach (var projected in Candidate.RawWordsInNativeHeadOrder)
        {
            token.ThrowIfCancellationRequested(); var word = projected.Publication.PublishedWord!;
            var refs = word.Word.FlatReferences;
            if (refs.Count > 4096 - referenceCount) throw new NotSupportedException("Total text-choice source-reference budget exceeded.");
            referenceCount += refs.Count;
            var positions = new Dictionary<ulong, LegacyOwnedMainGlyphDocumentPositions>();
            //024F30 first visits every WORD reference to refresh source owners;
            //only its subsequent loop asks for ordered glyph alternatives.
            foreach (ulong reference in refs)
            {
                token.ThrowIfCancellationRequested(); _ = pool.CopyRecord(reference, token);
                ulong key = LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
                if (unchecked((short)(key >> 24)) >= 0)
                {
                    var refreshed = Refresh.RefreshOwner(key, allocatedRecordStorage40, token);
                    if (!positions.ContainsKey(key))
                        positions.Add(key, raw.Owner.SourceDocument.Project(raw, refreshed.Source, token));
                }
                else
                {
                    var refreshed = SpanRefresh.RefreshOwner(key, token);
                    if (!positions.ContainsKey(key))
                        positions.Add(key, raw.Owner.SourceDocument.Project(raw, refreshed.Source, token));
                }
            }
            var glyphs = new List<LegacyOwnedMainTextGlyphChoices>();
            foreach (ulong reference in refs)
            {
                token.ThrowIfCancellationRequested();
                ulong key = LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
                LegacyOwnedMainRefreshedGlyphAlternatives? ordinary = null;
                LegacyOwnedMainRefreshedSpanGlyphOwner? span = null;
                LegacyPreparedGlyphAlternativeSelection selection;
                if (unchecked((short)(key >> 24)) >= 0)
                { ordinary = Refresh.Select(reference, allocatedRecordStorage40, selectionAllocation4, token: token); selection = ordinary.Selection; }
                else
                {
                    span = SpanRefresh.RefreshOwner(key, token);
                    uint marker = unchecked((uint)(int)(short)(reference >> 40));
                    selection = LegacyPreparedGlyphAlternativeSelection.Select(pool.CopyOwnerNode(key, token), marker, selectionAllocation4,
                        cancellationToken: token);
                }
                if (!selection.Succeeded)
                    throw new NotSupportedException("Actual refreshed glyph selection allocation refused; no accepted text fabricated.");
                //Only this canonical owner is addressed by213A0. Copying the
                //whole pool for every glyph would make publication quadratic.
                var owner = new LegacyNativeGlyphRecordOwner([
                    pool.CopyOwnerNode(key, token)]);
                //00C860 clears99, no external/mutable property packet enters
                //the typed fresh source.0283B0 queries12D0065 for that same99.
                var published = LegacyNativeTextGlyphCaller.Produce(owner, Candidate.Snapshot.SourcePolicy.Model.AcousticAlphabet,
                    header, selection.CopySelectionRows4(), key, false,
                    unchecked((int)selection.SelectedMarker!.Value), strings, publication, normalization,
                    actualControlLabelLookup: text => Emitter.ReadControlLabel(text, token),
                    actualFullGlyphEmitter: glyph => Emitter.Emit(glyph, token),
                    cancellationToken: token);
                if (published.Records.Count > 65536 - recordCount)
                    throw new NotSupportedException("Total text-choice publication record budget exceeded.");
                recordCount += published.Records.Count;
                foreach (var record in published.Records)
                {
                    if (record.ByteCount > 8_388_608 - byteCount) throw new NotSupportedException("Total text-choice UTF8 budget exceeded.");
                    byteCount += (int)record.ByteCount;
                }
                glyphs.Add(new(reference, ordinary, span, selection, published, positions[key]));
            }
            words.Add(new(word, glyphs.ToArray()));
        }
        token.ThrowIfCancellationRequested(); return words.AsReadOnly();
    }

    public void Dispose() { if (disposed) return; disposed = true; strings.Dispose(); }
}
