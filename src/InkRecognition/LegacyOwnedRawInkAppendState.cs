namespace InkRecognition;

// Portable ordinary C20 append ownership. No SDK page/history/observer or C51
// registration is acquired here. Existing immutable fresh-view guards remain.
internal sealed partial class LegacyOwnedRawInkAppendState : IDisposable
{
    internal const long DefaultClonePayloadBudget = 2L * 1024 * 1024;
    private readonly object gate = new();
    private readonly LegacyFreshRawInkCurrentView initial;
    private Frame current;
    private Borrow? borrowing;
    private Pending? pending;
    private bool disposed, operation;

    // An arbitrary internally constructed token still cannot equal the private
    // committed receipt. It contains no writable owner, document or arrays.
    internal sealed class Revision { internal Revision() { } }
    private sealed record Frame(Revision Receipt,LegacyOwnedRawInkDocument Projection,
        LegacyOwnedRawInkCurve[] Curves,LegacyOwnedRawInkCurrentViewEntry[] Views,
        int Points,int Sequence,Revision? Parent,long ClonePayloadReservation);

    internal sealed class PreparedAppend { internal PreparedAppend() { } }
    private sealed record Pending(PreparedAppend Receipt,Revision Parent,InkPoint[] Geometry,
        int TotalPoints,long Reservation);

    internal sealed partial class Borrow
    {
        private readonly LegacyOwnedRawInkAppendState owner;
        private readonly Revision revision;
        private readonly CancellationToken token;
        internal bool Active;
        internal Borrow(LegacyOwnedRawInkAppendState owner,Revision revision,CancellationToken token)
        {this.owner=owner;this.revision=revision;this.token=token;Active=true;}
        private void Check()
        {
            token.ThrowIfCancellationRequested();
            if(!Monitor.IsEntered(owner.gate)||!Active||!ReferenceEquals(owner.borrowing,this))
                throw new InvalidOperationException("Current Ink borrow escaped its owning callback.");
            owner.CheckRevision(revision);
        }
        internal int Count { get { Check(); return owner.current.Curves.Length; } }
        internal int Sequence { get { Check(); return owner.current.Sequence; } }
        internal LegacyOwnedRawInkDocument Document { get { Check(); return owner.current.Projection; } }
        internal long ClonePayloadReservation { get { Check(); return owner.current.ClonePayloadReservation; } }
        internal LegacyOwnedRawInkCurve Curve(int index) { Check(); return owner.current.Curves[index]; }
        internal LegacyOwnedRawInkCurrentViewEntry View(int index) { Check(); return owner.current.Views[index]; }
        internal void Validate() => Check();
    }

    private LegacyOwnedRawInkAppendState(LegacyFreshRawInkCurrentView actual,int points)
    {
        initial=actual;
        current=new(new(),actual.Document,actual.Curves.ToArray(),actual.Views.ToArray(),points,0,null,0);
    }

    internal static LegacyOwnedRawInkAppendState Adopt(LegacyFreshRawInkCurrentView actual,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actual);token.ThrowIfCancellationRequested();
        int count=actual.Document.ObjectCount,points=0;
        if(count is <1 or >256||actual.Curves.Count!=count||actual.Views.Count!=count)
            throw new NotSupportedException("Complete admitted fresh ordinary Ink required.");
        for(int i=0;i<count;i++)
        {
            token.ThrowIfCancellationRequested();var curve=actual.Curves[i];var view=actual.Views[i];
            var descriptor=view.Descriptor;int n=actual.Document.Strokes[i].Points.Count;
            if(!ReferenceEquals(curve.Document,actual.Document)||curve.OriginalSourceIndex!=i||
                !ReferenceEquals(view.Curve,curve)||view.CurrentViewIndex!=i||n!=curve.PointCount||
                descriptor.SourceIndex!=(uint)i||descriptor.PointCount!=n||descriptor.FirstPoint!=0||
                descriptor.LastPoint!=n-1||descriptor.FirstFraction!=0||descriptor.LastFraction!=0||
                !descriptor.FirstClosed||!descriptor.LastClosed||descriptor.IsKnownText||n is <1 or >4096)
                throw new ArgumentException("Exact same-document full C49/current-view lineage required.");
            points=checked(points+n);if(points>4096)throw new NotSupportedException("Bounded raw points required.");
        }
        token.ThrowIfCancellationRequested();return new(actual,points);
    }

    private void CheckRevision(Revision revision)
    {
        ObjectDisposedException.ThrowIf(disposed,this);
        if(!ReferenceEquals(current.Receipt,revision)||!ReferenceEquals(current.Curves[0],initial.Curves[0]))
            throw new InvalidOperationException("Exact privately committed current Ink revision required.");
    }
    private void Enter()
    {
        ObjectDisposedException.ThrowIf(disposed,this);
        if(operation)throw new InvalidOperationException("Reentrant current Ink operation.");
        operation=true;
    }
    internal Revision Capture()
    {lock(gate){ObjectDisposedException.ThrowIf(disposed,this);if(operation)throw new InvalidOperationException("Reentrant current Ink capture.");return current.Receipt;}}

    internal T WithCurrent<T>(Revision revision,Func<Borrow,T> action,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(revision);ArgumentNullException.ThrowIfNull(action);
        lock(gate)
        {
            Enter();Borrow? lease=null;
            try
            {
                token.ThrowIfCancellationRequested();CheckRevision(revision);
                borrowing=lease=new(this,revision,token);T value=action(lease);lease.Validate();return value;
            }
            finally {if(lease is not null)lease.Active=false;borrowing=null;operation=false;}
        }
    }

    // Caller geometry is copied into a NEW library-owned C49; this does not
    // claim retention of the caller's InkStroke identity or its side effects.
    internal PreparedAppend PrepareAppend(Revision revision,InkStroke inputGeometry,
        long clonePayloadBudget=DefaultClonePayloadBudget,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(revision);ArgumentNullException.ThrowIfNull(inputGeometry);
        if(clonePayloadBudget<0||clonePayloadBudget>DefaultClonePayloadBudget)
            throw new ArgumentOutOfRangeException(nameof(clonePayloadBudget));
        lock(gate)
        {
            Enter();
            try
            {
                token.ThrowIfCancellationRequested();CheckRevision(revision);
                if(pending is not null)throw new InvalidOperationException("One prepared append at a time.");
                var frame=current;int count=frame.Curves.Length;
                if(count==256)throw new NotSupportedException("Raw object limit before point read.");
                var supplied=inputGeometry.Points??throw new ArgumentException("Actual raw XY geometry required.");
                int added=supplied.Count;
                token.ThrowIfCancellationRequested();CheckRevision(revision);
                if(added is <1 or >4096||added>4096-frame.Points)
                    throw new NotSupportedException("Raw point limit before point read.");
                int points=checked(frame.Points+added),nextCount=count+1;
                // Conservative cloned payload reservation, not total GC/RSS.
                long reserve=checked(points*24L+nextCount*256L+512);
                if(reserve>clonePayloadBudget)throw new NotSupportedException("Clone payload budget before point read/allocation.");
                var appended=new InkPoint[added];
                for(int i=0;i<added;i++)
                {
                    token.ThrowIfCancellationRequested();var p=supplied[i];
                    token.ThrowIfCancellationRequested();CheckRevision(revision);
                    if(!float.IsFinite(p.X)||!float.IsFinite(p.Y)||MathF.Abs(p.X)>1_000_000||MathF.Abs(p.Y)>1_000_000)
                        throw new ArgumentException("Finite bounded raw XY points required.");
                    appended[i]=p;
                }
                var staged=new Pending(new(),revision,appended,points,reserve);
                token.ThrowIfCancellationRequested();CheckRevision(revision);pending=staged;return staged.Receipt;
            }
            finally {operation=false;}
        }
    }
    private Pending RequirePending(PreparedAppend receipt)
    {
        ObjectDisposedException.ThrowIf(disposed,this);
        if(pending is null||!ReferenceEquals(pending.Receipt,receipt))
            throw new InvalidOperationException("Exact privately prepared append required.");
        CheckRevision(pending.Parent);return pending;
    }
    internal int PreparedPointCount(PreparedAppend receipt)
    {lock(gate){if(operation)throw new InvalidOperationException("Reentrant prepared append read.");return RequirePending(receipt).Geometry.Length;}}
    internal void AbandonPrepared(PreparedAppend receipt)
    {
        lock(gate)
        {
            if(operation)throw new InvalidOperationException("Reentrant prepared append release.");
            if(pending is null)return;
            if(!ReferenceEquals(pending.Receipt,receipt))throw new InvalidOperationException("Foreign prepared append release.");
            pending=null;
        }
    }
    internal Revision CommitPrepared(PreparedAppend receipt,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        lock(gate)
        {
            Enter();Pending? staged=null;
            try
            {
                staged=RequirePending(receipt);token.ThrowIfCancellationRequested();
                var frame=current;int count=frame.Curves.Length,nextCount=count+1;
                // Feature projection is minted ONLY here from retained prefix
                // geometry plus this copied append. No foreign Document ingress.
                var strokes=new InkStroke[nextCount];
                for(int i=0;i<count;i++){token.ThrowIfCancellationRequested();strokes[i]=frame.Projection.Strokes[i];}
                strokes[count]=new(Array.AsReadOnly(staged.Geometry));
                var projection=LegacyOwnedRawInkDocument.Create(strokes,token);
                var curves=new LegacyOwnedRawInkCurve[nextCount];var views=new LegacyOwnedRawInkCurrentViewEntry[nextCount];
                for(int i=0;i<count;i++){token.ThrowIfCancellationRequested();curves[i]=frame.Curves[i];views[i]=frame.Views[i];}
                var descriptor=projection.CopySelectedInputs()[count] with {LastPoint=staged.Geometry.Length-1};
                curves[count]=new(projection,count);views[count]=new(curves[count],count,descriptor);
                var successor=new Frame(new(),projection,curves,views,staged.TotalPoints,checked(frame.Sequence+1),staged.Parent,staged.Reservation);
                // No callback, cancellation check, allocation or fallible work
                // follows the single raw commit. Dependent C51/INPUT ownership
                // must be preflighted separately; no A8 rollback is implied.
                token.ThrowIfCancellationRequested();CheckRevision(staged.Parent);current=successor;return successor.Receipt;
            }
            finally {if(staged is not null&&ReferenceEquals(pending,staged))pending=null;operation=false;}
        }
    }
    internal Revision Append(Revision revision,InkStroke inputGeometry,
        long clonePayloadBudget=DefaultClonePayloadBudget,CancellationToken token=default)
    {
        lock(gate)
        {
            // Keep successful commit and its idempotent cleanup indivisible
            // from a different thread preparing the next legitimate append.
            var receipt=PrepareAppend(revision,inputGeometry,clonePayloadBudget,token);
            try{return CommitPrepared(receipt,token);}finally{AbandonPrepared(receipt);}
        }
    }
    public void Dispose(){lock(gate){disposed=true;pending=null;if(borrowing is not null)borrowing.Active=false;}}
}
