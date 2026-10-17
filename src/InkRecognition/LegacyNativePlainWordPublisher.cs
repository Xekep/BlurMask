namespace InkRecognition;

/// <summary>Owned275 plain-word snapshot. Native vector pointer20 is replaced
/// by FlatReferences; header stores a zero pointer placeholder and exact
/// count/capacity. Pool identity and linked head are retained.</summary>
public sealed class LegacyNativePlainWordRecord
{
    private readonly byte[] data;
    private readonly ulong[] flat;
    public LegacyNativeGlyphRecordPool Pool {get;}
    public IReadOnlyList<ulong> FlatReferences {get;}
    public ulong HeadReference=>LegacyNativeWordBuilder.U64(data,8);
    internal LegacyNativePlainWordRecord(LegacyNativeGlyphRecordPool pool,byte[]data,ulong[]flat)
    {Pool=pool;this.data=data;this.flat=flat;FlatReferences=Array.AsReadOnly(flat);}
    public byte[] CopyStorage()=>data.ToArray();
    public int GetReferenceCount(CancellationToken cancellationToken=default)
    {cancellationToken.ThrowIfCancellationRequested();return flat.Length!=0?flat.Length:LegacyNativeWordBuilder.CopyChain(Pool,HeadReference,cancellationToken).Length;}
    public int GetSymbolSpan(CancellationToken cancellationToken=default)
    {
        int span=0;
        var references=flat.Length!=0?flat:LegacyNativeWordBuilder.CopyChain(Pool,HeadReference,cancellationToken);
        foreach(ulong reference in references)
        {
            cancellationToken.ThrowIfCancellationRequested();byte[] glyph=Pool.CopyRecord(reference,cancellationToken);
            int kind=LegacyNativeWordBuilder.U16(glyph,4)&7;
            span=unchecked(span+((kind is 1 or 3)?(LegacyNativeWordBuilder.U16(glyph,8)>>2)&0x1fff:1));
        }
        return span;
    }
    public ulong GetReadingOrderReference(int ordinal,CancellationToken cancellationToken=default)
    {
        int count=GetReferenceCount(cancellationToken);
        if(ordinal<0||ordinal>=count)throw new ArgumentOutOfRangeException(nameof(ordinal));
        var chain=LegacyNativeWordBuilder.CopyChain(Pool,HeadReference,cancellationToken);
        if(chain.Length<count)throw new ArgumentException("Actual linked source glyph chain no longer covers stored count.");
        return chain[count-1-ordinal];
    }
}

public sealed class LegacyNativePublishedWordNode
{
    public LegacyNativeWordSourceSpan Span {get;}
    public int WordIndex {get;private set;}
    public LegacyNativePlainWordRecord Word {get;private set;}
    public LegacyNativePublishedWordNode? Next {get;}
    internal LegacyNativePublishedWordNode(LegacyNativeWordSourceSpan span,int index,
        LegacyNativePlainWordRecord word,LegacyNativePublishedWordNode? next)
    {Span=span;WordIndex=index;Word=word;Next=next;}
    internal void RebindWord(int index,LegacyNativePlainWordRecord word)
    {
        if(index<0||!ReferenceEquals(Word.Pool,word.Pool))throw new ArgumentException("Same-pool WORD rebind required.");
        WordIndex=index;Word=word;
    }
}

/// <summary>279.virtual10/1416b0 plain prepend branch and275/13fe20 plain
/// word interning, with geometry finalization/order and auxiliary output
/// explicitly disabled. Source-span callbacks remain actual caller data.</summary>
public sealed class LegacyNativePlainWordPublisher
{
    private sealed class InterningState
    {
        internal readonly Dictionary<LegacyNativeWordSourceSpan,List<LegacyNativePlainWordRecord>> Spans=[];
        internal readonly List<LegacyNativePlainWordPublisher> Publishers=[];
        internal int WordCount;
    }
    private readonly LegacyNativeGlyphRecordPool pool;
    private readonly bool ownerC4;
    private readonly InterningState interning;
    private int nodeCount;
    private bool released;
    public LegacyNativePublishedWordNode? Head {get;private set;}

    public LegacyNativePlainWordPublisher(LegacyNativeGlyphRecordPool actualPool,
        bool actualOwnerC4,bool actualFinalizeWordGeometry,bool actualSortByGeometry)
    {
        ArgumentNullException.ThrowIfNull(actualPool);
        if(actualFinalizeWordGeometry||actualSortByGeometry)
            throw new NotSupportedException("Native word geometry-finalization/order branches require their actual source/option producers.");
        pool=actualPool;ownerC4=actualOwnerC4;interning=new();interning.Publishers.Add(this);
    }

    private LegacyNativePlainWordPublisher(LegacyNativePlainWordPublisher original)
    {pool=original.pool;ownerC4=original.ownerC4;interning=original.interning;interning.Publishers.Add(this);}

    /// <summary>Independent279 head/node budget with the identical TOP's275
    /// span interning, WordIndex identities, glyph pool and private policies.</summary>
    public LegacyNativePlainWordPublisher ForkAlternative()
    {
        if(released)throw new InvalidOperationException("A released279 list cannot fork another alternative.");
        return new(this);
    }

    //1415B0 releases only279 list nodes and their auxiliary node vectors.
    //The shared TOP's275 words/span indexes and274 pool remain owned.
    internal void ReleaseAlternativeList()
    {
        if(released)throw new InvalidOperationException("The279 list was already released.");
        Head=null;nodeCount=0;released=true;
    }

    public LegacyNativePublishedWordNode Publish(LegacyNativeWordBuilder actualWord,
        Func<int,ushort,int> actualFirstBoundary,Func<int,ushort,int> actualLastBoundary,
        CancellationToken cancellationToken=default)
        =>Publish(actualWord,actualFirstBoundary,actualLastBoundary,-1,-1,cancellationToken);

    /// <summary>1416b0 param7 replaces Word40's first scalar, param8 its last,
    /// each only when not-1. These are native span fields, not guessed source
    /// points. Both callbacks still execute before the native overrides.</summary>
    public LegacyNativePublishedWordNode Publish(LegacyNativeWordBuilder actualWord,
        Func<int,ushort,int> actualFirstBoundary,Func<int,ushort,int> actualLastBoundary,
        int actualFirstOverride,int actualLastOverride,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualWord);cancellationToken.ThrowIfCancellationRequested();
        if(released)throw new InvalidOperationException("A released279 list cannot publish another word.");
        if(!ReferenceEquals(actualWord.Pool,pool))throw new ArgumentException("Word and publisher must use the same TOP273 glyph pool.");
        if(nodeCount==4096)throw new ArgumentException("Published word nodes exceed the portable4096-node budget.");
        var span=actualWord.GetSourceSpan(actualFirstBoundary,actualLastBoundary,cancellationToken);
        if(actualFirstOverride!=-1)span=span with {First=actualFirstOverride};
        if(actualLastOverride!=-1)span=span with {Last=actualLastOverride};
        var record=InternPlainWord(actualWord,span,out int index,cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var node=new LegacyNativePublishedWordNode(span,index,record,Head);
        Head=node;nodeCount++;return node;
    }

    //13FE20 independently interns a WORD in an existing275 span owner. In
    //particular219580 must not allocate an extra279 list node when appending.
    internal LegacyNativePlainWordRecord InternPlainWord(LegacyNativeWordBuilder actualWord,
        LegacyNativeWordSourceSpan span,out int index,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualWord);cancellationToken.ThrowIfCancellationRequested();
        if(released)throw new InvalidOperationException("A released279 publisher cannot append WORD payloads.");
        if(!ReferenceEquals(actualWord.Pool,pool))throw new ArgumentException("Same TOP273 glyph pool required.");
        var chain=actualWord.CopyChain(cancellationToken);
        interning.Spans.TryGetValue(span,out var words);
        index=-1;
        if(words is not null)
            for(int i=0;i<words.Count;i++)if(words[i].HeadReference==actualWord.HeadReference){index=i;break;}
        LegacyNativePlainWordRecord record;
        if(index>=0)record=words![index]; //Native exact64-bit head identity, not labels/costs.
        else
        {
            if(interning.WordCount==4096)throw new ArgumentException("Stored words exceed the portable4096-word budget.");
            byte[] data=actualWord.CopyStorage(cancellationToken);
            int capacity=0;
            for(int count=1;count<=chain.Length;count++)if(capacity<count)capacity=Math.Max(count,(int)(capacity*1.5));
            LegacyNativeWordBuilder.W64(data,32,0);
            LegacyNativeWordBuilder.W32(data,40,chain.Length);LegacyNativeWordBuilder.W32(data,44,capacity);
            ushort flags=LegacyNativeWordBuilder.U16(data,50);bool carry=ownerC4;
            foreach(ulong reference in chain)
            {
                cancellationToken.ThrowIfCancellationRequested();byte[] glyph=pool.CopyRecord(reference,cancellationToken);
                ushort publication=LegacyNativeWordBuilder.U16(glyph,6);flags|=publication;
                if((publication&0x800)!=0&&(publication&0x400)==0)carry=false;
            }
            flags=carry?(ushort)(flags|0x400):(ushort)(flags&0xfbff);LegacyNativeWordBuilder.W16(data,50,flags);
            ulong[] flat=chain.Reverse().ToArray(); //Native prepends every chain reference, including zero-span rows.
            record=new(pool,data,flat);index=words?.Count??0;
            cancellationToken.ThrowIfCancellationRequested();
            if(words is null){words=[];interning.Spans.Add(span,words);}words.Add(record);interning.WordCount++;
        }
        return record;
    }

    //The caller supplies actual active TOP heads in native traversal order;
    //fork creation order is not silently substituted for native TOP ordering.
    internal LegacyNativePlainWordSpanSnapshot CaptureSpan(LegacyNativePublishedWordNode actualSpanNode,
        IReadOnlyList<LegacyNativePublishedWordNode?> actualHeadsInTopOrder,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualSpanNode);ArgumentNullException.ThrowIfNull(actualHeadsInTopOrder);
        token.ThrowIfCancellationRequested();
        if(released||actualHeadsInTopOrder.Count is <1 or >4096||
            !interning.Spans.TryGetValue(actualSpanNode.Span,out var words)||
            (uint)actualSpanNode.WordIndex>=words.Count||!ReferenceEquals(words[actualSpanNode.WordIndex],actualSpanNode.Word))
            throw new ArgumentException("An actual current same-TOP275 span entry required.");
        var selected=new List<LegacyNativePublishedWordNode>();var allNodes=new HashSet<LegacyNativePublishedWordNode>();int work=0;
        foreach(var head in actualHeadsInTopOrder)
        {
            token.ThrowIfCancellationRequested();
            if(!interning.Publishers.Any(p=>!p.released&&ReferenceEquals(p.Head,head)))
                throw new ArgumentException("Every supplied TOP head must be an actual active same-owner279 list.");
            var seen=new HashSet<LegacyNativePublishedWordNode>();
            for(var node=head;node is not null;node=node.Next)
            {
                token.ThrowIfCancellationRequested();
                if(!seen.Add(node)||!allNodes.Add(node)||++work>65536)throw new NotSupportedException("Bounded disjoint acyclic279 lists required.");
                if(node.Span!=actualSpanNode.Span)continue;
                if((uint)node.WordIndex>=words.Count||!ReferenceEquals(words[node.WordIndex],node.Word))
                    throw new ArgumentException("Actual279 selected ordinal and current275 row must agree.");
                selected.Add(node);
            }
        }
        if(selected.Count==0)throw new ArgumentException("Native13D360 requires an actual279 selection for this275 entry.");
        return new(this,interning,actualSpanNode.Span,words,words.ToArray(),selected.ToArray());
    }

    //Only the snapshot's one-shot apply may replace275 rows and rebind279
    //ordinals. Native source payload sidecars follow swaps, not original slots.
    internal void ApplySpanRanking(LegacyNativePlainWordSpanSnapshot snapshot,
        IReadOnlyList<byte[]> outputRows120,ReadOnlySpan<int> payloadSourceOrdinals,
        ReadOnlySpan<int> oldToNew,CancellationToken token=default)
        =>ApplySpanReplacement(snapshot,outputRows120,payloadSourceOrdinals,oldToNew,false,token);

    //275.V08/140B90 writes only its two mass channels16..23 after rank/rebind.
    internal void ApplySpanScores(LegacyNativePlainWordSpanSnapshot snapshot,
        IReadOnlyList<byte[]> outputRows120,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        int[] identity=Enumerable.Range(0,snapshot.Words.Count).ToArray();
        ApplySpanReplacement(snapshot,outputRows120,identity,identity,true,token);
    }

    private void ApplySpanReplacement(LegacyNativePlainWordSpanSnapshot snapshot,
        IReadOnlyList<byte[]> outputRows120,ReadOnlySpan<int> payloadSourceOrdinals,
        ReadOnlySpan<int> oldToNew,bool scoresOnly,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(snapshot);ArgumentNullException.ThrowIfNull(outputRows120);
        token.ThrowIfCancellationRequested();snapshot.BeginApply(this,interning);
        if(released||!interning.Spans.TryGetValue(snapshot.Span,out var words)||
            !snapshot.RetainsNativeList(words)||words.Count!=snapshot.Words.Count||
            words.Where((w,i)=>!ReferenceEquals(w,snapshot.Words[i])).Any())
            throw new InvalidOperationException("The captured275 payload owner is stale.");
        if(outputRows120.Count is <1 or >4095||payloadSourceOrdinals.Length!=outputRows120.Count||oldToNew.Length!=words.Count||
            scoresOnly&&outputRows120.Count!=words.Count)
            throw new ArgumentException("Complete native ranked rows, payload provenance and old-ordinal map required.");
        var replacement=new List<LegacyNativePlainWordRecord>(outputRows120.Count);var used=new HashSet<int>();
        for(int i=0;i<outputRows120.Count;i++)
        {
            token.ThrowIfCancellationRequested();int source=payloadSourceOrdinals[i];
            if((uint)source>=words.Count||!used.Add(source)||outputRows120[i] is null||outputRows120[i].Length!=120)
                throw new ArgumentException("Each output requires one distinct actual120-byte payload source.");
            var original=words[source];byte[] before=original.CopyStorage(),after=outputRows120[i].ToArray();
            if(!before.AsSpan(0,16).SequenceEqual(after.AsSpan(0,16))||
                !before.AsSpan(scoresOnly?24:20).SequenceEqual(after.AsSpan(scoresOnly?24:20))||
                !scoresOnly&&!before.AsSpan(16,4).SequenceEqual(after.AsSpan(16,4))&&LegacyNativeWordBuilder.I32(after,16)!=unchecked((int)0xff7fffff))
                throw new ArgumentException("Ranked payload must retain every actual source byte except the native special-cost sentinel.");
            replacement.Add(new(pool,after,original.FlatReferences.ToArray()));
        }
        for(int i=0;i<snapshot.SelectedNodes.Count;i++)
        {
            token.ThrowIfCancellationRequested();var node=snapshot.SelectedNodes[i];int original=snapshot.SelectedOrdinals[i];
            if(node.WordIndex!=original||(uint)original>=words.Count||!ReferenceEquals(words[original],node.Word)||
                (uint)oldToNew[original]>=replacement.Count)
                throw new InvalidOperationException("The selected279 payload or ranked ordinal is stale.");
        }
        //13F080 releases only each removed WORD's own vectors, not274 glyphs.
        //Immutable snapshots may remain readable; GC owns their vector memory.
        for(int i=0;i<snapshot.SelectedNodes.Count;i++)
        {int index=oldToNew[snapshot.SelectedOrdinals[i]];snapshot.SelectedNodes[i].RebindWord(index,replacement[index]);}
        interning.WordCount+=replacement.Count-words.Count;words.Clear();words.AddRange(replacement);
    }
}
