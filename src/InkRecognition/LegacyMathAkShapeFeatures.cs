using System.Numerics;

namespace InkRecognition;

public sealed partial class LegacyMathAkSourceGeometry
{
    /// <summary>Privately issued actual299 window0 features. It is geometry,
    /// not a label prediction, candidate tree or caller-prepared feature issuer.</summary>
    public sealed class ShapeFeatures
    {
        private readonly LegacyMathAkSourceGeometry source;
        private readonly LegacyMathAkShapePrototypes prototypes;
        private readonly short[] values;
        private readonly object issuer;
        internal ShapeFeatures(LegacyMathAkSourceGeometry source, object issuer, short[] values, int samples, int groups)
        { this.source = source; this.issuer = issuer; prototypes = source.profile.ShapePrototypes;
          this.values = values; ResampledPointCount = samples; ResampledStrokeCount = groups; }
        public int ResampledPointCount { get; }
        public int ResampledStrokeCount { get; }
        public int Rows => 30;
        public int Channels => 7;
        public bool HasClassifierOutput => false;
        public short[] CopyValues(CancellationToken token = default)
        { token.ThrowIfCancellationRequested(); var copy = (short[])values.Clone(); token.ThrowIfCancellationRequested(); return copy; }
        /// <summary>Diagnostic distances to this source's stored alternatives;
        /// this does not select or admit a classifier result.</summary>
        public float[] MeasurePrototypeDistances(int label, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var range = prototypes.Alternatives(label); var result = new float[range.End - range.Start];
            for (int i = 0; i < result.Length; i++) result[i] = prototypes.Measure(range.Start + i, values, token);
            token.ThrowIfCancellationRequested(); return result;
        }
        internal bool OwnedBy(LegacyMathAkSourceGeometry expected)
            => ReferenceEquals(source, expected) && ReferenceEquals(issuer, expected.temporalIssuer) && ReferenceEquals(prototypes, expected.profile.ShapePrototypes);
    }
    public bool OwnsShapeFeatures(ShapeFeatures? features) => features is not null && features.OwnedBy(this);

    /// <summary>Native3BCE0/3C2E0 selected30x7 source from this same private
    /// normalized XY group. Stored prototype/calibration ownership is shared.
    /// More than30 resampled points refuses before fixed-row writes; the native
    /// later feature loops lack that safe bound. Nonzero-window299 is excluded.</summary>
    public ShapeFeatures PrepareShapeFeatures(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var sampled = new Vector2[30]; var groupEnds = new int[30]; int count = 0, groups = 0;
        void BeginGroup()
        {
            token.ThrowIfCancellationRequested();
            if (groups == 30) throw new NotSupportedException("Selected shape resampled group bound exceeded.");
            groupEnds[groups++] = count;
        }
        void Append(float x, float y)
        {
            token.ThrowIfCancellationRequested();
            if (count == 30) throw new NotSupportedException("Selected shape resampling exceeds30 rows.");
            if (!float.IsFinite(x) || !float.IsFinite(y) || groups == 0)
                throw new NotSupportedException("Invalid selected shape resampling state.");
            sampled[count++] = new(x, y); groupEnds[groups - 1] = count;
        }
        static float Length(LegacyInkPoint first, LegacyInkPoint second)
        {
            float dx = second.X - first.X, dy = second.Y - first.Y;
            float xx = dx * dx, yy = dy * dy;
            float square = yy + xx;
            return (float)Math.Sqrt((double)square);
        }
        float total = 0;
        foreach (var row in normalized)
            for (int i = 1; i < row.Length; i++)
            { token.ThrowIfCancellationRequested(); total += Length(row[i - 1], row[i]); }
        float step = total / 29f;
        if (step == 0f)
        {
            BeginGroup(); LegacyInkPoint first = normalized[0][0];
            for (int i = 0; i < 30; i++) Append(first.X, first.Y);
        }
        else
        {
            float carry = 0;
            for (int s = 0; s < normalized.Length; s++)
            {
                var row = normalized[s]; bool pendingGroup = s != 0;
                if (s == 0) { BeginGroup(); Append(row[0].X, row[0].Y); }
                for (int i = 1; i < row.Length; i++)
                {
                    token.ThrowIfCancellationRequested();
                    float nextCarry = carry + Length(row[i - 1], row[i]);
                    if (step <= nextCarry)
                    {
                        float segment = nextCarry - carry;
                        float tolerance = Math.Max(Math.Abs(segment) * 2.3841858e-7f, 1.1920929e-7f);
                        if (Math.Abs(segment) <= tolerance)
                        {
                            if (pendingGroup) { BeginGroup(); pendingGroup = false; }
                            Append(row[i].X, row[i].Y); nextCarry = segment;
                        }
                        else
                        {
                            float fraction = (step - carry) / segment;
                            nextCarry = carry + (segment - step);
                            while (true)
                            {
                                token.ThrowIfCancellationRequested();
                                if (pendingGroup) { BeginGroup(); pendingGroup = false; }
                                float inverse = 1f - fraction;
                                float x0 = row[i - 1].X * inverse, x1 = row[i].X * fraction;
                                float y0 = row[i - 1].Y * inverse, y1 = row[i].Y * fraction;
                                Append(x0 + x1, y0 + y1);
                                float nextFraction = fraction + step / segment;
                                if (1f <= nextFraction) break;
                                if (!(fraction < nextFraction)) throw new NotSupportedException("Shape resampling made no progress.");
                                fraction = nextFraction; nextCarry -= step;
                            }
                        }
                    }
                    carry = nextCarry;
                }
            }
            //Literal endpoint append into the existing final emitted group,
            //even when the final original stroke emitted no separate sample.
            if (0.5f < carry / step)
            { var last = normalized[^1][^1]; Append(last.X, last.Y); }
        }
        if (count == 0) throw new NotSupportedException("Empty selected shape geometry.");
        float minX = sampled[0].X, maxX = minX, minY = sampled[0].Y, maxY = minY;
        for (int i = 1; i < count; i++)
        {
            Vector2 point = sampled[i];
            if (maxX <= point.X) maxX = point.X; if (point.X < minX) minX = point.X;
            if (maxY <= point.Y) maxY = point.Y; if (point.Y < minY) minY = point.Y;
        }
        float width = maxX - minX, height = maxY - minY;
        float extent = Math.Max(Math.Max(width, height), 1f);
        float offsetX = width < height ? ((height - maxX) + minX) * 0.5f : 0f;
        float offsetY = height < width ? ((width - maxY) + minY) * 0.5f : 0f;
        var output = new short[210];
        static short Round(float x)
        {
            float rounded = x <= 0f ? x - 0.5f : x + 0.5f;
            if (!float.IsFinite(rounded) || rounded < short.MinValue || rounded >= 32768f)
                throw new NotSupportedException("Selected shape channel outside signed-short subset.");
            return (short)(int)rounded;
        }
        int begin = 0;
        for (int g = 0; g < groups; g++)
        {
            int end = groupEnds[g];
            for (int i = begin; i < end; i++)
            {
                token.ThrowIfCancellationRequested();
                output[i * 7] = i == begin ? (short)8192 : (short)-8192;
                float x = ((sampled[i].X - minX) + offsetX) * 8192f;
                float y = ((sampled[i].Y - minY) + offsetY) * 8192f;
                output[i * 7 + 1] = Round(x / extent); output[i * 7 + 2] = Round(y / extent);
            }
            begin = end;
        }
        begin = 0;
        for (int g = 0; g < groups; g++)
        {
            int end = groupEnds[g];
            for (int i = begin; i < end; i++)
            {
                token.ThrowIfCancellationRequested();
                int before = i == begin ? i : i - 1, after = i == end - 1 ? i : i + 1;
                int dx = output[after * 7 + 1] - output[before * 7 + 1];
                int dy = output[after * 7 + 2] - output[before * 7 + 2];
                float norm = (float)Math.Sqrt((double)(dx * dx + dy * dy));
                float tolerance = Math.Max(Math.Abs(norm) * 2.3841858e-7f, 1.1920929e-7f);
                if (Math.Abs(norm) <= tolerance) { output[i * 7 + 3] = 0; output[i * 7 + 4] = 0; }
                else { output[i * 7 + 3] = Round((float)(dx * 8192) / norm); output[i * 7 + 4] = Round((float)(dy * 8192) / norm); }
            }
            begin = end;
        }
        begin = 0;
        for (int g = 0; g < groups; g++)
        {
            int end = groupEnds[g];
            for (int i = begin; i < end; i++)
            {
                int before = i == begin ? i : i - 1, after = i == end - 1 ? i : i + 1;
                int px = output[before * 7 + 3], py = output[before * 7 + 4];
                int nx = output[after * 7 + 3], ny = output[after * 7 + 4];
                output[i * 7 + 5] = (short)((px * nx + py * ny) >> 13);
                output[i * 7 + 6] = (short)((px * ny - py * nx) >> 13);
            }
            begin = end;
        }
        for (int i = count; i < 30; i++) output.AsSpan((i - 1) * 7, 7).CopyTo(output.AsSpan(i * 7, 7));
        for (int i = 0; i < 30; i++)
            for (int c = 0; c < 7; c++)
            { token.ThrowIfCancellationRequested(); output[i * 7 + c] = Round(output[i * 7 + c] * profile.ShapePrototypes.ComponentWeight(c)); }
        token.ThrowIfCancellationRequested(); return new(this, temporalIssuer, output, count, groups);
    }
}
