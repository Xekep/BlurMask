namespace InkRecognition;

/// <summary>Literal finite-fraction nontracing 212ec0 global quota decisions,
/// over explicit prepared descriptors/configuration and allocator growth bytes.
/// Descriptor-limit production and native word winner/confidence are separate.</summary>
public static class LegacyNativeWordQuotaController
{
    public static bool Apply(IReadOnlyList<LegacyNativeWordHeapDescriptor> descriptors,
        LegacyNativeWordControllerConfiguration configuration, float secondaryShareEc, int phase,
        LegacyNativeWordHeapGrowthProvider? growthProvider = null, CancellationToken cancellationToken = default,
        Action<int, ReadOnlyMemory<byte>>? observePromotionAttempt = null)
    {
        ArgumentNullException.ThrowIfNull(descriptors);
        cancellationToken.ThrowIfCancellationRequested();
        if (!float.IsFinite(secondaryShareEc) || secondaryShareEc > 1 ||
            !float.IsFinite(configuration.ExpansionFactor230c))
            throw new NotSupportedException("Checked quota scope requires finite factors and a secondary fraction no greater than one.");
        if (configuration.Trace2368 != 0)
            throw new NotSupportedException("Trace allocation is separate from nontracing quota storage.");
        if (descriptors.Count > 512)
            throw new NotSupportedException("Explicit bucket array exceeds this checked controller subset.");
        for (int index = 0; index < descriptors.Count; index++)
        {
            ArgumentNullException.ThrowIfNull(descriptors[index]);
            for (int other = 0; other < index; other++)
                if (ReferenceEquals(descriptors[index], descriptors[other]))
                    throw new ArgumentException("Native bucket descriptors require separate owned storage.");
        }

        uint budget = configuration.PrimaryArcLimit230a;
        if (phase != 0)
        {
            float product = ((configuration.PackedSource22e8 & 0x10) != 0 ? 2f : 1f) * configuration.ExpansionFactor230c;
            product *= configuration.PrimaryArcLimit230a;
            budget = TruncateUInt32(product);
        }
        uint total = 0;
        foreach (var descriptor in descriptors) total = unchecked(total + (uint)descriptor.Count);
        if (total <= budget) return true;

        bool positive = secondaryShareEc > 0;
        int half = descriptors.Count >> 1;
        int primaryEnd = positive ? half : descriptors.Count;
        if (positive && (descriptors.Count < 2 || (descriptors.Count & 1) != 0))
            throw new NotSupportedException("Positive-share scope needs the paired bucket array produced by 1fbb90.");

        void Promote(int target, ReadOnlyMemory<byte> record)
        {
            observePromotionAttempt?.Invoke(target, record.ToArray());
            descriptors[target].TryInsertRecord(record);
        }
        bool Resize(int index, uint capacity, Action<ReadOnlyMemory<byte>>? callback = null) =>
            descriptors[index].Resize(capacity, index, growthProvider, callback, cancellationToken);
        void SecondaryFallback(ReadOnlyMemory<byte> record)
        {
            // 1f8be0: try secondary0; its eviction promotes into primary0;
            // if secondary0 rejects, try the incoming record in primary0.
            observePromotionAttempt?.Invoke(half, record.ToArray());
            if (!descriptors[half].TryInsertRecord(record, root => Promote(0, root)))
                Promote(0, record);
        }

        if (positive)
        {
            for (int index = half + 1; index < descriptors.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var limits = descriptors[index].Limits;
                uint minimum = phase == 0 ? limits.PrimaryMinimum18 : limits.SecondaryMinimum24;
                uint transition = phase == 0 ? limits.PrimaryTransition1c : limits.SecondaryTransition28;
                if (minimum < descriptors[index].Count)
                {
                    int primaryTarget = index - half;
                    if (!Resize(index, transition, record => Promote(primaryTarget, record))) return false;
                    if (minimum < transition && !Resize(index, minimum, SecondaryFallback)) return false;
                }
            }
        }
        for (int index = 1; index < primaryEnd; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var limits = descriptors[index].Limits;
            uint minimum = phase == 0 ? limits.PrimaryMinimum18 : limits.SecondaryMinimum24;
            uint transition = phase == 0 ? limits.PrimaryTransition1c : limits.SecondaryTransition28;
            if (minimum < descriptors[index].Count)
            {
                if (!Resize(index, transition)) return false;
                if (minimum < transition && !Resize(index, minimum, record => Promote(0, record))) return false;
            }
        }

        int others = 0;
        for (int index = 1; index < descriptors.Count; index++)
            if (!positive || index != half) others = unchecked(others + descriptors[index].Count);
        int remaining = unchecked((int)budget - others);
        while (remaining < 1)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int selected = -1;
            int worstCost = -0x4000000;
            for (int index = 1; index < descriptors.Count; index++)
            {
                if ((!positive || index != half) && descriptors[index].Count != 0 &&
                    worstCost < descriptors[index].RootCost)
                {
                    selected = index;
                    worstCost = descriptors[index].RootCost;
                }
            }
            if (selected < 0)
                throw new NotSupportedException("Native sentinel selection would address an unavailable bucket; no winner is invented.");
            remaining = unchecked(remaining + 1);
            if (!Resize(selected, (uint)(descriptors[selected].Count - 1))) return false;
        }
        if (positive)
        {
            float scaled = (float)remaining * secondaryShareEc;
            uint secondaryCapacity = TruncateUInt32(scaled);
            remaining = unchecked(remaining - (int)secondaryCapacity);
            if (!Resize(half, secondaryCapacity, record => Promote(0, record))) return false;
        }
        return Resize(0, unchecked((uint)remaining));
    }

    private static uint TruncateUInt32(float value)
    {
        if (!float.IsFinite(value) || value < -9223372036854775808f || value >= 9223372036854775808f)
            throw new NotSupportedException("Native float-to-long conversion is outside the checked finite range.");
        return unchecked((uint)(long)value);
    }
}
