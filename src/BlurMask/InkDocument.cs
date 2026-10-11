using Avalonia;
using Avalonia.Media;
using System.Globalization;
using System.Diagnostics;
using BlurMask.Shared;
using Avalonia.Media.Imaging;

namespace BlurMask;

internal enum InkTool { Cursor, Select, Pen, Highlighter, Eraser, Line, Arrow, Rectangle, Ellipse, Text, Image, AiPen }
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
    private string _text = "";
    private double _fontSize = 28;
    public string Text { get => _text; set { if (_text != value) { _text = value; _bounds = null; _formatted = null; _textGeometry = null; } } }
    public double FontSize { get => _fontSize; set { if (_fontSize != value) { _fontSize = value; _bounds = null; _formatted = null; _textGeometry = null; } } }
    public string? Image { get; init; }
    public Matrix Transform { get; init; } = Matrix.Identity;
    public string Font { get; init; } = "";
    private Bitmap? _bitmap;
    private bool _imageDecoded;
    public double FadeSeconds { get; init; }
    internal long FadeStartedAt { get; private set; }
    private StreamGeometry? _geometry;
    private Rect? _bounds;
    private SolidColorBrush? _brush;
    private Pen? _pen;
    private FormattedText? _formatted;
    private Geometry? _textGeometry;
    private string? _textCulture;
    private FormattedText Formatted()
    {
        var culture = CultureInfo.CurrentCulture;
        if (_formatted is null || _textCulture != culture.Name)
        { _formatted = new(Text, culture, FlowDirection.LeftToRight, InkFonts.Face(Font), FontSize, _brush ??= new(Color)); _textCulture = culture.Name; _bounds = null; _textGeometry = null; }
        return _formatted;
    }

    internal void StartFading() => FadeStartedAt = Stopwatch.GetTimestamp();
    internal double FadeOpacity(long now)
    {
        if (FadeSeconds <= 0 || FadeStartedAt == 0) return 1;
        var remaining = FadeSeconds - Stopwatch.GetElapsedTime(FadeStartedAt, now).TotalSeconds;
        return Math.Clamp(remaining / Math.Min(1, FadeSeconds / 3), 0, 1);
    }

    public void AddPoint(Point point)
    {
        if (Tool is InkTool.Pen or InkTool.Highlighter)
        {
            if (((Vector)(point - Points[^1])).Length < 0.65) return;
            Points.Add(point);
            if (_bounds is { } bounds) _bounds = bounds.Union(new Rect(point, point).Inflate(Width / 2));
        }
        else
        {
            if (Points.Count == 1) Points.Add(point);
            else Points[1] = point;
            _bounds = null;
        }
        _geometry = null;
    }

    public void Draw(DrawingContext context)
    {
        using var transformed = context.PushTransform(Transform);
        var brush = _brush ??= new SolidColorBrush(Color);
        var pen = _pen ??= new Pen(brush, Width, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        var start = Points[0];
        var end = Points[^1];
        var rect = new Rect(start, end).Normalize();
        using var opacity = context.PushOpacity(FadeOpacity(Stopwatch.GetTimestamp()) * (Tool == InkTool.Highlighter ? 0.35 : 1));
        switch (Tool)
        {
            case InkTool.Image:
                if (!_imageDecoded)
                {
                    _imageDecoded = true;
                    try { if (BoardValidation.ValidImage(Image)) _bitmap = new Bitmap(new MemoryStream(Convert.FromBase64String(Image!))); }
                    catch (Exception ex) when (ex is ArgumentException or InvalidDataException or NotSupportedException) { }
                }
                if (_bitmap is not null) context.DrawImage(_bitmap, rect);
                break;
            case InkTool.Text:
                if (Points.Count == 2)
                {
                    var glyph = _textGeometry ??= (Font == "STIX Two Math" ? MathTypesetting.Build(Text, FontSize) : null) ?? Formatted().BuildGeometry(default);
                    var source = glyph?.Bounds ?? default;
                    if (glyph is not null && source.Width > 0 && source.Height > 0 && rect.Width > 0 && rect.Height > 0)
                    {
                        using var fit = context.PushTransform(Matrix.CreateTranslation(-source.X, -source.Y) *
                            Matrix.CreateScale(rect.Width / source.Width, rect.Height / source.Height) * Matrix.CreateTranslation(rect.X, rect.Y));
                        context.DrawGeometry(brush, null, glyph);
                    }
                }
                else context.DrawText(Formatted(), start);
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
        if (!Transform.TryInvert(out var inverse)) return false;
        point = point.Transform(inverse);
        // Keep the hit slop usable after shrinking an object.
        radius *= Math.Max(Math.Sqrt(inverse.M11 * inverse.M11 + inverse.M12 * inverse.M12),
            Math.Sqrt(inverse.M21 * inverse.M21 + inverse.M22 * inverse.M22));
        radius += Width / 2;
        if (!BaseBounds.Inflate(radius).Contains(point)) return false;
        var start = Points[0];
        var end = Points[^1];
        var rect = new Rect(start, end).Normalize();
        if (Tool == InkTool.Image) return rect.Inflate(radius).Contains(point);
        if (Tool == InkTool.Text)
        {
            if (Points.Count == 2) return rect.Inflate(radius).Contains(point);
            var text = Formatted();
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

    internal Rect Bounds => BaseBounds.TransformToAABB(Transform);
    internal Rect BaseBounds
    {
        get
        {
            if (_bounds is { } cached && (Tool != InkTool.Text || _textCulture == CultureInfo.CurrentCulture.Name)) return cached;
            if (Tool == InkTool.Text)
            {
                if (Points.Count == 2) return new Rect(Points[0], Points[1]).Normalize();
                var text = Formatted();
                return (_bounds = new Rect(Points[0], new Size(text.Width, text.Height))).Value;
            }
            var minX = Points[0].X; var maxX = minX; var minY = Points[0].Y; var maxY = minY;
            foreach (var point in Points)
            { minX = Math.Min(minX, point.X); maxX = Math.Max(maxX, point.X); minY = Math.Min(minY, point.Y); maxY = Math.Max(maxY, point.Y); }
            return (_bounds = new Rect(new Point(minX, minY), new Point(maxX, maxY))
                .Inflate(Tool == InkTool.Arrow ? Math.Max(14, Width * 3.5) + Width / 2 : Width / 2)).Value;
        }
    }
    internal InkMark Translate(Vector delta)
    {
        if (delta == default) return this;
        if (Transform != Matrix.Identity) return WithTransform(Transform * Matrix.CreateTranslation(delta));
        var copy = new InkMark(Tool, Color, Width, Points[0] + delta)
            { Id = Id, Text = Text, FontSize = FontSize, Font = Font, Image = Image, FadeSeconds = FadeSeconds, FadeStartedAt = FadeStartedAt,
                _bitmap = _bitmap, _imageDecoded = _imageDecoded };
        copy.Points.Clear(); copy.Points.AddRange(Points.Select(p => p + delta));
        return copy;
    }
    internal InkMark WithTransform(Matrix transform)
    {
        if (transform == Transform) return this;
        var copy = new InkMark(Tool, Color, Width, Points[0])
            { Id = Id, Text = Text, FontSize = FontSize, Font = Font, Image = Image, Transform = transform,
                FadeSeconds = FadeSeconds, FadeStartedAt = FadeStartedAt, _bitmap = _bitmap, _imageDecoded = _imageDecoded,
                _geometry = _geometry, _brush = _brush, _pen = _pen, _formatted = _formatted, _textGeometry = _textGeometry,
                _textCulture = _textCulture, _bounds = _bounds };
        copy.Points.Clear(); copy.Points.AddRange(Points); return copy;
    }
    internal bool Same(InkMark other) => Tool == other.Tool && Color == other.Color && Width == other.Width &&
        Text == other.Text && FontSize == other.FontSize && Font == other.Font && Image == other.Image && Transform == other.Transform && Points.SequenceEqual(other.Points);
}

internal sealed class InkDocument
{
    private sealed record Edit(List<InkMark> Before, List<InkMark> After);
    private readonly List<Edit> _undo = [];
    private readonly Stack<Edit> _redo = [];
    private List<InkMark>? _before;
    public List<InkMark> Marks { get; private set; } = [];
    // Presentation ink is local and never enters saved/shared documents or undo history.
    public List<InkMark> FadingMarks { get; } = [];
    public InkMark? Preview { get; set; }
    public bool CanUndo => _undo.Count > 0 || FadingMarks.Count > 0;
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
    public void Add(InkMark mark) { BeginEdit(); Append(mark); Commit(); }
    public void CompletePreview()
    {
        if (Preview is { } mark) Append(mark);
        Commit();
    }
    internal bool ReplaceRecognized(IReadOnlyList<InkMark> strokes, InkMark result)
    {
        if (HasActiveEdit || strokes.Count == 0 || strokes.Any(m => !Marks.Contains(m) && !FadingMarks.Contains(m))) return false;
        var first = Marks.FindIndex(m => ReferenceEquals(m, strokes[0]));
        var ids = strokes.Select(m => m.Id).ToHashSet();
        BeginEdit(); Marks.RemoveAll(m => ids.Contains(m.Id)); FadingMarks.RemoveAll(m => ids.Contains(m.Id));
        if (result.FadeSeconds > 0) Append(result);
        else Marks.Insert(Math.Clamp(first, 0, Marks.Count), result);
        Commit(); return true;
    }
    private void Append(InkMark mark)
    {
        if (mark.FadeSeconds > 0)
        {
            mark.StartFading();
            if (FadingMarks.Count >= 5000) FadingMarks.RemoveAt(0);
            FadingMarks.Add(mark);
        }
        else Marks.Add(mark);
    }
    internal void ExpireFading(long now) => FadingMarks.RemoveAll(m => m.FadeOpacity(now) <= 0);
    public void Clear() { BeginEdit(); Marks.Clear(); FadingMarks.Clear(); Commit(); }
    public void Undo()
    {
        Cancel();
        if (FadingMarks.Count > 0) { FadingMarks.RemoveAt(FadingMarks.Count - 1); Notify(); return; }
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
        var fromById = from.ToDictionary(m => m.Id);
        var toById = to.ToDictionary(m => m.Id);
        for (var i = 0; i < Marks.Count; i++)
            if (fromById.TryGetValue(Marks[i].Id, out var old) && toById.TryGetValue(Marks[i].Id, out var replacement) &&
                !old.Same(replacement) && Marks[i].Same(old)) Marks[i] = replacement;
        for (var i = 0; i < to.Count; i++)
            if (!previous.Contains(to[i].Id) && existing.Add(to[i].Id)) Marks.Insert(Math.Min(i, Marks.Count), to[i]);
    }
    public List<InkMarkDto> Export() => Marks.Select(ExportMark).ToList();
    internal static InkMarkDto ExportMark(InkMark mark) => new()
    {
        Id = mark.Id, Tool = mark.Tool.ToString(), Color = mark.Color.A == 255
            ? $"#{mark.Color.R:X2}{mark.Color.G:X2}{mark.Color.B:X2}"
            : $"#{mark.Color.R:X2}{mark.Color.G:X2}{mark.Color.B:X2}{mark.Color.A:X2}", Width = mark.Width,
        FontSize = mark.FontSize, Font = mark.Font, Text = mark.Text, Image = mark.Image,
        Transform = mark.Transform == Matrix.Identity ? null : new(mark.Transform.M11, mark.Transform.M12, mark.Transform.M21,
            mark.Transform.M22, mark.Transform.M31, mark.Transform.M32),
        Points = mark.Points.Select(p => new InkPointDto(p.X, p.Y)).ToList()
    };
    public void Import(List<InkMarkDto> marks, bool resetHistory = false)
    {
        if (HasActiveEdit) throw new InvalidOperationException("Finish the drawing gesture before importing.");
        var previous = Marks.ToDictionary(m => m.Id);
        Marks = marks.Select(dto =>
        {
            var cssColor = dto.Color.Length == 9 ? "#" + dto.Color[7..9] + dto.Color[1..7] : dto.Color;
            var mark = new InkMark(Enum.Parse<InkTool>(dto.Tool), Color.Parse(cssColor), dto.Width,
                new Point(dto.Points[0].X, dto.Points[0].Y))
            { Id = dto.Id, Text = dto.Text, FontSize = dto.FontSize, Font = dto.Font, Image = dto.Image,
                Transform = dto.Transform is { } t ? new Matrix(t.M11, t.M12, t.M21, t.M22, t.OffsetX, t.OffsetY) : Matrix.Identity };
            foreach (var point in dto.Points.Skip(1)) mark.AddPoint(new Point(point.X, point.Y));
            if (previous.TryGetValue(dto.Id, out var existing) && existing.Same(mark)) return existing;
            return mark;
        }).ToList();
        if (resetHistory) { _undo.Clear(); _redo.Clear(); FadingMarks.Clear(); }
        Notify();
    }
}
