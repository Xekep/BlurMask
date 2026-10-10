using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace BlurMask;

public sealed partial class App : Application
{
    private BlurMaskWindow? _mask;
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
            ShowDrawing();
            desktop.Exit += (_, _) => _drawing?.Dispose();

            if (Program.SmokeTest)
            {
                ShowMask();
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
            _drawing?.SmokeTest();
            await DrawingSmokeTest.VerifyRenderingAsync(_drawing!);
        }
        catch (Exception ex)
        {
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "BlurMask-smoke-error.txt"), ex.ToString());
            desktop.Shutdown(1);
            return;
        }
        // Exercise every privacy mode during CI. This is not a replacement for real pointer
        // input testing, but it catches broken mode rendering/AOT trimming before packaging.
        for (var i = 0; i < 5; i++)
        {
            await Task.Delay(120).ConfigureAwait(false);
            Dispatcher.UIThread.Post(() => _mask?.CycleModeForSmokeTest());
        }

        await Task.Delay(800).ConfigureAwait(false);
        Dispatcher.UIThread.Post(() => desktop.Shutdown());
    }

    // XAML event handlers -----------------------------------------------------

    private void TrayOnClicked(object? sender, EventArgs e) => ShowDrawing();

    private void DrawingOnClicked(object? sender, EventArgs e) => ShowDrawing();

    private void NewMaskOnClicked(object? sender, EventArgs e) => ShowMask();

    private void CloseMaskOnClicked(object? sender, EventArgs e) => _mask?.Close();

    private void ExitOnClicked(object? sender, EventArgs e)
    {
        _mask?.Close();
        _drawing?.Dispose();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }

    // Mask -------------------------------------------------------------------

    private void ShowDrawing()
    {
        if (_drawing is null)
        {
            var drawing = new DrawingSession(ShowMask, () => ExitOnClicked(this, EventArgs.Empty));
            _drawing = drawing;
            drawing.Toolbar.Closed += (_, _) =>
            {
                if (ReferenceEquals(_drawing, drawing)) _drawing = null;
            };
        }
        _drawing.Show();
    }

    private void ShowMask()
    {
        _drawing?.SelectTool(InkTool.Cursor);
        if (_mask is { IsVisible: true })
        {
            _mask.Activate();
            return;
        }

        var mask = new BlurMaskWindow();
        _mask = mask;
        mask.ModeChanged += MaskOnModeChanged;
        mask.Closed += (_, _) =>
        {
            mask.ModeChanged -= MaskOnModeChanged;
            if (ReferenceEquals(_mask, mask))
                _mask = null;

            UpdateTrayTooltip(null);
        };

        mask.Show();
        UpdateTrayTooltip(mask.Mode);
    }

    private void MaskOnModeChanged(PrivacyMode mode) => UpdateTrayTooltip(mode);

    private void UpdateTrayTooltip(PrivacyMode? mode)
    {
        _tray ??= TrayIcon.GetIcons(this)?.FirstOrDefault();
        if (_tray is null)
            return;

        _tray.ToolTipText = mode is null
            ? "BlurMask — клик: инструменты рисования"
            : $"BlurMask — {ModeName(mode.Value)}; ПКМ по маске: следующий режим";
    }

    private static string ModeName(PrivacyMode mode) => mode switch
    {
        PrivacyMode.Blur => "Blur",
        PrivacyMode.BigPixels => "Pixel Blur",
        PrivacyMode.Scramble => "Scramble",
        PrivacyMode.GlassBlocks => "Glass Refraction",
        PrivacyMode.Blackout => "Blackout",
        _ => mode.ToString()
    };
}
