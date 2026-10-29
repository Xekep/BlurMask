using System.Text;

namespace InkRecognition.Math;

internal sealed partial class Math2Mode0OriginalArena(LegacyLibraryOriginalDomNameOwner names)
{
    internal sealed class Node(int id,byte kind,LegacyLibraryOriginalDomName name,int original,int payload)
    {
        internal readonly int Id=id,OriginalId=original,PayloadOrigin=payload;internal readonly byte Kind=kind;
        internal readonly LegacyLibraryOriginalDomName Name=name;internal int Parent;internal byte Tag;
        internal string? Text,Uri,Prefix;internal long? Number;
    }
    internal readonly List<Node> Nodes=[];
    internal bool HasCanonicalNames(LegacyLibraryOriginalDomNameOwner actual)
        =>ReferenceEquals(names,actual)&&Nodes.All(n=>ReferenceEquals(n.Name.Owner,actual)&&ReferenceEquals(n.Name,actual.Intern(n.Name.Text)));
    private readonly Dictionary<int,List<Node>> children=[];
    internal IReadOnlyList<Node> Children(int id)=>children.TryGetValue(id,out var list)?list:[];
    private Node New(byte kind,string name,int original,int payload,byte tag,string? text,long? number,string? uri,string? prefix)
    {var node=new Node(Nodes.Count+1,kind,names.Intern(name),original,payload){Tag=tag,Text=text,Number=number,Uri=uri,Prefix=prefix};Nodes.Add(node);return node;}
    internal void Import(IReadOnlyList<Math2Mode0MathFieldNode> source)
    {
        foreach(var n in source){var node=New(n.Kind,n.Name,n.Id,0,n.ValueTag,n.StringValue,n.NumericValue,n.NamespaceUri,n.NamespacePrefix);if(node.Id!=n.Id)throw new NotSupportedException("Whole contiguous historical PAGE required.");node.Parent=n.Parent;if(n.Parent!=0){if(!children.TryGetValue(n.Parent,out var list))children[n.Parent]=list=[];list.Add(node);}}
    }
    internal int CloneOriginal(Math2Mode0OriginalArena source,int identity,int parent,Action<int> observer)
    {
        var n=source.Nodes[identity-1];var result=New(n.Kind,n.Name.Text,n.OriginalId,0,n.Tag,n.Text,n.Number,n.Uri,n.Prefix);if(parent!=0)Link(parent,result);observer(result.Id);
        foreach(var child in source.Children(identity).Where(n=>n.Kind==0))CloneOriginal(source,child.Id,result.Id,observer);
        foreach(var child in source.Children(identity).Where(n=>n.Kind!=0))CloneOriginal(source,child.Id,result.Id,observer);return result.Id;
    }
    internal int ClonePayload(IReadOnlyList<Math2Mode0PublishNode> source,int identity,int parent,Action<int> observer)
    {
        var index=source.GroupBy(n=>n.Parent).ToDictionary(g=>g.Key,g=>g.ToArray());var byId=source.ToDictionary(n=>n.Id);
        int Clone(int id,int attach)
        {
            var n=byId[id];var result=New(n.Kind,n.Name,0,n.Id,n.ValueTag,n.Text,n.Number,n.NamespaceUri,n.NamespacePrefix);if(attach!=0)Link(attach,result);observer(result.Id);
            if(index.TryGetValue(id,out var list)){foreach(var child in list.Where(n=>n.Kind==0))Clone(child.Id,result.Id);foreach(var child in list.Where(n=>n.Kind!=0))Clone(child.Id,result.Id);}return result.Id;
        }
        return Clone(identity,parent);
    }
    private void Link(int parent,Node node)
    {node.Parent=parent;if(!children.TryGetValue(parent,out var list))children[parent]=list=[];list.Add(node);}
    internal void Attach(int parent,int child,int ordinal)
    {
        var node=Nodes[child-1];if(node.Parent!=0||ordinal<0)throw new ArgumentException("Actual unattached payload/ordinal required.");
        if(!children.TryGetValue(parent,out var list))children[parent]=list=[];
        var actual=list.Where(n=>n.Kind!=0).ToArray();if(ordinal>actual.Length)throw new NotSupportedException("Fresh actual insertion ordinal required.");
        int index=ordinal==actual.Length?list.Count:list.IndexOf(actual[ordinal]);node.Parent=parent;list.Insert(index,node);
    }
    internal Math2Mode0OriginalDomNode[] Snapshot()=>Nodes.Select(n=>new Math2Mode0OriginalDomNode(n.Id,n.Kind,n.Name.Text,n.Parent,n.OriginalId,n.PayloadOrigin,n.Tag,n.Text,n.Number,n.Uri,n.Prefix)).ToArray();
}

internal static class Math2Mode0OriginalKernel
{
    internal static long Plan(Math2Mode0OriginalInput input,Math2Mode0OriginalTransferInput transfer,long budget,CancellationToken token)
    {
        input.Check(token);transfer.Check(token);var initial=input.ReadRegistry();
        if(initial.Length!=1||input.Next()!=input.InitialNext||input.InitialNext!=1||initial[0].Id!=input.InputId||initial[0].Category!=102||initial[0].Name!="INPUT"||!ReferenceEquals(initial[0].C22,input.InputC22)||!initial[0].Ranges.SequenceEqual(input.InputRanges))throw new NotSupportedException("Pristine actual original Math INPUT/Next required only before initial claim.");
        if(transfer.Operation!=2||transfer.Path.Length!=0||transfer.Ordinal!=1||transfer.Nodes.Count is<2 or>65536||input.HistoricalNodes.Count is<1 or>65536)throw new NotSupportedException("Actual fresh owning field-relative edit required.");
        var field=input.HistoricalNodes.Single(n=>n.Kind==3&&n.Name=="mathField"&&input.HistoricalNodes.Any(a=>a.Parent==n.Id&&a.Kind==0&&a.Name=="id"&&a.StringValue==input.FieldId));
        var inputNode=input.HistoricalNodes.Single(n=>n.Parent==field.Id&&n.Kind==3&&n.Name=="input");
        if(!input.HistoricalNodes.Any(n=>n.Parent==inputNode.Id&&n.Kind==0&&n.Name=="tag"&&n.ValueTag==4&&n.NumericValue==input.InputId)||input.HistoricalNodes.Any(n=>n.Name=="mathResult"))throw new NotSupportedException("Fresh original field/id/INPUT/tag required.");
        var payload=transfer.Nodes.Single(n=>n.Id==transfer.PayloadRoot);if(payload.Kind!=3||payload.Name!="mathResult"||payload.Parent!=0)throw new NotSupportedException("Actual owning mathResult subtree required.");
        var nodes=transfer.Nodes.ToDictionary(n=>n.Id);long valueBytes=0,rangeCount=0;
        foreach(var n in transfer.Nodes)
        {
            token.ThrowIfCancellationRequested();int id=n.Id,depth=0;while(id!=0){if(++depth>240||!nodes.TryGetValue(id,out var p))throw new NotSupportedException("Bounded outgoing payload depth required.");id=p.Parent;}
            valueBytes=checked(valueBytes+Encoding.UTF8.GetByteCount(n.Name)+Encoding.UTF8.GetByteCount(n.Text??"")+Encoding.UTF8.GetByteCount(n.NamespaceUri??"")+Encoding.UTF8.GetByteCount(n.NamespacePrefix??""));
        }
        long expected=input.InitialNext;
        foreach(var c in transfer.Commands)
        {
            token.ThrowIfCancellationRequested();if(c.Operation==7){if(c!=transfer.Commands[^1]||c.OldCounter20!=input.InitialNext||c.NewCounter18!=expected)throw new NotSupportedException("Actual final counter provenance required.");continue;}
            if(c.Operation!=4||c.Category!=102||c.Id!=expected++||c.Name is not("MATH_SYMBOL"or"MATH_NODE")||!c.DetachedC22||c.Ranges.Length==0)throw new NotSupportedException("Whole actual contiguous original Math command list required.");
            foreach(var r in c.Ranges)if(!LegacyOwnedRawInkRangeOperations.Contains(input.InputRanges,r))throw new NotSupportedException("Actual original INPUT range containment required.");rangeCount=checked(rangeCount+c.Ranges.LongLength);
        }
        if(transfer.Commands.Count<2||transfer.Commands[^1].Operation!=7)throw new NotSupportedException("Actual completed source counter required.");
        long count=checked(input.HistoricalNodes.Count*3L+transfer.Nodes.Count*2L);
        long planned=checked(transfer.Reservation+16384+count*1024+valueBytes*8+rangeCount*1024);
        if(count>65536||valueBytes>32L*1024*1024||planned>budget)throw new NotSupportedException("Whole original/source/journal/scratch/live envelope exceeds budget before copies.");return planned;
    }
    internal static void ValidateActiveRegistry(Math2Mode0OriginalInput input,Math2Mode0OriginalTransferInput transfer,Math2Mode0OriginalRegistry[] actual)
    {
        var live=input.ReadRegistry();var expected=actual.ToDictionary(r=>r.Id);var byId=live.ToDictionary(r=>r.Id);
        if(live.Length!=actual.Length||live.Any(e=>!expected.TryGetValue(e.Id,out var r)||!ReferenceEquals(r.Entry,e.Entry)||!ReferenceEquals(r.C22,e.C22)))throw new InvalidOperationException("Active original registry changed outside exact application.");
        foreach(var c in transfer.Commands.Where(c=>c.Operation==4))
        {if(!byId.TryGetValue(c.Id,out var entry)||entry.Category!=102||entry.Name!=c.Name||!entry.Ranges.SequenceEqual(c.Ranges))throw new NotSupportedException("Actual original requested-to-returned mapping must be identity.");}
        if(!byId.TryGetValue(input.InputId,out var original)||!ReferenceEquals(original.C22,input.InputC22)||!original.Ranges.SequenceEqual(input.InputRanges))throw new InvalidOperationException("Original INPUT changed.");
    }
    internal static void ValidateTags(Math2Mode0OriginalArena arena,int field,Math2Mode0OriginalRegistry[] registry,
        LegacyNativeInkPositionRange[] input,CancellationToken token)
    {
        var map=registry.ToDictionary(r=>r.Id);
        void Walk(int id)
        {
            token.ThrowIfCancellationRequested();foreach(var n in arena.Children(id))
            {
                if(n.Kind==0&&n.Name.Text=="tag")
                {if(n.Tag!=4||n.Number is not long tag||!map.TryGetValue(tag,out var row))throw new NotSupportedException("Strict library refusal of malformed or missing referenced original tag.");foreach(var r in row.Ranges)if(!LegacyOwnedRawInkRangeOperations.Contains(input,r))throw new NotSupportedException("Original tagged C22 outside INPUT.");}
                if(n.Kind==3)Walk(n.Id);
            }
        }
        Walk(field);
    }
}
