using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacySelectedInputRegistryReplay.Probe")]
namespace InkRecognition;

/// <summary>One library-owned replay of the actual transferred operation4/7
/// transaction onto an active registry overlay of its worker-selected INPUT.
/// This does not mutate historical C20 snapshots, apply DOM or accept an SDK page.
/// Scope/document disposal invalidates live operations; State stays historical.</summary>
public sealed class LegacyLibrarySelectedInputRegistryReplay
{
    private readonly object gate=new();
    private readonly LegacyOwnedFreshTextPendingCommand[] commands;
    private bool entered;
    private LegacyLibrarySelectedInputRegistryState state;
    public LegacyLibraryTextPublicationDocument DocumentOwner {get;}
    public LegacyLibraryTextPublicationScope Scope {get;}
    internal LegacyLibrarySelectedInputBinding InputBinding {get;}
    public LegacyFreshTextFieldInputContext InitialContext=>InputBinding.LegacyOrThrow;
    public LegacyRetainedTextFieldInputSelection Input=>InitialContext.Input;
    public LegacyLibraryOriginalSelectedFieldContext? OriginalSelectedInitialContext=>InputBinding.OriginalSelected;
    public LegacyLibrarySelectedFieldRegistryEntry? OriginalSelectedInput=>InputBinding.OriginalSelectedEntry;
    public IReadOnlyList<long> InitialRegistryIds=>Array.AsReadOnly(InputBinding.CopyInitialRegistryIds());
    public LegacyOwnedFreshTextSourceContext TransactionOwner {get;}
    public LegacyLibraryTextDomTransportRow TransportRow {get;}
    public LegacyLibrarySelectedInputRegistryState State {get{lock(gate)return state;}}
    internal bool HasEntered {get{lock(gate)return entered;}}

    internal LegacyLibrarySelectedInputRegistryReplay(LegacyLibraryTextPublicationDocument document,
        LegacyLibraryTextPublicationScope scope,LegacyLibraryTextDomTransportRow row,
        LegacyFreshMainDocumentCounterHandoff handoff,CancellationToken token)
    {
        DocumentOwner=document;Scope=scope;TransportRow=row;InputBinding=scope.Seed.InputBinding;
        TransactionOwner=row.TransactionOwner;commands=TransactionOwner.CopyPendingCommands().ToArray();
        if((InputBinding.Legacy is{} legacy&&(legacy.Parent is null||!ReferenceEquals(legacy.Input.Owner,legacy)))||
            !ReferenceEquals(TransactionOwner.InputBinding,InputBinding)||
            !ReferenceEquals(handoff.TransactionOwner,TransactionOwner)||TransactionOwner.Failed||
            !ReferenceEquals(InputBinding.Document,document.Document)||
            !ReferenceEquals(scope.Context.InputBinding,InputBinding))
            throw new ArgumentException("Exact fresh worker-selected INPUT/C22 and closed pending C4D required.");
        // Budgets precede cloning any pending C22. Do not accept caller command lists.
        if(commands.Length>65537)throw new NotSupportedException("Selected-registry command budget exceeded.");
        var initialIds=InputBinding.CopyInitialRegistryIds();
        var initialRanges=InputBinding.CopyInitialRegistryRanges();
        long total=initialRanges.Sum(r=>(long)r.Length);var ids=new HashSet<long>(initialIds);bool counter=false;
        if(initialIds.Length is <1 or >65537||ids.Count!=initialIds.Length||initialIds.Any(i=>i<0||i==long.MaxValue)||
            total>1048576||InputBinding.Next<=InputBinding.Maximum||initialIds.Max()!=InputBinding.Maximum)
            throw new NotSupportedException("Complete bounded active initial C51 registry required.");
        for(int i=0;i<commands.Length;i++)
        {
            token.ThrowIfCancellationRequested();var c=commands[i];
            if(!ReferenceEquals(c.Owner,TransactionOwner)||c.CommandIdentity!=i+1||
                c.NextCommandIdentity!=(i+1<commands.Length?i+2:0))
                throw new ArgumentException("Actual complete ordered transaction required.");
            if(c.Selection is{} s)
            {
                var ranges=s.CopyRanges();total=checked(total+ranges.Length);
                if(counter||s.Category!=100||s.Name is not ("CHAR" or "WORD" or "TEXT")||
                    s.SelectionId<0||s.SelectionId==long.MaxValue||!ids.Add(s.SelectionId)||ranges.Length>65536)
                    throw new NotSupportedException("Only unused exact IDs/category100 ordinary selections are supported.");
            }
            else if(c.Counter is{} end&&i==commands.Length-1&&end.OldCounter20==InputBinding.Next&&
                end.NewCounter18==handoff.FinalPendingCounter&&end.OldCounter20>=0&&end.NewCounter18>=0)
                counter=true;
            else throw new NotSupportedException("Only trailing actual operation7 is supported.");
            if(total>1048576)throw new NotSupportedException("Selected-registry range budget exceeded.");
        }
        if(ids.Count>65537)throw new NotSupportedException("Selected-registry selection budget exceeded.");
        state=new(this,InputBinding.Next,InputBinding.Maximum,[],0,false,false);
    }

    public static LegacyLibrarySelectedInputRegistryReplay Prepare(LegacyLibraryTextPublicationDocument actualDocument,
        CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(actualDocument);return actualDocument.ClaimRegistryReplay(token);}

    public static LegacyLibrarySelectedInputRegistryReplay PrepareOriginalSelected(LegacyLibraryTextPublicationDocument actualDocument,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualDocument);
        var row=actualDocument.ActualTransportRow??throw new InvalidOperationException("Actual transferred transaction required.");
        if(row.Owner.OriginalSelectedInputContext is null)throw new ArgumentException("Original-selected transaction required.");
        return actualDocument.ClaimRegistryReplay(token);
    }

    public LegacyLibrarySelectedInputRegistryState Run(CancellationToken token=default)=>RunCore(-1,-1,token);
    // Model only failure of the temporary C22 clone before VE0 registration.
    // This is not a claim of all internal C51 allocation side effects.
    internal LegacyLibrarySelectedInputRegistryState RunPrepared(int failCloneAtCommand=-1,
        int cancelBeforeCommand=-1,CancellationToken token=default)=>RunCore(failCloneAtCommand,cancelBeforeCommand,token);
    private LegacyLibrarySelectedInputRegistryState RunCore(int fail,int cancel,CancellationToken token)
        =>DocumentOwner.WithRegistryReplay(this,()=>
        {
            lock(gate)
            {
                if(fail < -1||fail>=commands.Length||cancel < -1||cancel>=commands.Length)
                    throw new ArgumentOutOfRangeException(nameof(fail));
                if(fail>=0&&commands[fail].Selection is null)
                    throw new ArgumentException("Clone refusal requires an actual operation4 command.");
                if(entered)throw new InvalidOperationException("Selected-registry replay retains its entered prefix and is single-use.");
                token.ThrowIfCancellationRequested();entered=true;
                var selections=new List<LegacyLibrarySelectedInputRegistrySelection>();
                long next=InputBinding.Next,maximum=InputBinding.Maximum;int completed=0;
                try
                {
                    for(int i=0;i<commands.Length;i++)
                    {
                        token.ThrowIfCancellationRequested();if(i==cancel)throw new OperationCanceledException("Prepared command-boundary cancellation.");
                        var command=commands[i];
                        if(command.Selection is{} selection)
                        {
                            if(i==fail)
                            {state=new(this,next,maximum,selections.ToArray(),completed,false,true);return state;}
                            var ranges=selection.CopyRanges(); // C22 clone targets the SAME selected C20 identity.
                            var clone=new LegacyLibrarySelectedInputRegistrySelection(this,selection,ranges);
                            var counters=AddExplicit(next,maximum,selection.SelectionId);
                            selections.Add(clone);next=counters.Next;maximum=counters.Maximum;
                        }
                        else
                        {
                            var counter=command.Counter!;
                            //078090 skips VF8 if Next already reached the desired value.
                            next=ApplyCounter(next,maximum,counter.OldCounter20,counter.NewCounter18);
                        }
                        completed=i+1;
                    }
                    state=new(this,next,maximum,selections.ToArray(),completed,true,false);return state;
                }
                catch{state=new(this,next,maximum,selections.ToArray(),completed,false,true);throw;}
            }
        },token);

    internal static (long Next,long Maximum) AddExplicit(long next,long maximum,long requested)
    {
        if(next<0||requested<0||requested==long.MaxValue)throw new NotSupportedException("Finite nonnegative explicit registry IDs required.");
        return(next<=requested?requested+1:next,Math.Max(maximum,requested));
    }
    internal static long ApplyCounter(long next,long maximum,long oldValue,long newValue)
    {
        if(next<0||oldValue<0||newValue<0)throw new NotSupportedException("Nonnegative operation7 counters required.");
        if(next>=newValue)return next;
        //C20.VF8 first clamps to Max+1, then calls C51.V28. Real active
        //registries have Next>Max, so that clamp is normally unnecessary.
        if(newValue<=maximum)
        {if(maximum==long.MaxValue)throw new NotSupportedException("Registry counter overflow is unsupported.");return maximum+1;}
        return newValue;
    }
}

/// <summary>Immutable active registry overlay after a replay prefix. INPUT and
/// InitialContext remain the original historical snapshot, not rewritten C20.</summary>
public sealed class LegacyLibrarySelectedInputRegistryState
{
    public LegacyLibrarySelectedInputRegistryReplay Owner {get;}
    public LegacyFreshTextFieldInputContext InitialContext=>Owner.InitialContext;
    public LegacyRetainedTextFieldInputSelection Input=>Owner.Input;
    public LegacyLibraryOriginalSelectedFieldContext? OriginalSelectedInitialContext=>Owner.OriginalSelectedInitialContext;
    public LegacyLibrarySelectedFieldRegistryEntry? OriginalSelectedInput=>Owner.OriginalSelectedInput;
    public IReadOnlyList<long> InitialRegistryIds=>Owner.InitialRegistryIds;
    public long NextRegistryId {get;}
    public long MaximumRegistryId {get;}
    public int CompletedCommandCount {get;}
    public bool Succeeded {get;}
    public bool IsFaulted {get;}
    public IReadOnlyList<LegacyLibrarySelectedInputRegistrySelection> Selections {get;}
    internal LegacyLibrarySelectedInputRegistryState(LegacyLibrarySelectedInputRegistryReplay owner,long next,long maximum,
        LegacyLibrarySelectedInputRegistrySelection[] selections,int completed,bool succeeded,bool faulted)
    {Owner=owner;NextRegistryId=next;MaximumRegistryId=maximum;
     Selections=Array.AsReadOnly(selections);CompletedCommandCount=completed;Succeeded=succeeded;IsFaulted=faulted;}
    /// <summary>Only assigned C51 Next18/Max20 lanes; pointers, buckets and
    /// allocator residue are not fabricated as a native registry image.</summary>
    public byte[] CopyCounterStorage16()
    {byte[] b=new byte[16];BinaryPrimitives.WriteInt64LittleEndian(b,NextRegistryId);
     BinaryPrimitives.WriteInt64LittleEndian(b.AsSpan(8),MaximumRegistryId);return b;}
}

/// <summary>One actual pending selection's retained raw16-byte ranges cloned
/// into the replay's exact selected INPUT context. No raw object ID remapping.</summary>
public sealed class LegacyLibrarySelectedInputRegistrySelection
{
    private readonly LegacyNativeInkPositionRange[] ranges;
    public LegacyLibrarySelectedInputRegistryReplay Owner {get;}
    public LegacyFreshTextFieldInputContext TargetContext=>Owner.InitialContext;
    public LegacyLibraryOriginalSelectedFieldContext? OriginalSelectedTargetContext=>Owner.OriginalSelectedInitialContext;
    public LegacyOwnedFreshTextSelection SourceCommand {get;}
    public long RegistryId=>SourceCommand.SelectionId;
    public string Name=>SourceCommand.Name;
    public int Category=>SourceCommand.Category;
    internal LegacyLibrarySelectedInputRegistrySelection(LegacyLibrarySelectedInputRegistryReplay owner,
        LegacyOwnedFreshTextSelection command,LegacyNativeInkPositionRange[] ranges)
    {Owner=owner;SourceCommand=command;this.ranges=ranges;}
    public LegacyNativeInkPositionRange[] CopyRanges()=>ranges.ToArray();
    public byte[] CopyRangeStorage16()
    {byte[] b=new byte[checked(ranges.Length*16)];for(int i=0;i<ranges.Length;i++)
     {BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16),ranges[i].First.Value);
      BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16+8),ranges[i].Last.Value);}return b;}
}
