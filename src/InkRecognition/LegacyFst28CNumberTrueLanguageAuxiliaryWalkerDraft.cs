namespace InkRecognition;

internal sealed partial class LegacyFst28CNumberTrueLanguageSourceDraft
{
    // No supplied tag/cost/row can manufacture this private parent identity.
    // The sole caller first obtains the exact private phase0 state; that issuer
    // remains closed/unimplemented, so this code is not currently executable.
    private sealed class CurrentTerminalParent
    {
        internal readonly LegacyFst28CNumberTrueLanguageSourceDraft Receiver;
        internal readonly NumericContinuation State;
        internal readonly int Ordinal;
        internal CurrentTerminalParent(LegacyFst28CNumberTrueLanguageSourceDraft receiver,
            NumericContinuation state,int ordinal)
        {Receiver=receiver;State=state;Ordinal=ordinal;}
    }
    internal (ulong Tag,int Ordinal) ReadActualCurrentParentValues(object opaqueProof,CancellationToken token)
    {
        RequireWholeAdmission(token);
        if(opaqueProof is not CurrentTerminalParent p||!ReferenceEquals(p.Receiver,this))
            throw new ArgumentException("No actual same-receiver private parent proof.");
        if(!p.State.TryReadActualParentForReceiver(this,p.Ordinal,out ulong tag,out _,token))
            throw new InvalidDataException("Actual private parent is not finite.");
        return (tag,p.Ordinal);
    }

    private void ProcessActualCurrentTerminalParents(CancellationToken token)
    {
        RequireWholeAdmission(token);
        // This factory STILL throws: no hypothetical phase0 state is created.
        var state=CreateNumericContinuation(token);
        state.ValidateParentReceiver(this,token);
        var source= this.source??throw new InvalidOperationException("No genuine retained source lease.");
        for(int parent=0;parent<125;parent++)
        {
            token.ThrowIfCancellationRequested();
            if(!state.TryReadActualParent(parent,out ulong tag,out _,token))break;
            var proof=new CurrentTerminalParent(this,state,parent);
            bool pending=((tag>>40)&2)!=0&&((tag>>56)&1)==0;int reads=0;
            while(true)
            {
                if(++reads>2)throw new InvalidDataException("Actual number current-terminal loop exceeded its bound.");
                int sourceReturned=source.ReadActualParentAuxiliary(this,proof,token);
                // This authenticated model's secondary closure has no visible
                // AUX rows. We still ran its actual backend/walker to obtain0.
                if(sourceReturned!=0)throw new NotSupportedException("Nonempty AUX payload writer is outside this exact model scope.");
                bool injected=false;
                if(pending)
                {
                    pending=false;injected=true;
                    _=state.WriteActualCurrentTerminal(parent,token);
                }
                // Injection keeps the native loop alive once without advancing
                // the backend accepted skip. A nonterminal parent stops at once.
                if(sourceReturned==0&&!injected)break;
            }
        }
        state.AssignActualContinuationOrdinals(token);
        state.ProduceActualContinuationAlternatives(token);
        state.FinalizeActualContinuation(token);
        source.ResetAfterActualNode0(this,state,token);
        completedNumeric=state;
        // The private caller returns only to the same closed orchestration.
        // No historical Page is mutated, and no live query/result is exported.

    }
}

public sealed partial class LegacyFst28CNumberInitialPage
{
    private sealed partial class RetainedSourceState
    {
        private readonly LegacyFst28CNumberPageRecord[] auxiliaryOuterRows=new LegacyFst28CNumberPageRecord[128];
        private readonly bool[] auxiliaryWalkActive=new bool[8];
        private ulong auxiliaryOuterTag=ulong.MaxValue;
        private int auxiliaryOuterAccepted=-1,auxiliaryOuterWritten,auxiliaryOuterOperation;
        private bool auxiliaryParametersInitialized;
        private object? auxiliaryCurrentParent;
        private int auxiliaryCurrentParentOrdinal=-1,auxiliaryColdSkip,auxiliaryParentReadCount;

        public int ReadActualParentAuxiliary(LegacyFst28CNumberTrueLanguageSourceDraft receiver,
            object actualParentProof,CancellationToken token)
        {
            ValidateLiveLease(receiver,Page,Pool,token);
            if(Interlocked.CompareExchange(ref auxiliaryOuterOperation,1,0)!=0)
                throw new InvalidOperationException("Actual AUX outer read is already active.");
            try
            {
                var (tag,ordinal)=receiver.ReadActualCurrentParentValues(actualParentProof,token);
                if(!ReferenceEquals(auxiliaryCurrentParent,actualParentProof))
                {
                    if(ordinal<=auxiliaryCurrentParentOrdinal)
                        throw new InvalidOperationException("Actual materialized parent order cannot replay.");
                    auxiliaryCurrentParent=actualParentProof;auxiliaryCurrentParentOrdinal=ordinal;
                    auxiliaryColdSkip=0;auxiliaryParentReadCount=0;
                }
                if(auxiliaryParentReadCount>=2)throw new InvalidOperationException("Actual parent source loop cannot replay.");
                auxiliaryParentReadCount++;
                if(auxiliaryFrames is null)InitializeAuxiliaryFrames(token);
                int returned=ReadNumberAuxiliaryOuter(tag,auxiliaryColdSkip,token);
                auxiliaryColdSkip=checked(auxiliaryColdSkip+returned);
                return returned; // Genuine walker count, never a fake empty backend.
            }
            catch{FailAuxiliaryOperation();throw;}
            finally{Volatile.Write(ref auxiliaryOuterOperation,0);}
        }
        private void InvalidateAuxiliaryRoot(CancellationToken token)
        {
            // Called only inside the already-exclusive outer operation after
            // full live authentication; this adds no nested source operation.
            token.ThrowIfCancellationRequested();var root=Frame(0);
            if(root.CurrentPage is not null&&root.CurrentPage.Consumed!=root.CurrentPage.Returned)
                throw new NotSupportedException("Bounded whole AUX read cannot discard an active output page.");
            BeginReadsetMutation();
            try
            {
                root.Tag=ulong.MaxValue;root.ParentContext=0;root.RequestedSkip=0;root.CurrentPage=null;
                CommitReadsetMutation(token);
            }
            catch{FailAuxiliaryOperation();throw;}
        }
        private int ReadNumberAuxiliaryOuter(ulong actualTag,int acceptedSkip,CancellationToken token)
        {
            // Native239100: same tag/acceptedSkip and fixed params; first cold
            // parameter transition invalidates the OUTER tag after root reset.
            // This is why the next same-tag request may replay once.
            bool changed=!auxiliaryParametersInitialized;
            if(auxiliaryOuterTag!=actualTag||auxiliaryOuterAccepted!=acceptedSkip||changed)
            {
                auxiliaryOuterTag=actualTag;auxiliaryOuterAccepted=0;
                InvalidateAuxiliaryRoot(token);
            }
            if(changed){auxiliaryOuterTag=ulong.MaxValue;auxiliaryParametersInitialized=true;}
            auxiliaryOuterWritten=0;
            WalkNumberAuxiliary(0,actualTag,0,0,0,0,0,acceptedSkip,token);
            // A different source alphabet/flag/caller requires an owning payload
            // path. This fixed actual root's raw closure predicts zero; counts
            // are checked AFTER real Fetch/Consume, not supplied as authority.
            if(auxiliaryOuterWritten!=0)
                throw new NotSupportedException("Authenticated number AUX closure unexpectedly emitted a visible row.");
            return auxiliaryOuterWritten;
        }
        private void WalkNumberAuxiliary(int depth,ulong actualTag,ulong actualParent,
            byte length,int cost,byte incomingKind,byte incomingFlag,int acceptedSkip,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();if((uint)depth>=8)throw new InvalidDataException("AUX depth exceeded8.");
            for(int ancestor=0;ancestor<depth;ancestor++)
                if(auxiliaryWalkActive[ancestor]&&Frame(ancestor).Tag==actualTag)return;
            auxiliaryWalkActive[depth]=true;
            try
            {
                var frame=Frame(depth);
                if(frame.Tag!=actualTag)FetchForChangedAuxiliaryTag(depth,actualTag,actualParent,token);
                else if(frame.ParentContext!=actualParent)
                    throw new NotSupportedException("Same-tag parent-context change requires native caller closure.");
                while(true)
                {
                    var page=ValidateCurrentAuxiliaryPage(frame);
                    while(page.Consumed<page.Returned)
                    {
                        var row=ConsumeAuxiliaryRecord(depth,token);
                        ushort symbol=row.Symbol;
                        byte flags=(byte)((row.CaseFlags&240)|(incomingKind&7)|((incomingFlag&1)<<3));
                        row=row with{CaseFlags=flags};
                        bool protocol=symbol>=192&&unchecked((ushort)(symbol+29))>=8;
                        bool handled=false,follow=false;
                        if(protocol)
                        {
                            // Exact secondary map admits only FFFD and FFFB.
                            // Other protocol contracts must not be guessed.
                            if(symbol==65531)
                            {
                                if((flags&7)!=1)flags=(byte)((flags&250)|2);
                                if(row.ContextIdentity==0)throw new InvalidDataException("FFFB requires its actual live leaf.");
                                ulong parent=ReadCurrentContextParent(frame,page,token);
                                row=row with{ContextIdentity=parent,CaseFlags=flags};
                            }
                            else if(symbol!=65533)
                                throw new NotSupportedException("Unproved number AUX protocol branch.");
                            handled=follow=true;
                        }
                        else
                            throw new NotSupportedException("This exact secondary map admits no ordinary/joiner output code.");
                        if(follow&&depth<7)
                            WalkNumberAuxiliary(depth+1,row.Tag,row.ContextIdentity,
                                unchecked((byte)(length+row.SourceCategory)),unchecked(cost+row.Cost),
                                (byte)(row.CaseFlags&7),(byte)((row.CaseFlags>>3)&1),acceptedSkip,token);
                        if(handled&&((row.Tag>>40)&2)!=0)
                        {
                            if(auxiliaryOuterAccepted>=acceptedSkip)
                            {
                                if(auxiliaryOuterWritten>=128)
                                    throw new NotSupportedException("Partial output-page continuation is outside this closed number slice.");
                                byte final=CarryNumberKind(row.CaseFlags,incomingKind,incomingFlag);
                                ulong tag=(row.Tag&~(132UL<<40))|(actualTag&(128UL<<40));
                                auxiliaryOuterRows[auxiliaryOuterWritten++]=row with{Tag=tag,
                                    Cost=unchecked(cost+row.Cost),SourceCategory=unchecked((byte)(length+row.SourceCategory)),CaseFlags=final};
                            }
                            auxiliaryOuterAccepted=checked(auxiliaryOuterAccepted+1);
                        }
                    }
                    // Native do/while: refill even if initial returned zero.
                    RefillAuxiliaryFrame(depth,token);
                    if(ValidateCurrentAuxiliaryPage(frame).Returned==0)break;
                }
            }
            finally{auxiliaryWalkActive[depth]=false;}
        }
        private static byte CarryNumberKind(byte value,byte kind,byte flag)
        {
            int current=value&7;
            current=kind switch{1=>1,2=>current==1?1:2,3=>((current-1)&252)!=0||current==3?3:current,
                4=>unchecked((byte)(current-1))>1?4:current,_=>current};
            return (byte)((value&240)|current|(((value>>3&1)|flag)<<3));
        }
    }
}
