using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Ordinary203c90 language gate over the owned graph188 boundary
/// producer, its resource span cap, and actual stored88 rows. This does not
/// select rows, construct source headers, prune the graph or run a scheduler.</summary>
public sealed class LegacyNativeWordLanguageGate
{
    private readonly LegacyOrdinaryRecognitionGraph graph;
    private readonly ushort replacement;

    public LegacyNativeWordLanguageGate(LegacyRussianModelBundle selectedModel,
        LegacyOrdinaryCursiveCandidateGraph graph)
        :this(selectedModel,LegacyOrdinaryRecognitionGraph.FromSameSource(graph)) { }

    internal LegacyNativeWordLanguageGate(LegacyRussianModelBundle selectedModel,LegacyOrdinaryRecognitionGraph graph)
    {
        ArgumentNullException.ThrowIfNull(selectedModel);
        ArgumentNullException.ThrowIfNull(graph);
        if (graph.MaximumAtoms != selectedModel.CursiveSpanProfile.MaximumAtoms)
            throw new ArgumentException("Language lookahead must own the selected resource graph span cap.");
        if (!selectedModel.AcousticAlphabet.TryGetLabel("\ufffd", out replacement))
            throw new NotSupportedException("Selected acoustic alphabet must contain the native replacement symbol.");
        this.graph = graph;
    }

    /// <summary>Frontend222.virtual100=152350. Ordinary graph188=1acf70
    /// reads bit2 of the actual arc flag byte, independently of group kind.</summary>
    public bool HasBoundary(int currentNode, int previousNode, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if ((uint)currentNode >= (uint)graph.NodeCount || previousNode < -1 || previousNode >= graph.NodeCount)
            throw new ArgumentOutOfRangeException(nameof(currentNode));
        var arcs = graph.GetNode(currentNode);
        if (arcs.Count > ushort.MaxValue) throw new NotSupportedException("Native arc ordinal is a ushort.");
        foreach (var arc in arcs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (unchecked(currentNode - arc.Span - 1) == previousNode && (arc.Flags & 4) != 0) return true;
        }
        return false;
    }

    /// <summary>Frontend222.virtual108=152400. Graph30=1a75d0 reads its
    /// retained raw8 cap and returns min(nextNode+1, cap).</summary>
    public bool HasFollowingBoundary(int previousNode, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if ((uint)previousNode >= (uint)graph.NodeCount) throw new ArgumentOutOfRangeException(nameof(previousNode));
        int nextNode = previousNode + 1;
        int extent = Math.Min(nextNode + 1, graph.MaximumAtoms);
        for (int offset = 0; offset < extent; offset++, nextNode++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (nextNode >= graph.NodeCount) return false;
            if (HasBoundary(nextNode, previousNode, cancellationToken)) return true;
        }
        return false;
    }

    /// <summary>param3 is the stored-row/source node; param4=-1 selects
    /// primary rows and frontend108, otherwise frontend100(param4,param3)
    /// and secondary rows. Packed22e8, knowledge54 and source headers retain
    /// their actual caller-owned values; no defaults or winner are inferred.</summary>
    public bool ShouldRun(int storedNode, int currentNode, uint packedController22e8,
        ushort primaryCount, ReadOnlySpan<byte> storedRows88, int knowledgeDistance54,
        ReadOnlySpan<uint> sourceHeaders, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if ((uint)storedNode >= (uint)graph.NodeCount || currentNode < -1 || currentNode >= graph.NodeCount)
            throw new ArgumentOutOfRangeException(nameof(storedNode));
        //203c90 returns immediately for this bit and does not touch row storage.
        if ((packedController22e8 & 8) != 0) return true;
        if (currentNode == -1 ? HasFollowingBoundary(storedNode, cancellationToken) :
            HasBoundary(currentNode, storedNode, cancellationToken)) return true;
        if (storedRows88.Length % 88 != 0 || storedRows88.Length / 88 > ushort.MaxValue ||
            primaryCount > storedRows88.Length / 88 || sourceHeaders.Length is < 1 or > 256)
            throw new ArgumentException("Actual bounded stored88 row extents and indexed source headers required.");
        int first = currentNode == -1 ? 0 : primaryCount;
        int end = currentNode == -1 ? primaryCount : storedRows88.Length / 88;
        for (int ordinal = first; ordinal < end; ordinal++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var row = storedRows88.Slice(ordinal * 88, 88);
            //Literal equality, not a >= impossible-cost truncation.
            if (BinaryPrimitives.ReadInt32LittleEndian(row[16..]) == 0x4000000) break;
            if (row[4] >= sourceHeaders.Length) throw new ArgumentException("Visited row source ID must own its descriptor header.");
            uint kind = sourceHeaders[row[4]] & 0x1e00;
            int anchor = BinaryPrimitives.ReadInt32LittleEndian(row[56..]);
            bool elapsed = unchecked(storedNode - anchor) >= knowledgeDistance54 && kind is >= 0x200 and <= 0xa00;
            if (BinaryPrimitives.ReadUInt16LittleEndian(row[36..]) == replacement || elapsed) return true;
        }
        return false;
    }
}
