using Avalonia;
using BlurMask.Shared;

namespace BlurMask;

internal sealed class RemoteInkPreview
{
    public InkMarkDto? Dto { get; private set; }
    public InkMark? Mark { get; private set; }
    public bool Apply(LiveFrame frame)
    {
        if (!LiveProtocol.TryMerge(frame, Dto, out var dto)) return false;
        if (frame.Offset == 0)
        {
            var document = new InkDocument(); document.Import([dto!]); Mark = document.Marks[0];
        }
        else foreach (var point in frame.Points ?? frame.Mark!.Points) Mark!.AddPoint(new Point(point.X, point.Y));
        Dto = dto;
        return true;
    }
}
