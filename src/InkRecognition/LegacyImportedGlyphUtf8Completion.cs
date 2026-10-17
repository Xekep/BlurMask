using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyImportedGlyphCompletion.Probe")]

namespace InkRecognition;

/// <summary>One imported glyph after the bounded default validUnicode0491A0
/// helper. The original pre-canonical bytes/identity are retained separately.</summary>
public sealed class LegacyImportedGlyphUtf8Completion
{
    private readonly byte[] bytes;
    public LegacyImportedControlGlyphBytes Raw {get;}
    public bool Changed {get;}
    private LegacyImportedGlyphUtf8Completion(LegacyImportedControlGlyphBytes raw,byte[] bytes,bool changed)
    {Raw=raw;this.bytes=bytes;Changed=changed;}
    public byte[] CopyBytes()=>bytes.ToArray();

    public static LegacyImportedGlyphUtf8Completion Complete(LegacyFreshMainGlobalControlImport owner,ulong reference,
        LegacyNativeUnicodeNormalizationData normalization,LegacyNativeCircledPayloadData payloads,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);owner.ValidateCompletedProjection(token);
        var raw=LegacyImportedControlGlyphBytes.ReadCore(owner.ImportedPool,reference,token);
        var result=CompleteCore(raw,normalization,payloads,token);owner.ValidateCompletedProjection(token);return result;
    }
    internal static LegacyImportedGlyphUtf8Completion CompleteCore(LegacyImportedControlGlyphBytes raw,
        LegacyNativeUnicodeNormalizationData normalization,LegacyNativeCircledPayloadData payloads,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(raw);ArgumentNullException.ThrowIfNull(normalization);ArgumentNullException.ThrowIfNull(payloads);
        token.ThrowIfCancellationRequested();
        if(!raw.IsValidUtf8)throw new NotSupportedException("Native malformed/C2 codec completion is not restored; raw bytes remain available.");
        //13C920 supplies one glyph's expanded tokens to13A0B0. Its0491A0
        //call follows that entire token loop, before the storage-only NUL.
        var completed=LegacyNativeCircledUtf8Writeback.Complete(raw.CopyBytes(),normalization,payloads,token);
        byte[] copy=completed.Bytes.ToArray();
        if(copy.Length>1_048_576)throw new NotSupportedException("Portable completed glyph byte budget exceeded.");
        token.ThrowIfCancellationRequested();return new(raw,copy,completed.Changed);
    }
}
