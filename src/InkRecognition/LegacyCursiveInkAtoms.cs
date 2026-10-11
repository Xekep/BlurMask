namespace InkRecognition;

/// <summary>Stored sampler297 cut marker. Kind is retained without guessing its meaning.</summary>
public readonly record struct LegacyCursiveCutMarker(int Kind, int StrokeIndex, int PointIndex);

/// <summary>One prepared sampler297 atom and ownership in its supplied prepared ink.</summary>
public readonly record struct LegacyCursiveInkAtom(LegacyCursiveCutMarker Marker, LegacyInkRange Range);

/// <summary>
/// Restored sampler297 atom ownership (1935e0) and one-atom path composition
/// (193690/193720). Caller-provided markers must come from a separate cut producer.
/// Native cuts address prepared ink, which may include inserted points; applying
/// them directly to unprepared raw strokes can select different geometry.
/// </summary>
public static class LegacyCursiveInkAtoms
{
    /// <summary>
    /// A next marker on the same physical stroke ends this atom at nextPoint+1;
    /// otherwise it ends at sourceStrokeLength. Adjacent atoms therefore share
    /// exactly one endpoint. Coordinates are neither interpolated nor duplicated
    /// by this metadata operation. Source/range counts are bounded to4096.
    /// </summary>
    public static LegacyCursiveInkAtom[] Build(ReadOnlySpan<int> sourcePointCounts,
        ReadOnlySpan<LegacyCursiveCutMarker> markers, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (sourcePointCounts.Length is < 1 or > 256) throw new ArgumentException("Expected 1..256 physical source strokes.", nameof(sourcePointCounts));
        if (markers.Length is < 1 or > 4096) throw new ArgumentException("Expected 1..4096 source cut markers.", nameof(markers));
        foreach (int count in sourcePointCounts)
            if (count is < 1 or > 4096) throw new ArgumentException("Source strokes require 1..4096 points.", nameof(sourcePointCounts));
        // Snapshot marker identity once. No marker ordering or cut location is
        // synthesized; nonmonotone source ownership is outside this producer.
        var cuts = markers.ToArray();
        for (int i = 0; i < cuts.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var marker = cuts[i];
            if ((uint)marker.StrokeIndex >= (uint)sourcePointCounts.Length || marker.PointIndex < 0 ||
                marker.PointIndex >= sourcePointCounts[marker.StrokeIndex])
                throw new ArgumentException("A cut marker is outside its physical source stroke.", nameof(markers));
            if (i != 0 && (marker.StrokeIndex < cuts[i-1].StrokeIndex ||
                marker.StrokeIndex == cuts[i-1].StrokeIndex && marker.PointIndex <= cuts[i-1].PointIndex))
                throw new ArgumentException("Cut markers require ordered, distinct source point positions.", nameof(markers));
        }
        var result = new LegacyCursiveInkAtom[cuts.Length];
        for (int i = 0; i < cuts.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var marker = cuts[i];
            int end = i + 1 < cuts.Length && cuts[i+1].StrokeIndex == marker.StrokeIndex
                ? cuts[i+1].PointIndex + 1 : sourcePointCounts[marker.StrokeIndex];
            result[i] = new(marker, new(marker.StrokeIndex, marker.PointIndex, end));
        }
        return result;
    }

    /// <summary>
    /// Compose a selected consecutive path of single-range atoms. Merge only
    /// same-stroke ranges sharing exactly one endpoint; extend the prior end,
    /// retaining its start. Adjacent disjoint ranges keep their pen-up boundary.
    /// This is the checked single-atom case of193690/193720, not a guessed
    /// implementation of native arbitrary multi-range group composition.
    /// </summary>
    public static LegacyInkRange[] Compose(ReadOnlySpan<LegacyInkRange> atoms,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (atoms.Length is < 1 or > 4096) throw new ArgumentException("Expected 1..4096 selected atoms.", nameof(atoms));
        var ranges = new List<LegacyInkRange>(Math.Min(atoms.Length,15));
        foreach (var atom in atoms)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (atom.StrokeIndex is < 0 or > 255 || atom.StartPoint < 0 || atom.StartPoint >= atom.EndPointExclusive || atom.EndPointExclusive > 4096)
                throw new ArgumentException("Malformed selected atom ownership.", nameof(atoms));
            if (ranges.Count != 0 && ranges[^1] is var previous && previous.StrokeIndex == atom.StrokeIndex &&
                previous.EndPointExclusive == atom.StartPoint + 1)
                ranges[^1] = previous with { EndPointExclusive = atom.EndPointExclusive };
            else
            {
                if (ranges.Count == 15) throw new ArgumentException("The composed glyph exceeds15 pen-separated ranges.", nameof(atoms));
                ranges.Add(atom);
            }
        }
        return ranges.ToArray();
    }
}
