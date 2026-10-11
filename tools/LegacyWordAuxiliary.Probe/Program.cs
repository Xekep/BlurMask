using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

var pairs = new float[169]; pairs[1 * 13 + 3] = -.25f; pairs[2 * 13 + 3] = .25f;
var profile = new LegacyWordAuxiliaryProfile(512, 4, 1, 0, 1, pairs);
Require(profile.GetBoundaryCost(2) == 49152, "Boundary offsets/scaling changed.");
Require(profile.ScoreCategory(3, new uint[] { 0x010000aa }, false, false) == new LegacyCategoryCost(2048, 1), "Tagged history transition differs.");
Require(profile.ScoreCategory(3, new uint[] { 0xf20000aa }, false, false) == new LegacyCategoryCost(-2048, 1), "Upper nibble/lower class bits leaked into category.");
Require(profile.ScoreCategory(3, new uint[] { 0x010000aa }, true, false) == new LegacyCategoryCost(-2, 0), "Clear-first mutation did not switch to fallback.");
Require(profile.ScoreCategory(3, new uint[] { 0x010000aa }, false, true) == new LegacyCategoryCost(0, 1), "Suppress flag discarded prefix count.");
Require(profile.ScoreCategory(3, [], false, false).Cost == 1 && profile.ScoreCategory(6, [], false, false).Cost == 2 &&
    profile.ScoreCategory(12, [], false, false).Cost == 3 && profile.ScoreCategory(0, new uint[] { 0x01000000 }, false, false) == new LegacyCategoryCost(0, 0), "Native fallback categories changed.");
var disabled = new LegacyWordAuxiliaryProfile(0, 0, 0, 0, 1, pairs);
Require(disabled.ScoreCategory(3, new uint[] { 0x01000000 }, false, false) == new LegacyCategoryCost(1, 0), "CategoryFlags=0 consulted transitions.");
pairs[14] = 1f / 16384; var ties = new LegacyWordAuxiliaryProfile(1, 0, 0, 0, 1, pairs);
Require(ties.ScoreCategory(1, new uint[] { 0x01000000 }, false, false).Cost == -1, "Positive tie not negated after rounding.");
pairs[14] = -1f / 16384; ties = new(1, 0, 0, 0, 1, pairs);
Require(ties.ScoreCategory(1, new uint[] { 0x01000000 }, false, false).Cost == 1, "Negative tie not rounded away before negating.");
MustReject(() => profile.ScoreCategory(13, [], false, false));
MustReject(() => profile.ScoreCategory(1, new uint[] { 0x0d000000 }, false, false));
MustReject(() => profile.ScoreCategory(1, new uint[2], false, false));
foreach (float scale in new[] { 0f, -1f, float.NaN, float.PositiveInfinity, float.MaxValue }) MustReject(() => profile.GetBoundaryCost(scale));
MustReject(() => new LegacyWordAuxiliaryProfile(0, 0, 0, 0, 1, new float[168]));
pairs[14] = float.NaN; MustReject(() => new LegacyWordAuxiliaryProfile(0, 0, 0, 0, 1, pairs));
byte[] own = OwnModel(); int categoryStart = own.Length - (2 + 16 + 16 + 0x2a4 + 0x2254);
void SetFloat(int at, float value) => BinaryPrimitives.WriteInt32LittleEndian(own.AsSpan(at), BitConverter.SingleToInt32Bits(value));
BinaryPrimitives.WriteUInt16LittleEndian(own.AsSpan(categoryStart), 512);
for (int p = 0; p < 4; p++)
{
    SetFloat(categoryStart - 128 + 32 + p * 4, p + 1); SetFloat(categoryStart - 128 + 48 + p * 4, p + .5f);
    SetFloat(categoryStart + 2 + p * 4, p * .25f); SetFloat(categoryStart + 18 + p * 4, p + 1);
}
SetFloat(categoryStart + 34 + (2 * 13 + 3) * 4, -.25f);
var ownProfiles = LegacyWordAuxiliaryProfile.LoadProfiles(own);
for (int p = 0; p < 4; p++)
    Require(ownProfiles[p].CategoryFlags == 512 && ownProfiles[p].GetBoundaryCost(1) == 4096 &&
        ownProfiles[p].ScoreCategory(3, new uint[] { 0x02000000 }, false, false).Cost == 2048, "Factory selected wrong profile coefficient or table index.");
using (var source = new CancellationTokenSource())
{
    source.Cancel(); try { LegacyWordAuxiliaryProfile.LoadProfiles(own, source.Token); throw new InvalidOperationException("Factory cancellation ignored."); }
    catch (OperationCanceledException) { }
}
SetFloat(categoryStart + 34 + 676, float.NaN); MustReject(() => LegacyWordAuxiliaryProfile.LoadProfiles(own));
Console.WriteLine("PASS own category transitions, signed tie rounding, fallback/suppress/clear flags, history tags, boundary scale, bounds and overflow.");
if (args.Length == 0) return;
if (args.Length == 2 && args[0] == "--controls")
{
    using var refs = JsonDocument.Parse(File.ReadAllBytes(args[1])); int checkedCases = 0;
    foreach (var c in refs.RootElement.GetProperty("cases").EnumerateArray())
    {
        var model = new LegacyWordAuxiliaryProfile(c.GetProperty("flags").GetUInt16(), c.GetProperty("wordOffset").GetSingle(), c.GetProperty("boundaryOffset").GetSingle(),
            c.GetProperty("categoryOffset").GetSingle(), c.GetProperty("categorySlope").GetSingle(), c.GetProperty("transitions").EnumerateArray().Select(v => v.GetSingle()).ToArray());
        Require(model.GetBoundaryCost(c.GetProperty("scale").GetSingle()) == c.GetProperty("boundaryCost").GetInt32(), "Independent boundary reference differs.");
        var value = model.ScoreCategory(c.GetProperty("category").GetInt32(), c.GetProperty("history").EnumerateArray().Select(v => v.GetUInt32()).ToArray(), c.GetProperty("clear").GetBoolean(), c.GetProperty("suppress").GetBoolean());
        Require(value.Cost == c.GetProperty("categoryCost").GetInt32() && value.TaggedHistoryPrefixLength == c.GetProperty("taggedCount").GetInt32(), "Independent category reference differs."); checkedCases++;
    }
    Require(checkedCases > 0, "Empty independent controls."); Console.WriteLine($"PASS {checkedCases} independent auxiliary cost references."); return;
}
if (args.Length != 1) throw new ArgumentException("Usage: LegacyWordAuxiliary.Probe [--controls own.json | lexical.res]");
byte[] resource = File.ReadAllBytes(args[0]); const int objectOffset = 2513123;
int length = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(resource.AsSpan(objectOffset + 4)) + 8);
var actual = LegacyWordAuxiliaryProfile.LoadProfiles(resource.AsSpan(objectOffset, length));
for (int p = 0; p < actual.Length; p++)
{
    Require(actual[p].CategoryFlags == 512, "Actual category flag differs.");
    Console.WriteLine($"ACTUAL profile={p}, explicitScale=1, boundaryCost={actual[p].GetBoundaryCost(1)}, category1to3={actual[p].ScoreCategory(3, new uint[] { 0x01000000 }, false, false).Cost}, clearCategory3={actual[p].ScoreCategory(3, new uint[] { 0x01000000 }, true, false).Cost}.");
}
Console.WriteLine("DIAGNOSTIC only: input categories, history flags and profile are explicit; no Unicode-category inference, total score or reranking.");
static void Require(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); }
static void MustReject(Action action)
{
    try { action(); throw new InvalidOperationException("Invalid inputs accepted."); }
    catch (ArgumentException) { } catch (OverflowException) { }
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
