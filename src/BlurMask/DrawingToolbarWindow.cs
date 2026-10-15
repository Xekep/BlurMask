using Avalonia;
using Avalonia.Automation;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

namespace BlurMask;

internal sealed class DrawingToolbarWindow : Window
{
    private static readonly IBrush Cyan = new SolidColorBrush(Color.Parse("#27C4E5"));
    private static readonly IBrush Edge = new SolidColorBrush(Color.Parse("#A8A8A8"));
    private static readonly ControlTheme FlatButton = CreateButtonTheme();
    private readonly DrawingSession _session;
    private readonly StackPanel _body = new() { Spacing = 0, Width = 32, ClipToBounds = true };
    private readonly Border _bodyContainer;
    private readonly Button _cursor;
    private readonly Button _drawing;
    private readonly Button _eraser;
    private readonly Button _undo;
    private readonly Button _selectedColor;
    private readonly Button _sizes;
    private readonly Button _boards;
    private readonly Button _menu;
    private readonly Button _shapes;
    private readonly Button _fading;
    private readonly Button _masks;
    private readonly Dictionary<InkTool, Button> _toolButtons = [];
    private readonly Dictionary<Button, ShortcutAction> _shortcutTips = [];
    private Window? _flyout;
    private Popup? _waylandPopup;
    private string? _flyoutName;
    private bool _collapsed;
    private bool _placed;
    private PixelPoint _dragStart;
    private PixelPoint _windowStart;
    private IPointer? _dragPointer;
    private readonly Button _handle;
    private readonly Button _eye;
    private readonly HashSet<Button> _hoverMenus = [];
    private readonly DispatcherTimer _hoverCloseTimer = new() { Interval = TimeSpan.FromMilliseconds(220) };
    private bool _flyoutPointerOver;
    private string? _updateVersion;
    private Window? _updateWindow;
    private Popup? _updatePopup;
    private Avalonia.Controls.Shapes.Path? _updateTail;
    private PixelRect? _flyoutRegionAnchor;

    public DrawingToolbarWindow(DrawingSession session)
    {
        _session = session;
        Title = UiText.T("BlurMask — инструменты рисования");
        Width = 52;
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

        var column = new StackPanel { Spacing = 3, Margin = new Thickness(8) };
        _handle = IconButton(ToolbarGlyph.Nib, UiText.T("Перетащить панель · двойной щелчок — свернуть"), ToggleCollapsed, 36, 36);
        _handle.Cursor = new Cursor(StandardCursorType.SizeAll);
        // Use explicit pointer capture so moving the toolbar does not activate the
        // program behind it and works across monitors with different scale factors.
        _handle.AddHandler(PointerPressedEvent, HandlePressed, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        _handle.AddHandler(PointerMovedEvent, HandleMoved, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        _handle.AddHandler(PointerReleasedEvent, HandleReleased, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        _handle.PointerCaptureLost += (_, _) => _dragPointer = null;
        column.Children.Add(_handle);

        _eye = IconButton(ToolbarGlyph.Eye, UiText.T("Свернуть панель / скрыть рисунок"), ToggleCollapsed);
        _body.Children.Add(_eye);
        _cursor = IconButton(ToolbarGlyph.Cursor, UiText.T("Курсор · Ctrl+Shift+1"), ActivateCursor);
        _body.Children.Add(_cursor);
        _shortcutTips[_cursor] = ShortcutAction.Cursor;
        _drawing = AddTool(ToolbarGlyph.Pen, UiText.T("Инструменты рисования · Ctrl+Shift+2"), ActivateDrawing);
        _shapes = IconButton(ToolbarGlyph.Shapes, UiText.T("Линии и фигуры"), ActivateShape);
        _shortcutTips[_drawing] = ShortcutAction.Pen;
        _body.Children.Add(_shapes);
        _eraser = AddTool(ToolbarGlyph.Eraser, UiText.T("Ластик · Ctrl+Shift+4"), () => session.SelectTool(InkTool.Eraser));
        _shortcutTips[_eraser] = ShortcutAction.Eraser;
        _sizes = IconButton(ToolbarGlyph.Size, UiText.T("Толщина пера"), () => { });
        _body.Children.Add(_sizes);
        _fading = IconButton(ToolbarGlyph.Fading, UiText.T("Исчезающие чернила"), () => { });
        _body.Children.Add(_fading);
        _shortcutTips[_fading] = ShortcutAction.ToggleFading;
        _undo = AddTool(ToolbarGlyph.Undo, UiText.T("Отменить · Ctrl+Shift+Z · ПКМ — повторить"), session.Undo);
        _shortcutTips[_undo] = ShortcutAction.Undo;
        _undo.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(_undo).Properties.IsRightButtonPressed) { session.Redo(); e.Handled = true; }
        };
        var clear = IconButton(ToolbarGlyph.Trash, UiText.T("Очистить"), session.Clear);
        _body.Children.Add(clear); _shortcutTips[clear] = ShortcutAction.Clear;
        _boards = IconButton(ToolbarGlyph.Board, UiText.T("Рабочий стол / белая / чёрная доска"), () => { if (Program.UseNativeWayland) session.OpenCanvas(); });
        _body.Children.Add(_boards);
        _masks = IconButton(ToolbarGlyph.Mask, UiText.T("Маски приватности"), () => { });
        _masks.IsEnabled = !Program.UseNativeWayland;
        _body.Children.Add(_masks);
        var camera = IconButton(ToolbarGlyph.Camera, UiText.T("Снимок экрана"), () => _ = session.CaptureScreenshotAsync());
        camera.IsEnabled = !Program.UseNativeWayland;
        _body.Children.Add(camera); _shortcutTips[camera] = ShortcutAction.Screenshot;
        _menu = IconButton(ToolbarGlyph.Menu, UiText.T("Меню BlurMask"), () => { });
        _body.Children.Add(_menu);

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
        ToolTip.SetTip(_selectedColor, UiText.T("Палитра цветов"));
        AutomationProperties.SetName(_selectedColor, UiText.T("Палитра цветов"));
        _body.Children.Add(_selectedColor);
        _bodyContainer = new Border
        {
            BorderBrush = Edge, BorderThickness = new Thickness(1), Background = Brushes.White,
            BoxShadow = new BoxShadows(new BoxShadow { Blur = 8, Spread = 1, OffsetX = 0, OffsetY = 0, Color = Color.FromArgb(100, 0, 0, 0) }),
            Child = _body, HorizontalAlignment = HorizontalAlignment.Center
        };
        column.Children.Add(_bodyContainer);
        Content = column;
        AddHandler(PointerPressedEvent, (_, e) =>
        {
            if (!e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed) return;
            _session.MinimizeToTray();
            e.Handled = true;
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        BindHoverMenu(_cursor, "cursor", ShowCursorModes);
        BindHoverMenu(_drawing, "tools", ShowTools);
        BindHoverMenu(_shapes, "shapes", ShowShapes);
        BindHoverMenu(_sizes, "sizes", ShowSizes);
        BindHoverMenu(_fading, "fading", ShowFading);
        if (!Program.UseNativeWayland) BindHoverMenu(_boards, "boards", ShowBoards);
        BindHoverMenu(_masks, "masks", ShowMasks);
        BindHoverMenu(_menu, "menu", ShowMenu);
        BindHoverMenu(_selectedColor, "palette", ShowPalette);
        foreach (var button in column.GetVisualDescendants().OfType<Button>())
            if (!_hoverMenus.Contains(button)) button.PointerEntered += (_, _) => CloseFlyout();
        _hoverCloseTimer.Tick += (_, _) =>
        {
            _hoverCloseTimer.Stop();
            if (_flyoutAnchor?.IsPointerOver != true && !_flyoutPointerOver &&
                !(_flyout?.GetVisualDescendants().OfType<ComboBox>().Any(c => c.IsDropDownOpen) ?? false)) CloseFlyout();
        };
        session.StateChanged += Refresh;
        Opened += (_, _) =>
        {
            if (!_collapsed && _bodyContainer.Bounds.Height > 0)
            {
                _bodyContainer.Height = _bodyContainer.Bounds.Height;
                _bodyContainer.Transitions =
                [
                    new DoubleTransition { Property = HeightProperty, Duration = TimeSpan.FromMilliseconds(180), Easing = new CubicEaseInOut() },
                    new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(150) }
                ];
            }
            if (!_placed && Screens.Primary is { } screen)
            {
                _placed = true;
                Position = new PixelPoint(screen.WorkingArea.X + 24,
                    screen.WorkingArea.Y + Math.Max(16, (screen.WorkingArea.Height - (int)(Height * screen.Scaling)) / 2));
            }
        };
        PositionChanged += (_, _) => { PlaceUpdateNotice(); PlaceFlyout(); session.UpdateOverlayRegions(); };
        LayoutUpdated += (_, _) => { PlaceUpdateNotice(); PlaceFlyout(); session.UpdateOverlayRegions(); };
        PropertyChanged += (_, e) =>
        {
            if (e.Property != IsVisibleProperty) return;
            if (!IsVisible) CloseUpdateNotice();
            else if (_updateVersion is { } version)
                Dispatcher.UIThread.Post(() => ShowUpdateAvailable(version), DispatcherPriority.Loaded);
        };
        KeyDown += (_, e) =>
        {
            if (session.HandleSelectionKey(e) || session.HandleLocalShortcut(e)) return;
            if (e.Key == Key.Escape) { CloseFlyout(); session.SelectTool(InkTool.Cursor, remember: false); e.Handled = true; }
        };
        Closed += (_, _) => { _updateVersion = null; _hoverCloseTimer.Stop(); CloseFlyout(); CloseUpdateNotice(); session.StateChanged -= Refresh; };
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
                Child = new Grid
                {
                    Children =
                    {
                        new Border { Name = "HoverSurface", IsHitTestVisible = false },
                        new ContentPresenter
                        {
                            [!ContentPresenter.ContentProperty] = button[!ContentProperty],
                            [!ContentPresenter.HorizontalContentAlignmentProperty] = button[!HorizontalContentAlignmentProperty],
                            [!ContentPresenter.VerticalContentAlignmentProperty] = button[!VerticalContentAlignmentProperty]
                        }
                    }
                }
            })));
        // A separate template layer also highlights buttons with a locally set
        // background (selected tools, thickness presets and colour swatches).
        theme.Children.Add(new Style(x => x.Nesting().Class(":pointerover").Template().Name("HoverSurface"))
        { Setters = { new Setter(Border.BackgroundProperty, new SolidColorBrush(Color.Parse("#D9F3F8"))) } });
        theme.Children.Add(new Style(x => x.Nesting().Class(":pressed").Template().Name("HoverSurface"))
        { Setters = { new Setter(Border.BackgroundProperty, Cyan) } });
        return theme;
    }

    private static Button IconButton(ToolbarGlyph glyph, string tip, Action click, double width = 32, double height = 30)
    {
        var button = new Button
        {
            Theme = FlatButton, Width = width, Height = height, Padding = new Thickness(0),
            Content = new ToolbarIcon(glyph, glyph == ToolbarGlyph.Nib) { Width = glyph == ToolbarGlyph.Nib ? 34 : 23, Height = glyph == ToolbarGlyph.Nib ? 34 : 23 }
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
        AutomationProperties.SetName(button, UiText.F("Цвет {0}", color));
        button.Click += (_, _) => { _session.Color = color; _session.NotifySettings(); CloseFlyout(); };
        return button;
    }

    private void Refresh()
    {
        foreach (var (button, action) in _shortcutTips)
        {
            var tip = AppSettings.Label(action) + " · " + _session.ShortcutText(action);
            if (action == ShortcutAction.Undo) tip += " · " + UiText.T("ПКМ — повторить");
            ToolTip.SetTip(button, tip); AutomationProperties.SetName(button, tip);
        }
        _cursor.Background = _session.Tool == _session.LastCursorTool ? Cyan : Brushes.Transparent;
        _cursor.Content = new ToolbarIcon(GlyphFor(_session.LastCursorTool)) { Width = 23, Height = 23 };
        _eraser.Background = _session.Tool == InkTool.Eraser ? Cyan : Brushes.Transparent;
        var isShape = _session.Tool is InkTool.Line or InkTool.Arrow or InkTool.Rectangle or InkTool.Ellipse;
        _drawing.Background = _session.Tool is InkTool.Pen or InkTool.Highlighter or InkTool.Text or InkTool.AiPen ? Cyan : Brushes.Transparent;
        _drawing.Content = new ToolbarIcon(GlyphFor(_session.LastDrawingTool)) { Width = 23, Height = 23 };
        _shapes.Background = isShape ? Cyan : Brushes.Transparent;
        _shapes.Content = new ToolbarIcon(GlyphFor(_session.LastShapeTool)) { Width = 23, Height = 23 };
        _fading.Background = _session.FadeSeconds > 0 ? Cyan : Brushes.Transparent;
        _fading.Content = new ToolbarIcon(ToolbarGlyph.Fading, timerFraction: _session.FadeSeconds switch { 2 => .25, 10 => .5, 30 => .9, _ => 0 }) { Width = 23, Height = 23 };
        _selectedColor.Background = new SolidColorBrush(_session.Color);
        _undo.Opacity = _session.Document.CanUndo ? 1 : 0.4;
        foreach (var (tool, button) in _toolButtons)
            button.Background = tool == _session.Tool ? Cyan : Brushes.Transparent;
    }
    private static ToolbarGlyph GlyphFor(InkTool tool) => tool switch
    {
        InkTool.Cursor => ToolbarGlyph.Cursor, InkTool.Select => ToolbarGlyph.Select,
        InkTool.Highlighter => ToolbarGlyph.Highlighter, InkTool.Text => ToolbarGlyph.Text,
        InkTool.AiPen => ToolbarGlyph.AiPen,
        InkTool.Line => ToolbarGlyph.Line, InkTool.Arrow => ToolbarGlyph.Arrow,
        InkTool.Rectangle => ToolbarGlyph.Rectangle, InkTool.Ellipse => ToolbarGlyph.Ellipse,
        _ => ToolbarGlyph.Pen
    };

    private void ActivateDrawing()
    {
        CloseFlyout();
        _session.SelectTool(_session.LastDrawingTool);
    }

    private void ActivateShape()
    {
        CloseFlyout();
        _session.SelectTool(_session.LastShapeTool);
    }

    private void ActivateCursor()
    {
        CloseFlyout();
        _session.SelectTool(_session.LastCursorTool);
    }

    private void BindHoverMenu(Button button, string name, Action open)
    {
        _hoverMenus.Add(button);
        button.PointerEntered += (_, _) =>
        {
            if (!button.IsEnabled || _dragPointer is not null) return;
            _hoverCloseTimer.Stop();
            if (_flyoutName != name) open();
        };
        button.PointerExited += (_, _) => QueueFlyoutClose();
    }

    private void QueueFlyoutClose()
    {
        _hoverCloseTimer.Stop();
        if (_flyoutName is not null) _hoverCloseTimer.Start();
    }

    public void ShowTools()
        => ShowToolChoices("tools", _drawing, [(InkTool.Pen, UiText.T("Перо")), (InkTool.AiPen, UiText.T("AI-перо")), (InkTool.Highlighter, UiText.T("Маркер")), (InkTool.Text, UiText.T("Текст"))]);

    internal void ShowCursorModes() => ShowToolChoices("cursor", _cursor,
        [(InkTool.Cursor, UiText.T("Обычный курсор")), (InkTool.Select, UiText.T("Выделить и переместить"))]);

    internal void ShowShapes()
        => ShowToolChoices("shapes", _shapes, [(InkTool.Line, UiText.T("Линия")), (InkTool.Arrow, UiText.T("Стрелка")),
            (InkTool.Rectangle, UiText.T("Прямоугольник")), (InkTool.Ellipse, UiText.T("Эллипс"))]);

    private void ShowToolChoices(string name, Button anchor, (InkTool Tool, string Label)[] choices)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 1 };
        var buttons = new Dictionary<InkTool, Button>();
        foreach (var (tool, label) in choices)
        {
            var button = IconButton(GlyphFor(tool), label, () => { _session.SelectTool(tool); CloseFlyout(); });
            buttons[tool] = button;
            row.Children.Add(button);
        }
        if (name == "tools" && _session.Tool == InkTool.AiPen)
        {
            var mode = new ComboBox { ItemsSource = new[] { UiText.T("Автоматически"), UiText.T("Текст"), UiText.T("Формулы") },
                SelectedIndex = _session.Settings.AiMode switch { "text" => 1, "math" => 2, _ => 0 }, MinWidth = 120, FontSize = 12 };
            var status = new TextBlock { FontSize = 11, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0) };
            void UpdateStatus() => status.Text = UiText.T(_session.Settings.AiMode == "math"
                ? AiMathRecognitionModels.Installed ? "Формулы · экспериментально" : "Модель формул не установлена"
                : _session.Settings.ExperimentalTextEngine ? "Новый движок · отдельные слова"
                : _session.Settings.AiMode == "auto" && AiMathRecognitionModels.Installed ? "Текст и формулы · автоматически"
                : File.Exists(AiRecognitionModels.PathName) ? "English · экспериментально" : "Модель AI-пера не установлена");
            mode.SelectionChanged += (_, _) => { _session.SetAiMode(mode.SelectedIndex switch { 1 => "text", 2 => "math", _ => "auto" }); UpdateStatus(); };
            UpdateStatus(); row.Children.Add(mode); row.Children.Add(status);
        }
        if (name == "cursor" && _session.Tool == InkTool.Select && _session.CanSolveSelectedMath)
        {
            var solve = IconButton(ToolbarGlyph.Text, UiText.T("Решить выделенную формулу"), () => { CloseFlyout(); _ = _session.SolveSelectedMathAsync(); });
            solve.Content = new TextBlock { Text = "=", FontSize = 23, FontWeight = FontWeight.SemiBold };
            row.Children.Add(solve);
        }
        ShowFlyout(name, row, anchor);
        if (_flyoutName == name)
            foreach (var (tool, button) in buttons) _toolButtons[tool] = button;
        Refresh();
    }
    internal void ShowFading()
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        foreach (var (seconds, label) in new[] { (0d, UiText.T("Выключены")), (2d, UiText.T("Быстро")),
            (10d, UiText.T("Долго")), (30d, UiText.T("Очень долго")) })
        {
            var button = IconButton(ToolbarGlyph.Fading, label, () => { CloseFlyout(); _session.SetFading(seconds); });
            button.Content = new ToolbarIcon(ToolbarGlyph.Fading, timerFraction: seconds switch { 2 => .25, 10 => .5, 30 => .9, _ => 0 }) { Width = 23, Height = 23 };
            button.Background = _session.FadeSeconds == seconds ? Cyan : Brushes.Transparent;
            if (seconds > 0) ToolTip.SetTip(button, label + " · " + UiText.F("{0} секунд", seconds));
            row.Children.Add(button);
        }
        ShowFlyout("fading", row, _fading);
    }
    internal void ShowMasks()
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        foreach (var (mode, glyph, label) in new[]
        {
            (PrivacyMode.Blur, ToolbarGlyph.MaskBlur, "Размытие"),
            (PrivacyMode.BigPixels, ToolbarGlyph.MaskPixels, "Крупные пиксели"),
            (PrivacyMode.Scramble, ToolbarGlyph.MaskScramble, "Перемешивание"),
            (PrivacyMode.GlassBlocks, ToolbarGlyph.MaskGlass, "Преломление стекла"),
            (PrivacyMode.Blackout, ToolbarGlyph.MaskBlackout, "Чёрная маска")
        }) row.Children.Add(IconButton(glyph, UiText.T(label), () => { CloseFlyout(); _session.CreateMask(mode); }));
        var panel = new StackPanel { Spacing = 5 }; panel.Children.Add(row);
        using var catalog = NativeWindowCatalog.TryCreate();
        if (_session.AttachMasksToWindows && catalog is not null)
            row.Children.Add(IconButton(ToolbarGlyph.MaskFill, UiText.T("Залить область окна"), () => { CloseFlyout(); _session.BeginWindowFill(); }));
        row.Children.Add(IconButton(ToolbarGlyph.Trash, UiText.T("Удалить все маски"), () => { CloseFlyout(); _session.CloseAllMasks(); }));
        var attach = new ToggleSwitch { Content = UiText.T("Привязать к окну"), IsChecked = _session.AttachMasksToWindows,
            IsEnabled = catalog is not null, FontSize = 12, Margin = new Thickness(4, 0), OnContent = UiText.T("Включено"), OffContent = UiText.T("Выключено") };
        ToolTip.SetTip(attach, UiText.T(catalog is null ? "Привязка недоступна в этой оконной системе" : "Определять окно под маской автоматически"));
        attach.IsCheckedChanged += (_, _) => { _session.SetMaskWindowAttachment(attach.IsChecked == true); CloseFlyout(); ShowMasks(); };
        panel.Children.Add(attach); ShowFlyout("masks", panel, _masks);
    }
    internal void ShowSizes()
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
        ShowFlyout("sizes", row, _sizes);
    }
    private void ShowBoards()
    {
        if (Program.UseNativeWayland) { _session.OpenCanvas(); return; }
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        foreach (var (board, name, color) in new[]
        {
            (BoardMode.Desktop, UiText.T("Рабочий стол"), "#D1E8EF"),
            (BoardMode.White, UiText.T("Белая доска"), "#FFFFFF"), (BoardMode.Black, UiText.T("Чёрная доска"), "#111111")
        })
        {
            var button = IconButton(ToolbarGlyph.Board, name, () => { _session.SelectScreen(); _session.SetBoard(board); CloseFlyout(); });
            button.Background = new SolidColorBrush(Color.Parse(color));
            button.BorderBrush = board == _session.Board ? Cyan : Edge;
            button.BorderThickness = new Thickness(2);
            row.Children.Add(button);
        }
        row.Children.Add(IconButton(ToolbarGlyph.Pen, AppSettings.Label(ShortcutAction.Canvas) + " · " + _session.ShortcutText(ShortcutAction.Canvas), () => { _session.OpenCanvas(); CloseFlyout(); }));
        ShowFlyout("boards", row, _boards);
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
        var apply = new Button { Content = UiText.T("Применить цвет"), HorizontalAlignment = HorizontalAlignment.Stretch };
        apply.Click += (_, _) =>
        {
            if (Color.TryParse(custom.Text, out var color))
            { _session.Color = color; _session.NotifySettings(); CloseFlyout(); }
            else custom.PlaceholderText = UiText.T("Введите #RRGGBB");
        };
        panel.Children.Add(apply);
        ShowFlyout("palette", panel, _selectedColor);
    }

    internal void ShowMenu()
    {
        var panel = new StackPanel { Width = 270, Spacing = 3, Margin = new Thickness(7) };
        panel.Children.Add(new TextBlock { Text = "BlurMask", FontSize = 18, FontWeight = FontWeight.SemiBold, Margin = new Thickness(5, 0, 0, 7) });
        panel.Children.Add(new TextBlock { Text = UiText.T("Размер текста"), FontSize = 12, Margin = new Thickness(5, 3) });
        var textSize = new Slider { Minimum = 14, Maximum = 72, Value = _session.TextSize, TickFrequency = 2, IsSnapToTickEnabled = true };
        textSize.ValueChanged += (_, _) => _session.TextSize = textSize.Value;
        panel.Children.Add(textSize);
        MenuItem(panel, UiText.T("Настройки"), _session.ShowSettings);
        panel.Children.Add(new Border { Height = 1, Background = Edge, Margin = new Thickness(0, 5) });
        MenuItem(panel, UiText.T("Свернуть в трей"), _session.MinimizeToTray);
        MenuItem(panel, UiText.T("Выход"), _session.Exit);
        ShowFlyout("menu", panel, _menu);
    }
    private void MenuItem(StackPanel panel, string text, Action click)
    {
        Control content = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap };
        var button = new Button { Theme = FlatButton, Content = new Border { Padding = new Thickness(7, 6), Child = content }, FontSize = 12, MinHeight = 30, HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left, Padding = new Thickness(7, 6) };
        button.Click += (_, _) => { CloseFlyout(); click(); };
        panel.Children.Add(button);
    }

    public void ShowMessage(string title, string message)
    {
        var panel = new StackPanel { Width = 290, Margin = new Thickness(12), Spacing = 10 };
        panel.Children.Add(new TextBlock { Text = title, FontWeight = FontWeight.SemiBold, FontSize = 15 });
        panel.Children.Add(new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap, FontSize = 12 });
        var close = new Button { Content = UiText.T("Закрыть"), HorizontalAlignment = HorizontalAlignment.Right };
        close.Click += (_, _) => CloseFlyout();
        panel.Children.Add(close);
        ShowFlyout("message", panel, _menu);
    }

    private Control? _flyoutAnchor;
    internal Task ScreenshotActionTask { get; private set; } = Task.CompletedTask;
    internal void ShowScreenshotActions(byte[] png, PixelRect region)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        var save = IconButton(ToolbarGlyph.Save, UiText.T("Сохранить снимок"), () => { });
        save.Click += async (_, _) =>
        {
            CloseFlyout();
            try
            {
                var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = UiText.T("Сохранить снимок"), SuggestedFileName = $"BlurMask-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.png",
                    DefaultExtension = "png", FileTypeChoices = [new FilePickerFileType("PNG") { Patterns = ["*.png"] }],
                    ShowOverwritePrompt = true
                });
                if (file is null) return;
                await using var stream = await file.OpenWriteAsync();
                stream.SetLength(0);
                await stream.WriteAsync(png);
            }
            catch (Exception ex) { ShowMessage(UiText.T("Не удалось сохранить снимок"), ex.Message); }
        };
        var copy = IconButton(ToolbarGlyph.Copy, UiText.T("Копировать снимок"), () => { });
        copy.IsEnabled = Clipboard is not null;
        copy.Click += (_, _) => ScreenshotActionTask = CopyScreenshotAsync(png);
        row.Children.Add(save); row.Children.Add(copy);
        row.Children.Add(IconButton(ToolbarGlyph.Close, UiText.T("Отмена"), CloseFlyout));
        ShowFlyout("screenshot", row, _handle);
        _flyoutRegionAnchor = region;
        PlaceFlyout();
    }

    private async Task CopyScreenshotAsync(byte[] png)
    {
        CloseFlyout();
        try
        {
            if (Clipboard is not { } clipboard) return;
            var transfer = new ClipboardBitmapTransfer(png);
            try { await clipboard.SetDataAsync(transfer); }
            catch { transfer.Dispose(); throw; }
            // Windows materializes delayed formats, preserving the screenshot
            // when the application exits. Other backends keep transfer ownership.
            await clipboard.FlushAsync();
        }
        catch (Exception ex) { ShowMessage(UiText.T("Не удалось скопировать снимок"), ex.Message); }
    }

    internal void ShowUpdateAvailable(string version)
    {
        _updateVersion = version;
        if (!IsVisible || _updateWindow is not null || _updatePopup is not null) return;
        var text = new StackPanel { Spacing = 7, Width = 220 };
        text.Children.Add(new TextBlock { Text = UiText.F("Вышла новая версия {0}", version),
            FontWeight = FontWeight.SemiBold, FontSize = 14, TextWrapping = TextWrapping.Wrap });
        text.Children.Add(new TextBlock { Text = UiText.T("Обновимся?"), FontSize = 12 });
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 7 };
        var download = new Button { Content = UiText.T("Скачать обновление"), FontSize = 12 };
        download.Click += async (_, _) =>
        {
            try { await Launcher.LaunchUriAsync(new Uri(AppUpdates.Website)); }
            catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception) { }
        };
        var later = new Button { Content = UiText.T("Позже"), FontSize = 12 };
        later.Click += (_, _) => { _updateVersion = null; CloseUpdateNotice(); };
        actions.Children.Add(download); actions.Children.Add(later); text.Children.Add(actions);
        var fill = new SolidColorBrush(Color.Parse("#FFF9E8"));
        var bubble = new Grid { ColumnDefinitions = new ColumnDefinitions("10,*,10") };
        var tail = new Avalonia.Controls.Shapes.Path { Data = Geometry.Parse("M10,0 L0,8 L10,16 Z"),
            Fill = fill, Width = 10, Height = 16, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 12, 0, 0) };
        var body = new Border { Background = fill, CornerRadius = new CornerRadius(12), Padding = new Thickness(12), Child = text };
        Grid.SetColumn(body, 1); bubble.Children.Add(tail); bubble.Children.Add(body);
        _updateTail = tail;
        if (Program.UseNativeWayland)
        {
            // A persistent non-light-dismiss popup uses parent-relative placement;
            // native Wayland has no global positioning for toplevel windows.
            var popup = new Popup { PlacementTarget = _handle, Placement = PlacementMode.Top,
                IsLightDismissEnabled = false, Child = bubble };
            _updatePopup = popup;
            ((StackPanel)Content!).Children.Add(popup);
            popup.IsOpen = true;
            return;
        }
        var notice = new Window
        {
            Title = UiText.T("Скачать обновление"), WindowDecorations = WindowDecorations.None,
            SizeToContent = SizeToContent.WidthAndHeight, ShowInTaskbar = false, Topmost = true,
            ShowActivated = false, CanResize = false, CanMinimize = false, CanMaximize = false,
            RequestedThemeVariant = ThemeVariant.Light, Background = Brushes.White, Content = bubble
        };
        if (!OperatingSystem.IsLinux())
        {
            notice.Background = Brushes.Transparent;
            notice.TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
            notice.TransparencyBackgroundFallback = Brushes.White;
        }
        _updateWindow = notice;
        notice.Opened += (_, _) => { PlaceUpdateNotice(); PlaceFlyout(); };
        notice.LayoutUpdated += (_, _) => PlaceUpdateNotice();
        notice.PositionChanged += (_, _) => _session.UpdateOverlayRegions();
        notice.Closed += (_, _) =>
        {
            if (!ReferenceEquals(_updateWindow, notice)) return;
            _updateWindow = null;
            _updateTail = null;
            _session.UpdateOverlayRegions();
        };
        notice.Show(this);
        Dispatcher.UIThread.Post(PlaceUpdateNotice, DispatcherPriority.Loaded);
    }

    private void PlaceUpdateNotice()
    {
        if (_updateWindow is not { IsVisible: true } notice || Screens.ScreenFromWindow(this) is not { } screen) return;
        var area = screen.WorkingArea;
        var width = (int)Math.Ceiling(notice.Bounds.Width * notice.RenderScaling);
        var height = (int)Math.Ceiling(notice.Bounds.Height * notice.RenderScaling);
        var x = Position.X + (int)Math.Ceiling(Bounds.Width * RenderScaling);
        if (x + width > area.Right) x = Position.X - width;
        if (_updateTail is { } tail)
        {
            var left = x < Position.X;
            Grid.SetColumn(tail, left ? 2 : 0);
            tail.RenderTransform = left ? new ScaleTransform(-1, 1) : null;
        }
        var position = new PixelPoint(Math.Clamp(x, area.X, Math.Max(area.X, area.Right - width)),
            Math.Clamp(_handle.PointToScreen(default).Y - (int)Math.Round(4 * RenderScaling),
                area.Y, Math.Max(area.Y, area.Bottom - height)));
        if (notice.Position != position) notice.Position = position;
    }

    private void CloseUpdateNotice()
    {
        var popup = _updatePopup; _updatePopup = null;
        if (popup is not null) { popup.IsOpen = false; ((StackPanel)Content!).Children.Remove(popup); }
        var notice = _updateWindow; _updateWindow = null;
        _updateTail = null;
        notice?.Close();
        _session.UpdateOverlayRegions();
    }

    internal Window? UpdateNoticeWindow => _updateWindow;
    internal Control? UpdateNoticeContent => _updateWindow?.Content as Control ?? _updatePopup?.Child;
    internal bool IsUpdateNoticeVisible => _updateWindow?.IsVisible == true || _updatePopup?.IsOpen == true;

    private void ShowFlyout(string name, Control content, Control anchor)
    {
        if (_flyoutName == name) return;
        CloseFlyout();
        _flyoutName = name;
        _flyoutAnchor = anchor;
        var surface = new Border { Background = Brushes.White, BorderBrush = Edge,
            BorderThickness = new Thickness(1), Padding = new Thickness(3), Child = content };
        surface.PointerEntered += (_, _) => { _flyoutPointerOver = true; _hoverCloseTimer.Stop(); };
        surface.PointerExited += (_, _) => { _flyoutPointerOver = false; QueueFlyoutClose(); };
        if (Program.UseNativeWayland)
        {
            // Native xdg_popup placement is relative to its parent; absolute window
            // coordinates and Topmost are unavailable on the Wayland backend.
            var popup = new Popup { PlacementTarget = anchor, Placement = PlacementMode.Right,
                IsLightDismissEnabled = true,
                Child = surface };
            _waylandPopup = popup;
            ((StackPanel)Content!).Children.Add(popup);
            popup.Closed += (_, _) => { if (ReferenceEquals(_waylandPopup, popup)) CloseFlyout(); };
            popup.IsOpen = true;
            return;
        }
        _flyout = new Window
        {
            Title = UiText.T("BlurMask — меню"), WindowDecorations = WindowDecorations.None,
            SizeToContent = SizeToContent.WidthAndHeight, ShowInTaskbar = false, Topmost = true,
            CanResize = false, CanMinimize = false, CanMaximize = false,
            RequestedThemeVariant = ThemeVariant.Light,
            Background = Brushes.White,
            Content = surface
        };
        var openedFlyout = _flyout;
        openedFlyout.Closed += (_, _) =>
        {
            if (!ReferenceEquals(_flyout, openedFlyout)) return;
            _flyout = null;
            _flyoutName = null;
            _flyoutAnchor = null;
            _hoverCloseTimer.Stop();
            _flyoutPointerOver = false;
            _toolButtons.Clear();
            _session.UpdateOverlayRegions();
        };
        _flyout.KeyDown += (_, e) => { if (e.Key == Key.Escape) CloseFlyout(); };
        _flyout.Opened += (_, _) => PlaceFlyout();
        _flyout.Resized += (_, _) => PlaceFlyout();
        _flyout.LayoutUpdated += (_, _) => PlaceFlyout();
        _flyout.PositionChanged += (_, _) => _session.UpdateOverlayRegions();
        _flyout.Show(this);
        PlaceFlyout();
        Dispatcher.UIThread.Post(PlaceFlyout, DispatcherPriority.Loaded);
    }
    private void PlaceFlyout()
    {
        if (_flyout is not { IsVisible: true } flyout || _flyoutAnchor is not { } anchor) return;
        var screen = _flyoutRegionAnchor is { } regionAnchor ? Screens.ScreenFromPoint(regionAnchor.BottomRight) ?? Screens.Primary : Screens.ScreenFromWindow(this) ?? Screens.Primary;
        if (screen is null) return;
        var area = screen.WorkingArea;
        var scale = RenderScaling;
        var width = (int)Math.Ceiling(flyout.Bounds.Width * flyout.RenderScaling);
        var height = (int)Math.Ceiling(flyout.Bounds.Height * flyout.RenderScaling);
        var x = _flyoutRegionAnchor is { } selection ? selection.Right - width : Position.X + (int)Math.Ceiling(Bounds.Width * scale);
        if (_flyoutRegionAnchor is null && x + width > area.Right) x = Position.X - width;
        // Anchor to the actual button after layout, including borders, margins and DPI.
        var y = _flyoutRegionAnchor is { } selectedArea ? selectedArea.Bottom + (int)Math.Round(6 * scale) : anchor.PointToScreen(default).Y - (int)Math.Round(4 * scale);
        if (_flyoutRegionAnchor is { } selectedRegion && y + height > area.Bottom) y = selectedRegion.Y - height - (int)Math.Round(6 * scale);
        if (_updateWindow is { IsVisible: true } notice)
        {
            var noticeBounds = new PixelRect(notice.Position, new PixelSize(
                (int)Math.Ceiling(notice.Bounds.Width * notice.RenderScaling), (int)Math.Ceiling(notice.Bounds.Height * notice.RenderScaling)));
            if (new PixelRect(x, y, width, height).Intersects(noticeBounds)) y = noticeBounds.Bottom + (int)Math.Round(6 * scale);
        }
        var position = new PixelPoint(Math.Clamp(x, area.X, Math.Max(area.X, area.Right - width)),
            Math.Clamp(y, area.Y, Math.Max(area.Y, area.Bottom - height)));
        if (flyout.Position != position) flyout.Position = position;
        _session.UpdateOverlayRegions();
    }
    public void CloseFlyout()
    {
        _hoverCloseTimer.Stop();
        _flyoutPointerOver = false;
        _flyoutRegionAnchor = null;
        var popup = _waylandPopup; _waylandPopup = null;
        if (popup is not null) { popup.IsOpen = false; ((StackPanel)Content!).Children.Remove(popup); }
        var flyout = _flyout;
        _flyout = null;
        _flyoutName = null;
        _flyoutAnchor = null;
        _toolButtons.Clear();
        flyout?.Close();
        _session.UpdateOverlayRegions();
    }
    public void RaiseAboveInk() { DesktopInkNative.Raise(this); if (_flyout is not null) DesktopInkNative.Raise(_flyout); if (_updateWindow is not null) DesktopInkNative.Raise(_updateWindow); }
    internal Window? OpenFlyout => _flyout;
    public void ClampToScreen()
    {
        if (Program.UseNativeWayland) return;
        var screen = Screens.ScreenFromWindow(this) ?? Screens.Primary;
        if (screen is null) return;
        var area = screen.WorkingArea;
        Position = new PixelPoint(Math.Clamp(Position.X, area.X, Math.Max(area.X, area.Right - (int)(Width * RenderScaling))),
            Math.Clamp(Position.Y, area.Y, Math.Max(area.Y, area.Bottom - (int)(Bounds.Height * RenderScaling))));
    }
    internal void ToggleCollapsed()
    {
        _collapsed = !_collapsed;
        _bodyContainer.Height = _collapsed ? _eye.Height + 2 : _body.Children.Sum(c => c.DesiredSize.Height) + 2;
        _eye.Content = new ToolbarIcon(_collapsed ? ToolbarGlyph.ClosedEye : ToolbarGlyph.Eye) { Width = 23, Height = 23 };
        ToolTip.SetTip(_eye, _collapsed ? UiText.T("Развернуть панель") : UiText.T("Свернуть панель / скрыть рисунок"));
        CloseFlyout();
        if (_collapsed && _session.InkVisible) _session.ToggleInk();
        else if (!_collapsed && !_session.InkVisible) _session.ToggleInk();
    }
    private void HandlePressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        if (e.ClickCount == 2) { ToggleCollapsed(); e.Handled = true; return; }
        if (Program.UseNativeWayland) { CloseFlyout(); BeginMoveDrag(e); e.Handled = true; return; }
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
