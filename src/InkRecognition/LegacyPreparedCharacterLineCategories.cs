namespace InkRecognition;

/// <summary>One explicit Document04efe0 UTF8-category catalog entry. No Unicode classification is inferred.</summary>
public readonly record struct LegacyCharacterLineCategoryEntry(string Text, sbyte MainCategory, sbyte AlternateCategory);

/// <summary>Two effective floats in a native 12-byte boundary table row; its reserved third float is not used.</summary>
public readonly record struct LegacyCharacterVerticalCategory(float BaselineOffset, float VerticalSpan);

/// <summary>
/// Bounded Document04efe0 catalog-to-alphabet preparation. Catalog data and actual resource alphabet are explicit;
/// this does not fabricate symbol labels, recognition categories or per-character stroke geometry.
/// </summary>
public sealed class LegacyPreparedCharacterLineCategoryCatalog
{
    private static readonly System.Text.UTF8Encoding StrictUtf8 = new(false, true);
    private readonly Dictionary<string, (sbyte Main, sbyte Alternate)> _entries = new(StringComparer.Ordinal);
    private readonly LegacyCharacterVerticalCategory[] _profiles;

    public LegacyPreparedCharacterLineCategoryCatalog(ReadOnlySpan<LegacyCharacterLineCategoryEntry> entries,
        ReadOnlySpan<LegacyCharacterVerticalCategory> profiles)
    {
        if (entries.Length > 8192 || profiles.IsEmpty || profiles.Length > 127)
            throw new ArgumentException("Explicit prepared category catalog budgets exceeded.");
        _profiles = profiles.ToArray();
        foreach (var p in _profiles)
            if (!float.IsFinite(p.BaselineOffset) || !float.IsFinite(p.VerticalSpan))
                throw new ArgumentException("Finite prepared boundary category floats required.");
        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.Text) || entry.Text.Length > 128 || entry.Text.Contains('\0') ||
                entry.MainCategory < -1 || entry.AlternateCategory < -1 || entry.MainCategory >= profiles.Length ||
                entry.AlternateCategory >= profiles.Length)
                throw new ArgumentException("Explicit UTF8 symbol text and valid signed category indices required.");
            _ = StrictUtf8.GetByteCount(entry.Text);
            // Native hash-entry producer overwrites category bytes if an existing text key is revisited.
            _entries[entry.Text] = (entry.MainCategory, entry.AlternateCategory);
        }
    }

    public LegacyPreparedCharacterLineAlphabetCategories BindAlphabet(ReadOnlySpan<string> symbols,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (symbols.Length >= ushort.MaxValue) throw new ArgumentException("Alphabet must leave FFFF as unresolved sentinel.");
        var categories = new (sbyte Main, sbyte Alternate)[symbols.Length];
        for (int i = 0; i < symbols.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(symbols[i]) || symbols[i].Length > 128 || symbols[i].Contains('\0'))
                throw new ArgumentException("Actual resource alphabet requires bounded nonempty symbol text.");
            _ = StrictUtf8.GetByteCount(symbols[i]);
            categories[i] = _entries.TryGetValue(symbols[i], out var value) ? value : ((sbyte)-1, (sbyte)-1);
        }
        return new(categories, _profiles);
    }
}

/// <summary>Owned driver98/A0 category bytes for an explicit resource alphabet.</summary>
public sealed class LegacyPreparedCharacterLineAlphabetCategories
{
    private readonly (sbyte Main, sbyte Alternate)[] _categories;
    private readonly LegacyCharacterVerticalCategory[] _profiles;
    public int Count => _categories.Length;

    internal LegacyPreparedCharacterLineAlphabetCategories((sbyte Main, sbyte Alternate)[] categories,
        LegacyCharacterVerticalCategory[] profiles)
    { _categories = categories; _profiles = (LegacyCharacterVerticalCategory[])profiles.Clone(); }

    public (sbyte Main, sbyte Alternate) ReadCategories(ushort label)
    {
        if (label >= _categories.Length) throw new ArgumentOutOfRangeException(nameof(label));
        return _categories[label];
    }

    public LegacyPreparedCharacterLineCategoryItemProducer CreateItemProducer() => new(this);
    internal LegacyCharacterVerticalCategory ReadProfile(sbyte index) => _profiles[index];
}

/// <summary>
/// Fresh Document0531b0 single-candidate template lifecycle. Sequential preparation retains profile
/// floats when native metadata is missing, including across its source-line flushes.
/// </summary>
public sealed class LegacyPreparedCharacterLineCategoryItemProducer
{
    private readonly LegacyPreparedCharacterLineAlphabetCategories _alphabet;
    private float _mainTop, _mainBottom, _alternateTop, _alternateBottom;
    internal LegacyPreparedCharacterLineCategoryItemProducer(LegacyPreparedCharacterLineAlphabetCategories alphabet)
    { _alphabet = alphabet; }

    /// <summary>
    /// Document0531b0's single-candidate item from actual prepared geometry and resolved alphabet ID.
    /// FFFF preserves the native unresolved-symbol branch. This is not an OCR/segmentation operation.
    /// </summary>
    public LegacyPreparedCharacterLineItem PrepareNextItem(ushort resolvedAlphabetLabel,
        float left, float right, float top, float bottom)
    {
        int kind = 0;
        float mainTop = _mainTop, mainBottom = _mainBottom, alternateTop = _alternateTop, alternateBottom = _alternateBottom;
        if (resolvedAlphabetLabel != ushort.MaxValue)
        {
            var categories = _alphabet.ReadCategories(resolvedAlphabetLabel);
            if (categories.Main != -1)
            {
                kind = 1;
                var p = _alphabet.ReadProfile(categories.Main);
                mainBottom = -1f - p.BaselineOffset * 2f;
                mainTop = p.VerticalSpan * 2f + mainBottom;
                if (categories.Alternate != -1)
                {
                    kind = 2;
                    p = _alphabet.ReadProfile(categories.Alternate);
                    alternateBottom = -1f - p.BaselineOffset * 2f;
                    alternateTop = p.VerticalSpan * 2f + alternateBottom;
                }
            }
        }
        var result = new LegacyPreparedCharacterLineItem(false, false, false, left, right, top, bottom,
            [new(1, 0, kind, mainTop, alternateTop, mainBottom, alternateBottom)]);
        _mainTop = mainTop; _mainBottom = mainBottom; _alternateTop = alternateTop; _alternateBottom = alternateBottom;
        return result;
    }
}
