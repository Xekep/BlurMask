using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

[assembly: InternalsVisibleTo("LegacyFst28CNumberInitialConsumer.Probe")]
namespace InkRecognition;

public readonly record struct LegacyFst28CNumberInitialCandidate(ulong SourceTag,ulong ContextIdentity,
    int Cost,int Increment,ushort Symbol,byte CompositeCategory,ushort SourceArc,
    int Auxiliary34,int PreviousNode,ulong PreviousContext);
internal sealed record LegacyFst28CNumberInitialAttempt(int Row,int Variant,ushort Arc,int Group,
    int Category,int Prototype,int ClassCost,int GeometryCost,byte[] RequestBefore,byte[] RequestAfter,
    int CostAfter,bool AcousticAccepted,bool HeapAccepted,LegacyInitialWordRecordFields? Fields);

/// <summary>Owned initial number qualifier/root/category/acoustic/heap branch.
/// Same private context pool is retained. No word-language selection, later query,
/// parent/history or accepted-text/default authority is supplied.</summary>
public sealed class LegacyFst28CNumberInitialConsumer
{
    public const int LogicalByteEnvelope=754206,WorkEnvelope=16264293;
    private const string AcousticHash="7A5F08809E81FB4A2D4C78B3EFD251D0986503B0D30D21DE3695DAB1F8A89978";
    private sealed class Claim {internal int Taken;}
    private static readonly ConditionalWeakTable<LegacyFst28CNumberInitialConsumerContext,Claim> Claims=new();
    internal LegacyFst28CNumberInitialConsumerContext ExpectedContext {get;}
    private LegacyFst28CNumberInitialConsumerContext.Pool? pool;
    private LegacyFst28CNumberConsumerGraft[] grafts=[];
    private ushort[] roots=[];
    private readonly object cohort=new();
    private LegacyNativeWordRecordHeap? heap;
    private ReadOnlyMemory<byte> historicalHeap;
    private Model? model;
    private int skippedLanguageCompletionClaim;
    private int attached,state;
    private int qualified,attempts,acousticAccepted,heapAccepted,heapRejected,missingAlternates;
    public LegacyFst28CNumberInitialConsumerContext Context=>ExpectedContext;
    public LegacyFst28CNumberInitialD0 D0=>ExpectedContext.D0;
    public int CandidateCount {get{RequireCompleted();return heap!.Count;}}
    public int HeapCapacity=>250;
    public int RowsVisited=>grafts.Length;
    public int QualifiedRows=>qualified;
    public int ScoringAttempts=>attempts;
    public int AcousticAcceptances=>acousticAccepted;
    public int HeapAcceptances=>heapAccepted;
    public int HeapRejections=>heapRejected;
    public int MissingEndAlternates=>missingAlternates;
    public int LogicalByteReservation=>LogicalByteEnvelope;
    public int WorkReservation=>WorkEnvelope;
    public bool HasQueryAuthority=>false;
    public bool HasParentHistoryAuthority=>false;
    public bool HasAcceptedText=>false;
    public bool HasCompleteWordStartup=>false;
    private LegacyFst28CNumberInitialConsumer(LegacyFst28CNumberInitialConsumerContext context)
    {ExpectedContext=context;}

    public static LegacyFst28CNumberInitialConsumer CreateActualDirectNumberInitialConsumer(
        LegacyFst28CNumberInitialConsumerContext context,ReadOnlySpan<byte> acousticResource,
        CancellationToken token=default)
        =>CreateBounded(context,acousticResource,LogicalByteEnvelope,WorkEnvelope,token);

    internal static LegacyFst28CNumberInitialConsumer CreateBounded(
        LegacyFst28CNumberInitialConsumerContext context,ReadOnlySpan<byte> acousticResource,
        int byteBudget,int workBudget,CancellationToken token=default,Action<string>? checkpoint=null,
        Action<LegacyFst28CNumberInitialAttempt>? observer=null)
    {
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(context);
        if(byteBudget is <0 or >LogicalByteEnvelope)throw new ArgumentOutOfRangeException(nameof(byteBudget));
        if(workBudget is <0 or >WorkEnvelope)throw new ArgumentOutOfRangeException(nameof(workBudget));
        if(byteBudget<LogicalByteEnvelope||workBudget<WorkEnvelope)
            throw new InvalidDataException("Whole initial consumer envelope is required before copies/claim.");
        var d0=context.D0;var graph=d0.Graph;
        if(acousticResource.Length!=572763||context.Count is <0 or >12||d0.AcousticEvaluations is <1 or >64||
            d0.CurrentNode!=0||d0.CurrentSpan!=0||d0.ReusedPreviousCache||
            !ReferenceEquals(d0.Bootstrap.Graph,graph)||!ReferenceEquals(d0.Policy,d0.Bootstrap.InitialState.TargetPreparation)||
            !ReferenceEquals(d0.Source,d0.Policy.PrimaryMap.Source)||!d0.Source.HasActualFreshMain135Authority||
            d0.Source.ActualAcousticSourceKind!=0x135||
            !StringComparer.OrdinalIgnoreCase.Equals(d0.Source.ActualAcousticFingerprint,AcousticHash))
            throw new ArgumentException("Exact completed direct-number context and same MAIN135 resource required.");
        void Step(string name){token.ThrowIfCancellationRequested();checkpoint?.Invoke(name);token.ThrowIfCancellationRequested();}
        var receiver=new LegacyFst28CNumberInitialConsumer(context);
        context.AttachInitialConsumer(receiver,token);
        Step("before-snapshot");var snapshot=new byte[572763];bool claimed=false;
        try
        {
            for(int at=0;at<snapshot.Length;at+=65536)
            {token.ThrowIfCancellationRequested();acousticResource.Slice(at,Math.Min(65536,snapshot.Length-at)).CopyTo(snapshot.AsSpan(at));}
            Step("after-snapshot");
            if(Convert.ToHexString(SHA256.HashData(snapshot))!=AcousticHash)
                throw new ArgumentException("Authenticated private same acoustic snapshot required.");
            Step("after-hash");
            receiver.model=Model.Load(snapshot,d0,token);Step("after-model");
            var arcs=graph.State.GetNode(0);
            if(arcs.Count is <1 or >256||graph.State.MaximumAtoms!=8||graph.State.Root.SelectorCount!=9)
                throw new NotSupportedException("Actual node0/root compact cap8 required.");
            // Existing constructor bounds the entire compact index array to2048.
            // Charge4096 temporary bytes even before the narrower256 admission.
            receiver.roots=graph.State.Root.CopySelector(1);
            if(receiver.roots.Length>256)throw new NotSupportedException("Initial root ordinal cap256 exceeded.");
            var selected=d0.CopySelectedGroups(token);
            if(selected.Length is <1 or >64)throw new InvalidDataException("Actual selected D0 domain differs.");
            var cache=new Dictionary<int,LegacyNativeWordAcousticGroupCosts>();
            var rootMembership=new bool[arcs.Count]; //<=256 transient bytes.
            foreach(ushort ordinal in receiver.roots)
            {
                token.ThrowIfCancellationRequested();
                if(ordinal>=arcs.Count||arcs[ordinal].Span!=0||arcs[ordinal].GroupId>int.MaxValue)
                    throw new InvalidDataException("Same actual node0 span0 root ordinal required.");
                rootMembership[ordinal]=true;int group=(int)arcs[ordinal].GroupId;
                if(!selected.Contains(group))throw new InvalidDataException("Root group is not selected by this D0.");
                if(!cache.ContainsKey(group))cache.Add(group,d0.CopyInitialConsumerSingleRangeCosts(group,token));
            }
            var eligibility=d0.CopyEligibility(token);
            if(eligibility.Length!=195)throw new InvalidDataException("Complete195 eligibility required.");
            foreach(byte b in eligibility)if(b>1)throw new InvalidDataException("Eligibility must retain actual0/1 storage.");
            receiver.ValidateRows(receiver.model,token);Step("after-preflight");
            Step("before-claim");
            if(Interlocked.CompareExchange(ref Claims.GetValue(context,static _=>new Claim()).Taken,1,0)!=0)
                throw new InvalidOperationException("This exact completed context already claimed initial consumption.");
            claimed=true;Volatile.Write(ref receiver.state,1);Step("after-claim");
            receiver.heap=new LegacyNativeWordRecordHeap(250);
            // Scalar quotas only. Do NOT Initialize a separate descriptor backing
            // and copy it into another heap; the chosen owner allocates once.
            var router=new LegacyNativeWordHeapRouter([receiver.heap],receiver.model.Buckets,-1,uint.MaxValue);
            var scorer=LegacyNativeWordAcousticRequestScorer.ForAlphabet(receiver.model.Thresholds,receiver.model.Alphabet);
            var cell=new LegacyNativeWordTransitionCostCell(1,0);var residue=new byte[80];
            LegacyNativeWordAcousticGroupCosts Cost(int node,ushort ordinal)
            {
                if(node!=0||ordinal>=arcs.Count||!rootMembership[ordinal])
                    throw new InvalidDataException("Foreign scoring node/root ordinal.");
                return cache[(int)arcs[ordinal].GroupId];
            }
            for(int rowIndex=0;rowIndex<receiver.grafts.Length;rowIndex++)
            {
                Step("before-row-"+rowIndex);var graft=receiver.grafts[rowIndex];var row=graft.SourceRecord;
                //152520 length0 bypasses98; no empty-word-character owner inferred.
                if(eligibility[row.Symbol]==0)continue;
                receiver.qualified++;
                for(int variant=0;variant<2;variant++)
                {
                    int category=receiver.model.Categories.GetSymbolCompositeCategory(row.Symbol,endBoundary:variant!=0);
                    if(category<0){if(variant==0)throw new InvalidDataException("Missing actual start category.");receiver.missingAlternates++;break;}
                    int prototype=receiver.model.Profile.CompositeCategories.Span[category].GeometryIndex;
                    foreach(ushort ordinal in receiver.roots)
                    {
                        Step("before-score-"+receiver.attempts);
                        int group=(int)arcs[ordinal].GroupId;
                        var request=LegacyNativeWordStartupRequestFactory.Create(0,0,ordinal,row.Symbol,category,row.Tag,
                            row.SourceCategory,row.CaseFlags,1,0,receiver.model.Profile,cell,row.Cost,residue,token);
                        byte[] before=observer is null?[]:request.Storage.ToArray();
                        bool accepted=scorer.Score(request,Cost,false,cancellationToken:token);
                        receiver.attempts++;
                        if(receiver.attempts>6144)throw new InvalidDataException("Whole attempt cap exceeded.");
                        Step("after-score-"+(receiver.attempts-1));
                        LegacyInitialWordRecordFields? fields=null;bool allocated=false;
                        if(accepted)
                        {
                            receiver.acousticAccepted++;
                            int auxiliary=unchecked((int)arcs[ordinal].StartContextId);
                            fields=new(row.Tag,graft.GraftedContextIdentity,cell.Value,request.ReadInt32(48),row.Symbol,
                                0,ordinal,ordinal,checked((byte)category),ushort.MaxValue,0,0,
                                receiver.model.Alphabet.IsSymbolFlagged(row.Symbol)?(byte)1:(byte)0,0,0,
                                (byte)(row.CaseFlags&7),unchecked((byte)((row.CaseFlags>>5&3)+row.SourceCategory)),0,0,
                                (row.CaseFlags&8)!=0?auxiliary:-1,auxiliary,0,ushort.MaxValue);
                            Step("before-allocate-"+(receiver.attempts-1));
                            var allocation=router.TryInsertInitialRecord(1,false,fields,0,false,cancellationToken:token);
                            allocated=allocation.Accepted;
                            if(allocated)receiver.heapAccepted++;else receiver.heapRejected++;
                            Step("after-allocate-"+(receiver.attempts-1));
                        }
                        if(observer is not null)
                        {
                            // Detached per-inspection copies only; never cost cell,
                            // live request storage, pool, Binding or heap ownership.
                            observer(new(rowIndex,variant,ordinal,group,category,prototype,cache[group].ClassCosts.Span[row.Symbol],
                                0,before,request.Storage.ToArray(),cell.Value,accepted,allocated,fields));
                            token.ThrowIfCancellationRequested();
                        }
                    }
                }
            }
            Step("before-heap-snapshot");receiver.historicalHeap=receiver.heap.GetSlotStorageSnapshot();
            if(receiver.historicalHeap.Length!=250*128||receiver.heap.Count>250)
                throw new InvalidDataException("Actual private bounded initial heap differs.");
            receiver.ValidateRetainedPool(token);Step("before-publish");
            Volatile.Write(ref receiver.state,2);return receiver;
        }
        catch{if(claimed)Volatile.Write(ref receiver.state,3);throw;}
        finally{CryptographicOperations.ZeroMemory(snapshot);}
    }

    internal void AcceptTrustedContext(LegacyFst28CNumberInitialConsumerContext context,
        LegacyFst28CNumberInitialConsumerContext.Pool actualPool,
        ReadOnlySpan<LegacyFst28CNumberConsumerGraft> actualRows,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(context,ExpectedContext)||!ReferenceEquals(actualPool.Owner,context.D0)||
            actualRows.Length!=context.Count||actualRows.Length>12||pool is not null||
            Interlocked.CompareExchange(ref attached,1,0)!=0)
            throw new ArgumentException("Exact one-use typed receiving context/pool required.");
        pool=actualPool;grafts=actualRows.ToArray();ValidateRetainedPool(token);
    }
    private void ValidateRetainedPool(CancellationToken token)
    {
        var p=pool??throw new InvalidOperationException("No trusted pool.");var d=p.ReadDiagnostic(18+grafts.Length,token);
        if(!d.Completed||!d.SameD0||!d.SameBootstrap||!d.SameGraph||!d.SameSource||!d.SameHeader||
            !d.SameBindingAndPage||!d.HistoricalInitialViewRetained||d.ActiveNodes!=18+grafts.Length||d.Pages!=1||
            !ReferenceEquals(p.Owner,D0))throw new InvalidOperationException("Retained actual completed pool differs.");
    }
    private void ValidateRows(Model model,CancellationToken token)
    {
        var context=pool!.OwnedContext;
        foreach(var g in grafts)
        {
            token.ThrowIfCancellationRequested();var r=g.SourceRecord;
            if(!g.Created||r.Symbol>=192||r.SourceCategory!=0||r.CaseFlags!=0||r.Extra!=0||(byte)(r.Tag>>32)!=1||
                r.ContextIdentity==g.GraftedContextIdentity||
                context.ReadNode(r.ContextIdentity) is not {ParentLink:0,ChildTreeRoot:0} original||original.Symbol!=r.Symbol||
                context.ReadNode(g.GraftedContextIdentity) is not {ChildTreeRoot:0} child||child.Symbol!=r.Symbol||
                child.ParentLink==0||context.ReadNode(child.ParentLink) is not {Symbol:0xfffa,ParentLink:0}||
                model.Categories.GetSymbolCompositeCategory(r.Symbol)<0)
                throw new InvalidDataException("Exact ordinary source leaf/grafted child/category required.");
            // The fixed first normal number page emits only digits and two signs.
            if(r.Symbol>9&&r.Symbol!=138&&r.Symbol!=185)
                throw new NotSupportedException("Only actual initial number digit/sign labels are supported.");
            for(int v=0;v<2;v++)
            {
                int c=model.Categories.GetSymbolCompositeCategory(r.Symbol,endBoundary:v!=0);
                if(c<0){if(v==0)throw new InvalidDataException("Actual start map missing.");break;}
                if(c>63||model.Profile.CompositeCategories.Span[c].GeometryIndex is <0 or >=19)
                    throw new InvalidDataException("Actual six-bit composite/signed prototype differs.");
            }
        }
    }
    private void RequireCompleted()
    {if(Volatile.Read(ref state)!=2)throw new InvalidOperationException("No completed owning initial heap.");}
    // Same-receiver scalar claim: no shared table, allocation or resize.
    internal void ClaimSkippedLanguageCompletion(LegacyFst28CNumberSkippedLanguageCompletion receiver)
    {
        ArgumentNullException.ThrowIfNull(receiver);
        if(!ReferenceEquals(receiver.ExpectedConsumer,this))throw new ArgumentException("Foreign receiving Number consumer.");
        RequireCompleted();
        if(Interlocked.CompareExchange(ref skippedLanguageCompletionClaim,1,0)!=0)
            throw new InvalidOperationException("This exact initial Number consumer already claimed node0 completion.");
    }
    // The retained TRUE path shares the original four-byte completion CAS.
    internal void ClaimTrueLanguageCompletion(LegacyFst28CNumberTrueLanguageSourceDraft receiver,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(receiver);receiver.ValidateReceivingConsumer(this,token);
        RequireCompleted();ValidateRetainedPool(token);
        if(Interlocked.CompareExchange(ref skippedLanguageCompletionClaim,1,0)!=0)
            throw new InvalidOperationException("This exact initial Number consumer already claimed node0 completion.");
    }
    internal void AttachTrueLanguageCompletion(LegacyFst28CNumberTrueLanguageSourceDraft receiver,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(receiver);receiver.ValidateReceivingConsumer(this,token);
        RequireCompleted();ValidateRetainedPool(token);
        receiver.AcceptTrustedConsumer(this,pool!,heap!,token);
    }
    internal bool ReadSkippedLanguageCompletionClaim()=>Volatile.Read(ref skippedLanguageCompletionClaim)!=0;
    // Trusted receiving read only; no snapshot grants completion authority.
    internal void AttachSkippedLanguageCompletion(LegacyFst28CNumberSkippedLanguageCompletion receiver,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(receiver);
        if(!ReferenceEquals(receiver.ExpectedConsumer,this))throw new ArgumentException("Foreign receiving Number consumer.");
        RequireCompleted();ValidateRetainedPool(token);
        receiver.AcceptTrustedInitial(this,pool!,heap!,grafts,token);
    }
    public LegacyFst28CNumberInitialCandidate ReadCandidate(int index)
    {
        RequireCompleted();if((uint)index>=(uint)heap!.Count)throw new ArgumentOutOfRangeException(nameof(index));
        var b=historicalHeap.Span.Slice(index*128,120);
        return new(BinaryPrimitives.ReadUInt64LittleEndian(b),BinaryPrimitives.ReadUInt64LittleEndian(b[8..]),
            BinaryPrimitives.ReadInt32LittleEndian(b[16..]),BinaryPrimitives.ReadInt32LittleEndian(b[20..]),
            BinaryPrimitives.ReadUInt16LittleEndian(b[36..]),(byte)(BinaryPrimitives.ReadUInt32LittleEndian(b[24..])>>16&63),
            BinaryPrimitives.ReadUInt16LittleEndian(b[28..]),BinaryPrimitives.ReadInt32LittleEndian(b[52..]),
            BinaryPrimitives.ReadInt32LittleEndian(b[104..]),BinaryPrimitives.ReadUInt64LittleEndian(b[112..]));
    }
    /// <summary>Known initialized bytes0..77 only. Native78..7F residue excluded.</summary>
    public byte[] CopyInitializedHeapRecord(int index)
    {RequireCompleted();if((uint)index>=(uint)heap!.Count)throw new ArgumentOutOfRangeException(nameof(index));return historicalHeap.Slice(index*128,120).ToArray();}
    public ushort[] CopyRootOrdinals(CancellationToken token=default)
    {token.ThrowIfCancellationRequested();RequireCompleted();return roots.ToArray();}
    internal int[] ReadOwnerDiagnostic(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();RequireCompleted();ValidateRetainedPool(token);
        return [1,ReferenceEquals(pool!.Owner,D0)?1:0,ReferenceEquals(D0.Graph,Context.D0.Graph)?1:0,
            ReferenceEquals(D0.Source,Context.D0.Source)?1:0,18+grafts.Length,pool.OwnedContext.NodeCount,pool.PageCount,
            roots.Length,heap!.Capacity,heap.Count,1,0,0,0];
    }
    internal static void ValidateAssociation(LegacyFst28CNumberInitialConsumerContext expected,
        LegacyFst28CNumberInitialConsumerContext actual,CancellationToken token=default)
    {token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(expected);ArgumentNullException.ThrowIfNull(actual);actual.AttachInitialConsumer(new(expected),token);}

    private sealed class Model
    {
        internal readonly LegacyCursiveCategoryProfile Profile;
        internal readonly LegacyCursiveCategoryMapper Categories;
        internal readonly LegacyInkAlphabet Alphabet;
        internal readonly LegacyAcousticCandidateThresholds Thresholds;
        internal readonly byte[] Buckets;
        private Model(LegacyCursiveCategoryProfile profile,LegacyInkAlphabet alphabet,
            LegacyAcousticCandidateThresholds thresholds,byte[] buckets)
        {Profile=profile;Categories=new(profile);Alphabet=alphabet;Thresholds=thresholds;Buckets=buckets;}
        internal static Model Load(ReadOnlySpan<byte> snapshot,LegacyFst28CNumberInitialD0 d0,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(BinaryPrimitives.ReadUInt32LittleEndian(snapshot[5233..])!=0x135||
                BinaryPrimitives.ReadUInt32LittleEndian(snapshot[188165..])!=0x297||snapshot[5279]!=0||snapshot[5280]!=0||
                BinaryPrimitives.ReadUInt16LittleEndian(snapshot[572667..])!=125||
                BinaryPrimitives.ReadInt32LittleEndian(snapshot[189363..])!=0x4000000)
                throw new InvalidDataException("Actual MAIN135/sampler297/default125/single-range geometry cap required.");
            var selected=LegacySourceSamplerCategoryOwner.ReadV10dSource(snapshot,0,token);
            if(selected.SamplerNodeOffset!=188157||selected.CategoryProfileOffset!=188222||
                selected.Categories.BaseEntryCount!=192||selected.Categories.CompositeCategories.Length!=35||selected.Categories.Prototypes.Length!=19)
                throw new InvalidDataException("Exact private actual main category profile required.");
            var alphabet=LegacyInkAlphabet.Load(snapshot.Slice(99,5134),0x103);
            if(alphabet.LabelCount!=192||alphabet.TryGetLabel("\u202b",out _))
                throw new NotSupportedException("Exact fresh no-embedding alphabet required.");
            var thresholds=new LegacyAcousticCandidateThresholds(BinaryPrimitives.ReadInt32LittleEndian(snapshot[566390..]),
                BinaryPrimitives.ReadInt32LittleEndian(snapshot[566394..]),BinaryPrimitives.ReadInt32LittleEndian(snapshot[566398..]));
            if(thresholds!=new LegacyAcousticCandidateThresholds(20480,24576,32768))
                throw new InvalidDataException("Actual v110 tail thresholds differ.");
            var metadata=d0.Bootstrap.ReadSourceMetadata(token);
            if(metadata!=new LegacyFst28CNumberSourceMetadata(1,0,1,0,1,uint.MaxValue,1))
                throw new InvalidDataException("Exact implicit0/selected-number1 source descriptor required.");
            // NEW constructor-only private scalar state, bound to this typed
            // bootstrap. It does not claim an old full controller was retained.
            LegacyNativeWordPreparedSource[] sources=[new(0,0,1),new(metadata.Header,metadata.Weight24,metadata.Weight28)];
            var summary=LegacyNativeWordHeapPreparation.SummarizeFresh(sources,token);
            var refresh=LegacyNativeWordScalarRefresh.Apply(0,summary.PackedSource22e8,false,0,0,0,-1,0,false);
            var fresh=LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with{PackedSource22e8=refresh.PackedSource22e8};
            var limits=LegacyNativeWordControllerLimitPreparation.Prepare(fresh,125,LegacyNativeWordKnowledgeLimits.CreateFresh00c860(),true);
            var prepared=LegacyNativeWordHeapPreparation.PrepareFresh(sources,limits.Configuration,0,token);
            var c=prepared.Configuration;
            if(summary.PackedSource22e8!=0||refresh.MaximumWordLength68!=0||limits.RequiresAlternativePoolReset||
                c.HeaderCount2400!=2||c.PrimaryAlternativeCount2402!=2||c.SecondaryAlternativeCount2404!=2||
                c.PrimaryArcLimit230a!=125||c.ExpansionFactor230c!=.25f||c.Mode22f6!=0||c.Trace2368!=0||c.SuppressPreviousContext2609!=0||
                c.OverrideHeap2458!=uint.MaxValue||prepared.Limits.Count!=1||
                prepared.Limits[0]!=new LegacyNativeWordHeapQuotaLimits(uint.MaxValue,uint.MaxValue,250,uint.MaxValue,uint.MaxValue,124,-1))
                throw new InvalidDataException("Exact fresh source/order0/general250 heap policy differs.");
            var buckets=prepared.CopySourceBuckets().ToArray();
            if(!buckets.AsSpan().SequenceEqual(new byte[]{0,0}))throw new InvalidDataException("Exact source buckets required.");
            token.ThrowIfCancellationRequested();return new(selected.Categories,alphabet,thresholds,buckets);
        }
    }
}

public sealed partial class LegacyFst28CNumberInitialD0
{
    // NEW private-row projection; no NN/features/normalized/range diagnostic
    // clone. Compact group ranges are the SAME original193860 owner, not the
    // independently normalized chronological NN-range snapshot.
    internal LegacyNativeWordAcousticGroupCosts CopyInitialConsumerSingleRangeCosts(int group,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        foreach(var row in rows)
        {
            if(row.Group!=group)continue;
            var groups=Graph.State.Groups;
            if((uint)group>=(uint)groups.Count||groups[group].AllocationId!=group||groups[group].Kind07!=0||
                groups[group].CopyRanges().Length!=1||row.FinalCosts.Length!=195)
                throw new NotSupportedException("Actual selected original single-range geometry required.");
            Span<int> geometry=stackalloc int[19];geometry.Clear();
            token.ThrowIfCancellationRequested();return new(row.FinalCosts,geometry);
        }
        throw new ArgumentException("Group is not selected by this exact initial D0.");
    }
}
