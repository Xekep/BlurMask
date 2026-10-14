using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Literal1fce60 stored88-row comparison. Differing source IDs whose
/// descriptor kind is1400h retain the native non-antisymmetric priority.</summary>
public sealed class LegacyNativeStoredWordComparer
{
    private readonly uint[] headers;

    public LegacyNativeStoredWordComparer(ReadOnlySpan<uint> preparedSourceHeaders22e0)
    {
        if (preparedSourceHeaders22e0.Length is < 1 or > 256)
            throw new ArgumentException("Actual bounded source-header table required.");
        headers = preparedSourceHeaders22e0.ToArray();
    }

    public int Compare(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        if (left.Length != 88 || right.Length != 88)
            throw new ArgumentException("Actual complete stored88 rows required.");
        int result;
        if ((result = U64(left, 40).CompareTo(U64(right, 40))) != 0) return result;
        if ((result = ((uint)left[33] & 63).CompareTo((uint)right[33] & 63)) != 0) return result;
        if ((result = ((uint)left[39] & 1).CompareTo((uint)right[39] & 1)) != 0) return result;
        if ((result = U16(left, 36).CompareTo(U16(right, 36))) != 0) return result;
        if ((result = U32(left, 24).CompareTo(U32(right, 24))) != 0) return result;
        if ((result = U16(left, 28).CompareTo(U16(right, 28))) != 0) return result;
        if ((result = ((uint)left[33] >> 7).CompareTo((uint)right[33] >> 7)) != 0) return result;
        if (left[4] == right[4]) return 0;
        if ((GetHeader(left[4]) & 0x1e00) == 0x1400) return -1;
        return (GetHeader(right[4]) & 0x1e00) == 0x1400 ? 1 : 0;
    }

    public uint GetHeader(byte sourceId) => sourceId < headers.Length ? headers[sourceId] :
        throw new ArgumentException("Source ID lies outside the actual prepared header table.");

    private static uint U16(ReadOnlySpan<byte> row, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(row[offset..]);
    private static uint U32(ReadOnlySpan<byte> row, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(row[offset..]);
    private static ulong U64(ReadOnlySpan<byte> row, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(row[offset..]);
}
