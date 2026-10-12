namespace InkRecognition;

public readonly record struct LegacyNativeOwnedWordRow(int Node, ushort Arc, LegacyNativeStoredWordRecord Row);

/// <summary>Explicit owning container for stored 88 / primary 40-byte row mutation.
/// It clones actual supplied rows, preserving their complete secondary blocks.</summary>
public sealed class LegacyNativePrimaryWordRowOwner
{
    private sealed record Data(byte[] Record, byte[] Primary, byte[] Secondary);
    private readonly Dictionary<(int, ushort), Data> rows = [];
    public ushort PrimaryAlternativeCount { get; }

    public LegacyNativePrimaryWordRowOwner(ushort primaryAlternativeCount,
        IReadOnlyList<LegacyNativeOwnedWordRow> suppliedRows)
    {
        if (primaryAlternativeCount is < 1 or > 20) throw new ArgumentOutOfRangeException(nameof(primaryAlternativeCount));
        ArgumentNullException.ThrowIfNull(suppliedRows);
        if (suppliedRows.Count > 65536) throw new ArgumentException("Bounded actual row collection required.");
        PrimaryAlternativeCount = primaryAlternativeCount;
        foreach (var supplied in suppliedRows)
        {
            ArgumentNullException.ThrowIfNull(supplied.Row);
            if (supplied.Node < 0 || supplied.Row.Record.Length != 88 ||
                supplied.Row.PrimaryAlternatives.Length != primaryAlternativeCount * 40)
                throw new ArgumentException("Actual owned stored 88-byte/configured primary rows required.");
            rows.Add((supplied.Node, supplied.Arc), new(supplied.Row.Record.ToArray(),
                supplied.Row.PrimaryAlternatives.ToArray(), supplied.Row.SecondaryAlternatives.ToArray()));
        }
    }
    internal ReadOnlySpan<byte> Record(int node, ushort arc) => Get(node, arc).Record;
    internal Span<byte> Alternative(int node, ushort arc, int index)
    {
        if ((uint)index >= PrimaryAlternativeCount) throw new ArgumentOutOfRangeException(nameof(index));
        return Get(node, arc).Primary.AsSpan(index * 40, 40);
    }
    public LegacyNativeStoredWordRecord CopyRow(int node, ushort arc)
    {
        var data = Get(node, arc);
        return new(data.Record.ToArray(), data.Primary.ToArray(), data.Secondary.ToArray());
    }
    private Data Get(int node, ushort arc) => rows.TryGetValue((node, arc), out var value) ? value
        : throw new ArgumentException("Actual row owner does not contain the requested node/arc.");
}
