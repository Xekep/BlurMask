using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainTextSourceInterner.Probe")]

namespace InkRecognition;

/// <summary>One01B4E0 source-key node. Identities are local to this owner and
/// are not native addresses. The source payload initialization024D40 remains
/// a later operation; interning does not manufacture glyph/text metadata.</summary>
public sealed class LegacyOwnedMainTextSourceNode
{
    private readonly LegacyNativeInkPositionRange[] ranges;
    private readonly LegacyOwnedMainGlyphDocumentPositions? firstSource;
    public LegacyOwnedMainTextSourceInterner Owner { get; }
    public int Identity { get; }
    public int RangeCount => ranges.Length;
    public LegacyOwnedMainProcessedGlyphSource? FirstProcessedSource {get;}
    public LegacyOwnedMainWordSourceCollection? FirstWordSource {get;}
    public LegacyOwnedMainGlyphDocumentPositions FirstSourcePositions => firstSource ??
        throw new InvalidOperationException("Explicitly prepared numeric node has no actual source-position producer.");
    internal LegacyOwnedMainTextSourceNode? Left,Right,Previous,Next;
    internal int Height=1;
    internal ReadOnlySpan<LegacyNativeInkPositionRange> Key=>ranges;
    internal LegacyOwnedMainTextSourceNode(LegacyOwnedMainTextSourceInterner owner,int identity,
        LegacyNativeInkPositionRange[] key,LegacyOwnedMainGlyphDocumentPositions? source,
        LegacyOwnedMainProcessedGlyphSource? processed=null,LegacyOwnedMainWordSourceCollection? wordSource=null)
    {Owner=owner;Identity=identity;ranges=key;firstSource=source;FirstProcessedSource=processed;FirstWordSource=wordSource;}
    public byte[] CopyRanges16()
    {
        byte[] bytes=new byte[ranges.Length*16];
        for(int i=0;i<ranges.Length;i++)
        {BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(i*16),ranges[i].First.Value);
         BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(i*16+8),ranges[i].Last.Value);}
        return bytes;
    }
}

public readonly record struct LegacyOwnedMainTextSourceInternResult(LegacyOwnedMainTextSourceNode Node,bool Inserted);
public readonly record struct LegacyMainTextSourceTreeSnapshot(int Identity,int Left,int Right,sbyte Balance,
    int PreviousInserted,int NextInserted);

/// <summary>Fresh01B4E0 ordered source-range interner, bound to one actual raw
/// root document. Key comparison is exact InkC21.F8/081870: ordered unsigned64
/// first/last pairs then unsigned count, with no sorting or geometric union.</summary>
public sealed class LegacyOwnedMainTextSourceInterner
{
    private readonly List<LegacyOwnedMainTextSourceNode> inserted=[];
    private readonly object gate=new();
    private LegacyOwnedMainTextSourceNode? root;
    private int rangeCount;
    public LegacyOwnedRawInkDocument Document { get; }
    public int Count {get{lock(gate)return inserted.Count;}}
    private LegacyOwnedMainTextSourceInterner(LegacyOwnedRawInkDocument document){Document=document;}
    public static LegacyOwnedMainTextSourceInterner Prepare(LegacyOwnedRawInkDocument actualDocument,
        CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(actualDocument);token.ThrowIfCancellationRequested();return new(actualDocument);}

    public LegacyOwnedMainTextSourceInternResult Intern(LegacyOwnedMainGlyphDocumentPositions positions,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(positions);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(positions.Document,Document)||
            !ReferenceEquals(positions.Result.Owner.SourceDocument,Document))
            throw new ArgumentException("Source positions from this identical original raw document required.");
        return InternCore(positions.Ranges.ToArray(),positions,token);
    }

    public LegacyOwnedMainTextSourceInternResult Intern(LegacyOwnedMainProcessedGlyphSource actualSource,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualSource);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(actualSource.Owner.Document,Document)||
            !ReferenceEquals(actualSource.OriginalPositions.Document,Document))
            throw new ArgumentException("Processed source key from this identical NEW document required.");
        return InternCore(actualSource.CopyRanges(),actualSource.OriginalPositions,token,actualSource);
    }

    internal LegacyOwnedMainTextSourceInternResult InternPrepared(ReadOnlySpan<LegacyNativeInkPositionRange> key,
        CancellationToken token=default)=>InternCore(key.ToArray(),null,token);

    internal LegacyOwnedMainTextSourceInternResult InternWordSource(LegacyOwnedMainWordSourceCollection source,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(source);source.CandidateSource.Validate(token);
        if(!ReferenceEquals(source.Document,Document))throw new ArgumentException("WORD source from the identical actual raw document required.");
        return InternCore(source.CopyRanges(),null,token,wordSource:source);
    }

    private LegacyOwnedMainTextSourceInternResult InternCore(LegacyNativeInkPositionRange[] key,
        LegacyOwnedMainGlyphDocumentPositions? source,CancellationToken token,
        LegacyOwnedMainProcessedGlyphSource? processed=null,LegacyOwnedMainWordSourceCollection? wordSource=null)
    {
        token.ThrowIfCancellationRequested();
        if(key.Length>65536)throw new ArgumentOutOfRangeException(nameof(key));
        lock(gate)
        {
            var current=root;
            while(current is not null)
            {
                token.ThrowIfCancellationRequested();int comparison=Compare(key,current.Key);
                if(comparison==0)return new(current,false);
                current=comparison<0?current.Left:current.Right;
            }
            if(inserted.Count==65536||key.Length>1_048_576-rangeCount)
                throw new NotSupportedException("Portable source-node/range budget exceeded.");
            token.ThrowIfCancellationRequested();
            //No callbacks/cancellation occur once links are changed. Native
            //01B4E0 has no cancellation callback inside its AVL mutation.
            var node=new LegacyOwnedMainTextSourceNode(this,inserted.Count+1,key,source,processed,wordSource);
            if(inserted.Count!=0){node.Previous=inserted[^1];inserted[^1].Next=node;}
            root=Insert(root,node);inserted.Add(node);rangeCount+=key.Length;
            return new(node,true);
        }
    }

    public IReadOnlyList<LegacyOwnedMainTextSourceNode> CopyInsertionOrder()
    {lock(gate)return Array.AsReadOnly(inserted.ToArray());}

    public int RootIdentity {get{lock(gate)return root?.Identity??0;}}

    public LegacyMainTextSourceTreeSnapshot[] CopyTree()
    {
        lock(gate)return inserted.Select(n=>new LegacyMainTextSourceTreeSnapshot(n.Identity,n.Left?.Identity??0,
            n.Right?.Identity??0,(sbyte)(Height(n.Right)-Height(n.Left)),n.Previous?.Identity??0,n.Next?.Identity??0)).ToArray();
    }

    /// <summary>Exact081870 unsigned lexical comparison. Distinct interval
    /// order, duplicates and segmentation remain distinct keys.</summary>
    public static int Compare(ReadOnlySpan<LegacyNativeInkPositionRange> first,ReadOnlySpan<LegacyNativeInkPositionRange> second)
    {
        int count=Math.Min(first.Length,second.Length);
        for(int i=0;i<count;i++)
        {
            ulong a=first[i].First.Value,b=second[i].First.Value;
            if(a!=b)return a<b?-1:1;
            a=first[i].Last.Value;b=second[i].Last.Value;if(a!=b)return a<b?-1:1;
        }
        return first.Length==second.Length?0:first.Length<second.Length?-1:1;
    }

    private static int Height(LegacyOwnedMainTextSourceNode? n)=>n?.Height??0;
    private static void Update(LegacyOwnedMainTextSourceNode n)=>n.Height=1+Math.Max(Height(n.Left),Height(n.Right));
    private static LegacyOwnedMainTextSourceNode Insert(LegacyOwnedMainTextSourceNode? parent,LegacyOwnedMainTextSourceNode node)
    {
        if(parent is null)return node;
        if(Compare(node.Key,parent.Key)<0)parent.Left=Insert(parent.Left,node);else parent.Right=Insert(parent.Right,node);
        Update(parent);int balance=Height(parent.Right)-Height(parent.Left);
        if(balance<-1)
        {if(Compare(node.Key,parent.Left!.Key)>0)parent.Left=RotateLeft(parent.Left);return RotateRight(parent);}
        if(balance>1)
        {if(Compare(node.Key,parent.Right!.Key)<0)parent.Right=RotateRight(parent.Right);return RotateLeft(parent);}
        return parent;
    }
    private static LegacyOwnedMainTextSourceNode RotateLeft(LegacyOwnedMainTextSourceNode n)
    {var r=n.Right!;n.Right=r.Left;r.Left=n;Update(n);Update(r);return r;}
    private static LegacyOwnedMainTextSourceNode RotateRight(LegacyOwnedMainTextSourceNode n)
    {var l=n.Left!;n.Left=l.Right;l.Right=n;Update(n);Update(l);return l;}
}
