namespace InkRecognition;

/// <summary>Wrapper292.V48=231AE0: owning terminal-only inner record32 query,
/// with transported case state. This is distinct from normal233090 expansion
/// and from the empty2B7.V58/24-byte continuation backend.</summary>
public sealed class LegacyNativeCaseAuxiliaryQuerySource
{
    private readonly LegacyAggregateDictionaryReader reader;
    private int active;
    private bool valid=true;
    public LegacySymbolContextFactory Contexts {get;}
    public LegacyOrdinaryCursiveCandidateGraph Graph {get;}
    private LegacyNativeCaseAuxiliaryQuerySource(LegacyAggregateDictionaryReader reader,
        LegacySymbolContextFactory contexts,LegacyOrdinaryCursiveCandidateGraph graph)
    {this.reader=reader;Contexts=contexts;Graph=graph;}
    public static LegacyNativeCaseAuxiliaryQuerySource BindOrdinaryDictionary(
        LegacyPreparedDictionaryQuerySource source,LegacySymbolContextFactory contexts,
        LegacyOrdinaryCursiveCandidateGraph graph)
    {
        var bound=LegacyOrdinaryDictionarySourceOwner.BindDictionary(source,contexts,graph);
        return new(bound.Read,contexts,graph);
    }
    /// <summary>All five transported state bits are valid here, including31;
    /// no case-policy row is dereferenced by231AE0. Source/header/remaining bit7
    /// values remain exact. Source tag is request data, never a pointer.</summary>
    public LegacyPreparedDictionaryQueryPage Read(ulong sourceTag,LegacyPreparedDictionaryQuery query,
        bool extraCosts=false,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(query);cancellationToken.ThrowIfCancellationRequested();
        if(Interlocked.CompareExchange(ref active,1,0)!=0)throw new InvalidOperationException("Case auxiliary source calls must be serialized.");
        try
        {
            if(!valid)throw new InvalidOperationException("Failed auxiliary source requires a fresh session.");
            if((uint)query.Node!=(uint)sourceTag)throw new ArgumentException("Source tag and prepared query node differ.");
            var before=(query.Node,query.Skip,query.Capacity,query.ContextRoot,query.ParentContext,
                query.Parameter1c,query.Parameter20,query.Flags24,query.Flags26,query.Category18,query.Parameter19);
            var page=reader(query,true,extraCosts,cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var after=(query.Node,query.Skip,query.Capacity,query.ContextRoot,query.ParentContext,
                query.Parameter1c,query.Parameter20,query.Flags24,query.Flags26,query.Category18,query.Parameter19);
            if(before!=after)throw new InvalidDataException("Auxiliary child changed prepared request fields.");
            int count=Math.Max(0,page.ReturnedCount);
            if(count>query.Capacity||page.Records.Length!=count*32||
                (extraCosts?page.ClearedExtraCosts.Length!=count*12:!page.ClearedExtraCosts.IsEmpty))
                throw new InvalidDataException("Auxiliary child record/cost extent differs.");
            byte[] bytes=page.Records.ToArray();byte wanted=(byte)(sourceTag>>56);
            for(int i=0;i<count;i++)
            {cancellationToken.ThrowIfCancellationRequested();int at=i*32+7;bytes[at]=(byte)((bytes[at]&0x83)|(wanted&0x7c));}
            return page with {Records=bytes,ClearedExtraCosts=page.ClearedExtraCosts.ToArray()};
        }
        catch{valid=false;throw;}
        finally{Volatile.Write(ref active,0);}
    }
}
