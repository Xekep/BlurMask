using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

[assembly: InternalsVisibleTo("LegacyMainDescriptorCuts.Probe")]

namespace InkRecognition;

/// <summary>One193380/191950 replacement or kind3 pending-control event.
/// Selected geometry records keep global physical and pre-replacement point
/// indices. They are not a concatenated whole-source prepared-record buffer.</summary>
public sealed class LegacyMainDescriptorCutStep
{
    private readonly byte[] descriptor;
    private readonly LegacyCursivePreparedPoint[] points;
    private readonly LegacyCursiveCutMarker[] markers;
    private readonly LegacyCursiveInkAtom[] atoms;
    private readonly int[] beforeCounts,afterCounts,pending;
    private readonly uint[] incoming,composed;
    public int DescriptorOrdinal {get;}
    public int Kind {get;}
    public int LineId {get;}
    public int FirstPhysical {get;}
    public int LastPhysical {get;}
    public float Baseline {get;}
    public float Height {get;}
    public bool IsPendingSeparator=>Kind==3;
    internal LegacyMainDescriptorCutStep(int ordinal,byte[] descriptor,int line,int first,int last,
        float baseline,float height,LegacyCursivePreparedPoint[] points,LegacyCursiveCutMarker[] markers,
        LegacyCursiveInkAtom[] atoms,int[] before,int[] after,uint[] incoming,uint[] composed,int[] pending)
    {DescriptorOrdinal=ordinal;this.descriptor=descriptor;Kind=BinaryPrimitives.ReadUInt16LittleEndian(descriptor)&15;
        LineId=line;FirstPhysical=first;LastPhysical=last;Baseline=baseline;Height=height;this.points=points;
        this.markers=markers;this.atoms=atoms;beforeCounts=before;afterCounts=after;this.incoming=incoming;
        this.composed=composed;this.pending=pending;}
    public byte[] CopyDescriptor()=>descriptor.ToArray();
    public LegacyCursivePreparedPoint[] CopySelectedPreparedPoints()=>points.ToArray();
    public LegacyCursiveCutMarker[] CopyMarkers()=>markers.ToArray();
    public LegacyCursiveInkAtom[] CopyAtoms()=>atoms.ToArray();
    public int[] CopyInputPointCounts()=>beforeCounts.ToArray();
    public int[] CopyOutputPointCounts()=>afterCounts.ToArray();
    public uint[] CopyIncomingPointMap()=>incoming.ToArray();
    public uint[] CopyComposedPointMap()=>composed.ToArray();
    /// <summary>Actual kind3 descriptor ordinals queued before this ordinary
    /// step. Each contributes pending flag1 and controlFFFFFFF9. Assignment to
    /// native graph context records is a later graph producer's responsibility.</summary>
    public int[] CopyPendingDescriptorOrdinals()=>pending.ToArray();
}

/// <summary>Fresh main0 multiple-descriptor cuts over ONE changing source218
/// XY/map domain. Owns actual24B/24D/29B/265 input and derives resource flags,
/// guides, physical probabilities and marker limits. Does not build21B graphs,
/// assign global NN groups, classify glyphs or recognize multiline text.</summary>
public sealed class LegacyFreshMainDescriptorCutSource
{
    private const string AcousticHash="7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978";
    private readonly LegacyMainDescriptorCutSequence output;
    private readonly byte[][] descriptors,guides;
    public LegacyFreshMainActiveGapSmoothed Preparation {get;}
    public LegacySourceSamplerCategoryOwner CategoryOwner {get;}
    public LegacyCursiveLatticeSpanProfile SpanProfile {get;}
    public int GraphOffset=>189379;
    public int AcousticOffset=>189734;
    public string SourceFingerprint=>AcousticHash;
    public bool GraphConfigurationByte3=>false;
    public bool HasCategoryLimitObject=>false;
    public IReadOnlyList<LegacyMainDescriptorCutStep> Steps=>output.Steps;
    /// <summary>Defensive point-map/metadata snapshot; its identity is not a
    /// substitute for the retained Preparation owner.</summary>
    public LegacySourcePointChronology Chronology=>output.CopyChronology();
    private LegacyFreshMainDescriptorCutSource(LegacyFreshMainActiveGapSmoothed input,
        LegacySourceSamplerCategoryOwner categories,LegacyCursiveLatticeSpanProfile span,
        byte[][] descriptors,byte[][] guides,LegacyMainDescriptorCutSequence output)
    {Preparation=input;CategoryOwner=categories;SpanProfile=span;this.descriptors=descriptors;this.guides=guides;this.output=output;}

    public static LegacyFreshMainDescriptorCutSource Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainActiveGapSmoothed actualInput,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualInput);cancellationToken.ThrowIfCancellationRequested();
        var lines=actualInput.Input.Input.Input;var source=lines.Owner.Controller.Source;
        if(resource.Length!=572763||!Convert.ToHexString(SHA256.HashData(resource)).Equals(AcousticHash,StringComparison.OrdinalIgnoreCase)||
            !StringComparer.OrdinalIgnoreCase.Equals(lines.Owner.Input.Input.Input.SourceFingerprint,AcousticHash)||
            lines.Owner.Controller.SourceOffset!=0||source.Profile.SourceKind!=0x135)
            throw new ArgumentException("Exact fresh main0 source/resource lineage required.");
        var categories=LegacySourceSamplerCategoryOwner.ReadV10dSource(resource,0,cancellationToken);
        if(categories.SamplerNodeOffset!=188157||U16(resource,14)!=0x10d||U16(resource,188171)!=0x106)
            throw new NotSupportedException("Actual247/v10D and297/v106 cut binding required.");
        int tail=checked(189734+8+I(resource,189738));
        if(I(resource,tail)!=0||8+I(resource,4)-tail!=190||resource[188216]!=6||F(resource,188217)!=4)
            throw new NotSupportedException("Absent optional category owner and exact marker-limit profile required.");
        //150D00 frontend91 -> config[3] ->1A6980 graph1D8. The typed fresh
        //input has NEW no-property containerC4=0; source135 and actual absent
        //F0009 complete the remaining two native predicates, independently of
        //the optional wrapper flags and frontend23F/type selection.
        if(lines.Owner.Controller.Alphabet.LookupUtf8([0xf3,0xb0,0x80,0x89])!=ushort.MaxValue)
            throw new NotSupportedException("Fresh ordinary graph configuration byte3 must be clear.");
        var span=LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(resource,189379);
        if(span.MaximumAtoms!=8||resource[189728]!=1||resource[189729]!=0)
            throw new InvalidDataException("Actual main graph ownership/profile required.");
        var descriptors=lines.CopyDescriptors();var guides=lines.CopyGuides();
        var output=LegacyMainDescriptorCutSequence.Execute(actualInput.Strokes,actualInput.Chronology,
            source.SourceStrokeProbabilities.Span,descriptors,guides,cancellationToken);
        return new(actualInput,categories,span,descriptors,guides,output);
    }

    public IReadOnlyList<InkStroke> CopyStrokes()=>output.CopyStrokes();
    public byte[][] CopyDescriptors()=>descriptors.Select(d=>d.ToArray()).ToArray();
    public byte[][] CopyGuides()=>guides.Select(g=>g.ToArray()).ToArray();
    public int[] CopyTrailingPendingDescriptorOrdinals()=>output.CopyTrailingPending();
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private static ushort U16(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(b[at..]);
    private static float F(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadSingleLittleEndian(b[at..]);
}

/// <summary>Sequential selected-interval point replacement. The public owning
/// entry supplies every input; independent arithmetic controls share this kernel
/// through the friend assembly without a prepared public source constructor.</summary>
internal sealed class LegacyMainDescriptorCutSequence
{
    private readonly InkStroke[] strokes;
    private readonly int[] trailing;
    public IReadOnlyList<LegacyMainDescriptorCutStep> Steps {get;}
    public LegacySourcePointChronology Chronology {get;}
    private LegacyMainDescriptorCutSequence(InkStroke[] strokes,LegacySourcePointChronology chronology,
        LegacyMainDescriptorCutStep[] steps,int[] trailing)
    {this.strokes=strokes;Chronology=chronology;Steps=Array.AsReadOnly(steps);this.trailing=trailing;}
    public IReadOnlyList<InkStroke> CopyStrokes()=>Array.AsReadOnly(strokes.Select(s=>new InkStroke(Array.AsReadOnly(s.Points.ToArray()))).ToArray());
    public int[] CopyTrailingPending()=>trailing.ToArray();
    public LegacySourcePointChronology CopyChronology()
    {
        int[] counts=Chronology.StrokePointCounts.ToArray();
        return Chronology.RemapPreparedPoints(counts,Enumerable.Range(0,counts.Sum()).Select(i=>(uint)i).ToArray());
    }

    internal static LegacyMainDescriptorCutSequence Execute(IReadOnlyList<InkStroke> source,
        LegacySourcePointChronology chronology,ReadOnlySpan<float> sourceProbabilities,
        IReadOnlyList<byte[]> sourceDescriptors,IReadOnlyList<byte[]> sourceGuides,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(source);ArgumentNullException.ThrowIfNull(chronology);
        ArgumentNullException.ThrowIfNull(sourceDescriptors);ArgumentNullException.ThrowIfNull(sourceGuides);token.ThrowIfCancellationRequested();
        //Create validates finite coordinates and the total4096/physical256
        //budget, but its whole-buffer geometry is not used for interval cuts.
        var checkedInk=LegacyCursivePreparedInk.Create(source,token);
        int[] counts=checkedInk.StrokePointCounts.ToArray();
        if(!chronology.StrokePointCounts.Span.SequenceEqual(counts)||sourceProbabilities.Length!=counts.Length||
            sourceDescriptors.Count is <1 or >512||sourceGuides.Count>512)
            throw new ArgumentException("Bounded corresponding source points/chronology/descriptors/probabilities required.");
        float[] probabilities=sourceProbabilities.ToArray();foreach(float p in probabilities)
            if(!float.IsFinite(p))throw new ArgumentException("Finite per-physical source probabilities required.");
        byte[][] descriptors=sourceDescriptors.Select(d=>d?.ToArray()??throw new ArgumentException("Null descriptor.")).ToArray();
        byte[][] guides=sourceGuides.Select(g=>g?.ToArray()??throw new ArgumentException("Null guide.")).ToArray();
        int next=0;foreach(var d in descriptors)
        {
            token.ThrowIfCancellationRequested();if(d.Length!=88)throw new ArgumentException("Complete88-byte descriptors required.");
            int kind=BinaryPrimitives.ReadUInt16LittleEndian(d)&15;
            if(kind==3)
            {if(BinaryPrimitives.ReadInt64LittleEndian(d.AsSpan(32))!=-1)throw new NotSupportedException("Only kind3 pending-control descriptors are restored.");continue;}
            if(kind!=0)throw new NotSupportedException("Only assigned ordinary cuts and pending kind3 metadata are restored.");
            int first=I(d,36),last=I(d,40),line=I(d,16);
            if(first!=next||last<first||last>=counts.Length||line<0||line>=guides.Length||guides[line].Length!=40)
                throw new ArgumentException("Assigned guides and a complete ordered physical partition required.");
            float baseline=F(guides[line],0),height=F(guides[line],4);
            if(!float.IsFinite(baseline)||!float.IsFinite(height)||height<=0)throw new ArgumentException("Finite assigned positive source guide required.");
            next=last+1;
        }
        if(next!=counts.Length)throw new ArgumentException("Every physical stroke must belong to an ordinary descriptor.");
        var current=source.Select(s=>new InkStroke(Array.AsReadOnly(s.Points.ToArray()))).ToArray();
        //Own metadata snapshots before any replacement, including implicit-map
        //chronologies. This is native factor1 identity composition, not a reset.
        var identity=Enumerable.Range(0,counts.Sum()).Select(i=>(uint)i).ToArray();
        var currentChronology=chronology.RemapPreparedPoints(counts,identity,1,token);
        var steps=new List<LegacyMainDescriptorCutStep>(descriptors.Length);var pending=new List<int>();
        for(int ordinal=0;ordinal<descriptors.Length;ordinal++)
        {
            token.ThrowIfCancellationRequested();var d=descriptors[ordinal];int kind=BinaryPrimitives.ReadUInt16LittleEndian(d)&15;
            if(kind==3)
            {
                pending.Add(ordinal);steps.Add(new(ordinal,d,I(d,16),-1,-1,0,0,[],[],[],counts.ToArray(),counts.ToArray(),[],[],[]));continue;
            }
            int first=I(d,36),last=I(d,40),line=I(d,16),selectedCount=last-first+1;
            float baseline=F(guides[line],0),height=F(guides[line],4);
            //191950's temporary geometry contains ONLY this interval, with a
            //zero final delta. A whole-global geometry preparation here would
            //incorrectly include a segment into the untouched suffix.
            var selected=current.Skip(first).Take(selectedCount).ToArray();
            var result=LegacyCursiveCutProducer.GenerateRussianVersion106(selected,baseline,height,
                probabilities.AsSpan(first,selectedCount).ToArray(),false,null,token);
            int prefix=0;for(int i=0;i<first;i++)prefix+=counts[i];
            int selectedInput=0;for(int i=first;i<=last;i++)selectedInput+=counts[i];
            int total=counts.Sum(),selectedOutput=result.PreparedInk.Points.Length;
            if(selectedOutput>4096-(total-selectedInput))throw new ArgumentException("Shared cut source exceeds the portable4096-point budget.");
            var incoming=new uint[total-selectedInput+selectedOutput];for(int i=0;i<prefix;i++)incoming[i]=(uint)i;
            var prepared=result.PreparedInk.Points.ToArray();
            for(int i=0;i<prepared.Length;i++)
            {
                token.ThrowIfCancellationRequested();var p=prepared[i];
                incoming[prefix+i]=checked((uint)(prefix+p.OriginalPointIndex));
                prepared[i]=p with {StrokeIndex=p.StrokeIndex+first,OriginalPointIndex=checked(p.OriginalPointIndex+prefix)};
            }
            int suffixDestination=prefix+selectedOutput;
            for(int i=prefix+selectedInput;i<total;i++)incoming[suffixDestination++]=(uint)i;
            int[] before=counts.ToArray();var updated=result.PreparedInk.ToStrokes();
            for(int i=0;i<selectedCount;i++){current[first+i]=updated[i];counts[first+i]=updated[i].Points.Count;}
            currentChronology=currentChronology.RemapPreparedPoints(counts,incoming,1,token);
            var markers=result.Markers.Select(m=>m with{StrokeIndex=m.StrokeIndex+first}).ToArray();
            var atoms=result.Atoms.Select(a=>new LegacyCursiveInkAtom(a.Marker with{StrokeIndex=a.Marker.StrokeIndex+first},
                a.Range with{StrokeIndex=a.Range.StrokeIndex+first})).ToArray();
            steps.Add(new(ordinal,d,line,first,last,baseline,height,prepared,markers,atoms,before,counts.ToArray(),incoming,
                currentChronology.PreparedPointMap.ToArray(),pending.ToArray()));pending.Clear();
        }
        token.ThrowIfCancellationRequested();return new(current,currentChronology,steps.ToArray(),pending.ToArray());
    }
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private static float F(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadSingleLittleEndian(b[at..]);
}
