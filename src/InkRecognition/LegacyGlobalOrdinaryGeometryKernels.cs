namespace InkRecognition;

internal readonly record struct LegacyOrdinaryVerticalGeometry(float MinimumX,float MaximumX,float MinimumY,float MaximumY,
    double Count,double SumY,double SumY2,float MeanY,float StandardDeviationY,float Baseline0,float Baseline1,float Top,float Bottom);
internal readonly record struct LegacyOrdinaryHorizontalGeometry(LegacyOrdinaryGeometryBoundary[] Boundaries,
    bool Reversed,float MeanWidth,float MeanSpacing);
internal sealed class LegacyOrdinaryGeometryScratch
{
    internal int Previous=-1,Next=-1;
    internal double Count,Sum,Sum2,Mean,Std;
    internal float Min=float.MaxValue,Max=-float.MaxValue;
    internal long TimeMin=long.MaxValue,TimeMax=long.MinValue;
    internal uint Written;
    internal bool GapClass;
    internal LegacyOrdinaryGeometryScratchSnapshot Snapshot(int index)=>new(index,Written,
        (Written&1)!=0?Previous:null,(Written&1)!=0?Next:null,
        (Written&4)!=0?Count:null,(Written&4)!=0?Sum:null,(Written&4)!=0?Sum2:null,
        (Written&8)!=0?Mean:null,(Written&8)!=0?Std:null,(Written&1)!=0?Min:null,(Written&1)!=0?Max:null,
        (Written&2)!=0?TimeMin:null,(Written&2)!=0?TimeMax:null,
        (Written&1)!=0?false:null,(Written&1)!=0?GapClass:null,(Written&1)!=0?false:null);
}

internal static class LegacyGlobalOrdinaryGeometryKernels
{
    internal static (float X,float Y) Rotate(InkPoint p,double cosine,double sine)
    {
        double a=(double)p.Y*sine,b=(double)p.X*cosine;
        float x=(float)(a+b);
        a=(double)p.Y*cosine;b=(double)p.X*sine;
        return(x,(float)(a-b));
    }
    internal static LegacyOrdinaryVerticalGeometry Vertical(LegacyOrdinaryGeometryView[] views,
        LegacyOrdinaryGeometryGuide guide,double angle,double cosine,double sine,CancellationToken token)
    {
        float xmin=float.MaxValue,xmax=-float.MaxValue,ymin=float.MaxValue,ymax=-float.MaxValue;
        double n=0,sum=0,sum2=0;
        foreach(var view in views)foreach(var point in view.Points)
        {
            token.ThrowIfCancellationRequested();var p=Rotate(point,cosine,sine);
            if(p.X<xmin)xmin=p.X;if(xmax<p.X)xmax=p.X;
            if(p.Y<ymin)ymin=p.Y;if(ymax<p.Y)ymax=p.Y;
            n+=1;sum+=p.Y;sum2=(double)p.Y*p.Y+sum2;
        }
        if(n<=0)throw new NotSupportedException("Empty WORD moments require the separate native empty-line branch.");
        double difference=guide.Angle-angle;
        double tangent=Math.Sin(difference)/Math.Cos(difference);
        float gx=(float)guide.X,gy=(float)guide.Y,h=(float)guide.Height;
        float rx=(float)(cosine*gx+sine*gy),ry=(float)(cosine*gy-sine*gx);
        double intercept=(double)ry-(double)rx*tangent;
        float b0=(float)((double)xmin*tangent+intercept),b1=(float)((double)xmax*tangent+intercept);
        float a=tangent<0?b1:b0,b=tangent<0?b0:b1;
        a=a-h*2.5f;b=h*1.5f+b;
        float top=ymin,bottom=ymax;if(a<top)top=a;if(bottom<b)bottom=b;
        double mean=sum/n;double variance=(sum2*n-sum*sum)/(n*n);
        double sd=variance<=0?0:Math.Sqrt(variance);
        Check(xmin,xmax,ymin,ymax,top,bottom,(float)mean,(float)sd,b0,b1);
        return new(xmin,xmax,ymin,ymax,n,sum,sum2,(float)mean,(float)sd,b0,b1,top,bottom);
    }

    //04CD20: unrolled4 assignment order is observable. Weight/second moment
    //reductions are NOT interchangeable with a sequential LINQ Sum.
    internal static void WeightedMoments(LegacyOrdinaryGeometryScratch state,InkPoint[] points,
        float centerY,float height,double cosine,double sine,CancellationToken token)
    {
        float twice=height+height;
        (float X,float Weight) Read(int i)
        {
            var p=Rotate(points[i],cosine,sine);float distance=MathF.Abs(p.Y-centerY);
            float weight=distance<=twice?(distance<=height?1f:.7f):.3f;
            if(p.X<state.Min)state.Min=p.X;if(state.Max<p.X)state.Max=p.X;
            return(p.X,weight);
        }
        int at=0;
        for(;at+3<points.Length;at+=4)
        {
            token.ThrowIfCancellationRequested();var a=Read(at);var b=Read(at+1);var c=Read(at+2);var d=Read(at+3);
            double av=(double)a.Weight*a.X,bv=(double)b.Weight*b.X,cv=(double)c.Weight*c.X,dv=(double)d.Weight*d.X;
            state.Count=(double)d.Weight+c.Weight+b.Weight+a.Weight+state.Count;
            state.Sum=state.Sum+av+bv+cv+dv;
            state.Sum2=dv*d.X+cv*c.X+bv*b.X+av*a.X+state.Sum2;
        }
        for(;at<points.Length;at++)
        {
            token.ThrowIfCancellationRequested();var p=Read(at);double v=(double)p.Weight*p.X;
            state.Count=(double)p.Weight+state.Count;state.Sum=state.Sum+v;state.Sum2=v*p.X+state.Sum2;
        }
    }
    internal static LegacyOrdinaryHorizontalGeometry Horizontal(LegacyOrdinaryGeometryCharacterState[] characters,
        LegacyOrdinaryGeometryView[] views,LegacyOrdinaryGeometryGuide guide,LegacyOrdinaryVerticalGeometry vertical,
        double cosine,double sine,LegacyOrdinaryGeometryScratch[] scratch,LegacyOrdinaryGeometryBoundary[] boundaries,
        Action<int,uint,string> write,Action<bool> setReversed,Action<float?,float?> setStatistics,
        Action<string> stage,CancellationToken token)
    {
        if(scratch.Length!=characters.Length||boundaries.Length!=characters.Length+1)throw new ArgumentException("Complete phase-owned scratch/boundaries required.");
        float height=(float)guide.Height;stage("04D2C0");
        for(int i=0;i<scratch.Length;i++)
        {
            token.ThrowIfCancellationRequested();var c=characters[i];var s=scratch[i];s.GapClass=(c.Selected&0x28000)!=0;s.Written|=1;
            var sourceViews=c.Views;
            if(sourceViews.Length==0)
                throw new NotSupportedException("CHAR has no actual original source geometry.");
            s.Written|=2;
            foreach(var v in sourceViews)
            {
                if(v.Start<s.TimeMin)s.TimeMin=v.Start;if(s.TimeMax<v.Start)s.TimeMax=v.Start;
                //C4B reads chunks<=20.20 is a multiple of the native unroll4.
                if(v.Actual.Kind==LegacyGlobalRawCurveViewKind.FractionalCropC4B)
                {
                    for(int first=0;first<v.Points.Length;first+=20)
                        WeightedMoments(s,v.Points.AsSpan(first,Math.Min(20,v.Points.Length-first)).ToArray(),
                            vertical.MeanY,height,cosine,sine,token);
                }
                else WeightedMoments(s,v.Points,vertical.MeanY,height,cosine,sine,token);
            }
            s.Written|=4;
            if(s.Count<=0)throw new NotSupportedException("Positive actual ordinary horizontal moments required.");
            s.Mean=s.Sum/s.Count;
            double variance=(s.Sum2*s.Count-s.Sum*s.Sum)/(s.Count*s.Count);
            s.Std=variance>0?Math.Sqrt(variance):0;
            double floor=(double)height*.05;if(s.Std<floor)s.Std=floor;
            s.Written|=8;
            Check(s.Min,s.Max,(float)s.Mean,(float)s.Std);
        }
        var order=BuildOrdinaryOrder(scratch,out bool reversed,token);
        setReversed(reversed);
        for(int p=0;p<order.Length;p++){token.ThrowIfCancellationRequested();boundaries[p]=new(null,order[p]);}
        boundaries[^1]=new(null,order[^1]);
        for(int p=0;p<order.Length;p++)
        {
            token.ThrowIfCancellationRequested();int i=order[p];float x=p==0?
                Left(scratch[i],height,characters[i].Flags,characters[i].Selected):
                (float)Gaussian(scratch[order[p-1]].Mean,scratch[order[p-1]].Std,scratch[i].Mean,scratch[i].Std);
            boundaries[p]=new(x,i);
        }
        boundaries[^1]=new(Right(scratch[order[^1]],height,characters[order[^1]].Flags,characters[order[^1]].Selected),order[^1]);
        foreach(var b in boundaries)Check(b.Coordinate!.Value);
        //Native copy scratch64 -> CHAR80 remains insertion order, NOT boundary order.
        for(int i=0;i<characters.Length;i++)
        {
            token.ThrowIfCancellationRequested();
            var c=characters[i];var s=scratch[i];c.Min=s.Min;c.Max=s.Max;c.Mean=(float)s.Mean;c.Std=(float)s.Std;
            c.TimeMin=s.TimeMin;c.TimeMax=s.TimeMax;
        }
        stage("04E0E0");double sum=0,count=0;
        for(int p=0;p<order.Length;p++)
            if((characters[order[p]].Selected&0x7000)==0)
            {count+=1;sum+=(double)(boundaries[p+1].Coordinate!.Value-boundaries[p].Coordinate!.Value);}
        float meanWidth=count>0?(float)(sum/count):0;
        setStatistics(meanWidth,null);
        for(int p=0;p<order.Length;p++)
        {
            token.ThrowIfCancellationRequested();int i=order[p];var c=characters[i];
            if((c.Selected&0x7000)==0x1000)
            {
                float width=boundaries[p+1].Coordinate!.Value-boundaries[p].Coordinate!.Value;
                if(width>meanWidth*4f)write(i,c.Flags|0x02000000u,"04E0E0");
                //Native spacing eligibility excludes first/last WORD and WORD0.
                //This admitted producer has exactly that actual WORD0.
            }
        }
        float spacing=0;if(spacing<meanWidth*.1f)spacing=meanWidth*1.2f;
        setStatistics(null,spacing);
        //04DA20 scans real leading/trailing nonordinary sources. None may
        //silently be reclassified as ordinary after phase-owned flag writes.
        stage("04DA20");int leading=0,trailing=0;
        while(leading<characters.Length&&(characters[leading].Flags&0x200000)!=0)leading++;
        while(trailing<characters.Length&&(characters[^(trailing+1)].Flags&0x200000)!=0)trailing++;
        if(leading!=0||trailing!=0)throw new NotSupportedException("Reached04C0C0 nonordinary run service.");
        //04DD50's C25 property service is lazy behind actual >2 index gap
        //and differing bit24 categories. Keep its exact reached predicate.
        stage("04DD50");int previous=-1;uint category=0;
        for(int i=0;i<characters.Length;i++)
        {
            token.ThrowIfCancellationRequested();var c=characters[i];
            if((c.Flags&0x200000)==0||(c.Selected&0x400)!=0)
            {
                uint next=(c.Flags>>24)&1;
                if(previous!=-1&&previous+2<i&&category!=next)
                    throw new NotSupportedException("Reached native differing-category C25 gap service.");
                previous=i;category=next;
            }
        }
        Check(meanWidth,spacing);return new(boundaries,reversed,meanWidth,spacing);
    }
    internal static float Left(LegacyOrdinaryGeometryScratch s,float height,uint metadata,uint selected)
    {
        float margin=(metadata&0x1000000)==0&&(selected&0xc0000)!=0?.25f:.1f;
        float result=(float)(s.Mean-s.Std*2.8),limit=s.Min-margin*height;
        if(result<=limit)result=limit;
        if((selected&0x10000)!=0){float half=(float)s.Mean-height*.5f;if(!(result<=half))result=half;}
        return result;
    }
    internal static float Right(LegacyOrdinaryGeometryScratch s,float height,uint metadata,uint selected)
    {
        float margin=(metadata&0x1000000)==0&&(selected&0xc0000)!=0?.25f:.1f;
        float result=margin*height+s.Max,gaussian=(float)(s.Std*2.8+s.Mean);
        if(gaussian<=result)result=gaussian;
        if((selected&0x10000)!=0){float half=height*.5f+(float)s.Mean;if(result<=half)result=half;}
        return result;
    }
    internal static double Gaussian(double first,double firstStd,double second,double secondStd)
    {
        const double epsilon=1.1920928955078125e-7;
        if(epsilon<=Math.Abs(firstStd)&&epsilon<=Math.Abs(secondStd))
        {
            double v2=secondStd*secondStd,v1=firstStd*firstStd;
            double a=1/(v1+v1)-1/(v2+v2),b=second/v2-first/v1;
            double log=Math.Log(secondStd/firstStd);
            double discriminant=b*b-((first*first/(v1+v1)-second*second/(v2+v2))-log)*a*4;
            if(0<=discriminant&&epsilon<=Math.Abs(a))
            {
                double root=Math.Sqrt(discriminant),x=(root-b)/(a+a);
                if(first<x&&x<second)return x;
                x=(-b-root)/(a+a);if(first<x&&x<second)return x;
            }
        }
        return(first+second)*.5;
    }
    internal static int[] BuildOrdinaryOrder(LegacyOrdinaryGeometryScratch[] rows,out bool reversed,CancellationToken token)
    {
        //Exact all-ordinary (no skipped scratch+20)04E480 chain branches.
        //Integer sentinels stay integers despite Ghidra's -NAN type guesses.
        int direction=0;
        for(int i=1;i<rows.Length;i++)
        {
            if(rows[i-1].Mean+rows[i-1].Std*(double).8f<rows[i].Mean){direction=1;break;}
            if(rows[i].Mean<rows[i-1].Mean-rows[i-1].Std*(double).8f){direction=-1;break;}
        }
        if(direction==0){reversed=false;return Enumerable.Range(0,rows.Length).ToArray();}
        int previous=-1,oldDirection=0,bridge=-1,low=-1,high=-1;
        for(int i=0;i<rows.Length;i++)
        {
            token.ThrowIfCancellationRequested();if(previous==-1){previous=i;continue;}
            var a=rows[previous];double current=rows[i].Mean;
            if(a.Mean+a.Std*(double).8f<current)direction=1;
            else if(current<a.Mean-a.Std*(double).8f)direction=-1;
            bool same=direction==oldDirection;
            if(!same&&direction<0&&oldDirection!=0&&low!=-1&&rows[low].Mean-rows[low].Std*(double).8f<=current)
            {direction=oldDirection;same=true;}
            else if(!same&&direction>=0&&oldDirection!=0&&high!=-1&&rows[high].Mean-rows[high].Std*(double).8f<=current)
            {direction=oldDirection;same=true;}
            if(direction<0)
            {
                if(!same){bridge=low;low=-1;oldDirection=direction;}
                if(bridge==-1){rows[i].Next=previous;rows[previous].Previous=i;}
                else{rows[i].Next=bridge;rows[bridge].Previous=i;bridge=-1;}
                high=previous;direction=oldDirection;
            }
            else
            {
                if(!same){low=previous;bridge=high;oldDirection=direction;}
                rows[i].Previous=previous;rows[previous].Next=i;
                if(bridge!=-1){rows[i].Next=bridge;rows[bridge].Previous=i;}
                high=previous;direction=oldDirection;
            }
            previous=i;
        }
        int head=-1,tail=-1;
        for(int i=0;i<rows.Length;i++)
        {
            if(rows[i].Previous==-1){if(head!=-1){reversed=false;return Enumerable.Range(0,rows.Length).ToArray();}head=i;}
            if(rows[i].Next==-1){if(tail!=-1){reversed=false;return Enumerable.Range(0,rows.Length).ToArray();}tail=i;}
        }
        if(head==-1||tail==-1){reversed=false;return Enumerable.Range(0,rows.Length).ToArray();}
        reversed=tail<head;int at=head;var order=new int[rows.Length];var seen=new bool[rows.Length];
        for(int i=0;i<order.Length;i++)
        {
            if((uint)at>=(uint)rows.Length||seen[at]){reversed=false;return Enumerable.Range(0,rows.Length).ToArray();}
            order[i]=at;seen[at]=true;at=rows[at].Next;
        }
        return order;
    }
    private static void Check(params float[] values)
    {if(values.Any(x=>!float.IsFinite(x)))throw new NotSupportedException("Numerical producer left its admitted finite domain.");}
}
