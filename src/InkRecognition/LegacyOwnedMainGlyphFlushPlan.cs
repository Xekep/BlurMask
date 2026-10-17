using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("LegacyMainGlyphFlush.Probe")]

namespace InkRecognition;

/// <summary>A genuine21FFD0 flush, before its callback. Packet template bytes
/// with a nonnegative seed origin are dependencies on initial WORD276 storage,
/// not zero-valued metadata. Association/publication are separate consumers.</summary>
public sealed class LegacyOwnedMainGlyphFlush
{
    private readonly byte[] rows, template;
    private readonly int[] origins;
    private readonly ulong[] residualAdditions;
    public LegacyOwnedMainGlyphBacktrackRows BacktrackRows {get;}
    public int CurrentCompactIndex {get;}
    public int FilteredCompactIndex {get;}
    public bool ProtocolFlush {get;}
    public int RowCount=>rows.Length/72;
    internal LegacyOwnedMainGlyphFlush(LegacyOwnedMainGlyphBacktrackRows owner,int current,int filtered,
        bool protocol,byte[] rows,byte[] template,int[] origins,ulong[] additions)
    {BacktrackRows=owner;CurrentCompactIndex=current;FilteredCompactIndex=filtered;ProtocolFlush=protocol;
        this.rows=rows;this.template=template;this.origins=origins;residualAdditions=additions;}
    public byte[] CopyRows72()=>rows.ToArray();
    public byte[] CopyPacketWriteTemplate()=>template.ToArray();
    /// <summary>Minus-one means the template byte is known. Other entries
    /// are exact offsets in the initial WORD120 seed, not native pointers.</summary>
    public int[] CopyInitialWordSeedOrigins()=>origins.ToArray();
    /// <summary>New residual references since the preceding flush. A real
    /// per-flush217DB0 consumer must also carry its unconsumed old references.</summary>
    public ulong[] CopyResidualAdditions()=>residualAdditions.ToArray();
}

public sealed class LegacyMainGlyphSourceMetadataEvent
{
    private readonly byte[] metadata;
    public int CompactIndex {get;}
    public ulong FullSourceTag {get;}
    public ushort RawSymbol {get;}
    internal LegacyMainGlyphSourceMetadataEvent(int index,ulong tag,ushort symbol,byte[] bytes)
    {CompactIndex=index;FullSourceTag=tag;RawSymbol=symbol;metadata=bytes;}
    public byte[] CopyMetadata16()=>metadata.ToArray();
}

/// <summary>Owned ordinary CASE21FFD0 flush selection and scalar/control
/// writes. The initial WORD seed, callback failure,217DB0 association and
///21FE30 publication remain separate. This is not a flattened text result.</summary>
public sealed class LegacyOwnedMainGlyphFlushPlan
{
    private readonly LegacyOwnedMainGlyphFlush[] flushes;
    private readonly LegacyMainGlyphSourceMetadataEvent[] metadataEvents;
    public LegacyOwnedMainGlyphBacktrackRows BacktrackRows {get;}
    public LegacyFreshCaseMainSourcePolicy SourcePolicy {get;}
    public IReadOnlyList<LegacyOwnedMainGlyphFlush> Flushes {get;}
    public IReadOnlyList<LegacyMainGlyphSourceMetadataEvent> SourceMetadataEvents {get;}
    private LegacyOwnedMainGlyphFlushPlan(LegacyOwnedMainGlyphBacktrackRows rows,
        LegacyFreshCaseMainSourcePolicy policy,LegacyOwnedMainGlyphFlush[] flushes,
        LegacyMainGlyphSourceMetadataEvent[] events)
    {BacktrackRows=rows;SourcePolicy=policy;this.flushes=flushes;metadataEvents=events;
        Flushes=Array.AsReadOnly(flushes);SourceMetadataEvents=Array.AsReadOnly(events);}

    public static LegacyOwnedMainGlyphFlushPlan Prepare(LegacyFreshCaseMainSourcePolicy policy,
        LegacyOwnedMainGlyphBacktrackRows backtrack,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(backtrack);
        if(backtrack.SecondaryTerminal is not null)
            throw new NotSupportedException("Configured flush planning requires PrepareConfigured.");
        return PrepareSelected(policy,backtrack,token);
    }

    public static LegacyOwnedMainGlyphFlushPlan PrepareConfigured(LegacyOwnedMainGlyphBacktrackRows backtrack,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(backtrack);
        if(backtrack.SecondaryTerminal is null||backtrack.CaseSourcePolicy is null)
            throw new ArgumentException("Actual configured CASE selected-terminal backtracking required.");
        return PrepareSelected(backtrack.CaseSourcePolicy,backtrack,token);
    }

    private static LegacyOwnedMainGlyphFlushPlan PrepareSelected(LegacyFreshCaseMainSourcePolicy policy,
        LegacyOwnedMainGlyphBacktrackRows backtrack,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(policy);ArgumentNullException.ThrowIfNull(backtrack);
        token.ThrowIfCancellationRequested();var materialization=backtrack.SelectedOwner;
        if(!ReferenceEquals(materialization.CaseSourcePolicy,policy)||
            !ReferenceEquals(materialization.Controller,policy.Controller)||
            !ReferenceEquals(materialization.ConfidenceOwner.CaseDictionaryOwner,policy.DictionaryOwner))
            throw new ArgumentException("One genuine CASE compact/backtrack/source owner required.");
        var controller=policy.Controller;var owner=controller.Owner;var graph=owner.SourceGraph;
        uint[] headers=owner.CopySourceHeaders();
        if(controller.Configuration.Trace2368!=0||controller.Configuration.Mode22f6!=0||
            controller.Refresh.KnowledgeOption22f8!=0||headers.Length!=2||headers[0]!=0||
            headers[1]!=policy.DictionaryOwner.Descriptor.Flags||(headers[1]&0x1e00)!=0xc00)
            throw new NotSupportedException("Fresh unmodified ordinary source1/class6 flush path required.");
        //20E230 resets25F8 and can append only after finding an A00 source.
        //Neither actual selected header is A00, so boundary-range override
        //cannot run. This is independent of the optional2558 enable value.
        var numeric=LegacyPreparedMainGlyphFlushPlan.Run(LegacyOrdinaryRecognitionGraph.FromSameSource(graph),owner.CommonModel.AcousticAlphabet,
            materialization.CopyCompactStorage(),backtrack.CopyRows(),materialization.CopyNodeStarts2478(),
            backtrack.CopyVisitedCompactIndices(),materialization.CopyCumulativePathCosts(),materialization.RecordCount,
            materialization.SelectedRecordIndex,materialization.SelectedAlternative,materialization.Winner,
            (tag,symbol,cancellation)=>policy.DictionaryOwner.Session.ReadTerminal(tag,symbol,0,cancellation),token);
        return new(backtrack,policy,numeric.Flushes.Select(f=>new LegacyOwnedMainGlyphFlush(backtrack,f.Current,f.Filtered,
            f.Protocol,f.Rows,f.Template,f.Origins,f.Additions)).ToArray(),numeric.Events);
    }

    //2210A6..221144: signed32 SUB/SHL wrap precedes CVTDQ2PS; DIVSS,
    //MULSS and their stores are separate. No exp/clamp/finite guard belongs
    //to this packet-writing stage, including zero denominators.
    internal static LegacyMainGlyphFlushCosts CalculateCosts(int total,int baseTotal,int field18,int base18,
        int baseNode,int filteredNode)
    {
        return LegacyPreparedMainGlyphFlushPlan.CalculateCosts(total,baseTotal,field18,base18,baseNode,filteredNode);
    }
    private static int ReferenceNode(ReadOnlySpan<byte> row)=>(unchecked((int)BinaryPrimitives.ReadUInt64LittleEndian(row[8..]))<<8)>>8;
    private static ulong Pack(int node,ushort arc)=>((uint)node&0xffffffUL)|((ulong)arc<<24);
    private static ushort U16(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(b[at..]);
    private static int I32(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private static void W16(Span<byte> b,int at,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(b[at..],value);
    private static void W32(Span<byte> b,int at,int value)=>BinaryPrimitives.WriteInt32LittleEndian(b[at..],value);
}

internal readonly record struct LegacyMainGlyphFlushCosts(uint LogMassBits,uint RelativeCostBits,uint RawField18DeltaBits);
