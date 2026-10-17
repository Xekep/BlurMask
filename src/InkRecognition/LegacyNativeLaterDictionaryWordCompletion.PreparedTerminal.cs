using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainLanguageDispatch.Probe")]

namespace InkRecognition;

public sealed partial class LegacyNativeLaterDictionaryWordCompletion
{
    private readonly HashSet<PreparedTerminal> preparedTerminals = [];

    internal sealed class PreparedTerminal
    {
        private readonly byte[] query, terminal;
        private readonly Action<CancellationToken> validateSourceLease;
        internal int EndingNode { get; }
        internal LegacySymbolContextFactory Contexts { get; }
        internal bool KnowledgeF5 { get; }
        internal int SubwordSource { get; }
        internal bool ExtendedMarkers { get; }
        internal bool SuppressPrevious { get; }
        internal PreparedTerminal(byte[] query, byte[] terminal, int endingNode,
            LegacySymbolContextFactory contexts, bool knowledgeF5, int subwordSource,
            bool extendedMarkers, bool suppressPrevious, Action<CancellationToken> validateSourceLease)
        {
            this.query = query; this.terminal = terminal; EndingNode = endingNode;
            Contexts = contexts; KnowledgeF5 = knowledgeF5; SubwordSource = subwordSource;
            ExtendedMarkers = extendedMarkers; SuppressPrevious = suppressPrevious;
            this.validateSourceLease = validateSourceLease;
        }
        internal byte[] CopyQuery() => query.ToArray();
        internal byte[] CopyTerminal() => terminal.ToArray();
        internal void ValidateSourceLease(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            validateSourceLease(token);
            token.ThrowIfCancellationRequested();
        }
    }

    /// <summary>Native source70 precedes203690. The snapshot is registered to
    /// this completion and its live span/epoch; full query32 and tag survive.</summary>
    internal PreparedTerminal CaptureActualTerminal(ReadOnlySpan<byte> actualQuery32, int endingNode,
        LegacySymbolContextFactory contexts, bool knowledgeF5, int subwordSourceIndex22ec,
        bool extendedMarkers22f8, bool suppressPrevious2609,
        Action<CancellationToken> validateSourceLease, CancellationToken cancellationToken)
    {
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Native later-word terminal reads must be serialized.");
        try
        {
            if (!valid) throw new InvalidOperationException("Failed later-word completion requires a fresh producer.");
            ArgumentNullException.ThrowIfNull(contexts);
            ArgumentNullException.ThrowIfNull(validateSourceLease);
            cancellationToken.ThrowIfCancellationRequested();
            if (actualQuery32.Length != 32 || actualQuery32[4] != sourceId || endingNode < 0)
                throw new ArgumentException("Actual selected-source complete query32/node required.");
            byte[] query = actualQuery32.ToArray();
            ushort symbol = U16(query, 20);
            if (U64(query, 24) != 0 || (query[23] & 7) is not (0 or 2) ||
                symbol >= alphabet.LabelCount || alphabet.IsVirtual(symbol) ||
                !alphabet.TryGetText(symbol, out _) || symbol == replacement)
                throw new NotSupportedException("Ordinary nonvirtual source query without replacement/protocol context required.");
            ulong context = U64(query, 8);
            if (context != 0) _ = contexts.ReadNode(context);
            ulong tag = U64(query, 0);
            validateSourceLease(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            caseSpan?.Validate(endingNode, contexts, knowledgeF5, subwordSourceIndex22ec,
                extendedMarkers22f8, suppressPrevious2609, cancellationToken);
            var token = caseSpan is null
                ? metadata!.ReadTerminal(tag & ~(255UL << 32), 0, cancellationToken)
                : caseSpan.ReadTerminal(tag, symbol, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            validateSourceLease(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            caseSpan?.Validate(endingNode, contexts, knowledgeF5, subwordSourceIndex22ec,
                extendedMarkers22f8, suppressPrevious2609, cancellationToken);
            if (token.Length != 16) throw new InvalidDataException("Owned source70 must return its complete16-byte token.");
            var captured = new PreparedTerminal(query, token.ToArray(), endingNode, contexts,
                knowledgeF5, subwordSourceIndex22ec, extendedMarkers22f8, suppressPrevious2609, validateSourceLease);
            preparedTerminals.Add(captured);
            return captured;
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    /// <summary>Consume one source70 snapshot for this query's entire context
    /// heap, in native reverse array order. No second70 read is performed.</summary>
    internal IReadOnlyList<LegacyNativeLaterDictionaryWordCompletionResult> ProcessActualQueryWithPreparedTerminal(
        PreparedTerminal captured, LegacyNativeLanguageContextRecombinationResult contextResult,
        ushort currentRowOrdinal, int availableSourceNodes,
        Func<int, IReadOnlyList<LegacyNativeStoredWordRecord>> owningNodeRowsLookup,
        Func<byte, uint> preparedSourceHeader, Func<int, int, int> descriptorLineE8,
        LegacyNativeWordHeapRouter router, ReadOnlySpan<byte> temporaryLexicalResidue40,
        CancellationToken cancellationToken)
    {
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Native later-word completion calls must be serialized.");
        try
        {
            if (!valid) throw new InvalidOperationException("Failed later-word completion requires a fresh producer.");
            ArgumentNullException.ThrowIfNull(captured);
            ArgumentNullException.ThrowIfNull(contextResult);
            cancellationToken.ThrowIfCancellationRequested();
            if (!preparedTerminals.Contains(captured))
                throw new ArgumentException("Terminal snapshot must belong to this completion and remain unconsumed.");
            if (contextResult.Candidates16.Length % 16 != 0 || contextResult.LastRequest80.Length != 80 ||
                contextResult.Candidates16.Length / 16 > ushort.MaxValue ||
                availableSourceNodes is < 1 or > 1_000_000 || captured.EndingNode >= availableSourceNodes ||
                temporaryLexicalResidue40.Length != 40)
                throw new ArgumentException("Actual complete context16 heap and request80 required.");
            //Detach the complete heap before any externally supplied owner callback.
            contextResult = contextResult with
            {
                Candidates16 = contextResult.Candidates16.ToArray(),
                LastRequest80 = contextResult.LastRequest80.ToArray()
            };
            captured.ValidateSourceLease(cancellationToken);
            caseSpan?.Validate(captured.EndingNode, captured.Contexts, captured.KnowledgeF5,
                captured.SubwordSource, captured.ExtendedMarkers, captured.SuppressPrevious, cancellationToken);
            caseSpan?.ValidateExtent(availableSourceNodes);
            preparedTerminals.Remove(captured);
            byte[] query = captured.CopyQuery(), terminal = captured.CopyTerminal();
            var results = new List<LegacyNativeLaterDictionaryWordCompletionResult>();
            for (int index = contextResult.Candidates16.Length / 16 - 1; index >= 0; index--)
            {
                cancellationToken.ThrowIfCancellationRequested();
                results.Add(ProcessCore(contextResult, index, captured.EndingNode, currentRowOrdinal,
                    availableSourceNodes, owningNodeRowsLookup, preparedSourceHeader, descriptorLineE8,
                    captured.Contexts, router, captured.KnowledgeF5, captured.SubwordSource,
                    captured.ExtendedMarkers, new byte[32], temporaryLexicalResidue40,
                    captured.SuppressPrevious, cancellationToken, query, terminal));
                captured.ValidateSourceLease(cancellationToken);
            }
            captured.ValidateSourceLease(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return results.AsReadOnly();
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }
}
