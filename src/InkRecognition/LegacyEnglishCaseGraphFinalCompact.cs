using System.Buffers.Binary;
namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private bool graphFinalCompactRequested,graphFinalCompactBusy;
    private GraphCompactData? graphCompactData;
    private List<GraphCompactScoreData>? graphCompactScores;
    internal sealed record GraphCompactScoreData(int Group,int Span,int PowerScaleBits,ushort[] Symbols,int[] Costs,int[] WeightBits);
    internal readonly record struct GraphCompactArc(int Group,byte Span,uint Start,uint End);
    internal sealed record GraphCompactNodeData(int SourceEnd,int RetainedEnd,byte[] Rows,byte[] RowMasks,byte[] Primary,byte[] PrimaryMasks);
    internal sealed record GraphCompactData(int Nodes,int Records,byte[] Storage,byte[] Masks,int[] Starts,
        int[] PrefixOwners,LegacyPreparedMainTerminalWinner Winner,byte[] SelectedRow,
        GraphCompactNodeData[] InputNodes,GraphCompactNodeData[] OutputNodes,GraphCompactArc[][] Graph,
        LegacySymbolContextNode[] Contexts,GraphCompactScoreData[] Scores,ushort[] FinalWorkingMap);

    // The old graph-only entry remains unchanged. This explicit opt-in producer
    // runs207CB0 and2257 while the actual last language lease is still active.
    internal GraphCompactData RunGraphFirstFinalCompact(CancellationToken token=default)
    {
        _=RunGraphFirstLoopCore(true,token);
        if(Volatile.Read(ref phase)!=2||graphCompactData is null)throw new InvalidOperationException("No completed same-owner compact result.");
        var r=graphCompactData;
        return r with{Storage=r.Storage.ToArray(),Masks=r.Masks.ToArray(),Starts=r.Starts.ToArray(),PrefixOwners=r.PrefixOwners.ToArray(),SelectedRow=r.SelectedRow.ToArray(),
            InputNodes=CopyNodes(r.InputNodes),OutputNodes=CopyNodes(r.OutputNodes),Graph=r.Graph.Select(n=>n.ToArray()).ToArray(),
            Contexts=r.Contexts.ToArray(),Scores=r.Scores.Select(s=>s with{Symbols=s.Symbols.ToArray(),Costs=s.Costs.ToArray(),WeightBits=s.WeightBits.ToArray()}).ToArray(),FinalWorkingMap=r.FinalWorkingMap.ToArray()};
    }
    private void RunGraphFinalCompact(GraphLanguageScope stage,CancellationToken token)
    {
        stage.RequireCompactPhase();stage.RequireFirstOnlyPattern();token.ThrowIfCancellationRequested();
        if(!graphFinalCompactRequested||graphFinalCompactBusy||graphCompactData is not null||
            stage.CurrentNode!=stage.Ring.ActualNodeCount-1)throw new InvalidOperationException("One actual LAST-node publication required.");
        var root=stage.Lease.Root;var pass=stage.Lease.Pass;
        var graph=pass.Initializer.PlanForAudit.BindingForAudit.Graph;
        var contexts=pass.Initializer.PlanForAudit.BindingForAudit.Contexts;
        var policy=graphLoopSourcePolicy??throw new InvalidOperationException("Same live source policy required.");
        var geometry=graphLoopGeometry??throw new InvalidOperationException("Same live group geometry required.");
        var c=policy.Heaps.Configuration;
        if(!ReferenceEquals(policy.Knowledge,root.Knowledge)||!ReferenceEquals(policy.Numeric,root.NumericOwner)||
            root.DescriptorFlags!=0xc0e||root.NumericOwner.Profile!=2||root.NumericOwner.SourceScale!=1||
            c.HeaderCount2400!=2||c.PrimaryAlternativeCount2402!=2||c.SecondaryAlternativeCount2404!=1||
            c.PrimaryArcLimit230a!=125||c.Mode22f6!=0||c.Trace2368!=0||policy.Heaps.SecondaryShareEc!=0||
            root.Source.Alphabet.TryGetLabel("\U000f0009",out _))
            throw new NotSupportedException("Actual source135 fresh ordinary2/2/1 no-sentinel compact policy required.");
        var budget=PrepareGraphCompactBudget(stage,graph,contexts,token);
        int slots=budget.RecordCapacity;
        stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,budget.AllocationBytes);
        stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,budget.CopyBytes);
        var storage=new byte[checked(slots*40)];var starts=new int[graph.NodeCount+1];var prefix=new int[starts.Length];
        var owned=new LegacyMainDirectWordNodeResult[graph.NodeCount];
        var inputNodes=new GraphCompactNodeData[graph.NodeCount];
        var graphData=new GraphCompactArc[graph.NodeCount][];
        graphCompactScores=new(graph.GroupCount);
        for(int node=0;node<owned.Length;node++)
        {
            token.ThrowIfCancellationRequested();int end=stage.Ring.SourceEndAt(node),retained=stage.Ring.RetainedEndAt(node);
            if(end is <125 or >156||retained<125||retained>end)throw new InvalidDataException("Complete live source extent required.");
            graphData[node]=graph.GetNode(node).Select(a=>new GraphCompactArc(a.GroupId,a.Span,a.StartContextId,a.EndContextId)).ToArray();
            inputNodes[node]=new(end,retained,stage.Ring.OwnedRowsAt(node).ToArray(),stage.Ring.OwnedRowMaskAt(node).ToArray(),
                stage.Ring.OwnedPrimaryAt(node).ToArray(),stage.Ring.OwnedPrimaryMaskAt(node).ToArray());
            owned[node]=new(node,0,0,0,0,true,0,0,(ushort)end,(ushort)retained,
                stage.Ring.OwnedRowsAt(node).ToArray(),stage.Ring.OwnedPrimaryAt(node).ToArray());
        }
        graphFinalCompactBusy=true;
        try
        {
            pass.Initializer.PlanForAudit.WithGraphPublicationReader(stage,reader=>
            {
                void Validate(CancellationToken ct)
                {stage.RequireCompactPhase();ct.ThrowIfCancellationRequested();if(!graphFinalCompactBusy||!graphFinalCompactRequested)throw new InvalidOperationException("Same private publication cohort required.");}
                // One request is constructed inside this actual shared reachability invocation.
                stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,8);stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,8);
                var reached=LegacyPreparedMainCompactReachability.Run(c,graph.MaximumAtoms,0,stage.CurrentNode,
                    n=>owned[n],reader,contexts.ContainerIdentity,Validate,false,token,5);
                var published=LegacyPreparedMainCompactPublication.Run(c,graph,contexts,root.Source.Alphabet,geometry.Categories,
                    false,root.NumericOwner.FinalUnigram94D,false,0,stage.CurrentNode,reached.Nodes,reached.LanguageProbe,
                    group=>LegacyMainCompactGroupScores.FromEnglishGraph(stage,group,token),storage,0,starts,prefix,Validate,token,[0,root.DescriptorFlags]);
                Validate(token);
                for(int node=0;node<published.Nodes.Length;node++)
                {
                    var row=published.Nodes[node];var target=stage.Ring.OwnedRowsAt(node);var primary=stage.Ring.OwnedPrimaryAt(node);
                    if(row.Records.Length!=target.Length||row.PrimaryAlternatives.Length!=primary.Length)throw new InvalidDataException("Publication changed live ring extent.");
                    stage.Ledger.CopyBytes(LegacyEnglishCaseNode0Ledger.Site.FinalImages,row.Records.Span,target);
                    stage.Ledger.CopyBytes(LegacyEnglishCaseNode0Ledger.Site.FinalImages,row.PrimaryAlternatives.Span,primary);
                    // Bit flags retain opaque payload. Only the actually written
                    // compact ordinal becomes fully known; unknown language bytes stay unknown.
                    for(int i=0;i<row.SourceEnd23f0;i++)if((target[i*88+6]&2)!=0)
                    {stage.Ring.OwnedRowMaskAt(node)[i*88+34]=255;stage.Ring.OwnedRowMaskAt(node)[i*88+35]=255;}
                }
                var final=stage.Ring.CurrentRows;
                LegacyPreparedTerminalSource[] sources=[new(0,0),new(root.DescriptorFlags,root.NumericOwner.FinalUnigram94D)];
                var winner=LegacyNativeMainTerminalSelection.SelectSingleAlternativeWithoutRescue(final,
                    checked((ushort)stage.Ring.SourceEndAt(stage.CurrentNode)),sources,new(125,false,false,false,false,0),0,contexts,token);
                var mask=new byte[checked(published.Count*40)];
                for(int i=0;i<mask.Length;i++)mask[i]=i%40 is 34 or 35?(byte)0:(byte)255;
                if(graphConfidenceScorer is not null)graphRawWord=BuildGraphRawWord(stage,published,winner,token);
                int contextCount=contexts.NodeCount;
                if(contextCount>16384)throw new NotSupportedException("Explicit detached compact context-export bound exceeded.");
                stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(contextCount*192L));
                stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(contextCount*192L));
                var contextData=contexts.CopyNodes().ToArray();
                var outputs=new GraphCompactNodeData[graph.NodeCount];
                for(int node=0;node<outputs.Length;node++)outputs[node]=new(stage.Ring.SourceEndAt(node),stage.Ring.RetainedEndAt(node),
                    stage.Ring.OwnedRowsAt(node).ToArray(),stage.Ring.OwnedRowMaskAt(node).ToArray(),stage.Ring.OwnedPrimaryAt(node).ToArray(),stage.Ring.OwnedPrimaryMaskAt(node).ToArray());
                stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,4096);
                stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,4096);
                var working=root.CopyWorkingMap();
                graphCompactData=new(graph.NodeCount,published.Count,published.Storage.AsSpan(0,published.Count*40).ToArray(),
                    mask,published.Starts,published.PrefixOwners,winner,winner.HasWinner?final.Slice(winner.Ordinal*88,88).ToArray():[],
                    inputNodes,outputs,graphData,contextData,graphCompactScores!.ToArray(),working);
                Validate(token);
            },token);
        }
        finally{graphFinalCompactBusy=false;graphCompactScores=null;}
    }
    private static GraphCompactNodeData[] CopyNodes(GraphCompactNodeData[] input)=>input.Select(n=>n with
    {Rows=n.Rows.ToArray(),RowMasks=n.RowMasks.ToArray(),Primary=n.Primary.ToArray(),PrimaryMasks=n.PrimaryMasks.ToArray()}).ToArray();
    internal void ObserveGraphCompactScore(GraphLanguageScope stage,int group,int span,float scale,ushort[] symbols,int[] costs,float[] weights)
    {
        RequireGraphPublication(stage,default);
        if(graphPeriodicActive)return; // Cached weight owner is retained; only detached score diagnostics are suppressed.
        if(graphCompactScores is null||symbols.Length>150||symbols.Length!=costs.Length||costs.Length!=weights.Length||
            graphCompactScores.Count>=4096||graphCompactScores.Any(x=>x.Group==group))throw new InvalidDataException("One bounded actual cached group export required.");
        graphCompactScores.Add(new(group,span,BitConverter.SingleToInt32Bits(scale),symbols.ToArray(),costs.ToArray(),weights.Select(BitConverter.SingleToInt32Bits).ToArray()));
    }
    internal void RequireGraphPublication(GraphLanguageScope stage,CancellationToken token)
    {stage.RequireCompactPhase();token.ThrowIfCancellationRequested();if(!graphFinalCompactRequested||!graphFinalCompactBusy||!ReferenceEquals(graphLanguageScope,stage))throw new InvalidOperationException("Exact active LAST publication required.");}
}

internal sealed partial class LegacyEnglishCaseInitializationPlan
{
    internal void WithGraphPublicationReader(LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,
        Action<LegacyNativeDelimiterQuerySource> consume,CancellationToken token)
    {
        stage.Owner.RequireGraphPublication(stage,token);RequirePublished(token);
        if(executionPool is null||Volatile.Read(ref executionReset)!=1||!ReferenceEquals(stage.Lease.Root,BoundRoot)||normal is null)
            throw new InvalidOperationException("Same pre-V80 actual normal reader required.");
        consume(normal);stage.Owner.RequireGraphPublication(stage,token);
    }
}
