using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace NovaPen;

public sealed partial class App : Application
{
    private readonly List<NovaPenWindow> _masks = [];
    private TrayIcon? _tray;
    private DrawingSession? _drawing;
    private readonly CancellationTokenSource _updateCheck = new();
    private string? _availableUpdate;
    private bool _updatePresented;

    public override void Initialize()
    {
        // Keep the tray icon/menu in compiled XAML. Besides being simpler, this is the
        // path Avalonia documents and keeps the Windows tray popup templates rooted for AOT.
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _tray = TrayIcon.GetIcons(this)?.FirstOrDefault();
            if (!Program.StartMinimized) ShowDrawing();
            UpdateTrayMenu();
            desktop.Exit += (_, _) =>
            {
                _updateCheck.Cancel();
                _updateCheck.Dispose();
                _drawing?.Dispose();
                DesktopInkNative.DisposePlatformResources();
            };

            if (Program.WebsitePreview)
                _ = SaveWebsitePreviewAsync(desktop);
            else if (Program.SmokeTest)
            {
                _ = ShutdownAfterSmokeTestAsync(desktop);
            }
            else _ = CheckForUpdatesAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task CheckForUpdatesAsync()
    {
        var version = await AppUpdates.CheckAsync(_updateCheck.Token).ConfigureAwait(false);
        if (version is null || _updateCheck.IsCancellationRequested) return;
        Dispatcher.UIThread.Post(() => { _availableUpdate = version; ShowUpdateNotice(); });
    }

    private void ShowUpdateNotice()
    {
        if (_updatePresented || _availableUpdate is null || _drawing?.Toolbar is not { IsVisible: true } toolbar) return;
        _updatePresented = true;
        toolbar.ShowUpdateAvailable(_availableUpdate);
    }

    private async Task ShutdownAfterSmokeTestAsync(IClassicDesktopStyleApplicationLifetime desktop)
    {
        await Task.Delay(200);
        try
        {
            if (Program.WaylandSmokeTest)
            {
                await DrawingSmokeTest.VerifyWaylandCanvasAsync(_drawing!);
                desktop.Shutdown();
                return;
            }
            if (Program.StartMinimized && (_drawing is not null || desktop.Windows.Count != 0))
                throw new InvalidOperationException("Minimized startup created an application window.");
            VerifyTrayToggle();
            ShowMask();
            _drawing?.SmokeTest();
            await DrawingSmokeTest.VerifyRenderingAsync(_drawing!);
            VerifyMaskEraser();
            foreach (var mask in _masks) mask.Hide();
            await DrawingSmokeTest.VerifyCanvasAsync(_drawing!);
            await DrawingSmokeTest.VerifyRegionScreenshotAsync(_drawing!);
            VerifyTrayToggle();
            await DrawingSmokeTest.VerifyScrambleAsync();
            await DrawingSmokeTest.VerifyWindowAttachmentAsync();
            await NativeMaskSmokeTest.RunAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "NovaPen-smoke-error.txt"), ex.ToString());
            if (Environment.GetEnvironmentVariable("BLURMASK_SMOKE_ARTIFACTS") is { Length: > 0 } artifacts)
            { Directory.CreateDirectory(artifacts); File.WriteAllText(Path.Combine(artifacts, "smoke-error.txt"), ex.ToString()); }
            desktop.Shutdown(1);
            return;
        }
        // Exercise every privacy mode during CI. This is not a replacement for real pointer
        // input testing, but it catches broken mode rendering/AOT trimming before packaging.
        for (var i = 0; i < 5; i++)
        {
            await Task.Delay(120).ConfigureAwait(false);
            Dispatcher.UIThread.Post(() => { foreach (var mask in _masks) mask.CycleModeForSmokeTest(); });
        }

        await Task.Delay(800).ConfigureAwait(false);
        Dispatcher.UIThread.Post(() => desktop.Shutdown());
    }

    // XAML event handlers -----------------------------------------------------

    private void TrayOnClicked(object? sender, EventArgs e)
    {
        if (_drawing?.Toolbar.IsVisible == true) _drawing.MinimizeToTray();
        else ShowDrawing();
    }

    private void UpdateTrayMenu()
    {
        if (_tray?.Menu?.Items.FirstOrDefault() is NativeMenuItem toggle)
            toggle.Header = _drawing?.Toolbar.IsVisible == true ? UiText.HideWindow : UiText.ShowWindow;
    }

    private void VerifyTrayToggle()
    {
        if (_tray?.Menu is not { Items.Count: 2 } menu || menu.Items[0] is not NativeMenuItem toggle ||
            menu.Items[1] is not NativeMenuItem exit || exit.Header != UiText.ExitApp)
            throw new InvalidOperationException("Tray menu must contain exactly visibility and exit actions.");
        if (_drawing?.Toolbar.IsVisible != true) TrayOnClicked(this, EventArgs.Empty);
        var session = _drawing!;
        var canvasVisible = session.Canvas?.IsVisible == true;
        var inkVisible = session.InkVisible;
        TrayOnClicked(this, EventArgs.Empty);
        if (session.Toolbar.IsVisible || session.Canvas?.IsVisible == true || session.InkVisible || toggle.Header != UiText.ShowWindow)
            throw new InvalidOperationException("Tray click did not hide the app or update its menu.");
        TrayOnClicked(this, EventArgs.Empty);
        if (!session.Toolbar.IsVisible || (session.Canvas?.IsVisible == true) != canvasVisible ||
            session.InkVisible != inkVisible || toggle.Header != UiText.HideWindow || !ReferenceEquals(session, _drawing))
            throw new InvalidOperationException("Tray click did not restore the existing drawing session.");
    }

    private void CloseAllMasks()
    {
        foreach (var mask in _masks.ToArray()) mask.Close();
    }
    private int EraseMasks(Point point, double radius)
    {
        var removed = 0;
        foreach (var mask in _masks.ToArray())
            if (mask.IsVisible && CanvasGeometry.IntersectsEraser(new Rect(mask.Position.X, mask.Position.Y,
                mask.Bounds.Width * mask.RenderScaling, mask.Bounds.Height * mask.RenderScaling), point, radius))
            { mask.Close(); removed++; }
        return removed;
    }
    private void VerifyMaskEraser()
    {
        var mask = _masks.First();
        var center = new Point(mask.Position.X + mask.Bounds.Width * mask.RenderScaling / 2,
            mask.Position.Y + mask.Bounds.Height * mask.RenderScaling / 2);
        _drawing!.SelectScreen(); _drawing.SelectTool(InkTool.Pen);
        if (mask.TryErase?.Invoke(center) != false || !_masks.Contains(mask))
            throw new InvalidOperationException("A drawing pen deleted a privacy mask.");
        _drawing.SelectTool(InkTool.Eraser);
        if (mask.TryErase?.Invoke(center) != true || _masks.Contains(mask) || mask.NativeOptics is not null)
            throw new InvalidOperationException("Mask eraser did not close the window and release its native optics.");
        _drawing.SelectTool(InkTool.Cursor);
        Console.WriteLine("Privacy mask eraser: own mask input closes the window and disposes native optics; pen leaves masks intact.");
    }

    private async Task SaveWebsitePreviewAsync(IClassicDesktopStyleApplicationLifetime desktop)
    {
        try { await WebsitePreview.SaveAsync(_drawing!); desktop.Shutdown(); }
        catch (Exception ex) { Console.Error.WriteLine(ex); desktop.Shutdown(1); }
    }

    private void ExitOnClicked(object? sender, EventArgs e)
    {
        CloseAllMasks();
        _drawing?.Dispose();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }

    // Mask -------------------------------------------------------------------

    private void ShowDrawing()
    {
        if (_drawing is null)
        {
            var drawing = new DrawingSession(ShowMask, () => ExitOnClicked(this, EventArgs.Empty),
                mode => ShowMask(mode), CloseAllMasks, enabled => { foreach (var mask in _masks.ToArray()) mask.SetWindowAttachment(enabled); }, EraseMasks, FillWindowRegion);
            _drawing = drawing;
            drawing.Toolbar.PropertyChanged += (_, e) =>
            {
                if (e.Property == Visual.IsVisibleProperty)
                {
                    UpdateTrayMenu();
                    if (drawing.Toolbar.IsVisible) Dispatcher.UIThread.Post(ShowUpdateNotice, DispatcherPriority.Loaded);
                }
            };
            drawing.Toolbar.Closed += (_, _) =>
            {
                if (ReferenceEquals(_drawing, drawing)) { _drawing = null; UpdateTrayMenu(); }
            };
        }
        _drawing.Show();
        if (Program.UseNativeWayland && _drawing.Canvas is null) _drawing.OpenCanvas();
    }

    private void ShowMask() => ShowMask(PrivacyMode.Blur);

    private void ShowMask(PrivacyMode mode) => ShowMask(mode, null, null);

    private void FillWindowRegion(TrackedWindow selected)
    {
        var catalog = NativeWindowCatalog.TryCreate();
        if (catalog is null) return;
        var region = catalog.ReadRegion(selected);
        if (region is not { Visible: true } || region.Bounds.Width <= 0 || region.Bounds.Height <= 0) { catalog.Dispose(); return; }
        ShowMask(PrivacyMode.Blackout, catalog, region);
    }
    private void ShowMask(PrivacyMode mode, IWindowCatalog? catalog, TrackedWindow? region)
    {
        if (Program.UseNativeWayland) { catalog?.Dispose(); return; }
        _drawing?.SelectTool(InkTool.Cursor);
        var mask = new NovaPenWindow();
        // A mask may be above the ink input layer; route its own input to the
        // eraser too, so clicking it cannot accidentally start a window drag.
        mask.TryErase = point => (_drawing?.EraseMasks(point, Math.Max(8, _drawing.PenSize * 2) * mask.RenderScaling) ?? 0) > 0;
        mask.SetMode(mode);
        if (region is not null)
        {
            double scale = (_drawing?.Toolbar.Screens.ScreenFromPoint(region.Bounds.Position) ?? _drawing?.Toolbar.Screens.Primary)?.Scaling ?? 1;
            mask.WindowStartupLocation = WindowStartupLocation.Manual; mask.Position = region.Bounds.Position;
            mask.MinWidth = 1 / scale; mask.MinHeight = 1 / scale; mask.Width = region.Bounds.Width / scale; mask.Height = region.Bounds.Height / scale;
            mask.Topmost = false; mask.ShowActivated = false;
        }
        _masks.Add(mask);
        mask.ModeChanged += MaskOnModeChanged;
        mask.Closed += (_, _) =>
        {
            mask.ModeChanged -= MaskOnModeChanged;
            _masks.Remove(mask);
            UpdateTrayTooltip(_masks.LastOrDefault()?.Mode);
        };

        mask.Show();
        if (region is not null && catalog is not null) mask.AttachFilledRegion(catalog, region);
        else mask.SetWindowAttachment(_drawing?.AttachMasksToWindows == true);
        UpdateTrayTooltip(mask.Mode);
    }

    private void MaskOnModeChanged(PrivacyMode mode) => UpdateTrayTooltip(mode);

    private void UpdateTrayTooltip(PrivacyMode? mode)
    {
        _tray ??= TrayIcon.GetIcons(this)?.FirstOrDefault();
        if (_tray is null)
            return;

        _tray.ToolTipText = mode is null
            ? UiText.TrayTooltip
            : UiText.F("NovaPen — {0}; ПКМ по маске: следующий режим", ModeName(mode.Value));
    }

    private static string ModeName(PrivacyMode mode) => mode switch
    {
        PrivacyMode.Blur => UiText.T("Размытие"),
        PrivacyMode.BigPixels => UiText.T("Крупные пиксели"),
        PrivacyMode.Scramble => UiText.T("Перемешивание"),
        PrivacyMode.GlassBlocks => UiText.T("Преломление стекла"),
        PrivacyMode.Blackout => UiText.T("Чёрная маска"),
        _ => mode.ToString()
    };
}
