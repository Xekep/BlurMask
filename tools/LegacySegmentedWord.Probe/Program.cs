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
    var lattice = own.RecognizeAlternatives([
        new(0, 1, ownGroups[0], -.5f), new(1, 3, ownGroups[1]),
        new(0, 2, ownGroups[0]), new(2, 3, ownGroups[1])], 3);
    Require(lattice.Count == 1 && lattice[0].Text == "ИП" && lattice[0].Cuts.Span.SequenceEqual(new[] { 0, 2, 3 }) &&
        Math.Abs(lattice[0].LogScore - ownResult[0].LogScore) < 1e-6, "Scored alternative boundaries lost the better segmentation.");
    Require(own.RecognizeAlternatives([new(0, 1, ownGroups[0])], 2).Count == 0, "Partial alternative ink coverage accepted.");
    foreach (float badCost in new[] { float.NaN, float.PositiveInfinity, .1f, -1001 })
    {
        try { own.RecognizeAlternatives([new(0, 1, ownGroups[0], badCost)], 1); throw new InvalidOperationException("Invalid segmentation score accepted."); }
        catch (ArgumentException) { }
    }
    try { own.RecognizeAlternatives(Enumerable.Repeat(new LegacyInkGlyphAlternative(0, 1, ownGroups[0]), 5).ToArray(), 1); throw new InvalidOperationException("Alternative scoring work cap ignored."); }
    catch (ArgumentException) { }
    try { own.RecognizeAlternatives([new(0, 1, ownGroups[0])], 1, cancellationToken: canceled.Token); throw new InvalidOperationException("Alternative scoring cancellation ignored."); }
    catch (OperationCanceledException) { }
    try { own.Recognize([OwnGlyphs(1, 0, 0)[0], OwnGlyphs(1, 0, 0)[1], OwnGlyphs(1, 0, 0)[0]]); throw new InvalidOperationException("Overlong word accepted."); }
    catch (ArgumentException) { }
    // Two network outputs are uppercase И and unrelated к; lowercase и is
    // present only as a stored related form, not as a classifier output.
    var formsAcoustic = LegacyInkAlphabet.Load(BuildAlphabet("Ики", withRelatedForms: true), 0x103);
    var formsLexical = LegacyInkAlphabet.Load(BuildAlphabet("пи"), 0x103);
    var formsGraph = LegacyPackedLexicon.Load(BuildGraph(lastLabel: 1));
    var exactForms = new LegacySegmentedWordRecognizer(ownNetwork, ownActivations, formsAcoustic, formsLexical, formsGraph);
    var expandForms = new LegacySegmentedWordRecognizer(ownNetwork, ownActivations, formsAcoustic, formsLexical, formsGraph, relatedFormMode: 0);
    Require(exactForms.Recognize(ownGroups).Count == 0, "Exact text mapping invented a lowercase candidate.");
    var expanded = expandForms.Recognize(ownGroups);
    Require(expanded.Count == 1 && expanded[0].Text == "ии" && Math.Abs(expanded[0].LogScore + 2 * Math.Log(2)) < 1e-6,
        "Stored related forms were not mapped, or duplicate triplet entries inflated the class probability.");
    var expandedCuts = expandForms.RecognizeAlternatives([new(0, 1, ownGroups[0]), new(1, 2, ownGroups[1])], 2);
    Require(expandedCuts.Count == 1 && expandedCuts[0].Text == "ии" && Math.Abs(expandedCuts[0].LogScore - expanded[0].LogScore) < 1e-6,
        "Alternative cuts did not retain related-form mapping.");
    foreach (int mode in new[] { -1, 3 })
    {
        try { _ = new LegacySegmentedWordRecognizer(ownNetwork, ownActivations, formsAcoustic, formsLexical, formsGraph, mode); throw new InvalidOperationException("Invalid relation mode accepted."); }
        catch (ArgumentOutOfRangeException) { }
    }
    // All classifier families can share this mapping without assuming their
    // acoustic IDs match dictionary IDs or adding duplicate alias evidence.
    var mapping = new LegacyAcousticLexicalMapping(ownAcoustic, ownLexical, 2);
    var mapped = mapping.Map([8192, 24576]);
    Require(mapped.Length == 2 && mapped[0].Label == 0 && mapped[1].Label == 1 &&
        Math.Abs(mapped[0].LogScore - Math.Log(.75)) < 1e-6 &&
        Math.Abs(mapped[1].LogScore - Math.Log(.25)) < 1e-6,
        "Reusable mapping assumed equal alphabet IDs or changed quantized probabilities.");
    var sharedForms = new LegacyAcousticLexicalMapping(formsAcoustic, formsLexical, 3, 0);
    var mergedForms = sharedForms.Map([8192, 0, 24576]);
    Require(mergedForms.Length == 1 && mergedForms[0].Label == 1 &&
        Math.Abs(mergedForms[0].LogScore - Math.Log(.75)) < 1e-6,
        "Aliases from separate source classes summed evidence instead of retaining the strongest source.");
    Require(sharedForms.Map([0, 32768, 0]).Length == 0 && mapping.Map([0, 0]).Length == 0,
        "Absent dictionary symbols or zero-probability choices were invented.");
    foreach (int[] bad in new[] { new[] { 1 }, new[] { -1, 1 }, new[] { 32769, 0 } })
    {
        try { mapping.Map(bad); throw new InvalidOperationException("Invalid acoustic scores accepted."); }
        catch (ArgumentException) { }
    }
    try { mapping.Map([8192, 24576], canceled.Token); throw new InvalidOperationException("Reusable mapping ignored cancellation."); }
    catch (OperationCanceledException) { }
    foreach (int width in new[] { 0, 3, 513 })
    {
        try { _ = new LegacyAcousticLexicalMapping(ownAcoustic, ownLexical, width); throw new InvalidOperationException("Invalid mapping width accepted."); }
        catch (ArgumentOutOfRangeException) { }
    }
    Console.WriteLine("PASS reusable acoustic/lexical mapping: reversed IDs, aliases, strongest-source merging, absent/zero scores, bounds and cancellation.");
    Console.WriteLine("PASS own segmented-word pipeline, reversed label spaces, probabilities, terminal boundaries and cancellation. No vendor assets required.");
    return;
}
bool benchmark = args.Length == 4 && args[3] == "--benchmark";
bool relatedWords = args.Length == 4 && args[3] == "--related-words";
if (args.Length != 3 && !benchmark && !relatedWords) throw new ArgumentException("Usage: LegacySegmentedWord.Probe [acoustic.res lexical.res activations.json [--benchmark | --related-words]]");
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
    var alternativeResults = recognizer.RecognizeAlternatives([
        new(0, 1, groups[0]), new(1, 3, groups[1]),
        new(0, 2, groups[0], -.1f), new(2, 3, groups[1])], 3);
    Require(alternativeResults.Count != 0 && alternativeResults[0].Text == "ИП" &&
        alternativeResults[0].Cuts.Span.SequenceEqual(new[] { 0, 1, 3 }) &&
        Math.Abs(alternativeResults[0].LogScore - results[0].LogScore) < 1e-6,
        "Actual classifier → alternative cuts → dictionary scoring differs.");
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
if (relatedWords)
{
    var expandedRecognizer = new LegacySegmentedWordRecognizer(network, activations, acousticAlphabet, lexicalAlphabet, graph, relatedFormMode: 0);
    LegacyInkStroke[] Letter(params short[][][] strokes) => strokes.Select(stroke => new LegacyInkStroke(
        stroke.Select(point => new LegacyInkPoint(point[0], point[1])).ToArray())).ToArray();
    var m = Letter([[0, 90], [0, 10], [25, 60], [50, 10], [50, 90]]);
    var a = Letter([[0, 90], [25, 10], [50, 90]], [[10, 60], [40, 60]]);
    var i = Letter([[0, 10], [0, 90], [50, 10], [50, 90]]);
    var r = Letter([[0, 90], [0, 10], [40, 10], [50, 20], [50, 40], [40, 50], [0, 50]]);
    foreach (var (sample, expected, glyphs) in new (string, string, LegacyInkStroke[][])[] { ("МАМА", "мама", [m, a, m, a]), ("МИР", "мир", [m, i, r]) })
    {
        foreach (var (scale, x, y) in new[] { (1, 0, 0), (1, -100, 300), (2, 0, 0), (3, 200, -100) })
        {
            var physical = glyphs.SelectMany((glyph, index) => glyph.Select(stroke => new LegacyInkStroke(
                stroke.Points.ToArray().Select(point => new LegacyInkPoint(checked((short)((point.X + index * 70) * scale + x)),
                    checked((short)(point.Y * scale + y)))).ToArray()))).ToArray();
            Require(LegacySeparatedInkSegmenter.TrySegment(physical, out var wordGroups) && wordGroups.Length == glyphs.Length,
                "Original printed word did not produce one whole-stroke group per proposed letter.");
            foreach (var (policy, engine) in new[] { ("exact", recognizer), ("storedForms", expandedRecognizer) })
            {
                var matches = engine.Recognize(wordGroups).Take(5).ToArray();
                if (policy == "storedForms") Require(matches.Length != 0 && matches[0].Text == expected,
                    "Original printed word is not the first related-form dictionary match.");
                using var buffer = new MemoryStream(); using (var writer = new Utf8JsonWriter(buffer))
                {
                    writer.WriteStartObject(); writer.WriteString("ownPrintedSample", sample); writer.WriteString("mapping", policy);
                    writer.WriteNumber("scale", scale); writer.WriteNumber("x", x); writer.WriteNumber("y", y);
                    writer.WriteStartArray("matches"); foreach (var match in matches)
                    { writer.WriteStartObject(); writer.WriteString("text", match.Text); writer.WriteNumber("logScore", match.LogScore); writer.WriteEndObject(); }
                    writer.WriteEndArray(); writer.WriteEndObject();
                }
                Console.WriteLine(System.Text.Encoding.UTF8.GetString(buffer.ToArray()));
            }
        }
    }
    Console.WriteLine("PASS eight own separated printed МАМА/МИР variants through original whole-stroke grouping and optional stored-form dictionary mapping. This does not measure handwriting accuracy or preserve written case.");
}
if (benchmark)
{
    var sample = OwnGlyphs(1, 0, 0);
    LegacyInkGlyphAlternative[] hypotheses = [new(0, 1, sample[0]), new(1, 3, sample[1]),
        new(0, 2, sample[0], -.1f), new(2, 3, sample[1])];
    _ = recognizer.RecognizeAlternatives(hypotheses, 3); // Warm up reusable models and runtime.
    long allocated = GC.GetAllocatedBytesForCurrentThread();
    var elapsed = System.Diagnostics.Stopwatch.StartNew();
    for (int i = 0; i < 100; i++)
        Require(recognizer.RecognizeAlternatives(hypotheses, 3)[0].Text == "ИП", "Repeated scoring changed the best word.");
    elapsed.Stop(); allocated = GC.GetAllocatedBytesForCurrentThread() - allocated;
    using var process = System.Diagnostics.Process.GetCurrentProcess();
    Console.WriteLine($"BENCH 100 reused-model alternative word requests, 400 glyph evaluations: {elapsed.Elapsed.TotalMilliseconds:F1} ms; " +
        $"{allocated / 100} allocated bytes/request; {graph.StorageBytes} packed dictionary bytes; " +
        $"{process.PrivateMemorySize64 / (1024 * 1024)} MiB process private memory (includes source resources/runtime, not application memory).");
}

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
static byte[] BuildAlphabet(string text, bool withRelatedForms = false)
{
    var encoded = text.EnumerateRunes().Select(rune => System.Text.Encoding.UTF8.GetBytes(rune.ToString())).ToArray();
    int count = encoded.Length;
    using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
    for (int i = 0; i < 7; i++) writer.Write((ushort)65535); writer.Write((ushort)count);
    writer.Write((ushort)1); writer.Write((ushort)0);
    foreach (int tableSize in new[] { 32, 64 })
        for (int i = 0; i < tableSize; i++) writer.Write(i == tableSize - 1 ? (ushort)0 : (ushort)65535);
    writer.Write((ushort)0); writer.Write((ushort)count);
    uint offset = 0;
    for (int i = 0; i < count; i++) { writer.Write(offset); writer.Write((uint)i); offset += (uint)encoded[i].Length; }
    writer.Write(offset); writer.Write(65535u);
    foreach (var bytes in encoded) writer.Write(bytes);
    for (int i = 0; i < count; i++) writer.Write((ushort)i);
    writer.Write((byte)0); writer.Write((ushort)(withRelatedForms ? 1 : 0));
    if (withRelatedForms)
    {
        Require(count == 3, "Original related-form fixture expects three Cyrillic labels.");
        writer.Write((ushort)0); writer.Write((ushort)2); writer.Write((ushort)0);
        writer.Write(1u); writer.Write(new byte[] { 0, 255, 0 });
    }
    return stream.ToArray();
}
static byte[] BuildGraph(uint lastLabel = 0)
{
    using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
    writer.Write(0x0101fe90u); writer.Write(0u); writer.Write(0x2b4u); writer.Write(0x01000000u);
    writer.Write(0u); writer.Write(3u); writer.Write(2u);
    writer.Write(0x01000000u); writer.Write(3u); writer.Write((byte)2); writer.Write(0b100100u | (lastLabel << 4)); // 0,1,terminal|lastLabel.
    writer.Write((byte)2);
    writer.Write(0x01000000u); writer.Write(3u); writer.Write((byte)1); writer.Write(0b011u); // Child counts1,1,0.
    writer.Write(1u); var data = stream.ToArray(); BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(4), (uint)data.Length - 8); return data;
}
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
