using System.Buffers.Binary;

namespace InkRecognition.Math;

/// <summary>Finite fresh/no-deletion041CF0 numeric score. Prepared alpha is
/// explicit; this does not resolve request policy or publish completed fields.</summary>
internal readonly record struct LegacyPreparedFreshMathSegmentScore(
    float Before, float Penalty, float Alpha, float After)
{
    internal static LegacyPreparedFreshMathSegmentScore FromActualPreparedBranch(
        LegacyPreparedFreshMathSegments segments, float preparedAlpha, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(segments); token.ThrowIfCancellationRequested();
        var attention = segments.Source;
        float before = LegacyPreparedMathCompletionScore.WithoutConstraintModel(
            BinaryPrimitives.ReadSingleLittleEndian(attention.Source.CopyRawRecord()), attention.Symbols, token);
        return EvaluatePrepared(attention.ActivateSigmoid(token), attention.Symbols,
            attention.Segments, segments.CopyAssignments(), before, preparedAlpha, token);
    }

    internal static LegacyPreparedFreshMathSegmentScore EvaluatePrepared(ReadOnlySpan<float> probabilities,
        int symbols, int segmentCount, ReadOnlySpan<int> assignments,
        float before, float alpha, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (symbols is < 1 or > 1000 || segmentCount is < 1 or > 6000 ||
            (long)symbols * segmentCount > 1_000_000 || probabilities.Length != (long)symbols * segmentCount ||
            assignments.Length != segmentCount || !float.IsFinite(before) || !float.IsFinite(alpha))
            throw new ArgumentException("Bounded finite complete fresh segmentation score required.");
        foreach (float value in probabilities)
        {
            token.ThrowIfCancellationRequested();
            if (!float.IsFinite(value) || value is < 0f or > 1f)
                throw new NotSupportedException("Finite activated probabilities required.");
        }
        foreach (int index in assignments)
            if (index < -1 || index >= symbols)
                throw new ArgumentException("Assignment must be absent(-1) or an original primary position.");
        float penalty = 0f; int count = 0;
        for (int segment = 0; segment < segmentCount; segment++)
        {
            token.ThrowIfCancellationRequested();
            int symbol = assignments[segment];
            if (symbol == -1) continue;
            count++;
            // Native MAXSS floor is F32 bits2EDBE6FF. MathF.Log is the
            // portable adaptation of1A8C30, not a vendor CPU equivalence claim.
            float probability = MathF.Max(probabilities[symbol * segmentCount + segment], 1e-10f);
            float logarithm = MathF.Log(probability);
            penalty -= logarithm;
        }
        if (count > 0) penalty /= (float)count;
        float weighted = penalty * alpha;
        float after = weighted + before;
        if (!float.IsFinite(after)) throw new NotSupportedException("Nonfinite score output is outside this prepared adaptation.");
        token.ThrowIfCancellationRequested();
        return new(before, penalty, alpha, after);
    }
}
