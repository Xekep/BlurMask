using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativeMainInitialSpecialResult(int VisitedQueries, int QualifiedQueries,
    int AcousticCalls, int PrimaryAllocations, int PrimitiveExtraAllocations);

/// <summary>
/// Ordinary1FD240 initial single-context branch including apostrophe/quote
/// variant forcing and F0002/3/4/8 extra records. Main caches and eligibility
/// come from an owned D0 span. Protocol queries, associated2300 rewrites,
/// input-method mode and tracing require their separate native producers.
/// </summary>
public sealed class LegacyNativeMainInitialSpecialCandidates
{
    private readonly LegacyOwnedMainWordSourceServices owner;
    private readonly LegacyCursiveCategoryMapper categories;
    private readonly LegacyNativeWordAcousticRequestScorer scorer;

    private readonly LegacyNativeWordTransitionCostCell cell;
    private readonly byte[] residue;
    private readonly ushort apostrophe, quote, excludedLoop, excludedShadda;
    private readonly ushort[] duplicatedPrimitives;
    private readonly uint[] headers;
    private bool valid = true;
    private int active;

    public LegacyNativeMainInitialSpecialCandidates(ReadOnlySpan<byte> actualResource,
        LegacyOwnedMainWordSpanPreparation owner, LegacyNativeWordTransitionCostCell costCell,
        ReadOnlySpan<byte> existingRequestResidue80)
        :this(actualResource,LegacyOwnedMainWordSourceServices.FromSameSource(owner),costCell,existingRequestResidue80) { }

    internal LegacyNativeMainInitialSpecialCandidates(ReadOnlySpan<byte> actualResource,
        LegacyOwnedMainWordSourceServices owner,LegacyNativeWordTransitionCostCell costCell,
        ReadOnlySpan<byte> existingRequestResidue80)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(costCell);
        if (existingRequestResidue80.Length != 80)
            throw new ArgumentException("Actual complete80 request residue required.");
        this.owner = owner;
        cell = costCell;
        residue = existingRequestResidue80.ToArray();
        //The owned source validates its pinned resource on construction. This
        //additional geometry owner check rejects a different resource here.
        var sourceOwner = LegacySourceSamplerCategoryOwner.ReadV10dSource(actualResource, 0);
        if (sourceOwner.SourceFingerprint != owner.Categories.SourceFingerprint)
            throw new ArgumentException("Initial consumer must retain its actual main resource owner.");
        var alphabet = owner.Model.AcousticAlphabet;
        categories = new(owner.Categories.Categories);
        scorer = LegacyNativeWordAcousticRequestScorer.ForAlphabet(
            LegacyAcousticCandidateThresholds.ReadV110Node(actualResource[owner.OrdinaryAcousticOffset..], out _), alphabet);
        apostrophe = Resolve("'");
        quote = Resolve("\"");
        excludedLoop = Resolve("\U000f0001");
        excludedShadda = Resolve("\u0651");
        duplicatedPrimitives = [Resolve("\U000f0002"), Resolve("\U000f0003"), Resolve("\U000f0004"), Resolve("\U000f0008")];
        headers = owner.CopySourceHeaders();

        ushort Resolve(string text) => alphabet.TryGetLabel(text, out ushort symbol) ? symbol : ushort.MaxValue;
    }

    /// <summary>Consume a complete explicit prepared source page. This permits
    /// a later native decorator producer to transport primitive queries absent
    /// from the direct LK alphabet; it does not invent such query records.</summary>
    public LegacyNativeMainInitialSpecialResult ProcessPreparedPage(LegacyOwnedMainWordSpan span,
        ReadOnlySpan<byte> actualQueries32, ushort inputFlags, LegacyNativeWordHeapRouter router,
        float secondaryScaleEc, bool suppressHeader22f7, bool suppressPrevious2609,
        CancellationToken cancellationToken = default)
        =>ProcessOwnedPage(owner.Wrap(span,cancellationToken),actualQueries32,inputFlags,router,secondaryScaleEc,
            suppressHeader22f7,suppressPrevious2609,cancellationToken);

    internal LegacyNativeMainInitialSpecialResult ProcessOwnedPage(LegacyOwnedMainWordSourceServices.SpanLease span,
        ReadOnlySpan<byte> actualQueries32, ushort inputFlags, LegacyNativeWordHeapRouter router,
        float secondaryScaleEc, bool suppressHeader22f7, bool suppressPrevious2609,
        CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Main initial candidate calls must be serialized.");
        try
        {
            if (!valid) throw new InvalidOperationException("Failed initial producer requires a fresh instance.");
            ArgumentNullException.ThrowIfNull(span);
            ArgumentNullException.ThrowIfNull(router);
            cancellationToken.ThrowIfCancellationRequested();
            owner.ValidateSpan(span, cancellationToken);
            if (!span.IsInitial || actualQueries32.Length % 32 != 0 || actualQueries32.Length > 512 * 32 ||
                !float.IsFinite(secondaryScaleEc))
                throw new ArgumentException("Whole-prefix main span and complete bounded query page required.");
            byte[] queries = actualQueries32.ToArray();
            var arcs = owner.Graph.GetSuccessorOrdinals(-1, ushort.MaxValue, span.Span, cancellationToken);
            int visited = 0, qualified = 0, calls = 0, primary = 0, extras = 0;
            foreach (int index in Enumerable.Range(0, queries.Length / 32))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var row = queries.AsSpan(index * 32, 32);
                visited++;
                ushort symbol = U16(row, 20);
                if (symbol == excludedLoop || symbol == excludedShadda) continue;
                if (symbol >= owner.Model.AcousticAlphabet.LabelCount)
                    throw new NotSupportedException("Protocol FFE3..FFEA queries need their source58 expansion producer.");
                byte source = row[4], flags = row[23], length = row[22];
                if (source >= headers.Length)
                    throw new ArgumentException("Query tag must retain its prepared source header owner.");
                ulong context = U64(row, 8);
                if (context == 0 || owner.Contexts.ReadNode(context).Symbol != symbol)
                    throw new ArgumentException("Actual query symbol differs from its owning symbolic context.");
                context = LegacyNativeWordStartupRequestFactory.GraftOrdinaryInitialContext(owner.Contexts, context, cancellationToken);
                if (!span.Qualifier.Qualifies(symbol, length, context, owner.Contexts, cancellationToken)) continue;
                qualified++;
                bool isQuote = symbol == apostrophe || symbol == quote;
                int variantCount = (inputFlags & 0x10) == 0 || isQuote ? 2 : 1;
                for (int variant = 0; variant < variantCount; variant++)
                {
                    //With an ordinary single initial context the grafted parent
                    //is FFFA, never ZWNJ; absent associated2300 uses normal maps.
                    int category = categories.GetSymbolCompositeCategory(symbol, variant != 0);
                    if (category < 0)
                    {
                        if (variant == 0) throw new NotSupportedException("Native starting composite entry is absent.");
                        break;
                    }
                    foreach (int ordinal in arcs.Span)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        ushort arc = checked((ushort)ordinal);
                        ulong tag = U64(row, 0);
                        int baseCost = I32(row, 16);
                        var request = LegacyNativeWordStartupRequestFactory.Create((uint)span.EndingNode, span.Span,
                            arc, symbol, category, tag, length, flags, headers[source], 0,
                            owner.Categories.Categories, cell, baseCost, residue, cancellationToken);
                        calls++;
                        if (!scorer.Score(request, (node, currentArc) => owner.GetCosts(span, node, currentArc, cancellationToken),
                            false, cancellationToken: cancellationToken)) continue;
                        int first = unchecked((int)owner.Graph.GetNode(span.EndingNode)[ordinal].StartContextId);
                        bool flagged = !suppressHeader22f7 && owner.Model.AcousticAlphabet.IsSymbolFlagged(symbol);
                        var fields = new LegacyInitialWordRecordFields(tag, context, cell.Value, request.ReadInt32(48),
                            symbol, checked((ushort)span.EndingNode), arc, arc, checked((byte)category), ushort.MaxValue,
                            unchecked((sbyte)span.Span), 0, flagged ? (byte)1 : (byte)0, 0, 0,
                            (byte)(flags & 7), unchecked((byte)((flags >> 5 & 3) + length)), 0, 0,
                            (flags & 8) != 0 ? first : -1, first, span.EndingNode, ushort.MaxValue);
                        var result = router.TryInsertInitialRecord(source, false, fields, secondaryScaleEc,
                            suppressPrevious2609, cancellationToken: cancellationToken);
                        cancellationToken.ThrowIfCancellationRequested();
                        if (result.Accepted) primary++;
                        //152040 B8 returns ordinary graph kind0. Native1FD240
                        //then emits a second record for these four labels when
                        //mode22F6=false and aggregate source kinds low3>1.
                        if (LegacyNativeInitialPrimitiveExtraRecord.TryCreate(fields, baseCost, 0, false,
                            owner.SourceSummary.Flags, duplicatedPrimitives, out var extra))
                        {
                            cell.Value = extra!.CandidateCost10;
                            result = router.TryInsertInitialRecord(source, false, extra, secondaryScaleEc,
                                suppressPrevious2609, cancellationToken: cancellationToken);
                            cancellationToken.ThrowIfCancellationRequested();
                            if (result.Accepted) extras++;
                        }
                    }
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            return new(visited, qualified, calls, primary, extras);
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    private static ushort U16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
    private static ulong U64(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(data[offset..]);
    private static int I32(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadInt32LittleEndian(data[offset..]);
}
