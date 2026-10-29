using System.Text;
namespace InkRecognition.Math;

internal sealed partial class Math2Mode0PriorSelectedScope
{
    private readonly Math2Mode0CurrentInputReceipt? changedReceipt;
    private Math2Mode0OriginalApplication.CurrentInputBorrow? activeChangedBorrow;
    private Math2Mode0PriorSelectedScope(Math2Mode0OriginalApplication.CurrentInputBorrow borrow,
        Math2Mode0CurrentInputReceipt receipt,Math2Mode0OriginalArena selected,Math2Mode0PriorInk ink,long bytes)
    {
        borrow.CheckReceipt(receipt);parent=borrow.Owner;originalArena=borrow.AppliedArenaIdentity;
        originalInk=borrow.OriginalInkIdentity;names=borrow.Names;epoch=borrow.AppliedEpoch;
        selectedArena=selected;selectedInk=ink;reservation=bytes;changedReceipt=receipt;
        if(ReferenceEquals(originalInk,ink.Identity)||!ReferenceEquals(ink.Document,borrow.Document)||
            !ReferenceEquals(ink.Names,names)||!selected.HasCanonicalNames(names))
            throw new InvalidOperationException("Genuine current-selected clone/child projection/names required.");
    }
    internal static Math2Mode0PriorSelectedScope CaptureChanged(Math2Mode0OriginalApplication.CurrentInputBorrow borrow,
        Math2Mode0CurrentInputReceipt receipt,long budget,CancellationToken token)
    {
        borrow.CheckReceipt(receipt);long plan=borrow.PlanSelected(budget,token);
        var ink=borrow.CloneCurrentInk(token);var arena=borrow.CloneCurrentField(token);
        borrow.CheckReceipt(receipt);token.ThrowIfCancellationRequested();return new(borrow,receipt,arena,ink,plan);
    }
    private T WithChangedFreezeInput<T>(Func<FreezeBorrow,T> consume,CancellationToken token)
        =>WithCurrent(()=>
        {
            void Check()
            {
                ObjectDisposedException.ThrowIf(disposed,this);token.ThrowIfCancellationRequested();
                (activeChangedBorrow??throw new InvalidOperationException("Current ancestor borrow required.")).CheckReceipt(changedReceipt??throw new InvalidOperationException());
            }
            Check();var input=FreezeBorrow.Issue(this,Check);activeFreezeBorrow=input;
            try{var result=consume(input);input.Check();return result;}finally{activeFreezeBorrow=null;}
        },token);
    private T WithChangedCurrent<T>(Func<T> inspect,CancellationToken token)
        =>parent.WithChangedInput(changedReceipt??throw new InvalidOperationException(),borrow=>
        {
            lock(gate)
            {
                ObjectDisposedException.ThrowIf(disposed,this);borrow.CheckReceipt(changedReceipt);
                if(!ReferenceEquals(borrow.AppliedArenaIdentity,originalArena)||!ReferenceEquals(borrow.OriginalInkIdentity,originalInk)||
                    !ReferenceEquals(borrow.Names,names)||!ReferenceEquals(borrow.Document,selectedInk.Document)||borrow.AppliedEpoch!=epoch||activeChangedBorrow is not null)
                    throw new InvalidOperationException("Current-selected ancestor/epoch/projection changed.");
                activeChangedBorrow=borrow;
                try{var result=inspect();borrow.Check();return result;}finally{activeChangedBorrow=null;}
            }
        },token);
}
internal sealed partial class Math2Mode0OriginalApplication
{
    internal Math2Mode0PriorSelectedScope CreateCurrentSelectedScope(Math2Mode0CurrentInputReceipt receipt,
        long budget=Math2Mode0PriorSelectedScope.DefaultBudget,CancellationToken token=default)
        =>WithChangedInput(receipt,b=>Math2Mode0PriorSelectedScope.CaptureChanged(b,receipt,budget,token),token);
    internal CurrentInputBorrow RequireCurrentInput(Math2Mode0CurrentInputReceipt receipt)
    {
        if(!Monitor.IsEntered(gate)||currentInputBorrow is null)throw new InvalidOperationException("Actual active current-input ancestor required.");
        currentInputBorrow.CheckReceipt(receipt);return currentInputBorrow;
    }
    internal sealed partial class CurrentInputBorrow
    {
        internal Math2Mode0OriginalApplication Owner {get{Check();return owner;}}
        internal LegacyLibraryOriginalDomNameOwner Names {get{Check();return owner.originalNames;}}
        internal long AppliedEpoch {get{Check();return epoch;}}
        internal Math2Mode0CurrentInputReceipt Receipt {get{Check();return handle.Receipt;}}
        internal void CheckReceipt(Math2Mode0CurrentInputReceipt receipt)
        {Check();if(!ReferenceEquals(receipt,handle.Receipt))throw new ArgumentException("Foreign current-input receipt.");}
        internal void CheckSource(Math2Mode0OriginalMathFieldSource source,object ink)
        {Check();if(!ReferenceEquals(input.Source,source)||!ReferenceEquals(input.Ink,ink))throw new ArgumentException("Foreign current source/Ink.");}
        internal LegacyOwnedCurrentRawIdentityExport ExportIdentity(ReadOnlySpan<LegacyNativeInkPositionRange> ranges,CancellationToken token)
        {Check();var result=raw.ExportIdentityTransforms(ranges,token);Check();return result;}
        internal long PlanSelected(long budget,CancellationToken token)
        {
            Check();if(budget<0||budget>Math2Mode0PriorSelectedScope.DefaultBudget)throw new ArgumentOutOfRangeException(nameof(budget));
            long nodes=0,values=0,ranges=0;var unique=new HashSet<object>(ReferenceEqualityComparer.Instance);
            void Walk(int id,int depth)
            {
                token.ThrowIfCancellationRequested();if(depth>240||++nodes>65536)throw new NotSupportedException("Bounded current DOM required.");
                var n=owner.active.Nodes[id-1];values=checked(values+Encoding.UTF8.GetByteCount(n.Name.Text)+Encoding.UTF8.GetByteCount(n.Text??"")+Encoding.UTF8.GetByteCount(n.Uri??"")+Encoding.UTF8.GetByteCount(n.Prefix??""));
                foreach(var child in owner.active.Children(id))Walk(child.Id,depth+1);
            }
            Walk(owner.originalField,0);var rows=input.ReadRegistry();
            if(rows.Length is <1 or >65536)throw new NotSupportedException("Bounded current registry required.");
            foreach(var row in rows){token.ThrowIfCancellationRequested();if(unique.Add(row.C22))ranges=checked(ranges+row.Ranges.LongLength);}
            if(ranges>65536||values>2L*1024*1024)throw new NotSupportedException("Bounded current payload required.");
            long plan=checked(raw.ClonePayloadReservation+8192+nodes*1024+rows.LongLength*512+ranges*128+values*8);
            if(plan>budget)throw new NotSupportedException("Current-selected clone budget before copy.");Check();return plan;
        }
        internal Math2Mode0PriorInk CloneCurrentInk(CancellationToken token)
        {Check();return input.Source.CloneChangedOriginalForPrior(this,token);}
        internal Math2Mode0OriginalArena CloneCurrentField(CancellationToken token)
        {
            Check();var arena=new Math2Mode0OriginalArena(owner.originalNames);
            arena.CloneOriginal(owner.active,owner.originalField,0,_=>token.ThrowIfCancellationRequested());Check();return arena;
        }
    }
}
internal sealed partial class Math2Mode0OriginalMathFieldSource
{
    private sealed class ChangedPriorInk(InkOwner ink,LegacyLibraryOriginalDomNameOwner names,
        Math2Mode0OriginalMathFieldSource source,Math2Mode0OriginalApplication app,Math2Mode0CurrentInputReceipt receipt):Math2Mode0PriorInk
    {
        internal override object Identity=>ink;
        internal override LegacyOwnedRawInkDocument Document=>ink.Document;
        internal override LegacyLibraryOriginalDomNameOwner Names=>names;
        internal override long Next=>ink.Next;
        internal override int MaximumRangeCount=>ink.Registry.Values.Max(e=>e.C22.Ranges.Length);
        internal override long PlanThird()=>checked(8192L+ink.Registry.Count*512L+ink.Registry.Values.Sum(e=>e.C22.Ranges.LongLength)*128L);
        internal override Math2Mode0PriorRetainedInk CloneThird(CancellationToken token)
            =>source.CloneChangedPriorThirdInk(ink,names,app,receipt,token);
        internal override object SelectionIdentity(long id)=>ink.Registry[id].C22;
        internal override ReadOnlySpan<LegacyNativeInkPositionRange> BorrowSelectionRanges(object selection)
            =>selection is Selection actual&&ReferenceEquals(actual.Owner,ink)?actual.Ranges:throw new ArgumentException("Same current-selected C22 required.");
        internal override Math2Mode0OriginalRegistryRow[] Snapshot()
        {
            var ids=new Dictionary<object,int>(ReferenceEqualityComparer.Instance);
            return ink.Registry.Values.OrderBy(e=>e.Id).Select(e=>
            {if(!ids.TryGetValue(e.C22,out int id)){id=ids.Count+1;ids.Add(e.C22,id);}return new Math2Mode0OriginalRegistryRow(e.Id,e.Name,e.Category,e.C22.Ranges.ToArray(),id);}).ToArray();
        }
        internal override Math2Mode0CurrentC23 SelectInput(long id,CancellationToken token)
            =>throw new NotSupportedException("A changed selected Ink requires its actual current-input borrow.");
        internal override Math2Mode0CurrentC23 SelectChangedInput(long id,Math2Mode0OriginalApplication.CurrentInputBorrow borrow,CancellationToken token)
        {
            borrow.CheckReceipt(receipt);borrow.CheckSource(source,source.originalInk);
            if(!ReferenceEquals(borrow.Owner,app)||!ReferenceEquals(borrow.Document,ink.Document)||
                !ink.Registry.TryGetValue(id,out var input)||input.Name!="INPUT"||input.Category!=102||
                !ReferenceEquals(input.Owner,ink)||!ReferenceEquals(input.C22.Owner,ink))
                throw new InvalidOperationException("Actual current-selected INPUT102 required.");
            return new ChangedCurrentC23(ink,input,borrow,token);
        }
    }
    private sealed class ChangedCurrentC23:Math2Mode0CurrentC23
    {
        private readonly InkOwner ink;private readonly RegistryEntry input;
        private readonly LegacyNativeInkPositionRange[] ranges;private readonly Math2Mode0IdentityTransforms transforms;
        private readonly bool full;
        internal ChangedCurrentC23(InkOwner ink,RegistryEntry input,Math2Mode0OriginalApplication.CurrentInputBorrow borrow,CancellationToken token)
        {
            borrow.Check();this.ink=ink;this.input=input;
            var doc=borrow.Document;var domain=new LegacyNativeInkPositionRange[]{new(
                LegacyNativeInkPosition.Pack(0,0,3,doc.Strokes[0].Points.Count),
                LegacyNativeInkPosition.Pack((uint)(doc.ObjectCount-1),doc.Strokes[^1].Points.Count-1,5,doc.Strokes[^1].Points.Count))};
            ranges=input.C22.Storage is null&&input.C22.Ranges.Length==0?[]:
                LegacyOwnedRawInkRangeOperations.Prepare(doc).Intersect(domain,input.C22.Ranges,token);
            full=ranges.AsSpan().SequenceEqual(domain);
            if(!full)throw new NotSupportedException("Full actual current INPUT required; partial C23 is not this route.");
            var export=borrow.ExportIdentity(ranges,token);
            if(!ReferenceEquals(export.Document,ink.Document))throw new InvalidOperationException("Current transform/projection owner mismatch.");
            transforms=Math2Mode0IdentityTransforms.Current(export);borrow.Check();
        }
        internal override object Identity=>this;
        internal override object InkIdentity=>ink;
        internal override object InputC22Identity=>input.C22;
        internal override long InputId=>input.Id;
        internal override LegacyOwnedRawInkDocument Document=>ink.Document;
        internal override ReadOnlySpan<LegacyNativeInkPositionRange> Ranges=>ranges;
        internal override Math2Mode0IdentityTransforms IdentityTransforms=>transforms;
        internal override bool FullDomain=>full;
    }
    internal Math2Mode0PriorInk CloneChangedOriginalForPrior(Math2Mode0OriginalApplication.CurrentInputBorrow borrow,CancellationToken token)
    {
        if(!Monitor.IsEntered(gate))throw new InvalidOperationException("Actual current field gate required.");
        CheckScope();borrow.CheckSource(this,originalInk);var clone=new InkOwner(borrow.Document);
        var aliases=new Dictionary<Selection,Selection>(ReferenceEqualityComparer.Instance);long maximum=-1;
        foreach(var entry in originalInk.Registry.Values.OrderBy(e=>e.Id))
        {
            token.ThrowIfCancellationRequested();borrow.Check();
            if(entry.Id<0||!ReferenceEquals(entry.Owner,originalInk)||!ReferenceEquals(entry.C22.Owner,originalInk))throw new InvalidOperationException("Current original C22 owner lost.");
            if(!aliases.TryGetValue(entry.C22,out var copy))
            {copy=new Selection(clone,entry.C22.Ranges.ToArray(),entry.C22.Storage is null);aliases.Add(entry.C22,copy);}
            clone.Registry.Add(entry.Id,new(entry.Id,entry.Name,entry.Category,clone,copy));maximum=System.Math.Max(maximum,entry.Id);
        }
        if(originalInk.Next<=maximum)throw new InvalidOperationException("Real current Next must exceed live IDs.");
        clone.Next=originalInk.Next;borrow.Check();token.ThrowIfCancellationRequested();return new ChangedPriorInk(clone,OriginalNameOwner,this,borrow.Owner,borrow.Receipt);
    }
    private Math2Mode0PriorRetainedInk CloneChangedPriorThirdInk(InkOwner current,LegacyLibraryOriginalDomNameOwner names,
        Math2Mode0OriginalApplication app,Math2Mode0CurrentInputReceipt receipt,CancellationToken token)
    {
        if(!Monitor.IsEntered(gate))throw new InvalidOperationException("Actual original field gate required.");
        var borrow=app.RequireCurrentInput(receipt);borrow.CheckSource(this,originalInk);
        if(!ReferenceEquals(current.Document,borrow.Document)||!ReferenceEquals(names,OriginalNameOwner))throw new ArgumentException("Current-selected document/names required.");
        var clone=new InkOwner(current.Document);var aliases=new Dictionary<Selection,Selection>(ReferenceEqualityComparer.Instance);long maximum=-1;
        foreach(var entry in current.Registry.Values.OrderBy(e=>e.Id))
        {
            token.ThrowIfCancellationRequested();borrow.Check();
            if(entry.Id<0||!ReferenceEquals(entry.Owner,current)||!ReferenceEquals(entry.C22.Owner,current))throw new InvalidOperationException("Current-selected C22 owner lost.");
            if(!aliases.TryGetValue(entry.C22,out var copy))
            {copy=new Selection(clone,entry.C22.Ranges.ToArray(),entry.C22.Storage is null);aliases.Add(entry.C22,copy);}
            clone.Registry.Add(entry.Id,new(entry.Id,entry.Name,entry.Category,clone,copy));maximum=System.Math.Max(maximum,entry.Id);
        }
        if(current.Next<=maximum)throw new InvalidOperationException("Current-selected Next must exceed live IDs.");clone.Next=current.Next;
        var doc=borrow.Document;var domain=new LegacyNativeInkPositionRange[]{new(
            LegacyNativeInkPosition.Pack(0,0,3,doc.Strokes[0].Points.Count),
            LegacyNativeInkPosition.Pack((uint)(doc.ObjectCount-1),doc.Strokes[^1].Points.Count-1,5,doc.Strokes[^1].Points.Count))};
        borrow.Check();return new PriorThirdInk(this,clone,names,domain);
    }
}

internal sealed record Math2Mode0CurrentIdentitySnapshot(int Matrices,int Nonidentity,byte[] MatrixBytes);
internal sealed partial class Math2Mode0PriorC23Preparation
{
    internal Math2Mode0CurrentIdentitySnapshot CopyIdentitySnapshot(CancellationToken token=default)
        =>WithLive(()=>new Math2Mode0CurrentIdentitySnapshot(c23!.IdentityTransforms.MatricesResolved,
            c23.IdentityTransforms.NativeNonidentityTransformCount,c23.IdentityTransforms.CopyResolvedMatrices()),token);
}
