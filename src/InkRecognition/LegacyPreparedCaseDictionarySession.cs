using System.Buffers.Binary;
using System.Runtime.CompilerServices;
namespace InkRecognition;

/// <summary>One selected292 dictionary with shared owned backend for normal/auxiliary
/// queries, permission refresh, terminal metadata and restart. Container90/C4 and
/// source-selection order remain explicit prepared inputs, not inferred defaults.</summary>
public sealed class LegacyPreparedCaseDictionarySession
{
    private readonly LegacyPreparedDictionaryQuerySource dictionary;
    private LegacyPreparedDictionaryWorkingMap installedMap;
    private readonly ConditionalWeakTable<LegacyPreparedDictionaryQuery,LegacyPreparedDictionaryWorkingMap> queryMaps=new();
    private LegacyPreparedDictionaryWorkingMap? queriedMap;
    private long controllerEpoch,preparedMapEpoch;
    private int nextControllerNode=-1,lastOwnedSpan=-1;
    private bool ownedKeyProgression,externalQuerySinceEpoch;
    private (int Node,byte Span)? currentOwnedKey;
    private readonly LegacyTaggedAggregateDictionaryQuerySource queries;
    private readonly LegacyNativeCaseWordMetadata metadata;
    private readonly LegacyPreparedDictionaryRestartTags restart;
    private readonly bool ignoreCaseC4,extraCosts;
    private int active;
    private bool valid=true;
    public LegacyRussianModelBundle Model {get;}
    public LegacyPreparedCaseDictionarySymbolBinding Symbols {get;}
    public LegacyOrdinaryCursiveCandidateGraph Graph {get;}
    public LegacySymbolContextFactory Contexts=>Symbols.Symbols.Contexts;

    public LegacyPreparedCaseDictionarySession(LegacyRussianModelBundle model,
        LegacyDictionaryResourcePrefix selectedResource,uint containerCaseMode90,bool ignoreCaseC4,
        LegacyNativeCaseData caseData,LegacyNativeTextPublicationData publication,
        LegacyPreparedCaseDictionarySymbolBinding symbols,LegacyOrdinaryCursiveCandidateGraph graph,
        LegacyPreparedDictionaryRestartTags restartTags,bool extraCostSideTable)
    {
        ArgumentNullException.ThrowIfNull(model);ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(graph);
        if(!ReferenceEquals(symbols.Symbols.AcousticAlphabet,model.AcousticAlphabet)||
            !ReferenceEquals(symbols.Symbols.LexicalAlphabet,model.LexicalAlphabet)||
            !ReferenceEquals(symbols.Language,model.LanguageModel))
            throw new ArgumentException("Case symbols/language must belong to the selected model, not equal-sized substitutes.");
        Model=model;Symbols=symbols;Graph=graph;restart=restartTags;
        this.ignoreCaseC4=ignoreCaseC4;extraCosts=extraCostSideTable;
        installedMap=symbols.Symbols.CreateWorkingMap(symbols.DescriptorFlags);
        dictionary=new(model.Lexicon,installedMap,symbols.Symbols.CopyInitialContexts());
        var child=LegacyTaggedDictionaryChild.BindCase292(selectedResource,containerCaseMode90,
            model.LanguageModel,model.LexicalAlphabet,caseData,publication,symbols.Preparation,
            Contexts,dictionary,graph,extraCostSideTable);
        queries=new([child]);
        metadata=new(new(model.Lexicon,model.WordIndex),new(symbols.Preparation,
            LegacyNativeCasePathPolicy.Create(model.LanguageModel.CasePathVersion),Contexts));
    }

    public LegacyAggregateDictionaryQueryPage Read(ulong sourceTag,LegacyPreparedDictionaryQuery query,
        bool auxiliary=false,CancellationToken cancellationToken=default)
        =>ReadCore(null,sourceTag,query,auxiliary,cancellationToken);
    internal LegacyAggregateDictionaryQueryPage ReadForMap(LegacyPreparedDictionaryWorkingMap expectedMap,
        ulong sourceTag,LegacyPreparedDictionaryQuery query,bool auxiliary,CancellationToken cancellationToken)
        =>ReadCore(expectedMap,sourceTag,query,auxiliary,cancellationToken);
    private LegacyAggregateDictionaryQueryPage ReadCore(LegacyPreparedDictionaryWorkingMap? expectedMap,
        ulong sourceTag,LegacyPreparedDictionaryQuery query,bool auxiliary,CancellationToken cancellationToken)
    {
        Enter(cancellationToken);
        try
        {
            RequireInstalledMap(expectedMap);
            bool ownedKey=expectedMap is not null&&ownedKeyProgression&&!externalQuerySinceEpoch&&
                currentOwnedKey is{} key&&query.Parameter1c==(uint)key.Node&&query.Parameter20==key.Span;
            if(expectedMap is null)externalQuerySinceEpoch=true;
            else if(!ownedKey)ownedKeyProgression=false;
            if(!ownedKey&&expectedMap is not null&&queriedMap is not null&&!ReferenceEquals(queriedMap,expectedMap))
                throw new InvalidOperationException("The shared backend's eight query slots have crossed a permission map without the native controller node epoch.");
            //The native controller resets query caches separately from source68.
            //This bounded owning-span adapter does not fabricate that epoch.
            //Refuse a persistent request across maps BEFORE backend error handling,
            //so another valid span can still use this shared session.
            if(queryMaps.TryGetValue(query,out var previousMap))
            {if(!ownedKey&&expectedMap is not null&&!ReferenceEquals(previousMap,expectedMap))throw new InvalidOperationException("Case query reuse across permission maps requires its proven source key progression or native controller query epoch.");}
            else queryMaps.Add(query,installedMap);
            queriedMap??=installedMap;
            try{return queries.Read(sourceTag,query,auxiliary,extraCosts,cancellationToken);}
            catch{valid=false;throw;}
        }
        finally{Exit();}
    }

    /// <summary>Prepared source V80:236DC0 forwards selected292, whose232450
    /// calls inner2B7.V80/092970→092840 then231ED0.2144F0 invokes this before
    /// the loop with start−1 and after each completed node. This is distinct
    /// from source68 permission refresh; no per-span scheduling is invented.</summary>
    public void PrepareControllerNode(int completedNode,CancellationToken cancellationToken=default)
    {
        Enter(cancellationToken);
        try
        {
            if(completedNode< -1||completedNode>=Graph.NodeCount)throw new ArgumentOutOfRangeException(nameof(completedNode));
            queries.PrepareControllerNode(cancellationToken);
            queryMaps.Clear();queriedMap=null;controllerEpoch=checked(controllerEpoch+1);
            nextControllerNode=completedNode+1;lastOwnedSpan=-1;
            ownedKeyProgression=true;externalQuerySinceEpoch=false;currentOwnedKey=null;
        }
        finally{Exit();}
    }

    /// <summary>234840 then093F80 on the SAME backend used by both query paths.
    /// Native query invalidation is a separate controller epoch; no automatic
    /// cache reset is invented here.</summary>
    public void RefreshPermissions(ReadOnlySpan<byte> acousticPermissions,CancellationToken cancellationToken=default)
        =>_=RefreshPermissionsAndGetMap(acousticPermissions,cancellationToken);

    //Common span bridge needs the SAME case-expanded map installed in this
    //backend, not a second propagation or a stale pre-wrapper map snapshot.
    internal LegacyPreparedDictionaryWorkingMap RefreshPermissionsAndGetMap(ReadOnlySpan<byte> acousticPermissions,
        CancellationToken cancellationToken=default)
        =>RefreshMapCore(acousticPermissions,null,cancellationToken);

    //Only the actual graph/cache D0 owner supplies this key. It is not a caller
    //success flag or a reset request: native1FD240/201270 write these exact1C/20.
    internal LegacyPreparedDictionaryWorkingMap RefreshOwnedSpanAndGetMap(ReadOnlySpan<byte> acousticPermissions,
        LegacyOrdinaryCursiveCandidateGraph graph,int node,byte span,CancellationToken cancellationToken)
    {
        if(!ReferenceEquals(graph,Graph))throw new ArgumentException("Span key requires this session's actual graph owner.");
        if((uint)node>=Graph.NodeCount||span>node||!Graph.GetNode(node).Any(arc=>arc.Span>=span))
            throw new ArgumentException("An actual graph span key is required.");
        return RefreshMapCore(acousticPermissions,(node,span),cancellationToken);
    }

    private LegacyPreparedDictionaryWorkingMap RefreshMapCore(ReadOnlySpan<byte> acousticPermissions,
        (int Node,byte Span)? ownedKey,CancellationToken cancellationToken)
    {
        Enter(cancellationToken);
        try
        {
            var map=Symbols.Refresh(acousticPermissions,cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();InstallMap(map);
            if(ownedKey is{} key&&ownedKeyProgression&&!externalQuerySinceEpoch&&
                key.Node==nextControllerNode&&key.Span>lastOwnedSpan)
            {currentOwnedKey=key;lastOwnedSpan=key.Span;}
            else{currentOwnedKey=null;ownedKeyProgression=false;}
            return map;
        }
        finally{Exit();}
    }

    public LegacyPreparedAcousticPermissionResult BuildAndRefreshPermissions(
        IReadOnlyList<LegacyPreparedAcousticPermissionGroup> actualGroups,CancellationToken cancellationToken=default)
    {
        Enter(cancellationToken);
        try
        {
            var produced=Symbols.BuildAndRefresh(actualGroups,cancellationToken);
            if(produced.Succeeded){cancellationToken.ThrowIfCancellationRequested();InstallMap(produced.WorkingMap!);currentOwnedKey=null;ownedKeyProgression=false;}
            return produced;
        }
        finally{Exit();}
    }

    /// <summary>236B50 removes selected-source byte4 before233F70, which copies
    /// case state only for incoming header05.bit2; aggregate stamps source1
    /// only when the returned tag's header05.bit2 is set.</summary>
    public ulong ResolveRestart(ulong selectedSourceTag,CancellationToken cancellationToken=default)
        =>ResolveRestartCore(null,selectedSourceTag,cancellationToken);
    internal ulong ResolveRestartForMap(LegacyPreparedDictionaryWorkingMap expectedMap,ulong selectedSourceTag,CancellationToken cancellationToken)
        =>ResolveRestartCore(expectedMap,selectedSourceTag,cancellationToken);
    private ulong ResolveRestartCore(LegacyPreparedDictionaryWorkingMap? expectedMap,ulong selectedSourceTag,CancellationToken cancellationToken)
    {
        Enter(cancellationToken);
        try
        {
            RequireInstalledMap(expectedMap);
            RequireSelectedSource(selectedSourceTag);
            ulong innerRequest=selectedSourceTag&~(255UL<<32);
            ulong result=LegacyNativeCasePathPolicy.ApplyRestart(restart.Resolve(innerRequest),innerRequest);
            return (result&(4UL<<40))!=0?(result&~(255UL<<32))|(1UL<<32):result;
        }
        finally{Exit();}
    }

    /// <summary>236D20 selects child by source byte4, passes the complete tag
    /// to234970, then publishes selected-source ID at outputU32+0. Inner word
    /// identity/category/boundary channels remain separate.</summary>
    public ReadOnlyMemory<byte> ReadTerminal(ulong selectedSourceTag,ushort endingSymbol,
        byte boundaryArgument,CancellationToken cancellationToken=default)
        =>ReadTerminalCore(null,selectedSourceTag,endingSymbol,boundaryArgument,cancellationToken);
    internal ReadOnlyMemory<byte> ReadTerminalForMap(LegacyPreparedDictionaryWorkingMap expectedMap,ulong selectedSourceTag,
        ushort endingSymbol,byte boundaryArgument,CancellationToken cancellationToken)
        =>ReadTerminalCore(expectedMap,selectedSourceTag,endingSymbol,boundaryArgument,cancellationToken);
    private ReadOnlyMemory<byte> ReadTerminalCore(LegacyPreparedDictionaryWorkingMap? expectedMap,ulong selectedSourceTag,
        ushort endingSymbol,byte boundaryArgument,CancellationToken cancellationToken)
    {
        Enter(cancellationToken);
        try
        {
            RequireInstalledMap(expectedMap);
            RequireSelectedSource(selectedSourceTag);
            byte[] result=metadata.ReadTerminal(selectedSourceTag,endingSymbol,boundaryArgument,
                ignoreCaseC4,cancellationToken).ToArray();
            BinaryPrimitives.WriteUInt32LittleEndian(result,1);
            return result;
        }
        finally{Exit();}
    }

    private static void RequireSelectedSource(ulong tag)
    {if((byte)(tag>>32)!=1)throw new ArgumentException("This session owns exactly selected source1; implicit0/other source IDs are not interchangeable.");}
    internal bool IsInstalledMap(LegacyPreparedDictionaryWorkingMap expectedMap)
        =>ReferenceEquals(Volatile.Read(ref installedMap),expectedMap)&&Volatile.Read(ref preparedMapEpoch)==Volatile.Read(ref controllerEpoch);
    private void RequireInstalledMap(LegacyPreparedDictionaryWorkingMap? expectedMap)
    {if(expectedMap is not null&&(!ReferenceEquals(installedMap,expectedMap)||preparedMapEpoch!=controllerEpoch))throw new InvalidOperationException("Selected source map was refreshed by another owning span/session call or requires preparation after its controller node epoch.");}
    private void InstallMap(LegacyPreparedDictionaryWorkingMap map)
    {dictionary.SetWorkingMap(map);Volatile.Write(ref installedMap,map);Volatile.Write(ref preparedMapEpoch,controllerEpoch);}
    private void Enter(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(Interlocked.CompareExchange(ref active,1,0)!=0)throw new InvalidOperationException("Selected source operations must be serialized.");
        if(!valid){Exit();throw new InvalidOperationException("Failed query requires a new selected-source session.");}
    }
    private void Exit()=>Volatile.Write(ref active,0);
}
