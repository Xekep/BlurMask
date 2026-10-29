using System.Buffers.Binary;
using System.Text;
namespace InkRecognition.Math;

// Prepared numeric/DOM operands; ownership is supplied ONLY by the separate live adapter.
internal sealed partial class Math2DomDiffScalar
{
    internal readonly byte Tag;
    private readonly byte[] storage,heap;
    private Math2DomDiffScalar(byte tag,byte[] storage,byte[] heap){Tag=tag;this.storage=storage;this.heap=heap;}
    internal static Math2DomDiffScalar From(byte tag,string? text,long? number)
    {
        var data=new byte[16];byte[] bytes=[];
        switch(tag)
        {
            case 0:break; // Structural nodes have no compared scalar.
            case 1:case 2:
                if(text is null)throw new NotSupportedException("Owned string payload required.");
                bytes=new UTF8Encoding(false,true).GetBytes(text);
                if(tag==1){if(bytes.Length<16)throw new NotSupportedException("Canonical produced heap string required.");BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(8),(uint)bytes.Length);}
                else {if(bytes.Length is<1 or>15)throw new NotSupportedException("Canonical produced inline1..15 string required.");bytes.CopyTo(data,0);data[15]=bytes.Length==15?(byte)0:(byte)bytes.Length;bytes=[];}
                break;
            case 3:break;
            case 4:if(number is not long n)throw new NotSupportedException();BinaryPrimitives.WriteInt64LittleEndian(data,n);break;
            case 7:if(number is not long i||i<int.MinValue||i>int.MaxValue)throw new NotSupportedException();BinaryPrimitives.WriteInt32LittleEndian(data,(int)i);break;
            case 10:if(number!=0)throw new NotSupportedException("Only actual produced nrs FLOAT32 zero is admitted.");break;
            default:throw new NotSupportedException("Unported produced DOM scalar tag.");
        }
        return new(tag,data,bytes);
    }
    internal Math2DomDiffScalar Clone(Math2DomDiffWork work){work.Visit(checked(storage.LongLength+heap.LongLength));return new(Tag,storage.ToArray(),heap.ToArray());}
    internal byte[] CopyStorage()=>storage.ToArray();
    internal byte[] CopyHeap()=>heap.ToArray();
    internal Math2DomValueView View()=>new(Tag,storage,heap,Tag==1);
    internal bool Equal(Math2DomDiffScalar other,Math2DomDiffWork work)
    {
        work.Visit(checked(16L+storage.LongLength+other.storage.LongLength+heap.LongLength+other.heap.LongLength));
        // EngineE5820 mode0 dispatches directly to C2B80. Current produced
        // values use identical scalar tags; unequal string representation tags
        // are unequal. Other cross-numeric coercions are deliberately refused.
        if(Tag!=other.Tag)
        {if((Tag is 1 or 2 or 3)&&(other.Tag is 1 or 2 or 3))return false;throw new NotSupportedException("Cross-numeric DOM equality is not in this produced scope.");}
        return Tag switch
        {
            1=>heap.AsSpan().SequenceEqual(other.heap),
            2=>storage[15]==other.storage[15]&&storage.AsSpan(0,storage[15]==0?15:storage[15]).SequenceEqual(other.storage.AsSpan(0,other.storage[15]==0?15:other.storage[15])),
            3=>true,
            4=>BinaryPrimitives.ReadInt64LittleEndian(storage)==BinaryPrimitives.ReadInt64LittleEndian(other.storage),
            7=>BinaryPrimitives.ReadInt32LittleEndian(storage)==BinaryPrimitives.ReadInt32LittleEndian(other.storage),
            10=>true, // Both constructors admitted only canonical FLOAT32 zero.
            _=>throw new NotSupportedException("Structural/unknown scalar is not an equality operand.")
        };
    }
}
internal sealed class Math2DomDiffNode(object writer,object identity,int id,byte kind,LegacyLibraryOriginalDomName name,Math2DomDiffScalar scalar)
{
    internal readonly object Writer=writer,Identity=identity;
    internal readonly int Id=id;
    internal readonly byte Kind=kind;
    internal readonly LegacyLibraryOriginalDomName Name=name;
    internal readonly Math2DomDiffScalar Scalar=scalar;
    internal Math2DomDiffNode? Parent;
    internal readonly List<Math2DomDiffNode> Attributes=[],Children=[];
}
internal sealed record Math2DomDiffActionSnapshot(byte Operation,int NodeAt18,int NodeAt20,int Ordinal,int SourceCandidate,
    byte EnvelopeKind,string Name,byte OldTag,byte NewTag,byte[] OldStorage,byte[] NewStorage,byte[] OldHeap,byte[] NewHeap,bool Filtered);
internal sealed class Math2DomDiffAction
{
    internal readonly byte Operation;
    internal readonly Math2DomDiffNode NodeAt18;
    internal readonly Math2DomDiffNode? NodeAt20;
    internal readonly int Ordinal,SourceCandidate;
    internal readonly Math2DomDiffScalar? OldValue,NewValue;
    internal Math2DomDiffAction(byte operation,Math2DomDiffNode at18,Math2DomDiffNode? at20=null,int ordinal=-1,int sourceCandidate=0,
        Math2DomDiffScalar? oldValue=null,Math2DomDiffScalar? newValue=null)
    {Operation=operation;NodeAt18=at18;NodeAt20=at20;Ordinal=ordinal;SourceCandidate=sourceCandidate;OldValue=oldValue;NewValue=newValue;}
    internal Math2DomDiffActionSnapshot Snapshot(bool filtered=false)=>new(Operation,NodeAt18.Id,NodeAt20?.Id??0,Ordinal,SourceCandidate,
        NodeAt18.Kind,NodeAt18.Name.Text,OldValue?.Tag??0,NewValue?.Tag??0,OldValue?.CopyStorage()??[],NewValue?.CopyStorage()??[],OldValue?.CopyHeap()??[],NewValue?.CopyHeap()??[],filtered);
}
internal sealed class Math2DomDiffWork(CancellationToken token)
{
    internal const long MaximumVisits=16L*1024*1024,MaximumCells=1048576;
    internal long Visits,Cells;
    internal CancellationToken Token=>token;
    internal void Visit(long n=1){token.ThrowIfCancellationRequested();if(n<0||n>MaximumVisits-Visits)throw new NotSupportedException("Default DOM diff work limit before read.");Visits+=n;}
    internal void Matrix(long cells){Visit();if(cells<0||cells>MaximumCells-Cells)throw new NotSupportedException("Default DOM diff matrix limit before allocation.");Cells+=cells;}
}
internal readonly record struct Math2DomDiffPair(int Old,int New);
internal static class Math2DomDefaultDiff
{
    internal static float PairCost(Math2DomDiffNode old,Math2DomDiffNode next,Math2DomDiffWork work)
    {
        work.Visit();if(old.Kind!=next.Kind)return 10;
        if(!ReferenceEquals(old.Name.Owner,next.Name.Owner))throw new NotSupportedException("Shared canonical name owner required.");
        return old.Kind switch
        {
            3=>ReferenceEquals(old.Name,next.Name)?0:2, // D4810: schema/cache pointer0.
            0=>!ReferenceEquals(old.Name,next.Name)?2:old.Scalar.Equal(next.Scalar,work)?0:1,
            1=>old.Scalar.Equal(next.Scalar,work)?0:1,
            _=>10
        };
    }
    internal static Math2DomDiffPair[] Align(IReadOnlyList<Math2DomDiffNode> old,IReadOnlyList<Math2DomDiffNode> next,Math2DomDiffWork work)
    {
        work.Visit();int prefix=0,suffix=0;
        while(prefix<System.Math.Min(old.Count,next.Count)&&PairCost(old[prefix],next[prefix],work)==0)prefix++;
        while(old.Count-prefix-suffix>1&&next.Count-prefix-suffix>1&&PairCost(old[old.Count-1-suffix],next[next.Count-1-suffix],work)==0)suffix++;
        int m=old.Count-prefix-suffix,n=next.Count-prefix-suffix;
        if(m>512||n>512)throw new NotSupportedException("Bounded native default child matrix required.");
        long count=checked((m+1L)*(n+1));work.Matrix(count);var matrix=new float[count];int stride=n+1;
        for(int j=0;j<=n;j++){work.Visit();matrix[j]=j;}
        for(int i=1;i<=m;i++)
        {
            work.Visit();matrix[i*stride]=i;
            for(int j=1;j<=n;j++)
            {
                work.Visit(4);float above=matrix[(i-1)*stride+j]+1f,left=matrix[i*stride+j-1]+1f;
                float best=left<=above?left:above,diagonal=matrix[(i-1)*stride+j-1];
                if(diagonal<best){float scaled=PairCost(old[prefix+i-1],next[prefix+j-1],work)*10f;float replacement=scaled+diagonal;if(replacement<=best)best=replacement;}
                matrix[i*stride+j]=best;
            }
        }
        var reverse=new List<Math2DomDiffPair>();int a=m,b=n;
        while(a>0||b>0)
        {
            work.Visit(4);float above=a>0?matrix[(a-1)*stride+b]:float.MaxValue,left=b>0?matrix[a*stride+b-1]:float.MaxValue;
            float diagonal=a>0&&b>0?matrix[(a-1)*stride+b-1]:float.MaxValue;
            if(a<1||b<1||matrix[a*stride+b]-diagonal>1f)diagonal=float.MaxValue;
            float minimum=left<=above?left:above;if(diagonal<=minimum)minimum=diagonal;
            if(minimum==diagonal){reverse.Add(new(prefix+a-1,prefix+b-1));a--;b--;}
            else if(minimum==left){reverse.Add(new(-1,prefix+b-1));b--;}
            else {reverse.Add(new(prefix+a-1,-1));a--;}
        }
        reverse.Reverse();var result=new List<Math2DomDiffPair>(prefix+reverse.Count+suffix);
        for(int i=0;i<prefix;i++){work.Visit();result.Add(new(i,i));}result.AddRange(reverse);
        for(int i=suffix;i>0;i--){work.Visit();result.Add(new(old.Count-i,next.Count-i));}return result.ToArray();
    }
    internal static List<Math2DomDiffAction> Produce(Math2DomDiffNode oldRoot,Math2DomDiffNode newRoot,
        Func<Math2DomDiffNode,Math2DomDiffNode> cloneIntoOldWriter,Math2DomDiffWork work,Action<Math2DomDiffAction>? linked=null)
    {
        if(oldRoot.Kind!=3||newRoot.Kind!=3||!ReferenceEquals(oldRoot.Name,newRoot.Name)||ReferenceEquals(oldRoot.Writer,newRoot.Writer))throw new NotSupportedException("Distinct owned writers/same canonical Math root required.");
        var actions=new List<Math2DomDiffAction>();
        void Link(Math2DomDiffAction action){work.Visit();if(actions.Count>=65536)throw new NotSupportedException("Bounded produced action count.");actions.Add(action);linked?.Invoke(action);}
        Math2DomDiffNode Clone(Math2DomDiffNode value){work.Visit();var copy=cloneIntoOldWriter(value);if(!ReferenceEquals(copy.Writer,oldRoot.Writer))throw new InvalidOperationException("Actual payload clone must belong to retained OLD writer.");return copy;}
        void Walk(Math2DomDiffNode old,Math2DomDiffNode next,int depth)
        {
            work.Visit();if(depth>64)throw new NotSupportedException("Bounded actual default Math DOM depth required.");
            if(old.Kind!=next.Kind)throw new InvalidDataException("Default alignment unexpectedly paired different kinds.");
            if(old.Kind==1)
            {if(!old.Scalar.Equal(next.Scalar,work))Link(new(7,old,sourceCandidate:next.Id,oldValue:old.Scalar.Clone(work),newValue:next.Scalar.Clone(work)));return;}
            if(old.Kind!=3||!ReferenceEquals(old.Name,next.Name))throw new NotSupportedException("Default paired element names must match in this Math scope.");
            foreach(var attr in old.Attributes)
            {
                work.Visit();Math2DomDiffNode? found=null;foreach(var other in next.Attributes){work.Visit();if(ReferenceEquals(attr.Name,other.Name)){found=other;break;}}
                if(found is null)Link(new(6,old,attr));
                else if(!attr.Scalar.Equal(found.Scalar,work))Link(new(7,attr,sourceCandidate:found.Id,oldValue:attr.Scalar.Clone(work),newValue:found.Scalar.Clone(work)));
            }
            foreach(var attr in next.Attributes)
            {work.Visit();bool found=false;foreach(var other in old.Attributes){work.Visit();if(ReferenceEquals(attr.Name,other.Name)){found=true;break;}}if(!found)Link(new(5,old,Clone(attr),sourceCandidate:attr.Id));}
            int cursor=-1;foreach(var pair in Align(old.Children,next.Children,work))
            {
                work.Visit();if(pair.Old<0){cursor=cursor<0?0:cursor+1;var value=next.Children[pair.New];Link(new(2,old,Clone(value),cursor,value.Id));}
                else if(pair.New<0){var value=old.Children[pair.Old];Link(new(4,old,value,cursor<0?0:cursor+1));}
                else {Walk(old.Children[pair.Old],next.Children[pair.New],depth+1);cursor=cursor<0?0:cursor+1;}
            }
        }
        Walk(oldRoot,newRoot,0);return actions;
    }
}
