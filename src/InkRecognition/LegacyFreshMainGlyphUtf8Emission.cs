using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainGlyphUtf8Emission.Probe")]

namespace InkRecognition;

/// <summary>Fresh main TOP273 virtual50 emission with its distinct control2A
/// and alternate-string ownership. Only the fresh raw, no imported/reused text
/// lineage is supported. This does not implement273 virtual30 text import.</summary>
public sealed class LegacyFreshMainGlyphUtf8Emission
{
    private readonly LegacyNativeUnicodeNormalizationData normalization;
    private readonly LegacyNativeCircledPayloadData circles;
    public LegacyFreshMainSingleLinePlainRecognitionResult RawResult { get; }
    public LegacyFreshMainPlainResultOwnership TopOwner { get; }
    public LegacyInkAlphabet PrimaryAlphabet { get; }
    /// <summary>Engine2A050A20 creates an empty, non-inserting lookup owner.
    /// This is not the acoustic alphabet's symbol count.</summary>
    public int ControlSymbolCount => 0;
    /// <summary>Source secondary170..18F copied by2271F0. Fresh raw ingestion
    /// and156950 source720 publication leave both alternate vectors empty.</summary>
    public int AlternateOffsetCount => 0;

    private LegacyFreshMainGlyphUtf8Emission(LegacyFreshMainSingleLinePlainRecognitionResult raw,
        LegacyNativeUnicodeNormalizationData normalization, LegacyNativeCircledPayloadData circles)
    {
        RawResult = raw; TopOwner = raw.Session.Ownership.TopOwner;
        PrimaryAlphabet = raw.Session.Plan.SourcePolicy.Model.AcousticAlphabet;
        this.normalization = normalization; this.circles = circles;
    }

    public static LegacyFreshMainGlyphUtf8Emission Prepare(LegacyFreshMainSingleLinePlainRecognitionResult raw,
        LegacyNativeUnicodeNormalizationData normalization, LegacyNativeCircledPayloadData circles,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(raw); ArgumentNullException.ThrowIfNull(normalization);
        ArgumentNullException.ThrowIfNull(circles); token.ThrowIfCancellationRequested();
        var session = raw.Session; var policy = session.Plan.SourcePolicy;
        if (!raw.PublicationCompleted || !session.IsComplete || session.IsStopped ||
            !ReferenceEquals(session.Ownership.Materialization.CaseSourcePolicy, policy) ||
            !ReferenceEquals(session.Ownership.Materialization.TerminalResult, raw.Terminal) ||
            !ReferenceEquals(session.Ownership.Geometry.Graph, raw.Sampled.SourceRanges.Graph) ||
            !ReferenceEquals(raw.Preprocessed.Owner, raw.Owner.Input) ||
            policy.HasAssociatedSource || policy.Controller.Configuration.Trace2368 != 0 ||
            policy.Controller.Refresh.SourceFormat22f9 != 0)
            throw new ArgumentException("Completed identical fresh raw main TOP, source and nontracing plain publication required.");
        token.ThrowIfCancellationRequested(); return new(raw, normalization, circles);
    }

    /// <summary>Lookup-only018F70 -> Engine2A virtual28/0511B0. No symbol is
    /// inserted by emission, including newline, ordinary space and NBSP.</summary>
    public ushort ReadControlLabel(string text, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(text); token.ThrowIfCancellationRequested();
        return ushort.MaxValue;
    }

    /// <summary>The reference and all40 bytes must still belong to this exact
    /// TOP pool. A detached/synthetic record cannot supply emitted labels.</summary>
    public byte[] Emit(LegacyNativeOwnedGlyphRecord glyph, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(glyph); token.ThrowIfCancellationRequested();
        byte[] actual = TopOwner.GlyphPool.CopyRecord(glyph.Reference, token);
        if (!actual.AsSpan().SequenceEqual(glyph.ReadRecord()))
            throw new ArgumentException("Current identical TOP glyph reference and complete record required.");
        return EmitPreparedDescriptor(glyph.Symbol, token);
    }

    //The friend probe exercises the numeric descriptor kernel separately from
    //genuine decoded references. It never replaces a decoded pool record.
    internal byte[] EmitPreparedDescriptor(LegacyNativeGlyphSymbolDescriptor descriptor, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        //13C920 passes a NONNULL TOP+20 alternate object even when empty.
        //13A0B0 rejects offset-count<2 before indexing it; null-table primary
        //fallback in the reusable prepared helper is not this native case.
        if ((descriptor.KindFlags & 7) == 4)
            throw new InvalidDataException("Native273 virtual50 refuses a kind4 label in the actual empty alternate owner.");
        return LegacyNativeCircledUtf8Writeback.Emit(descriptor, PrimaryAlphabet, normalization, circles,
            actualControlLabelLookup: text => ReadControlLabel(text, token), cancellationToken: token);
    }
}
