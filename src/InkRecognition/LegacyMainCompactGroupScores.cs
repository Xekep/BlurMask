namespace InkRecognition;

// Cached scores retain their original typed model owner. The shared numeric
// publisher cannot introduce a classifier or substitute candidate weights.
internal sealed partial class LegacyMainCompactGroupScores
{
    private readonly Func<ushort, CancellationToken, float> readWeight;
    private readonly ushort[] symbols;
    internal ReadOnlyMemory<ushort> CandidateSymbols=>symbols;

    private LegacyMainCompactGroupScores(ReadOnlyMemory<LegacyMainGlyphConfidenceCandidate> candidates,
        Func<ushort, CancellationToken, float> readWeight)
    {
        symbols=new ushort[candidates.Length];
        for(int i=0;i<symbols.Length;i++)symbols[i]=candidates.Span[i].Symbol;
        this.readWeight = readWeight;
    }

    internal float ReadNormalizedCandidateWeight(ushort symbol, CancellationToken token) => readWeight(symbol, token);

    internal static LegacyMainCompactGroupScores FromOld(LegacyOwnedRussianMainGlyphConfidenceGroup group) =>
        new(group.Candidates, group.ReadNormalizedCandidateWeight);

    internal static LegacyMainCompactGroupScores FromGlobal(LegacyFreshMainGlobalGlyphConfidenceGroup group) =>
        new(group.Candidates, group.ReadNormalizedCandidateWeight);
}
