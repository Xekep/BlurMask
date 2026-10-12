namespace InkRecognition;

/// <summary>Portable context identity and exact native symbolic fields. IDs are not addresses.</summary>
public sealed record LegacySymbolContextNode(ulong Identity, ulong ParentLink, ulong ChildTreeRoot,
    ulong Left, ulong Right, ushort Symbol, sbyte Balance, int PageIndex, int PageSlot);

/// <summary>Opaque same-container checkpoint of the live symbolic index and page allocator.</summary>
public sealed class LegacySymbolContextSnapshot
{
    internal ulong Owner { get; }
    internal ulong Root { get; }
    internal LegacySymbolContextNode[] Nodes { get; }
    internal ulong[] TreeOwners { get; }
    internal int[] Used { get; }
    internal int Cursor { get; }
    internal LegacySymbolContextSnapshot(ulong owner, ulong root, LegacySymbolContextNode[] nodes,
        ulong[] treeOwners, int[] used, int cursor)
    { Owner=owner; Root=root; Nodes=nodes; TreeOwners=treeOwners; Used=used; Cursor=cursor; }
}

/// <summary>
/// Bounded native0918d0 ushort AVL index and091b30 102-node page allocator.
/// Symbol-path parent links and per-parent child trees are independent of BST
/// linkage. Portable stable IDs never dereference, encode or imitate native
/// pointers. This is a shared context factory, not a word-startup scheduler.
/// Instances require serialized caller access. Cancellation precedes insertion;
/// a committed bounded insertion completes its rotations without interruption.
/// </summary>
public sealed class LegacySymbolContextFactory
{
    private static long nextIdentity;
    private readonly Dictionary<ulong, Node> nodes = [];
    private readonly List<int> used = [];
    private readonly int maximumNodes;
    private ulong root;
    private int cursor;
    public ulong ContainerIdentity { get; } = NextIdentity();
    public ulong RootTreeIdentity => root;
    public int NodeCount => nodes.Count;
    public int PageCount => used.Count;
    public int PageDescriptorCapacity { get; private set; }
    public int CurrentPageIndex => cursor;
    public const int NodesPerPage = 102;
    public const int NativeNodeBytes = 40;
    public const int NativePageBytes = 4080;

    public LegacySymbolContextFactory(int maximumNodes = 131072)
    {
        if (maximumNodes is < 1 or > 1_000_000) throw new ArgumentOutOfRangeException(nameof(maximumNodes));
        this.maximumNodes=maximumNodes;
    }

    public LegacyDictionaryQueryContext ResolveInitial(ushort symbol, CancellationToken cancellationToken = default)
        => Insert(ref root, 0, symbol, cancellationToken);

    /// <summary>0918d0 on parent+8, allocating from this shared container's pool.</summary>
    public LegacyDictionaryQueryContext ResolveChild(ulong parentIdentity, ushort symbol,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested(); var parent=Get(parentIdentity);
        return Insert(ref parent.ChildRoot, parentIdentity, symbol, cancellationToken);
    }

    /// <summary>0920a0/092990 write newly created node00 separately from AVL child pointers.</summary>
    public void LinkCreatedContext(ulong childIdentity, ulong parentIdentity)
    {
        var child=Get(childIdentity); _=Get(parentIdentity);
        if (child.TreeOwner != parentIdentity || child.Parent != 0 && child.Parent != parentIdentity)
            throw new ArgumentException("Created child must belong to this parent's symbol tree.");
        child.Parent=parentIdentity;
    }

    /// <summary>
    ///092990 initial source68 map: lexical positions preserve their exact mapped
    /// acoustic/control key. FFFF/FFFD entries remain zero; other duplicate keys
    /// share one root-tree node. No script aliases or dictionary state is inferred.
    /// Allocation bound failure yields a zero context, matching native null data.
    /// A canceled batch returns no map; earlier completed insertions remain live.
    /// </summary>
    public ulong[] CreateInitialSourceMap(ReadOnlySpan<ushort> exactDictionaryMap,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (exactDictionaryMap.Length > ushort.MaxValue) throw new ArgumentException("Bounded prepared lexical map required.");
        var result=new ulong[exactDictionaryMap.Length];
        for (int i=0;i<result.Length;i++)
        {
            cancellationToken.ThrowIfCancellationRequested(); ushort key=exactDictionaryMap[i];
            if (key is ushort.MaxValue or 0xfffd) continue;
            result[i]=ResolveInitial(key,cancellationToken).Identity;
        }
        return result;
    }

    public LegacySymbolContextNode ReadNode(ulong identity) => View(Get(identity));
    public IReadOnlyList<LegacySymbolContextNode> CopyNodes()
        => Array.AsReadOnly(nodes.Values.OrderBy(n=>n.Page).ThenBy(n=>n.Slot).Select(View).ToArray());
    public int[] CopyPageUsedCounts() => used.ToArray();

    /// <summary>
    /// Symbolic counterpart of20d8f0's saved container/page copy. It captures
    /// node identities and fields, AVL roots and used counts, not allocator bytes.
    /// </summary>
    public LegacySymbolContextSnapshot Capture(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested(); var all=nodes.Values.OrderBy(n=>n.Page).ThenBy(n=>n.Slot).ToArray();
        var data=new LegacySymbolContextNode[all.Length];var owners=new ulong[all.Length];
        for(int i=0;i<all.Length;i++){cancellationToken.ThrowIfCancellationRequested();data[i]=View(all[i]);owners[i]=all[i].TreeOwner;}
        return new(ContainerIdentity,root,data,owners,used.ToArray(),cursor);
    }

    /// <summary>
    ///1f6e50 restores the saved roots/used pages while retaining current page
    /// descriptor capacity. Post-checkpoint identities become invalid; new nodes
    /// get fresh portable IDs even if an old physical page slot is reused.
    /// Snapshot and dictionary initial map IDs survive a same-owner restore.
    /// </summary>
    public void Restore(LegacySymbolContextSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);cancellationToken.ThrowIfCancellationRequested();
        if(snapshot.Owner!=ContainerIdentity)throw new ArgumentException("Context checkpoint belongs to another container.");
        var restored=new Dictionary<ulong,Node>();
        for(int i=0;i<snapshot.Nodes.Length;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();var n=snapshot.Nodes[i];
            restored.Add(n.Identity,new(n.Identity,n.Symbol,snapshot.TreeOwners[i],n.PageIndex,n.PageSlot)
                {Parent=n.ParentLink,ChildRoot=n.ChildTreeRoot,Left=n.Left,Right=n.Right,Balance=n.Balance});
        }
        cancellationToken.ThrowIfCancellationRequested();nodes.Clear();foreach(var entry in restored)nodes.Add(entry.Key,entry.Value);
        root=snapshot.Root;used.Clear();used.AddRange(snapshot.Used);cursor=snapshot.Cursor;
    }

    private LegacyDictionaryQueryContext Insert(ref ulong treeRoot, ulong owner, ushort symbol, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();Span<ulong> path=stackalloc ulong[32];Span<bool> right=stackalloc bool[32];
        int depth=0;ulong current=treeRoot;
        while(current!=0)
        {
            token.ThrowIfCancellationRequested();var n=Get(current);
            if(n.TreeOwner!=owner)throw new InvalidDataException("Symbol tree ownership differs.");
            if(n.Key==symbol)return new(n.Id,false);
            if(depth==path.Length)throw new InvalidDataException("Bounded native AVL path exceeds32 direction bits.");
            path[depth]=current;right[depth]=symbol>n.Key;current=right[depth]?n.Right:n.Left;depth++;
        }
        token.ThrowIfCancellationRequested();
        if(nodes.Count==maximumNodes)return new(0,false);
        var inserted=Allocate(symbol,owner);ulong id=inserted.Id;
        if(depth==0)treeRoot=id;
        else
        {
            var parent=Get(path[depth-1]);if(right[depth-1])parent.Right=id;else parent.Left=id;
            // Equivalent last-nonzero-pivot update: balance is right height minus left height.
            for(int i=depth-1;i>=0;i--)
            {
                var pivot=Get(path[i]);pivot.Balance=checked((sbyte)(pivot.Balance+(right[i]?1:-1)));
                if(pivot.Balance==0)break;
                if(pivot.Balance is -1 or 1)continue;
                if(pivot.Balance is not (-2 or 2))throw new InvalidDataException("Native AVL balance invariant differs.");
                ulong replacement=Rotate(pivot);
                if(i==0)treeRoot=replacement;
                else {var previous=Get(path[i-1]);if(right[i-1])previous.Right=replacement;else previous.Left=replacement;}
                break;
            }
        }
        return new(id,true);
    }

    private Node Allocate(ushort symbol, ulong owner)
    {
        int page=-1;
        if(cursor<used.Count&&used[cursor]<NodesPerPage)page=cursor;
        else for(int i=used.Count-1;i>=0;i--)if(used[i]<NodesPerPage){page=i;break;}
        if(page<0)
        {
            page=used.Count;int count=page+1;
            if(count>PageDescriptorCapacity)PageDescriptorCapacity=Math.Max(count,(int)(PageDescriptorCapacity*1.5));
            used.Add(0);
        }
        cursor=page;int slot=used[page]++;var n=new Node(NextIdentity(),symbol,owner,page,slot);nodes.Add(n.Id,n);return n;
    }

    /// <summary>0917c0 single/double rotations, including native zero-child-balance cases.</summary>
    private ulong Rotate(Node p)
    {
        if(p.Balance<1)
        {
            var left=Get(p.Left);
            if(left.Balance<1)
            {
                p.Left=left.Right;left.Right=p.Id;bool zero=left.Balance==0;
                left.Balance=zero?(sbyte)1:(sbyte)0;p.Balance=zero?(sbyte)-1:(sbyte)0;return left.Id;
            }
            var middle=Get(left.Right);p.Left=middle.Right;left.Right=middle.Left;middle.Right=p.Id;middle.Left=left.Id;
            if(middle.Balance<0){p.Balance=1;left.Balance=0;}
            else if(middle.Balance>0){left.Balance=-1;p.Balance=0;}
            else {p.Balance=0;left.Balance=0;}
            middle.Balance=0;return middle.Id;
        }
        var r=Get(p.Right);
        if(r.Balance>=0)
        {
            p.Right=r.Left;r.Left=p.Id;bool zero=r.Balance==0;
            r.Balance=zero?(sbyte)-1:(sbyte)0;p.Balance=zero?(sbyte)1:(sbyte)0;return r.Id;
        }
        var m=Get(r.Left);p.Right=m.Left;r.Left=m.Right;m.Left=p.Id;m.Right=r.Id;
        if(m.Balance>0){p.Balance=-1;r.Balance=0;}
        else if(m.Balance<0){r.Balance=1;p.Balance=0;}
        else {p.Balance=0;r.Balance=0;}
        m.Balance=0;return m.Id;
    }

    private Node Get(ulong id)=>nodes.TryGetValue(id,out var n)?n:throw new ArgumentException("Context identity is absent, stale or belongs to another container.");
    private static LegacySymbolContextNode View(Node n)=>new(n.Id,n.Parent,n.ChildRoot,n.Left,n.Right,n.Key,n.Balance,n.Page,n.Slot);
    private static ulong NextIdentity()
    {
        long value=Interlocked.Increment(ref nextIdentity);
        if(value<=0)throw new InvalidOperationException("Portable context identity domain exhausted.");return (ulong)value;
    }
    private sealed class Node(ulong id,ushort key,ulong owner,int page,int slot)
    {
        public ulong Id=id,Parent,ChildRoot,Left,Right;public ushort Key=key;public sbyte Balance;
        public ulong TreeOwner=owner;public int Page=page,Slot=slot;
    }
}
