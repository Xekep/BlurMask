using System.Runtime.CompilerServices;
using System.Text;
namespace InkRecognition.Math;

internal abstract class Math2Mode0PriorRetainedInk:Math2Mode0RetainedInk
{internal abstract Math2Mode0PriorCandidateRegistry[] CopyCandidateRegistry();}

internal sealed partial class Math2Mode0PriorRawOnlySource
{
    internal T WithCandidateInput<T>(Func<Math2Mode0PriorC23Preparation.CandidateBorrow,Math2Mode0FirstResultCopy,T> consume,CancellationToken token)
        =>WithLive(()=>
        {
            if(!complete||copy is null)throw new InvalidOperationException("Genuine completed new NN result required.");
            void Check(){ObjectDisposedException.ThrowIf(released,this);if(failed||!complete||copy is null)throw new InvalidOperationException("Same live completed NN owner required.");copy.ValidateForOuterCopy(token);token.ThrowIfCancellationRequested();}
            Check();return preparation.WithActiveCandidate(this,Check,b=>consume(b,copy),token);
        },token);
}
internal sealed partial class Math2Mode0PriorC23Preparation
{
    private CandidateBorrow? activeCandidateBorrow;
    internal sealed class CandidateBorrow
    {
        private readonly Math2Mode0PriorC23Preparation owner;
        private readonly Action sourceCheck;
        private readonly object actualSource;
        private CandidateBorrow(Math2Mode0PriorC23Preparation owner,object source,Action check){this.owner=owner;actualSource=source;sourceCheck=check;}
        internal static CandidateBorrow Issue(Math2Mode0PriorC23Preparation owner,object source,Action check)
        {if(!Monitor.IsEntered(owner.gate)||owner.activeCandidateBorrow is not null||owner.activeLiveCheck is null||!ReferenceEquals(owner.actualSource,source))throw new InvalidOperationException("Owning active candidate source required.");return new(owner,source,check);}
        internal void Check()
        {
            if(!Monitor.IsEntered(owner.gate)||!ReferenceEquals(owner.activeCandidateBorrow,this)||!ReferenceEquals(owner.actualSource,actualSource)||!owner.sourcePrepared||owner.work?.Completed!=true)
                throw new InvalidOperationException("Exact active completed new NN/current-C23 candidate borrow required.");
            (owner.activeLiveCheck??throw new InvalidOperationException("No live ancestor chain."))();sourceCheck();
        }
        internal bool IsFor(Math2Mode0PriorSelectedScope scope){Check();return ReferenceEquals(owner.scope,scope);}
        internal Math2Mode0PriorSelectedScope Scope {get{Check();return owner.scope;}}
        internal long Reservation{get{Check();return owner.reservation;}}
        internal LegacyOwnedRawInkDocument Document{get{Check();return owner.c23!.Document;}}
        internal LegacyNativeInkPositionRange[] CopyInputRanges(){Check();return owner.c23!.Ranges.ToArray();}
        internal LegacyLibraryOriginalDomNameOwner Names=>owner.scope.CandidateNames(this);
        internal long SelectedPlan(CancellationToken token)=>owner.scope.CandidatePlan(this,token);
        internal long SelectedNext=>owner.scope.CandidateNext(this);
        internal Math2Mode0PriorRetainedInk CloneThird(CancellationToken token)=>owner.scope.CloneCandidateThird(this,token);
        internal Math2Mode0OriginalArena CloneTemporary(CancellationToken token)=>owner.scope.CloneCandidateField(this,token);
        internal object CurrentC23Identity{get{Check();return owner.c23!.Identity;}}
        internal object CurrentInputC22Identity{get{Check();return owner.c23!.InputC22Identity;}}
        internal object CurrentInkIdentity{get{Check();return owner.c23!.InkIdentity;}}
    }
    internal T WithActiveCandidate<T>(object source,Action check,Func<CandidateBorrow,T> consume,CancellationToken token)
    {
        if(!Monitor.IsEntered(gate)||activeLiveCheck is null||activeCandidateBorrow is not null||!ReferenceEquals(actualSource,source)||!sourcePrepared||work?.Completed!=true)
            throw new InvalidOperationException("Exact synchronous completed NN source under current C23 gate required.");
        activeLiveCheck();check();var borrow=CandidateBorrow.Issue(this,source,check);activeCandidateBorrow=borrow;
        try{var result=consume(borrow);borrow.Check();token.ThrowIfCancellationRequested();return result;}
        finally{activeCandidateBorrow=null;}
    }
}
internal sealed partial class Math2Mode0PriorSelectedScope
{
    private void CheckCandidate(Math2Mode0PriorC23Preparation.CandidateBorrow borrow,CancellationToken token=default)
    {
        borrow.Check();if(!Monitor.IsEntered(gate)||!borrow.IsFor(this))throw new InvalidOperationException("Exact same private selected scope gate required.");
        ObjectDisposedException.ThrowIf(disposed,this);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(borrow.CurrentInkIdentity,selectedInk.Identity)||!ReferenceEquals(borrow.Document,selectedInk.Document)||!ReferenceEquals(selectedInk.Names,names))throw new InvalidOperationException("Current selected Ink/names lost.");
    }
    internal LegacyLibraryOriginalDomNameOwner CandidateNames(Math2Mode0PriorC23Preparation.CandidateBorrow borrow){CheckCandidate(borrow);return names;}
    internal long CandidateNext(Math2Mode0PriorC23Preparation.CandidateBorrow borrow){CheckCandidate(borrow);return selectedInk.Next;}
    internal long CandidatePlan(Math2Mode0PriorC23Preparation.CandidateBorrow borrow,CancellationToken token)
    {
        CheckCandidate(borrow,token);if(selectedArena.Nodes.Count>65536||!selectedArena.HasCanonicalNames(names))throw new NotSupportedException("Whole canonical current selected field required.");
        long values=0;foreach(var n in selectedArena.Nodes){token.ThrowIfCancellationRequested();values=checked(values+Encoding.UTF8.GetByteCount(n.Text??"")+Encoding.UTF8.GetByteCount(n.Uri??"")+Encoding.UTF8.GetByteCount(n.Prefix??""));}
        return checked(selectedInk.PlanThird()+selectedArena.Nodes.Count*1024L+values*8);
    }
    internal Math2Mode0PriorRetainedInk CloneCandidateThird(Math2Mode0PriorC23Preparation.CandidateBorrow borrow,CancellationToken token)
    {CheckCandidate(borrow,token);var clone=selectedInk.CloneThird(token);CheckCandidate(borrow,token);return clone;}
    internal Math2Mode0OriginalArena CloneCandidateField(Math2Mode0PriorC23Preparation.CandidateBorrow borrow,CancellationToken token)
    {
        CheckCandidate(borrow,token);var field=selectedArena.Nodes.Single(n=>n.Kind==3&&n.Parent==0&&n.Name.Text=="mathField");
        var arena=new Math2Mode0OriginalArena(names);arena.CloneOriginal(selectedArena,field.Id,0,_=>{CheckCandidate(borrow,token);});CheckCandidate(borrow,token);return arena;
    }
}
internal sealed partial class Math2Mode0OriginalMathFieldSource
{
    private sealed class PriorThirdInk(Math2Mode0OriginalMathFieldSource sourceField, InkOwner ink, LegacyLibraryOriginalDomNameOwner actualNames, LegacyNativeInkPositionRange[]? changedDomain=null)
        : Math2Mode0PriorRetainedInk
    {
        internal override Math2Mode0PriorCandidateRegistry[] CopyCandidateRegistry()
        {
            lock(inkGate)
            {
                var aliases=new Dictionary<object,int>(ReferenceEqualityComparer.Instance);
                return ink.Registry.Values.OrderBy(e=>e.Id).Select(e=>{if(!aliases.TryGetValue(e.C22,out int id)){id=aliases.Count+1;aliases.Add(e.C22,id);}return new Math2Mode0PriorCandidateRegistry(e.Id,e.Name,e.Category,id,e.C22.Ranges.ToArray());}).ToArray();
            }
        }
        private readonly object inkGate = new();
        private bool disposed;
        internal override object Identity => ink;
        internal override LegacyLibraryOriginalDomNameOwner Names => actualNames;
        internal override long SnapshotNext { get { lock (inkGate) return ink.Next; } }
        private void Parent()
        {
            if (!Monitor.IsEntered(sourceField.gate)) throw new InvalidOperationException("Retained Ink operations require the original field gate.");
            sourceField.CheckScope();
        }
        private void Live() => ObjectDisposedException.ThrowIf(disposed, this);
        private Selection Owned(object selection)
            => selection is Selection actual && ReferenceEquals(actual.Owner, ink) ? actual
                : throw new ArgumentException("C22 must belong to this exact retained Ink clone.");
        private LegacyLibraryOriginalDomName Named(object name)
            => name is LegacyLibraryOriginalDomName actual && ReferenceEquals(actual.Owner, Names) ? actual
                : throw new ArgumentException("Canonical name must belong to the retained shared DOM name owner.");
        private Math2Mode0PublishRegistry[] Rows(bool copy)
            => ink.Registry.Values.OrderBy(e => e.Id).Select(e => new Math2Mode0PublishRegistry(e.Id, e.Name,
                e.Category, copy ? e.C22.Ranges.ToArray() : e.C22.Ranges)).ToArray();
        internal override Math2Mode0PublishRegistry[] BorrowRegistry()
        { Parent(); lock (inkGate) { Live(); return Rows(false); } }
        internal override Math2Mode0PublishRegistry[] CopyRegistry()
        { lock (inkGate) return Rows(true); }
        internal override object CreateSelection(ReadOnlySpan<LegacyNativeInkPositionRange> ranges)
        {
            Parent(); lock (inkGate)
            {
                Live();
                if (ranges.IsEmpty) throw new NotSupportedException("This current third clone admits nonempty produced ranges only.");
                foreach (var range in ranges)
                {
                    LegacyNativeSourceRangeUtf8.ValidatePosition(range.First); LegacyNativeSourceRangeUtf8.ValidatePosition(range.Last);
                    if (!LegacyOwnedRawInkRangeOperations.Contains(changedDomain??sourceField.domain, range)) throw new ArgumentException("C22 range lies outside the retained source INPUT.");
                }
                return new Selection(ink, ranges.ToArray());
            }
        }
        internal override LegacyNativeInkPositionRange[] CopySelectionRanges(object selection)
        { lock (inkGate) return Owned(selection).Ranges.ToArray(); }
        internal override long Register(LegacyLibraryOriginalDomName name, object selection)
        {
            Parent(); lock (inkGate)
            {
                Live(); var actual = Owned(selection); var canonicalName = Named(name);
                if (canonicalName.Text is not ("MATH_SYMBOL" or "MATH_NODE")) throw new NotSupportedException("Only produced Math selection names are admitted.");
                var canonical = ink.Registry.Values.Select(e => e.C22).FirstOrDefault(c =>
                    LegacyOwnedRawInkRangeOperations.Equal(c.Ranges, actual.Ranges)) ?? actual;
                long id = ink.Next++;
                ink.Registry.Add(id, new(id, canonicalName.Text, 102, ink, canonical));
                return id;
            }
        }
        public override int CompareName(object left, object right)
        { Parent(); lock (inkGate) { Live(); return string.CompareOrdinal(Named(left).Text, Named(right).Text); } }
        public override object? FindOldSelection(long id)
        { Parent(); lock (inkGate) { Live(); return ink.Registry.TryGetValue(id, out var entry) ? entry.C22 : null; } }
        public override object? FindOldName(long id)
        { Parent(); lock (inkGate) { Live(); return ink.Registry.TryGetValue(id, out var entry) ? Names.Intern(entry.Name) : null; } }
        public override int CountRanges(object selection)
        { Parent(); lock (inkGate) { Live(); return Owned(selection).Ranges.Length; } }
        public override bool EqualRanges(object oldSelection, object pendingSelection)
        { Parent(); lock (inkGate) { Live(); return LegacyOwnedRawInkRangeOperations.Equal(Owned(oldSelection).Ranges, Owned(pendingSelection).Ranges); } }
        public override bool RemoveSelection(long id)
        {
            Parent(); lock (inkGate)
            {
                Live();
                // Ink069630 returns success immediately when C51 has no ID.
                // Duplicate stale tags therefore still call removal twice,
                // with the second call succeeding as a no-op. Next never rewinds.
                ink.Registry.Remove(id); return true;
            }
        }
        public override void Dispose() { lock (inkGate) disposed = true; }
    }

    private Math2Mode0PriorRetainedInk ClonePriorThirdInk(InkOwner current,LegacyLibraryOriginalDomNameOwner names,CancellationToken token)
    {
        if(!Monitor.IsEntered(gate))throw new InvalidOperationException("Actual ancestor field gate required.");CheckScope();token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(current.Document,Document)||!ReferenceEquals(names,OriginalNameOwner))throw new InvalidOperationException("Actual prior selected Document/canonical names required.");
        var clone=new InkOwner(Document);var aliases=new Dictionary<Selection,Selection>(ReferenceEqualityComparer.Instance);long maximum=-1;
        foreach(var entry in current.Registry.Values.OrderBy(e=>e.Id))
        {
            token.ThrowIfCancellationRequested();
            if(entry.Id<0||!ReferenceEquals(entry.Owner,current)||!ReferenceEquals(entry.C22.Owner,current))throw new InvalidOperationException("Actual selected registry/C22 lost.");
            if(!aliases.TryGetValue(entry.C22,out var selection))
            {token.ThrowIfCancellationRequested();selection=new Selection(clone,entry.C22.Ranges.ToArray(),entry.C22.Storage is null);aliases.Add(entry.C22,selection);}
            clone.Registry.Add(entry.Id,new(entry.Id,entry.Name,entry.Category,clone,selection));maximum=System.Math.Max(maximum,entry.Id);
        }
        if(current.Next<=maximum)throw new InvalidOperationException("Next must exceed every live registry ID.");clone.Next=current.Next;token.ThrowIfCancellationRequested();CheckScope();return new PriorThirdInk(this,clone,names);
    }
}

internal enum Math2Mode0PriorCandidateStage { Claimed, OuterCopied, ThirdCloned, FieldCloned, CandidateRoot, Sequence, Pending, Symbol, SegmentList, Terminal, Completed }
internal sealed record Math2Mode0PriorCandidateRegistry(long Id,string Name,int Category,int CanonicalSelection,LegacyNativeInkPositionRange[] Ranges);
internal sealed record Math2Mode0PriorCandidateSnapshot(bool Complete,bool Ready,bool Failed,bool Released,long Reservation,long TemporaryNext,long RegistryNext,
    Math2Mode0PublishNode[] Candidate,Math2Mode0OriginalDomNode[] Temporary,Math2Mode0PriorCandidateRegistry[] Registry,Math2Mode0PublishPending[] Pending,long[] OldTags);
// Genuine prepared inputs to a FUTURE nonempty-old diff. No diff/action is fabricated here.
internal sealed partial class Math2Mode0PriorCandidatePreparation:IDisposable
{
    internal const long DefaultBudget=16L*1024*1024;
    private sealed class Claim{internal int Used;}
    private static readonly ConditionalWeakTable<Math2Mode0PriorRawOnlySource,Claim> Claims=new();
    private sealed class Node(int id,byte kind,LegacyLibraryOriginalDomName name)
    {internal readonly int Id=id;internal readonly byte Kind=kind;internal readonly LegacyLibraryOriginalDomName Name=name;internal int Parent;internal byte Tag;internal string? Text;internal long? Number;}
    private sealed class Pending(long id,LegacyLibraryOriginalDomName name,object? selection)
    {internal readonly long Id=id;internal readonly LegacyLibraryOriginalDomName Name=name;internal readonly object? Selection=selection;}
    private readonly object gate=new();
    private readonly Math2Mode0PriorRawOnlySource source;
    private readonly Math2Mode0FirstResultCopy actualCopy;
    private readonly Math2Mode0PriorSelectedScope scope;
    private readonly object c23,selectedInk,inputC22;
    private readonly LegacyLibraryOriginalDomNameOwner names;
    private readonly long reservation;
    private readonly List<Node> candidate=[];
    private readonly List<Pending> pending=[];
    private readonly List<long> oldTags=[];
    private Math2Mode0OuterSymbolVector? outer;
    private Math2Mode0PriorRetainedInk? third;
    private Math2Mode0OriginalArena? temporary;
    private long temporaryNext=2;
    private bool complete,ready,failed,released;
    private Math2Mode0PriorCandidatePreparation(Math2Mode0PriorRawOnlySource source,Math2Mode0FirstResultCopy copy,
        Math2Mode0PriorC23Preparation.CandidateBorrow input,LegacyLibraryOriginalDomNameOwner names,long reservation)
    {this.source=source;actualCopy=copy;scope=input.Scope;c23=input.CurrentC23Identity;selectedInk=input.CurrentInkIdentity;inputC22=input.CurrentInputC22Identity;this.names=names;this.reservation=reservation;}
    internal static Math2Mode0PriorCandidatePreparation Create(Math2Mode0PriorRawOnlySource source,CancellationToken token=default)
        =>CreateDiagnostic(source,DefaultBudget,null,token);
    internal static Math2Mode0PriorCandidatePreparation CreateDiagnostic(Math2Mode0PriorRawOnlySource source,long budget,
        Action<Math2Mode0PriorCandidateStage,Math2Mode0PriorCandidatePreparation>? observer,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(source);token.ThrowIfCancellationRequested();if(budget<0||budget>DefaultBudget)throw new ArgumentOutOfRangeException(nameof(budget));
        Math2Mode0PriorCandidatePreparation? target=null;
        try
        {
            var result=source.WithCandidateInput((input,copy)=>copy.WithSymbolRowsForOuterCopy((rows,ranges,copyReservation)=>
                {
                    if(rows.Length is<1 or>65535||rows.Length!=ranges.Length||32L+rows.LongLength*8>65536)throw new NotSupportedException("Complete bounded candidate row/node domain required.");
                    long bytes=0,totalRanges=0;
                    for(int i=0;i<rows.Length;i++)
                    {token.ThrowIfCancellationRequested();if(rows[i] is null||rows[i].ByteCount<1||rows[i].ByteCount!=rows[i].Text.ByteCount)throw new NotSupportedException("Actual nonempty first-alternative strings required.");bytes=checked(bytes+rows[i].ByteCount);totalRanges=checked(totalRanges+ranges[i].LongLength);}
                    if(totalRanges>65536)throw new NotSupportedException("Whole selected/candidate ranges bound required.");
                    long plan=checked(input.Reservation+copyReservation+input.SelectedPlan(token)+16384+rows.LongLength*2048+bytes*32+totalRanges*512);
                    if(plan>budget)throw new NotSupportedException("Combined selected/third/candidate envelope exceeds budget before claim.");
                    input.Check();if(Interlocked.CompareExchange(ref Claims.GetValue(source,_=>new()).Used,1,0)!=0)throw new InvalidOperationException("Completed NN candidate claim already consumed.");
                    target=new(source,copy,input,input.Names,plan);lock(target.gate)target.Build(input,observer,token);return target;
                },token),token);
            lock(result.gate){ObjectDisposedException.ThrowIf(result.released,result);if(result.failed||!result.complete)throw new InvalidOperationException("Candidate completion fence failed.");result.ready=true;}return result;
        }
        catch{if(target is not null){lock(target.gate){target.failed=true;if(observer is null)target.Dispose();}}throw;}
    }
    private void Build(Math2Mode0PriorC23Preparation.CandidateBorrow input,
        Action<Math2Mode0PriorCandidateStage,Math2Mode0PriorCandidatePreparation>? observe,CancellationToken token)
    {
        void Stage(Math2Mode0PriorCandidateStage stage){observe?.Invoke(stage,this);input.Check();ObjectDisposedException.ThrowIf(released,this);token.ThrowIfCancellationRequested();}
        Stage(Math2Mode0PriorCandidateStage.Claimed);
        outer=Math2Mode0OuterSymbolVector.CreateSingle(actualCopy,token);outer.ValidateDocument(input.Document,token);Stage(Math2Mode0PriorCandidateStage.OuterCopied);
        third=input.CloneThird(token);if(ReferenceEquals(third.Identity,selectedInk)||!ReferenceEquals(third.Names,names)||third.SnapshotNext!=input.SelectedNext)throw new InvalidOperationException("Actual third clone/shared names/Next required.");Stage(Math2Mode0PriorCandidateStage.ThirdCloned);
        temporary=input.CloneTemporary(token);
        var result=temporary.Children(1).Single(n=>n.Kind==3&&n.Name.Text=="mathResult");
        void Old(int id)
        {
            token.ThrowIfCancellationRequested();foreach(var n in temporary.Children(id).Where(n=>n.Kind==0&&n.Name.Text=="tag"))
            {if(n.Tag!=4||n.Number is not long value||value<0||third.FindOldSelection(value)is null)throw new NotSupportedException("Actual registered old typed tags required.");oldTags.Add(value);}
            foreach(var n in temporary.Children(id).Where(n=>n.Kind!=0))Old(n.Id);
        }
        Old(result.Id);if(oldTags.Count==0||!temporary.HasCanonicalNames(names))throw new NotSupportedException("Genuine existing old mathResult/registered tag cohort required.");Stage(Math2Mode0PriorCandidateStage.FieldCloned);
        Node New(byte kind,string name,int parent=0){var n=new Node(candidate.Count+1,kind,names.Intern(name)){Parent=parent};candidate.Add(n);return n;}
        static byte StringTag(string text)=>text.Length==0?(byte)3:Encoding.UTF8.GetByteCount(text)<16?(byte)2:(byte)1;
        void Attribute(Node parent,string name,byte tag,string? text=null,long? number=null){var n=New(0,name,parent.Id);n.Tag=tag;n.Text=text;n.Number=number;}
        var operations=LegacyOwnedRawInkRangeOperations.Prepare(input.Document);
        string Format(LegacyNativeInkPositionRange[] ranges){var r=LegacyNativeSourceRangeUtf8.Format(ranges,operations,token:token);if(!r.Succeeded)throw new InvalidOperationException("C21 serialization failed.");return Encoding.UTF8.GetString(r.Bytes);}
        void String(Node parent,string name,string value)=>Attribute(parent,name,StringTag(value),value);
        long Register(string name,LegacyNativeInkPositionRange[] ranges)
        {long id=temporaryNext++;var p=new Pending(id,names.Intern(name),ranges.Length==0?null:third.CreateSelection(ranges));pending.Add(p);Stage(Math2Mode0PriorCandidateStage.Pending);return id;}
        var document=New(4,"");var root=New(3,"mathResult",document.Id);Stage(Math2Mode0PriorCandidateStage.CandidateRoot);
        var sequence=New(3,"sequence",root.Id);Stage(Math2Mode0PriorCandidateStage.Sequence);
        foreach(var row in outer.CopySymbols())
        {
            token.ThrowIfCancellationRequested();var symbol=New(3,"symbol");
            if(row.Ranges.Length!=0){String(symbol,"range",Format(row.Ranges));long id=Register("MATH_SYMBOL",row.Ranges);Attribute(symbol,"tag",4,number:-id);}
            var text=New(1,"",symbol.Id);byte[] raw=row.Alternatives[0].Text.ReadBytes();int nul=Array.IndexOf(raw,(byte)0);text.Text=new UTF8Encoding(false,true).GetString(raw,0,nul<0?raw.Length:nul);text.Tag=StringTag(text.Text);symbol.Parent=sequence.Id;Stage(Math2Mode0PriorCandidateStage.Symbol);
        }
        string full=Format(input.CopyInputRanges());var list=New(3,"symbolSegmentList",root.Id);var segment=New(3,"symbolSegment",list.Id);String(segment,"range",full);
        var choice=New(3,"symbolCandidate",segment.Id);Attribute(choice,"nrs",10,number:0);var character=New(3,"char",choice.Id);var question=New(1,"",character.Id);question.Tag=2;question.Text="?";Stage(Math2Mode0PriorCandidateStage.SegmentList);
        var terminal=New(3,"mathTerminalNode",root.Id);String(terminal,"name","");String(terminal,"ref",full);Attribute(terminal,"selected",7,number:0);long terminalId=Register("MATH_NODE",input.CopyInputRanges());Attribute(terminal,"tag",4,number:-terminalId);Stage(Math2Mode0PriorCandidateStage.Terminal);
        if(third.SnapshotNext!=input.SelectedNext||!temporary.HasCanonicalNames(names)||candidate.Any(n=>!ReferenceEquals(n.Name.Owner,names)))throw new InvalidOperationException("Pre-diff source/name/registry state changed.");
        complete=true;Stage(Math2Mode0PriorCandidateStage.Completed);
    }
    private T WithLive<T>(Func<T> consume,CancellationToken token)
        =>source.WithCandidateInput((input,copy)=>
        {
            lock(gate)
            {
                void Check(){input.Check();ObjectDisposedException.ThrowIf(released,this);if(failed||!complete||!ready||!ReferenceEquals(copy,actualCopy)||!ReferenceEquals(input.Scope,scope)||!ReferenceEquals(input.CurrentC23Identity,c23)||!ReferenceEquals(input.CurrentInkIdentity,selectedInk)||!ReferenceEquals(input.CurrentInputC22Identity,inputC22)||!ReferenceEquals(input.Names,names))throw new InvalidOperationException("Same ready pre-diff candidate cohort required.");}
                Check();var value=consume();Check();return value;
            }
        },token);
    internal object ThirdInkIdentity=>WithLive(()=>third!.Identity,default);
    internal object ThirdSelectionIdentity(long id)=>WithLive(()=>third!.FindOldSelection(id)??throw new KeyNotFoundException(),default);
    internal bool HasCanonicalNames=>WithLive(()=>temporary!.HasCanonicalNames(names)&&candidate.All(n=>ReferenceEquals(n.Name,names.Intern(n.Name.Text))),default);
    internal Math2Mode0PriorCandidateSnapshot CopyResult(CancellationToken token=default)=>WithLive(CopySnapshot,token);
    internal Math2Mode0PriorCandidateSnapshot CopySnapshot()
    {
        lock(gate)
        {
            var registry=third is not null&&!released?third.CopyCandidateRegistry():[];
            return new(complete,ready,failed,released,reservation,temporaryNext,third?.SnapshotNext??-1,
                candidate.Select(n=>new Math2Mode0PublishNode(n.Id,n.Kind,n.Name.Text,n.Parent,0,n.Tag,n.Text,n.Number,null,null)).ToArray(),temporary?.Snapshot()??[],registry,
                pending.Select(p=>new Math2Mode0PublishPending(p.Id,p.Name.Text,-1,p.Selection is not null,p.Selection is null?[]:third!.CopySelectionRanges(p.Selection))).ToArray(),oldTags.ToArray());
        }
    }
    public void Dispose(){lock(gate){if(released)return;released=true;outer?.Dispose();third?.Dispose();}}
}
