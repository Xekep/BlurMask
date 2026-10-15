using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyOwnedMainGlyphConfidence.Probe")]
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
        LegacyPreparedFreshMain240AcousticPolicy policy,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();Model=model;Graph=graph;CaseDictionaryOwner=caseOwner;DirectDictionaryOwner=directOwner;
        this.score=score;this.sourceProbability=sourceProbability;
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
        return new(resource,owner.Model,owner.Graph,owner,null,scorer.Score,physical=>original.SourceStrokeProbabilities.Span[physical],scorer.Policy,token);
    }

    public static LegacyOwnedRussianMainGlyphConfidence Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshCaseRussianDictionaryPreparation owner,LegacyFreshMainSingleLineContext context,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);ArgumentNullException.ThrowIfNull(context);token.ThrowIfCancellationRequested();
        var scorer=owner.CreateAcousticScorer(resource,context,token);var source=scorer.Source;
        if(!ReferenceEquals(owner.Graph,source.SourceRanges.Graph))throw new ArgumentException("Same sampled main line graph required.");
        var original=source.Owner.Preparation.Input.Input.Input.Owner.Controller.Source;
        return new(resource,owner.Model,owner.Graph,owner,null,scorer.Score,physical=>original.SourceStrokeProbabilities.Span[physical],scorer.Policy,token);
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
            physical=>original.SourceStrokeProbabilities.Span[physical],scorer.Policy,token);
    }

    public static LegacyOwnedRussianMainGlyphConfidence Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedRussianDictionaryPreparation owner,LegacyFreshMainSingleLineContext context,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);ArgumentNullException.ThrowIfNull(context);token.ThrowIfCancellationRequested();
        var scorer=owner.CreateAcousticScorer(resource,context,token);var source=scorer.Source;
        var original=source.Owner.Preparation.Input.Input.Input.Owner.Controller.Source;
        return new(resource,owner.Model,source.SourceRanges.Graph,null,owner,scorer.Score,
            physical=>original.SourceStrokeProbabilities.Span[physical],scorer.Policy,token);
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
    {
        ArgumentNullException.ThrowIfNull(family);ArgumentNullException.ThrowIfNull(actualPrimitiveProbability);token.ThrowIfCancellationRequested();
        if(candidates.Length>150)throw new InvalidDataException("Bounded actual candidate cache required.");
        var result=new LegacyMainGlyphConfidenceCandidate[candidates.Length];
        if(family.Count!=7||family.Take(4).Any(f=>f is null||f.Value.Length!=180)||family.Skip(4).Any(f=>f is not null))
            throw new InvalidDataException("Actual four180-output EXP families required.");
        for(int i=0;i<candidates.Length;i++)
        {
            token.ThrowIfCancellationRequested();ushort symbol=candidates[i].Symbol;float confidence=0;
            if(i<20)
            {
                if(symbol==closed)
                {
                    //1A7970 returns stored ranges.1BCBD0 reads first physical
                    //range ID before218.V60, not chronologically sorted ranges.
                    confidence=actualPrimitiveProbability();token.ThrowIfCancellationRequested();
                    if(!float.IsFinite(confidence))throw new InvalidDataException("Finite original source probability required.");
                }
                else
                {
                    if(!labels.TryResolveOutputLabel(symbol,out ushort output))
                        throw new NotSupportedException("Actual retained symbol requires an unimplemented confidence service.");
                    //The original symbol indexes192x4 calibration; only the
                    //neural lookup uses3A0. Native order is family1+0+2+3+0+0+0.
                    float a=calibration.Calibrate(symbol,0,family[0]!.Value.Span[output],token);
                    float b=calibration.Calibrate(symbol,1,family[1]!.Value.Span[output],token);
                    float sum=b+a;
                    sum+=calibration.Calibrate(symbol,2,family[2]!.Value.Span[output],token);
                    sum+=calibration.Calibrate(symbol,3,family[3]!.Value.Span[output],token);
                    sum+=0f;sum+=0f;sum+=0f;sum+=0f;
                    confidence=sum/4; if(confidence>=1)confidence=1;
                }
            }
            result[i]=new(symbol,candidates[i].Cost,confidence);
        }
        token.ThrowIfCancellationRequested();return result;
    }
}

/// <summary>Owned confidence vector snapshots for one real sampled main group.</summary>
public sealed class LegacyOwnedRussianMainGlyphConfidenceGroup
{
    private readonly LegacyMainGlyphConfidenceCandidate[] candidates;
    public LegacyOwnedRussianMainGlyphConfidence Owner {get;}
    public int GroupIndex {get;}
    internal LegacyPreparedFreshMain240AcousticScores ActualAcousticScores {get;}
    public ReadOnlyMemory<LegacyMainGlyphConfidenceCandidate> Candidates=>candidates.ToArray();
    internal ReadOnlySpan<LegacyMainGlyphConfidenceCandidate> OwnedCandidates=>candidates;
    internal LegacyOwnedRussianMainGlyphConfidenceGroup(LegacyOwnedRussianMainGlyphConfidence owner,int group,
        LegacyMainGlyphConfidenceCandidate[] candidates,LegacyPreparedFreshMain240AcousticScores scores)
    {Owner=owner;GroupIndex=group;this.candidates=candidates;ActualAcousticScores=scores;}
    public int[] CopyFamilyProbabilities(int family)
    {if((uint)family>=4)throw new ArgumentOutOfRangeException(nameof(family));return ActualAcousticScores.Glyph.Scores.FamilyProbabilities[family]!.Value.ToArray();}
    /// <summary>1C1300 channel0; absent candidates return0.</summary>
    public float ReadSymbolScore(ushort symbol,CancellationToken token=default)
    {token.ThrowIfCancellationRequested();foreach(var candidate in candidates){token.ThrowIfCancellationRequested();if(candidate.Symbol==symbol)return candidate.Confidence;}return 0;}
    /// <summary>1C1520 returns the first candidate vector's channel0, or0.</summary>
    public float ReadFirstCandidateScore(CancellationToken token=default)
    {token.ThrowIfCancellationRequested();return candidates.Length==0?0:candidates[0].Confidence;}
}
