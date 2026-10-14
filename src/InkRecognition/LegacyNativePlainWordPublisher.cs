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
    public int WordIndex {get;}
    public LegacyNativePlainWordRecord Word {get;}
    public LegacyNativePublishedWordNode? Next {get;}
    internal LegacyNativePublishedWordNode(LegacyNativeWordSourceSpan span,int index,
        LegacyNativePlainWordRecord word,LegacyNativePublishedWordNode? next)
    {Span=span;WordIndex=index;Word=word;Next=next;}
}

/// <summary>279.virtual10/1416b0 plain prepend branch and275/13fe20 plain
/// word interning, with geometry finalization/order and auxiliary output
/// explicitly disabled. Source-span callbacks remain actual caller data.</summary>
public sealed class LegacyNativePlainWordPublisher
{
    private readonly LegacyNativeGlyphRecordPool pool;
    private readonly bool ownerC4;
    private readonly Dictionary<LegacyNativeWordSourceSpan,List<LegacyNativePlainWordRecord>> spans=[];
    private int wordCount,nodeCount;
    public LegacyNativePublishedWordNode? Head {get;private set;}

    public LegacyNativePlainWordPublisher(LegacyNativeGlyphRecordPool actualPool,
        bool actualOwnerC4,bool actualFinalizeWordGeometry,bool actualSortByGeometry)
    {
        ArgumentNullException.ThrowIfNull(actualPool);
        if(actualFinalizeWordGeometry||actualSortByGeometry)
            throw new NotSupportedException("Native word geometry-finalization/order branches require their actual source/option producers.");
        pool=actualPool;ownerC4=actualOwnerC4;
    }

    public LegacyNativePublishedWordNode Publish(LegacyNativeWordBuilder actualWord,
        Func<int,ushort,int> actualFirstBoundary,Func<int,ushort,int> actualLastBoundary,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualWord);cancellationToken.ThrowIfCancellationRequested();
        if(!ReferenceEquals(actualWord.Pool,pool))throw new ArgumentException("Word and publisher must use the same TOP273 glyph pool.");
        if(nodeCount==4096)throw new ArgumentException("Published word nodes exceed the portable4096-node budget.");
        var span=actualWord.GetSourceSpan(actualFirstBoundary,actualLastBoundary,cancellationToken);
        var chain=actualWord.CopyChain(cancellationToken);
        spans.TryGetValue(span,out var words);
        int index=-1;
        if(words is not null)
            for(int i=0;i<words.Count;i++)if(words[i].HeadReference==actualWord.HeadReference){index=i;break;}
        LegacyNativePlainWordRecord record;
        if(index>=0)record=words![index]; //Native exact64-bit head identity, not labels/costs.
        else
        {
            if(wordCount==4096)throw new ArgumentException("Stored words exceed the portable4096-word budget.");
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
            if(words is null){words=[];spans.Add(span,words);}words.Add(record);wordCount++;
        }
        cancellationToken.ThrowIfCancellationRequested();
        var node=new LegacyNativePublishedWordNode(span,index,record,Head);
        Head=node;nodeCount++;return node;
    }
}
