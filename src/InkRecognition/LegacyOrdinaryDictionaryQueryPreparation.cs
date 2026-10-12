using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Explicit prepared scalar inputs read by ordinary 1fd240/201270.
/// Frontend280 is obtained from the live owner, rather than inferred from flags.</summary>
public readonly record struct LegacyOrdinaryDictionaryQueryPolicy(byte Mode2601, byte Limit2602,
    bool Boundary254c, bool SuppressPrevious2609, int PreviousDistanceLimit54);

/// <summary>Ordinary initial/continuation query field producers. A rejected gate
/// leaves the reused query unchanged. Context container, capacity and dictionary
/// cache identity retain their caller-owned prepared values. Alternative 1fe740
/// and auxiliary 204290 queries are distinct producers, not aliases of this one.</summary>
public static class LegacyOrdinaryDictionaryQueryPreparation
{
    public static ulong? ApplyInitial(LegacyPreparedDictionaryQuery query,
        LegacyOrdinaryDictionaryQueryPolicy policy, uint ownerNode, uint span, ushort inputFlags,
        bool frontend280, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query); cancellationToken.ThrowIfCancellationRequested();
        bool limited = frontend280 ? (inputFlags & 3) != 0 : policy.Mode2601 != 0;
        if (limited && unchecked((int)span) > policy.Limit2602) return null;
        const ulong tag = 8UL << 40;
        Set(query, tag, limited ? policy.Limit2602 : (byte)0, ownerNode, span,
            inputFlags, policy.Boundary254c);
        return tag;
    }

    /// <param name="storedParent">Complete ordinary 88-byte parent row. Fields
    /// 00/21/26/38 are consumed literally; opaque tag bytes are transported.</param>
    public static ulong? ApplyContinuation(LegacyPreparedDictionaryQuery query,
        LegacyOrdinaryDictionaryQueryPolicy policy, ReadOnlySpan<byte> storedParent,
        uint ownerNode, uint span, ushort inputFlags, bool frontend280,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query); cancellationToken.ThrowIfCancellationRequested();
        if (storedParent.Length != 0x58) throw new ArgumentException("A complete ordinary stored parent row is required.");
        bool limited = frontend280 ? (inputFlags & 3) != 0 : policy.Mode2601 != 0;
        byte count = storedParent[0x26];
        if (limited && unchecked((int)((uint)count + span)) > policy.Limit2602 ||
            (storedParent[0x21] & 0x80) != 0 ||
            !policy.SuppressPrevious2609 && unchecked((int)(ownerNode - BinaryPrimitives.ReadUInt32LittleEndian(storedParent[0x38..]))) > policy.PreviousDistanceLimit54)
            return null;
        ulong tag = BinaryPrimitives.ReadUInt64LittleEndian(storedParent);
        byte remaining = limited && count < policy.Limit2602 ? (byte)(policy.Limit2602 - count) : (byte)0;
        Set(query, tag, remaining, ownerNode, span, inputFlags, policy.Boundary254c);
        return tag;
    }

    private static void Set(LegacyPreparedDictionaryQuery query, ulong tag, byte remaining,
        uint ownerNode, uint span, ushort inputFlags, bool boundary)
    {
        query.Node = unchecked((int)(uint)tag);
        query.ParentContext = 0;
        query.Category18 = 0;
        query.Parameter19 = remaining;
        query.Parameter1c = ownerNode;
        query.Parameter20 = span;
        query.Flags24 = boundary && (inputFlags & 0x10) != 0 ? (ushort)4 : (ushort)0;
        query.Flags26 = inputFlags;
        query.Skip = 0;
    }
}
