using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Threading;

namespace BlurMask;

public sealed class App : Application
{
    private BlurMaskWindow? _mask;
    private TrayIcon? _tray;

    public override void Initialize()
    {
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            InstallTray(desktop);
            desktop.Exit += (_, _) => DisposeTray();

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
        await Task.Delay(1500).ConfigureAwait(false);
        Dispatcher.UIThread.Post(() => desktop.Shutdown());
    }

    private void InstallTray(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var create = new NativeMenuItem("Новая маска");
        create.Click += (_, _) => ShowMask();

        var close = new NativeMenuItem("Закрыть маску");
        close.Click += (_, _) => _mask?.Close();

        var exit = new NativeMenuItem("Закрыть программу");
        exit.Click += (_, _) =>
        {
            _mask?.Close();
            DisposeTray();
            desktop.Shutdown();
        };

        var tray = new TrayIcon
        {
            ToolTipText = "BlurMask — клик: маска; ПКМ по маске: режим",
            Icon = LoadIcon(),
            Menu = new NativeMenu
            {
                create,
                close,
                new NativeMenuItemSeparator(),
                exit
            }
        };

        tray.Clicked += (_, _) => ShowMask();
        _tray = tray;
        TrayIcon.SetIcons(this, new TrayIcons { tray });
    }

    private void DisposeTray()
    {
        if (_tray is null)
            return;

        TrayIcon.SetIcons(this, null);
        _tray.Dispose();
        _tray = null;
    }

    private static WindowIcon? LoadIcon()
    {
        try
        {
            return new WindowIcon(AssetLoader.Open(new Uri("avares://BlurMask/Assets/BlurMask.ico")));
        }
        catch
        {
            return null;
        }
    }

    private void ShowMask()
    {
        if (_mask is { IsVisible: true })
        {
            _mask.Activate();
            return;
        }

        _mask = new BlurMaskWindow();
        _mask.ModeChanged += MaskOnModeChanged;
        _mask.Closed += (_, _) =>
        {
            if (_mask is not null)
                _mask.ModeChanged -= MaskOnModeChanged;
            _mask = null;
            UpdateTrayTooltip(null);
        };
        _mask.Show();
        UpdateTrayTooltip(_mask.Mode);
    }

    private void MaskOnModeChanged(PrivacyMode mode) => UpdateTrayTooltip(mode);

    private void UpdateTrayTooltip(PrivacyMode? mode)
    {
        if (_tray is null)
            return;

        _tray.ToolTipText = mode is null
            ? "BlurMask — клик: создать маску"
            : $"BlurMask — {ModeName(mode.Value)}; ПКМ по маске: следующий режим";
    }

    private static string ModeName(PrivacyMode mode) => mode switch
    {
        PrivacyMode.Blur => "Blur",
        PrivacyMode.Acrylic => "Acrylic",
        PrivacyMode.Frosted => "Frosted",
        PrivacyMode.PixelMosaic => "Pixel Mosaic",
        PrivacyMode.Blackout => "Blackout",
        _ => mode.ToString()
    };
}
