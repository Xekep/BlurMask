using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;

internal static class InkRecognitionChecks
{
    internal static void Run()
    {
        VerifyScalarQuantization();
        var pack = File.ReadAllBytes(Path.Combine("tools", "ink-reference-fixture.bin"));
        using var expected = JsonDocument.Parse(File.ReadAllBytes(Path.Combine("tools", "ink-reference-expected.json")));
        var data = expected.RootElement;
        if (!Convert.ToHexStringLower(SHA256.HashData(pack)).Equals(data.GetProperty("modelSha256").GetString(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Synthetic recognition fixture checksum mismatch.");
        using var source = new MemoryStream(pack);
        var model = ModelReader.Read(source);
        var features = data.GetProperty("features").EnumerateArray().Select(x => x.GetSingle()).ToArray();
        var reference = data.GetProperty("probabilities").EnumerateArray().Select(x => x.GetSingle()).ToArray();
        var probabilities = model.Network.Run(features, data.GetProperty("frames").GetInt32(), default);
        if (probabilities.Length != reference.Length) throw new InvalidOperationException("Synthetic inference dimensions differ.");
        var error = probabilities.Zip(reference, (a, b) => MathF.Abs(a - b)).Max();
        if (error > data.GetProperty("tolerance").GetSingle()) throw new InvalidOperationException($"Recognition kernels differ from independent goldens: {error:G5}.");
        var parallel = Task.WhenAll(Enumerable.Range(0, 4).Select(_ => Task.Run(() => model.Network.Run(features, data.GetProperty("frames").GetInt32(), default)))).GetAwaiter().GetResult();
        if (parallel.Any(result => !result.SequenceEqual(probabilities))) throw new InvalidOperationException("Concurrent inference mutated model weights or shared buffers.");
        foreach (var bytes in new[] { pack[..20], pack[..^1], pack.Concat(new byte[] { 0 }).ToArray(), Corrupt(pack) })
            Expect<InvalidDataException>(() => StrokeTextRecognizer.Load(new MemoryStream(bytes)));
        var oversized = pack.ToArray(); System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(oversized.AsSpan(12), uint.MaxValue);
        Expect<InvalidDataException>(() => StrokeTextRecognizer.Load(new MemoryStream(oversized)));
        VerifyGeometry(model);
        VerifyCtc();
        using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
        var recognizer = StrokeTextRecognizer.Load(new MemoryStream(pack));
        Expect<OperationCanceledException>(() => recognizer.Recognize([new(new InkPoint[] { new(0, 0), new(10, 20) })], cancelled.Token));
        Expect<ArgumentException>(() => recognizer.Recognize([new(new InkPoint[] { new(float.NaN, 0) })]));
        Expect<ArgumentException>(() => recognizer.Recognize([new(new InkPoint[20001])]));
        if (recognizer.Recognize([]).Text.Length != 0 || recognizer.Recognize([new(new InkPoint[] { new(0, 5), new(10, 5) })]).Text.Length != 0)
            throw new InvalidOperationException("Degenerate ink produced text.");
        Console.WriteLine($"Portable ink recognition: synthetic float/int16/int8 BLSTM golden max error {error:G5}; malformed packs, geometry, CTC, limits and cancellation passed.");
    }

    private static void VerifyScalarQuantization()
    {
        // Directed expected CVTTSS2SI32 values from the pinned scalar routine.
        (float Rounded, int Converted)[] cases =
        [
            (0f, 0), (-0f, 0), (float.Epsilon, 0), (-float.Epsilon, 0),
            (.99999f, 0), (-.99999f, 0), (1.99999f, 1), (-1.99999f, -1),
            (2147483520f, 2147483520), (-2147483648f, int.MinValue),
            (2147483648f, int.MinValue), (-2147483904f, int.MinValue),
            (float.PositiveInfinity, int.MinValue), (float.NegativeInfinity, int.MinValue),
            (float.NaN, int.MinValue), (float.MaxValue, int.MinValue), (-float.MaxValue, int.MinValue)
        ];
        foreach (int limit in new[] { 0, 1, 2047, 4095, 32767 })
            foreach (var (rounded, converted) in cases)
                if (LegacyScalarInt16ActivationQuantization.ConvertRoundedAndClamp(rounded, limit) != Math.Clamp(converted, -limit, limit))
                    throw new InvalidOperationException("Scalar integer conversion order differs from native directed values.");
        Expect<ArgumentOutOfRangeException>(() => LegacyScalarInt16ActivationQuantization.ConvertRoundedAndClamp(0, -1));
        Expect<ArgumentOutOfRangeException>(() => LegacyScalarInt16ActivationQuantization.ConvertRoundedAndClamp(0, 32768));
        Console.WriteLine("Scalar activation conversion: 85 directed native boundary cases and two invalid limits passed.");
    }

    private static byte[] Corrupt(byte[] pack) { var changed = pack.ToArray(); changed[^1] ^= 1; return changed; }
    private static void Expect<T>(Action action) where T : Exception
    { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}."); }

    private static void VerifyGeometry(RecognitionModel model)
    {
        InkStroke[] strokes = [new(new InkPoint[] { new(3, 4), new(6, 8), new(6, 10) }), new(new InkPoint[] { new(8, 5), new(9, 7) })];
        var original = StrokeFeatures.Create(strokes, model.Means, model.Scales, model.Spacing, model.MinPoints, default);
        var shifted = strokes.Select(s => new InkStroke(s.Points.Select(p => new InkPoint(p.X * 2 + 100, p.Y * 2 + 200)).ToArray())).ToArray();
        var transformed = StrokeFeatures.Create(shifted, model.Means, model.Scales, model.Spacing, model.MinPoints, default);
        // Absolute float32 interpolation can lose a few ULPs after translation.
        if (original.Length != transformed.Length || original.Zip(transformed, (a, b) => MathF.Abs(a - b)).Any(e => e > 1e-4))
            throw new InvalidOperationException($"Recognition changed under translation and uniform scaling: lengths {original.Length}/{transformed.Length}, error {(original.Length == transformed.Length ? original.Zip(transformed, (a, b) => MathF.Abs(a - b)).Max() : -1):G5}.");
        if (original.Any(x => !float.IsFinite(x))) throw new InvalidOperationException("Features are not finite.");
    }

    private static void VerifyCtc()
    {
        var decoded = CtcDecoder.Decode([.9f, .1f, .8f, .2f, .1f, .9f, .95f, .05f], 4, ["a", null], 1);
        if (decoded.Text != "aa" || MathF.Abs(decoded.Confidence - MathF.Sqrt(.9f * .95f)) > 1e-6)
            throw new InvalidOperationException("CTC did not collapse repeated labels or preserve a repeated letter after a blank.");
        if (CtcDecoder.Decode([.9f, .05f, .05f], 1, [null, "a", null], 2).Text != "")
            throw new InvalidOperationException("Special model token became printable text.");
        Expect<InvalidDataException>(() => CtcDecoder.Decode([float.NaN, .1f], 1, ["a", null], 1));
    }
}
