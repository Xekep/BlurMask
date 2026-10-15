using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainResidualSource.Probe")]
namespace InkRecognition;

/// <summary>Functional ordinary1AC3C0 source overlay. Higher references own
/// merged source ranges and retain the original last BASE source reference;
/// they never become new neural groups or substituted geometry boxes.</summary>
public sealed class LegacyOwnedRussianMainResidualSource
{
    private readonly LegacyOwnedFreshMainWordControllerPolicy controller;
    private readonly List<Extra>[] extras;
    private readonly object gate=new();
    private int totalExtras,totalStoredRanges;
    public LegacyOwnedRussianMainSourceGeometry Geometry {get;}
    public LegacyOwnedRussianMainGlyphConfidence ConfidenceOwner=>Geometry.Owner;
    public LegacyOrdinaryCursiveCandidateGraph Graph=>Geometry.Graph;
    public bool IsCaseAware {get;}
    private LegacyOwnedRussianMainResidualSource(LegacyOwnedRussianMainSourceGeometry geometry,
        LegacyOwnedFreshMainWordControllerPolicy controller,bool isCase)
    {Geometry=geometry;this.controller=controller;IsCaseAware=isCase;extras=Enumerable.Range(0,Graph.NodeCount).Select(_=>new List<Extra>()).ToArray();}

    public static LegacyOwnedRussianMainResidualSource Prepare(LegacyOwnedRussianMainSourceGeometry geometry,
        LegacyOwnedFreshMainWordControllerPolicy policy,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(geometry);ArgumentNullException.ThrowIfNull(policy);token.ThrowIfCancellationRequested();
        if(policy.Owner.IsCaseAware||!ReferenceEquals(geometry.Owner.DirectDictionaryOwner,policy.Owner.DictionaryOwner))
            throw new ArgumentException("Same actual fresh DIRECT dictionary owner required.");
        Validate(geometry,policy,token);return new(geometry,policy,false);
    }
    public static LegacyOwnedRussianMainResidualSource Prepare(LegacyOwnedRussianMainSourceGeometry geometry,
        LegacyFreshCaseMainSourcePolicy policy,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(geometry);ArgumentNullException.ThrowIfNull(policy);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(geometry.Owner.CaseDictionaryOwner,policy.DictionaryOwner))
            throw new ArgumentException("Same actual fresh CASE dictionary owner required.");
        Validate(geometry,policy.Controller,token);return new(geometry,policy.Controller,true);
    }
    private static void Validate(LegacyOwnedRussianMainSourceGeometry geometry,LegacyOwnedFreshMainWordControllerPolicy policy,CancellationToken token)
    {
        if(!ReferenceEquals(geometry.Graph,policy.Owner.SourceGraph)||!ReferenceEquals(geometry.Owner.Model,policy.Owner.CommonModel)||
            policy.Configuration.Trace2368!=0||policy.Configuration.Mode22f6!=0||policy.Refresh.KnowledgeOption22f8!=0||geometry.CollectionC1)
            throw new NotSupportedException("Same unmodified nontracing ordinary135 main source required.");
        //Fresh00C860 clears98..9F, including99. The independent2458B0
        //temporary knowledge99 override is outside these private fresh owners.
        token.ThrowIfCancellationRequested();
    }

    public ulong ResolveBaseReference(ulong reference,CancellationToken token=default)
    {
        lock(gate)
        {
            var s=Decode(reference,token);int baseCount=Graph.GetNode(s.Node).Count;
            return s.Arc<baseCount?reference:extras[s.Node][s.Arc-baseCount].OriginalLast;
        }
    }
    public LegacyInkRange[] CopySourceRanges(ulong reference,CancellationToken token=default)
    {
        lock(gate)
        {
            var s=Decode(reference,token);int baseCount=Graph.GetNode(s.Node).Count;
            return s.Arc<baseCount?Geometry.CopyGroupRanges(s.Node,s.Arc,token):extras[s.Node][s.Arc-baseCount].Ranges.ToArray();
        }
    }
    public int ReadRangeCapacity(ulong reference,CancellationToken token=default)
    {
        lock(gate){var s=Decode(reference,token);int count=Graph.GetNode(s.Node).Count;
            if(s.Arc<count)throw new NotSupportedException("Native base-group range capacity is not represented by this overlay.");
            return extras[s.Node][s.Arc-count].Capacity;}
    }
    public LegacyNativeResidualBounds ReadBounds(ulong reference,CancellationToken token=default)
    {var b=LegacyNativeResidualSource.Decode(ResolveBaseReference(reference,token));return Geometry.ReadBounds(b.Node,b.Arc,token);}

    public LegacyNativeResidualMerge MergeSources(ReadOnlySpan<ulong> input,CancellationToken token=default)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();if(input.Length is <1 or >65536)throw new ArgumentException("Bounded nonempty source merge required.");
            //Native count1 returns all64 input bits unchanged. Portable owner
            //validation still refuses an unknown source reference.
            if(input.Length==1){_=Decode(input[0],token);return new(true,input[0]);}
            var refs=input.ToArray();var concatenated=new List<LegacyInkRange>();int capacity=0;
            foreach(ulong reference in refs)
            {
                var s=Decode(reference,token);
                //Native concatenation dereferences BASE node arc arrays before
                //the extra cache search. A higher multi-input is not justified.
                if(s.Arc>=Graph.GetNode(s.Node).Count)throw new NotSupportedException("Native multi-input higher-arc concatenation is not restored.");
                var ranges=Geometry.CopyGroupRanges(s.Node,s.Arc,token);int required=checked(concatenated.Count+ranges.Length);
                if(required>1_048_576)throw new NotSupportedException("Portable merge range-work budget exceeded.");
                if(capacity<required)capacity=Math.Max(required,(int)(capacity*1.5));
                concatenated.AddRange(ranges);
            }
            var merged=SortAndCoalesce(concatenated.ToArray(),token);var last=LegacyNativeResidualSource.Decode(refs[^1]);
            int baseCount=Graph.GetNode(last.Node).Count;
            for(int i=0;i<extras[last.Node].Count;i++)
            {token.ThrowIfCancellationRequested();if(extras[last.Node][i].Ranges.AsSpan().SequenceEqual(merged))return new(true,Pack(last.Node,baseCount+i));}
            //Native stops at the FIRST matching global ordinary group, even
            //if a later duplicate group might have matching context endpoints.
            int matching=-1;
            for(int group=0;group<Graph.Groups.Count;group++)
            {token.ThrowIfCancellationRequested();if(Geometry.GroupRangesEqual(group,merged,token)){matching=group;break;}}
            if(matching!=-1)
            {
                int low=int.MaxValue,high=int.MinValue;
                foreach(ulong reference in refs)
                {var s=LegacyNativeResidualSource.Decode(reference);var arc=Graph.GetNode(s.Node)[s.Arc];low=Math.Min(low,unchecked((int)arc.StartContextId));high=Math.Max(high,unchecked((int)arc.EndContextId));}
                for(int node=0;node<Graph.NodeCount;node++)for(int arc=0;arc<Graph.GetNode(node).Count;arc++)
                {token.ThrowIfCancellationRequested();var a=Graph.GetNode(node)[arc];if(a.GroupId==matching&&unchecked((int)a.StartContextId)==low&&unchecked((int)a.EndContextId)==high)return new(true,Pack(node,arc));}
            }
            if(baseCount+extras[last.Node].Count>=65536||totalExtras>=65536||merged.Length>1_048_576-totalStoredRanges)
                throw new NotSupportedException("Portable higher-source ordinal/storage budget exceeded.");
            token.ThrowIfCancellationRequested();int ordinal=baseCount+extras[last.Node].Count;
            extras[last.Node].Add(new(merged,capacity,refs[^1]));totalExtras++;totalStoredRanges+=merged.Length;return new(true,Pack(last.Node,ordinal));
        }
    }

    public LegacyOwnedMainResidualAssociation Apply(LegacyOwnedMainGlyphBacktrackRows backtrack,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(backtrack);token.ThrowIfCancellationRequested();
        var materialization=backtrack.Materialization;
        if(!ReferenceEquals(materialization.TerminalResult.PassResult.ControllerOwner,controller)||
            !ReferenceEquals(materialization.ConfidenceOwner,ConfidenceOwner))
            throw new ArgumentException("Same actual compact/backtrack/controller/confidence owner required.");
        //217DB0 belongs to each individual218560 flush, not the complete
        //21FFD0 trace. Until that segment owner is supplied, a whole-trace
        //association could move residual ink across word boundaries.
        var residuals=backtrack.CopyResidualSourceReferences();
        if(residuals.Length!=0)
            throw new NotSupportedException("Nonempty residual association requires an owned native flush segment.");
        lock(gate)
        {
            LegacyNativeResidualSource Base(LegacyNativeResidualSource source)=>LegacyNativeResidualSource.Decode(ResolveBaseReference(Pack(source.Node,source.Arc),token));
            var operations=new LegacyNativeResidualAssociationOperations(
                s=>{var b=Base(s);return Geometry.ReadKind(b.Node,b.Arc,token);},
                s=>{var b=Base(s);return Geometry.ReadLine(b.Node,b.Arc,token);},
                s=>ReadBounds(Pack(s.Node,s.Arc),token),line=>Geometry.ReadGuide(line,token),
                symbol=>Geometry.ReadAlphabetFlag(symbol,token),
                s=>{var b=Base(s);return Geometry.ReadTopMark(b.Node,b.Arc,token);},MergeSources);
            var result=LegacyNativeResidualGlyphAssociation.Apply(backtrack.CopyRows(),residuals,
                false,Geometry.CollectionC1,checked((ushort)ConfidenceOwner.Model.AcousticAlphabet.LabelCount),operations,token);
            token.ThrowIfCancellationRequested();return new(this,backtrack,result);
        }
    }

    private LegacyNativeResidualSource Decode(ulong reference,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var s=LegacyNativeResidualSource.Decode(reference);
        if((uint)s.Node>=(uint)Graph.NodeCount||s.Arc>=Graph.GetNode(s.Node).Count+extras[s.Node].Count)
            throw new ArgumentException("Owned base or previously allocated higher source reference required.");return s;
    }
    private sealed record Extra(LegacyInkRange[] Ranges,int Capacity,ulong OriginalLast);
    private static ulong Pack(int node,int arc)=>(uint)node&0xffffffUL|((ulong)checked((ushort)arc)<<24);

    internal static LegacyInkRange[] SortAndCoalesce(LegacyInkRange[] owned,CancellationToken token=default)
    {
        if(owned.Length==0)throw new ArgumentException("Nonempty range storage required.");
        var pending=new Stack<(int Left,int Right)>();pending.Push((0,owned.Length-1));
        while(pending.Count!=0)
        {
            token.ThrowIfCancellationRequested();var(left,right)=pending.Pop();int i=left,j=right;var pivot=owned[(left+right)/2];
            int Compare(LegacyInkRange x)=>x.StrokeIndex!=pivot.StrokeIndex?x.StrokeIndex.CompareTo(pivot.StrokeIndex):x.StartPoint.CompareTo(pivot.StartPoint);
            do{while(Compare(owned[i])<0)i++;while(Compare(owned[j])>0)j--;if(i>j)break;(owned[i],owned[j])=(owned[j],owned[i]);i++;j--;}while(i<=j);
            //Same disjoint partitions and swaps, without native recursion's
            //unbounded managed call-stack requirement on adversarial order.
            if(i<right)pending.Push((i,right));if(left<j)pending.Push((left,j));
        }
        var ranges=new List<LegacyInkRange>(owned.Length);
        //Equivalent compaction avoids repeating native memmove for every join.
        foreach(var range in owned)
        {
            token.ThrowIfCancellationRequested();
            if(ranges.Count!=0&&range.StrokeIndex==ranges[^1].StrokeIndex&&range.StartPoint==ranges[^1].EndPointExclusive-1)
            {var previous=ranges[^1];ranges[^1]=new(previous.StrokeIndex,previous.StartPoint,range.EndPointExclusive);}
            else ranges.Add(range);
        }
        return ranges.ToArray();
    }
}

public sealed class LegacyOwnedMainResidualAssociation
{
    private readonly LegacyNativeResidualAssociationResult result;
    public LegacyOwnedRussianMainResidualSource Owner {get;}
    public LegacyOwnedMainGlyphBacktrackRows BacktrackRows {get;}
    public LegacyOwnedRussianMainSourceGeometry Geometry=>Owner.Geometry;
    public bool Succeeded=>result.Succeeded;
    public int RemainingCount=>result.RemainingCount;
    public byte[] CopyRows72()=>result.CopyRows();
    public ulong[] CopyResidualStorage()=>result.CopyResidualStorage();
    internal LegacyOwnedMainResidualAssociation(LegacyOwnedRussianMainResidualSource owner,LegacyOwnedMainGlyphBacktrackRows backtrack,LegacyNativeResidualAssociationResult result)
    {Owner=owner;BacktrackRows=backtrack;this.result=result;}
}
