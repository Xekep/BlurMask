using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if (args.Length != 1) throw new ArgumentException("Supply independent line-classifier controls JSON.");
if (new FileInfo(args[0]).Length > 8 * 1024 * 1024) throw new ArgumentException("Control file exceeds 8 MiB.");
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0])); int count = 0;
foreach (var model in document.RootElement.EnumerateArray())
{
    string path = model.GetProperty("resource").GetString()!;
    using var stream = File.OpenRead(path);
    var classifier = PreparedLineBoundaryClassifier.Load(stream);
    foreach (var item in model.GetProperty("controls").EnumerateArray())
    {
        float[] input = item.GetProperty("input").EnumerateArray().Select(x => x.GetSingle()).ToArray();
        float[] copy = (float[])input.Clone();
        int frames = item.GetProperty("frames").GetInt32(); bool normalized = item.GetProperty("normalized").GetBoolean();
        float[] actual = classifier.Score(input, frames, normalized);
        float[] expected = item.GetProperty("expected").EnumerateArray().Select(x => x.GetSingle()).ToArray();
        Require(actual.Length == expected.Length, "Output shape.");
        for (int i = 0; i < actual.Length; i++) Require(MathF.Abs(actual[i] - expected[i]) <= 2e-4f, $"Classifier mismatch {count}:{i}, {actual[i]} vs {expected[i]}.");
        for (int i = 0; i < frames; i++) Require(MathF.Abs(actual[i*2]+actual[i*2+1]-1) < 1e-5f, "Probability normalization.");
        Require(copy.SequenceEqual(input), "Caller features mutated.");
        Require(classifier.Score(input, frames, normalized).SequenceEqual(actual), "Repeated run changed recurrent state.");
        Task<float[]>[] parallel = Enumerable.Range(0, 3).Select(_ => Task.Run(() => classifier.Score(input, frames, normalized))).ToArray();
        Task.WaitAll(parallel); Require(parallel.All(t => t.Result.SequenceEqual(actual)), "Concurrent scorer state leaked.");
        count++;
    }
    Throws<ArgumentException>(() => classifier.Score(new float[41], 1));
    Throws<ArgumentException>(() => classifier.Score(new float[42], 0));
    float[] invalid = new float[42]; invalid[0] = float.NaN;
    Throws<ArgumentException>(() => classifier.Score(invalid, 1));
    Throws<OperationCanceledException>(() => classifier.Score(new float[42], 1, false, new(true)));
    byte[] bytes = File.ReadAllBytes(path), truncated = bytes[..^1];
    Throws<InvalidDataException>(() => PreparedLineBoundaryClassifier.Load(new MemoryStream(truncated)));
    byte[] extra = new byte[bytes.Length+1]; bytes.CopyTo(extra, 0);
    Throws<InvalidDataException>(() => PreparedLineBoundaryClassifier.Load(new MemoryStream(extra)));
    // All stored values are finite, but an excessive scale times a nonzero
    // bias overflows the output logits. The scorer must reject the result.
    byte[] overflow = (byte[])bytes.Clone();
    int payloadSize = BinaryPrimitives.ReadInt32LittleEndian(overflow.AsSpan(132));
    int columns = ((classifier.HiddenWidth * 2 + 1 + 15) / 16) * 16;
    int outputStart = 160 + payloadSize - (2 * columns * 2 + 96);
    overflow.AsSpan(outputStart, columns * 2).Clear();
    BinaryPrimitives.WriteInt16LittleEndian(overflow.AsSpan(outputStart + classifier.HiddenWidth * 4), 100);
    BinaryPrimitives.WriteSingleLittleEndian(overflow.AsSpan(160 + payloadSize - 96), float.MaxValue);
    var overflowClassifier = PreparedLineBoundaryClassifier.Load(new MemoryStream(overflow));
    Throws<InvalidDataException>(() => overflowClassifier.Score(new float[42], 1, true));
    bytes[12] = 1; Throws<InvalidDataException>(() => PreparedLineBoundaryClassifier.Load(new MemoryStream(bytes)));
}
Require(count > 0, "Empty controls.");
Console.WriteLine($"PASS {count} independent affine100/BLSTM/two-output classifier controls, normalization, repeated/concurrent inference, immutable input, rejected resources and cancellation.");
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}."); }
