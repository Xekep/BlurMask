namespace InkRecognition;

/// <summary>
/// Zero-based terminal preorder IDs for the checked ordered type2b4 tree.
/// Native graph construction 1802489e0 assigns this order. This mapping does
/// not account for other graph encodings, runtime vocabulary permutations or
/// contextual aliases; it never treats breadth-first node IDs as word IDs.
/// </summary>
public sealed class LegacyLexiconWordIndex
{
    private readonly LegacyPackedLexicon _graph;
    private readonly int[] _wordIds;
    public int WordCount { get; }
    public int StorageBytes => checked(_wordIds.Length * 4);

    public LegacyLexiconWordIndex(LegacyPackedLexicon graph, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(graph); cancellationToken.ThrowIfCancellationRequested();
        _graph = graph; _wordIds = new int[graph.NodeCount]; Array.Fill(_wordIds, -1);
        Span<int> first = stackalloc int[257], end = stackalloc int[257];
        int[] scratch = new int[16];
        (int First, int End) Children(int state)
        {
            int count = graph.GetChildCount(state);
            if (count > scratch.Length) scratch = new int[count];
            graph.CopyChildren(state, scratch);
            int start = count == 0 ? 0 : scratch[0];
            // The validated type2b4 tree stores contiguous BFS child ranges.
            return (start, checked(start + count));
        }
        (first[0], end[0]) = Children(0);
        int depth = 0, words = 0, visited = 1;
        while (depth >= 0)
        {
            if ((visited & 1023) == 0) cancellationToken.ThrowIfCancellationRequested();
            if (first[depth] == end[depth]) { depth--; continue; }
            int state = first[depth]++; visited++;
            if (graph.IsTerminal(state)) _wordIds[state] = words++;
            if (graph.GetChildCount(state) == 0) continue;
            if (depth + 1 >= graph.MaximumWordLength || depth + 1 >= 256)
                throw new InvalidDataException("Dictionary tree exceeds declared maximum word length.");
            depth++; (first[depth], end[depth]) = Children(state);
        }
        if (visited != graph.NodeCount || words != graph.StoredTerminalCount)
            throw new InvalidDataException("Terminal enumeration differs from validated graph bounds.");
        WordCount = words;
    }

    /// <summary>Return the terminal's stored enumeration ID; nonterminals return false.</summary>
    public bool TryGetWordId(int graphState, out int wordId)
    {
        if ((uint)graphState >= _wordIds.Length) throw new ArgumentOutOfRangeException(nameof(graphState));
        wordId = _wordIds[graphState]; return wordId >= 0;
    }

    public bool TryGetWordId(ReadOnlySpan<ushort> labels, out int wordId)
    {
        wordId = -1;
        if (labels.Length is 0 || labels.Length > _graph.MaximumWordLength) return false;
        int state = 0;
        foreach (ushort label in labels) if (!_graph.TryAdvance(state, label, out state)) return false;
        return TryGetWordId(state, out wordId);
    }
}
