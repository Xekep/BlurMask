using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Math2DomValue.Probe")]

namespace InkRecognition.Math;

// Borrowed bytes, not a DOM node/owner. The caller retains its lifetime gate.
internal readonly ref struct Math2DomValueView(byte tag, ReadOnlySpan<byte> storage,
    ReadOnlySpan<byte> heap = default, bool hasHeapPointer = false)
{
    internal byte Tag { get; } = tag;
    internal ReadOnlySpan<byte> Storage { get; } = storage;
    internal ReadOnlySpan<byte> Heap { get; } = heap;
    internal bool HasHeapPointer { get; } = hasHeapPointer;
}

internal readonly ref struct Math2DomTextView(bool hasPointer, ReadOnlySpan<byte> storage, int length)
{
    internal bool HasPointer { get; } = hasPointer;
    internal ReadOnlySpan<byte> Storage { get; } = storage;
    // Preserve MOVSX inline length and raw DWORD length. Invalid lanes cannot
    // become a managed slice; they remain observable metadata until admission.
    internal int Length { get; } = length;
    internal ReadOnlySpan<byte> BoundedBytes => HasPointer && (uint)Length <= (uint)Storage.Length
        ? Storage[..Length] : throw new NotSupportedException("DOM string storage does not cover its declared interval.");
}

internal interface IMath2DomFloatingFormatter
{
    int Format(double value, Span<byte> destination, int precision, int nativeParameter);
}

internal static class Math2DomValueConversion
{
    // Document0078D0. Built-in mode2 formatting uses exact integer nearest-even
    // generation; the reviewed caller is precision9/17 and parameter-1.
    internal static Math2DomTextView Text(Math2DomValueView value, Span<byte> scratch,
        IMath2DomFloatingFormatter? floating = null)
    {
        var data = value.Storage;
        switch (value.Tag)
        {
            case 1:
                return new(value.HasHeapPointer, value.Heap,
                    unchecked((int)BinaryPrimitives.ReadUInt32LittleEndian(data[8..])));
            case 2:
                int length = unchecked((sbyte)data[15]);
                return new(true, data, length == 0 ? 15 : length);
            case 3:
                return new(true, data, 0);
            case 4: case 5: case 6: case 7: case 8: case 9: case 10:
                if (scratch.Length < 50) throw new ArgumentException("Native numeric conversion requires 50 scratch bytes.", nameof(scratch));
                scratch = scratch[..50];
                int count = value.Tag switch
                {
                    4 => Math2DomStringService.FormatSigned(BinaryPrimitives.ReadInt64LittleEndian(data), scratch),
                    6 => Math2DomStringService.FormatBoolean(data[0] != 0, scratch),
                    7 => Math2DomStringService.FormatSigned(BinaryPrimitives.ReadInt32LittleEndian(data), scratch),
                    8 => Math2DomStringService.FormatUnsigned(BinaryPrimitives.ReadUInt32LittleEndian(data), scratch),
                    9 => Math2DomStringService.FormatUnsigned(BinaryPrimitives.ReadUInt64LittleEndian(data), scratch),
                    5 => RequireFloating(floating).Format(BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(data)), scratch, 17, -1),
                    10 => RequireFloating(floating).Format((double)BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(data)), scratch, 9, -1),
                    _ => throw new InvalidOperationException()
                };
                return new(true, scratch, count);
            default:
                return new(false, default, 0);
        }
    }

    private static IMath2DomFloatingFormatter RequireFloating(IMath2DomFloatingFormatter? service)
        => service ?? Math2DomFloatingFormatter.Default;

    // Document031E20. Unlike CB8's op7 callback, strings require FULL bounded
    // consumption here; trailing bytes and leading whitespace are not accepted.
    internal static long Number(Math2DomValueView value, long fallback, Action<uint> unsupported)
    {
        ArgumentNullException.ThrowIfNull(unsupported);
        var data = value.Storage;
        switch (value.Tag)
        {
            case 1: case 2:
                var text = Text(value, default);
                long number = 0;
                int end = Math2DomStringService.ParseDecimal(text.BoundedBytes, 0, ref number);
                return end == text.Length ? number : fallback;
            case 4: case 9:
                return Signed64(BinaryPrimitives.ReadInt64LittleEndian(data));
            case 6:
                return data[0] != 0 ? 1 : 0;
            case 7:
                return BinaryPrimitives.ReadInt32LittleEndian(data);
            case 8:
                return BinaryPrimitives.ReadUInt32LittleEndian(data);
            case 5:
                double d = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(data));
                double rounded = d < 0 ? d - .5 : d + .5;
                return double.IsNaN(rounded) || rounded < -9223372036854775808d || rounded >= 9223372036854775808d
                    ? long.MinValue : (long)rounded;
            case 10:
                float f = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(data));
                float shifted = f < 0 ? f - .5f : f + .5f;
                return float.IsNaN(shifted) || shifted < -2147483648f || shifted >= 2147483648f
                    ? int.MinValue : (int)shifted;
            default:
                unsupported(0x02000000);
                return fallback;
        }
    }

    // Fast typed branch shared with the real fresh publisher: no temporary
    // byte serialization/allocation and no authority derived from scalar data.
    internal static long Signed64(long actualValue) => actualValue;

    internal static long RegistrationTag(long actualValue)
    {
        long tag = Signed64(actualValue);
        if (tag == long.MaxValue) throw new NotSupportedException("Native CB8 registration rejects its INT64_MAX sentinel.");
        return tag;
    }

    internal static long CollectedTag(long actualValue)
    {
        long tag = Signed64(actualValue);
        if (tag == -1) throw new NotSupportedException("Native CB8 tag collection rejects its -1 sentinel.");
        return tag;
    }
}
