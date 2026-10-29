using System.Buffers.Binary;
using System.Text;
namespace InkRecognition.Math;

internal sealed partial class Math2DomDiffScalar
{
    internal (string? Text,long? Number) CopyWriterFields(Math2DomDiffWork work)
    {
        work.Visit(checked(32L+storage.LongLength+heap.LongLength*3L));
        return Tag switch
        {
            1=>(new UTF8Encoding(false,true).GetString(heap),null),
            2=>(new UTF8Encoding(false,true).GetString(storage,0,storage[15]==0?15:storage[15]),null),
            3=>(string.Empty,null),
            4=>(null,BinaryPrimitives.ReadInt64LittleEndian(storage)),
            7=>(null,BinaryPrimitives.ReadInt32LittleEndian(storage)),
            10=>(null,0),
            _=>throw new NotSupportedException("Only proved produced scalar tags can enter the temporary writer.")
        };
    }
}

internal sealed partial class Math2Mode0OriginalArena
{
    private readonly HashSet<Node> writerFreed=[];
    internal bool WriterFreed(Node node)=>writerFreed.Contains(node);
    private Node WriterNode(Math2DomDiffNode view,Math2DomDiffWork work)
    {
        work.Visit();
        if(!ReferenceEquals(view.Writer,this)||view.Identity is not Node node||node.Id<1||node.Id>Nodes.Count||
            !ReferenceEquals(Nodes[node.Id-1],node)||!ReferenceEquals(view.Name,node.Name)||writerFreed.Contains(node))
            throw new InvalidOperationException("Same live temporary arena node required; a diagnostic ID is not an input.");
        return node;
    }
    private List<Node> WriterChildren(Node parent)
    {if(!children.TryGetValue(parent.Id,out var list))children[parent.Id]=list=[];return list;}
    private void WriterParent(Node parent,Node value,bool attribute,Math2DomDiffWork work)
    {
        if(parent.Kind is not(3 or 4)||(attribute?value.Kind!=0:value.Kind is<1 or>3))
            throw new NotSupportedException("Proved composite parent and typed payload required.");
        int depth=0;for(var at=parent;;)
        {
            work.Visit();if(++depth>64)throw new NotSupportedException("Bounded current parent chain required.");if(ReferenceEquals(at,value))throw new InvalidOperationException("Cyclic temporary writer insertion.");
            if(at.Parent==0)break;
            if(at.Parent<1||at.Parent>Nodes.Count)throw new InvalidOperationException("Invalid retained parent chain.");
            at=Nodes[at.Parent-1];
        }
    }
    private void WriterDetach(Node node,Math2DomDiffWork work)
    {
        work.Visit();if(node.Parent==0)return;
        var old=Nodes[node.Parent-1];var list=WriterChildren(old);work.Visit(checked(list.Count*2L));
        if(!list.Remove(node))throw new InvalidOperationException("Actual child/attribute parent relation lost.");
        node.Parent=0;
    }
    // Pure typed-arena operation. Only a separate live owning borrow may pass a
    // produced action here. Prepared probes can test this kernel without gaining
    // any current-C23, application, revision or publication authority.
    internal bool ApplyTemporaryAction(Math2DomDiffAction action,Math2DomDiffWork work)
    {
        work.Visit();
        if(action.Operation is 1 or 3)throw new NotSupportedException("These operations are outside the produced default Math diff.");
        if(action.Operation is not(2 or 4 or 5 or 6 or 7))return false;
        var target=WriterNode(action.NodeAt18,work);
        switch(action.Operation)
        {
            case 2:
            {
                var payload=WriterNode(action.NodeAt20??throw new InvalidOperationException(),work);
                WriterParent(target,payload,false,work);var list=WriterChildren(target);Node? before=null;int ordinal=0;
                // E4B90 chooses the current child before invoking the writer.
                // An absent (including negative) ordinal takes append, not reject.
                foreach(var child in list){work.Visit();if(child.Kind==0)continue;if(ordinal++==action.Ordinal){before=child;break;}}
                if(ReferenceEquals(before,payload))throw new NotSupportedException("Native insert-before-self is not a valid produced payload.");
                WriterDetach(payload,work);work.Visit(list.Count);
                int index=before is null?list.Count:list.IndexOf(before);
                if(index<0)throw new InvalidOperationException("Insertion anchor disappeared.");
                work.Visit(list.Count-index+1L);
                list.Insert(index,payload);payload.Parent=target.Id;return true;
            }
            case 4:
            {
                // The payload identity at +20 is NOT the deletion selector.
                // The native dispatcher selects from the NOW-current ordinal.
                if(target.Kind is not(3 or 4))return true;
                int ordinal=0;Node? remove=null;
                foreach(var child in WriterChildren(target)){work.Visit();if(child.Kind==0)continue;if(ordinal++==action.Ordinal){remove=child;break;}}
                if(remove is not null)WriterDetach(remove,work);return true;
            }
            case 5:
            {
                var attribute=WriterNode(action.NodeAt20??throw new InvalidOperationException(),work);
                WriterParent(target,attribute,true,work);
                // EB3B0 returns before entering mutation when already attached.
                if(attribute.Parent==target.Id)return true;
                var list=WriterChildren(target);var unique=new HashSet<LegacyLibraryOriginalDomName>();
                foreach(var old in list){work.Visit();if(old.Kind==0&&!unique.Add(old.Name))throw new NotSupportedException("Unique canonical native attribute chain required.");}
                WriterDetach(attribute,work);
                for(int i=0;i<list.Count;)
                {
                    work.Visit();var old=list[i];
                    if(old.Kind==0&&ReferenceEquals(old.Name,attribute.Name))
                    {work.Visit(list.Count-i);list.RemoveAt(i);old.Parent=0;writerFreed.Add(old);}else i++;
                }
                // Attributes are a separate native chain. Appending after its
                // last member preserves its order in the combined managed list.
                int end=0;for(int i=0;i<list.Count;i++){work.Visit();if(list[i].Kind==0)end=i+1;}
                work.Visit(list.Count+1L);list.Insert(end,attribute);attribute.Parent=target.Id;return true;
            }
            case 6:
            {
                var attribute=WriterNode(action.NodeAt20??throw new InvalidOperationException(),work);
                var list=WriterChildren(target);work.Visit(list.Count);
                // EB5A0 removes by exact identity; a missing attr is a no-op.
                int at=list.FindIndex(n=>n.Kind==0&&ReferenceEquals(n,attribute));
                if(at>=0){work.Visit(list.Count);list.RemoveAt(at);attribute.Parent=0;}return true;
            }
            case 7:
            {
                if(target.Kind is not(0 or 1))throw new NotSupportedException("Actual attribute/text scalar target required.");
                var value=action.NewValue??throw new InvalidOperationException("Actual produced new scalar required.");
                var fields=value.CopyWriterFields(work); // Copy/validate BEFORE replacing the old payload.
                work.Visit();target.Tag=value.Tag;target.Text=fields.Text;target.Number=fields.Number;return true;
            }
        }
        throw new InvalidOperationException();
    }
}
