using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if (args.Length is not (2 or 8)) throw new ArgumentException("controls AK [LK engine separation case publication actualCapture]");
using var data = JsonDocument.Parse(File.ReadAllBytes(args[0]));
byte[] ak = File.ReadAllBytes(args[1]);
int streams = 0, callbackCount = 0, guards = 0, merges = 0;
foreach (var test in data.RootElement.GetProperty("cases").EnumerateArray())
{
    var contexts = new LegacySymbolContextFactory();
    ulong[] ids = test.GetProperty("paths").EnumerateArray().Select(path => Path(contexts, Keys(path))).ToArray();
    byte[] heap = RebindHeap(Bytes(test, "slots"));
    byte[] primary = RebindPool(Bytes(test, "primaryFull")), secondary = RebindPool(Bytes(test, "secondaryFull"));
    ushort pc = test.GetProperty("pc").GetUInt16(), sc = test.GetProperty("sc").GetUInt16();
    int active = test.GetProperty("active").GetInt32(); uint node = test.GetProperty("node").GetUInt32();
    int previous = unchecked((int)node - (sbyte)heap[128 + 32] - 1);
    var rows = new List<LegacyNativeStoredWordRecord>();
    var rawRows = test.GetProperty("records88"); var parentPrimary = test.GetProperty("primaryParents"); var parentSecondary = test.GetProperty("pools");
    for (int arc = 0; arc < rawRows.GetArrayLength(); arc++)
    {
        byte[] row = Convert.FromHexString(rawRows[arc].GetString()!);
        if (arc == 0) Write64(row, 8, ids[checked((int)Read64(row, 8))]);
        rows.Add(new(row, RebindPool(Convert.FromHexString(parentPrimary[arc].GetString()!)), RebindPool(Convert.FromHexString(parentSecondary[arc].GetString()!))));
    }
    uint[] headers = test.GetProperty("headers").EnumerateArray().Select(v => v.GetUInt32()).ToArray();
    var config = LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with
    { PrimaryAlternativeCount2402 = pc, SecondaryAlternativeCount2404 = sc, PrimaryArcLimit230a = test.GetProperty("limit").GetUInt16(), Mode22f6 = 0 };
    var profile = LegacyCursiveCategoryProfile.Load(ak.AsSpan(test.GetProperty("profileOffset").GetInt32()), test.GetProperty("profileOffset").GetInt32(), out _);
    var policy = new LegacyNativeHigherArcRecombinationPolicy(65535, test.GetProperty("threshold").GetInt32(),
        test.GetProperty("capEnabled").GetBoolean(), test.GetProperty("cap").GetByte(), test.GetProperty("permission").GetByte());
    var reset = new LegacyNativeHigherArcResetParameters(65535, test.GetProperty("resetSource").GetUInt32());
    ushort[] symbolsArray = Keys(test.GetProperty("symbols")); var symbols = new LegacyNativeWordRecombinationSymbols(symbolsArray[0], symbolsArray[1], symbolsArray[2], symbolsArray[3]);
    int observed = 0, reads = 0; var expectedCallbacks = test.GetProperty("expectedCallbacks");
    JsonElement Observe(int kind, LegacyNativeWordRecombinationRequest request)
    {
        if (observed >= expectedCallbacks.GetArrayLength()) throw new Exception("Extra geometry callback.");
        var expected = expectedCallbacks[observed++];
        if (kind != expected.GetProperty("kind").GetInt32() || !request.GetSnapshot().Span.SequenceEqual(Bytes(expected, "request")) || request.Cost.Value != expected.GetProperty("cost").GetInt32())
            throw new Exception($"Full80/cost callback differs at stream{streams}/callback{observed - 1}.");
        return test.GetProperty("scripts")[request.ReadUInt16(16) - 100];
    }
    var operations = new LegacyNativeHigherArcTransitionOperations((request, _) =>
    { var script = Observe(0, request); request.Cost.Value = unchecked(request.Cost.Value + script.GetProperty("d118").GetInt32()); request.WriteByte(60, (byte)(request.ReadByte(60) ^ 0x11)); return script.GetProperty("a118").GetBoolean(); },
    (request, _) => { var script = Observe(1, request); request.Cost.Value = unchecked(request.Cost.Value + script.GetProperty("d140").GetInt32()); request.WriteByte(61, unchecked((byte)(request.ReadByte(61) + 3))); return script.GetProperty("a140").GetBoolean(); },
    (request, _) => Keys(Observe(2, request).GetProperty("successors")));
    IReadOnlyList<LegacyNativeStoredWordRecord> Rows(int ending)
    { if (ending != previous) throw new Exception("Dispatcher queried a fabricated node."); reads++; return rows; }
    byte[] originalHeap = heap.ToArray(), originalP = primary.ToArray(), originalS = secondary.ToArray();
    var result = LegacyNativePrimaryWordRecombination.Apply(heap, active, node, config, Rows, s => headers[s], profile, contexts,
        s => (ushort)(s ^ 0x55), reset, policy, symbols, operations, test.GetProperty("costCell").GetUInt64(), Bytes(test, "residue"), primary, secondary);
    if (!result.HeapStorage128.Span.SequenceEqual(RebindHeap(Bytes(test, "expectedHeap"))) ||
       !result.PrimaryStorage40.Span.SequenceEqual(RebindPool(Bytes(test, "expectedP"))) ||
       !result.SecondaryStorage40.Span.SequenceEqual(RebindPool(Bytes(test, "expectedS"))))
        throw new Exception($"Complete mixed heap/pools differ at stream{streams}.");
    if (observed != expectedCallbacks.GetArrayLength() || reads != 1 || result.Steps.Count != active) throw new Exception("Callback/snapshot/step lifecycle differs.");
    for (int i = 0; i < active; i++)
    {
        var step = result.Steps[i]; int route = test.GetProperty("routes")[i].GetInt32();
        if ((int)step.Route != route || step.SlotIndex != i || step.PoolOrdinal != test.GetProperty("ordinals")[i].GetInt32() ||
            step.PreviousEndingNode != (i == 0 ? -1 : previous) || step.PreviousRowOrdinal != Read16(heap, i * 128 + 30))
            throw new Exception("Actual128/pool ordinal routing changed.");
        if (route == 3)
        {
            bool accepted = test.GetProperty(i == 1 ? "accepted" : "secondAccepted").GetBoolean();
            int filtered = test.GetProperty(i == 1 ? "filtered" : "secondFiltered").GetInt32(), merged = test.GetProperty(i == 1 ? "merged" : "secondMerged").GetInt32();
            if (step.InitialGeometryAccepted != accepted || step.FilteredCandidates != filtered || step.MergedCandidates != merged) throw new Exception("General branch outcome changed.");
            merges += step.MergedCandidates;
        }
    }
    if (!heap.SequenceEqual(originalHeap) || !primary.SequenceEqual(originalP) || !secondary.SequenceEqual(originalS)) throw new Exception("Dispatcher changed caller storage.");
    callbackCount += observed;
    if (streams == 0)
    {
        //Native entry cannot call or validate unused providers/ordinals in1/1.
        var noOp = LegacyNativePrimaryWordRecombination.Apply(heap, active, node, config with { PrimaryAlternativeCount2402 = 1, SecondaryAlternativeCount2404 = 1, Trace2368 = 1 },
            null, null, null, null, null, default, default, default, null, 0, new byte[80], new byte[40], new byte[40]);
        if (!noOp.HeapStorage128.Span.SequenceEqual(heap) || noOp.Steps.Count != 0) throw new Exception("Native1/1 early return touched row ownership."); guards++;
        var empty = LegacyNativePrimaryWordRecombination.Apply(heap, 0, node, config with { Trace2368 = 1 }, null, null, null, null, null, default, default, default, null, 0, new byte[80], primary, secondary);
        if (!empty.HeapStorage128.Span.SequenceEqual(heap) || empty.Steps.Count != 0) throw new Exception("Empty heap touched callback state."); guards++;
        for (int boundary = 0; boundary < 6; boundary++)
        {
            using var cancel = new CancellationTokenSource();
            bool Stop(int at) { if (boundary == at) cancel.Cancel(); return false; }
            var callbacks = new LegacyNativeHigherArcTransitionOperations((_, _) => { Stop(3); return boundary != 3; }, (_, _) => { Stop(4); return boundary != 4; }, (_, _) => { Stop(5); return new ushort[] { 23 }; });
            try
            {
                LegacyNativePrimaryWordRecombination.Apply(heap, active, node, config,
                    n => { Stop(0); return rows; }, s => { Stop(1); return headers[s]; }, profile, contexts,
                    s => { Stop(2); return (ushort)(s ^ 0x55); }, reset, policy, symbols, callbacks, 1, Bytes(test, "residue"), primary, secondary, cancel.Token);
                throw new Exception($"Cancellation callback{boundary} was ignored.");
            }
            catch (OperationCanceledException) { guards++; }
        }
        Reject(() => LegacyNativePrimaryWordRecombination.Apply(heap, active, node, config, Rows, s => headers[s], profile, contexts, s => s, reset, policy, symbols, operations, 1, new byte[79], primary, secondary));
        byte[] wrong = heap.ToArray(); Write32(wrong, 88, 6);
        Reject(() => LegacyNativePrimaryWordRecombination.Apply(wrong, active, node, config, Rows, s => headers[s], profile, contexts, s => s, reset, policy, symbols, operations, 1, new byte[80], primary, secondary));
        wrong = heap.ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(wrong.AsSpan(128 + 30), 500);
        Reject(() => LegacyNativePrimaryWordRecombination.Apply(wrong, active, node, config, Rows, s => headers[s], profile, contexts, s => s, reset, policy, symbols, operations, 1, new byte[80], primary, secondary));
        Reject(() => LegacyNativePrimaryWordRecombination.Apply(heap, active, node, config, null, null, null, null, null, default, default, default, null, 0, new byte[80], primary, secondary, new(true)));
    }
    streams++;
    byte[] RebindHeap(byte[] bytes)
    { for (int i = 0; i < test.GetProperty("active").GetInt32(); i++) Write64(bytes, i * 128 + 8, ids[checked((int)Read64(bytes, i * 128 + 8))]); return bytes; }
    byte[] RebindPool(byte[] bytes)
    { for (int i = 0; i < bytes.Length / 40; i++) for (int k = 0; k < 2; k++) Write64(bytes, i * 40 + 8 + k * 8, ids[checked((int)Read64(bytes, i * 40 + 8 + k * 8))]); return bytes; }
}
Console.WriteLine($"PASS {streams} independently generated five-slot streams/{streams * 5} exact routes/{callbackCount} full80 callbacks/{merges} general merges/{guards} guards; higher branches explicitly prepared.");
void Reject(Action action) { try { action(); } catch (Exception e) when (e is ArgumentException or NotSupportedException or OperationCanceledException) { guards++; return; } throw new Exception("Invalid owning input was accepted."); }
static byte[] Bytes(JsonElement e, string name) => Convert.FromHexString(e.GetProperty(name).GetString()!);
static ushort[] Keys(JsonElement e) => e.EnumerateArray().Select(v => v.GetUInt16()).ToArray();
static ulong Path(LegacySymbolContextFactory factory, ushort[] path)
{
    ulong id = 0; foreach (ushort symbol in path) { var next = id == 0 ? factory.ResolveInitial(symbol) : factory.ResolveChild(id, symbol); if (next.Created && id != 0) factory.LinkCreatedContext(next.Identity, id); id = next.Identity; }
    return id;
}
static ulong Read64(ReadOnlySpan<byte> b, int o) => BinaryPrimitives.ReadUInt64LittleEndian(b[o..]);
static ushort Read16(ReadOnlySpan<byte> b, int o) => BinaryPrimitives.ReadUInt16LittleEndian(b[o..]);
static void Write64(Span<byte> b, int o, ulong v) => BinaryPrimitives.WriteUInt64LittleEndian(b[o..], v);
static void Write32(Span<byte> b, int o, int v) => BinaryPrimitives.WriteInt32LittleEndian(b[o..], v);

if (args.Length == 8) ActualCasePrefix.Run(args);
