namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    // This is a private live continuation, not a snapshot/result ingress. The
    // caller still owns sourceV80(currentNode) and must perform its actual
    // normal/auxiliary/backend reset after this numeric common tail succeeds.
    private LegacyNativeWordHeapDescriptor.Node0Phase1Result CompleteGraphPhase1(
        GraphLanguageScope stage,int[] ordinals,CancellationToken token)
    {
        stage.Require(token);
        if(!ReferenceEquals(stage.Owner,this))
            throw new InvalidOperationException("Exact generic phase1 owner required.");
        var result=stage.Descriptor.CompleteGraphFirstPhase1(stage,ordinals,token);
        _=stage.Ring.Cleanup(token);
        stage.Ring.ClearNode0Counters(result.SourceEnd,result.RetainedEnd);
        token.ThrowIfCancellationRequested();stage.RequirePhase1();
        return result;
    }
}
