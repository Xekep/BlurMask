using System.Buffers.Binary;
using InkRecognition;

//Procedural213C00 transcription. Never invokes the new prepass or packet
//factory. Native geometry, context AVL and1F8D60 have separate scalar/byte
//oracles; this checks their actual ordering and complete heap transport.
internal static class OriginalSoftHyphenOracle
{
    internal static LegacyNativeSoftHyphenWordPrepassResult Run(
        LegacyFreshMainGlobalWordControllerPolicy policy,
        LegacyOwnedMainWordSourceServices.SpanLease span, ushort parentOrdinal,
        ReadOnlyMemory<byte> parentMemory, Func<int, ushort, ReadOnlyMemory<byte>> read,
        LegacyNativeMainWordRequestFrame frame, LegacyNativeWordHeapRouter router,
        LegacyNativeWordAcousticRequestScorer acoustic)
    {
        var source = policy.Services;
        var alphabet = source.Model.AcousticAlphabet;
        if (!alphabet.TryGetLabel("\u00ad", out ushort soft) || !alphabet.TryGetLabel("-", out ushort dash))
            throw new Exception("Actual main soft/dash labels absent.");
        byte[] parent = parentMemory.ToArray();
        uint header = source.CopySourceHeaders()[parent[4]];
        if ((header & 0x1e00) != 0xc00) throw new Exception("This actual oracle owns ordinary C00 only.");
        if (!span.Qualifier.Qualifies(soft, 0, 0, source.Contexts))
            return new(true, false, 0, 0, 0, 0, 0, ReadOnlyMemory<byte>.Empty);
        byte[] packet = frame.CopyBytes();
        I(packet, 0, span.EndingNode); I(packet, 4, U(parent, 24)); I(packet, 8, span.PreviousEndingNode);
        U(packet, 14, (ushort)(BinaryPrimitives.ReadUInt32LittleEndian(parent.AsSpan(24)) >> 22));
        U(packet, 16, U(parent, 28));
        var map = source.Categories.Categories;
        packet[18] = unchecked((byte)map.StartGeometryMap.Span[dash]);
        packet[19] = (byte)map.CompositeCategories.Span[parent[26] & 63].GeometryIndex;
        I(packet, 20, 3); I(packet, 24, 0); packet[28] = 0;
        I(packet, 32, span.Span); U(packet, 36, soft); U(packet, 38, U(parent, 36));
        BinaryPrimitives.WriteUInt64LittleEndian(packet.AsSpan(40), frame.Cost.Identity);
        packet[56] = packet[72] = 0; packet[73] = (byte)(parent[7] & 1);
        I(packet, 64, (int)(header >> 9 & 15)); I(packet, 68, (int)(header >> 9 & 15));
        var request = new LegacyNativeWordRecombinationRequest(packet, frame.Cost);
        ulong context = source.Contexts.ResolveInitial(policy.Refresh.KnowledgeMarkerA0 != 0 ? dash : soft).Identity;
        var callbacks = source.GlobalTransitions!.CreateOperations();
        var arcs = callbacks.SuccessorsF0F8(request, default).ToArray();
        int geometry = 0, scores = 0, accepted = 0;
        foreach (ushort arc in arcs)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(request.Storage[12..], arc);
            request.Cost.Value = I(parent, 16);
            BinaryPrimitives.WriteInt32LittleEndian(request.Storage[48..], I(parent, 20));
            geometry++;
            if (!callbacks.Calculate118(request, default)) continue;
            if (!callbacks.Validate140(request, default)) continue;
            scores++;
            if (!acoustic.Score(request, (node, ordinal) => source.GetCosts(span, node, ordinal, default), false)) continue;
            var fields = new LegacyInitialWordRecordFields(BinaryPrimitives.ReadUInt64LittleEndian(parent),
                context, request.Cost.Value, request.ReadInt32(48), dash, (ushort)request.ReadInt32(0),
                request.ReadUInt16(12), request.ReadUInt16(12), unchecked((byte)map.StartGeometryMap.Span[dash]),
                parentOrdinal, unchecked((sbyte)request.ReadInt32(32)), parent[38], (byte)(parent[5] >> 7), 1,
                0, request.ReadByte(28), request.ReadByte(56), 0, 0,
                I(parent, 48), I(parent, 52), I(parent, 56), U(parent, 60));
            if (router.TryInsertInitialRecord(parent[4], false, fields, 0, false, read).Accepted) accepted++;
        }
        frame.Commit(request);
        return new(true, true, arcs.Length, geometry, scores, accepted, context, request.GetSnapshot());
    }

    private static ushort U(ReadOnlySpan<byte> b, int p) => BinaryPrimitives.ReadUInt16LittleEndian(b[p..]);
    private static int I(ReadOnlySpan<byte> b, int p) => BinaryPrimitives.ReadInt32LittleEndian(b[p..]);
    private static void U(Span<byte> b, int p, ushort v) => BinaryPrimitives.WriteUInt16LittleEndian(b[p..], v);
    private static void I(Span<byte> b, int p, int v) => BinaryPrimitives.WriteInt32LittleEndian(b[p..], v);
}
