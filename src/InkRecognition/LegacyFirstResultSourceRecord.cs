using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>The two inclusive twelve-byte source keys emitted by graph21b.virtual120.</summary>
public readonly record struct LegacyFirstResultSourceRange(LegacySourcePointKey First, LegacySourcePointKey Last);

/// <summary>Signed fields of the result-service48 glyph reference. This is a
/// native graph key, not a physical-stroke ordinal or a Unicode character.</summary>
public readonly record struct LegacyFirstResultGlyphKey(int Node, short Arc, short Alternative)
{
    public static LegacyFirstResultGlyphKey Decode(ulong value)
    {
        int node = (int)(value & 0xffffff);
        if ((node & 0x800000) != 0) node -= 0x1000000;
        return new(node, unchecked((short)(value >> 24)), unchecked((short)(value >> 40)));
    }
}

/// <summary>Bounded156950/012bc0/157030/157340 source-row producer. Inputs
/// must come from actual nested glyph alternatives and graph source-range callbacks.
/// It neither materializes273 glyphs nor infers a main247 line/frame. The
/// pointer at00 is represented by the separately owned Ranges collection;
/// its serialized placeholder is zero, not a native address.</summary>
public sealed class LegacyFirstResultSourceRecord
{
    private readonly byte[] storage;
    private readonly LegacyFirstResultSourceRange[] ranges;
    public const int ByteSize = 720;
    /// <summary>Portable work bound; not a native157030 algorithm parameter.
    /// Repeated/overlapping spans each consume their own point visits.</summary>
    public const int MaximumGeometryPointVisits = 1_048_576;
    public int LabelCount { get; }
    public IReadOnlyList<LegacyFirstResultSourceRange> Ranges { get; }
    public ReadOnlyMemory<byte> CopyStorage() => storage.ToArray();
    private LegacyFirstResultSourceRecord(byte[] storage, LegacyFirstResultSourceRange[] ranges, int count)
    { this.storage = storage; this.ranges = ranges; LabelCount = count; Ranges = Array.AsReadOnly(ranges); }

    /// <summary>Ordinary157c60 flat-stroke submission filter. Source scale is
    /// the parent's actual secondary.B0 double. It is not frame height or a
    /// classifier confidence. The last physical stroke is kept if otherwise
    /// the child would receive no input.</summary>
    public static int[] SelectChildPhysicalIndices(IReadOnlyList<InkStroke> parentStrokes,
        int firstPhysical, int lastPhysical, int descriptorKind, double sourceScale,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parentStrokes);
        cancellationToken.ThrowIfCancellationRequested();
        if (parentStrokes.Count is < 1 or > 256 || firstPhysical < 0 || lastPhysical < firstPhysical ||
            lastPhysical >= parentStrokes.Count || descriptorKind is not (0 or 6) ||
            !double.IsFinite(sourceScale) || sourceScale < 0)
            throw new ArgumentException("Actual bounded ordinary descriptor physical interval and parent source scale required.");
        var selected = new List<int>();
        for (int stroke = firstPhysical; stroke <= lastPhysical; stroke++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var points = parentStrokes[stroke]?.Points ?? throw new ArgumentException("Null physical stroke.");
            if (points.Count is < 1 or > 4096) throw new ArgumentException("Nonempty bounded physical stroke required.");
            float minX = points[0].X, maxX = minX, minY = points[0].Y, maxY = minY;
            foreach (var p in points)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!float.IsFinite(p.X) || !float.IsFinite(p.Y)) throw new ArgumentException("Finite retained points required.");
                if (p.X < minX) minX = p.X; if (maxX < p.X) maxX = p.X;
                if (p.Y < minY) minY = p.Y; if (maxY < p.Y) maxY = p.Y;
            }
            float width = maxX - minX, height = maxY - minY;
            if (descriptorKind == 0 && (double)width > sourceScale * 3.0 &&
                (double)(height / width) < .02 && (selected.Count != 0 || stroke != lastPhysical)) continue;
            selected.Add(stroke);
        }
        return selected.ToArray();
    }

    /// <summary>157c60 updates descriptor34/38 around append. Only an empty
    /// kind0/6 result interval is accepted; reused-source cloning157970 and
    /// downstream descriptor shifts158970 are deliberately separate.</summary>
    public static byte[] PublishDescriptorInterval(ReadOnlySpan<byte> descriptor88,
        int existingSourceCount, int appendedSourceCount, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (descriptor88.Length != 88 || existingSourceCount is < 0 or > 4096 ||
            appendedSourceCount < 0 || appendedSourceCount > 4096 - existingSourceCount)
            throw new ArgumentException("Bounded complete source descriptor and append counts required.");
        int kind = U16(descriptor88, 0) & 15;
        if (kind is not (0 or 6) || I32(descriptor88, 56) > I32(descriptor88, 52))
            throw new NotSupportedException("157c60 requires an ordinary unresolved descriptor result interval.");
        byte[] result = descriptor88.ToArray();
        W32(result, 52, existingSourceCount); W32(result, 56, existingSourceCount + appendedSourceCount);
        return result;
    }

    /// <summary>Source keys refer to the filtered child physical buffers;
    /// physicalMap restores their parent-buffer ordinals. All endpoint points
    /// are inclusive retained-point indices, not original raw-point IDs.</summary>
    public static LegacyFirstResultSourceRecord Create(ReadOnlySpan<byte> glyphAlternatives40,
        int selectedAlternative, IReadOnlyList<LegacyFirstResultSourceRange> childRanges,
        IReadOnlyList<int> physicalMap, IReadOnlyList<InkStroke> parentStrokes,
        LegacyCursiveCategoryProfile categories, float scoreExponent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(childRanges); ArgumentNullException.ThrowIfNull(physicalMap);
        ArgumentNullException.ThrowIfNull(parentStrokes); ArgumentNullException.ThrowIfNull(categories);
        cancellationToken.ThrowIfCancellationRequested();
        if (glyphAlternatives40.Length % 40 != 0 || glyphAlternatives40.Length / 40 is < 1 or > 4096 ||
            (uint)selectedAlternative >= (uint)(glyphAlternatives40.Length / 40) ||
            childRanges.Count is < 1 or > 4096 || physicalMap.Count is < 1 or > 256 ||
            parentStrokes.Count is < 1 or > 256 || !float.IsFinite(scoreExponent))
            throw new ArgumentException("Bounded actual nested glyph alternatives, source ranges and explicit exponent required.");
        int[] mapping = physicalMap.ToArray();
        for (int i = 0; i < mapping.Length; i++)
            if ((uint)mapping[i] >= (uint)parentStrokes.Count || (i != 0 && mapping[i] <= mapping[i - 1]))
                throw new ArgumentException("Filtered child strokes must map in increasing parent order.");
        foreach (var stroke in parentStrokes)
        {
            if (stroke?.Points.Count is not (>= 1 and <= 4096))
                throw new ArgumentException("Owned finite retained parent physical buffers required.");
            foreach (var p in stroke.Points)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!float.IsFinite(p.X) || !float.IsFinite(p.Y))
                    throw new ArgumentException("Owned finite retained parent physical buffers required.");
            }
        }
        var projected = new List<LegacyFirstResultSourceRange>();
        foreach (var range in childRanges)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var a = range.First; var b = range.Last;
            if (a.First != b.First || a.Second < 0 || b.Second < a.Second || b.Second >= mapping.Length ||
                a.Third < 0 || b.Third < 0 || a.Third >= parentStrokes[mapping[a.Second]].Points.Count ||
                b.Third >= parentStrokes[mapping[b.Second]].Points.Count || a.Second == b.Second && b.Third < a.Third)
                throw new NotSupportedException("Ordered same-group inclusive source keys required.");
            //012bc0 splits discontinuous mappings and then coalesces adjacent
            //parent spans. This equivalent maximal-run form preserves both
            //cropped outer endpoints and full intervening retained strokes.
            int run = a.Second;
            for (int s = a.Second; s <= b.Second; s++)
            {
                if (s != b.Second && mapping[s + 1] == mapping[s] + 1) continue;
                int firstPoint = run == a.Second ? a.Third : 0;
                int lastPoint = s == b.Second ? b.Third : parentStrokes[mapping[s]].Points.Count - 1;
                if (projected.Count == 4096) throw new ArgumentException("Projected source ranges exceed bounded storage.");
                projected.Add(new(new(a.First, mapping[run], firstPoint), new(b.First, mapping[s], lastPoint)));
                run = s + 1;
            }
        }
        //Preflight counts visits without walking point coordinates. Native
        //157030 visits overlapping spans repeatedly; do not silently merge
        //their membership or permit unbounded work from bounded range counts.
        int remainingPointVisits = MaximumGeometryPointVisits;
        foreach (var range in projected)
            for (int stroke = range.First.Second; stroke <= range.Last.Second; stroke++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int first = stroke == range.First.Second ? range.First.Third : 0;
                int last = stroke == range.Last.Second ? range.Last.Third : parentStrokes[stroke].Points.Count - 1;
                int visits = last - first + 1;
                if (visits > remainingPointVisits)
                    throw new ArgumentException("Source geometry exceeds the portable point-visit work budget.");
                remainingPointVisits -= visits;
            }
        byte[] data = new byte[ByteSize];
        BinaryPrimitives.WriteInt64LittleEndian(data.AsSpan(8), projected.Count);
        W32(data, 16, -1); //156950: no pre-existing source item
        float minX = float.MaxValue, maxX = -float.MaxValue, minY = float.MaxValue, maxY = -float.MaxValue;
        foreach (var range in projected)
            for (int stroke = range.First.Second; stroke <= range.Last.Second; stroke++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var points = parentStrokes[stroke].Points;
                int first = stroke == range.First.Second ? range.First.Third : 0;
                int last = stroke == range.Last.Second ? range.Last.Third : points.Count - 1;
                for (int point = first; point <= last; point++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var p = points[point];
                    if (p.X < minX) minX = p.X; if (maxX < p.X) maxX = p.X;
                    if (p.Y < minY) minY = p.Y; if (maxY < p.Y) maxY = p.Y;
                }
            }
        WF(data, 24, minX); WF(data, 28, maxX); WF(data, 32, minY); WF(data, 36, maxY);
        var selected = glyphAlternatives40.Slice(selectedAlternative * 40, 40);
        ushort chosen = U16(selected, 8);
        W16(data, 40, chosen);
        WF(data, 80, (float)Math.Pow(F(selected, 12), scoreExponent));
        W32(data, 84, I32(selected, 16)); W32(data, 240, I32(selected, 20));
        int count = 1;
        for (int row = 0; row < glyphAlternatives40.Length / 40 && count < 20; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = glyphAlternatives40.Slice(row * 40, 40); ushort label = U16(source, 8);
            bool found = false;
            for (int earlier = 0; earlier < count; earlier++) if (U16(data, 40 + earlier * 2) == label) { found = true; break; }
            if (found) continue;
            W16(data, 40 + count * 2, label);
            W32(data, 80 + count * 8, I32(source, 12)); W32(data, 84 + count * 8, I32(source, 16));
            W32(data, 240 + count * 4, I32(source, 20)); count++;
        }
        for (int i = count; i < 20; i++) { W16(data, 40 + i * 2, ushort.MaxValue); W32(data, 240 + i * 4, 0x4000000); }
        for (int i = 0; i < 20; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ushort label = U16(data, 40 + i * 2);
            if (label == ushort.MaxValue) continue;
            if (label >= categories.StartGeometryMap.Length) throw new NotSupportedException("Result label lacks its owned source category entry.");
            int start = categories.StartGeometryMap.Span[label];
            if (start == -1) continue;
            var prototype = categories.Prototypes.Span[start];
            float lower = -1f - prototype.First * 2f;
            float upper = prototype.Second * 2f + lower;
            WF(data, 560 + i * 8, lower); WF(data, 400 + i * 8, upper);
            if (categories.EndGeometryMap.Span[label] == -1) W32(data, 320 + i * 4, 1);
            else
            {
                //157340 deliberately indexes StartMap again for the second
                //channel; using EndMap's prototype here changes native data.
                WF(data, 564 + i * 8, lower); WF(data, 404 + i * 8, upper);
                W32(data, 320 + i * 4, 2);
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(data, projected.ToArray(), count);
    }

    private static ushort U16(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadUInt16LittleEndian(d[o..]);
    private static int I32(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadInt32LittleEndian(d[o..]);
    private static float F(ReadOnlySpan<byte> d, int o) => BitConverter.Int32BitsToSingle(I32(d, o));
    private static void W16(byte[] d, int o, ushort v) => BinaryPrimitives.WriteUInt16LittleEndian(d.AsSpan(o), v);
    private static void W32(byte[] d, int o, int v) => BinaryPrimitives.WriteInt32LittleEndian(d.AsSpan(o), v);
    private static void WF(byte[] d, int o, float v) => W32(d, o, BitConverter.SingleToInt32Bits(v));
}
