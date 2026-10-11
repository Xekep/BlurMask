using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length >= 6 && args[0] == "--lookup")
{
    byte[] resource = File.ReadAllBytes(args[1]);
    var alphabet = LegacyInkAlphabet.Load(resource.AsSpan(int.Parse(args[2])), Convert.ToUInt16(args[3], 16));
    int offset = int.Parse(args[4]);
    int length = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(resource.AsSpan(offset + 4, 4)) + 8);
    var graph = LegacyPackedLexicon.Load(resource.AsSpan(offset, length));
    foreach (string word in args.Skip(5))
    {
        var labels = new List<ushort>(); bool mapped = true;
        foreach (var rune in word.EnumerateRunes())
        {
            if (!alphabet.TryGetLabel(rune.ToString(), out ushort label)) { mapped = false; break; }
            labels.Add(label);
        }
        using var buffer = new MemoryStream(); using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject(); writer.WriteString("text", word); writer.WriteBoolean("mapped", mapped);
            writer.WriteBoolean("terminal", mapped && graph.Contains(labels.ToArray()));
            writer.WriteStartArray("labels"); foreach (ushort label in labels) writer.WriteNumberValue(label);
            writer.WriteEndArray(); writer.WriteEndObject();
        }
        Console.WriteLine(System.Text.Encoding.UTF8.GetString(buffer.ToArray()));
    }
    return;
}

if (args.Length == 5 && args[0] == "--form-controls")
{
    byte[] resource = File.ReadAllBytes(args[1]);
    var alphabet = LegacyInkAlphabet.Load(resource.AsSpan(int.Parse(args[2])), Convert.ToUInt16(args[3], 16));
    using var controls = JsonDocument.Parse(File.ReadAllBytes(args[4])); int tested = 0;
    foreach (var item in controls.RootElement.EnumerateArray())
    {
        bool actual = alphabet.TryGetRelatedForms(item.GetProperty("label").GetUInt16(), item.GetProperty("mode").GetInt32(), out var forms);
        Require(actual == item.GetProperty("found").GetBoolean(), "Related-form availability differs.");
        var expected = item.GetProperty("forms").EnumerateArray().Select(value => value.GetUInt16()).ToArray();
        Require(new ushort[] { forms.First, forms.Second, forms.Third }.SequenceEqual(expected), "Related-form labels differ.");
        tested++;
    }
    Require(tested > 0, "Empty form controls.");
    Console.WriteLine($"PASS {tested} supplied related-form controls. Native engine was not executed.");
    return;
}

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
if (args.Length != 0) throw new ArgumentException("Usage: LegacyLexicon.Probe [--controls resource.res graph-offset own-controls.json | --alphabet-controls resource.res offset version-hex controls.json | --form-controls resource.res offset version-hex controls.json | --lookup resource.res alphabet-offset version-hex graph-offset words...]");

var related = LegacyInkAlphabet.Load(BuildAlphabet(false, relations: 2), 0x103);
Require(related.IsVirtual(0) && !related.IsVirtual(1) && !related.IsSymbolFlagged(0) && related.IsSymbolFlagged(1),
    "Native virtual slot or stored per-symbol flag differs.");
Require(related.TryGetRelatedForms(1, 0, out var injectedFirst) && injectedFirst == new LegacyInkRelatedForms(1, 0, 0),
    "Missing first relation slot was not filled with the requested label.");
Require(related.TryGetRelatedForms(0, 1, out var fallback) && fallback == new LegacyInkRelatedForms(65535, 0, 0),
    "Absent mode1 did not fall back to mode0.");
Require(related.TryGetRelatedForms(0, 2, out var injectedSecond) && injectedSecond == new LegacyInkRelatedForms(1, 0, 1) &&
    !related.TryGetRelatedForms(1, 2, out _), "Independent mode2 lookup or missing relation differs.");
var wideRelations = LegacyInkAlphabet.Load(BuildAlphabet(false, relations: 128), 0x103);
Require(wideRelations.TryGetRelatedForms(0, 2, out var wideForm) && wideForm == injectedSecond, "Signed-short relation lookup differs.");
byte[] relatedFixture = BuildAlphabet(false, relations: 2);
for (int n = 0; n < relatedFixture.Length; n++) RejectAlphabet(relatedFixture.AsSpan(0, n), 0x103);
var badRelation = relatedFixture.ToArray(); badRelation[^4] = 127; RejectAlphabet(badRelation, 0x103);
badRelation = relatedFixture.ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(badRelation.AsSpan(badRelation.Length - 20), 2); RejectAlphabet(badRelation, 0x103);
try { related.TryGetRelatedForms(0, 3, out _); throw new Exception("Unknown relation mode accepted."); } catch (ArgumentOutOfRangeException) { }
Console.WriteLine("PASS stored related forms: mode fallback, virtual slots, symbol bits, sentinel injection, narrow/wide maps and malformed data.");

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
Require(LegacyLexicalDecoder.Decode(lexicon, lattice, beamWidth: 1).Single().Labels.Span.SequenceEqual(new ushort[] { 1, 2 }),
    "A final nonterminal consumed the narrow fixed-position terminal beam.");
Require(LegacyLexicalDecoder.Decode(lexicon, [ReadOnlyMemory<LegacyLexicalCandidate>.Empty]).Count == 0,
    "Empty candidate lattice accepted.");
using var canceled = new CancellationTokenSource(); canceled.Cancel();
try { LegacyLexicalDecoder.Decode(lexicon, lattice, cancellationToken: canceled.Token); throw new InvalidOperationException("Cancellation ignored."); }
catch (OperationCanceledException) { }
try { LegacyLexicalDecoder.Decode(lexicon, [new LegacyLexicalCandidate[] { new(1, float.NaN) }]); throw new InvalidOperationException("NaN score accepted."); }
catch (ArgumentException) { }
// Different glyph boundaries can spell the same word. Keep its highest-scoring
// cut sequence; a stronger impossible prefix and a nonterminal ending must lose.
LegacyLexicalArc[] arcs = [new(0, 1, 1, -.2f), new(1, 4, 2, -.4f),
    new(0, 2, 1, -.1f), new(2, 4, 2, -.1f), new(0, 4, 3, -.3f),
    new(0, 1, 2, 0), new(1, 4, 3, -.01f), new(0, 4, 1, -.4f)];
var latticeWords = LegacyLexicalLatticeDecoder.Decode(lexicon, arcs, 4);
Require(latticeWords.Count == 3 && latticeWords[0].Labels.Span.SequenceEqual(new ushort[] { 1, 2 }) &&
    latticeWords[0].Cuts.Span.SequenceEqual(new[] { 0, 2, 4 }) && Math.Abs(latticeWords[0].LogScore + .2) < 1e-6,
    "Alternative cut search did not recover the better dictionary word/boundaries.");
Require(latticeWords[1].Labels.Span.SequenceEqual(new ushort[] { 3 }) && latticeWords[2].Labels.Span.SequenceEqual(new ushort[] { 1 }),
    "Lattice ordering, duplicate-word merge or terminal filtering differs.");
Require(LegacyLexicalLatticeDecoder.Decode(lexicon, [new(0, 1, 1, 0)], 2).Count == 0,
    "Incomplete ink coverage accepted.");
Require(LegacyLexicalLatticeDecoder.Decode(lexicon, [new(0, 1, 1, 0), new(1, 2, 3, 0)], 2).Count == 0,
    "Nonterminal lattice word accepted.");
Require(LegacyLexicalLatticeDecoder.Decode(lexicon, [new(0, 1, 1, 0), new(1, 2, 3, 0), new(1, 2, 2, -1)], 2, beamWidth: 1)
    .Single().Labels.Span.SequenceEqual(new ushort[] { 1, 2 }), "A final nonterminal consumed the narrow terminal beam.");
foreach (var bad in new[] { new LegacyLexicalArc(0, 0, 1, 0), new(1, 0, 1, 0), new(-1, 1, 1, 0),
    new(0, 5, 1, 0), new(0, 1, 1, float.NaN), new(0, 1, 1, float.NegativeInfinity), new(0, 1, 1, .1f) })
{
    try { LegacyLexicalLatticeDecoder.Decode(lexicon, [bad], 4); throw new InvalidOperationException("Invalid lattice arc accepted."); }
    catch (ArgumentException) { }
}
try { LegacyLexicalLatticeDecoder.Decode(lexicon, arcs, 4, cancellationToken: canceled.Token); throw new InvalidOperationException("Lattice cancellation ignored."); }
catch (OperationCanceledException) { }
try { LegacyLexicalLatticeDecoder.Decode(lexicon, Enumerable.Repeat(arcs[0], 129).ToArray(), 4); throw new InvalidOperationException("Per-cut hypothesis cap ignored."); }
catch (ArgumentException) { }
try { LegacyLexicalLatticeDecoder.Decode(lexicon, arcs, 129); throw new InvalidOperationException("Cut cap ignored."); }
catch (ArgumentOutOfRangeException) { }
Console.WriteLine("PASS alternative glyph cuts: dictionary prefixes, best boundaries, duplicate words, terminal/coverage guards, malformed arcs, limits and cancellation.");
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
static byte[] BuildAlphabet(bool version104, int relations = 0)
{
    using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
    for (int i = 0; i < 7; i++) writer.Write((ushort)65535); writer.Write((ushort)2);
    writer.Write((ushort)1); writer.Write((ushort)0);
    foreach (int count in new[] { 32, 64 })
        for (int i = 0; i < count; i++) writer.Write(i == count - 1 ? (ushort)0 : (ushort)65535);
    writer.Write((ushort)0); writer.Write((ushort)2);
    writer.Write(0u); writer.Write(0u); writer.Write(2u); writer.Write(1u); writer.Write(4u); writer.Write(65535u);
    writer.Write(System.Text.Encoding.UTF8.GetBytes("ИП"));
    writer.Write(relations == 0 ? (ushort)0 : ushort.MaxValue); writer.Write((ushort)1); // Reverse label table.
    writer.Write((byte)(relations == 0 ? 0 : 1));
    if (relations != 0) writer.Write((byte)2);
    writer.Write((ushort)relations);
    if (relations != 0)
    {
        for (int i = 0; i < relations; i++)
        {
            writer.Write(i == 0 ? ushort.MaxValue : (ushort)1);
            writer.Write(i == 0 ? (ushort)0 : ushort.MaxValue);
            writer.Write(i == 0 ? (ushort)0 : (ushort)1);
        }
        writer.Write(5u); // Modes0/2 present; mode1 must fall back to mode0.
        if (relations < 128) writer.Write(new byte[] { 0, 0, (byte)(relations - 1), 255 });
        else { writer.Write((ushort)0); writer.Write((ushort)0); writer.Write((ushort)(relations - 1)); writer.Write(ushort.MaxValue); }
    }
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
