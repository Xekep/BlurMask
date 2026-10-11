namespace InkRecognition;

public enum LegacyAcousticCandidateRejection { None,ImpossibleClass,KindThreshold,TotalLimit }
public readonly record struct LegacyAcousticCandidateScore(int ClassCost,int GeometryCost,
    int PreviousPathCost,int TotalCost,bool Accepted,LegacyAcousticCandidateRejection Rejection);

/// <summary>
/// Checked acoustic21d virtual58 (1c0fd0): class gate, five symbol geometry
/// bypasses, supplied composite-category cost, prior path addition and strict
/// budget. It neither maps labels to lexicon nor recovers search orchestration.
/// </summary>
public sealed class LegacyPreparedAcousticCandidateScorer
{
    private readonly LegacyAcousticCandidateThresholds thresholds;
    private readonly ushort[] bypassLabels;
    public LegacyPreparedAcousticCandidateScorer(LegacyAcousticCandidateThresholds thresholds,
        ReadOnlySpan<ushort> fiveGeometryBypassLabels)
    {
        thresholds.Validate();
        if(fiveGeometryBypassLabels.Length!=5)throw new ArgumentException("Expected five explicitly resolved acoustic labels.",nameof(fiveGeometryBypassLabels));
        this.thresholds=thresholds;bypassLabels=fiveGeometryBypassLabels.ToArray();
    }
    /// <summary>Native1b0760 resolves these exact UTF8 strings; missing labels remain65535.</summary>
    public static LegacyPreparedAcousticCandidateScorer ForAlphabet(LegacyAcousticCandidateThresholds thresholds,
        LegacyInkAlphabet alphabet)
    {
        ArgumentNullException.ThrowIfNull(alphabet);
        var labels=new ushort[5];int i=0;
        foreach(string symbol in new[]{"\U000f0001","\U000f0002","\U000f0003","\U000f0004","\U000f0008"})
            labels[i++]=alphabet.TryGetLabel(symbol,out ushort label) ? label:ushort.MaxValue;
        return new(thresholds,labels);
    }
    public LegacyAcousticCandidateScore Evaluate(ReadOnlySpan<int> classCosts,ushort acousticSymbol,
        ReadOnlySpan<int> compositeCategoryCosts,int compositeCategory,int candidateKind,
        int previousPathCost,int totalCostLimit,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(classCosts.Length is <1 or >65535 || acousticSymbol>=classCosts.Length)
            throw new ArgumentException("Acoustic symbol is outside supplied class-cost cache.",nameof(acousticSymbol));
        int classCost=classCosts[acousticSymbol];
        if(classCost is <0 or >0x4000000)throw new ArgumentException("Class cost is outside the checked native domain.",nameof(classCosts));
        if(candidateKind<0||totalCostLimit is <0 or >0x4000000)
            throw new ArgumentOutOfRangeException(nameof(candidateKind));
        if(classCost==0x4000000)return Rejected(LegacyAcousticCandidateRejection.ImpossibleClass);
        int threshold=candidateKind switch{1=>thresholds.KindOne,2=>thresholds.KindTwo,3=>thresholds.KindThree,_=>0x4000000};
        if(classCost>threshold)return Rejected(LegacyAcousticCandidateRejection.KindThreshold);
        int geometryCost=0;
        if(!bypassLabels.AsSpan().Contains(acousticSymbol))
        {
            if(compositeCategoryCosts.Length is <1 or >127 || compositeCategory<0 || compositeCategory>=compositeCategoryCosts.Length)
                throw new ArgumentException("Composite category is outside supplied geometry-cost cache.",nameof(compositeCategory));
            geometryCost=compositeCategoryCosts[compositeCategory];
            if(geometryCost is <0 or >0x4000000)throw new ArgumentException("Geometry cost exceeds checked native domain.",nameof(compositeCategoryCosts));
        }
        // Native signed additions can wrap. Reject overflow; signed previous
        // lexical path costs remain explicit and are not clamped to zero.
        int total=checked(checked(geometryCost+previousPathCost)+classCost);
        bool accepted=total<totalCostLimit;
        return new(classCost,geometryCost,previousPathCost,total,accepted,
            accepted ? LegacyAcousticCandidateRejection.None:LegacyAcousticCandidateRejection.TotalLimit);

        LegacyAcousticCandidateScore Rejected(LegacyAcousticCandidateRejection why)=>
            new(classCost,0,previousPathCost,0x4000000,false,why);
    }
}
