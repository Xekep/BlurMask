using Avalonia;

namespace BlurMask;

internal static class Program
{
    internal static bool SmokeTest { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        SmokeTest = args.Any(static arg => string.Equals(arg, "--smoke-test", StringComparison.OrdinalIgnoreCase));
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, Avalonia.Controls.ShutdownMode.OnExplicitShutdown);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
