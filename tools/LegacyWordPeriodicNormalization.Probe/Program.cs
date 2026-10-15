using System.Text.Json;
using InkRecognition;

if (args.Length != 1) throw new ArgumentException("controls.json required");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int count = 0, bytes = 0, vectorCells = 0;
foreach (var test in controls.RootElement.EnumerateArray())
{
    byte[] input = Convert.FromHexString(test.GetProperty("rows").GetString()!);
    byte[] original = input.ToArray();
    int delta = test.GetProperty("delta").GetInt32();
    var actual = LegacyNativeWordPeriodicCostNormalization.ApplySourceRows(input, test.GetProperty("end").GetInt32(), delta);
    Require(actual.Span.SequenceEqual(Convert.FromHexString(test.GetProperty("expected").GetString()!)) && input.AsSpan().SequenceEqual(original),
        "Whole88 normalization changed cost14/outsideSourceEnd/residue or missed signed wrap.");
    int[] vector = test.GetProperty("vector").EnumerateArray().Select(value => value.GetInt32()).ToArray();
    var updated = LegacyNativeWordPeriodicCostNormalization.AddCumulative(vector, test.GetProperty("current").GetInt32(),
        test.GetProperty("backward").GetInt32(), delta);
    Require(updated.Span.SequenceEqual(test.GetProperty("updated").EnumerateArray().Select(value => value.GetInt32()).ToArray()),
        "Inclusive cumulative2520 range or signed wrap differs.");
    count++; bytes += input.Length; vectorCells += vector.Length;
}
int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or OperationCanceledException)
    { guards++; return; }
    throw new InvalidDataException("Invalid periodic normalization extent accepted.");
}
Reject(() => LegacyNativeWordPeriodicCostNormalization.ApplySourceRows(new byte[88], 2, 1));
Reject(() => LegacyNativeWordPeriodicCostNormalization.ApplySourceRows(new byte[88], 1, 0));
Reject(() => LegacyNativeWordPeriodicCostNormalization.ApplySourceRows(new byte[88], 1, 0x4000000));
Reject(() => LegacyNativeWordPeriodicCostNormalization.AddCumulative([0], -1, 0, 1));
Reject(() => LegacyNativeWordPeriodicCostNormalization.AddCumulative([0], 0, -1, 1));
Reject(() => LegacyNativeWordPeriodicCostNormalization.ApplySourceRows(new byte[88], 1, 1, new(true)));
Reject(() => LegacyNativeWordPeriodicCostNormalization.AddCumulative([0], 0, 0, 1, new(true)));
Console.WriteLine($"PASS {count} independent periodic controls / {bytes} whole88 bytes / {vectorCells} cumulative cells +{guards} guards; outside-source/cost14 preserved, signed wraps and inclusive backward bound.");

static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
