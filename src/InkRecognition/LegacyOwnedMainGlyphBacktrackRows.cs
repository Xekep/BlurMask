using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Actual21FFD0 numeric append events from an owned compact winner.
/// Row72 padding remains explicit stack residue. Word-boundary callbacks,
/// residual217DB0 association and218560 chain publication are separate.</summary>
public sealed class LegacyOwnedMainGlyphBacktrackRows
{
    private readonly byte[] rows;
    private readonly int[] visited;
    private readonly ulong[] residuals;
    internal LegacyMainSelectedCompactWordOwner SelectedOwner {get;}
    public LegacyOwnedMainCompactWordMaterialization Materialization => SelectedOwner.Single ??
        throw new InvalidOperationException("Configured secondary backtracking has no count1 materialization.");
    public LegacyOwnedMainSecondaryCompactTerminal? SecondaryTerminal => SelectedOwner.Secondary;
    public LegacyOwnedFreshMainWordControllerPolicy ControllerOwner => SelectedOwner.Controller;
    public LegacyOwnedRussianMainGlyphConfidence ConfidenceOwner => SelectedOwner.ConfidenceOwner;
    public LegacyFreshCaseMainSourcePolicy? CaseSourcePolicy => SelectedOwner.CaseSourcePolicy;
    public int Count=>rows.Length/72;
    public byte[] CopyRows()=>rows.ToArray();
    public int[] CopyVisitedCompactIndices()=>visited.ToArray();
    public ulong[] CopyResidualSourceReferences()=>residuals.ToArray();
    private LegacyOwnedMainGlyphBacktrackRows(LegacyMainSelectedCompactWordOwner materialization,
        byte[] rows,int[] visited,ulong[] residuals)
    {SelectedOwner=materialization;this.rows=rows;this.visited=visited;this.residuals=residuals;}

    /// <summary>Walk the real numeric terminal and materialized2460/2478 owners.
    /// Supported ordinary graph subtypes0..3 retain native marker skipping,
    /// independent filtered lookahead, state carry and216610 key production.
    /// This returns append events, not flattened word segments or accepted text.</summary>
    public static LegacyOwnedMainGlyphBacktrackRows Prepare(ReadOnlySpan<byte> acousticResource,
        LegacyOwnedMainCompactWordMaterialization materialization,
        LegacyOwnedRussianMainSourceSymbolCallbacks sourceCallbacks,
        ReadOnlySpan<byte> stackRowResidue72,CancellationToken token=default)
        =>PrepareCore(acousticResource,null,materialization,sourceCallbacks,stackRowResidue72,token);

    /// <summary>The same numeric append algorithm over the genuine fresh292
    /// CASE compact owner. Exact retained factory-policy identity is required;
    /// no DIRECT dictionary or prepared winner substitutes for that owner.</summary>
    public static LegacyOwnedMainGlyphBacktrackRows Prepare(ReadOnlySpan<byte> acousticResource,
        LegacyFreshCaseMainSourcePolicy caseSourcePolicy,
        LegacyOwnedMainCompactWordMaterialization materialization,
        LegacyOwnedRussianMainSourceSymbolCallbacks sourceCallbacks,
        ReadOnlySpan<byte> stackRowResidue72,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(caseSourcePolicy);
        return PrepareCore(acousticResource,caseSourcePolicy,materialization,sourceCallbacks,stackRowResidue72,token);
    }

    private static LegacyOwnedMainGlyphBacktrackRows PrepareCore(ReadOnlySpan<byte> acousticResource,
        LegacyFreshCaseMainSourcePolicy? caseSourcePolicy,
        LegacyOwnedMainCompactWordMaterialization materialization,
        LegacyOwnedRussianMainSourceSymbolCallbacks sourceCallbacks,
        ReadOnlySpan<byte> stackRowResidue72,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(materialization);ArgumentNullException.ThrowIfNull(sourceCallbacks);
        return PrepareSelected(acousticResource,caseSourcePolicy,LegacyMainSelectedCompactWordOwner.FromSingle(materialization),
            sourceCallbacks,stackRowResidue72,token);
    }

    public static LegacyOwnedMainGlyphBacktrackRows PrepareConfigured(ReadOnlySpan<byte> acousticResource,
        LegacyOwnedMainSecondaryCompactTerminal terminal,LegacyOwnedRussianMainSourceSymbolCallbacks sourceCallbacks,
        ReadOnlySpan<byte> stackRowResidue72,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        return PrepareSelected(acousticResource,terminal.Materialization.PassResult.SourcePolicy,
            LegacyMainSelectedCompactWordOwner.FromSecondary(terminal),sourceCallbacks,stackRowResidue72,token);
    }

    private static LegacyOwnedMainGlyphBacktrackRows PrepareSelected(ReadOnlySpan<byte> acousticResource,
        LegacyFreshCaseMainSourcePolicy? caseSourcePolicy,LegacyMainSelectedCompactWordOwner materialization,
        LegacyOwnedRussianMainSourceSymbolCallbacks sourceCallbacks,ReadOnlySpan<byte> stackRowResidue72,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(sourceCallbacks);token.ThrowIfCancellationRequested();
        var policy=materialization.Controller;
        var owner=policy.Owner;var confidence=materialization.ConfidenceOwner;
        bool caseAware=caseSourcePolicy is not null;
        if(stackRowResidue72.Length!=72||!ReferenceEquals(sourceCallbacks.Owner,confidence)||
            !ReferenceEquals(confidence.Graph,owner.SourceGraph)||!ReferenceEquals(confidence.Model,owner.CommonModel)||
            (caseAware? !owner.IsCaseAware||!ReferenceEquals(materialization.CaseSourcePolicy,caseSourcePolicy)||
                !ReferenceEquals(caseSourcePolicy!.Controller,policy)||
                !ReferenceEquals(confidence.CaseDictionaryOwner,caseSourcePolicy.DictionaryOwner):
                owner.IsCaseAware||materialization.CaseSourcePolicy is not null||
                !ReferenceEquals(confidence.DirectDictionaryOwner,owner.DictionaryOwner)))
            throw new ArgumentException("Same actual compact/source/confidence/factory and complete72-byte stack residue required.");
        if(acousticResource.Length!=572763||!StringComparer.OrdinalIgnoreCase.Equals(
            Convert.ToHexString(SHA256.HashData(acousticResource)),owner.SelectedSourceOwner.SourceFingerprint)||
            policy.Configuration.Trace2368!=0||policy.Configuration.Mode22f6!=0||
            policy.Refresh.KnowledgeOption22f8!=0)
            throw new NotSupportedException("Actual fresh unmodified Russian nontracing ordinary source required.");
        var graph=owner.SourceGraph;var alphabet=owner.CommonModel.AcousticAlphabet;
        ushort unit=BinaryPrimitives.ReadUInt16LittleEndian(acousticResource[105..]);
        //21FFD0 always calls frontend1A0 before its loop with capacity0.
        //The actual owned source callback supplies both zero result flags;
        //no boolean or manufactured delimiter result is accepted here.
        byte[] endpoint=new byte[64];W32(endpoint,0,-1);W16(endpoint,4,ushort.MaxValue);
        W32(endpoint,8,-1);W16(endpoint,12,ushort.MaxValue);W64(endpoint,16,ulong.MaxValue);
        W32(endpoint,24,graph.NodeCount);W64(endpoint,28,ulong.MaxValue);W16(endpoint,36,ushort.MaxValue);W64(endpoint,40,1);
        var initial=sourceCallbacks.ReadDelimiter(endpoint,0,token);
        if(initial.RemainingCursor!=0||initial.EmittedCount!=0||initial.NativeFlag0C!=0||initial.NativeFlag0D!=0)
            throw new NotSupportedException("Initial synthetic delimiter row requires its additional native path.");
        var result=LegacyPreparedMainGlyphBacktrackRows.Run(LegacyOrdinaryRecognitionGraph.FromSameSource(graph),alphabet,unit,
            materialization.CopyCompactStorage(),materialization.CopyNodeStarts2478(),materialization.RecordCount,
            materialization.SelectedRecordIndex,materialization.SelectedAlternative,stackRowResidue72,token);
        return new(materialization,result.Rows,result.Visited,result.Residuals);
    }

    private static (int,ushort) Reference(ReadOnlySpan<byte> row)
    {ulong key=BinaryPrimitives.ReadUInt64LittleEndian(row[8..]);return ((unchecked((int)key)<<8)>>8,(ushort)(key>>24));}
    private static ulong Pack(int node,ushort arc)=>((uint)node&0xffffffUL)|((ulong)arc<<24);
    private static ushort U16(ReadOnlySpan<byte> bytes,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(bytes[at..]);
    private static int I32(ReadOnlySpan<byte> bytes,int at)=>BinaryPrimitives.ReadInt32LittleEndian(bytes[at..]);
    private static void W16(Span<byte> bytes,int at,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(bytes[at..],value);
    private static void W32(Span<byte> bytes,int at,int value)=>BinaryPrimitives.WriteInt32LittleEndian(bytes[at..],value);
    private static void W64(Span<byte> bytes,int at,ulong value)=>BinaryPrimitives.WriteUInt64LittleEndian(bytes[at..],value);
}
