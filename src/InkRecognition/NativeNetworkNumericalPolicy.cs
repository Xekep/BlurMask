namespace InkRecognition;

// The historical portable policy remains the default of every existing caller.
// SymbolSequenceScalar closes only the selected scalar summation/clamp choices;
// it does not emulate MXCSR FTZ/DAZ or promise native CRT/SIMD bit equivalence.
internal enum NativeNetworkNumericalPolicy { ExistingPortable, SymbolSequenceScalar }

internal static class NativeSymbolSequenceScalarMath
{
    internal static float Sigmoid(float value) => 1f / (1f + MathF.Exp(-value));

    internal static float SequentialSum(ReadOnlySpan<float> values)
    {
        float sum = 0;
        foreach (float value in values) sum += value;
        return sum;
    }
}
