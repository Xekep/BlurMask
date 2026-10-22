using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
[assembly:InternalsVisibleTo("LegacyFullEnglishFirstController.Probe")]
namespace InkRecognition;

/// <summary>Actual full-FIRST ranked costs and1941B0 initializer, after complete
/// B1. No primary/context/terminal/WORD/source720/frame or app-default change.</summary>
public sealed class LegacyFullEnglishFirstControllerCosts
{
    public LegacyFullEnglishFirstController Owner {get;}
    private readonly object gate=new();
    private readonly LegacyFullEnglishFirstAcousticCache[] children;
    private readonly LegacyFullEnglishFirstPolicy policy;
    private readonly LegacyFullEnglishFirstControllerBudget budget;
    private readonly float[] table;
    private readonly List<LegacyFullEnglishFirstControllerChild> prefix=[];
    private bool entered,running,complete,faulted;
    public bool IsComplete {get{lock(gate)return complete;}}
    public bool IsFaulted {get{lock(gate)return faulted;}}
    public long AdditionalLogicalEnvelopeBytes=>budget.Bytes;
    /// <summary>Conservative sum of B1's partition reservations plus additional
    /// B2 reservation. Logical work/copy accounting, not managed or process RSS.</summary>
    public long CombinedLogicalReservationBytes=>64L*1024*1024+budget.Bytes;
    public int GroupCount=>budget.Groups;
    private LegacyFullEnglishFirstControllerCosts(LegacyFullEnglishFirstController owner,LegacyCursiveSeparationTable nativeTable,long maximumBytes,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(owner);ArgumentNullException.ThrowIfNull(nativeTable);token.ThrowIfCancellationRequested();Owner=owner;
        if(!owner.IsComplete||owner.IsFaulted||!owner.Parent.IsComplete||owner.Parent.IsFaulted)throw new ArgumentException("Complete same actual B1 parent authority required.");
        children=owner.Parent.CopyCompletedChildren().ToArray();if(children.Length is <1 or >256)throw new ArgumentException("Actual completed children required.");
        //Only bounded small references/counts/category payloads before whole preflight.
        policy=new(owner,children[0],token);budget=new(children,token);
        if(maximumBytes<0||maximumBytes>LegacyFullEnglishFirstControllerBudget.Limit||budget.Bytes>maximumBytes)throw new ArgumentException("B2 logical admission refused before entry.");
        table=nativeTable.Values.ToArray();if(!SHA256.HashData(MemoryMarshal.AsBytes(table.AsSpan())).AsSpan().SequenceEqual(Convert.FromHexString("B23ED52A3A10808EAB597A8F371A7BBB66BDE2FAB5FE2666431B1B192ABC1BAF")))throw new ArgumentException("Pinned native separation table required.");
        foreach(var c in children)
        {
            token.ThrowIfCancellationRequested();if(!ReferenceEquals(c.Owner.Child.Owner,owner.Parent)||!ReferenceEquals(c.Model,owner.Parent.Model)||!ReferenceEquals(c.Owner.Sampler.Source,policy.Model.Source))throw new ArgumentException("Same canonical full parent child/source required.");
            if(!float.IsFinite(c.Owner.Preparation.Line.Height)||c.Owner.Preparation.Line.Height<=0)throw new NotSupportedException("Actual positive same-cut guide required.");
        }
    }
    public static LegacyFullEnglishFirstControllerCosts Prepare(LegacyFullEnglishFirstController owner,LegacyCursiveSeparationTable table,CancellationToken token=default)=>new(owner,table,LegacyFullEnglishFirstControllerBudget.Limit,token);
    internal static LegacyFullEnglishFirstControllerCosts PrepareBounded(LegacyFullEnglishFirstController owner,LegacyCursiveSeparationTable table,long bytes,CancellationToken token=default)=>new(owner,table,bytes,token);
    public void Run(CancellationToken token=default)=>RunCore(null,token);
    internal void RunPrepared(Action<string> checkpoint,CancellationToken token=default)=>RunCore(checkpoint,token);
    private void RunCore(Action<string>? checkpoint,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate){if(entered||running)throw new InvalidOperationException("B2 owner is one-use.");entered=running=true;}
        try
        {
            checkpoint?.Invoke("entry");token.ThrowIfCancellationRequested();
            foreach(var cache in children)
            {
                checkpoint?.Invoke("child:entry");token.ThrowIfCancellationRequested();var c=cache.Owner;var rows=new Dictionary<int,LegacyFullEnglishFirstRankedGroup>();
                var ink=LegacyCursivePreparedInk.Create(c.CopyCutStrokes(),token);
                foreach(int id in c.CopyLiveGroupIds())
                {
                    token.ThrowIfCancellationRequested();var actual=cache.ScoreGroup(id);var confidence=Owner.ReadGroup(actual);
                    int geometry=LegacyCursiveGroupSeparationCost.Evaluate(ink,c.CopyGroupRanges(id),c.Preparation.Line.Height,policy.F("68"),policy.F("108"),table,policy.I("118"),cancellationToken:token);
                    var ranked=new LegacyOrdinaryRankedAcousticGroup(new(actual.CopyFinalCosts(),actual.CopyCandidates(),actual.FallbackCost),policy.Model.Source.Alphabet,policy.Header,Enumerable.Repeat(geometry,policy.GeometryCount).ToArray(),policy.CostScale,token);
                    rows.Add(id,new(this,actual,ranked,geometry,confidence.CopyValues()));checkpoint?.Invoke("group");token.ThrowIfCancellationRequested();
                }
                var state=new LegacyFullEnglishFirstTransitionGeometry(this,policy,c,table,token);checkpoint?.Invoke("geometry");token.ThrowIfCancellationRequested();
                var child=new LegacyFullEnglishFirstControllerChild(this,cache,state,rows);lock(gate){token.ThrowIfCancellationRequested();prefix.Add(child);}
                checkpoint?.Invoke("child:published");token.ThrowIfCancellationRequested();
            }
            token.ThrowIfCancellationRequested();lock(gate)complete=true;
        }
        catch{lock(gate)faulted=true;throw;}
        finally{lock(gate)running=false;}
    }
    /// <summary>Immutable completed child prefix remains readable after fault.
    /// Historical data supplies no primary/context/request authority.</summary>
    public IReadOnlyList<LegacyFullEnglishFirstControllerChild> CopyCompletedChildren(){lock(gate)return Array.AsReadOnly(prefix.ToArray());}
}

public sealed class LegacyFullEnglishFirstControllerChild
{
    public LegacyFullEnglishFirstControllerCosts Owner {get;}
    public LegacyFullEnglishFirstAcousticCache ActualCache {get;}
    public LegacyFullEnglishFirstTransitionGeometry Geometry {get;}
    private readonly Dictionary<int,LegacyFullEnglishFirstRankedGroup> groups;
    internal LegacyFullEnglishFirstControllerChild(LegacyFullEnglishFirstControllerCosts owner,LegacyFullEnglishFirstAcousticCache cache,LegacyFullEnglishFirstTransitionGeometry geometry,Dictionary<int,LegacyFullEnglishFirstRankedGroup> groups)
    {Owner=owner;ActualCache=cache;Geometry=geometry;this.groups=groups;}
    public int[] CopyGroupIds()=>groups.Keys.ToArray();
    public LegacyFullEnglishFirstRankedGroup ReadGroup(LegacyFullEnglishFirstCachedGroup actual)
    {
        ArgumentNullException.ThrowIfNull(actual);
        if(!ReferenceEquals(actual.Owner,ActualCache)||!groups.TryGetValue(actual.GroupId,out var group)||!ReferenceEquals(group.ActualCache,actual))throw new ArgumentException("Same canonical completed actual group required.");return group;
    }
}

/// <summary>Detached ranked candidates and distinct calibrated B1 confidence.
/// Entries are initializer data, not requests issued by a primary scheduler.</summary>
public sealed class LegacyFullEnglishFirstRankedGroup
{
    public LegacyFullEnglishFirstControllerCosts Owner {get;}
    public LegacyFullEnglishFirstCachedGroup ActualCache {get;}
    public int GeometryCost {get;}
    private readonly LegacyOrdinaryRankedAcousticGroup ranked;
    private readonly float[] confidence;
    internal LegacyFullEnglishFirstRankedGroup(LegacyFullEnglishFirstControllerCosts owner,LegacyFullEnglishFirstCachedGroup cache,LegacyOrdinaryRankedAcousticGroup ranked,int geometry,float[] confidence)
    {Owner=owner;ActualCache=cache;this.ranked=ranked;GeometryCost=geometry;this.confidence=confidence;}
    public LegacyRankedAcousticEntry[] CopyEntries()=>ranked.Entries.ToArray();
    public float[] CopyConfidence()=>confidence.ToArray();
    //PREPARED scalar controls only; no public caller row/request promotion.
    internal int PreparedClassQuery(ushort symbol,ushort ordinal,int geometry,bool top,bool primitive,CancellationToken token=default)=>ranked.Query(symbol,ordinal,geometry,top,primitive,token);
    internal byte PreparedLikelihood(ushort symbol,CancellationToken token=default)=>ranked.GetFirstControllerLikelihoodByte(symbol,token);
}
