using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;

namespace BlurMask;

internal sealed class DesktopInkWindow : Window
{
    private readonly DrawingSession _session;
    private readonly InkSurface _surface;
    private readonly Canvas _editLayer = new();
    private TextBox? _textBox;
    private Point _textOrigin;
    private IPointer? _pointer;
    private bool _middleErasing;
    private Point _lastErase;
    private readonly PixelRect _screenBounds;
    private PixelRect[]? _controlRegion;
    private readonly HyprlandInkOverlay? _hyprlandEffects;

    public DesktopInkWindow(DrawingSession session, Screen screen)
    {
        _session = session;
        _screenBounds = screen.Bounds;
        Title = UiText.T("BlurMask — слой рисования");
        _hyprlandEffects = HyprlandInkOverlay.TryCreate(Title);
        WindowDecorations = WindowDecorations.None;
        ShowInTaskbar = false;
        ShowActivated = false;
        Topmost = true;
        CanResize = false;
        CanMinimize = false;
        CanMaximize = false;
        Background = Brushes.Transparent;
        TransparencyBackgroundFallback = Brushes.Transparent;
        TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
        Position = screen.Bounds.Position;
        Width = screen.Bounds.Width / screen.Scaling;
        Height = screen.Bounds.Height / screen.Scaling;
        _surface = new InkSurface(session, this) { Focusable = true };
        var grid = new Grid();
        grid.Children.Add(_surface);
        grid.Children.Add(_editLayer);
        Content = grid;
        _surface.PointerPressed += OnPressed;
        _surface.PointerMoved += OnMoved;
        _surface.PointerExited += (_, _) => _session.ClearWindowFillPreview();
        _surface.PointerReleased += OnReleased;
        _surface.PointerCaptureLost += (_, _) => CancelGesture();
        AddHandler(KeyDownEvent, OnKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        Opened += (_, _) =>
        {
            Position = _screenBounds.Position;
            ApplyState();
        };
        Closing += (_, e) =>
        {
            if (e.IsProgrammatic || e.CloseReason != WindowCloseReason.WindowClosing) return;
            // A compositor's close shortcut should leave this reusable layer
            // available for the next stroke instead of destroying its handle.
            e.Cancel = true;
            _session.SelectTool(InkTool.Cursor, remember: false);
        };
        Closed += (_, _) => { CancelGesture(); _hyprlandEffects?.Dispose(); };
        PositionChanged += (_, _) => UpdateControlExclusion();
        SizeChanged += (_, _) => UpdateControlExclusion();
    }

    public void ApplyState()
    {
        var drawing = _session.PickingScreenshot || _session.Target == DrawingTarget.Screen && (_session.PickingMaskRegion || _session.Tool != InkTool.Cursor) && _session.InkVisible;
        _surface.Cursor = new Cursor(_session.Tool == InkTool.Select ? StandardCursorType.SizeAll : drawing ? StandardCursorType.Cross : StandardCursorType.Arrow);
        _surface.InvalidateVisual();
        if (OperatingSystem.IsWindows())
        {
            if (_session.Target == DrawingTarget.Canvas)
            {
                DesktopInkNative.SetClickThrough(this, true);
                if (IsVisible) Hide();
                return;
            }
            if (!IsVisible) Show();
            DesktopInkNative.SetClickThrough(this, !drawing);
        }
        else
        {
            // Never leave an invisible input-blocking window on unsupported backends.
            if (drawing && !IsVisible) { Show(); _hyprlandEffects?.Apply(); }
            else if (!drawing && IsVisible) { _hyprlandEffects?.Suspend(); Hide(); }
            UpdateControlExclusion();
        }
    }

    internal void UpdateControlExclusion()
    {
        if (!OperatingSystem.IsLinux() || !IsVisible || Bounds.Width <= 0 || Bounds.Height <= 0) return;
        var bounds = new PixelRect(Position, new PixelSize((int)Math.Ceiling(Bounds.Width * RenderScaling),
            (int)Math.Ceiling(Bounds.Height * RenderScaling)));
        var regions = new List<PixelRect> { bounds };
        foreach (var control in new Window?[] { _session.Toolbar, _session.Toolbar.OpenFlyout, _session.Toolbar.UpdateNoticeWindow })
        {
            if (control is not { IsVisible: true } || control.Bounds.Width <= 0 || control.Bounds.Height <= 0) continue;
            var hole = new PixelRect(control.Position, new PixelSize((int)Math.Ceiling(control.Bounds.Width * control.RenderScaling),
                (int)Math.Ceiling(control.Bounds.Height * control.RenderScaling)));
            var remaining = new List<PixelRect>();
            foreach (var region in regions)
            {
                var overlap = region.Intersect(hole);
                if (overlap.Width <= 0 || overlap.Height <= 0) { remaining.Add(region); continue; }
                Add(new(region.X, region.Y, region.Width, overlap.Y - region.Y));
                Add(new(region.X, overlap.Bottom, region.Width, region.Bottom - overlap.Bottom));
                Add(new(region.X, overlap.Y, overlap.X - region.X, overlap.Height));
                Add(new(overlap.Right, overlap.Y, region.Right - overlap.Right, overlap.Height));
                void Add(PixelRect rectangle) { if (rectangle.Width > 0 && rectangle.Height > 0) remaining.Add(rectangle); }
            }
            regions = remaining;
        }
        var next = regions.ToArray();
        if (_controlRegion?.SequenceEqual(next) == true) return;
        if (DesktopInkNative.ExcludeControls(this, bounds, next)) _controlRegion = next;
    }

    public void Refresh() => _surface.InvalidateVisual();
    internal Point DesktopPoint(Point local) => new(Position.X + local.X * RenderScaling, Position.Y + local.Y * RenderScaling);

    private void OnPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(_surface);
        if (point.Properties.IsRightButtonPressed)
        {
            _session.SelectTool(InkTool.Cursor, remember: false);
            e.Handled = true;
            return;
        }
        if (point.Properties.IsLeftButtonPressed && _session.PickingMaskRegion)
        {
            e.Handled = true;
            _session.CompleteWindowFill(new PixelPoint((int)Math.Round(DesktopPoint(point.Position).X), (int)Math.Round(DesktopPoint(point.Position).Y)));
            return;
        }
        if (point.Properties.IsLeftButtonPressed && _session.PickingScreenshot)
        {
            _pointer = e.Pointer; e.Pointer.Capture(_surface);
            var desktop = DesktopPoint(point.Position);
            _session.BeginScreenshot(new PixelPoint((int)Math.Round(desktop.X), (int)Math.Round(desktop.Y)));
            _session.MoveScreenshot(new PixelPoint((int)Math.Round(desktop.X), (int)Math.Round(desktop.Y)));
            e.Handled = true; return;
        }
        var middleErasing = point.Properties.IsMiddleButtonPressed && _session.MiddleButtonErases &&
            !_session.PickingScreenshot && !_session.PickingMaskRegion;
        if ((!point.Properties.IsLeftButtonPressed && !middleErasing) || _session.Tool == InkTool.Cursor || _session.Target != DrawingTarget.Screen) return;
        _session.Toolbar.CloseFlyout();
        e.Handled = true;
        FinishText();
        var origin = DesktopPoint(point.Position);
        if (_session.Tool == InkTool.Text)
        {
            BeginText(point.Position, origin);
            return;
        }
        if (_pointer is not null) return;
        _pointer = e.Pointer;
        _middleErasing = middleErasing;
        e.Pointer.Capture(_surface);
        if (_session.Tool == InkTool.Select)
        {
            Activate(); _surface.Focus();
            _session.DesktopSelection.Begin(origin, 6 * RenderScaling, e.KeyModifiers.HasFlag(KeyModifiers.Shift)); return;
        }
        _session.DesktopDocument.BeginEdit();
        _session.BeginAiStroke(_session.DesktopDocument);
        if (_session.Tool == InkTool.Eraser || _middleErasing)
        {
            _lastErase = origin;
            Erase(origin);
        }
        else
        {
            var width = _session.PenSize * RenderScaling * (_session.Tool == InkTool.Highlighter ? 4 : 1);
            _session.DesktopDocument.Preview = new InkMark(_session.Tool == InkTool.AiPen ? InkTool.Pen : _session.Tool, _session.Color, width, origin)
                { FadeSeconds = _session.FadeSeconds };
            _session.DesktopDocument.Notify();
        }
    }

    private void OnMoved(object? sender, PointerEventArgs e)
    {
        if (_session.PickingScreenshot)
        {
            if (_pointer == e.Pointer)
            {
                var desktop = DesktopPoint(e.GetPosition(_surface));
                _session.MoveScreenshot(new PixelPoint((int)Math.Round(desktop.X), (int)Math.Round(desktop.Y)));
            }
            e.Handled = true; return;
        }
        if (_session.PickingMaskRegion)
        {
            var desktop = DesktopPoint(e.GetPosition(_surface));
            _session.UpdateWindowFillPreview(new PixelPoint((int)Math.Round(desktop.X), (int)Math.Round(desktop.Y)));
            e.Handled = true;
            return;
        }
        if (_session.Tool == InkTool.Eraser && e.GetCurrentPoint(_surface).Properties.IsLeftButtonPressed)
            _session.EraseMasks(DesktopPoint(e.GetPosition(_surface)), Math.Max(8, _session.PenSize * 2) * RenderScaling);
        if (_session.Tool == InkTool.Select)
            _surface.Cursor = InkSelection.NativeCursor(_session.DesktopSelection.CursorAt(DesktopPoint(e.GetPosition(_surface)), 1 / RenderScaling));
        if (_pointer != e.Pointer) return;
        if (_session.PickingScreenshot)
        {
            var desktop = DesktopPoint(e.GetPosition(_surface));
            _session.MoveScreenshot(new PixelPoint((int)Math.Round(desktop.X), (int)Math.Round(desktop.Y)));
            _pointer = null; e.Pointer.Capture(null);
            _session.CompleteScreenshot(); e.Handled = true; return;
        }
        var point = DesktopPoint(e.GetPosition(_surface));
        if (_session.Tool == InkTool.Select) { _session.DesktopSelection.Move(point, constrain: e.KeyModifiers.HasFlag(KeyModifiers.Shift)); e.Handled = true; return; }
        if (_session.Tool == InkTool.Eraser || _middleErasing)
        {
            var distance = ((Vector)(point - _lastErase)).Length;
            var steps = Math.Max(1, (int)Math.Ceiling(distance / Math.Max(2, _session.PenSize * RenderScaling)));
            for (var i = 1; i <= steps; i++) Erase(_lastErase + (point - _lastErase) * ((double)i / steps));
            _lastErase = point;
        }
        else if (_session.DesktopDocument.Preview is { } mark)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift) && mark.Tool is not (InkTool.Pen or InkTool.Highlighter))
            {
                var start = mark.Points[0];
                var delta = (Vector)(point - start);
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
            mark.AddPoint(point);
            _session.DesktopDocument.Notify();
        }
        e.Handled = true;
    }

    private void Erase(Point point)
    {
        var radius = Math.Max(8, _session.PenSize * 2) * RenderScaling;
        _session.EraseMasks(point, radius, _middleErasing);
        var removed = _session.DesktopDocument.Marks.RemoveAll(mark => mark.Hit(point, radius));
        removed += _session.DesktopDocument.FadingMarks.RemoveAll(mark => mark.Hit(point, radius));
        if (removed > 0)
            _session.DesktopDocument.Notify();
    }

    private void OnReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_pointer != e.Pointer) return;
        var properties = e.GetCurrentPoint(_surface).Properties;
        if (_middleErasing ? properties.IsMiddleButtonPressed : properties.IsLeftButtonPressed) return;
        var middleErasing = _middleErasing;
        if (_session.Tool == InkTool.Select) _session.DesktopSelection.Complete();
        else _session.CompleteStroke(_session.DesktopDocument);
        _pointer = null;
        _middleErasing = false;
        e.Pointer.Capture(null);
        if (middleErasing) _session.ResumeAiStroke(_session.DesktopDocument);
        e.Handled = true;
    }

    public void CancelGesture()
    {
        var pointer = _pointer;
        _pointer = null;
        _middleErasing = false;
        if (pointer is not null)
        {
            if (_session.PickingScreenshot) _session.CancelScreenshot();
            pointer.Capture(null);
            _session.DesktopSelection.Cancel();
            _session.DesktopDocument.Cancel();
            _session.ResumeAiStroke(_session.DesktopDocument);
        }
    }

    private void BeginText(Point local, Point origin)
    {
        _textOrigin = origin;
        _textBox = new TextBox
        {
            Width = Math.Min(380, Math.Max(120, Bounds.Width - local.X)),
            MinHeight = 48,
            FontSize = _session.TextSize,
            FontFamily = InkFonts.Face("Inter").FontFamily,
            Foreground = new SolidColorBrush(_session.Color),
            Background = new SolidColorBrush(Color.FromArgb(235, 255, 255, 255)),
            PlaceholderText = UiText.T("Текст · Enter — готово"),
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap
        };
        var left = Math.Clamp(local.X, 0, Math.Max(0, Bounds.Width - _textBox.Width));
        var top = Math.Clamp(local.Y, 0, Math.Max(0, Bounds.Height - 70));
        _textOrigin = DesktopPoint(new Point(left, top));
        Canvas.SetLeft(_textBox, left);
        Canvas.SetTop(_textBox, top);
        _editLayer.Children.Add(_textBox);
        Activate();
        _textBox.Focus();
    }

    public void FinishText(bool cancel = false)
    {
        if (_textBox is not { } box) return;
        _textBox = null;
        _editLayer.Children.Remove(box);
        if (!cancel && !string.IsNullOrWhiteSpace(box.Text))
            _session.DesktopDocument.Add(new InkMark(InkTool.Text, _session.Color, 1, _textOrigin)
            { Text = box.Text, Font = "Inter", FontSize = _session.TextSize * RenderScaling, FadeSeconds = _session.FadeSeconds });
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (_textBox is not null)
        {
            if (e.Key == Key.Escape) { FinishText(true); e.Handled = true; }
            else if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift)) { FinishText(); e.Handled = true; }
            return;
        }
        if (_session.HandleSelectionKey(e) || _session.HandleLocalShortcut(e)) return;
        if (e.Key == Key.Escape) { _session.SelectTool(InkTool.Cursor, remember: false); e.Handled = true; }
        else if (e.Key == Key.Z && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            CancelGesture();
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift)) _session.DesktopDocument.Redo();
            else _session.DesktopDocument.Undo();
            e.Handled = true;
        }
    }

    private sealed class InkSurface(DrawingSession session, DesktopInkWindow window) : Control
    {
        public override void Render(DrawingContext context)
        {
            var active = session.PickingScreenshot || session.Target == DrawingTarget.Screen && (session.PickingMaskRegion || session.Tool != InkTool.Cursor) && session.InkVisible;
            // A non-zero alpha over the entire screen makes empty desktop areas accept ink.
            context.DrawRectangle(active ? new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)) : Brushes.Transparent,
                null, new Rect(Bounds.Size));
            if (!session.InkVisible && !session.PickingScreenshot) return;
            if (session.InkVisible && session.Board != BoardMode.Desktop)
                context.DrawRectangle(session.Board == BoardMode.White ? Brushes.White : Brushes.Black, null, new Rect(Bounds.Size));
            using var transform = context.PushTransform(
                Matrix.CreateTranslation(-window.Position.X, -window.Position.Y) * Matrix.CreateScale(1 / window.RenderScaling, 1 / window.RenderScaling));
            var visible = new Rect(window.Position.X, window.Position.Y, Bounds.Width * window.RenderScaling, Bounds.Height * window.RenderScaling);
            if (session.InkVisible)
            {
                foreach (var mark in session.DesktopDocument.Marks) if (mark.Bounds.Intersects(visible)) mark.Draw(context);
                foreach (var mark in session.DesktopDocument.FadingMarks) if (mark.Bounds.Intersects(visible)) mark.Draw(context);
            }
            session.DesktopDocument.Preview?.Draw(context);
            if (session.PickingMaskRegion && session.WindowFillPreview is { } region)
                context.DrawRectangle(Brushes.Black, null, new Rect(region.Bounds.X, region.Bounds.Y, region.Bounds.Width, region.Bounds.Height));
            if (session.Tool == InkTool.Select) session.DesktopSelection.Draw(context, 1 / window.RenderScaling);
            if (session.PickingScreenshot)
            {
                Rect? selection = session.ScreenshotPreview is { } capture ? new Rect(capture.X, capture.Y, capture.Width, capture.Height) : null;
                var shade = new SolidColorBrush(Color.FromArgb(75, 0, 0, 0));
                foreach (var rectangle in ScreenRegionSelection.Outside(visible, selection))
                    if (rectangle.Width > 0 && rectangle.Height > 0) context.DrawRectangle(shade, null, rectangle);
                if (selection is { } selected)
                {
                    // Coordinates inside this transform are physical screen pixels.
                    var frame = new Rect(selected.X + .5, selected.Y + .5, Math.Max(0, selected.Width - 1), Math.Max(0, selected.Height - 1));
                    context.DrawRectangle(null, new Pen(Brushes.Black, 1, new DashStyle([4, 3], 0)), frame);
                    var scaling = window.RenderScaling;
                    var label = new FormattedText($"{(int)selected.Width} × {(int)selected.Height} px",
                        System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                        new Typeface("Inter"), 12 * scaling, Brushes.Black);
                    var labelSize = new Size(label.Width + 12 * scaling, label.Height + 6 * scaling);
                    var left = Math.Clamp(selected.Left, visible.Left, Math.Max(visible.Left, visible.Right - labelSize.Width));
                    var top = selected.Top - labelSize.Height - 5 * scaling;
                    if (top < visible.Top) top = selected.Top + 5 * scaling;
                    top = Math.Clamp(top, visible.Top, Math.Max(visible.Top, visible.Bottom - labelSize.Height));
                    context.DrawRectangle(Brushes.White, new Pen(Brushes.Black, 1), new Rect(new Point(left, top), labelSize), 3 * scaling, 3 * scaling);
                    context.DrawText(label, new Point(left + 6 * scaling, top + 3 * scaling));
                }
            }
        }
    }
}
