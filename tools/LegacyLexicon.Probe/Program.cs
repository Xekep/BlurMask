using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length == 5 && args[0] == "--alphabet-controls")
{
    byte[] resource = File.ReadAllBytes(args[1]); int offset = int.Parse(args[2]);
    var alphabet = LegacyInkAlphabet.Load(resource.AsSpan(offset), Convert.ToUInt16(args[3], 16));
    using var controls = JsonDocument.Parse(File.ReadAllBytes(args[4])); int tested = 0;
    foreach (var item in controls.RootElement.GetProperty("mapping").EnumerateObject())
    {
        ushort label = ushort.Parse(item.Name); string value = item.Value.GetString()!;
        Require(alphabet.TryGetText(label, out string actual) && actual == value, "Alphabet label mapping mismatch.");
        Require(alphabet.TryGetLabel(value, out ushort actualLabel) && actualLabel == label, "Alphabet text mapping mismatch.");
        tested++;
    }
    Require(alphabet.LabelCount == controls.RootElement.GetProperty("label_count").GetInt32(), "Alphabet dimension mismatch.");
    Require(alphabet.BytesConsumed == controls.RootElement.GetProperty("end").GetInt32() - offset, "Alphabet payload end mismatch.");
    Console.WriteLine($"PASS {tested} alphabet mappings; {alphabet.BytesConsumed} consumed bytes. No ink recognition tested.");
    return;
}

if (args.Length == 4 && args[0] == "--controls")
{
    byte[] resource = File.ReadAllBytes(args[1]);
    int offset = int.Parse(args[2]);
    Require(offset >= 0 && resource.Length - offset >= 8, "Invalid graph offset.");
    int length = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(resource.AsSpan(offset + 4, 4)) + 8);
    var graph = LegacyPackedLexicon.Load(resource.AsSpan(offset, length));
    using var controls = JsonDocument.Parse(File.ReadAllBytes(args[3]));
    int tested = 0;
    foreach (var test in controls.RootElement.GetProperty("cases").EnumerateArray())
    {
        ushort[] labels = test.GetProperty("labels").EnumerateArray().Select(x => x.GetUInt16()).ToArray();
        Require(graph.Contains(labels) == test.GetProperty("terminal").GetBoolean(), "Dictionary lookup mismatch.");
        tested++;
    }
    Console.WriteLine($"PASS {tested} dictionary controls; {graph.NodeCount} nodes, {graph.StorageBytes} owned bytes. No ink recognition tested.");
    return;
}
if (args.Length != 0) throw new ArgumentException("Usage: LegacyLexicon.Probe [--controls resource.res graph-offset own-controls.json | --alphabet-controls resource.res offset version-hex controls.json]");

// An original two-label Cyrillic fixture tests both serialized alphabet versions.
byte[] ownAlphabet = BuildAlphabet(false);
foreach (bool version104 in new[] { false, true })
{
    byte[] payload = BuildAlphabet(version104); ushort version = version104 ? (ushort)0x104 : (ushort)0x103;
    var alphabet = LegacyInkAlphabet.Load(payload, version);
    Require(alphabet.LabelCount == 2 && alphabet.BytesConsumed == payload.Length &&
        alphabet.TryGetText(0, out string first) && first == "И" && alphabet.TryGetLabel("П", out ushort second) && second == 1,
        "Own Cyrillic alphabet mapping failed.");
    Require(!alphabet.TryGetLabel("a", out _) && !alphabet.TryGetText(2, out _), "Missing alphabet entry accepted.");
    for (int n = 0; n < payload.Length; n++) RejectAlphabet(payload.AsSpan(0, n), version);
}
var brokenUtf8 = ownAlphabet.ToArray(); brokenUtf8[242] = 255; RejectAlphabet(brokenUtf8, 0x103);
var nonmonotonicStrings = ownAlphabet.ToArray(); BinaryPrimitives.WriteUInt32LittleEndian(nonmonotonicStrings.AsSpan(226, 4), 5); RejectAlphabet(nonmonotonicStrings, 0x103);
RejectAlphabet(ownAlphabet, 0x102);

// Own BFS dictionary: terminal a, ab, c; nonterminal ac with descendant acb.
// Width3 deliberately crosses byte/word boundaries, as does the offset array.
byte[] fixture = Build([0, 5, 7, 6, 3, 6], [2, 2, 0, 0, 1, 0], 3, 3, 3, 4);
var lexicon = LegacyPackedLexicon.Load(fixture);
Require(lexicon.Contains([1]) && lexicon.Contains([1, 2]) && lexicon.Contains([3]) && lexicon.Contains([1, 3, 2]), "Synthetic valid words rejected.");
Require(!lexicon.Contains([]) && !lexicon.Contains([1, 3]) && !lexicon.Contains([2]) && !lexicon.Contains([1, 2, 3]), "Synthetic nonwords accepted.");
Require(lexicon.TryAdvance(0, 1, out int a) && a == 1 && lexicon.GetLabel(a) == 1 && lexicon.IsTerminal(a), "Root transition mismatch.");
Span<int> children = stackalloc int[2];
Require(lexicon.CopyChildren(a, children) == 2 && children.SequenceEqual([3, 4]), "Child enumeration mismatch.");
Require(lexicon.GetChildCount(2) == 0 && lexicon.StorageBytes < 32, "Packed storage/leaf mismatch.");

for (int n = 0; n < fixture.Length; n++) Reject(fixture.AsSpan(0, n));
foreach (int offset in new[] { 0, 4, 8, 12, 16, 20, 24, 28, 32 })
{
    var bad = fixture.ToArray(); BinaryPrimitives.WriteUInt32LittleEndian(bad.AsSpan(offset, 4), uint.MaxValue); Reject(bad);
}
Reject(Build([0, 5, 7, 6, 3, 6], [2, 2, 0, 0, 2, 0], 3, 3, 3, 4)); // Child overflow.
Reject(Build([0, 5, 7, 6, 3, 6], [2, 1, 0, 0, 1, 0], 3, 3, 3, 4)); // Uncovered node.
Reject(Build([0, 5, 7, 6, 3, 6], [2, 2, 0, 0, 1, 0], 3, 1, 3, 4)); // Offset too wide.
Reject(Build([0, 7, 5, 6, 3, 6], [2, 2, 0, 0, 1, 0], 3, 3, 3, 4)); // Unordered sibling labels.
Reject(Build([0, 5, 5, 6, 3, 6], [2, 2, 0, 0, 1, 0], 3, 3, 3, 4)); // Duplicate sibling labels.
Reject(Build([0, 5], [0, 1], 3, 3, 3, 1)); // Unreachable self-child despite total count=N-1.
var wrongTerminals = fixture.ToArray(); wrongTerminals[^1] = 1; Reject(wrongTerminals);

// Larger own fixture exercises 9-bit labels and long offset crossings.
var wideLabels = Enumerable.Range(0, 134).Select(i => i == 0 ? 0u : (uint)i | 256u).ToArray();
var wideCounts = new uint[134]; wideCounts[0] = 133;
var wide = LegacyPackedLexicon.Load(Build(wideLabels, wideCounts, 9, 8, 8, 1));
for (ushort i = 1; i <= 133; i++) Require(wide.Contains([i]) && wide.TryAdvance(0, i, out int child) && child == i, "Wide packed value mismatch.");
Require(!wide.Contains([134]), "Missing wide label accepted.");

// The highest-scoring raw choice at position0 is absent from the dictionary.
// Prefix filtering recovers ab, keeps duplicate label scores and rejects ac.
ReadOnlyMemory<LegacyLexicalCandidate>[] lattice = [
    new LegacyLexicalCandidate[] { new(2, 0), new(1, -.2f), new(1, -.5f), new(3, -.3f) },
    new LegacyLexicalCandidate[] { new(3, -.1f), new(2, -.4f) }
];
var decoded = LegacyLexicalDecoder.Decode(lexicon, lattice);
Require(decoded.Count == 1 && decoded[0].Labels.Span.SequenceEqual(new ushort[] { 1, 2 }) && Math.Abs(decoded[0].LogScore + .6) < 1e-6,
    "Dictionary beam search failed to recover a valid ambiguous word.");
Require(LegacyLexicalDecoder.Decode(lexicon, [ReadOnlyMemory<LegacyLexicalCandidate>.Empty]).Count == 0,
    "Empty candidate lattice accepted.");
using var canceled = new CancellationTokenSource(); canceled.Cancel();
try { LegacyLexicalDecoder.Decode(lexicon, lattice, cancellationToken: canceled.Token); throw new InvalidOperationException("Cancellation ignored."); }
catch (OperationCanceledException) { }
try { LegacyLexicalDecoder.Decode(lexicon, [new LegacyLexicalCandidate[] { new(1, float.NaN) }]); throw new InvalidOperationException("NaN score accepted."); }
catch (ArgumentException) { }
Console.WriteLine("PASS own packed-trie transitions, terminal words, crossing bits and malformed input. No ink recognition tested.");

static byte[] Build(uint[] labels, uint[] counts, byte labelWidth, byte offsetWidth, byte countWidth, uint maxLength)
{
    using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
    writer.Write(0x0101fe90u); writer.Write(0u); writer.Write(0x2b4u); writer.Write(0x01000000u);
    writer.Write(0u); writer.Write((uint)labels.Length); writer.Write(maxLength);
    WritePacked(writer, labels, labelWidth); writer.Write(offsetWidth); WritePacked(writer, counts, countWidth);
    writer.Write((uint)labels.Skip(1).Count(x => (x & (1u << (labelWidth - 1))) != 0));
    byte[] data = stream.ToArray(); BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(4), (uint)data.Length - 8); return data;
}
static byte[] BuildAlphabet(bool version104)
{
    using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
    for (int i = 0; i < 7; i++) writer.Write((ushort)65535); writer.Write((ushort)2);
    writer.Write((ushort)1); writer.Write((ushort)0);
    foreach (int count in new[] { 32, 64 })
        for (int i = 0; i < count; i++) writer.Write(i == count - 1 ? (ushort)0 : (ushort)65535);
    writer.Write((ushort)0); writer.Write((ushort)2);
    writer.Write(0u); writer.Write(0u); writer.Write(2u); writer.Write(1u); writer.Write(4u); writer.Write(65535u);
    writer.Write(System.Text.Encoding.UTF8.GetBytes("ИП"));
    writer.Write((ushort)0); writer.Write((ushort)1); // Reverse label table.
    writer.Write((byte)0); writer.Write((ushort)0); // No bitset or triplets.
    if (version104) writer.Write((byte)1);
    return stream.ToArray();
}
static void RejectAlphabet(ReadOnlySpan<byte> data, ushort version)
{
    try { LegacyInkAlphabet.Load(data, version); }
    catch (InvalidDataException) { return; }
    throw new InvalidOperationException("Malformed alphabet accepted.");
}
static void WritePacked(BinaryWriter writer, uint[] values, byte width)
{
    writer.Write(0x01000000u); writer.Write((uint)values.Length); writer.Write(width);
    var bytes = new byte[((values.Length * width + 31) / 32) * 4];
    for (int i = 0; i < values.Length; i++)
        for (int j = 0; j < width; j++)
            if ((values[i] & (1u << j)) != 0) { int bit = i * width + j; bytes[bit / 8] |= (byte)(1 << (bit % 8)); }
    writer.Write(bytes);
}
static void Reject(ReadOnlySpan<byte> data)
{
    try { LegacyPackedLexicon.Load(data); }
    catch (Exception e) when (e is InvalidDataException or OverflowException) { return; }
    throw new InvalidOperationException("Malformed graph accepted.");
}
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
