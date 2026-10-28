namespace InkRecognition.Math;

// Concrete implementations are created only by the original field's gated
// selected-Ink factory. Opaque C22 handles belong to that exact retained clone.
// Snapshot methods return inspection data; they never restore owning authority.
internal abstract class Math2Mode0RetainedInk : IMath2DomCb8Services, IMath2DomCb8Removal, IDisposable
{
    internal abstract object Identity { get; }
    internal abstract LegacyLibraryOriginalDomNameOwner Names { get; }
    internal abstract long SnapshotNext { get; }
    internal abstract Math2Mode0PublishRegistry[] BorrowRegistry();
    internal abstract Math2Mode0PublishRegistry[] CopyRegistry();
    internal abstract object CreateSelection(ReadOnlySpan<LegacyNativeInkPositionRange> ranges);
    internal abstract LegacyNativeInkPositionRange[] CopySelectionRanges(object selection);
    internal abstract long Register(LegacyLibraryOriginalDomName name, object selection);
    public abstract int CompareName(object left, object right);
    public abstract object? FindOldSelection(long id);
    public abstract object? FindOldName(long id);
    public abstract int CountRanges(object selection);
    public abstract bool EqualRanges(object oldSelection, object pendingSelection);
    public abstract bool RemoveSelection(long id);
    public abstract void Dispose();
}
