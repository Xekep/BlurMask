using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;

namespace NovaPen;

internal sealed partial class WindowsWindowCatalog
{
    private readonly Dictionary<nint, (nint Owner, bool NoActivate)> _maskOwners = [];

    private bool AttachMaskOwner(nint mask, nint root)
    {
        if (!_maskOwners.ContainsKey(mask))
            _maskOwners[mask] = (GetMaskExStyle(mask, -8), (GetMaskExStyle(mask, -20) & 0x08000000) != 0);
        if (GetMaskExStyle(mask, -8) != root)
        {
            _ = SetMaskExStyle(mask, -8, root); // GWLP_HWNDPARENT: owner of a top-level HWND.
            if (GetMaskExStyle(mask, -8) != root) return false;
        }
        return true;
    }
    internal bool HidesWithMinimizedOwner(Window mask, TrackedWindow target) =>
        mask.TryGetPlatformHandle() is { HandleDescriptor: "HWND" } handle &&
        GetMaskExStyle(handle.Handle, -8) == (nint)target.StackingId && IsIconic((nint)target.StackingId) != 0;

    public void ReleaseMask(Window mask)
    {
        if (mask.TryGetPlatformHandle() is { HandleDescriptor: "HWND" } handle) ReleaseMaskOwner(handle.Handle);
    }
    private void ReleaseMaskOwner(nint mask)
    {
        if (!_maskOwners.Remove(mask, out var original) || IsWindow(mask) == 0) return;
        _ = SetMaskExStyle(mask, -8, IsWindow(original.Owner) != 0 ? original.Owner : 0);
        var style = GetMaskExStyle(mask, -20);
        _ = SetMaskExStyle(mask, -20, original.NoActivate ? style | (nint)0x08000000 : style & ~(nint)0x08000000);
    }
    private void ReleaseMaskOwners()
    {
        foreach (var mask in _maskOwners.Keys.ToArray()) ReleaseMaskOwner(mask);
    }
    public TrackedWindow? FindChildAt(PixelPoint point)
    {
        // Share target eligibility with ordinary attachment, then hit test
        // native children within that application (including untitled ones).
        var root = FindAt(point);
        return root is null ? null : FindChildRegion(root, point);
    }
    internal TrackedWindow? FindChildRegion(TrackedWindow root, PixelPoint point)
    {
        var result = NativeWindowHierarchy.FindDeepest(root, point, (region, _) =>
        {
            var clientPoint = new FillPoint { X = point.X, Y = point.Y };
            if (ScreenToClient((nint)region.Id, ref clientPoint) == 0) return NativeChildHit.Invalid;
            // Native hit testing respects child z-order and transparent group-box
            // areas; a rectangular scan can select the enclosing group instead.
            var child = RealChildWindowFromPoint((nint)region.Id, clientPoint);
            if (child == 0 || child == (nint)region.Id) return NativeChildHit.End;
            if (GetAncestor(child, 1) != (nint)region.Id || Read((nuint)child) is not { } next ||
                !TryClientBounds((nint)region.Id, out var clip)) return NativeChildHit.Invalid;
            return new NativeChildHit(true, next with
            {
                ProcessId = root.ProcessId, ParentId = root.Id,
                Bounds = next.Bounds.Intersect(region.Bounds).Intersect(clip)
            });
        });
        // Recheck actual ancestry once. Rechecking every prefix at every
        // level made native queries quadratic in nesting depth.
        var current = result is null ? null : ReadRegion(result);
        return current is { Visible: true } && current.Bounds.Contains(point) ? current : null;
    }
    public TrackedWindow? ReadRegion(TrackedWindow region)
    {
        if (region.ParentId == 0) return Read(region.Id);
        var hwnd = (nint)region.Id;
        var root = Read(region.ParentId);
        var child = Read(region.Id);
        if (root is null || child is null || GetAncestor(hwnd, 2) != (nint)root.Id || root.ProcessId != region.ProcessId) return null;
        if (!NativeWindowHierarchy.TryClipToRoot(region.Id, root.Id, child.Bounds, id =>
        {
            var parent = GetAncestor((nint)id, 1);
            return parent != 0 && TryClientBounds(parent, out var clip)
                ? new NativeParentClip((nuint)parent, clip, IsWindowVisible(parent) != 0) : null;
        }, out var bounds, out var visible)) return null;
        return child with { ProcessId = root.ProcessId, Bounds = bounds, Visible = visible && root.Visible && child.Visible && bounds.Width > 0 && bounds.Height > 0, ParentId = root.Id };
    }
    private static bool TryClientBounds(nint hwnd, out PixelRect bounds)
    {
        bounds = default;
        if (GetClientRect(hwnd, out var rect) == 0) return false;
        int width = rect.Right - rect.Left, height = rect.Bottom - rect.Top;
        if (ClientToScreen(hwnd, ref rect) == 0) return false;
        bounds = new(rect.Left, rect.Top, Math.Max(0, width), Math.Max(0, height));
        return true;
    }
    public bool PlaceAbove(Window mask, TrackedWindow target)
    {
        if (mask.TryGetPlatformHandle() is not { HandleDescriptor: "HWND" } handle || handle.Handle == 0) return false;
        nint hwnd = handle.Handle, root = (nint)target.StackingId;
        if (IsWindow(root) == 0) return false;
        // Owned HWNDs move with their owner in the native z-order transaction.
        // Foreground switches must not wait for an out-of-context event or timer.
        if (!AttachMaskOwner(hwnd, root)) return false;
        const uint flags = 0x0010 | 0x0001 | 0x0002 | 0x0200; // no activation, geometry or owner changes
        var style = GetMaskExStyle(hwnd, -20);
        if ((style & 0x08000000) == 0) // WS_EX_NOACTIVATE: apply once, not every tracking tick.
        {
            _ = SetMaskExStyle(hwnd, -20, style | (nint)0x08000000);
            if (SetMaskWindowPos(hwnd, 0, 0, 0, 0, 0, flags | 0x0004 | 0x0020) == 0) return false;
        }
        bool targetTop = (GetMaskExStyle(root, -20) & 8) != 0;
        if (((style & 8) != 0) != targetTop && SetMaskWindowPos(hwnd, targetTop ? -1 : -2, 0, 0, 0, 0, flags) == 0) return false;
        nint previous = GetWindow(root, 3); // GW_HWNDPREV, immediately above target
        if (previous == hwnd) return true;
        for (nint current = previous; current != 0 && AttachedMaskWindows.SharesTarget((nuint)current, target.StackingId); current = GetWindow(current, 3))
            if (current == hwnd) return true;
        // Inserting below a topmost predecessor would promote a normal mask.
        nint after = previous == 0 || !targetTop && (GetMaskExStyle(previous, -20) & 8) != 0 ? 0 : previous;
        return SetMaskWindowPos(hwnd, after, 0, 0, 0, 0, flags) != 0;
    }
    internal nuint[] StackingWindows()
    {
        var result = new List<nuint>();
        for (nint hwnd = GetTopWindow(0); hwnd != 0 && result.Count < 10000; hwnd = GetWindow(hwnd, 2)) result.Add((nuint)hwnd);
        result.Reverse(); return result.ToArray();
    }
    [LibraryImport("user32.dll")] private static partial nint GetAncestor(nint window, uint flags);
    [StructLayout(LayoutKind.Sequential)] private struct FillPoint { public int X, Y; }
    [LibraryImport("user32.dll")] private static partial int ScreenToClient(nint window, ref FillPoint point);
    [LibraryImport("user32.dll")] private static partial nint RealChildWindowFromPoint(nint parent, FillPoint point);
    [LibraryImport("user32.dll")] private static partial int GetClientRect(nint window, out NativeRect rectangle);
    [LibraryImport("user32.dll")] private static partial int ClientToScreen(nint window, ref NativeRect point);
    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW")] private static partial nint GetMaskExStyle(nint window, int index);
    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW")] private static partial nint SetMaskExStyle(nint window, int index, nint value);
    [LibraryImport("user32.dll", EntryPoint = "SetWindowPos")] private static partial int SetMaskWindowPos(nint window, nint after, int x, int y, int width, int height, uint flags);
}

internal sealed partial class MacWindowCatalog
{
    // Other applications' NSViews are not public WindowServer windows. Whole
    // window fill remains available without Accessibility permission.
    public TrackedWindow? FindChildAt(PixelPoint point) => FindAt(point);
    public bool PlaceAbove(Window mask, TrackedWindow target)
    {
        if (mask.TryGetPlatformHandle() is not { HandleDescriptor: "NSWindow" } handle || handle.Handle == 0) return false;
        var array = CGWindowListCopyWindowInfo(1 | 16, 0);
        if (array == 0) return false;
        var windows = new List<nuint>();
        try
        {
            for (nint i = 0; i < CFArrayGetCount(array); i++)
            {
                var window = FromDictionary(CFArrayGetValueAtIndex(array, i));
                if (window is null) continue;
                windows.Add(window.Id);
            }
        }
        finally { CFRelease(array); }
        if (!windows.Contains(target.StackingId)) return false;
        windows.Reverse();
        if (!AttachedMaskWindows.IsAbove(windows, WindowNumber(handle.Handle), target.StackingId))
            SendMaskOrder(handle.Handle, sel_registerName("orderWindow:relativeTo:"), 1, (nint)target.StackingId);
        return true;
    }
    [LibraryImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static partial void SendMaskOrder(nint receiver, nint selector, nint order, nint relative);
}

internal sealed partial class XcbWindowCatalog
{
    private uint[] Children(uint window) => TryChildren(window, out var result) ? result : [];
    private bool TryChildren(uint window, out uint[] result)
    {
        result = [];
        var reply = xcb_query_tree_reply(_connection, xcb_query_tree(_connection, window), out var error);
        try
        {
            if (reply == 0) return false;
            int count = xcb_query_tree_children_length(reply);
            // Do not truncate bottom-to-top stacking data and then select an
            // older child as if it were the topmost window under the pointer.
            if (count < 0 || count > WindowTraversalBudget.MaximumWindows) return false;
            var data = xcb_query_tree_children(reply);
            result = new uint[count]; for (int i = 0; i < count; i++) result[i] = unchecked((uint)Marshal.ReadInt32(data, i * 4));
            return true;
        }
        finally { Free(reply, error); }
    }
    private uint Parent(uint window)
    {
        var reply = xcb_query_tree_reply(_connection, xcb_query_tree(_connection, window), out var error);
        try { return reply == 0 ? 0 : unchecked((uint)Marshal.ReadInt32(reply, 12)); }
        finally { Free(reply, error); }
    }
    public TrackedWindow? FindChildAt(PixelPoint point)
    {
        var root = FindAt(point); if (root is null) return null;
        return FindChildRegion(root, point);
    }
    internal TrackedWindow? FindChildRegion(TrackedWindow root, PixelPoint point)
    {
        var result = NativeWindowHierarchy.FindDeepest(root, point, (region, budget) =>
        {
            if (!TryChildren((uint)region.Id, out var children)) return NativeChildHit.Invalid;
            foreach (var id in children.Reverse())
            {
                if (!budget.Spend()) return NativeChildHit.Invalid;
                var child = Read(id);
                if (child is null) return NativeChildHit.Invalid;
                if (!child.Visible) continue;
                var bounds = child.Bounds.Intersect(region.Bounds);
                if (bounds.Contains(point)) return new NativeChildHit(true,
                    child with { ProcessId = root.ProcessId, Bounds = bounds, ParentId = root.Id });
            }
            return NativeChildHit.End;
        });
        var current = result is null ? null : ReadRegion(result);
        return current is { Visible: true } && current.Bounds.Contains(point) ? current : null;
    }
    internal nuint[] StackingWindows()
    {
        var windows = Property(_root, "_NET_CLIENT_LIST_STACKING");
        if (windows.Length == 0) windows = Children(_root);
        return windows.Select(id => (nuint)id).ToArray();
    }
    public TrackedWindow? ReadRegion(TrackedWindow region)
    {
        if (region.ParentId == 0) return Read(region.Id);
        var root = Read(region.ParentId); var child = Read(region.Id);
        if (root is null || child is null || root.ProcessId != region.ProcessId) return null;
        if (!NativeWindowHierarchy.TryClipToRoot(region.Id, root.Id, child.Bounds, id =>
        {
            var parentId = Parent((uint)id);
            var parent = parentId == 0 || parentId == _root ? null : Read(parentId);
            return parent is null ? null : new NativeParentClip(parentId, parent.Bounds, parent.Visible);
        }, out var bounds, out var visible)) return null;
        return child with { ProcessId = root.ProcessId, Bounds = bounds, Visible = visible && root.Visible && child.Visible && bounds.Width > 0 && bounds.Height > 0, ParentId = root.Id };
    }
    private uint Frame(uint window)
    {
        for (int depth = 0; depth < 32; depth++)
        {
            uint parent = Parent(window); if (parent == _root) return window;
            if (parent == 0) return 0; window = parent;
        }
        return 0;
    }
    public unsafe bool PlaceAbove(Window mask, TrackedWindow target)
    {
        if (xcb_connection_has_error(_connection) != 0 || mask.TryGetPlatformHandle() is not { HandleDescriptor: "XID" } handle) return false;
        uint id = (uint)handle.Handle, sibling = (uint)target.StackingId;
        bool topmost = Property(sibling, "_NET_WM_STATE").Contains(Atom("_NET_WM_STATE_ABOVE"));
        if (mask.Topmost != topmost) mask.Topmost = topmost;
        var windows = Property(_root, "_NET_CLIENT_LIST_STACKING");
        if (AttachedMaskWindows.IsAbove(windows.Select(window => (nuint)window).ToArray(), id, sibling)) return true;
        if (Property(_root, "_NET_SUPPORTED").Contains(Atom("_NET_RESTACK_WINDOW")))
        {
            var message = new MaskClientMessage { Type = 33, Format = 32, Window = id, MessageType = Atom("_NET_RESTACK_WINDOW"), Source = 2, Sibling = sibling, StackMode = 0 };
            var error = xcb_request_check(_connection, SendMaskEvent(_connection, 0, _root, 0x180000, &message));
            if (error != 0) { free(error); return false; }
        }
        else
        {
            uint frame = Frame(id), siblingFrame = Frame(sibling);
            if (frame == 0 || siblingFrame == 0) return false;
            uint* values = stackalloc uint[2] { siblingFrame, 0 };
            var error = xcb_request_check(_connection, ConfigureMaskStack(_connection, frame, 32 | 64, values));
            if (error != 0) { free(error); return false; }
        }
        _ = xcb_flush(_connection); return true;
    }
    [StructLayout(LayoutKind.Sequential)] private struct MaskClientMessage
    { public byte Type, Format; public ushort Sequence; public uint Window, MessageType, Source, Sibling, StackMode, Unused1, Unused2; }
    [LibraryImport("libxcb.so.1", EntryPoint = "xcb_send_event_checked")]
    private static unsafe partial Cookie SendMaskEvent(nint connection, byte propagate, uint destination, uint mask, MaskClientMessage* message);
    [LibraryImport("libxcb.so.1", EntryPoint = "xcb_configure_window_checked")]
    private static unsafe partial Cookie ConfigureMaskStack(nint connection, uint window, ushort mask, uint* values);
    [LibraryImport("libxcb.so.1")] private static partial int xcb_flush(nint connection);
}
