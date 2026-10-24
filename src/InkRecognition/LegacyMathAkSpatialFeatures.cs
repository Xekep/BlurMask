namespace InkRecognition;

public sealed partial class LegacyMathAkSourceGeometry
{
    //Same private source feature cohort token; typed feature classes remain disjoint.
    public bool OwnsSpatialFeatures(SpatialFeatures value)
        => value is not null && value.MatchesIssuer(this, temporalIssuer);

    /// <summary>Actual configured legacy1211 whole-source spatial202+boundary2
    /// projection for one stroke and an absent guide. No model is evaluated.</summary>
    public SpatialFeatures PrepareUnguidedSingleStrokeSpatial(CancellationToken token = default)
    {
        //Same source and same native210..219 overlap as1F, not caller features.
        short[] temporal = PrepareUnguidedSingleStrokeTemporal(token).CopyValues();
        LegacyInkPoint[] input = normalized[0];
        int minX = input.Min(p => (int)p.X), maxX = input.Max(p => (int)p.X);
        int minY = input.Min(p => (int)p.Y), maxY = input.Max(p => (int)p.Y);
        int width = maxX - minX, height = maxY - minY;
        int factor = 127 * 1024 / Math.Max(width, height);
        int sw = width * factor >> 10, sh = height * factor >> 10;
        int xp = sw < sh ? (127 - sw) >> 1 : 0;
        int yp = sh < sw ? (127 - sh) >> 1 : 0;
        var x = new int[input.Length]; var y = new int[input.Length];
        for (int i = 0; i < input.Length; i++)
        { x[i] = ((input[i].X - minX) * factor >> 10) + xp; y[i] = ((input[i].Y - minY) * factor >> 10) + yp; }
        const int side = 128, border = 130, maximumPixels = 16384;
        var bitmap = new bool[border * border];
        var pixels = new LegacyInkPoint[maximumPixels]; int count = 0;
        void Add(int px, int py)
        {
            if ((uint)px >= side || (uint)py >= side) throw new InvalidDataException("Raster point outside actual128 grid.");
            int at = px + 1 + (py + 1) * border;
            if (bitmap[at]) return;
            if (count == maximumPixels) throw new InvalidDataException("Whole raster bound exceeded.");
            if ((count & 255) == 0) token.ThrowIfCancellationRequested();
            bitmap[at] = true; pixels[count++] = new((short)px, (short)py);
        }
        //030400: native fixed tie rule/error>=0 diagonal. Original source grid
        //points are used, not the30 temporal resampling points.
        for (int edge = 0; edge < input.Length - 1; edge++)
        {
            if ((edge & 15) == 0) token.ThrowIfCancellationRequested();
            int ax = x[edge], ay = y[edge], bx = x[edge + 1], by = y[edge + 1];
            if (ax > bx) { (ax, bx) = (bx, ax); (ay, by) = (by, ay); }
            int dx = bx - ax, dy = by - ay, sign = dy < 0 ? -1 : 1;
            int vertical = Math.Abs(dy), major = Math.Max(dx, vertical), minor = Math.Min(dx, vertical);
            int error = 2 * minor - major;
            bool steep = vertical > dx;
            for (int i = 0; i <= major; i++)
            {
                Add(ax, ay);
                if (error < 0) { if (steep) ay += sign; else ax++; error += 2 * minor; }
                else { ax++; ay += sign; error += 2 * (minor - major); }
            }
        }
        if (count == 0) throw new InvalidDataException("Empty actual raster.");
        var hc = new int[side]; var vc = new int[side];
        var pc = new int[255]; var mc = new int[255];
        var hmin = Enumerable.Repeat(side, side).ToArray(); var vmin = (int[])hmin.Clone();
        var hmax = (int[])hmin.Clone(); var vmax = (int[])hmin.Clone();
        int[] DiagonalInitial() => Enumerable.Range(0,255).Select(i => i < side ? i + 1 : 255 - i).ToArray();
        var pmin = DiagonalInitial(); var pmax = DiagonalInitial();
        var mmin = DiagonalInitial(); var mmax = DiagonalInitial();
        static void Inc(int[] a, int i) => a[i] = unchecked((short)(a[i] + 1));
        static void Min(int[] a, int i, int v) { if (v < a[i]) a[i] = v; }
        //030FE0 literal8-neighbor crossing/moment policy. A diagonal connection
        //can create a crossing in the OTHER orientation at its adjacent index.
        for (int i = 0; i < count; i++)
        {
            if ((i & 255) == 0) token.ThrowIfCancellationRequested();
            int px = pixels[i].X, py = pixels[i].Y, at = px + 1 + (py + 1) * border;
            bool left = bitmap[at - 1], downLeft = bitmap[at + border - 1];
            bool down = bitmap[at + border], downRight = bitmap[at + border + 1];
            bool right = bitmap[at + 1], up = bitmap[at - border];
            if (!right) Inc(hc, py);
            if (!up) Inc(vc, px);
            if (!downLeft) Inc(pc, px + py);
            else if (!down && !left)
            {
                int index = px - py + 126;
                Inc(mc, index); Min(mmin, index, Math.Min(py, px - 1));
                Min(mmax, index, py + 1 < px ? 127 - px : 126 - py);
            }
            if (!downRight) Inc(mc, px - py + 127);
            else if (!down && !right)
            {
                int index = px + py + 1;
                Inc(pc, index); Min(pmin, index, index >= side ? 126 - px : py);
                Min(pmax, index, index >= side ? 126 - py : px);
            }
            Min(hmin, py, px); Min(vmin, px, py);
            Min(hmax, py, 127 - px); Min(vmax, px, 127 - py);
            int plus = px + py, minus = px - py + 127;
            Min(pmin, plus, plus >= side ? 127 - px : py);
            Min(pmax, plus, plus >= side ? 127 - py : px);
            Min(mmin, minus, Math.Min(px, py)); Min(mmax, minus, 127 - Math.Max(px, py));
        }
        var spatial = new short[192];
        static int SumShort(int[] a, int start, int length)
        { int v = 0; for (int i = 0; i < length; i++) v = unchecked((short)(v + a[start + i])); return v; }

        static short DistanceValue(int sum) => unchecked((short)(sum * 2 - 1024));
        static short DiagonalDistance(int[] a, int start)
        {
            int sum = 0;
            for (int i = 0; i < 16; i++)
            {
                int index = start + i, denominator = index < 128 ? index + 1 : 255 - index;
                sum = unchecked((short)(sum + (((ushort)a[index] << 8) / denominator)));
            }
            return unchecked((short)((unchecked((uint)(sum * 128)) >> 8) - 1024));
        }
        //0315F0:12 groups of16. Both diagonal halves intentionally share the
        //central index127; it is not a conventional255->256 interpolation.
        for (int bin = 0; bin < 16; bin++)
        {
            int at = bin * 8, diag = bin < 8 ? bin * 16 : 127 + (bin - 8) * 16;
            spatial[bin] = SpatialCountProjection(SumShort(hc, at, 8));
            spatial[16 + bin] = SpatialCountProjection(SumShort(pc, diag, 16));
            spatial[32 + bin] = SpatialCountProjection(SumShort(vc, at, 8));
            spatial[48 + bin] = SpatialCountProjection(SumShort(mc, diag, 16));
            spatial[64 + bin] = DistanceValue(SumShort(hmin, at, 8));
            spatial[80 + bin] = DiagonalDistance(pmin, diag);
            spatial[96 + bin] = DistanceValue(SumShort(vmin, at, 8));
            spatial[112 + bin] = DiagonalDistance(mmin, diag);
            spatial[128 + bin] = DistanceValue(SumShort(hmax, at, 8));
            spatial[144 + bin] = DiagonalDistance(pmax, diag);
            spatial[160 + bin] = DistanceValue(SumShort(vmax, at, 8));
            spatial[176 + bin] = DiagonalDistance(mmax, diag);
        }
        var values = new short[204];
        Array.Copy(temporal, 210, values, 0, 10);
        for (int i = 0; i < spatial.Length; i++)
        {
            int minimum = profile.MinimumAt(0, 220 + i), range = profile.MaximumAt(0, 220 + i) - minimum;
            values[10 + i] = range == 0 ? spatial[i] : unchecked((short)(((spatial[i] - minimum) << 11) / range - 1024));
        }
        values[202] = 1024; values[203] = 1024;
        token.ThrowIfCancellationRequested(); return new(this, temporalIssuer, values, count);
    }
    private static short SpatialCountProjection(int sum)
        => (short)Math.Clamp((int)unchecked((short)(sum * 64 - 1024)), -1024, 1024);
    public sealed class SpatialFeatures
    {
        private readonly LegacyMathAkSourceGeometry source;
        private readonly object issuer;
        private readonly short[] values;
        internal SpatialFeatures(LegacyMathAkSourceGeometry source, object issuer, short[] values, int count)
        { this.source = source; this.issuer = issuer; this.values = values; RasterPixelCount = count; }
        internal bool MatchesIssuer(LegacyMathAkSourceGeometry expected, object token)
            => ReferenceEquals(source, expected) && ReferenceEquals(issuer, token);
        public LegacyMathAkSourceGeometry Source => source;
        public int FeatureCount => 204;
        public int ModelStart => 1_151_097;
        public int RasterPixelCount { get; }
        public bool HasCandidateAuthority => false;
        public bool HasClassifierOutput => false;
        public short[] CopyValues() => (short[])values.Clone();
    }
}
