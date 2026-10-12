using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 1) throw new ArgumentException("controls.json required");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0, requests = 0, failures = 0, releases = 0;
byte[] Bytes(JsonElement item) => Convert.FromBase64String(item.GetProperty("backing").GetString()!);
void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
void Reject(Action action)
{
    try { action(); } catch (Exception e) when (e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException) { return; }
    throw new InvalidOperationException("Invalid/cancelled reset was accepted.");
}
foreach (var test in controls.RootElement.GetProperty("cases").EnumerateArray())
{
    var descriptors = test.GetProperty("before").EnumerateArray().Select(item =>
    {
        var limits = item.GetProperty("limits");
        return new LegacyNativeWordHeapDescriptor(Bytes(item), item.GetProperty("cap").GetInt32(),
            item.GetProperty("count").GetInt32(), item.GetProperty("tag").GetUInt64(),
            new(limits[0].GetUInt32(), limits[1].GetUInt32(), limits[2].GetUInt32(),
                limits[3].GetUInt32(), limits[4].GetUInt32(), limits[5].GetUInt32(), limits[6].GetInt32()));
    }).ToArray();
    var traceVectors = test.GetProperty("traces").EnumerateArray().Select(item =>
        new LegacyNativeWordTraceVector(Bytes(item), item.GetProperty("count").GetInt32(),
            item.GetProperty("tag").GetUInt64(), item.GetProperty("used").GetUInt32())).ToArray();
    var grow = test.GetProperty("growth"); int call = 0;
    var released = new List<(int Vector, int Entry, ulong Tag)>();
    var config = LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with { Trace2368 = test.GetProperty("trace").GetBoolean() ? (byte)7 : (byte)0 };
    bool result = LegacyNativeWordPassReset.Reset(descriptors, config, test.GetProperty("phase").GetInt32(),
        (index, request) =>
        {
            Require(call < grow.GetArrayLength(), "Unexpected allocation.");
            var expected = grow[call++]; var before = test.GetProperty("before")[index];
            Require(index == expected.GetProperty("index").GetInt32() && request.RequiredAllocatedSlots == expected.GetProperty("capacity").GetInt32() &&
                request.PreviousStorageTag10 == before.GetProperty("tag").GetUInt64() && request.PreviousStorage.Span.SequenceEqual(Bytes(before)), "Allocator order or retained storage differs.");
            Require(descriptors[index].Count == 0 && descriptors[index].Capacity == request.RequiredAllocatedSlots, "Native descriptor stores did not precede allocation.");
            return expected.GetProperty("failed").GetBoolean() ? null : new LegacyNativeWordHeapGrowthResult(Bytes(expected), expected.GetProperty("tag").GetUInt64());
        }, traceVectors, (vector, entry, tag) => released.Add((vector, entry, tag)));
    Require(result == test.GetProperty("succeeded").GetBoolean() && call == grow.GetArrayLength(), "Reset failure stop or allocation count differs.");
    var expectedDescriptors = test.GetProperty("after");
    for (int index = 0; index < descriptors.Length; index++)
    {
        var actual = descriptors[index]; var expected = expectedDescriptors[index];
        Require(actual.Count == expected.GetProperty("count").GetInt32() && actual.Capacity == expected.GetProperty("cap").GetInt32() &&
            actual.StorageTag10 == expected.GetProperty("tag").GetUInt64() && actual.GetBackingStorageSnapshot().Span.SequenceEqual(Bytes(expected)), "Reset descriptor image differs, including inactive/opaque bytes.");
    }
    for (int index = 0; index < traceVectors.Length; index++)
    {
        var actual = traceVectors[index]; var expected = test.GetProperty("afterTraces")[index];
        Require(actual.Count == expected.GetProperty("count").GetInt32() && actual.StorageIdentity == expected.GetProperty("tag").GetUInt64() &&
            actual.UsedCount == expected.GetProperty("used").GetUInt32() && actual.CopyHeaderStorage().Span.SequenceEqual(Bytes(expected)) && actual.Capacity == Bytes(expected).Length / 16,
            "Trace header/count/allocation state differs.");
    }
    Require(released.SequenceEqual(test.GetProperty("released").EnumerateArray().Select(item => (item[0].GetInt32(), item[1].GetInt32(), item[2].GetUInt64()))), "Trace payload/vector release ordering differs.");
    requests += call; if (!result) failures++; releases += released.Count; cases++;
}

var fresh = LegacyNativeWordControllerConfiguration.CreateFresh20baa0();
var descriptor = new LegacyNativeWordHeapDescriptor(new byte[128], 1, 0, 9, new(0, 0, 3, 0, 0, 2, -1));
Reject(() => LegacyNativeWordPassReset.Reset([descriptor, descriptor], fresh, 0));
Reject(() => LegacyNativeWordPassReset.Reset([], fresh with { Trace2368 = 1 }, 0));
Reject(() => LegacyNativeWordPassReset.Reset([], fresh, 0, cancellationToken: new CancellationToken(true)));
Reject(() => new LegacyNativeWordTraceVector(new byte[17], 0, 0, 0));
foreach (bool allocationFailure in new[] { false, true })
{
    using var cancellation = new CancellationTokenSource();
    var heap = new LegacyNativeWordHeapDescriptor(new byte[128], 1, 0, 9, descriptor.Limits);
    Reject(() => LegacyNativeWordPassReset.Reset([heap], fresh, 0, (index, request) =>
    {
        cancellation.Cancel();
        return allocationFailure ? null : new LegacyNativeWordHeapGrowthResult(new byte[request.RequiredAllocatedSlots * 128], 999);
    }, cancellationToken: cancellation.Token));
    Require(heap.Count == 0 && heap.Capacity == 3 && heap.AllocatedCapacity == 1 && heap.StorageTag10 == 9, "Cancelled allocation installed bytes or rolled back prewritten fields.");
}
var small = new LegacyNativeWordTraceVector(new byte[16], 1, 3, 2);
Reject(() => LegacyNativeWordPassReset.Reset([], fresh with { Trace2368 = 1 }, 0, traceVectors: [small, small]));
var large = new LegacyNativeWordTraceVector(new byte[160], 10, 4, 9);
Reject(() => LegacyNativeWordPassReset.Reset([], fresh with { Trace2368 = 1 }, 0, traceVectors: [small, large]));
Require(small.UsedCount == 2 && large.UsedCount == 9, "Invalid trace ownership mutated state.");
using (var cancellation = new CancellationTokenSource())
{
    int calls = 0;
    Reject(() => LegacyNativeWordPassReset.Reset([], fresh with { Trace2368 = 1 }, 0, traceVectors: [large, small],
        traceRelease: (vector, entry, tag) => { calls++; cancellation.Cancel(); }, cancellationToken: cancellation.Token));
    Require(calls == 1 && large.Count == 10 && large.UsedCount == 9 && small.UsedCount == 2, "Cancelled trace release continued or pretended a completed free.");
}
// A second pass must retain and reuse the allocation, even though every active
// result is cleared. New accepted records overwrite only their own slot.
var prior = new byte[512]; BinaryPrimitives.WriteInt32LittleEndian(prior.AsSpan(16), 123);
prior.AsSpan(120, 8).Fill(0xbc);
var reusable = new LegacyNativeWordHeapDescriptor(prior, 4, 1, 77, new(0, 0, 2, 0, 0, 3, -1));
Require(LegacyNativeWordPassReset.Reset([reusable], fresh, 1, (_, _) => throw new Exception("Reuse allocated")) && reusable.Count == 0 && reusable.Capacity == 3 && reusable.AllocatedCapacity == 4 && reusable.StorageTag10 == 77 && reusable.GetBackingStorageSnapshot().Span.SequenceEqual(prior), "Pass reset discarded reusable allocation or stale storage.");
var acceptedRecord = new byte[128]; BinaryPrimitives.WriteInt32LittleEndian(acceptedRecord.AsSpan(16), 5); acceptedRecord.AsSpan(120).Fill(0xfe);
Require(reusable.TryInsertRecord(acceptedRecord) && reusable.Count == 1 && reusable.GetHeapSnapshot().GetSlotSnapshot(0).Span.SequenceEqual(acceptedRecord), "Reset pass did not accept a fresh record.");
Require(LegacyNativeWordPassReset.Reset([reusable], fresh, 0) && reusable.Count == 0 && reusable.Capacity == 2 && reusable.AllocatedCapacity == 4, "Repeated phase reset retained an old active result.");
Console.WriteLine($"PASS {cases} complete pass resets / {requests} allocation requests / {failures} failures / {releases} trace releases; full storage, phase quotas,9/10 boundary, reuse, ownership and cancellation.");
