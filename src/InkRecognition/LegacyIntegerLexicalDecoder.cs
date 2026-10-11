namespace InkRecognition;

public sealed record LegacyIntegerLexicalMatch(ReadOnlyMemory<ushort> Labels,
    int AcousticCost, int Cost, int TerminalState);

/// <summary>
/// Original bounded integer beam search through supplied dictionary labels.
/// It preserves integer costs and allows a caller's pure terminal scorer to
/// filter/rank complete words before final pruning. It is not the SDK search.
/// </summary>
public static class LegacyIntegerLexicalDecoder
{
    /// <param name="scoreTerminal">Optional pure callback: terminal state and cumulative acoustic cost -> accepted total, or null.
    /// For one terminal it must preserve ordering by acoustic cost. No native flags/defaults are inferred.</param>
    public static IReadOnlyList<LegacyIntegerLexicalMatch> Decode(LegacyPackedLexicon lexicon,
        IReadOnlyList<ReadOnlyMemory<LegacyIntegerLexicalCandidate>> positions, int beamWidth = 64,
        Func<int, int, int?>? scoreTerminal = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lexicon); ArgumentNullException.ThrowIfNull(positions);
        cancellationToken.ThrowIfCancellationRequested();
        if (beamWidth is < 1 or > 256) throw new ArgumentOutOfRangeException(nameof(beamWidth));
        if (positions.Count > lexicon.MaximumWordLength) throw new ArgumentException("Sequence exceeds dictionary word length.", nameof(positions));
        foreach (var position in positions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (position.Length > 512) throw new ArgumentException("One position exceeds512 choices.", nameof(positions));
            foreach (var choice in position.Span)
                if (choice.Cost is < 0 or > LegacyAcousticFixedLog.ImpossibleCost)
                    throw new ArgumentException("Expected bounded nonnegative glyph costs.", nameof(positions));
        }
        if (positions.Count == 0) return [];
        var beam = new List<Path> { new(0, 0, null, 0, 0) };
        for (int index = 0; index < positions.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool last = index == positions.Count - 1;
            var next = new Dictionary<int, Path>();
            foreach (var choice in positions[index].Span)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (choice.Cost == LegacyAcousticFixedLog.ImpossibleCost) continue;
                foreach (var path in beam)
                {
                    if (!lexicon.TryAdvance(path.State, choice.Label, out int state) || (last && !lexicon.IsTerminal(state))) continue;
                    int acoustic = checked(path.Acoustic + choice.Cost);
                    int? cost = last && scoreTerminal is not null ? scoreTerminal(state, acoustic) : acoustic;
                    if (cost is null) continue;
                    if (!next.TryGetValue(state, out var prior) || acoustic < prior.Acoustic)
                        next[state] = new(state, choice.Label, path, acoustic, cost.Value);
                }
            }
            if (next.Count == 0) return [];
            beam = next.Values.OrderBy(path => path.Cost).ThenBy(path => path.State).Take(beamWidth).ToList();
        }
        return beam.Select(path =>
        {
            var labels = new ushort[positions.Count]; Path? current = path;
            for (int i = labels.Length - 1; i >= 0; i--) { labels[i] = current!.Label; current = current.Parent; }
            return new LegacyIntegerLexicalMatch(labels, path.Acoustic, path.Cost, path.State);
        }).ToArray();
    }

    private sealed record Path(int State, ushort Label, Path? Parent, int Acoustic, int Cost);
}
