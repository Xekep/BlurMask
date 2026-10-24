using System.Buffers.Binary;
using System.Security.Cryptography;
namespace InkRecognition;

/// <summary>Explicit configured2ALT same-TOP source completion. The existing
/// NoCandidate path is unchanged for bit8 refusal. This is not app acceptance.</summary>
public sealed partial class LegacyFreshMainConfiguredWordAugmentation
{
    private readonly object gate=new();
    private readonly LegacyConfiguredSourceOperationDebit debit;
    private readonly LegacyFreshMainConfiguredSingleLinePlainRecognitionResult source;
    private readonly LegacyOwnedMainCandidateSource[] candidates;
    private readonly LegacyNativePublishedWordNode[] heads;
    private readonly LegacyNativePlainWordPublisher publisher;
    private readonly LegacyFreshCaseMainSourcePolicy policy;
    private readonly LegacyMainConfiguredAlternativeResidue residue;
    private readonly byte[] compact;
    private readonly int[] starts;
    private readonly int compactBackingBytes;
    private readonly uint[] sourceHeaders;
    private readonly int publisherHistoryCount;
    private readonly LegacyOwnedRussianMainSourceSymbolCallbacks callbacks;
    private readonly LegacyOwnedRussianMainInteriorDelimiter interior;
    private readonly LegacyOwnedRussianOrdinaryGlyphProducer ordinary;
    private readonly LegacyOwnedRussianProtocolGlyphProducer protocol;
    private readonly LegacyFreshMainGlyphUtf8Emission emission;
    private readonly ushort marker,joiner,closed,unit,softHyphen;
    private LegacyOwnedMainWordSourceCollection? sources;
    private int started;
    private bool faulted,complete;
    private int controller24a0;
    private LegacyGlobalSpanAlternativeWalkKernel.Result? enteredEmission;
    public bool IsTopV68Completed=>complete;
    public bool IsFaulted=>faulted;
    public bool SourcesMarkedComplete {get;private set;}
    public int EligibleCandidates {get;private set;}
    public int AppendedWords {get;private set;}
    public int DuplicateWords {get;private set;}
    public int UpdatedNewDuplicates {get;private set;}
    public int QuotaAfter {get;private set;}
    public LegacyOwnedMainWordSourceCollection Sources=>sources??throw new InvalidOperationException("Source callback has not exported this span.");

    private LegacyFreshMainConfiguredWordAugmentation(byte[] resource,
        LegacyFreshMainConfiguredSingleLinePlainRecognitionResult source,LegacyOwnedMainCandidateSource[] candidates,
        LegacyNativePublishedWordNode[] heads,LegacyMainConfiguredAlternativeResidue residue,
        LegacyCursiveSeparationTable table,LegacyNativeUnicodeNormalizationData normalization,
        LegacyNativeCircledPayloadData circles,CancellationToken token,LegacyConfiguredSourceOperationDebit debit,RawCollector? raw)
    {
        this.raw=raw;this.debit=debit;debit.Attach(this);
        this.source=source;this.candidates=candidates;this.heads=heads;this.residue=residue;
        publisher=candidates[0].Ownership.WordPublisher;policy=candidates[0].SourcePolicy;
        publisherHistoryCount=publisher.RequireConfiguredActiveHeads(heads,token);
        sourceHeaders=policy.Controller.Owner.CopySourceHeaders();
        if(source.Compact.RecordCount is<1 or>131072)throw new NotSupportedException("Bounded actual active compact count required.");
        // Actual configured Run caps original compact allocation at64MiB and
        // publication at131072 rows; growth cannot exceed that larger cap.
        byte[] backing=source.Compact.CopyCompactStorage();compactBackingBytes=backing.Length;
        if(backing.Length>64*1024*1024||source.Compact.RecordCount>backing.Length/40)
            throw new NotSupportedException("Actual configured compact backing envelope changed.");
        compact=backing.AsSpan(0,checked(source.Compact.RecordCount*40)).ToArray();starts=source.Compact.CopyNodeStarts2478();
        callbacks=LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(resource,candidates[0].ConfidenceOwner,source.Sampled,token);
        interior=LegacyOwnedRussianMainInteriorDelimiter.Prepare(resource,callbacks,source.Geometry,token);
        ordinary=LegacyOwnedRussianOrdinaryGlyphProducer.PrepareConfiguredAugmentation(resource,policy,candidates[0].ConfidenceOwner,table,debit,token);
        protocol=LegacyOwnedRussianProtocolGlyphProducer.PrepareCase(resource,policy,callbacks,interior,token);
        emission=LegacyFreshMainGlyphUtf8Emission.PrepareConfigured(candidates[0],normalization,circles,token);
        ushort Label(string value)=>policy.Model.AcousticAlphabet.TryGetLabel(value,out ushort label)?label:ushort.MaxValue;
        marker=Label("\u0003");joiner=Label("\u200d");closed=Label("\U000f0001");softHyphen=Label("\u00ad");
        unit=BinaryPrimitives.ReadUInt16LittleEndian(resource.AsSpan(105));
        raw?.Bind(this);
    }
    public static LegacyFreshMainConfiguredWordAugmentation Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainConfiguredSingleLinePlainRecognitionResult actual,LegacyCursiveSeparationTable table,
        LegacyNativeUnicodeNormalizationData normalization,LegacyNativeCircledPayloadData circles,
        LegacyMainConfiguredAlternativeResidue callerResidue,CancellationToken token=default)
        =>PrepareCore(resource,actual,table,normalization,circles,callerResidue,token,null);
    private static LegacyFreshMainConfiguredWordAugmentation PrepareCore(ReadOnlySpan<byte> resource,
        LegacyFreshMainConfiguredSingleLinePlainRecognitionResult actual,LegacyCursiveSeparationTable table,
        LegacyNativeUnicodeNormalizationData normalization,LegacyNativeCircledPayloadData circles,
        LegacyMainConfiguredAlternativeResidue callerResidue,CancellationToken token,RawCollector? raw)
    {
        ArgumentNullException.ThrowIfNull(actual);ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(normalization);ArgumentNullException.ThrowIfNull(circles);
        ArgumentNullException.ThrowIfNull(callerResidue);token.ThrowIfCancellationRequested();
        using var preparation=LegacyConfiguredSourceCompletionClaim.Acquire(actual.Alternatives.TopOwner??throw new InvalidOperationException("Actual configured TOP required."),token);
        var alternatives=actual.Alternatives.CopyActiveAlternatives();
        if(!actual.Alternatives.IsComplete||actual.Alternatives.IsFaulted||alternatives.Length!=2)
            throw new NotSupportedException("Exactly two completed actual configured alternatives required.");
        var candidates=alternatives.Select(a=>LegacyOwnedMainCandidateSource.FromConfigured(actual,a,token)).ToArray();
        var first=candidates[0];var policy=first.SourcePolicy;var controller=policy.Controller;
        if(resource.Length!=572763)throw new ArgumentException("Same complete primary acoustic resource required.");
        // Authenticate one private snapshot BEFORE any category/scalar read.
        var debit=LegacyConfiguredSourceOperationDebit.Prepare(actual,first,token);
        byte[] captured=resource.ToArray();
        try
        {
        if(!Convert.ToHexString(SHA256.HashData(captured)).Equals(
            controller.Owner.SelectedSourceOwner.SourceFingerprint,StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Same actual primary acoustic resource required.");
        if(policy.HasAssociatedSource||policy.Model.SourceKind!=0x135||controller.SecondaryScaleEc!=0||
            controller.SecondaryCountProperty12d0003!=2||controller.Configuration.Trace2368!=0||
            controller.Configuration.Mode22f6!=0||controller.Refresh.KnowledgeOption22f8!=0||controller.Refresh.SourceFormat22f9!=0||
            first.TopOwner.OwnerC4)
            throw new NotSupportedException("Fresh configured2 nontracing ordinary135 source required.");
        var heads=new LegacyNativePublishedWordNode[2];
        for(int i=0;i<2;i++)
        {
            var c=candidates[i];c.Validate(token);
            if(!ReferenceEquals(c.TopOwner,first.TopOwner)||!ReferenceEquals(c.SourceIdentity,first.SourceIdentity)||
                !ReferenceEquals(c.ConfidenceOwner,first.ConfidenceOwner)||c.WordHead18 is not{} head||head.Next is not null||
                i!=0&&head.Span!=heads[0].Span)
                throw new NotSupportedException("Both genuine279 heads must select one same-TOP275 span.");
            heads[i]=head;
        }
        foreach(string label in new[]{"\u2610","\u2611","\u2612"})
            if(policy.Model.AcousticAlphabet.TryGetLabel(label,out _))throw new NotSupportedException("Enabled checkbox rewrite requires its own owner.");
        // A private snapshot, not imported compact/rows/candidates or model outputs.
        var copied=new LegacyMainConfiguredAlternativeResidue(callerResidue.Word,callerResidue.Backtrack,
            callerResidue.Glyph,callerResidue.Labels,callerResidue.Request,callerResidue.Leading,
            callerResidue.Trailing,callerResidue.Left,callerResidue.Right);
        token.ThrowIfCancellationRequested();
        return new(captured,actual,candidates,heads,copied,table,normalization,circles,token,debit,raw);
        }
        finally { CryptographicOperations.ZeroMemory(captured); }
    }
    private void Validate(CancellationToken token,bool entered)
    {
        token.ThrowIfCancellationRequested();
        if(faulted||source.Alternatives.IsFaulted||!source.Alternatives.IsComplete)
            throw new InvalidOperationException("Healthy exact configured source required.");
        var actual=source.Alternatives.CopyActiveAlternatives();
        if(actual.Length!=2)throw new InvalidOperationException("Configured ALT membership changed.");
        for(int i=0;i<2;i++)
        {
            candidates[i].Validate(token);
            if(!ReferenceEquals(actual[i],candidates[i].ConfiguredAlternative)||!ReferenceEquals(heads[i],candidates[i].WordHead18))
                throw new InvalidOperationException("Actual configured ALT order or279 owner changed.");
        }
        if(publisher.RequireConfiguredActiveHeads(heads,token)!=publisherHistoryCount)
            throw new InvalidOperationException("Configured publisher history changed.");
        _=publisher.CaptureSpan(heads[0],heads,token);
        if(entered)LegacyConfiguredSourceCompletionClaim.Require(candidates[0].TopOwner,this,false,token);
    }
    private void RequireEntered(CancellationToken token)
    {
        // Fixed two-ALT scalar checks only. Full publication traversal is kept
        // at entry/stage boundaries, not multiplied by each source callback.
        token.ThrowIfCancellationRequested();
        if(faulted||complete||candidates[0].ConfiguredAlternative.IsReleased||candidates[1].ConfiguredAlternative.IsReleased||
            !candidates[0].Session.IsComplete||!candidates[1].Session.IsComplete||
            candidates[0].Session.IsStopped||candidates[1].Session.IsStopped)
            throw new InvalidOperationException("Live exact configured source stage required.");
        publisher.RequireConfiguredHistoryCount(publisherHistoryCount,heads[0]);
        candidates[1].Ownership.WordPublisher.RequireConfiguredHistoryCount(publisherHistoryCount,heads[1]);
        LegacyConfiguredSourceCompletionClaim.Require(candidates[0].TopOwner,this,false,token);
    }
    public void Run(CancellationToken token=default)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();if(started!=0)throw new InvalidOperationException("Entered configured augmentation cannot retry.");
            using var preflight=LegacyConfiguredSourceCompletionClaim.Acquire(candidates[0].TopOwner,token);
            Validate(token,false);
            var initial=publisher.CaptureSpan(heads[0],heads,token);
            var budget=LegacyConfiguredWordAugmentationEnvelope.Prepare(compact,starts,policy.Graph.NodeCount,initial,compactBackingBytes,publisherHistoryCount,token);
            debit.Selected(this,budget.Estimate);
            var working=candidates[0].Ownership.CreateWorkingWord(residue.Word,token);
            preflight.Enter(this,true,token);started=1;preflight.Dispose();
            try
            {
                // No preclaim stateful walk/emission.13E7F0 marks shared275 only
                // AFTER219580 callback returns, including its failed prefix.
                try{debit.Enter(this,token);raw?.Begin(initial,token);Augment(working,budget,initial.Words.Count,token);}
                finally
                {
                    LegacyConfiguredSourceCompletionClaim.MarkAfterCallback(candidates[0].TopOwner,this);
                    SourcesMarkedComplete=true;
                }
                Rank(budget,token);
                if(controller24a0!=0)throw new InvalidOperationException("Actual augmentation row scratch remains pending.");
                token.ThrowIfCancellationRequested();debit.Complete(token);LegacyConfiguredSourceCompletionClaim.Complete(candidates[0].TopOwner,this,token);complete=true;
            }
            catch{faulted=true;LegacyConfiguredSourceCompletionClaim.Poison(candidates[0].TopOwner,this);throw;}
        }
    }
    private void Augment(LegacyNativeWordBuilder word,LegacyConfiguredWordAugmentationEnvelope budget,int originalCount,CancellationToken token)
    {
        Validate(token,true);
        // Fresh frontendV228 only invalidates its optimization; no quote orF4
        // normalization is repeated after the configured ALT assembly did it.
        sources=LegacyOwnedMainWordSourceCollection.PrepareConfigured(candidates[0],token);
        var walker=new LegacyGlobalSpanAlternativeWalkKernel(compact,compact.Length/40,policy.Graph.NodeCount,
            marker,joiner,closed,unit,softHyphen,residue.Request,First,Last,(request,cursor,ct)=>
            {
                budget.Request();RequireEntered(ct);var d=BinaryPrimitives.ReadInt32LittleEndian(request.AsSpan(40))==1?
                    interior.ReadDelimiter(request,cursor,ct):callbacks.ReadDelimiter(request,cursor,ct);
                if(!ReferenceEquals(d.Owner,callbacks))throw new InvalidOperationException("Actual configured delimiter owner required.");
                return new(d.RemainingCursor,d.EmittedCount,d.NativeFlag0C,d.NativeFlag0D);
            });
        var scan=LegacyPreparedMainWordAugmentationCandidates.Scan(compact,compact.Length/40,starts,heads[0].Span.First,heads[0].Span.Last,
            candidates[0].TopOwner.OwnerC4,First,Last,(index,minimum)=>
            {
                RequireEntered(token);var inspect=walker.Run(index,minimum,false,ushort.MaxValue,[],token);controller24a0=0;
                return new(inspect.Status,inspect.LastSourceKey);
            },token);
        EligibleCandidates=scan.CompactIndices.Length;
        raw?.Integers(2,scan.CompactIndices);
        foreach(int candidate in scan.CompactIndices)
        {
            RequireEntered(token);raw?.Candidate(candidate);word.ResetPlain(token);word.ApplyAugmentationFlag32((compact[candidate*40+32]&16)!=0,token);
            // Native219A81 tests this only AFTERentry/reset. No fabricatedV70
            // fiveDWORD data is substituted; failure retains sharedmark+poison.
            if(BinaryPrimitives.ReadInt32LittleEndian(compact.AsSpan(candidate*40+36))!=-1)
                throw new NotSupportedException("Actual configured dictionaryV70 auxiliary fiveDWORD issuer is not yet connected.");
            int minimum=heads[0].Span.First<0?-1:LegacyPreparedMainWordAugmentationCandidates.DecodeContextNode(heads[0].Span.First);
            var rows=walker.Run(candidate,minimum,true,65534,residue.Backtrack,token);
            enteredEmission=rows;controller24a0=rows.Rows72.Length/72;
            raw?.Bytes(3,rows.Rows72);
            if(rows.Status is 1 or -1)throw new InvalidOperationException("Actual configured direct-parent emission failed.");
            if(rows.ResidualReferences.Length!=0)
                throw new NotSupportedException("Configured219580 common-frontend residual217DB0 lease is not yet joined; candidate-local ALT overlay cannot substitute.");
            if(!candidates[0].Ownership.TryReadTopWidthStatistics(out uint width))
                throw new InvalidOperationException("Actual first ALT must retain genuine TOP.B0 width.");
            word.ApplySpanAlternativeFlags30(rows.WordFlags,token);
            var chain=LegacyMainOrdinaryGlyphChainKernel.Apply(word,rows.Rows72,[],policy.Graph.NodeCount,
                compact.Length/40,starts,compact,n=>policy.Graph.GetNode(n).Count,ordinary.Produce,
                (row,pool,scratch,ac,b0,request,ct)=>
                {budget.Request();RequireEntered(ct);return protocol.Produce(row,pool,scratch,ac,b0,request,ct).Finalization;},
                ushort.MaxValue,2,width,false,residue.Glyph,residue.Labels,residue.Request,1,token);
            if(!chain.Succeeded)throw new InvalidOperationException("Actual configured218560 returned failure.");
            controller24a0=0;
            var costs=LegacyPreparedMainAugmentedWordCosts.Calculate(compact,compact.Length/40,candidate,rows.LastDirectParentIndex,
                LegacyPreparedMainWordAugmentationCandidates.DecodeContextNode(heads[0].Span.First),
                LegacyPreparedMainWordAugmentationCandidates.DecodeContextNode(heads[0].Span.Last),0x135,token);
            byte[] data=word.CopyStorage(token);word.SetRawScalars(costs.Score10,BinaryPrimitives.ReadSingleLittleEndian(data.AsSpan(20)),costs.Cost18,costs.Cost1C,token);
            raw?.Word(4,word,token);
            var before=publisher.CaptureSpan(heads[0],heads,token);int match=-1;
            for(int i=0;i<before.Words.Count;i++)if(Equal(before.Words[i].CopyStorage(),word.CopyStorage(token),budget,token)){match=i;break;}
            if(match==-1){_=publisher.InternPlainWord(word,heads[0].Span,out _,token);AppendedWords++;}
            else
            {
                DuplicateWords++;var existing=before.Words[match].CopyStorage();
                if(match>=originalCount&&(ClassWord(rows.WordFlags,existing)||costs.Cost18<BinaryPrimitives.ReadSingleLittleEndian(existing.AsSpan(24))))
                {publisher.UpdateAugmentedDuplicate(before,match,originalCount,costs.Score10-0.00001f,costs.Cost18,costs.Cost1C,rows.WordFlags,token);UpdatedNewDuplicates++;}
                if(chain.FirstNewReference!=ulong.MaxValue)_=candidates[0].TopOwner.GlyphPool.RemoveCreatedChainSegment(chain.FirstNewReference,chain.LastProcessedReference,token);
            }
            raw?.Outcome(match,AppendedWords,DuplicateWords,UpdatedNewDuplicates);
        }
        int First(int node,ushort arc){budget.Request();RequireEntered(token);return unchecked((int)policy.Graph.GetNode(node)[arc].StartContextId);}
        int Last(int node,ushort arc){budget.Request();RequireEntered(token);return unchecked((int)policy.Graph.GetNode(node)[arc].EndContextId);}
        bool ClassWord(ushort flag,byte[] old)
        {
            if(BinaryPrimitives.ReadUInt16LittleEndian(old.AsSpan(48))!=flag||unchecked((short)flag)<0)return false;
            if(flag>=sourceHeaders.Length)throw new NotSupportedException("Actual configured source header ordinal required.");
            return(sourceHeaders[flag]&0x1c)!=0;
        }
    }
    private bool Equal(ReadOnlySpan<byte> left,ReadOnlySpan<byte> right,LegacyConfiguredWordAugmentationEnvelope budget,CancellationToken token)
    {
        budget.Equality();
        if(raw is null)return LegacyNativeMainWordEquality.Compare(candidates[0].TopOwner.GlyphPool,left,right,
            residue.Left,residue.Right,joiner,static symbol=>symbol,text=>emission.ReadControlLabel(text,token),token);
        raw.EqualityBefore(left,right,residue.Left,residue.Right,joiner);
        bool equal=LegacyNativeMainWordEquality.CompareWithReadObserver(candidates[0].TopOwner.GlyphPool,left,right,
            residue.Left,residue.Right,joiner,static symbol=>symbol,text=>
            {ushort label=emission.ReadControlLabel(text,token);raw.Control(text,label);return label;},token,raw.ReadObserver);
        raw.EqualityAfter(equal,residue.Left,residue.Right);return equal;
    }
    private void Rank(LegacyConfiguredWordAugmentationEnvelope budget,CancellationToken token)
    {
        Validate(token,true);var current=publisher.CaptureSpan(heads[0],heads,token);
        raw?.Snapshot(10,current,token);
        var ranked=LegacyPreparedMainWordRanking.Rank(current.Words.Select(w=>w.CopyStorage()).ToArray(),current.SelectedOrdinals.ToArray(),
            LegacyPreparedMainWordRanking.InitialQuota(2,65535,false),false,(a,b)=>Equal(a,b,budget,token),token);
        raw?.Ranking(ranked);
        publisher.ApplySpanRanking(current,ranked.Words,ranked.OutputSourceOrdinals,ranked.OldToNew,token);QuotaAfter=ranked.QuotaAfter;
        current=publisher.CaptureSpan(heads[0],heads,token);raw?.Snapshot(12,current,token);var scored=LegacyPreparedMainSpanScorePublication.Normalize(current.Words.Select(w=>w.CopyStorage()).ToArray(),token);
        raw?.Words(13,scored.Words);raw?.Integers(14,scored.ScoreOrder);
        publisher.ApplySpanScores(current,scored.Words,token);
        raw?.FinalHeads(token);
    }
    // Fault-terminal diagnostic DATA only, never a resume/source constructor.
    // One requested copy is bounded by the existing4096-row executor limit.
    internal byte[] CopyFaultEmissionRowsForDiagnostic(CancellationToken token)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(!faulted||complete||started==0)throw new InvalidOperationException("An actual entered failure prefix required.");
            if(enteredEmission is null)return [];
            if(enteredEmission.Rows72.Length%72!=0||enteredEmission.Rows72.Length>4096*72)
                throw new InvalidDataException("Actual fault row prefix exceeds diagnostic reserve.");
            byte[] copy=enteredEmission.Rows72.ToArray();token.ThrowIfCancellationRequested();return copy;
        }
    }
    internal void RequireCompleted(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(!complete||faulted||!SourcesMarkedComplete)throw new InvalidOperationException("Successful configured augmentation required.");
        Validate(token,false);LegacyConfiguredSourceCompletionClaim.Require(candidates[0].TopOwner,this,true,token);
    }
    internal LegacyOwnedMainWordSourceCollection SourcesFor(LegacyOwnedMainCandidateSource candidate,CancellationToken token)
    {
        RequireCompleted(token);if(!candidates.Any(c=>ReferenceEquals(c.CandidateIdentity,candidate.CandidateIdentity)&&ReferenceEquals(c.SourceIdentity,candidate.SourceIdentity)))
            throw new ArgumentException("Exact candidate of completed configured augmentation required.");
        return Sources.BindSameSpanCandidate(candidate,token);
    }
}

// Trusted NEW-only same-class accessor. It bounds public ForkAlternative history
// BEFORE CaptureSpan's registered-head searches; never substitutes singleton
// GLOBAL validation. No publisher fields/policies are modified by this seam.
public sealed partial class LegacyNativePlainWordPublisher
{
    internal int RequireConfiguredActiveHeads(IReadOnlyList<LegacyNativePublishedWordNode> heads,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();int n=interning.Publishers.Count;
        if(released||n is<2 or>4096||heads.Count!=2||ReferenceEquals(heads[0],heads[1]))
            throw new NotSupportedException("Bounded genuine two-head configured publisher history required.");
        int found=0;
        for(int i=0;i<n;i++)
        {
            token.ThrowIfCancellationRequested();var p=interning.Publishers[i];if(p.released||p.Head is null)continue;
            if(ReferenceEquals(p.Head,heads[0]))found|=1;
            else if(ReferenceEquals(p.Head,heads[1]))found|=2;
            else throw new InvalidOperationException("Unexpected active279 publisher outside actual configured ALT set.");
        }
        if(found!=3||interning.Publishers.Count!=n)throw new InvalidOperationException("Configured279 membership changed.");
        return n;
    }
    internal void RequireConfiguredHistoryCount(int expected,LegacyNativePublishedWordNode head)
    {
        if(released||interning.Publishers.Count!=expected||!ReferenceEquals(Head,head))
            throw new InvalidOperationException("Actual entered configured publisher identity changed.");
    }
}
