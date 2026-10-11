using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Observed type21b geometry-reorder policy, before native seed-arc expansion.</summary>
public readonly record struct LegacyOrdinaryCursiveSeedProfile(bool Reorder, byte InsertionShift,
    float BackwardHeightTolerance, float InsertionFraction)
{
    public static LegacyOrdinaryCursiveSeedProfile ReadV10aResourceNode(ReadOnlySpan<byte> resource, int nodeOffset)
    {
        _ = LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(resource, nodeOffset);
        var node = resource.Slice(nodeOffset, 355);
        // Two6x6 scalar tables precede the three reorder flags.
        if (node[306] > 1 || node[307] != 1 || node[308] != 0)
            throw new NotSupportedException("Expected ordinary geometry reorder and one-slot insertion policy.");
        // Ordinary LTR branch: vertical-position adjustments must be zero.
        if (Read(node,333) != 0 || Read(node,337) != 0 || Read(node,341) != 0)
            throw new NotSupportedException("Nonzero source-height positioning adjustments are not supported.");
        float tolerance = Read(node,329), fraction = Read(node,345);
        if (!float.IsFinite(tolerance) || tolerance < 0 || !float.IsFinite(fraction) || fraction is < 0 or > 1)
            throw new InvalidDataException("Invalid ordinary seed geometry policy.");
        return new(node[306] != 0, node[307], tolerance, fraction);
    }
    private static float Read(ReadOnlySpan<byte> node,int relative) =>
        BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(node.Slice(relative,4)));
}

public readonly record struct LegacyOrdinaryCursiveSeed(int SourceAtomIndex, LegacyCursiveInkAtom Atom,
    bool Deferred, int OriginalInsertionSlot, sbyte InsertionDisplacement,
    float MinimumX, float MinimumY, float MaximumX, float MaximumY);

/// <summary>Owned native19f850 ordinary LTR seed ordering, including deferred lineage.</summary>
public sealed class LegacyOrdinaryCursiveSeedResult
{
    private readonly LegacyOrdinaryCursiveSeed[] seeds;
    public IReadOnlyList<LegacyOrdinaryCursiveSeed> Seeds => Array.AsReadOnly(seeds);
    public int DeferredCount { get; }
    internal LegacyOrdinaryCursiveSeedResult(LegacyOrdinaryCursiveSeed[] seeds)
    { this.seeds = seeds; DeferredCount = seeds.Count(s => s.Deferred); }

    /// <summary>
    /// Only the nondeferred branch has native ordinary-chain seed arcs. Deferred
    /// seeds require1a1ef0 skip alternatives; silently linearizing them is wrong.
    /// </summary>
    public LegacyCursiveCandidateLattice CreateNondeferredOrdinaryChain(int maximumAtoms,
        CancellationToken cancellationToken = default)
    {
        if (DeferredCount != 0)
            throw new NotSupportedException("Deferred native seeds require the native skip-arc producer; a reordered chain is not equivalent.");
        return LegacyCursiveCandidateLattice.GenerateOrdinaryChain(seeds.Select(s => s.Atom).ToArray(),
            Enumerable.Repeat(-1, seeds.Length).ToArray(), maximumAtoms, cancellationToken);
    }
}

/// <summary>
/// Native19f850 geometry branch for one ordinary LTR source, sampler297.version106,
/// explicit source70 flags allzero and sourceF0 boundary IDs allunknown=-1.
/// Input ownership is supplied by the existing native cut producer. This is
/// seed ordering, not full native graph construction or terminal selection.
/// </summary>
public static class LegacyOrdinaryCursiveSeedProducer
{
    public static LegacyOrdinaryCursiveSeedResult GenerateLeftToRight(LegacyCursiveCutResult cuts,
        float sourceHeight, LegacyOrdinaryCursiveSeedProfile profile,
        ReadOnlySpan<byte> sourceDirectionFlags, ReadOnlySpan<int> physicalBoundaryIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cuts);
        cancellationToken.ThrowIfCancellationRequested();
        if (!float.IsFinite(sourceHeight) || sourceHeight <= 0 || profile.InsertionShift != 1 ||
            !float.IsFinite(profile.BackwardHeightTolerance) || profile.BackwardHeightTolerance < 0 ||
            !float.IsFinite(profile.InsertionFraction) || profile.InsertionFraction is < 0 or > 1)
            throw new ArgumentException("Invalid explicit ordinary seed frame/policy.");
        var counts = cuts.PreparedInk.StrokePointCounts.Span;
        if (sourceDirectionFlags.Length != counts.Length || physicalBoundaryIds.Length != counts.Length)
            throw new ArgumentException("One explicit source direction flag and boundary ID required per physical stroke.");
        for (int i = 0; i < counts.Length; i++)
            if (sourceDirectionFlags[i] != 0 || physicalBoundaryIds[i] != -1)
                throw new NotSupportedException("Only unidirectional LTR ink with unknown source boundaries is restored here.");
        var atoms = cuts.Atoms.ToArray();
        if (atoms.Length is < 1 or > 4096)
            throw new ArgumentException("Expected1..4096 native prepared atoms.");
        var starts = new int[counts.Length];
        for (int i = 1; i < starts.Length; i++) starts[i] = checked(starts[i - 1] + counts[i - 1]);
        var points = cuts.PreparedInk.Points.Span;
        var wholeMaximumX = new float[counts.Length];
        for (int stroke = 0; stroke < counts.Length; stroke++)
        {
            float maximum = -float.MaxValue;
            for (int p = 0; p < counts[stroke]; p++) maximum = Math.Max(maximum, points[starts[stroke] + p].X);
            wholeMaximumX[stroke] = maximum;
        }
        var ordinary = new List<LegacyOrdinaryCursiveSeed>();
        var deferred = new List<LegacyOrdinaryCursiveSeed>();
        float previousMaximumX = -float.MaxValue;
        int previousStroke = -1, previousStart = -1;
        for (int atomIndex = 0; atomIndex < atoms.Length; atomIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var atom = atoms[atomIndex]; var range = atom.Range;
            if ((uint)range.StrokeIndex >= (uint)counts.Length || range.StartPoint < 0 ||
                range.EndPointExclusive <= range.StartPoint || range.EndPointExclusive > counts[range.StrokeIndex] ||
                range.StrokeIndex < previousStroke || range.StrokeIndex == previousStroke && range.StartPoint <= previousStart)
                throw new ArgumentException("Expected native source-ordered atom ownership.");
            if (atom.Marker.StrokeIndex != range.StrokeIndex || atom.Marker.PointIndex != range.StartPoint ||
                previousStroke != range.StrokeIndex && (range.StrokeIndex != previousStroke + 1 || range.StartPoint != 0) ||
                atomIndex > 0 && previousStroke == range.StrokeIndex && atoms[atomIndex - 1].Range.EndPointExclusive != range.StartPoint + 1 ||
                (atomIndex == atoms.Length - 1 || atoms[atomIndex + 1].Range.StrokeIndex != range.StrokeIndex) && range.EndPointExclusive != counts[range.StrokeIndex])
                throw new ArgumentException("Native seeds must cover every physical stroke with only shared cut endpoints.");
            previousStroke = range.StrokeIndex; previousStart = range.StartPoint;
            float minX = float.MaxValue, minY = float.MaxValue, maxX = -float.MaxValue, maxY = -float.MaxValue;
            for (int p = range.StartPoint; p < range.EndPointExclusive; p++)
            {
                var point = points[starts[range.StrokeIndex] + p];
                minX = Math.Min(minX, point.X); minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X); maxY = Math.Max(maxY, point.Y);
            }
            bool isDeferred = profile.Reorder && wholeMaximumX[range.StrokeIndex] <
                previousMaximumX - sourceHeight * profile.BackwardHeightTolerance;
            var seed = new LegacyOrdinaryCursiveSeed(atomIndex, atom, isDeferred, -1, 0, minX, minY, maxX, maxY);
            if (isDeferred) deferred.Add(seed);
            else { ordinary.Add(seed); previousMaximumX = Math.Max(previousMaximumX, maxX); }
        }
        if (previousStroke != counts.Length - 1)
            throw new ArgumentException("Native seeds omitted a physical source stroke.");
        // A finite first physical stroke always produces an ordinary seed.
        if (ordinary.Count == 0) throw new InvalidDataException("Native ordinary seed anchor is missing.");
        int ordinaryCount = ordinary.Count;
        float Position(LegacyOrdinaryCursiveSeed seed) =>
            (1f - profile.InsertionFraction) * seed.MinimumX + profile.InsertionFraction * seed.MaximumX;
        for (int i = 0; i < deferred.Count; i++)
        {
            var seed = deferred[i]; float target = Position(seed);
            int slot = 0; float best = target < ordinary[0].MinimumX ? 0 : Math.Abs(target - ordinary[0].MinimumX);
            for (int right = 1; right < ordinaryCount; right++)
            {
                float leftEdge = ordinary[right - 1].MaximumX, rightEdge = ordinary[right].MinimumX;
                float distance;
                if (leftEdge < rightEdge)
                    distance = target < leftEdge ? leftEdge - target : target > rightEdge ? target - rightEdge : 0;
                else distance = Math.Abs(target - (rightEdge + leftEdge) * .5f);
                if (distance < best) { slot = right; best = distance; }
            }
            float tail = target <= ordinary[^1].MaximumX ? Math.Abs(target - ordinary[^1].MaximumX) : 0;
            if (tail < best) slot = ordinaryCount;
            deferred[i] = seed with { OriginalInsertionSlot = slot };
        }
        // Native processes the largest integer slot first, then the largest
        // LTR weighted position. Strict ties preserve the first encounter.
        while (deferred.Count != 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int selected = 0;
            for (int i = 1; i < deferred.Count; i++)
                if (deferred[i].OriginalInsertionSlot > deferred[selected].OriginalInsertionSlot ||
                    deferred[i].OriginalInsertionSlot == deferred[selected].OriginalInsertionSlot && Position(deferred[selected]) < Position(deferred[i])) selected = i;
            var seed = deferred[selected];
            int at = Math.Min(seed.OriginalInsertionSlot + profile.InsertionShift, ordinaryCount);
            seed = seed with { InsertionDisplacement = checked((sbyte)(seed.OriginalInsertionSlot - at)) };
            ordinary.Insert(at, seed); deferred.RemoveAt(selected);
        }
        return new(ordinary.ToArray());
    }
}
