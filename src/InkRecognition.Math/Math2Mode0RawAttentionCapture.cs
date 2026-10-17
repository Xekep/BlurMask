namespace InkRecognition.Math;

internal sealed partial class Math2Mode0TensorState
{
    internal Math2Mode0RawAttentionFrameCopy CopyRawAttentionForHistory(Math2Mode0PrefixStep prefixStep,
        Math2Mode0RawAttentionHistory history, CancellationToken token)
        => WithStep(prefixStep.TensorStep, () =>
        {
            if (!policy.ForceNonobservablePositions)
                throw new NotSupportedException("The authenticated forced-nonobservable cold policy is required.");
            return prefixStep.TensorStep.Output(1).CopyRawHistoryFrame(prefixStep, history,
                policy.Gate, checked((int)segments), BeamCount, token);
        });
}

internal sealed unsafe partial class OrtTensor
{
    // The prefix and tensor-state monitors remain held during this pointer
    // borrow. No whole decoder tuple, attention history, or KV buffers copied.
    internal Math2Mode0RawAttentionFrameCopy CopyRawHistoryFrame(Math2Mode0PrefixStep step,
        Math2Mode0RawAttentionHistory history, LegacyMath2ObservableTokens actualGate,
        int expectedSegments, int expectedBeams, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var (type, shape, pointer) = Metadata();
        if (type != 1 || shape.Length != 3 || shape[0] != expectedSegments || shape[0] is < 1 or > 6000 ||
            shape[1] != expectedBeams || shape[1] is < 1 or > 16 || shape[2] != 1)
            throw new NotSupportedException("Only the actual bounded F32[S,B,1] attention output is supported.");
        int count = checked(expectedSegments * expectedBeams);
        history.ReserveActualRawFrame(step, actualGate, expectedSegments, expectedBeams, checked(count * 4L), token);
        float[] values = new float[count];
        var input = new ReadOnlySpan<float>((void*)pointer, count);
        for (int at = 0; at < count; at++)
        {
            token.ThrowIfCancellationRequested();
            if (!float.IsFinite(input[at])) throw new InvalidDataException("Finite raw attention required.");
            values[at] = input[at];
        }
        token.ThrowIfCancellationRequested();
        return new(expectedSegments, expectedBeams, values);
    }
}
