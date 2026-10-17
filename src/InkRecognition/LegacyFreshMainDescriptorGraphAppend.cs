using System.Security.Cryptography;

namespace InkRecognition;

public sealed class LegacyMainDescriptorLocalGraph
{
    public LegacyMainDescriptorCutStep Source {get;}
    public LegacyOrdinaryCursiveSeedResult Seeds {get;}
    public LegacyOrdinaryCursiveSeedGraph SeedGraph {get;}
    /// <summary>Local arithmetic projection, with physical indices rebased to
    /// this descriptor. It is not a global classifier graph or source map.</summary>
    public LegacyOrdinarySamplerGroupPruningResult LocalPruning {get;}
    public LegacyMainGraphAppendState LocalState {get;}
    public LegacyMainGraphAppendResult Append {get;}
    internal LegacyMainLocalGraphMetadata Metadata {get;}
    internal LegacyMainDescriptorLocalGraph(LegacyMainDescriptorCutStep source,LegacyOrdinaryCursiveSeedResult seeds,
        LegacyOrdinaryCursiveSeedGraph seedGraph,LegacyOrdinarySamplerGroupPruningResult pruning,
        LegacyMainLocalGraphMetadata metadata,LegacyMainGraphAppendState local,LegacyMainGraphAppendResult append)
    {Source=source;Seeds=seeds;SeedGraph=seedGraph;LocalPruning=pruning;Metadata=metadata;LocalState=local;Append=append;}
}

/// <summary>Actual multiatom descriptor local allocation/pruning and19D470
/// append over ONE owned sequential cut source. Preserves descriptor/line and
/// global physical ranges, pending contexts, placement and sticky references.
/// Cross-source1A3B30/19E8B0 and boundary pruning have NOT run. Consequently
/// this exposes append state, not a finished global NN/recognition graph.</summary>
public sealed class LegacyFreshMainDescriptorGraphAppend
{
    public LegacyFreshMainDescriptorCutSource Source {get;}
    public LegacyMainGraphAppendState State {get;}
    public IReadOnlyList<LegacyMainDescriptorLocalGraph> Steps {get;}
    private LegacyFreshMainDescriptorGraphAppend(LegacyFreshMainDescriptorCutSource source,
        LegacyMainGraphAppendState state,LegacyMainDescriptorLocalGraph[] steps)
    {Source=source;State=state;Steps=Array.AsReadOnly(steps);}

    public LegacyMainDescriptorCutStep ResolveGroupSource(int groupIndex,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if((uint)groupIndex>=(uint)State.GroupCount)throw new ArgumentOutOfRangeException(nameof(groupIndex));
        int descriptor=State.Groups[groupIndex].Fields.DescriptorOrdinal;
        return Source.Steps.Single(s=>s.DescriptorOrdinal==descriptor&&!s.IsPendingSeparator);
    }

    public static LegacyFreshMainDescriptorGraphAppend Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainDescriptorCutSource source,LegacyCursiveSeparationTable table,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(source);ArgumentNullException.ThrowIfNull(table);token.ThrowIfCancellationRequested();
        if(resource.Length!=572763||!Convert.ToHexString(SHA256.HashData(resource)).Equals(source.SourceFingerprint,StringComparison.OrdinalIgnoreCase)||
            source.CategoryOwner.SelectedSourceOffset!=0||source.CategoryOwner.SamplerNodeOffset!=188157||source.GraphOffset!=189379||
            source.SpanProfile.MaximumAtoms!=8||source.GraphConfigurationByte3)
            throw new ArgumentException("Same actual fresh main0 cut source/resource required.");
        var seedProfile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,source.GraphOffset);
        var pruneProfile=LegacyOrdinarySamplerGroupPruningProfile.ReadV106Node(resource,source.CategoryOwner.SamplerNodeOffset);
        var state=LegacyMainGraphAppendState.Empty(source.SpanProfile.MaximumAtoms);var output=new List<LegacyMainDescriptorLocalGraph>();
        foreach(var step in source.Steps)
        {
            token.ThrowIfCancellationRequested();if(step.IsPendingSeparator)continue;
            if(step.Kind!=0)throw new NotSupportedException("Only actual ordinary source descriptors are restored.");
            var localCuts=CreateLocalGeometryView(step,token);
            //Fresh ordinary source218 physical metadata70=0,F0=-1. The
            //retained owner excludes external/reused property state; these
            //are source metadata predicates, not optional graph defaults.
            int count=step.LastPhysical-step.FirstPhysical+1;
            var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(localCuts,step.Height,seedProfile,
                new byte[count],Enumerable.Repeat(-1,count).ToArray(),token);
            var seedGraph=LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds,token);
            var metadata=new LegacyMainLocalGraphMetadata(seeds,checked((ushort)step.DescriptorOrdinal),step.FirstPhysical,
                step.CopyPendingDescriptorOrdinals(),token);
            var generated=LegacyOrdinaryCursiveCandidateGraph.GenerateWithAllocations(seeds,seedGraph,state.MaximumAtoms,131072,metadata,token);
            var pruning=LegacyOrdinarySamplerGroupPruning.Prepare(generated,localCuts.PreparedInk,step.Height,pruneProfile,table,token);
            var local=metadata.Complete(pruning,token);var appended=state.Append(local,token);
            output.Add(new(step,seeds,seedGraph,pruning,metadata,local,appended));state=appended.State;
        }
        token.ThrowIfCancellationRequested();return new(source,state,output.ToArray());
    }

    internal static LegacyCursiveCutResult CreateLocalGeometryView(LegacyMainDescriptorCutStep step,CancellationToken token=default)
    {
        var points=step.CopySelectedPreparedPoints();var counts=step.CopyOutputPointCounts();int first=step.FirstPhysical,last=step.LastPhysical;
        if(first<0||last<first||last>=counts.Length||step.Kind!=0)throw new ArgumentException("Ordinary physical descriptor interval required.");
        var strokes=new InkStroke[last-first+1];int offset=0;
        for(int physical=first;physical<=last;physical++)
        {
            token.ThrowIfCancellationRequested();int length=counts[physical];
            if(length<1||offset>points.Length-length)throw new InvalidDataException("Selected coordinate/count ownership differs.");
            var row=new InkPoint[length];for(int p=0;p<length;p++)
            {if(points[offset+p].StrokeIndex!=physical)throw new InvalidDataException("Selected physical identity differs.");row[p]=new(points[offset+p].X,points[offset+p].Y);}
            strokes[physical-first]=new(Array.AsReadOnly(row));offset+=length;
        }
        if(offset!=points.Length)throw new InvalidDataException("Unconsumed selected points.");
        var ink=LegacyCursivePreparedInk.Create(strokes,token).WithTags(points.Select(p=>p.Tag).ToArray());
        //This local view is ONLY seed/pruning arithmetic. Its freshly numbered
        //OriginalPointIndex is never published as source chronology. Check all
        //geometry bits against the retained actual selected records.
        for(int i=0;i<points.Length;i++)
        {
            token.ThrowIfCancellationRequested();var a=points[i];var b=ink.Points.Span[i];
            if(BitConverter.SingleToInt32Bits(a.DeltaX)!=BitConverter.SingleToInt32Bits(b.DeltaX)||
                BitConverter.SingleToInt32Bits(a.DeltaY)!=BitConverter.SingleToInt32Bits(b.DeltaY)||
                BitConverter.SingleToInt32Bits(a.SegmentLength)!=BitConverter.SingleToInt32Bits(b.SegmentLength))
                throw new NotSupportedException("Selected descriptor geometry must retain exact native forward records.");
        }
        var markers=step.CopyMarkers().Select(m=>m with{StrokeIndex=m.StrokeIndex-first}).ToArray();
        var atoms=step.CopyAtoms().Select(a=>new LegacyCursiveInkAtom(a.Marker with{StrokeIndex=a.Marker.StrokeIndex-first},
            a.Range with{StrokeIndex=a.Range.StrokeIndex-first})).ToArray();
        return new(ink,Array.AsReadOnly(markers),Array.AsReadOnly(atoms));
    }
}
