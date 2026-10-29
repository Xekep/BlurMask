using System.Runtime.CompilerServices;
using System.Text;
namespace InkRecognition.Math;

internal sealed partial class Math2Mode0OriginalArena
{
    // This allocator confers no source authority; the private owning borrow
    // supplies actual candidate fields and authenticates every source node.
    internal Node NewDiffPayload(byte kind,string name,int origin,byte tag,string? text,long? number,int parent)
    {var n=New(kind,name,0,origin,tag,text,number,null,null);if(parent!=0)Link(parent,n);return n;}
}
internal sealed partial class Math2Mode0PriorCandidatePreparation
{
    private DiffBorrow? activeDiffBorrow;
    internal sealed partial class DiffBorrow
    {
        private readonly Math2Mode0PriorCandidatePreparation owner;
        private readonly Action check;
        private DiffBorrow(Math2Mode0PriorCandidatePreparation owner,Action check){this.owner=owner;this.check=check;}
        internal static DiffBorrow Issue(Math2Mode0PriorCandidatePreparation owner,Action check)
        {if(!Monitor.IsEntered(owner.gate)||owner.activeDiffBorrow is not null)throw new InvalidOperationException("Exclusive actual candidate gate required.");check();return new(owner,check);}
        internal void Check(){if(!Monitor.IsEntered(owner.gate)||!ReferenceEquals(owner.activeDiffBorrow,this))throw new InvalidOperationException("Actual synchronous pre-diff owner borrow required.");check();}
        internal object Identity{get{Check();return owner;}}
        internal long Plan(long budget,Math2DomDiffWork work)
        {
            Check();work.Visit();long nodes=checked(owner.candidate.Count+owner.temporary!.Nodes.Count);
            long cells=checked((nodes+1)*(nodes+1));if(nodes>1023||cells>Math2DomDiffWork.MaximumCells)throw new NotSupportedException("Bounded whole default Math diff domain required before payload copies.");
            long bytes=0;foreach(var n in owner.candidate){work.Visit(checked(1L+(n.Text?.Length??0)*3L));bytes=checked(bytes+Encoding.UTF8.GetByteCount(n.Text??""));}foreach(var n in owner.temporary.Nodes){work.Visit(checked(1L+(n.Text?.Length??0)*3L));bytes=checked(bytes+Encoding.UTF8.GetByteCount(n.Text??""));}
            long plan=checked(owner.reservation+32768+nodes*2048+cells*4+bytes*8);if(plan>budget)throw new NotSupportedException("Whole actual diff/payload/action envelope exceeds budget before claim.");return plan;
        }
        internal void Views(Math2DomDiffWork work,out Math2DomDiffNode oldRoot,out Math2DomDiffNode nextRoot)
        {
            Check();var temp=owner.temporary!;var oldMap=new Dictionary<int,Math2DomDiffNode>();var newMap=new Dictionary<int,Math2DomDiffNode>();
            Math2DomDiffNode Old(Math2Mode0OriginalArena.Node n)
            {
                Check();work.Visit();if(n.Uri is not null||n.Prefix is not null)throw new NotSupportedException("The actual library default Math scope has no namespace policy.");
                if(oldMap.TryGetValue(n.Id,out var existing))return existing;
                work.Visit(checked(16L+(n.Text?.Length??0)*3L));var value=Math2DomDiffScalar.From(n.Kind is 0 or 1?n.Tag:(byte)0,n.Text,n.Number);
                var result=new Math2DomDiffNode(temp,n,n.Id,n.Kind,n.Name,value);oldMap.Add(n.Id,result);
                foreach(var child in temp.Children(n.Id)){work.Visit();var c=Old(child);c.Parent=result;(child.Kind==0?result.Attributes:result.Children).Add(c);}return result;
            }
            Math2DomDiffNode Next(Node n)
            {
                Check();work.Visit();if(newMap.TryGetValue(n.Id,out var existing))return existing;
                work.Visit(checked(16L+(n.Text?.Length??0)*3L));var value=Math2DomDiffScalar.From(n.Kind is 0 or 1?n.Tag:(byte)0,n.Text,n.Number);
                var result=new Math2DomDiffNode(owner.candidate,n,n.Id,n.Kind,n.Name,value);newMap.Add(n.Id,result);
                foreach(var child in owner.candidate){work.Visit();if(child.Parent!=n.Id)continue;var c=Next(child);c.Parent=result;(child.Kind==0?result.Attributes:result.Children).Add(c);}return result;
            }
            work.Visit(temp.Children(1).Count+owner.candidate.Count);var old=temp.Children(1).Single(n=>n.Kind==3&&n.Name.Text=="mathResult");
            var next=owner.candidate.Single(n=>n.Kind==3&&n.Name.Text=="mathResult"&&owner.candidate[n.Parent-1].Kind==4);
            oldRoot=Old(old);nextRoot=Next(next);
            if(!ReferenceEquals(oldRoot.Name,nextRoot.Name)||!ReferenceEquals(oldRoot.Name.Owner,owner.names))throw new InvalidOperationException("Actual shared canonical Math result roots required.");
            Check();
        }
        internal Math2DomDiffNode ClonePayload(Math2DomDiffNode source,Math2DomDiffWork work)
        {
            Check();work.Visit(owner.candidate.Count);if(!ReferenceEquals(source.Writer,owner.candidate)||source.Identity is not Node actual||!owner.candidate.Contains(actual))throw new InvalidOperationException("Only this actual candidate may supply an insertion payload.");
            Math2DomDiffNode Clone(Node n,int parent)
            {
                Check();work.Visit(checked(128L+(n.Text?.Length??0)*3L));
                var raw=owner.temporary!.NewDiffPayload(n.Kind,n.Name.Text,n.Id,n.Tag,n.Text,n.Number,parent);
                var node=new Math2DomDiffNode(owner.temporary,raw,raw.Id,raw.Kind,raw.Name,Math2DomDiffScalar.From(raw.Kind is 0 or 1?raw.Tag:(byte)0,raw.Text,raw.Number));
                foreach(var child in owner.candidate){work.Visit();if(child.Parent!=n.Id||child.Kind!=0)continue;var copy=Clone(child,raw.Id);copy.Parent=node;node.Attributes.Add(copy);}
                foreach(var child in owner.candidate){work.Visit();if(child.Parent!=n.Id||child.Kind==0)continue;var copy=Clone(child,raw.Id);copy.Parent=node;node.Children.Add(copy);}
                return node;
            }
            return Clone(actual,0);
        }
        private sealed class PendingView(Math2Mode0PriorCandidatePreparation owner):IReadOnlyList<Math2DomCb8Pending>
        {
            public int Count=>owner.pending.Count;
            public Math2DomCb8Pending this[int i]{get{var p=owner.pending[owner.pending.Count-1-i];return new(p.Id,p.Name,p.Selection);}}
            public IEnumerator<Math2DomCb8Pending> GetEnumerator(){for(int i=0;i<Count;i++)yield return this[i];}
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()=>GetEnumerator();
        }
        internal bool MatchProduced(Math2DomDiffAction action,Math2DomDiffWork work)
        {
            Check();work.Visit(64);var temp=owner.temporary!;
            if(!ReferenceEquals(action.NodeAt18.Writer,temp)||action.NodeAt18.Identity is not Math2Mode0OriginalArena.Node old||!ReferenceEquals(temp.Nodes[old.Id-1],old))throw new InvalidOperationException("Actual produced action must retain this OLD writer node.");
            var tag=owner.names.Intern("tag");
            if(action.Operation==7&&action.NodeAt18.Kind==0&&ReferenceEquals(action.NodeAt18.Name,tag))
            {
                int maximum=0;foreach(var p in owner.pending){work.Visit();if(p.Selection is not null)maximum=System.Math.Max(maximum,owner.third!.CountRanges(p.Selection));}
                foreach(long id in owner.oldTags){work.Visit();var selection=owner.third!.FindOldSelection(id);if(selection is not null)maximum=System.Math.Max(maximum,owner.third.CountRanges(selection));}
                work.Visit(checked(4096L+owner.pending.Count*64L+maximum*32L));
            }
            // Values come from E7760-style produced scalar snapshots; this
            // adapter has no caller opcode/tag/old-ID/selection ingress.
            bool matched=Math2DomCb8EditMatcher.Matches(action.Operation,action.NodeAt18.Kind,
                action.NodeAt18.Kind==0?action.NodeAt18.Name:null,action.NodeAt18.Kind==3?action.NodeAt18.Name:null,tag,
                action.OldValue is null?default:action.OldValue.View(),action.NewValue is null?default:action.NewValue.View(),
                new PendingView(owner),owner.third!,token:work.Token);
            Check();return matched;
        }
    }
    internal T WithDiffInput<T>(Func<DiffBorrow,T> consume,CancellationToken token)
        =>source.WithCandidateInput((input,copy)=>
        {
            lock(gate)
            {
                void Check(){input.Check();ObjectDisposedException.ThrowIf(released,this);if(failed||!complete||!ready||!ReferenceEquals(copy,actualCopy)||!ReferenceEquals(input.Scope,scope)||!ReferenceEquals(input.CurrentC23Identity,c23)||!ReferenceEquals(input.CurrentInkIdentity,selectedInk)||!ReferenceEquals(input.CurrentInputC22Identity,inputC22)||!ReferenceEquals(input.Names,names))throw new InvalidOperationException("Same ready actual pre-diff cohort required.");token.ThrowIfCancellationRequested();}
                Check();if(activeDiffBorrow is not null)throw new InvalidOperationException("Exclusive current candidate diff borrow required.");var borrow=DiffBorrow.Issue(this,Check);activeDiffBorrow=borrow;
                try{var result=consume(borrow);borrow.Check();return result;}finally{activeDiffBorrow=null;}
            }
        },token);
}
internal enum Math2Mode0DomDiffStage{Claimed,RootsBound,ActionLinked,Produced,Filtered,Completed}
internal sealed record Math2Mode0DomDiffSnapshot(bool Complete,bool Ready,bool Failed,bool Released,long Reservation,long Visits,long MatrixCells,
    int Produced,int Kept,int Filtered,bool HeaderLive,Math2DomDiffActionSnapshot[] Actions);
internal sealed partial class Math2Mode0PriorDomDiff:IDisposable
{
    private sealed class Claim{internal int Used;}
    private static readonly ConditionalWeakTable<Math2Mode0PriorCandidatePreparation,Claim> Claims=new();
    private readonly object gate=new();
    private readonly Math2Mode0PriorCandidatePreparation candidate;
    private readonly long reservation;
    private readonly Math2DomDiffWork work;
    private List<Math2DomDiffAction> kept=[];
    private readonly List<Math2DomDiffActionSnapshot> trace=[];
    private bool complete,ready,failed,released,headerLive=true;
    private int filtered;
    private Math2Mode0PriorDomDiff(Math2Mode0PriorCandidatePreparation candidate,long reservation,Math2DomDiffWork work){this.candidate=candidate;this.reservation=reservation;this.work=work;}
    internal static Math2Mode0PriorDomDiff Create(Math2Mode0PriorCandidatePreparation candidate,CancellationToken token=default)
        =>CreateDiagnostic(candidate,Math2Mode0PriorCandidatePreparation.DefaultBudget,null,token);
    internal static Math2Mode0PriorDomDiff CreateDiagnostic(Math2Mode0PriorCandidatePreparation candidate,long budget,
        Action<Math2Mode0DomDiffStage,Math2Mode0PriorDomDiff>? observe,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(candidate);token.ThrowIfCancellationRequested();if(budget<0||budget>Math2Mode0PriorCandidatePreparation.DefaultBudget)throw new ArgumentOutOfRangeException(nameof(budget));Math2Mode0PriorDomDiff? target=null;
        try
        {
            var result=candidate.WithDiffInput(input=>
            {
                var work=new Math2DomDiffWork(token);long plan=input.Plan(budget,work);
                if(Interlocked.CompareExchange(ref Claims.GetValue(candidate,_=>new()).Used,1,0)!=0)throw new InvalidOperationException("Actual candidate diff claim already consumed.");
                target=new(candidate,plan,work);lock(target.gate)
                {
                    void Stage(Math2Mode0DomDiffStage stage){observe?.Invoke(stage,target);input.Check();ObjectDisposedException.ThrowIf(target.released,target);token.ThrowIfCancellationRequested();}
                    Stage(Math2Mode0DomDiffStage.Claimed);input.Views(work,out var oldRoot,out var newRoot);Stage(Math2Mode0DomDiffStage.RootsBound);
                    Math2DomDefaultDiff.Produce(oldRoot,newRoot,n=>input.ClonePayload(n,work),work,a=>{target.kept.Add(a);target.trace.Add(a.Snapshot());Stage(Math2Mode0DomDiffStage.ActionLinked);});Stage(Math2Mode0DomDiffStage.Produced);
                    for(int i=0;i<target.kept.Count;)
                    {
                        work.Visit();var action=target.kept[i];bool remove=input.MatchProduced(action,work);
                        if(remove){int at=target.trace.FindIndex(a=>a.Operation==action.Operation&&a.NodeAt18==action.NodeAt18.Id&&a.SourceCandidate==action.SourceCandidate);if(at<0)throw new InvalidOperationException();target.trace[at]=target.trace[at]with{Filtered=true};target.kept.RemoveAt(i);target.filtered++;}else i++;
                        Stage(Math2Mode0DomDiffStage.Filtered);
                    }
                    target.complete=true;Stage(Math2Mode0DomDiffStage.Completed);return target;
                }
            },token);
            lock(result.gate){ObjectDisposedException.ThrowIf(result.released,result);if(result.failed||!result.complete)throw new InvalidOperationException();result.ready=true;}return result;
        }
        catch{if(target is not null){lock(target.gate){target.failed=true;if(observe is null)target.Dispose();}}throw;}
    }
    internal Math2Mode0DomDiffSnapshot CopyResult(CancellationToken token=default)
        =>candidate.WithDiffInput(input=>{lock(gate){input.Check();ObjectDisposedException.ThrowIf(released,this);if(failed||!complete||!ready)throw new InvalidOperationException("Only a ready same-owner produced diff can be observed.");return CopySnapshot();}},token);
    internal Math2Mode0DomDiffSnapshot CopySnapshot()
    {lock(gate)return new(complete,ready,failed,released,reservation,work.Visits,work.Cells,trace.Count,kept.Count,filtered,headerLive,trace.Select(a=>a with{OldStorage=a.OldStorage.ToArray(),NewStorage=a.NewStorage.ToArray(),OldHeap=a.OldHeap.ToArray(),NewHeap=a.NewHeap.ToArray()}).ToArray());}
    public void Dispose(){lock(gate){if(released)return;released=true;headerLive=false;kept.Clear();}}
}
