using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Owned logical node storage for fresh DIRECT/CASE2/2/1 and the
/// named preconstruction-property CASE2/2/2 controller.
/// Allocation bytes are supplied explicitly. Grouped1FA300 allocation/ring
///pointer identity is not reconstructed by this logical row owner.</summary>
public sealed class LegacyOwnedFreshMainWordNodeBuffer
{
    private readonly byte[] records, primary, secondary;
    private readonly LegacyOwnedFreshMainWordControllerPolicy policy;
    private bool languageStarted, compacted;
    private int primaryWritten, languageWritten;
    public int EndingNode { get; }
    public int Capacity { get; }
    public ushort PrimaryLimit => policy.Configuration.PrimaryArcLimit230a;
    public ushort SourceEnd23f0 { get; private set; }
    public ushort RetainedEnd23f8 { get; private set; }

    public LegacyOwnedFreshMainWordNodeBuffer(LegacyOwnedFreshMainWordControllerPolicy policy, int endingNode,
        ReadOnlySpan<byte> actualRecordAllocation, ReadOnlySpan<byte> actualPrimaryAlternativeAllocation,
        CancellationToken cancellationToken = default)
        : this(policy, endingNode, actualRecordAllocation, actualPrimaryAlternativeAllocation, [], cancellationToken) { }

    public LegacyOwnedFreshMainWordNodeBuffer(LegacyOwnedFreshMainWordControllerPolicy policy, int endingNode,
        ReadOnlySpan<byte> actualRecordAllocation, ReadOnlySpan<byte> actualPrimaryAlternativeAllocation,
        ReadOnlySpan<byte> actualSecondaryAlternativeAllocation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(policy);
        cancellationToken.ThrowIfCancellationRequested();
        var configuration = policy.Configuration;
        if (configuration.PrimaryAlternativeCount2402 != 2 ||
            !(configuration.SecondaryAlternativeCount2404 == 1 && policy.SecondaryCountProperty12d0003 is null ||
              configuration.SecondaryAlternativeCount2404 == 2 && policy.SecondaryCountProperty12d0003 == 2) ||
            configuration.Trace2368 != 0 || configuration.Mode22f6 != 0 ||
            (uint)endingNode >= (uint)policy.Owner.SourceGraph.NodeCount)
            throw new NotSupportedException("Fresh owned2/2/1 or named configured CASE2/2/2 node required.");
        float expansion = ((configuration.PackedSource22e8 & 0x10) != 0 ? 2f : 1f) * configuration.ExpansionFactor230c;
        expansion *= configuration.PrimaryArcLimit230a;
        int extent = checked(configuration.PrimaryArcLimit230a + (int)expansion);
        if (extent > ushort.MaxValue || actualRecordAllocation.Length != extent * 88 ||
            actualPrimaryAlternativeAllocation.Length != configuration.PrimaryArcLimit230a * 80 ||
            actualSecondaryAlternativeAllocation.Length != (configuration.SecondaryAlternativeCount2404 > 1
                ? extent * configuration.SecondaryAlternativeCount2404 * 40 : 0))
            throw new ArgumentException("Explicit full row/primary allocation must match the native prepared extent.");
        this.policy = policy; EndingNode = endingNode; Capacity = extent;
        records = actualRecordAllocation.ToArray(); primary = actualPrimaryAlternativeAllocation.ToArray();
        secondary = actualSecondaryAlternativeAllocation.ToArray();
        //2144F0 initializes only first125 row costs and the first alternative's
        //two costs. Other allocation bytes, including the second alternative,
        //retain their explicit owner until the actual213410 write.
        for (int ordinal = 0; ordinal < PrimaryLimit; ordinal++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Write32(records, ordinal * 88 + 16, 0x4000000);
            Write32(records, ordinal * 88 + 20, 0x4000000);
            Write32(primary, ordinal * 80, 0x4000000);
            Write32(primary, ordinal * 80 + 4, 0x4000000);
            if (configuration.SecondaryAlternativeCount2404 > 1)
            {
                Write32(secondary, ordinal * configuration.SecondaryAlternativeCount2404 * 40, 0x4000000);
                Write32(secondary, ordinal * configuration.SecondaryAlternativeCount2404 * 40 + 4, 0x4000000);
            }
        }
        SourceEnd23f0 = RetainedEnd23f8 = PrimaryLimit;
    }

    public void WritePrimary(ReadOnlySpan<byte> actualSlot128, ReadOnlySpan<byte> actualPrimaryPool,
        CancellationToken cancellationToken = default)
        => WritePrimary(actualSlot128, actualPrimaryPool, [], cancellationToken);

    public void WritePrimary(ReadOnlySpan<byte> actualSlot128, ReadOnlySpan<byte> actualPrimaryPool,
        ReadOnlySpan<byte> actualSecondaryPool, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (languageStarted || primaryWritten >= PrimaryLimit)
            throw new InvalidOperationException("Native primary materialization must precede language within its quota.");
        var row = LegacyNativeWordRowMaterializer.Materialize(actualSlot128, checked((ushort)primaryWritten),
            PrimaryLimit, 2, policy.Configuration.SecondaryAlternativeCount2404, actualPrimaryPool,
            actualSecondaryPool, cancellationToken);
        row.Record.Span.CopyTo(records.AsSpan(primaryWritten * 88));
        row.PrimaryAlternatives.Span.CopyTo(primary.AsSpan(primaryWritten * 80));
        if (secondary.Length != 0) row.SecondaryAlternatives.Span.CopyTo(secondary.AsSpan(primaryWritten * 80));
        primaryWritten++;
    }

    public void BeginLanguage(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (languageStarted) throw new InvalidOperationException("Native node language phase has already started.");
        for (int ordinal = 0; ordinal < PrimaryLimit; ordinal++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            BinaryPrimitives.WriteUInt16LittleEndian(records.AsSpan(ordinal * 88 + 62), ushort.MaxValue);
        }
        languageStarted = true;
    }

    public void WriteLanguage(ReadOnlySpan<byte> actualSlot128, CancellationToken cancellationToken = default)
        => WriteLanguage(actualSlot128, [], cancellationToken);

    public void WriteLanguage(ReadOnlySpan<byte> actualSlot128, ReadOnlySpan<byte> actualSecondaryPool,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int ordinal = PrimaryLimit + languageWritten;
        if (!languageStarted || compacted || ordinal >= Capacity)
            throw new InvalidOperationException("Native language materialization needs the started node and its expanded quota.");
        var row = LegacyNativeWordRowMaterializer.Materialize(actualSlot128, checked((ushort)ordinal),
            PrimaryLimit, 2, policy.Configuration.SecondaryAlternativeCount2404,
            secondaryAlternativeStorage2430: actualSecondaryPool, cancellationToken: cancellationToken);
        row.Record.Span.CopyTo(records.AsSpan(ordinal * 88));
        if (secondary.Length != 0) row.SecondaryAlternatives.Span.CopyTo(secondary.AsSpan(ordinal * 80));
        languageWritten++;
        SourceEnd23f0 = checked((ushort)(PrimaryLimit + languageWritten));
    }

    public void CompactLanguage(LegacyNativeStoredWordComparer comparer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        cancellationToken.ThrowIfCancellationRequested();
        if (!languageStarted || compacted) throw new InvalidOperationException("Compaction follows actual language materialization exactly once.");
        var result = LegacyNativeStoredWordCompaction.Prepare(records, SourceEnd23f0, PrimaryLimit,
            policy.Configuration.SecondaryAlternativeCount2404, secondary, comparer, cancellationToken);
        result.FullRecordStorage.Span.CopyTo(records);
        result.FullSecondaryAlternativeStorage.Span.CopyTo(secondary);
        RetainedEnd23f8 = result.RetainedCount;
        CleanupPrimaryAlternatives(cancellationToken);
        compacted = true;
    }

    /// <summary>2144F0's false language-gate branch writes both counts to the
    ///primary limit, bypasses212B10, and still performs the common alternative
    ///cleanup. It must not sort or deduplicate the primary source rows again.</summary>
    public void FinishWithoutLanguage(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!languageStarted || compacted || languageWritten != 0)
            throw new InvalidOperationException("The false language branch requires a started node without language writes.");
        SourceEnd23f0 = RetainedEnd23f8 = PrimaryLimit;
        CleanupPrimaryAlternatives(cancellationToken);
        compacted = true;
    }

    private void CleanupPrimaryAlternatives(CancellationToken cancellationToken)
    {
        //2144F0 invalidates primary alternatives whose unreset parent points
        //into the secondary ordinals. Only cost00 is written in this cleanup.
        for (int index = 0; index < PrimaryLimit * 2; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var alternative = primary.AsSpan(index * 40, 40);
            if (BinaryPrimitives.ReadInt32LittleEndian(alternative) < 0x4000000 &&
                (alternative[31] & 32) == 0 && BinaryPrimitives.ReadUInt16LittleEndian(alternative[24..]) >= PrimaryLimit)
                BinaryPrimitives.WriteInt32LittleEndian(alternative, 0x4000000);
        }
    }

    public ReadOnlyMemory<byte> CopyRecordStorage() => records.ToArray();
    public ReadOnlyMemory<byte> CopySecondaryStorage() => secondary.ToArray();
    //207CB0 changes only publication marks and compact-relative indices in
    //already completed source rows. Validate the entire allocation before
    //writing anything: future2144F0 parents still own every other byte.
    internal void ApplyCompactPublication(LegacyOwnedFreshMainWordControllerPolicy producingOwner,
        LegacyMainDirectWordNodeResult published, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (secondary.Length != 0)
            throw new NotSupportedException("Configured secondary compact publication requires its own2410 writeback bridge.");
        if (!ReferenceEquals(producingOwner, policy) || !compacted || published.EndingNode != EndingNode ||
            published.SourceEnd23f0 != SourceEnd23f0 || published.RetainedEnd23f8 != RetainedEnd23f8 ||
            published.Records.Length != records.Length || published.PrimaryAlternatives.Length != primary.Length)
            throw new ArgumentException("Publication must retain the completed node's exact owner, counts and allocation extents.");
        var updatedRecords = published.Records.Span;
        var updatedPrimary = published.PrimaryAlternatives.Span;
        for (int index = 0; index < records.Length; index++)
        {
            if ((index & 1023) == 0) cancellationToken.ThrowIfCancellationRequested();
            int ordinal = index / 88, field = index % 88;
            int allowed = ordinal < SourceEnd23f0 ? field == 6 ? 2 : field is 0x22 or 0x23 ? 255 : 0 : 0;
            if (((records[index] ^ updatedRecords[index]) & ~allowed) != 0)
                throw new InvalidDataException("Compact publication cannot change another source-row field.");
        }
        for (int index = 0; index < primary.Length; index++)
        {
            if ((index & 1023) == 0) cancellationToken.ThrowIfCancellationRequested();
            int field = index % 40;
            int allowed = field == 0x1b ? 128 : field is 0x1c or 0x1d ? 255 : 0;
            if (((primary[index] ^ updatedPrimary[index]) & ~allowed) != 0)
                throw new InvalidDataException("Compact publication cannot change another primary-alternative field.");
        }
        cancellationToken.ThrowIfCancellationRequested();
        //No callbacks/cancellation points between the two owned-array copies.
        updatedRecords.CopyTo(records);
        updatedPrimary.CopyTo(primary);
    }

    internal void ApplyPeriodicCostOffset(int positiveMinimum, CancellationToken cancellationToken)
    {
        if (!compacted) throw new InvalidOperationException("Periodic normalization follows completed node materialization.");
        if (secondary.Length != 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (positiveMinimum is < 1 or >= 0x4000000) throw new ArgumentOutOfRangeException(nameof(positiveMinimum));
            for (int ordinal = 0; ordinal < SourceEnd23f0; ordinal++)
            {
                for (int k = 0; k < policy.Configuration.SecondaryAlternativeCount2404; k++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var alternative = secondary.AsSpan((ordinal * policy.Configuration.SecondaryAlternativeCount2404 + k) * 40, 40);
                    int cost = BinaryPrimitives.ReadInt32LittleEndian(alternative);
                    if (cost >= 0x4000000) break;
                    BinaryPrimitives.WriteInt32LittleEndian(alternative, unchecked(cost - positiveMinimum));
                }
                var row = records.AsSpan(ordinal * 88, 88);
                int rowCost = BinaryPrimitives.ReadInt32LittleEndian(row[16..]);
                if (rowCost < 0x4000000)
                    BinaryPrimitives.WriteInt32LittleEndian(row[16..], unchecked(rowCost - positiveMinimum));
            }
            return;
        }
        var shifted = LegacyNativeWordPeriodicCostNormalization.ApplySourceRows(records, SourceEnd23f0,
            positiveMinimum, cancellationToken);
        shifted.Span.CopyTo(records);
    }
    public ReadOnlyMemory<byte> CopyPrimaryStorage() => primary.ToArray();
    public LegacyNativeStoredWordRecord Read(ushort ordinal)
    {
        if (ordinal >= SourceEnd23f0) throw new ArgumentOutOfRangeException(nameof(ordinal));
        return new(records.AsSpan(ordinal * 88, 88).ToArray(),
            ordinal < PrimaryLimit ? primary.AsSpan(ordinal * 80, 80).ToArray() : [],
            secondary.Length == 0 ? [] : secondary.AsSpan(ordinal * 80, 80).ToArray());
    }
    /// <summary>203690 reads source23F0;1FE740/203C90 instead use retained23F8.
    /// Both counts deliberately include the125 primary allocation/sentinels.</summary>
    public IReadOnlyList<LegacyNativeStoredWordRecord> CopyRows(bool retained = false)
    {
        var output = new LegacyNativeStoredWordRecord[retained ? RetainedEnd23f8 : SourceEnd23f0];
        for (ushort ordinal = 0; ordinal < output.Length; ordinal++) output[ordinal] = Read(ordinal);
        return Array.AsReadOnly(output);
    }
    private static void Write32(Span<byte> buffer, int offset, int value)
        => BinaryPrimitives.WriteInt32LittleEndian(buffer[offset..], value);
}
