using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace BlurMask;

// Match writing scale using real font metrics; keep glyph proportions natural.
// Run on the UI thread after inference, where the native font platform is available.
internal static class AiTypesetLayout
{
    private const double Step = 1.25;
    private sealed record Metrics(Rect Bounds, double Baseline);

    internal static double MatchSize(double inkHeight, double glyphHeight, double referenceSize)
    {
        var size = referenceSize * inkHeight / glyphHeight;
        // Group close handwriting sizes instead of producing a different font
        // size for every slight height variation. World scale, not camera zoom.
        return 16 * Math.Pow(Step, Math.Round(Math.Log(size / 16, Step)));
    }

    internal static void Fit(InkMark mark, Rect ink, string? sizingText, IEnumerable<InkMark> nearby)
    {
        if (mark.Tool != InkTool.Text || ink.Height <= 0 || ink.Width <= 0) return;
        var original = Measure(sizingText ?? mark.Text, mark.Font, 64);
        if (original is null || original.Bounds.Height <= 0) return;
        var desiredSize = MatchSize(ink.Height, original.Bounds.Height, 64);
        if (!double.IsFinite(desiredSize) || desiredSize <= 0) return;
        double? alignedBaseline = null;
        // A previously converted nearby word in the same row supplies the
        // shared size/baseline. Skip transformed objects and other font families.
        foreach (var neighbor in nearby.Where(m => m.Tool == InkTool.Text && m.Points.Count == 2 &&
                     m.Transform == Matrix.Identity && m.Font == mark.Font && m.Id != mark.Id)
                 .OrderBy(m => Math.Abs(m.Bounds.Center.X - ink.Center.X)))
        {
            var bounds = new Rect(neighbor.Points[0], neighbor.Points[1]).Normalize();
            if (Math.Abs(bounds.Center.Y - ink.Center.Y) > Math.Max(ink.Height, bounds.Height) * .5 ||
                Math.Max(0, Math.Max(bounds.Left - ink.Right, ink.Left - bounds.Right)) > ink.Height * 2) continue;
            var measured = Measure(neighbor.Text, neighbor.Font, neighbor.FontSize);
            if (measured is null || measured.Bounds.Height <= 0) continue;
            var scale = bounds.Height / measured.Bounds.Height;
            var size = neighbor.FontSize * scale;
            if (size < desiredSize / Step || size > desiredSize * Step) continue;
            desiredSize = size;
            alignedBaseline = bounds.Top + (measured.Baseline - measured.Bounds.Top) * scale;
            break;
        }
        // FontSize's document limit is independent of zoom/world dimensions.
        // A uniform rectangle scale also supports very large writing without
        // distorting the font or increasing the persisted font-size limit.
        mark.FontSize = Math.Clamp(desiredSize, 8, 300);
        var output = Measure(mark.Text, mark.Font, mark.FontSize);
        if (output is null || output.Bounds.Width <= 0 || output.Bounds.Height <= 0) return;
        var uniform = desiredSize / mark.FontSize;
        uniform = Math.Min(uniform, (CanvasGeometry.CoordinateLimit - ink.Left) / output.Bounds.Width);
        uniform = Math.Min(uniform, CanvasGeometry.CoordinateLimit * 2 / output.Bounds.Height);
        if (!double.IsFinite(uniform) || uniform <= 0) return;
        var height = output.Bounds.Height * uniform;
        var top = alignedBaseline is { } baseline ? baseline - (output.Baseline - output.Bounds.Top) * uniform : ink.Center.Y - height / 2;
        top = Math.Clamp(top, -CanvasGeometry.CoordinateLimit, CanvasGeometry.CoordinateLimit - height);
        mark.Points[0] = new(ink.Left, top);
        mark.AddPoint(new(ink.Left + output.Bounds.Width * uniform, top + height));
    }

    private static Metrics? Measure(string text, string font, double size)
    {
        if (font == "STIX Two Math")
        {
            var layout = MathTypesetting.Typeset(text, size);
            return layout is { } math ? new(math.Geometry.Bounds, math.Baseline) : null;
        }
        var formatted = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            InkFonts.Face(font), size, Brushes.Black);
        return formatted.BuildGeometry(default) is { } geometry ? new(geometry.Bounds, formatted.Baseline) : null;
    }
}
