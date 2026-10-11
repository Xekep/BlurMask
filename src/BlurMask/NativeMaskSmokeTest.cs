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
        nint child = 0, tinyChild = 0;
        BlurMaskWindow? secondMask = null;
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
                catalog.ReleaseMask(mask);
                mask.AttachFilledRegion(new WindowsWindowCatalog(), region); mask.RefreshAttachment(); await Task.Delay(50);
                Require(mask.HasPrivacyCover && mask.Position == region.Bounds.Position && Math.Abs(mask.Width * mask.RenderScaling - region.Bounds.Width) <= 1,
                    "Black fill did not use the native child's actual bounds.");
                _ = MoveWindow(child, 25, 35, 70, 35, 1); await Task.Delay(40); mask.RefreshAttachment();
                var resized = win.ReadRegion(region)!;
                Require(mask.Position == resized.Bounds.Position && Math.Abs(mask.Height * mask.RenderScaling - resized.Bounds.Height) <= 1,
                    "Black child fill did not track nonuniform child resize.");
                tinyChild = CreateWindowExW(0, "STATIC", "Tiny private region", 0x50000000, 130, 90, 8, 6, (nint)target.Id, 0, 0, 0);
                Require(tinyChild != 0, "Tiny child probe creation failed.");
                var tinyRegion = win.ReadRegion(new((nuint)tinyChild, target.ProcessId, default, true, target.Id))!;
                secondMask = new BlurMaskWindow { ShowActivated = false, Topmost = false, WindowStartupLocation = WindowStartupLocation.Manual };
                secondMask.Show(); secondMask.AttachFilledRegion(new WindowsWindowCatalog(), tinyRegion); secondMask.RefreshAttachment();
                await Task.Delay(60);
                var secondId = (nuint)secondMask.TryGetPlatformHandle()!.Handle;
                Require(Math.Abs(secondMask.MinWidth * secondMask.RenderScaling - 1) <= .01 &&
                    Math.Abs(secondMask.MinHeight * secondMask.RenderScaling - 1) <= .01 &&
                    Math.Abs(secondMask.Width * secondMask.RenderScaling - 8) <= .01 &&
                    Math.Abs(secondMask.Height * secondMask.RenderScaling - 6) <= .01,
                    $"Tiny child fill was enlarged: {secondMask.Width}×{secondMask.Height} DIP, scale {secondMask.RenderScaling}, minimum {secondMask.MinWidth}×{secondMask.MinHeight}.");
                Require(GetOwner((nint)maskId, 4) == (nint)target.Id && GetOwner((nint)secondId, 4) == (nint)target.Id,
                    "Attached masks have no native owner, leaving foreground changes dependent on callbacks.");
                blocker.Show();
                // No await/dispatcher callback/RefreshAttachment between restacking and assertions.
                // Both fills must follow the owner's native z-order transaction immediately.
                for (int i = 0; i < 20; i++)
                {
                    Require(RaiseWindow((nint)blockerId, 0, 0, 0, 0, 0, 0x0013) != 0, "Could not raise native occluder.");
                    stack = Stack();
                    Require(Array.IndexOf(stack, maskId) < Array.IndexOf(stack, blockerId) &&
                        Array.IndexOf(stack, secondId) < Array.IndexOf(stack, blockerId), "Attached fills covered a foreground occluder.");
                    Require(RaiseWindow((nint)target.Id, 0, 0, 0, 0, 0, 0x0013) != 0, "Could not raise native target.");
                    stack = Stack();
                    Require(Array.IndexOf(stack, target.Id) < Array.IndexOf(stack, maskId) &&
                        Array.IndexOf(stack, target.Id) < Array.IndexOf(stack, secondId), "Owner promotion exposed a fill before its tracking callback.");
                }
                root.WindowState = WindowState.Minimized; await Task.Delay(60); mask.RefreshAttachment(); secondMask.RefreshAttachment();
                Require(IsVisible((nint)maskId) == 0 && IsVisible((nint)secondId) == 0,
                    "Native owner minimization left attached fills on screen.");
                root.WindowState = WindowState.Normal; root.Activate(); await Task.Delay(80);
                Require(IsVisible((nint)maskId) != 0 && IsVisible((nint)secondId) != 0 &&
                    GetOwner((nint)maskId, 4) == (nint)target.Id && GetOwner((nint)secondId, 4) == (nint)target.Id,
                    "Restoring the native owner lost a fill or reset its ownership.");
                secondMask.Close(); secondMask = null; _ = DestroyWindow(tinyChild); tinyChild = 0;
                blocker.Hide();
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
            if (tinyChild != 0) _ = DestroyWindow(tinyChild);
            secondMask?.Close(); catalog.ReleaseMask(mask);
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
    [LibraryImport("user32.dll", EntryPoint = "GetWindow")] private static partial nint GetOwner(nint window, uint command);
    [LibraryImport("user32.dll", EntryPoint = "SetWindowPos")] private static partial int RaiseWindow(nint window, nint after, int x, int y, int width, int height, uint flags);
    [LibraryImport("user32.dll", EntryPoint = "IsWindowVisible")] private static partial int IsVisible(nint window);
}
