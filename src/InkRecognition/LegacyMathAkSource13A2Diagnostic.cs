using System.Numerics;

namespace InkRecognition;

internal sealed partial class LegacyMathAkOriginalInputContext
{
    //Diagnostic DATA only. This is a separate fresh route, never a historical resume.
    //No context/frame/output/claim object escapes through the detached result.
    internal static Source13A2DiagnosticData PrepareFreshThrough13A2Diagnostic(
        LegacyMathAkSourceGeometry original,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(original);token.ThrowIfCancellationRequested();
        Require13A0Admission();
        int s=original.StrokeCount,n=original.PointCount,p=checked(n+4*s);
        if(s is<1 or>64||n is<1 or>LegacyMathAkSourceGeometry.MaximumPoints||
            p>LegacyMathAkProcessedStrokePrefix.MaximumExpandedPoints)
            throw new InvalidOperationException("Original source cardinality mismatch.");
        var budget=new Source13A0Budget();int w=(s>>5)+1,a=(s+31)>>5;
        budget.Reserve(checked(32L*n+100L*p+228L*s+6L*s*(s+1)+8L*s*w+4L*(a+w)+216));
        budget.Reserve(checked(316L*p+156L*s+68));
        var frame=LegacyMathAkOriginalInputFrame.CreateFresh(original,token);
        var context=new LegacyMathAkOriginalInputContext(frame);
        try
        {
            context.prefix=LegacyMathAkProcessedStrokePrefix.PrepareOriginalInput(frame,token);
            context.Preflight(token);context.RunFresh13B6(token);
            context.Run13A0(budget,null,null,token);
            //The same source is still phase 2. No Snapshot or old scalar observer
            //can manufacture this call, and no finished context is accepted.
            var sampled=context.Run13A2Sampler(token);
            var data=context.Export13A2Diagnostic(sampled,token);
            token.ThrowIfCancellationRequested();context.phase=3;return data;
        }
        catch{context.phase=-1;throw;}
    }

    private Source13A2DiagnosticData Export13A2Diagnostic(Source13A2Output sampled,CancellationToken token)
    {
        Live();token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(sampled.Owner,this)||source13A0 is null||prefix is null)
            throw new InvalidOperationException("Diagnostic same fresh sampler owner required.");
        int s=input.StrokeCount,before=source13A0.PointCount,after=sampled.Points.Length,m=sampled.Markers.Length;
        if(source13A0.Points.Length!=s||source13A0.OriginalIndices.Length!=s||
            sampled.Ends.Length!=s||sampled.OriginalIndices.Length!=after||sampled.Metrics.Length!=s)
            throw new InvalidOperationException("Diagnostic current source cardinality mismatch.");
        //A separate enforced diagnostic reservation. The old 512-byte scalar
        //observer is not charged or used. These are logical payload/copy units.
        const long byteLimit=64L*1024*1024,workLimit=64L*1024*1024;
        long bytes=checked(256+12L*before+12L*after+24L*m+32L*s);
        long work=checked(6L*before+6L*after+12L*m+12L*s);
        if(bytes>byteLimit||work>workLimit)
            throw new NotSupportedException("Sampler diagnostic array reservation exhausted.");
        token.ThrowIfCancellationRequested();
        var beforeBits=new int[checked(2*before)];var beforeMap=new int[before];var beforeEnds=new int[s];
        var pointBits=new int[checked(2*after)];var map=new int[after];var ends=new int[s];
        var markers=new int[checked(3*m)];var ranges=new int[checked(3*m)];var metrics=new int[checked(6*s)];
        int at=0;
        for(int stroke=0;stroke<s;stroke++)
        {
            token.ThrowIfCancellationRequested();var row=source13A0.Points[stroke];var old=source13A0.OriginalIndices[stroke];
            if(row.Length!=old.Length||row.Length<1||row.Length>before-at)
                throw new InvalidOperationException("Diagnostic original row header mismatch.");
            for(int i=0;i<row.Length;i++,at++)
            {
                token.ThrowIfCancellationRequested();beforeBits[2*at]=BitConverter.SingleToInt32Bits(row[i].X);
                beforeBits[2*at+1]=BitConverter.SingleToInt32Bits(row[i].Y);beforeMap[at]=old[i];
            }
            beforeEnds[stroke]=at;ends[stroke]=sampled.Ends[stroke];
            var metric=sampled.Metrics[stroke];int k=6*stroke;
            metrics[k]=BitConverter.SingleToInt32Bits(metric.MinimumX);metrics[k+1]=BitConverter.SingleToInt32Bits(metric.MinimumY);
            metrics[k+2]=BitConverter.SingleToInt32Bits(metric.MaximumX);metrics[k+3]=BitConverter.SingleToInt32Bits(metric.MaximumY);
            metrics[k+4]=BitConverter.SingleToInt32Bits(metric.CenterX);metrics[k+5]=BitConverter.SingleToInt32Bits(metric.CenterY);
        }
        if(at!=before)throw new InvalidOperationException("Diagnostic before count mismatch.");
        for(int i=0;i<after;i++)
        {
            token.ThrowIfCancellationRequested();pointBits[2*i]=BitConverter.SingleToInt32Bits(sampled.Points[i].X);
            pointBits[2*i+1]=BitConverter.SingleToInt32Bits(sampled.Points[i].Y);map[i]=sampled.OriginalIndices[i];
        }
        for(int i=0;i<m;i++)
        {
            token.ThrowIfCancellationRequested();var cut=sampled.Markers[i];int k=3*i;
            markers[k]=cut.Tag;markers[k+1]=cut.Stroke;markers[k+2]=cut.Offset;
            var range=sampled.Range(i);ranges[k]=range.Stroke;ranges[k+1]=range.Start;ranges[k+2]=range.End;
        }
        Live();token.ThrowIfCancellationRequested();
        return new(Source13A2DiagnosticVersion,BitConverter.SingleToInt32Bits(prefix.EstimatedScale),beforeBits,beforeMap,
            beforeEnds,pointBits,map,ends,markers,ranges,metrics,sampled.ChargedBytes,sampled.ChargedWork,bytes,work);
    }
    private const int Source13A2DiagnosticVersion=1;
    internal sealed record Source13A2DiagnosticData(int Version,int HeightBits,
        int[] BeforePointBits,int[] BeforeOriginalIndices,int[] BeforeEnds,
        int[] PointBits,int[] OriginalIndices,int[] Ends,int[] MarkerFields,int[] RangeFields,int[] MetricBits,
        long SamplerChargedBytes,long SamplerChargedWork,long DiagnosticChargedBytes,long DiagnosticChargedWork);
}
