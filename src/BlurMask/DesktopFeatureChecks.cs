using System.Text;
using Avalonia;
using Avalonia.Controls;

namespace BlurMask;

internal static class DesktopFeatureChecks
{
    internal static void Run()
    {
        byte[] Release(string tag, bool draft = false, bool preview = false) => Encoding.UTF8.GetBytes(
            $"{{\"draft\":{draft.ToString().ToLowerInvariant()},\"prerelease\":{preview.ToString().ToLowerInvariant()},\"tag_name\":\"{tag}\"}}");
        Require(AppUpdates.ReadNewVersion(Release("v1.2.8"), "1.2.7+abc") == "1.2.8", "New stable release was missed.");
        Require(AppUpdates.ReadNewVersion(Release("v1.2.8"), "1.2.8-rc.1+abc") == "1.2.8", "Stable replacement for RC was missed.");
        foreach (var current in new[] { "1.2.8", "1.2.8.0", "1.2.9-rc.1", "invalid" })
            Require(AppUpdates.ReadNewVersion(Release("v1.2.8"), current) is null, "Updater proposed a duplicate, downgrade or unversioned build.");
        foreach (var metadata in new[] { Release("v1.2.9", draft: true), Release("v1.2.9-rc.1", preview: true), Release("bogus"), "[]"u8.ToArray(), "{}"u8.ToArray() })
            Require(AppUpdates.ReadNewVersion(metadata, "1.2.7") is null, "Invalid or unpublished update was offered.");

        var child = new TrackedWindow(2, int.MaxValue, new PixelRect(-100, 20, 80, 60), true, 1);
        var catalog = new FakeCatalog { Next = child };
        using (var picker = new WindowFillPicker(catalog))
        {
            Require(picker.Move(new(-60, 40), force: true) == child, "Child preview did not preserve its native identity/bounds.");
            catalog.Next = child with { Bounds = new(-80, 30, 50, 40) };
            Require(picker.Move(new(-60, 40), force: true)?.Bounds == catalog.Next.Bounds, "Confirmation did not revalidate the resized child.");
            foreach (var bad in new TrackedWindow?[] { null, child with { Visible = false }, child with { ProcessId = Environment.ProcessId }, child with { Bounds = new(0, 0, 0, 0) } })
            {
                catalog.Next = bad;
                Require(picker.Move(new(-60, 40), force: true) is null, "Unavailable/self/empty region remained filled.");
            }
            catalog.Next = child; picker.Move(new(-60, 40), force: true); picker.Clear();
            Require(picker.Region is null, "Cancel left a preview region.");
        }
        Require(catalog.Disposed, "Picker leaked its native catalog.");
        var selection = new ScreenRegionSelection();
        selection.Begin(new(100, 80)); selection.Move(new(-50, -20));
        Require(selection.Complete() == new PixelRect(-50, -20, 150, 100), "Reverse/multiple-screen screenshot selection changed physical pixels.");
        var screen = new Rect(-200, -100, 600, 400);
        var chosen = new Rect(-50, -20, 150, 100);
        var outside = ScreenRegionSelection.Outside(screen, chosen);
        Require(Math.Abs(outside.Sum(r => r.Width * r.Height) + chosen.Width * chosen.Height - screen.Width * screen.Height) < .001,
            "Screenshot shading overlaps or misses its transparent selection.");
        Require(outside.All(r => !r.Intersects(chosen)), "Screenshot shading entered the selected area.");
        selection.Begin(new(0, 0)); selection.Move(new(1, 2));
        Require(selection.Complete() is null, "Accidental click produced an empty screenshot.");
        Console.WriteLine("Desktop update checks: stable/RC versions, metadata guards; window fill: child identity, resize revalidation, cancel and native resource ownership passed.");
    }

    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private sealed class FakeCatalog : IWindowCatalog
    {
        public TrackedWindow? Next;
        public bool Disposed;
        public TrackedWindow? FindAt(PixelPoint point) => throw new InvalidOperationException("Picker must select a child region.");
        public TrackedWindow? FindChildAt(PixelPoint point) => Next;
        public TrackedWindow? Read(nuint id) => Next;
        public bool PlaceAbove(Window mask, TrackedWindow target) => false;
        public void Dispose() => Disposed = true;
    }
}
