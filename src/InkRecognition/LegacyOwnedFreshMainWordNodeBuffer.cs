using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Owned logical node storage for the fresh DIRECT2/2/1 controller.
/// Allocation bytes are supplied explicitly. Grouped1FA300 allocation/ring
///pointer identity is not reconstructed by this logical row owner.</summary>
public sealed class LegacyOwnedFreshMainWordNodeBuffer
{
    private readonly byte[] records, primary;
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
    {
        ArgumentNullException.ThrowIfNull(policy);
        cancellationToken.ThrowIfCancellationRequested();
        var configuration = policy.Configuration;
        if (configuration.PrimaryAlternativeCount2402 != 2 || configuration.SecondaryAlternativeCount2404 != 1 ||
            configuration.Trace2368 != 0 || configuration.Mode22f6 != 0 ||
            (uint)endingNode >= (uint)policy.Owner.Source.SourceRanges.Graph.NodeCount)
            throw new NotSupportedException("Fresh owned nontracing DIRECT2/2/1 node required.");
        float expansion = ((configuration.PackedSource22e8 & 0x10) != 0 ? 2f : 1f) * configuration.ExpansionFactor230c;
        expansion *= configuration.PrimaryArcLimit230a;
        int extent = checked(configuration.PrimaryArcLimit230a + (int)expansion);
        if (extent > ushort.MaxValue || actualRecordAllocation.Length != extent * 88 ||
            actualPrimaryAlternativeAllocation.Length != configuration.PrimaryArcLimit230a * 80)
            throw new ArgumentException("Explicit full row/primary allocation must match the native prepared extent.");
        this.policy = policy; EndingNode = endingNode; Capacity = extent;
        records = actualRecordAllocation.ToArray(); primary = actualPrimaryAlternativeAllocation.ToArray();
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
        }
        SourceEnd23f0 = RetainedEnd23f8 = PrimaryLimit;
    }

    public void WritePrimary(ReadOnlySpan<byte> actualSlot128, ReadOnlySpan<byte> actualPrimaryPool,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (languageStarted || primaryWritten >= PrimaryLimit)
            throw new InvalidOperationException("Native primary materialization must precede language within its quota.");
        var row = LegacyNativeWordRowMaterializer.Materialize(actualSlot128, checked((ushort)primaryWritten),
            PrimaryLimit, 2, 1, actualPrimaryPool, cancellationToken: cancellationToken);
        row.Record.Span.CopyTo(records.AsSpan(primaryWritten * 88));
        row.PrimaryAlternatives.Span.CopyTo(primary.AsSpan(primaryWritten * 80));
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
    {
        cancellationToken.ThrowIfCancellationRequested();
        int ordinal = PrimaryLimit + languageWritten;
        if (!languageStarted || compacted || ordinal >= Capacity)
            throw new InvalidOperationException("Native language materialization needs the started node and its expanded quota.");
        var row = LegacyNativeWordRowMaterializer.Materialize(actualSlot128, checked((ushort)ordinal),
            PrimaryLimit, 2, 1, cancellationToken: cancellationToken);
        row.Record.Span.CopyTo(records.AsSpan(ordinal * 88));
        languageWritten++;
        SourceEnd23f0 = checked((ushort)(PrimaryLimit + languageWritten));
    }

    public void CompactLanguage(LegacyNativeStoredWordComparer comparer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        cancellationToken.ThrowIfCancellationRequested();
        if (!languageStarted || compacted) throw new InvalidOperationException("Compaction follows actual language materialization exactly once.");
        var result = LegacyNativeStoredWordCompaction.Prepare(records, SourceEnd23f0, PrimaryLimit, 1, [], comparer, cancellationToken);
        result.FullRecordStorage.Span.CopyTo(records);
        RetainedEnd23f8 = result.RetainedCount;
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
        compacted = true;
    }

    public ReadOnlyMemory<byte> CopyRecordStorage() => records.ToArray();
    public ReadOnlyMemory<byte> CopyPrimaryStorage() => primary.ToArray();
    public LegacyNativeStoredWordRecord Read(ushort ordinal)
    {
        if (ordinal >= SourceEnd23f0) throw new ArgumentOutOfRangeException(nameof(ordinal));
        return new(records.AsSpan(ordinal * 88, 88).ToArray(),
            ordinal < PrimaryLimit ? primary.AsSpan(ordinal * 80, 80).ToArray() : [], []);
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
