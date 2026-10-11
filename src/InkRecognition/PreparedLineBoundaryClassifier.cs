using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>
/// The checked embedded 2e3 42-input affine100/BLSTM30-or-50/two-output profile.
/// Inputs are prepared pair features, not raw ink; outputs have no assigned font-size semantics.
/// </summary>
public sealed class PreparedLineBoundaryClassifier
{
    public const int InputWidth = 42;
    public const int OutputWidth = 2;
    private readonly NativeNetwork _network;
    private readonly float[] _means;
    private readonly float[] _scales;
    public int HiddenWidth { get; }

    private PreparedLineBoundaryClassifier(NativeNetwork network, float[] means, float[] scales, int hiddenWidth)
    { _network = network; _means = means; _scales = scales; HiddenWidth = hiddenWidth; }

    public static PreparedLineBoundaryClassifier Load(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead) throw new ArgumentException("Resource stream must be readable.", nameof(stream));
        byte[] header = new byte[16];
        try { stream.ReadExactly(header); }
        catch (EndOfStreamException e) { throw new InvalidDataException("Truncated line model header.", e); }
        uint length = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(4));
        Require(header.AsSpan(0, 4).SequenceEqual(new byte[] { 0x90, 0xfe, 1, 1 }) &&
            length is >= 8 and <= 1_000_000 && BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(8)) == 0x5f &&
            BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(12)) == 0 &&
            BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(14)) == 0x106, "Unsupported embedded line model header.");
        byte[] resource = new byte[checked((int)length + 8)];
        header.CopyTo(resource, 0);
        try { stream.ReadExactly(resource.AsSpan(16)); }
        catch (EndOfStreamException e) { throw new InvalidDataException("Truncated line model.", e); }
        Require(stream.ReadByte() == -1, "Trailing resource bytes.");
        var cursor = new Reader(resource, 16);
        cursor.Expect(1); cursor.Expect(3);
        cursor.Layer(1, InputWidth); cursor.Layer(1, 100);
        cursor.Expect(8); int hidden = cursor.Count(30, 50); Require(hidden is 30 or 50, "Unsupported line-model hidden width.");
        cursor.Expect(0); cursor.Expect(2);
        cursor.Expect(0); cursor.Expect(OutputWidth); cursor.Expect(1); cursor.Expect(0);
        cursor.Expect(0); cursor.Expect(0); cursor.Expect(0); cursor.Expect(0);
        foreach (int value in new[] { 0, 1, 1, 3, 2, 0, 8 }) cursor.Expect(value);
        int payloadLength = cursor.Count(1, 1_000_000);
        while (cursor.Position % 32 != 0) Require(cursor.Byte() == 255, "Invalid line-model alignment padding.");
        var payload = new Reader(cursor.Take(payloadLength));
        AffineWeights preparation = payload.Matrix(100, InputWidth);
        AffineWeights[][] incoming = new AffineWeights[2][], recurrent = new AffineWeights[2][];
        float[][][] peepholes = new float[2][][];
        for (int d = 0; d < 2; d++) incoming[d] = Enumerable.Range(0, 4).Select(_ => payload.Matrix(hidden, 100)).ToArray();
        for (int d = 0; d < 2; d++)
        {
            recurrent[d] = Enumerable.Range(0, 4).Select(_ => payload.Matrix(hidden, hidden)).ToArray();
            peepholes[d] = Enumerable.Range(0, 3).Select(_ => payload.Peephole(hidden)).ToArray();
        }
        AffineWeights output = payload.Matrix(OutputWidth, hidden * 2);
        Require(payload.Remaining == 0, "Unconsumed line-model tensor payload.");
        cursor.Expect(InputWidth * 8);
        float[] means = cursor.Floats(InputWidth), scales = cursor.Floats(InputWidth);
        Require(scales.All(s => s > 0), "Invalid line feature normalization scales.");
        Require(cursor.Remaining == 0, "Unconsumed line-model fields.");
        LstmDirection Direction(int d) => new(incoming[d], recurrent[d], peepholes[d][0], peepholes[d][1], peepholes[d][2]);
        var network = new NativeNetwork(InputWidth, [new(hidden, Direction(0), Direction(1))], output, preparation);
        return new(network, means, scales, hidden);
    }

    /// <summary>Returns frame-major probabilities for the two native outputs; each call owns scratch memory.</summary>
    public float[] Score(ReadOnlySpan<float> pairFeatures, int frames, bool alreadyNormalized = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (frames is < 1 or > 4096 || (long)frames * InputWidth != pairFeatures.Length)
            throw new ArgumentException("Expected one to 4096 prepared 42-channel frames.");
        float[] input = pairFeatures.ToArray();
        for (int i = 0; i < input.Length; i++)
        {
            if ((i & 255) == 0) cancellationToken.ThrowIfCancellationRequested();
            if (!float.IsFinite(input[i])) throw new ArgumentException("Pair features must be finite.");
            if (!alreadyNormalized)
            {
                float centered = input[i] - _means[i % InputWidth];
                input[i] = centered * _scales[i % InputWidth];
                if (!float.IsFinite(input[i])) throw new ArgumentException("Normalized pair features overflow.");
            }
        }
        float[] probabilities = _network.Run(input, frames, cancellationToken);
        foreach (float value in probabilities)
            if (!float.IsFinite(value) || value is < 0 or > 1)
                throw new InvalidDataException("Line model produced invalid probabilities.");
        return probabilities;
    }

    private static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
    private sealed class Reader(ReadOnlyMemory<byte> data, int start = 0)
    {
        internal int Position { get; private set; } = start;
        internal int Remaining => data.Length - Position;
        internal ReadOnlyMemory<byte> Take(int bytes)
        { Require(bytes >= 0 && bytes <= Remaining, "Truncated line-model field."); var result = data.Slice(Position, bytes); Position += bytes; return result; }
        internal byte Byte() => Take(1).Span[0];
        internal int Count(int minimum, int maximum)
        { uint value = BinaryPrimitives.ReadUInt32LittleEndian(Take(4).Span); Require(value >= minimum && value <= maximum, "Invalid line-model count."); return (int)value; }
        internal void Expect(int value) => Require(Count(0, int.MaxValue) == value, "Unsupported line-model field.");
        internal void Layer(int kind, int width) { Expect(kind); Expect(width); Expect(0); Expect(2); }
        internal float[] Floats(int count)
        {
            var bytes = Take(checked(count * 4)).Span; var values = new float[count];
            for (int i = 0; i < count; i++) { values[i] = BinaryPrimitives.ReadSingleLittleEndian(bytes[(i * 4)..]); Require(float.IsFinite(values[i]), "Nonfinite line-model tensor."); }
            return values;
        }
        internal AffineWeights Matrix(int rows, int inputWidth)
        {
            int columns = ModelFormat.Align(ModelFormat.Align(inputWidth + 1), 16);
            var bytes = Take(checked(rows * columns * 2)).Span; var weights = new short[rows * columns];
            for (int i = 0; i < weights.Length; i++) weights[i] = BinaryPrimitives.ReadInt16LittleEndian(bytes[(i * 2)..]);
            float[] metadata = Floats(ModelFormat.Align(rows) * 2), auxiliary = Floats(8);
            float[] scales = new float[rows], sums = new float[rows];
            Require(auxiliary[0] is > 0 and <= 32767, "Invalid line-model quantizer limit.");
            for (int r = 0; r < rows; r++)
            {
                scales[r] = metadata[r * 2]; sums[r] = metadata[r * 2 + 1];
                Require(scales[r] >= 0, "Negative line-model row scale.");
                for (int c = inputWidth + 1; c < columns; c++) Require(weights[r * columns + c] == 0, "Nonzero line-model matrix padding.");
            }
            return new(rows, inputWidth, columns, 2, null, weights, null, scales, sums, auxiliary);
        }
        internal float[] Peephole(int width)
        { float[] values = Floats(ModelFormat.Align(width)); Require(values.AsSpan(width).IndexOfAnyExcept(0f) < 0, "Nonzero line-model peephole padding."); return values.AsSpan(0, width).ToArray(); }
    }
}
