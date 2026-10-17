using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>One-shot ordinary global207CB0 compact publication from the actual
/// selected pass and its cached acoustic owner. This is not glyph/text output.</summary>
public sealed class LegacyFreshMainGlobalCompactWordMaterialization
{
    private readonly LegacyPreparedMainCompactPublication.Result result;
    public LegacyFreshMainGlobalTerminalResult TerminalResult { get; }
    public LegacyFreshMainGlobalCompactReachability Reachability { get; }
    public LegacyFreshMainGlobalGlyphConfidence ConfidenceOwner { get; }
    public int RecordCount => result.Count;
    public int SelectedRecordIndex { get; }

    private LegacyFreshMainGlobalCompactWordMaterialization(LegacyFreshMainGlobalTerminalResult terminal,
        LegacyFreshMainGlobalCompactReachability reachability, LegacyFreshMainGlobalGlyphConfidence confidence,
        LegacyPreparedMainCompactPublication.Result result, int selected)
    {
        TerminalResult = terminal; Reachability = reachability; ConfidenceOwner = confidence;
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
        if (policy.Configuration.HeaderCount2400 != 2 || pass.NodeCount > policy.MaximumParentDistance54)
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
            pass.ValidateResultQueryLease, token);
        int selected = checked(published.Starts[pass.NodeCount - 1] + BinaryPrimitives.ReadUInt16LittleEndian(
            published.Nodes[^1].Records.Span.Slice(checked(terminal.Winner.Ordinal * 88 + 0x22), 2)));
        if ((uint)selected >= (uint)published.Count)
            throw new InvalidDataException("Selected global row22 does not name the published compact pool.");
        return new(terminal, reachable, confidence, published, selected);
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
