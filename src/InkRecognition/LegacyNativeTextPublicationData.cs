using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;

namespace InkRecognition;

/// <summary>Exact default script/grapheme/literal data used by Text019030/018b40, independent of host Unicode version.</summary>
public sealed class LegacyNativeTextPublicationData
{
    public const string ExpectedTableSha256 = "dd431ff067120a92b87cb35cb4d34253516740e4e0e9fedf841f8d258317edcc";
    private readonly int[][] _scripts, _controls, _extends, _prepends, _joiners;
    private readonly (byte[] Bytes, int[] Fields)[] _literals;
    private LegacyNativeTextPublicationData(int[][] scripts, int[][] controls, int[][] extends, int[][] prepends,
        int[][] joiners, (byte[], int[])[] literals)
    { _scripts = scripts; _controls = controls; _extends = extends; _prepends = prepends; _joiners = joiners; _literals = literals; }

    public static LegacyNativeTextPublicationData Load(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var bytes = new MemoryStream(); byte[] buffer = new byte[8192]; int read;
        while ((read = stream.Read(buffer)) != 0)
        { if (bytes.Length + read > 131072) throw new InvalidDataException("Publication model budget exceeded."); bytes.Write(buffer, 0, read); }
        using var json = JsonDocument.Parse(bytes.ToArray()); var root = json.RootElement;
        Require("format", "legacy-native-text-publication-data-v1");
        Require("engineSha256", "d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c");
        Require("textSha256", "18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f");
        Require("tableSha256", ExpectedTableSha256);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var scripts = Ranges("scriptRanges", 716, 3); var controls = Ranges("controlRanges", 19, 2);
        var extends = Ranges("extendRanges", 225, 2); var prepends = Ranges("prependRanges", 11, 2);
        var joiners = Ranges("joinerRanges", 77, 2);
        var literals = new (byte[], int[])[120]; var rows = root.GetProperty("literalRows");
        if (rows.GetArrayLength() != 120) throw new InvalidDataException("Expected120 native literal rows.");
        Hash(120);
        int literalIndex = 0; var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in rows.EnumerateArray())
        {
            if (row.GetArrayLength() != 6) throw new InvalidDataException("Invalid literal row.");
            byte[] value = Convert.FromHexString(row[0].GetString()!); int[] fields = row.EnumerateArray().Skip(1).Select(v => v.GetInt32()).ToArray();
            int classification = fields[0];
            if (value.Length is < 1 or > 64 || value.AsSpan().Contains((byte)0) || classification < 0 || classification > 3)
                throw new InvalidDataException("Invalid bounded native literal.");
            Hash(value.Length); hash.AppendData(value); foreach (int field in fields) Hash(field);
            for (int i = 1; i < 5; i++) if (literalIndex + fields[i] is < 0 or >= 120) throw new InvalidDataException("Literal relative bounds differ.");
            if (!seen.Add(Convert.ToHexString(value))) throw new InvalidDataException("Duplicate literal.");
            literals[literalIndex++] = (value, fields);
        }
        if (Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant() != ExpectedTableSha256)
            throw new InvalidDataException("Publication numeric checksum differs.");
        return new(scripts, controls, extends, prepends, joiners, literals);

        void Require(string property, string value)
        { if (root.GetProperty(property).GetString() != value) throw new InvalidDataException("Publication provenance differs: " + property); }
        void Hash(int value) { Span<byte> b = stackalloc byte[4]; BinaryPrimitives.WriteInt32LittleEndian(b, value); hash.AppendData(b); }
        int[][] Ranges(string property, int count, int width)
        {
            var values = root.GetProperty(property); if (values.GetArrayLength() != count) throw new InvalidDataException("Range count differs.");
            Hash(count); var result = new int[count][]; int last = -1, index = 0;
            foreach (var row in values.EnumerateArray())
            {
                if (row.GetArrayLength() != width) throw new InvalidDataException("Range width differs.");
                int[] fields = row.EnumerateArray().Select(x => x.GetInt32()).ToArray();
                if (fields[0] <= last || fields[1] < fields[0] || fields[1] > 0x10ffff || (width == 3 && (fields[2] is < 1 or > 147)))
                    throw new InvalidDataException("Native range order/value differs.");
                foreach (int field in fields) Hash(field);
                last = fields[1]; result[index++] = fields;
            }
            return result;
        }
    }

    /// <summary>Engine05f290 native script ID. Missing entries return0; this is not a general Unicode category.</summary>
    public int ReadScript(int codePoint) { Validate(codePoint); int index = Search(_scripts, codePoint); return index < 0 ? 0 : _scripts[index][2]; }
    public int ReadGraphemeProperty(int codePoint)
    {
        Validate(codePoint);
        if (codePoint == 13) return 1; if (codePoint == 10) return 2;
        if (codePoint is >= 0x1100 and < 0x1113) return 0x10;
        if (codePoint is >= 0x1161 and <= 0x1175) return 0x20;
        if (codePoint is >= 0x11a7 and < 0x11c3) return 0x40;
        if (codePoint is >= 0xac00 and <= 0xd7a3) return (codePoint - 0xac00) % 28 == 0 ? 0x80 : 0x100;
        if (Search(_controls, codePoint) >= 0) return 4;
        if (Search(_extends, codePoint) >= 0) return 8;
        return Search(_prepends, codePoint) >= 0 ? 0x200 : 0;
    }
    internal bool InJoinerRange(int codePoint) => Search(_joiners, codePoint) >= 0;
    /// <summary>018b40 uses NUL-terminated lookup even when the interned interval contains embedded zero bytes.</summary>
    public int ReadLiteralClass(ReadOnlySpan<byte> utf8)
    {
        int zero = utf8.IndexOf((byte)0); if (zero >= 0) utf8 = utf8[..zero];
        if (utf8.Length > 64) return -1;
        int lo = 0, hi = 120, depth = 0;
        while (lo < hi)
        {
            int mid = (lo + hi) / 2; var row = _literals[mid];
            byte target = depth < utf8.Length ? utf8[depth] : (byte)0;
            byte expected = depth < row.Bytes.Length ? row.Bytes[depth] : (byte)0;
            if (target < expected) hi = mid;
            else if (target > expected) lo = mid + 1;
            else if (depth < 2 && target != 0)
            { lo = mid + row.Fields[1 + depth * 2]; hi = mid + row.Fields[2 + depth * 2] + 1; depth++; }
            else return utf8[depth..].SequenceEqual(row.Bytes.AsSpan(depth)) ? row.Fields[0] : -1;
        }
        return -1;
    }
    private static int Search(int[][] ranges, int cp)
    {
        int lo = 0, hi = ranges.Length;
        while (lo < hi) { int mid = (lo + hi) / 2; var r = ranges[mid]; if (cp < r[0]) hi = mid; else if (cp > r[1]) lo = mid + 1; else return mid; }
        return -1;
    }
    private static void Validate(int cp) { if ((uint)cp > 0x10ffff) throw new ArgumentOutOfRangeException(nameof(cp)); }
}
