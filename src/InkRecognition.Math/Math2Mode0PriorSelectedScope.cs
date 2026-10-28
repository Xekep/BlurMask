namespace InkRecognition.Math;

// Bounded ownership of a selected copy of an APPLIED result.
// No inference/publication/diff API is supplied by this scope.
internal abstract class Math2Mode0PriorInk
{
    internal abstract object Identity { get; }
    internal abstract LegacyOwnedRawInkDocument Document { get; }
    internal abstract Math2Mode0CurrentC23 SelectInput(long inputId,CancellationToken token);
    internal abstract LegacyLibraryOriginalDomNameOwner Names { get; }
    internal abstract long Next { get; }
    internal abstract Math2Mode0OriginalRegistryRow[] Snapshot();
    internal abstract object SelectionIdentity(long id);
    internal abstract int MaximumRangeCount { get; }
    internal abstract ReadOnlySpan<LegacyNativeInkPositionRange> BorrowSelectionRanges(object selection);
}

internal sealed record Math2Mode0PriorSnapshot(long Epoch, long Reservation, long Next,
    Math2Mode0OriginalDomNode[] SelectedField, Math2Mode0OriginalRegistryRow[] Registry);

internal sealed partial class Math2Mode0PriorSelectedScope : IDisposable
{
    internal const long DefaultBudget = 16L * 1024 * 1024;
    private readonly object gate = new();
    private readonly Math2Mode0OriginalApplication parent;
    private readonly object originalArena;
    private readonly Math2Mode0OriginalArena selectedArena;
    private readonly object originalInk;
    private readonly Math2Mode0PriorInk selectedInk;
    private readonly LegacyLibraryOriginalDomNameOwner names;
    private readonly long epoch, reservation;
    private bool disposed;
    private Math2Mode0PriorSelectedScope(Math2Mode0OriginalApplication.PriorBorrow borrow,
        Math2Mode0OriginalArena selected, Math2Mode0PriorInk ink, long bytes)
    {
        borrow.Check(); parent=borrow.Owner; originalArena=borrow.ArenaIdentity;
        originalInk=borrow.InkIdentity; names=borrow.Names; epoch=borrow.Epoch;
        selectedArena=selected; selectedInk=ink; reservation=bytes;
        if(ReferenceEquals(originalInk,ink.Identity)||!ReferenceEquals(ink.Names,names)||!selected.HasCanonicalNames(names))
            throw new InvalidOperationException("Distinct selected clone/shared current names required.");
    }
    internal static Math2Mode0PriorSelectedScope Capture(Math2Mode0OriginalApplication.PriorBorrow borrow,
        long budget, CancellationToken token)
    {
        borrow.Check(); token.ThrowIfCancellationRequested();
        long planned=borrow.Plan(budget,token); // No DOM/C22 clone before admission.
        var ink=borrow.CloneCurrentInk(token);
        var arena=borrow.CloneCurrentField(token);
        borrow.Check(); token.ThrowIfCancellationRequested();
        return new(borrow,arena,ink,planned);
    }
    // No own gate is held when entering the ancestor chain.
    internal T WithCurrent<T>(Func<T> inspect,CancellationToken token=default)
        =>parent.WithCompletedOriginalInput(borrow=>
        {
            lock(gate)
            {
                ObjectDisposedException.ThrowIf(disposed,this);
                borrow.Check();
                if(!ReferenceEquals(borrow.ArenaIdentity,originalArena)||!ReferenceEquals(borrow.InkIdentity,originalInk)||
                    !ReferenceEquals(borrow.Names,names)||borrow.Epoch!=epoch)
                    throw new InvalidOperationException("Prior scope no longer matches the applied epoch.");
                return inspect();
            }
        },token);
    internal Math2Mode0PriorSnapshot CopySnapshot(CancellationToken token=default)
        =>WithCurrent(()=>new Math2Mode0PriorSnapshot(epoch,reservation,selectedInk.Next,selectedArena.Snapshot(),selectedInk.Snapshot()),token);
    internal object SelectedInkIdentity=>WithCurrent(()=>selectedInk.Identity);
    internal object SelectedC22Identity(long id)=>WithCurrent(()=>selectedInk.SelectionIdentity(id));
    internal LegacyLibraryOriginalDomNameOwner NameOwner=>WithCurrent(()=>names);
    internal bool IsFrom(Math2Mode0OriginalApplication actual)=>ReferenceEquals(parent,actual);
    public void Dispose(){lock(gate)disposed=true;}
}

internal sealed partial class Math2Mode0OriginalApplication
{
    private PriorBorrow? activePriorBorrow;
    private bool priorReady; // Set only AFTER ApplyDom completion observer returns.
    private sealed class ArenaWitness
    {internal readonly Math2Mode0OriginalArena Arena;internal ArenaWitness(Math2Mode0OriginalArena arena){Arena=arena;}}
    private ArenaWitness? priorArenaWitness;
    // Private-constructor capability. The reference must also be the currently
    // issued borrow, under the actual field/application gates; finally revokes it.
    internal sealed class PriorBorrow
    {
        private readonly Math2Mode0OriginalApplication owner;
        private readonly Math2Mode0OriginalInput input;
        private readonly Math2Mode0OriginalTransferInput transfer;
        private readonly long epoch;
        internal Math2Mode0OriginalApplication Owner=>owner;
        internal object ArenaIdentity { get { Check();return owner.priorArenaWitness??=new ArenaWitness(owner.active); } }
        internal object InkIdentity { get { Check();return input.Ink; } }
        internal LegacyLibraryOriginalDomNameOwner Names { get { Check();return owner.originalNames; } }
        internal long Epoch { get { Check();return epoch; } }
        private PriorBorrow(Math2Mode0OriginalApplication owner,Math2Mode0OriginalInput input,Math2Mode0OriginalTransferInput transfer)
        {this.owner=owner;this.input=input;this.transfer=transfer;epoch=owner.epoch;}
        internal static PriorBorrow Issue(Math2Mode0OriginalApplication owner,Math2Mode0OriginalInput input,Math2Mode0OriginalTransferInput transfer)
        {
            if(!Monitor.IsEntered(owner.gate)||owner.activePriorBorrow is not null||!ReferenceEquals(owner.field,input.Source)||!ReferenceEquals(owner.actualInk,input.Ink))
                throw new InvalidOperationException("Only the actual application may issue a pending borrow.");
            input.Check(default);transfer.Check(default);return new(owner,input,transfer);
        }
        internal void Check()
        {
            if(!Monitor.IsEntered(owner.gate)||!ReferenceEquals(owner.activePriorBorrow,this))
                throw new InvalidOperationException("Exact current application borrow required.");
            input.Check(default);transfer.Check(default);
            if(owner.released||owner.failed||!owner.complete||!owner.priorReady||!owner.sourceSucceeded||!owner.journalComplete||
                !owner.domEntered||owner.rolledBack||owner.epoch!=epoch||epoch<1||!ReferenceEquals(owner.actualInk,input.Ink))
                throw new InvalidOperationException("Completed applied original result required.");
        }
        internal void CheckSource(Math2Mode0OriginalMathFieldSource source,object ink)
        {Check();if(!ReferenceEquals(input.Source,source)||!ReferenceEquals(input.Ink,ink))throw new ArgumentException("Foreign source/Ink.");}
        internal long Plan(long budget,CancellationToken token)
        {
            Check();if(budget<0||budget>Math2Mode0PriorSelectedScope.DefaultBudget)throw new ArgumentOutOfRangeException(nameof(budget));
            long nodes=0,values=0,ranges=0;var unique=new HashSet<object>(ReferenceEqualityComparer.Instance);
            void Walk(int id,int depth)
            {
                token.ThrowIfCancellationRequested();if(depth>240||++nodes>65536)throw new NotSupportedException("Prior DOM bound.");
                var n=owner.active.Nodes[id-1];values=checked(values+System.Text.Encoding.UTF8.GetByteCount(n.Name.Text)+System.Text.Encoding.UTF8.GetByteCount(n.Text??"")+System.Text.Encoding.UTF8.GetByteCount(n.Uri??"")+System.Text.Encoding.UTF8.GetByteCount(n.Prefix??""));
                foreach(var c in owner.active.Children(id))Walk(c.Id,depth+1);
            }
            Walk(owner.originalField,0);
            var rows=input.ReadRegistry();if(rows.Length is<1 or>65536)throw new NotSupportedException("Prior registry bound.");
            foreach(var row in rows){token.ThrowIfCancellationRequested();if(unique.Add(row.C22))ranges=checked(ranges+row.Ranges.LongLength);}
            if(ranges>65536||values>2L*1024*1024)throw new NotSupportedException("Prior payload bound.");
            long planned=checked(8192+nodes*1024+rows.LongLength*512+ranges*128+values*8);
            if(planned>budget)throw new NotSupportedException("Whole prior scope exceeds budget before copies.");return planned;
        }
        internal Math2Mode0PriorInk CloneCurrentInk(CancellationToken token){Check();token.ThrowIfCancellationRequested();return input.Source.CloneCurrentOriginalForPrior(this,token);}
        internal Math2Mode0OriginalArena CloneCurrentField(CancellationToken token)
        {Check();var arena=new Math2Mode0OriginalArena(owner.originalNames);arena.CloneOriginal(owner.active,owner.originalField,0,_=>token.ThrowIfCancellationRequested());Check();return arena;}
        internal object OriginalC22Identity(long id){Check();return input.ReadRegistry().Single(r=>r.Id==id).C22;}
    }
    internal T WithCompletedOriginalInput<T>(Func<PriorBorrow,T> consume,CancellationToken token=default)
        =>WithLive((input,transfer)=>
        {
            token.ThrowIfCancellationRequested();
            if(!complete||!priorReady||!sourceSucceeded||!journalComplete||!domEntered||rolledBack||epoch<1||activePriorBorrow is not null)
                throw new InvalidOperationException("Exclusive completed applied original borrow required.");
            Math2Mode0OriginalKernel.ValidateActiveRegistry(input,transfer,registry);
            if(input.Next()!=activeNext||!ReferenceEquals(input.Names,originalNames)||!active.HasCanonicalNames(originalNames))
                throw new InvalidOperationException("Current original registry/names changed.");
            var fieldNode=active.Nodes[originalField-1];
            if(fieldNode.Kind!=3||fieldNode.Name.Text!="mathField"||active.Children(originalField).Count(n=>n.Kind==3&&n.Name.Text=="mathResult")!=1)
                throw new InvalidOperationException("Actual applied mathResult child required.");
            Math2Mode0OriginalKernel.ValidateTags(active,originalField,registry,input.InputRanges,token);
            var borrow=PriorBorrow.Issue(this,input,transfer);activePriorBorrow=borrow;
            try{var result=consume(borrow);borrow.Check();token.ThrowIfCancellationRequested();return result;}
            finally{activePriorBorrow=null;}
        },token);
    internal Math2Mode0PriorSelectedScope CreatePriorSelectedScope(long budget=Math2Mode0PriorSelectedScope.DefaultBudget,CancellationToken token=default)
        =>WithCompletedOriginalInput(b=>Math2Mode0PriorSelectedScope.Capture(b,budget,token),token);
}

internal sealed partial class Math2Mode0OriginalMathFieldSource
{
    private sealed class PriorInk(InkOwner ink,LegacyLibraryOriginalDomNameOwner names,Math2Mode0OriginalMathFieldSource sourceField):Math2Mode0PriorInk
    {
        internal override object Identity=>ink;
        internal override LegacyOwnedRawInkDocument Document=>ink.Document;
        internal override Math2Mode0CurrentC23 SelectInput(long inputId,CancellationToken token)
        {
            if(!ink.Registry.TryGetValue(inputId,out var entry)||entry.Name!="INPUT"||entry.Category!=102||!ReferenceEquals(entry.Owner,ink)||!ReferenceEquals(entry.C22.Owner,ink))throw new InvalidOperationException("Actual current-selected INPUT102 required.");
            return new CurrentC23(ink,entry,sourceField,token);
        }
        internal override LegacyLibraryOriginalDomNameOwner Names=>names;
        internal override long Next=>ink.Next;
        internal override object SelectionIdentity(long id)=>ink.Registry[id].C22;
        internal override int MaximumRangeCount=>ink.Registry.Values.Max(e=>e.C22.Ranges.Length);
        internal override ReadOnlySpan<LegacyNativeInkPositionRange> BorrowSelectionRanges(object selection)
            =>selection is Selection actual&&ReferenceEquals(actual.Owner,ink)?actual.Ranges:throw new ArgumentException("Exact prior selected C22 required.");
        internal override Math2Mode0OriginalRegistryRow[] Snapshot()
        {
            var canonical=new Dictionary<object,int>(ReferenceEqualityComparer.Instance);
            return ink.Registry.Values.OrderBy(e=>e.Id).Select(e=>
            {if(!canonical.TryGetValue(e.C22,out int id)){id=canonical.Count+1;canonical.Add(e.C22,id);}return new Math2Mode0OriginalRegistryRow(e.Id,e.Name,e.Category,e.C22.Ranges.ToArray(),id);}).ToArray();
        }
    }
    internal Math2Mode0PriorInk CloneCurrentOriginalForPrior(Math2Mode0OriginalApplication.PriorBorrow borrow,CancellationToken token=default)
    {
        if(!Monitor.IsEntered(gate))throw new InvalidOperationException("Original field gate required.");
        CheckScope();token.ThrowIfCancellationRequested();borrow.CheckSource(this,originalInk);
        var clone=new InkOwner(Document);
        var aliases=new Dictionary<Selection,Selection>(ReferenceEqualityComparer.Instance);
        long maximum=-1;
        foreach(var entry in originalInk.Registry.Values.OrderBy(e=>e.Id))
        {
            token.ThrowIfCancellationRequested();
            if(entry.Id<0||!ReferenceEquals(entry.Owner,originalInk)||!ReferenceEquals(entry.C22.Owner,originalInk))throw new InvalidOperationException("Current original C22 owner lost.");
            if(!aliases.TryGetValue(entry.C22,out var selection))
            {selection=new Selection(clone,entry.C22.Ranges.ToArray(),entry.C22.Storage is null);aliases.Add(entry.C22,selection);}
            clone.Registry.Add(entry.Id,new(entry.Id,entry.Name,entry.Category,clone,selection));maximum=System.Math.Max(maximum,entry.Id);
        }
        if(originalInk.Next<=maximum)throw new InvalidOperationException("Current original Next must exceed all live IDs.");
        clone.Next=originalInk.Next;token.ThrowIfCancellationRequested();borrow.CheckSource(this,originalInk);
        return new PriorInk(clone,OriginalNameOwner,this);
    }
}
