using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Actual global21FFD0 numeric append events, retaining the selected
/// compact pool and real source callback owner. Not finalized glyphs or text.</summary>
public sealed class LegacyFreshMainGlobalGlyphBacktrackRows
{
    private readonly LegacyPreparedMainGlyphBacktrackRows.Result result;
    internal LegacyGlobalSelectedCompactOwner CompactOwner { get; }
    public LegacyFreshMainGlobalCompactWordMaterialization Materialization => CompactOwner.RequireOld();
    public LegacyFreshMainGlobalPublishedTerminalResult PublishedTerminal => CompactOwner.RequirePublished();
    public LegacyFreshMainGlobalSourceSymbolCallbacks SourceCallbacks { get; }
    public int Count => result.Rows.Length / 72;
    private LegacyFreshMainGlobalGlyphBacktrackRows(LegacyGlobalSelectedCompactOwner materialization,
        LegacyFreshMainGlobalSourceSymbolCallbacks callbacks, LegacyPreparedMainGlyphBacktrackRows.Result result)
    { CompactOwner = materialization; SourceCallbacks = callbacks; this.result = result; }

    public static LegacyFreshMainGlobalGlyphBacktrackRows Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalCompactWordMaterialization materialization,
        LegacyFreshMainGlobalSourceSymbolCallbacks callbacks, ReadOnlySpan<byte> stackRowResidue72,
        CancellationToken token = default)
        => PrepareCore(resource, LegacyGlobalSelectedCompactOwner.FromOld(materialization), callbacks, stackRowResidue72, token);

    public static LegacyFreshMainGlobalGlyphBacktrackRows PreparePublished(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalPublishedTerminalResult selected, LegacyFreshMainGlobalSourceSymbolCallbacks callbacks,
        ReadOnlySpan<byte> stackRowResidue72, CancellationToken token = default)
        => PrepareCore(resource, LegacyGlobalSelectedCompactOwner.FromPublished(selected, token), callbacks, stackRowResidue72, token);

    private static LegacyFreshMainGlobalGlyphBacktrackRows PrepareCore(ReadOnlySpan<byte> resource,
        LegacyGlobalSelectedCompactOwner materialization, LegacyFreshMainGlobalSourceSymbolCallbacks callbacks,
        ReadOnlySpan<byte> stackRowResidue72, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(materialization); ArgumentNullException.ThrowIfNull(callbacks);
        token.ThrowIfCancellationRequested();
        var pass = materialization.Pass; var policy = pass.Policy;
        if (stackRowResidue72.Length != 72 || !ReferenceEquals(callbacks.Geometry.Policy, policy) ||
            !ReferenceEquals(callbacks.Geometry.Confidence, materialization.Confidence))
            throw new ArgumentException("The real global compact, source callback and complete72-byte stack residue must share their owner.");
        if (resource.Length != 572763 || !Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(resource)).Equals(
            policy.Owner.Graph.Source.Source.SourceFingerprint, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The exact selected acoustic resource is required.");
        if (policy.Configuration.Trace2368 != 0 || policy.Configuration.Mode22f6 != 0 || policy.Refresh.KnowledgeOption22f8 != 0)
            throw new NotSupportedException("Fresh nontracing ordinary global backtracking required.");
        pass.ValidateResultQueryLease(token);
        byte[] endpoint = new byte[64];
        BinaryPrimitives.WriteInt32LittleEndian(endpoint, -1);
        BinaryPrimitives.WriteUInt16LittleEndian(endpoint.AsSpan(4), ushort.MaxValue);
        BinaryPrimitives.WriteInt32LittleEndian(endpoint.AsSpan(8), -1);
        BinaryPrimitives.WriteUInt16LittleEndian(endpoint.AsSpan(12), ushort.MaxValue);
        BinaryPrimitives.WriteUInt64LittleEndian(endpoint.AsSpan(16), ulong.MaxValue);
        BinaryPrimitives.WriteInt32LittleEndian(endpoint.AsSpan(24), pass.NodeCount);
        BinaryPrimitives.WriteUInt64LittleEndian(endpoint.AsSpan(28), ulong.MaxValue);
        BinaryPrimitives.WriteUInt16LittleEndian(endpoint.AsSpan(36), ushort.MaxValue);
        BinaryPrimitives.WriteUInt64LittleEndian(endpoint.AsSpan(40), 1);
        var initial = callbacks.ReadDelimiter(endpoint, 0, token);
        if (initial.RemainingCursor != 0 || initial.CopySymbols().Length != 0 || initial.NativeFlag0C != 0 || initial.NativeFlag0D != 0)
            throw new NotSupportedException("Initial synthetic delimiter append requires its separate native path.");
        var rows = LegacyPreparedMainGlyphBacktrackRows.Run(LegacyOrdinaryRecognitionGraph.FromGlobal(policy.Owner.Graph),
            policy.Owner.Model.AcousticAlphabet, BinaryPrimitives.ReadUInt16LittleEndian(resource[105..]),
            materialization.CopyCompactStorage(), materialization.CopyNodeStarts2478(), materialization.RecordCount,
            materialization.SelectedRecordIndex, ushort.MaxValue, stackRowResidue72, token);
        pass.ValidateResultQueryLease(token);
        return new(materialization, callbacks, rows);
    }

    public byte[] CopyRows() => result.Rows.ToArray();
    public int[] CopyVisitedCompactIndices() => result.Visited.ToArray();
    public ulong[] CopyResidualSourceReferences() => result.Residuals.ToArray();
}
