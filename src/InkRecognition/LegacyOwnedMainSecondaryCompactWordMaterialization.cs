using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Configured2 actual CASE207CB0 storage. Selection consumes the
/// published2410.1C fields, not a manufactured count1 terminal or WORD.</summary>
public sealed class LegacyOwnedMainSecondaryCompactWordMaterialization
{
    private readonly byte[] storage;
    private readonly int[] starts;
    private readonly LegacyMainDirectWordNodeResult[] nodes;
    public LegacyOwnedMainCaseWordPassResult PassResult { get; }
    public LegacyOwnedRussianMainGlyphConfidence ConfidenceOwner { get; }
    public int RecordCount { get; }

    private LegacyOwnedMainSecondaryCompactWordMaterialization(LegacyOwnedMainCaseWordPassResult pass,
        LegacyOwnedRussianMainGlyphConfidence confidence, LegacyMainCompactPublicationState state,
        LegacyMainDirectWordNodeResult[] nodes)
    {
        PassResult = pass; ConfidenceOwner = confidence; RecordCount = state.Count;
        storage = state.Storage.ToArray(); starts = state.Starts.ToArray(); this.nodes = nodes;
    }

    public static LegacyOwnedMainSecondaryCompactWordMaterialization Materialize(ReadOnlySpan<byte> resource,
        LegacyOwnedMainCaseWordPassResult actualPass, LegacyOwnedRussianMainGlyphConfidence confidence,
        ReadOnlySpan<byte> allocatorResidue40, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualPass); ArgumentNullException.ThrowIfNull(confidence);
        token.ThrowIfCancellationRequested();
        var sourcePolicy = actualPass.SourcePolicy;
        LegacyOwnedMainCompactWordMaterialization.ValidatePublicationOwner(resource,
            sourcePolicy.Controller, sourcePolicy, confidence);
        if (actualPass.NextUnpublishedNode2490 != 0 || actualPass.NodeCount > 120)
            throw new NotSupportedException("Configured fresh one-shot publication through node119 required.");
        var selection = LegacyOwnedMainSecondaryTerminalCursor.Prepare(actualPass, token);
        var state = new LegacyMainCompactPublicationState(sourcePolicy.Controller, allocatorResidue40);
        actualPass.Storage.BeginCompactWordPublication(token);
        var source = LegacyMainCompactPublicationSource.Final(resource, actualPass.Storage, sourcePolicy, 0, token);
        var stage = LegacyOwnedMainCompactWordMaterialization.Publish(resource, source, confidence, state, token, selection);
        return new(actualPass, confidence, state, stage.Nodes);
    }

    public byte[] CopyCompactStorage() => storage.ToArray();
    public int[] CopyNodeStarts2478() => starts.ToArray();
    public LegacyMainDirectWordNodeResult GetNodeSnapshot(int node)
    {
        if ((uint)node >= nodes.Length) throw new ArgumentOutOfRangeException(nameof(node));
        var snapshot = nodes[node];
        return snapshot with { Records = snapshot.Records.ToArray(), PrimaryAlternatives = snapshot.PrimaryAlternatives.ToArray(),
            SecondaryAlternatives = snapshot.SecondaryAlternatives.ToArray() };
    }

    /// <summary>Numeric published terminal only. Exclusions remain explicit
    /// mapped identities; no accepted-output counter or callback is supplied.</summary>
    public LegacyPreparedMainSecondaryTerminalWinner SelectNext(ReadOnlySpan<ulong> excludedIdentities,
        CancellationToken token = default)
    {
        PassResult.Storage.ValidateResultQueryEpoch(token);
        var policy = PassResult.SourcePolicy;
        var controller = policy.Controller;
        var model = policy.Model;
        int penalty = LegacyOwnedRussianTerminalSources.PreparePositiveScalePenalty(model.LanguageModel,
            model.LanguageProfiles[model.ResourceLanguageProfileIndex], policy.DictionaryOwner.Descriptor.RuntimeLanguageScale, token);
        bool sentinels = model.SourceKind == 0x137 || model.AcousticAlphabet.TryGetLabel("\U000f0009", out _);
        var options = new LegacyPreparedMainTerminalOptions(controller.Configuration.PrimaryArcLimit230a,
            sentinels, false, false, false, 0);
        var final = nodes[^1];
        var winner = LegacyNativeMainSecondaryTerminalSelection.SelectWithoutRescue(final.Records.Span,
            final.SourceEnd23f0, final.SecondaryAlternatives.Span, 2,
            [new(0, 0), new(policy.DictionaryOwner.Descriptor.Flags, penalty)], options,
            final.CumulativePathCost2520, policy.DictionaryOwner.Contexts, excludedIdentities, token);
        if (winner.Winner.HasWinner)
        {
            ushort relative = BinaryPrimitives.ReadUInt16LittleEndian(final.SecondaryAlternatives.Span.Slice(
                (winner.Winner.Ordinal * 2 + winner.SecondaryOrdinal) * 40 + 0x1c));
            if (relative == ushort.MaxValue || starts[^2] + relative >= starts[^1])
                throw new InvalidDataException("Selected secondary must own a genuine published compact index.");
        }
        token.ThrowIfCancellationRequested();
        return winner;
    }

    /// <summary>Same numeric choice with unforgeable published-storage
    /// ownership for subsequent configured backtracking. Null means no winner.</summary>
    public LegacyOwnedMainSecondaryCompactTerminal? SelectOwned(ReadOnlySpan<ulong> excludedIdentities,
        CancellationToken token = default)
    {
        var selected = SelectNext(excludedIdentities, token);
        if (!selected.Winner.HasWinner) return null;
        int index = checked(starts[^2] + (int)selected.Winner.RowField22);
        var last = nodes[^1];
        byte[] row = last.Records.Span.Slice(selected.Winner.Ordinal * 88, 88).ToArray();
        byte[] alternative = last.SecondaryAlternatives.Span.Slice(
            (selected.Winner.Ordinal * 2 + selected.SecondaryOrdinal) * 40, 40).ToArray();
        token.ThrowIfCancellationRequested();
        return new(this, selected, index, row, alternative);
    }
}

/// <summary>One genuinely finite selected2410 entry and its actual2460
/// index. This retains its configured owner; it is not a count1 terminal.</summary>
public sealed class LegacyOwnedMainSecondaryCompactTerminal
{
    private readonly byte[] row88, alternative40;
    public LegacyOwnedMainSecondaryCompactWordMaterialization Materialization { get; }
    public LegacyPreparedMainSecondaryTerminalWinner Selection { get; }
    public int SelectedRecordIndex { get; }
    internal LegacyOwnedMainSecondaryCompactTerminal(LegacyOwnedMainSecondaryCompactWordMaterialization materialization,
        LegacyPreparedMainSecondaryTerminalWinner selection, int index, byte[] row, byte[] alternative)
    { Materialization = materialization; Selection = selection; SelectedRecordIndex = index; row88 = row; alternative40 = alternative; }
    public byte[] CopyStoredRow88() => row88.ToArray();
    public byte[] CopySecondaryAlternative40() => alternative40.ToArray();
}
