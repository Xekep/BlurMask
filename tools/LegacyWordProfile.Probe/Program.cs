using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

var ownProfile = new LegacyWordLanguageCostProfile(.5f, 1, 2, .25f, -.5f, -.25f);
Require(ownProfile.TransformWordCost(8192, 2) == -8192 && ownProfile.TransformClassCost(8192, false, .5f) == 7168 &&
    ownProfile.TransformClassCost(8192, true, .5f) == -1024, "Explicit scalar cost arithmetic changed.");
Require(ownProfile.TransformWordCost(-1, 1) == -8192 && ownProfile.TransformWordCost(1, 1) == -8191,
    "Signed float-to-int truncation became rounding.");
foreach (float scale in new[] { 0f, -1f, float.NaN, float.PositiveInfinity })
{
    try { ownProfile.TransformWordCost(1, scale); throw new InvalidOperationException("Invalid runtime scale accepted."); }
    catch (ArgumentOutOfRangeException) { }
}
try { new LegacyWordLanguageCostProfile(float.NaN, 0, 0, 0, 0, 0); throw new InvalidOperationException("Nonfinite coefficient accepted."); }
catch (ArgumentException) { }
foreach (var (cost, slope) in new[] { (int.MaxValue, 2f), (int.MinValue, 2f), (1, float.MaxValue) })
{
    try { new LegacyWordLanguageCostProfile(slope, 0, 0, 0, 0, 0).TransformWordCost(cost, 2); throw new InvalidOperationException("Unsupported cost overflow accepted."); }
    catch (ArgumentOutOfRangeException) { }
}
byte[] own = OwnModel(); int calibrationStart = own.Length - (2 + 16 + 16 + 0x2a4 + 0x2254) - 128;
void WriteFloat(int offset, float value) => BinaryPrimitives.WriteInt32LittleEndian(own.AsSpan(calibrationStart + offset), BitConverter.SingleToInt32Bits(value));
for (int p = 0; p < 4; p++)
{
    WriteFloat(p * 8, p + .25f); WriteFloat(p * 8 + 4, p + .5f); WriteFloat(32 + p * 4, p + 1f);
    WriteFloat(64 + p * 8, p + 2f); WriteFloat(64 + p * 8 + 4, p + 3f);
}
var ownProfiles = LegacyWordLanguageCostProfile.LoadProfiles(own);
for (int p = 0; p < 4; p++)
    Require(ownProfiles[p].WordSlope == p + 2f && ownProfiles[p].WordOffset == p + 1f &&
        ownProfiles[p].UnigramSlope == p + 2f && ownProfiles[p].UnigramOffset == p + .25f &&
        ownProfiles[p].BigramSlope == p + 3f && ownProfiles[p].BigramOffset == p + .5f,
        "Factory read incorrect model-major fields or treated class slope as offset.");
using (var canceled = new CancellationTokenSource())
{
    canceled.Cancel(); try { LegacyWordLanguageCostProfile.LoadProfiles(own, canceled.Token); throw new InvalidOperationException("Factory cancellation ignored."); }
    catch (OperationCanceledException) { }
}
WriteFloat(64, float.NaN);
try { LegacyWordLanguageCostProfile.LoadProfiles(own); throw new InvalidOperationException("Nonfinite stored coefficient accepted."); }
catch (ArgumentException) { }
Console.WriteLine("PASS own stored four-profile layout, explicit scalar float arithmetic, signed truncation, scale/coefficient guards, cancellation and cost overflow. No native mode inference or reranking.");
if (args.Length == 0) return;
if (args.Length == 2 && args[0] == "--controls")
{
    using var refs = JsonDocument.Parse(File.ReadAllBytes(args[1])); int checkedCases = 0;
    foreach (var c in refs.RootElement.GetProperty("cases").EnumerateArray())
    {
        var profile = new LegacyWordLanguageCostProfile(c.GetProperty("slope").GetSingle(), c.GetProperty("offset").GetSingle(), 0, 0, 0, 0);
        Require(profile.TransformWordCost(c.GetProperty("cost").GetInt32(), c.GetProperty("runtimeScale").GetSingle()) == c.GetProperty("expected").GetInt32(), "Independent float arithmetic reference differs.");
        checkedCases++;
    }
    Require(checkedCases > 0, "No independent controls."); Console.WriteLine($"PASS {checkedCases} independent own float-to-int cost calibration references."); return;
}
if (args.Length != 1) throw new ArgumentException("Usage: LegacyWordProfile.Probe [--controls own-controls.json | lexical.res]");
if (new FileInfo(args[0]).Length > 16 * 1024 * 1024) throw new ArgumentException("Resource exceeds16MiB.");
byte[] resource = File.ReadAllBytes(args[0]);
ReadOnlySpan<byte> ObjectAt(int offset) => resource.AsSpan(offset, checked((int)BinaryPrimitives.ReadUInt32LittleEndian(resource.AsSpan(offset + 4)) + 8));
var language = LegacyWordLanguageModel.Load(ObjectAt(2513123));
var profiles = LegacyWordLanguageCostProfile.LoadProfiles(ObjectAt(2513123));
var index = new LegacyLexiconWordIndex(LegacyPackedLexicon.Load(ObjectAt(609980)));
foreach (var (name, labels) in new (string, ushort[])[] { ("mir", [94, 84, 83]), ("mif", [94, 84, 135]) })
{
    Require(index.TryGetWordId(labels, out int wordId), "Expected actual dictionary terminal absent.");
    int cls = language.GetWordClass(wordId), wordCost = language.GetStoredWordCost(wordId), classCost = language.GetStoredClassCost(cls);
    for (int p = 0; p < profiles.Length; p++)
        Console.WriteLine($"ACTUAL word={name}, profile={p}, explicitRuntimeScale=1, wordSlope={profiles[p].WordSlope}, wordOffset={profiles[p].WordOffset}, uniSlope={profiles[p].UnigramSlope}, uniOffset={profiles[p].UnigramOffset}, wordCost={profiles[p].TransformWordCost(wordCost, 1)}, unigramCost={profiles[p].TransformClassCost(classCost, false, 1)}.");
}
Console.WriteLine("DIAGNOSTIC stored profiles only; runtimeScale=1 explicitly supplied for comparison. Native external mode selection, history/category costs and acoustic-cost addition remain unverified; no word reranking applied.");
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
