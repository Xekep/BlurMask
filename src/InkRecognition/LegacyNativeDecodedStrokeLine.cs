namespace InkRecognition;

/// <summary>
/// One explicitly prepared decoded line, backed by actual raw source points and winning endpoint/label fields.
/// The caller's real decoder must produce endpoints and resolved alphabet labels; this does not recognize them.
/// </summary>
public sealed class LegacyNativeDecodedStrokeLine
{
    private readonly LegacyNativeStrokeBounds[] _itemBounds;
    public ReadOnlySpan<LegacyNativeStrokeBounds> ItemBounds => _itemBounds;
    public LegacyPreparedCharacterLineFitResult? Fit { get; }

    private LegacyNativeDecodedStrokeLine(LegacyNativeStrokeBounds[] bounds, LegacyPreparedCharacterLineFitResult? fit)
    { _itemBounds = bounds; Fit = fit; }

    /// <summary>Actual per-item stroke geometry -> Document0531b0 category template -> C1A primary prepared fit.</summary>
    public static LegacyNativeDecodedStrokeLine Prepare(LegacyNativeRawStrokeGeometry sourceOwner,
        LegacyNativeInkPositionSnapshot positionOwner, ReadOnlySpan<LegacyNativeDecodedItemEndpoint> endpoints,
        ReadOnlySpan<ushort> resolvedAlphabetLabels, LegacyPreparedCharacterLineAlphabetCategories alphabetCategories,
        LegacyCharacterLineFitProfile profile, bool forceZeroSlope = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceOwner); ArgumentNullException.ThrowIfNull(positionOwner);
        ArgumentNullException.ThrowIfNull(alphabetCategories);
        cancellationToken.ThrowIfCancellationRequested();
        if (resolvedAlphabetLabels.Length > 256) throw new ArgumentOutOfRangeException(nameof(resolvedAlphabetLabels));
        var bounds = sourceOwner.ReadDecodedItemBounds(positionOwner, resolvedAlphabetLabels.Length, endpoints, cancellationToken);
        var producer = alphabetCategories.CreateItemProducer();
        var items = new List<LegacyPreparedCharacterLineItem>();
        for (int item = 0; item < bounds.Length; item++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            //The prepared adapter excludes no-ink rows. It does not manufacture geometry for separators/empty source lists.
            if (!bounds[item].HasPoints) continue;
            var b = bounds[item];
            items.Add(producer.PrepareNextItem(resolvedAlphabetLabels[item], b.Left, b.Right, b.Top, b.Bottom));
        }
        var fit = LegacyPreparedCharacterLineFit.Fit(items.ToArray(), profile, forceZeroSlope,
            cancellationToken: cancellationToken);
        return new(bounds, fit);
    }

    /// <summary>
    /// Accepted primary fit float output -> source2DA double x-height ->2DB owner ->SDK metric.
    /// Character source-line IDs are still actual prepared inputs; multi-line lifecycle/fallbacks are outside this adapter.
    /// </summary>
    public LegacyPreparedResultLineMetrics CreateAcceptedSourceLineMetrics(ReadOnlySpan<int> derivedCharacterSourceLineIds,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (derivedCharacterSourceLineIds.Length > 65536)
            throw new ArgumentOutOfRangeException(nameof(derivedCharacterSourceLineIds));
        if (Fit is not { } fit) throw new InvalidOperationException("No primary prepared fit; caller must resolve the native fallback lifecycle.");
        var (_, height, _) = fit.ToPreparedFitOutput();
        if (!float.IsFinite(height) || height <= 0)
            throw new InvalidDataException("An accepted positive finite native source-line x-height is required.");
        foreach (int id in derivedCharacterSourceLineIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (id != 0) throw new ArgumentException("This adapter owns one explicit source line; supplied character line IDs must be0.");
        }
        return LegacyPreparedResultLineMetrics.Create([(double)height], derivedCharacterSourceLineIds, cancellationToken);
    }
}
