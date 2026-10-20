using System.Buffers.Binary;
namespace InkRecognition;

public sealed partial class LegacyEnglish119FinalizedSingleSource
{
    private bool localCandidatesReserved;
    internal LegacyEnglish119MainLocalCandidates ReserveLocalCandidates(LegacyCursiveSeparationTable table,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();if(!complete||faulted||localCandidatesReserved)throw new InvalidOperationException("One complete unreserved finalized EN source required.");
            //The constructor validates real wire branches before reserving;
            //no cut/graph execution or external callback occurs under this gate.
            var next=new LegacyEnglish119MainLocalCandidates(this,original,chronology,points,probabilities,descriptor,guide,table,token);
            token.ThrowIfCancellationRequested();localCandidatesReserved=true;return next;
        }
    }
}

/// <summary>Actual MAIN297/108 cuts and shift1/cap8 deferred LOCAL ordinary
///graph. Not global compact publication, mainNN, masks or a linguistic winner.</summary>
public sealed class LegacyEnglish119MainLocalCandidates
{
    private readonly object gate=new();
    private readonly LegacyOrdinarySourceInk original;
    private readonly LegacySourcePointChronology inputChronology;
    private readonly InkStroke[] points;
    private readonly float[] probabilities;
    private readonly LegacyCursiveSeparationTable table;
    private readonly LegacyOrdinaryCursiveSeedProfile seedProfile;
    private readonly LegacyOrdinarySamplerGroupPruningProfile pruneProfile;
    private LegacyCursiveCutResult? cuts;
    private LegacySourcePointChronology? chronology;
    private LegacyOrdinaryCursiveSeedResult? seeds;
    private LegacyOrdinaryGraphSourceRanges? ranges;
    private LegacyOrdinarySamplerGroupPruningResult? pruning;
    private readonly List<string> trace=[];
    private bool entered,faulted,complete;
    public LegacyEnglish119FinalizedSingleSource Owner{get;}
    public LegacySampler297WireProfile Sampler=>Owner.MainWire.Sampler;
    public int MaximumAtoms=>8;
    public byte InsertionShift=>seedProfile.InsertionShift;
    public float Baseline{get;}
    public float Height{get;}
    public bool IsComplete{get{lock(gate)return complete;}}
    public bool IsFaulted{get{lock(gate)return faulted;}}
    public bool HasEntered{get{lock(gate)return entered;}}
    public int NodeCount{get{lock(gate)return ranges?.Graph.NodeCount??0;}}
    public int GroupCount{get{lock(gate)return ranges?.Graph.Groups.Count??0;}}
    public int DeferredCount{get{lock(gate)return seeds?.DeferredCount??0;}}
    public int RemovedArcCount{get{lock(gate)return pruning?.RemovedArcCount??0;}}
    public int CandidateCount{get{lock(gate)return ranges?.Graph.CandidateCount??0;}}
    public string[] CopyTrace(){lock(gate)return trace.ToArray();}
    public LegacyCursiveCategoryProfile ReadActualCategories(CancellationToken token=default)=>Sampler.ReadCategories(token);
    public InkStroke[] CopyCutStrokes(){lock(gate)return cuts?.PreparedInk.ToStrokes().Select(s=>new InkStroke(s.Points.ToArray())).ToArray()??[];}
    public LegacyCursivePreparedPoint[] CopyCutRecords(){lock(gate)return cuts?.PreparedInk.Points.ToArray()??[];}
    public int[] CopyCutCounts(){lock(gate)return cuts?.PreparedInk.StrokePointCounts.ToArray()??[];}
    public LegacyCursiveInkAtom[] CopyAtoms(){lock(gate)return cuts?.Atoms.ToArray()??[];}
    public LegacyCursiveCutMarker[] CopyMarkers(){lock(gate)return cuts?.Markers.ToArray()??[];}
    public LegacyOrdinaryCursiveSeed[] CopySeeds(){lock(gate)return seeds?.Seeds.ToArray()??[];}
    public LegacyInkRange[] CopyGroupRanges(int group)
    {lock(gate){if(ranges is null||(uint)group>=(uint)ranges.Graph.Groups.Count)throw new ArgumentOutOfRangeException(nameof(group));return ranges.Graph.Groups[group].Ranges.ToArray();}}
    public LegacyOrdinaryCursiveCandidateArc[] CopyNode(int node)
    {lock(gate){if(ranges is null)throw new InvalidOperationException("Completed local graph required.");return ranges.Graph.GetNode(node).Select(a=>a with{PredecessorArcIndices=a.PredecessorArcIndices.ToArray(),Successors=Array.AsReadOnly(a.Successors.ToArray())}).ToArray();}}
    public LegacyFirstResultSourceRange[] ResolveCandidate(int node,int arc,CancellationToken token=default)
    {lock(gate){if(ranges is null)throw new InvalidOperationException("Completed local graph required.");return ranges.ResolveCandidate(node,arc,token);}}
    public LegacySourcePointIdentity ResolveCutPoint(int stroke,int point)
    {lock(gate){if(chronology is null)throw new InvalidOperationException("Completed inserted-point chronology required.");return chronology.Resolve(stroke,point);}}
    public LegacySourcePointIdentity ResolveCutEndingMetadata(int stroke,int point,CancellationToken token=default)=>original.ResolveEndingMetadata(ResolveCutPoint(stroke,point),token);
    public static LegacyEnglish119MainLocalCandidates Prepare(LegacyEnglish119FinalizedSingleSource actualCompletedSource,
        LegacyCursiveSeparationTable table,CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(actualCompletedSource);ArgumentNullException.ThrowIfNull(table);return actualCompletedSource.ReserveLocalCandidates(table,token);}
    internal LegacyEnglish119MainLocalCandidates(LegacyEnglish119FinalizedSingleSource owner,LegacyOrdinarySourceInk original,
        LegacySourcePointChronology chronology,InkStroke[] points,float[] probabilities,byte[] descriptor,byte[] guide,LegacyCursiveSeparationTable table,CancellationToken token)
    {
        Owner=owner;this.original=original;inputChronology=chronology;this.points=points;this.probabilities=probabilities;this.table=SnapshotTable(table,token);
        token.ThrowIfCancellationRequested();var wire=owner.MainWire;var source=wire.Source;var sampler=wire.Sampler;
        if(!ReferenceEquals(sampler.Source,source)||sampler.Child.Offset!=182412||sampler.Child.Version!=0x108||sampler.ReadCategories(token).BaseEntryCount!=217||source.Alphabet.TryGetLabel("\U000f0009",out _))throw new NotSupportedException("Actual main108/category201 without F0009 branch required.");
        byte[] S(string name)=>sampler.Fields.Single(f=>f.Destination==name).CopyBytes();
        int I(string name)=>BinaryPrimitives.ReadInt32LittleEndian(S(name));float F(string name)=>BinaryPrimitives.ReadSingleLittleEndian(S(name));
        if(S("134.present")[0]!=0||S("1D0.present")[0]!=0||source.Fields.Single(f=>f.Owner=="247"&&f.Destination=="148.present").CopyBytes()[0]!=0)throw new NotSupportedException("Actual absent optional main closure/category caps required.");
        var envelope=source.CopyChildEnvelopeBytes(sampler.Child,token);
        if(!envelope.AsSpan(16,49).SequenceEqual(Convert.FromHexString("0000003f3333b33ecdcc4c3e6666e63f0001003333b33ecdcc4c3e0000803e0000003f0000003fcdcccc3d060000804001")))throw new NotSupportedException("Exact actual191950 scalar interpreter required.");
        var graph=source.CopyChildEnvelopeBytes(wire.Graph,token);var span=LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(graph,0);seedProfile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(graph,0);
        if(span.MaximumAtoms!=8||span.NativeSecondByte!=1||!seedProfile.Reorder||seedProfile.InsertionShift!=1)throw new NotSupportedException("Actual deferred main21B profile required.");
        Baseline=BinaryPrimitives.ReadSingleLittleEndian(guide);Height=BinaryPrimitives.ReadSingleLittleEndian(guide.AsSpan(4));
        if(!float.IsFinite(Baseline)||!float.IsFinite(Height)||Height<=0||points.Length!=probabilities.Length||points.Length!=chronology.StrokePointCounts.Length)throw new ArgumentException("Whole actual assigned frame/point/probability lineage required.");
        pruneProfile=new(new(F("68"),F("114"),I("118")),I("11C"));
        if(pruneProfile.RejectionThreshold<0||pruneProfile.RejectionThreshold>0x4000000||!float.IsFinite(pruneProfile.Separation.GapOffset)||!float.IsFinite(pruneProfile.Separation.Slope)||pruneProfile.Separation.Slope<0)throw new InvalidDataException("Actual finite main pruning fields required.");
    }
    private static LegacyCursiveSeparationTable SnapshotTable(LegacyCursiveSeparationTable actual,CancellationToken token)
    {
        //Public ReadOnlyMemory can be recovered by its caller. Retain a NEW
        //hash-validated bounded table, rather than the caller's mutable storage.
        float[] values=actual.Values.ToArray();if(values.Length!=256)throw new ArgumentException("Whole actual global separation table required.");
        using var stream=new MemoryStream();using(var w=new System.Text.Json.Utf8JsonWriter(stream))
        {
            w.WriteStartObject();w.WriteNumber("schemaVersion",1);w.WriteString("profile","legacy-ru-lite-separation-v1");
            w.WriteString("sha256","b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf");w.WriteStartArray("values");
            foreach(float value in values){token.ThrowIfCancellationRequested();if(!float.IsFinite(value))throw new ArgumentException("Finite global separation table required.");w.WriteNumberValue(value);}
            w.WriteEndArray();w.WriteEndObject();w.Flush();
        }
        return LegacyCursiveSeparationTable.Load(stream.ToArray(),token);
    }
    public LegacyEnglish119MainLocalCandidates Run(CancellationToken token=default)=>RunCore(null,token);
    internal LegacyEnglish119MainLocalCandidates RunPrepared(Action<string> checkpoint,CancellationToken token=default)=>RunCore(checkpoint,token);
    private LegacyEnglish119MainLocalCandidates RunCore(Action<string>? checkpoint,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate){token.ThrowIfCancellationRequested();if(entered)throw new InvalidOperationException("Local candidate producer is once-only.");entered=true;trace.Add("enter");}
        void Check(string stage){lock(gate)trace.Add(stage);checkpoint?.Invoke(stage);token.ThrowIfCancellationRequested();}
        try
        {
            checkpoint?.Invoke("enter");token.ThrowIfCancellationRequested();
            var cut=LegacyCursiveCutProducer.GenerateRussianVersion106(points,Baseline,Height,probabilities,false,null,token);
            int total=0;foreach(int n in cut.PreparedInk.StrokePointCounts.Span){token.ThrowIfCancellationRequested();if(n<1||n>4096-total)throw new ArgumentException("Whole inserted-point budget exceeded.");total+=n;}
            if(cut.Atoms.Count is<1 or>4096)throw new ArgumentException("Bounded complete main atoms required.");
            var map=inputChronology.WithInsertedPreparedInk(cut.PreparedInk,token);lock(gate){cuts=cut;chronology=map;}Check("108:cuts");
            var seed=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cut,Height,seedProfile,new byte[points.Length],Enumerable.Repeat(-1,points.Length).ToArray(),token);
            lock(gate)seeds=seed;Check("21B:deferred-seeds");
            var workBudget=new LegacyOrdinaryGraphWorkBudget(token);
            var raw=LegacyOrdinaryGraphSourceRanges.PrepareBounded(seed,map,original,8,4096,workBudget,token);
            int rangeCount=0,edgeCount=0;foreach(var group in raw.Graph.Groups){token.ThrowIfCancellationRequested();if(group.Ranges.Length>65536-rangeCount)throw new ArgumentException("Whole local group range budget exceeded.");rangeCount+=group.Ranges.Length;}
            for(int n=0;n<raw.Graph.NodeCount;n++)foreach(var arc in raw.Graph.GetNode(n))
            {token.ThrowIfCancellationRequested();int edges=checked(arc.PredecessorArcIndices.Length+arc.Successors.Count);if(edges>131072-edgeCount)throw new ArgumentException("Whole local graph adjacency budget exceeded.");edgeCount+=edges;}
            lock(gate)ranges=raw;Check("21B:local-ranges");
            workBudget.PruningCopies(raw.Graph.NodeCount,raw.Graph.Groups.Count,raw.Graph.CandidateCount);
            var pruned=LegacyOrdinarySamplerGroupPruning.PrepareSingleDescriptor(raw.Graph,cut.PreparedInk,Height,pruneProfile,table,token);
            var final=raw.WithSamplerPruning(pruned,token);lock(gate){ranges=final;pruning=pruned;}Check("108:local-pruning");
            lock(gate){complete=true;trace.Add("complete");}return this;
        }
        catch{lock(gate){faulted=true;trace.Add("fault");}throw;}
    }
}

