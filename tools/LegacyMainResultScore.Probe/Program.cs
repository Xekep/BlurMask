using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 2) throw new ArgumentException("controls.json capture.json");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
using var output = File.Create(args[1]);
using var writer = new Utf8JsonWriter(output);
writer.WriteStartObject();
writer.WriteStartArray("scalar");
int scalarCount = 0, normalizationCount = 0, disagreements = 0;
foreach (var control in controls.RootElement.GetProperty("scalar").EnumerateArray())
{
    var numbers = control.GetProperty("input").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    var input = new LegacyMainResultScoreInput(numbers[0], numbers[1], numbers[2], numbers[3], numbers[4]);
    var expected = control.GetProperty("expected");
    var result = LegacyMainResultScore.Evaluate(input);
    Require(result.IntegerDifference == expected[0].GetInt32(), "Main signed32 subtraction did not wrap exactly.");
    Check(result.NormalizedPathCost10, expected[1].GetUInt32(), 0);
    Check(result.NormalizedRowCost14, expected[2].GetUInt32(), 0);
    Check(result.LogMass, expected[3].GetUInt32(), 0);
    Check(result.Mass08, expected[4].GetUInt32(), 2);
    if (control.GetProperty("differentFloatBeforeSubtract").GetBoolean())
    {
        float oldOrder = ((float)input.TotalCost - (float)input.ReferenceCost) / unchecked(input.SourceNodeCount << 13);
        Require(Bits(oldOrder) != Bits(result.NormalizedPathCost10), "Control failed to distinguish the225FB0 cast order.");
        disagreements++;
    }
    byte[] original = Convert.FromHexString(control.GetProperty("original").GetString()!);
    byte[] untouched = original.ToArray();
    byte[] row = LegacyMainResultScore.WriteCostChannels(original, input);
    CheckStorage(row, Convert.FromHexString(control.GetProperty("row").GetString()!), 1, true);
    Require(original.AsSpan().SequenceEqual(untouched), "Scalar publication changed its input allocation.");
    writer.WriteStartObject(); writer.WriteNumber("difference", result.IntegerDifference);
    WriteBits("path", result.NormalizedPathCost10); WriteBits("row14", result.NormalizedRowCost14);
    WriteBits("log", result.LogMass); WriteBits("mass", result.Mass08);
    writer.WriteString("row", Convert.ToHexString(row)); writer.WriteEndObject();
    scalarCount++;
}
writer.WriteEndArray(); writer.WriteStartArray("normalization");
foreach (var control in controls.RootElement.GetProperty("normalization").EnumerateArray())
{
    byte[] original = Convert.FromHexString(control.GetProperty("original").GetString()!);
    byte[] untouched = original.ToArray();
    int count = control.GetProperty("count").GetInt32();
    float oldTop = Value(control.GetProperty("oldTopBits").GetUInt32());
    var result = LegacyMainResultScorePublication.Normalize(original, count, oldTop);
    CheckStorage(result.CopyStorage(), Convert.FromHexString(control.GetProperty("storage").GetString()!), count, false);
    Check(result.Top80, control.GetProperty("topBits").GetUInt32(), 0);
    Require(result.ActiveCount == count, "Normalizer lost the explicit active count.");
    if (count == 0)
    {
        Require(result.RawFirstMass08 is null, "Empty storage invented a first mass.");
        Require(Bits(result.Top80) == Bits(oldTop), "Empty storage changed an untouched TOP80 bit pattern.");
    }
    else Require(Bits(result.RawFirstMass08!.Value) == control.GetProperty("rawFirstBits").GetUInt32(),
        "Raw first mass, including its NaN payload, was changed by normalization.");
    Require(original.AsSpan().SequenceEqual(untouched), "Normalization changed its caller storage.");
    byte[] leaked = result.CopyStorage();
    if (leaked.Length != 0)
    {
        leaked[0] ^= 255;
        Require(result.CopyStorage()[0] != leaked[0], "Normalizer leaked mutable internal row storage.");
    }
    writer.WriteStartObject(); writer.WriteString("storage", Convert.ToHexString(result.CopyStorage()));
    WriteBits("top", result.Top80);
    if (result.RawFirstMass08 is float first) WriteBits("first", first); else writer.WriteNull("first");
    writer.WriteEndObject(); normalizationCount++;
}
writer.WriteEndArray();
Require(disagreements == controls.RootElement.GetProperty("castOrderDisagreements").GetInt32() && disagreements > 0,
    "No meaningful cast-order controls were exercised.");

int guards = 0;
Reject(() => LegacyMainResultScore.Evaluate(new(1, 0, 0, 0, 0x135)));
Reject(() => LegacyMainResultScore.Evaluate(new(1, 0, -1, 0, 0x135)));
Reject(() => LegacyMainResultScore.Evaluate(new(1, 0, 1, 0, 0x135), new(true)));
Reject(() => LegacyMainResultScore.WriteCostChannels(new byte[39], new(1, 0, 1, 0, 0x135)));
Reject(() => LegacyMainResultScore.WriteCostChannels(new byte[40], new(1, 0, 1, 0, 0x135), new(true)));
Reject(() => LegacyMainResultScorePublication.Normalize(new byte[41], 0, -1));
Reject(() => LegacyMainResultScorePublication.Normalize(new byte[40], -1, -1));
Reject(() => LegacyMainResultScorePublication.Normalize(new byte[40], 2, -1));
Reject(() => LegacyMainResultScorePublication.Normalize(new byte[4097 * 40], 0, -1));
Reject(() => LegacyMainResultScorePublication.Normalize(new byte[40], 1, -1, new(true)));
writer.WriteNumber("guards", guards); writer.WriteNumber("castOrderDisagreements", disagreements);
writer.WriteEndObject(); writer.Flush();
Console.WriteLine($"PASS {scalarCount} main signed32/float scalar rows, {normalizationCount} full-allocation normalizations, {disagreements} cast-order distinctions and {guards} guards; explicit inputs, no owning result/acceptance claim.");

void WriteBits(string name, float value) => writer.WriteNumber(name, Bits(value));
static uint Bits(float value) => BitConverter.SingleToUInt32Bits(value);
static float Value(uint bits) => BitConverter.UInt32BitsToSingle(bits);
static void Require(bool value, string message)
{ if (!value) throw new InvalidDataException(message); }
static void Check(float actual, uint expectedBits, uint tolerance)
{
    float expected = Value(expectedBits);
    if (float.IsNaN(expected)) { Require(float.IsNaN(actual), "Native arithmetic NaN was lost."); return; }
    uint observed = Bits(actual);
    if (observed == expectedBits) return;
    Require(!float.IsNaN(actual) && !float.IsInfinity(actual) && !float.IsInfinity(expected), "Finite/infinite result classification changed.");
    //ULP allowance is only supplied for platform exp/expf. Exact operations,
    //MINSS selections, zero signs and preserved allocator bytes use tolerance0.
    Require(((observed ^ expectedBits) & 0x80000000) == 0 && Math.Abs((long)observed - expectedBits) <= tolerance,
        $"Float mismatch: expected{expectedBits:X8}, observed{observed:X8}, tolerance{tolerance}.");
}
static void CheckStorage(byte[] actual, byte[] expected, int count, bool scalar)
{
    Require(actual.Length == expected.Length, "Full row allocation extent changed.");
    for (int at = 0; at < actual.Length;)
    {
        int ordinal = at / 40, field = at % 40;
        bool numeric = ordinal < count && (scalar ? field is 8 or 16 or 20 : field is 8 or 12);
        if (numeric)
        {
            uint expectedBits = BinaryPrimitives.ReadUInt32LittleEndian(expected.AsSpan(at));
            float observed = Value(BinaryPrimitives.ReadUInt32LittleEndian(actual.AsSpan(at)));
            uint tolerance = scalar ? field == 8 ? 2U : 0U : field == 12 ? 2U : 0U;
            Check(observed, expectedBits, tolerance); at += 4;
        }
        else { Require(actual[at] == expected[at], $"Nonchannel byte changed at{at}."); at++; }
    }
}
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or OperationCanceledException) { guards++; return; }
    throw new InvalidDataException("Malformed or canceled scalar publication accepted.");
}
