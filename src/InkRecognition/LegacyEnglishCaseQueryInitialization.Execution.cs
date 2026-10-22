namespace InkRecognition;

public sealed partial class LegacyEnglishCaseQueryInitialization
{
    internal static LegacyEnglishCaseQueryInitialization PrepareExecution(LegacyEnglish119MainKnowledge knowledge,
        LegacyEnglishLkFreshNumericBinding numeric,LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,
        LegacyEnglishCaseExecutionPoolReceipt pool,Action<string>? observer,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(cases);ArgumentNullException.ThrowIfNull(publication);ArgumentNullException.ThrowIfNull(pool);token.ThrowIfCancellationRequested();
        var result=new LegacyEnglishCaseQueryInitialization();
        var p=LegacyEnglishCaseInitializationPlan.IssueExecution(result,knowledge,numeric,cases,publication,pool,observer);
        try
        {
            p.Check("init:preflight",token);var root=LegacyEnglishFreshCaseMainSource.PrepareCore(knowledge,numeric,cases,publication,stage=>p.Check("root:"+stage,token),token,p);
            p.Publish(root,token);return result;
        }
        catch{p.FailBeforePublication();throw;}
    }
}

internal sealed partial class LegacyEnglishCaseInitializationPlan
{
    private readonly LegacyEnglishCaseExecutionPoolReceipt? executionPool;
    private int executionReset;
    private LegacyEnglishCaseInitializationPlan(LegacyEnglishCaseQueryInitialization o,LegacyEnglish119MainKnowledge k,
        LegacyEnglishLkFreshNumericBinding n,LegacyNativeCaseData c,LegacyNativeTextPublicationData p,LegacyEnglishCaseExecutionPoolReceipt pool,Action<string>? callback)
        :this(o,k,n,c,p,callback is null?null:(_,stage)=>callback(stage))
    {pool.Claim(this,k,n);executionPool=pool;}
    internal static LegacyEnglishCaseInitializationPlan IssueExecution(LegacyEnglishCaseQueryInitialization o,LegacyEnglish119MainKnowledge k,
        LegacyEnglishLkFreshNumericBinding n,LegacyNativeCaseData c,LegacyNativeTextPublicationData p,LegacyEnglishCaseExecutionPoolReceipt pool,Action<string>? callback)
    {var result=new LegacyEnglishCaseInitializationPlan(o,k,n,c,p,pool,callback);o.AttachPlan(result);return result;}
    internal void ResetFirstExecutionNode(Action<string>? observer,CancellationToken token)
    {
        RequirePublished(token);if(executionPool is null||Interlocked.CompareExchange(ref executionReset,1,0)!=0)throw new InvalidOperationException("One actual before-node(-1) execution epoch required.");
        //2372F0: all outer normal slots, single auxiliary, then28D->292->2B7.
        normal!.ResetForControllerNode(token);observer?.Invoke("query:reset-normal");token.ThrowIfCancellationRequested();
        auxiliary!.ResetForControllerNode(token);observer?.Invoke("query:reset-auxiliary");token.ThrowIfCancellationRequested();
        aggregate!.PrepareControllerNode(token);observer?.Invoke("query:reset-backend");token.ThrowIfCancellationRequested();
    }
    internal LegacyNativeDelimiterQueryPage ReadInitialExecutionPage(LegacyEnglishFreshCaseMainSource.InitialQueryLease lease,LegacyNativeDelimiterQuery query,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(lease);ArgumentNullException.ThrowIfNull(query);
        if(executionPool is null||Volatile.Read(ref phase)!=5||Volatile.Read(ref executionReset)!=1)throw new InvalidOperationException("Actual initialized reset execution source required.");
        if(!ReferenceEquals(lease.Root,BoundRoot)||!ReferenceEquals(lease.Owner.Initializer,owner))throw new InvalidOperationException("Exact initialized execution owner required.");
        lease.Root.ValidateInitialQuery(lease);lease.Owner.RequireIssuedForAudit(query);
        ValidateParent(token);return normal!.Read(query,token);
    }
}
