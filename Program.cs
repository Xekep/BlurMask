using Avalonia;

namespace BlurMask;

internal static class Program
{
    internal static bool SmokeTest { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        // Acquire the single-instance guard before Avalonia or any native UI code starts.
        // A second launch exits silently with success and never creates a tray icon/window.
        using var singleInstance = SingleInstanceGuard.TryAcquire();
        if (singleInstance is null)
            return;

        SmokeTest = args.Any(static arg =>
            string.Equals(arg, "--smoke-test", StringComparison.OrdinalIgnoreCase));

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(
            args,
            Avalonia.Controls.ShutdownMode.OnExplicitShutdown);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
