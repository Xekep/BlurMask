using Avalonia;

namespace BlurMask;

// Owns the native catalog only while the interactive picker is active.
internal sealed class WindowFillPicker(IWindowCatalog catalog) : IDisposable
{
    internal TrackedWindow? Region { get; private set; }
    internal TrackedWindow? Move(PixelPoint point, bool force = false)
    {
        // Pointer moves are not followed by a timer. Reusing an earlier hit here
        // can leave its preview visible indefinitely when the pointer stops.
        var region = catalog.FindChildAt(point);
        Region = region is { Visible: true } && region.ProcessId != Environment.ProcessId &&
            region.Bounds.Width > 0 && region.Bounds.Height > 0 && region.Bounds.Contains(point) ? region : null;
        return Region;
    }
    internal void Clear() { Region = null; }
    public void Dispose() { Clear(); catalog.Dispose(); }
}
