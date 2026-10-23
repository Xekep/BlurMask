using System.Buffers.Binary;
namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseInitialQueryPass
{
    private LegacyEnglishCaseInitialConsumer? initialConsumer;
    internal static LegacyEnglishCaseInitialConsumer PreparePrimaryConsumer(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,
        LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,long maximumLogicalBytes=128L*1024*1024,CancellationToken token=default)
    {
        // Every large source/budget preflight precedes issuing this private pass.
        var policy=LegacyEnglishCaseInitialSourcePolicy.Prepare(knowledge,numeric,maximumLogicalBytes,token);
        var frontend=knowledge.Owner.RetainInitialConsumerSource(knowledge,token);
        var heap=new LegacyEnglishCaseInitialHeap(policy);
        var pass=Prepare(knowledge,numeric,cases,publication,maximumLogicalBytes,token:token);
        try
        {
            policy.Bind(pass);frontend.Bind(pass);var consumer=new LegacyEnglishCaseInitialConsumer(pass,policy,frontend,heap);pass.initialConsumer=consumer;
            token.ThrowIfCancellationRequested();return consumer;
        }
        catch{Volatile.Write(ref pass.phase,3);pass.Root.FaultInitialExecutionOwner(pass);throw;}
    }
}
// Complete first node0/span0 PRIMARY allocation prefix. No terminal/winner/history.
internal sealed partial class LegacyEnglishCaseInitialConsumer
{
    private readonly LegacyEnglishCaseInitialQueryPass pass;
    private readonly LegacyEnglishCaseInitialSourcePolicy policy;
    private readonly LegacyEnglishCaseInitialFrontend frontend;
    private readonly LegacyEnglishCaseInitialHeap heap;
    private readonly LegacyEnglishCaseInitialAcousticRequest request;
    private readonly Func<int,ushort,LegacyNativeWordAcousticGroupCosts> readCosts;
    private readonly Func<int,ushort,bool> suppressed;
    private LegacyEnglishCaseInitialHeapSnapshot? pending;
    private LegacyEnglishFreshCaseMainSource.InitialQueryLease? issuedLease;
    private LegacyEnglishCasePrimaryCompletion? completion;
    private bool completing;
    private int completionThread;
    private int run,rows,qualified,scoring;private bool currentPrepared;
    internal int MaximumContextNodes=>1289;
    internal int CompletedRows=>Volatile.Read(ref rows);
    internal int QualifiedRows=>Volatile.Read(ref qualified);
    internal int AttemptedScoringCalls=>Volatile.Read(ref scoring);
    internal long LogicalCeiling=>LegacyEnglishCaseInitialSourcePolicy.CompleteLogicalCeiling;
    internal void AttachPrimaryCompletion(LegacyEnglishCasePrimaryCompletion owner)
    {
        if(Volatile.Read(ref run)!=0||completion is not null||!owner.IsPreparedFor(this))throw new InvalidOperationException("One exact prepared primary completion owner required.");
        completion=owner;
    }
    internal void RequirePrimaryCompletion(LegacyEnglishCasePrimaryCompletion owner,LegacyEnglishCaseInitialHeap actualHeap,
        LegacyEnglishCaseInitialSourcePolicy actualPolicy,LegacyEnglishFreshCaseMainSource.InitialQueryLease lease)
    {
        RequireLease(lease);
        if(!completing||completionThread!=Environment.CurrentManagedThreadId||!ReferenceEquals(completion,owner)||!ReferenceEquals(heap,actualHeap)||!ReferenceEquals(policy,actualPolicy)||!owner.IsRunningFor(this))
            throw new InvalidOperationException("Exact synchronous live primary completion seam required.");
    }
    internal LegacyEnglishCaseInitialConsumer(LegacyEnglishCaseInitialQueryPass pass,LegacyEnglishCaseInitialSourcePolicy policy,LegacyEnglishCaseInitialFrontend frontend,LegacyEnglishCaseInitialHeap heap)
    {this.pass=pass;this.policy=policy;this.frontend=frontend;this.heap=heap;request=new(this);readCosts=frontend.ReadCosts;suppressed=frontend.Suppressed;}
    internal LegacyEnglishCaseInitialHeapSnapshot Run(Action<string>? observer=null,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if(Interlocked.CompareExchange(ref run,1,0)!=0)throw new InvalidOperationException("One primary consumer execution required.");
        try{_=pass.Run(observer,token);token.ThrowIfCancellationRequested();if(pass.IsFaulted||pending is null)throw new InvalidOperationException("Only completed owned primary prefix may publish.");Volatile.Write(ref run,2);return pending;}
        catch{Volatile.Write(ref run,3);throw;}
        finally{issuedLease=null;}
    }
    internal void RequireLease(LegacyEnglishFreshCaseMainSource.InitialQueryLease lease)
    {
        if(Volatile.Read(ref run)!=1||!ReferenceEquals(issuedLease,lease)||!ReferenceEquals(lease.Owner,pass)||!ReferenceEquals(pass.Initializer.RootForAudit,lease.Root))throw new InvalidOperationException("Exact active privately issued consumer lease required.");policy.Require(lease);
    }
    internal void Consume(LegacyEnglishFreshCaseMainSource.InitialQueryLease lease,LegacyEnglishCaseInitialQueryRecord page,CancellationToken token)
    {
        // A rejected preentry/foreign/stale call must not publish its lease.
        if(Volatile.Read(ref run)!=1)throw new InvalidOperationException("Executing private consumer required before issuing its lease.");
        policy.Require(lease);
        if(issuedLease is null)issuedLease=lease;RequireLease(lease);frontend.Require(lease,token);
        if(!ReferenceEquals(pass.CopyCompletedPages().LastOrDefault(),page)||page.Epoch!=lease.Epoch||page.Skip!=rows)throw new InvalidOperationException("Active page must be consumed exactly before native Skip advance.");
        if(!currentPrepared){frontend.PrepareCurrentCosts(lease,token);currentPrepared=true;}
        var binding=pass.Initializer.PlanForAudit.BindingForAudit;var contexts=binding.Contexts;
        var eligibility=(pass.D0??throw new InvalidOperationException("Actual pooled D0 required.")).CopyEligibility();
        var alphabet=policy.Knowledge.Source.Alphabet;
        ushort Resolve(string text)=>alphabet.TryGetLabel(text,out ushort label)?label:ushort.MaxValue;
        ushort loop=Resolve("\U000f0001"),shadda=Resolve("\u0651");
        ushort extra2=Resolve("\U000f0002"),extra3=Resolve("\U000f0003"),extra4=Resolve("\U000f0004"),extra8=Resolve("\U000f0008");
        var bytes=page.CopyRecords();if(bytes.Length!=page.Count*32)throw new InvalidDataException("Exact issued page extent required.");
        for(int at=0;at<bytes.Length;at+=32)
        {
            token.ThrowIfCancellationRequested();RequireLease(lease);var row=bytes.AsSpan(at,32);
            ulong tag=BinaryPrimitives.ReadUInt64LittleEndian(row),queryContext=BinaryPrimitives.ReadUInt64LittleEndian(row[8..]);
            ushort symbol=BinaryPrimitives.ReadUInt16LittleEndian(row[20..]);
            if((byte)(tag>>32)!=1||symbol>=217||row[22]!=0||row[23]!=0||BinaryPrimitives.ReadUInt64LittleEndian(row[24..])!=0)throw new NotSupportedException("Only actually proved first-row source/flags/length/padding branch admitted.");
            if(symbol==loop||symbol==shadda){rows++;continue;}
            // Native special-label extras can require a second allocation or
            // protocol dispatch. Not observed in this bounded actual corpus;
            // refuse the reached whole operation instead of silently omitting it.
            if(symbol==extra2||symbol==extra3||symbol==extra4||symbol==extra8)throw new NotSupportedException("Special-label startup extras require their separate native producer.");
            var leaf=contexts.ReadNode(queryContext);if(leaf.Symbol!=symbol)throw new InvalidDataException("Actual source context symbol required.");
            if(leaf.ParentLink!=0){var parent=contexts.ReadNode(leaf.ParentLink);if(parent.Symbol!=0xfffa||parent.ParentLink!=0)throw new NotSupportedException("Deeper quote/context rewrite requires its genuine producer.");}
            // Fresh FFFA(root)->AL has NULL grandparent; apostrophe ancestry
            // predicate is false. Frontend280 is false in sealed ordinary graph.
            ulong context=LegacyNativeWordStartupRequestFactory.GraftOrdinaryInitialContext(contexts,queryContext,token);
            if(eligibility[symbol]==0){rows++;continue;}qualified++;
            for(int variant=0;variant<2;variant++)
            {
                int category=(variant==0?frontend.Categories.StartCompositeMap:frontend.Categories.EndCompositeMap).Span[symbol];
                if(category<0){if(variant==0)throw new NotSupportedException("Missing primary category branch.");break;}
                if(category>63)throw new InvalidDataException("Six-bit native category required.");
                int geometry=frontend.Categories.CompositeCategories.Span[category].GeometryIndex;
                foreach(int ordinal in frontend.RootArcs)
                {
                    token.ThrowIfCancellationRequested();if(++scoring>4194304)throw new InvalidDataException("Whole primary scoring work cap exceeded.");
                    ushort arc=checked((ushort)ordinal);var accessor=request.Issue(lease,arc,symbol,geometry,BinaryPrimitives.ReadInt32LittleEndian(row[16..]),tag);
                    // No frontendA1 guess: native graphE8 is proven false from
                    // the sealed ordinary compact-byte6 producer.
                    if(!frontend.Kernel.ScoreCore(accessor,readCosts,true,suppressed,token))continue;
                    int start=frontend.StartContext(arc);
                    var fields=new LegacyInitialWordRecordFields(tag,context,request.Cost.Value,accessor.ReadInt32(48),symbol,0,arc,arc,(byte)category,65535,0,0,
                        !policy.SuppressHeaderFlag&&alphabet.IsSymbolFlagged(symbol)?(byte)1:(byte)0,0,0,0,0,0,0,-1,start,0,65535);
                    _=heap.Insert(fields,token);
                }
            }
            rows++;if(contexts.NodeCount>MaximumContextNodes)throw new InvalidDataException("Whole source context growth cap exceeded.");
        }
        RequireLease(lease);token.ThrowIfCancellationRequested();
    }
    internal void CompletePrefix(LegacyEnglishFreshCaseMainSource.InitialQueryLease lease,CancellationToken token)
    {
        RequireLease(lease);frontend.Require(lease,token);if(rows!=pass.ConsumedRows)throw new InvalidDataException("Complete primary consumer row prefix required.");
        if(completion is not null)
        {
            completionThread=Environment.CurrentManagedThreadId;completing=true;
            try{completion.CompleteOwned(this,heap,policy,lease,token);}
            finally{completing=false;completionThread=0;}
        }
        pending=heap.CopyHistorical();token.ThrowIfCancellationRequested();RequireLease(lease);
    }
    internal LegacyEnglishCaseInitialHeapSnapshot CopyHistoricalHeap()=>heap.CopyHistorical();
}
