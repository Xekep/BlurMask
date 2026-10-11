namespace InkRecognition;

/// <summary>One native 2e3 proposal. Ordering 0/1/2 uses mean/top/bottom Y; 3 uses physical order.</summary>
public sealed record LegacyNeuralLineProposal(int WindowIndex, int Ordering,
    ReadOnlyMemory<int> StrokeIndices)
{
    public bool UsesSpatialClassifier => Ordering != 3;
}

public readonly record struct LegacyNeuralLinePair(int FirstStroke, int SecondStroke,
    float Confidence, bool LabelOne);

public sealed record LegacyNeuralLineProposalBatch(float WindowWidth,
    IReadOnlyList<LegacyNeuralLineProposal> Proposals);

/// <summary>
/// The ordinary 072610 horizontal-window and 073de0 proposal producers, followed
/// by the checked 07d7c0 neural consumer. This produces pair evidence, not final
/// rows: native vertical sweeps and 07c160 group merging remain separate stages.
/// Scale and active physical strokes are supplied explicitly.
/// </summary>
public static class LegacyNeuralLineProposals
{
    public static LegacyNeuralLineProposalBatch Prepare(
        IReadOnlyList<LegacyLineStrokeStatistics> strokes, ReadOnlySpan<bool> active,
        float nominalWindowWidth, float overlap, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(strokes);
        cancellationToken.ThrowIfCancellationRequested();
        if (strokes.Count is < 1 or > 256 || active.Length != strokes.Count ||
            !float.IsFinite(nominalWindowWidth) || nominalWindowWidth <= 0 ||
            !float.IsFinite(overlap) || overlap < 0 || overlap >= 1)
            throw new ArgumentException("Expected bounded prepared strokes, active flags and positive window width.");
        float left = float.MaxValue, right = -float.MaxValue;
        int activeCount = 0;
        for (int i = 0; i < strokes.Count; i++)
        {
            if (strokes[i] is null) throw new ArgumentException("Null stroke record.");
            if (!active[i]) continue;
            activeCount++;
            left = MathF.Min(left, strokes[i].Values[0]);
            right = MathF.Max(right, strokes[i].Values[2]);
        }
        if (activeCount == 0) throw new ArgumentException("At least one active physical stroke is required.");
        float extent = (right - left) + .01f;
        float width = MathF.Min(extent, nominalWindowWidth);
        float step = (1f - overlap) * width;
        float quotient = (extent - (width - step)) / step;
        if (!float.IsFinite(quotient) || quotient < 0 || quotient >= 512 || !float.IsFinite(step) || step <= 0)
            throw new ArgumentException("Native window count exceeds the bounded supported profile.");
        int regularCount = (int)MathF.Floor(quotient);
        int tail = nominalWindowWidth <= extent ? 0 : -1;
        if (tail >= 0 && overlap <= quotient - regularCount) tail = 1;
        if (regularCount == 0) regularCount = 1;
        var windows = new List<(float Left, float Right)>();
        for (int i = 0; i < regularCount; i++)
        {
            float start = (float)i * step + left;
            windows.Add((start, start + width));
        }
        if (tail == 0) windows[^1] = (windows[^1].Left, right);
        else if (tail > 0) windows.Add((right - width, right));
        if (windows.Count > 1) windows.Add((left, right));
        var proposals = new List<LegacyNeuralLineProposal>();
        for (int w = 0; w < windows.Count; w++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var ids = new List<int>();
            for (int s = 0; s < strokes.Count; s++)
                if (active[s] && strokes[s].Values[0] <= windows[w].Right && windows[w].Left <= strokes[s].Values[2]) ids.Add(s);
            if (ids.Count == 0) continue;
            if (w != windows.Count - 1 || windows.Count == 1)
                for (int ordering = 0; ordering < 3; ordering++)
                {
                    int[] sorted = ids.ToArray();
                    SortNative(sorted, strokes, ordering == 0 ? 5 : ordering == 1 ? 1 : 3, 0, sorted.Length - 1, cancellationToken);
                    proposals.Add(new(w, ordering, sorted));
                }
            proposals.Add(new(w, 3, ids.ToArray()));
        }
        return new(width, proposals.AsReadOnly());
    }

    /// <summary>
    /// Uses the 30-state network for Y-sorted proposals and 50-state for physical
    /// order. Crucially native inference skips the first self-pair feature row.
    /// No vertical pruning or final row interpretation is substituted here.
    /// </summary>
    public static LegacyNeuralLinePair[] Score(
        IReadOnlyList<LegacyLineStrokeStatistics> strokes, LegacyNeuralLineProposalBatch batch,
        float scale, PreparedLineBoundaryClassifier spatial30, PreparedLineBoundaryClassifier physical50,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batch); ArgumentNullException.ThrowIfNull(spatial30);
        ArgumentNullException.ThrowIfNull(physical50); ArgumentNullException.ThrowIfNull(strokes);
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(batch.Proposals);
        if (spatial30.HiddenWidth != 30 || physical50.HiddenWidth != 50 || !float.IsFinite(scale) || scale <= 0 ||
            !float.IsFinite(batch.WindowWidth) || batch.WindowWidth <= 0 ||
            strokes.Count is < 1 or > 256 || batch.Proposals.Count > 2052)
            throw new ArgumentException("Expected the checked 30/50 classifier pair and bounded prepared proposals.");
        var pairs = new List<LegacyNeuralLinePair>();
        var pairIndices = new Dictionary<(int, int), int>();
        foreach (var proposal in batch.Proposals)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(proposal);
            int[] ids = proposal.StrokeIndices.ToArray();
            if (ids.Length is < 1 or > 256 || ids.Distinct().Count() != ids.Length || proposal.Ordering is < 0 or > 3 ||
                proposal.WindowIndex is < 0 or > 512 || ids.Any(i => i < 0 || i >= strokes.Count || strokes[i] is null))
                throw new ArgumentException("Invalid physical stroke proposal.");
            if (ids.Length == 1) continue;
            float[] features = LegacyLinePairFeatures.Extract(strokes, ids, batch.WindowWidth, scale,
                cancellationToken: cancellationToken);
            var network = proposal.UsesSpatialClassifier ? spatial30 : physical50;
            float[] probabilities = network.Score(features.AsSpan(LegacyLinePairFeatures.Width), ids.Length - 1,
                cancellationToken: cancellationToken);
            for (int row = 0; row < ids.Length - 1; row++)
            {
                // Native virtual C0 requests confidence of the winning class.
                // Ties preserve the lower class index (strict maximum update).
                bool one = probabilities[row * 2] < probabilities[row * 2 + 1];
                Merge(pairs, pairIndices, ids[row], ids[row + 1], one ? probabilities[row * 2 + 1] : probabilities[row * 2], one);
            }
        }
        return pairs.ToArray();
    }

    /// <summary>07bc90: undirected pair identity, first encounter order, strict larger-confidence replacement.</summary>
    public static LegacyNeuralLinePair[] Aggregate(IReadOnlyList<LegacyNeuralLinePair> evidence,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        cancellationToken.ThrowIfCancellationRequested();
        if (evidence.Count > 65536) throw new ArgumentException("Too much pair evidence.");
        var output = new List<LegacyNeuralLinePair>();
        var pairIndices = new Dictionary<(int, int), int>();
        foreach (var pair in evidence)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (pair.FirstStroke is < 0 or > 255 || pair.SecondStroke is < 0 or > 255 || pair.FirstStroke == pair.SecondStroke ||
                !float.IsFinite(pair.Confidence) || pair.Confidence is < 0 or > 1)
                throw new ArgumentException("Invalid pair evidence.");
            Merge(output, pairIndices, pair.FirstStroke, pair.SecondStroke, pair.Confidence, pair.LabelOne);
        }
        return output.ToArray();
    }

    private static void Merge(List<LegacyNeuralLinePair> pairs, Dictionary<(int, int), int> pairIndices,
        int a, int b, float confidence, bool one)
    {
        if (a > b) (a, b) = (b, a);
        if (pairIndices.TryGetValue((a, b), out int i))
        {
            if (pairs[i].Confidence < confidence) pairs[i] = new(a, b, confidence, one);
            return;
        }
        pairIndices.Add((a, b), pairs.Count);
        pairs.Add(new(a, b, confidence, one));
    }

    // 072550 middle-pivot quicksort swaps equal keys. A stable sort changes NN
    // sequence context when strokes have equal means/edges, so retain this order.
    private static void SortNative(int[] ids, IReadOnlyList<LegacyLineStrokeStatistics> strokes, int channel,
        int first, int last, CancellationToken cancellationToken)
    {
        while (first < last)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int l = first, r = last; float pivot = strokes[ids[first + (last - first) / 2]].Values[channel];
            do
            {
                while (strokes[ids[l]].Values[channel] < pivot) l++;
                while (pivot < strokes[ids[r]].Values[channel]) r--;
                if (l <= r) { (ids[l], ids[r]) = (ids[r], ids[l]); l++; r--; }
            } while (l <= r);
            if (first < r) SortNative(ids, strokes, channel, first, r, cancellationToken);
            first = l;
        }
    }
}
