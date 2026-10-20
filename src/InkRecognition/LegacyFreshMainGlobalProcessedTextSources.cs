namespace InkRecognition;

internal sealed class LegacyGlobalSourceSelectedGlyph
{
    internal LegacyFreshMainGlobalTextSourceWrapper Owner{get;}
    internal int Index{get;}
    internal ulong Reference{get;}
    internal LegacyPreparedGlyphAlternativeSelection Selection{get;}
    internal LegacyNativeGlyphRecordOwner Glyphs{get;}
    internal LegacyGlobalSourceSelectedGlyph(LegacyFreshMainGlobalTextSourceWrapper owner,int index,ulong reference,
        LegacyPreparedGlyphAlternativeSelection selection,LegacyNativeGlyphRecordOwner glyphs)
    {Owner=owner;Index=index;Reference=reference;Selection=selection;Glyphs=glyphs;}
}

/// <summary>Actual same-scope024F30 source processing receipt. Status is
/// produced by the shared range kernel, never supplied as a public flag.</summary>
public sealed class LegacyFreshMainGlobalProcessedGlyphSource
{
    private readonly LegacyNativeInkPositionRange[] ranges;
    public LegacyFreshMainGlobalTextSourceWrapper Owner{get;}
    public LegacyFreshMainGlobalGlyphDocumentPositions OriginalPositions{get;}
    public int WordIndex{get;}
    public ulong Reference{get;}
    public bool IsOrdinarySource{get;}
    public LegacyOwnedRawInkDocument Document=>Owner.Scope.Document;
    internal LegacyGlobalSourceSelectedGlyph Selected{get;}
    internal LegacyFreshMainGlobalProcessedGlyphSource(LegacyFreshMainGlobalTextSourceWrapper owner,
        LegacyGlobalSourceSelectedGlyph selected,LegacyFreshMainGlobalGlyphDocumentPositions positions,
        bool ordinary,LegacyNativeInkPositionRange[] ranges)
    {Owner=owner;Selected=selected;OriginalPositions=positions;WordIndex=selected.Index;Reference=selected.Reference;IsOrdinarySource=ordinary;this.ranges=ranges.ToArray();}
    public LegacyNativeInkPositionRange[] CopyRanges(CancellationToken token=default)
    {Owner.Scope.Validate();token.ThrowIfCancellationRequested();return ranges.ToArray();}
}

public sealed class LegacyFreshMainGlobalTextSourceChoice
{
    public LegacyFreshMainGlobalProcessedGlyphSource ProcessedSource{get;}
    public LegacyFreshMainGlobalTextSourcePayload Payload{get;}
    public LegacyPreparedGlyphAlternativeSelection Selection{get;}
    public int Marker{get;}
    public bool Reused{get;}
    internal LegacyFreshMainGlobalTextSourceChoice(LegacyFreshMainGlobalProcessedGlyphSource source,
        LegacyFreshMainGlobalTextSourcePayload payload,LegacyPreparedGlyphAlternativeSelection selection,int marker,bool reused)
    {ProcessedSource=source;Payload=payload;Selection=selection;Marker=marker;Reused=reused;}
    public byte[] CopySelectedUtf8(CancellationToken token=default)
    {
        ProcessedSource.Owner.Validate(token);var records=Payload.CopyRecords(token);
        if((uint)Marker>=(uint)records.Count||records[Marker].Text is not{} text)
            throw new InvalidOperationException("Actual global source marker has no available text.");
        return text.ReadBytes();
    }
}

public sealed class LegacyFreshMainGlobalTextSourceWord
{
    public LegacyFreshMainGlobalTextSourceWrapper Owner{get;}
    public IReadOnlyList<LegacyFreshMainGlobalTextSourceChoice> Glyphs{get;}
    public uint SourceFlags{get;}
    public uint SelectedFlags{get;}
    private readonly LegacyNativeInkPositionRange[] ranges;
    internal LegacyFreshMainGlobalTextSourceWord(LegacyFreshMainGlobalTextSourceWrapper owner,
        LegacyFreshMainGlobalTextSourceChoice[] glyphs,LegacyNativeInkPositionRange[] ranges,uint sourceFlags,uint selectedFlags)
    {Owner=owner;Glyphs=Array.AsReadOnly(glyphs);this.ranges=ranges;SourceFlags=sourceFlags;SelectedFlags=selectedFlags;}
    public LegacyNativeInkPositionRange[] CopyOrdinaryRanges(CancellationToken token=default)
    {Owner.Validate(token);return ranges.ToArray();}
}
