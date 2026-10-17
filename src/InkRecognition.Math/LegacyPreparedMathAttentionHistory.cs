namespace InkRecognition.Math;

/// <summary>Prepared numerical036F20 backward trace only. Callback readers
/// do not confer a Step, recording or history owner. Destination is caller-owned.</summary>
internal static class LegacyPreparedMathAttentionHistory
{
    internal static void WritePrepared(Span<float> destination, int symbols, int segments, int parent,
        Func<int, ushort> primaryAt, Func<int, int, int> ancestorAt,
        IReadOnlyList<int> beamsAtTime, Func<int, int, int, float> rawAt,
        LegacyMath2ObservableTokens gate, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(primaryAt); ArgumentNullException.ThrowIfNull(ancestorAt);
        ArgumentNullException.ThrowIfNull(beamsAtTime); ArgumentNullException.ThrowIfNull(rawAt);
        ArgumentNullException.ThrowIfNull(gate); token.ThrowIfCancellationRequested();
        if (symbols < 0 || symbols > 1000 || segments < 1 || segments > 6000 || parent < 0 || parent > 15 ||
            beamsAtTime.Count < symbols || beamsAtTime.Take(symbols).Any(beams => beams is < 1 or > 16) ||
            destination.Length != checked(symbols * segments))
            throw new ArgumentException("Complete bounded prepared history geometry required.");
        int ancestor = ancestorAt(parent, symbols);
        for (int time = symbols - 1; time >= 0; time--)
        {
            token.ThrowIfCancellationRequested();
            if (ancestor < 0 || ancestor >= beamsAtTime[time])
                throw new InvalidDataException("Ancestral rank must address that time's raw attention frame.");
            bool observable = gate.IsObservable(primaryAt(time));
            for (int segment = 0; segment < segments; segment++)
            {
                token.ThrowIfCancellationRequested();
                float value = observable ? rawAt(time, ancestor, segment) : -float.MaxValue;
                if (!float.IsFinite(value)) throw new InvalidDataException("Finite raw attention required.");
                destination[time * segments + segment] = value;
            }
            ancestor = ancestorAt(ancestor, time);
        }
        if (ancestor != -1) throw new InvalidDataException("Cold ancestry must terminate at the actual -1 seed.");
        token.ThrowIfCancellationRequested();
    }
}
