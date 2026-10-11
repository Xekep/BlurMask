namespace InkRecognition;

/// <summary>One caller-scored glyph hypothesis between two ordered ink cut positions.</summary>
public readonly record struct LegacyLexicalArc(int Start, int End, ushort Label, float LogScore);

/// <summary>A dictionary word and its chosen ink cuts, including the first and final cut.</summary>
public sealed record LegacyLexicalLatticeMatch(ReadOnlyMemory<ushort> Labels,
    ReadOnlyMemory<int> Cuts, double LogScore, int TerminalState);

/// <summary>
/// Original bounded dictionary beam search over alternative glyph boundaries.
/// Arcs must already contain extracted/scored glyphs in the lexical alphabet.
/// This decoder does not generate cuts or reproduce native cursive segmentation.
/// </summary>
public static class LegacyLexicalLatticeDecoder
{
    /// <summary>
    /// Cover all ink from cut0 through finalCut with nonoverlapping forward arcs.
    /// Merge duplicate word prefixes at the same cut, keep bounded beams, then
    /// return terminal dictionary words. Scores are caller-supplied additive log
    /// costs: segmentation/length calibration must be provided by the caller.
    /// </summary>
    public static IReadOnlyList<LegacyLexicalLatticeMatch> Decode(LegacyPackedLexicon lexicon,
        IReadOnlyList<LegacyLexicalArc> arcs, int finalCut, int beamWidth = 64,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lexicon); ArgumentNullException.ThrowIfNull(arcs);
        cancellationToken.ThrowIfCancellationRequested();
        if (finalCut is < 1 or > 128) throw new ArgumentOutOfRangeException(nameof(finalCut));
        if (beamWidth is < 1 or > 128) throw new ArgumentOutOfRangeException(nameof(beamWidth));
        if (arcs.Count > 16384) throw new ArgumentException("The cut lattice exceeds 16384 arcs.", nameof(arcs));
        var outgoing = new List<LegacyLexicalArc>?[finalCut];
        foreach (var arc in arcs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (arc.Start < 0 || arc.Start >= arc.End || arc.End > finalCut ||
                !float.IsFinite(arc.LogScore) || arc.LogScore is < -1_000_000 or > 0)
                throw new ArgumentException("Lattice arcs require forward cuts and finite nonpositive log scores.", nameof(arcs));
            var choices = outgoing[arc.Start] ??= [];
            if (choices.Count == 128) throw new ArgumentException("One cut exceeds 128 glyph hypotheses.", nameof(arcs));
            choices.Add(arc);
        }
        var beams = new Dictionary<int, Path>?[finalCut + 1];
        beams[0] = new() { [0] = new(0, 0, 0, 0, null, 0) };
        for (int cut = 0; cut < finalCut; cut++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (beams[cut] is not { Count: > 0 } bucket || outgoing[cut] is not { Count: > 0 } choices) continue;
            var current = Best(bucket, beamWidth).ToArray();
            beams[cut] = null; // Paths retain their own parents; the map is no longer needed.
            foreach (var arc in choices)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var destination = beams[arc.End] ??= [];
                foreach (var path in current)
                {
                    if (path.Length == lexicon.MaximumWordLength || !lexicon.TryAdvance(path.State, arc.Label, out int state)) continue;
                    if (arc.End == finalCut && !lexicon.IsTerminal(state)) continue;
                    double score = path.Score + arc.LogScore;
                    if (!destination.TryGetValue(state, out var previous) || score > previous.Score)
                        destination[state] = new(state, arc.Label, arc.End, path.Length + 1, path, score);
                }
                // Waiting cuts retain at most two beams, rather than every path
                // from every earlier cut. This is approximate bounded beam search.
                if (destination.Count > beamWidth * 2) beams[arc.End] = Trim(destination, beamWidth);
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        if (beams[finalCut] is not { Count: > 0 } completed) return [];
        return Best(completed, beamWidth).Where(path => lexicon.IsTerminal(path.State)).Select(path =>
        {
            var labels = new ushort[path.Length]; var cuts = new int[path.Length + 1];
            Path current = path;
            for (int i = path.Length - 1; i >= 0; i--)
            {
                labels[i] = current.Label; cuts[i + 1] = current.Cut; current = current.Parent!;
            }
            return new LegacyLexicalLatticeMatch(labels, cuts, path.Score, path.State);
        }).ToArray();
    }

    private static IEnumerable<Path> Best(Dictionary<int, Path> paths, int count) =>
        paths.Values.OrderByDescending(path => path.Score).ThenBy(path => path.State).Take(count);
    private static Dictionary<int, Path> Trim(Dictionary<int, Path> paths, int count) =>
        Best(paths, count).ToDictionary(path => path.State);
    private sealed record Path(int State, ushort Label, int Cut, int Length, Path? Parent, double Score);
}
