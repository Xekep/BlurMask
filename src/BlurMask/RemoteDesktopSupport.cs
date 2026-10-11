using System.Runtime.InteropServices;

namespace BlurMask;

internal static partial class RemoteDesktopSupport
{
    internal static bool IsRemoteSession
    {
        get
        {
            if (!OperatingSystem.IsWindows()) return false;
            if (Program.SmokeTest && Environment.GetEnvironmentVariable("BLURMASK_TEST_REMOTE_SESSION") == "1") return true;
            // Query the actual session protocol: SM_REMOTESESSION alone can miss RemoteFX.
            if (WTSQuerySessionInformationW(0, -1, 16, out var buffer, out var size) != 0)
            {
                try { if (size >= 2) return Marshal.ReadInt16(buffer) != 0; }
                finally { WTSFreeMemory(buffer); }
            }
            return GetSystemMetrics(0x1000) != 0;
        }
    }

    [LibraryImport("wtsapi32.dll")]
    private static partial int WTSQuerySessionInformationW(nint server, int session, int infoClass, out nint buffer, out int bytes);
    [LibraryImport("wtsapi32.dll")]
    private static partial void WTSFreeMemory(nint memory);
    [LibraryImport("user32.dll")]
    private static partial int GetSystemMetrics(int index);
}
