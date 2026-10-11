using Avalonia;
using System.Text;

namespace BlurMask;

// Batch layout is separate from inference: pen lifts do not define character boundaries.
internal static class AiInkLayout
{
    internal sealed record Result(InkMark[] Strokes, InkMark Text);
    private sealed class Group(InkMark stroke)
    {
        internal readonly List<InkMark> Strokes = [stroke];
        internal Rect Bounds = stroke.Bounds;
        internal void Merge(Group other) { Strokes.AddRange(other.Strokes); Bounds = Bounds.Union(other.Bounds); }
    }
    internal static IReadOnlyList<Result> Recognize(IReadOnlyList<InkMark> strokes, string language, string font = "Inter")
    {
        if (strokes.Count == 0 || strokes.Count > 256 || strokes.Sum(m => m.Points.Count) > 20000) return [];
        var groups = strokes.Select(s => new Group(s)).ToList();
        // Join components sharing a horizontal interval (crossbars, dots, =, +, etc.).
        // Keep separate rows apart, even when their X positions are identical.
        for (var i = 0; i < groups.Count; i++)
            for (var j = i + 1; j < groups.Count; j++)
            {
                var a = groups[i].Bounds; var b = groups[j].Bounds;
                if (groups[i].Strokes[0].Color != groups[j].Strokes[0].Color || groups[i].Strokes[0].FadeSeconds != groups[j].Strokes[0].FadeSeconds) continue;
                var overlap = Math.Min(a.Right, b.Right) - Math.Max(a.Left, b.Left);
                var distance = Math.Abs(a.Center.Y - b.Center.Y);
                var horizontal = a.Height < a.Width * .15 && b.Height < b.Width * .15;
                var tolerance = Math.Max(a.Height, b.Height) * .85;
                if (horizontal) tolerance = Math.Max(tolerance, Math.Min(a.Width, b.Width) * .75);
                if (overlap < Math.Min(a.Width, b.Width) * .1 || distance > tolerance) continue;
                groups[i].Merge(groups[j]); groups.RemoveAt(j); i = -1; break;
            }
        var rows = new List<List<Group>>();
        foreach (var group in groups.OrderByDescending(g => g.Bounds.Height))
        {
            var row = rows.FirstOrDefault(r => r[0].Strokes[0].Color == group.Strokes[0].Color && r[0].Strokes[0].FadeSeconds == group.Strokes[0].FadeSeconds &&
                Math.Abs(r[0].Bounds.Center.Y - group.Bounds.Center.Y) <=
                Math.Max(r[0].Bounds.Height, group.Bounds.Height) * .55);
            if (row is null) rows.Add([group]); else row.Add(group);
        }
        var results = new List<Result>();
        foreach (var row in rows)
        {
            row.Sort((a, b) => a.Bounds.X.CompareTo(b.Bounds.X));
            var bounds = row.Select(g => g.Bounds).Aggregate((a, b) => a.Union(b));
            if (bounds.Width < 2 || bounds.Height < 2 || bounds.Left < -100000 || bounds.Top < -100000 || bounds.Right > 100000 || bounds.Bottom > 100000) continue;
            var value = new StringBuilder(); var uncertain = false;
            // Keep confident digits/operators separate from handwritten words so a text
            // recognizer cannot silently substitute a mathematical sign.
            var predictions = row.Select(g => AiSymbolNetwork.Instance.Value.Recognize(AiPenRecognizer.Rasterize(g.Strokes, g.Bounds))).ToArray();
            bool Connected(int index) => row[index].Bounds.Width > row[index].Bounds.Height * 1.5 && row[index].Strokes.Any(s => s.Points.Count > 8);
            bool Symbol(int index) => !Connected(index) && predictions[index].Confidence >= .75f && !char.IsLetter(predictions[index].Symbol);
            for (var i = 0; i < row.Count;)
            {
                if (i > 0 && row[i].Bounds.Left - row[i - 1].Bounds.Right > bounds.Height * .65) value.Append(' ');
                if (Symbol(i)) { value.Append(predictions[i].Symbol); i++; continue; }
                var end = i + 1;
                while (end < row.Count && !Symbol(end) && row[end].Bounds.Left - row[end - 1].Bounds.Right <= bounds.Height * .65) end++;
                var part = row.Skip(i).Take(end - i).SelectMany(g => g.Strokes).ToArray();
                var area = part.Select(s => s.Bounds).Aggregate((a, b) => a.Union(b));
                var line = RecognizeWord(part, area, language);
                if (line.Confidence < .75f || string.IsNullOrWhiteSpace(line.Text)) { uncertain = true; break; }
                value.Append(line.Text); i = end;
            }
            var sources = row.SelectMany(g => g.Strokes).ToArray(); var first = sources[0];
            // Retain the entire handwritten row when any component is uncertain.
            if (uncertain) continue;
            var text = new InkMark(InkTool.Text, first.Color, first.Width, bounds.TopLeft)
                { Id = first.Id, Text = value.ToString(), Font = font, FontSize = Math.Clamp(bounds.Height, 8, 300), FadeSeconds = first.FadeSeconds };
            text.AddPoint(bounds.BottomRight); results.Add(new(sources, text));
        }
        return results;
    }
    internal static (string Text, float Confidence) RecognizeWord(IReadOnlyList<InkMark> strokes, Rect bounds, string language)
    {
        if (language == "ru") return AiHandwritingNetwork.Instance.Value.Recognize(AiPenRecognizer.RasterizeLine(strokes, bounds));
        var english = AiHandwritingNetwork.English.Value.Recognize(AiPenRecognizer.RasterizeLine(strokes, bounds, 32, 128));
        if (language == "en" || (english.Confidence >= .97f && english.Text.Length >= 3)) return english;
        var russian = AiHandwritingNetwork.Instance.Value.Recognize(AiPenRecognizer.RasterizeLine(strokes, bounds));
        // Very short lookalikes have no unique script. Use the interface only as a tie-break.
        if (Math.Abs(russian.Confidence - english.Confidence) < .08f)
            return UiText.Language == "ru" ? russian : english;
        return russian.Confidence > english.Confidence ? russian : english;
    }
}
