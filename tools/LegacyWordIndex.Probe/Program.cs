using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

var graph = LegacyPackedLexicon.Load(OwnGraph()); var index = new LegacyLexiconWordIndex(graph);
Require(index.WordCount == 5, "Word count changed.");
foreach (var (labels, expected) in new (ushort[], int)[] { ([1], 0), ([1, 2], 1), ([2], 2), ([2, 1], 3), ([3, 1], 4) })
    Require(index.TryGetWordId(labels, out int id) && id == expected, "Terminal prefix/preorder rank assumed BFS or wrong order.");
Require(!index.TryGetWordId(new ushort[] { 3 }, out _) && !index.TryGetWordId(new ushort[] { 1, 1 }, out _) &&
    !index.TryGetWordId(Array.Empty<ushort>(), out _) && !index.TryGetWordId(0, out _), "Nonword/root got a word ID.");
Require(index.TryGetWordId(2, out int nodeId) && nodeId == 2 && index.TryGetWordId(4, out nodeId) && nodeId == 1,
    "Graph node IDs were treated as word IDs.");
foreach (int state in new[] { -1, 7, int.MaxValue })
{
    try { index.TryGetWordId(state, out _); throw new InvalidOperationException("Invalid graph state accepted."); }
    catch (ArgumentOutOfRangeException) { }
}
using (var canceled = new CancellationTokenSource())
{
    canceled.Cancel();
    try { _ = new LegacyLexiconWordIndex(graph, canceled.Token); throw new InvalidOperationException("Cancellation ignored."); }
    catch (OperationCanceledException) { }
}
var badDepth = OwnGraph(); BinaryPrimitives.WriteUInt32LittleEndian(badDepth.AsSpan(24), 1);
try { _ = new LegacyLexiconWordIndex(LegacyPackedLexicon.Load(badDepth)); throw new InvalidOperationException("Declared depth ignored."); }
catch (InvalidDataException) { }
Console.WriteLine("PASS own ordered terminal index: terminal prefixes, DFS/BFS distinction, nonwords, invalid states, declared depth and cancellation. No ink recognition.");
if (args.Length == 0) return;
if (args.Length is < 1 or > 2) throw new ArgumentException("Usage: LegacyWordIndex.Probe [lexical.res [independent-controls.json]]");
if (new FileInfo(args[0]).Length > 16 * 1024 * 1024) throw new ArgumentException("Resource exceeds16MiB.");
byte[] resource = File.ReadAllBytes(args[0]);
LegacyPackedLexicon GraphAt(int offset) => LegacyPackedLexicon.Load(resource.AsSpan(offset, checked((int)BinaryPrimitives.ReadUInt32LittleEndian(resource.AsSpan(offset + 4)) + 8)));
var actualGraph = GraphAt(609980); var actualIndex = new LegacyLexiconWordIndex(actualGraph);
var language = LegacyWordLanguageModel.Load(resource.AsSpan(2513123, checked((int)BinaryPrimitives.ReadUInt32LittleEndian(resource.AsSpan(2513127)) + 8)));
Require(actualIndex.WordCount == language.WordCount, "Dictionary/LM stored word count differs.");
Console.WriteLine($"ACTUAL enumerated={actualIndex.WordCount}, retainedWordIndexBytes={actualIndex.StorageBytes}.");
if (args.Length == 2)
{
    using var references = JsonDocument.Parse(File.ReadAllBytes(args[1])); int checkedCases = 0;
    foreach (var c in references.RootElement.GetProperty("cases").EnumerateArray())
    {
        ushort[] labels = c.GetProperty("labels").EnumerateArray().Select(x => x.GetUInt16()).ToArray();
        Require(actualIndex.TryGetWordId(labels, out int id) && id == c.GetProperty("wordId").GetInt32(), "Actual terminal enumeration reference differs.");
        Require(language.GetStoredWordCost(id) == c.GetProperty("storedWordCost").GetInt32(), "Actual dictionary-to-stored-word-cost reference differs.");
        checkedCases++;
        Console.WriteLine($"ACTUAL wordId={id}, wordClass={language.GetWordClass(id)}, storedWordCost={language.GetStoredWordCost(id)}, storedClassUnigram={language.GetStoredClassCost(language.GetWordClass(id))}.");
    }
    Require(checkedCases > 0, "No actual references.");
    Console.WriteLine($"PASS {checkedCases} actual terminal-rank/stored-word-cost references. Costs are uncalibrated, word-level scoring and runtime permutations remain caller responsibilities.");
}
static byte[] OwnGraph()
{
    using var stream = new MemoryStream(); using var w = new BinaryWriter(stream);
    w.Write(0x0101fe90u); w.Write(0u); w.Write(0x2b4u); w.Write(0x01000000u); w.Write(0u); w.Write(7u); w.Write(2u);
    void Packed(int[] values, byte bits)
    {
        w.Write(0x01000000u); w.Write((uint)values.Length); w.Write(bits);
        var bytes = new byte[(values.Length * bits + 31) / 32 * 4];
        for (int i = 0; i < values.Length; i++) for (int j = 0; j < bits; j++)
            if ((values[i] & (1 << j)) != 0) bytes[(i * bits + j) / 8] |= (byte)(1 << ((i * bits + j) % 8));
        w.Write(bytes);
    }
    Packed([0, 5, 6, 3, 6, 5, 5], 3); w.Write((byte)3); Packed([3, 1, 1, 1, 0, 0, 0], 2); w.Write(5u);
    byte[] result = stream.ToArray(); BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(4), (uint)result.Length - 8); return result;
}
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
