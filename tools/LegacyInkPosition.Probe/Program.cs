using System.Text.Json;
using InkRecognition;

if (args.Length != 1) throw new ArgumentException("Supply independent position controls JSON.");
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
var root = document.RootElement;
if (root.GetProperty("format").GetString() != "original-native-ink-position-controls-v1")
    throw new InvalidDataException("Unexpected controls format.");
int packs = 0, canonical = 0, snapshots = 0, selectedCases = 0, rangeCount = 0;
foreach (var control in root.GetProperty("codec").EnumerateArray())
{
    var actual = LegacyNativeInkPosition.Pack(control[0].GetUInt32(), control[1].GetSingle(),
        control[2].GetByte(), control[3].GetInt32());
    Equal(actual.Value, control[4].GetUInt64(), "position packing"); packs++;
}

foreach (var control in root.GetProperty("selected").EnumerateArray())
{
    var originals = ReadRanges(control.GetProperty("originals"));
    var inputs = control.GetProperty("objects").EnumerateArray().Select(value =>
        new LegacyNativeSourceInkObject(value[0].GetInt32(), value[1].GetBoolean())).ToArray();
    var descriptors = LegacyNativeSelectedInkProducer.Select(originals, inputs);
    Equal(descriptors.Length, control.GetProperty("descriptors").GetArrayLength(), "selected count");
    for (int n = 0; n < descriptors.Length; n++)
    {
        var actual = descriptors[n]; var expected = control.GetProperty("descriptors")[n];
        Equal(actual.SourceIndex, expected[0].GetUInt32(), "selected source index");
        Equal(actual.PointCount, expected[1].GetInt32(), "selected points");
        Equal(actual.FirstPoint, expected[2].GetInt32(), "selected first point");
        Equal(actual.LastPoint, expected[3].GetInt32(), "selected last point");
        Equal(BitConverter.SingleToInt32Bits(actual.FirstFraction), BitConverter.SingleToInt32Bits(expected[4].GetSingle()), "selected first fraction");
        Equal(BitConverter.SingleToInt32Bits(actual.LastFraction), BitConverter.SingleToInt32Bits(expected[5].GetSingle()), "selected last fraction");
        Equal(actual.FirstClosed, expected[6].GetBoolean(), "selected first closed");
        Equal(actual.LastClosed, expected[7].GetBoolean(), "selected last closed");
        Equal(actual.IsKnownText, expected[8].GetBoolean(), "selected known text");
    }
    var snapshot = LegacyNativeSelectedInkProducer.CreateSnapshot(originals, inputs);
    int index = 0;
    foreach (var value in control.GetProperty("positions").EnumerateArray())
        Equal(snapshot.Positions[index++].Value, value.GetUInt64(), "composed position");
    Equal(snapshot.PositionCount, index, "composed count"); index = 0;
    foreach (var value in control.GetProperty("separators").EnumerateArray())
        Equal(snapshot.Separators[index++], value.GetInt32(), "composed separator");
    foreach (var query in control.GetProperty("queries").EnumerateArray())
    { CheckRanges(snapshot.ReadRange(query[0].GetInt32(), query[1].GetInt32()), query[2]); rangeCount++; }
    selectedCases++;
}
foreach (var control in root.GetProperty("canonical").EnumerateArray())
{
    var actual = LegacyNativeInkPosition.Canonicalize(new(control[0].GetUInt64()), new(control[1].GetUInt64()));
    Equal(actual.First.Value, control[2].GetUInt64(), "canonical first");
    Equal(actual.Last.Value, control[3].GetUInt64(), "canonical last"); canonical++;
}
foreach (var control in root.GetProperty("snapshots").EnumerateArray())
{
    var selected = control.GetProperty("selections").EnumerateArray().Select(value =>
        new LegacyNativeSelectedInkRange(value[0].GetUInt32(), value[1].GetInt32(), value[2].GetInt32(),
            value[3].GetInt32(), value[4].GetSingle(), value[5].GetSingle(), value[6].GetBoolean(),
            value[7].GetBoolean(), value[8].GetBoolean())).ToArray();
    var originals = ReadRanges(control.GetProperty("originals"));
    var snapshot = LegacyNativeInkPositionSnapshot.Create(selected, originals);
    int index = 0;
    foreach (var value in control.GetProperty("positions").EnumerateArray())
        Equal(snapshot.Positions[index++].Value, value.GetUInt64(), "snapshot position");
    Equal(snapshot.PositionCount, index, "snapshot position count");
    index = 0;
    foreach (var value in control.GetProperty("separators").EnumerateArray())
        Equal(snapshot.Separators[index++], value.GetInt32(), "snapshot separator");
    Equal(snapshot.SelectionCount, index, "snapshot selection count");
    foreach (var query in control.GetProperty("queries").EnumerateArray())
    {
        CheckRanges(snapshot.ReadRange(query[0].GetInt32(), query[1].GetInt32()), query[2]); rangeCount++;
    }
    var endpoints = control.GetProperty("endpoints").EnumerateArray()
        .Select(value => new LegacyNativeDecodedItemEndpoint(value[0].GetInt32(), value[1].GetInt32())).ToArray();
    var itemRanges = snapshot.ReadDecodedItemRanges(2, endpoints);
    CheckRanges(itemRanges[0], control.GetProperty("items")[0]);
    CheckRanges(itemRanges[1], control.GetProperty("items")[1]);
    // Caller arrays and result arrays are owned: mutation cannot change this native owner snapshot.
    if (selected.Length != 0) selected[0] = default;
    if (originals.Length != 0)
    {
        originals[0] = default;
        var returned = snapshot.ReadRange(0, snapshot.PositionCount);
        CheckRanges(returned, control.GetProperty("originals"));
        returned[0] = default;
        CheckRanges(snapshot.ReadRange(0, snapshot.PositionCount), control.GetProperty("originals"));
    }
    snapshots++;
}

var ordinary = new LegacyNativeSelectedInkRange(4, 4, 0, -1, 0, 0, true, true);
var ownSnapshot = LegacyNativeInkPositionSnapshot.Create([ordinary], []);
Reject(() => LegacyNativeInkPosition.Pack(0, float.NaN, 3, 2));
Reject(() => LegacyNativeInkPosition.Pack(0, 0, 0, 2));
Reject(() => LegacyNativeInkPosition.Pack(0, 2, 3, 2));
Reject(() => LegacyNativeInkPosition.Pack(0, 0, 3, 65537));
Reject(() => LegacyNativeInkPositionSnapshot.Create([ordinary with { PointCount = 0 }], []));
Reject(() => LegacyNativeInkPositionSnapshot.Create([ordinary with { FirstPoint = 3, LastClosed = false }], []));
Reject(() => ownSnapshot.ReadRange(-1, 0));
Reject(() => ownSnapshot.ReadRange(1, 0));
Reject(() => ownSnapshot.ReadRange(0, 5));
Reject(() => ownSnapshot.ReadDecodedItemRanges(1, [new(1, 0)]));
Reject(() => ownSnapshot.ReadDecodedItemRanges(1, [new(0, 3), new(0, 1)]));
var ignoredNegative = ownSnapshot.ReadDecodedItemRanges(1, [new(-99, 1), new(0, 3)]);
var expectedAfterGap = ownSnapshot.ReadRange(1, 3);
if (!ignoredNegative[0].AsSpan().SequenceEqual(expectedAfterGap)) throw new Exception("Negative index did not consume endpoint.");
using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
RejectCanceled(() => LegacyNativeInkPositionSnapshot.Create([ordinary], [], cancellation.Token));
RejectCanceled(() => ownSnapshot.ReadRange(0, 0, cancellation.Token));
RejectCanceled(() => ownSnapshot.ReadDecodedItemRanges(1, [], cancellation.Token));
RejectCanceled(() => LegacyNativeSelectedInkProducer.Select([], [], cancellation.Token));
Reject(() => LegacyNativeSelectedInkProducer.Select([new(new(1UL << 32), new(1UL << 32))], [new(3)]));
Reject(() => LegacyNativeSelectedInkProducer.Select([new(new(1UL << 32), new(0))], [new(3), new(3)]));

Console.WriteLine($"PASS {packs} pack / {canonical} canonical / {snapshots} snapshot / {selectedCases} source-selection / {rangeCount} interval controls; decoded endpoints, ownership, guards and cancellation");

static LegacyNativeInkPositionRange[] ReadRanges(JsonElement values) => values.EnumerateArray()
    .Select(value => new LegacyNativeInkPositionRange(new(value[0].GetUInt64()), new(value[1].GetUInt64()))).ToArray();
static void CheckRanges(LegacyNativeInkPositionRange[] actual, JsonElement expected)
{
    Equal(actual.Length, expected.GetArrayLength(), "range count");
    for (int index = 0; index < actual.Length; index++)
    {
        Equal(actual[index].First.Value, expected[index][0].GetUInt64(), "range first");
        Equal(actual[index].Last.Value, expected[index][1].GetUInt64(), "range last");
    }
}
static void Equal<T>(T actual, T expected, string stage) where T : IEquatable<T>
{ if (!actual.Equals(expected)) throw new Exception($"{stage}: expected {expected}, received {actual}"); }
static void Reject(Action callback)
{
    try { callback(); } catch (ArgumentException) { return; }
    throw new Exception("Invalid prepared input accepted.");
}
static void RejectCanceled(Action callback)
{
    try { callback(); } catch (OperationCanceledException) { return; }
    throw new Exception("Canceled operation accepted.");
}
