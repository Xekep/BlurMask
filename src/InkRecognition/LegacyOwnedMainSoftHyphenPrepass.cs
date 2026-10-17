using System.Security.Cryptography;

namespace InkRecognition;

//The ordinary fresh main caller supplies its actual current D0 qualifier,
//same geometry instance, class-cache128, context factory and shared request.
internal sealed class LegacyOwnedMainSoftHyphenPrepass
{
    private readonly LegacyOwnedMainWordSourceServices source;
    private readonly LegacyNativeHigherArcTransitionOperations operations;
    private readonly LegacyNativeWordAcousticRequestScorer scorer;
    private readonly LegacyNativeMainWordRequestFrame frame;
    private readonly uint[] headers;
    private readonly bool knowledgeMarkerA0;
    private readonly ushort soft, dash;
    private int active;
    private bool valid = true;
    internal bool HasLabels => soft != ushort.MaxValue && dash != ushort.MaxValue;

    internal LegacyOwnedMainSoftHyphenPrepass(ReadOnlySpan<byte> resource,
        LegacyOwnedMainWordSourceServices source, LegacyNativeHigherArcTransitionOperations operations,
        LegacyNativeWordAcousticRequestScorer scorer, LegacyNativeMainWordRequestFrame frame,
        bool knowledgeMarkerA0, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(operations);
        ArgumentNullException.ThrowIfNull(scorer); ArgumentNullException.ThrowIfNull(frame);
        token.ThrowIfCancellationRequested();
        if (resource.Length != 572763 || source.Model.SourceKind != 0x135 ||
            !Convert.ToHexString(SHA256.HashData(resource)).Equals(source.Categories.SourceFingerprint, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The exact fresh main sampler resource/model/source is required.");
        headers = source.CopySourceHeaders();
        if (headers is not [0, var selected] || (selected & 0x1e00) != 0xc00)
            throw new NotSupportedException("Actual fresh implicit0 and one ordinary C00 source required.");
        this.source = source; this.operations = operations; this.scorer = scorer; this.frame = frame;
        this.knowledgeMarkerA0 = knowledgeMarkerA0;
        var alphabet = source.Model.AcousticAlphabet;
        soft = alphabet.TryGetLabel("\u00ad", out ushort s) ? s : ushort.MaxValue;
        dash = alphabet.TryGetLabel("-", out ushort d) ? d : ushort.MaxValue;
    }

    internal LegacyNativeSoftHyphenWordPrepassResult Process(LegacyOwnedMainWordSourceServices.SpanLease span,
        ushort parentOrdinal, Func<int, ushort, ReadOnlyMemory<byte>> owningRows,
        LegacyNativeWordHeapRouter router, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Owned soft-hyphen calls must be serialized.");
        try
        {
            if (!valid) throw new InvalidOperationException("Failed soft-hyphen producer cannot be resumed.");
            source.ValidateSpan(span, token);
            if (!HasLabels) return new(false, false, 0, 0, 0, 0, 0, ReadOnlyMemory<byte>.Empty);
            if (span.PreviousEndingNode < 0) throw new ArgumentException("A real nonroot current D0 span is required.");
            var parent = owningRows(span.PreviousEndingNode, parentOrdinal);
            token.ThrowIfCancellationRequested();
            if (parent.Length != 88) throw new ArgumentException("Actual stored88 parent required.");
            var result = LegacyNativeSoftHyphenWordPrepass.Run(span.EndingNode, span.Span,
                span.PreviousEndingNode, parentOrdinal, parent.Span, headers, soft, dash,
                source.Categories.Categories, source.Contexts, span.Qualifier, operations,
                Score, frame, router, owningRows, false, knowledgeMarkerA0, false, false, token);
            source.ValidateSpan(span, token); token.ThrowIfCancellationRequested();
            return result;

            bool Score(LegacyNativeWordRecombinationRequest request, CancellationToken cancellation)
            {
                source.ValidateSpan(span, cancellation);
                return scorer.Score(request, (n, a) => source.GetCosts(span, n, a, cancellation),
                    false, cancellationToken: cancellation);
            }
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }
}
