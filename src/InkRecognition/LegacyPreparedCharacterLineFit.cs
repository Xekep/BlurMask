using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>The 24-byte C1A configuration supplied to Ink020d00, not a raw-ink default.</summary>
public readonly record struct LegacyCharacterLineFitProfile(byte MinimumItemCount, byte MaximumItemCount,
    float MinimumSlope, float MaximumSlope, float MinimumHeight, float MaximumBoxWidth, float KnownGlyphWeightCap)
{
    public static LegacyCharacterLineFitProfile ReadPrepared(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != 24) throw new InvalidDataException("C1A configuration requires exactly 24 bytes.");
        var result = new LegacyCharacterLineFitProfile(bytes[0], bytes[1],
            BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(bytes[4..8])),
            BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(bytes[8..12])),
            BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(bytes[12..16])),
            BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(bytes[16..20])),
            BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(bytes[20..24])));
        result.Validate();
        return result;
    }

    internal void Validate()
    {
        if (MaximumItemCount <= MinimumItemCount || !float.IsFinite(MinimumSlope) || !float.IsFinite(MaximumSlope) ||
            MinimumSlope > MaximumSlope || !float.IsFinite(MinimumHeight) || MinimumHeight < 0 ||
            !float.IsFinite(MaximumBoxWidth) || MaximumBoxWidth < 0 || !float.IsFinite(KnownGlyphWeightCap))
            throw new ArgumentException("Finite ordered prepared C1A bounds and distinct item-count thresholds required.");
    }
}

/// <summary>One explicit native prepared candidate: weight pair, kind, main/alternate vertical profiles.</summary>
public readonly record struct LegacyCharacterLineCandidate(float BaseWeight, float WeightCoefficient, int Kind,
    float MainTop, float AlternateTop, float MainBottom, float AlternateBottom);

/// <summary>Immutable snapshot of a 0x248-byte C1A item. Candidate production is deliberately separate.</summary>
public sealed class LegacyPreparedCharacterLineItem
{
    private readonly LegacyCharacterLineCandidate[] _candidates = new LegacyCharacterLineCandidate[20];
    public bool KnownGlyph { get; }
    public bool ClipVerticalToWidth { get; }
    public bool DownweightMarker { get; }
    public float Left { get; }
    public float Right { get; }
    public float Top { get; }
    public float Bottom { get; }
    public int CandidateCount { get; }
    internal ReadOnlySpan<LegacyCharacterLineCandidate> Candidates => _candidates;

    public LegacyPreparedCharacterLineItem(bool knownGlyph, bool clipVerticalToWidth, bool downweightMarker,
        float left, float right, float top, float bottom, ReadOnlySpan<LegacyCharacterLineCandidate> candidates,
        int? candidateCount = null)
    {
        int count = candidateCount ?? candidates.Length;
        if (candidates.Length > 20 || count < 0 || count > candidates.Length ||
            !float.IsFinite(left) || !float.IsFinite(right) || !float.IsFinite(top) || !float.IsFinite(bottom) ||
            left > right || top > bottom)
            throw new ArgumentException("Finite ordered prepared item bounds and at most 20 candidates required.");
        foreach (var c in candidates)
            if (!float.IsFinite(c.BaseWeight) || !float.IsFinite(c.WeightCoefficient) || !float.IsFinite(c.MainTop) ||
                !float.IsFinite(c.AlternateTop) || !float.IsFinite(c.MainBottom) || !float.IsFinite(c.AlternateBottom))
                throw new ArgumentException("All native candidate fields must be finite, including inactive slots.");
        candidates.CopyTo(_candidates);
        KnownGlyph = knownGlyph; ClipVerticalToWidth = clipVerticalToWidth; DownweightMarker = downweightMarker;
        Left = left; Right = right; Top = top; Bottom = bottom; CandidateCount = count;
    }

    public LegacyCharacterLineCandidate[] CopyPreparedSlots() => (LegacyCharacterLineCandidate[])_candidates.Clone();

    public static LegacyPreparedCharacterLineItem ReadPrepared(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != 0x248) throw new InvalidDataException("C1A item requires exactly 584 bytes.");
        uint count = BinaryPrimitives.ReadUInt32LittleEndian(bytes[4..8]);
        if (count > 20) throw new InvalidDataException("C1A candidate count exceeds its 20-slot storage.");
        var slots = new LegacyCharacterLineCandidate[20];
        for (int i = 0; i < 20; i++)
            slots[i] = new(ReadFloat(bytes, 0x18 + i * 8), ReadFloat(bytes, 0x1c + i * 8),
                BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(0xb8 + i * 4, 4)),
                ReadFloat(bytes, 0x108 + i * 8), ReadFloat(bytes, 0x10c + i * 8),
                ReadFloat(bytes, 0x1a8 + i * 8), ReadFloat(bytes, 0x1ac + i * 8));
        return new(bytes[0] != 0, bytes[1] != 0, bytes[2] != 0, ReadFloat(bytes, 8), ReadFloat(bytes, 12),
            ReadFloat(bytes, 16), ReadFloat(bytes, 20), slots, (int)count);
    }

    private static float ReadFloat(ReadOnlySpan<byte> bytes, int offset) =>
        BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(offset, 4)));
}

/// <summary>Primary fit output. RawProfileAffinity is a native diagnostic score, not calibrated confidence.</summary>
public readonly record struct LegacyPreparedCharacterLineFitResult(LegacyCharacterLineSolution Solution,
    int Iterations, double RawProfileAffinity)
{
    public (float BaselineY, float XHeight, float Slope) ToPreparedFitOutput() => Solution.ToPreparedFitOutput();
}

/// <summary>
/// Prepared C1A Ink021110 iterative fit. This does not produce candidates from raw strokes,
/// invoke the Ink0221a0 bbox fallback, or infer recognition/font accuracy.
/// </summary>
public static class LegacyPreparedCharacterLineFit
{
    public static LegacyPreparedCharacterLineFitResult? Fit(ReadOnlySpan<LegacyPreparedCharacterLineItem> items,
        LegacyCharacterLineFitProfile profile, bool forceZeroSlope = false,
        double incomingProfileAffinity = double.MaxValue, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested(); profile.Validate();
        if (items.Length > 256) throw new ArgumentException("Prepared C1A item budget is 256.");
        if (!double.IsFinite(incomingProfileAffinity) || incomingProfileAffinity < 0)
            throw new ArgumentException("Finite nonnegative incoming diagnostic affinity required.");
        if (items.IsEmpty) return null;
        int known = 0;
        foreach (var item in items)
        {
            if (item is null) throw new ArgumentException("Prepared items must not be null.");
            if (item.KnownGlyph) known++;
        }
        float knownScale = profile.KnownGlyphWeightCap > 0 && known > 0
            ? Math.Min(profile.KnownGlyphWeightCap, (float)known) / (float)known : 1;
        float blend = ((float)items.Length - (float)profile.MinimumItemCount) /
            ((float)profile.MaximumItemCount - (float)profile.MinimumItemCount);
        blend = Math.Clamp(blend, 0, 1);
        double lower = (double)profile.MinimumSlope * (double)blend, upper = (double)profile.MaximumSlope * (double)blend;
        var observations = new LegacyCharacterLineObservation[items.Length * 40];
        Span<double> weights = stackalloc double[20];
        var current = new LegacyCharacterLineSolution(1, 0, 0);
        double lastX = 0, lastTop = 0, lastBottom = 0;
        for (int pass = 0; pass <= 250; pass++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int used = 0;
            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                double x = (double)((item.Right + item.Left) * .5f), top = item.Top, bottom = item.Bottom;
                if (item.ClipVerticalToWidth)
                {
                    float width = item.Right - item.Left, middle = (item.Top + item.Bottom) * .5f;
                    if (profile.MaximumBoxWidth <= width) width = profile.MaximumBoxWidth;
                    top = (double)(middle - width * .5f); bottom = (double)(middle + width * .5f);
                }
                if (!double.IsFinite(x) || !double.IsFinite(top) || !double.IsFinite(bottom))
                    throw new InvalidDataException("Prepared C1A float geometry overflow.");
                lastX = x; lastTop = top; lastBottom = bottom;
                var slots = item.Candidates;
                for (int i = 0; i < 20; i++)
                {
                    double w = (double)slots[i].BaseWeight * (double)slots[i].BaseWeight;
                    if (item.DownweightMarker) w *= 0.009999999776482582;
                    if (item.KnownGlyph) w *= (double)knownScale;
                    weights[i] = w;
                }
                int count = pass == 250 ? 1 : item.CandidateCount;
                double normalizedTop = ((top - x * current.Slope) - current.Intercept) / current.HeightScale;
                double normalizedBottom = ((bottom - x * current.Slope) - current.Intercept) / current.HeightScale;
                for (int i = 0; i < count; i++)
                {
                    var c = slots[i]; double w = weights[i], pTop = c.MainTop, pBottom = c.MainBottom;
                    if (pass == 0 || pass == 250)
                    {
                        if (c.Kind == 2)
                        {
                            w *= .5;
                            observations[used++] = new(x, top, bottom, c.AlternateTop, c.AlternateBottom, w);
                        }
                    }
                    else
                    {
                        double affinity = Affinity(normalizedTop, normalizedBottom, pTop, pBottom);
                        if (c.Kind == 2)
                        {
                            double alternate = Affinity(normalizedTop, normalizedBottom, c.AlternateTop, c.AlternateBottom);
                            if (affinity < alternate) { affinity = alternate; pTop = c.AlternateTop; pBottom = c.AlternateBottom; }
                        }
                        w = w * .5 * affinity;
                    }
                    observations[used++] = new(x, top, bottom, pTop, pBottom, w);
                }
            }
            var moments = LegacyPreparedCharacterLineEquations.Accumulate(observations.AsSpan(0, used), cancellationToken);
            if (moments.W <= 0) return null;
            var next = LegacyPreparedCharacterLineEquations.Solve(moments, lower, upper, forceZeroSlope);
            if (next.HeightScale == 0) return null;
            double ds = next.Slope - current.Slope, dh = next.HeightScale - current.HeightScale,
                di = next.Intercept - current.Intercept;
            double delta = (ds * ds + dh * dh) + di * di;
            current = next;
            if (pass == 250 || (pass >= 9 && delta < 1e-10))
            {
                // Native finisher normalizes the LAST item once and reuses its scratch weights
                // for each item's profiles. Preserve the observed implementation, not an intended score.
                double nt = ((lastTop - lastX * current.Slope) - current.Intercept) / current.HeightScale;
                double nb = ((lastBottom - lastX * current.Slope) - current.Intercept) / current.HeightScale;
                double total = 0;
                foreach (var item in items)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    for (int i = 0; i < item.CandidateCount; i++)
                    {
                        var c = item.Candidates[i];
                        double a = Affinity(nt, nb, c.MainTop, c.MainBottom);
                        if (c.Kind == 2) a = Math.Max(a, Affinity(nt, nb, c.AlternateTop, c.AlternateBottom));
                        total += weights[i] * .5 * a;
                    }
                }
                double score = total / (double)items.Length;
                if (!double.IsFinite(score)) throw new InvalidDataException("Prepared C1A diagnostic score overflow.");
                var result = new LegacyPreparedCharacterLineFitResult(current, pass + 1, Math.Min(incomingProfileAffinity, score));
                _ = result.ToPreparedFitOutput();
                return result;
            }
        }
        throw new InvalidOperationException("Bounded native C1A iteration limit was exceeded.");
    }

    private static double Affinity(double top, double bottom, double profileTop, double profileBottom)
    {
        double result = 1 / (Math.Abs(top - profileTop) + .7) + 1 / (Math.Abs(bottom - profileBottom) + .7);
        if (!double.IsFinite(result)) throw new InvalidDataException("Prepared C1A affinity arithmetic overflow.");
        return result;
    }
}
