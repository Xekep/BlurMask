using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglishQuerySession.Probe")]
namespace InkRecognition;

/// <summary>Independent complete work limits, not SDK defaults or alphabet caps.</summary>
public readonly record struct LegacyEnglishQuerySessionBounds(int MaximumAttemptedInsertions,int MaximumOperations,int MaximumOutputRows,int MaximumCheckpoints,long MaximumAdditionalBytes=16L*1024*1024);

/// <summary>Historical defensive preparation/diagnostic state; never an NN query,
///D0 lease, native pointer or controller-ready source request.</summary>
public sealed class LegacyEnglishQueryPreparationSnapshot
{
    private readonly LegacySymbolContextSnapshot pool;
    private readonly ushort[] primary,auxiliary,working;
    private readonly ulong[] initial;
    public bool IsFaulted{get;}
    public int AttemptedInsertions{get;}
    public int Operations{get;}
    public int OutputRows{get;}
    public int Checkpoints{get;}
    public long Epoch{get;}
    public ulong Root{get;}
    public int PageDescriptorCapacity{get;}
    public int CurrentPage{get;}
    internal LegacyEnglishQueryPreparationSnapshot(LegacySymbolContextSnapshot pool,ushort[] primary,ushort[] auxiliary,ushort[] working,ulong[] initial,bool fault,int attempts,int operations,int outputs,int checkpoints,long epoch,int pageCapacity,int currentPage)
    {this.pool=pool;this.primary=primary;this.auxiliary=auxiliary;this.working=working;this.initial=initial;IsFaulted=fault;AttemptedInsertions=attempts;Operations=operations;OutputRows=outputs;Checkpoints=checkpoints;Epoch=epoch;Root=pool.Root;PageDescriptorCapacity=pageCapacity;CurrentPage=currentPage;}
    public LegacySymbolContextNode[] CopyNodes()=>pool.Nodes.ToArray();
    public ulong[] CopyTreeOwners()=>pool.TreeOwners.ToArray();
    public int[] CopyPageUsed()=>pool.Used.ToArray();
    public ushort[] CopyPrimaryMap()=>primary.ToArray();
    public ushort[] CopyAuxiliaryMap()=>auxiliary.ToArray();
    public ushort[] CopyWorkingMap()=>working.ToArray();
    public ulong[] CopyInitialSource68()=>initial.ToArray();
}

/// <summary>Fresh source-binding PREPARATION only. Independent main-only source
///policy, own pool/source68/ring; M's map-only104 pool remains unchanged. No public
///startup/continuation/permission API until sealed actual N/D0 requests exist.</summary>
public sealed partial class LegacyEnglishFreshDictionaryQuerySession
{
    private readonly object gate=new();
    private readonly LegacyDictionaryInitialSymbolMaps maps;
    private readonly LegacySymbolContextFactory contexts;
    private readonly LegacyPreparedDictionaryQuerySource querySource;
    private LegacyPreparedDictionaryWorkingMap working;
    private readonly LegacyEnglishQuerySessionCheckpoint baseline;
    private int attempts,operations,outputs,checkpoints=1;
    private long epoch=1;
    private bool faulted,active;
    public LegacyEnglishMainOnlyAcousticScorer Scorer{get;}
    public LegacyEnglishSparseTerminalQuery LanguageQuery{get;}
    public LegacyEnglishMainOnlySourceMasks Masks=>Scorer.Masks;
    public LegacySource247WireLayout Source=>Masks.Source;
    public LegacyEnglishQuerySessionBounds Bounds{get;}
    public int MaximumLiveContexts{get;}
    public long AdditionalLogicalAllocationBudget{get;}
    public int InitialContextCount{get;}
    public uint DescriptorFlags{get;}
    public uint SelectedPriority{get;}
    public int SelectedSourceIndex{get;}
    public string Scope=>"fresh-source-query-preparation/independent-main13B-only/not-controller-ready";
    public bool IsFaulted{get{lock(gate)return faulted;}}
    private LegacyEnglishFreshDictionaryQuerySession(LegacyEnglishMainOnlyAcousticScorer scorer,LegacyEnglishSparseTerminalQuery language,LegacyEnglishQuerySessionBounds bounds,long budget,LegacyDictionaryInitialSymbolMaps maps,LegacySymbolContextFactory contexts,LegacyPreparedDictionaryWorkingMap working,LegacyPreparedDictionaryQuerySource query,LegacySymbolContextSnapshot snapshot,uint flags,uint priority,int index)
    {Scorer=scorer;LanguageQuery=language;Bounds=bounds;AdditionalLogicalAllocationBudget=budget;MaximumLiveContexts=checked(104+bounds.MaximumAttemptedInsertions);this.maps=maps;this.contexts=contexts;this.working=working;querySource=query;baseline=new(this,snapshot);InitialContextCount=contexts.NodeCount;DescriptorFlags=flags;SelectedPriority=priority;SelectedSourceIndex=index;}
    public static LegacyEnglishFreshDictionaryQuerySession Prepare(LegacyEnglishMainOnlyAcousticScorer scorer,LegacyEnglishSparseTerminalQuery language,LegacyEnglishQuerySessionBounds bounds,CancellationToken token=default)=>PrepareCore(scorer,language,bounds,null,token);
    internal static LegacyEnglishFreshDictionaryQuerySession PreparePrepared(LegacyEnglishMainOnlyAcousticScorer scorer,LegacyEnglishSparseTerminalQuery language,LegacyEnglishQuerySessionBounds bounds,Action<string> checkpoint,CancellationToken token)=>PrepareCore(scorer,language,bounds,checkpoint,token);
    private static LegacyEnglishFreshDictionaryQuerySession PrepareCore(LegacyEnglishMainOnlyAcousticScorer scorer,LegacyEnglishSparseTerminalQuery language,LegacyEnglishQuerySessionBounds bounds,Action<string>? checkpoint,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(scorer);ArgumentNullException.ThrowIfNull(language);token.ThrowIfCancellationRequested();Validate(scorer,language);
        long budget=Preflight(bounds);
        void Check(string stage){checkpoint?.Invoke(stage);token.ThrowIfCancellationRequested();}
        Check("session:preflight");var data=language.DataOwner;
        //104 is an upper bound on INITIAL source-map insertions, not the live
        //pool cap. Every later attempted lookup has its explicit separate budget.
        var contexts=new LegacySymbolContextFactory(checked(104+bounds.MaximumAttemptedInsertions));
        var maps=LegacyDictionaryInitialSymbolMaps.Prepare(data.AcousticAlphabet,data.LexicalAlphabet,contexts,[],token);
        Check("session:maps");if(!maps.CopyPrimary().SequenceEqual(scorer.Masks.CopyPrimaryMap())||!maps.CopyAuxiliary().SequenceEqual(scorer.Masks.CopyAuxiliaryMap()))throw new InvalidDataException("Fresh native source maps must match the SAME model's independent MAIN policy.");
        var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(data.Prefix,data.Dictionary,(uint)language.LanguageOwner.Order,true,token);
        var selected=new LegacyPreparedDictionarySourceSelectionBuilder();selected.Add(new(descriptor,(uint)language.LanguageOwner.ClassSlots,contexts.ContainerIdentity));var selection=selected.Prepare();
        if(descriptor.Flags!=scorer.Masks.DescriptorFlags||selection.Count!=1||selection[0].Priority!=1024||selection[0].SourceIndex!=0)throw new InvalidDataException("Actual independent single MAIN source binding required.");
        var working=maps.CreateWorkingMap(descriptor.Flags);var query=new LegacyPreparedDictionaryQuerySource(data.Dictionary,working,maps.CopyInitialContexts());
        Check("session:source");var snapshot=contexts.Capture(token);Check("session:checkpoint");Validate(scorer,language);Check("session:publish");
        return new(scorer,language,bounds,budget,maps,contexts,working,query,snapshot,descriptor.Flags,selection[0].Priority,selection[0].SourceIndex);
    }
    public static long Preflight(LegacyEnglishQuerySessionBounds b)
    {
        if(b.MaximumAttemptedInsertions is<0 or>65536||b.MaximumOperations is<1 or>4096||b.MaximumOutputRows is<0 or>65536||b.MaximumCheckpoints is<1 or>8)throw new ArgumentOutOfRangeException(nameof(b),"Complete bounded source-query work domain required.");
        long nodes=checked(104L+b.MaximumAttemptedInsertions);
        //Complete conservative additional logical allocation/work envelope:
        //node objects+dictionary growth/resize/restore workspace <=1KiB/node;
        //every retained checkpoint's symbolic records/owner arrays <=128B/node;
        //all queried32B rows+12B costs+headers <=64B/output; per-operation maps,
        //permission input and cursor/bookkeeping <=1KiB; fixed64KiB covers8x256
        //label slots,128x32 scratch, source maps/IDs, AVL stacks and headers.
        //Existing scorer/models/M/V58 are shared. This is not process RSS nor
        //a promise for arbitrarily many caller-retained defensive snapshots.
        long bytes=checked(nodes*1024+nodes*128*b.MaximumCheckpoints+b.MaximumOutputRows*64L+b.MaximumOperations*1024L+64*1024);
        if(b.MaximumAdditionalBytes<bytes||bytes>16L*1024*1024)throw new ArgumentException("Whole future source-query allocation/work budget exceeded.");return bytes;
    }
    private static void Validate(LegacyEnglishMainOnlyAcousticScorer scorer,LegacyEnglishSparseTerminalQuery language)
    {
        language.Validate();var masks=scorer.Masks;
        if(!ReferenceEquals(masks,language.Masks)||!ReferenceEquals(scorer.Model.Source,masks.Source)||!ReferenceEquals(masks.Source,language.DataOwner.Source))throw new ArgumentException("Exact SAME N/M/P/L source binding required, not equal bytes or dimensions.");
        if(masks.Policy!="independent-main13B-only/nonbypassed/no-user-restriction"||!masks.Knowledge.Owner.IsComplete||masks.Knowledge.Owner.IsFaulted)throw new InvalidOperationException("Completed actual independent MAIN owner required.");
    }
    public LegacyEnglishQueryPreparationSnapshot CopyPreparationSnapshot(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {token.ThrowIfCancellationRequested();var snapshot=contexts.Capture(token);return new(snapshot,maps.CopyPrimary(),maps.CopyAuxiliary(),working.CopySelectedMap(false,0),maps.CopyInitialContexts(),faulted,attempts,operations,outputs,checkpoints,epoch,contexts.PageDescriptorCapacity,contexts.CurrentPageIndex);}
    }
}

/// <summary>Opaque same-session checkpoint; internal prepared transport only.</summary>
internal sealed class LegacyEnglishQuerySessionCheckpoint
{
    internal LegacyEnglishFreshDictionaryQuerySession Owner{get;}
    internal LegacySymbolContextSnapshot Pool{get;}
    internal LegacyEnglishQuerySessionCheckpoint(LegacyEnglishFreshDictionaryQuerySession owner,LegacySymbolContextSnapshot pool){Owner=owner;Pool=pool;}
}
