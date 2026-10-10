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
            foreach (var mask in _masks) mask.Hide();
            await DrawingSmokeTest.VerifyCanvasAsync(_drawing!);
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
            Dispatcher.UIThread.Post(() => { foreach (var mask in _masks) mask.CycleModeForSmokeTest(); });
        }

        await Task.Delay(800).ConfigureAwait(false);
        Dispatcher.UIThread.Post(() => desktop.Shutdown());
    }

    // XAML event handlers -----------------------------------------------------

    private void TrayOnClicked(object? sender, EventArgs e) => ShowDrawing();

    private void DrawingOnClicked(object? sender, EventArgs e) => ShowDrawing();

    private void NewMaskOnClicked(object? sender, EventArgs e) => ShowMask();

    private void CloseMaskOnClicked(object? sender, EventArgs e) => _masks.LastOrDefault()?.Close();

    private void CloseAllMasks()
    {
        foreach (var mask in _masks.ToArray()) mask.Close();
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
                mode => ShowMask(mode), CloseAllMasks);
            _drawing = drawing;
            drawing.Toolbar.Closed += (_, _) =>
            {
                if (ReferenceEquals(_drawing, drawing)) _drawing = null;
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
        UpdateTrayTooltip(mask.Mode);
    }

    private void MaskOnModeChanged(PrivacyMode mode) => UpdateTrayTooltip(mode);

    private void UpdateTrayTooltip(PrivacyMode? mode)
    {
        _tray ??= TrayIcon.GetIcons(this)?.FirstOrDefault();
        if (_tray is null)
            return;

        _tray.ToolTipText = mode is null
            ? UiText.T("BlurMask — клик: инструменты рисования")
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
