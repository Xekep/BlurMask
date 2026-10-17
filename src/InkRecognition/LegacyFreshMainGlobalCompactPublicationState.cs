using System.Security.Cryptography;
namespace InkRecognition;

//Logical2460/2478/2488/2490 ownership, distinct from native physical ring allocation.
internal sealed class LegacyFreshMainGlobalCompactPublicationState
{
    internal LegacyFreshMainGlobalWordControllerPolicy Policy { get; }
    internal byte[] Storage { get; private set; }
    internal byte[] InitialResidue { get; }
    internal int[] Starts { get; private set; }
    internal int[] PrefixOwners { get; private set; }
    internal int Count { get; private set; }
    internal int NextNode { get; private set; }
    internal int PublicationCount { get; private set; }

    internal LegacyFreshMainGlobalCompactPublicationState(LegacyFreshMainGlobalWordControllerPolicy policy,
        ReadOnlySpan<byte> residue)
    {
        if (residue.IsEmpty || residue.Length % 40 != 0 || residue.Length > 64 * 1024 * 1024)
            throw new ArgumentException("Explicit bounded complete compact40 allocator storage required.");
        Policy = policy; Storage = residue.ToArray(); InitialResidue = residue.ToArray();
        Starts = new int[policy.Owner.Graph.State.NodeCount + 1]; PrefixOwners = new int[Starts.Length];
    }
    internal static void ValidateOwner(ReadOnlySpan<byte> resource, LegacyFreshMainGlobalWordControllerPolicy policy,
        LegacyFreshMainGlobalGlyphConfidence confidence)
    {
        ArgumentNullException.ThrowIfNull(policy); ArgumentNullException.ThrowIfNull(confidence);
        if (!ReferenceEquals(confidence.Owner, policy.Owner) || resource.Length != 572763 ||
            !Convert.ToHexString(SHA256.HashData(resource)).Equals(policy.Owner.Graph.Source.Source.SourceFingerprint,
                StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The actual selected global resource/controller/cached confidence owner required.");
        _ = LegacyFreshMainGlobalTerminalInvocation.Prepare(policy);
        var c = policy.Configuration;
        if (c.HeaderCount2400 != 2 || c.PrimaryAlternativeCount2402 != 2 || c.SecondaryAlternativeCount2404 != 1 ||
            c.Mode22f6 != 0 || c.Trace2368 != 0 || (c.PackedSource22e8 & 7) <= 1)
            throw new NotSupportedException("Actual fresh global mode0/2/2/1 ordinary publication required.");
    }
    internal (LegacyPreparedMainCompactReachability.Result Reachability, LegacyPreparedMainCompactPublication.Result Publication)
        Publish(int endingNode, Func<int, LegacyMainDirectWordNodeResult> snapshot, LegacyNativeDelimiterQuerySource reader,
            LegacyFreshMainGlobalGlyphConfidence confidence, Action<CancellationToken> validate, CancellationToken token)
    {
        validate(token);
        if (!ReferenceEquals(confidence.Owner, Policy.Owner) || NextNode > endingNode || endingNode >= Starts.Length - 1)
            throw new ArgumentException("Publication must advance its exact global source/pool.");
        int start = NextNode;
        var owner = Policy.Owner;
        var invocation = LegacyFreshMainGlobalTerminalInvocation.Prepare(Policy, token);
        var reach = LegacyPreparedMainCompactReachability.Run(Policy.Configuration, owner.Graph.State.MaximumAtoms,
            start, endingNode, snapshot, reader, owner.Contexts.ContainerIdentity, validate, false, token);
        var published = LegacyPreparedMainCompactPublication.Run(Policy.Configuration,
            LegacyOrdinaryRecognitionGraph.FromGlobal(owner.Graph), owner.Contexts, owner.Model.AcousticAlphabet,
            owner.Symbols.Geometry.CategoryOwner.Categories, invocation.Options.ContextSentinels24b8,
            invocation.LanguagePenalty2534, false, start, endingNode, reach.Nodes, reach.LanguageProbe,
            group => LegacyMainCompactGroupScores.FromGlobal(confidence.ReadGroup(group, token)),
            Storage, Count, Starts, PrefixOwners, validate, token);
        validate(token);
        if (published.Storage.Length != Storage.Length || published.Count < Count || published.Count > Storage.Length / 40 ||
            published.Starts.Length != Starts.Length || published.PrefixOwners.Length != Starts.Length)
            throw new InvalidDataException("Global compact pool lost its allocation/count/table extents.");
        Storage = published.Storage; Count = published.Count; Starts = published.Starts; PrefixOwners = published.PrefixOwners;
        NextNode = endingNode + 1; PublicationCount++;
        return (reach, published);
    }
    internal LegacyFreshMainGlobalCompactPublicationState Copy() => new(Policy, InitialResidue)
    {
        Storage = Storage.ToArray(), Starts = Starts.ToArray(), PrefixOwners = PrefixOwners.ToArray(),
        Count = Count, NextNode = NextNode, PublicationCount = PublicationCount
    };
}

/// <summary>Copies of one actual global intermediate207CB0 invocation beforeV80.
/// These are logical publication rows, not a selected winner or decoded text.</summary>
public sealed class LegacyFreshMainGlobalIntermediatePublication
{
    private readonly LegacyMainDirectWordNodeResult[] before, after;
    private readonly byte[] storage;
    private readonly int[] starts, prefix;
    public int StartingNode2490 { get; }
    public int EndingNode { get; }
    public int PreviousRecordCount2470 { get; }
    public int RecordCount2470 { get; }
    public int QueryChecks { get; }
    public int BackendCalls { get; }
    public int AcousticEvaluationsBefore { get; }
    public int AcousticEvaluationsAfter { get; }
    internal LegacyFreshMainGlobalIntermediatePublication(int start, int end, int previousCount,
        Func<int, LegacyMainDirectWordNodeResult> snapshot, LegacyFreshMainGlobalCompactPublicationState state,
        LegacyPreparedMainCompactReachability.Result reach, LegacyPreparedMainCompactPublication.Result published,
        int acousticBefore, int acousticAfter)
    {
        StartingNode2490 = start; EndingNode = end; PreviousRecordCount2470 = previousCount; RecordCount2470 = state.Count;
        QueryChecks = reach.Counters.Checks; BackendCalls = reach.Counters.Calls;
        AcousticEvaluationsBefore = acousticBefore; AcousticEvaluationsAfter = acousticAfter;
        before = Enumerable.Range(0, end + 1).Select(n => Copy(snapshot(n))).ToArray();
        after = published.Nodes.Select(Copy).ToArray(); storage = state.Storage.ToArray();
        starts = state.Starts.AsSpan(0, end + 2).ToArray(); prefix = state.PrefixOwners.AsSpan(0, end + 2).ToArray();
    }
    public LegacyMainDirectWordNodeResult GetInputNodeSnapshot(int node) => Copy(Read(before, node));
    public LegacyMainDirectWordNodeResult GetPublishedNodeSnapshot(int node) => Copy(Read(after, node));
    private static LegacyMainDirectWordNodeResult Read(LegacyMainDirectWordNodeResult[] nodes, int node)
    {
        if ((uint)node >= (uint)nodes.Length) throw new ArgumentOutOfRangeException(nameof(node));
        return nodes[node];
    }
    public byte[] CopyCompactStorage() => storage.ToArray();
    public int[] CopyNodeStarts2478() => starts.ToArray();
    public int[] CopyPrefixOwners2488() => prefix.ToArray();
    private static LegacyMainDirectWordNodeResult Copy(LegacyMainDirectWordNodeResult node) => node with
    { Records = node.Records.ToArray(), PrimaryAlternatives = node.PrimaryAlternatives.ToArray(), SecondaryAlternatives = node.SecondaryAlternatives.ToArray() };
}
