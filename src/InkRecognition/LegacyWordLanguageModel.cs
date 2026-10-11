using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>
/// Read-only stored lookup tables from the inspected type2d1/version101,
/// order-two, flags3 class-based word language resource. This is neither a
/// recognizer nor the native calibrated language search. Word IDs belong to
/// the caller; graph-state-to-word-ID correspondence is not inferred here.
/// </summary>
public sealed class LegacyWordLanguageModel
{
    private readonly byte[] _classes, _wordCodes, _unigramCodes, _bigramCodes;
    private readonly int[] _wordLevels, _unigramLevels, _bigramLevels;
    private readonly int _classBits;
    public int WordCount { get; }
    public int ClassCount { get; }
    public int DeclaredBigramCount { get; }
    /// <summary>First stored codebook level, not a scan of actual word frequencies.</summary>
    public int GetStoredInitialWordLevel() => _wordLevels[0];
    /// <summary>First stored unigram/bigram codebook level; no runtime history is selected.</summary>
    public int GetStoredInitialClassLevel(bool bigram) => (bigram ? _bigramLevels : _unigramLevels)[0];
    public int StorageBytes => _classes.Length + _wordCodes.Length + _unigramCodes.Length + _bigramCodes.Length +
        4 * (_wordLevels.Length + _unigramLevels.Length + _bigramLevels.Length);

    private LegacyWordLanguageModel(int words, int classes, int bigrams, int bits, byte[] packed,
        byte[] wordCodes, byte[] unigram, byte[] bigram, int[] wordLevels, int[] unigramLevels, int[] bigramLevels)
    {
        WordCount = words; ClassCount = classes; DeclaredBigramCount = bigrams; _classBits = bits;
        _classes = packed; _wordCodes = wordCodes; _unigramCodes = unigram; _bigramCodes = bigram;
        _wordLevels = wordLevels; _unigramLevels = unigramLevels; _bigramLevels = bigramLevels;
    }

    /// <summary>
    /// Parse exactly one bounded, complete stored object. Resource metadata is
    /// read as data; no engine, activation checks, authentication or patches run.
    /// The inspected profile has no activation identity requirement (field50=0).
    /// Other profiles and nonzero identity requirements are unsupported.
    /// </summary>
    public static LegacyWordLanguageModel Load(ReadOnlySpan<byte> data, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (data.Length is < 400 or > 16 * 1024 * 1024) throw Invalid("Language object length outside bounds.");
        var r = new Reader(data);
        if (r.U32() != 0x0101fe90 || r.U32() != data.Length - 8 || r.U32() != 0x2d1 || r.U32() != 0x01000000 || r.U16() != 0x100)
            throw Invalid("Unsupported language object header/base version.");
        r.Skip(4 + 56); // Stored base identity and metadata fields; not activation execution.
        if (r.U32() != uint.MaxValue || r.U32() != 0x01000000 || r.U32() != 0)
            throw Invalid("Unsupported base metadata encoding or identity requirement.");
        if (r.U32() != 2 || r.U32() != 3) throw Invalid("Only order2 flags3 language storage is restored.");
        int words = r.Count(1, 2_000_000), classes = r.Count(1, 2048);
        if (r.Count(1, 2048) != classes) throw Invalid("Unigram count differs from class count.");
        int bigrams = r.Count(0, checked(classes * classes));
        if (r.U32() != 0x01000000 || r.Count(1, 2_000_000) != words) throw Invalid("Unsupported word-class packed header.");
        int bits = r.Byte();
        if (bits is < 1 or > 11 || (1 << bits) < classes) throw Invalid("Invalid packed word-class width.");
        byte[] packed = r.Bytes(checked((int)(((long)words * bits + 31) / 32 * 4)));
        byte[] wordCodes = r.Bytes(words);
        int[] wordLevels = r.Ints(r.Count(1, 256));
        byte[] unigram = r.Bytes(classes), bigram = r.Bytes(checked(classes * classes));
        int uniLevels = r.Count(1, 256), biLevels = r.Count(1, 256);
        int[] unigramLevels = r.Ints(uniLevels), bigramLevels = r.Ints(biLevels);
        // No backoff table exists for this order2/flags3 profile. The remaining
        // calibration and 13-category punctuation tables have proven sizes but
        // are not interpreted by these raw lookup methods.
        if (r.U32() != 0 || r.U32() != 0) throw Invalid("Unsupported order2 backoff storage.");
        r.Skip(4 * 2 * 4 + 16 + 16 + 4 * 2 * 4 + 16 + 16);
        r.Skip(2 + 16 + 16 + 0x2a4 + 0x2254);
        if (!r.Finished) throw Invalid("Unconsumed language object bytes.");
        var result = new LegacyWordLanguageModel(words, classes, bigrams, bits, packed, wordCodes, unigram, bigram,
            wordLevels, unigramLevels, bigramLevels);
        for (int i = 0; i < words; i++)
        {
            if ((i & 1023) == 0) cancellationToken.ThrowIfCancellationRequested();
            if (result.GetWordClass(i) >= classes || wordCodes[i] >= wordLevels.Length)
                throw Invalid("Stored word lookup exceeds its table.");
        }
        ValidateCodes(unigram, uniLevels, cancellationToken); ValidateCodes(bigram, biLevels, cancellationToken);
        return result;
    }

    public int GetWordClass(int wordId)
    {
        ValidateWord(wordId);
        long bit = (long)wordId * _classBits;
        int offset = checked((int)(bit >> 5) * 4), shift = (int)(bit & 31);
        uint value = BinaryPrimitives.ReadUInt32LittleEndian(_classes.AsSpan(offset)) >> shift;
        if (shift + _classBits > 32)
            value |= BinaryPrimitives.ReadUInt32LittleEndian(_classes.AsSpan(offset + 4)) << (32 - shift);
        return (int)(value & ((1u << _classBits) - 1));
    }

    /// <summary>Uncalibrated stored per-word integer cost. No class cost is added.</summary>
    public int GetStoredWordCost(int wordId) { ValidateWord(wordId); return _wordLevels[_wordCodes[wordId]]; }

    /// <summary>
    /// Uncalibrated stored class integer cost. Null previous class uses the
    /// unigram table; otherwise the native dense row index is previous*C+current.
    /// Resource profiles must calibrate these costs before native-equivalent use.
    /// </summary>
    public int GetStoredClassCost(int currentClass, int? previousClass = null)
    {
        ValidateClass(currentClass);
        if (previousClass is null) return _unigramLevels[_unigramCodes[currentClass]];
        ValidateClass(previousClass.Value);
        return _bigramLevels[_bigramCodes[previousClass.Value * ClassCount + currentClass]];
    }

    private void ValidateWord(int id) { if ((uint)id >= WordCount) throw new ArgumentOutOfRangeException(nameof(id)); }
    private void ValidateClass(int id) { if ((uint)id >= ClassCount) throw new ArgumentOutOfRangeException(nameof(id)); }
    private static void ValidateCodes(byte[] codes, int count, CancellationToken token)
    {
        for (int i = 0; i < codes.Length; i++) { if ((i & 1023) == 0) token.ThrowIfCancellationRequested(); if (codes[i] >= count) throw Invalid("Stored class code exceeds level table."); }
    }
    private static InvalidDataException Invalid(string message) => new(message);

    private ref struct Reader
    {
        private readonly ReadOnlySpan<byte> _data;
        private int _cursor;
        public Reader(ReadOnlySpan<byte> data) { _data = data; _cursor = 0; }
        public bool Finished => _cursor == _data.Length;
        private ReadOnlySpan<byte> Take(int n)
        {
            if (n < 0 || n > _data.Length - _cursor) throw Invalid("Truncated language object.");
            var part = _data.Slice(_cursor, n); _cursor += n; return part;
        }
        public byte Byte() => Take(1)[0];
        public ushort U16() => BinaryPrimitives.ReadUInt16LittleEndian(Take(2));
        public uint U32() => BinaryPrimitives.ReadUInt32LittleEndian(Take(4));
        public int Count(int min, int max) { uint n = U32(); if (n < min || n > max) throw Invalid("Language count outside bounds."); return (int)n; }
        public byte[] Bytes(int n) => Take(n).ToArray();
        public void Skip(int n) => Take(n);
        public int[] Ints(int n)
        {
            var bytes = Take(checked(n * 4)); var result = new int[n];
            for (int i = 0; i < n; i++) result[i] = BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(i * 4, 4));
            return result;
        }
    }
}
