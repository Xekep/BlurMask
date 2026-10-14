namespace InkRecognition;

public sealed record LegacyNativeWordSymbolPreparationResult(
    LegacyOrdinaryGraphPermissionPass PermissionPass, LegacyNativeWordSymbolQualifier? Qualifier,
    bool EligibilityRebuilt);

/// <summary>
/// Owned ordinary frontend1520e0: actual cache/source preparation, followed by
/// the span-selected backend1c0f60 union and frontend110 qualification.
/// Graph/source ownership, cache lifecycle and arc order come from the existing
/// ordinary permission session. Calls must be serialized. Failure invalidates
/// this session; no partially prepared table can be returned as successful.
/// </summary>
public sealed class LegacyNativeWordSymbolPreparationSession
{
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private readonly LegacyOrdinaryGraphAcousticPermissionSession preparation;
    private readonly int labelCount;
    private readonly ReadOnlyMemory<byte>? wordCharacters;
    private LegacyNativeWordSymbolQualifier? qualifier;
    private bool valid = true;
    private int active;

    public LegacyNativeWordSymbolPreparationSession(LegacyOrdinaryCursiveCandidateGraph graph,
        LegacyPreparedDictionaryWorkingMap source, ReadOnlySpan<ushort> reservedSlots,
        Func<LegacyOrdinaryCursiveCandidateInk, CancellationToken, LegacyOrdinaryGraphAcousticRow?> compute,
        ReadOnlyMemory<byte>? wordCharacterBitset98 = null,
        ReadOnlyMemory<byte>? existingEligibility88 = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(source);
        cancellationToken.ThrowIfCancellationRequested();
        labelCount = source.AcousticSymbolCount;
        if (labelCount is < 1 or > 4093 ||
            wordCharacterBitset98 is { } words && words.Length != (labelCount + 7) / 8 ||
            existingEligibility88 is { } storage && storage.Length != labelCount + 3)
            throw new ArgumentException("Matching bounded frontend alphabet and explicit storage required.");
        this.graph = graph;
        wordCharacters = wordCharacterBitset98 is { } suppliedWords
            ? new ReadOnlyMemory<byte>(suppliedWords.ToArray()) : (ReadOnlyMemory<byte>?)null;
        preparation = new(graph, source, reservedSlots, compute);
        if (existingEligibility88 is { } initial)
            qualifier = new(labelCount, initial.Span, wordCharacters, cancellationToken);
    }

    /// <summary>
    /// Param5=true still prepares cache/source permissions but returns the previous
    /// frontend88 bytes unchanged. It does not union the newly selected rows.
    /// Without supplied or previously prepared storage that branch is unsupported.
    /// Param5=false clears and rebuilds from every matching arc in ordinal order.
    /// </summary>
    public LegacyNativeWordSymbolPreparationResult Prepare(int anchorNode, byte requestedSpan,
        bool reuseComputed, CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Frontend symbol preparation calls must be serialized.");
        try
        {
            if (!valid) throw new InvalidOperationException("Failed frontend preparation requires a fresh session.");
            cancellationToken.ThrowIfCancellationRequested();
            if (reuseComputed && qualifier is null)
                throw new NotSupportedException("Native reuse requires existing frontend88 storage; uninitialized bytes are not invented.");
            var pass = preparation.Evaluate(anchorNode, requestedSpan, reuseComputed, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!pass.Succeeded)
            {
                valid = false;
                return new(pass, null, false);
            }
            if (!reuseComputed)
            {
                var arcs = graph.GetNode(anchorNode);
                int selected = 0;
                foreach (var arc in arcs)
                    if (arc.Span == requestedSpan) selected++;
                if (pass.SelectedRows.Count != selected || pass.SelectedPositions.Count != selected)
                    throw new InvalidDataException("Owned frontend span selection differs from cache preparation.");
                var union = new byte[labelCount + 3];
                int row = 0;
                for (int ordinal = 0; ordinal < arcs.Count; ordinal++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (arcs[ordinal].Span != requestedSpan) continue;
                    var actual = pass.SelectedRows[row];
                    var position = new LegacyGraphGlyphPosition(anchorNode, ordinal);
                    if (actual.Position != position || pass.SelectedPositions[row] != position ||
                        actual.GroupId != arcs[ordinal].GroupId || actual.Costs.Length != union.Length)
                        throw new InvalidDataException("Owned frontend cache identity or width differs.");
                    for (int symbol = 0; symbol < union.Length; symbol++)
                        if (actual.Costs.Span[symbol] < 0x4000000) union[symbol] = 1;
                    row++;
                }
                qualifier = new(labelCount, union, wordCharacters, cancellationToken);
            }
            cancellationToken.ThrowIfCancellationRequested();
            return new(pass, qualifier!, !reuseComputed);
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }
}
