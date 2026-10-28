using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace NovaPen;

internal static class DesktopScreenshotCapture
{
    internal static void Save(string path, PixelRect? region)
    {
        if (OperatingSystem.IsMacOS())
        {
            var start = new ProcessStartInfo("/usr/sbin/screencapture") { UseShellExecute = false, CreateNoWindow = true };
            start.ArgumentList.Add("-x");
            if (region is { } area) start.ArgumentList.Add(string.Create(CultureInfo.InvariantCulture, $"-R{area.X},{area.Y},{area.Width},{area.Height}"));
            start.ArgumentList.Add(path);
            using var process = Process.Start(start) ?? throw new InvalidOperationException("Screen capture could not start.");
            if (!process.WaitForExit(15000)) { process.Kill(); throw new TimeoutException("Screen capture timed out."); }
            if (process.ExitCode != 0 || !File.Exists(path)) throw new InvalidOperationException("Screen capture failed. Allow Screen Recording in system settings.");
            return;
        }
        if (OperatingSystem.IsLinux() && !Program.UseNativeWayland)
        {
            using var catalog = XcbWindowCatalog.TryCreate() ?? throw new InvalidOperationException("X11 screen is unavailable.");
            catalog.SaveScreenshot(path, region);
            return;
        }
        throw new PlatformNotSupportedException("Screen capture is unavailable on this window backend.");
    }
}

internal sealed partial class XcbWindowCatalog
{
    internal unsafe void SaveScreenshot(string path, PixelRect? requested)
    {
        var setup = xcb_get_setup(_connection);
        var screens = xcb_setup_roots_iterator(setup);
        while (screens.Remaining > 0 && unchecked((uint)Marshal.ReadInt32(screens.Data)) != _root) xcb_screen_next(ref screens);
        if (screens.Remaining == 0 || screens.Data == 0) throw new InvalidOperationException("X11 screen is unavailable.");
        var screen = screens.Data;
        var bounds = new PixelRect(0, 0, (ushort)Marshal.ReadInt16(screen, 20), (ushort)Marshal.ReadInt16(screen, 22));
        var region = requested?.Intersect(bounds) ?? bounds;
        if (region.Width <= 0 || region.Height <= 0 || (long)region.Width * region.Height > 32_000_000)
            throw new InvalidOperationException("Screen capture area is empty or too large.");
        var visualId = unchecked((uint)Marshal.ReadInt32(screen, 32));
        uint red = 0, green = 0, blue = 0;
        var depths = xcb_screen_allowed_depths_iterator(screen);
        while (depths.Remaining > 0)
        {
            var visuals = xcb_depth_visuals_iterator(depths.Data);
            while (visuals.Remaining > 0)
            {
                if (unchecked((uint)Marshal.ReadInt32(visuals.Data)) == visualId)
                {
                    red = unchecked((uint)Marshal.ReadInt32(visuals.Data, 8));
                    green = unchecked((uint)Marshal.ReadInt32(visuals.Data, 12));
                    blue = unchecked((uint)Marshal.ReadInt32(visuals.Data, 16));
                    break;
                }
                xcb_visualtype_next(ref visuals);
            }
            if (red != 0) break;
            xcb_depth_next(ref depths);
        }
        if (red == 0 || green == 0 || blue == 0) throw new InvalidOperationException("Unsupported X11 colour visual.");
        var reply = xcb_get_image_reply(_connection,
            xcb_get_image(_connection, 2, _root, checked((short)region.X), checked((short)region.Y),
                checked((ushort)region.Width), checked((ushort)region.Height), uint.MaxValue), out var error);
        try
        {
            if (reply == 0 || error != 0) throw new InvalidOperationException("X11 could not capture the selected area.");
            var depth = Marshal.ReadByte(reply, 1);
            int bits = 0, padding = 0;
            var formats = xcb_setup_pixmap_formats_iterator(setup);
            while (formats.Remaining > 0)
            {
                if (Marshal.ReadByte(formats.Data) == depth) { bits = Marshal.ReadByte(formats.Data, 1); padding = Marshal.ReadByte(formats.Data, 2); break; }
                xcb_format_next(ref formats);
            }
            if (bits is not (16 or 24 or 32) || padding is not (8 or 16 or 32)) throw new InvalidOperationException("Unsupported X11 pixel layout.");
            int stride = checked(((region.Width * bits + padding - 1) / padding) * (padding / 8));
            int length = xcb_get_image_data_length(reply);
            if (length < (long)stride * region.Height) throw new InvalidOperationException("Truncated X11 screen image.");
            var pixels = new ReadOnlySpan<byte>((void*)xcb_get_image_data(reply), length);
            bool bigEndian = Marshal.ReadByte(setup, 30) != 0;
            using var image = new WriteableBitmap(new PixelSize(region.Width, region.Height), new Avalonia.Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
            using (var target = image.Lock())
            {
                var output = new Span<byte>((void*)target.Address, target.RowBytes * region.Height);
                for (int y = 0; y < region.Height; y++) for (int x = 0; x < region.Width; x++)
                {
                    int sourceOffset = y * stride + x * (bits / 8); uint value = 0;
                    for (int b = 0; b < bits / 8; b++) value |= (uint)pixels[sourceOffset + b] << (8 * (bigEndian ? bits / 8 - b - 1 : b));
                    int destination = y * target.RowBytes + x * 4;
                    output[destination] = Channel(value, blue); output[destination + 1] = Channel(value, green);
                    output[destination + 2] = Channel(value, red); output[destination + 3] = 255;
                }
            }
            image.Save(path, PngBitmapEncoderOptions.Default);
        }
        finally { Free(reply, error); }
    }
    private static byte Channel(uint pixel, uint mask)
    {
        int shift = BitOperations.TrailingZeroCount(mask);
        uint maximum = mask >> shift;
        return (byte)(((ulong)((pixel & mask) >> shift) * 255 + maximum / 2) / maximum);
    }

    // Iterator ABI is the same pointer/count/index layout as the existing screen iterator.
    [LibraryImport("libxcb.so.1")] private static partial ScreenIterator xcb_screen_allowed_depths_iterator(nint screen);
    [LibraryImport("libxcb.so.1")] private static partial ScreenIterator xcb_depth_visuals_iterator(nint depth);
    [LibraryImport("libxcb.so.1")] private static partial void xcb_visualtype_next(ref ScreenIterator iterator);
    [LibraryImport("libxcb.so.1")] private static partial void xcb_depth_next(ref ScreenIterator iterator);
    [LibraryImport("libxcb.so.1")] private static partial ScreenIterator xcb_setup_pixmap_formats_iterator(nint setup);
    [LibraryImport("libxcb.so.1")] private static partial void xcb_format_next(ref ScreenIterator iterator);
    [LibraryImport("libxcb.so.1")] private static partial uint xcb_get_image(nint connection, byte format, uint drawable, short x, short y, ushort width, ushort height, uint planeMask);
    [LibraryImport("libxcb.so.1")] private static partial nint xcb_get_image_reply(nint connection, uint cookie, out nint error);
    [LibraryImport("libxcb.so.1")] private static partial nint xcb_get_image_data(nint reply);
    [LibraryImport("libxcb.so.1")] private static partial int xcb_get_image_data_length(nint reply);
}
