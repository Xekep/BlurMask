using System.Buffers.Binary;
namespace InkRecognition;

internal readonly record struct LegacyFullEnglishFirstGlyphEmissionAttempt(int Encounter,ulong Reference,
    ulong Key,int Descriptor,int Ordinal,ushort Symbol,bool Existing);
internal readonly record struct LegacyFullEnglishFirstGlyphEmissionEntry(int Encounter,ulong Reference,
    ulong Key,LegacyFullEnglishFirstGlyphEmissionPlan? Plan);
internal readonly record struct LegacyFullEnglishFirstGlyphEmissionRange(ulong Key,int Pass,int Ordinal,int Count,
    LegacyFirstResultSourceRange Range);

//After aggregate preflight, capture original rows once and preflight ALL actual
//A8 attempts/header prototypes before Run may claim the new owner.
internal sealed class LegacyFullEnglishFirstGlyphEmissionChildPlan
{
    internal LegacyFullEnglishFirstPlainChild Original{get;}
    internal LegacyNativeGlyphRecordNode[] Originals{get;}
    internal LegacyFullEnglishFirstGlyphEmissionEntry[] Entries{get;}
    internal LegacyFullEnglishFirstGlyphEmissionWord[] Words{get;}
    internal int OriginalRows{get;} internal int Appends{get;} internal int NormalOwners{get;}
    internal LegacyFullEnglishFirstGlyphEmissionChildPlan(LegacyFullEnglishFirstGlyphEmissionPolicy policy,
        LegacyFullEnglishFirstPlainChild child,CancellationToken token)
    {
        Original=child;policy.Validate(child,token);Originals=child.CopyGlyphOwners(token).ToArray();
        if(Originals.Length>4096)throw new NotSupportedException("B7 private owner cap4096.");
        var owners=new Dictionary<ulong,LegacyNativeGlyphRecordNode>();
        foreach(var node in Originals)
        {
            token.ThrowIfCancellationRequested();int count=(node.PackedCount>>2)&2047;
            if(count is <1 or >2040||node.Records.Length!=count*40||!owners.TryAdd(node.Key,node))throw new InvalidDataException("Faithful B6 owner snapshot required.");
            OriginalRows=checked(OriginalRows+count);
        }
        var entries=new List<LegacyFullEnglishFirstGlyphEmissionEntry>();var attempts=new Dictionary<ulong,int>();var touched=new HashSet<ulong>();int encounter=0;
        Words=new LegacyFullEnglishFirstGlyphEmissionWord[child.NativeWords.Count];
        for(int i=0;i<Words.Length;i++)
        {
            token.ThrowIfCancellationRequested();var word=child.NativeWords[i];Words[i]=new(child.Words[i],token);
            foreach(ulong reference in word.Published.Word.FlatReferences)
            {
                token.ThrowIfCancellationRequested();int ordinal=encounter++;var decoded=LegacyFirstResultGlyphKey.Decode(reference);
                ulong key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
                if(!owners.TryGetValue(key,out var existing)||decoded.Alternative<0||decoded.Alternative>=((existing.PackedCount>>2)&2047))
                    throw new InvalidDataException("Every WORD reference must resolve in this original pool.");
                if(decoded.Arc<0)continue;
                touched.Add(key);var plan=policy.Plan(child,ordinal,reference,token);
                if(plan.HasValue){attempts[key]=checked(attempts.GetValueOrDefault(key)+1);Appends++;}
                entries.Add(new(ordinal,reference,key,plan));
            }
        }
        foreach(var item in attempts)
        {token.ThrowIfCancellationRequested();if(((owners[item.Key].PackedCount>>2)&2047)+item.Value>2040)throw new NotSupportedException("B7 prospective native per-owner2040 row cap.");}
        if(OriginalRows+Appends>65536)throw new NotSupportedException("B7 private total row cap65536.");
        NormalOwners=touched.Count;Entries=entries.ToArray();
    }
}

internal sealed class LegacyFullEnglishFirstGlyphEmissionPool
{
    private readonly LegacyNativeGlyphRecordPool pool;
    private readonly Dictionary<ulong,LegacyFirstResultSourceRange[]> ranges=[];
    private readonly LegacyFullEnglishFirstGlyphEmissionChildPlan plan;
    internal int AttemptCount{get;private set;} internal int FreshCount{get;private set;}
    internal int RangePassCount{get;private set;}
    internal LegacyFullEnglishFirstGlyphEmissionPool(LegacyFullEnglishFirstGlyphEmissionChildPlan plan,CancellationToken token)
    {
        this.plan=plan;pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false,token);
        foreach(var original in plan.Originals)
        {
            token.ThrowIfCancellationRequested();bool force=(original.PackedCount&0x8000)!=0;int count=original.Records.Length/40;
            for(int i=0;i<count;i++)
            {
                token.ThrowIfCancellationRequested();var row=original.Records.Span.Slice(i*40,40);
                int descriptor=BinaryPrimitives.ReadInt32LittleEndian(row);var insertion=pool.AppendDescriptor(original.Key,descriptor,force,new byte[40],token);
                if(insertion.ExistingDescriptor||insertion.Reference!=(original.Key|((ulong)i<<40)))throw new InvalidDataException("B7 clone must retain descriptor ordinal and duplicate state.");
                pool.ReplaceRecord(insertion.Reference,row,token);
            }
            var copied=pool.CopyOwnerNode(original.Key,token);
            if(copied.PackedCount!=original.PackedCount||!copied.Records.Span.SequenceEqual(original.Records.Span))throw new InvalidDataException("B7 clone did not retain full opaque bytes.");
        }
    }
    internal void Apply(LegacyFullEnglishFirstGlyphEmissionEntry entry,LegacyFullEnglishFirstGlyphEmissionPolicy.Cache cache,
        ReadOnlySpan<byte> allocation,Action<string>? checkpoint,
        Action<LegacyFullEnglishFirstGlyphEmissionAttempt>? attemptObserver,
        Action<LegacyFullEnglishFirstGlyphEmissionQuery>? queryObserver,
        Action<LegacyFullEnglishFirstGlyphEmissionRange>? rangeObserver,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(!ranges.ContainsKey(entry.Key))
        {
            var decoded=LegacyFirstResultGlyphKey.Decode(entry.Reference);var service=plan.Original.Factory.Candidates.SourceRanges;
            var counted=service.ResolveCandidate(decoded.Node,decoded.Arc,token);RangePassCount++;
            if(counted.Length is <1 or >8)throw new InvalidDataException("Actual nonempty bounded count range required.");
            for(int i=0;i<counted.Length;i++){rangeObserver?.Invoke(new(entry.Key,0,i,counted.Length,counted[i]));token.ThrowIfCancellationRequested();}
            checkpoint?.Invoke("range:counted");token.ThrowIfCancellationRequested();
            var filled=service.ResolveCandidate(decoded.Node,decoded.Arc,token);RangePassCount++;
            if(filled.Length is <1 or >8)throw new InvalidDataException("Actual nonempty bounded fill range required.");
            for(int i=0;i<filled.Length;i++){rangeObserver?.Invoke(new(entry.Key,1,i,filled.Length,filled[i]));token.ThrowIfCancellationRequested();}
            if(!counted.AsSpan().SequenceEqual(filled))throw new InvalidDataException("Source range count/fill identity mismatch.");
            ranges.Add(entry.Key,filled);checkpoint?.Invoke("range:filled");token.ThrowIfCancellationRequested();
        }
        if(entry.Plan is not{} selected)return;
        var insertion=pool.AppendDescriptor(entry.Key,selected.Descriptor,false,allocation,token);AttemptCount++;
        if(AttemptCount>plan.Appends)throw new InvalidDataException("B7 attempted append reservation exceeded.");
        int ordinal=LegacyFirstResultGlyphKey.Decode(insertion.Reference).Alternative;
        var observation=new LegacyFullEnglishFirstGlyphEmissionAttempt(entry.Encounter,entry.Reference,entry.Key,selected.Descriptor,ordinal,selected.Symbol,insertion.ExistingDescriptor);
        checkpoint?.Invoke("glyph:interned");token.ThrowIfCancellationRequested();
        if(!insertion.ExistingDescriptor)
        {
            FreshCount++;pool.SetOrdinarySymbol(insertion.Reference,selected.Symbol,token);
            float score=cache.Score(selected,token);byte[] record=pool.CopyRecord(insertion.Reference,token);
            BinaryPrimitives.WriteSingleLittleEndian(record.AsSpan(16),score);pool.ReplaceRecord(insertion.Reference,record,token);
            checkpoint?.Invoke("glyph:score");token.ThrowIfCancellationRequested();
            var query=cache.Query(selected,token);record=pool.CopyRecord(insertion.Reference,token);
            BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(20),query.Cost);
            ushort flags=BinaryPrimitives.ReadUInt16LittleEndian(record.AsSpan(4));BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(4),(ushort)(flags&0xfff7));
            ushort publish=BinaryPrimitives.ReadUInt16LittleEndian(record.AsSpan(6));BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(6),(ushort)(publish|(selected.Symbol==209?0x48:0x40)));
            BinaryPrimitives.WriteSingleLittleEndian(record.AsSpan(12),selected.Likelihood/255f);pool.ReplaceRecord(insertion.Reference,record,token);
            queryObserver?.Invoke(query);token.ThrowIfCancellationRequested();
        }
        attemptObserver?.Invoke(observation);checkpoint?.Invoke("glyph:complete");token.ThrowIfCancellationRequested();
    }
    internal LegacyNativeGlyphRecordNode[] CopyOwners(CancellationToken token)=>pool.CopyOwnerNodes(token).ToArray();
    internal Dictionary<ulong,LegacyFirstResultSourceRange[]> CopyRanges(CancellationToken token)
    {var copied=new Dictionary<ulong,LegacyFirstResultSourceRange[]>();foreach(var item in ranges){token.ThrowIfCancellationRequested();copied.Add(item.Key,item.Value.ToArray());}return copied;}
}
