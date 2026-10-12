using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyRankedAcousticEntry(ushort Symbol,int Cost,float NormalizedLikelihood);

/// <summary>Ordinary ranked group record: native1be530 top20 likelihood and
/// virtual88/1c13a0 class/category query. External confidence pruning, group
/// construction, and wholetext winner search remain separate producers.</summary>
public sealed class LegacyOrdinaryRankedAcousticGroup
{
    public const int ImpossibleCost=0x4000000;
    private readonly LegacyRankedAcousticEntry[] entries;
    private readonly int[] geometryCosts;
    private readonly ushort[] bypass;
    private readonly int fallbackCost;
    private readonly int symbolCount;
    private readonly float scale;
    public ReadOnlyMemory<LegacyRankedAcousticEntry> Entries=>entries;

    /// <summary>The supplied ordered shortlist must already include native class
    /// corrections/pruning. Scale is record1c, not a guessed confidence slope.</summary>
    public LegacyOrdinaryRankedAcousticGroup(LegacyOrdinaryAcousticClassCacheResult cache,
        LegacyInkAlphabet alphabet,ReadOnlySpan<byte> serializedHeader,ReadOnlySpan<int> compositeCategoryCosts,
        float nativeCostScale,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(cache);ArgumentNullException.ThrowIfNull(alphabet);cancellationToken.ThrowIfCancellationRequested();
        if(serializedHeader.Length!=16||cache.Costs.Length!=alphabet.LabelCount+3||cache.Candidates.Length>150||
            compositeCategoryCosts.Length is <1 or >127||!float.IsFinite(nativeCostScale)||nativeCostScale<=0||
            BinaryPrimitives.ReadUInt16LittleEndian(serializedHeader[14..])!=alphabet.LabelCount||cache.FallbackCost>ImpossibleCost)
            throw new ArgumentException("Owned alphabet/header, bounded ranked group, category costs and positive native scale required.");
        symbolCount=alphabet.LabelCount;scale=nativeCostScale;fallbackCost=cache.FallbackCost;geometryCosts=compositeCategoryCosts.ToArray();
        foreach(int v in geometryCosts)if(v is <0 or >ImpossibleCost)throw new ArgumentException("Invalid category geometry cost.");
        var excluded=new List<ushort>(8);
        foreach(string s in new[]{"\U000f0001","\U000f0002","\U000f0003","\U000f0004","\U000f0008"})
            if(alphabet.TryGetLabel(s,out ushort id))excluded.Add(id);
        for(int slot=4;slot<=6;slot++)
        {
            ushort id=BinaryPrimitives.ReadUInt16LittleEndian(serializedHeader[(slot*2)..]);
            if(id!=ushort.MaxValue){if(id>=symbolCount)throw new ArgumentException("Header label outside alphabet.");excluded.Add(id);}
        }
        bypass=excluded.ToArray();entries=new LegacyRankedAcousticEntry[cache.Candidates.Length];var seen=new HashSet<ushort>();
        int last=int.MinValue;
        for(int i=0;i<entries.Length;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();var item=cache.Candidates.Span[i];
            if(item.Symbol>=symbolCount||item.Cost>=ImpossibleCost||item.Cost<last||!seen.Add(item.Symbol))
                throw new ArgumentException("Expected distinct finite native signed-cost ordered shortlist.");
            last=item.Cost;entries[i]=new(item.Symbol,item.Cost,0);
        }
        if(entries.Length==1){entries[0]=entries[0] with {NormalizedLikelihood=1};return;}
        int n=Math.Min(20,entries.Length);float sum=0;float denominator=scale*8192f;
        for(int i=0;i<n;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            float exponent=-(float)entries[i].Cost/denominator;
            float value=(float)Math.Exp(exponent);
            if(!float.IsFinite(value))throw new NotSupportedException("Native likelihood leaves the checked finite float domain.");
            entries[i]=entries[i] with {NormalizedLikelihood=value};sum+=value;
        }
        if(n!=0&&(!float.IsFinite(sum)||sum<=0))throw new NotSupportedException("Native unstabilized likelihood normalization has zero/overflow sum.");
        for(int i=0;i<n;i++)entries[i]=entries[i] with {NormalizedLikelihood=entries[i].NormalizedLikelihood/sum};
    }

    /// <summary>Native virtual78/1c1260 returns entry0's top20-normalized
    /// likelihood, or zero for a missing label. This is not acceptance confidence.</summary>
    public float GetNormalizedLikelihood(ushort symbol,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach(var item in entries){cancellationToken.ThrowIfCancellationRequested();if(item.Symbol==symbol)return item.NormalizedLikelihood;}
        return 0;
    }

    /// <summary>1ef070 stores trunc(likelihood*255), clipped to1..255.</summary>
    public byte GetFirstControllerLikelihoodByte(ushort symbol,CancellationToken cancellationToken=default)
        =>(byte)Math.Clamp((int)(GetNormalizedLikelihood(symbol,cancellationToken)*255f),1,255);

    /// <summary>1c13a0 request: symbol+8, shortlist ordinal+6 (FFFF=lookup),
    /// signed category+A, restrict-top20+B, primitive cost+C. The native ordinal
    /// branch deliberately does not verify symbol equals the selected entry.</summary>
    public int Query(ushort symbol,ushort shortlistOrdinal,int compositeCategory,bool restrictTopTwenty,
        bool usePrimitiveCost,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(symbol>symbolCount+2)throw new ArgumentOutOfRangeException(nameof(symbol));
        int cost;
        if(usePrimitiveCost)
        {
            float v=scale*40000f;if(!float.IsFinite(v)||v>=2147483648f)throw new NotSupportedException("Native primitive cost exceeds checked integer range.");
            cost=(int)v;
        }
        else if(shortlistOrdinal==ushort.MaxValue&&symbol==symbolCount)cost=fallbackCost;
        else
        {
            int ordinal=shortlistOrdinal;
            if(ordinal==ushort.MaxValue)
            {
                ordinal=0;while(ordinal<entries.Length&&entries[ordinal].Symbol!=symbol){cancellationToken.ThrowIfCancellationRequested();ordinal++;}
            }
            if(ordinal>=entries.Length||restrictTopTwenty&&ordinal>19)return ImpossibleCost;
            cost=entries[ordinal].Cost;
        }
        if(cost>=ImpossibleCost)return cost;
        if(!bypass.AsSpan().Contains(symbol))
        {
            if((uint)compositeCategory>=(uint)geometryCosts.Length)throw new ArgumentOutOfRangeException(nameof(compositeCategory));
            cost=checked(cost+geometryCosts[compositeCategory]);
        }
        return cost;
    }
}
