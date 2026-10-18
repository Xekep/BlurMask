using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyOriginalInputPatchCompatibility.Probe")]
namespace InkRecognition;

/// <summary>Bounded ordinary operation4/7 preflight over the exact original
/// INPUT parent and incoming C4D. Models06EC60/077850 compatibility, not a CBB
/// page/history receiver. Once claimed, this owner's successful compatible
/// result is required before the document's selected-registry replay can run.</summary>
public sealed class LegacyLibraryOriginalInputPatchCompatibility
{
    private readonly object gate=new();
    private readonly LegacyOwnedFreshTextPendingCommand[] commands;
    private readonly LegacyNativeInkPositionRange[] domain;
    private bool entered;
    private LegacyLibraryOriginalInputPatchCompatibilityState state;
    public LegacyLibrarySelectedInputRegistryReplay Replay {get;}
    public LegacyLibraryTextPublicationDocument DocumentOwner=>Replay.DocumentOwner;
    public LegacyLibraryTextPublicationScope Scope=>Replay.Scope;
    public LegacyFreshTextFieldInputContext OriginalContext {get;}
    public LegacyRetainedTextFieldInputSelection OriginalInput=>OriginalContext.Input;
    public LegacyOwnedFreshTextSourceContext TransactionOwner=>Replay.TransactionOwner;
    public LegacyLibraryOriginalInputPatchCompatibilityState State {get{lock(gate)return state;}}
    internal LegacyLibraryOriginalInputPatchCompatibility(LegacyLibrarySelectedInputRegistryReplay replay,CancellationToken token)
    {
        Replay=replay;OriginalContext=replay.InitialContext.Parent??throw new ArgumentException("Original INPUT parent required.");
        ValidateOriginal(replay,OriginalContext);
        var ranges=LegacyOwnedRawInkRangeOperations.Prepare(OriginalContext.Document);
        LegacyNativeInkPositionRange[] built=[];
        foreach(var curve in OriginalContext.CurrentView.Curves)
        {
            token.ThrowIfCancellationRequested();uint id=checked((uint)curve.OriginalSourceIndex);
            var first=LegacyNativeInkPosition.Pack(id,0,3,curve.PointCount);
            var last=LegacyNativeInkPosition.Pack(id,curve.PointCount-1,5,curve.PointCount);
            built=ranges.Union(built,[new(first,last)],token);
        }
        //065940 builds C20+20 from the appended raw C49s. Only after that
        //producer do we compare to the retained INPUT C22; it is not assumed.
        if(!built.AsSpan().SequenceEqual(OriginalInput.CopyRanges())||
            !built.AsSpan().SequenceEqual(replay.Input.CopyRanges()))
            throw new NotSupportedException("Full original raw-domain/INPUT/selected-C22 identity required.");
        domain=built;commands=TransactionOwner.CopyPendingCommands().ToArray();
        if(commands.Length>65537||commands.Any(c=>!ReferenceEquals(c.Owner,TransactionOwner)||c.OperationCode is not (4 or 7)))
            throw new NotSupportedException("Bounded actual ordinary operation4/7 transaction required.");
        state=new(this,false,false,0,false,0,[]);
    }
    public static LegacyLibraryOriginalInputPatchCompatibility Prepare(LegacyLibrarySelectedInputRegistryReplay actualReplay,
        CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(actualReplay);return actualReplay.DocumentOwner.ClaimOriginalCompatibility(actualReplay,token);}
    internal static void ValidateOriginal(LegacyLibrarySelectedInputRegistryReplay replay,LegacyFreshTextFieldInputContext original)
    {
        if(!ReferenceEquals(original,replay.InitialContext.Parent)||original.Parent is not null||
            !ReferenceEquals(original.Document,replay.DocumentOwner.Document)||
            !ReferenceEquals(original.CurrentView,replay.InitialContext.CurrentView)||
            !ReferenceEquals(replay.Scope.Context.SelectedInput,replay.InitialContext)||
            !ReferenceEquals(original.Input.Owner,original)||original.RawObjectCount!=original.Document.ObjectCount)
            throw new ArgumentException("Exact constructor-original C20 parent/INPUT/raw domain required; selected copies are different owners.");
    }
    public LegacyNativeInkPositionRange[] CopyOriginalInkDomain()=>domain.ToArray();
    public LegacyLibraryOriginalInputPatchCompatibilityState Run(CancellationToken token=default)=>RunCore(-1,-1,-1,token);
    //Prepared faults/counterexamples are not substituted for production commands.
    internal LegacyLibraryOriginalInputPatchCompatibilityState RunPrepared(int failureStage=-1,
        int incompatibleAtCommand=-1,int cancelBeforeCommand=-1,CancellationToken token=default)
        =>RunCore(failureStage,incompatibleAtCommand,cancelBeforeCommand,token);
    private LegacyLibraryOriginalInputPatchCompatibilityState RunCore(int failure,int incompatible,int cancel,CancellationToken token)
        =>DocumentOwner.WithOriginalCompatibility(this,()=>
        {
            lock(gate)
            {
                if(failure is < -1 or >1||incompatible < -1||incompatible>=commands.Length||cancel < -1||cancel>=commands.Length)
                    throw new ArgumentOutOfRangeException(nameof(failure));
                if(incompatible>=0&&commands[incompatible].Selection is null)
                    throw new ArgumentException("Prepared incompatibility requires an operation4 command.");
                if(entered)throw new InvalidOperationException("Preflight retains its entered prefix and is single-use.");
                token.ThrowIfCancellationRequested();entered=true;int inspected=0;byte[] presence=[];
                try
                {
                    //077850 writes compatibility at its final return even
                    //when its C21 copy or raw-presence allocation failed.
                    if(failure==0){state=new(this,true,false,1,true,inspected,presence);return state;}
                    var localDomain=domain.ToArray();
                    if(failure==1){state=new(this,true,false,1,true,inspected,presence);return state;}
                    presence=new byte[OriginalContext.RawObjectCount];
                    for(int i=0;i<presence.Length;i++)
                    {
                        var curve=OriginalContext.CurrentView.Curves[i];
                        if(curve.OriginalSourceIndex!=i||!ReferenceEquals(curve.Document,OriginalContext.Document))
                            throw new NotSupportedException("Complete original raw slot ownership required.");
                        presence[i]=1; //Actual retained C49, not an assumed external source slot.
                    }
                    bool compatible=true;
                    for(int i=0;i<commands.Length&&compatible;i++)
                    {
                        token.ThrowIfCancellationRequested();if(i==cancel)throw new OperationCanceledException("Prepared command-boundary cancellation.");
                        if(commands[i].Selection is{} selection)
                        {
                            var request=selection.CopyRanges();
                            if(i==incompatible)
                            {
                                //Explicit PREPARED out-of-root C22 counterexample.
                                uint outside=checked((uint)OriginalContext.RawObjectCount);
                                request=[new(LegacyNativeInkPosition.Pack(outside,0,3,1),LegacyNativeInkPosition.Pack(outside,0,5,1))];
                            }
                            compatible=CheckSelection(localDomain,request);
                        }
                        inspected=i+1;
                    }
                    state=new(this,true,true,compatible?(byte)1:(byte)0,!compatible,inspected,presence);return state;
                }
                catch{state=new(this,true,false,0,true,inspected,presence,compatibilityWritten:false);throw;}
            }
        },token);
    internal static bool CheckSelection(ReadOnlySpan<LegacyNativeInkPositionRange> domain,ReadOnlySpan<LegacyNativeInkPositionRange> request)
    {
        //077850 skips V120 for an empty operation4 C22. V120 alone has a
        //different empty-request return; do not conflate the two contracts.
        foreach(var range in request)if(!LegacyOwnedRawInkRangeOperations.Contains(domain,range))return false;
        return true;
    }
}

/// <summary>Immutable preflight prefix. Worked is the native execution return,
/// CompatibilityByte is the separate caller output (initially0). Allocation
/// refusal can leave Worked=false while writing compatibility1. No write here
/// applies a registry, DOM, page revision or history transaction.</summary>
public sealed class LegacyLibraryOriginalInputPatchCompatibilityState
{
    private readonly byte[] presence;
    public LegacyLibraryOriginalInputPatchCompatibility Owner {get;}
    public bool WasRun {get;}
    public bool Worked {get;}
    public byte CompatibilityByte {get;}
    public bool CompatibilityWasWritten {get;}
    public bool IsFaulted {get;}
    public int InspectedCommandCount {get;}
    public bool IsReady=>WasRun&&Worked&&CompatibilityWasWritten&&CompatibilityByte!=0&&!IsFaulted;
    internal LegacyLibraryOriginalInputPatchCompatibilityState(LegacyLibraryOriginalInputPatchCompatibility owner,
        bool wasRun,bool worked,byte compatible,bool faulted,int inspected,byte[] presence,bool compatibilityWritten=true)
    {Owner=owner;WasRun=wasRun;Worked=worked;CompatibilityByte=compatible;IsFaulted=faulted;InspectedCommandCount=inspected;
     this.presence=presence;CompatibilityWasWritten=wasRun&&compatibilityWritten;}
    public byte[] CopyRawPresence()=>presence.ToArray();
    public byte[] CopyOutputBytes()=>[Worked?(byte)1:(byte)0,CompatibilityByte];
}
