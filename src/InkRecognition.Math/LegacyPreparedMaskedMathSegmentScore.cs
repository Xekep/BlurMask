using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathMaskedSegmentScore.Probe")]

namespace InkRecognition.Math;

/// <summary>Prepared finite041CF0 deletion-mask branch. The caller supplies
/// the already compressed activated matrix; no request policy or EF4 dispatch
/// is inferred, and no completed-record fields are published.</summary>
internal sealed class LegacyPreparedMaskedMathSegmentScore
{
    private readonly int[] remap;
    internal float Before { get; }
    internal float Penalty { get; }
    internal float Alpha { get; }
    internal float After { get; }
    internal int AssignedCount { get; }
    internal int ContributingCount { get; }
    internal int[] CopyOriginalToCompressedRows() => remap.ToArray();
    // Native optional diagnostic90 receives these two F32 stores before
    // multiplication/addition. Return detached values, not a fake native pointer.
    internal float[] CopyDiagnosticValues() => [Before, Penalty];

    private LegacyPreparedMaskedMathSegmentScore(int[] remap, float before,
        float penalty, float alpha, float after, int assigned, int contributing)
    {
        this.remap = remap; Before = before; Penalty = penalty;
        Alpha = alpha; After = after; AssignedCount = assigned;
        ContributingCount = contributing;
    }

    internal static LegacyPreparedMaskedMathSegmentScore EvaluatePrepared(
        ReadOnlySpan<byte> deletionMask, ReadOnlySpan<float> compressedProbabilities,
        int segmentCount, ReadOnlySpan<int> assignments, float before, float alpha,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (deletionMask.Length is < 1 or > 1000 || segmentCount is < 1 or > 6000 ||
            assignments.Length != segmentCount || !float.IsFinite(before) || !float.IsFinite(alpha))
            throw new ArgumentException("Bounded finite prepared mask, matrix and assignments required.");
        int rows = 0;
        foreach (byte deleted in deletionMask) if (deleted == 0) rows++;
        if ((long)rows * segmentCount > 1_000_000 ||
            compressedProbabilities.Length != (long)rows * segmentCount)
            throw new ArgumentException("Matrix rows must equal the number of zero mask entries.");
        foreach (float value in compressedProbabilities)
        {
            token.ThrowIfCancellationRequested();
            if (!float.IsFinite(value) || value is < 0f or > 1f)
                throw new NotSupportedException("Only finite activated probabilities are supported.");
        }
        foreach (int symbol in assignments)
            if (symbol < -1 || symbol >= deletionMask.Length)
                throw new ArgumentException("Assignments use original symbol positions or absent(-1).");

        int[] remap = new int[deletionMask.Length]; int next = 0;
        for (int original = 0; original < remap.Length; original++)
        {
            remap[original] = next;
            if (deletionMask[original] == 0) next++;
        }
        float penalty = 0f; int assigned = 0, contributing = 0;
        for (int segment = 0; segment < segmentCount; segment++)
        {
            token.ThrowIfCancellationRequested();
            int original = assignments[segment];
            if (original == -1) continue;
            // INC occurs before the mask test. Deleted assignments contribute
            // to the denominator even though their log is omitted.
            assigned++;
            if (deletionMask[original] != 0) continue;
            contributing++;
            float probability = MathF.Max(compressedProbabilities[
                remap[original] * segmentCount + segment], 1e-10f);
            penalty -= MathF.Log(probability);
        }
        if (assigned > 0) penalty /= (float)assigned;
        float weighted = penalty * alpha;
        float after = weighted + before;
        if (!float.IsFinite(after))
            throw new NotSupportedException("Nonfinite score output is outside this prepared adaptation.");
        token.ThrowIfCancellationRequested();
        return new(remap, before, penalty, alpha, after, assigned, contributing);
    }
}
