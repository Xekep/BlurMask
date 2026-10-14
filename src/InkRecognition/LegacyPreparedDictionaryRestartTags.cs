namespace InkRecognition;

/// <summary>Actual dictionary2b7 runtime78/80 restart tags. They are packed
///source-query data, not symbol-context identities or native pointers.</summary>
public readonly record struct LegacyPreparedDictionaryRestartTags(ulong Root78, ulong Subword80)
{
    /// <summary>Literal fresh092790 writes; configured runtime overrides are
    ///represented by an explicitly supplied pair instead of guessed defaults.</summary>
    public static LegacyPreparedDictionaryRestartTags CreateFresh()
        => new(8UL << 40, 1UL | (12UL << 40));

    /// <summary>093f60 tests incoming source tag header05.bit2.</summary>
    public ulong Resolve(ulong incomingSourceTag) => (incomingSourceTag & (4UL << 40)) != 0 ? Subword80 : Root78;

    /// <summary>Actual28d.virtual50=236b50: select the owned child, invoke its
    ///restart getter with byte04 cleared, then stamp that byte only if the
    ///returned header05.bit2 is set. All other64-bit tag bytes survive.
    ///Raw assembly is required here: decompilation loses the upper24 bits.</summary>
    public static ulong ResolveAggregate(ulong incomingSourceTag,
        IReadOnlyList<LegacyPreparedDictionaryRestartTags> selectedChildren)
    {
        ArgumentNullException.ThrowIfNull(selectedChildren);
        byte sourceId = (byte)(incomingSourceTag >> 32);
        if (selectedChildren.Count > 255 || sourceId == 0 || sourceId > selectedChildren.Count)
            throw new ArgumentException("Actual bounded selected source ID must own its restart getter.");
        ulong result = selectedChildren[sourceId - 1].Resolve(incomingSourceTag & ~(255UL << 32));
        return (result & (4UL << 40)) != 0 ? (result & ~(255UL << 32)) | (ulong)sourceId << 32 : result;
    }
}
