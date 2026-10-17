using System.Runtime.CompilerServices;
using System.Security.Cryptography;

[assembly: InternalsVisibleTo("LegacyMainCompactGraph.Probe")]

namespace InkRecognition;

/// <summary>Ordinary19C4E0 group semantics after the96-to56-byte conversion.
/// Allocation IDs survive, including groups with no remaining arc references.
/// Native pointers/capacities and freed history storage are not serialized.</summary>
public sealed class LegacyMainCompactOrdinaryGroup
{
    private readonly LegacyInkRange[] ranges;
    public int AllocationId {get;}
    public ushort DescriptorOrdinal {get;}
    public ushort SecondaryDescriptorOrdinal {get;}
    public byte Flags04 {get;}
    public byte Flags05 {get;}
    public byte Flags06 {get;}
    public byte Kind07=>0;
    public LegacyMainGraphAppendBounds Bounds {get;}
    internal LegacyMainCompactOrdinaryGroup(int ordinal,LegacyMainGraphAppendGroup source)
    {
        AllocationId=ordinal;var f=source.Fields;DescriptorOrdinal=f.DescriptorOrdinal;
        SecondaryDescriptorOrdinal=f.SecondaryDescriptorOrdinal;
        Flags04=f.Flags51;Flags05=f.Flags52;Flags06=f.Flags50;
        Bounds=source.Bounds;ranges=source.CopyRanges();
    }
    public LegacyInkRange[] CopyRanges()=>ranges.ToArray();
}

/// <summary>19C310's cumulative selector boundaries and ordered ushort data.
/// Selector0 is the predecessor list; selector1..cap are outgoing span lists.
/// Root sentinel−1 is represented by its nativeFFFF ordinal.</summary>
public sealed class LegacyMainCompactOrdinaryArc
{
    private readonly ushort[] ends,indices;
    public uint GroupId {get;}
    public byte Span {get;}
    public sbyte Placement {get;}
    public byte Flags {get;}
    public uint StartContextId {get;}
    public uint EndContextId {get;}
    public ushort AuxiliaryContextIndex {get;}
    public ushort Extra1a {get;}
    public uint Scalar1cBits {get;}
    public int SelectorCount=>ends.Length;
    internal LegacyMainCompactOrdinaryArc(LegacyMainGraphAppendArc source,CancellationToken token)
    {
        GroupId=source.GroupId;Span=source.Span;Placement=source.Placement;Flags=source.Flags;
        StartContextId=source.StartContextId;EndContextId=source.EndContextId;
        AuxiliaryContextIndex=source.AuxiliaryContextIndex;Extra1a=source.Extra1a;Scalar1cBits=source.Scalar1cBits;
        var next=source.CopySuccessors();var previous=source.CopyPredecessors();
        ends=new ushort[next.Length+1];int count=previous.Length;
        if(count>2048)throw new NotSupportedException("Native19C310 selector total exceeds2048.");
        ends[0]=(ushort)count;
        for(int i=0;i<next.Length;i++)
        {
            token.ThrowIfCancellationRequested();count=checked(count+next[i].Length);
            if(count>2048)throw new NotSupportedException("Native19C310 selector total exceeds2048.");
            ends[i+1]=(ushort)count;
        }
        indices=new ushort[count];int at=0;
        foreach(int index in previous)indices[at++]=index==-1?ushort.MaxValue:checked((ushort)index);
        foreach(var row in next)foreach(int index in row)
        {token.ThrowIfCancellationRequested();indices[at++]=checked((ushort)index);}
    }
    public ushort[] CopyCumulativeEnds()=>ends.ToArray();
    public ushort[] CopyIndices()=>indices.ToArray();
    public ushort[] CopySelector(int selector)
    {
        if((uint)selector>=(uint)ends.Length)throw new ArgumentOutOfRangeException(nameof(selector));
        int first=selector==0?0:ends[selector-1];return indices.AsSpan(first,ends[selector]-first).ToArray();
    }
}

/// <summary>Compact ordinary graph projection. Preserves root-first and then
/// node/arc traversal, allocation IDs and all context/source metadata. Does not
/// invent same-source SeedGroupIndices or provide a global120 source resolver.</summary>
public sealed class LegacyMainCompactOrdinaryState
{
    private readonly LegacyMainCompactOrdinaryGroup[] groups;
    private readonly LegacyMainCompactOrdinaryArc[][] nodes;
    public int MaximumAtoms {get;}
    public int NodeCount=>nodes.Length;
    public int CandidateCount {get;}
    public IReadOnlyList<LegacyMainCompactOrdinaryGroup> Groups=>Array.AsReadOnly(groups);
    public LegacyMainCompactOrdinaryArc Root {get;}
    private LegacyMainCompactOrdinaryState(LegacyMainGraphAppendState source,CancellationToken token)
    {
        MaximumAtoms=source.MaximumAtoms;
        //The actual main cap8 fits19C310's header before its pointer at18.
        //Other compact-header layouts are not inferred from a prepared cap.
        if(MaximumAtoms is <1 or >8)throw new NotSupportedException("Restored compact ordinary cap1..8 required.");
        groups=new LegacyMainCompactOrdinaryGroup[source.GroupCount];
        for(int g=0;g<groups.Length;g++){token.ThrowIfCancellationRequested();groups[g]=new(g,source.Groups[g]);}
        Root=new(source.Root,token);nodes=new LegacyMainCompactOrdinaryArc[source.NodeCount][];int count=0;
        for(int n=0;n<nodes.Length;n++)
        {
            token.ThrowIfCancellationRequested();var input=source.GetNode(n);nodes[n]=new LegacyMainCompactOrdinaryArc[input.Count];
            for(int a=0;a<input.Count;a++){token.ThrowIfCancellationRequested();nodes[n][a]=new(input[a],token);count++;}
        }
        CandidateCount=count;
    }
    internal static LegacyMainCompactOrdinaryState FromState(LegacyMainGraphAppendState source,CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(source);token.ThrowIfCancellationRequested();return new(source,token);}
    public IReadOnlyList<LegacyMainCompactOrdinaryArc> GetNode(int node)
    {if((uint)node>=(uint)nodes.Length)throw new ArgumentOutOfRangeException(nameof(node));return Array.AsReadOnly(nodes[node]);}
}

/// <summary>Actual fresh main source-bound19C4E0 ordinary compact graph.
/// Uses the shared post-cross/post-prune state; no per-line sampler loop,
/// fabricated atom IDs, global227 context or NN-quality claim.</summary>
public sealed class LegacyFreshMainCompactOrdinaryGraph
{
    public LegacyFreshMainGlobalOrdinaryState Source {get;}
    public LegacyMainCompactOrdinaryState State {get;}
    private LegacyFreshMainCompactOrdinaryGraph(LegacyFreshMainGlobalOrdinaryState source,LegacyMainCompactOrdinaryState state)
    {Source=source;State=state;}
    public static LegacyFreshMainCompactOrdinaryGraph Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalOrdinaryState source,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(source);token.ThrowIfCancellationRequested();
        if(resource.Length!=572763||!Convert.ToHexString(SHA256.HashData(resource)).Equals(source.Source.SourceFingerprint,StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Same actual fresh main acoustic resource/source owner required.");
        var result=LegacyMainCompactOrdinaryState.FromState(source.State,token);
        token.ThrowIfCancellationRequested();return new(source,result);
    }
    public LegacyMainGlobalGroupSource ResolveGroupSource(int group,CancellationToken token=default)
        =>Source.ResolveGroupSource(group,token);
}
