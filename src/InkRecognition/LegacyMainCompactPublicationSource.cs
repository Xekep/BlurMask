using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainLiveCompactPublication.Probe")]

namespace InkRecognition;

//One genuine207CB0 invocation. Both final and intermediate adapters retain
//their producing source; this object never creates a backend, D0 or V80.
internal sealed class LegacyMainCompactPublicationSource
{
    internal LegacyOwnedFreshMainWordControllerPolicy Controller { get; }
    internal LegacyFreshCaseMainSourcePolicy? CasePolicy { get; }
    internal LegacyOwnedMainDirectWordPassResult? FinalPass { get; }
    internal LegacyOwnedMainWordSpanPreparation Owner => Controller.Owner;
    internal int StartingNode { get; }
    internal int EndingNode { get; }
    internal LegacyNativeDelimiterQuerySource Reader { get; }
    private readonly Func<int, LegacyMainDirectWordNodeResult> snapshot;
    private readonly Action<CancellationToken> validate;

    internal LegacyMainCompactPublicationSource(LegacyOwnedFreshMainWordControllerPolicy controller,
        LegacyFreshCaseMainSourcePolicy? casePolicy, int startingNode, int endingNode,
        Func<int, LegacyMainDirectWordNodeResult> snapshot, LegacyNativeDelimiterQuerySource reader,
        Action<CancellationToken> validate, LegacyOwnedMainDirectWordPassResult? finalPass = null)
    {
        Controller = controller; CasePolicy = casePolicy; StartingNode = startingNode; EndingNode = endingNode;
        this.snapshot = snapshot; Reader = reader; this.validate = validate; FinalPass = finalPass;
        if (startingNode < 0 || endingNode < startingNode || endingNode >= Owner.SourceGraph.NodeCount)
            throw new ArgumentOutOfRangeException(nameof(startingNode));
        if (Owner.IsCaseAware != (casePolicy is not null) ||
            casePolicy is not null && !ReferenceEquals(casePolicy.Controller, controller))
            throw new ArgumentException("Publication must retain its typed source controller.");
    }

    internal LegacyMainDirectWordNodeResult Snapshot(int node)
    {
        if ((uint)node > EndingNode) throw new ArgumentOutOfRangeException(nameof(node));
        var result = snapshot(node);
        if (result.EndingNode != node) throw new InvalidDataException("Publication source lost native node order.");
        return result with { Records = result.Records.ToArray(), PrimaryAlternatives = result.PrimaryAlternatives.ToArray() };
    }

    internal void Validate(CancellationToken token)
    { token.ThrowIfCancellationRequested(); validate(token); }

    internal static LegacyMainCompactPublicationSource Final(ReadOnlySpan<byte> resource,
        LegacyOwnedMainDirectWordPassResult pass, LegacyFreshCaseMainSourcePolicy? casePolicy,
        int startingNode, CancellationToken token)
    {
        pass.ValidateResultQueryEpoch(token);
        var owner = pass.ControllerOwner.Owner;
        var reader = new LegacyNativeDelimiterQuerySource(owner.CommonModel.AcousticAlphabet, owner.Contexts,
            (tag, query, cancellation) => pass.ReadResultQuery(tag, query, false, cancellation),
            LegacyNativeDelimiterQuerySource.ReadV10dCoordinateMode(resource));
        reader.ResetForControllerNode(token); //Only after the actual final V80.
        return new(pass.ControllerOwner, casePolicy, startingNode, pass.NodeCount - 1,
            pass.GetNodeSnapshot, reader, pass.ValidateResultQueryEpoch, pass);
    }
}

//2460/2470/2478/2488/2490 persist across intermediate207CB0 invocations.
//Storage is explicit caller allocator residue, never fabricated native zeros.
internal sealed class LegacyMainCompactPublicationState
{
    internal LegacyOwnedFreshMainWordControllerPolicy Controller { get; }
    internal byte[] Storage { get; private set; }
    internal int[] Starts { get; private set; }
    internal int[] PrefixOwners { get; private set; }
    internal int Count { get; private set; }
    internal int NextNode { get; private set; }
    internal int PublicationCount { get; private set; }
    internal byte[] InitialResidue { get; }

    internal LegacyMainCompactPublicationState(LegacyOwnedFreshMainWordControllerPolicy controller, ReadOnlySpan<byte> residue)
    {
        if (residue.Length == 0 || residue.Length % 40 != 0 || residue.Length > 64 * 1024 * 1024)
            throw new ArgumentException("Bounded explicit compact40 allocator storage required.");
        Controller = controller; Storage = residue.ToArray(); InitialResidue = residue.ToArray();
        Starts = new int[controller.Owner.SourceGraph.NodeCount + 1];
        PrefixOwners = new int[Starts.Length];
        //Only entries covered by actual publication may be consumed. Initial
        //2490 is the proven fresh constructor zero; unvisited table bytes are
        //not exposed as reconstructed native allocator contents.
    }

    internal void Commit(LegacyMainCompactPublicationSource source, byte[] storage, int count,
        int[] starts, int[] prefixOwners, CancellationToken token)
    {
        source.Validate(token);
        if (!ReferenceEquals(source.Controller, Controller) || source.StartingNode != NextNode ||
            storage.Length != Storage.Length || count < Count || count > storage.Length / 40 ||
            starts.Length != Starts.Length || prefixOwners.Length != Starts.Length)
            throw new InvalidOperationException("Compact publication state must advance its exact owning source.");
        Storage = storage; Count = count; Starts = starts; PrefixOwners = prefixOwners;
        NextNode = source.EndingNode + 1; PublicationCount++;
    }

    internal LegacyMainCompactPublicationState Copy()
        => new(Controller, InitialResidue)
        {
            Storage = Storage.ToArray(), Starts = Starts.ToArray(), PrefixOwners = PrefixOwners.ToArray(),
            Count = Count, NextNode = NextNode, PublicationCount = PublicationCount
        };
}
