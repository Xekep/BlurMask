using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Sampler297/v106 main-parser scalar fields used by1941b0 and
///195c40. Disabled pair-geometry storage is retained as data, not interpreted
///as an active float; the observed main profile contains an inactive NaN.</summary>
public sealed class LegacyMainWordTransitionProfile
{
    public int NodeOffset { get; }
    public LegacyCursiveCategoryProfile Categories { get; }
    public float DefaultGap { get; }
    public float TranslationThreshold { get; }
    public float GapSlope { get; }
    public float SourceGapScale { get; }
    public float SourceMatrixScale { get; }
    public int JoinMaximumCost { get; }
    public int MatrixMaximumCost { get; }
    public ReadOnlyMemory<float> Matrix { get; }
    public ReadOnlyMemory<byte> DisabledPairFields { get; }

    private LegacyMainWordTransitionProfile(int offset, LegacyCursiveCategoryProfile categories,
        float gap, float threshold, float slope, float gapScale, float matrixScale,
        int joinMaximum, int matrixMaximum, float[] matrix, byte[] disabledPairFields)
    {
        NodeOffset = offset; Categories = categories; DefaultGap = gap;
        TranslationThreshold = threshold; GapSlope = slope; SourceGapScale = gapScale;
        SourceMatrixScale = matrixScale; JoinMaximumCost = joinMaximum;
        MatrixMaximumCost = matrixMaximum; Matrix = matrix; DisabledPairFields = disabledPairFields;
    }

    public static LegacyMainWordTransitionProfile ReadV106Node(ReadOnlySpan<byte> resource, int nodeOffset,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (nodeOffset < 0 || nodeOffset > resource.Length - 69)
            throw new InvalidDataException("Missing main sampler297 node.");
        var node = resource[nodeOffset..];
        uint length = BinaryPrimitives.ReadUInt32LittleEndian(node[4..]);
        if (BinaryPrimitives.ReadUInt32LittleEndian(node) != 0x0101fe90 ||
            BinaryPrimitives.ReadUInt32LittleEndian(node[8..]) != 0x297 ||
            BinaryPrimitives.ReadUInt16LittleEndian(node[12..]) != 0 ||
            BinaryPrimitives.ReadUInt16LittleEndian(node[14..]) != 0x106 || node[64] != 1 ||
            length > node.Length - 8)
            throw new InvalidDataException("Expected owned uncompressed sampler297/v106 category profile.");
        node = node[..checked((int)length + 8)];
        var categories = LegacyCursiveCategoryProfile.Load(node[65..], nodeOffset + 65, out int consumed, cancellationToken);
        int at = 65 + consumed;
        if (node.Length - at != 214 || node[at + 8] != 0)
            throw new NotSupportedException("Only the main sampler with disabled runtime50/pair geometry is supported.");
        var matrix = new float[36];
        for (int i = 0; i < matrix.Length; i++) matrix[i] = Float(node, at + 41 + i * 4);
        float gap = Float(node, at + 29), threshold = Float(node, at + 37), slope = Float(node, at + 185);
        float gapScale = Float(node, at + 190), matrixScale = Float(node, at + 194);
        int joinMaximum = BinaryPrimitives.ReadInt32LittleEndian(node[(at + 206)..]);
        int matrixMaximum = BinaryPrimitives.ReadInt32LittleEndian(node[(at + 210)..]);
        if (node[at + 189] != 0 || slope < 0 || gapScale < 0 || matrixScale < 0 ||
            joinMaximum is < 0 or > 0x4000000 || matrixMaximum is < 0 or > 0x4000000)
            throw new NotSupportedException("Unsupported main ordinary cache configuration.");
        cancellationToken.ThrowIfCancellationRequested();
        return new(nodeOffset, categories, gap, threshold, slope, gapScale, matrixScale,
            joinMaximum, matrixMaximum, matrix, node.Slice(at + 9, 16).ToArray());
    }

    private static float Float(ReadOnlySpan<byte> data, int offset)
    {
        float value = BinaryPrimitives.ReadSingleLittleEndian(data[offset..]);
        if (!float.IsFinite(value)) throw new InvalidDataException("Active main geometry scalar is nonfinite.");
        return value;
    }
}
