namespace InkRecognition;
public sealed partial class LegacyOrdinaryGraphSourceRanges
{
    internal LegacyOrdinaryGraphSourceReference[] ResolvePeriodicForwardWithResiduals(LegacyEnglish119MainLocalCandidates.GraphWordRangeReceipt receipt,
        uint first,uint last,CancellationToken token)
    {
        receipt.RequirePeriodicSource(this,token);
        return ResolvePeriodicForwardRows(receipt,first,last,token).Select(r=>new LegacyOrdinaryGraphSourceReference(r.First.Key,r.Tail?r.Last.Key:r.LastStart.Key,
            r.First.RemainderByte,r.Tail?r.Last.RemainderByte:r.LastStart.RemainderByte)).ToArray();
    }
    private List<Row> ResolvePeriodicForwardRows(LegacyEnglish119MainLocalCandidates.GraphWordRangeReceipt receipt,uint firstContext,uint lastContext,CancellationToken cancellationToken)
    {
        receipt.RequirePeriodicSource(this,cancellationToken);cancellationToken.ThrowIfCancellationRequested();
        if(firstContext>int.MaxValue||lastContext>int.MaxValue||firstContext>lastContext||
            (firstContext>>7)>=(uint)seedGraph.NodeCount||(lastContext>>7)>=(uint)seedGraph.NodeCount)
            throw new NotSupportedException("Bounded ordinary forward graph context interval required.");
        var frames=new List<(int Node,int Ordinal)>();int visits=0;bool found=false;
        int firstNode=(int)(firstContext>>7);
        for(int ordinal=0;ordinal<seedGraph.ReadSourceNode(firstNode).Length&&!found;ordinal++)
        {
            receipt.ChargeVisits(32);var arc=seedGraph.ReadSourceNode(firstNode)[ordinal];if(arc.StartContextId!=firstContext)continue;
            frames.Clear();var stack=new Stack<(int Node,int Ordinal,int Child)>();stack.Push((firstNode,ordinal,-1));
            while(stack.Count!=0)
            {
                cancellationToken.ThrowIfCancellationRequested();if(++visits>1_048_576)throw new ArgumentException("Source path visit budget exceeded.");
                receipt.ChargeVisits(64);var frame=stack.Pop();var current=seedGraph.ReadSourceNode(frame.Node)[frame.Ordinal];
                if(frame.Child==-1)
                {
                    if(lastContext<current.EndContextId)continue;
                    frames.Add((frame.Node,frame.Ordinal));
                    if(current.EndContextId==lastContext){found=true;break;}
                    stack.Push((frame.Node,frame.Ordinal,0));
                }
                else if(frame.Child<current.SuccessorArcIndices.Length)
                {
                    stack.Push((frame.Node,frame.Ordinal,frame.Child+1));
                    int next=current.SuccessorArcIndices.Span[frame.Child];
                    if(frame.Node+1>=seedGraph.NodeCount||next<0||next>=seedGraph.ReadSourceNode(frame.Node+1).Length)
                        throw new InvalidDataException("Actual seed successor ownership required.");
                    stack.Push((frame.Node+1,next,-1));
                }
                else frames.RemoveAt(frames.Count-1);
            }
        }
        if(!found)throw new InvalidDataException("Actual ordinary source path does not reach its requested ending context.");
        receipt.ChargeVisits(checked(256L+frames.Count*256L+frames.Count*(long)frames.Count*32L+keys.Length*8L));
        var path=frames.Select(frame=>seeds.Seeds[seedGraph.ReadSourceNode(frame.Node)[frame.Ordinal].SeedGroupIndex].Atom.Range).ToArray();
        return ProjectOwnedRows(prepared,original,path,keys,cancellationToken);
    }

}
