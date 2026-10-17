using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Actual ordinary global frontend1A0/1A8 source callbacks. Pending
/// metadata is handled before endpoint/geometry, retaining actual controls.
/// General kind1 geometry and custom/known-input groups remain separate.</summary>
public sealed class LegacyFreshMainGlobalSourceSymbolCallbacks
{
    private readonly int[][] controls;
    public LegacyFreshMainGlobalSourceGeometry Geometry{get;}
    public LegacyFreshMainCompactOrdinaryGraph Graph=>Geometry.Graph;
    private LegacyFreshMainGlobalSourceSymbolCallbacks(LegacyFreshMainGlobalSourceGeometry geometry,int[][] controls)
    {Geometry=geometry;this.controls=controls;}
    public static LegacyFreshMainGlobalSourceSymbolCallbacks Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalSourceGeometry geometry,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(geometry);token.ThrowIfCancellationRequested();
        var source=geometry.Graph.Source.Source;
        if(resource.Length!=572763||!Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(resource)).Equals(
            source.SourceFingerprint,StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Same actual global source resource required.");
        if(source.GraphOffset!=189379||resource[source.GraphOffset+350]!=0)
            throw new NotSupportedException("Actual main disabled graph159 Asian delimiter required.");
        //19F850 clears graph1F0 after attaching pending19EB10 to a real
        //ordinary allocation. Trailing/unconsumed fallback metadata is not
        //represented by the compact group/context owner and cannot be guessed.
        if(source.Steps.Count==0||source.Steps[^1].IsPendingSeparator||
            geometry.Graph.Source.LocalGraphs.Steps.Any(s=>s.LocalState.GroupCount==0))
            throw new NotSupportedException("Consumed pending metadata and an ordinary final allocation required; trailing fallback metadata is separate.");
        var contexts=geometry.Graph.Source.State.Contexts;var controls=new int[contexts.Count][];
        for(int i=0;i<contexts.Count;i++)
        {
            token.ThrowIfCancellationRequested();byte[] prefix=contexts[i].CopyPrefix();controls[i]=contexts[i].CopyControls();
            if(prefix[0]!=1||prefix.AsSpan(1,5).ContainsAnyExcept((byte)0)||BinaryPrimitives.ReadInt32LittleEndian(prefix.AsSpan(8))!=0||
                controls[i].Any(c=>c!=-7))
                throw new NotSupportedException("Actual fresh kind3 pending prefix and -7 controls required.");
        }
        return new(geometry,controls);
    }
    public ushort ReadFallback(int node,ushort arc,CancellationToken token=default)
    {_=Geometry.ReadKind(node,arc,token);return ushort.MaxValue;}
    public LegacyRussianMainReplacementResolution ResolveReplacement(int node,ushort arc,CancellationToken token=default)
    {
        _=ReadFallback(node,arc,token);
        if(!Geometry.Policy.Owner.Model.AcousticAlphabet.TryGetLabel("\ufffd",out ushort replacement))
            throw new InvalidDataException("Actual acoustic replacement label required.");
        return new(replacement,replacement,0);
    }
    public LegacyFreshMainGlobalDelimiterResult ReadDelimiter(ReadOnlySpan<byte> preparedRequest64,int cursor,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(preparedRequest64.Length!=64||cursor is <0 or >256)
            throw new ArgumentException("Complete request64 and portable256-label scratch cursor required.");
        byte[] request=preparedRequest64.ToArray();
        int I(int at)=>BinaryPrimitives.ReadInt32LittleEndian(request.AsSpan(at));
        ushort U(int at)=>BinaryPrimitives.ReadUInt16LittleEndian(request.AsSpan(at));
        int previous=I(8),current=I(24);
        LegacyMainCompactOrdinaryArc? actual=null;
        if(current!=Graph.State.NodeCount){_=Geometry.ReadKind(current,U(28),token);actual=Graph.State.GetNode(current)[U(28)];}
        int[] metadata=[];
        if(actual is not null&&actual.AuxiliaryContextIndex!=ushort.MaxValue)
        {
            if(actual.AuxiliaryContextIndex>=controls.Length)throw new InvalidDataException("Actual global context allocation required.");
            metadata=controls[actual.AuxiliaryContextIndex];
        }
        if(cursor==0)return new(this,cursor,[]);
        if(metadata.Length!=0)
        {
            //NEW00C860 WORD8=10h ->150D00 config1=1 ->1A6980 graph1D9.
            //For each -7 control1AAE70 emitsFFF9. 1A96D0 returns0 because
            //the control's high signed short isFFFF; prefix5 is also0.
            //After the scratch is exhausted native decrements without write.
            token.ThrowIfCancellationRequested();
            var span=CalculatePendingWriteSpan(cursor,metadata.Length);
            return new(this,span.RemainingCursor,Enumerable.Repeat((ushort)0xfff9,span.WrittenCount).ToArray());
        }
        if(previous==-1)return new(this,cursor,[]);
        _=Geometry.ReadKind(previous,U(12),token);
        if(current==Graph.State.NodeCount)return new(this,cursor,[]);
        //1AB1xx resolves request10/14 before its ordinary-kind dispatch,
        //including kind0. Retain that read's actual ownership/in-bounds guard.
        int auxiliaryNode=I(16);ushort auxiliaryArc=U(20);
        if(auxiliaryNode==-1)
        {
            if(auxiliaryArc!=0||Graph.State.Root.GroupId>=(uint)Graph.State.Groups.Count)
                throw new NotSupportedException("Native root auxiliary source requires its actual arc0/group allocation.");
        }
        else _=Geometry.ReadKind(auxiliaryNode,auxiliaryArc,token);
        int kind=I(40);
        if(kind==0)return new(this,cursor-1,[0xfffc]);
        if(kind==2)return(actual!.Flags&8)!=0?new(this,cursor-1,[0xfffb]):new(this,cursor,[]);
        if(kind==1)
        {
            //Both actual descriptors are ordinary0: request2C==2's
            //known-input(1/2) test is false and branches directly toFFFB.
            if(I(44)==2)return new(this,cursor-1,[0xfffb]);
            throw new NotSupportedException("Global interior kind1 geometry/category policy is not restored by this source callback.");
        }
        return new(this,cursor,[]);
    }
    internal static (int RemainingCursor,int WrittenCount) CalculatePendingWriteSpan(int cursor,int actualControlCount)
    {
        if(cursor is <0 or >256||actualControlCount is <1 or >4096)throw new ArgumentOutOfRangeException(nameof(actualControlCount));
        return cursor==0?(0,0):(unchecked(cursor-actualControlCount),Math.Min(cursor,actualControlCount));
    }
}

/// <summary>Actual pointer-free1AAE70 output. A negative remaining cursor is
/// possible after metadata exhaustion; only the in-bounds native writes are
/// represented. This is not a finalized glyph or a text string.</summary>
public sealed class LegacyFreshMainGlobalDelimiterResult
{
    private readonly ushort[] symbols;
    public LegacyFreshMainGlobalSourceSymbolCallbacks Owner{get;}
    public int RemainingCursor{get;}
    public byte NativeFlag0C=>0;
    public byte NativeFlag0D=>0;
    internal LegacyFreshMainGlobalDelimiterResult(LegacyFreshMainGlobalSourceSymbolCallbacks owner,int cursor,ushort[] symbols)
    {Owner=owner;RemainingCursor=cursor;this.symbols=symbols;}
    public ushort[] CopySymbols()=>symbols.ToArray();
}
