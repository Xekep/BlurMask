namespace InkRecognition;

/// <summary>First1F42 assigned ordinary1AAE70 request: policy2C=0,gap30=0,
/// no metadata/custom geometry. Only source-bound adapters create its inputs.</summary>
internal static class LegacyNativeFirstInteriorDelimiterKernel
{
    private static readonly int[] Policies=[0,1,4,2,4,1,0, 2,2,2,2,2,0,0, 1,1,2,1,2,0,0,
        3,1,2,2,2,0,0, 1,1,2,1,2,0,0, 2,0,0,0,0,0,0, 0,0,0,0,0,0,0];
    internal static LegacyFirstInteriorDelimiterResult Read(ushort currentSymbol,ushort previousSymbol,
        int currentFirstId,int previousFirstId,uint previousStartContext,byte currentFlags,float height,
        ushort hyphen,ushort slash,ushort hash,ushort at,
        Func<LegacyNativeResidualBounds> readLeftBounds,Func<LegacyNativeResidualBounds> readRightBounds,
        CancellationToken token)
    {
        if((uint)currentFirstId>=7||(uint)previousFirstId>=7)
            throw new ArgumentException("Native seven-category VALUE domain required.");
        int policy=Policies[currentFirstId+previousFirstId*7];
        if(currentSymbol==hash||currentSymbol==at||currentSymbol==hyphen||previousSymbol==hyphen||
            currentSymbol==slash||previousSymbol==slash)policy=5;
        else if(policy==4)policy=2;
        bool emit;
        if(previousSymbol==hyphen&&previousStartContext==0)emit=true;
        else
        {
            if(policy==3)policy=previousStartContext!=0?2:1;
            var left=readLeftBounds();var right=readRightBounds();
            float gap=right.MinimumX<=left.MinimumX?left.MinimumX-right.MaximumX:right.MinimumX-left.MaximumX;
            emit=LegacyOwnedRussianMainInteriorDelimiter.EvaluatePolicy(policy,height,
                0,gap,float.MaxValue,float.MaxValue,previousSymbol==hyphen||previousSymbol==slash,currentSymbol==hyphen||currentSymbol==slash);
        }
        if(!emit&&(currentFlags&8)!=0)emit=true;
        token.ThrowIfCancellationRequested();return new(emit?(ushort)0xfffb:ushort.MaxValue,emit?1:0,0,0);
    }
}
