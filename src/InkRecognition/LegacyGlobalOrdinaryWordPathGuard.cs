namespace InkRecognition;

//1FCF40 marked ordinary branch. Only the actual one-source C00 (=kind6)
//case dictionary is accepted here; kind7/interval/source-kind5 exceptions
//are not substituted for this branch.
internal sealed class LegacyGlobalOrdinaryWordPathGuard
{
    private readonly LegacyOwnedMainWordSourceServices source;
    internal LegacyGlobalOrdinaryWordPathGuard(LegacyOwnedMainWordSourceServices source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if(!source.IsGlobal||!source.IsCaseAware||source.CopySourceHeaders() is not [0,var header]||
            (header>>9&15)!=6)throw new ArgumentException("Actual global fresh implicit0/C00 case source required.");
        this.source=source;
    }
    internal bool AllowsMarked(LegacyNativeWordRecombinationRequest request,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);token.ThrowIfCancellationRequested();
        if(request.ReadByte(28)is not(0 or 2)||request.ReadUInt32(68)!=6||request.ReadByte(60)==0)
            throw new NotSupportedException("Actual marked ordinary request and previous C00 source required.");
        int current=request.ReadInt32(0),previous=request.ReadInt32(4),next=unchecked(previous+1);
        if((uint)current>=(uint)source.Graph.NodeCount||(uint)previous>=(uint)source.Graph.NodeCount)
            throw new ArgumentOutOfRangeException(nameof(request));
        if(next>=unchecked(current+1))return false;
        int line=ReadLine(previous,request.ReadUInt16(14),token);
        if(line==-1)return false;
        int nextLine=ReadLine(next,0,token);
        token.ThrowIfCancellationRequested();return nextLine!=line&&unchecked(nextLine-line)==1;
    }
    private int ReadLine(int node,ushort arc,CancellationToken token)
    {
        var graph=source.Graph.GlobalSource!;var row=graph.State.GetNode(node);
        if(arc>=row.Count)throw new ArgumentOutOfRangeException(nameof(arc));
        return graph.ResolveGroupSource(checked((int)row[arc].GroupId),token).Primary.LineId;
    }
}
