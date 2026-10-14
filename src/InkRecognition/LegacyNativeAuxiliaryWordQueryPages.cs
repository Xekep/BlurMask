using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativeAuxiliaryWordQueryPage(int SourceReturnedCount, int AcceptedSkip,
    bool InjectedCurrentTerminal, ReadOnlyMemory<byte> Records32, ReadOnlyMemory<byte> BufferSnapshot);

public sealed record LegacyNativeAuxiliaryWordQueryResult(bool Completed, int SourceReadCount,
    int FinalAcceptedSkip, IReadOnlyList<LegacyNativeAuxiliaryWordQueryPage> ConsumedPages);

/// <summary>
/// Ordinary204290 reused1648 request -> aggregate.virtual48 -> source-owned
/// auxiliary pages -> one current-terminal injection. The selected source chain,
/// live2600/2601/2602/254C fields and shared context container remain owned inputs.
/// Associated2300-category bypass is not inferred. Frontend280 and its
/// actual graph-input flags are separate explicit getter results.
/// </summary>
public sealed class LegacyNativeAuxiliaryWordQueryPages
{
    private readonly LegacyPreparedAggregateDictionaryQuerySource source;
    private readonly LegacyPreparedDictionaryQuery query;
    private readonly byte[] buffer;
    private bool valid = true;
    private int active;

    /// <param name="bufferAllocationResidue">Full capacity*32 output allocation,
    /// including bytes24..31 which current-terminal injection does not write.</param>
    public LegacyNativeAuxiliaryWordQueryPages(LegacyPreparedAggregateDictionaryQuerySource source,
        LegacySymbolContextFactory contexts, int preparedCapacity, ReadOnlySpan<byte> bufferAllocationResidue)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(contexts);
        if (preparedCapacity is < 1 or > 512 || bufferAllocationResidue.Length != preparedCapacity * 32)
            throw new ArgumentException("Actual prepared capacity and complete output-buffer allocation required.");
        this.source = source;
        query = new() { Capacity = preparedCapacity, ContextRoot = contexts.ContainerIdentity };
        buffer = bufferAllocationResidue.ToArray();
    }

    public LegacyNativeAuxiliaryWordQueryResult Run(ReadOnlySpan<byte> currentStored88, int endingNode,
        LegacyOrdinaryDictionaryQueryPolicy policy, ushort inputFlags, bool frontend280, bool sourceMode2600,
        Func<LegacyNativeAuxiliaryWordQueryPage, CancellationToken, bool> consumePage,
        int maximumReadIterations, CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Native auxiliary query calls must be serialized.");
        try
        {
            if (!valid) throw new InvalidOperationException("Failed query preparation requires a fresh session.");
            return RunCore(currentStored88, endingNode, policy, inputFlags, frontend280, sourceMode2600,
                consumePage, maximumReadIterations, cancellationToken);
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    private LegacyNativeAuxiliaryWordQueryResult RunCore(ReadOnlySpan<byte> currentStored88, int endingNode,
        LegacyOrdinaryDictionaryQueryPolicy policy, ushort inputFlags, bool frontend280, bool sourceMode2600,
        Func<LegacyNativeAuxiliaryWordQueryPage, CancellationToken, bool> consumePage,
        int maximumReadIterations, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(consumePage);
        cancellationToken.ThrowIfCancellationRequested();
        if (currentStored88.Length != 88 || endingNode < 0 || maximumReadIterations is < 1 or > 2_000_001)
            throw new ArgumentException("Actual current row, source node and explicit iteration bound required.");
        byte[] row = currentStored88.ToArray();
        if (!frontend280 && inputFlags != 0)
            throw new ArgumentException("204290 non-special frontend retains its initialized zero input flags.");
        ulong tag = BinaryPrimitives.ReadUInt64LittleEndian(row);
        if (row[4] == 0 || (uint)tag > int.MaxValue)
            throw new NotSupportedException("An actual selected-source current tag with bounded dictionary node is required.");
        query.Node = (int)(uint)tag;
        query.ParentContext = 0;
        query.Category18 = 0;
        bool limited = frontend280 ? (inputFlags & 3) != 0 : policy.Mode2601 != 0;
        query.Parameter19 = limited && row[38] < policy.Limit2602
            ? (byte)(policy.Limit2602 - row[38]) : (byte)0;
        query.Parameter1c = (uint)endingNode;
        //Native sign extends the stored char20 before the full32-bit write.
        query.Parameter20 = unchecked((uint)(int)(sbyte)row[32]);
        query.Flags24 = policy.Boundary254c && (inputFlags & 0x10) != 0 ? (ushort)4 : (ushort)0;
        query.Flags26 = inputFlags;
        query.Skip = 0;
        bool pendingTerminal = (row[5] & 2) != 0 && (row[7] & 1) == 0;
        bool bypassSource = sourceMode2600 || (row[6] & 1) != 0;
        var consumed = new List<LegacyNativeAuxiliaryWordQueryPage>();
        int sourceReads = 0;
        for (int iteration = 0; iteration < maximumReadIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int returned = 0;
            if (!bypassSource)
            {
                var page = source.Read(tag, query, auxiliary: true, clearExtraCosts: false, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                sourceReads++;
                returned = page.ReturnedCount;
                //Native-1 abort can reach a negative output slot when a pending
                //terminal exists. Refuse that unsafe pointer-domain branch.
                if (returned < 0)
                    throw new NotSupportedException("Negative native auxiliary count cannot safely index the output buffer.");
                if (returned > query.Capacity || page.Records.Length != returned * 32 || !page.ExtraCosts.IsEmpty)
                    throw new InvalidDataException("Auxiliary page differs from the actual prepared output extent.");
                page.Records.Span.CopyTo(buffer);
            }
            query.Skip = checked(query.Skip + returned);
            if (query.Skip > 2_000_000)
                throw new NotSupportedException("Accepted dictionary skip exceeds the checked source domain.");
            bool injected = pendingTerminal && returned < query.Capacity;
            int count = returned;
            if (injected)
            {
                var terminal = LegacyNativeFirstDictionaryWordCompletion.CreateAlreadyTerminalQuery(
                    row, buffer.AsSpan(returned * 32, 32), cancellationToken);
                if (!terminal.HasValue) throw new InvalidDataException("Current-terminal predicate changed after its snapshot.");
                terminal.Value.Span.CopyTo(buffer.AsSpan(returned * 32, 32));
                pendingTerminal = false;
                count++;
            }
            if (count == 0)
                return new(true, sourceReads, query.Skip, consumed.AsReadOnly());
            var output = new LegacyNativeAuxiliaryWordQueryPage(returned, query.Skip, injected,
                buffer.AsSpan(0, count * 32).ToArray(), buffer.ToArray());
            consumed.Add(output);
            bool accepted = consumePage(output, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!accepted)
            {
                valid = false;
                return new(false, sourceReads, query.Skip, consumed.AsReadOnly());
            }
        }
        throw new InvalidOperationException("Explicit auxiliary iteration bound exhausted before the native zero-count stop.");
    }
}
