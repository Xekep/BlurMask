using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Actual first1F42D0 interior request to frontend1A0/1AAE70.
/// Uses this first invocation's assigned24A guide, graph and cut coordinates;
/// no main-source frame or caller delimiter decision is substituted.</summary>
public sealed class LegacyFirstOwnedInteriorDelimiter
{
    public LegacyFreshFirstOwnedRecognition Owner {get;}
    private readonly LegacyCursiveCategoryProfile categories;
    private readonly Dictionary<int,LegacyNativeResidualBounds> bounds=[];
    private readonly ushort hyphen,slash,hash,at;
    private static readonly int[] Policies=[0,1,4,2,4,1,0, 2,2,2,2,2,0,0, 1,1,2,1,2,0,0,
        3,1,2,2,2,0,0, 1,1,2,1,2,0,0, 2,0,0,0,0,0,0, 0,0,0,0,0,0,0];
    private LegacyFirstOwnedInteriorDelimiter(LegacyFreshFirstOwnedRecognition owner,LegacyCursiveCategoryProfile categories)
    {
        Owner=owner;this.categories=categories;var alphabet=owner.Factory.InvocationPolicy.Binding.Alphabet;
        ushort Label(string s)=>alphabet.TryGetLabel(s,out ushort value)?value:ushort.MaxValue;
        hyphen=Label("-");slash=Label("/");hash=Label("#");at=Label("@");
    }
    public static LegacyFirstOwnedInteriorDelimiter Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshFirstOwnedRecognition owner,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);token.ThrowIfCancellationRequested();var factory=owner.Factory;
        var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,factory.InvocationPolicy.Binding.SourceOffset,token);
        var model=LegacyFirstAcousticModel.ReadSource(resource,factory.InvocationPolicy.Binding.SourceOffset,token);
        factory.ValidateInvocation(factory.AcousticModel,factory.Frame,factory.InvocationPolicy,token);
        if(model.SourceFingerprint!=factory.SourceFingerprint||!ReferenceEquals(owner.Prepared.SourceRanges.OriginalSource,factory.Frame.Row.OriginalSource)||
            !ReferenceEquals(owner.Prepared.SourceRanges.PreparedChronology,owner.Cuts.Chronology))
            throw new ArgumentException("Exact first factory/cuts/graph/source snapshot required.");
        var graph=owner.Prepared.ResourceGraphOffset;
        if(factory.Frame.Descriptor.CurrentLineId!=0||factory.Frame.Descriptor.Flags!=0x10||
            factory.InvocationPolicy.Binding.NativeModeE0||factory.InvocationPolicy.Binding.SourceKind!=0x135||
            owner.Prepared.SpanProfile.NativeSecondByte!=1||resource[graph+350]!=0)
            throw new NotSupportedException("Fresh ordinary assigned first guide and native non-C4 graph policy required.");
        //Assigned line0 excludes the unassigned1D9 geometry branch even though
        //the NEW container's +8 bit4 is set. Native graph9=1 excludes the AL
        //cased adjustment. Version106 sampler has no custom VE0 geometry.
        token.ThrowIfCancellationRequested();return new(owner,profile.Categories);
    }
    public LegacyFirstInteriorDelimiterResult Read(int previousNode,ushort previousArc,ushort previousSymbol,
        int currentNode,ushort currentArc,ushort currentSymbol,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();var graph=Owner.Prepared.SourceRanges.Graph;
        var prior=graph.GetNode(previousNode);var current=graph.GetNode(currentNode);
        if(previousArc>=prior.Count||currentArc>=current.Count)throw new ArgumentException("Actual first interior graph arcs required.");
        var a=current[currentArc];var b=prior[previousArc];
        //This is the exact1F42 request: kind1, policy2C=0, sourceGap30=0,
        //special34/38=0, no before/after arc. Custom metadata is absent.
        int policy=Policies[FirstId(currentSymbol)+FirstId(previousSymbol)*7];
        if(currentSymbol==hash||currentSymbol==at||currentSymbol==hyphen||previousSymbol==hyphen||
            currentSymbol==slash||previousSymbol==slash)policy=5;
        else if(policy==4)policy=2;
        bool emit;
        if(previousSymbol==hyphen&&b.StartContextId==0)emit=true;
        else
        {
            if(policy==3)policy=b.StartContextId!=0?2:1;
            var left=ReadBounds(b.GroupId,token);var right=ReadBounds(a.GroupId,token);
            float gap=right.MinimumX<=left.MinimumX?left.MinimumX-right.MaximumX:right.MinimumX-left.MaximumX;
            emit=LegacyOwnedRussianMainInteriorDelimiter.EvaluatePolicy(policy,Owner.Factory.Frame.Row.Frame.Line.Height,
                0,gap,float.MaxValue,float.MaxValue,previousSymbol==hyphen||previousSymbol==slash,currentSymbol==hyphen||currentSymbol==slash);
        }
        if(!emit&&(a.Flags&8)!=0)emit=true;
        token.ThrowIfCancellationRequested();return new(emit?(ushort)0xfffb:ushort.MaxValue,emit?1:0,0,0);
    }
    private int FirstId(ushort symbol)
    {
        if(symbol==ushort.MaxValue)return 0;
        if(symbol>=categories.FirstMap.Length||categories.FirstMap.Span[symbol]<0)
            throw new NotSupportedException("Actual first delimiter category mapping is unavailable.");
        int first=categories.FirstIds.Span[categories.FirstMap.Span[symbol]];
        if((uint)first>=7)throw new InvalidDataException("Actual first seven-category domain required.");return first;
    }
    public LegacyNativeResidualBounds ReadBounds(int group,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if(bounds.TryGetValue(group,out var existing))return existing;
        var groups=Owner.Prepared.SourceRanges.Graph.Groups;if((uint)group>=(uint)groups.Count)throw new ArgumentOutOfRangeException(nameof(group));
        var ink=Owner.Cuts.Cuts.PreparedInk;var counts=ink.StrokePointCounts.Span;var starts=new int[counts.Length];
        for(int i=1;i<starts.Length;i++)starts[i]=starts[i-1]+counts[i-1];
        float minx=float.MaxValue,miny=float.MaxValue,maxx=-float.MaxValue,maxy=-float.MaxValue;
        foreach(var r in groups[group].Ranges.Span)for(int p=r.StartPoint;p<r.EndPointExclusive;p++)
        {
            token.ThrowIfCancellationRequested();var point=ink.Points.Span[starts[r.StrokeIndex]+p];
            minx=Math.Min(minx,point.X);miny=Math.Min(miny,point.Y);maxx=Math.Max(maxx,point.X);maxy=Math.Max(maxy,point.Y);
        }
        var result=new LegacyNativeResidualBounds(minx,miny,maxx,maxy);token.ThrowIfCancellationRequested();bounds.Add(group,result);return result;
    }
}
public readonly record struct LegacyFirstInteriorDelimiterResult(ushort Symbol,int Count,byte ComponentFlag,byte ExternalVariant);
