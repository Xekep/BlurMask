using Avalonia;
using Avalonia.Media;
using System.Globalization;
using BlurMask.Shared;

namespace BlurMask;

internal enum InkTool { Cursor, Pen, Highlighter, Eraser, Line, Arrow, Rectangle, Ellipse, Text }
internal enum BoardMode { Desktop, White, Black }

// Coordinates are physical desktop pixels, including negative monitor origins. Each
// monitor transforms the same document into its own logical coordinate space.
internal sealed class InkMark(InkTool tool, Color color, double width, Point start)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public InkTool Tool { get; } = tool;
    public Color Color { get; } = color;
    public double Width { get; } = width;
    public List<Point> Points { get; } = [start];
    public string Text { get; set; } = "";
    public double FontSize { get; set; } = 28;
    private StreamGeometry? _geometry;

    public void AddPoint(Point point)
    {
        if (Tool is InkTool.Pen or InkTool.Highlighter)
        {
            if (((Vector)(point - Points[^1])).Length < 0.65) return;
            Points.Add(point);
        }
        else if (Points.Count == 1) Points.Add(point);
        else Points[1] = point;
        _geometry = null;
    }

    public void Draw(DrawingContext context)
    {
        var brush = new SolidColorBrush(Color);
        var pen = new Pen(brush, Width, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        var start = Points[0];
        var end = Points[^1];
        var rect = new Rect(start, end).Normalize();
        using var opacity = context.PushOpacity(Tool == InkTool.Highlighter ? 0.35 : 1);
        switch (Tool)
        {
            case InkTool.Text:
                context.DrawText(new FormattedText(Text, CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, Typeface.Default, FontSize, brush), start);
                break;
            case InkTool.Rectangle: context.DrawRectangle(null, pen, rect); break;
            case InkTool.Ellipse: context.DrawEllipse(null, pen, rect); break;
            case InkTool.Line: context.DrawLine(pen, start, end); break;
            case InkTool.Arrow:
                context.DrawLine(pen, start, end);
                var angle = Math.Atan2(end.Y - start.Y, end.X - start.X);
                var length = Math.Max(14, Width * 3.5);
                context.DrawLine(pen, end, end - new Vector(Math.Cos(angle - 0.5), Math.Sin(angle - 0.5)) * length);
                context.DrawLine(pen, end, end - new Vector(Math.Cos(angle + 0.5), Math.Sin(angle + 0.5)) * length);
                break;
            default:
                if (Points.Count == 1) context.DrawEllipse(brush, null, start, Width / 2, Width / 2);
                else
                {
                    // One geometry per stroke and one opacity layer per highlighter,
                    // so segment joins don't accumulate alpha and turn into dark blobs.
                    if (_geometry is null)
                    {
                        _geometry = new StreamGeometry();
                        using var path = _geometry.Open();
                        path.BeginFigure(start, false);
                        foreach (var point in Points.Skip(1)) path.LineTo(point);
                        path.EndFigure(false);
                    }
                    context.DrawGeometry(null, pen, _geometry);
                }
                break;
        }
    }

    public bool Hit(Point point, double radius)
    {
        radius += Width / 2;
        var start = Points[0];
        var end = Points[^1];
        var rect = new Rect(start, end).Normalize();
        if (Tool == InkTool.Text)
        {
            var text = new FormattedText(Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                Typeface.Default, FontSize, Brushes.Black);
            return new Rect(start, new Size(text.Width, text.Height)).Inflate(radius).Contains(point);
        }
        if (Tool == InkTool.Rectangle)
            return rect.Inflate(radius).Contains(point) &&
                (rect.Width <= radius * 2 || rect.Height <= radius * 2 || !rect.Deflate(radius).Contains(point));
        if (Tool == InkTool.Ellipse)
        {
            var rx = rect.Width / 2;
            var ry = rect.Height / 2;
            if (rx < 1 || ry < 1) return Distance(point, start, end) <= radius;
            var dx = point.X - rect.Center.X;
            var dy = point.Y - rect.Center.Y;
            var angle = Math.Atan2(dy / ry, dx / rx);
            return ((Vector)(point - new Point(rect.Center.X + rx * Math.Cos(angle), rect.Center.Y + ry * Math.Sin(angle)))).Length <= radius;
        }
        if (Tool == InkTool.Arrow)
        {
            var angle = Math.Atan2(end.Y - start.Y, end.X - start.X);
            var length = Math.Max(14, Width * 3.5);
            if (Distance(point, end, end - new Vector(Math.Cos(angle - 0.5), Math.Sin(angle - 0.5)) * length) <= radius ||
                Distance(point, end, end - new Vector(Math.Cos(angle + 0.5), Math.Sin(angle + 0.5)) * length) <= radius) return true;
        }
        if (Points.Count == 1) return ((Vector)(point - start)).Length <= radius;
        for (var i = 1; i < Points.Count; i++)
            if (Distance(point, Points[i - 1], Points[i]) <= radius) return true;
        return false;
    }

    internal static double Distance(Point point, Point a, Point b)
    {
        var segment = b - a;
        var square = segment.X * segment.X + segment.Y * segment.Y;
        if (square == 0) return ((Vector)(point - a)).Length;
        var offset = point - a;
        var t = Math.Clamp((offset.X * segment.X + offset.Y * segment.Y) / square, 0, 1);
        return ((Vector)(point - (a + segment * t))).Length;
    }
}

internal sealed class InkDocument
{
    private sealed record Edit(List<InkMark> Before, List<InkMark> After);
    private readonly List<Edit> _undo = [];
    private readonly Stack<Edit> _redo = [];
    private List<InkMark>? _before;
    public List<InkMark> Marks { get; private set; } = [];
    public InkMark? Preview { get; set; }
    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;
    public bool HasActiveEdit => _before is not null;
    public event Action? Changed;

    public void BeginEdit() { _before ??= new List<InkMark>(Marks); }
    public void Notify() => Changed?.Invoke();
    public void Commit()
    {
        if (_before is not null && !_before.SequenceEqual(Marks))
        {
            _undo.Add(new Edit(_before, new List<InkMark>(Marks)));
            if (_undo.Count > 100) _undo.RemoveAt(0);
            _redo.Clear();
        }
        _before = null;
        Preview = null;
        Notify();
    }
    public void Cancel()
    {
        if (_before is not null) Marks = _before;
        _before = null;
        Preview = null;
        Notify();
    }
    public void Add(InkMark mark) { BeginEdit(); Marks.Add(mark); Commit(); }
    public void Clear() { BeginEdit(); Marks.Clear(); Commit(); }
    public void Undo()
    {
        Cancel();
        if (_undo.Count == 0) return;
        var edit = _undo[^1];
        _undo.RemoveAt(_undo.Count - 1);
        ApplyHistory(edit.After, edit.Before);
        _redo.Push(edit);
        Notify();
    }
    public void Redo()
    {
        Cancel();
        if (!_redo.TryPop(out var edit)) return;
        ApplyHistory(edit.Before, edit.After);
        _undo.Add(edit);
        Notify();
    }
    private void ApplyHistory(List<InkMark> from, List<InkMark> to)
    {
        var keep = to.Select(m => m.Id).ToHashSet();
        var remove = from.Where(m => !keep.Contains(m.Id)).Select(m => m.Id).ToHashSet();
        Marks.RemoveAll(m => remove.Contains(m.Id));
        var existing = Marks.Select(m => m.Id).ToHashSet();
        var previous = from.Select(m => m.Id).ToHashSet();
        for (var i = 0; i < to.Count; i++)
            if (!previous.Contains(to[i].Id) && existing.Add(to[i].Id)) Marks.Insert(Math.Min(i, Marks.Count), to[i]);
    }
    public List<InkMarkDto> Export() => Marks.Select(mark => new InkMarkDto
    {
        Id = mark.Id, Tool = mark.Tool.ToString(), Color = mark.Color.A == 255
            ? $"#{mark.Color.R:X2}{mark.Color.G:X2}{mark.Color.B:X2}"
            : $"#{mark.Color.R:X2}{mark.Color.G:X2}{mark.Color.B:X2}{mark.Color.A:X2}", Width = mark.Width,
        FontSize = mark.FontSize, Text = mark.Text,
        Points = mark.Points.Select(p => new InkPointDto(p.X, p.Y)).ToList()
    }).ToList();
    public void Import(List<InkMarkDto> marks, bool resetHistory = false)
    {
        if (HasActiveEdit) throw new InvalidOperationException("Finish the drawing gesture before importing.");
        var previous = Marks.ToDictionary(m => m.Id);
        Marks = marks.Select(dto =>
        {
            if (previous.TryGetValue(dto.Id, out var existing)) return existing;
            var cssColor = dto.Color.Length == 9 ? "#" + dto.Color[7..9] + dto.Color[1..7] : dto.Color;
            var mark = new InkMark(Enum.Parse<InkTool>(dto.Tool), Color.Parse(cssColor), dto.Width,
                new Point(dto.Points[0].X, dto.Points[0].Y))
            { Id = dto.Id, Text = dto.Text, FontSize = dto.FontSize };
            foreach (var point in dto.Points.Skip(1)) mark.AddPoint(new Point(point.X, point.Y));
            return mark;
        }).ToList();
        if (resetHistory) { _undo.Clear(); _redo.Clear(); }
        Notify();
    }
}
