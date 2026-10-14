using System.Text;

namespace InkRecognition;

public sealed record LegacyNativeCircledUtf8Result(ReadOnlyMemory<byte> Bytes, bool Changed);

/// <summary>Complete bounded default-validUTF8 Engine0491A0 canonical scratch/marker classifier/writeback orchestration.</summary>
public static class LegacyNativeCircledUtf8Writeback
{
    public static LegacyNativeCircledUtf8Result Complete(ReadOnlySpan<byte> originalUtf8,
        LegacyNativeUnicodeNormalizationData normalization, LegacyNativeCircledPayloadData payloads,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(normalization);ArgumentNullException.ThrowIfNull(payloads);cancellationToken.ThrowIfCancellationRequested();
        byte[] scratch=LegacyNativeCanonicalScratch.PrepareUtf8Scratch(originalUtf8,normalization,cancellationToken);
        int cursor=0, replacements=0, copied=0;using var output=new MemoryStream();Span<byte>encoded=stackalloc byte[4];
        while(cursor<=scratch.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int startRelative=scratch.AsSpan(cursor).IndexOf("\U000f0000"u8);
            if(startRelative<0)break;
            int start=cursor+startRelative,begin=start+4;
            int endRelative=scratch.AsSpan(begin).IndexOf("\U000f0001"u8);
            if(endRelative<0)break;
            int end=begin+endRelative,after=end+4;
            if(payloads.TryReadCircledCodePoint(scratch.AsSpan(begin,end-begin),out int cp))
            {
                if(++replacements>65536)throw new ArgumentOutOfRangeException(nameof(originalUtf8),"Circle work budget exceeded.");
                int count=new Rune(cp).EncodeToUtf8(encoded);
                output.Write(scratch.AsSpan(copied,start-copied));output.Write(encoded[..count]);copied=after;
                //094960 overwrites the end iterator with the new insertion end; the next search uses this updated offset.
                //Replacements cannot introduce markers. Streaming the untouched suffix is byte-equivalent to native memmove/resume.
                cursor=after;
            }
            else cursor=after;
        }
        cancellationToken.ThrowIfCancellationRequested();
        //0491A0 clears/copies the original destination only if at least one circled write succeeded.
        if(replacements==0)return new(originalUtf8.ToArray(),false);
        output.Write(scratch.AsSpan(copied));return new(output.ToArray(),true);
    }

    /// <summary>Actual274 expansion/emission followed by the complete default UTF8 helper; control/alternate owners stay distinct.</summary>
    public static byte[] Emit(LegacyNativeGlyphSymbolDescriptor glyph,LegacyInkAlphabet actualPrimaryAlphabet,
        LegacyNativeUnicodeNormalizationData normalization,LegacyNativeCircledPayloadData payloads,
        LegacyNativeGlyphAlternateUtf8Table? actualAlternateOwner=null,Func<string,ushort>? actualControlLabelLookup=null,
        CancellationToken cancellationToken=default)
    {
        var tokens=LegacyNativeGlyphUtf8Tokens.Expand(glyph,actualControlLabelLookup,cancellationToken);
        byte[]raw=LegacyNativeGlyphUtf8Tokens.EmitBeforeCanonicalComposition(tokens,actualPrimaryAlphabet,actualAlternateOwner,cancellationToken);
        return Complete(raw,normalization,payloads,cancellationToken).Bytes.ToArray();
    }
}
