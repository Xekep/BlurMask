namespace InkRecognition;

/// <summary>A prepared dictionary edge after the restored map/terminal gate.</summary>
public readonly record struct LegacyPreparedDictionaryTransition(int NextState, ushort LexicalLabel,
    ushort AcousticSymbol, byte Flags);

/// <summary>
/// Immutable prepared type2b7 source maps: preserved58, mutable-native-working50
/// represented by a new snapshot, and explicit auxiliary60. Does not infer
/// permissions, query lifecycle, native source factories or Unicode aliases.
/// </summary>
public sealed class LegacyPreparedDictionaryWorkingMap
{
    private readonly ushort[] preserved, working, auxiliary;
    public int AcousticSymbolCount { get; }
    public int LexicalSymbolCount => preserved.Length;
    public uint DescriptorFlags { get; }

    internal bool SharesAuxiliaryMap(LegacyPreparedDictionaryWorkingMap other)
        => ReferenceEquals(auxiliary, other.auxiliary);

    /// <summary>Own copies of explicit maps; absent auxiliary data cannot be silently selected.</summary>
    public LegacyPreparedDictionaryWorkingMap(int acousticSymbolCount, uint descriptorFlags,
        ReadOnlySpan<ushort> preservedMap, ReadOnlySpan<ushort> auxiliaryMap)
    {
        if (acousticSymbolCount is < 1 or > ushort.MaxValue || preservedMap.Length is < 1 or > ushort.MaxValue ||
            !auxiliaryMap.IsEmpty && auxiliaryMap.Length != preservedMap.Length)
            throw new ArgumentException("Bounded explicit dictionary maps and acoustic alphabet required.");
        AcousticSymbolCount = acousticSymbolCount; DescriptorFlags = descriptorFlags;
        preserved = preservedMap.ToArray(); working = preservedMap.ToArray(); auxiliary = auxiliaryMap.ToArray();
    }

    private LegacyPreparedDictionaryWorkingMap(LegacyPreparedDictionaryWorkingMap source, ushort[] nextWorking)
    {
        AcousticSymbolCount = source.AcousticSymbolCount; DescriptorFlags = source.DescriptorFlags;
        preserved = source.preserved; auxiliary = source.auxiliary; working = nextWorking;
    }

    /// <summary>
    /// Text093f80 restores58→50 before filtering. Category800 requires permission
    /// bits1|4; all other categories require bit1. Control tokens at/above the
    /// acoustic symbol count are unchanged. Each refresh starts from preserved.
    /// </summary>
    public LegacyPreparedDictionaryWorkingMap Refresh(ReadOnlySpan<byte> acousticPermissions,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (acousticPermissions.Length != AcousticSymbolCount)
            throw new ArgumentException("Explicit acoustic permission byte for every symbol required.");
        byte required = (DescriptorFlags & 0x1e00) == 0x800 ? (byte)5 : (byte)1;
        var updated = preserved.ToArray();
        for (int i = 0; i < updated.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested(); ushort symbol = updated[i];
            if (symbol < AcousticSymbolCount && (acousticPermissions[symbol] & required) != required)
                updated[i] = ushort.MaxValue;
        }
        return new(this, updated);
    }

    /// <summary>
    /// Text093b50: auxiliary enumeration selects60; ordinary query byte36 bit40
    /// selects preserved58, otherwise working50. Return an owned public copy.
    /// </summary>
    public ushort[] CopySelectedMap(bool auxiliaryEnumeration, byte queryFlags36) =>
        Select(auxiliaryEnumeration, queryFlags36).ToArray();

    //Immutable model-owned maps can be borrowed inside a serialized query.
    //The public API still returns an independent, mutable caller snapshot.
    internal ReadOnlySpan<ushort> ReadSelectedMap(bool auxiliaryEnumeration, byte queryFlags36) =>
        Select(auxiliaryEnumeration, queryFlags36);

    /// <summary>
    /// Checked dictionary graph→selected map→candidate gate shared by0920a0,
    /// 0922f0 and092540. FFFF suppresses an edge; auxiliary enumeration also
    /// rejects nonterminal edges. Low flags encode root-origin1 and terminal2.
    /// The packed backend supplies edges; native query ring/pagination and
    /// parent/context pointers are not reproduced here.
    /// </summary>
    public LegacyPreparedDictionaryTransition[] GetTransitions(LegacyPackedLexicon lexicon,
        int state, bool auxiliaryEnumeration, byte queryFlags36,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lexicon); cancellationToken.ThrowIfCancellationRequested();
        var selected = Select(auxiliaryEnumeration, queryFlags36);
        int count = lexicon.GetChildCount(state);
        if (count > 512) throw new ArgumentException("Prepared dictionary state exceeds512 immediate transitions.");
        var children = new int[count]; lexicon.CopyChildren(state, children);
        var result = new List<LegacyPreparedDictionaryTransition>(count);
        foreach (int child in children)
        {
            cancellationToken.ThrowIfCancellationRequested(); ushort lexical = lexicon.GetLabel(child);
            if (lexical >= selected.Length) throw new InvalidDataException("Dictionary edge outside explicit lexical map.");
            ushort acoustic = selected[lexical]; bool terminal = lexicon.IsTerminal(child);
            if (acoustic == ushort.MaxValue || auxiliaryEnumeration && !terminal) continue;
            result.Add(new(child, lexical, acoustic, (byte)((state == 0 ? 1 : 0) | (terminal ? 2 : 0))));
        }
        return result.ToArray();
    }

    /// <summary>
    /// Original diagnostic adapter from explicit acoustic costs to dictionary
    /// labels using this restored selected map. It adds no related/case aliases.
    /// This adapter is not native word search, pagination or candidate scoring.
    /// </summary>
    public LegacyIntegerLexicalCandidate[] MapAcousticCosts(ReadOnlySpan<int> acousticCosts,
        bool auxiliaryEnumeration, byte queryFlags36, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (acousticCosts.Length is < 1 || acousticCosts.Length > AcousticSymbolCount)
            throw new ArgumentException("Bounded explicit acoustic costs required.");
        foreach (int cost in acousticCosts)
            if (cost is < 0 or > LegacyAcousticFixedLog.ImpossibleCost)
                throw new ArgumentException("Acoustic costs must lie in0..ImpossibleCost.");
        var selected = Select(auxiliaryEnumeration, queryFlags36);
        var result = new List<LegacyIntegerLexicalCandidate>();
        for (int lexical = 0; lexical < selected.Length; lexical++)
        {
            cancellationToken.ThrowIfCancellationRequested(); ushort acoustic = selected[lexical];
            if (acoustic < acousticCosts.Length && acousticCosts[acoustic] < LegacyAcousticFixedLog.ImpossibleCost)
                result.Add(new((ushort)lexical, acousticCosts[acoustic]));
        }
        return result.ToArray();
    }

    private ReadOnlySpan<ushort> Select(bool auxiliaryEnumeration, byte queryFlags36)
    {
        if (!auxiliaryEnumeration) return (queryFlags36 & 0x40) != 0 ? preserved : working;
        if (auxiliary.Length == 0) throw new NotSupportedException("Auxiliary map60 was not prepared.");
        return auxiliary;
    }
}
