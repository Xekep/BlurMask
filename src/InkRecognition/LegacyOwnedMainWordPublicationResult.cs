namespace InkRecognition;

/// <summary>Result of the owning21FE30 callback. Only that callback creates
/// completion tokens for the exact associated flush consumed by218560.</summary>
public sealed class LegacyOwnedMainWordPublicationResult
{
    public LegacyOwnedMainAssociatedGlyphFlush Association {get;}
    public bool Succeeded {get;}
    public LegacyNativePublishedWordNode? PublishedWord {get;}
    internal LegacyOwnedMainWordPublicationResult(LegacyOwnedMainAssociatedGlyphFlush association,
        bool succeeded,LegacyNativePublishedWordNode? publishedWord)
    {Association=association;Succeeded=succeeded;PublishedWord=publishedWord;}
}
