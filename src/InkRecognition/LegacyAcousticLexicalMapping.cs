namespace InkRecognition;

public readonly record struct LegacyIntegerLexicalCandidate(ushort Label, int Cost);

/// <summary>
/// Reusable acoustic-class to dictionary-label mapping for bounded classifier
/// output. Optional stored forms keep the strongest source score per label.
/// This original policy does not implement native case scoring or preservation.
/// </summary>
public sealed class LegacyAcousticLexicalMapping
{
    private readonly ushort[][] _labels;
    public int OutputWidth => _labels.Length;

    public LegacyAcousticLexicalMapping(LegacyInkAlphabet acousticAlphabet,
        LegacyInkAlphabet lexicalAlphabet, int outputWidth, int? relatedFormMode = null)
    {
        ArgumentNullException.ThrowIfNull(acousticAlphabet);
        ArgumentNullException.ThrowIfNull(lexicalAlphabet);
        if (outputWidth is < 1 or > 512 || outputWidth > acousticAlphabet.LabelCount)
            throw new ArgumentOutOfRangeException(nameof(outputWidth));
        if (relatedFormMode is < 0 or > 2)
            throw new ArgumentOutOfRangeException(nameof(relatedFormMode));
        _labels = new ushort[outputWidth][];
        var allLabels = new HashSet<ushort>();
        for (ushort source = 0; source < outputWidth; source++)
        {
            var labels = new List<ushort>(4);
            void Add(ushort acoustic)
            {
                if (acoustic != ushort.MaxValue && acousticAlphabet.TryGetText(acoustic, out string text) &&
                    lexicalAlphabet.TryGetLabel(text, out ushort label) && !labels.Contains(label))
                { labels.Add(label); allLabels.Add(label); }
            }
            Add(source);
            if (relatedFormMode is int mode && acousticAlphabet.TryGetRelatedForms(source, mode, out var forms))
            { Add(forms.First); Add(forms.Second); Add(forms.Third); }
            _labels[source] = labels.ToArray();
        }
        if (allLabels.Count > 512)
            throw new ArgumentException("Mapped glyph choices exceed the bounded decoder domain.", nameof(lexicalAlphabet));
    }

    /// <summary>
    /// Map individually quantized EXP probabilities in 0..32768. Rounded inputs
    /// need not sum exactly to 32768. Zero scores and absent dictionary symbols
    /// produce no choice. Related aliases retain a maximum, never a score sum.
    /// </summary>
    public LegacyLexicalCandidate[] Map(ReadOnlySpan<int> probability32768,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (probability32768.Length != OutputWidth)
            throw new ArgumentException("Acoustic score width differs from its alphabet mapping.", nameof(probability32768));
        foreach (int probability in probability32768)
            if (probability is < 0 or > 32768)
                throw new ArgumentException("Acoustic probabilities must be in 0..32768.", nameof(probability32768));
        var choices = new Dictionary<ushort, float>();
        for (int source = 0; source < OutputWidth; source++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (probability32768[source] == 0) continue;
            float score = (float)Math.Log(probability32768[source] / 32768d);
            foreach (ushort label in _labels[source])
                if (!choices.TryGetValue(label, out float previous) || score > previous)
                    choices[label] = score;
        }
        return choices.OrderBy(choice => choice.Key)
            .Select(choice => new LegacyLexicalCandidate(choice.Key, choice.Value)).ToArray();
    }

    /// <summary>
    /// Preserve the native integer-cost channel, retaining the lowest source
    /// cost for each supplied alias. This alias policy is original and does
    /// not infer native case penalties. Impossible classes produce no label.
    /// </summary>
    public LegacyIntegerLexicalCandidate[] MapIntegerCosts(ReadOnlySpan<int> costs,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (costs.Length != OutputWidth) throw new ArgumentException("Acoustic cost width differs from mapping.", nameof(costs));
        foreach (int cost in costs)
            if (cost is < 0 or > LegacyAcousticFixedLog.ImpossibleCost)
                throw new ArgumentException("Expected bounded nonnegative integer costs.", nameof(costs));
        var choices = new Dictionary<ushort, int>();
        for (int source = 0; source < OutputWidth; source++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (costs[source] == LegacyAcousticFixedLog.ImpossibleCost) continue;
            foreach (ushort label in _labels[source])
                if (!choices.TryGetValue(label, out int previous) || costs[source] < previous) choices[label] = costs[source];
        }
        return choices.OrderBy(choice => choice.Key).Select(choice => new LegacyIntegerLexicalCandidate(choice.Key, choice.Value)).ToArray();
    }
}
