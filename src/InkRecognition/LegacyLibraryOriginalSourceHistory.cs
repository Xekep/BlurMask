using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyOriginalSourceHistory.Probe")]
namespace InkRecognition;

/// <summary>One explicitly library-owned original-source history lifetime.
/// Retains the original bootstrap before issuing its worker selection. The
/// one-depth library frame is not native Base70/CBB/page/revision acceptance.</summary>
public sealed class LegacyLibraryOriginalSourceHistory : IDisposable
{
    private readonly object gate=new();
    private bool disposed;
    private LegacyLibraryOriginalSelectedFieldContext? selected;
    private LegacyLibraryOriginalSourceApplication? application;
    private readonly LegacyLibraryOriginalSourceState initial;
    public LegacyLibraryOriginalTextFieldBootstrap Original {get;}
    public LegacyOwnedRawInkDocument Document=>Original.Document;
    public LegacyFreshRawInkCurrentView CurrentView=>Original.CurrentView;
    public LegacyLibraryOriginalTextFieldBootstrapState HistoricalOriginal {get;}
    public LegacyLibraryOriginalSourceState ActiveState {get{lock(gate)return application?.State??initial;}}
    internal LegacyLibraryOriginalSourceHistory(LegacyLibraryOriginalTextFieldBootstrap original)
    {
        Original=original;HistoricalOriginal=original.State;
        if(!HistoricalOriginal.Succeeded||HistoricalOriginal.IsFaulted||HistoricalOriginal.Inputs.Count is <1 or >16||
            HistoricalOriginal.Inputs.Any(e=>e.RegistryId<0||e.RegistryId>=HistoricalOriginal.NextRegistryId||
                e.Category!=100||e.Name!="INPUT"||!ReferenceEquals(e.Owner,original))||
            HistoricalOriginal.Inputs.Select(e=>e.RegistryId).Distinct().Count()!=HistoricalOriginal.Inputs.Count)
            throw new NotSupportedException("Complete bounded fresh original INPUT registry required.");
        initial=new(this,new(this),false,false,false,false,0);
    }
    public static LegacyLibraryOriginalSourceHistory Prepare(LegacyLibraryOriginalTextFieldBootstrap actualOriginal,
        CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(actualOriginal);return actualOriginal.ClaimSourceHistory(token);}
    public LegacyLibraryOriginalSelectedFieldContext CreateSelectedInput(LegacyLibraryOriginalFieldSelection actualInput,
        CancellationToken token=default)
    {
        lock(gate)
        {
            Validate(token);if(selected is not null)throw new InvalidOperationException("One pre-NN worker selection per history lifetime.");
            var value=LegacyLibraryOriginalSelectedFieldContext.Prepare(Original,actualInput,token);selected=value;return value;
        }
    }
    public LegacyLibraryOriginalSourceApplication PrepareApplication(LegacyLibraryTextPublicationDocument actualDocument,
        LegacyLibraryOriginalInputPatchCompatibility actualCompatibility,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualDocument);ArgumentNullException.ThrowIfNull(actualCompatibility);
        return actualDocument.ClaimOriginalSourceApplication(this,actualCompatibility,token);
    }
    internal LegacyLibraryOriginalSourceApplication ClaimApplication(LegacyLibraryTextPublicationDocument document,
        LegacyLibraryOriginalInputPatchCompatibility compatibility,CancellationToken token)
    {
        lock(gate)
        {
            Validate(token);if(application is not null)throw new InvalidOperationException("One application per source history.");
            var replay=compatibility.Replay;
            if(selected is null||!selected.State.Succeeded||!compatibility.State.IsReady||
                !ReferenceEquals(compatibility.OriginalBootstrap,Original)||!ReferenceEquals(replay.DocumentOwner,document)||
                !ReferenceEquals(replay.OriginalSelectedInitialContext,selected)||
                !ReferenceEquals(replay.Scope.Context.InputBinding.OriginalSelected,selected)||
                !ReferenceEquals(document.Document,Document))
                throw new ArgumentException("Exact history-issued pre-NN selection/original preflight/document required.");
            if(replay.Scope.Context.InputBinding.Next!=HistoricalOriginal.NextRegistryId||
                !replay.InitialRegistryIds.SequenceEqual(HistoricalOriginal.Inputs.Select(e=>e.RegistryId)))
                throw new ArgumentException("Complete original and selected registry identity required.");
            var value=new LegacyLibraryOriginalSourceApplication(this,document,compatibility,token);application=value;return value;
        }
    }
    internal T WithApplication<T>(LegacyLibraryOriginalSourceApplication actual,Func<T> action,CancellationToken token)
    {lock(gate){Validate(token);if(!ReferenceEquals(application,actual))throw new ArgumentException("Exact retained application required.");return action();}}
    internal void Validate(CancellationToken token){ObjectDisposedException.ThrowIf(disposed,this);token.ThrowIfCancellationRequested();}
    public void Dispose(){lock(gate)disposed=true;}
}

/// <summary>Original-domain execution and its generated journal, separate from
/// worker-selected replay. Live operations borrow the actual document/scope;
/// closing either or the history invalidates them. Snapshots remain historical.</summary>
public sealed class LegacyLibraryOriginalSourceApplication
{
    private readonly LegacyPreparedOriginalSourceJournal kernel;
    private readonly LegacyOwnedFreshTextPendingCommand[] commands;
    private bool entered,rollbackEntered;
    private LegacyLibraryOriginalSourceState state;
    public LegacyLibraryOriginalSourceHistory Owner {get;}
    public LegacyLibraryTextPublicationDocument DocumentOwner {get;}
    public LegacyLibraryOriginalInputPatchCompatibility Compatibility {get;}
    public LegacyOwnedFreshTextSourceContext TransactionOwner=>Compatibility.TransactionOwner;
    public LegacyLibraryOriginalSourceState State=>Volatile.Read(ref state);
    internal LegacyLibraryOriginalSourceApplication(LegacyLibraryOriginalSourceHistory owner,LegacyLibraryTextPublicationDocument document,
        LegacyLibraryOriginalInputPatchCompatibility compatibility,CancellationToken token)
    {
        Owner=owner;DocumentOwner=document;Compatibility=compatibility;
        commands=TransactionOwner.CopyPendingCommands().ToArray();var initial=owner.HistoricalOriginal;
        long expected=initial.NextRegistryId,total=initial.Inputs.Sum(e=>(long)e.CopyRanges().Length);
        if(commands.Length is <2 or >65537)throw new NotSupportedException("Bounded actual complete operation4/7 transaction required.");
        for(int i=0;i<commands.Length;i++)
        {
            token.ThrowIfCancellationRequested();var c=commands[i];
            if(!ReferenceEquals(c.Owner,TransactionOwner)||c.CommandIdentity!=i+1||c.NextCommandIdentity!=(i+1==commands.Length?0:i+2))
                throw new ArgumentException("Exact ordered transferred C4D required.");
            if(c.Selection is{} s)
            {
                var ranges=s.CopyRanges();total=checked(total+ranges.Length);
                if(s.SelectionId!=expected++||s.SelectionId==long.MaxValue||s.Category!=100||s.Name is not ("CHAR" or "WORD" or "TEXT")||
                    ranges.Length>65536||i==commands.Length-1||total>1048576)
                    throw new NotSupportedException("Only actual contiguous new-ID category100 ordinary selection commands supported.");
                _=owner.CurrentView.ExportIdentityTransforms(ranges,token);
            }
            else if(i!=commands.Length-1||c.Counter is not{} end||end.OldCounter20!=initial.NextRegistryId||end.NewCounter18!=expected)
                throw new NotSupportedException("Actual trailing counter handoff required.");
        }
        if(initial.Inputs.Count+commands.Length-1>65537)throw new NotSupportedException("Registry selection budget exceeded.");
        kernel=new(owner);state=new(owner,kernel,false,false,false,false,0);
    }
    public LegacyLibraryOriginalSourceState Run(CancellationToken token=default)=>RunCore(-1,-1,-1,token);
    //Temporary clone refusal is before VE0. Observer refusal is ignored by VE0.
    internal LegacyLibraryOriginalSourceState RunPrepared(int failTemporaryClone=-1,int failObserverHistory=-1,
        int cancelBeforeCommand=-1,CancellationToken token=default)=>RunCore(failTemporaryClone,failObserverHistory,cancelBeforeCommand,token);
    private LegacyLibraryOriginalSourceState RunCore(int clone,int observer,int cancel,CancellationToken token)
        =>DocumentOwner.WithOriginalSourceApplication(this,()=>Owner.WithApplication(this,()=>
        {
            if(entered)throw new InvalidOperationException("Entered application is single-use.");
            foreach(int n in new[]{clone,observer,cancel})if(n < -1||n>=commands.Length)throw new ArgumentOutOfRangeException(nameof(clone));
            if((clone>=0&&commands[clone].Selection is null)||(observer>=0&&commands[observer].Selection is null))
                throw new ArgumentException("Clone/history refusal requires an actual operation4.");
            token.ThrowIfCancellationRequested();entered=true;kernel.Trace.Add("library-source-frame:begin");bool success=false;
            try
            {
                for(int i=0;i<commands.Length;i++)
                {
                    token.ThrowIfCancellationRequested();if(i==cancel)throw new OperationCanceledException("Prepared command-boundary cancellation.");
                    var c=commands[i];if(c.Selection is{} selection)
                    {
                        if(i==clone){kernel.Trace.Add("temporary-C22:refused");break;}
                        kernel.Add(selection,i==observer,token);
                    }
                    else kernel.Counter(c.Counter!);
                    kernel.Completed=i+1;
                }
                success=kernel.Completed==commands.Length;
            }
            finally
            {
                kernel.Trace.Add("library-source-frame:end");
                Capture(success,!success,false);
            }
            return state;
        },token),token);
    public LegacyLibraryOriginalSourceState Rollback(CancellationToken token=default)=>RollbackCore(-1,token);
    internal LegacyLibraryOriginalSourceState RollbackPrepared(int cancelBeforeReverse=-1,CancellationToken token=default)
        =>RollbackCore(cancelBeforeReverse,token);
    private LegacyLibraryOriginalSourceState RollbackCore(int cancel,CancellationToken token)
        =>DocumentOwner.WithOriginalSourceApplication(this,()=>Owner.WithApplication(this,()=>
        {
            if(!entered||rollbackEntered||!kernel.JournalComplete)throw new InvalidOperationException("One complete generated inverse frame required.");
            if(cancel < -1||cancel>=kernel.Journal.Count)throw new ArgumentOutOfRangeException(nameof(cancel));
            token.ThrowIfCancellationRequested();rollbackEntered=true;kernel.Recording=false;kernel.Trace.Add("rollback:recording0");
            try
            {
                for(int i=kernel.Journal.Count-1;i>=0;i--)
                {
                    token.ThrowIfCancellationRequested();if(kernel.Reversed==cancel)throw new OperationCanceledException("Prepared inverse-command cancellation.");
                    kernel.Delete(kernel.Journal[i]);kernel.Reversed++;
                }
                kernel.Recording=true;kernel.Trace.Add("rollback:recording1");return Capture(state.ExecutionSucceeded,false,true);
            }
            catch{Capture(state.ExecutionSucceeded,true,false);throw;}
        },token),token);
    internal void ValidateForLibraryApply(CancellationToken token)
        =>Owner.WithApplication(this,()=>{if(!state.IsSafeForLibraryApply)throw new InvalidOperationException("Claimed original application must execute with complete history before library target apply.");return true;},token);
    private LegacyLibraryOriginalSourceState Capture(bool execution,bool faulted,bool reversed)
    {var value=new LegacyLibraryOriginalSourceState(Owner,kernel,entered,execution,faulted,reversed,kernel.Reversed);Volatile.Write(ref state,value);return value;}
}

public sealed class LegacyLibraryOriginalSourceRegistryEntry
{
    public LegacyLibraryOriginalSourceHistory Owner {get;}
    public long RegistryId {get;}
    public string Name {get;}
    public int Category {get;}
    public LegacyLibraryOriginalInkSelection RangesOwner {get;}
    public LegacyOwnedFreshTextSelection? SourceCommand {get;}
    internal LegacyLibraryOriginalSourceRegistryEntry(LegacyLibraryOriginalSourceHistory owner,long id,string name,int category,
        LegacyLibraryOriginalInkSelection ranges,LegacyOwnedFreshTextSelection? source)
    {Owner=owner;RegistryId=id;Name=name;Category=category;RangesOwner=ranges;SourceCommand=source;}
    public LegacyNativeInkPositionRange[] CopyRanges()=>RangesOwner.CopyRanges();
}
public sealed class LegacyLibraryOriginalSourceJournalRecord
{
    public LegacyLibraryOriginalSourceHistory Owner {get;}
    public LegacyOwnedFreshTextSelection SourceCommand {get;}
    public long RegistryId=>SourceCommand.SelectionId;
    public int CommandIdentity=>SourceCommand.CommandIdentity;
    public byte Operation=>4;
    public string Name=>SourceCommand.Name;
    public int Category=>SourceCommand.Category;
    public LegacyLibraryOriginalInkSelection CopiedSelection {get;}
    internal LegacyLibraryOriginalSourceJournalRecord(LegacyLibraryOriginalSourceHistory owner,LegacyOwnedFreshTextSelection command,
        LegacyLibraryOriginalInkSelection ranges){Owner=owner;SourceCommand=command;CopiedSelection=ranges;}
}
public sealed class LegacyLibraryOriginalSourceState
{
    public LegacyLibraryOriginalSourceHistory Owner {get;}
    public bool HasEntered {get;}
    public bool ExecutionSucceeded {get;}
    public bool IsFaulted {get;}
    public bool JournalComplete {get;}
    public bool WasRolledBack {get;}
    public bool RecordingEnabled {get;}
    public bool IsSafeForLibraryApply=>HasEntered&&ExecutionSucceeded&&JournalComplete&&!IsFaulted&&!WasRolledBack;
    public int CompletedCommandCount {get;}
    public int ReversedCommandCount {get;}
    public long NextRegistryId {get;}
    public long CachedMaximumRegistryId {get;}
    public bool MaximumDirty {get;}
    public IReadOnlyList<LegacyLibraryOriginalSourceRegistryEntry> Registry {get;}
    public IReadOnlyList<LegacyLibraryOriginalSourceJournalRecord> GeneratedJournal {get;}
    public IReadOnlyList<string> Trace {get;}
    internal LegacyLibraryOriginalSourceState(LegacyLibraryOriginalSourceHistory owner,LegacyPreparedOriginalSourceJournal data,
        bool entered,bool executed,bool faulted,bool rollback,int reversed)
    {Owner=owner;HasEntered=entered;ExecutionSucceeded=executed;IsFaulted=faulted;WasRolledBack=rollback;
     JournalComplete=data.JournalComplete;RecordingEnabled=data.Recording;CompletedCommandCount=data.Completed;
     ReversedCommandCount=reversed;NextRegistryId=data.Next;CachedMaximumRegistryId=data.Maximum;MaximumDirty=data.MaximumDirty;
     Registry=Array.AsReadOnly(data.Entries.ToArray());GeneratedJournal=Array.AsReadOnly(data.Journal.ToArray());Trace=Array.AsReadOnly(data.Trace.ToArray());}
    /// <summary>Only assigned C51 Next18/Max20/dirty28. No pointer or index bytes.</summary>
    public byte[] CopyCounterStorage17(){byte[] b=new byte[17];BinaryPrimitives.WriteInt64LittleEndian(b,NextRegistryId);
     BinaryPrimitives.WriteInt64LittleEndian(b.AsSpan(8),CachedMaximumRegistryId);b[16]=MaximumDirty?(byte)1:(byte)0;return b;}
}
