namespace InkRecognition;

// SOURCE ONLY: no compile/run. Original normal QueryFrame.Tag stays readonly.
public sealed partial class LegacyFst28CNumberInitialPage
{
    private sealed partial class SourceCursor
    {
        // Current loaded AUX state is separate from immutable old normal Owner.
        internal AuxiliaryRawFill? AuxiliaryLoaded;
    }
    private sealed class AuxiliaryFrame
    {
        internal readonly RetainedSourceState Issuer;
        internal readonly Input Input;
        internal readonly int Depth;
        internal readonly object Header,Cohort,InitialRequest;
        internal readonly LegacyFst28CNumberPageRecord[] Rows=new LegacyFst28CNumberPageRecord[128];
        internal ulong Tag=ulong.MaxValue;
        internal int RequestedSkip,Generation;
        internal ulong ParentContext;
        // Q+40/source and Q+48/fixed slot. Never cleared on another Q's reload.
        internal int AssignedSlot=-1;
        internal AuxiliaryPageReceipt? CurrentPage;
        internal AuxiliaryFrame(RetainedSourceState issuer,Input input,int depth)
        {Issuer=issuer;Input=input;Depth=depth;Header=input.Header;Cohort=input.Cohort;InitialRequest=input.Request;}
    }
    private sealed class AuxiliaryRawFill
    {
        internal readonly RetainedSourceState Issuer;
        internal readonly AuxiliaryFrame FillFrame;
        internal readonly int Slot,SlotGeneration;
        internal readonly uint State;
        internal readonly LegacyFst13AOutgoingResult Result;
        internal AuxiliaryRawFill(RetainedSourceState issuer,AuxiliaryFrame frame,int slot,
            int slotGeneration,uint state,LegacyFst13AOutgoingResult result)
        {Issuer=issuer;FillFrame=frame;Slot=slot;SlotGeneration=slotGeneration;State=state;Result=result;}
    }
    private sealed class AuxiliaryPageReceipt
    {
        internal readonly RetainedSourceState Issuer;
        internal readonly AuxiliaryFrame Frame;
        internal readonly AuxiliaryRawFill RawFill;
        internal readonly ulong Tag,ParentContext;
        internal readonly int FrameGeneration,RequestedSkip,Returned,PhysicalAfter,AcceptedAfter;
        internal int Consumed;
        internal AuxiliaryPageReceipt(RetainedSourceState issuer,AuxiliaryFrame frame,AuxiliaryRawFill raw,
            int generation,int requested,int returned,int physical,int accepted)
        {Issuer=issuer;Frame=frame;RawFill=raw;Tag=frame.Tag;ParentContext=frame.ParentContext;FrameGeneration=generation;
         RequestedSkip=requested;Returned=returned;PhysicalAfter=physical;AcceptedAfter=accepted;}
    }
    private sealed partial class RetainedSourceState
    {
        private AuxiliaryFrame[]? auxiliaryFrames;
        private int auxiliaryNextSlot,auxiliaryFetches,auxiliaryConsumes,auxiliaryOperation,auxiliaryFailed;
        private readonly int[] auxiliarySlotGenerations=new int[8];
        private void ValidateIssuedAuxiliaryPages(CancellationToken token)
        {
            if(Volatile.Read(ref auxiliaryFailed)!=0)throw new InvalidOperationException("Private AUX lease is poisoned.");
            if(auxiliaryFrames is null)return;
            for(int i=0;i<8;i++)
            {
                token.ThrowIfCancellationRequested();var f=Frame(i);
                if(f.CurrentPage is not null)_=ValidateCurrentAuxiliaryPage(f);
                else if(f.Tag!=ulong.MaxValue||f.RequestedSkip!=0||f.ParentContext!=0||f.Generation<0||
                    f.AssignedSlot is < -1 or >7)
                    throw new InvalidDataException("Unissued AUX frame contains live state.");
            }
        }
        private void EnterAuxiliaryOperation(CancellationToken token)
        {
            ValidateLiveLease(Receiver,Page,Pool,token);
            if(Volatile.Read(ref auxiliaryFailed)!=0)throw new InvalidOperationException("Private AUX lease is poisoned.");
            if(Interlocked.CompareExchange(ref auxiliaryOperation,1,0)!=0)
                throw new InvalidOperationException("Private AUX lease operation is already active.");
            try{BeginReadsetMutation();}catch{Volatile.Write(ref auxiliaryOperation,0);throw;}
        }
        private void FailAuxiliaryOperation(){Volatile.Write(ref auxiliaryFailed,1);frozenReadset?.Poison();}
        private void LeaveAuxiliaryOperation(CancellationToken token)
        {
            try{if(Volatile.Read(ref auxiliaryFailed)==0)CommitReadsetMutation(token);}
            catch{FailAuxiliaryOperation();throw;}
            finally{Volatile.Write(ref auxiliaryOperation,0);}
        }
        private void InitializeAuxiliaryFrames(CancellationToken token)
        {
            EnterAuxiliaryOperation(token);
            try
            {
                if(auxiliaryFrames is not null)throw new InvalidOperationException("AUX frames already issued.");
                var frames=new AuxiliaryFrame[8];for(int i=0;i<8;i++){token.ThrowIfCancellationRequested();frames[i]=new(this,input,i);}
                auxiliaryNextSlot=nextCursor;auxiliaryFrames=frames;
            }
            catch{FailAuxiliaryOperation();throw;}finally{LeaveAuxiliaryOperation(token);}
        }
        private AuxiliaryFrame Frame(int depth)
        {
            if(auxiliaryFrames is null||(uint)depth>=8)throw new InvalidOperationException("Actual private AUX frames are not issued.");
            var f=auxiliaryFrames[depth];
            if(!ReferenceEquals(f.Issuer,this)||!ReferenceEquals(f.Input,input)||f.Depth!=depth||
                !ReferenceEquals(f.Header,input.Header)||!ReferenceEquals(f.Cohort,input.Cohort)||
                !ReferenceEquals(f.InitialRequest,input.Request))throw new InvalidDataException("Foreign AUX frame.");
            return f;
        }
        // Future genuine239100/2388E0 caller invokes this ONLY when its actual
        // per-level Tag changes. Same-tag replay must preserve the current skip.
        // No caller-visible tag/frame/state API or prepared request is supplied.
        private void FetchForChangedAuxiliaryTag(int depth,ulong actualTag,ulong actualParentContext,CancellationToken token)
        {
            EnterAuxiliaryOperation(token);
            try
            {
                var f=Frame(depth);if(f.Tag==actualTag)throw new InvalidOperationException("Same tag is not a changed-level request.");
                if(f.CurrentPage is not null&&f.CurrentPage.Consumed!=f.CurrentPage.Returned)
                    throw new InvalidOperationException("Cannot replace an unconsumed private AUX page.");
                uint state=(uint)actualTag;
                if(state is 0 or >=98||((actualTag>>32)&255)!=1)
                    throw new NotSupportedException("This AUX subset requires a real nonzero singleton state/tag.");
                if(actualParentContext!=0)
                    throw new NotSupportedException("Exact number AUX protocol path must produce NULL parent context.");
                f.Tag=actualTag;f.ParentContext=actualParentContext;f.RequestedSkip=0;FetchAuxiliaryCore(f,token);
                // Literal2388E0 clears the NEXT level tag after changed-tag
                // initial fetch. Keep its frame/slot identity and generation.
                if(depth<7)
                {
                    var child=Frame(depth+1);
                    if(child.CurrentPage is not null&&child.CurrentPage.Consumed!=child.CurrentPage.Returned)
                        throw new NotSupportedException("Pending child output cannot be invalidated by this bounded caller.");
                    child.Tag=ulong.MaxValue;child.ParentContext=0;child.RequestedSkip=0;child.CurrentPage=null;
                }
            }
            catch{FailAuxiliaryOperation();throw;}finally{LeaveAuxiliaryOperation(token);}
        }
        private void RefillAuxiliaryFrame(int depth,CancellationToken token)
        {
            EnterAuxiliaryOperation(token);
            try
            {
                var f=Frame(depth);var page=ValidateCurrentAuxiliaryPage(f);
                if(page.Consumed!=page.Returned)throw new InvalidOperationException("Actual page must be consumed before refill.");
                f.RequestedSkip=checked(page.RequestedSkip+page.Returned);
                FetchAuxiliaryCore(f,token); // Required even when initial Returned==0.
            }
            catch{FailAuxiliaryOperation();throw;}finally{LeaveAuxiliaryOperation(token);}
        }
        private LegacyFst28CNumberPageRecord ConsumeAuxiliaryRecord(int depth,CancellationToken token)
        {
            EnterAuxiliaryOperation(token);
            try
            {
                var f=Frame(depth);var page=ValidateCurrentAuxiliaryPage(f);
                if(page.Consumed>=page.Returned)throw new InvalidOperationException("No actual AUX page row remains.");
                if(auxiliaryConsumes>=4000)throw new InvalidDataException("AUX consumed-record work ceiling exceeded.");
                token.ThrowIfCancellationRequested();var row=f.Rows[page.Consumed++];auxiliaryConsumes++;return row;
            }
            catch{FailAuxiliaryOperation();throw;}finally{LeaveAuxiliaryOperation(token);}
        }
        private AuxiliaryPageReceipt ValidateCurrentAuxiliaryPage(AuxiliaryFrame frame)
        {
            var page=frame.CurrentPage??throw new InvalidOperationException("No Fetch-issued AUX page.");
            if(!ReferenceEquals(page.Issuer,this)||!ReferenceEquals(page.Frame,frame)||
                !ReferenceEquals(page.RawFill.Issuer,this)||page.Tag!=frame.Tag||page.FrameGeneration!=frame.Generation||
                page.ParentContext!=frame.ParentContext||page.ParentContext!=0||
                page.RequestedSkip!=frame.RequestedSkip||page.RequestedSkip is <0 or >12||page.Returned is <0 or >12||
                page.RawFill.State!=(uint)page.Tag||page.RawFill.Result.OutputFlag!=0||
                page.Consumed<0||page.Consumed>page.Returned||page.PhysicalAfter<0||page.PhysicalAfter>page.RawFill.Result.Count||
                page.AcceptedAfter<0||page.AcceptedAfter>page.PhysicalAfter||
                page.Returned!=Math.Max(0,page.AcceptedAfter-page.RequestedSkip))
                throw new InvalidDataException("Private Fetch/page chronology receipt differs.");
            int rank=0;
            for(int i=0;i<page.PhysicalAfter;i++)
                if(input.D0.Policy.MapSecondary(page.RawFill.Result.GetRow(i).SourceLabel).Code!=65535)rank++;
            if(rank!=page.AcceptedAfter)throw new InvalidDataException("Issued page raw-prefix rank differs.");
            // Copied per-frame Rows remain valid if another frame reloads its
            // shared raw ring slot. Only frame generation invalidates this page.
            return page;
        }
        private void FetchAuxiliaryCore(AuxiliaryFrame f,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();if(auxiliaryFetches>=2000)throw new InvalidDataException("AUX backend work ceiling exceeded.");
            if(f.ParentContext!=0||f.RequestedSkip is <0 or >12||(uint)f.Tag is 0 or >=98)
                throw new NotSupportedException("Bounded actual nonzero AUX source request required.");
            int slot=f.AssignedSlot;bool firstSourceBinding=slot<0;
            if(slot<0){slot=auxiliaryNextSlot;auxiliaryNextSlot=(auxiliaryNextSlot+1)&7;f.AssignedSlot=slot;}
            if((uint)slot>=8)throw new InvalidDataException("Source-issued fixed slot differs.");
            var cursor=ring[slot];uint state=(uint)f.Tag;var loaded=cursor.AuxiliaryLoaded;
            // Same Q keeps its slot. A different Q may have loaded another
            // state into that slot: native state mismatch reloads IN THIS SLOT.
            if(firstSourceBinding||cursor.NumberEndResetDomains is not null||loaded is null||!ReferenceEquals(loaded.Issuer,this)||loaded.State!=state)
            {
                var outgoing=input.Outgoing.Query(state,12,12*16,token);
                RequireFrozenReadset().CompareOutgoing(outgoing);
                if(outgoing.State!=state||outgoing.OutputFlag!=0||outgoing.Count>12)
                    throw new InvalidDataException("Genuine immutable outgoing source result differs.");
                for(int i=0;i<outgoing.Count;i++)
                {
                    token.ThrowIfCancellationRequested();var row=outgoing.GetRow(i);
                    if(row.SourceLabel>=16||row.TargetState>=98||(row.TargetByte7&~1)!=0)
                        throw new InvalidDataException("Actual AUX raw outgoing domain differs.");
                    cursor.Rows[i]=row;cursor.Mapped[i]=input.D0.Policy.MapSecondary(row.SourceLabel).Code;
                }
                cursor.Count=outgoing.Count;cursor.Physical=cursor.Accepted=0;cursor.Owner=null;
                loaded=new(this,f,slot,checked(++auxiliarySlotGenerations[slot]),state,outgoing);
                cursor.AuxiliaryLoaded=loaded;
                cursor.NumberEndResetDomains=null; // Only a genuine refill can restore a loaded key.
            }
            ValidateAuxiliaryOwner(loaded,cursor,slot,token);
            if(f.RequestedSkip<cursor.Accepted)cursor.Physical=cursor.Accepted=0;
            while(cursor.Accepted<f.RequestedSkip&&cursor.Physical<cursor.Count)
            {token.ThrowIfCancellationRequested();if(cursor.Mapped[cursor.Physical++]!=65535)cursor.Accepted++;}
            int returned=0;
            while(cursor.Physical<cursor.Count&&returned<128)
            {
                token.ThrowIfCancellationRequested();int index=cursor.Physical++;var row=cursor.Rows[index];ushort code=cursor.Mapped[index];
                if(code==65535)continue;
                ulong context=sourceNodes[row.SourceLabel];if(code==65533?context!=0:context==0)
                    throw new InvalidDataException("Actual AUX missing/nonNULL context differs.");
                // Nonzero-state/C0false emission uses actual sourceV70 flag,
                // not the state0 callback's per-target Byte7 rule. This actual
                // immutable raw source Result.OutputFlag is authenticated0.
                ulong tag=row.TargetState|(1UL<<32);
                f.Rows[returned++]=new(tag,context,row.Cost,code,0,0,0);cursor.Accepted++;
            }
            auxiliaryFetches++;f.Generation=checked(f.Generation+1);
            // Issue ONLY after complete real source processing, before any
            // callback/record consumption; no caller-supplied success flag.
            f.CurrentPage=new(this,f,loaded,f.Generation,f.RequestedSkip,returned,cursor.Physical,cursor.Accepted);
            _=ValidateCurrentAuxiliaryPage(f);
        }
        private void ValidateAuxiliaryOwner(AuxiliaryRawFill loaded,SourceCursor cursor,int slot,CancellationToken token)
        {
            if(cursor.NumberEndResetDomains is not null||!ReferenceEquals(loaded.Issuer,this)||loaded.Slot!=slot||loaded.State is 0 or >=98||
                loaded.SlotGeneration!=auxiliarySlotGenerations[slot]||!ReferenceEquals(cursor.AuxiliaryLoaded,loaded)||
                cursor.Owner is not null||loaded.Result.State!=loaded.State||loaded.Result.Count!=cursor.Count||
                !ReferenceEquals(loaded.FillFrame.Issuer,this)||!ReferenceEquals(loaded.FillFrame.Input,input)||
                cursor.Count is <0 or >12||cursor.Physical<0||cursor.Physical>cursor.Count||cursor.Accepted<0||cursor.Accepted>cursor.Physical)
                throw new InvalidDataException("Exact current raw slot generation/source association differs.");
            int rank=0;
            for(int i=0;i<cursor.Count;i++)
            {
                token.ThrowIfCancellationRequested();var row=loaded.Result.GetRow(i);
                if(row!=cursor.Rows[i]||cursor.Mapped[i]!=input.D0.Policy.MapSecondary(row.SourceLabel).Code)
                    throw new InvalidDataException("Actual immutable AUX raw result/secondary map differs.");
                if(i<cursor.Physical&&cursor.Mapped[i]!=65535)rank++;
            }
            if(rank!=cursor.Accepted)throw new InvalidDataException("Actual filtered raw-prefix rank differs.");
            // Do not compare the fill frame's CURRENT Tag with loaded.State:
            // that frame can legitimately have selected another level tag.
        }
    }
}
