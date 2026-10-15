using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Explicit nontracing64-byte21FE30 input. Its source/score/segment
/// producer remains responsible for every field; this type infers no bounds.</summary>
public sealed class LegacyPreparedMainWordPublicationPacket
{
    private readonly byte[] packet;
    public ushort PreviousSymbol=>BinaryPrimitives.ReadUInt16LittleEndian(packet.AsSpan(0x2e));
    public int FirstBoundaryOverride=>BinaryPrimitives.ReadInt32LittleEndian(packet.AsSpan(0x30));
    public int LastBoundaryOverride=>BinaryPrimitives.ReadInt32LittleEndian(packet.AsSpan(0x34));
    //These are218560 boundary hints. TOP273.AC/B0 are separate result-owner
    //fields and must never be inferred from these two packet slots.
    public uint BoundaryHint38Bits=>BinaryPrimitives.ReadUInt32LittleEndian(packet.AsSpan(0x38));
    public uint BoundaryHint3CBits=>BinaryPrimitives.ReadUInt32LittleEndian(packet.AsSpan(0x3c));
    public LegacyPreparedMainWordPublicationPacket(ReadOnlySpan<byte> actualPacket64)
    {
        if(actualPacket64.Length!=64)throw new ArgumentException("Complete actual nontracing packet64 required.");
        packet=actualPacket64.ToArray();
    }
    public byte[] CopyStorage()=>packet.ToArray();

    /// <summary>21FE30's field writes before218560. Retains every untouched
    /// WORD276 byte, including head/pointer identities, field14 and padding.
    /// This storage operation does not construct an owned word or publish it.</summary>
    public byte[] CopyWordStorageWithPacketFields(ReadOnlySpan<byte> actualWordStorage120)
    {
        if(actualWordStorage120.Length!=120)throw new ArgumentException("Complete actual WORD276 storage120 required.");
        byte[] result=actualWordStorage120.ToArray();
        Copy(0x00,0x10,4);Copy(0x04,0x18,4);Copy(0x08,0x1c,4);
        Copy(0x0c,0x30,2);Copy(0x0e,0x32,2);Copy(0x10,0x35,1);Copy(0x11,0x36,1);
        Copy(0x14,0x3c,4);Copy(0x1c,0x44,4);Copy(0x20,0x48,4);Copy(0x24,0x4c,4);Copy(0x28,0x50,4);Copy(0x2c,0x37,1);
        return result;
        void Copy(int source,int destination,int count)=>packet.AsSpan(source,count).CopyTo(result.AsSpan(destination,count));
    }
}
