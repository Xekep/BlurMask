using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainPrimaryRecombination.Probe")]

namespace InkRecognition;

public enum LegacyNativePrimaryWordRecombinationRoute
{
    Root,
    PrimaryParent,
    SecondaryParentReset,
    SecondaryParentGeometry
}

public readonly record struct LegacyNativePrimaryWordRecombinationStep(int SlotIndex, int PoolOrdinal,
    int PreviousEndingNode, ushort PreviousRowOrdinal, LegacyNativePrimaryWordRecombinationRoute Route,
    bool InitialGeometryAccepted, int FilteredCandidates, int MergedCandidates);

public sealed record LegacyNativePrimaryWordRecombinationResult(ReadOnlyMemory<byte> HeapStorage128,
    ReadOnlyMemory<byte> PrimaryStorage40, ReadOnlyMemory<byte> SecondaryStorage40,
    IReadOnlyList<LegacyNativePrimaryWordRecombinationStep> Steps);

/// <summary>Nontracing20E6A0 branch dispatcher in actual heap-slot order. Full
/// owning source rows and configured alternative pools are explicit inputs;
/// this does not schedule2144F0, invent secondary predecessors or pick winners.</summary>
public static class LegacyNativePrimaryWordRecombination
{
    public static LegacyNativePrimaryWordRecombinationResult Apply(ReadOnlySpan<byte> fullHeapStorage128,
        int activeCount, uint currentNode, LegacyNativeWordControllerConfiguration configuration,
        Func<int, IReadOnlyList<LegacyNativeStoredWordRecord>>? owningNodeRowsLookup,
        Func<byte, uint>? sourceHeaderLookup, LegacyCursiveCategoryProfile? categoryProfile,
        LegacySymbolContextFactory? contexts, Func<ushort, ushort>? mapSymbol2360,
        LegacyNativeHigherArcResetParameters resetParameters,
        LegacyNativeHigherArcRecombinationPolicy recombinationPolicy,
        LegacyNativeWordRecombinationSymbols symbols, LegacyNativeHigherArcTransitionOperations? operations,
        ulong costCellIdentity, ReadOnlySpan<byte> requestResidue80,
        ReadOnlySpan<byte> primaryStorage40, ReadOnlySpan<byte> secondaryStorage40,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ushort pc = configuration.PrimaryAlternativeCount2402, sc = configuration.SecondaryAlternativeCount2404;
        //Portable byte-shape bounds are deliberately distinct from the native
        //entry guard. No source callbacks/ordinals/context values are touched
        //when both configured counts are one (or there are no active rows).
        if (pc is < 1 or > 16 || sc is < 1 or > 16 || fullHeapStorage128.Length % 128 != 0 ||
            fullHeapStorage128.Length / 128 > 4096 || (uint)activeCount > (uint)(fullHeapStorage128.Length / 128) ||
            primaryStorage40.Length % (pc * 40) != 0 || secondaryStorage40.Length % (sc * 40) != 0 ||
            requestResidue80.Length != 80)
            throw new ArgumentException("Complete bounded heap/request/pool storage and checked configured counts required.");
        byte[] heap = fullHeapStorage128.ToArray(), primary = primaryStorage40.ToArray(), secondary = secondaryStorage40.ToArray();
        var steps = new List<LegacyNativePrimaryWordRecombinationStep>();
        if (pc == 1 && sc == 1 || activeCount == 0)
            return new(heap, primary, secondary, steps.AsReadOnly());
        if (configuration.Trace2368 != 0) throw new NotSupportedException("Native640-byte trace payloads remain a separate owner.");
        ArgumentNullException.ThrowIfNull(owningNodeRowsLookup); ArgumentNullException.ThrowIfNull(sourceHeaderLookup);
        ArgumentNullException.ThrowIfNull(categoryProfile); ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(mapSymbol2360);
        if (resetParameters.SpecialLabel24ea != recombinationPolicy.ResetSpecialLabel24ea)
            throw new ArgumentException("One actual controller24EA value must select both secondary-parent branches.");
        byte[] requestResidue = requestResidue80.ToArray();
        var snapshots = new Dictionary<int, LegacyNativeStoredWordRecord[]>();
        for (int index = 0; index < activeCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var slot = heap.AsSpan(index * 128, 128);
            uint previous = unchecked(currentNode - (uint)(int)(sbyte)slot[32] - 1);
            ushort parentOrdinal = U16(slot, 30); int ordinal = I32(slot, 88);
            if (ordinal < 0 || pc > 1 && ordinal >= primary.Length / (pc * 40) ||
                sc > 1 && ordinal >= secondary.Length / (sc * 40))
                throw new ArgumentException("Actual ordinal58 must address every enabled full alternative pool.");
            LegacyNativePrimaryWordRecombinationRoute route;
            bool accepted = true; int filtered = 0, merged = 0;
            if (previous == uint.MaxValue)
            {
                route = LegacyNativePrimaryWordRecombinationRoute.Root;
                var result = LegacyNativeRootWordAlternativeProducer.Create(slot, 1, currentNode,
                    configuration, contexts, mapSymbol2360, primary, secondary, cancellationToken);
                primary = result.PrimaryStorage.ToArray(); secondary = result.SecondaryStorage.ToArray();
            }
            else
            {
                if (previous > int.MaxValue) throw new NotSupportedException("Actual nonnegative owned predecessor node or root -1 required.");
                var rows = Rows((int)previous);
                if (parentOrdinal >= rows.Count) throw new ArgumentException("Actual predecessor ordinal lies outside source23F0 rows.");
                if (parentOrdinal < configuration.PrimaryArcLimit230a)
                {
                    route = LegacyNativePrimaryWordRecombinationRoute.PrimaryParent;
                    var result = LegacyNativeInitialParentWordAlternativeProducer.Create(slot, 1, currentNode,
                        configuration, Lookup, contexts, mapSymbol2360, primary, secondary, cancellationToken);
                    primary = result.PrimaryStorage.ToArray(); secondary = result.SecondaryStorage.ToArray();
                }
                else
                {
                    uint header = Header(slot[4]);
                    if (LegacyNativeHigherArcResetAlternativeProducer.IsResetSource(configuration.Mode22f6,
                        header, U16(slot, 36), resetParameters.SpecialLabel24ea))
                    {
                        route = LegacyNativePrimaryWordRecombinationRoute.SecondaryParentReset;
                        var result = LegacyNativeHigherArcResetAlternativeProducer.Create(slot, 1, currentNode,
                            configuration, resetParameters, Header, Lookup, contexts, mapSymbol2360,
                            primary, secondary, cancellationToken);
                        primary = result.PrimaryStorage.ToArray(); secondary = result.SecondaryStorage.ToArray();
                    }
                    else
                    {
                        route = LegacyNativePrimaryWordRecombinationRoute.SecondaryParentGeometry;
                        if (sc < 2) throw new NotSupportedException("General higher geometry currently requires its actual configured secondary block.");
                        ArgumentNullException.ThrowIfNull(operations);
                        var result = LegacyNativeHigherArcRecombination.Create(slot, currentNode,
                            configuration, rows, Lookup, Header, categoryProfile, contexts, mapSymbol2360,
                            recombinationPolicy, symbols, operations, costCellIdentity, requestResidue,
                            primary.AsSpan(pc > 1 ? ordinal * pc * 40 : 0, pc > 1 ? pc * 40 : 0),
                            secondary.AsSpan(ordinal * sc * 40, sc * 40), cancellationToken);
                        //Keep every unrelated/inactive block and all128 bytes
                        //of this exact slot, including its real ordinal58.
                        result.HeapSlot.Span.CopyTo(slot);
                        if (pc > 1) result.PrimaryStorage.Span.CopyTo(primary.AsSpan(ordinal * pc * 40, pc * 40));
                        result.SecondaryStorage.Span.CopyTo(secondary.AsSpan(ordinal * sc * 40, sc * 40));
                        accepted = result.InitialAccepted; filtered = result.FilteredCandidates; merged = result.MergedCandidates;
                        //The same native local request scratch survives later
                        //general-branch rows, rather than fresh zero padding.
                        requestResidue = result.LastRequest.ToArray();
                    }
                }
            }
            steps.Add(new(index, ordinal, unchecked((int)previous), parentOrdinal, route, accepted, filtered, merged));
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(heap, primary, secondary, steps.AsReadOnly());

        IReadOnlyList<LegacyNativeStoredWordRecord> Rows(int node)
        {
            if (!snapshots.TryGetValue(node, out var rows))
            {
                var actual = owningNodeRowsLookup(node); cancellationToken.ThrowIfCancellationRequested();
                if (actual is null || actual.Count > ushort.MaxValue)
                    throw new ArgumentException("An actual source23F0 row collection is required for this computed node.");
                rows = new LegacyNativeStoredWordRecord[actual.Count];
                for (int i = 0; i < rows.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested(); var row = actual[i];
                    if (row is null || row.Record.Length != 88)
                        throw new ArgumentException("Each source ordinal must own a complete stored88 row.");
                    rows[i] = new(row.Record.ToArray(), row.PrimaryAlternatives.ToArray(), row.SecondaryAlternatives.ToArray());
                }
                snapshots.Add(node, rows);
            }
            return rows;
        }
        LegacyNativeStoredWordRecord Lookup(int node, ushort ordinal)
        {
            var rows = Rows(node);
            if (ordinal >= rows.Count) throw new ArgumentException("Referenced predecessor ordinal lies outside its computed owning node.");
            return rows[ordinal];
        }
        uint Header(byte source)
        { uint value = sourceHeaderLookup(source); cancellationToken.ThrowIfCancellationRequested(); return value; }
    }
    private static int I32(ReadOnlySpan<byte> row, int at) => BinaryPrimitives.ReadInt32LittleEndian(row[at..]);
    private static ushort U16(ReadOnlySpan<byte> row, int at) => BinaryPrimitives.ReadUInt16LittleEndian(row[at..]);
}
