namespace InkRecognition;

/// <summary>Metadata of the actual indexed source owner, not recognized labels or caller character rectangles.</summary>
public readonly record struct LegacyNativeSourceInkObject(int PointCount, bool IsKnownText = false);

/// <summary>Ink C21 global148/150: source-position ranges -> selected objects -> C79 fresh position snapshot.</summary>
public static class LegacyNativeSelectedInkProducer
{
    /// <summary>081ea0/081ed0 selected source-object count and descriptor fields, preserving range order.</summary>
    public static LegacyNativeSelectedInkRange[] Select(ReadOnlySpan<LegacyNativeInkPositionRange> ranges,
        ReadOnlySpan<LegacyNativeSourceInkObject> sourceObjects, CancellationToken cancellationToken = default)
    {
        if (ranges.Length > LegacyNativeInkPositionSnapshot.MaximumSelections ||
            sourceObjects.Length > LegacyNativeInkPositionSnapshot.MaximumSelections)
            throw new ArgumentOutOfRangeException(nameof(ranges));
        cancellationToken.ThrowIfCancellationRequested();
        var selections = new List<LegacyNativeSelectedInkRange>();
        foreach (var range in ranges)
        {
            cancellationToken.ThrowIfCancellationRequested();
            uint first = range.First.SourceIndex, last = range.Last.SourceIndex;
            if (last < first || last >= sourceObjects.Length)
                throw new ArgumentException("Source position refers outside the indexed owner.", nameof(ranges));
            int count = checked((int)(last - first + 1));
            if (count > LegacyNativeInkPositionSnapshot.MaximumSelections - selections.Count)
                throw new ArgumentOutOfRangeException(nameof(ranges));
            for (uint source = first; source <= last; source++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var input = sourceObjects[(int)source];
                bool partialFirst = source == first &&
                    (range.First.PointIndex != 0 || range.First.Fraction != 0 || range.First.Mode != 3);
                bool partialLast = source == last && range.Last.Fraction != 255;
                byte firstFraction = range.First.Fraction == 255 ? (byte)0 : range.First.Fraction;
                byte lastFraction = range.Last.Fraction == 255 ? (byte)0 : range.Last.Fraction;
                float firstPart = partialFirst ? firstFraction * .005f : 0f;
                float lastPart = partialLast ? lastFraction * .005f : 0f;
                selections.Add(new(source, input.PointCount,
                    partialFirst ? range.First.PointIndex : 0,
                    partialLast ? range.Last.PointIndex : -1,
                    firstPart, lastPart, !partialFirst || range.First.Mode == 3,
                    !partialLast || range.Last.Mode == 5, input.IsKnownText));
            }
        }
        return selections.ToArray();
    }

    /// <summary>Composes the actual native descriptor producer with00f130; no inferred character labels or whole-word box.</summary>
    public static LegacyNativeInkPositionSnapshot CreateSnapshot(ReadOnlySpan<LegacyNativeInkPositionRange> ranges,
        ReadOnlySpan<LegacyNativeSourceInkObject> sourceObjects, CancellationToken cancellationToken = default) =>
        LegacyNativeInkPositionSnapshot.Create(Select(ranges, sourceObjects, cancellationToken), ranges, cancellationToken);
}
