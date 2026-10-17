using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainDocumentSourceIndices.Probe")]

namespace InkRecognition;

/// <summary>One actual CHAR or WORD payload and its selected0207A0/020860
/// range header. Source identities are local typed identities, never pointers.</summary>
public sealed class LegacyOwnedMainDocumentIndexedSource
{
    public LegacyFreshMainDocumentSelectedSources State {get;}
    public bool IsWord {get;}
    public int SourceNodeIdentity {get;}
    public uint Flags {get;}
    public int NativeRangeHeaderOffset {get;}
    private readonly LegacyOwnedMainDocumentCharacterState? character;
    private readonly LegacyNativeInkPositionRange[] ranges;
    public LegacyOwnedMainDocumentCharacterState Character=>character??throw new InvalidOperationException("WORD source has no CHAR payload.");
    public LegacyOwnedMainWordSourcePayload Word=>IsWord?State.OriginalWordSource:throw new InvalidOperationException("CHAR source has no WORD payload.");
    public LegacyNativeInkPositionRange[] CopyRanges()=>ranges.ToArray();
    internal LegacyOwnedMainDocumentIndexedSource(LegacyFreshMainDocumentSelectedSources state,
        LegacyOwnedMainDocumentCharacterState? character,int offset,LegacyNativeInkPositionRange[] ranges)
    {State=state;this.character=character;IsWord=character is null;NativeRangeHeaderOffset=offset;this.ranges=ranges;
     SourceNodeIdentity=IsWord?state.OriginalWordSource.SourceNode.Identity:character!.OriginalSource.SourceNode.Identity;
     Flags=IsWord?state.WordFlags:character!.Flags;}
}

/// <summary>A portable AVL node snapshot; page identity is allocation creation
/// order, not the native address-sorted page-directory index. Cached interval
/// bounds and unknown native padding are deliberately not represented.</summary>
public sealed class LegacyMainDocumentIndexNode
{
    public LegacyMainDocumentSourceIndexSnapshot Owner {get;}
    public int Identity {get;}
    public int CreationPageIdentity {get;}
    public int Slot {get;}
    public int Left {get;}
    public int Right {get;}
    public sbyte Balance {get;}
    public int MembershipHead {get;}
    public LegacyOwnedMainDocumentIndexedSource? CurrentSource {get;}
    private readonly LegacyNativeInkPositionRange[] key;
    public LegacyNativeInkPositionRange[] CopyKey()=>key.ToArray();
    internal LegacyMainDocumentIndexNode(LegacyMainDocumentSourceIndexSnapshot owner,LegacyMainDocumentSourceIndexKernel<LegacyOwnedMainDocumentIndexedSource>.Node node,bool vector)
    {Owner=owner;Identity=node.Identity;CreationPageIdentity=node.PageIdentity;Slot=node.Slot;Left=node.Left?.Identity??0;
     Right=node.Right?.Identity??0;Balance=node.Balance;MembershipHead=node.MembershipHead;
     key=node.Key.ToArray();CurrentSource=vector?node.CurrentSource:null;}
}

public sealed record LegacyMainDocumentIndexMembership(int Identity,int CreationPageIdentity,int Slot,
    LegacyOwnedMainDocumentIndexedSource Source,int Next);

/// <summary>Read-only current prefix of one fresh native interval/vector index.
/// Vector CurrentSource is also the latest equal-key header owner; its native
/// cached-vector count is cleared by every successful source insertion.</summary>
public sealed class LegacyMainDocumentSourceIndexSnapshot
{
    public LegacyFreshMainDocumentSourceIndices Owner {get;}
    public bool IsWord {get;}
    public int IntervalRoot {get;}
    public int VectorRoot {get;}
    public bool IntervalDirty {get;}
    public int SourcesAdded {get;}
    public IReadOnlyList<LegacyMainDocumentIndexNode> Intervals {get;}
    public IReadOnlyList<LegacyMainDocumentIndexNode> Vectors {get;}
    public IReadOnlyList<LegacyMainDocumentIndexMembership> Memberships {get;}
    internal LegacyMainDocumentSourceIndexSnapshot(LegacyFreshMainDocumentSourceIndices owner,LegacyMainDocumentSourceIndexKernel<LegacyOwnedMainDocumentIndexedSource> k,bool word)
    {Owner=owner;IsWord=word;IntervalRoot=k.Intervals.Root?.Identity??0;VectorRoot=k.Vectors.Root?.Identity??0;IntervalDirty=k.Dirty;SourcesAdded=k.SourcesAdded;
     Intervals=Array.AsReadOnly(k.Intervals.Nodes.Select(n=>new LegacyMainDocumentIndexNode(this,n,false)).ToArray());
     Vectors=Array.AsReadOnly(k.Vectors.Nodes.Select(n=>new LegacyMainDocumentIndexNode(this,n,true)).ToArray());
     Memberships=Array.AsReadOnly(k.Memberships.Select(m=>new LegacyMainDocumentIndexMembership(m.Identity,m.PageIdentity,m.Slot,m.Source,m.Next)).ToArray());}
}

/// <summary>Fresh020920 indices over complete actual CHAR/WORD source owners.
/// Both passes must finish before IsReady (native2DA+1F0). Prior/reused indices,
/// prior range-header owners and native allocation failures remain unsupported.
/// This stage does not accept or publish a document.</summary>
public sealed class LegacyFreshMainDocumentSourceIndices
{
    private sealed class Entry {internal bool Claimed;}
    private static readonly ConditionalWeakTable<LegacyFreshMainWordSourceChoices,Entry> lifetime=new();
    private readonly object gate=new();
    private readonly LegacyMainDocumentSourceIndexKernel<LegacyOwnedMainDocumentIndexedSource> characters=new(),words=new();
    private readonly List<LegacyOwnedMainDocumentIndexedSource> sources=[];
    private bool entered,ready,faulted;
    public LegacyFreshMainDocumentSelectedSources State {get;}
    public bool IsReady {get{lock(gate)return ready;}}
    public bool IsFaulted {get{lock(gate)return faulted;}}
    public IReadOnlyList<LegacyOwnedMainDocumentIndexedSource> CopySources(){lock(gate)return Array.AsReadOnly(sources.ToArray());}
    public LegacyMainDocumentSourceIndexSnapshot CopyCharacterIndex(){lock(gate)return new(this,characters,false);}
    public LegacyMainDocumentSourceIndexSnapshot CopyWordIndex(){lock(gate)return new(this,words,true);}
    private LegacyFreshMainDocumentSourceIndices(LegacyFreshMainDocumentSelectedSources state){State=state;}
    public static LegacyFreshMainDocumentSourceIndices Prepare(LegacyFreshMainDocumentSelectedSources actualState,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualState);Validate(actualState,token);
        var entry=lifetime.GetValue(actualState.Text.WordChoices.Owner,_=>new());
        lock(entry)
        {token.ThrowIfCancellationRequested();if(entry.Claimed)throw new InvalidOperationException("This actual fresh document index lifetime has already been claimed.");
         entry.Claimed=true;return new(actualState);}
    }
    public LegacyFreshMainDocumentSourceIndices Run(CancellationToken token=default)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();if(entered)throw new InvalidOperationException("Fresh source indexing is single-use; failed prefixes cannot be reset.");
            Validate(State,token);entered=true;
            try
            {
                var byId=State.Characters.ToDictionary(c=>c.OriginalSource.SourceNode.Identity);
                var insertion=State.Characters[0].OriginalSource.SourceNode.Owner.CopyInsertionOrder();
                foreach(var node in insertion)
                {
                    token.ThrowIfCancellationRequested();var source=byId[node.Identity];if((source.Flags&0x200000)!=0)continue;
                    if(source.OriginalSource.PriorRangeCount!=0)throw new NotSupportedException("Actual prior CHAR range-header owner is unavailable.");
                    var ranges=source.OriginalSource.SourceNode.FirstProcessedSource?.CopyRanges()??
                        throw new NotSupportedException("Actual original CHAR key-header owner required.");
                    var indexed=new LegacyOwnedMainDocumentIndexedSource(State,source,0x20,ranges);sources.Add(indexed);characters.Add(indexed,ranges,token);
                }
                if((State.WordFlags&0x200000)==0)
                {
                    var ranges=State.OriginalWordSource.CopyOrdinaryRanges();int offset=0x30;
                    if(ranges.Length==0){ranges=State.OriginalWordSource.CopySourceKeyRanges();offset=0x20;}
                    var indexed=new LegacyOwnedMainDocumentIndexedSource(State,null,offset,ranges);sources.Add(indexed);words.Add(indexed,ranges,token);
                }
                Validate(State,token);ready=true;return this;
            }
            catch{faulted=true;throw;}
        }
    }
    internal void ValidateCurrent(CancellationToken token=default)
    {lock(gate){token.ThrowIfCancellationRequested();if(!ready||faulted)throw new InvalidOperationException("Both source index passes must have completed.");Validate(State,token);}}
    private static void Validate(LegacyFreshMainDocumentSelectedSources state,CancellationToken token)
    {
        state.Validate(token);var word=state.OriginalWordSource.SourceNode;var inserted=word.Owner.CopyInsertionOrder();
        if(inserted.Count!=1||!ReferenceEquals(inserted[0],word))
            throw new NotSupportedException("Complete actual fresh singleton WORD source list required.");
        var nodes=state.Characters[0].OriginalSource.SourceNode.Owner.CopyInsertionOrder();
        if(nodes.Count!=state.CharacterTree.Count)throw new InvalidOperationException("Complete CHAR insertion list changed.");
        for(int i=0;i<nodes.Count;i++)
        {
            var shape=state.CharacterTree.Single(n=>n.Identity==nodes[i].Identity);
            if(shape.PreviousInserted!=(i==0?0:nodes[i-1].Identity)||shape.NextInserted!=(i+1==nodes.Count?0:nodes[i+1].Identity))
                throw new InvalidOperationException("Actual CHAR insertion-list and AVL identities disagree.");
        }
    }
}
