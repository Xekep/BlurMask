using System.Buffers.Binary;
internal static class OwnWordFixture
{
public static byte[] Create()
{
    using var stream = new MemoryStream(); using var w = new BinaryWriter(stream);
    w.Write(0x0101fe90u); w.Write(0u); w.Write(0x2d1u); w.Write(0x01000000u); w.Write((ushort)0x100);
    w.Write(0u); w.Write(new byte[56]); w.Write(uint.MaxValue); w.Write(0x01000000u); w.Write(0u);
    w.Write(2u); w.Write(3u); w.Write(41u); w.Write(5u); w.Write(5u); w.Write(17u);
    w.Write(0x01000000u); w.Write(41u); w.Write((byte)3);
    var packed = new byte[16];
    for (int i = 0; i < 41; i++)
        for (int bit = 0; bit < 3; bit++) if (((i * 7 % 5) & (1 << bit)) != 0) packed[(i * 3 + bit) / 8] |= (byte)(1 << ((i * 3 + bit) % 8));
    w.Write(packed); w.Write(Enumerable.Range(0, 41).Select(i => (byte)(i % 3)).ToArray());
    w.Write(3u); foreach (int n in new[] { 100, 300, 800 }) w.Write(n);
    w.Write(Enumerable.Range(0, 5).Select(i => (byte)(i % 3)).ToArray());
    w.Write(Enumerable.Range(0, 25).Select(i => (byte)(i % 3)).ToArray());
    w.Write(3u); w.Write(3u); foreach (int n in new[] { 11, 23, 47, 102, 501, 903 }) w.Write(n);
    w.Write(0u); w.Write(0u); w.Write(new byte[4 * 2 * 4 + 16 + 16 + 4 * 2 * 4 + 16 + 16]);
    w.Write(new byte[2 + 16 + 16 + 0x2a4 + 0x2254]);
    byte[] result = stream.ToArray(); BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(4), (uint)result.Length - 8); return result;
}

}
