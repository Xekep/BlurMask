namespace InkRecognition;

public readonly record struct LegacyLexicalCandidate(ushort Label, float LogScore);

/// <summary>A dictionary-constrained path through caller-supplied glyph candidates.</summary>
public sealed record LegacyLexicalMatch(ReadOnlyMemory<ushort> Labels, double LogScore, int TerminalState);

/// <summary>
/// Original bounded beam search over a legacy packed lexicon. Input must already
/// be segmented and mapped to the lexicon alphabet. This does not restore native
/// cursive segmentation, language-model costs or the SDK word recognition engine.
/// </summary>
public static class LegacyLexicalDecoder
{
    public static IReadOnlyList<LegacyLexicalMatch> Decode(LegacyPackedLexicon lexicon,
        IReadOnlyList<ReadOnlyMemory<LegacyLexicalCandidate>> positions,
        int beamWidth = 64, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lexicon); ArgumentNullException.ThrowIfNull(positions);
        if (beamWidth is < 1 or > 256) throw new ArgumentOutOfRangeException(nameof(beamWidth));
        if (positions.Count > lexicon.MaximumWordLength) throw new ArgumentException("Glyph sequence exceeds dictionary word length.", nameof(positions));
        if (positions.Count == 0) return [];
        var beam = new List<Path> { new(0, 0, null, 0) };
        for (int position = 0; position < positions.Count; position++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var choices = positions[position].Span;
            if (choices.Length > 512) throw new ArgumentException("Too many candidates for one glyph.", nameof(positions));
            // A trie gives every state one prefix. Duplicate acoustic candidates
            // can therefore merge by resulting state without losing distinct words.
            var next = new Dictionary<int, Path>();
            foreach (var choice in choices)
            {
                if (!float.IsFinite(choice.LogScore) || choice.LogScore is < -1_000_000 or > 0)
                    throw new ArgumentException("Candidates require finite nonpositive log scores.", nameof(positions));
                foreach (var path in beam)
                {
                    if (!lexicon.TryAdvance(path.State, choice.Label, out int state)) continue;
                    if (position == positions.Count - 1 && !lexicon.IsTerminal(state)) continue;
                    double score = path.Score + choice.LogScore;
                    if (!next.TryGetValue(state, out var previous) || score > previous.Score)
                        next[state] = new(state, choice.Label, path, score);
                }
            }
            if (next.Count == 0) return [];
            beam = next.Values.OrderByDescending(x => x.Score).ThenBy(x => x.State).Take(beamWidth).ToList();
        }
        return beam.Where(x => lexicon.IsTerminal(x.State)).Select(path =>
        {
            var labels = new ushort[positions.Count]; Path? current = path;
            for (int i = labels.Length - 1; i >= 0; i--)
            {
                labels[i] = current!.Label; current = current.Parent;
            }
            return new LegacyLexicalMatch(labels, path.Score, path.State);
        }).ToArray();
    }

    private sealed record Path(int State, ushort Label, Path? Parent, double Score);
}
