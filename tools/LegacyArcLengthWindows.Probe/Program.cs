using InkRecognition;
using System.Text.Json;
if (args.Length > 1) throw new ArgumentException("Supply at most one own reference JSON.");
if (args.Length == 1)
{
    if (new FileInfo(args[0]).Length > 8 * 1024 * 1024) throw new ArgumentException("Control file exceeds 8 MiB.");
    using var document = JsonDocument.Parse(File.ReadAllBytes(args[0])); int count = 0;
    foreach (var item in document.RootElement.EnumerateArray())
    {
        var cumulative = item.GetProperty("cumulative").EnumerateArray().Select(p => p.GetInt32()).ToArray();
        var actual = LegacyArcLengthWindows.Partition(cumulative, item.GetProperty("count").GetInt32());
        var expected = item.GetProperty("windows");
        Require(actual.Count == expected.GetArrayLength(), "Window count mismatch.");
        for (int i = 0; i < actual.Count; i++)
            Require(actual[i].Span.SequenceEqual(expected[i].EnumerateArray().Select(p => p.GetByte()).ToArray()), $"Reference mismatch {count}:{i}.");
        Require(Enumerable.Range(0, actual.Count).SelectMany(i => actual[i].ToArray()).SequenceEqual(Enumerable.Range(0, cumulative.Length).Select(i => (byte)i)), "Points omitted or duplicated.");
        count++;
    }
    Require(count > 0, "Empty reference controls."); Console.WriteLine($"PASS {count} independent temporal window membership comparisons.");
}
var singleton = LegacyArcLengthWindows.Partition([0]);
Require(singleton[0].Span.SequenceEqual(new byte[] { 0 }) && singleton[15].IsEmpty, "Trailing empty windows must be explicit.");
var exact = LegacyArcLengthWindows.Partition(Enumerable.Range(0, 20).ToArray(), 2);
Require(exact[0].Length == 11 && exact[1].Length == 9, "Rounded strict boundary mismatch.");
var hugeJump = LegacyArcLengthWindows.Partition(Enumerable.Repeat(0, 16).Append(100).ToArray());
Require(hugeJump[0].Length == 16 && hugeJump[1].Span.SequenceEqual(new byte[] { 16 }) && hugeJump[2].IsEmpty,
    "A large jump must advance once, not fill guessed intermediate windows.");
var flat = LegacyArcLengthWindows.Partition(new int[32]);
Require(flat[0].Length == 32 && flat[1].IsEmpty, "Zero physical arc length mismatch.");
Throws<ArgumentException>(() => LegacyArcLengthWindows.Partition([]));
Throws<ArgumentException>(() => LegacyArcLengthWindows.Partition(new int[256]));
Throws<ArgumentException>(() => LegacyArcLengthWindows.Partition([1, 2]));
Throws<ArgumentException>(() => LegacyArcLengthWindows.Partition([0, 2, 1]));
Throws<ArgumentException>(() => LegacyArcLengthWindows.Partition([0, int.MaxValue]));
Throws<ArgumentOutOfRangeException>(() => LegacyArcLengthWindows.Partition([0], 0));
Throws<OperationCanceledException>(() => LegacyArcLengthWindows.Partition([0], 16, new CancellationToken(true)));
Console.WriteLine("PASS own temporal windows, rounded strict boundary, large jump, flat geometry, bounds and cancellation. Coordinate preparation and 313-channel recognition are not tested.");
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}."); }
