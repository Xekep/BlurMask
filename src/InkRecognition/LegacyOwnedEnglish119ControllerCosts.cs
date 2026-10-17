using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>FIRST119 controller cost requests over the actual cached English
/// candidates. Candidate24.field0 and vector10[0] remain different channels.
/// This owner does not run primary recurrence, select a word or publish text.</summary>
public sealed class LegacyOwnedEnglish119ControllerCosts
{
    public LegacyOwnedEnglish119GlyphConfidence Owner {get;}
    public LegacyFirstAcoustic119WireModel Model=>Owner.Model;
    public int GeometryPrototypeCount {get;}
    public int CompositeCategoryCount=>composites.Length;
    public float CostScale=>1;
    public float GapOffset {get;}
    public float GapSlope {get;}
    public int GeometryMaximumCost {get;}
    private readonly LegacyCursivePreparedInk ink;
    private readonly float[] table;
    private readonly byte[] header;
    private readonly LegacyCursiveCompositeCategory[] composites;
    private readonly int[] firstIds;
    private readonly object gate=new();
    private readonly Dictionary<int,LegacyEnglish119ControllerGroupCosts> completed=[];
    public int ScoredGroupCount {get{lock(gate)return completed.Count;}}

    private LegacyOwnedEnglish119ControllerCosts(LegacyOwnedEnglish119GlyphConfidence owner,
        LegacyCursiveSeparationTable table,CancellationToken token)
    {
        Owner=owner;var candidates=owner.Owner.Owner;var sampler=candidates.Sampler;
        byte[] S(string name)=>sampler.Fields.Single(f=>f.Destination==name).CopyBytes();
        float F(string name)=>BinaryPrimitives.ReadSingleLittleEndian(S(name));
        if(sampler.Child.Version!=0x108||S("50")[0]!=1||S("134.present")[0]!=0||S("1D0.present")[0]!=0||
            BinaryPrimitives.ReadSingleLittleEndian(Model.Fields.Single(f=>f.Owner=="21D"&&f.Destination=="100").CopyBytes())!=0)
            throw new NotSupportedException("Actual first108 ordinary geometry and disabled119 power scaling required.");
        var categories=sampler.ReadCategories(token);
        if(categories.BaseEntryCount!=Model.Source.Alphabet.LabelCount||categories.Prototypes.Length is <1 or >127)
            throw new InvalidDataException("Same owned alphabet/category geometry domain required.");
        GeometryPrototypeCount=categories.Prototypes.Length;composites=categories.CompositeCategories.ToArray();
        firstIds=categories.FirstIds.ToArray().Select(v=>(int)v).ToArray();
        GapOffset=F("68");GapSlope=F("108");GeometryMaximumCost=BinaryPrimitives.ReadInt32LittleEndian(S("118"));
        if(!float.IsFinite(GapOffset)||!float.IsFinite(GapSlope)||GapSlope<0||GeometryMaximumCost is <0 or >0x4000000)
            throw new InvalidDataException("Finite ordinary separation settings required.");
        this.table=table.Values.ToArray();
        if(!SHA256.HashData(MemoryMarshal.AsBytes(this.table.AsSpan())).AsSpan().SequenceEqual(
            Convert.FromHexString("B23ED52A3A10808EAB597A8F371A7BBB66BDE2FAB5FE2666431B1B192ABC1BAF")))
            throw new ArgumentException("The pinned native256-entry log table is required.",nameof(table));
        header=Model.Source.Alphabet.CopySerializedHeader();
        //193860 reads XY in the POST-cut source domain and stored range order.
        //Delta/tag fields are not read by this kernel; no normalization order.
        ink=LegacyCursivePreparedInk.Create(candidates.CopyCutStrokes(),token);
    }

    public static LegacyOwnedEnglish119ControllerCosts Prepare(LegacyOwnedEnglish119GlyphConfidence owner,
        LegacyCursiveSeparationTable nativeTable,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);ArgumentNullException.ThrowIfNull(nativeTable);token.ThrowIfCancellationRequested();
        var result=new LegacyOwnedEnglish119ControllerCosts(owner,nativeTable,token);token.ThrowIfCancellationRequested();return result;
    }

    /// <summary>Map a real composite category to the geometry-vector request
    /// index. Composite.FirstId is the resolved first-ID VALUE; the serialized
    /// first-map index addresses the separately retained first-ID table.</summary>
    public LegacyCursiveCompositeCategory ReadCompositeCategory(int compositeCategory)
    {if((uint)compositeCategory>=(uint)composites.Length)throw new ArgumentOutOfRangeException(nameof(compositeCategory));return composites[compositeCategory];}
    public int[] CopyFirstCategoryIds()=>firstIds.ToArray();
    public LegacyEnglish119ControllerGroupCosts ScoreGroup(int group,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();if(completed.TryGetValue(group,out var found))return found;
            var confidence=Owner.ScoreGroup(group,token);var actual=confidence.ActualCache;
            if(!ReferenceEquals(actual.Owner,Owner.Owner)||!ReferenceEquals(actual.Raw.Owner,Owner.Owner.Owner)||actual.Raw.GroupId!=group)
                throw new ArgumentException("Same actual119 cache, candidate source and group required.");
            var ranges=Owner.Owner.Owner.CopyGroupRanges(group);
            if(!ranges.AsSpan().SequenceEqual(actual.Raw.Features.CopyStoredRanges()))
                throw new InvalidDataException("Stored graph and classified group ranges differ.");
            int geometry=LegacyCursiveGroupSeparationCost.Evaluate(ink,ranges,Owner.Owner.Owner.Preparation.Line.Height,
                GapOffset,GapSlope,table,GeometryMaximumCost,cancellationToken:token);
            var cache=new LegacyOrdinaryAcousticClassCacheResult(actual.CopyFinalCosts(),actual.CopyCandidates(),actual.FallbackCost);
            var ranked=new LegacyOrdinaryRankedAcousticGroup(cache,Model.Source.Alphabet,header,
                Enumerable.Repeat(geometry,GeometryPrototypeCount).ToArray(),CostScale,token);
            var result=new LegacyEnglish119ControllerGroupCosts(this,confidence,ranked,geometry);
            token.ThrowIfCancellationRequested();completed.Add(group,result);return result;
        }
    }
}

/// <summary>One immutable actual group. Requests are explicit downstream
/// inputs, not proof that the native primary scheduler issued them.</summary>
public sealed class LegacyEnglish119ControllerGroupCosts
{
    public LegacyOwnedEnglish119ControllerCosts Owner {get;}
    public LegacyEnglish119ConfidenceGroup Confidence {get;}
    public LegacyEnglish119CachedGroup ActualCache=>Confidence.ActualCache;
    public int GroupId=>ActualCache.Raw.GroupId;
    public int GeometryCost {get;}
    private readonly LegacyOrdinaryRankedAcousticGroup ranked;
    internal LegacyEnglish119ControllerGroupCosts(LegacyOwnedEnglish119ControllerCosts owner,
        LegacyEnglish119ConfidenceGroup confidence,LegacyOrdinaryRankedAcousticGroup ranked,int geometry)
    {Owner=owner;Confidence=confidence;this.ranked=ranked;GeometryCost=geometry;}
    public LegacyRankedAcousticEntry[] CopyEntries()=>ranked.Entries.ToArray();
    /// <summary>Native1BE530/1C1260 candidate24.field0, not calibrated glyph
    /// confidence vector10[0]. Math.Exp is a portable transcendental adaptation.</summary>
    public float ReadNormalizedCandidateWeight(ushort symbol,CancellationToken token=default)=>ranked.GetNormalizedLikelihood(symbol,token);
    public byte ReadLikelihoodByte(ushort symbol,CancellationToken token=default)=>ranked.GetFirstControllerLikelihoodByte(symbol,token);
    /// <summary>Native1C13A0 request symbol/ordinal/signed geometry index and
    /// flags. Synthetic blank/count+1/count+2 belong to this AL, never RU IDs.</summary>
    public int ReadClassCost(ushort symbol,ushort ordinal,int geometryPrototypeIndex,bool restrictTopTwenty,
        bool primitiveCost,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(geometryPrototypeIndex is <sbyte.MinValue or >sbyte.MaxValue)throw new ArgumentOutOfRangeException(nameof(geometryPrototypeIndex));
        return ranked.Query(symbol,ordinal,geometryPrototypeIndex,restrictTopTwenty,primitiveCost,token);
    }
}
