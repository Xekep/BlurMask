namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private LegacyAggregateDictionaryQueryPage ReadGraphAuxiliary(GraphLanguageScope stage,ulong tag,LegacyPreparedDictionaryQuery query,CancellationToken token)
    {
        stage.Require(token);var plan=stage.Lease.Pass.Initializer.PlanForAudit;
        if(query.Capacity!=128||query.ContextRoot!=plan.BindingForAudit.Contexts.ContainerIdentity||graphControllerValues is null||graphControllerMask is null)
            throw new InvalidOperationException("Same privately initialized generic auxiliary query/controller required.");
        plan.BindingForAudit.Dictionary.RequireGraphEmptyAuxiliary(index,stage);
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryOuter,4096);
        index.ChargeEmptyAuxiliaryRead(tag);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryOuter,926);
        // Exact empty-map/index/scratch identity is authenticated BEFORE the new metadata-read precharge above.
        // The actual immutable empty auxiliary map and retained dictionary scratch were authenticated above.
        // Its4096 output array is already reserved. Cover closure/DTO/query-metadata wrappers before entering the transitive empty path.
        if(graphPeriodicActive){ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryMetadata,1024);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryMetadata,1024);}
        var page=plan.ReadGraphAuxiliary(stage,tag,query,token);
        if(page.ReturnedCount!=0||!page.Records.IsEmpty)throw new NotSupportedException("Actual immutable empty auxiliary map branch required.");
        stage.Require(token);return new(page.ReturnedCount,page.Records,ReadOnlyMemory<byte>.Empty,page.SlotIndex,page.AcceptedCount,null);
    }
}
public sealed partial class LegacyPreparedDictionaryQuerySource
{
    internal void RequireGraphEmptyAuxiliary(LegacyEnglishCaseNode0LexiconIndex index,LegacyEnglishCaseNode0Completion.GraphLanguageScope stage)
    {
        stage.Require();index.ChargeBindingChecks();
        if(reading||!ReferenceEquals(index.Numeric,stage.Lease.Root.NumericOwner)||!ReferenceEquals(lexicon,index.Lexicon)||!map.SharesAuxiliaryMap(stage.Lease.Map)||recordScratch.Length<4096)
            throw new InvalidOperationException("Same initialized generic dictionary/immutable auxiliary map and retained scratch required.");
        index.RequireEmptyAuxiliary(map.ReadSelectedMap(true,0));
    }
}
internal sealed partial class LegacyEnglishCaseInitializationPlan
{
    internal LegacyNativeDelimiterQueryPage ReadGraphAuxiliary(LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,ulong tag,LegacyPreparedDictionaryQuery query,CancellationToken token)
    {
        stage.Require(token);RequirePublished(token);
        if(!ReferenceEquals(stage.Lease.Pass.Initializer.PlanForAudit,this)||!ReferenceEquals(stage.Lease.Root,BoundRoot)||executionPool is null||Volatile.Read(ref executionReset)!=1)
            throw new InvalidOperationException("Exact active generic graph initialized auxiliary owner required.");
        stage.Lease.Root.ValidateNode1(stage.Lease);ValidateParent(token);
        var page=auxiliary!.Read(tag,query,token);stage.Require(token);return page;
    }
}
