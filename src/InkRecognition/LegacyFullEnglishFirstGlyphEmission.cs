using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyFullEnglishFirstGlyphEmission.Probe")]
namespace InkRecognition;

/// <summary>Detached B6 WORD bytes/references reinterpreted only against the
/// same child's private B7 enriched pool. No serialized273 or accepted text.</summary>
public sealed class LegacyFullEnglishFirstGlyphEmissionWord
{
    private readonly byte[] storage;private readonly ulong[] references;
    public bool IsDelimiter{get;} public int WordIndex{get;} public LegacyNativeWordSourceSpan Span{get;}
    internal LegacyFullEnglishFirstGlyphEmissionWord(LegacyFullEnglishFirstPlainWordSnapshot original,CancellationToken token)
    {token.ThrowIfCancellationRequested();storage=original.CopyStorage();references=original.CopyReadingOrderReferences();IsDelimiter=original.IsDelimiter;WordIndex=original.WordIndex;Span=original.Span;}
    public byte[] CopyStorage()=>storage.ToArray();public ulong[] CopyReadingOrderReferences()=>references.ToArray();
}

/// <summary>Immutable historical child data. Only a complete nonfaulted parent
/// can authorize a later stage; no source720/final-guide producer exists here.</summary>
public sealed class LegacyFullEnglishFirstGlyphEmissionChild
{
    public LegacyFullEnglishFirstGlyphEmission Owner{get;}
    public LegacyFullEnglishFirstPlainChild Original{get;}
    public IReadOnlyList<LegacyFullEnglishFirstGlyphEmissionWord> Words{get;}
    public int AppendAttempts{get;} public int FreshRows{get;} public int RangePasses{get;}
    public IReadOnlyDictionary<ulong,float> OwnerFirstScores1C=>Original.OwnerFirstScores1C;
    private readonly LegacyNativeGlyphRecordNode[] owners;
    private readonly Dictionary<ulong,LegacyFirstResultSourceRange[]> ranges;
    internal LegacyFullEnglishFirstGlyphEmissionChild(LegacyFullEnglishFirstGlyphEmission owner,
        LegacyFullEnglishFirstGlyphEmissionChildPlan plan,LegacyFullEnglishFirstGlyphEmissionPool pool,CancellationToken token)
    {Owner=owner;Original=plan.Original;Words=Array.AsReadOnly(plan.Words);AppendAttempts=pool.AttemptCount;FreshRows=pool.FreshCount;RangePasses=pool.RangePassCount;owners=pool.CopyOwners(token);ranges=pool.CopyRanges(token);}
    public IReadOnlyList<LegacyNativeGlyphRecordNode> CopyGlyphOwners(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();var copied=new LegacyNativeGlyphRecordNode[owners.Length];
        for(int i=0;i<copied.Length;i++){token.ThrowIfCancellationRequested();var row=owners[i];copied[i]=new(row.Key,row.PackedCount,row.Records.ToArray());}return Array.AsReadOnly(copied);
    }
    public ulong[] CopySourceRangeKeys()=>ranges.Keys.ToArray();
    public LegacyFirstResultSourceRange[] CopySourceRanges(ulong canonicalKey,CancellationToken token=default)
    {token.ThrowIfCancellationRequested();return ranges.TryGetValue(canonicalKey,out var value)?value.ToArray():throw new ArgumentException("Actual enriched ordinary owner required.");}
}

/// <summary>Fresh actual full-FIRST23F.V90 A8 enrichment of an independent B6
/// rich-pool fork. Default new rows are library-zeroed. Reached182/183/184 are
/// explicitly refused before entry; their prototype byte remains unproved.
/// No SDK execution, source720, accepted text, final guide or default change.</summary>
public sealed class LegacyFullEnglishFirstGlyphEmission
{
    public LegacyFullEnglishFirstPlainText Owner{get;}
    private readonly object gate=new();private readonly LegacyFullEnglishFirstGlyphEmissionPolicy policy;
    private readonly LegacyFullEnglishFirstGlyphEmissionBudget budget;
    private readonly LegacyFullEnglishFirstGlyphEmissionChildPlan[] children;
    private readonly List<LegacyFullEnglishFirstGlyphEmissionChild> prefix=[];
    private bool entered,running,complete,faulted;
    public bool IsComplete{get{lock(gate)return complete;}}public bool IsFaulted{get{lock(gate)return faulted;}}
    public long AdditionalLogicalPeakBytes=>budget.Peak;public long LogicalCopyWorkBytes=>budget.Work;public long LogicalVisitWork=>budget.Visits;
    public int ReservedAppendAttempts=>budget.AttemptedAppends;
    private LegacyFullEnglishFirstGlyphEmission(LegacyFullEnglishFirstPlainText owner,long peak,long work,long visits,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(owner);Owner=owner;token.ThrowIfCancellationRequested();RequireComplete();
        var originals=owner.CopyCompletedChildren().ToArray();if(originals.Length is <1 or >256)throw new ArgumentException("Complete full B6 child set required.");
        foreach(var child in originals){token.ThrowIfCancellationRequested();if(!ReferenceEquals(child.Owner,owner))throw new ArgumentException("Same canonical B6 child required.");}
        policy=new(owner,originals[0],token);
        //Bound aggregate work BEFORE full original-row/ref copying. Small child
        //references/category payloads and bounded counting sets precede this.
        budget=new(owner,originals,peak,work,visits,token);
        //ALL reached A8 symbols across ALL children are validated before even
        //the first original rich-row snapshot; no descriptor hit bypasses this.
        foreach(var child in originals)policy.PreflightSymbols(child,token);
        children=new LegacyFullEnglishFirstGlyphEmissionChildPlan[originals.Length];int h=0,e=0,u=0;
        for(int i=0;i<children.Length;i++)
        {
            token.ThrowIfCancellationRequested();var plan=new LegacyFullEnglishFirstGlyphEmissionChildPlan(policy,originals[i],token);
            h=checked(h+plan.OriginalRows);e=checked(e+plan.Appends);u=checked(u+plan.NormalOwners);
            if(h>budget.OriginalRows||e>budget.AttemptedAppends||u>budget.Owners||h+e>65536)
                throw new InvalidDataException("B7 snapshots/attempts exceed whole-parent reservation.");children[i]=plan;
        }
    }
    private void RequireComplete()
    {
        var terminal=Owner.Owner;var context=terminal.Owner;var primary=context.Owner;var initializer=primary.Owner;var confidence=initializer.Owner;
        if(!Owner.IsComplete||Owner.IsFaulted||!terminal.IsComplete||terminal.IsFaulted||!context.IsComplete||context.IsFaulted||!primary.IsComplete||primary.IsFaulted||
           !initializer.IsComplete||initializer.IsFaulted||!confidence.IsComplete||confidence.IsFaulted||!confidence.Parent.IsComplete||confidence.Parent.IsFaulted)
            throw new ArgumentException("Complete nonfaulted canonical full B6..A required; historical prefixes are not authority.");
    }
    public static LegacyFullEnglishFirstGlyphEmission Prepare(LegacyFullEnglishFirstPlainText owner,CancellationToken token=default)
        =>new(owner,LegacyFullEnglishFirstGlyphEmissionBudget.PeakLimit,LegacyFullEnglishFirstGlyphEmissionBudget.WorkLimit,LegacyFullEnglishFirstGlyphEmissionBudget.VisitLimit,token);
    internal static LegacyFullEnglishFirstGlyphEmission PrepareBounded(LegacyFullEnglishFirstPlainText owner,long peak,long work,long visits,CancellationToken token=default)=>new(owner,peak,work,visits,token);
    public void Run(CancellationToken token=default)=>RunCore(null,null,null,null,new byte[40],token);
    internal void RunPrepared(Action<string>? checkpoint,Action<LegacyFullEnglishFirstGlyphEmissionAttempt>? attemptObserver,
        Action<LegacyFullEnglishFirstGlyphEmissionQuery>? queryObserver,ReadOnlyMemory<byte> allocation,CancellationToken token=default)
        =>RunCore(checkpoint,attemptObserver,queryObserver,null,allocation,token);
    internal void RunObserved(Action<string>? checkpoint,Action<LegacyFullEnglishFirstGlyphEmissionAttempt>? attemptObserver,
        Action<LegacyFullEnglishFirstGlyphEmissionQuery>? queryObserver,Action<LegacyFullEnglishFirstGlyphEmissionRange>? rangeObserver,
        ReadOnlyMemory<byte> allocation,CancellationToken token=default)=>RunCore(checkpoint,attemptObserver,queryObserver,rangeObserver,allocation,token);
    private void RunCore(Action<string>? checkpoint,Action<LegacyFullEnglishFirstGlyphEmissionAttempt>? attemptObserver,
        Action<LegacyFullEnglishFirstGlyphEmissionQuery>? queryObserver,Action<LegacyFullEnglishFirstGlyphEmissionRange>? rangeObserver,ReadOnlyMemory<byte> allocation,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();RequireComplete();if(allocation.Length!=40)throw new ArgumentException("Explicit row storage40 required.");
        allocation=allocation.ToArray();lock(gate){token.ThrowIfCancellationRequested();if(entered||running)throw new InvalidOperationException("B7 owner is one-use.");entered=running=true;}
        try
        {
            checkpoint?.Invoke("entry");token.ThrowIfCancellationRequested();int attempts=0,rangepasses=0;
            foreach(var child in children)
            {
                checkpoint?.Invoke("child:entry");token.ThrowIfCancellationRequested();policy.Validate(child.Original,token);
                var pool=new LegacyFullEnglishFirstGlyphEmissionPool(child,token);var cache=policy.Bind(child.Original,token);
                checkpoint?.Invoke("child:cloned");token.ThrowIfCancellationRequested();
                foreach(var entry in child.Entries){token.ThrowIfCancellationRequested();pool.Apply(entry,cache,allocation.Span,checkpoint,attemptObserver,queryObserver,rangeObserver,token);}
                attempts=checked(attempts+pool.AttemptCount);rangepasses=checked(rangepasses+pool.RangePassCount);
                if(attempts>budget.AttemptedAppends||rangepasses>2*budget.Owners)throw new InvalidDataException("B7 actual work exceeded whole-parent reservation.");
                var value=new LegacyFullEnglishFirstGlyphEmissionChild(this,child,pool,token);checkpoint?.Invoke("child:materialized");token.ThrowIfCancellationRequested();
                lock(gate){token.ThrowIfCancellationRequested();prefix.Add(value);}checkpoint?.Invoke("child:published");token.ThrowIfCancellationRequested();
            }
            lock(gate){token.ThrowIfCancellationRequested();complete=true;}
        }
        catch{lock(gate)faulted=true;throw;}finally{lock(gate)running=false;}
    }
    /// <summary>Completed defensive child prefix. A faulted/incomplete owner
    /// retains historical data only and cannot authorize downstream work.</summary>
    public IReadOnlyList<LegacyFullEnglishFirstGlyphEmissionChild> CopyCompletedChildren(){lock(gate)return Array.AsReadOnly(prefix.ToArray());}
}
