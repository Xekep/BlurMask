namespace InkRecognition;

/// <summary>Actual156950 source720 rows for one selected first TEXT submission.
/// The parent map/category/exponent are derived from the same main249 owner.</summary>
public sealed class LegacyMainMaterializedFirstTextSource
{
    public LegacyFreshMainFirstResultLines SourceOwner {get;}
    public int DescriptorOrdinal {get;}
    public LegacyFirstMaterializedPlainText Text {get;}
    public IReadOnlyList<LegacyFirstResultSourceRecord> Rows {get;}
    internal LegacyMainMaterializedFirstTextSource(LegacyFreshMainFirstResultLines source,int descriptor,
        LegacyFirstMaterializedPlainText text,LegacyFirstResultSourceRecord[] rows)
    {SourceOwner=source;DescriptorOrdinal=descriptor;Text=text;Rows=Array.AsReadOnly(rows);}
}

public static class LegacyFirstPlainTextSourceMaterializer
{
    /// <summary>Walk the actual279 linked word order and each275 reading order.
    /// Native156950 skips negative-arc delimiter references, not guessed labels.
    /// Empty publication produces zero rows; no fallback glyph is manufactured.</summary>
    public static LegacyMainMaterializedFirstTextSource MaterializeForSubmission(LegacyFreshMainFirstResultLines source,
        int descriptorOrdinal,LegacyFirstMaterializedPlainText text,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(source);ArgumentNullException.ThrowIfNull(text);token.ThrowIfCancellationRequested();
        var submission=source.FirstSubmissions.SingleOrDefault(s=>s.DescriptorOrdinal==descriptorOrdinal)
            ??throw new ArgumentException("Actual ordinary first submission required.");
        var factory=text.Result.Factory;var ranges=text.Owner.Prepared.SourceRanges;var categories=source.Controller.FirstCategoryOwner;
        if(!ReferenceEquals(submission.Factory,factory)||!ReferenceEquals(text.Owner.Factory,factory)||
            !ReferenceEquals(ranges.Graph,text.Graph)||!ReferenceEquals(ranges.PreparedChronology,text.Cuts.Chronology)||
            !ReferenceEquals(ranges.OriginalSource,factory.Frame.Row.OriginalSource)||
            !StringComparer.OrdinalIgnoreCase.Equals(categories.SourceFingerprint,factory.SourceFingerprint)||
            categories.SelectedSourceOffset!=factory.InvocationPolicy.Binding.SourceOffset)
            throw new ArgumentException("Same first TEXT/submission/pool/source/category ownership required.");
        int[] map=submission.CurrentParentPhysicalIndices.ToArray();var parent=source.Controller.Source.Strokes;
        var descriptor=source.Input.CopyDescriptors()[descriptorOrdinal];
        int firstPhysical=System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(36));
        int lastPhysical=System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(40));
        int[] actualMap=LegacyFirstResultSourceRecord.SelectChildPhysicalIndices(parent,firstPhysical,lastPhysical,0,
            source.Input.Input.Input.Scale.Scales.Primary,token);
        if(!map.AsSpan().SequenceEqual(actualMap))throw new ArgumentException("Submission map differs from its actual parent descriptor/filter.");
        if(map.Length!=factory.Frame.Row.OriginalSource.Strokes.Count||parent.Count is <1 or >256)
            throw new ArgumentException("Actual child-to-current-parent submission required.");
        for(int i=0;i<map.Length;i++)if((uint)map[i]>=(uint)parent.Count||i>0&&map[i]<=map[i-1])
            throw new ArgumentException("Actual increasing child-to-parent map required.");
        var entries=new List<(LegacyNativeGlyphRecordNode Owner,short Selected,LegacyFirstResultSourceRange[] Ranges)>();
        int pointBudget=LegacyFirstResultSourceMaterializer.MaximumTotalPointVisits,rangeBudget=LegacyFirstResultSourceMaterializer.MaximumTotalRanges;
        int references=0;var nativeWord=text.Result.WordPublisher.Head;
        foreach(var published in text.Words)
        {
            token.ThrowIfCancellationRequested();
            if(!ReferenceEquals(published.Published,nativeWord)||!ReferenceEquals(published.Published.Word.Pool,text.Result.GlyphPool))
                throw new ArgumentException("Actual279 ordered word list and shared273 pool required.");
            nativeWord=nativeWord.Next;var refs=published.Published.Word.FlatReferences;
            if(refs.Count>4096-references)throw new ArgumentException("Portable first TEXT reference budget exceeded.");references+=refs.Count;
            foreach(ulong reference in refs)
            {
                token.ThrowIfCancellationRequested();var key=LegacyFirstResultGlyphKey.Decode(reference);if(key.Arc<0)continue;
                if(entries.Count==4096)throw new ArgumentException("Portable total source720 row budget exceeded.");
                var owner=text.Result.GlyphPool.CopyOwnerNode(LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference),token);
                if(key.Alternative<0||key.Alternative>=owner.Records.Length/40)throw new ArgumentException("Actual signed selected glyph alternative required.");
                var rawRanges=ranges.ResolveCandidate(key.Node,key.Arc,token);
                if(rawRanges.Length>rangeBudget)throw new ArgumentException("Total source range budget exceeded.");rangeBudget-=rawRanges.Length;
                foreach(var range in rawRanges)
                {
                    var first=range.First;var last=range.Last;
                    if(first.First!=last.First||first.Second<0||last.Second<first.Second||last.Second>=map.Length||
                        first.Third<0||last.Third<0||first.Third>=parent[map[first.Second]].Points.Count||last.Third>=parent[map[last.Second]].Points.Count||
                        first.Second==last.Second&&last.Third<first.Third)throw new ArgumentException("Actual child keys must address retained parent submission points.");
                    for(int physical=first.Second;physical<=last.Second;physical++)
                    {
                        token.ThrowIfCancellationRequested();int visits=(physical==last.Second?last.Third+1:parent[map[physical]].Points.Count)-
                            (physical==first.Second?first.Third:0);
                        if(visits>pointBudget)throw new ArgumentException("Total source point-visit budget exceeded.");pointBudget-=visits;
                    }
                }
                entries.Add((owner,key.Alternative,rawRanges));
            }
        }
        if(nativeWord is not null)throw new ArgumentException("Complete actual279 list required.");
        float exponent=categories.Serialized249SourceScoreExponent??factory.SourceScoreExponent;
        var rows=new LegacyFirstResultSourceRecord[entries.Count];
        for(int i=0;i<rows.Length;i++)
        {
            token.ThrowIfCancellationRequested();var entry=entries[i];
            rows[i]=LegacyFirstResultSourceRecord.Create(entry.Owner.Records.Span,entry.Selected,entry.Ranges,map,parent,categories.Categories,exponent,token);
        }
        token.ThrowIfCancellationRequested();return new(source,descriptorOrdinal,text,rows);
    }
}
