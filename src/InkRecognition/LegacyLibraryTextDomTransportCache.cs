using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyLibraryTextDomTransport.Probe")]
namespace InkRecognition;

/// <summary>Explicit library transport cache for one actual BE2/pending-C4D
/// pair. CreateFresh(target) does not construct05F720's native receiver A,
/// execute C20 history or accept a recognized document.</summary>
public sealed class LegacyLibraryTextDomTransportCache:IDisposable
{
    private readonly object gate=new();
    private LegacyLibraryTextDomTransportRow? row;
    private bool disposed;
    private int capacity;
    public LegacyLibraryTextDomTarget Target {get;}
    public LegacyFreshMainTextDocumentSeed Seed=>Target.Seed;
    public LegacyFreshTextFieldInputContext SelectedInputContext=>Seed.SelectedInputContext;
    public bool IsDisposed {get{lock(gate)return disposed;}}
    public int Count {get{lock(gate)return row is null?0:1;}}
    public int Capacity {get{lock(gate)return capacity;}}
    private LegacyLibraryTextDomTransportCache(LegacyLibraryTextDomTarget target){Target=target;}
    public static LegacyLibraryTextDomTransportCache CreateFresh(LegacyLibraryTextDomTarget actualTarget)
    {ArgumentNullException.ThrowIfNull(actualTarget);return new(actualTarget);}
    public IReadOnlyList<LegacyLibraryTextDomTransportRow> CopyRows()
    {lock(gate){ObjectDisposedException.ThrowIf(disposed,this);return Array.AsReadOnly(row is null?Array.Empty<LegacyLibraryTextDomTransportRow>():[row]);}}
    internal LegacyLibraryTextDomTransportRow Append(LegacyLibraryTextDomTransfer incoming,bool failAllocation,CancellationToken token)
    {
        //Target -> cache -> transfer; Dispose never takes a target monitor.
        using var operation=Target.EnterOperation(incoming.Epoch,token);
        lock(gate)
        {
            ObjectDisposedException.ThrowIf(disposed,this);
            if(!ReferenceEquals(incoming.Target,Target)||!ReferenceEquals(incoming.Seed,Seed)||
                !ReferenceEquals(incoming.SelectedInputContext,SelectedInputContext))
                throw new ArgumentException("Exact library target, seed and selected INPUT/C20 source required.");
            if(row is not null)throw new NotSupportedException("This actual single-transaction source has no second pending C4D producer.");
            return incoming.MoveTo(()=>
            {
                //Native11B860's required count+1 capacity. Failed reserve
                //does not publish count/capacity or consume either slot.
                if(failAllocation)throw new InvalidOperationException("Prepared11B860 allocation failure; incoming ownership retained.");
                var created=incoming.CreateRow(this);capacity=1;row=created;return created;
            },token);
        }
    }
    public void Dispose()
    {
        lock(gate)
        {
            if(disposed)return;disposed=true;row?.Release();row=null;capacity=0;
        }
    }
}

/// <summary>Immutable40-byte scalar snapshot with actual typed pointer
/// sidecars. Release relinquishes this cache's leases, never mutating borrowed
/// recognition/source commands or destroying a caller's managed DOM value.</summary>
public sealed class LegacyLibraryTextDomTransportRow
{
    private readonly object gate=new();
    private readonly byte[] storage=new byte[40];
    private LegacyLibraryTextDomEditResult? edit;
    private LegacyOwnedFreshTextSourceContext? transaction;
    public long BeforeVersion=>-1;
    public long AfterVersion=>-1;
    public bool IsReleased {get{lock(gate)return edit is null;}}
    public LegacyLibraryTextDomEditResult DomEdit {get{lock(gate)return edit??throw new ObjectDisposedException(nameof(LegacyLibraryTextDomTransportRow));}}
    public LegacyOwnedFreshTextSourceContext TransactionOwner {get{lock(gate)return transaction??throw new ObjectDisposedException(nameof(LegacyLibraryTextDomTransportRow));}}
    public LegacyLibraryTextDomTransportCache Owner {get;}
    internal LegacyLibraryTextDomTransportRow(LegacyLibraryTextDomTransportCache owner,LegacyLibraryTextDomEditResult actualEdit,LegacyOwnedFreshTextSourceContext actualTransaction)
    {
        Owner=owner;edit=actualEdit;transaction=actualTransaction;
        BinaryPrimitives.WriteInt64LittleEndian(storage,-1);BinaryPrimitives.WriteInt64LittleEndian(storage.AsSpan(8),-1);
    }
    /// <summary>Versions00/08 are-1; pointer lanes10/18/20 are zero
    /// placeholders with exact typed sidecars. Reverse result18 is null.</summary>
    public byte[] CopyScalarStorage40()=>storage.ToArray();
    internal void Release(){lock(gate){edit=null;transaction=null;}}
}

/// <summary>One-use owned transfer receipt over a successful actual V50
/// export and its same-source pending C4D. No public caller list or success
/// flag supplies the transaction. Failed reserve retains both incoming slots.</summary>
public sealed class LegacyLibraryTextDomTransfer:IDisposable
{
    private sealed class Lease {internal bool Claimed;}
    private static readonly object claimGate=new();
    private static readonly ConditionalWeakTable<LegacyLibraryTextDomEditResult,Lease> edits=new();
    private static readonly ConditionalWeakTable<LegacyOwnedFreshTextSourceContext,Lease> transactions=new();
    private readonly object gate=new();
    private LegacyLibraryTextDomEditResult? pendingEdit;
    private LegacyOwnedFreshTextSourceContext? pendingTransaction;
    private bool transferred,disposed;
    public LegacyLibraryTextDomTarget Target {get;}
    public LegacyFreshMainTextDocumentSeed Seed=>Target.Seed;
    public LegacyFreshTextFieldInputContext SelectedInputContext=>Seed.SelectedInputContext;
    public long Epoch {get;}
    public bool WasTransferred {get{lock(gate)return transferred;}}
    public bool IsDisposed {get{lock(gate)return disposed;}}
    public LegacyLibraryTextDomEditResult? PendingEdit {get{lock(gate)return pendingEdit;}}
    public LegacyOwnedFreshTextSourceContext? PendingTransaction {get{lock(gate)return pendingTransaction;}}
    private LegacyLibraryTextDomTransfer(LegacyLibraryTextDomEditResult edit,LegacyOwnedFreshTextSourceContext transaction)
    {Target=edit.Target;Epoch=edit.Epoch;pendingEdit=edit;pendingTransaction=transaction;}
    public static LegacyLibraryTextDomTransfer Prepare(LegacyLibraryTextDomEditResult actualEdit,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualEdit);token.ThrowIfCancellationRequested();
        using var operation=actualEdit.Target.EnterOperation(actualEdit.Epoch,token);
        var source=actualEdit.Source.Sources;var handoff=source.CounterHandoff;var transaction=handoff.TransactionOwner;
        LegacyFreshMainDocumentCounterHandoff.Validate(handoff.Transforms,token);
        if(!actualEdit.Succeeded||!ReferenceEquals(actualEdit.Source.Target,actualEdit.Target)||
            !ReferenceEquals(source.Seed,actualEdit.Target.Seed)||!ReferenceEquals(source.Document,actualEdit.Target.Document)||
            !ReferenceEquals(transaction,source.Seed.PendingSelections)||!ReferenceEquals(transaction.SelectedInputContext,source.Seed.SelectedInputContext))
            throw new ArgumentException("Successful same-target export and actual selected-source pending C4D required.");
        var current=transaction.CopyPendingCommands();
        if(current.Count!=handoff.Commands.Count||current.Where((c,i)=>!SameCommand(c,handoff.Commands[i])).Any())
            throw new InvalidOperationException("The exact closed pending transaction prefix changed.");
        lock(claimGate)
        {
            token.ThrowIfCancellationRequested();var e=edits.GetValue(actualEdit,_=>new());var t=transactions.GetValue(transaction,_=>new());
            if(e.Claimed||t.Claimed)throw new InvalidOperationException("This actual edit/pending transaction already owns a transfer receipt.");
            var receipt=new LegacyLibraryTextDomTransfer(actualEdit,transaction);e.Claimed=t.Claimed=true;return receipt;
        }
    }
    private static bool SameCommand(LegacyOwnedFreshTextPendingCommand a,LegacyOwnedFreshTextPendingCommand b)
        =>ReferenceEquals(a.Owner,b.Owner)&&ReferenceEquals(a.Selection,b.Selection)&&ReferenceEquals(a.Counter,b.Counter)&&
            a.CommandIdentity==b.CommandIdentity&&a.NextCommandIdentity==b.NextCommandIdentity;
    public LegacyLibraryTextDomTransportRow TransferTo(LegacyLibraryTextDomTransportCache actualCache,CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(actualCache);token.ThrowIfCancellationRequested();return actualCache.Append(this,false,token);}
    internal LegacyLibraryTextDomTransportRow TransferPrepared(LegacyLibraryTextDomTransportCache cache,bool failAllocation,CancellationToken token=default)
        =>cache.Append(this,failAllocation,token);
    internal LegacyLibraryTextDomTransportRow MoveTo(Func<LegacyLibraryTextDomTransportRow> append,CancellationToken token)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();ObjectDisposedException.ThrowIf(disposed,this);
            if(transferred||pendingEdit is null||pendingTransaction is null)throw new InvalidOperationException("Transfer receipt already consumed.");
            var row=append();pendingEdit=null;pendingTransaction=null;transferred=true;return row;
        }
    }
    internal LegacyLibraryTextDomTransportRow CreateRow(LegacyLibraryTextDomTransportCache cache)
        =>new(cache,pendingEdit!,pendingTransaction!);
    public void Dispose(){lock(gate){if(disposed)return;disposed=true;pendingEdit=null;pendingTransaction=null;}}
}
