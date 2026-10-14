using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Owned single-physical/single-source720 main preparation through24B,
///24A's assigned-line branch,24D's zero-gap shear branch,29B's zero-gap branch,
///and265 smoothing. Rejects the general multi-glyph estimator rather than
///substituting a zero shear or an assumed gap for that unimplemented branch.</summary>
public sealed class LegacyFreshSingleGlyphMainPostprocessing
{
    public LegacyFreshSingleStrokeMainLine Input {get;}
    public LegacyOrientationSmoothingProfile Smoothing {get;}
    public float SerializedZeroGap {get;}
    public int Processor24DOffset {get;}
    public int Processor29BOffset {get;}
    public int Processor265Offset {get;}
    private LegacyFreshSingleGlyphMainPostprocessing(LegacyFreshSingleStrokeMainLine input,
        LegacyOrientationSmoothingProfile smoothing,float gap,int d,int b,int smooth)
    {Input=input;Smoothing=smoothing;SerializedZeroGap=gap;Processor24DOffset=d;Processor29BOffset=b;Processor265Offset=smooth;}

    public static LegacyFreshSingleGlyphMainPostprocessing Prepare(ReadOnlySpan<byte> resource,int sourceOffset,
        InkStroke actualRawStroke,float actualResolution,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();byte[] snapshot=resource.ToArray();
        var config=LegacyOrdinarySourceInk.ReadV10dResourceProfile(snapshot,sourceOffset);
        int end=NodeEnd(snapshot,sourceOffset,0x247,snapshot.Length);
        int at=config.EnclosureFlagOffset+12;
        if(at>end-4||I(snapshot,at)!=6)throw new NotSupportedException("Exact owned249/24B/24A/24D/29B/265 processor sequence required.");
        at+=4;int[] kinds=[0x249,0x24b,0x24a,0x24d,0x29b,0x265];int[] offsets=new int[6];
        for(int index=0;index<kinds.Length;index++)
        {offsets[index]=at;at=NodeEnd(snapshot,at,kinds[index],end);cancellationToken.ThrowIfCancellationRequested();}
        int d=offsets[3],b=offsets[4],smooth=offsets[5];
        if(NodeEnd(snapshot,d,0x24d,end)-d!=38||U16(snapshot,d+14)!=0x100||
            snapshot[d+16]>=snapshot[d+17])throw new InvalidDataException("Finite24D/v100 count gate required.");
        //1781E0 reads two count bytes then five floats: floor,normalizer,
        //radian angles and radian step. With one glyph gaps0 and min>=0,
        //blend=0. Related-form multiplication remains finite with these bounds.
        float floor=F(snapshot,d+18),normalizer=F(snapshot,d+22),low=F(snapshot,d+26),high=F(snapshot,d+30),step=F(snapshot,d+34);
        if(!float.IsFinite(floor)||floor<0||floor>1||!float.IsFinite(normalizer)||normalizer<=0||normalizer>10||
            !float.IsFinite(low)||!float.IsFinite(high)||low< -1.5f||high>1.5f||low>=high||
            !float.IsFinite(step)||step<=0||step>3)
            throw new InvalidDataException("Bounded24D finite zero-gap profile required.");
        if(NodeEnd(snapshot,b,0x29b,end)-b!=58||U16(snapshot,b+14)!=0x100)
            throw new InvalidDataException("Exact29B/v100 profile required.");
        for(int i=0;i<6;i++)if(!float.IsFinite(F(snapshot,b+16+i*4)))throw new InvalidDataException("Finite29B profile required.");
        if(snapshot[b+40]>=snapshot[b+41])throw new InvalidDataException("29B count gate required.");
        for(int i=0;i<4;i++)if(!float.IsFinite(F(snapshot,b+42+i*4)))throw new InvalidDataException("Finite29B tail required.");
        //1763E0 reads zero-gap source9C value immediately after the count bytes.
        float gap=F(snapshot,b+42);
        var smoothing=LegacyOrientationSmoothingProfile.ReadV102Node(snapshot,smooth);
        var input=LegacyFreshSingleStrokeMainLine.Prepare(snapshot,sourceOffset,actualRawStroke,actualResolution,cancellationToken);
        return new(input,smoothing,gap,d,b,smooth);
    }

    public LegacyFreshSingleGlyphMainPrepared ApplyFirstResult(LegacyFirstMaterializedPlainWord actualWord,
        LegacyOrdinaryGraphSourceRanges actualSourceRanges,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var line=Input.ApplyFirstResult(actualWord,actualSourceRanges,cancellationToken);
        if(line.SourceRows.Count!=1||line.Next24ARequiresLineInference||line.Output.Deskew is not{} deskew)
            throw new NotSupportedException("Only actual one-glyph assigned-line24D/29B branch is restored.");
        var guide=deskew.CopyGuide().ToArray();float height=F(guide,4),baseline=F(guide,0);
        if(!float.IsFinite(height)||height<=0||!float.IsFinite(baseline))throw new ArgumentException("Actual positive main NN line required.");
        var physical=deskew.CopyPhysicalPoints();
        if(physical.Length!=1)throw new ArgumentException("Actual owned singleton physical source required.");
        var before=deskew.Rows[0];byte[] storage=before.CopyStorage().ToArray();
        float primaryWeight=F(storage,80);
        if(!float.IsFinite(primaryWeight)||primaryWeight<0||primaryWeight>1)
            throw new ArgumentException("Finite actual source probability weight required.");
        //177280: gapcount0 -> blend0 -> grid[-0,+0], so no grid iteration.
        //The direction scanner uses the same scaled bounds: only zero ratios
        //contribute. No previous-source object exists, hence chosen shear+0.
        //Execute the native floating operations rather than retaining pointers
        //or silently bypassing their signed-zero and row-bounds effects.
        const float shear=0;float anchor=shear*baseline;
        for(int p=0;p<physical[0].Length;p++)
        {cancellationToken.ThrowIfCancellationRequested();var point=physical[0][p];physical[0][p]=point with{X=(-shear*point.Y+point.X)+anchor};}
        if(before.Ranges.Count==0)
        {
            float centerY=(F(storage,36)+F(storage,32))*.5f,halfY=(F(storage,36)-F(storage,32))*.5f;
            float halfX=(F(storage,28)-F(storage,24))*.5f;
            float centerX=((F(storage,28)+F(storage,24))*.5f+centerY*-shear)+anchor;
            WF(storage,24,centerX-halfX);WF(storage,28,halfX+centerX);
            WF(storage,32,centerY-halfY);WF(storage,36,halfY+centerY);
        }
        else
        {
            float left=float.MaxValue,right=-float.MaxValue,top=float.MaxValue,bottom=-float.MaxValue;
            int visits=0;
            foreach(var range in before.Ranges)
            {
                var first=range.First;var last=range.Last;
                if(first.First!=last.First||first.Second!=0||last.Second!=0||first.Third<0||last.Third<first.Third||last.Third>=physical[0].Length)
                    throw new ArgumentException("Owned singleton retained range required.");
                for(int p=first.Third;p<=last.Third;p++)
                {
                    cancellationToken.ThrowIfCancellationRequested();if(++visits>1_048_576)throw new ArgumentException("Range visit bound exceeded.");
                    var point=physical[0][p];if(point.X<=left)left=point.X;if(right<=point.X)right=point.X;
                    if(point.Y<=top)top=point.Y;if(bottom<=point.Y)bottom=point.Y;
                }
            }
            WF(storage,24,left);WF(storage,28,right);WF(storage,32,top);WF(storage,36,bottom);
        }
        var row=new LegacyFirstResultLineRow(storage,before.Ranges);
        //15ABE0 runs before249/24B;265 must consume those ORIGINAL source metrics,
        //not recompute exact-axis metrics from rotated or sheared points.
        var metrics=LegacySourceOrientationPreparation.ExtractMetrics(Input.Controller.Source,cancellationToken);
        var smoothed=LegacyOrientationStrokePreparation.PrepareWithPointKeys([new InkStroke(physical[0])],Smoothing,height,metrics,cancellationToken);
        //16A130 sets source128=200; singleton15F050's118 identity map preserves
        //the265 generated fractional200 keys. Source720 ranges/boxes stay in
        //their published pre265 domain:173C10 does not rewrite that buffer.
        var originalKeys=Enumerable.Range(0,Input.Controller.Source.Strokes[0].Points.Count)
            .Select(p=>Input.Controller.Source.Chronology.Resolve(0,p).Key).ToArray();
        var chronology=LegacySourcePointChronology.CreatePrepared(smoothed.PhysicalPointCounts.Span,originalKeys,
            Enumerable.Range(0,originalKeys.Length).ToArray(),smoothed.PointKeys200.Span.ToArray().Select(k=>checked((uint)k)).ToArray(),200);
        return new(line,guide,physical,row,smoothed,chronology,SerializedZeroGap,metrics);
    }
    private static int NodeEnd(ReadOnlySpan<byte>b,int at,int kind,int end)
    {
        if(at<0||end>b.Length||at>end-16||I(b,at)!=0x101fe90||I(b,at+8)!=kind||U16(b,at+12)!=0)
            throw new InvalidDataException("Complete owned unencoded processor node required.");
        uint length=BinaryPrimitives.ReadUInt32LittleEndian(b[(at+4)..]);if(length<8||length>end-at-8)throw new InvalidDataException("Truncated processor node.");
        return checked(at+8+(int)length);
    }
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private static ushort U16(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(b[at..]);
    private static float F(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadSingleLittleEndian(b[at..]);
    private static void WF(Span<byte>b,int at,float value)=>BinaryPrimitives.WriteSingleLittleEndian(b[at..],value);
}

/// <summary>Source points after265 with actual guide and factor200 chronology.
/// SourceRow retains the native pre265 publication geometry/range domain.
/// No main glyph labels, winner confidence or modern Document font metadata.</summary>
public sealed class LegacyFreshSingleGlyphMainPrepared
{
    private readonly byte[] guide;
    private readonly InkPoint[][] physical;
    public LegacyFreshSingleStrokeMainLineResult LinePreparation {get;}
    public LegacyFirstResultLineRow SourceRow {get;}
    public LegacyOrientationStrokePreparationWithKeysResult Smoothed {get;}
    public LegacySourcePointChronology Chronology {get;}
    public LegacyOrientationStrokeMetrics OriginalSourceMetrics {get;}
    public float Shear=>0;
    public float SourceGap {get;}
    internal LegacyFreshSingleGlyphMainPrepared(LegacyFreshSingleStrokeMainLineResult line,byte[] guide,InkPoint[][] physical,
        LegacyFirstResultLineRow row,LegacyOrientationStrokePreparationWithKeysResult smoothed,LegacySourcePointChronology chronology,
        float gap,LegacyOrientationStrokeMetrics metrics)
    {LinePreparation=line;this.guide=guide;this.physical=physical;SourceRow=row;Smoothed=smoothed;Chronology=chronology;SourceGap=gap;OriginalSourceMetrics=metrics;}
    public ReadOnlyMemory<byte> CopyGuide()=>guide.ToArray();
    public InkPoint[][] CopyBefore265Points()=>physical.Select(points=>points.ToArray()).ToArray();
}
