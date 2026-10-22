using System.Runtime.CompilerServices;
namespace InkRecognition;

//Logical per-run library reservations, not RSS/GC arena or native allocation sizes.
internal sealed class LegacyFullEnglishFirstTerminalBudget
{
    internal const long PeakLimit=16L*1024*1024,WorkLimit=32L*1024*1024,VisitLimit=8L*1024*1024;
    internal readonly int Nodes,Children;
    internal readonly long Peak,Work,Visits;
    internal LegacyFullEnglishFirstTerminalBudget(IReadOnlyList<LegacyFullEnglishFirstContextChild> children,long peak,long work,long visits,CancellationToken token)
    {
        if(IntPtr.Size>8||RuntimeHelpers.IsReferenceOrContainsReferences<LegacyFirstTerminalChoice>()||Unsafe.SizeOf<LegacyFirstTerminalChoice>()>64)
            throw new NotSupportedException("Bounded reference-free terminal choice slot required.");
        Children=children.Count;if(Children is<1 or>256)throw new ArgumentException("Whole actual full B4 child set required.");
        foreach(var child in children){token.ThrowIfCancellationRequested();int count=child.NodeCount;if(count<1||count>4096-Nodes)throw new ArgumentException("At most4096 aggregate actual B8 nodes required.");Nodes=checked(Nodes+count);}
        (Peak,Work,Visits)=Calculate(Nodes,Children);
        if(peak<0||peak>PeakLimit||work<0||work>WorkLimit||visits<0||visits>VisitLimit||Peak>peak||Work>work||Visits>visits)
            throw new ArgumentException("Whole B5 logical budget refused before row/grid/trace allocation.");
    }
    internal static(long Peak,long Work,long Visits) Calculate(int nodes,int children)
    {
        if(nodes is<1 or>4096||children is<1 or>256||children>nodes)throw new ArgumentOutOfRangeException(nameof(nodes));
        return(checked(4194304L+2048L*nodes+1024L*children),checked(4194304L+4096L*nodes+2048L*children),checked(1024L*nodes+4096L*children));
    }
}
