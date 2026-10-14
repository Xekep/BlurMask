using System.Text;
using System.Text.Json;
using InkRecognition;

if (args.Length != 2) throw new ArgumentException("controls.json acoustic.res required");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int prefixes = 0, resources = 0;
foreach (var row in controls.RootElement.GetProperty("prefix").EnumerateArray())
{
    byte[] name = Convert.FromBase64String(row.GetProperty("name").GetString()!);
    byte[] prefix = Convert.FromBase64String(row.GetProperty("prefix").GetString()!);
    Require(LegacyAcousticSourceIdentifier.HasBytePrefix(name, prefix) == row.GetProperty("expected").GetBoolean(), "Independent native byte-prefix result differs.");
    prefixes++;
}
foreach (var row in controls.RootElement.GetProperty("resource").EnumerateArray())
{
    byte[] bytes = Convert.FromBase64String(row.GetProperty("resource").GetString()!);
    byte[] before = bytes.ToArray();
    Require(LegacyAcousticSourceIdentifier.Read247V10d(bytes) == row.GetProperty("identifier").GetString(), "Independent source133 identifier differs.");
    Require(bytes.AsSpan().SequenceEqual(before), "Identifier reader mutated its input.");
    resources++;
}
byte[] acoustic = File.ReadAllBytes(args[1]);
Require(LegacyAcousticSourceIdentifier.Read247V10d(acoustic) == "ru_RU-ak-cur.lite", "Actual source133 resource identifier differs.");
Require(!LegacyAcousticSourceIdentifier.HasBytePrefix(Encoding.UTF8.GetBytes(LegacyAcousticSourceIdentifier.Read247V10d(acoustic)), "zh_"u8), "Actual RU unexpectedly enables the Chinese confidence intersection.");
int guards = 0;
foreach (int offset in new[] { 0, 4, 8, 12, 16, 18, 20, 80, 93, 97 })
{
    var modified = acoustic.ToArray(); modified[offset] ^= 1;
    Reject(() => LegacyAcousticSourceIdentifier.Read247V10d(modified)); guards++;
}
Reject(() => LegacyAcousticSourceIdentifier.Read247V10d(acoustic.AsSpan(0, 98))); guards++;
var unterminated = acoustic.ToArray();
for (int i = 0; i < 56; i++) unterminated[24 + i] = (byte)(0x41 ^ new byte[] { 0x14, 0x12, 0x19, 0x77 }[i & 3]);
Reject(() => LegacyAcousticSourceIdentifier.Read247V10d(unterminated)); guards++;
var invalidUtf8 = acoustic.ToArray(); invalidUtf8[24] = 0xff ^ 0x14; invalidUtf8[25] = 0x12;
Reject(() => LegacyAcousticSourceIdentifier.Read247V10d(invalidUtf8)); guards++;
Reject(() => LegacyAcousticSourceIdentifier.HasBytePrefix(new byte[65536], [])); guards++;
Reject(() => LegacyAcousticSourceIdentifier.HasBytePrefix([], new byte[65536])); guards++;
Require(!LegacyAcousticSourceIdentifier.HasBytePrefix("foozh_CN"u8, "zh_"u8), "Substring was incorrectly treated as prefix.");
Require(LegacyAcousticSourceIdentifier.HasBytePrefix("zh_CN"u8, "zh_"u8), "Actual ordinal prefix was rejected.");
Console.WriteLine($"PASS {prefixes} independent byte-prefix cases/{resources} encoded source133 identifiers/{guards} malformed guards; actual RU acoustic identifier -> no zh_ confidence-intersection branch. Prefix data only; not resource factory or complete main controller.");
static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
static void Reject(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or InvalidDataException) { return; }
    throw new InvalidDataException("Expected malformed identifier rejection.");
}
