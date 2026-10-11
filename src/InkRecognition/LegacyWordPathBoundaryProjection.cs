using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Exact checked fields of a native0x58 word-path record.</summary>
public readonly record struct LegacyPreparedWordPathReference(int EndingNodeIndex,
    ushort CategoryBits, sbyte SourceSpan)
{
    public static LegacyPreparedWordPathReference Read(ReadOnlySpan<byte> record, int endingNodeIndex)
    {
        if (record.Length != 0x58 || endingNodeIndex is < -1 or > 1_000_000)
            throw new ArgumentException("One native0x58 word record and bounded ending node required.");
        return new(endingNodeIndex, BinaryPrimitives.ReadUInt16LittleEndian(record[0x1a..]),
            unchecked((sbyte)record[0x20]));
    }
}

/// <summary>
/// Frontend222 virtualE8/152280 over primary ordinary candidate cells:
/// node/arc ->group ->source input descriptor ->descriptor+10. Supplemental
/// remap cells and cross-source graph mutations are not inferred.
/// </summary>
public sealed class LegacyPreparedSourceBoundarySnapshot
{
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private readonly int[] boundaries;
    private readonly ushort[] inputs;

    public LegacyPreparedSourceBoundarySnapshot(LegacyOrdinaryCursiveCandidateGraph graph,
        ReadOnlySpan<byte> sourceInputDescriptors, ReadOnlySpan<ushort> groupInputIndices)
    {
        ArgumentNullException.ThrowIfNull(graph);
        if (sourceInputDescriptors.Length is < 0x58 or > 256 * 0x58 || sourceInputDescriptors.Length % 0x58 != 0 ||
            groupInputIndices.Length != graph.Groups.Count)
            throw new ArgumentException("Native input descriptor records and one input ID per graph group required.");
        boundaries = new int[sourceInputDescriptors.Length / 0x58];
        for (int input = 0; input < boundaries.Length; input++)
            boundaries[input] = BinaryPrimitives.ReadInt32LittleEndian(sourceInputDescriptors[(input * 0x58 + 0x10)..]);
        foreach (ushort input in groupInputIndices)
            if (input >= boundaries.Length) throw new ArgumentException("Group references an unavailable source input descriptor.");
        this.graph = graph; inputs = groupInputIndices.ToArray();
    }

    /// <summary>
    /// Bounded fresh ordinary/no custom metadata ingestion:163970 initializes
    /// source94=-1;167210 reuses the same low-type0 input descriptor and writes
    /// that field at+10. The single-source19f850/1a3b30 graph retains input ID0.
    /// Existing recognized text, explicit scope setters and lifecycle mutation
    /// require supplied descriptor records instead of this fresh factory.
    /// </summary>
    public static LegacyPreparedSourceBoundarySnapshot CreateFreshOrdinary(
        LegacyOrdinaryCursiveCutSource source, LegacyOrdinaryCursiveCandidateGraph graph)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(graph);
        var counts = source.Cuts.PreparedInk.StrokePointCounts.Span;
        foreach (var group in graph.Groups) foreach (var range in group.Ranges.Span)
            if ((uint)range.StrokeIndex >= (uint)counts.Length || range.StartPoint < 0 ||
                range.EndPointExclusive <= range.StartPoint || range.EndPointExclusive > counts[range.StrokeIndex])
                throw new ArgumentException("Graph ownership must address the supplied fresh ordinary source.");
        var descriptor = new byte[0x58];
        BinaryPrimitives.WriteInt32LittleEndian(descriptor.AsSpan(0x10), -1);
        return new(graph, descriptor, new ushort[graph.Groups.Count]);
    }

    public int GetGroupBoundary(int groupId)
    {
        if (groupId == -1) return -1; //152280's missing-group branch.
        if ((uint)groupId >= (uint)inputs.Length) throw new ArgumentOutOfRangeException(nameof(groupId));
        return boundaries[inputs[groupId]];
    }
    public int GetBoundary(int nodeIndex, int arcIndex)
    {
        var node = graph.GetNode(nodeIndex);
        if ((uint)arcIndex >= (uint)node.Count) throw new ArgumentOutOfRangeException(nameof(arcIndex));
        return GetGroupBoundary(node[arcIndex].GroupId);
    }
}

public readonly record struct LegacyWordPathBoundaryProjection(int MappedCategory,
    int SourceStartNode, int PreviousNode, int BoundaryDifference, int AddedCost);

/// <summary>
/// Checked204290 previous-word category/source-boundary block only. It consumes
/// an actual prepared record reference and supplied source lookup; no previous
/// record selection, winner, confidence or category production is synthesized.
/// </summary>
public static class LegacyWordPathBoundaryCost
{
    public static LegacyWordPathBoundaryProjection Evaluate(LegacyPreparedWordPathReference previousWord,
        LegacyCursiveCategoryMapper categories, int availableSourceNodes, bool nativeMode22f6,
        byte packedPathType, Func<int, int, int>? inputBoundaryLookup,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(categories); cancellationToken.ThrowIfCancellationRequested();
        if (availableSourceNodes is < 0 or > 1_000_000 || previousWord.EndingNodeIndex is < -1 or > 1_000_000)
            throw new ArgumentOutOfRangeException(nameof(availableSourceNodes));
        if (nativeMode22f6 || (packedPathType & 0x1c) <= 7) return new(-1, -1, -1, 0, 0);
        int mapped = categories.GetBeamFirstId((byte)previousWord.CategoryBits);
        int start = previousWord.EndingNodeIndex - previousWord.SourceSpan;
        int preceding = start - 1, difference = 0;
        if (preceding >= 0 && start < availableSourceNodes && inputBoundaryLookup is not null)
        {
            int previousBoundary = inputBoundaryLookup(preceding, 0);
            if (previousBoundary != -1)
            {
                cancellationToken.ThrowIfCancellationRequested();
                difference = unchecked(inputBoundaryLookup(start, 0) - previousBoundary);
            }
        }
        int added = (mapped is 2 or 4) && (preceding == -1 || difference != 0) ? 8192 : 0;
        return new(mapped, start, preceding, difference, added);
    }
}
