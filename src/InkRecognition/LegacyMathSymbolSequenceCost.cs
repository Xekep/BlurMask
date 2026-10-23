namespace InkRecognition;

// Pure numeric primitive. Scalars here are diagnostic inputs, not Prediction or
// candidate authority. The owning API supplies its actual target/factor/probability.
internal static class LegacyMathSymbolSequenceCost
{
    internal static float AccumulateToken(float runningCost, float probability, int target, float ignoreFactor)
    {
        if (!float.IsFinite(runningCost) || runningCost < 0f || !float.IsFinite(probability) ||
            probability < 0f || probability > 1f || (uint)target >= LegacyMathAkSymbolSequence.OutputCount ||
            !float.IsFinite(ignoreFactor) || ignoreFactor <= 0f || ignoreFactor > 1f)
            throw new ArgumentException("Invalid bounded scalar cost input.");
        if (target == LegacyMathAkSymbolSequence.IgnoreId) probability *= ignoreFactor;
        float minimumNormal = BitConverter.Int32BitsToSingle(0x00800000);
        float logFloor = BitConverter.Int32BitsToSingle(unchecked((int)0xc2aeac50));
        float log = probability >= minimumNormal ? MathF.Log(probability) : logFloor;
        float result = runningCost - log;
        if (!float.IsFinite(result)) throw new InvalidDataException("Nonfinite symbol-sequence cost.");
        return result;
    }
}
