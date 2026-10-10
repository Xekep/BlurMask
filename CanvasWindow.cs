using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using BlurMask.Shared;

namespace BlurMask;

internal sealed class CanvasWindow : Window
{
    private readonly DrawingSession _session;
    private readonly CanvasInkSurface _surface;
    private readonly TextBox _title;
    private readonly TextBlock _status;
    private readonly Border _chrome;
    private readonly Border _statusBadge;
    private readonly Border _zoomBadge;
    private readonly CanvasCloudClient _cloud = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(1300) };
    private readonly string _statePath;
    private BoardSnapshot _board = new() { Title = UiText.T("Новое полотно") };
    private BoardSnapshot _baseline = new();
    private BoardConnection? _connection;
    private BoardPatch? _pending;
    private BoardSnapshot? _pendingBefore;
    private string? _viewKey;
    private (BoardSnapshot Remote, BoardSnapshot Before)? _deferred;
    private bool _busy, _shutdown, _loading, _dirty;
    private int _generation;
    public bool ReadOnly => _board.ReadOnly;
    public string BoardBackground => _board.Background;
    public int BoardWidth => _board.Width;
    public int BoardHeight => _board.Height;
    internal CanvasInkSurface Surface => _surface;

    public CanvasWindow(DrawingSession session)
    {
        _session = session;
        Title = UiText.T("BlurMask — Полотно");
        Width = 1100; Height = 760; MinWidth = 650; MinHeight = 450;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        RequestedThemeVariant = ThemeVariant.Light;
        Background = new SolidColorBrush(Color.Parse("#F0F3F5"));
        var storage = Program.SmokeTest
            ? Path.Combine(Environment.GetEnvironmentVariable("BLURMASK_SMOKE_ARTIFACTS") ?? Path.GetTempPath(), "canvas-smoke")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BlurMask");
        _statePath = Path.Combine(storage, "canvas.json");
        if (!Program.SmokeTest) LoadState();
        _surface = new CanvasInkSurface(session, this);
        var workspace = new Grid();
        workspace.Children.Add(_surface);
        var top = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        _title = new TextBox { Text = _board.Title, Width = 210, MaxLength = 120, IsReadOnly = ReadOnly,
            FontSize = 13, BorderThickness = new Thickness(0), Background = Brushes.Transparent };
        _title.LostFocus += (_, _) =>
        {
            if (ReadOnly) return;
            _board = _board with { Title = string.IsNullOrWhiteSpace(_title.Text) ? UiText.T("Новое полотно") : _title.Text.Trim() };
            SaveState();
        };
        top.Children.Add(_title);
        top.Children.Add(CanvasButton("⊡", UiText.T("Вместить"), () => _surface.Fit()));
        var share = CanvasButton("↗ " + UiText.T("Совместная работа"), UiText.T("Поделиться"), () => _ = ShareAsync());
        share.FontSize = 13; top.Children.Add(share);
        var menuButton = CanvasButton("···", UiText.T("Меню полотна"), () => { });
        var menu = new ContextMenu();
        foreach (var (label, action) in new (string, Action)[]
        {
            ("Вставить изображение · Ctrl+V", () => _ = PasteImageAsync()), ("Новое", NewLocal),
            ("Подключить", ShowConnection), ("Фон", ToggleBackground), ("Сохранить PNG", SavePng),
            ("Экран", session.SelectScreen), ("Настройки", session.ShowSettings), ("Скрыть меню · Tab", ToggleChrome)
        })
        {
            var item = new MenuItem { Header = UiText.T(label) };
            item.Click += (_, _) => action(); menu.Items.Add(item);
        }
        menuButton.Click += (_, _) => menu.Open(menuButton);
        top.Children.Add(menuButton);
        _chrome = new Border { Child = top, Background = Brushes.White, CornerRadius = new CornerRadius(12),
            Padding = new Thickness(8, 5), Margin = new Thickness(62, 12, 12, 0),
            HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top,
            BoxShadow = new BoxShadows(new BoxShadow { Blur = 18, OffsetY = 4, Color = Color.FromArgb(30, 0, 0, 0) }) };
        workspace.Children.Add(_chrome);
        var reveal = CanvasButton(new ToolbarIcon(ToolbarGlyph.Nib, true) { Width = 28, Height = 28 }, UiText.T("Показать / скрыть меню · Tab"), ToggleChrome);
        reveal.Margin = new Thickness(12, 12, 0, 0); reveal.Width = 40; reveal.Height = 40;
        reveal.HorizontalAlignment = HorizontalAlignment.Left; reveal.VerticalAlignment = VerticalAlignment.Top;
        workspace.Children.Add(reveal);
        _status = new TextBlock { Text = _connection is null ? UiText.T("Сохранено на устройстве") : UiText.T("Подключение…"), FontSize = 12 };
        _statusBadge = new Border { Child = _status, Background = new SolidColorBrush(Color.FromArgb(225, 255, 255, 255)),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(10, 5), Margin = new Thickness(12),
            MaxWidth = 500, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Bottom,
            IsHitTestVisible = false };
        workspace.Children.Add(_statusBadge); Content = workspace;
        var zoomRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        var zoomLevel = CanvasButton("100%", UiText.T("Масштаб 100%"), () => _surface.ZoomAt(1 / _surface.Zoom));
        zoomLevel.FontSize = 12; zoomLevel.MinWidth = 60;
        zoomRow.Children.Add(CanvasButton("−", UiText.T("Уменьшить масштаб"), () => _surface.ZoomAt(1 / 1.2)));
        zoomRow.Children.Add(zoomLevel);
        zoomRow.Children.Add(CanvasButton("+", UiText.T("Увеличить масштаб"), () => _surface.ZoomAt(1.2)));
        zoomRow.Children.Add(CanvasButton("⊡", UiText.T("Вместить"), _surface.Fit));
        _zoomBadge = new Border { Child = zoomRow, Background = Brushes.White, CornerRadius = new CornerRadius(10),
            Padding = new Thickness(4), Margin = new Thickness(12), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom };
        workspace.Children.Add(_zoomBadge);
        _surface.ViewChanged += () => zoomLevel.Content = _surface.Zoom < .01 ? $"{_surface.Zoom * 100:0.0}%" : $"{_surface.Zoom * 100:0}%";
        AddHandler(KeyDownEvent, CanvasKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        session.CanvasDocument.Changed += OnDocumentChanged;
        _timer.Tick += (_, _) => { if (_dirty && !session.CanvasDocument.HasActiveEdit) SaveState(); _ = SyncAsync(); };
        _timer.Start();
        Activated += (_, _) => { if (!_shutdown) session.ActivateCanvas(); };
        Closing += (_, e) =>
        {
            if (_shutdown) return;
            e.Cancel = true; FinishInput(); SaveState(); Hide(); session.SelectScreen();
        };
        Closed += (_, _) => { _timer.Stop(); session.CanvasDocument.Changed -= OnDocumentChanged; _cloud.Dispose(); };
    }

    private static Button CanvasButton(object content, string tip, Action action)
    {
        var button = new Button { Content = content, Padding = new Thickness(8, 4), FontSize = 20,
            Height = 32, MinWidth = 32, CornerRadius = new CornerRadius(8), Background = Brushes.Transparent,
            BorderThickness = new Thickness(0) };
        ToolTip.SetTip(button, tip); Avalonia.Automation.AutomationProperties.SetName(button, tip);
        button.Click += (_, _) => action(); return button;
    }
    internal void ToggleChrome() { _chrome.IsVisible = !_chrome.IsVisible; _statusBadge.IsVisible = _zoomBadge.IsVisible = _chrome.IsVisible; _surface.Focus(); }
    private void ToggleBackground()
    {
        if (ReadOnly) return;
        _board = _board with { Background = _board.Background == "white" ? "black" : "white" };
        if (BoardBackground == "black" && _session.Color == Colors.Black) _session.Color = Colors.White;
        SaveState(); Refresh();
    }
    private void CanvasKeyDown(object? sender, KeyEventArgs e)
    {
        if (FocusManager?.GetFocusedElement() is TextBox) return;
        if (_session.HandleLocalShortcut(e)) return;
        if (e.Key == Key.Tab && e.KeyModifiers == KeyModifiers.None) { ToggleChrome(); e.Handled = true; }
        else if (e.KeyModifiers == KeyModifiers.Control && e.Key is Key.Add or Key.OemPlus or Key.Subtract or Key.OemMinus or Key.D0 or Key.NumPad0)
        {
            if (e.Key is Key.D0 or Key.NumPad0) _surface.Fit();
            else _surface.ZoomAt(e.Key is Key.Subtract or Key.OemMinus ? 1 / 1.2 : 1.2);
            e.Handled = true;
        }
        else if (e.Key == Key.V && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        { _ = PasteImageAsync(); e.Handled = true; }
    }
    internal async Task PasteImageAsync()
    {
        if (ReadOnly || Clipboard is null) return;
        var generation = _generation;
        try
        {
            using var source = await Clipboard.TryGetBitmapAsync();
            if (generation != _generation || _shutdown || ReadOnly) return;
            if (source is null) { _status.Text = UiText.T("В буфере нет изображения"); return; }
            InsertImage(source);
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or InvalidOperationException or NotSupportedException)
        { _status.Text = UiText.T("Не удалось вставить изображение"); }
    }
    internal void InsertImage(Bitmap source)
    {
        if (ReadOnly || _session.CanvasDocument.Marks.Count >= BoardValidation.MaxMarks) return;
        FinishInput();
        var original = source.PixelSize;
        if (original.Width <= 0 || original.Height <= 0 || (long)original.Width * original.Height > 64 * 1024 * 1024)
            throw new ArgumentException("Image dimensions exceed the limit.");
        var ratio = Math.Min(1, 2048d / Math.Max(original.Width, original.Height));
        byte[] png;
        while (true)
        {
            var size = new PixelSize(Math.Max(1, (int)(original.Width * ratio)), Math.Max(1, (int)(original.Height * ratio)));
            var view = new Image { Source = source, Width = size.Width, Height = size.Height, Stretch = Stretch.Fill };
            view.Measure(new Size(size.Width, size.Height)); view.Arrange(new Rect(0, 0, size.Width, size.Height));
            using var bitmap = new RenderTargetBitmap(size, new Vector(96, 96)); bitmap.Render(view);
            using var stream = new MemoryStream(); bitmap.Save(stream, PngBitmapEncoderOptions.Default); png = stream.ToArray();
            if (png.Length <= 1500 * 1024) break;
            ratio *= .75;
            if (Math.Max(original.Width, original.Height) * ratio < 128) throw new ArgumentException("Image exceeds the size limit.");
        }
        var scale = Math.Min(1, Math.Min(BoardWidth * .7 / original.Width, BoardHeight * .7 / original.Height));
        var dimensions = new Size(original.Width * scale, original.Height * scale);
        var center = _surface.ToBoard(new Point(_surface.Bounds.Width / 2, _surface.Bounds.Height / 2));
        var limit = CanvasGeometry.CoordinateLimit;
        var origin = new Point(Math.Clamp(center.X - dimensions.Width / 2, -limit, limit - dimensions.Width),
            Math.Clamp(center.Y - dimensions.Height / 2, -limit, limit - dimensions.Height));
        var mark = new InkMark(InkTool.Image, Colors.Black, 1, origin) { Image = Convert.ToBase64String(png) };
        mark.AddPoint(origin + new Vector(dimensions.Width, dimensions.Height));
        var candidate = Snapshot() with { Marks = [.. _session.CanvasDocument.Export(), new InkMarkDto
            { Id = mark.Id, Tool = "Image", Image = mark.Image, Points = mark.Points.Select(p => new InkPointDto(p.X, p.Y)).ToList() }] };
        if (!BoardValidation.Valid(candidate) || System.Text.Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(candidate, BoardJsonContext.Default.BoardSnapshot)) > BoardValidation.MaxDocumentBytes)
        { _status.Text = UiText.T("Полотно достигло лимита размера"); return; }
        _session.ActivateCanvas(); _session.CanvasDocument.Add(mark); _session.SelectTool(InkTool.Select);
        _session.CanvasSelection.Begin(mark.Bounds.Center, 0); _session.CanvasSelection.Complete(); SaveState();
    }
    private void OnDocumentChanged() { if (!_loading) _dirty = true; }
    private void NewLocal()
    {
        if (_busy) { _status.Text = UiText.T("Дождитесь текущего сохранения."); return; }
        FinishInput(); SaveState();
        try { if (File.Exists(_statePath)) File.Copy(_statePath, _statePath + $".backup-{DateTime.Now:yyyyMMdd-HHmmss-fff}.json", true); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { _status.Text = UiText.T("Не удалось сохранить резервную копию: ") + ex.Message; return; }
        _generation++; _connection = null; _pending = null; _pendingBefore = null; _viewKey = null; _deferred = null;
        _baseline = new(); Apply(new() { Title = UiText.T("Новое полотно") }, true); SaveState(); _surface.Fit();
        _status.Text = UiText.T("Сохранено на устройстве");
    }
    public void Refresh() { _surface.UpdateBounds(); _surface.InvalidateVisual(); _title.IsReadOnly = ReadOnly; }
    public void FinishInput() => _surface.FinishInput();
    public void Shutdown() { _shutdown = true; FinishInput(); SaveState(); _timer.Stop(); Close(); }
    internal BoardSnapshot Snapshot() => _board with { Marks = _session.CanvasDocument.Export() };
    private void Apply(BoardSnapshot board, bool resetHistory = false)
    {
        if (!BoardValidation.Valid(board)) throw new InvalidDataException(UiText.T("Сервер вернул некорректное полотно."));
        _loading = true;
        try { _board = board; _session.CanvasDocument.Import(board.Marks, resetHistory); if (resetHistory) _surface.ResetBounds(); if (!_title.IsFocused) _title.Text = board.Title; Refresh(); }
        finally { _loading = false; }
    }
    private void LoadState()
    {
        try
        {
            if (!File.Exists(_statePath)) return;
            var state = JsonSerializer.Deserialize(File.ReadAllText(_statePath), CanvasJsonContext.Default.CanvasState);
            if (state is null || !BoardValidation.Valid(state.Board)) return;
            _board = state.Board; _baseline = state.Baseline; _connection = state.Connection;
            _pending = state.Pending; _pendingBefore = state.PendingBefore; _viewKey = state.ViewKey;
            _session.CanvasDocument.Import(state.Board.Marks, true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or ArgumentException) { /* Keep a corrupt cache intact until the next valid save. */ }
    }
    private void SaveState()
    {
        if (_session.CanvasDocument.HasActiveEdit) { _dirty = true; return; }
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_statePath)!);
            var state = new CanvasState { Board = Snapshot(), Baseline = _baseline, Connection = _connection,
                Pending = _pending, PendingBefore = _pendingBefore, ViewKey = _viewKey };
            File.WriteAllText(_statePath + ".tmp", JsonSerializer.Serialize(state, CanvasJsonContext.Default.CanvasState));
            File.Move(_statePath + ".tmp", _statePath, true); _dirty = false;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { _status.Text = UiText.T("Не удалось сохранить на устройстве."); }
    }

    public async Task SyncAsync()
    {
        if (_busy || _shutdown || _connection is null || _session.CanvasDocument.HasActiveEdit) return;
        if (_deferred is { } deferred)
        {
            Apply(CanvasMerge.Rebase(deferred.Remote, deferred.Before, Snapshot()));
            _baseline = deferred.Remote; _deferred = null; SaveState();
        }
        _busy = true;
        var generation = _generation;
        try
        {
            var before = _pendingBefore ?? Snapshot();
            var patch = _pending ?? CanvasMerge.Difference(_baseline, before);
            BoardSnapshot remote;
            if (!ReadOnly && CanvasMerge.HasChanges(patch))
            {
                _pending = patch; _pendingBefore = before; SaveState();
                remote = await _cloud.ApplyAsync(_connection, patch);
                if (generation != _generation || _shutdown) return;
                _pending = null; _pendingBefore = null;
            }
            else remote = await _cloud.ReadAsync(_connection);
            if (generation != _generation || _shutdown) return;
            if (_session.CanvasDocument.HasActiveEdit) _deferred = (remote, before);
            else
            {
                Apply(CanvasMerge.Rebase(remote, before, Snapshot()));
                _baseline = remote; SaveState();
            }
            _status.Text = remote.ReadOnly ? UiText.T("Только просмотр · подключено") : UiText.T("Сохранено · подключено");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or InvalidDataException)
        { if (!_shutdown) _status.Text = UiText.F("Локально · {0}", ex.Message); }
        finally { _busy = false; }
    }
    internal async Task CreateCloudAsync(string server)
    {
        if (_busy) throw new InvalidOperationException(UiText.T("Дождитесь текущего сохранения."));
        FinishInput(); _busy = true;
        try
        {
            var local = Snapshot();
            var created = await _cloud.CreateAsync(server, local);
            if (_shutdown) return;
            _generation++; _connection = new(CanvasCloudClient.Server(server), created.Board.Id, created.EditKey);
            _viewKey = created.ViewKey; _baseline = created.Board;
            _board = local with { Id = created.Board.Id, Revision = 0, ReadOnly = false };
            _pending = null; _pendingBefore = null; SaveState();
        }
        finally { _busy = false; }
        await SyncAsync();
    }
    internal async Task ConnectAsync(string link)
    {
        if (_busy) throw new InvalidOperationException(UiText.T("Дождитесь текущего сохранения."));
        FinishInput(); _busy = true;
        try
        {
            var connection = CanvasCloudClient.ParseLink(link);
            var remote = await _cloud.ReadAsync(connection);
            if (_shutdown) return;
            // Preserve the outgoing local board before opening a different shared board.
            var backup = _statePath + $".backup-{DateTime.Now:yyyyMMdd-HHmmss}.json";
            if (File.Exists(_statePath)) File.Copy(_statePath, backup, true);
            _generation++; _connection = connection; _viewKey = null;
            _pending = null; _pendingBefore = null; _deferred = null; _baseline = remote;
            Apply(remote, true); SaveState(); _surface.Fit();
            _status.Text = remote.ReadOnly ? UiText.T("Только просмотр · подключено") : UiText.T("Сохранено · подключено");
        }
        finally { _busy = false; }
    }
    private Window Dialog(string title, StackPanel panel)
    {
        var dialog = new Window { Title = title, Width = 500, SizeToContent = SizeToContent.Height,
            CanResize = false, WindowStartupLocation = WindowStartupLocation.CenterOwner,
            RequestedThemeVariant = ThemeVariant.Light, Content = panel, Topmost = true };
        dialog.Show(this); return dialog;
    }
    private void ShowConnection()
    {
        var panel = new StackPanel { Margin = new Thickness(18), Spacing = 10 };
        panel.Children.Add(new TextBlock { Text = UiText.T("Открыть общее полотно"), FontSize = 18 });
        var link = new TextBox { PlaceholderText = UiText.T("Код приглашения или ссылка") };
        panel.Children.Add(link);
        var error = new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DarkRed };
        var open = new Button { Content = UiText.T("Подключиться") }; panel.Children.Add(open);
        panel.Children.Add(new TextBlock { Text = UiText.T("Чтобы пригласить других в текущее полотно, нажмите «Совместная работа»."), TextWrapping = TextWrapping.Wrap });
        panel.Children.Add(error);
        var dialog = Dialog(UiText.T("Подключить полотно"), panel);
        open.Click += async (_, _) => { open.IsEnabled = false; try { await ConnectAsync(link.Text ?? ""); dialog.Close(); } catch (Exception ex) { error.Text = ex.Message; } finally { open.IsEnabled = true; } };
    }
    internal async Task ShareAsync(string? testServer = null)
    {
        if (_busy) { _status.Text = UiText.T("Дождитесь текущего сохранения."); return; }
        try
        {
            if (_connection is null) await CreateCloudAsync(Program.SmokeTest && testServer is not null ? testServer : CanvasCloudClient.DefaultServer);
            if (_shutdown || _connection is null) return;
            await SyncAsync();
            var panel = new StackPanel { Margin = new Thickness(18), Spacing = 9 };
            panel.Children.Add(new TextBlock { Text = UiText.T("Поделиться полотном"), FontSize = 18 });
            panel.Children.Add(new TextBlock { Text = UiText.T("Отправьте приглашение. Получатель откроет в BlurMask «Полотно → Меню → Подключить» и вставит код или ссылку."), TextWrapping = TextWrapping.Wrap });
            AddLink(panel, ReadOnly ? UiText.T("Только просмотр") : UiText.T("Рисовать вместе"), CanvasCloudClient.Link(_connection));
            if (!ReadOnly && _viewKey is not null) AddLink(panel, UiText.T("Только просмотр"), CanvasCloudClient.Link(_connection with { Key = _viewKey }));
            Dialog(UiText.T("Ссылка на полотно"), panel);
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or InvalidOperationException or TaskCanceledException or JsonException)
        { _status.Text = ex.Message; _session.Toolbar.ShowMessage(UiText.T("Совместная работа"), ex.Message); }
    }
    private void AddLink(StackPanel panel, string label, string link)
    {
        panel.Children.Add(new TextBlock { Text = label, FontSize = 12 });
        var code = link[(link.IndexOf('#') + 1)..];
        panel.Children.Add(new TextBox { Text = code, IsReadOnly = true });
        var copy = new Button { Content = UiText.T("Копировать код") };
        copy.Click += async (_, _) => { if (Clipboard is not null) { await Clipboard.SetTextAsync(code); copy.Content = UiText.T("Скопировано"); } };
        panel.Children.Add(copy);
        var copyLink = new Button { Content = UiText.T("Копировать ссылку") };
        copyLink.Click += async (_, _) => { if (Clipboard is not null) { await Clipboard.SetTextAsync(link); copyLink.Content = UiText.T("Скопировано"); } };
        panel.Children.Add(copyLink);
    }
    private void SavePng()
    {
        FinishInput();
        try
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "BlurMask");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, $"Canvas-{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.png");
            ExportPng(path); _status.Text = UiText.F("PNG сохранён: {0}", path);
        }
        catch (IOException ex) { _status.Text = ex.Message; }
    }
    internal void ExportPng(string path)
    {
        var bounds = CanvasGeometry.ForMarks(BoardWidth, BoardHeight, _session.CanvasDocument.Marks);
        var (size, scale) = CanvasGeometry.ExportSize(bounds);
        var export = new BoardRenderControl(Snapshot(), bounds, scale) { Width = size.Width, Height = size.Height };
        export.Measure(new Size(size.Width, size.Height)); export.Arrange(new Rect(0, 0, size.Width, size.Height));
        using var image = new RenderTargetBitmap(size, new Vector(96, 96));
        image.Render(export); image.Save(path, PngBitmapEncoderOptions.Default);
    }
    private sealed class BoardRenderControl(BoardSnapshot board, Rect worldBounds, double scale) : Control
    {
        public override void Render(DrawingContext context)
        {
            context.DrawRectangle(board.Background == "black" ? Brushes.Black : Brushes.White, null, new Rect(Bounds.Size));
            var document = new InkDocument(); document.Import(board.Marks);
            using var transform = context.PushTransform(Matrix.CreateTranslation(-worldBounds.X, -worldBounds.Y) * Matrix.CreateScale(scale, scale));
            foreach (var mark in document.Marks) mark.Draw(context);
        }
    }
}
