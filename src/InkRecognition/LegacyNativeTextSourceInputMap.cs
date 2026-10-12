namespace InkRecognition;

/// <summary>Two signed source-key fields plus a point ordinal, as stored in each half of a legacy24-byte reference.</summary>
public readonly record struct LegacyNativeTextSourceKey(int Group, int InputOrdinal, int PointOrdinal);
public readonly record struct LegacyNativeTextSourceReference(LegacyNativeTextSourceKey First, LegacyNativeTextSourceKey Last,
    byte FirstResidual = 0, byte LastResidual = 0);

/// <summary>
/// Fresh ordinary source-input rows and cumulative group offsets consumed by05a5b0/05ad80.
/// Rows retain actual source object indices. Group offsets are prepared native inputs, not physical-ordinal aliases.
/// Existing source geometry/C25 rows are intentionally outside this map.
/// </summary>
public sealed class LegacyNativeTextSourceInputMap
{
    private readonly LegacyNativeSelectedInkRange[] _rows;
    private readonly int[] _prefixes;
    public int GroupOffset { get; }
    public int InputCount => _rows.Length;

    public LegacyNativeTextSourceInputMap(ReadOnlySpan<LegacyNativeSelectedInkRange> preparedRows,
        ReadOnlySpan<int> cumulativeGroupOffsets, int groupOffset)
    {
        if (preparedRows.Length > 65536 || cumulativeGroupOffsets.Length > 65536)
            throw new ArgumentOutOfRangeException(nameof(preparedRows));
        _rows = preparedRows.ToArray(); _prefixes = cumulativeGroupOffsets.ToArray(); GroupOffset = groupOffset;
        int previous = 0;
        foreach (int offset in _prefixes)
        {
            if (offset < previous || offset > _rows.Length) throw new ArgumentException("Ordered bounded actual cumulative offsets required.");
            previous = offset;
        }
        foreach (var row in _rows)
        {
            if (row.IsKnownText || row.SourceIndex == uint.MaxValue || row.PointCount is < 1 or > 65536 || row.FirstPoint < 0 || row.LastPoint < -1 ||
                !float.IsFinite(row.FirstFraction) || !float.IsFinite(row.LastFraction) ||
                row.FirstFraction is < 0 or >= 1 || row.LastFraction is < 0 or >= 1)
                throw new ArgumentException("Fresh ordinary prepared input rows required; cached/C25 geometry unsupported.");
            int last = row.LastPoint == -1 ? row.PointCount - 1 : row.LastPoint;
            if (row.FirstPoint > last || last >= row.PointCount ||
                (row.FirstPoint == last && row.FirstFraction > row.LastFraction) ||
                (row.FirstFraction != 0 && row.FirstPoint + 1 >= row.PointCount) ||
                (row.LastFraction != 0 && last + 1 >= row.PointCount))
                throw new ArgumentException("Prepared input row exceeds its actual source coordinate interval.");
        }
    }

    /// <summary>Exact bounded05a5b0 key lookup. Nonpositive adjusted group uses the native flat-row branch.</summary>
    public bool TryResolve(int group, int inputOrdinal, out LegacyNativeSelectedInkRange row)
    {
        int adjusted = unchecked(group + GroupOffset), index = inputOrdinal;
        row = default;
        if (adjusted > 0)
        {
            if ((uint)adjusted >= (uint)_prefixes.Length || inputOrdinal < 0) return false;
            int first = _prefixes[adjusted - 1];
            if (inputOrdinal >= _prefixes[adjusted] - first) return false;
            index = inputOrdinal + first;
        }
        if ((uint)index >= (uint)_rows.Length) return false;
        row = _rows[index]; return true;
    }

    /// <summary>05ad80 traversal and053f90 ordinary projection for normal nonnegative decoded references.</summary>
    public LegacyNativeInkPositionRange[] ProjectNormalReferences(ReadOnlySpan<LegacyNativeTextSourceReference> references,
        CancellationToken cancellationToken = default)
    {
        if (references.Length > 65536) throw new ArgumentOutOfRangeException(nameof(references));
        cancellationToken.ThrowIfCancellationRequested();
        var result = new List<LegacyNativeInkPositionRange>();
        int visits = 0;
        foreach (var reference in references)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (reference.First.Group < 0 || reference.Last.Group < reference.First.Group ||
                reference.First.InputOrdinal < 0 || reference.Last.InputOrdinal < 0 ||
                reference.First.PointOrdinal < 0 || reference.Last.PointOrdinal < 0)
                throw new ArgumentException("Ordered actual normal source-reference keys required.", nameof(references));
            //The native05afa0 preflight checks both boundary keys before05ad80 emits anything.
            if (!TryResolve(reference.First.Group, reference.First.InputOrdinal, out _) ||
                !TryResolve(reference.Last.Group, reference.Last.InputOrdinal, out _))
                throw new ArgumentException("Native source-reference boundary key cannot be resolved.", nameof(references));
            if ((long)reference.Last.Group - reference.First.Group > 65536)
                throw new ArgumentOutOfRangeException(nameof(references));
            for (int group = reference.First.Group; ; group++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (++visits > 1_048_576) throw new ArgumentOutOfRangeException(nameof(references), "Prepared traversal work budget exceeded.");
                int first = group == reference.First.Group ? reference.First.InputOrdinal : 0;
                int last;
                if (group == reference.Last.Group) last = reference.Last.InputOrdinal;
                else
                {
                    long adjusted = (long)group + GroupOffset;
                    if (adjusted < 0 || adjusted >= _prefixes.Length)
                        throw new ArgumentException("Native intermediate group has no prepared prefix boundary.", nameof(references));
                    int previous = adjusted == 0 ? 0 : _prefixes[(int)adjusted - 1];
                    last = _prefixes[(int)adjusted] - previous - 1;
                }
                for (int ordinal = first; ordinal <= last; ordinal++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (++visits > 1_048_576) throw new ArgumentOutOfRangeException(nameof(references), "Prepared traversal work budget exceeded.");
                    if (!TryResolve(group, ordinal, out var row)) throw new ArgumentException("Unresolved native source-reference key.");
                    float? start = group == reference.First.Group && ordinal == reference.First.InputOrdinal
                        ? (float)reference.First.PointOrdinal - (float)reference.FirstResidual / 200f : null;
                    float? end = group == reference.Last.Group && ordinal == reference.Last.InputOrdinal
                        ? (float)reference.Last.PointOrdinal - (float)reference.LastResidual / 200f : null;
                    result.Add(LegacyNativeTextSourceRangeProjection.Project(row, start, end));
                    if (result.Count > 65536) throw new ArgumentOutOfRangeException(nameof(references));
                }
                if (group == reference.Last.Group) break;
            }
        }
        return result.ToArray();
    }
}
