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
    private readonly LegacyDirectionalGlyphClassifier _classifier;
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
        _classifier = new(families, activations, temporal, spatial);
        _alphabet = lexicalAlphabet; _lexicon = lexicon;
        _context = new(initialContext, activations);
        _mapping = new(acousticAlphabet, lexicalAlphabet, _classifier.OutputWidth, relatedFormMode);
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
            var confidence = _classifier.Classify(glyph, baseline, context, 1024, 1024,
                includeBaselineFamilies, cancellationToken).Confidence.Span;
            // Explicit original cost adapter; not the native pre-confidence cost channel.
            var quantized = new int[confidence.Length];
            for (int i = 0; i < quantized.Length; i++) quantized[i] = (int)(confidence[i] * 32768);
            choices[target] = _mapping.Map(quantized, cancellationToken);
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

}
