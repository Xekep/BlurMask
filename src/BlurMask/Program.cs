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
    internal static bool UseNativeWayland { get; private set; }
    internal static bool WaylandSmokeTest { get; private set; }
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
        if (args.Contains("--recognition-memory-test", StringComparer.OrdinalIgnoreCase)) { RecognitionMemoryProbe.Run(args); return; }
        if (args.Contains("--mask-performance-test", StringComparer.OrdinalIgnoreCase)) { MaskPerformanceProbe.Run(); return; }
        if (args.Contains("--ai-pen-test", StringComparer.OrdinalIgnoreCase)) { AiPenProbe.Run(); return; }
        if (args.Contains("--overlay-platform-test", StringComparer.OrdinalIgnoreCase)) { OverlayPlatformProbe.Run(); return; }
        WebsitePreview = args.Contains("--website-preview", StringComparer.OrdinalIgnoreCase);
        WaylandSmokeTest = args.Contains("--wayland-smoke-test", StringComparer.OrdinalIgnoreCase);
        StartMinimized = !WebsitePreview && args.Contains("--minimized", StringComparer.OrdinalIgnoreCase);
        var isolatedSmoke = WebsitePreview || WaylandSmokeTest || args.Contains("--isolated-smoke-test", StringComparer.OrdinalIgnoreCase);
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
        var display = Settings.LinuxDisplayBackend;
        for (var i = 0; i < args.Length; i++)
            if (args[i] == "--display-backend")
            {
                if (++i >= args.Length || args[i] is not ("auto" or "x11" or "wayland"))
                    throw new ArgumentException("--display-backend requires auto, x11 or wayland.");
                display = args[i];
            }
        UseNativeWayland = OperatingSystem.IsLinux() && LinuxDisplayPolicy.UseWayland(display,
            Environment.GetEnvironmentVariable("DISPLAY"), Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));
        if (WaylandSmokeTest && !UseNativeWayland) throw new InvalidOperationException("Wayland smoke test requires the native Wayland backend.");
        if (OperatingSystem.IsLinux() && display == "wayland" && !UseNativeWayland)
            Console.Error.WriteLine("No WAYLAND_DISPLAY: using X11. Change the backend in Settings or pass --display-backend x11.");
        if (UseNativeWayland && args.Contains("--software-rendering", StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException("Native Wayland requires EGL. Use --display-backend x11 --software-rendering for CPU rendering.");
        SoftwareRendering = LinuxDisplayPolicy.UseSoftware(args.Contains("--software-rendering", StringComparer.OrdinalIgnoreCase),
            RemoteDesktopSupport.IsRemoteSession, OperatingSystem.IsLinux(), UseNativeWayland,
            Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR"), Environment.GetEnvironmentVariable("HYPRLAND_INSTANCE_SIGNATURE"));
        if (OperatingSystem.IsLinux()) Console.WriteLine($"Linux window backend: {(UseNativeWayland ? "Wayland" : "X11/XWayland")}; renderer: {(UseNativeWayland ? "Wayland EGL/framebuffer" : SoftwareRendering ? "CPU" : "default")}.");

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(
            args,
            Avalonia.Controls.ShutdownMode.OnExplicitShutdown);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        var builder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
#if LINUX_WAYLAND_BACKEND
        if (UseNativeWayland) builder.UseWayland();
#endif
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
