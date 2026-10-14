using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Exact24D/v100 reader1781E0. Angle parameters become float tangents;
/// the search step is the tangent interval divided by the radian step count.</summary>
public sealed record LegacyOrdinaryShearProfile(byte MinimumGaps,byte MaximumGaps,float RelatedFloor,
    float RelatedNormalizer,float MinimumShear,float MaximumShear,float SearchStep)
{
    public static LegacyOrdinaryShearProfile ReadV100Node(ReadOnlySpan<byte> resource,int offset)
    {
        if(offset<0||offset>resource.Length-38||I(resource,offset)!=0x101fe90||I(resource,offset+4)!=30||
            I(resource,offset+8)!=0x24d||BinaryPrimitives.ReadUInt16LittleEndian(resource[(offset+12)..])!=0||
            BinaryPrimitives.ReadUInt16LittleEndian(resource[(offset+14)..])!=0x100)
            throw new InvalidDataException("Complete unencoded24D/v100 node required.");
        float low=F(resource,offset+26),high=F(resource,offset+30),angleStep=F(resource,offset+34);
        if(!float.IsFinite(low)||!float.IsFinite(high)||low<=-1.5f||high>=1.5f||low>=high||!float.IsFinite(angleStep)||angleStep<=0)
            throw new InvalidDataException("Bounded finite native shear angles required.");
        float a=(float)Math.Tan(low),b=(float)Math.Tan(high),step=(b-a)/((high-low)/angleStep);
        var result=new LegacyOrdinaryShearProfile(resource[offset+16],resource[offset+17],F(resource,offset+18),F(resource,offset+22),a,b,step);
        result.Validate();return result;
    }
    internal void Validate()
    {
        if(MinimumGaps>=MaximumGaps||!float.IsFinite(RelatedFloor)||RelatedFloor<0||RelatedFloor>1||
            !float.IsFinite(RelatedNormalizer)||RelatedNormalizer<=0||!float.IsFinite(MinimumShear)||
            !float.IsFinite(MaximumShear)||MinimumShear>=MaximumShear||!float.IsFinite(SearchStep)||SearchStep<=0||
            (MaximumShear-MinimumShear)/SearchStep>4096)
            throw new ArgumentException("Bounded finite native24D profile required.");
    }
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private static float F(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadSingleLittleEndian(b[at..]);
}

public sealed class LegacyOrdinaryShearResult
{
    private readonly InkPoint[][] points;
    public IReadOnlyList<LegacyFirstResultLineRow> Rows {get;}
    public float Blend {get;}
    public float MinimumSearchShear {get;}
    public float MaximumSearchShear {get;}
    public float GapSearchShear {get;}
    public float DirectionShear {get;}
    public float Shear {get;}
    public int GridIterations {get;}
    internal LegacyOrdinaryShearResult(InkPoint[][] points,LegacyFirstResultLineRow[] rows,float blend,float min,float max,
        float gap,float direction,float shear,int iterations)
    {this.points=points;Rows=Array.AsReadOnly(rows);Blend=blend;MinimumSearchShear=min;MaximumSearchShear=max;
        GapSearchShear=gap;DirectionShear=direction;Shear=shear;GridIterations=iterations;}
    public InkPoint[][] CopyPhysicalPoints()=>points.Select(s=>s.ToArray()).ToArray();
}

/// <summary>Text177280 ordinary/no-previous-source24D producer: actual source720
/// memberships -> float gap search + length-weighted physical directions -> shear
/// and remeasured720 boxes. It does not substitute word boxes for physical ranges,
/// infer glyph labels, implement29B gaps, or select a recognition winner.</summary>
public static class LegacyNativeOrdinaryShearPreparation
{
    public const int MaximumPointVisits=16_777_216;
    public static LegacyOrdinaryShearResult Apply(LegacyOrdinaryShearProfile profile,IReadOnlyList<ReadOnlyMemory<byte>> descriptors,
        IReadOnlyList<ReadOnlyMemory<byte>> guides,IReadOnlyList<InkStroke> physicalStrokes,IReadOnlyList<int> physicalAssociations,
        IReadOnlyList<LegacyFirstResultLineRow> sourceRows,LegacyNativeCaseAlphabetRanges casedRanges,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(profile);ArgumentNullException.ThrowIfNull(descriptors);ArgumentNullException.ThrowIfNull(guides);
        ArgumentNullException.ThrowIfNull(physicalStrokes);ArgumentNullException.ThrowIfNull(physicalAssociations);
        ArgumentNullException.ThrowIfNull(sourceRows);ArgumentNullException.ThrowIfNull(casedRanges);profile.Validate();
        cancellationToken.ThrowIfCancellationRequested();
        if(descriptors.Count is <1 or >512||guides.Count>512||sourceRows.Count>4096||physicalStrokes.Count is <1 or >256||
            physicalAssociations.Count!=physicalStrokes.Count)
            throw new ArgumentException("Bounded complete source intervals required.");
        var points=physicalStrokes.Select(s=>s?.Points.ToArray()??throw new ArgumentException("Physical stroke missing.")).ToArray();
        foreach(var stroke in points)
        {if(stroke.Length is <1 or >4096)throw new ArgumentException("Bounded nonempty physical strokes required.");
            foreach(var p in stroke)if(!float.IsFinite(p.X)||!float.IsFinite(p.Y))throw new ArgumentException("Finite source points required.");}
        var active=new List<(int First,int End,int StartPhysical,int EndPhysical,float Baseline,float Height)>();
        var seenPhysical=new bool[points.Length];var seenRows=new bool[sourceRows.Count];uint gaps=0;
        foreach(var memory in descriptors)
        {
            var d=memory.Span;if(d.Length!=88)throw new ArgumentException("Complete88-byte descriptor required.");
            int kind=BinaryPrimitives.ReadUInt16LittleEndian(d)&15;if(kind is not (0 or 6)){if(kind!=3)throw new NotSupportedException("Only ordinary/separator descriptors restored.");continue;}
            int line=I(d,16),first=I(d,52),end=I(d,56),startPhysical=I(d,36),endPhysical=I(d,40);
            if(line<0||line>=guides.Count||guides[line].Length!=40||first<0||end<=first||end>sourceRows.Count||
                startPhysical<0||endPhysical<startPhysical||endPhysical>=points.Length)
                throw new ArgumentException("Assigned nonempty ordinary descriptor with actual720/physical intervals required.");
            float baseline=F(guides[line].Span,0),height=F(guides[line].Span,4);
            if(!float.IsFinite(baseline)||!float.IsFinite(height)||height<=0)throw new ArgumentException("Positive finite current guide required.");
            gaps=unchecked(gaps+(uint)(end-first)-1);
            for(int p=startPhysical;p<=endPhysical;p++){if(seenPhysical[p])throw new ArgumentException("Physical interval overlap.");seenPhysical[p]=true;}
            for(int r=first;r<end;r++)
            {
                if(seenRows[r])throw new ArgumentException("Source720 interval overlap.");seenRows[r]=true;
                var b=sourceRows[r].CopyStorage().Span;short label=BinaryPrimitives.ReadInt16LittleEndian(b[40..]);float weight=F(b,80);
                if(label< -1||label>=casedRanges.Alphabet.LabelCount||!float.IsFinite(weight)||weight<0||weight>1)
                    throw new ArgumentException("Actual alphabet label and finite source weight required.");
                foreach(var range in sourceRows[r].Ranges)
                {
                    var a=range.First;var z=range.Last;
                    if(a.First!=z.First||a.Second<startPhysical||z.Second>endPhysical||z.Second<a.Second||
                        a.Third<0||a.Third>=points[a.Second].Length||z.Third<0||z.Third>=points[z.Second].Length||
                        (a.Second==z.Second&&z.Third<a.Third))throw new ArgumentException("Actual ordered retained-source range required.");
                }
            }
            active.Add((first,end,startPhysical,endPhysical,baseline,height));
        }
        float blend=Math.Clamp(((float)gaps-profile.MinimumGaps)/(profile.MaximumGaps-profile.MinimumGaps),0,1);
        float total=0,relatedFirst=0;
        foreach(var d in active)for(int r=d.First;r<d.End;r++)
        {
            var b=sourceRows[r].CopyStorage().Span;short label=BinaryPrimitives.ReadInt16LittleEndian(b[40..]);if(label==-1)continue;
            float weight=F(b,80);bool cased=In((ushort)label,casedRanges.FirstStart,casedRanges.FirstEnd)||In((ushort)label,casedRanges.SecondStart,casedRanges.SecondEnd);
            if(cased&&casedRanges.Alphabet.TryGetRelatedForms((ushort)label,0,out var forms)&&forms.First==(ushort)label)relatedFirst+=weight;
            total+=weight;
        }
        if(total!=0)blend*=Math.Max(profile.RelatedFloor,Math.Min(1,relatedFirst/(total*profile.RelatedNormalizer)));
        float low=blend*profile.MinimumShear,high=blend*profile.MaximumShear,gapShear=0,bestGap=-float.MaxValue;
        int visits=0,iterations=0;
        for(float candidate=low;candidate<high;candidate+=profile.SearchStep)
        {
            if(++iterations>4096)throw new ArgumentException("Shear grid bound exceeded.");cancellationToken.ThrowIfCancellationRequested();
            float sum=0;int count=0;
            foreach(var d in active)
            {
                float frontier=-float.MaxValue,anchor=candidate*d.Baseline;
                for(int r=d.First;r<d.End;r++)
                {
                    if(sourceRows[r].Ranges.Count==0)continue;
                    float min=float.MaxValue,max=-float.MaxValue;
                    Visit(sourceRows[r],p=>{float x=(-candidate*p.Y+p.X)+anchor;if(x<=min)min=x;if(max<=x)max=x;});
                    if(frontier!=-float.MaxValue){sum+=(min-frontier)/d.Height;count++;}
                    if(frontier<=max)frontier=max;
                }
            }
            if(count!=0&&bestGap<sum){bestGap=sum;gapShear=candidate;}
        }
        double length=0,weighted=0;
        foreach(var d in active)for(int s=d.StartPhysical;s<=d.EndPhysical;s++)
        {
            if(physicalAssociations[s]!=-1)continue;
            for(int p=0;p<points[s].Length-1;p++)
            {
                cancellationToken.ThrowIfCancellationRequested();float dx=points[s][p+1].X-points[s][p].X,dy=points[s][p+1].Y-points[s][p].Y;
                if(dy<0){dx=-dx;dy=-dy;}if(dy<=1e-10f)continue;
                float ratio=dx/dy;if(ratio<low||ratio>high)continue;double segment=Math.Sqrt((double)(dy*dy+dx*dx));
                length+=segment;weighted+=(double)ratio*segment;
            }
        }
        float direction=length>=1e-10?(float)(weighted/length):0,shear=(direction+gapShear)*.5f;
        if(!float.IsFinite(shear))throw new InvalidDataException("Native shear overflow.");
        var rows=sourceRows.ToArray();
        foreach(var d in active)
        {
            float anchor=shear*d.Baseline;
            for(int s=d.StartPhysical;s<=d.EndPhysical;s++)for(int p=0;p<points[s].Length;p++)
            {cancellationToken.ThrowIfCancellationRequested();var point=points[s][p];float x=(-shear*point.Y+point.X)+anchor;
                if(!float.IsFinite(x))throw new InvalidDataException("Sheared source point overflow.");points[s][p]=point with{X=x};}
            for(int r=d.First;r<d.End;r++)
            {
                byte[] b=sourceRows[r].CopyStorage().ToArray();
                if(sourceRows[r].Ranges.Count==0)
                {
                    float cy=(F(b,36)+F(b,32))*.5f,hy=(F(b,36)-F(b,32))*.5f,hx=(F(b,28)-F(b,24))*.5f;
                    float cx=((F(b,28)+F(b,24))*.5f+cy*-shear)+anchor;
                    WF(b,24,cx-hx);WF(b,28,hx+cx);WF(b,32,cy-hy);WF(b,36,hy+cy);
                }
                else
                {
                    float min=float.MaxValue,max=-float.MaxValue,top=float.MaxValue,bottom=-float.MaxValue;
                    Visit(sourceRows[r],p=>{if(p.X<=min)min=p.X;if(max<=p.X)max=p.X;if(p.Y<=top)top=p.Y;if(bottom<=p.Y)bottom=p.Y;});
                    WF(b,24,min);WF(b,28,max);WF(b,32,top);WF(b,36,bottom);
                }
                for(int k=24;k<=36;k+=4)if(!float.IsFinite(F(b,k)))throw new InvalidDataException("Sheared720 bounds overflow.");
                rows[r]=new(b,sourceRows[r].Ranges);
            }
        }
        return new(points,rows,blend,low,high,gapShear,direction,shear,iterations);
        void Visit(LegacyFirstResultLineRow row,Action<InkPoint> action)
        {
            foreach(var range in row.Ranges)for(int s=range.First.Second;s<=range.Last.Second;s++)
            {
                int first=s==range.First.Second?range.First.Third:0,last=s==range.Last.Second?range.Last.Third:points[s].Length-1;
                for(int p=first;p<=last;p++){if(++visits>MaximumPointVisits)throw new ArgumentException("Source range visit bound exceeded.");cancellationToken.ThrowIfCancellationRequested();action(points[s][p]);}
            }
        }
    }
    private static bool In(ushort label,ushort first,ushort end)=>label>=first&&label<=end;
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private static float F(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadSingleLittleEndian(b[at..]);
    private static void WF(Span<byte>b,int at,float value)=>BinaryPrimitives.WriteSingleLittleEndian(b[at..],value);
}
