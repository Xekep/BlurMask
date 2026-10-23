namespace InkRecognition;
// Field transport alone confers no source, row, query or cache authority.
internal interface ILegacyNativeWordAcousticAccessor
{
    LegacyNativeWordTransitionCostCell Cost { get; }
    int ReadInt32(int offset);
    ushort ReadUInt16(int offset);
    uint ReadUInt32(int offset);
    byte ReadByte(int offset);
    void WriteByte(int offset, byte value);
}
internal readonly struct LegacyNativeFullWordAcousticAccessor(LegacyNativeWordRecombinationRequest request) : ILegacyNativeWordAcousticAccessor
{
    public LegacyNativeWordTransitionCostCell Cost=>request.Cost;
    public int ReadInt32(int offset)=>request.ReadInt32(offset);
    public ushort ReadUInt16(int offset)=>request.ReadUInt16(offset);
    public uint ReadUInt32(int offset)=>request.ReadUInt32(offset);
    public byte ReadByte(int offset)=>request.ReadByte(offset);
    public void WriteByte(int offset,byte value)=>request.WriteByte(offset,value);
}
