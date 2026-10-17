using System.Buffers.Binary;

namespace InkRecognition.Math;

internal sealed record LegacyPreparedMathEosScore(int Rank,int SourceBeam,float CandidateCost,
    float CompletedScore,ushort[] Prefix);

/// <summary>Positive prepared delta/EOS numerical handoff on copied owned raw
/// candidates. No037430 allocation lifecycle, max-step fallback, acceptance,
/// native completion record construction or actual network loop is claimed.</summary>
internal sealed class LegacyPreparedMathEosHandoff
{
    private readonly byte[] afterPruning,afterEos,afterResort;
    internal LegacyPreparedMathUnrestrictedCandidates Source {get;}
    internal LegacyPreparedMathEosScore[] Scores {get;}
    internal int ActiveCount {get;}
    private LegacyPreparedMathEosHandoff(LegacyPreparedMathUnrestrictedCandidates source,byte[] pruned,
        byte[] eos,byte[] sorted,LegacyPreparedMathEosScore[] scores,int active)
    {Source=source;afterPruning=pruned;afterEos=eos;afterResort=sorted;Scores=scores;ActiveCount=active;}
    internal byte[] CopyAfterPruning()=>afterPruning.ToArray();
    internal byte[] CopyAfterEos()=>afterEos.ToArray();
    internal byte[] CopyAfterResort()=>afterResort.ToArray();
    internal static LegacyPreparedMathEosHandoff Apply(LegacyPreparedMathUnrestrictedCandidates actualSource,
        float delta,LegacyMath2ObservableTokens actualGate,IReadOnlyList<ushort[]> preparedPrefixes,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualSource);ArgumentNullException.ThrowIfNull(actualGate);
        ArgumentNullException.ThrowIfNull(preparedPrefixes);token.ThrowIfCancellationRequested();
        if(actualSource.VocabularyCount!=297||preparedPrefixes.Count!=actualSource.ActiveBeams)
            throw new ArgumentException("The same supported297-class source and complete prepared prefix sidecars required.");
        var prefixes=new ushort[preparedPrefixes.Count][];
        for(int i=0;i<prefixes.Length;i++)
        {
            var row=preparedPrefixes[i];if(row is null||row.Length>1_000_000||row.Any(value=>value>=297))
                throw new NotSupportedException("Bounded complete decoder-prefix tokens required.");
            prefixes[i]=row.ToArray();
        }
        byte[] records=actualSource.CopySortedStorage();int capacity=actualSource.BeamWidth*2;
        float Cost(int rank)=>BinaryPrimitives.ReadSingleLittleEndian(records.AsSpan(rank*48));
        void Mask(int rank)=>BinaryPrimitives.WriteSingleLittleEndian(records.AsSpan(rank*48),float.MaxValue);
        //The successful prepared completion-score branch has finite nonnegative
        //costs. Native NaN/error/allocation paths are separate, not normalized.
        for(int i=0;i<capacity;i++)if(!float.IsFinite(Cost(i))||Cost(i)<0)
            throw new NotSupportedException("Finite nonnegative prepared candidate costs required.");
        if(delta<1000f)
        {
            float threshold=Cost(0)+delta;
            for(int i=1;i<capacity;i++)if(Cost(i)>=threshold)Mask(i);
        }
        var pruned=records.ToArray();var scores=new List<LegacyPreparedMathEosScore>();
        for(int rank=0;rank<capacity;rank++)
        {
            token.ThrowIfCancellationRequested();
            if(BinaryPrimitives.ReadUInt64LittleEndian(records.AsSpan(rank*48+8))!=1||Cost(rank)==float.MaxValue)continue;
            int beam=BinaryPrimitives.ReadInt32LittleEndian(records.AsSpan(rank*48+40));
            if(beam<0||beam>=prefixes.Length)throw new ArgumentException("Candidate prefix owner is outside the retained source.");
            var prefix=prefixes[beam];
            if(rank<actualSource.BeamWidth&&prefix.Any(actualGate.IsObservable))
                scores.Add(new(rank,beam,Cost(rank),LegacyPreparedMathCompletionScore.WithoutConstraintModel(Cost(rank),prefix.Length,token),prefix.ToArray()));
            Mask(rank);
        }
        var eos=records.ToArray();records=LegacyPreparedMathRawCandidateOrdering.Sort(records,token);
        int active=1;while(active<actualSource.BeamWidth&&Cost(active)!=float.MaxValue)active++;
        return new(actualSource,pruned,eos,records,scores.ToArray(),active);
    }
}
