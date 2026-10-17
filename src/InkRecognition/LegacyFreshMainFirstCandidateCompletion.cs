using System.Runtime.CompilerServices;

namespace InkRecognition;

/// <summary>Bounded first candidate after actual quote cleanup and freshF4
/// no-op. Raw glyph bytes precede string-wrapper normalization/acceptance.
/// No spaces or joined transcription are manufactured between WORD records.</summary>
public sealed class LegacyFreshMainCompletedFirstCandidate
{
    public LegacyFreshMainSingleLinePlainRecognitionResult RawResult { get; }
    public LegacyOwnedMainFirstAlternativeSnapshot Snapshot { get; }
    public LegacyFreshMainResultGlyphNormalizationNoOp GlyphNormalization { get; }
    public IReadOnlyList<LegacyOwnedMainWordUtf8Projection> RawWordsInNativeHeadOrder { get; }
    public long Reference78 => Snapshot.Reference78;
    public float RawFirstMass08 => Snapshot.RawFirstMass08;
    public float FinalTop80 => Snapshot.NormalizedTop80;
    internal LegacyFreshMainCompletedFirstCandidate(LegacyFreshMainSingleLinePlainRecognitionResult raw,
        LegacyOwnedMainFirstAlternativeSnapshot snapshot, LegacyFreshMainResultGlyphNormalizationNoOp normalization,
        LegacyOwnedMainWordUtf8Projection[] words)
    { RawResult = raw; Snapshot = snapshot; GlyphNormalization = normalization; RawWordsInNativeHeadOrder = Array.AsReadOnly(words); }
    public byte[] CopyScoredAlternativeStorage400() => Snapshot.CopyScoredStorage400();
    public byte[] CopyFinalScoreStorage400() => Snapshot.CopyNormalizedScoreStorage400();
}

/// <summary>Post-success first-candidate composition, not native2271F0 failure
/// ordering or a complete273 alternative/exclusion/wrapper implementation.</summary>
public static class LegacyFreshMainFirstCandidateCompletion
{
    private sealed class CompletionClaim { internal int Started; }
    private static readonly ConditionalWeakTable<LegacyFreshMainSingleLinePlainRecognitionResult, CompletionClaim> claims = new();

    /// <summary>Single-use once quote execution begins, including failed or
    /// canceled prefixes. Pure precondition failures leave the candidate usable.
    /// Quotes can mutate the actual shared pool; failures never roll them back.</summary>
    public static LegacyFreshMainCompletedFirstCandidate Complete(
        LegacyFreshMainSingleLinePlainRecognitionResult actualResult, ReadOnlySpan<byte> allocation400,
        Span<byte> leadingCallerStorage4, Span<byte> trailingCallerStorage4, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualResult); token.ThrowIfCancellationRequested();
        if (allocation400.Length != 400 || leadingCallerStorage4.Length != 4 || trailingCallerStorage4.Length != 4 ||
            leadingCallerStorage4.Overlaps(trailingCallerStorage4))
            throw new ArgumentException("One complete400-byte alternative allocation and two separate4-byte scratch slots required.");
        //Pure ownership/copy preflight happens before the single-use mutation
        //claim. The scalar snapshot computes final score bytes early, without
        //mutating pool/query state; its postquote transport remains identical.
        var snapshot = LegacyOwnedMainFirstAlternativeSnapshot.Capture(actualResult, allocation400, token);
        var normalization = LegacyFreshMainResultGlyphNormalization.Prepare(snapshot.SourcePolicy, token);
        var claim = claims.GetValue(actualResult, static _ => new());
        if (Interlocked.CompareExchange(ref claim.Started, 1, 0) != 0)
            throw new InvalidOperationException("This actual first candidate has already entered completion, including failed prefixes.");
        token.ThrowIfCancellationRequested();
        LegacyOwnedMainPublishedQuoteNormalization.Apply(actualResult, leadingCallerStorage4, trailingCallerStorage4, token);
        var noOp = normalization.Apply(snapshot, token);

        //Native279.Head18/Next20 order, preserving distinct WORD records and
        //their real callback/source owners. Do not guess separators or rename
        //confusable labels to force a language result.
        var publications = actualResult.Publications.Where(p => p.PublishedWord is not null)
            .ToDictionary(p => p.PublishedWord!);
        var projected = new List<LegacyOwnedMainWordUtf8Projection>();
        for (var node = snapshot.WordHead18; node is not null; node = node.Next)
        {
            token.ThrowIfCancellationRequested();
            if (!publications.TryGetValue(node, out var publication))
                throw new InvalidDataException("Completed WORD node lost its exact original publication callback.");
            projected.Add(LegacyOwnedMainWordUtf8Projection.Project(publication, token));
        }
        token.ThrowIfCancellationRequested();
        return new(actualResult, snapshot, noOp, projected.ToArray());
    }
}
