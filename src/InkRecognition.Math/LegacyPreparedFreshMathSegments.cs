using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathFreshSegments.Probe")]

namespace InkRecognition.Math;

internal sealed partial class Math2Mode0CompletedEventLease
{
    // Fingerprinted resource gate retained by this event's exact cold owner.
    // It owns no native tensor and remains valid for copied post-loop data.
    internal LegacyMath2ObservableTokens ReadAttentionAlphabet() => step.TensorStep.Owner.ObservableGate;
}

/// <summary>Observable-row filtering, fresh041020 assignment and041A30 run
/// compression. Explicit prepared no-history branch, not complete EF4 policy,
/// output-unit publication or final score acceptance.</summary>
internal sealed class LegacyPreparedFreshMathSegments
{
    private readonly int[] observableIndices, assignments;
    private readonly MathSegmentRun[] runs;
    internal Math2Mode0CompletedAttention Source { get; }
    private LegacyPreparedFreshMathSegments(Math2Mode0CompletedAttention source,
        int[] indices, int[] assignments, MathSegmentRun[] runs)
    { Source = source; observableIndices = indices; this.assignments = assignments; this.runs = runs; }

    internal static LegacyPreparedFreshMathSegments Prepare(Math2Mode0CompletedAttention source,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(source); token.ThrowIfCancellationRequested();
        var alphabet = source.Source.ReadAttentionAlphabet();
        ushort[] primary = source.Source.CopyPrimary();
        if (primary.Length != source.Symbols) throw new InvalidDataException("Same event primary prefix required.");
        int[] indices = Enumerable.Range(0, primary.Length).Where(i => alphabet.IsObservable(primary[i])).ToArray();
        if (indices.Length == 0) throw new NotSupportedException("At least one actual observable old-prefix symbol required.");
        float[] activated = source.ActivateSigmoid(token);
        var filtered = new float[checked(indices.Length * source.Segments)];
        for (int row = 0; row < indices.Length; row++)
        {
            token.ThrowIfCancellationRequested();
            activated.AsSpan(indices[row] * source.Segments, source.Segments)
                .CopyTo(filtered.AsSpan(row * source.Segments));
        }
        var assignments = AssignPreparedProbabilities(filtered, indices, source.Segments, token);
        var runs = CompressPrepared(assignments, token);
        token.ThrowIfCancellationRequested();
        return new(source, indices, assignments, runs);
    }
    internal int[] CopyObservableIndices() => observableIndices.ToArray();
    internal int[] CopyAssignments() => assignments.ToArray();
    internal MathSegmentRun[] CopyRuns() => runs.ToArray();

    internal static int[] AssignPreparedProbabilities(ReadOnlySpan<float> probabilities,
        ReadOnlySpan<int> originalIndices, int segments, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (originalIndices.Length is < 1 or > 1000 || segments is < 1 or > 6000 ||
            (long)originalIndices.Length * segments > 1_000_000 ||
            probabilities.Length != (long)originalIndices.Length * segments)
            throw new ArgumentException("Bounded complete prepared observable-row probabilities required.");
        for (int i = 0; i < originalIndices.Length; i++)
            if (originalIndices[i] is < 0 or > 999 || i > 0 && originalIndices[i] <= originalIndices[i - 1])
                throw new ArgumentException("Ordered distinct original prefix positions required.");
        foreach (float value in probabilities)
        {
            token.ThrowIfCancellationRequested();
            if (!float.IsFinite(value) || value is < 0f or > 1f)
                throw new NotSupportedException("This prepared adaptation accepts finite activated probabilities only.");
        }
        var result = new int[segments];
        for (int segment = 0; segment < segments; segment++)
        {
            token.ThrowIfCancellationRequested();
            float best = -float.MaxValue; int winner = -1;
            for (int row = 0; row < originalIndices.Length; row++)
            {
                float value = probabilities[row * segments + segment];
                //041020 keeps the first winner on equal finite values.
                if (value <= best) continue;
                best = value; winner = row;
            }
            result[segment] = originalIndices[winner];
        }
        return result;
    }

    internal static MathSegmentRun[] CompressPrepared(ReadOnlySpan<int> assignments,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (assignments.Length > 6000 || assignments.ContainsAnyExceptInRange(0, 999))
            throw new ArgumentException("Bounded complete original-index assignments required.");
        if (assignments.IsEmpty) return [];
        var result = new List<MathSegmentRun>();
        int previous = assignments[0];
        for (int i = 1; i < assignments.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            if (assignments[i] != previous)
            {
                result.Add(new(previous, i)); previous = assignments[i];
            }
        }
        result.Add(new(previous, assignments.Length));
        token.ThrowIfCancellationRequested();
        return result.ToArray();
    }
}

internal readonly record struct MathSegmentRun(int PrimaryIndex, int EndSegmentExclusive);
