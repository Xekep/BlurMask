using System.Security.Cryptography;
namespace InkRecognition;

/// <summary>Experimental oneTOP/multiple-WORD V68 coordinator. One genuine original
/// Scope, one source claim, all unique275 augmentation before any carried-quota rank.
/// No app wiring, fabricated node/snapshot ingress or terminal-tail fallback.</summary>
public sealed partial class LegacyFreshMainGlobalSourceBatchCompletion
{
    //Explicit NEW operational maximum, not a change to standalone64Mi work policy.
    //Default remains standalone64Mi; callers choosing a larger batch reserve do so explicitly.
    public const long MaximumStructuralWork=2L*LegacyGlobalWordAugmentationEnvelope.WorkLimit;
    private readonly LegacyLibraryGlobalTextPublicationScope scope;
    private readonly LegacyFreshMainGlobalPlainRecognitionResult source;
    private readonly LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot snapshot;
    private readonly LegacyNativePlainWordPublisher publisher;
    private readonly LegacyFreshMainGlobalGlyphExecutionSession session;
    private readonly LegacyNativePlainWordBatchBinding binding;
    private readonly byte[] compact;
    private readonly int[] starts;
    private readonly int compactAllocationBytes,snapshotDependencyBytes;
    private readonly SpanScope[] spans;
    private readonly long maximumWork,maximumCopy;
    private long facadeCopiedPayload;
    internal void ReserveSourceFacade(long bytes,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(entered!=0||facadeCopiedPayload!=0||bytes!=816)throw new InvalidOperationException("One fixed actual GLOBAL facade payload reserve required.");
        facadeCopiedPayload=bytes;
    }
    private int entered,thread,phase,current=-1;
    private bool faulted,complete;
    public bool QuoteCleanupCompleted {get;private set;}
    public bool SourcesMarkedComplete=>spans.All(s=>s.SourceMarked);
    public bool RankingApplied=>spans.All(s=>s.RankDone);
    public bool SpanScoresApplied=>RankingApplied;
    public bool CheckboxGuardVerified=>complete;
    public bool IsComplete=>Volatile.Read(ref complete);
    public bool IsFaulted=>faulted;
    public int PublishedWordCount=>binding.Nodes.Length;
    public int UniqueSpanCount=>spans.Length;
    public long PreflightStructuralWork{get;private set;}
    public long PreflightEnumeratedCopyBytes{get;private set;}
    private LegacyFreshMainGlobalSourceBatchCompletion(LegacyLibraryGlobalTextPublicationScope scope,
        LegacyFreshMainGlobalPlainRecognitionResult source,LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot snapshot,
        LegacyNativePlainWordBatchBinding binding,byte[] compact,int[] starts,int compactAllocationBytes,int snapshotDependencyBytes,ReadOnlySpan<byte> acoustic,long maximumWork,long maximumCopy)
    {
        this.scope=scope;this.source=source;this.snapshot=snapshot;this.binding=binding;this.compact=compact;this.starts=starts;
        this.snapshotDependencyBytes=snapshotDependencyBytes;this.compactAllocationBytes=compactAllocationBytes;this.maximumWork=maximumWork;this.maximumCopy=maximumCopy;session=source.Session!;publisher=session.Ownership.WordPublisher;
        spans=new SpanScope[binding.Owners.Length];
        for(int i=0;i<spans.Length;i++)
        {
            var stage=new SpanScope(this,i);spans[i]=stage;
            stage.Bind(LegacyFreshMainGlobalWordAugmentation.CreateBatchWorker(stage,acoustic,source,snapshot,compact,starts,binding.Owners[i]));
        }
    }
    public static LegacyFreshMainGlobalSourceBatchCompletion Prepare(LegacyLibraryGlobalTextPublicationScope originalScope,
        LegacyFreshMainGlobalPlainRecognitionResult actualSource,ReadOnlySpan<byte> acousticResource,
        ReadOnlySpan<byte> alternativeAllocation400,long maximumStructuralWork=64L*1024*1024,
        long maximumCopiedPayloadBytes=128L*1024*1024,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(originalScope);ArgumentNullException.ThrowIfNull(actualSource);token.ThrowIfCancellationRequested();
        originalScope.RequireActualAugmentationSource(actualSource,token);
        if(maximumStructuralWork is <1 or >MaximumStructuralWork||maximumCopiedPayloadBytes is <1 or >LegacyGlobalWordAugmentationEnvelope.CopyLimit||
            !actualSource.HasWinner||!actualSource.PublicationCompleted||actualSource.Session is not{} session||!session.IsComplete||session.IsStopped||
            !ReferenceEquals(session.Ownership.PublishedTerminal,actualSource.Selection)||alternativeAllocation400.Length!=400||acousticResource.Length!=572763)
            throw new ArgumentException("Original successful source and explicit bounded batch resources required.");
        using var preparation=session.AcquireSourcePreflight(token);
        if(checked((long)acousticResource.Length+32)>maximumCopiedPayloadBytes)
            throw new NotSupportedException("Owned acoustic copy/hash exceeds batch preclaim envelope.");
        //Own before hashing and before every worker's scalar parse. A caller's
        //mutable buffer can never provide a second, inconsistent resource read.
        byte[] ownedAcoustic=acousticResource.ToArray();
        var policy=actualSource.Selection.Publication.Policy;var cuts=policy.Owner.Graph.Source.Source;
        if(policy.Owner.Model.SourceKind!=0x135||policy.HasAssociatedSource||policy.Configuration.Trace2368!=0||policy.Configuration.Mode22f6!=0||
            policy.Refresh.KnowledgeOption22f8!=0||policy.Refresh.SourceFormat22f9!=0||cuts.Steps.Count!=1||cuts.Steps[0].IsPendingSeparator||
            cuts.Steps[0].Kind!=0||policy.Owner.Graph.Source.State.Contexts.Count!=0||
            !StringComparer.OrdinalIgnoreCase.Equals(Convert.ToHexString(SHA256.HashData(ownedAcoustic)),cuts.SourceFingerprint))
            throw new NotSupportedException("Same fresh ordinary no-associated135 source and original resource required.");
        foreach(string value in new[]{"\u2610","\u2611","\u2612"})
            if(policy.Owner.Model.AcousticAlphabet.TryGetLabel(value,out _))throw new NotSupportedException("Native checkbox rewrite requires its own owner.");
        var publisher=session.Ownership.WordPublisher;var binding=publisher.CaptureSourceBatch(token);
        if(actualSource.RawWordsInNativeHeadOrder.Count!=binding.Nodes.Length)
            throw new InvalidOperationException("Actual completed callback WORD count changed.");
        int snapshotDependencyBytes=actualSource.Selection.Publication.BatchSnapshotDependencyPayload;
        if(checked((long)snapshotDependencyBytes+1776+24+572763+4L*actualSource.Selection.Publication.BatchStartsLength)>maximumCopiedPayloadBytes)
            throw new NotSupportedException("Actual one-time snapshot copies exceed preclaim envelope.");
        var snapshot=LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(actualSource.Selection,session,actualSource.Publications,alternativeAllocation400,token);
        if(snapshot.ActiveAlternativeCount60!=1||snapshot.PublishedWordCount!=binding.Nodes.Length)
            throw new NotSupportedException("One genuine TOP and complete callback chain required.");
        var publication=actualSource.Selection.Publication;int count=publication.RecordCount2470;
        int compactAllocationBytes=publication.BatchCompactAllocationLength,startsLength=publication.BatchStartsLength;
        //Scalar actual-array lengths before either clone; caller DATA cannot supply them.
        if(count is <1 or >131072||publication.NodeCount is <1 or >4096||startsLength!=publication.NodeCount+1||
            compactAllocationBytes%40!=0||compactAllocationBytes<checked(count*40)||
            checked((long)compactAllocationBytes+40L*count+4L*startsLength+snapshotDependencyBytes+1776+24+572763)>maximumCopiedPayloadBytes)
            throw new NotSupportedException("Actual compact/start copies exceed batch preclaim extent.");
        //One private compact/starts copy for every span; no per-word model copy/hash.
        byte[] full=publication.CopyCompactStorage();int[] starts=publication.CopyNodeStarts2478();
        if(full.Length!=compactAllocationBytes||starts.Length!=startsLength)throw new InvalidOperationException("Actual source storage lengths changed.");
        byte[] compact=full.AsSpan(0,checked(count*40)).ToArray();
        publisher.RequireSourceBatch(binding,true,token);token.ThrowIfCancellationRequested();
        return new(originalScope,actualSource,snapshot,binding,compact,starts,compactAllocationBytes,snapshotDependencyBytes,ownedAcoustic,maximumStructuralWork,maximumCopiedPayloadBytes);
    }
    private void Validate(CancellationToken token,bool claimed)
    {
        token.ThrowIfCancellationRequested();scope.RequireActualAugmentationSource(source,token);
        if(faulted||!source.PublicationCompleted||!session.IsComplete||session.IsStopped||
            !ReferenceEquals(source.Session,session)||source.Selection.Publication.Policy.Owner.AcousticEvaluations!=source.CachedMainGroupEvaluations)
            throw new InvalidOperationException("Same healthy original batch source/cache required.");
        DebitValidationLoops(checked((long)binding.PublisherCount+binding.Nodes.Length+spans.Length),token);
        publisher.RequireSourceBatch(binding,false,token);
        if(claimed)
        {
            session.RequireSourceCompletion(this,token);
            if(!Volatile.Read(ref complete)&&thread!=Environment.CurrentManagedThreadId)throw new InvalidOperationException("Entered batch thread changed.");
        }
    }
    public Result Run(ReadOnlySpan<byte> wordAllocatorResidue120,ReadOnlySpan<byte> glyphAllocatorResidue40,
        ReadOnlySpan<ushort> labelStackResidue256,ReadOnlySpan<byte> requestResidue64,ReadOnlySpan<byte> walkResidue72,
        Span<byte> leadingScratch4,Span<byte> trailingScratch4,Span<byte> compareLeft4,Span<byte> compareRight4,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        using var preflight=session.AcquireSourcePreflight(token);
        if(Volatile.Read(ref entered)!=0)throw new InvalidOperationException("Entered batch completion cannot retry.");
        Validate(token,false);
        if(wordAllocatorResidue120.Length!=120||glyphAllocatorResidue40.Length!=40||labelStackResidue256.Length!=256||requestResidue64.Length!=64||walkResidue72.Length!=72||
            leadingScratch4.Length!=4||trailingScratch4.Length!=4||compareLeft4.Length!=4||compareRight4.Length!=4||
            leadingScratch4.Overlaps(trailingScratch4)||compareLeft4.Overlaps(compareRight4))
            throw new ArgumentException("Complete separate native allocator/caller residues required.");
        int points=source.Owner.SourceDocument.Strokes.Sum(stroke=>stroke.Points.Count);
        if(5L*(compact.Length/40)>maximumWork||checked(60L*(compact.Length/40)+4L*starts.Length+compactAllocationBytes)>maximumCopy)
            throw new NotSupportedException("Shared immutable batch preparation exceeds operational envelope.");
        var topology=LegacyGlobalWordAugmentationEnvelope.PrepareBatchTopology(compact,starts,source.Selection.Publication.Policy.Owner.Graph.State.NodeCount,points,compactAllocationBytes,token);
        var sourceRangeBound=source.Selection.Publication.Policy.Owner.Graph.State.BoundBatchForwardRanges(token);
        topology.MaximumSourceRanges=sourceRangeBound.MaximumPathRanges;
        topology.SourceGraphScanWork=sourceRangeBound.ScanWork;
        //Each source read caches each actual selector1 at most once; two
        //count/fill reads copy ushort payloads twice, not graph authority.
        topology.SourceSelectorCopyBytes=checked(4L*sourceRangeBound.SelectorEntries);
        long referenceUses=0;
        foreach(var node in binding.Nodes)
        {
            token.ThrowIfCancellationRequested();int count=node.Word.FlatReferences.Count;
            if(count is <1 or >4096||count>65536-referenceUses)throw new NotSupportedException("Whole original batch reference-use cap exceeded before claim.");
            referenceUses+=count;
        }
        var envelopes=new LegacyGlobalWordAugmentationEnvelope[spans.Length];
        long guardSlots=checked((long)binding.PublisherCount+binding.Nodes.Length+spans.Length);
        long work=checked(topology.SharedWork+5*referenceUses+2L*binding.Nodes.Length+5*guardSlots);
        //144R: preflight chain48R + quote chain48R/reverse8R/leading40R.
        //120W: tail40W and at most two actual ReplaceRecord40 writes/word.
        //Snapshot1776 = four400 buffers +40 score row +88beam +16AL header +32SHA.
        //Extra24 is the three-element checkbox-label reference array; strings/BCL metadata remain separate.
        long copy=checked(facadeCopiedPayload+topology.SharedCopy+144*referenceUses+120L*binding.Nodes.Length+
            8L*binding.Nodes.Length+48L*spans.Length+816+1776+24+572763+snapshotDependencyBytes+4L*starts.Length);
        for(int i=0;i<spans.Length;i++)
        {
            token.ThrowIfCancellationRequested();var node=binding.Owners[i];
            envelopes[i]=LegacyGlobalWordAugmentationEnvelope.PrepareBatchSpan(topology,node.Span.Last,node.Word.FlatReferences.Count,token);
            var bound=envelopes[i].Bounds;
            //Source entry, count/fill close, dry(q), candidate(entry/mutation/end:3q),
            //rank entry/end:4+4q; each Request triggers <=2 current-scope checks.
            long guardCalls=checked(4L+4L*bound.Candidates+2L*bound.Requests);
            int selections=0;for(int j=0;j<binding.Nodes.Length;j++)if(binding.Nodes[j].Span==node.Span)selections++;
            long snapshots=checked(4L+2L*bound.Candidates);
            work=checked(work+LegacyGlobalWordAugmentationEnvelope.SpanWork(bound,topology.MaximumSourceRanges)+guardCalls*guardSlots+
                binding.Nodes.Length+snapshots*(binding.Nodes.Length+binding.PublisherCount+2L*selections+bound.Candidates+1)+
                (bound.Candidates+4L)*selections);
            //Complete-head CaptureSpan retains every selected279 for this owner.
            //Explicit arrays: selected refs8K +ordinals4K +rows8A per capture;
            //rebind stores12K per ranking/normalization or new-duplicate update.
            copy=checked(copy+bound.CopyBytes+snapshots*(12L*selections+8L*(bound.Candidates+1))+
                12L*selections*(bound.Candidates+2));
            if(work>maximumWork||copy>maximumCopy)throw new NotSupportedException("Cumulative whole-batch structural/copy envelope exceeded before claim.");
        }
        PreflightStructuralWork=work;PreflightEnumeratedCopyBytes=copy;
        InitializeValidationMeter(work);
        //Flat metadata alone is not a linked-chain proof. Validate the SAME actual
        //pool chain before entry; no cloned row becomes authority. Successful reads
        //are bounded by referenceUses; malformed chain refuses before mutation.
        foreach(var node in binding.Nodes)
        {
            var chain=LegacyNativeWordBuilder.CopyChain(session.Ownership.GlyphPool,node.Word.HeadReference,token);
            if(chain.Length!=node.Word.FlatReferences.Count)throw new InvalidOperationException("Original actual batch chain length changed.");
            for(int i=0;i<chain.Length;i++)if(chain[chain.Length-1-i]!=node.Word.FlatReferences[i])throw new InvalidOperationException("Original actual batch linked/flat order changed.");
        }
        var working=LegacyNativeWordBuilder.CreateEmpty(session.Ownership.GlyphPool,wordAllocatorResidue120,token);
        byte[] records=glyphAllocatorResidue40.ToArray(),requests=requestResidue64.ToArray(),walk=walkResidue72.ToArray();ushort[] labels=labelStackResidue256.ToArray();
        var results=new LegacyFreshMainGlobalWordAugmentation.Result[spans.Length];
        DebitValidationLoops(checked((long)binding.PublisherCount+binding.Nodes.Length+spans.Length),token);
        publisher.RequireSourceBatch(binding,true,token);Validate(token,false);
        if(Interlocked.CompareExchange(ref entered,1,0)!=0)throw new InvalidOperationException("Entered batch completion cannot retry.");
        thread=Environment.CurrentManagedThreadId;session.ClaimSourceCompletion(this,true,token);preflight.Dispose();
        try
        {
            LegacyFreshMainGlobalPublishedQuoteNormalization.ApplyPublished(snapshot,leadingScratch4,trailingScratch4,token);
            QuoteCleanupCompleted=true;
            _=LegacyFreshMainGlobalResultGlyphNormalization.Prepare(source.Selection.Publication.Policy,token).ApplyPublished(snapshot,token);
            phase=1;
            for(current=0;current<spans.Length;current++)
            {
                // Native13E886 marks this shared275 AFTER the callback returns,
                // including failure. The preallocated sidecar cannot debit/fail.
                try{spans[current].Worker.RunBatchSource(working,records,labels,requests,walk,compareLeft4,compareRight4,envelopes[current],token);spans[current].SourceDone=true;}
                finally{spans[current].SourceMarked=true;}
            }
            //Native13E7F0 finishes every unique275 before13D360 starts.
            phase=2;int quota=LegacyPreparedMainWordRanking.InitialQuota(65535,65535,false);
            for(current=0;current<spans.Length;current++)
            {var result=spans[current].Worker.RankBatchSource(quota,compareLeft4,compareRight4,envelopes[current],token);quota=result.QuotaAfter;results[current]=result;spans[current].RankDone=true;}
            current=-1;Validate(token,true);session.CompleteSourceCompletion(this,token);phase=3;Volatile.Write(ref complete,true);ReleaseCompletedValidationMeter();
            return new(binding.Nodes.Length,results,quota,work,copy);
        }
        catch{faulted=true;phase=-1;current=-1;session.FaultSourceCompletion(this);throw;}
    }
    internal long RemainingEnumeratedCopyBudget(CancellationToken token){RequireCompleted(token);lock(validationMeterGate)return maximumCopy-PreflightEnumeratedCopyBytes-earlyTextPreparationCopyBytes-associationCopyReserve;}
    internal void RequireCompleted(CancellationToken token)
    {Validate(token,true);DebitValidationLoops(spans.Length,token);if(!Volatile.Read(ref complete)||phase!=3||spans.Any(s=>!s.SourceDone||!s.SourceMarked||!s.RankDone))throw new InvalidOperationException("Entire actual batch must complete before WORD/TEXT publication.");}
    internal sealed class SpanScope
    {
        private readonly LegacyFreshMainGlobalSourceBatchCompletion owner;
        private readonly int ordinal;
        private LegacyFreshMainGlobalWordAugmentation? worker;
        internal bool SourceDone,SourceMarked,RankDone;
        internal SpanScope(LegacyFreshMainGlobalSourceBatchCompletion owner,int ordinal){this.owner=owner;this.ordinal=ordinal;}
        internal LegacyFreshMainGlobalWordAugmentation Worker=>worker??throw new InvalidOperationException("Batch worker unbound.");
        internal void Bind(LegacyFreshMainGlobalWordAugmentation actual)
        {if(worker is not null)throw new InvalidOperationException("Batch worker already bound.");worker=actual;}
        internal void Require(LegacyFreshMainGlobalWordAugmentation actual,CancellationToken token)
        {
            owner.Validate(token,true);
            if(!ReferenceEquals(worker,actual)||owner.current!=ordinal||!ReferenceEquals(owner.spans[ordinal],this)||owner.phase is not(1 or 2))
                throw new InvalidOperationException("Exact private running batch/current-span worker required.");
        }
        internal void RequireSource(LegacyFreshMainGlobalWordAugmentation actual,CancellationToken token)
        {Require(actual,token);if(owner.phase!=1||SourceDone)throw new InvalidOperationException("Source span cannot repeat or run after ranking.");}
        internal void RequireTargetOwner(LegacyFreshMainGlobalWordAugmentation actual,
            LegacyNativePlainWordPublisher actualPublisher,LegacyNativePublishedWordNode actualTarget,CancellationToken token)
        {
            RequireSource(actual,token);
            if(!ReferenceEquals(actualPublisher,owner.publisher)||!ReferenceEquals(actualTarget,owner.binding.Owners[ordinal]))
                throw new InvalidOperationException("Exact preexisting current-span275 owner/publisher required for native219580 append.");
        }
        internal void ChargeAssociation(LegacyFreshMainGlobalWordAugmentation actual,long work,long copied,CancellationToken token)
        {RequireSource(actual,token);owner.ChargeAssociation(work,copied,token);}
        internal void RecordAssociation(LegacyFreshMainGlobalWordAugmentation actual,int residual,int rows,int merges,int newExtras,CancellationToken token)
        {RequireSource(actual,token);owner.RecordAssociation(residual,rows,merges,newExtras);}
        internal void RequireRanking(LegacyFreshMainGlobalWordAugmentation actual,CancellationToken token)
        {Require(actual,token);if(owner.phase!=2||RankDone||owner.spans.Any(s=>!s.SourceDone||!s.SourceMarked))throw new InvalidOperationException("Ranking requires all unique source spans complete.");}
        internal void RequireText(LegacyFreshMainGlobalWordAugmentation actual,CancellationToken token)
        {
            owner.RequireCompleted(token);
            if(!ReferenceEquals(worker,actual)||!ReferenceEquals(owner.spans[ordinal],this)||!SourceDone||!RankDone)
                throw new InvalidOperationException("Exact completed same-batch worker required for WORD range projection.");
        }
        internal LegacyNativePublishedWordNode[] Heads(LegacyFreshMainGlobalWordAugmentation actual)
        {if(!ReferenceEquals(worker,actual))throw new InvalidOperationException("Exact batch worker required.");return owner.binding.Heads;}
    }
    internal LegacyOrdinaryGraphSourceReference[] CopyHeadReferences(CancellationToken token)
    {
        RequireCompleted(token);int count=spans[0].Worker.BatchTextReferenceCount(spans[0],token);
        ChargeTextPreparationCopy(checked(104L*count),token);
        return spans[0].Worker.CopyBatchReferences(spans[0],token);
    }
    internal LegacyNativeInkPositionRange[] CopyHeadRanges(CancellationToken token)
    {
        RequireCompleted(token);var doc=scope.Document;
        var map=new LegacyNativeTextSourceInputMap(doc.CopySelectedInputs(),[],0);
        LegacyNativeInkPositionRange[] original=[new(LegacyNativeInkPosition.Pack(0,0,3,doc.Strokes[0].Points.Count),
            LegacyNativeInkPosition.Pack((uint)doc.ObjectCount-1,doc.Strokes[^1].Points.Count-1,5,doc.Strokes[^1].Points.Count))];
        return spans[0].Worker.ProjectBatchTextRanges(spans[0],map,original,token);
    }
    public sealed class Result
    {
        private readonly LegacyFreshMainGlobalWordAugmentation.Result[] spans;
        public int WordNodeCount{get;}public int UniqueSpanCount=>spans.Length;public int QuotaAfter{get;}
        public long StructuralWorkReserve{get;}public long EnumeratedCopyReserve{get;}
        internal Result(int count,LegacyFreshMainGlobalWordAugmentation.Result[] spans,int quota,long work,long copy)
        {WordNodeCount=count;this.spans=spans;QuotaAfter=quota;StructuralWorkReserve=work;EnumeratedCopyReserve=copy;}
        public LegacyFreshMainGlobalWordAugmentation.Result[] CopySpanResults()=>spans.ToArray();
        //Detached DATA results are never completion/WORD/TEXT authority.
    }
}
