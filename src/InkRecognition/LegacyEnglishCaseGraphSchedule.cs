namespace InkRecognition;
// Numeric schedule only. Values are not D0/source/query authority.
internal static class LegacyEnglishCaseGraphSchedule
{
    internal static int MaximumSpan(int node,IReadOnlyList<LegacyOrdinaryRecognitionGraph.Arc> arcs,CancellationToken token)
    {
        if(node is <1 or >48||arcs.Count is <1 or >1024)throw new NotSupportedException("Bounded current graph node/arc extent required.");
        int maximum=0;
        foreach(var arc in arcs){token.ThrowIfCancellationRequested();if(arc.Span>=8||arc.Span>node)throw new NotSupportedException("Actual bounded nonnegative predecessor span required.");maximum=Math.Max(maximum,arc.Span);}
        return maximum;
    }
    internal static int PreviousNode(int node,byte span)
    {if(node is <1 or >48||span>=8||span>node)throw new NotSupportedException("Actual bounded predecessor required.");return node-span-1;}
    internal static (int Node,int Span) AfterSpan(int node,byte span,bool last)
    {_=PreviousNode(node,span);return last?(node+1,0):(node,span+1);}
}
