using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
using InkRecognition.Math;

if (args.Length != 4) throw new ArgumentException("model.ikrmath rawMath2.res official-runtime capture.json");
using var runtime = new OrtRuntime(Path.GetFullPath(args[2]));
using var run = runtime.CreateRun(CancellationToken.None);
byte[] resource = File.ReadAllBytes(args[1]);
using var stream = File.OpenRead(args[0]);
using var bundle = Math2Mode0ColdBundle.Read(runtime, stream, resource);
var features = MathPreprocessor.Process([
    new InkStroke([new InkPoint(0, 30), new(0, 0), new(10, 18), new(20, 0), new(20, 30)]),
    new InkStroke([new InkPoint(35, 0), new(35, 30)])]);
using var source = runtime.Tensor(features.Values, features.Frames, 1, 26);
using var downsampling = runtime.Tensor(Enumerable.Repeat(1L, features.Frames).ToArray(), features.Frames, 1);
using var ends = runtime.Tensor(features.StrokeEnds, 1, features.StrokeEnds.Length);
using var canceled = new CancellationTokenSource(); canceled.Cancel();
int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or InvalidOperationException or
        OperationCanceledException or NotSupportedException or InvalidDataException)
    { guards++; return; }
    throw new InvalidDataException("Required initial-order guard did not reject.");
}
Math2Mode0ColdRecordingLoop Prepare(int width) => width == 1
    ? Math2Mode0ColdRecordingLoop.PrepareColdDefault(bundle, run, source, downsampling, ends)
    : Math2Mode0ColdRecordingLoop.PrepareWithPreparedPolicy(bundle, run, source, downsampling, ends, width, 1f, 1000f);
Projection Project(Math2Mode0CompletedRecordSnapshot record, Math2Mode0CompletedEventLease source)
    => new(BitConverter.SingleToInt32Bits(record.Score), record.Frontier, record.Parent,
        record.Primary.ToArray(), record.PrimaryCapacity, record.Secondary.ToArray(), record.SecondaryCapacity,
        record.Library.LogicalIdentity, record.Library.CopyInitializedPoolQwords(), record.Library.InitializedPoolDword10,
        record.NestedTensorCount, source.Offset, source.Rank, Convert.ToBase64String(source.CopyRawRecord()),
        source.CopyPrimary(), source.CopySecondary(), source.CopyAncestry());
Projection[] ProjectVector(Math2Mode0CompletedRecords vector)
    => vector.CopyRecords().Select((row, index) => Project(row, vector.ReadSourceEvent(index))).ToArray();

var cases = new List<ActualCase>();
foreach (int width in new[] { 1, 3, 16 })
{
    var loop = Prepare(width);
    Reject(() => loop.OrderInitialCompleted());
    Math2Mode0CompletedRecords? vector = null;
    Math2Mode0PrefixContext? retainedContext = null;
    loop.RunDiagnostic((stage, step, candidates, prefix, recorder, fallback) =>
    { vector = recorder; retainedContext = prefix; });
    var before = ProjectVector(vector!);
    var original = vector!.CopyRecords();
    var sources = Enumerable.Range(0, vector.Count).Select(vector.ReadSourceEvent).ToArray();
    int capacity = vector.Capacity;
    Reject(() => loop.OrderInitialCompleted(canceled.Token));
    var moves = new List<Move>();
    var ordered = loop.OrderInitialDiagnostic((stage, index) =>
    {
        if (stage != Math2Mode0InitialOrderingStage.AfterOwnerMove) return;
        var rows = vector.CopyRecords();
        int[] order = Enumerable.Range(0, rows.Length).Select(i => Array.IndexOf(sources, vector.ReadSourceEvent(i))).ToArray();
        if (order.Distinct().Count() != sources.Length || rows.Any(row => row.Library.IsDisposed))
            throw new InvalidDataException("A move lost or released an actual owner.");
        moves.Add(new(index, order));
    });
    int[] permutation = ordered.CopyOriginalOrdinals();
    var sorted = ordered.CopyRecords();
    for (int index = 0; index < sorted.Length; index++)
    {
        if (!ReferenceEquals(sorted[index].Library, original[permutation[index]].Library) ||
            !ReferenceEquals(ordered.ReadOwnedSource(index), sources[permutation[index]]) ||
            sorted[index].Library.IsDisposed || sorted[index].Library.ReleaseCount != 0)
            throw new InvalidDataException("Record, Library3E and actual event must move together.");
    }
    if (!ReferenceEquals(ordered.VectorIdentity, vector) || ordered.IsPostprocessed || ordered.IsFinalOrder ||
        ordered.HasTruncatedAlternatives || vector.Count != before.Length || vector.Capacity != capacity)
        throw new InvalidDataException("Initial ordering must neither postprocess nor truncate.");
    var after = sorted.Select((row, index) => Project(row, ordered.ReadOwnedSource(index))).ToArray();
    var damaged = ordered.CopyOriginalOrdinals(); if (damaged.Length != 0) damaged[0] = -1;
    if (!ordered.CopyOriginalOrdinals().SequenceEqual(permutation)) throw new InvalidDataException("Mutable permutation alias.");
    if (sorted.Length != 0 && sorted[0].Primary.Length != 0)
    {
        ushort expected = sorted[0].Primary[0]; sorted[0].Primary[0] ^= 17;
        if (ordered.CopyRecords()[0].Primary[0] != expected) throw new InvalidDataException("Mutable record snapshot alias.");
    }
    var fake = new Math2Mode0InitiallyOrderedCompleted(loop, vector, permutation);
    Reject(() => vector.CheckOwner(retainedContext!));
    Reject(() => fake.CopyRecords());
    Reject(() => fake.CopyOriginalOrdinals());
    Reject(() => fake.ReadOwnedSource(0));
    Reject(() => loop.OrderInitialCompleted());
    loop.Dispose(); loop.Dispose();
    if (original.Any(row => !row.Library.IsDisposed || row.Library.ReleaseCount != 1 || !row.Library.IsPoolReleased))
        throw new InvalidDataException("Each original Library3E must be released exactly once by context disposal.");
    Reject(() => ordered.CopyRecords());
    Reject(() => loop.OrderInitialCompleted());
    cases.Add(new(width, loop.MaximumPosition, loop.SuccessfulDecoderCalls, capacity, before, after,
        permutation, moves.ToArray(), original.Select(row => row.Library.ReleaseCount).ToArray()));
    Console.WriteLine($"actual initial sort width{width}: {before.Length} records/{moves.Count} owner swaps");
}

// Failures use the real prepared-width16 vector. They are managed boundaries,
// not evidence that native036810 has callbacks or throwing pointer moves.
var failures = new List<Failure>();
foreach (var target in Enum.GetValues<Math2Mode0InitialOrderingStage>())
foreach (bool cancel in new[] { false, true })
{
    var loop = Prepare(16);
    Math2Mode0CompletedRecords? vector = null;
    loop.RunDiagnostic((stage, step, candidates, prefix, recorder, fallback) => vector = recorder);
    var before = ProjectVector(vector!);
    var original = vector!.CopyRecords();
    var sources = Enumerable.Range(0, vector.Count).Select(vector.ReadSourceEvent).ToArray();
    using var stop = new CancellationTokenSource();
    bool reached = false;
    Reject(() => loop.OrderInitialDiagnostic((stage, index) =>
    {
        if (stage != target) return;
        reached = true;
        if (cancel) stop.Cancel(); else throw new InvalidDataException("Prepared initial-order boundary failure.");
    }, stop.Token));
    if (!reached || !loop.IsFailed || !vector.IsFailed || vector.Count != before.Length)
        throw new InvalidDataException("Ordering failure must preserve and poison the actual vector.");
    int[] order = Enumerable.Range(0, vector.Count).Select(i => Array.IndexOf(sources, vector.ReadSourceEvent(i))).ToArray();
    if (order.Distinct().Count() != sources.Length || original.Any(row => row.Library.IsDisposed))
        throw new InvalidDataException("Failure lost or prematurely released a counted owner.");
    var after = ProjectVector(vector);
    Reject(() => loop.OrderInitialCompleted()); Reject(() => loop.RunToRecordedPrefix());
    loop.Dispose();
    if (original.Any(row => row.Library.ReleaseCount != 1 || !row.Library.IsPoolReleased))
        throw new InvalidDataException("Failed vector cleanup must release every original child once.");
    failures.Add(new(target.ToString(), cancel, before, after, order,
        original.Select(row => row.Library.ReleaseCount).ToArray()));
}

var prepared = new List<Prepared>();
uint[] bits = [0xff800000, 0xff7fffff, 0xbf800000, 0x80000000, 0, 0x3f800000, 0x7f7fffff, 0x7f800000, 0x7fc00001];
int[] counts = [0, 1, 2, 19, 20, 21, 22, 41, 42, 64, 255];
for (int control = 0; control < 512; control++)
{
    int count = counts[control % counts.Length];
    byte[] input = new byte[count * 168];
    for (int index = 0; index < count; index++)
    {
        var row = input.AsSpan(index * 168, 168);
        for (int at = 0; at < 168; at++) row[at] = unchecked((byte)(control * 17 + index * 31 + at));
        BinaryPrimitives.WriteUInt32LittleEndian(row, control % 4 == 0 ? 0x3f800000u : bits[(index * 7 + control) % bits.Length]);
        BinaryPrimitives.WriteInt32LittleEndian(row[8..], index);
    }
    byte[] output = LegacyPreparedMathCandidateOrdering.Sort(input);
    prepared.Add(new(control, count, Convert.ToBase64String(input), Convert.ToBase64String(output)));
}
File.WriteAllText(args[3], JsonSerializer.Serialize(new Capture(runtime.Version,
    Convert.ToHexStringLower(SHA256.HashData(resource)), cases.ToArray(), failures.ToArray(), prepared.ToArray(), guards),
    CaptureJson.Default.Capture));
Console.WriteLine($"PASS{cases.Count} actual initial-order vectors/{cases.Sum(value => value.Before.Length)} owned records; " +
    $"{failures.Count} prepared failure/cancel boundaries/512 prepared byte-sort controls/{guards} guards; NOT postprocessed");

record Projection(int ScoreBits, long Frontier, int Parent, ushort[] Primary, int PrimaryCapacity,
    ushort[] Secondary, int SecondaryCapacity, int LibraryId, ulong[] LibraryQwords, uint LibraryDword10,
    int NestedTensorCount, long SourceOffset, int SourceRank, string SourceRaw48, ushort[] SourcePrimary,
    ushort[] SourceSecondary, int[][] SourceAncestry);
record Move(int TargetIndex, int[] ActualOriginalOrdinals);
record ActualCase(int PreparedWidth, ulong MaximumPosition, int DecoderCalls, int VectorCapacity,
    Projection[] Before, Projection[] After, int[] OriginalOrdinals, Move[] Moves, int[] ReleaseCounts);
record Failure(string Stage, bool Cancel, Projection[] Before, Projection[] After,
    int[] ActualOriginalOrdinals, int[] ReleaseCounts);
record Prepared(int Control, int Count, string Input, string Output);
record Capture(string Runtime, string Resource, ActualCase[] Cases, Failure[] Failures, Prepared[] Prepared, int Guards);
[JsonSerializable(typeof(Capture))]
partial class CaptureJson : JsonSerializerContext { }
