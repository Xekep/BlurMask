namespace InkRecognition;

/// <summary>An actual global D0 snapshot. Its owner/epoch and installed292 map
/// are required by every query; it does not represent a decoded word.</summary>
public sealed class LegacyOwnedGlobalMainWordSpan
{
    internal readonly LegacyFreshMainGlobalCaseSession Owner;
    internal readonly long Epoch;
    internal readonly LegacyPreparedDictionaryWorkingMap Map;
    internal readonly LegacyNativeWordSymbolQualifier Qualifier;
    public int EndingNode { get; }
    public byte Span { get; }
    public int ComputedRows { get; }
    public int ReusedRows { get; }
    private readonly byte[] permissions;
    internal LegacyOwnedGlobalMainWordSpan(LegacyFreshMainGlobalCaseSession owner,long epoch,int node,byte span,
        LegacyPreparedDictionaryWorkingMap map,LegacyNativeWordSymbolPreparationResult result)
    {
        Owner=owner;Epoch=epoch;EndingNode=node;Span=span;Map=map;Qualifier=result.Qualifier!;
        ComputedRows=result.PermissionPass.ComputedRows;ReusedRows=result.PermissionPass.ReusedRows;
        permissions=result.PermissionPass.PermissionResult.Permissions.ToArray();
    }
    public byte[] CopyPermissions()=>permissions.ToArray();
    public ushort[] CopyWorkingMap()=>Map.CopySelectedMap(false,0);
    public byte[] CopyEligibility()=>Qualifier.GetEligibilitySnapshot().ToArray();
}

/// <summary>Actual global ordinary cache/source68/292 query lifecycle over one
/// owned symbol/context/backend. Node epochs remain explicit controller events.
/// This source adapter does not supply word geometry, main loop or final output.</summary>
public sealed class LegacyFreshMainGlobalCaseSession
{
    private readonly LegacyOrdinaryRecognitionGraph graph;
    private readonly LegacyPreparedCaseDictionarySession dictionary;
    private readonly LegacyNativeWordSymbolPreparationSession cache;
    private readonly Dictionary<int,int[]> costs=[];
    private int active,lastCompleted=-2,nextSpan;
    private long epoch;
    private bool valid=true;
    public LegacyFreshMainGlobalAcousticScorer Acoustic { get; }
    public LegacyFreshMainGlobalCaseSymbols Symbols=>Acoustic.Owner;
    public LegacyFreshMainCompactOrdinaryGraph Graph=>Symbols.Geometry.Graph;
    public LegacyRussianModelBundle Model=>Symbols.Model;
    public LegacySymbolContextFactory Contexts=>Symbols.Contexts;
    public int AcousticEvaluations { get; private set; }
    private LegacyFreshMainGlobalCaseSession(LegacyFreshMainGlobalAcousticScorer acoustic,
        LegacyPreparedCaseDictionarySession dictionary,LegacyOrdinaryRecognitionGraph graph,
        ReadOnlySpan<ushort> reserved,CancellationToken token)
    {
        Acoustic=acoustic;this.dictionary=dictionary;this.graph=graph;
        var rawMap=Symbols.Symbols.Symbols.CreateWorkingMap(Symbols.Descriptor.Flags);
        cache=new(graph,rawMap,reserved,Compute,cancellationToken:token);
    }
    public static LegacyFreshMainGlobalCaseSession Prepare(ReadOnlySpan<byte> acousticResource,
        ReadOnlySpan<byte> lexicalResource,LegacyRussianModelBundle model,LegacyFreshMainGlobalGlyphGeometry geometry,
        LegacyNativeCaseData caseData,LegacyNativeTextPublicationData publication,CancellationToken token=default)
    {
        var symbols=LegacyFreshMainGlobalCaseSymbols.Prepare(acousticResource,lexicalResource,model,geometry,caseData,publication,token);
        var scorer=LegacyFreshMainGlobalAcousticScorer.Prepare(acousticResource,symbols,token);
        var graph=LegacyOrdinaryRecognitionGraph.FromGlobal(geometry.Graph);
        var dictionary=new LegacyPreparedCaseDictionarySession(model,symbols.ResourcePrefix,uint.MaxValue,false,
            caseData,publication,symbols.Symbols,graph,LegacyPreparedDictionaryRestartTags.CreateFresh(),false);
        var reserved=LegacyPreparedAcousticDictionaryPermissions.ReadReservedSlots(model.AcousticAlphabet,acousticResource.Slice(99,16));
        token.ThrowIfCancellationRequested();return new(scorer,dictionary,graph,reserved,token);
    }
    public int GetSpanExtent(int node)
    {
        int extent=0;foreach(var arc in graph.GetNode(node))extent=Math.Max(extent,arc.Span+1);return extent;
    }
    /// <summary>Explicit V80 before the loop(-1) and after each completed node.
    /// It is never inferred from a permission refresh or a query.</summary>
    public void AdvanceNodeEpoch(int completedNode,CancellationToken token=default)
    {
        Enter(token);
        try
        {
            if(completedNode!=lastCompleted+1||completedNode>=graph.NodeCount||
                completedNode>=0&&nextSpan!=GetSpanExtent(completedNode))
                throw new ArgumentException("Ordered controller V80 requires all actual D0 spans of the completed node.");
            dictionary.PrepareControllerNode(completedNode,token);lastCompleted=completedNode;nextSpan=0;epoch=checked(epoch+1);
        }
        catch{valid=false;throw;}finally{Exit();}
    }
    /// <summary>One actual ascending-span D0: compute/reuse final cache rows,
    /// produce acoustic permissions, then234840→093F80 ONCE on the shared292 backend.</summary>
    public LegacyOwnedGlobalMainWordSpan PrepareSpan(int node,byte span,CancellationToken token=default)
    {
        Enter(token);
        try
        {
            if(lastCompleted!=node-1||node<0||node>=graph.NodeCount||span!=nextSpan||span>=GetSpanExtent(node)||span>node)
                throw new ArgumentException("Actual ascending controller node/span schedule required.");
            var result=cache.Prepare(node,span,false,token);
            if(!result.PermissionPass.Succeeded||result.Qualifier is null)
                throw new InvalidOperationException("Global ordinary acoustic preparation failed.");
            var map=dictionary.RefreshOwnedSpanAndGetMap(result.PermissionPass.PermissionResult.Permissions.Span,graph,node,span,token);
            token.ThrowIfCancellationRequested();nextSpan++;
            return new(this,checked(++epoch),node,span,map,result);
        }
        catch{valid=false;throw;}finally{Exit();}
    }
    public int[] CopyClassCosts(LegacyOwnedGlobalMainWordSpan span,int node,ushort ordinal,CancellationToken token=default)
    {
        Enter(token);try
        {
            Check(span,token);var arcs=graph.GetNode(node);
            if(node!=span.EndingNode||ordinal>=arcs.Count||arcs[ordinal].Span!=span.Span)
                throw new ArgumentException("Actual selected global span/arc required.");
            return costs[arcs[ordinal].GroupId].ToArray();
        }
        finally{Exit();}
    }
    public LegacyAggregateDictionaryQueryPage Read(LegacyOwnedGlobalMainWordSpan span,ulong sourceTag,
        LegacyPreparedDictionaryQuery query,bool auxiliary=false,CancellationToken token=default)
    {
        Enter(token);try
        {
            Check(span,token);ArgumentNullException.ThrowIfNull(query);
            if(query.ContextRoot!=Contexts.ContainerIdentity)throw new ArgumentException("Query requires this source's actual context container.");
            if(query.ParentContext!=0)_=Contexts.ReadNode(query.ParentContext);
            return dictionary.ReadForMap(span.Map,sourceTag,query,auxiliary,token);
        }
        finally{Exit();}
    }
    public ReadOnlyMemory<byte> ReadWordTerminal(LegacyOwnedGlobalMainWordSpan span,ulong sourceTag,ushort endingSymbol,
        byte boundaryArgument,CancellationToken token=default)
    {
        Enter(token);try{Check(span,token);return dictionary.ReadTerminalForMap(span.Map,sourceTag,endingSymbol,boundaryArgument,token);}
        finally{Exit();}
    }
    public ulong ResolveWordRestart(LegacyOwnedGlobalMainWordSpan span,ulong sourceTag,CancellationToken token=default)
    {
        Enter(token);try{Check(span,token);return dictionary.ResolveRestartForMap(span.Map,sourceTag,token);}
        finally{Exit();}
    }
    public bool Qualifies(LegacyOwnedGlobalMainWordSpan span,ushort symbol,byte queryLength,ulong context,CancellationToken token=default)
    {
        Enter(token);try{Check(span,token);return span.Qualifier.Qualifies(symbol,queryLength,context,Contexts,token);}
        finally{Exit();}
    }
    private LegacyOrdinaryGraphAcousticRow Compute(int group,CancellationToken token)
    {
        //Only private owned copies survive the call. The public scorer result's
        //recoverable ReadOnlyMemory buffers are never retained as cache storage.
        var result=Acoustic.Score(group,token);costs[group]=result.Cache.Costs.ToArray();AcousticEvaluations++;
        return new(costs[group],Acoustic.CostScale);
    }
    internal void ValidateOwnedSpan(LegacyOwnedGlobalMainWordSpan span,CancellationToken token)
    {Enter(token);try{Check(span,token);}finally{Exit();}}
    private void Check(LegacyOwnedGlobalMainWordSpan span,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(span);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(span.Owner,this)||span.Epoch!=epoch||!dictionary.IsInstalledMap(span.Map))
            throw new InvalidOperationException("Global span belongs to a stale or foreign D0/map epoch.");
    }
    private void Enter(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(Interlocked.CompareExchange(ref active,1,0)!=0)throw new InvalidOperationException("Global case source calls must be serialized.");
        if(!valid){Exit();throw new InvalidOperationException("Failed global source preparation requires a fresh owner.");}
    }
    private void Exit()=>Volatile.Write(ref active,0);
}
