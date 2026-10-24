using System.Numerics;

namespace InkRecognition;

/// <summary>
/// Library-owned fresh append-only1F40 input identity/sidecars. Point storage is
/// shared through the immutable ORIGINAL geometry owner, not normalized XY.
/// This is not an existing RawInkDocument selection or a processed graph/request.
/// </summary>
internal sealed class LegacyMathAkOriginalInputFrame
{
    private readonly LegacyMathAkSourceGeometry original;
    private readonly object documentIdentity = new();
    private readonly int[] ends, strokeIds, strokeById, itemKinds;
    private readonly uint[] activeWords;
    internal LegacyMathAkSourceGeometry OriginalOwner => original;
    internal object DocumentIdentity => documentIdentity;
    internal int ItemCount => strokeById.Length;
    internal int StrokeCount => strokeIds.Length;
    internal int PointCount => original.PointCount;
    internal int ActiveWordCount => activeWords.Length;
    //006A40 reserves one extra zero processed word exactly at32-item boundaries.
    internal int ProcessedWordCount => (ItemCount >> 5) + 1;
    private LegacyMathAkOriginalInputFrame(LegacyMathAkSourceGeometry original,
        int[] ends, int[] ids, int[] reverse, int[] kinds, uint[] active)
    { this.original = original; this.ends = ends; strokeIds = ids; strokeById = reverse;
      itemKinds = kinds; activeWords = active; }

    internal static LegacyMathAkOriginalInputFrame CreateFresh(
        LegacyMathAkSourceGeometry original, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(original); token.ThrowIfCancellationRequested();
        int count = original.StrokeCount;
        if (count is < 1 or > LegacyMathAkSourceGeometry.MaximumStrokes ||
            original.PointCount is < 1 or > LegacyMathAkSourceGeometry.MaximumPoints)
            throw new InvalidOperationException("Original input owner cardinality invariant failed.");
        int[] cumulative = original.CopyCumulativeEnds(token);
        if (cumulative.Length != count) throw new InvalidOperationException("Original input ends invariant failed.");
        int previous = 0;
        foreach (int end in cumulative)
        {
            if (end <= previous || end > original.PointCount)
                throw new InvalidOperationException("Original input append range invariant failed.");
            previous = end;
        }
        if (previous != original.PointCount) throw new InvalidOperationException("Original point header mismatch.");
        var ids = new int[count]; var reverse = new int[count]; var kinds = new int[count];
        var active = new uint[((count - 1) >> 5) + 1];
        int itemCount = 0, strokeCount = 0, pointEnd = 0;
        for (int s = 0; s < count; s++)
        {
            token.ThrowIfCancellationRequested();
            //001DB0 starts all counts at0.002900 captures old+48 and old+2C,
            //appends the actual stroke(034810 bit-copy), then appends its ID,
            //reverse ordinal, kind-1 and sets only the new bit.001A20 zeroes
            //new mask words; managed fixed-size zeroed storage preserves values.
            int id = itemCount, ordinal = strokeCount;
            int appended = cumulative[s] - pointEnd;
            if (appended <= 0) throw new InvalidOperationException("Empty native append not admitted.");
            pointEnd += appended; strokeCount++;
            ids[s] = id; reverse[id] = ordinal; kinds[id] = -1; itemCount++;
            active[id >> 5] |= 1u << (id & 31);
        }
        if (itemCount != count || strokeCount != count || pointEnd != original.PointCount)
            throw new InvalidOperationException("Fresh input transition invariant failed.");
        token.ThrowIfCancellationRequested();
        var frame = new LegacyMathAkOriginalInputFrame(original, cumulative, ids, reverse, kinds, active);
        token.ThrowIfCancellationRequested(); return frame;
    }

    internal int ReadStrokeId(int ordinal) => strokeIds[ordinal];
    internal int ReadStrokeOrdinal(int id) => strokeById[id];
    internal int ReadItemKind(int id) => itemKinds[id];
    internal bool IsActive(int id)
    {
        if ((uint)id >= (uint)ItemCount) throw new ArgumentOutOfRangeException(nameof(id));
        return (activeWords[id >> 5] & (1u << (id & 31))) != 0;
    }
    internal Vector2[][] CopyOriginalStrokes(CancellationToken token) => original.CopyOriginalStrokes(token);
    internal int[] CopyEnds(CancellationToken token) => Clone(ends, token);
    internal int[] CopyStrokeIds(CancellationToken token) => Clone(strokeIds, token);
    internal int[] CopyStrokeById(CancellationToken token) => Clone(strokeById, token);
    internal int[] CopyItemKinds(CancellationToken token) => Clone(itemKinds, token);
    internal uint[] CopyActiveWords(CancellationToken token) => Clone(activeWords, token);
    private static T[] Clone<T>(T[] source, CancellationToken token)
    { token.ThrowIfCancellationRequested(); var result = (T[])source.Clone(); token.ThrowIfCancellationRequested(); return result; }
}
