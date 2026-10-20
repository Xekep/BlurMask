using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyGlobalDocumentSourceIndices.Probe")]
namespace InkRecognition;

public sealed class LegacyGlobalDocumentIndexedSource
{
    public LegacyFreshMainGlobalDocumentSelectedSources State{get;}
    public bool IsWord{get;}
    public int SourceNodeIdentity{get;}
    public uint Flags{get;}
    public int NativeRangeHeaderOffset{get;}
    private readonly LegacyGlobalDocumentCharacterState? character;
    private readonly LegacyNativeInkPositionRange[] key;
    public LegacyGlobalDocumentCharacterState Character=>character??throw new InvalidOperationException("WORD source has no CHAR payload.");
    public LegacyFreshMainGlobalWordSourcePayload Word=>IsWord?State.Publication.WordSource:throw new InvalidOperationException("CHAR source has no WORD payload.");
    internal LegacyGlobalDocumentIndexedSource(LegacyFreshMainGlobalDocumentSelectedSources state,LegacyGlobalDocumentCharacterState? character,int header,LegacyNativeInkPositionRange[] ranges)
    {State=state;this.character=character;IsWord=character is null;SourceNodeIdentity=IsWord?state.Publication.WordSource.Node.Identity:character!.OriginalSource.Node.Identity;
     Flags=IsWord?state.WordFlags:character!.Flags;NativeRangeHeaderOffset=header;key=ranges;}
    public LegacyNativeInkPositionRange[] CopyRanges()=>key.ToArray();
    internal ReadOnlySpan<LegacyNativeInkPositionRange> Key=>key;
}
public sealed class LegacyGlobalDocumentIndexNode
{
    public LegacyGlobalDocumentSourceIndexSnapshot Owner{get;}
    public int Identity{get;}
    public int CreationPageIdentity{get;}
    public int Slot{get;}
    public int Left{get;}
    public int Right{get;}
    public sbyte Balance{get;}
    public int MembershipHead{get;}
    public LegacyGlobalDocumentIndexedSource? CurrentSource{get;}
    /// <summary>0207A0/020860 clears the vector's C21 cached count after every
    /// successful insertion. Interval nodes have no such scalar.</summary>
    public int? CachedVectorCount{get;}
    private readonly LegacyNativeInkPositionRange[] key;
    internal LegacyGlobalDocumentIndexNode(LegacyGlobalDocumentSourceIndexSnapshot owner,LegacyMainDocumentSourceIndexKernel<LegacyGlobalDocumentIndexedSource>.Node node,bool vector)
    {Owner=owner;Identity=node.Identity;CreationPageIdentity=node.PageIdentity;Slot=node.Slot;Left=node.Left?.Identity??0;Right=node.Right?.Identity??0;
     Balance=node.Balance;MembershipHead=node.MembershipHead;CurrentSource=vector?node.CurrentSource:null;CachedVectorCount=vector?0:null;key=node.Key.ToArray();}
    public LegacyNativeInkPositionRange[] CopyKey()=>key.ToArray();
}
public sealed record LegacyGlobalDocumentIndexMembership(int Identity,int CreationPageIdentity,int Slot,LegacyGlobalDocumentIndexedSource Source,int Next);
public sealed class LegacyGlobalDocumentSourceIndexSnapshot
{
    public LegacyFreshMainGlobalDocumentSourceIndices Owner{get;}
    public bool IsWord{get;}
    public int IntervalRoot{get;}
    public int VectorRoot{get;}
    public bool IntervalDirty{get;}
    public int SourcesAdded{get;}
    public IReadOnlyList<LegacyGlobalDocumentIndexNode> Intervals{get;}
    public IReadOnlyList<LegacyGlobalDocumentIndexNode> Vectors{get;}
    public IReadOnlyList<LegacyGlobalDocumentIndexMembership> Memberships{get;}
    internal LegacyGlobalDocumentSourceIndexSnapshot(LegacyFreshMainGlobalDocumentSourceIndices owner,LegacyMainDocumentSourceIndexKernel<LegacyGlobalDocumentIndexedSource> kernel,bool word)
    {Owner=owner;IsWord=word;IntervalRoot=kernel.Intervals.Root?.Identity??0;VectorRoot=kernel.Vectors.Root?.Identity??0;IntervalDirty=kernel.Dirty;SourcesAdded=kernel.SourcesAdded;
     Intervals=Array.AsReadOnly(kernel.Intervals.Nodes.Select(n=>new LegacyGlobalDocumentIndexNode(this,n,false)).ToArray());
     Vectors=Array.AsReadOnly(kernel.Vectors.Nodes.Select(n=>new LegacyGlobalDocumentIndexNode(this,n,true)).ToArray());
     Memberships=Array.AsReadOnly(kernel.Memberships.Select(m=>new LegacyGlobalDocumentIndexMembership(m.Identity,m.PageIdentity,m.Slot,m.Source,m.Next)).ToArray());}
}
public readonly record struct LegacyGlobalDocumentIndexPrefix(string Stage,int CharacterSources,int CharacterIntervals,int CharacterVectors,int CharacterMemberships,bool CharacterDirty,
    int WordSources,int WordIntervals,int WordVectors,int WordMemberships,bool WordDirty);

/// <summary>Fresh020920 index owner for the exact actual global selected-source
/// overlay. Uses unchanged numeric kernel; no configured2 or native pointers.</summary>
public sealed class LegacyFreshMainGlobalDocumentSourceIndices
{
    private sealed class Entry{internal bool Claimed;}
    private static readonly ConditionalWeakTable<LegacyFreshMainGlobalWordTextPublication,Entry> lifetimes=new();
    private readonly object gate=new();
    private readonly LegacyMainDocumentSourceIndexKernel<LegacyGlobalDocumentIndexedSource> chars=new(),words=new();
    private readonly List<LegacyGlobalDocumentIndexedSource> sources=[];
    private readonly List<LegacyGlobalDocumentIndexPrefix> prefixes=[];
    private readonly LegacyGlobalDocumentIndexedSource[] plans;
    private readonly long next;
    private bool entered,ready,faulted;
    public LegacyFreshMainGlobalDocumentSelectedSources State{get;}
    public bool IsReady{get{lock(gate)return ready;}}
    public bool IsFaulted{get{lock(gate)return faulted;}}
    public int PreflightSourceCount=>plans.Length;
    public int PreflightMembershipCount{get;}
    private LegacyFreshMainGlobalDocumentSourceIndices(LegacyFreshMainGlobalDocumentSelectedSources state,LegacyGlobalDocumentIndexedSource[] plans,int memberships)
    {State=state;this.plans=plans;PreflightMembershipCount=memberships;next=state.Publication.Scope.SharedState.Selections.NextSelectionId;}
    public static LegacyFreshMainGlobalDocumentSourceIndices Prepare(LegacyFreshMainGlobalDocumentSelectedSources actual,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actual);actual.Validate(token);var publication=actual.Publication;
        var nodes=publication.Wrapper.CharacterInterner.CopyInsertionOrder();var shape=actual.CharacterTree.ToDictionary(n=>n.Identity);
        var byId=actual.Characters.ToDictionary(c=>c.OriginalSource.Node.Identity);
        if(nodes.Count!=byId.Count||nodes.Count!=shape.Count)throw new InvalidOperationException("Complete actual CHAR insertion list required.");
        var plans=new List<LegacyGlobalDocumentIndexedSource>();
        for(int i=0;i<nodes.Count;i++)
        {
            token.ThrowIfCancellationRequested();var node=nodes[i];
            if(!byId.TryGetValue(node.Identity,out var character)||!shape.TryGetValue(node.Identity,out var row)||
                !ReferenceEquals(character.OriginalSource.Node,node)||row.PreviousInserted!=(i==0?0:nodes[i-1].Identity)||row.NextInserted!=(i+1==nodes.Count?0:nodes[i+1].Identity))
                throw new InvalidOperationException("Actual CHAR AVL and insertion-list identities disagree.");
            if((character.Flags&0x200000)!=0)continue;
            var source=character.OriginalSource;
            if(source.PriorRangeCount!=0||!ReferenceEquals(node.FirstGlobalSource,source.Source))throw new NotSupportedException("Actual original global CHAR range header required; prior headers are excluded.");
            plans.Add(new(actual,character,0x20,source.Source.CopyRanges(token)));
        }
        var word=publication.WordSource;var wordNodes=publication.Scope.SharedState.Words.CopyInsertionOrder();
        if(wordNodes.Count!=1||!ReferenceEquals(wordNodes[0],word.Node)||!ReferenceEquals(word.Node.FirstGlobalWordSource,publication.OriginalWordSource))
            throw new NotSupportedException("Complete actual singleton global WORD insertion list required.");
        if((actual.WordFlags&0x200000)==0)
        {
            var ranges=word.CopyOrdinaryRanges();int header=0x30;if(ranges.Length==0){ranges=publication.OriginalWordSource.CopyOriginalKey();header=0x20;}
            plans.Add(new(actual,null,header,ranges));
        }
        int memberships=0;
        foreach(bool isWord in new[]{false,true})
        {
            var bounds=new LegacyGlobalDocumentIndexBudget();
            foreach(var plan in plans.Where(p=>p.IsWord==isWord)){token.ThrowIfCancellationRequested();bounds.Add(plan.Key);}
            memberships=checked(memberships+bounds.Memberships);
        }
        //Preflight all complete keys/counts before claiming the real invocation.
        actual.Validate(token);var entry=lifetimes.GetValue(publication,_=>new());
        lock(entry)
        {
            token.ThrowIfCancellationRequested();actual.Validate(token);
            if(entry.Claimed)throw new InvalidOperationException("This actual global publication index lifetime is already claimed.");
            var result=new LegacyFreshMainGlobalDocumentSourceIndices(actual,plans.ToArray(),memberships);entry.Claimed=true;return result;
        }
    }
    public LegacyFreshMainGlobalDocumentSourceIndices Run(CancellationToken token=default)=>RunCore(-1,token);
    internal LegacyFreshMainGlobalDocumentSourceIndices RunPreparedFailure(int afterCompletedSources,CancellationToken token=default)
    {if(afterCompletedSources<0||afterCompletedSources>plans.Length)throw new ArgumentOutOfRangeException(nameof(afterCompletedSources));return RunCore(afterCompletedSources,token);}
    private LegacyFreshMainGlobalDocumentSourceIndices RunCore(int failAfter,CancellationToken token)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();if(entered)throw new InvalidOperationException("Global source indexing is single-use; entered failures are poisoned.");
            ValidateState(token);entered=true;
            try
            {
                Record("entered");if(failAfter==0)throw new NotSupportedException("Prepared entered prefix failure.");
                foreach(var plan in plans)
                {
                    token.ThrowIfCancellationRequested();sources.Add(plan);Record(plan.IsWord?"word-enter":"char-enter");
                    (plan.IsWord?words:chars).Add(plan,plan.Key,token);Record(plan.IsWord?"word-complete":"char-complete");
                    if(sources.Count==failAfter)throw new NotSupportedException("Prepared completed-source prefix failure.");
                }
                ValidateState(token);ready=true;Record("ready");return this;
            }
            catch{faulted=true;Record("fault");throw;}
        }
    }
    private void ValidateState(CancellationToken token)
    {
        State.Validate(token);var selection=State.Publication.Scope.SharedState.Selections;
        if(selection.Failed||selection.NextSelectionId!=next)throw new InvalidOperationException("Actual global source selection prefix changed.");
        var nodes=State.Publication.Scope.SharedState.Words.CopyInsertionOrder();
        if(nodes.Count!=1||!ReferenceEquals(nodes[0],State.Publication.WordSource.Node))throw new InvalidOperationException("Actual WORD insertion list changed.");
    }
    public void ValidateCurrent(CancellationToken token=default)
    {lock(gate){token.ThrowIfCancellationRequested();if(!ready||faulted)throw new InvalidOperationException("Both actual global source-index passes must complete.");ValidateState(token);}}
    public IReadOnlyList<LegacyGlobalDocumentIndexedSource> CopySources(){lock(gate)return Array.AsReadOnly(sources.ToArray());}
    public LegacyGlobalDocumentSourceIndexSnapshot CopyCharacterIndex(){lock(gate)return new(this,chars,false);}
    public LegacyGlobalDocumentSourceIndexSnapshot CopyWordIndex(){lock(gate)return new(this,words,true);}
    public LegacyGlobalDocumentIndexPrefix[] CopyPrefixes(){lock(gate)return prefixes.ToArray();}
    private void Record(string stage)=>prefixes.Add(new(stage,chars.SourcesAdded,chars.Intervals.Nodes.Count,chars.Vectors.Nodes.Count,chars.Memberships.Count,chars.Dirty,
        words.SourcesAdded,words.Intervals.Nodes.Count,words.Vectors.Nodes.Count,words.Memberships.Count,words.Dirty));
}

//Bounded scalar/key preflight, never a shadow index or fabricated source owner.
internal sealed class LegacyGlobalDocumentIndexBudget
{
    private sealed class Vectors:IEqualityComparer<LegacyNativeInkPositionRange[]>
    {
        public bool Equals(LegacyNativeInkPositionRange[]? a,LegacyNativeInkPositionRange[]? b)=>a is not null&&b is not null&&a.AsSpan().SequenceEqual(b);
        public int GetHashCode(LegacyNativeInkPositionRange[] key){var hash=new HashCode();foreach(var r in key){hash.Add(r.First.Value);hash.Add(r.Last.Value);}return hash.ToHashCode();}
    }
    private readonly HashSet<LegacyNativeInkPositionRange> intervals=[];
    private readonly HashSet<LegacyNativeInkPositionRange[]> vectors=new(new Vectors());
    internal int Memberships{get;private set;}
    internal void Add(ReadOnlySpan<LegacyNativeInkPositionRange> key)
    {
        if(key.Length>65536||key.Length>1_048_576-Memberships)throw new NotSupportedException("Whole index raw-membership/range budget exceeded before claim.");
        foreach(var range in key)intervals.Add(range);vectors.Add(key.ToArray());
        if(intervals.Count>65536||vectors.Count>65536)throw new NotSupportedException("Whole interval/vector node budget exceeded before claim.");
        //Any valid AVL with<=65536 nodes has height<=22: minimum nodes for
        //height23 is75024. Therefore every insertion path is below native32.
        Memberships+=key.Length;
    }
}
