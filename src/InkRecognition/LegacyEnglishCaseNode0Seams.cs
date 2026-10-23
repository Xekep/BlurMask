namespace InkRecognition;

internal sealed partial class LegacyEnglishCasePrimaryCompletion
{
    private LegacyEnglishCaseNode0Completion? node0;
    internal LegacyEnglishCaseInitializationNumericTables.Node0Handles PrepareNode0NumericHandles(LegacyEnglishCaseNode0Completion owner)
    {
        owner.RequireNumericPreparation(this);
        if(Volatile.Read(ref phase)!=0||node0 is not null)
            throw new InvalidOperationException("Unused prepared primary numeric seam required.");
        return consumer.PrepareNode0NumericHandles(this,owner);
    }
    internal void AttachNode0Completion(LegacyEnglishCaseNode0Completion owner)
    {
        if(Volatile.Read(ref phase)!=0||!owner.IsPreparedFor(this)||Interlocked.CompareExchange(ref node0,owner,null) is not null)
            throw new InvalidOperationException("One exact prepared whole-node attachment required.");
    }
    internal void FaultUnattachedNode0Preparation()
    {
        // Only a locally held exact prepared primary may reach this failure
        // path. No fake full owner is constructed after a constructor/OOM fault.
        if(Volatile.Read(ref phase)!=0||node0 is not null)throw new InvalidOperationException("Unattached prepared primary required for preparation fault.");
        consumer.FaultUnattachedNode0Preparation(this);Volatile.Write(ref phase,3);
    }
    internal void InitializeNode0(LegacyEnglishCaseInitialConsumer actual,LegacyEnglishCaseInitialQueryPass pass,
        LegacyEnglishFreshCaseMainSource.InitializationLease lease,CancellationToken token)
    {node0?.Provision(this,actual,pass,lease,token);}
    internal void RequireNode0Initialization(LegacyEnglishCaseNode0Completion owner,LegacyEnglishCaseInitialConsumer actual,
        LegacyEnglishCaseInitialQueryPass pass,LegacyEnglishFreshCaseMainSource.InitializationLease lease)
    {
        if(!ReferenceEquals(node0,owner)||!owner.IsRunningFor(this)||!ReferenceEquals(consumer,actual)||!IsRunningFor(actual))
            throw new InvalidOperationException("Exact full/primary/consumer running pair required.");
        actual.RequireNode0Initialization(this,pass,lease);
    }
    internal LegacyNativeWordControllerConfiguration ReadNode0InitializationConfiguration(LegacyEnglishCaseNode0Completion owner,
        LegacyEnglishCaseInitialConsumer actual,LegacyEnglishCaseInitialQueryPass pass,LegacyEnglishFreshCaseMainSource.InitializationLease lease)
    {RequireNode0Initialization(owner,actual,pass,lease);return actual.ReadNode0InitializationConfiguration(this,pass,lease);}
    internal void RequireNode0Live(LegacyEnglishCaseNode0Completion owner,LegacyEnglishCaseInitialConsumer actual,LegacyEnglishCaseInitialHeap heap,
        LegacyEnglishCaseInitialSourcePolicy policy,LegacyEnglishFreshCaseMainSource.InitialQueryLease lease)
    {
        if(!ReferenceEquals(node0,owner)||!owner.IsRunningFor(this)||!ReferenceEquals(consumer,actual))throw new InvalidOperationException("Exact full primary owner required.");
        actual.RequirePrimaryCompletion(this,heap,policy,lease);
    }
    internal void FaultNode0Completion(LegacyEnglishCaseNode0Completion owner)
    {
        if(!ReferenceEquals(node0,owner)||!owner.IsRunningFor(this))throw new InvalidOperationException("Exact active full fault owner/thread required.");
        consumer.FaultNode0Completion(this,owner);
    }
    internal void RequireNode0Fault(LegacyEnglishCaseInitialConsumer actual,LegacyEnglishCaseNode0Completion owner)
    {if(!ReferenceEquals(consumer,actual)||!ReferenceEquals(node0,owner)||!owner.IsRunningFor(this))throw new InvalidOperationException("Exact active full fault pair/thread required.");}
}
internal sealed partial class LegacyEnglishCaseInitialConsumer
{
    internal LegacyEnglishCaseInitializationNumericTables.Node0Handles PrepareNode0NumericHandles(
        LegacyEnglishCasePrimaryCompletion primary,LegacyEnglishCaseNode0Completion full)
    {
        full.RequireNumericPreparation(primary);
        if(Volatile.Read(ref run)!=0||!ReferenceEquals(completion,primary)||!primary.IsPreparedFor(this)||issuedLease is not null)
            throw new InvalidOperationException("Exact unused prepared consumer numeric seam required.");
        return LegacyEnglishCaseInitializationNumericTables.Node0Handles.Prepare(pass.Initializer.NumericForAudit,full,primary);
    }
    internal int ReadNode0BeginningPenalty(LegacyEnglishCasePrimaryCompletion owner,LegacyEnglishCaseInitialHeap actualHeap,
        LegacyEnglishCaseInitialSourcePolicy actualPolicy,LegacyEnglishFreshCaseMainSource.InitialQueryLease lease,
        byte storedCategory,CancellationToken token)
    {
        RequirePrimaryCompletion(owner,actualHeap,actualPolicy,lease);frontend.Require(lease,token);
        // Same private frontend profile loaded by the OLD preparation. Borrow
        // its immutable-to-this-owner backing only inside this scalar method;
        // no new mapper, array, ROM, profile or caller-provided view is issued.
        var profile=frontend.Categories;
        ReadOnlySpan<LegacyCursiveCompositeCategory> categories=profile.CompositeCategories.Span;
        int composite=storedCategory&63;
        if(profile.BaseEntryCount!=217||profile.Prototypes.Length!=24||categories.Length!=42||(uint)composite>=(uint)categories.Length)
            throw new NotSupportedException("Exact existing same-source42-composite category required.");
        int first=categories[composite].FirstId;
        if((uint)first>6)throw new InvalidDataException("Actual seven first-list semantic IDs required.");
        token.ThrowIfCancellationRequested();RequirePrimaryCompletion(owner,actualHeap,actualPolicy,lease);
        return first is 2 or 4?8192:0;
    }
    internal void FaultUnattachedNode0Preparation(LegacyEnglishCasePrimaryCompletion owner)
    {
        if(Volatile.Read(ref run)!=0||!ReferenceEquals(completion,owner)||!owner.IsPreparedFor(this)||issuedLease is not null)
            throw new InvalidOperationException("Exact still-prepared private consumer required.");
        pass.Root.FaultInitialExecutionOwner(pass);
    }
    internal void InitializeWholeNode(LegacyEnglishFreshCaseMainSource.InitializationLease lease,CancellationToken token)
    {completion?.InitializeNode0(this,pass,lease,token);}
    internal void RequireNode0Initialization(LegacyEnglishCasePrimaryCompletion owner,LegacyEnglishCaseInitialQueryPass actual,
        LegacyEnglishFreshCaseMainSource.InitializationLease lease)
    {
        if(Volatile.Read(ref run)!=1||!ReferenceEquals(completion,owner)||!ReferenceEquals(pass,actual)||!owner.IsRunningFor(this)||completing||issuedLease is not null)
            throw new InvalidOperationException("Exact before-D0 consumer initialization seam required.");
        pass.Root.ValidateNode0Initialization(pass,lease);
    }
    internal LegacyNativeWordControllerConfiguration ReadNode0InitializationConfiguration(LegacyEnglishCasePrimaryCompletion owner,
        LegacyEnglishCaseInitialQueryPass actual,LegacyEnglishFreshCaseMainSource.InitializationLease lease)
    {
        RequireNode0Initialization(owner,actual,lease);
        if(policy.Heaps.SecondaryShareEc!=0||policy.Heaps.Limits.Count!=1||!ReferenceEquals(policy.Knowledge,pass.Root.Knowledge)||!ReferenceEquals(policy.Numeric,pass.Root.NumericOwner))
            throw new NotSupportedException("Exact same-source one-general fresh heap preparation required.");
        return policy.Heaps.Configuration;
    }
    internal void FaultNode0Completion(LegacyEnglishCasePrimaryCompletion owner,LegacyEnglishCaseNode0Completion full)
    {
        // Primary phase may already be2/3, and no completing lease need remain.
        // Full owner/thread authentication is performed in the private pair.
        if(!ReferenceEquals(completion,owner))throw new InvalidOperationException("Foreign full fault source.");owner.RequireNode0Fault(this,full);
        pass.Root.FaultInitialExecutionOwner(pass);
    }
}
public sealed partial class LegacyEnglishFreshCaseMainSource
{
    internal void ValidateNode0Initialization(LegacyEnglishCaseInitialQueryPass pass,InitializationLease lease)
    {
        lock(gate)
        {
            ValidateInitializationAccess(lease);
            if(!ReferenceEquals(queryOwner,pass)||!ReferenceEquals(initializationOwner,pass.Initializer)||
                !ReferenceEquals(lease.Owner,pass.Initializer)||firstQueryEntered||activeQuery is not null||version!=0)
                throw new InvalidOperationException("Exact active initialization reference before first D0 required.");
        }
    }
}
internal sealed partial class LegacyEnglishCaseInitializationPlan
{
    internal void ResetNode0Completion(LegacyEnglishCaseNode0Completion.Stage stage,CancellationToken token)
    {
        stage.Require();RequirePublished(token);stage.ClaimEndReset(this);stage.ChargeReset();
        if(executionPool is null||Volatile.Read(ref executionReset)!=1)throw new InvalidOperationException("Same completed initial reset required.");
        // Distinct V80(0), never reuse one-shot V80(-1). On this source these
        // methods create scalar query objects/empty arrays only, payload0.
        normal!.ResetForControllerNode(token);token.ThrowIfCancellationRequested();stage.Require();
        auxiliary!.ResetForControllerNode(token);token.ThrowIfCancellationRequested();stage.Require();
        aggregate!.PrepareControllerNode(token);token.ThrowIfCancellationRequested();stage.Require();
    }
}
