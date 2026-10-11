namespace InkRecognition;

/// <summary>
/// Individually recovered sampler297 geometry stages. These methods consume
/// prepared point records; they do not replace the complete native cut producer.
/// </summary>
public static class LegacyCursiveMarkerGeometry
{
    /// <summary>
    /// The vertical-direction pass of191950. Down-to-up reversals receive7;
    /// up-to-down reversals receive8 only when the previous segment moves right.
    /// Horizontal segments retain direction. Final points are not inspected.
    /// </summary>
    public static int[] MarkVerticalReversals(ReadOnlySpan<int> sourcePointCounts,
        ReadOnlySpan<float> nextDeltaX, ReadOnlySpan<float> nextDeltaY,
        ReadOnlySpan<int> preparedTags, CancellationToken cancellationToken = default)
    {
        int count = Validate(sourcePointCounts, preparedTags, cancellationToken);
        if (nextDeltaX.Length != count || nextDeltaY.Length != count)
            throw new ArgumentException("Prepared segment deltas must match the physical point count.");
        ValidateFinite(nextDeltaX); ValidateFinite(nextDeltaY);
        var tags = preparedTags.ToArray();
        int start = 0;
        foreach (int points in sourcePointCounts)
        {
            int direction = 0;
            for (int i = start; i < start + points - 1; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (nextDeltaY[i] < 0)
                {
                    if (direction == -1) tags[i] = 7;
                    direction = 1;
                }
                else if (nextDeltaY[i] > 0)
                {
                    if (direction == 1 && i > 0 && nextDeltaX[i-1] > 0) tags[i] = 8;
                    direction = -1;
                }
            }
            start += points;
        }
        return tags;
    }

    /// <summary>
    ///1911d0 distance pruning, over the complete prepared point buffer. Tags1
    /// are pen lifts. The immediate point before a lift and final point are
    /// cleared independently of distance; other points clear only below radius.
    /// Equality stops pruning. The caller supplies native prepared lengths.
    /// </summary>
    public static int[] PruneNearPenLifts(ReadOnlySpan<int> sourcePointCounts,
        ReadOnlySpan<float> nextSegmentLengths, ReadOnlySpan<int> preparedTags,
        float lineHeight, float relativeRadius, CancellationToken cancellationToken = default)
    {
        int count = Validate(sourcePointCounts, preparedTags, cancellationToken);
        if (nextSegmentLengths.Length != count)
            throw new ArgumentException("Prepared lengths must match the physical point count.", nameof(nextSegmentLengths));
        if (!float.IsFinite(lineHeight) || lineHeight < 0 || !float.IsFinite(relativeRadius) || relativeRadius < 0)
            throw new ArgumentOutOfRangeException(nameof(lineHeight), "Pruning radius requires finite nonnegative operands.");
        ValidateFinite(nextSegmentLengths);
        foreach (float length in nextSegmentLengths)
            if (length < 0) throw new ArgumentException("Prepared lengths cannot be negative.", nameof(nextSegmentLengths));
        float radius = lineHeight * relativeRadius;
        if (!float.IsFinite(radius)) throw new ArgumentOutOfRangeException(nameof(lineHeight), "Pruning radius overflowed.");
        var tags = preparedTags.ToArray();
        for (int i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (tags[i] != 1) continue;
            if (i > 0)
            {
                if (tags[i-1] != 1) tags[i-1] = 0;
                float distance = 0;
                for (int j = i-2; j >= 0; j--)
                {
                    distance += nextSegmentLengths[j];
                    if (distance >= radius || tags[j] == 1) break;
                    tags[j] = 0;
                }
            }
            float forward = 0;
            for (int j = i+1; j < count; j++)
            {
                forward += nextSegmentLengths[j-1];
                if (forward >= radius || tags[j] == 1) break;
                tags[j] = 0;
            }
        }
        if (tags[^1] != 1) tags[^1] = 0;
        float trailing = 0;
        for (int j = count-2; j >= 0; j--)
        {
            cancellationToken.ThrowIfCancellationRequested();
            trailing += nextSegmentLengths[j];
            if (trailing >= radius || tags[j] == 1) break;
            tags[j] = 0;
        }
        return tags;
    }

    /// <summary>
    ///191950 projection: emit nonzero tags except11..14, retaining source point
    /// order and opaque kind. No sorting, inserted endpoints or guessed cuts.
    /// </summary>
    public static LegacyCursiveCutMarker[] ProjectMarkers(ReadOnlySpan<int> sourcePointCounts,
        ReadOnlySpan<int> preparedTags, CancellationToken cancellationToken = default)
    {
        Validate(sourcePointCounts, preparedTags, cancellationToken);
        var markers = new List<LegacyCursiveCutMarker>();
        int offset = 0;
        for (int stroke = 0; stroke < sourcePointCounts.Length; stroke++)
        {
            for (int point = 0; point < sourcePointCounts[stroke]; point++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int tag = preparedTags[offset + point];
                if (tag != 0 && unchecked((uint)(tag - 11)) > 3)
                    markers.Add(new(tag, stroke, point));
            }
            offset += sourcePointCounts[stroke];
        }
        return markers.ToArray();
    }

    private static int Validate(ReadOnlySpan<int> sourcePointCounts, ReadOnlySpan<int> tags, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (sourcePointCounts.Length is < 1 or > 256)
            throw new ArgumentException("Expected1..256 physical strokes.", nameof(sourcePointCounts));
        int count = 0;
        foreach (int points in sourcePointCounts)
        {
            if (points is < 1 or > 4096) throw new ArgumentException("Expected1..4096 points per stroke.", nameof(sourcePointCounts));
            count += points;
            if (count > 4096) throw new ArgumentException("Prepared ink exceeds4096 points.", nameof(sourcePointCounts));
        }
        if (tags.Length != count) throw new ArgumentException("Prepared tags must match the physical point count.", nameof(tags));
        return count;
    }

    private static void ValidateFinite(ReadOnlySpan<float> values)
    {
        foreach (float value in values)
            if (!float.IsFinite(value)) throw new ArgumentException("Prepared geometry must be finite.");
    }
}
