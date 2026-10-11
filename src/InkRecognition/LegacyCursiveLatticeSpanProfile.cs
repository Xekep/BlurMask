using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>Checked first two serialized fields of Text type21b version10a.</summary>
public readonly record struct LegacyCursiveLatticeSpanProfile(byte MaximumAtoms,byte NativeSecondByte)
{
    /// <summary>
    /// Read the span prefix of one complete uncompressed version10a node.
    /// Remaining geometric/token policies are deliberately not interpreted.
    /// The native cap may exceed the separately checked ordinary-chain API limit.
    /// </summary>
    public static LegacyCursiveLatticeSpanProfile ReadV10aResourceNode(ReadOnlySpan<byte> resource,int nodeOffset)
    {
        if(resource.Length>32*1024*1024 || nodeOffset<0 || nodeOffset>resource.Length-355)
            throw new InvalidDataException("Missing or oversized type21b node.");
        var node=resource.Slice(nodeOffset,355);
        if(BinaryPrimitives.ReadUInt32LittleEndian(node)!=0x0101fe90 ||
            BinaryPrimitives.ReadUInt32LittleEndian(node[4..])!=347 ||
            BinaryPrimitives.ReadUInt32LittleEndian(node[8..])!=0x21b ||
            BinaryPrimitives.ReadUInt16LittleEndian(node[12..])!=0 ||
            BinaryPrimitives.ReadUInt16LittleEndian(node[14..])!=0x10a || node[16]==0)
            throw new InvalidDataException("Expected complete uncompressed type21b version10a span prefix.");
        return new(node[16],node[17]);
    }
}
