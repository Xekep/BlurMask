using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace BlurMask;

// Run on CI desktops only, never from a headless local probe.
internal static partial class NativeMaskSmokeTest
{
    internal static async Task RunAsync()
    {
        using var catalog = NativeWindowCatalog.TryCreate();
        if (catalog is not (WindowsWindowCatalog or XcbWindowCatalog)) return;
        var root = NewWindow("Mask target", new(450, 180), Brushes.Red);
        var blocker = NewWindow("Mask occluder", new(520, 220), Brushes.Blue);
        var mask = new BlurMaskWindow { WindowStartupLocation = WindowStartupLocation.Manual, Position = new(470, 200),
            Width = 250, Height = 160, Topmost = false, ShowActivated = false };
        mask.SetMode(PrivacyMode.Blackout);
        nint child = 0;
        try
        {
            root.Show(); mask.Show(); blocker.Show(); await Task.Delay(120);
            var target = catalog.Read((nuint)root.TryGetPlatformHandle()!.Handle)!;
            var maskId = (nuint)mask.TryGetPlatformHandle()!.Handle;
            var blockerId = (nuint)blocker.TryGetPlatformHandle()!.Handle;
            Require(catalog.PlaceAbove(mask, target), "Native mask restacking failed."); await Task.Delay(100);
            nuint[] Stack() => catalog is WindowsWindowCatalog windows ? windows.StackingWindows() : ((XcbWindowCatalog)catalog).StackingWindows();
            var stack = Stack();
            Require(Array.IndexOf(stack, target.Id) < Array.IndexOf(stack, maskId) && Array.IndexOf(stack, maskId) < Array.IndexOf(stack, blockerId),
                "The attached mask covered an occluding window or was placed below its target.");
            blocker.Hide(); root.Activate(); await Task.Delay(80);
            Require(catalog.PlaceAbove(mask, target), "Native mask did not return above its exposed target."); await Task.Delay(80);
            Require(Array.IndexOf(Stack(), target.Id) < Array.IndexOf(Stack(), maskId), "Mask did not remain above its exposed target.");
            var point = target.Bounds.Position + new PixelPoint(60, 90);
            if (catalog is WindowsWindowCatalog win)
            {
                child = CreateWindowExW(0, "STATIC", "Privacy region", 0x50000000, 40, 70, 110, 60, (nint)target.Id, 0, 0, 0);
                Require(child != 0, "Native child probe creation failed.");
                await Task.Delay(50);
                var region = win.FindChildRegion(target, point);
                Require(region.Id == (nuint)child && region.ParentId == target.Id, "Point selection did not locate the native child.");
                mask.AttachFilledRegion(new WindowsWindowCatalog(), region); mask.RefreshAttachment(); await Task.Delay(50);
                Require(mask.HasPrivacyCover && mask.Position == region.Bounds.Position && Math.Abs(mask.Width * mask.RenderScaling - region.Bounds.Width) <= 1,
                    "Black fill did not use the native child's actual bounds.");
                _ = MoveWindow(child, 25, 35, 70, 35, 1); await Task.Delay(40); mask.RefreshAttachment();
                var resized = win.ReadRegion(region)!;
                Require(mask.Position == resized.Bounds.Position && Math.Abs(mask.Height * mask.RenderScaling - resized.Bounds.Height) <= 1,
                    "Black child fill did not track nonuniform child resize.");
                _ = DestroyWindow(child); child = 0; mask.RefreshAttachment();
                await Task.Delay(100); mask.RefreshAttachment();
                Require(mask.HasPrivacyCover && mask.Position == target.Bounds.Position && Math.Abs(mask.Width * mask.RenderScaling - target.Bounds.Width) <= 1,
                    "Removing the child exposed replacement content instead of protecting its parent.");
                var nativeMask = catalog.Read(maskId)!;
                Require(nativeMask.Bounds.Size == target.Bounds.Size &&
                    DesktopInkNative.ReadScreenPixel(target.Bounds.Position + new PixelPoint(10, 10)) == 0 &&
                    DesktopInkNative.ReadScreenPixel(target.Bounds.BottomRight - new PixelPoint(10, 10)) == 0,
                    "The rendered parent cover still exposed content after layout/paint settled.");
            }
            else
            {
                var xcb = (XcbWindowCatalog)catalog;
                var region = xcb.FindChildRegion(target, point);
                Require(region.Bounds.Contains(point), "X11 child/client lookup missed its selected point.");
                var fillCatalog = XcbWindowCatalog.TryCreate()!;
                mask.AttachFilledRegion(fillCatalog, region); mask.RefreshAttachment(); await Task.Delay(40);
                Require(mask.HasPrivacyCover && mask.Position == region.Bounds.Position, "X11 selected region was not filled at its actual position.");
            }
            if (OperatingSystem.IsWindows() && Environment.GetEnvironmentVariable("BLURMASK_SMOKE_ARTIFACTS") is { Length: > 0 } directory)
            {
                Directory.CreateDirectory(directory);
                DesktopInkNative.SaveScreenshot(Path.Combine(directory, "attached-mask.png"), target.Bounds);
            }
            var captured = Path.Combine(Path.GetTempPath(), $"BlurMask-native-region-{Guid.NewGuid():N}.png");
            try
            {
                DesktopInkNative.SaveScreenshot(captured, new PixelRect(target.Bounds.Position, new PixelSize(32, 24)));
                using var image = new Avalonia.Media.Imaging.Bitmap(captured);
                Require(image.PixelSize == new PixelSize(32, 24), "Native area capture returned full-screen pixels or an invalid image.");
            }
            finally { File.Delete(captured); }
        }
        finally
        {
            if (child != 0) _ = DestroyWindow(child);
            mask.Close(); blocker.Close(); root.Close();
        }
    }
    private static Window NewWindow(string title, PixelPoint position, IBrush color) => new()
    { Title = title, Width = 320, Height = 220, Position = position, WindowStartupLocation = WindowStartupLocation.Manual,
        WindowDecorations = WindowDecorations.None, ShowInTaskbar = false, Background = color, Content = new Border { Background = color } };
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint CreateWindowExW(uint style, string className, string title, uint flags, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [LibraryImport("user32.dll")] private static partial int MoveWindow(nint window, int x, int y, int width, int height, int repaint);
    [LibraryImport("user32.dll")] private static partial int DestroyWindow(nint window);
}
