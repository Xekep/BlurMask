using System.Text.Json;
using InkRecognition;

void Require(bool condition, string message) { if (!condition) throw new InvalidDataException(message); }
void Reject(Action action) { try { action(); } catch (Exception e) when (e is ArgumentException or InvalidDataException or OperationCanceledException) { return; } throw new InvalidDataException("Malformed/canceled input accepted."); }

byte[] a = [1, 2, 3, 4, 5], b = [0, 17, 0, 0, 0], lower = [0, 3, 4, 0, 0], upper = [0, 3, 0, 2, 0];
Require(LegacyAcousticSymbolRangeFilter.Apply(a, b, lower, upper, 3) == 2 && a.AsSpan().SequenceEqual(new byte[] { 1, 2, 0, 0, 5 }) && b[1] == 17, "Zero/equality/neighbor bounds differ.");
Require(LegacyAcousticSymbolRangeFilter.Apply(a, b, [], upper, 0) == 0 && a[0] == 1, "Missing lower table must disable filtering.");
Require(LegacyAcousticSymbolRangeFilter.Apply(a, b, lower, [], 255) == 0 && a[0] == 1, "Missing upper table must disable filtering.");
Reject(() => LegacyAcousticSymbolRangeFilter.Apply(a, new byte[4], lower, upper, 3));
Reject(() => LegacyAcousticSymbolRangeFilter.Apply(a, b, [1], upper, 3));
Reject(() => LegacyAcousticSymbolRangeFilter.Apply(a, b, a, upper, 3));
Reject(() => LegacyAcousticSymbolRangeFilter.DecodePackedFields([], 0, 1));
Reject(() => LegacyAcousticSymbolRangeFilter.DecodePackedFields([], 9, 1));
Reject(() => LegacyAcousticSymbolRangeFilter.DecodePackedFields([], 8, 1));
using var canceled = new CancellationTokenSource(); canceled.Cancel();
Reject(() => LegacyAcousticSymbolRangeFilter.Apply(a, b, lower, upper, 3, canceled.Token));

int checkedCases = 0;
if (args is [var path])
{
    using var json = JsonDocument.Parse(File.ReadAllBytes(path));
    foreach (var test in json.RootElement.GetProperty("cases").EnumerateArray())
    {
        byte[] Bytes(string name) => test.GetProperty(name).EnumerateArray().Select(v => v.GetByte()).ToArray();
        int width = test.GetProperty("width").GetInt32(), count = test.GetProperty("count").GetInt32();
        var lo = LegacyAcousticSymbolRangeFilter.DecodePackedFields(Convert.FromHexString(test.GetProperty("lowerPacked").GetString()!), width, count);
        var hi = LegacyAcousticSymbolRangeFilter.DecodePackedFields(Convert.FromHexString(test.GetProperty("upperPacked").GetString()!), width, count);
        Require(lo.AsSpan().SequenceEqual(Bytes("lower")) && hi.AsSpan().SequenceEqual(Bytes("upper")), "Independent packed fields differ.");
        var primary = Bytes("primary"); var secondary = Bytes("secondary");
        int removed = LegacyAcousticSymbolRangeFilter.Apply(primary, secondary, lo, hi, test.GetProperty("current").GetByte());
        Require(primary.AsSpan().SequenceEqual(Bytes("expectedPrimary")) && secondary.AsSpan().SequenceEqual(Bytes("expectedSecondary")) && removed == test.GetProperty("cleared").GetInt32(), "Independent range/mask outputs differ.");
        checkedCases++;
    }
}
else if (args.Length != 0) throw new ArgumentException("Usage: LegacySymbolRange.Probe [own-reference-controls.json]");
Console.WriteLine($"Native category-range filter: zero/equality/disabled/bounds/alias/cancellation guards PASS; {checkedCases} independent packed/category controls PASS.");
