using System.Text.Json;
using InkRecognition;

if (args.Length != 1) throw new ArgumentException("Independent glyph score controls JSON required.");
using var json = JsonDocument.Parse(File.ReadAllBytes(args[0])); int cases = 0;
foreach (var row in json.RootElement.GetProperty("cases").EnumerateArray())
{
    byte[] records = Convert.FromHexString(row.GetProperty("rows").GetString()!);
    var vectorArrays = row.GetProperty("vectors").EnumerateArray().Select(a => a.EnumerateArray()
        .Select(v => BitConverter.Int32BitsToSingle(v.GetInt32())).ToArray()).ToArray();
    var vectors = vectorArrays.Select(v => (ReadOnlyMemory<float>)v).ToArray();
    int[] geometry = row.GetProperty("geometry").EnumerateArray().Select(v => v.GetInt32()).ToArray();
    ushort[] bypass = row.GetProperty("bypass").EnumerateArray().Select(v => v.GetUInt16()).ToArray();
    var cache = new LegacyNativeGlyphScoreCache(records, vectors, geometry, row.GetProperty("blank").GetUInt16(),
        row.GetProperty("blankCost").GetInt32(), row.GetProperty("scale").GetSingle(), bypass);
    float score = cache.ReadSymbolScore(row.GetProperty("symbol").GetUInt16(), row.GetProperty("channel").GetByte());
    if (BitConverter.SingleToInt32Bits(score) != row.GetProperty("expectedScore").GetInt32() ||
        BitConverter.SingleToInt32Bits(cache.ReadFirstCandidateScore()) != row.GetProperty("expectedFirst").GetInt32() ||
        cache.ReadRequestCost(Convert.FromHexString(row.GetProperty("request").GetString()!)) != row.GetProperty("expectedCost").GetInt32())
        throw new Exception($"Independent native glyph channel/cost mismatch at {cases}.");
    byte[] copy = cache.CopyCandidateRecords(); copy.AsSpan().Fill(42);
    records.AsSpan().Clear(); foreach (var v in vectorArrays) v.AsSpan().Clear(); geometry.AsSpan().Clear(); bypass.AsSpan().Clear();
    if (!cache.CopyCandidateRecords().AsSpan().SequenceEqual(Convert.FromHexString(row.GetProperty("rows").GetString()!)))
        throw new Exception("Candidate record ownership lost.");
    if (BitConverter.SingleToInt32Bits(cache.ReadSymbolScore(row.GetProperty("symbol").GetUInt16(), row.GetProperty("channel").GetByte())) != row.GetProperty("expectedScore").GetInt32())
        throw new Exception("Confidence owner lost.");
    using var canceled = new CancellationTokenSource(); canceled.Cancel();
    try { cache.ReadSymbolScore(0, 0, canceled.Token); throw new Exception("Cancellation ignored."); } catch (OperationCanceledException) { }
    cases++;
}
var inputRow = new byte[24]; inputRow[8] = 7;
var own = new LegacyNativeGlyphScoreCache(inputRow, [(ReadOnlyMemory<float>)new float[] { .75f }], [5], 8, 99, 1, [8,9,10,11,12,13,14,15]);
Reject(() => own.ReadSymbolScore(7, 1));
Reject(() => own.ReadRequestCost(new byte[15]));
var request = new byte[16]; request[6] = request[7] = 255; request[8] = 7; request[10] = 255;
Reject(() => own.ReadRequestCost(request));
Reject(() => new LegacyNativeGlyphScoreCache(new byte[25], [], [], 8, 0, 1, new ushort[8]));
Reject(() => new LegacyNativeGlyphScoreCache(inputRow, [(ReadOnlyMemory<float>)Array.Empty<float>()], [], 8, 0, 1, new ushort[8]));
Reject(() => new LegacyNativeGlyphScoreCache([], [], [], 8, 0, float.NaN, new ushort[8]));
Reject(() => new LegacyNativeGlyphScoreCache([], [], [], 8, 0, 1, new ushort[7]));
Reject(() => new LegacyNativeGlyphScoreCache([], [], new int[129], 8, 0, 1, new ushort[8]));
Console.WriteLine($"PASS {cases} independent group/vector/first-score/16-byte cost controls, raw float bits, top20/blank/scaled fallback, native signed wrap, ownership/cancellation and 8 guards. Confidence vector production and actual graph owner resolution remain upstream.");
static void Reject(Action action)
{ try { action(); throw new Exception("Invalid ownership/shape was accepted."); } catch (ArgumentException) { } }
