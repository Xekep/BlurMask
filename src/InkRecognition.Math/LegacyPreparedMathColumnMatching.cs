using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathColumnMatching.Probe")]
namespace InkRecognition.Math;

/// <summary>Prepared finite040270/0403E0/040670 matching kernel only.
/// Does not correct source units, mutate an actual record or select a request.</summary>
internal sealed class LegacyPreparedMathColumnMatching
{
    private readonly int[] rows, columns;
    private readonly float[] adjusted;
    internal int Rounds { get; }
    internal int[] CopyRows() => rows.ToArray();
    internal int[] CopyColumns() => columns.ToArray();
    internal float[] CopyAdjusted() => adjusted.ToArray();
    private LegacyPreparedMathColumnMatching(int[] rows, int[] columns, float[] adjusted, int rounds)
    { this.rows = rows; this.columns = columns; this.adjusted = adjusted; Rounds = rounds; }

    internal static LegacyPreparedMathColumnMatching EvaluatePrepared(ReadOnlySpan<float> source,
        int rowCount, int columnCount, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (rowCount < 0 || rowCount > 1000 || columnCount < 0 || columnCount > 3000 ||
            (long)rowCount * columnCount > 1_000_000 || source.Length != (long)rowCount * columnCount)
            throw new ArgumentException("Bounded row-major prepared matrix required.");
        // Native internally reserves both extrema. This bounded adapter refuses
        // caller sentinels/NaNs rather than claiming its progress proof covers them.
        foreach (float value in source)
            if (!float.IsFinite(value) || value <= -float.MaxValue || value >= float.MaxValue)
                throw new ArgumentException("Finite values strictly inside native lock sentinels required.");
        float[] matrix = source.ToArray();
        int target = System.Math.Min(rowCount, columnCount), rounds = 0;
        int[] selectedRows = [], selectedColumns = [];
        if (target == 0) return new([], [], matrix, 0);
        while (true)
        {
            token.ThrowIfCancellationRequested();
            if (rounds != 0)
            {
                //040670 masks every selected row/column, then restores each
                //paired cell to +FMAX. Restore AFTER all masking, not per pair.
                foreach (int row in selectedRows)
                    matrix.AsSpan(row * columnCount, columnCount).Fill(-float.MaxValue);
                foreach (int column in selectedColumns)
                    for (int row = 0; row < rowCount; row++) matrix[row * columnCount + column] = -float.MaxValue;
                for (int i = 0; i < selectedRows.Length; i++)
                    matrix[selectedRows[i] * columnCount + selectedColumns[i]] = float.MaxValue;
            }
            int[] bestRows = new int[columnCount];
            float[] bestScores = new float[columnCount];
            for (int column = 0; column < columnCount; column++)
            {
                int best = -1; float score = -float.MaxValue;
                for (int row = 0; row < rowCount; row++)
                {
                    float value = matrix[row * columnCount + column];
                    if (value > score) { best = row; score = value; }
                }
                bestRows[column] = best; bestScores[column] = score;
            }
            var nextRows = new List<int>(); var nextColumns = new List<int>();
            for (int first = 0; first < columnCount; first++)
            {
                int row = bestRows[first];
                if (row < 0) continue;
                int chosen = first; float score = bestScores[first];
                for (int later = first + 1; later < columnCount; later++)
                    if (bestRows[later] == row)
                    {
                        bestRows[later] = -1;
                        if (bestScores[later] > score) { chosen = later; score = bestScores[later]; }
                    }
                nextRows.Add(row); nextColumns.Add(chosen);
            }
            rounds++;
            if (nextRows.Count <= selectedRows.Length)
                throw new InvalidDataException("Prepared matching did not progress; no partial result is accepted.");
            selectedRows = nextRows.ToArray(); selectedColumns = nextColumns.ToArray();
            if (selectedRows.Length == target) break;
        }
        token.ThrowIfCancellationRequested();
        return new(selectedRows, selectedColumns, matrix, rounds);
    }
}
