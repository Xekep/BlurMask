using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

var graph = LegacyPackedLexicon.Load(OwnGraph()); var index = new LegacyLexiconWordIndex(graph);
var model = LegacyWordLanguageModel.Load(OwnModel());
LegacyWordCandidateScorer Scorer(float offset = 0) => new(new(model,
    new(1, offset, 1, 0, 1, 0), new(0, offset, offset - 1, 0, 0, new float[169]), 1));
var decoder = new LegacyIntegerWordDecoder(graph, index, Scorer());
ReadOnlyMemory<LegacyIntegerLexicalCandidate>[] choices = [new LegacyIntegerLexicalCandidate[] { new(1, 0), new(2, 100), new(3, 20) }];
var winner = decoder.Decode(choices, [], 0, false, false, false, 0, 10000, false, 1).Single();
Require(winner.WordId == 1 && winner.Labels.Span.SequenceEqual(new ushort[] { 2 }) && winner.AcousticCost == 100 && winner.Score.TotalCost == 250 && winner.Score.Language.WordCost == 100 && winner.Score.Language.ClassCost == 50,
    "Terminal lexical scoring must happen before final beam1 pruning.");
Require(decoder.Decode(choices, [], 0, false, false, false, 7, 257, false, 3).Single().WordId == 1, "Additional path cost or inclusive budget differs.");
Require(decoder.Decode(choices, [], 0, false, false, false, 7, 257, true, 3).Count == 0, "Explicit full heap tie accepted.");
var negative = new LegacyIntegerWordDecoder(graph, index, Scorer(.125f)).Decode(choices, [], 0, false, false, false, 0, 0, false, 3);
Require(negative.Count == 2 && negative[0].Score.TotalCost == -774 && negative[0].AcousticCost == 100, "Negative terminal lexical offset must rescue positive acoustic costs.");
Require(decoder.Decode(choices, [], 0, false, false, false, int.MaxValue, LegacyWordCandidateScorer.ImpossibleCost, false, 3).Count == 0, "Additional path overflow wrapped.");
MustReject(() => new LegacyIntegerWordDecoder(LegacyPackedLexicon.Load(OwnGraph()), index, Scorer()));
MustReject(() => new LegacyIntegerWordDecoder(graph, index, new(new(LegacyWordLanguageModel.Load(OwnModel(2)), new(1, 0, 1, 0, 1, 0), new(0, 0, 0, 0, 0, new float[169]), 1))));
MustReject(() => decoder.Decode(choices, new uint[2], 0, false, false, false, 0, 10, false, 1));
MustReject(() => decoder.Decode(choices, [], 13, false, false, false, 0, 10, false, 1));
MustReject(() => decoder.Decode(choices, [], 0, false, false, false, 0, 10, false, 0));
MustReject(() => decoder.Decode(choices, [], 0, false, false, false, 0, LegacyWordCandidateScorer.ImpossibleCost + 1, false, 1));
using (var cancelled = new CancellationTokenSource())
{
    cancelled.Cancel(); try { decoder.Decode(choices, [], 0, false, false, false, 0, 10, false, 1, cancelled.Token); throw new Exception("Cancellation ignored."); }
    catch (OperationCanceledException) { }
}
Console.WriteLine("PASS own integer word pipeline: terminal rescoring before pruning, cost components, explicit limits/heap ties, negative lexical rescue, graph/model ownership, bounds and cancellation.");
if (args.Length == 0) return;
if (args.Length == 2 && args[0] == "--controls")
{
    using var references = JsonDocument.Parse(File.ReadAllBytes(args[1])); int count = 0;
    foreach (var c in references.RootElement.GetProperty("cases").EnumerateArray())
    {
        var positions = new ReadOnlyMemory<LegacyIntegerLexicalCandidate>[] { c.GetProperty("choices").EnumerateArray().Select(x => new LegacyIntegerLexicalCandidate(x[0].GetUInt16(), x[1].GetInt32())).ToArray() };
        var d = new LegacyIntegerWordDecoder(graph, index, Scorer(c.GetProperty("offset").GetSingle()));
        var results = d.Decode(positions, c.GetProperty("history").EnumerateArray().Select(x => x.GetUInt32()).ToArray(), c.GetProperty("category").GetInt32(), c.GetProperty("clear").GetBoolean(), c.GetProperty("suppress").GetBoolean(), c.GetProperty("boundary").GetBoolean(), c.GetProperty("additional").GetInt32(), c.GetProperty("limit").GetInt32(), c.GetProperty("full").GetBoolean(), c.GetProperty("beam").GetInt32());
        var expected = c.GetProperty("expected").EnumerateArray().ToArray(); Require(results.Count == expected.Length, "Independent pipeline count differs.");
        for (int i = 0; i < results.Count; i++)
        {
            var e = expected[i]; var r = results[i];
            Require(r.WordId == e.GetProperty("wordId").GetInt32() && r.AcousticCost == e.GetProperty("acoustic").GetInt32() && r.Score.PreLexicalPathCost == e.GetProperty("path").GetInt32() && r.Score.Language.LexicalSubtotal == e.GetProperty("lexical").GetInt32() && r.Score.TotalCost == e.GetProperty("total").GetInt64(), "Independent pipeline component differs.");
        }
        count++;
    }
    Require(count > 0, "Empty integer word references."); Console.WriteLine($"PASS {count} independent integer word pipeline references."); return;
}
if (args.Length != 1) throw new ArgumentException("Usage: LegacyIntegerWord.Probe [--controls own.json | lexical.res]");
byte[] resource = File.ReadAllBytes(args[0]);
ReadOnlySpan<byte> At(int start) => resource.AsSpan(start, checked((int)BinaryPrimitives.ReadUInt32LittleEndian(resource.AsSpan(start + 4)) + 8));
var actualGraph = LegacyPackedLexicon.Load(At(609980)); var actualIndex = new LegacyLexiconWordIndex(actualGraph);
var actualModel = LegacyWordLanguageModel.Load(At(2513123));
var profiles = LegacyWordLanguageCostProfile.LoadProfiles(At(2513123)); var auxiliary = LegacyWordAuxiliaryProfile.LoadProfiles(At(2513123));
var actual = new LegacyIntegerWordDecoder(actualGraph, actualIndex, new(new(actualModel, profiles[2], auxiliary[2], 1)));
ReadOnlyMemory<LegacyIntegerLexicalCandidate>[] prepared = [new LegacyIntegerLexicalCandidate[] { new(94, 20) }, new LegacyIntegerLexicalCandidate[] { new(84, 30) }, new LegacyIntegerLexicalCandidate[] { new(83, 100), new(135, 0) }];
int cases = 0;
foreach (bool prior in new[] { false, true }) foreach (bool boundary in new[] { false, true })
{
    int mirLexical = (prior ? 11531 : 4537) + (boundary ? 11264 : 0);
    int mifLexical = (prior ? 16424 : 9461) + (boundary ? 11264 : 0);
    var result = actual.Decode(prepared, prior ? new uint[] { 643 } : [], 0, false, false, boundary, 7, LegacyWordCandidateScorer.ImpossibleCost, false, 2);
    Require(result.Count == 2 && result[0].WordId == 198893 && result[1].WordId == 200417 && result[0].AcousticCost == 150 && result[1].AcousticCost == 50 && result[0].Score.Language.LexicalSubtotal == mirLexical && result[1].Score.Language.LexicalSubtotal == mifLexical && result[0].Score.TotalCost == mirLexical + 157 && result[1].Score.TotalCost == mifLexical + 57, "Actual supplied-choice dictionary/component result differs.");
    var narrow = actual.Decode(prepared, prior ? new uint[] { 643 } : [], 0, false, false, boundary, 7, LegacyWordCandidateScorer.ImpossibleCost, false, 1).Single();
    Require(narrow.WordId == 198893, "Actual terminal language reranking lost beam1 winner.");
    Console.WriteLine($"ACTUAL supplied-choice dictionary: profile=2 scale=1 prior={(prior ? "643" : "none")} boundary={boundary} mir acoustic=150 lexical={mirLexical} total={mirLexical + 157}; mif acoustic=50 lexical={mifLexical} total={mifLexical + 57}; beam1=mir."); cases++;
}
Console.WriteLine($"PASS {cases} actual resource dictionary pipeline controls. Glyph choices are explicitly synthetic; no handwriting accuracy or cursive recognition is claimed.");

static void Require(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); }
static void MustReject(Action call) { try { call(); throw new Exception("Invalid input accepted."); } catch (ArgumentException) { } }
static byte[] OwnGraph()
{
    using var stream = new MemoryStream(); using var w = new BinaryWriter(stream);
    w.Write(0x0101fe90u); w.Write(0u); w.Write(0x2b4u); w.Write(0x01000000u); w.Write(0u); w.Write(4u); w.Write(1u);
    w.Write(0x01000000u); w.Write(4u); w.Write((byte)3); w.Write(0u | (5u << 3) | (6u << 6) | (7u << 9));
    w.Write((byte)3); w.Write(0x01000000u); w.Write(4u); w.Write((byte)2); w.Write(3u); w.Write(3u);
    var bytes = stream.ToArray(); BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), (uint)bytes.Length - 8); return bytes;
}
static byte[] OwnModel(int words = 3)
{
    using var stream = new MemoryStream(); using var w = new BinaryWriter(stream);
    w.Write(0x0101fe90u); w.Write(0u); w.Write(0x2d1u); w.Write(0x01000000u); w.Write((ushort)0x100);
    w.Write(0u); w.Write(new byte[56]); w.Write(uint.MaxValue); w.Write(0x01000000u); w.Write(0u);
    w.Write(2u); w.Write(3u); w.Write((uint)words); w.Write(2u); w.Write(2u); w.Write(4u);
    w.Write(0x01000000u); w.Write((uint)words); w.Write((byte)1); w.Write(2u);
    w.Write(new byte[] { 0, 1, 2 }.Take(words).ToArray()); w.Write(3u); foreach (int x in new[] { 1000, 100, 300 }) w.Write(x);
    w.Write(new byte[] { 0, 1 }); w.Write(new byte[] { 0, 1, 2, 3 });
    w.Write(2u); w.Write(4u); foreach (int x in new[] { 30, 50, 40, 80, 120, 160 }) w.Write(x);
    w.Write(0u); w.Write(0u); w.Write(new byte[128 + 2 + 16 + 16 + 0x2a4 + 0x2254]);
    var bytes = stream.ToArray(); BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), (uint)bytes.Length - 8); return bytes;
}
