using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyNativeLanguageContextPolicy(bool CountCapEnabled, byte CountCap2602,
    bool RequireSecondaryFlag, bool ReuseKind6Geometry, byte Permission2309, ushort PrimaryLimit230a,
    ushort SecondaryEnd23f0);

public sealed record LegacyNativeLanguageContextRecombinationResult(bool InitialGeometryAccepted,
    ReadOnlyMemory<byte> Candidates16, ReadOnlyMemory<byte> LastRequest80, int MatchedRows, int GeometryCalls);

/// <summary>Ordinary203690 later-word context producer through full80
///118/140/F0F8 operations and the actual203640/203520 candidate heap.
///Source rows, frame callbacks, runtime limits and portable cost-cell identities
///remain owned explicit inputs. This is not whole204290 language scheduling.</summary>
public static class LegacyNativeLanguageContextRecombination
{
    private const int Impossible = 0x4000000;

    public static LegacyNativeLanguageContextRecombinationResult Collect(ReadOnlySpan<byte> currentRow88,
        ushort currentOrdinal, LegacyNativeWordParentRowLookup owningRowLookup,
        Func<int, IReadOnlyList<LegacyNativeStoredWordRecord>> owningNodeRowsLookup, Func<byte, uint> sourceHeader,
        LegacyCursiveCategoryProfile categories, LegacySymbolContextFactory contexts,
        LegacyNativeWordRecombinationSymbols symbols, LegacyNativeLanguageContextPolicy policy,
        LegacyNativeHigherArcTransitionOperations operations, ulong originalCostCellIdentity,
        ulong candidateCostCellIdentity, ReadOnlySpan<byte> requestResidue80, int candidateCapacity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(owningRowLookup);
        ArgumentNullException.ThrowIfNull(owningNodeRowsLookup);
        ArgumentNullException.ThrowIfNull(sourceHeader);
        ArgumentNullException.ThrowIfNull(categories);
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(operations);
        ArgumentNullException.ThrowIfNull(operations.Calculate118);
        ArgumentNullException.ThrowIfNull(operations.Validate140);
        ArgumentNullException.ThrowIfNull(operations.SuccessorsF0F8);
        cancellationToken.ThrowIfCancellationRequested();
        if (currentRow88.Length != 88 || requestResidue80.Length != 80 ||
            policy.PrimaryLimit230a > policy.SecondaryEnd23f0 ||
            candidateCapacity is < 0 or > ushort.MaxValue ||
            originalCostCellIdentity == 0 || candidateCostCellIdentity == 0 || originalCostCellIdentity == candidateCostCellIdentity)
            throw new ArgumentException("Actual bounded current/previous rows, secondary extents and distinct owned cost cells required.");
        //Snapshot current input and stack residue before external owner callbacks.
        byte[] current = currentRow88.ToArray();
        byte[] bytes = requestResidue80.ToArray();
        int anchorEnding = I32(current, 56);
        if (anchorEnding < 0 || U16(current, 60) == ushort.MaxValue)
            throw new NotSupportedException("Actual materialized nonroot word anchor required.");
        byte[] anchor = Lookup(anchorEnding, U16(current, 60));
        int span = (sbyte)anchor[32];
        int previousNode = unchecked(anchorEnding - span - 1);
        if (span is < 0 or > 14 || previousNode < 0)
            throw new NotSupportedException("Ordinary later-word anchor and nonnegative previous node required.");
        var previousRows = owningNodeRowsLookup(previousNode);
        cancellationToken.ThrowIfCancellationRequested();
        if (previousRows is null || previousRows.Count > ushort.MaxValue || policy.SecondaryEnd23f0 > previousRows.Count)
            throw new ArgumentException("Actual previous-node row owner and bounded secondary extent required.");
        //Snapshot the selected node collection before header/geometry callbacks.
        var previous = new byte[previousRows.Count][];
        for (int index = 0; index < previous.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var row = previousRows[index];
            if (row is null || row.Record.Length != 88) throw new ArgumentException("Each previous ordinal must own a complete stored88 row.");
            previous[index] = row.Record.ToArray();
        }
        ushort originalOrdinal = U16(anchor, 30);
        if (originalOrdinal >= previous.Length)
            throw new NotSupportedException("Ordinary later-word anchor and referenced previous-node ordinal required.");
        byte[] original = previous[originalOrdinal];
        var originalCell = new LegacyNativeWordTransitionCostCell(originalCostCellIdentity, I32(original, 16));
        var request = new LegacyNativeWordRecombinationRequest(bytes, originalCell);
        W32(bytes, 0, anchorEnding);
        W32(bytes, 8, previousNode);
        W16(bytes, 12, U16(anchor, 28));
        bytes[18] = Geometry(anchor);
        W32(bytes, 20, First(anchor));
        bytes[28] = (byte)(anchor[39] >> 1 & 7);
        if (bytes[28] is not (0 or 2)) throw new NotSupportedException("Plain-symbol anchor kinds0/2 required.");
        W32(bytes, 32, span);
        if (U16(anchor, 24) == anchorEnding)
        {
            W16(bytes, 36, U16(anchor, 36));
            bytes[72] = (byte)(anchor[7] & 1);
        }
        else
        {
            ulong suffix = U64(anchor, 8);
            if (suffix == 0) throw new ArgumentException("Actual anchor suffix symbol context required.");
            ushort symbol = contexts.ReadNode(suffix).Symbol;
            bytes[72] = 0;
            if (symbol == 0xfffb) W16(bytes, 36, symbols.Symbol24e6);
            else if (symbol == 0xfffc) W16(bytes, 36, symbols.Symbol24e8);
            else if (unchecked((ushort)(symbol + 9)) < 3) W16(bytes, 36, symbols.Symbol24e4);
        }
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(40), originalCell.Identity);
        W32(bytes, 52, Impossible);
        bytes[56] = (byte)(anchor[33] & 63);
        bytes[57] = bytes[59] = 0;
        bytes[58] = policy.Permission2309;
        W16(bytes, 60, 0);
        uint currentKind = Header(anchor[4]) >> 9 & 15;
        W32(bytes, 64, unchecked((int)currentKind));
        UpdatePrevious(original);
        W32(bytes, 48, I32(original, 20));
        int originalRawCost = originalCell.Value;
        int lastCalls = 0;
        bool initialAccepted = CalculateAndValidate();
        int costDelta = Impossible, incrementDelta = Impossible, originalGeometryDelta = 0;
        if (initialAccepted)
        {
            costDelta = unchecked(I32(current, 16) - originalCell.Value);
            incrementDelta = unchecked(I32(current, 20) - I32(bytes, 48));
            originalGeometryDelta = unchecked(originalCell.Value - originalRawCost);
        }
        var heap = new LegacyNativeLanguageContextCandidateHeap(candidateCapacity);
        ulong state = U64(original, 40);
        int matched = 0, geometryCalls = lastCalls;
        var candidateCell = new LegacyNativeWordTransitionCostCell(candidateCostCellIdentity, 0);
        for (ushort ordinal = policy.PrimaryLimit230a; ordinal < policy.SecondaryEnd23f0; ordinal++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var row = previous[ordinal];
            if ((row[33] & 0x80) != 0 || U64(row, 40) != state ||
                policy.CountCapEnabled && row[38] + span > policy.CountCap2602 ||
                policy.RequireSecondaryFlag && (row[39] & 1) == 0) continue;
            W16(bytes, 16, U16(row, 28));
            var successors = operations.SuccessorsF0F8(request, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!successors.Span.Contains(U16(anchor, 28))) continue;
            matched++;
            int rawCost = I32(row, 16), rawIncrement = I32(row, 20);
            if (policy.ReuseKind6Geometry && request.ReadUInt32(64) == 6)
            {
                int total = unchecked(costDelta + rawCost);
                if (rawCost < Impossible && total < Impossible)
                    heap.TryInsert(total, unchecked(incrementDelta + rawIncrement), ordinal,
                        originalGeometryDelta, currentOrdinal, cancellationToken);
            }
            else
            {
                request = new LegacyNativeWordRecombinationRequest(bytes, candidateCell);
                UpdatePrevious(row);
                BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(40), candidateCell.Identity);
                candidateCell.Value = rawCost;
                W32(bytes, 48, rawIncrement);
                bool accepted = CalculateAndValidate();
                geometryCalls += lastCalls;
                int total = unchecked(costDelta + candidateCell.Value);
                if (accepted && candidateCell.Value < Impossible && total < Impossible)
                    heap.TryInsert(total, unchecked(incrementDelta + I32(bytes, 48)), ordinal,
                        unchecked(candidateCell.Value - rawCost - originalGeometryDelta), ushort.MaxValue, cancellationToken);
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(initialAccepted, heap.GetSnapshot(), request.GetSnapshot(), matched, geometryCalls);

        byte[] Lookup(int node, ushort ordinal)
        {
            var row = owningRowLookup(node, ordinal);
            cancellationToken.ThrowIfCancellationRequested();
            if (row is null || row.Record.Length != 88) throw new ArgumentException("Referenced node/ordinal must own a complete stored88 row.");
            return row.Record.ToArray();
        }
        uint Header(byte sourceId)
        {
            uint result = sourceHeader(sourceId);
            cancellationToken.ThrowIfCancellationRequested();
            return result;
        }
        void UpdatePrevious(byte[] row)
        {
            W32(bytes, 4, U16(row, 24));
            W16(bytes, 14, (ushort)(BinaryPrimitives.ReadUInt32LittleEndian(row.AsSpan(24)) >> 22));
            W16(bytes, 16, U16(row, 28));
            W16(bytes, 38, U16(row, 36));
            ushort relatedOrdinal = U16(row, 30);
            if (relatedOrdinal >= previous.Length) throw new ArgumentException("Previous related ordinal must own the same previous node.");
            bytes[73] = (byte)(previous[relatedOrdinal][7] & 1);
            W32(bytes, 68, unchecked((int)(Header(row[4]) >> 9 & 15)));
            bytes[19] = Geometry(row);
            W32(bytes, 24, First(row));
        }
        int Category(byte[] row)
        {
            int category = U16(row, 26) & 63;
            if ((uint)category >= (uint)categories.CompositeCategories.Length) throw new ArgumentException("Actual sampler composite category required.");
            return category;
        }
        byte Geometry(byte[] row) => checked((byte)categories.CompositeCategories.Span[Category(row)].GeometryIndex);
        int First(byte[] row) => categories.CompositeCategories.Span[Category(row)].FirstId;
        bool CalculateAndValidate()
        {
            lastCalls = 1;
            bool accepted = operations.Calculate118(request, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!accepted) return false;
            lastCalls++;
            accepted = operations.Validate140(request, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return accepted;
        }
    }

    private static int I32(ReadOnlySpan<byte> source, int offset) => BinaryPrimitives.ReadInt32LittleEndian(source[offset..]);
    private static ushort U16(ReadOnlySpan<byte> source, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(source[offset..]);
    private static ulong U64(ReadOnlySpan<byte> source, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(source[offset..]);
    private static void W16(Span<byte> target, int offset, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(target[offset..], value);
    private static void W32(Span<byte> target, int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(target[offset..], value);
}
