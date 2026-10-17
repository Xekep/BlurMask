using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>One refresh scoring result. The normalized symbol remains the
/// explicit216860 output boundary; this type does not guess or resolve a label.</summary>
public sealed class LegacyOwnedMainGlyphRefreshScores
{
    private readonly byte[] request;
    public LegacyOwnedRussianMainGlyphScoring Owner {get;}
    public LegacyOwnedMainGlyphSourceMetadataResult Source {get;}
    public ushort NormalizedSymbol {get;}
    public uint FirstCandidateScoreBits {get;}
    public uint SymbolScoreBits {get;}
    public int Cost {get;}
    internal LegacyOwnedMainGlyphRefreshScores(LegacyOwnedRussianMainGlyphScoring owner,
        LegacyOwnedMainGlyphSourceMetadataResult source,ushort symbol,byte[] request,
        float first,float score,int cost)
    {Owner=owner;Source=source;NormalizedSymbol=symbol;this.request=request;
        FirstCandidateScoreBits=BitConverter.SingleToUInt32Bits(first);
        SymbolScoreBits=BitConverter.SingleToUInt32Bits(score);Cost=cost;}
    public byte[] CopyRequest16()=>request.ToArray();
}

/// <summary>Common actual main1C1520/1C1300/1C13A0 scoring, sharing the same
/// acoustic and geometry cache construction as ordinary216BC0. Immutable source
/// snapshots make native preparation's3C2 last-group invalidation redundant;
/// no sampler outputs or pool descriptors are mutated by this portable owner.</summary>
public sealed class LegacyOwnedRussianMainGlyphScoring
{
    private readonly LegacyCursivePreparedInk ink;
    private readonly LegacyInkRange[][] ranges;
    private readonly LegacyCursiveSeparationProfile separation;
    private readonly float[] separationTable;
    private readonly LegacyCursiveCategoryProfile categories;
    private readonly float height,powerScale;
    private readonly ushort blank,closed;
    private readonly ushort[] bypass;
    private readonly Dictionary<int,LegacyNativeGlyphScoreCache> caches=[];
    private readonly object gate=new();
    private readonly LegacyOwnedMainGlyphSourceMetadata? metadata;
    public LegacyOwnedRussianMainGlyphConfidence ConfidenceOwner {get;}
    public LegacyOwnedFreshMainWordControllerPolicy Controller {get;}

    internal LegacyOwnedRussianMainGlyphScoring(ReadOnlySpan<byte> resource,
        LegacyOwnedFreshMainWordControllerPolicy controller,LegacyOwnedRussianMainGlyphConfidence confidence,
        LegacyCursiveSeparationTable table,CancellationToken token,
        LegacyOwnedMainGlyphSourceMetadata? metadata=null)
    {
        ArgumentNullException.ThrowIfNull(controller);ArgumentNullException.ThrowIfNull(confidence);
        ArgumentNullException.ThrowIfNull(table);token.ThrowIfCancellationRequested();
        var owner=controller.Owner;
        if(!ReferenceEquals(confidence.Graph,owner.SourceGraph)||!ReferenceEquals(confidence.Model,owner.CommonModel)||
            (owner.IsCaseAware?!ReferenceEquals(confidence.CaseDictionaryOwner,owner.CaseDictionaryOwner):
                !ReferenceEquals(confidence.DirectDictionaryOwner,owner.DictionaryOwner))||confidence.OwnerScoreChannel88!=0)
            throw new ArgumentException("One actual main source, confidence, graph and model required.");
        if(resource.Length!=572763||!StringComparer.OrdinalIgnoreCase.Equals(
            Convert.ToHexString(SHA256.HashData(resource)),owner.SelectedSourceOwner.SourceFingerprint))
            throw new InvalidDataException("Same pinned main acoustic source required.");
        ConfidenceOwner=confidence;Controller=controller;this.metadata=metadata;
        var categoryOwner=LegacySourceSamplerCategoryOwner.ReadV10dSource(resource,0,token);
        if(categoryOwner.SamplerNodeOffset!=owner.SelectedSourceOwner.SamplerNodeOffset||
            categoryOwner.CategoryProfileOffset!=owner.SelectedSourceOwner.CategoryProfileOffset)
            throw new ArgumentException("The actual selected main sampler category owner is required.");
        categories=categoryOwner.Categories;
        separation=LegacyCursiveSeparationProfile.ReadV106Node(resource,categoryOwner.SamplerNodeOffset);
        separationTable=table.Values.ToArray();var bytes=new byte[separationTable.Length*4];
        for(int i=0;i<separationTable.Length;i++)BinaryPrimitives.WriteSingleLittleEndian(bytes.AsSpan(i*4),separationTable[i]);
        if(!SHA256.HashData(bytes).AsSpan().SequenceEqual(Convert.FromHexString(
            "b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf")))
            throw new InvalidDataException("Actual engine separation table required.");
        ink=owner.SourceInk.WithTags(owner.SourceInk.Points.Span.ToArray().Select(p=>p.Tag).ToArray());
        ranges=owner.SourceGraph.Groups.Select(group=>group.Ranges.ToArray()).ToArray();height=owner.SourceHeight;
        _=LegacyOrdinaryAcousticCacheProfile.ReadV110Node(resource[189734..],token);
        powerScale=LegacyPreparedAcousticDictionaryPermissions.ReadDisabledPowerScale(resource[189734..],token);
        if(powerScale!=1)throw new NotSupportedException("Actual disabled main group power branch required.");
        var alphabet=owner.CommonModel.AcousticAlphabet;
        ushort Label(string text)=>alphabet.TryGetLabel(text,out ushort symbol)?symbol:ushort.MaxValue;
        blank=U16(resource,99);closed=Label("\U000f0001");
        bypass=[closed,Label("\U000f0004"),Label("\U000f0008"),Label("\U000f0002"),Label("\U000f0003"),
            U16(resource,107),U16(resource,109),U16(resource,111)];
        token.ThrowIfCancellationRequested();
    }

    /// <summary>Binds the scoped217670 scoring preparation to the exact actual
    /// source exporter, materialization, geometry and confidence owner. The
    /// current pinned sampler preparation slot is null; acoustic preparation
    /// resets only a last-group sentinel, not the immutable group score values.</summary>
    public static LegacyOwnedRussianMainGlyphScoring Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedMainGlyphSourceMetadata metadata,LegacyCursiveSeparationTable table,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(metadata);token.ThrowIfCancellationRequested();
        return new(resource,metadata.Ownership.Controller,metadata.Ownership.Materialization.ConfidenceOwner,table,token,metadata);
    }

    /// <summary>Scores one prepared normalized216860 symbol for actual ordinary
    /// source metadata. useEndCategory is compact20.bit40. The target higher
    /// source key is preserved; scoring uses its real original acoustic group.
    /// No fast unit-score shortcut from216BC0 is applied to lazy217670.</summary>
    public LegacyOwnedMainGlyphRefreshScores ReadPreparedSymbol(LegacyOwnedMainGlyphSourceMetadataResult source,
        ushort normalizedSymbol,bool useEndCategory,CancellationToken token=default)
    {
        ValidateSource(source,token);
        var cache=GetCache(source.AcousticGroupId,token);
        byte[] request=CreateRequest(source.BaseNode,source.BaseArc,normalizedSymbol,useEndCategory);
        float first=cache.ReadFirstCandidateScore(token);
        float symbol=cache.ReadSymbolScore(normalizedSymbol,ConfidenceOwner.OwnerScoreChannel88,token);
        int cost=cache.ReadRequestCost(request,token);
        token.ThrowIfCancellationRequested();return new(this,source,normalizedSymbol,request,first,symbol,cost);
    }

    /// <summary>Native owner1C is written before symbol resolution. This
    /// separate read lets the caller preserve that write if resolution later
    /// fails; it never substitutes normalized candidate24.field0.</summary>
    public uint ReadFirstCandidateScoreBits(LegacyOwnedMainGlyphSourceMetadataResult source,
        CancellationToken token=default)
    {
        ValidateSource(source,token);
        return BitConverter.SingleToUInt32Bits(GetCache(source.AcousticGroupId,token).ReadFirstCandidateScore(token));
    }

    /// <summary>Native glyph10 is written before constructing the geometry
    /// request/calling1C0. This preserves that scalar write if a subsequent
    /// category lookup or cost operation is unsupported.</summary>
    public uint ReadSymbolScoreBits(LegacyOwnedMainGlyphSourceMetadataResult source,ushort normalizedSymbol,
        CancellationToken token=default)
    {
        ValidateSource(source,token);
        return BitConverter.SingleToUInt32Bits(GetCache(source.AcousticGroupId,token)
            .ReadSymbolScore(normalizedSymbol,ConfidenceOwner.OwnerScoreChannel88,token));
    }

    private void ValidateSource(LegacyOwnedMainGlyphSourceMetadataResult source,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(source);token.ThrowIfCancellationRequested();
        if(metadata is null||!ReferenceEquals(source.Owner,metadata)||
            !ReferenceEquals(metadata.SourceOverlay.ConfidenceOwner,ConfidenceOwner))
            throw new ArgumentException("Actual source metadata from this exact refresh owner required.");
    }

    internal byte[] CreateRequest(int node,ushort arc,ushort symbol,bool useEndCategory)
    {
        byte geometry=255;
        if(symbol!=closed)
        {
            if(symbol>=categories.StartCompositeMap.Length)throw new NotSupportedException("Requested label escapes actual category maps.");
            int composite=categories.EndCompositeMap.Span[symbol];
            if(!useEndCategory||composite==-1)composite=categories.StartCompositeMap.Span[symbol];
            if((uint)composite>=(uint)categories.CompositeCategories.Length)
                throw new NotSupportedException("Native category access lacks a supported composite owner.");
            geometry=checked((byte)categories.CompositeCategories.Span[composite].GeometryIndex);
        }
        byte[] request=new byte[16];BinaryPrimitives.WriteInt32LittleEndian(request,node);
        W16(request,4,arc);W16(request,6,ushort.MaxValue);W16(request,8,symbol);request[10]=geometry;return request;
    }

    internal LegacyNativeGlyphScoreCache GetCache(int group,CancellationToken token)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(caches.TryGetValue(group,out var cached))return cached;
            var actual=ConfidenceOwner.ScoreGroup(group,token);var scores=actual.ActualAcousticScores;
            var candidates=actual.Candidates.Span;
            int geometry=LegacyCursiveGroupSeparationCost.Evaluate(ink,ranges[group],height,
                separation.GapOffset,separation.Slope,separationTable,separation.MaximumCost,cancellationToken:token);
            byte[] rows=new byte[candidates.Length*24];var vectors=new ReadOnlyMemory<float>[candidates.Length];
            for(int i=0;i<candidates.Length;i++)
            {token.ThrowIfCancellationRequested();W16(rows,i*24+8,candidates[i].Symbol);
                BinaryPrimitives.WriteInt32LittleEndian(rows.AsSpan(i*24+4),candidates[i].Cost);
                vectors[i]=new float[]{candidates[i].Confidence};}
            var result=new LegacyNativeGlyphScoreCache(rows,vectors,
                Enumerable.Repeat(geometry,categories.Prototypes.Length).ToArray(),blank,
                scores.Cache.FallbackCost,powerScale,bypass,token);
            token.ThrowIfCancellationRequested();caches.Add(group,result);return result;
        }
    }
    private static ushort U16(ReadOnlySpan<byte> bytes,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(bytes[at..]);
    private static void W16(Span<byte> bytes,int at,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(bytes[at..],value);
}
