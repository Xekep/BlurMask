using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>An opaque child envelope in one retained linguistic resource.
/// A wire layout does not deserialize, configure or select this child.</summary>
public sealed class LegacyLinguisticRoot13FWireChild
{
    internal LegacyLinguisticRoot13FWireLayout Owner { get; }
    public int Ordinal { get; }
    public int Offset { get; }
    public int End { get; }
    public uint Kind { get; }
    public ushort Version { get; }
    public int PaddingLength { get; }

    internal LegacyLinguisticRoot13FWireChild(LegacyLinguisticRoot13FWireLayout owner,
        int ordinal, int offset, int end, uint kind, ushort version, int padding)
    { Owner = owner; Ordinal = ordinal; Offset = offset; End = end; Kind = kind; Version = version; PaddingLength = padding; }
}

/// <summary>Immutable, bounded 13F/102 wire layout. Reads the native 133/100
/// prefix and sequential child envelopes. Licensing-related prefix fields stay
/// opaque data; this is not activation, source admission or a recognizer.</summary>
public sealed class LegacyLinguisticRoot13FWireLayout
{
    public const int MaximumResourceBytes = 64 * 1024 * 1024;
    public const int MaximumChildren = 4096;
    private readonly byte[] bytes;
    private readonly int prefixLength;
    public string Fingerprint { get; }
    public int End => bytes.Length;
    public ushort Version => 0x102;
    public IReadOnlyList<LegacyLinguisticRoot13FWireChild> Children { get; }

    private LegacyLinguisticRoot13FWireLayout(byte[] bytes, int prefixLength,
        List<(int Offset, int End, uint Kind, ushort Version, int Padding)> children)
    {
        this.bytes = bytes; this.prefixLength = prefixLength;
        Fingerprint = Convert.ToHexString(SHA256.HashData(bytes));
        Children = Array.AsReadOnly(children.Select((c, i) => new LegacyLinguisticRoot13FWireChild(
            this, i, c.Offset, c.End, c.Kind, c.Version, c.Padding)).ToArray());
    }

    /// <summary>Read one complete root file at offset zero. Nested stream origins,
    /// other versions and encoded envelopes need their own reader contract.</summary>
    public static LegacyLinguisticRoot13FWireLayout Read(ReadOnlySpan<byte> resource,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (resource.Length is < 88 or > MaximumResourceBytes)
            throw new InvalidDataException("Bounded complete linguistic root required.");
        // Own the complete bounded snapshot before parsing any child. Later
        // caller mutations cannot change a validated envelope or its identity.
        var owned = new byte[resource.Length];
        for (int at = 0; at < owned.Length; at += 65536)
        {
            token.ThrowIfCancellationRequested();
            resource.Slice(at, Math.Min(65536, owned.Length - at)).CopyTo(owned.AsSpan(at));
        }
        var root = Header(owned, 0, owned.Length);
        if (root.End != owned.Length || root.Kind != 0x13f || root.Version != 0x102)
            throw new NotSupportedException("Exact complete unencoded13F/102 wire contract required.");
        // Native133/100 reads four bytes at+10, fixed56-byte name, then a
        // sentinel. Only -1 consumes a separate raw4C; raw50 always follows.
        int atCount = BinaryPrimitives.ReadInt32LittleEndian(owned.AsSpan(76)) == -1 ? 88 : 84;
        if (atCount > owned.Length - 4) throw new InvalidDataException("Truncated133 prefix.");
        uint count = BinaryPrimitives.ReadUInt32LittleEndian(owned.AsSpan(atCount));
        int position = atCount + 4;
        if (count > MaximumChildren || count > (owned.Length - position) / 16)
            throw new InvalidDataException("Whole child count exceeds the envelope or budget.");
        var children = new List<(int Offset, int End, uint Kind, ushort Version, int Padding)>((int)count);
        for (int i = 0; i < count; i++)
        {
            token.ThrowIfCancellationRequested();
            var child = Header(owned, position, owned.Length);
            // Native aligns between children, never after the final child.
            int next = i + 1 < count ? (child.End + 3) & ~3 : child.End;
            if (next > owned.Length) throw new InvalidDataException("Child alignment exceeds root.");
            children.Add((position, child.End, child.Kind, child.Version, next - child.End));
            position = next;
        }
        if (position != owned.Length) throw new InvalidDataException("Unconsumed linguistic root bytes.");
        token.ThrowIfCancellationRequested();
        return new(owned, atCount - 16, children);
    }

    public byte[] CopyBase133Prefix() => bytes.AsSpan(16, prefixLength).ToArray();
    public byte[] CopyEnvelope() => bytes.ToArray();
    public byte[] CopyChildEnvelope(LegacyLinguisticRoot13FWireChild child)
    { RequireChild(child); return bytes.AsSpan(child.Offset, child.End - child.Offset).ToArray(); }
    public byte[] CopyChildPadding(LegacyLinguisticRoot13FWireChild child)
    { RequireChild(child); return bytes.AsSpan(child.End, child.PaddingLength).ToArray(); }

    private void RequireChild(LegacyLinguisticRoot13FWireChild child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (!ReferenceEquals(child.Owner, this))
            throw new ArgumentException("The exact retained child owner is required.", nameof(child));
    }
    private static (int End, uint Kind, ushort Version) Header(byte[] data, int offset, int limit)
    {
        if (offset < 0 || offset > limit - 16)
            throw new InvalidDataException("Truncated resource envelope.");
        var header = data.AsSpan(offset, 16);
        uint length = BinaryPrimitives.ReadUInt32LittleEndian(header[4..]);
        if (BinaryPrimitives.ReadUInt32LittleEndian(header) != 0x0101fe90 || length < 8 ||
            length > (uint)(limit - offset - 8))
            throw new InvalidDataException("Invalid resource envelope extent.");
        if (BinaryPrimitives.ReadUInt16LittleEndian(header[12..]) != 0)
            throw new NotSupportedException("Encoded envelopes are outside this wire reader.");
        return (offset + 8 + (int)length, BinaryPrimitives.ReadUInt32LittleEndian(header[8..]),
            BinaryPrimitives.ReadUInt16LittleEndian(header[14..]));
    }
}
