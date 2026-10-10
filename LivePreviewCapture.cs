using BlurMask.Shared;

namespace BlurMask;

// Owned by the sender, under its lock. Capture only newly drawn points.
internal static class LivePreviewCapture
{
    public static InkMarkDto Capture(InkMark mark, InkMarkDto? previous)
    {
        if (previous?.Id != mark.Id || mark.Tool is not (InkTool.Pen or InkTool.Highlighter) || previous.Points.Count > mark.Points.Count)
            return InkDocument.ExportMark(mark);
        for (var i = previous.Points.Count; i < mark.Points.Count; i++)
            previous.Points.Add(new(mark.Points[i].X, mark.Points[i].Y));
        return previous;
    }
}
