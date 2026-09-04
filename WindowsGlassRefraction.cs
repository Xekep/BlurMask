using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace BlurMask;

/// <summary>
/// Windows optical-glass backend using a fixed number of Magnification controls.
///
/// The previous implementation created one native Magnifier HWND for every glass cell.
/// A moderately sized mask could therefore make DWM manage hundreds of live child windows.
/// This implementation always creates only four full-size magnifier planes. Each plane is
/// clipped with a disjoint Win32 region made from 30x30 pixel glass cells and has a slightly
/// different source offset/scale/color transform. Neighboring cells therefore sample different
/// desktop pixels while the native window count stays constant as the mask grows.
///
/// There is no polling/render timer. Magnifier controls keep live desktop content themselves;
/// BlurMask only updates source geometry when Avalonia reports a move or resize.
/// </summary>
internal sealed partial class WindowsGlassRefraction : IDisposable
{
    private const int PlaneCount = 4;
    private const int TilePitch = 32;
    private const int Seam = 1;
    private const int TileSide = TilePitch - Seam * 2; // 30 px, always square.

    private const uint WsChild = 0x40000000;
    private const uint WsVisible = 0x10000000;
    private const uint WsDisabled = 0x08000000;
    private const uint WsClipSiblings = 0x04000000;
    private const uint WsExTransparent = 0x00000020;
    private const uint WsExNoActivate = 0x08000000;

    private const uint SwpNoZOrder = 0x0004;
    private const uint SwpNoActivate = 0x0010;
    private const uint SwpShowWindow = 0x0040;
    private const int SwHide = 0;
    private const int SwShowNa = 8;

    private const uint MwFilterModeExclude = 0;
    private const int RgnOr = 2;

    private readonly Window _window;
    private readonly nint _host;
    private readonly Plane[] _planes = new Plane[PlaneCount];

    private bool _enabled;
    private bool _disposed;
    private int _lastWidth = -1;
    private int _lastHeight = -1;
    private int _lastOriginX = int.MinValue;
    private int _lastOriginY = int.MinValue;
    private int _lastColumns = -1;
    private int _lastRows = -1;

    private WindowsGlassRefraction(Window window, nint host)
    {
        _window = window;
        _host = host;

        CreatePlanes();

        _window.PositionChanged += OnWindowPositionChanged;
        _window.Resized += OnWindowResized;
    }

    public static WindowsGlassRefraction? TryCreate(Window window)
    {
        if (!OperatingSystem.IsWindows())
            return null;

        try
        {
            var handle = window.TryGetPlatformHandle();
            if (handle is null ||
                !string.Equals(handle.HandleDescriptor, "HWND", StringComparison.OrdinalIgnoreCase) ||
                handle.Handle == IntPtr.Zero)
            {
                return null;
            }

            if (MagInitialize() == 0)
                return null;

            try
            {
                return new WindowsGlassRefraction(window, handle.Handle);
            }
            catch
            {
                _ = MagUninitialize();
                throw;
            }
        }
        catch
        {
            return null;
        }
    }

    private void CreatePlanes()
    {
        var instance = GetModuleHandleW(null);
        if (instance == IntPtr.Zero)
            throw new InvalidOperationException("GetModuleHandleW failed.");

        for (var i = 0; i < PlaneCount; i++)
        {
            var hwnd = CreateWindowExW(
                WsExTransparent | WsExNoActivate,
                "Magnifier",
                null,
                WsChild | WsVisible | WsDisabled | WsClipSiblings,
                0,
                0,
                1,
                1,
                _host,
                IntPtr.Zero,
                instance,
                IntPtr.Zero);

            if (hwnd == IntPtr.Zero)
                throw new InvalidOperationException($"Failed to create magnifier plane {i}.");

            _ = EnableWindow(hwnd, 0);

            unsafe
            {
                var excluded = _host;
                _ = MagSetWindowFilterList(hwnd, MwFilterModeExclude, 1, &excluded);
            }

            var optics = OpticsForPlane(i);
            var transform = MagTransform.Scale(optics.Scale);
            if (MagSetWindowTransform(hwnd, ref transform) == 0)
                throw new InvalidOperationException($"MagSetWindowTransform failed for plane {i}.");

            var frost = MagColorEffect.Frosted(i);
            _ = MagSetColorEffect(hwnd, ref frost);

            _planes[i] = new Plane(hwnd, optics.Scale, optics.OffsetX, optics.OffsetY);
            _ = ShowWindow(hwnd, SwHide);
        }
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _enabled == enabled)
            return;

        _enabled = enabled;

        if (enabled)
        {
            // Force exactly one complete sync when entering the mode. Afterwards only actual
            // native move/resize notifications touch source geometry.
            _lastWidth = -1;
            _lastHeight = -1;
            _lastOriginX = int.MinValue;
            _lastOriginY = int.MinValue;
            _lastColumns = -1;
            _lastRows = -1;
            UpdateGeometry(force: true);

            foreach (var plane in _planes)
            {
                if (plane is not null)
                    _ = ShowWindow(plane.Hwnd, SwShowNa);
            }
        }
        else
        {
            foreach (var plane in _planes)
            {
                if (plane is not null)
                    _ = ShowWindow(plane.Hwnd, SwHide);
            }
        }
    }

    private void OnWindowPositionChanged(object? sender, PixelPointEventArgs e)
    {
        if (_enabled && !_disposed)
            UpdateGeometry(force: false);
    }

    private void OnWindowResized(object? sender, WindowResizedEventArgs e)
    {
        if (_enabled && !_disposed)
            UpdateGeometry(force: false);
    }

    private void UpdateGeometry(bool force)
    {
        if (GetClientRect(_host, out var client) == 0)
            return;

        var width = Math.Max(1, client.Right - client.Left);
        var height = Math.Max(1, client.Bottom - client.Top);

        var origin = new NativePoint(0, 0);
        if (ClientToScreen(_host, ref origin) == 0)
            return;

        var sizeChanged = width != _lastWidth || height != _lastHeight;
        var positionChanged = origin.X != _lastOriginX || origin.Y != _lastOriginY;

        if (!force && !sizeChanged && !positionChanged)
            return;

        _lastWidth = width;
        _lastHeight = height;
        _lastOriginX = origin.X;
        _lastOriginY = origin.Y;

        var columns = Math.Max(1, (width + TilePitch - 1) / TilePitch);
        var rows = Math.Max(1, (height + TilePitch - 1) / TilePitch);
        var gridChanged = columns != _lastColumns || rows != _lastRows;

        if (force || sizeChanged)
        {
            for (var i = 0; i < PlaneCount; i++)
            {
                var plane = _planes[i];
                _ = SetWindowPos(
                    plane.Hwnd,
                    IntPtr.Zero,
                    0,
                    0,
                    width,
                    height,
                    SwpNoZOrder | SwpNoActivate | (_enabled ? SwpShowWindow : 0));
            }
        }

        // A region depends only on the number of 32 px grid cells, not the exact client size.
        // The child window clips any overhanging edge cell automatically. During interactive
        // resizing this avoids rebuilding hundreds of GDI rectangles for every single pixel.
        if (force || gridChanged)
        {
            for (var i = 0; i < PlaneCount; i++)
                RebuildPlaneRegion(_planes[i].Hwnd, i, columns, rows);

            _lastColumns = columns;
            _lastRows = rows;
        }

        // Four source updates per move/resize, regardless of how many glass cells are visible.
        // The old design performed one update per cell and created a native window per cell.
        for (var i = 0; i < PlaneCount; i++)
        {
            var plane = _planes[i];
            UpdatePlaneSource(plane, origin.X, origin.Y, width, height);
        }
    }

    private static void RebuildPlaneRegion(nint hwnd, int planeIndex, int columns, int rows)
    {
        var combined = CreateRectRgn(0, 0, 0, 0);
        if (combined == IntPtr.Zero)
            return;

        var handedToWindow = false;
        try
        {
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    if (PlaneForCell(column, row) != planeIndex)
                        continue;

                    var left = column * TilePitch + Seam;
                    var top = row * TilePitch + Seam;
                    var right = left + TileSide;
                    var bottom = top + TileSide;

                    var cell = CreateRectRgn(left, top, right, bottom);
                    if (cell == IntPtr.Zero)
                        continue;

                    try
                    {
                        _ = CombineRgn(combined, combined, cell, RgnOr);
                    }
                    finally
                    {
                        _ = DeleteObject(cell);
                    }
                }
            }

            // On success Windows owns the HRGN and deletes it when it is replaced/destroyed.
            if (SetWindowRgn(hwnd, combined, 1) != 0)
                handedToWindow = true;
        }
        finally
        {
            if (!handedToWindow)
                _ = DeleteObject(combined);
        }
    }

    private static int PlaneForCell(int column, int row)
    {
        // Hash instead of a plain checkerboard. Straight edges crossing the grid therefore
        // encounter irregular refraction directions, much closer to cast/textured glass.
        return (int)(Hash(column, row) % PlaneCount);
    }

    private static void UpdatePlaneSource(Plane plane, int originX, int originY, int width, int height)
    {
        var sourceWidth = Math.Max(1, (int)Math.Round(width / plane.Scale));
        var sourceHeight = Math.Max(1, (int)Math.Round(height / plane.Scale));

        // Magnify very slightly around the window centre, then shift each optical plane a few
        // pixels in a different direction. Region clipping turns these four global transforms
        // into a piecewise/tiled refraction field.
        var left = originX + (width - sourceWidth) / 2 + plane.OffsetX;
        var top = originY + (height - sourceHeight) / 2 + plane.OffsetY;

        var source = new NativeRect(
            left,
            top,
            left + sourceWidth,
            top + sourceHeight);

        _ = MagSetWindowSource(plane.Hwnd, source);
        _ = InvalidateRect(plane.Hwnd, IntPtr.Zero, 0);
    }

    private static (float Scale, int OffsetX, int OffsetY) OpticsForPlane(int plane) => plane switch
    {
        0 => (1.010f, -3, -2),
        1 => (1.016f,  3, -2),
        2 => (1.022f, -2,  3),
        _ => (1.013f,  3,  2)
    };

    private static uint Hash(int x, int y)
    {
        unchecked
        {
            uint h = 2166136261;
            h = (h ^ (uint)(x + 17)) * 16777619;
            h = (h ^ (uint)(y + 31)) * 16777619;
            h ^= h >> 13;
            h *= 0x5bd1e995;
            h ^= h >> 15;
            return h;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _enabled = false;

        _window.PositionChanged -= OnWindowPositionChanged;
        _window.Resized -= OnWindowResized;

        foreach (var plane in _planes)
        {
            if (plane is not null && plane.Hwnd != IntPtr.Zero)
                _ = DestroyWindow(plane.Hwnd);
        }

        try
        {
            _ = MagUninitialize();
        }
        catch
        {
            // Best-effort native cleanup during shutdown.
        }
    }

    private sealed class Plane
    {
        public nint Hwnd { get; }
        public float Scale { get; }
        public int OffsetX { get; }
        public int OffsetY { get; }

        public Plane(nint hwnd, float scale, int offsetX, int offsetY)
        {
            Hwnd = hwnd;
            Scale = scale;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;

        public NativePoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public NativeRect(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MagTransform
    {
        public float M11;
        public float M12;
        public float M13;
        public float M21;
        public float M22;
        public float M23;
        public float M31;
        public float M32;
        public float M33;

        public static MagTransform Scale(float scale) => new()
        {
            M11 = scale,
            M22 = scale,
            M33 = 1.0f
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MagColorEffect
    {
        public float M11; public float M12; public float M13; public float M14; public float M15;
        public float M21; public float M22; public float M23; public float M24; public float M25;
        public float M31; public float M32; public float M33; public float M34; public float M35;
        public float M41; public float M42; public float M43; public float M44; public float M45;
        public float M51; public float M52; public float M53; public float M54; public float M55;

        public static MagColorEffect Frosted(int plane)
        {
            // Lower contrast + a small white lift gives the "clouded glass" part of the effect.
            // Spatial distortion comes from the source transform; this matrix stays cheap.
            var cross = 0.050f + plane * 0.004f;
            var main = 0.78f - plane * 0.012f;
            var lift = 0.105f + plane * 0.010f;

            return new MagColorEffect
            {
                M11 = main, M12 = cross, M13 = cross,
                M21 = cross, M22 = main, M23 = cross,
                M31 = cross, M32 = cross, M33 = main,
                M44 = 1.0f,
                M51 = lift, M52 = lift, M53 = lift,
                M55 = 1.0f
            };
        }
    }

    [LibraryImport("Magnification.dll")]
    private static partial int MagInitialize();

    [LibraryImport("Magnification.dll")]
    private static partial int MagUninitialize();

    [LibraryImport("Magnification.dll")]
    private static partial int MagSetWindowSource(nint hwnd, NativeRect rect);

    [LibraryImport("Magnification.dll")]
    private static partial int MagSetWindowTransform(nint hwnd, ref MagTransform transform);

    [LibraryImport("Magnification.dll")]
    private static partial int MagSetColorEffect(nint hwnd, ref MagColorEffect effect);

    [LibraryImport("Magnification.dll")]
    private static unsafe partial int MagSetWindowFilterList(nint hwnd, uint filterMode, int count, nint* windows);

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint GetModuleHandleW(string? moduleName);

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint CreateWindowExW(
        uint exStyle,
        string className,
        string? windowName,
        uint style,
        int x,
        int y,
        int width,
        int height,
        nint parent,
        nint menu,
        nint instance,
        nint param);

    [LibraryImport("user32.dll")]
    private static partial int DestroyWindow(nint hwnd);

    [LibraryImport("user32.dll")]
    private static partial int EnableWindow(nint hwnd, int enable);

    [LibraryImport("user32.dll")]
    private static partial int ShowWindow(nint hwnd, int command);

    [LibraryImport("user32.dll")]
    private static partial int SetWindowPos(
        nint hwnd,
        nint insertAfter,
        int x,
        int y,
        int width,
        int height,
        uint flags);

    [LibraryImport("user32.dll")]
    private static partial int GetClientRect(nint hwnd, out NativeRect rect);

    [LibraryImport("user32.dll")]
    private static partial int ClientToScreen(nint hwnd, ref NativePoint point);

    [LibraryImport("user32.dll")]
    private static partial int InvalidateRect(nint hwnd, nint rect, int erase);

    [LibraryImport("gdi32.dll")]
    private static partial nint CreateRectRgn(int left, int top, int right, int bottom);

    [LibraryImport("gdi32.dll")]
    private static partial int CombineRgn(nint dest, nint source1, nint source2, int mode);

    [LibraryImport("gdi32.dll")]
    private static partial int DeleteObject(nint obj);

    [LibraryImport("user32.dll")]
    private static partial int SetWindowRgn(nint hwnd, nint region, int redraw);
}
