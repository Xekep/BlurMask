using Avalonia;

namespace NovaPen;

// A window resize can reflow arbitrary content. Relative coordinates cannot
// identify that content, so ordinary attachments latch a full opaque cover.
internal sealed class MaskAttachmentCoverage(PixelRect target, PixelRect mask, bool fillRegion)
{
    internal bool RequiresOpaqueCover { get; private set; } = fillRegion;
    internal PixelRect Follow(PixelRect current)
    {
        if (current.Size != target.Size) RequiresOpaqueCover = true;
        return RequiresOpaqueCover ? current : new PixelRect(
            current.X + mask.X - target.X, current.Y + mask.Y - target.Y, mask.Width, mask.Height);
    }
}
