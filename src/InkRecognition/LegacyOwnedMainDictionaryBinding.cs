namespace InkRecognition;

/// <summary>Common owned span services without substituting a DIRECT backend
/// for a case-aware source. The legacy DIRECT public surface stays available
/// only for the original branch.</summary>
internal sealed class LegacyOwnedMainDictionaryBinding
{
    private readonly LegacyPreparedDictionaryQuerySource? directBackend;
    private readonly LegacyPreparedAggregateDictionaryQuerySource? directQueries;
    private LegacyPreparedDictionaryWorkingMap? installedCaseMap;
    public LegacyOwnedRussianDictionaryPreparation? Direct {get;}
    public LegacyFreshCaseRussianDictionaryPreparation? Case {get;}
    public LegacyRussianModelBundle Model=>Direct?.Model??Case!.Model;
    public LegacySymbolContextFactory Contexts=>Direct?.Contexts??Case!.Contexts;
    public LegacyDictionaryInitialSymbolMaps Symbols=>Direct?.Symbols??Case!.Symbols.Symbols;
    public LegacyPreparedDictionarySourceDescriptor Descriptor=>Direct?.Descriptor??Case!.Descriptor;
    public LegacyPreparedLinguisticSourceSummary SourceSummary=>Direct?.SourceSummary??Case!.SourceSummary;
    public uint[] CopySourceHeaders()=>Direct?.CopySourceHeaders()??Case!.CopySourceHeaders();

    public LegacyOwnedMainDictionaryBinding(LegacyOwnedRussianDictionaryPreparation direct,
        LegacyOrdinaryCursiveCandidateGraph graph)
    {
        Direct=direct;var map=Symbols.CreateWorkingMap(Descriptor.Flags);
        directBackend=new(Model.Lexicon,map,Symbols.CopyInitialContexts());
        directQueries=new([LegacyOrdinaryDictionarySourceOwner.BindDictionary(directBackend,Contexts,graph)]);
    }
    public LegacyOwnedMainDictionaryBinding(LegacyFreshCaseRussianDictionaryPreparation owned,
        LegacyOrdinaryCursiveCandidateGraph graph)
    {ArgumentNullException.ThrowIfNull(owned);if(!ReferenceEquals(owned.Graph,graph))throw new ArgumentException("Case dictionary and span graph must be the same owner.");Case=owned;}
    public LegacyPreparedDictionaryWorkingMap CreateRawPermissionMap()=>Symbols.CreateWorkingMap(Descriptor.Flags);
    public void AdvanceNodeEpoch(int completedNode,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(Case is{} owned){owned.Session.PrepareControllerNode(completedNode,token);installedCaseMap=null;}
        else directBackend!.ResetQueries();
    }
    public LegacyPreparedDictionaryWorkingMap Refresh(LegacyPreparedAcousticPermissionResult produced,
        LegacyOrdinaryCursiveCandidateGraph graph,int node,byte span,CancellationToken token)
    {
        if(!produced.Succeeded||produced.WorkingMap is null)throw new InvalidDataException("Failed acoustic permissions cannot refresh selected sources.");
        if(Case is{} owned)return installedCaseMap=owned.Session.RefreshOwnedSpanAndGetMap(produced.Permissions.Span,graph,node,span,token);
        directBackend!.SetWorkingMap(produced.WorkingMap);return produced.WorkingMap;
    }
    public LegacyAggregateDictionaryQueryPage Read(ulong tag,LegacyPreparedDictionaryQuery query,
        bool auxiliary,bool extraCosts,CancellationToken token)
    {
        if(Case is{} owned)
        {
            if(extraCosts!=owned.CaptureSideCosts2368)throw new ArgumentException("Fresh case source has native capture2368=0.");
            return owned.Session.ReadForMap(installedCaseMap??throw new InvalidOperationException("Case span map is not prepared."),tag,query,auxiliary,token);
        }
        return directQueries!.Read(tag,query,auxiliary,extraCosts,token);
    }
    public ReadOnlyMemory<byte> ReadTerminal(ulong tag,ushort symbol,byte boundary,CancellationToken token)
    {
        if(Case is{} owned)return owned.Session.ReadTerminalForMap(installedCaseMap??throw new InvalidOperationException("Case span map is not prepared."),tag,symbol,boundary,token);
        if((byte)(tag>>32)!=1)throw new ArgumentException("Owned selected source1 terminal required.");
        byte[] result=Direct!.WordMetadata.ReadTerminal(tag,boundary,token).ToArray();
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(result,1);return result;
    }
    public ulong ResolveRestart(ulong tag,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();return Case is{} owned?owned.Session.ResolveRestartForMap(installedCaseMap??throw new InvalidOperationException("Case span map is not prepared."),tag,token):
            LegacyPreparedDictionaryRestartTags.ResolveAggregate(tag,[Direct!.RestartTags]);
    }
    public void ValidateCurrentMap()
    {if(Case is{} owned&&(installedCaseMap is null||!owned.Session.IsInstalledMap(installedCaseMap)))throw new InvalidOperationException("Case span map belongs to a different session refresh.");}
}
