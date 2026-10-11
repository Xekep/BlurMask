using Avalonia;

namespace BlurMask;

// Owns the native catalog only while the interactive picker is active.
internal sealed class WindowFillPicker(IWindowCatalog catalog) : IDisposable
{
    private long _lastLookup = long.MinValue;
    internal TrackedWindow? Region { get; private set; }
    internal TrackedWindow? Move(PixelPoint point, bool force = false)
    {
        var now = Environment.TickCount64;
        if (!force && _lastLookup != long.MinValue && now - _lastLookup < 32) return Region;
        _lastLookup = now;
        var region = catalog.FindChildAt(point);
        Region = region is { Visible: true } && region.ProcessId != Environment.ProcessId &&
            region.Bounds.Width > 0 && region.Bounds.Height > 0 && region.Bounds.Contains(point) ? region : null;
        return Region;
    }
    internal void Clear() { Region = null; _lastLookup = long.MinValue; }
    public void Dispose() { Clear(); catalog.Dispose(); }
}
