using System.Buffers.Binary;
namespace InkRecognition;

public sealed class LegacyNativeCaseNormalQuery
{
    public required LegacyPreparedDictionaryQuery Request {get;init;}
    public ulong SourceTag {get;set;}
    internal LegacyNativeCaseNormalQuerySource? Owner;
    internal int SlotIndex;
}
public sealed record LegacyNativeCaseNormalQueryPage(int ReturnedCount,ReadOnlyMemory<byte> Records,
    ReadOnlyMemory<byte> ExtraCosts,int SlotIndex,int AcceptedCount,int BackendOffset);

/// <summary>Ordinary-frontend233090: eight query slots,128-row backend blocks,
/// exact skip counting and record32 case expansion. This is the normal query;
/// auxiliary24-row233FC0 and the controller source-selection factory are separate.</summary>
public sealed class LegacyNativeCaseNormalQuerySource
{
    private readonly LegacyAggregateDictionaryReader reader;
    private readonly Slot[] slots=Enumerable.Range(0,8).Select(_=>new Slot()).ToArray();
    private int nextSlot,active;
    private bool valid=true;
    private byte[] recordScratch=[],costScratch=[];
    public LegacyNativeCaseRecordExpansion Expansion {get;}
    public bool DutchLigatureMode {get;}
    public bool ExtraCostSideTable {get;}
    private sealed class Slot
    {
        public ulong Tag=ulong.MaxValue;
        public LegacyPreparedDictionaryQuery Inner=new();
        public int BlockCursor,BlockCount,Offset,Accepted;
        public byte[] Records=[],Costs=[];
    }
    private LegacyNativeCaseNormalQuerySource(LegacyNativeCaseRecordExpansion expansion,
        LegacyAggregateDictionaryReader reader,bool dutchLigatureMode,bool extraCosts)
    {Expansion=expansion;this.reader=reader;DutchLigatureMode=dutchLigatureMode;ExtraCostSideTable=extraCosts;}

    //231ED0 after inner V80 succeeds;232037 also clears8378 round-robin cursor.
    internal void ResetForControllerNode()
    {
        if(Interlocked.CompareExchange(ref active,1,0)!=0)throw new InvalidOperationException("Case queries must be serialized.");
        try
        {
            if(!valid)throw new InvalidOperationException("Failed case query requires a fresh session.");
            foreach(var slot in slots)
            {slot.Tag=ulong.MaxValue;slot.BlockCursor=slot.BlockCount=slot.Offset=slot.Accepted=0;slot.Records=[];slot.Costs=[];}
            nextSlot=0;
        }
        finally{Volatile.Write(ref active,0);}
    }

    /// <summary>The ordinary graph proves frontend280 false. The prepared
    /// dictionary and case tables must belong to the same selected source;
    /// locale/side-table fields remain explicit until20C9D0 ownership is wired.</summary>
    public static LegacyNativeCaseNormalQuerySource BindOrdinaryDictionary(
        LegacyNativeCaseRecordExpansion expansion,LegacyPreparedDictionaryQuerySource source,
        LegacyOrdinaryCursiveCandidateGraph graph,bool dutchLigatureMode,bool extraCosts)
        =>BindOwnedOrdinaryDictionary(expansion,source,LegacyOrdinaryRecognitionGraph.FromSameSource(graph),dutchLigatureMode,extraCosts);

    internal static LegacyNativeCaseNormalQuerySource BindOwnedOrdinaryDictionary(
        LegacyNativeCaseRecordExpansion expansion,LegacyPreparedDictionaryQuerySource source,
        LegacyOrdinaryRecognitionGraph graph,bool dutchLigatureMode,bool extraCosts)
    {
        ArgumentNullException.ThrowIfNull(expansion);ArgumentNullException.ThrowIfNull(source);
        var bound=LegacyOrdinaryDictionarySourceOwner.BindOwnedDictionary(source,expansion.Rewrite.Contexts,graph);
        return new(expansion,bound.Read,dutchLigatureMode,extraCosts);
    }

    /// <summary>233FC0's actual inner2B7.V58=05A3F0 returns0, so this bound
    /// dictionary has no24-byte continuation records. This is not a substitute
    /// for nonempty28C/provider continuation sources.</summary>
    public ReadOnlyMemory<byte> ReadAuxiliary24(ulong sourceTag,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(!valid)throw new InvalidOperationException("Failed case query requires a fresh session.");
        Expansion.Policy.ValidateState((byte)(sourceTag>>56)>>2&31);
        return ReadOnlyMemory<byte>.Empty;
    }

    public LegacyNativeCaseNormalQueryPage Read(LegacyNativeCaseNormalQuery query,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(query);ArgumentNullException.ThrowIfNull(query.Request);
        cancellationToken.ThrowIfCancellationRequested();
        if(Interlocked.CompareExchange(ref active,1,0)!=0)throw new InvalidOperationException("Case queries must be serialized.");
        try
        {
            if(!valid)throw new InvalidOperationException("Failed case query requires a fresh session.");
            var q=query.Request;var budget=q.PeriodicQuery;budget?.Allocate(256);budget?.Copy(256);budget?.Visit(128);
            if((uint)q.Node!=(uint)query.SourceTag || q.Skip is <0 or >2_000_000 || q.Capacity is <1 or >512)
                throw new ArgumentException("Owned source tag/node and bounded skip/capacity required.");
            int state=(byte)(query.SourceTag>>56)>>2&31;
            Expansion.Policy.ValidateState(state);
            if(!ReferenceEquals(query.Owner,this))
            {query.Owner=this;query.SlotIndex=nextSlot;nextSlot=(nextSlot+1)&7;slots[query.SlotIndex].Tag=ulong.MaxValue;}
            var slot=slots[query.SlotIndex];var inner=slot.Inner;
            if(slot.Tag!=query.SourceTag || slot.Accepted!=q.Skip || inner.Parameter1c!=q.Parameter1c ||
                inner.Parameter20!=q.Parameter20 || inner.Flags24!=q.Flags24 || inner.Flags26!=q.Flags26)
            {
                slot.Tag=query.SourceTag;slot.BlockCursor=slot.BlockCount=slot.Offset=slot.Accepted=0;
                slot.Records=[];slot.Costs=[];
                inner.Node=q.Node;inner.Category18=q.Category18;inner.Parameter19=q.Parameter19;
                inner.Parameter1c=q.Parameter1c;inner.Parameter20=q.Parameter20;
                inner.Flags24=q.Flags24;inner.Flags26=q.Flags26;
            }
            if(budget is not null)budget.Propagate(q,inner);
            inner.ContextRoot=q.ContextRoot;inner.ParentContext=q.ParentContext;
            int recordLength=q.Capacity*32,costLength=ExtraCostSideTable?q.Capacity*12:0;
            if(recordScratch.Length<recordLength){budget?.Allocate(recordLength);recordScratch=new byte[recordLength];}
            if(costScratch.Length<costLength){budget?.Allocate(costLength);costScratch=new byte[costLength];}
            //Only exact emitted prefixes are copied into returned owned pages.
            //Memory slices retain the original capacity even if a callback
            //mutates the public request while it is being serviced.
            Memory<byte> output=recordScratch.AsMemory(0,recordLength),costs=costScratch.AsMemory(0,costLength);
            int written=0;
            if(q.Capacity<=4)return Result(0);
            while(true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if(slot.BlockCursor==slot.BlockCount)
                {
                    slot.Offset=checked(slot.Offset+slot.BlockCount);inner.Skip=slot.Offset;inner.Capacity=128;
                    var before=(inner.Node,inner.Skip,inner.Capacity,inner.ContextRoot,inner.ParentContext,
                        inner.Parameter1c,inner.Parameter20,inner.Flags24,inner.Flags26,inner.Category18,inner.Parameter19);
                    // BoundDictionary.ReadWithContexts may create its token frame and two delegates for this actual backend fetch.
                    budget?.Allocate(256);budget?.Copy(256);budget?.Visit(256);var page=reader(inner,false,ExtraCostSideTable,cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    var after=(inner.Node,inner.Skip,inner.Capacity,inner.ContextRoot,inner.ParentContext,
                        inner.Parameter1c,inner.Parameter20,inner.Flags24,inner.Flags26,inner.Category18,inner.Parameter19);
                    if(before!=after)throw new InvalidDataException("Inner case source mutated request fields.");
                    int count=page.ReturnedCount;
                    if(count is < -1 or >128 || page.Records.Length!=Math.Max(0,count)*32 ||
                        (ExtraCostSideTable?page.ClearedExtraCosts.Length!=Math.Max(0,count)*12:!page.ClearedExtraCosts.IsEmpty))
                        throw new InvalidDataException("Inner case source block extent differs.");
                    slot.BlockCount=count;slot.BlockCursor=0;
                    if(count==-1){valid=false;return Result(-1);}
                    if(count==0)return Result(written);
                    budget?.Allocate(checked(page.Records.Length+page.ClearedExtraCosts.Length));budget?.Copy(checked(page.Records.Length+page.ClearedExtraCosts.Length));
                    slot.Records=page.Records.ToArray();slot.Costs=page.ClearedExtraCosts.ToArray();
                }
                while(slot.BlockCursor<slot.BlockCount && slot.Accepted<q.Skip)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    budget?.Visit(128);slot.Accepted=checked(slot.Accepted+Expansion.CountForSkip(slot.Records.AsSpan(slot.BlockCursor*32,32),state));
                    slot.BlockCursor++;
                }
                while(slot.BlockCursor<slot.BlockCount)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if(written>=q.Capacity-4)return Result(written);
                    int ordinal=slot.BlockCursor++;
                    //Expand max5 rows, List(5), record objects, read-only wrapper and closure; context views/inserts metered by factory.
                    budget?.Allocate(1024);budget?.Copy(2048);budget?.Visit(8192);var expanded=Expansion.Expand(slot.Records.AsSpan(ordinal*32,32),state,q.ContextRoot,q.ParentContext,
                        DutchLigatureMode,out int nextState,cancellationToken);
                    state=nextState;
                    foreach(var value in expanded)
                    {
                        if(written>=q.Capacity)throw new InvalidDataException("Native four-row slack exceeded.");
                        budget?.Copy(64);budget?.Visit(64);value.Record.Span.CopyTo(output.Span[(written*32)..]);
                        if(ExtraCostSideTable)
                        {
                            slot.Costs.AsSpan(ordinal*12,12).CopyTo(costs.Span[(written*12)..]);
                            var field=costs.Span.Slice(written*12+4,4);
                            BinaryPrimitives.WriteInt32LittleEndian(field,unchecked(BinaryPrimitives.ReadInt32LittleEndian(field)+value.CostDelta));
                        }
                        written++;slot.Accepted=checked(slot.Accepted+1);
                    }
                }
                if(written>=q.Capacity-4)return Result(written);
            }
            LegacyNativeCaseNormalQueryPage Result(int count)
            {long bytes=checked(Math.Max(0,count)*(ExtraCostSideTable?44L:32L));budget?.Allocate(64+bytes);budget?.Copy(bytes);return new(count,
                count<0?ReadOnlyMemory<byte>.Empty:output.Slice(0,count*32).ToArray(),
                !ExtraCostSideTable||count<0?ReadOnlyMemory<byte>.Empty:costs.Slice(0,count*12).ToArray(),
                query.SlotIndex,slot.Accepted,slot.Offset); }
        }
        catch{valid=false;throw;}
        finally{Volatile.Write(ref active,0);}
    }
}
