namespace InkRecognition;

/// <summary>156950 source720 stage from the actual rich selected word and its
/// shared TOP273 glyph pool. Source ranges and sampler categories retain their
/// own producers; parent buffers/map remain the actual157C60 submission data.
/// Does not infer parent membership from the acoustic normalization rectangles.</summary>
public static class LegacyFirstResultSourceMaterializer
{
    public const int MaximumTotalPointVisits=1_048_576;
    public const int MaximumTotalRanges=65_536;

    public static LegacyFirstResultSourceRecord[] Materialize(LegacyFirstMaterializedPlainWord actualWord,
        LegacyOrdinaryGraphSourceRanges actualSourceRanges, LegacySourceSamplerCategoryOwner actualCategoryOwner,
        IReadOnlyList<int> actualChildToParentMap,IReadOnlyList<InkStroke> actualRetainedParentBuffers,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualWord);ArgumentNullException.ThrowIfNull(actualSourceRanges);
        ArgumentNullException.ThrowIfNull(actualCategoryOwner);ArgumentNullException.ThrowIfNull(actualChildToParentMap);
        ArgumentNullException.ThrowIfNull(actualRetainedParentBuffers);cancellationToken.ThrowIfCancellationRequested();
        var result=actualWord.Result;var factory=result.Factory;var word=actualWord.PublishedWord.Word;
        ulong[] references=word.FlatReferences.ToArray();
        //A loaded249 clone copies its serialized1C8 after initialization.
        //A direct raw factory without that payload retains its constructor one.
        float exponent=actualCategoryOwner.Serialized249SourceScoreExponent??factory.SourceScoreExponent;
        if(!ReferenceEquals(word.Pool,result.GlyphPool)||!ReferenceEquals(actualSourceRanges.Graph,actualWord.Graph)||
            !ReferenceEquals(actualSourceRanges.PreparedChronology,actualWord.Cuts.Chronology)||
            !ReferenceEquals(actualSourceRanges.OriginalSource,factory.Frame.Row.OriginalSource)||
            !StringComparer.OrdinalIgnoreCase.Equals(actualCategoryOwner.SourceFingerprint,factory.SourceFingerprint)||
            actualCategoryOwner.SelectedSourceOffset!=factory.InvocationPolicy.Binding.SourceOffset)
            throw new ArgumentException("Shared actual first word/pool/graph/source and selected-source sampler ownership required.");
        if(actualChildToParentMap.Count!=factory.Frame.Row.OriginalSource.Strokes.Count||
            actualRetainedParentBuffers.Count is <1 or >256||references.Length>4096||
            references.Length!=word.GetReferenceCount(cancellationToken))
            throw new ArgumentException("Actual bounded157C60 child submission map and parent retained buffers required.");
        int[] map=actualChildToParentMap.ToArray();
        for(int i=0;i<map.Length;i++)if((uint)map[i]>=(uint)actualRetainedParentBuffers.Count||i>0&&map[i]<=map[i-1])
            throw new ArgumentException("Actual increasing child-to-parent submission map required.");
        var parent=new InkStroke[actualRetainedParentBuffers.Count];
        for(int i=0;i<parent.Length;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();var points=actualRetainedParentBuffers[i]?.Points;
            if(points?.Count is not (>=1 and <=4096))throw new ArgumentException("Finite nonempty retained parent buffers required.");
            var snapshot=points.ToArray();foreach(var p in snapshot)if(!float.IsFinite(p.X)||!float.IsFinite(p.Y))
                throw new ArgumentException("Finite retained parent points required.");
            parent[i]=new(snapshot);
        }
        var entries=new List<(LegacyNativeGlyphRecordNode Owner,short Selected,LegacyFirstResultSourceRange[] Ranges)>();
        int pointBudget=MaximumTotalPointVisits,rangeBudget=MaximumTotalRanges;
        //275 already owns reading-order flat references. Snapshot once rather
        //than traversing/copying the full linked chain for every glyph.
        for(int ordinal=0;ordinal<references.Length;ordinal++)
        {
            cancellationToken.ThrowIfCancellationRequested();ulong reference=references[ordinal];
            var key=LegacyFirstResultGlyphKey.Decode(reference);
            if(key.Arc<0)continue; //Native156950 omits delimiter references.
            var owner=result.GlyphPool.CopyOwnerNode(LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference),cancellationToken);
            if(key.Alternative<0||key.Alternative>=owner.Records.Length/40)
                throw new ArgumentException("Actual signed selected glyph alternative required.");
            var ranges=actualSourceRanges.ResolveCandidate(key.Node,key.Arc,cancellationToken);
            if(ranges.Length>rangeBudget)throw new ArgumentException("Total source range budget exceeded.");rangeBudget-=ranges.Length;
            foreach(var range in ranges)
            {
                var first=range.First;var last=range.Last;
                if(first.First!=last.First||first.Second<0||last.Second<first.Second||last.Second>=map.Length||
                    first.Third<0||last.Third<0||first.Third>=parent[map[first.Second]].Points.Count||
                    last.Third>=parent[map[last.Second]].Points.Count||first.Second==last.Second&&last.Third<first.Third)
                    throw new ArgumentException("Actual child raw ordinals must address retained parent submission buffers.");
                for(int physical=first.Second;physical<=last.Second;physical++)
                {
                    int visits=(physical==last.Second?last.Third+1:parent[map[physical]].Points.Count)-
                        (physical==first.Second?first.Third:0);
                    if(visits>pointBudget)throw new ArgumentException("Total source point-visit budget exceeded.");pointBudget-=visits;
                }
            }
            entries.Add((owner,key.Alternative,ranges));
        }
        var output=new LegacyFirstResultSourceRecord[entries.Count];
        for(int i=0;i<output.Length;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();var entry=entries[i];
            output[i]=LegacyFirstResultSourceRecord.Create(entry.Owner.Records.Span,entry.Selected,entry.Ranges,map,parent,
                actualCategoryOwner.Categories,exponent,cancellationToken);
        }
        return output;
    }
}
