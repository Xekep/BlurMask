using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglishCasePrimaryCompletion.Probe")]
namespace InkRecognition;

//First native PRIMARY materialization only. No204290, parent88 or completed node.
internal sealed partial class LegacyEnglishCasePrimaryCompletion
{
    internal const long CompleteLogicalCeiling=127687945;
    internal const long AdditionalLogicalReserve=1048576;
    internal const long EnumeratedPayloadUpperBytes=690192;
    internal const int MaximumContextNodes=1506;
    private readonly LegacyEnglishCaseInitialConsumer consumer;
    private readonly LegacyEnglish119MainKnowledge knowledge;
    private readonly LegacyEnglishLkFreshNumericBinding numeric;
    private LegacyEnglishCasePrimaryCompletionReceipt? pending;
    private LegacyEnglishCaseInitialQueryPass? completionPass;
    private int phase,entered;
    private long payload,copyWork;
    private const long CopyWorkLimit=64L*1024*1024;
    private LegacyEnglishCasePrimaryCompletion(LegacyEnglishCaseInitialConsumer consumer,LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric)
    {this.consumer=consumer;this.knowledge=knowledge;this.numeric=numeric;}
    internal static LegacyEnglishCasePrimaryCompletion Prepare(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,
        LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,long maximumLogicalBytes=128L*1024*1024,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(knowledge);ArgumentNullException.ThrowIfNull(numeric);token.ThrowIfCancellationRequested();
        if(maximumLogicalBytes<CompleteLogicalCeiling||maximumLogicalBytes>128L*1024*1024||!ReferenceEquals(knowledge.Source,numeric.DataOwner.Source))
            throw new ArgumentException("Complete same-source primary-only logical envelope required before claim.");
        if(1289+217!=MaximumContextNodes||MaximumContextNodes>LegacyEnglishCaseExecutionPoolReceipt.Capacity)throw new InvalidDataException("Complete context preflight required.");
        var consumer=LegacyEnglishCaseInitialQueryPass.PreparePrimaryConsumer(knowledge,numeric,cases,publication,maximumLogicalBytes,token);
        var result=new LegacyEnglishCasePrimaryCompletion(consumer,knowledge,numeric);consumer.AttachPrimaryCompletion(result);return result;
    }
    internal bool IsPreparedFor(LegacyEnglishCaseInitialConsumer value)=>Volatile.Read(ref phase)==0&&ReferenceEquals(consumer,value);
    internal bool IsRunningFor(LegacyEnglishCaseInitialConsumer value)=>Volatile.Read(ref phase)==1&&ReferenceEquals(consumer,value)&&
        knowledge.Owner.IsComplete&&!knowledge.Owner.IsFaulted&&knowledge.Owner.Owner.IsComplete&&!knowledge.Owner.Owner.IsFaulted;
    internal LegacyEnglishCasePrimaryCompletionReceipt Run(Action<string>? observer=null,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if(Interlocked.CompareExchange(ref phase,1,0)!=0)throw new InvalidOperationException("One primary completion execution required.");
        try
        {
            _=consumer.Run(observer,token);token.ThrowIfCancellationRequested();
            var result=pending??throw new InvalidOperationException("No completed primary materialization.");
            Volatile.Write(ref phase,2);return result;
        }
        catch{pending=null;Volatile.Write(ref phase,3);if(completionPass is not null)completionPass.Root.FaultInitialExecutionOwner(completionPass);throw;}
    }
    private void Charge(long bytes,long work=0)
    {
        if(bytes<0||work<0||bytes>EnumeratedPayloadUpperBytes-payload||work>CopyWorkLimit-copyWork)
            throw new InvalidDataException("Whole primary copy/work allowance exhausted before allocation.");
        payload+=bytes;copyWork+=work;
    }
    internal void CompleteOwned(LegacyEnglishCaseInitialConsumer actual,LegacyEnglishCaseInitialHeap heap,
        LegacyEnglishCaseInitialSourcePolicy policy,LegacyEnglishFreshCaseMainSource.InitialQueryLease lease,CancellationToken token)
    {
        actual.RequirePrimaryCompletion(this,heap,policy,lease);token.ThrowIfCancellationRequested();
        if(Interlocked.CompareExchange(ref entered,1,0)!=0||!ReferenceEquals(policy.Knowledge,knowledge)||!ReferenceEquals(policy.Numeric,numeric)||
            !ReferenceEquals(lease.Root.Knowledge,knowledge)||!ReferenceEquals(lease.Root.NumericOwner,numeric))
            throw new InvalidOperationException("Exact privately issued primary completion/source required.");
        completionPass=lease.Owner;
        var contexts=lease.Owner.Initializer.PlanForAudit.BindingForAudit.Contexts;
        var configuration=policy.Heaps.Configuration;
        if(lease.Owner.Initializer.PlanForAudit.ContextCapacity!=2048||contexts.NodeCount>1289||configuration.PrimaryArcLimit230a!=125||
            configuration.PrimaryAlternativeCount2402!=2||configuration.SecondaryAlternativeCount2404!=1||configuration.Trace2368!=0||
            configuration.Mode22f6!=0||(configuration.PackedSource22e8&7)!=3||policy.Heaps.SecondaryShareEc!=0||policy.Heaps.Limits.Count!=1)
            throw new NotSupportedException("Exact native first primary2/secondary1/one-general policy required.");
        //Full-capacity transfer happens only while the old source lease is active.
        Charge(250*128);var transfer=heap.TransferPrimary(this,actual,policy,lease,token);
        if(transfer.Storage.Length!=250*128||(uint)transfer.Count>250)throw new InvalidDataException("Actual live general heap250 required.");
        int count=transfer.Count,before=contexts.NodeCount;
        Charge(250*4,250*128);var ordinals=new int[250];Array.Fill(ordinals,-1);
        for(int i=0;i<count;i++)
        {
            token.ThrowIfCancellationRequested();actual.RequirePrimaryCompletion(this,heap,policy,lease);
            var slot=transfer.Storage.Span.Slice(i*128,128);
            if(slot[4]!=1||slot[0x20]!=0||BinaryPrimitives.ReadInt32LittleEndian(slot[0x38..])!=0||
                BinaryPrimitives.ReadUInt16LittleEndian(slot[0x3c..])!=ushort.MaxValue)throw new NotSupportedException("Actual source1/node0/span0 startup anchor required.");
            var leaf=contexts.ReadNode(BinaryPrimitives.ReadUInt64LittleEndian(slot[8..]));
            if(leaf.Symbol>=217||leaf.ParentLink==0)throw new NotSupportedException("Actual FFFA-to-ordinary-AL suffix required.");
            var root=contexts.ReadNode(leaf.ParentLink);
            if(root.Symbol!=0xfffa||root.ParentLink!=0)throw new NotSupportedException("No deeper/foreign primary suffix admitted.");
            BinaryPrimitives.WriteInt32LittleEndian(slot[0x58..],i);
        }
        //Fixed producer/ordering call counts are charged before unchanged kernels.
        Charge(3*250*80+2L*count*(8+60)+217*40+3*4080+65536,3L*count*128);
        var residue=new byte[250*80];
        var rootAlternatives=LegacyNativeRootWordAlternativeProducer.Create(transfer.Storage.Span,count,0,configuration,contexts,Identity,residue,[],token);
        if(contexts.NodeCount>MaximumContextNodes||contexts.NodeCount-before>217)throw new InvalidDataException("Primary mapped-root context stage exceeded its preflight.");
        var pool=new LegacyNativeWordAlternativeBuffer(2,rootAlternatives.PrimaryStorage.Span);
        //Sort worst-case swaps, compaction, rebuild and quota hole copies. This
        //is a reserved upper bound, never an invented count of actual moves.
        long sortWork=3L*128*count*count+128L*count+16L*128*count+40L*8*count;
        Charge(2*4+4*250*128+250*128+8192+250*80,sortWork);
        var ordered=LegacyNativeWordAlternativeOrdering.Prepare(transfer.Storage.Span,count,false,configuration,[0,0xc0e],false,pool,null,token);
        Charge(2*250*128);var backing=ordered.Heap.GetSlotStorageSnapshot();
        var descriptor=new LegacyNativeWordHeapDescriptor(backing.Span,250,ordered.Heap.Count,0,policy.Heaps.Limits[0]);
        if(!LegacyNativeWordQuotaController.Apply([descriptor],configuration,0,0,cancellationToken:token))throw new InvalidDataException("Actual phase0 quota failed.");
        if(descriptor.Count>125||descriptor.Capacity>250||descriptor.AllocatedCapacity!=250)throw new InvalidDataException("Native retained backing/quota bound required.");
        Charge(250*128);var retained=descriptor.GetBackingStorageSnapshot();
        Charge(125*(88+80)+2*125*(88+80)+65536);var rows=new byte[125*88];var alternatives=new byte[125*80];
        var rowMasks=new byte[rows.Length];var alternativeMasks=new byte[alternatives.Length];
        for(int i=0;i<125;i++)
        {
            var row=rows.AsSpan(i*88,88);var alt=alternatives.AsSpan(i*80,80);
            BinaryPrimitives.WriteInt32LittleEndian(row[0x10..],0x4000000);BinaryPrimitives.WriteInt32LittleEndian(row[0x14..],0x4000000);
            rowMasks.AsSpan(i*88+0x10,8).Fill(255);
            BinaryPrimitives.WriteInt32LittleEndian(alt,0x4000000);BinaryPrimitives.WriteInt32LittleEndian(alt[4..],0x4000000);
            alternativeMasks.AsSpan(i*80,8).Fill(255);
        }
        for(int i=0;i<descriptor.Count;i++)
        {
            token.ThrowIfCancellationRequested();actual.RequirePrimaryCompletion(this,heap,policy,lease);
            Charge(88+80,88+80);var slot=retained.Span.Slice(i*128,128);
            ordinals[i]=BinaryPrimitives.ReadInt32LittleEndian(slot[0x58..]);
            var stored=LegacyNativeWordRowMaterializer.Materialize(slot,(ushort)i,125,2,1,ordered.PrimaryAlternativeStorage.Span,cancellationToken:token);
            stored.Record.Span.CopyTo(rows.AsSpan(i*88,88));stored.PrimaryAlternatives.Span.CopyTo(alternatives.AsSpan(i*80,80));
            rowMasks.AsSpan(i*88,88).Fill(255);
            for(int j=0;j<2;j++)
            {
                var alt=alternatives.AsSpan(i*80+j*40,40);var mask=alternativeMasks.AsSpan(i*80+j*40,40);
                mask[..8].Fill(255);mask[0x1e]=255;
                if(BinaryPrimitives.ReadInt32LittleEndian(alt)<0x4000000){mask[8..0x1b].Fill(255);mask[0x1f]=0x3f;}
            }
        }
        //Reserve one defensive export, including masks and the ordinal vector.
        Charge(2*125*(88+80)+250*4);
        token.ThrowIfCancellationRequested();actual.RequirePrimaryCompletion(this,heap,policy,lease);
        pending=new(rows,alternatives,rowMasks,alternativeMasks,ordinals,descriptor.Count,count,contexts.NodeCount,3*count,payload,copyWork);
        node0?.CompleteOwned(this,actual,heap,policy,lease,descriptor,rows,alternatives,rowMasks,alternativeMasks,ordinals,count,token);
    }
    private static ushort Identity(ushort symbol)=>symbol; //Pinned native1F8300; never caller supplied.
}
