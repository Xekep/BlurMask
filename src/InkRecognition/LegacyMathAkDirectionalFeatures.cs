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
        => PrepareDirectionalTemporalCore(PrepareDirectionalFrame(token), token);

    private DirectionalTemporalFeatures PrepareDirectionalTemporalCore(DirectionalFrame frame, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var samples = frame.Samples; var direction = frame.Direction;
        var intersections = frame.Intersections; var global = frame.Global;
        int count = frame.Count;
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
