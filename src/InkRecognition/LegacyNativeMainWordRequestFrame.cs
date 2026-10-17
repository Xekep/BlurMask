namespace InkRecognition;

//The2144F0 caller's one request80 and pointed-to cost cell survive both
//2136D0 and213C00, including callback writes to otherwise untouched fields.
internal sealed class LegacyNativeMainWordRequestFrame
{
    private readonly byte[] bytes;
    internal LegacyNativeWordTransitionCostCell Cost { get; }
    internal LegacyNativeMainWordRequestFrame(LegacyNativeWordTransitionCostCell cost,
        ReadOnlySpan<byte> existing80)
    {
        ArgumentNullException.ThrowIfNull(cost);
        if (existing80.Length != 80) throw new ArgumentException("Complete caller request80 required.");
        Cost = cost; bytes = existing80.ToArray();
    }
    internal byte[] CopyBytes() => bytes.ToArray();
    internal void Commit(LegacyNativeWordRecombinationRequest request)
    {
        if (!ReferenceEquals(request.Cost, Cost)) throw new ArgumentException("The same caller cost cell must survive.");
        request.GetSnapshot().Span.CopyTo(bytes);
    }
}
