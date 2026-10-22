namespace InkRecognition;

public sealed partial class LegacyEnglishFreshCaseMainSource
{
    private LegacyEnglishCaseInitialQueryPass? queryOwner;
    private InitialQueryLease? activeQuery;
    private bool firstQueryEntered;
    internal void ClaimInitialQueryOwner(LegacyEnglishCaseInitialQueryPass owner,CancellationToken token)
    {
        RequireCurrent();token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();if(faulted||busy||queryOwner is not null||scorer is not null||spanOwner is not null||version!=0||entered!=0||!ReferenceEquals(initializationOwner,owner.Initializer))
                throw new InvalidOperationException("Private pristine initialized first-query source required.");
            queryOwner=owner;
        }
    }
    internal InitialQueryLease BeginInitialQuery(LegacyEnglishCaseInitialQueryPass owner,LegacyEnglishFreshCaseMainSpanSession actualSpan,
        LegacyEnglishFreshCaseMainSpanSession.Receipt receipt,CancellationToken token)
    {
        RequireCurrent();token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();
            //Observational currentness must be checked BEFORE our own busy bit.
            if(faulted||busy||firstQueryEntered||!ReferenceEquals(queryOwner,owner)||!ReferenceEquals(spanOwner,actualSpan)||!actualSpan.IsCurrent(receipt)||receipt.Anchor!=0||receipt.Span!=0||receipt.Epoch!=version)
                throw new InvalidOperationException("Exact current private first-D0 receipt required.");
            var dictionary=owner.Initializer.PlanForAudit.BindingForAudit.Dictionary;
            var lease=InitialQueryLease.Issue(this,owner,version,working);
            dictionary.SetWorkingMap(working); //Exact map; NO second source68 refresh.
            firstQueryEntered=true;activeQuery=lease;busy=true;return lease;
        }
    }
    internal void ValidateInitialQuery(InitialQueryLease lease)
    {
        lock(gate)if(faulted||!busy||!ReferenceEquals(activeQuery,lease)||!ReferenceEquals(lease.Root,this)||!ReferenceEquals(queryOwner,lease.Owner)||lease.Epoch!=version||!ReferenceEquals(lease.Map,working))
            throw new InvalidOperationException("Exact active first-query lease/version/map required.");
    }
    internal void CompleteInitialQuery(InitialQueryLease lease)
    {lock(gate){ValidateInitialQuery(lease);activeQuery=null;busy=false;}}
    internal void PoisonInitialQuery(InitialQueryLease lease)
    {lock(gate){if(ReferenceEquals(activeQuery,lease)&&ReferenceEquals(lease.Root,this)&&ReferenceEquals(queryOwner,lease.Owner)){faulted=true;activeQuery=null;busy=false;}}}
    internal void FaultInitialExecutionOwner(LegacyEnglishCaseInitialQueryPass owner)
    {
        //The local executor is permanently failed after its consumed reset/D0.
        //A rejected next-stage entry cannot fault or clear another active lease.
        lock(gate){if(ReferenceEquals(queryOwner,owner)&&!busy)faulted=true;}
    }
    internal sealed class InitialQueryLease
    {
        private InitialQueryLease(LegacyEnglishFreshCaseMainSource root,LegacyEnglishCaseInitialQueryPass owner,long epoch,LegacyPreparedDictionaryWorkingMap map){Root=root;Owner=owner;Epoch=epoch;Map=map;}
        internal static InitialQueryLease Issue(LegacyEnglishFreshCaseMainSource root,LegacyEnglishCaseInitialQueryPass owner,long epoch,LegacyPreparedDictionaryWorkingMap map)=>new(root,owner,epoch,map);
        internal LegacyEnglishFreshCaseMainSource Root{get;}
        internal LegacyEnglishCaseInitialQueryPass Owner{get;}
        internal long Epoch{get;}
        internal LegacyPreparedDictionaryWorkingMap Map{get;}
    }
}
