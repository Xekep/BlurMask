namespace InkRecognition;

public sealed partial class LegacyMathAkSourceGeometry
{
    private readonly object temporalIssuer = new();
    public bool OwnsTemporalFeatures(TemporalFeatures value)
        => value is not null && value.MatchesIssuer(this, temporalIssuer);
    /// <summary>
    /// Actual base-tail1F temporal220+boundary2 feature projection for ONE whole
    /// stroke without a baseline guide. Unsupported degenerate/multistroke paths
    /// refuse. This produces neural inputs, never predictions or candidate trees.
    /// </summary>
    public TemporalFeatures PrepareUnguidedSingleStrokeTemporal(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (StrokeCount != 1 || PointCount is < 2 or > 4096)
            throw new NotSupportedException("Single whole stroke with2..4096 points required.");
        const int count = 30, unit = 1024;
        LegacyInkPoint[] points = normalized[0];
        int minX = points.Min(p => (int)p.X), maxX = points.Max(p => (int)p.X);
        int minY = points.Min(p => (int)p.Y), maxY = points.Max(p => (int)p.Y);
        int width = maxX - minX, height = maxY - minY, extent = Math.Max(width, height);
        if (extent == 0) throw new NotSupportedException("Degenerate0322A0 branch is not implemented.");
        //02ED20 actual16x8 geometry normalization, integer fixed-point factor.
        int factor = (128 * 1024 - 1024) / extent;
        int scaledWidth = width * factor >> 10, scaledHeight = height * factor >> 10;
        int pad = scaledWidth > scaledHeight ? (127 - scaledHeight) >> 1
            : scaledWidth < scaledHeight ? (127 - scaledWidth) >> 1 : 0;
        int xPad = scaledWidth < scaledHeight ? pad : 0, yPad = scaledHeight < scaledWidth ? pad : 0;
        var x = new int[points.Length]; var y = new int[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            if ((i & 255) == 0) token.ThrowIfCancellationRequested();
            x[i] = ((points[i].X - minX) * factor >> 10) + xPad;
            y[i] = ((points[i].Y - minY) * factor >> 10) + yPad;
        }
        var edge = new int[points.Length - 1]; int total = 0;
        for (int i = 0; i < edge.Length; i++)
        {
            int dx = (x[i + 1] - x[i]) << 4, dy = (y[i + 1] - y[i]) << 4;
            edge[i] = NativeNorm(unchecked((uint)(dx * dx + dy * dy))); total += edge[i];
        }
        if (total == 0) throw new NotSupportedException("Collapsed resampling path is unsupported.");
        int step = total / (count - 1);
        if (step == 0) throw new NotSupportedException("Zero resampling interval is unsupported.");
        var sx = new int[count]; var sy = new int[count]; sx[0] = x[0]; sy[0] = y[0];
        //Literal single-stroke02F3D0 carry/interpolation chronology. In particular,
        //the repeated-in-edge division uses the native accumulated distance.
        int made = 0, edgeIndex = 0, pointIndex = 0, carry = 0;
        int before = 0, savedPoint = 0, savedEdge = 0, output = 1;
        while (made < count - 1)
        {
            token.ThrowIfCancellationRequested(); int accumulated = carry;
            if (accumulated < step)
            {
                do
                {
                    int oldCarry = carry; accumulated = oldCarry;
                    if (pointIndex >= points.Length - 1) break;
                    pointIndex++; accumulated = oldCarry + edge[edgeIndex]; carry = accumulated;
                    edgeIndex++; before = oldCarry;
                } while (accumulated < step);
                savedPoint = pointIndex; savedEdge = edgeIndex;
            }
            if (edgeIndex == 0 || edge[edgeIndex - 1] == 0)
                throw new NotSupportedException("Native interpolation denominator is outside the supported subset.");
            made++; int at = edgeIndex - 1, dx = x[edgeIndex] - x[at], dy = y[edgeIndex] - y[at];
            sx[output] = (step - before) * dx / edge[at] + x[at];
            sy[output++] = dy * (step - before) / edge[at] + y[at];
            carry = edge[at] - step + before; int multiplier = 2;
            if (step <= carry)
            {
                do
                {
                    if (made >= count - 1) break;
                    carry -= step; made++;
                    sx[output] = dx * multiplier * step / accumulated + x[at];
                    sy[output++] = dy * multiplier * step / accumulated + y[at]; multiplier++;
                } while (step <= carry);
                pointIndex = savedPoint; edgeIndex = savedEdge;
            }
        }
        if (output != count) throw new InvalidDataException("Native temporal sample count differs.");
        var ddx = new int[count - 1]; var ddy = new int[count - 1]; var length = new int[count - 1];
        int sampledLength = 0;
        for (int i = 0; i < count - 1; i++)
        {
            ddx[i] = (sx[i + 1] - sx[i]) << 4; ddy[i] = (sy[i + 1] - sy[i]) << 4;
            length[i] = NativeNorm(unchecked((uint)(ddx[i] * ddx[i] + ddy[i] * ddy[i]))); sampledLength += length[i];
        }
        int posX = 0, negX = 0, posY = 0, negY = 0, posTurn = 0, negTurn = 0;
        static void Add(ref int positive, ref int negative, int sign, int weight)
        { if (sign > 0) positive += weight; else if (sign < 0) negative += weight; else { positive += weight >> 1; negative += weight >> 1; } }
        for (int i = 0; i < count - 1; i++)
        {
            Add(ref posX, ref negX, ddx[i], length[i]); Add(ref posY, ref negY, ddy[i], length[i]);
            if (i + 1 < count - 1)
                Add(ref posTurn, ref negTurn, (ddx[i] * ddy[i + 1] - ddy[i] * ddx[i + 1]) >> 8, length[i] + length[i + 1]);
        }
        var features = new short[222];
        void Pair(int at, int positive, int negative)
        {
            int sum = positive + negative;
            features[at] = sum == 0 ? (short)-unit : unchecked((short)(unit * positive * 2 / sum - unit));
            features[at + 1] = sum == 0 ? (short)-unit : unchecked((short)(unit * negative * 2 / sum - unit));
        }
        Pair(0, posX, negX); Pair(2, posY, negY); Pair(4, posTurn, negTurn);
        for (int i = 0; i < count; i++)
        {
            features[6 + i] = unit;
            features[36 + i] = unchecked((short)(sx[i] * 16 - unit));
            features[66 + i] = unchecked((short)(sy[i] * 16 - unit));
            if (i < count - 1)
            {
                features[96 + i] = length[i] == 0 ? (short)0 : unchecked((short)(unit * ddx[i] / length[i]));
                features[125 + i] = length[i] == 0 ? (short)0 : unchecked((short)(unit * ddy[i] / length[i]));
            }
        }
        for (int i = 0; i < count - 2; i++)
        {
            features[154 + i] = unchecked((short)((features[97 + i] * features[96 + i] + features[126 + i] * features[125 + i]) >> 10));
            features[182 + i] = unchecked((short)((features[96 + i] * features[126 + i] - features[97 + i] * features[125 + i]) >> 10));
        }
        //030020 actual3F global mask: all six outputs plus its leading unit.
        int sumX = sx.Sum(), sumY = sy.Sum();
        features[210] = unit;
        features[211] = unchecked((short)Math.Min(unit, 256 * (scaledHeight + 1) / (scaledWidth + 1) - unit));
        features[212] = unchecked((short)(16 * sumX / count - unit));
        features[213] = unchecked((short)(16 * sumY / count - unit));
        features[214] = -768;
        features[215] = unchecked((short)Math.Clamp((2 * sampledLength >> 4) - unit, -unit, unit));
        features[216] = sampledLength == 0 ? (short)-unit : (short)unit;
        //0327A0 calibrates only the temporal/global prefix here. The following
        //three baseline outputs stay zero for the explicit absent-guide branch.
        for (int i = 0; i < 217; i++)
        {
            int range = calibrationMaximum[i] - calibrationMinimum[i];
            if (range != 0) features[i] = unchecked((short)(((features[i] - calibrationMinimum[i]) << 11) / range - unit));
        }
        //010EA0 actual full-source start/end fields are+1f;212.V90 quantizer10.
        features[220] = unit; features[221] = unit;
        token.ThrowIfCancellationRequested();
        return new(this, temporalIssuer, features, sx, sy);
    }
    private static int NativeNorm(uint square)
    {
        //05EAC0. Its complete16384B table equals round(sqrt(i)) for ALL i.
        int shift = 0;
        if (square >= 16384)
        {
            shift = 1;
            do { shift++; } while ((square >> (2 * shift)) >= 16384);
        }
        return (int)(Math.Sqrt(square >> (shift * 2)) + .5) << shift;
    }
    public sealed class TemporalFeatures
    {
        private readonly LegacyMathAkSourceGeometry source;
        private readonly object issuer;
        private readonly short[] features;
        private readonly int[] x, y;
        internal TemporalFeatures(LegacyMathAkSourceGeometry source, object issuer, short[] features, int[] x, int[] y)
        { this.source = source; this.issuer = issuer; this.features = features; this.x = x; this.y = y; }
        internal bool MatchesIssuer(LegacyMathAkSourceGeometry expected, object token)
            => ReferenceEquals(source, expected) && ReferenceEquals(issuer, token);
        public LegacyMathAkSourceGeometry Source => source;
        public int ModelStart => 1_007_739;
        public int FeatureCount => 222;
        public bool HasCandidateAuthority => false;
        public bool HasClassifierOutput => false;
        public short[] CopyValues() => (short[])features.Clone();
        public int[][] CopyTemporalPoints() => x.Select((v, i) => new[] {v, y[i]}).ToArray();
    }
}
