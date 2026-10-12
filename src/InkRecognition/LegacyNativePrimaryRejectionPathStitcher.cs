using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Literal nontracing 2113f0 on already selected 36-byte candidates and
/// actual owning primary row storage. Candidate collection 210870 is separate.
/// Mutations are live and nontransactional; no gap or missing row is invented.</summary>
public static class LegacyNativePrimaryRejectionPathStitcher
{
    private readonly record struct Point(int Node, ushort Arc, int Alternative);

    public static void Apply(ReadOnlySpan<byte> candidates, int activeCount, ushort primaryArcLimit230a,
        LegacyNativePrimaryWordRowOwner owner, LegacySymbolContextFactory contexts,
        Func<ushort, ushort> mapSymbol2360, CancellationToken cancellationToken = default,
        bool tracingEnabled2368 = false, LegacyNativeWordParentRowLookup? higherArcRowLookup = null)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(mapSymbol2360);
        cancellationToken.ThrowIfCancellationRequested();
        if (tracingEnabled2368)
            throw new NotSupportedException("Native tracing 0x280-byte side tables are not supplied.");
        if (candidates.Length % 36 != 0 || candidates.Length / 36 > 65536 ||
            (uint)activeCount > (uint)(candidates.Length / 36))
            throw new ArgumentException("Complete bounded selected 36-byte candidate storage required.");
        // The native first pass walks every selected path before any stitching.
        for (int index = 0; index < activeCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var candidate = candidates.Slice(index * 36, 36);
            if (candidate[32] == 0) continue;
            foreach (var point in Chain(candidate))
            {
                var alternative = owner.Alternative(point.Node, point.Arc, point.Alternative);
                if ((alternative[31] & 0x40) == 0) SetCost(alternative, 0x4000000);
            }
        }
        for (int index = 0; index < activeCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var candidate = candidates.Slice(index * 36, 36);
            if (candidate[32] == 0) continue;
            var chain = Chain(candidate);
            Point previous = new(I32(candidate, 16), U16(candidate, 26), candidate[31]);
            for (int position = 0; position < chain.Count; position++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var point = chain[position];
                if (position != 0) previous = chain[position - 1];
                var record = Record(point.Node, point.Arc);
                var alternative = owner.Alternative(point.Node, point.Arc, point.Alternative);
                bool linked = false;
                if (Cost(alternative) < 0x4000000)
                {
                    linked = PreviousNode(point.Node, alternative[26]) == previous.Node &&
                        U16(alternative, 24) == previous.Arc && (alternative[27] & 0x7f) == previous.Alternative;
                    if (!linked)
                    {
                        int gap = point.Alternative + 1;
                        while (gap < owner.PrimaryAlternativeCount && Cost(owner.Alternative(point.Node, point.Arc, gap)) < 0x4000000) gap++;
                        if (gap == owner.PrimaryAlternativeCount) break; // Native abandons the remaining path.
                        point = point with { Alternative = gap };
                        chain[position] = point;
                        alternative = owner.Alternative(point.Node, point.Arc, gap);
                    }
                }
                if (linked)
                {
                    alternative[31] |= 0x40;
                    continue;
                }
                if (I32(alternative, 4) > 0x3ffffff)
                {
                    int increment = unchecked(I32(owner.Alternative(previous.Node, previous.Arc, 0), 4) -
                        I32(Record(previous.Node, previous.Arc), 20) + I32(record, 20));
                    BinaryPrimitives.WriteInt32LittleEndian(alternative[4..], increment);
                    alternative[30] = record[38];
                }
                alternative[31] |= 0x40;
                alternative[26] = unchecked((byte)(point.Node - previous.Node - 1));
                alternative[31] = (byte)((alternative[31] & 0xe0) | (record[33] & 31));
                byte highBit = (byte)(alternative[27] & 0x80);
                BinaryPrimitives.WriteUInt16LittleEndian(alternative[24..], previous.Arc);
                alternative[27] = (byte)(highBit | (previous.Alternative & 0x7f));
                if (position == 0)
                {
                    owner.Alternative(previous.Node, previous.Arc, previous.Alternative)[31] |= 0x40;
                    SetCost(alternative, I32(candidate, 4));
                    alternative[31] = (byte)((alternative[31] & 0xdf) | 0x80);
                }
                else
                {
                    int physicalPrevious = PreviousNode(point.Node, record[32]);
                    ushort physicalArc = U16(record, 30);
                    int cost = unchecked(Cost(owner.Alternative(previous.Node, previous.Arc, previous.Alternative)) -
                        I32(Record(physicalPrevious, physicalArc), 16) + I32(record, 16));
                    SetCost(alternative, cost);
                }
                var source = owner.Alternative(previous.Node, previous.Arc, previous.Alternative);
                ulong mapped = U64(source, 8), raw = U64(source, 16);
                if (U16(record, 24) == point.Node || (sbyte)record[5] >= 0)
                {
                    mapped = LegacyNativeWordContextComposition.CombineMapped(contexts, mapped, U64(record, 8),
                        mapSymbol2360, cancellationToken);
                    raw = LegacyNativeWordContextComposition.CombineRaw(contexts, raw, U64(record, 8), cancellationToken);
                }
                BinaryPrimitives.WriteUInt64LittleEndian(alternative[8..], mapped);
                BinaryPrimitives.WriteUInt64LittleEndian(alternative[16..], raw);
            }
        }
        cancellationToken.ThrowIfCancellationRequested();

        ReadOnlySpan<byte> Record(int node, ushort arc)
        {
            if (arc < primaryArcLimit230a) return owner.Record(node, arc);
            if (higherArcRowLookup is null)
                throw new NotSupportedException("Actual higher-arc stored record ownership is required.");
            cancellationToken.ThrowIfCancellationRequested();
            var row = higherArcRowLookup(node, arc);
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(row);
            if (row.Record.Length != 88) throw new ArgumentException("Complete actual higher-arc stored record required.");
            return row.Record.ToArray();
        }

        List<Point> Chain(ReadOnlySpan<byte> candidate)
        {
            int start = I32(candidate, 8), node = I32(candidate, 12), alternativeIndex = candidate[30];
            ushort arc = U16(candidate, 22);
            var points = new List<Point>();
            var visited = new HashSet<Point>();
            while (start <= node && arc < primaryArcLimit230a)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var point = new Point(node, arc, alternativeIndex);
                if (points.Count == 4096 || !visited.Add(point))
                    throw new NotSupportedException("Bounded native primary path is cyclic or exceeds 4096 points.");
                points.Add(point);
                var source = owner.Alternative(node, arc, alternativeIndex);
                node = PreviousNode(node, source[26]);
                arc = U16(source, 24);
                alternativeIndex = source[27] & 0x7f;
            }
            points.Reverse(); // Native inserts each predecessor at vector front.
            return points;
        }
    }
    private static int PreviousNode(int node, byte span) => unchecked(node - (sbyte)span - 1);
    private static int I32(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadInt32LittleEndian(value[offset..]);
    private static ushort U16(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(value[offset..]);
    private static ulong U64(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(value[offset..]);
    private static int Cost(ReadOnlySpan<byte> value) => I32(value, 0);
    private static void SetCost(Span<byte> value, int cost) => BinaryPrimitives.WriteInt32LittleEndian(value, cost);
}
