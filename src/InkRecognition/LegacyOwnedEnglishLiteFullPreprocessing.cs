using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Actual selected ENlite119/source247 fresh raw packet through
///24A, all24C directions and265. No features, activation factory, NN or graph.</summary>
public sealed class LegacyOwnedEnglishLiteFullPreprocessing
{
    public LegacyFirstAcoustic119WireModel Model {get;}
    public LegacySource247WireLayout Source=>Model.Source;
    public LegacySource247WireChild GeometryChild {get;}
    public LegacySource247WireChild SmoothingChild {get;}
    public LegacyPreparedEnglishFirstSlant Slant {get;}
    public LegacyAutomaticLineEstimate Estimate {get;}
    public LegacyLinePair Line {get;}
    public LegacyOrientationStrokeMetrics SourceMetrics {get;}
    public float SmoothingWeight {get;}
    /// <summary>Ingress snapshot before24A assigns descriptor+10.</summary>
    public LegacyOrdinaryRawInputDescriptor RawDescriptor {get;}
    private readonly LegacyOrdinarySourceInk original;
    private readonly InkStroke[] deskewed,prepared;
    private readonly int[] keys,counts;
    private readonly LegacySourcePointChronology chronology;
    private LegacyOwnedEnglishLiteFullPreprocessing(LegacyFirstAcoustic119WireModel model,
        LegacySource247WireChild geometry,LegacySource247WireChild smoothing,LegacyFreshOrdinaryRawPacket packet,
        LegacyAutomaticLineEstimate estimate,LegacyDeskewedLine frame,LegacyOrientationStrokeMetrics metrics,
        LegacyPreparedEnglishFirstSlant slant,LegacyOrientationStrokePreparationWithKeysResult prepared,
        LegacySourcePointChronology chronology)
    {Model=model;GeometryChild=geometry;SmoothingChild=smoothing;original=packet.Source;RawDescriptor=packet.Descriptor;
        Estimate=estimate;Line=frame.Line;SourceMetrics=metrics;Slant=slant;SmoothingWeight=prepared.SmoothingWeight;
        deskewed=frame.Strokes.Select(p=>new InkStroke(p)).ToArray();this.prepared=prepared.Strokes.ToArray();
        keys=prepared.PointKeys200.ToArray();counts=prepared.PhysicalPointCounts.ToArray();this.chronology=chronology;}

    public static LegacyOwnedEnglishLiteFullPreprocessing Prepare(LegacyFirstAcoustic119WireModel model,
        IReadOnlyList<InkStroke> rawRow,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(model);token.ThrowIfCancellationRequested();var source=model.Source;
        byte[] Field(string owner,string name)=>source.Fields.Single(f=>f.Owner==owner&&f.Destination==name).CopyBytes();
        if(source.Version!=0x110||source.SourceKind!=0x135||
            new[]{"20","21","legacyThirdPresence","22"}.Any(f=>Field("parametersC0",f)[0]!=0))
            throw new NotSupportedException("Selected ordinary247/110 with no early processors required.");
        if(BinaryPrimitives.ReadUInt16LittleEndian(model.Fields.Single(f=>f.Owner=="profile0"&&f.Destination=="28").CopyBytes())!=0||
            new[]{"2C","30","34","38","3C","40","44"}.Any(name=>BinaryPrimitives.ReadSingleLittleEndian(model.Fields.Single(f=>f.Owner=="profile0"&&f.Destination==name).CopyBytes())!=0))
            throw new NotSupportedException("Only lite first feature settings are supported.");
        var children=source.Children.Where(c=>c.Destination.StartsWith("parametersC0.58[",StringComparison.Ordinal)).ToArray();
        if(children.Length!=3||children[0].Kind!=0x24a||children[1].Kind!=0x24c||children[2].Kind!=0x265)
            throw new NotSupportedException("Ordered actual24A/24C/265 required.");
        var geometry=LegacyAutomaticLineProfile.ReadResourceNode(source.CopyChildEnvelopeBytes(children[0],token),0);
        var smoothing=LegacyOrientationSmoothingProfile.ReadV102Node(source.CopyChildEnvelopeBytes(children[2],token),0);
        var profile=new LegacyOrdinarySourceProfile(source.SourceKind,Field("parametersC0","40")[0]!=0,source.ParametersOffset,source.EnclosureFlagOffset);
        var packet=LegacyFreshOrdinaryRawPacket.PrepareDirect(rawRow,profile,token);
        if(packet.DescriptorCount!=1||(packet.Descriptor.Flags&15)!=0||packet.Descriptor.FirstPhysicalStroke!=0||packet.Descriptor.LastPhysicalStroke!=packet.PhysicalStrokeCount-1)
            throw new InvalidDataException("Fresh unscoped ordinary descriptor membership changed.");
        var metrics=LegacySourceOrientationPreparation.ExtractMetrics(packet.Source,token);
        var points=packet.Source.Strokes.Select(s=>s.Points.ToArray()).ToArray();
        var estimate=LegacyAutomaticLineEstimator.Estimate(points,geometry,token);
        var frame=LegacyLineDeskew.Apply(points,estimate.Line,(float)estimate.Slope,token);
        var slant=LegacyPreparedEnglishFirstSlant.Apply(source,children[1],frame.Strokes.Select(p=>new InkStroke(p)).ToArray(),token);
        //176A10 only changes XY.265 keeps the guide0/4 written by24A and
        //the original15ABE0 metrics; it does not fit a new slant-transformed band.
        var prepared=LegacyOrientationStrokePreparation.PrepareWithPointKeys(slant.CopyStrokes(),smoothing,frame.Line.Height,metrics,token);
        var rawKeys=new List<LegacySourcePointKey>();
        for(int s=0;s<packet.PhysicalStrokeCount;s++)for(int p=0;p<packet.Source.Strokes[s].Points.Count;p++)rawKeys.Add(packet.Source.Chronology.Resolve(s,p).Key);
        var chronology=LegacySourcePointChronology.CreatePrepared(prepared.PhysicalPointCounts.Span,rawKeys.ToArray(),
            Enumerable.Range(0,rawKeys.Count).ToArray(),prepared.PointKeys200.ToArray().Select(k=>checked((uint)k)).ToArray(),200);
        token.ThrowIfCancellationRequested();return new(model,children[0],children[2],packet,estimate,frame,metrics,slant,prepared,chronology);
    }
    public InkStroke[] CopyOriginalStrokes()=>original.Strokes.Select(s=>new InkStroke(s.Points.ToArray())).ToArray();
    public InkStroke[] CopyDeskewedStrokes()=>deskewed.Select(s=>new InkStroke(s.Points.ToArray())).ToArray();
    public InkStroke[] CopyPreparedStrokes()=>prepared.Select(s=>new InkStroke(s.Points.ToArray())).ToArray();
    public int[] CopyPointKeys200()=>keys.ToArray();
    public int[] CopyPhysicalPointCounts()=>counts.ToArray();
    public LegacySourcePointIdentity ResolvePreparedPoint(int physical,int point)=>chronology.Resolve(physical,point);
}
