using Avalonia;

namespace NovaPen;

internal sealed class ScreenRegionSelection
{
    private PixelPoint? _start;
    internal PixelRect? Region { get; private set; }
    internal void Begin(PixelPoint point) { _start = point; Move(point); }
    internal void Move(PixelPoint point)
    {
        if (_start is not { } start) return;
        Region = new PixelRect(Math.Min(start.X, point.X), Math.Min(start.Y, point.Y),
            checked((int)Math.Abs((long)point.X - start.X)), checked((int)Math.Abs((long)point.Y - start.Y)));
    }
    internal PixelRect? Complete() => Region is { Width: >= 3, Height: >= 3 } region ? region : null;
    internal static Rect[] Outside(Rect screen, Rect? selected)
    {
        var inner = selected?.Intersect(screen) ?? default;
        if (inner.Width <= 0 || inner.Height <= 0) return [screen];
        return [new(screen.X, screen.Y, screen.Width, inner.Y - screen.Y),
            new(screen.X, inner.Bottom, screen.Width, screen.Bottom - inner.Bottom),
            new(screen.X, inner.Y, inner.X - screen.X, inner.Height),
            new(inner.Right, inner.Y, screen.Right - inner.Right, inner.Height)];
    }
}
