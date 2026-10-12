namespace InkRecognition;

/// <summary>Portable value representation of an Ink C79 eight-byte source position. No native pointer identity.</summary>
public readonly record struct LegacyNativeInkPosition(ulong Value)
{
    public uint SourceIndex => (uint)(Value >> 32);
    public ushort PointIndex => (ushort)(Value >> 16);
    public byte Fraction => (byte)(Value >> 8);
    public byte Mode => (byte)Value;

    /// <summary>Exact bounded00db00 packing, including float32 quantization and terminal FF fraction.</summary>
    public static LegacyNativeInkPosition Pack(uint sourceIndex, float pointPosition, byte mode, int pointCount)
    {
        if (pointCount is < 1 or > 65536) throw new ArgumentOutOfRangeException(nameof(pointCount));
        if (!float.IsFinite(pointPosition) || pointPosition < 0 || pointPosition > pointCount - 1)
            throw new ArgumentOutOfRangeException(nameof(pointPosition));
        if (mode is not (1 or 3 or 5 or 7)) throw new ArgumentOutOfRangeException(nameof(mode));
        int integral = (int)Math.Floor((double)pointPosition);
        float remainder = pointPosition - (float)integral;
        bool terminalMode = mode is 3 or 5;
        byte fraction;
        if (remainder == 0 && integral == pointCount - 1)
            fraction = terminalMode ? (byte)255 : (byte)0;
        else
        {
            float scaled = remainder * 200f;
            int rounded = (int)(scaled + .5f);
            fraction = (byte)rounded;
            if (fraction == 200)
            {
                integral++;
                fraction = terminalMode && integral == pointCount - 1 ? (byte)255 : (byte)0;
            }
        }
        return new(((ulong)sourceIndex << 32) | ((ulong)(ushort)integral << 16) | ((ulong)fraction << 8) | mode);
    }

    /// <summary>Exact00ebe0 range-mode canonicalization; fractional FF semantics are preserved.</summary>
    public static LegacyNativeInkPositionRange Canonicalize(LegacyNativeInkPosition first, LegacyNativeInkPosition last)
    {
        ulong start = first.Value, end = last.Value;
        byte startMode = first.Mode, endMode = last.Mode;
        if (first.SourceIndex == last.SourceIndex && first.PointIndex == last.PointIndex &&
            (first.Fraction == last.Fraction || first.Fraction == 255 || last.Fraction == 255))
        {
            start = (start & ~255UL) | 3;
            end = (end & ~255UL) | 5;
            if (last.Fraction == 255) start = (start & ~0xff00UL) | 0xff00;
            return new(new(start), new(end));
        }
        if (startMode == 5) start = (start & ~255UL) | 3;
        else if (startMode is 1 or 7)
        {
            if (startMode == 1) start = (start & ~255UL) | 7;
            if (first.Fraction == 255) start &= ~0xff00UL;
        }
        bool specialEnd = ((unchecked((uint)(ushort)last.Value + 0xfdU)) & 0xfffdU) == 0;
        if (endMode == 5)
        {
            if (specialEnd) return new(new(start), new(end));
            end = (end & ~255UL) | 1;
        }
        else if (endMode == 3)
        {
            end = (end & ~255UL) | (specialEnd ? 5UL : 1UL);
            if (specialEnd) return new(new(start), new(end));
        }
        else if (endMode == 7) return new(new(start), new((end & ~255UL) | 5));
        else if (endMode != 1) return new(new(start), new(end));
        if (last.Fraction == 255) end &= ~0xff00UL;
        return new(new(start), new(end));
    }
}

public readonly record struct LegacyNativeInkPositionRange(LegacyNativeInkPosition First, LegacyNativeInkPosition Last);
