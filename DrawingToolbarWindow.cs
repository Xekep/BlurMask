using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace BlurMask;

internal sealed class DrawingToolbarWindow : Window
{
    private static readonly IBrush Cyan = new SolidColorBrush(Color.Parse("#27C4E5"));
    private static readonly IBrush Edge = new SolidColorBrush(Color.Parse("#A8A8A8"));
    private static readonly ControlTheme FlatButton = CreateButtonTheme();
    private readonly DrawingSession _session;
    private readonly StackPanel _body = new() { Spacing = 0, Width = 32 };
    private readonly Button _cursor;
    private readonly Button _drawing;
    private readonly Button _eraser;
    private readonly Button _undo;
    private readonly Button _selectedColor;
    private readonly Dictionary<InkTool, Button> _toolButtons = [];
    private Window? _flyout;
    private string? _flyoutName;
    private bool _collapsed;
    private bool _placed;
    private PixelPoint _dragStart;
    private PixelPoint _windowStart;
    private IPointer? _dragPointer;
    private readonly Button _handle;

    public DrawingToolbarWindow(DrawingSession session)
    {
        _session = session;
        Title = "BlurMask — инструменты рисования";
        Width = 42;
        SizeToContent = SizeToContent.Height;
        WindowDecorations = WindowDecorations.None;
        Background = Brushes.Transparent;
        TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
        TransparencyBackgroundFallback = Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        ShowActivated = false;
        CanResize = false;
        CanMinimize = false;
        CanMaximize = false;
        RequestedThemeVariant = ThemeVariant.Light;

        var column = new StackPanel { Spacing = 3, Margin = new Thickness(3) };
        _handle = IconButton(ToolbarGlyph.Nib, "Перетащить панель · двойной щелчок — свернуть", ToggleCollapsed, 36, 36);
        _handle.Cursor = new Cursor(StandardCursorType.SizeAll);
        // Use explicit pointer capture so moving the toolbar does not activate the
        // program behind it and works across monitors with different scale factors.
        _handle.AddHandler(PointerPressedEvent, HandlePressed, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        _handle.AddHandler(PointerMovedEvent, HandleMoved, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        _handle.AddHandler(PointerReleasedEvent, HandleReleased, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        _handle.PointerCaptureLost += (_, _) => _dragPointer = null;
        column.Children.Add(_handle);

        _body.Children.Add(IconButton(ToolbarGlyph.Eye, "Свернуть панель / скрыть рисунок", ToggleCollapsed));
        _cursor = AddTool(ToolbarGlyph.Cursor, "Курсор · Ctrl+Shift+1", () => session.SelectTool(InkTool.Cursor));
        _drawing = AddTool(ToolbarGlyph.Pen, "Инструменты рисования · Ctrl+Shift+2", () =>
        {
            if (session.Tool is InkTool.Cursor or InkTool.Eraser) session.SelectTool(InkTool.Pen);
            ShowTools();
        });
        _eraser = AddTool(ToolbarGlyph.Eraser, "Ластик · Ctrl+Shift+4", () => session.SelectTool(InkTool.Eraser));
        _body.Children.Add(IconButton(ToolbarGlyph.Size, "Толщина пера", ShowSizes));
        _undo = AddTool(ToolbarGlyph.Undo, "Отменить · Ctrl+Shift+Z · ПКМ — повторить", session.Undo);
        _undo.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(_undo).Properties.IsRightButtonPressed) { session.Redo(); e.Handled = true; }
        };
        _body.Children.Add(IconButton(ToolbarGlyph.Trash, "Очистить экран · Ctrl+Shift+F", session.Clear));
        _body.Children.Add(IconButton(ToolbarGlyph.Board, "Рабочий стол / белая / чёрная доска", ShowBoards));
        _body.Children.Add(IconButton(ToolbarGlyph.Camera, "Снимок экрана · Ctrl+Shift+S", () => _ = session.CaptureScreenshotAsync()));
        _body.Children.Add(IconButton(ToolbarGlyph.Menu, "Меню BlurMask", ShowMenu));

        var quick = new UniformGrid { Columns = 2, Rows = 2, Height = 30 };
        foreach (var color in new[] { "#00C9ED", "#FFF400", "#FF0099", "#111111" })
        {
            var parsed = Color.Parse(color);
            quick.Children.Add(ColorButton(parsed, 16, 15));
        }
        _body.Children.Add(quick);
        _selectedColor = new Button
        {
            Theme = FlatButton, Width = 32, Height = 30, Padding = new Thickness(0),
            Content = new TextBlock { Text = "◢", FontSize = 11, Foreground = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom }
        };
        _selectedColor.Click += (_, _) => ShowPalette();
        ToolTip.SetTip(_selectedColor, "Палитра цветов");
        AutomationProperties.SetName(_selectedColor, "Палитра цветов");
        _body.Children.Add(_selectedColor);
        column.Children.Add(new Border
        {
            BorderBrush = Edge, BorderThickness = new Thickness(1), Background = Brushes.White,
            BoxShadow = new BoxShadows(new BoxShadow { Blur = 3, OffsetX = 1, OffsetY = 1, Color = Color.FromArgb(75, 0, 0, 0) }),
            Child = _body, HorizontalAlignment = HorizontalAlignment.Center
        });
        Content = column;
        session.StateChanged += Refresh;
        Opened += (_, _) =>
        {
            if (!_placed && Screens.Primary is { } screen)
            {
                _placed = true;
                Position = new PixelPoint(screen.WorkingArea.X + 24,
                    screen.WorkingArea.Y + Math.Max(16, (screen.WorkingArea.Height - (int)(Height * screen.Scaling)) / 2));
            }
        };
        PositionChanged += (_, _) => PlaceFlyout();
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape) { CloseFlyout(); session.SelectTool(InkTool.Cursor); e.Handled = true; }
        };
        Closed += (_, _) => session.StateChanged -= Refresh;
        Refresh();
    }

    private static ControlTheme CreateButtonTheme()
    {
        var theme = new ControlTheme(typeof(Button));
        theme.Setters.Add(new Setter(BackgroundProperty, Brushes.Transparent));
        theme.Setters.Add(new Setter(TemplateProperty, new FuncControlTemplate<Button>((button, _) =>
            new Border
            {
                [!Border.BackgroundProperty] = button[!BackgroundProperty],
                [!Border.BorderBrushProperty] = button[!BorderBrushProperty],
                [!Border.BorderThicknessProperty] = button[!BorderThicknessProperty],
                Child = new ContentPresenter
                {
                    [!ContentPresenter.ContentProperty] = button[!ContentProperty],
                    [!ContentPresenter.HorizontalContentAlignmentProperty] = button[!HorizontalContentAlignmentProperty],
                    [!ContentPresenter.VerticalContentAlignmentProperty] = button[!VerticalContentAlignmentProperty]
                }
            })));
        theme.Children.Add(new Style(x => x.Nesting().Class(":pointerover"))
        { Setters = { new Setter(BackgroundProperty, new SolidColorBrush(Color.Parse("#D9F3F8"))) } });
        theme.Children.Add(new Style(x => x.Nesting().Class(":pressed"))
        { Setters = { new Setter(BackgroundProperty, Cyan) } });
        return theme;
    }

    private static Button IconButton(ToolbarGlyph glyph, string tip, Action click, double width = 32, double height = 30)
    {
        var button = new Button
        {
            Theme = FlatButton, Width = width, Height = height, Padding = new Thickness(0),
            Content = new ToolbarIcon(glyph) { Width = glyph == ToolbarGlyph.Nib ? 34 : 23, Height = glyph == ToolbarGlyph.Nib ? 34 : 23 }
        };
        button.Click += (_, _) => click();
        ToolTip.SetTip(button, tip);
        AutomationProperties.SetName(button, tip);
        return button;
    }

    private Button AddTool(ToolbarGlyph glyph, string tip, Action click)
    {
        var button = IconButton(glyph, tip, () => { CloseFlyout(); click(); });
        _body.Children.Add(button);
        return button;
    }
    private Button ColorButton(Color color, double width, double height)
    {
        var button = new Button { Theme = FlatButton, Width = width, Height = height, Background = new SolidColorBrush(color) };
        ToolTip.SetTip(button, color.ToString());
        AutomationProperties.SetName(button, $"Цвет {color}");
        button.Click += (_, _) => { _session.Color = color; _session.NotifySettings(); CloseFlyout(); };
        return button;
    }

    private void Refresh()
    {
        _cursor.Background = _session.Tool == InkTool.Cursor ? Cyan : Brushes.Transparent;
        _eraser.Background = _session.Tool == InkTool.Eraser ? Cyan : Brushes.Transparent;
        _drawing.Background = _session.Tool is not (InkTool.Cursor or InkTool.Eraser) ? Cyan : Brushes.Transparent;
        _drawing.Content = new ToolbarIcon(GlyphFor(_session.Tool)) { Width = 23, Height = 23 };
        _selectedColor.Background = new SolidColorBrush(_session.Color);
        _undo.Opacity = _session.Document.CanUndo ? 1 : 0.4;
        foreach (var (tool, button) in _toolButtons)
            button.Background = tool == _session.Tool ? Cyan : Brushes.Transparent;
    }
    private static ToolbarGlyph GlyphFor(InkTool tool) => tool switch
    {
        InkTool.Highlighter => ToolbarGlyph.Highlighter, InkTool.Text => ToolbarGlyph.Text,
        InkTool.Line => ToolbarGlyph.Line, InkTool.Arrow => ToolbarGlyph.Arrow,
        InkTool.Rectangle => ToolbarGlyph.Rectangle, InkTool.Ellipse => ToolbarGlyph.Ellipse,
        _ => ToolbarGlyph.Pen
    };

    public void ShowTools()
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 1 };
        var buttons = new Dictionary<InkTool, Button>();
        foreach (var (tool, name) in new[]
        {
            (InkTool.Pen, "Перо"), (InkTool.Highlighter, "Маркер"), (InkTool.Text, "Текст"),
            (InkTool.Line, "Линия"), (InkTool.Arrow, "Стрелка"),
            (InkTool.Rectangle, "Прямоугольник"), (InkTool.Ellipse, "Эллипс")
        })
        {
            var button = IconButton(GlyphFor(tool), name, () => { _session.SelectTool(tool); CloseFlyout(); });
            buttons[tool] = button;
            row.Children.Add(button);
        }
        ShowFlyout("tools", row, 98);
        if (_flyoutName == "tools")
            foreach (var (tool, button) in buttons) _toolButtons[tool] = button;
        Refresh();
    }
    private void ShowSizes()
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        foreach (var size in new[] { 2d, 4d, 8d, 12d, 20d })
        {
            var button = new Button
            {
                Theme = FlatButton, Width = 32, Height = 32,
                Background = _session.PenSize == size ? Cyan : Brushes.Transparent,
                Content = new Avalonia.Controls.Shapes.Ellipse { Width = size, Height = size, Fill = Brushes.Black }
            };
            ToolTip.SetTip(button, $"{size} px");
            button.Click += (_, _) => { _session.PenSize = size; _session.NotifySettings(); CloseFlyout(); };
            row.Children.Add(button);
        }
        ShowFlyout("sizes", row, 158);
    }
    private void ShowBoards()
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        foreach (var (board, name, color) in new[]
        {
            (BoardMode.Desktop, "Рабочий стол", "#D1E8EF"),
            (BoardMode.White, "Белая доска", "#FFFFFF"), (BoardMode.Black, "Чёрная доска", "#111111")
        })
        {
            var button = IconButton(ToolbarGlyph.Board, name, () => { _session.SetBoard(board); CloseFlyout(); });
            button.Background = new SolidColorBrush(Color.Parse(color));
            button.BorderBrush = board == _session.Board ? Cyan : Edge;
            button.BorderThickness = new Thickness(2);
            row.Children.Add(button);
        }
        ShowFlyout("boards", row, 248);
    }
    public void ShowPalette()
    {
        var panel = new StackPanel { Spacing = 7, Width = 184, Margin = new Thickness(5) };
        var colors = new UniformGrid { Columns = 6, Rows = 4 };
        foreach (var color in new[]
        {
            "#FFFFFF", "#FFF400", "#FF9900", "#FF3333", "#FF0099", "#9933FF",
            "#00C9ED", "#0088FF", "#2244CC", "#00CC66", "#66FF00", "#111111",
            "#CCCCCC", "#FFF6A0", "#FFCC99", "#FF99AA", "#FF99DD", "#CCAAFF",
            "#99EEFF", "#88BBFF", "#8888DD", "#88DDAA", "#AACC66", "#666666"
        }) colors.Children.Add(ColorButton(Color.Parse(color), 29, 27));
        panel.Children.Add(colors);
        var custom = new TextBox { Text = _session.Color.ToString(), PlaceholderText = "#RRGGBB", FontSize = 13 };
        panel.Children.Add(custom);
        var apply = new Button { Content = "Применить цвет", HorizontalAlignment = HorizontalAlignment.Stretch };
        apply.Click += (_, _) =>
        {
            if (Color.TryParse(custom.Text, out var color))
            { _session.Color = color; _session.NotifySettings(); CloseFlyout(); }
            else custom.PlaceholderText = "Введите #RRGGBB";
        };
        panel.Children.Add(apply);
        ShowFlyout("palette", panel, 350);
    }

    internal void ShowMenu()
    {
        var panel = new StackPanel { Width = 230, Spacing = 3, Margin = new Thickness(7) };
        panel.Children.Add(new TextBlock { Text = "BlurMask", FontSize = 18, FontWeight = FontWeight.SemiBold, Margin = new Thickness(5, 0, 0, 7) });
        MenuItem(panel, "Новая маска", _session.NewMask);
        MenuItem(panel, "Отменить                         Ctrl+Shift+Z", _session.Undo);
        MenuItem(panel, "Повторить                       Ctrl+Shift+Y", _session.Redo);
        MenuItem(panel, _session.InkVisible ? "Скрыть рисунок                  Ctrl+Shift+H" : "Показать рисунок                Ctrl+Shift+H", _session.ToggleInk);
        MenuItem(panel, "Очистить экран                  Ctrl+Shift+F", _session.Clear);
        panel.Children.Add(new Border { Height = 1, Background = Edge, Margin = new Thickness(0, 5) });
        panel.Children.Add(new TextBlock { Text = "Размер текста", FontSize = 12, Margin = new Thickness(5, 3) });
        var textSize = new Slider { Minimum = 14, Maximum = 72, Value = _session.TextSize, TickFrequency = 2, IsSnapToTickEnabled = true };
        textSize.ValueChanged += (_, _) => _session.TextSize = textSize.Value;
        panel.Children.Add(textSize);
        MenuItem(panel, "Горячие клавиши", ShowShortcuts);
        MenuItem(panel, "Снимок экрана                   Ctrl+Shift+S", () => _ = _session.CaptureScreenshotAsync());
        MenuItem(panel, "Свернуть панель", ToggleCollapsed);
        MenuItem(panel, "Выход", _session.Exit);
        ShowFlyout("menu", panel, 280);
    }
    private void MenuItem(StackPanel panel, string text, Action click)
    {
        var button = new Button { Theme = FlatButton, Content = new TextBlock { Text = text, Margin = new Thickness(7, 6) }, FontSize = 12, MinHeight = 30, HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left, Padding = new Thickness(7, 6) };
        button.Click += (_, _) => { CloseFlyout(); click(); };
        panel.Children.Add(button);
    }
    private void ShowShortcuts() => ShowMessage("Горячие клавиши",
        "Ctrl+Shift+1   Курсор\nCtrl+Shift+2   Перо\nCtrl+Shift+3   Маркер\nCtrl+Shift+4   Ластик\nCtrl+Shift+Z   Отменить\nCtrl+Shift+Y   Повторить\nCtrl+Shift+F   Очистить\nCtrl+Shift+H   Скрыть / показать рисунок\nCtrl+Shift+S   Снимок экрана\n\nEsc или ПКМ на экране — курсор\nShift — квадрат, круг, угол 45°\nТекст: Enter — готово; Shift+Enter — новая строка");

    public void ShowMessage(string title, string message)
    {
        var panel = new StackPanel { Width = 290, Margin = new Thickness(12), Spacing = 10 };
        panel.Children.Add(new TextBlock { Text = title, FontWeight = FontWeight.SemiBold, FontSize = 15 });
        panel.Children.Add(new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap, FontSize = 12 });
        var close = new Button { Content = "Закрыть", HorizontalAlignment = HorizontalAlignment.Right };
        close.Click += (_, _) => CloseFlyout();
        panel.Children.Add(close);
        ShowFlyout("message", panel, 220);
    }

    private double _flyoutOffset;
    private void ShowFlyout(string name, Control content, double offset)
    {
        if (_flyoutName == name) { CloseFlyout(); return; }
        CloseFlyout();
        _flyoutName = name;
        _flyoutOffset = offset;
        _flyout = new Window
        {
            Title = "BlurMask — меню", WindowDecorations = WindowDecorations.None,
            SizeToContent = SizeToContent.WidthAndHeight, ShowInTaskbar = false, Topmost = true,
            CanResize = false, CanMinimize = false, CanMaximize = false,
            RequestedThemeVariant = ThemeVariant.Light,
            Background = Brushes.White,
            Content = new Border { BorderBrush = Edge, BorderThickness = new Thickness(1), Padding = new Thickness(3), Child = content }
        };
        var openedFlyout = _flyout;
        openedFlyout.Closed += (_, _) =>
        {
            if (!ReferenceEquals(_flyout, openedFlyout)) return;
            _flyout = null;
            _flyoutName = null;
            _toolButtons.Clear();
        };
        _flyout.KeyDown += (_, e) => { if (e.Key == Key.Escape) CloseFlyout(); };
        _flyout.Opened += (_, _) => PlaceFlyout();
        _flyout.Resized += (_, _) => PlaceFlyout();
        _flyout.Show(this);
        PlaceFlyout();
    }
    private void PlaceFlyout()
    {
        if (_flyout is not { IsVisible: true } flyout) return;
        var screen = Screens.ScreenFromWindow(this) ?? Screens.Primary;
        if (screen is null) return;
        var area = screen.WorkingArea;
        var scale = RenderScaling;
        var width = (int)Math.Ceiling(flyout.Bounds.Width * scale);
        var height = (int)Math.Ceiling(flyout.Bounds.Height * scale);
        var x = Position.X + (int)(Width * scale);
        if (x + width > area.Right) x = Position.X - width;
        var y = Position.Y + (int)(_flyoutOffset * scale);
        flyout.Position = new PixelPoint(Math.Clamp(x, area.X, Math.Max(area.X, area.Right - width)),
            Math.Clamp(y, area.Y, Math.Max(area.Y, area.Bottom - height)));
    }
    public void CloseFlyout()
    {
        var flyout = _flyout;
        _flyout = null;
        _flyoutName = null;
        _toolButtons.Clear();
        flyout?.Close();
    }
    public void RaiseAboveInk() { DesktopInkNative.Raise(this); if (_flyout is not null) DesktopInkNative.Raise(_flyout); }
    internal Window? OpenFlyout => _flyout;
    public void ClampToScreen()
    {
        var screen = Screens.ScreenFromWindow(this) ?? Screens.Primary;
        if (screen is null) return;
        var area = screen.WorkingArea;
        Position = new PixelPoint(Math.Clamp(Position.X, area.X, Math.Max(area.X, area.Right - (int)(Width * RenderScaling))),
            Math.Clamp(Position.Y, area.Y, Math.Max(area.Y, area.Bottom - (int)(Bounds.Height * RenderScaling))));
    }
    private void ToggleCollapsed()
    {
        _collapsed = !_collapsed;
        ((Control)_body.Parent!).IsVisible = !_collapsed;
        CloseFlyout();
        if (_collapsed && _session.InkVisible) _session.ToggleInk();
        else if (!_collapsed && !_session.InkVisible) _session.ToggleInk();
    }
    private void HandlePressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        if (e.ClickCount == 2) { ToggleCollapsed(); e.Handled = true; return; }
        _dragStart = this.PointToScreen(e.GetPosition(this));
        _windowStart = Position;
        _dragPointer = e.Pointer;
        e.Pointer.Capture(_handle);
        CloseFlyout();
        e.Handled = true;
    }
    private void HandleMoved(object? sender, PointerEventArgs e)
    {
        if (_dragPointer != e.Pointer) return;
        var current = this.PointToScreen(e.GetPosition(this));
        Position = new PixelPoint(_windowStart.X + current.X - _dragStart.X, _windowStart.Y + current.Y - _dragStart.Y);
        e.Handled = true;
    }
    private void HandleReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragPointer != e.Pointer) return;
        var current = this.PointToScreen(e.GetPosition(this));
        var click = Math.Abs(current.X - _dragStart.X) + Math.Abs(current.Y - _dragStart.Y) < 4;
        _dragPointer = null;
        e.Pointer.Capture(null);
        ClampToScreen();
        if (click && _collapsed) ToggleCollapsed();
        e.Handled = true;
    }
}
