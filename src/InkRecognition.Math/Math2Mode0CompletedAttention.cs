using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathCompletedAttention.Probe")]

namespace InkRecognition.Math;

internal sealed partial class Math2Mode0TensorState
{
    // Read only the attention input, never clone the encoder and KV caches.
    internal (float[] Values, int Segments) CopyPreviousAttention(Math2Mode0DecoderStep step,
        int parent, int symbols, CancellationToken token)
        => WithStep(step, () =>
        {
            if (!policy.ForceNonobservablePositions)
                throw new NotSupportedException("The actual forced-nonobservable cold policy is required.");
            return inputs[3].CopyTransposedF32Beam(parent, BeamCount, symbols, token);
        });
}

internal sealed partial class Math2Mode0PrefixContext
{
    private bool attentionActualHistory = true;
    private void ObserveAttentionSelection(Math2Mode0PrefixSelection selection)
    {
        // Deliberately sticky. An alternate primary symbol can have a different
        // observable status from the raw symbol masked in EF1. No imported or
        // diagnostic ancestry is inferred to satisfy the carried-state identity.
        if (selection.IsDiagnostic) attentionActualHistory = false;
    }

    internal void RequireActualAttentionHistory(Math2Mode0PrefixStep step)
        => WithStep(step, () =>
        {
            if (!attentionActualHistory)
                throw new NotSupportedException("Attention requires an uninterrupted actual cold selection history.");
            return true;
        });
}

internal sealed partial class Math2Mode0CompletedEventLease
{
    private Math2Mode0CompletedAttention? attentionSnapshot;

    internal Math2Mode0CompletedAttention CaptureAttention(CancellationToken token = default)
        => step.WithActive(() =>
        {
            token.ThrowIfCancellationRequested();
            recorder.CheckOwner(step.PrefixOwner);
            step.PrefixOwner.RequireActualAttentionHistory(step);
            if (attentionSnapshot is not null) return attentionSnapshot;
            int parent = BinaryPrimitives.ReadInt32LittleEndian(row.AsSpan(40));
            int symbols = primary.Length;
            if (symbols < 1 || symbols != step.Generation.Offset ||
                parent < 0 || parent >= step.Generation.ActiveCount)
                throw new NotSupportedException("Nonempty actual old-prefix attention required.");
            var previous = step.TensorStep.Owner.CopyPreviousAttention(step.TensorStep, parent, symbols, token);
            token.ThrowIfCancellationRequested();
            attentionSnapshot = new(this, symbols, previous.Segments, previous.Values);
            return attentionSnapshot;
        });
}

/// <summary>Immutable copied attention bound to an actual event, captured
/// before Apply. No retained ORT/KV tensors; no fabricated completed output.</summary>
internal sealed class Math2Mode0CompletedAttention
{
    private readonly float[] values;
    internal Math2Mode0CompletedEventLease Source { get; }
    internal int Symbols { get; }
    internal int Segments { get; }
    internal Math2Mode0CompletedAttention(Math2Mode0CompletedEventLease source,
        int symbols, int segments, float[] ownedValues)
    { Source = source; Symbols = symbols; Segments = segments; values = ownedValues; }
    internal float[] CopyAttention() => values.ToArray();
    internal float[] ActivateSigmoid(CancellationToken token = default)
        => LegacyPortableMathSegmentation.ActivatePreparedSigmoid(values, Symbols, Segments, token);
}

internal sealed unsafe partial class OrtTensor
{
    // Native CPU tensor memory is borrowed only while the parent state monitor
    // is held. Allocate one selected [symbols,segments] snapshot, no full beam
    // tensor or KV-cache copies, and validate the work budget before allocation.
    internal (float[] Values, int Segments) CopyTransposedF32Beam(int parent,
        int expectedBeams, int expectedSymbols, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var (type, shape, pointer) = Metadata();
        if (type != 1 || shape.Length != 3 || shape[0] is < 1 or > 6000 ||
            shape[1] != expectedBeams || shape[2] != expectedSymbols ||
            parent < 0 || parent >= expectedBeams || expectedSymbols is < 1 or > 1000 ||
            (long)expectedSymbols * shape[0] > 1_000_000)
            throw new NotSupportedException("Bounded actual [segments,beams,prefixLength] attention required.");
        int segments = checked((int)shape[0]);
        var input = new ReadOnlySpan<float>((void*)pointer, Size(shape));
        var result = new float[checked(expectedSymbols * segments)];
        for (int segment = 0; segment < segments; segment++)
            for (int symbol = 0; symbol < expectedSymbols; symbol++)
            {
                token.ThrowIfCancellationRequested();
                float value = input[checked((segment * expectedBeams + parent) * expectedSymbols + symbol)];
                if (!float.IsFinite(value)) throw new InvalidDataException("Finite selected attention required.");
                result[symbol * segments + segment] = value;
            }
        token.ThrowIfCancellationRequested();
        return (result, segments);
    }
}
