namespace InkRecognition;
internal sealed partial class LegacyMathAkOriginalInputContext
{
//Arithmetic preimage: LegacyCursiveLoopCuts.cs; actual Math/Text instruction correspondence is frozen separately.
//ActualMath071830; numerical preimage Text18E1F0.
private static class SamplerLoop
{


    internal static SamplerFrame Apply(SamplerFrame ink, int strokeIndex,
        float lineHeight, float reversalSpacing, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ink);
        cancellationToken.ThrowIfCancellationRequested();
        if((uint)strokeIndex >= (uint)ink.StrokePointCounts.Length)throw new ArgumentOutOfRangeException(nameof(strokeIndex));
        if(!float.IsFinite(lineHeight) || lineHeight < 0 || !float.IsFinite(reversalSpacing) || reversalSpacing < 0 ||
            !float.IsFinite(lineHeight*reversalSpacing))throw new ArgumentException("Loop spacing requires finite nonnegative values.");
        int start=0;
        for(int stroke=0;ink.Test(stroke<strokeIndex,cancellationToken);stroke++)start+=ink.StrokePointCounts.Span[stroke];
        var records=ink.CopyRecords();
        //Exact finite-coordinate fast path. With strictly monotone X, every
        //nonadjacent closed segment interval is disjoint in X, so the original
        //BoxesOverlap gate is false for every candidate pair. No tag, point,
        //source map or insertion can change. Equality/reversal takes the full
        //original loop below. This is a portable optimization, not a native
        //instruction/cancellation/failure-prefix identity claim.
        int monotoneEnd=checked(start+ink.StrokePointCounts.Span[strokeIndex]);
        int monotoneDirection=0;bool strictX=true;
        for(int probe=start+1;ink.Test(probe<monotoneEnd,cancellationToken);probe++)
        {
            float previousX=records[probe-1].X,currentX=records[probe].X;
            int stepDirection=currentX>previousX?1:currentX<previousX?-1:0;
            if(stepDirection==0||(monotoneDirection!=0&&stepDirection!=monotoneDirection))
            {strictX=false;break;}
            monotoneDirection=stepDirection;
        }
        if(strictX)return ink;
        for(int i=start;ink.Test(i<start+ink.StrokePointCounts.Span[strokeIndex]-1,cancellationToken);i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int end=start+ink.StrokePointCounts.Span[strokeIndex];
            var a=records[i];var b=records[i+1];
            for(int j=i+2;ink.Test(j<end-1,cancellationToken);j++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var c=records[j];var d=records[j+1];
                if(!BoxesOverlap(a,b,c,d) || !TryIntersection(new(a.X,a.Y),new(b.X,b.Y),new(c.X,c.Y),new(d.X,d.Y),out var intersection))continue;
                int xDirection=0,yDirection=0,xReversals=0,yReversals=0;
                float lastX=-1,lastY=-1,turn=0;
                for(int k=i;ink.Test(k<j,cancellationToken);k++)
                {
                    var p=records[k];var next=records[k+1];
                    if(p.SegmentLength>0 && next.SegmentLength>0)
                        turn+=(p.DeltaX*next.DeltaY-p.DeltaY*next.DeltaX)/(p.SegmentLength*next.SegmentLength);
                    Count(p.DeltaX,p.X,lineHeight*reversalSpacing,ref xDirection,ref lastX,ref xReversals);
                    Count(p.DeltaY,p.Y,lineHeight*reversalSpacing,ref yDirection,ref lastY,ref yReversals);
                }
                if(xReversals>2 || yReversals>2)continue;
                //Every growth is charged cumulatively inside the private Insert.
                //18df60 reserves two points and inserts the same intersection
                // before old i+1 and old j+1. End-first insertion keeps both
                // predecessor source-map identities exactly as its memmoves.
                ink=ink.Insert(strokeIndex,j+1-start,intersection,12,cancellationToken)
                    .Insert(strokeIndex,i+1-start,intersection,11,cancellationToken);
                records=ink.CopyRecords();
                int innerStart=i+2,innerEnd=j+2;
                bool nested=false;
                for(int k=innerStart;ink.Test(k<innerEnd,cancellationToken);k++)if(IsLoopTag(records[k].Tag)){nested=true;break;}
                for(int k=innerStart;ink.Test(k<innerEnd,cancellationToken);k++)
                {
                    int old=records[k].Tag,nextTag=old;
                    if(turn<0)
                    {
                        if(old==0 || !nested && (old is 7 or 8))nextTag=13;
                    }
                    else if(nested)
                    {
                        if(old==0)nextTag=14;
                    }
                    else nextTag=old==7 ? 9:14;
                    records[k]=records[k] with{Tag=nextTag};
                }
                ink=ink.WithTags(ink.CopyTags(records));
                // Native advances initial point by two after the double insert,
                // then the outer loop increments once more.
                i+=2;
                break;
            }
        }
        return ink;
    }






    internal static bool TryIntersection(InkPoint firstStart,InkPoint firstEnd,InkPoint secondStart,
        InkPoint secondEnd,out InkPoint point)
    {
        point=default;
        Validate(firstStart);Validate(firstEnd);Validate(secondStart);Validate(secondEnd);
        float a=firstEnd.X-firstStart.X,b=secondEnd.X-secondStart.X;
        float c=firstStart.Y-firstEnd.Y,d=secondStart.Y-secondEnd.Y;
        float e=-c*firstStart.X-a*firstStart.Y,g=-d*secondStart.X-b*secondStart.Y;
        float determinant=b*c-d*a;
        if(!(Math.Abs(determinant)>=1e-10f))return false;
        float x=(g*a-b*e)/determinant,y=(d*e-g*c)/determinant;
        point=new(x,y);
        return Inside(x,firstStart.X,firstEnd.X) && Inside(y,firstStart.Y,firstEnd.Y) &&
            Inside(x,secondStart.X,secondEnd.X) && Inside(y,secondStart.Y,secondEnd.Y);
    }

    private static bool Inside(float value,float first,float second)=>Math.Min(first,second)<=value && value<=Math.Max(first,second);
    private static void Validate(InkPoint p)
    {
        if(!float.IsFinite(p.X) || !float.IsFinite(p.Y))throw new ArgumentException("Intersection requires finite coordinates.");
    }
    private static bool IsLoopTag(int tag)=>unchecked((uint)(tag-11))<4;
    private static bool BoxesOverlap(LegacyCursivePreparedPoint a,LegacyCursivePreparedPoint b,
        LegacyCursivePreparedPoint c,LegacyCursivePreparedPoint d)=>
        Math.Min(a.X,b.X)<=Math.Max(c.X,d.X) && Math.Min(c.X,d.X)<=Math.Max(a.X,b.X) &&
        Math.Min(a.Y,b.Y)<=Math.Max(c.Y,d.Y) && Math.Min(c.Y,d.Y)<=Math.Max(a.Y,b.Y);

    private static void Count(float delta,float coordinate,float minimumDistance,ref int direction,
        ref float previousPosition,ref int count)
    {
        if(delta>0)
        {
            if(direction>0 && (previousPosition==-1 || minimumDistance<Math.Abs(coordinate-previousPosition)))
            {previousPosition=coordinate;count++;}
            direction=-1;
        }
        else if(delta<0)
        {
            if(direction<0 && (previousPosition==-1 || minimumDistance<Math.Abs(coordinate-previousPosition)))
            {previousPosition=coordinate;count++;}
            direction=1;
        }
    }
}

//Arithmetic preimage: LegacyCursiveHorizontalCuts.cs; actual Math/Text instruction correspondence is frozen separately.
//ActualMath071FA0; numerical preimage Text18E960, explicit actual policy.
private static class SamplerHorizontal
{
    internal static SamplerFrame Apply(SamplerFrame ink, int strokeIndex,
        float lineHeight, LegacyCursiveHorizontalCutSettings settings, bool reverseX = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ink);
        cancellationToken.ThrowIfCancellationRequested();
        if((uint)strokeIndex >= (uint)ink.StrokePointCounts.Length)throw new ArgumentOutOfRangeException(nameof(strokeIndex));
        if(!float.IsFinite(lineHeight) || lineHeight < 0)throw new ArgumentOutOfRangeException(nameof(lineHeight));
        ink.Reserve(24,6);
        float[] parameters=[settings.LooseSlope,settings.StrongSlope,settings.MinimumArc,settings.LongArc,settings.GapRadius,settings.ReversalSpacing];
        foreach(float p in parameters)if(!float.IsFinite(p) || p<0 || !float.IsFinite(p*lineHeight))throw new ArgumentException("Cut settings require finite nonnegative values.",nameof(settings));
        if(settings.LooseSlope==0 || settings.StrongSlope==0)throw new ArgumentException("Slope denominators must be positive.",nameof(settings));
        ink.Reserve(48,1);
        var state=new State(ink,strokeIndex,cancellationToken);
        bool groupActive=false,strong=false,inLoop=false;
        int groupStart=0;
        for(int i=state.Start;state.Test(i<state.End-1);i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var p=state.Record(i);int tag=state.Tag(i);
            float dx=reverseX ? -p.DeltaX : p.DeltaX;
            bool close;
            if(tag is >=11 and <=14 && tag!=12)
            {
                inLoop=true;close=true;
            }
            else
            {
                if(tag==12)inLoop=false;
                if(inLoop)close=true;
                else
                {
                    close=dx<=Math.Abs(p.DeltaY)/settings.LooseSlope || state.Tag(i+1)==11 || state.End-2<=i;
                    if(!close)
                    {
                        if(Math.Abs(p.DeltaY/settings.StrongSlope)<dx)strong=true;
                        if(!groupActive){groupActive=true;groupStart=i;}
                    }
                }
            }
            if(!close)continue;
            if(groupActive && strong)
            {
                int end=i==state.End-2 ? i+1 : i;
                float arc=state.Length(groupStart,end);
                int sevens=0,eights=0;
                for(int j=groupStart;state.Test(j<=end);j++){sevens+=state.Tag(j)==7 ? 1:0;eights+=state.Tag(j)==8 ? 1:0;}
                bool before=true,after=true;
                if(sevens+eights==1)
                {
                    before=false;after=false;
                    for(int j=state.Start+1;state.Test(j<=end);j++)if(state.Tag(j)!=0){before=true;break;}
                    for(int j=end;state.Test(j<state.End);j++)if(state.Tag(j)!=0){after=true;break;}
                }
                if(lineHeight*settings.MinimumArc<arc && before && after)
                {
                    bool preserveReversals=false;
                    if(sevens==0 && eights==0)
                        General(state,groupStart,ref end,lineHeight*settings.GapRadius);
                    else if(sevens==0 || eights==0)
                        SingleDirection(state,groupStart,ref end,arc,sevens!=0 ? 7:8,lineHeight,settings,ref preserveReversals);
                    else
                    {
                        float displacement=state.Record(end).X-state.Record(groupStart).X;
                        if(reverseX)displacement=-displacement;
                        Mixed(state,groupStart,ref end,displacement*settings.ReversalSpacing);
                        preserveReversals=true;
                    }
                    if(!settings.KeepOneAnchor)
                    {
                        uint mask=preserveReversals ? 0x19f2u:0x1872u;
                        for(int j=groupStart;state.Test(j<=end);j++)
                        {
                            uint value=unchecked((uint)state.Tag(j));
                            if(value>12 || (mask>>((int)value&31)&1)==0)state.SetTag(j,0);
                        }
                    }
                    else
                    {
                        bool kept=false;
                        for(int j=end-1;state.Test(j>=groupStart);j--)
                        {
                            int value=state.Tag(j);
                            if(value is >=4 and <=6){if(kept)state.SetTag(j,0);else kept=true;}
                            else if(value!=1 && value!=11 && value!=12)state.SetTag(j,0);
                        }
                    }
                }
            }
            groupActive=false;
        }
        return state.Finish();
    }

    private static void SingleDirection(State state,int start,ref int end,float arc,int reversal,
        float height,LegacyCursiveHorizontalCutSettings settings,ref bool preserve)
    {
        float gap=0;int cursor=end;
        if(end<state.End-1)
        {
            while(state.Test(cursor<state.End-1))
            {
                gap+=state.Record(cursor).SegmentLength;
                int next=state.Tag(cursor+1);
                if(next!=0 && !Loop(next))
                {
                    if(gap<=height*settings.GapRadius)preserve=true;
                    else
                    {
                        var tail=AfterLastReversal(state,start,end,reversal);
                        preserve=tail.Index==end;
                        if(tail.Index<end)
                        {
                            int segment=state.InsertAtArc(tail.Index,end,tail.Half,reversal==7 ? 4:6);
                            if(segment>=0){end++;cursor=segment;}
                        }
                    }
                    break;
                }
                cursor++;
            }
        }
        if(cursor==state.End-1)
        {
            if(height*settings.GapRadius<gap)
            {
                var tail=AfterLastReversal(state,start,end,reversal);
                if(tail.Index!=end)
                {
                    if(tail.Index<end && state.InsertAtArc(tail.Index,end,tail.Half,reversal==7 ? 4:6)>=0)end++;
                }
                else preserve=true;
            }
            else preserve=true;
        }
        if(height*settings.LongArc<arc)
        {
            float distance=0;
            for(int i=start;state.Test(i<end);i++)
            {
                distance+=state.Record(i).SegmentLength;
                if(state.Tag(i+1)==reversal)break;
            }
            if(distance<=arc*.25f || arc*.75f<=distance)
            {
                if(state.InsertAtArc(start,end,arc*.5f,reversal==7 ? 4:6)>=0)end++;
            }
            else preserve=true;
        }
    }

    private static (int Index,float Half) AfterLastReversal(State state,int start,int end,int reversal)
    {
        int last=-1;float length=0;
        for(int i=start;state.Test(i<=end);i++)
        {
            if(state.Tag(i)==reversal){length=0;last=i;}
            length+=state.Record(i).SegmentLength;
        }
        return(last,(length-state.Record(end).SegmentLength)*.5f);
    }

    private static void Mixed(State state,int start,ref int end,float spacing)
    {
        int last=end;float tail=0;bool seven=true;
        for(;state.Test(last>=start);last--)
        {
            if(state.Tag(last)==8){seven=false;break;}
            if(state.Tag(last)==7)break;
            tail+=state.Record(last-1).SegmentLength;
        }
        state.SetTag(last,0);
        if(last==end)state.SetTag(last,seven ? 4:6);
        else if(state.InsertAtArc(last,end,tail*.5f,seven ? 4:6)>=0)end++;
        for(int i=start;state.Test(i<=end);i++)if(state.Tag(i)==8)state.SetTag(i,0);
        bool first=true;float accumulated=0;
        for(int i=start;state.Test(i<=end);i++)
        {
            if(state.Tag(i)==7)
            {
                if(first)first=false;
                else if(accumulated<spacing){state.SetTag(i,0);accumulated+=state.Record(i).SegmentLength;continue;}
                accumulated=0;
            }
            accumulated+=state.Record(i).SegmentLength;
        }
    }

    private static void General(State state,int start,ref int end,float radius)
    {
        var first=state.Record(start);var final=state.Record(end);
        float chord=Hypot(final.X-first.X,final.Y-first.Y),minimum=0,maximum=0;
        int negative=-1,positive=-1;
        for(int i=start+1;state.Test(i<=end);i++)
        {
            var p=state.Record(i);float distance=Hypot(p.X-first.X,p.Y-first.Y);
            if(distance==0)continue;
            float cross=(((p.X-first.X)/distance)*((final.Y-first.Y)/chord)-((p.Y-first.Y)/distance)*((final.X-first.X)/chord))*distance;
            if(minimum<=cross){if(maximum<cross){maximum=cross;positive=i;}}
            // Native18fd60 uses COMISS/JBE: an unordered chord comparison does
            // not update either extremum, unlike a naive decompiler else.
            else if(cross<minimum){minimum=cross;negative=i;}
        }
        int left=start,right=end;
        if(negative<0){if(positive>=0)left=right=positive;}
        else if(positive<0)left=right=negative;
        else{left=Math.Min(negative,positive);right=Math.Max(negative,positive);}
        float before=0,after=0;
        for(int i=left-1;state.Test(i>=state.Start);i--){before+=state.Record(i).SegmentLength;if(state.Tag(i)!=0 && !Loop(state.Tag(i)))break;}
        for(int i=right;state.Test(i<state.End-1);i++){after+=state.Record(i).SegmentLength;if(state.Tag(i+1)!=0 && !Loop(state.Tag(i+1)))break;}
        if(left==right)
        {
            if(radius<before && radius<after)state.SetTag(left,5);
            return;
        }
        float half=state.Length(left,right)*.5f,walked=0;
        for(int i=left;state.Test(i<right);i++)
        {
            var p=state.Record(i);walked+=p.SegmentLength;
            if(walked<=half)continue;
            float fraction=(half-(walked-p.SegmentLength))/p.SegmentLength;
            float x=fraction*p.DeltaX+p.X,y=fraction*p.DeltaY+p.Y;
            float firstPart=Hypot(x-p.X,y-p.Y);
            if(radius<firstPart+(walked-p.SegmentLength)+before)
            {
                var next=state.Record(i+1);
                float remainder=Hypot(x-next.X,y-next.Y);
                for(int j=i+1;state.Test(j<right);j++)remainder+=state.Record(j).SegmentLength;
                if(radius<after+remainder){state.Insert(i+1,new(x,y),5);end++;}
            }
            break;
        }
    }

    private static bool Loop(int tag)=>unchecked((uint)(tag-11))<4;
    private static float Hypot(float x,float y)=>(float)Math.Sqrt((double)y*y+(double)x*x);

    private sealed class State
    {
        private SamplerFrame ink;
        private readonly int stroke;
        private readonly CancellationToken token;
        public bool Test(bool condition)=>ink.Test(condition,token);
        private LegacyCursivePreparedPoint[] records;
        private int[] tags;
        public int Start { get; }
        public int End=>Start+ink.StrokePointCounts.Span[stroke];
        public State(SamplerFrame ink,int stroke,CancellationToken token)
        {
            this.ink=ink;this.stroke=stroke;this.token=token;
            for(int i=0;ink.Test(i<stroke,token);i++)Start+=ink.StrokePointCounts.Span[i];
            records=ink.CopyRecords();tags=ink.CopyTags(records);
        }
        public LegacyCursivePreparedPoint Record(int i)=>records[i];
        public int Tag(int i)=>tags[i];
        public void SetTag(int i,int tag){ink.Reserve(0,1);tags[i]=tag;}
        public float Length(int start,int end)
        {
            float sum=0;for(int i=start;ink.Test(i<end,token);i++){token.ThrowIfCancellationRequested();sum+=records[i].SegmentLength;}return sum;
        }
        public void Insert(int global,InkPoint point,int tag)
        {
            token.ThrowIfCancellationRequested();ink=ink.WithTags(tags).Insert(stroke,global-Start,point,tag,token);
            records=ink.CopyRecords();tags=ink.CopyTags(records);
        }
        public int InsertAtArc(int start,int end,float target,int tag)
        {
            float distance=0;
            for(int i=start;ink.Test(i<end,token);i++)
            {
                distance+=records[i].SegmentLength;
                if(target<distance)
                {
                    float t=(target-(distance-records[i].SegmentLength))/records[i].SegmentLength;
                    Insert(i+1,new(t*records[i].DeltaX+records[i].X,t*records[i].DeltaY+records[i].Y),tag);return i;
                }
            }
            return -1;
        }
        public SamplerFrame Finish()=>ink.WithTags(tags);
    }
}

//Arithmetic preimage: LegacyCursiveBaselineCuts.cs; actual Math/Text instruction correspondence is frozen separately.
//ActualMath073990; numerical preimage Text190350.
private static class SamplerBaseline
{






    internal static SamplerFrame Apply(SamplerFrame ink,int strokeIndex,
        float? baseline,float lineHeight,float gapRadius,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(ink);
        cancellationToken.ThrowIfCancellationRequested();
        if((uint)strokeIndex>=(uint)ink.StrokePointCounts.Length)throw new ArgumentOutOfRangeException(nameof(strokeIndex));
        if(baseline.HasValue && !float.IsFinite(baseline.Value) || !float.IsFinite(lineHeight) || lineHeight<0 ||
            !float.IsFinite(gapRadius) || gapRadius<0 || !float.IsFinite(lineHeight*gapRadius))
            throw new ArgumentException("Baseline cuts require finite line metrics and radius.");
        if(baseline==float.MaxValue)baseline=null;
        int start=0;for(int s=0;ink.Test(s<strokeIndex,cancellationToken);s++)start+=ink.StrokePointCounts.Span[s];
        var records=ink.CopyRecords();var tags=ink.CopyTags(records);
        bool pending=false;int pendingPoint=0,anchor=start;
        for(int i=start;ink.Test(i<start+ink.StrokePointCounts.Span[strokeIndex],cancellationToken);i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int tag=tags[i];
            if(tag==12){pending=true;pendingPoint=i;continue;}
            if(tag==0 || Hidden(tag))continue;
            if(pending && !Hidden(tags[i-1]))
            {
                if(tag is 6 or 8)
                {
                    if(baseline.HasValue)
                    {
                        float y=baseline.Value;
                        if(tags[anchor]==9)
                        {
                            if(Math.Abs(records[anchor].Y-y)<Math.Abs(records[pendingPoint].Y-y))
                            {anchor=i;pending=false;continue;}
                            tags[anchor]=14;
                        }
                        if(records[pendingPoint].Y<=y)
                        {
                            float distance=0;
                            // Native sums this branch in reverse point order.
                            for(int j=i-1;ink.Test(j>=pendingPoint,cancellationToken);j--)distance+=records[j].SegmentLength;
                            if(lineHeight*gapRadius<distance)tags[pendingPoint]=2;
                        }
                        else
                        {
                            float distance=0;int candidate=i;
                            while(ink.Test(pendingPoint<candidate,cancellationToken))
                            {
                                distance+=records[candidate-1].SegmentLength;
                                if(lineHeight*gapRadius<distance)break;
                                candidate--;
                            }
                            if(records[candidate].Y<=y)
                            {
                                for(int j=pendingPoint;ink.Test(j<i,cancellationToken);j++)if(records[j].Y<y){tags[j]=2;break;}
                            }
                            else tags[candidate]=2;
                        }
                    }
                }
                else if(tag==11)
                {
                    float total=0;for(int j=pendingPoint;ink.Test(j<i,cancellationToken);j++)total+=records[j].SegmentLength;
                    float distance=0;
                    for(int j=pendingPoint;ink.Test(j<i,cancellationToken);j++)
                    {
                        distance+=records[j].SegmentLength;
                        if(total*.5f<distance)
                        {
                            float fraction=(total*.5f-(distance-records[j].SegmentLength))/records[j].SegmentLength;
                            var p=records[j];
                            var point=new InkPoint(fraction*p.DeltaX+p.X,fraction*p.DeltaY+p.Y);
                            ink=ink.WithTags(tags).Insert(strokeIndex,j+1-start,point,2,cancellationToken);
                            records=ink.CopyRecords();tags=ink.CopyTags(records);i++;
                            break;
                        }
                    }
                }
                else if((tag is 7 or 4) && records[i].X>records[pendingPoint].X)
                {
                    float distance=0;for(int j=pendingPoint;ink.Test(j<i,cancellationToken);j++)distance+=records[j].SegmentLength;
                    if(lineHeight<distance)tags[pendingPoint]=2;
                }
            }
            anchor=i;pending=false;
        }
        return ink.WithTags(tags);
    }

    private static bool Hidden(int tag)=>unchecked((uint)(tag-13))<2;
}

//Arithmetic preimage: LegacyCursiveControllerFilters.cs; actual Math/Text instruction correspondence is frozen separately.
//ActualMath074F90; numerical preimage Text191950 filter blocks.
private static class SamplerFilters
{

    internal static SamplerFrame BeforeBaseline(SamplerFrame ink,int strokeIndex,CancellationToken cancellationToken=default)
    {
        var (points,tags,start,end)=Prepare(ink,strokeIndex,cancellationToken);
        bool pending=false;int anchor=0;
        for(int i=start;ink.Test(i<end-1,cancellationToken);i++)
        {
            cancellationToken.ThrowIfCancellationRequested();int tag=tags[i];
            if(tag is 4 or 7){pending=true;anchor=i;continue;}
            if(tag is 0 or 13 or 14)continue;
            if(pending && tag==11 && points[anchor].Y-points[i].Y>0)
            {
                float dy=points[anchor].Y-points[i].Y,dx=points[i].X-points[anchor].X;
                for(int j=anchor+1;ink.Test(j<=i,cancellationToken);j++)
                {
                    float x=points[j].X-points[anchor].X;
                    bool outside=dx==0 ? x<0 : x==0 ? dx>0 : (points[anchor].Y-points[j].Y)/x>dy/dx;
                    if(outside){tags[i]=2;break;}
                }
            }
            pending=false;
        }
        pending=false;anchor=0;
        for(int i=start;ink.Test(i<end,cancellationToken);i++)
        {
            int tag=tags[i];
            if(tag==11)
            {
                if(!pending || i==0 || points[i-1].DeltaX<=0){pending=true;anchor=i;}
                else{if(anchor!=0)tags[anchor]=0;tags[i]=2;pending=false;anchor=0;}
            }
            else if(tag is 12 or 1)pending=false;
        }
        return ink.WithTags(tags);
    }

    internal static SamplerFrame AfterBaselineUnknown(SamplerFrame ink,int strokeIndex,float? baseline,float lineHeight,CancellationToken cancellationToken=default)
    {
        if(!float.IsFinite(lineHeight) || lineHeight<0 || baseline is float b && !float.IsFinite(b))throw new ArgumentException("Controller filters require finite nonnegative line height and explicit/unknown baseline.");
        var (points,tags,start,end)=Prepare(ink,strokeIndex,cancellationToken);
        int anchor=start;float anchorY=points[start].Y;
        for(int i=start+1;ink.Test(i<end,cancellationToken);i++)
        {
            cancellationToken.ThrowIfCancellationRequested();if(tags[i] is 0 or 1 or 13 or 14)continue;
            if(baseline is float known && known!=float.MaxValue && points[i].Y<0 && known+lineHeight*ink.Policy.HighAnchorMargin<anchorY &&
                points[anchor].X<points[i].X && tags[anchor] is 4 or 5 or 7)
            {
                for(int j=anchor;ink.Test(j<i,cancellationToken);j++)if(points[j].Y<known)
                {
                    // With a positive margin, the high anchor itself cannot satisfy this test.
                    tags[j-1]=2;tags[anchor]=0;break;
                }
            }
            anchor=i;anchorY=points[i].Y;
        }
        float previousY=points[start].Y;
        for(int i=start+1;ink.Test(i<end-1,cancellationToken);i++)
        {
            if(tags[i] is 0 or 1 or 13 or 14)continue;
            if(tags[i]==11 && tags[i+1]==13 && lineHeight*ink.Policy.LoopDrop<points[i].Y-previousY)tags[i]=2;
            previousY=points[i].Y;
        }
        return ink.WithTags(tags);
    }

    private static (LegacyCursivePreparedPoint[] Points,int[] Tags,int Start,int End) Prepare(SamplerFrame ink,int strokeIndex,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(ink);token.ThrowIfCancellationRequested();
        if((uint)strokeIndex>=(uint)ink.StrokePointCounts.Length)throw new ArgumentOutOfRangeException(nameof(strokeIndex));
        int start=0;for(int i=0;ink.Test(i<strokeIndex,token);i++)start+=ink.StrokePointCounts.Span[i];
        var points=ink.CopyRecords();return(points,ink.CopyTags(points),start,start+ink.StrokePointCounts.Span[strokeIndex]);
    }
}

}
