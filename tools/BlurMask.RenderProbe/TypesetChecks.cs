using System.Globalization;
using Avalonia;
using Avalonia.Media;
using BlurMask.Shared;
using InkRecognition;

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
        // Use real embedded glyph metrics: changing just one ascender must not
        // enlarge a row of otherwise equally sized handwriting.
        var bodies = new[] { new Rect(50, 80, 12, 20), new Rect(72, 80, 12, 20), new Rect(94, 80, 12, 20) };
        var mixed = bodies.Append(new Rect(116, 40, 12, 60)).ToArray();
        var ordinary = Create("hello"); AiTypesetLayout.Fit(ordinary, new Rect(50, 80, 80, 20), null, [], bodies); Natural(ordinary);
        var tall = Create("hello"); AiTypesetLayout.Fit(tall, new Rect(50, 40, 80, 60), null, [], mixed); Natural(tall);
        if (ordinary.FontSize != tall.FontSize || Math.Abs(Baseline(ordinary) - Baseline(tall)) > .00001)
            throw new InvalidOperationException("One tall handwriting component changed the row's font size or baseline.");
        var punctuated = mixed.Concat([new Rect(137, 97, 3, 3), new Rect(50, 65, 90, 2)]).ToArray();
        var dots = Create("hello."); AiTypesetLayout.Fit(dots, new Rect(50, 40, 90, 60), null, [], punctuated); Natural(dots);
        if (dots.FontSize != ordinary.FontSize)
            throw new InvalidOperationException("A dot or detached crossbar shrank the writing scale.");
        var bodyEstimate = AiTypesetLayout.WritingBody(mixed)!.Value;
        var scaledEstimate = AiTypesetLayout.WritingBody(mixed.Select(b => new Rect(b.X * 3, b.Y * 3, b.Width * 3, b.Height * 3)).ToArray())!.Value;
        if (scaledEstimate.Height != bodyEstimate.Height * 3 || scaledEstimate.Baseline != bodyEstimate.Baseline * 3)
            throw new InvalidOperationException("Writing body size depends on camera scale rather than ink coordinates.");
        LegacyFontSizeTextSample[] prepared = [new('a', 1, 20, 0, 10), new('b', 1, 20, 10, 20), new('c', 1, 40, 20, 30), new('.', 1, 1, 30, 31)];
        var native = Create("abc."); AiTypesetLayout.Fit(native, ink, null, [], mixed, prepared); Natural(native);
        var x = new FormattedText("x", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, InkFonts.Face("Inter"), 64, Brushes.Black);
        var expectedPrepared = LegacyHandwritingFontSizePolicy.MatchPrepared(64, (float)x.BuildGeometry(default)!.Bounds.Height, prepared);
        if (Math.Abs(native.FontSize - expectedPrepared) > .00001)
            throw new InvalidOperationException("Prepared recognition ranges were sized by minimum/maximum instead of the recovered mean policy.");
        var expression = Create("2+2=", "STIX Two Math"); AiTypesetLayout.Fit(expression, ink, "2+2=", []); Natural(expression);
        var answer = Create("2+2=4", "STIX Two Math"); AiTypesetLayout.Fit(answer, ink, "2+2=", []); Natural(answer);
        if (answer.FontSize != expression.FontSize || answer.Bounds.Width <= expression.Bounds.Width)
            throw new InvalidOperationException("Computed answer changed the expression's font size or did not extend naturally.");
        var hintedMath = Create("2+2=", "STIX Two Math"); AiTypesetLayout.Fit(hintedMath, ink, "2+2=", [], mixed, prepared); Natural(hintedMath);
        if (hintedMath.FontSize != expression.FontSize)
            throw new InvalidOperationException("Text body hints changed a structural formula's size.");
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
        Console.WriteLine("AI typeset: real embedded font metrics, tall-component/dot rejection, prepared range mean, natural aspect, writing scale, neighbor baseline, numeric answer size and peer roundtrip passed.");
    }
}
