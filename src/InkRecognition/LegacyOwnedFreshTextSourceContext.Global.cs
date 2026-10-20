namespace InkRecognition;

public sealed partial class LegacyOwnedFreshTextSourceContext
{
    internal LegacyOwnedFreshTextSelection? CreateGlobalCharacterSelection(
        LegacyFreshMainGlobalProcessedGlyphSource actualSource,int preparedFailureStage,CancellationToken token)
    {
        actualSource.Owner.RequireCurrentSource(actualSource,token);
        var scope=actualSource.Owner.Scope;
        if(!actualSource.IsOrdinarySource||!ReferenceEquals(scope.SharedState.Selections,this)||
            !ReferenceEquals(InputBinding,scope.Context.InputBinding)||!ReferenceEquals(Document,actualSource.Document)||
            !ReferenceEquals(scope.Seed.PendingSelections,this)||!ReferenceEquals(scope.Seed.InputBinding,InputBinding))
            throw new ArgumentException("Actual ordinary global receipt, identical scope INPUT/Seed/CHAR transaction required.");
        long output=-1;
        //Production passes0. The internal diagnostics use the SAME actual
        //receipt with labelled native allocation-refusal controls1..4.
        return CreateCore(actualSource.CopyRanges(token),0,ref output,preparedFailureStage,token);
    }
}
