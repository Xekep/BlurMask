namespace InkRecognition;

// Literal scalar-policy1A5DC0 conversion boundary, not an all-CPU quantizer.
// The caller preserves its existing F32 multiplication and rounding expression.
// Conversion precedes integer clamp: x86 CVTTSS2SI yields INT_MIN for invalid
// or out-of-range operands. No no-inline attribute or forced call boundary.
internal static class LegacyScalarInt16ActivationQuantization
{
    internal static int ConvertRoundedAndClamp(float rounded,int integerLimit)
    {
        if((uint)integerLimit>32767)throw new ArgumentOutOfRangeException(nameof(integerLimit));
        // Both bounds are exactly representable in F32. NaN fails this ordered
        // interval test; infinities/out-of-range values select integer indefinite.
        int converted=rounded>=-2147483648f&&rounded<2147483648f?(int)rounded:int.MinValue;
        return converted < -integerLimit ? -integerLimit : converted > integerLimit ? integerLimit : converted;
    }
}
