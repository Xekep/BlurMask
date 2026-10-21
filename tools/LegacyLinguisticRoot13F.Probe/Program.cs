using InkRecognition;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;

if (args.Length != 2) throw new ArgumentException("MODELS CAPTURE");
var rows = new List<(string Name, LegacyLinguisticRoot13FWireLayout Root)>(); int guards = 0;
void Refuse(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new Exception("Expected whole layout refusal.");
}
string Sha(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
void Capture(string name, byte[] data)
{
    var root = LegacyLinguisticRoot13FWireLayout.Read(data);
    byte[] envelope = root.CopyEnvelope();
    if (!envelope.SequenceEqual(data)) throw new Exception("Owned root bytes differ.");
    rows.Add((name, root));
    Array.Fill(data, (byte)0); Array.Fill(envelope, (byte)0);
    if (Sha(root.CopyEnvelope()) != root.Fingerprint.ToLowerInvariant()) throw new Exception("Caller mutated retained bytes.");
    var prefix = root.CopyBase133Prefix(); prefix[0] ^= 255;
    if (Sha(prefix) == Sha(root.CopyBase133Prefix())) throw new Exception("Prefix copy aliases retained state.");
    var other = LegacyLinguisticRoot13FWireLayout.Read(root.CopyEnvelope());
    if (root.Children.Count != 0)
    {
        Refuse(() => root.CopyChildEnvelope(other.Children[0]));
        Refuse(() => root.CopyChildPadding(other.Children[0]));
        var first = root.Children[0]; byte[] child = root.CopyChildEnvelope(first); child[0] ^= 255;
        if (Sha(child) == Sha(root.CopyChildEnvelope(first))) throw new Exception("Child copy aliases retained state.");
    }
}
foreach (string file in new[] { "ru-RU/ru_RU-lk-text.lite.res", "en-US/en_US-lk-text.lite.res" })
    Capture(file, File.ReadAllBytes(Path.Combine(args[0], file)));
foreach (bool extended in new[] { false, true })
    foreach (int count in new[] { 0, 1, 3 }) Capture($"procedural-{extended}-{count}", Build(extended, count));
Capture("procedural-False-4096", Build(false, LegacyLinguisticRoot13FWireLayout.MaximumChildren));

byte[] valid = Build(true, 3);
void Mutate(int offset, Action<byte[], int> change)
{ byte[] copy = valid.ToArray(); change(copy, offset); Refuse(() => LegacyLinguisticRoot13FWireLayout.Read(copy)); }
Mutate(0, (b, i) => b[i] ^= 1);
Mutate(4, (b, i) => BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(i), uint.MaxValue));
Mutate(8, (b, i) => BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(i), 0x247));
Mutate(12, (b, i) => BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(i), 1));
Mutate(14, (b, i) => BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(i), 0x101));
Mutate(88, (b, i) => BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(i), 4097));
Mutate(88, (b, i) => BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(i), uint.MaxValue));
Mutate(88, (b, i) => BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(i), 2));
Mutate(92, (b, i) => b[i] ^= 1);
Mutate(96, (b, i) => BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(i), 7));
Mutate(96, (b, i) => BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(i), uint.MaxValue));
Mutate(104, (b, i) => BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(i), 1));
foreach (int size in new[] { 0, 7, 15, 76, 79, 83, 87, 91, valid.Length - 1 })
    Refuse(() => LegacyLinguisticRoot13FWireLayout.Read(valid.AsSpan(0, size)));
byte[] trailing = valid.Concat(new byte[] { 1 }).ToArray();
BinaryPrimitives.WriteUInt32LittleEndian(trailing.AsSpan(4), (uint)(trailing.Length - 8));
Refuse(() => LegacyLinguisticRoot13FWireLayout.Read(trailing));
byte[] missingExtendedCount = Build(true, 0)[..88];
BinaryPrimitives.WriteUInt32LittleEndian(missingExtendedCount.AsSpan(4), 80);
Refuse(() => LegacyLinguisticRoot13FWireLayout.Read(missingExtendedCount));
Refuse(() => LegacyLinguisticRoot13FWireLayout.Read(new byte[LegacyLinguisticRoot13FWireLayout.MaximumResourceBytes + 1]));
using (var cancel = new CancellationTokenSource())
{ cancel.Cancel(); Refuse(() => LegacyLinguisticRoot13FWireLayout.Read(valid, cancel.Token)); }
using (var output = File.Create(args[1]))
using (var w = new Utf8JsonWriter(output))
{
    w.WriteStartObject(); w.WriteString("Scope", "Opaque13F wire layout only; no selection, activation or recognition");
    w.WriteNumber("Guards", guards); w.WriteBoolean("TargetsRead", false); w.WriteStartArray("Rows");
    foreach (var (name, root) in rows)
    {
        w.WriteStartObject(); w.WriteString("Name", name); w.WriteNumber("End", root.End); w.WriteNumber("Version", root.Version);
        w.WriteString("Fingerprint", root.Fingerprint.ToLowerInvariant());
        w.WriteNumber("Base133Length", root.CopyBase133Prefix().Length); w.WriteString("Base133Sha256", Sha(root.CopyBase133Prefix()));
        w.WriteStartArray("Children");
        foreach (var c in root.Children)
        {
            w.WriteStartObject(); w.WriteNumber("Ordinal", c.Ordinal); w.WriteNumber("Offset", c.Offset); w.WriteNumber("End", c.End);
            w.WriteNumber("Kind", c.Kind); w.WriteNumber("Version", c.Version); w.WriteNumber("PaddingLength", c.PaddingLength);
            w.WriteString("Sha256", Sha(root.CopyChildEnvelope(c))); w.WriteString("PaddingHex", Convert.ToHexStringLower(root.CopyChildPadding(c)));
            w.WriteEndObject();
        }
        w.WriteEndArray(); w.WriteEndObject();
    }
    w.WriteEndArray(); w.WriteEndObject();
}
Console.WriteLine($"PASS {rows.Count} root layouts, {guards} ownership/malformed/cancel guards; no source selection or recognition");

static byte[] Build(bool extended, int count)
{
    using var stream = new MemoryStream(); using var w = new BinaryWriter(stream);
    w.Write(0x0101fe90u); w.Write(0u); w.Write(0x13fu); w.Write((ushort)0); w.Write((ushort)0x102);
    w.Write(0x12345678u); w.Write(Enumerable.Range(0, 56).Select(i => (byte)i).ToArray());
    w.Write(extended ? -1 : 0x1234567); if (extended) w.Write(0x76543210u); w.Write(0x99aabbccu); w.Write((uint)count);
    for (int i = 0; i < count; i++)
    {
        int payload = i + 1;
        w.Write(0x0101fe90u); w.Write((uint)(8 + payload)); w.Write((uint)(0xf000 + i)); w.Write((ushort)0); w.Write((ushort)(0x100 + i));
        for (int j = 0; j < payload; j++) w.Write((byte)(0x40 + j));
        if (i + 1 < count) while ((stream.Position & 3) != 0) w.Write((byte)0xff);
    }
    w.Flush(); stream.Position = 4; w.Write((uint)(stream.Length - 8)); w.Flush(); return stream.ToArray();
}
