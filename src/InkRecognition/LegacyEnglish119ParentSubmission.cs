using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglish119Parent.Probe")]
namespace InkRecognition;

/// <summary>One actual fresh EN main descriptor's pre-NN filtered submission.
/// Map ordinals address retained current-parent strokes, not original raw IDs.</summary>
public sealed class LegacyEnglish119ParentChild
{
    public LegacyEnglish119ParentSubmission Owner{get;}
    public int DescriptorOrdinal{get;}
    public LegacyEnglish119SourceSubmission Submission{get;}
    public bool IsSoleFlat{get;}
    private readonly int[] map;
    internal IReadOnlyList<int> Map=>map;
    internal LegacyEnglish119ParentChild(LegacyEnglish119ParentSubmission owner,int ordinal,int[] actualMap,bool soleFlat,CancellationToken token)
    {
        Owner=owner;DescriptorOrdinal=ordinal;map=actualMap;IsSoleFlat=soleFlat;
        Submission=LegacyEnglish119SourceSubmission.Prepare(owner.Model,map.Select(i=>owner.Current[i]).ToArray(),token);
    }
    public int[] CopyCurrentParentMap()=>map.ToArray();
}

/// <summary>Fresh ordinary EN247/110 main ingress through owned scale/2E3
/// split/reorder and157C60 submissions. Stops at first249: no24B+, main119,
/// page/prior/property or document acceptance is implied.</summary>
public sealed partial class LegacyEnglish119ParentSubmission
{
    private const string MainHash="ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657";
    private readonly object gate=new();
    private readonly InkStroke[] raw;
    private readonly LegacyFreshOrdinaryRawPacket packet;
    private readonly LegacyFreshPhysicalBuffer split,current;
    private readonly LegacyEnglish119ParentChild[] children;
    private readonly LegacyFreshMainPhysicalReceiverCall[] calls;
    private readonly LegacyUnknownLineGroups groups;
    private readonly float[] samples;
    private readonly byte[] provider,early2D0;
    private readonly List<LegacyFirstResultSourceRecord> prefix=[];
    private readonly List<LegacyEnglish119ParentRowOrigin> origins=[];
    private byte[][] publishedDescriptors;
    private bool entered,faulted,completed;
    public LegacyFirstAcoustic119WireModel Model{get;}
    public float PhysicalResolution{get;}
    public LegacyLineScales SourceScales{get;}
    public float InitialNeuralScale{get;}
    public bool ParentScaleOverride=>false;
    public IReadOnlyList<LegacyEnglish119ParentChild> Children{get;}
    public bool IsFaulted{get{lock(gate)return faulted;}}
    public bool IsComplete{get{lock(gate)return completed;}}
    internal IReadOnlyList<InkStroke> Current=>current.Strokes;
    private LegacyEnglish119ParentSubmission(LegacyFirstAcoustic119WireModel model,InkStroke[] raw,float resolution,
        LegacyFreshOrdinaryRawPacket packet,LegacyOrdinaryLineScaleSnapshot scale,LegacyOrdinaryNeuralLineGrouping grouping,
        LegacyFreshPhysicalBuffer split,LegacyFreshPhysicalOrder reordered,byte[] provider,byte[] early2D0,CancellationToken token)
    {
        Model=model;this.raw=raw;PhysicalResolution=resolution;this.packet=packet;SourceScales=scale.Scales;
        samples=scale.Samples.ToArray();groups=grouping.Selected;InitialNeuralScale=grouping.NeuralScale;
        this.split=split;current=reordered.Buffer;calls=reordered.Calls;this.provider=provider;this.early2D0=early2D0;
        publishedDescriptors=current.Descriptors.Select(d=>d.ToArray()).ToArray();
        var list=new List<LegacyEnglish119ParentChild>();
        for(int d=0;d<current.Descriptors.Length;d++)
        {
            token.ThrowIfCancellationRequested();var descriptor=current.Descriptors[d];if((descriptor[0]&15)==3)continue;
            int first=I(descriptor,36),last=I(descriptor,40);
            int[] map=LegacyFirstResultSourceRecord.SelectChildPhysicalIndices(current.Strokes,first,last,0,SourceScales.Primary,token);
            bool soleFlat=first==last&&Flat(current.Strokes[first],SourceScales.Primary,token);
            list.Add(new(this,d,map,soleFlat,token));
        }
        children=list.ToArray();Children=Array.AsReadOnly(children);
    }
    public static LegacyEnglish119ParentSubmission Prepare(LegacyFirstAcoustic119WireModel actualSelectedModel,
        IReadOnlyList<InkStroke> exactMainRaw,float actualPhysicalResolution,
        ReadOnlySpan<byte> spatial30Resource,ReadOnlySpan<byte> physical50Resource,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualSelectedModel);ArgumentNullException.ThrowIfNull(exactMainRaw);token.ThrowIfCancellationRequested();
        var parent=actualSelectedModel.ParentSource;
        if(parent.Offset!=0||parent.Version!=0x110||parent.Base134Version!=0x300||parent.SourceKind!=0x135||parent.CoordinateMode!=2||
            !parent.Fingerprint.Equals(MainHash,StringComparison.OrdinalIgnoreCase)||
            actualSelectedModel.SelectedProcessor.Offset!=8827||actualSelectedModel.Source.Offset!=8843||
            actualSelectedModel.SourceScoreExponent!=.75f||!ReferenceEquals(actualSelectedModel.SelectedProcessor.Owner,parent))
            throw new ArgumentException("Actual selected EN lite main247/110 and its own first249 required.");
        if(!float.IsFinite(actualPhysicalResolution)||actualPhysicalResolution<0||actualPhysicalResolution>1_000_000)
            throw new ArgumentException("Mandatory finite actual source resolution required.");
        CheckHash(spatial30Resource,85812,"10f4b43cd02873f1995f543dece70e70c1f22cbdb7dcbf63e1c5c205966ac70e");
        CheckHash(physical50Resource,161332,"01f9fd0bf4b5c0c3c1acc170ee82938e24a132696acbfbcfb34573a9d0550c7e");
        byte[] Field(string name)=>parent.Fields.Single(f=>f.Owner=="parametersC0"&&f.Destination==name).CopyBytes();
        if(!new[]{"20","21","legacyThirdPresence","22"}.Select(n=>Field(n)[0]).SequenceEqual(new byte[]{0,1,1,1})||
            BinaryPrimitives.ReadSingleLittleEndian(Field("24"))!=.25f)
            throw new InvalidDataException("Actual EN early source flags and tolerance required.");
        var provider=parent.CopyChildEnvelopeBytes(parent.Children.Single(c=>c.Destination=="parametersC0.30"),token);
        var early=parent.CopyChildEnvelopeBytes(parent.Children.Single(c=>c.Destination=="parametersC0.38"),token);
        CheckHash(provider,135,"319b7934037964b875a6f76739f4b336de0d36e2f7f449b991c8088fb3c3e38a");
        CheckHash(early,693,"1bf9e83966098a4e611ae2ed6a53a43c30e270b8ced586260ec6c1e0775fd7bd");
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
        token.ThrowIfCancellationRequested();return new(actualSelectedModel,raw,actualPhysicalResolution,packet,scale,grouping,split,reordered,provider,early,token);
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
    public IReadOnlyList<LegacyFirstResultSourceRecord> CopyRowPrefix(){lock(gate)return Array.AsReadOnly(prefix.ToArray());}
    public IReadOnlyList<LegacyEnglish119ParentRowOrigin> CopyOriginPrefix(){lock(gate)return Array.AsReadOnly(origins.ToArray());}
    //Publication and typed preflight are in a separate NEW companion file.
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

