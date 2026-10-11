using System.Text;

namespace InkRecognition;

public sealed record LegacySegmentedWord(string Text, double LogScore);

/// <summary>A caller-proposed complete glyph spanning alternative ordered ink cuts.</summary>
public sealed record LegacyInkGlyphAlternative(int Start, int End,
    IReadOnlyList<LegacyInkStroke> Strokes, float SegmentationLogPenalty = 0);

public sealed record LegacySegmentedWordAlternative(string Text, double LogScore, ReadOnlyMemory<int> Cuts);

/// <summary>
/// Experimental original pipeline for a caller-segmented word using the checked
/// 229-input raster classifier and a packed dictionary. This narrow path does not
/// implement automatic glyph segmentation, joined handwriting or the SDK engine.
/// </summary>
public sealed class LegacySegmentedWordRecognizer
{
    private readonly LegacyInkNetwork _network;
    private readonly LegacyInkActivations _activations;
    private readonly LegacyInkAlphabet _lexicalAlphabet;
    private readonly LegacyPackedLexicon _lexicon;
    private readonly ushort?[] _outputLabels;

    public LegacySegmentedWordRecognizer(LegacyInkNetwork rasterNetwork, LegacyInkActivations activations,
        LegacyInkAlphabet acousticAlphabet, LegacyInkAlphabet lexicalAlphabet, LegacyPackedLexicon lexicon)
    {
        ArgumentNullException.ThrowIfNull(rasterNetwork); ArgumentNullException.ThrowIfNull(activations);
        ArgumentNullException.ThrowIfNull(acousticAlphabet); ArgumentNullException.ThrowIfNull(lexicalAlphabet);
        ArgumentNullException.ThrowIfNull(lexicon);
        if (rasterNetwork.InputWidth != LegacyRasterFeatures.IsolatedInputWidth || rasterNetwork.Kind != 0x2f ||
            rasterNetwork.OutputWidth > acousticAlphabet.LabelCount)
            throw new ArgumentException("The checked isolated229 raster profile is required.", nameof(rasterNetwork));
        _network = rasterNetwork; _activations = activations; _lexicalAlphabet = lexicalAlphabet; _lexicon = lexicon;
        _outputLabels = new ushort?[rasterNetwork.OutputWidth];
        for (ushort i = 0; i < _outputLabels.Length; i++)
            if (acousticAlphabet.TryGetText(i, out string text) && lexicalAlphabet.TryGetLabel(text, out ushort label))
                _outputLabels[i] = label;
    }

    /// <summary>
    /// Each entry is one complete, already segmented glyph in reading order.
    /// Context remains empty for this isolated-glyph profile. Scores are acoustic
    /// log probabilities summed along a dictionary path, not calibrated confidence.
    /// </summary>
    public IReadOnlyList<LegacySegmentedWord> Recognize(IReadOnlyList<LegacyInkStroke[]> glyphs,
        int beamWidth = 64, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(glyphs);
        if (glyphs.Count > _lexicon.MaximumWordLength) throw new ArgumentException("Word exceeds dictionary length.", nameof(glyphs));
        var positions = new ReadOnlyMemory<LegacyLexicalCandidate>[glyphs.Count];
        for (int i = 0; i < glyphs.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(glyphs[i]);
            positions[i] = ScoreGlyph(glyphs[i], cancellationToken);
        }
        var matches = LegacyLexicalDecoder.Decode(_lexicon, positions, beamWidth, cancellationToken);
        var results = new List<LegacySegmentedWord>(matches.Count);
        foreach (var match in matches)
        {
            results.Add(new(MapLabels(match.Labels.Span), match.LogScore));
        }
        return results;
    }

    /// <summary>
    /// Score up to128 caller-proposed glyphs, at most four per start cut, and
    /// compare alternative dictionary segmentations. Each glyph contributes its
    /// best32 acoustic classes plus the caller's calibrated segmentation cost.
    /// No cuts are generated here, and scores are not acceptance confidence.
    /// </summary>
    public IReadOnlyList<LegacySegmentedWordAlternative> RecognizeAlternatives(
        IReadOnlyList<LegacyInkGlyphAlternative> alternatives, int finalCut, int beamWidth = 64,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alternatives);
        cancellationToken.ThrowIfCancellationRequested();
        if (finalCut is < 1 or > 128) throw new ArgumentOutOfRangeException(nameof(finalCut));
        if (beamWidth is < 1 or > 128) throw new ArgumentOutOfRangeException(nameof(beamWidth));
        if (alternatives.Count > 128) throw new ArgumentException("Too many scored glyph alternatives.", nameof(alternatives));
        var counts = new int[finalCut];
        // Validate the entire request before doing network inference.
        foreach (var alternative in alternatives)
        {
            cancellationToken.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(alternative);
            ArgumentNullException.ThrowIfNull(alternative.Strokes);
            if (alternative.Start < 0 || alternative.Start >= alternative.End || alternative.End > finalCut ||
                !float.IsFinite(alternative.SegmentationLogPenalty) || alternative.SegmentationLogPenalty is < -1000 or > 0)
                throw new ArgumentException("Invalid glyph boundaries or segmentation cost.", nameof(alternatives));
            if (++counts[alternative.Start] > 4) throw new ArgumentException("One cut exceeds four scored glyph alternatives.", nameof(alternatives));
        }
        var arcs = new List<LegacyLexicalArc>();
        foreach (var alternative in alternatives)
            foreach (var choice in ScoreGlyph(alternative.Strokes, cancellationToken)
                .OrderByDescending(choice => choice.LogScore).ThenBy(choice => choice.Label).Take(32))
                arcs.Add(new(alternative.Start, alternative.End, choice.Label, choice.LogScore + alternative.SegmentationLogPenalty));
        return LegacyLexicalLatticeDecoder.Decode(_lexicon, arcs, finalCut, beamWidth, cancellationToken)
            .Select(match => new LegacySegmentedWordAlternative(MapLabels(match.Labels.Span), match.LogScore, match.Cuts)).ToArray();
    }

    private LegacyLexicalCandidate[] ScoreGlyph(IReadOnlyList<LegacyInkStroke> strokes, CancellationToken token)
    {
        short[] features = LegacyRasterFeatures.ExtractIsolatedInput(strokes, token);
        int[] raw = _network.Evaluate(features, _activations, cancellationToken: token);
        var probabilities = LegacyClassScores.Convert(raw).Probability32768.Span;
        var choices = new List<LegacyLexicalCandidate>();
        for (int output = 0; output < _outputLabels.Length; output++)
            if (_outputLabels[output] is ushort label && probabilities[output] > 0)
                choices.Add(new(label, (float)Math.Log(probabilities[output] / 32768d)));
        return choices.ToArray();
    }

    private string MapLabels(ReadOnlySpan<ushort> labels)
    {
        var text = new StringBuilder();
        foreach (ushort label in labels)
        {
            if (!_lexicalAlphabet.TryGetText(label, out string value)) throw new InvalidOperationException("Decoded label is absent from the supplied lexical alphabet.");
            text.Append(value);
        }
        return text.ToString();
    }
}
