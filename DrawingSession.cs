using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System.Diagnostics;
using Avalonia.Input;

namespace BlurMask;

internal enum DrawingTarget { Screen, Canvas }

internal sealed class DrawingSession : IDisposable
{
    private readonly List<DesktopInkWindow> _overlays = [];
    private readonly Action _newMask;
    private readonly Action _exit;
    private readonly Action<PrivacyMode> _selectMask;
    private readonly Action _closeMask;
    private readonly Action<bool>? _setMaskAttachment;
    private DesktopInkNative? _hotkeys;
    private SettingsWindow? _settingsWindow;
    private Color _color = Color.Parse("#FFF400"), _previousColor = Colors.Black;
    internal static readonly string[] QuickColors = ["#00C9ED", "#FFF400", "#FF0099", "#111111", "#FFFFFF", "#FF4D4D"];
    internal AppSettings Settings { get; private set; } = Program.Settings.CloneSettings();
    internal IReadOnlyList<ShortcutAction> UnavailableHotkeys => _hotkeys?.Failed ?? [];
    internal SettingsWindow? OpenSettingsWindow => _settingsWindow;
    private InkTool _toolBeforeHide = InkTool.Cursor;
    private bool _disposed;
    private bool _capturing;
    private bool _inTray, _trayInkVisible;
    private readonly DispatcherTimer _fadeTimer = new() { Interval = TimeSpan.FromMilliseconds(33) };
    private readonly DispatcherTimer _screenTimer = new() { Interval = TimeSpan.FromMilliseconds(300) };
    private (bool Undo, bool Redo) _historyState;
    public InkDocument DesktopDocument { get; } = new();
    public InkDocument CanvasDocument { get; } = new();
    internal InkSelection DesktopSelection { get; }
    internal InkSelection CanvasSelection { get; }
    public InkDocument Document => Target == DrawingTarget.Screen ? DesktopDocument : CanvasDocument;
    public DrawingTarget Target { get; private set; }
    public CanvasWindow? Canvas { get; private set; }
    public InkTool Tool { get; private set; } = InkTool.Cursor;
    public Color Color { get => _color; set { if (_color != value) { _previousColor = _color; _color = value; } } }
    public double PenSize { get; set; } = 4;
    public double TextSize { get; set; } = 28;
    public double FadeSeconds { get; private set; }
    private double _previousFadeSeconds = 10;
    public BoardMode Board { get; private set; }
    public bool InkVisible { get; private set; } = true;
    public bool AttachMasksToWindows { get; private set; }
    public DrawingToolbarWindow Toolbar { get; }
    internal IReadOnlyList<DesktopInkWindow> Overlays => _overlays;
    public event Action? StateChanged;

    public DrawingSession(Action newMask, Action exit, Action<PrivacyMode> selectMask, Action closeMask, Action<bool>? setMaskAttachment = null)
    {
        DesktopSelection = new(DesktopDocument);
        CanvasSelection = new(CanvasDocument);
        _newMask = newMask;
        _exit = exit;
        _selectMask = selectMask;
        _closeMask = closeMask;
        _setMaskAttachment = setMaskAttachment;
        _screenTimer.Tick += (_, _) => { _screenTimer.Stop(); RebuildOverlays(); };
        Toolbar = new DrawingToolbarWindow(this);
        DesktopDocument.Changed += RefreshDesktopDocument;
        CanvasDocument.Changed += RefreshCanvasDocument;
        _fadeTimer.Tick += (_, _) =>
        {
            var previousDesktop = DesktopDocument.FadingMarks.Count;
            var previousCanvas = CanvasDocument.FadingMarks.Count;
            var now = Stopwatch.GetTimestamp();
            DesktopDocument.ExpireFading(now);
            CanvasDocument.ExpireFading(now);
            if (previousDesktop > 0) foreach (var overlay in _overlays) overlay.Refresh();
            if (previousCanvas > 0) Canvas?.Refresh();
            if (previousDesktop != DesktopDocument.FadingMarks.Count || previousCanvas != CanvasDocument.FadingMarks.Count) RefreshHistory();
            if (DesktopDocument.FadingMarks.Count == 0 && CanvasDocument.FadingMarks.Count == 0)
                _fadeTimer.Stop();
        };
        Toolbar.Screens.Changed += OnScreensChanged;
        Toolbar.Opened += (_, _) =>
        {
            if (_overlays.Count == 0) RebuildOverlays();
            _hotkeys ??= DesktopInkNative.TryRegisterHotkeys(Toolbar, HandleShortcut, Settings);
            Toolbar.RaiseAboveInk();
        };
        Toolbar.Closed += (_, _) => Dispose();
    }

    public void Show()
    {
        if (_inTray)
        {
            _inTray = false;
            if (_trayInkVisible && !InkVisible) ToggleInk();
        }
        if (!Toolbar.IsVisible) Toolbar.Show();
        else Toolbar.Activate();
    }

    public void MinimizeToTray()
    {
        if (_inTray) return;
        FinishInput(); Toolbar.CloseFlyout();
        _trayInkVisible = InkVisible; _inTray = true;
        if (InkVisible) ToggleInk();
        Canvas?.Hide(); Toolbar.Hide();
    }
    internal void ToggleToolbarVisibility()
    {
        if (Toolbar.IsVisible) { Toolbar.CloseFlyout(); Toolbar.Hide(); }
        else Show();
    }

    private void OnScreensChanged(object? sender, EventArgs e)
    {
        // RDP reconnects can report several intermediate/empty screen layouts.
        _screenTimer.Stop(); _screenTimer.Start();
    }
    private void RebuildOverlays()
    {
        if (_disposed) return;
        if (Toolbar.Screens.All.Count == 0) return;
        foreach (var overlay in _overlays) { overlay.FinishText(); overlay.Close(); }
        _overlays.Clear();
        foreach (var screen in Toolbar.Screens.All)
        {
            var overlay = new DesktopInkWindow(this, screen);
            _overlays.Add(overlay);
            overlay.Show();
        }
        // The controls must always sit above the ink input layer.
        Toolbar.RaiseAboveInk();
        Toolbar.ClampToScreen();
    }

    public void SelectTool(InkTool tool)
    {
        Toolbar.CloseFlyout();
        foreach (var overlay in _overlays) { overlay.FinishText(); overlay.CancelGesture(); }
        Canvas?.FinishInput();
        Tool = tool;
        if (tool != InkTool.Select) { DesktopSelection.Clear(); CanvasSelection.Clear(); }
        if (tool != InkTool.Cursor) InkVisible = true;
        UpdateState();
    }
    public void ToggleInk()
    {
        foreach (var overlay in _overlays) { overlay.FinishText(); overlay.CancelGesture(); }
        Canvas?.FinishInput();
        if (InkVisible) _toolBeforeHide = Tool;
        InkVisible = !InkVisible;
        Tool = InkVisible ? _toolBeforeHide : InkTool.Cursor;
        UpdateState();
    }
    public void SetBoard(BoardMode board) { Board = board; UpdateState(); }
    public void NotifySettings() => StateChanged?.Invoke();
    public void SetFading(double seconds)
    {
        FinishInput();
        FadeSeconds = seconds;
        if (seconds > 0) _previousFadeSeconds = seconds;
        NotifySettings();
    }
    public void NewMask() { SelectScreen(); SelectTool(InkTool.Cursor); _newMask(); }
    public void CreateMask(PrivacyMode mode) { SelectScreen(); SelectTool(InkTool.Cursor); _selectMask(mode); }
    public void CloseAllMasks() => _closeMask();
    public void SetMaskWindowAttachment(bool enabled)
    {
        AttachMasksToWindows = enabled; _setMaskAttachment?.Invoke(enabled); NotifySettings();
    }
    public void OpenCanvas()
    {
        FinishInput();
        if (Canvas is null) { Canvas = new CanvasWindow(this); Color = Colors.Black; }
        Target = DrawingTarget.Canvas;
        InkVisible = true;
        if (Tool == InkTool.Cursor) Tool = InkTool.Pen;
        Canvas.Show();
        Canvas.Activate();
        UpdateState();
    }
    public void SelectScreen()
    {
        FinishInput();
        Target = DrawingTarget.Screen;
        UpdateState();
    }
    public void ActivateCanvas()
    {
        if (Target == DrawingTarget.Canvas) return;
        FinishInput();
        Target = DrawingTarget.Canvas;
        UpdateState();
    }
    public void Exit() => _exit();
    public void Undo() { if (Target == DrawingTarget.Canvas && Canvas?.ReadOnly == true) return; FinishInput(); Document.Undo(); }
    public void Redo() { if (Target == DrawingTarget.Canvas && Canvas?.ReadOnly == true) return; FinishInput(); Document.Redo(); }
    public void Clear() { if (Target == DrawingTarget.Canvas && Canvas?.ReadOnly == true) return; FinishInput(); Document.Clear(); }
    private void FinishInput()
    {
        foreach (var overlay in _overlays) { overlay.FinishText(); overlay.CancelGesture(); }
        Canvas?.FinishInput();
    }
    private void UpdateState()
    {
        foreach (var overlay in _overlays) overlay.ApplyState();
        Canvas?.Refresh();
        Toolbar.RaiseAboveInk();
        _historyState = (Document.CanUndo, Document.CanRedo);
        StateChanged?.Invoke();
    }
    private void RefreshDesktopDocument()
    {
        if (DesktopDocument.FadingMarks.Count > 0) _fadeTimer.Start();
        foreach (var overlay in _overlays) overlay.Refresh();
        RefreshHistory();
    }
    private void RefreshCanvasDocument()
    {
        if (CanvasDocument.FadingMarks.Count > 0) _fadeTimer.Start();
        Canvas?.Refresh(); RefreshHistory();
    }
    private void RefreshHistory()
    {
        var state = (Document.CanUndo, Document.CanRedo);
        if (_historyState == state) return;
        _historyState = state; StateChanged?.Invoke();
    }

    internal string ShortcutText(ShortcutAction action) => Settings.Shortcuts[action].ToString();
    internal bool HandleLocalShortcut(KeyEventArgs e)
    {
        if (e.Handled || _settingsWindow is not null || !Settings.HotkeysEnabled) return false;
        foreach (var (action, binding) in Settings.Shortcuts)
        {
            if (!binding.Enabled || binding.Key != e.Key || binding.Modifiers != e.KeyModifiers) continue;
            // A registered Windows hotkey arrives as WM_HOTKEY, not WM_KEYDOWN.
            // Only use the local route for unregistered combinations and other OSes.
            if (_hotkeys?.Registered(action) == true) return false;
            HandleShortcut(action); e.Handled = true; return true;
        }
        return false;
    }
    internal void HandleShortcut(ShortcutAction action)
    {
        if (_settingsWindow is not null) return;
        switch (action)
        {
            case ShortcutAction.Cursor: SelectTool(InkTool.Cursor); break;
            case ShortcutAction.Pen: SelectTool(InkTool.Pen); break;
            case ShortcutAction.Highlighter: SelectTool(InkTool.Highlighter); break;
            case ShortcutAction.Eraser: SelectTool(InkTool.Eraser); break;
            case ShortcutAction.Line: SelectTool(InkTool.Line); break;
            case ShortcutAction.Arrow: SelectTool(InkTool.Arrow); break;
            case ShortcutAction.Ellipse: SelectTool(InkTool.Ellipse); break;
            case ShortcutAction.Rectangle: SelectTool(InkTool.Rectangle); break;
            case ShortcutAction.Text: SelectTool(InkTool.Text); break;
            case ShortcutAction.Undo: Undo(); break;
            case ShortcutAction.Redo: Redo(); break;
            case ShortcutAction.Clear: Clear(); break;
            case ShortcutAction.ToggleInk: ToggleInk(); break;
            case ShortcutAction.Screenshot: _ = CaptureScreenshotAsync(); break;
            case ShortcutAction.Canvas: OpenCanvas(); break;
            case ShortcutAction.Screen: SelectScreen(); break;
            case ShortcutAction.IncreaseSize: PenSize = Math.Min(18, PenSize + 1); NotifySettings(); break;
            case ShortcutAction.DecreaseSize: PenSize = Math.Max(1, PenSize - 1); NotifySettings(); break;
            case ShortcutAction.Whiteboard: SelectScreen(); SetBoard(Board == BoardMode.White ? BoardMode.Desktop : BoardMode.White); break;
            case ShortcutAction.Blackboard: SelectScreen(); SetBoard(Board == BoardMode.Black ? BoardMode.Desktop : BoardMode.Black); break;
            case ShortcutAction.LastColor: Color = _previousColor; NotifySettings(); break;
            case ShortcutAction.ToggleToolbar: ToggleToolbarVisibility(); break;
            case ShortcutAction.ToggleFading: SetFading(FadeSeconds > 0 ? 0 : _previousFadeSeconds); break;
            case >= ShortcutAction.QuickColor1 and <= ShortcutAction.QuickColor6:
                Color = Color.Parse(QuickColors[(int)action - (int)ShortcutAction.QuickColor1]); NotifySettings(); break;
        }
    }
    internal void ShowSettings()
    {
        if (_settingsWindow is not null) { _settingsWindow.Activate(); return; }
        FinishInput(); Toolbar.CloseFlyout(); SelectTool(InkTool.Cursor);
        var unavailable = UnavailableHotkeys.ToArray();
        _hotkeys?.Dispose(); _hotkeys = null;
        _settingsWindow = new SettingsWindow(this, unavailable);
        _settingsWindow.Closed += (_, _) =>
        {
            _settingsWindow = null;
            if (!_disposed) _hotkeys = DesktopInkNative.TryRegisterHotkeys(Toolbar, HandleShortcut, Settings);
        };
        _settingsWindow.Show(Toolbar);
    }
    internal void SaveSettings(AppSettings settings)
    {
        if (!Program.SmokeTest) SettingsStore.Save(settings);
        Settings = settings.CloneSettings();
        if (!Program.SmokeTest) Program.Settings = settings.CloneSettings();
        if (_settingsWindow is null)
        { _hotkeys?.Dispose(); _hotkeys = DesktopInkNative.TryRegisterHotkeys(Toolbar, HandleShortcut, Settings); }
        NotifySettings();
    }

    public async Task CaptureScreenshotAsync()
    {
        if (_capturing) return;
        _capturing = true;
        FinishInput();
        Toolbar.CloseFlyout();
        Toolbar.Hide();
        try
        {
            // Give DWM time to remove the toolbar before capturing the annotated desktop.
            await Task.Delay(180);
            if (_disposed) return;
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "BlurMask");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, $"BlurMask-{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.png");
            DesktopInkNative.SaveScreenshot(path);
            Toolbar.Show();
            Toolbar.ShowMessage(UiText.T("Снимок сохранён"), path);
        }
        catch (Exception ex)
        {
            Toolbar.Show();
            Toolbar.ShowMessage(UiText.T("Не удалось сохранить снимок"), ex.Message);
        }
        finally
        {
            _capturing = false;
            if (!_disposed) Toolbar.RaiseAboveInk();
        }
    }

    internal void SmokeTest()
    {
        DrawingSmokeTest.VerifyDocument(Document);
        var retained = new InkMark(InkTool.Pen, Color, PenSize, new Point(100, 100));
        DesktopDocument.Add(retained);
        RebuildOverlays();
        if (!DesktopDocument.Marks.Contains(retained)) throw new InvalidOperationException("Screen rebuild discarded desktop ink.");
        DesktopDocument.Clear();
        SelectTool(InkTool.Pen);
        SetBoard(BoardMode.White);
        Toolbar.ShowTools();
        SetBoard(BoardMode.Black);
        Toolbar.ShowPalette();
        SetBoard(BoardMode.Desktop);
        ToggleInk();
        ToggleInk();
        SelectTool(InkTool.Cursor);
        Toolbar.CloseFlyout();
        if (OperatingSystem.IsWindows())
            foreach (var overlay in _overlays) DesktopInkNative.VerifyClickThrough(overlay);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _fadeTimer.Stop();
        _screenTimer.Stop();
        _hotkeys?.Dispose();
        _settingsWindow?.Close();
        Toolbar.Screens.Changed -= OnScreensChanged;
        DesktopDocument.Changed -= RefreshDesktopDocument;
        CanvasDocument.Changed -= RefreshCanvasDocument;
        Canvas?.Shutdown();
        Toolbar.CloseFlyout();
        foreach (var overlay in _overlays) overlay.Close();
        _overlays.Clear();
        Toolbar.Close();
    }
}
