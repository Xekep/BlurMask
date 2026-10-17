namespace InkRecognition;

/// <summary>Copied imported WORD references -> raw pre-canonical glyph/WORD
/// byte intervals. This is not completedV50, document publication or text.</summary>
public sealed class LegacyFreshMainImportedWordBytes
{
    private readonly byte[] bytes;
    public LegacyFreshMainGlobalControlImport Owner {get;}
    public int WordIndex {get;}
    public LegacyNativeWordSourceSpan SourceSpan {get;}
    public IReadOnlyList<LegacyImportedControlGlyphBytes> Glyphs {get;}
    public bool IsValidUtf8 {get;}
    public bool ContainsNativeUnitLiteral {get;}
    private LegacyFreshMainImportedWordBytes(LegacyFreshMainGlobalControlImport owner,LegacyImportedControlWord word,
        LegacyImportedControlGlyphBytes[] glyphs,byte[] bytes)
    {
        Owner=owner;WordIndex=word.WordIndex;SourceSpan=word.SourceSpan;Glyphs=Array.AsReadOnly(glyphs);this.bytes=bytes;
        IsValidUtf8=LegacyImportedControlGlyphBytes.ValidUtf8(bytes);ContainsNativeUnitLiteral=glyphs.Any(g=>g.ContainsNativeUnitLiteral);
    }
    public byte[] CopyBytes()=>bytes.ToArray();
    public static IReadOnlyList<LegacyFreshMainImportedWordBytes> Project(LegacyFreshMainGlobalControlImport owner,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);owner.ValidateCompletedProjection(token);
        var intervals=CopyWordIntervals(owner.ImportedPool,owner.Words.Select(word=>word.CopyReferences()).ToArray(),token);
        owner.ValidateCompletedProjection(token);
        var words=new LegacyFreshMainImportedWordBytes[intervals.Length];
        for(int i=0;i<words.Length;i++)words[i]=new(owner,owner.Words[i],intervals[i].Glyphs,intervals[i].Bytes);
        return Array.AsReadOnly(words);
    }
    //Owner-free interval composition; public ownership is attached only after
    //actual WORD-derived references and result leases have both been validated.
    internal static LegacyImportedWordByteInterval[] CopyWordIntervals(LegacyNativeGlyphControlImport pool,
        IReadOnlyList<ulong[]> orderedReferences,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(orderedReferences.Count>4096)throw new NotSupportedException("Portable imported WORD count exceeded.");
        int referenceCount=0;
        foreach(var references in orderedReferences)
        {
            token.ThrowIfCancellationRequested();
            if(references.Length>4096)throw new NotSupportedException("Portable imported reference count exceeded.");
            referenceCount=checked(referenceCount+references.Length);
            if(referenceCount>65536)throw new NotSupportedException("Portable aggregate imported reference budget exceeded.");
        }
        var words=new List<LegacyImportedWordByteInterval>();int work=0,totalBytes=0;
        foreach(var references in orderedReferences)
        {
            token.ThrowIfCancellationRequested();var glyphs=new List<LegacyImportedControlGlyphBytes>();using var bytes=new MemoryStream();
            foreach(ulong reference in references)
            {
                token.ThrowIfCancellationRequested();var glyph=LegacyImportedControlGlyphBytes.ReadCore(pool,reference,token);
                work=checked(work+glyph.TokenCount);if(work>1_048_576)throw new NotSupportedException("Portable imported token budget exceeded.");
                byte[] value=glyph.CopyBytes();int required=checked(totalBytes+value.Length);
                if(required>8_388_608)throw new NotSupportedException("Portable aggregate imported byte budget exceeded.");
                if(value.Length>8_388_608-bytes.Length)throw new NotSupportedException("Portable imported WORD byte budget exceeded.");
                totalBytes=required;
                bytes.Write(value);glyphs.Add(glyph);
            }
            words.Add(new(glyphs.ToArray(),bytes.ToArray()));
        }
        token.ThrowIfCancellationRequested();return words.ToArray();
    }
}

internal readonly record struct LegacyImportedWordByteInterval(LegacyImportedControlGlyphBytes[] Glyphs,byte[] Bytes);
