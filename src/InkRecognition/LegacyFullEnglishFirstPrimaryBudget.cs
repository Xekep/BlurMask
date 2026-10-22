namespace InkRecognition;

//Logical allocation/copy and visit accounting. None of these bounds claims RSS.
internal sealed class LegacyFullEnglishFirstPrimaryBudget
{
    internal const long PeakLimit=64L*1024*1024,WorkLimit=192L*1024*1024,VisitLimit=128L*1024*1024;
    internal readonly long Peak,Work,Visits;
    internal readonly int Groups,Arcs,Nodes,Connections;
    internal static (long Peak,long Work) Envelope(int g,int a,int n,int e)
    {
        if(g is <0 or >4096||a is <0 or >4096||n is <0 or >4096||e is <0 or >1000000)throw new ArgumentException("B3 cumulative structural admission exceeded.");
        return(checked(2L*1024*1024+8192L*a+1536L*n+512L*g+16L*e),checked(2L*1024*1024+36864L*a+4096L*n+512L*g+24L*e));
    }
    internal LegacyFullEnglishFirstPrimaryBudget(IReadOnlyList<LegacyFullEnglishFirstControllerChild> children,long peak,long work,long visits,CancellationToken token)
    {
        if(IntPtr.Size>8||System.Runtime.CompilerServices.Unsafe.SizeOf<LegacyFirstOrdinaryGlyphRequest>()>32||System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<LegacyFirstOrdinaryGlyphRequest>())throw new NotSupportedException("B3 reservation layout domain exceeded.");
        if(peak<0||peak>PeakLimit||work<0||work>WorkLimit||visits<0||visits>VisitLimit)throw new ArgumentException("Private B3 bounds required.");
        int g=0,a=0,n=0,e=0;long v=0;
        foreach(var child in children)
        {
            token.ThrowIfCancellationRequested();var c=child.ActualCache.Owner;var graph=c.SourceRanges.Graph;var groups=graph.Groups;
            int ca=0,d=0,p=0,cg=c.CopyLiveGroupIds().Length;
            //The canonical producer forms live IDs by Distinct over these arcs.
            //Each retained group therefore has an arc witness: G<=A, even with sparse IDs.
            if(cg>graph.CandidateCount)throw new InvalidDataException("Live groups require actual arc witnesses.");
            g=checked(g+cg);n=checked(n+graph.NodeCount);
            if(graph.NodeCount==0)throw new NotSupportedException("Empty B3 child refused as a whole.");
            //Small temporary ID copies are covered by the fixed2MiB allowance.
            for(int i=0;i<graph.NodeCount;i++)
            {
                token.ThrowIfCancellationRequested();var row=graph.GetNode(i);if(row.Count==0)throw new NotSupportedException("Empty B3 node refused as a whole.");
                int roots=0;
                foreach(var arc in row)
                {
                    token.ThrowIfCancellationRequested();ca=checked(ca+1);a=checked(a+1);
                    if(arc.Span>7||arc.Span>i||(arc.Flags&8)!=0||!c.IsLive(arc.GroupId))throw new NotSupportedException("Same ordinary full FIRST arc domain required.");
                    var ranges=groups[arc.GroupId].Ranges;
                    if(ranges.Length is <1 or >15)throw new NotSupportedException("Actual full feature source range domain required.");
                    p=Math.Max(p,arc.PredecessorArcIndices.Length);int degree=arc.Successors.Count;d=Math.Max(d,degree);e=checked(e+degree);
                    foreach(var edge in arc.Successors)
                    {token.ThrowIfCancellationRequested();if(edge.NodeOffset is <1 or >15||i+edge.NodeOffset>=graph.NodeCount||edge.ArcIndex<0||edge.ArcIndex>=graph.GetNode(i+edge.NodeOffset).Count)throw new InvalidDataException("Actual successor ordinal required.");}
                    int previous=i-arc.Span-1;
                    foreach(int pred in arc.PredecessorArcIndices.Span)
                    {token.ThrowIfCancellationRequested();if(previous<0?pred!=-1:pred<0||pred>=graph.GetNode(previous).Count)throw new InvalidDataException("Actual predecessor ordinal required.");}
                    if(i==arc.Span&&arc.PredecessorArcIndices.Span.Contains(-1)){e=checked(e+1);roots++;}
                    _=Envelope(g,a,n,e);
                }
                d=Math.Max(d,roots);
            }
            if(ca==0)throw new NotSupportedException("Empty B3 arcs refused as a whole.");
            long q=checked(41L*ca);v=checked(v+q*192+8L*ca+15L*cg+q*(1+5L*(graph.NodeCount+d+4L*p+32))+15L*graph.NodeCount);
            var x=Envelope(g,a,n,e);if(x.Peak>peak||x.Work>work||v>visits)throw new ArgumentException("B3 parent-wide logical preflight refused before allocation/entry.");
        }
        (Peak,Work)=Envelope(g,a,n,e);Visits=v;Groups=g;Arcs=a;Nodes=n;Connections=e;
    }
}
