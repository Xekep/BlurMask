namespace InkRecognition;

/// <summary>
/// Ordinary, unknown-row, unlocked branch of Text07a850: order groups by mean-Y,
/// then merge adjacent compatible groups. No prior rows, supplied guidelines or
/// reassignment of unselected physical strokes is invented.
/// </summary>
public static class LegacyUnknownLinePostselection
{
    public static LegacyUnknownLineGroups Apply(IReadOnlyList<LegacyLineStrokeStatistics> records,
        LegacyUnknownLineGroups selected, float scale, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(records); ArgumentNullException.ThrowIfNull(selected);
        cancellationToken.ThrowIfCancellationRequested();
        if (records.Count is < 1 or > 256 || selected.Groups.Count > records.Count ||
            !float.IsFinite(scale) || scale <= 0 || scale > 1_000_000)
            throw new ArgumentException("Expected bounded ordinary unknown line groups.");
        foreach (var r in records)
        {
            ArgumentNullException.ThrowIfNull(r);
            foreach (float v in r.Values) if (MathF.Abs(v) > 1e12f) throw new ArgumentException("Unsupported geometry magnitude.");
        }
        bool[] seen = new bool[records.Count];
        var groups = new List<int[]>();
        foreach (var memory in selected.Groups)
        {
            int[] ids = memory.ToArray();
            if (ids.Length == 0) throw new ArgumentException("Empty group.");
            foreach (int id in ids)
            {
                if ((uint)id >= records.Count || seen[id]) throw new ArgumentException("Invalid or repeated physical stroke.");
                seen[id] = true;
            }
            groups.Add(ids);
        }
        var order = groups.Select((g, i) => (Index:i, Key:Mean(g))).ToArray();
        Sort(order, 0, order.Length - 1);
        for (int i = order.Length - 1; i > 0; i--)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int left = order[i - 1].Index, right = order[i].Index;
            var a = groups[left]; var b = groups[right];
            var ba = Bounds(a); var bb = Bounds(b);
            float dy = MathF.Abs(order[i - 1].Key - order[i].Key);
            bool yOverlap = !(bb.MaxY < ba.MinY || ba.MaxY < bb.MinY);
            if (scale <= dy && !yOverlap) continue;
            bool merge = (ba.MaxX <= bb.MinX && NearEnds(a, b, ba, bb)) ||
                (bb.MaxX <= ba.MinX && NearEnds(b, a, bb, ba));
            if (!merge)
            {
                if (scale <= dy) continue;
                var intervals = new List<(float Start,float End)>();
                foreach (int id in a) AddInterval(intervals, records[id].Values[0], records[id].Values[2]);
                float covered = 0;
                foreach (int id in b) covered += Covered(intervals, records[id].Values[0], records[id].Values[2]);
                merge = covered < scale * .5f;
            }
            if (!merge) continue;
            groups[left] = a.Concat(b).ToArray(); groups[right] = [];
            order[i].Index = -1;
            // The native ordering key of the surviving group is deliberately
            // not recomputed after merging; traversal proceeds upwards.
        }
        int[] membership = Enumerable.Repeat(-1, records.Count).ToArray();
        var output = new List<ReadOnlyMemory<int>>();
        foreach (var item in order)
        {
            if (item.Index < 0) continue;
            var ids = groups[item.Index]; int g = output.Count;
            foreach (int id in ids) membership[id] = g;
            output.Add(ids);
        }
        return new(output.AsReadOnly(), membership);

        float Mean(int[] ids) { float sum=0; foreach(int id in ids) sum += records[id].Values[5]; return sum/ids.Length; }
        (float MinX,float MinY,float MaxX,float MaxY) Bounds(int[] ids)
        {
            float minX=float.MaxValue,minY=float.MaxValue,maxX=-float.MaxValue,maxY=-float.MaxValue;
            foreach(int id in ids) { var r=records[id].Values; minX=MathF.Min(minX,r[0]);minY=MathF.Min(minY,r[1]);maxX=MathF.Max(maxX,r[2]);maxY=MathF.Max(maxY,r[3]); }
            return (minX,minY,maxX,maxY);
        }
        bool NearEnds(int[] a,int[] b,(float MinX,float MinY,float MaxX,float MaxY) ba,(float MinX,float MinY,float MaxX,float MaxY) bb)
        {
            float xA=ba.MaxX-MathF.Min(ba.MaxX-ba.MinX,scale*5f);
            float xB=bb.MinX+MathF.Min(bb.MaxX-bb.MinX,scale*5f);
            float edgeA=0,totalA=0,edgeB=0,totalB=0;int na=0,nb=0;
            foreach(int id in a) { var r=records[id].Values;totalA+=r[5];if(xA<=r[0]||xA<r[2]){edgeA+=r[5];na++;} }
            foreach(int id in b) { var r=records[id].Values;totalB+=r[5];if(r[2]<=xB||r[0]<xB){edgeB+=r[5];nb++;} }
            float meanA=totalA/a.Length,meanB=totalB/b.Length;
            if(scale*.25f<MathF.Abs(edgeA/na-meanA)||scale*.25f<MathF.Abs(edgeB/nb-meanB))
                return MathF.Abs(edgeB/nb-edgeA/na)<scale*1.75f;
            return MathF.Abs(meanB-meanA)<scale;
        }
        void Sort((int Index,float Key)[] values,int lo,int hi)
        {
            if(lo>=hi)return;float pivot=values[(lo+hi)/2].Key;int l=lo,r=hi;
            while(l<=r){while(values[l].Key<pivot)l++;while(pivot<values[r].Key)r--;if(l<=r){(values[l],values[r])=(values[r],values[l]);l++;r--;}}
            if(lo<r)Sort(values,lo,r);if(l<hi)Sort(values,l,hi);
        }
    }

    private static void AddInterval(List<(float Start,float End)> list,float start,float end)
    {
        int last=list.Count-1;while(last>=0&&list[last].Start>end+1f)last--;
        int first=0;while(first<=last&&list[first].End<start-1f)first++;
        if(first<=last)
        {
            start=MathF.Min(start,list[first].Start);end=MathF.Max(end,list[last].End);
            list[first]=(start,end);if(last>first)list.RemoveRange(first+1,last-first);
        }
        else list.Insert(first,(start,end));
    }

    private static float Covered(List<(float Start,float End)> list,float start,float end)
    {
        const float eps=1.1920928955078125e-7f;
        int last=list.Count-1;while(last>=0&&list[last].Start>end+eps)last--;
        int first=0;while(first<=last&&list[first].End<start-eps)first++;
        if(first>last)return 0;
        // Text071c80 returns the complete query width when both endpoints
        // resolve to one interval, rather than a clamped intersection.
        if(first==last)return end-start;
        float covered=list[first].End-start;
        for(int i=first+1;i<last;i++)covered+=list[i].End-list[i].Start;
        covered+=end-list[last].Start;
        float tolerance=MathF.Max(MathF.Abs(covered)*2.384185791015625e-7f,eps);
        return tolerance<MathF.Abs(covered)?covered:0;
    }
}
