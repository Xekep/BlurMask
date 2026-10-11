using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;

namespace BlurMask;

internal sealed record TrackedWindow(nuint Id, int ProcessId, PixelRect Bounds, bool Visible, nuint ParentId = 0)
{
    internal nuint StackingId => ParentId == 0 ? Id : ParentId;
}

internal interface IWindowCatalog : IDisposable
{
    TrackedWindow? FindAt(PixelPoint point);
    TrackedWindow? Read(nuint id);
    TrackedWindow? FindChildAt(PixelPoint point) => FindAt(point);
    TrackedWindow? ReadRegion(TrackedWindow region) => Read(region.Id);
    bool PlaceAbove(Window mask, TrackedWindow target);
    void ReleaseMask(Window mask) { }
}

internal static class NativeWindowCatalog
{
    internal static IWindowCatalog? TryCreate()
    {
        try
        {
            if (OperatingSystem.IsWindows()) return new WindowsWindowCatalog();
            if (OperatingSystem.IsMacOS()) return new MacWindowCatalog();
            if (OperatingSystem.IsLinux() && !Program.UseNativeWayland && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")))
                return XcbWindowCatalog.TryCreate();
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or InvalidOperationException) { }
        return null;
    }
}

internal sealed partial class WindowsWindowCatalog : IWindowCatalog
{
    public TrackedWindow? FindAt(PixelPoint point)
    {
        var hwnd = GetTopWindow(0);
        for (var i = 0; hwnd != 0 && i < 10000; i++, hwnd = GetWindow(hwnd, 2))
        {
            var window = Read((nuint)hwnd);
            if (window is { Visible: true } && window.ProcessId != Environment.ProcessId && window.Bounds.Contains(point) &&
                GetWindowTextLengthW(hwnd) > 0) return window;
        }
        return null;
    }
    public TrackedWindow? Read(nuint id)
    {
        var hwnd = (nint)id;
        if (IsWindow(hwnd) == 0 || GetWindowRect(hwnd, out var rect) == 0) return null;
        _ = GetWindowThreadProcessId(hwnd, out var pid);
        var visible = IsWindowVisible(hwnd) != 0 && IsIconic(hwnd) == 0;
        if (DwmGetWindowAttribute(hwnd, 14, out var cloaked, 4) == 0 && cloaked != 0) visible = false;
        return new(id, (int)pid, new PixelRect(rect.Left, rect.Top, Math.Max(0, rect.Right - rect.Left), Math.Max(0, rect.Bottom - rect.Top)), visible);
    }
    public void Dispose() => ReleaseMaskOwners();
    [StructLayout(LayoutKind.Sequential)] private struct NativeRect { public int Left, Top, Right, Bottom; }
    [LibraryImport("user32.dll")] private static partial nint GetTopWindow(nint hwnd);
    [LibraryImport("user32.dll")] private static partial nint GetWindow(nint hwnd, uint command);
    [LibraryImport("user32.dll")] private static partial int IsWindow(nint hwnd);
    [LibraryImport("user32.dll")] private static partial int IsWindowVisible(nint hwnd);
    [LibraryImport("user32.dll")] private static partial int IsIconic(nint hwnd);
    [LibraryImport("user32.dll")] private static partial int GetWindowRect(nint hwnd, out NativeRect rect);
    [LibraryImport("user32.dll")] private static partial uint GetWindowThreadProcessId(nint hwnd, out uint process);
    [LibraryImport("user32.dll")] private static partial int GetWindowTextLengthW(nint hwnd);
    [LibraryImport("dwmapi.dll")] private static partial int DwmGetWindowAttribute(nint hwnd, uint attribute, out int value, uint size);
}

internal sealed partial class MacWindowCatalog : IWindowCatalog
{
    internal static nuint WindowNumber(nint handle) => SendWindowNumber(handle, sel_registerName("windowNumber"));
    private const string Quartz = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";
    private const string CF = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
    private readonly Dictionary<string, nint> _keys = new();
    private nint Key(string name)
    {
        if (!_keys.TryGetValue(name, out var key)) _keys[name] = key = CFStringCreateWithCString(0, name, 0x08000100);
        return key;
    }
    private double Number(nint dictionary, string name)
    {
        var value = CFDictionaryGetValue(dictionary, Key(name));
        return value != 0 && CFNumberGetValue(value, 13, out var result) != 0 ? result : 0;
    }
    private TrackedWindow? FromDictionary(nint dictionary)
    {
        if (dictionary == 0) return null;
        var id = (nuint)Number(dictionary, "kCGWindowNumber");
        var bounds = CFDictionaryGetValue(dictionary, Key("kCGWindowBounds"));
        if (bounds == 0 || id == 0) return null;
        var visible = CFDictionaryGetValue(dictionary, Key("kCGWindowIsOnscreen"));
        return new(id, (int)Number(dictionary, "kCGWindowOwnerPID"), new PixelRect(
            (int)Number(bounds, "X"), (int)Number(bounds, "Y"), Math.Max(0, (int)Number(bounds, "Width")), Math.Max(0, (int)Number(bounds, "Height"))),
            visible != 0 && CFBooleanGetValue(visible) != 0 && Number(dictionary, "kCGWindowLayer") == 0);
    }
    public TrackedWindow? FindAt(PixelPoint point)
    {
        var array = CGWindowListCopyWindowInfo(1 | 16, 0);
        if (array == 0) return null;
        try
        {
            for (nint i = 0; i < CFArrayGetCount(array); i++)
            {
                var window = FromDictionary(CFArrayGetValueAtIndex(array, i));
                if (window is { Visible: true } && window.ProcessId != Environment.ProcessId && window.Bounds.Contains(point)) return window;
            }
            return null;
        }
        finally { CFRelease(array); }
    }
    public TrackedWindow? Read(nuint id)
    {
        var array = CGWindowListCopyWindowInfo(8, (uint)id);
        if (array == 0) return null;
        try { return CFArrayGetCount(array) > 0 ? FromDictionary(CFArrayGetValueAtIndex(array, 0)) : null; }
        finally { CFRelease(array); }
    }
    public void Dispose() { foreach (var key in _keys.Values) CFRelease(key); _keys.Clear(); }
    [LibraryImport(Quartz)] private static partial nint CGWindowListCopyWindowInfo(uint options, uint relative);
    [LibraryImport(CF, StringMarshalling = StringMarshalling.Utf8)] private static partial nint CFStringCreateWithCString(nint allocator, string text, uint encoding);
    [LibraryImport(CF)] private static partial nint CFArrayGetCount(nint array);
    [LibraryImport(CF)] private static partial nint CFArrayGetValueAtIndex(nint array, nint index);
    [LibraryImport(CF)] private static partial nint CFDictionaryGetValue(nint dictionary, nint key);
    [LibraryImport(CF)] private static partial byte CFNumberGetValue(nint number, nint type, out double value);
    [LibraryImport(CF)] private static partial byte CFBooleanGetValue(nint value);
    [LibraryImport(CF)] private static partial void CFRelease(nint value);
    [LibraryImport("/usr/lib/libobjc.A.dylib", StringMarshalling = StringMarshalling.Utf8)] private static partial nint sel_registerName(string name);
    [LibraryImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")] private static partial nuint SendWindowNumber(nint receiver, nint selector);
}

// XCB returns errors as data. Unlike a separate Xlib connection with its default
// global error handler, a disappearing foreign window cannot terminate the app.
internal sealed partial class XcbWindowCatalog(nint connection, uint root) : IWindowCatalog
{
    private readonly nint _connection = connection;
    private readonly uint _root = root;
    private readonly Dictionary<string, uint> _atoms = new();
    internal static XcbWindowCatalog? TryCreate()
    {
        var connection = xcb_connect(0, out var screen);
        if (connection == 0) return null;
        if (xcb_connection_has_error(connection) != 0) { xcb_disconnect(connection); return null; }
        var iterator = xcb_setup_roots_iterator(xcb_get_setup(connection));
        for (var i = 0; i < screen && iterator.Remaining > 0; i++) xcb_screen_next(ref iterator);
        if (iterator.Data == 0) { xcb_disconnect(connection); return null; }
        return new(connection, unchecked((uint)Marshal.ReadInt32(iterator.Data)));
    }
    private uint Atom(string name)
    {
        if (_atoms.TryGetValue(name, out var atom)) return atom;
        var reply = xcb_intern_atom_reply(_connection, xcb_intern_atom(_connection, 0, (ushort)name.Length, name), out var error);
        try { return _atoms[name] = reply == 0 ? 0 : unchecked((uint)Marshal.ReadInt32(reply, 8)); }
        finally { Free(reply, error); }
    }
    private uint[] Property(uint window, string name)
    {
        var atom = Atom(name); if (atom == 0) return [];
        var reply = xcb_get_property_reply(_connection, xcb_get_property(_connection, 0, window, atom, 0, 0, 4096), out var error);
        try
        {
            if (reply == 0 || Marshal.ReadByte(reply, 1) != 32) return [];
            var count = Math.Min(4096, xcb_get_property_value_length(reply) / 4); var data = xcb_get_property_value(reply);
            var result = new uint[count]; for (var i = 0; i < count; i++) result[i] = unchecked((uint)Marshal.ReadInt32(data, i * 4));
            return result;
        }
        finally { Free(reply, error); }
    }
    public TrackedWindow? FindAt(PixelPoint point)
    {
        if (xcb_connection_has_error(_connection) != 0) return null;
        var windows = Property(_root, "_NET_CLIENT_LIST_STACKING");
        if (windows.Length == 0)
        {
            var reply = xcb_query_tree_reply(_connection, xcb_query_tree(_connection, _root), out var error);
            try
            {
                if (reply == 0) return null;
                var count = Math.Min(4096, xcb_query_tree_children_length(reply)); var data = xcb_query_tree_children(reply);
                windows = new uint[count]; for (var i = 0; i < count; i++) windows[i] = unchecked((uint)Marshal.ReadInt32(data, i * 4));
            }
            finally { Free(reply, error); }
        }
        foreach (var id in windows.Reverse())
        {
            var window = Read(id);
            if (window is { Visible: true } && window.ProcessId != Environment.ProcessId && window.ProcessId > 0 && window.Bounds.Contains(point)) return window;
        }
        return null;
    }
    public TrackedWindow? Read(nuint id)
    {
        if (xcb_connection_has_error(_connection) != 0) return null;
        var attributes = xcb_get_window_attributes_reply(_connection, xcb_get_window_attributes(_connection, (uint)id), out var attributeError);
        var geometry = xcb_get_geometry_reply(_connection, xcb_get_geometry(_connection, (uint)id), out var geometryError);
        var coordinates = xcb_translate_coordinates_reply(_connection, xcb_translate_coordinates(_connection, (uint)id, _root, 0, 0), out var coordinateError);
        try
        {
            if (attributes == 0 || geometry == 0 || coordinates == 0) return null;
            var pid = Property((uint)id, "_NET_WM_PID").FirstOrDefault();
            return new(id, (int)pid, new PixelRect(Marshal.ReadInt16(coordinates, 12), Marshal.ReadInt16(coordinates, 14),
                (ushort)Marshal.ReadInt16(geometry, 16), (ushort)Marshal.ReadInt16(geometry, 18)), Marshal.ReadByte(attributes, 26) == 2);
        }
        finally { Free(attributes, attributeError); Free(geometry, geometryError); Free(coordinates, coordinateError); }
    }
    internal void Raise(nuint id)
    {
        if (xcb_connection_has_error(_connection) != 0) return;
        // XCB_CONFIG_WINDOW_STACK_MODE / XCB_STACK_MODE_ABOVE. A checked
        // request keeps a window closing between events from becoming fatal.
        uint above = 0;
        var error = xcb_request_check(_connection,
            xcb_configure_window_checked(_connection, (uint)id, 64, in above));
        if (error != 0) free(error);
    }
    internal unsafe bool SetInkRegion(nuint id, PixelRect bounds, IReadOnlyList<PixelRect> regions)
    {
        if (xcb_connection_has_error(_connection) != 0) return false;
        var rectangles = regions.Select(r => new ShapeRectangle
        {
            X = (short)(r.X - bounds.X), Y = (short)(r.Y - bounds.Y),
            Width = (ushort)r.Width, Height = (ushort)r.Height
        }).ToArray();
        var windows = new List<uint> { (uint)id };
        // GLX/EGL render into a child X window. Its input shape needs the same
        // holes as the parent, otherwise it can still intercept the controls.
        var reply = xcb_query_tree_reply(_connection, xcb_query_tree(_connection, (uint)id), out var treeError);
        try
        {
            if (reply != 0)
            {
                var count = Math.Min(64, xcb_query_tree_children_length(reply));
                var children = xcb_query_tree_children(reply);
                for (var i = 0; i < count; i++) windows.Add(unchecked((uint)Marshal.ReadInt32(children, i * 4)));
            }
        }
        finally { Free(reply, treeError); }
        var success = true;
        fixed (ShapeRectangle* data = rectangles)
            foreach (var window in windows)
                foreach (byte kind in new byte[] { 0, 2 }) // Bounding and Input.
                {
                    var error = xcb_request_check(_connection,
                        xcb_shape_rectangles_checked(_connection, 0, kind, 0, window, 0, 0, (uint)rectangles.Length, data));
                    if (error == 0) continue;
                    success = false;
                    free(error);
                }
        return success;
    }
    private static void Free(nint reply, nint error) { if (reply != 0) free(reply); if (error != 0) free(error); }
    public void Dispose() => xcb_disconnect(_connection);
    [StructLayout(LayoutKind.Sequential)] private struct Cookie { public uint Sequence; }
    [StructLayout(LayoutKind.Sequential)] private struct ShapeRectangle { public short X, Y; public ushort Width, Height; }
    [StructLayout(LayoutKind.Sequential)] private struct ScreenIterator { public nint Data; public int Remaining, Index; }
    [LibraryImport("libc.so.6")] private static partial void free(nint pointer);
    [LibraryImport("libxcb.so.1")] private static partial nint xcb_connect(nint display, out int screen);
    [LibraryImport("libxcb.so.1")] private static partial void xcb_disconnect(nint connection);
    [LibraryImport("libxcb.so.1")] private static partial int xcb_connection_has_error(nint connection);
    [LibraryImport("libxcb.so.1")] private static partial nint xcb_get_setup(nint connection);
    [LibraryImport("libxcb.so.1")] private static partial ScreenIterator xcb_setup_roots_iterator(nint setup);
    [LibraryImport("libxcb.so.1")] private static partial void xcb_screen_next(ref ScreenIterator iterator);
    [LibraryImport("libxcb.so.1", StringMarshalling = StringMarshalling.Utf8)] private static partial Cookie xcb_intern_atom(nint connection, byte onlyIfExists, ushort length, string name);
    [LibraryImport("libxcb.so.1")] private static partial nint xcb_intern_atom_reply(nint connection, Cookie cookie, out nint error);
    [LibraryImport("libxcb.so.1")] private static partial Cookie xcb_get_property(nint connection, byte delete, uint window, uint property, uint type, uint offset, uint length);
    [LibraryImport("libxcb.so.1")] private static partial nint xcb_get_property_reply(nint connection, Cookie cookie, out nint error);
    [LibraryImport("libxcb.so.1")] private static partial int xcb_get_property_value_length(nint reply);
    [LibraryImport("libxcb.so.1")] private static partial nint xcb_get_property_value(nint reply);
    [LibraryImport("libxcb.so.1")] private static partial Cookie xcb_query_tree(nint connection, uint window);
    [LibraryImport("libxcb.so.1")] private static partial nint xcb_query_tree_reply(nint connection, Cookie cookie, out nint error);
    [LibraryImport("libxcb.so.1")] private static partial int xcb_query_tree_children_length(nint reply);
    [LibraryImport("libxcb.so.1")] private static partial nint xcb_query_tree_children(nint reply);
    [LibraryImport("libxcb.so.1")] private static partial Cookie xcb_get_window_attributes(nint connection, uint window);
    [LibraryImport("libxcb.so.1")] private static partial nint xcb_get_window_attributes_reply(nint connection, Cookie cookie, out nint error);
    [LibraryImport("libxcb.so.1")] private static partial Cookie xcb_get_geometry(nint connection, uint window);
    [LibraryImport("libxcb.so.1")] private static partial nint xcb_get_geometry_reply(nint connection, Cookie cookie, out nint error);
    [LibraryImport("libxcb.so.1")] private static partial Cookie xcb_translate_coordinates(nint connection, uint source, uint destination, short x, short y);
    [LibraryImport("libxcb.so.1")] private static partial nint xcb_translate_coordinates_reply(nint connection, Cookie cookie, out nint error);
    [LibraryImport("libxcb.so.1")] private static partial Cookie xcb_configure_window_checked(nint connection, uint window, ushort mask, in uint values);
    [LibraryImport("libxcb.so.1")] private static partial nint xcb_request_check(nint connection, Cookie cookie);
    [LibraryImport("libxcb-shape.so.0")] private static unsafe partial Cookie xcb_shape_rectangles_checked(nint connection,
        byte operation, byte kind, byte ordering, uint window, short x, short y, uint length, ShapeRectangle* rectangles);
}
