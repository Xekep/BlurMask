namespace InkRecognition;

/// <summary>Exact child249 result fields state154/d0/158 consumed by081750; these are not acoustic probabilities.</summary>
public readonly record struct LegacyOrientationChildCost(uint CostSum, uint ElementCount, ulong Cardinality);
public readonly record struct LegacyOrientationCandidateCost(int AngleIndex, LegacyOrientationChildCost Child, int WeightedCost);
public sealed record LegacyOrientationDecision(float Degrees, float Scale, float Coverage,
    ReadOnlyMemory<LegacyOrientationProjection> RankedProjections, ReadOnlyMemory<int> RetainedAngleIndices,
    ReadOnlyMemory<LegacyOrientationCandidateCost> ChildCosts);

/// <summary>
/// Prepared2d0 shortlist/geometry/child-cost policy080e20/081af0/081660/081750/081e90.
/// Input is the output of optional child265, not a substitute for that producer.
/// Child249 remains an explicit scorer until its complete source orchestration is supplied.
/// </summary>
public static class LegacyLineOrientationPolicy
{
    public static LegacyOrientationDecision Choose(IReadOnlyList<InkStroke> scoringStrokes, LegacyLineOrientationProfile profile,
        float scale, Func<IReadOnlyList<InkStroke>, float, CancellationToken, LegacyOrientationChildCost> child249,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile); ArgumentNullException.ThrowIfNull(child249); profile.Validate();
        var angles = LegacyLineOrientationProjection.CreateAngles(profile, cancellationToken);
        var ranked = LegacyLineOrientationProjection.Score(scoringStrokes, angles, scale, cancellationToken);
        var candidates = ranked.Take(Math.Min(profile.ShortlistCount, ranked.Length)).Select(p => p.AngleIndex).ToList();
        if (scoringStrokes.Count > 8)
            for (int i = candidates.Count - 1; i >= 0; i--)
                if (RejectOrder(LegacyLineOrientationProjection.Rotate(scoringStrokes, angles[candidates[i]], cancellationToken), scale, profile, cancellationToken)) candidates.RemoveAt(i);
        if (candidates.Count == 0) return new(0, scale, 0, ranked, Array.Empty<int>(), Array.Empty<LegacyOrientationCandidateCost>());
        if (candidates.Count == 1) return new(angles[candidates[0]], scale, 0, ranked, candidates.ToArray(), Array.Empty<LegacyOrientationCandidateCost>());
        float coverage = Coverage(LegacyLineOrientationProjection.Rotate(scoringStrokes, angles[candidates[0]], cancellationToken), scale, cancellationToken);
        if (coverage >= profile.CoverageThreshold)
        {
            int first = candidates[0];
            if (!LegacyLineOrientationProjection.Approximately(MathF.Abs(angles[first]), 90))
            {
                if (scoringStrokes.Count >= 7) return new(angles[first], scale, coverage, ranked, candidates.ToArray(), Array.Empty<LegacyOrientationCandidateCost>());
                candidates = [Find(0), first];
            }
            else
            {
                bool plus = candidates.Any(i => LegacyLineOrientationProjection.Approximately(angles[i], 90));
                bool minus = candidates.Any(i => LegacyLineOrientationProjection.Approximately(angles[i], -90));
                candidates = [];
                if (plus) candidates.Add(Find(90)); if (minus) candidates.Add(Find(-90));
                if (scoringStrokes.Count < 7 && !candidates.Contains(Find(0))) candidates.Add(Find(0));
                if (candidates.Count == 1) return new(angles[candidates[0]], scale, coverage, ranked, candidates.ToArray(), Array.Empty<LegacyOrientationCandidateCost>());
            }
        }
        else if (scoringStrokes.Count < 7 && !candidates.Contains(Find(0))) candidates.Add(Find(0));

        int Find(float degrees)
        {
            int index = Array.FindIndex(angles, a => LegacyLineOrientationProjection.Approximately(a, degrees));
            if (index < 0) throw new ArgumentException("Native policy requires a missing cardinal angle."); return index;
        }
        int best = int.MaxValue; float selected = float.MaxValue; var scores = new List<LegacyOrientationCandidateCost>();
        foreach (int index in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var child = child249(LegacyLineOrientationProjection.Rotate(scoringStrokes, angles[index], cancellationToken), scale, cancellationToken);
            int cost = WeightChild(child, profile.CostCardinalityExponent); scores.Add(new(index, child, cost));
            if (cost < best) { best = cost; selected = angles[index]; }
        }
        // The native routine retains FLT_MAX if all costs are INT_MAX. Do not rotate invalid coordinates.
        if (selected == float.MaxValue) throw new InvalidDataException("Child249 supplied no strict finite orientation winner.");
        return new(selected, scale, coverage, ranked, candidates.ToArray(), scores.ToArray());
    }

    public static int WeightChild(LegacyOrientationChildCost child, float exponent)
    {
        if (!float.IsFinite(exponent) || exponent is < 0 or > 8) throw new ArgumentException("Invalid cardinality exponent.");
        uint quotient = child.ElementCount == 0 ? int.MaxValue : child.CostSum / child.ElementCount;
        float cost = (float)Math.Pow(child.Cardinality, (double)exponent) * unchecked((int)quotient);
        // Exact CVTTSS2SI indefinite result, including NaN and signed32 overflow.
        return !float.IsFinite(cost) || cost >= 2147483648f || cost < -2147483648f ? int.MinValue : (int)cost;
    }

    public static bool RejectOrder(IReadOnlyList<InkStroke> rotatedStrokes, float scale, LegacyLineOrientationProfile profile,
        CancellationToken cancellationToken = default)
    {
        LegacyLineOrientationProjection.Validate(rotatedStrokes, cancellationToken); profile.Validate(); ValidateScale(scale);
        int eligible = 0, forward = 0, above = 0, below = 0; Bounds? previous = null;
        foreach (var stroke in rotatedStrokes)
        {
            var b = Box(stroke.Points);
            if (MathF.Max(b.Width, b.Height) < scale * .5f || b.Height < scale * .35f) continue;
            eligible++; InkPoint center = ArcCenter(stroke.Points);
            if (previous is Bounds p)
            {
                if (center.Y <= p.Top || p.Bottom <= center.Y)
                {
                    if (p.Left < center.X && center.X < p.Right)
                    {
                        float middle = (p.Bottom + p.Top) * .5f;
                        if (center.Y > middle) above++;
                        else if (center.Y < middle) below++;
                    }
                }
                else
                {
                    float middle = (p.Right + p.Left) * .5f;
                    if (profile.ReverseHorizontalOrder ? center.X < middle : center.X > middle) forward++;
                }
            }
            previous = b;
        }
        return eligible > 8 && ((float)forward / eligible < profile.MinimumForwardFraction ||
            (float)above / eligible > profile.MaximumRowJumpFraction || (float)below / eligible > profile.MaximumRowJumpFraction);
    }

    public static float Coverage(IReadOnlyList<InkStroke> rotatedStrokes, float scale, CancellationToken cancellationToken = default)
    {
        LegacyLineOrientationProjection.Validate(rotatedStrokes, cancellationToken); ValidateScale(scale);
        var boxes = rotatedStrokes.Select(s => Box(s.Points)).ToArray(); int first = Array.FindIndex(boxes, b => scale * .5f < b.Width || scale * .5f < b.Height);
        if (first < 0) first = boxes.Length - 1; //native ends on the final tiny bbox, rather than removing all input.
        Bounds current = boxes[first]; float widths = 0, heights = 0;
        for (int i = first + 1; i < boxes.Length; i++)
        {
            var next = boxes[i]; if (next.Width < scale * .5f && next.Height < scale * .5f) continue;
            if (next.Top <= current.Bottom) current = new(MathF.Min(current.Left, next.Left), MathF.Min(current.Top, next.Top), MathF.Max(current.Right, next.Right), MathF.Max(current.Bottom, next.Bottom));
            else { widths += current.Width; heights += current.Height; current = next; }
        }
        heights += current.Height;
        return LegacyLineOrientationProjection.Approximately(heights, 0) ? 0 : (current.Width + widths) / heights;
    }

    private static void ValidateScale(float scale) { if (!float.IsFinite(scale) || scale <= 0 || scale > 1_000_000) throw new ArgumentException("Invalid orientation scale."); }
    private readonly record struct Bounds(float Left, float Top, float Right, float Bottom)
    { public float Width => Right - Left; public float Height => Bottom - Top; }
    private static Bounds Box(IReadOnlyList<InkPoint> points)
    {
        float left = points[0].X, right = left, top = points[0].Y, bottom = top;
        foreach (var p in points) { left = MathF.Min(left, p.X); right = MathF.Max(right, p.X); top = MathF.Min(top, p.Y); bottom = MathF.Max(bottom, p.Y); }
        return new(left, top, right, bottom);
    }
    private static InkPoint ArcCenter(IReadOnlyList<InkPoint> points)
    {
        double arc = 0, x = 0, y = 0;
        for (int i = 1; i < points.Count; i++)
        {
            float dx = points[i].X - points[i - 1].X, dy = points[i].Y - points[i - 1].Y;
            double length = Math.Sqrt((double)dx * dx + (double)dy * dy); arc += length;
            x += (double)(points[i].X + points[i - 1].X) * length; y += (double)(points[i].Y + points[i - 1].Y) * length;
        }
        return arc <= 0 ? points[0] : new((float)(x * .5d / arc), (float)(y * .5d / arc));
    }
}
