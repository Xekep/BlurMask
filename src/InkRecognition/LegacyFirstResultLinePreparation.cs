using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Actual 170220 alphabet lookups: F0002,F0003,-,_,30FC,4E00 and FFFD.
/// The IDs belong to the source219 alphabet; 65535 represents a failed lookup.</summary>
public readonly record struct LegacyFirstResultLineMarkers(ushort First, ushort Second, ushort Hyphen,
    ushort Underscore, ushort ProlongedSound, ushort IdeographOne, ushort Replacement);

/// <summary>Resource 24B options read by170790. Slopes in the resource are already
/// tangents; only the separate caller configuration setter16f080 converts degrees.</summary>
public readonly record struct LegacyFirstResultLineProfile(float Option10, float Option14, float Option18,
    float MinimumSlope, float MaximumSlope, byte MinimumItemCount, byte MaximumItemCount)
{
    public static LegacyFirstResultLineProfile ReadV103Node(ReadOnlySpan<byte> resource, int nodeOffset)
    {
        if (nodeOffset < 0 || nodeOffset > resource.Length - 39)
            throw new InvalidDataException("Complete version103 24B node required.");
        var n = resource[nodeOffset..];
        if (BinaryPrimitives.ReadUInt32LittleEndian(n) != 0x101fe90 ||
            BinaryPrimitives.ReadUInt32LittleEndian(n[4..]) != 31 ||
            BinaryPrimitives.ReadUInt32LittleEndian(n[8..]) != 0x24b ||
            BinaryPrimitives.ReadUInt16LittleEndian(n[12..]) != 0 ||
            BinaryPrimitives.ReadUInt16LittleEndian(n[14..]) != 0x103)
            throw new InvalidDataException("Only the confirmed unencoded39-byte 24B/version103 layout is supported.");
        var p = new LegacyFirstResultLineProfile(F(n,16), F(n,20), F(n,24), F(n,28), F(n,32), n[36], n[37]);
        p.Validate(); return p;
    }

    /// <summary>170220 constructs the C1A config from source10 physical resolution
    /// and source68 explicit line metadata. None of these inputs is a glyph bbox.</summary>
    public LegacyCharacterLineFitProfile PrepareFitProfile(float physicalResolution,
        int explicitLineCount, ReadOnlySpan<uint> soleExplicitLine)
    {
        Validate();
        if (!float.IsFinite(physicalResolution) || physicalResolution < 0 || explicitLineCount < 0 ||
            (explicitLineCount == 1 && soleExplicitLine.Length < 3))
            throw new ArgumentException("Finite physical resolution and complete actual source line metadata required.");
        float minimum = physicalResolution == 0 ? 1e-6f : (physicalResolution / 25.4f) * .75f;
        float width = float.MaxValue;
        if (explicitLineCount == 1 && soleExplicitLine[0] >= 2 && (float)soleExplicitLine[2] > 0)
            width = (float)soleExplicitLine[2] * .25f;
        if (!float.IsFinite(minimum)) throw new InvalidDataException("Source physical resolution overflow.");
        //16EF60/16F080 initialize48 to zero;170220 forwards it unchanged.
        return new(MinimumItemCount, MaximumItemCount, MinimumSlope, MaximumSlope, minimum, width, 0);
    }

    private void Validate()
    {
        if (!float.IsFinite(Option10) || !float.IsFinite(Option14) || !float.IsFinite(Option18) ||
            !float.IsFinite(MinimumSlope) || !float.IsFinite(MaximumSlope) || MinimumSlope > MaximumSlope ||
            MaximumItemCount <= MinimumItemCount)
            throw new InvalidDataException("Finite ordered24B options required.");
    }
    private static float F(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadSingleLittleEndian(bytes[offset..]);
}

/// <summary>Exact16F6F0 source720→C1A584 transfer. This consumes actual first-result
/// rows, never a guessed character rectangle. Source descriptor34/38 supplies a
/// half-open row interval; source24/28 supplies an independent physical interval.
/// It does not run24B/24A/24D/29B or materialize an OCR winner.</summary>
public static class LegacyFirstResultLinePreparation
{
    public static LegacyPreparedCharacterLineItem[] Prepare(IReadOnlyList<LegacyFirstResultSourceRecord> rows,
        int first, int end, LegacyFirstResultLineMarkers markers, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rows); cancellationToken.ThrowIfCancellationRequested();
        if (first < 0 || end < first || end > rows.Count || end - first > 256)
            throw new ArgumentException("Bounded actual descriptor34/38 half-open source interval required.");
        var output = new LegacyPreparedCharacterLineItem[end - first];
        for (int i = first; i < end; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (rows[i] is null) throw new ArgumentException("Null first-result row.");
            output[i-first] = ReadSourceRow(rows[i].CopyStorage().Span, markers);
        }
        return output;
    }

    /// <summary>The checked prepared snapshot entry point, including inactive slots.
    /// Rows produced by LegacyFirstResultSourceRecord can use Prepare instead.</summary>
    public static LegacyPreparedCharacterLineItem ReadSourceRow(ReadOnlySpan<byte> row720,
        LegacyFirstResultLineMarkers markers)
    {
        if (row720.Length != 720) throw new InvalidDataException("Exactly720 source bytes required.");
        var data = new byte[584];
        uint flags = BinaryPrimitives.ReadUInt32LittleEndian(row720[0x14..]);
        data[0] = flags != 0 && (flags & 0x2000) == 0 ? (byte)1 : (byte)0;
        ushort primary = BinaryPrimitives.ReadUInt16LittleEndian(row720[0x28..]);
        data[1] = Match(primary, markers.First) || Match(primary, markers.Second) || Match(primary, markers.Hyphen) ||
            Match(primary, markers.Underscore) || Match(primary, markers.ProlongedSound) || Match(primary, markers.IdeographOne) ? (byte)1 : (byte)0;
        data[2] = Match(primary, markers.Replacement) ? (byte)1 : (byte)0;
        row720.Slice(0x18,16).CopyTo(data.AsSpan(8));
        int count = 0;
        for (int i = 0; i < 20; i++)
        {
            if (BinaryPrimitives.ReadUInt16LittleEndian(row720[(0x28+i*2)..]) == ushort.MaxValue) break;
            row720.Slice(0x50+i*8,8).CopyTo(data.AsSpan(0x18+i*8));
            row720.Slice(0x140+i*4,4).CopyTo(data.AsSpan(0xb8+i*4));
            row720.Slice(0x190+i*8,8).CopyTo(data.AsSpan(0x108+i*8));
            row720.Slice(0x230+i*8,8).CopyTo(data.AsSpan(0x1a8+i*8));
            count++;
        }
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(4), count);
        return LegacyPreparedCharacterLineItem.ReadPrepared(data);
    }

    private static bool Match(ushort primary, ushort marker) => marker != ushort.MaxValue && primary == marker;
}
