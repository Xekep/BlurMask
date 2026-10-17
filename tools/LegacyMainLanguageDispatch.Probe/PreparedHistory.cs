using System.Buffers.Binary;
using InkRecognition;

internal static class PreparedHistory
{
    internal static int Verify(byte[] ak, byte[] lk, byte[] engine, LegacyRussianModelBundle model,
        LegacyCursiveSeparationTable table, LegacyNativeCaseData cases, LegacyNativeTextPublicationData publication)
    {
        var sampled = MainFixture.Create(ak, engine, table);
        var dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, sampled, cases, publication);
        var owner = new LegacyOwnedMainWordSpanPreparation(ak, dictionary, sampled, table);
        var controller = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner);
        var policy = LegacyFreshCaseMainSourcePolicy.Prepare(ak, lk, controller);
        uint[] headers = owner.CopySourceHeaders();
        var initial = new LegacyNativeMainInitialSpecialCandidates(ak, owner, new(1, 0), new byte[80]);
        byte[]? query = null, parent = null, terminalRow = null;
        int ending = -1;

        //Only this startup/first-language payload is inferred. The later anchor,
        //row placements and two-element heap below are explicitly PREPARED.
        for (int node = 1; node < owner.SourceGraph.NodeCount; node++)
        {
            if (owner.GetSpanExtent(node) <= node) continue;
            owner.AdvanceNodeEpoch(node - 1);
            var live = owner.Prepare(node, checked((byte)node));
            var input = new LegacyNativeWordRecordHeap(256);
            var sourceQuery = new LegacyPreparedDictionaryQuery
            {
                Capacity = 512, ContextRoot = owner.Contexts.ContainerIdentity,
                Parameter1c = (uint)node, Parameter20 = (uint)node
            };
            while (true)
            {
                var page = owner.Read(live, 8UL << 40, sourceQuery);
                if (page.ReturnedCount == 0) break;
                if (page.ReturnedCount < 0) throw new Exception("Actual case startup query failed.");
                initial.ProcessPreparedPage(live, page.Records.Span, 0, Router(input), 0, false, false);
                sourceQuery.Skip += page.ReturnedCount;
            }
            var rows = Enumerable.Range(0, input.Count).Select(index =>
                LegacyNativeWordRowMaterializer.Materialize(input.GetSlotSnapshot(index).Span,
                    (ushort)index, 1, 1, 1).Record.ToArray()).ToArray();
            for (ushort ordinal = 0; ordinal < rows.Length; ordinal++)
            {
                var injected = LegacyNativeFirstDictionaryWordCompletion.CreateAlreadyTerminalQuery(rows[ordinal], new byte[32]);
                if (!injected.HasValue) continue;
                var heap = new LegacyNativeWordRecordHeap(256);
                var result = LegacyNativeFirstDictionaryWordCompletion.ForFreshCaseSpan(policy, live).ProcessCandidate(
                    injected.Value.Span, node, ordinal, rows[ordinal], Router(heap), false, new byte[40],
                    (n, a) => n == node && a < rows.Length ? rows[a] : throw new Exception("Foreign first anchor."), false);
                if (!result.LanguageAccepted || !result.Allocation.Accepted) throw new Exception("Actual first completion failed.");
                query = injected.Value.ToArray(); parent = rows[ordinal];
                terminalRow = heap.GetSlotSnapshot(0).Span[..88].ToArray(); ending = node;
            }
        }
        if (query is null || parent is null || terminalRow is null) throw new Exception("Actual first-language payload missing.");
        owner.AdvanceNodeEpoch(ending - 1);
        var span = owner.Prepare(ending, checked((byte)ending));
        if (span.Span == 0) throw new Exception("Prepared anchor span must differ from the actual live D0 span.");

        byte[] current = parent.ToArray(), prior = parent.ToArray(), secondary = terminalRow.ToArray();
        current[6] &= 254; current[7] &= 254; current[32] = 0;
        W32(current, 56, ending); W16(current, 60, 0); W16(current, 30, 0); W16(current, 24, ending);
        ushort arc = checked((ushort)owner.SourceGraph.GetNode(ending).ToList().FindIndex(item => item.Span == 0));
        W16(current, 28, arc);
        prior[4] = secondary[4] = 1; prior[33] &= 127; secondary[33] &= 127;
        BinaryPrimitives.WriteUInt64LittleEndian(prior.AsSpan(40), BinaryPrimitives.ReadUInt64LittleEndian(secondary.AsSpan(40)));
        byte[] request = new byte[80];
        W32(request, 0, ending); W32(request, 8, ending - 1); W32(request, 32, 0); W16(request, 12, arc);
        byte[] contexts = new byte[32];
        for (int index = 0; index < 2; index++)
        {
            W32(contexts, index * 16, index == 0 ? 200000 : 100000);
            W32(contexts, index * 16 + 4, 321); W32(contexts, index * 16 + 8, -47);
            W16(contexts, index * 16 + 12, controller.Configuration.PrimaryArcLimit230a);
            W16(contexts, index * 16 + 14, ushort.MaxValue);
        }
        var packet = new LegacyNativeLanguageContextRecombinationResult(true, contexts, request, 2, 4);
        IReadOnlyList<LegacyNativeStoredWordRecord> Rows(int node)
        {
            if (node == ending) return [Stored(current)];
            if (node != ending - 1) throw new Exception("Foreign prepared previous node.");
            var rows = Enumerable.Range(0, controller.Configuration.PrimaryArcLimit230a).Select(_ => Stored(prior)).ToList();
            rows.Add(Stored(secondary));
            return rows;
        }
        LegacyNativeLaterDictionaryWordCompletion Completion() => LegacyNativeLaterDictionaryWordCompletion.ForFreshCaseSpan(policy, span);
        byte[] residue = Enumerable.Range(0, 40).Select(index => (byte)(index * 37 + 19)).ToArray();
        byte[] storage = Enumerable.Range(0, 256 * 128).Select(index => (byte)(index * 17 + 23)).ToArray();
        var expectedHeap = new LegacyNativeWordRecordHeap(256, storage.ToArray());
        var expectedRouter = Router(expectedHeap);
        var baseline = Completion();
        var expected = new List<LegacyNativeLaterDictionaryWordCompletionResult>();
        for (int index = 1; index >= 0; index--)
            expected.Add(baseline.ProcessActualQuery(query, packet, index, ending, 0, owner.SourceGraph.NodeCount,
                Rows, id => headers[id], sampled.GetSourceLineIdForArc, owner.Contexts, expectedRouter,
                false, -1, false, residue, false));
        var actualHeap = new LegacyNativeWordRecordHeap(256, storage.ToArray());
        var producer = Completion();
        int leaseChecks = 0;
        var captured = producer.CaptureActualTerminal(query, ending, owner.Contexts, false, -1, false, false,
            token =>
            {
                owner.ValidateSpan(span, token);
                if (++leaseChecks == 3)
                {
                    Array.Fill(contexts, (byte)255);
                    Array.Fill(request, (byte)255);
                }
            }, default);
        var actual = producer.ProcessActualQueryWithPreparedTerminal(captured, packet, 0, owner.SourceGraph.NodeCount,
            Rows, id => headers[id], sampled.GetSourceLineIdForArc, Router(actualHeap), residue, default);
        if (actual.Count != 2 || actualHeap.Count != expectedHeap.Count ||
            !actualHeap.GetSlotStorageSnapshot().Span.SequenceEqual(expectedHeap.GetSlotStorageSnapshot().Span))
            throw new Exception("Prepared reverse-heap dispatch changed complete active/inactive128 storage.");
        for (int index = 0; index < 2; index++)
        {
            if (!actual[index].Allocation.Accepted || !actual[index].Language.Record40.Span.SequenceEqual(expected[index].Language.Record40.Span) ||
                actual[index].PreLexicalCost != expected[index].PreLexicalCost || actual[index].RestartTag28 != expected[index].RestartTag28 ||
                actual[index].ContextualLineage5c != expected[index].ContextualLineage5c)
                throw new Exception("Prepared captured terminal differs from the existing later consumer.");
        }
        return actual.Count;
    }

    private static LegacyNativeWordHeapRouter Router(LegacyNativeWordRecordHeap heap) =>
        new([heap, new(256)], new byte[] { 0, 0 }, -1, uint.MaxValue);
    private static LegacyNativeStoredWordRecord Stored(byte[] row)
    {
        byte[] slot = new byte[128]; row.CopyTo(slot, 0);
        return LegacyNativeWordRowMaterializer.Materialize(slot, 0, 0, 1, 1);
    }
    private static void W16(byte[] bytes, int offset, int value) => BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset), (ushort)value);
    private static void W32(byte[] bytes, int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(offset), value);
}
