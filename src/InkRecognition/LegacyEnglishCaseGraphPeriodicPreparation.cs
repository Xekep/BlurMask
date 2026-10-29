namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private bool graphPeriodicPrepared;
    // No public/raw bounds issuer: only this actual completed source can create the private receipt.
    internal static LegacyEnglishCaseNode0Completion PrepareGraphFirstPeriodicWordSourceCompletion(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,
        LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,long maximumLogicalBytes=128L*1024*1024,CancellationToken token=default)
    {
        var preparation=GraphPeriodicPreparation.Issue(knowledge,token);
        return PrepareNode1Spans(knowledge,numeric,cases,publication,maximumLogicalBytes,token,preparation);
    }
    internal readonly struct GraphPeriodicPreparation
    {
        private readonly LegacyEnglish119MainKnowledge knowledge;
        private readonly LegacyOrdinaryCursiveCandidateGraph graph;
        private readonly LegacyEnglishMainD0Bounds bounds;
        private GraphPeriodicPreparation(LegacyEnglish119MainKnowledge knowledge,LegacyOrdinaryCursiveCandidateGraph graph,LegacyEnglishMainD0Bounds bounds)
        {this.knowledge=knowledge;this.graph=graph;this.bounds=bounds;}
        internal static GraphPeriodicPreparation Issue(LegacyEnglish119MainKnowledge knowledge,CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(knowledge);token.ThrowIfCancellationRequested();var local=knowledge.Owner;var graph=local.RetainCompletedD0Graph(token);
            PreflightGraphLoopPayload(knowledge,token,true);
            int preparations=1,selected=0;
            for(int node=0;node<graph.NodeCount;node++)
            {
                var arcs=graph.GetNode(node);if(arcs.Count is<1 or>1024)throw new NotSupportedException("Actual nonempty periodic node schedule required.");int maximum=0;
                foreach(var arc in arcs){token.ThrowIfCancellationRequested();if(arc.Span>=8||arc.Span>node)throw new InvalidDataException("Actual periodic rawspan0..7 required.");maximum=Math.Max(maximum,arc.Span);selected=checked(selected+1);}
                if(node!=0)preparations=checked(preparations+maximum+1);
            }
            if(selected!=graph.CandidateCount||selected!=local.CandidateCount||graph.NodeCount!=local.NodeCount||graph.Groups.Count!=local.GroupCount)throw new InvalidDataException("Complete actual periodic D0 dimensions required.");
            var bounds=new LegacyEnglishMainD0Bounds(preparations,selected);
            long d0=LegacyEnglishMainSpanSession.Preflight(local.GroupCount,local.NodeCount,local.CandidateCount,bounds),score=LegacyEnglishMainInkSnapshot.Preflight(local.GroupCount,local.NodeCount,local.CandidateCount);
            if(checked(d0+score+1048576)>LegacyEnglishFreshCaseMainSource.CompleteLogicalEnvelopeUpperBytes)throw new ArgumentException("Complete additive periodic case envelope exceeded before claim.");
            return new(knowledge,graph,bounds);
        }
        internal LegacyEnglishMainD0Bounds Require(LegacyEnglish119MainKnowledge actual,CancellationToken token)
        {token.ThrowIfCancellationRequested();if(!ReferenceEquals(knowledge,actual)||!ReferenceEquals(graph,actual.Owner.RetainCompletedD0Graph(token)))throw new InvalidOperationException("Same completed source-bound periodic D0 preparation required.");return bounds;}
    }
}
