namespace InkRecognition;

/// <summary>Bounded sampler297 derived-table mapping used by the native text beam.</summary>
public sealed class LegacyCursiveCategoryMapper
{
    private readonly int[] firstIds;
    private readonly int[][] maps;
    public int CompositeCategoryCount=>firstIds.Length;
    public int SymbolEntryCount=>maps[0].Length;
    public ReadOnlyMemory<int> CompositeFirstIds=>firstIds;

    public LegacyCursiveCategoryMapper(LegacyCursiveCategoryProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        firstIds=profile.CompositeCategories.ToArray().Select(p=>p.FirstId).ToArray();
        maps=[profile.StartCompositeMap.ToArray(),profile.EndCompositeMap.ToArray(),
            profile.ZeroStartCompositeMap.ToArray(),profile.ZeroEndCompositeMap.ToArray()];
    }

    /// <summary>Derived data+70 lookup. Composite IDs are distinct from raw neural-network slots.</summary>
    public int GetFirstId(int compositeCategory)
    {
        if((uint)compositeCategory>=(uint)firstIds.Length)throw new ArgumentOutOfRangeException(nameof(compositeCategory));
        return firstIds[compositeCategory];
    }

    /// <summary>Beam record category is its low six bits; invalid derived-table indices remain errors.</summary>
    public int GetBeamFirstId(byte recordCategory)=>GetFirstId(recordCategory&0x3f);

    /// <summary>Native data+50/+58/+60/+68 maps; a missing serialized mapping remains minus one.</summary>
    public int GetSymbolCompositeCategory(int symbolEntry,bool endBoundary=false,bool zeroFirstId=false)
    {
        if((uint)symbolEntry>=(uint)SymbolEntryCount)throw new ArgumentOutOfRangeException(nameof(symbolEntry));
        return maps[(zeroFirstId ? 2:0)+(endBoundary ? 1:0)][symbolEntry];
    }

    /// <summary>Combine an explicit symbol-boundary map with data+70, preserving missing mappings.</summary>
    public int GetSymbolFirstId(int symbolEntry,bool endBoundary=false,bool zeroFirstId=false)
    {
        int composite=GetSymbolCompositeCategory(symbolEntry,endBoundary,zeroFirstId);
        return composite<0 ? -1:GetFirstId(composite);
    }
}
