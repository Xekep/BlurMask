using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if (args.Length != 3) throw new ArgumentException("controls AK capture");
byte[] ak = File.ReadAllBytes(args[1]);
var categories = LegacyCursiveCategoryProfile.Load(ak.AsSpan(188222), 188222, out _);
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
const ulong Cell = 0x1122334455667788;
const ushort Soft = 183, Dash = 185;
int packets = 0, streams = 0, traceCount = 0, allocations = 0, guards = 0;
foreach (var item in document.RootElement.GetProperty("packets").EnumerateArray())
{
    byte[] parent = Hex(item, "parent"), residue = Hex(item, "residue");
    var cell = new LegacyNativeWordTransitionCostCell(Cell, 73);
    var request = LegacyNativeWordSoftHyphenRequestFactory.Create(item.GetProperty("current").GetInt32(),
        item.GetProperty("span").GetByte(), item.GetProperty("previous").GetInt32(), parent,
        item.GetProperty("header").GetUInt32(), Soft, Dash, categories, cell, residue);
    Require(request.GetSnapshot().Span.SequenceEqual(Hex(item, "expected")) && cell.Value == 73,
        "Independent pre-F0 full80 packet/caller cost differs.");
    Require(parent.AsSpan().SequenceEqual(Hex(item, "parent")) && residue.AsSpan().SequenceEqual(Hex(item, "residue")),
        "Factory changed its input memory.");
    packets++;
}
foreach (var item in document.RootElement.GetProperty("streams").EnumerateArray())
{
    var contexts = new LegacySymbolContextFactory();
    var cell = new LegacyNativeWordTransitionCostCell(Cell, 73);
    var frame = new LegacyNativeMainWordRequestFrame(cell, Hex(item, "residue"));
    var heap = new LegacyNativeWordRecordHeap(item.GetProperty("capacity").GetInt32(), Hex(item, "initial"));
    var router = new LegacyNativeWordHeapRouter([heap], new byte[] { 0, 0 }, -1, uint.MaxValue);
    var traces = new List<(string Stage, byte[] Bytes, int Value)>();
    ushort[] arcs = item.GetProperty("arcs").EnumerateArray().Select(v => v.GetUInt16()).ToArray();
    byte[] parent = Hex(item, "parent");
    int[] classes = Enumerable.Repeat(0x4000000, 195).ToArray();
    if (item.GetProperty("qualifies").GetBoolean()) classes[Soft] = 0;
    var qualifier = new LegacyNativeWordSymbolQualifier(192, [new(classes, [])]);
    bool applied = item.GetProperty("applied").GetBoolean();
    var operations = new LegacyNativeHigherArcTransitionOperations(
        (request, _) =>
        {
            Log("118", request); int arc = request.ReadUInt16(12);
            request.Cost.Value += (arc + 1) * 7;
            BinaryPrimitives.WriteInt32LittleEndian(request.Storage[48..], request.ReadInt32(48) + (arc + 1) * 3);
            request.WriteByte(59, (byte)(request.ReadByte(59) ^ 0x5a));
            return arc % 4 != 0;
        },
        (request, _) =>
        {
            Log("140", request); request.Cost.Value += 11;
            request.WriteByte(60, unchecked((byte)(request.ReadByte(60) + 1)));
            return request.ReadUInt16(12) % 4 != 1;
        },
        (request, _) =>
        {
            Log("F0", request); request.WriteByte(57, unchecked((byte)(request.ReadByte(57) + 3)));
            return arcs;
        });
    var produced = LegacyNativeSoftHyphenWordPrepass.Run(item.GetProperty("current").GetInt32(),
        item.GetProperty("span").GetByte(), item.GetProperty("previous").GetInt32(), 17, parent,
        new uint[] { 0, item.GetProperty("header").GetUInt32() }, Soft, Dash, categories, contexts,
        applied ? qualifier : null, applied ? operations : null, applied ? Score : null,
        frame, router, Root, item.GetProperty("special").GetBoolean(), item.GetProperty("marker").GetBoolean(),
        false, false, default);
    Require(produced.Applied == applied && produced.Qualified == item.GetProperty("qualified").GetBoolean() &&
        produced.AcceptedAllocations == item.GetProperty("accepted").GetInt32() &&
        produced.GeometryCalls == item.GetProperty("geometry").GetInt32() &&
        produced.AcousticCalls == item.GetProperty("acoustic").GetInt32() &&
        heap.Count == item.GetProperty("count").GetInt32() && cell.Value == item.GetProperty("cost").GetInt32(),
        "Independent callback/prepass/allocator result differs.");
    Require(frame.CopyBytes().AsSpan().SequenceEqual(Hex(item, "frame")), "Caller frame dropped prior callback writes.");
    var expectedTraces = item.GetProperty("traces").EnumerateArray().ToArray();
    Require(traces.Count == expectedTraces.Length, "Native callback count/order differs.");
    for (int index = 0; index < traces.Count; index++)
        Require(traces[index].Stage == expectedTraces[index][0].GetString() &&
            traces[index].Bytes.AsSpan().SequenceEqual(Convert.FromHexString(expectedTraces[index][1].GetString()!)) &&
            traces[index].Value == expectedTraces[index][2].GetInt32(), "Independent full80 callback snapshot/cost differs.");
    byte[] actual = heap.GetSlotStorageSnapshot().ToArray();
    for (int index = 0; index < heap.Count; index++)
    {
        ulong id = BinaryPrimitives.ReadUInt64LittleEndian(actual.AsSpan(index * 128 + 8));
        Require(id == produced.ContextIdentity, "Allocated word08 does not name the same0918D0 context.");
        actual.AsSpan(index * 128 + 8, 8).Clear();
    }
    Require(actual.AsSpan().SequenceEqual(Hex(item, "expected")), "Independent whole active/inactive128 heap storage differs.");
    if (produced.Qualified)
    {
        var symbol = contexts.ReadNode(produced.ContextIdentity);
        Require(contexts.NodeCount == 1 && symbol.ParentLink == 0 && symbol.Symbol ==
            (item.GetProperty("marker").GetBoolean() ? Dash : Soft), "Wrong initial context key/ownership.");
    }
    else Require(contexts.NodeCount == 0 && produced.ContextIdentity == 0, "Native early return inserted a context.");
    streams++; traceCount += traces.Count; allocations += produced.AcceptedAllocations;

    void Log(string stage, LegacyNativeWordRecombinationRequest request) => traces.Add((stage, request.GetSnapshot().ToArray(), request.Cost.Value));
    bool Score(LegacyNativeWordRecombinationRequest request, CancellationToken _)
    {
        Log("128", request); request.Cost.Value += 13;
        BinaryPrimitives.WriteInt32LittleEndian(request.Storage[48..], request.ReadInt32(48) - 5);
        request.WriteByte(61, (byte)(request.ReadByte(61) ^ 0xa5));
        return request.ReadUInt16(12) % 4 != 2;
    }
}

//Early no-op preserves frame/cell even with inactive callbacks and trace mode.
foreach (uint header in new uint[] { 0x20a, 0x40a, 0xa0a, 0x120a, 0x140a })
{
    var frame = new LegacyNativeMainWordRequestFrame(new(Cell, 73), Enumerable.Repeat((byte)0xc7, 80).ToArray());
    byte[] parent = new byte[88]; parent[4] = 1;
    var noOp = LegacyNativeSoftHyphenWordPrepass.Run(1, 0, 0, 0, parent, new uint[] { 0, header },
        ushort.MaxValue, ushort.MaxValue, categories, new(), null, null, null, frame,
        new([new LegacyNativeWordRecordHeap(1)], new byte[] { 0, 0 }, -1, uint.MaxValue), Root, false, false, true, false, default);
    Require(!noOp.Applied && frame.CopyBytes().All(b => b == 0xc7) && frame.Cost.Value == 73, "Inactive native branch was validated/mutated.");
    guards++;
}

using (var cancel = new CancellationTokenSource())
{
    byte[] parent = new byte[88]; parent[4] = 1;
    var frame = new LegacyNativeMainWordRequestFrame(new(Cell, 73), new byte[80]);
    int[] finite = new int[195];
    var operations = new LegacyNativeHigherArcTransitionOperations((_, _) => true, (_, _) => true,
        (request, _) => { request.WriteByte(57, 0x5a); cancel.Cancel(); return new ushort[] { 0 }; });
    Reject(() => LegacyNativeSoftHyphenWordPrepass.Run(1, 0, 0, 0, parent, new uint[] { 0, 0xc0a },
        Soft, Dash, categories, new(), new(192, [new(finite, [])]), operations, (_, _) => true, frame,
        new([new LegacyNativeWordRecordHeap(1)], new byte[] { 0, 0 }, -1, uint.MaxValue), Root,
        false, false, false, false, cancel.Token));
    Require(frame.CopyBytes()[57] == 0x5a, "Cancellation discarded native callback prefix writes.");
}

foreach (string stage in new[] { "118", "140", "128" })
foreach (bool returned in new[] { false, true })
{
    using var cancel = new CancellationTokenSource();
    byte[] parent = new byte[88]; parent[4] = 1;
    var frame = new LegacyNativeMainWordRequestFrame(new(Cell, 73), new byte[80]);
    var heap = new LegacyNativeWordRecordHeap(1);
    var operations = new LegacyNativeHigherArcTransitionOperations(
        (request, _) => Callback("118", request), (request, _) => Callback("140", request),
        (_, _) => new ushort[] { 0 });
    Reject(() => LegacyNativeSoftHyphenWordPrepass.Run(1,0,0,0,parent,new uint[] {0,0xc0a},
        Soft,Dash,categories,new(),new(192,[new(new int[195],[])]),operations,
        (request, _) => Callback("128",request),frame,
        new([heap],new byte[] {0,0},-1,uint.MaxValue),Root,false,false,false,false,cancel.Token));
    Require(frame.CopyBytes()[57] == 0xa5 && heap.Count == 0,
        "Canceled true/false callback lost prefix writes or reached allocation.");
    bool Callback(string actual, LegacyNativeWordRecombinationRequest request)
    {
        if (actual != stage) return true;
        request.WriteByte(57,0xa5); cancel.Cancel(); return returned;
    }
}

foreach (bool qualified in new[] { false, true })
{
    byte[] parent = new byte[88]; parent[4]=1;
    var frame = new LegacyNativeMainWordRequestFrame(new(Cell,73),new byte[80]);
    var contexts = new LegacySymbolContextFactory();
    var costs = Enumerable.Repeat(0x4000000,195).ToArray(); if (qualified) costs[Soft]=0;
    LegacyNativeSoftHyphenWordPrepassResult Run() => LegacyNativeSoftHyphenWordPrepass.Run(
        1,0,0,0,parent,new uint[] {0,0xc0a},Soft,Dash,categories,contexts,new(192,[new(costs,[])]),
        null,null,frame,new([new LegacyNativeWordRecordHeap(1)],new byte[] {0,0},-1,uint.MaxValue),
        Root,false,false,true,false,default);
    if (qualified) Reject(() => Run());
    else { Require(!Run().Qualified,"Unqualified trace branch executed its body."); guards++; }
    Require(contexts.NodeCount==0 && frame.CopyBytes().All(b=>b==0) && frame.Cost.Value==73,
        "Trace prerequisite failure/no-op changed caller state.");
}

using (var file = File.Create(args[2]))
using (var json = new Utf8JsonWriter(file))
{
    json.WriteStartObject(); json.WriteNumber("packets", packets); json.WriteNumber("streams", streams);
    json.WriteNumber("full80Callbacks", traceCount); json.WriteNumber("acceptedAllocations", allocations);
    json.WriteNumber("guards", guards); json.WriteString("scope", "independent PREPARED213C00 controls, not inferred handwriting");
    json.WriteEndObject();
}
Console.WriteLine($"PASS {packets} independent full80 packets/{streams} prepared streams/{traceCount} callback snapshots/{allocations} allocations/{guards} guards; no inferred candidate claim.");

static byte[] Hex(JsonElement row, string name) => Convert.FromHexString(row.GetProperty(name).GetString()!);
static ReadOnlyMemory<byte> Root(int node, ushort ordinal)
    => node == 0 && ordinal == 0 ? new byte[88] : throw new ArgumentException("Exact prepared root-row owner required.");
static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new Exception("Invalid soft-hyphen scope accepted.");
}
