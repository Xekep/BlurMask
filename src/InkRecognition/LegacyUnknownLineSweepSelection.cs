namespace InkRecognition;

public sealed record LegacyUnknownLineSelectionStage(LegacyUnknownLineGroups Selected, IReadOnlyList<LegacyNeuralLinePair> DeferredPairs);

/// <summary>Ordinary unknown-row07c160 continuation with explicit future-stroke mask; no known or guideline rows.</summary>
public static class LegacyUnknownLineSweepSelection
{
    public static LegacyUnknownLineSelectionStage Select(IReadOnlyList<LegacyLineStrokeStatistics> records,
        IReadOnlyList<LegacyNeuralLinePair> evidence, float scale, LegacyUnknownLineGroups previous,
        ReadOnlySpan<bool> futureStrokeMask, bool finalStrip, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(records); ArgumentNullException.ThrowIfNull(evidence); ArgumentNullException.ThrowIfNull(previous); ArgumentNullException.ThrowIfNull(previous.Groups);
        if (futureStrokeMask.Length != records.Count) throw new ArgumentException("Expected one future flag per physical stroke.");
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
        //07be60 mutates evidence before07c160's confidence sort. Non-final
        //future rejection is per visited pair; once a first-ID block is accepted,
        //the remainder of that block is sorted/copied without rechecking flags.
        var ordered = unique.ToArray();
        SortIds(ordered, 0, ordered.Length - 1, false);
        var costs = new Dictionary<(int,int),(float Join,float Split)>();
        for (int start=0; start<ordered.Length;)
        {
            var first=ordered[start];
            if (!finalStrip && (futureStrokeMask[first.FirstStroke] || futureStrokeMask[first.SecondStroke])) { start++; continue; }
            int end=start; while(end+1<ordered.Length && ordered[end+1].FirstStroke==first.FirstStroke) end++;
            SortIds(ordered,start,end,true);
            for(int i=start;i<=end;i++)
            {
                var p=ordered[i]; float other=1.0000001192092896f-p.Confidence;
                costs.Add((p.FirstStroke,p.SecondStroke), p.LabelOne ? (MathF.Log(other),MathF.Log(p.Confidence)) :
                    (MathF.Log(p.Confidence),MathF.Log(other)));
            }
            start=end+1;
        }
        SortConfidence(ordered,0,ordered.Length-1,cancellationToken);
        int[] membership = Enumerable.Repeat(-1, records.Count).ToArray();
        var groups = new List<List<int>>();
        foreach (var inputGroup in previous.Groups)
        {
            int[] ids = inputGroup.ToArray(); if (ids.Length == 0) throw new ArgumentException("Empty prior physical group.");
            foreach (int id in ids) { if ((uint)id >= records.Count || membership[id] >= 0) throw new ArgumentException("Invalid prior physical ownership."); membership[id] = groups.Count; }
            groups.Add(ids.ToList());
        }
        var deferred = new List<LegacyNeuralLinePair>();
        foreach (var pair in ordered)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!finalStrip && (futureStrokeMask[pair.FirstStroke] || futureStrokeMask[pair.SecondStroke])) { deferred.Add(pair); continue; }
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
        return new(new(Array.AsReadOnly(groups.Select(g => new ReadOnlyMemory<int>(g.ToArray())).ToArray()), membership), deferred.AsReadOnly());

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

    private static void SortIds(LegacyNeuralLinePair[] values,int first,int last,bool second)
    {
        while(first<last)
        {
            int l=first,r=last,pivot=second?values[first+(last-first)/2].SecondStroke:values[first+(last-first)/2].FirstStroke;
            do
            {
                while((second?values[l].SecondStroke:values[l].FirstStroke)<pivot)l++;
                while(pivot<(second?values[r].SecondStroke:values[r].FirstStroke))r--;
                if(l<=r){(values[l],values[r])=(values[r],values[l]);l++;r--;}
            }while(l<=r);
            if(first<r)SortIds(values,first,r,second);first=l;
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
