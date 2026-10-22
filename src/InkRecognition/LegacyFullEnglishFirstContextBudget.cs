namespace InkRecognition;

//Library logical work/copy reservations, not native limits or CLR/process RSS.
internal sealed class LegacyFullEnglishFirstContextBudget
{
    internal const long PeakLimit=64L*1024*1024,WorkLimit=256L*1024*1024,VisitLimit=128L*1024*1024;
    internal readonly long Peak,Work,Visits;
    internal readonly int Groups,Arcs,Nodes,Connections,ResolveAttempts;
    internal static (long Peak,long Work) Envelope(int g,int a,int n,int e,int k)
    {
        if(g is <0 or >4096||a is <0 or >4096||n is <0 or >4096||e is <0 or >1000000||k is <0 or >32768)throw new ArgumentException("B4 cumulative admission exceeded.");
        return(checked(4L*1024*1024+4096L*a+4096L*n+512L*g+16L*e+320L*k),checked(4L*1024*1024+24576L*a+16384L*n+1024L*g+24L*e+1536L*k));
    }
    internal LegacyFullEnglishFirstContextBudget(IReadOnlyList<LegacyFullEnglishFirstPrimaryChild> children,long peak,long work,long visits,CancellationToken token)
    {
        if(IntPtr.Size>8||System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<LegacyFirstAcousticRecordFields>()||System.Runtime.CompilerServices.Unsafe.SizeOf<LegacyFirstAcousticRecordFields>()>64||
            System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<LegacyFirstAcousticShortlistCandidate>()||System.Runtime.CompilerServices.Unsafe.SizeOf<LegacyFirstAcousticShortlistCandidate>()>16||
            System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<LegacyRankedAcousticEntry>()||System.Runtime.CompilerServices.Unsafe.SizeOf<LegacyRankedAcousticEntry>()>32)
            throw new NotSupportedException("B4 reference-free slot/layout domain exceeded.");
        if(peak<0||peak>PeakLimit||work<0||work>WorkLimit||visits<0||visits>VisitLimit)throw new ArgumentException("Private B4 bounds required.");
        int g=0,a=0,n=0,e=0,k=0;long v=0;
        foreach(var child in children)
        {
            token.ThrowIfCancellationRequested();var c=child.Initializer.ActualCache.Owner;var graph=c.SourceRanges.Graph;
            int cn=graph.NodeCount,cg=c.CopyLiveGroupIds().Length,ca=0,d=0,p=0;long t=0,r=0,ck=0;
            if(cn is <1 or >4096||child.Primary.NodeCount!=cn||child.Primary.BeamWidth!=5||cg>graph.CandidateCount)throw new ArgumentException("Canonical full primary dimensions/live arc witnesses required.");
            //Two bounded primary copies per node are within the fixed4MiB small
            //preflight allowance. Large geometry/selector/entry copies follow admission.
            var counts=new byte[cn];for(int i=0;i<cn;i++){token.ThrowIfCancellationRequested();var row=child.Primary.GetRowCopy(i);if(row.Count>5)throw new InvalidDataException("Actual primary beam5 required.");counts[i]=checked((byte)row.Count);}
            for(int i=0;i<cn;i++)
            {
                token.ThrowIfCancellationRequested();var arcs=graph.GetNode(i);int roots=0;
                foreach(var arc in arcs)
                {
                    token.ThrowIfCancellationRequested();ca=checked(ca+1);e=checked(e+arc.Successors.Count);d=Math.Max(d,arc.Successors.Count);p=Math.Max(p,arc.PredecessorArcIndices.Length);
                    if(arc.Span>7||arc.Span>i||(arc.Flags&8)!=0)throw new NotSupportedException("Actual ordinary full graph domain required.");
                    if(i==arc.Span&&arc.PredecessorArcIndices.Span.Contains(-1)){e=checked(e+1);roots++;}
                }
                d=Math.Max(d,roots);var row=child.Primary.GetRowCopy(i);
                for(int j=0;j<row.Count;j++)
                {
                    token.ThrowIfCancellationRequested();var entry=row.Get(j);int parent=i-entry.Span-1;
                    if(entry.Traits!=0||entry.Span>7||parent<-1||entry.ArcOrdinal>=arcs.Count||arcs[entry.ArcOrdinal].Span!=entry.Span||
                        parent==-1&&entry.PreviousAlternative!=255||parent>=0&&entry.PreviousAlternative>=counts[parent])throw new InvalidDataException("Canonical zero-trait primary predecessor required.");
                    r=checked(r+1);if(parent==-1)ck=checked(ck+2);else{t=checked(t+counts[parent]);ck=checked(ck+4L*counts[parent]);}
                }
            }
            g=checked(g+cg);a=checked(a+ca);n=checked(n+cn);if(ck>32768-k)throw new ArgumentException("B4 whole attempted-resolve budget exceeded before entry.");k=checked(k+(int)ck);
            v=checked(v+1024L*cn+512L*ca+320L*r+2*t*(cn+d+32L)+6*t*(p+128L)+128*ck);
            var x=Envelope(g,a,n,e,k);if(x.Peak>peak||x.Work>work||v>visits)throw new ArgumentException("B4 parent-wide logical reservation refused before entry.");
        }
        (Peak,Work)=Envelope(g,a,n,e,k);Visits=v;Groups=g;Arcs=a;Nodes=n;Connections=e;ResolveAttempts=k;
    }
}
