using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglishGraphLoop.ActualEntry")]
namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private LegacyEnglishCaseNode1Geometry? graphLoopGeometry;
    private LegacyEnglishCaseInitialSourcePolicy? graphLoopSourcePolicy;
    private bool graphLoopActive;
    private GraphLoopMetrics graphLoopMetrics;
    internal readonly record struct GraphLoopMetrics(int Nodes,int CompletedNodes,long QueryRows,long QueryPages,long ScoringCalls,long AcceptedAllocations,long SelectedD0Rows);
    internal GraphLoopMetrics GraphLoopDiagnostics=>Volatile.Read(ref phase)==2?graphLoopMetrics:throw new InvalidOperationException("Completed graph DATA only.");
    private int graphLoopNode,graphLoopSpan,graphLoopPrevious,graphLoopParent=-2,graphLoopSkip;
    private LegacyNativeDelimiterQuery? graphLoopQuery;
    private LegacyEnglishFreshCaseMainSource.Node1QueryLease? graphLoopLease;

    // One current-node/span owner. Nodes>=50 need native normalization;
    // >=120 also need intermediate publication before physical ring reuse.
    // Those services are not silently omitted from this bounded successor.
    internal LegacyEnglishCaseGraphNodeSnapshot RunGraphFirstLoop(CancellationToken token=default)=>RunGraphFirstLoopCore(false,token);
    private LegacyEnglishCaseGraphNodeSnapshot RunGraphFirstLoopCore(bool finalCompact,CancellationToken token,bool captureMainConfidence=false)
    {
        token.ThrowIfCancellationRequested();primary.RequireGraphLoop(this,token);
        if(Interlocked.CompareExchange(ref phase,1,0)!=0)throw new InvalidOperationException("One owning graph run required.");
        thread=Environment.CurrentManagedThreadId;graphLoopActive=true;graphFinalCompactRequested=finalCompact;
        LegacyEnglishFreshCaseMainSource? root=null;
        try
        {
            if(captureMainConfidence)graphConfidenceScorer=primary.AttachGraphMainConfidence(this,ledger,token);
            _=primary.Run(token:token);
            if(!pending||ring is null||completedBacking is null||activeStage is not null)throw new InvalidOperationException("Same completed node0 private state required.");
            node1Pass=primary.Node1Pass(this);var pass=node1Pass;root=pass.Root;
            var graph=pass.Initializer.PlanForAudit.BindingForAudit.Graph;
            if(graph.NodeCount is <2 or >49)throw new NotSupportedException("Current graph loop admits2..49 nodes; native50-node normalization and120-slot publication remain separate.");
            var geometry=LegacyEnglishCaseNode1Geometry.Prepare(LegacyEnglishCaseNode1GeometryProfile.Prepare(root.Knowledge,token),token);
            graphLoopGeometry=geometry;var categories=geometry.Categories;var contexts=pass.Initializer.PlanForAudit.BindingForAudit.Contexts;
            var sourcePolicy=LegacyEnglishCaseInitialSourcePolicy.Prepare(root.Knowledge,root.NumericOwner,128L*1024*1024,token);
            graphLoopSourcePolicy=sourcePolicy;
            var scorer=LegacyNativeWordAcousticRequestScorer.ForAlphabet(ReadNode1Thresholds(root.Knowledge),root.Source.Alphabet);
            var cell=new LegacyNativeWordTransitionCostCell(1,0);
            var ordinary=LegacyNativeOrdinaryParentWordCandidates.ForEnglishNode1(geometry,contexts,scorer,cell);
            var startup=new LegacyNativeOrdinaryWordStartup(geometry.ActualGraph,root.Source.Alphabet,categories,contexts,scorer,cell,new byte[80]);
            InitializeGraphController(pass,token);ring.EnableGraphCursor(token);root.ClaimNode1(this,pass,token);
            long totalQueries=0,totalRows=0,totalScores=0,totalAllocations=0,totalSelected=0;
            var ownedHistory=new Dictionary<int,LegacyNativeStoredWordRecord[]>();
            LegacyEnglishCaseGraphNodeSnapshot? last=null;
            for(int node=1;node<graph.NodeCount;node++)
            {
                graphLoopNode=node;token.ThrowIfCancellationRequested();
                var arcs=graph.GetNode(node);int maximum=LegacyEnglishCaseGraphSchedule.MaximumSpan(node,arcs,token);
                var heap=new LegacyNativeWordRecordHeap(250);var prep=sourcePolicy.Heaps;
                var router=new LegacyNativeWordHeapRouter([heap],prep.CopySourceBuckets().Span,prep.Sources.OverrideSourceIndex22f0,prep.Configuration.OverrideHeap2458);
                for(byte span=0;span<=maximum;span++)
                {
                    graphLoopSpan=span;graphLoopPrevious=LegacyEnglishCaseGraphSchedule.PreviousNode(node,span);graphLoopParent=-2;graphLoopQuery=null;
                    var d0=pass.SpanSession.PrepareSpanCore(node,span,false,null,token);
                    var costs=new Dictionary<ushort,LegacyNativeWordAcousticGroupCosts>();
                    foreach(var selected in d0.CopySelectedRows())
                    {
                        if(selected.Anchor!=node||selected.Span!=span)throw new InvalidDataException("Same current-node span D0 rows required.");
                        costs.Add(checked((ushort)selected.ArcIndex),geometry.CreateGraphGroupCosts(node,checked((ushort)selected.ArcIndex),selected.Costs,span,token));
                    }
                    totalSelected=checked(totalSelected+costs.Count);
                    var qualifier=new LegacyNativeWordSymbolQualifier(217,costs.Values.ToArray(),cancellationToken:token);
                    var lease=root.BeginGraphSpan(this,pass,d0,token);graphLoopLease=lease;
                    if(graphLoopPrevious==-1)ReadPages(-1,ReadOnlyMemory<byte>.Empty);
                    else
                    {
                        // Only native ordinary PRIMARY ordinals<125, in order;
                        // sourceEnd is used separately for anchor/history lookups.
                        var rows=ring.OwnedRowsAt(graphLoopPrevious);
                        for(int ordinal=0;ordinal<125;ordinal++)
                        {
                            token.ThrowIfCancellationRequested();var parent=rows.Slice(ordinal*88,88);
                            if(BinaryPrimitives.ReadInt32LittleEndian(parent[16..])>=0x4000000)break;
                            _=LegacyEnglishCaseNode1ParentMasks.Require(parent,ring.OwnedRowMaskAt(graphLoopPrevious).Slice(ordinal*88,88),
                                ring.OwnedPrimaryAt(graphLoopPrevious).Slice(ordinal*80,80),ring.OwnedPrimaryMaskAt(graphLoopPrevious).Slice(ordinal*80,80),token);
                            if((parent[6]&1)!=0||(parent[33]&128)!=0)throw new NotSupportedException("Reached flag/soft-hyphen prepass must not be omitted.");
                            ReadPages(ordinal,parent.ToArray());
                        }
                    }
                    graphLoopQuery=null;graphLoopParent=-2;
                    if(span==maximum)
                    {
                        var images=LegacyEnglishCaseNode1Materialization.BuildGraph(heap,prep,categories,contexts,node,OwnedRows,root.Source.Alphabet,token);
                        _=PrepareGraphLanguageInputs(lease,node,images,categories,true,token);
                        last=graphCompletedNode??throw new InvalidDataException("Same live graph node did not complete.");
                    }
                    root.EndGraphSpan(lease,span==maximum);graphLoopLease=null;

                    void ReadPages(int ordinal,ReadOnlyMemory<byte> parent)
                    {
                        var request=new LegacyPreparedDictionaryQuery{Capacity=128,ContextRoot=contexts.ContainerIdentity};
                        ulong? tag=ordinal<0?LegacyOrdinaryDictionaryQueryPreparation.ApplyInitial(request,new(0,0,false,false,120),checked((uint)node),span,0,false,token):
                            LegacyOrdinaryDictionaryQueryPreparation.ApplyContinuation(request,new(0,0,false,false,120),parent.Span,checked((uint)node),span,0,false,token);
                        if(tag is null)return;
                        graphLoopParent=ordinal;graphLoopSkip=0;graphLoopQuery=new(){SourceTag=tag.Value,Request=request};int pages=0;
                        while(true)
                        {
                            token.ThrowIfCancellationRequested();
                            if(++pages>5||++totalQueries>1048576)throw new InvalidDataException("Whole graph query-page bound exceeded before read.");
                            var page=pass.Initializer.PlanForAudit.ReadGraphSpan(lease,graphLoopQuery,token);
                            if(page.ReturnedCount is <0 or >128||page.Records.Length!=page.ReturnedCount*32||page.ReturnedCount>400-graphLoopSkip||totalRows+page.ReturnedCount>1048576)
                                throw new InvalidDataException("Actual finite graph query prefix required.");
                            ObserveGraphControllerWrite(page.Records.Span,token);
                            for(int at=0;at<page.Records.Length;at+=32)
                            {
                                var qrow=page.Records.Span.Slice(at,32);ushort label=BinaryPrimitives.ReadUInt16LittleEndian(qrow[20..]);
                                if(qrow[23]!=0||qrow[22]!=0||qrow[4]!=1||!LegacyNativeOrdinaryWordSymbols.Supports(root.Source.Alphabet,label))
                                    throw new NotSupportedException("Reached nonordinary query row requires its native producer.");
                            }
                            if((long)page.ReturnedCount*2*costs.Count>1048576-totalScores)throw new InvalidDataException("Whole graph scoring bound exceeded before callback.");
                            var headers=new Dictionary<byte,uint>{{1,0xc0e}};
                            LegacyNativeWordAcousticGroupCosts Lookup(int n,ushort a)=>n==node&&costs.TryGetValue(a,out var c)?c:throw new InvalidDataException("Foreign graph span cost lookup.");
                            var result=ordinal<0?startup.ProcessPage(page.Records.Span,checked((ushort)node),span,0,qualifier,headers,Lookup,router,0,sourcePolicy.SuppressHeaderFlag,0,false,false,null,token):
                                ordinary.ProcessPage(page.Records.Span,checked((ushort)node),span,0,(ushort)ordinal,parent.Span,0,qualifier,headers,Lookup,
                                    (n,a)=>OwnedRows(n)[a].Record,router,0,false,sourcePolicy.SuppressHeaderFlag,0,false,false,null,token);
                            if(!result.Succeeded)throw new InvalidDataException("Actual ordinary graph page failed.");
                            totalRows+=page.ReturnedCount;totalScores+=result.ScoringCalls;totalAllocations+=result.AcceptedAllocations;
                            graphLoopSkip=checked(graphLoopSkip+page.ReturnedCount);request.Skip=graphLoopSkip;if(page.ReturnedCount==0)break;
                        }
                    }
                }
            }
            token.ThrowIfCancellationRequested();graphLoopMetrics=new(graph.NodeCount,graph.NodeCount,totalRows,totalQueries,totalScores,totalAllocations,totalSelected);pending=false;completedBacking=null;Volatile.Write(ref phase,2);
            return last??throw new InvalidDataException("No completed graph node.");

            IReadOnlyList<LegacyNativeStoredWordRecord> OwnedRows(int node)
            {
                token.ThrowIfCancellationRequested();if(ownedHistory.TryGetValue(node,out var saved))return saved;int count=ring.SourceEndAt(node);var rows=ring.OwnedRowsAt(node);var alternatives=ring.OwnedPrimaryAt(node);
                if(count is <125 or >156)throw new InvalidDataException("Exact retained SourceEnd required.");
                ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,checked(count*256L));ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,checked(count*176L));
                var result=new LegacyNativeStoredWordRecord[count];
                for(int i=0;i<count;i++)result[i]=new(rows.Slice(i*88,88).ToArray(),i<125?alternatives.Slice(i*80,80).ToArray():[],[]);
                ownedHistory.Add(node,result);return result;
            }
        }
        catch
        {
            if(root is not null)root.FaultNode1(this);pending=false;completedBacking=null;pendingReceipt=null;activeStage=null;
            primary.FaultNode0Completion(this);Volatile.Write(ref phase,3);throw;
        }
        finally{graphLoopSourcePolicy=null;graphLoopGeometry=null;graphLoopActive=false;graphLoopQuery=null;graphLoopLease=null;graphLanguageScope=null;graphControllerValues=null;graphControllerMask=null;graphCompletedNode=null;node1Pass=null;thread=0;}
    }
    internal void RequireGraphLoopSpan(int node,byte span)
    {if(!graphLoopActive||graphLoopNode!=node||graphLoopSpan!=span||Volatile.Read(ref phase)!=1||thread!=Environment.CurrentManagedThreadId)throw new InvalidOperationException("Same active graph loop/node/span required.");}
    private void RequireGraphLoopQuery(LegacyEnglishFreshCaseMainSource.Node1QueryLease lease,LegacyNativeDelimiterQuery query)
    {
        RequireGraphLoopSpan(lease.Node,lease.Span);var q=query.Request;
        if(!ReferenceEquals(graphLoopLease,lease)||!ReferenceEquals(graphLoopQuery,query)||graphLoopParent< -1||
            (graphLoopPrevious==-1?graphLoopParent!=-1:graphLoopParent is <0 or >=125)||q.Parameter1c!=lease.Node||q.Parameter20!=lease.Span||q.Skip!=graphLoopSkip||q.Capacity!=128||q.ContextRoot!=lease.Pass.Initializer.PlanForAudit.BindingForAudit.Contexts.ContainerIdentity)
            throw new InvalidOperationException("Only current privately issued graph query may read.");
    }
    private bool GraphLanguageRequired(GraphLanguageScope scope,int count,CancellationToken token)
    {
        scope.Require(token);var graph=scope.Lease.Pass.Initializer.PlanForAudit.BindingForAudit.Graph;int node=scope.CurrentNode;
        if(node==graph.NodeCount-1)return true;
        int next=node+1,extent=Math.Min(next+1,graph.MaximumAtoms);
        for(int offset=0;offset<extent&&next<graph.NodeCount;offset++,next++)foreach(var arc in graph.GetNode(next))
        {token.ThrowIfCancellationRequested();if(next-arc.Span-1==node&&(arc.Flags&4)!=0)return true;}
        ushort replacement=scope.Lease.Root.Source.Alphabet.TryGetLabel("\ufffd",out var label)?label:throw new InvalidDataException("Same replacement label required.");
        for(int i=0;i<count;i++)
        {
            var row=scope.Ring.CurrentRows.Slice(i*88,88);if(BinaryPrimitives.ReadInt32LittleEndian(row[16..])==0x4000000)break;
            if(row[4]!=1)throw new NotSupportedException("Same declared source1 header required.");
            if(BinaryPrimitives.ReadUInt16LittleEndian(row[36..])==replacement)return true;
            // kindC0E&1E00=C00 lies outside native elapsed kinds200..A00.
        }
        return false;
    }
    private void SealGraphEmptyLanguage(GraphLanguageScope scope,CancellationToken token)
    {
        scope.Require(token);scope.Descriptor.ClearActiveCount();
        if(!scope.Descriptor.Resize(124,0,null,cancellationToken:token)||scope.Descriptor.AllocatedCapacity!=250)throw new InvalidDataException("Same retained phase1 backing required.");
        graphLanguageFirstOnly=true;_=scope.Descriptor.StampGraphPhysicalOrdinals(scope,token);scope.SealCanonicalFirstOnly();
    }
}
