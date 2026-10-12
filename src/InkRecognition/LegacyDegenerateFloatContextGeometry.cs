namespace InkRecognition;

/// <summary>Native1c8400 width-plus-one/height-plus-one==1 special stage.
/// Caller has validated bounded, nonempty float source points and references.</summary>
internal static class LegacyDegenerateFloatContextGeometry
{
    internal static float[] ExtractValidated(IReadOnlyList<InkStroke> strokes,float height,
        float horizontalReference,float verticalReference,float minimumY,bool maximumX,
        CancellationToken cancellationToken)
    {
        float four=height*4f,twice=height+height;
        var result=new float[30];result[0]=1;
        result[1]=Clip((minimumY-verticalReference)/four);
        //The special path sets the accumulator and last-point pointer to the
        //FIRST selected source point, bypassing both normal mean-Y modes.
        result[2]=Clip((strokes[0].Points[0].Y-verticalReference)/four);
        result[3]=result[4]=Clip(1f/twice-1f);result[5]=-1;
        result[6]=result[7]=-.5f; //Both native marked-run averages are1.
        float boundary=-float.MaxValue;
        foreach(var stroke in strokes)
        {
            float local=maximumX?-float.MaxValue:float.MaxValue;
            foreach(var point in stroke.Points)
            {cancellationToken.ThrowIfCancellationRequested();local=maximumX?Math.Max(local,point.X):Math.Min(local,point.X);}
            boundary=Math.Max(boundary,local);
        }
        result[12]=Clip((boundary-horizontalReference)/four);
        //Four direction fractions and both endpoint deltas remain zero.
        for(int i=0;i<3;i++)
        {
            result[15+i]=strokes.Count==i+1?1:-1;
            result[18+i*4]=i==0?Clip(-verticalReference/four):0;
            result[19+i*4]=result[20+i*4]=result[21+i*4]=-1;
        }
        foreach(float value in result)if(!float.IsFinite(value))
            throw new ArgumentException("Degenerate context references exceed the finite result domain.");
        return result;
    }
    private static float Clip(float value)=>value<-1?-1:value>1?1:value;
}
