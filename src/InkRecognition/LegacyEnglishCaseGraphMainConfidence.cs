namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private LegacyEnglishFreshCaseMainScorer? graphConfidenceScorer;
    internal sealed record GraphMainConfidenceObservation(int Group,int Span,int SourceProbabilityBits,
        LegacyAcousticCacheCandidate[] Candidates,int[][] Probabilities,int[] ConfidenceBits);
    private Action<GraphMainConfidenceObservation>? graphConfidenceObserver;
    internal void ObserveGraphMainConfidence(LegacyEnglishFreshCaseMainScorer scorer,int group,int span,float sourceProbability,
        LegacyAcousticCacheCandidate[] detachedCandidates,IReadOnlyList<ReadOnlyMemory<int>> original,float[] privateValues,CancellationToken token)
    {
        RequireGraphMainConfidence(scorer,token);var epoch=scorer.CaseSource.RequireGraphConfidenceD0(scorer,null,token);var observer=graphConfidenceObserver;
        if(observer is null)return;
        // Value-only diagnostic: NN buffers/cache values never escape. Probe
        // retention is bounded independently and is not production cache state.
        var copy=new int[4][];for(int i=0;i<4;i++)copy[i]=original[i].ToArray();
        observer(new(group,span,BitConverter.SingleToInt32Bits(sourceProbability),detachedCandidates,
            copy,privateValues.Select(BitConverter.SingleToInt32Bits).ToArray()));
        scorer.CaseSource.RequireGraphConfidenceD0(scorer,epoch,token);
        RequireGraphMainConfidence(scorer,token);
    }
    internal void RequireGraphMainConfidence(LegacyEnglishFreshCaseMainScorer scorer,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(!graphLoopActive||!IsRunningFor(primary)||!ReferenceEquals(graphConfidenceScorer,scorer))
            throw new InvalidOperationException("Same running graph confidence cohort/thread required.");
    }
    internal void RequireGraphMainConfidencePreparation(LegacyEnglishCasePrimaryCompletion actual,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(!graphLoopActive||!IsRunningFor(actual)||graphConfidenceScorer is not null)
            throw new InvalidOperationException("One pre-primary graph confidence preparation required.");
    }
}
internal sealed partial class LegacyEnglishCasePrimaryCompletion
{
    internal LegacyEnglishFreshCaseMainScorer AttachGraphMainConfidence(LegacyEnglishCaseNode0Completion owner,
        LegacyEnglishCaseNode0Ledger ledger,CancellationToken token)
    {
        owner.RequireGraphMainConfidencePreparation(this,token);
        if(!ReferenceEquals(node0,owner)||Volatile.Read(ref phase)!=0)
            throw new InvalidOperationException("Exact unused primary owner required before networks.");
        return consumer.AttachGraphMainConfidence(this,owner,ledger,token);
    }
}
internal sealed partial class LegacyEnglishCaseInitialConsumer
{
    internal LegacyEnglishFreshCaseMainScorer AttachGraphMainConfidence(LegacyEnglishCasePrimaryCompletion actual,
        LegacyEnglishCaseNode0Completion owner,LegacyEnglishCaseNode0Ledger ledger,CancellationToken token)
    {
        owner.RequireGraphMainConfidencePreparation(actual,token);
        if(Volatile.Read(ref run)!=0||!ReferenceEquals(completion,actual)||!actual.IsPreparedFor(this)||issuedLease is not null||pass.IsFaulted)
            throw new InvalidOperationException("Exact unused consumer/scorer required before original D0.");
        pass.Scorer.AttachGraphMainConfidence(owner,ledger,token);return pass.Scorer;
    }
}
public sealed partial class LegacyEnglishFreshCaseMainScorer
{
    // Null on all previous entries. The additional default-null reference is
    // eight logical bytes on64-bit; the OLD path adds one null branch/Compute.
    private MainConfidenceCapture? mainConfidenceCapture;
    private sealed class MainConfidenceCapture
    {
        private readonly LegacyEnglishFreshCaseMainScorer scorer;
        private readonly LegacyEnglishCaseNode0Completion owner;
        private readonly LegacyEnglishCaseMainConfidenceProfile profile;
        private readonly Group?[] groups;
        internal sealed record Group(LegacyEnglishMainArcScores Scores,float[] Values);
        internal MainConfidenceCapture(LegacyEnglishFreshCaseMainScorer scorer,LegacyEnglishCaseNode0Completion owner,
            LegacyEnglishCaseMainConfidenceProfile profile,int count)
        {this.scorer=scorer;this.owner=owner;this.profile=profile;groups=new Group?[count];}
        internal void Observe(LegacyEnglishMainGlyphFeatures features,LegacyEnglishMainNetworkResult[] networks,
            LegacyEnglishMainArcScores scores,CancellationToken token)
        {
            owner.RequireGraphMainConfidence(scorer,token);
            var epoch=scorer.CaseSource.RequireGraphConfidenceD0(scorer,null,token);
            if(!ReferenceEquals(profile.Source,scorer.Model.Source)||features.Group!=scores.GroupId||
                (uint)scores.GroupId>=(uint)groups.Length||groups[scores.GroupId] is not null||networks.Length!=4||features.Stored.Length==0)
                throw new InvalidDataException("One original same-source computed group required.");
            int physical=features.Stored[0].StrokeIndex;
            if((uint)physical>=(uint)scorer.ink.Probabilities.Length)throw new InvalidDataException("Original first stored source probability required.");
            ReadOnlyMemory<int>[] outputs=new ReadOnlyMemory<int>[4];
            for(int i=0;i<4;i++){if(networks[i].Slot!=i)throw new InvalidDataException("Original family order required.");outputs[i]=networks[i].Probabilities;}
            var candidates=scores.CopyCandidates();
            var values=LegacyEnglishCaseMainConfidenceKernel.Evaluate(profile,candidates,outputs,scorer.ink.Probabilities[physical],token);
            owner.ObserveGraphMainConfidence(scorer,scores.GroupId,scores.Span,scorer.ink.Probabilities[physical],candidates,outputs,values,token);
            scorer.CaseSource.RequireGraphConfidenceD0(scorer,epoch,token);
            owner.RequireGraphMainConfidence(scorer,token);groups[scores.GroupId]=new(scores,values);
        }
        internal (LegacyEnglishMainArcScores Scores,float[] Values) Copy(LegacyEnglishCaseNode0Completion actual,int group,CancellationToken token)
        {
            owner.RequireGraphMainConfidence(scorer,token);
            if(!ReferenceEquals(owner,actual)||(uint)group>=(uint)groups.Length||groups[group] is not{} value||
                !ReferenceEquals(value.Scores,scorer.cache[group]))throw new InvalidOperationException("Same original computed cache/group required.");
            return(value.Scores,value.Values.ToArray());
        }
    }
    internal void AttachGraphMainConfidence(LegacyEnglishCaseNode0Completion owner,LegacyEnglishCaseNode0Ledger ledger,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(mainConfidenceCapture is not null||cache.Length is<1 or>4096||cache.Any(x=>x is not null)||Model.FreshMode13Flags!=0x21400)
            throw new InvalidOperationException("Fresh same-owner MAIN cache before its first original Compute required.");
        // Fixed conservative reservation includes profile361525B, map/group
        // temporaries, per-group candidate copies<=150*8, four borrowed-output
        // references, retained floats<=150*4 and metadata. No NN arrays retained.
        long reserve=checked(LegacyEnglishCaseMainConfidenceProfile.PreparationEnvelope+8192L*cache.Length);
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,reserve);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,reserve);
        var profile=LegacyEnglishCaseMainConfidenceProfile.Read(Model.Source,token);
        token.ThrowIfCancellationRequested();mainConfidenceCapture=new(this,owner,profile,cache.Length);
    }
    internal (LegacyEnglishMainArcScores Scores,float[] Values) CopyGraphMainConfidence(
        LegacyEnglishCaseNode0Completion owner,int group,CancellationToken token)
        =>(mainConfidenceCapture??throw new InvalidOperationException("Original compute confidence capture absent.")).Copy(owner,group,token);
}

public sealed partial class LegacyEnglishFreshCaseMainSource
{
    // O(1) actual busy epoch authentication. No copy or callback under gate.
    // Returned opaque lease remains internal to the producer, not observation.
    internal D0Lease RequireGraphConfidenceD0(LegacyEnglishFreshCaseMainScorer actual,D0Lease? expected,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();var lease=activeD0;
            if(faulted||!busy||activeScore is not null||!ReferenceEquals(scorer,actual)||lease is null||
                !ReferenceEquals(lease.Root,this)||!ReferenceEquals(spanOwner,lease.Owner)||!ReferenceEquals(lease.Owner.Scorer,actual)||
                lease.Version!=version||expected is not null&&!ReferenceEquals(expected,lease))
                throw new InvalidOperationException("Exact original active same-scorer D0 epoch required around confidence capture.");
            return lease;
        }
    }
}
