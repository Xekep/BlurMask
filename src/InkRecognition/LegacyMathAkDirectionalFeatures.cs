namespace InkRecognition;

public sealed partial class LegacyMathAkSourceGeometry
{
    public bool OwnsDirectionalTemporalFeatures(DirectionalTemporalFeatures value)
        => value is not null && value.MatchesIssuer(this, temporalIssuer, profile);

    /// <summary>Actual selected3F279 A111 temporal family for one whole physical
    /// stroke without a guide. Uses source-owned normalized points and shared
    /// profile calibration. This issues input features, never predictions/trees.</summary>
    public DirectionalTemporalFeatures PrepareUnguidedSingleStrokeDirectionalTemporal(
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
        var membership = LegacyArcLengthWindows.Partition(samples.CumulativePhysicalLengths.Span, 16, token);
        var windows = new int[16][];
        for (int i = 0; i < windows.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            windows[i] = LegacyDirectionalWindowStatistics.Extract(samples, membership[i].Span,
                direction.Flags.Span, intersections.Flags.Span, false, token);
        }
        short[] values = profile.ComposeUnguidedDirectionalTemporal(global, windows, token);
        token.ThrowIfCancellationRequested();
        return new(this, temporalIssuer, profile, values, count);
    }
    public sealed class DirectionalTemporalFeatures
    {
        private readonly LegacyMathAkSourceGeometry source;
        private readonly object issuer;
        private readonly LegacyMathAkFeatureProfile profile;
        private readonly short[] values;
        internal DirectionalTemporalFeatures(LegacyMathAkSourceGeometry source, object issuer,
            LegacyMathAkFeatureProfile profile, short[] values, int count)
        { this.source = source; this.issuer = issuer; this.profile = profile; this.values = values; PreparedPointCount = count; }
        internal bool MatchesIssuer(LegacyMathAkSourceGeometry expected, object token, LegacyMathAkFeatureProfile expectedProfile)
            => ReferenceEquals(source, expected) && ReferenceEquals(issuer, token) && ReferenceEquals(profile, expectedProfile);
        public LegacyMathAkSourceGeometry Source => source;
        public int ModelStart => 1_289_175;
        public int FeatureCount => 279;
        public int PreparedPointCount { get; }
        public bool HasClassifierOutput => false;
        public bool HasCandidateAuthority => false;
        public short[] CopyValues() => (short[])values.Clone();
    }
}
