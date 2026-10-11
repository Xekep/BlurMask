namespace InkRecognition;

/// <summary>A dictionary match retaining its variable prepared-ink partition.</summary>
public sealed record LegacyVariableWordMatch(ReadOnlyMemory<ushort> Labels,
    IReadOnlyList<LegacyCursiveCandidateGroup> Glyphs, int AcousticCost, int Cost, int TerminalState);

public sealed record LegacyVariableWordSearchResult(IReadOnlyList<LegacyVariableWordMatch> Matches,
    int ScoredGroups, long ExpandedChoices);

/// <summary>
/// Original bounded dictionary beam over the checked ordinary variable-cut lattice.
/// No fixed partition is chosen before recognition. Supplied glyph costs must be
/// independent of the prefix; native path-dependent candidate corrections are
/// outside this search. Terminal scoring precedes final beam pruning.
/// </summary>
public static class LegacyVariableWordSearch
{
    public static LegacyVariableWordSearchResult Search(LegacyPackedLexicon lexicon,
        LegacyCursiveCandidateLattice lattice,
        Func<LegacyCursiveCandidateGroup, ReadOnlyMemory<LegacyIntegerLexicalCandidate>> scoreGroup,
        int beamWidth = 64, long maximumExpandedChoices = 2_000_000,
        Func<int, int, int?>? scoreTerminal = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lexicon); ArgumentNullException.ThrowIfNull(lattice);
        ArgumentNullException.ThrowIfNull(scoreGroup); cancellationToken.ThrowIfCancellationRequested();
        if (beamWidth is < 1 or > 256) throw new ArgumentOutOfRangeException(nameof(beamWidth));
        if (maximumExpandedChoices is < 1 or > 100_000_000) throw new ArgumentOutOfRangeException(nameof(maximumExpandedChoices));
        var frontiers = new List<Path>[lattice.NodeCount + 1];
        frontiers[0] = [new(0, 0, null, null, 0, 0, 0)];
        int scoredGroups = 0; long expanded = 0;
        for (int end = 0; end < lattice.NodeCount; end++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool last = end == lattice.NodeCount - 1;
            var next = new Dictionary<int, Path>();
            foreach (var group in lattice.GetNode(end))
            {
                var prefixes = frontiers[group.FirstAtomIndex];
                if (prefixes is null || prefixes.Count == 0) continue;
                // Score arcs separately: equal geometry/group IDs can still
                // have different native arc flags and category corrections.
                ReadOnlyMemory<LegacyIntegerLexicalCandidate> choices = scoreGroup(group).ToArray(); scoredGroups++;
                if (choices.Length > 512) throw new ArgumentException("A scored group exceeds512 lexical choices.", nameof(scoreGroup));
                foreach (var choice in choices.Span)
                    if (choice.Cost is < -LegacyAcousticFixedLog.ImpossibleCost or > LegacyAcousticFixedLog.ImpossibleCost)
                        throw new ArgumentException("Group costs exceed the explicit signed search domain.", nameof(scoreGroup));
                foreach (var prefix in prefixes)
                {
                    if (prefix.Depth >= lexicon.MaximumWordLength) continue;
                    foreach (var choice in choices.Span)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (++expanded > maximumExpandedChoices)
                            throw new InvalidOperationException("Variable word search exceeded its explicit expansion budget.");
                        if (choice.Cost == LegacyAcousticFixedLog.ImpossibleCost ||
                            !lexicon.TryAdvance(prefix.State, choice.Label, out int state) ||
                            last && !lexicon.IsTerminal(state)) continue;
                        long cumulative = (long)prefix.Acoustic + choice.Cost;
                        if (cumulative < -LegacyAcousticFixedLog.ImpossibleCost || cumulative >= LegacyAcousticFixedLog.ImpossibleCost) continue;
                        int acoustic = (int)cumulative;
                        if (next.TryGetValue(state, out var previous) && previous.Acoustic <= acoustic) continue;
                        int? cost = last && scoreTerminal is not null ? scoreTerminal(state, acoustic) : acoustic;
                        if (cost is null) continue;
                        next[state] = new(state, choice.Label, prefix, group, acoustic, cost.Value, prefix.Depth + 1);
                    }
                }
            }
            // One frontier per covered-atom count. A dead prefix does not abort
            // the word: a later candidate may span across that unreachable cut.
            frontiers[end + 1] = next.Values.OrderBy(p => p.Cost).ThenBy(p => p.State).Take(beamWidth).ToList();
        }
        var matches = frontiers[^1].Select(path =>
        {
            var labels = new ushort[path.Depth]; var glyphs = new LegacyCursiveCandidateGroup[path.Depth];
            Path? current = path;
            for (int i = path.Depth - 1; i >= 0; i--)
            { labels[i] = current!.Label; glyphs[i] = current.Group!; current = current.Parent; }
            return new LegacyVariableWordMatch(labels, Array.AsReadOnly(glyphs), path.Acoustic, path.Cost, path.State);
        }).ToArray();
        return new(Array.AsReadOnly(matches), scoredGroups, expanded);
    }

    private sealed record Path(int State, ushort Label, Path? Parent,
        LegacyCursiveCandidateGroup? Group, int Acoustic, int Cost, int Depth);
}
