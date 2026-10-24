namespace InkRecognition;

public sealed partial class LegacyMathAkSourceGeometry
{
    public bool OwnsDirectionalUpperFeatures(DirectionalUpperFeatures value)
        => value is not null && value.MatchesIssuer(this, temporalIssuer, profile);

    /// <summary>Actual selected5F223 A111 upper directed-segment family for one whole physical
    /// stroke without a guide. Uses source-owned normalized points and shared
    /// profile calibration. This issues input features, never predictions/trees.</summary>
    public DirectionalUpperFeatures PrepareUnguidedSingleStrokeDirectionalUpper(
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (StrokeCount != 1 || PointCount is < 2 or > 4096)
            throw new NotSupportedException("Single whole stroke with2..4096 points required.");
        LegacyInkPoint[] points = normalized[0];
        var first = points[0]; bool hasExtent = false;
        foreach (var point in points)
        { token.ThrowIfCancellationRequested(); if (point != first) hasExtent = true; }
        if (!hasExtent) throw new NotSupportedException("Degenerate2B1 feature branch is outside this bounded subset.");
        //00FB90 has already projected the genuine source. B4D10 then prepares
        //directional coordinates/resampling100; encoded source88=1 omits unit.
        var strokes = new[] { new LegacyInkStroke(points) };
        var samples = LegacyDirectionalResampling.Prepare(strokes, 100, false, token);
        int count = samples.CumulativePhysicalLengths.Length;
        if (count is < 1 or > 255)
            throw new NotSupportedException("Actual2B1 borrowed byte-index consumers require1..255 prepared samples.");
        var direction = LegacyDirectional2B1Landmarks.Extract(samples.Strokes, token);
        var intersections = LegacyDirectionalIntersections.Extract(samples, 10, token);
        var global = LegacyDirectionalStatistics.Extract(samples.Strokes, samples.Width, samples.Height,
            samples.PhysicalArcLength, direction.Flags.Span, intersections.Flags.Span, intersections.SelfLoopCount, token);
        //A111 bit14=0/bit15=1 selects upper BCDB0 coordinate quantiles5x5.
        var memberships = LegacySpatialWindows.Partition(samples.Strokes[0].Points,
            samples.Width, samples.Height, 5, 5, true, token);
        var cells = new int[25][];
        for (int i = 0; i < cells.Length; i++)
        {
            token.ThrowIfCancellationRequested(); cells[i] = new int[8];
            AccumulateUpperDirections(samples.Strokes[0].Points, memberships[i].Span,
                direction.Flags.Span, cells[i], token);
        }
        short[] values = profile.ComposeUnguidedDirectionalUpper(global, cells, samples.PhysicalArcLength, token);
        token.ThrowIfCancellationRequested();
        return new(this, temporalIssuer, profile, values, count);
    }
    //BDC10: membership assigns the START of an outgoing segment; next point need not be in the cell.
    private static void AccumulateUpperDirections(ReadOnlySpan<LegacyInkPoint> points,
        ReadOnlySpan<int> members, ReadOnlySpan<byte> flags, Span<int> histogram, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        foreach (int i in members)
        {
            token.ThrowIfCancellationRequested();
            if (i >= points.Length - 1 || (flags[i] >> 6) is 2 or 3) continue;
            int dx = points[i + 1].X - points[i].X, dy = points[i].Y - points[i + 1].Y;
            int x = Math.Abs(dx), y = Math.Abs(dy);
            int cardinal = x >= y ? (dx >= 0 ? 0 : 4) : (dy > 0 ? 2 : 6);
            int diagonal = dy > 0 ? (dx >= 0 ? 1 : 3) : (dx >= 0 ? 7 : 5);
            histogram[cardinal] += Math.Abs(x - y);
            histogram[diagonal] += Math.Min(x, y) * 1448 >> 10;
        }
    }
    public sealed class DirectionalUpperFeatures
    {
        private readonly LegacyMathAkSourceGeometry source;
        private readonly object issuer;
        private readonly LegacyMathAkFeatureProfile profile;
        private readonly short[] values;
        internal DirectionalUpperFeatures(LegacyMathAkSourceGeometry source, object issuer,
            LegacyMathAkFeatureProfile profile, short[] values, int count)
        { this.source = source; this.issuer = issuer; this.profile = profile; this.values = values; PreparedPointCount = count; }
        internal bool MatchesIssuer(LegacyMathAkSourceGeometry expected, object token, LegacyMathAkFeatureProfile expectedProfile)
            => ReferenceEquals(source, expected) && ReferenceEquals(issuer, token) && ReferenceEquals(profile, expectedProfile);
        public LegacyMathAkSourceGeometry Source => source;
        public int ModelStart => 1_629_111;
        public int FeatureCount => 223;
        public int PreparedPointCount { get; }
        public bool HasClassifierOutput => false;
        public bool HasCandidateAuthority => false;
        public short[] CopyValues() => (short[])values.Clone();
    }
}
