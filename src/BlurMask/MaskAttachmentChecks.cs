using Avalonia;

namespace BlurMask;

internal static class MaskAttachmentChecks
{
    internal static void Run()
    {
        var coverage = new MaskAttachmentCoverage(new(100, 200, 400, 300), new(150, 240, 100, 80), false);
        Require(coverage.Follow(new(300, 100, 400, 300)) == new PixelRect(350, 140, 100, 80) && !coverage.RequiresOpaqueCover,
            "Translation changed the protected region size.");
        Require(coverage.Follow(new(300, 100, 200, 300)) == new PixelRect(300, 100, 200, 300) && coverage.RequiresOpaqueCover,
            "Nonuniform resize did not protect the entire reflowed window.");
        Require(coverage.Follow(new(100, 200, 400, 300)) == new PixelRect(100, 200, 400, 300),
            "Restoring original dimensions prematurely removed resize protection.");
        Require(coverage.Follow(new(-500, -200, 1, 1)) == new PixelRect(-500, -200, 1, 1), "Small/negative bounds broke coverage.");
        var fill = new MaskAttachmentCoverage(new(100, 200, 400, 300), default, true);
        Require(fill.Follow(new(120, 215, 80, 55)) == new PixelRect(120, 215, 80, 55) && fill.RequiresOpaqueCover,
            "Child fill scaled stale coordinates instead of covering its real bounds.");
        var reset = new MaskAttachmentCoverage(new(100, 200, 400, 300), new(160, 250, 60, 40), false);
        Require(reset.Follow(new(100, 200, 400, 300)) == new PixelRect(160, 250, 60, 40) && !reset.RequiresOpaqueCover,
            "Reconfiguration did not replace the old resize guard.");
        Require(AttachedMaskWindows.IsAbove([9, 1, 2, 3], 2, 1) && !AttachedMaskWindows.IsAbove([9, 1, 3, 2], 2, 1) &&
            !AttachedMaskWindows.IsAbove([2, 1, 3], 2, 1), "Foreign occluder or mask below target accepted as matching stack order.");
        Console.WriteLine("Mask attachments: translation, nonuniform resize, latched opaque cover, tiny/negative bounds, child fill and foreign occluder order passed.");
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
