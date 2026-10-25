using System.Numerics;
namespace InkRecognition;

public sealed partial class LegacyMathAkFeatureProfile
{
    //Private-profile data use, never a source/request/evaluator issuer.
    internal void RequireFirst276FeatureShape(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var d=first276.Description;
        if(d.SlotOrdinal!=2||d.WhichModel!=1||d.CoordinatePrefix!=0||d.BoundaryPair!=1||
            d.CalibrationWidth!=276||d.ModelInputWidth!=276||d.Source58!=0||
            first276.ReadProgramByte(0)!=1||first276.ReadProgramByte(1)!=1||
            FirstFeatureU16(2)!=8192||FirstFeatureU16(23)!=0x11||
            FirstFeatureU32(25)!=0x1e1fff||FirstFeatureU32(33)!=0xe1fff||
            FirstFeatureU32(45)!=100||FirstFeatureU32(49)!=10||
            FirstFeatureU16(53)!=1||FirstFeatureU16(55)!=16||
            first276.ReadProgramByte(69)!=1||first276.ReadProgramByte(70)!=2)
            throw new NotSupportedException("Actual FIRST directional shape differs.");
    }
    private int FirstFeatureU16(int at)=>first276.ReadProgramByte(at)|first276.ReadProgramByte(at+1)<<8;
    private uint FirstFeatureU32(int at)=>(uint)(first276.ReadProgramByte(at)|first276.ReadProgramByte(at+1)<<8|
        first276.ReadProgramByte(at+2)<<16|first276.ReadProgramByte(at+3)<<24);
    internal short[] ComposeFirst276Geometry(int[] global,int[][] windows,CancellationToken token)
    {
        RequireFirst276FeatureShape(token);
        //FIRST calibration274 is independent from all selected-tail arrays.
        return first276.ComposeGeometry(global,windows,token);
    }
    private sealed partial class First276Program
    {
        internal short[] ComposeGeometry(int[] global,int[][] windows,CancellationToken token)
            =>LegacyBaselineFreeDirectional2B1Assembly.Compose(global,windows,false,
                minimum.AsSpan(0,274),maximum.AsSpan(0,274),token);
    }
}

internal sealed partial class LegacyMathAkOriginalInputContext
{
    private sealed partial class First276SourceBinding
    {
        private sealed partial class First276ExclusiveStorage
        {
            internal short[] CopyFirst276Data(First276FreshInputProducer actual,CancellationToken token)
            {
                actual.RequireOwnedInputRead(this,binding,token);RequireInputProducer(binding,token);
                var copy=new short[276];
                for(int i=0;i<copy.Length;i++){token.ThrowIfCancellationRequested();copy[i]=input[i];}
                return copy;
            }
            internal void WriteFirst276Geometry(First276FreshInputProducer actual,short[] core,
                short firstBoundary,short lastBoundary,CancellationToken token)
            {
                actual.RequireOwnedInputWrite(this,binding,token);RequireInputProducer(binding,token);
                if(core.Length!=274)throw new InvalidOperationException("Complete actual FIRST core required.");
                for(int i=0;i<274;i++){token.ThrowIfCancellationRequested();input[i]=core[i];}
                input[274]=firstBoundary;input[275]=lastBoundary;
                //Physical padding276..287 stays unknown/unread. No evaluator flags.
            }
        }
        private sealed partial class First276FreshInputProducer:ILegacyDirectionalStructuralDebit
        {
            //Unadmitted until the additive source/helper ledger is closed.
            private const long FeaturePreflightUpper=2803,FeatureNormalizeUpper=917,FeatureResampleUpper=833;
            private const long FeatureLandmarkUpper=672,FeatureIntersectionUpper=1537,FeatureWindowUpper=1120;
            private const long FeatureGlobalUpper=833,FeatureAssemblyUpper=2202,FeatureWriteUpper=31644;
            private const long FeatureCoordinateUpper=984,FeatureDistanceUpper=1070,FeatureValueUpper=249;
            private const long FeatureVarianceUpper=33,FeatureArcUpper=497,FeaturePairUpper=5821,FeatureIntervalUpper=51;
            private const long FeatureGridCallbackUpper=1664,FeatureIntervalCallbackUpper=1608;
            internal void RequireOwnedInputRead(First276ExclusiveStorage actual,First276SourceBinding source,
                CancellationToken token)
            {
                RequireProducer(token);
                if(!ReferenceEquals(actual,storage)||!ReferenceEquals(source,binding)||featureCount!=276||featurePreparedCount<1)
                    throw new InvalidOperationException("Complete same-storage FIRST input copy required.");
            }
            internal void RequireOwnedInputWrite(First276ExclusiveStorage actual,First276SourceBinding source,
                CancellationToken token)
            {
                RequireProducer(token);
                if(!ReferenceEquals(actual,storage)||!ReferenceEquals(source,binding)||featureCount!=0)
                    throw new InvalidOperationException("One same-storage FIRST input write required.");
            }
            private void PrepareActualFirstFeatures(CancellationToken token)
            {
                Charge(0,checked(64L*(rangeCount+17)),checked(FeaturePreflightUpper*((long)rangeCount+17)+RequireSourceUpper),token);RequireProducer(token);
                binding.profile.RequireFirst276FeatureShape(token);
                if(featureCount!=0||featurePreparedCount!=0||rangeCount is<1 or>4)
                    throw new InvalidOperationException("One real FIRST gathered request required.");
                var actual=binding.sampled;int s=actual.Ends.Length;
                if(!ReferenceEquals(actual.Owner,binding.owner)||s!=binding.owner.input.StrokeCount||s is<1 or>64||
                    actual.Points.Length<1||actual.Ends[s-1]!=actual.Points.Length)
                    throw new InvalidOperationException("Actual sampled coordinate source changed.");
                int points=0;
                for(int i=0;i<rangeCount;i++)
                {
                    var r=ranges[i];int id=r.OriginalId;
                    if((uint)id>=(uint)s||r.Start<0||r.InclusiveEnd<r.Start)
                        throw new InvalidOperationException("Actual FIRST range changed.");
                    int first=id==0?0:actual.Ends[id-1],length=actual.Ends[id]-first;
                    if(r.InclusiveEnd>=length||binding.owner.input.ReadStrokeOrdinal(id)!=id||
                        binding.owner.input.ReadItemKind(id)!=-1)
                        throw new InvalidOperationException("Same raw original range owner required.");
                    points=checked(points+r.InclusiveEnd-r.Start+1);
                }
                if(points is<1 or>4096)
                    throw new NotSupportedException("Existing directional source-point domain exceeded.");
                //FB90 rescans actual requested XY. The selected metric bbox must
                //not replace its own normalization min/max or coordinate reads.
                Charge(checked(8L*points+16L*rangeCount),checked(64L*points+64L*rangeCount+128),
                    checked(FeatureNormalizeUpper*((long)points+rangeCount+1)),token);
                var strokes=NormalizeActualFirstRanges(actual,points,token);
                Charge(checked(12L*points+28*256+80L*rangeCount+92),checked(64L*points+128*256+128L*rangeCount+1024),
                    checked(FeatureCoordinateUpper*((long)points+rangeCount+32)+
                        FeatureResampleUpper*((long)points+256+rangeCount+1)+
                        FeatureDistanceUpper*(2L*points+256+1)+FeatureValueUpper*((long)points+256+rangeCount+1)),token);
                var samples=LegacyDirectionalResampling.Prepare(strokes,100,false,token);
                int p=samples.CumulativePhysicalLengths.Length;
                if(p is<1 or>255)throw new NotSupportedException("Existing actual byte-index feature domain exceeded.");
                Charge(0,4,0,token);featurePreparedCount=p;
                Charge(checked(p+4L*rangeCount+32),checked(32L*p+16L*rangeCount+64),
                    checked(FeatureLandmarkUpper*(4L*p+rangeCount+1)),token);
                var direction=LegacyDirectional2B1Landmarks.Extract(samples.Strokes,token);
                //Flags/result can allocate before the grid callback; reserve
                //them here. Grid/pairs and actual intervals are debited inside.
                Charge(p+20,checked(2L*p+40),checked(FeatureIntersectionUpper*((long)p+rangeCount+1)),token);
                var intersections=LegacyDirectionalIntersections.ExtractWithStructuralDebit(samples,10,token,this);
                Charge(68,checked(64L*p+256),checked(FeatureGlobalUpper*((long)p+rangeCount+1)+
                    FeatureVarianceUpper*31*((long)p+1)),token);
                var global=LegacyDirectionalStatistics.Extract(samples.Strokes,samples.Width,samples.Height,
                    samples.PhysicalArcLength,direction.Flags.Span,intersections.Flags.Span,intersections.SelfLoopCount,token);
                Charge(checked(65L*p+1288),checked(512L*p+4096),
                    checked(FeatureWindowUpper*16*((long)p+rangeCount+1)+FeatureVarianceUpper*31*16*((long)p+1)+
                        FeatureArcUpper*((long)p+17)),token);
                var members=LegacyArcLengthWindows.Partition(samples.CumulativePhysicalLengths.Span,16,token);
                var windows=new int[16][];
                for(int i=0;i<16;i++)windows[i]=LegacyDirectionalWindowStatistics.Extract(samples,members[i].Span,
                    direction.Flags.Span,intersections.Flags.Span,false,token);
                Charge(676,1352,checked(FeatureAssemblyUpper*291),token);
                var core=binding.profile.ComposeFirst276Geometry(global,windows,token);
                //Fresh request14/15=0 -> native matcher10C/D0, not resource fields.
                //The physical length is from SAME sampled source, not raw count.
                var firstRange=ranges[0];var lastRange=ranges[rangeCount-1];
                int lastBase=lastRange.OriginalId==0?0:actual.Ends[lastRange.OriginalId-1];
                int lastLength=actual.Ends[lastRange.OriginalId]-lastBase;
                int q=binding.profile.ReadFirst276InputPolicy(token).InputQuantizer;
                if(q!=10)throw new InvalidOperationException("Same authenticated FIRST quantizer required.");
                short firstBoundary=QuantizeFirstBoundary(firstRange.Start==0,q);
                short lastBoundary=QuantizeFirstBoundary(lastRange.InclusiveEnd+1==lastLength,q);
                Charge(0,556,FeatureWriteUpper,token);
                storage.WriteFirst276Geometry(this,core,firstBoundary,lastBoundary,token);
                featureCount=276;
            }
            void ILegacyDirectionalStructuralDebit.BeforeGrid(int points,int cells,CancellationToken token)=>DebitFirstGrid(points,cells,token);
            private void DebitFirstGrid(int points,int cells,CancellationToken token)
            {
                if(featurePreparedCount!=points||featureCount!=0||points is<1 or>255||cells is<1 or>10000)
                    throw new InvalidOperationException("Same active FIRST directional grid required.");
                long pairs=(long)points*(points-1)/2,payload=checked(4L*cells+24L*points+16);
                Charge(payload,checked(2*payload+64*pairs+64L*points+8L*cells),
                    checked(FeatureIntersectionUpper*(4L*points+rangeCount+5)+FeaturePairUpper*pairs+
                        FeatureGridCallbackUpper),token);
                RequireProducer(token);
            }
            void ILegacyDirectionalStructuralDebit.BeforeInterval(int count,CancellationToken token)=>DebitFirstInterval(count,token);
            private void DebitFirstInterval(int count,CancellationToken token)
            {
                if(featurePreparedCount is<1 or>255||featureCount!=0||count<0||count>featurePreparedCount)
                    throw new InvalidOperationException("Same active FIRST intersection interval required.");
                Charge(0,checked(8L*count),checked(FeatureIntervalUpper*((long)count+1)+
                    FeatureIntervalCallbackUpper),token);
                RequireProducer(token);
            }
            private LegacyInkStroke[] NormalizeActualFirstRanges(Source13A2Output actual,int points,CancellationToken token)
            {
                float x0=float.MaxValue,y0=float.MaxValue,x1=-float.MaxValue,y1=-float.MaxValue;
                for(int i=0;i<rangeCount;i++)
                {
                    var r=ranges[i];int start=(r.OriginalId==0?0:actual.Ends[r.OriginalId-1])+r.Start;
                    for(int j=0;j<=r.InclusiveEnd-r.Start;j++)
                    {
                        token.ThrowIfCancellationRequested();var xy=actual.Points[start+j];Finite(xy.X);Finite(xy.Y);
                        if(xy.X<=x0)x0=xy.X;if(x1<xy.X)x1=xy.X;
                        if(xy.Y<=y0)y0=xy.Y;if(y1<xy.Y)y1=xy.Y;
                    }
                }
                float scale=1f;
                if(x1!=x0&&y1!=y0){scale=8192f/(x1-x0);float ys=8192f/(y1-y0);if(ys<=scale)scale=ys;}
                Finite(scale);
                var rows=new LegacyInkStroke[rangeCount];
                for(int i=0;i<rangeCount;i++)
                {
                    var r=ranges[i];int start=(r.OriginalId==0?0:actual.Ends[r.OriginalId-1])+r.Start;
                    var row=new LegacyInkPoint[r.InclusiveEnd-r.Start+1];
                    for(int j=0;j<row.Length;j++)
                    {
                        token.ThrowIfCancellationRequested();var xy=actual.Points[start+j];
                        row[j]=new(FirstCoordinate((xy.X-x0)*scale+0f),FirstCoordinate((xy.Y-y0)*scale+0f));
                    }
                    rows[i]=new LegacyInkStroke(row);
                }
                //Source148 is same immutable139D scale. Native FB90 also returns
                //these short baseline operands; mask11 excludes their channels.
                float sourceScale=binding.owner.prefix!.EstimatedScale;Finite(sourceScale);
                if(sourceScale>0f)
                {
                    _=FirstOperand((maxY-y0)*scale+0f);_=FirstOperand(sourceScale*scale);
                }
                //Else native emits baseline0/heightFFFF; no baseline channel is
                //enabled. Do not use these values to fabricate extra features.
                return rows;
            }
            private static short QuantizeFirstBoundary(bool boundary,int q)
            {
                //Actual EBB60: F32 power-of-two multiply, CVTT, low16 store.
                //The actual input is strictly +1/-1; q comes from SAME snapshot.
                float value=(boundary?1f:-1f)*(1<<q);
                return unchecked((short)(int)value);
            }
            private static short FirstCoordinate(float value)
            {
                float rounded=value<=0f?value-.5f:value+.5f;
                if(!float.IsFinite(rounded)||rounded<short.MinValue||rounded>=32768f)
                    throw new NotSupportedException("FIRST normalized coordinate exceeds existing nonwrapped short domain.");
                return (short)(int)rounded;
            }
            private static short FirstOperand(float value)
            {
                float rounded=value<=0f?value-.5f:value+.5f;
                if(!float.IsFinite(rounded)||rounded<-2147483648f||rounded>=2147483648f)
                    throw new NotSupportedException("FIRST baseline operand exceeds checked CVTT domain.");
                return unchecked((short)(int)rounded);
            }
        }
    }
}
