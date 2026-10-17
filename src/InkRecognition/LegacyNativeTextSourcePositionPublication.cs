namespace InkRecognition;

/// <summary>05AFA0 fresh ordinary input branches. Ink C21.V80=07FA50 appends
/// ordered ranges without coalescing. Cached/C25 geometry is excluded by the map.
/// Original ranges are actual source-collection selections, not glyph boxes.</summary>
public static class LegacyNativeTextSourcePositionPublication
{
    public static LegacyNativeInkPositionRange[] Project(LegacyNativeTextSourceInputMap map,
        ReadOnlySpan<LegacyNativeTextSourceReference> references, bool ordinaryGlyph,
        ReadOnlySpan<LegacyNativeInkPositionRange> originalRanges, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(map); token.ThrowIfCancellationRequested();
        if (references.Length is < 1 or > 65536 || originalRanges.Length != 1)
            throw new NotSupportedException("Bounded references and one original source collection range required.");
        if (originalRanges[0].First.Mode != 3 || originalRanges[0].Last.Mode != 5 ||
            originalRanges[0].First.Value > originalRanges[0].Last.Value)
            throw new NotSupportedException("One actual closed source collection interval required.");
        foreach (var reference in references)
        {
            token.ThrowIfCancellationRequested();
            Validate(reference.First); Validate(reference.Last);
        }
        if (ordinaryGlyph)
        {
            var result = map.ProjectNormalReferences(references, token);
            var originalEnd = originalRanges[0].Last;
            for (int i = 0; i < result.Length; i++)
            {
                token.ThrowIfCancellationRequested(); var first = result[i].First; var last = result[i].Last;
                bool closed = SameCoordinate(last, originalEnd) || last.Fraction == 255 ||
                    last.PointIndex == 0 && last.Fraction == 0 || SameCoordinate(first, last);
                result[i] = new(first, new((last.Value & ~255UL) | (closed ? 5UL : 1UL)));
            }
            return result;
        }

        //The single-reference insertion branch uses selected object boundaries,
        //not the source reference's point ordinal. Preserve this native rule.
        var insertion = new List<LegacyNativeInkPositionRange>();
        if (references.Length == 1)
        {
            var reference = references[0]; var first = Resolve(reference.First);
            int end = first.LastPoint == -1 ? first.PointCount - 1 : first.LastPoint;
            Add(first, (float)end + first.LastFraction);
            if (reference.First.Group != reference.Last.Group || reference.First.InputOrdinal != reference.Last.InputOrdinal)
            {
                var last = Resolve(reference.Last); Add(last, (float)last.FirstPoint + last.FirstFraction);
            }
        }
        else
        {
            for (int i = 0; i < references.Length; i++)
            {
                token.ThrowIfCancellationRequested(); var reference = references[i]; var row = Resolve(reference.First);
                float relative = (float)reference.First.PointOrdinal - (float)reference.FirstResidual / 200f;
                int end = row.LastPoint == -1 ? row.PointCount - 1 : row.LastPoint;
                float length = (float)((row.LastFraction > 0 ? 1 : 0) - row.FirstPoint + end);
                float absolute = i == 0 && relative == length ? (float)end + row.LastFraction :
                    (float)row.FirstPoint + (relative == 0 ? row.FirstFraction : relative);
                Add(row, absolute);
            }
        }
        return insertion.ToArray();

        void Validate(LegacyNativeTextSourceKey key)
        {
            var row = Resolve(key);
            if (key.PointOrdinal < 0 || key.PointOrdinal >= row.PointCount)
                throw new ArgumentException("Actual reference point lies outside its ordinary source object.");
        }
        LegacyNativeSelectedInkRange Resolve(LegacyNativeTextSourceKey key) =>
            map.TryResolve(key.Group, key.InputOrdinal, out var row) ? row :
                throw new ArgumentException("Actual source key cannot be resolved.");
        void Add(LegacyNativeSelectedInkRange row, float coordinate)
        {
            var position = LegacyNativeInkPosition.PackInsertion(row.SourceIndex, coordinate, row.PointCount);
            insertion.Add(new(position, position));
        }
    }

    private static bool SameCoordinate(LegacyNativeInkPosition a, LegacyNativeInkPosition b) =>
        a.SourceIndex == b.SourceIndex && a.PointIndex == b.PointIndex &&
        (a.Fraction == b.Fraction || a.Fraction == 255 || b.Fraction == 255);
}
