using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace BlurMask;

/// <summary>
/// AppKit interop без зависимости от net11.0-macos.
/// NSVisualEffectView размывает именно содержимое за окном.
/// LibraryImport используется вместо DllImport, чтобы interop был дружелюбен к Native AOT.
/// </summary>
internal static partial class MacVisualEffectBlur
{
    private const string ObjCLib = "/usr/lib/libobjc.A.dylib";

    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint(double x, double y)
    {
        public double X = x;
        public double Y = y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CGSize(double width, double height)
    {
        public double Width = width;
        public double Height = height;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CGRect(double x, double y, double width, double height)
    {
        public CGPoint Origin = new(x, y);
        public CGSize Size = new(width, height);
    }

    [LibraryImport(ObjCLib, StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr objc_getClass(string name);

    [LibraryImport(ObjCLib, StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr sel_registerName(string name);

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial IntPtr SendIntPtr(IntPtr receiver, IntPtr selector);

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial nuint SendNUInt(IntPtr receiver, IntPtr selector);

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial IntPtr SendIntPtrNUInt(IntPtr receiver, IntPtr selector, nuint value);

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial IntPtr SendIntPtrRect(IntPtr receiver, IntPtr selector, CGRect value);

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial void SendVoidIntPtr(IntPtr receiver, IntPtr selector, IntPtr value);

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial void SendVoidNUInt(IntPtr receiver, IntPtr selector, nuint value);

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial void SendVoidNInt(IntPtr receiver, IntPtr selector, nint value);

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial void SendVoidByte(IntPtr receiver, IntPtr selector, byte value);

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial void SendVoidIntPtrNIntIntPtr(
        IntPtr receiver,
        IntPtr selector,
        IntPtr view,
        nint positioned,
        IntPtr relativeTo);

    public static void TryApply(Window window)
    {
        try
        {
            var handle = window.TryGetPlatformHandle();
            if (handle is null || !string.Equals(handle.HandleDescriptor, "NSWindow", StringComparison.Ordinal))
                return;

            var nsWindow = handle.Handle;
            if (nsWindow == IntPtr.Zero)
                return;

            var contentView = SendIntPtr(nsWindow, Sel("contentView"));
            if (contentView == IntPtr.Zero)
                return;

            // AppKit использует points; для окна Avalonia это логические DIP.
            var frame = new CGRect(
                0,
                0,
                Math.Max(1, window.ClientSize.Width),
                Math.Max(1, window.ClientSize.Height));

            var effectClass = objc_getClass("NSVisualEffectView");
            if (effectClass == IntPtr.Zero)
                return;

            var effect = SendIntPtr(effectClass, Sel("alloc"));
            effect = SendIntPtrRect(effect, Sel("initWithFrame:"), frame);
            if (effect == IntPtr.Zero)
                return;

            // NSVisualEffectBlendingModeBehindWindow = 0.
            SendVoidNInt(effect, Sel("setBlendingMode:"), 0);
            // NSVisualEffectStateActive = 1.
            SendVoidNInt(effect, Sel("setState:"), 1);
            // NSViewWidthSizable (2) | NSViewHeightSizable (16).
            SendVoidNUInt(effect, Sel("setAutoresizingMask:"), 18);

            // BOOL в Objective-C ABI занимает 1 байт на современных macOS.
            SendVoidByte(nsWindow, Sel("setOpaque:"), 0);

            var nsColor = objc_getClass("NSColor");
            if (nsColor != IntPtr.Zero)
            {
                var clearColor = SendIntPtr(nsColor, Sel("clearColor"));
                if (clearColor != IntPtr.Zero)
                    SendVoidIntPtr(nsWindow, Sel("setBackgroundColor:"), clearColor);
            }

            var subviews = SendIntPtr(contentView, Sel("subviews"));
            var count = subviews == IntPtr.Zero ? 0u : SendNUInt(subviews, Sel("count"));
            var firstSubview = count > 0
                ? SendIntPtrNUInt(subviews, Sel("objectAtIndex:"), 0)
                : IntPtr.Zero;

            if (firstSubview != IntPtr.Zero)
            {
                // NSWindowBelow = -1.
                SendVoidIntPtrNIntIntPtr(
                    contentView,
                    Sel("addSubview:positioned:relativeTo:"),
                    effect,
                    -1,
                    firstSubview);
            }
            else
            {
                SendVoidIntPtr(contentView, Sel("addSubview:"), effect);
            }
        }
        catch
        {
            // При несовместимости конкретной версии AppKit/Avalonia остаётся transparency fallback.
        }
    }

    private static IntPtr Sel(string name) => sel_registerName(name);
}
