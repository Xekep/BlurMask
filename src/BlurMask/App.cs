using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace BlurMask;

public sealed partial class App : Application
{
    private readonly List<BlurMaskWindow> _masks = [];
    private TrayIcon? _tray;
    private DrawingSession? _drawing;

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
            desktop.Exit += (_, _) => _drawing?.Dispose();

            if (Program.WebsitePreview)
                _ = SaveWebsitePreviewAsync(desktop);
            else if (Program.SmokeTest)
            {
                _ = ShutdownAfterSmokeTestAsync(desktop);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task ShutdownAfterSmokeTestAsync(IClassicDesktopStyleApplicationLifetime desktop)
    {
        await Task.Delay(200);
        try
        {
            if (Program.StartMinimized && (_drawing is not null || desktop.Windows.Count != 0))
                throw new InvalidOperationException("Minimized startup created an application window.");
            VerifyTrayToggle();
            ShowMask();
            _drawing?.SmokeTest();
            await DrawingSmokeTest.VerifyRenderingAsync(_drawing!);
            foreach (var mask in _masks) mask.Hide();
            await DrawingSmokeTest.VerifyCanvasAsync(_drawing!);
            VerifyTrayToggle();
            await DrawingSmokeTest.VerifyScrambleAsync();
            await DrawingSmokeTest.VerifyWindowAttachmentAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "BlurMask-smoke-error.txt"), ex.ToString());
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
                mode => ShowMask(mode), CloseAllMasks, enabled => { foreach (var mask in _masks.ToArray()) mask.SetWindowAttachment(enabled); });
            _drawing = drawing;
            drawing.Toolbar.PropertyChanged += (_, e) =>
            {
                if (e.Property == Visual.IsVisibleProperty) UpdateTrayMenu();
            };
            drawing.Toolbar.Closed += (_, _) =>
            {
                if (ReferenceEquals(_drawing, drawing)) { _drawing = null; UpdateTrayMenu(); }
            };
        }
        _drawing.Show();
    }

    private void ShowMask() => ShowMask(PrivacyMode.Blur);

    private void ShowMask(PrivacyMode mode)
    {
        _drawing?.SelectTool(InkTool.Cursor);
        var mask = new BlurMaskWindow();
        mask.SetMode(mode);
        _masks.Add(mask);
        mask.ModeChanged += MaskOnModeChanged;
        mask.Closed += (_, _) =>
        {
            mask.ModeChanged -= MaskOnModeChanged;
            _masks.Remove(mask);
            UpdateTrayTooltip(_masks.LastOrDefault()?.Mode);
        };

        mask.Show();
        mask.SetWindowAttachment(_drawing?.AttachMasksToWindows == true);
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
            : UiText.F("BlurMask — {0}; ПКМ по маске: следующий режим", ModeName(mode.Value));
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
