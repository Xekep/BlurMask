namespace InkRecognition;

/// <summary>One global21FFD0 callback packet. Nonnegative origin entries are
/// dependencies on the initial WORD120 seed; they are not invented zero bytes.</summary>
public sealed class LegacyFreshMainGlobalGlyphFlush
{
    private readonly LegacyPreparedMainGlyphFlushPlan.Flush value;
    public LegacyFreshMainGlobalGlyphBacktrackRows BacktrackRows { get; }
    public int CurrentCompactIndex => value.Current;
    public int FilteredCompactIndex => value.Filtered;
    public bool ProtocolFlush => value.Protocol;
    public int RowCount => value.Rows.Length / 72;
    internal LegacyFreshMainGlobalGlyphFlush(LegacyFreshMainGlobalGlyphBacktrackRows backtrack,
        LegacyPreparedMainGlyphFlushPlan.Flush value) { BacktrackRows = backtrack; this.value = value; }
    public byte[] CopyRows72() => value.Rows.ToArray();
    public byte[] CopyPacketWriteTemplate() => value.Template.ToArray();
    public int[] CopyInitialWordSeedOrigins() => value.Origins.ToArray();
    public ulong[] CopyResidualAdditions() => value.Additions.ToArray();
}

/// <summary>Actual global compact-to-flush numeric planning and source70
/// metadata reads. Association, callback success and WORD publication remain
/// separate; no accepted text or completed callback is synthesized.</summary>
public sealed class LegacyFreshMainGlobalGlyphFlushPlan
{
    public LegacyFreshMainGlobalGlyphBacktrackRows BacktrackRows { get; }
    public IReadOnlyList<LegacyFreshMainGlobalGlyphFlush> Flushes { get; }
    public IReadOnlyList<LegacyMainGlyphSourceMetadataEvent> SourceMetadataEvents { get; }
    private LegacyFreshMainGlobalGlyphFlushPlan(LegacyFreshMainGlobalGlyphBacktrackRows backtrack,
        LegacyPreparedMainGlyphFlushPlan.Result numeric)
    {
        BacktrackRows = backtrack;
        Flushes = Array.AsReadOnly(numeric.Flushes.Select(f => new LegacyFreshMainGlobalGlyphFlush(backtrack, f)).ToArray());
        SourceMetadataEvents = Array.AsReadOnly(numeric.Events);
    }

    public static LegacyFreshMainGlobalGlyphFlushPlan Prepare(LegacyFreshMainGlobalGlyphBacktrackRows backtrack,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(backtrack); token.ThrowIfCancellationRequested();
        var materialization = backtrack.Materialization; var pass = materialization.TerminalResult.PassResult;
        var policy = pass.Policy; uint[] headers = policy.Owner.Symbols.CopySourceHeaders();
        if (!ReferenceEquals(backtrack.SourceCallbacks.Geometry.Policy, policy) ||
            !ReferenceEquals(backtrack.SourceCallbacks.Geometry.Confidence, materialization.ConfidenceOwner))
            throw new ArgumentException("One genuine global compact/backtrack/source owner required.");
        if (policy.Configuration.Trace2368 != 0 || policy.Configuration.Mode22f6 != 0 ||
            policy.Refresh.KnowledgeOption22f8 != 0 || headers.Length != 2 || headers[0] != 0 ||
            headers[1] != policy.Owner.Symbols.Descriptor.Flags || (headers[1] & 0x1e00) != 0xc00)
            throw new NotSupportedException("Fresh unmodified ordinary global source1/class6 flush path required.");
        pass.ValidateResultQueryLease(token);
        var numeric = LegacyPreparedMainGlyphFlushPlan.Run(LegacyOrdinaryRecognitionGraph.FromGlobal(policy.Owner.Graph),
            policy.Owner.Model.AcousticAlphabet, materialization.CopyCompactStorage(), backtrack.CopyRows(),
            materialization.CopyNodeStarts2478(), backtrack.CopyVisitedCompactIndices(), pass.CopyCumulativePathCosts(),
            materialization.RecordCount, materialization.SelectedRecordIndex, ushort.MaxValue, materialization.TerminalResult.Winner,
            (tag, symbol, cancellation) => pass.ReadResultTerminal(tag, symbol, 0, cancellation), token);
        pass.ValidateResultQueryLease(token);
        return new(backtrack, numeric);
    }
}
