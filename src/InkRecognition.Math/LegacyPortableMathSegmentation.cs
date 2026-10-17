using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathSegmentation.Probe")]

namespace InkRecognition.Math;

/// <summary>Prepared contiguous symbol/segment sigmoid arithmetic from036C70.
/// MathF.Exp is a portable adaptation, not the vendor CPU dispatch. This does
/// not reconstruct attention ancestry or execute EF4.</summary>
internal static class LegacyPortableMathSegmentation
{
    internal static float[] ActivatePreparedSigmoid(ReadOnlySpan<float> attention,
        int symbols, int segments, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (symbols is < 1 or > 1000 || segments is < 1 or > 6000 ||
            (long)symbols * segments > 1_000_000 || attention.Length != (long)symbols * segments)
            throw new ArgumentException("Bounded complete prepared [symbols,segments] attention required.");
        //Validate before allocating output. Negative finite maximum is a real
        //nonobservable-symbol mask; it must remain accepted and maps to zero.
        foreach (float value in attention)
        {
            token.ThrowIfCancellationRequested();
            if (!float.IsFinite(value)) throw new NotSupportedException("Nonfinite prepared attention is outside this adaptation.");
        }
        var result = new float[attention.Length];
        for (int i = 0; i < result.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            float exponential = MathF.Exp(-attention[i]);
            //Native EXP result, ADDSS and DIVSS are separate F32 operations.
            //Positive EXP overflow intentionally yields +infinity and then0.
            float denominator = exponential + 1f;
            result[i] = 1f / denominator;
        }
        token.ThrowIfCancellationRequested();
        return result;
    }
}
