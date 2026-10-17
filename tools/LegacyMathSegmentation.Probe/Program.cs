using System.Text.Json;
using InkRecognition.Math;

if (args.Length != 1) throw new ArgumentException("capture.json");
var rows = new List<(int Symbols, int Segments, float[] Input, float[] Output)>();
float[] edges = [-float.MaxValue, -1000f, -104f, -89f, -88f, -20f, -8.175f,
    -1f, -float.Epsilon, -0f, 0f, float.Epsilon, 1f, 8.175f, 20f, 88f, 104f, 1000f, float.MaxValue];
void Capture(int symbols, int segments, float[] input)
{
    var output = LegacyPortableMathSegmentation.ActivatePreparedSigmoid(input, symbols, segments);
    rows.Add((symbols, segments, input.ToArray(), output));
}
Capture(1, edges.Length, edges);
Capture(edges.Length, 1, edges);
Capture(3, 19, Enumerable.Range(0, 57).Select(i => edges[(i * 7) % edges.Length]).ToArray());
Capture(7, 257, Enumerable.Range(0, 7 * 257).Select(i => (i - 899) / 17f).ToArray());
int guards = 0;
void Refuse(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new InvalidDataException("A required segmentation refusal was lost.");
}
Refuse(() => LegacyPortableMathSegmentation.ActivatePreparedSigmoid([], 0, 1));
Refuse(() => LegacyPortableMathSegmentation.ActivatePreparedSigmoid([], 1, 0));
Refuse(() => LegacyPortableMathSegmentation.ActivatePreparedSigmoid([], 1001, 1));
Refuse(() => LegacyPortableMathSegmentation.ActivatePreparedSigmoid([], 1, 6001));
Refuse(() => LegacyPortableMathSegmentation.ActivatePreparedSigmoid([], 1000, 6000));
Refuse(() => LegacyPortableMathSegmentation.ActivatePreparedSigmoid([1f], 1, 2));
foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
    Refuse(() => LegacyPortableMathSegmentation.ActivatePreparedSigmoid([invalid], 1, 1));
Refuse(() => LegacyPortableMathSegmentation.ActivatePreparedSigmoid([0f], 1, 1, new(true)));
using var stream = File.Create(args[0]);
using var writer = new Utf8JsonWriter(stream);
writer.WriteStartObject(); writer.WriteNumber("guards", guards); writer.WriteStartArray("cases");
foreach (var row in rows)
{
    writer.WriteStartObject(); writer.WriteNumber("symbols", row.Symbols); writer.WriteNumber("segments", row.Segments);
    static byte[] Bytes(float[] values) { var bytes = new byte[values.Length * 4]; Buffer.BlockCopy(values, 0, bytes, 0, bytes.Length); return bytes; }
    writer.WriteBase64String("input", Bytes(row.Input)); writer.WriteBase64String("output", Bytes(row.Output)); writer.WriteEndObject();
}
writer.WriteEndArray(); writer.WriteEndObject();
Console.WriteLine($"PASS prepared segmentation sigmoid: {rows.Sum(row => row.Input.Length)} values/{guards} guards; no attention ancestry or EF4 execution.");
