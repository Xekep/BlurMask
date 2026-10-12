namespace InkRecognition;

/// <summary>C1E float32 XY bounds. Empty retains the native max/min sentinels rather than an invented rectangle.</summary>
public readonly record struct LegacyNativeStrokeBounds(float Left, float Top, float Right, float Bottom)
{
    public static LegacyNativeStrokeBounds Empty => new(float.MaxValue, float.MaxValue, -float.MaxValue, -float.MaxValue);
    public bool HasPoints => Left <= Right && Top <= Bottom;
}

/// <summary>
/// Owned fresh ordinary raw-stroke branch: indexed source ranges ->081ed0 selections ->C4A/C4B points ->C1E bounds.
/// No known-text objects, affine views or C20 prepared partial-view cache are represented by this owner.
/// </summary>
public sealed class LegacyNativeRawStrokeGeometry
{
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<LegacyNativeInkPositionSnapshot,
        LegacyNativeRawStrokeGeometry> PositionOwners = new();
    private readonly InkPoint[][] _sources;
    private readonly LegacyNativeSourceInkObject[] _metadata;
    public int SourceCount => _sources.Length;

    public LegacyNativeRawStrokeGeometry(ReadOnlySpan<InkStroke> sources, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (sources.Length > LegacyNativeInkPositionSnapshot.MaximumSelections)
            throw new ArgumentOutOfRangeException(nameof(sources));
        _sources = new InkPoint[sources.Length][];
        _metadata = new LegacyNativeSourceInkObject[sources.Length];
        int total = 0;
        for (int source = 0; source < sources.Length; source++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var points = sources[source]?.Points;
            if (points is null || points.Count is < 1 or > 65536 ||
                points.Count > LegacyNativeInkPositionSnapshot.MaximumPositions - total)
                throw new ArgumentException("Bounded nonempty actual raw point arrays required.", nameof(sources));
            total += points.Count;
            var copy = new InkPoint[points.Count];
            for (int point = 0; point < copy.Length; point++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var p = points[point];
                if (!float.IsFinite(p.X) || !float.IsFinite(p.Y))
                    throw new ArgumentException("Raw point coordinates must be finite.", nameof(sources));
                copy[point] = p;
            }
            _sources[source] = copy;
            _metadata[source] = new(copy.Length);
        }
    }

    /// <summary>Fresh source range -> C79 position owner from the same indexed point snapshot.</summary>
    public LegacyNativeInkPositionSnapshot CreatePositionSnapshot(ReadOnlySpan<LegacyNativeInkPositionRange> originalRanges,
        CancellationToken cancellationToken = default)
    {
        var snapshot = LegacyNativeSelectedInkProducer.CreateSnapshot(originalRanges, _metadata, cancellationToken);
        PositionOwners.Add(snapshot, this);
        return snapshot;
    }

    /// <summary>Exact ordinary C4A integer slice or08eb50/C4B fractional endpoint view, with owned returned points.</summary>
    public InkPoint[] ReadSelectedPoints(LegacyNativeSelectedInkRange selection,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (selection.IsKnownText || selection.SourceIndex >= _sources.Length)
            throw new ArgumentException("An actual ordinary source object from this indexed owner is required.", nameof(selection));
        var source = _sources[(int)selection.SourceIndex];
        int first = selection.FirstPoint, last = selection.LastPoint == -1 ? source.Length - 1 : selection.LastPoint;
        float firstPart = selection.FirstFraction, lastPart = selection.LastFraction;
        if (selection.PointCount != source.Length || first < 0 || last < first || last >= source.Length ||
            !float.IsFinite(firstPart) || !float.IsFinite(lastPart) || firstPart is < 0 or >= 1 || lastPart is < 0 or >= 1 ||
            (firstPart != 0 && first + 1 >= source.Length) || (lastPart != 0 && last + 1 >= source.Length) ||
            (first == last && firstPart > lastPart))
            throw new ArgumentException("Finite ordered selected coordinates in the actual source owner required.", nameof(selection));
        //08eb50 count: nonzero last fraction adds one point; nonzero first fraction replaces the first integer point.
        int count = last - first + 1 + (lastPart != 0 ? 1 : 0);
        var result = new InkPoint[count];
        int cursor = 0, integerFirst = first;
        if (firstPart != 0)
        { result[cursor++] = Interpolate(source[first], source[first + 1], firstPart); integerFirst++; }
        for (int point = integerFirst; point <= last; point++)
        { cancellationToken.ThrowIfCancellationRequested(); result[cursor++] = source[point]; }
        if (lastPart != 0) result[cursor++] = Interpolate(source[last], source[last + 1], lastPart);
        if (cursor != count) throw new InvalidOperationException("C4B prepared view cardinality mismatch.");
        return result;
    }

    /// <summary>083630 ordered union of selected object bounds;0629e0 uses strict comparisons within each point view.</summary>
    public LegacyNativeStrokeBounds ReadRangeBounds(ReadOnlySpan<LegacyNativeInkPositionRange> ranges,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var selected = LegacyNativeSelectedInkProducer.Select(ranges, _metadata, cancellationToken);
        var total = LegacyNativeStrokeBounds.Empty;
        foreach (var selection in selected)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bounds = ReadPointBounds(ReadSelectedPoints(selection, cancellationToken));
            //Native aggregation overwrites equal minima, whereas its point scanner retains the earlier equal value.
            float left = bounds.Left <= total.Left ? bounds.Left : total.Left;
            float top = bounds.Top <= total.Top ? bounds.Top : total.Top;
            float right = bounds.Right <= total.Right ? total.Right : bounds.Right;
            float bottom = bounds.Bottom <= total.Bottom ? total.Bottom : bounds.Bottom;
            total = new(left, top, right, bottom);
        }
        return total;
    }

    /// <summary>Genuine prepared decoded item endpoints -> per-item geometry from source points, without labels or boxes.</summary>
    public LegacyNativeStrokeBounds[] ReadDecodedItemBounds(LegacyNativeInkPositionSnapshot positionOwner,
        int itemCount, ReadOnlySpan<LegacyNativeDecodedItemEndpoint> endpoints, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(positionOwner);
        if (!PositionOwners.TryGetValue(positionOwner, out var owner) || !ReferenceEquals(owner, this))
            throw new ArgumentException("Decoded positions must come from this actual indexed raw-source owner.", nameof(positionOwner));
        var ranges = positionOwner.ReadDecodedItemRanges(itemCount, endpoints, cancellationToken);
        var result = new LegacyNativeStrokeBounds[itemCount];
        for (int item = 0; item < result.Length; item++)
        { cancellationToken.ThrowIfCancellationRequested(); result[item] = ReadRangeBounds(ranges[item], cancellationToken); }
        return result;
    }

    private static InkPoint Interpolate(InkPoint first, InkPoint next, float fraction)
    {
        float dx = next.X - first.X, dy = next.Y - first.Y;
        float xDelta = dx * fraction, yDelta = dy * fraction;
        float x = xDelta + first.X, y = yDelta + first.Y;
        if (!float.IsFinite(x) || !float.IsFinite(y))
            throw new InvalidDataException("Native float32 interpolation overflowed finite coordinates.");
        return new(x, y);
    }

    private static LegacyNativeStrokeBounds ReadPointBounds(ReadOnlySpan<InkPoint> points)
    {
        var bounds = LegacyNativeStrokeBounds.Empty;
        foreach (var point in points)
        {
            float left = point.X < bounds.Left ? point.X : bounds.Left;
            float top = point.Y < bounds.Top ? point.Y : bounds.Top;
            float right = bounds.Right < point.X ? point.X : bounds.Right;
            float bottom = bounds.Bottom < point.Y ? point.Y : bounds.Bottom;
            bounds = new(left, top, right, bottom);
        }
        return bounds;
    }
}
