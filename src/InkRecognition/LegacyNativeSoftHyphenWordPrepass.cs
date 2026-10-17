using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainSoftHyphen.Probe")]

namespace InkRecognition;

internal sealed record LegacyNativeSoftHyphenWordPrepassResult(bool Applied, bool Qualified,
    int Successors, int GeometryCalls, int AcousticCalls, int AcceptedAllocations,
    ulong ContextIdentity, ReadOnlyMemory<byte> LastRequest80);

//Literal nontracing213C00 flow over explicit typed callbacks/owners. The
//owning scheduler adapter supplies its actual D0 qualifier, same request frame,
//geometry/cache128 and router. Prepared controls are not inferred handwriting.
internal static class LegacyNativeSoftHyphenWordPrepass
{
    internal static LegacyNativeSoftHyphenWordPrepassResult Run(int currentNode, byte span,
        int previousNode, ushort previousOrdinal, ReadOnlySpan<byte> parent88,
        ReadOnlySpan<uint> sourceHeaders, ushort soft24c2, ushort dash24c0,
        LegacyCursiveCategoryProfile categories, LegacySymbolContextFactory contexts,
        LegacyNativeWordSymbolQualifier? qualifier, LegacyNativeHigherArcTransitionOperations? operations,
        Func<LegacyNativeWordRecombinationRequest, CancellationToken, bool>? score128,
        LegacyNativeMainWordRequestFrame frame, LegacyNativeWordHeapRouter router,
        Func<int, ushort, ReadOnlyMemory<byte>> owningRows, bool kindA00ExplicitlyEligible,
        bool knowledgeMarkerA0, bool trace2368, bool suppressPrevious2609,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(categories); ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(frame); ArgumentNullException.ThrowIfNull(router);
        ArgumentNullException.ThrowIfNull(owningRows); token.ThrowIfCancellationRequested();
        if (parent88.Length != 88 || currentNode < 0 || previousNode < 0 || span > 14 ||
            currentNode - span - 1 != previousNode || sourceHeaders.Length is < 1 or > 256)
            throw new ArgumentException("Actual bounded adjacent word node/span, full parent and source table required.");
        byte[] parent = parent88.ToArray();
        byte sourceId = parent[4];
        if (sourceId >= sourceHeaders.Length) throw new ArgumentException("Parent source must own its source header.");
        uint header = sourceHeaders[sourceId], kind = header & 0x1e00;
        bool eligible = kind is 0x800 or 0xc00 or 0xe00 or 0x1000 || kind == 0xa00 && kindA00ExplicitlyEligible;
        //Native source-kind no-op precedes frontend110 and callback validation.
        if (!eligible) return new(false, false, 0, 0, 0, 0, 0, ReadOnlyMemory<byte>.Empty);
        ArgumentNullException.ThrowIfNull(qualifier);
        if (!qualifier.Qualifies(soft24c2, 0, 0, contexts, token))
            return new(true, false, 0, 0, 0, 0, 0, ReadOnlyMemory<byte>.Empty);
        if (trace2368) throw new NotSupportedException("Native640-byte trace allocation remains a separate owner.");
        ArgumentNullException.ThrowIfNull(operations); ArgumentNullException.ThrowIfNull(operations.SuccessorsF0F8);
        ArgumentNullException.ThrowIfNull(operations.Calculate118); ArgumentNullException.ThrowIfNull(operations.Validate140);
        ArgumentNullException.ThrowIfNull(score128);
        var request = LegacyNativeWordSoftHyphenRequestFactory.Create(currentNode, span, previousNode,
            parent, header, soft24c2, dash24c0, categories, frame.Cost, frame.CopyBytes(), token);
        int geometryCalls = 0, acousticCalls = 0, allocations = 0;
        ulong context = 0; ushort[] arcs = [];
        try
        {
            //0918D0 lookup/insertion precedes F0 even when no arcs are returned.
            context = contexts.ResolveInitial(knowledgeMarkerA0 ? dash24c0 : soft24c2, token).Identity;
            arcs = operations.SuccessorsF0F8(request, token).ToArray();
            token.ThrowIfCancellationRequested();
            if (arcs.Length > ushort.MaxValue) throw new NotSupportedException("Native bounded ushort successor count required.");
            foreach (ushort arc in arcs)
            {
                token.ThrowIfCancellationRequested();
                BinaryPrimitives.WriteUInt16LittleEndian(request.Storage[12..], arc);
                frame.Cost.Value = I32(parent, 16);
                BinaryPrimitives.WriteInt32LittleEndian(request.Storage[48..], I32(parent, 20));
                geometryCalls++;
                bool accepted = operations.Calculate118(request, token);
                token.ThrowIfCancellationRequested();
                if (accepted)
                {
                    accepted = operations.Validate140(request, token);
                    token.ThrowIfCancellationRequested();
                }
                if (accepted)
                {
                    acousticCalls++;
                    accepted = score128(request, token);
                    token.ThrowIfCancellationRequested();
                }
                if (!accepted) continue;
                //213C00 qualifies/request36 uses soft24C2, but initializes
                //record24 and its map28 category with ASCII dash24C0.
                var fields = new LegacyInitialWordRecordFields(U64(parent, 0), context,
                    frame.Cost.Value, request.ReadInt32(48), dash24c0,
                    checked((ushort)request.ReadInt32(0)), request.ReadUInt16(12), request.ReadUInt16(12),
                    unchecked((byte)categories.StartGeometryMap.Span[dash24c0]), previousOrdinal,
                    unchecked((sbyte)request.ReadInt32(32)), parent[38], (byte)(parent[5] >> 7), 1, 0,
                    request.ReadByte(28), request.ReadByte(56), 0, 0,
                    I32(parent, 48), I32(parent, 52), I32(parent, 56), U16(parent, 60));
                var allocation = router.TryInsertInitialRecord(sourceId, false, fields, 0,
                    suppressPrevious2609, Lookup, token);
                token.ThrowIfCancellationRequested();
                //1F8D60 reports success on ordinary heap rejection. Continue
                //later successors without invalidating a completed allocator.
                if (allocation.Accepted) allocations++;
            }
            token.ThrowIfCancellationRequested();
            return new(true, true, arcs.Length, geometryCalls, acousticCalls, allocations,
                context, request.GetSnapshot());
        }
        finally { frame.Commit(request); }

        ReadOnlyMemory<byte> Lookup(int node, ushort ordinal)
        {
            if (node == previousNode && ordinal == previousOrdinal) return parent;
            var value = owningRows(node, ordinal);
            token.ThrowIfCancellationRequested();
            if (value.Length != 88) throw new ArgumentException("Referenced word history requires its actual88-byte row.");
            return value;
        }
    }

    private static ushort U16(ReadOnlySpan<byte> b, int p) => BinaryPrimitives.ReadUInt16LittleEndian(b[p..]);
    private static int I32(ReadOnlySpan<byte> b, int p) => BinaryPrimitives.ReadInt32LittleEndian(b[p..]);
    private static ulong U64(ReadOnlySpan<byte> b, int p) => BinaryPrimitives.ReadUInt64LittleEndian(b[p..]);
}
