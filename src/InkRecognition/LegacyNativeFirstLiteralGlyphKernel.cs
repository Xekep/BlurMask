using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>Literal13AF30 count1 and first1F42 flag writes. Source-bound
/// adapters validate the plan before calling this model-independent kernel.</summary>
internal static class LegacyNativeFirstLiteralGlyphKernel
{
    internal static void Apply(LegacyNativeGlyphRecordPool pool,ulong actualReference,
        LegacyFirstLiteralGlyphSymbolPlan plan,CancellationToken cancellationToken)
    {
        _=LegacyNativeGlyphSymbolUpdate.Apply(pool,actualReference,plan.PublishedLabel,1,0,false,cancellationToken);
        byte[] glyph=pool.CopyRecord(actualReference,cancellationToken);
        ushort kind=BinaryPrimitives.ReadUInt16LittleEndian(glyph.AsSpan(4));
        BinaryPrimitives.WriteUInt16LittleEndian(glyph.AsSpan(4),(ushort)((kind&0xfff7)|plan.CategoryMismatchFlag4));
        ushort flags=BinaryPrimitives.ReadUInt16LittleEndian(glyph.AsSpan(6));
        BinaryPrimitives.WriteUInt16LittleEndian(glyph.AsSpan(6),(ushort)(flags|plan.PublicationFlags6));
        pool.ReplaceRecord(actualReference,glyph,cancellationToken);
    }
}
