namespace InkRecognition;

//Private immutable data captured from the one actual D0 classifier evaluation.
//No recoverable public ReadOnlyMemory buffer is stored or exposed here.
internal sealed class LegacyGlobalMainAcousticSnapshot
{
    private readonly int[][] families;
    private readonly int[] rawCosts,finalCosts;
    private readonly LegacyAcousticCacheCandidate[] candidates;
    internal float SourceProbability{get;}
    internal float CostScale{get;}
    internal ReadOnlySpan<LegacyAcousticCacheCandidate> Candidates=>candidates;
    internal LegacyGlobalMainAcousticSnapshot(LegacyPreparedFreshMain240AcousticScores scores,float probability,float scale)
    {
        var source=scores.Glyph.Scores.FamilyProbabilities;
        if(source.Count!=7||source.Take(4).Any(f=>f is null||f.Value.Length!=180)||source.Skip(4).Any(f=>f is not null)||
            !float.IsFinite(probability)||scale!=1)
            throw new InvalidDataException("Actual global four180-family vectors, source probability and disabled power scale required.");
        families=source.Take(4).Select(f=>f!.Value.ToArray()).ToArray();
        rawCosts=scores.Glyph.IntegerCosts.ToArray();finalCosts=scores.Cache.Costs.ToArray();candidates=scores.Cache.Candidates.ToArray();
        SourceProbability=probability;CostScale=scale;
    }
    internal IReadOnlyList<ReadOnlyMemory<int>?> ReadFamilies()
        =>Array.AsReadOnly(families.Select(f=>(ReadOnlyMemory<int>?)f).Concat(new ReadOnlyMemory<int>?[3]).ToArray());
    internal int[] CopyFamily(int family)
    {if((uint)family>=4)throw new ArgumentOutOfRangeException(nameof(family));return families[family].ToArray();}
    internal int[] CopyRawCosts()=>rawCosts.ToArray();
    internal int[] CopyFinalCosts()=>finalCosts.ToArray();
}

public sealed partial class LegacyFreshMainGlobalCaseSession
{
    private readonly Dictionary<int,LegacyGlobalMainAcousticSnapshot> acousticSnapshots=[];
    internal LegacyGlobalMainAcousticSnapshot ReadAcousticSnapshot(int group,CancellationToken token)
    {
        Enter(token);
        try
        {
            if((uint)group>=(uint)graph.GroupCount)throw new ArgumentOutOfRangeException(nameof(group));
            return acousticSnapshots.TryGetValue(group,out var value)?value:
                throw new InvalidOperationException("The actual D0 controller has not evaluated this global group; confidence never runs a replacement NN.");
        }
        finally{Exit();}
    }
}
