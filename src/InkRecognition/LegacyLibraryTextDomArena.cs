using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Immutable native-value lane. Strings retain literal UTF8 before
/// the first NUL; numeric tags4/5/6/7/8 retain their raw assigned bytes.</summary>
public sealed class LegacyLibraryTextDomValue
{
    private readonly byte[] bytes;
    public byte NativeTag {get;}
    public int ByteLength=>bytes.Length;
    internal LegacyLibraryTextDomValue(byte tag,ReadOnlySpan<byte> data){NativeTag=tag;bytes=data.ToArray();}
    public byte[] CopyBytes()=>bytes.ToArray();
}
public sealed class LegacyLibraryTextDomNode
{
    public LegacyLibraryTextDomArena Arena {get;}
    public int Identity {get;}
    public byte Kind {get;}
    public string Name {get;}
    public int NameIdentity {get;}
    public LegacyLibraryTextDomValue Value {get;}
    public IReadOnlyList<LegacyLibraryTextDomNode> Attributes {get;}
    public IReadOnlyList<LegacyLibraryTextDomNode> Children {get;}
    internal LegacyLibraryTextDomNode(LegacyLibraryTextDomArena arena,LegacyDomNode node)
    {Arena=arena;Identity=node.Identity;Kind=node.Kind;Name=node.Name;NameIdentity=node.NameIdentity;Value=new(node.Tag,node.Value);
     Attributes=Array.AsReadOnly(node.Attributes.Select(n=>new LegacyLibraryTextDomNode(arena,n)).ToArray());
     Children=Array.AsReadOnly(node.Children.Select(n=>new LegacyLibraryTextDomNode(arena,n)).ToArray());}
}
internal sealed class LegacyDomNode(int identity,byte kind,string name,int nameIdentity)
{
    internal readonly int Identity=identity,NameIdentity=nameIdentity;
    internal readonly byte Kind=kind;
    internal readonly string Name=name;
    internal byte Tag=kind is 0 or 1?(byte)3:(byte)0;
    internal byte[] Value=[];
    internal string? NamespacePrefix, NamespaceUri;
    internal bool Released;
    internal LegacyDomNode? Parent;
    internal readonly List<LegacyDomNode> Attributes=[],Children=[];
}
internal sealed record LegacyDomNodeState(int Identity,byte Kind,string Name,int NameIdentity,byte Tag,
    byte[] Value,int Parent,bool Released,int[] Attributes,int[] Children);

/// <summary>Retained library BEE-name-owner adaptation. Handles are local
/// creation identities, never native string addresses.</summary>
public sealed class LegacyLibraryTextDomNamePool
{
    private readonly Dictionary<string,int> names=new(StringComparer.Ordinal);
    private readonly object gate=new();
    private readonly LegacyFreshMainTextDocumentSeed? seed;
    public LegacyFreshMainTextDocumentSeed Seed=>seed??throw new InvalidOperationException("This pre-publication INPUT name owner has no result Seed.");
    public LegacyOwnedRawInkDocument Document {get;}
    internal LegacyLibraryTextDomNamePool(LegacyFreshMainTextDocumentSeed seed){this.seed=seed;Document=seed.Document;}
    internal LegacyLibraryTextDomNamePool(LegacyFreshTextFieldInputContext input){Document=input.Document;}
    internal LegacyLibraryTextDomNamePool(LegacyOwnedRawInkDocument document){Document=document;}
    internal (int Identity,bool Added) Intern(string name)
    {lock(gate){if(names.TryGetValue(name,out int id))return(id,false);id=names.Count+1;names.Add(name,id);return(id,true);}}
    public IReadOnlyList<string> CopyNames(){lock(gate)return Array.AsReadOnly(names.OrderBy(n=>n.Value).Select(n=>n.Key).ToArray());}
}

/// <summary>Explicit library-owned parameterized BCC no-history arena.
/// This is not an SDK existing page. Managed storage owns all allocations,
/// including native-unattached failed nodes until the arena is collected.</summary>
public sealed class LegacyLibraryTextDomArena
{
    private readonly LegacyFreshMainTextDocumentSeed? seed;
    public LegacyFreshMainTextDocumentSeed Seed=>seed??throw new InvalidOperationException("This pre-publication INPUT arena has no result Seed.");
    public LegacyOwnedRawInkDocument Document {get;}
    public bool IsParameterizedNoHistory=>true;
    public LegacyLibraryTextDomNamePool NamePool {get;}
    private readonly List<LegacyDomNode> nodes=[];
    private readonly List<string> trace=[];
    private readonly bool traceEnabled=true;
    private int failAt,cancelAt;
    private int attempts;
    internal LegacyLibraryTextDomArena(LegacyFreshMainTextDocumentSeed seed,int failAt=-1,int cancelAt=-1)
    {this.seed=seed;Document=seed.Document;NamePool=new(seed);this.failAt=failAt;this.cancelAt=cancelAt;}
    internal LegacyLibraryTextDomArena(LegacyFreshMainTextDocumentSeed seed,LegacyLibraryTextDomNamePool sharedNames,int failAt=-1,int cancelAt=-1)
    {if(!ReferenceEquals(seed,sharedNames.Seed))throw new ArgumentException("Foreign BEE name owner.");this.seed=seed;Document=seed.Document;NamePool=sharedNames;this.failAt=failAt;this.cancelAt=cancelAt;}
    internal LegacyLibraryTextDomArena(LegacyFreshTextFieldInputContext input,int failAt=-1,int cancelAt=-1)
    {Document=input.Document;NamePool=new(input);this.failAt=failAt;this.cancelAt=cancelAt;}
    internal LegacyLibraryTextDomArena(LegacyOwnedRawInkDocument document,int failAt=-1,int cancelAt=-1)
    {Document=document;NamePool=new(document);this.failAt=failAt;this.cancelAt=cancelAt;}
    //The fresh English owner supplies its authenticated actual request, never
    //a new empty parent, a source registry ID or a replacement name pool.
    internal LegacyLibraryTextDomArena(LegacyLibraryTextInputRequest actualRequest)
    {Document=actualRequest.Owner.Document;NamePool=actualRequest.Arena.NamePool;failAt=cancelAt=-1;traceEnabled=false;}
    internal IReadOnlyList<(int Identity,string Prefix,string Uri)> CopyNamespacePairs() =>
        nodes.Where(n=>n.Kind==2).Select(n=>(n.Identity,n.NamespacePrefix!,n.NamespaceUri!)).ToArray();
    internal (LegacyDomNode Root,LegacyDomNode Field,LegacyDomNode Input) ImportSelectedPage(
        LegacyLibraryOriginalSelectedFieldContext source,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(Document,source.Document)||!source.State.Succeeded||source.State.IsFaulted||nodes.Count!=0)
            throw new ArgumentException("Exact completed selected PAGE and empty same-document reader facade required.");
        var map=new Dictionary<int,LegacyDomNode>();var snapshot=source.State;
        foreach(var n in snapshot.Nodes)
        {
            token.ThrowIfCancellationRequested();
            if(n.Kind is not(0 or 2 or 3 or 4))throw new NotSupportedException("Ordinary selected PAGE node kinds required.");
            var copy=Allocate(n.Kind,n.Name)??throw new InvalidOperationException("Selected reader-facade allocation refusal.");
            map.Add(n.Identity,copy);
            if(n.Kind==2)
            {
                copy.NamespacePrefix=n.NamespacePrefixName?.Text??throw new InvalidDataException("Actual namespace prefix missing.");
                copy.NamespaceUri=n.NamespaceUri??throw new InvalidDataException("Actual namespace URI missing.");
                NamePool.Intern(copy.NamespacePrefix);NamePool.Intern(copy.NamespaceUri);
            }
            else if(n.Kind==0)
            {
                if(n.NativeValueTag is 1 or 2 or 3)
                {if(!SetString(copy,n.CopyValueBytes()))throw new InvalidOperationException("Selected attribute-facade copy refusal.");}
                else if(n.NativeValueTag is 4 or 5 or 6 or 7 or 8)
                {copy.Tag=n.NativeValueTag;copy.Value=n.CopyValueBytes();if(traceEnabled)trace.Add("value:"+copy.Identity+":"+copy.Tag+":"+Convert.ToBase64String(copy.Value));}
                else throw new NotSupportedException("Exact selected attribute lane required.");
            }
        }
        foreach(var n in snapshot.Nodes)
        {
            var parent=map[n.Identity];
            foreach(int id in n.Attributes)
            {var a=map[id];if(parent.Kind!=3||a.Kind!=0||a.Parent is not null)throw new InvalidDataException("Selected attribute ownership lost.");a.Parent=parent;parent.Attributes.Add(a);if(traceEnabled)trace.Add("attr:"+parent.Identity+":"+a.Identity);}
            foreach(int id in n.Children)Append(parent,map[id]);
        }
        return(map[snapshot.RootIdentity],map[snapshot.SelectedFieldIdentity],map[snapshot.SelectedInputIdentity]);
    }
    internal void BeginFaultScope(int fail=-1,int cancel=-1)
    {if(fail< -1||cancel< -1)throw new ArgumentOutOfRangeException(nameof(fail));failAt=fail<0?-1:checked(attempts+fail);cancelAt=cancel<0?-1:checked(attempts+cancel);}
    internal IReadOnlyList<string> Trace=>trace.AsReadOnly();
    internal int Attempts=>attempts;
    internal bool Allow(string operation)
    {
        int ordinal=attempts++;if(traceEnabled)trace.Add("try:"+operation);
        if(ordinal==cancelAt)throw new OperationCanceledException("Prepared native-service cancellation prefix.");
        if(ordinal==failAt){if(traceEnabled)trace.Add("fail:"+operation);return false;}return true;
    }
    private LegacyDomNode? Allocate(byte kind,string name="",int resolvedName=-1)
    {
        int named=resolvedName;
        if(named<0){named=0;if(name.Length!=0){var result=NamePool.Intern(name);named=result.Identity;if(result.Added)if(traceEnabled)trace.Add("name:"+named+":"+name);}}
        if(!Allow("node"+kind))return null;
        var node=new LegacyDomNode(nodes.Count+1,kind,name,named);nodes.Add(node);if(traceEnabled)trace.Add("new:"+node.Identity+":"+kind+":"+name);return node;
    }
    internal (int Nodes,int NodeCapacity,int Attributes,int AttributeCapacity) ReadEnglishLayout(LegacyDomNode? element)
        =>(nodes.Count,nodes.Capacity,element?.Attributes.Count??0,element?.Attributes.Capacity??0);
    internal LegacyDomNode? EnglishElement(LegacyEnglishCaseNode0Completion.GraphWordPublicationContext parent,int slot,string expected,CancellationToken token)
    {var name=parent.PrepareFreshDomNode(this,slot,expected,null,0,token);return Allocate(3,name.Name,name.Identity);}
    internal LegacyDomNode? EnglishText(LegacyEnglishCaseNode0Completion.GraphWordPublicationContext parent,byte[] raw,CancellationToken token)
    {
        parent.PrepareFreshDomNode(this,-1,"",null,raw.Length,token);var node=Allocate(1);
        if(node is null)return null;
        if(!SetString(node,raw,parent,token)){node.Released=true;return null;}return node;
    }
    internal bool EnglishStringAttribute(LegacyEnglishCaseNode0Completion.GraphWordPublicationContext parent,LegacyDomNode node,
        int slot,string expected,byte[] value,CancellationToken token)
    {
        var name=parent.PrepareFreshDomNode(this,slot,expected,node,value.Length,token);ValidateElement(node);
        var attribute=node.Attributes.FirstOrDefault(a=>a.NameIdentity==name.Identity);bool created=attribute is null;
        attribute??=Allocate(0,name.Name,name.Identity);if(attribute is null)return false;
        if(!SetString(attribute,value,parent,token)){if(created)Release(attribute);return false;}
        if(created){attribute.Parent=node;node.Attributes.Add(attribute);}return true;
    }
    internal bool EnglishNumericAttribute(LegacyEnglishCaseNode0Completion.GraphWordPublicationContext parent,LegacyDomNode node,
        int slot,string expected,byte tag,long raw,CancellationToken token)
    {
        int width=tag is 4 or 5?8:tag is 7 or 8?4:throw new ArgumentOutOfRangeException(nameof(tag));
        var name=parent.PrepareFreshDomNode(this,slot,expected,node,width,token);ValidateElement(node);
        var attribute=node.Attributes.FirstOrDefault(a=>a.NameIdentity==name.Identity);bool created=attribute is null;
        attribute??=Allocate(0,name.Name,name.Identity);if(attribute is null)return false;
        var value=new byte[width];if(width==8)BinaryPrimitives.WriteInt64LittleEndian(value,raw);else BinaryPrimitives.WriteInt32LittleEndian(value,unchecked((int)raw));
        parent.RequireFreshDomPublication(this,token);attribute.Tag=tag;attribute.Value=value;
        if(created){attribute.Parent=node;node.Attributes.Add(attribute);}return true;
    }
    internal void EnglishAppend(LegacyEnglishCaseNode0Completion.GraphWordPublicationContext owner,LegacyDomNode parent,LegacyDomNode child,CancellationToken token)
    {
        owner.PrepareFreshDomAppend(this,parent,token);Validate(parent);Validate(child);
        if(parent.Kind is not(3 or 4)||child.Kind==0||child.Parent is not null||ReferenceEquals(parent,child))throw new ArgumentException("Actual unattached English DOM child required.");
        int depth=0;for(var cursor=parent.Parent;cursor is not null;cursor=cursor.Parent)
        {token.ThrowIfCancellationRequested();if(++depth>8||ReferenceEquals(cursor,child))throw new InvalidDataException("Bounded acyclic fresh DOM parent chain required.");}
        child.Parent=parent;parent.Children.Add(child);
    }
    internal LegacyDomNode? Element(string name)=>Allocate(3,name);
    internal LegacyDomNode? TryConstructorRoot()=>Allocate(4);
    internal LegacyDomNode ConstructorRoot()=>TryConstructorRoot()??throw new InvalidOperationException("Fresh BCC root allocation failed.");
    internal LegacyDomNode? Text(ReadOnlySpan<byte> raw)
    {
        var node=Allocate(1);if(node is null)return null;
        if(!SetString(node,raw)){node.Released=true;if(traceEnabled)trace.Add("release-bare:"+node.Identity);return null;}return node;
    }
    internal static byte[] Literal(ReadOnlySpan<byte> raw)
    {int zero=raw.IndexOf((byte)0);return(zero<0?raw:raw[..zero]).ToArray();}
    private bool SetString(LegacyDomNode node,ReadOnlySpan<byte> raw,LegacyEnglishCaseNode0Completion.GraphWordPublicationContext? english=null,CancellationToken token=default)
    {
        Validate(node);byte[] value=Literal(raw);
        if(value.Length>=16&&!Allow("value-bytes"))return false;
#if ENGLISH_FRESH_CONTINUATION_CONTROLS
        if(english is not null)LegacyFreshContinuationTestHooks.Checkpoint?.Invoke("after-dom-arena-string-copy");
#endif
        english?.RequireFreshDomPublication(this,token);
        node.Tag=value.Length==0?(byte)3:value.Length<16?(byte)2:(byte)1;node.Value=value;
        if(traceEnabled)trace.Add("value:"+node.Identity+":"+node.Tag+":"+Convert.ToBase64String(value));return true;
    }
    internal bool StringAttribute(LegacyDomNode node,string name,ReadOnlySpan<byte> value)
    {
        ValidateElement(node);var attribute=node.Attributes.FirstOrDefault(a=>a.Name==name);bool created=attribute is null;
        attribute??=Allocate(0,name);if(attribute is null)return false;
        if(!SetString(attribute,value)){if(created)Release(attribute);return false;}
        if(created){attribute.Parent=node;node.Attributes.Add(attribute);if(traceEnabled)trace.Add("attr:"+node.Identity+":"+attribute.Identity);}return true;
    }
    internal bool NumericAttribute(LegacyDomNode node,string name,byte tag,ReadOnlySpan<byte> raw)
    {
        ValidateElement(node);int width=tag switch{4 or 5=>8,6=>1,7 or 8=>4,_=>throw new ArgumentOutOfRangeException(nameof(tag))};
        if(raw.Length!=width)throw new ArgumentException("Exact native numeric lane required.");
        var attribute=node.Attributes.FirstOrDefault(a=>a.Name==name);bool created=attribute is null;
        attribute??=Allocate(0,name);if(attribute is null)return false;
        attribute.Tag=tag;attribute.Value=raw.ToArray();if(traceEnabled)trace.Add("value:"+attribute.Identity+":"+tag+":"+Convert.ToBase64String(raw));
        if(created){attribute.Parent=node;node.Attributes.Add(attribute);if(traceEnabled)trace.Add("attr:"+node.Identity+":"+attribute.Identity);}return true;
    }
    internal bool I32(LegacyDomNode n,string name,int value){byte[] b=new byte[4];BinaryPrimitives.WriteInt32LittleEndian(b,value);return NumericAttribute(n,name,7,b);}
    internal bool U32(LegacyDomNode n,string name,uint value){byte[] b=new byte[4];BinaryPrimitives.WriteUInt32LittleEndian(b,value);return NumericAttribute(n,name,8,b);}
    internal bool I64(LegacyDomNode n,string name,long value){byte[] b=new byte[8];BinaryPrimitives.WriteInt64LittleEndian(b,value);return NumericAttribute(n,name,4,b);}
    internal bool F64(LegacyDomNode n,string name,double value){byte[] b=new byte[8];BinaryPrimitives.WriteDoubleLittleEndian(b,value);return NumericAttribute(n,name,5,b);}
    internal bool Byte(LegacyDomNode n,string name,byte value)=>NumericAttribute(n,name,6,[value]);
    internal void Append(LegacyDomNode parent,LegacyDomNode child)
    {
        Validate(parent);if(parent.Kind is not(3 or 4))throw new ArgumentException("A native container required.");Validate(child);if(child.Kind==0||child.Parent is not null||ReferenceEquals(parent,child))throw new ArgumentException("An unattached same-arena child is required.");
        for(var current=parent.Parent;current is not null;current=current.Parent)if(ReferenceEquals(current,child))throw new ArgumentException("Cyclic DOM append.");
        child.Parent=parent;parent.Children.Add(child);if(traceEnabled)trace.Add("append:"+parent.Identity+":"+child.Identity);
    }
    internal void DetachAndRelease(LegacyDomNode parent,LegacyDomNode child)
    {Validate(parent);Validate(child);if(!ReferenceEquals(child.Parent,parent)||!parent.Children.Remove(child))throw new ArgumentException("Exact attached target child required.");child.Parent=null;if(traceEnabled)trace.Add("remove:"+parent.Identity+":"+child.Identity);Release(child);}
    internal LegacyDomNode? CloneFrom(LegacyLibraryTextDomArena sourceArena,LegacyDomNode source,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();sourceArena.Validate(source);
        if(source.Kind==1)return Text(source.Value);
        if(source.Kind!=3)throw new NotSupportedException("Recursive ordinary element/text payload required.");
        var copy=Element(source.Name);if(copy is null)return null;
        foreach(var attribute in source.Attributes)
        {
            token.ThrowIfCancellationRequested();var a=Allocate(0,attribute.Name);if(a is null){Release(copy);return null;}
            bool ok=true;
            if(attribute.Tag is 1 or 2 or 3)ok=SetString(a,attribute.Value);
            else if(attribute.Tag is 4 or 5 or 6 or 7 or 8){a.Tag=attribute.Tag;a.Value=attribute.Value.ToArray();if(traceEnabled)trace.Add("value:"+a.Identity+":"+a.Tag+":"+Convert.ToBase64String(a.Value));}
            else throw new NotSupportedException("Unsupported native attribute lane.");
            if(!ok){a.Released=true;if(traceEnabled)trace.Add("release-bare:"+a.Identity);Release(copy);return null;}
            a.Parent=copy;copy.Attributes.Add(a);if(traceEnabled)trace.Add("attr:"+copy.Identity+":"+a.Identity);
        }
        foreach(var child in source.Children)
        {var cloned=CloneFrom(sourceArena,child,token);if(cloned is null){Release(copy);return null;}Append(copy,cloned);}
        return copy;
    }
    internal void ClearChildren(LegacyDomNode node)
    {
        Validate(node);if(traceEnabled)trace.Add("clear:"+node.Identity);
        foreach(var child in node.Children.ToArray()){child.Parent=null;Release(child);}node.Children.Clear();
    }
    internal void Release(LegacyDomNode node)
    {
        Validate(node);
        foreach(var attribute in node.Attributes.ToArray()){attribute.Parent=null;Release(attribute);}node.Attributes.Clear();
        ClearChildren(node);node.Released=true;if(traceEnabled)trace.Add("release:"+node.Identity);
    }
    private void Validate(LegacyDomNode node)
    {if(node.Released||node.Identity<1||node.Identity>nodes.Count||!ReferenceEquals(nodes[node.Identity-1],node))throw new ArgumentException("A live node from this exact arena required.");}
    private void ValidateElement(LegacyDomNode node){Validate(node);if(node.Kind!=3)throw new ArgumentException("An element node required.");}
    internal void Record(string value){if(traceEnabled)trace.Add(value);}
    internal LegacyLibraryTextDomNode Snapshot(LegacyDomNode node){Validate(node);return new(this,node);}
    internal LegacyDomNodeState[] CopyStates()=>nodes.Select(n=>new LegacyDomNodeState(n.Identity,n.Kind,n.Name,n.NameIdentity,n.Tag,
        n.Value.ToArray(),n.Parent?.Identity??0,n.Released,n.Attributes.Select(a=>a.Identity).ToArray(),n.Children.Select(c=>c.Identity).ToArray())).ToArray();
}
