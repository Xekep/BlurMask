using System.Security.Cryptography;
using System.Text;

namespace InkRecognition.Math;

internal sealed record MathModel(string[] Labels, byte[][] Graphs)
{
    public static MathModel Read(Stream source)
    {
        using var reader = new BinaryReader(source, Encoding.UTF8, leaveOpen: true);
        if (!reader.ReadBytes(8).AsSpan().SequenceEqual("IKRMATH1"u8) || reader.ReadUInt32() != 1)
            throw new InvalidDataException("Unsupported Math recognition model pack.");
        uint size = reader.ReadUInt32();
        if (size > 64 * 1024 * 1024 || size < 64) throw new InvalidDataException("Invalid Math pack size.");
        var hash = reader.ReadBytes(32); var body = reader.ReadBytes((int)size);
        if (hash.Length != 32 || body.Length != size || !CryptographicOperations.FixedTimeEquals(hash, SHA256.HashData(body)) || source.ReadByte() != -1)
            throw new InvalidDataException("Math model hash, length or trailing data mismatch.");
        using var stream = new MemoryStream(body, writable: false);
        using var r = new BinaryReader(stream, new UTF8Encoding(false, true));
        if (r.ReadUInt32() != 26 || r.ReadUInt32() != 297 || r.ReadUInt32() != 0 || r.ReadUInt32() != 1 || r.ReadUInt32() != 1)
            throw new InvalidDataException("Unsupported Math feature/decoder profile.");
        var labels = new string[297];
        for (int i = 0; i < labels.Length; i++)
        {
            uint length = r.ReadUInt32();
            if (length > 4096 || length > stream.Length - stream.Position) throw new InvalidDataException("Invalid Math label.");
            labels[i] = new UTF8Encoding(false, true).GetString(r.ReadBytes((int)length));
        }
        if (labels[0] != "[bos]" || labels[1] != "[eos]" || r.ReadUInt32() != 3) throw new InvalidDataException("Invalid Math vocabulary/graphs.");
        var graphs = new byte[3][];
        for (int i = 0; i < graphs.Length; i++)
        {
            uint length = r.ReadUInt32();
            if (length < 8 || length > 32 * 1024 * 1024 || length > stream.Length - stream.Position) throw new InvalidDataException("Invalid Math graph size.");
            graphs[i] = r.ReadBytes((int)length);
            if (!graphs[i].AsSpan(4, 4).SequenceEqual("ORTM"u8)) throw new InvalidDataException("Expected ORT graph.");
        }
        if (stream.Position != stream.Length) throw new InvalidDataException("Unexpected Math profile data.");
        return new(labels, graphs);
    }
}
