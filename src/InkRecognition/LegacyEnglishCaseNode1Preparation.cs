namespace InkRecognition;
// Opt-in node1 source entry only. OLD preparation paths remain byte-for-byte unchanged.
internal sealed partial class LegacyEnglishCaseNode0Completion
{
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
        var primary=LegacyEnglishCasePrimaryCompletion.PrepareNode1(knowledge,numeric,cases,publication,maximumLogicalBytes,token);
        try
        {
            ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,264+48);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,264+8);
            var result=new LegacyEnglishCaseNode0Completion(primary,ledger,index,fresh);token.ThrowIfCancellationRequested();primary.AttachNode0Completion(result);return result;
        }
        catch{primary.FaultUnattachedNode0Preparation();throw;}
    }
}
internal sealed partial class LegacyEnglishCasePrimaryCompletion
{
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
