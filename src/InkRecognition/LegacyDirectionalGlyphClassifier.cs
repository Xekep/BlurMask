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
    private readonly bool _actual2B1;
    private readonly int _specialOutputLabel;
    public int OutputWidth { get; }

    public LegacyDirectionalGlyphClassifier(IReadOnlyList<LegacyDirectionalAcousticFamily> families,
        LegacyInkActivations activations, LegacyDirectionalCalibration temporal, LegacyDirectionalCalibration spatial)
        : this(families, activations, temporal, spatial, false) { }

    internal static LegacyDirectionalGlyphClassifier CreateActual2B1(
        IReadOnlyList<LegacyDirectionalAcousticFamily> families, LegacyInkActivations activations,
        LegacyDirectionalCalibration temporal, LegacyDirectionalCalibration spatial) =>
        new(families, activations, temporal, spatial, true);

    // Only the authenticated fresh RU bundle supplies this actual AL and mask policy.
    internal static LegacyDirectionalGlyphClassifier CreateActual2B1Mixed(
        IReadOnlyList<LegacyDirectionalAcousticFamily> families, LegacyInkActivations activations,
        LegacyDirectionalCalibration temporal, LegacyDirectionalCalibration spatial,
        LegacyInkAlphabet alphabet, byte actualMaskPolicy)
    {
        ArgumentNullException.ThrowIfNull(alphabet);
        if (actualMaskPolicy != 0) throw new NotSupportedException("Masked mixed conversion is not restored.");
        byte[] header = alphabet.CopySerializedHeader();
        if (header.Length != 16) throw new InvalidDataException("Actual AL header required.");
        ushort label = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(6));
        return new(families, activations, temporal, spatial, true, label == ushort.MaxValue ? -1 : label);
    }

    private LegacyDirectionalGlyphClassifier(IReadOnlyList<LegacyDirectionalAcousticFamily> families,
        LegacyInkActivations activations, LegacyDirectionalCalibration temporal, LegacyDirectionalCalibration spatial,
        bool actual2B1, int specialOutputLabel = -1)
    {
        _actual2B1 = actual2B1; _specialOutputLabel = specialOutputLabel;
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
        if (specialOutputLabel < -1 || specialOutputLabel >= OutputWidth || (specialOutputLabel >= 0 && !actual2B1))
            throw new InvalidDataException("Actual AL special label is outside the network output mapping.");
        _ = LegacyAcousticEnsemble.Combine(validation);
        _activations = activations; _temporal = Snapshot(temporal); _spatial = Snapshot(spatial);
    }

    public LegacyDirectionalGlyphScores Classify(IReadOnlyList<LegacyInkStroke> glyph, LegacyInkBaseline baseline,
        ReadOnlySpan<short> context, short startBoundary, short endBoundary, bool includeBaselineFamilies = true,
        CancellationToken cancellationToken = default)
        =>ClassifyWithConfiguredDebit(glyph,baseline,context,startBoundary,endBoundary,includeBaselineFamilies,cancellationToken,null);
    internal LegacyDirectionalGlyphScores ClassifyWithConfiguredDebit(IReadOnlyList<LegacyInkStroke> glyph,LegacyInkBaseline baseline,
        ReadOnlySpan<short> context,short startBoundary,short endBoundary,bool includeBaselineFamilies,
        CancellationToken cancellationToken,LegacyConfiguredSourceOperationDebit? debit)
    {
        if(debit is not null&&(!_actual2B1||!includeBaselineFamilies))throw new ArgumentException("Configured debit requires the existing actual2B1 classifier.");
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
            //Actual1111 MAIN dispatches00A9F0 for zero integer extent. The
            //generic geometry API keeps its existing nondegenerate domain.
            if (!_actual2B1 || !LegacyMainPointGlyphFeatures.TryCreate(glyph, baseline, context,
                startBoundary, endBoundary, cancellationToken, out inputs[0], out inputs[1]))
            {
                debit?.BasicRaster(points,glyph.Count,cancellationToken);
                inputs[0] = LegacyStrokeFeatures.ExtractInputWithBaseline(glyph, baseline, context, startBoundary, endBoundary, cancellationToken);
                inputs[1] = LegacyRasterFeatures.ExtractInputWithBaseline(glyph, baseline, context, startBoundary, endBoundary, cancellationToken);
            }
        }
        if (_actual2B1)
        {
            var directional = LegacySaturatedDirectionalFeatures.ExtractWithConfiguredDebit(glyph, baseline,
                _temporal.Minima.Span, _temporal.Maxima.Span, _spatial.Minima.Span, _spatial.Maxima.Span,
                context, startBoundary, endBoundary, cancellationToken,debit);
            inputs[2] = directional.Temporal; inputs[3] = directional.Spatial;
        }
        else
        {
            inputs[2] = LegacyTemporalDirectionalFeatures.Extract(glyph, baseline, _temporal.Minima.Span, _temporal.Maxima.Span,
                context, startBoundary, endBoundary, cancellationToken);
            inputs[3] = LegacySpatialDirectionalFeatures.Extract(glyph, baseline, _spatial.Minima.Span, _spatial.Maxima.Span,
                context, startBoundary, endBoundary, cancellationToken);
        }
        var scores = new LegacyAcousticFamilyScores?[7];
        var probabilities = new ReadOnlyMemory<int>?[7];
        debit?.Neural(cancellationToken);
        for (int i = includeBaselineFamilies ? 0 : 2; i < 4; i++)
        {
            var family = _families[i];
            var actualRaw = family.Network.Evaluate(inputs[i], _activations, cancellationToken: cancellationToken);
            ReadOnlyMemory<int> probability = _specialOutputLabel >= 0
                ? LegacyClassScores.ConvertMixedProbabilities(actualRaw, _specialOutputLabel, cancellationToken)
                : LegacyClassScores.Convert(actualRaw, cancellationToken).Probability32768;
            probabilities[i] = probability;
            scores[i] = new(probability, family.Centers, family.Slopes);
        }
        return new(_specialOutputLabel >= 0 ? LegacyAcousticEnsemble.CombineMixed(scores, cancellationToken)
            : LegacyAcousticEnsemble.Combine(scores, cancellationToken), probabilities);
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
