using Avalonia;

namespace NovaPen;

// World coordinates never change when the board grows. Bounds are reconstructed
// from shared marks, so older documents and concurrent edits need no migration.
internal static class CanvasGeometry
{
    internal const double CoordinateLimit = 100000;
    internal static double PenWidth(double screenWidth, double zoom) => Math.Clamp(screenWidth / zoom, .5, 200);
    internal static bool IntersectsEraser(Rect bounds, Point point, double radius)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0 || !double.IsFinite(radius) || radius < 0) return false;
        var nearest = new Point(Math.Clamp(point.X, bounds.Left, bounds.Right), Math.Clamp(point.Y, bounds.Top, bounds.Bottom));
        return ((Vector)(point - nearest)).SquaredLength <= radius * radius;
    }
    internal static Rect Grow(Rect board, Rect ink)
    {
        const double gutter = 256, step = 1024;
        var left = board.Left; var top = board.Top; var right = board.Right; var bottom = board.Bottom;
        if (ink.Left < left + gutter) left = Math.Floor((ink.Left - gutter) / step) * step;
        if (ink.Top < top + gutter) top = Math.Floor((ink.Top - gutter) / step) * step;
        if (ink.Right > right - gutter) right = Math.Ceiling((ink.Right + gutter) / step) * step;
        if (ink.Bottom > bottom - gutter) bottom = Math.Ceiling((ink.Bottom + gutter) / step) * step;
        return new Rect(left, top, right - left, bottom - top);
    }
    internal static Rect ForMarks(int width, int height, IEnumerable<InkMark> marks)
    {
        var bounds = new Rect(0, 0, width, height);
        foreach (var mark in marks) bounds = Grow(bounds, mark.Bounds);
        return bounds;
    }
    internal static (PixelSize Size, double Scale) ExportSize(Rect bounds)
    {
        var scale = Math.Min(1, Math.Min(4096 / bounds.Width, 4096 / bounds.Height));
        return (new PixelSize(Math.Max(1, (int)Math.Ceiling(bounds.Width * scale)),
            Math.Max(1, (int)Math.Ceiling(bounds.Height * scale))), scale);
    }
}
