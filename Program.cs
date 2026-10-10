using Avalonia;
using System.Globalization;

namespace BlurMask;

internal static class Program
{
    internal static bool SmokeTest { get; private set; }
    internal static bool WebsitePreview { get; private set; }
    public static bool ShowTray => !SmokeTest;
    internal static bool SoftwareRendering { get; private set; }
    internal static AppSettings Settings { get; set; } = new();

    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Contains("--performance-test", StringComparer.OrdinalIgnoreCase)) { PerformanceProbe.Run(); return; }
        WebsitePreview = args.Contains("--website-preview", StringComparer.OrdinalIgnoreCase);
        var isolatedSmoke = WebsitePreview || args.Contains("--isolated-smoke-test", StringComparer.OrdinalIgnoreCase);
        // Acquire the single-instance guard before Avalonia or any native UI code starts.
        // A second launch exits silently with success and never creates a tray icon/window.
        using var singleInstance = isolatedSmoke ? null : SingleInstanceGuard.TryAcquire();
        if (singleInstance is null && !isolatedSmoke)
            return;

        SmokeTest = isolatedSmoke || args.Any(static arg =>
            string.Equals(arg, "--smoke-test", StringComparison.OrdinalIgnoreCase));
        Settings = SmokeTest ? new() : SettingsStore.Load();
        if (SmokeTest && Environment.GetEnvironmentVariable("BLURMASK_TEST_LANGUAGE") is { Length: > 0 } language)
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(language);
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture;
        }
        SoftwareRendering = args.Contains("--software-rendering", StringComparer.OrdinalIgnoreCase) ||
            RemoteDesktopSupport.IsRemoteSession;

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(
            args,
            Avalonia.Controls.ShutdownMode.OnExplicitShutdown);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        var builder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
        if (OperatingSystem.IsWindows() && SoftwareRendering)
            builder.With(new Win32PlatformOptions
            {
                RenderingMode = [Win32RenderingMode.Software],
                CompositionMode = [Win32CompositionMode.RedirectionSurface]
            });
        return builder;
    }
}
