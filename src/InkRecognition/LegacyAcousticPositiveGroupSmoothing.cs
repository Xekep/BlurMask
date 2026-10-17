using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglish119Cache.Probe")]
namespace InkRecognition;

internal readonly record struct LegacyAcousticGroupWrite(int Group,int Entry,ushort Symbol,int Before,int After);

/// <summary>Ordered1BB020 C8/D0/D8/E0 stage. EXP/log use portable Math
/// transcendental functions with the native float conversion boundaries.</summary>
internal static class LegacyAcousticPositiveGroupSmoothing
{
    internal static void Validate(IReadOnlyList<LegacySymbolMaskKnowledgeGroup> groups,int symbolCount,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(groups);token.ThrowIfCancellationRequested();if(groups.Count>512)throw new ArgumentException("Bounded acoustic groups required.");
        int entries=0;foreach(var group in groups)
        {
            token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(group);entries=checked(entries+group.Symbols.Length);
            if(entries>65535||!float.IsFinite(group.Weight)||group.Weight>1)throw new NotSupportedException("Finite bounded mixture coefficients at most1 required.");
            foreach(ushort label in group.Symbols.Span)if(label>=symbolCount)throw new ArgumentException("Group outside its acoustic alphabet.");
        }
    }
    internal static void ApplyInPlace(int[] costs,byte[] mask,IReadOnlyList<LegacySymbolMaskKnowledgeGroup> groups,
        CancellationToken token,List<LegacyAcousticGroupWrite>? writes=null)
    {
        ArgumentNullException.ThrowIfNull(costs);ArgumentNullException.ThrowIfNull(mask);
        if(mask.Length>costs.Length)throw new ArgumentException("Full acoustic mask/cache required.");
        Validate(groups,mask.Length,token);
        for(int g=0;g<groups.Count;g++)
        {
            token.ThrowIfCancellationRequested();var group=groups[g];if(group.Weight<=0)continue;
            float maximum=0;foreach(ushort label in group.Symbols.Span)
                if(mask[label]!=0)maximum=MathF.Max(maximum,Probability(costs[label]));
            if(maximum==0)continue;
            for(int entry=0;entry<group.Symbols.Length;entry++)
            {
                token.ThrowIfCancellationRequested();ushort label=group.Symbols.Span[entry];if(mask[label]==0)continue;
                int before=costs[label];float p=Probability(before),oneMinus=1f-group.Weight;
                float left=oneMinus*p,right=group.Weight*maximum,mix=left+right;
                if(!float.IsFinite(mix)||mix<=0)throw new NotSupportedException("Mixture falls outside finite positive portable logarithm domain.");
                double value=-Math.Log((double)mix)*8192d,rounded=value<=0?value-.5:value+.5;
                if(!double.IsFinite(rounded)||rounded<int.MinValue||rounded>=2147483648d)throw new NotSupportedException("Mixture cost exceeds bounded native int conversion.");
                costs[label]=(int)rounded;writes?.Add(new(g,entry,label,before,costs[label]));
            }
        }
    }
    private static float Probability(int cost)
    {
        if(cost>=0x4000000)return 0;
        //Actual primitive/dot costs include−1. Larger negative costs are a
        //separate unsupported portable domain rather than repaired values.
        if(cost< -1)throw new NotSupportedException("Negative group input outside the restored primitive/dot range.");
        float argument=-(float)cost*(1f/8192f);return(float)Math.Exp((double)argument);
    }
}
