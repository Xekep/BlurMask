using Avalonia;

namespace BlurMask;

// Retain the original ink behind live text so a later pen lift can complete
// a crossbar, word or formula without treating typeset glyphs as pen strokes.
internal sealed class AiInkBatch(InkDocument document)
{
    private sealed record Rendered(InkMark Mark, InkMark[] Sources, string Value, double FontSize, Point[] Points);
    private readonly List<InkMark> _strokes = [];
    private readonly List<Rendered> _rendered = [];

    internal void Add(InkMark mark)
    {
        if (mark.Points.Count > 20000) { Clear(); return; }
        var current = Snapshot();
        if (current.Length == 0) Clear();
        else
        {
            var previous = current[^1];
            var a = current.Select(stroke => stroke.Bounds).Aggregate((left, right) => left.Union(right)); var b = mark.Bounds;
            var size = Math.Max(2, Math.Max(a.Height, b.Height));
            if (a.Height < a.Width * .15 && b.Height < b.Width * .15)
                size = Math.Max(size, Math.Min(a.Width, b.Width) * .75);
            var gapX = Math.Max(0, Math.Max(a.Left - b.Right, b.Left - a.Right));
            var gapY = Math.Max(0, Math.Max(a.Top - b.Bottom, b.Top - a.Bottom));
            if (previous.Color != mark.Color || previous.FadeSeconds != mark.FadeSeconds ||
                gapX > size * 2 || gapY > size || _strokes.Count >= 256 ||
                _strokes.Sum(stroke => stroke.Points.Count) + mark.Points.Count > 20000)
                Clear();
        }
        _strokes.Add(mark);
    }

    internal InkMark[] Snapshot()
    {
        Prune();
        var visible = document.Marks.Concat(document.FadingMarks).ToHashSet();
        var retained = _rendered.Where(context => visible.Contains(context.Mark) && !context.Sources.Any(visible.Contains))
            .SelectMany(context => context.Sources).ToHashSet();
        return _strokes.Where(source => visible.Contains(source) || retained.Contains(source)).ToArray();
    }

    internal bool Apply(AiInkLayout.Result result)
    {
        if (result.Strokes.Length == 0 || document.HasActiveEdit) return false;
        var sources = result.Strokes.ToHashSet();
        var current = Snapshot();
        if (sources.Count != result.Strokes.Length || sources.Any(source => !current.Contains(source))) return false;
        var targets = new List<InkMark>(); var consumed = new List<Rendered>();
        foreach (var context in _rendered)
        {
            if (!document.Marks.Contains(context.Mark) && !document.FadingMarks.Contains(context.Mark)) continue;
            if (context.Sources.Any(source => document.Marks.Contains(source) || document.FadingMarks.Contains(source))) continue;
            if (!context.Sources.Any(sources.Contains)) continue;
            // A partial reclassification must not delete a complete live word.
            if (!context.Sources.All(sources.Contains)) return false;
            targets.Add(context.Mark); consumed.Add(context);
        }
        var represented = consumed.SelectMany(context => context.Sources).ToHashSet();
        targets.AddRange(result.Strokes.Where(source => !represented.Contains(source)));
        // Avoid adding identical history entries when another nearby component
        // causes an already recognized word to be evaluated again.
        if (targets.Count == 1 && targets[0].Tool == InkTool.Text && targets[0].Same(result.Text)) return true;
        if (!document.ReplaceRecognized(targets, result.Text)) return false;
        // Keep bounded prior associations for document undo/redo. They share
        // the same capped original ink; only visible generations enter inference.
        _rendered.Add(new(result.Text, current.Where(sources.Contains).ToArray(), result.Text.Text, result.Text.FontSize, result.Text.Points.ToArray()));
        if (_rendered.Count > 100) _rendered.RemoveAt(0);
        return true;
    }

    private void Prune()
    {
        _rendered.RemoveAll(context => context.Mark.Text != context.Value || context.Mark.FontSize != context.FontSize || !context.Mark.Points.SequenceEqual(context.Points));
    }

    internal void Clear() { _strokes.Clear(); _rendered.Clear(); }
}
