using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

// Original zero-weight classifier, deliberately reversed acoustic/lexical label
// IDs, and a dictionary accepting only ИП exercise the complete mapping path.
// Fixed equal acoustic scores cannot manufacture the result without that path.
if (args.Length == 0)
{
    var ownNetwork = LegacyInkNetwork.Load(BuildNetwork());
    var ownActivations = new LegacyInkActivations(new int[48], Enumerable.Repeat(512, 128).ToArray());
    var ownAcoustic = LegacyInkAlphabet.Load(BuildAlphabet("ИП"), 0x103);
    var ownLexical = LegacyInkAlphabet.Load(BuildAlphabet("ПИ"), 0x103);
    var ownGraph = LegacyPackedLexicon.Load(BuildGraph());
    var own = new LegacySegmentedWordRecognizer(ownNetwork, ownActivations, ownAcoustic, ownLexical, ownGraph);
    Require(LegacySeparatedInkSegmenter.TrySegment(OwnGlyphs(1, 0, 0).SelectMany(x => x).Reverse().ToArray(), out var ownGroups),
        "Separated physical ink did not produce groups.");
    var ownResult = own.Recognize(ownGroups);
    Require(ownResult.Count == 1 && ownResult[0].Text == "ИП" && Math.Abs(ownResult[0].LogScore + 2 * Math.Log(2)) < 1e-6,
        "Acoustic→text→lexical label mapping or pipeline probabilities failed.");
    Require(own.Recognize([]).Count == 0 && own.Recognize([OwnGlyphs(1, 0, 0)[0]]).Count == 0,
        "Incomplete/nonterminal word path accepted.");
    using var canceled = new CancellationTokenSource(); canceled.Cancel();
    try { own.Recognize(OwnGlyphs(1, 0, 0), cancellationToken: canceled.Token); throw new InvalidOperationException("Cancellation ignored."); }
    catch (OperationCanceledException) { }
    try { own.Recognize([[], []]); throw new InvalidOperationException("Empty glyph accepted."); }
    catch (ArgumentException) { }
    try { own.Recognize([OwnGlyphs(1, 0, 0)[0], OwnGlyphs(1, 0, 0)[1], OwnGlyphs(1, 0, 0)[0]]); throw new InvalidOperationException("Overlong word accepted."); }
    catch (ArgumentException) { }
    Console.WriteLine("PASS own segmented-word pipeline, reversed label spaces, probabilities, terminal boundaries and cancellation. No vendor assets required.");
    return;
}
if (args.Length != 3) throw new ArgumentException("Usage: LegacySegmentedWord.Probe [acoustic.res lexical.res activations.json]");
byte[] acoustic = File.ReadAllBytes(args[0]), lexical = File.ReadAllBytes(args[1]);
using var tables = JsonDocument.Parse(File.ReadAllBytes(args[2]));
int[] Values(JsonElement x) => x.EnumerateArray().Select(x => x.GetInt32()).ToArray();
var activations = new LegacyInkActivations(Values(tables.RootElement.GetProperty("tanh").GetProperty("values")),
    Values(tables.RootElement.GetProperty("sigmoid").GetProperty("values")));
// Checked offsets identify the inspected legacy lite RU assets. Different
// resources must supply their own proven offsets and label correspondence.
var network = LegacyInkNetwork.Load(acoustic.AsSpan(105197, 187592 - 105197));
var acousticAlphabet = LegacyInkAlphabet.Load(acoustic.AsSpan(99), 0x103);
var lexicalAlphabet = LegacyInkAlphabet.Load(lexical.AsSpan(607074), 0x103);
int graphLength = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(lexical.AsSpan(609984, 4)) + 8);
var graph = LegacyPackedLexicon.Load(lexical.AsSpan(609980, graphLength));
var recognizer = new LegacySegmentedWordRecognizer(network, activations, acousticAlphabet, lexicalAlphabet, graph);

// Own printed abbreviation ИП: two independent four-point physical strokes.
// Original separated-ink grouping proposes complete glyphs; it does not cut
// joined strokes or reproduce native SDK segmentation/recognition accuracy.
foreach (var (scale, x, y) in new[] { (1, 0, 0), (1, -100, 300), (2, 0, 0), (3, 200, -100) })
{
    var strokes = OwnGlyphs(scale, x, y).SelectMany(g => g).Reverse().ToArray();
    Require(LegacySeparatedInkSegmenter.TrySegment(strokes, out var groups) && groups.Length == 2,
        "Original separated-ink grouping failed for own printed abbreviation.");
    var results = recognizer.Recognize(groups);
    Require(results.Count != 0 && results[0].Text == "ИП", "Own segmented ИП example was not the best dictionary path.");
    foreach (var result in results.Take(3))
    {
        using var buffer = new MemoryStream(); using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject(); writer.WriteNumber("scale", scale); writer.WriteNumber("x", x); writer.WriteNumber("y", y);
            writer.WriteString("text", result.Text); writer.WriteNumber("logScore", result.LogScore); writer.WriteEndObject();
        }
        Console.WriteLine(System.Text.Encoding.UTF8.GetString(buffer.ToArray()));
    }
}
Console.WriteLine("PASS four own separated printed ИП variants through original whole-stroke grouping, actual raw-ink classifier, alphabet mapping and dictionary. Joined cursive is not tested.");

static LegacyInkStroke[][] OwnGlyphs(int scale, int x, int y)
{
    LegacyInkPoint[] Points(int[][] points) => points.Select(p => new LegacyInkPoint(checked((short)(p[0] * scale + x)),
        checked((short)(p[1] * scale + y)))).ToArray();
    return [
        [new(Points([[10, 10], [10, 90], [60, 10], [60, 90]]))],
        [new(Points([[80, 90], [80, 10], [130, 10], [130, 90]]))]
    ];
}
static byte[] BuildNetwork()
{
    using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
    writer.Write((ushort)0xfea0); writer.Write((ushort)0x106); writer.Write(0u); writer.Write((byte)0x12); writer.Write((ushort)0x2f);
    writer.Write(229u); writer.Write(2u); writer.Write(0u); writer.Write(0u); writer.Write(3u); writer.Write((ushort)10);
    writer.Write(new byte[229]); writer.Write(new byte[] { 1, 1 });
    writer.Write(new byte[4]); writer.Write(new byte[] { 1, 1, 1 });
    var data = stream.ToArray(); BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(4), (uint)data.Length - 8); return data;
}
static byte[] BuildAlphabet(string text)
{
    using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
    for (int i = 0; i < 7; i++) writer.Write((ushort)65535); writer.Write((ushort)2);
    writer.Write((ushort)1); writer.Write((ushort)0);
    foreach (int count in new[] { 32, 64 })
        for (int i = 0; i < count; i++) writer.Write(i == count - 1 ? (ushort)0 : (ushort)65535);
    writer.Write((ushort)0); writer.Write((ushort)2);
    writer.Write(0u); writer.Write(0u); writer.Write(2u); writer.Write(1u); writer.Write(4u); writer.Write(65535u);
    writer.Write(System.Text.Encoding.UTF8.GetBytes(text)); writer.Write((ushort)0); writer.Write((ushort)1);
    writer.Write((byte)0); writer.Write((ushort)0); return stream.ToArray();
}
static byte[] BuildGraph()
{
    using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
    writer.Write(0x0101fe90u); writer.Write(0u); writer.Write(0x2b4u); writer.Write(0x01000000u);
    writer.Write(0u); writer.Write(3u); writer.Write(2u);
    writer.Write(0x01000000u); writer.Write(3u); writer.Write((byte)2); writer.Write(0b100100u); // 0,1,terminal|0.
    writer.Write((byte)2);
    writer.Write(0x01000000u); writer.Write(3u); writer.Write((byte)1); writer.Write(0b011u); // Child counts1,1,0.
    writer.Write(1u); var data = stream.ToArray(); BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(4), (uint)data.Length - 8); return data;
}
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
