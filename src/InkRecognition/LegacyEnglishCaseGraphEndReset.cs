namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseInitializationPlan
{
    internal void ResetGraphNode(LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,CancellationToken token)
    {
        stage.RequirePhase1();RequirePublished(token);stage.ClaimEndReset(this);stage.ChargeReset();
        if(executionPool is null||Volatile.Read(ref executionReset)!=1||!ReferenceEquals(stage.Lease.Root,BoundRoot))
            throw new InvalidOperationException("Same initialized generic source required for pernode V80.");
        normal!.ResetForControllerNode(token);stage.Require(token);
        auxiliary!.ResetForControllerNode(token);stage.Require(token);
        aggregate!.PrepareControllerNode(token);stage.Require(token);
    }
}
