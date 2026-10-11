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
    private readonly Func<Point, double, int>? _eraseMasks;
    private readonly Action<TrackedWindow>? _fillWindowRegion;
    private WindowFillPicker? _windowFillPicker;
    internal TrackedWindow? WindowFillPreview => _windowFillPicker?.Region;
    internal bool PickingMaskRegion { get; private set; }
    private DesktopInkNative? _hotkeys;
    private SettingsWindow? _settingsWindow;
    private Color _color = Color.Parse("#FFF400"), _previousColor = Colors.Black;
    internal static readonly string[] QuickColors = ["#00C9ED", "#FFF400", "#FF0099", "#111111", "#FFFFFF", "#FF4D4D"];
    internal AppSettings Settings { get; private set; } = Program.Settings.CloneSettings();
    internal IReadOnlyList<ShortcutAction> UnavailableHotkeys => _hotkeys?.Failed ?? [];
    internal SettingsWindow? OpenSettingsWindow => _settingsWindow;
    private InkTool _toolBeforeHide = InkTool.Cursor;
    private bool _disposed;
    private CancellationTokenSource? _mathSolve;
    private bool _capturing;
    private ScreenRegionSelection? _screenshotSelection;
    private TaskCompletionSource<PixelRect?>? _screenshotResult;
    internal bool PickingScreenshot => _screenshotSelection is not null;
    internal PixelRect? ScreenshotPreview => _screenshotSelection?.Region;
    private bool _inTray, _trayInkVisible, _trayCanvasVisible, _traySettingsVisible;
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
    internal InkTool LastDrawingTool { get; private set; } = InkTool.Pen;
    internal InkTool LastShapeTool { get; private set; } = InkTool.Line;
    internal InkTool LastCursorTool { get; private set; } = InkTool.Cursor;
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
    private readonly AiPenRecognizer _desktopAi, _canvasAi;
    internal void RecognizeAiInk() { FinishInput(); _desktopAi.Flush(); _canvasAi.Flush(); }

    internal bool CanSolveSelectedMath =>
        !(Target == DrawingTarget.Canvas && Canvas?.ReadOnly == true) &&
        Document.Marks.Any(m => (Target == DrawingTarget.Canvas ? CanvasSelection : DesktopSelection).SelectedIds.Contains(m.Id) &&
            m.Tool == InkTool.Text && (m.Font == "STIX Two Math" || AiAutomaticLayout.Mathematical(m.Text)));

    private void ShowMathCalculationFailure()
    {
        if (_disposed) return;
        Toolbar.ShowMessage(UiText.T("Не удалось вычислить"), UiText.T(AiMathSolver.Executable is null
            ? "Проверьте распознанную формулу. Для сложных вычислений подключите Giac в настройках."
            : "Проверьте распознанную формулу: эта запись не поддерживается или результат не найден."));
    }

    internal async Task SolveSelectedMathAsync()
    {
        FinishInput();
        if (!CanSolveSelectedMath) return;
        var document = Document;
        var ids = (Target == DrawingTarget.Canvas ? CanvasSelection : DesktopSelection).SelectedIds.ToHashSet();
        var sources = document.Marks.Where(m => ids.Contains(m.Id) && m.Tool == InkTool.Text &&
            (m.Font == "STIX Two Math" || AiAutomaticLayout.Mathematical(m.Text))).Take(16).ToArray();
        _mathSolve?.Cancel();
        using var cancellation = new CancellationTokenSource(); _mathSolve = cancellation;
        bool failureShown = false;
        try
        {
            foreach (var original in sources)
            {
                var text = original.Text; var request = text.TrimEnd().TrimEnd('=').TrimEnd();
                var solution = await Task.Run(() => AiMathSolver.Calculate(request, cancellation.Token), cancellation.Token);
                if (!_disposed && !cancellation.IsCancellationRequested && original.Text == text && solution is null && !failureShown)
                {
                    ShowMathCalculationFailure(); failureShown = true;
                }
                if (_disposed || cancellation.IsCancellationRequested || original.Text != text || solution is null ||
                    !document.Marks.Contains(original) || document.HasActiveEdit || ReferenceEquals(document, CanvasDocument) && Canvas?.ReadOnly == true) continue;
                var calculated = request + (solution.IsEquation ? "\\Rightarrow " : "=") + solution.LaTex;
                if (!MathTypesetting.Supported(calculated)) continue;
                var bounds = original.BaseBounds;
                var mark = new InkMark(InkTool.Text, original.Color, original.Width, bounds.TopLeft)
                { Id = original.Id, Text = calculated, Font = "STIX Two Math", FontSize = original.FontSize, Transform = original.Transform, FadeSeconds = original.FadeSeconds };
                mark.AddPoint(bounds.BottomRight);
                AiTypesetLayout.Fit(mark, bounds, request, document.Marks);
                document.ReplaceRecognized([original], mark);
            }
        }
        catch (OperationCanceledException) { }
        finally { if (ReferenceEquals(_mathSolve, cancellation)) _mathSolve = null; }
    }

    public DrawingSession(Action newMask, Action exit, Action<PrivacyMode> selectMask, Action closeMask,
        Action<bool>? setMaskAttachment = null, Func<Point, double, int>? eraseMasks = null, Action<TrackedWindow>? fillWindowRegion = null)
    {
        DesktopSelection = new(DesktopDocument);
        CanvasSelection = new(CanvasDocument);
        _desktopAi = new(DesktopDocument, () => Settings.AiMode); _canvasAi = new(CanvasDocument, () => Settings.AiMode);
        _desktopAi.CalculationFailed += ShowMathCalculationFailure;
        _canvasAi.CalculationFailed += ShowMathCalculationFailure;
        _newMask = newMask;
        _exit = exit;
        _selectMask = selectMask;
        _closeMask = closeMask;
        _setMaskAttachment = setMaskAttachment;
        _eraseMasks = eraseMasks;
        _fillWindowRegion = fillWindowRegion;
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
            if (_trayCanvasVisible) Canvas?.Show();
            if (_traySettingsVisible) _settingsWindow?.Show();
        }
        if (!Toolbar.IsVisible) Toolbar.Show();
        else Toolbar.Activate();
    }

    public void MinimizeToTray()
    {
        if (_inTray) return;
        FinishInput(); Toolbar.CloseFlyout();
        _trayInkVisible = InkVisible; _inTray = true;
        _trayCanvasVisible = Canvas?.IsVisible == true;
        _traySettingsVisible = _settingsWindow?.IsVisible == true;
        if (InkVisible) ToggleInk();
        _settingsWindow?.Hide(); Canvas?.Hide(); Toolbar.Hide();
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
        if (_disposed || Program.UseNativeWayland) return;
        if (Toolbar.Screens.All.Count == 0) return;
        foreach (var overlay in _overlays) { overlay.FinishText(); overlay.Close(); }
        _overlays.Clear();
        foreach (var screen in Toolbar.Screens.All)
        {
            var overlay = new DesktopInkWindow(this, screen);
            _overlays.Add(overlay);
            // Apply visibility before mapping: cursor/canvas mode must never
            // briefly create a transparent, input-blocking Linux window.
            overlay.ApplyState();
        }
        // The controls must always sit above the ink input layer.
        Toolbar.RaiseAboveInk();
        Toolbar.ClampToScreen();
    }

    public void SelectTool(InkTool tool, bool remember = true)
    {
        CancelScreenshot();
        CancelWindowFill();
        if (Tool == InkTool.AiPen && tool != InkTool.AiPen) { _desktopAi.Flush(); _canvasAi.Flush(); }
        Toolbar.CloseFlyout();
        foreach (var overlay in _overlays) { overlay.FinishText(); overlay.CancelGesture(); }
        Canvas?.FinishInput();
        Tool = tool;
        if (remember)
            switch (tool)
            {
                case InkTool.Pen or InkTool.AiPen or InkTool.Highlighter or InkTool.Text: LastDrawingTool = tool; break;
                case InkTool.Line or InkTool.Arrow or InkTool.Rectangle or InkTool.Ellipse: LastShapeTool = tool; break;
                case InkTool.Cursor or InkTool.Select: LastCursorTool = tool; break;
            }
        if (tool != InkTool.Select) { DesktopSelection.Clear(); CanvasSelection.Clear(); }
        if (tool != InkTool.Cursor) InkVisible = true;
        UpdateState();
    }
    public void ToggleInk()
    {
        CancelScreenshot();
        CancelWindowFill();
        foreach (var overlay in _overlays) { overlay.FinishText(); overlay.CancelGesture(); }
        Canvas?.FinishInput();
        if (InkVisible && Target == DrawingTarget.Screen) _toolBeforeHide = Tool;
        InkVisible = !InkVisible;
        if (Target == DrawingTarget.Screen) Tool = InkVisible ? _toolBeforeHide : InkTool.Cursor;
        UpdateState();
    }
    public void SetBoard(BoardMode board) { Board = board; UpdateState(); }
    public void NotifySettings() => StateChanged?.Invoke();
    internal void SetAiMode(string mode)
    {
        RecognizeAiInk();
        SaveSettings(Settings with { AiMode = mode });
    }
    public void SetFading(double seconds)
    {
        FinishInput();
        FadeSeconds = seconds;
        if (seconds > 0) _previousFadeSeconds = seconds;
        NotifySettings();
    }
    public void NewMask() { if (Program.UseNativeWayland) return; SelectScreen(); SelectTool(InkTool.Cursor, remember: false); _newMask(); }
    public void CreateMask(PrivacyMode mode) { if (Program.UseNativeWayland) return; SelectScreen(); SelectTool(InkTool.Cursor, remember: false); _selectMask(mode); }
    public void CloseAllMasks() => _closeMask();
    internal int EraseMasks(Point point, double radius) => Tool == InkTool.Eraser && Target == DrawingTarget.Screen && InkVisible
        ? _eraseMasks?.Invoke(point, radius) ?? 0 : 0;
    public void SetMaskWindowAttachment(bool enabled)
    {
        AttachMasksToWindows = enabled; _setMaskAttachment?.Invoke(enabled); NotifySettings();
        if (!enabled && PickingMaskRegion) { CancelWindowFill(); UpdateState(); }
    }
    internal void BeginWindowFill()
    {
        if (!AttachMasksToWindows || Program.UseNativeWayland || _fillWindowRegion is null) return;
        SelectScreen(); SelectTool(InkTool.Cursor, remember: false);
        var catalog = NativeWindowCatalog.TryCreate();
        if (catalog is null) return;
        _windowFillPicker = new(catalog);
        PickingMaskRegion = true; InkVisible = true; UpdateState();
        Toolbar.Activate();
    }
    internal void UpdateWindowFillPreview(PixelPoint point)
    {
        if (!PickingMaskRegion || _windowFillPicker is null) return;
        var previous = _windowFillPicker.Region;
        if (_windowFillPicker.Move(point) != previous)
            foreach (var overlay in _overlays) overlay.Refresh();
    }
    internal void ClearWindowFillPreview()
    {
        if (_windowFillPicker?.Region is null) return;
        _windowFillPicker.Clear();
        foreach (var overlay in _overlays) overlay.Refresh();
    }
    private void CancelWindowFill()
    {
        PickingMaskRegion = false;
        _windowFillPicker?.Dispose(); _windowFillPicker = null;
        foreach (var overlay in _overlays) overlay.Refresh();
    }
    internal void CompleteWindowFill(PixelPoint point)
    {
        if (!PickingMaskRegion) return;
        var selected = _windowFillPicker?.Move(point, force: true);
        CancelWindowFill(); UpdateState();
        if (selected is not null) _fillWindowRegion?.Invoke(selected);
    }
    public void OpenCanvas() => OpenCanvas(null);
    internal void OpenCanvas(string? isolatedStatePath)
    {
        _desktopAi.Flush();
        FinishInput();
        if (Canvas is null) { Canvas = new CanvasWindow(this, isolatedStatePath); Color = Colors.Black; }
        Target = DrawingTarget.Canvas;
        InkVisible = true;
        if (Tool == InkTool.Cursor) Tool = LastDrawingTool;
        Canvas.Show();
        Canvas.Activate();
        UpdateState();
    }
    public void SelectScreen()
    {
        if (Program.UseNativeWayland) { Target = DrawingTarget.Canvas; UpdateState(); return; }
        _canvasAi.Flush();
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
        CancelScreenshot();
        CancelWindowFill();
        _mathSolve?.Cancel();
        foreach (var overlay in _overlays) { overlay.FinishText(); overlay.CancelGesture(); }
        Canvas?.FinishInput();
    }
    internal void BeginAiStroke(InkDocument document)
    {
        if (Tool == InkTool.AiPen) (ReferenceEquals(document, DesktopDocument) ? _desktopAi : _canvasAi).Pause();
    }
    internal void ResumeAiStroke(InkDocument document)
        => (ReferenceEquals(document, DesktopDocument) ? _desktopAi : _canvasAi).Resume();
    internal void CompleteStroke(InkDocument document)
    {
        var mark = document.Preview; document.CompletePreview();
        if (Tool == InkTool.AiPen && mark is not null)
            (ReferenceEquals(document, DesktopDocument) ? _desktopAi : _canvasAi).Add(mark);
    }
    private void UpdateState()
    {
        foreach (var overlay in _overlays) overlay.ApplyState();
        Canvas?.Refresh();
        Toolbar.RaiseAboveInk();
        _historyState = (Document.CanUndo, Document.CanRedo);
        StateChanged?.Invoke();
    }
    internal void UpdateOverlayRegions()
    {
        foreach (var overlay in _overlays) overlay.UpdateControlExclusion();
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
    internal bool HandleSelectionKey(KeyEventArgs e)
    {
        if (e.Handled || Tool != InkTool.Select || _settingsWindow is not null ||
            Target == DrawingTarget.Canvas && Canvas?.ReadOnly == true) return false;
        var delete = e.Key is Key.Delete or Key.Back && e.KeyModifiers is KeyModifiers.None or KeyModifiers.Shift;
        var all = e.Key == Key.A && (e.KeyModifiers == KeyModifiers.Control ||
            OperatingSystem.IsMacOS() && e.KeyModifiers == KeyModifiers.Meta);
        if (!delete && !all) return false;
        FinishInput();
        var selection = Target == DrawingTarget.Screen ? DesktopSelection : CanvasSelection;
        if (delete) selection.Delete(); else selection.SelectAll();
        e.Handled = true; return true;
    }
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
        FinishInput(); Toolbar.CloseFlyout(); SelectTool(InkTool.Cursor, remember: false);
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
        if (Settings.AiMode != settings.AiMode) RecognizeAiInk();
        if (!Program.SmokeTest) SettingsStore.Save(settings);
        Settings = settings.CloneSettings();
        if (!Program.SmokeTest) Program.Settings = settings.CloneSettings();
        if (_settingsWindow is null)
        { _hotkeys?.Dispose(); _hotkeys = DesktopInkNative.TryRegisterHotkeys(Toolbar, HandleShortcut, Settings); }
        NotifySettings();
    }

    public async Task CaptureScreenshotAsync()
    {
        if (Program.UseNativeWayland) return;
        if (_capturing) return;
        _capturing = true;
        FinishInput();
        var previousTarget = Target;
        Toolbar.CloseFlyout();
        Toolbar.Hide();
        try
        {
            Target = DrawingTarget.Screen;
            _screenshotResult = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _screenshotSelection = new();
            UpdateState();
            var active = _overlays.FirstOrDefault(w => w.IsVisible);
            active?.Activate();
            var region = await _screenshotResult.Task;
            _screenshotSelection = null; _screenshotResult = null;
            UpdateState();
            if (region is null || _disposed) return;
            // Remove selection shading/outline before capturing, keep annotated ink.
            await Task.Delay(180);
            if (_disposed) return;
            var path = Path.Combine(Path.GetTempPath(), $"BlurMask-capture-{Guid.NewGuid():N}.png");
            byte[] png;
            try { await Task.Run(() => DesktopInkNative.SaveScreenshot(path, region)); png = await File.ReadAllBytesAsync(path); }
            finally { File.Delete(path); }
            if (_disposed || _inTray) return;
            Toolbar.Show();
            Toolbar.ShowScreenshotActions(png, region.Value);
        }
        catch (Exception ex)
        {
            if (!_disposed && !_inTray) { Toolbar.Show(); Toolbar.ShowMessage(UiText.T("Не удалось сохранить снимок"), ex.Message); }
        }
        finally
        {
            _capturing = false;
            _screenshotSelection = null; _screenshotResult = null;
            Target = previousTarget;
            if (!_disposed) { UpdateState(); if (!_inTray) Toolbar.Show(); Toolbar.RaiseAboveInk(); }
        }
    }

    internal void BeginScreenshot(PixelPoint point) => _screenshotSelection?.Begin(point);
    internal void MoveScreenshot(PixelPoint point)
    {
        _screenshotSelection?.Move(point);
        foreach (var overlay in _overlays) overlay.Refresh();
    }
    internal void CompleteScreenshot() => _screenshotResult?.TrySetResult(_screenshotSelection?.Complete());
    internal void CancelScreenshot() => _screenshotResult?.TrySetResult(null);

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
        CancelScreenshot();
        CancelWindowFill();
        _mathSolve?.Cancel();
        _fadeTimer.Stop();
        _screenTimer.Stop();
        _desktopAi.Dispose(); _canvasAi.Dispose();
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
