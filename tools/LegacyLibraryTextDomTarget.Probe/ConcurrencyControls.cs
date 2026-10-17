using InkRecognition;

//The inference/CHAR/WORD/TEXT owners are the same real handoff as the raw-byte
//oracle. Only scheduling barriers are prepared; no fabricated DOM payloads.
internal static class ConcurrencyControls
{
    internal static void Run(LegacyFreshMainDocumentCounterHandoff handoff)
    {
        LegacyFreshMainDetachedTextDom Builder(LegacyLibraryTextDomTarget target)
            =>LegacyFreshMainDetachedTextDom.PrepareForTarget(LegacyFreshMainSourceRangeUtf8.Prepare(handoff),target);
        LegacyLibraryTextDomTarget Populated()
        {
            var target=LegacyLibraryTextDomTarget.CreateFresh(handoff.Transforms.Seed);
            target.Apply(LegacyLibraryTextDomInsertion.Prepare(Builder(target).Build()).Export());return target;
        }

        //A real previous child is captured, while a competing real edit tries
        //to remove it. The nonblocking monitor check makes this fail with the
        //old gap, without using a sleep as evidence that Apply was blocked.
        var buildTarget=Populated();var before=buildTarget.Root;
        var competingEdit=LegacyLibraryTextDomInsertion.Prepare(Builder(buildTarget).Build()).Export();
        var build=Builder(buildTarget);var staleBuild=Builder(buildTarget);
        var staleInsertion=LegacyLibraryTextDomInsertion.Prepare(Builder(buildTarget).Build());
        LegacyLibraryTextDomApplyResult? applied=null;
        using(var block=new BlockedOperation(buildTarget,()=>applied=buildTarget.Apply(competingEdit)))
        {
            var copied=build.BuildPrepared(afterCapture:block.Attempt);
            block.Join();
            if(!copied.Succeeded||copied.Root.Children.Count==0||before.Children.Count!=1||applied is null||buildTarget.Epoch!=2)
                throw new InvalidDataException("Captured prior child was not safely copied before replacement.");
        }
        Refuse(()=>staleBuild.Build());Refuse(()=>staleInsertion.Export());
        if(staleBuild.HasEnteredPrepared||staleBuild.Arena is not null||staleInsertion.HasEnteredPrepared)
            throw new InvalidDataException("A stale epoch consumed a builder/export before validation.");

        //V50's complete clone phase also owns the target scope, rather than
        //merely checking the epoch and allowing Apply to interleave afterward.
        var exportTarget=Populated();
        var applyEdit=LegacyLibraryTextDomInsertion.Prepare(Builder(exportTarget).Build()).Export();
        var export=LegacyLibraryTextDomInsertion.Prepare(Builder(exportTarget).Build());
        LegacyLibraryTextDomEditResult exported;
        using(var block=new BlockedOperation(exportTarget,()=>exportTarget.Apply(applyEdit)))
        {exported=export.ExportPrepared(afterValidation:block.Attempt);block.Join();}
        if(!exported.Succeeded||exported.Rows.Count!=2||exportTarget.Epoch!=2)throw new InvalidDataException("Serialized export failed.");
        Refuse(()=>exportTarget.Apply(exported));
        if(exportTarget.IsFaulted)throw new InvalidDataException("Stale preflight poisoned the target.");

        //Root is a deep snapshot acquired under the same scope. Reentrancy
        //inside the owning operation is intentional; no builder lock is taken
        //by Apply, so target-before-builder lock order has no reverse edge.
        using(exportTarget.EnterOperation(exportTarget.Epoch,default))
            if(exportTarget.Root.Children.Count!=1)throw new InvalidDataException("Reentrant root snapshot failed.");
        Console.WriteLine("PASS2 deterministic target Build/Export versus Apply barriers; stale preconsume/reentrant snapshot guards.");
    }
    private static void Refuse(Action action)
    {
        try{action();}catch(InvalidOperationException){return;}
        throw new InvalidDataException("Missing stale-epoch refusal.");
    }
    private sealed class BlockedOperation:IDisposable
    {
        private readonly LegacyLibraryTextDomTarget target;
        private readonly Action operation;
        private readonly ManualResetEventSlim attempted=new(),finished=new();
        private Thread? thread;
        private Exception? error;
        private bool entered;
        internal BlockedOperation(LegacyLibraryTextDomTarget target,Action operation){this.target=target;this.operation=operation;}
        internal void Attempt()
        {
            thread=new Thread(()=>
            {
                try{entered=target.CanEnterOperationPrepared();attempted.Set();operation();}
                catch(Exception e){error=e;}
                finally{attempted.Set();finished.Set();}
            }){IsBackground=true};thread.Start();
            if(!attempted.Wait(TimeSpan.FromSeconds(10)))throw new TimeoutException("Competing operation was not scheduled.");
            if(entered)throw new InvalidDataException("Target scope was released before the live copy phase.");
            if(finished.IsSet)throw new InvalidDataException("Apply entered while the writer held the target scope.");
        }
        internal void Join()
        {
            if(thread is null||!thread.Join(TimeSpan.FromSeconds(10)))throw new TimeoutException("Target lock order deadlocked.");
            if(error is not null)throw new InvalidDataException("Competing actual Apply failed.",error);
        }
        public void Dispose(){thread?.Join(TimeSpan.FromSeconds(10));attempted.Dispose();finished.Dispose();}
    }
}
