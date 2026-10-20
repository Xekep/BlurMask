using System.Buffers.Binary;
using System.Text;

namespace InkRecognition;

/// <summary>Bounded library graph over the retained original BEE-name owner.
/// Local identities and origins are typed sidecars, never native pointers.</summary>
internal sealed class LegacyOriginalFieldDomKernel
{
    internal sealed class Node(int id,int origin,byte kind,LegacyLibraryOriginalDomName? name)
    {
        internal readonly int Id=id,OriginalIdentity=origin;
        internal readonly byte Kind=kind;
        internal readonly LegacyLibraryOriginalDomName? Name=name;
        internal byte Tag;internal byte[] Value=[];
        internal LegacyLibraryOriginalDomName? Prefix,Uri;
        internal Node? Parent;
        internal readonly List<Node> Attributes=[],Children=[];
    }
    internal readonly LegacyLibraryOriginalDomNameOwner Names;
    internal readonly List<Node> Nodes=[];
    internal readonly List<string> Trace=[];
    internal int Attempts;
    private int failure=-1,cancellation=-1;
    internal readonly Node Root,Field;
    internal const int MaxNodes=65536,MaxDepth=256,MaxValueBytes=32*1024*1024;

    internal LegacyOriginalFieldDomKernel(LegacyLibraryOriginalTextFieldBootstrapState original,
        LegacyLibraryOriginalFieldSelection input,CancellationToken token)
    {
        Names=original.Owner.NameOwner;
        if(!original.Succeeded||original.IsFaulted||!ReferenceEquals(input.Owner,original.Owner)||
            !original.Inputs.Any(e=>ReferenceEquals(e,input))||original.Nodes.Count is <1 or >MaxNodes)
            throw new ArgumentException("Complete exact original PAGE/field/INPUT graph required.");
        var source=original.Nodes.ToDictionary(n=>n.Identity);var seen=new HashSet<int>();long bytes=0;
        void Check(int id,int parent,int depth)
        {
            token.ThrowIfCancellationRequested();
            if(depth>MaxDepth||!source.TryGetValue(id,out var n)||!seen.Add(id)||n.ParentIdentity!=parent||
                n.Kind is not(0 or 2 or 3 or 4)||!ReferenceEquals(n.NameOwner,Names)||
                (n.CanonicalName is not null&&!ReferenceEquals(n.CanonicalName.Owner,Names)))
                throw new NotSupportedException("Bounded acyclic original graph and exact original name owner required.");
            bytes=checked(bytes+n.CopyValueBytes().Length+Encoding.UTF8.GetByteCount(n.Name));
            if(bytes>MaxValueBytes)throw new NotSupportedException("Original PAGE byte budget exceeded.");
            foreach(int a in n.Attributes){if(source[a].Kind is not(0 or 2))throw new NotSupportedException("Original attribute lane required.");Check(a,id,depth+1);}
            foreach(int c in n.Children){if(source[c].Kind==0)throw new NotSupportedException("Original child lane required.");Check(c,id,depth+1);}
        }
        Check(original.RootIdentity,0,0);
        if(seen.Count!=source.Count)throw new NotSupportedException("Unattached original nodes unsupported.");
        Node Copy(int id)
        {
            var n=source[id];var v=new Node(Nodes.Count+1,n.Identity,n.Kind,n.CanonicalName)
                {Tag=n.NativeValueTag,Value=n.CopyValueBytes(),Prefix=n.NamespacePrefixName,Uri=n.NamespaceUriName};Nodes.Add(v);
            foreach(int a in n.Attributes){var c=Copy(a);c.Parent=v;v.Attributes.Add(c);}
            foreach(int a in n.Children){var c=Copy(a);c.Parent=v;v.Children.Add(c);}return v;
        }
        Root=Copy(original.RootIdentity);Field=Nodes.Single(n=>n.OriginalIdentity==input.FieldNodeIdentity);
        var page=Root.Children.Single();
        if(Root.Kind!=4||page.Kind!=3||page.Name?.Text!="page"||page.Children.Count!=3||
            page.Children[0].Kind!=2||page.Children[0].Prefix?.Text!=""||page.Children[0].Uri?.Text!=LegacyLibraryOriginalTextFieldBootstrap.PageNamespace||
            page.Children[1].Name?.Text!="content"||page.Children[2].Name?.Text!="layout"||
            Field.Kind!=3||Field.Name?.Text!="textField"||!ReferenceEquals(Field.Parent,page.Children[1])||
            Field.Children.Count!=1||Field.Children[0].OriginalIdentity!=input.InputNodeIdentity||
            Field.Children[0].Name?.Text!="input"||ReadId(Field)!=input.FieldId||ReadTag(Field.Children[0])!=input.RegistryId)
            throw new NotSupportedException("Fresh direct original textField with its sole actual INPUT required.");
        foreach(var entry in original.Inputs)
        {
            var f=Nodes.Single(n=>n.OriginalIdentity==entry.FieldNodeIdentity);
            if(!ReferenceEquals(f.Parent,page.Children[1])||f.Kind!=3||f.Name?.Text!="textField"||
                ReadId(f)!=entry.FieldId||f.Children.Count!=1||f.Children[0].OriginalIdentity!=entry.InputNodeIdentity||
                ReadTag(f.Children[0])!=entry.RegistryId)throw new NotSupportedException("Complete fresh sibling fields/INPUTs required.");
        }
        if(page.Children[1].Children.Count!=original.Inputs.Count)throw new NotSupportedException("Unexpected original content child.");
    }
    internal static long ReadTag(Node node)
    {
        var a=node.Attributes.SingleOrDefault(n=>n.Name?.Text=="tag");
        if(a is null)return -1;
        if(a.Kind!=0||a.Tag!=4||a.Value.Length!=8)throw new NotSupportedException("Actual signed64 tag value-kind4 required.");
        long result=BinaryPrimitives.ReadInt64LittleEndian(a.Value);
        if(result<0)throw new NotSupportedException("Original FIELD route requires nonnegative actual registry tags.");return result;
    }
    private static string ReadId(Node n)
    {
        var a=n.Attributes.Single(x=>x.Name?.Text=="id");
        if(a.Tag is not(1 or 2 or 3))throw new NotSupportedException("Original literal field id required.");
        return Encoding.UTF8.GetString(a.Value);
    }
    internal static (int Nodes,long Bytes,int Depth) MeasurePayload(LegacyLibraryTextDomNode root,CancellationToken token)
    {
        int count=0,maxDepth=0;long bytes=0;var seen=new HashSet<int>();var arena=root.Arena;
        void Visit(LegacyLibraryTextDomNode n,int depth)
        {
            token.ThrowIfCancellationRequested();
            if(++count>MaxNodes||depth>MaxDepth||!ReferenceEquals(n.Arena,arena)||!seen.Add(n.Identity)||n.Kind is not(0 or 1 or 3))
                throw new NotSupportedException("Bounded exact-arena ordinary result payload required.");
            maxDepth=Math.Max(maxDepth,depth);bytes=checked(bytes+n.Value.ByteLength+Encoding.UTF8.GetByteCount(n.Name));
            if(bytes>MaxValueBytes)throw new NotSupportedException("Result payload byte budget exceeded.");
            if(n.Attributes.Any(a=>a.Kind!=0))throw new NotSupportedException("Ordinary result attribute kind required.");
            foreach(var a in n.Attributes)Visit(a,depth+1);foreach(var c in n.Children)Visit(c,depth+1);
        }
        Visit(root,0);if(root.Kind!=3||root.Name!="textResult")throw new NotSupportedException("Actual result writer root required.");
        return(count,bytes,maxDepth);
    }
    internal void Begin(int fail,int cancel)
    {if(fail< -1||cancel< -1)throw new ArgumentOutOfRangeException(nameof(fail));Attempts=0;failure=fail;cancellation=cancel;Trace.Clear();}
    internal void Step(string label,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();int i=Attempts++;Trace.Add("try:"+label);
        if(i==cancellation)throw new OperationCanceledException("Prepared FIELD DOM service cancellation.");
        if(i==failure)throw new DomServiceRefusal();
    }
    internal sealed class DomServiceRefusal:Exception;
    internal Node CloneField(CancellationToken token)
    {
        Node Copy(Node n)
        {
            Step("scratch-node",token);var v=new Node(Nodes.Count+1,n.OriginalIdentity,n.Kind,n.Name)
                {Tag=n.Tag,Value=n.Value.ToArray(),Prefix=n.Prefix,Uri=n.Uri};Nodes.Add(v);
            foreach(var a in n.Attributes){var c=Copy(a);c.Parent=v;v.Attributes.Add(c);}
            foreach(var a in n.Children){var c=Copy(a);c.Parent=v;v.Children.Add(c);}return v;
        }
        return Copy(Field);
    }
    internal Node ClonePayload(LegacyLibraryTextDomNode n,CancellationToken token)
    {
        Step("payload-node",token);
        var v=new Node(Nodes.Count+1,0,n.Kind,n.Name.Length==0?null:Names.Intern(n.Name))
            {Tag=n.Value.NativeTag,Value=n.Value.CopyBytes()};Nodes.Add(v);
        foreach(var a in n.Attributes){var c=ClonePayload(a,token);c.Parent=v;v.Attributes.Add(c);}
        foreach(var a in n.Children){var c=ClonePayload(a,token);c.Parent=v;v.Children.Add(c);}return v;
    }
    internal void Append(Node parent,Node node,int ordinal,CancellationToken token)
    {
        Step("append",token);
        if(ordinal!=parent.Children.Count||node.Parent is not null)throw new InvalidOperationException("Derived fresh-field append ordinal required.");
        node.Parent=parent;parent.Children.Add(node);Trace.Add("append:"+parent.Id+":"+node.Id+":"+ordinal);
    }
    internal LegacyLibraryOriginalFieldDomNode Snapshot(Node root)=>new(root,Names);
}

/// <summary>Immutable original-domain graph snapshot. Identity is library-local;
/// OriginalIdentity is retained provenance, never an invented native address.</summary>
public sealed class LegacyLibraryOriginalFieldDomNode
{
    private readonly byte[] value;
    public int Identity {get;}
    public int OriginalIdentity {get;}
    public byte Kind {get;}
    public LegacyLibraryOriginalDomNameOwner NameOwner {get;}
    public LegacyLibraryOriginalDomName? CanonicalName {get;}
    public string Name=>CanonicalName?.Text??"";
    public byte NativeValueTag {get;}
    public LegacyLibraryOriginalDomName? NamespacePrefixName {get;}
    public LegacyLibraryOriginalDomName? NamespaceUriName {get;}
    public IReadOnlyList<LegacyLibraryOriginalFieldDomNode> Attributes {get;}
    public IReadOnlyList<LegacyLibraryOriginalFieldDomNode> Children {get;}
    internal LegacyLibraryOriginalFieldDomNode(LegacyOriginalFieldDomKernel.Node n,LegacyLibraryOriginalDomNameOwner names)
    {Identity=n.Id;OriginalIdentity=n.OriginalIdentity;Kind=n.Kind;NameOwner=names;CanonicalName=n.Name;NativeValueTag=n.Tag;value=n.Value.ToArray();
     NamespacePrefixName=n.Prefix;NamespaceUriName=n.Uri;Attributes=Array.AsReadOnly(n.Attributes.Select(a=>new LegacyLibraryOriginalFieldDomNode(a,names)).ToArray());
     Children=Array.AsReadOnly(n.Children.Select(a=>new LegacyLibraryOriginalFieldDomNode(a,names)).ToArray());}
    public byte[] CopyValueBytes()=>value.ToArray();
}
