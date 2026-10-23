namespace InkRecognition;

// SOURCE ONLY; unconditional whole-admission refusal remains before every
// public/factory prefix claim. These private bodies are not a factory or grant.
internal sealed partial class LegacyFst28CNumberTrueLanguageSourceDraft
{
    private NumericContinuation? completedNumeric;
    // Called only by a future exact fresh factory after genuine construction.
    // No caller/old historical Consumer can invoke this private worker.
    private void ExecuteConstructedTrueConsumer(CancellationToken token,Action<string>? checkpoint)
    {
        RequireWholeAdmission(token); // Always throws in this source packet.
        var actual=ExpectedConsumer;ValidateReceivingConsumer(actual,token);
        void Step(string name){token.ThrowIfCancellationRequested();checkpoint?.Invoke(name);token.ThrowIfCancellationRequested();}
        bool claimed=false;
        try
        {
            Step("before-node0-claim");actual.ClaimTrueLanguageCompletion(this,token);
            claimed=true; // The existing shared consumer CAS is the LAST claim operation.
            Step("after-node0-claim");actual.AttachTrueLanguageCompletion(this,token);
            Step("after-original-heap-handoff");
            ProcessActualCurrentTerminalParents(token);
            var finished=completedNumeric??throw new InvalidDataException("No exact private finalized numeric state.");
            finished.RequireFinalizedReceiver(this,token);
            Step("after-private-finalization");
            Volatile.Write(ref stage,5); // Closed completed storage only; no query/node1 authority.
            Step("before-factory-return");
        }
        catch
        {
            if(claimed)
            {
                // One-way whole postclaim poison. Never clear/restore a claim,
                // initial prefix, heap, context, cursor, masks or partial writes.
                Volatile.Write(ref stage,6);
                source!.PoisonAfterClaim(this);
            }
            throw;
        }
    }
    internal void ValidateActualFinishedNumericProof(object proof,CancellationToken token)
    {
        RequireWholeAdmission(token);
        if(proof is not NumericContinuation state)
            throw new ArgumentException("No genuine private numeric finalization proof.");
        state.RequireFinalizedReceiver(this,token);
    }
    private sealed partial class NumericContinuation
    {
        internal void RequireFinalizedReceiver(LegacyFst28CNumberTrueLanguageSourceDraft expected,CancellationToken token)
        {
            if(!ReferenceEquals(receiver,expected))throw new ArgumentException("Foreign private numeric state.");
            Validate(token);
            if(Volatile.Read(ref finalization)!=2||sourceEnd is <125 or >156||retainedEnd<125||retainedEnd>sourceEnd)
                throw new InvalidDataException("Actual private numeric tail has not finished.");
        }
    }
}

public sealed partial class LegacyFst28CNumberInitialPage
{
    private sealed class NumberEndResetKeyDomains
    {
        internal readonly RetainedSourceState Issuer;
        internal readonly int Slot,State=-1;
        internal readonly ulong CachedContext=ulong.MaxValue,CachedParameters=ulong.MaxValue;
        internal NumberEndResetKeyDomains(RetainedSourceState issuer,int slot){Issuer=issuer;Slot=slot;}
    }
    private sealed partial class SourceCursor
    {
        // Actual CURRENT invalid key domains, separate from retained old Owner
        // and immutable AuxiliaryRawFill metadata describing previous storage.
        internal NumberEndResetKeyDomains? NumberEndResetDomains;
    }
    private sealed partial class RetainedSourceState
    {
        private int actualNode0EndReset;
        private void ValidateActualEndResetDomains(NumberEndResetKeyDomains domains,int slot)
        {
            if(!ReferenceEquals(domains.Issuer,this)||domains.Slot!=slot||domains.State!=-1||
                domains.CachedContext!=ulong.MaxValue||domains.CachedParameters!=ulong.MaxValue||
                Volatile.Read(ref actualNode0EndReset) is not (1 or 2))
                throw new InvalidDataException("Actual end-reset key domains differ.");
        }
        public void ResetAfterActualNode0(LegacyFst28CNumberTrueLanguageSourceDraft receiver,
            object actualNumericProof,CancellationToken token)
        {
            ValidateLiveLease(receiver,Page,Pool,token);
            receiver.ValidateActualFinishedNumericProof(actualNumericProof,token);
            if(Volatile.Read(ref auxiliaryOperation)!=0||Volatile.Read(ref auxiliaryOuterOperation)!=0)
                throw new InvalidOperationException("Cannot reset an active source operation.");
            if(Interlocked.CompareExchange(ref actualNode0EndReset,1,0)!=0)
                throw new InvalidOperationException("Actual node0 end reset is consumed or poisoned.");
            BeginReadsetMutation();
            try
            {
                // Native290237080/236F30 resets actual AUX outer/session first.
                // Old normal Engine is already closed and no surviving normal
                // query authority exists: its released sessions are NOT rebuilt
                // or mutably reconstructed from historical Page values.
                if(auxiliaryFrames is not null)
                {
                    for(int i=0;i<8;i++)
                    {
                        token.ThrowIfCancellationRequested();var f=Frame(i);
                        if(f.CurrentPage is not null&&f.CurrentPage.Consumed!=f.CurrentPage.Returned)
                            throw new NotSupportedException("Unconsumed actual AUX output cannot enter this node0 reset.");
                        if(auxiliaryWalkActive[i])throw new InvalidOperationException("AUX recursive frame is still active.");
                        f.Tag=ulong.MaxValue;f.ParentContext=0;f.RequestedSkip=0;f.CurrentPage=null;
                        //236F30 also clears each OUTER request Q+40/source and
                        //Q+48/slot. This is distinct from the28C reset below.
                        // Typed -1 means NULL source binding, not a native slot.
                        f.AssignedSlot=-1;
                    }
                }
                auxiliaryOuterTag=ulong.MaxValue;auxiliaryOuterAccepted=-1;auxiliaryOuterWritten=0;
                auxiliaryParametersInitialized=false;
                auxiliaryCurrentParent=null;auxiliaryCurrentParentOrdinal=-1;
                auxiliaryColdSkip=0;auxiliaryParentReadCount=0;
                //23E360 retains SAME frontend+70. Already-authenticated Input,
                //Binding,Graph,Source,Header,Cohort and context references stay.
                //23DED0:8 State=-1,8 cachedContext=-1,8 cachedParams=-1,
                //ringIndex=0. Counts, raw/mapped storage and map arrays survive.
                // Immutable old fill metadata is historical storage, not the
                // newly invalid CURRENT loaded key. No row bytes are fabricated.
                for(int i=0;i<8;i++)
                {token.ThrowIfCancellationRequested();ring[i].NumberEndResetDomains=new(this,i);}
                auxiliaryNextSlot=0;
                token.ThrowIfCancellationRequested();Volatile.Write(ref actualNode0EndReset,2);
                CommitReadsetMutation(token);RequireFrozenReadset().Revoke();
            }
            catch{FailAuxiliaryOperation();throw;}
        }
        public void PoisonAfterClaim(LegacyFst28CNumberTrueLanguageSourceDraft receiver)
        {
            // No token/gate/field-copy callback here: cancellation cannot prevent
            // cleanup poison. Exact private receiver only; never a CAS loser.
            if(!ReferenceEquals(receiver,Receiver))throw new ArgumentException("Foreign postclaim poison receiver.");
            frozenReadset?.Poison();
            Volatile.Write(ref auxiliaryFailed,1);Volatile.Write(ref entered,2);
            Volatile.Write(ref actualNode0EndReset,3);
        }
    }
}
