using System.Runtime.CompilerServices;

namespace InkRecognition;

/// <summary>21FBF0 over the completed actual global main WORD list. This
/// mutates the shared glyph pool, not the scalar alternative snapshot. Final
/// glyph normalization, document transport and text acceptance are separate.</summary>
public static class LegacyFreshMainGlobalPublishedQuoteNormalization
{
    private sealed class Claim { internal int Started; }
    private static readonly ConditionalWeakTable<LegacyFreshMainGlobalGlyphExecutionSession, Claim> claims = new();

    /// <summary>Single-use per actual callback session once mutation starts,
    /// including exceptions/cancellation. Two separate caller scratch slots are
    /// carried across the native prepend-order WORD list. No control alphabet
    /// is substituted with the primary acoustic alphabet.</summary>
    public static void Apply(LegacyFreshMainGlobalFirstAlternativeSnapshot actualSnapshot,
        Span<byte> leadingCallerStorage4, Span<byte> trailingCallerStorage4,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualSnapshot); token.ThrowIfCancellationRequested();
        if (leadingCallerStorage4.Length != 4 || trailingCallerStorage4.Length != 4 ||
            leadingCallerStorage4.Overlaps(trailingCallerStorage4))
            throw new ArgumentException("Two distinct complete four-byte caller scratch slots required.");
        var session = actualSnapshot.Session;
        var ownership = session.Ownership;
        var policy = actualSnapshot.SourcePolicy;
        actualSnapshot.Terminal.PassResult.ValidateResultQueryLease(token);
        if (!session.IsComplete || session.IsStopped ||
            !ReferenceEquals(actualSnapshot.Terminal, ownership.Materialization.TerminalResult) ||
            !ReferenceEquals(actualSnapshot.WordHead18, ownership.WordPublisher.Head) ||
            !ReferenceEquals(actualSnapshot.BeamOwner20, ownership.Materialization) ||
            !ReferenceEquals(ownership.Geometry.Policy, policy))
            throw new InvalidOperationException("The completed global WORD list and exact result owner must remain current.");
        var words = new List<LegacyNativePlainWordRecord>();
        for (var node = actualSnapshot.WordHead18; node is not null; node = node.Next)
        {
            token.ThrowIfCancellationRequested();
            if (words.Count == 4096 || !ReferenceEquals(node.Word.Pool, ownership.GlyphPool))
                throw new ArgumentException("Bounded same-pool actual published WORD list required.");
            words.Add(node.Word);
        }
        if (words.Count != actualSnapshot.PublishedWordCount)
            throw new InvalidOperationException("The captured global published WORD count changed.");
        var normalizer = LegacyNativeWordQuoteNormalization.Prepare(policy.Owner.Model.AcousticAlphabet, token);
        var claim = claims.GetValue(session, static _ => new());
        if (Interlocked.CompareExchange(ref claim.Started, 1, 0) != 0)
            throw new InvalidOperationException("This actual global callback session already entered quote cleanup.");
        normalizer.NormalizePublished(ownership.GlyphPool, words, leadingCallerStorage4,
            trailingCallerStorage4, null, token);
        actualSnapshot.Terminal.PassResult.ValidateResultQueryLease(token);
    }
}
