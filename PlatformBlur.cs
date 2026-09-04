using Avalonia.Controls;

namespace BlurMask;

internal static class PlatformBlur
{
    public static void TryApplyNativeEnhancements(Window window)
    {
        // Windows и KDE/X11 используют TransparencyLevelHint напрямую.
        // Для macOS добавляем NSVisualEffectView, потому что Avalonia сама blur-behind там пока не даёт.
        if (OperatingSystem.IsMacOS())
            MacVisualEffectBlur.TryApply(window);
    }
}
