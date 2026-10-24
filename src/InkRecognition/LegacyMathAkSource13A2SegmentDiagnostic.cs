using System.Buffers.Binary;

namespace InkRecognition;

internal sealed partial class LegacyMathAkOriginalInputContext
{
    //Separate fresh diagnostic: never resumes a historical sampler DATA object.
    //The original13A0 and sampler diagnostic factories remain unchanged.
    internal static Source13A2SegmentDiagnosticData PrepareFreshThrough13A2SegmentsDiagnostic(
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
            var sampled=context.Run13A2Sampler(token);
            var segments=context.Build13A2Segments(sampled,token);
            var data=context.Export13A2SegmentsDiagnostic(sampled,segments,token);
            token.ThrowIfCancellationRequested();context.phase=3;return data;
        }
        catch{context.phase=-1;throw;}
    }
    private Source13A2SegmentDiagnosticData Export13A2SegmentsDiagnostic(Source13A2Output sampled,
        Source13A2SegmentFrame segments,CancellationToken token)
    {
        Live();segments.Validate(token);
        if(!ReferenceEquals(sampled.Owner,this))throw new InvalidOperationException("Same diagnostic source required.");
        int m=segments.Count,s=segments.OriginalCount;
        //Four detached arrays, independently paid before allocation. Native
        //pointer/padding bytes have zero placeholders with UNKNOWN mask bits.
        long bytes=checked(128+120L*m+4L*(s+1)),work=checked(320L*m+16L*s+128);
        const long diagnosticByteLimit=64L*1024*1024,diagnosticWorkLimit=64L*1024*1024;
        if(bytes>diagnosticByteLimit||work>diagnosticWorkLimit)
            throw new NotSupportedException("Segment diagnostic array reservation exhausted.");
        token.ThrowIfCancellationRequested();
        //This independently enforces the already-reviewed sampler export64MiB
        //reservation; it is not charged to the old512B scalar observer.
        var sampler=Export13A2Diagnostic(sampled,token);
        var records=new byte[checked(96*m)];var masks=new byte[checked(12*m)];
        var ranges=new int[checked(3*m)];var starts=new int[s+1];
        for(int i=0;i<m;i++)
        {
            token.ThrowIfCancellationRequested();var value=segments.Read(i,token);
            Span<byte> row=records.AsSpan(96*i,96),mask=masks.AsSpan(12*i,12);
            //Candidate vector00..0F is genuinely empty. Allocated native range
            //pointer10..17 is NOT represented by the managed value storage.
            Mark13A2Known(mask,0,16);
            BinaryPrimitives.WriteInt32LittleEndian(row[0x18..],1);
            BinaryPrimitives.WriteInt32LittleEndian(row[0x1c..],1);row[0x20]=value.EndsOriginal;
            Mark13A2Known(mask,0x18,9);
            var metric=value.Metric;
            Write13A2Float(row,0x24,metric.MinimumX);Write13A2Float(row,0x28,metric.MinimumY);
            Write13A2Float(row,0x2c,metric.MaximumX);Write13A2Float(row,0x30,metric.MaximumY);
            Write13A2Float(row,0x34,metric.CenterX);Write13A2Float(row,0x38,metric.CenterY);
            Mark13A2Known(mask,0x24,24);
            ranges[3*i]=value.Range.OriginalId;ranges[3*i+1]=value.Range.Start;ranges[3*i+2]=value.Range.InclusiveEnd;
        }
        for(int i=0;i<=s;i++){token.ThrowIfCancellationRequested();starts[i]=segments.ReadOriginalStart(i,token);}
        segments.Validate(token);Live();token.ThrowIfCancellationRequested();
        return new(1,sampler,records,masks,ranges,starts,m,segments.TotalMembership,segments.RequiredWorkWords,
            segments.ChargedBytes,segments.ChargedWork,bytes,work);
    }
    private static void Write13A2Float(Span<byte> row,int offset,float value)
        =>BinaryPrimitives.WriteInt32LittleEndian(row[offset..],BitConverter.SingleToInt32Bits(value));
    private static void Mark13A2Known(Span<byte> mask,int first,int count)
    {
        //Only the three literal fixed ranges above reach this helper. Their
        //49 writes plus52 condition tests per row fit the prepaid320M envelope.
        for(int i=first;i<first+count;i++)mask[i>>3]|=(byte)(1<<(i&7));
    }
    internal sealed record Source13A2SegmentDiagnosticData(int Version,Source13A2DiagnosticData Sampler,
        byte[] RecordBytes,byte[] DefinedMasks,int[] RangeTriples,int[] OriginalOffsets,
        int SegmentCount,int TotalMembership,int RequiredWorkWords,long BridgeChargedBytes,long BridgeChargedWork,
        long DiagnosticChargedBytes,long DiagnosticChargedWork);
}
