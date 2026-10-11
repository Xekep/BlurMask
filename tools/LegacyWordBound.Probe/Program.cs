using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

var model = LegacyWordLanguageModel.Load(OwnModel());
Require(model.GetStoredInitialWordLevel() == 100 && model.GetStoredWordCost(0) == 300 && model.GetStoredInitialClassLevel(false) == 11 && model.GetStoredClassCost(0) == 23 && model.GetStoredInitialClassLevel(true) == 102 && model.GetStoredClassCost(0, 0) == 501,
    "Initial codebook levels must not be confused with word/class0 lookup.");
var bound = LegacyWordInitialLanguageBound.GetNativeInitialLanguageBound(model, new(1, 0, 1, 0, 1, 0), 1);
Require(bound == new LegacyInitialLanguageBound(100, 11, 102, 111), "Initial word plus minimum initial class bound differs.");
var negative = LegacyWordInitialLanguageBound.GetNativeInitialLanguageBound(model, new(-1, 0, -1, 0, -1, 0), 1);
Require(negative.NativeInitialBound == -202 && -300 - 501 < negative.NativeInitialBound,
    "Arbitrary negative profiles must retain native first-level semantics, not be called universal lower bounds.");
var capped = LegacyWordInitialLanguageBound.GetNativeInitialLanguageBound(model, new(0, -10000, 0, -10000, 0, -10000), 1);
Require(capped.NativeInitialBound == LegacyWordCandidateScorer.ImpossibleCost, "Native sentinel initial cap differs.");
MustReject(() => LegacyWordInitialLanguageBound.GetNativeInitialLanguageBound(model, new(1, 0, 2, 0, 1, 0), 1));
MustReject(() => LegacyWordInitialLanguageBound.GetNativeInitialLanguageBound(model, new(1, 0, 1, 0, 1, 0), 0));
MustReject(() => LegacyWordInitialLanguageBound.GetNativeInitialLanguageBound(model, new(0, -150000, 0, -150000, 0, -150000), 1));
using (var cancellation = new CancellationTokenSource())
{
    cancellation.Cancel(); try { LegacyWordInitialLanguageBound.GetNativeInitialLanguageBound(model, new(1, 0, 1, 0, 1, 0), 1, cancellation.Token); throw new Exception("Cancellation ignored."); }
    catch (OperationCanceledException) { }
}
Console.WriteLine("PASS own first-codebook language cache bound: distinct lookup IDs, calibrated word/unigram/bigram, negative profiles, sentinel cap, checked overflow and cancellation.");
if (args.Length == 0) return;
if (args.Length == 2 && args[0] == "--controls")
{
    using var document = JsonDocument.Parse(File.ReadAllBytes(args[1])); int count = 0;
    foreach (var c in document.RootElement.GetProperty("cases").EnumerateArray())
    {
        float ws = c.GetProperty("ws").GetSingle();
        var p = new LegacyWordLanguageCostProfile(ws, c.GetProperty("wo").GetSingle(), ws, c.GetProperty("uo").GetSingle(), c.GetProperty("bs").GetSingle(), c.GetProperty("bo").GetSingle());
        var result = LegacyWordInitialLanguageBound.GetNativeInitialLanguageBound(model, p, c.GetProperty("scale").GetSingle());
        Require(result == new LegacyInitialLanguageBound(c.GetProperty("word").GetInt32(), c.GetProperty("uni").GetInt32(), c.GetProperty("bi").GetInt32(), c.GetProperty("bound").GetInt32()), "Independent first-codebook cache bound differs."); count++;
    }
    Require(count > 0, "Empty bound references."); Console.WriteLine($"PASS {count} independent native initial-bound arithmetic controls."); return;
}
if (args.Length != 1) throw new ArgumentException("Usage: LegacyWordBound.Probe [--controls own.json | lexical.res]");
byte[] resource = File.ReadAllBytes(args[0]); const int start = 2513123;
var language = resource.AsSpan(start, checked((int)BinaryPrimitives.ReadUInt32LittleEndian(resource.AsSpan(start + 4)) + 8));
var actual = LegacyWordLanguageModel.Load(language); var profiles = LegacyWordLanguageCostProfile.LoadProfiles(language);
Require(actual.GetStoredInitialWordLevel() == 193 && actual.GetStoredInitialClassLevel(false) == 20663 && actual.GetStoredInitialClassLevel(true) == 3567, "Actual initial codebook storage differs.");
int[] expected = [-31264, -10437, -10437, -9521];
for (int i = 0; i < profiles.Length; i++)
{
    var value = LegacyWordInitialLanguageBound.GetNativeInitialLanguageBound(actual, profiles[i], 1);
    Require(value.NativeInitialBound == expected[i], "Actual profile initial cache bound differs.");
    Console.WriteLine($"ACTUAL profile={i} explicitScale=1 firstWord={value.WordCost} firstUnigram={value.UnigramCost} firstBigram={value.BigramCost} nativeInitialBound={value.NativeInitialBound}; category/boundary excluded.");
}
Console.WriteLine("PASS 4 actual profile initial-cache bounds; this is native elementary arithmetic, not an arbitrary-parameter search guarantee or handwriting test.");
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static void MustReject(Action call) { try { call(); throw new Exception("Invalid input accepted."); } catch (ArgumentException) { } catch (OverflowException) { } }
static byte[] OwnModel()
{
    using var stream = new MemoryStream(); using var w = new BinaryWriter(stream);
    w.Write(0x0101fe90u); w.Write(0u); w.Write(0x2d1u); w.Write(0x01000000u); w.Write((ushort)0x100);
    w.Write(0u); w.Write(new byte[56]); w.Write(uint.MaxValue); w.Write(0x01000000u); w.Write(0u);
    w.Write(2u); w.Write(3u); w.Write(1u); w.Write(1u); w.Write(1u); w.Write(1u);
    w.Write(0x01000000u); w.Write(1u); w.Write((byte)1); w.Write(0u); w.Write((byte)1);
    w.Write(2u); w.Write(100); w.Write(300); w.Write((byte)1); w.Write((byte)1);
    w.Write(2u); w.Write(2u); foreach (int n in new[] { 11, 23, 102, 501 }) w.Write(n);
    w.Write(0u); w.Write(0u); w.Write(new byte[128 + 2 + 16 + 16 + 0x2a4 + 0x2254]);
    var bytes = stream.ToArray(); BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), (uint)bytes.Length - 8); return bytes;
}
