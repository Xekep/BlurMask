namespace InkRecognition;

//Fresh-only017EF0/01A520 AVL and0184E0 membership kernel. Page identities are
//portable creation identities; native page directories are address-sorted.
internal sealed class LegacyMainDocumentSourceIndexKernel<T> where T:notnull
{
    internal sealed class Node
    {
        internal readonly int Identity,PageIdentity,Slot;
        internal readonly LegacyNativeInkPositionRange[] Key;
        internal Node? Left,Right;
        internal sbyte Balance;
        internal int MembershipHead;
        internal T CurrentSource=default!;
        internal Node(int identity,int slots,ReadOnlySpan<LegacyNativeInkPositionRange> key)
        {Identity=identity;PageIdentity=(identity-1)/slots+1;Slot=(identity-1)%slots;Key=key.ToArray();}
    }
    internal sealed record Membership(int Identity,int PageIdentity,int Slot,T Source,int Next);
    internal sealed class Tree
    {
        internal Node? Root;
        internal List<Node> Nodes {get;}=[];
        private readonly int slots;
        internal Tree(int slots){this.slots=slots;}
        internal (Node Node,bool Inserted) Intern(ReadOnlySpan<LegacyNativeInkPositionRange> key,CancellationToken token=default,ILegacyFreshSourceIndexGuard? guard=null)
        {
            token.ThrowIfCancellationRequested();guard?.Require(token);var path=new List<(Node Node,bool Right)>();var cursor=Root;
            while(cursor is not null)
            {
                token.ThrowIfCancellationRequested();guard?.Require(token);int compare=LegacyOwnedMainTextSourceInterner.Compare(key,cursor.Key);
                token.ThrowIfCancellationRequested();guard?.Require(token);
                if(compare==0)return(cursor,false);
                if(path.Count==32)throw new NotSupportedException("Native32-bit AVL insertion path exceeded.");
                bool right=compare>0;path.Add((cursor,right));cursor=right?cursor.Right:cursor.Left;
            }
            if(Nodes.Count==65536)throw new NotSupportedException("Portable fresh index-node budget exceeded.");
            var added=new Node(Nodes.Count+1,slots,key);
#if ENGLISH_FRESH_CONTINUATION_CONTROLS
            LegacyFreshContinuationTestHooks.Checkpoint?.Invoke("after-index-key-copy");
#endif
            token.ThrowIfCancellationRequested();guard?.Require(token);Nodes.Add(added);
            if(path.Count==0){Root=added;return(added,true);}
            var parent=path[^1];if(parent.Right)parent.Node.Right=added;else parent.Node.Left=added;
            for(int i=path.Count-1;i>=0;i--)
            {
                token.ThrowIfCancellationRequested();guard?.Require(token);var p=path[i];p.Node.Balance=(sbyte)(p.Node.Balance+(p.Right?1:-1));
                if(p.Node.Balance==0)break;
                if(p.Node.Balance is not(-2 or 2))continue;
                var rotated=Rotate(p.Node);
                if(i==0)Root=rotated;
                else if(path[i-1].Right)path[i-1].Node.Right=rotated;
                else path[i-1].Node.Left=rotated;
                break;
            }
            return(added,true);
        }
        //Literal017DE0/019990 balance cases (different native balance offsets).
        private static Node Rotate(Node a)
        {
            if(a.Balance<1)
            {
                var b=a.Left!;var c=b.Right;
                if(b.Balance<1)
                {a.Left=c;b.Right=a;sbyte old=b.Balance;b.Balance=(sbyte)(old==0?1:0);a.Balance=(sbyte)(old!=0?0:-1);return b;}
                a.Left=c!.Right;b.Right=c.Left;c.Right=a;c.Left=b;
                if(c.Balance<0){a.Balance=1;b.Balance=0;}
                else if(c.Balance>0){b.Balance=-1;a.Balance=0;}
                else{a.Balance=0;b.Balance=0;}
                c.Balance=0;return c;
            }
            else
            {
                var b=a.Right!;var c=b.Left;
                if(b.Balance>=0)
                {a.Right=c;b.Left=a;sbyte old=b.Balance;b.Balance=(sbyte)(old!=0?0:-1);a.Balance=(sbyte)(old==0?1:0);return b;}
                a.Right=c!.Left;b.Left=c.Right;c.Left=a;c.Right=b;
                if(c.Balance>0){a.Balance=-1;b.Balance=0;}
                else if(c.Balance<0){b.Balance=1;a.Balance=0;}
                else{a.Balance=0;b.Balance=0;}
                c.Balance=0;return c;
            }
        }
    }
    internal Tree Intervals {get;}=new(64);
    internal Tree Vectors {get;}=new(73);
    internal List<Membership> Memberships {get;}=[];
    internal bool Dirty {get;private set;}
    internal int SourcesAdded {get;private set;}
    private int rangeCount;
    internal void Add(T source,ReadOnlySpan<LegacyNativeInkPositionRange> key,CancellationToken token=default,ILegacyFreshSourceIndexGuard? guard=null)
    {
        token.ThrowIfCancellationRequested();guard?.Require(token);
        if(key.Length>65536||key.Length>1_048_576-rangeCount)throw new NotSupportedException("Portable source-index range budget exceeded.");
        bool insertedAny=false;
        foreach(var range in key)
        {
            token.ThrowIfCancellationRequested();guard?.Require(token);
            var (node,inserted)=Intervals.Intern([range],token,guard);
            insertedAny|=inserted;
            token.ThrowIfCancellationRequested();guard?.Require(token);int id=Memberships.Count+1;
            var member=new Membership(id,(id-1)/256+1,(id-1)%256,source,node.MembershipHead);
            token.ThrowIfCancellationRequested();guard?.Require(token);Memberships.Add(member);node.MembershipHead=id;rangeCount++;
        }
        //0184E0 commits dirty only after the entire membership loop succeeds.
        Dirty|=insertedAny;
        var vector=Vectors.Intern(key,token,guard).Node;
        //0207A0/020860 replaces source20, clears cache-vector count28+8,
        //and retains/updates the exact key-header owner18 on both paths.
        token.ThrowIfCancellationRequested();guard?.Require(token);vector.CurrentSource=source;SourcesAdded++;
    }
}

//Optional private owning revocation gate. Default/prepared callers retain null.
internal interface ILegacyFreshSourceIndexGuard
{void Require(CancellationToken token);}
