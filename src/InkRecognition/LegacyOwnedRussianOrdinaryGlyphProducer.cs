using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>The explicit216BC0 in/out scratch and scalar residue. Copies protect
/// the256 labels; a failed/unsupported operation does not roll back earlier writes.</summary>
public sealed class LegacyOrdinaryGlyphProductionScratch
{
    private readonly ushort[] labels;
    public int RemainingCursor { get; internal set; }
    public byte ComponentFlag { get; internal set; }
    public byte ExternalVariant { get; internal set; }
    public uint Score0CBits { get; internal set; }
    public uint Score10Bits { get; internal set; }
    public int Cost14 { get; internal set; }
    public ushort PublicationFlags06 { get; internal set; }

    public LegacyOrdinaryGlyphProductionScratch(ReadOnlySpan<ushort> actualLabels256, int remainingCursor,
        byte componentFlag=0, byte externalVariant=0, uint score0CBits=0, uint score10Bits=0,
        int cost14=0, ushort publicationFlags06=0)
    {
        if(actualLabels256.Length!=256)throw new ArgumentException("Complete256-label scratch required.");
        labels=actualLabels256.ToArray();RemainingCursor=remainingCursor;ComponentFlag=componentFlag;
        ExternalVariant=externalVariant;Score0CBits=score0CBits;Score10Bits=score10Bits;
        Cost14=cost14;PublicationFlags06=publicationFlags06;
    }
    public ushort[] CopyLabels()=>labels.ToArray();
    internal void Write(ushort symbol)
    {
        if((uint)RemainingCursor>=256)throw new NotSupportedException("Native scratch write would escape its256-label owner.");
        labels[RemainingCursor]=symbol;
    }
    internal LegacyPreparedGlyphFinalizationInput Snapshot()=>new(true,RemainingCursor,labels,
        ComponentFlag,ExternalVariant,Score0CBits,Score10Bits,Cost14,PublicationFlags06);
}

/// <summary>Bounded216BC0 ordinary subtype0 producer with actual fresh Russian
/// main confidence and geometry. The72-byte backtrack row is explicitly prepared,
/// with its node/arc checked against the same graph. This does not produce that
/// row, choose a winning path, handle protocol delimiters or implement218560.</summary>
public sealed class LegacyOwnedRussianOrdinaryGlyphProducer
{
    private readonly LegacyOwnedRussianMainGlyphConfidence confidence;
    private readonly LegacyOwnedRussianGlyphSymbolResolver? resolver;
    private readonly LegacyOwnedRussianMainSourceSymbolCallbacks? sourceCallbacks;
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private readonly LegacyRussianModelBundle model;
    private readonly LegacyFreshCaseMainSourcePolicy? casePolicy;
    private readonly LegacyOwnedRussianMainGlyphScoring scoring;
    private readonly LegacyMainOrdinaryGlyphProductionOperations operations;
    private readonly ushort blank, unitScore, closed, apostrophe, quote;
    public LegacyFreshCaseMainSourcePolicy Policy => casePolicy ??
        throw new InvalidOperationException("The DIRECT producer has DirectPolicy, not an invented case policy.");
    public LegacyOwnedFreshMainWordControllerPolicy? DirectPolicy {get;}


    private LegacyOwnedRussianOrdinaryGlyphProducer(ReadOnlySpan<byte> resource,LegacyFreshCaseMainSourcePolicy? policy,
        LegacyOwnedFreshMainWordControllerPolicy? directPolicy,LegacyOwnedRussianMainGlyphConfidence confidence,
        LegacyCursiveSeparationTable table,LegacyOwnedRussianMainSourceSymbolCallbacks? sourceCallbacks,
        LegacyNativeTextPublicationData? publication,CancellationToken token,LegacyConfiguredSourceOperationDebit? debit=null)
    {
        casePolicy=policy;DirectPolicy=directPolicy;this.confidence=confidence;this.sourceCallbacks=sourceCallbacks;
        var controller=policy?.Controller??directPolicy??throw new ArgumentException("An actual owning source policy is required.");
        var owner=controller.Owner;graph=owner.SourceGraph;model=owner.CommonModel;
        bool direct=policy is null;
        if((direct? owner.IsCaseAware||!ReferenceEquals(confidence.DirectDictionaryOwner,owner.DictionaryOwner):
            !ReferenceEquals(confidence.CaseDictionaryOwner,policy!.DictionaryOwner))||
            !ReferenceEquals(confidence.Graph,graph)||!ReferenceEquals(confidence.Model,model)||
            confidence.OwnerScoreChannel88!=0)
            throw new ArgumentException("One actual fresh source, graph, model and confidence owner required.");
        if(resource.Length!=572763||!StringComparer.OrdinalIgnoreCase.Equals(
            Convert.ToHexString(SHA256.HashData(resource)),owner.SelectedSourceOwner.SourceFingerprint))
            throw new InvalidDataException("Same pinned acoustic source required.");
        if(direct)
        {
            if(sourceCallbacks is null||!ReferenceEquals(sourceCallbacks.Owner,confidence)||publication is null||
                controller.Configuration.Trace2368!=0||controller.Configuration.Mode22f6!=0||controller.Refresh.KnowledgeOption22f8!=0)
                throw new ArgumentException("Fresh ordinary DIRECT policy, exact source callbacks and publication data required.");
            if(LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet,token).Length!=0)
                throw new NotSupportedException("The DIRECT scalar branch requires its actual empty wide-pair table.");
            for(ushort i=0;i<model.AcousticAlphabet.LabelCount;i++)
            {
                token.ThrowIfCancellationRequested();
                if(model.AcousticAlphabet.TryGetText(i,out var text))foreach(var rune in text.EnumerateRunes())
                    if(publication.ReadScript(rune.Value)==5)
                        throw new NotSupportedException("Arabic map production is outside this ordinary scalar branch.");
            }
        }
        else resolver=LegacyOwnedRussianGlyphSymbolResolver.Prepare(policy!.DictionaryOwner,
            policy.DictionaryOwner.Symbols.Preparation,token);
        scoring=new(resource,controller,confidence,table,token,debit:debit);
        //Engine053F20 writes the seven stored headers to8,A,C,E,10,12,14;
        //ALcount is separately1A. In particular headerE is178, not192.
        blank=U16(resource,99);unitScore=U16(resource,105);
        var alphabet=model.AcousticAlphabet;
        ushort Label(string text)=>alphabet.TryGetLabel(text,out ushort symbol)?symbol:ushort.MaxValue;
        closed=Label("\U000f0001");apostrophe=Label("'");quote=Label("\"");
        operations=new(alphabet.LabelCount,unitScore,apostrophe,quote,(node,arc,ct)=>
        {ct.ThrowIfCancellationRequested();var rows=graph.GetNode(node);if(arc>=rows.Count)throw new ArgumentException("Prepared row arc escapes the actual owned graph node.");return rows[arc].GroupId;},
            ResolveOwnedSymbol,scoring.GetCache,scoring.CreateRequest);
        token.ThrowIfCancellationRequested();
    }

    internal static LegacyOwnedRussianOrdinaryGlyphProducer PrepareConfiguredAugmentation(ReadOnlySpan<byte> resource,
        LegacyFreshCaseMainSourcePolicy policy,LegacyOwnedRussianMainGlyphConfidence confidence,
        LegacyCursiveSeparationTable table,LegacyConfiguredSourceOperationDebit debit,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(debit);debit.ReserveScoringScope(confidence,policy.Controller.Owner.MainLineSource,token);
        return new(resource,policy,null,confidence,table,null,null,token,debit);
    }

    public static LegacyOwnedRussianOrdinaryGlyphProducer Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshCaseMainSourcePolicy policy,LegacyOwnedRussianMainGlyphConfidence confidence,
        LegacyCursiveSeparationTable actualTable,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(policy);ArgumentNullException.ThrowIfNull(confidence);
        ArgumentNullException.ThrowIfNull(actualTable);token.ThrowIfCancellationRequested();
        return new(resource,policy,null,confidence,actualTable,null,null,token);
    }

    /// <summary>The same ordinary scalar producer bound to a genuine DIRECT
    /// source. Publication data proves absent Arabic mapping; actual empty wide
    /// pairs and same-confidence callbacks close the replacement branch. No
    /// case wrapper or prepared boolean substitutes for these owners.</summary>
    public static LegacyOwnedRussianOrdinaryGlyphProducer Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedFreshMainWordControllerPolicy directPolicy,LegacyOwnedRussianMainGlyphConfidence confidence,
        LegacyCursiveSeparationTable actualTable,LegacyOwnedRussianMainSourceSymbolCallbacks sourceCallbacks,
        LegacyNativeTextPublicationData publication,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(directPolicy);ArgumentNullException.ThrowIfNull(confidence);
        ArgumentNullException.ThrowIfNull(actualTable);ArgumentNullException.ThrowIfNull(sourceCallbacks);
        ArgumentNullException.ThrowIfNull(publication);token.ThrowIfCancellationRequested();
        return new(resource,null,directPolicy,confidence,actualTable,sourceCallbacks,publication,token);
    }

    /// <summary>Normal216BC0 success returns the existing2173E0 input snapshot.
    /// Structural scope guards precede native writes. Once entered, flags clear,
    /// cursor decrements, symbol is appended, then scores/flags are written.
    /// Unsupported services/cancellation after entry retain that prefix. The
    /// supported no-wide/no-Arabic resolver has no native false-return branch.</summary>
    public LegacyPreparedGlyphFinalizationInput Produce(ReadOnlySpan<byte> preparedRow72,
        LegacyOrdinaryGlyphProductionScratch scratch,ulong conversionArgument=0,
        ReadOnlySpan<byte> preparedCompact40=default,CancellationToken token=default)
        =>LegacyMainOrdinaryGlyphProductionKernel.Produce(operations,preparedRow72,scratch,conversionArgument,preparedCompact40,token);

    private LegacyRussianMainReplacementResolution ResolveOwnedSymbol(ushort symbol,ulong conversionArgument,int node,ushort arc,CancellationToken token)
    {
        if(resolver is not null)
        {var value=resolver.Resolve(symbol,conversionArgument,false,token);return new(value.EmittedSymbol,value.NormalizedOriginal,0);}
        if(unchecked((byte)conversionArgument)!=0&&symbol==model.AcousticAlphabet.LabelCount+1)return new(apostrophe,apostrophe,0);
        if(unchecked((byte)conversionArgument)!=0&&symbol==model.AcousticAlphabet.LabelCount+2)return new(quote,quote,0);
        if(model.AcousticAlphabet.TryGetLabel("\ufffd",out ushort replacement)&&symbol==replacement)
            return sourceCallbacks!.ResolveReplacement(node,arc,token);
        return new(symbol,symbol,0);
    }

    private static ushort U16(ReadOnlySpan<byte> bytes,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(bytes[at..]);
    private static void W16(Span<byte> bytes,int at,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(bytes[at..],value);
}
