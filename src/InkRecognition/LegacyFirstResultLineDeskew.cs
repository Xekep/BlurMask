using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Owned720-byte row and actual retained-point ranges. A zero-range row
/// is the native16F170 opposite-corner branch, not a fabricated glyph membership.</summary>
public sealed class LegacyFirstResultLineRow
{
    private readonly byte[] storage;
    private readonly LegacyFirstResultSourceRange[] ranges;
    public IReadOnlyList<LegacyFirstResultSourceRange> Ranges { get; }
    public ReadOnlyMemory<byte> CopyStorage() => storage.ToArray();
    public LegacyFirstResultLineRow(ReadOnlySpan<byte> row720, IReadOnlyList<LegacyFirstResultSourceRange> sourceRanges)
    {
        ArgumentNullException.ThrowIfNull(sourceRanges);
        if(row720.Length!=720||sourceRanges.Count>4096||BinaryPrimitives.ReadInt64LittleEndian(row720[8..])!=sourceRanges.Count)
            throw new ArgumentException("Complete720-byte row with its actual source-count/range ownership required.");
        storage=row720.ToArray();ranges=sourceRanges.ToArray();Ranges=Array.AsReadOnly(ranges);
    }
    public static LegacyFirstResultLineRow FromFirstResult(LegacyFirstResultSourceRecord row)
    {ArgumentNullException.ThrowIfNull(row);return new(row.CopyStorage().Span,row.Ranges);}
}

/// <summary>Owned16F170 result; original points, rows and40-byte guide are never modified.</summary>
public sealed class LegacyFirstResultLineDeskewResult
{
    private readonly byte[] guide;
    private readonly InkPoint[][] points;
    public IReadOnlyList<LegacyFirstResultLineRow> Rows { get; }
    public float Cosine { get; }
    public float Sine { get; }
    public float TranslationX { get; }
    public float TranslationY { get; }
    internal LegacyFirstResultLineDeskewResult(byte[] guide,InkPoint[][] points,LegacyFirstResultLineRow[] rows,
        float cosine,float sine,float tx,float ty)
    {this.guide=guide;this.points=points;Rows=Array.AsReadOnly(rows);Cosine=cosine;Sine=sine;TranslationX=tx;TranslationY=ty;}
    public ReadOnlyMemory<byte> CopyGuide()=>guide.ToArray();
    public InkPoint[][] CopyPhysicalPoints()=>points.Select(p=>p.ToArray()).ToArray();
}

/// <summary>Native Text16F170: rotate a descriptor's physical points, preserve
/// copied guide geometry when requested, then remeasure actual720 source ranges.
/// This is a bounded prepared postprocessor, not the whole24B/main247 pipeline.</summary>
public static class LegacyFirstResultLineDeskew
{
    public const int MaximumRangePointVisits=1_048_576;
    public const int MaximumRetainedRanges=65_536;
    public static LegacyFirstResultLineDeskewResult Apply(IReadOnlyList<InkStroke> physicalStrokes,
        int firstPhysical,int lastPhysical,IReadOnlyList<LegacyFirstResultLineRow> sourceRows,
        int firstRow,int endRow,ReadOnlySpan<byte> guide40,float minimumHeight,bool preserveCopiedGuide,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(physicalStrokes);ArgumentNullException.ThrowIfNull(sourceRows);
        cancellationToken.ThrowIfCancellationRequested();
        if(physicalStrokes.Count is <1 or >256||firstPhysical<0||lastPhysical<firstPhysical||lastPhysical>=physicalStrokes.Count||
            sourceRows.Count>4096||firstRow<0||endRow<firstRow||endRow>sourceRows.Count||endRow-firstRow>256||guide40.Length!=40||
            !float.IsFinite(minimumHeight)||minimumHeight<0)
            throw new ArgumentException("Bounded actual descriptor intervals, complete guide and physical minimum required.");
        byte[] guide=guide40.ToArray();float baseline=F(guide,0),height=F(guide,4),slope=F(guide,8);
        if(!float.IsFinite(baseline)||!float.IsFinite(height)||!float.IsFinite(slope))throw new ArgumentException("Finite guide0/4/8 required.");
        var points=new InkPoint[physicalStrokes.Count][];int remaining=MaximumRangePointVisits;
        for(int s=0;s<points.Length;s++)
        {
            if(physicalStrokes[s] is null||physicalStrokes[s].Points.Count>4096)throw new ArgumentException("Bounded physical buffers required.");
            points[s]=physicalStrokes[s].Points.ToArray();
            foreach(var p in points[s]){cancellationToken.ThrowIfCancellationRequested();if(!float.IsFinite(p.X)||!float.IsFinite(p.Y))throw new ArgumentException("Finite physical points required.");}
        }
        var rows=new LegacyFirstResultLineRow[sourceRows.Count];
        int remainingRanges=MaximumRetainedRanges;
        for(int i=0;i<rows.Length;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var row=sourceRows[i]??throw new ArgumentException("Null source row.");
            if(row.Ranges.Count>remainingRanges)throw new ArgumentException("Total retained range budget exceeded.");
            remainingRanges-=row.Ranges.Count;
            rows[i]=new(row.CopyStorage().Span,row.Ranges);
            if(i<firstRow||i>=endRow)continue;
            foreach(var range in row.Ranges)
            {
                var a=range.First;var z=range.Last;
                if(a.First!=z.First||a.Second<0||z.Second<a.Second||z.Second>=points.Length||a.Third<0||z.Third<0||
                    a.Third>=points[a.Second].Length||z.Third>=points[z.Second].Length||a.Second==z.Second&&z.Third<a.Third)
                    throw new ArgumentException("Actual ordered retained parent-point ranges required.");
                for(int s=a.Second;s<=z.Second;s++)
                {int count=(s==z.Second?z.Third+1:points[s].Length)-(s==a.Second?a.Third:0);if(count>remaining)throw new ArgumentException("Range point-visit work bound exceeded.");remaining-=count;}
            }
        }
        float minX=0,maxX=0;bool any=false;
        for(int s=firstPhysical;s<=lastPhysical;s++)foreach(var p in points[s])
        {cancellationToken.ThrowIfCancellationRequested();if(!any){minX=maxX=p.X;any=true;}else{if(p.X<minX)minX=p.X;if(maxX<p.X)maxX=p.X;}}
        float square=slope*slope;float cosine=1f/(float)Math.Sqrt((double)square+1.0);
        float sine=(float)Math.Sqrt((double)(1f-cosine*cosine));if(slope>0)sine=-sine;
        float centerX=(maxX+minX)*.5f,centerY=centerX*slope+baseline;
        float tx=(centerY*sine-centerX*cosine)+centerX,ty=(-sine*centerX-centerY*cosine)+centerY;
        if(!float.IsFinite(square)||!float.IsFinite(cosine)||!float.IsFinite(sine)||!float.IsFinite(tx)||!float.IsFinite(ty))
            throw new InvalidDataException("Line deskew overflow.");
        if(!preserveCopiedGuide)
        {
            float b=baseline*cosine+sine*0f+ty;
            float h=((baseline+height)*cosine+sine*0f+ty)-b;
            if(minimumHeight>h)h=minimumHeight;
            if(!float.IsFinite(b)||!float.IsFinite(h))throw new InvalidDataException("Line guide overflow.");
            WF(guide,0,b);WF(guide,4,h);
        }
        bool transform=cosine!=1f||-sine!=0f||tx!=0f||sine!=0f||ty!=0f;
        if(transform)for(int s=firstPhysical;s<=lastPhysical;s++)for(int j=0;j<points[s].Length;j++)
        {cancellationToken.ThrowIfCancellationRequested();var p=points[s][j];points[s][j]=Transform(p,cosine,sine,tx,ty);}
        for(int i=firstRow;i<endRow;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();var row=rows[i];byte[] storage=row.CopyStorage().ToArray();
            if(row.Ranges.Count==0)
            {
                var a=Transform(new(F(storage,0x18),F(storage,0x20)),cosine,sine,tx,ty);
                var z=Transform(new(F(storage,0x1c),F(storage,0x24)),cosine,sine,tx,ty);
                WF(storage,0x18,a.X);WF(storage,0x20,a.Y);WF(storage,0x1c,z.X);WF(storage,0x24,z.Y);
            }
            else
            {
                float left=float.MaxValue,right=-float.MaxValue,top=float.MaxValue,bottom=-float.MaxValue;
                foreach(var range in row.Ranges)for(int s=range.First.Second;s<=range.Last.Second;s++)
                {
                    int first=s==range.First.Second?range.First.Third:0,last=s==range.Last.Second?range.Last.Third:points[s].Length-1;
                    for(int j=first;j<=last;j++)
                    {cancellationToken.ThrowIfCancellationRequested();var p=points[s][j];if(p.X<left)left=p.X;if(right<p.X)right=p.X;if(p.Y<top)top=p.Y;if(bottom<p.Y)bottom=p.Y;}
                }
                WF(storage,0x18,left);WF(storage,0x1c,right);WF(storage,0x20,top);WF(storage,0x24,bottom);
            }
            rows[i]=new(storage,row.Ranges);
        }
        return new(guide,points,rows,cosine,sine,tx,ty);
    }
    private static InkPoint Transform(InkPoint p,float cosine,float sine,float tx,float ty)
    {
        float x=-sine*p.Y+p.X*cosine+tx,y=p.Y*cosine+p.X*sine+ty;
        if(!float.IsFinite(x)||!float.IsFinite(y))throw new InvalidDataException("Deskew point overflow.");return new(x,y);
    }
    private static float F(ReadOnlySpan<byte>b,int o)=>BinaryPrimitives.ReadSingleLittleEndian(b[o..]);
    private static void WF(Span<byte>b,int o,float v)=>BinaryPrimitives.WriteSingleLittleEndian(b[o..],v);
}
