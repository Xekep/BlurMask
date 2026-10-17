namespace InkRecognition;

// Cached scores retain their original typed model owner. The shared numeric
// publisher cannot introduce a classifier or substitute candidate weights.
internal sealed class LegacyMainCompactGroupScores
{
    private readonly Func<ushort, CancellationToken, float> readWeight;
    internal ReadOnlyMemory<LegacyMainGlyphConfidenceCandidate> Candidates { get; }

    private LegacyMainCompactGroupScores(ReadOnlyMemory<LegacyMainGlyphConfidenceCandidate> candidates,
        Func<ushort, CancellationToken, float> readWeight)
    {
        Candidates = candidates;
        this.readWeight = readWeight;
    }

    internal float ReadNormalizedCandidateWeight(ushort symbol, CancellationToken token) => readWeight(symbol, token);

    internal static LegacyMainCompactGroupScores FromOld(LegacyOwnedRussianMainGlyphConfidenceGroup group) =>
        new(group.Candidates, group.ReadNormalizedCandidateWeight);

    internal static LegacyMainCompactGroupScores FromGlobal(LegacyFreshMainGlobalGlyphConfidenceGroup group) =>
        new(group.Candidates, group.ReadNormalizedCandidateWeight);
}
