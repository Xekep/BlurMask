using System.Numerics;

namespace InkRecognition;

internal readonly record struct LegacyMathAkOriginalSmoothingPolicy(float Minimum,float Maximum,float Extra,
    float OrthogonalLow,float OrthogonalHigh,float CornerLow,float CornerHigh,float FlatnessSquaredFactor);

internal sealed partial class LegacyMathAkOriginalInputContext
{
    //Adds8 logical reference bytes to EVERY context, even historical/null ones.
    private Source13A0Frame? source13A0;
    private const long Source13A0LogicalPayloadLimit=32L*1024*1024;
    internal static LegacyMathAkOriginalInputContext PrepareFreshThrough13A0(
        LegacyMathAkSourceGeometry original,CancellationToken token=default)
        =>PrepareFreshThrough13A0Core(original,null,token);
    internal static LegacyMathAkOriginalInputContext PrepareFreshThrough13A0Diagnostic(
        LegacyMathAkSourceGeometry original,Action<Source13A0Checkpoint,Source13A0Diagnostic> observer,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(observer);
        return PrepareFreshThrough13A0Core(original,observer,token);
    }
    private static LegacyMathAkOriginalInputContext PrepareFreshThrough13A0Core(
        LegacyMathAkSourceGeometry original,Action<Source13A0Checkpoint,Source13A0Diagnostic>? observer,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(original);token.ThrowIfCancellationRequested();
        Require13A0Admission(); //Scalar-only entry before original-frame creation and claim.
        int s=original.StrokeCount,n=original.PointCount,p=checked(n+4*s);
        if(s is<1 or>64||n is<1 or>LegacyMathAkSourceGeometry.MaximumPoints||
            p>LegacyMathAkProcessedStrokePrefix.MaximumExpandedPoints)
            throw new InvalidOperationException("Original source cardinality mismatch.");
        var budget=new Source13A0Budget();
        int w=(s>>5)+1,a=(s+31)>>5;
        //Exact existing site ledger proposal plus the new optional8B field.
        budget.Reserve(checked(32L*n+100L*p+228L*s+6L*s*(s+1)+8L*s*w+4L*(a+w)+216));
        budget.Reserve(checked(316L*p+156L*s+68));
        var frame=LegacyMathAkOriginalInputFrame.CreateFresh(original,token);
        var context=new LegacyMathAkOriginalInputContext(frame);
        try
        {
            context.prefix=LegacyMathAkProcessedStrokePrefix.PrepareOriginalInput(frame,token);
            context.Preflight(token);context.RunFresh13B6(token);
            Source13A0Diagnostic? diagnostic=observer is null?null:Source13A0Diagnostic.Issue(context);
            Notify13A0(observer,diagnostic,Source13A0Checkpoint.After13B6,token);
            context.Run13A0(budget,observer,diagnostic,token);
            token.ThrowIfCancellationRequested();context.phase=3;return context;
        }
        catch{context.phase=-1;throw;} //Same consumed owner poisoned, never restoreclaim.
    }
    private static void Require13A0Admission()
    { }

    internal enum Source13A0Checkpoint { After13B6,AfterCountAndReservation,AfterFillBeforePublication }
    //One view per diagnostic factory call. Scalar-only snapshots, no retained-array export.
    internal readonly record struct Source13A0Snapshot(int Phase,int Claim,bool OwningThreadMatches,
        bool SourcePublished,int DescriptorCount,int RangeResultCount,int OriginalPointCount,
        int PrefixPointCount,int ProcessedWordCount,uint Word0,uint Word1,uint Word2);
    internal sealed class Source13A0Diagnostic
    {
        private readonly LegacyMathAkOriginalInputContext owner;
        private readonly int issuerThread;
        private int snapshots;
        private Source13A0Diagnostic(LegacyMathAkOriginalInputContext owner)
        {
            owner.Live();this.owner=owner;issuerThread=Environment.CurrentManagedThreadId;
        }
        //Only the fresh factory has an unexposed live Context; completed historical
        //contexts fail Live, and this view never returns its Context reference.
        internal static Source13A0Diagnostic Issue(LegacyMathAkOriginalInputContext owner)
        {owner.Live();return new Source13A0Diagnostic(owner);}
        internal Source13A0Snapshot Snapshot()
        {
            //Foreign-thread refusals do not consume the four-success budget.
            if(Environment.CurrentManagedThreadId!=issuerThread||owner.owningThread!=issuerThread)
                throw new InvalidOperationException("Source13A0 diagnostic issuer thread mismatch.");
            if(owner.claim!=1||owner.phase is not(2 or 3 or -1)||owner.prefix is null||
                !ReferenceEquals(owner.prefix.OriginalInput,owner.input)||
                !ReferenceEquals(owner.documentIdentity,owner.input.DocumentIdentity))
                throw new InvalidOperationException("Source13A0 diagnostic live lineage mismatch.");
            if(snapshots>=4)throw new NotSupportedException("Source13A0 diagnostic snapshot reservation exhausted.");
            if(owner.processed.Length is<1 or>3)throw new InvalidOperationException("Source13A0 diagnostic word count mismatch.");
            var result=new Source13A0Snapshot(owner.phase,owner.claim,true,owner.source13A0 is not null,
                owner.descriptorCount,owner.resultCount,owner.input.PointCount,owner.prefix.PointCount,
                owner.processed.Length,owner.processed[0],owner.processed.Length>1?owner.processed[1]:0,
                owner.processed.Length>2?owner.processed[2]:0);
            snapshots++;return result;
        }
    }
    private static void Notify13A0(Action<Source13A0Checkpoint,Source13A0Diagnostic>? observer,
        Source13A0Diagnostic? diagnostic,Source13A0Checkpoint checkpoint,CancellationToken token)
    {
        if(observer is not null)observer(checkpoint,diagnostic!);
        token.ThrowIfCancellationRequested();
    }

    private sealed class Source13A0Budget
    {
        private long reserved,maximumOutput,countNodes,fillNodes;
        internal void Reserve(long bytes)
        {
            if(bytes<0||bytes>Source13A0LogicalPayloadLimit-reserved)
                throw new NotSupportedException("Original-source logical operation reservation exhausted.");
            reserved+=bytes;
        }
        internal long CountedOutput {get;private set;}
        internal void BeginCounting()=>maximumOutput=(Source13A0LogicalPayloadLimit-reserved)/12;
        internal void CountNode(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(countNodes>=checked(2*maximumOutput+12))
                throw new NotSupportedException("Original-source count traversal reservation exhausted.");
            countNodes++;
        }
        internal void CountOutput()
        {
            if(CountedOutput>=maximumOutput)throw new NotSupportedException("Original-source exact output exceeds remaining reservation.");
            CountedOutput++;
        }
        internal void ReserveOutput()=>Reserve(checked(12*CountedOutput));
        internal void FillNode(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(fillNodes>=countNodes)throw new InvalidOperationException("Spline fill exceeds counted work.");
            fillNodes++;
        }
        internal void CompleteFill()
        {if(fillNodes!=countNodes)throw new InvalidOperationException("Spline count/fill chronology mismatch.");}
    }
    private sealed class Source13A0Frame
    {
        internal readonly Vector2[][] Points;
        internal readonly int[][] OriginalIndices;
        internal readonly int PointCount;
        internal Source13A0Frame(Vector2[][] points,int[][] indices,int count)
        {Points=points;OriginalIndices=indices;PointCount=count;}
    }
    private void Run13A0(Source13A0Budget budget,
        Action<Source13A0Checkpoint,Source13A0Diagnostic>? observer,Source13A0Diagnostic? diagnostic,
        CancellationToken token)
    {
        Live();var policy=input.OriginalOwner.FeatureProfile.OriginalSmoothing;
        int segments=0,orthogonal=0,corners=0;
        for(int s=0;s<input.StrokeCount;s++)
        {
            var row=Row(s,token).Points;
            for(int i=1;i<row.Length;i++)
            {
                token.ThrowIfCancellationRequested();segments++;
                bool vertical=LegacyLineOrientationProjection.Approximately(row[i].X,row[i-1].X);
                bool horizontal=LegacyLineOrientationProjection.Approximately(row[i].Y,row[i-1].Y);
                if(vertical||horizontal)orthogonal++;
                if(i+1<row.Length&&((vertical&&LegacyLineOrientationProjection.Approximately(row[i].Y,row[i+1].Y))||
                    (horizontal&&LegacyLineOrientationProjection.Approximately(row[i].X,row[i+1].X))))corners++;
            }
        }
        float ortho=segments==0?0f:(float)orthogonal/segments;
        float corner=segments<=input.StrokeCount?0f:(float)corners/(segments-input.StrokeCount);
        float weight=policy.Minimum;
        if(policy.Minimum<policy.Maximum&&policy.OrthogonalLow>0&&policy.OrthogonalLow<policy.OrthogonalHigh&&ortho>=policy.OrthogonalLow)
            weight=ortho>policy.OrthogonalHigh?policy.Maximum:
                ((policy.Maximum-policy.Minimum)*(ortho-policy.OrthogonalLow))/(policy.OrthogonalHigh-policy.OrthogonalLow)+policy.Minimum;
        if(policy.Extra>0&&policy.CornerLow>0&&policy.CornerLow<policy.CornerHigh)
        {
            if(corner>policy.CornerHigh)weight+=policy.Extra;
            else if(corner>policy.CornerLow)weight+=((corner-policy.CornerLow)*policy.Extra)/(policy.CornerHigh-policy.CornerLow);
        }
        Finite(weight);float scale=prefix!.EstimatedScale;
        var plans=new Source13A0SplinePlan[input.StrokeCount];int offset=0;
        budget.BeginCounting();
        for(int s=0;s<plans.Length;s++)
        {
            Live();token.ThrowIfCancellationRequested();var row=Row(s,token);row.Validate(prefix,s);
            plans[s]=new Source13A0SplinePlan(this,row,s,offset,weight,scale,policy.FlatnessSquaredFactor,token);
            offset=checked(offset+row.Length);plans[s].Count(budget,token);
        }
        if(offset!=prefix.PointCount)throw new InvalidOperationException("Same current source point header changed.");
        token.ThrowIfCancellationRequested();budget.ReserveOutput();
        Notify13A0(observer,diagnostic,Source13A0Checkpoint.AfterCountAndReservation,token);
        var points=new Vector2[plans.Length][];var indices=new int[plans.Length][];int count=0;
        for(int s=0;s<plans.Length;s++)
        {
            Live();token.ThrowIfCancellationRequested();int q=plans[s].OutputCount;
            points[s]=new Vector2[q];indices[s]=new int[q];
            plans[s].Fill(points[s],indices[s],budget,token);count=checked(count+q);
        }
        budget.CompleteFill();
        if(count!=budget.CountedOutput)throw new InvalidOperationException("Output header differs from exact count.");
        Live();token.ThrowIfCancellationRequested();
        Notify13A0(observer,diagnostic,Source13A0Checkpoint.AfterFillBeforePublication,token);
        source13A0=new Source13A0Frame(points,indices,count);
    }
    internal Vector2[][] Copy13A0Points(CancellationToken token)
    {
        Finished();token.ThrowIfCancellationRequested();var current=source13A0??throw new InvalidOperationException("No13A0 stage in this historical context.");
        var copy=new Vector2[current.Points.Length][];
        for(int i=0;i<copy.Length;i++){token.ThrowIfCancellationRequested();copy[i]=(Vector2[])current.Points[i].Clone();}
        token.ThrowIfCancellationRequested();return copy;
    }
    internal int[][] Copy13A0OriginalIndices(CancellationToken token)
    {
        Finished();token.ThrowIfCancellationRequested();var current=source13A0??throw new InvalidOperationException("No13A0 stage in this historical context.");
        var copy=new int[current.OriginalIndices.Length][];
        for(int i=0;i<copy.Length;i++){token.ThrowIfCancellationRequested();copy[i]=(int[])current.OriginalIndices[i].Clone();}
        token.ThrowIfCancellationRequested();return copy;
    }
}
