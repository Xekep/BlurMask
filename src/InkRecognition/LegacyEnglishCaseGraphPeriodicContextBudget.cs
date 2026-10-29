namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private GraphPeriodicContextBudget? graphPeriodicContextBudget;
    internal sealed class GraphPeriodicContextBudget
    {
        private readonly LegacyEnglishCaseNode0Completion owner;
        private readonly LegacyEnglishCaseInitialQueryPass pass;
        private readonly LegacySymbolContextFactory contexts;
        private readonly int thread;
        private GraphPeriodicContextBudget(LegacyEnglishCaseNode0Completion owner,LegacySymbolContextFactory contexts)
        {this.owner=owner;pass=owner.node1Pass!;this.contexts=contexts;thread=Environment.CurrentManagedThreadId;}
        internal static GraphPeriodicContextBudget Issue(LegacyEnglishCaseNode0Completion owner,LegacySymbolContextFactory contexts,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();if(!owner.graphPeriodicActive||!owner.graphLoopActive||owner.graphPeriodicContextBudget is not null||owner.node1Pass is null||
                !ReferenceEquals(contexts,owner.node1Pass.Initializer.PlanForAudit.BindingForAudit.Contexts))
                throw new InvalidOperationException("Same fresh periodic context budget required.");
            owner.ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,72);owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,72);
            var value=new GraphPeriodicContextBudget(owner,contexts);owner.graphPeriodicContextBudget=value;bool attached=false;try{contexts.AttachPeriodicBudget(value);attached=true;_=GraphPeriodicQueryScope.Issue(owner,contexts);return value;}catch{if(attached)contexts.DetachPeriodicBudget(value);owner.graphPeriodicContextBudget=null;throw;}
        }
        internal void Require(LegacySymbolContextFactory actual)
        {
            if(!owner.graphPeriodicActive||!owner.graphLoopActive||Volatile.Read(ref owner.phase)!=1||owner.thread!=thread||thread!=Environment.CurrentManagedThreadId||
                !ReferenceEquals(owner.graphPeriodicContextBudget,this)||!ReferenceEquals(owner.node1Pass,pass)||!ReferenceEquals(contexts,actual))
                throw new InvalidOperationException("Same actual periodic source context/root/pass/thread required.");
        }
        internal void BeforeNormalReset(LegacyNativeDelimiterQuerySource actual,CancellationToken token)
        {Require(contexts);owner.graphPeriodicQueryScope!.BeforeActualNormalReset(actual,token);}
        internal GraphPeriodicQueryScope BeginQuery(LegacyNativeDelimiterQuerySource actual,LegacyPreparedDictionaryQuery request,CancellationToken token)
        {Require(contexts);var value=owner.graphPeriodicQueryScope??throw new InvalidOperationException("Actual query scope not issued.");value.Begin(actual,request,token);return value;}
        internal void BeforeView(LegacySymbolContextFactory actual)
        {Require(actual);owner.ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,96);owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,96);owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,64);}
        internal void BeforeInsert(LegacySymbolContextFactory actual,int nodes,int dictionaryCapacity,int pages,int pageCapacity)
        {
            Require(actual);owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,64);
            long bytes=128; // New semantic Node and dictionary entry; native page is a logical .NET model, not a4080B allocation.
            if(nodes==dictionaryCapacity)bytes=checked(bytes+4096L+160L*(nodes+1));
            if(pages==pageCapacity)bytes=checked(bytes+256L+16L*(pages+1));
            // Conservative backing growth envelope; no EnsureCapacity mutation or reserve refund.
            owner.ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,bytes);owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,bytes);
        }
        internal int Length(ulong identity,int maximum=50)
        {
            int count=0;while(identity!=0)
            {
                Require(contexts);if(count==maximum)throw new NotSupportedException("Actual context ancestry exceeds the native fifty-symbol frame.");
                owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,16);identity=contexts.ReadPeriodicParentLink(this,identity);count++;
            }
            return count;
        }
        internal void BeforeComposition(ulong suffix)
        {
            int length=Length(suffix,4096);long bytes=checked(512L+160L*length);
            owner.ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,bytes);owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,bytes*2);
            owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,checked(512L+256L*length));
        }
        internal void BeforeFrame(ulong current,ulong previous)
        {
            int length=checked(Length(current)+Length(previous)+2);long bytes=checked(4608L+32L*length);
            owner.ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,bytes);owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,bytes*2);
            owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(8192L+512L*length));
        }
        internal void Close(){contexts.DetachPeriodicBudget(this);}
    }
}
public sealed partial class LegacySymbolContextFactory
{
    internal void AttachPeriodicBudget(LegacyEnglishCaseNode0Completion.GraphPeriodicContextBudget actual)
    {actual.Require(this);if(periodicBudget is not null)throw new InvalidOperationException("One private periodic context budget required.");periodicBudget=actual;}
    internal void DetachPeriodicBudget(LegacyEnglishCaseNode0Completion.GraphPeriodicContextBudget actual)
    {if(!ReferenceEquals(periodicBudget,actual))throw new InvalidOperationException("Same periodic budget required for detach.");periodicBudget=null;}
    internal ulong ReadPeriodicParentLink(LegacyEnglishCaseNode0Completion.GraphPeriodicContextBudget actual,ulong identity)
    {actual.Require(this);if(!ReferenceEquals(periodicBudget,actual))throw new InvalidOperationException("Attached same periodic budget required.");return Get(identity).Parent;}
}
