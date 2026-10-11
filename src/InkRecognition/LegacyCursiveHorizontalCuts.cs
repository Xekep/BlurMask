namespace InkRecognition;

/// <summary>Explicit native18e960 profile fields; no learned or guessed cut parameters.</summary>
public readonly record struct LegacyCursiveHorizontalCutSettings(float LooseSlope, float StrongSlope,
    float MinimumArc, float LongArc, float GapRadius, float ReversalSpacing, bool KeepOneAnchor)
{
    /// <summary>Serialized values shared by both Russian sampler297 version106 nodes.</summary>
    public static LegacyCursiveHorizontalCutSettings RussianVersion106 => new(.5f,.35f,.2f,1.8f,.35f,.25f,false);
}

/// <summary>
/// Restored18e960 prepared-ink horizontal segment stage. Input tags must come
/// from the preceding native geometry stages, including loop classification.
/// This is not a replacement for the complete191950 producer.
/// </summary>
public static class LegacyCursiveHorizontalCuts
{
    public static LegacyCursivePreparedInk Apply(LegacyCursivePreparedInk ink, int strokeIndex,
        float lineHeight, LegacyCursiveHorizontalCutSettings settings, bool reverseX = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ink);
        cancellationToken.ThrowIfCancellationRequested();
        if((uint)strokeIndex >= (uint)ink.StrokePointCounts.Length)throw new ArgumentOutOfRangeException(nameof(strokeIndex));
        if(!float.IsFinite(lineHeight) || lineHeight <= 0)throw new ArgumentOutOfRangeException(nameof(lineHeight));
        float[] parameters=[settings.LooseSlope,settings.StrongSlope,settings.MinimumArc,settings.LongArc,settings.GapRadius,settings.ReversalSpacing];
        foreach(float p in parameters)if(!float.IsFinite(p) || p<0 || !float.IsFinite(p*lineHeight))throw new ArgumentException("Cut settings require finite nonnegative values.",nameof(settings));
        if(settings.LooseSlope==0 || settings.StrongSlope==0)throw new ArgumentException("Slope denominators must be positive.",nameof(settings));
        var state=new State(ink,strokeIndex,cancellationToken);
        bool groupActive=false,strong=false,inLoop=false;
        int groupStart=0;
        for(int i=state.Start;i<state.End-1;i++)
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
                for(int j=groupStart;j<=end;j++){sevens+=state.Tag(j)==7 ? 1:0;eights+=state.Tag(j)==8 ? 1:0;}
                bool before=true,after=true;
                if(sevens+eights==1)
                {
                    before=false;after=false;
                    for(int j=state.Start+1;j<=end;j++)if(state.Tag(j)!=0){before=true;break;}
                    for(int j=end;j<state.End;j++)if(state.Tag(j)!=0){after=true;break;}
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
                        for(int j=groupStart;j<=end;j++)
                        {
                            uint value=unchecked((uint)state.Tag(j));
                            if(value>12 || (mask>>((int)value&31)&1)==0)state.SetTag(j,0);
                        }
                    }
                    else
                    {
                        bool kept=false;
                        for(int j=end-1;j>=groupStart;j--)
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
            while(cursor<state.End-1)
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
            for(int i=start;i<end;i++)
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
        for(int i=start;i<=end;i++)
        {
            if(state.Tag(i)==reversal){length=0;last=i;}
            length+=state.Record(i).SegmentLength;
        }
        return(last,(length-state.Record(end).SegmentLength)*.5f);
    }

    private static void Mixed(State state,int start,ref int end,float spacing)
    {
        int last=end;float tail=0;bool seven=true;
        for(;last>=start;last--)
        {
            if(state.Tag(last)==8){seven=false;break;}
            if(state.Tag(last)==7)break;
            tail+=state.Record(last-1).SegmentLength;
        }
        state.SetTag(last,0);
        if(last==end)state.SetTag(last,seven ? 4:6);
        else if(state.InsertAtArc(last,end,tail*.5f,seven ? 4:6)>=0)end++;
        for(int i=start;i<=end;i++)if(state.Tag(i)==8)state.SetTag(i,0);
        bool first=true;float accumulated=0;
        for(int i=start;i<=end;i++)
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
        for(int i=start+1;i<=end;i++)
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
        for(int i=left-1;i>=state.Start;i--){before+=state.Record(i).SegmentLength;if(state.Tag(i)!=0 && !Loop(state.Tag(i)))break;}
        for(int i=right;i<state.End-1;i++){after+=state.Record(i).SegmentLength;if(state.Tag(i+1)!=0 && !Loop(state.Tag(i+1)))break;}
        if(left==right)
        {
            if(radius<before && radius<after)state.SetTag(left,5);
            return;
        }
        float half=state.Length(left,right)*.5f,walked=0;
        for(int i=left;i<right;i++)
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
                for(int j=i+1;j<right;j++)remainder+=state.Record(j).SegmentLength;
                if(radius<after+remainder){state.Insert(i+1,new(x,y),5);end++;}
            }
            break;
        }
    }

    private static bool Loop(int tag)=>unchecked((uint)(tag-11))<4;
    private static float Hypot(float x,float y)=>(float)Math.Sqrt((double)y*y+(double)x*x);

    private sealed class State
    {
        private LegacyCursivePreparedInk ink;
        private readonly int stroke;
        private readonly CancellationToken token;
        private LegacyCursivePreparedPoint[] records;
        private int[] tags;
        public int Start { get; }
        public int End=>Start+ink.StrokePointCounts.Span[stroke];
        public State(LegacyCursivePreparedInk ink,int stroke,CancellationToken token)
        {
            this.ink=ink;this.stroke=stroke;this.token=token;
            for(int i=0;i<stroke;i++)Start+=ink.StrokePointCounts.Span[i];
            records=ink.Points.ToArray();tags=records.Select(p=>p.Tag).ToArray();
        }
        public LegacyCursivePreparedPoint Record(int i)=>records[i];
        public int Tag(int i)=>tags[i];
        public void SetTag(int i,int tag)=>tags[i]=tag;
        public float Length(int start,int end)
        {
            float sum=0;for(int i=start;i<end;i++){token.ThrowIfCancellationRequested();sum+=records[i].SegmentLength;}return sum;
        }
        public void Insert(int global,InkPoint point,int tag)
        {
            token.ThrowIfCancellationRequested();ink=ink.WithTags(tags).Insert(stroke,global-Start,point,tag,token);
            records=ink.Points.ToArray();tags=records.Select(p=>p.Tag).ToArray();
        }
        public int InsertAtArc(int start,int end,float target,int tag)
        {
            float distance=0;
            for(int i=start;i<end;i++)
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
        public LegacyCursivePreparedInk Finish()=>ink.WithTags(tags);
    }
}
