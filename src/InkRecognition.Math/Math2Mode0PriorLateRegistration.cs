namespace InkRecognition.Math;

internal sealed partial class Math2Mode0OriginalMathFieldSource
{
    private sealed partial class PriorThirdInk
    {
        internal override Math2Mode0PriorCandidateRegistry[] CopyLateRegistry(Math2DomDiffWork work)
        {
            Parent();lock(inkGate)
            {
                Live();int count=ink.Registry.Count;work.Visit(checked(32L+count*32L));
                foreach(var entry in ink.Registry.Values){work.Visit();work.Visit(checked(entry.C22.Ranges.LongLength*4));}
                return CopyCandidateRegistry();
            }
        }
        internal override void PrepareLateRegistration(object selection,Math2DomDiffWork work)
        {
            Parent();lock(inkGate)
            {
                Live();var actual=Owned(selection);work.Visit();
                // Existing Register scans canonical C22 values. Reserve every
                // comparison before it executes; this does not charge an emit.
                work.Visit(checked(64L+ink.Registry.Count*(4L+actual.Ranges.LongLength*2)));
            }
        }
    }
}
internal sealed record Math2Mode0LateData(long Next,Math2Mode0PriorCandidateRegistry[] Registry,
    Math2Mode0PublishPending[] Pending,Math2Mode0OriginalDomNode[] Temporary);
internal enum Math2Mode0LateStage{Claimed,BeforeRegister,Registered,TagWritten,Resolved,TagCollected,Collected,Removed,Completed}
internal sealed partial class Math2Mode0PriorCandidatePreparation
{
    internal sealed partial class DiffBorrow
    {
        internal long PlanLate(long budget,bool diagnostic,Math2DomDiffWork work)
        {
            Check();var temp=owner.temporary!;work.Visit();
            if(temp.Nodes.Count>65536||owner.pending.Count>65536)throw new NotSupportedException("Bounded actual late CB8 domain required.");
            long ranges=0,bytes=0;
            foreach(var n in temp.Nodes){work.Visit();work.Visit(n.Text?.Length??0);bytes=checked(bytes+(n.Text?.Length??0)*3L);}
            foreach(var p in owner.pending){work.Visit();if(p.Selection is not null)ranges=checked(ranges+owner.third!.CountRanges(p.Selection));}
            work.Visit(ranges);var registry=owner.third!.CopyLateRegistry(work);
            foreach(var r in registry){work.Visit();ranges=checked(ranges+r.Ranges.LongLength);}
            int tagCount=0;foreach(var tag in LiveLateTags(work)){work.Visit();tagCount++;}
            long copies=diagnostic?checked(8L+owner.pending.Count*3L+tagCount*2L+owner.oldTags.Count):1;
            long payload=checked(temp.Nodes.Count*512L+bytes*4+ranges*64+(owner.pending.Count+registry.Length)*512L);
            long plan=checked(owner.reservation+65536+payload*copies);
            if(plan>budget)throw new NotSupportedException("Conservative late clone/prefix payload reservation exceeds budget before claim.");return plan;
        }
        private IEnumerable<Math2Mode0OriginalArena.Node> LiveLateTags(Math2DomDiffWork work)
        {
            Check();var temp=owner.temporary!;var roots=temp.Children(1);work.Visit(checked(1L+roots.Count));
            var root=roots.Single(n=>n.Kind==3&&n.Name.Text=="mathResult");
            IEnumerable<Math2Mode0OriginalArena.Node> Walk(Math2Mode0OriginalArena.Node parent,int depth)
            {
                Check();work.Visit();if(depth>64)throw new NotSupportedException("Bounded live Math descendants required.");
                var children=temp.Children(parent.Id);work.Visit(children.Count);
                foreach(var child in children)
                {
                    if(child.Kind!=3)continue;
                    if(temp.WriterFreed(child)||child.Parent!=parent.Id)throw new InvalidOperationException("Actual live descendant required.");
                    var attributes=temp.Children(child.Id);work.Visit(attributes.Count);
                    foreach(var a in attributes)if(a.Kind==0&&a.Name.Text=="tag")
                    {
                        if(temp.WriterFreed(a)||a.Parent!=child.Id||!ReferenceEquals(a.Name.Owner,owner.names)||a.Tag!=4||a.Number is null)
                            throw new NotSupportedException("Actual live typed INT64 tag required.");
                        yield return a;
                    }
                    foreach(var a in Walk(child,depth+1))yield return a;
                }
            }
            // Native .//*/@tag excludes attributes of the context root itself.
            return Walk(root,0);
        }
        internal (int Tags,int Old) LateCapacities{get{Check();return(owner.temporary!.Nodes.Count,owner.oldTags.Count);}}
        internal void ResolveLate(Math2Mode0PriorDomDiff.TemporaryApplyBorrow caller,Math2Mode0PriorLateRegistration.ActiveBorrow authority,Math2DomDiffWork work,Action<Math2Mode0LateStage,long> progress)
        {
            ArgumentNullException.ThrowIfNull(caller);caller.RequireLateCandidate(this,authority);Check();foreach(var tag in LiveLateTags(work))
            {
                Check();work.Visit();long value=tag.Number!.Value;
                bool resolved=Math2Cb8LateNumericKernel.Resolve(value,owner.pending,p=>p.Id,p=>p.Resolved,
                    (p,id)=>{p.Resolved=id;progress(Math2Mode0LateStage.Registered,id);},
                    p=>
                    {
                        if(p.Selection is null)throw new NotSupportedException("This late owning scope requires actual nonempty pending C22.");
                        owner.third!.PrepareLateRegistration(p.Selection,work);progress(Math2Mode0LateStage.BeforeRegister,p.Id);
                        return owner.third.Register(p.Name,p.Selection);
                    },
                    id=>{work.Visit(32);tag.Tag=4;tag.Text=null;tag.Number=id;progress(Math2Mode0LateStage.TagWritten,id);return true;},work);
                if(!resolved)throw new NotSupportedException("Actual native numeric late callback refused; completed registration/cache/write prefixes retained.");
            }
        }
        internal void CollectLate(Math2Mode0PriorDomDiff.TemporaryApplyBorrow caller,Math2Mode0PriorLateRegistration.ActiveBorrow authority,Math2DomDiffWork work,Action<long> add)
        {ArgumentNullException.ThrowIfNull(caller);caller.RequireLateCandidate(this,authority);Check();foreach(var tag in LiveLateTags(work)){work.Visit();long value=Math2DomValueConversion.CollectedTag(tag.Number!.Value);add(value);}}
        internal void RemoveLate(Math2Mode0PriorDomDiff.TemporaryApplyBorrow caller,Math2Mode0PriorLateRegistration.ActiveBorrow authority,IReadOnlyList<long> current,Math2DomDiffWork work,Action<long> removed)
        {
            ArgumentNullException.ThrowIfNull(caller);caller.RequireLateCandidate(this,authority);Check();foreach(long old in owner.oldTags)
            {
                work.Visit();bool found=false;
                for(int i=0;i<current.Count;i++){work.Visit();if(current[i]==old){found=true;break;}}
                if(found)continue;work.Visit(32);
                if(!owner.third!.RemoveSelection(old))throw new NotSupportedException("Stale removal stopped; prior removed prefix retained.");
                removed(old);
            }
        }
        internal Math2Mode0LateData CopyLateData(Math2DomDiffWork work)
        {
            Check();var registry=owner.third!.CopyLateRegistry(work);var rows=new Math2Mode0PublishPending[owner.pending.Count];
            work.Visit(owner.pending.Count);
            for(int i=0;i<rows.Length;i++)
            {
                var p=owner.pending[i];work.Visit();if(p.Selection is not null)work.Visit(owner.third.CountRanges(p.Selection));
                rows[i]=new(p.Id,p.Name.Text,p.Resolved,p.Selection is not null,p.Selection is null?[]:owner.third.CopySelectionRanges(p.Selection));
            }
            return new(owner.third.SnapshotNext,registry,rows,CopyTemporary(work));
        }
    }
}
internal sealed partial class Math2Mode0PriorDomDiff
{
    internal sealed partial class TemporaryApplyBorrow
    {
        internal long PlanLate(long budget,bool diagnostic,Math2DomDiffWork work){Check();if(owner.headerLive)throw new InvalidOperationException("Actual consumed temporary header required.");return input.PlanLate(budget,diagnostic,work);}
        internal (int Tags,int Old) LateCapacities{get{Check();return input.LateCapacities;}}
        internal void ResolveLate(Math2Mode0PriorLateRegistration.ActiveBorrow authority,Math2DomDiffWork work,Action<Math2Mode0LateStage,long> progress){RequireLateCandidate(input,authority);input.ResolveLate(this,authority,work,progress);}
        internal void CollectLate(Math2Mode0PriorLateRegistration.ActiveBorrow authority,Math2DomDiffWork work,Action<long> add){RequireLateCandidate(input,authority);input.CollectLate(this,authority,work,add);}
        internal void RemoveLate(Math2Mode0PriorLateRegistration.ActiveBorrow authority,IReadOnlyList<long> tags,Math2DomDiffWork work,Action<long> removed){RequireLateCandidate(input,authority);input.RemoveLate(this,authority,tags,work,removed);}
        internal void RequireLateCandidate(Math2Mode0PriorCandidatePreparation.DiffBorrow actual,Math2Mode0PriorLateRegistration.ActiveBorrow authority)
        {ArgumentNullException.ThrowIfNull(authority);Check();authority.Check(this);if(!ReferenceEquals(input,actual))throw new InvalidOperationException("Same actual late candidate required.");}
        internal Math2Mode0LateData CopyLateData(Math2DomDiffWork work){Check();return input.CopyLateData(work);}
    }
}
internal sealed partial class Math2Mode0PriorTemporaryApply
{
    private bool lateClaimed;
    private Math2Mode0PriorLateRegistration? claimedLate;
    internal T WithLateInput<T>(Func<Math2Mode0PriorDomDiff.TemporaryApplyBorrow,T> consume,CancellationToken token=default)
        =>diff.WithTemporaryApplyInput(input=>
        {
            lock(gate)
            {
                CheckLate(input,token);var result=consume(input);CheckLate(input,token);return result;
            }
        },token);
    internal void CheckLate(Math2Mode0PriorDomDiff.TemporaryApplyBorrow input,CancellationToken token=default)
    {
        if(!Monitor.IsEntered(gate))throw new InvalidOperationException("Actual completed temporary parent gate required.");
        input.Check();ObjectDisposedException.ThrowIf(released,this);
        if(failed||!complete||!ready||!headerConsumed||input.HeaderLive||!ReferenceEquals(input.Identity,diff))throw new InvalidOperationException("Same completed actual temporary application required.");token.ThrowIfCancellationRequested();
    }
    internal void ClaimLate(Math2Mode0PriorDomDiff.TemporaryApplyBorrow input,Math2Mode0PriorLateRegistration phase)
    {CheckLate(input);if(lateClaimed||!phase.IsFor(this))throw new InvalidOperationException("Exclusive unclaimed completed temporary application required.");lateClaimed=true;claimedLate=phase;}
    internal void CheckLateGrant(Math2Mode0PriorDomDiff.TemporaryApplyBorrow input,Math2Mode0PriorLateRegistration phase)
    {CheckLate(input);if(!lateClaimed||!ReferenceEquals(claimedLate,phase)||!phase.IsFor(this))throw new InvalidOperationException("Exact privately claimed late phase required.");}
}
internal sealed record Math2Mode0LateSnapshot(bool Complete,bool Ready,bool Failed,bool Released,long Reservation,long Visits,
    int Registrations,int TagsWritten,bool RegistrationInProgress,long[] NewTags,long[] Removed,Math2Mode0LateData? Observed);
internal sealed partial class Math2Mode0PriorLateRegistration:IDisposable
{
    private ActiveBorrow? activeBorrow;
    private Math2Mode0PriorDomDiff.TemporaryApplyBorrow? activeInput;
    internal sealed class ActiveBorrow
    {
        private readonly Math2Mode0PriorLateRegistration owner;
        private ActiveBorrow(Math2Mode0PriorLateRegistration owner){this.owner=owner;}
        internal static ActiveBorrow Issue(Math2Mode0PriorLateRegistration owner,Math2Mode0PriorDomDiff.TemporaryApplyBorrow input)
        {
            if(!Monitor.IsEntered(owner.gate)||owner.activeBorrow is not null||!ReferenceEquals(owner.activeInput,input)||owner.complete||owner.failed||owner.released)
                throw new InvalidOperationException("Only the claimed private active late factory may issue its synchronous grant.");
            owner.parent.CheckLateGrant(input,owner);return new(owner);
        }
        internal void Check(Math2Mode0PriorDomDiff.TemporaryApplyBorrow input)
        {
            if(!Monitor.IsEntered(owner.gate)||!ReferenceEquals(owner.activeBorrow,this)||!ReferenceEquals(owner.activeInput,input)||owner.failed||owner.released)
                throw new InvalidOperationException("Exact unrevoked active late grant required on the owning thread.");
            owner.parent.CheckLateGrant(input,owner);
        }
    }
    private readonly object gate=new();private readonly Math2Mode0PriorTemporaryApply parent;private readonly Math2DomDiffWork work;private readonly long reservation;
    private bool complete,ready,failed,released,registering;private int registrations,written;private readonly List<long> tags,removed;private Math2Mode0LateData? observed;
    private Math2Mode0PriorLateRegistration(Math2Mode0PriorTemporaryApply parent,Math2DomDiffWork work,long reservation,(int Tags,int Old) capacity){this.parent=parent;this.work=work;this.reservation=reservation;tags=new(capacity.Tags);removed=new(capacity.Old);}
    internal bool IsFor(Math2Mode0PriorTemporaryApply actual)=>ReferenceEquals(parent,actual);
    internal static Math2Mode0PriorLateRegistration Create(Math2Mode0PriorTemporaryApply parent,CancellationToken token=default)
        =>CreateDiagnostic(parent,Math2Mode0PriorCandidatePreparation.DefaultBudget,null,token);
    internal static Math2Mode0PriorLateRegistration CreateDiagnostic(Math2Mode0PriorTemporaryApply parent,long budget,
        Action<Math2Mode0LateStage,Math2Mode0PriorLateRegistration>? observe,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(parent);token.ThrowIfCancellationRequested();if(budget<0||budget>Math2Mode0PriorCandidatePreparation.DefaultBudget)throw new ArgumentOutOfRangeException(nameof(budget));Math2Mode0PriorLateRegistration? target=null;
        try
        {
            var result=parent.WithLateInput(input=>
            {
                var work=new Math2DomDiffWork(token);long plan=input.PlanLate(budget,observe is not null,work);target=new(parent,work,plan,input.LateCapacities);parent.ClaimLate(input,target);
                lock(target.gate)
                {
                    void Stage(Math2Mode0LateStage stage)
                    {
                        if(observe is not null)target.observed=input.CopyLateData(work);
                        observe?.Invoke(stage,target);parent.CheckLate(input,token);ObjectDisposedException.ThrowIf(target.released,target);token.ThrowIfCancellationRequested();
                    }
                    target.activeInput=input;var authority=ActiveBorrow.Issue(target,input);target.activeBorrow=authority;
                    try
                    {
                    Stage(Math2Mode0LateStage.Claimed);
                    input.ResolveLate(authority,work,(stage,id)=>
                    {
                        if(stage==Math2Mode0LateStage.BeforeRegister)target.registering=true;
                        else if(stage==Math2Mode0LateStage.Registered){target.registrations++;target.registering=false;}
                        else if(stage==Math2Mode0LateStage.TagWritten)target.written++;
                        Stage(stage);
                    });Stage(Math2Mode0LateStage.Resolved);
                    input.CollectLate(authority,work,id=>{work.Visit(16);target.tags.Add(id);Stage(Math2Mode0LateStage.TagCollected);});Stage(Math2Mode0LateStage.Collected);
                    input.RemoveLate(authority,target.tags,work,id=>{target.removed.Add(id);Stage(Math2Mode0LateStage.Removed);});
                    target.observed=input.CopyLateData(work);target.complete=true;Stage(Math2Mode0LateStage.Completed);return target;
                    }
                    finally{target.activeBorrow=null;target.activeInput=null;}
                }
            },token);
            lock(result.gate){ObjectDisposedException.ThrowIf(result.released,result);if(result.failed||!result.complete)throw new InvalidOperationException("Late ancestor completion fence failed.");result.ready=true;}return result;
        }
        catch{if(target is not null){lock(target.gate){target.failed=true;if(observe is null)target.Dispose();}}throw;}
    }
    internal Math2Mode0LateSnapshot CopyResult(CancellationToken token=default)
        =>parent.WithLateInput(input=>{lock(gate){input.Check();ObjectDisposedException.ThrowIf(released,this);if(failed||!complete||!ready)throw new InvalidOperationException("Same completed actual late phase required.");return CopySnapshot();}},token);
    internal Math2Mode0LateSnapshot CopySnapshot(){lock(gate)return new(complete,ready,failed,released,reservation,work.Visits,registrations,written,registering,tags.ToArray(),removed.ToArray(),observed);}
    public void Dispose(){lock(gate){released=true;observed=null;}}
}

// Numeric callback kernel only: these records/delegates confer no owning arena,
// retained-Ink or publication authority. The owning adapter above supplies every
// callback from its same private candidate and strict typed live tag.
internal static class Math2Cb8LateNumericKernel
{
    internal static bool Resolve<T>(long value,IReadOnlyList<T> appendOrder,Func<T,long> id,Func<T,long> cached,
        Action<T,long> cache,Func<T,long> register,Func<long,bool> write,Math2DomDiffWork work)where T:class
    {
        work.Visit();if(value==long.MaxValue)return false;if(value>=0)return true;
        long key=unchecked(-value);T? match=null;
        for(int i=appendOrder.Count-1;i>=0;i--){work.Visit();var row=appendOrder[i];if(id(row)==key){match=row;break;}}
        if(match is null)return false;work.Visit();long result=cached(match);
        if(result<0){result=register(match);cache(match,result);if(result<0)return false;}
        return write(result);
    }
}
