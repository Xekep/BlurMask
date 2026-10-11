namespace InkRecognition;

/// <summary>Independent confidence and pre-confidence probability channels for one prepared glyph.</summary>
public sealed class LegacyDirectionalGlyphScores
{
    public ReadOnlyMemory<float> Confidence { get; }
    public IReadOnlyList<ReadOnlyMemory<int>?> FamilyProbabilities { get; }

    internal LegacyDirectionalGlyphScores(float[] confidence, ReadOnlyMemory<int>?[] probabilities)
    { Confidence = confidence; FamilyProbabilities = Array.AsReadOnly(probabilities); }

    /// <summary>
    /// Obtain the restored integer channel with seven explicit weights and an
    /// explicit multiplier. Inactive slots must have weight zero. This is not
    /// confidence-to-log conversion or a complete acoustic/language score.
    /// </summary>
    public int[] GetIntegerCosts(ReadOnlySpan<float> weights, float multiplier,
        ReadOnlySpan<byte> enabledClasses = default, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (weights.Length != 7) throw new ArgumentException("Expected seven explicit family weights.", nameof(weights));
        var slots = new LegacyAcousticCostFamily?[7];
        for (int i = 0; i < slots.Length; i++)
        {
            if (!float.IsFinite(weights[i]) || weights[i] < 0 || (FamilyProbabilities[i] is null && weights[i] != 0))
                throw new ArgumentException("Finite nonnegative weights required; inactive slots must be zero.", nameof(weights));
            if (FamilyProbabilities[i] is { } probabilities) slots[i] = new(probabilities, weights[i]);
        }
        return LegacyAcousticCostEnsemble.Combine(slots, multiplier, enabledClasses, cancellationToken);
    }
}

/// <summary>
/// Four checked directional feature/neural families, consuming prepared ink,
/// explicit baseline, context and clipped-boundary channels. Context selection,
/// candidate cuts and dictionary search are independent upstream operations.
/// </summary>
public sealed class LegacyDirectionalGlyphClassifier
{
    private readonly LegacyDirectionalAcousticFamily[] _families;
    private readonly LegacyDirectionalCalibration _temporal, _spatial;
    private readonly LegacyInkActivations _activations;
    public int OutputWidth { get; }

    public LegacyDirectionalGlyphClassifier(IReadOnlyList<LegacyDirectionalAcousticFamily> families,
        LegacyInkActivations activations, LegacyDirectionalCalibration temporal, LegacyDirectionalCalibration spatial)
    {
        ArgumentNullException.ThrowIfNull(families); ArgumentNullException.ThrowIfNull(activations);
        if (families.Count != 4) throw new ArgumentException("Exactly four matching acoustic families required.", nameof(families));
        int[] widths = [256,238,313,313]; ushort[] kinds = [0x1f,0x2f,0x3f,0x4f];
        _families = new LegacyDirectionalAcousticFamily[4];
        var validation = new LegacyAcousticFamilyScores?[7];
        for (int i = 0; i < 4; i++)
        {
            var family = families[i]; ArgumentNullException.ThrowIfNull(family); ArgumentNullException.ThrowIfNull(family.Network);
            if (family.Network.InputWidth != widths[i] || family.Network.Kind != kinds[i] || family.Network.OutputWidth is < 1 or > 512 ||
                (OutputWidth != 0 && OutputWidth != family.Network.OutputWidth))
                throw new ArgumentException("Family ordering, widths or output profile differ.", nameof(families));
            OutputWidth = family.Network.OutputWidth;
            _families[i] = new(family.Network, family.Centers.ToArray(), family.Slopes.ToArray());
            validation[i] = new(new int[OutputWidth], _families[i].Centers, _families[i].Slopes);
        }
        _ = LegacyAcousticEnsemble.Combine(validation);
        _activations = activations; _temporal = Snapshot(temporal); _spatial = Snapshot(spatial);
    }

    public LegacyDirectionalGlyphScores Classify(IReadOnlyList<LegacyInkStroke> glyph, LegacyInkBaseline baseline,
        ReadOnlySpan<short> context, short startBoundary, short endBoundary, bool includeBaselineFamilies = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(glyph); cancellationToken.ThrowIfCancellationRequested();
        if (glyph.Count is < 1 or > 256 || baseline.Height <= 0 || context.Length is not (0 or 28) ||
            startBoundary is not (-1024 or 1024) || endBoundary is not (-1024 or 1024))
            throw new ArgumentException("Bounded glyph, positive baseline height, context0/28 and explicit +/-1024 boundaries required.");
        int points = 0;
        foreach (var stroke in glyph)
        {
            cancellationToken.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(stroke);
            if (stroke.Points.Length > 4096 - points) throw new ArgumentException("Glyph exceeds4096 points.", nameof(glyph));
            points += stroke.Points.Length;
        }
        if (context.IsEmpty)
        {
            var neutralContext = new short[28];
            Array.Fill(neutralContext, (short)-1024);
            context = neutralContext;
        }
        short[][] inputs = new short[4][];
        if (includeBaselineFamilies)
        {
            inputs[0] = LegacyStrokeFeatures.ExtractInputWithBaseline(glyph, baseline, context, startBoundary, endBoundary, cancellationToken);
            inputs[1] = LegacyRasterFeatures.ExtractInputWithBaseline(glyph, baseline, context, startBoundary, endBoundary, cancellationToken);
        }
        inputs[2] = LegacyTemporalDirectionalFeatures.Extract(glyph, baseline, _temporal.Minima.Span, _temporal.Maxima.Span,
            context, startBoundary, endBoundary, cancellationToken);
        inputs[3] = LegacySpatialDirectionalFeatures.Extract(glyph, baseline, _spatial.Minima.Span, _spatial.Maxima.Span,
            context, startBoundary, endBoundary, cancellationToken);
        var scores = new LegacyAcousticFamilyScores?[7];
        var probabilities = new ReadOnlyMemory<int>?[7];
        for (int i = includeBaselineFamilies ? 0 : 2; i < 4; i++)
        {
            var family = _families[i];
            var probability = LegacyClassScores.Convert(family.Network.Evaluate(inputs[i], _activations,
                cancellationToken: cancellationToken), cancellationToken).Probability32768;
            probabilities[i] = probability;
            scores[i] = new(probability, family.Centers, family.Slopes);
        }
        return new(LegacyAcousticEnsemble.Combine(scores, cancellationToken), probabilities);
    }

    private static LegacyDirectionalCalibration Snapshot(LegacyDirectionalCalibration calibration)
    {
        ArgumentNullException.ThrowIfNull(calibration);
        if (calibration.Minima.Length != 274 || calibration.Maxima.Length != 274)
            throw new ArgumentException("Matching274-column calibration required.");
        var minima = calibration.Minima.ToArray(); var maxima = calibration.Maxima.ToArray();
        for (int i = 0; i < minima.Length; i++)
            if (minima[i] > maxima[i]) throw new ArgumentException("Invalid geometry range.");
        return new(minima, maxima);
    }
}
