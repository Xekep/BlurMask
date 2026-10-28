using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;

namespace NovaPen;

// All Avalonia windows use a headless backend. The Windows helper creates only
// no-activate tool windows at -25000,-25000, never a desktop-sized privacy mask.
internal static partial class Program
{
    internal static bool UseNativeWayland => false;
    private static int _checks, _syntheticMoves;
    private static int Main(string[] args)
    {
        if (args is ["--native-helper"]) { NativeHelper(); return 0; }
        if (args.Length > 1 || args.Length == 1 && args[0] is not ("--headless-only" or "--native-only"))
            throw new ArgumentException("[--headless-only | --native-only]");
        bool headless = args is not ["--native-only"], native = args is not ["--headless-only"];
        int failures = 0;
        void Test(string name, Action action)
        {
            try { action(); Console.WriteLine($"PASS {name}"); }
            catch (Exception error) { failures++; Console.Error.WriteLine($"FAIL {name}: {error.Message}"); }
        }
        if (headless)
        {
            AppBuilder.Configure<Application>().UseHeadless(new AvaloniaHeadlessPlatformOptions()).SetupWithoutStarting();
            Test("headless-controller-rebinding", Tracking);
            Test("headless-child-fallback-and-recovery", ChildTracking);
        }
        if (native)
        {
            if (OperatingSystem.IsWindows()) Test("offscreen-win32-native-picking", NativePicking);
            else Console.WriteLine("SKIP offscreen Win32 picker: Windows is required.");
        }
        Console.WriteLine($"Mask probe: checks={_checks}, failures={failures}, syntheticPositionCallbacks={_syntheticMoves}.");
        return failures == 0 ? 0 : 1;
    }

    private static void MoveHeadless(Window mask, PixelPoint point)
    {
        bool notified = false;
        void Observe(object? sender, PixelPointEventArgs e) { if (e.Point == point) notified = true; }
        mask.PositionChanged += Observe;
        try { mask.Position = point; }
        finally { mask.PositionChanged -= Observe; }
        // A backend may store the position without raising its platform callback.
        // Exercise the real WindowBase -> controller notification, not OnMoved by reflection.
        if (!notified)
        {
            // Avalonia exposes this backend callback internally, so the optional
            // test adapter reaches the interface property through reflection.
            var contract = mask.PlatformImpl!.GetType().GetInterfaces().Single(t => t.Name == "IWindowBaseImpl");
            var callback = contract.GetProperty("PositionChanged", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .GetValue(mask.PlatformImpl) as Action<PixelPoint>;
            (callback ?? throw new InvalidOperationException("Missing platform position callback."))(point);
            _syntheticMoves++;
        }
        Require(mask.Position == point, "Headless platform did not store the requested position.");
    }

    private static DispatcherTimer Timer(MaskWindowAttachment attachment) =>
        (DispatcherTimer)typeof(MaskWindowAttachment).GetField("_timer", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(attachment)!;

    private static void Tracking()
    {
        using var catalog = new FakeCatalog();
        var mask = new NovaPenWindow { Width = 80, Height = 60, Position = new(100, 100), WindowStartupLocation = WindowStartupLocation.Manual };
        mask.Show();
        try
        {
            using var attachment = new MaskWindowAttachment(mask, catalog);
            Thread.Sleep(320); attachment.Update();
            Require(catalog.LastTarget == 101 && catalog.FindCalls == 1, "Initial controller attachment did not query/bind its supplied target.");
            catalog.A = catalog.A with { Bounds = new(200, 100, 500, 500) };
            attachment.Update();
            Require(mask.Position == new PixelPoint(300, 200), "Attached mask did not follow target translation.");
            // FakeCatalog is not WindowsWindowCatalog, so simulate its established
            // slow idle poll. This inspects controller policy, not WinEvent delivery.
            Timer(attachment).Interval = TimeSpan.FromMilliseconds(500);
            catalog.NextTarget = 102;
            MoveHeadless(mask, new(900, 200));
            bool fastCadence = Timer(attachment).Interval == TimeSpan.FromMilliseconds(32);
            attachment.Update();
            Require(catalog.FindCalls == 1, "Manual movement skipped the quiet-period debounce.");
            Thread.Sleep(320); attachment.Update();
            Require(catalog.LastTarget == 102 && catalog.FindCalls == 2, "Dragging the mask did not query and bind the supplied replacement target.");
            catalog.B = catalog.B with { Bounds = new(900, 100, 500, 500) };
            attachment.Update();
            Require(mask.Position == new PixelPoint(1000, 200), "Rebound mask did not follow its new target.");
            Require(fastCadence, "Manual movement did not switch the slow idle poll back to32ms.");
        }
        finally { mask.Close(); }
    }

    private static void ChildTracking()
    {
        using var catalog = new FakeCatalog();
        var filled = new NovaPenWindow { Width = 8, Height = 6, Position = new(920, 140), WindowStartupLocation = WindowStartupLocation.Manual };
        filled.Show();
        try
        {
            using var attachment = new MaskWindowAttachment(filled, catalog, catalog.Child);
            catalog.Child = catalog.Child with { Bounds = new(970, 170, 8, 6) };
            attachment.Update();
            Require(filled.Position == new PixelPoint(970, 170) && filled.Width == 8 && filled.Height == 6,
                "Child fill did not follow the tiny child.");
            catalog.Child = catalog.Child with { Visible = false };
            attachment.Update();
            Require(filled.Position == catalog.B.Bounds.Position && filled.Width == 500 && filled.Height == 500 && filled.PrivacyCover,
                "Unavailable child did not retain the existing fail-closed parent cover.");
            catalog.Child = catalog.Child with { Visible = true, Bounds = new(980, 180, 8, 6) };
            int hiddenReads = catalog.RegionReads;
            attachment.Update();
            Require(catalog.RegionReads > hiddenReads && filled.Position == catalog.Child.Bounds.Position && filled.Width == 8 && filled.Height == 6,
                "A transient child hide permanently promoted its attachment to the parent.");
            catalog.ChildPresent = false;
            attachment.Update();
            Require(filled.Position == catalog.B.Bounds.Position && filled.Width == 500 && filled.Height == 500,
                "Missing child did not retain the fail-closed parent cover.");
            catalog.ChildPresent = true;
            catalog.Child = catalog.Child with { Bounds = new(990, 190, 8, 6) };
            attachment.Update();
            Require(filled.Position == catalog.Child.Bounds.Position && filled.Width == 8 && filled.Height == 6,
                "Restored child identity was not re-queried after a missing-child fallback.");
            catalog.B = catalog.B with { Visible = false };
            attachment.Update();
            Require(!filled.IsVisible && !filled.TargetVisible, "Hidden root did not hide its mask.");
            catalog.B = catalog.B with { Visible = true };
            attachment.Update();
            Require(filled.IsVisible && filled.TargetVisible && filled.Width == 8 && filled.Height == 6, "Restored root did not restore the small child mask.");
        }
        finally { filled.Close(); }
    }

    private static void NativePicking()
    {
        var start = new ProcessStartInfo(Environment.ProcessPath!) { RedirectStandardInput = true, RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
        if (Path.GetFileNameWithoutExtension(Environment.ProcessPath!).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            start.ArgumentList.Add(typeof(Program).Assembly.Location);
        start.ArgumentList.Add("--native-helper");
        using var helper = Process.Start(start) ?? throw new InvalidOperationException("Cannot start offscreen native helper.");
        try
        {
            var line = helper.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
            var handles = JsonSerializer.Deserialize<ulong[]>(line ?? "") ?? throw new InvalidOperationException("Missing helper handles.");
            using var catalog = new WindowsWindowCatalog();
            var point = new PixelPoint(-24975, -24965);
            Require(catalog.FindAt(point)?.Id == (nuint)handles[0], "Captionless application window was skipped or a click-through overlay was attached.");
            var child = catalog.FindChildAt(point);
            Require(child?.Id == (nuint)handles[1] && child.ParentId == (nuint)handles[0] && child.Bounds == new PixelRect(-24980, -24970, 8, 6),
                "Child fill picked the transparent covering window rather than the real native child.");
            Require(catalog.FindAt(new(-24500, -24500)) is null && catalog.FindChildAt(new(-24500, -24500)) is null,
                "Desktop/shell surface was accepted as an application target.");
            Require(catalog.FindAt(new(-25350, -24950))?.Id == (nuint)handles[2],
                "A nonlayered painting-order style was incorrectly rejected as mouse-transparent.");
        }
        finally
        {
            if (!helper.HasExited) { helper.StandardInput.WriteLine("exit"); helper.StandardInput.Flush(); }
            if (!helper.WaitForExit(3000)) { helper.Kill(entireProcessTree: true); helper.WaitForExit(); }
        }
    }

    private static void NativeHelper()
    {
        var windows = new List<nint>();
        nint Create(string cls, string title, uint style, uint ex, int x, int y, int width, int height, nint parent = 0)
        {
            var hwnd = CreateWindowExW(ex, cls, title, style, x, y, width, height, parent, 0, 0, 0);
            if (hwnd == 0) throw new InvalidOperationException("Cannot create offscreen native fixture.");
            windows.Add(hwnd); return hwnd;
        }
        try
        {
            // Register a shell-class fixture locally; no Explorer window is touched.
            var wc = new WindowClass { Size = (uint)Marshal.SizeOf<WindowClass>(), WndProc = Marshal.GetFunctionPointerForDelegate(Procedure), ClassName = "WorkerW" };
            if (RegisterClassExW(ref wc) == 0) throw new InvalidOperationException("Cannot register shell-class fixture.");
            const uint popup = 0x90000000, tool = 0x08000080;
            _ = Create("WorkerW", "Desktop fixture", popup, tool, -24600, -24600, 200, 200);
            var root = Create("STATIC", "", popup, tool, -25000, -25000, 200, 200);
            var child = Create("STATIC", "child", 0x50000000, 0, 20, 30, 8, 6, root);
            // Only LAYERED|TRANSPARENT has the documented mouse-pass-through
            // guarantee. Nonlayered TRANSPARENT affects sibling painting order.
            var overlay = Create("STATIC", "Click-through overlay", popup, tool | 0x80020, -25000, -25000, 200, 200);
            if (SetLayeredWindowAttributes(overlay, 0, 255, 2) == 0)
                throw new InvalidOperationException("Cannot initialize offscreen layered fixture.");
            var painted = Create("STATIC", "Painting-order fixture", popup, tool | 0x20, -25400, -25000, 100, 100);
            Console.WriteLine(JsonSerializer.Serialize(new[] { (ulong)root, (ulong)child, (ulong)painted })); Console.Out.Flush();
            // No cross-process messages are required by the catalog; stdin bounds lifetime.
            _ = Console.ReadLine();
        }
        finally { foreach (var window in windows.AsEnumerable().Reverse()) _ = DestroyWindow(window); }
    }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); _checks++; }
    private sealed class FakeCatalog : IWindowCatalog
    {
        internal TrackedWindow A = new(101, -1, new(0, 0, 500, 500), true);
        internal TrackedWindow B = new(102, -1, new(800, 100, 500, 500), true);
        internal TrackedWindow Child = new(103, -1, new(920, 140, 8, 6), true, 102);
        internal nuint LastTarget, NextTarget = 101;
        internal int FindCalls, RegionReads;
        internal bool ChildPresent = true;
        public TrackedWindow? FindAt(PixelPoint point)
        {
            // Avalonia.Headless PointToScreen currently ignores Window.Position.
            // Supply the intended target and verify calls; real point picking is
            // tested separately against offscreen native HWNDs in another process.
            FindCalls++;
            return NextTarget == A.Id ? A : NextTarget == B.Id ? B : null;
        }
        public TrackedWindow? Read(nuint id) => id == A.Id ? A : id == B.Id ? B : null;
        public TrackedWindow? ReadRegion(TrackedWindow region) { RegionReads++; return region.Id == Child.Id ? ChildPresent ? Child : null : Read(region.Id); }
        public bool PlaceAbove(Window mask, TrackedWindow target) { LastTarget = target.StackingId; return true; }
        public void Dispose() { }
    }
    private delegate nint WindowProcedure(nint window, uint message, nint wParam, nint lParam);
    private static readonly WindowProcedure Procedure = DefWindowProcW;
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WindowClass
    {
        public uint Size, Style; public nint WndProc; public int ClassExtra, WindowExtra;
        public nint Instance, Icon, Cursor, Background, MenuName;
        [MarshalAs(UnmanagedType.LPWStr)] public string ClassName;
        public nint SmallIcon;
    }
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern ushort RegisterClassExW(ref WindowClass value);
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)] private static partial nint CreateWindowExW(uint ex, string cls, string title, uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [LibraryImport("user32.dll")] private static partial int DestroyWindow(nint window);
    [LibraryImport("user32.dll")] private static partial int SetLayeredWindowAttributes(nint window, uint color, byte alpha, uint flags);
    [LibraryImport("user32.dll")] private static partial nint DefWindowProcW(nint window, uint message, nint wParam, nint lParam);
}

// Minimal view for exercising the production attachment controller. Rendering,
// capture backends and the main App are deliberately absent from this process.
internal sealed class NovaPenWindow : Window
{
    internal bool PrivacyCover { get; private set; }
    internal bool TargetVisible { get; private set; } = true;
    internal void SetPrivacyCover(bool enabled) => PrivacyCover = enabled;
    internal void SetAttachedTargetVisible(bool visible) => TargetVisible = visible;
}
