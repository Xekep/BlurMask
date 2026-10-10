using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace BlurMask;

internal sealed class DrawingSession : IDisposable
{
    private readonly List<DesktopInkWindow> _overlays = [];
    private readonly Action _newMask;
    private readonly Action _exit;
    private DesktopInkNative? _hotkeys;
    private InkTool _toolBeforeHide = InkTool.Cursor;
    private bool _disposed;
    private bool _capturing;
    public InkDocument Document { get; } = new();
    public InkTool Tool { get; private set; } = InkTool.Cursor;
    public Color Color { get; set; } = Color.Parse("#FFF400");
    public double PenSize { get; set; } = 4;
    public double TextSize { get; set; } = 28;
    public BoardMode Board { get; private set; }
    public bool InkVisible { get; private set; } = true;
    public DrawingToolbarWindow Toolbar { get; }
    internal IReadOnlyList<DesktopInkWindow> Overlays => _overlays;
    public event Action? StateChanged;

    public DrawingSession(Action newMask, Action exit)
    {
        _newMask = newMask;
        _exit = exit;
        Toolbar = new DrawingToolbarWindow(this);
        Document.Changed += RefreshDocument;
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

    private void OnScreensChanged(object? sender, EventArgs e) => RebuildOverlays();
    private void RebuildOverlays()
    {
        if (_disposed) return;
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
        Tool = tool;
        if (tool != InkTool.Cursor) InkVisible = true;
        UpdateState();
    }
    public void ToggleInk()
    {
        foreach (var overlay in _overlays) { overlay.FinishText(); overlay.CancelGesture(); }
        if (InkVisible) _toolBeforeHide = Tool;
        InkVisible = !InkVisible;
        Tool = InkVisible ? _toolBeforeHide : InkTool.Cursor;
        UpdateState();
    }
    public void SetBoard(BoardMode board) { Board = board; UpdateState(); }
    public void NotifySettings() => StateChanged?.Invoke();
    public void NewMask() { SelectTool(InkTool.Cursor); _newMask(); }
    public void Exit() => _exit();
    public void Undo() { FinishInput(); Document.Undo(); }
    public void Redo() { FinishInput(); Document.Redo(); }
    public void Clear() { FinishInput(); Document.Clear(); }
    private void FinishInput()
    {
        foreach (var overlay in _overlays) { overlay.FinishText(); overlay.CancelGesture(); }
    }
    private void UpdateState()
    {
        foreach (var overlay in _overlays) overlay.ApplyState();
        Toolbar.RaiseAboveInk();
        StateChanged?.Invoke();
    }
    private void RefreshDocument()
    {
        foreach (var overlay in _overlays) overlay.Refresh();
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
            Toolbar.ShowMessage("Снимок сохранён", path);
        }
        catch (Exception ex)
        {
            Toolbar.Show();
            Toolbar.ShowMessage("Не удалось сохранить снимок", ex.Message);
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
        _hotkeys?.Dispose();
        Toolbar.Screens.Changed -= OnScreensChanged;
        Document.Changed -= RefreshDocument;
        Toolbar.CloseFlyout();
        foreach (var overlay in _overlays) overlay.Close();
        _overlays.Clear();
        Toolbar.Close();
    }
}
