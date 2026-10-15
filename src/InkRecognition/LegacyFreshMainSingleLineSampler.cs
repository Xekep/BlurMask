using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Actual fresh main source with one assigned descriptor and any
/// bounded number of physical strokes, after24B/24D/29B/265. Keeps actual
/// main ownership, guides, source720 and original metadata. Multiple descriptors
/// and nonnull optional category-limit objects are deliberately excluded.</summary>
public sealed class LegacyFreshMainSingleLineSampler
{
    private readonly byte[] resource,descriptor;
    public LegacyFreshMainActiveGapSmoothed Preparation {get;}
    public LegacySourceSamplerCategoryOwner CategoryOwner {get;}
    public LegacyMainWordTransitionProfile TransitionProfile {get;}
    public LegacyInkAlphabet AcousticAlphabet {get;}
    public LegacyCursiveLatticeSpanProfile SpanProfile {get;}
    public int GraphOffset {get;}
    public int AcousticOffset {get;}
    public short AcousticTargetHeight {get;}
    public float SourceBaseline {get;}
    public float SourceHeight {get;}
    public int SourceLineId {get;}
    public int SourceDescriptorOrdinal=>0;
    public ReadOnlyMemory<byte> CopyDescriptor()=>descriptor.ToArray();
    private LegacyFreshMainSingleLineSampler(byte[] resource,LegacyFreshMainActiveGapSmoothed preparation,
        LegacySourceSamplerCategoryOwner categories,LegacyMainWordTransitionProfile transition,LegacyInkAlphabet alphabet,
        LegacyCursiveLatticeSpanProfile span,int graph,int acoustic,short target,byte[] descriptor,float baseline,float height,int line)
    {this.resource=resource;Preparation=preparation;CategoryOwner=categories;TransitionProfile=transition;AcousticAlphabet=alphabet;
        SpanProfile=span;GraphOffset=graph;AcousticOffset=acoustic;AcousticTargetHeight=target;this.descriptor=descriptor;
        SourceBaseline=baseline;SourceHeight=height;SourceLineId=line;}

    public static LegacyFreshMainSingleLineSampler Prepare(ReadOnlySpan<byte> actualResource,LegacyFreshMainActiveGapSmoothed actualSource,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualSource);cancellationToken.ThrowIfCancellationRequested();
        var lines=actualSource.Input.Input.Input;
        if(!Convert.ToHexString(SHA256.HashData(actualResource)).Equals(lines.Owner.Input.Input.Input.SourceFingerprint,StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Exact actual main source owner required.");
        byte[][] descriptors=lines.CopyDescriptors();
        if(descriptors.Length!=1||(BinaryPrimitives.ReadUInt16LittleEndian(descriptors[0])&15)!=0)
            throw new NotSupportedException("One actual assigned ordinary descriptor required; multiple-source cut-domain replacement is separate.");
        byte[] descriptor=descriptors[0];int line=I(descriptor,16),first=I(descriptor,36),last=I(descriptor,40);
        var guides=lines.CopyGuides();
        if(first!=0||last!=actualSource.Strokes.Count-1||line<0||line>=guides.Length||guides[line].Length!=40)
            throw new ArgumentException("Actual full physical interval and source guide owner required.");
        float baseline=F(guides[line],0),height=F(guides[line],4);
        if(!float.IsFinite(baseline)||!float.IsFinite(height)||height<=0)throw new ArgumentException("Finite assigned main line required.");
        var categories=LegacySourceSamplerCategoryOwner.ReadV10dSource(actualResource,0,cancellationToken);
        int sampler=categories.SamplerNodeOffset;
        var prefix=Convert.FromHexString("0000003f3333b33ecdcc4c3e6666e63f0001003333b33ecdcc4c3e0000803e0000003f0000003fcdcccc3d060000804001");
        if(!actualResource.Slice(sampler+16,prefix.Length).SequenceEqual(prefix))throw new NotSupportedException("Actual Russian297/v106 scalar profile required.");
        var transition=LegacyMainWordTransitionProfile.ReadV106Node(actualResource,sampler,cancellationToken);
        int graph=checked(sampler+8+I(actualResource,sampler+4));var span=LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(actualResource,graph);
        int acoustic=checked(graph+8+I(actualResource,graph+4));
        if(acoustic!=189734||span.MaximumAtoms>15||I(actualResource,acoustic)!=0x101fe90||I(actualResource,acoustic+8)!=0x21d||
            BinaryPrimitives.ReadUInt16LittleEndian(actualResource[(acoustic+14)..])!=0x110||actualResource[acoustic+16]!=255||actualResource[acoustic+17]!=255)
            throw new InvalidDataException("Actual main21D/v110 FF/FF normalization required.");
        short target=BinaryPrimitives.ReadInt16LittleEndian(actualResource[(acoustic+18)..]);if(target<=0)throw new InvalidDataException("Positive actual main normalization height required.");
        var lookup=lines.Owner.Controller.Alphabet;var alphabet=LegacyInkAlphabet.Load(actualResource[lookup.AlphabetPayloadOffset..],
            BinaryPrimitives.ReadUInt16LittleEndian(actualResource[(lookup.AlphabetPayloadOffset-2)..]));
        if(alphabet.LabelCount!=transition.Categories.BaseEntryCount)throw new InvalidDataException("Actual main category/alphabet ownership differs.");
        return new(actualResource.ToArray(),actualSource,categories,transition,alphabet,span,graph,acoustic,target,descriptor,baseline,height,line);
    }

    public LegacyFreshMainSingleLineSampled Sample(LegacyCursiveSeparationTable table,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(table);cancellationToken.ThrowIfCancellationRequested();
        var markerPolicy=LegacyFreshMainMarkerLimitPolicy.Prepare(resource,this,cancellationToken);
        var cuts=markerPolicy.GenerateCuts(cancellationToken);
        var chronology=Preparation.Chronology.WithInsertedPreparedInk(cuts.PreparedInk,cancellationToken);
        int count=Preparation.Strokes.Count;
        var profile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,GraphOffset);
        var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,SourceHeight,profile,new byte[count],
            Enumerable.Repeat(-1,count).ToArray(),cancellationToken);
        var currentSource=Preparation.Input.Input.Input.Owner.Controller.Source;
        var ranges=LegacyOrdinaryGraphSourceRanges.Prepare(seeds,chronology,currentSource,SpanProfile.MaximumAtoms,cancellationToken:cancellationToken);
        var pruning=LegacyOrdinarySamplerGroupPruning.Prepare(ranges.Graph,cuts.PreparedInk,SourceHeight,
            LegacyOrdinarySamplerGroupPruningProfile.ReadV106Node(resource,CategoryOwner.SamplerNodeOffset),table,cancellationToken);
        ranges=ranges.WithSamplerPruning(pruning,cancellationToken);
        return new(this,cuts,chronology,ranges,pruning);
    }
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private static float F(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadSingleLittleEndian(b[at..]);
}

public sealed class LegacyFreshMainSingleLineSampled
{
    public LegacyFreshMainSingleLineSampler Owner {get;}
    public LegacyCursiveCutResult Cuts {get;}
    public LegacySourcePointChronology Chronology {get;}
    public LegacyOrdinaryGraphSourceRanges SourceRanges {get;}
    public LegacyOrdinarySamplerGroupPruningResult Pruning {get;}
    internal LegacyFreshMainSingleLineSampled(LegacyFreshMainSingleLineSampler owner,LegacyCursiveCutResult cuts,
        LegacySourcePointChronology chronology,LegacyOrdinaryGraphSourceRanges ranges,LegacyOrdinarySamplerGroupPruningResult pruning)
    {Owner=owner;Cuts=cuts;Chronology=chronology;SourceRanges=ranges;Pruning=pruning;}

    public LegacyNormalizedPreparedGlyph NormalizeGroup(int groupIndex,CancellationToken cancellationToken=default)
    {
        var ordered=GetChronologicalGroupRanges(groupIndex,cancellationToken);float min=GetGroupMinimumX(groupIndex,cancellationToken);
        return LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(Cuts.PreparedInk,ordered,min,Owner.SourceBaseline,
            Owner.SourceHeight,Owner.AcousticTargetHeight,Owner.SpanProfile.MaximumAtoms,cancellationToken);
    }
    public LegacyInkRange[] GetChronologicalGroupRanges(int groupIndex,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();var groups=SourceRanges.Graph.Groups;
        if((uint)groupIndex>=(uint)groups.Count)throw new ArgumentOutOfRangeException(nameof(groupIndex));
        var ranges=groups[groupIndex].Ranges.ToArray();var order=Chronology.OrderRangeIndicesForNormalization(ranges,cancellationToken);
        return order.Select(i=>ranges[i]).ToArray();
    }
    public float GetGroupMinimumX(int groupIndex,CancellationToken cancellationToken=default)
    {
        var ranges=GetChronologicalGroupRanges(groupIndex,cancellationToken);var counts=Cuts.PreparedInk.StrokePointCounts.Span;
        var starts=new int[counts.Length];for(int i=1;i<starts.Length;i++)starts[i]=starts[i-1]+counts[i-1];
        float min=float.MaxValue;foreach(var range in ranges)for(int p=range.StartPoint;p<range.EndPointExclusive;p++)
        {cancellationToken.ThrowIfCancellationRequested();min=Math.Min(min,Cuts.PreparedInk.Points.Span[starts[range.StrokeIndex]+p].X);}return min;
    }
    public LegacySourcePrimitiveScope GetPrimitiveScope(int groupIndex,CancellationToken cancellationToken=default)
    {
        var ranges=GetChronologicalGroupRanges(groupIndex,cancellationToken);bool whole=false,zero=false;int physical=-1;float? probability=null;
        var original=Owner.Preparation.Input.Input.Input.Owner.Controller.Source;
        if(ranges.Length==1)
        {
            var range=ranges[0];whole=range.StartPoint==0&&range.EndPointExclusive==Cuts.PreparedInk.StrokePointCounts.Span[range.StrokeIndex];
            if(whole)
            {
                physical=range.StrokeIndex;probability=original.SourceStrokeProbabilities.Span[physical];
                zero=Chronology.Resolve(physical,0).Key.Third==0&&original.ResolveEndingMetadata(Chronology.Resolve(physical,range.EndPointExclusive-1),cancellationToken).Key.Third==0;
            }
        }
        return new(whole,zero,physical,probability);
    }

    /// <summary>Prepared ordinary240 geometry over this exact source: descriptor
    /// ID/line/flags are actual; fresh physical endpoint nibbles/boundary fields
    /// and ordinary19F850/1A3B30 group metadata retain their native zero/minus-one state.
    /// Controller240 construction/selection remains outside this method.</summary>
    public LegacyMainWordTransitionGeometry CreatePrepared240TransitionGeometry(LegacyCursiveSeparationTable table,
        CancellationToken cancellationToken=default)
    {
        var graph=SourceRanges.Graph;ushort flags=BinaryPrimitives.ReadUInt16LittleEndian(Owner.CopyDescriptor().Span);
        var facts=new ReadOnlyMemory<LegacyMainWordArcSourceFacts>[graph.NodeCount];
        for(int node=0;node<graph.NodeCount;node++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            facts[node]=Enumerable.Repeat(new LegacyMainWordArcSourceFacts(Owner.SourceDescriptorOrdinal,Owner.SourceLineId,flags,-1,0,0,0),graph.GetNode(node).Count).ToArray();
        }
        return new(Owner.TransitionProfile,Owner.AcousticAlphabet,Cuts.PreparedInk,graph,facts,
            Owner.SourceHeight,Owner.Preparation.SourceGap,table,cancellationToken);
    }
}
