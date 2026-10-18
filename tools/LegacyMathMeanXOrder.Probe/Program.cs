using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
using InkRecognition.Math;

if (args.Length != 1) throw new ArgumentException("capture.json");
var cases = new List<Case>(); uint random = 0x43a159b7;
uint Next() => random = unchecked(random * 1664525u + 1013904223u);
byte[] Encode(LegacyMath2MeanXOrder.Entry[] rows)
{
    var bytes = new byte[rows.Length * 16];
    for (int i = 0; i < rows.Length; i++)
    {
        var span = bytes.AsSpan(i * 16, 16);
        BinaryPrimitives.WriteUInt32LittleEndian(span, rows[i].OriginalIndex);
        BinaryPrimitives.WriteUInt32LittleEndian(span[4..], rows[i].MeanBits);
        BinaryPrimitives.WriteUInt64LittleEndian(span[8..], rows[i].Opaque);
    }
    return bytes;
}
void Add(string family, LegacyMath2MeanXOrder.Entry[] input)
{
    var before = Encode(input); var after = LegacyMath2MeanXOrder.SortPrepared(input);
    if (!before.SequenceEqual(Encode(input))) throw new InvalidDataException("Caller storage changed.");
    cases.Add(new(family, before, Encode(after)));
}
foreach (int count in new[] { 0, 1, 2, 19, 20, 21, 22, 40, 41, 42, 43, 63, 64, 128, 255, 256 })
foreach (int family in Enumerable.Range(0, 8))
{
    var rows = new LegacyMath2MeanXOrder.Entry[count];
    for (int i = 0; i < count; i++)
    {
        uint bits = family switch
        {
            0 => BitConverter.SingleToUInt32Bits(7),
            1 => BitConverter.SingleToUInt32Bits(i),
            2 => BitConverter.SingleToUInt32Bits(count - i),
            3 => BitConverter.SingleToUInt32Bits(i % 3),
            4 => (i & 1) == 0 ? 0 : 0x80000000,
            5 => new uint[] { 0x7fc00001, 0x7f800000, 0xff800000, 0x7fa12345, 0xffc00003, 0, 0x80000000 }[i % 7],
            6 => Next(),
            _ => BitConverter.SingleToUInt32Bits((int)(Next() % 9) - 4)
        };
        rows[i] = new((uint)i, bits, ((ulong)Next() << 32) | Next());
    }
    Add($"threshold:{count}:{family}", rows);
}
for (int control = 0; control < 4096; control++)
{
    int count = control % 257; var rows = new LegacyMath2MeanXOrder.Entry[count];
    for (int i = 0; i < count; i++)
    {
        uint bits = control % 3 == 0 ? Next() : BitConverter.SingleToUInt32Bits((int)(Next() % 17) - 8);
        rows[i] = new(Next(), bits, ((ulong)Next() << 32) | Next());
    }
    Add("prepared-full-record", rows);
}
var sources = new List<Source>();
for (int control = 0; control < 257; control++)
{
    var means = Enumerable.Range(0, control).Select(i => (float)((i * 17 + control) % 9 - 4)).ToArray();
    sources.Add(new(means.Select(BitConverter.SingleToUInt32Bits).ToArray(), LegacyMath2MeanXOrder.CopySourceOrder(means)));
}
var features = new List<Feature>();
foreach (int count in new[] { 2, 19, 20, 21, 22, 41, 42, 43, 256 })
{
    InkStroke[] ink = Enumerable.Range(0, count).Select(i => new InkStroke([new InkPoint(0, i * 3)])).ToArray();
    var value = MathPreprocessor.Process(ink);
    features.Add(new(count, BitConverter.SingleToUInt32Bits(value.Scale), value.Frames, value.StrokeEnds,
        value.Values.Select(BitConverter.SingleToUInt32Bits).ToArray()));
}
int guards = 0;
void Reject(Action action)
{
    try { action(); } catch (Exception e) when (e is ArgumentException or OperationCanceledException) { guards++; return; }
    throw new InvalidDataException("Expected mean-order refusal.");
}
Reject(() => LegacyMath2MeanXOrder.SortPrepared(new LegacyMath2MeanXOrder.Entry[257]));
Reject(() => LegacyMath2MeanXOrder.CopySourceOrder(new float[257]));
using var canceled = new CancellationTokenSource(); canceled.Cancel();
Reject(() => LegacyMath2MeanXOrder.SortPrepared([], canceled.Token));
Reject(() => LegacyMath2MeanXOrder.CopySourceOrder([], canceled.Token));
int[] ties20 = LegacyMath2MeanXOrder.CopySourceOrder(new float[20]);
int[] ties21 = LegacyMath2MeanXOrder.CopySourceOrder(new float[21]);
if (!ties20.SequenceEqual(Enumerable.Range(0, 20)) || !ties21.SequenceEqual(Enumerable.Range(0, 21).Reverse()))
    throw new InvalidDataException("Native count20/21 tie discriminator failed.");
File.WriteAllText(args[0], JsonSerializer.Serialize(new Capture(cases.ToArray(), sources.ToArray(), features.ToArray(), guards), CaptureJson.Default.Capture));
Console.WriteLine($"PASS {cases.Count} prepared16B controls/{sources.Count} source-order projections/{features.Count} actual feature inputs/{guards} guards; count20 stable/count21 reversed equal keys.");
record Case(string Family, byte[] Before, byte[] After);
record Source(uint[] MeanBits, int[] Order);
record Feature(int Count, uint ScaleBits, int Frames, long[] StrokeEnds, uint[] Values);
record Capture(Case[] Cases, Source[] Sources, Feature[] Features, int Guards);
[JsonSerializable(typeof(Capture))]
internal partial class CaptureJson : JsonSerializerContext;
