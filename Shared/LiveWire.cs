using System.Buffers.Binary;
using System.Text.Json;

namespace BlurMask.Shared;

// Required v2 protocol. JSON carries initial style, shapes and control messages;
// point deltas and bounded saved patches use lossless binary coordinates.
public static class LiveWire
{
    public const string Protocol = "blurmask.live.v2";
    public static bool IsDelta(LiveFrame frame) => frame.Type == "points";
    public static byte[] Encode(LiveFrame frame)
    {
        var points = frame.Points ?? frame.Mark!.Points;
        var id = frame.Mark?.Id ?? frame.MarkId;
        if (frame.Offset <= 0 || id == Guid.Empty || points.Count is < 1 or > LiveProtocol.ChunkPoints)
            throw new InvalidDataException("Invalid point delta.");
        var bytes = new byte[42 + points.Count * 16];
        bytes[0] = 66; bytes[1] = 77; bytes[2] = 2; bytes[3] = 1;
        frame.Peer.TryWriteBytes(bytes.AsSpan(4, 16), true, out _);
        id.TryWriteBytes(bytes.AsSpan(20, 16), true, out _);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(36), frame.Offset);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(40), (ushort)points.Count);
        for (var i = 0; i < points.Count; i++)
        {
            BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(42 + i * 16), points[i].X);
            BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(50 + i * 16), points[i].Y);
        }
        return bytes;
    }
    public static LiveFrame Decode(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 4 && bytes[0] == 66 && bytes[1] == 77 && bytes[2] == 2 && bytes[3] == 2) return DecodeChange(bytes);
        if (bytes.Length < 42 || bytes[0] != 66 || bytes[1] != 77 || bytes[2] != 2 || bytes[3] != 1)
            throw new InvalidDataException("Invalid live packet signature.");
        var offset = BinaryPrimitives.ReadInt32LittleEndian(bytes[36..]);
        var count = BinaryPrimitives.ReadUInt16LittleEndian(bytes[40..]);
        var id = new Guid(bytes.Slice(20, 16), true);
        if (offset <= 0 || id == Guid.Empty || count is < 1 or > LiveProtocol.ChunkPoints || bytes.Length != 42 + count * 16)
            throw new InvalidDataException("Invalid live packet length/offset.");
        var points = new List<InkPointDto>(count);
        for (var i = 0; i < count; i++)
        {
            var x = BinaryPrimitives.ReadDoubleLittleEndian(bytes[(42 + i * 16)..]);
            var y = BinaryPrimitives.ReadDoubleLittleEndian(bytes[(50 + i * 16)..]);
            if (!double.IsFinite(x) || !double.IsFinite(y) || Math.Abs(x) > 100000 || Math.Abs(y) > 100000)
                throw new InvalidDataException("Invalid live coordinates.");
            points.Add(new(x, y));
        }
        return new() { Type = "points", Peer = new Guid(bytes.Slice(4, 16), true), MarkId = id, Points = points, Offset = offset };
    }
    public static byte[] EncodeChange(LiveFrame frame)
    {
        var patch = frame.Patch ?? throw new InvalidDataException("Missing saved patch.");
        var marks = patch.Add.Concat(patch.Update).ToArray();
        var metadata = patch with { Add = patch.Add.Select(m => m with { Points = [] }).ToList(),
            Update = patch.Update.Select(m => m with { Points = [] }).ToList() };
        var json = JsonSerializer.SerializeToUtf8Bytes(metadata, BoardJsonContext.Default.BoardPatch);
        var bytes = new byte[24 + json.Length + marks.Sum(m => 2 + m.Points.Count * 16)];
        bytes[0] = 66; bytes[1] = 77; bytes[2] = 2; bytes[3] = 2;
        BinaryPrimitives.WriteInt64LittleEndian(bytes.AsSpan(4), frame.BaseRevision);
        BinaryPrimitives.WriteInt64LittleEndian(bytes.AsSpan(12), frame.Revision);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(20), json.Length); json.CopyTo(bytes, 24);
        var offset = 24 + json.Length;
        foreach (var mark in marks)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset), checked((ushort)mark.Points.Count)); offset += 2;
            foreach (var point in mark.Points)
            {
                BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(offset), point.X);
                BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(offset + 8), point.Y); offset += 16;
            }
        }
        return bytes;
    }
    private static LiveFrame DecodeChange(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 24) throw new InvalidDataException("Truncated saved patch.");
        var length = BinaryPrimitives.ReadInt32LittleEndian(bytes[20..]);
        if (length <= 0 || length > bytes.Length - 24) throw new InvalidDataException("Invalid saved metadata length.");
        var patch = JsonSerializer.Deserialize(bytes.Slice(24, length), BoardJsonContext.Default.BoardPatch)
            ?? throw new InvalidDataException("Missing saved metadata.");
        if (patch.Add is null || patch.Update is null || patch.Add.Count + patch.Update.Count > BoardValidation.MaxMarks)
            throw new InvalidDataException("Invalid saved marks.");
        var offset = 24 + length;
        foreach (var list in new[] { patch.Add, patch.Update })
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] is null || offset > bytes.Length - 2) throw new InvalidDataException("Missing saved points.");
                var count = BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..]); offset += 2;
                if (count is < 1 or > 20000 || offset > bytes.Length - count * 16) throw new InvalidDataException("Invalid saved point count.");
                var points = new List<InkPointDto>(count);
                for (var j = 0; j < count; j++)
                {
                    points.Add(new(BinaryPrimitives.ReadDoubleLittleEndian(bytes[offset..]), BinaryPrimitives.ReadDoubleLittleEndian(bytes[(offset + 8)..])));
                    offset += 16;
                }
                list[i] = list[i] with { Points = points };
            }
        if (offset != bytes.Length || !BoardValidation.Valid(patch)) throw new InvalidDataException("Invalid saved patch.");
        var before = BinaryPrimitives.ReadInt64LittleEndian(bytes[4..]); var after = BinaryPrimitives.ReadInt64LittleEndian(bytes[12..]);
        if (before < 0 || before == long.MaxValue || after != before + 1) throw new InvalidDataException("Invalid saved revision.");
        return new() { Type = "changed", BaseRevision = before, Revision = after, Patch = patch };
    }
}
