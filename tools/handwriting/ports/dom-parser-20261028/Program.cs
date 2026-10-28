using System.Text.Json;
using DomParserPort;

using var document = JsonDocument.Parse(File.ReadAllText(args[0]));
int checks = 0;
foreach (var test in document.RootElement.GetProperty("parse").EnumerateArray())
{
    long value = 12345;
    int end = DefaultStringService.ParseDecimal(Convert.FromHexString(test.GetProperty("hex").GetString()!), test.GetProperty("flags").GetByte(), ref value);
    if (end != test.GetProperty("end").GetInt32() || value != test.GetProperty("value").GetInt64()) throw new Exception($"Parser case {checks}");
    checks++;
}
foreach (var test in document.RootElement.GetProperty("format").EnumerateArray())
{
    byte[] output = Enumerable.Repeat((byte)0xcc, test.GetProperty("capacity").GetInt32()).ToArray();
    int length = test.GetProperty("kind").GetString() == "bool" ? DefaultStringService.FormatBoolean(test.GetProperty("boolean").GetBoolean(), output) : DefaultStringService.FormatSigned(test.GetProperty("value").GetInt64(), output);
    if (length != test.GetProperty("length").GetInt32() || !output.AsSpan().SequenceEqual(Convert.FromHexString(test.GetProperty("hex").GetString()!))) throw new Exception($"Formatter case {checks}");
    checks++;
}
Console.WriteLine($"Passed {checks} independent reference fixtures.");
