using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;

namespace InkRecognition;

/// <summary>
/// Portable exact default Engine+78 data for5ebb0 combining classes,5ed00 canonical decomposition and5ec00 composition.
/// This does not run0491a0's stream ordering/normalization/circled-marker lifecycle.
/// </summary>
public sealed class LegacyNativeUnicodeNormalizationData
{
    public const string ExpectedTableSha256 = "7a0cd7a1a1a931466a868718394b90dc591726bad93909f7fbe5e5ea0fa2a885";
    private readonly (int First, int Last, int Class)[] _ranges;
    private readonly Dictionary<int, int[]> _decompositions;
    private readonly Dictionary<ulong, int> _compositions;
    private LegacyNativeUnicodeNormalizationData((int, int, int)[] ranges, Dictionary<int, int[]> decompositions,
        Dictionary<ulong, int> compositions) { _ranges = ranges; _decompositions = decompositions; _compositions = compositions; }

    public static LegacyNativeUnicodeNormalizationData Load(Stream model)
    {
        ArgumentNullException.ThrowIfNull(model);
        using var bytes = new MemoryStream(); byte[] buffer = new byte[8192]; int read;
        while ((read = model.Read(buffer)) != 0)
        {
            if (bytes.Length + read > 1_048_576) throw new InvalidDataException("Portable Unicode model budget exceeded.");
            bytes.Write(buffer, 0, read);
        }
        using var json = JsonDocument.Parse(bytes.ToArray()); var root = json.RootElement;
        Require(root, "format", "legacy-native-unicode-normalization-data-v1"); Require(root, "unicodeVersion", "9.0.0");
        Require(root, "engineSha256", "d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c");
        Require(root, "unicodeDataSha256", "68dfc414d28257b9b5d6ddbb8b466c768c00ebdf6cbf7784364a9b6cad55ee8f");
        Require(root, "derivedNormalizationSha256", "02d8e12cea7c61a9f3cc5fbf8eaceadf8da23fe83d60c64cf647088830b810cc");
        Require(root, "tableSha256", ExpectedTableSha256);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var rangeRows = root.GetProperty("combiningRanges");
        if (rangeRows.GetArrayLength() != 517) throw new InvalidDataException("Expected exact517 native CCC ranges.");
        Hash(hash, 517); var ranges = new (int First, int Last, int Class)[517]; int previous = -1, index = 0;
        foreach (var row in rangeRows.EnumerateArray())
        {
            if (row.GetArrayLength() != 3) throw new InvalidDataException("Invalid CCC row width.");
            int first = row[0].GetInt32(), last = row[1].GetInt32(), ccc = row[2].GetInt32();
            if (first <= previous || last < first || last > 0x10ffff || (uint)ccc > 255) throw new InvalidDataException("Invalid ordered native CCC range.");
            Hash(hash, first); Hash(hash, last); Hash(hash, ccc); ranges[index++] = (first, last, ccc); previous = last;
        }
        var decompositionRows = root.GetProperty("canonicalDecompositions");
        if (decompositionRows.GetArrayLength() != 2060) throw new InvalidDataException("Expected exact2060 canonical decomposition rows.");
        Hash(hash, 2060); previous = -1; var decompositions = new Dictionary<int, int[]>(2060);
        foreach (var row in decompositionRows.EnumerateArray())
        {
            if (row.GetArrayLength() < 3) throw new InvalidDataException("Invalid decomposition row.");
            int cp = row[0].GetInt32(), count = row[1].GetInt32();
            if (cp <= previous || cp > 0x10ffff || count is < 1 or > 18 || row.GetArrayLength() != count + 2)
                throw new InvalidDataException("Invalid ordered bounded canonical decomposition.");
            Hash(hash, cp); Hash(hash, count); var parts = new int[count];
            for (int i = 0; i < count; i++)
            {
                int part = row[i + 2].GetInt32();
                if ((uint)part > 0x10ffff || part is >= 0xd800 and <= 0xdfff) throw new InvalidDataException("Invalid decomposition scalar.");
                parts[i] = part; Hash(hash, part);
            }
            decompositions.Add(cp, parts); previous = cp;
        }
        var compositionRows = root.GetProperty("compositions");
        if (compositionRows.GetArrayLength() != 940) throw new InvalidDataException("Expected exact940 composition rows.");
        Hash(hash, 940); var compositions = new Dictionary<ulong, int>(940); ulong previousKey = 0; bool firstRow = true;
        foreach (var row in compositionRows.EnumerateArray())
        {
            if (row.GetArrayLength() != 3) throw new InvalidDataException("Invalid composition row width.");
            int first = row[0].GetInt32(), second = row[1].GetInt32(), result = row[2].GetInt32();
            if ((uint)first > 0x10ffff || (uint)second > 0x10ffff || (uint)result > 0x10ffff) throw new InvalidDataException("Invalid composition code point.");
            ulong key = Key(first, second);
            if (!firstRow && key <= previousKey) throw new InvalidDataException("Native composition pairs are not strictly ordered.");
            Hash(hash, first); Hash(hash, second); Hash(hash, result); compositions.Add(key, result);
            firstRow = false; previousKey = key;
        }
        if (Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant() != ExpectedTableSha256)
            throw new InvalidDataException("Portable Unicode table checksum differs.");
        return new(ranges, decompositions, compositions);
    }

    /// <summary>Exact5ebb0: gaps return−1, not the host framework's default combining class0.</summary>
    public int ReadCombiningClass(int codePoint)
    {
        Validate(codePoint); int lo = 0, hi = _ranges.Length;
        while (lo < hi)
        {
            int middle = (lo + hi) / 2; var range = _ranges[middle];
            if (codePoint < range.First) hi = middle;
            else if (codePoint > range.Last) lo = middle + 1;
            else return range.Class;
        }
        return -1;
    }

    /// <summary>5ed00 canonical-only direct lookup, including037E/0387 refusals and algorithmic Hangul. Returned array is owned.</summary>
    public bool TryReadDirectCanonicalDecomposition(int codePoint, out int[] components)
    {
        Validate(codePoint); components = [];
        if (codePoint is 0x37e or 0x387) return false;
        uint syllable = unchecked((uint)(codePoint - 0xac00));
        if (syllable < 0x2ba4)
        {
            int trailing = (int)syllable % 28;
            int leading = (int)syllable / 588 + 0x1100, vowel = (int)syllable % 588 / 28 + 0x1161;
            components = trailing == 0 ? [leading, vowel] : [leading, vowel, trailing + 0x11a7]; return true;
        }
        if (!_decompositions.TryGetValue(codePoint, out var stored)) return false;
        components = (int[])stored.Clone(); return true;
    }

    /// <summary>Exact5ec00 prepared scalar pair lookup with the two algorithmic Hangul branches.</summary>
    public bool TryCompose(int first, int second, out int result)
    {
        Validate(first); Validate(second);
        if (unchecked((uint)(first - 0x1100)) < 19 && unchecked((uint)(second - 0x1161)) < 21)
        { result = ((first - 0x1100) * 21 + second - 0x1161) * 28 + 0xac00; return true; }
        uint syllable = unchecked((uint)(first - 0xac00));
        if (syllable < 0x2ba4 && syllable % 28 == 0 && unchecked((uint)(second - 0x11a8)) < 27)
        { result = first + second - 0x11a7; return true; }
        if (_compositions.TryGetValue(Key(first, second), out result)) return true;
        result = -1; return false;
    }

    private static ulong Key(int first, int second) => ((ulong)(uint)first << 32) | (uint)second;
    private static void Validate(int cp) { if ((uint)cp > 0x10ffff) throw new ArgumentOutOfRangeException(nameof(cp)); }
    private static void Require(JsonElement root, string key, string value)
    { if (root.GetProperty(key).GetString() != value) throw new InvalidDataException("Pinned Unicode model metadata differs: " + key); }
    private static void Hash(IncrementalHash hash, int value)
    { Span<byte> bytes = stackalloc byte[4]; BinaryPrimitives.WriteInt32LittleEndian(bytes, value); hash.AppendData(bytes); }
}
