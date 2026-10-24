namespace InkRecognition;

public sealed partial class LegacyMathAkSourceGeometry
{
    /// <summary>Prepares the shared directional geometry once, then issues the
    /// selected3F/4F/5F inputs. No network or candidate classifier is executed.</summary>
    public DirectionalFeatureBundle PrepareUnguidedSingleStrokeDirectionalBundle(
        CancellationToken token = default)
    {
        var frame = PrepareDirectionalFrame(token);
        var temporal = PrepareDirectionalTemporalCore(frame, token);
        var spatial = PrepareDirectionalSpatialCore(frame, token);
        var upper = PrepareDirectionalUpperCore(frame, token);
        token.ThrowIfCancellationRequested();
        return new(this, temporalIssuer, temporal, spatial, upper);
    }

    public bool OwnsDirectionalFeatureBundle(DirectionalFeatureBundle value)
        => value is not null && value.MatchesIssuer(this, temporalIssuer)
            && OwnsDirectionalTemporalFeatures(value.Temporal)
            && OwnsDirectionalSpatialFeatures(value.Spatial)
            && OwnsDirectionalUpperFeatures(value.Upper);

    // This call-local owner never escapes. In particular, the returned bundle
    // retains only completed feature arrays, not samples, flags or memberships.
    private sealed class DirectionalFrame
    {
        internal readonly LegacyDirectionalSamples Samples;
        internal readonly LegacyDirectionalLandmarkSet Direction;
        internal readonly LegacyDirectionalIntersectionSet Intersections;
        internal readonly int[] Global;
        internal int Count => Samples.CumulativePhysicalLengths.Length;
        internal DirectionalFrame(LegacyDirectionalSamples samples,
            LegacyDirectionalLandmarkSet direction,
            LegacyDirectionalIntersectionSet intersections, int[] global)
        { Samples = samples; Direction = direction; Intersections = intersections; Global = global; }
    }

    private DirectionalFrame PrepareDirectionalFrame(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (StrokeCount != 1 || PointCount is < 2 or > 4096)
            throw new NotSupportedException("Single whole stroke with2..4096 points required.");
        LegacyInkPoint[] points = normalized[0];
        var first = points[0]; bool hasExtent = false;
        foreach (var point in points)
        { token.ThrowIfCancellationRequested(); if (point != first) hasExtent = true; }
        if (!hasExtent) throw new NotSupportedException("Degenerate2B1 feature branch is outside this bounded subset.");
        // Same00FB90 source projection and B4D10 target100 as the individual
        // producers. Encoded source88=1 omits the coordinate unit.
        var strokes = new[] { new LegacyInkStroke(points) };
        var samples = LegacyDirectionalResampling.Prepare(strokes, 100, false, token);
        int count = samples.CumulativePhysicalLengths.Length;
        if (count is < 1 or > 255)
            throw new NotSupportedException("Actual2B1 borrowed byte-index consumers require1..255 prepared samples.");
        var direction = LegacyDirectional2B1Landmarks.Extract(samples.Strokes, token);
        var intersections = LegacyDirectionalIntersections.Extract(samples, 10, token);
        var global = LegacyDirectionalStatistics.Extract(samples.Strokes, samples.Width, samples.Height,
            samples.PhysicalArcLength, direction.Flags.Span, intersections.Flags.Span, intersections.SelfLoopCount, token);
        token.ThrowIfCancellationRequested();
        return new(samples, direction, intersections, global);
    }

    public sealed class DirectionalFeatureBundle
    {
        private readonly LegacyMathAkSourceGeometry source;
        private readonly object issuer;
        internal DirectionalFeatureBundle(LegacyMathAkSourceGeometry source, object issuer,
            DirectionalTemporalFeatures temporal, DirectionalSpatialFeatures spatial,
            DirectionalUpperFeatures upper)
        { this.source = source; this.issuer = issuer; Temporal = temporal; Spatial = spatial; Upper = upper; }
        internal bool MatchesIssuer(LegacyMathAkSourceGeometry expected, object token)
            => ReferenceEquals(source, expected) && ReferenceEquals(issuer, token);
        public LegacyMathAkSourceGeometry Source => source;
        public DirectionalTemporalFeatures Temporal { get; }
        public DirectionalSpatialFeatures Spatial { get; }
        public DirectionalUpperFeatures Upper { get; }
        public bool HasCandidateAuthority => false;
        public bool HasClassifierOutput => false;
    }
}
