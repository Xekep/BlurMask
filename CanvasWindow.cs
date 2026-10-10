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
    private readonly CanvasCloudClient _cloud = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(1300) };
    private readonly string _statePath;
    private BoardSnapshot _board = new();
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
        Title = "BlurMask — Полотно";
        Width = 1100; Height = 760; MinWidth = 650; MinHeight = 450;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        RequestedThemeVariant = ThemeVariant.Light;
        Background = new SolidColorBrush(Color.Parse("#F0F3F5"));
        var storage = Program.SmokeTest
            ? Path.Combine(Environment.GetEnvironmentVariable("BLURMASK_SMOKE_ARTIFACTS") ?? Path.GetTempPath(), "canvas-smoke")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BlurMask");
        _statePath = Path.Combine(storage, "canvas.json");
        if (!Program.SmokeTest) LoadState();
        var dock = new DockPanel();
        var top = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(12, 8), ItemSpacing = 7, LineSpacing = 7 };
        _title = new TextBox { Text = _board.Title, Width = 220, MaxLength = 120, IsReadOnly = ReadOnly };
        _title.LostFocus += (_, _) =>
        {
            if (ReadOnly) return;
            _board = _board with { Title = string.IsNullOrWhiteSpace(_title.Text) ? "Новое полотно" : _title.Text.Trim() };
            SaveState();
        };
        top.Children.Add(_title);
        AddButton(top, "Новое", NewLocal);
        AddButton(top, "Экран", session.SelectScreen);
        AddButton(top, "Перо", () => { session.ActivateCanvas(); session.SelectTool(InkTool.Pen); });
        AddButton(top, "Ластик", () => { session.ActivateCanvas(); session.SelectTool(InkTool.Eraser); });
        AddButton(top, "↶", session.Undo);
        AddButton(top, "↷", session.Redo);
        AddButton(top, "Вместить", () => _surface?.Fit());
        AddButton(top, "Фон", () =>
        {
            if (ReadOnly) return;
            _board = _board with { Background = _board.Background == "white" ? "black" : "white" };
            if (BoardBackground == "black" && session.Color == Colors.Black) session.Color = Colors.White;
            SaveState(); Refresh();
        });
        AddButton(top, "Подключить", ShowConnection);
        AddButton(top, "Поделиться", () => _ = ShareAsync());
        AddButton(top, "PNG", SavePng);
        var header = new Border { Background = Brushes.White, BorderBrush = new SolidColorBrush(Color.Parse("#D8DFE4")),
            BorderThickness = new Thickness(0, 0, 0, 1), Child = top };
        DockPanel.SetDock(header, Dock.Top); dock.Children.Add(header);
        var footer = new DockPanel { Margin = new Thickness(12, 5) };
        _status = new TextBlock { Text = _connection is null ? "Сохранено на устройстве" : "Подключение…", FontSize = 12 };
        footer.Children.Add(_status);
        footer.Children.Add(new TextBlock { Text = "Ctrl+Shift+5 — полотно · Ctrl+Shift+6 — экран · колесо — масштаб",
            FontSize = 11, Foreground = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Right });
        DockPanel.SetDock(footer, Dock.Bottom); dock.Children.Add(footer);
        _surface = new CanvasInkSurface(session, this);
        dock.Children.Add(_surface); Content = dock;
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

    private static void AddButton(Panel panel, string text, Action action)
    {
        var button = new Button { Content = text, Padding = new Thickness(9, 6), FontSize = 12 };
        button.Click += (_, _) => action(); panel.Children.Add(button);
    }
    private void OnDocumentChanged() { if (!_loading) _dirty = true; }
    private void NewLocal()
    {
        if (_busy) { _status.Text = "Дождитесь текущего сохранения."; return; }
        FinishInput(); SaveState();
        try { if (File.Exists(_statePath)) File.Copy(_statePath, _statePath + $".backup-{DateTime.Now:yyyyMMdd-HHmmss-fff}.json", true); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { _status.Text = "Не удалось сохранить резервную копию: " + ex.Message; return; }
        _generation++; _connection = null; _pending = null; _pendingBefore = null; _viewKey = null; _deferred = null;
        _baseline = new(); Apply(new(), true); SaveState(); _surface.Fit();
        _status.Text = "Сохранено на устройстве";
    }
    public void Refresh() { _surface.InvalidateVisual(); _title.IsReadOnly = ReadOnly; }
    public void FinishInput() => _surface.FinishInput();
    public void Shutdown() { _shutdown = true; FinishInput(); SaveState(); _timer.Stop(); Close(); }
    internal BoardSnapshot Snapshot() => _board with { Marks = _session.CanvasDocument.Export() };
    private void Apply(BoardSnapshot board, bool resetHistory = false)
    {
        if (!BoardValidation.Valid(board)) throw new InvalidDataException("Сервер вернул некорректное полотно.");
        _loading = true;
        try { _board = board; _session.CanvasDocument.Import(board.Marks, resetHistory); if (!_title.IsFocused) _title.Text = board.Title; Refresh(); }
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
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { _status.Text = "Не удалось сохранить на устройстве."; }
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
            _status.Text = remote.ReadOnly ? "Только просмотр · подключено" : "Сохранено · подключено";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or InvalidDataException)
        { if (!_shutdown) _status.Text = $"Локально · {ex.Message}"; }
        finally { _busy = false; }
    }
    internal async Task CreateCloudAsync(string server)
    {
        if (_busy) throw new InvalidOperationException("Дождитесь текущего сохранения.");
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
        if (_busy) throw new InvalidOperationException("Дождитесь текущего сохранения.");
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
            _status.Text = remote.ReadOnly ? "Только просмотр · подключено" : "Сохранено · подключено";
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
        panel.Children.Add(new TextBlock { Text = "Открыть общее полотно", FontSize = 18 });
        var link = new TextBox { PlaceholderText = "Код приглашения или ссылка" };
        panel.Children.Add(link);
        var error = new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DarkRed };
        var open = new Button { Content = "Подключиться" }; panel.Children.Add(open);
        panel.Children.Add(new TextBlock { Text = "Или сохранить текущее полотно на сервере", Margin = new Thickness(0, 12, 0, 0) });
        var create = new Button { Content = "Создать облачную копию" }; panel.Children.Add(create); panel.Children.Add(error);
        var dialog = Dialog("Подключить полотно", panel);
        open.Click += async (_, _) => { open.IsEnabled = false; try { await ConnectAsync(link.Text ?? ""); dialog.Close(); } catch (Exception ex) { error.Text = ex.Message; } finally { open.IsEnabled = true; } };
        create.Click += async (_, _) => { create.IsEnabled = false; try { await CreateCloudAsync(CanvasCloudClient.DefaultServer); dialog.Close(); } catch (Exception ex) { error.Text = ex.Message; } finally { create.IsEnabled = true; } };
    }
    private async Task ShareAsync()
    {
        if (_connection is null) { ShowConnection(); return; }
        await SyncAsync();
        var panel = new StackPanel { Margin = new Thickness(18), Spacing = 9 };
        panel.Children.Add(new TextBlock { Text = "Поделиться полотном", FontSize = 18 });
        panel.Children.Add(new TextBlock { Text = "Отправьте код приглашения. Получатель вставит его в «Подключить». Можно также скопировать ссылку для браузера.", TextWrapping = TextWrapping.Wrap });
        AddLink(panel, ReadOnly ? "Только просмотр" : "Можно рисовать", CanvasCloudClient.Link(_connection));
        if (!ReadOnly && _viewKey is not null) AddLink(panel, "Только просмотр", CanvasCloudClient.Link(_connection with { Key = _viewKey }));
        Dialog("Ссылка на полотно", panel);
    }
    private void AddLink(StackPanel panel, string label, string link)
    {
        panel.Children.Add(new TextBlock { Text = label, FontSize = 12 });
        var code = link[(link.IndexOf('#') + 1)..];
        panel.Children.Add(new TextBox { Text = code, IsReadOnly = true });
        var copy = new Button { Content = "Копировать код" };
        copy.Click += async (_, _) => { if (Clipboard is not null) { await Clipboard.SetTextAsync(code); copy.Content = "Скопировано"; } };
        panel.Children.Add(copy);
        var copyLink = new Button { Content = "Копировать ссылку" };
        copyLink.Click += async (_, _) => { if (Clipboard is not null) { await Clipboard.SetTextAsync(link); copyLink.Content = "Скопировано"; } };
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
            ExportPng(path); _status.Text = $"PNG сохранён: {path}";
        }
        catch (IOException ex) { _status.Text = ex.Message; }
    }
    internal void ExportPng(string path)
    {
        var export = new BoardRenderControl(Snapshot()) { Width = BoardWidth, Height = BoardHeight };
        export.Measure(new Size(BoardWidth, BoardHeight)); export.Arrange(new Rect(0, 0, BoardWidth, BoardHeight));
        using var image = new RenderTargetBitmap(new PixelSize(BoardWidth, BoardHeight), new Vector(96, 96));
        image.Render(export); image.Save(path, PngBitmapEncoderOptions.Default);
    }
    private sealed class BoardRenderControl(BoardSnapshot board) : Control
    {
        public override void Render(DrawingContext context)
        {
            context.DrawRectangle(board.Background == "black" ? Brushes.Black : Brushes.White, null, new Rect(Bounds.Size));
            var document = new InkDocument(); document.Import(board.Marks);
            foreach (var mark in document.Marks) mark.Draw(context);
        }
    }
}
