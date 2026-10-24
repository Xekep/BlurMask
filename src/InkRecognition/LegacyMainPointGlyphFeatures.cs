using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("LegacyMainPointGlyphFeatures.Probe")]
namespace InkRecognition;

//Actual1111/3F,30/16/8/exponent10 MAIN baseline families only. This is
//00A9F0+00A770, not fabricated extent or a FIRST217/199 feature receipt.
internal static class LegacyMainPointGlyphFeatures
{
    internal static bool TryCreate(IReadOnlyList<LegacyInkStroke> strokes,LegacyInkBaseline baseline,
        ReadOnlySpan<short> context,short start,short end,CancellationToken token,
        out short[] trajectory,out short[] raster)
    {
        ArgumentNullException.ThrowIfNull(strokes);token.ThrowIfCancellationRequested();trajectory=[];raster=[];
        if(baseline.Y!=0||baseline.Height!=100)return false;
        if(context.Length!=28||start is not(-1024 or 1024)||end is not(-1024 or 1024))
            throw new ArgumentException("Actual context28 and explicit boundaries required.");
        if(strokes.Count is <1 or >15)return false;
        LegacyInkPoint? first=null;int count=0;
        foreach(var stroke in strokes)
        {
            token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(stroke);
            if(stroke.Points.Length>4096-count)throw new ArgumentException("Point glyph exceeds4096 points.");
            count+=stroke.Points.Length;
            foreach(var point in stroke.Points)
            {
                token.ThrowIfCancellationRequested();
                if(first is {} origin&&point!=origin)return false;
                first=point;
            }
        }
        if(first is null)return false;
        trajectory=new short[256];trajectory.AsSpan(6,30).Fill(1024);
        ReadOnlySpan<short> statistics=[1024,-768,0,0,-768,1024,-1024];
        statistics.CopyTo(trajectory.AsSpan(210,7));
        Span<short> pointBaseline=stackalloc short[9];WriteBaseline(first.Value.Y,pointBaseline);
        pointBaseline.CopyTo(trajectory.AsSpan(217,9));
        raster=new short[238];statistics.CopyTo(raster);
        pointBaseline.CopyTo(raster.AsSpan(7,9));
        raster.AsSpan(16,64).Fill(-1024);raster.AsSpan(80,128).Fill(1024);
        context.CopyTo(trajectory.AsSpan(226,28));context.CopyTo(raster.AsSpan(208,28));
        trajectory[^2]=raster[^2]=start;trajectory[^1]=raster[^1]=end;
        token.ThrowIfCancellationRequested();return true;
    }
    //Literal00A7701111 callback order: originalshortY, baseline0/height100,
    //truncate division, storeSHORT beforeclamp, originalpositive test first.
    //The secondcomparison remains literal even though unreachable here.
    private static void WriteBaseline(short y,Span<short> result)
    {
        const int unit=1024,height=100;
        result[0]=result[1]=result[3]=-unit;
        result[2]=(short)Math.Clamp((int)unchecked((short)(256*y/height)),-unit,unit);
        if(y>0){result[4]=result[5]=result[7]=-unit;result[6]=unit;result[8]=unchecked((short)(512*(-y-height)/height-unit));}
        else if(y>=height){result[4]=result[6]=result[7]=result[8]=-unit;result[5]=unit;}
        else{result[4]=unit;result[5]=result[6]=result[8]=-unit;result[7]=unchecked((short)(512*y/height-unit));}
    }
}
