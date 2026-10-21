using System.Runtime.CompilerServices;
namespace InkRecognition;

/// <summary>Fresh selected-main numerical2D2.V48 tables only. No configured
///packet1/2 policy, V58query, source masks, main inference or winner.</summary>
public sealed class LegacyEnglishLkFreshNumericBinding
{
    private readonly int[] wordCosts;
    private readonly int[][] classCosts,backoffCosts;
    private readonly int[] case2,case3;
    public LegacyEnglishLkSparseLanguageData LanguageOwner{get;}
    public LegacyEnglishLkPackedData DataOwner=>LanguageOwner.DataOwner;
    public int SourceKind=>DataOwner.Source.SourceKind;
    public int Profile=>0x137-SourceKind;
    public float SourceScale=>DataOwner.Prefix.RuntimeLanguageScale;
    public int LiteralPacketByte0=>0;
    public int Context00{get;}
    public int Boundary94C{get;}
    public int FinalUnigram94D{get;}
    public float CaseBias{get;}
    public float CaseMultiplier{get;}
    private LegacyEnglishLkFreshNumericBinding(LegacyEnglishLkSparseLanguageData language,int[] word,int[][] classes,int[][] backoff,int[] pairs,int[] triples,int context,int boundary,int final,float bias,float mult)
    {LanguageOwner=language;wordCosts=word;classCosts=classes;backoffCosts=backoff;case2=pairs;case3=triples;Context00=context;Boundary94C=boundary;FinalUnigram94D=final;CaseBias=bias;CaseMultiplier=mult;}
    public static LegacyEnglishLkFreshNumericBinding Create(LegacyEnglishLkSparseLanguageData language,CancellationToken token=default)=>CreateCore(language,null,token);
    internal static LegacyEnglishLkFreshNumericBinding CreatePrepared(LegacyEnglishLkSparseLanguageData language,Action<string> checkpoint,CancellationToken token=default)=>CreateCore(language,checkpoint,token);
    private static LegacyEnglishLkFreshNumericBinding CreateCore(LegacyEnglishLkSparseLanguageData language,Action<string>? checkpoint,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(language);token.ThrowIfCancellationRequested();LegacyEnglishSparseCopyBudget.Preflight();
        if(language.DataOwner.Source.SourceKind!=0x135||language.DataOwner.Prefix.RuntimeLanguageScale!=1||language.Order!=3||language.StoredDefault268C!=unchecked((int)0xfc000000))throw new InvalidDataException("Actual English sparse numeric owner required.");
        void Check(string stage){checkpoint?.Invoke(stage);token.ThrowIfCancellationRequested();}
        int profile=0x137-language.DataOwner.Source.SourceKind;float scale=language.DataOwner.Prefix.RuntimeLanguageScale;
        float[] bias=new float[3],mult=new float[3];for(int i=0;i<3;i++){bias[i]=Multiply(scale,language.Float("F0."+profile,i));mult[i]=Multiply(scale,language.Float("130."+profile,i));}
        float before=Multiply(scale,language.Float("110",profile)),after=Multiply(scale,language.Float("120",profile));
        Check("numeric:word");int[] word=new int[256];for(int i=0;i<256;i++){if((i&63)==0)token.ThrowIfCancellationRequested();word[i]=Cost(language.Int32("A8",i),mult[0],before);}
        int[][] classes=new int[3][],backoff=new int[2][];
        for(int order=0;order<3;order++)
        {
            Check("numeric:class");classes[order]=new int[256];for(int i=0;i<256;i++){if((i&63)==0)token.ThrowIfCancellationRequested();classes[order][i]=Cost(language.Int32("D8."+order,i),mult[order],bias[order]);}
            if(order<2){Check("numeric:backoff");backoff[order]=new int[256];for(int i=0;i<256;i++){if((i&63)==0)token.ThrowIfCancellationRequested();backoff[order][i]=Cost(language.Int32("E8."+order,i),mult[order],0);}}
        }
        float caseBias=language.Float("154",profile),caseMult=language.Float("164",profile);int[] pairs=new int[144],triples=new int[1728];
        for(int i=1;i<=12;i++)
        {
            Check("numeric:case");for(int j=1;j<=12;j++)
            {
                pairs[(i-1)*12+j-1]=CaseCost(language.Float("174",i*13+j),caseMult,caseBias);
                for(int k=1;k<=12;k++)triples[((i-1)*12+j-1)*12+k-1]=CaseCost(language.Float("418",i*169+j*13+k),caseMult,caseBias);
            }
        }
        Check("numeric:publish");return new(language,word,classes,backoff,pairs,triples,checked(-language.StoredDefault268C),Truncate(Multiply(Subtract(after,before),-8192)),classes[0][255],caseBias,caseMult);
    }
    //Separate non-inlined scalar operations preserve each native binary32 SSE
    //rounding boundary and forbid a multiply/add contraction into an FMA.
    [MethodImpl(MethodImplOptions.NoInlining)]private static float Multiply(float a,float b)=>a*b;
    [MethodImpl(MethodImplOptions.NoInlining)]private static float Subtract(float a,float b)=>a-b;
    [MethodImpl(MethodImplOptions.NoInlining)]private static float Add(float a,float b)=>a+b;
    private static int Truncate(float value){if(!float.IsFinite(value)||(double)value<int.MinValue||(double)value>=2147483648d)throw new InvalidDataException("Bounded numerical language integer required.");return(int)value;}
    private static int Cost(int raw,float mult,float bias)=>Truncate(Multiply(Subtract(Multiply(Multiply(raw,0.0001220703125f),mult),bias),8192));
    private static int CaseCost(float raw,float mult,float bias){float x=Multiply(Add(Multiply(mult,raw),bias),8192);return checked(-Truncate(x>0?Add(x,0.5f):Subtract(x,0.5f)));}
    public int[] CopyWordCosts(){LegacyEnglishSparseCopyBudget.Snapshot(1024);return wordCosts.ToArray();}
    public int[] CopyClassCosts(int order){if((uint)order>=3)throw new ArgumentOutOfRangeException(nameof(order));LegacyEnglishSparseCopyBudget.Snapshot(1024);return classCosts[order].ToArray();}
    public int[] CopyBackoffCosts(int order){if((uint)order>=2)throw new ArgumentOutOfRangeException(nameof(order));LegacyEnglishSparseCopyBudget.Snapshot(1024);return backoffCosts[order].ToArray();}
    public int GetCaseCost(int previous,int current)
    {
        if(previous==0&&current==0)return 0;if(previous<1||previous>12||current<1||current>12)throw new ArgumentOutOfRangeException(nameof(current),"Only native written case domain is available.");return case2[(previous-1)*12+current-1];
    }
    public int GetCaseCost(int oldest,int previous,int current)
    {
        if(oldest==0)return GetCaseCost(previous,current);if(oldest<1||oldest>12||previous<1||previous>12||current<1||current>12)throw new ArgumentOutOfRangeException(nameof(current),"Only native written case domain is available.");return case3[((oldest-1)*12+previous-1)*12+current-1];
    }
}
