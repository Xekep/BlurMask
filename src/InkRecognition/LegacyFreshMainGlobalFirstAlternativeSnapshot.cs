namespace InkRecognition;

/// <summary>One fresh global main alternative after successful real callbacks.
/// This is a post-callback snapshot, not native allocation/failure ordering,
/// duplicate exclusion, quote normalization or an accepted OCR result.</summary>
public sealed class LegacyFreshMainGlobalFirstAlternativeSnapshot
{
    private readonly byte[] allocated, scored, normalized, selectedBeam;
    public LegacyFreshMainGlobalTerminalResult Terminal { get; }
    public LegacyFreshMainGlobalGlyphExecutionSession Session { get; }
    public LegacyFreshMainGlobalWordControllerPolicy SourcePolicy => Session.Ownership.Materialization.TerminalResult.PassResult.Policy;
    public LegacyNativePublishedWordNode? WordHead18 { get; }
    public LegacyFreshMainGlobalCompactWordMaterialization BeamOwner20 => Session.Ownership.Materialization;
    public int SelectedBeamOrdinal20 => Terminal.Winner.Ordinal;
    public int SourceNodeCount2310 { get; }
    public long Reference78 { get; }
    public int ActiveAlternativeCount60 => 1;
    public int AllocatedAlternativeCapacity70 => 10;
    public float FreshTop80 => -1f;
    public float NormalizedTop80 { get; }
    public float RawFirstMass08 { get; }
    public int PublishedWordCount { get; }

    private LegacyFreshMainGlobalFirstAlternativeSnapshot(LegacyFreshMainGlobalTerminalResult terminal,
        LegacyFreshMainGlobalGlyphExecutionSession session, LegacyNativePublishedWordNode? head,
        int count, long reference, int words, byte[] allocated, byte[] scored,
        byte[] normalized, float mass, float top80, byte[] selectedBeam)
    {
        Terminal = terminal; Session = session; WordHead18 = head;
        SourceNodeCount2310 = count; Reference78 = reference; PublishedWordCount = words;
        this.allocated = allocated; this.scored = scored; this.normalized = normalized;
        RawFirstMass08 = mass; NormalizedTop80 = top80; this.selectedBeam = selectedBeam;
    }

    public byte[] CopyConstructorStorage400() => allocated.ToArray();
    public byte[] CopyScoredStorage400() => scored.ToArray();
    public byte[] CopyNormalizedScoreStorage400() => normalized.ToArray();
    public byte[] CopySelectedBeam88() => selectedBeam.ToArray();

    /// <summary>Requires exact completed callback tokens in native order.
    /// Pointer fields0/18/20 are zero portable placeholders; actual VT policy,
    /// linked WORD head and beam owner/ordinal are retained as typed identities.
    /// One actual400-byte allocation has its first40 bytes constructed in-place.</summary>
    public static LegacyFreshMainGlobalFirstAlternativeSnapshot Capture(
        LegacyFreshMainGlobalTerminalResult terminal, LegacyFreshMainGlobalGlyphExecutionSession session,
        IReadOnlyList<LegacyFreshMainGlobalGlyphExecutionResult> publications,
        ReadOnlySpan<byte> actualAlternativeAllocation400, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(terminal); ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(publications); token.ThrowIfCancellationRequested();
        if (actualAlternativeAllocation400.Length != 400)
            throw new ArgumentException("One explicit native first273.V8 allocation of ten40-byte rows required.");
        var ownership = session.Ownership;
        terminal.PassResult.ValidateResultQueryLease(token);
        var policy = ownership.Materialization.TerminalResult.PassResult.Policy;
        var pass = terminal.PassResult;
        if (!ReferenceEquals(ownership.Materialization.TerminalResult, terminal) ||
            !ReferenceEquals(pass.Policy, policy) ||
            !ReferenceEquals(policy.Owner.Graph, ownership.Geometry.Graph) ||
            !ReferenceEquals(ownership.Geometry.Policy, policy))
            throw new ArgumentException("Terminal, actual callbacks, source graph and result pool must have identical ownership.");
        if (!terminal.Winner.HasWinner || !session.IsComplete || session.IsStopped ||
            publications.Count != session.CompletedFlushCount || publications.Count != session.Associations.Plan.Flushes.Count)
            throw new InvalidOperationException("A valid first terminal and its complete successful actual callback prefix required.");
        if (policy.Owner.Model.SourceKind != 0x135 || policy.HasAssociatedSource ||
            policy.Configuration.Mode22f6 != 0 ||
            policy.Configuration.SecondaryAlternativeCount2404 != 1 ||
            policy.Configuration.Trace2368 != 0 || policy.SecondaryScaleEc != 0 ||
            (policy.Configuration.PackedSource22e8 & 0x20) != 0)
            throw new NotSupportedException("Only the checked fresh global kind0x135/no-rescue plain main alternative is restored.");

        int sourceCount = pass.NodeCount;
        if (sourceCount <= 0 || sourceCount != policy.Owner.Graph.State.NodeCount)
            throw new ArgumentException("Actual controller2310 count must cover the identical completed source graph.");
        var expectedHead = ownership.WordPublisher.Head;
        var walk = expectedHead;
        int wordCount = 0;
        //The list prepends. Reject extra external publication or a foreign,
        //reordered, repeated or missing completion token, rather than adopting it.
        for (int ordinal = publications.Count - 1; ordinal >= 0; ordinal--)
        {
            token.ThrowIfCancellationRequested();
            var publication = publications[ordinal];
            if (publication is null || !publication.Succeeded ||
                !ReferenceEquals(publication.ExecutionSession, session) ||
                !ReferenceEquals(publication.Association.Session, session.Associations) ||
                publication.Association.Ordinal != ordinal)
                throw new ArgumentException("Every actual same-session successful callback token is required in native order.");
            if (publication.PublishedWord is not { } published) continue;
            if (!ReferenceEquals(walk, published) || !ReferenceEquals(published.Word.Pool, ownership.GlyphPool))
                throw new ArgumentException("The current published WORD list must consist exactly of these actual callbacks.");
            walk = walk.Next; wordCount++;
        }
        if (walk is not null || !ReferenceEquals(ownership.WordPublisher.Head, expectedHead))
            throw new ArgumentException("Extra publisher nodes do not belong to this completed callback sequence.");

        //Fresh policy is NEW20E/fresh00C860, with no property/setter input:
        //qword98 clears99; qwordA4 clearsA8. First valid output setsreference78.
        long reference = LegacyPreparedResultReferencePolicy.UpdateReferenceCost(0,
            terminal.Winner.TotalCost, pass.FinalCumulativeCost, true, false, false, token);
        if (reference != terminal.Winner.TotalCost)
            throw new InvalidOperationException("The selected first native terminal must establish its own fresh reference.");
        byte[] allocation = actualAlternativeAllocation400.ToArray();
        LegacyOwnedMainFirstAlternativeSnapshot.ConstructRow40(allocation.AsSpan(0, 40), token);
        var input = new LegacyMainResultScoreInput(terminal.Winner.TotalCost, checked((int)reference),
            sourceCount, terminal.Winner.RowCost14, policy.Owner.Model.SourceKind);
        byte[] scored = allocation.ToArray();
        LegacyMainResultScore.WriteCostChannels(scored.AsSpan(0, 40), input, token).CopyTo(scored, 0);
        var output = LegacyMainResultScorePublication.Normalize(scored, 1, -1f, token);
        //Native2271F0 selects after207CB0. The Terminal object also retains
        //a prepublication diagnostic row; its old6/22 bytes are not row20.
        var actualFinalNode = ownership.Materialization.GetNodeSnapshot(sourceCount - 1);
        byte[] beam = actualFinalNode.Records.Span.Slice(checked(terminal.Winner.Ordinal * 88), 88).ToArray();
        if (beam.Length != 88) throw new InvalidDataException("The actual selected native beam row must retain its complete88 bytes.");
        pass.ValidateResultQueryLease(token);
        token.ThrowIfCancellationRequested();
        return new(terminal, session, expectedHead, sourceCount, reference, wordCount, allocation,
            scored, output.CopyStorage(), output.RawFirstMass08!.Value, output.Top80, beam);
    }

}
