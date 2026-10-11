namespace InkRecognition;

public sealed record LegacyUnknownLineGroups(IReadOnlyList<ReadOnlyMemory<int>> Groups,
    ReadOnlyMemory<int> StrokeGroupIndices);

/// <summary>
/// The empty-prior, ordinary unknown-row branch of Text07c160. All source known
/// and locked flags are false and no supplied guideline rows exist. Native
/// post-selection singleton/attachment pass07a850 is deliberately separate.
/// </summary>
public static class LegacyUnknownLineGroupSelector
{
    public static LegacyUnknownLineGroups Select(IReadOnlyList<LegacyLineStrokeStatistics> records,
        IReadOnlyList<LegacyNeuralLinePair> evidence, float scale, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(records); ArgumentNullException.ThrowIfNull(evidence);
        cancellationToken.ThrowIfCancellationRequested();
        if (records.Count is < 1 or > 256 || !float.IsFinite(scale) || scale <= 0 || scale > 1_000_000 || evidence.Count > 32640 ||
            records.Any(r => r is null)) throw new ArgumentException("Expected bounded prepared ordinary strokes and pair evidence.");
        foreach (var record in records)
            foreach (float value in record.Values)
                if (MathF.Abs(value) > 1e12f) throw new ArgumentException("Prepared record exceeds the supported finite geometry range.");
        var unique = LegacyNeuralLineProposals.Aggregate(evidence, cancellationToken);
        if (unique.Length != evidence.Count || unique.Any(p => p.SecondStroke >= records.Count || p.Confidence <= 0))
            throw new ArgumentException("Expected unique in-range native winning pairs with positive confidence.");
        double pitch = (double)scale * 1.4500000476837158;
        // Native07be60 orders by first endpoint, then second endpoint. Unique
        // endpoints make this order independent of equal-key sort exchanges.
        var costs = unique.OrderBy(p => p.FirstStroke).ThenBy(p => p.SecondStroke).ToDictionary(
            p => (p.FirstStroke, p.SecondStroke), p =>
            {
                float other = 1.0000001192092896f - p.Confidence;
                return p.LabelOne ? (Join: MathF.Log(other), Split: MathF.Log(p.Confidence)) :
                    (Join: MathF.Log(p.Confidence), Split: MathF.Log(other));
            });
        var ordered = unique.ToArray(); SortConfidence(ordered, 0, ordered.Length - 1, cancellationToken);
        int[] membership = Enumerable.Repeat(-1, records.Count).ToArray();
        var groups = new List<List<int>>();
        foreach (var pair in ordered)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (pair.LabelOne) continue;
            int a = pair.FirstStroke, b = pair.SecondStroke;
            int ga = membership[a], gb = membership[b];
            if (ga == gb && ga >= 0) continue;
            // 079280 cannot reject an unknown-row candidate: rowCount=0 and
            // nonnegative centroid imply nearest row index remains−1; a
            // negative centroid is also assigned−1. No adjacent row exists.
            if (ga < 0 && gb < 0)
            {
                if (pair.Confidence <= .99f && !Near([a], [b])) continue;
                groups.Add([a, b]); membership[a] = membership[b] = groups.Count - 1;
            }
            else if (ga < 0)
            {
                if (Join(groups[gb], [a])) { groups[gb].Add(a); membership[a] = gb; }
            }
            else if (gb < 0)
            {
                if (Join(groups[ga], [b])) { groups[ga].Add(b); membership[b] = ga; }
            }
            else
            {
                // Source/group known flags are false in this supported branch.
                float limit = scale * 10f;
                if (records[a].Values[3] - records[a].Values[1] > limit ||
                    records[b].Values[3] - records[b].Values[1] > limit || !Join(groups[ga], groups[gb]) ||
                    (pair.Confidence < .98f && !Near(groups[ga], groups[gb])) || !Compatible(groups[ga], groups[gb])) continue;
                groups[ga].AddRange(groups[gb]);
                foreach (int id in groups[gb]) membership[id] = ga;
                groups.RemoveAt(gb);
                for (int i = 0; i < membership.Length; i++) if (membership[i] >= gb) membership[i]--;
            }
        }
        return new(Array.AsReadOnly(groups.Select(g => new ReadOnlyMemory<int>(g.ToArray())).ToArray()), membership);

        bool Near(IReadOnlyList<int> a, IReadOnlyList<int> b) => Math.Abs(Center(a) - Center(b)) < pitch;
        double Center(IReadOnlyList<int> ids)
        {
            float weight = 0;
            foreach (int id in ids) weight += MathF.Max(records[id].Values[6], 1f);
            double value = 0;
            foreach (int id in ids) value += (MathF.Max(records[id].Values[6], 1f) / (double)weight) * records[id].Values[5];
            return value;
        }
        bool Join(IReadOnlyList<int> a, IReadOnlyList<int> b)
        {
            float join = 0, split = 0;
            foreach (int x in a) foreach (int y in b)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (costs.TryGetValue(x < y ? (x, y) : (y, x), out var c)) { join += c.Join; split += c.Split; }
            }
            if (join != 0 && split != 0 && (join + split >= -5f || MathF.Abs(split - join) < 5.5f)) return Near(a, b);
            return split < join;
        }
        bool Compatible(List<int> a, List<int> b)
        {
            a.Sort(); b.Sort(); // Native079a10 mutates both physical-ID lists.
            int alo = a[(int)MathF.Round((float)a.Count / 20f, MidpointRounding.AwayFromZero)], ahi = a[(int)MathF.Floor(((float)a.Count / 20f) * 19f)];
            int blo = b[(int)MathF.Round((float)b.Count / 20f, MidpointRounding.AwayFromZero)], bhi = b[(int)MathF.Floor(((float)b.Count / 20f) * 19f)];
            if (alo < bhi && blo < ahi) return true;
            var aa = Bounds(a); float aw = aa.Right - aa.Left, ah = aa.Bottom - aa.Top;
            if (aw / ah < 5f) return true;
            var bb = Bounds(b); float bw = bb.Right - bb.Left, bh = bb.Bottom - bb.Top;
            if (!(bw / bh >= 5f && MathF.Abs(ah - bh) <= scale * .25f)) return true;
            float ox = (bw + aw) - (MathF.Max(aa.Right, bb.Right) - MathF.Min(aa.Left, bb.Left));
            if (ox <= 0 || MathF.Max(ox / bw, ox / aw) < .75f) return true;
            float oy = (bh + ah) - (MathF.Max(aa.Bottom, bb.Bottom) - MathF.Min(aa.Top, bb.Top));
            return MathF.Max(oy / bh, oy / ah) >= .75f;
        }
        (float Left, float Top, float Right, float Bottom) Bounds(IReadOnlyList<int> ids)
        {
            float left = float.MaxValue, top = float.MaxValue, right = -float.MaxValue, bottom = -float.MaxValue;
            foreach (int id in ids)
            {
                var f = records[id].Values; left = MathF.Min(left, f[0]); top = MathF.Min(top, f[1]);
                right = MathF.Max(right, f[2]); bottom = MathF.Max(bottom, f[3]);
            }
            return (left, top, right, bottom);
        }
    }

    private static void SortConfidence(LegacyNeuralLinePair[] pairs, int first, int last, CancellationToken token)
    {
        while (first < last)
        {
            token.ThrowIfCancellationRequested();
            int l = first, r = last; float pivot = pairs[first + (last - first) / 2].Confidence;
            do
            {
                while (pivot < pairs[l].Confidence) l++;
                while (pairs[r].Confidence < pivot) r--;
                if (l <= r) { (pairs[l], pairs[r]) = (pairs[r], pairs[l]); l++; r--; }
            } while (l <= r);
            if (first < r) SortConfidence(pairs, first, r, token);
            first = l;
        }
    }
}
