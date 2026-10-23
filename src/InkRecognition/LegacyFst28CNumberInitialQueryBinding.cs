using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyFst28CNumberInitialQueryBinding.Probe")]

namespace InkRecognition;

public readonly record struct LegacyFst28CNumberAuxiliaryBinding(int Slot, ushort Key,
    ulong NodeIdentity, bool HistoryPayloadPresent, bool SecondaryPayloadPresent);
public readonly record struct LegacyFst28CNumberResetWrite(int Offset, int Width, long Value);

internal readonly record struct LegacyFst28CNumberRequestDiagnostic(ulong Tag,
    bool HeaderNonNull, bool HeaderSameBootstrap, bool HeaderSameRequest,
    bool CohortSame, bool D0Same, bool ParentAbsent, byte Source, int Start, int Span,
    byte Flags, int Skip, int Take, bool CachedOwnerAbsent, bool LiveTreeNonNull);

/// <summary>Initial active-context/header/reset/request prefix.
/// No query, paging, history consumer, Ready or number-recognition authority.
/// Existing bootstrap/initial constructor views remain historical snapshots.</summary>
public sealed class LegacyFst28CNumberInitialQueryBinding
{
    // Complete bounded protocol domains; not CLR allocation/RSS or native byte image.
    public const int LogicalByteEnvelope = 8954;
    public const int WorkEnvelope = 22202;
    private sealed class Claim { internal int Used; }
    private static readonly ConditionalWeakTable<LegacyFst28CFreshPrimaryControllerBootstrap, Claim> claims = new();
    private readonly object cohort = new(), historicalHeader;
    // One private live overlay is associated with the SAME existing header token.
    // The factory retains its native-symbolic node identities. No public mutator.
    private readonly LegacySymbolContextFactory context;
    private readonly IReadOnlyList<LegacySymbolContextNode> nodes;
    private readonly ushort[] preserved, current, secondary;
    private readonly ulong[] labelNodes, prefixNodes;
    private readonly int[] labelSlots;
    private readonly LegacyFst28CNumberAuxiliaryBinding[] auxiliaries;
    private readonly LegacyFst28CNumberResetWrite[] resetWrites;
    private sealed class InitialRequest(object cohort, object header, LegacyFst28CNumberInitialD0 d0)
    {
        internal readonly object Cohort = cohort, Header = header;
        internal readonly LegacyFst28CNumberInitialD0 Owner = d0;
        internal ulong Tag => 8UL << 40;
        internal object? Parent => null;
        internal byte Source => 0;
        internal int Start => 0;
        internal int Span => 0;
        internal byte Flags => 0;
        internal int Skip => 0;
        internal int Take => 128;
        internal object? CachedOwner => null;
    }
    private readonly InitialRequest request;
    public LegacyFst28CNumberInitialD0 D0 { get; }
    public LegacyFst28CFreshPrimaryControllerBootstrap Bootstrap => D0.Bootstrap;
    public LegacyFst28CInitialState InitialState => D0.InitialState;
    public LegacyFst28CTargetPreparation Policy => D0.Policy;
    public LegacyFst13AOutgoingSource Source => D0.Source;
    public LegacyFreshMainCompactOrdinaryGraph Graph => D0.Graph;
    public bool HasQueryAuthority => false;
    public bool HasPagingAuthority => false;
    public int CursorCapacity => 12;
    public int CursorCount => 8;
    public int NextCursor => 0;
    public ulong LiveTreeRoot => context.RootTreeIdentity;
    public int LiveNodeCount => context.NodeCount;
    public int LogicalByteReservation => LogicalByteEnvelope;
    public int WorkReservation => WorkEnvelope;
    private LegacyFst28CNumberInitialQueryBinding(LegacyFst28CNumberInitialD0 d0, object header,
        LegacySymbolContextFactory context, IReadOnlyList<LegacySymbolContextNode> nodes,
        ushort[] preserved, ushort[] current, ushort[] secondary, ulong[] labelNodes, int[] labelSlots,
        LegacyFst28CNumberAuxiliaryBinding[] auxiliaries, ulong[] prefixNodes,
        LegacyFst28CNumberResetWrite[] resetWrites)
    {
        D0=d0; historicalHeader=header; this.context=context; this.nodes=nodes;
        this.preserved=preserved; this.current=current; this.secondary=secondary;
        this.labelNodes=labelNodes; this.labelSlots=labelSlots; this.auxiliaries=auxiliaries;
        this.prefixNodes=prefixNodes; this.resetWrites=resetWrites;
        request=new(cohort,header,d0);
    }
    public static LegacyFst28CNumberInitialQueryBinding CreateFreshInitialPrefix(
        LegacyFst28CNumberInitialD0 d0, CancellationToken token=default)
        => CreateBounded(d0,LogicalByteEnvelope,WorkEnvelope,token);

    internal static LegacyFst28CNumberInitialQueryBinding CreateBounded(
        LegacyFst28CNumberInitialD0 d0,int byteBudget,int workBudget,CancellationToken token=default,
        Action<string>? checkpoint=null)
    {
        token.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(d0);
        if(byteBudget is <0 or >LogicalByteEnvelope)throw new ArgumentOutOfRangeException(nameof(byteBudget));
        if(workBudget is <0 or >WorkEnvelope)throw new ArgumentOutOfRangeException(nameof(workBudget));
        if(byteBudget<LogicalByteEnvelope||workBudget<WorkEnvelope)
            throw new InvalidDataException("Whole initial binding envelope exceeds budget.");
        // The immutable genuine D0 already validates every selected initial ordinary
        // arc, its compact group and full NN/cache provenance. Do not deep-copy
        // diagnostic CopyGroup or CopySelectedGroups merely to repeat that proof.
        var boot=d0.Bootstrap; var initial=d0.InitialState; var policy=d0.Policy;
        if(d0.CurrentNode!=0||d0.CurrentSpan!=0||d0.ReusedPreviousCache||
            d0.AcousticEvaluations is <1 or >64||
            !ReferenceEquals(boot.Graph,d0.Graph)||!ReferenceEquals(boot.InitialState,initial)||
            !ReferenceEquals(initial.TargetPreparation,policy)||!ReferenceEquals(policy.PrimaryMap.Source,d0.Source)||
            policy.PrimaryMap.SourceLabelCount!=16||initial.Capacity!=12||d0.Source.StateCount!=98||
            initial.DecodedSourceName!="ru_RU-lk-text.number"||boot.AdmittedSourceCount!=1||boot.SelectedOrdinal!=0)
            throw new ArgumentException("Exact typed same-owner initial number D0 required.");
        var constructor=boot.ReadConstructorState(token);
        var metadata=boot.ReadSourceMetadata(token);
        if(!constructor.Primary||!constructor.HeaderPresent||!constructor.TreeRootAbsent||
            !constructor.ParentAbsent||constructor.ContextIssued||constructor.EnumerationFlags!=8||
            constructor.State!=0||constructor.Source!=0||constructor.Skip!=0||constructor.Take!=128||
            !constructor.CachedOwnerAbsent||metadata.Header!=1||metadata.SourceIndex!=0||
            metadata.AggregateNamespace!=1||metadata.AcceptedSkipBase!=uint.MaxValue||metadata.Weight24!=0||metadata.Weight28!=1||metadata.Priority!=1)
            throw new InvalidDataException("Actual singleton constructor prefix differs.");
        var header=boot.RetainInitialNumberHeaderIdentity(d0,token);
        void Checkpoint(string stage){token.ThrowIfCancellationRequested();checkpoint?.Invoke(stage);token.ThrowIfCancellationRequested();}
        Checkpoint("before-claim");
        var claim=claims.GetValue(boot,static _=>new Claim());
        if(Interlocked.CompareExchange(ref claim.Used,1,0)!=0)
            throw new InvalidOperationException("Same bootstrap header already claimed or poisoned.");
        Checkpoint("after-claim");
        // Actual copy API inventory: preserved32B + current32B + secondary32B.
        // Secondary is built by16 scalar MapSecondary.Code calls, NOT CopySecondary
        // (which would copy16 two-ushort structs). No other model/NN/geometry copy.
        ushort[] preserved=d0.Masks.CopyPreservedMap(token),current=d0.CopyCurrentPrimary(token),secondary=new ushort[16];
        if(preserved.Length!=16||current.Length!=16)throw new InvalidDataException("Actual map dimension differs.");
        for(ushort i=0;i<16;i++){token.ThrowIfCancellationRequested();secondary[i]=policy.MapSecondary(i).Code;
            if(preserved[i]!=policy.PrimaryMap.MapLabel(i).Code)throw new InvalidDataException("Preserved source label differs.");}
        Checkpoint("after-map-copies");
        var context=new LegacySymbolContextFactory(18);
        ulong[] labelNodes=context.CreateInitialSourceMap(preserved,token); //16*8B.
        var labelSlots=new int[16]; //16*4B.
        var auxiliaries=new LegacyFst28CNumberAuxiliaryBinding[15]; //15*16 protocol bytes.
        int slot=0;
        for(ushort i=0;i<16;i++)
        {
            token.ThrowIfCancellationRequested(); var historicalSlot=initial.ReadAuxiliarySlot(i);
            if(preserved[i] is 0xffff or 0xfffd)
            {if(historicalSlot is not null||labelNodes[i]!=0)throw new InvalidDataException("Missing label/slot differs.");labelSlots[i]=-1;continue;}
            if(slot>=15||historicalSlot!=slot||labelNodes[i]==0)
                throw new InvalidDataException("Existing source slot identity differs.");
            var node=context.ReadNode(labelNodes[i]);
            if(node.Symbol!=preserved[i])throw new InvalidDataException("Existing source context key differs.");
            labelSlots[i]=slot;auxiliaries[slot]=new(slot,preserved[i],labelNodes[i],false,false);slot++;
        }
        if(slot!=15||context.NodeCount!=15)throw new InvalidDataException("Actual15 retained source slots required.");
        Checkpoint("after-source-nodes");
        ushort[] prefixes=[0xfffb,0xfff9,0xfffb,0xfffc,0xfffa]; //5*2B.
        var prefixNodes=new ulong[5]; //5*8B.
        for(int i=0;i<5;i++){token.ThrowIfCancellationRequested();prefixNodes[i]=context.ResolveInitial(prefixes[i],token).Identity;
            if(prefixNodes[i]==0)throw new InvalidDataException("Whole context node bound differs.");}
        if(context.NodeCount!=18||context.RootTreeIdentity==0||prefixNodes[0]!=prefixNodes[2]||prefixNodes[0]!=labelNodes[11])
            throw new InvalidDataException("Actual18-node live context/space alias differs.");
        var nodeArray=new LegacySymbolContextNode[18]; //18*(56+8)B protocol view+reference domains.
        for(int i=0;i<18;i++)
        {
            token.ThrowIfCancellationRequested();
            ulong id=i<15?auxiliaries[i].NodeIdentity:prefixNodes[i==15?1:i==16?3:4];
            nodeArray[i]=context.ReadNode(id);
        }
        IReadOnlyList<LegacySymbolContextNode> nodes=Array.AsReadOnly(nodeArray);
        void ValidateLink(ulong link)
        {
            if(link==0)return;bool found=false;
            for(int j=0;j<nodes.Count;j++)found|=nodes[j].Identity==link;
            if(!found)throw new InvalidDataException("Foreign AVL link.");
        }
        for(int i=0;i<nodes.Count;i++)
        {
            token.ThrowIfCancellationRequested();var node=nodes[i];
            if(node.ParentLink!=0||node.ChildTreeRoot!=0||node.PageIndex!=0||node.PageSlot!=i||node.Balance is <-1 or >1)
                throw new InvalidDataException("Fresh root-only AVL fields differ.");
            ValidateLink(node.Left);ValidateLink(node.Right);
        }
        // EXACT25 native store domains. Untouched fields/array contents have NO
        // synthetic values; this type deliberately has no ReadResetCursor.Count API.
        var reset=new LegacyFst28CNumberResetWrite[25];int k=0;reset[k++]=new(0xc8,4,-1);
        for(int i=0;i<7;i++){reset[k++]=new(0x100+i*0x50,8,-1);reset[k++]=new(0x110+i*0x50,8,-1);reset[k++]=new(0x118+i*0x50,4,-1);}
        reset[k++]=new(0x330,8,-1);reset[k++]=new(0x340,8,-1);reset[k++]=new(0x348,4,0);
        for(int i=0;i<reset.Length;i++)for(int j=0;j<i;j++)
            if(reset[i].Offset<reset[j].Offset+reset[j].Width&&reset[j].Offset<reset[i].Offset+reset[i].Width)
                throw new InvalidDataException("Native reset store domains overlap.");
        // QWORD340 covers340..347; DWORD348 covers348..34B: adjacent, NOT overlapping.
        Checkpoint("before-publish");
        return new(d0,header,context,nodes,preserved,current,secondary,labelNodes,labelSlots,auxiliaries,prefixNodes,reset);
    }
    public bool RetainsExactHeaderIdentity(CancellationToken token=default)
    {token.ThrowIfCancellationRequested();return ReferenceEquals(historicalHeader,Bootstrap.RetainInitialNumberHeaderIdentity(D0,token))&&
        ReferenceEquals(request.Header,historicalHeader)&&ReferenceEquals(request.Cohort,cohort)&&ReferenceEquals(request.Owner,D0);}
    internal LegacyFst28CNumberRequestDiagnostic ReadRequestDiagnostic(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        return new(request.Tag,request.Header is not null,
            ReferenceEquals(historicalHeader,Bootstrap.RetainInitialNumberHeaderIdentity(D0,token)),
            ReferenceEquals(historicalHeader,request.Header),ReferenceEquals(cohort,request.Cohort),
            ReferenceEquals(D0,request.Owner),request.Parent is null,request.Source,request.Start,
            request.Span,request.Flags,request.Skip,request.Take,request.CachedOwner is null,LiveTreeRoot!=0);
    }
    // Genuine private prefix handoff; detached diagnostics are not accepted.
    internal LegacyFst28CNumberInitialPage.Input RetainInitialPageInput(object invocation, CancellationToken token)
    {
        token.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(invocation);
        if(!RetainsExactHeaderIdentity(token)||!ReferenceEquals(request.Owner,D0)||
            !ReferenceEquals(request.Header,historicalHeader)||!ReferenceEquals(request.Cohort,cohort)||
            request.Tag!=(8UL<<40)||request.Parent is not null||request.Source!=0||request.Start!=0||
            request.Span!=0||request.Flags!=0||request.Skip!=0||request.Take!=128||request.CachedOwner is not null||
            context.NodeCount!=18||context.RootTreeIdentity==0||nodes.Count!=18)
            throw new InvalidDataException("Private initial request/header/live cohort differs.");
        return new(invocation,this,request,historicalHeader,cohort,context,D0,request.Tag,
            request.Source,request.Start,request.Span,request.Flags,request.Skip,request.Take);
    }
    // Detached inspection cannot be accepted as a query/current-context receipt.
    public IReadOnlyList<LegacySymbolContextNode> ReadLiveNodes()=>nodes;
    public ushort ReadPreservedCode(ushort label)=>Read(preserved,label);
    public ushort ReadCurrentCode(ushort label)=>Read(current,label);
    public ushort ReadSecondaryCode(ushort label)=>Read(secondary,label);
    public ulong ReadSourceNode(ushort label){if(label>=16)throw new ArgumentOutOfRangeException(nameof(label));return labelNodes[label];}
    public int ReadExistingSlot(ushort label){if(label>=16)throw new ArgumentOutOfRangeException(nameof(label));return labelSlots[label];}
    public LegacyFst28CNumberAuxiliaryBinding ReadAuxiliary(int slot)
    {if((uint)slot>=15)throw new ArgumentOutOfRangeException(nameof(slot));return auxiliaries[slot];}
    public ulong ReadOuterPrefixNode(int ordinal)
    {if((uint)ordinal>=5)throw new ArgumentOutOfRangeException(nameof(ordinal));return prefixNodes[ordinal];}
    public LegacyFst28CNumberResetWrite ReadResetWrite(int ordinal)
    {if((uint)ordinal>=25)throw new ArgumentOutOfRangeException(nameof(ordinal));return resetWrites[ordinal];}
    private static ushort Read(ushort[] values,ushort label)
    {if(label>=16)throw new ArgumentOutOfRangeException(nameof(label));return values[label];}
}
