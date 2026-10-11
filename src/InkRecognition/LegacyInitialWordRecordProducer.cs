using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>
/// Explicit data arguments of the checked1f8d60 prefix writer. Numeric suffixes
/// identify native offsets where the full field semantics remain unresolved.
/// Opaque64-bit values are copied as data, never dereferenced as native pointers.
/// </summary>
public sealed record LegacyInitialWordRecordFields(
    ulong SourceTag, ulong Auxiliary08, int CandidateCost10, int IncrementalCost14,
    ushort Label24, ushort SourceNode18, ushort PackedSourceArcIndex,
    ushort SourceArcIndex1c, byte CompositeCategory, ushort PreviousArcIndex1e,
    sbyte SourceSpan, byte Count26, byte HeaderFlag5, byte HeaderFlag6,
    byte RequestedSecondaryFlag, byte PackedKind27, byte SmallState21,
    byte StateBit6, byte StateBit7, int Auxiliary30, int Auxiliary34,
    int AnchorEndingNode38, ushort AnchorArcIndex3c);

/// <summary>
/// Native1f75b0 zeroed-slot ->1f8d60 initialized0x58 word prefix. This stage
/// does not allocate/select heap buckets or synthesize previous-record/winner
/// policy. The0x80 heap tail, trace clone and linguistic history remain separate.
/// </summary>
public static class LegacyInitialWordRecordProducer
{
    public static byte[] InitializeAllocatedPrefix(LegacyInitialWordRecordFields fields,
        float secondaryScale, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fields); cancellationToken.ThrowIfCancellationRequested();
        if (!float.IsFinite(secondaryScale))
            throw new ArgumentException("Finite explicit secondary-heap scale required.", nameof(secondaryScale));
        byte secondary = secondaryScale == 0 ? (byte)0 : fields.RequestedSecondaryFlag;
        //1f75b0 zeros0..77 before this writer, including untouched54..57.
        var prefix = new byte[0x58];
        BinaryPrimitives.WriteUInt64LittleEndian(prefix, fields.SourceTag);
        byte extraHeader = (fields.SourceTag & 0x20000000000UL) != 0 && fields.HeaderFlag6 == 0
            ? (byte)2 : (byte)0;
        prefix[5] = unchecked((byte)(((fields.SourceTag >> 40) & 0x7d) |
            (uint)fields.HeaderFlag5 << 7 | extraHeader));
        prefix[6] = (byte)(((fields.SourceTag >> 48) & 0xfc) | (uint)(fields.HeaderFlag6 & 1));
        BinaryPrimitives.WriteUInt64LittleEndian(prefix.AsSpan(8), fields.Auxiliary08);
        BinaryPrimitives.WriteInt32LittleEndian(prefix.AsSpan(0x10), fields.CandidateCost10);
        BinaryPrimitives.WriteInt32LittleEndian(prefix.AsSpan(0x14), fields.IncrementalCost14);
        uint packed = fields.SourceNode18 | (uint)(fields.CompositeCategory & 63) << 16 |
            (uint)(fields.PackedSourceArcIndex & 1023) << 22;
        BinaryPrimitives.WriteUInt32LittleEndian(prefix.AsSpan(0x18), packed);
        BinaryPrimitives.WriteUInt16LittleEndian(prefix.AsSpan(0x1c), fields.SourceArcIndex1c);
        BinaryPrimitives.WriteUInt16LittleEndian(prefix.AsSpan(0x1e), fields.PreviousArcIndex1e);
        prefix[0x20] = unchecked((byte)fields.SourceSpan);
        prefix[0x21] = (byte)((fields.SmallState21 & 63) | (fields.StateBit6 & 1) << 6 |
            (fields.StateBit7 & 1) << 7);
        BinaryPrimitives.WriteUInt16LittleEndian(prefix.AsSpan(0x22), ushort.MaxValue);
        BinaryPrimitives.WriteUInt16LittleEndian(prefix.AsSpan(0x24), fields.Label24);
        prefix[0x26] = fields.Count26;
        prefix[0x27] = (byte)((fields.PackedKind27 & 7) << 1 | (secondary & 1));
        BinaryPrimitives.WriteUInt64LittleEndian(prefix.AsSpan(0x28), ulong.MaxValue);
        BinaryPrimitives.WriteInt32LittleEndian(prefix.AsSpan(0x30), fields.Auxiliary30);
        BinaryPrimitives.WriteInt32LittleEndian(prefix.AsSpan(0x34), fields.Auxiliary34);
        BinaryPrimitives.WriteInt32LittleEndian(prefix.AsSpan(0x38), fields.AnchorEndingNode38);
        BinaryPrimitives.WriteUInt16LittleEndian(prefix.AsSpan(0x3c), fields.AnchorArcIndex3c);
        BinaryPrimitives.WriteUInt16LittleEndian(prefix.AsSpan(0x3e), ushort.MaxValue);
        return prefix;
    }

    /// <summary>
    /// Checked ordinary first startup alternative of1fd240: category comes
    /// from actual sampler StartCompositeMap(data+50), and1f8d60 receives the
    /// returned source arc, owner node and low byte of supplied source span.
    /// Remaining fields are explicit initializer inputs. Missing map emits no
    /// record. Alternate end/special-symbol/controller branches are not guessed.
    /// </summary>
    public static bool TryCreateOrdinaryStartupPrefix(LegacyInitialWordRecordFields explicitFields,
        LegacyCursiveCategoryMapper categories, int selectedSymbolEntry, ushort endingNode,
        ushort returnedSourceArc, uint sourceSpan, float secondaryScale, out byte[] prefix,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(explicitFields); ArgumentNullException.ThrowIfNull(categories);
        cancellationToken.ThrowIfCancellationRequested();
        int category = categories.GetSymbolCompositeCategory(selectedSymbolEntry);
        if (category < 0) { prefix = []; return false; }
        if (category > 63)
            throw new NotSupportedException("Checked ordinary startup requires a six-bit composite category.");
        var resolved = explicitFields with
        {
            Label24 = checked((ushort)selectedSymbolEntry), SourceNode18 = endingNode,
            PackedSourceArcIndex = returnedSourceArc, SourceArcIndex1c = returnedSourceArc,
            CompositeCategory = (byte)category, PreviousArcIndex1e = ushort.MaxValue,
            SourceSpan = unchecked((sbyte)(byte)sourceSpan),
            AnchorEndingNode38 = endingNode, AnchorArcIndex3c = ushort.MaxValue
        };
        prefix = InitializeAllocatedPrefix(resolved, secondaryScale, cancellationToken);
        return true;
    }
}
