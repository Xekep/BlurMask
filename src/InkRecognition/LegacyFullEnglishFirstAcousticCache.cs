using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Fresh no-property/no-linguistic FIRST119 ensemble and integer220
/// cache over actual candidate ownership. No caller masks, scope, groups or
/// costs are accepted. Confidence and word controller remain separate.</summary>
public sealed class LegacyFullEnglishFirstAcousticCache
{
    public LegacyFullEnglishFirstCandidates Owner {get;}
    public LegacyFirstAcoustic119WireModel Model=>Owner.Model;
    public uint KnowledgeFlags {get;}
    public bool PruneCandidates {get;}
    public int PositiveGroupCount=>groups.Length;
    public int ScoredGroupCount {get{lock(gate)return completed.Count;}}
    private readonly object gate=new();
    private readonly LegacyFullEnglishFirstBudget budget;
    private readonly Dictionary<int,LegacyFullEnglishFirstCachedGroup> completed=[];
    private readonly byte[] primary,secondary;
    private readonly LegacySymbolMaskKnowledgeGroup[] groups;
    private readonly LegacyOrdinaryAcousticClassCache cache;
    private readonly ushort disabledPrototype;
    private LegacyFullEnglishFirstAcousticCache(LegacyFullEnglishFirstCandidates owner,uint flags,bool prune,
        LegacyAcousticSymbolMasks masks,LegacySymbolMaskKnowledgeGroup[] groups,LegacyOrdinaryAcousticClassCache cache,ushort disabled,LegacyFullEnglishFirstBudget budget)
    {Owner=owner;KnowledgeFlags=flags;PruneCandidates=prune;primary=masks.Primary.ToArray();secondary=masks.Secondary.ToArray();this.groups=groups;this.cache=cache;disabledPrototype=disabled;this.budget=budget;}

    internal static LegacyFullEnglishFirstAcousticCache Prepare(LegacyFullEnglishFirstCandidates owner,LegacyFullEnglishFirstBudget budget,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(owner);token.ThrowIfCancellationRequested();var model=owner.Model;var source=model.Source;
        byte[] A(string name)=>model.Fields.Single(f=>f.Owner=="21D"&&f.Destination==name).CopyBytes();
        float F(string name)=>BinaryPrimitives.ReadSingleLittleEndian(A(name));
        int I(string name)=>BinaryPrimitives.ReadInt32LittleEndian(A(name));
        ushort H(string name)=>BinaryPrimitives.ReadUInt16LittleEndian(A(name));
        if(model.CostMultiplier!=1||model.Slots[0].Mode!=2||model.Slots[1].Mode!=2||
            model.Slots.Take(4).Any(s=>s.Mode!=2||s.Networks.Count!=1)||model.Slots.Skip(4).Any(s=>s.Mode!=0||s.Weight!=0||s.Networks.Count!=0))
            throw new NotSupportedException("Fresh unit multiplier/four actual full mode2 families required.");
        foreach(string flag in new[]{"78","80","B8","148/150.present"})if(A(flag)[0]!=0)
            throw new NotSupportedException("Optional first119 cache/relationship/size payload remains unsupported.");
        if(F("100")!=0||I("108")!=0||I("128")!=0)
            throw new NotSupportedException("Power, calibration or correction payload remains unsupported.");
        //053F20 does invoke0573B0. The pinned actual AL's script scan makes
        //that builder return before allocating88; a different AL needs its
        //own derived-form owner and must not inherit this absence proof.
        byte[] alphabetBytes=source.Fields.Single(f=>f.Owner=="2B"&&f.Destination=="payload").CopyBytes();
        if(!Convert.ToHexString(SHA256.HashData(alphabetBytes)).Equals("5554E287937D2D8682C46EB7D85A701B15F1B86AFB45F55228DBC8C1EBF7A1FF",StringComparison.Ordinal))
            throw new NotSupportedException("The audited actual full-FIRST no-Arabic stored alphabet is required.");
        var alphabet=source.Alphabet;byte[] header=alphabet.CopySerializedHeader();
        if(LegacyWideSymbolPairs.CreateUnrestricted(alphabet,token).Length!=0)
            throw new NotSupportedException("Nonempty wide eligibility is not restored by this fresh119 endpoint.");
        var profile=new LegacyOrdinaryAcousticCacheProfile(F("7C"),F("A4"),F("A8"),F("AC"),F("B0"),F("B4"),I("E8"),H("EC"),H("EE"));
        if(profile.LongDashWeight==0&&alphabet.TryGetLabel("\u30fc",out _)||profile.CjkDashWeight==0&&alphabet.TryGetLabel("\u4e00",out _))
            throw new NotSupportedException("Zero-weight nonfinite CJK primitive conversion remains unsupported.");
        int count=H("C8");var groups=new LegacySymbolMaskKnowledgeGroup[count];
        if(count!=0)
        {
            byte[] ends=A("D0"),labels=A("D8");
            for(int i=0;i<count;i++)
            {
                token.ThrowIfCancellationRequested();int first=BinaryPrimitives.ReadUInt16LittleEndian(ends.AsSpan(i*2)),last=BinaryPrimitives.ReadUInt16LittleEndian(ends.AsSpan((i+1)*2));
                ushort[] members=new ushort[last-first];for(int j=0;j<members.Length;j++)members[j]=BinaryPrimitives.ReadUInt16LittleEndian(labels.AsSpan((first+j)*2));
                groups[i]=new(members,F($"E0[{i}]"));
            }
        }
        LegacyAcousticPositiveGroupSmoothing.Validate(groups,alphabet.LabelCount,token);
        var masks=new LegacyOrdinarySymbolMaskProducer(alphabet,header).Build(LegacySymbolMaskControllerOutcome.UnboundOrBypassed,[],groups,[],[],[],token);
        //00C860 mode13/variant2;1D60D0 updates the actual flags object. The
        //new raw source owner admits no property/reused/external state.
        byte[] metadata=source.Fields.Single(f=>f.Owner=="247"&&f.Destination=="16C").CopyBytes();
        uint flags=0x21703u&~BinaryPrimitives.ReadUInt32LittleEndian(metadata.AsSpan(13*4));
        bool prune=source.Fields.Single(f=>f.Owner=="247"&&f.Destination=="1EC").CopyBytes()[0]>13;
        token.ThrowIfCancellationRequested();return new(owner,flags,prune,masks,groups,new(profile,alphabet,header),BinaryPrimitives.ReadUInt16LittleEndian(header),budget);
    }

    public LegacyAcousticSymbolMasks CopyMasks()=>new(primary.ToArray(),secondary.ToArray());
    /// <summary>Read only already-computed small results; never enters recognition.</summary>
    public LegacyFullEnglishFirstCachedGroup ScoreGroup(int group)
    {lock(gate)return completed.TryGetValue(group,out var result)?result:throw new InvalidOperationException("Only completed parent-owned groups are readable.");}

    internal LegacyFullEnglishFirstCachedGroup Compute(int group,CancellationToken token,Action<string>? checkpoint=null,Action<int[]>? neuralObserver=null,LegacyFullEnglishFirstController? confidence=null)
    {
        token.ThrowIfCancellationRequested();lock(gate){if(completed.TryGetValue(group,out var found))return found;}
        {
            var features=LegacyFullEnglishFirstFeatures.Prepare(Owner,group,token);
            checkpoint?.Invoke("score:features");token.ThrowIfCancellationRequested();
            var inputs=features.Inputs;var families=new LegacyAcousticCostFamily?[7];
            var scalar=confidence?.Begin(this,group,token);
            for(int i=0;i<4;i++){budget.Network(token);var output=Owner.Knowledge.Evaluate(i,inputs[i],token);families[i]=new(output.CopyProbabilities(),1);scalar?.Feed(i,families[i]!.Probability32768.Span,token);checkpoint?.Invoke("score:network"+i);token.ThrowIfCancellationRequested();}
            int[] costs=LegacyAcousticCostEnsemble.Combine(families,1,primary.AsSpan(0,182),token);
            if(disabledPrototype<costs.Length)costs[disabledPrototype]=0x4000000;
            //Diagnostic-only copy, consumed off-lock. Public production Run
            //passes null and retains no neural vectors.
            neuralObserver?.Invoke(costs.ToArray());token.ThrowIfCancellationRequested();
            var rs=features.CopyStoredRanges();int[] counts=Owner.CopyCutCounts();var first=rs[0];var last=rs[^1];
            bool whole=features.CopyMergedStrokes().Length==1&&first.StartPoint==0&&last.EndPointExclusive==counts[last.StrokeIndex];
            bool zero=false;float? probability=null;
            if(whole)
            {
                var start=Owner.ResolveCutPoint(first.StrokeIndex,first.StartPoint);var end=Owner.ResolveCutPoint(last.StrokeIndex,last.EndPointExclusive-1);
                zero=start.Key.Third==0&&Owner.Preparation.OriginalSource.ResolveEndingMetadata(end,token).Key.Third==0;
                probability=Owner.CopyPhysicalProbabilities()[first.StrokeIndex];
            }
            var context=new LegacyOrdinaryAcousticCacheContext(PruneCandidates,false,(KnowledgeFlags&16)!=0,whole,zero,probability,false,Array.Empty<ushort>());
            var final=cache.BuildWithGroups(costs,features.CopyMergedStrokes(),primary,context,[],groups,token);
            var result=new LegacyFullEnglishFirstCachedGroup(this,group,final,whole,zero,probability);
            checkpoint?.Invoke("score:publish");token.ThrowIfCancellationRequested();
            lock(gate){token.ThrowIfCancellationRequested();completed.Add(group,result);}
            if(scalar is not null)confidence!.Publish(result,scalar,token);return result;
        }
    }
}

/// <summary>Detached integer pre-confidence cache for one actual owned group.</summary>
public sealed class LegacyFullEnglishFirstCachedGroup
{
    public LegacyFullEnglishFirstAcousticCache Owner {get;}
    public int GroupId{get;}
    public bool WholePhysicalScope {get;}
    public bool EndpointKeysZero {get;}
    public float? SourceProbability {get;}
    public int FallbackCost {get;}
    private readonly int[] costs;
    private readonly LegacyAcousticCacheCandidate[] candidates;
    internal LegacyFullEnglishFirstCachedGroup(LegacyFullEnglishFirstAcousticCache owner,int group,
        LegacyOrdinaryAcousticClassCacheResult final,bool whole,bool zero,float? probability)
    {Owner=owner;GroupId=group;costs=final.Costs.ToArray();candidates=final.Candidates.ToArray();FallbackCost=final.FallbackCost;
        WholePhysicalScope=whole;EndpointKeysZero=zero;SourceProbability=probability;}
    public int[] CopyFinalCosts()=>costs.ToArray();
    public LegacyAcousticCacheCandidate[] CopyCandidates()=>candidates.ToArray();
}
