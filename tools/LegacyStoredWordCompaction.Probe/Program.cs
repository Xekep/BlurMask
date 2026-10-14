using InkRecognition;
using System.Text.Json;

if (args.Length != 5) throw new ArgumentException("controls.json AK LK engine.json separation.json required");
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int comparisons = 0, nonAntisymmetric = 0;
foreach (var test in document.RootElement.GetProperty("comparisons").EnumerateArray())
{
    var comparer = new LegacyNativeStoredWordComparer(Headers(test));
    byte[] a = Hex(test, "a"), b = Hex(test, "b");
    int expected = test.GetProperty("expected").GetInt32();
    Require(comparer.Compare(a, b) == expected, "Independent stored88 comparison differs.");
    if (expected == -1 && comparer.Compare(b, a) == -1) nonAntisymmetric++;
    comparisons++;
}
Require(nonAntisymmetric > 0, "Differing source IDs with two kind1400 descriptors were not exercised.");
Console.WriteLine($"PASS {comparisons} independent88-row comparisons including {nonAntisymmetric} non-antisymmetric source-priority pairs.");

int compactions = 0, unsupported = 0;
foreach (var test in document.RootElement.GetProperty("compactions").EnumerateArray())
{
    var comparer = new LegacyNativeStoredWordComparer(Headers(test));
    byte[] rows = Hex(test, "rows"), payloads = Hex(test, "payloads");
    bool mustReject = test.TryGetProperty("unsupported", out var property) && property.GetBoolean();
    try
    {
        var result = LegacyNativeStoredWordCompaction.Prepare(rows, test.GetProperty("active").GetUInt16(),
            test.GetProperty("primary").GetUInt16(), test.GetProperty("count").GetUInt16(), payloads, comparer);
        Require(!mustReject && result.RetainedCount == test.GetProperty("expectedCount").GetUInt16() &&
            result.FullRecordStorage.Span.SequenceEqual(Hex(test, "expectedRows")) &&
            result.FullSecondaryAlternativeStorage.Span.SequenceEqual(Hex(test, "expectedPayloads")),
            "Independent native sort/dedup/source fallback or full payload transport differs.");
    }
    catch (NotSupportedException) when (mustReject) { unsupported++; }
    Require(rows.AsSpan().SequenceEqual(Hex(test, "rows")) && payloads.AsSpan().SequenceEqual(Hex(test, "payloads")),
        "Owning compactor mutated its input storage.");
    compactions++;
}
Console.WriteLine($"PASS {compactions} complete-storage/payload compactions; {unsupported} native partition limits rejected.");

int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new InvalidDataException("Malformed native compaction accepted.");
}
var guardComparer = new LegacyNativeStoredWordComparer(new uint[] { 0, 0xc00 });
Reject(() => guardComparer.Compare(new byte[87], new byte[88]));
Reject(() => LegacyNativeStoredWordCompaction.Prepare(new byte[87], 0, 0, 1, [], guardComparer));
Reject(() => LegacyNativeStoredWordCompaction.Prepare(new byte[88], 2, 1, 1, [], guardComparer));
Reject(() => LegacyNativeStoredWordCompaction.Prepare(new byte[176], 2, 1, 2, new byte[159], guardComparer));
Reject(() => LegacyNativeStoredWordCompaction.Prepare(new byte[88], 1, 0, 1, [], guardComparer));
Reject(() => LegacyNativeStoredWordCompaction.Prepare(new byte[88], 0, 0, 1, [], guardComparer, new CancellationToken(true)));
var missingHeader = new byte[176];
missingHeader[92] = 2;
Reject(() => LegacyNativeStoredWordCompaction.Prepare(missingHeader, 2, 1, 1, [], guardComparer));
var emptyPass = LegacyNativeStoredWordCompaction.Prepare(new byte[176], 1, 9, 1, [], guardComparer);
Require(emptyPass.RetainedCount == 1, "Native count<=primary limit early return changed count.");
Console.WriteLine($"PASS {guards} extent, payload, ushort-wrap, source-owner and cancellation guards.");
ActualStoredCompaction.Run(args);

static uint[] Headers(JsonElement test) => test.GetProperty("headers").EnumerateArray().Select(x => x.GetUInt32()).ToArray();
static byte[] Hex(JsonElement test, string name) => Convert.FromHexString(test.GetProperty(name).GetString()!);
static void Require(bool condition, string message) { if (!condition) throw new InvalidDataException(message); }
