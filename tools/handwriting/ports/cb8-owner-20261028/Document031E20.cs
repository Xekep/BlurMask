using System.Buffers.Binary;

namespace Cb8OwnerProposal;

// Proposed semantic port, not a DOM owner or accepted-tag policy. The adapter
// supplies a borrowed actual attribute payload under the owning DOM's gate.
internal interface INumericAttributeServices
{
    ReadOnlyMemory<byte> StringBytes(byte tag, ReadOnlyMemory<byte> payload);
    // Null models a null returned pointer; otherwise the offset from start.
    int? ParseInt64(ReadOnlyMemory<byte> text, out long value);
    void UnsupportedValue(uint error);
}

internal static class Document031E20
{
    internal static long Read(byte tag, ReadOnlyMemory<byte> payload, long fallback,
        INumericAttributeServices services)
    {
        var p = payload.Span;
        switch (tag)
        {
            case 1:
            case 2:
                var text = services.StringBytes(tag, payload);
                int? end = services.ParseInt64(text, out long value);
                return end == text.Length ? value : fallback;
            case 4:
            case 9:
                return BinaryPrimitives.ReadInt64LittleEndian(p);
            case 6:
                return p[0] != 0 ? 1 : 0;
            case 7:
                return BinaryPrimitives.ReadInt32LittleEndian(p);
            case 8:
                return BinaryPrimitives.ReadUInt32LittleEndian(p);
            case 5:
                double d = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(p));
                return Truncate64(d < 0 ? d - 0.5 : d + 0.5);
            case 10:
                float f = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(p));
                float rounded = f < 0 ? f - 0.5f : f + 0.5f;
                return Truncate32(rounded);
            default:
                services.UnsupportedValue(0x02000000);
                return fallback;
        }
    }

    // x64 CVTT*2SI returns integer indefinite on NaN, infinity and overflow.
    private static long Truncate64(double v) => double.IsNaN(v) || v < -9223372036854775808d || v >= 9223372036854775808d
        ? long.MinValue : (long)v;
    private static int Truncate32(float v) => float.IsNaN(v) || v < -2147483648f || v >= 2147483648f
        ? int.MinValue : (int)v;
}
