namespace InkRecognition.Math;

internal sealed partial class Math2Mode0FirstResultCopy
{
    //Private metadata/payload view held under the SAME source gate. Only the
    //trusted outer copy kernel receives these arrays; inspection stays copied.
    internal T WithSymbolRowsForOuterCopy<T>(Func<Math2Mode0OutputUnit[],
        LegacyNativeInkPositionRange[][], long, T> action, CancellationToken token)
    {
        lock (gate)
        {
            ValidateForOuterCopy(token);
            if (rows.Length != sourceRanges.Length)
                throw new InvalidOperationException("Complete actual source ranges must match every first-copy row.");
            return action(rows, sourceRanges, reservation);
        }
    }

    internal void ValidateForOuterCopy(CancellationToken token)
    {
        lock (gate)
        {
            token.ThrowIfCancellationRequested(); ObjectDisposedException.ThrowIf(released, this);
            if (!complete || failed || library is null || library.IsDisposed)
                throw new InvalidOperationException("The same successful live first-copy owner is required.");
        }
    }
}
