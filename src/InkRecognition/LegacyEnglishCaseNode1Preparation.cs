namespace InkRecognition;
// Opt-in node1 source entry only. OLD preparation paths remain byte-for-byte unchanged.
internal sealed partial class LegacyEnglishCaseNode0Completion
{
internal static LegacyEnglishCaseNode0Completion PrepareNode1Spans(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,
        LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,long maximumLogicalBytes=128L*1024*1024,CancellationToken token=default, LegacyEnglishCaseNode0Completion.GraphPeriodicPreparation? periodicPreparation=null)
    {
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(knowledge);ArgumentNullException.ThrowIfNull(numeric);
        if(periodicPreparation is not null)_=periodicPreparation.Value.Require(knowledge,token);
        // Fixed ledger construction precedes the OLD root claim. No resource
        // copy/hash/load, code-cache dictionary or terminal index is hidden here.
        PreflightNode1WholeStart(knowledge,token);
        LegacyEnglishCaseNode0ModelIndexPreparation.Prepare(numeric,token:token);
        var ledger=LegacyEnglishCaseNode0Ledger.Create(maximumLogicalBytes);
        var fresh=LegacyEnglishCaseNode0NativeFreshPolicy.Prepare(knowledge,numeric,ledger,token);
        var index=LegacyEnglishCaseNode0LexiconIndex.Prepare(numeric,ledger,token);
        // NEW immutable holder only: owner8 + packed44 + four code refs32.
        // OLD NumericTables constructor remains exact original. Seven lookups
        // execute only below through the exact private prepared-consumer seam.
        // 12256 is conditional .NET10.0.12 Release source work, not pack/CPU proof.
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.NumericHandleMetadata,84);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.NumericHandleMetadata,180);
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.NumericHandlePrepare,12256);
        // Additional node0 attachment field on the existing OLD primary type.
        // OLD standalone guards/reservations are not changed.
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,8);
        // Unconditional instance-field surcharge for the81 requests built by this one-child source preparation.
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,648);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,648);
        var primary=LegacyEnglishCasePrimaryCompletion.PrepareNode1Spans(knowledge,numeric,cases,publication,maximumLogicalBytes,token,periodicPreparation,periodicPreparation is null?null:ledger);
        try
        {
            ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,264+48+64);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,264+8+64);
            var result=new LegacyEnglishCaseNode0Completion(primary,ledger,index,fresh);result.graphPeriodicPrepared=periodicPreparation is not null;token.ThrowIfCancellationRequested();primary.AttachNode0Completion(result);return result;
        }
        catch{primary.FaultUnattachedNode0Preparation();throw;}
    }

internal static LegacyEnglishCaseNode0Completion PrepareNode1(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,
        LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,long maximumLogicalBytes=128L*1024*1024,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(knowledge);ArgumentNullException.ThrowIfNull(numeric);
        // Fixed ledger construction precedes the OLD root claim. No resource
        // copy/hash/load, code-cache dictionary or terminal index is hidden here.
        PreflightNode1(knowledge,token);
        LegacyEnglishCaseNode0ModelIndexPreparation.Prepare(numeric,token:token);
        var ledger=LegacyEnglishCaseNode0Ledger.Create(maximumLogicalBytes);
        var fresh=LegacyEnglishCaseNode0NativeFreshPolicy.Prepare(knowledge,numeric,ledger,token);
        var index=LegacyEnglishCaseNode0LexiconIndex.Prepare(numeric,ledger,token);
        // NEW immutable holder only: owner8 + packed44 + four code refs32.
        // OLD NumericTables constructor remains exact original. Seven lookups
        // execute only below through the exact private prepared-consumer seam.
        // 12256 is conditional .NET10.0.12 Release source work, not pack/CPU proof.
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.NumericHandleMetadata,84);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.NumericHandleMetadata,180);
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.NumericHandlePrepare,12256);
        // Additional node0 attachment field on the existing OLD primary type.
        // OLD standalone guards/reservations are not changed.
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,8);
        // Unconditional instance-field surcharge for the81 requests built by this one-child source preparation.
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,648);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,648);
        var primary=LegacyEnglishCasePrimaryCompletion.PrepareNode1(knowledge,numeric,cases,publication,maximumLogicalBytes,token);
        try
        {
            ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,264+48+64);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,264+8+64);
            var result=new LegacyEnglishCaseNode0Completion(primary,ledger,index,fresh);token.ThrowIfCancellationRequested();primary.AttachNode0Completion(result);return result;
        }
        catch{primary.FaultUnattachedNode0Preparation();throw;}
    }
}
internal sealed partial class LegacyEnglishCasePrimaryCompletion
{
internal static LegacyEnglishCasePrimaryCompletion PrepareNode1Spans(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,
        LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,long maximumLogicalBytes=128L*1024*1024,CancellationToken token=default, LegacyEnglishCaseNode0Completion.GraphPeriodicPreparation? periodicPreparation=null,LegacyEnglishCaseNode0Ledger? wideLedger=null)
    {
        ArgumentNullException.ThrowIfNull(knowledge);ArgumentNullException.ThrowIfNull(numeric);token.ThrowIfCancellationRequested();
        if(maximumLogicalBytes<CompleteLogicalCeiling||maximumLogicalBytes>128L*1024*1024||!ReferenceEquals(knowledge.Source,numeric.DataOwner.Source))
            throw new ArgumentException("Complete same-source primary-only logical envelope required before claim.");
        if(1289+217!=MaximumContextNodes||MaximumContextNodes>LegacyEnglishCaseExecutionPoolReceipt.Capacity)throw new InvalidDataException("Complete context preflight required.");
        var consumer=LegacyEnglishCaseInitialQueryPass.PrepareNode1SpansConsumer(knowledge,numeric,cases,publication,maximumLogicalBytes,token,periodicPreparation,wideLedger);
        var result=new LegacyEnglishCasePrimaryCompletion(consumer,knowledge,numeric);consumer.AttachPrimaryCompletion(result);return result;
    }

internal static LegacyEnglishCasePrimaryCompletion PrepareNode1(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,
        LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,long maximumLogicalBytes=128L*1024*1024,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(knowledge);ArgumentNullException.ThrowIfNull(numeric);token.ThrowIfCancellationRequested();
        if(maximumLogicalBytes<CompleteLogicalCeiling||maximumLogicalBytes>128L*1024*1024||!ReferenceEquals(knowledge.Source,numeric.DataOwner.Source))
            throw new ArgumentException("Complete same-source primary-only logical envelope required before claim.");
        if(1289+217!=MaximumContextNodes||MaximumContextNodes>LegacyEnglishCaseExecutionPoolReceipt.Capacity)throw new InvalidDataException("Complete context preflight required.");
        var consumer=LegacyEnglishCaseInitialQueryPass.PrepareNode1Consumer(knowledge,numeric,cases,publication,maximumLogicalBytes,token);
        var result=new LegacyEnglishCasePrimaryCompletion(consumer,knowledge,numeric);consumer.AttachPrimaryCompletion(result);return result;
    }
    internal LegacyEnglishCaseInitialQueryPass Node1Pass(LegacyEnglishCaseNode0Completion actual)
    {
        if(!ReferenceEquals(node0,actual)||!actual.IsRunningFor(this)||Volatile.Read(ref phase)!=2||completionPass is null||pending is null)
            throw new InvalidOperationException("Same completed primary and running owning node1 caller required.");
        return completionPass;
    }
}
internal sealed partial class LegacyEnglishCaseInitialQueryPass
{
internal static LegacyEnglishCaseInitialQueryPass PrepareNode1SpansPass(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,
        LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,long maximumLogicalBytes=128L*1024*1024,Action<string>? observer=null,CancellationToken token=default, LegacyEnglishCaseNode0Completion.GraphPeriodicPreparation? periodicPreparation=null,LegacyEnglishCaseNode0Ledger? wideLedger=null)
    {
        var spanBounds=periodicPreparation is null?PrepareNode1SpansD0Bounds(knowledge,token):periodicPreparation.Value.Require(knowledge,token);
        if((periodicPreparation is null)!=(wideLedger is null))throw new InvalidOperationException("Periodic preparation and its actual ledger must travel together.");
        var pool=periodicPreparation is null?LegacyEnglishCaseExecutionPoolReceipt.Preflight(knowledge,numeric,maximumLogicalBytes,token):
            LegacyEnglishCaseExecutionPoolReceipt.PreflightOuterWide(knowledge,numeric,maximumLogicalBytes,wideLedger!,publication,periodicPreparation.Value,token);
        var initializer=LegacyEnglishCaseQueryInitialization.PrepareExecution(knowledge,numeric,cases,publication,pool,observer,token);var result=new LegacyEnglishCaseInitialQueryPass(initializer);bool claimed=false;
        try
        {
            result.Root.ClaimInitialQueryOwner(result,token);claimed=true;observer?.Invoke("execution:claimed");token.ThrowIfCancellationRequested();
            result.Scorer=LegacyEnglishFreshCaseMainScorer.Prepare(result.Root,token);observer?.Invoke("execution:scorer");token.ThrowIfCancellationRequested();
            result.SpanSession=LegacyEnglishFreshCaseMainSpanSession.Prepare(result.Scorer,spanBounds,token);
            observer?.Invoke("execution:prepared");token.ThrowIfCancellationRequested();return result;
        }
        catch{if(claimed){Volatile.Write(ref result.phase,3);result.Root.PoisonInitializationOwner(initializer);}throw;}
    }

    // Exact ordinary graph2..49 D0 schedule, validated before any execution/root claim.
    // Larger graph node1-prefix preparation keeps its prior three-pass bound;
    // the separate periodic continuation must prepare its own authenticated schedule.
    private static LegacyEnglishMainD0Bounds PrepareNode1SpansD0Bounds(LegacyEnglish119MainKnowledge knowledge,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(knowledge);token.ThrowIfCancellationRequested();
        var local=knowledge.Owner;var graph=local.RetainCompletedD0Graph(token);
        int preparations=3,selectedRows=local.CandidateCount;
        if(graph.NodeCount is>=2 and<=49)
        {
            LegacyEnglishCaseNode0Completion.PreflightGraphLoopPayload(knowledge,token);
            preparations=1;selectedRows=0; // The initial consumer executes node0/span0 exactly once.
            for(int node=0;node<graph.NodeCount;node++)
            {
                token.ThrowIfCancellationRequested();var arcs=graph.GetNode(node);
                if(arcs.Count is<1 or>1024)throw new NotSupportedException("Actual nonempty bounded graph D0 schedule required before claim.");
                int maximum=0;
                foreach(var arc in arcs)
                {
                    token.ThrowIfCancellationRequested();
                    if(arc.Span>=8||arc.Span>node)throw new NotSupportedException("Actual bounded graph D0 span required before claim.");
                    maximum=Math.Max(maximum,arc.Span);selectedRows=checked(selectedRows+1);
                }
                if(node!=0)preparations=checked(preparations+maximum+1);
            }
            if(selectedRows!=graph.CandidateCount||selectedRows!=local.CandidateCount||graph.NodeCount!=local.NodeCount||graph.Groups.Count!=local.GroupCount)
                throw new InvalidDataException("Exact complete actual graph D0 row/dimension accounting required before claim.");
        }
        var bounds=new LegacyEnglishMainD0Bounds(preparations,selectedRows);
        long d0Envelope=LegacyEnglishMainSpanSession.Preflight(local.GroupCount,local.NodeCount,local.CandidateCount,bounds);
        long scoreEnvelope=LegacyEnglishMainInkSnapshot.Preflight(local.GroupCount,local.NodeCount,local.CandidateCount);
        if(checked(d0Envelope+scoreEnvelope+1048576)>LegacyEnglishFreshCaseMainSource.CompleteLogicalEnvelopeUpperBytes)
            throw new ArgumentException("Complete additive case envelope exceeded before claim.");
        return bounds;
    }

internal static LegacyEnglishCaseInitialConsumer PrepareNode1SpansConsumer(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,
        LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,long maximumLogicalBytes=128L*1024*1024,CancellationToken token=default, LegacyEnglishCaseNode0Completion.GraphPeriodicPreparation? periodicPreparation=null,LegacyEnglishCaseNode0Ledger? wideLedger=null)
    {
        // Every large source/budget preflight precedes issuing this private pass.
        var policy=LegacyEnglishCaseInitialSourcePolicy.Prepare(knowledge,numeric,maximumLogicalBytes,token);
        var frontend=knowledge.Owner.RetainInitialConsumerSource(knowledge,token);
        var heap=new LegacyEnglishCaseInitialHeap(policy);
        var pass=PrepareNode1SpansPass(knowledge,numeric,cases,publication,maximumLogicalBytes,token:token,periodicPreparation:periodicPreparation,wideLedger:wideLedger);
        try
        {
            policy.Bind(pass);frontend.Bind(pass);var consumer=new LegacyEnglishCaseInitialConsumer(pass,policy,frontend,heap);pass.initialConsumer=consumer;
            token.ThrowIfCancellationRequested();return consumer;
        }
        catch{Volatile.Write(ref pass.phase,3);pass.Root.FaultInitialExecutionOwner(pass);throw;}
    }

internal static LegacyEnglishCaseInitialConsumer PrepareNode1Consumer(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,
        LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,long maximumLogicalBytes=128L*1024*1024,CancellationToken token=default)
    {
        // Every large source/budget preflight precedes issuing this private pass.
        var policy=LegacyEnglishCaseInitialSourcePolicy.Prepare(knowledge,numeric,maximumLogicalBytes,token);
        var frontend=knowledge.Owner.RetainInitialConsumerSource(knowledge,token);
        var heap=new LegacyEnglishCaseInitialHeap(policy);
        var pass=PrepareNode1Pass(knowledge,numeric,cases,publication,maximumLogicalBytes,token:token);
        try
        {
            policy.Bind(pass);frontend.Bind(pass);var consumer=new LegacyEnglishCaseInitialConsumer(pass,policy,frontend,heap);pass.initialConsumer=consumer;
            token.ThrowIfCancellationRequested();return consumer;
        }
        catch{Volatile.Write(ref pass.phase,3);pass.Root.FaultInitialExecutionOwner(pass);throw;}
    }
internal static LegacyEnglishCaseInitialQueryPass PrepareNode1Pass(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,
        LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,long maximumLogicalBytes=128L*1024*1024,Action<string>? observer=null,CancellationToken token=default)
    {
        var pool=LegacyEnglishCaseExecutionPoolReceipt.Preflight(knowledge,numeric,maximumLogicalBytes,token);
        var initializer=LegacyEnglishCaseQueryInitialization.PrepareExecution(knowledge,numeric,cases,publication,pool,observer,token);var result=new LegacyEnglishCaseInitialQueryPass(initializer);bool claimed=false;
        try
        {
            result.Root.ClaimInitialQueryOwner(result,token);claimed=true;observer?.Invoke("execution:claimed");token.ThrowIfCancellationRequested();
            result.Scorer=LegacyEnglishFreshCaseMainScorer.Prepare(result.Root,token);observer?.Invoke("execution:scorer");token.ThrowIfCancellationRequested();
            result.SpanSession=LegacyEnglishFreshCaseMainSpanSession.Prepare(result.Scorer,new(2,knowledge.Owner.CandidateCount),token);
            observer?.Invoke("execution:prepared");token.ThrowIfCancellationRequested();return result;
        }
        catch{if(claimed){Volatile.Write(ref result.phase,3);result.Root.PoisonInitializationOwner(initializer);}throw;}
    }
}
