namespace InkRecognition;

public readonly record struct LegacyCursiveGlyphBoundarySigns(short Start, short End);

/// <summary>
/// Checked1b7f70 boundary inputs appended by1b75b0. Range order must already
/// match native chronology. Ordinary initialization1b0760 zeros both suppression
/// bytes; explicit overrides remain available for other caller-owned scopes.
/// </summary>
public static class LegacyCursiveGlyphBoundaryPolicy
{
    public static LegacyCursiveGlyphBoundarySigns Resolve(ReadOnlySpan<LegacyInkRange> chronologicalRanges,
        ReadOnlySpan<int> physicalPointCounts, int candidateKind,
        bool suppressStart, bool suppressEnd, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (physicalPointCounts.Length is < 1 or > 256 || chronologicalRanges.Length is < 1 or > 15 || candidateKind is < 0 or > 255)
            throw new ArgumentException("Expected bounded source counts, chronological ranges and native group kind.");
        foreach (int count in physicalPointCounts)
            if (count is < 1 or > 4096) throw new ArgumentException("Physical counts outside the prepared source domain.");
        foreach (var range in chronologicalRanges)
            if ((uint)range.StrokeIndex >= (uint)physicalPointCounts.Length || range.StartPoint < 0 ||
                range.EndPointExclusive <= range.StartPoint || range.EndPointExclusive > physicalPointCounts[range.StrokeIndex])
                throw new ArgumentException("Boundary range lies outside its prepared physical source.");
        if (candidateKind == 1)
        {
            // Native kind1 checks1a5 once and copies that sign to both inputs;
            // it does not inspect source endpoints or1a6 in this branch.
            short both = suppressStart ? (short)-1024 : (short)1024;
            return new(both, both);
        }
        short start = !suppressStart && chronologicalRanges[0].StartPoint == 0 ? (short)1024 : (short)-1024;
        var last = chronologicalRanges[^1];
        short end = !suppressEnd && last.EndPointExclusive == physicalPointCounts[last.StrokeIndex] ? (short)1024 : (short)-1024;
        return new(start, end);
    }
}
