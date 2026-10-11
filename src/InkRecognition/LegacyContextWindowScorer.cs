namespace InkRecognition;

/// <summary>
/// Checked single caller-supplied line subset of1c9870: bounded whole-physical
/// windows after stroke0, cumulative earlier-stroke X extreme, extended30 extraction,
/// initial7001 classifier and strict nonblank-mass acceptance. Line height/Y
/// reference, scopes and model profile remain explicit. No automatic line fit,
/// fractional source geometry, cached-window invalidation or cursive cuts.
/// </summary>
public sealed class LegacyContextWindowScorer
{
    private readonly LegacyInkNetwork _network;
    private readonly LegacyInkActivations _activations;
    private readonly int _blankOutput;

    public LegacyContextWindowScorer(LegacyInkNetwork initialContextNetwork,
        LegacyInkActivations activations, int blankOutput = 27)
    {
        ArgumentNullException.ThrowIfNull(initialContextNetwork); ArgumentNullException.ThrowIfNull(activations);
        if (initialContextNetwork.Kind != 0x7001 || initialContextNetwork.InputWidth != 30 || initialContextNetwork.OutputWidth != 28)
            throw new ArgumentException("The checked initial7001 context30→28 profile is required.");
        if (blankOutput is < 0 or >= 28) throw new ArgumentOutOfRangeException(nameof(blankOutput));
        _network = initialContextNetwork; _activations = activations; _blankOutput = blankOutput;
    }

    public LegacyPriorContextGroup[] ScoreLine(IReadOnlyList<LegacyInkStroke> physicalStrokes,
        int lineId, float lineHeight, float verticalReference, ReadOnlySpan<int> physicalScopeIds,
        int maximumWindowStrokes = 2, float nonblankCutoff = .1f, bool checkScope = false,
        bool useMaximumX = false, bool usePointMeanY = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(physicalStrokes);
        if (physicalStrokes.Count is < 1 or > 256 || physicalScopeIds.Length != physicalStrokes.Count ||
            maximumWindowStrokes is < 1 or > 3 || !float.IsFinite(lineHeight) || lineHeight <= 0 ||
            !float.IsFinite(verticalReference) || !float.IsFinite(nonblankCutoff) || nonblankCutoff is < 0 or > 1)
            throw new ArgumentException("Expected1..256 physical strokes with scope IDs, positive height, finite reference,1..3 window size and cutoff0..1.");
        cancellationToken.ThrowIfCancellationRequested(); int totalPoints = 0;
        foreach (var stroke in physicalStrokes)
        {
            ArgumentNullException.ThrowIfNull(stroke); totalPoints += stroke.Points.Length;
            if (totalPoints > 4096) throw new ArgumentException("Caller line exceeds4096 physical points.");
        }
        var accepted = new List<LegacyPriorContextGroup>();
        float xReference = useMaximumX ? float.MaxValue : -float.MaxValue;
        for (int start = 1; start < physicalStrokes.Count; start++)
        {
            cancellationToken.ThrowIfCancellationRequested(); var preceding = physicalStrokes[start - 1].Points;
            // 1c9870 initializes fVar41 outside the start-loop. This is a
            // cumulative reference from every earlier physical stroke, even
            // when source drawing moves backwards or crosses scope changes.
            // The geometry maximum-X mode pairs with a minimum reference.
            foreach (var point in preceding)
                xReference = useMaximumX ? Math.Min(xReference, point.X) : Math.Max(xReference, point.X);
            float sumX = 0; int pointCount = 0;
            for (int length = 1; length <= maximumWindowStrokes && start + length <= physicalStrokes.Count; length++)
            {
                int end = start + length - 1;
                if (checkScope && physicalScopeIds[end] != physicalScopeIds[start]) break;
                foreach (var point in physicalStrokes[end].Points) { sumX += point.X; pointCount++; }
                var window = new LegacyInkStroke[length]; for (int i = 0; i < length; i++) window[i] = physicalStrokes[start + i];
                // This restored geometry profile explicitly rejects a native
                // zero-extent branch. Fail instead of manufacturing a window.
                var geometry = LegacyContextGeometry.Extract(window, lineHeight, xReference, verticalReference,
                    useMaximumX, usePointMeanY, cancellationToken);
                var fixedInput = LegacyFloatInputQuantization.Quantize(geometry, 10, cancellationToken);
                var scores = LegacyClassScores.Convert(_network.Evaluate(fixedInput, _activations,
                    cancellationToken: cancellationToken), cancellationToken).Probability32768.Span;
                var probability = new float[28]; float nonblankMass = 0;
                for (int i = 0; i < probability.Length; i++)
                {
                    probability[i] = scores[i] * (1f / 32768f);
                    if (i != _blankOutput) nonblankMass += probability[i];
                }
                if (nonblankCutoff < nonblankMass)
                    accepted.Add(new(lineId, physicalScopeIds[start], start, end, sumX / pointCount, probability));
            }
        }
        return accepted.ToArray();
    }
}
