using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Checked scalar configuration view of a prepared controller snapshot;
/// pointer fields are neither read nor reconstructed. Fresh values are literal
/// stores in 20baa0 and do not stand in for later preparation/configuration.</summary>
public readonly record struct LegacyNativeWordControllerConfiguration(
    ushort HeaderCount2400, ushort PrimaryAlternativeCount2402, ushort SecondaryAlternativeCount2404,
    ushort PrimaryArcLimit230a, float ExpansionFactor230c, byte Mode22f6,
    uint PackedSource22e8, byte Trace2368, byte SuppressPreviousContext2609, uint OverrideHeap2458)
{
    public static LegacyNativeWordControllerConfiguration CreateFresh20baa0() =>
        new(2, 2, 2, 0, 0.25f, 0, 0, 0, 0, uint.MaxValue);

    public static LegacyNativeWordControllerConfiguration ReadPreparedSnapshot(ReadOnlySpan<byte> controller)
    {
        if (controller.Length < 0x260a)
            throw new ArgumentException("Prepared native controller snapshot must cover scalar fields through 2609.");
        return new(U16(controller, 0x2400), U16(controller, 0x2402), U16(controller, 0x2404),
            U16(controller, 0x230a), BitConverter.Int32BitsToSingle(I32(controller, 0x230c)),
            controller[0x22f6], U32(controller, 0x22e8), controller[0x2368], controller[0x2609],
            U32(controller, 0x2458));
    }

    private static ushort U16(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(value[offset..]);
    private static int I32(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadInt32LittleEndian(value[offset..]);
    private static uint U32(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(value[offset..]);
}
