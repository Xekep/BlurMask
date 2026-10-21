namespace InkRecognition;

/// <summary>Actual EN P/L maps and MAIN119 masks under the explicitly independent
///main13B-only/nonbypassed/no-user-restriction library policy. Not native fullroot
///configured sources, refresh epochs, MAIN neural scores or a linguistic winner.</summary>
public sealed class LegacyEnglishMainOnlySourceMasks
{
    private readonly ushort[] primaryMap,auxiliaryMap;
    private readonly ulong[] contextMap;
    private readonly byte[] seed,primary,secondary;
    private readonly LegacySymbolContextFactory contexts;
    public LegacyEnglish119MainKnowledge Knowledge{get;}
    public LegacyEnglishLkFreshNumericBinding NumericOwner{get;}
    public LegacySource247WireLayout Source=>Knowledge.Source;
    public string Policy=>"independent-main13B-only/nonbypassed/no-user-restriction";
    public uint DescriptorFlags{get;}
    public uint LanguageOrder{get;}
    public uint SelectedPriority{get;}
    public int SelectedSourceIndex{get;}
    public int ContextCount=>contexts.NodeCount;
    public int AcousticSymbolCount=>217;
    public int NeuralClassCount=>182;
    public int CacheWidth=>220;
    public bool HasContextualForms=>false;
    public int WidePairCount=>0;
    public long LogicalCopyWorkBytes=>Knowledge.LogicalCopyWorkBytes;
    public ushort[] CopyPrimaryMap()=>primaryMap.ToArray();
    public ushort[] CopyAuxiliaryMap()=>auxiliaryMap.ToArray();
    public ulong[] CopyInitialContextMap()=>contextMap.ToArray();
    public byte[] CopyControllerSeed()=>seed.ToArray();
    public byte[] CopyPrimaryMask()=>primary.ToArray();
    public byte[] CopySecondaryMask()=>secondary.ToArray();
    private LegacyEnglishMainOnlySourceMasks(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,
        LegacyDictionaryInitialSymbolMaps maps,LegacySymbolContextFactory contexts,LegacyPreparedDictionarySourceDescriptor descriptor,
        uint priority,int index,byte[] seed,byte[] primary,byte[] secondary)
    {Knowledge=knowledge;NumericOwner=numeric;primaryMap=maps.CopyPrimary();auxiliaryMap=maps.CopyAuxiliary();contextMap=maps.CopyInitialContexts();this.contexts=contexts;DescriptorFlags=descriptor.Flags;LanguageOrder=descriptor.LanguageOrder;SelectedPriority=priority;SelectedSourceIndex=index;this.seed=seed;this.primary=primary;this.secondary=secondary;}
    public static LegacyEnglishMainOnlySourceMasks Create(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding actualFresh,CancellationToken token=default)=>CreateCore(knowledge,actualFresh,null,token);
    internal static LegacyEnglishMainOnlySourceMasks CreatePrepared(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding actualFresh,Action<string> checkpoint,CancellationToken token=default)=>CreateCore(knowledge,actualFresh,checkpoint,token);
    private static LegacyEnglishMainOnlySourceMasks CreateCore(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,Action<string>? checkpoint,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(knowledge);ArgumentNullException.ThrowIfNull(numeric);token.ThrowIfCancellationRequested();
        var data=numeric.DataOwner;var language=numeric.LanguageOwner;
        if(!ReferenceEquals(knowledge.Source,data.Source)||!ReferenceEquals(data.AcousticAlphabet,knowledge.Source.Alphabet)||!knowledge.Owner.IsComplete||knowledge.Owner.IsFaulted)
            throw new ArgumentException("SAME actual completed MAIN and P/L source instance required.");
        if(data.LexicalAlphabet.LabelCount!=104||data.AcousticAlphabet.LabelCount!=217||language.Order!=3||language.ClassSlots!=1024||numeric.Profile!=2||numeric.SourceScale!=1||numeric.LiteralPacketByte0!=0||knowledge.GroupCount!=7||knowledge.GroupSymbolCount!=25)
            throw new InvalidDataException("Actual completed English fresh sparse binding required.");
        _=LegacyEnglish119MainKnowledge.Preflight(knowledge.MainWire.Fields.Count,7,25,knowledge.MainWire.Networks.Count,104,217);
        void Check(string stage){checkpoint?.Invoke(stage);token.ThrowIfCancellationRequested();}
        //The accepted AK fingerprint is pinned in Knowledge. Static M scans all
        //actual217 texts through the pinned716 Script table: no Arabic candidate,
        //so native contextual88 staysNULL (V68), despite V58 case maps being true.
        //There are no wide candidates even BEFORE native restricted eligibility.
        //No caller-supplied bool, empty-form assertion, residue or map is accepted.
        for(int i=0;i<104;i++){token.ThrowIfCancellationRequested();if(data.LexicalAlphabet.IsVirtual((ushort)i))throw new NotSupportedException("Actual nonvirtual lexical104 required; no allocator residue synthesized.");}
        Check("masks:preflight");
        var contexts=new LegacySymbolContextFactory(104);
        var maps=LegacyDictionaryInitialSymbolMaps.Prepare(data.AcousticAlphabet,data.LexicalAlphabet,contexts,[],token);
        if(contexts.NodeCount!=86)throw new InvalidDataException("Actual shared context source map required.");
        Check("masks:source-maps");
        var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(data.Prefix,data.Dictionary,(uint)language.Order,true);
        var selection=new LegacyPreparedDictionarySourceSelectionBuilder();selection.Add(new(descriptor,(uint)language.ClassSlots,contexts.ContainerIdentity));var selected=selection.Prepare();
        if(selected.Count!=1||descriptor.Flags!=0xc0e||descriptor.LanguageOrder!=3||selected[0].Priority!=1024||selected[0].SourceIndex!=0)
            throw new InvalidDataException("Actual fresh independent main13B source descriptor required.");
        Check("masks:selected-main");
        //1D6D50 enumerates all104 labels;093AF0 maps preserved58, not V58query.
        var source=maps.CreateSymbolSource();var controller=LegacyLinguisticSymbolMaskController.Build(217,[source],true,token);
        if(controller.Outcome!=LegacySymbolMaskControllerOutcome.Succeeded)throw new InvalidDataException("Actual owned enumeration must complete.");
        Check("masks:enumerated");
        var producer=new LegacyOrdinarySymbolMaskProducer(data.AcousticAlphabet,data.AcousticAlphabet.CopySerializedHeader());
        var masks=producer.Build(controller.Outcome,controller.Mask.Span,knowledge.CopyGroups(),[],[],[],token);
        Check("masks:produced");
        var result=new LegacyEnglishMainOnlySourceMasks(knowledge,numeric,maps,contexts,descriptor,selected[0].Priority,selected[0].SourceIndex,controller.Mask.ToArray(),masks.Primary.ToArray(),masks.Secondary.ToArray());
        Check("masks:complete");return result;
    }
}
