using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Fresh ENlite source247/119 through actual297/108 cuts, ordinary
/// capped/pruned21B groups and no-initial107 classification. This is not the
/// final119 cache, mask or word controller. Returned geometry is detached.</summary>
public sealed class LegacyOwnedEnglish119Candidates
{
    public LegacyOwnedEnglishLiteFullPreprocessing Preparation {get;}
    public LegacyFirstAcoustic119WireModel Model=>Preparation.Model;
    public LegacySampler297WireProfile Sampler {get;}
    public LegacySource247WireChild GraphChild {get;}
    public int MaximumAtoms {get;}
    public int NodeCount=>ranges.Graph.NodeCount;
    public int GroupCount=>ranges.Graph.Groups.Count;
    /// <summary>Live group allocation IDs. Native later group compaction is
    /// not performed by this endpoint; IDs are never replaced with seed IDs.</summary>
    public int[] CopyLiveGroupIds()=>liveGroups.ToArray();
    public int CandidateCount=>ranges.Graph.CandidateCount;
    public int RemovedArcCount {get;}
    private readonly LegacyCursiveCutResult cuts;
    private readonly LegacySourcePointChronology chronology;
    private readonly LegacyOrdinaryGraphSourceRanges ranges;
    internal LegacyOrdinaryGraphSourceRanges SourceRanges=>ranges;
    private readonly LegacyPreparedNetwork107Evaluator[] networks;
    private readonly int[] liveGroups;
    private LegacyOwnedEnglish119Candidates(LegacyOwnedEnglishLiteFullPreprocessing owner,LegacySampler297WireProfile sampler,
        LegacySource247WireChild graph,int cap,LegacyCursiveCutResult cuts,LegacySourcePointChronology chronology,
        LegacyOrdinaryGraphSourceRanges ranges,int removed,LegacyPreparedNetwork107Evaluator[] networks)
    {Preparation=owner;Sampler=sampler;GraphChild=graph;MaximumAtoms=cap;this.cuts=cuts;this.chronology=chronology;
        this.ranges=ranges;RemovedArcCount=removed;this.networks=networks;
        liveGroups=Enumerable.Range(0,NodeCount).SelectMany(n=>ranges.Graph.GetNode(n).Select(a=>a.GroupId)).Distinct().Order().ToArray();}

    public static LegacyOwnedEnglish119Candidates Prepare(LegacyOwnedEnglishLiteFullPreprocessing owner,
        LegacyCursiveSeparationTable table,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);ArgumentNullException.ThrowIfNull(table);token.ThrowIfCancellationRequested();
        var source=owner.Source;
        if(source.Version!=0x110||source.SourceKind!=0x135)throw new NotSupportedException("Actual fresh ordinary ENlite source required.");
        if(source.Alphabet.TryGetLabel("\U000F0009",out _))
            throw new NotSupportedException("The sourceF0009 graph-configuration branch remains unsupported.");
        var samplerChild=source.Children.Single(c=>c.Destination=="247.130");
        var graphChild=source.Children.Single(c=>c.Destination=="247.138");
        var sampler=source.ReadSampler(samplerChild,token);
        byte[] S(string name)=>sampler.Fields.Single(f=>f.Destination==name).CopyBytes();
        float F(string name)=>BinaryPrimitives.ReadSingleLittleEndian(S(name));
        int I(string name)=>BinaryPrimitives.ReadInt32LittleEndian(S(name));
        if(samplerChild.Version!=0x108||S("134.present")[0]!=0||S("1D0.present")[0]!=0||
            source.Fields.Single(f=>f.Owner=="247"&&f.Destination=="148.present").CopyBytes()[0]!=0)
            throw new NotSupportedException("Actual108 with absent optional closure/source-name/category-cap payloads required.");
        byte[] envelope=source.CopyChildEnvelopeBytes(samplerChild,token);
        if(!envelope.AsSpan(16,49).SequenceEqual(Convert.FromHexString("0000003f3333b33ecdcc4c3e6666e63f0001003333b33ecdcc4c3e0000803e0000003f0000003fcdcccc3d060000804001")))
            throw new NotSupportedException("Checked191950 cut scalar profile required.");
        var categories=sampler.ReadCategories(token);
        if(categories.BaseEntryCount!=source.Alphabet.LabelCount)throw new InvalidDataException("Same owned sampler/category/alphabet required.");
        byte[] graphBytes=source.CopyChildEnvelopeBytes(graphChild,token);
        var span=LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(graphBytes,0);
        var seedProfile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(graphBytes,0);
        if(span.MaximumAtoms>15||seedProfile.InsertionShift!=0||graphChild.End!=owner.Model.AcousticChild.Offset)
            throw new NotSupportedException("Owned shift0 bounded21B adjacent to actual119 required.");
        // Same scalar interpreter, validated above; no106 resource reader or
        // Russian preprocessing is used for this108 owner.
        var cuts=LegacyCursiveCutProducer.GenerateRussianVersion106(owner.CopyPreparedStrokes(),owner.Line.Baseline,owner.Line.Height,
            owner.OriginalSource.SourceStrokeProbabilities.ToArray(),false,null,token);
        var chronology=owner.PreparedChronology.WithInsertedPreparedInk(cuts.PreparedInk,token);
        //167210 physical1B/1A=0 and boundary18=-1 survive24A/24C/265.
        int physical=owner.CopyPhysicalPointCounts().Length;
        var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,owner.Line.Height,seedProfile,
            new byte[physical],Enumerable.Repeat(-1,physical).ToArray(),token);
        var ranges=LegacyOrdinaryGraphSourceRanges.Prepare(seeds,chronology,owner.OriginalSource,span.MaximumAtoms,cancellationToken:token);
        var separation=new LegacyCursiveSeparationProfile(F("68"),F("114"),I("118"));
        var profile=new LegacyOrdinarySamplerGroupPruningProfile(separation,I("11C"));
        var prune=LegacyOrdinarySamplerGroupPruning.PrepareSingleDescriptor(ranges.Graph,cuts.PreparedInk,owner.Line.Height,profile,table,token);
        ranges=ranges.WithSamplerPruning(prune,token);
        var activations=LegacyOwnedFirst119Activations.Prepare(owner.Model,token);
        var networks=new[]{activations.CreatePreparedEvaluator(0,token),activations.CreatePreparedEvaluator(1,token)};
        token.ThrowIfCancellationRequested();return new(owner,sampler,graphChild,span.MaximumAtoms,cuts,chronology,ranges,prune.RemovedArcCount,networks);
    }

    public LegacyEnglish119CandidateScore ScoreGroup(int group,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if((uint)group>=(uint)GroupCount)throw new ArgumentOutOfRangeException(nameof(group));
        if(Array.BinarySearch(liveGroups,group)<0)throw new NotSupportedException("A removed, unreferenced allocation is not a live candidate.");
        var prepared=LegacyPreparedEnglishNoInitialGlyph.PrepareOwnedCuts(Preparation,cuts,chronology,ranges.Graph.Groups[group].Ranges.Span,token);
        var features=new LegacyEnglish119CandidateFeatures(this,prepared);
        var outputs=new[]{networks[0].EvaluatePreparedSourceUnmasked(features.CopyTrajectoryInput(),token),
            networks[1].EvaluatePreparedSourceUnmasked(features.CopyRasterInput(),token)};
        token.ThrowIfCancellationRequested();return new(this,group,features,outputs[0],outputs[1]);
    }
    public LegacyInkRange[] CopyGroupRanges(int group)
    {if((uint)group>=(uint)GroupCount)throw new ArgumentOutOfRangeException(nameof(group));return ranges.Graph.Groups[group].Ranges.ToArray();}
    public LegacyOrdinaryCursiveCandidateArc[] CopyNode(int node)=>ranges.Graph.GetNode(node).Select(a=>a with
        {PredecessorArcIndices=a.PredecessorArcIndices.ToArray(),Successors=Array.AsReadOnly(a.Successors.ToArray())}).ToArray();
    public LegacyFirstResultSourceRange[] ResolveCandidate(int node,int ordinal,CancellationToken token=default)=>ranges.ResolveCandidate(node,ordinal,token);
    public LegacySourcePointIdentity ResolveCutPoint(int physical,int point)=>chronology.Resolve(physical,point);
    public InkStroke[] CopyCutStrokes()=>cuts.PreparedInk.ToStrokes().ToArray();
    public LegacyCursivePreparedPoint[] CopyCutRecords()=>cuts.PreparedInk.Points.ToArray();
    public int[] CopyCutCounts()=>cuts.PreparedInk.StrokePointCounts.ToArray();
    public LegacyCursiveCutMarker[] CopyMarkers()=>cuts.Markers.ToArray();
    public LegacyCursiveInkAtom[] CopyAtoms()=>cuts.Atoms.ToArray();
    public float[] CopyPhysicalProbabilities()=>Preparation.OriginalSource.SourceStrokeProbabilities.ToArray();
}

/// <summary>Two actual107 outputs for one real retained-source group. They
/// precede final119 class cache and do not constitute an accepted symbol.</summary>
public sealed record LegacyEnglish119CandidateScore(LegacyOwnedEnglish119Candidates Owner,int GroupId,
    LegacyEnglish119CandidateFeatures Features,LegacyPreparedNetwork107Result Trajectory,LegacyPreparedNetwork107Result Raster);

/// <summary>Features whose ranges address this sampler's POST-cut domain,
/// not its preprocessing owner's earlier265 coordinates.</summary>
public sealed class LegacyEnglish119CandidateFeatures
{
    public LegacyOwnedEnglish119Candidates Owner {get;}
    private readonly LegacyPreparedEnglishNoInitialGlyph value;
    internal LegacyEnglish119CandidateFeatures(LegacyOwnedEnglish119Candidates owner,LegacyPreparedEnglishNoInitialGlyph value)
    {Owner=owner;this.value=value;}
    public LegacyCursiveGlyphBoundarySigns Boundary=>value.Boundary;
    public bool DegenerateIntegerGeometry=>value.DegenerateIntegerGeometry;
    public LegacyInkRange[] CopyStoredRanges()=>value.CopyStoredRanges();
    public LegacyInkRange[] CopyOrderedRanges()=>value.CopyOrderedRanges();
    public int[] CopyNormalizationOrder()=>value.CopyNormalizationOrder();
    public int[] CopyRetainedPenUpBoundaries()=>value.CopyRetainedPenUpBoundaries();
    public LegacyInkStroke[] CopyNormalizedStrokes()=>value.CopyNormalizedStrokes();
    public LegacyInkStroke[] CopyMergedStrokes()=>value.CopyMergedStrokes();
    public short[] CopyTrajectoryInput()=>value.CopyTrajectoryInput();
    public short[] CopyRasterInput()=>value.CopyRasterInput();
}
