namespace InkRecognition;

/// <summary>Immutable provenance for one source720 row in actual ordered
/// WORD/reference traversal, with inclusive pre-dedup child input keys.</summary>
public sealed class LegacyEnglish119SourceRecordOrigin
{
    public int WordOrdinal {get;}
    public int ReferenceOrdinal {get;}
    public ulong GlyphReference {get;}
    private readonly LegacyFirstResultSourceRange[] ranges;
    internal LegacyEnglish119SourceRecordOrigin(int word,int ordinal,ulong reference,LegacyFirstResultSourceRange[] ranges)
    {WordOrdinal=word;ReferenceOrdinal=ordinal;GlyphReference=reference;this.ranges=ranges.ToArray();}
    public LegacyFirstResultSourceRange[] CopyChildRanges()=>ranges.ToArray();
}

/// <summary>Actual selected119 rich result -> independent source720 rows.
/// These rows address exact submitted-child coordinates. No native main parent,
/// TOP273 serialization, UTF8, DOM or application publication is implied.</summary>
public sealed class LegacyEnglish119SourceRecords
{
    public LegacyEnglish119SourceSubmission Submission {get;}
    public LegacyEnglish119PlainTextResult Text {get;}
    public IReadOnlyList<LegacyFirstResultSourceRecord> Rows {get;}
    public IReadOnlyList<LegacyEnglish119SourceRecordOrigin> Origins {get;}
    internal LegacyEnglish119SourceRecords(LegacyEnglish119SourceSubmission submission,LegacyEnglish119PlainTextResult text,
        LegacyFirstResultSourceRecord[] rows,LegacyEnglish119SourceRecordOrigin[] origins)
    {Submission=submission;Text=text;Rows=Array.AsReadOnly(rows);Origins=Array.AsReadOnly(origins);}
}

public static class LegacyOwnedEnglish119SourceRecords
{
    public const int MaximumReferences=4096;
    public const int MaximumRanges=65_536;
    public const int MaximumPointVisits=1_048_576;
    public static LegacyEnglish119SourceRecords Materialize(LegacyEnglish119SourceSubmission submission,
        LegacyEnglish119PlainTextResult actualText,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(actualText);token.ThrowIfCancellationRequested();
        var factory=actualText.Factory;var preparation=submission.Preparation;var candidates=factory.Candidates;
        var ranges=candidates.SourceRanges;var model=submission.Model;var policy=factory.Policy;
        if(!ReferenceEquals(factory.Preparation,preparation)||!ReferenceEquals(candidates.Preparation,preparation)||
            !ReferenceEquals(preparation.Model,model)||!ReferenceEquals(policy.Owner.Model,model)||
            !ReferenceEquals(preparation.Source,model.Source)||!ReferenceEquals(candidates.Sampler.Source,model.Source)||
            !ReferenceEquals(policy.SourceRanges,ranges)||!ReferenceEquals(factory.Graph,ranges.Graph)||
            !ReferenceEquals(ranges.OriginalSource,preparation.OriginalSource)||
            !ReferenceEquals(factory.Categories,policy.Categories)||model.SelectedProcessor.Kind!=0x249||
            model.Source.Version!=0x110||model.Source.SourceKind!=0x135||candidates.Sampler.Child.Version!=0x108||
            !float.IsFinite(model.SourceScoreExponent)||model.SourceScoreExponent<0||
            submission.Raw.Count!=preparation.OriginalSource.Strokes.Count)
            throw new ArgumentException("Same exact pre-NN EN submission/model/preparation/pool/graph/sampler108 required.");
        //The private result already checked each WORD pool. Check complete
        //publication order/count before snapshotting refs or copying owners.
        int count=0;var next=actualText.Storage.WordPublisher.Head;
        foreach(var word in actualText.NativeWords)
        {
            token.ThrowIfCancellationRequested();
            if(!ReferenceEquals(word.Published,next)||!ReferenceEquals(word.Published.Word.Pool,actualText.Storage.GlyphPool))
                throw new ArgumentException("Actual complete ordered WORD list and shared glyph pool required.");
            next=next.Next;int n=word.Published.Word.GetReferenceCount(token);
            if(n<0||n>MaximumReferences-count)throw new ArgumentException("Total source reference budget exceeded.");count+=n;
        }
        if(next is not null)throw new ArgumentException("Actual complete WORD list required.");
        var entries=new List<Entry>(count);var owners=new Dictionary<ulong,LegacyNativeGlyphRecordNode>();
        int rangeBudget=MaximumRanges,pointBudget=MaximumPointVisits,wordOrdinal=0;
        foreach(var word in actualText.NativeWords)
        {
            token.ThrowIfCancellationRequested();var refs=word.Published.Word.FlatReferences.ToArray();
            for(int ordinal=0;ordinal<refs.Length;ordinal++)
            {
                token.ThrowIfCancellationRequested();ulong reference=refs[ordinal];var key=LegacyFirstResultGlyphKey.Decode(reference);
                if(key.Arc<0)continue;
                ulong ownerKey=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
                if(!owners.TryGetValue(ownerKey,out var owner))
                {owner=actualText.Storage.GlyphPool.CopyOwnerNode(ownerKey,token);owners.Add(ownerKey,owner);}
                if(key.Alternative<0||key.Alternative>=owner.Records.Length/40)throw new ArgumentException("Actual signed selected alternative required.");
                var resolved=ranges.ResolveCandidate(key.Node,key.Arc,token);
                PreflightRanges(resolved,submission.Raw,ref rangeBudget,ref pointBudget,token);
                entries.Add(new(owner,key.Alternative,resolved,wordOrdinal,ordinal,reference));
            }
            wordOrdinal++;
        }
        //All stage bounds are checked before any720 row geometry. Identity
        //mapping leaves projected fragment count exactly resolved.Length.
        var rows=new LegacyFirstResultSourceRecord[entries.Count];var origins=new LegacyEnglish119SourceRecordOrigin[rows.Length];
        for(int i=0;i<rows.Length;i++)
        {
            token.ThrowIfCancellationRequested();var entry=entries[i];
            rows[i]=LegacyFirstResultSourceRecord.Create(entry.Owner.Records.Span,entry.Selected,entry.Ranges,submission.PhysicalMap,
                submission.Raw,policy.Categories,model.SourceScoreExponent,token);
            origins[i]=new(entry.Word,entry.Ordinal,entry.Reference,entry.Ranges);
        }
        token.ThrowIfCancellationRequested();return new(submission,actualText,rows,origins);
    }
    //Internal scalar-only preflight is shared with explicitly prepared bound/
    //cancellation probes. It neither creates an owned receipt nor emits a row.
    internal static void PreflightRanges(IReadOnlyList<LegacyFirstResultSourceRange> resolved,IReadOnlyList<InkStroke> raw,
        ref int rangeBudget,ref int pointBudget,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(resolved.Count is <1 or >4096||resolved.Count>rangeBudget)throw new ArgumentException("Total source range budget exceeded.");
        rangeBudget-=resolved.Count;
        foreach(var range in resolved)
        {
            token.ThrowIfCancellationRequested();var first=range.First;var last=range.Last;
            if(first.First!=last.First||first.Second<0||last.Second<first.Second||last.Second>=raw.Count||
                first.Third<0||last.Third<0||first.Third>=raw[first.Second].Points.Count||last.Third>=raw[last.Second].Points.Count||
                first.Second==last.Second&&last.Third<first.Third)
                throw new ArgumentException("Source metadata must address exact pre-dedup child coordinates.");
            for(int s=first.Second;s<=last.Second;s++)
            {
                token.ThrowIfCancellationRequested();int visits=(s==last.Second?last.Third+1:raw[s].Points.Count)-(s==first.Second?first.Third:0);
                if(visits>pointBudget)throw new ArgumentException("Total source geometry point-visit budget exceeded.");pointBudget-=visits;
            }
        }
    }
    private sealed record Entry(LegacyNativeGlyphRecordNode Owner,short Selected,LegacyFirstResultSourceRange[] Ranges,int Word,int Ordinal,ulong Reference);
}
