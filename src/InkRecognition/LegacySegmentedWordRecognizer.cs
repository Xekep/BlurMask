using System.Text;

namespace InkRecognition;

public sealed record LegacySegmentedWord(string Text, double LogScore);

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
            short[] features = LegacyRasterFeatures.ExtractIsolatedInput(glyphs[i], cancellationToken);
            int[] raw = _network.Evaluate(features, _activations, cancellationToken: cancellationToken);
            var probabilities = LegacyClassScores.Convert(raw).Probability32768.Span;
            var choices = new List<LegacyLexicalCandidate>();
            for (int output = 0; output < _outputLabels.Length; output++)
                if (_outputLabels[output] is ushort label && probabilities[output] > 0)
                    choices.Add(new(label, (float)Math.Log(probabilities[output] / 32768d)));
            positions[i] = choices.ToArray();
        }
        var matches = LegacyLexicalDecoder.Decode(_lexicon, positions, beamWidth, cancellationToken);
        var results = new List<LegacySegmentedWord>(matches.Count);
        foreach (var match in matches)
        {
            var text = new StringBuilder();
            foreach (ushort label in match.Labels.Span)
            {
                if (!_lexicalAlphabet.TryGetText(label, out string value)) throw new InvalidOperationException("Decoded label is absent from the supplied lexical alphabet.");
                text.Append(value);
            }
            results.Add(new(text.ToString(), match.LogScore));
        }
        return results;
    }
}
