using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("LegacyMainGlobalFinalPublication.Probe")]
namespace InkRecognition;

/// <summary>Final207CB0 over the actual completed LIVE pass, BEFORE terminal
/// selection. It retains the same pool, outer290 and final source lease.
/// Logical rows/explicit allocation residue do not restore physical ring aliases
/// or native partial-allocation failure effects.</summary>
public sealed class LegacyFreshMainGlobalFinalPublication
{
    private readonly LegacyPreparedMainCompactPublication.Result result;
    public LegacyFreshMainGlobalWordPassResult PassResult { get; }
    public LegacyFreshMainGlobalGlyphConfidence ConfidenceOwner { get; }
    public LegacyFreshMainGlobalWordControllerPolicy Policy => PassResult.Policy;
    public int NodeCount => result.Nodes.Length;
    public int StartingNode2490 { get; }
    public int NextNode2490 => NodeCount;
    public int PreviousRecordCount2470 { get; }
    public int RecordCount2470 => result.Count;
    internal int BatchCompactAllocationLength=>result.Storage.Length;
    internal int BatchStartsLength=>result.Starts.Length;
    internal int BatchSnapshotDependencyPayload=>checked(result.Nodes[^1].Records.Length+result.Nodes[^1].PrimaryAlternatives.Length+result.Nodes[^1].SecondaryAlternatives.Length);
    public int PublicationCount { get; }
    public int QueryChecks { get; }
    public int BackendCalls { get; }
    internal ReadOnlySpan<byte> FinalRecords => result.Nodes[^1].Records.Span;
    internal ushort FinalSourceEnd => result.Nodes[^1].SourceEnd23f0;
    internal int FinalCumulativeCost => result.Nodes[^1].CumulativePathCost2520;
    internal int FinalNodeStart => result.Starts[^2];

    private LegacyFreshMainGlobalFinalPublication(LegacyFreshMainGlobalWordPassResult pass,
        LegacyFreshMainGlobalGlyphConfidence confidence, int startingNode, int previousCount,
        LegacyFreshMainGlobalCompactPublicationState state, LegacyPreparedMainCompactReachability.Result reachability,
        LegacyPreparedMainCompactPublication.Result publication)
    {
        PassResult = pass; ConfidenceOwner = confidence; StartingNode2490 = startingNode;
        PreviousRecordCount2470 = previousCount; PublicationCount = state.PublicationCount;
        QueryChecks = reachability.Counters.Checks; BackendCalls = reachability.Counters.Calls;
        //The numeric result/state are private fresh owned arrays, not caller memory.
        result = publication;
    }
    public static LegacyFreshMainGlobalFinalPublication Append(LegacyFreshMainGlobalWordPassResult actualLivePass,
        LegacyFreshMainGlobalGlyphConfidence confidence, ReadOnlySpan<byte> resource, CancellationToken token = default)
        => AppendCore(actualLivePass, confidence, resource, null, 240, token);

    /// <summary>Final207CB0 after the same budgeted repeated LIVE producer.
    /// The old Append API retains its one-cycle supported domain.</summary>
    public static LegacyFreshMainGlobalFinalPublication AppendRepeated(
        LegacyFreshMainGlobalRepeatedWordPassResult actualRepeatedPass,
        LegacyFreshMainGlobalGlyphConfidence confidence, ReadOnlySpan<byte> resource,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualRepeatedPass);
        actualRepeatedPass.Validate(token);
        return AppendCore(actualRepeatedPass.PassResult, confidence, resource, null,
            actualRepeatedPass.Owner.Budget.MaximumLogicalNodes, token);
    }

    //Prepared postnumeric failure control only. No public callback, forced gate
    //or synthetic winner is added to the native production path.
    internal static LegacyFreshMainGlobalFinalPublication AppendWithObserver(LegacyFreshMainGlobalWordPassResult actualLivePass,
        LegacyFreshMainGlobalGlyphConfidence confidence, ReadOnlySpan<byte> resource,
        Action<LegacyFreshMainGlobalFinalPublication> observer, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(observer);
        return AppendCore(actualLivePass, confidence, resource, observer, 240, token);
    }
    private static LegacyFreshMainGlobalFinalPublication AppendCore(LegacyFreshMainGlobalWordPassResult pass,
        LegacyFreshMainGlobalGlyphConfidence confidence, ReadOnlySpan<byte> resource,
        Action<LegacyFreshMainGlobalFinalPublication>? observer, int maximumNodes, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(pass); token.ThrowIfCancellationRequested();
        LegacyFreshMainGlobalCompactPublicationState.ValidateOwner(resource, pass.Policy, confidence);
        if (pass.NodeCount < 1 || pass.NodeCount > maximumNodes)
            throw new NotSupportedException("Bounded global final publication supports at most one LIVE cycle.");
        var state = pass.IntermediatePublicationState?.Copy() ??
            throw new ArgumentException("The actual completed pass must bind the typed LIVE pool producer.");
        if (!ReferenceEquals(state.Policy, pass.Policy) || state.NextNode >= pass.NodeCount)
            throw new ArgumentException("The retained global pool must have a remaining final publication range.");
        var reader = pass.RequirePublicationReader(token); //Actual finalV80 has already reset THIS outer290.
        pass.BeginCompactPublication(token); //Same claim as old positive append; all started failures consume it.
        int start = state.NextNode, previous = state.Count, evaluations = pass.Policy.Owner.AcousticEvaluations;
        var stage = state.Publish(pass.NodeCount - 1, pass.GetNodeSnapshot, reader, confidence,
            pass.ValidateResultQueryLease, token, LegacyFreshMainGlobalPublicationContext.ForResult(pass,start,token));
        if (pass.Policy.Owner.AcousticEvaluations != evaluations)
            throw new InvalidOperationException("Final207CB0 cannot reevaluate NN groups.");
        var completed = new LegacyFreshMainGlobalFinalPublication(pass, confidence, start, previous,
            state, stage.Reachability, stage.Publication);
        observer?.Invoke(completed);
        //A blocked return does not undo query/context side effects or resurrect
        //the consumed pass. Intermediate prefixes remain in their owning snapshot.
        pass.ValidateResultQueryLease(token);
        return completed;
    }
    internal void Validate(CancellationToken token) => PassResult.ValidateResultQueryLease(token);
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
