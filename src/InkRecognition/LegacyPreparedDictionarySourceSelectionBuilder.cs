namespace InkRecognition;

/// <summary>Prepared child descriptor and stored first n-gram count; identities are portable caller-owned tokens.</summary>
public sealed record LegacyPreparedDictionarySourceSelectionInput(
    LegacyPreparedDictionarySourceDescriptor Descriptor, uint? FirstStoredNgramCount, ulong ContextOwnerIdentity)
{
    /// <summary>
    ///2d1.virtual40=0fe000 reads array78[0]. For the checked order2/flags3 loader
    /// that array starts with ClassCount, not LanguageOrder or WordCount.
    /// This helper requires a matching prepared order-two descriptor/model.
    /// </summary>
    public static LegacyPreparedDictionarySourceSelectionInput FromObservedOrderTwoModel(
        LegacyPreparedDictionarySourceDescriptor descriptor, LegacyWordLanguageModel model, ulong contextOwnerIdentity)
    {
        ArgumentNullException.ThrowIfNull(descriptor); ArgumentNullException.ThrowIfNull(model);
        if (descriptor.LanguageOrder != 2) throw new ArgumentException("Matching observed order-two descriptor required.");
        return new(descriptor, checked((uint)model.ClassCount), contextOwnerIdentity);
    }
}

/// <summary>One24-byte native selected-row counterpart after236490.</summary>
public sealed record LegacyPreparedSelectedDictionarySource(int SourceIndex, uint Priority,
    uint AcceptedSkipBase, LegacyPreparedDictionarySourceDescriptor Descriptor, ulong ContextOwnerIdentity);

/// <summary>
/// Bounded236020/236200 add/remove and236490/236370 prepared selection producer.
/// Child factory/preparation is upstream; this does not call native source hooks.
/// Fresh all-source selection and distinct descriptor owners are supported.
/// Arbitrary selected subsets/shared mutable native descriptor aliases are not inferred.
/// </summary>
public sealed class LegacyPreparedDictionarySourceSelectionBuilder
{
    private readonly List<LegacyPreparedDictionarySourceSelectionInput> sources = [];
    public int SourceCount => sources.Count;

    public int Add(LegacyPreparedDictionarySourceSelectionInput source)
    {
        ArgumentNullException.ThrowIfNull(source); Validate(source);
        if (sources.Count == 255) throw new InvalidOperationException("Native aggregate permits at most255 sources.");
        if (sources.Any(s => ReferenceEquals(s.Descriptor, source.Descriptor)))
            throw new NotSupportedException("Shared mutable native descriptor aliases are outside the prepared distinct-owner scope.");
        int index = sources.Count; sources.Add(source); return index;
    }

    /// <summary>236200 removes one source/selected row and shifts remaining indices, preserving relative insertion order.</summary>
    public LegacyPreparedDictionarySourceSelectionInput Remove(int sourceIndex)
    {
        if ((uint)sourceIndex >= sources.Count) throw new ArgumentOutOfRangeException(nameof(sourceIndex));
        var removed = sources[sourceIndex]; sources.RemoveAt(sourceIndex); return removed;
    }

    /// <summary>
    /// Prepare descriptor rows after the upstream child.prepare/descriptor getter
    /// succeeded.236490 uses first stored n-gram count for categories800/C00 with
    /// language present, otherwise1.236370 sorts unsigned priority then source
    /// index descending. Native float summation follows that order and divides
    /// only Weight24 when sum&gt;1. Skip base resets toFFFFFFFF. Source metadata,
    /// other descriptor fields and context ownership retain their supplied facts.
    /// </summary>
    public IReadOnlyList<LegacyPreparedSelectedDictionarySource> Prepare(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();var result=new LegacyPreparedSelectedDictionarySource[sources.Count];
        for(int i=0;i<result.Length;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();var source=sources[i];Validate(source);
            uint category=source.Descriptor.Flags & 0x1e00;
            uint priority=category is 0x800 or 0xc00 ? source.FirstStoredNgramCount ?? 1u : 1u;
            result[i]=new(i,priority,uint.MaxValue,source.Descriptor,source.ContextOwnerIdentity);
        }
        Sort(result,0,result.Length-1,cancellationToken);
        float sum=0;
        foreach(var item in result){cancellationToken.ThrowIfCancellationRequested();sum+=item.Descriptor.Weight24;}
        if(!float.IsFinite(sum))throw new InvalidDataException("Prepared fixed-point source weights overflowed their bounded sum.");
        if(sum>1)
            for(int i=0;i<result.Length;i++)
            {
                cancellationToken.ThrowIfCancellationRequested();var row=result[i];
                result[i]=row with { Descriptor=row.Descriptor with { Weight24=row.Descriptor.Weight24/sum } };
            }
        return Array.AsReadOnly(result);
    }

    private static void Validate(LegacyPreparedDictionarySourceSelectionInput input)
    {
        ArgumentNullException.ThrowIfNull(input.Descriptor);var d=input.Descriptor;
        if(d.LanguageOrder>3 || ((d.Flags & 0x1c) != (d.LanguageOrder*4 & 0x1c)) ||
            (d.LanguageOrder==0) != (input.FirstStoredNgramCount is null) ||
            !float.IsFinite(d.Weight24) || !float.IsFinite(d.Weight28) ||
            Math.Abs(d.Weight24)>32768 || Math.Abs(d.Weight28)>32768 ||
            !float.IsFinite(d.RuntimeLanguageScale) || d.RuntimeLanguageScale<=0 || d.Identifier is null)
            throw new ArgumentException("Matching prepared language-presence/count facts and finite native fixed-point weights required.");
    }

    //236370 copies all24 bytes per exchange. Here whole immutable typed rows move.
    private static void Sort(LegacyPreparedSelectedDictionarySource[] rows,int first,int last,CancellationToken token)
    {
        while(first<last)
        {
            token.ThrowIfCancellationRequested();int i=first,j=last;
            var pivot=rows[first+(last-first)/2];
            while(i<=j)
            {
                while(Before(rows[i],pivot))i++;
                while(Before(pivot,rows[j]))j--;
                if(i>j)break;
                (rows[i],rows[j])=(rows[j],rows[i]);i++;j--;
            }
            if(first<j)Sort(rows,first,j,token);first=i;
        }
    }
    private static bool Before(LegacyPreparedSelectedDictionarySource a,LegacyPreparedSelectedDictionarySource b)
        => a.Priority>b.Priority || a.Priority==b.Priority && a.SourceIndex>b.SourceIndex;
}
