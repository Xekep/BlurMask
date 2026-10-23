using System.Runtime.CompilerServices;
using InkRecognition;
internal static class OwnerGuards
{
    internal static object Run(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,
        LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,LegacyEnglishCasePrimaryCompletion completed,
        LegacyEnglishCasePrimaryCompletionReceipt receipt,LegacyEnglishCasePrimaryCompletionSnapshot snapshot)
    {
        var names=new List<string>();var cancellations=new List<string>();
        LegacyEnglishCasePrimaryCompletion Fresh()=>LegacyEnglishCasePrimaryCompletion.Prepare(knowledge,numeric,cases,publication);
        void Assert(bool ok,string reason){if(!ok)throw new Exception(reason);}
        Exception Guard(string name,Action action)
        {
            try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or InvalidDataException or NotSupportedException or OperationCanceledException)
            {names.Add(name);return e;}
            throw new Exception("Expected primary-owner refusal: "+name);
        }
        Guard("budget-low",()=>LegacyEnglishCasePrimaryCompletion.Prepare(knowledge,numeric,cases,publication,127687944));
        Guard("budget-high",()=>LegacyEnglishCasePrimaryCompletion.Prepare(knowledge,numeric,cases,publication,134217729));
        Guard("one-owner-run",()=>completed.Run());Guard("one-receipt-export",()=>receipt.Export());
        if(snapshot.Rows88.Length>0){byte old=Rows(receipt)[0];snapshot.Rows88[0]^=255;Assert(Rows(receipt)[0]==old,"Export exposed private record bytes.");snapshot.Rows88[0]^=255;}
        var heap=Heap(Consumer(completed));long attempts=heap.CopyHistorical().Attempts;
        var consumed=Guard("mutation-after-transfer",()=>heap.Insert(null!,default));
        Assert(consumed is InvalidOperationException&&consumed.Message.Contains("consumed")&&heap.CopyHistorical().Attempts==attempts,"Transfer did not consume exact heap mutation authority.");
        var retry=Fresh();var rp=Pass(Consumer(retry));using(var cancel=new CancellationTokenSource())
        {cancel.Cancel();var e=Guard("preentry-cancel",()=>retry.Run(token:cancel.Token));Assert(e is OperationCanceledException&&rp.ResetAttempts==0&&!rp.IsFaulted,"Preentry cancellation consumed source or had another cause.");}
        _=retry.Run();
        foreach(string stage in new[]{"query:reset-normal","query:reset-auxiliary","query:reset-backend","query:reset","query:before-D0","span:entered","source:bytes","source:expanded","source:refreshed","source:forwarded","span:installed","query:D0","query:map-installed","query:before-read","query:page","query:publish"})
        {
            var fresh=Fresh();var pass=Pass(Consumer(fresh));using var cancel=new CancellationTokenSource();
            bool reached=false;
            var e=Guard("cancel:"+stage,()=>fresh.Run(s=>{if(s==stage){reached=true;cancel.Cancel();}},cancel.Token));
            Assert(reached&&e is OperationCanceledException,"Requested cancellation stage was not reached or failed for another cause: "+stage);
            Assert(pass.IsFaulted&&Pending(fresh) is null,"Entered cancellation published pending bytes or lost source fault: "+stage);
            Guard("canceled-retry:"+stage,()=>fresh.Run());cancellations.Add(stage);
        }
        var fault=Fresh();var fp=Pass(Consumer(fault));
        bool publishReached=false;
        var publishError=Guard("publish-fault",()=>fault.Run(s=>{if(s=="query:publish"){publishReached=true;throw new InvalidDataException("Test-only final publication failure.");}}));
        Assert(publishReached&&publishError is InvalidDataException&&publishError.Message=="Test-only final publication failure.","Requested publish fault was not reached or failed for another cause.");
        Assert(fp.IsFaulted&&Pending(fault) is null,"Final failure retained pending receipt.");
        var running=Fresh();var actual=Consumer(running);var actualPass=Pass(actual);bool checkedActive=false;
        LegacyEnglishFreshCaseMainSource.InitialQueryLease? stale=null;
        _=running.Run(stage=>
        {
            if(stage!="query:page"||checkedActive)return;checkedActive=true;
            var lease=Lease(actualPass)??throw new Exception("Actual active lease missing.");stale=lease;
            var policy=Policy(actual);var actualHeap=Heap(actual);
            Guard("outside-completion-seam",()=>running.CompleteOwned(actual,actualHeap,policy,lease,default));
            Guard("outside-transfer-seam",()=>actualHeap.TransferPrimary(running,actual,policy,lease,default));
            var forged=LegacyEnglishFreshCaseMainSource.InitialQueryLease.Issue(lease.Root,lease.Owner,lease.Epoch,lease.Map);
            Guard("same-byte-foreign-lease",()=>running.CompleteOwned(actual,actualHeap,policy,forged,default));
            var foreign=Fresh();Guard("foreign-consumer",()=>running.CompleteOwned(Consumer(foreign),actualHeap,policy,lease,default));
            Guard("foreign-heap",()=>running.CompleteOwned(actual,new LegacyEnglishCaseInitialHeap(policy),policy,lease,default));
            Assert(!actualPass.IsFaulted&&Entered(running)==0,"Preentry guard damaged an active owner.");
        });
        Assert(checkedActive,"Active seam guards were not reached.");
        Guard("stale-closed-lease",()=>running.CompleteOwned(actual,Heap(actual),Policy(actual),stale!,default));
        var foreignOwner=Fresh();var unattached=LegacyEnglishCaseInitialQueryPass.PreparePrimaryConsumer(knowledge,numeric,cases,publication);
        Guard("foreign-prepared-attachment",()=>unattached.AttachPrimaryCompletion(foreignOwner));
        Assert(unattached.Run().Count>=0,"Foreign attachment damaged the original unattached consumer.");
        return JsonOutput.D(("Names",names),("Cancellations",cancellations),("Count",names.Count),("CancellationCount",cancellations.Count));
    }
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="consumer")]private static extern ref LegacyEnglishCaseInitialConsumer Consumer(LegacyEnglishCasePrimaryCompletion owner);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="pass")]private static extern ref LegacyEnglishCaseInitialQueryPass Pass(LegacyEnglishCaseInitialConsumer consumer);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="heap")]private static extern ref LegacyEnglishCaseInitialHeap Heap(LegacyEnglishCaseInitialConsumer consumer);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="policy")]private static extern ref LegacyEnglishCaseInitialSourcePolicy Policy(LegacyEnglishCaseInitialConsumer consumer);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="activeQueryLease")]private static extern ref LegacyEnglishFreshCaseMainSource.InitialQueryLease? Lease(LegacyEnglishCaseInitialQueryPass pass);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="pending")]private static extern ref LegacyEnglishCasePrimaryCompletionReceipt? Pending(LegacyEnglishCasePrimaryCompletion owner);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="entered")]private static extern ref int Entered(LegacyEnglishCasePrimaryCompletion owner);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="rows")]private static extern ref byte[] Rows(LegacyEnglishCasePrimaryCompletionReceipt receipt);
}
