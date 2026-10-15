using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>The explicit216BC0 in/out scratch and scalar residue. Copies protect
/// the256 labels; a failed/unsupported operation does not roll back earlier writes.</summary>
public sealed class LegacyOrdinaryGlyphProductionScratch
{
    private readonly ushort[] labels;
    public int RemainingCursor { get; internal set; }
    public byte ComponentFlag { get; internal set; }
    public byte ExternalVariant { get; internal set; }
    public uint Score0CBits { get; internal set; }
    public uint Score10Bits { get; internal set; }
    public int Cost14 { get; internal set; }
    public ushort PublicationFlags06 { get; internal set; }

    public LegacyOrdinaryGlyphProductionScratch(ReadOnlySpan<ushort> actualLabels256, int remainingCursor,
        byte componentFlag=0, byte externalVariant=0, uint score0CBits=0, uint score10Bits=0,
        int cost14=0, ushort publicationFlags06=0)
    {
        if(actualLabels256.Length!=256)throw new ArgumentException("Complete256-label scratch required.");
        labels=actualLabels256.ToArray();RemainingCursor=remainingCursor;ComponentFlag=componentFlag;
        ExternalVariant=externalVariant;Score0CBits=score0CBits;Score10Bits=score10Bits;
        Cost14=cost14;PublicationFlags06=publicationFlags06;
    }
    public ushort[] CopyLabels()=>labels.ToArray();
    internal void Write(ushort symbol)
    {
        if((uint)RemainingCursor>=256)throw new NotSupportedException("Native scratch write would escape its256-label owner.");
        labels[RemainingCursor]=symbol;
    }
    internal LegacyPreparedGlyphFinalizationInput Snapshot()=>new(true,RemainingCursor,labels,
        ComponentFlag,ExternalVariant,Score0CBits,Score10Bits,Cost14,PublicationFlags06);
}

/// <summary>Bounded216BC0 ordinary subtype0 producer with actual fresh Russian
/// main confidence and geometry. The72-byte backtrack row is explicitly prepared,
/// with its node/arc checked against the same graph. This does not produce that
/// row, choose a winning path, handle protocol delimiters or implement218560.</summary>
public sealed class LegacyOwnedRussianOrdinaryGlyphProducer
{
    private readonly LegacyOwnedRussianMainGlyphConfidence confidence;
    private readonly LegacyOwnedRussianGlyphSymbolResolver? resolver;
    private readonly LegacyOwnedRussianMainSourceSymbolCallbacks? sourceCallbacks;
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private readonly LegacyRussianModelBundle model;
    private readonly LegacyFreshCaseMainSourcePolicy? casePolicy;
    private readonly LegacyCursivePreparedInk ink;
    private readonly LegacyInkRange[][] ranges;
    private readonly LegacyCursiveSeparationProfile separation;
    private readonly float[] separationTable;
    private readonly LegacyCursiveCategoryProfile categories;
    private readonly float height, powerScale;
    private readonly ushort blank, unitScore, closed, apostrophe, quote;
    private readonly ushort[] bypass;
    private readonly Dictionary<int,LegacyNativeGlyphScoreCache> caches=[];
    public LegacyFreshCaseMainSourcePolicy Policy => casePolicy ??
        throw new InvalidOperationException("The DIRECT producer has DirectPolicy, not an invented case policy.");
    public LegacyOwnedFreshMainWordControllerPolicy? DirectPolicy {get;}


    private LegacyOwnedRussianOrdinaryGlyphProducer(ReadOnlySpan<byte> resource,LegacyFreshCaseMainSourcePolicy? policy,
        LegacyOwnedFreshMainWordControllerPolicy? directPolicy,LegacyOwnedRussianMainGlyphConfidence confidence,
        LegacyCursiveSeparationTable table,LegacyOwnedRussianMainSourceSymbolCallbacks? sourceCallbacks,
        LegacyNativeTextPublicationData? publication,CancellationToken token)
    {
        casePolicy=policy;DirectPolicy=directPolicy;this.confidence=confidence;this.sourceCallbacks=sourceCallbacks;
        var controller=policy?.Controller??directPolicy??throw new ArgumentException("An actual owning source policy is required.");
        var owner=controller.Owner;graph=owner.SourceGraph;model=owner.CommonModel;
        bool direct=policy is null;
        if((direct? owner.IsCaseAware||!ReferenceEquals(confidence.DirectDictionaryOwner,owner.DictionaryOwner):
            !ReferenceEquals(confidence.CaseDictionaryOwner,policy!.DictionaryOwner))||
            !ReferenceEquals(confidence.Graph,graph)||!ReferenceEquals(confidence.Model,model)||
            confidence.OwnerScoreChannel88!=0)
            throw new ArgumentException("One actual fresh source, graph, model and confidence owner required.");
        if(resource.Length!=572763||!StringComparer.OrdinalIgnoreCase.Equals(
            Convert.ToHexString(SHA256.HashData(resource)),owner.SelectedSourceOwner.SourceFingerprint))
            throw new InvalidDataException("Same pinned acoustic source required.");
        if(direct)
        {
            if(sourceCallbacks is null||!ReferenceEquals(sourceCallbacks.Owner,confidence)||publication is null||
                controller.Configuration.Trace2368!=0||controller.Configuration.Mode22f6!=0||controller.Refresh.KnowledgeOption22f8!=0)
                throw new ArgumentException("Fresh ordinary DIRECT policy, exact source callbacks and publication data required.");
            if(LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet,token).Length!=0)
                throw new NotSupportedException("The DIRECT scalar branch requires its actual empty wide-pair table.");
            for(ushort i=0;i<model.AcousticAlphabet.LabelCount;i++)
            {
                token.ThrowIfCancellationRequested();
                if(model.AcousticAlphabet.TryGetText(i,out var text))foreach(var rune in text.EnumerateRunes())
                    if(publication.ReadScript(rune.Value)==5)
                        throw new NotSupportedException("Arabic map production is outside this ordinary scalar branch.");
            }
        }
        else resolver=LegacyOwnedRussianGlyphSymbolResolver.Prepare(policy!.DictionaryOwner,
            policy.DictionaryOwner.Symbols.Preparation,token);
        var categoryOwner=LegacySourceSamplerCategoryOwner.ReadV10dSource(resource,0,token);
        if(categoryOwner.SamplerNodeOffset!=owner.SelectedSourceOwner.SamplerNodeOffset||
            categoryOwner.CategoryProfileOffset!=owner.SelectedSourceOwner.CategoryProfileOffset)
            throw new ArgumentException("The actual selected main sampler category owner is required.");
        //A private fresh parse keeps public ReadOnlyMemory snapshots from
        //allowing a caller to replace the maps used by this producer.
        categories=categoryOwner.Categories;
        separation=LegacyCursiveSeparationProfile.ReadV106Node(resource,categoryOwner.SamplerNodeOffset);
        separationTable=table.Values.ToArray();var bytes=new byte[separationTable.Length*4];
        for(int i=0;i<separationTable.Length;i++)BinaryPrimitives.WriteSingleLittleEndian(bytes.AsSpan(i*4),separationTable[i]);
        if(!SHA256.HashData(bytes).AsSpan().SequenceEqual(Convert.FromHexString(
            "b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf")))
            throw new InvalidDataException("Actual engine separation table required.");
        ink=owner.SourceInk.WithTags(owner.SourceInk.Points.Span.ToArray().Select(p=>p.Tag).ToArray());
        ranges=graph.Groups.Select(group=>group.Ranges.ToArray()).ToArray();height=owner.SourceHeight;
        _=LegacyOrdinaryAcousticCacheProfile.ReadV110Node(resource[189734..],token);
        powerScale=LegacyPreparedAcousticDictionaryPermissions.ReadDisabledPowerScale(resource[189734..],token);
        if(powerScale!=1)throw new NotSupportedException("Actual disabled main group power branch required.");
        //Engine053F20 writes the seven stored headers to8,A,C,E,10,12,14;
        //ALcount is separately1A. In particular headerE is178, not192.
        blank=U16(resource,99);unitScore=U16(resource,105);
        var alphabet=model.AcousticAlphabet;
        ushort Label(string text)=>alphabet.TryGetLabel(text,out ushort symbol)?symbol:ushort.MaxValue;
        closed=Label("\U000f0001");apostrophe=Label("'");quote=Label("\"");
        bypass=[closed,Label("\U000f0004"),Label("\U000f0008"),Label("\U000f0002"),Label("\U000f0003"),
            U16(resource,107),U16(resource,109),U16(resource,111)];
        token.ThrowIfCancellationRequested();
    }

    public static LegacyOwnedRussianOrdinaryGlyphProducer Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshCaseMainSourcePolicy policy,LegacyOwnedRussianMainGlyphConfidence confidence,
        LegacyCursiveSeparationTable actualTable,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(policy);ArgumentNullException.ThrowIfNull(confidence);
        ArgumentNullException.ThrowIfNull(actualTable);token.ThrowIfCancellationRequested();
        return new(resource,policy,null,confidence,actualTable,null,null,token);
    }

    /// <summary>The same ordinary scalar producer bound to a genuine DIRECT
    /// source. Publication data proves absent Arabic mapping; actual empty wide
    /// pairs and same-confidence callbacks close the replacement branch. No
    /// case wrapper or prepared boolean substitutes for these owners.</summary>
    public static LegacyOwnedRussianOrdinaryGlyphProducer Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedFreshMainWordControllerPolicy directPolicy,LegacyOwnedRussianMainGlyphConfidence confidence,
        LegacyCursiveSeparationTable actualTable,LegacyOwnedRussianMainSourceSymbolCallbacks sourceCallbacks,
        LegacyNativeTextPublicationData publication,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(directPolicy);ArgumentNullException.ThrowIfNull(confidence);
        ArgumentNullException.ThrowIfNull(actualTable);ArgumentNullException.ThrowIfNull(sourceCallbacks);
        ArgumentNullException.ThrowIfNull(publication);token.ThrowIfCancellationRequested();
        return new(resource,null,directPolicy,confidence,actualTable,sourceCallbacks,publication,token);
    }

    /// <summary>Normal216BC0 success returns the existing2173E0 input snapshot.
    /// Structural scope guards precede native writes. Once entered, flags clear,
    /// cursor decrements, symbol is appended, then scores/flags are written.
    /// Unsupported services/cancellation after entry retain that prefix. The
    /// supported no-wide/no-Arabic resolver has no native false-return branch.</summary>
    public LegacyPreparedGlyphFinalizationInput Produce(ReadOnlySpan<byte> preparedRow72,
        LegacyOrdinaryGlyphProductionScratch scratch,ulong conversionArgument=0,
        ReadOnlySpan<byte> preparedCompact40=default,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(scratch);token.ThrowIfCancellationRequested();
        if(preparedRow72.Length!=72)throw new ArgumentException("Complete prepared backtrack row72 required.");
        if(preparedRow72[0x3c]!=0)throw new NotSupportedException("Only ordinary subtype0 is restored.");
        int node=BinaryPrimitives.ReadInt32LittleEndian(preparedRow72);ushort arc=U16(preparedRow72,4);
        var arcs=graph.GetNode(node);
        if(arc>=arcs.Count)throw new ArgumentException("Prepared row arc escapes the actual owned graph node.");
        int compact=BinaryPrimitives.ReadInt32LittleEndian(preparedRow72[0x20..]);
        //21709C..2170BC resolves this neighbor before216860, even though
        //the bounded no-wide resolver never consumes its value. Keep the
        //native read in bounds instead of silently accepting an invalid row.
        if(compact!=-1&&(preparedCompact40.Length%40!=0||(uint)compact>=(uint)(preparedCompact40.Length/40)))
            throw new ArgumentException("Prepared row requires its actual in-bounds compact40 neighbor.");
        if(compact!=-1)_=U16(preparedCompact40,compact*40+0x1c);
        ushort symbol=U16(preparedRow72,0x3a);
        if(unchecked((ushort)(symbol+6))<3)
            throw new NotSupportedException("FFFA/FFFB/FFFC require the native delimiter/multi-output callback.");
        if(symbol>=model.AcousticAlphabet.LabelCount+3&&unchecked((ushort)(symbol+9))>2)
            throw new NotSupportedException("This portable ordinary scope excludes other unchecked synthetic labels.");
        scratch.ComponentFlag=0;scratch.ExternalVariant=0;
        scratch.RemainingCursor=unchecked(scratch.RemainingCursor-1);
        token.ThrowIfCancellationRequested();
        ushort emitted;
        if(resolver is not null)emitted=resolver.Resolve(symbol,conversionArgument,false,token).EmittedSymbol;
        else if(unchecked((byte)conversionArgument)!=0&&symbol==model.AcousticAlphabet.LabelCount+1)emitted=apostrophe;
        else if(unchecked((byte)conversionArgument)!=0&&symbol==model.AcousticAlphabet.LabelCount+2)emitted=quote;
        else if(model.AcousticAlphabet.TryGetLabel("\ufffd",out ushort replacement)&&symbol==replacement)
        {
            var result=sourceCallbacks!.ResolveReplacement(node,arc,token);
            emitted=result.EmittedSymbol;scratch.ExternalVariant=result.ExternalVariant;
        }
        else emitted=symbol;
        scratch.Write(emitted);
        token.ThrowIfCancellationRequested();
        if(symbol==unitScore||unchecked((ushort)(symbol+9))<=5)
        {scratch.Score0CBits=0x3f800000;scratch.Score10Bits=0x3f800000;scratch.Cost14=0;}
        else
        {
            scratch.Score0CBits=BitConverter.SingleToUInt32Bits(preparedRow72[0x3e]/255f);
            ushort scoreSymbol=symbol==model.AcousticAlphabet.LabelCount+1?apostrophe:
                symbol==model.AcousticAlphabet.LabelCount+2?quote:symbol;
            var cache=ScoreCache(arcs[arc].GroupId,token);
            scratch.Score10Bits=BitConverter.SingleToUInt32Bits(cache.ReadSymbolScore(scoreSymbol,0,token));
            var request=CreateRequest(preparedRow72,node,arc,scoreSymbol);
            scratch.Cost14=cache.ReadRequestCost(request,token);
        }
        token.ThrowIfCancellationRequested();scratch.PublicationFlags06=0;
        return scratch.Snapshot();
    }

    private byte[] CreateRequest(ReadOnlySpan<byte> row,int node,ushort arc,ushort symbol)
    {
        byte geometry=255;
        if(symbol!=closed)
        {
            if(symbol>=categories.StartCompositeMap.Length)throw new NotSupportedException("Requested label escapes actual category maps.");
            int composite=categories.EndCompositeMap.Span[symbol];
            if(row[0x3d]==0||composite==-1)composite=categories.StartCompositeMap.Span[symbol];
            if((uint)composite>=(uint)categories.CompositeCategories.Length)
                throw new NotSupportedException("Native category access lacks a supported composite owner.");
            geometry=checked((byte)categories.CompositeCategories.Span[composite].GeometryIndex);
        }
        byte[] request=new byte[16];BinaryPrimitives.WriteInt32LittleEndian(request,node);
        W16(request,4,arc);W16(request,6,ushort.MaxValue);W16(request,8,symbol);request[10]=geometry;
        return request;
    }

    private LegacyNativeGlyphScoreCache ScoreCache(int group,CancellationToken token)
    {
        if(caches.TryGetValue(group,out var cached))return cached;
        var actual=confidence.ScoreGroup(group,token);var scores=actual.ActualAcousticScores;
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
    private static ushort U16(ReadOnlySpan<byte> bytes,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(bytes[at..]);
    private static void W16(Span<byte> bytes,int at,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(bytes[at..],value);
}
