using System.Buffers.Binary;

namespace InkRecognition;

public enum LegacyNativeWordRecordComparison
{
    InitialFull1fc940,
    InitialReduced1fc9c0,
    InitialFullHistory1fca30,
    InitialReducedHistory1fcac0,
    ContinuationFull1fcb50,
    ContinuationReduced1fcc20,
    ContinuationFullHistory1fccc0,
    ContinuationReducedHistory1fcdb0
}

/// <summary>Literal 1fc* comparisons, including native signed subtraction wrap
/// and the special differing-source-ID priority which is not antisymmetric.</summary>
public sealed class LegacyNativeWordRecordComparer
{
    private readonly uint[] sourceHeaders;
    public LegacyNativeWordRecordComparison Comparison { get; }

    public LegacyNativeWordRecordComparer(LegacyNativeWordRecordComparison comparison,
        ReadOnlySpan<uint> sourceDescriptorHeaders22e0 = default)
    {
        if (!Enum.IsDefined(comparison))
            throw new ArgumentOutOfRangeException(nameof(comparison));
        Comparison = comparison;
        sourceHeaders = sourceDescriptorHeaders22e0.ToArray();
    }

    public static LegacyNativeWordRecordComparison Select(bool continuation,
        bool mode22f6, byte packed22e8, bool frontend280Flag)
    {
        bool history = !mode22f6 && (packed22e8 & 7) > 1;
        return (continuation, history, frontend280Flag) switch
        {
            (false, false, false) => LegacyNativeWordRecordComparison.InitialFull1fc940,
            (false, false, true) => LegacyNativeWordRecordComparison.InitialReduced1fc9c0,
            (false, true, false) => LegacyNativeWordRecordComparison.InitialFullHistory1fca30,
            (false, true, true) => LegacyNativeWordRecordComparison.InitialReducedHistory1fcac0,
            (true, false, false) => LegacyNativeWordRecordComparison.ContinuationFull1fcb50,
            (true, false, true) => LegacyNativeWordRecordComparison.ContinuationReduced1fcc20,
            (true, true, false) => LegacyNativeWordRecordComparison.ContinuationFullHistory1fccc0,
            _ => LegacyNativeWordRecordComparison.ContinuationReducedHistory1fcdb0
        };
    }

    public int Compare(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        if (left.Length != 0x80 || right.Length != 0x80)
            throw new ArgumentException("Comparison requires complete native 0x80 records.");
        bool continuation = Comparison >= LegacyNativeWordRecordComparison.ContinuationFull1fcb50;
        bool reduced = ((int)Comparison & 1) != 0;
        bool history = ((int)Comparison & 2) != 0;
        int result;
        if (!continuation)
        {
            if ((result = U64(left, 0).CompareTo(U64(right, 0))) != 0) return result;
            if ((result = U16(left, 0x24).CompareTo(U16(right, 0x24))) != 0) return result;
            if (reduced)
            {
                if ((result = (U16(left, 0x1a) & 63).CompareTo(U16(right, 0x1a) & 63)) != 0) return result;
            }
            else
            {
                if ((result = U32(left, 0x18).CompareTo(U32(right, 0x18))) != 0) return result;
                if ((result = I32(left, 0x30).CompareTo(I32(right, 0x30))) != 0) return result;
                if ((result = I32(left, 0x34).CompareTo(I32(right, 0x34))) != 0) return result;
                if ((result = U16(left, 0x1c).CompareTo(U16(right, 0x1c))) != 0) return result;
            }
            if (history)
            {
                if ((result = U64(left, 0x70).CompareTo(U64(right, 0x70))) != 0) return result;
                if ((result = I32(left, 0x68).CompareTo(I32(right, 0x68))) != 0) return result;
            }
            if ((result = (left[0x21] >> 7).CompareTo(right[0x21] >> 7)) != 0) return result;
            return reduced ? (left[0x27] & 1).CompareTo(right[0x27] & 1) : 0;
        }

        if ((result = U64(left, 0x28).CompareTo(U64(right, 0x28))) != 0) return result;
        if ((result = (left[0x21] & 63).CompareTo(right[0x21] & 63)) != 0) return result;
        if ((result = U16(left, 0x24).CompareTo(U16(right, 0x24))) != 0) return result;
        if ((result = U32(left, 0x18).CompareTo(U32(right, 0x18))) != 0) return result;
        if ((result = U16(left, 0x1c).CompareTo(U16(right, 0x1c))) != 0) return result;
        if ((result = (left[0x21] >> 7).CompareTo(right[0x21] >> 7)) != 0) return result;
        if (reduced && (result = (left[0x27] & 1).CompareTo(right[0x27] & 1)) != 0) return result;
        if (!history)
        {
            if ((result = U32(left, 0x50).CompareTo(U32(right, 0x50))) != 0) return result;
            return reduced ? 0 : CompareSourcePriority(left[4], right[4]);
        }
        if ((result = I32(left, 0x68).CompareTo(I32(right, 0x68))) != 0) return result;
        if (!reduced && (result = CompareSourcePriority(left[4], right[4])) != 0) return result;
        return ComparePayload(left[0x40..0x54], left[4], right[0x40..0x54], right[4]);
    }

    /// <summary>Native 1f7230 payload comparison; unnormalised signed difference
    /// is retained. Only counts whose reads fit the explicit five-dword payload
    /// are supported, rather than reading beyond the stored payload.</summary>
    public static int ComparePayload(ReadOnlySpan<byte> left, byte leftSourceId,
        ReadOnlySpan<byte> right, byte rightSourceId)
    {
        if (left.Length != 20 || right.Length != 20)
            throw new ArgumentException("Native context payload has exactly five dwords.");
        int count = Math.Min(left[0], right[0]);
        if (count != 0)
        {
            if (leftSourceId != rightSourceId) return leftSourceId < rightSourceId ? -1 : 1;
            if (count > 4)
                throw new NotSupportedException("Native payload count would read beyond the explicit context payload.");
            for (int index = count; index > 0; index--)
            {
                int difference = unchecked(I32(left, index * 4) - I32(right, index * 4));
                if (difference != 0) return difference;
            }
        }
        if (left[0] != right[0]) return left[0] < right[0] ? -1 : 1;
        return unchecked(I32(left, 16) - I32(right, 16));
    }

    private int CompareSourcePriority(byte left, byte right)
    {
        if (left == right) return 0;
        if ((GetHeader(left) & 0x1e00) == 0x1400) return -1;
        return (GetHeader(right) & 0x1e00) == 0x1400 ? 1 : 0;
    }

    private uint GetHeader(byte index) => index < sourceHeaders.Length ? sourceHeaders[index] :
        throw new ArgumentException("The explicit source descriptor headers do not cover the used source ID.");
    // UInt16.CompareTo returns an arithmetic difference on .NET; widening to
    // uint preserves the literal native -1/0/+1 result for these fields.
    private static uint U16(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(value[offset..]);
    private static uint U32(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(value[offset..]);
    private static int I32(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadInt32LittleEndian(value[offset..]);
    private static ulong U64(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(value[offset..]);
}
