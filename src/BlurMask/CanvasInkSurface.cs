using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace BlurMask;

internal sealed class CanvasInkSurface : Grid
{
    private readonly DrawingSession _session;
    private readonly CanvasWindow _window;
    private readonly BoardSurface _surface;
    private readonly LiveSurface _liveSurface;
    private InkMark[] _retainedMarks = [];
    private (double Zoom, Vector Offset, Rect Board, string Background, bool Visible, double Scale)? _retainedView;
    private readonly Canvas _editorLayer = new();
    private TextBox? _editor;
    private Point _textPoint;
    private IPointer? _pointer;
    private bool _panning, _autoFit = true;
    private Point _panStart, _offsetStart, _lastErase;
    internal double Zoom { get; private set; } = 1;
    internal Vector Offset { get; private set; } = new(20, 20);
    internal Rect BoardBounds { get; private set; }
    internal event Action? ViewChanged;

    public CanvasInkSurface(DrawingSession session, CanvasWindow window)
    {
        _session = session; _window = window; ClipToBounds = true;
        ResetBounds();
        _surface = new BoardSurface(this) { CacheMode = new BitmapCache() };
        _liveSurface = new LiveSurface(this) { IsHitTestVisible = false };
        Children.Add(_surface); Children.Add(_liveSurface); Children.Add(_editorLayer);
        _surface.PointerPressed += Pressed;
        _surface.PointerMoved += Moved;
        _surface.PointerReleased += Released;
        _surface.PointerCaptureLost += (_, _) => CancelGesture();
        _surface.PointerWheelChanged += (_, e) =>
        {
            ZoomAt(Math.Pow(1.15, e.Delta.Y), e.GetPosition(this)); e.Handled = true;
        };
        AddHandler(KeyDownEvent, KeyPressed, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        SizeChanged += (_, _) => { if (_autoFit && Bounds.Width > 0 && Bounds.Height > 0) Fit(); };
        Focusable = true;
    }
    public void Fit()
    {
        _autoFit = true;
        if (Bounds.Width <= 0 || Bounds.Height <= 0) return;
        UpdateBounds();
        Zoom = Math.Clamp(Math.Min((Bounds.Width - 24) / BoardBounds.Width, (Bounds.Height - 24) / BoardBounds.Height), .002, 1);
        Offset = (Vector)new Point(Bounds.Width / 2, Bounds.Height / 2) - (Vector)BoardBounds.Center * Zoom;
        InvalidateVisual(); ViewChanged?.Invoke();
    }
    internal void ZoomAt(double factor, Point? anchor = null)
    {
        FinishText(); _autoFit = false;
        var local = anchor ?? new Point(Bounds.Width / 2, Bounds.Height / 2); var point = ToBoard(local);
        Zoom = Math.Clamp(Zoom * factor, .002, 8);
        Offset = (Vector)local - (Vector)point * Zoom;
        InvalidateVisual(); ViewChanged?.Invoke();
    }
    internal void ResetBounds() => BoardBounds = CanvasGeometry.ForMarks(_window.BoardWidth, _window.BoardHeight, _session.CanvasDocument.Marks);
    internal void UpdateBounds()
    {
        foreach (var mark in _session.CanvasDocument.Marks.Concat(_session.CanvasDocument.FadingMarks)) BoardBounds = CanvasGeometry.Grow(BoardBounds, mark.Bounds);
        foreach (var mark in _window.RemotePreviews) BoardBounds = CanvasGeometry.Grow(BoardBounds, mark.Bounds);
        if (_session.CanvasDocument.Preview is { } preview) BoardBounds = CanvasGeometry.Grow(BoardBounds, preview.Bounds);
    }
    internal Point ToBoard(Point point) => new((point.X - Offset.X) / Zoom, (point.Y - Offset.Y) / Zoom);
    internal void SetTestView(double zoom, Vector offset)
    {
        if (!Program.SmokeTest) throw new InvalidOperationException("Test camera is for isolated smoke tests only.");
        FinishInput(); _autoFit = false; Zoom = Math.Clamp(zoom, .002, 8); Offset = offset;
        UpdateBounds(); InvalidateVisual(); ViewChanged?.Invoke();
    }
    public new void InvalidateVisual() { _retainedView = null; RefreshContent(); }
    internal void RefreshContent()
    {
        var marks = _session.CanvasDocument.Marks;
        var view = (Zoom, Offset, BoardBounds, _window.BoardBackground, _session.InkVisible,
            TopLevel.GetTopLevel(this)?.RenderScaling ?? 1);
        var changed = _retainedView != view || _retainedMarks.Length != marks.Count;
        if (!changed)
            for (var i = 0; i < marks.Count; i++)
                if (!ReferenceEquals(_retainedMarks[i], marks[i])) { changed = true; break; }
        if (changed)
        {
            _retainedView = view; _retainedMarks = marks.ToArray();
            _surface.InvalidateVisual();
        }
        // Mouse moves and peer previews do not redraw the saved vector/raster
        // layer. Its cache is viewport-sized, independent of the world extent.
        _liveSurface.InvalidateVisual();
    }
    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        if (_pointer is not null) return;
        var current = e.GetCurrentPoint(this);
        if (!current.Properties.IsLeftButtonPressed && !current.Properties.IsMiddleButtonPressed) return;
        _session.ActivateCanvas(); _session.Toolbar.CloseFlyout();
        FinishText();
        var local = current.Position; var point = ToBoard(local);
        if (_session.Tool == InkTool.Cursor || current.Properties.IsMiddleButtonPressed || _window.ReadOnly)
        {
            _autoFit = false;
            _panning = true; _panStart = local; _offsetStart = (Point)Offset;
            _pointer = e.Pointer; e.Pointer.Capture(_surface); e.Handled = true; return;
        }
        point = ClampPoint(point); _autoFit = false;
        if (_session.Tool == InkTool.Text) { StartText(local, point); e.Handled = true; return; }
        _pointer = e.Pointer; e.Pointer.Capture(_surface); Focus();
        if (_session.Tool == InkTool.Select) { _session.CanvasSelection.Begin(point, 6 / Zoom); e.Handled = true; return; }
        _session.CanvasDocument.BeginEdit();
        _session.BeginAiStroke(_session.CanvasDocument);
        if (_session.Tool == InkTool.Eraser) { _lastErase = point; Erase(point); }
        else _session.CanvasDocument.Preview = new InkMark(_session.Tool == InkTool.AiPen ? InkTool.Pen : _session.Tool, _session.Color,
            CanvasGeometry.PenWidth(_session.PenSize * (_session.Tool == InkTool.Highlighter ? 4 : 1), Zoom), point)
            { FadeSeconds = _session.FadeSeconds };
        _session.CanvasDocument.Notify(); e.Handled = true;
    }
    private void Moved(object? sender, PointerEventArgs e)
    {
        if (_session.Tool == InkTool.Select && !_window.ReadOnly)
            _surface.Cursor = InkSelection.NativeCursor(_session.CanvasSelection.CursorAt(ToBoard(e.GetPosition(this)), Zoom));
        if (_pointer != e.Pointer) return;
        var local = e.GetPosition(this);
        if (_panning) { Offset = (Vector)_offsetStart + (Vector)(local - _panStart); InvalidateVisual(); return; }
        var point = ClampPoint(ToBoard(local));
        if (_session.Tool == InkTool.Select && !_window.ReadOnly) { _session.CanvasSelection.Move(point, true, e.KeyModifiers.HasFlag(KeyModifiers.Shift)); e.Handled = true; return; }
        if (_session.Tool == InkTool.Eraser)
        {
            var steps = Math.Max(1, (int)Math.Ceiling(((Vector)(point - _lastErase)).Length / Math.Max(2, _session.PenSize)));
            for (var i = 1; i <= steps; i++) Erase(_lastErase + (point - _lastErase) * ((double)i / steps));
            _lastErase = point;
        }
        else if (_session.CanvasDocument.Preview is { } mark)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift) && mark.Tool is not (InkTool.Pen or InkTool.Highlighter))
            {
                var start = mark.Points[0]; var delta = (Vector)(point - start);
                if (mark.Tool is InkTool.Rectangle or InkTool.Ellipse)
                {
                    var side = Math.Max(Math.Abs(delta.X), Math.Abs(delta.Y));
                    point = start + new Vector(Math.CopySign(side, delta.X), Math.CopySign(side, delta.Y));
                }
                else
                {
                    var angle = Math.Round(Math.Atan2(delta.Y, delta.X) / (Math.PI / 4)) * Math.PI / 4;
                    point = start + new Vector(Math.Cos(angle), Math.Sin(angle)) * delta.Length;
                }
            }
            if (mark.Points.Count < 20000) mark.AddPoint(ClampPoint(point));
            _session.CanvasDocument.Notify();
        }
        e.Handled = true;
    }
    private static Point ClampPoint(Point point) => new(Math.Clamp(point.X, -CanvasGeometry.CoordinateLimit, CanvasGeometry.CoordinateLimit),
        Math.Clamp(point.Y, -CanvasGeometry.CoordinateLimit, CanvasGeometry.CoordinateLimit));
    private void Erase(Point point)
    {
        var radius = Math.Max(8, _session.PenSize * 2) / Zoom;
        var removed = _session.CanvasDocument.Marks.RemoveAll(mark => mark.Hit(point, radius));
        removed += _session.CanvasDocument.FadingMarks.RemoveAll(mark => mark.Hit(point, radius));
        if (removed > 0)
            _session.CanvasDocument.Notify();
    }
    private void Released(object? sender, PointerReleasedEventArgs e)
    {
        if (_pointer != e.Pointer) return;
        var pointer = _pointer; _pointer = null;
        if (!_panning)
        {
            if (_session.Tool == InkTool.Select) _session.CanvasSelection.Complete();
            else if (_session.CanvasDocument.Marks.Count < 5000) _session.CompleteStroke(_session.CanvasDocument);
            else _session.CanvasDocument.Cancel();
        }
        _panning = false; pointer.Capture(null); e.Handled = true;
    }
    private void CancelGesture()
    {
        var pointer = _pointer; _pointer = null;
        if (pointer is not null)
        {
            pointer.Capture(null);
            if (!_panning) { _session.CanvasSelection.Cancel(); _session.CanvasDocument.Cancel(); _session.ResumeAiStroke(_session.CanvasDocument); }
        }
        _panning = false;
    }
    private void StartText(Point local, Point point)
    {
        _autoFit = false;
        _textPoint = ClampPoint(ToBoard(new Point(Math.Clamp(local.X, 0, Math.Max(0, Bounds.Width - 340)),
            Math.Clamp(local.Y, 0, Math.Max(0, Bounds.Height - 60)))));
        _editor = new TextBox { Width = 340, MinHeight = 48, FontSize = Math.Max(14, _session.TextSize * Zoom),
            FontFamily = InkFonts.Face("Inter").FontFamily,
            Foreground = new SolidColorBrush(_session.Color), Background = Brushes.White, AcceptsReturn = true,
            MaxLength = 4000, PlaceholderText = UiText.T("Текст · Enter — готово") };
        Canvas.SetLeft(_editor, Math.Clamp(local.X, 0, Math.Max(0, Bounds.Width - 340)));
        Canvas.SetTop(_editor, Math.Clamp(local.Y, 0, Math.Max(0, Bounds.Height - 60)));
        _editorLayer.Children.Add(_editor); _editor.Focus();
    }
    private void FinishText(bool cancel = false)
    {
        if (_editor is not { } box) return;
        _editor = null; _editorLayer.Children.Remove(box);
        if (!cancel && !_window.ReadOnly && !string.IsNullOrWhiteSpace(box.Text))
            _session.CanvasDocument.Add(new InkMark(InkTool.Text, _session.Color, 1, _textPoint)
                { Text = box.Text, Font = "Inter", FontSize = _session.TextSize, FadeSeconds = _session.FadeSeconds });
    }
    public void FinishInput() { FinishText(); CancelGesture(); }
    private void KeyPressed(object? sender, KeyEventArgs e)
    {
        if (_editor is not null)
        {
            if (e.Key == Key.Escape) { FinishText(true); e.Handled = true; }
            else if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift)) { FinishText(); e.Handled = true; }
            return;
        }
        if (_session.HandleLocalShortcut(e)) return;
        if (e.Key == Key.Escape) { CancelGesture(); _session.SelectTool(InkTool.Cursor); e.Handled = true; }
        else if (!_window.ReadOnly && e.Key is Key.Delete or Key.Back && _session.Tool == InkTool.Select)
        { _session.CanvasSelection.Delete(); e.Handled = true; }
        else if (!_window.ReadOnly && e.Key == Key.Z && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            CancelGesture();
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift)) _session.CanvasDocument.Redo();
            else _session.CanvasDocument.Undo();
            e.Handled = true;
        }
    }
    private sealed class BoardSurface(CanvasInkSurface owner) : Control
    {
        public override void Render(DrawingContext context)
        {
            context.DrawRectangle(new SolidColorBrush(Color.Parse("#E9EDF1")), null, new Rect(Bounds.Size));
            using var transform = context.PushTransform(Matrix.CreateScale(owner.Zoom, owner.Zoom) * Matrix.CreateTranslation(owner.Offset));
            var rect = owner.BoardBounds;
            context.DrawRectangle(owner._window.BoardBackground == "black" ? Brushes.Black : Brushes.White,
                new Pen(new SolidColorBrush(Color.Parse("#D8E0E5")), 1 / owner.Zoom), rect);
            using var clip = context.PushClip(rect);
            if (!owner._session.InkVisible) return;
            var visible = new Rect(owner.ToBoard(default), owner.ToBoard(new Point(Bounds.Width, Bounds.Height))).Normalize();
            var scale = owner.Zoom * (TopLevel.GetTopLevel(owner)?.RenderScaling ?? 1);
            foreach (var mark in owner._session.CanvasDocument.Marks) if (mark.Bounds.Intersects(visible)) mark.Draw(context, scale);
        }
    }
    private sealed class LiveSurface(CanvasInkSurface owner) : Control
    {
        public override void Render(DrawingContext context)
        {
            if (!owner._session.InkVisible) return;
            using var transform = context.PushTransform(Matrix.CreateScale(owner.Zoom, owner.Zoom) * Matrix.CreateTranslation(owner.Offset));
            using var clip = context.PushClip(owner.BoardBounds);
            var visible = new Rect(owner.ToBoard(default), owner.ToBoard(new Point(Bounds.Width, Bounds.Height))).Normalize();
            var scale = owner.Zoom * (TopLevel.GetTopLevel(owner)?.RenderScaling ?? 1);
            foreach (var mark in owner._session.CanvasDocument.FadingMarks) if (mark.Bounds.Intersects(visible)) mark.Draw(context, scale);
            foreach (var mark in owner._window.RemotePreviews) if (mark.Bounds.Intersects(visible)) mark.Draw(context, scale);
            owner._session.CanvasDocument.Preview?.Draw(context, scale);
            if (owner._session.Tool == InkTool.Select && !owner._window.ReadOnly) owner._session.CanvasSelection.Draw(context, owner.Zoom);
        }
    }
}
