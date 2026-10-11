using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

byte[] own = OwnModel(); var model = LegacyWordLanguageModel.Load(own);
var profile = new LegacyWordLanguageCostProfile(1, 0, 1, 0, 1, 0);
var pairs = new float[169]; pairs[1 * 13 + 3] = -.25f;
var auxiliary = new LegacyWordAuxiliaryProfile(512, 0, -1, 0, 1, pairs);
var scorer = new LegacyWordLanguageScorer(model, profile, auxiliary, 1);
Require(scorer.Score(1, [], 3, false, false, false) == new LegacyWordLanguageScore(2, null, 300, 47, 1, 0, 348), "Own first word fields differ.");
Require(scorer.Score(1, new uint[] { 0x01000004 }, 3, false, false, true) == new LegacyWordLanguageScore(2, 4, 300, 501, 2048, 8192, 11041), "Bigram class or category/boundary addition differs.");
Require(scorer.Score(1, new uint[] { 0x01000004 }, 3, true, false, false).LexicalSubtotal == 799,
    "Clearing category must not discard prior class or change to unigram.");
Require(scorer.Score(1, new uint[] { 0x01000004 }, 3, false, true, false).LexicalSubtotal == 801, "Suppression leaked into class lookup.");
MustReject(() => scorer.Score(1, new uint[] { 5 }, 0, false, false, false));
MustReject(() => scorer.Score(41, [], 0, false, false, false));
MustReject(() => scorer.Score(0, new uint[2], 0, false, false, false));
MustReject(() => new LegacyWordLanguageScorer(model, new(1, 1, 1, 0, 1, 0), auxiliary, 1));
MustReject(() => new LegacyWordLanguageScorer(model, new(1, 0, 2, 0, 1, 0), auxiliary, 1));
foreach (float scale in new[] { 0f, -1f, float.NaN, float.PositiveInfinity }) MustReject(() => new LegacyWordLanguageScorer(model, profile, auxiliary, scale));
using (var source = new CancellationTokenSource())
{
    source.Cancel(); try { scorer.Score(1, [], 0, false, false, false, source.Token); throw new InvalidOperationException("Cancellation ignored."); }
    catch (OperationCanceledException) { }
}
var huge = new LegacyWordLanguageCostProfile(0, -150000, 0, -150000, 0, -150000);
var hugeAux = new LegacyWordAuxiliaryProfile(0, -150000, -150000, 0, 0, pairs);
MustReject(() => new LegacyWordLanguageScorer(model, huge, hugeAux, 1).Score(0, [], 0, false, false, false));
Console.WriteLine("PASS own known-word lexical fields/subtotal, distinct unigram/bigram/category/boundary, flags, matching profiles, bounds, overflow and cancellation.");
if (args.Length == 0) return;
if (args.Length == 2 && args[0] == "--controls")
{
    using var refs = JsonDocument.Parse(File.ReadAllBytes(args[1])); int checkedCases = 0;
    foreach (var c in refs.RootElement.GetProperty("cases").EnumerateArray())
    {
        float wordSlope = c.GetProperty("wordSlope").GetSingle(), wordOffset = c.GetProperty("wordOffset").GetSingle();
        var p = new LegacyWordLanguageCostProfile(wordSlope, wordOffset, wordSlope, c.GetProperty("unigramOffset").GetSingle(), c.GetProperty("bigramSlope").GetSingle(), c.GetProperty("bigramOffset").GetSingle());
        var a = new LegacyWordAuxiliaryProfile(c.GetProperty("flags").GetUInt16(), wordOffset, c.GetProperty("boundaryOffset").GetSingle(), c.GetProperty("categoryOffset").GetSingle(), c.GetProperty("categorySlope").GetSingle(), c.GetProperty("transitions").EnumerateArray().Select(v => v.GetSingle()).ToArray());
        var s = new LegacyWordLanguageScorer(model, p, a, c.GetProperty("scale").GetSingle());
        var result = s.Score(c.GetProperty("wordId").GetInt32(), c.GetProperty("history").EnumerateArray().Select(v => v.GetUInt32()).ToArray(), c.GetProperty("category").GetInt32(), c.GetProperty("clear").GetBoolean(), c.GetProperty("suppress").GetBoolean(), c.GetProperty("includeBoundary").GetBoolean());
        Require(result.WordCost == c.GetProperty("wordCost").GetInt32() && result.ClassCost == c.GetProperty("classCost").GetInt32() && result.CategoryCost == c.GetProperty("categoryCost").GetInt32() && result.BoundaryCost == c.GetProperty("boundaryCost").GetInt32() && result.LexicalSubtotal == c.GetProperty("subtotal").GetInt32(), "Independent complete lexical reference differs."); checkedCases++;
    }
    Require(checkedCases > 0, "Empty references."); Console.WriteLine($"PASS {checkedCases} independent complete lexical subtotal references."); return;
}
if (args.Length != 1) throw new ArgumentException("Usage: LegacyWordScorer.Probe [--controls own.json | lexical.res]");
byte[] resource = File.ReadAllBytes(args[0]); const int languageOffset = 2513123;
ReadOnlySpan<byte> ObjectAt(int offset) => resource.AsSpan(offset, checked((int)BinaryPrimitives.ReadUInt32LittleEndian(resource.AsSpan(offset + 4)) + 8));
var actualModel = LegacyWordLanguageModel.Load(ObjectAt(languageOffset));
var actualProfiles = LegacyWordLanguageCostProfile.LoadProfiles(ObjectAt(languageOffset));
var actualAuxiliary = LegacyWordAuxiliaryProfile.LoadProfiles(ObjectAt(languageOffset));
var index = new LegacyLexiconWordIndex(LegacyPackedLexicon.Load(ObjectAt(609980)));
foreach (var (word, labels) in new (string, ushort[])[] { ("mir", [94,84,83]), ("mif", [94,84,135]) })
{
    Require(index.TryGetWordId(labels, out int id), "Actual word terminal absent.");
    for (int p = 0; p < 4; p++)
    {
        var actualScorer = new LegacyWordLanguageScorer(actualModel, actualProfiles[p], actualAuxiliary[p], 1);
        foreach (bool hasHistory in new[] { false, true }) foreach (bool boundary in new[] { false, true })
        {
            // Class643 is the inspected dictionary class of mama; category0
            // is explicitly diagnostic neutral, not inferred from Unicode.
            var value = actualScorer.Score(id, hasHistory ? new uint[] { 643 } : [], 0, false, false, boundary);
            Console.WriteLine($"ACTUAL word={word}, profile={p}, explicitScale=1, previousClass={(hasHistory ? "643" : "none")}, boundaryFlag={boundary}, wordCost={value.WordCost}, classCost={value.ClassCost}, boundaryCost={value.BoundaryCost}, lexicalSubtotal={value.LexicalSubtotal}.");
        }
    }
}
Console.WriteLine("DIAGNOSTIC lexical subtotal only; no acoustic total, runtime profile selection, Unicode category inference, word acceptance or reranking.");
static void Require(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); }
static void MustReject(Action action)
{
    try { action(); throw new InvalidOperationException("Invalid inputs accepted."); } catch (ArgumentException) { } catch (OverflowException) { }
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
