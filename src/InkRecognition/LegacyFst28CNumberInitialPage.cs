using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace InkRecognition;

/// <summary>Exact logical fields of one produced32-byte normal record.
/// ContextIdentity is a same-container symbolic node ID, never a pointer.</summary>
public readonly record struct LegacyFst28CNumberPageRecord(ulong Tag, ulong ContextIdentity,
    int Cost, ushort Symbol, byte SourceCategory, byte CaseFlags, ulong Extra);
public readonly record struct LegacyFst28CNumberPageCall(uint State, ulong Tag, int Depth,
    int SourceSlot, int Skip, int PhysicalBefore, int AcceptedBefore,
    int PhysicalAfter, int AcceptedAfter, int Returned);
public readonly record struct LegacyFst28CNumberPageCursor(uint State, int SourceSlot,
    int Physical, int Accepted, int RawCount);

/// <summary>One complete first normal page for the genuine constructor-only
/// number source and initial typed prefix. Not a generic Query/Ready/paging,
/// history consumer, accepted-text or number-recognition API.</summary>
public sealed class LegacyFst28CNumberInitialPage
{
    public const int LogicalByteEnvelope=42288;
    public const int WorkEnvelope=165168;
    private sealed class Claim { internal int Used; }
    private static readonly ConditionalWeakTable<LegacyFst28CNumberInitialQueryBinding,Claim> claims=new();

    // Only the binding's trusted accessor constructs the actual input for this
    // invocation. No public API accepts this type or detached scalar requests.
    internal sealed class Input(object invocation,LegacyFst28CNumberInitialQueryBinding binding,
        object request,object header,object cohort,LegacySymbolContextFactory context,
        LegacyFst28CNumberInitialD0 d0,ulong tag,byte source,int start,int span,
        byte flags,int skip,int take)
    {
        internal readonly object Invocation=invocation,Request=request,Header=header,Cohort=cohort;
        internal readonly LegacyFst28CNumberInitialQueryBinding Binding=binding;
        internal readonly LegacySymbolContextFactory Context=context;
        internal readonly LegacyFst28CNumberInitialD0 D0=d0;
        internal readonly ulong Tag=tag;
        internal readonly byte Source=source,Flags=flags;
        internal readonly int Start=start,Span=span,Skip=skip,Take=take;
        internal LegacyFst13AOutgoingSource Outgoing=>D0.Source;
    }
    private readonly LegacyFst28CNumberPageRecord[] records;
    private readonly LegacyFst28CNumberPageCall[] calls;
    private readonly LegacyFst28CNumberPageCursor[] cursors;
    public LegacyFst28CNumberInitialQueryBinding Binding {get;}
    public int Count {get;}
    public int BackendCallCount {get;}
    public int SourceFillCount {get;}
    public int SourceRowsCopied {get;}
    public int OuterDepthUsed {get;}
    public int SourceNextCursor=>SourceFillCount&7;
    public int ContextNodesAdded=>0;
    public bool HasPagingAuthority=>false;
    public bool HasGeneralQueryAuthority=>false;
    public int LogicalByteReservation=>LogicalByteEnvelope;
    public int WorkReservation=>WorkEnvelope;
    private LegacyFst28CNumberInitialPage(Engine engine)
    {
        Binding=engine.Input.Binding; records=engine.Output; calls=engine.Calls; cursors=engine.FinalCursors;
        Count=engine.OutputCount;BackendCallCount=engine.CallCount;SourceFillCount=engine.FillCount;
        SourceRowsCopied=engine.RowsCopied;OuterDepthUsed=engine.DepthUsed;
    }
    public LegacyFst28CNumberPageRecord ReadRecord(int ordinal)
    {if((uint)ordinal>=(uint)Count)throw new ArgumentOutOfRangeException(nameof(ordinal));return records[ordinal];}
    public LegacyFst28CNumberPageCall ReadBackendCall(int ordinal)
    {if((uint)ordinal>=(uint)BackendCallCount)throw new ArgumentOutOfRangeException(nameof(ordinal));return calls[ordinal];}
    public LegacyFst28CNumberPageCursor ReadSourceCursor(int ordinal)
    {if((uint)ordinal>=(uint)SourceFillCount)throw new ArgumentOutOfRangeException(nameof(ordinal));return cursors[ordinal];}
    public byte[] CopyRecordBytes(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();var bytes=new byte[Count*32];
        for(int i=0;i<Count;i++)
        {
            token.ThrowIfCancellationRequested();var r=records[i];var row=bytes.AsSpan(i*32,32);
            BinaryPrimitives.WriteUInt64LittleEndian(row,r.Tag);
            BinaryPrimitives.WriteUInt64LittleEndian(row[8..],r.ContextIdentity);
            BinaryPrimitives.WriteInt32LittleEndian(row[16..],r.Cost);
            BinaryPrimitives.WriteUInt16LittleEndian(row[20..],r.Symbol);
            row[22]=r.SourceCategory;row[23]=r.CaseFlags;
            BinaryPrimitives.WriteUInt64LittleEndian(row[24..],r.Extra);
        }
        token.ThrowIfCancellationRequested();return bytes;
    }
    public static LegacyFst28CNumberInitialPage CreateFirstNormalPage(
        LegacyFst28CNumberInitialQueryBinding binding,CancellationToken token=default)
        =>CreateBounded(binding,LogicalByteEnvelope,WorkEnvelope,token);
    internal static LegacyFst28CNumberInitialPage CreateBounded(
        LegacyFst28CNumberInitialQueryBinding binding,int byteBudget,int workBudget,
        CancellationToken token=default,Action<string>? checkpoint=null)
    {
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(binding);
        if(byteBudget is <0 or >LogicalByteEnvelope)throw new ArgumentOutOfRangeException(nameof(byteBudget));
        if(workBudget is <0 or >WorkEnvelope)throw new ArgumentOutOfRangeException(nameof(workBudget));
        if(byteBudget<LogicalByteEnvelope||workBudget<WorkEnvelope)
            throw new InvalidDataException("Complete first-page protocol/work budget required.");
        var invocation=new object();var input=binding.RetainInitialPageInput(invocation,token);
        if(!ReferenceEquals(input.Invocation,invocation)||!ReferenceEquals(input.Binding,binding)||
            !ReferenceEquals(input.D0,binding.D0)||!ReferenceEquals(input.Outgoing,binding.Source)||
            !ReferenceEquals(input.D0.Graph,binding.Graph)||!ReferenceEquals(input.D0.Policy,binding.Policy)||
            !ReferenceEquals(input.D0.InitialState,binding.InitialState)||!ReferenceEquals(input.D0.Bootstrap,binding.Bootstrap)||
            input.Tag!=(8UL<<40)||input.Source!=0||input.Start!=0||input.Span!=0||input.Flags!=0||input.Skip!=0||input.Take!=128||
            input.Context.NodeCount!=18||input.Context.RootTreeIdentity!=binding.LiveTreeRoot||
            binding.CursorCapacity!=12||binding.CursorCount!=8||input.Outgoing.StateCount!=98||
            input.Outgoing.TransitionWordCount!=299||!input.Outgoing.HasActualFreshMain135Authority)
            throw new InvalidDataException("Actual initial request/source/header/cohort association differs.");
        // Fixed counts and all copied association domains are admitted BEFORE
        // claim and allocations. No hashing/model/NN/coordinate/graph traversal.
        for(ushort label=0;label<16;label++)
        {
            token.ThrowIfCancellationRequested();var node=binding.ReadSourceNode(label);
            var preserved=binding.ReadPreservedCode(label);var mapped=binding.ReadCurrentCode(label);
            if(preserved!=binding.Policy.PrimaryMap.MapLabel(label).Code||
                (mapped!=ushort.MaxValue&&mapped!=preserved)||
                (preserved is 0xfffd or 0xffff ? node!=0 : node==0))
                throw new InvalidDataException("Immutable primary/source-node membership differs.");
            if(node!=0)
            {
                var live=input.Context.ReadNode(node);var slot=binding.ReadExistingSlot(label);
                if(live.Symbol!=preserved||live.PageIndex!=0||live.PageSlot!=slot||slot is <0 or >=15||
                    binding.ReadAuxiliary(slot) is not {HistoryPayloadPresent:false,SecondaryPayloadPresent:false} aux||
                    aux.NodeIdentity!=node||aux.Key!=preserved||aux.Slot!=slot)
                    throw new InvalidDataException("Exact retained auxiliary/live context differs.");
            }
        }
        void Checkpoint(string stage){token.ThrowIfCancellationRequested();checkpoint?.Invoke(stage);token.ThrowIfCancellationRequested();}
        Checkpoint("before-claim");var claim=claims.GetValue(binding,static _=>new Claim());
        if(Interlocked.CompareExchange(ref claim.Used,1,0)!=0)
            throw new InvalidOperationException("Initial binding page already issued or poisoned.");
        Checkpoint("after-claim");var engine=new Engine(input,token,Checkpoint);
        engine.Run();Checkpoint("before-publish");return new(engine);
    }
    // Executing request frames are library-owned state reconstructed from the
    // immutable retained private prefix. This does not mutate its historical
    // diagnostic snapshot or claim a native Q pointer identity.
    private sealed class QueryFrame(ulong tag,Input input)
    {
        internal readonly ulong Tag=tag;
        internal readonly object Header=input.Header,Cohort=input.Cohort,InitialRequest=input.Request;
        internal int AcceptedSkip;
        internal BackendOwner? CachedOwner;
    }
    private sealed class Level
    {
        internal QueryFrame? Query;
        internal readonly LegacyFst28CNumberPageRecord[] Rows=new LegacyFst28CNumberPageRecord[128];
        internal int Count,Position,Cost;
        internal bool Active;
    }
    private sealed class BackendOwner(Input input,QueryFrame query,int slot,uint state)
    {
        internal readonly Input Input=input;
        internal readonly QueryFrame Query=query;
        internal readonly int Slot=slot;
        internal readonly uint State=state;
    }
    private sealed class SourceCursor
    {
        internal BackendOwner? Owner;
        internal readonly LegacyFst13AOutgoingRow[] Rows=new LegacyFst13AOutgoingRow[12];
        internal readonly ushort[] Mapped=new ushort[12];
        internal int Count,Physical,Accepted;
    }
    private sealed class Engine
    {
        internal readonly Input Input;
        internal readonly LegacyFst28CNumberPageRecord[] Output=new LegacyFst28CNumberPageRecord[128];
        internal readonly LegacyFst28CNumberPageCall[] Calls=new LegacyFst28CNumberPageCall[6];
        internal readonly LegacyFst28CNumberPageCursor[] FinalCursors=new LegacyFst28CNumberPageCursor[3];
        private readonly Level[] levels=new Level[8];
        private readonly SourceCursor[] ring=new SourceCursor[8];
        private readonly ushort[] current=new ushort[16];
        private readonly ulong[] sourceNodes=new ulong[16];
        private readonly CancellationToken token;
        private readonly Action<string> checkpoint;
        internal int OutputCount,CallCount,FillCount,RowsCopied,DepthUsed;
        internal Engine(Input input,CancellationToken token,Action<string> checkpoint)
        {
            Input=input;this.token=token;this.checkpoint=checkpoint;
            for(int i=0;i<8;i++){levels[i]=new();ring[i]=new();}
            for(ushort i=0;i<16;i++){token.ThrowIfCancellationRequested();current[i]=input.Binding.ReadCurrentCode(i);sourceNodes[i]=input.Binding.ReadSourceNode(i);}
            checkpoint("after-owned-copies");
        }
        internal void Run()
        {
            Walk(Input.Tag,0,0);
            if(FillCount!=3||CallCount!=6||RowsCopied!=14||DepthUsed!=3||OutputCount>12||Input.Context.NodeCount!=18)
                throw new InvalidDataException("Complete closed initial traversal differs.");
            for(int i=0;i<FillCount;i++)
            {
                var c=ring[i];var o=c.Owner??throw new InvalidDataException("Unfilled source cursor.");
                FinalCursors[i]=new(o.State,i,c.Physical,c.Accepted,c.Count);
            }
            checkpoint("after-complete-page");
        }
        private void Walk(ulong tag,int depth,int accumulated)
        {
            token.ThrowIfCancellationRequested();if((uint)depth>=8)throw new InvalidDataException("Eight-level bound exceeded.");
            for(int i=0;i<depth;i++)if(levels[i].Active&&levels[i].Query!.Tag==tag)
                throw new InvalidDataException("Source traversal cycle.");
            var level=levels[depth];if(level.Query is not null)throw new InvalidDataException("Unsupported repeated level owner.");
            level.Query=new(tag,Input);level.Cost=accumulated;level.Active=true;DepthUsed=Math.Max(DepthUsed,depth+1);
            Fill(level,depth);
            while(true)
            {
                while(level.Position<level.Count)
                {
                    token.ThrowIfCancellationRequested();var row=level.Rows[level.Position++];
                    if(row.SourceCategory!=0||row.CaseFlags!=0||row.Extra!=0)throw new InvalidDataException("Unsupported row metadata.");
                    if(row.Symbol>=192&&unchecked((ushort)(row.Symbol+29))>=8)
                    {
                        if(row.Symbol!=0xfffd||row.ContextIdentity!=0)throw new InvalidDataException("Unsupported protocol/history parent.");
                        Walk(row.Tag,depth+1,checked(accumulated+row.Cost));
                    }
                    else
                    {
                        if(row.Symbol>=192||row.ContextIdentity==0||OutputCount>=12)
                            throw new InvalidDataException("Unsupported ordinary row or complete output bound.");
                        Output[OutputCount++]=row with {Cost=checked(accumulated+row.Cost)};
                    }
                }
                // Native2377CC empty-initial path still reaches237D3A V40.
                // Only the returned refill count at237D4E terminates the loop.
                level.Query.AcceptedSkip=checked(level.Query.AcceptedSkip+level.Count);
                Fill(level,depth);
                if(level.Count==0)break;
            }
            level.Active=false;
        }
        private void Fill(Level level,int depth)
        {
            token.ThrowIfCancellationRequested();if(CallCount>=6)throw new InvalidDataException("Backend call bound exceeded.");
            var q=level.Query??throw new InvalidDataException("Missing executing query.");
            if(!ReferenceEquals(q.Header,Input.Header)||!ReferenceEquals(q.Cohort,Input.Cohort)||
                !ReferenceEquals(q.InitialRequest,Input.Request)||q.AcceptedSkip is <0 or >12)
                throw new InvalidDataException("Foreign executing query/header/cohort.");
            uint state=(uint)q.Tag;byte ns=(byte)(q.Tag>>32),flags=(byte)(q.Tag>>40);
            if(!((state==0&&ns==0&&flags==8&&depth==0)||
                (state==97&&ns==1&&flags==1&&depth==1)||(state==65&&ns==1&&flags==0&&depth==2)))
                throw new InvalidDataException("Unsupported singleton namespace/root/prefix branch.");
            SourceCursor cursor;
            if(q.CachedOwner is null)
            {
                if(FillCount>=3)throw new InvalidDataException("Initial source fill bound exceeded.");
                int slot=FillCount&7;cursor=ring[slot];
                if(cursor.Owner is not null)throw new InvalidDataException("Unexpected ring eviction.");
                var owner=new BackendOwner(Input,q,slot,state);
                // Genuine V80 primitive, bounded whole result before own cache copy.
                checkpoint("before-source-fill");var outgoing=Input.Outgoing.Query(state,12,12*16,token);
                if(outgoing.State!=state||outgoing.OutputFlag!=0||outgoing.Count>12)
                    throw new InvalidDataException("Immutable source fill differs.");
                cursor.Count=outgoing.Count;
                for(int i=0;i<outgoing.Count;i++)
                {
                    token.ThrowIfCancellationRequested();var r=outgoing.GetRow(i);
                    if(r.SourceLabel>=16||r.TargetState>=98||(r.TargetByte7&~1)!=0)
                        throw new InvalidDataException("Actual outgoing row domain differs.");
                    cursor.Rows[i]=r;cursor.Mapped[i]=current[r.SourceLabel];
                }
                cursor.Owner=owner;q.CachedOwner=owner;FillCount++;RowsCopied+=cursor.Count;
                checkpoint("after-source-fill");
            }
            else
            {
                var owner=q.CachedOwner;
                if(!ReferenceEquals(owner.Input,Input)||!ReferenceEquals(owner.Query,q)||owner.State!=state||
                    (uint)owner.Slot>=8||!ReferenceEquals(ring[owner.Slot].Owner,owner))
                    throw new InvalidDataException("Foreign per-level cached source owner.");
                cursor=ring[owner.Slot];
            }
            int beforePhysical=cursor.Physical,beforeAccepted=cursor.Accepted;
            if(q.AcceptedSkip<cursor.Accepted)cursor.Physical=cursor.Accepted=0;
            while(cursor.Accepted<q.AcceptedSkip&&cursor.Physical<cursor.Count)
            {token.ThrowIfCancellationRequested();if(cursor.Mapped[cursor.Physical++]!=ushort.MaxValue)cursor.Accepted++;}
            level.Position=level.Count=0;
            while(cursor.Physical<cursor.Count&&level.Count<128)
            {
                token.ThrowIfCancellationRequested();int index=cursor.Physical++;var r=cursor.Rows[index];var code=cursor.Mapped[index];
                if(code==ushort.MaxValue)continue;
                // Actual state0callback240010 versus normal parentNULL/C0false.
                byte targetFlags=(byte)((state==0?1:0)|((r.TargetByte7&1)<<1));
                ulong tag=r.TargetState|(1UL<<32)|((ulong)targetFlags<<40);
                ulong context=sourceNodes[r.SourceLabel];
                if(code==0xfffd?context!=0:context==0)throw new InvalidDataException("Missing versus live auxiliary context differs.");
                level.Rows[level.Count++]=new(tag,context,r.Cost,code,0,0,0);cursor.Accepted++;
            }
            Calls[CallCount++]=new(state,q.Tag,depth,q.CachedOwner!.Slot,q.AcceptedSkip,
                beforePhysical,beforeAccepted,cursor.Physical,cursor.Accepted,level.Count);
            checkpoint("after-backend-call");
        }
    }
}
