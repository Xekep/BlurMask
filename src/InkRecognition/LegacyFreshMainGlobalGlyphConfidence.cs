using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Actual global main confidence from the source session's one D0 NN
/// evaluation. It accepts no replacement probabilities, masks or candidate costs.</summary>
public sealed class LegacyFreshMainGlobalGlyphConfidence
{
    private readonly LegacyMainConfidenceCalibration calibration;
    private readonly LegacyAcousticConfidenceLabelMap labels;
    private readonly ushort closed;
    private readonly Dictionary<int,LegacyFreshMainGlobalGlyphConfidenceGroup> groups=[];
    private int active;
    public LegacyFreshMainGlobalCaseSession Owner{get;}
    public LegacyFreshMainCompactOrdinaryGraph Graph=>Owner.Graph;
    public byte OwnerScoreChannel88=>0;
    private LegacyFreshMainGlobalGlyphConfidence(ReadOnlySpan<byte> resource,LegacyFreshMainGlobalCaseSession owner,CancellationToken token)
    {
        Owner=owner;var alphabet=owner.Model.AcousticAlphabet;
        if(resource.Length!=572763||!Convert.ToHexString(SHA256.HashData(resource)).Equals(
            Graph.Source.Source.SourceFingerprint,StringComparison.OrdinalIgnoreCase)||
            owner.Model.SourceKind!=0x135||alphabet.LabelCount!=192||owner.Acoustic.CostScale!=1||
            (owner.Acoustic.Policy.FrontendFlags&0x800)!=0)
            throw new ArgumentException("Actual global main0 source/model with ordinary four-family confidence required.");
        calibration=LegacyMainConfidenceCalibration.ReadRussianSource(resource,token);
        var knowledge=LegacyOrdinarySymbolMaskProducer.ReadV110KnowledgeGroups(resource[189734..],alphabet.LabelCount,out _,token);
        labels=new(alphabet,resource.Slice(99,16),180,knowledge,token);
        if(!alphabet.TryGetLabel("\U000f0001",out closed)||labels.TryResolveOutputLabel(closed,out _)||
            LegacyWideSymbolPairs.CreateUnrestricted(alphabet,token).Length!=0)
            throw new NotSupportedException("Actual source-probability closed primitive and no-wide Russian branch required.");
    }
    public static LegacyFreshMainGlobalGlyphConfidence Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalCaseSession owner,CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(owner);token.ThrowIfCancellationRequested();return new(resource,owner,token);}
    public LegacyFreshMainGlobalGlyphConfidenceGroup ReadGroup(int group,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(Interlocked.CompareExchange(ref active,1,0)!=0)throw new InvalidOperationException("Global confidence reads must be serialized.");
        try
        {
            //Validate the actual source even on repeated reads. This never
            //prepares a new span or recalculates the classifier/cache.
            var snapshot=Owner.ReadAcousticSnapshot(group,token);
            if(groups.TryGetValue(group,out var existing))return existing;
            var candidates=LegacyMainGlyphConfidenceKernel.Evaluate(snapshot.Candidates,snapshot.ReadFamilies(),
                ()=>snapshot.SourceProbability,calibration,labels,closed,token);
            var value=new LegacyFreshMainGlobalGlyphConfidenceGroup(this,group,snapshot,candidates,token);
            token.ThrowIfCancellationRequested();groups.Add(group,value);return value;
        }
        finally{Volatile.Write(ref active,0);}
    }
}

/// <summary>Privately owned scalar/vector snapshot. Every public array/memory
/// snapshot is copied; no returned mutation changes later native channel reads.</summary>
public sealed class LegacyFreshMainGlobalGlyphConfidenceGroup
{
    private readonly LegacyGlobalMainAcousticSnapshot snapshot;
    private readonly LegacyMainGlyphConfidenceCandidate[] candidates;
    private readonly float[] weights;
    public LegacyFreshMainGlobalGlyphConfidence Owner{get;}
    public int GroupIndex{get;}
    public float GroupPowerScale=>snapshot.CostScale;
    public float FirstStoredSourceProbability=>snapshot.SourceProbability;
    public ReadOnlyMemory<LegacyMainGlyphConfidenceCandidate> Candidates=>candidates.ToArray();
    internal LegacyFreshMainGlobalGlyphConfidenceGroup(LegacyFreshMainGlobalGlyphConfidence owner,int group,
        LegacyGlobalMainAcousticSnapshot snapshot,LegacyMainGlyphConfidenceCandidate[] candidates,CancellationToken token)
    {Owner=owner;GroupIndex=group;this.snapshot=snapshot;this.candidates=candidates;
        weights=LegacyOwnedRussianMainGlyphConfidenceGroup.CalculateCandidateWeights(candidates.Select(c=>c.Cost).ToArray(),snapshot.CostScale,token);}
    public int[] CopyFamilyProbabilities(int family)=>snapshot.CopyFamily(family);
    public int[] CopyRawIntegerCosts()=>snapshot.CopyRawCosts();
    public int[] CopyFinalClassCosts()=>snapshot.CopyFinalCosts();
    public float ReadSymbolScore(ushort symbol,CancellationToken token=default)
    {token.ThrowIfCancellationRequested();foreach(var c in candidates){token.ThrowIfCancellationRequested();if(c.Symbol==symbol)return c.Confidence;}return 0;}
    public float ReadFirstCandidateScore(CancellationToken token=default)
    {token.ThrowIfCancellationRequested();return candidates.Length==0?0:candidates[0].Confidence;}
    public float ReadNormalizedCandidateWeight(ushort symbol,CancellationToken token=default)
    {token.ThrowIfCancellationRequested();for(int i=0;i<candidates.Length;i++){token.ThrowIfCancellationRequested();if(candidates[i].Symbol==symbol)return weights[i];}return 0;}
}
