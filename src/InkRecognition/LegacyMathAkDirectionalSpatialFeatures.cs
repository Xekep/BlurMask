namespace InkRecognition;

public sealed partial class LegacyMathAkSourceGeometry
{
    public bool OwnsDirectionalSpatialFeatures(DirectionalSpatialFeatures value)
        => value is not null && value.MatchesIssuer(this, temporalIssuer, profile);

    /// <summary>Actual selected4F279 A111 coordinate-quantile spatial family for one whole physical
    /// stroke without a guide. Uses source-owned normalized points and shared
    /// profile calibration. This issues input features, never predictions/trees.</summary>
    public DirectionalSpatialFeatures PrepareUnguidedSingleStrokeDirectionalSpatial(
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
        //A111 bit12=0/bit13=1 selects BCDB0 coordinate quantiles, not BD280.
        //BFF40 emits only the primary4x4 family; prepared upper5x5 is unread.
        var membership = LegacySpatialWindows.Partition(samples.Strokes[0].Points,
            samples.Width, samples.Height, 4, 4, true, token);
        var windows = new int[16][];
        for (int i = 0; i < windows.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            var indices = new byte[membership[i].Length];
            for (int j = 0; j < indices.Length; j++)
                indices[j] = checked((byte)membership[i].Span[j]);
            windows[i] = LegacySpatialWindowStatistics.Extract(samples, indices,
                direction.Flags.Span, intersections.Flags.Span, false, token);
        }
        short[] values = profile.ComposeUnguidedDirectionalSpatial(global, windows, token);
        token.ThrowIfCancellationRequested();
        return new(this, temporalIssuer, profile, values, count);
    }
    public sealed class DirectionalSpatialFeatures
    {
        private readonly LegacyMathAkSourceGeometry source;
        private readonly object issuer;
        private readonly LegacyMathAkFeatureProfile profile;
        private readonly short[] values;
        internal DirectionalSpatialFeatures(LegacyMathAkSourceGeometry source, object issuer,
            LegacyMathAkFeatureProfile profile, short[] values, int count)
        { this.source = source; this.issuer = issuer; this.profile = profile; this.values = values; PreparedPointCount = count; }
        internal bool MatchesIssuer(LegacyMathAkSourceGeometry expected, object token, LegacyMathAkFeatureProfile expectedProfile)
            => ReferenceEquals(source, expected) && ReferenceEquals(issuer, token) && ReferenceEquals(profile, expectedProfile);
        public LegacyMathAkSourceGeometry Source => source;
        public int ModelStart => 1_459_255;
        public int FeatureCount => 279;
        public int PreparedPointCount { get; }
        public bool HasClassifierOutput => false;
        public bool HasCandidateAuthority => false;
        public short[] CopyValues() => (short[])values.Clone();
    }
}
