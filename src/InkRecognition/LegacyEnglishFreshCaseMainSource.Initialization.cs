namespace InkRecognition;

public sealed partial class LegacyEnglishFreshCaseMainSource
{
    private LegacyEnglishCaseQueryInitialization? initializationOwner;
    private InitializationLease? activeInitialization;
    private long initializationEpoch;
    internal void ClaimInitialization(LegacyEnglishCaseQueryInitialization owner,LegacyEnglishCaseInitializationBindings binding,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(owner);ArgumentNullException.ThrowIfNull(binding);binding.RequireRootBinding(owner,this);RequireCurrent();token.ThrowIfCancellationRequested();
        lock(gate)
        {
            token.ThrowIfCancellationRequested();if(!ReferenceEquals(contexts,binding.Contexts)||!ReferenceEquals(maps.Contexts,binding.Contexts)||faulted||busy||initializationOwner is not null||scorer is not null||spanOwner is not null||activeScore is not null||activeD0 is not null||version!=0||entered!=0||selected!=0)
                throw new InvalidOperationException("Exact pristine private initialization root required.");
            //Owned O(1) setter; no reader has run and no callbacks/gates occur.
            binding.Dictionary.SetWorkingMap(working);
            initializationOwner=owner;
        }
    }
    internal InitializationLease BeginInitializationAccess(LegacyEnglishCaseQueryInitialization owner,CancellationToken token)
    {
        RequireCurrent();token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();if(faulted||busy||activeD0 is not null||activeScore is not null||activeInitialization is not null||!ReferenceEquals(initializationOwner,owner))
                throw new InvalidOperationException("Serialized exact initialization owner required.");
            long next=checked(initializationEpoch+1);var lease=InitializationLease.Issue(this,owner,next);initializationEpoch=next;activeInitialization=lease;busy=true;return lease;
        }
    }
    private void ValidateInitializationAccess(InitializationLease lease)
    {
        if(faulted||!busy||!ReferenceEquals(activeInitialization,lease)||!ReferenceEquals(lease.Root,this)||!ReferenceEquals(initializationOwner,lease.Owner)||lease.Epoch!=initializationEpoch)
            throw new InvalidOperationException("Exact active opaque initialization inspection lease required.");
    }
    internal void CompleteInitializationAccess(InitializationLease lease)
    {lock(gate){ValidateInitializationAccess(lease);activeInitialization=null;busy=false;}}
    internal void PoisonInitializationAccess(InitializationLease lease)
    {
        lock(gate)
        {
            //Rejected or stale leases must not clear/poison another active operation.
            if(ReferenceEquals(activeInitialization,lease)&&ReferenceEquals(lease.Root,this)&&ReferenceEquals(initializationOwner,lease.Owner)&&lease.Epoch==initializationEpoch)
            {faulted=true;activeInitialization=null;busy=false;}
        }
    }
    internal void PoisonInitializationOwner(LegacyEnglishCaseQueryInitialization owner)
    {lock(gate){if(ReferenceEquals(initializationOwner,owner)){faulted=true;if(activeInitialization is not null){activeInitialization=null;busy=false;}}}}
    internal sealed class InitializationLease
    {
        private InitializationLease(LegacyEnglishFreshCaseMainSource root,LegacyEnglishCaseQueryInitialization owner,long epoch){Root=root;Owner=owner;Epoch=epoch;}
        internal static InitializationLease Issue(LegacyEnglishFreshCaseMainSource root,LegacyEnglishCaseQueryInitialization owner,long epoch)=>new(root,owner,epoch);
        internal LegacyEnglishFreshCaseMainSource Root{get;}
        internal LegacyEnglishCaseQueryInitialization Owner{get;}
        internal long Epoch{get;}
    }
}
