using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Resource2a9/v100 and the ordinary2dd initializer06ddd0.</summary>
public readonly record struct LegacyRawLineCandidateProfile(uint MaximumUnlabeledMerges,
    float MaximumExtent, bool HorizontalEnabled, bool VerticalEnabled, bool DirectionalHorizontalMerge,
    float BaseCost, float XGapWeight, float YGapWeight, float OrderedGapWeight,
    float VarianceWeight, float InverseVarianceWeight, float ZeroVarianceCost,
    float RegressionBlendLength, float MergeGapWeight, float UpperOverlapWeight,
    float LowerOverlapWeight, float OverlapBlendLength, float MaximumOverlap)
{
    public static LegacyRawLineCandidateProfile ReadV100Node(ReadOnlySpan<byte> resource, int offset)
    {
        if (resource.Length > 64 * 1024 * 1024 || offset < 0 || offset > resource.Length - 79)
            throw new InvalidDataException("Missing bounded2a9/v100 resource.");
        var node = resource.Slice(offset, 79);
        if (BinaryPrimitives.ReadUInt32LittleEndian(node) != 0x0101fe90 ||
            BinaryPrimitives.ReadUInt32LittleEndian(node[4..]) != 71 ||
            BinaryPrimitives.ReadUInt32LittleEndian(node[8..]) != 0x2a9 ||
            BinaryPrimitives.ReadUInt16LittleEndian(node[12..]) != 0 ||
            BinaryPrimitives.ReadUInt16LittleEndian(node[14..]) != 0x100 ||
            node[24] > 1 || node[25] > 1 || node[26] > 1)
            throw new InvalidDataException("Expected unencoded2a9/v100 node.");
        var p = new LegacyRawLineCandidateProfile(BinaryPrimitives.ReadUInt32LittleEndian(node[16..]),
            BinaryPrimitives.ReadSingleLittleEndian(node[20..]), node[24] != 0, node[25] != 0, node[26] != 0,
            F(node, 27), F(node, 31), F(node, 35), F(node, 39), F(node, 43), F(node, 47), F(node, 51),
            F(node, 55), F(node, 59), F(node, 63), F(node, 67), F(node, 71), F(node, 75));
        p.Validate(); return p;
    }

    private static float F(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadSingleLittleEndian(data[offset..]);

    internal void Validate()
    {
        ReadOnlySpan<float> values = [MaximumExtent, BaseCost, XGapWeight, YGapWeight, OrderedGapWeight,
            VarianceWeight, InverseVarianceWeight, ZeroVarianceCost, RegressionBlendLength,
            MergeGapWeight, UpperOverlapWeight, LowerOverlapWeight, OverlapBlendLength, MaximumOverlap];
        foreach (float v in values)
            if (!float.IsFinite(v) || v < 0) throw new ArgumentException("Invalid raw line cost coefficient.");
        if (MaximumUnlabeledMerges > 4096 || (!HorizontalEnabled && !VerticalEnabled))
            throw new ArgumentException("Bounded merge budget and an enabled projection required.");
    }
}
