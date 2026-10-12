using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Explicit allocator-release observations for 016a20. Entry -1
/// denotes the vector allocation; other indices denote owned row payloads.
/// Opaque identities are data, never dereferenced or freed as native pointers.</summary>
public delegate void LegacyNativeWordTraceRelease(int vectorIndex, int entryIndex, ulong allocationIdentity);

/// <summary>Owned 16-byte trace-vector headers at controller2370/2388.
/// Payload allocation identities are explicit; this does not implement the
/// trace-record producer or infer native addresses from managed objects.</summary>
public sealed class LegacyNativeWordTraceVector
{
    private byte[] storage;
    private bool resetting;
    public int Count { get; private set; }
    public int Capacity => storage.Length / 16;
    public ulong StorageIdentity { get; private set; }
    public uint UsedCount { get; private set; }

    public LegacyNativeWordTraceVector(ReadOnlySpan<byte> headerStorage, int count,
        ulong storageIdentity, uint usedCount)
    {
        if (headerStorage.Length % 16 != 0 || headerStorage.Length / 16 > 4096 ||
            (uint)count > (uint)(headerStorage.Length / 16))
            throw new ArgumentException("Explicit complete 16-byte trace headers and bounded count required.");
        storage = headerStorage.ToArray(); Count = count;
        StorageIdentity = storageIdentity; UsedCount = usedCount;
    }

    public ReadOnlyMemory<byte> CopyHeaderStorage() => storage.ToArray();

    internal void Reset(int vectorIndex, LegacyNativeWordTraceRelease? release,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (resetting) throw new InvalidOperationException("Trace reset is not reentrant.");
        resetting = true;
        try
        {
            if (Count < 10)
            {
                for (int entry = 0; entry < Count; entry++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    BinaryPrimitives.WriteUInt16LittleEndian(storage.AsSpan(entry * 16 + 8), 0);
                }
            }
            else
            {
                if (release is null) throw new NotSupportedException("Large native trace vectors need explicit payload/vector release callbacks.");
                for (int entry = 0; entry < Count; entry++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    release(vectorIndex, entry, BinaryPrimitives.ReadUInt64LittleEndian(storage.AsSpan(entry * 16)));
                    cancellationToken.ThrowIfCancellationRequested();
                }
                release(vectorIndex, -1, StorageIdentity);
                cancellationToken.ThrowIfCancellationRequested();
                storage = []; Count = 0; StorageIdentity = 0;
            }
            UsedCount = 0;
        }
        finally { resetting = false; }
    }
}

/// <summary>Native212d80 pass reset: clear every heap's active count before
/// resizing to its phase-specific initial quota, then reset the two optional
/// trace vectors. Allocation failure preserves earlier mutations and stops.
/// Scheduling, trace production and the native final winner remain separate.</summary>
public static class LegacyNativeWordPassReset
{
    public static bool Reset(IReadOnlyList<LegacyNativeWordHeapDescriptor> descriptors,
        LegacyNativeWordControllerConfiguration configuration, int phase,
        LegacyNativeWordHeapGrowthProvider? growthProvider = null,
        IReadOnlyList<LegacyNativeWordTraceVector>? traceVectors = null,
        LegacyNativeWordTraceRelease? traceRelease = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptors);
        cancellationToken.ThrowIfCancellationRequested();
        var owned = descriptors.ToArray();
        if (owned.Length > 512) throw new NotSupportedException("Owned reset descriptor budget is 512.");
        var seen = new HashSet<LegacyNativeWordHeapDescriptor>(ReferenceEqualityComparer.Instance);
        foreach (var descriptor in owned)
        {
            ArgumentNullException.ThrowIfNull(descriptor);
            if (!seen.Add(descriptor)) throw new ArgumentException("Heap reset descriptors must have separate ownership.");
        }
        LegacyNativeWordTraceVector[]? traces = null;
        if (configuration.Trace2368 != 0)
        {
            if (traceVectors is null || traceVectors.Count != 2)
                throw new ArgumentException("Tracing reset requires the explicit controller2370/2388 vectors.");
            traces = traceVectors.ToArray();
            ArgumentNullException.ThrowIfNull(traces[0]); ArgumentNullException.ThrowIfNull(traces[1]);
            if (ReferenceEquals(traces[0], traces[1])) throw new ArgumentException("Trace vectors require separate ownership.");
            if (traceRelease is null && traces.Any(trace => trace.Count >= 10))
                throw new NotSupportedException("Trace-vector release callbacks must be supplied before reset.");
        }
        for (int index = 0; index < owned.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var descriptor = owned[index];
            descriptor.ClearActiveCount();
            uint capacity = phase == 0 ? descriptor.Limits.PrimaryInitialCapacity20 : descriptor.Limits.SecondaryInitialCapacity2c;
            bool succeeded = descriptor.Resize(capacity, index, growthProvider, cancellationToken: cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!succeeded) return false;
        }
        if (traces is not null)
            for (int index = 0; index < traces.Length; index++)
                traces[index].Reset(index, traceRelease, cancellationToken);
        return true;
    }
}
