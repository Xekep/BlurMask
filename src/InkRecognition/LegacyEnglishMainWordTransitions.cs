using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglishMainWordTransitions.Probe")]
namespace InkRecognition;

public sealed record LegacyEnglishMainWordTransitionState(int GroupId,LegacyFirstTransitionBounds Bounds,
    float FutureMinimumX,float PrefixMaximumX,int JoinCost,int TripleJoinCost,int BlankCost,int[] CategoryMatrix);

/// <summary>Same actual English MAIN1941B0 and bounded ordinary118/140 numeric
///component. Prepared request80 is data, never dictionary/controller authority.
///Unsupported reached branches fault this component, never skip a word row.</summary>
public sealed class LegacyEnglishMainWordTransitions
{
    private const int Impossible=0x4000000;
    private readonly LegacyEnglish119MainWordSource source;
    private readonly LegacyOrdinaryCandidateConnections connections;
    private readonly LegacyCursiveCategoryProfile categories;
    private readonly float[][] future,prefix;
    private readonly int[][] join,triple,blank;
    private readonly int[][][] matrix;
    private int active;
    private bool faulted;
    public LegacyEnglishMainWordTransitionProfile Profile=>source.Profile;
    public LegacyEnglish119MainLocalCandidates Owner=>source.Owner;
    public long LogicalAllocationEnvelope=>source.LogicalAllocationEnvelope;
    public int NodeCount=>source.Graph.NodeCount;
    public bool IsFaulted=>Volatile.Read(ref faulted);
    private LegacyEnglishMainWordTransitions(LegacyEnglish119MainWordSource source,CancellationToken token)
    {
        this.source=source;var graph=source.Graph;categories=Profile.ReadCategories(token);connections=new(graph,token);
        int count=graph.NodeCount;future=new float[count][];prefix=new float[count][];join=new int[count][];triple=new int[count][];blank=new int[count][];matrix=new int[count][][];
        for(int n=0;n<count;n++)
        {
            token.ThrowIfCancellationRequested();int length=graph.GetNode(n).Count;future[n]=new float[length];prefix[n]=new float[length];join[n]=new int[length];triple[n]=new int[length];blank[n]=new int[length];matrix[n]=new int[length][];
            for(int a=0;a<length;a++){matrix[n][a]=Enumerable.Repeat(Impossible,36).ToArray();join[n][a]=triple[n][a]=blank[n][a]=Impossible;}
        }
        //1941B0 backwards immediate-successor fold, then native forward prefix.
        for(int n=count-1;n>=0;n--)for(ushort a=0;a<future[n].Length;a++)
        {
            token.ThrowIfCancellationRequested();float low=Box(n,a).MinimumX;
            if(n!=graph.GetNode(n)[a].Span&&n+1<count)foreach(int next in connections.GetSuccessorOrdinals(n,a,0,token).Span)low=Math.Min(low,future[n+1][next]);
            future[n][a]=low;
        }
        float defaultGap=source.SourceGap>=0?source.SourceGap*Profile.SourceGapScale:Profile.DefaultGap;
        var margins=Profile.CopyMatrix();float scale=source.SourceGap<0||margins[0]==0?1:source.SourceGap*Profile.SourceMatrixScale/margins[0];
        for(int n=0;n<count;n++)for(int a=0;a<future[n].Length;a++)
        {
            token.ThrowIfCancellationRequested();var arc=graph.GetNode(n)[a];var box=Box(n,a);prefix[n][a]=box.MaximumX;
            if(n==arc.Span)continue;
            int previous=n-arc.Span-1;float high=-float.MaxValue;
            foreach(int prior in arc.PredecessorArcIndices.Span){token.ThrowIfCancellationRequested();if((uint)prior>=(uint)prefix[previous].Length)throw new InvalidDataException("Actual previous ordinal out of bounds.");high=Math.Max(high,prefix[previous][prior]);}
            if(high==-float.MaxValue)high=box.MaximumX;prefix[n][a]=Math.Max(box.MaximumX,high);
            float gap=(future[n][a]-high)/source.Height;
            join[n][a]=Math.Min(LegacyFirstOrdinaryTransitionGeometry.LogCost((gap-defaultGap)*-Profile.GapSlope,source.Table,true),Profile.JoinMaximumCost);
            triple[n][a]=Math.Min(LegacyFirstOrdinaryTransitionGeometry.LogCost((gap-defaultGap*3f)*-Profile.GapSlope,source.Table,true),Profile.JoinMaximumCost);
            blank[n][a]=LegacyCursiveBlankGapCost.Evaluate(box.MinimumX,box.MaximumX,high,source.Height,Profile.BlankThreshold,Profile.GapSlope,false,source.Table);
            for(int i=0;i<36;i++)
            {
                int cost=LegacyFirstOrdinaryTransitionGeometry.LogCost((gap-scale*margins[i])*Profile.GapSlope,source.Table,false);
                matrix[n][a][i]=i/6==5?unchecked(cost-1):i%6==5?cost:Math.Min(cost,Profile.MatrixMaximumCost);
            }
        }
        source.Validate(token);
    }
    public static LegacyEnglishMainWordTransitions Prepare(LegacyEnglishMainWordTransitionProfile profile,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(profile);profile.Validate(token);
        var actual=profile.Policy.QuerySession.Masks.Knowledge.Owner.RetainWordSource(profile,token);
        return new(actual,token);
    }
    public LegacyEnglishMainWordTransitionState CopyState(int node,int arc,CancellationToken token=default)
    {
        source.Validate(token);if(IsFaulted)throw new InvalidOperationException("Faulted transition component.");var box=Box(node,arc);var actual=source.Graph.GetNode(node)[arc];
        return new(actual.GroupId,box,future[node][arc],prefix[node][arc],join[node][arc],triple[node][arc],blank[node][arc],matrix[node][arc].ToArray());
    }
    internal LegacyFirstTransitionBounds[] CopyPreparedGroupBounds(CancellationToken token=default)
    {source.Validate(token);if(IsFaulted)throw new InvalidOperationException("Faulted transition component.");return source.Bounds.ToArray();}
    public bool Calculate118(LegacyNativeWordRecombinationRequest request,CancellationToken token=default)=>Execute(request,false,token);
    public bool Validate140(LegacyNativeWordRecombinationRequest request,CancellationToken token=default)=>Execute(request,true,token);
    internal bool ExecutePrepared(LegacyNativeWordRecombinationRequest request,bool pair,Action<string> checkpoint,CancellationToken token=default)=>Execute(request,pair,token,checkpoint);
    private bool Execute(LegacyNativeWordRecombinationRequest r,bool pair,CancellationToken token,Action<string>? checkpoint=null)
    {
        ArgumentNullException.ThrowIfNull(r);token.ThrowIfCancellationRequested();source.Validate(token);
        if(Interlocked.CompareExchange(ref active,1,0)!=0)throw new InvalidOperationException("One serialized transition operation required.");
        try
        {
            if(IsFaulted)throw new InvalidOperationException("Faulted transition component.");
            checkpoint?.Invoke("transition:entered");token.ThrowIfCancellationRequested();
            Check(r,pair,token);checkpoint?.Invoke("transition:validated");token.ThrowIfCancellationRequested();int before=r.Cost.Value;bool accepted;
            if(pair)accepted=Pair(r,token);else accepted=Gap(r,token);
            checkpoint?.Invoke("transition:cost");token.ThrowIfCancellationRequested();
            //1525E0/1528C0 frontend accumulator delta, including rejected IMP.
            Write32(r,48,accepted?unchecked(r.ReadInt32(48)+r.Cost.Value-before):Impossible);
            checkpoint?.Invoke("transition:accumulated");token.ThrowIfCancellationRequested();source.Validate(token);return accepted;
        }
        catch{Volatile.Write(ref faulted,true);throw;}
        finally{Volatile.Write(ref active,0);}
    }
    private bool Gap(LegacyNativeWordRecombinationRequest r,CancellationToken token)
    {
        int n=r.ReadInt32(0),a=r.ReadUInt16(12),previous=r.ReadInt32(4),p=r.ReadUInt16(14);byte kind=r.ReadByte(28);
        if(kind==2&&!source.WholeStarts[source.Graph.GetNode(n)[a].GroupId]){r.Cost.Value=Impossible;return false;}
        int correction=join[n][a];
        if(kind==2)
        {
            int current=r.ReadInt32(20),prior=r.ReadInt32(24);if(current is<0 or>5||prior is<0 or>5)throw new NotSupportedException("Actual ordinary first-ID matrix values0..5 required.");
            correction=matrix[n][a][current+prior*6];
        }
        var currentBox=Box(n,a);var previousBox=Box(previous,p);
        float movement=Math.Max(currentBox.MinimumX-previousBox.MinimumX,currentBox.MaximumX-previousBox.MaximumX)/source.Height;
        float value=movement<=Profile.TranslationThreshold?(movement-Profile.TranslationThreshold)*Profile.GapSlope:0;
        if(!float.IsFinite(value)||value>=2147483648f||value<int.MinValue)throw new NotSupportedException("Native translation conversion outside certified domain.");
        r.Cost.Value=unchecked(r.Cost.Value-(int)value*8192+correction);token.ThrowIfCancellationRequested();return r.Cost.Value<r.ReadInt32(52);
    }
    private bool Pair(LegacyNativeWordRecombinationRequest r,CancellationToken token)
    {
        int n=r.ReadInt32(0),a=r.ReadUInt16(12),previous=r.ReadInt32(4);var current=Box(n,a);int currentGeometry=unchecked((sbyte)r.ReadByte(18));
        if((uint)currentGeometry>=(uint)categories.Prototypes.Length)throw new NotSupportedException("Actual signed current geometry index required.");
        int delta;
        if(r.ReadUInt16(38)==ushort.MaxValue)
        {
            var prototype=categories.Prototypes.Span[currentGeometry];float inverse=1f/source.Height;
            float residual=MathF.Abs(prototype.Second-(current.MaximumY-current.MinimumY)*inverse)+MathF.Abs(prototype.First-(current.MaximumY-source.Baseline)*inverse);
            float value=Math.Min(residual*Profile.PairScale,Profile.PairMaximum)*8192f;
            delta=Truncate(value<=0?value-.5f:value+.5f);
        }
        else
        {
            int previousGeometry=unchecked((sbyte)r.ReadByte(19));if((uint)previousGeometry>=(uint)categories.Prototypes.Length)throw new NotSupportedException("Actual signed previous geometry index required.");
            var prior=Box(previous,r.ReadUInt16(14));int ending=r.ReadInt32(8),endingArc=r.ReadUInt16(16);
            if(previous!=ending||r.ReadUInt16(14)!=endingArc)
            {
                //Native1ACFA0 expands the SOURCE bbox by ordinary ENDING bbox.
                var last=Box(ending,endingArc);prior=new(Math.Min(prior.MinimumX,last.MinimumX),Math.Min(prior.MinimumY,last.MinimumY),Math.Max(prior.MaximumX,last.MaximumX),Math.Max(prior.MaximumY,last.MaximumY));
            }
            delta=LegacyFirstOrdinaryTransitionGeometry.PairCost(current,prior,categories.Prototypes.Span[currentGeometry],categories.Prototypes.Span[previousGeometry],source.Height,Profile.PairScale,Profile.PairOffset,Profile.PairMaximum);
        }
        token.ThrowIfCancellationRequested();r.Cost.Value=unchecked(r.Cost.Value+delta);return r.Cost.Value<r.ReadInt32(52);
    }
    private void Check(LegacyNativeWordRecombinationRequest r,bool pair,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var graph=source.Graph;int n=r.ReadInt32(0),a=r.ReadUInt16(12);
        if((uint)n>=(uint)graph.NodeCount||(uint)a>=(uint)graph.GetNode(n).Count||r.ReadUInt32(32)!=graph.GetNode(n)[a].Span||r.ReadByte(28)is not(0 or 2)||r.ReadUInt32(64)!=((Profile.Policy.QuerySession.DescriptorFlags>>9)&15))throw new NotSupportedException("Actual bounded current arc/span/source kind and ordinary0/2 required.");
        source.RequireOrdinaryGroup(n,a);ushort symbol=r.ReadUInt16(36);RequireOrdinary(symbol);
        int previous=r.ReadInt32(4),ending=r.ReadInt32(8),p=r.ReadUInt16(14),last=r.ReadUInt16(16);
        if(r.ReadUInt16(38)==ushort.MaxValue)
        {
            if(!pair||previous!=-1||ending!=-1||p!=ushort.MaxValue||last!=ushort.MaxValue||r.ReadUInt32(68)!=0)throw new NotSupportedException("Actual ordinary initial140 endpoints required.");return;
        }
        RequireOrdinary(r.ReadUInt16(38));
        if((uint)previous>=(uint)graph.NodeCount||(uint)ending>=(uint)graph.NodeCount||previous>ending||ending!=n-graph.GetNode(n)[a].Span-1||(uint)p>=(uint)graph.GetNode(previous).Count||(uint)last>=(uint)graph.GetNode(ending).Count||r.ReadUInt32(68)!=r.ReadUInt32(64)||!graph.GetNode(n)[a].PredecessorArcIndices.Span.Contains(last))throw new NotSupportedException("Actual same-source previous SOURCE/ENDING and predecessor selector required.");
        source.RequireOrdinaryGroup(previous,p);source.RequireOrdinaryGroup(ending,last);
        if(pair&&symbol==Label(".")&&r.ReadUInt16(38)==symbol)throw new NotSupportedException("Reached native dot-pair special geometry requires its separate producer.");
    }
    private ushort Label(string text)=>Profile.Policy.Source.Alphabet.TryGetLabel(text,out ushort label)?label:ushort.MaxValue;
    private void RequireOrdinary(ushort symbol)
    {if(!LegacyNativeOrdinaryWordSymbols.Supports(Profile.Policy.Source.Alphabet,symbol)||symbol==Label("\u2610"))throw new NotSupportedException("Reached protocol/replacement/box geometry branch is outside this ordinary component.");}
    private LegacyFirstTransitionBounds Box(int n,int a)
    {if((uint)n>=(uint)source.Graph.NodeCount||(uint)a>=(uint)source.Graph.GetNode(n).Count)throw new ArgumentOutOfRangeException(nameof(n));return source.Bounds[source.Graph.GetNode(n)[a].GroupId];}
    private static int Truncate(float value)
    {if(!float.IsFinite(value)||value>=2147483648f||value<int.MinValue)throw new NotSupportedException("Native pair conversion outside certified domain.");return(int)value;}
    private static void Write32(LegacyNativeWordRecombinationRequest r,int at,int value)
    {Span<byte>b=stackalloc byte[4];BinaryPrimitives.WriteInt32LittleEndian(b,value);for(int i=0;i<4;i++)r.WriteByte(at+i,b[i]);}
}

