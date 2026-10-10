using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

namespace BlurMask;

internal sealed partial class DesktopInkNative : IDisposable
{
    private const int ExStyle = -20;
    private const long Transparent = 0x20;
    private const long Layered = 0x80000;
    private readonly Window _window;
    private readonly Action<ShortcutAction> _callback;
    private readonly List<int> _registered = [];
    private readonly nint _handle;
    internal IReadOnlyList<ShortcutAction> Failed { get; }
    internal bool Registered(ShortcutAction action) => _registered.Contains((int)action + 1);
    internal static void TestShortcutMessage(Window window, ShortcutAction action) => SendMessageW(Handle(window), 0x0312, (nint)((int)action + 1), 0);
    private DesktopInkNative(Window window, Action<ShortcutAction> callback, AppSettings settings)
    {
        _window = window;
        _callback = callback;
        _handle = Handle(window);
        Win32Properties.AddWndProcHookCallback(window, OnMessage);
        var failed = new List<ShortcutAction>();
        if (settings.HotkeysEnabled)
            foreach (var (action, binding) in settings.Shortcuts)
            {
                if (!binding.Enabled || !binding.Valid) continue;
                var id = (int)action + 1;
                if (RegisterHotKey(_handle, id, binding.NativeModifiers, binding.VirtualKey) != 0) _registered.Add(id);
                else failed.Add(action);
            }
        Failed = failed;
    }

    public static DesktopInkNative? TryRegisterHotkeys(Window window, Action<ShortcutAction> callback, AppSettings settings) =>
        OperatingSystem.IsWindows() ? new DesktopInkNative(window, callback, settings) : null;

    private nint OnMessage(nint hwnd, uint message, nint wParam, nint lParam, ref bool handled)
    {
        if (message == 0x0312 && _registered.Contains((int)wParam))
        {
            var id = (int)wParam;
            Dispatcher.UIThread.Post(() => _callback((ShortcutAction)(id - 1)));
            handled = true;
        }
        return 0;
    }

    public static void SetClickThrough(Window window, bool enabled)
    {
        if (!OperatingSystem.IsWindows()) return;
        var hwnd = Handle(window);
        var style = GetWindowLongPtrW(hwnd, ExStyle).ToInt64();
        var next = enabled ? style | Transparent | Layered : style & ~(Transparent | Layered);
        if (next == style) return;
        SetWindowLongPtrW(hwnd, ExStyle, (nint)next);
        if (enabled && SetLayeredWindowAttributes(hwnd, 0, 255, 2) == 0)
            throw new Win32Exception(Marshal.GetLastPInvokeError(), UiText.T("Не удалось включить режим курсора."));
        SetWindowPos(hwnd, 0, 0, 0, 0, 0, 0x1 | 0x2 | 0x4 | 0x10 | 0x20);
    }

    public static void Raise(Window window)
    {
        if (OperatingSystem.IsWindows() && window.IsVisible)
            SetWindowPos(Handle(window), -1, 0, 0, 0, 0, 0x1 | 0x2 | 0x10);
    }
    internal static void VerifyClickThrough(Window window)
    {
        var style = GetWindowLongPtrW(Handle(window), ExStyle).ToInt64();
        if ((style & (Transparent | Layered)) != (Transparent | Layered))
            throw new InvalidOperationException("Desktop overlay is not click-through in cursor mode.");
        VerifyInputTarget(window, false);
    }
    internal static void VerifyInputTarget(Window window, bool drawing)
    {
        var target = WindowFromPoint(new NativePoint { X = window.Position.X + 180, Y = window.Position.Y + 120 });
        var root = target == 0 ? 0 : GetAncestor(target, 2);
        if ((root == Handle(window)) != drawing)
            throw new InvalidOperationException(drawing ? "Drawing overlay does not receive desktop input." : "Cursor-mode overlay still intercepts desktop input.");
    }
    internal static void TestMouseGesture(Window window, int x, int y, int endX, int endY)
    {
        var hwnd = Handle(window);
        static nint Position(int px, int py) => (nint)((py << 16) | (px & 0xffff));
        // Send only to this app's HWND; do not move the user's system cursor or
        // inject clicks into unrelated applications during CI/development checks.
        SendMessageW(hwnd, 0x200, 0, Position(x, y));
        SendMessageW(hwnd, 0x201, 1, Position(x, y));
        SendMessageW(hwnd, 0x200, 1, Position(endX, endY));
        SendMessageW(hwnd, 0x202, 0, Position(endX, endY));
    }
    internal static void TestKey(Window window, int key)
    {
        var hwnd = Handle(window);
        SendMessageW(hwnd, 0x100, key, 1);
        SendMessageW(hwnd, 0x101, key, unchecked((nint)0xC0000001));
    }
    private static nint Handle(Window window) => window.TryGetPlatformHandle()?.Handle
        ?? throw new InvalidOperationException("Native window handle unavailable.");

    public static unsafe void SaveScreenshot(string path, PixelRect? region = null)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException(UiText.T("Снимки рабочего стола поддерживаются в Windows."));
        var x = region?.X ?? GetSystemMetrics(76);
        var y = region?.Y ?? GetSystemMetrics(77);
        var width = region?.Width ?? GetSystemMetrics(78);
        var height = region?.Height ?? GetSystemMetrics(79);
        if (width <= 0 || height <= 0) throw new InvalidOperationException(UiText.T("Рабочий стол недоступен."));
        var desktop = GetDC(0);
        if (desktop == 0) throw new Win32Exception(Marshal.GetLastPInvokeError());
        nint memory = 0, bitmap = 0, previous = 0;
        try
        {
            memory = CreateCompatibleDC(desktop);
            var info = new BitmapInfo
            {
                Size = 40, Width = width, Height = -height, Planes = 1, BitCount = 32
            };
            bitmap = CreateDIBSection(desktop, ref info, 0, out var bits, 0, 0);
            if (memory == 0 || bitmap == 0 || bits == 0) throw new Win32Exception(Marshal.GetLastPInvokeError());
            previous = SelectObject(memory, bitmap);
            // CAPTUREBLT includes layered annotation windows.
            if (BitBlt(memory, 0, 0, width, height, desktop, x, y, 0x00CC0020 | 0x40000000) == 0)
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            var pixels = new Span<byte>((void*)bits, checked(width * height * 4));
            for (var i = 3; i < pixels.Length; i += 4) pixels[i] = 255;
            using var image = new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Opaque, bits,
                new PixelSize(width, height), new Vector(96, 96), width * 4);
            image.Save(path, PngBitmapEncoderOptions.Default);
        }
        finally
        {
            if (previous != 0) SelectObject(memory, previous);
            if (bitmap != 0) DeleteObject(bitmap);
            if (memory != 0) DeleteDC(memory);
            ReleaseDC(0, desktop);
        }
    }

    public void Dispose()
    {
        foreach (var id in _registered) UnregisterHotKey(_handle, id);
        _registered.Clear();
        Win32Properties.RemoveWndProcHookCallback(_window, OnMessage);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint { public int X, Y; }
    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfo
    {
        public uint Size;
        public int Width, Height;
        public ushort Planes, BitCount;
        public uint Compression, SizeImage;
        public int XPelsPerMeter, YPelsPerMeter;
        public uint ClrUsed, ClrImportant, Color;
    }
    [LibraryImport("user32.dll", SetLastError = true)] private static partial int RegisterHotKey(nint hwnd, int id, uint modifiers, uint key);
    [LibraryImport("user32.dll")] private static partial int UnregisterHotKey(nint hwnd, int id);
    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW")] private static partial nint GetWindowLongPtrW(nint hwnd, int index);
    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW")] private static partial nint SetWindowLongPtrW(nint hwnd, int index, nint value);
    [LibraryImport("user32.dll", SetLastError = true)] private static partial int SetLayeredWindowAttributes(nint hwnd, uint color, byte alpha, uint flags);
    [LibraryImport("user32.dll")] private static partial int SetWindowPos(nint hwnd, nint after, int x, int y, int width, int height, uint flags);
    [LibraryImport("user32.dll")] private static partial int GetSystemMetrics(int index);
    [LibraryImport("user32.dll")] private static partial nint WindowFromPoint(NativePoint point);
    [LibraryImport("user32.dll")] private static partial nint GetAncestor(nint hwnd, uint flags);
    [LibraryImport("user32.dll")] private static partial nint SendMessageW(nint hwnd, uint message, nint wParam, nint lParam);
    [LibraryImport("user32.dll")] private static partial nint GetDC(nint hwnd);
    [LibraryImport("gdi32.dll")] private static partial uint GetPixel(nint dc, int x, int y);
    internal static uint ReadScreenPixel(PixelPoint point)
    {
        var dc = GetDC(0);
        try { return dc == 0 ? uint.MaxValue : GetPixel(dc, point.X, point.Y); }
        finally { if (dc != 0) ReleaseDC(0, dc); }
    }
    [LibraryImport("user32.dll")] private static partial int ReleaseDC(nint hwnd, nint dc);
    [LibraryImport("gdi32.dll")] private static partial nint CreateCompatibleDC(nint dc);
    [LibraryImport("gdi32.dll", SetLastError = true)] private static partial nint CreateDIBSection(nint dc, ref BitmapInfo info, uint usage, out nint bits, nint section, uint offset);
    [LibraryImport("gdi32.dll")] private static partial nint SelectObject(nint dc, nint obj);
    [LibraryImport("gdi32.dll", SetLastError = true)] private static partial int BitBlt(nint dc, int x, int y, int width, int height, nint source, int sourceX, int sourceY, uint flags);
    [LibraryImport("gdi32.dll")] private static partial int DeleteObject(nint obj);
    [LibraryImport("gdi32.dll")] private static partial int DeleteDC(nint dc);
}
