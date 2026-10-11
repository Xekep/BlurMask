using Avalonia;
using Avalonia.Media;
using Avalonia.Input;
using BlurMask.Shared;

namespace BlurMask;

// Replace marks during dragging so undo snapshots stay immutable.
internal sealed class InkSelection(InkDocument document)
{
    private readonly HashSet<Guid> _selected = [];
    internal IReadOnlyCollection<Guid> SelectedIds => _selected;
    internal Guid? Selected => _selected.Count == 1 ? _selected.First() : null;
    private InkMark? _original;
    private InkMark[] _originals = [];
    private int[] _indices = [];
    private bool _marquee;
    private Point _end;
    private double _marqueeThreshold;
    private HashSet<Guid> _marqueeBefore = [], _marqueeBase = [];
    private Point _start;
    private int _handle = -1;
    private static readonly Dictionary<StandardCursorType, Cursor> Cursors = new();
    internal static Cursor NativeCursor(StandardCursorType type)
    { if (!Cursors.TryGetValue(type, out var cursor)) Cursors[type] = cursor = new(type); return cursor; }
    private static readonly (double X, double Y)[] Positions = [(0,0),(.5,0),(1,0),(1,.5),(1,1),(.5,1),(0,1),(0,.5)];
    private InkMark? Current
    {
        get
        {
            if (_selected.Count == 0) return null;
            if (_selected.Count == 1) return document.Marks.Find(m => m.Id == _selected.First());
            var marks = document.Marks.Where(m => _selected.Contains(m.Id)).ToArray();
            if (marks.Length == 0) return null;
            if (marks.Length == 1) return marks[0];
            var bounds = marks.Select(m => m.Bounds).Aggregate((a, b) => a.Union(b));
            var frame = new InkMark(InkTool.Rectangle, Colors.Transparent, 0, bounds.TopLeft);
            frame.AddPoint(bounds.BottomRight); return frame;
        }
    }
    internal static Point HandlePoint(InkMark mark, int index, double scale)
    {
        var box = mark.BaseBounds; var p = Positions[index < 8 ? index : (index - 8) * 2];
        var point = new Point(box.X + box.Width * p.X, box.Y + box.Height * p.Y).Transform(mark.Transform);
        if (index < 8) return point;
        var outward = (Vector)(point - box.Center.Transform(mark.Transform));
        return point + outward / Math.Max(.001, outward.Length) * (20 / scale);
    }
    private static int HitHandle(InkMark mark, Point point, double scale)
    {
        var best = -1; var distance = 8 / scale;
        for (var i = 0; i < 12; i++)
        { var d = ((Vector)(point - HandlePoint(mark, i, scale))).Length; if (d <= distance) { distance = d; best = i; } }
        return best;
    }
    internal StandardCursorType CursorAt(Point point, double scale)
    {
        if (Current is not { } mark) return StandardCursorType.Arrow;
        var handle = _original is not null ? _handle : HitHandle(mark, point, scale);
        if (handle >= 8) return StandardCursorType.Hand;
        if (handle < 0) return (_selected.Count > 1 ? mark.Bounds.Contains(point) : mark.Hit(point, 6 / scale))
            ? StandardCursorType.SizeAll : StandardCursorType.Arrow;
        var p = Positions[handle]; var x = p.X - .5; var y = p.Y - .5;
        var angle = Math.Atan2(x * mark.Transform.M12 + y * mark.Transform.M22, x * mark.Transform.M11 + y * mark.Transform.M21);
        var axis = ((int)Math.Round(angle / (Math.PI / 4)) % 4 + 4) % 4;
        return axis switch { 0 => StandardCursorType.SizeWestEast, 1 => StandardCursorType.TopLeftCorner,
            2 => StandardCursorType.SizeNorthSouth, _ => StandardCursorType.TopRightCorner };
    }
    internal void Begin(Point point, double tolerance, bool toggle = false)
    {
        Cancel();
        var existing = document.Marks.Select(m => m.Id).ToHashSet();
        _selected.IntersectWith(existing);
        var frame = Current;
        _handle = !toggle && tolerance > 0 && frame is not null ? HitHandle(frame, point, 6 / tolerance) : -1;
        var hit = _handle < 0 ? document.Marks.LastOrDefault(m => m.Hit(point, tolerance)) : null;
        _start = point;
        if (toggle && hit is not null)
        {
            if (!_selected.Add(hit.Id)) _selected.Remove(hit.Id);
            document.Notify(); return;
        }
        if (_handle < 0 && hit is not null)
        {
            if (!_selected.Contains(hit.Id)) { _selected.Clear(); _selected.Add(hit.Id); }
        }
        else if (_handle < 0 && (toggle || frame is null || _selected.Count < 2 || !frame.Bounds.Contains(point)))
        {
            _marqueeBefore = [.. _selected];
            if (!toggle) _selected.Clear();
            _marqueeBase = [.. _selected];
            _marquee = true; _end = point; _marqueeThreshold = Math.Max(.001, tolerance / 2);
            document.Notify(); return;
        }
        _original = Current;
        _originals = document.Marks.Where(m => _selected.Contains(m.Id)).ToArray();
        var indices = document.Marks.Select((m, i) => (m.Id, i)).ToDictionary(p => p.Id, p => p.i);
        _indices = _originals.Select(m => indices[m.Id]).ToArray();
        if (_originals.Length > 0) document.BeginEdit();
        document.Notify();
    }
    internal void Move(Point point, bool limitToCanvas = false, bool constrain = false)
    {
        if (_marquee)
        {
            _end = point; _selected.Clear(); _selected.UnionWith(_marqueeBase);
            if (((Vector)(_end - _start)).Length >= _marqueeThreshold)
            {
                var area = new Rect(_start, _end).Normalize();
                foreach (var mark in document.Marks) if (area.Intersects(mark.Bounds)) _selected.Add(mark.Id);
            }
            document.Notify(); return;
        }
        if (_original is null || _originals.Length == 0) return;
        InkMark replacement;
        if (_handle >= 8)
        {
            var center = _original.BaseBounds.Center.Transform(_original.Transform);
            var first = _start - center; var next = point - center;
            var angle = Math.Atan2(next.Y, next.X) - Math.Atan2(first.Y, first.X);
            if (constrain) angle = Math.Round(angle / (Math.PI / 12)) * (Math.PI / 12);
            replacement = _original.WithTransform(_original.Transform * Matrix.CreateTranslation(-center.X, -center.Y) *
                Matrix.CreateRotation(angle) * Matrix.CreateTranslation(center.X, center.Y));
        }
        else if (_handle >= 0)
        {
            if (!_original.Transform.TryInvert(out var inverse)) return;
            var delta = point.Transform(inverse) - _start.Transform(inverse);
            var box = _original.BaseBounds; var active = Positions[_handle];
            var anchor = new Point(box.X + box.Width * (1 - active.X), box.Y + box.Height * (1 - active.Y));
            var sx = active.X == .5 ? 1 : Math.Max(.01, 1 + delta.X / Math.CopySign(Math.Max(.01, box.Width), active.X - .5));
            var sy = active.Y == .5 ? 1 : Math.Max(.01, 1 + delta.Y / Math.CopySign(Math.Max(.01, box.Height), active.Y - .5));
            if (constrain) sx = sy = _handle is 1 or 5 ? sy : _handle is 3 or 7 ? sx : Math.Max(sx, sy);
            replacement = _original.WithTransform(Matrix.CreateTranslation(-anchor.X, -anchor.Y) * Matrix.CreateScale(sx, sy) *
                Matrix.CreateTranslation(anchor.X, anchor.Y) * _original.Transform);
        }
        else
        {
            var delta = point - _start;
            if (limitToCanvas)
            {
                var b = _original.Bounds; var limit = CanvasGeometry.CoordinateLimit;
                delta = new(Math.Clamp(delta.X, -limit - b.Left, limit - b.Right), Math.Clamp(delta.Y, -limit - b.Top, limit - b.Bottom));
            }
            replacement = _original.Translate(delta);
        }
        var replacements = new InkMark[_originals.Length];
        for (var i = 0; i < _originals.Length; i++)
        {
            replacements[i] = _originals.Length == 1 ? replacement : _handle < 0
                ? _originals[i].Translate(replacement.Points[0] - _original.Points[0])
                : _originals[i].WithTransform(_originals[i].Transform * replacement.Transform);
            var bounds = replacements[i].Bounds; var t = replacements[i].Transform;
            if (!double.IsFinite(bounds.Width) || !double.IsFinite(bounds.Height) ||
                Math.Max(Math.Max(Math.Abs(t.M11), Math.Abs(t.M12)), Math.Max(Math.Abs(t.M21), Math.Abs(t.M22))) > 1000 ||
                Math.Abs(t.M11 * t.M22 - t.M12 * t.M21) < 1e-8 || bounds.Left < -100000 || bounds.Top < -100000 ||
                bounds.Right > 100000 || bounds.Bottom > 100000) return;
            var index = _indices[i];
            if (index < 0 || index >= document.Marks.Count || document.Marks[index].Id != _originals[i].Id)
                _indices[i] = document.Marks.FindIndex(m => m.Id == _originals[i].Id);
            if (_indices[i] < 0) return;
        }
        // Validate the whole group before modifying any mark; one drag is one edit.
        for (var i = 0; i < replacements.Length; i++) document.Marks[_indices[i]] = replacements[i];
        document.Notify();
    }
    internal void Complete()
    {
        var editing = _originals.Length > 0;
        _original = null; _originals = []; _indices = []; _marquee = false;
        if (editing) document.Commit(); else document.Notify();
    }
    internal void Cancel()
    {
        var editing = _originals.Length > 0; var marquee = _marquee;
        _original = null; _originals = []; _indices = []; _marquee = false;
        if (marquee) { _selected.Clear(); _selected.UnionWith(_marqueeBefore); }
        if (editing) document.Cancel(); else if (marquee) document.Notify();
    }
    internal void Clear() { Cancel(); _selected.Clear(); document.Notify(); }
    internal void SelectAll() { Cancel(); _selected.Clear(); _selected.UnionWith(document.Marks.Select(m => m.Id)); document.Notify(); }
    internal void Delete()
    {
        Cancel();
        if (_selected.Count == 0) return;
        document.BeginEdit(); document.Marks.RemoveAll(m => _selected.Contains(m.Id)); _selected.Clear(); document.Commit();
    }
    internal void Draw(DrawingContext context, double scale)
    {
        var brush = new SolidColorBrush(Color.Parse("#00A7CC"));
        if (_marquee)
        {
            context.DrawRectangle(new SolidColorBrush(Color.FromArgb(25, 0, 167, 204)),
                new Pen(brush, 1 / scale), new Rect(_start, _end).Normalize());
        }
        if (Current is not { } mark) return;
        var pen = new Pen(brush, 1 / scale);
        var corners = new[] { 0, 2, 4, 6 }.Select(i => HandlePoint(mark, i, scale)).ToArray();
        var frame = new Pen(brush, 1.5 / scale, new DashStyle([4, 3], 0));
        for (var i = 0; i < 4; i++) context.DrawLine(frame, corners[i], corners[(i + 1) % 4]);
        for (var i = 0; i < 8; i++)
        {
            var point = HandlePoint(mark, i, scale);
            context.DrawRectangle(Brushes.White, pen, new Rect(point - new Vector(3.5 / scale, 3.5 / scale), new Size(7 / scale, 7 / scale)));
        }
        for (var i = 8; i < 12; i++)
        {
            var point = HandlePoint(mark, i, scale); var r = 4 / scale;
            context.DrawLine(pen, corners[i - 8], point);
            context.DrawEllipse(Brushes.White, pen, point, r, r);
            context.DrawLine(pen, point + new Vector(-r, 0), point + new Vector(0, -r));
            context.DrawLine(pen, point + new Vector(0, -r), point + new Vector(r / 3, -r / 3));
        }
    }
}
