using System.Text;

namespace InkRecognition;

/// <summary>One caller-owned model and its matching mode1 per-class coefficients.</summary>
public sealed record LegacyDirectionalAcousticFamily(LegacyInkNetwork Network,
    ReadOnlyMemory<float> Centers, ReadOnlyMemory<float> Slopes);

/// <summary>Caller-owned trained geometry ranges for one 313-input family.</summary>
public sealed record LegacyDirectionalCalibration(ReadOnlyMemory<short> Minima,
    ReadOnlyMemory<short> Maxima);

public sealed record LegacyDirectionalWordResult(IReadOnlyList<LegacySegmentedWord> Words,
    int ContextWindowCount, int GlyphCount);

/// <summary>
/// Experimental reusable separated-word path: four inspected feature families,
/// explicit source baseline, fresh prior context, mode1 calibration and packed
/// dictionary search. Confidence-to-cost/alias/beam policies are original;
/// native lexical costs, case preservation and joined-character cuts are absent.
/// </summary>
public sealed class LegacyDirectionalWordRecognizer
{
    private readonly LegacyDirectionalAcousticFamily[] _families;
    private readonly LegacyDirectionalCalibration _temporal, _spatial;
    private readonly LegacyInkActivations _activations;
    private readonly LegacyContextWindowScorer _context;
    private readonly LegacyAcousticLexicalMapping _mapping;
    private readonly LegacyInkAlphabet _alphabet;
    private readonly LegacyPackedLexicon _lexicon;

    /// <param name="families">Exactly four native slots: trajectory256, raster238, temporal313, spatial313.</param>
    public LegacyDirectionalWordRecognizer(IReadOnlyList<LegacyDirectionalAcousticFamily> families,
        LegacyInkNetwork initialContext, LegacyInkActivations activations,
        LegacyInkAlphabet acousticAlphabet, LegacyInkAlphabet lexicalAlphabet, LegacyPackedLexicon lexicon,
        LegacyDirectionalCalibration temporal, LegacyDirectionalCalibration spatial,
        int? relatedFormMode = null)
    {
        ArgumentNullException.ThrowIfNull(families); ArgumentNullException.ThrowIfNull(activations);
        ArgumentNullException.ThrowIfNull(lexicalAlphabet); ArgumentNullException.ThrowIfNull(lexicon);
        if (families.Count != 4) throw new ArgumentException("Exactly four matching acoustic families are required.", nameof(families));
        int[] widths = [256, 238, 313, 313]; ushort[] kinds = [0x1f, 0x2f, 0x3f, 0x4f];
        _families = new LegacyDirectionalAcousticFamily[4];
        int outputs = 0;
        var validation = new LegacyAcousticFamilyScores?[7];
        for (int i = 0; i < 4; i++)
        {
            var family = families[i]; ArgumentNullException.ThrowIfNull(family);
            ArgumentNullException.ThrowIfNull(family.Network);
            if (family.Network.InputWidth != widths[i] || family.Network.Kind != kinds[i] ||
                family.Network.OutputWidth is < 1 or > 512 ||
                (outputs != 0 && family.Network.OutputWidth != outputs))
                throw new ArgumentException("Acoustic family profile, ordering or output widths differ.", nameof(families));
            outputs = family.Network.OutputWidth;
            _families[i] = new(family.Network, family.Centers.ToArray(), family.Slopes.ToArray());
            validation[i] = new(new int[outputs], _families[i].Centers, _families[i].Slopes);
        }
        _ = LegacyAcousticEnsemble.Combine(validation);
        _temporal = Snapshot(temporal); _spatial = Snapshot(spatial);
        _activations = activations; _alphabet = lexicalAlphabet; _lexicon = lexicon;
        _context = new(initialContext, activations);
        _mapping = new(acousticAlphabet, lexicalAlphabet, outputs, relatedFormMode);
    }

    /// <summary>
    /// Process one caller-supplied line/word containing separated whole physical
    /// glyph strokes, at most 256 strokes and 4096 points. Baseline height must
    /// be positive and expressed in source units. A connected word is not split
    /// into letters here. Results are candidates, not acceptance confidence.
    /// </summary>
    public LegacyDirectionalWordResult Recognize(IReadOnlyList<LegacyInkStroke> strokes,
        LegacyInkBaseline baseline, int beamWidth = 64, bool includeBaselineFamilies = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(strokes); cancellationToken.ThrowIfCancellationRequested();
        if (strokes.Count is < 1 or > 256 || baseline.Height <= 0 || beamWidth is < 1 or > 256)
            throw new ArgumentException("Expected 1..256 physical strokes, positive source height and beam width 1..256.");
        int totalPoints = 0;
        var indices = new Dictionary<LegacyInkStroke, int>(ReferenceEqualityComparer.Instance);
        for (int i = 0; i < strokes.Count; i++)
        {
            ArgumentNullException.ThrowIfNull(strokes[i]);
            if (strokes[i].Points.Length > 4096 - totalPoints || !indices.TryAdd(strokes[i], i))
                throw new ArgumentException("The line exceeds 4096 points or repeats a physical stroke instance.", nameof(strokes));
            totalPoints += strokes[i].Points.Length;
        }
        if (!LegacySeparatedInkSegmenter.TrySegment(strokes, out var glyphs) ||
            glyphs.Length > _lexicon.MaximumWordLength)
            return new([], 0, 0);
        var groups = _context.ScoreLine(strokes, 0, baseline.Height, baseline.Y,
            new int[strokes.Count], cancellationToken: cancellationToken);
        var choices = new ReadOnlyMemory<LegacyLexicalCandidate>[glyphs.Length];
        for (int target = 0; target < glyphs.Length; target++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var glyph = glyphs[target];
            var ids = glyph.Select(stroke => indices[stroke]).ToArray();
            var selected = LegacyPriorContextSelection.Select(groups, 0, ids);
            float sumX = 0; int pointCount = 0;
            foreach (var stroke in glyph) foreach (var point in stroke.Points)
            { sumX += point.X; pointCount++; }
            var context = LegacyFloatInputQuantization.Quantize(
                LegacyContextWeighting.Combine(selected, sumX / pointCount, baseline.Height, 0, 1, 1),
                cancellationToken: cancellationToken);
            short[][] inputs = new short[4][];
            if (includeBaselineFamilies)
            {
                inputs[0] = LegacyStrokeFeatures.ExtractInputWithBaseline(glyph, baseline, context, 1024, 1024, cancellationToken);
                inputs[1] = LegacyRasterFeatures.ExtractInputWithBaseline(glyph, baseline, context, 1024, 1024, cancellationToken);
            }
            inputs[2] = LegacyTemporalDirectionalFeatures.Extract(glyph, baseline, _temporal.Minima.Span,
                _temporal.Maxima.Span, context, 1024, 1024, cancellationToken);
            inputs[3] = LegacySpatialDirectionalFeatures.Extract(glyph, baseline, _spatial.Minima.Span,
                _spatial.Maxima.Span, context, 1024, 1024, cancellationToken);
            var slots = new LegacyAcousticFamilyScores?[7];
            for (int i = includeBaselineFamilies ? 0 : 2; i < 4; i++)
            {
                var family = _families[i];
                slots[i] = new(LegacyClassScores.Convert(family.Network.Evaluate(inputs[i], _activations,
                    cancellationToken: cancellationToken), cancellationToken).Probability32768,
                    family.Centers, family.Slopes);
            }
            var confidence = LegacyAcousticEnsemble.Combine(slots, cancellationToken);
            // Explicit original cost adapter; not the native pre-confidence cost channel.
            choices[target] = _mapping.Map(confidence.Select(value => (int)(value * 32768)).ToArray(), cancellationToken);
        }
        var decoded = LegacyLexicalDecoder.Decode(_lexicon, choices, beamWidth, cancellationToken);
        var words = decoded.Select(match => new LegacySegmentedWord(Text(match.Labels.Span), match.LogScore)).ToArray();
        return new(words, groups.Length, glyphs.Length);
    }

    private string Text(ReadOnlySpan<ushort> labels)
    {
        var text = new StringBuilder();
        foreach (ushort label in labels)
        {
            if (!_alphabet.TryGetText(label, out string value)) throw new InvalidOperationException("Unknown dictionary label.");
            text.Append(value);
        }
        return text.ToString();
    }

    private static LegacyDirectionalCalibration Snapshot(LegacyDirectionalCalibration calibration)
    {
        ArgumentNullException.ThrowIfNull(calibration);
        if (calibration.Minima.Length != 274 || calibration.Maxima.Length != 274)
            throw new ArgumentException("Matching 274-column geometry calibration is required.");
        var minima = calibration.Minima.ToArray(); var maxima = calibration.Maxima.ToArray();
        for (int i = 0; i < 274; i++)
            if (minima[i] > maxima[i])
                throw new ArgumentException("Geometry calibration ranges must be nondecreasing.");
        return new(minima, maxima);
    }
}
