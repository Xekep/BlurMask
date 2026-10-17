namespace InkRecognition;

/// <summary>Actual per-flush217DB0 chronology on the global source. A live
/// association must be acknowledged by an internal same-flush executor before
/// another segment is entered. This stage does not assert publication success.</summary>
public sealed class LegacyFreshMainGlobalGlyphAssociationSession
{
    private readonly object gate=new();
    private ulong[] residual=[];
    private int remaining,next;
    private bool stopped;
    private LegacyFreshMainGlobalAssociatedGlyphFlush? current;
    public LegacyFreshMainGlobalGlyphFlushPlan Plan{get;}
    public LegacyFreshMainGlobalResidualSource SourceOverlay{get;}
    public LegacyFreshMainGlobalGlyphSourceMetadata Metadata{get;}
    public bool IsStopped{get{lock(gate)return stopped;}}
    public int CompletedFlushCount{get{lock(gate)return next;}}
    public bool IsComplete{get{lock(gate)return !stopped&&current is null&&next==Plan.Flushes.Count;}}
    private LegacyFreshMainGlobalGlyphAssociationSession(LegacyFreshMainGlobalGlyphFlushPlan plan,
        LegacyFreshMainGlobalResidualSource overlay,LegacyFreshMainGlobalGlyphSourceMetadata metadata)
    {Plan=plan;SourceOverlay=overlay;Metadata=metadata;}
    public static LegacyFreshMainGlobalGlyphAssociationSession Prepare(LegacyFreshMainGlobalGlyphFlushPlan plan,
        LegacyFreshMainGlobalSourceGeometry geometry,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(plan);ArgumentNullException.ThrowIfNull(geometry);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(plan.BacktrackRows.SourceCallbacks.Geometry,geometry))
            throw new ArgumentException("Same actual flush/source geometry owner required.");
        plan.BacktrackRows.CompactOwner.Pass.ValidateResultQueryLease(token);
        var overlay=LegacyFreshMainGlobalResidualSource.Prepare(geometry,token);
        return new(plan,overlay,LegacyFreshMainGlobalGlyphSourceMetadata.Prepare(overlay,token));
    }
    public LegacyFreshMainGlobalAssociatedGlyphFlush PrepareNext(CancellationToken token=default)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(stopped||current is not null||next>=Plan.Flushes.Count)
                throw new InvalidOperationException("Stopped/completed/awaiting-callback association session cannot advance.");
            var pass=Plan.BacktrackRows.CompactOwner.Pass;pass.ValidateResultQueryLease(token);
            var flush=Plan.Flushes[next];var additions=flush.CopyResidualAdditions();
            if(remaining<0||remaining>residual.Length||additions.Length>65536-remaining)
                throw new NotSupportedException("Actual residual carry prefix exceeds its portable source-vector budget.");
            ulong[] input=new ulong[remaining+additions.Length];residual.AsSpan(0,remaining).CopyTo(input);additions.CopyTo(input,remaining);
            stopped=true;var result=SourceOverlay.Associate(flush.CopyRows72(),input,token);
            residual=result.CopyResidualStorage();remaining=result.RemainingCount;
            current=new(this,flush,next,result);pass.ValidateResultQueryLease(token);stopped=false;return current;
        }
    }
    //Only a same-assembly owning callback may acknowledge success; no public
    //caller-supplied bool resets chronology or fabricates completed publication.
    internal void CompleteCurrent(LegacyFreshMainGlobalAssociatedGlyphFlush association,bool callbackSucceeded,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(association);token.ThrowIfCancellationRequested();
        lock(gate)
        {
            if(stopped||!ReferenceEquals(current,association))throw new InvalidOperationException("Current live same-session association required.");
            Plan.BacktrackRows.CompactOwner.Pass.ValidateResultQueryLease(token);
            if(callbackSucceeded&&!association.Succeeded)throw new InvalidOperationException("Failed217DB0 prefix cannot be acknowledged as published.");
            if(!callbackSucceeded){stopped=true;return;}next++;current=null;
        }
    }
    internal void ValidateCurrent(LegacyFreshMainGlobalAssociatedGlyphFlush association,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(association);token.ThrowIfCancellationRequested();
        lock(gate)
        {
            if(stopped||!ReferenceEquals(current,association))throw new InvalidOperationException("Current live global association required.");
            Plan.BacktrackRows.CompactOwner.Pass.ValidateResultQueryLease(token);
        }
    }
    internal void CompleteExecuted(LegacyFreshMainGlobalGlyphExecutionResult result,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(result);
        if(!ReferenceEquals(result.ExecutionSession.Associations,this))throw new ArgumentException("Actual executor result belongs to another association session.");
        CompleteCurrent(result.Association,result.Succeeded,token);
    }
    internal void AbortExecution(LegacyFreshMainGlobalAssociatedGlyphFlush? association)
    {
        lock(gate)
        {
            if(association is not null&&!ReferenceEquals(current,association))throw new ArgumentException("Cannot abort a foreign global flush.");
            stopped=true;
        }
    }
}

public sealed class LegacyFreshMainGlobalAssociatedGlyphFlush
{
    private readonly LegacyNativeResidualAssociationResult result;
    public LegacyFreshMainGlobalGlyphAssociationSession Session{get;}
    public LegacyFreshMainGlobalGlyphFlush Flush{get;}
    public int Ordinal{get;}
    public bool Succeeded=>result.Succeeded;
    public int RemainingCount=>result.RemainingCount;
    internal LegacyFreshMainGlobalAssociatedGlyphFlush(LegacyFreshMainGlobalGlyphAssociationSession session,
        LegacyFreshMainGlobalGlyphFlush flush,int ordinal,LegacyNativeResidualAssociationResult result)
    {Session=session;Flush=flush;Ordinal=ordinal;this.result=result;}
    public byte[] CopyRows72()=>result.CopyRows();
    public ulong[] CopyResidualStorage()=>result.CopyResidualStorage();
}
