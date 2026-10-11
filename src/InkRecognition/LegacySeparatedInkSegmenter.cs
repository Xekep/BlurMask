namespace InkRecognition;

/// <summary>
/// Original, conservative grouping of whole physical strokes in one separated
/// left-to-right line. This proposes complete glyph groups; it neither cuts a
/// connected cursive stroke nor establishes that each group is one character.
/// </summary>
public static class LegacySeparatedInkSegmenter
{
    /// <summary>
    /// Group overlapping horizontal ink intervals, including late dots/accents,
    /// while retaining acquisition order inside each group. Close intervals use
    /// a scale-relative tolerance of 1/32 of the tallest physical stroke.
    /// False means empty, degenerate, multiple-row or unsupported-size geometry.
    /// Malformed input and requests exceeding the bounded work budget throw.
    /// </summary>
    public static bool TrySegment(IReadOnlyList<LegacyInkStroke> strokes,
        out LegacyInkStroke[][] glyphs, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(strokes);
        cancellationToken.ThrowIfCancellationRequested();
        glyphs = [];
        if (strokes.Count == 0) return false;
        if (strokes.Count > 128) throw new ArgumentException("A separated line exceeds 128 physical strokes.", nameof(strokes));
        var intervals = new Interval[strokes.Count];
        int pointCount = 0, tallest = 0, lineTop = short.MaxValue, lineBottom = short.MinValue;
        for (int i = 0; i < strokes.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var stroke = strokes[i]; ArgumentNullException.ThrowIfNull(stroke);
            pointCount += stroke.Points.Length;
            if (pointCount > 32768) throw new ArgumentException("A separated line exceeds 32768 points.", nameof(strokes));
            int left = short.MaxValue, top = short.MaxValue, right = short.MinValue, bottom = short.MinValue;
            foreach (var point in stroke.Points)
            {
                left = Math.Min(left, point.X); right = Math.Max(right, point.X);
                top = Math.Min(top, point.Y); bottom = Math.Max(bottom, point.Y);
            }
            intervals[i] = new(i, left, top, right, bottom, stroke.Points.Length);
            tallest = Math.Max(tallest, bottom - top);
            lineTop = Math.Min(lineTop, top); lineBottom = Math.Max(lineBottom, bottom);
        }
        // Entirely horizontal or point-only input does not establish a text line.
        // No fixed pixel threshold: short coordinates can come from any scale.
        if (tallest == 0 || lineBottom - lineTop > 2 * tallest) return false;
        Array.Sort(intervals, static (a, b) => a.Left != b.Left ? a.Left.CompareTo(b.Left) : a.Index.CompareTo(b.Index));
        int tolerance = tallest / 32;
        var groups = new List<Group>();
        foreach (var interval in intervals)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (groups.Count == 0 || interval.Left > groups[^1].Right + tolerance)
                groups.Add(new(interval));
            else groups[^1].Add(interval);
        }
        if (groups.Count > 64) return false;
        // Separate glyph groups must share a vertical text band. Intervals from
        // disconnected rows remain uncertain instead of inventing reading order.
        int commonTop = groups.Max(g => g.Top), commonBottom = groups.Min(g => g.Bottom);
        if (commonTop >= commonBottom) return false;
        var result = new LegacyInkStroke[groups.Count][];
        for (int i = 0; i < groups.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var group = groups[i];
            if (group.Indices.Count > 15 || group.Points > 4096 || group.Right == group.Left && group.Bottom == group.Top)
                return false;
            group.Indices.Sort();
            result[i] = group.Indices.Select(index => strokes[index]).ToArray();
        }
        glyphs = result;
        return true;
    }

    private readonly record struct Interval(int Index, int Left, int Top, int Right, int Bottom, int Points);
    private sealed class Group
    {
        internal readonly List<int> Indices = [];
        internal int Left, Top, Right, Bottom, Points;
        internal Group(Interval first) { Left = first.Left; Top = first.Top; Right = first.Right; Bottom = first.Bottom; Add(first); }
        internal void Add(Interval stroke)
        {
            Indices.Add(stroke.Index); Left = Math.Min(Left, stroke.Left); Right = Math.Max(Right, stroke.Right);
            Top = Math.Min(Top, stroke.Top); Bottom = Math.Max(Bottom, stroke.Bottom); Points += stroke.Points;
        }
    }
}
