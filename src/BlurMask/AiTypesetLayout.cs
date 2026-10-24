using System.Globalization;
using System.Text;
using Avalonia;
using Avalonia.Media;
using InkRecognition;

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

    internal static void Fit(InkMark mark, Rect ink, string? sizingText, IEnumerable<InkMark> nearby,
        IReadOnlyList<Rect>? sizingComponents = null,
        IReadOnlyList<LegacyFontSizeTextSample>? preparedFontSizeSamples = null)
    {
        if (mark.Tool != InkTool.Text || ink.Height <= 0 || ink.Width <= 0) return;
        var original = Measure(sizingText ?? mark.Text, mark.Font, 64);
        if (original is null || original.Bounds.Height <= 0) return;
        var desiredSize = MatchSize(ink.Height, original.Bounds.Height, 64);
        double? writingBaseline = null;
        if (mark.Font != "STIX Two Math")
        {
            var body = WritingBody(sizingComponents);
            if (body is { } writing)
            {
                var reference = Measure((sizingText ?? mark.Text).EnumerateRunes().Any(Rune.IsLower) ? "x" : "H", mark.Font, 64);
                if (reference?.Bounds.Height > 0)
                    desiredSize = MatchSize(writing.Height, reference.Bounds.Height, 64);
                writingBaseline = writing.Baseline;
            }
            if (preparedFontSizeSamples is { Count: > 0 and <= 4096 })
            {
                // The recovered native policy averages eligible recognized
                // ranges. It requires their prepared containing-line x-height;
                // component boxes are never relabeled as prepared line data.
                var reference = Measure("x", mark.Font, 64);
                if (reference?.Bounds.Height > 0)
                    desiredSize = LegacyHandwritingFontSizePolicy.MatchPrepared(64,
                        (float)reference.Bounds.Height, preparedFontSizeSamples.ToArray());
            }
        }
        if (!double.IsFinite(desiredSize) || desiredSize <= 0) return;
        double? alignedBaseline = writingBaseline;
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

    //Display policy for detached WORD runs. Actual prepared line height and
    //baseline are used; this is not a recovered native font-face selection.
    internal static double? PositionedDesiredFontSize(double sourceXHeight,double referenceXHeight)
    {
        if(!double.IsFinite(sourceXHeight)||sourceXHeight<=0||!double.IsFinite(referenceXHeight)||referenceXHeight<=0)return null;
        double value=64*sourceXHeight/referenceXHeight;return double.IsFinite(value)&&value>0?value:null;
    }
    internal static bool FitPositioned(InkMark mark,Rect ink,LegacyCompletedGlobalTextWords.PreparedLine line)
    {
        var reference=Measure("x",mark.Font,64);
        if(reference is null||reference.Bounds.Height<=0)return false;
        if(PositionedDesiredFontSize(line.XHeight,reference.Bounds.Height) is not{} desired)return false;
        mark.FontSize=Math.Clamp(desired,8,300);
        var glyph=Measure(mark.Text,mark.Font,mark.FontSize);
        if(glyph is null||glyph.Bounds.Width<=0||glyph.Bounds.Height<=0)return false;
        double scale=desired/mark.FontSize;
        double baseline=line.OriginY+(ink.Left-line.OriginX)*Math.Tan(line.Angle);
        double top=baseline-(glyph.Baseline-glyph.Bounds.Top)*scale;
        double width=glyph.Bounds.Width*scale,height=glyph.Bounds.Height*scale;
        var local=new Rect(ink.Left,top,width,height);
        var world=local.TransformToAABB(mark.Transform);
        if(!double.IsFinite(world.Left)||!double.IsFinite(world.Top)||!double.IsFinite(world.Right)||!double.IsFinite(world.Bottom)||
            world.Left< -CanvasGeometry.CoordinateLimit||world.Top< -CanvasGeometry.CoordinateLimit||
            world.Right>CanvasGeometry.CoordinateLimit||world.Bottom>CanvasGeometry.CoordinateLimit)return false;
        mark.Points[0]=local.TopLeft;mark.AddPoint(local.BottomRight);return true;
    }

    internal static (double Height, double Baseline)? WritingBody(IReadOnlyList<Rect>? components)
    {
        if (components is null || components.Count is < 2 or > 256) return null;
        var candidates = components.Where(b => double.IsFinite(b.Height) && double.IsFinite(b.Width) &&
            double.IsFinite(b.Bottom) && b.Height >= 2 && b.Width >= 0 && b.Height >= b.Width * .15)
            .OrderBy(b => b.Height).ToArray();
        if (candidates.Length < 2) return null;
        // Use the lower body-size cluster. An unusually tall capital/descender
        // cannot raise its threshold; small detached dots/crossbars are excluded.
        double lowerMedian = candidates[(candidates.Length - 1) / 2].Height;
        candidates = candidates.Where(b => b.Height >= lowerMedian * .35).ToArray();
        double height = candidates[(int)Math.Floor((candidates.Length - 1) * .35)].Height;
        var bottoms = candidates.Select(b => b.Bottom).Order().ToArray();
        return (height, bottoms[(bottoms.Length - 1) / 2]);
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
