using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Resource2d0/v100 reader082b80; child processor offsets retain their exact serialized identity.</summary>
public sealed record LegacyLineOrientationProfile(float MinimumDegrees, float MaximumDegrees, float StepDegrees,
    float ScaleQuantile, int? PrimaryProcessorOffset, int? SecondaryProcessorOffset, float CostCardinalityExponent,
    byte ShortlistCount, float CoverageThreshold, float MinimumForwardFraction, float MaximumRowJumpFraction,
    bool ReverseHorizontalOrder, bool FinalFlag)
{
    public static LegacyLineOrientationProfile ReadResourceNode(ReadOnlySpan<byte> resource, int offset)
    {
        if (resource.Length > 64 * 1024 * 1024 || offset < 0 || offset > resource.Length - 16)
            throw new InvalidDataException("Invalid orientation resource bounds.");
        var reader = new Reader(resource, offset);
        if (reader.U32() != 0x0101fe90) throw new InvalidDataException("Invalid orientation magic.");
        uint length = reader.U32();
        if (length < 8 || length > resource.Length - offset - 8) throw new InvalidDataException("Invalid orientation length.");
        reader.End = checked(offset + 8 + (int)length);
        if (reader.U32() != 0x2d0 || reader.U16() != 0 || reader.U16() != 0x100)
            throw new InvalidDataException("Expected unencoded resource2d0/v100.");
        float minimum = reader.Float(), maximum = reader.Float(), step = reader.Float(), quantile = reader.Float();
        int? primary = reader.Flag() ? reader.Node() : null;
        int? secondary = reader.Flag() ? reader.Node() : null;
        var result = new LegacyLineOrientationProfile(minimum, maximum, step, quantile, primary, secondary,
            reader.Float(), reader.U8(), reader.Float(), reader.Float(), reader.Float(), reader.Flag(), reader.Flag());
        if (reader.Position != reader.End) throw new InvalidDataException("Unexpected orientation resource tail.");
        result.Validate(); return result;
    }

    public void Validate()
    {
        if (!float.IsFinite(MinimumDegrees) || !float.IsFinite(MaximumDegrees) || MinimumDegrees < -180 || MaximumDegrees > 180 ||
            MinimumDegrees > MaximumDegrees || !float.IsFinite(StepDegrees) || StepDegrees <= 0 ||
            (MaximumDegrees - MinimumDegrees) / StepDegrees > 128 || !float.IsFinite(ScaleQuantile) || ScaleQuantile is < 0 or > 1 ||
            !float.IsFinite(CostCardinalityExponent) || CostCardinalityExponent is < 0 or > 8 || ShortlistCount == 0 ||
            !float.IsFinite(CoverageThreshold) || CoverageThreshold < 0 || CoverageThreshold > 1_000_000 ||
            !float.IsFinite(MinimumForwardFraction) || MinimumForwardFraction is < 0 or > 1 ||
            !float.IsFinite(MaximumRowJumpFraction) || MaximumRowJumpFraction is < 0 or > 1)
            throw new ArgumentException("Unsupported finite orientation profile.");
    }

    private ref struct Reader
    {
        private readonly ReadOnlySpan<byte> data;
        public int Position, End;
        public Reader(ReadOnlySpan<byte> data, int position) { this.data = data; Position = position; End = data.Length; }
        private int Take(int count) { if (count < 0 || count > End - Position) throw new InvalidDataException("Truncated orientation resource."); int at = Position; Position += count; return at; }
        public byte U8() => data[Take(1)];
        public bool Flag() { byte value = U8(); if (value > 1) throw new InvalidDataException("Invalid orientation flag."); return value != 0; }
        public ushort U16() => BinaryPrimitives.ReadUInt16LittleEndian(data[Take(2)..]);
        public uint U32() => BinaryPrimitives.ReadUInt32LittleEndian(data[Take(4)..]);
        public float Float() => BinaryPrimitives.ReadSingleLittleEndian(data[Take(4)..]);
        public int Node() { int start = Position; if (U32() != 0x0101fe90) throw new InvalidDataException("Invalid orientation child."); uint size = U32(); if (size < 8 || size > End - Position) throw new InvalidDataException("Invalid orientation child length."); Take((int)size); return start; }
    }
}
