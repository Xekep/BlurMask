using System.Runtime.CompilerServices;

namespace InkRecognition.Math;

// Implemented only beside the concrete selected InkOwner/Selection classes.
internal abstract class Math2Mode0CurrentC23
{
    internal abstract object Identity { get; }
    internal abstract object InkIdentity { get; }
    internal abstract object InputC22Identity { get; }
    internal abstract long InputId { get; }
    internal abstract LegacyOwnedRawInkDocument Document { get; }
    internal abstract ReadOnlySpan<LegacyNativeInkPositionRange> Ranges { get; }
    internal abstract LegacyOwnedRawInkIdentityExport IdentityTransforms { get; }
    internal abstract bool FullDomain { get; }
}
internal sealed record Math2Mode0CurrentC23Snapshot(long Reservation,long InputId,bool Complete,bool Failed,bool Released,
    bool InferenceClaimed,bool SourcePrepared,int WorkCount,byte InitialComputeFlag,bool WorkCompleted,int TransformCount,int FreezeCount,
    LegacyNativeInkPositionRange[] C23Ranges,LegacyNativeInkPositionRange[] WorkRanges);
internal enum Math2Mode0CurrentC23Stage { Claimed, Selected, Work, Complete }

internal sealed partial class Math2Mode0PriorC23Preparation:IDisposable
{
    internal const long DefaultBudget=16L*1024*1024;
    private sealed class Claim {internal int Used;}
    private static readonly ConditionalWeakTable<Math2Mode0PriorSelectedScope,Claim> Claims=new();
    private readonly object gate=new();
    private readonly Math2Mode0PriorSelectedScope scope;
    private readonly Math2Mode0PriorFreezePreparation freeze;
    private readonly long reservation;
    private Math2Mode0CurrentC23? c23;
    // One concrete owned F0 range copy, not a caller-supplied F0 record array.
    private sealed class Work(Math2Mode0CurrentC23 actual)
    {
        internal readonly Math2Mode0CurrentC23 Selection=actual;
        internal readonly LegacyNativeInkPositionRange[] Ranges=actual.Ranges.ToArray();
        internal readonly LegacyOwnedRawInkIdentityExport Transforms=actual.IdentityTransforms;
        internal readonly byte InitialComputeFlag=1;
        internal bool Completed;
    }
    private Work? work;
    private bool complete,failed,released,inferenceClaimed,sourcePrepared;
    private object? actualSource;
    private NnBorrow? activeNnBorrow;
    private Action? activeLiveCheck;
    private Math2Mode0PriorC23Preparation(Math2Mode0PriorSelectedScope scope,Math2Mode0PriorFreezePreparation freeze,long reservation)
    {this.scope=scope;this.freeze=freeze;this.reservation=reservation;}
    internal static Math2Mode0PriorC23Preparation Create(Math2Mode0PriorSelectedScope scope,Math2Mode0PriorFreezePreparation freeze,CancellationToken token=default)
        =>CreateDiagnostic(scope,freeze,DefaultBudget,null,token);
    internal static Math2Mode0PriorC23Preparation CreateDiagnostic(Math2Mode0PriorSelectedScope scope,Math2Mode0PriorFreezePreparation freeze,long budget,
        Action<Math2Mode0CurrentC23Stage,Math2Mode0PriorC23Preparation>? observe,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(scope);ArgumentNullException.ThrowIfNull(freeze);token.ThrowIfCancellationRequested();
        if(budget<0||budget>DefaultBudget)throw new ArgumentOutOfRangeException(nameof(budget));
        Math2Mode0PriorC23Preparation? target=null;
        try
        {
            return scope.WithFreezeInput(input=>freeze.WithZeroRows(scope,input,freezeReservation=>
            {
                long planned=input.PlanC23(freezeReservation,budget,token);
                if(Interlocked.CompareExchange(ref Claims.GetValue(scope,_=>new()).Used,1,0)!=0)throw new InvalidOperationException("Current C23 scope already consumed.");
                target=new(scope,freeze,planned);
                lock(target.gate)
                {
                    void Stage(Math2Mode0CurrentC23Stage stage)
                    {observe?.Invoke(stage,target);ObjectDisposedException.ThrowIf(target.released,target);input.Check();token.ThrowIfCancellationRequested();}
                    Stage(Math2Mode0CurrentC23Stage.Claimed);
                    target.c23=input.SelectCurrentInput(token);Stage(Math2Mode0CurrentC23Stage.Selected);
                    if(!target.c23.FullDomain||target.c23.Ranges.IsEmpty)throw new NotSupportedException("Current NN route requires the actual complete ordinary INPUT C23.");
                    if(target.c23.IdentityTransforms.NativeNonidentityTransformCount!=0)throw new NotSupportedException("Real nonidentity transform preparation is not ported.");
                    target.work=new(target.c23);Stage(Math2Mode0CurrentC23Stage.Work);
                    Stage(Math2Mode0CurrentC23Stage.Complete);target.complete=true;return target;
                }
            }),token);
        }
        catch{if(target is not null){lock(target.gate){target.failed=true;if(observe is null)target.Dispose();}}throw;}
    }
    private T WithLive<T>(Func<T> consume,CancellationToken token=default)
        =>scope.WithFreezeInput(input=>freeze.WithZeroRows(scope,input,_=>
        {lock(gate)
        {
            void Check(){input.Check();ObjectDisposedException.ThrowIf(released,this);if(!complete||failed||c23 is null||work is null)throw new InvalidOperationException("Live completed current C23/work required.");token.ThrowIfCancellationRequested();}
            Check();if(activeLiveCheck is not null)throw new InvalidOperationException("Exclusive current C23 borrow required.");activeLiveCheck=Check;
            try{var result=consume();Check();return result;}finally{activeLiveCheck=null;}
        }}),token);
    internal Math2Mode0CurrentC23Snapshot CopySnapshot()
    {lock(gate)return new(reservation,c23?.InputId??-1,complete,failed,released,inferenceClaimed,sourcePrepared,work is null?0:1,work?.InitialComputeFlag??0,work?.Completed??false,work?.Transforms.NativeNonidentityTransformCount??0,0,c23?.Ranges.ToArray()??[],work?.Ranges.ToArray()??[]);}
    internal Math2Mode0CurrentC23Snapshot CopyResult(CancellationToken token=default)=>WithLive(CopySnapshot,token);
    internal object SelectedInkIdentity=>WithLive(()=>c23!.InkIdentity);
    internal object C23Identity=>WithLive(()=>c23!.Identity);
    internal object InputC22Identity=>WithLive(()=>c23!.InputC22Identity);
    internal LegacyOwnedRawInkDocument Document=>WithLive(()=>c23!.Document);

    // The same private current C23/work is retained BEFORE feature/tensor/NN
    //preparation. Old OriginalFieldSource.Binding cannot be reset or substituted.
    internal sealed class NnBorrow
    {
        private readonly Math2Mode0PriorC23Preparation owner;
        private bool rawClaimed;
        private NnBorrow(Math2Mode0PriorC23Preparation owner){this.owner=owner;}
        internal static NnBorrow Issue(Math2Mode0PriorC23Preparation owner)
        {if(!Monitor.IsEntered(owner.gate)||owner.activeNnBorrow is not null)throw new InvalidOperationException();return new(owner);}
        internal void Check()
        {if(!Monitor.IsEntered(owner.gate)||!ReferenceEquals(owner.activeNnBorrow,this)||owner.released||owner.failed||!owner.complete||!owner.inferenceClaimed)throw new InvalidOperationException("Actual active prior NN borrow required.");(owner.activeLiveCheck??throw new InvalidOperationException("Current ancestor borrow required."))();}
        internal void ClaimRawSource(){Check();if(rawClaimed)throw new InvalidOperationException("One raw NN source per C23 claim.");rawClaimed=true;}
        internal LegacyOwnedRawInkDocument Document {get{Check();return owner.c23!.Document;}}
        internal long Reservation {get{Check();return owner.reservation;}}
        internal void Commit(object source)
        {Check();if(!rawClaimed||owner.actualSource is not null)throw new InvalidOperationException();owner.actualSource=source;owner.sourcePrepared=true;}
    }
    internal T WithNewNn<T>(Func<NnBorrow,T> prepare,CancellationToken token)
    {
        try{return WithLive(()=>
        {
            token.ThrowIfCancellationRequested();if(inferenceClaimed)throw new InvalidOperationException("Current C23 inference already claimed.");
            long raw=c23!.Document.Strokes.Sum(s=>(long)s.Points.Count);
            long upper=checked(reservation+raw*8+MathPreprocessor.MaxFrames*8L+c23.Document.ObjectCount*128L+32);
            if(upper>Math2Mode0SourceBoundOutput.DefaultPayloadBudget)throw new NotSupportedException("Combined current-C23/raw-position envelope exceeds budget before inference claim.");
            inferenceClaimed=true;var input=NnBorrow.Issue(this);activeNnBorrow=input;
            try{var result=prepare(input);input.Check();if(!sourcePrepared)throw new InvalidOperationException("Prior NN preparation did not commit its real source.");return result;}
            finally{activeNnBorrow=null;}
        },token);}
        catch{lock(gate){if(inferenceClaimed&&!sourcePrepared)failed=true;}throw;}
    }
    internal T WithPreparedSource<T>(object expected,Func<T> consume,CancellationToken token=default)
        =>WithLive(()=>{if(!sourcePrepared||!ReferenceEquals(actualSource,expected))throw new ArgumentException("Exact current C23 NN source required.");return consume();},token);
    internal void CheckRunningSource(object expected)
    {if(!Monitor.IsEntered(gate)||!ReferenceEquals(actualSource,expected))throw new ArgumentException();(activeLiveCheck??throw new InvalidOperationException("Current ancestor borrow required."))();}
    internal void MarkWorkComplete(object expected)
    {if(!Monitor.IsEntered(gate)||!ReferenceEquals(actualSource,expected)||work is null)throw new ArgumentException();work.Completed=true;}
    internal void FailPreparedSource(object expected)
    {lock(gate){if(ReferenceEquals(actualSource,expected))failed=true;}}
    public void Dispose(){lock(gate)released=true;}
}

internal sealed partial class Math2Mode0PriorSelectedScope
{
    internal sealed partial class FreezeBorrow
    {
        internal bool IsFor(Math2Mode0PriorSelectedScope expected){Check();return ReferenceEquals(owner,expected);}
        internal long PlanC23(long freezeReservation,long budget,CancellationToken token)
        {Check();token.ThrowIfCancellationRequested();long planned=checked(owner.reservation+freezeReservation+8192+owner.selectedInk.MaximumRangeCount*64L+owner.selectedInk.Document.ObjectCount*128L);if(planned>budget)throw new NotSupportedException("Combined current selection/work payload exceeds budget before claim.");return planned;}
        internal Math2Mode0CurrentC23 SelectCurrentInput(CancellationToken token)
        {
            Check();var field=owner.selectedArena.Nodes.Single(n=>n.Parent==0&&n.Kind==3&&n.Name.Text=="mathField");
            var input=owner.selectedArena.Children(field.Id).FirstOrDefault(n=>n.Kind==3&&n.Name.Text=="input")??throw new InvalidOperationException("Current field INPUT missing.");
            var tag=owner.selectedArena.Children(input.Id).FirstOrDefault(n=>n.Kind==0&&n.Name.Text=="tag");
            if(tag?.Tag!=4||tag.Number is not long id||id==-1)throw new NotSupportedException("Actual current typed INPUT tag required.");
            var selection=owner.selectedInk.SelectInput(id,token);
            if(!ReferenceEquals(selection.InkIdentity,owner.selectedInk.Identity)||!ReferenceEquals(selection.InputC22Identity,owner.selectedInk.SelectionIdentity(id)))throw new InvalidOperationException("Current C23 lost its same selected Ink/INPUT.");
            return selection;
        }
    }
}
internal sealed partial class Math2Mode0PriorFreezePreparation
{
    internal T WithZeroRows<T>(Math2Mode0PriorSelectedScope actual,Math2Mode0PriorSelectedScope.FreezeBorrow borrow,Func<long,T> consume)
    {
        borrow.Check();if(!borrow.IsFor(actual)||!ReferenceEquals(scope,actual))throw new ArgumentException("Exact owning freeze preparation/current scope required.");
        lock(gate)
        {
            ObjectDisposedException.ThrowIf(released,this);if(!complete||failed)throw new InvalidOperationException("Complete live freeze preparation required.");
            if(rows.Count!=0)throw new NotSupportedException("Nonzero freeze requires actual0290C0 scale and02A830 terminal/transform owners.");
            var result=consume(reservation);borrow.Check();ObjectDisposedException.ThrowIf(released,this);return result;
        }
    }
}
internal sealed partial class Math2Mode0OriginalMathFieldSource
{
    private sealed class CurrentC23:Math2Mode0CurrentC23
    {
        private readonly InkOwner ink;
        private readonly RegistryEntry input;
        private readonly LegacyNativeInkPositionRange[] ranges;
        private readonly LegacyOwnedRawInkIdentityExport transforms;
        private readonly bool full;
        internal CurrentC23(InkOwner ink,RegistryEntry input,Math2Mode0OriginalMathFieldSource source,CancellationToken token)
        {
            this.ink=ink;this.input=input;token.ThrowIfCancellationRequested();
            // Native starts null/0; pointer-identical null/0 is a no-op. The
            //admitted full current INPUT takes the copy+actual intersection path.
            ranges=input.C22.Storage is null&&input.C22.Ranges.Length==0?[]:
                LegacyOwnedRawInkRangeOperations.Prepare(ink.Document).Intersect(source.domain,input.C22.Ranges,token);
            full=ranges.AsSpan().SequenceEqual(source.domain);
            transforms=source.CurrentView.ExportIdentityTransforms(ranges,token);
        }
        internal override object Identity=>this;
        internal override object InkIdentity=>ink;
        internal override object InputC22Identity=>input.C22;
        internal override long InputId=>input.Id;
        internal override LegacyOwnedRawInkDocument Document=>ink.Document;
        internal override ReadOnlySpan<LegacyNativeInkPositionRange> Ranges=>ranges;
        internal override LegacyOwnedRawInkIdentityExport IdentityTransforms=>transforms;
        internal override bool FullDomain=>full;
    }
}
