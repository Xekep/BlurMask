namespace InkRecognition.Math;

// Must be supplied by the same retained Ink owner as the old/new tag vectors.
// A list of matching numeric IDs is not authority to delete selections.
internal interface IMath2DomCb8Removal
{
    bool RemoveSelection(long id);
}

internal static class Math2DomCb8TagCleanup
{
    // Document0339D0 / CB8.V50. Ordered vectors, not mathematical sets.
    // Native stops at first failed removal and leaves earlier removals applied.
    internal static bool RemoveStale(ReadOnlySpan<long> oldTags, ReadOnlySpan<long> newTags,
        IMath2DomCb8Removal retainedInk, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(retainedInk);
        foreach (long id in oldTags)
        {
            token.ThrowIfCancellationRequested();
            bool retained = false;
            foreach (long next in newTags)
                if (id == next) { retained = true; break; }
            if (!retained && !retainedInk.RemoveSelection(id)) return false;
        }
        return true;
    }
}
