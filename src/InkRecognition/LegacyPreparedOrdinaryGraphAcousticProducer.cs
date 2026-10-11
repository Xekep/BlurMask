using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>
/// Resource-backed ordinary graph row producer. Source primitive flags and
/// enclosure probabilities come from the owned source endpoint metadata.
/// Other cache policy switches, the prepared mask/controller and guide frame
/// remain explicit upstream policy; external hooks are not synthesized.
/// </summary>
public sealed class LegacyPreparedOrdinaryGraphAcousticProducer
{
    private readonly LegacyPreparedWordGroupEvaluator evaluator;
    private readonly LegacyOrdinaryAcousticClassCache cache;
    private readonly LegacyOrdinarySourceEndpointMetadata endpoints;
    private readonly LegacyOrdinaryAcousticCacheContext template;
    private readonly byte[] enabled;
    private readonly LegacyAcousticCachePair[] pairs;
    private readonly float scale;
    private readonly int budget;
    public int ClassifiedGroups { get; private set; }
    public int ContextWindowCount => evaluator.ContextWindowCount;

    public LegacyPreparedOrdinaryGraphAcousticProducer(LegacyRussianModelBundle model,
        ReadOnlySpan<byte> acousticResource, LegacyOrdinaryCursiveCutSource source,
        ReadOnlySpan<int> rawPhysicalPointCounts, LegacyCursiveWordSearchOptions options,
        ReadOnlySpan<byte> preparedEnabledSymbols, LegacyOrdinaryAcousticCacheContext cachePolicy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model); ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(options); ArgumentNullException.ThrowIfNull(cachePolicy);
        cancellationToken.ThrowIfCancellationRequested();
        if (acousticResource.Length != 572763 || !SHA256.HashData(acousticResource).AsSpan().SequenceEqual(
            Convert.FromHexString("7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978")))
            throw new InvalidDataException("Matching pinned ordinary Russian v110 resource required.");
        if (preparedEnabledSymbols.Length != model.AcousticAlphabet.LabelCount || options.MaximumScoredGroups is < 1 or > 32768)
            throw new ArgumentException("Matching prepared symbol mask and bounded neural-group budget required.");
        foreach (byte value in preparedEnabledSymbols) if (value > 1) throw new ArgumentException("Mask requires0/1.");
        enabled = preparedEnabledSymbols.ToArray(); budget = options.MaximumScoredGroups;
        template = cachePolicy with { ScopePenaltyLabels = cachePolicy.ScopePenaltyLabels.ToArray() };
        endpoints = new(source, rawPhysicalPointCounts, cancellationToken);
        evaluator = new(model, source, options, cancellationToken);
        cache = new(LegacyOrdinaryAcousticCacheProfile.ReadV110Node(acousticResource[189734..], cancellationToken),
            model.AcousticAlphabet, acousticResource.Slice(99,16));
        scale = LegacyPreparedAcousticDictionaryPermissions.ReadDisabledPowerScale(acousticResource[189734..], cancellationToken);
        pairs = LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet);
    }

    /// <summary>
    /// Called by the native-selector cache session only when recomputation is
    /// required. No caller passes row-kind, succeeded, recomputed or primitive
    /// scope flags. A thrown unsupported pipeline request invalidates the session.
    /// </summary>
    public LegacyOrdinaryGraphAcousticRow Compute(LegacyOrdinaryCursiveCandidateInk group,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group); cancellationToken.ThrowIfCancellationRequested();
        if (ClassifiedGroups >= budget) throw new InvalidOperationException("Ordinary graph acoustic producer exceeded its explicit group budget.");
        var glyph = evaluator.Classify(group.Ranges.Span, cancellationToken);
        var context = endpoints.BindContext(glyph.Geometry, template, cancellationToken);
        var final = cache.Build(glyph.IntegerCosts.Span, glyph.Geometry.Strokes, enabled, context, pairs, cancellationToken);
        ClassifiedGroups++;
        return new(final.Costs, scale);
    }
}
