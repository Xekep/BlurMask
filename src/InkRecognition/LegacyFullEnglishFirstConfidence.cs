namespace InkRecognition;

/// <summary>Detached historical scalar confidence for an actual completed group.
/// Not candidate cost, word confidence, a controller winner or accepted text.</summary>
public sealed class LegacyFullEnglishFirstConfidence
{
    public LegacyFullEnglishFirstController Owner {get;}
    public LegacyFullEnglishFirstCachedGroup ActualCache {get;}
    private readonly float[] values;
    internal LegacyFullEnglishFirstConfidence(LegacyFullEnglishFirstController owner,LegacyFullEnglishFirstCachedGroup cache,float[] values)
    {Owner=owner;ActualCache=cache;this.values=values.ToArray();}
    public float[] CopyValues()=>values.ToArray();
    public float ReadSymbolScore(ushort symbol)
    {var candidates=ActualCache.CopyCandidates();for(int i=0;i<candidates.Length;i++)if(candidates[i].Symbol==symbol)return values[i];return 0;}
}
