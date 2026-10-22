using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglishCaseMain.Probe")]
namespace InkRecognition;

/// <summary>Explicit fresh one13B case M/N/source68 component. Not a complete
///290 query backend, language history, controller search or recognized word.</summary>
public sealed class LegacyEnglishFreshCaseMainSource
{
    private readonly object gate=new();
    private readonly byte[] seed,primary,secondary;
    private readonly uint[] callbacks;
    private readonly LegacyDictionaryInitialSymbolMaps maps;
    private readonly LegacySymbolContextFactory contexts;
    private readonly LegacyInkRelatedForms[] triplets;
    private readonly Source68Forwarder forwarder;
    private LegacyPreparedDictionaryWorkingMap working;
    private LegacyEnglishFreshCaseMainScorer? scorer;
    private LegacyEnglishFreshCaseMainSpanSession? spanOwner;
    private LegacyEnglishMainD0Bounds bounds;
    private D0Lease? activeD0;
    private object? activeScore;
    private bool faulted,busy;
    private long version;
    private int entered,selected;
    public LegacyEnglish119MainKnowledge Knowledge{get;}
    public LegacyEnglishLkFreshNumericBinding NumericOwner{get;}
    public LegacySource247WireLayout Source=>Knowledge.Source;
    public uint DescriptorFlags{get;}
    public ushort CasePathVersion{get;}
    public int InitialContextCount=>86;
    public long AdditionalLogicalEnvelopeBytes=>1048576;
    public const long CompleteLogicalEnvelopeUpperBytes=81L*1024*1024;
    public bool IsFaulted{get{lock(gate)return faulted;}}
    public int EnteredPreparations{get{lock(gate)return entered;}}
    public int ConsumedSelectedRows{get{lock(gate)return selected;}}
    public uint[] CopyEnumerationTokens()=>callbacks.ToArray();
    public byte[] CopyControllerSeed()=>seed.ToArray();
    public byte[] CopyPrimaryMask()=>primary.ToArray();
    public byte[] CopySecondaryMask()=>secondary.ToArray();
    public ushort[] CopyPreservedMap()=>maps.CopyPrimary();
    public ushort[] CopyAuxiliaryMap()=>maps.CopyAuxiliary();
    public ulong[] CopyInitialSource68()=>maps.CopyInitialContexts();
    public LegacySymbolContextNode[] CopyInitialContextNodes()=>contexts.CopyNodes().ToArray();
    public int[] CopyInitialPageCounts()=>contexts.CopyPageUsedCounts();
    public ulong InitialRootIdentity=>contexts.RootTreeIdentity;
    public ushort[] CopyWorkingMap(){lock(gate)return working.CopySelectedMap(false,0);}
    private LegacyEnglishFreshCaseMainSource(LegacyEnglish119MainKnowledge k,LegacyEnglishLkFreshNumericBinding n,
        LegacyDictionaryInitialSymbolMaps m,LegacySymbolContextFactory c,uint flags,ushort v,
        uint[] tokens,byte[] s,byte[] p,byte[] secondary)
    {Knowledge=k;NumericOwner=n;maps=m;contexts=c;DescriptorFlags=flags;CasePathVersion=v;callbacks=tokens;seed=s;primary=p;this.secondary=secondary;triplets=Source.Alphabet.CopyStoredCaseTriplets();working=maps.CreateWorkingMap(flags);forwarder=new(this);}
    public static LegacyEnglishFreshCaseMainSource Prepare(LegacyEnglish119MainKnowledge knowledge,
        LegacyEnglishLkFreshNumericBinding numeric,LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,CancellationToken token=default)
        =>PrepareCore(knowledge,numeric,cases,publication,null,token);
    internal static LegacyEnglishFreshCaseMainSource PrepareCore(LegacyEnglish119MainKnowledge knowledge,
        LegacyEnglishLkFreshNumericBinding numeric,LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,Action<string>? checkpoint,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(knowledge);ArgumentNullException.ThrowIfNull(numeric);ArgumentNullException.ThrowIfNull(cases);ArgumentNullException.ThrowIfNull(publication);token.ThrowIfCancellationRequested();
        var data=numeric.DataOwner;var language=numeric.LanguageOwner;
        if(!ReferenceEquals(knowledge.Source,data.Source)||!ReferenceEquals(data.AcousticAlphabet,knowledge.Source.Alphabet)||!knowledge.Owner.IsComplete||knowledge.Owner.IsFaulted)
            throw new ArgumentException("Same actual complete MAIN/P-L Source instance required.");
        if(data.LexicalAlphabet.LabelCount!=104||data.AcousticAlphabet.LabelCount!=217||language.Order!=3||language.ClassSlots!=1024||numeric.Profile!=2||numeric.SourceScale!=1||numeric.LiteralPacketByte0!=0||knowledge.GroupCount!=7||knowledge.GroupSymbolCount!=25)
            throw new NotSupportedException("Exact fresh English one13B case component required.");
        _=LegacyEnglish119MainKnowledge.Preflight(knowledge.MainWire.Fields.Count,7,25,knowledge.MainWire.Networks.Count,104,217);
        _=LegacyEnglishMainInkSnapshot.Preflight(knowledge.Owner.GroupCount,knowledge.Owner.NodeCount,knowledge.Owner.CandidateCount);
        byte[] version=language.CopyFieldPayload("150.casePathVersion");
        if(version.Length!=2||BinaryPrimitives.ReadUInt16LittleEndian(version)!=0x200)throw new NotSupportedException("Actual English TCM0200 owner required.");
        var stored=data.AcousticAlphabet.CopyStoredCaseTriplets();if(stored.Length!=30)throw new NotSupportedException("Actual stored case triplets required.");
        foreach(var row in stored)foreach(ushort label in new[]{row.First,row.Second,row.Third})if(label!=65535&&label>=217)throw new InvalidDataException("Owned case label outside217.");
        for(int i=0;i<104;i++){token.ThrowIfCancellationRequested();if(data.LexicalAlphabet.IsVirtual((ushort)i))throw new NotSupportedException("No virtual allocation residue is synthesized.");}
        void Check(string stage){checkpoint?.Invoke(stage);token.ThrowIfCancellationRequested();}
        Check("case:preflight");
        // Finite initial-key pass only: this is NOT a native query-pool cap.
        var contexts=new LegacySymbolContextFactory(104);
        var maps=LegacyDictionaryInitialSymbolMaps.Prepare(data.AcousticAlphabet,data.LexicalAlphabet,contexts,[],token);
        if(contexts.NodeCount!=86)throw new InvalidDataException("Actual initial context topology required.");
        var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(data.Prefix,data.Dictionary,(uint)language.Order,true);
        var selection=new LegacyPreparedDictionarySourceSelectionBuilder();selection.Add(new(descriptor,1024,contexts.ContainerIdentity));var selected=selection.Prepare();
        if(selected.Count!=1||descriptor.Flags!=0xc0e||descriptor.LanguageOrder!=3||selected[0].Priority!=1024||selected[0].SourceIndex!=0)throw new InvalidDataException("Actual one-source descriptor required.");
        Check("case:maps");
        var ranges=LegacyNativeCaseAlphabetRanges.Prepare(data.AcousticAlphabet,cases,publication,token);var enumeration=new LegacyNativeCaseSymbolEnumeration(ranges);
        var tokens=new List<uint>(936);foreach(ushort label in maps.CopyPrimary())
        {token.ThrowIfCancellationRequested();if(!enumeration.Enumerate(label,value=>{if(tokens.Count==936)throw new InvalidDataException("Whole callback bound exceeded.");tokens.Add(value);return true;},token))throw new InvalidDataException("Complete case enumeration required.");}
        if(tokens.Count!=544)throw new InvalidDataException("Exact actual case callback extent required.");
        var controller=LegacyLinguisticSymbolMaskController.Build(217,[new(tokens.ToArray(),ReadOnlyMemory<ushort>.Empty)],true,token);
        if(controller.Outcome!=LegacySymbolMaskControllerOutcome.Succeeded)throw new InvalidDataException("Complete case controller mask required.");
        Check("case:enumerated");
        var producer=new LegacyOrdinarySymbolMaskProducer(data.AcousticAlphabet,data.AcousticAlphabet.CopySerializedHeader());
        var masks=producer.Build(controller.Outcome,controller.Mask.Span,knowledge.CopyGroups(),[],[],[],token);
        var result=new LegacyEnglishFreshCaseMainSource(knowledge,numeric,maps,contexts,descriptor.Flags,0x200,tokens.ToArray(),controller.Mask.ToArray(),masks.Primary.ToArray(),masks.Secondary.ToArray());
        Check("case:complete");result.RequireCurrent();return result;
    }
    internal void RequireCurrent()
    {
        if(!Knowledge.Owner.IsComplete||Knowledge.Owner.IsFaulted||!ReferenceEquals(Knowledge.MainWire.Source,Source)||!ReferenceEquals(NumericOwner.DataOwner.Source,Source))throw new InvalidOperationException("Live actual case MAIN source required.");
        lock(gate){if(faulted)throw new InvalidOperationException("Faulted case source requires fresh preparation.");}
    }
    internal void ClaimScorer(LegacyEnglishFreshCaseMainScorer actual,CancellationToken token)
    {RequireCurrent();token.ThrowIfCancellationRequested();lock(gate){token.ThrowIfCancellationRequested();if(faulted||busy||scorer is not null||!ReferenceEquals(actual.CaseSource,this))throw new InvalidOperationException("One fresh case scorer required.");scorer=actual;}}
    internal void PoisonScorer(LegacyEnglishFreshCaseMainScorer actual)
    {lock(gate){if(ReferenceEquals(scorer,actual)){faulted=true;busy=false;activeScore=null;activeD0=null;}}}
    internal object BeginScore(LegacyEnglishFreshCaseMainScorer actual,CancellationToken token)
    {RequireCurrent();token.ThrowIfCancellationRequested();lock(gate){token.ThrowIfCancellationRequested();if(faulted||busy||!ReferenceEquals(scorer,actual))throw new InvalidOperationException("Serialized exact case scorer required.");activeScore=new();busy=true;return activeScore;}}
    internal void FinishScore(LegacyEnglishFreshCaseMainScorer actual,object operation)
    {lock(gate){if(faulted||!busy||!ReferenceEquals(scorer,actual)||!ReferenceEquals(activeScore,operation))throw new InvalidOperationException("Exact active score operation required.");activeScore=null;busy=false;}}
    internal (LegacyPreparedDictionaryWorkingMap,Source68Forwarder) ClaimD0(LegacyEnglishFreshCaseMainSpanSession owner,LegacyEnglishMainD0Bounds b,CancellationToken token)
    {RequireCurrent();token.ThrowIfCancellationRequested();lock(gate){token.ThrowIfCancellationRequested();if(faulted||busy||spanOwner is not null||!ReferenceEquals(scorer,owner.Scorer))throw new InvalidOperationException("One fresh case D0 owner required.");spanOwner=owner;bounds=b;return(working,forwarder);}}
    internal D0Lease BeginD0(LegacyEnglishFreshCaseMainSpanSession owner,LegacyPreparedDictionaryWorkingMap previous,int count,CancellationToken token,Action<string>? observer=null)
    {
        RequireCurrent();token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();if(faulted||busy||!ReferenceEquals(spanOwner,owner)||!ReferenceEquals(working,previous))throw new InvalidOperationException("Exact serialized case D0 owner/map required.");
            if(entered>=bounds.MaximumPreparations||count<0||count>bounds.MaximumSelectedRows-selected)throw new ArgumentException("Complete remaining case D0 budget required before entry.");
            var next=checked(version+1);var lease=D0Lease.Issue(this,owner,previous,next,observer);busy=true;activeD0=lease;version=next;entered++;selected+=count;return lease;
        }
    }
    internal void ValidateLease(D0Lease lease)
    {lock(gate){if(faulted||!busy||!ReferenceEquals(activeD0,lease)||!ReferenceEquals(lease.Root,this)||!ReferenceEquals(spanOwner,lease.Owner)||lease.Version!=version)throw new InvalidOperationException("Current opaque same-case D0 lease required.");}}
    internal void Install(D0Lease lease,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();ValidateLease(lease);var next=lease.Pending??throw new InvalidOperationException("Actual once-forwarded pending map required.");
            if(lease.Installed||!ReferenceEquals(working,lease.Previous)||next.DescriptorFlags!=DescriptorFlags||next.AcousticSymbolCount!=217||next.LexicalSymbolCount!=104||!next.SharesAuxiliaryMap(working))throw new InvalidDataException("Exact case map lineage required.");
            working=next;lease.Installed=true;
        }
    }
    internal void Complete(D0Lease lease)
    {lock(gate){ValidateLease(lease);if(!lease.Installed||!ReferenceEquals(working,lease.Pending))throw new InvalidOperationException("Installed complete case D0 map required.");activeD0=null;busy=false;}}
    internal bool IsCurrent(LegacyEnglishFreshCaseMainSpanSession owner,long epoch)
    {lock(gate)return !faulted&&!busy&&ReferenceEquals(spanOwner,owner)&&version==epoch;}
    internal void PoisonD0(LegacyEnglishFreshCaseMainSpanSession owner)
    {lock(gate){if(ReferenceEquals(spanOwner,owner)){faulted=true;busy=false;activeD0=null;}}}
    // Private issuing constructor. Internal factory cannot create authority:
    // only the root's exact active identity is accepted by ValidateLease.
    internal sealed class D0Lease
    {
        private readonly Action<string>? observer;
        private D0Lease(LegacyEnglishFreshCaseMainSource root,LegacyEnglishFreshCaseMainSpanSession owner,LegacyPreparedDictionaryWorkingMap previous,long version,Action<string>? observer){Root=root;Owner=owner;Previous=previous;Version=version;this.observer=observer;}
        internal static D0Lease Issue(LegacyEnglishFreshCaseMainSource root,LegacyEnglishFreshCaseMainSpanSession owner,LegacyPreparedDictionaryWorkingMap previous,long version,Action<string>? observer=null)=>new(root,owner,previous,version,observer);
        internal void Check(string stage,CancellationToken token){observer?.Invoke(stage);token.ThrowIfCancellationRequested();}
        internal LegacyEnglishFreshCaseMainSource Root{get;}
        internal LegacyEnglishFreshCaseMainSpanSession Owner{get;}
        internal LegacyPreparedDictionaryWorkingMap Previous{get;}
        internal long Version{get;}
        internal LegacyPreparedDictionaryWorkingMap? Pending{get;set;}
        internal byte[]? RawPermissions{get;set;}
        internal bool Installed{get;set;}
    }
    internal sealed class Source68Forwarder
    {
        private readonly LegacyEnglishFreshCaseMainSource root;
        private LegacyOrdinaryGraphAcousticPermissionSession? session;
        internal Source68Forwarder(LegacyEnglishFreshCaseMainSource owner){root=owner;}
        internal void BindSession(LegacyOrdinaryGraphAcousticPermissionSession actual,LegacyPreparedDictionaryWorkingMap initial)
        {lock(root.gate){if(!ReferenceEquals(root.forwarder,this)||session is not null||root.spanOwner is null||!ReferenceEquals(root.working,initial)||root.faulted)throw new InvalidOperationException("Exact fresh case target binding required.");session=actual;}}
        internal void RequireLease(LegacyOrdinaryGraphAcousticPermissionSession actual,D0Lease lease)
        {root.ValidateLease(lease);if(!ReferenceEquals(root.forwarder,this)||!ReferenceEquals(session,actual)||lease.Pending is not null||lease.Installed)throw new InvalidOperationException("Exact unconsumed case source68 invocation required.");}
        internal LegacyPreparedAcousticPermissionResult BuildAndRefresh(LegacyOrdinaryGraphAcousticPermissionSession actual,D0Lease lease,
            LegacyPreparedDictionaryWorkingMap source,ReadOnlySpan<ushort> reserved,IReadOnlyList<LegacyPreparedAcousticPermissionGroup> rows,CancellationToken token)
        {
            RequireLease(actual,lease);if(!ReferenceEquals(lease.Previous,source))throw new InvalidOperationException("Exact active previous map required.");
            var bytes=LegacyPreparedAcousticDictionaryPermissions.BuildBytes(217,reserved,rows,token);if(!bytes.Succeeded)return new(false,ReadOnlyMemory<byte>.Empty,null);
            lease.Check("source:bytes",token);
            var expanded=LegacyNativeCasePermissions.ExpandStoredPermissionsCore(217,root.triplets,root.CasePathVersion,bytes.Permissions.Span,token);
            lease.Check("source:expanded",token);var next=source.Refresh(expanded,token);lease.Check("source:refreshed",token);root.ValidateLease(lease);
            lease.RawPermissions=bytes.Permissions.ToArray();lease.Pending=next;lease.Check("source:forwarded",token);return new(true,expanded,next);
        }
    }
}
