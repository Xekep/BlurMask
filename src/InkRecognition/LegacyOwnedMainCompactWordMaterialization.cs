using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Owned ordinary207CB0 compact storage and its real per-node2478
/// offsets. Typed DIRECT/CASE entrypoints retain their exact source owner;
/// this is not21FFD0 glyph/text output.</summary>
public sealed class LegacyOwnedMainCompactWordMaterialization
{
    private readonly byte[] storage;
    private readonly int[] starts;
    private readonly int[] prefixOwners;
    private readonly LegacyMainDirectWordNodeResult[] nodes;
    public LegacyOwnedMainTerminalResult TerminalResult { get; }
    public LegacyOwnedMainCompactWordReachability Reachability { get; }
    public LegacyOwnedRussianMainGlyphConfidence ConfidenceOwner { get; }
    public LegacyFreshCaseMainSourcePolicy? CaseSourcePolicy { get; }
    public int RecordCount { get; }
    public int SelectedRecordIndex { get; }
    public ushort SelectedAlternative => ushort.MaxValue;

    private LegacyOwnedMainCompactWordMaterialization(LegacyOwnedMainTerminalResult terminal,
        LegacyOwnedMainCompactWordReachability reachable, LegacyOwnedRussianMainGlyphConfidence confidence,
        byte[] storage, int count, int[] starts, int[] prefixOwners, LegacyMainDirectWordNodeResult[] nodes, int selected,
        LegacyFreshCaseMainSourcePolicy? caseSourcePolicy)
    {
        TerminalResult = terminal; Reachability = reachable; ConfidenceOwner = confidence;
        this.storage = storage; RecordCount = count; this.starts = starts; this.nodes = nodes;
        this.prefixOwners = prefixOwners;
        SelectedRecordIndex = selected;
        CaseSourcePolicy = caseSourcePolicy;
    }

    /// <summary>Fresh mode0/2/2/1 DIRECT route with no associated source or
    /// tracing. The residue is explicit complete compact40 allocator storage;
    /// no zeroed or native physical-allocation policy is inferred from it.</summary>
    public static LegacyOwnedMainCompactWordMaterialization Materialize(ReadOnlySpan<byte> resource,
        LegacyOwnedMainTerminalResult terminal, LegacyOwnedRussianMainGlyphConfidence confidence,
        ReadOnlySpan<byte> allocatorResidue40, CancellationToken cancellationToken = default)
        => MaterializeCore(resource, terminal, confidence, allocatorResidue40, null, cancellationToken);

    internal static LegacyOwnedMainCompactWordMaterialization MaterializeCase(ReadOnlySpan<byte> resource,
        LegacyFreshCaseMainSourcePolicy sourcePolicy, LegacyOwnedMainTerminalResult terminal,
        LegacyOwnedRussianMainGlyphConfidence confidence, ReadOnlySpan<byte> allocatorResidue40,
        CancellationToken cancellationToken)
        => MaterializeCore(resource, terminal, confidence, allocatorResidue40, sourcePolicy, cancellationToken);

    private static LegacyOwnedMainCompactWordMaterialization MaterializeCore(ReadOnlySpan<byte> resource,
        LegacyOwnedMainTerminalResult terminal, LegacyOwnedRussianMainGlyphConfidence confidence,
        ReadOnlySpan<byte> allocatorResidue40, LegacyFreshCaseMainSourcePolicy? casePolicy,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(terminal); ArgumentNullException.ThrowIfNull(confidence);
        cancellationToken.ThrowIfCancellationRequested();
        var pass = terminal.PassResult;
        if (pass.IntermediatePublicationState is null && pass.NodeCount > pass.ControllerOwner.MaximumParentDistance54)
            throw new NotSupportedException("Fresh one-shot publication requires a source before the native intermediate-publication boundary.");
        if (!terminal.Winner.HasWinner)
            throw new NotSupportedException("A real preselected numeric terminal is required.");
        ValidatePublicationOwner(resource, pass.ControllerOwner, casePolicy, confidence);
        var state = pass.IntermediatePublicationState?.Copy() ?? new LegacyMainCompactPublicationState(pass.ControllerOwner, allocatorResidue40);
        if (pass.IntermediatePublicationState is not null && !allocatorResidue40.SequenceEqual(state.InitialResidue))
            throw new ArgumentException("Final publication must retain the intermediate compact allocator owner.");
        pass.BeginCompactWordPublication(cancellationToken);
        var source = LegacyMainCompactPublicationSource.Final(resource, pass, casePolicy, state.NextNode, cancellationToken);
        var stage = Publish(resource, source, confidence, state, cancellationToken);
        int selected = state.Starts[pass.NodeCount - 1] + U16(stage.Nodes[^1].Records.Span, terminal.Winner.Ordinal * 88 + 0x22);
        if ((uint)selected >= state.Count) throw new InvalidDataException("Selected native row22 does not name the published compact pool.");
        return new(terminal, stage.Reachability, confidence, state.Storage.ToArray(), state.Count,
            state.Starts.ToArray(), state.PrefixOwners.ToArray(), stage.Nodes, selected, casePolicy);
    }

    internal sealed record PublicationStage(LegacyOwnedMainCompactWordReachability Reachability,
        LegacyMainDirectWordNodeResult[] Nodes);

    internal static void ValidatePublicationOwner(ReadOnlySpan<byte> resource,
        LegacyOwnedFreshMainWordControllerPolicy controller, LegacyFreshCaseMainSourcePolicy? casePolicy,
        LegacyOwnedRussianMainGlyphConfidence confidence)
    {
        var owner = controller.Owner;
        if (!ReferenceEquals(confidence.Model, owner.CommonModel) || !ReferenceEquals(confidence.Graph, owner.SourceGraph) ||
            (casePolicy is null ? owner.IsCaseAware || !ReferenceEquals(confidence.DirectDictionaryOwner, owner.DictionaryOwner) :
                !owner.IsCaseAware || !ReferenceEquals(casePolicy.Controller, controller) ||
                !ReferenceEquals(owner.CaseDictionaryOwner, casePolicy.DictionaryOwner) ||
                !ReferenceEquals(confidence.CaseDictionaryOwner, casePolicy.DictionaryOwner)))
            throw new ArgumentException("Publication requires its actual typed controller, graph and candidate owner.");
        if (resource.Length != 572763 || !StringComparer.OrdinalIgnoreCase.Equals(
            Convert.ToHexString(SHA256.HashData(resource)), owner.SelectedSourceOwner.SourceFingerprint))
            throw new ArgumentException("Compact publication must use its exact selected main resource.");
    }

    internal static PublicationStage Publish(ReadOnlySpan<byte> resource, LegacyMainCompactPublicationSource source,
        LegacyOwnedRussianMainGlyphConfidence confidence, LegacyMainCompactPublicationState state,
        CancellationToken cancellationToken, LegacyOwnedMainSecondaryTerminalCursor? configuredSecondary = null)
    {
        source.Validate(cancellationToken);
        var owner = source.Owner;
        var casePolicy = source.CasePolicy;
        var configuration = source.Controller.Configuration;
        if (!ReferenceEquals(state.Controller, source.Controller) || state.NextNode != source.StartingNode)
            throw new ArgumentException("Publication must advance its actual persistent compact state.");
        if (!ReferenceEquals(confidence.Model, owner.CommonModel) || !ReferenceEquals(confidence.Graph, owner.SourceGraph))
            throw new ArgumentException("Candidate weights, source graph and terminal must share the actual model owner.");
        LegacyOwnedFreshCaseMainTerminalInvocation? caseInvocation = null;
        if (casePolicy is null)
        {
            if (owner.IsCaseAware || !ReferenceEquals(confidence.DirectDictionaryOwner, owner.DictionaryOwner))
                throw new ArgumentException("The DIRECT entrypoint requires its actual DIRECT dictionary owner.");
        }
        else
        {
            if (!owner.IsCaseAware || !ReferenceEquals(casePolicy.Controller, source.Controller) ||
                !ReferenceEquals(owner.CaseDictionaryOwner, casePolicy.DictionaryOwner) ||
                !ReferenceEquals(confidence.CaseDictionaryOwner, casePolicy.DictionaryOwner))
                throw new ArgumentException("One actual fresh CASE source/controller/candidate owner required.");
            if (configuredSecondary is null)
                caseInvocation = LegacyOwnedFreshCaseMainTerminalInvocation.Prepare(casePolicy, cancellationToken);
            else if (!ReferenceEquals(configuredSecondary.PassResult.Storage, source.FinalPass) ||
                !ReferenceEquals(configuredSecondary.SourcePolicy, casePolicy))
                throw new ArgumentException("Configured publication requires the same actual terminal cursor/pass/source.");
        }
        if (configuration.HeaderCount2400 != 2)
            throw new NotSupportedException("The actual fresh two-header publication policy is required.");
        if (resource.Length != 572763 || !StringComparer.OrdinalIgnoreCase.Equals(
            Convert.ToHexString(SHA256.HashData(resource)), owner.SelectedSourceOwner.SourceFingerprint))
            throw new ArgumentException("Compact publication must use its exact selected main resource.");
        var reachable = LegacyOwnedMainCompactWordReachability.PrepareSource(resource, source, cancellationToken, configuredSecondary is not null);
        int languagePenalty = configuredSecondary is null
            ? caseInvocation?.LanguagePenalty2534 ?? LegacyOwnedRussianTerminalSources.Prepare(owner.DictionaryOwner, cancellationToken).LanguagePenalty2534
            : LegacyOwnedRussianTerminalSources.PreparePositiveScalePenalty(owner.CommonModel.LanguageModel,
                owner.CommonModel.LanguageProfiles[owner.CommonModel.ResourceLanguageProfileIndex],
                casePolicy!.DictionaryOwner.Descriptor.RuntimeLanguageScale, cancellationToken);
        var contextSentinels=configuredSecondary?.Options.ContextSentinels24b8??caseInvocation?.Options.ContextSentinels24b8??
            LegacyOwnedFreshMainTerminalInvocation.Prepare(source.Controller).Options.ContextSentinels24b8;
        var result=LegacyPreparedMainCompactPublication.Run(configuration,LegacyOrdinaryRecognitionGraph.FromSameSource(owner.SourceGraph),
            owner.Contexts,owner.CommonModel.AcousticAlphabet,owner.SelectedSourceOwner.Categories,contextSentinels,languagePenalty,
            configuredSecondary is not null,source.StartingNode,source.EndingNode,
            Enumerable.Range(0,source.EndingNode+1).Select(reachable.GetNodeSnapshot).ToArray(),reachable.HasLanguageContinuation,
            group=>LegacyMainCompactGroupScores.FromOld(confidence.ScoreGroup(group,cancellationToken)),
            state.Storage,state.Count,state.Starts,state.PrefixOwners,source.Validate,cancellationToken);
        state.Commit(source,result.Storage,result.Count,result.Starts,result.PrefixOwners,cancellationToken);
        return new(reachable,result.Nodes);
    }

    public byte[] CopyCompactStorage() => (byte[])storage.Clone();
    public int[] CopyNodeStarts2478() => (int[])starts.Clone();
    public int[] CopyPrefixOwners2488() => (int[])prefixOwners.Clone();
    public LegacyMainDirectWordNodeResult GetNodeSnapshot(int node)
    {
        if ((uint)node >= nodes.Length) throw new ArgumentOutOfRangeException(nameof(node));
        var row = nodes[node];
        return row with { Records = row.Records.ToArray(), PrimaryAlternatives = row.PrimaryAlternatives.ToArray() };
    }

    private static ushort U16(ReadOnlySpan<byte> bytes,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(bytes[at..]);
}
