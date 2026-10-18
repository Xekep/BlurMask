using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathSourceAssignments.Probe")]
namespace InkRecognition.Math;

/// <summary>Prepared041020 source-aware assignment insertion. Its explicit
/// all-columns flag is not a recovered request policy.</summary>
internal sealed class LegacyPreparedMathSourceAssignments
{
    private readonly int[] assignments;
    internal bool AlreadyComplete { get; }
    internal int[] CopyAssignments() => assignments.ToArray();
    private LegacyPreparedMathSourceAssignments(int[] assignments, bool complete)
    { this.assignments = assignments; AlreadyComplete = complete; }

    internal static LegacyPreparedMathSourceAssignments EvaluatePrepared(ReadOnlySpan<uint> sourceUnits,
        ReadOnlySpan<int> initialAssignments, ReadOnlySpan<int> observableOriginalRows,
        ReadOnlySpan<float> probabilities, int segments, bool allColumns,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        int n = observableOriginalRows.Length;
        if (segments is < 0 or > 6000 || n > 1000 ||
            sourceUnits.Length != 0 && sourceUnits.Length != segments ||
            initialAssignments.Length > segments || (long)n * segments > 1_000_000 ||
            probabilities.Length != (long)n * segments)
            throw new ArgumentException("Bounded prepared source columns, assignment prefix and probabilities required.");
        int previous = -1;
        foreach (int value in observableOriginalRows)
        {
            if (value <= previous || value > 999)
                throw new ArgumentException("Increasing original observable row indices required.");
            previous = value;
        }
        foreach (int value in initialAssignments)
            if (value < -1 || value > 999) throw new ArgumentException("Prepared original output indices required.");
        foreach (float value in probabilities)
            if (!float.IsFinite(value) || value < 0 || value > 1)
                throw new ArgumentException("Finite activated probabilities required.");
        //Native041020 checks this BEFORE its source/flag-dependent scan.
        if (initialAssignments.Length == segments) return new(initialAssignments.ToArray(), true);
        int retained = 0;
        if (!allColumns && sourceUnits.Length != 0)
            foreach (uint unit in sourceUnits) if ((unit >> 24) == 0) retained++;
        if (initialAssignments.Length != retained)
            throw new NotSupportedException("Prepared initial assignments must match retained zero-flag source columns.");
        if (segments > retained && n == 0)
            throw new NotSupportedException("An observable row is required for every new column assignment.");
        var result = new List<int>(initialAssignments.ToArray());
        for (int column = 0; column < segments; column++)
        {
            token.ThrowIfCancellationRequested();
            if (!allColumns && sourceUnits.Length != 0 && (sourceUnits[column] >> 24) == 0) continue;
            float best = -float.MaxValue; int selected = -1;
            for (int row = 0; row < n; row++)
                if (probabilities[row * segments + column] > best)
                { best = probabilities[row * segments + column]; selected = row; }
            if (selected < 0 || column > result.Count)
                throw new NotSupportedException("Prepared source and assignment positions must form a complete sequence.");
            result.Insert(column, observableOriginalRows[selected]);
        }
        token.ThrowIfCancellationRequested();
        if (result.Count != segments) throw new InvalidDataException("Incomplete source-aware assignment sequence.");
        return new(result.ToArray(), false);
    }
}
