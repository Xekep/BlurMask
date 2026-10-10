using Avalonia;
using Avalonia.Media;

namespace BlurMask;

// Replace marks during dragging so undo snapshots stay immutable.
internal sealed class InkSelection(InkDocument document)
{
    internal Guid? Selected { get; private set; }
    private InkMark? _original;
    private Point _start;
    internal void Begin(Point point, double tolerance)
    {
        _original = document.Marks.LastOrDefault(m => m.Hit(point, tolerance));
        Selected = _original?.Id;
        _start = point;
        if (_original is not null) document.BeginEdit();
        document.Notify();
    }
    internal void Move(Point point, bool limitToCanvas = false)
    {
        if (_original is null) return;
        var index = document.Marks.FindIndex(m => m.Id == _original.Id);
        if (index < 0) return;
        var delta = point - _start;
        if (limitToCanvas)
        {
            var limit = CanvasGeometry.CoordinateLimit;
            delta = new Point(Math.Clamp(delta.X, -limit - _original.Points.Min(p => p.X), limit - _original.Points.Max(p => p.X)),
                Math.Clamp(delta.Y, -limit - _original.Points.Min(p => p.Y), limit - _original.Points.Max(p => p.Y)));
        }
        document.Marks[index] = _original.Translate(delta);
        document.Notify();
    }
    internal void Complete() { if (_original is not null) document.Commit(); _original = null; }
    internal void Cancel() { if (_original is not null) document.Cancel(); _original = null; }
    internal void Clear() { Cancel(); Selected = null; document.Notify(); }
    internal void Delete()
    {
        Cancel();
        if (Selected is not { } id) return;
        document.BeginEdit(); document.Marks.RemoveAll(m => m.Id == id); document.Commit(); Selected = null;
    }
    internal void Draw(DrawingContext context, double scale)
    {
        if (Selected is not { } id || document.Marks.Find(m => m.Id == id) is not { } mark) return;
        var rect = mark.Bounds.Inflate(5 / scale);
        var brush = new SolidColorBrush(Color.Parse("#00A7CC"));
        context.DrawRectangle(null, new Pen(brush, 1.5 / scale, new DashStyle([4, 3], 0)), rect);
        foreach (var point in new[] {rect.TopLeft, rect.TopRight, rect.BottomLeft, rect.BottomRight})
            context.DrawRectangle(Brushes.White, new Pen(brush, 1 / scale), new Rect(point - new Vector(3 / scale, 3 / scale), new Size(6 / scale, 6 / scale)));
    }
}
