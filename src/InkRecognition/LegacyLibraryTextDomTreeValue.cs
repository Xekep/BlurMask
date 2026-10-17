namespace InkRecognition;

/// <summary>Owner-free immutable DOM content. Contains no Arena, Seed,
/// recognition context or scope reference. Names are immutable strings.
/// Copying this value does not keep the recognition graph alive.</summary>
public sealed class LegacyLibraryTextDomTreeValue
{
    public const int MaximumNodes=65536, MaximumValueBytes=4*1024*1024, MaximumDepth=128;
    private readonly byte[] value;
    public int Identity {get;}
    public byte Kind {get;}
    public string Name {get;}
    public int NameIdentity {get;}
    public byte NativeTag {get;}
    public IReadOnlyList<LegacyLibraryTextDomTreeValue> Attributes {get;}
    public IReadOnlyList<LegacyLibraryTextDomTreeValue> Children {get;}
    public byte[] CopyValueBytes()=>value.ToArray();
    private LegacyLibraryTextDomTreeValue(LegacyLibraryTextDomNode source,
        LegacyLibraryTextDomTreeValue[] attributes,LegacyLibraryTextDomTreeValue[] children)
    {
        Identity=source.Identity;Kind=source.Kind;Name=source.Name;
        NameIdentity=source.NameIdentity;NativeTag=source.Value.NativeTag;
        value=source.Value.CopyBytes();Attributes=Array.AsReadOnly(attributes);Children=Array.AsReadOnly(children);
    }
    /// <summary>Validate the complete immutable source before copying value
    /// payloads. Limits bound node count, value bytes and recursion depth,
    /// not total CLR memory or inspection copies. Caller limits may be lower.</summary>
    public static LegacyLibraryTextDomTreeValue CopyFrom(LegacyLibraryTextDomNode source,
        int maximumNodes=MaximumNodes,int maximumValueBytes=MaximumValueBytes,
        int maximumDepth=MaximumDepth,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(source);token.ThrowIfCancellationRequested();
        if(maximumNodes is <1 or >MaximumNodes||maximumValueBytes is <0 or >MaximumValueBytes||maximumDepth is <1 or >MaximumDepth)
            throw new ArgumentOutOfRangeException(nameof(maximumNodes),"Only lower positive node/depth and nonnegative value limits are supported.");
        var pending=new Stack<(LegacyLibraryTextDomNode Node,int Depth)>();pending.Push((source,1));
        int scheduled=1;long bytes=0;
        while(pending.Count!=0)
        {
            token.ThrowIfCancellationRequested();var(node,depth)=pending.Pop();
            if(depth>maximumDepth)throw new NotSupportedException("DOM value depth budget exhausted.");
            bytes+=node.Value.ByteLength;
            if(bytes>maximumValueBytes)throw new NotSupportedException("DOM copied-value byte budget exhausted.");
            long next=(long)scheduled+node.Attributes.Count+node.Children.Count;
            if(next>maximumNodes)throw new NotSupportedException("DOM copied-node budget exhausted.");
            scheduled=(int)next;
            foreach(var child in node.Attributes)pending.Push((child,depth+1));
            foreach(var child in node.Children)pending.Push((child,depth+1));
        }
        LegacyLibraryTextDomTreeValue Copy(LegacyLibraryTextDomNode node)
        {
            token.ThrowIfCancellationRequested();
            var attributes=node.Attributes.Select(Copy).ToArray();var children=node.Children.Select(Copy).ToArray();
            return new(node,attributes,children);
        }
        var result=Copy(source);token.ThrowIfCancellationRequested();return result;
    }
}
