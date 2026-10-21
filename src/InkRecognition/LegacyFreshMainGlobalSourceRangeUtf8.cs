using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyGlobalSourceRangeUtf8.Probe")]
namespace InkRecognition;

/// <summary>Owned source-coordinate serialization, not a DOM node or registry selection.</summary>
public sealed class LegacyGlobalSourceRangeUtf8Entry
{
    private readonly LegacyNativeInkPositionRange[] original,normalized;
    private readonly byte[] bytes,callerBytes;
    public LegacyFreshMainGlobalSourceRangeUtf8 Owner{get;}
    public string Kind{get;}
    public int SourceNodeIdentity{get;}
    public uint SourceFlags{get;}
    public LegacyFreshMainGlobalTextSourcePayload? CharacterSource{get;}
    public LegacyFreshMainGlobalWordSourcePayload? WordSource{get;}
    public LegacyFreshMainGlobalTextSelectionReceipt? TextSource{get;}
    internal LegacyGlobalSourceRangeUtf8Entry(LegacyFreshMainGlobalSourceRangeUtf8 owner,string kind,int identity,uint flags,
        LegacyNativeInkPositionRange[] ranges,LegacyPreparedRangeUtf8 formatted,
        LegacyFreshMainGlobalTextSourcePayload? character=null,LegacyFreshMainGlobalWordSourcePayload? word=null,
        LegacyFreshMainGlobalTextSelectionReceipt? text=null)
    {
        Owner=owner;Kind=kind;SourceNodeIdentity=identity;SourceFlags=flags;original=ranges;
        normalized=formatted.Ranges;bytes=formatted.Bytes;callerBytes=LegacyNativeSourceRangeUtf8.CallerFirstBrace(bytes,flags);
        CharacterSource=character;WordSource=word;TextSource=text;
    }
    public LegacyNativeInkPositionRange[] CopyOriginalRanges(){Owner.ValidateCurrent();return original.ToArray();}
    public LegacyNativeInkPositionRange[] CopyNormalizedRanges(){Owner.ValidateCurrent();return normalized.ToArray();}
    public byte[] CopyUtf8(){Owner.ValidateCurrent();return bytes.ToArray();}
    public byte[] CopyCallerUtf8(){Owner.ValidateCurrent();return callerBytes.ToArray();}
}

/// <summary>Actual global CHAR insertion order, raw WORD and TEXT range strings
/// after operation7. No pending command is executed and no document is accepted.</summary>
public sealed class LegacyFreshMainGlobalSourceRangeUtf8
{
    private LegacyGlobalSourceRangeUtf8Entry[] sources=[];
    public LegacyFreshMainGlobalCounterHandoff CounterHandoff{get;}
    public LegacyOwnedRawInkDocument Document=>CounterHandoff.Transforms.Seed.Document;
    public IReadOnlyList<LegacyGlobalSourceRangeUtf8Entry> Sources
    {get{ValidateCurrent();return Array.AsReadOnly(sources);}}
    private LegacyFreshMainGlobalSourceRangeUtf8(LegacyFreshMainGlobalCounterHandoff actual){CounterHandoff=actual;}
    public static LegacyFreshMainGlobalSourceRangeUtf8 Prepare(LegacyFreshMainGlobalCounterHandoff actual,CancellationToken token=default)
        =>PrepareCore(actual,null,token);
    internal static LegacyFreshMainGlobalSourceRangeUtf8 PrepareDiagnostic(LegacyFreshMainGlobalCounterHandoff actual,
        Action<int> checkpoint,CancellationToken token=default)=>PrepareCore(actual,checkpoint,token);
    private static LegacyFreshMainGlobalSourceRangeUtf8 PrepareCore(LegacyFreshMainGlobalCounterHandoff actual,
        Action<int>? checkpoint,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(actual);actual.ValidateCurrent(token);
        var cleanup=actual.Transforms.Cleanup;var state=cleanup.State;var parent=state.Publication;
        var owner=new LegacyFreshMainGlobalSourceRangeUtf8(actual);
        if(!ReferenceEquals(parent.Scope.Document,owner.Document)||!ReferenceEquals(parent.Text.Selection?.Owner,actual.TransactionOwner))
            throw new ArgumentException("Same actual global source document and pending transaction required.");
        var nodes=parent.Wrapper.CharacterInterner.CopyInsertionOrder();
        var byId=state.Characters.ToDictionary(c=>c.OriginalSource.Node.Identity);
        if(nodes.Count is<1 or>4096||nodes.Count!=byId.Count||!cleanup.CopyCharacterVisitOrder().SequenceEqual(nodes.Select(n=>n.Identity)))
            throw new NotSupportedException("Complete actual cleaned CHAR insertion list required.");
        //This first copy is already bounded by the genuine parent publication's
        //whole projected-range budget. Preflight all NEW formatter work before
        //copying CHAR/WORD keys or allocating output buffers.
        var text=parent.Text.CopyRanges();
        var counts=new int[nodes.Count+2];
        for(int i=0;i<nodes.Count;i++)
        {
            token.ThrowIfCancellationRequested();var node=nodes[i];
            if(!byId.TryGetValue(node.Identity,out var c)||!ReferenceEquals(c.OriginalSource.Node,node)||
                !ReferenceEquals(node.FirstGlobalSource,c.OriginalSource.Source)||!ReferenceEquals(c.OriginalSource.Owner,parent.Wrapper))
                throw new InvalidOperationException("Actual global CHAR identity changed.");
            counts[i]=node.RangeCount;
        }
        counts[^2]=parent.WordSource.Node.RangeCount;counts[^1]=text.Length;Preflight(counts);
        var operations=LegacyOwnedRawInkRangeOperations.Prepare(owner.Document);var output=new List<LegacyGlobalSourceRangeUtf8Entry>(counts.Length);
        foreach(var node in nodes)
        {
            var character=byId[node.Identity];Add("CHAR",node.Identity,character.Flags,
                character.OriginalSource.Source.CopyRanges(token),character.OriginalSource);
        }
        Add("WORD",parent.WordSource.Node.Identity,cleanup.WordFlags,parent.OriginalWordSource.CopyOriginalKey(),word:parent.WordSource);
        Add("TEXT",0,0,text,textSource:parent.Text);
        actual.ValidateCurrent(token);owner.sources=output.ToArray();return owner;

        void Add(string kind,int id,uint flags,LegacyNativeInkPositionRange[] ranges,
            LegacyFreshMainGlobalTextSourcePayload? character=null,LegacyFreshMainGlobalWordSourcePayload? word=null,
            LegacyFreshMainGlobalTextSelectionReceipt? textSource=null)
        {
            token.ThrowIfCancellationRequested();int index=output.Count;
            if(ranges.Length!=counts[index])throw new InvalidOperationException("Actual source key count changed after work preflight.");
            foreach(var range in ranges){Position(range.First);Position(range.Last);}
            var formatted=LegacyNativeSourceRangeUtf8.Format(ranges,operations,token:token);
            if(!formatted.Succeeded)throw new InvalidOperationException("Source range formatting failed.");
            output.Add(new(owner,kind,id,flags,ranges,formatted,character,word,textSource));
            checkpoint?.Invoke(output.Count);token.ThrowIfCancellationRequested();
        }
        void Position(LegacyNativeInkPosition p)
        {
            if(p.SourceIndex>=(uint)owner.Document.ObjectCount||p.PointIndex>=owner.Document.Strokes[(int)p.SourceIndex].Points.Count)
                throw new NotSupportedException("Original raw-object coordinates required; registry IDs are not source IDs.");
            LegacyNativeSourceRangeUtf8.ValidatePosition(p);
        }
    }
    internal static long Preflight(ReadOnlySpan<int> counts)
    {
        if(counts.Length is<3 or>4098)throw new NotSupportedException("Bounded complete source inventory required.");
        long total=0,work=0;
        foreach(int count in counts)
        {
            if(count is<0 or>65536)throw new NotSupportedException("Bounded source ranges required.");
            total+=count;work+=(long)count*count;
            if(total>65536||work>16_777_216)throw new NotSupportedException("Whole source range/canonicalization budget exceeded.");
        }
        //Copies, canonicalization scratch and ASCII output, not an RSS promise.
        return checked(total*256+counts.Length*256L);
    }
    public void ValidateCurrent(CancellationToken token=default)=>CounterHandoff.ValidateCurrent(token);
}
