namespace InkRecognition;

internal sealed class LegacyDetachedSourceNameTable
{
    internal const int MaximumSources = 256, MaximumPayloadBytes = 8 * 1024 * 1024;
    private readonly int[] offsets;
    private readonly byte[] payload;
    internal int SourceCount { get; }
    internal long NativeAllocationByteCount => checked((SourceCount + 1L) * 8 + payload.Length + 1);
    private LegacyDetachedSourceNameTable(int count, int[] offsets, byte[] payload)
    { SourceCount = count; this.offsets = offsets; this.payload = payload; }

    //Two native source passes, with the last ordinal resolving source-1.
    //Failure leaves no published table. The optional reserve callback represents
    //the single native allocation failure, not per-name partial publication.
    internal static LegacyDetachedSourceNameTable Copy(int count, Func<int, byte[]?> source,
        CancellationToken token, Func<long, bool>? reserve = null)
    {
        ArgumentNullException.ThrowIfNull(source); token.ThrowIfCancellationRequested();
        if (count < 0 || count > MaximumSources)
            throw new NotSupportedException("Portable source-count budget exceeded.");
        int bytes = count;
        var lengths = new int[count]; var present = new bool[count];
        for (int ordinal = 1; ordinal <= count; ordinal++)
        {
            token.ThrowIfCancellationRequested(); byte[]? name = source(ordinal == count ? -1 : ordinal);
            token.ThrowIfCancellationRequested(); present[ordinal - 1] = name is not null;
            int length = name is null ? 0 : CStringLength(name);
            if (length > MaximumPayloadBytes - bytes)
                throw new NotSupportedException("Portable aggregate source-name byte budget exceeded.");
            lengths[ordinal - 1] = length; bytes += length;
        }
        long allocation = checked((count + 1L) * 8 + bytes + 1);
        if (reserve is not null && !reserve(allocation))
            throw new OutOfMemoryException("Source-name allocation failed before metadata publication.");
        token.ThrowIfCancellationRequested();
        var offsets = Enumerable.Repeat(-1, count + 1).ToArray(); var payload = new byte[bytes]; int cursor = 0;
        for (int ordinal = 1; ordinal <= count; ordinal++)
        {
            token.ThrowIfCancellationRequested(); byte[]? name = source(ordinal == count ? -1 : ordinal);
            token.ThrowIfCancellationRequested(); int length = name is null ? 0 : CStringLength(name);
            if ((name is not null) != present[ordinal - 1] || length != lengths[ordinal - 1])
                throw new InvalidOperationException("Source-name storage changed between native copy passes.");
            if (name is not null) { offsets[ordinal] = cursor; name.AsSpan(0, length).CopyTo(payload.AsSpan(cursor)); cursor += length; }
            payload[cursor++] = 0;
        }
        token.ThrowIfCancellationRequested(); return new(count, offsets, payload);
    }

    internal byte[]? Read(short sourceId, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (sourceId == -2) return null;
        int index = sourceId == -1 ? SourceCount : sourceId;
        if ((uint)index >= (uint)offsets.Length)
            throw new ArgumentOutOfRangeException(nameof(sourceId), "Native unchecked source index is outside the owned table.");
        int offset = offsets[index];
        return offset < 0 ? null : payload.AsSpan(offset, CStringLength(payload.AsSpan(offset))).ToArray();
    }
    internal int[] CopySlotOffsets() => offsets.ToArray();
    internal byte[] CopyWrittenPayload() => payload.ToArray();
    private static int CStringLength(ReadOnlySpan<byte> name)
    { int end = name.IndexOf((byte)0); return end < 0 ? name.Length : end; }
}
