namespace InkRecognition;

//Private live topology receipt. Numerical/prepared node vectors cannot construct it.
internal sealed class LegacyNativePlainWordBatchBinding
{
    private readonly object nativeOwner;
    private readonly LegacyNativePlainWordPublisher publisher;
    internal readonly LegacyNativePublishedWordNode[] Nodes;
    internal readonly LegacyNativePublishedWordNode[] Owners;
    internal readonly LegacyNativePublishedWordNode[] Heads;
    internal readonly object[] Lists;
    internal readonly LegacyNativePlainWordRecord[] InitialWords;
    internal readonly int PublisherCount;
    internal LegacyNativePlainWordBatchBinding(LegacyNativePlainWordPublisher publisher,object nativeOwner,
        LegacyNativePublishedWordNode[] nodes,LegacyNativePublishedWordNode[] owners,object[] lists,int publisherCount)
    {this.publisher=publisher;this.nativeOwner=nativeOwner;Nodes=nodes;Owners=owners;Lists=lists;PublisherCount=publisherCount;
        Heads=[nodes[0]];InitialWords=owners.Select(n=>n.Word).ToArray();}
    internal void RequireIssuer(LegacyNativePlainWordPublisher actual,object owner)
    {if(!ReferenceEquals(actual,publisher)||!ReferenceEquals(owner,nativeOwner))throw new InvalidOperationException("Exact private batch topology issuer required.");}
}

public sealed partial class LegacyNativePlainWordPublisher
{
    //No copied payload or transferable view. All extents come from the SAME
    //private lists before the completed-batch WORD cursor allocates snapshots.
    internal (int Rows,int References,int Selections) InspectBatchTextSpan(LegacyNativePlainWordBatchBinding actual,
        int ownerOrdinal,CancellationToken token,LegacyFreshMainGlobalSourceBatchCompletion validationBudget)
    {
        validationBudget.RequireValidationBinding(this,actual);
        validationBudget.DebitValidationLoops(checked((long)actual.PublisherCount+actual.Nodes.Length+actual.Owners.Length),token);
        RequireSourceBatch(actual,false,token);
        if((uint)ownerOrdinal>=actual.Owners.Length)throw new ArgumentOutOfRangeException(nameof(ownerOrdinal));
        var node=actual.Owners[ownerOrdinal];var words=interning.Spans[node.Span];
        if(words.Count is <1 or >4095)throw new NotSupportedException("Bounded completed275 owner required.");
        validationBudget.DebitValidationLoops(checked((long)words.Count+actual.Nodes.Length),token);
        int references=0,selections=0;
        foreach(var row in words)
        {
            token.ThrowIfCancellationRequested();
            if(!ReferenceEquals(row.Pool,pool)||row.FlatReferences.Count is <1 or >4096||row.FlatReferences.Count>65536-references)
                throw new NotSupportedException("Completed275 owner reference extent exceeds batch TEXT subset.");
            references+=row.FlatReferences.Count;
        }
        foreach(var selected in actual.Nodes)if(selected.Span==node.Span)selections++;
        return(words.Count,references,selections);
    }
    //Read-only bounded preflight. Identity is actual private275 List, not copied rows.
    internal LegacyNativePlainWordBatchBinding CaptureSourceBatch(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(released||Head is null||interning.Publishers.Count is <1 or >4096)
            throw new NotSupportedException("Bounded actual active global publisher required.");
        int publishers=interning.Publishers.Count,active=0;
        for(int i=0;i<publishers;i++)if(!interning.Publishers[i].released&&interning.Publishers[i].Head is not null)active++;
        if(active!=1||interning.Publishers.Count!=publishers)throw new NotSupportedException("One actual TOP publisher required.");
        var nodes=new List<LegacyNativePublishedWordNode>();var owners=new List<LegacyNativePublishedWordNode>();
        var lists=new List<object>();var seen=new HashSet<LegacyNativePublishedWordNode>();
        for(var node=Head;node is not null;node=node.Next)
        {
            token.ThrowIfCancellationRequested();
            if(nodes.Count==4096||!seen.Add(node)||!ReferenceEquals(node.Word.Pool,pool)||
                !interning.Spans.TryGetValue(node.Span,out var words)||words.Count!=1||node.WordIndex!=0||!ReferenceEquals(words[0],node.Word)||
                node.Span.First<0||node.Span.Last<node.Span.First)
                throw new NotSupportedException("Unchanged bounded forward singleton original275 owners required.");
            nodes.Add(node);
            if(!lists.Any(list=>ReferenceEquals(list,words)))
            {
                if(owners.Count==64)throw new NotSupportedException("Operational batch supports at most64 actual span owners.");
                owners.Add(node);lists.Add(words);
            }
        }
        var result=new LegacyNativePlainWordBatchBinding(this,interning,nodes.ToArray(),owners.ToArray(),lists.ToArray(),publishers);
        RequireSourceBatch(result,true,token);return result;
    }
    internal void RequireSourceBatch(LegacyNativePlainWordBatchBinding actual,bool beforeEntry,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();actual.RequireIssuer(this,interning);
        if(released||interning.Publishers.Count!=actual.PublisherCount||!ReferenceEquals(Head,actual.Heads[0]))
            throw new InvalidOperationException("Actual batch publisher history/head changed.");
        int active=0;for(int i=0;i<actual.PublisherCount;i++)if(!interning.Publishers[i].released&&interning.Publishers[i].Head is not null)active++;
        if(active!=1)throw new InvalidOperationException("Actual batch TOP alternatives changed.");
        var cursor=Head;
        for(int i=0;i<actual.Nodes.Length;i++)
        {
            token.ThrowIfCancellationRequested();var expected=actual.Nodes[i];
            if(!ReferenceEquals(cursor,expected)||!interning.Spans.TryGetValue(expected.Span,out var rows)||
                (uint)expected.WordIndex>=rows.Count||!ReferenceEquals(rows[expected.WordIndex],expected.Word)||!ReferenceEquals(expected.Word.Pool,pool))
                throw new InvalidOperationException("Actual batch279 chain/current275 binding changed.");
            cursor=cursor!.Next;
        }
        if(cursor is not null)throw new InvalidOperationException("Actual batch279 tail changed.");
        for(int i=0;i<actual.Owners.Length;i++)
        {
            var owner=actual.Owners[i];
            if(!interning.Spans.TryGetValue(owner.Span,out var rows)||!ReferenceEquals(rows,actual.Lists[i])||
                beforeEntry&&(rows.Count!=1||!ReferenceEquals(rows[0],actual.InitialWords[i])))
                throw new InvalidOperationException("Actual private batch275 owner changed.");
        }
    }
}
