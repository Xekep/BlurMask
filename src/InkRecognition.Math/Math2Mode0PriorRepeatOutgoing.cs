using System.Text;
using RepeatAccess=InkRecognition.Math.Math2Mode0PriorLateRegistration.RepeatBorrow.Access;
namespace InkRecognition.Math;

// Synchronous borrowed DATA, not an authority token. The owning factory accepts
// only a completed late owner, and creates this view inside its private chain.
internal sealed record Math2Mode0RepeatCohort(Math2Mode0OriginalArena Original,
    Math2Mode0OriginalArena.Node OriginalField,Math2Mode0OriginalArena Late,
    Math2Mode0OriginalArena.Node LateField,LegacyLibraryOriginalDomNameOwner Names,
    object OriginalInk,object ThirdInk,LegacyOwnedRawInkDocument Document,
    Math2Mode0CurrentInputReceipt Receipt,Math2Mode0OriginalApplication Application,Math2Mode0CurrentInputHandle Handle,long Epoch,long OldNext,long NewNext,
    Math2Mode0OriginalRegistry[] OldRegistry,Math2Mode0PublishRegistry[] NewRegistry,
    Math2Mode0CurrentIdentitySnapshot Identity,Action Check);
internal sealed partial class Math2Mode0OriginalMathFieldSource
{
    private sealed partial class PriorThirdInk
    {internal override LegacyOwnedRawInkDocument RepeatDocument{get{Parent();lock(inkGate){Live();return ink.Document;}}}}
}
internal sealed partial class Math2Mode0OriginalApplication
{
    internal sealed partial class CurrentInputBorrow
    {
        internal (Math2Mode0OriginalArena Arena,Math2Mode0OriginalArena.Node Field,Math2Mode0OriginalRegistry[] Registry,long Next,
            Math2Mode0CurrentIdentitySnapshot Geometry) ReadRepeatOriginal(Math2Mode0PriorCandidatePreparation.DiffBorrow candidate,Math2Mode0PriorDomDiff.TemporaryApplyBorrow caller,RepeatAccess access,Math2DomDiffWork work)
        {
            ArgumentNullException.ThrowIfNull(access);candidate.RequireRepeatCurrent(this,caller,access);Check();work.Visit();long next=input.Next();if(next is <1 or >65536)throw new NotSupportedException("Bounded actual current source Next required.");
            work.Visit(checked(65536+next*64));var registry=input.ReadRegistry();if(registry.Length>next)throw new InvalidOperationException("Current signed-ID registry/Next invariant lost.");
            foreach(var row in registry){work.Visit();work.Visit(row.Ranges.LongLength);if(row.Id<0||row.Id>=next)throw new NotSupportedException("Current library auto-ID profile required.");}
            work.Visit(owner.active.Nodes.Count);var field=owner.active.Nodes[owner.originalField-1];
            if(field.Kind!=3||field.Name.Text!="mathField"||!ReferenceEquals(field.Name.Owner,owner.originalNames))throw new InvalidOperationException("Actual current original field required.");
            var at=field;int depth=0;while(at.Parent!=0){work.Visit();if(++depth>64||at.Parent<1||at.Parent>owner.active.Nodes.Count)throw new NotSupportedException("Bounded original context ancestry required.");at=owner.active.Nodes[at.Parent-1];}
            if(at.Kind!=4||!ReferenceEquals(at,owner.active.Nodes[0]))throw new NotSupportedException("This native export profile requires the actual document context root.");
            var actualInput=registry.Single(r=>r.Id==handle.InputId);work.Visit(checked(4096L+raw.Count*4096L));
            var identity=ExportIdentity(actualInput.Ranges,work.Token);
            if(!ReferenceEquals(identity.Document,raw.Document)||identity.MatricesResolved!=raw.Count||identity.NativeNonidentityTransformCount!=0)
                throw new NotSupportedException("Actual full current C49 identity geometry required.");
            Check();return(owner.active,field,registry,next,new(identity.MatricesResolved,0,identity.CopyResolvedMatrices()));
        }
    }
}
internal sealed partial class Math2Mode0PriorSelectedScope
{
    internal Math2Mode0OriginalApplication.CurrentInputBorrow RepeatCurrent(Math2Mode0PriorCandidatePreparation.DiffBorrow candidate,Math2Mode0PriorDomDiff.TemporaryApplyBorrow caller,RepeatAccess access)
    {
        ArgumentNullException.ThrowIfNull(access);access.CheckFor(caller,candidate);candidate.Check();if(!Monitor.IsEntered(gate)||!candidate.IsForRepeatScope(this)||changedReceipt is null||activeChangedBorrow is null)
            throw new InvalidOperationException("Actual active changed-input scope required; fresh/prior snapshot is not an export source.");
        activeChangedBorrow.CheckReceipt(changedReceipt);return activeChangedBorrow;
    }
}
internal sealed partial class Math2Mode0PriorCandidatePreparation
{
    internal sealed partial class DiffBorrow
    {
        internal void RequireRepeatCurrent(Math2Mode0OriginalApplication.CurrentInputBorrow current,Math2Mode0PriorDomDiff.TemporaryApplyBorrow caller,RepeatAccess access)
        {ArgumentNullException.ThrowIfNull(access);access.CheckFor(caller,this);if(!ReferenceEquals(owner.scope.RepeatCurrent(this,caller,access),current))throw new InvalidOperationException("Exact privately accessed original current borrow required.");}
        internal bool IsForRepeatScope(Math2Mode0PriorSelectedScope actual){Check();return ReferenceEquals(owner.scope,actual);}
        internal Math2Mode0RepeatCohort ReadRepeatCohort(RepeatAccess authority,
            Math2Mode0PriorDomDiff.TemporaryApplyBorrow caller,Math2DomDiffWork work)
        {
            authority.CheckFor(caller,this);Check();var current=owner.scope.RepeatCurrent(this,caller,authority);var old=current.ReadRepeatOriginal(this,caller,authority,work);
            if(!ReferenceEquals(owner.third!.RepeatDocument,current.Document)||!ReferenceEquals(owner.third.Names,current.Names)||
                !ReferenceEquals(owner.names,current.Names)||ReferenceEquals(owner.third.Identity,current.OriginalInkIdentity))
                throw new InvalidOperationException("Same current projection/shared names and distinct genuine THIRD required.");
            work.Visit(checked(65536+owner.third.SnapshotNext*64));var rows=owner.third.BorrowRegistry();
            foreach(var row in rows){work.Visit();work.Visit(row.Ranges.LongLength);}
            var temp=owner.temporary!;work.Visit();var next=temp.Nodes[0];
            if(next.Kind!=3||next.Parent!=0||temp.WriterFreed(next)||!ReferenceEquals(next.Name,old.Field.Name))
                throw new InvalidOperationException("Same actual completed late field required.");
            void CohortCheck(){authority.CheckFor(caller,this);current.Check();}
            CohortCheck();return new(old.Arena,old.Field,temp,next,owner.names,current.OriginalInkIdentity,owner.third.Identity,
                current.Document,current.Receipt,current.Owner,current.RequireHandleForApply(this,caller,authority),current.AppliedEpoch,old.Next,owner.third.SnapshotNext,old.Registry,rows,old.Geometry,CohortCheck);
        }
    }
}
internal sealed partial class Math2Mode0PriorDomDiff
{
    internal sealed partial class TemporaryApplyBorrow
    {
        internal void RequireRepeatCandidate(Math2Mode0PriorCandidatePreparation.DiffBorrow actual,Math2Mode0PriorLateRegistration.RepeatBorrow authority)
        {ArgumentNullException.ThrowIfNull(authority);Check();authority.CheckFor(this);if(!ReferenceEquals(input,actual))throw new InvalidOperationException("Exact actual repeat candidate required.");}
        internal Math2Mode0RepeatCohort ReadRepeatCohort(RepeatAccess authority,Math2DomDiffWork work)
        {ArgumentNullException.ThrowIfNull(authority);authority.CheckFor(this,input);return input.ReadRepeatCohort(authority,this,work);}
    }
}
internal sealed partial class Math2Mode0PriorLateRegistration
{
    private RepeatBorrow? activeRepeatBorrow;private bool repeatClaimed;private Math2Mode0PriorRepeatOutgoing? claimedRepeat;
    internal sealed partial class RepeatBorrow
    {
        private readonly Math2Mode0PriorLateRegistration owner;private readonly Math2Mode0PriorDomDiff.TemporaryApplyBorrow input;
        private RepeatBorrow(Math2Mode0PriorLateRegistration owner,Math2Mode0PriorDomDiff.TemporaryApplyBorrow input){this.owner=owner;this.input=input;}
        internal static RepeatBorrow Issue(Math2Mode0PriorLateRegistration owner,Math2Mode0PriorDomDiff.TemporaryApplyBorrow input)
        {if(!Monitor.IsEntered(owner.gate)||owner.activeRepeatBorrow is not null)throw new InvalidOperationException("Exclusive completed late borrow required.");owner.CheckRepeat(input);return new(owner,input);}
        internal void Check(){if(!Monitor.IsEntered(owner.gate)||!ReferenceEquals(owner.activeRepeatBorrow,this))throw new InvalidOperationException("Exact synchronous completed late export borrow required.");owner.CheckRepeat(input);}
        internal void CheckFor(Math2Mode0PriorDomDiff.TemporaryApplyBorrow actual){Check();if(!ReferenceEquals(input,actual))throw new InvalidOperationException("Foreign temporary input.");}
        internal long Plan(long budget,Math2DomDiffWork work)
        =>WithAccess<long>(null,AccessKind.Plan,a=>Math2Mode0PriorRepeatOutgoing.PlanBorrowed(input.ReadRepeatCohort(a,work),owner.reservation,budget,work));
        internal void BuildClaimed(Math2Mode0PriorRepeatOutgoing target,Math2DomDiffWork work,Action<Math2Mode0RepeatOutgoingStage,Math2Mode0PriorRepeatOutgoing>? observe)
        {WithAccess(target,AccessKind.Build,a=>{target.BuildClaimed(this,input.ReadRepeatCohort(a,work),observe);return 0;});}
        internal void ValidateResult(Math2Mode0PriorRepeatOutgoing target,Math2DomDiffWork work)
        {WithAccess(target,AccessKind.Validate,a=>{target.ValidateCohort(this,input.ReadRepeatCohort(a,work));return 0;});}
        private Access? activeAccess;private bool openingAccess;
        internal enum AccessKind{Plan,Build,Validate}
        internal sealed class Access
        {
            private readonly RepeatBorrow owner;private readonly Math2Mode0PriorRepeatOutgoing? target;private readonly AccessKind kind;
            private Access(RepeatBorrow owner,Math2Mode0PriorRepeatOutgoing? target,AccessKind kind){this.owner=owner;this.target=target;this.kind=kind;}
            internal static Access Issue(RepeatBorrow owner,Math2Mode0PriorRepeatOutgoing? target,AccessKind kind)
            {
                owner.Check();if(!owner.openingAccess||owner.activeAccess is not null)throw new InvalidOperationException("Only the closed private plan/build/read operation may issue its extraction grant.");
                if(kind==AccessKind.Plan){if(target is not null)throw new InvalidOperationException();}
                else{ArgumentNullException.ThrowIfNull(target);target.RequirePrivateAccess(owner,kind);}
                return new(owner,target,kind);
            }
            internal void CheckFor(Math2Mode0PriorDomDiff.TemporaryApplyBorrow caller,Math2Mode0PriorCandidatePreparation.DiffBorrow candidate)
            {
                owner.Check();if(!ReferenceEquals(owner.activeAccess,this)||!ReferenceEquals(owner.input,caller))throw new InvalidOperationException("Exact unrevoked private repeat extraction grant required.");
                if(kind!=AccessKind.Plan)target!.RequirePrivateAccess(owner,kind);caller.RequireRepeatCandidate(candidate,owner);
            }
        }
        private T WithAccess<T>(Math2Mode0PriorRepeatOutgoing? target,AccessKind kind,Func<Access,T> consume)
        {
            Check();if(activeAccess is not null||openingAccess)throw new InvalidOperationException("Exclusive private extraction operation required.");
            openingAccess=true;
            try
            {
                var access=Access.Issue(this,target,kind);activeAccess=access;openingAccess=false;
                try{return consume(access);}finally{activeAccess=null;}
            }
            finally{openingAccess=false;}
        }
        internal long Reservation{get{Check();return owner.reservation;}}
        internal void Claim(Math2Mode0PriorRepeatOutgoing target)
        {Check();if(owner.repeatClaimed||!target.IsFor(owner))throw new InvalidOperationException("Completed late outgoing claim already used or foreign.");owner.repeatClaimed=true;owner.claimedRepeat=target;}
        internal void CheckClaim(Math2Mode0PriorRepeatOutgoing target){Check();if(!owner.repeatClaimed||!ReferenceEquals(owner.claimedRepeat,target))throw new InvalidOperationException("Exact claimed repeat outgoing owner required.");}
    }
    private void CheckRepeat(Math2Mode0PriorDomDiff.TemporaryApplyBorrow input)
    {
        parent.CheckLateGrant(input,this);ObjectDisposedException.ThrowIf(released,this);
        if(failed||!complete||!ready||activeBorrow is not null||activeInput is not null)throw new InvalidOperationException("Only fully ready late owner may enter repeat export.");
    }
    internal T WithRepeatInput<T>(Func<RepeatBorrow,T> consume,CancellationToken token=default)
        =>parent.WithLateInput(input=>
        {
            lock(gate)
            {
                CheckRepeat(input);if(activeRepeatBorrow is not null)throw new InvalidOperationException("Exclusive repeat export access required.");
                var borrow=RepeatBorrow.Issue(this,input);activeRepeatBorrow=borrow;
                try{var value=consume(borrow);borrow.Check();token.ThrowIfCancellationRequested();return value;}finally{activeRepeatBorrow=null;}
            }
        },token);
}
internal static class Math2Mode0RepeatDomViews
{
    // Prepared canonical-name data only. Core internals stay internal; these
    // tokens cannot construct a field, Ink, receipt or owning export.
    internal static LegacyLibraryOriginalDomNameOwner PreparedNames()=>new();
    internal static LegacyLibraryOriginalDomName PreparedName(LegacyLibraryOriginalDomNameOwner owner,string text)=>owner.Intern(text);
    internal static Math2DomDiffNode Build(Math2Mode0OriginalArena arena,Math2Mode0OriginalArena.Node root,
        LegacyLibraryOriginalDomNameOwner names,Math2DomDiffWork work,Action check)
    {
        var seen=new HashSet<object>(ReferenceEqualityComparer.Instance);
        Math2DomDiffNode Walk(Math2Mode0OriginalArena.Node n,int depth)
        {
            check();work.Visit();if(depth>64||n.Id<1||n.Id>arena.Nodes.Count||!ReferenceEquals(arena.Nodes[n.Id-1],n)||arena.WriterFreed(n)||
                !seen.Add(n)||!ReferenceEquals(n.Name.Owner,names)||n.Uri is not null||n.Prefix is not null)throw new NotSupportedException("Bounded canonical live default-schema field required.");
            work.Visit(checked(32L+(n.Text?.Length??0)*3L));var result=new Math2DomDiffNode(arena,n,n.Id,n.Kind,n.Name,Math2DomDiffScalar.From(n.Kind is 0 or 1?n.Tag:(byte)0,n.Text,n.Number));
            var list=arena.Children(n.Id);work.Visit(list.Count);
            foreach(var child in list){if(child.Parent!=n.Id)throw new InvalidOperationException("Live actual parent relation lost.");var value=Walk(child,depth+1);value.Parent=result;(child.Kind==0?result.Attributes:result.Children).Add(value);}return result;
        }
        return Walk(root,0);
    }
}
internal sealed record Math2Mode0RepeatTreeNode(int Id,byte Kind,string Name,byte ValueTag,byte[] Storage,byte[] Heap,int[] Attributes,int[] Children);
internal sealed record Math2Mode0RepeatDomAction(byte Operation,int[] Path,int Ordinal,int PayloadRoot,int OriginalNode,int NewNode);
internal sealed record Math2Mode0RepeatSourceCommand(byte Operation,long Id,string? Name,int Category,
    LegacyNativeInkPositionRange[] Ranges,long OldCounter20,long NewCounter18,bool DetachedC22);
internal sealed record Math2Mode0RepeatOutgoingSnapshot(bool Complete,bool Ready,bool Failed,bool Released,long Reservation,long Visits,
    string ComparisonProfile,Math2Mode0RepeatDomAction[] Dom,Math2Mode0RepeatSourceCommand[] Source,
    Math2Mode0OriginalDomNode[] FourthWriter,int[] ReleasedScratchRoots,bool DistinctFourthNames,
    long OldNext,long NewNext,long Epoch,bool SameCurrentProjection,Math2Mode0CurrentIdentitySnapshot Geometry,Math2DomDiffActionSnapshot[] RawActions,
    Math2Mode0OriginalDomNode[] OriginalBefore,Math2Mode0OriginalDomNode[] LateBefore,Math2Mode0OriginalDomNode[] OriginalAfter,Math2Mode0OriginalDomNode[] LateAfter,Math2Mode0RepeatTreeNode[] OriginalTree,Math2Mode0RepeatTreeNode[] LateTree);

internal enum Math2Mode0RepeatOutgoingStage{Claimed,RootsBound,DomLinked,DomProduced,DomSerialized,SourceCommand,Packaged,Completed}
internal sealed partial class Math2Mode0OriginalArena
{
    internal void ReleaseRepeatScratch(Node root,Math2DomDiffWork work)
    {
        work.Visit();if(root.Parent!=0||root.Id<1||root.Id>Nodes.Count||!ReferenceEquals(Nodes[root.Id-1],root)||writerFreed.Contains(root))
            throw new InvalidOperationException("Own unattached fourth-writer scratch root required.");
        void Mark(Node n,int depth)
        {work.Visit();if(depth>64)throw new NotSupportedException();var list=Children(n.Id);work.Visit(list.Count);foreach(var child in list)Mark(child,depth+1);writerFreed.Add(n);}
        Mark(root,0);
    }
}
// Owning repeat export only. It never calls an original receiver, changes an
// original registry/arena/counter, or accepts action arrays/snapshots as inputs.
internal sealed partial class Math2Mode0PriorRepeatOutgoing:IDisposable
{
    internal const long DefaultBudget=64L*1024*1024;
    private sealed class DetachedC22(LegacyNativeInkPositionRange[] ranges){internal readonly LegacyNativeInkPositionRange[] Ranges=ranges;}
    private sealed record OwnedSource(byte Operation,long Id,string? Name,int Category,DetachedC22? C22,long Before,long After);
    private sealed record OwnedDom(byte Operation,int[] Path,int Ordinal,Math2Mode0OriginalArena.Node? Payload,int OriginalNode,int NewNode);
    private readonly object gate=new();private readonly Math2Mode0PriorLateRegistration parent;private readonly Math2DomDiffWork work;private readonly long reservation;
    private readonly LegacyLibraryOriginalDomNameOwner names=new();private readonly Math2Mode0OriginalArena fourth;
    private readonly List<OwnedDom> dom=[];private readonly List<OwnedSource> source=[];private readonly List<Math2DomDiffActionSnapshot> raw=[];private readonly List<int> releasedScratch=[];
    private Math2Mode0RepeatTreeNode[] originalTree=[],lateTree=[];private Math2Mode0OriginalDomNode[] originalBefore=[],lateBefore=[],originalAfter=[],lateAfter=[];private Math2Mode0CurrentIdentitySnapshot geometry=new(0,0,[]);
    private LegacyLibraryOriginalDomNameOwner? sourceNames;private object? originalInk,thirdInk,originalArena;private LegacyOwnedRawInkDocument? document;private Math2Mode0CurrentInputReceipt? receipt;
    private Math2Mode0PriorLateRegistration.RepeatBorrow? activeBuildBorrow,activeValidateBorrow;private long epoch,oldNext,newNext;private bool complete,ready,failed,released,packaged;
    private Math2Mode0PriorRepeatOutgoing(Math2Mode0PriorLateRegistration parent,Math2DomDiffWork work,long reservation)
    {this.parent=parent;this.work=work;this.reservation=reservation;fourth=new(names);fourth.NewDiffPayload(4,"",0,0,null,null,0);}
    internal bool IsFor(Math2Mode0PriorLateRegistration actual)=>ReferenceEquals(parent,actual);
    internal static Math2Mode0PriorRepeatOutgoing Create(Math2Mode0PriorLateRegistration parent,CancellationToken token=default)
        =>CreateDiagnostic(parent,DefaultBudget,null,token);
    internal static Math2Mode0PriorRepeatOutgoing CreateDiagnostic(Math2Mode0PriorLateRegistration parent,long budget,
        Action<Math2Mode0RepeatOutgoingStage,Math2Mode0PriorRepeatOutgoing>? observe,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(parent);token.ThrowIfCancellationRequested();if(budget<0||budget>DefaultBudget)throw new ArgumentOutOfRangeException(nameof(budget));Math2Mode0PriorRepeatOutgoing? target=null;
        try
        {
            var result=parent.WithRepeatInput(input=>
            {
                var work=new Math2DomDiffWork(token);long plan=input.Plan(budget,work);
                target=new(parent,work,plan);input.Claim(target);
                lock(target.gate)
                {
                    target.activeBuildBorrow=input;
                    try{input.BuildClaimed(target,work,observe);return target;}
                    finally{target.activeBuildBorrow=null;}
                }
            },token);
            lock(result.gate){ObjectDisposedException.ThrowIf(result.released,result);if(result.failed||!result.complete||!result.packaged)throw new InvalidOperationException("Repeat export ancestor completion fence failed.");result.ready=true;}return result;
        }
        catch{if(target is not null){lock(target.gate){target.failed=true;if(observe is null)target.Dispose();}}throw;}
    }
    internal static long PlanBorrowed(Math2Mode0RepeatCohort c,long prior,long budget,Math2DomDiffWork work)
    {
        c.Check();work.Visit();long nodes=checked(c.Original.Nodes.Count+c.Late.Nodes.Count),cells=checked((nodes+1)*(nodes+1));
        if(nodes>1023||cells>Math2DomDiffWork.MaximumCells||c.OldNext is <1 or >65536||c.NewNext<c.OldNext||c.NewNext>65536||c.NewRegistry.Length>65536)
            throw new NotSupportedException("Bounded default-schema/current raw identity repeat export required.");
        long bytes=0,ranges=0;foreach(var arena in new[]{c.Original,c.Late})foreach(var n in arena.Nodes)
        {work.Visit(checked(1L+(n.Text?.Length??0)*3L));bytes=checked(bytes+Encoding.UTF8.GetByteCount(n.Text??""));}
        foreach(var row in c.OldRegistry){work.Visit();ranges=checked(ranges+row.Ranges.LongLength);}
        foreach(var row in c.NewRegistry){work.Visit();ranges=checked(ranges+row.Ranges.LongLength);}
        work.Visit(ranges);ValidateCommon(c,work);
        long plan=checked(prior+65536+cells*8+nodes*8192+bytes*64+ranges*256+(c.OldRegistry.Length+c.NewRegistry.Length)*2048L);
        if(plan>budget)throw new NotSupportedException("Conservative fourth-writer/diff/payload/source reservation before claim exceeds budget.");return plan;
    }
    internal void RequirePrivateAccess(Math2Mode0PriorLateRegistration.RepeatBorrow input,Math2Mode0PriorLateRegistration.RepeatBorrow.AccessKind kind)
    {
        if(!Monitor.IsEntered(gate)||failed||released||!ReferenceEquals(kind==Math2Mode0PriorLateRegistration.RepeatBorrow.AccessKind.Build?activeBuildBorrow:activeValidateBorrow,input)||
            (kind==Math2Mode0PriorLateRegistration.RepeatBorrow.AccessKind.Build?ready:!complete||!ready||!packaged))
            throw new InvalidOperationException("Exact active private factory build/result read required before original extraction.");
        input.CheckClaim(this);
    }
    internal void BuildClaimed(Math2Mode0PriorLateRegistration.RepeatBorrow input,Math2Mode0RepeatCohort cohort,
        Action<Math2Mode0RepeatOutgoingStage,Math2Mode0PriorRepeatOutgoing>? observe)
    {
        if(!Monitor.IsEntered(gate)||!ReferenceEquals(activeBuildBorrow,input)||complete||failed||released)
            throw new InvalidOperationException("Only this active private owning export factory may build.");
        input.CheckClaim(this);cohort.Check();Bind(cohort,work);
        void Stage(Math2Mode0RepeatOutgoingStage stage){observe?.Invoke(stage,this);input.CheckClaim(this);cohort.Check();ObjectDisposedException.ThrowIf(released,this);work.Token.ThrowIfCancellationRequested();}
        Stage(Math2Mode0RepeatOutgoingStage.Claimed);Build(cohort,Stage);complete=true;Stage(Math2Mode0RepeatOutgoingStage.Completed);
    }
    internal void ValidateCohort(Math2Mode0PriorLateRegistration.RepeatBorrow input,Math2Mode0RepeatCohort c)
    {
        if(!Monitor.IsEntered(gate))throw new InvalidOperationException("Actual export gate required.");input.CheckClaim(this);c.Check();ObjectDisposedException.ThrowIf(released,this);
        if(failed||!complete||!ready||!packaged||!ReferenceEquals(c.Original,originalArena)||!ReferenceEquals(c.OriginalInk,originalInk)||!ReferenceEquals(c.ThirdInk,thirdInk)||!ReferenceEquals(c.Document,document)||!ReferenceEquals(c.Receipt,receipt)||c.Epoch!=epoch||c.OldNext!=oldNext||c.NewNext!=newNext)
            throw new InvalidOperationException("Same completed live owning repeat export required.");
    }
    private static void ValidateCommon(Math2Mode0RepeatCohort c,Math2DomDiffWork work)
    {
        work.Visit(checked(32L*(c.OldRegistry.Length+c.NewRegistry.Length)));
        var old=c.OldRegistry.OrderBy(r=>r.Id).ToArray();var next=c.NewRegistry.OrderBy(r=>r.Id).ToArray();
        foreach(var r in old){work.Visit();if(r.Category!=102||r.Name is not("INPUT"or"MATH_SYMBOL"or"MATH_NODE"))throw new NotSupportedException("Library null-metadata Math source profile only.");}
        foreach(var r in next){work.Visit();if(r.Id<0||r.Id>=c.NewNext||r.Category!=102||r.Name is not("INPUT"or"MATH_SYMBOL"or"MATH_NODE"))throw new NotSupportedException("Actual auto-ID Math source registry required.");}
        int i=0,j=0;while(i<old.Length&&j<next.Length)
        {
            work.Visit();var a=old[i];var b=next[j];if(a.Id<b.Id){i++;continue;}if(a.Id>b.Id){j++;continue;}
            work.Visit(checked(8L+a.Ranges.LongLength+b.Ranges.LongLength));
            if(a.Name!=b.Name||a.Category!=b.Category||!a.Ranges.AsSpan().SequenceEqual(b.Ranges))
                throw new NotSupportedException("Changed common IDs require the distinct native UPDATE5 union, not fresh TransferCommand reuse.");i++;j++;
        }
    }
    private void Bind(Math2Mode0RepeatCohort c,Math2DomDiffWork work)
    {
        sourceNames=c.Names;originalInk=c.OriginalInk;thirdInk=c.ThirdInk;originalArena=c.Original;document=c.Document;receipt=c.Receipt;epoch=c.Epoch;oldNext=c.OldNext;newNext=c.NewNext;geometry=c.Identity;
        work.Visit(checked((c.Original.Nodes.Count+c.Late.Nodes.Count)*4L));originalBefore=c.Original.Snapshot();lateBefore=c.Late.Snapshot();
    }
    private void Build(Math2Mode0RepeatCohort c,Action<Math2Mode0RepeatOutgoingStage> stage)
    {
        var old=Math2Mode0RepeatDomViews.Build(c.Original,c.OriginalField,c.Names,work,c.Check);
        var next=Math2Mode0RepeatDomViews.Build(c.Late,c.LateField,c.Names,work,c.Check);originalTree=CopyTree(old,work);lateTree=CopyTree(next,work);stage(Math2Mode0RepeatOutgoingStage.RootsBound);
        var scratch=new List<Math2Mode0OriginalArena.Node>();
        Math2DomDiffNode CloneOperand(Math2DomDiffNode value)
        {
            c.Check();work.Visit();Math2Mode0OriginalArena sourceArena;
            if(ReferenceEquals(value.Writer,c.Original))sourceArena=c.Original;else if(ReferenceEquals(value.Writer,c.Late))sourceArena=c.Late;else throw new InvalidOperationException("Only actual old/current late operands may enter the fourth writer.");
            if(value.Identity is not Math2Mode0OriginalArena.Node n||n.Id<1||n.Id>sourceArena.Nodes.Count||!ReferenceEquals(sourceArena.Nodes[n.Id-1],n))throw new InvalidOperationException("Actual same source node required.");
            int id=fourth.CloneOriginal(sourceArena,n.Id,0,_=>{c.Check();work.Visit(128);});var root=fourth.Nodes[id-1];scratch.Add(root);
            return Math2Mode0RepeatDomViews.Build(fourth,root,names,work,c.Check);
        }
        var actions=Math2DomDefaultDiff.ProduceExport(old,next,fourth,CloneOperand,work,a=>{raw.Add(a.Snapshot());stage(Math2Mode0RepeatOutgoingStage.DomLinked);});
        if(actions.Count==0)throw new NotSupportedException("Empty native BC2 allocation policy is outside this first genuine changed-input scope.");stage(Math2Mode0RepeatOutgoingStage.DomProduced);
        foreach(var a in actions)
        {
            c.Check();work.Visit();if(!ReferenceEquals(a.NodeAt18.Writer,c.Original))throw new InvalidOperationException("Produced repeat action must reference actual original field.");
            var parent=a.Operation==7?a.NodeAt18.Parent:a.NodeAt18;var path=PreparedPath(old,parent,work);int ordinal=a.Operation is 2 or 4?a.Ordinal:0;Math2Mode0OriginalArena.Node? payload=null;
            if(a.Operation is 2 or 5 or 6 or 7)
            {
                var from=a.Operation==7?a.NodeAt18:a.NodeAt20??throw new InvalidOperationException();var arena=ReferenceEquals(from.Writer,fourth)?fourth:ReferenceEquals(from.Writer,c.Original)?c.Original:throw new InvalidOperationException();
                if(from.Identity is not Math2Mode0OriginalArena.Node actual)throw new InvalidOperationException();
                int id=fourth.CloneOriginal(arena,actual.Id,0,_=>{c.Check();work.Visit(128);});payload=fourth.Nodes[id-1];
                if(a.Operation==7){var scalar=a.NewValue??throw new InvalidOperationException();var fields=scalar.CopyWriterFields(work);payload.Tag=scalar.Tag;payload.Text=fields.Text;payload.Number=fields.Number;}
            }
            else if(a.Operation!=4)throw new NotSupportedException("Only produced default Math operations2/4/5/6/7 are serialized.");
            dom.Add(new(a.Operation,path,ordinal,payload,a.NodeAt18.Id,a.SourceCandidate));
        }
        // E3A70 releases the intermediate header operands after E35C0 made the
        // independently retained exported payloads. Typed freed markers, no ABI.
        fourth.EnsureRepeatScratchCapacity(scratch.Count,work);
        foreach(var n in scratch){fourth.ReleaseRepeatScratch(n,work);releasedScratch.Add(n.Id);}stage(Math2Mode0RepeatOutgoingStage.DomSerialized);
        work.Visit(checked(32L*(c.OldRegistry.Length+c.NewRegistry.Length)));var oldRows=c.OldRegistry.OrderBy(r=>r.Id).ToArray();var newRows=c.NewRegistry.OrderBy(r=>r.Id).ToArray();
        var oldIds=new HashSet<long>();var newIds=new HashSet<long>();foreach(var r in oldRows){work.Visit();oldIds.Add(r.Id);}foreach(var r in newRows){work.Visit();newIds.Add(r.Id);}
        void Command(byte operation,long id,string name,int category,LegacyNativeInkPositionRange[] ranges)
        {
            c.Check();work.Visit(checked(64L+name.Length+ranges.LongLength*2));var detached=new DetachedC22(ranges.ToArray());source.Add(new(operation,id,new string(name.AsSpan()),category,detached,-1,-1));stage(Math2Mode0RepeatOutgoingStage.SourceCommand);
        }
        // 07AE90 uses signed ordered sets, not pending order. Common rows were
        // proved unchanged before claim, so no unsupported UPDATE5 is emitted.
        foreach(var r in newRows){work.Visit();if(!oldIds.Contains(r.Id))Command(4,r.Id,r.Name,r.Category,r.Ranges);}
        foreach(var r in oldRows){work.Visit();if(!newIds.Contains(r.Id))Command(6,r.Id,r.Name,r.Category,r.Ranges);}
        if(c.OldNext<c.NewNext){work.Visit(32);source.Add(new(7,-1,null,0,null,c.OldNext,c.NewNext));stage(Math2Mode0RepeatOutgoingStage.SourceCommand);}
        work.Visit(checked((c.Original.Nodes.Count+c.Late.Nodes.Count)*4L));originalAfter=c.Original.Snapshot();lateAfter=c.Late.Snapshot();
        if(!originalBefore.SequenceEqual(originalAfter)||!lateBefore.SequenceEqual(lateAfter))throw new InvalidOperationException("Repeat export must leave both actual source arenas unchanged.");
        packaged=true;stage(Math2Mode0RepeatOutgoingStage.Packaged);
    }
    private static Math2Mode0RepeatTreeNode[] CopyTree(Math2DomDiffNode root,Math2DomDiffWork work)
    {
        var result=new List<Math2Mode0RepeatTreeNode>();
        void Walk(Math2DomDiffNode n)
        {
            work.Visit(checked(32L+n.Attributes.Count+n.Children.Count));var storage=n.Scalar.CopyStorage();var heap=n.Scalar.CopyHeap();work.Visit(heap.LongLength);
            result.Add(new(n.Id,n.Kind,n.Name.Text,n.Scalar.Tag,storage,heap,n.Attributes.Select(a=>a.Id).ToArray(),n.Children.Select(c=>c.Id).ToArray()));
            foreach(var attr in n.Attributes)Walk(attr);foreach(var child in n.Children)Walk(child);
        }
        Walk(root);return result.ToArray();
    }
    internal static int[] PreparedPath(Math2DomDiffNode anchor,Math2DomDiffNode? node,Math2DomDiffWork work)
    {
        var reversed=new List<int>();while(!ReferenceEquals(node,anchor))
        {
            work.Visit();if(node is null||node.Parent is null||reversed.Count>=64)throw new InvalidOperationException("Produced action path must terminate at the actual original field.");
            var siblings=node.Parent.Children;work.Visit(siblings.Count);int ordinal=siblings.FindIndex(n=>ReferenceEquals(n,node));if(ordinal<0)throw new InvalidOperationException("Actual previous sibling ordinal required.");reversed.Add(ordinal);node=node.Parent;
        }
        reversed.Reverse();return reversed.ToArray();
    }
    internal Math2Mode0RepeatOutgoingSnapshot CopyResult(CancellationToken token=default)
        =>parent.WithRepeatInput(b=>{lock(gate){activeValidateBorrow=b;try{b.ValidateResult(this,new(token));return CopySnapshot();}finally{activeValidateBorrow=null;}}},token);
    internal Math2Mode0RepeatOutgoingSnapshot CopySnapshot()
    {
        lock(gate)return new(complete,ready,failed,released,reservation,work.Visits,"admitted-library-default-schema0/native-local88-unresolved",
            dom.Select(d=>new Math2Mode0RepeatDomAction(d.Operation,d.Path.ToArray(),d.Ordinal,d.Payload?.Id??0,d.OriginalNode,d.NewNode)).ToArray(),
            source.Select(s=>new Math2Mode0RepeatSourceCommand(s.Operation,s.Id,s.Name,s.Category,s.C22?.Ranges.ToArray()??[],s.Before,s.After,s.C22 is not null)).ToArray(),
            fourth.Snapshot(),releasedScratch.ToArray(),sourceNames is not null&&!ReferenceEquals(names,sourceNames),oldNext,newNext,epoch,document is not null,geometry,
            raw.Select(a=>a with{OldStorage=a.OldStorage.ToArray(),NewStorage=a.NewStorage.ToArray(),OldHeap=a.OldHeap.ToArray(),NewHeap=a.NewHeap.ToArray()}).ToArray(),originalBefore.ToArray(),lateBefore.ToArray(),originalAfter.ToArray(),lateAfter.ToArray(),originalTree.Select(t=>t with{Storage=t.Storage.ToArray(),Heap=t.Heap.ToArray(),Attributes=t.Attributes.ToArray(),Children=t.Children.ToArray()}).ToArray(),lateTree.Select(t=>t with{Storage=t.Storage.ToArray(),Heap=t.Heap.ToArray(),Attributes=t.Attributes.ToArray(),Children=t.Children.ToArray()}).ToArray());
    }
    public void Dispose(){lock(gate){if(released)return;released=true;if(activeApplyLease is null)ClearRepeatApplyPayloads();}}
}
internal sealed partial class Math2Mode0OriginalArena
{
    internal void ClearRepeatOwned(){Nodes.Clear();children.Clear();writerFreed.Clear();}
    internal void EnsureRepeatScratchCapacity(int roots,Math2DomDiffWork work)
    {work.Visit(checked(1L+Nodes.Count));writerFreed.EnsureCapacity(Nodes.Count);}
}
