namespace InkRecognition;

public sealed partial class LegacyOwnedFreshTextSourceContext
{
    private void ValidateGlobalWordText(LegacyFreshMainGlobalWordTextPublication parent,CancellationToken token)
    {
        parent.RequireSelectionOwner(this,token);
        if(!ReferenceEquals(parent.Scope.SharedState.Selections,this)||!ReferenceEquals(parent.Scope.Seed.PendingSelections,this)||
            !ReferenceEquals(InputBinding,parent.Scope.Seed.InputBinding)||!ReferenceEquals(Document,parent.Scope.Document))
            throw new ArgumentException("Exact global Seed/INPUT/pending transaction required.");
    }
    internal LegacyOwnedFreshTextSelection? CreateGlobalWordSelection(LegacyFreshMainGlobalWordSourcePayload actual,int failure,CancellationToken token)
    {
        var parent=actual.OriginalSource.Owner;ValidateGlobalWordText(parent,token);parent.RequireWordRegistration(actual,token);
        long output=long.MaxValue;
        return CreateCore(actual.OriginalSource.CopyOriginalKey(),1,ref output,failure,token);
    }
    internal LegacyOwnedFreshTextSelection? CreateGlobalTextSelection(LegacyFreshMainGlobalTextSelectionReceipt actual,int failure,CancellationToken token)
    {
        ValidateGlobalWordText(actual.Owner,token);actual.Owner.RequireTextRegistration(actual,token);
        long output=-1;return CreateCore(actual.CopyRanges(),2,ref output,failure,token);
    }
}
