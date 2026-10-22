using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
namespace InkRecognition;

/// <summary>One actual fresh EN main descriptor's pre-NN filtered submission.
/// Map ordinals address retained current-parent strokes, not original raw IDs.</summary>
public sealed class LegacyFullEnglishFirstChildInput
{
    public LegacyFullEnglishFirstParentInput Owner{get;}
    public int DescriptorOrdinal{get;}
    public LegacyFullEnglishFirstSubmission Submission{get;}
    public bool IsSoleFlat{get;}
    private readonly int[] map;
    internal IReadOnlyList<int> Map=>map;
    internal LegacyFullEnglishFirstChildInput(LegacyFullEnglishFirstParentInput owner,int ordinal,int[] actualMap,bool soleFlat,CancellationToken token)
    {
        Owner=owner;DescriptorOrdinal=ordinal;map=actualMap;IsSoleFlat=soleFlat;
        Submission=LegacyFullEnglishFirstSubmission.Prepare(owner.Knowledge,owner.Budget,map.Select(i=>owner.Current[i]).ToArray(),token);
    }
    public int[] CopyCurrentParentMap()=>map.ToArray();
}

/// <summary>Fresh ordinary EN247/110 main ingress through owned scale/2E3
/// split/reorder and157C60 submissions. Stops at first249: no24B+, main119,
/// page/prior/property or document acceptance is implied.</summary>
public sealed class LegacyFullEnglishFirstParentInput
{
    private const string MainHash="355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927";
    private readonly object gate=new();
    private readonly InkStroke[] raw;
    private readonly LegacyFreshOrdinaryRawPacket packet;
    private readonly LegacyFreshPhysicalBuffer split,current;
    private readonly LegacyFullEnglishFirstChildInput[] children;
    private readonly LegacyFreshMainPhysicalReceiverCall[] calls;
    private readonly LegacyUnknownLineGroups groups;
    private readonly float[] samples;
    private readonly byte[] provider,early2D0;
    private readonly List<LegacyFullEnglishFirstAcousticCache> prefix=[];
    private readonly LegacyFullEnglishFirstBudget budget;
    public LegacyFullEnglishFirstKnowledge Knowledge{get;}
    internal LegacyFullEnglishFirstBudget Budget=>budget;
    private readonly byte[][] publishedDescriptors;
    private bool entered,faulted,completed;
    public LegacyFirstAcoustic119WireModel Model{get;}
    public float PhysicalResolution{get;}
    public LegacyLineScales SourceScales{get;}
    public float InitialNeuralScale{get;}
    public bool ParentScaleOverride=>false;
    public IReadOnlyList<LegacyFullEnglishFirstChildInput> Children{get;}
    public bool IsFaulted{get{lock(gate)return faulted;}}
    public bool IsComplete{get{lock(gate)return completed;}}
    internal IReadOnlyList<InkStroke> Current=>current.Strokes;
    private LegacyFullEnglishFirstParentInput(LegacyFullEnglishFirstKnowledge knowledge,LegacyFullEnglishFirstBudget budget,InkStroke[] raw,float resolution,
        LegacyFreshOrdinaryRawPacket packet,LegacyOrdinaryLineScaleSnapshot scale,LegacyOrdinaryNeuralLineGrouping grouping,
        LegacyFreshPhysicalBuffer split,LegacyFreshPhysicalOrder reordered,byte[] provider,byte[] early2D0,CancellationToken token)
    {
        Knowledge=knowledge;this.budget=budget;Model=knowledge.Model;this.raw=raw;PhysicalResolution=resolution;this.packet=packet;SourceScales=scale.Scales;
        samples=scale.Samples.ToArray();groups=grouping.Selected;InitialNeuralScale=grouping.NeuralScale;
        this.split=split;current=reordered.Buffer;calls=reordered.Calls;this.provider=provider;this.early2D0=early2D0;
        publishedDescriptors=current.Descriptors.Select(d=>d.ToArray()).ToArray();
        int derived=current.Strokes.Sum(s=>s.Points.Count);if(derived>8192)throw new ArgumentException("Whole derived source point budget exceeded.");
        int filteredPoints=0;var planned=new List<(int Descriptor,int[] Map,bool SoleFlat)>();
        for(int d=0;d<current.Descriptors.Length;d++)
        {
            token.ThrowIfCancellationRequested();var descriptor=current.Descriptors[d];if((descriptor[0]&15)==3)continue;
            int first=I(descriptor,36),last=I(descriptor,40);
            int[] map=LegacyFirstResultSourceRecord.SelectChildPhysicalIndices(current.Strokes,first,last,0,SourceScales.Primary,token);
            bool soleFlat=first==last&&Flat(current.Strokes[first],SourceScales.Primary,token);
            int retained=map.Sum(i=>current.Strokes[i].Points.Count);if(retained>8192-filteredPoints)throw new ArgumentException("Whole parent filtered-input copy budget exceeded.");filteredPoints+=retained;
            if(map.Length==0)throw new NotSupportedException("A native filtered-empty child requires a separate result lifecycle; no fallback is fabricated.");
            planned.Add((d,map,soleFlat));
        }
        if(planned.Count==0)throw new NotSupportedException("No ordinary full-FIRST child is available.");
        budget.Children(planned.Count);
        children=planned.Select(p=>new LegacyFullEnglishFirstChildInput(this,p.Descriptor,p.Map,p.SoleFlat,token)).ToArray();Children=Array.AsReadOnly(children);
    }
    public static LegacyFullEnglishFirstParentInput Prepare(LegacyFullEnglishFirstKnowledge knowledge,
        IReadOnlyList<InkStroke> exactMainRaw,float actualPhysicalResolution,
        ReadOnlySpan<byte> spatial30Resource,ReadOnlySpan<byte> physical50Resource,LegacyFullEnglishFirstBounds? bounds=null,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(knowledge);var actualSelectedModel=knowledge.Model;var budget=new LegacyFullEnglishFirstBudget(bounds??new(),token);ArgumentNullException.ThrowIfNull(exactMainRaw);token.ThrowIfCancellationRequested();
        var parent=actualSelectedModel.ParentSource;
        if(parent.Offset!=0||parent.Version!=0x110||parent.Base134Version!=0x300||parent.SourceKind!=0x135||parent.CoordinateMode!=2||
            !parent.Fingerprint.Equals(MainHash,StringComparison.OrdinalIgnoreCase)||
            actualSelectedModel.SelectedProcessor.Offset!=8827||actualSelectedModel.Source.Offset!=8843||
            actualSelectedModel.SourceScoreExponent!=.75f||!ReferenceEquals(actualSelectedModel.SelectedProcessor.Owner,parent))
            throw new ArgumentException("Actual selected full English main247/110 and its own first249 required.");
        if(!float.IsFinite(actualPhysicalResolution)||actualPhysicalResolution<0||actualPhysicalResolution>1_000_000)
            throw new ArgumentException("Mandatory finite actual source resolution required.");
        if(spatial30Resource.Length!=85812||physical50Resource.Length!=161332)throw new ArgumentException("Exact bounded line-resource lengths required before copying.");
        //Own both bounded wire snapshots before fingerprint checks and every
        //later numeric load, so mutable caller bytes cannot switch models.
        spatial30Resource=spatial30Resource.ToArray();physical50Resource=physical50Resource.ToArray();token.ThrowIfCancellationRequested();
        CheckHash(spatial30Resource,85812,"10f4b43cd02873f1995f543dece70e70c1f22cbdb7dcbf63e1c5c205966ac70e");
        CheckHash(physical50Resource,161332,"01f9fd0bf4b5c0c3c1acc170ee82938e24a132696acbfbcfb34573a9d0550c7e");
        byte[] Field(string name)=>parent.Fields.Single(f=>f.Owner=="parametersC0"&&f.Destination==name).CopyBytes();
        if(!new[]{"20","21","legacyThirdPresence","22"}.Select(n=>Field(n)[0]).SequenceEqual(new byte[]{0,1,1,1})||
            BinaryPrimitives.ReadSingleLittleEndian(Field("24"))!=.25f)
            throw new InvalidDataException("Actual EN early source flags and tolerance required.");
        var provider=parent.CopyChildEnvelopeBytes(parent.Children.Single(c=>c.Destination=="parametersC0.30"),token);
        var early=parent.CopyChildEnvelopeBytes(parent.Children.Single(c=>c.Destination=="parametersC0.38"),token);
        CheckHash(provider,135,"319b7934037964b875a6f76739f4b336de0d36e2f7f449b991c8088fb3c3e38a");
        CheckHash(early,693,"5b5a4bbfbf980aab897be5a7c4a093f4d721c54b6afc3e77ccee2f3024b17474");
        //Retain actual EN2D0 despite native freshB4=0 predicate shortcut.
        var raw=Snapshot(exactMainRaw,token);
        var sourceProfile=new LegacyOrdinarySourceProfile(parent.SourceKind,Field("40")[0]!=0,parent.ParametersOffset,parent.EnclosureFlagOffset);
        var packet=LegacyFreshOrdinaryRawPacket.PrepareDirect(raw,sourceProfile,token);
        float F(string name)=>BinaryPrimitives.ReadSingleLittleEndian(Field(name));
        var scale=LegacyOrdinaryLineScaleSource.Extract(packet.Source,new(F("10"),F("14"),F("18"),F("1C")),actualPhysicalResolution,token);
        using var stream30=new MemoryStream(spatial30Resource.ToArray(),false);using var stream50=new MemoryStream(physical50Resource.ToArray(),false);
        var spatial=PreparedLineBoundaryClassifier.Load(stream30);var physical=PreparedLineBoundaryClassifier.Load(stream50);
        var grouping=LegacyOrdinaryNeuralLineInferenceFactory.InferGrouping(scale,actualPhysicalResolution,new bool[packet.PhysicalStrokeCount],true,.8f,spatial,physical,5,.75f,token);
        var split=LegacyFreshMainPhysicalKernels.Split(packet,grouping.Selected,token);
        var reordered=LegacyFreshMainPhysicalKernels.Reorder(split,scale,grouping,actualPhysicalResolution,spatial30Resource,physical50Resource,token);
        token.ThrowIfCancellationRequested();return new(knowledge,budget,raw,actualPhysicalResolution,packet,scale,grouping,split,reordered,provider,early,token);
    }
    public InkStroke[] CopyRaw()=>Copy(raw);
    public InkStroke[] CopyRetainedIngress()=>Copy(packet.Source.Strokes);
    public InkStroke[] CopySplitStrokes()=>Copy(split.Strokes);
    public InkStroke[] CopyCurrentStrokes()=>Copy(current.Strokes);
    public int[] CopySplitOriginalPhysical()=>split.OriginalPhysical.ToArray();
    public int[] CopyCurrentOriginalPhysical()=>current.OriginalPhysical.ToArray();
    public float[] CopyCurrentProbabilities()=>current.Probabilities.ToArray();
    public float[] CopyScaleSamples()=>samples.ToArray();
    public int[][] CopyProviderGroups()=>groups.Groups.Select(g=>g.ToArray()).ToArray();
    public LegacyFreshMainPhysicalReceiverCall[] CopyReceiverCalls()=>calls.ToArray();
    public byte[] CopyActualProviderEnvelope()=>provider.ToArray();
    public byte[] CopyActualEarly2D0Envelope()=>early2D0.ToArray();
    public LegacySourcePointIdentity ResolveCurrentPoint(int stroke,int point)=>current.Chronology.Resolve(stroke,point);
    public LegacySourcePointIdentity ResolveCurrentEndingMetadata(int stroke,int point,CancellationToken token=default)
        =>packet.Source.ResolveEndingMetadata(current.Chronology.Resolve(stroke,point),token);
    public byte[][] CopyDescriptors(){lock(gate)return publishedDescriptors.Select(d=>d.ToArray()).ToArray();}
    public IReadOnlyList<LegacyFullEnglishFirstAcousticCache> CopyCompletedChildren(){lock(gate)return Array.AsReadOnly(prefix.ToArray());}
    /// <summary>Started NN work reservations, including a canceled attempt.</summary>
    public int NetworkEvaluationCount=>budget.NetworkCount;
    public long LogicalSourcePayloadEnvelopeBytes=>LegacyFullEnglishFirstBudget.SourceEnvelope(4096,8192,256);
    public long LogicalRetainedCacheEnvelopeBytes=>budget.CurrentCacheEnvelope;
    /// <summary>One sequential bounded complete acoustic pass. Historical completed
    /// child caches remain readable after failure; no partial result is accepted.</summary>
    public void Run(LegacyCursiveSeparationTable table,CancellationToken token=default)=>RunCore(table,null,null,token);
    internal void RunPrepared(LegacyCursiveSeparationTable table,Action<string> checkpoint,CancellationToken token)=>RunCore(table,checkpoint,null,token);
    internal void RunDiagnostic(LegacyCursiveSeparationTable table,Action<LegacyFullEnglishFirstCandidates,int,int[]> observer,CancellationToken token=default)=>RunCore(table,null,observer,token);
    internal void RequireConfidencePristine(){lock(gate)if(entered)throw new InvalidOperationException("Pristine full parent required.");}
    internal void RunWithConfidence(LegacyCursiveSeparationTable table,LegacyFullEnglishFirstController confidence,Action<string>? checkpoint,CancellationToken token)=>RunCore(table,checkpoint,null,token,confidence);
    private void RunCore(LegacyCursiveSeparationTable table,Action<string>? checkpoint,Action<LegacyFullEnglishFirstCandidates,int,int[]>? neuralObserver,CancellationToken token,LegacyFullEnglishFirstController? confidence=null)
    {
        ArgumentNullException.ThrowIfNull(table);token.ThrowIfCancellationRequested();
        table=SnapshotTable(table,token);
        lock(gate){token.ThrowIfCancellationRequested();if(entered)throw new InvalidOperationException("Full FIRST parent is one-use.");confidence?.Enter(this);entered=true;}
        try
        {
            checkpoint?.Invoke("run:entry");token.ThrowIfCancellationRequested();
            foreach(var child in children)
            {
                token.ThrowIfCancellationRequested();var candidates=LegacyFullEnglishFirstCandidates.Prepare(child,table,budget,token);
                checkpoint?.Invoke("run:candidates");token.ThrowIfCancellationRequested();
                var cache=LegacyFullEnglishFirstAcousticCache.Prepare(candidates,budget,token);
                foreach(int group in candidates.CopyLiveGroupIds())cache.Compute(group,token,checkpoint,neuralObserver is null?null:costs=>neuralObserver(candidates,group,costs),confidence);
                checkpoint?.Invoke("run:child");token.ThrowIfCancellationRequested();lock(gate)prefix.Add(cache);
            }
            token.ThrowIfCancellationRequested();lock(gate)completed=true;
        }
        catch{lock(gate)faulted=true;throw;}
    }
    //No source720 publication or parent finalization belongs to this endpoint.
    internal static InkStroke[] Copy(IReadOnlyList<InkStroke> source)=>source.Select(s=>new InkStroke(s.Points.ToArray())).ToArray();
    private static InkStroke[] Snapshot(IReadOnlyList<InkStroke> source,CancellationToken token)
    {
        int count=source.Count;if(count is<1 or>256)throw new ArgumentException("Actual1..256 raw physical strokes required.");
        var points=new IReadOnlyList<InkPoint>[count];var sizes=new int[count];int total=0;
        for(int s=0;s<count;s++)
        {
            token.ThrowIfCancellationRequested();var p=source[s]?.Points;int n=p?.Count??0;
            if(n<1||n>4096-total)throw new ArgumentException("At most4096 aggregate raw points required before copying.");
            total+=n;points[s]=p!;sizes[s]=n;
            for(int i=0;i<n;i++){token.ThrowIfCancellationRequested();if(!Finite(p![i]))throw new ArgumentException("Finite bounded ordinary coordinates required.");}
        }
        var raw=new InkStroke[count];
        for(int s=0;s<count;s++)
        {
            if(points[s].Count!=sizes[s])throw new ArgumentException("Raw input changed during snapshot.");var copy=new InkPoint[sizes[s]];
            for(int p=0;p<copy.Length;p++){token.ThrowIfCancellationRequested();copy[p]=points[s][p];if(!Finite(copy[p]))throw new ArgumentException("Finite snapshot required.");}
            raw[s]=new(Array.AsReadOnly(copy));
        }
        return raw;
    }
    private static LegacyCursiveSeparationTable SnapshotTable(LegacyCursiveSeparationTable table,CancellationToken token)
    {
        var values=table.Values.ToArray();if(values.Length!=256)throw new ArgumentException("Actual shared256-float table required.");
        using var bytes=new MemoryStream();using(var writer=new Utf8JsonWriter(bytes))
        {
            writer.WriteStartObject();writer.WriteNumber("schemaVersion",1);writer.WriteString("profile","legacy-ru-lite-separation-v1");writer.WriteString("sha256","b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf");writer.WriteStartArray("values");
            foreach(float value in values){token.ThrowIfCancellationRequested();if(!float.IsFinite(value))throw new ArgumentException("Finite actual shared-table snapshot required.");writer.WriteNumberValue(value);}writer.WriteEndArray();writer.WriteEndObject();
        }
        //Existing loader validates every native float bit. A private new table
        //prevents the caller's public ReadOnlyMemory backing array changing
        //the active pass after this before-entry preflight.
        return LegacyCursiveSeparationTable.Load(bytes.ToArray(),token);
    }
    private static bool Finite(InkPoint p)=>float.IsFinite(p.X)&&float.IsFinite(p.Y)&&MathF.Abs(p.X)<=1_000_000&&MathF.Abs(p.Y)<=1_000_000;
    private static bool Flat(InkStroke stroke,double scale,CancellationToken token)
    {
        float x0=float.MaxValue,x1=-float.MaxValue,y0=x0,y1=x1;
        foreach(var p in stroke.Points){token.ThrowIfCancellationRequested();x0=MathF.Min(x0,p.X);x1=MathF.Max(x1,p.X);y0=MathF.Min(y0,p.Y);y1=MathF.Max(y1,p.Y);}
        float w=x1-x0,h=y1-y0;return(double)w>3*scale&&(double)(h/w)<.02;
    }
    private static int I(byte[] b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan(at));
    private static void CheckHash(ReadOnlySpan<byte> bytes,int length,string hash)
    {if(bytes.Length!=length||!Convert.ToHexString(SHA256.HashData(bytes)).Equals(hash,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Exact retained EN/provider wire envelope required.");}
}
