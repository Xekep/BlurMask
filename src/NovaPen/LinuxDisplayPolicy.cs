namespace NovaPen;

// Backend choice is made before any native window or renderer is initialized.
internal static class LinuxDisplayPolicy
{
    internal static bool UseWayland(string preference, string? x11Display, string? waylandDisplay) =>
        !string.IsNullOrWhiteSpace(waylandDisplay) &&
        (preference == "wayland" || preference == "auto" && string.IsNullOrWhiteSpace(x11Display));

    internal static bool UseSoftware(bool requested, bool remote, bool linux, bool wayland, string? runtime, string? signature) =>
        !wayland && (requested || remote || linux && HyprlandInkOverlay.SocketPath(runtime, signature) is not null);
}
