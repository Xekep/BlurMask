using System.Runtime.CompilerServices;
namespace InkRecognition;

public readonly record struct LegacyPhysicalTimingSourceSnapshot(int OriginalSourceIndex,int PointCount,
    LegacyPhysicalRawTimingKind Kind,float ExplicitFormatRateHz,int NativePeriodMicroseconds,
    long BaseTagMicroseconds,int? NativeTimeChannel,int? NativeChannelMultiplier);

internal sealed record LegacyPhysicalTimingSource(int Index,int Points,LegacyPhysicalRawTimingKind Kind,
    float Rate,int Period,long Base,int[]? Samples);

/// <summary>Typed C1C/C49 constructor semantics for one exact NEW raw root.
/// Fixed unit-ms leaf only, no generic unit grammar or SDK execution.</summary>
public sealed class LegacyPhysicalRawInkTiming
{
    //Leave a guard interval for native C4B's final +.5 conversion.
    public const long MaximumAbsoluteMicroseconds=(1L<<52)-2;
    private readonly LegacyPhysicalTimingSource[] sources;
    private readonly ConditionalWeakTable<LegacyGlobalOrdinaryCroppedSource,HashSet<LegacyGlobalOrdinaryCurveView>> memberships=new();
    public LegacyPhysicalRawInkIngress Ingress{get;}
    public LegacyOwnedRawInkDocument Document=>Ingress.Document;
    internal LegacyPhysicalRawInkTiming(LegacyPhysicalRawInkIngress ingress,LegacyPhysicalTimingSource[] sources)
    {Ingress=ingress;this.sources=sources;}

    internal static int NativePeriod(float rate)
    {
        if(!float.IsFinite(rate)||rate<=0)throw new ArgumentOutOfRangeException(nameof(rate));
        float period=1_000_000f/rate;period=period+.5f;
        if(!float.IsFinite(period)||period<1||period>=2147483648f)
            throw new NotSupportedException("Native signed32 positive sample period required.");
        return (int)period;
    }
    internal static void ValidateConstructor(LegacyPhysicalStrokeConstructor p,int count,CancellationToken token)
    {
        int period=NativePeriod(p.FormatRateHz);Bound(p.BaseTagMicroseconds);
        if(p.Kind==LegacyPhysicalRawTimingKind.Uniform)
        {
            if(p.Count!=0)throw new ArgumentException("Uniform constructor has no actual time channel.");
            Bound(checked(p.BaseTagMicroseconds+(long)period*(uint)(count-1)));return;
        }
        if(p.Kind!=LegacyPhysicalRawTimingKind.MeasuredMilliseconds||p.Count!=count)
            throw new ArgumentException("Exact same-source complete actual channel required.");
        int last=int.MinValue;
        for(int i=0;i<count;i++)
        {
            token.ThrowIfCancellationRequested();int sample=p.Sample(i);
            if(sample<last)throw new NotSupportedException("Monotone actual sample times required.");
            last=sample;Bound(checked(p.BaseTagMicroseconds+(long)sample*NativeMillisecondMultiplier()));
        }
    }
    internal static int NativeMillisecondMultiplier()
    {
        //B3A50("ms",1): factor1, time exponent1, time scale.001,
        //other six scales1. B4450 divides by target E7060 time scale1e-6.
        double factor=1;
        for(int dimension=0;dimension<7;dimension++)
            factor*=dimension==2?.001/1e-6:1;
        double coefficient=(factor*1)/1;
        return checked((int)coefficient);
    }
    private static void Bound(long value)
    {
        if(value< -MaximumAbsoluteMicroseconds||value>MaximumAbsoluteMicroseconds)
            throw new NotSupportedException("Bounded exact signed64 to double temporal domain required.");
    }
    public void Validate(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(Ingress.TimingReceipt,this)||!ReferenceEquals(Ingress.CurrentView.Document,Document)||
            Document.ObjectCount!=sources.Length||Ingress.CurrentView.Curves.Count!=sources.Length)
            throw new InvalidOperationException("Exact physical raw ingress identity required.");
        for(int i=0;i<sources.Length;i++)
        {
            token.ThrowIfCancellationRequested();var s=sources[i];
            if(s.Index!=i||s.Points!=Document.Strokes[i].Points.Count||
                Ingress.CurrentView.Curves[i].OriginalSourceIndex!=i||
                !ReferenceEquals(Ingress.CurrentView.Curves[i].Document,Document))
                throw new InvalidOperationException("Same original C49 constructor and raw slot required.");
        }
    }
    public IReadOnlyList<LegacyPhysicalTimingSourceSnapshot> CopySources(CancellationToken token=default)
    {
        Validate(token);return Array.AsReadOnly(sources.Select(s=>new LegacyPhysicalTimingSourceSnapshot(s.Index,s.Points,
            s.Kind,s.Rate,s.Period,s.Base,s.Kind==LegacyPhysicalRawTimingKind.Uniform?null:4,
            s.Kind==LegacyPhysicalRawTimingKind.Uniform?null:NativeMillisecondMultiplier())).ToArray());
    }
    public int[]? CopyMeasuredMilliseconds(int source,CancellationToken token=default)
    {Validate(token);return Get(source).Samples?.ToArray();}
    public long ReadOriginalSample(int source,int point,CancellationToken token=default)
    {Validate(token);var s=Get(source);if((uint)point>=(uint)s.Points)throw new ArgumentOutOfRangeException(nameof(point));return Original(s,point);}
    private LegacyPhysicalTimingSource Get(int source)
    {if((uint)source>=(uint)sources.Length)throw new ArgumentOutOfRangeException(nameof(source));return sources[source];}
    private static long Original(LegacyPhysicalTimingSource s,int point)
        =>checked(s.Base+(s.Samples is{} samples?(long)samples[point]*NativeMillisecondMultiplier():(long)s.Period*(uint)point));

    public long ReadViewStart(LegacyGlobalOrdinaryCurveView view,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(view);Validate(token);view.Owner.Validate(token);
        if(!ReferenceEquals(view.Owner.Publication.Scope.Document,Document)||
            !memberships.GetValue(view.Owner,owner=>new(owner.Views)).Contains(view))
            throw new ArgumentException("Actual produced cropped view membership required.");
        return ReadViewStartValidated(view);
    }
    internal long ReadViewStartValidated(LegacyGlobalOrdinaryCurveView view)
    {
        if(!ReferenceEquals(view.Owner.Publication.Scope.Document,Document))
            throw new ArgumentException("Actual cropped view must use this exact timed raw document.");
        var s=Get(view.RawObjectIndex);int p=view.FirstOriginalPoint;
        if((uint)p>=(uint)s.Points)throw new NotSupportedException("Actual crop start out of original source.");
        if(view.Kind==LegacyGlobalRawCurveViewKind.OriginalC49)return s.Base;
        long first=Original(s,p);
        if(view.Kind==LegacyGlobalRawCurveViewKind.IntegerCropC4A)return first;
        if(view.Kind!=LegacyGlobalRawCurveViewKind.FractionalCropC4B)
            throw new NotSupportedException("Actual ordinary C49/C4A/C4B temporal view required.");
        float fraction=view.FirstFraction;
        if(!float.IsFinite(fraction)||fraction<0||fraction>=1)
            throw new NotSupportedException("Finite bounded actual first fraction required.");
        if(fraction==0)return first;
        if(p+1>=s.Points)throw new NotSupportedException("Fractional start needs actual next point.");
        long next=Original(s,p+1);
        return NativeFractionalStart(first,next,fraction);
    }
    internal static long NativeFractionalStart(long first,long next,float fraction)
    {
        Bound(first);Bound(next);
        if(!float.IsFinite(fraction)||fraction<0||fraction>=1)throw new ArgumentOutOfRangeException(nameof(fraction));
        if(fraction==0)return first;
        double delta=(double)next-(double)first;
        double scaled=(double)fraction*delta;
        double value=scaled+(double)first;value=value+.5;
        if(!double.IsFinite(value)||value<=-(double)(1L<<52)||value>=(double)(1L<<52))
            throw new NotSupportedException("Native fractional temporal conversion outside admitted domain.");
        long converted=checked((long)value);Bound(converted);return converted;
    }
}
