using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyEnglishFirstSlant.Probe")]

namespace InkRecognition;

/// <summary>Prepared176A10 fresh-unassigned single-kind0-descriptor arithmetic.
/// The supplied points are explicitly prepared; Apply does not establish their
/// source preprocessing provenance. The owned full-preprocessing factory does.</summary>
public sealed class LegacyPreparedEnglishFirstSlant
{
    public LegacySource247WireLayout? Source {get;}
    public LegacySource247WireChild? Processor {get;}
    public int DirectionCount {get;}
    public bool Applied=>DirectionCount>=20;
    public double AcceptedLength {get;}
    public double WeightedAngleSum {get;}
    public double Angle {get;}
    private readonly InkStroke[] strokes;
    private LegacyPreparedEnglishFirstSlant(LegacySource247WireLayout? source,LegacySource247WireChild? processor,
        InkStroke[] strokes,int count,double length,double weighted,double angle)
    {Source=source;Processor=processor;this.strokes=strokes;DirectionCount=count;AcceptedLength=length;WeightedAngleSum=weighted;Angle=angle;}

    public static LegacyPreparedEnglishFirstSlant Apply(LegacySource247WireLayout source,
        LegacySource247WireChild processor,IReadOnlyList<InkStroke> deskewed,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(source);source.ValidateChild(processor,token);
        byte[] bytes=source.CopyChildEnvelopeBytes(processor,token);
        if(processor.Kind!=0x24c||processor.Version!=0x100||bytes.Length!=24)
            throw new NotSupportedException("Owned24C/100 two-angle wire profile required.");
        return ApplyCore(source,processor,deskewed,BinaryPrimitives.ReadSingleLittleEndian(bytes.AsSpan(16)),
            BinaryPrimitives.ReadSingleLittleEndian(bytes.AsSpan(20)),token);
    }

    internal static LegacyPreparedEnglishFirstSlant ApplyPreparedAngles(IReadOnlyList<InkStroke> points,
        float minimum,float maximum,CancellationToken token=default)=>ApplyCore(null,null,points,minimum,maximum,token);

    private static LegacyPreparedEnglishFirstSlant ApplyCore(LegacySource247WireLayout? source,
        LegacySource247WireChild? processor,IReadOnlyList<InkStroke> input,float minimum,float maximum,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(input);token.ThrowIfCancellationRequested();
        if(!float.IsFinite(minimum)||!float.IsFinite(maximum)||minimum>maximum)throw new ArgumentException("Finite ordered24C radians required.");
        if(input.Count is <1 or >256)throw new ArgumentException("Expected1..256 physical strokes.");
        var points=new InkPoint[input.Count][];int total=0;
        for(int s=0;s<points.Length;s++)
        {
            var p=input[s]?.Points??throw new ArgumentException("Null physical stroke.");
            if(p.Count==0||p.Count>4096-total)throw new ArgumentException("Expected1..4096 total points.");
            total+=p.Count;points[s]=p.ToArray();
        }
        int count=LegacyOwnedEnglishLiteFirstPreprocessing.CountFreshUnassignedDirections(points,token);
        double length=0,weighted=0,angle=0;
        if(count>=20)
        {
            foreach(var p in points)for(int i=1;i<p.Length;i++)
            {
                token.ThrowIfCancellationRequested();float dy=p[i].Y-p[i-1].Y,dx=p[i].X-p[i-1].X;
                if(dy<0){dy=-dy;dx=-dx;}
                //Portable transcendental adaptation of24DD70's CPU dispatch.
                double a=Math.Atan2(dx,dy);
                if(a<minimum||a>maximum)continue;
                float squared=dy*dy+dx*dx;
                if(!float.IsFinite(squared))throw new InvalidDataException("Portable finite24C squared-length bound exceeded.");
                double weight=Math.Sqrt(squared);length+=weight;weighted+=weight*a;
            }
            angle=length==0?0:weighted/length;
            //The source matrix uses float cos/sin of the double weighted angle;
            //its yx coefficient is literal0, not sin(angle). No guide is written.
            float c=(float)Math.Cos(angle),s=(float)Math.Sin(angle);
            float minX=float.MaxValue,minY=float.MaxValue,maxX=-float.MaxValue,maxY=-float.MaxValue;
            foreach(var p in points)foreach(var v in p)
            {token.ThrowIfCancellationRequested();minX=MathF.Min(minX,v.X);minY=MathF.Min(minY,v.Y);maxX=MathF.Max(maxX,v.X);maxY=MathF.Max(maxY,v.Y);}
            float cx=(maxX+minX)*.5f,cy=(maxY+minY)*.5f;
            float tx=(cy*s-cx*c)+cx,ty=cy-cy*c;
            if(!float.IsFinite(tx)||!float.IsFinite(ty))throw new InvalidDataException("Portable finite24C center bound exceeded.");
            foreach(var p in points)for(int i=0;i<p.Length;i++)
            {
                token.ThrowIfCancellationRequested();var v=p[i];
                float x=(v.Y*-s+v.X*c)+tx,y=(v.Y*c+v.X*0f)+ty;
                if(!float.IsFinite(x)||!float.IsFinite(y))throw new InvalidDataException("Nonfinite24C output.");
                p[i]=new(x,y);
            }
        }
        token.ThrowIfCancellationRequested();
        return new(source,processor,points.Select(p=>new InkStroke(p)).ToArray(),count,length,weighted,angle);
    }
    public InkStroke[] CopyStrokes()=>strokes.Select(s=>new InkStroke(s.Points.ToArray())).ToArray();
}
