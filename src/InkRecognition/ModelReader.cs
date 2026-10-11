using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace InkRecognition;

internal static class ModelReader
{
    private static readonly UTF8Encoding Utf8 = new(false, true);
    private sealed record Tensor(string Name, int Kind, int Offset, int Bytes, int Rows, int Columns, int InputWidth, int Storage);
    private sealed record Layer(int Width, bool Bidirectional, int Storage);

    internal static RecognitionModel Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead) throw new ArgumentException("Model stream must be readable.", nameof(stream));
        Span<byte> header = stackalloc byte[48];
        try { stream.ReadExactly(header); }
        catch (EndOfStreamException e) { throw new InvalidDataException("Truncated model header.", e); }
        Require(header[..8].SequenceEqual(ModelFormat.Magic), "Invalid model magic.");
        Require(BinaryPrimitives.ReadUInt32LittleEndian(header[8..]) == ModelFormat.Version, "Unsupported model version.");
        var bodyBytes = BinaryPrimitives.ReadUInt32LittleEndian(header[12..]);
        Require(bodyBytes > 0 && bodyBytes <= ModelFormat.MaxBodyBytes, "Model body exceeds the supported size.");
        var body = new byte[(int)bodyBytes];
        try { stream.ReadExactly(body); }
        catch (EndOfStreamException e) { throw new InvalidDataException("Truncated model body.", e); }
        Require(stream.ReadByte() == -1, "Unexpected bytes after model body.");
        Require(CryptographicOperations.FixedTimeEquals(SHA256.HashData(body), header[16..48]), "Model checksum mismatch.");
        try { return Parse(body); }
        catch (OverflowException e) { throw new InvalidDataException("Model dimensions overflow.", e); }
        catch (DecoderFallbackException e) { throw new InvalidDataException("Invalid UTF-8 in model.", e); }
    }

    private static RecognitionModel Parse(byte[] body)
    {
        var reader = new Cursor(body);
        int input = reader.Count(1, ModelFormat.MaxInputWidth, "input width");
        int output = reader.Count(2, ModelFormat.MaxOutputWidth, "output width");
        int blank = reader.Count(0, output - 1, "blank index");
        int layerCount = reader.Count(1, ModelFormat.MaxLayers, "layer count");
        int labelCount = reader.Count(1, output - 1, "label count");
        Require(labelCount == output - 1, "Labels must cover every nonblank output.");
        string language = reader.Text(128);
        Require(!string.IsNullOrWhiteSpace(language), "Missing model language.");
        var means = reader.Floats(input);
        var scales = reader.Floats(input);
        Require(scales.All(x => x > 0), "Normalization scales must be positive.");
        float spacing = reader.Float();
        Require(spacing > 0, "Invalid point spacing.");
        int minPoints = reader.Count(1, 65536, "minimum point count");
        int linkMode = reader.Count(0, 65536, "link mode");
        var architecture = new Layer[layerCount];
        for (int i = 0; i < architecture.Length; i++)
            architecture[i] = new(reader.Count(1, ModelFormat.MaxHiddenWidth, "hidden width"),
                reader.Count(0, 1, "bidirectional flag") == 1, reader.Count(1, 3, "storage code"));
        int outputStorage = reader.Count(1, 3, "output storage code");
        var labels = new string?[output];
        for (int i = 0; i < output; i++) if (i != blank) labels[i] = reader.Text(64);
        int tensorCount = reader.Count(1, ModelFormat.MaxTensorCount, "tensor count");
        var tensors = new Dictionary<string, Tensor>(StringComparer.Ordinal);
        int expectedOffset = 0;
        for (int i = 0; i < tensorCount; i++)
        {
            var tensor = new Tensor(reader.Text(256), reader.Count(1, 2, "tensor kind"),
                reader.Count(0, ModelFormat.MaxBodyBytes, "tensor offset"),
                reader.Count(1, ModelFormat.MaxBodyBytes, "tensor bytes"),
                reader.Count(1, ModelFormat.MaxOutputWidth, "tensor rows"),
                reader.Count(1, ModelFormat.MaxTensorColumns, "tensor columns"),
                reader.Count(0, ModelFormat.MaxOutputWidth, "tensor input width"),
                reader.Count(1, 3, "tensor storage"));
            Require(tensor.Offset == expectedOffset, "Tensor data must be contiguous and ordered.");
            expectedOffset = checked(tensor.Offset + tensor.Bytes);
            Require(expectedOffset <= ModelFormat.MaxBodyBytes, "Tensor extends beyond the model limit.");
            Require(tensors.TryAdd(tensor.Name, tensor), "Duplicate tensor name.");
        }
        int payloadLength = reader.Count(1, ModelFormat.MaxBodyBytes, "parameter payload bytes");
        Require(payloadLength == expectedOffset, "Parameter payload size does not match tensors.");
        var payload = reader.Take(payloadLength);
        Require(reader.Remaining == 0, "Unexpected model body fields.");
        var layers = new LstmLayer[layerCount];
        int previous = input;
        for (int i = 0; i < layers.Length; i++)
        {
            var spec = architecture[i];
            var forward = Direction($"layer{i + 1}.forward", spec.Width, previous, spec.Storage);
            var backward = spec.Bidirectional ? Direction($"layer{i + 1}.backward", spec.Width, previous, spec.Storage) : null;
            layers[i] = new(spec.Width, forward, backward);
            previous = layers[i].OutputWidth;
        }
        var affine = Matrix("output", output, previous, outputStorage);
        Require(tensors.Count == 0, "Unexpected or unused model tensors.");
        return new(language, labels, blank, means, scales, spacing, minPoints, linkMode, new(input, layers, affine));

        Tensor TakeTensor(string name, int kind)
        {
            Require(tensors.Remove(name, out var tensor), $"Missing tensor {name}.");
            Require(tensor!.Kind == kind, $"Invalid tensor kind for {name}.");
            return tensor;
        }
        AffineWeights Matrix(string name, int rows, int width, int code)
        {
            var tensor = TakeTensor(name, 1);
            int columns = ModelFormat.Align(ModelFormat.Align(width + 1), code == 1 ? 8 : code == 2 ? 16 : 32);
            int cells = checked(rows * columns);
            int bytes = checked(cells * (code == 1 ? 4 : code == 2 ? 2 : 1));
            if (code != 1) bytes = checked(bytes + ModelFormat.Align(rows) * 8 + 32);
            Require(tensor.Rows == rows && tensor.Columns == columns && tensor.InputWidth == width &&
                tensor.Storage == code && tensor.Bytes == bytes, $"Invalid shape/storage for {name}.");
            var cursor = new Cursor(payload.Slice(tensor.Offset, tensor.Bytes));
            float[]? floats = code == 1 ? cursor.Floats(cells) : null;
            short[]? shorts = code == 2 ? cursor.Shorts(cells) : null;
            sbyte[]? signed = code == 3 ? cursor.SignedBytes(cells) : null;
            var rowScales = new float[rows]; var sums = new float[rows];
            var auxiliary = new float[8];
            if (code == 1) Array.Fill(rowScales, 1f);
            else
            {
                var metadata = cursor.Floats(checked(ModelFormat.Align(rows) * 2));
                for (int r = 0; r < rows; r++)
                {
                    rowScales[r] = metadata[r * 2]; sums[r] = metadata[r * 2 + 1];
                    Require(rowScales[r] >= 0, "Negative quantized row scale.");
                }
                auxiliary = cursor.Floats(8);
                if (code == 2) Require(auxiliary[0] > 0 && auxiliary[0] <= 32767, "Invalid int16 quantizer limit.");
            }
            for (int r = 0; r < rows; r++)
                for (int c = width + 1; c < columns; c++)
                    Require(code == 1 ? floats![r * columns + c] == 0 : code == 2 ? shorts![r * columns + c] == 0 : signed![r * columns + c] == 0,
                        "Nonzero matrix column padding.");
            Require(cursor.Remaining == 0, "Unconsumed affine tensor data.");
            return new(rows, width, columns, code, floats, shorts, signed, rowScales, sums, auxiliary);
        }
        float[] Peephole(string name, int width)
        {
            var tensor = TakeTensor(name, 2);
            Require(tensor.Rows == width && tensor.Columns == 1 && tensor.InputWidth == 0 && tensor.Storage == 1 &&
                tensor.Bytes == checked(ModelFormat.Align(width) * 4), $"Invalid peephole shape for {name}.");
            var values = new Cursor(payload.Slice(tensor.Offset, tensor.Bytes)).Floats(ModelFormat.Align(width));
            Require(values.AsSpan(width).IndexOfAnyExcept(0f) < 0, "Nonzero peephole padding.");
            return values.AsSpan(0, width).ToArray();
        }
        LstmDirection Direction(string name, int width, int previousWidth, int code)
        {
            string[] gates = ["g", "i", "f", "o"];
            var incoming = gates.Select(g => Matrix(name + ".input." + g, width, previousWidth, code)).ToArray();
            var recurrent = gates.Select(g => Matrix(name + ".recurrent." + g, width, width, code)).ToArray();
            return new(incoming, recurrent, Peephole(name + ".recurrent.pi", width),
                Peephole(name + ".recurrent.pf", width), Peephole(name + ".recurrent.po", width));
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException(message);
    }
    private sealed class Cursor(ReadOnlyMemory<byte> memory)
    {
        private int _position;
        internal int Remaining => memory.Length - _position;
        internal ReadOnlyMemory<byte> Take(int count)
        {
            Require(count >= 0 && count <= Remaining, "Truncated model field.");
            var value = memory.Slice(_position, count); _position += count; return value;
        }
        internal int Count(int minimum, int maximum, string field)
        {
            uint value = BinaryPrimitives.ReadUInt32LittleEndian(Take(4).Span);
            Require(value >= minimum && value <= maximum, $"Invalid {field}.");
            return (int)value;
        }
        internal string Text(int maximum) => Utf8.GetString(Take(Count(0, maximum, "UTF-8 length")).Span);
        internal float Float()
        {
            float value = BinaryPrimitives.ReadSingleLittleEndian(Take(4).Span);
            Require(float.IsFinite(value), "Nonfinite model value."); return value;
        }
        internal float[] Floats(int count)
        {
            var result = new float[count];
            for (int i = 0; i < count; i++) result[i] = Float();
            return result;
        }
        internal short[] Shorts(int count)
        {
            var source = Take(checked(count * 2)).Span; var result = new short[count];
            for (int i = 0; i < count; i++) result[i] = BinaryPrimitives.ReadInt16LittleEndian(source[(i * 2)..]);
            return result;
        }
        internal sbyte[] SignedBytes(int count)
        {
            var source = Take(count).Span; var result = new sbyte[count];
            for (int i = 0; i < count; i++) result[i] = unchecked((sbyte)source[i]);
            return result;
        }
    }
}
