using Avalonia;
using System.Globalization;

namespace BlurMask;

internal static class Program
{
    internal static bool SmokeTest { get; private set; }
    internal static bool WebsitePreview { get; private set; }
    internal static bool StartMinimized { get; private set; }
    public static bool ShowTray => !SmokeTest;
    internal static bool SoftwareRendering { get; private set; }
    internal static AppSettings Settings { get; set; } = new();

    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length == 2 && args[0] == "--install-ink-math-model")
        {
            using var model = File.OpenRead(args[1]);
            AiMathRecognitionModels.Install(model);
            Console.WriteLine($"Installed math recognition model: {AiMathRecognitionModels.PathName}");
            return;
        }
        if (args.Length == 2 && args[0] == "--install-ink-model")
        {
            using var model = File.OpenRead(args[1]);
            AiRecognitionModels.Install(model);
            Console.WriteLine($"Installed ink recognition model: {AiRecognitionModels.PathName}");
            return;
        }
        if (args.Contains("--performance-test", StringComparer.OrdinalIgnoreCase)) { PerformanceProbe.Run(); return; }
        if (args.Contains("--mask-performance-test", StringComparer.OrdinalIgnoreCase)) { MaskPerformanceProbe.Run(); return; }
        if (args.Contains("--ai-pen-test", StringComparer.OrdinalIgnoreCase)) { AiPenProbe.Run(); return; }
        if (args.Contains("--overlay-platform-test", StringComparer.OrdinalIgnoreCase)) { OverlayPlatformProbe.Run(); return; }
        WebsitePreview = args.Contains("--website-preview", StringComparer.OrdinalIgnoreCase);
        StartMinimized = !WebsitePreview && args.Contains("--minimized", StringComparer.OrdinalIgnoreCase);
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
        if (OperatingSystem.IsLinux() && SoftwareRendering)
            builder.With(new X11PlatformOptions
            {
                RenderingMode = [X11RenderingMode.Software]
            });
        return builder;
    }
}
