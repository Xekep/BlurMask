namespace InkRecognition.Math;

internal sealed class Math2Mode0CurrentInputReceipt { internal Math2Mode0CurrentInputReceipt() { } }
internal sealed record Math2Mode0CurrentInputSnapshot(long AppliedEpoch,int InputSequence,long InputId,long Next,
    int ObjectCount,Math2Mode0OriginalRegistryRow[] Registry);

// This new handle is separate from the captured fresh application registry.
// It is not a native CBB token, history transaction or pending DOM receiver.
internal abstract class Math2Mode0CurrentInputHandle
{
    internal abstract Math2Mode0CurrentInputReceipt Receipt { get; }
    internal abstract void Check(Math2Mode0OriginalApplication app,Math2Mode0OriginalInput input);
    internal abstract T WithRaw<T>(Func<LegacyOwnedRawInkAppendState.Borrow,T> consume,CancellationToken token);
    internal abstract Math2Mode0OriginalRegistryRow[] Snapshot();
    internal abstract object SelectionIdentity(long id);
    internal abstract LegacyNativeInkPositionRange[] InputRanges { get; }
    internal abstract long AppliedEpoch { get; }
    internal abstract long InputId { get; }
    internal abstract long Next { get; }
    internal abstract void Publish(Math2Mode0OriginalApplication app,Math2Mode0OriginalInput input);
    internal abstract void Fail();
}

internal sealed partial class Math2Mode0OriginalMathFieldSource
{
    private LegacyOwnedRawInkAppendState? rawAppendState;
    private Math2Mode0CurrentInputHandle? currentInputHandle;
    private bool currentInputFailed;
    private sealed class ChangedInputHandle(Math2Mode0OriginalMathFieldSource source,
        Math2Mode0OriginalApplication app,LegacyOwnedRawInkAppendState raw,
        RegistryEntry input,Math2Mode0OriginalRegistry[] expected,long next,long appliedEpoch):Math2Mode0CurrentInputHandle
    {
        private readonly Math2Mode0CurrentInputReceipt receipt=new();
        internal LegacyOwnedRawInkAppendState.Revision? Revision;
        internal bool Ready;
        internal override Math2Mode0CurrentInputReceipt Receipt=>receipt;
        internal override long AppliedEpoch=>appliedEpoch;
        internal override long InputId=>input.Id;
        internal override long Next=>next;
        internal override LegacyNativeInkPositionRange[] InputRanges=>input.C22.Ranges;
        internal override void Check(Math2Mode0OriginalApplication actual,Math2Mode0OriginalInput original)
            =>CheckOwner(actual,original,true);
        private void CheckOwner(Math2Mode0OriginalApplication actual,Math2Mode0OriginalInput original,bool requireReady)
        {
            if(!Monitor.IsEntered(source.gate))throw new InvalidOperationException("Actual original field gate required.");
            source.CheckScope();
            if((requireReady&&!Ready)||source.currentInputFailed||!ReferenceEquals(source.currentInputHandle,this)||
                !ReferenceEquals(app,actual)||!ReferenceEquals(original.Source,source)||
                !ReferenceEquals(original.Ink,source.originalInk)||!ReferenceEquals(original.Names,source.OriginalNameOwner)||
                source.originalInk.Next!=next||source.originalInk.Registry.Count!=expected.Length||
                !ReferenceEquals(source.originalC22,input.C22)||!ReferenceEquals(source.originalRanges,input.C22.Ranges))
                throw new InvalidOperationException("Exact committed changed-input authority required.");
            foreach(var row in expected)
                if(!source.originalInk.Registry.TryGetValue(row.Id,out var live)||!ReferenceEquals(live,row.Entry)||
                    !ReferenceEquals(live.C22,row.C22)||!ReferenceEquals(live.Owner,source.originalInk))
                    throw new InvalidOperationException("Changed registry identity lost.");
        }
        internal override T WithRaw<T>(Func<LegacyOwnedRawInkAppendState.Borrow,T> consume,CancellationToken token)
            =>raw.WithCurrent(Revision??throw new InvalidOperationException("Raw commit absent."),consume,token);
        internal override object SelectionIdentity(long id)=>expected.Single(e=>e.Id==id).C22;
        internal override Math2Mode0OriginalRegistryRow[] Snapshot()
        {
            var aliases=new Dictionary<object,int>(ReferenceEqualityComparer.Instance);
            return expected.Select(e=>
            {if(!aliases.TryGetValue(e.C22,out int id)){id=aliases.Count+1;aliases.Add(e.C22,id);}return new Math2Mode0OriginalRegistryRow(e.Id,e.Name,e.Category,e.Ranges.ToArray(),id);}).ToArray();
        }
        internal override void Publish(Math2Mode0OriginalApplication actual,Math2Mode0OriginalInput original)
        {
            CheckOwner(actual,original,false);
            WithRaw(b=>{b.Validate();return 0;},default);
            Ready=true;
        }
        internal override void Fail(){lock(source.gate){source.currentInputFailed=true;Ready=false;}}
    }
    internal Math2Mode0CurrentInputHandle AppendCurrentInput(Math2Mode0OriginalApplication.PriorBorrow borrow,
        InkStroke geometry,long rawBudget,CancellationToken token)
    {
        if(!Monitor.IsEntered(gate))throw new InvalidOperationException("Ancestor original field gate required.");
        CheckScope();borrow.CheckSource(this,originalInk);token.ThrowIfCancellationRequested();
        if(currentInputHandle is not null||currentInputFailed)throw new NotSupportedException("This packet admits one appended input revision only.");
        if(originalInk.Registry.Count is <1 or >65536||originalRanges.Length is <1 or >65536||
            !originalInk.Registry.TryGetValue(tag,out var old)||old.Name!="INPUT"||old.Category!=102||
            !ReferenceEquals(old.C22,originalC22)||!ReferenceEquals(old.Owner,originalInk)||
            !ReferenceEquals(old.C22.Owner,originalInk))throw new InvalidOperationException("Actual original INPUT102 required.");
        long rangesCount=0;
        foreach(var row in originalInk.Registry.Values)
        {token.ThrowIfCancellationRequested();rangesCount=checked(rangesCount+row.C22.Ranges.LongLength);}
        long planned=checked(8192+originalInk.Registry.Count*256L+rangesCount*128L+LegacyOwnedRawInkAppendState.DefaultClonePayloadBudget);
        if(rangesCount>65536||planned>DefaultPayloadBudget)
            throw new NotSupportedException("Bounded current registry/clone payload required before append preparation.");
        rawAppendState??=LegacyOwnedRawInkAppendState.Adopt(CurrentView,token);
        var revision=rawAppendState.Capture();
        var prepared=rawAppendState.PrepareAppend(revision,geometry,rawBudget,token);
        bool rawCommitted=false;
        try
        {
            int count=rawAppendState.WithCurrent(revision,b=>b.Count,token);
            int points=rawAppendState.PreparedPointCount(prepared);
            var added=new LegacyNativeInkPositionRange(
                LegacyNativeInkPosition.Pack((uint)count,0,3,points),
                LegacyNativeInkPosition.Pack((uint)count,points-1,5,points));
            // 004FA0: union actual new-object selection with existing INPUT,
            // then NEW same-Ink C22; never mutate a shared old canonical C22.
            var ranges=LegacyOwnedRawInkRangeOperations.Prepare(Document).Union(old.C22.Ranges,[added],token);
            var canonical=originalInk.Registry.Values.Select(e=>e.C22).FirstOrDefault(c=>
                LegacyOwnedRawInkRangeOperations.Equal(c.Ranges,ranges));
            var selection=canonical??new Selection(originalInk,ranges);
            var entry=new RegistryEntry(old.Id,old.Name,old.Category,originalInk,selection);
            var expected=originalInk.Registry.Values.OrderBy(e=>e.Id).Select(e=>
            {var value=e.Id==old.Id?entry:e;return new Math2Mode0OriginalRegistry(value.Id,value.Name,value.Category,value.C22.Ranges,value,value.C22);}).ToArray();
            var handle=new ChangedInputHandle(this,borrow.Owner,rawAppendState,entry,expected,originalInk.Next,borrow.Epoch);
            // Everything dependent on the raw append is allocated before its
            // commit. Existing long-key overwrite neither resizes nor callbacks.
            borrow.CheckSource(this,originalInk);token.ThrowIfCancellationRequested();
            var successor=rawAppendState.CommitPrepared(prepared,token);rawCommitted=true;
            originalInk.Registry[old.Id]=entry;
            originalC22=selection;originalRanges=selection.Ranges;
            handle.Revision=successor;currentInputHandle=handle;
            return handle;
        }
        catch {if(rawCommitted)currentInputFailed=true;throw;}
        finally {rawAppendState.AbandonPrepared(prepared);}
    }
}

internal sealed partial class Math2Mode0OriginalApplication
{
    private Math2Mode0CurrentInputHandle? changedInput;
    private CurrentInputBorrow? currentInputBorrow;
    internal Math2Mode0CurrentInputReceipt AppendCurrentInput(InkStroke inputGeometry,
        long rawBudget=LegacyOwnedRawInkAppendState.DefaultClonePayloadBudget,CancellationToken token=default)
    {
        Math2Mode0CurrentInputHandle? committed=null;
        try
        {
            var receipt=WithCompletedOriginalInput(borrow=>
            {
                if(changedInput is not null)throw new NotSupportedException("One current-input append in this packet.");
                var actual=field.AppendCurrentInput(borrow,inputGeometry,rawBudget,token);
                // Preserve the old captured registry. Its existing fresh/prior
                // guards must reject the now changed INPUT instead of relaxing.
                changedInput=committed=actual;return actual.Receipt;
            },token);
            // Publication readiness follows successful return through all old
            // ancestor guards; provisional commit cannot yield a successor.
            WithLive((input,transfer)=>
            {
                if(!complete||!priorReady||!sourceSucceeded||!journalComplete||!domEntered||rolledBack||
                    epoch!=committed!.AppliedEpoch||!ReferenceEquals(actualInk,input.Ink))
                    throw new InvalidOperationException("Applied ancestor changed before current-input publication.");
                committed.Publish(this,input);return 0;
            },default);
            return receipt;
        }
        catch {if(committed is not null)committed.Fail();throw;}
    }
    internal sealed partial class CurrentInputBorrow
    {
        private readonly Math2Mode0OriginalApplication owner;
        private readonly Math2Mode0OriginalInput input;
        private readonly Math2Mode0OriginalTransferInput transfer;
        private readonly Math2Mode0CurrentInputHandle handle;
        private readonly LegacyOwnedRawInkAppendState.Borrow raw;
        private readonly long epoch;
        private CurrentInputBorrow(Math2Mode0OriginalApplication owner,Math2Mode0OriginalInput input,
            Math2Mode0OriginalTransferInput transfer,Math2Mode0CurrentInputHandle handle,LegacyOwnedRawInkAppendState.Borrow raw)
        {this.owner=owner;this.input=input;this.transfer=transfer;this.handle=handle;this.raw=raw;epoch=owner.epoch;}
        internal static CurrentInputBorrow Issue(Math2Mode0OriginalApplication owner,Math2Mode0OriginalInput input,
            Math2Mode0OriginalTransferInput transfer,Math2Mode0CurrentInputHandle handle,LegacyOwnedRawInkAppendState.Borrow raw)
        {if(!Monitor.IsEntered(owner.gate)||owner.currentInputBorrow is not null)throw new InvalidOperationException();return new(owner,input,transfer,handle,raw);}
        internal void Check()
        {
            if(!Monitor.IsEntered(owner.gate)||!ReferenceEquals(owner.currentInputBorrow,this)||
                !ReferenceEquals(owner.changedInput,handle)||owner.released||owner.failed||!owner.complete||!owner.priorReady||
                !owner.sourceSucceeded||!owner.journalComplete||!owner.domEntered||owner.rolledBack||owner.epoch!=epoch||epoch!=handle.AppliedEpoch||epoch<1||
                !ReferenceEquals(owner.actualInk,input.Ink))throw new InvalidOperationException("Actual completed old result/current-input borrow required.");
            input.Check(default);transfer.Check(default);handle.Check(owner,input);raw.Validate();
        }
        internal LegacyOwnedRawInkDocument Document {get{Check();return raw.Document;}}
        internal object OriginalInkIdentity {get{Check();return input.Ink;}}
        internal object AppliedArenaIdentity {get{Check();return owner.priorArenaWitness??=new ArenaWitness(owner.active);}}
        internal LegacyOwnedRawInkCurve Curve(int index){Check();return raw.Curve(index);}
        internal LegacyOwnedRawInkCurrentViewEntry View(int index){Check();return raw.View(index);}
        internal object SelectionIdentity(long id){Check();return handle.SelectionIdentity(id);}
        internal Math2Mode0CurrentInputSnapshot CopySnapshot()
        {Check();return new(epoch,raw.Sequence,handle.InputId,handle.Next,raw.Count,handle.Snapshot());}
    }
    internal T WithChangedInput<T>(Math2Mode0CurrentInputReceipt receipt,Func<CurrentInputBorrow,T> consume,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(receipt);ArgumentNullException.ThrowIfNull(consume);
        return WithLive((input,transfer)=>
        {
            var handle=changedInput??throw new InvalidOperationException("No owned changed-input revision.");
            if(!ReferenceEquals(receipt,handle.Receipt)||currentInputBorrow is not null)
                throw new InvalidOperationException("Exact current-input receipt/exclusive borrow required.");
            if(!complete||!priorReady||!sourceSucceeded||!journalComplete||!domEntered||rolledBack||epoch!=handle.AppliedEpoch)
                throw new InvalidOperationException("Exact completed applied epoch required.");
            handle.Check(this,input);
            Math2Mode0OriginalKernel.ValidateTags(active,originalField,input.ReadRegistry(),handle.InputRanges,token);
            return handle.WithRaw(raw=>
            {
                var borrow=CurrentInputBorrow.Issue(this,input,transfer,handle,raw);currentInputBorrow=borrow;
                try{borrow.Check();T result=consume(borrow);borrow.Check();token.ThrowIfCancellationRequested();return result;}
                finally{currentInputBorrow=null;}
            },token);
        },token);
    }
}
