using System.Buffers.Binary;
using System.Text;

namespace InkRecognition;

/// <summary>Data-only source133 identifier prefix; does not activate or instantiate a vendor resource.</summary>
public static class LegacyAcousticSourceIdentifier
{
    /// <summary>
    /// Read the observed247/v10d →134/v201 →133/v100 prefix. Text1d2680's
    /// FFFFFFFF marker selects the full56-byte XOR branch of1d22e0. Native
    /// source+14 is a NUL-terminated byte string; bytes after its first NUL
    /// are not part of the name. Later acoustic fields are not loaded here.
    /// </summary>
    public static string Read247V10d(ReadOnlySpan<byte> resource)
    {
        if (resource.Length is < 99 or > 64 * 1024 * 1024 ||
            U32(resource, 0) != 0x0101fe90 || U32(resource, 4) != resource.Length - 8 ||
            U32(resource, 8) != 0x247 || U32(resource, 12) != 0x010d0000 ||
            U16(resource, 16) != 0x201 || U16(resource, 18) != 0x100 ||
            U32(resource, 20) != 0x00e17557 || U32(resource, 80) != uint.MaxValue ||
            U32(resource, 93) != 0x2b || U16(resource, 97) != 0x103)
            throw new InvalidDataException("Unsupported observed acoustic source133 identifier prefix.");
        Span<byte> decoded = stackalloc byte[56];
        ReadOnlySpan<byte> key = [0x14, 0x12, 0x19, 0x77];
        for (int i = 0; i < decoded.Length; i++) decoded[i] = (byte)(resource[24 + i] ^ key[i & 3]);
        int end = decoded.IndexOf((byte)0);
        if (end < 0) throw new InvalidDataException("Source133 identifier has no bounded NUL terminator.");
        try { return new UTF8Encoding(false, true).GetString(decoded[..end]); }
        catch (DecoderFallbackException e) { throw new InvalidDataException("Invalid source133 identifier UTF8.", e); }
    }

    /// <summary>
    /// Engine service37.virtual58=089ad0: ordinal byte PREFIX comparison,
    /// with the two implicit-length input strings terminated at their first
    /// NUL. This differs from substring service37.virtual10=089680.
    /// </summary>
    public static bool HasBytePrefix(ReadOnlySpan<byte> identifier, ReadOnlySpan<byte> prefix)
    {
        if (identifier.Length > 65535 || prefix.Length > 65535)
            throw new ArgumentException("Bounded source identifier byte strings required.");
        int end = identifier.IndexOf((byte)0); if (end >= 0) identifier = identifier[..end];
        end = prefix.IndexOf((byte)0); if (end >= 0) prefix = prefix[..end];
        return identifier.StartsWith(prefix);
    }

    private static ushort U16(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..]);
    private static uint U32(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(bytes[offset..]);
}
