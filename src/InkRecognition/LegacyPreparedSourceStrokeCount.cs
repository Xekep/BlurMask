namespace InkRecognition;

/// <summary>
/// Native Text1801b4e40 count byte: distinct source-record int32 identifiers
/// among selected prepared ranges, with Text1801b5cf0 prefix-template additions.
/// Range discovery and the source-record identifier producer remain caller data.
/// This is not a language, letter-case, physical pen-up or confidence estimator.
/// </summary>
public static class LegacyPreparedSourceStrokeCount
{
    /// <summary>
    /// Repeated fragments of the same source identifier count once, regardless
    /// of adjacency. Native byte arithmetic wraps modulo256. Added template
    /// strokes each increment the byte even when their geometry overlaps.
    /// Storage limits and cancellation are original defensive behavior.
    /// </summary>
    public static byte Count(ReadOnlySpan<int> orderedSourceIdentifiers,
        int addedTemplateStrokes = 0, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (orderedSourceIdentifiers.Length > 4096 || addedTemplateStrokes is < 0 or > 4096)
            throw new ArgumentOutOfRangeException(nameof(orderedSourceIdentifiers), "Expected bounded prepared ranges/template strokes.");
        byte result = 0;
        for (int range = 0; range < orderedSourceIdentifiers.Length; range++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int earlier = 0;
            while (earlier < range && orderedSourceIdentifiers[earlier] != orderedSourceIdentifiers[range]) earlier++;
            if (earlier == range) result = unchecked((byte)(result + 1));
        }
        return unchecked((byte)(result + addedTemplateStrokes));
    }
}
