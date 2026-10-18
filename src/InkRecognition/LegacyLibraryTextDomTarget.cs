using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyLibraryTextDomTarget.Probe")]
namespace InkRecognition;

/// <summary>Explicit NEW parameterized-BCC kind4 root. This is a library DOM
/// target, never an SDK textField/page or an accepted C20 result.</summary>
public sealed class LegacyLibraryTextDomTarget
{
    private readonly object gate=new();
    private readonly LegacyDomNode root;
    private long epoch;
    private bool faulted;
    public LegacyFreshMainTextDocumentSeed Seed {get;}
    public LegacyOwnedRawInkDocument Document=>Seed.Document;
    public LegacyLibraryTextDomArena Arena {get;}
    public bool IsFaulted {get{lock(gate)return faulted;}}
    public long Epoch {get{lock(gate)return epoch;}}
    public LegacyLibraryTextDomNode Root {get{lock(gate)return Arena.Snapshot(root);}}
    private LegacyLibraryTextDomTarget(LegacyFreshMainTextDocumentSeed seed)
    {Seed=seed;Arena=new(seed);root=Arena.ConstructorRoot();}
    public static LegacyLibraryTextDomTarget CreateFresh(LegacyFreshMainTextDocumentSeed sameSeed)
    {ArgumentNullException.ThrowIfNull(sameSeed);return new(sameSeed);}
    internal long CaptureEpoch(LegacyFreshMainTextDocumentSeed seed,CancellationToken token)
    {lock(gate){token.ThrowIfCancellationRequested();if(!ReferenceEquals(Seed,seed)||!ReferenceEquals(Document,seed.Document))throw new ArgumentException("Exact seed/document target required.");ValidateEpoch(epoch,token);return epoch;}}
    internal void ValidateEpoch(long expected,CancellationToken token)
    {lock(gate){token.ThrowIfCancellationRequested();if(faulted||epoch!=expected)throw new InvalidOperationException("Faulted or stale library DOM target.");}}
    //Target first, then builder-local gate. Holding this reentrant scope also
    //protects a captured live child until its writer copy is complete.
    internal IDisposable EnterOperation(long expected,CancellationToken token)
    {
        Monitor.Enter(gate);
        try{ValidateEpoch(expected,token);return new OperationScope(gate);}
        catch{Monitor.Exit(gate);throw;}
    }
    private sealed class OperationScope(object gate):IDisposable
    {
        private object? held=gate;
        public void Dispose(){var value=Interlocked.Exchange(ref held,null);if(value is not null)Monitor.Exit(value);}
    }
    internal bool CanEnterOperationPrepared()
    {if(!Monitor.TryEnter(gate))return false;Monitor.Exit(gate);return true;}
    internal T WithPatchPreflight<T>(LegacyLibraryTextDomEditResult edit,
        Func<LegacyLibraryTextDomArena,LegacyDomNode,T> action,CancellationToken token)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(!ReferenceEquals(edit.Target,this)||!edit.Succeeded)
                throw new ArgumentException("Successful exact-target edit required.");
            ValidateEpoch(edit.Epoch,token);return action(Arena,root);
        }
    }
    internal (LegacyLibraryTextDomNode Before,LegacyDomNode? Previous) CaptureInsertion(long expected,CancellationToken token)
    {lock(gate){ValidateEpoch(expected,token);if(root.Children.Count>1||root.Children.Any(n=>n.Kind!=3||n.Name!="textResult"))throw new NotSupportedException("Only a fresh root or its one previously inserted textResult is supported.");return(Arena.Snapshot(root),root.Children.SingleOrDefault());}}
    public LegacyLibraryTextDomApplyResult Apply(LegacyLibraryTextDomEditResult actualEdit,CancellationToken token=default)
        =>ApplyCore(actualEdit,-1,-1,token);
    internal LegacyLibraryTextDomApplyResult ApplyPrepared(LegacyLibraryTextDomEditResult edit,int fail=-1,int cancel=-1,CancellationToken token=default)
        =>ApplyCore(edit,fail,cancel,token);
    private LegacyLibraryTextDomApplyResult ApplyCore(LegacyLibraryTextDomEditResult edit,int fail,int cancel,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(edit);
        lock(gate)
        {
            token.ThrowIfCancellationRequested();if(!ReferenceEquals(edit.Target,this)||!edit.Succeeded)throw new ArgumentException("Successful export for this exact target required.");
            ValidateEpoch(edit.Epoch,token);edit.ClaimApply();var before=Arena.Snapshot(root);Arena.BeginFaultScope(fail,cancel);
            try
            {
                foreach(var row in edit.OwnedRows)
                {
                    token.ThrowIfCancellationRequested();if(row.Operation==4)
                    {if(root.Children.Count!=1||!ReferenceEquals(root.Children[0],edit.Previous))throw new InvalidOperationException("The retained target child changed.");Arena.DetachAndRelease(root,root.Children[0]);}
                    else
                    {
                        if(root.Children.Count!=0)throw new InvalidOperationException("Fresh insertion index0 requires an empty target.");
                        var copied=Arena.CloneFrom(edit.Source.Arena,row.LivePayload,token);
                        if(copied is null)throw new NotSupportedException("Native BE2 clone-null proceeds into Append(null); this boundary is refused.");
                        Arena.Append(root,copied);
                    }
                }
                epoch=checked(epoch+1);return new(this,edit,before,Arena.Snapshot(root));
            }
            catch{faulted=true;throw;}
        }
    }
}

public sealed class LegacyLibraryTextDomEditRow
{
    private readonly byte[] storage;
    public uint Operation {get;}
    public uint ChildOrdinal=>0;
    public IReadOnlyList<uint> Path {get;}=Array.AsReadOnly(Array.Empty<uint>());
    public LegacyLibraryTextDomNode Payload {get;}
    internal LegacyDomNode LivePayload {get;}
    internal LegacyLibraryTextDomEditRow(uint operation,LegacyLibraryTextDomArena arena,LegacyDomNode payload)
    {Operation=operation;LivePayload=payload;Payload=arena.Snapshot(payload);storage=new byte[40];BinaryPrimitives.WriteUInt32LittleEndian(storage,operation);}
    /// <summary>Assigned/default-allocator scalar lanes. Pointer-shaped lanes
    /// 08 and20 are zero placeholders whose identities are typed sidecars.</summary>
    public byte[] CopyScalarStorage40()=>storage.ToArray();
}
public sealed class LegacyLibraryTextDomEditResult
{
    private int applied;
    public LegacyLibraryTextDomTarget Target {get;}
    public LegacyOwnedMainDetachedTextDom Source {get;}
    public long Epoch {get;}
    public bool Succeeded {get;}
    public LegacyLibraryTextDomNode BeforeRoot {get;}
    public IReadOnlyList<LegacyLibraryTextDomEditRow> Rows {get;}
    internal LegacyLibraryTextDomEditRow[] OwnedRows {get;}
    internal LegacyDomNode? Previous {get;}
    internal LegacyLibraryTextDomEditResult(LegacyOwnedMainDetachedTextDom source,LegacyLibraryTextDomNode before,LegacyDomNode? previous,bool success,LegacyLibraryTextDomEditRow[] rows)
    {Source=source;Target=source.Target!;Epoch=source.TargetEpoch;BeforeRoot=before;Previous=previous;Succeeded=success;OwnedRows=rows;Rows=Array.AsReadOnly(rows);}
    internal void ClaimApply(){if(Interlocked.CompareExchange(ref applied,1,0)!=0)throw new InvalidOperationException("BE2 export application is single-use.");}
}
public sealed class LegacyLibraryTextDomApplyResult
{
    public LegacyLibraryTextDomTarget Target {get;}
    public LegacyLibraryTextDomEditResult Edit {get;}
    public LegacyLibraryTextDomNode BeforeRoot {get;}
    public LegacyLibraryTextDomNode AfterRoot {get;}
    internal LegacyLibraryTextDomApplyResult(LegacyLibraryTextDomTarget target,LegacyLibraryTextDomEditResult edit,LegacyLibraryTextDomNode before,LegacyLibraryTextDomNode after)
    {Target=target;Edit=edit;BeforeRoot=before;AfterRoot=after;}
}
/// <summary>Owned fresh061510 queue/V50 export, without native C8F/C20
/// acceptance. Exact formatting invocations remain single-use.</summary>
public sealed class LegacyLibraryTextDomInsertion
{
    private sealed class Lease {internal bool Claimed;}
    private static readonly ConditionalWeakTable<LegacyOwnedMainDetachedTextDom,Lease> lifetime=new();
    private int entered;
    internal bool HasEnteredPrepared=>Volatile.Read(ref entered)!=0;
    public LegacyOwnedMainDetachedTextDom Source {get;}
    private LegacyLibraryTextDomInsertion(LegacyOwnedMainDetachedTextDom source){Source=source;}
    public static LegacyLibraryTextDomInsertion Prepare(LegacyOwnedMainDetachedTextDom actualSource,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualSource);token.ThrowIfCancellationRequested();
        if(!actualSource.Succeeded||actualSource.Target is null||actualSource.LiveRoot is null||!ReferenceEquals(actualSource.Target.Seed,actualSource.Sources.Seed)||
            !ReferenceEquals(actualSource.Arena.NamePool,actualSource.Target.Arena.NamePool))throw new ArgumentException("Successful same-target writer result required.");
        actualSource.Target.ValidateEpoch(actualSource.TargetEpoch,token);var lease=lifetime.GetValue(actualSource,_=>new());
        lock(lease){if(lease.Claimed)throw new InvalidOperationException("This exact DOM result already owns an export.");lease.Claimed=true;}
        return new(actualSource);
    }
    public LegacyLibraryTextDomEditResult Export(CancellationToken token=default)=>ExportCore(-1,-1,token);
    internal LegacyLibraryTextDomEditResult ExportPrepared(int fail=-1,int cancel=-1,CancellationToken token=default,Action? afterValidation=null)=>ExportCore(fail,cancel,token,afterValidation);
    private LegacyLibraryTextDomEditResult ExportCore(int fail,int cancel,CancellationToken token,Action? afterValidation=null)
    {
        token.ThrowIfCancellationRequested();var target=Source.Target!;using var operation=target.EnterOperation(Source.TargetEpoch,token);
        afterValidation?.Invoke();
        if(Interlocked.CompareExchange(ref entered,1,0)!=0)throw new InvalidOperationException("Export is single-use, including failed prefixes.");
        Source.Arena.BeginFaultScope(fail,cancel);var rows=new List<LegacyLibraryTextDomEditRow>();
        if(!Source.Arena.Allow("export-header")||!Source.Arena.Allow("export-array"))return new(Source,Source.BeforeTargetRoot!,Source.PreviousTargetNode,false,rows.ToArray());
        if(Source.QueuedPreviousPayload is{} previous)
        {
            var cloned=Source.Arena.CloneFrom(Source.Arena,previous,token);
            if(cloned is null)return new(Source,Source.BeforeTargetRoot!,Source.PreviousTargetNode,false,rows.ToArray());
            rows.Add(new(4,Source.Arena,cloned));
        }
        var payload=Source.Arena.CloneFrom(Source.Arena,Source.LiveRoot!,token);
        if(payload is null)return new(Source,Source.BeforeTargetRoot!,Source.PreviousTargetNode,false,rows.ToArray());
        rows.Add(new(2,Source.Arena,payload));return new(Source,Source.BeforeTargetRoot!,Source.PreviousTargetNode,true,rows.ToArray());
    }
}
