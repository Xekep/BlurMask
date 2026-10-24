using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>One-shot ordinary global207CB0 compact publication from the actual
/// selected pass and its cached acoustic owner. This is not glyph/text output.</summary>
public sealed class LegacyFreshMainGlobalCompactWordMaterialization
{
    private readonly LegacyPreparedMainCompactPublication.Result result;
    public LegacyFreshMainGlobalTerminalResult TerminalResult { get; }
    private readonly LegacyFreshMainGlobalCompactReachability? reachability;
    public LegacyFreshMainGlobalCompactReachability Reachability => reachability ??
        throw new InvalidOperationException("LIVE-appended publication owns its persistent global pool, not a one-shot reachability object.");
    public LegacyFreshMainGlobalGlyphConfidence ConfidenceOwner { get; }
    public int RecordCount => result.Count;
    public int SelectedRecordIndex { get; }

    private LegacyFreshMainGlobalCompactWordMaterialization(LegacyFreshMainGlobalTerminalResult terminal,
        LegacyFreshMainGlobalCompactReachability? reachability, LegacyFreshMainGlobalGlyphConfidence confidence,
        LegacyPreparedMainCompactPublication.Result result, int selected)
    {
        TerminalResult = terminal; this.reachability = reachability; ConfidenceOwner = confidence;
        this.result = result; SelectedRecordIndex = selected;
    }

    public static LegacyFreshMainGlobalCompactWordMaterialization Materialize(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalTerminalResult terminal, LegacyFreshMainGlobalGlyphConfidence confidence,
        ReadOnlySpan<byte> allocatorResidue40, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(terminal); ArgumentNullException.ThrowIfNull(confidence);
        token.ThrowIfCancellationRequested();
        var pass = terminal.PassResult; var policy = pass.Policy; var owner = policy.Owner;
        if (!ReferenceEquals(confidence.Owner, owner) || !ReferenceEquals(terminal.Invocation.Policy, policy))
            throw new ArgumentException("The selected global pass and cached candidate weights must share their actual owner.");
        if (policy.Configuration.HeaderCount2400 != 2 || pass.NodeCount > policy.MaximumParentDistance54 ||
            pass.IntermediatePublicationState is not null)
            throw new NotSupportedException("Fresh two-header global publication before the intermediate boundary required.");
        if (allocatorResidue40.IsEmpty || allocatorResidue40.Length % 40 != 0 || allocatorResidue40.Length > 64 * 1024 * 1024)
            throw new ArgumentException("Explicit complete compact40 allocator residue required.");
        var reachable = LegacyFreshMainGlobalCompactReachability.Prepare(resource, terminal, token);
        pass.BeginCompactPublication(token);
        var published = LegacyPreparedMainCompactPublication.Run(policy.Configuration,
            LegacyOrdinaryRecognitionGraph.FromGlobal(owner.Graph), owner.Contexts, owner.Model.AcousticAlphabet,
            owner.Symbols.Geometry.CategoryOwner.Categories, terminal.Invocation.Options.ContextSentinels24b8,
            terminal.Invocation.LanguagePenalty2534, false, 0, pass.NodeCount - 1,
            Enumerable.Range(0, pass.NodeCount).Select(reachable.GetNodeSnapshot).ToArray(),
            reachable.HasLanguageContinuation, group => LegacyMainCompactGroupScores.FromGlobal(confidence.ReadGroup(group, token)),
            allocatorResidue40, 0, new int[pass.NodeCount + 1], new int[pass.NodeCount + 1],
            pass.ValidateResultQueryLease, token, owner.Symbols.CopySourceHeaders());
        int selected = checked(published.Starts[pass.NodeCount - 1] + BinaryPrimitives.ReadUInt16LittleEndian(
            published.Nodes[^1].Records.Span.Slice(checked(terminal.Winner.Ordinal * 88 + 0x22), 2)));
        if ((uint)selected >= (uint)published.Count)
            throw new InvalidDataException("Selected global row22 does not name the published compact pool.");
        return new(terminal, reachable, confidence, published, selected);
    }

    /// <summary>Append after actual finalV80 to the SAME pool produced by the
    /// explicit LIVE pass. No intermediate reset, cloned source, replacement
    /// D0 map or acoustic evaluation is introduced.</summary>
    public static LegacyFreshMainGlobalCompactWordMaterialization MaterializeWithIntermediatePublication(
        ReadOnlySpan<byte> resource, LegacyFreshMainGlobalTerminalResult terminal,
        LegacyFreshMainGlobalGlyphConfidence confidence, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(terminal); token.ThrowIfCancellationRequested();
        var pass = terminal.PassResult; var policy = pass.Policy;
        LegacyFreshMainGlobalCompactPublicationState.ValidateOwner(resource, policy, confidence);
        if (!terminal.Winner.HasWinner || !ReferenceEquals(terminal.Invocation.Policy, policy))
            throw new ArgumentException("Actual selected global terminal required; a missing winner cannot be manufactured.");
        var state = pass.IntermediatePublicationState?.Copy() ??
            throw new ArgumentException("The producing global pass must bind the explicit LIVE pool producer.");
        if (state.NextNode >= pass.NodeCount) throw new InvalidOperationException("A remaining final append range required.");
        pass.ValidateResultQueryLease(token);
        //The SAME scheduler outer290 has already received its actual finalV80
        //reset. Preserve its identity and route reads through the captured final
        //lease; do not allocate/reset a substitute reader here.
        var reader = pass.RequirePublicationReader(token);
        pass.BeginCompactPublication(token);
        int evaluations = policy.Owner.AcousticEvaluations;
        var stage = state.Publish(pass.NodeCount - 1, pass.GetNodeSnapshot, reader, confidence, pass.ValidateResultQueryLease, token);
        if (policy.Owner.AcousticEvaluations != evaluations) throw new InvalidOperationException("Final append cannot repeat NN evaluation.");
        int selected = checked(stage.Publication.Starts[^2] + BinaryPrimitives.ReadUInt16LittleEndian(
            stage.Publication.Nodes[^1].Records.Span.Slice(terminal.Winner.Ordinal * 88 + 0x22, 2)));
        if ((uint)selected >= (uint)stage.Publication.Count) throw new InvalidDataException("Selected row22 must name the retained compact pool.");
        return new(terminal, null, confidence, stage.Publication, selected);
    }

    public byte[] CopyCompactStorage() => result.Storage.ToArray();
    public int[] CopyNodeStarts2478() => result.Starts.ToArray();
    public int[] CopyPrefixOwners2488() => result.PrefixOwners.ToArray();
    public LegacyMainDirectWordNodeResult GetNodeSnapshot(int node)
    {
        if ((uint)node >= (uint)result.Nodes.Length) throw new ArgumentOutOfRangeException(nameof(node));
        var value = result.Nodes[node];
        return value with { Records = value.Records.ToArray(), PrimaryAlternatives = value.PrimaryAlternatives.ToArray(),
            SecondaryAlternatives = value.SecondaryAlternatives.ToArray() };
    }
}
