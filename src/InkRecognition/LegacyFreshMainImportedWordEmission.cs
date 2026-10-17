namespace InkRecognition;

/// <summary>Ordered imported WORD bytes formed AFTER independent default
/// glyph-helper calls. This is not wholeWORD normalization or accepted text.</summary>
public sealed class LegacyFreshMainImportedWordEmission
{
    private readonly byte[] bytes;
    public LegacyFreshMainImportedWordBytes Raw {get;}
    public LegacyFreshMainGlobalControlImport Owner=>Raw.Owner;
    public LegacyNativeUnicodeNormalizationData Normalization {get;}
    public LegacyNativeCircledPayloadData Payloads {get;}
    public IReadOnlyList<LegacyImportedGlyphUtf8Completion> Glyphs {get;}
    public bool AnyGlyphChanged {get;}
    private LegacyFreshMainImportedWordEmission(LegacyFreshMainImportedWordBytes raw,LegacyNativeUnicodeNormalizationData normalization,
        LegacyNativeCircledPayloadData payloads,LegacyCompletedImportedWordInterval interval)
    {
        Raw=raw;Normalization=normalization;Payloads=payloads;Glyphs=Array.AsReadOnly(interval.Glyphs);bytes=interval.Bytes;
        AnyGlyphChanged=interval.Glyphs.Any(g=>g.Changed);
    }
    public byte[] CopyBytes()=>bytes.ToArray();
    public static IReadOnlyList<LegacyFreshMainImportedWordEmission> Complete(LegacyFreshMainGlobalControlImport owner,
        LegacyNativeUnicodeNormalizationData normalization,LegacyNativeCircledPayloadData payloads,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);ArgumentNullException.ThrowIfNull(normalization);ArgumentNullException.ThrowIfNull(payloads);
        owner.ValidateCompletedProjection(token);
        var raw=LegacyFreshMainImportedWordBytes.Project(owner,token);
        var intervals=CompleteIntervals(raw.Select(w=>w.Glyphs).ToArray(),normalization,payloads,token);
        owner.ValidateCompletedProjection(token);
        var result=new LegacyFreshMainImportedWordEmission[raw.Count];
        for(int i=0;i<result.Length;i++)result[i]=new(raw[i],normalization,payloads,intervals[i]);
        return Array.AsReadOnly(result);
    }
    internal static LegacyCompletedImportedWordInterval[] CompleteIntervals(
        IReadOnlyList<IReadOnlyList<LegacyImportedControlGlyphBytes>> words,
        LegacyNativeUnicodeNormalizationData normalization,LegacyNativeCircledPayloadData payloads,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(normalization);ArgumentNullException.ThrowIfNull(payloads);token.ThrowIfCancellationRequested();
        if(words.Count>4096)throw new NotSupportedException("Portable completed WORD count exceeded.");
        int count=0;
        foreach(var word in words)
        {
            token.ThrowIfCancellationRequested();
            if(word.Count>4096)throw new NotSupportedException("Portable completed WORD reference count exceeded.");
            count=checked(count+word.Count);
            if(count>65536)throw new NotSupportedException("Portable aggregate completed reference budget exceeded.");
        }
        var result=new List<LegacyCompletedImportedWordInterval>();int total=0;
        foreach(var word in words)
        {
            var glyphs=new List<LegacyImportedGlyphUtf8Completion>();using var output=new MemoryStream();
            foreach(var rawGlyph in word)
            {
                token.ThrowIfCancellationRequested();var glyph=LegacyImportedGlyphUtf8Completion.CompleteCore(rawGlyph,normalization,payloads,token);
                byte[] value=glyph.CopyBytes();int required=checked(total+value.Length);
                if(required>8_388_608)throw new NotSupportedException("Portable aggregate completed byte budget exceeded.");
                total=required;output.Write(value);glyphs.Add(glyph);
            }
            result.Add(new(glyphs.ToArray(),output.ToArray()));
        }
        token.ThrowIfCancellationRequested();return result.ToArray();
    }
}

internal readonly record struct LegacyCompletedImportedWordInterval(LegacyImportedGlyphUtf8Completion[] Glyphs,byte[] Bytes);
