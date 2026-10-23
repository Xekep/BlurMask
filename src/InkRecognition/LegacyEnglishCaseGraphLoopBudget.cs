namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    internal const long GraphLoopAdditionalLogicalPayloadCeiling=32L*1024*1024;
    // Separate simultaneous payload reservation, additional to the original
    // source/model and node0 ledgers. This is not cumulative GC allocation,
    // CPU work or RSS. No old ceiling is raised by this opt-in path.
    internal static void PreflightGraphLoopPayload(LegacyEnglish119MainKnowledge knowledge,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var graph=knowledge.Owner.RetainCompletedD0Graph(token);
        if(graph.NodeCount is <2 or >49||graph.Groups.Count>4096||graph.CandidateCount>4096)
            throw new NotSupportedException("Whole generic-loop source extent exceeds its private payload reservation.");
        long arcs=0;
        for(int node=0;node<graph.NodeCount;node++)
        {
            var rows=graph.GetNode(node);if(rows.Count>1024)throw new NotSupportedException("Per-node graph arc cap exceeded.");
            foreach(var arc in rows){token.ThrowIfCancellationRequested();if(arc.Span>=8||arc.Span>node||++arcs>4096)throw new NotSupportedException("Actual whole bounded span schedule required.");}
        }
        if(arcs!=graph.CandidateCount)throw new InvalidDataException("Complete source arc accounting required.");
        const long geometry=16L*1024*1024; // Existing prepared source enforces this before geometry arrays.
        long selected=checked(arcs*2048L); // <=4096*(880 class +96 copied geometry +96 transient geometry +976 container/array/ref allowance).
        long history=checked(graph.NodeCount*156L*256); // rows88+primary80+record/ref/array allowances, once per retained node.
        const long heapAndMaterialization=2L*1024*1024; //250x128 heap, all sort/recombine/quota/materialize clones+two detached node images.
        const long pagesParentsAndMetadata=1024L*1024; //128x32 pages, fixed headers/requests/parent88 and bounded private callback frames.
        long peak=checked(geometry+selected+history+heapAndMaterialization+pagesParentsAndMetadata);
        if(peak>GraphLoopAdditionalLogicalPayloadCeiling)throw new NotSupportedException("Whole generic-loop simultaneous payload envelope exceeded before entry.");
    }
}
