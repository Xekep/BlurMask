using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyOwnedMainGlyphConfidence.Probe")]
[assembly:InternalsVisibleTo("LegacyMainCandidateWeight.Probe")]
namespace InkRecognition;

public readonly record struct LegacyMainGlyphConfidenceCandidate(ushort Symbol,int Cost,float Confidence);

/// <summary>Actual main0 group confidence, retaining its DIRECT or case dictionary, sampled
/// graph, original source probability and owned four-family scorer. This restores
/// the ordinary1BCBD0 scalar vectors, not a recognition winner or UTF8 publication.</summary>
public sealed class LegacyOwnedRussianMainGlyphConfidence
{
    private readonly Func<int,CancellationToken,LegacyPreparedFreshMain240AcousticScores> score;
    private readonly Func<int,float> sourceProbability;
    private readonly LegacyMainConfidenceCalibration calibration;
    private readonly LegacyAcousticConfidenceLabelMap labels;
    private readonly ushort closed;
    private readonly float groupPowerScale;
    internal float NativeGroupPowerScale=>groupPowerScale;
    public LegacyFreshCaseRussianDictionaryPreparation? CaseDictionaryOwner {get;}
    public LegacyOwnedRussianDictionaryPreparation? DirectDictionaryOwner {get;}
    public LegacyRussianModelBundle Model {get;}
    public LegacyOrdinaryCursiveCandidateGraph Graph {get;}
    /// <summary>NEW00C860's zeroed88, without property/reuse overrides.</summary>
    public byte OwnerScoreChannel88=>0;

    private LegacyOwnedRussianMainGlyphConfidence(ReadOnlySpan<byte> resource,
        LegacyRussianModelBundle model,LegacyOrdinaryCursiveCandidateGraph graph,
        LegacyFreshCaseRussianDictionaryPreparation? caseOwner,LegacyOwnedRussianDictionaryPreparation? directOwner,
        Func<int,CancellationToken,LegacyPreparedFreshMain240AcousticScores> score,Func<int,float> sourceProbability,
        LegacyPreparedFreshMain240AcousticPolicy policy,float groupPowerScale,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();Model=model;Graph=graph;CaseDictionaryOwner=caseOwner;DirectDictionaryOwner=directOwner;
        this.score=score;this.sourceProbability=sourceProbability;
        if(groupPowerScale!=1)throw new NotSupportedException("Actual main raw100=0 group scale1 required.");
        this.groupPowerScale=groupPowerScale;
        if((policy.FrontendFlags&0x800)!=0)throw new NotSupportedException("Individual network confidence channels are a separate branch.");
        calibration=LegacyMainConfidenceCalibration.ReadRussianSource(resource,token);
        var alphabet=model.AcousticAlphabet;
        var groups=LegacyOrdinarySymbolMaskProducer.ReadV110KnowledgeGroups(resource[189734..],alphabet.LabelCount,out _,token);
        labels=new(alphabet,resource.Slice(99,16),180,groups,token);
        if(!alphabet.TryGetLabel("\U000f0001",out closed)||labels.TryResolveOutputLabel(closed,out _))
            throw new NotSupportedException("Actual closed primitive with source21860 probability required.");
        if(LegacyWideSymbolPairs.CreateUnrestricted(alphabet,token).Length!=0)
            throw new NotSupportedException("Wide confidence service is not part of this actual Russian branch.");
        token.ThrowIfCancellationRequested();
    }

    public static LegacyOwnedRussianMainGlyphConfidence Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshCaseRussianDictionaryPreparation owner,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);token.ThrowIfCancellationRequested();
        var scorer=owner.CreateAcousticScorer(resource,token);var source=scorer.Source;
        if(!ReferenceEquals(owner.Graph,source.SourceRanges.Graph))throw new ArgumentException("Same sampled main graph required.");
        var original=source.Owner.Postprocessing.Input.Controller.Source;
        return new(resource,owner.Model,owner.Graph,owner,null,scorer.Score,physical=>original.SourceStrokeProbabilities.Span[physical],scorer.Policy,scorer.CostScale,token);
    }

    public static LegacyOwnedRussianMainGlyphConfidence Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshCaseRussianDictionaryPreparation owner,LegacyFreshMainSingleLineContext context,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);ArgumentNullException.ThrowIfNull(context);token.ThrowIfCancellationRequested();
        var scorer=owner.CreateAcousticScorer(resource,context,token);var source=scorer.Source;
        if(!ReferenceEquals(owner.Graph,source.SourceRanges.Graph))throw new ArgumentException("Same sampled main line graph required.");
        var original=source.Owner.Preparation.Input.Input.Input.Owner.Controller.Source;
        return new(resource,owner.Model,owner.Graph,owner,null,scorer.Score,physical=>original.SourceStrokeProbabilities.Span[physical],scorer.Policy,scorer.CostScale,token);
    }

    /// <summary>Retain the actual DIRECT owner and its mask/scorer; no parallel
    /// case dictionary or case alphabet is manufactured for this path.</summary>
    public static LegacyOwnedRussianMainGlyphConfidence Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedRussianDictionaryPreparation owner,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);token.ThrowIfCancellationRequested();
        var scorer=owner.CreateAcousticScorer(resource,token);var source=scorer.Source;
        var original=source.Owner.Postprocessing.Input.Controller.Source;
        return new(resource,owner.Model,source.SourceRanges.Graph,null,owner,scorer.Score,
            physical=>original.SourceStrokeProbabilities.Span[physical],scorer.Policy,scorer.CostScale,token);
    }

    public static LegacyOwnedRussianMainGlyphConfidence Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedRussianDictionaryPreparation owner,LegacyFreshMainSingleLineContext context,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);ArgumentNullException.ThrowIfNull(context);token.ThrowIfCancellationRequested();
        var scorer=owner.CreateAcousticScorer(resource,context,token);var source=scorer.Source;
        var original=source.Owner.Preparation.Input.Input.Input.Owner.Controller.Source;
        return new(resource,owner.Model,source.SourceRanges.Graph,null,owner,scorer.Score,
            physical=>original.SourceStrokeProbabilities.Span[physical],scorer.Policy,scorer.CostScale,token);
    }

    /// <summary>Scores the actual graph group internally. No prepared replacement
    /// cache, probability, mask, alias list or owner88 channel is accepted.</summary>
    public LegacyOwnedRussianMainGlyphConfidenceGroup ScoreGroup(int groupIndex,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if((uint)groupIndex>=(uint)Graph.Groups.Count)throw new ArgumentOutOfRangeException(nameof(groupIndex));
        var actual=score(groupIndex,token);var candidates=actual.Cache.Candidates.Span;
        var result=EvaluateCandidates(candidates,actual.Glyph.Scores.FamilyProbabilities,
            ()=>sourceProbability(Graph.Groups[groupIndex].Ranges.Span[0].StrokeIndex),token);
        token.ThrowIfCancellationRequested();return new(this,groupIndex,result,actual);
    }

    internal LegacyMainGlyphConfidenceCandidate[] EvaluateCandidates(ReadOnlySpan<LegacyAcousticCacheCandidate> candidates,
        IReadOnlyList<ReadOnlyMemory<int>?> family,Func<float> actualPrimitiveProbability,CancellationToken token=default)
        =>LegacyMainGlyphConfidenceKernel.Evaluate(candidates,family,actualPrimitiveProbability,calibration,labels,closed,token);
}

/// <summary>Owned confidence vector snapshots for one real sampled main group.</summary>
public sealed class LegacyOwnedRussianMainGlyphConfidenceGroup
{
    private readonly LegacyMainGlyphConfidenceCandidate[] candidates;
    private readonly float[] normalizedCandidateWeights;
    public LegacyOwnedRussianMainGlyphConfidence Owner {get;}
    public int GroupIndex {get;}
    public float GroupPowerScale=>Owner.NativeGroupPowerScale;
    internal LegacyPreparedFreshMain240AcousticScores ActualAcousticScores {get;}
    public ReadOnlyMemory<LegacyMainGlyphConfidenceCandidate> Candidates=>candidates.ToArray();
    internal ReadOnlySpan<LegacyMainGlyphConfidenceCandidate> OwnedCandidates=>candidates;
    internal LegacyOwnedRussianMainGlyphConfidenceGroup(LegacyOwnedRussianMainGlyphConfidence owner,int group,
        LegacyMainGlyphConfidenceCandidate[] candidates,LegacyPreparedFreshMain240AcousticScores scores)
    {Owner=owner;GroupIndex=group;this.candidates=candidates;ActualAcousticScores=scores;
        normalizedCandidateWeights=CalculateCandidateWeights(candidates.Select(c=>c.Cost).ToArray(),GroupPowerScale);}
    public int[] CopyFamilyProbabilities(int family)
    {if((uint)family>=4)throw new ArgumentOutOfRangeException(nameof(family));return ActualAcousticScores.Glyph.Scores.FamilyProbabilities[family]!.Value.ToArray();}
    /// <summary>1C1300 channel0; absent candidates return0.</summary>
    public float ReadSymbolScore(ushort symbol,CancellationToken token=default)
    {token.ThrowIfCancellationRequested();foreach(var candidate in candidates){token.ThrowIfCancellationRequested();if(candidate.Symbol==symbol)return candidate.Confidence;}return 0;}
    /// <summary>1C1520 returns the first candidate vector's channel0, or0.</summary>
    public float ReadFirstCandidateScore(CancellationToken token=default)
    {token.ThrowIfCancellationRequested();return candidates.Length==0?0:candidates[0].Confidence;}

    /// <summary>Frontend150 ->ordinary21D.V78=1C1260 returns candidate24.field0,
    /// normalized by1BE530. This is NOT the vector10 confidence returned by1B8.
    /// The first matching candidate wins; an absent label returns0.</summary>
    public float ReadNormalizedCandidateWeight(ushort symbol,CancellationToken token=default)
    {token.ThrowIfCancellationRequested();for(int i=0;i<candidates.Length;i++){token.ThrowIfCancellationRequested();if(candidates[i].Symbol==symbol)return normalizedCandidateWeights[i];}return 0;}

    internal static float[] CalculateCandidateWeights(ReadOnlySpan<int> costs,float groupScale,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(costs.Length>150||!float.IsFinite(groupScale)||groupScale<=0)throw new ArgumentException("Bounded actual candidate costs and finite positive group scale required.");
        float[] result=new float[costs.Length];if(costs.IsEmpty)return result;
        if(costs.Length==1){result[0]=1;return result;}
        int count=Math.Min(20,costs.Length);float denominator=groupScale*8192f,sum=0;
        for(int i=0;i<count;i++)
        {token.ThrowIfCancellationRequested();result[i]=(float)Math.Exp((double)(-(float)costs[i]/denominator));sum+=result[i];}
        for(int i=0;i<count;i++){token.ThrowIfCancellationRequested();result[i]/=sum;}
        return result;
    }
}
