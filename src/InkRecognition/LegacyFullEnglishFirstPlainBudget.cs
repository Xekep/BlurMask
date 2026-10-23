namespace InkRecognition;

//Conservative logical admission/work only, not process RSS. No reservations
//are refunded for hidden rows, duplicate descriptors or losing word interns.
internal sealed class LegacyFullEnglishFirstPlainBudget
{
    internal const long PeakLimit=64L<<20,WorkLimit=512L<<20,VisitLimit=256L<<20;
    internal long Peak{get;} internal long Work{get;} internal long Visits{get;}
    internal int TraceCells{get;} internal int RichAttempts{get;} internal int WordAttempts{get;}
    internal LegacyFullEnglishFirstPlainBudget(IReadOnlyList<LegacyFullEnglishFirstTerminalChild> children,long peak,long work,long visits,CancellationToken token)
    {
        if(children.Count is <1 or >256)throw new NotSupportedException("Bounded full B6 children required.");
        long t=0,g=0,p=0,v=0,q=0;
        foreach(var child in children)
        {
            token.ThrowIfCancellationRequested();int tc=child.Cells.Count;if(tc>256)throw new NotSupportedException("B6 child trace cap256.");
            t=checked(t+tc);if(t>4096)throw new NotSupportedException("B6 parent trace cap4096.");q=checked(q+(tc+2L)*(tc+2L));
            var candidates=child.Context.Primary.Initializer.ActualCache.Owner;var graph=candidates.SourceRanges.Graph;var groups=graph.Groups;
            var counts=candidates.Counts;p=checked(p+counts.Sum(x=>(long)x));if(p>8192)throw new NotSupportedException("B6 cut cap8192.");
            var selected=new HashSet<int>();
            foreach(var cell in child.Cells)
            {
                token.ThrowIfCancellationRequested();var arcs=graph.GetNode(cell.Node);
                if(cell.Arc>=arcs.Count||cell.Span!=arcs[cell.Arc].Span||cell.Column!=0)throw new InvalidDataException("Same full selected arc required.");
                int id=arcs[cell.Arc].GroupId;if(!selected.Add(id))continue;
                g++;if(g>4096)throw new NotSupportedException("B6 selected group cap4096.");
                foreach(var r in groups[id].Ranges.Span)
                {
                    token.ThrowIfCancellationRequested();if((uint)r.StrokeIndex>=(uint)counts.Length||r.StartPoint<0||r.EndPointExclusive>counts[r.StrokeIndex]||r.StartPoint>=r.EndPointExclusive)throw new InvalidDataException("Same cut range required.");
                    v=checked(v+r.EndPointExclusive-r.StartPoint);if(v>1048576)throw new NotSupportedException("B6 selected range visit cap.");
                }
            }
        }
        TraceCells=checked((int)t);RichAttempts=checked((int)(2*t+2*children.Count));WordAttempts=checked((int)(2*t));
        Peak=checked((8L<<20)+8192*t+2048*g+2048*children.Count+128*p+1024*WordAttempts);
        Work=checked((16L<<20)+32768*t+4096*g+4096*children.Count+512*p+256*q);
        Visits=checked(8192*t+1024*g+4096*children.Count+64*q+32*v);
        if(peak<0||peak>PeakLimit||work<0||work>WorkLimit||visits<0||visits>VisitLimit||Peak>peak||Work>work||Visits>visits)throw new ArgumentException("Full B6 logical preflight refused before entry.");
    }
}
