namespace InkRecognition.Math;

/// <summary>A produced scoped receipt, not authorization inferred from raw
/// Document equality. Retains only the field/context and detached first copy,
/// never the decoder, Step, SourceBoundOutput or recognition source.</summary>
internal sealed class Math2Mode0SelectedMathFieldCopy : IDisposable
{
    private readonly object gate = new();
    private readonly Math2Mode0OriginalMathFieldSource.Binding binding;
    private bool disposed;
    internal Math2Mode0FirstResultCopy Copy { get; }
    internal Math2Mode0OriginalMathFieldSource Field => binding.Owner;
    private Math2Mode0SelectedMathFieldCopy(Math2Mode0FirstResultCopy copy,
        Math2Mode0OriginalMathFieldSource.Binding binding) { Copy = copy; this.binding = binding; }
    internal static Math2Mode0SelectedMathFieldCopy Produce(Math2Mode0SourceBoundOutput actualOutput,
        Math2Mode0OriginalMathFieldSource actualField, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(actualOutput); ArgumentNullException.ThrowIfNull(actualField);
        return actualOutput.WithSelectedMathFieldForCopy(actualField, binding =>
        {
            var copy = Math2Mode0FirstResultCopy.Create(actualOutput, token);
            try { binding.ValidatePrepared(token); return new Math2Mode0SelectedMathFieldCopy(copy, binding); }
            catch { copy.Dispose(); throw; }
        }, token);
    }
    internal void Validate(Math2Mode0FirstResultCopy actualCopy, Math2Mode0OriginalMathFieldSource actualField,
        CancellationToken token = default)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            binding.Validate(actualField, token); binding.ValidatePrepared(token);
            if (!ReferenceEquals(actualCopy, Copy) || !ReferenceEquals(Copy.Document, binding.Document))
                throw new ArgumentException("Exact produced Math field/copy receipt required.");
            //Validates completed/live copy without cloning payload arrays.
            Copy.ValidateForOuterCopy(token);
        }
    }
    public void Dispose() { lock (gate) { if (disposed) return; disposed = true; Copy.Dispose(); } }
}

internal sealed partial class Math2Mode0SourceBoundOutput
{
    internal Math2Mode0SelectedMathFieldCopy CopySelectedMathField(Math2Mode0OriginalMathFieldSource actualField,
        CancellationToken token = default) => Math2Mode0SelectedMathFieldCopy.Produce(this, actualField, token);
    internal T WithSelectedMathFieldForCopy<T>(Math2Mode0OriginalMathFieldSource actualField,
        Func<Math2Mode0OriginalMathFieldSource.Binding, T> action, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(actualField); ArgumentNullException.ThrowIfNull(action);
        return Output.WithSourceAttachment(this, () =>
        {
            ValidateForCopy(Output, token);
            var field = Output.Owner.Ef4.Source.ReadSelectedMathField(actualField);
            if (!ReferenceEquals(Document, field.Document)) throw new ArgumentException("Same original Math Document required.");
            token.ThrowIfCancellationRequested(); var result = action(field);
            try { field.ValidatePrepared(token); return result; }
            catch { if (result is IDisposable disposable) disposable.Dispose(); throw; }
        });
    }
}
