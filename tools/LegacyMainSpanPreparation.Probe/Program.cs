using System.Buffers.Binary;
using InkRecognition;

if (args.Length != 4) throw new ArgumentException("AK LK engine separation required");
byte[] ak = File.ReadAllBytes(args[0]);
byte[] lk = File.ReadAllBytes(args[1]);
byte[] engine = File.ReadAllBytes(args[2]);
byte[] tableBytes = File.ReadAllBytes(args[3]);
var fixture = MainFixture.Create(ak, lk, engine, tableBytes);
var owner = Create();
var graph = fixture.Sampled.SourceRanges.Graph;
var model = owner.DictionaryOwner.Model;
ushort[] reserved = LegacyPreparedAcousticDictionaryPermissions.ReadReservedSlots(model.AcousticAlphabet, ak.AsSpan(99, 16));
ushort[] originalMap = owner.DictionaryOwner.Symbols.CopyPrimary();
var pool = new List<(int Group, byte Scope)>();
int steps = 0, selectedRows = 0, classValues = 0, rootRecords = 0, initialRecords = 0;
var rootSymbols = new HashSet<ushort>();
LegacyOwnedMainWordSpan? firstSpan = null;
foreach (int node in Enumerable.Range(0, graph.NodeCount))
{
    var arcs = graph.GetNode(node);
    int expectedExtent = arcs.Count == 0 ? 0 : arcs.Select(a => (int)a.Span).Max() + 1;
    Require(owner.GetSpanExtent(node) == expectedExtent, "Native arc span extent differs.");
    for (byte span = 0; span < expectedExtent; span++)
    {
        var prepared = owner.Prepare(node, span);
        firstSpan ??= prepared;
        Require(prepared.PreviousEndingNode == node - span - 1 && prepared.IsInitial == (span == node),
            "Ordered span changed original previous-source ownership.");
        var positions = Enumerable.Range(0, arcs.Count).Where(i => arcs[i].Span == span).ToArray();
        var present = arcs.Select(a => a.GroupId).ToHashSet();
        for (int p = 0; p < pool.Count; p++)
            if (pool[p].Scope == span && !present.Contains(pool[p].Group)) pool[p] = (-1, span);
        int computed = 0;
        byte[] expectedPermissions = new byte[192];
        byte[] expectedEligibility = new byte[195];
        foreach (ushort symbol in reserved)
            if (symbol != ushort.MaxValue) expectedPermissions[symbol] = 3;
        foreach (int ordinal in positions)
        {
            int group = arcs[ordinal].GroupId;
            int existing = pool.FindIndex(p => p.Group == group);
            if (existing < 0)
            {
                computed++;
                int vacancy = pool.FindIndex(p => p.Group == -1);
                if (vacancy < 0) pool.Add((group, span));
                else pool[vacancy] = (group, span);
            }
            var independent = fixture.Acoustic.Score(group).Cache.Costs.Span;
            var callback = owner.GetCosts(prepared, node, (ushort)ordinal);
            Require(callback.ClassCosts.Span.SequenceEqual(independent), "Owned selected cache differs from actual NN195.");
            for (int symbol = 0; symbol < 195; symbol++)
            {
                if (independent[symbol] < 0x4000000) expectedEligibility[symbol] = 1;
                if (symbol < 192)
                {
                    if (independent[symbol] < 0x4000000) expectedPermissions[symbol] |= 3;
                    if (independent[symbol] < 40000) expectedPermissions[symbol] |= 4;
                }
                classValues++;
            }
        }
        var pass = prepared.PermissionPass;
        Require(pass.ComputedRows == computed && pass.ReusedRows == positions.Length - computed,
            "Original pool invalidation/reuse order differs.");
        Require(pass.SelectedPositions.Select(p => p.ArcIndex).SequenceEqual(positions) &&
            pass.PermissionResult.Permissions.Span.SequenceEqual(expectedPermissions) &&
            prepared.Qualifier.GetEligibilitySnapshot().Span.SequenceEqual(expectedEligibility),
            "Span selection, source permissions or finite union differs.");
        ushort[] expectedMap = originalMap.Select(symbol => symbol < 192 && (expectedPermissions[symbol] & 1) == 0
            ? ushort.MaxValue : symbol).ToArray();
        Require(pass.PermissionResult.WorkingMap!.CopySelectedMap(false, 0).AsSpan().SequenceEqual(expectedMap),
            "Actual source68 did not restore58/filter50.");
        if (prepared.IsInitial)
        {
            //Full unfiltered root page proves source queries consume the freshly
            //prepared working map. Special startup consumers remain separate.
            var query = new LegacyPreparedDictionaryQuery { Node = 0, Capacity = 512, ContextRoot = owner.Contexts.ContainerIdentity };
            var page = owner.Read(prepared, 8UL << 40, query);
            var range = model.Lexicon.GetChildRange(0);
            var expected = Enumerable.Range(range.Start + 1, range.End - range.Start - 1)
                .Where(child => expectedMap[model.Lexicon.GetLabel(child)] != ushort.MaxValue).ToArray();
            Require(page.ReturnedCount == expected.Length && page.Records.Length == expected.Length * 32,
                "Full root source page count differs from permission-filtered edges.");
            for (int i = 0; i < expected.Length; i++)
            {
                var row = page.Records.Span.Slice(i * 32, 32);
                ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(row[20..]);
                rootSymbols.Add(symbol);
                ulong context = BinaryPrimitives.ReadUInt64LittleEndian(row[8..]);
                Require(BinaryPrimitives.ReadInt32LittleEndian(row) == expected[i] && row[4] == 1 &&
                    symbol == expectedMap[model.Lexicon.GetLabel(expected[i])] && owner.Contexts.ReadNode(context).Symbol == symbol,
                    "Full root query node/source/label/context ownership differs.");
                rootRecords++;
            }
            Require(owner.Read(prepared, 8UL << 40, new LegacyPreparedDictionaryQuery
            { Node = 0, Skip = expected.Length, Capacity = 512, ContextRoot = owner.Contexts.ContainerIdentity }).ReturnedCount == 0,
                "Root query pagination failed to reach its actual source end.");
            var thresholds = LegacyAcousticCandidateThresholds.ReadV110Node(ak.AsSpan(189734), out _);
            var startup = new LegacyNativeOrdinaryWordStartup(graph, model.AcousticAlphabet,
                fixture.Sampled.Owner.TransitionProfile.Categories, owner.Contexts,
                LegacyNativeWordAcousticRequestScorer.ForAlphabet(thresholds, model.AcousticAlphabet),
                new(0x8877665544332211, 0), new byte[80]);
            var heap = new LegacyNativeWordRecordHeap(owner.SourceRowBudget);
            var router = new LegacyNativeWordHeapRouter([heap, new(0)], new byte[] { 0, 0 }, 0, uint.MaxValue);
            uint[] headers = owner.DictionaryOwner.CopySourceHeaders();
            var consumed = startup.ProcessPage(page.Records.Span, checked((ushort)node), span, 0,
                prepared.Qualifier, new Dictionary<byte, uint> { [0] = headers[0], [1] = headers[1] },
                (n, arc) => owner.GetCosts(prepared, n, arc), router, 0,
                false, 0, false, false, null);
            var map = new LegacyCursiveCategoryMapper(fixture.Sampled.Owner.TransitionProfile.Categories);
            var expectedCandidates = new List<(int Cost, ushort Symbol, ushort Arc, int Category)>();
            for (int i = 0; i < page.ReturnedCount; i++)
            {
                var row = page.Records.Span.Slice(i * 32, 32);
                ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(row[20..]);
                foreach (bool end in new[] { false, true })
                {
                    int category = map.GetSymbolCompositeCategory(symbol, end);
                    if (category < 0) continue;
                    foreach (int ordinal in positions)
                    {
                        var costs = owner.GetCosts(prepared, node, (ushort)ordinal);
                        int value = unchecked(BinaryPrimitives.ReadInt32LittleEndian(row[16..]) +
                            costs.ClassCosts.Span[symbol] + costs.GeometryCosts.Span[
                                fixture.Sampled.Owner.TransitionProfile.Categories.CompositeCategories.Span[category].GeometryIndex]);
                        if (value < 0x4000000) expectedCandidates.Add((value, symbol, (ushort)ordinal, category));
                    }
                }
            }
            Require(consumed.Succeeded && consumed.RowsVisited == expected.Length &&
                consumed.AcceptedAllocations == expectedCandidates.Count && heap.Count == expectedCandidates.Count,
                "Full permission-refreshed initial page did not produce its exact native candidate extent.");
            for (int i = 0; i < heap.Count; i++)
            {
                var record = heap.GetSlotSnapshot(i).Span;
                var key = (BinaryPrimitives.ReadInt32LittleEndian(record[16..]),
                    BinaryPrimitives.ReadUInt16LittleEndian(record[36..]),
                    BinaryPrimitives.ReadUInt16LittleEndian(record[28..]), (int)(record[26] & 63));
                int matching = expectedCandidates.IndexOf(key);
                Require(matching >= 0, "Unfiltered actual NN/dictionary initial record changed its cost/label/arc/category.");
                expectedCandidates.RemoveAt(matching);
                ulong context = BinaryPrimitives.ReadUInt64LittleEndian(record[8..]);
                var nodeContext = owner.Contexts.ReadNode(context);
                Require(record[4] == 1 && nodeContext.Symbol == key.Item2 &&
                    owner.Contexts.ReadNode(nodeContext.ParentLink).Symbol == 0xfffa &&
                    BinaryPrimitives.ReadUInt16LittleEndian(record[24..]) == node &&
                    BinaryPrimitives.ReadUInt16LittleEndian(record[30..]) == ushort.MaxValue,
                    "Unfiltered native initial record lost source/FFFA context/first-node/root ownership.");
                initialRecords++;
            }
            Require(expectedCandidates.Count == 0, "Unfiltered actual initial candidates were dropped.");
        }
        selectedRows += positions.Length;
        steps++;
    }
}
Require(owner.SourceRowBudget == 125 && selectedRows > 0 && steps > 0 && rootRecords > 0,
    "Actual source budget was confused with active rows/spans.");
var repeated = owner.Prepare(graph.NodeCount - 1, 0);
Require(repeated.PermissionPass.ComputedRows == 0 && repeated.PermissionPass.ReusedRows > 0 &&
    repeated.PermissionPass.SelectedRows.All(row => !row.Recomputed),
    "Repeated D0 false did not preserve actual native pool reuse.");
Console.WriteLine($"PASS{steps} ordered main D0 spans, {selectedRows} actual cache rows/{classValues} class values, {rootRecords} full unfiltered dictionary records -> {initialRecords} real NN/native initial128 candidates; source budget125 explicit.");
Console.WriteLine("Actual refreshed root labels: " + string.Join(" ", rootSymbols.Order()
    .Select(symbol => model.AcousticAlphabet.TryGetText(symbol, out string text) ? text : $"label{symbol}")));

int guards = 0;
Reject(() => owner.GetCosts(firstSpan!, firstSpan!.EndingNode, 0)); guards++;
foreach (bool cancel in new[] { false, true })
{
    var target = Create();
    Reject(() => target.Prepare(cancel ? 0 : graph.NodeCount, 0,
        cancel ? new CancellationToken(true) : default)); guards++;
    Reject(() => target.Prepare(0, 0)); guards++;
}
var wrong = Create();
var correct = Create();
var foreignSpan = correct.Prepare(0, 0);
Reject(() => wrong.GetCosts(foreignSpan, 0, 0)); guards++;
var bounded = Create();
var boundedSpan = bounded.Prepare(0, 0);
Reject(() => bounded.GetCosts(boundedSpan, 0, ushort.MaxValue)); guards++;
Require(rootRecords != owner.SourceRowBudget, "Actual query count accidentally equals the unrelated row budget.");
Console.WriteLine($"PASS{guards} stale/foreign/out-of-domain/cancellation/failed-session guards.");

LegacyOwnedMainWordSpanPreparation Create() => new(ak, lk, engine, fixture.Sampled, fixture.Table);
static void Require(bool condition, string message) { if (!condition) throw new InvalidDataException(message); }
static void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or OperationCanceledException) { return; }
    throw new InvalidDataException("Expected owned span refusal.");
}
