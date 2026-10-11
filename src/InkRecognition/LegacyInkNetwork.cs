using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>
/// Experimental legacy fixed-vector classifier. This class does not accept pen
/// coordinates or decode words. Activations and weights must be supplied by the caller.
/// </summary>
public sealed class LegacyInkNetwork
{
    private readonly byte[] _data;
    private readonly int _hiddenWeights, _hiddenScales, _outputWeights, _outputScales;
    public int InputWidth { get; }
    public int HiddenWidth { get; }
    public int OutputWidth { get; }
    public ushort Kind { get; }

    private LegacyInkNetwork(byte[] data, int inputs, int hidden, int outputs, ushort kind,
        int hiddenWeights, int hiddenScales, int outputWeights, int outputScales)
    {
        _data = data; InputWidth = inputs; HiddenWidth = hidden - 1;
        OutputWidth = outputs - 1; Kind = kind;
        _hiddenWeights = hiddenWeights; _hiddenScales = hiddenScales;
        _outputWeights = outputWeights; _outputScales = outputScales;
    }

    /// <summary>Read one complete FEA0/version106, int8 row-scaled two-layer object.</summary>
    public static LegacyInkNetwork Load(ReadOnlySpan<byte> data)
    {
        if (data.Length is < 33 or > 64 * 1024 * 1024) throw Invalid("Invalid network length.");
        if (U16(data, 0) != 0xfea0 || U16(data, 2) != 0x106 || U32(data, 4) != data.Length - 8)
            throw Invalid("Unsupported legacy network header.");
        if (data[8] != 0x12 || U32(data, 19) != 0 || U32(data, 23) != 0 || U16(data, 31) != 10)
            throw Invalid("Unsupported legacy network architecture/quantization.");
        int inputs = Width(data, 11), hidden = Width(data, 15), outputs = Width(data, 27);
        int cursor = 33;
        var first = ReadMatrix(data, ref cursor, inputs, hidden);
        var second = ReadMatrix(data, ref cursor, hidden, outputs);
        if (cursor != data.Length) throw Invalid("Unconsumed legacy network bytes.");
        return new(data.ToArray(), inputs, hidden, outputs, U16(data, 9),
            first.Weights, first.Scales, second.Weights, second.Scales);
    }

    /// <summary>
    /// Evaluate every stored int16 input channel. No global input bias is inserted.
    /// Feature extraction, input-slot semantics and classifier labels are not inferred.
    /// Raw mode returns scaled int32 dots; sigmoid mode returns integer scores 0..1024.
    /// </summary>
    public int[] Evaluate(ReadOnlySpan<short> features, LegacyInkActivations activations,
        bool sigmoidOutput = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(activations);
        if (features.Length != InputWidth) throw new ArgumentException("Full legacy feature vector required.", nameof(features));
        cancellationToken.ThrowIfCancellationRequested();
        var hidden = new short[HiddenWidth + 1];
        hidden[0] = 1024;
        for (int row = 0; row < HiddenWidth; row++)
        {
            if ((row & 31) == 0) cancellationToken.ThrowIfCancellationRequested();
            int dot = Dot(features, _hiddenWeights + row * InputWidth, _data[_hiddenScales + row + 1]);
            hidden[row + 1] = (short)activations.Hidden(dot);
        }
        var output = new int[OutputWidth];
        for (int row = 0; row < output.Length; row++)
        {
            if ((row & 31) == 0) cancellationToken.ThrowIfCancellationRequested();
            int dot = Dot(hidden, _outputWeights + row * hidden.Length, _data[_outputScales + row + 1]);
            output[row] = sigmoidOutput ? activations.Output(dot) : dot;
        }
        return output;
    }

    private int Dot(ReadOnlySpan<short> features, int offset, byte scale)
    {
        int sum = 0;
        unchecked
        {
            for (int i = 0; i < features.Length; i++) sum += features[i] * (sbyte)_data[offset + i];
            return sum * scale;
        }
    }

    private static (int Weights, int Scales) ReadMatrix(ReadOnlySpan<byte> data, ref int cursor, int columns, int rows)
    {
        int bytes = checked(columns * (rows - 1));
        if ((long)cursor + bytes + rows > data.Length) throw Invalid("Truncated legacy matrix.");
        int weights = cursor, scales = cursor + bytes;
        if (data[scales] != 1) throw Invalid("Invalid leading row scale.");
        for (int i = 1; i < rows; i++) if (data[scales + i] == 0) throw Invalid("Zero legacy row scale.");
        cursor = scales + rows;
        return (weights, scales);
    }

    private static int Width(ReadOnlySpan<byte> data, int offset)
    {
        uint width = U32(data, offset);
        return width is > 1 and <= 4096 ? (int)width : throw Invalid("Invalid legacy layer width.");
    }
    private static ushort U16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
    private static uint U32(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(data[offset..]);
    private static InvalidDataException Invalid(string reason) => new(reason);
}

/// <summary>Caller-supplied integer activations. No vendor tables are embedded.</summary>
public sealed class LegacyInkActivations
{
    private readonly int[] _hidden, _output;

    public LegacyInkActivations(ReadOnlySpan<int> hidden, ReadOnlySpan<int> output)
    {
        Validate(hidden, 48, 0, 1018);
        Validate(output, 128, 512, 1024);
        _hidden = hidden.ToArray(); _output = output.ToArray();
    }

    private static void Validate(ReadOnlySpan<int> table, int length, int first, int maximum)
    {
        if (table.Length != length || table[0] != first) throw new ArgumentException("Invalid legacy activation shape/origin.");
        for (int i = 1; i < table.Length; i++)
            if (table[i] < table[i - 1] || table[i] > maximum) throw new ArgumentException("Invalid legacy activation range/order.");
    }

    internal int Hidden(int value)
    {
        long index = Math.Abs((long)value) >> 16;
        int result = index < _hidden.Length ? _hidden[(int)index] : 1018;
        return value <= 0 ? -result : result;
    }

    internal int Output(int value)
    {
        long index = Math.Abs((long)value) >> 15;
        int result = index < _output.Length ? _output[(int)index] : 1024;
        return value <= 0 ? 1024 - result : result;
    }
}
