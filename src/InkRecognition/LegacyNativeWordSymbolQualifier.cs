namespace InkRecognition;

/// <summary>
/// Backend1c0f60 finite-cost union followed by frontend152520 qualification.
/// For owned native node/span selection use LegacyNativeWordSymbolPreparationSession.
/// Word-character restriction presence is explicit, never inferred from locale.
/// </summary>
public sealed class LegacyNativeWordSymbolQualifier
{
    private readonly byte[] eligible;
    private readonly byte[]? wordCharacters;
    public int AcousticLabelCount { get; }
    public ReadOnlyMemory<byte> GetEligibilitySnapshot() => eligible.ToArray();

    public LegacyNativeWordSymbolQualifier(int acousticLabelCount,
        IReadOnlyList<LegacyNativeWordAcousticGroupCosts> actualSelectedRows,
        ReadOnlyMemory<byte>? wordCharacterBitset98 = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actualSelectedRows);
        cancellationToken.ThrowIfCancellationRequested();
        if (acousticLabelCount is < 1 or > 4093 || actualSelectedRows.Count > 65535 ||
            wordCharacterBitset98 is { } supplied && supplied.Length != (acousticLabelCount + 7) / 8)
            throw new ArgumentException("Actual bounded alphabet, selected cache rows and optional exact word bitset required.");
        foreach (var row in actualSelectedRows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(row);
            if (row.ClassCosts.Length != acousticLabelCount + 3)
                throw new ArgumentException("Actual selected final-cache width must equal alphabet count plus 3.");
        }
        AcousticLabelCount = acousticLabelCount;
        eligible = new byte[acousticLabelCount + 3];
        wordCharacters = wordCharacterBitset98?.ToArray();
        foreach (var row in actualSelectedRows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (int i = 0; i < eligible.Length; i++)
                if (row.ClassCosts.Span[i] < 0x4000000) eligible[i] = 1;
        }
    }

    internal LegacyNativeWordSymbolQualifier(int acousticLabelCount, ReadOnlySpan<byte> nativeEligibility88,
        ReadOnlyMemory<byte>? wordCharacterBitset98, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (acousticLabelCount is < 1 or > 4093 || nativeEligibility88.Length != acousticLabelCount + 3 ||
            wordCharacterBitset98 is { } supplied && supplied.Length != (acousticLabelCount + 7) / 8)
            throw new ArgumentException("Exact bounded frontend88/98 storage required.");
        AcousticLabelCount = acousticLabelCount;
        eligible = nativeEligibility88.ToArray();
        wordCharacters = wordCharacterBitset98?.ToArray();
    }

    public bool Qualifies(ushort currentSymbol, byte queryLength, ulong contextIdentity,
        LegacySymbolContextFactory contexts, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contexts); cancellationToken.ThrowIfCancellationRequested();
        if (currentSymbol >= eligible.Length) throw new ArgumentException("Current symbol lies outside the native count+3 table.");
        if (queryLength != 0 && contextIdentity != 0 && wordCharacters is not null)
        {
            int visited = 0;
            while (contextIdentity != 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (++visited > contexts.NodeCount) throw new InvalidDataException("Symbol parent links contain a cycle.");
                var node = contexts.ReadNode(contextIdentity);
                if (node.Symbol < AcousticLabelCount &&
                    (wordCharacters[node.Symbol >> 3] & (1 << (node.Symbol & 7))) == 0) return false;
                contextIdentity = node.ParentLink;
            }
        }
        return eligible[currentSymbol] != 0;
    }
}
