using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;
[assembly:InternalsVisibleTo("LegacyGlobalWordText.Probe")]
namespace InkRecognition;

internal readonly record struct LegacyGlobalWordTextPreparedFault(int Character=0,int Word=0,bool LineAllocation=false,int Text=0);
//Headless deterministic concurrency/cancellation boundary, internal diagnostics
//only. Production accepts neither a callback nor externally supplied state.
internal sealed class LegacyGlobalWordTextReservationBarrier:IDisposable
{
    internal ManualResetEventSlim Reserved{get;}=new(false);
    internal ManualResetEventSlim Resume{get;}=new(false);
    internal void Wait(CancellationToken token){Reserved.Set();Resume.Wait(token);}
    public void Dispose(){Reserved.Dispose();Resume.Dispose();}
}
internal sealed class LegacyGlobalWordTextWorkBudget
{
    internal int Rows{get;private set;}
    internal int Ranges{get;private set;}
    internal int Utf8Bytes=>Rows*4;
    internal void AddSource(int existingRows,int compactRows,int sourceReferences,int physicalInputs)
    {
        if(existingRows<0||compactRows<0||sourceReferences<0||physicalInputs is<1 or>256)
            throw new ArgumentOutOfRangeException(nameof(existingRows));
        long addedRows=(long)existingRows+compactRows,addedRanges=(long)sourceReferences*physicalInputs;
        if(addedRows>65536-Rows||addedRanges>1_048_576-Ranges)
            throw new NotSupportedException("Whole WORD row/projected-range budgets exceeded before claim.");
        Rows+=(int)addedRows;Ranges+=(int)addedRanges;
    }
}
public readonly record struct LegacyGlobalWordTextPrefix(string Stage,int WordNodes,int CharacterNodes,
    uint? WordFlags,uint? ChoiceFlags,int? ChoiceCount,int? CharacterLinks,long? WordSelectionId,long NextCounter,int Lines,int WrapperOffset,int DocumentAlternatives);

/// <summary>Bounded native-order global1/oneWORD parent -> actual CHAR ->
/// WORD -> source lines -> NULL-projected TEXT. No old configured owner,
/// postcoverage split/238B0 or document/DOM adoption is manufactured.</summary>
public sealed partial class LegacyFreshMainGlobalWordTextPublication
{
    private static readonly ConditionalWeakTable<LegacyFreshMainGlobalTextSourceWrapper,LegacyFreshMainGlobalWordTextPublication> owners=new();
    private readonly object gate=new();
    private readonly List<string> trace=[];
    private readonly List<LegacyGlobalWordTextPrefix> prefixes=[];
    private readonly LegacyNativeSourceLineInput[] lineInputs;
    private readonly LegacyNativeSourceLineGuide?[] guides;
    private readonly byte[][] descriptorBytes,guideBytes;
    private readonly Dictionary<ulong,LegacyNativeGlyphRecordNode> ownerSnapshots=[];
    private readonly Dictionary<ulong,int>? allWordReferenceCounts,allWordRefreshCandidates;
    private byte[] wordStorage;
    private readonly byte[] compactStorage;
    private ulong[] wordReferences;
    private LegacyGlobalCompletedWordSpan? allWords;
    private readonly LegacyGlobalCompletedWordSpan[]? batchSpans;
    private readonly LegacyFreshMainGlobalSourceBatchCompletion.CompletedTextWords? batchWords;
    private readonly List<LegacyFreshMainGlobalWordSourcePayload> publishedWords=[];
    private readonly List<int> publishedSelectedIndices=[];
    private int batchWordOrdinal=-1;
    internal bool IsLaterBatchWord=>batchWords is not null&&batchWordOrdinal>0;
    private int selectedWordIndex;
    private readonly int activeCompactCount;
    private readonly LegacyNativeInkPositionRange[] textKey;
    internal LegacyOwnedRawInkRangeOperations RangeOperations{get;}
    private LegacyNativePublishedSourceLine[] lines=[];
    private LegacyFreshMainGlobalWordSourcePayload? payload;
    private LegacyFreshMainGlobalTextSelectionReceipt? text;
    private LegacyFreshMainGlobalTextSourceWord? characters;
    private bool entered,faulted,completed,documentLink,documentChoiceAllocated;
    private string stage="prepared";
    private int nextCharacter=-1;
    public LegacyFreshMainGlobalTextSourceWrapper Wrapper{get;}
    public LegacyLibraryGlobalTextPublicationScope Scope=>Wrapper.Scope;
    private LegacyFreshMainGlobalWordSourceReceipt originalWordSource;
    public LegacyFreshMainGlobalWordSourceReceipt OriginalWordSource=>completed&&publishedWords.Count>1?throw new NotSupportedException("Use the completed batch WORD source vector."):originalWordSource;
    public LegacyFreshMainGlobalWordSourcePayload WordSource=>completed&&publishedWords.Count>1?throw new NotSupportedException("Use the completed batch WORD source vector."):payload??throw new InvalidOperationException("WORD source has not been interned.");
    public IReadOnlyList<LegacyFreshMainGlobalWordSourcePayload> CopyPublishedWordSources(){Scope.Validate();lock(gate)return Array.AsReadOnly(publishedWords.ToArray());}
    public LegacyFreshMainGlobalTextSelectionReceipt Text=>text??throw new InvalidOperationException("TEXT publication has not been entered.");
    public int SelectedWordIndex=>completed&&publishedWords.Count>1?throw new NotSupportedException("Use the actual batch WORD selected-index vector."):selectedWordIndex;
    public int[] CopyPublishedWordSelectedIndices(){Scope.Validate();lock(gate)return publishedSelectedIndices.ToArray();}
    // Current selected TOP279 route;275 payload alternatives remain distinct.
    public int AlternativeCount=>Wrapper.ActualCompletion.Snapshot.ActiveAlternativeCount60;
    public int ActualWordPayloadCount=>batchWords is not null&&batchWords.Count>1?throw new NotSupportedException("Actual275 payload counts belong to individual batch WORD owners."):Wrapper.ActualWordPayloadCount;
    public bool PublishesAllWordChoices=>allWords is not null;
    public long AllWordNativeChoiceAndLinkLayoutBytes=>batchSpans?.Sum(s=>s.NativeChoiceAndLinkLayoutBytes)??allWords?.NativeChoiceAndLinkLayoutBytes??0;
    public long AllWordReferenceArrayCopyBytes=>batchSpans?.Sum(s=>s.ReferenceArrayCopyBytes)??allWords?.ReferenceArrayCopyBytes??0;
    public byte[][] CopyActualWordPayloads120(CancellationToken token=default)
    {if(batchWords is not null&&batchWords.Count>1){Scope.Validate();throw new NotSupportedException("Actual275 payload frames belong to individual batch WORD owners.");}return Wrapper.CopyActualWordPayloads120(token);}
    public bool IsFaulted{get{lock(gate)return faulted;}}
    public bool IsComplete{get{lock(gate)return completed;}}
    public int LineCount{get{lock(gate)return lines.Length;}}
    public int WrapperLineOffset{get;private set;}
    //Operational ceiling for these explicitly enumerated retained byte/typed arrays;
    //not total managed allocation, object metadata, BCL growth, RSS or native ABI policy.
    private const long MaximumAllChoiceEnumeratedArrays=128L*1024*1024;
    public long AllWordEnumeratedArrayEnvelopeBytes{get;private set;}
    public int PreflightMaximumAlternativeRows{get;}
    public int PreflightMaximumProjectedRanges{get;}
    private LegacyFreshMainGlobalWordTextPublication(LegacyFreshMainGlobalTextSourceWrapper wrapper,CancellationToken token,bool publishAll=false)
    {
        Wrapper=wrapper;wrapper.ValidatePreclaim(token);var completion=wrapper.ActualCompletion;
        batchWords=completion.BatchWords;selectedWordIndex=completion.Word.WordIndex;
        if(completion.Snapshot.ActiveAlternativeCount60!=1||(batchWords is null&&(completion.Word.Next is not null||selectedWordIndex!=0&&!completion.HasActualRankedSingletonFacade))||
            !ReferenceEquals(completion.Snapshot.WordHead18,completion.Word)||
            !ReferenceEquals(Scope.Seed,Scope.Context.Result.PublicationSeed)||
            !ReferenceEquals(Scope.Seed.PendingSelections,Scope.SharedState.Selections)||
            !ReferenceEquals(Scope.Seed.StringPool,wrapper.Choices.StringPool)||Scope.SharedState.Words.Count!=0)
            throw new NotSupportedException("Exact fresh one-alternative/one-choice WORD and original shared Seed required.");
        if(batchWords is not null)
        {
            if(!publishAll)throw new NotSupportedException("A completed source batch requires all275 publication per actual WORD.");
            batchWords.ReservePreparationCopy(checked(12L*batchWords.Count),token);
            publishedSourceSelectedIndices=new int[batchWords.Count];
            publishedOriginalSources=new LegacyFreshMainGlobalWordSourceReceipt[batchWords.Count];
            batchSpans=new LegacyGlobalCompletedWordSpan[batchWords.Count];
            for(int i=0;i<batchSpans.Length;i++){batchSpans[i]=LegacyGlobalCompletedWordSpan.CaptureBatch(completion,i,token);batchSpans[i].Bind(this);}
            allWords=batchSpans[0];
        }
        else{allWords=publishAll?LegacyGlobalCompletedWordSpan.Capture(completion,token):null;allWords?.Bind(this);}
        if(allWords is not null){allWordReferenceCounts=new();allWordRefreshCandidates=new();}
        wordStorage=completion.Word.Word.CopyStorage();wordReferences=completion.Word.Word.FlatReferences.ToArray();
        originalWordSource=new(this);
        var selected=Scope.Document.CopySelectedInputs();var ops=LegacyOwnedRawInkRangeOperations.Prepare(Scope.Document);RangeOperations=ops;
        LegacyNativeInkPositionRange[] projected=[];
        for(int i=0;i<selected.Length;i++)
        {
            token.ThrowIfCancellationRequested();var input=selected[i];
            if(input.IsKnownText||input.SourceIndex!=(uint)i||input.PointCount!=Scope.Document.Strokes[i].Points.Count)
                throw new NotSupportedException("Actual complete original raw INPUT rows required for NULL projection.");
            var r=LegacyNativeTextSourceRangeProjection.Project(input);
            if(r.First.Mode!=4)projected=ops.Union(projected,[r],token);
        }
        textKey=projected;
        var cuts=completion.Snapshot.SourcePolicy.Owner.Graph.Source.Source;
        var actualLines=cuts.Preparation.Input.Input.Input;
        if(actualLines.Owner.Controller.SourceOffset!=0||cuts.Steps.Count!=1||cuts.Steps[0].Kind!=0)
            throw new NotSupportedException("Actual fresh main0 ordinary descriptor/guide owner required.");
        descriptorBytes=cuts.CopyDescriptors();guideBytes=cuts.CopyGuides();
        if(!Same(descriptorBytes,actualLines.CopyDescriptors())||!Same(guideBytes,actualLines.CopyGuides())||
            descriptorBytes.Length is <1 or>4096||guideBytes.Length>4096)
            throw new ArgumentException("Exact retained global source descriptor and guide lineage required.");
        lineInputs=new LegacyNativeSourceLineInput[descriptorBytes.Length];
        int maximum=0;
        for(int i=0;i<lineInputs.Length;i++)
        {
            token.ThrowIfCancellationRequested();byte[] d=descriptorBytes[i];if(d.Length!=88)throw new InvalidDataException("Actual descriptor88 required.");
            int line=BinaryPrimitives.ReadInt32LittleEndian(d.AsSpan(16));lineInputs[i]=new(d[0],line);
            if((d[0]&15)!=3&&line>=0){if(line>=4096)throw new NotSupportedException("Bounded assigned source line required.");maximum=Math.Max(maximum,line+1);}
        }
        if(maximum<1||maximum>guideBytes.Length)throw new NotSupportedException("Actual assigned line must resolve its guide.");
        guides=new LegacyNativeSourceLineGuide?[guideBytes.Length];
        for(int i=0;i<guides.Length;i++)
        {
            token.ThrowIfCancellationRequested();byte[] g=guideBytes[i];if(g.Length!=40)throw new InvalidDataException("Actual guide40 required.");
            var guide=new LegacyNativeSourceLineGuide(F(g,8),F(g,12),F(g,16),F(g,20),F(g,24));
            _=LegacyNativeSourceLinePublication.ReadPreparedLineInfo(guide,0);guides[i]=guide;
        }
        //No normalizer can expand a lone scalar without a marker pair. The
        //pinned ordinary AL and compact appends each emit one primary token.
        var alphabet=wrapper.Choices.Emitter.PrimaryAlphabet;
        for(int i=0;i<alphabet.LabelCount;i++)
        {
            token.ThrowIfCancellationRequested();
            //Missing in-range labels are rejected by the existing emitter;
            //they cannot contribute an unbounded fallback string.
            if(alphabet.TryGetText((ushort)i,out string value))ProveScalarBytes(Encoding.UTF8.GetBytes(value),wrapper.Choices.Normalization,token);
        }
        foreach(int cp in new[]{0xa0,0x20,0xa,0xf0000,0xf0010,0xf0011,0xf0012,0xf0013,0xf0014,0xf0015,0xf0016,0xf0017,0xf0018,0xfffd})
            ProveScalarBytes(Encoding.UTF8.GetBytes(new Rune(cp).ToString()),wrapper.Choices.Normalization,token);
        byte[] compact=completion.Source.Selection.Publication.CopyCompactStorage();int[] starts=completion.Source.Selection.Publication.CopyNodeStarts2478();
        int compactCount=completion.Source.Selection.Publication.RecordCount2470;var budget=new LegacyGlobalWordTextWorkBudget();
        compactStorage=compact;activeCompactCount=compactCount;
        if(compactCount<1||compactCount>compact.Length/40)throw new InvalidDataException("Actual active compact extent required.");
        var pool=completion.Source.Session!.Ownership.GlyphPool;
        foreach(var entry in EnumerateAllReferences(token))
        {
            ulong reference=entry.Reference;
            if(unchecked((short)(reference>>24))<0)throw new NotSupportedException("Every batch WORD choice requires ordinary nonnegative source references before claim.");
            token.ThrowIfCancellationRequested();ulong ownerKey=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
            if(allWords is not null)allWordReferenceCounts![ownerKey]=allWordReferenceCounts.GetValueOrDefault(ownerKey)+1;
            if(!ownerSnapshots.TryGetValue(ownerKey,out var owner)){owner=pool.CopyOwnerNode(ownerKey,token);ownerSnapshots.Add(ownerKey,owner);}
            int ordinal=unchecked((short)(reference>>40));if((uint)ordinal>=(uint)(owner.Records.Length/40))throw new NotSupportedException("Actual signed global WORD marker required.");
            var selectedRow=owner.Records.Span.Slice(ordinal*40,40);
            ushort kind=BinaryPrimitives.ReadUInt16LittleEndian(selectedRow[4..]),run=BinaryPrimitives.ReadUInt16LittleEndian(selectedRow[8..]);
            if((kind&7)==1&&(run&0x7ffc)!=0&&(run&3)==3)continue;
            for(int i=0;i<owner.Records.Length;i+=40)
                if((BinaryPrimitives.ReadUInt16LittleEndian(owner.Records.Span.Slice(i+4))&7)!=0)
                    throw new NotSupportedException("The bounded parent admits ordinary scalar alternatives only.");
            var higher=batchWords is null?default:ReserveHigherMetadata(entry.Span!,entry.Word,entry.Offset,reference,token);
            var source=wrapper.Choices.Refresh.SourceMetadata.Read(ownerKey,token);
            if(allWords is not null&&source.IsHigherSource)
            {
                if(batchWords is null)throw new NotSupportedException("All275 publication currently requires ordinary base-source glyph owners.");
                RequireHigherMetadata(higher,source,token);
            }
            if(batchWords is null)budget.AddSource(owner.Records.Length/40,compactCount,source.ReferenceCount,Scope.Document.ObjectCount);
            int matchingCompact=0;
            for(int row=starts[source.BaseNode];row<starts[source.BaseNode+1];row++)
            {
                token.ThrowIfCancellationRequested();var data=compact.AsSpan(row*40,40);byte traits=data[32];
                if((traits&0x20)==0||(traits&7)!=0||BinaryPrimitives.ReadUInt64LittleEndian(data[8..])!=source.BaseSourceKey)continue;
                matchingCompact++;
                ushort symbol=BinaryPrimitives.ReadUInt16LittleEndian(data[28..]);
                var resolved=completion.Source.Session!.OrdinaryProducer.ResolveForRefresh(source.BaseNode,source.BaseArc,symbol,token);
                if(resolved.ExternalVariant!=0||resolved.EmittedSymbol is>=0xfff9 and<=0xfffc)
                    throw new NotSupportedException("Nonordinary compact replacement/control variants require a separate parent budget contract.");
            }
            //The new batch uses the exact same three predicates as the
            //refresh kernel, over its same private compact/source interval.
            //Only those rows can append; unrelated source rows cannot emit
            //alternatives here. Standalone's historical bound stays intact.
            if(batchWords is not null)budget.AddSource(owner.Records.Length/40,matchingCompact,source.ReferenceCount,Scope.Document.ObjectCount);
            if(allWords is not null)
            {
                if(allWordRefreshCandidates!.TryGetValue(ownerKey,out int prior)&&prior!=matchingCompact)throw new InvalidDataException("Same actual source owner must retain its compact candidate extent.");
                allWordRefreshCandidates![ownerKey]=matchingCompact;
            }
        }
        PreflightMaximumAlternativeRows=budget.Rows;PreflightMaximumProjectedRanges=budget.Ranges;
        //upper<=65536, <=4 bytes/scalar, <=4096 CHAR links, one WORD48,
        //one document32+WORD24, <=4096 lines and <=256 NULL INPUT rows.
        token.ThrowIfCancellationRequested();
    }
    public static LegacyFreshMainGlobalWordTextPublication Prepare(LegacyFreshMainGlobalTextSourceWrapper actual,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actual);actual.ValidatePreclaim(token);
        var value=owners.GetValue(actual,w=>new(w,token));if(value.allWords is not null)throw new InvalidOperationException("Wrapper already reserved for all275 WORD publication.");value.ValidateUnused(token);return value;
    }
    /// <summary>Opt-in all actual275 WORD choices under the same one TOP/source transaction.
    /// The current selected279 ordinal is preserved; no argmax or caller payload ingress.</summary>
    public static LegacyFreshMainGlobalWordTextPublication PrepareAllWords(LegacyFreshMainGlobalTextSourceWrapper actual,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actual);actual.ValidatePreclaim(token);
        var value=owners.GetValue(actual,w=>new(w,token,true));
        if(value.allWords is null)throw new InvalidOperationException("Wrapper already prepared for selected-only WORD publication.");
        value.ValidateUnused(token);return value;
    }
    internal void RequireSpanCursor(LegacyFreshMainGlobalTextSourceWrapper wrapper,LegacyGlobalCompletedWordSpan.Cursor cursor,CancellationToken token)
    {
        ValidateLive(token);
        if(allWords is null||!ReferenceEquals(wrapper,Wrapper)||!ReferenceEquals(cursor.Owner,allWords))throw new ArgumentException("Private same-parent actual275 cursor required.");
        allWords.RequireCursor(this,cursor,token);
    }
    private void ValidateUnused(CancellationToken token)
    {
        Scope.Validate();token.ThrowIfCancellationRequested();Wrapper.ValidatePreclaim(token);allWords?.Require(token);
        if(entered||Scope.SharedState.Words.Count!=0)throw new InvalidOperationException("Unused empty global WORD parent required.");
        var c=Wrapper.ActualCompletion;
        if(c.Word.WordIndex!=selectedWordIndex||(batchWords is null&&c.Word.Next is not null)||!ReferenceEquals(c.Snapshot.WordHead18,c.Word)||
            !c.Word.Word.CopyStorage().AsSpan().SequenceEqual(wordStorage)||!c.Word.Word.FlatReferences.SequenceEqual(wordReferences)||
            c.Source.Selection.Publication.RecordCount2470!=activeCompactCount||!c.Source.Selection.Publication.CopyCompactStorage().AsSpan().SequenceEqual(compactStorage))
            throw new InvalidOperationException("Actual WORD/compact source changed after whole-stage preflight.");
        foreach(var before in ownerSnapshots.Values)
        {
            token.ThrowIfCancellationRequested();var current=c.Source.Session!.Ownership.GlyphPool.CopyOwnerNode(before.Key,token);
            if(current.PackedCount!=before.PackedCount||!current.Records.Span.SequenceEqual(before.Records.Span))
                throw new InvalidOperationException("Actual glyph owner changed after whole-stage preflight.");
        }
    }
    internal void RequireWordIntern(LegacyFreshMainGlobalWordSourceReceipt actual,CancellationToken token)
    {ValidateLive(token);if(stage!="word-intern"||!ReferenceEquals(actual,OriginalWordSource)||payload is not null)throw new ArgumentException("Current actual global WORD source receipt required.");}
    internal void RequireCharacterEntry(LegacyFreshMainGlobalTextSourceWrapper actual,CancellationToken token)
    {ValidateLive(token);if(stage!="characters"||!ReferenceEquals(actual,Wrapper)||payload is null||!IsCurrentPayloadSource())throw new ArgumentException("Initialized actual WORD parent required before CHAR entry.");}
    internal void VisitPositiveSource(LegacyFreshMainGlobalTextSourceWrapper actual,int index,CancellationToken token)
    {RequireCharacterEntry(actual,token);payload!.Flags&=~0x200000u;Record("positive:"+index);}
    internal void AppendCharacter(LegacyFreshMainGlobalTextSourceWrapper actual,LegacyFreshMainGlobalTextSourceChoice choice,CancellationToken token)
    {
        RequireCharacterEntry(actual,token);int index=choice.ProcessedSource.WordIndex;
        if(index<=nextCharacter)throw new InvalidOperationException("Native CHAR link order changed.");
        payload!.Append(choice,token);nextCharacter=index;Record("character:"+index);
    }
    internal void RequireSelectionOwner(LegacyOwnedFreshTextSourceContext actual,CancellationToken token)
    {ValidateLive(token);if(!ReferenceEquals(Scope.SharedState.Selections,actual))throw new ArgumentException("Exact pending global transaction required.");}
    internal void RequireWordRegistration(LegacyFreshMainGlobalWordSourcePayload actual,CancellationToken token)
    {ValidateLive(token);if(stage!="word-register"||!ReferenceEquals(actual,payload)||characters is null||!ReferenceEquals(actual.Node.FirstGlobalWordSource,OriginalWordSource))throw new ArgumentException("Completed actual CHAR/WORD source prefix required.");}
    internal void RequireTextRegistration(LegacyFreshMainGlobalTextSelectionReceipt actual,CancellationToken token)
    {ValidateLive(token);if(stage!="text-register"||!documentLink||WrapperLineOffset==0||!ReferenceEquals(actual,text))throw new ArgumentException("Actual WORD/source-lines prefix required before TEXT.");}
    private void ValidateLive(CancellationToken token)
    {Scope.Validate();token.ThrowIfCancellationRequested();if(!entered||faulted||completed)throw new InvalidOperationException("Current entered successful global WORD publication required.");allWords?.Require(token);batchWords?.Require(token);}
    public LegacyFreshMainGlobalWordTextPublication Publish(ReadOnlySpan<byte> allocator40,ReadOnlySpan<byte> selection4,CancellationToken token=default)
        =>PublishCore(allocator40,selection4,default,token);
    internal LegacyFreshMainGlobalWordTextPublication PublishPrepared(ReadOnlySpan<byte> allocator40,ReadOnlySpan<byte> selection4,LegacyGlobalWordTextPreparedFault fault,CancellationToken token=default)
    {
        if(fault.Character is<0 or>4||fault.Word is<0 or>4||fault.Text is<0 or>4||
            (fault.Character!=0?1:0)+(fault.Word!=0?1:0)+(fault.Text!=0?1:0)+(fault.LineAllocation?1:0)>1)throw new ArgumentException("One separately labelled native failure control required.");
        return PublishCore(allocator40,selection4,fault,token);
    }
    internal LegacyFreshMainGlobalWordTextPublication PublishPreparedReservationBoundary(ReadOnlySpan<byte> allocator40,ReadOnlySpan<byte> selection4,
        LegacyGlobalWordTextReservationBarrier barrier,CancellationToken token)
        =>PublishCore(allocator40,selection4,default,token,barrier);
    private LegacyFreshMainGlobalWordTextPublication PublishCore(ReadOnlySpan<byte> allocator40,ReadOnlySpan<byte> selection4,LegacyGlobalWordTextPreparedFault fault,CancellationToken token,
        LegacyGlobalWordTextReservationBarrier? barrier=null)
    {
        lock(gate)
        {
            ValidateUnused(token);if(allocator40.Length!=40||selection4.Length%4!=0||selection4.Length>65536*4)throw new ArgumentException("Complete allocator40/selection4 storage required.");
            if(allWords is not null)
            {
                //Select retains actual initial owner rows plus every possible same-owner refresh append, bounded by pool2046.
                //One retained ordinary range union per WORD <=sum source range terms; use R*P conservatively.
                long r=batchSpans is null?allWords.TotalReferences:batchSpans.Sum(s=>(long)s.TotalReferences);
                long ownerClones=0;
                foreach(var pair in allWordReferenceCounts!)
                {
                    long uses=pair.Value,initialRows=ownerSnapshots[pair.Key].Records.Length/40;
                    long upperRows=Math.Min(2046,checked(initialRows+uses*allWordRefreshCandidates!.GetValueOrDefault(pair.Key)));
                    ownerClones=checked(ownerClones+40L*uses*upperRows);
                }
                long known=checked(ownerClones+r*selection4.Length+16L*r*PreflightMaximumProjectedRanges+
                    8L*r+8L*(batchSpans is null?allWords.Count:batchSpans.Sum(s=>(long)s.Count))+compactStorage.Length+ownerSnapshots.Values.Sum(n=>(long)n.Records.Length));
                if(known>MaximumAllChoiceEnumeratedArrays)throw new NotSupportedException("All275 enumerated retained-array envelope exceeds128MiB before claim.");
                AllWordEnumeratedArrayEnvelopeBytes=known;
                if(batchWords is not null)batchWords.ReservePublication(checked(known+120L*batchWords.Count+8L*r),token);
            }
            Wrapper.ReserveForWord(this,token);entered=true;
            try
            {
                Record("reserved");barrier?.Wait(token);token.ThrowIfCancellationRequested();
                documentChoiceAllocated=true;Record("document-choice32");
                if(batchWords is null)
                {
                    PublishCurrentWord(allocator40,selection4,fault,token);
                    publishedWords.Add(payload!);publishedSelectedIndices.Add(selectedWordIndex);documentLink=true;Record("document-word24");
                }
                else
                {
                    batchWords.Begin(token);
                    for(batchWordOrdinal=0;batchWordOrdinal<batchSpans!.Length;batchWordOrdinal++)
                    {
                        var actual=batchWords.Open(token);allWords=batchSpans[batchWordOrdinal];
                        currentPayloadFirstOrdinal=actual.FirstPayloadOrdinal(token);currentReusePlan=actual.ReusePlan(token);
                        sourceSelectedWordIndex=actual.SelectedIndex(token);selectedWordIndex=sourceSelectedWordIndex;wordStorage=actual.ActualNode(token).Word.CopyStorage();
                        wordReferences=actual.ActualNode(token).Word.FlatReferences.ToArray();
                        originalWordSource=new(this);payload=null;characters=null;nextCharacter=-1;
                        PublishCurrentWord(allocator40,selection4,fault,token);
                        publishedWords.Add(payload!);publishedSelectedIndices.Add(selectedWordIndex);publishedSourceSelectedIndices![batchWordOrdinal]=sourceSelectedWordIndex;publishedOriginalSources![batchWordOrdinal]=OriginalWordSource;documentLink=true;Record("document-word24:"+batchWordOrdinal);
                        batchWords.Close(actual,token);
                    }
                    batchWords.Finish(token);
                }
                stage="source-lines";
                int append=0;foreach(var input in lineInputs)if((input.Flags&15)!=3&&input.SourceLineId>=0)append=Math.Max(append,input.SourceLineId+1);
                if(fault.LineAllocation){WrapperLineOffset=append;Record("source-line-allocation-refused");throw new NotSupportedException("Prepared native source-line allocation refusal retained wrapper offset update.");}
                var published=LegacyNativeSourceLinePublication.Append(lines,0,lineInputs,guides,0,token);lines=published.Lines.ToArray();WrapperLineOffset=published.NextWrapperLineOffset;Record("source-lines");
                text=new(this,textKey);stage="text-register";text.Selection=Scope.SharedState.Selections.CreateGlobalTextSelection(text,fault.Text,token);Record("text-registration-prefix");
                if(text.Selection is null)throw new NotSupportedException("TEXT registration refused with its native counter/pending prefix retained.");
#if BLURMASK_PUBLICATION_DIAGNOSTICS
                Record("text-registered");stage="complete";completed=true;
                // Only the already-owned singleton parent row; do not relabel a last batch row as its whole batch.
                var scoreTrace=Scope.DiagnosticScoreTrace;
                if(batchWords is null&&scoreTrace is not null&&scoreTrace.Admit(Wrapper.ActualCompletion.Source))
                    scoreTrace.CaptureSelected("ParentSelectedCompleted",selectedWordIndex,wordStorage,wordReferences,true);
                return this;
#else
                Record("text-registered");stage="complete";completed=true;return this;
#endif
            }
            catch{faulted=true;allWords?.Fault(this);if(batchWords is not null)batchWords.Fault();stage="fault";Wrapper.FaultReservedWord(this);Record("fault");throw;}
        }
    }
    private IEnumerable<(LegacyGlobalCompletedWordSpan? Span,int Word,int Offset,ulong Reference)> EnumerateAllReferences(CancellationToken token)
    {
        if(batchSpans is not null){foreach(var span in batchSpans)foreach(var entry in span.EnumerateIndexedReferences(token))yield return entry;}
        else if(allWords is not null){foreach(ulong reference in allWords.EnumerateReferences(token))yield return(null,-1,-1,reference);}
        else foreach(ulong reference in wordReferences)yield return(null,-1,-1,reference);
    }
    private void PublishCurrentWord(ReadOnlySpan<byte> allocator40,ReadOnlySpan<byte> selection4,LegacyGlobalWordTextPreparedFault fault,CancellationToken token)
    {
                stage="word-intern";var node=Scope.SharedState.Words.InternGlobalWord(OriginalWordSource,token);
                if(!node.Inserted){ReconcileCurrentWord(node.Node,allocator40,selection4,fault,token);return;}Record("word-intern");
                payload=new(OriginalWordSource,node.Node);Record("word-defaults");
                if(allWords is null)payload.ChoiceCount=1;else payload.InitializeChoices(allWords.Count,selectedWordIndex);
                Record("word-choice48");payload.Flags=(payload.Flags&~0x800000u)|0x200000;Record("word-initial-flags");
                stage="characters";
                if(allWords is null)
                {
                    characters=Wrapper.PublishIntoWord(this,allocator40,selection4,fault.Character,token);
                    if(characters.SelectedFlags!=payload.ChoiceFlags||(characters.SourceFlags&0x200000)!=(payload.Flags&0x200000)||
                        !LegacyOwnedRawInkRangeOperations.Equal(characters.CopyOrdinaryRanges(token),payload.CopyOrdinaryRanges()))throw new InvalidDataException("Typed WORD parent and actual CHAR output diverged.");
                }
                else
                {
                    for(int ordinal=0;ordinal<allWords.Count;ordinal++)
                    {
                        var cursor=allWords.Open(this,token);payload.BeginChoice(ordinal);nextCharacter=-1;
                        Wrapper.BeginSpanWord(this,cursor,token);Record("word-begin:"+ordinal);
                        var output=Wrapper.PublishIntoWord(this,allocator40,selection4,fault.Character,token);
                        if(output.SelectedFlags!=payload.CurrentChoiceFlags||
                            !LegacyOwnedRawInkRangeOperations.Equal(output.CopyOrdinaryRanges(token),payload.CopyCurrentOrdinaryRanges()))
                            throw new InvalidDataException("Actual275 WORD and its CHAR choice output diverged.");
                        payload.CompleteChoice();if(ordinal==selectedWordIndex)characters=output;
                        Wrapper.CloseSpanWord(this,cursor,token);allWords.Close(this,cursor,token);Record("word-complete:"+ordinal);
                    }
                    allWords.RequireComplete(this,token);
                }
                Record("characters-complete");payload.Flags|=payload.AllChoiceFlags;Record("word-final-flags");stage="word-register";
                if((payload.Flags&0x200000)!=0)payload.SelectionId=-1;
                else
                {
                    var selection=Scope.SharedState.Selections.CreateGlobalWordSelection(payload,fault.Word,token);
                    if(selection is not null){payload.Selection=selection;payload.SelectionId=selection.SelectionId;}
                    var key=OriginalWordSource.CopyOriginalKey();bool same=key.Length==1&&SameCoordinate(key[0]);payload.Flags=(payload.Flags&~0x800000u)|(same?0x800000u:0);
                    Record("word-registration-prefix");if(selection is null)throw new NotSupportedException("WORD registration refused with its native counter/default prefix retained.");
                }
                Record("word-registered");
    }
    public string[] CopyTrace(){Scope.Validate();lock(gate)return trace.ToArray();}
    public LegacyGlobalWordTextPrefix[] CopyPrefixes(){Scope.Validate();lock(gate)return prefixes.ToArray();}
    public IReadOnlyList<LegacyOwnedFreshTextSelection> CopyPendingSelections(){Scope.Validate();lock(gate)return Scope.SharedState.Selections.CopyPendingSelections();}
    public byte[][] CopyDescriptors(){Scope.Validate();return descriptorBytes.Select(d=>d.ToArray()).ToArray();}
    public byte[][] CopyGuides(){Scope.Validate();return guideBytes.Select(d=>d.ToArray()).ToArray();}
    public byte[] CopySourceLines56(){Scope.Validate();lock(gate)return LegacyFreshMainSourceLinePublication.Encode(lines);}
    public byte[] CopyDocumentWordLink24(){Scope.Validate();lock(gate){if(!documentLink)throw new InvalidOperationException("Document WORD link has not been appended.");if(publishedWords.Count!=1)throw new NotSupportedException("Use the complete actual WORD link vector.");return CopyDocumentWordLinks24();}}
    public byte[] CopyDocumentWordLinks24()
    {Scope.Validate();lock(gate){byte[] b=new byte[checked(24*publishedWords.Count)];for(int i=0;i<publishedWords.Count;i++)BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(24*i+16),publishedSelectedIndices[i]);return b;}}
    public byte[] CopyDocumentChoice32(){Scope.Validate();lock(gate){if(!documentChoiceAllocated)throw new InvalidOperationException("Document choice has not been allocated.");byte[] b=new byte[32];uint flags=0;for(int i=0;i<publishedWords.Count;i++)flags|=(allWords is null?publishedWords[i].ChoiceFlags:publishedWords[i].ChoiceAt(publishedSelectedIndices[i]).Flags)&1023;BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(8),flags);BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(24),publishedWords.Count);BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(28),publishedWords.Count);return b;}}
    public bool HasDocumentWordLink{get{lock(gate)return documentLink;}}
    public bool HasTextReceipt{get{lock(gate)return text is not null;}}
    public int DocumentAlternativeCount{get{lock(gate)return documentChoiceAllocated?1:0;}}
    public LegacyNativeInkPositionRange[] CopyNullProjectedTextKey(){Scope.Validate();return textKey.ToArray();}
    private static bool Same(byte[][] a,byte[][] b)=>a.Length==b.Length&&!a.Where((row,i)=>!row.AsSpan().SequenceEqual(b[i])).Any();
    private void Record(string value)
    {
        trace.Add(value);prefixes.Add(new(value,Scope.SharedState.Words.Count,Scope.SharedState.Characters.Count,
            payload?.Flags,payload?.ChoiceFlags,payload?.ChoiceCount,payload?.LinkCount,payload?.SelectionId,
            Scope.SharedState.Selections.NextSelectionId,lines.Length,WrapperLineOffset,documentChoiceAllocated?1:0));
    }
    private static float F(byte[] b,int at)=>BinaryPrimitives.ReadSingleLittleEndian(b.AsSpan(at));
    internal static bool SameCoordinate(LegacyNativeInkPositionRange r)=>r.First.SourceIndex==r.Last.SourceIndex&&r.First.PointIndex==r.Last.PointIndex&&(r.First.Fraction==r.Last.Fraction||r.First.Fraction==255||r.Last.Fraction==255);
    private static void ProveScalarBytes(byte[] raw,LegacyNativeUnicodeNormalizationData normalization,CancellationToken token)
    {
        if(raw.Length>4||Encoding.UTF8.GetString(raw).EnumerateRunes().Take(2).Count()>1)
            throw new NotSupportedException("Bounded single-scalar primary emission required.");
        byte[] scratch=LegacyNativeCanonicalScratch.PrepareUtf8Scratch(raw,normalization,token);
        int begin=scratch.AsSpan().IndexOf("\U000f0000"u8);
        if(begin>=0&&scratch.AsSpan(begin+4).IndexOf("\U000f0001"u8)>=0)
            throw new NotSupportedException("Canonical scratch could reach circled writeback; the scalar byte bound is not applicable.");
        //Complete returns ORIGINAL bytes when no marker pair can be replaced.
        //0213A0 may replace one2639/263A by a four-byte smiley;021000 only
        //classifies and interns that exact interval without normalization.
    }
}
