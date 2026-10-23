using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglishCaseInitialQuery.Probe")]
namespace InkRecognition;

//Only the first true2144F0 prefix. No public request, graph, map or flags ingress.
internal sealed partial class LegacyEnglishCaseInitialQueryPass
{
    internal const ulong InitialTag=0x0000080000000000UL;
    private int phase; //0prepared,1executing,2complete,3failed
    private readonly List<LegacyEnglishCaseInitialQueryRecord> pages=new(5);
    private readonly object pageGate=new();
    private readonly LegacyNativeDelimiterQuery query;
    private LegacyEnglishFreshCaseMainSource.InitialQueryLease? activeQueryLease;
    internal LegacyEnglishCaseQueryInitialization Initializer{get;}
    internal LegacyEnglishFreshCaseMainScorer Scorer{get;private set;}=null!;
    internal LegacyEnglishFreshCaseMainSpanSession SpanSession{get;private set;}=null!;
    internal LegacyEnglishFreshCaseMainSpanSession.Receipt? D0{get;private set;}
    internal LegacyEnglishFreshCaseMainSource Root=>Initializer.RootForAudit;
    internal bool IsFaulted=>Volatile.Read(ref phase)==3||Root.IsFaulted;
    internal int EnteredQueries{get;private set;}
    internal int ConsumedRows{get;private set;}
    internal int ResetCount{get;private set;}
    internal int ResetAttempts{get;private set;}
    internal int ResetCompletedStages{get;private set;}
    internal LegacyEnglishCaseInitialQueryRecord[] CopyCompletedPages(){lock(pageGate)return pages.ToArray();}
    internal long LogicalCeiling=>LegacyEnglishCaseExecutionPoolReceipt.CompleteLogicalCeiling;
    private LegacyEnglishCaseInitialQueryPass(LegacyEnglishCaseQueryInitialization initializer)
    {
        Initializer=initializer;query=new(){SourceTag=InitialTag,Request=new(){Node=0,Skip=0,Capacity=128,ContextRoot=initializer.PlanForAudit.BindingForAudit.Contexts.ContainerIdentity,
            ParentContext=0,Parameter1c=0,Parameter20=0,Flags24=0,Flags26=0,Category18=0,Parameter19=0}};
    }
    internal static LegacyEnglishCaseInitialQueryPass Prepare(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,
        LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,long maximumLogicalBytes=128L*1024*1024,Action<string>? observer=null,CancellationToken token=default)
    {
        var pool=LegacyEnglishCaseExecutionPoolReceipt.Preflight(knowledge,numeric,maximumLogicalBytes,token);
        var initializer=LegacyEnglishCaseQueryInitialization.PrepareExecution(knowledge,numeric,cases,publication,pool,observer,token);var result=new LegacyEnglishCaseInitialQueryPass(initializer);bool claimed=false;
        try
        {
            result.Root.ClaimInitialQueryOwner(result,token);claimed=true;observer?.Invoke("execution:claimed");token.ThrowIfCancellationRequested();
            result.Scorer=LegacyEnglishFreshCaseMainScorer.Prepare(result.Root,token);observer?.Invoke("execution:scorer");token.ThrowIfCancellationRequested();
            result.SpanSession=LegacyEnglishFreshCaseMainSpanSession.Prepare(result.Scorer,new(1,knowledge.Owner.CandidateCount),token);
            observer?.Invoke("execution:prepared");token.ThrowIfCancellationRequested();return result;
        }
        catch{if(claimed){Volatile.Write(ref result.phase,3);result.Root.PoisonInitializationOwner(initializer);}throw;}
    }
    internal LegacyEnglishCaseInitialQueryRecord[] Run(Action<string>? observer=null,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if(Interlocked.CompareExchange(ref phase,1,0)!=0)throw new InvalidOperationException("One first native query execution required.");
        LegacyEnglishFreshCaseMainSource.InitialQueryLease? lease=null;bool entered=false;
        try
        {
            //Native sourceV80(-1) happens BEFORE first D0; serialized separately.
            var reset=Root.BeginInitializationAccess(Initializer,token);entered=true;
            try{ResetAttempts++;Initializer.PlanForAudit.ResetFirstExecutionNode(stage=>{ResetCompletedStages++;observer?.Invoke(stage);},token);ResetCount++;observer?.Invoke("query:reset");token.ThrowIfCancellationRequested();Root.CompleteInitializationAccess(reset);}
            catch{Root.PoisonInitializationAccess(reset);throw;}
            observer?.Invoke("query:before-D0");token.ThrowIfCancellationRequested();
            D0=SpanSession.PrepareSpanCore(0,0,false,observer,token);observer?.Invoke("query:D0");token.ThrowIfCancellationRequested();
            lease=Root.BeginInitialQuery(this,SpanSession,D0,token);activeQueryLease=lease;EnteredQueries++;observer?.Invoke("query:map-installed");token.ThrowIfCancellationRequested();
            while(true)
            {
                Root.ValidateInitialQuery(lease);ValidateIssuer(query);if(pages.Count==5)throw new InvalidDataException("Whole five-page first-query bound exceeded.");
                observer?.Invoke("query:before-read");token.ThrowIfCancellationRequested();
                var page=Initializer.PlanForAudit.ReadInitialExecutionPage(lease,query,token);
                if(page.ReturnedCount is<0 or>128||page.Records.Length!=page.ReturnedCount*32||page.ReturnedCount>400-ConsumedRows)throw new InvalidDataException("Complete source-bound first-query page required.");
                var completed=LegacyEnglishCaseInitialQueryRecord.Issue(this,lease,query.Request.Skip,page);
                lock(pageGate)pages.Add(completed);
                initialConsumer?.Consume(lease,completed,token);
                ConsumedRows+=page.ReturnedCount;query.Request.Skip=checked(query.Request.Skip+page.ReturnedCount);
                if(Initializer.PlanForAudit.BindingForAudit.Contexts.NodeCount>(initialConsumer?.MaximumContextNodes??889))throw new InvalidDataException("Whole query context growth bound exceeded.");
                observer?.Invoke("query:page");token.ThrowIfCancellationRequested();if(page.ReturnedCount==0)break;
            }
            initialConsumer?.CompletePrefix(lease,token);
            var output=CopyCompletedPages();observer?.Invoke("query:publish");token.ThrowIfCancellationRequested();Root.CompleteInitialQuery(lease);lease=null;Volatile.Write(ref phase,2);return output;
        }
        catch{Volatile.Write(ref phase,entered?3:0);if(lease is not null)Root.PoisonInitialQuery(lease);else if(entered)Root.FaultInitialExecutionOwner(this);throw;}
        finally{activeQueryLease=null;}
    }
    private void ValidateIssuer(LegacyNativeDelimiterQuery value)
    {
        ArgumentNullException.ThrowIfNull(value);ArgumentNullException.ThrowIfNull(value.Request);
        var q=value.Request;if(!ReferenceEquals(value,query)||value.SourceTag!=InitialTag||q.Node!=0||q.ContextRoot!=Initializer.PlanForAudit.BindingForAudit.Contexts.ContainerIdentity||q.ParentContext!=0||q.Capacity!=128||q.Parameter1c!=0||q.Parameter20!=0||q.Flags24!=0||q.Flags26!=0||q.Category18!=0||q.Parameter19!=0||q.Skip!=ConsumedRows)
            throw new InvalidOperationException("Exact privately issued nonzero first-query state required.");
    }
    internal void RequireIssuedForAudit(LegacyNativeDelimiterQuery other)=>ValidateIssuer(other);
    internal LegacyNativeDelimiterQueryPage ReadUnissuedFrameForAudit(LegacyNativeDelimiterQuery other,CancellationToken token=default)
        =>Initializer.PlanForAudit.ReadInitialExecutionPage(activeQueryLease??throw new InvalidOperationException("No actual active first-query lease."),other,token);
}
