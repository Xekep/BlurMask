using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

var data = OwnModel(); var own = LegacyWordLanguageModel.Load(data);
Require(own.WordCount == 41 && own.ClassCount == 5 && own.DeclaredBigramCount == 17, "Stored dimensions changed.");
for (int i = 0; i < own.WordCount; i++)
    Require(own.GetWordClass(i) == i * 7 % 5 && own.GetStoredWordCost(i) == new[] { 100, 300, 800 }[i % 3],
        "Packed word class or stored word-cost level lookup differs.");
for (int current = 0; current < 5; current++)
{
    Require(own.GetStoredClassCost(current) == new[] { 11, 23, 47 }[current % 3], "Unigram lookup changed.");
    for (int previous = 0; previous < 5; previous++)
        Require(own.GetStoredClassCost(current, previous) == new[] { 102, 501, 903 }[(previous * 5 + current) % 3],
            "Dense previous/current class lookup transposed or changed.");
}
foreach (int index in new[] { -1, 41, int.MaxValue })
{
    RejectRange(() => own.GetWordClass(index)); RejectRange(() => own.GetStoredWordCost(index));
}
foreach (int index in new[] { -1, 5, int.MaxValue })
{
    RejectRange(() => own.GetStoredClassCost(index)); RejectRange(() => own.GetStoredClassCost(0, index));
}
foreach (int length in Enumerable.Range(0, data.Length / 97).Select(i => i * 97).Append(data.Length - 1))
    RejectData(data.AsSpan(0, length).ToArray());
foreach (int offset in new[] { 0, 8, 12, 16, 78, 82, 86, 90, 94, 98, 102, 106, 114 })
{
    var changed = (byte[])data.Clone(); changed[offset] ^= 0x80; RejectData(changed);
}
var badClass = (byte[])data.Clone(); badClass[123] = (byte)((badClass[123] & ~7) | 6); RejectData(badClass);
var badCode = (byte[])data.Clone(); badCode[139] = 3; RejectData(badCode);
using (var canceled = new CancellationTokenSource())
{
    canceled.Cancel();
    try { LegacyWordLanguageModel.Load(data, canceled.Token); throw new InvalidOperationException("Cancellation ignored."); }
    catch (OperationCanceledException) { }
}
Console.WriteLine("PASS own class-based word LM storage, bit-crossing map, word/unigram/dense bigram level lookup, malformed/truncated objects, bounds and cancellation. Raw stored costs only; no word-ID inference or native search claims.");
if (args.Length == 0) return;
if (args.Length is < 1 or > 2) throw new ArgumentException("Usage: LegacyWordLanguage.Probe [lexical.res [independent-controls.json]]");
if (new FileInfo(args[0]).Length > 16 * 1024 * 1024) throw new ArgumentException("Resource exceeds 16MiB.");
byte[] resource = File.ReadAllBytes(args[0]);
const int offsetActual = 2513123;
int bytes = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(resource.AsSpan(offsetActual + 4, 4)) + 8);
var actual = LegacyWordLanguageModel.Load(resource.AsSpan(offsetActual, bytes));
Require(actual.WordCount == 439826 && actual.ClassCount == 1024 && actual.DeclaredBigramCount == 681695,
    "Caller resource is not the inspected RU profile.");
Console.WriteLine($"ACTUAL exact object bytes={bytes}, wordCount={actual.WordCount}, classes={actual.ClassCount}, declaredBigrams={actual.DeclaredBigramCount}, retainedLookupBytes={actual.StorageBytes}.");
if (args.Length == 2)
{
    using var controls = JsonDocument.Parse(File.ReadAllBytes(args[1])); int checkedCases = 0;
    foreach (var c in controls.RootElement.GetProperty("words").EnumerateArray())
    {
        Require(actual.GetWordClass(c.GetProperty("id").GetInt32()) == c.GetProperty("class").GetInt32() &&
            actual.GetStoredWordCost(c.GetProperty("id").GetInt32()) == c.GetProperty("cost").GetInt32(), "Actual word reference lookup differs.");
        checkedCases++;
    }
    foreach (var c in controls.RootElement.GetProperty("classes").EnumerateArray())
    {
        int? previous = c.GetProperty("previous").ValueKind == JsonValueKind.Null ? null : c.GetProperty("previous").GetInt32();
        Require(actual.GetStoredClassCost(c.GetProperty("current").GetInt32(), previous) == c.GetProperty("cost").GetInt32(), "Actual class reference lookup differs.");
        checkedCases++;
    }
    Require(checkedCases > 0, "No independent reference cases.");
    Console.WriteLine($"PASS {checkedCases} independent actual stored word/class cost references. No asset distributed.");
}

static byte[] OwnModel()
{
    using var stream = new MemoryStream(); using var w = new BinaryWriter(stream);
    w.Write(0x0101fe90u); w.Write(0u); w.Write(0x2d1u); w.Write(0x01000000u); w.Write((ushort)0x100);
    w.Write(0u); w.Write(new byte[56]); w.Write(uint.MaxValue); w.Write(0x01000000u); w.Write(0u);
    w.Write(2u); w.Write(3u); w.Write(41u); w.Write(5u); w.Write(5u); w.Write(17u);
    w.Write(0x01000000u); w.Write(41u); w.Write((byte)3);
    var packed = new byte[16];
    for (int i = 0; i < 41; i++)
        for (int bit = 0; bit < 3; bit++) if (((i * 7 % 5) & (1 << bit)) != 0) packed[(i * 3 + bit) / 8] |= (byte)(1 << ((i * 3 + bit) % 8));
    w.Write(packed); w.Write(Enumerable.Range(0, 41).Select(i => (byte)(i % 3)).ToArray());
    w.Write(3u); foreach (int n in new[] { 100, 300, 800 }) w.Write(n);
    w.Write(Enumerable.Range(0, 5).Select(i => (byte)(i % 3)).ToArray());
    w.Write(Enumerable.Range(0, 25).Select(i => (byte)(i % 3)).ToArray());
    w.Write(3u); w.Write(3u); foreach (int n in new[] { 11, 23, 47, 102, 501, 903 }) w.Write(n);
    w.Write(0u); w.Write(0u); w.Write(new byte[4 * 2 * 4 + 16 + 16 + 4 * 2 * 4 + 16 + 16]);
    w.Write(new byte[2 + 16 + 16 + 0x2a4 + 0x2254]);
    byte[] result = stream.ToArray(); BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(4), (uint)result.Length - 8); return result;
}
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
static void RejectData(byte[] data)
{
    try { LegacyWordLanguageModel.Load(data); throw new InvalidOperationException("Malformed language object accepted."); }
    catch (InvalidDataException) { }
}
static void RejectRange(Action action)
{
    try { action(); throw new InvalidOperationException("Invalid lookup index accepted."); }
    catch (ArgumentOutOfRangeException) { }
}
