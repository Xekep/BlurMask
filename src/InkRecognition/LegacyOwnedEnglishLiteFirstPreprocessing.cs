using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyEnglishLiteFirstPreprocessing.Probe")]

namespace InkRecognition;

/// <summary>Fresh ordinary English-lite first-source24A, the unchanged24C
/// branch, and265. Retains the selected119/source247 owners. No feature vector,
/// neural evaluation, candidate graph or word decoder is constructed.</summary>
public sealed class LegacyOwnedEnglishLiteFirstPreprocessing
{
    public LegacyFirstAcoustic119WireModel Model {get;}
    public LegacySource247WireLayout Source=>Model.Source;
    public LegacySource247WireChild GeometryChild {get;}
    public LegacySource247WireChild SlantChild {get;}
    public LegacySource247WireChild SmoothingChild {get;}
    public LegacyAutomaticLineProfile GeometryProfile {get;}
    public LegacyOrientationSmoothingProfile SmoothingProfile {get;}
    public float MinimumSlantRadians {get;}
    public float MaximumSlantRadians {get;}
    public LegacyAutomaticLineEstimate Estimate {get;}
    public LegacyLinePair Line=>frame.Line;
    public LegacyOrientationStrokeMetrics SourceMetrics {get;}
    public float SmoothingWeight=>prepared.SmoothingWeight;
    public int SlantDirectionCount {get;}
    private readonly LegacyOrdinarySourceInk original;
    private readonly LegacyDeskewedLine frame;
    private readonly LegacyOrientationStrokePreparationWithKeysResult prepared;
    private readonly LegacySourcePointChronology chronology;

    private LegacyOwnedEnglishLiteFirstPreprocessing(LegacyFirstAcoustic119WireModel model,
        LegacySource247WireChild[] children,LegacyAutomaticLineProfile geometry,
        LegacyOrientationSmoothingProfile smoothing,float minimum,float maximum,
        LegacyOrdinarySourceInk original,LegacyAutomaticLineEstimate estimate,LegacyDeskewedLine frame,
        LegacyOrientationStrokeMetrics metrics,int count,LegacyOrientationStrokePreparationWithKeysResult prepared,
        LegacySourcePointChronology chronology)
    {Model=model;GeometryChild=children[0];SlantChild=children[1];SmoothingChild=children[2];
        GeometryProfile=geometry;SmoothingProfile=smoothing;MinimumSlantRadians=minimum;MaximumSlantRadians=maximum;
        this.original=original;Estimate=estimate;this.frame=frame;SourceMetrics=metrics;SlantDirectionCount=count;
        this.prepared=prepared;this.chronology=chronology;}

    public static LegacyOwnedEnglishLiteFirstPreprocessing Prepare(LegacyFirstAcoustic119WireModel model,
        IReadOnlyList<InkStroke> raw,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(model);token.ThrowIfCancellationRequested();var source=model.Source;
        if(source.SourceKind!=0x135||source.Version!=0x110)
            throw new NotSupportedException("Actual ordinary247/110 first source required.");
        foreach(string f in new[]{"20","21","legacyThirdPresence","22"})
            if(Field(source.Fields,"parametersC0",f)[0]!=0)
                throw new NotSupportedException("Early source grouping/orientation processors remain outside this fresh-row scope.");
        //The full cursive source has additional profile features. This owner
        //does not silently discard that branch while presenting a lite path.
        if(BinaryPrimitives.ReadUInt16LittleEndian(Field(model.Fields,"profile0","28"))!=0||
            new[]{"2C","30","34","38","3C","40","44"}.Any(f=>
                BinaryPrimitives.ReadSingleLittleEndian(Field(model.Fields,"profile0",f))!=0))
            throw new NotSupportedException("Only the lite first profile without the additional feature settings is supported.");
        var children=source.Children.Where(c=>c.Destination.StartsWith("parametersC0.58[",StringComparison.Ordinal)).ToArray();
        if(children.Length!=3||children[0].Kind!=0x24a||children[1].Kind!=0x24c||children[2].Kind!=0x265)
            throw new NotSupportedException("Exact ordered24A/24C/265 source processors required.");
        var geometry=LegacyAutomaticLineProfile.ReadResourceNode(source.CopyChildEnvelopeBytes(children[0],token),0);
        var smoothing=LegacyOrientationSmoothingProfile.ReadV102Node(source.CopyChildEnvelopeBytes(children[2],token),0);
        byte[] slant=source.CopyChildEnvelopeBytes(children[1],token);
        if(slant.Length!=24||children[1].Version!=0x100)
            throw new NotSupportedException("Actual24C/100 two-angle wire profile required.");
        float minimum=BinaryPrimitives.ReadSingleLittleEndian(slant.AsSpan(16)),maximum=BinaryPrimitives.ReadSingleLittleEndian(slant.AsSpan(20));
        if(!float.IsFinite(minimum)||!float.IsFinite(maximum)||minimum>maximum)
            throw new InvalidDataException("Invalid serialized24C radians.");
        var profile=new LegacyOrdinarySourceProfile(source.SourceKind,Field(source.Fields,"parametersC0","40")[0]!=0,
            source.ParametersOffset,source.EnclosureFlagOffset);
        var original=LegacyOrdinarySourceInk.Prepare(raw,profile,token);
        var metrics=LegacySourceOrientationPreparation.ExtractMetrics(original,token);
        var points=original.Strokes.Select(s=>s.Points.ToArray()).ToArray();
        var estimate=LegacyAutomaticLineEstimator.Estimate(points,geometry,token);
        //NEW/no-prior/no-property source:16A130 scale168=0, descriptor14=0.
        //16E850 assigns descriptor+10, not167210 physical1C+4. Physical
        //assignment remains-1 through24A,169750 and16E570 (static audit).
        var frame=LegacyLineDeskew.Apply(points,estimate.Line,(float)estimate.Slope,token);
        int count=CountFreshUnassignedDirections(frame.Strokes,token);
        if(count>=20)throw new NotSupportedException("Positive24C geometry (direction count>=20) remains unported;265 has not run.");
        var prepared=LegacyOrientationStrokePreparation.PrepareWithPointKeys(frame.Strokes.Select(p=>new InkStroke(p)).ToArray(),
            smoothing,frame.Line.Height,metrics,token);
        var keys=new List<LegacySourcePointKey>();
        for(int s=0;s<original.Strokes.Count;s++)for(int p=0;p<original.Strokes[s].Points.Count;p++)
            keys.Add(original.Chronology.Resolve(s,p).Key);
        var chronology=LegacySourcePointChronology.CreatePrepared(prepared.PhysicalPointCounts.Span,keys.ToArray(),
            Enumerable.Range(0,keys.Count).ToArray(),prepared.PointKeys200.ToArray().Select(k=>checked((uint)k)).ToArray(),200);
        token.ThrowIfCancellationRequested();
        return new(model,children,geometry,smoothing,minimum,maximum,original,estimate,frame,metrics,count,prepared,chronology);
    }

    private static byte[] Field(IReadOnlyList<LegacyResourceWireField> fields,string owner,string destination)=>
        fields.Single(f=>f.Owner==owner&&f.Destination==destination).CopyBytes();

    //176A10 fresh physical1C+4=-1 branch. Kept internal: arbitrary caller
    //assignment flags are not an alternative public source factory.
    internal static int CountFreshUnassignedDirections(IReadOnlyList<InkPoint[]> strokes,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(strokes);token.ThrowIfCancellationRequested();
        if(strokes.Count is <1 or >256)throw new ArgumentException("Expected bounded physical strokes.");
        int count=0,total=0;
        foreach(var points in strokes)
        {
            if(points is null||points.Length==0||points.Length>4096-total)throw new ArgumentException("Expected1..4096 points.");
            total+=points.Length;int state=0;
            for(int i=0;i<points.Length;i++)
            {
                token.ThrowIfCancellationRequested();
                if(!float.IsFinite(points[i].X)||!float.IsFinite(points[i].Y))throw new ArgumentException("Nonfinite point.");
                if(i==0)continue;
                if(points[i].Y<points[i-1].Y){if(state<=0)count++;state=1;}
                else if(points[i].Y>points[i-1].Y){if(state>=0)count++;state=-1;}
            }
            if(state!=0)count++;
        }
        return count;
    }

    public InkStroke[] CopyOriginalStrokes()=>original.Strokes.Select(s=>new InkStroke(s.Points.ToArray())).ToArray();
    public InkStroke[] CopyDeskewedStrokes()=>frame.Strokes.Select(p=>new InkStroke(p.ToArray())).ToArray();
    public InkStroke[] CopyPreparedStrokes()=>prepared.Strokes.Select(s=>new InkStroke(s.Points.ToArray())).ToArray();
    public int[] CopyPointKeys200()=>prepared.PointKeys200.ToArray();
    public int[] CopyPhysicalPointCounts()=>prepared.PhysicalPointCounts.ToArray();
    public float[] CopySourceProbabilities()=>original.SourceStrokeProbabilities.ToArray();
    public LegacySourcePointIdentity ResolvePreparedPoint(int physical,int point)=>chronology.Resolve(physical,point);
}
