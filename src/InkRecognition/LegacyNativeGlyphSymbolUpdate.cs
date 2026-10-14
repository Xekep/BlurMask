using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>274.virtual30 /13af30 symbol production. A count-zero record is
/// retained in the word chain with kind1/label1 and contributes zero span.
/// Counts are native signed integers; the encoded macro branch rejects only
/// count&gt;=8192. This does not infer any classifier label or component count.</summary>
public static class LegacyNativeGlyphSymbolUpdate
{
    public static bool Apply(LegacyNativeGlyphRecordPool pool, ulong reference,
        ushort symbol, int componentCount, byte componentFlag, bool externalVariant,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pool);
        cancellationToken.ThrowIfCancellationRequested();
        byte[] record = pool.CopyRecord(reference, cancellationToken);
        ushort kind;
        if (externalVariant) kind = 4;
        else if (componentCount == 0) { kind = 1; symbol = 1; }
        else if (symbol is >= 0xfff9 and <= 0xfffc)
        {
            if (componentCount >= 8192) return false;
            ushort packed = unchecked((ushort)((componentFlag << 13) | (componentCount & 0x1fff)));
            symbol = unchecked((ushort)((packed << 2) | ((symbol - 1) & 3)));
            kind = 1;
        }
        else kind = 0;
        ushort flags = BinaryPrimitives.ReadUInt16LittleEndian(record.AsSpan(4));
        BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(4), (ushort)((flags & 0xfff8) | kind));
        BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(8), symbol);
        pool.ReplaceRecord(reference, record, cancellationToken);
        return true;
    }
}
