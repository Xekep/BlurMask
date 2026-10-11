using System.Globalization;
using Avalonia;
using Avalonia.Media;
using BlurMask.Shared;

namespace BlurMask;

internal static class TypesetChecks
{
    internal static void Run()
    {
        var ink = new Rect(50, 50, 400, 42);
        InkMark Create(string value, string font = "Inter")
        {
            var mark = new InkMark(InkTool.Text, Colors.Black, 2, ink.TopLeft) { Text = value, Font = font, FontSize = 42 };
            mark.AddPoint(ink.BottomRight); return mark;
        }
        (Rect Bounds, double Baseline) Metrics(InkMark mark)
        {
            if (mark.Font == "STIX Two Math")
            {
                var math = MathTypesetting.Typeset(mark.Text, mark.FontSize)!.Value;
                return (math.Geometry.Bounds, math.Baseline);
            }
            var text = new FormattedText(mark.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                InkFonts.Face(mark.Font), mark.FontSize, Brushes.Black);
            return (text.BuildGeometry(default)!.Bounds, text.Baseline);
        }
        double Baseline(InkMark mark)
        {
            var metrics = Metrics(mark); var box = new Rect(mark.Points[0], mark.Points[1]);
            return box.Top + (metrics.Baseline - metrics.Bounds.Top) * box.Height / metrics.Bounds.Height;
        }
        void Natural(InkMark mark)
        {
            var glyph = Metrics(mark).Bounds; var box = new Rect(mark.Points[0], mark.Points[1]);
            if (Math.Abs(box.Width / glyph.Width - box.Height / glyph.Height) > .00001 || !BoardValidation.Valid(InkDocument.ExportMark(mark)))
                throw new InvalidOperationException("AI typeset distorts font proportions or violates the shared document.");
        }
        var word = Create("hello"); AiTypesetLayout.Fit(word, ink, null, []); Natural(word);
        if (Math.Abs(word.Bounds.Height / ink.Height - 1) > .13 || word.Bounds.Width >= ink.Width * .9)
            throw new InvalidOperationException("AI word was stretched to the handwritten width or lost the writing scale.");
        var nextInk = new Rect(word.Bounds.Right + 8, ink.Y + 2, 400, ink.Height * 1.04);
        var next = Create("world"); AiTypesetLayout.Fit(next, nextInk, null, [word]); Natural(next);
        if (next.FontSize != word.FontSize || Math.Abs(Baseline(next) - Baseline(word)) > .00001)
            throw new InvalidOperationException("Nearby words did not share font size and baseline.");
        var expression = Create("2+2=", "STIX Two Math"); AiTypesetLayout.Fit(expression, ink, "2+2=", []); Natural(expression);
        var answer = Create("2+2=4", "STIX Two Math"); AiTypesetLayout.Fit(answer, ink, "2+2=", []); Natural(answer);
        if (answer.FontSize != expression.FontSize || answer.Bounds.Width <= expression.Bounds.Width)
            throw new InvalidOperationException("Computed answer changed the expression's font size or did not extend naturally.");
        var fraction = Create("\\frac{1}{2}=0.5", "STIX Two Math");
        AiTypesetLayout.Fit(fraction, ink, "\\frac{1}{2}=", []); Natural(fraction);
        foreach (var (source, result) in new[]
        {
            (@"\int x^2\mathrm{d}x=", @"\frac{x^{3}}{3}+C"),
            (@"\frac{d}{dx}\sin(x)=", @"\cos x"),
            (@"x^2-1=0\Rightarrow ", @"x\in \left\{-1,1\right\}"),
            (@"\inv\begin{matrix}1&2\\3&4\end{matrix}=", @"\left[\begin{array}{cc}-2&1\\\frac{3}{2}&-\frac{1}{2}\end{array}\right]")
        })
        {
            var solved = Create(source + result, "STIX Two Math");
            AiTypesetLayout.Fit(solved, ink, source, []); Natural(solved);
            if (Metrics(solved).Bounds.Width <= 0) throw new InvalidOperationException("CAS result has no visible typeset geometry.");
        }
        var large = Create("Hello"); AiTypesetLayout.Fit(large, new Rect(0, 0, 3000, 1200), null, []); Natural(large);
        if (large.FontSize != 300 || Math.Abs(large.Bounds.Height / 1200 - 1) > .13)
            throw new InvalidOperationException("Large world-space lettering lost its visual size at the font-size limit.");
        var edge = Create("hello"); AiTypesetLayout.Fit(edge, new Rect(99990, 99000, 10, 900), null, []); Natural(edge);
        var peer = new InkDocument(); peer.Import([InkDocument.ExportMark(answer)]);
        if (!peer.Marks[0].Same(answer)) throw new InvalidOperationException("Peer changed the typeset size or natural aspect ratio.");
        Console.WriteLine("AI typeset: real embedded font metrics, natural aspect, writing scale, neighbor baseline, numeric answer size and peer roundtrip passed.");
    }
}
