using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

var model = LegacyWordLanguageModel.Load(OwnModel());
var pairs = new float[169]; pairs[16] = -.25f;
var language = new LegacyWordLanguageScorer(model, new(1, 0, 1, 0, 1, 0), new(512, 0, -1, 0, 1, pairs), 1);
var scorer = new LegacyWordCandidateScorer(language);
var first = scorer.Score(1, [], 3, false, false, false, 1234, 1582, false);
Require(first.IsAccepted && first.TotalCost == 1582 && first.LexicalBudget == 348 && first.Language.WordClass == 2, "Inclusive lexical budget or accumulated path differs.");
Require(!scorer.Score(1, [], 3, false, false, false, 1234, 1582, true).IsAccepted, "Full final heap must reject a tie.");
Require(!scorer.Score(1, [], 3, false, false, false, 1234, 1581, false).IsAccepted, "Budget exceeded by one accepted.");
var full = scorer.Score(1, new uint[] { 0x01000004 }, 3, false, false, true, 77, 11119, true);
Require(full.IsAccepted && full.Language.LexicalSubtotal == 11041 && full.TotalCost == 11118, "Class/category/boundary path differs.");
var negativeLanguage = new LegacyWordLanguageScorer(model, new(1, .125f, 1, 0, 1, 0), new(0, .125f, .125f, 0, 0, pairs), 1);
var negative = new LegacyWordCandidateScorer(negativeLanguage).Score(1, [], 0, false, false, false, 500, 0, false);
Require(negative.IsAccepted && negative.TotalCost == -177 && negative.Language.LexicalSubtotal == -677, "A negative lexical offset must rescue the path rather than pre-pruning it.");
Require(!scorer.Score(1, [], 3, false, false, false, int.MaxValue, 0, false).FitsNativeIntegerRange, "Total overflow wrapped.");
Require(!scorer.Score(1, [], 3, false, false, false, int.MinValue, 0, false).FitsNativeIntegerRange, "Lexical budget overflow wrapped.");
var sentinel = scorer.Score(1, [], 3, false, false, false, LegacyWordCandidateScorer.ImpossibleCost - 348, LegacyWordCandidateScorer.ImpossibleCost, false);
Require(!sentinel.IsAccepted && sentinel.TotalCost == LegacyWordCandidateScorer.ImpossibleCost, "Impossible total accepted at inclusive limit.");
var impossibleLanguage = new LegacyWordLanguageScorer(model, new(1, -10000, 1, 0, 1, 0), new(0, -10000, -10000, 0, 0, pairs), 1);
Require(!new LegacyWordCandidateScorer(impossibleLanguage).Score(1, [], 0, false, false, false, -20000000, LegacyWordCandidateScorer.ImpossibleCost, false).IsAccepted, "Impossible lexical record rescued by negative path.");
MustReject(() => scorer.Score(1, [], 0, false, false, false, 0, LegacyWordCandidateScorer.ImpossibleCost + 1, false));
MustReject(() => new LegacyWordCandidateScorer(null!));
using (var source = new CancellationTokenSource())
{
    source.Cancel(); try { scorer.Score(1, [], 0, false, false, false, 0, 1000, false, source.Token); throw new InvalidOperationException("Cancellation ignored."); }
    catch (OperationCanceledException) { }
}
Console.WriteLine("PASS own ordinary word candidate path: lexical/category/boundary composition, inclusive budget, strict full heap ties, negative offsets, sentinels, widened range guards and cancellation.");
if (args.Length == 0) return;
if (args.Length != 2 || args[0] != "--controls") throw new ArgumentException("Usage: LegacyWordCandidate.Probe [--controls own.json]");
using var refs = JsonDocument.Parse(File.ReadAllBytes(args[1])); int cases = 0;
foreach (var c in refs.RootElement.GetProperty("cases").EnumerateArray())
{
    var r = scorer.Score(c.GetProperty("wordId").GetInt32(), c.GetProperty("history").EnumerateArray().Select(v => v.GetUInt32()).ToArray(), c.GetProperty("category").GetInt32(), c.GetProperty("clear").GetBoolean(), c.GetProperty("suppress").GetBoolean(), c.GetProperty("boundary").GetBoolean(), c.GetProperty("path").GetInt32(), c.GetProperty("limit").GetInt32(), c.GetProperty("full").GetBoolean());
    Require(r.Language.LexicalSubtotal == c.GetProperty("lexical").GetInt32() && r.LexicalBudget == c.GetProperty("budget").GetInt64() && r.TotalCost == c.GetProperty("total").GetInt64() && r.FitsNativeIntegerRange == c.GetProperty("fits").GetBoolean() && r.IsAccepted == c.GetProperty("accepted").GetBoolean(), "Independent complete word candidate reference differs."); cases++;
}
Require(cases > 0, "Empty candidate references."); Console.WriteLine($"PASS {cases} independent ordinary candidate references.");
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static void MustReject(Action action) { try { action(); throw new InvalidOperationException("Invalid input accepted."); } catch (ArgumentException) { } }

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
