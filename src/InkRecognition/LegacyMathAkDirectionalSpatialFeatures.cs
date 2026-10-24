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
        => PrepareDirectionalSpatialCore(PrepareDirectionalFrame(token), token);

    private DirectionalSpatialFeatures PrepareDirectionalSpatialCore(DirectionalFrame frame, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var samples = frame.Samples; var direction = frame.Direction;
        var intersections = frame.Intersections; var global = frame.Global;
        int count = frame.Count;
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
