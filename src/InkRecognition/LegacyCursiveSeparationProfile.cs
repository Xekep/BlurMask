using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>Ordinary grouping fields of the confirmed type297 version106 resource.</summary>
public readonly record struct LegacyCursiveSeparationProfile(float GapOffset,float Slope,int MaximumCost)
{
    public static LegacyCursiveSeparationProfile ReadV106Node(ReadOnlySpan<byte> resource,int nodeOffset)
    {
        if(nodeOffset<0 || nodeOffset>resource.Length-69)throw new InvalidDataException("Missing type297 header.");
        var node=resource[nodeOffset..];
        if(BinaryPrimitives.ReadUInt32LittleEndian(node)!=0x0101fe90 ||
            BinaryPrimitives.ReadUInt32LittleEndian(node[8..])!=0x297 ||
            BinaryPrimitives.ReadUInt16LittleEndian(node[12..])!=0 ||
            BinaryPrimitives.ReadUInt16LittleEndian(node[14..])!=0x106 || node[64]==0)
            throw new InvalidDataException("Expected uncompressed sampler297 version106 with category profile.");
        uint size=BinaryPrimitives.ReadUInt32LittleEndian(node[4..]);
        if(size<8 || size>node.Length-8)throw new InvalidDataException("Sampler size lies outside supplied resource.");
        node=node[..checked((int)size+8)];
        _=LegacyCursiveCategoryProfile.Load(node[65..],nodeOffset+65,out int categoryBytes);
        int fields=65+categoryBytes;
        // Reader196500:48/4c floats,50byte,54..74floatfields,
        // six6-float records78..104,108float,10cbyte,110..124fields.
        if(fields>node.Length-214)throw new InvalidDataException("Truncated sampler separation fields.");
        float gap=BinaryPrimitives.ReadSingleLittleEndian(node[(fields+25)..]);
        float slope=BinaryPrimitives.ReadSingleLittleEndian(node[(fields+185)..]);
        int cap=BinaryPrimitives.ReadInt32LittleEndian(node[(fields+198)..]);
        if(!float.IsFinite(gap) || !float.IsFinite(slope) || slope<0 || cap is <0 or >LegacyCursiveGroupSeparationCost.ImpossibleCost)
            throw new InvalidDataException("Sampler separation parameters exceed the checked domain.");
        return new(gap,slope,cap);
    }
}
