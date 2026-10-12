namespace InkRecognition;

/// <summary>Actual selected input descriptor fields consumed by00f130; not a character bounding rectangle.</summary>
public readonly record struct LegacyNativeSelectedInkRange(uint SourceIndex, int PointCount,
    int FirstPoint, int LastPoint, float FirstFraction, float LastFraction, bool FirstClosed, bool LastClosed,
    bool IsKnownText = false);

/// <summary>
/// Fresh C79 position and separator production00f130 and native010850 range segmentation.
/// Does not implement00f7e0 postmerge cache generation, EED winner generation or global bounds178.
/// </summary>
public sealed class LegacyNativeInkPositionSnapshot
{
    public const int MaximumSelections = 65536;
    public const int MaximumPositions = 1048576;
    private readonly LegacyNativeInkPosition[] _positions;
    private readonly int[] _separators;
    private readonly LegacyNativeInkPositionRange[] _originalRanges;
    public int PositionCount => _positions.Length;
    public int SelectionCount => _separators.Length;
    public ReadOnlySpan<LegacyNativeInkPosition> Positions => _positions;
    public ReadOnlySpan<int> Separators => _separators;

    private LegacyNativeInkPositionSnapshot(LegacyNativeInkPosition[] positions, int[] separators,
        LegacyNativeInkPositionRange[] originals)
    { _positions = positions; _separators = separators; _originalRanges = originals; }

    /// <summary>Produces the fresh, empty-cache branch using explicit original source-collection ranges.</summary>
    public static LegacyNativeInkPositionSnapshot Create(ReadOnlySpan<LegacyNativeSelectedInkRange> selections,
        ReadOnlySpan<LegacyNativeInkPositionRange> originalRanges, CancellationToken cancellationToken = default)
    {
        if (selections.Length > MaximumSelections || originalRanges.Length > MaximumSelections)
            throw new ArgumentOutOfRangeException(nameof(selections));
        cancellationToken.ThrowIfCancellationRequested();
        var positions = new List<LegacyNativeInkPosition>();
        var separators = new int[selections.Length];
        for (int index = 0; index < selections.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var selection = selections[index];
            if (selection.IsKnownText)
            {
                Add(new(((ulong)selection.SourceIndex << 32) | 3));
                Add(new(((ulong)selection.SourceIndex << 32) | 0x3ff05));
            }
            else
            {
                if (selection.PointCount is < 1 or > 65536 || selection.FirstPoint < 0 ||
                    selection.LastPoint < -1 || !float.IsFinite(selection.FirstFraction) ||
                    !float.IsFinite(selection.LastFraction) || selection.FirstFraction is < 0 or >= 1 ||
                    selection.LastFraction is < 0 or >= 1)
                    throw new ArgumentOutOfRangeException(nameof(selections));
                int lastPoint = selection.LastPoint == -1 ? selection.PointCount - 1 : selection.LastPoint;
                float firstCoordinate = (float)selection.FirstPoint + selection.FirstFraction;
                float lastCoordinate = (float)lastPoint + selection.LastFraction;
                if (firstCoordinate > lastCoordinate) throw new ArgumentOutOfRangeException(nameof(selections));
                var first = LegacyNativeInkPosition.Pack(selection.SourceIndex, firstCoordinate,
                    selection.FirstClosed ? (byte)3 : (byte)7, selection.PointCount);
                var last = LegacyNativeInkPosition.Pack(selection.SourceIndex, lastCoordinate,
                    selection.LastClosed ? (byte)5 : (byte)1, selection.PointCount);
                bool sameCoordinate = first.SourceIndex == last.SourceIndex && first.PointIndex == last.PointIndex &&
                    (first.Fraction == last.Fraction || first.Fraction == 255 || last.Fraction == 255);
                if (sameCoordinate && (!selection.FirstClosed || !selection.LastClosed))
                    throw new ArgumentException("Native selection cannot be empty with an open endpoint.", nameof(selections));
                Add(first);
                for (int point = first.PointIndex + 1; point <= last.PointIndex; point++)
                {
                    if ((point & 255) == 0) cancellationToken.ThrowIfCancellationRequested();
                    ulong raw = ((ulong)selection.SourceIndex << 32) | ((ulong)point << 16) |
                        (point == selection.PointCount - 1 ? 0xff05UL : 5UL);
                    if (raw < last.Value) Add(new(raw));
                }
                if (!sameCoordinate) Add(last);
            }
            separators[index] = positions.Count;
        }
        return new(positions.ToArray(), separators, originalRanges.ToArray());

        void Add(LegacyNativeInkPosition position)
        {
            if (positions.Count == MaximumPositions) throw new ArgumentOutOfRangeException(nameof(selections));
            positions.Add(position);
        }
    }

    /// <summary>
    /// Returns ordered canonical source-position ranges. Full range preserves the original collection;
    /// partial ranges follow separators, including native empty gaps. Fresh-cache branch only.
    /// </summary>
    public LegacyNativeInkPositionRange[] ReadRange(int firstEndpoint, int lastEndpoint,
        CancellationToken cancellationToken = default)
    {
        if (firstEndpoint < 0 || lastEndpoint < firstEndpoint || lastEndpoint > PositionCount)
            throw new ArgumentOutOfRangeException(nameof(firstEndpoint));
        cancellationToken.ThrowIfCancellationRequested();
        if (firstEndpoint == 0 && lastEndpoint == PositionCount) return (LegacyNativeInkPositionRange[])_originalRanges.Clone();
        if (firstEndpoint == lastEndpoint) return [];
        bool gapStart = firstEndpoint != 0 && Contains(firstEndpoint + 1) && !Contains(firstEndpoint);
        bool endsAtSeparator = lastEndpoint == 0 || lastEndpoint == PositionCount || Contains(lastEndpoint);
        if (gapStart && ++firstEndpoint == lastEndpoint && endsAtSeparator) return [];
        int firstSegment = LowerBound(firstEndpoint);
        if (firstSegment < _separators.Length && _separators[firstSegment] == firstEndpoint) firstSegment++;
        int lastSegment = LowerBound(lastEndpoint);
        if (lastSegment < firstSegment) return [];
        var ranges = new List<LegacyNativeInkPositionRange>();
        for (int segment = firstSegment; segment <= lastSegment; segment++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int first = segment == firstSegment ? firstEndpoint : _separators[segment - 1];
            int last = segment < lastSegment ? _separators[segment] - 1 : lastEndpoint - (endsAtSeparator ? 1 : 0);
            if ((uint)first >= (uint)PositionCount || (uint)last >= (uint)PositionCount)
                throw new InvalidOperationException("Invalid prepared native position interval.");
            ranges.Add(LegacyNativeInkPosition.Canonicalize(_positions[first], _positions[last]));
        }
        return ranges.ToArray();
    }

    /// <summary>046ae0 endpoint consumption into existing decoded item indices, without producing labels or merging IDs.</summary>
    public LegacyNativeInkPositionRange[][] ReadDecodedItemRanges(int itemCount,
        ReadOnlySpan<LegacyNativeDecodedItemEndpoint> endpoints, CancellationToken cancellationToken = default)
    {
        if (itemCount is < 0 or > MaximumSelections || endpoints.Length > MaximumPositions)
            throw new ArgumentOutOfRangeException(nameof(itemCount));
        cancellationToken.ThrowIfCancellationRequested();
        var items = new List<LegacyNativeInkPositionRange>[itemCount];
        for (int index = 0; index < items.Length; index++) items[index] = [];
        int previous = 0;
        foreach (var endpoint in endpoints)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (endpoint.ItemIndex >= itemCount)
                throw new ArgumentOutOfRangeException(nameof(endpoints));
            var ranges = ReadRange(previous, endpoint.PositionEndpoint, cancellationToken);
            if (endpoint.ItemIndex >= 0) items[endpoint.ItemIndex].AddRange(ranges);
            previous = endpoint.PositionEndpoint;
        }
        return items.Select(item => item.ToArray()).ToArray();
    }

    private bool Contains(int endpoint)
    { int index = LowerBound(endpoint); return index < _separators.Length && _separators[index] == endpoint; }
    private int LowerBound(int endpoint)
    {
        int first = 0, last = _separators.Length;
        while (first < last) { int middle = first + (last - first) / 2;
            if (_separators[middle] < endpoint) first = middle + 1; else last = middle; }
        return first;
    }
}

public readonly record struct LegacyNativeDecodedItemEndpoint(int ItemIndex, int PositionEndpoint);
