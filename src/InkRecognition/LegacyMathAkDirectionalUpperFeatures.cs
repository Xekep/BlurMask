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
        => PrepareDirectionalUpperCore(PrepareDirectionalFrame(token), token);

    private DirectionalUpperFeatures PrepareDirectionalUpperCore(DirectionalFrame frame, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var samples = frame.Samples; var direction = frame.Direction;
        var intersections = frame.Intersections; var global = frame.Global;
        int count = frame.Count;
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
