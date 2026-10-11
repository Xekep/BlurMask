using System.Buffers;
using System.Reflection;

namespace BlurMask;

// Small fixed CNN, evaluated directly in native-compiled C#. No ML runtime or network access.
internal sealed class AiSymbolNetwork
{
    internal static readonly Lazy<AiSymbolNetwork> Instance = new(() => new());
    private readonly float[] _c1, _b1, _c2, _b2, _f1, _fb1, _f2, _fb2;
    internal char[] Labels { get; }
    private AiSymbolNetwork()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BlurMask.AiPen.bin")
            ?? throw new InvalidDataException("AI pen model is missing.");
        using var reader = new BinaryReader(stream);
        if (!reader.ReadBytes(8).AsSpan().SequenceEqual("BMPEN001"u8) || reader.ReadInt32() != 32 ||
            reader.ReadInt32() != 12 || reader.ReadInt32() != 24 || reader.ReadInt32() != 96)
            throw new InvalidDataException("Invalid AI pen model.");
        var classes = reader.ReadInt32();
        if (classes is < 1 or > 256) throw new InvalidDataException("Invalid AI pen vocabulary.");
        Labels = new char[classes];
        for (var i = 0; i < classes; i++) Labels[i] = checked((char)reader.ReadInt32());
        _c1 = Read(12 * 9); _b1 = Read(12); _c2 = Read(24 * 12 * 9); _b2 = Read(24);
        _f1 = Read(96 * 864); _fb1 = Read(96); _f2 = Read(classes * 96); _fb2 = Read(classes);
        if (stream.Position != stream.Length) throw new InvalidDataException("AI pen model has trailing data.");
        float[] Read(int count)
        {
            var values = new float[count];
            for (var i = 0; i < count; i++) values[i] = reader.ReadSingle();
            return values;
        }
    }

    internal float[] Predict(ReadOnlySpan<float> pixels)
    {
        if (pixels.Length != 1024) throw new ArgumentException("Expected a 32×32 symbol.");
        var buffer = ArrayPool<float>.Shared.Rent(12 * 15 * 15 + 24 * 6 * 6 + 96);
        try
        {
            var first = buffer.AsSpan(0, 2700); var second = buffer.AsSpan(2700, 864); var hidden = buffer.AsSpan(3564, 96);
            ConvolveAndPool(pixels, 1, 32, _c1, _b1, first);
            ConvolveAndPool(first, 12, 15, _c2, _b2, second);
            Dense(second, _f1, _fb1, hidden, true);
            var logits = new float[Labels.Length]; Dense(hidden, _f2, _fb2, logits, false);
            return logits;
        }
        finally { ArrayPool<float>.Shared.Return(buffer); }
    }

    internal (char Symbol, float Confidence) Recognize(ReadOnlySpan<float> pixels)
    {
        var logits = Predict(pixels); var best = 0;
        for (var i = 1; i < logits.Length; i++) if (logits[i] > logits[best]) best = i;
        var total = 0f;
        foreach (var value in logits) total += MathF.Exp(value - logits[best]);
        return (Labels[best], 1 / total);
    }

    private static void ConvolveAndPool(ReadOnlySpan<float> input, int channels, int width,
        ReadOnlySpan<float> weights, ReadOnlySpan<float> bias, Span<float> output)
    {
        var pooledWidth = (width - 2) / 2;
        for (var channel = 0; channel < bias.Length; channel++)
            for (var y = 0; y < pooledWidth; y++) for (var x = 0; x < pooledWidth; x++)
            {
                var maximum = 0f;
                for (var dy = 0; dy < 2; dy++) for (var dx = 0; dx < 2; dx++)
                {
                    var sum = bias[channel];
                    for (var source = 0; source < channels; source++)
                        for (var ky = 0; ky < 3; ky++)
                        {
                            var pixel = source * width * width + (y * 2 + dy + ky) * width + x * 2 + dx;
                            var weight = (channel * channels + source) * 9 + ky * 3;
                            sum += input[pixel] * weights[weight];
                            sum += input[pixel + 1] * weights[weight + 1];
                            sum += input[pixel + 2] * weights[weight + 2];
                        }
                    maximum = MathF.Max(maximum, sum);
                }
                output[channel * pooledWidth * pooledWidth + y * pooledWidth + x] = maximum;
            }
    }
    private static void Dense(ReadOnlySpan<float> input, ReadOnlySpan<float> weights,
        ReadOnlySpan<float> bias, Span<float> output, bool relu)
    {
        for (var row = 0; row < bias.Length; row++)
        {
            var sum = bias[row]; var start = row * input.Length;
            for (var column = 0; column < input.Length; column++) sum += input[column] * weights[start + column];
            output[row] = relu ? MathF.Max(0, sum) : sum;
        }
    }
}
