using System.Numerics;

namespace InkRecognition;

internal sealed partial class LegacyMathAkOriginalInputContext
{
    //Created only by Run13A0 from the live same-prefix context. No raw-array
    //constructor or external callback can issue this plan or its output frame.
    private sealed class Source13A0SplinePlan
    {
        private readonly LegacyMathAkOriginalInputContext owner;
        private readonly LegacyMathAkProcessedStrokePrefix.OperatorSourceRow source;
        private readonly int ordinal,inputOffset,mode,uniqueCount;
        private readonly Vector2[] unique;
        private readonly int[] accepted;
        private readonly double[] lengths;
        private readonly LegacyCubicCoefficients[] x,y;
        private readonly double threshold;
        internal int OutputCount {get;private set;}
        private bool counting;
        private int written;
        private Source13A0Budget? budget;
        private Vector2[]? outputPoints;
        private int[]? outputIndices;
        internal Source13A0SplinePlan(LegacyMathAkOriginalInputContext owner,
            LegacyMathAkProcessedStrokePrefix.OperatorSourceRow source,int ordinal,int inputOffset,
            float weight,float scale,float flatness,CancellationToken token)
        {
            owner.Live();source.Validate(owner.prefix!,ordinal);token.ThrowIfCancellationRequested();
            this.owner=owner;this.source=source;this.ordinal=ordinal;this.inputOffset=inputOffset;
            var row=source.Points;
            //Native172B48..172B51: unsigned(rawCount-3)<=0x1ffd.
            //Outside3..8192, copy original points/keys; do not deduplicate,
            //fit, or evaluate a spline-only threshold on a bypassed row.
            if(row.Length<3||row.Length>8192)
            {unique=[];accepted=[];lengths=[];x=[];y=[];mode=0;return;}
            unique=new Vector2[row.Length];accepted=new int[row.Length];
            int n=1;unique[0]=row[0];accepted[0]=0;
            for(int i=1;i<row.Length;i++)
            {
                token.ThrowIfCancellationRequested();
                if(!LegacyLineOrientationProjection.Approximately(row[i].X,unique[n-1].X)||
                    !LegacyLineOrientationProjection.Approximately(row[i].Y,unique[n-1].Y))
                {unique[n]=row[i];accepted[n++]=i;}
                else if(i==row.Length-1)accepted[n-1]=i;
            }
            uniqueCount=n;
            lengths=[];x=[];y=[];
            if(n<2){mode=0;return;}
            if(n<4){mode=1;return;}
            float t=scale*flatness*scale;Finite(t);threshold=t;
            mode=2;lengths=new double[n-1];double arc=0;
            var valuesX=new float[n];var valuesY=new float[n];
            for(int i=0;i<n;i++)
            {
                token.ThrowIfCancellationRequested();valuesX[i]=unique[i].X;valuesY[i]=unique[i].Y;
                if(i+1<n)
                {
                    float dx=unique[i+1].X-unique[i].X,dy=unique[i+1].Y-unique[i].Y;Finite(dx);Finite(dy);
                    lengths[i]=Math.Sqrt((double)dx*dx+(double)dy*dy);arc+=lengths[i];
                }
            }
            double penalty=(double)((float)n*weight*weight*weight*weight*scale*scale*scale*scale)/arc;
            x=LegacyCubicStrokeSmoothing.FitOwnedMathSource(valuesX,lengths,penalty,token);
            y=LegacyCubicStrokeSmoothing.FitOwnedMathSource(valuesY,lengths,penalty,token);
        }
        private void Authenticate(CancellationToken token)
        {owner.Live();source.Validate(owner.prefix!,ordinal);token.ThrowIfCancellationRequested();}
        internal void Count(Source13A0Budget budget,CancellationToken token)
        {
            Authenticate(token);if(OutputCount!=0||this.budget is not null)throw new InvalidOperationException("Spline plan already counted.");
            this.budget=budget;counting=true;written=0;Visit(token);OutputCount=written;
            if(OutputCount<1)throw new InvalidOperationException("Empty original-source spline row.");
            this.budget=null;
        }
        internal void Fill(Vector2[] points,int[] originalIndices,Source13A0Budget budget,CancellationToken token)
        {
            Authenticate(token);
            if(OutputCount<1||points.Length!=OutputCount||originalIndices.Length!=OutputCount||this.budget is not null)
                throw new InvalidOperationException("Exact counted spline output required.");
            this.budget=budget;counting=false;written=0;outputPoints=points;outputIndices=originalIndices;
            try{Visit(token);if(written!=OutputCount)throw new InvalidOperationException("Spline output differs from count.");}
            finally{this.budget=null;outputPoints=null;outputIndices=null;}
        }
        private void Emit(Vector2 point,int key,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();Finite(point.X);Finite(point.Y);
            int local=key-inputOffset;
            if((uint)local>=(uint)source.Length)throw new InvalidOperationException("Smoothing key escaped original current row.");
            if(counting)budget!.CountOutput();
            else
            {
                if((uint)written>=(uint)OutputCount)throw new InvalidOperationException("Spline fill exceeds exact row reservation.");
                ReadOnlySpan<int> map=owner.prefix!.ReadOperatorOriginalIndexRow(ordinal);
                if(map.Length!=source.Length)throw new InvalidOperationException("Same-prefix point map changed.");
                int original=map[local];
                if((uint)original>=(uint)owner.input.PointCount)throw new InvalidOperationException("Foreign original point identity.");
                outputPoints![written]=point;outputIndices![written]=original;
            }
            written=checked(written+1);
        }
        private void Visit(CancellationToken token)
        {
            if(mode==0)
            {
                var raw=source.Points;
                for(int i=0;i<raw.Length;i++)Emit(raw[i],inputOffset+i,token);
            }
            else if(mode==1)
            {for(int i=0;i<uniqueCount;i++)Emit(unique[i],inputOffset+accepted[i],token);}
            else
            {
                for(int i=0;i<lengths.Length;i++)
                {
                    token.ThrowIfCancellationRequested();Curve curve=new(ToBezier(x[i],lengths[i]),ToBezier(y[i],lengths[i]));
                    if(i==0)Emit(new((float)curve.X.A,(float)curve.Y.A),inputOffset+accepted[0],token);
                    Walk(curve,0f,1f,0,false,inputOffset+accepted[i+1],token);
                }
            }
        }
        private readonly record struct Axis(double A,double B,double C,double D)
        {
            internal (Axis Left,Axis Right) Split()
            {
                double first=(A+B)*.5,middle=(B+C)*.5,last=(C+D)*.5;
                double left=(middle+first)*.5,right=(last+middle)*.5,center=(right+left)*.5;
                return(new(A,first,left,center),new(center,right,last,D));
            }
        }
        private readonly record struct Curve(Axis X,Axis Y)
        {
            internal (Curve Left,Curve Right) Split()
            {var xx=X.Split();var yy=Y.Split();return(new(xx.Left,yy.Left),new(xx.Right,yy.Right));}
        }
        private static Axis ToBezier(LegacyCubicCoefficients c,double h)
        {
            var axis=new Axis(c.Value,(c.Linear*h)/3+c.Value,
                ((c.Linear+c.Linear+c.Quadratic*h)*h)/3+c.Value,
                ((h*c.Cubic+c.Quadratic)*h+c.Linear)*h+c.Value);
            if(!double.IsFinite(axis.A)||!double.IsFinite(axis.B)||!double.IsFinite(axis.C)||!double.IsFinite(axis.D))
                throw new NotSupportedException("Original-source Bezier outside finite scalar subset.");
            return axis;
        }
        private static double Distance(double px,double py,double x0,double y0,double x1,double y1)
        {
            double dx=px-x0,dy=py-y0,sx=x1-x0,sy=y1-y0,dot=dy*sy+dx*sx;
            if(dot<=0)return dy*dy+dx*dx;
            double squared=sy*sy+sx*sx;
            if(squared<=dot)return(py-y1)*(py-y1)+(px-x1)*(px-x1);
            double cross=dx*sy-dy*sx;return cross*cross/squared;
        }
        private void Walk(Curve curve,float from,float to,int depth,bool fractional,int next,CancellationToken token)
        {
            if(counting)budget!.CountNode(token);else budget!.FillNode(token);
            if(depth>=11||(Distance(curve.X.B,curve.Y.B,curve.X.A,curve.Y.A,curve.X.D,curve.Y.D)<threshold&&
                Distance(curve.X.C,curve.Y.C,curve.X.A,curve.Y.A,curve.X.D,curve.Y.D)<threshold))
            {
                //Actual173B90 multiplier is1. Leftsubtree171F00 rounds the
                //F32 value(next-1+to); right1722D0 uses the exact endpoint.
                float raw=(float)(next-1)+to;Finite(raw);
                int key=fractional?(int)MathF.Round(raw,MidpointRounding.AwayFromZero):next;
                Emit(new((float)curve.X.D,(float)curve.Y.D),key,token);return;
            }
            var pair=curve.Split();float middle=(from+to)*.5f;
            Walk(pair.Left,from,middle,depth+1,true,next,token);
            Walk(pair.Right,middle,to,depth+1,fractional,next,token);
        }
    }
}
