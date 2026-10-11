using InkRecognition;
using System.Text.Json;
if (args.Length != 1) throw new ArgumentException("Supply font-size controls.");
if (new FileInfo(args[0]).Length > 8 * 1024 * 1024) throw new ArgumentException("Controls exceed 8 MiB.");
using var doc = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int policyCount = 0, filterCount = 0, quantizeCount = 0, updateCount = 0;
foreach (var c in doc.RootElement.GetProperty("filters").EnumerateArray())
{
    int cp = c[0].GetInt32();
    Require(LegacyHandwritingFontSizePolicy.IsExcludedCodePoint(cp) == c[1].GetBoolean(), $"Excluded U+{cp:X}");
    Require(LegacyHandwritingFontSizePolicy.UsesCharacterXHeight(cp) == c[2].GetBoolean(), $"Character-height U+{cp:X}"); filterCount++;
}
foreach (var c in doc.RootElement.GetProperty("policy").EnumerateArray())
{
    var text = c.GetProperty("text").EnumerateArray().Select(s => new LegacyFontSizeTextSample(
        s[0].GetInt32(), s[1].GetInt32(), s[2].GetSingle(), s[3].GetSingle(), s[4].GetSingle(), s[5].GetUInt32())).ToArray();
    var math = c.GetProperty("math").EnumerateArray().Select(s => new LegacyFontSizeMathSample(s[0].GetUInt64(), s[1].GetSingle())).ToArray();
    Equal(LegacyHandwritingFontSizePolicy.MatchPrepared(c.GetProperty("style").GetSingle(), c.GetProperty("reference").GetSingle(),
        text, math, c.GetProperty("enabled").GetBoolean(), c.GetProperty("available").GetBoolean()), c.GetProperty("expected").GetSingle()); policyCount++;
}
foreach (var c in doc.RootElement.GetProperty("quantize").EnumerateArray())
{ Equal(LegacyHandwritingFontSizePolicy.QuantizePrepared(c[0].GetSingle(), c[1].GetSingle(), c[2].GetSingle()), c[3].GetSingle()); quantizeCount++; }
foreach (var c in doc.RootElement.GetProperty("updates").EnumerateArray())
{ Require(LegacyHandwritingFontSizePolicy.RequiresSizeUpdate(c[0].GetSingle(), c[1].GetSingle()) == c[2].GetBoolean(), "Update tolerance"); updateCount++; }
Throws<ArgumentException>(() => LegacyHandwritingFontSizePolicy.MatchPrepared(float.NaN, 1, []));
Throws<ArgumentException>(() => LegacyHandwritingFontSizePolicy.MatchPrepared(4, 1, [new(65, 0, 1, 0, 1)]));
Throws<InvalidDataException>(() => LegacyHandwritingFontSizePolicy.MatchPrepared(float.MaxValue, float.Epsilon, [new(65, 1, 1, 0, 1)]));
Throws<InvalidDataException>(() => LegacyHandwritingFontSizePolicy.QuantizePrepared(float.MaxValue, float.Epsilon, .1f));
Throws<ArgumentException>(() => LegacyHandwritingFontSizePolicy.RequiresSizeUpdate(1, float.PositiveInfinity));
Console.WriteLine($"PASS {policyCount} prepared handwriting font-size controls, {filterCount} Unicode branch controls, {quantizeCount} quantization controls, {updateCount} dirty-tolerance controls; finite/overflow rejection.");
static void Equal(float a, float b) => Require(BitConverter.SingleToInt32Bits(a) == BitConverter.SingleToInt32Bits(b), $"Size {a:R} vs {b:R}");
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}"); }
