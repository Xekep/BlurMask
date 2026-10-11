using System.Text.Json;
using InkRecognition;

Require(LegacyAcousticFixedLog.LogProbability(0) == -0x4000000 && LegacyAcousticFixedLog.ProbabilityCost(0) == 85173,
    "Raw zero sentinel and scorer clamp-to-one became conflated.");
Require(LegacyAcousticFixedLog.LogProbability(32768) == 0 && LegacyAcousticFixedLog.LogProbability(16384) == -5678 &&
    LegacyAcousticFixedLog.LogProbability(8192) == -11356, "Exact powers-of-two costs changed.");
Require(LegacyAcousticFixedLog.LogProbability(24) == LegacyAcousticFixedLog.LogProbability(16) &&
    LegacyAcousticFixedLog.LogProbability(25) == LegacyAcousticFixedLog.LogProbability(32),
    "Remainder-eight tie must use lower positive table index.");
Require(LegacyAcousticFixedLog.LogProbability(15) != LegacyAcousticFixedLog.LogProbability(16), "Small-probability branch discarded.");
int prior = -LegacyAcousticFixedLog.ImpossibleCost;
for (int p = 0; p <= 32768; p++)
{
    int value = LegacyAcousticFixedLog.LogProbability(p);
    Require(value >= prior && value <= 0, "Log probability must be bounded and monotone."); prior = value;
    Require(LegacyAcousticFixedLog.ProbabilityCost(p) >= 0, "Positive cost channel became negative.");
}
foreach (int p in new[] { -1, int.MinValue, 32769, int.MaxValue })
{
    try { LegacyAcousticFixedLog.LogProbability(p); throw new InvalidOperationException("Out-of-domain log accepted."); }
    catch (ArgumentOutOfRangeException) { }
    try { LegacyAcousticFixedLog.ProbabilityCost(p); throw new InvalidOperationException("Out-of-domain cost accepted."); }
    catch (ArgumentOutOfRangeException) { }
}
Console.WriteLine("PASS analytic powers, small-probability branch, tie-down rounding, zero sentinel vs score clamp, all-domain monotonicity and bounds.");
if (args.Length == 0) return;
if (args.Length != 1) throw new ArgumentException("Usage: LegacyFixedLog.Probe [independent-controls.json]");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int expectedP = 0;
foreach (var c in controls.RootElement.GetProperty("cases").EnumerateArray())
{
    int p = c.GetProperty("probability").GetInt32(); Require(p == expectedP++, "References must cover whole bounded domain in order.");
    Require(LegacyAcousticFixedLog.LogProbability(p) == c.GetProperty("log").GetInt32(), $"Independent log reference differs at {p}.");
    Require(LegacyAcousticFixedLog.ProbabilityCost(p) == c.GetProperty("cost").GetInt32(), $"Independent cost reference differs at {p}.");
}
Require(expectedP == 32769, "Expected whole32769 probability domain.");
Console.WriteLine("PASS all32769 independent probability log/cost references.");
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
