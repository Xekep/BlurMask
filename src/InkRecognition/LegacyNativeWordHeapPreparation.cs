using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>The compact prepared source header returned by source.virtual28.
/// For dictionary2b7,093aa0 returns object+20, so its weights are24/28.</summary>
public readonly record struct LegacyNativeWordPreparedSource(uint Header, float MinimumWeight, float TransitionWeight)
{
    public static LegacyNativeWordPreparedSource FromDictionary(LegacyPreparedDictionarySourceDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        return new(descriptor.Flags, descriptor.Weight24, descriptor.Weight28);
    }
}

/// <summary>Fresh20d8f0 scalar source summary; indices address the supplied
/// controller source order, not aggregate selected-source original indices.</summary>
public readonly record struct LegacyNativeWordSourceSummary(uint PackedSource22e8,
    int SubwordSourceIndex22ec, int OverrideSourceIndex22f0);

public sealed record LegacyNativeWordHeapInitialization(bool Succeeded,
    IReadOnlyList<LegacyNativeWordHeapDescriptor> Descriptors, int InitializedDescriptorCount);

/// <summary>Prepared1fbb90 quota fields, source buckets and actual initial
/// allocation producer. Descriptor-array allocation must already have succeeded;
/// record backing bytes remain supplied by the caller's explicit allocator.</summary>
public sealed class LegacyNativeWordHeapPreparation
{
    private readonly LegacyNativeWordHeapQuotaLimits[] limits;
    private readonly byte[] buckets;
    public LegacyNativeWordSourceSummary Sources { get; }
    public LegacyNativeWordControllerConfiguration Configuration { get; }
    public float SecondaryShareEc { get; }
    public IReadOnlyList<LegacyNativeWordHeapQuotaLimits> Limits { get; }
    public ReadOnlyMemory<byte> CopySourceBuckets() => buckets.ToArray();

    private LegacyNativeWordHeapPreparation(LegacyNativeWordHeapQuotaLimits[] limits, byte[] buckets,
        LegacyNativeWordSourceSummary sources, LegacyNativeWordControllerConfiguration configuration, float share)
    {
        this.limits = limits; this.buckets = buckets; Sources = sources;
        Configuration = configuration; SecondaryShareEc = share;
        Limits = Array.AsReadOnly(limits);
    }

    /// <summary>Fresh20d8f0 starts packed flags at zero and both special source
    /// indices at-1. Last matching category800/A00 writes its source index.</summary>
    public static LegacyNativeWordSourceSummary SummarizeFresh(ReadOnlySpan<LegacyNativeWordPreparedSource> sources,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (sources.Length > 255) throw new NotSupportedException("Checked fresh source array permits at most255 entries.");
        uint packed = 0; int subword = -1, special = -1;
        for (int index = 0; index < sources.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = sources[index];
            if (!float.IsFinite(source.MinimumWeight) || !float.IsFinite(source.TransitionWeight))
                throw new ArgumentException("Finite prepared source weights required.");
            uint header = source.Header, category = header & 0x1e00;
            packed = (packed & ~7u) | Math.Max(packed & 7, (header >> 2) & 7);
            packed |= (header >> 2) & 8;
            if ((header & 0x20) != 0 && category != 0x800) packed |= 0x10;
            if (category == 0xa00) { packed |= 0x20; special = index; }
            packed |= (header >> 1) & 0xc0;
            if (category == 0x800) subword = index;
        }
        return new(packed, subword, special);
    }

    public static LegacyNativeWordHeapPreparation PrepareFresh(ReadOnlySpan<LegacyNativeWordPreparedSource> sources,
        LegacyNativeWordControllerConfiguration configuration, float secondaryShareEc,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!float.IsFinite(secondaryShareEc) || secondaryShareEc is < 0 or > 1 ||
            !float.IsFinite(configuration.ExpansionFactor230c) || configuration.OverrideHeap2458 != uint.MaxValue)
            throw new NotSupportedException("Fresh preparation needs finite factors, share0..1 and an unset override heap.");
        var summary = SummarizeFresh(sources, cancellationToken);
        configuration = configuration with { PackedSource22e8 = summary.PackedSource22e8 };
        uint budget = configuration.PrimaryArcLimit230a;
        float expanded = (summary.PackedSource22e8 & 0x10) != 0 ? 2f : 1f;
        expanded *= configuration.ExpansionFactor230c;
        expanded *= configuration.PrimaryArcLimit230a;
        uint expandedBudget = TruncateUInt32(expanded);
        var fields = new List<LegacyNativeWordHeapQuotaLimits> { default };
        var buckets = new byte[sources.Length];
        int primaryMinimumSum = 0, secondaryMinimumSum = 0;
        for (int index = 0; index < sources.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = sources[index];
            if (source.MinimumWeight == 0 && source.TransitionWeight == 1) continue;
            buckets[index] = unchecked((byte)fields.Count);
            uint primaryMinimum = Minimum(budget, source.MinimumWeight);
            uint primaryTransition = Weighted(budget, source.TransitionWeight);
            uint secondaryMinimum = Minimum(expandedBudget, source.MinimumWeight);
            uint secondaryTransition = Weighted(expandedBudget, source.TransitionWeight);
            fields.Add(new(primaryMinimum, primaryTransition, unchecked(primaryTransition * 2 + 2),
                secondaryMinimum, secondaryTransition, unchecked(secondaryTransition * 4 + 1), index));
            primaryMinimumSum = unchecked(primaryMinimumSum + (int)primaryMinimum);
            secondaryMinimumSum = unchecked(secondaryMinimumSum + (int)secondaryMinimum);
        }
        int primaryCount = fields.Count;
        if (secondaryShareEc == 0)
        {
            int matched = fields.FindIndex(row => row.SourceIndex30 == summary.OverrideSourceIndex22f0);
            if (matched >= 0)
            {
                configuration = configuration with { OverrideHeap2458 = (uint)fields.Count };
                var row = fields[matched];
                primaryMinimumSum = unchecked(primaryMinimumSum + (int)row.PrimaryMinimum18);
                fields.Add(row with { SourceIndex30 = -1, SecondaryMinimum24 = 0,
                    SecondaryTransition28 = 0, SecondaryInitialCapacity2c = 0 });
            }
        }
        else
        {
            // ASM1fbcac passes r9d=the entire pre-split descriptor count.
            // 1f77b0 zeroes every new56-byte descriptor, including source30.
            for (int index = 0; index < primaryCount; index++) fields.Add(default);
            for (int index = 1; index < primaryCount; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var original = fields[index];
                fields[index] = Scale(original, 1f - secondaryShareEc);
                fields[index + primaryCount] = Scale(original, secondaryShareEc);
            }
        }
        int remainder = Math.Max(2, unchecked((int)budget - primaryMinimumSum));
        uint expandedRemainder = (uint)Math.Max(1, unchecked((int)expandedBudget - secondaryMinimumSum));
        uint primaryCapacity = unchecked((uint)(remainder * 2));
        uint expandedCapacity = unchecked(expandedRemainder * 4);
        var general = new LegacyNativeWordHeapQuotaLimits(uint.MaxValue, uint.MaxValue, primaryCapacity,
            uint.MaxValue, uint.MaxValue, expandedCapacity, -1);
        if (secondaryShareEc == 0) fields[0] = general;
        else
        {
            fields[0] = general with { PrimaryInitialCapacity20 = Weighted(primaryCapacity, 1f - secondaryShareEc),
                SecondaryInitialCapacity2c = Weighted(expandedCapacity, 1f - secondaryShareEc) };
            // Native copies18..2F only. Source30 of secondary general stays0
            // from the allocator's proven memset, unlike primary general's-1.
            fields[primaryCount] = fields[0] with { PrimaryInitialCapacity20 = Weighted(primaryCapacity, secondaryShareEc),
                SecondaryInitialCapacity2c = Weighted(expandedCapacity, secondaryShareEc), SourceIndex30 = 0 };
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(fields.ToArray(), buckets, summary, configuration, secondaryShareEc);
    }

    /// <summary>56-byte descriptor image before record allocation. All newly
    /// appended descriptor bytes really are zeroed by1f77b0; record slots are not.</summary>
    public ReadOnlyMemory<byte> CopyPreparedDescriptorStorage()
    {
        var storage = new byte[limits.Length * 56];
        for (int index = 0; index < limits.Length; index++)
        {
            var row = limits[index]; var target = storage.AsSpan(index * 56, 56);
            Write(target, 0x18, row.PrimaryMinimum18); Write(target, 0x1c, row.PrimaryTransition1c);
            Write(target, 0x20, row.PrimaryInitialCapacity20); Write(target, 0x24, row.SecondaryMinimum24);
            Write(target, 0x28, row.SecondaryTransition28); Write(target, 0x2c, row.SecondaryInitialCapacity2c);
            Write(target, 0x30, unchecked((uint)row.SourceIndex30));
        }
        return storage;
    }

    public LegacyNativeWordHeapInitialization Initialize(LegacyNativeWordHeapGrowthProvider growthProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(growthProvider); cancellationToken.ThrowIfCancellationRequested();
        var descriptors = limits.Select(row => new LegacyNativeWordHeapDescriptor([], 0, 0, 0, row)).ToArray();
        var view = Array.AsReadOnly(descriptors); int completed = 0;
        for (int index = 0; index < descriptors.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            uint capacity = Math.Max(limits[index].PrimaryInitialCapacity20, limits[index].SecondaryInitialCapacity2c);
            bool succeeded = descriptors[index].Resize(capacity, index, growthProvider, cancellationToken: cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!succeeded) return new(false, view, completed);
            completed++;
        }
        return new(true, view, completed);
    }

    private static LegacyNativeWordHeapQuotaLimits Scale(LegacyNativeWordHeapQuotaLimits row, float share)
        => new(row.PrimaryMinimum18 == 0 ? 0 : Weighted(row.PrimaryMinimum18, share),
            Weighted(row.PrimaryTransition1c, share), Weighted(row.PrimaryInitialCapacity20, share),
            row.SecondaryMinimum24 == 0 ? 0 : Weighted(row.SecondaryMinimum24, share),
            Weighted(row.SecondaryTransition28, share), Weighted(row.SecondaryInitialCapacity2c, share), row.SourceIndex30);
    private static uint Minimum(uint budget, float weight) => weight <= 0 ? 0 : Weighted(budget, weight);
    private static uint Weighted(uint value, float weight) => Math.Max(1u, TruncateUInt32((float)value * weight));
    private static uint TruncateUInt32(float value)
    {
        if (!float.IsFinite(value) || value < -9223372036854775808f || value >= 9223372036854775808f)
            throw new NotSupportedException("Native float-to-long conversion exceeds the checked finite range.");
        return unchecked((uint)(long)value);
    }
    private static void Write(Span<byte> destination, int offset, uint value)
        => BinaryPrimitives.WriteUInt32LittleEndian(destination[offset..], value);
}
