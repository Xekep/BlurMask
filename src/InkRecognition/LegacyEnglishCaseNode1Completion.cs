using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglishCaseNode1.Probe")]
namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private LegacyEnglishCaseInitialQueryPass? node1Pass;
    private LegacyNativeDelimiterQuery? node1Query;
    private int node1Skip,node1Parent=-1;
    internal const long Node1AdditionalLogicalPayloadCeiling=32L*1024*1024;
    private static void PreflightNode1(LegacyEnglish119MainKnowledge knowledge,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var graph=knowledge.Owner.RetainCompletedD0Graph(token);
        if(graph.NodeCount is <2 or >4096||graph.Groups.Count>4096||graph.CandidateCount>4096||graph.GetNode(1).Count>1024)
            throw new NotSupportedException("Node1 requires the same bounded multi-node MAIN graph.");
        // Simultaneous logical payload envelope (not CLR/RSS): complete source +
        // geometry16Mi; MemoryStream capacity4Mi plus exported bytes4Mi;
        // <=1024 cache costs/dictionary/qualifier and diagnostic costs at4096B; node0 copied125 rows,
        // per-row80B alternatives/3array-header allowances; quota/comparer pools,
        // 128/88/40B materialization and transient page copies inside fixed2Mi.
        long reserve=16L*1024*1024+8L*1024*1024+1024L*4096+125L*256+2L*1024*1024;
        if(reserve>Node1AdditionalLogicalPayloadCeiling)throw new InvalidDataException("Whole separate node1 payload reservation unavailable.");
    }
    // Entry is separate from Run. OLD readiness and export gate stay closed.
    // This returns a primary continuation PREFIX, not a completed node/word.
    internal LegacyEnglishCaseNode1Snapshot RunNode1(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if(Interlocked.CompareExchange(ref phase,1,0)!=0)throw new InvalidOperationException("One owning node0/node1 invocation required.");
        thread=Environment.CurrentManagedThreadId;LegacyEnglishFreshCaseMainSource? root=null;
        try
        {
            _=primary.Run(token:token);token.ThrowIfCancellationRequested();
            if(!pending||ring is null||completedBacking is null||activeStage is not null)throw new InvalidOperationException("Actual after-FIRST/phase1 node0 state required.");
            node1Pass=primary.Node1Pass(this);var pass=node1Pass;root=pass.Root;
            var graph=pass.Initializer.PlanForAudit.BindingForAudit.Graph;
            if(graph.NodeCount<2||graph.GetNode(1).Count>1024)throw new NotSupportedException("Bounded actual node1 source required.");
            // Logical dedicated node1 payload/copy reservation: geometry <=16Mi,
            // rows/heaps/pools/metadata <=2Mi, accumulated query bytes <=4Mi.
            // Finite caps below, not CPU/RSS certification; OLD ledger untouched.
            var geometry=LegacyEnglishCaseNode1Geometry.Prepare(LegacyEnglishCaseNode1GeometryProfile.Prepare(root.Knowledge,token),token);
            var categories=geometry.Categories;var contexts=pass.Initializer.PlanForAudit.BindingForAudit.Contexts;
            var parents=new LegacyNativeStoredWordRecord[125];var parentRows=ring.CurrentRows;var parentAlts=ring.CurrentPrimary;
            for(int i=0;i<125;i++)
            {
                token.ThrowIfCancellationRequested();var row=parentRows.Slice(i*88,88);var mask=ring.CurrentRowMask.AsSpan(i*88,88);
                _=LegacyEnglishCaseNode1ParentMasks.Require(row,mask,parentAlts.Slice(i*80,80),ring.CurrentPrimaryMask.AsSpan(i*80,80),token);
                parents[i]=new(row.ToArray(),parentAlts.Slice(i*80,80).ToArray(),[]);
            }
            // Capture all owned node0 rows once, after phase1. No public snapshot
            // is consumed/promoted; the same private owner supplies these copies.
            root.ClaimNode1(this,pass,token);
            var d0=pass.SpanSession.PrepareSpanCore(1,0,false,null,token);
            var selected=d0.CopySelectedRows();var costs=new Dictionary<ushort,LegacyNativeWordAcousticGroupCosts>();
            foreach(var row in selected)
            {
                token.ThrowIfCancellationRequested();if(row.Anchor!=1||row.Span!=0||row.Costs.Length!=220)throw new InvalidDataException("Actual node1 D0 rows required.");
                costs.Add(checked((ushort)row.ArcIndex),geometry.CreateGroupCosts(1,checked((ushort)row.ArcIndex),row.Costs,token));
            }
            var qualifier=new LegacyNativeWordSymbolQualifier(217,costs.Values.ToArray(),cancellationToken:token);
            var lease=root.BeginNode1(this,pass,d0,token);var sourcePolicy=LegacyEnglishCaseInitialSourcePolicy.Prepare(root.Knowledge,root.NumericOwner,128L*1024*1024,token);
            var heap=new LegacyNativeWordRecordHeap(250);var h=sourcePolicy.Heaps;
            var router=new LegacyNativeWordHeapRouter([heap],h.CopySourceBuckets().Span,h.Sources.OverrideSourceIndex22f0,h.Configuration.OverrideHeap2458);
            var cell=new LegacyNativeWordTransitionCostCell(1,0);
            var kernel=LegacyNativeOrdinaryParentWordCandidates.ForEnglishNode1(geometry,contexts,
                LegacyNativeWordAcousticRequestScorer.ForAlphabet(ReadNode1Thresholds(root.Knowledge),root.Source.Alphabet),cell);
            int queryRows=0,scoring=0,accepted=0,queries=0;var frames=new List<LegacyEnglishCaseNode1QueryFrame>(625);using var observed=new MemoryStream();
            for(int p=0;p<125;p++)
            {
                token.ThrowIfCancellationRequested();root.ValidateNode1(lease);var parent=parents[p].Record.Span;
                if(BinaryPrimitives.ReadInt32LittleEndian(parent[16..])>=0x4000000)continue;
                // No omitted flag/soft-hyphen prepass is silently claimed.
                if((parent[6]&1)!=0||(parent[33]&128)!=0)throw new NotSupportedException("Reached special/disabled parent requires separate native branch.");
                var q=new LegacyPreparedDictionaryQuery{Capacity=128,ContextRoot=contexts.ContainerIdentity};
                var tag=LegacyOrdinaryDictionaryQueryPreparation.ApplyContinuation(q,new(0,0,false,false,120),parent,1,0,0,false,token);
                if(tag is null)continue;
                node1Query=new(){SourceTag=tag.Value,Request=q};node1Parent=p;node1Skip=0;int pages=0;
                while(true)
                {
                    if(++pages>5||++queries>625)throw new InvalidDataException("Node1 page count exceeded before query.");
                    var page=pass.Initializer.PlanForAudit.ReadNode1(lease,node1Query,token);
                    if(page.ReturnedCount is <0 or >128||page.Records.Length!=page.ReturnedCount*32||page.ReturnedCount>400-node1Skip||page.Records.Length>4*1024*1024-observed.Length)
                        throw new InvalidDataException("Bounded actual node1 page required.");
                    // Before score callbacks: refuse all reached unsupported row
                    // kinds and protocol symbols, never silently drop outputs.
                    for(int at=0;at<page.Records.Length;at+=32)
                    {
                        var row=page.Records.Span.Slice(at,32);ushort symbol=BinaryPrimitives.ReadUInt16LittleEndian(row[20..]);
                        if(row[23]!=0||row[22]!=0||!LegacyNativeOrdinaryWordSymbols.Supports(root.Source.Alphabet,symbol)||((BinaryPrimitives.ReadUInt64LittleEndian(row)>>32)&255)!=1)
                            throw new NotSupportedException("Only actual ordinary source1 kind0/length0 continuation admitted.");
                    }
                    long remaining=1048576L-scoring;
                    if((long)page.ReturnedCount*2*costs.Count>remaining)throw new InvalidDataException("Whole node1 scoring allowance unavailable before callbacks.");
                    frames.Add(new(p,node1Skip,node1Query.SourceTag,page.ReturnedCount));observed.Write(page.Records.Span);
                    var result=kernel.ProcessPage(page.Records.Span,1,0,0,(ushort)p,parent,0,qualifier,new Dictionary<byte,uint>{{1,0xc0e}},
                        (node,arc)=>node==1&&costs.TryGetValue(arc,out var cost)?cost:throw new InvalidDataException("Foreign node1 cache lookup."),
                        (node,arc)=>node==0&&arc<125?parents[arc].Record:throw new InvalidDataException("Foreign parent lookup."),router,0,false,sourcePolicy.SuppressHeaderFlag,0,false,false,null,token);
                    if(!result.Succeeded)throw new InvalidDataException("Node1 parent page failed.");
                    queryRows+=page.ReturnedCount;scoring+=result.ScoringCalls;accepted+=result.AcceptedAllocations;
                    node1Skip=checked(node1Skip+page.ReturnedCount);q.Skip=node1Skip;
                    if(page.ReturnedCount==0)break;
                }
            }
            node1Query=null;node1Parent=-1;
            var images=LegacyEnglishCaseNode1Materialization.Build(heap,h,categories,contexts,parents,root.Source.Alphabet,token);
            token.ThrowIfCancellationRequested();root.ValidateNode1(lease);root.EndNode1(lease);
            var resultSnapshot=new LegacyEnglishCaseNode1Snapshot(images,observed.ToArray(),queryRows,queries,scoring,accepted,selected.Length,
                CopyNode1Parents(parents,false),CopyNode1Parents(parents,true),
                selected.Select(r=>r.ArcIndex).ToArray(),selected.Select(r=>r.Costs.ToArray()).ToArray(),frames.ToArray(),contexts.CopyNodes().ToArray(),
                ring.CurrentRowMask.AsSpan(0,125*88).ToArray(),ring.CurrentPrimaryMask.ToArray());
            pending=false;completedBacking=null;node1Pass=null;Volatile.Write(ref phase,2);return resultSnapshot;
        }
        catch
        {
            if(root is not null)root.FaultNode1(this);pending=false;completedBacking=null;pendingReceipt=null;activeStage=null;
            primary.FaultNode0Completion(this);Volatile.Write(ref phase,3);throw;
        }
        finally{node1Query=null;node1Pass=null;thread=0;}
    }
    internal void RequireNode1(LegacyEnglishFreshCaseMainSource root,LegacyEnglishCaseInitialQueryPass pass)
    {
        if(Volatile.Read(ref phase)!=1||thread!=Environment.CurrentManagedThreadId||!pending||ring is null||
            !ReferenceEquals(node1Pass,pass)||!ReferenceEquals(pass.Root,root)||!ReferenceEquals(primary.Node1Pass(this),pass))
            throw new InvalidOperationException("Exact same running private node0/node1 owner/thread required.");
    }
    internal void RequireNode1Query(LegacyEnglishFreshCaseMainSource.Node1QueryLease lease,LegacyNativeDelimiterQuery query)
    {
        RequireNode1(lease.Root,lease.Pass);
        if(!ReferenceEquals(node1Query,query)||(uint)node1Parent>=125||query.Request.Skip!=node1Skip||query.Request.Capacity!=128||query.Request.Parameter1c!=1||query.Request.Parameter20!=0||query.Request.ContextRoot!=lease.Pass.Initializer.PlanForAudit.BindingForAudit.Contexts.ContainerIdentity)
            throw new InvalidOperationException("Only this active parent continuation query is accepted.");
    }
    private static byte[] CopyNode1Parents(LegacyNativeStoredWordRecord[] parents,bool alternatives)
    {
        int width=alternatives?80:88;var bytes=new byte[125*width];
        for(int i=0;i<125;i++)(alternatives?parents[i].PrimaryAlternatives:parents[i].Record).Span.CopyTo(bytes.AsSpan(i*width,width));return bytes;
    }
    private static LegacyAcousticCandidateThresholds ReadNode1Thresholds(LegacyEnglish119MainKnowledge knowledge)
    {
        int Read(string name){var f=knowledge.MainWire.Fields.Single(f=>f.Owner=="21D"&&f.Destination==name);if(f.ElementSize!=4||f.Count!=1)throw new InvalidDataException("Actual MAIN threshold field required.");return BinaryPrimitives.ReadInt32LittleEndian(f.CopyBytes());}
        return new(Read("F0"),Read("F4"),Read("F8"));
    }
}
internal readonly record struct LegacyEnglishCaseNode1QueryFrame(int ParentOrdinal,int Skip,ulong SourceTag,int Returned);
internal sealed class LegacyEnglishCaseNode1Snapshot
{
    internal string Scope=>"ordinary201270/node1-span0-primary-prefix/no-flag-prepass/no-node1-language-terminal";
    private readonly byte[] heap,rows,alternatives,queries,parentRows,parentAlternatives,parentRowMasks,parentAlternativeMasks;
    private readonly int[] selectedOrdinals;
    private readonly int[][] selectedCosts;
    private readonly LegacyEnglishCaseNode1QueryFrame[] frames;
    private readonly LegacySymbolContextNode[] contexts;
    internal int Count{get;}internal int QueryRows{get;}internal int QueryPages{get;}internal int ScoringCalls{get;}internal int AcceptedAllocations{get;}internal int SelectedD0Rows{get;}
    internal LegacyEnglishCaseNode1Snapshot(LegacyEnglishCaseNode1Materialization.Images images,byte[] queries,int queryRows,int pages,int scoring,int accepted,int selected,
        byte[] parentRows,byte[] parentAlternatives,int[] selectedOrdinals,int[][] selectedCosts,LegacyEnglishCaseNode1QueryFrame[] frames,LegacySymbolContextNode[] contexts,byte[] parentRowMasks,byte[] parentAlternativeMasks)
    {this.parentRowMasks=parentRowMasks;this.parentAlternativeMasks=parentAlternativeMasks;this.parentRows=parentRows;this.parentAlternatives=parentAlternatives;this.selectedOrdinals=selectedOrdinals;this.selectedCosts=selectedCosts;this.frames=frames;this.contexts=contexts;heap=images.Heap128;rows=images.Rows88;alternatives=images.Alternatives80;this.queries=queries;Count=images.Count;QueryRows=queryRows;QueryPages=pages;ScoringCalls=scoring;AcceptedAllocations=accepted;SelectedD0Rows=selected;}
    internal byte[] CopyParentRowMasks()=>parentRowMasks.ToArray();internal byte[] CopyParentAlternativeMasks()=>parentAlternativeMasks.ToArray();
    internal byte[] CopyParentRows88()=>parentRows.ToArray();internal byte[] CopyParentAlternatives80()=>parentAlternatives.ToArray();
    internal int[] CopySelectedOrdinals()=>selectedOrdinals.ToArray();internal int[][] CopySelectedCosts()=>selectedCosts.Select(c=>c.ToArray()).ToArray();
    internal LegacyEnglishCaseNode1QueryFrame[] CopyQueryFrames()=>frames.ToArray();internal LegacySymbolContextNode[] CopyContexts()=>contexts.ToArray();
    internal byte[] CopyHeap128()=>heap.ToArray();internal byte[] CopyRows88()=>rows.ToArray();internal byte[] CopyAlternatives80()=>alternatives.ToArray();internal byte[] CopyQueryRows32()=>queries.ToArray();
}
