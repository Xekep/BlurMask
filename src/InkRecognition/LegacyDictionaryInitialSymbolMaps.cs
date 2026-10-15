namespace InkRecognition;

/// <summary>Owned source50/58/60/68 output of the exact, normalization-disabled092990 symbol pass.</summary>
public sealed class LegacyDictionaryInitialSymbolMaps
{
    private readonly ushort[] primary, auxiliary;
    private readonly ulong[] contextIds;
    public int AcousticSymbolCount { get; }
    public LegacyInkAlphabet AcousticAlphabet { get; }
    public LegacyInkAlphabet LexicalAlphabet { get; }
    public LegacySymbolContextFactory Contexts { get; }
    public ushort[] CopyPrimary() => primary.ToArray();
    public ushort[] CopyAuxiliary() => auxiliary.ToArray();
    public ulong[] CopyInitialContexts() => contextIds.ToArray();

    private LegacyDictionaryInitialSymbolMaps(LegacyInkAlphabet acoustic, LegacyInkAlphabet lexical, LegacySymbolContextFactory contexts,
        ushort[] primary, ushort[] auxiliary, ulong[] contextIds)
    { AcousticAlphabet=acoustic;LexicalAlphabet=lexical;AcousticSymbolCount = acoustic.LabelCount; Contexts = contexts; this.primary = primary; this.auxiliary = auxiliary; this.contextIds = contextIds; }

    /// <summary>
    /// Native initializes auxiliary60 toFFFF only for nonvirtual symbols. It
    /// retains allocator bytes for virtual entries; exact residue is required
    /// when any virtual entry exists. Ordinary mapped labels leave60=FFFF;
    /// controls at/above alphabet count and exact ZWNJ/ZWJ copy their mapped
    /// value into60. Preserved58 copies initial50 before permission filtering.
    /// </summary>
    public static LegacyDictionaryInitialSymbolMaps Prepare(LegacyInkAlphabet acoustic,
        LegacyInkAlphabet lexical, LegacySymbolContextFactory contexts,
        ReadOnlySpan<ushort> virtualAuxiliaryAllocationResidue,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(acoustic); ArgumentNullException.ThrowIfNull(lexical);
        ArgumentNullException.ThrowIfNull(contexts); cancellationToken.ThrowIfCancellationRequested();
        bool hasVirtual = Enumerable.Range(0, lexical.LabelCount).Any(i => lexical.IsVirtual((ushort)i));
        if (virtualAuxiliaryAllocationResidue.Length != lexical.LabelCount &&
            (hasVirtual || !virtualAuxiliaryAllocationResidue.IsEmpty))
            throw new ArgumentException("Exact auxiliary allocator residue per lexical symbol is required for virtual entries.");
        ushort[] primary = LegacyLinguisticSymbolMaskController.CreateExactDictionaryMap(acoustic, lexical, cancellationToken);
        ushort[] auxiliary = virtualAuxiliaryAllocationResidue.IsEmpty
            ? new ushort[lexical.LabelCount] : virtualAuxiliaryAllocationResidue.ToArray();
        for (int i = 0; i < primary.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (lexical.IsVirtual((ushort)i)) continue;
            auxiliary[i] = ushort.MaxValue;
            if (!lexical.TryGetText((ushort)i, out string text))
                throw new InvalidDataException("Real lexical symbol has no source text.");
            if (primary[i] >= acoustic.LabelCount || text is "\u200c" or "\u200d")
                auxiliary[i] = primary[i];
        }
        ulong[] initial = contexts.CreateInitialSourceMap(primary, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return new(acoustic, lexical, contexts, primary, auxiliary, initial);
    }

    public LegacyPreparedDictionaryWorkingMap CreateWorkingMap(uint descriptorFlags)
        => new(AcousticSymbolCount, descriptorFlags, primary, auxiliary);

    public LegacyPreparedDictionaryQuerySource CreateQuerySource(LegacyPackedLexicon dictionary, uint descriptorFlags)
        => new(dictionary, CreateWorkingMap(descriptorFlags), contextIds);

    /// <summary>093af0 uses preserved58, independent of source50 permission mutations.</summary>
    public LegacyLinguisticSymbolSource CreateSymbolSource()
        => new(Enumerable.Range(0, primary.Length).Select(i => (uint)i).ToArray(), primary.ToArray());
}
