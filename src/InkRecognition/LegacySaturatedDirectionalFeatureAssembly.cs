namespace InkRecognition;

/// <summary>Native2B1 BF090/BFF40 geometry transforms: first signed-short
///saturation, then274-column calibration and second saturation, then baseline9.
///Admitted shape is global17/local16x16/baseline9 with F0 active. Calibration
///is explicit data. Old wrapping2B0 APIs remain separate.</summary>
public static class LegacySaturatedDirectionalFeatureAssembly
{
    public static short[] Compose(ReadOnlySpan<int> global,IReadOnlyList<int[]> local,
        ReadOnlySpan<int> baseline,bool spatial,ReadOnlySpan<short> minima,
        ReadOnlySpan<short> maxima,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(local);token.ThrowIfCancellationRequested();
        if(global.Length!=17||local.Count!=16||baseline.Length!=9||minima.Length!=274||maxima.Length!=274)
            throw new ArgumentException("Actual17/16x16/9 and274-column calibration dimensions required.");
        var windows=new int[16][];
        for(int i=0;i<16;i++)
        {
            token.ThrowIfCancellationRequested();var window=local[i];
            if(window is null||window.Length!=16)throw new ArgumentException("Sixteen raw local channels required.");
            windows[i]=window;
        }
        var result=new short[283];int at=0;result[at++]=1024;
        unchecked
        {
            for(int i=0;i<17;i++){token.ThrowIfCancellationRequested();int v=global[i];result[at++]=Clamp(i switch{2=>(v*2048-2048)/5-1024,3=>Ratio(v,3072),8=>v*2,>=9 and<=15=>Ratio(v,3),_=>Ratio(v,1024)});}
            foreach(var window in windows)for(int i=0;i<16;i++){token.ThrowIfCancellationRequested();int v=window[i];result[at++]=Clamp(i switch{0 or 1 or 2 when spatial=>Ratio(v,256),4 or 5 or>=9 and<=14=>v*2048-1024,8=>v*2,15=>Ratio(v,spatial?256:128),_=>Ratio(v,1024)});}
            // F0 active resets native calibration start to1. Read the already
            // saturated short; do not carry the unsaturated transformed int.
            for(int i=1;i<274;i++)
            {
                token.ThrowIfCancellationRequested();int range=maxima[i]-minima[i];
                if(range!=0)result[i]=Clamp((result[i]-minima[i])*2048/range-1024);
            }
            for(int i=0;i<9;i++){token.ThrowIfCancellationRequested();int v=baseline[i]+(i==3?4096:0);result[at++]=Clamp(Ratio(v,i is 2 or 3?8192:i>=6?1024:4096));}
        }
        if(at!=283)throw new InvalidDataException("Whole actual2B1 vector required.");return result;
    }
    private static int Ratio(int v,int d)=>unchecked(v*2048)/d-1024;
    private static short Clamp(int v)=>(short)Math.Clamp(v,short.MinValue,short.MaxValue);
}
