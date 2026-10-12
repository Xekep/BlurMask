using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace InkRecognition;

/// <summary>Portable static character-line catalog. Loading requires no vendor PE, DLL or SDK.</summary>
public sealed class LegacyCharacterLineCategoryModel
{
    public const string ExpectedSourceSha256 = "17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f";
    public const string ExpectedTableSha256 = "066c999f557389e2c286a8d88f40d5d3bf4e76f00abc7e63fa43823a187211fc";
    public string SourceSha256 => ExpectedSourceSha256;
    public string TableSha256 => ExpectedTableSha256;
    public int EntryCount => 3923;
    public int ProfileCount => 45;
    public int CyrillicEntryCount => 122;
    public LegacyPreparedCharacterLineCategoryCatalog Catalog { get; }
    private LegacyCharacterLineCategoryModel(LegacyPreparedCharacterLineCategoryCatalog catalog) => Catalog = catalog;

    /// <summary>Reads the version1 portable JSON with full ordered-table checksum. Caller retains stream ownership.</summary>
    public static LegacyCharacterLineCategoryModel Load(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream); cancellationToken.ThrowIfCancellationRequested();
        const int budget = 1024 * 1024;
        using var bytes = new MemoryStream();
        Span<byte> buffer = stackalloc byte[8192];
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int read = stream.Read(buffer);
            if (read == 0) break;
            if (bytes.Length + read > budget) throw new InvalidDataException("Character-line model exceeds 1MiB budget.");
            bytes.Write(buffer[..read]);
        }
        try
        {
            using var doc = JsonDocument.Parse(bytes.GetBuffer().AsMemory(0, checked((int)bytes.Length)),
                new JsonDocumentOptions { MaxDepth = 16 });
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) throw new InvalidDataException("Character-line model object required.");
            var properties = new HashSet<string>(StringComparer.Ordinal);
            foreach (var p in root.EnumerateObject())
                if (!properties.Add(p.Name)) throw new InvalidDataException("Duplicate character-line model property.");
            if (root.GetProperty("format").GetString() != "blurmask-character-line-categories" ||
                root.GetProperty("version").GetInt32() != 1 ||
                root.GetProperty("sourceBinary").GetString() != "MyScriptDocument.dll" ||
                root.GetProperty("sourceSha256").GetString() != ExpectedSourceSha256 ||
                root.GetProperty("tableSha256").GetString() != ExpectedTableSha256)
                throw new InvalidDataException("Unsupported or unpinned character-line model.");
            var entryJson = root.GetProperty("entries"); var profileJson = root.GetProperty("profiles");
            if (entryJson.GetArrayLength() != 3923 || profileJson.GetArrayLength() != 45)
                throw new InvalidDataException("Character-line table cardinality changed.");
            var entries = new LegacyCharacterLineCategoryEntry[3923];
            var profiles = new LegacyCharacterVerticalCategory[45];
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            var utf8 = new UTF8Encoding(false, true);
            Span<byte> numeric = stackalloc byte[12]; Span<byte> textBuffer = stackalloc byte[512];
            BinaryPrimitives.WriteUInt32LittleEndian(numeric, 3923); hash.AppendData(numeric[..4]);
            var keys = new HashSet<string>(StringComparer.Ordinal);
            int index = 0, cyrillic = 0;
            foreach (var item in entryJson.EnumerateArray())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (item.GetArrayLength() != 3) throw new InvalidDataException("Character-line entry shape changed.");
                string text = item[0].GetString() ?? throw new InvalidDataException("Null character-line symbol.");
                int main = item[1].GetInt32(), alternate = item[2].GetInt32();
                if (text.Length is < 1 or > 128 || text.Contains('\0') || main is < -1 or >= 45 || alternate is < -1 or >= 45 || !keys.Add(text))
                    throw new InvalidDataException("Invalid or duplicate character-line category entry.");
                int length = utf8.GetBytes(text.AsSpan(), textBuffer);
                BinaryPrimitives.WriteUInt32LittleEndian(numeric, (uint)length); hash.AppendData(numeric[..4]);
                hash.AppendData(textBuffer[..length]);
                numeric[0] = unchecked((byte)(sbyte)main); numeric[1] = unchecked((byte)(sbyte)alternate);
                hash.AppendData(numeric[..2]);
                if (text.Any(ch => ch is >= '\u0400' and <= '\u04ff')) cyrillic++;
                entries[index++] = new(text, (sbyte)main, (sbyte)alternate);
            }
            if (cyrillic != 122) throw new InvalidDataException("Character-line Cyrillic catalog cardinality changed.");
            BinaryPrimitives.WriteUInt32LittleEndian(numeric, 45); hash.AppendData(numeric[..4]);
            index = 0;
            foreach (var item in profileJson.EnumerateArray())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (item.GetArrayLength() != 3) throw new InvalidDataException("Character-line profile shape changed.");
                float offset = item[0].GetSingle(), span = item[1].GetSingle(), reserved = item[2].GetSingle();
                if (!float.IsFinite(offset) || !float.IsFinite(span) || BitConverter.SingleToInt32Bits(reserved) != 0)
                    throw new InvalidDataException("Invalid character-line category profile.");
                BinaryPrimitives.WriteInt32LittleEndian(numeric, BitConverter.SingleToInt32Bits(offset));
                BinaryPrimitives.WriteInt32LittleEndian(numeric[4..], BitConverter.SingleToInt32Bits(span));
                BinaryPrimitives.WriteInt32LittleEndian(numeric[8..], BitConverter.SingleToInt32Bits(reserved));
                hash.AppendData(numeric);
                profiles[index++] = new(offset, span);
            }
            if (Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant() != ExpectedTableSha256)
                throw new InvalidDataException("Character-line ordered table checksum mismatch.");
            return new(new(entries, profiles));
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or FormatException or
            OverflowException or EncoderFallbackException or ArgumentException)
        { throw new InvalidDataException("Malformed portable character-line model.", ex); }
    }
}
