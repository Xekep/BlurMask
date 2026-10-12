namespace InkRecognition;

/// <summary>Portable index replacing a native2DB line-table source-record pointer; range end is exclusive.</summary>
public readonly record struct LegacyPreparedResultLineRange(int SourceLineIndex, int CharacterStart, int CharacterEnd);

/// <summary>
/// Text050660's prepared source2DA F8 -> result2DB58 line owner/range binding and Document177590 SDK narrowing.
/// Actual source-line metrics and derived character source-line IDs remain required prepared inputs.
/// </summary>
public sealed class LegacyPreparedResultLineMetrics
{
    private readonly double[] _sourceLineXHeights;
    private readonly int[] _characterLineIds;
    private readonly LegacyPreparedResultLineRange[] _lines;
    public int LineCount => _lines.Length;
    public int CharacterCount => _characterLineIds.Length;

    private LegacyPreparedResultLineMetrics(double[] heights, int[] ids, LegacyPreparedResultLineRange[] lines)
    { _sourceLineXHeights = heights; _characterLineIds = ids; _lines = lines; }

    public static LegacyPreparedResultLineMetrics Create(ReadOnlySpan<double> sourceLineXHeights,
        ReadOnlySpan<int> derivedCharacterSourceLineIds, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (sourceLineXHeights.Length > 4096 || derivedCharacterSourceLineIds.Length > 65536)
            throw new ArgumentException("Explicit prepared result-line budgets exceeded.");
        foreach (double h in sourceLineXHeights)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!double.IsFinite(h)) throw new ArgumentException("Finite prepared source-line metrics required.");
        }
        foreach (int id in derivedCharacterSourceLineIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (id < 0) throw new ArgumentException("Source-line IDs must already be derived, including native automatic-ID assignment.");
        }
        var heights = sourceLineXHeights.ToArray();
        var ids = derivedCharacterSourceLineIds.ToArray();
        // Exact native forward pass: lower only the immediately preceding ID; do not globally sort/repair.
        for (int i = 1; i < ids.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (ids[i] < ids[i - 1]) ids[i - 1] = ids[i];
        }
        // Native source-line table construction is gated on result character-row count !=0.
        var lines = new LegacyPreparedResultLineRange[ids.Length == 0 ? 0 : heights.Length];
        int cursor = 0;
        for (int line = 0; line < lines.Length; line++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int start = cursor;
            while (cursor < ids.Length && ids[cursor] == line)
            { cancellationToken.ThrowIfCancellationRequested(); cursor++; }
            lines[line] = new(line, start, cursor);
        }
        return new(heights, ids, lines);
    }

    public int[] CopyDerivedCharacterLineIds() => (int[])_characterLineIds.Clone();
    public LegacyPreparedResultLineRange[] CopyLineRanges() => (LegacyPreparedResultLineRange[])_lines.Clone();

    /// <summary>Document177590 reads source-line record double+20 and narrows to SDK lineInfo float+18.</summary>
    public float ReadSourceLineXHeight(int resultLineIndex)
    {
        if ((uint)resultLineIndex >= (uint)_lines.Length) throw new ArgumentOutOfRangeException(nameof(resultLineIndex));
        float h = (float)_sourceLineXHeights[_lines[resultLineIndex].SourceLineIndex];
        if (!float.IsFinite(h)) throw new InvalidDataException("Prepared source-line x-height exceeds SDK binary32 range.");
        return h;
    }

    /// <summary>Transports actual prepared line metric with an explicit recognized-range tuple; no range or width is invented.</summary>
    public LegacyFontSizeTextSample CreateFontSizeSample(int resultLineIndex, int firstCodePoint,
        int characterCount, float wordLeft, float wordRight, uint flags = 0) =>
        new(firstCodePoint, characterCount, ReadSourceLineXHeight(resultLineIndex), wordLeft, wordRight, flags);
}
