namespace InkRecognition;

public sealed partial class LegacyOwnedMainDirectWordPass
{
    private LegacyFreshMainGlobalCaseSession.LiveQueryLease? globalLivePublicationEpoch;
    private LegacyFreshMainGlobalWordPassResult? globalCompletedPublicationResult;
    internal LegacyNativeDelimiterQuerySource BindGlobalResultPublicationReader(LegacyFreshMainGlobalWordPassResult result,
        CancellationToken token)
    {
        if (!ReferenceEquals(result.Policy, policy.Global) || globalCompletedPublicationResult is not null ||
            nodes.Count != owner.Graph.NodeCount || globalLivePublicationEpoch is not null)
            throw new ArgumentException("Only the completed actual global pass can retain its outer290.");
        result.ValidateResultQueryLease(token);
        globalCompletedPublicationResult = result;
        //RunCore has already performed the actual final ResetSources/V80.
        return delimiter;
    }
    internal sealed record GlobalIntermediatePublication(LegacyFreshMainGlobalGlyphConfidence Confidence,
        LegacyFreshMainGlobalCompactPublicationState State,
        Action<LegacyFreshMainGlobalIntermediatePublication>? Observer, int? PreparedEndingNode = null);

    internal IReadOnlyList<LegacyMainDirectWordNodeResult> RunGlobalWithIntermediatePublication(
        GlobalIntermediatePublication publication, CancellationToken token)
    {
        if (!ReferenceEquals(policy.Global, publication.State.Policy) ||
            !ReferenceEquals(publication.Confidence.Owner, owner.RequireGlobalSource()))
            throw new ArgumentException("Actual global controller/publication owner required.");
        return RunCore(token, null, publication);
    }
    private void PublishGlobalIntermediate(GlobalIntermediatePublication publication, int endingNode,
        IReadOnlyList<LegacyMainDirectWordNodeResult> completed, CancellationToken token)
    {
        var span = currentSpan ?? throw new InvalidOperationException("Actual last global D0 required.");
        if (span.EndingNode != endingNode || globalLivePublicationEpoch is not null || livePublicationEpoch is not null)
            throw new InvalidOperationException("One genuine global publication/query scope required.");
        var source = owner.RequireGlobalSource();
        owner.ValidateSpan(span, token);
        var lease = source.CaptureLiveQueryLease(span.GlobalSource!, token);
        globalLivePublicationEpoch = lease;
        try
        {
            LegacyMainDirectWordNodeResult Snapshot(int node) => completed[node] with
            {
                Records = nodes[node].CopyRecordStorage(), PrimaryAlternatives = nodes[node].CopyPrimaryStorage(),
                SecondaryAlternatives = nodes[node].CopySecondaryStorage(), CumulativePathCost2520 = cumulative[node]
            };
            int start = publication.State.NextNode, previousCount = publication.State.Count;
            int acousticBefore = source.AcousticEvaluations;
            var stage = publication.State.Publish(endingNode, Snapshot, delimiter, publication.Confidence,
                cancellation => source.ValidateLiveQueryLease(lease, cancellation), token);
            if (source.AcousticEvaluations != acousticBefore)
                throw new InvalidOperationException("Compact publication cannot reevaluate main NN caches.");
            if (publication.Observer is { } observer)
                observer(new(start, endingNode, previousCount, Snapshot, publication.State, stage.Reachability,
                    stage.Publication, acousticBefore, source.AcousticEvaluations));
            //Observers may cancel, refresh, reset or query the source. Validate
            //after the callback before changing ANY live owning node bytes.
            source.ValidateLiveQueryLease(lease, token);
            for (int node = start; node <= endingNode; node++)
                nodes[node].ApplyCompactPublication(policy.Global!, stage.Publication.Nodes[node], token);
            source.EndLiveQueryLease(lease, token);
        }
        finally { globalLivePublicationEpoch = null; }
    }
}
