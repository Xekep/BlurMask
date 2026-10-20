using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyGlobalTextSourceWrapper.Probe")]
namespace InkRecognition;

/// <summary>Bounded actual global024F30 CHAR source publication. One global
/// scope/Seed/3E and original input domain survive all native-order operations.
/// This does not register WORD/TEXT or accept an external document.</summary>
public sealed class LegacyFreshMainGlobalTextSourceWrapper
{
    private static readonly ConditionalWeakTable<LegacyFreshMainGlobalTextGlyphChoices,LegacyFreshMainGlobalTextSourceWrapper> owners=new();
    private readonly object gate=new();
    private readonly LegacyNativeInkPositionRange[] original,wordClip;
    private readonly LegacyNativeSelectedInkRange[] inputs;
    private readonly byte[][] descriptors;
    private readonly ulong[] references;
    private readonly LegacyOwnedRawInkRangeOperations operations;
    private readonly Dictionary<int,LegacyFreshMainGlobalTextSourcePayload> payloads=[];
    private readonly List<string> trace=[];
    private LegacyNativeInkPositionRange[][] ranges=[];
    private LegacyFreshMainGlobalGlyphDocumentPositions?[] positions=[];
    private bool[] skipped=[],positive=[],joiner=[];
    private byte[][] descriptorSnapshots=[];
    private LegacyGlobalSourceSelectedGlyph? currentSelection;
    private LegacyFreshMainGlobalProcessedGlyphSource? currentSource;
    private LegacyFreshMainGlobalTextSourcePayload? initializingPayload;
    private bool entered,firstPassComplete,faulted,completed;
    private int nextSelection;
    private int lineOffset;
    private LegacyFreshMainGlobalWordTextPublication? reservedWordParent;
    public LegacyFreshMainGlobalTextGlyphChoices Choices{get;}
    public LegacyLibraryGlobalTextPublicationScope Scope=>Choices.Scope;
    public LegacyFreshMainGlobalNoCandidateSourceCompletion Completion=>Choices.Completion;
    public LegacyOwnedMainTextSourceInterner CharacterInterner=>Scope.SharedState.Characters;
    public bool IsFaulted{get{lock(gate)return faulted;}}
    private LegacyFreshMainGlobalTextSourceWrapper(LegacyFreshMainGlobalTextGlyphChoices choices,CancellationToken token)
    {
        Choices=choices;choices.Validate(token);original=choices.Projection.CopyOriginalRoot(token);
        wordClip=Completion.CopyRanges();inputs=Scope.Document.CopySelectedInputs();
        references=Completion.Word.Word.FlatReferences.ToArray();operations=LegacyOwnedRawInkRangeOperations.Prepare(Scope.Document);
        descriptors=Completion.Snapshot.SourcePolicy.Owner.Graph.Source.Source.CopyDescriptors();
        ValidateDomain(token);
    }
    public static LegacyFreshMainGlobalTextSourceWrapper Prepare(LegacyLibraryGlobalTextPublicationScope actualScope,
        LegacyFreshMainGlobalNoCandidateSourceCompletion actualCompletion,ReadOnlySpan<byte> acousticResource,
        LegacyNativeTextPublicationData properties,LegacyNativeUnicodeNormalizationData normalization,
        LegacyNativeCircledPayloadData circles,CancellationToken token=default)
    {
        var choices=LegacyFreshMainGlobalTextGlyphChoices.Prepare(actualScope,actualCompletion,acousticResource,properties,normalization,circles,token);
        var result=owners.GetValue(choices,c=>new(c,token));result.ValidatePreclaim(token);return result;
    }
    private void ValidateDomain(CancellationToken token)
    {
        Scope.Validate();Completion.Validate(token);var shared=Scope.SharedState;var seed=Scope.Seed;
        if(!ReferenceEquals(shared.Document,Scope.Document)||!ReferenceEquals(shared.Strings,Choices.StringPool)||
            !ReferenceEquals(seed,Scope.Context.Result.PublicationSeed)||!ReferenceEquals(seed.StringPool,shared.Strings)||
            !ReferenceEquals(seed.PendingSelections,shared.Selections)||!ReferenceEquals(seed.InputBinding,Scope.Context.InputBinding)||
            !ReferenceEquals(shared.Selections.InputBinding,seed.InputBinding)||!ReferenceEquals(CharacterInterner.Document,Scope.Document))
            throw new ArgumentException("Exact global Document/Seed/input binding/shared3E/CHAR transaction required.");
        if(original.Length!=1||!LegacyOwnedRawInkRangeOperations.Equal(original,Scope.Context.InputBinding.CopyRanges())||
            !LegacyOwnedRawInkRangeOperations.Equal(original,Scope.Context.Result.CopyInputRanges208())||inputs.Any(r=>r.IsKnownText)||
            descriptors.Length!=1||(descriptors[0][0]&15)!=0||references.Length is <1 or>4096)
            throw new NotSupportedException("Bounded fresh full-domain ordinary global source collection required.");
        foreach(ulong reference in references)
            if(unchecked((short)(reference>>24))<0)throw new NotSupportedException("Negative/protocol global source wrappers remain excluded.");
    }
    internal void ValidatePreclaim(CancellationToken token)
    {
        Choices.Validate(token);ValidateDomain(token);
        if(entered||CharacterInterner.Count!=0||Scope.SharedState.Selections.CopyPendingSelections().Count!=0)
            throw new InvalidOperationException("Fresh empty actual global CHAR cache/transaction and unused wrapper required.");
    }
    internal void Validate(CancellationToken token)
    {
        Choices.Validate(token);ValidateDomain(token);
        if(faulted)throw new InvalidOperationException("Entered global source-wrapper prefix is poisoned.");
    }
    internal ulong ReferenceAt(int index)=>references[index];
    internal void RequireNextSelection(int index,CancellationToken token)
    {Validate(token);if(!entered||!firstPassComplete||index!=nextSelection||(uint)index>=(uint)references.Length||skipped[index])throw new InvalidOperationException("Actual next non-skipped source after complete first pass required.");}
    internal void RequireCurrentSelection(LegacyGlobalSourceSelectedGlyph selected,CancellationToken token)
    {RequireNextSelection(selected.Index,token);if(!ReferenceEquals(currentSelection,selected)||!ReferenceEquals(selected.Owner,this))throw new ArgumentException("Actual current global selection receipt required.");}
    internal void RequireCurrentSource(LegacyFreshMainGlobalProcessedGlyphSource source,CancellationToken token)
    {RequireCurrentSelection(source.Selected,token);if(!ReferenceEquals(currentSource,source)||!ReferenceEquals(source.Owner,this))throw new ArgumentException("Actual current processed global receipt required.");}
    internal void RequireInitializingPayload(LegacyFreshMainGlobalTextSourcePayload payload)
    {
        RequireCurrentSource(payload.Source,default);
        if(!ReferenceEquals(initializingPayload,payload)||!ReferenceEquals(payload.Node.FirstGlobalSource,payload.Source)||
            !ReferenceEquals(payload.Node.Owner,CharacterInterner))throw new ArgumentException("Actual newly interned global source payload required.");
    }
    internal void RequireReusablePayload(LegacyFreshMainGlobalTextSourcePayload payload,LegacyGlobalSourceSelectedGlyph selected,CancellationToken token)
    {
        RequireCurrentSelection(selected,token);
        if(!payload.IsComplete||!ReferenceEquals(payload.Owner,this)||!ReferenceEquals(payload.Node.FirstGlobalSource,payload.Source)||
            !payloads.TryGetValue(payload.Node.Identity,out var actual)||!ReferenceEquals(actual,payload))
            throw new ArgumentException("Exact successfully initialized same-wrapper global payload required.");
    }
    internal (byte Prior,int Visits) ReadPrior(LegacyFreshMainGlobalProcessedGlyphSource source,CancellationToken token)
    {
        RequireCurrentSource(source,token);
        return VisitFreshPrior(source.OriginalPositions.Source.References,inputs,token);
    }
    //The actual route passes only its retained V140/C4A rows. Internal
    //numeric diagnostics separately exercise native empty/reversed traversal.
    internal static (byte Prior,int Visits) VisitFreshPrior(IEnumerable<LegacyOrdinaryGraphSourceReference> references,
        LegacyNativeSelectedInkRange[] inputs,CancellationToken token)
    {
        int visits=0;byte prior=1;
        foreach(var r in references)
        {
            if(r.First.First!=0||r.Last.First!=0||(uint)r.First.Second>=(uint)inputs.Length||(uint)r.Last.Second>=(uint)inputs.Length)
                throw new NotSupportedException("Actual fresh group0 original C4A bounds required.");
            for(int i=r.First.Second;i<=r.Last.Second;i++)
            {token.ThrowIfCancellationRequested();if(++visits>1_048_576||inputs[i].IsKnownText)throw new NotSupportedException("Fresh bounded C4A visitation required.");prior=0;}
        }
        return(prior,visits);
    }
    internal int ReadSourceLine(LegacyFreshMainGlobalProcessedGlyphSource source,CancellationToken token)
    {
        RequireCurrentSource(source,token);if(unchecked((short)(source.Reference>>24))<0)return-1;
        var graph=Completion.Snapshot.SourcePolicy.Owner.Graph;var metadata=source.OriginalPositions.Source;
        if(!ReferenceEquals(metadata.Owner,Completion.Source.Session!.Associations.Metadata)||graph.State.NodeCount<=0||
            (uint)metadata.AcousticGroupId>=(uint)graph.State.Groups.Count)
            throw new NotSupportedException("Actual nonempty global V68/V78 group source required.");
        ushort ordinal=graph.State.Groups[metadata.AcousticGroupId].DescriptorOrdinal;
        if(ordinal>=descriptors.Length||descriptors[ordinal].Length!=88)throw new NotSupportedException("Actual owned source descriptor88 required.");
        int line=BinaryPrimitives.ReadInt32LittleEndian(descriptors[ordinal].AsSpan(16));
        if(graph.Source.ResolveGroupSource(metadata.AcousticGroupId,token).Primary.LineId!=line)
            throw new InvalidDataException("Actual graph group and assigned descriptor LineId disagree.");
        return unchecked(line+lineOffset);
    }
    public LegacyFreshMainGlobalTextSourceWord Publish(ReadOnlySpan<byte> allocator40,ReadOnlySpan<byte> selection4,CancellationToken token=default)
        =>PublishCore(allocator40,selection4,0,token);
    internal void ReserveForWord(LegacyFreshMainGlobalWordTextPublication actual,CancellationToken token)
    {
        lock(gate)
        {
            ValidatePreclaim(token);
            if(reservedWordParent is not null||!ReferenceEquals(actual.Wrapper,this))
                throw new InvalidOperationException("Unused actual global wrapper required for WORD reservation.");
            //Reserve the public Choices entrypoint too, before any WORD
            //interner/payload prefix. No concurrent standalone publisher can
            //claim this scope in the parent-before-CHAR interval.
            Choices.ClaimSourceWrapper(this,token);reservedWordParent=actual;
        }
    }
    internal LegacyFreshMainGlobalTextSourceWord PublishIntoWord(LegacyFreshMainGlobalWordTextPublication actual,
        ReadOnlySpan<byte> allocator40,ReadOnlySpan<byte> selection4,int preparedCharFailure,CancellationToken token)
        =>PublishCore(allocator40,selection4,preparedCharFailure,token,actual);
    internal void FaultReservedWord(LegacyFreshMainGlobalWordTextPublication actual)
    {
        lock(gate)
        {
            if(!ReferenceEquals(reservedWordParent,actual))throw new ArgumentException("Exact reserved WORD parent required.");
            if(!faulted){faulted=true;trace.Add("word-parent-fault");}
            Choices.FaultSourceWrapper(this);
        }
    }
    internal LegacyFreshMainGlobalTextSourceWord PublishPreparedCharFailure(ReadOnlySpan<byte> allocator40,
        ReadOnlySpan<byte> selection4,int failure,CancellationToken token=default)
    {if(failure is<1 or>4)throw new ArgumentOutOfRangeException(nameof(failure));return PublishCore(allocator40,selection4,failure,token);}
    private LegacyFreshMainGlobalTextSourceWord PublishCore(ReadOnlySpan<byte> allocator40,ReadOnlySpan<byte> selection4,
        int preparedCharFailure,CancellationToken token,LegacyFreshMainGlobalWordTextPublication? wordParent=null)
    {
        lock(gate)
        {
            ValidatePreclaim(token);
            if(!ReferenceEquals(reservedWordParent,wordParent))throw new InvalidOperationException("This wrapper belongs to a reserved native-order WORD publication.");
            wordParent?.RequireCharacterEntry(this,token);
            if(allocator40.Length!=40||selection4.Length%4!=0||selection4.Length>65536*4)
                throw new ArgumentException("Complete allocator40 and whole bounded selection4 storage required.");
            if(wordParent is null)Choices.ClaimSourceWrapper(this,token);
            else Choices.ValidateWrapper(this,token);
            entered=true;trace.Add("claim");
            try
            {
                byte[] beforeWord=Completion.Word.Word.CopyStorage();int nn=Completion.Source.CachedMainGroupEvaluations;
                //Native028902 initializes this at each alternative; LineId is
                //still read from its actual group/descriptor producer.
                lineOffset=0;int n=references.Length;var pool=Choices.Emitter.TopOwner.GlyphPool;
                ranges=new LegacyNativeInkPositionRange[n][];positions=new LegacyFreshMainGlobalGlyphDocumentPositions?[n];
                descriptorSnapshots=new byte[n][];skipped=new bool[n];positive=new bool[n];joiner=new bool[n];
                bool hasJoiner=Choices.Emitter.PrimaryAlphabet.TryGetLabel("\u200d",out ushort joinerLabel);
                int retainedRanges=0;
                for(int i=0;i<n;i++)
                {
                    Validate(token);ulong reference=references[i],key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
                    //Snapshot THIS descriptor immediately before THIS refresh.
                    byte[] record=pool.CopyRecord(reference,token);descriptorSnapshots[i]=record;trace.Add("descriptor:"+i);
                    ushort flags=BinaryPrimitives.ReadUInt16LittleEndian(record.AsSpan(4)),symbol=BinaryPrimitives.ReadUInt16LittleEndian(record.AsSpan(8));
                    skipped[i]=(flags&7)==1&&(symbol&0x7ffc)!=0&&(symbol&3)==3;positive[i]=unchecked((short)(reference>>24))>=0;
                    joiner[i]=hasJoiner&&(flags&7)==0&&symbol==joinerLabel;
                    if(skipped[i]){ranges[i]=[];trace.Add("skip:"+i);continue;}
                    if(positive[i])wordParent?.VisitPositiveSource(this,i,token);
                    var refreshed=Choices.Refresh.RefreshOwner(key,allocator40,token);trace.Add("refresh:"+i);
                    var projected=Choices.Projection.Project(refreshed.Source,token);positions[i]=projected;
                    ranges[i]=operations.Intersect(projected.CopyRanges(token),wordClip,token);
                    if(ranges[i].Length>1_048_576-retainedRanges)throw new NotSupportedException("Global source range retention budget exceeded.");
                    retainedRanges+=ranges[i].Length;trace.Add("clip:"+i);
                }
                firstPassComplete=true;trace.Add("first-pass-complete");
                var output=new List<LegacyFreshMainGlobalTextSourceChoice>();LegacyNativeInkPositionRange[] ordinaryRanges=[];
                uint sourceFlags=0x200000,selectedFlags=0;
                for(int i=0;i<n;i++)if(!skipped[i]&&positive[i])sourceFlags&=~0x200000u;
                for(int i=0;i<n;i++)
                {
                    nextSelection=i;if(skipped[i])continue;
                    currentSelection=Choices.SelectForWrapper(this,i,selection4,token);trace.Add("select:"+i);
                    var processed=LegacyPreparedMainTextSourceProcessing.Process(operations,original,ranges,skipped,positive,joiner,i,token);
                    ranges[i]=processed.Key;
                    currentSource=new(this,currentSelection,positions[i]!,processed.Ordinary,processed.Key);trace.Add("process:"+i);
                    var key=CharacterInterner.InternGlobalProcessed(currentSource,token);trace.Add((key.Inserted?"intern-new:":"intern-reuse:")+i);
                    LegacyFreshMainGlobalTextSourcePayload payload;int marker;
                    if(key.Inserted)
                    {
                        var fresh=Choices.EmitFreshForWrapper(currentSelection,token);trace.Add("fresh:"+i);marker=fresh.AdjustedMarkerIndex;
                        payload=new(currentSource,key.Node,fresh);payloads.Add(key.Node.Identity,payload);initializingPayload=payload;
                        bool initialized=payload.Initialize(preparedCharFailure,token);trace.Add("payload:"+i);
                        if(!initialized)throw new NotSupportedException("Actual global CHAR registration refusal retained its typed metadata prefix.");
                        initializingPayload=null;
                    }
                    else
                    {
                        if(!payloads.TryGetValue(key.Node.Identity,out payload!)||!payload.IsComplete||
                            !ReferenceEquals(key.Node.FirstGlobalSource,payload.Source)||!ReferenceEquals(payload.Owner,this))
                            throw new NotSupportedException("Existing source node has no successful same-wrapper global payload.");
                        var reuse=payload.Reuse(currentSelection,token);trace.Add("reuse:"+i);marker=reuse.AdjustedMarkerIndex;
                        if(!reuse.Succeeded)throw new NotSupportedException("Actual global21760 reuse retained a failed prefix.");
                    }
                    if(currentSource.IsOrdinarySource)ordinaryRanges=operations.Union(ordinaryRanges,payload.Source.CopyRanges(token),token);
                    var rows=payload.CopyRecords(token);
                    if((uint)marker>=(uint)rows.Count||rows[marker].Text is null)
                        throw new NotSupportedException("Actual global source marker has no available row.");
                    selectedFlags|=rows[marker].Flags;output.Add(new(currentSource,payload,currentSelection.Selection,marker,!key.Inserted));
                    wordParent?.AppendCharacter(this,output[^1],token);
                    currentSelection=null;currentSource=null;trace.Add("glyph-complete:"+i);
                }
                if(!Completion.Word.Word.CopyStorage().AsSpan().SequenceEqual(beforeWord)||
                    !Completion.Word.Word.FlatReferences.SequenceEqual(references)||Completion.Snapshot.SourcePolicy.Owner.AcousticEvaluations!=nn)
                    throw new InvalidOperationException("Source wrapping changed actual WORD references or reran NN.");
                Validate(token);completed=true;trace.Add("complete");return new(this,output.ToArray(),ordinaryRanges,sourceFlags,selectedFlags);
            }
            catch{faulted=true;Choices.FaultSourceWrapper(this);trace.Add("fault");throw;}
        }
    }
    public string[] CopyTrace(){Scope.Validate();lock(gate)return trace.ToArray();}
    public IReadOnlyList<LegacyFreshMainGlobalTextSourcePayload> CopyPayloadPrefix(){Scope.Validate();lock(gate)return Array.AsReadOnly(payloads.Values.ToArray());}
    public LegacyNativeInkPositionRange[] CopyOriginalDomain(){Scope.Validate();return original.ToArray();}
    public LegacyNativeInkPositionRange[] CopyWordClip(){Scope.Validate();return wordClip.ToArray();}
    internal byte[][] CopyDescriptorSnapshots()=>descriptorSnapshots.Select(d=>d.ToArray()).ToArray();
    internal bool Completed=>completed;
}
