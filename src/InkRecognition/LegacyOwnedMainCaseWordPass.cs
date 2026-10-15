namespace InkRecognition;

/// <summary>Actual fresh292 main source through the shared ascending node/span
/// scheduler. The original DIRECT constructor keeps rejecting case owners.
/// Later-word/protocol branches unsupported by the consumers fail explicitly;
/// returned logical rows are not a recognized-text result.</summary>
public sealed class LegacyOwnedMainCaseWordPass
{
    private readonly LegacyOwnedMainDirectWordPass scheduler;
    public LegacyFreshCaseMainSourcePolicy SourcePolicy {get;}

    public LegacyOwnedMainCaseWordPass(ReadOnlySpan<byte> resource, LegacyFreshCaseMainSourcePolicy policy,
        LegacyCursiveSeparationTable table, LegacyMainDirectStorageProvider allocator,
        CancellationToken cancellationToken=default)
        : this(resource,policy,null,table,allocator,cancellationToken) { }

    public LegacyOwnedMainCaseWordPass(ReadOnlySpan<byte> resource, LegacyFreshCaseMainSourcePolicy policy,
        LegacyPreparedFreshMainLine240AcousticScorer? actualLineAcoustic,
        LegacyCursiveSeparationTable table, LegacyMainDirectStorageProvider allocator,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(policy);SourcePolicy=policy;
        scheduler=new(resource,policy.Controller,actualLineAcoustic,table,allocator,policy,cancellationToken);
    }

    public IReadOnlyList<LegacyMainDirectWordNodeResult> Run(CancellationToken cancellationToken=default)
        =>scheduler.Run(cancellationToken);

    /// <summary>Deep-owned successful storage. This does not invoke terminal
    /// selection or materialize the compact backtracking records.</summary>
    public LegacyOwnedMainCaseWordPassResult RunOwned(CancellationToken cancellationToken=default)
        =>new(SourcePolicy,scheduler.RunOwned(cancellationToken));

    /// <summary>Actual live intermediate publication through this same typed
    /// CASE scheduler and persistent292/290 source; no parallel DIRECT owner.</summary>
    public LegacyOwnedMainCaseWordPassResult RunOwnedWithIntermediatePublication(ReadOnlySpan<byte> resource,
        LegacyOwnedRussianMainGlyphConfidence candidateOwner, ReadOnlySpan<byte> compactAllocatorResidue40,
        Action<LegacyMainIntermediateCompactPublication>? publicationObserver = null, CancellationToken cancellationToken = default)
        => new(SourcePolicy, scheduler.RunOwnedWithIntermediatePublication(resource, candidateOwner,
            compactAllocatorResidue40, publicationObserver, cancellationToken));
}

public sealed class LegacyOwnedMainCaseWordPassResult
{
    internal LegacyOwnedMainDirectWordPassResult Storage {get;}
    public LegacyFreshCaseMainSourcePolicy SourcePolicy {get;}
    public int NodeCount=>Storage.NodeCount;
    internal LegacyOwnedMainCaseWordPassResult(LegacyFreshCaseMainSourcePolicy policy,
        LegacyOwnedMainDirectWordPassResult storage)
    {
        if(!ReferenceEquals(policy.Controller,storage.ControllerOwner))
            throw new ArgumentException("Actual case pass/controller storage owner required.");
        SourcePolicy=policy;Storage=storage;
    }
    public LegacyMainDirectWordNodeResult GetNodeSnapshot(int endingNode)=>Storage.GetNodeSnapshot(endingNode);
    public int[] CopyCumulativePathCosts()=>Storage.CopyCumulativePathCosts();
    public int IntermediatePublicationCount => Storage.IntermediatePublicationCount;
    public int NextUnpublishedNode2490 => Storage.NextUnpublishedNode2490;
    public byte[] CopyIntermediateCompactStorage() => Storage.CopyIntermediateCompactStorage();
    public int[] CopyIntermediateNodeStarts2478() => Storage.CopyIntermediateNodeStarts2478();
    public int[] CopyIntermediatePrefixOwners2488() => Storage.CopyIntermediatePrefixOwners2488();
}
