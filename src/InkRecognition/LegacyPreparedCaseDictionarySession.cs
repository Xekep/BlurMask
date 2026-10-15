using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>One selected292 dictionary with shared owned backend for normal/auxiliary
/// queries, permission refresh, terminal metadata and restart. Container90/C4 and
/// source-selection order remain explicit prepared inputs, not inferred defaults.</summary>
public sealed class LegacyPreparedCaseDictionarySession
{
    private readonly LegacyPreparedDictionaryQuerySource dictionary;
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
        dictionary=symbols.Symbols.CreateQuerySource(model.Lexicon,symbols.DescriptorFlags);
        var child=LegacyTaggedDictionaryChild.BindCase292(selectedResource,containerCaseMode90,
            model.LanguageModel,model.LexicalAlphabet,caseData,publication,symbols.Preparation,
            Contexts,dictionary,graph,extraCostSideTable);
        queries=new([child]);
        metadata=new(new(model.Lexicon,model.WordIndex),new(symbols.Preparation,
            LegacyNativeCasePathPolicy.Create(model.LanguageModel.CasePathVersion),Contexts));
    }

    public LegacyAggregateDictionaryQueryPage Read(ulong sourceTag,LegacyPreparedDictionaryQuery query,
        bool auxiliary=false,CancellationToken cancellationToken=default)
    {
        Enter(cancellationToken);
        try{return queries.Read(sourceTag,query,auxiliary,extraCosts,cancellationToken);}
        catch{valid=false;throw;}
        finally{Exit();}
    }

    /// <summary>234840 then093F80 on the SAME backend used by both query paths.
    /// Native query invalidation is a separate controller epoch; no automatic
    /// cache reset is invented here.</summary>
    public void RefreshPermissions(ReadOnlySpan<byte> acousticPermissions,CancellationToken cancellationToken=default)
    {
        Enter(cancellationToken);
        try{dictionary.SetWorkingMap(Symbols.Refresh(acousticPermissions,cancellationToken));}
        finally{Exit();}
    }

    public LegacyPreparedAcousticPermissionResult BuildAndRefreshPermissions(
        IReadOnlyList<LegacyPreparedAcousticPermissionGroup> actualGroups,CancellationToken cancellationToken=default)
    {
        Enter(cancellationToken);
        try
        {
            var produced=Symbols.BuildAndRefresh(actualGroups,cancellationToken);
            if(produced.Succeeded)dictionary.SetWorkingMap(produced.WorkingMap!);
            return produced;
        }
        finally{Exit();}
    }

    /// <summary>236B50 removes selected-source byte4 before233F70, which copies
    /// case state only for incoming header05.bit2; aggregate stamps source1
    /// only when the returned tag's header05.bit2 is set.</summary>
    public ulong ResolveRestart(ulong selectedSourceTag,CancellationToken cancellationToken=default)
    {
        Enter(cancellationToken);
        try
        {
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
    {
        Enter(cancellationToken);
        try
        {
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
    private void Enter(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(Interlocked.CompareExchange(ref active,1,0)!=0)throw new InvalidOperationException("Selected source operations must be serialized.");
        if(!valid){Exit();throw new InvalidOperationException("Failed query requires a new selected-source session.");}
    }
    private void Exit()=>Volatile.Write(ref active,0);
}
