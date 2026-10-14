using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Fresh one-physical main247 preparation, followed by the owned
///297/v106 sampler and its source218 point-buffer replacement. The closed
///physical/category-limit branch is deliberately rejected. No main word winner,
///context probabilities or guessed first-parser frame is supplied.</summary>
public sealed class LegacyFreshSingleGlyphMainSampler
{
    private readonly byte[] resource;
    public LegacyFreshSingleGlyphMainPostprocessing Postprocessing {get;}
    public LegacySourceSamplerCategoryOwner CategoryOwner {get;}
    public LegacyMainWordTransitionProfile TransitionProfile {get;}
    public LegacyInkAlphabet AcousticAlphabet {get;}
    public LegacyCursiveLatticeSpanProfile SpanProfile {get;}
    public int GraphOffset {get;}
    public int AcousticOffset {get;}
    public short AcousticTargetHeight {get;}
    private LegacyFreshSingleGlyphMainSampler(byte[] resource,LegacyFreshSingleGlyphMainPostprocessing post,
        LegacySourceSamplerCategoryOwner categories,LegacyMainWordTransitionProfile transition,
        LegacyCursiveLatticeSpanProfile span,int graph,int acoustic,short target,LegacyInkAlphabet alphabet)
    {this.resource=resource;Postprocessing=post;CategoryOwner=categories;TransitionProfile=transition;
        SpanProfile=span;GraphOffset=graph;AcousticOffset=acoustic;AcousticTargetHeight=target;AcousticAlphabet=alphabet;}

    public static LegacyFreshSingleGlyphMainSampler Prepare(ReadOnlySpan<byte> actualResource,int sourceOffset,
        InkStroke actualRawStroke,float actualResolution,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();var snapshot=actualResource.ToArray();
        var post=LegacyFreshSingleGlyphMainPostprocessing.Prepare(snapshot,sourceOffset,actualRawStroke,actualResolution,cancellationToken);
        var owner=LegacySourceSamplerCategoryOwner.ReadV10dSource(snapshot,sourceOffset,cancellationToken);
        int sampler=owner.SamplerNodeOffset;
        //196500: version106 forces134=0 and138/140=NULL. Those fields are
        //NOT the three earlier prefix bytes.18D590 copies8..1A7 after18D400,
        //so runtime244 (whole physical only) remains constructor zero.
        var prefix=Convert.FromHexString("0000003f3333b33ecdcc4c3e6666e63f0001003333b33ecdcc4c3e0000803e0000003f0000003fcdcccc3d060000804001");
        if(!snapshot.AsSpan(sampler+16,prefix.Length).SequenceEqual(prefix))
            throw new NotSupportedException("Exact checked Russian106 cut scalar profile required.");
        var transition=LegacyMainWordTransitionProfile.ReadV106Node(snapshot,sampler,cancellationToken);
        int graph=checked(sampler+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(snapshot.AsSpan(sampler+4)));
        var span=LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(snapshot,graph);
        int acoustic=checked(graph+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(snapshot.AsSpan(graph+4)));
        if(span.MaximumAtoms>15||acoustic>snapshot.Length-21||BinaryPrimitives.ReadUInt32LittleEndian(snapshot.AsSpan(acoustic))!=0x101fe90||
            BinaryPrimitives.ReadUInt32LittleEndian(snapshot.AsSpan(acoustic+8))!=0x21d||
            BinaryPrimitives.ReadUInt16LittleEndian(snapshot.AsSpan(acoustic+12))!=0||
            BinaryPrimitives.ReadUInt16LittleEndian(snapshot.AsSpan(acoustic+14))!=0x110||
            snapshot[acoustic+16]!=255||snapshot[acoustic+17]!=255)
            throw new NotSupportedException("Owned main ordinary21B/21D with FF/FF normalization required.");
        short target=BinaryPrimitives.ReadInt16LittleEndian(snapshot.AsSpan(acoustic+18));
        int sourceEnd=checked(sourceOffset+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(snapshot.AsSpan(sourceOffset+4)));
        uint acousticLength=BinaryPrimitives.ReadUInt32LittleEndian(snapshot.AsSpan(acoustic+4));
        if(acousticLength<13||acousticLength>sourceEnd-acoustic-8)throw new InvalidDataException("Truncated owned main acoustic node.");
        if(target<=0)throw new InvalidDataException("Positive stored main acoustic target required.");
        int alphabetOffset=post.Input.Controller.Alphabet.AlphabetPayloadOffset;
        if(BinaryPrimitives.ReadUInt16LittleEndian(snapshot.AsSpan(alphabetOffset-2))!=0x103)
            throw new NotSupportedException("Checked owned acoustic alphabet103 required.");
        var alphabet=LegacyInkAlphabet.Load(snapshot.AsSpan(alphabetOffset),0x103);
        if(alphabet.LabelCount!=transition.Categories.BaseEntryCount)throw new InvalidDataException("Main acoustic/category ownership differs.");
        cancellationToken.ThrowIfCancellationRequested();return new(snapshot,post,owner,transition,span,graph,acoustic,target,alphabet);
    }

    public LegacyFreshSingleGlyphMainSampled ApplyFirstResult(LegacyFirstMaterializedPlainWord actualWord,
        LegacyOrdinaryGraphSourceRanges actualFirstRanges,LegacyCursiveSeparationTable table,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(table);cancellationToken.ThrowIfCancellationRequested();
        var prepared=Postprocessing.ApplyFirstResult(actualWord,actualFirstRanges,cancellationToken);
        byte[] guide=prepared.CopyGuide().ToArray();float height=F(guide,4),baseline=F(guide,0);
        var current=prepared.Smoothed.Strokes;
        var p=current[0].Points;float dx=p[0].X-p[^1].X,dy=p[0].Y-p[^1].Y;
        float square=dy*dy+dx*dx;
        if(!float.IsFinite(square)||Math.Sqrt((double)square)<(double)(height*.8f))
            throw new NotSupportedException("Closed physical category-limit producer is not yet owned by this endpoint.");
        var raw=Postprocessing.Input.Controller.Source;
        //The physical record+C value survives the transformed buffers; do not
        //rerun2AD on resampled points or invent a baseline confidence.
        var cuts=LegacyCursiveCutProducer.GenerateRussianVersion106(current,baseline,height,
            raw.SourceStrokeProbabilities.ToArray(),false,null,cancellationToken);
        //193380 ->218.virtual30=168B40 ->16AA00(mapFactor1,orderNULL).
        //Thus cut insertions replace current points, compose the prior200 map,
        //and leave source720 publication ranges in their previous domain.
        var chronology=prepared.Chronology.WithInsertedPreparedInk(cuts.PreparedInk,cancellationToken);
        //167210 physical1B/1A=0,18=-1; bounded fresh postpasses do not write
        //these bytes.19F850 initializes ordinary group6/7=0;19EF90's endpoint
        //nibbles and1A3B30 merged flags are consequently zero in this scope.
        var seedProfile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,GraphOffset);
        var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,height,seedProfile,[0],[-1],cancellationToken);
        var ranges=LegacyOrdinaryGraphSourceRanges.Prepare(seeds,chronology,raw,SpanProfile.MaximumAtoms,cancellationToken:cancellationToken);
        var pruneProfile=LegacyOrdinarySamplerGroupPruningProfile.ReadV106Node(resource,CategoryOwner.SamplerNodeOffset);
        var pruning=LegacyOrdinarySamplerGroupPruning.Prepare(ranges.Graph,cuts.PreparedInk,height,pruneProfile,table,cancellationToken);
        ranges=ranges.WithSamplerPruning(pruning,cancellationToken);
        return new(this,prepared,cuts,chronology,ranges,pruning,baseline,height);
    }
    private static float F(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadSingleLittleEndian(b[at..]);
}

/// <summary>Actual main source cut coordinates, retained source-key identities,
///resource-capped/pruned candidate graph and native FF/FF glyph normalization.
///Still requires the main acoustic/context/cache and final word lifecycle.</summary>
public sealed class LegacyFreshSingleGlyphMainSampled
{
    public LegacyFreshSingleGlyphMainSampler Owner {get;}
    public LegacyFreshSingleGlyphMainPrepared Preparation {get;}
    public LegacyCursiveCutResult Cuts {get;}
    public LegacySourcePointChronology Chronology {get;}
    public LegacyOrdinaryGraphSourceRanges SourceRanges {get;}
    public LegacyOrdinarySamplerGroupPruningResult Pruning {get;}
    public float SourceBaseline {get;}
    public float SourceHeight {get;}
    internal LegacyFreshSingleGlyphMainSampled(LegacyFreshSingleGlyphMainSampler owner,
        LegacyFreshSingleGlyphMainPrepared preparation,LegacyCursiveCutResult cuts,LegacySourcePointChronology chronology,
        LegacyOrdinaryGraphSourceRanges ranges,LegacyOrdinarySamplerGroupPruningResult pruning,float baseline,float height)
    {Owner=owner;Preparation=preparation;Cuts=cuts;Chronology=chronology;SourceRanges=ranges;Pruning=pruning;SourceBaseline=baseline;SourceHeight=height;}

    public LegacyNormalizedPreparedGlyph NormalizeGroup(int groupIndex,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();var groups=SourceRanges.Graph.Groups;
        if((uint)groupIndex>=(uint)groups.Count)throw new ArgumentOutOfRangeException(nameof(groupIndex));
        var ranges=groups[groupIndex].Ranges.Span;
        if(ranges.Length!=1)throw new NotSupportedException("One physical normalized range branch required.");
        var range=ranges[0];float minimum=float.MaxValue;
        for(int p=range.StartPoint;p<range.EndPointExclusive;p++)minimum=Math.Min(minimum,Cuts.PreparedInk.Points.Span[p].X);
        return LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(Cuts.PreparedInk,ranges,minimum,
            SourceBaseline,SourceHeight,Owner.AcousticTargetHeight,1,cancellationToken);
    }

    /// <summary>152280 resolves a valid ordinary arc through group descriptor0
    ///and reads that source descriptor's actual assigned line INT32+10.
    ///Unknown/missing graph endpoints are not mapped to this valid-arc API.</summary>
    public int GetSourceLineIdForArc(int node,int ordinal)
    {
        var arcs=SourceRanges.Graph.GetNode(node);
        if((uint)ordinal>=(uint)arcs.Count)throw new ArgumentOutOfRangeException(nameof(ordinal));
        if((uint)arcs[ordinal].GroupId>=(uint)SourceRanges.Graph.Groups.Count)
            throw new InvalidDataException("Ordinary source group ownership is invalid.");
        return BinaryPrimitives.ReadInt32LittleEndian(Preparation.LinePreparation.Output.CopyDescriptor().Span[16..]);
    }

    /// <summary>Prepared ordinary240 invocation:150D00's owner+60 type test
    ///then18D920 sets sampler210=false, enabling the main gap scales. This
    ///method does NOT select controller240 or prove its creation/lifecycle.
    ///The source metadata, line and graph supplied to that scoped consumer
    ///are derived here. Controller23F uses another scale branch.</summary>
    public LegacyMainWordTransitionGeometry CreatePrepared240TransitionGeometry(LegacyCursiveSeparationTable table,
        CancellationToken cancellationToken=default)
    {
        var graph=SourceRanges.Graph;byte[] descriptor=Preparation.LinePreparation.Output.CopyDescriptor().ToArray();
        int line=BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(16));
        ushort flags=BinaryPrimitives.ReadUInt16LittleEndian(descriptor);
        var facts=new ReadOnlyMemory<LegacyMainWordArcSourceFacts>[graph.NodeCount];
        for(int node=0;node<graph.NodeCount;node++)
        {cancellationToken.ThrowIfCancellationRequested();facts[node]=Enumerable.Repeat(new LegacyMainWordArcSourceFacts(0,line,flags,-1,0,0,0),graph.GetNode(node).Count).ToArray();}
        return new(Owner.TransitionProfile,Owner.AcousticAlphabet,
            Cuts.PreparedInk,graph,facts,SourceHeight,Preparation.SourceGap,table,cancellationToken);
    }
}
