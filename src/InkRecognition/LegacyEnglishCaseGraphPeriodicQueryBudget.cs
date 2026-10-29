namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private GraphPeriodicQueryScope? graphPeriodicQueryScope;
    internal sealed class GraphPeriodicQueryScope:ILegacyEnglishPeriodicCaseWrapperProbeBudget
    {
        private readonly LegacyEnglishCaseNode0Completion owner;
        private readonly LegacySymbolContextFactory contexts;
        private readonly LegacyPreparedDictionaryQuery?[] requests;
        private readonly LegacyNativeCaseNormalQuery?[] caseWrappers;
        private int wrapperCount;
        private readonly LegacyEnglishPeriodicCaseWrapperIndex wrapperIndex;
        private readonly LegacyEnglishPeriodicLastWrapperIndex lastWrapperIndex;
        private readonly LegacyEnglishPeriodicRequestMembershipIndex membershipIndex;
        private int count,state;
        private GraphPeriodicQueryScope(LegacyEnglishCaseNode0Completion owner,LegacySymbolContextFactory contexts)
        {this.owner=owner;this.contexts=contexts;requests=new LegacyPreparedDictionaryQuery?[80];caseWrappers=new LegacyNativeCaseNormalQuery?[64];wrapperIndex=new();lastWrapperIndex=new();membershipIndex=new();}
        internal static GraphPeriodicQueryScope Issue(LegacyEnglishCaseNode0Completion owner,LegacySymbolContextFactory contexts)
        {
            owner.graphPeriodicContextBudget!.Require(contexts);if(owner.graphPeriodicQueryScope is not null)throw new InvalidOperationException("One actual query ledger scope required.");
            owner.ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,4116);owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,4684);
            var value=new GraphPeriodicQueryScope(owner,contexts);owner.graphPeriodicQueryScope=value;return value;
        }
        private void Require()
        {owner.graphPeriodicContextBudget!.Require(contexts);if(state!=1||!ReferenceEquals(owner.graphPeriodicQueryScope,this))throw new InvalidOperationException("Same synchronous periodic normal-query scope required.");}
        internal void Begin(LegacyNativeDelimiterQuerySource actual,LegacyPreparedDictionaryQuery request,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();owner.graphPeriodicContextBudget!.Require(contexts);
            owner.node1Pass!.Initializer.PlanForAudit.RequirePeriodicNormal(owner,actual,token);
            if(state!=0||count!=0)throw new InvalidOperationException("Nested/reused actual query scope refused.");state=1;
            try{Copy(2336);Enroll(request);}catch{End();throw;}
        }
        private void Enroll(LegacyPreparedDictionaryQuery request)
        {
            Require();Visit(64);int found=membershipIndex.Find(request,requests,count,out int free,this);if(found>=0)return;
            if(count==requests.Length||request.PeriodicQuery is not null)throw new InvalidOperationException("Actual fixed query propagation extent exceeded or foreign active request.");
            Copy(28);requests[count]=request;membershipIndex.Publish(free,count);count++;request.PeriodicQuery=this;
        }
        internal void Propagate(LegacyPreparedDictionaryQuery parent,LegacyPreparedDictionaryQuery child)
        {Require();if(!ReferenceEquals(parent.PeriodicQuery,this))throw new InvalidOperationException("Actual entered query parent required.");Enroll(child);}
        internal LegacyNativeCaseNormalQuery CaseWrapper(LegacyPreparedDictionaryQuery request)
        {
#if ENGLISH_FRESH_CONTINUATION_CONTROLS
            LegacyFreshContinuationTestHooks.WrapperObserved?.Invoke(request);
#endif
            Require();if(!ReferenceEquals(request.PeriodicQuery,this))throw new InvalidOperationException("Actual propagated case-wrapper request required.");
            //Before reading last-cache metadata, count or any array element.
            //The32-unit precharge also covers two eventual Remember stores.
            owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.PageCopies,32);
            int found=lastWrapperIndex.Find(request,caseWrappers,wrapperCount);if(found>=0)return caseWrappers[found]!;
            Visit(64); // Unchanged cold lookup/hash metadata.
            found=wrapperIndex.Find(request,caseWrappers,wrapperCount,out int free,this);
            if(found>=0){owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.PageCopies,12);lastWrapperIndex.Remember(request,found);return caseWrappers[found]!;}
            // Actual normal V80 replaces64 Level.Query identities and case V80 clears its eight slots. This cache is reset at that actual boundary below.
            if(wrapperCount==caseWrappers.Length)throw new InvalidOperationException("Actual native normal8x8 request-wrapper extent exceeded.");
            Allocate(96);Copy(100);var value=new LegacyNativeCaseNormalQuery{Request=request};caseWrappers[wrapperCount]=value;wrapperIndex.Publish(free,wrapperCount);wrapperCount++;
            owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.PageCopies,12);lastWrapperIndex.Remember(request,wrapperCount-1);return value;
        }
        internal void BeforeActualNormalReset(LegacyNativeDelimiterQuerySource actual,CancellationToken token)
        {
#if ENGLISH_FRESH_CONTINUATION_CONTROLS
            LegacyFreshContinuationTestHooks.WrapperObserved?.Invoke(null);
#endif
            owner.graphPeriodicContextBudget!.Require(contexts);owner.node1Pass!.Initializer.PlanForAudit.RequirePeriodicNormal(owner,actual,token);
            if(state!=0||count!=0)throw new InvalidOperationException("Actual normal sourceV80 requires all synchronous query frames closed.");
            owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Reset,1044);owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Reset,512);
            for(int i=0;i<wrapperCount;i++)caseWrappers[i]=null;wrapperCount=0;wrapperIndex.Clear();lastWrapperIndex.Clear();
        }
        // Closed synchronous index loop only: outer CaseWrapper retained both
        // original Require checks; no model/source/user callbacks can intervene.
        void ILegacyEnglishPeriodicCaseWrapperProbeBudget.BeforeProbe()
            =>owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.PageCopies,32);
        internal void Allocate(long bytes){Require();owner.ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.PageCopies,bytes);}
        internal void Copy(long bytes){Require();owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.PageCopies,bytes);}
        internal void Visit(long units){Require();owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.PageCopies,units);}
        internal void End()
        {
            // Cleanup must execute even after a quota exception. Its bounded writes were reserved by Issue/preparation.
            for(int i=0;i<count;i++){var q=requests[i]!;if(ReferenceEquals(q.PeriodicQuery,this))q.PeriodicQuery=null;requests[i]=null;}if(count!=0)membershipIndex.Clear();count=0;state=0;
        }
        internal void Close(){End();for(int i=0;i<wrapperCount;i++)caseWrappers[i]=null;wrapperCount=0;wrapperIndex.Clear();lastWrapperIndex.Clear();state=2;}
    }
}
public sealed partial class LegacySymbolContextFactory
{
    internal void ObservePeriodicNormalReset(LegacyNativeDelimiterQuerySource actual,CancellationToken token)
    {periodicBudget?.BeforeNormalReset(actual,token);}
    internal LegacyEnglishCaseNode0Completion.GraphPeriodicQueryScope? BeginPeriodicNormalQuery(LegacyNativeDelimiterQuerySource actual,LegacyPreparedDictionaryQuery request,CancellationToken token)
    {if(periodicBudget is null)return null;return periodicBudget.BeginQuery(actual,request,token);}
}
internal sealed partial class LegacyEnglishCaseInitializationPlan
{
    internal void RequirePeriodicNormal(LegacyEnglishCaseNode0Completion owner,LegacyNativeDelimiterQuerySource actual,CancellationToken token)
    {token.ThrowIfCancellationRequested();if(executionPool is null||Volatile.Read(ref phase)!=5||Volatile.Read(ref executionReset)!=1||!ReferenceEquals(normal,actual)||BoundRoot is not{} root||!ReferenceEquals(owner,root.Node0ForPeriodicQuery()))throw new InvalidOperationException("Actual selected initialized normal-query owner required.");}
}
public sealed partial class LegacyEnglishFreshCaseMainSource
{
    internal LegacyEnglishCaseNode0Completion? Node0ForPeriodicQuery()=>node1Owner;
}


// Pure bounded identity search; no query enrollment, source/WORD authority or
// source callbacks. Only ordinals, never request/owner handles, are retained.
internal interface ILegacyEnglishPeriodicCaseWrapperProbeBudget{void BeforeProbe();}
internal sealed class LegacyEnglishPeriodicCaseWrapperIndex
{
    private readonly int[] slots=new int[128];
    internal static long LookupVisits(int count)
    {if((uint)count>64)throw new ArgumentOutOfRangeException(nameof(count));return checked(64L+32L*(count+1));}
    internal int Find(LegacyPreparedDictionaryQuery request,ReadOnlySpan<LegacyNativeCaseNormalQuery?> wrappers,int count,out int free,ILegacyEnglishPeriodicCaseWrapperProbeBudget? budget=null)
    {
        ArgumentNullException.ThrowIfNull(request);
        if(wrappers.Length!=64||(uint)count>64)throw new ArgumentOutOfRangeException(nameof(count));
        int at=System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(request)&127;
        // No deletions/tombstones: <=count occupied slots then one empty.
        for(int probe=0;probe<=count;probe++)
        {
            budget?.BeforeProbe(); // BEFORE the actual slot/identity reads; no post-facto debit.
            int encoded=slots[at];
            if(encoded==0){free=at;return -1;}
            int ordinal=encoded-1;
            if((uint)ordinal>=(uint)count||wrappers[ordinal] is not{} wrapper)
                throw new InvalidDataException("Private wrapper-index population changed.");
            if(ReferenceEquals(wrapper.Request,request)){free=-1;return ordinal;}
            at=(at+1)&127;
        }
        throw new InvalidDataException("Private wrapper-index probe bound changed.");
    }
    internal void Publish(int slot,int ordinal)
    {
        if((uint)slot>=128||(uint)ordinal>=64||slots[slot]!=0)throw new InvalidOperationException("One empty wrapper-index slot required.");
        slots[slot]=ordinal+1;
    }
    internal void Clear()=>Array.Clear(slots);
}

// Private enrollment index; integer ordinals only. Cleared at every End.
internal sealed class LegacyEnglishPeriodicRequestMembershipIndex
{
    private readonly int[] slots=new int[256];
    internal int Find(LegacyPreparedDictionaryQuery request,ReadOnlySpan<LegacyPreparedDictionaryQuery?> requests,int count,out int free,ILegacyEnglishPeriodicCaseWrapperProbeBudget? budget=null)
    {
        ArgumentNullException.ThrowIfNull(request);
        if(requests.Length!=80||(uint)count>80)throw new ArgumentOutOfRangeException(nameof(count));
        int at=System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(request)&255;
        for(int probe=0;probe<=count;probe++)
        {
            budget?.BeforeProbe(); //32 logical units BEFORE slot/ordinal/request reads
            int encoded=slots[at];
            if(encoded==0){free=at;return -1;}
            int ordinal=encoded-1;
            if((uint)ordinal>=(uint)count||requests[ordinal] is not{} enrolled)
                throw new InvalidDataException("Private query enrollment population changed.");
            if(ReferenceEquals(enrolled,request)){free=-1;return ordinal;}
            at=(at+1)&255;
        }
        throw new InvalidDataException("Private query enrollment probe bound changed.");
    }
    internal void Publish(int slot,int ordinal)
    {
        if((uint)slot>=256||(uint)ordinal>=80||slots[slot]!=0)throw new InvalidOperationException("One empty enrollment-index slot required.");
        slots[slot]=ordinal+1;
    }
    internal void Clear()=>Array.Clear(slots);
}


//Pure one-entry reference identity memo. No source callbacks, hash calls or
//mutable query fields participate. The caller prepays32 before argument reads.
internal sealed class LegacyEnglishPeriodicLastWrapperIndex
{
    private LegacyPreparedDictionaryQuery? request;
    private int ordinal=-1;
    internal int Find(LegacyPreparedDictionaryQuery actual,ReadOnlySpan<LegacyNativeCaseNormalQuery?> wrappers,int count)
    {
        ArgumentNullException.ThrowIfNull(actual);
        if((uint)count>64||(uint)count>(uint)wrappers.Length)throw new ArgumentOutOfRangeException(nameof(count));
        if(!ReferenceEquals(request,actual))return -1;
        if((uint)ordinal>=(uint)count)throw new InvalidDataException("Corrupt last-wrapper ordinal.");
        var value=wrappers[ordinal];
        if(value is null||!ReferenceEquals(value.Request,actual))throw new InvalidDataException("Corrupt last-wrapper identity.");
        return ordinal;
    }
    //Only a successful real existing-index lookup or committed new slot calls
    //Remember; its two stores are prepaid by the caller's lookup32/copy12.
    internal void Remember(LegacyPreparedDictionaryQuery actual,int index){request=actual;ordinal=index;}
    internal void Clear(){request=null;ordinal=-1;}
}
