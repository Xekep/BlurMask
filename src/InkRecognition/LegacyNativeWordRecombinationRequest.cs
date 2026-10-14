using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Portable owned reference cell for native request offset28. The
/// explicit identity is not a native pointer and is never dereferenced.</summary>
public sealed class LegacyNativeWordTransitionCostCell
{
    public ulong Identity { get; }
    public int Value { get; set; }
    public LegacyNativeWordTransitionCostCell(ulong identity, int value)
    {
        if (identity == 0) throw new ArgumentOutOfRangeException(nameof(identity));
        Identity = identity;
        Value = value;
    }
}

/// <summary>Native request spans through byte49, requiring at least74 bytes.
/// This owned aligned80-byte form preserves unknown caller residue; offset28
/// contains only the portable cost-cell identity, not a vendor ABI address.</summary>
public sealed class LegacyNativeWordRecombinationRequest
{
    private readonly byte[] bytes;
    public LegacyNativeWordTransitionCostCell Cost { get; }
    internal LegacyNativeWordRecombinationRequest(byte[] bytes, LegacyNativeWordTransitionCostCell cost)
    { this.bytes = bytes; Cost = cost; }
    public ReadOnlyMemory<byte> GetSnapshot() => bytes.ToArray();
    public byte ReadByte(int offset) => bytes[offset];
    public ushort ReadUInt16(int offset) => BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset, 2));
    public uint ReadUInt32(int offset) => BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset, 4));
    public int ReadInt32(int offset) => BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(offset, 4));
    public void WriteByte(int offset, byte value) => bytes[offset] = value;
    internal Span<byte> Storage => bytes;
}

public readonly record struct LegacyNativeWordRecombinationSymbols(ushort Symbol24e4,
    ushort Symbol24e6, ushort Symbol24e8, ushort Symbol24f4);

/// <summary>Literal80-byte20e6a0 request producer from owning rows, prepared
/// headers and the actual sampler-derived composite profile. Source callback
/// implementation and candidate eligibility remain separate.</summary>
public static class LegacyNativeWordRecombinationRequestFactory
{
    public static LegacyNativeWordRecombinationRequest Create(ReadOnlySpan<byte> heapSlot, uint currentNode,
        ReadOnlySpan<byte> originalParentRecord, LegacyCursiveCategoryProfile categoryProfile,
        LegacyNativeWordParentRowLookup owningRowLookup, Func<byte, uint> preparedSourceHeader,
        LegacySymbolContextFactory contexts, LegacyNativeWordRecombinationSymbols symbols,
        byte permission2309, LegacyNativeWordTransitionCostCell costCell, ReadOnlySpan<byte> stackResidue80,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(categoryProfile);
        ArgumentNullException.ThrowIfNull(owningRowLookup);
        ArgumentNullException.ThrowIfNull(preparedSourceHeader);
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(costCell);
        cancellationToken.ThrowIfCancellationRequested();
        if (heapSlot.Length != 128 || originalParentRecord.Length != 88 || stackResidue80.Length != 80)
            throw new ArgumentException("Actual full128 heap slot, stored88 parent and aligned80-byte residue required.");
        uint previous = unchecked(currentNode - (uint)(int)(sbyte)heapSlot[32] - 1);
        if (previous > int.MaxValue) throw new NotSupportedException("Actual nonnegative previous node required.");
        var bytes = stackResidue80.ToArray();
        var request = new LegacyNativeWordRecombinationRequest(bytes, costCell);
        Write32(bytes, 0, currentNode);
        Write32(bytes, 8, previous);
        Write16(bytes, 12, U16(heapSlot, 28));
        bytes[18] = Geometry(categoryProfile, heapSlot[26] & 63);
        Write32(bytes, 20, unchecked((uint)First(categoryProfile, heapSlot[26] & 63)));
        bytes[28] = (byte)((heapSlot[39] >> 1) & 7);
        Write32(bytes, 32, unchecked((uint)(int)(sbyte)heapSlot[32]));
        if (U16(heapSlot, 24) == currentNode)
        {
            Write16(bytes, 36, U16(heapSlot, 36));
            bytes[72] = (byte)(heapSlot[7] & 1);
        }
        else
        {
            ulong suffix = U64(heapSlot, 8);
            if (suffix == 0) throw new ArgumentException("Actual current suffix symbol node required.");
            ushort symbol = contexts.ReadNode(suffix).Symbol;
            bytes[72] = 0;
            if (symbol == 0xfffb) Write16(bytes, 36, symbols.Symbol24e6);
            else if (symbol == 0xfffc) Write16(bytes, 36, symbols.Symbol24e8);
            else if (unchecked((ushort)(symbol + 9)) < 3) Write16(bytes, 36, symbols.Symbol24e4);
            // Native has no other assignment: actual supplied residue survives.
        }
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(40), costCell.Identity);
        Write32(bytes, 52, 0x4000000);
        bytes[56] = (byte)(heapSlot[33] & 63);
        bytes[57] = 0;
        bytes[58] = permission2309;
        uint header = preparedSourceHeader(heapSlot[4]);
        cancellationToken.ThrowIfCancellationRequested();
        Write32(bytes, 64, (header >> 9) & 15);
        UpdatePrevious(request, originalParentRecord, (int)previous, categoryProfile, owningRowLookup,
            preparedSourceHeader, cancellationToken);
        costCell.Value = BinaryPrimitives.ReadInt32LittleEndian(originalParentRecord[16..]);
        cancellationToken.ThrowIfCancellationRequested();
        return request;
    }

    public static void UpdatePrevious(LegacyNativeWordRecombinationRequest request,
        ReadOnlySpan<byte> candidateParentRecord, int previousNode,
        LegacyCursiveCategoryProfile categoryProfile, LegacyNativeWordParentRowLookup owningRowLookup,
        Func<byte, uint> preparedSourceHeader, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(categoryProfile);
        ArgumentNullException.ThrowIfNull(owningRowLookup);
        ArgumentNullException.ThrowIfNull(preparedSourceHeader);
        cancellationToken.ThrowIfCancellationRequested();
        if (candidateParentRecord.Length != 88 || previousNode < 0 || previousNode != request.ReadInt32(8))
            throw new ArgumentException("Actual stored88 candidate and previous node required.");
        var bytes = request.Storage;
        Write32(bytes, 4, U16(candidateParentRecord, 24));
        Write16(bytes, 14, (ushort)(BinaryPrimitives.ReadUInt32LittleEndian(candidateParentRecord[24..]) >> 22));
        Write16(bytes, 16, U16(candidateParentRecord, 28));
        Write16(bytes, 38, U16(candidateParentRecord, 36));
        var related = owningRowLookup(previousNode, U16(candidateParentRecord, 30));
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(related);
        if (related.Record.Length != 88) throw new ArgumentException("Actual referenced stored88 record required.");
        bytes[73] = (byte)(related.Record.Span[7] & 1);
        uint header = preparedSourceHeader(candidateParentRecord[4]);
        cancellationToken.ThrowIfCancellationRequested();
        Write32(bytes, 68, (header >> 9) & 15);
        bytes[19] = Geometry(categoryProfile, candidateParentRecord[26] & 63);
        Write32(bytes, 24, unchecked((uint)First(categoryProfile, candidateParentRecord[26] & 63)));
    }

    private static byte Geometry(LegacyCursiveCategoryProfile profile, int category)
    {
        if ((uint)category >= (uint)profile.CompositeCategories.Length)
            throw new ArgumentOutOfRangeException(nameof(category));
        return checked((byte)profile.CompositeCategories.Span[category].GeometryIndex);
    }
    private static int First(LegacyCursiveCategoryProfile profile, int category)
        => profile.CompositeCategories.Span[category].FirstId;
    private static ushort U16(ReadOnlySpan<byte> source, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(source[offset..]);
    private static ulong U64(ReadOnlySpan<byte> source, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(source[offset..]);
    private static void Write16(Span<byte> target, int offset, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(target[offset..], value);
    private static void Write32(Span<byte> target, int offset, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(target[offset..], value);
}
