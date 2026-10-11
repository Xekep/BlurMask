using System.Text.Json;
using InkRecognition;

LegacyAcousticCostFamily?[] slots = new LegacyAcousticCostFamily?[7];
slots[0] = new(new[] { 16384, 32768, 0 }, 1);
Require(LegacyAcousticCostEnsemble.Combine(slots, 1).SequenceEqual(new[] { 5678, 0, 85173 }), "Single family changed exact fixed-log channel.");
slots[1] = new(new[] { 8192, 16384, 1 }, 1);
Require(LegacyAcousticCostEnsemble.Combine(slots, 1).SequenceEqual(new[] { 8517, 2839, 85173 }), "Two-weight integer shift normalization changed.");
Require(LegacyAcousticCostEnsemble.Combine(slots, 2).SequenceEqual(new[] { 17034, 5678, 170346 }), "Multiplier leaked into normalization denominator.");
Require(LegacyAcousticCostEnsemble.Combine(slots, 1, new byte[] { 1, 0, 1 })[1] == 0x4000000, "Excluded class retained cost.");
slots[0] = new(new[] { 0, 32768 }, 0); slots[1] = null;
Require(LegacyAcousticCostEnsemble.Combine(slots, 1).All(c => c == 0x4000000), "Zero-weight profile must return impossible costs.");
slots[0] = new(new[] { 0, 1 }, 1);
Require(LegacyAcousticCostEnsemble.Combine(slots, 1000).All(c => c == 0x4000000), "Native upper cost cap absent.");
foreach (float multiplier in new[] { 0f, -1f, float.NaN, float.PositiveInfinity })
    MustReject(() => LegacyAcousticCostEnsemble.Combine(slots, multiplier));
MustReject(() => LegacyAcousticCostEnsemble.Combine(new LegacyAcousticCostFamily?[6], 1));
MustReject(() => LegacyAcousticCostEnsemble.Combine(new LegacyAcousticCostFamily?[7], 1));
slots[1] = new(new[] { 3 }, 1); MustReject(() => LegacyAcousticCostEnsemble.Combine(slots, 1)); slots[1] = null;
slots[0] = new(new[] { 32769 }, 1); MustReject(() => LegacyAcousticCostEnsemble.Combine(slots, 1));
slots[0] = new(new[] { 0 }, -1); MustReject(() => LegacyAcousticCostEnsemble.Combine(slots, 1));
slots[0] = new(new[] { 0 }, float.MaxValue); MustReject(() => LegacyAcousticCostEnsemble.Combine(slots, 1));
slots[0] = new(new[] { 0 }, 1);
using (var source = new CancellationTokenSource())
{
    source.Cancel(); try { LegacyAcousticCostEnsemble.Combine(slots, 1, cancellationToken: source.Token); throw new InvalidOperationException("Cancellation ignored."); }
    catch (OperationCanceledException) { }
}
Console.WriteLine("PASS explicit weights/multiplier, exact1 and2 normalization branches, mask, zero sum, cost cap, bounds, overflow and cancellation.");
if (args.Length == 0) return;
if (args.Length != 1) throw new ArgumentException("Usage: LegacyCostEnsemble.Probe [independent-controls.json]");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0])); int checkedCases = 0;
foreach (var c in controls.RootElement.GetProperty("cases").EnumerateArray())
{
    var actualSlots = c.GetProperty("slots").EnumerateArray().Select(s => s.ValueKind == JsonValueKind.Null ? null :
        new LegacyAcousticCostFamily(s.GetProperty("probabilities").EnumerateArray().Select(v => v.GetInt32()).ToArray(), s.GetProperty("weight").GetSingle())).ToArray();
    byte[] mask = c.GetProperty("mask").EnumerateArray().Select(v => v.GetByte()).ToArray();
    int[] actual = LegacyAcousticCostEnsemble.Combine(actualSlots, c.GetProperty("multiplier").GetSingle(), mask);
    Require(actual.SequenceEqual(c.GetProperty("expected").EnumerateArray().Select(v => v.GetInt32())), $"Independent ensemble reference differs case{checkedCases}.");
    checkedCases++;
}
Require(checkedCases > 0, "Empty independent controls."); Console.WriteLine($"PASS {checkedCases} independent weighted integer cost references.");
static void Require(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); }
static void MustReject(Action action)
{
    try { action(); throw new InvalidOperationException("Invalid inputs accepted."); }
    catch (ArgumentException) { }
    catch (OverflowException) { }
}
