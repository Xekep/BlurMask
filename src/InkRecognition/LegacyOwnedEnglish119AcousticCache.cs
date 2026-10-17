using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Fresh no-property/no-linguistic FIRST119 ensemble and integer220
/// cache over actual candidate ownership. No caller masks, scope, groups or
/// costs are accepted. Confidence and word controller remain separate.</summary>
public sealed class LegacyOwnedEnglish119AcousticCache
{
    public LegacyOwnedEnglish119Candidates Owner {get;}
    public LegacyFirstAcoustic119WireModel Model=>Owner.Model;
    public uint KnowledgeFlags {get;}
    public bool PruneCandidates {get;}
    public int PositiveGroupCount=>groups.Length;
    public int ScoredGroupCount {get{lock(gate)return completed.Count;}}
    private readonly object gate=new();
    private readonly Dictionary<int,LegacyEnglish119CachedGroup> completed=[];
    private readonly byte[] primary,secondary;
    private readonly LegacySymbolMaskKnowledgeGroup[] groups;
    private readonly LegacyOrdinaryAcousticClassCache cache;
    private readonly ushort disabledPrototype;
    private LegacyOwnedEnglish119AcousticCache(LegacyOwnedEnglish119Candidates owner,uint flags,bool prune,
        LegacyAcousticSymbolMasks masks,LegacySymbolMaskKnowledgeGroup[] groups,LegacyOrdinaryAcousticClassCache cache,ushort disabled)
    {Owner=owner;KnowledgeFlags=flags;PruneCandidates=prune;primary=masks.Primary.ToArray();secondary=masks.Secondary.ToArray();this.groups=groups;this.cache=cache;disabledPrototype=disabled;}

    public static LegacyOwnedEnglish119AcousticCache Prepare(LegacyOwnedEnglish119Candidates owner,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);token.ThrowIfCancellationRequested();var model=owner.Model;var source=model.Source;
        byte[] A(string name)=>model.Fields.Single(f=>f.Owner=="21D"&&f.Destination==name).CopyBytes();
        float F(string name)=>BinaryPrimitives.ReadSingleLittleEndian(A(name));
        int I(string name)=>BinaryPrimitives.ReadInt32LittleEndian(A(name));
        ushort H(string name)=>BinaryPrimitives.ReadUInt16LittleEndian(A(name));
        if(model.CostMultiplier!=1||model.Slots[0].Mode!=2||model.Slots[1].Mode!=2||
            model.Slots.Skip(2).Any(s=>s.Mode!=0||s.Weight!=0||s.Networks.Count!=0))
            throw new NotSupportedException("Fresh unit multiplier/two mode2 families required.");
        foreach(string flag in new[]{"78","80","B8","148/150.present"})if(A(flag)[0]!=0)
            throw new NotSupportedException("Optional first119 cache/relationship/size payload remains unsupported.");
        if(F("100")!=0||I("108")!=0||I("128")!=0)
            throw new NotSupportedException("Power, calibration or correction payload remains unsupported.");
        //053F20 does invoke0573B0. The pinned actual AL's script scan makes
        //that builder return before allocating88; a different AL needs its
        //own derived-form owner and must not inherit this absence proof.
        byte[] alphabetBytes=source.Fields.Single(f=>f.Owner=="2B"&&f.Destination=="payload").CopyBytes();
        if(!Convert.ToHexString(SHA256.HashData(alphabetBytes)).Equals("5554E287937D2D8682C46EB7D85A701B15F1B86AFB45F55228DBC8C1EBF7A1FF",StringComparison.Ordinal))
            throw new NotSupportedException("The audited no-Arabic ENlite stored alphabet is required.");
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
        token.ThrowIfCancellationRequested();return new(owner,flags,prune,masks,groups,new(profile,alphabet,header),BinaryPrimitives.ReadUInt16LittleEndian(header));
    }

    public LegacyAcousticSymbolMasks CopyMasks()=>new(primary.ToArray(),secondary.ToArray());
    public LegacyEnglish119CachedGroup ScoreGroup(int group,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();if(completed.TryGetValue(group,out var found))return found;
            var raw=Owner.ScoreGroup(group,token);
            int width=raw.Trajectory.CopyProbabilities().Length;
            int[] costs=LegacyAcousticCostEnsemble.Combine(new LegacyAcousticCostFamily?[]{
                new(raw.Trajectory.CopyProbabilities(),1),new(raw.Raster.CopyProbabilities(),1),null,null,null,null,null},1,primary.AsSpan(0,width),token);
            if(disabledPrototype<costs.Length)costs[disabledPrototype]=0x4000000;
            var rs=raw.Features.CopyStoredRanges();int[] counts=Owner.CopyCutCounts();var first=rs[0];var last=rs[^1];
            bool whole=raw.Features.CopyMergedStrokes().Length==1&&first.StartPoint==0&&last.EndPointExclusive==counts[last.StrokeIndex];
            bool zero=false;float? probability=null;
            if(whole)
            {
                var start=Owner.ResolveCutPoint(first.StrokeIndex,first.StartPoint);var end=Owner.ResolveCutPoint(last.StrokeIndex,last.EndPointExclusive-1);
                zero=start.Key.Third==0&&Owner.Preparation.OriginalSource.ResolveEndingMetadata(end,token).Key.Third==0;
                probability=Owner.CopyPhysicalProbabilities()[first.StrokeIndex];
            }
            var context=new LegacyOrdinaryAcousticCacheContext(PruneCandidates,false,(KnowledgeFlags&16)!=0,whole,zero,probability,false,Array.Empty<ushort>());
            var final=cache.BuildWithGroups(costs,raw.Features.CopyMergedStrokes(),primary,context,[],groups,token);
            var result=new LegacyEnglish119CachedGroup(this,raw,costs,final,whole,zero,probability);
            token.ThrowIfCancellationRequested();completed.Add(group,result);return result;
        }
    }
}

/// <summary>Detached integer pre-confidence cache for one actual owned group.</summary>
public sealed class LegacyEnglish119CachedGroup
{
    public LegacyOwnedEnglish119AcousticCache Owner {get;}
    public LegacyEnglish119CandidateScore Raw {get;}
    public bool WholePhysicalScope {get;}
    public bool EndpointKeysZero {get;}
    public float? SourceProbability {get;}
    public int FallbackCost {get;}
    private readonly int[] neural,costs;
    private readonly LegacyAcousticCacheCandidate[] candidates;
    internal LegacyEnglish119CachedGroup(LegacyOwnedEnglish119AcousticCache owner,LegacyEnglish119CandidateScore raw,int[] neural,
        LegacyOrdinaryAcousticClassCacheResult final,bool whole,bool zero,float? probability)
    {Owner=owner;Raw=raw;this.neural=neural.ToArray();costs=final.Costs.ToArray();candidates=final.Candidates.ToArray();FallbackCost=final.FallbackCost;
        WholePhysicalScope=whole;EndpointKeysZero=zero;SourceProbability=probability;}
    public int[] CopyNeuralCosts()=>neural.ToArray();
    public int[] CopyFinalCosts()=>costs.ToArray();
    public LegacyAcousticCacheCandidate[] CopyCandidates()=>candidates.ToArray();
}
