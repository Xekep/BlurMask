using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using InkRecognition.Math;

if (args.Length != 2) throw new ArgumentException("DEFAULT_SERVICE_CONTROLS NUMERIC_READER_CONTROLS");
int controls = 0;
using (var data = JsonDocument.Parse(File.ReadAllBytes(args[0])))
{
    foreach (var test in data.RootElement.GetProperty("parse").EnumerateArray())
    {
        long value = 12345;
        int end = Math2DomStringService.ParseDecimal(Convert.FromHexString(test.GetProperty("hex").GetString()!), test.GetProperty("flags").GetByte(), ref value);
        Require(end == test.GetProperty("end").GetInt32() && value == test.GetProperty("value").GetInt64(), "decimal parser");
    }
    foreach (var test in data.RootElement.GetProperty("format").EnumerateArray())
    {
        byte[] buffer = Enumerable.Repeat((byte)0xcc, test.GetProperty("capacity").GetInt32()).ToArray();
        int length = test.GetProperty("kind").GetString() == "bool"
            ? Math2DomStringService.FormatBoolean(test.GetProperty("boolean").GetBoolean(), buffer)
            : Math2DomStringService.FormatSigned(test.GetProperty("value").GetInt64(), buffer);
        Require(length == test.GetProperty("length").GetInt32() && buffer.AsSpan().SequenceEqual(Convert.FromHexString(test.GetProperty("hex").GetString()!)), "scalar formatter");
    }
}
using (var data = JsonDocument.Parse(File.ReadAllBytes(args[1])))
{
    foreach (var test in data.RootElement.GetProperty("numbers").EnumerateArray())
    {
        var heap = test.GetProperty("heap");
        var value = new Math2DomValueView(test.GetProperty("tag").GetByte(), Convert.FromHexString(test.GetProperty("storage").GetString()!),
            heap.ValueKind == JsonValueKind.Null ? default : Convert.FromHexString(heap.GetString()!), heap.ValueKind != JsonValueKind.Null);
        int logs = 0;
        long number = Math2DomValueConversion.Number(value, 12345, code => { if (code != 0x02000000) throw new Exception("error code"); logs++; });
        Require(number == test.GetProperty("value").GetInt64() && logs == (test.GetProperty("logged").GetBoolean() ? 1 : 0), "numeric attribute");
    }
    byte[] output = new byte[64];
    foreach (var test in data.RootElement.GetProperty("layouts").EnumerateArray())
    {
        output.AsSpan().Fill(0xcc);
        int count = Math2DomFloatingFormatter.Compose(Encoding.ASCII.GetBytes(test.GetProperty("digits").GetString()!),
            test.GetProperty("dp").GetInt32(), test.GetProperty("negative").GetBoolean(), test.GetProperty("precision").GetInt32(), output);
        Require(Encoding.ASCII.GetString(output.AsSpan(0, count)) == test.GetProperty("text").GetString(), "native floating layout tuple");
        Require(output[count] == 0 && output.AsSpan(count + 1).IndexOfAnyExcept((byte)0xcc) < 0, "floating layout extent");
    }
    foreach (var test in data.RootElement.GetProperty("specials").EnumerateArray())
    {
        output.AsSpan().Fill(0xcc);
        var result = Math2DomValueConversion.Text(new(test.GetProperty("tag").GetByte(), Convert.FromHexString(test.GetProperty("storage").GetString()!)), output);
        Require(Encoding.ASCII.GetString(result.BoundedBytes) == test.GetProperty("text").GetString(), "actual DOM zero/special conversion");
        Require(output[result.Length] == 0 && output.AsSpan(result.Length + 1).IndexOfAnyExcept((byte)0xcc) < 0, "special extent");
    }
}

byte[] scratch = Enumerable.Repeat((byte)0xcc, 64).ToArray();
void Require(bool condition, string message)
{ if (!condition) throw new InvalidDataException(message + " at " + controls); controls++; }
void Text(byte tag, byte[] storage, string expected)
{
    scratch.AsSpan().Fill(0xcc);
    var result = Math2DomValueConversion.Text(new(tag, storage), scratch);
    Require(result.HasPointer && Encoding.ASCII.GetString(result.BoundedBytes) == expected && result.Storage.Overlaps(scratch), "numeric text storage");
    Require(scratch[result.Length] == 0 && scratch.AsSpan(result.Length + 1).IndexOfAnyExcept((byte)0xcc) < 0, "scratch extent");
}
Text(4, BitConverter.GetBytes(long.MinValue), "-9223372036854775808");
Text(4, BitConverter.GetBytes(long.MaxValue), "9223372036854775807");
Text(7, BitConverter.GetBytes(int.MinValue), "-2147483648");
Text(8, BitConverter.GetBytes(uint.MaxValue), "4294967295");
Text(9, BitConverter.GetBytes(ulong.MaxValue), "18446744073709551615");
Text(6, [0], "false"); Text(6, [255], "true");
byte[] inline = "123456789012345\0"u8.ToArray();
var inlineText = Math2DomValueConversion.Text(new(2, inline), []);
Require(inlineText.HasPointer && inlineText.Length == 15 && inlineText.Storage.Overlaps(inline), "zero inline length means15");
inline[15] = 3;
Require(Encoding.ASCII.GetString(Math2DomValueConversion.Text(new(2, inline), []).BoundedBytes) == "123", "explicit inline length");
inline[15] = 255;
Require(Math2DomValueConversion.Text(new(2, inline), []).Length == -1, "signed inline length");
byte[] lane = new byte[16], heapBytes = [0xff, 0, 65];
BinaryPrimitives.WriteUInt32LittleEndian(lane.AsSpan(8), 3);
var heapText = Math2DomValueConversion.Text(new(1, lane, heapBytes, true), []);
Require(heapText.Storage.Overlaps(heapBytes) && heapText.BoundedBytes.SequenceEqual(heapBytes), "heap length preservesNUL/invalidUTF8");
Require(!Math2DomValueConversion.Text(new(1, lane), []).HasPointer, "null heap pointer");
Require(Math2DomValueConversion.Text(new(3, lane), []).HasPointer, "empty value keeps pointer");
Require(!Math2DomValueConversion.Text(new(0, lane), []).HasPointer, "unsupported value has null pointer");
var formatter = new Floating();
foreach (var (tag, payload, expected, precision) in new (byte, byte[], double, int)[]
    { (5, BitConverter.GetBytes(-2.5), -2.5, 17), (10, BitConverter.GetBytes(.1f), (double).1f, 9) })
{
    formatter.Calls = 0;
    var result = Math2DomValueConversion.Text(new(tag, payload), scratch, formatter);
    Require(formatter.Calls == 1 && formatter.Value == expected && formatter.Precision == precision && result.Length == 2 && result.BoundedBytes.SequenceEqual("fp"u8), "floating dispatch, not dtoa equivalence");
}
try { Math2DomValueConversion.Text(new(5, BitConverter.GetBytes(1.0)), scratch); throw new Exception("missing dtoa accepted"); }
catch (NotSupportedException) { controls++; }
try { Math2DomValueConversion.Text(new(4, BitConverter.GetBytes(1L)), new byte[49]); throw new Exception("short scratch accepted"); }
catch (ArgumentException) { controls++; }
try { Math2DomValueConversion.RegistrationTag(long.MaxValue); throw new Exception("registration sentinel accepted"); }
catch (NotSupportedException) { controls++; }
try { Math2DomValueConversion.CollectedTag(-1); throw new Exception("collection sentinel accepted"); }
catch (NotSupportedException) { controls++; }
Require(Math2DomValueConversion.RegistrationTag(-2) == -2 && Math2DomValueConversion.CollectedTag(0) == 0, "typed real caller scalar path");
foreach (var (digits, dp, precision) in new (string, int, int)[] {
    ("", 1, 17), ("1234567890", 1, 9), ("1", 1, 16), ("1", -324, 17), ("1", 310, 17),
    ("00", 1, 17), ("0", 0, 17), ("1x", 1, 17), ("1\0", 1, 17), ("nan", 9999, 17), ("NaN", 1, 17), ("1", 9999, 17) })
{
    scratch.AsSpan().Fill(0xcc);
    try { Math2DomFloatingFormatter.Compose(Encoding.ASCII.GetBytes(digits), dp, false, precision, scratch); throw new Exception("bad tuple accepted"); }
    catch (NotSupportedException) { Require(scratch.AsSpan().IndexOfAnyExcept((byte)0xcc) < 0, "bad tuple must not write"); }
}
foreach (int capacity in new[] { 0, 1, 24, 49 })
{
    try { Math2DomFloatingFormatter.Compose("1"u8, 1, false, 17, new byte[capacity]); throw new Exception("short floating scratch accepted"); }
    catch (ArgumentException) { controls++; }
}
scratch.AsSpan().Fill(0xcc); scratch[0] = (byte)'1';
try { Math2DomFloatingFormatter.Compose(scratch.AsSpan(0, 1), 1, false, 17, scratch); throw new Exception("alias accepted"); }
catch (ArgumentException) { controls++; }
foreach (double value in new[] { double.Epsilon, -double.Epsilon, 1d, double.MaxValue })
{
    scratch.AsSpan().Fill(0xcc);
    try { Math2DomFloatingFormatter.SpecialValuesOnly.Format(value, scratch, 17, -1); throw new Exception("unproven finite generator admitted"); }
    catch (NotSupportedException) { Require(scratch.AsSpan().IndexOfAnyExcept((byte)0xcc) < 0, "finite refusal preserves output"); }
}
var tupleGenerator = new DigitTuple();
var layoutFormatter = new Math2DomFloatingFormatter(tupleGenerator);
var composed = Math2DomValueConversion.Text(new(5, BitConverter.GetBytes(1d)), scratch, layoutFormatter);
Require(Encoding.ASCII.GetString(composed.BoundedBytes) == "1234567" && tupleGenerator.Calls == 1, "injected tuple composition only, not dtoa equivalence");
tupleGenerator.Count = 18;
try { layoutFormatter.Format(1d, scratch, 17, -1); throw new Exception("oversized helper result accepted"); }
catch (NotSupportedException) { controls++; }
tupleGenerator.Count = 7;
tupleGenerator.Negative = true;
try { layoutFormatter.Format(1d, scratch, 17, -1); throw new Exception("changed helper sign accepted"); }
catch (NotSupportedException) { controls++; }
tupleGenerator.Negative = false; tupleGenerator.Position = 9999;
try { layoutFormatter.Format(1d, scratch, 17, -1); throw new Exception("changed helper classification accepted"); }
catch (NotSupportedException) { controls++; }
try { layoutFormatter.Format(1d, scratch, 17, 0); throw new Exception("fractional-limit branch admitted"); }
catch (NotSupportedException) { controls++; }
// The compositor is allocation-free after singleton/runtime warmup. This is
// a local property, not a throughput or whole-recognizer performance claim.
Math2DomFloatingFormatter.Compose("12345678901234567"u8, -323, true, 17, scratch);
long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
for (int i = 0; i < 10000; i++)
    Math2DomFloatingFormatter.Compose("12345678901234567"u8, -323, true, 17, scratch);
Require(GC.GetAllocatedBytesForCurrentThread() == allocatedBefore, "floating compositor allocation");
Console.WriteLine($"PASS {controls} string/numeric/DOM/layout checks. Nonzero finite digit generation and CB8 prior-result ownership remain unproven.");

sealed class DigitTuple : IMath2DomDigitGenerator
{
    internal int Calls, Count = 7;
    internal int Position = 7;
    internal bool Negative;
    public int Generate(double value, int precision, Span<byte> digits, out int decimalPosition, out bool negative)
    {
        if (value != 1d || precision != 17 || digits.Length != 17) throw new Exception("generator parameters");
        Calls++; "1234567"u8.CopyTo(digits); decimalPosition = Position; negative = Negative; return Count;
    }
}

sealed class Floating : IMath2DomFloatingFormatter
{
    internal int Calls, Precision;
    internal double Value;
    public int Format(double value, Span<byte> destination, int precision, int nativeParameter)
    {
        if (destination.Length != 50 || nativeParameter != -1) throw new Exception("native formatter parameters");
        Calls++; Value = value; Precision = precision; "fp\0"u8.CopyTo(destination); return 2;
    }
}
