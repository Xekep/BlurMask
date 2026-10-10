using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System.Diagnostics;

namespace BlurMask;

internal enum DrawingTarget { Screen, Canvas }

internal sealed class DrawingSession : IDisposable
{
    private readonly List<DesktopInkWindow> _overlays = [];
    private readonly Action _newMask;
    private readonly Action _exit;
    private readonly Action<PrivacyMode> _selectMask;
    private readonly Action _closeMask;
    private DesktopInkNative? _hotkeys;
    private InkTool _toolBeforeHide = InkTool.Cursor;
    private bool _disposed;
    private bool _capturing;
    private readonly DispatcherTimer _fadeTimer = new() { Interval = TimeSpan.FromMilliseconds(33) };
    private readonly DispatcherTimer _screenTimer = new() { Interval = TimeSpan.FromMilliseconds(300) };
    public InkDocument DesktopDocument { get; } = new();
    public InkDocument CanvasDocument { get; } = new();
    internal InkSelection DesktopSelection { get; }
    internal InkSelection CanvasSelection { get; }
    public InkDocument Document => Target == DrawingTarget.Screen ? DesktopDocument : CanvasDocument;
    public DrawingTarget Target { get; private set; }
    public CanvasWindow? Canvas { get; private set; }
    public InkTool Tool { get; private set; } = InkTool.Cursor;
    public Color Color { get; set; } = Color.Parse("#FFF400");
    public double PenSize { get; set; } = 4;
    public double TextSize { get; set; } = 28;
    public double FadeSeconds { get; private set; }
    public BoardMode Board { get; private set; }
    public bool InkVisible { get; private set; } = true;
    public DrawingToolbarWindow Toolbar { get; }
    internal IReadOnlyList<DesktopInkWindow> Overlays => _overlays;
    public event Action? StateChanged;

    public DrawingSession(Action newMask, Action exit, Action<PrivacyMode> selectMask, Action closeMask)
    {
        DesktopSelection = new(DesktopDocument);
        CanvasSelection = new(CanvasDocument);
        _newMask = newMask;
        _exit = exit;
        _selectMask = selectMask;
        _closeMask = closeMask;
        _screenTimer.Tick += (_, _) => { _screenTimer.Stop(); RebuildOverlays(); };
        Toolbar = new DrawingToolbarWindow(this);
        DesktopDocument.Changed += RefreshDocument;
        CanvasDocument.Changed += RefreshDocument;
        _fadeTimer.Tick += (_, _) =>
        {
            var previousCount = DesktopDocument.FadingMarks.Count + CanvasDocument.FadingMarks.Count;
            var now = Stopwatch.GetTimestamp();
            DesktopDocument.ExpireFading(now);
            CanvasDocument.ExpireFading(now);
            foreach (var overlay in _overlays) overlay.Refresh();
            Canvas?.Refresh();
            if (previousCount != DesktopDocument.FadingMarks.Count + CanvasDocument.FadingMarks.Count)
                StateChanged?.Invoke();
            if (DesktopDocument.FadingMarks.Count == 0 && CanvasDocument.FadingMarks.Count == 0)
                _fadeTimer.Stop();
        };
        Toolbar.Screens.Changed += OnScreensChanged;
        Toolbar.Opened += (_, _) =>
        {
            if (_overlays.Count == 0) RebuildOverlays();
            _hotkeys ??= DesktopInkNative.TryRegisterHotkeys(Toolbar, HandleShortcut);
            Toolbar.RaiseAboveInk();
        };
        Toolbar.Closed += (_, _) => Dispose();
    }

    public void Show()
    {
        if (!Toolbar.IsVisible) Toolbar.Show();
        else Toolbar.Activate();
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
        NotifySettings();
    }
    public void NewMask() { SelectScreen(); SelectTool(InkTool.Cursor); _newMask(); }
    public void CreateMask(PrivacyMode mode) { SelectScreen(); SelectTool(InkTool.Cursor); _selectMask(mode); }
    public void CloseAllMasks() => _closeMask();
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
        StateChanged?.Invoke();
    }
    private void RefreshDocument()
    {
        if (DesktopDocument.FadingMarks.Count > 0 || CanvasDocument.FadingMarks.Count > 0) _fadeTimer.Start();
        foreach (var overlay in _overlays) overlay.Refresh();
        Canvas?.Refresh();
        StateChanged?.Invoke();
    }

    private void HandleShortcut(int id)
    {
        switch (id)
        {
            case 1: SelectTool(InkTool.Cursor); break;
            case 2: SelectTool(InkTool.Pen); break;
            case 3: SelectTool(InkTool.Highlighter); break;
            case 4: SelectTool(InkTool.Eraser); break;
            case 5: Undo(); break;
            case 6: Clear(); break;
            case 7: ToggleInk(); break;
            case 8: Redo(); break;
            case 9: _ = CaptureScreenshotAsync(); break;
            case 10: OpenCanvas(); break;
            case 11: SelectScreen(); break;
        }
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
        Toolbar.Screens.Changed -= OnScreensChanged;
        DesktopDocument.Changed -= RefreshDocument;
        CanvasDocument.Changed -= RefreshDocument;
        Canvas?.Shutdown();
        Toolbar.CloseFlyout();
        foreach (var overlay in _overlays) overlay.Close();
        _overlays.Clear();
        Toolbar.Close();
    }
}
