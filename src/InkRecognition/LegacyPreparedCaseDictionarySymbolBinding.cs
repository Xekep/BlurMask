namespace InkRecognition;

/// <summary>Prepared292 symbol enumeration and permission→inner2B7 refresh over
/// the same owned initial maps/alphabet. Source selection and live query epochs
/// remain the caller's owning factory responsibility.</summary>
public sealed class LegacyPreparedCaseDictionarySymbolBinding
{
    private readonly LegacyNativeCaseSymbolEnumeration enumeration;
    private readonly LegacyNativeCasePermissions permissions;
    private readonly LegacyPreparedDictionaryWorkingMap initialWorkingMap;
    private readonly uint[] tokens;
    public LegacyDictionaryInitialSymbolMaps Symbols { get; }
    public LegacyNativeCasePreparation Preparation { get; }
    public LegacyWordLanguageModel Language { get; }
    public uint DescriptorFlags => initialWorkingMap.DescriptorFlags;
    //All source68 snapshots retain this same immutable58/60 owner.
    internal LegacyPreparedDictionaryWorkingMap InitialWorkingMap => initialWorkingMap;

    public LegacyPreparedCaseDictionarySymbolBinding(LegacyDictionaryInitialSymbolMaps symbols,
        LegacyNativeCasePreparation preparation,LegacyWordLanguageModel language,uint descriptorFlags,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(symbols);ArgumentNullException.ThrowIfNull(preparation);
        ArgumentNullException.ThrowIfNull(language);cancellationToken.ThrowIfCancellationRequested();
        if(!ReferenceEquals(symbols.AcousticAlphabet,preparation.Alphabet))
            throw new ArgumentException("Case preparation must own the initial map's acoustic alphabet.");
        Symbols=symbols;Preparation=preparation;Language=language;
        enumeration=new(preparation.Ranges);permissions=new(preparation.Alphabet,language);
        initialWorkingMap=symbols.CreateWorkingMap(descriptorFlags);
        var output=new List<uint>();
        //093AF0 reads preserved58 for every lexical label.232C80 receives
        //those mapped acoustic/control labels, not lexical ordinals.
        foreach(ushort label in symbols.CopyPrimary())
        {
            cancellationToken.ThrowIfCancellationRequested();
            enumeration.Enumerate(label,value=>{output.Add(value);return true;},cancellationToken);
        }
        tokens=output.ToArray();
    }

    /// <summary>Already mapped tokens, including repeated callbacks and controls.
    /// An empty dictionary map prevents a second lexical→acoustic conversion.</summary>
    public LegacyLinguisticSymbolSource CreateSymbolSource()=>new(tokens.ToArray(),ReadOnlyMemory<ushort>.Empty);

    /// <summary>Exact234840 stored-case propagation followed by inner093F80.
    /// A new map starts from preserved58 on every call, never a previous filter.</summary>
    public LegacyPreparedDictionaryWorkingMap Refresh(ReadOnlySpan<byte> acousticPermissions,
        CancellationToken cancellationToken=default)
        =>initialWorkingMap.Refresh(permissions.ExpandStoredPermissions(acousticPermissions,cancellationToken),cancellationToken);

    /// <summary>Compose1C01B0 permission production with wrapper292 source68.
    /// Failed acoustic preparation does not forward partial permissions.</summary>
    public LegacyPreparedAcousticPermissionResult BuildAndRefresh(
        IReadOnlyList<LegacyPreparedAcousticPermissionGroup> processedGroups,CancellationToken cancellationToken=default)
    {
        var reserved=LegacyPreparedAcousticDictionaryPermissions.ReadReservedSlots(Preparation.Alphabet,
            Preparation.Alphabet.CopySerializedHeader());
        var produced=LegacyPreparedAcousticDictionaryPermissions.BuildAndRefresh(initialWorkingMap,reserved,processedGroups,cancellationToken);
        if(!produced.Succeeded)return new(false,ReadOnlyMemory<byte>.Empty,null);
        byte[] expanded=permissions.ExpandStoredPermissions(produced.Permissions.Span,cancellationToken);
        return new(true,expanded,initialWorkingMap.Refresh(expanded,cancellationToken));
    }
}
