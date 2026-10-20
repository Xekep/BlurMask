namespace InkRecognition;

/// <summary>Live postV68 global273.V50 with its original pool/primary AL,
/// separate fresh empty control2A and nonnull-empty alternate owner. This is
/// per-glyph default-helper emission, not imported bytes or CHAR/TEXT output.</summary>
public sealed class LegacyFreshMainGlobalGlyphUtf8Emission
{
    private readonly LegacyNativeUnicodeNormalizationData normalization;
    private readonly LegacyNativeCircledPayloadData circles;
    public LegacyFreshMainGlobalNoCandidateSourceCompletion Completion {get;}
    public LegacyFreshMainGlobalPlainResultOwnership TopOwner=>Completion.Source.Session!.Ownership;
    public LegacyInkAlphabet PrimaryAlphabet=>Completion.Snapshot.SourcePolicy.Owner.Model.AcousticAlphabet;
    public int ControlSymbolCount=>0;
    public int AlternateOffsetCount=>0;
    private LegacyFreshMainGlobalGlyphUtf8Emission(LegacyFreshMainGlobalNoCandidateSourceCompletion completion,
        LegacyNativeUnicodeNormalizationData normalization,LegacyNativeCircledPayloadData circles)
    {Completion=completion;this.normalization=normalization;this.circles=circles;}
    public static LegacyFreshMainGlobalGlyphUtf8Emission Prepare(LegacyFreshMainGlobalNoCandidateSourceCompletion actualCompletedSources,
        LegacyNativeUnicodeNormalizationData normalization,LegacyNativeCircledPayloadData circles,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualCompletedSources);ArgumentNullException.ThrowIfNull(normalization);ArgumentNullException.ThrowIfNull(circles);
        actualCompletedSources.Validate(token);
        if(!actualCompletedSources.IsTopV68Completed||actualCompletedSources.IsFaulted)
            throw new ArgumentException("Actual successful same live global source completion required.");
        //Native13B870/050A20 ->13C920 uses distinct control2A and alternate
        //vectors. This private fresh raw path has no external/prior text orV30.
        var policy=actualCompletedSources.Snapshot.SourcePolicy;
        if(policy.HasAssociatedSource||policy.Configuration.Trace2368!=0||policy.Refresh.SourceFormat22f9!=0||policy.Owner.Model.SourceKind!=0x135)
            throw new NotSupportedException("Fresh ordinary nontracing live source required.");
        return new(actualCompletedSources,normalization,circles);
    }
    public ushort ReadControlLabel(string text,CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(text);Completion.Validate(token);return ushort.MaxValue;}
    public byte[] Emit(LegacyNativeOwnedGlyphRecord glyph,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(glyph);Completion.Validate(token);
        if(!TopOwner.GlyphPool.CopyRecord(glyph.Reference,token).AsSpan().SequenceEqual(glyph.ReadRecord()))
            throw new ArgumentException("Current exact same global pool record required.");
        if((glyph.Symbol.KindFlags&7)==4)
            throw new InvalidDataException("Native liveV50 fails on kind4 with its nonnull-empty alternate owner.");
        byte[] output=LegacyNativeCircledUtf8Writeback.Emit(glyph.Symbol,PrimaryAlphabet,normalization,circles,
            actualControlLabelLookup:text=>ReadControlLabel(text,token),cancellationToken:token);
        Completion.Validate(token);return output;
    }
}
