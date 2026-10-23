using System.Runtime.CompilerServices;
using InkRecognition;

// SOURCE-ONLY owning controls. No PASS can be emitted unless every requested
// observer is reached and the exact exception/poisoned state is witnessed.
internal static class Node0Guards
{
    internal static readonly string[] CancellationStages=[
        "query:reset-normal","query:reset-auxiliary","query:reset-backend","query:reset","query:before-D0",
        "span:entered","source:bytes","source:expanded","source:refreshed","source:forwarded","span:installed",
        "query:D0","query:map-installed","query:before-read","query:page","query:publish","node0:after-oldRun"];
    internal static object Run(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,
        LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,LegacyEnglishCaseNode0Completion completed,
        LegacyEnglishCaseNode0Completion.DataExport data)
    {
        var names=new List<string>();var canceled=new List<object>();var faults=new List<object>();object? preentry=null;
        LegacyEnglishCaseNode0Completion Fresh()=>LegacyEnglishCaseNode0Completion.Prepare(knowledge,numeric,cases,publication);
        static void Assert(bool value,string message){if(!value)throw new InvalidDataException(message);}
        Exception Refusal(string name,Action action,Type exactType,string? exactMessage=null)
        {
            Exception? error=null;try{action();}catch(Exception e){error=e;}
            Assert(error is not null&&error.GetType()==exactType&&(exactMessage is null||error.Message==exactMessage),"Wrong/missing node0 refusal: "+name);
            names.Add(name);return error!;
        }
        void Poisoned(LegacyEnglishCaseNode0Completion owner,string name)
        {
            Assert(Pass(Consumer(Primary(owner))).IsFaulted&&Phase(owner)==3&&!Pending(owner)&&
                !owner.HasPendingCaptureForProbe&&!owner.HasPendingReceiptForProbe&&Stage(owner) is null&&Thread(owner)==0,
                "Entered failure retained authority/data or lost exact pass fault: "+name);
        }
        Refusal("budget-low",()=>LegacyEnglishCaseNode0Completion.Prepare(knowledge,numeric,cases,publication,LegacyEnglishCaseNode0Ledger.AggregateCeiling-1),typeof(ArgumentOutOfRangeException));
        Refusal("budget-high",()=>LegacyEnglishCaseNode0Completion.Prepare(knowledge,numeric,cases,publication,LegacyEnglishCaseNode0Ledger.MaximumAggregate+1),typeof(ArgumentOutOfRangeException));
        Refusal("one-owner-run",()=>completed.Run(),typeof(InvalidOperationException),"One whole-node draft entry required.");
        var retry=Fresh();var pass=Pass(Consumer(Primary(retry)));
        using(var cts=new CancellationTokenSource())
        {
            cts.Cancel();var error=Refusal("preentry-cancel",()=>retry.Run(token:cts.Token),typeof(OperationCanceledException));
            Assert(pass.ResetAttempts==0&&!pass.IsFaulted&&Phase(retry)==0,"Preentry cancellation claimed/mutated owner.");
            bool tokenMatches=((OperationCanceledException)error).CancellationToken==cts.Token;
            Assert(tokenMatches,"Preentry cancellation exception carried a different token.");
            preentry=JsonOutput.D(("Exception",error.GetType().Name),("TokenMatches",tokenMatches),("ResetAttempts",pass.ResetAttempts),("Faulted",pass.IsFaulted),("Phase",Phase(retry)));
        }
        _=retry.Run();Assert(Phase(retry)==2&&!pass.IsFaulted,"Preentry-canceled fresh owner failed subsequent successful Run.");
        foreach(string stage in CancellationStages)
        {
            var owner=Fresh();using var cts=new CancellationTokenSource();bool reached=false;
            var error=Refusal("cancel:"+stage,()=>owner.Run(s=>{if(s==stage){reached=true;cts.Cancel();}},cts.Token),typeof(OperationCanceledException));
            bool tokenMatches=((OperationCanceledException)error).CancellationToken==cts.Token;
            Assert(reached&&tokenMatches,"Requested node0 cancellation observer/token not witnessed: "+stage);Poisoned(owner,stage);
            Refusal("canceled-retry:"+stage,()=>owner.Run(),typeof(InvalidOperationException),"One whole-node draft entry required.");
            canceled.Add(JsonOutput.D(("Stage",stage),("Reached",reached),("Exception",error.GetType().Name),("TokenMatches",tokenMatches),
                ("Faulted",Pass(Consumer(Primary(owner))).IsFaulted),("Phase",Phase(owner)),("PendingCleared",!Pending(owner)&&!owner.HasPendingCaptureForProbe&&!owner.HasPendingReceiptForProbe)));
        }
        foreach(string stage in new[]{"query:publish","node0:after-oldRun"})
        {
            var owner=Fresh();bool reached=false;string message="Test-only node0 observer fault:"+stage;
            var error=Refusal("observer-fault:"+stage,()=>owner.Run(s=>{if(s==stage){reached=true;throw new InvalidDataException(message);}}),typeof(InvalidDataException),message);
            Assert(reached,"Requested node0 fault observer not reached: "+stage);Poisoned(owner,stage);
            Refusal("faulted-retry:"+stage,()=>owner.Run(),typeof(InvalidOperationException),"One whole-node draft entry required.");
            faults.Add(JsonOutput.D(("Stage",stage),("Reached",reached),("Exception",error.GetType().Name),("Message",message),
                ("Faulted",Pass(Consumer(Primary(owner))).IsFaulted),("Phase",Phase(owner)),("PendingCleared",!Pending(owner)&&!owner.HasPendingCaptureForProbe&&!owner.HasPendingReceiptForProbe)));
        }
        var ring=Ring(completed)??throw new InvalidDataException("Completed private ring missing.");
        byte old=PrivateRows(ring)[0][0],caller=data.Rows[0];data.Rows[0]^=255;
        Assert(PrivateRows(ring)[0][0]==old,"Caller DATA aliases private row backing.");data.Rows[0]=caller;names.Add("detached-row-copy");
        Refusal("one-primary-diagnostic-export",()=>data.ExportPrimaryDiagnostic(),typeof(InvalidOperationException),"One budgeted primary history export required.");
        Assert(names.Count==44&&names.Distinct(StringComparer.Ordinal).Count()==44,"Exact44 ordered unique owning guard names required.");
        return JsonOutput.D(("Names",names),("Count",names.Count),("Preentry",preentry),("PreentryRetryPhase",Phase(retry)),("PreentryRetryFaulted",pass.IsFaulted),
            ("Cancellations",canceled),("CancellationCount",canceled.Count),("Faults",faults),
            ("ExplicitCoverageExclusions",new[]{"mid-sort cancellation","mid-context cancellation","other uninstrumented phase1 mutation windows","cross-thread/race exact-claim controls not yet implemented"}));
    }
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="primary")]private static extern ref LegacyEnglishCasePrimaryCompletion Primary(LegacyEnglishCaseNode0Completion owner);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="consumer")]private static extern ref LegacyEnglishCaseInitialConsumer Consumer(LegacyEnglishCasePrimaryCompletion owner);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="pass")]private static extern ref LegacyEnglishCaseInitialQueryPass Pass(LegacyEnglishCaseInitialConsumer consumer);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="phase")]private static extern ref int Phase(LegacyEnglishCaseNode0Completion owner);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="pending")]private static extern ref bool Pending(LegacyEnglishCaseNode0Completion owner);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="thread")]private static extern ref int Thread(LegacyEnglishCaseNode0Completion owner);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="activeStage")]private static extern ref LegacyEnglishCaseNode0Completion.Stage? Stage(LegacyEnglishCaseNode0Completion owner);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="ring")]private static extern ref LegacyEnglishCaseNode0Ring? Ring(LegacyEnglishCaseNode0Completion owner);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="rows")]private static extern ref byte[][] PrivateRows(LegacyEnglishCaseNode0Ring ring);
}
