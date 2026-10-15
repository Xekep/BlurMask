using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Owned ordinary21B source callbacks used by main frontend1A0/1A8.
/// Custom-token and known-input group types are absent from this graph producer.
/// Interior kind1 geometry-driven delimiters are not restored here.</summary>
public sealed class LegacyOwnedRussianMainSourceSymbolCallbacks
{
    public LegacyOwnedRussianMainGlyphConfidence Owner {get;}
    public LegacyOrdinaryCursiveCandidateGraph Graph=>Owner.Graph;
    private readonly bool assignedLineNoAsianDelimiter;
    private LegacyOwnedRussianMainSourceSymbolCallbacks(LegacyOwnedRussianMainGlyphConfidence owner,bool assignedLineNoAsianDelimiter=false)
    {Owner=owner;this.assignedLineNoAsianDelimiter=assignedLineNoAsianDelimiter;}

    public static LegacyOwnedRussianMainSourceSymbolCallbacks Prepare(LegacyOwnedRussianMainGlyphConfidence owner,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(owner);cancellationToken.ThrowIfCancellationRequested();
        if(GraphInvalid(owner.Graph)||owner.Model.SourceKind!=0x135||
            (owner.CaseDictionaryOwner is null)==(owner.DirectDictionaryOwner is null))
            throw new ArgumentException("Actual owned Russian ordinary main source required.");
        return new(owner);
    }

    public static LegacyOwnedRussianMainSourceSymbolCallbacks Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedRussianMainGlyphConfidence owner,LegacyFreshSingleGlyphMainSampled source,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(source);_=Prepare(owner,token);
        return PrepareAssigned(resource,owner,source.SourceRanges.Graph,source.Owner.GraphOffset,
            source.Preparation.LinePreparation.Output.CopyDescriptor().Span,token);
    }

    public static LegacyOwnedRussianMainSourceSymbolCallbacks Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedRussianMainGlyphConfidence owner,LegacyFreshMainSingleLineSampled source,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(source);_=Prepare(owner,token);
        return PrepareAssigned(resource,owner,source.SourceRanges.Graph,source.Owner.GraphOffset,source.Owner.CopyDescriptor().Span,token);
    }

    private static LegacyOwnedRussianMainSourceSymbolCallbacks PrepareAssigned(ReadOnlySpan<byte> resource,
        LegacyOwnedRussianMainGlyphConfidence owner,LegacyOrdinaryCursiveCandidateGraph graph,int graphOffset,
        ReadOnlySpan<byte> descriptor,CancellationToken token)
    {
        if(!ReferenceEquals(graph,owner.Graph)||resource.Length!=572763||
            !SHA256.HashData(resource).AsSpan().SequenceEqual(Convert.FromHexString("7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978")))
            throw new ArgumentException("Same owned actual main resource/source graph required.");
        _=LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(resource,graphOffset);
        if(graphOffset!=189379||resource[graphOffset+350]!=0||descriptor.Length<44||
            (U16(descriptor,0)&15)!=0||I32(descriptor,16)<0)
            throw new NotSupportedException("Actual ordinary assigned source line and disabled graph159 Asian delimiter required.");
        token.ThrowIfCancellationRequested();return new(owner,true);
    }

    /// <summary>152DC0 resolves graph68's group then graph88's type. Every group
    /// in the owned ordinary source is type0, so the known-input type1 label
    /// lookup is not entered and frontend1A8 returnsFFFF. This is not a guessed
    /// replacement label, nor support for known-input source records.</summary>
    public ushort ReadFallback(int node,ushort arc,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();ValidateArc(node,arc);
        return ushort.MaxValue;
    }

    /// <summary>216860's FFFD branch over this actual type0 callback. AFFFF
    /// result retains the actual replacement label and externalvariant0.</summary>
    public LegacyRussianMainReplacementResolution ResolveReplacement(int node,ushort arc,CancellationToken token=default)
    {
        _=ReadFallback(node,arc,token);
        if(!Owner.Model.AcousticAlphabet.TryGetLabel("\ufffd",out ushort replacement))
            throw new InvalidDataException("Actual replacement label absent.");
        token.ThrowIfCancellationRequested();return new(replacement,replacement,0);
    }

    /// <summary>1AAE70 request64 input offsets: previous node8/arcC,
    /// current node18/arc1C, kind28. The request is explicitly prepared, not
    /// reconstructed from a winning row here. Cursor owns a256-label buffer.
    /// Result flagsC/D are the actual empty-metadata path's zeros.</summary>
    public LegacyOwnedMainDelimiterResult ReadDelimiter(ReadOnlySpan<byte> preparedRequest64,int cursor,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(preparedRequest64.Length!=64)throw new ArgumentException("Complete prepared native delimiter request64 required.");
        if(cursor is <0 or >256)throw new ArgumentOutOfRangeException(nameof(cursor),"Portable256-label scratch bound exceeded.");
        int previous=I32(preparedRequest64,8),current=I32(preparedRequest64,0x18);
        ushort previousArc=U16(preparedRequest64,0xc),currentArc=U16(preparedRequest64,0x1c);
        if(previous!=-1)ValidateArc(previous,previousArc);
        var currentRecord=current==Graph.NodeCount?null:ValidateArc(current,currentArc);
        //Ordinary19EB10 metadata=-1;1A1EF0/1A37B0 preserve that index;
        //fresh1A7230 fallback.count208=0.1AAE70 sets outputC/D=0 before
        //the null-buffer/cursor<1 return and before reading delimiter kind.
        if(cursor==0||previous==-1||current==Graph.NodeCount)return new(this,cursor,[]);
        int kind=I32(preparedRequest64,0x28);
        //Kind2's conjunction requires BOTH source descriptor line IDs=-1.
        //Our owned groups retain descriptor0, whose actual line is assigned.
        //Thus the alternative branch tests graph159; checked main159=0 exits.
        if(kind==2&&assignedLineNoAsianDelimiter)
            return (currentRecord!.Flags&8)!=0?new(this,cursor-1,[0xfffb]):new(this,cursor,[]);
        if(kind!=0)throw new NotSupportedException("Interior kind1/2 needs its actual source geometry/category delimiter policy.");
        //Both ordinary arc owners exist, no custom metadata; kind0 writes
        //FFFC in both the1D8 shortcut and the ordinary geometry branch.
        return new(this,cursor-1,[0xfffc]);
    }

    private LegacyOrdinaryCursiveCandidateArc ValidateArc(int node,ushort ordinal)
    {
        var arcs=Graph.GetNode(node);
        if(ordinal>=arcs.Count||(uint)arcs[ordinal].GroupId>=(uint)Graph.Groups.Count)
            throw new ArgumentException("Actual ordinary source node/arc/group required.");
        return arcs[ordinal];
    }
    private static bool GraphInvalid(LegacyOrdinaryCursiveCandidateGraph graph)=>graph.NodeCount is <1 or >4096||graph.CandidateCount>131072;
    private static int I32(ReadOnlySpan<byte> data,int at)=>BinaryPrimitives.ReadInt32LittleEndian(data[at..]);
    private static ushort U16(ReadOnlySpan<byte> data,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(data[at..]);
}

public readonly record struct LegacyRussianMainReplacementResolution(ushort EmittedSymbol,ushort NormalizedOriginal,byte ExternalVariant);

public sealed class LegacyOwnedMainDelimiterResult
{
    private readonly ushort[] symbols;
    public LegacyOwnedRussianMainSourceSymbolCallbacks Owner {get;}
    public int RemainingCursor {get;}
    public byte NativeFlag0C=>0;
    public byte NativeFlag0D=>0;
    public int EmittedCount=>symbols.Length;
    internal LegacyOwnedMainDelimiterResult(LegacyOwnedRussianMainSourceSymbolCallbacks owner,int cursor,ushort[] symbols)
    {Owner=owner;RemainingCursor=cursor;this.symbols=symbols;}
    public ushort[] CopySymbols()=>symbols.ToArray();
}
