using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Fresh024D40 source payload with portable sidecars for native
/// vectors/strings. Unwritten padding, pointers and allocator capacities are
/// not presented as recovered native bytes.</summary>
public sealed class LegacyOwnedMainTextSourcePayload
{
    private readonly LegacyPreparedMainTextGlyphReuse rows;
    public LegacyOwnedMainTextSourceNode SourceNode {get;}
    public LegacyOwnedMainProcessedGlyphSource FirstSource {get;}
    public LegacyOwnedFreshTextSelection? Selection {get;}
    public long SelectionId=>Selection?.SelectionId??-1;
    public uint Flags=>rows.AggregateFlags;
    public int SourceLineId=>FirstSource.SourceLineId;
    public int Field08=>-1;
    public ulong Field00=>0;
    public uint Field1C=>0;
    public long Field40=>-1;
    public int Field48=>-1;
    public byte Field64=>0;
    public int PriorRangeCount=>0;
    public int RecordCount=>rows.RecordCount;
    public IReadOnlyList<LegacyPreparedTextGlyphReuseRecord> CopyRecords()=>rows.CopyRecords();
    internal LegacyOwnedMainTextSourcePayload(LegacyOwnedMainTextSourceNode node,
        LegacyOwnedMainProcessedGlyphSource source,LegacyOwnedFreshTextSourceContext context,
        LegacyFreshMainTextGlyphChoices choices,LegacyNativeTextGlyphCallerResult published,
        LegacyNativeTextPublicationData properties,LegacyNativeUnicodeNormalizationData normalization,CancellationToken token)
    {
        if(!ReferenceEquals(node.FirstProcessedSource,source)||!ReferenceEquals(node.Owner.Document,context.Document)||
            !ReferenceEquals(source.Owner.Choices,choices))throw new ArgumentException("Exact fresh source-node/selection/publication ownership required.");
        SourceNode=node;FirstSource=source;
        //024D40 registers the CHAR selection before initializing other
        //metadata.021D10 has no cached-result vectors for a NEW raw C4A root.
        if(source.IsOrdinarySource)Selection=context.CreateCharacterSelection(source,token);
        var copy=new List<LegacyPreparedTextGlyphReuseRecord>();uint aggregate=0;
        foreach(var record in published.Records)
        {
            choices.StringPool.Validate(record.Text);aggregate|=record.Flags;
            var storage=new byte[40];BinaryPrimitives.WriteUInt32LittleEndian(storage.AsSpan(8),record.Flags);
            BinaryPrimitives.WriteUInt32LittleEndian(storage.AsSpan(32),record.ByteCount);
            copy.Add(new(storage,record.Text));
        }
        bool priorFlag=false;
        if(source.IsOrdinarySource)
        {
            var actual=source.OriginalPositions.OrdinarySource??throw new InvalidDataException("Positive source lost its actual24-byte reference owner.");
            //021D10 starts its out byte at1. Any visited fresh ordinary C4A
            //input clears it; an empty traversal leaves it1. Do not replace
            //that native empty case with a blanket fresh=false assumption.
            priorFlag=!actual.References.Any(r=>r.First.First<=r.Last.First&&r.First.Second<=r.Last.Second);
        }
        uint degenerate=source.IsOrdinarySource&&source.RangeCount==1&&SameCoordinate(source.CopyRanges()[0])?0x800000u:0;
        uint flags=(((priorFlag?1u:0u)<<24)^((source.IsOrdinarySource?0u:0x200000u)|(aggregate&0xce1fffffu)|degenerate))&0xfdffffffu;
        rows=new(choices.Emitter,choices.StringPool,properties,normalization,copy,flags);
    }
    internal LegacyPreparedTextGlyphReuseResult Reuse(LegacyPreparedMainTextGlyphSelection selected,CancellationToken token)
        =>rows.Apply(selected.GlyphOwner,selected.Selection.CopySelectionRows4(),
            LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(selected.Reference),unchecked((int)selected.Selection.SelectedMarker!.Value),token);
    private static bool SameCoordinate(LegacyNativeInkPositionRange r)
        =>r.First.SourceIndex==r.Last.SourceIndex&&r.First.PointIndex==r.Last.PointIndex&&
          (r.First.Fraction==r.Last.Fraction||r.First.Fraction==255||r.Last.Fraction==255);
}

public sealed class LegacyOwnedMainTextSourceChoice
{
    public ulong GlyphReference {get;}
    public LegacyOwnedMainProcessedGlyphSource ProcessedSource {get;}
    public LegacyOwnedMainTextSourcePayload Source {get;}
    public int SelectedMarker {get;}
    public bool ReusedSourceKey {get;}
    public LegacyPreparedGlyphAlternativeSelection InitialSelection {get;}
    internal LegacyOwnedMainTextSourceChoice(ulong reference,LegacyOwnedMainProcessedGlyphSource processed,
        LegacyOwnedMainTextSourcePayload source,int marker,bool reused,LegacyPreparedGlyphAlternativeSelection selection)
    {GlyphReference=reference;ProcessedSource=processed;Source=source;SelectedMarker=marker;ReusedSourceKey=reused;InitialSelection=selection;}
    public byte[] CopySelectedUtf8()
    {
        var rows=Source.CopyRecords();
        if((uint)SelectedMarker>=(uint)rows.Count||rows[SelectedMarker].Text is not{} text)
            throw new InvalidOperationException("Native selected marker has no available text row.");
        return text.ReadBytes();
    }
}

public sealed class LegacyOwnedMainTextSourceWord
{
    private readonly LegacyNativeInkPositionRange[] ranges;
    public LegacyNativePublishedWordNode Word {get;}
    public IReadOnlyList<LegacyOwnedMainTextSourceChoice> Glyphs {get;}
    public uint SourceFlags {get;}
    public uint SelectedFlags {get;}
    internal LegacyOwnedMainTextSourceWord(LegacyNativePublishedWordNode word,
        LegacyOwnedMainTextSourceChoice[] glyphs,LegacyNativeInkPositionRange[] ranges,uint sourceFlags,uint selectedFlags)
    {Word=word;Glyphs=Array.AsReadOnly(glyphs);this.ranges=ranges;SourceFlags=sourceFlags;SelectedFlags=selectedFlags;}
    public LegacyNativeInkPositionRange[] CopyOrdinarySourceRanges()=>ranges.ToArray();
}

/// <summary>Bounded first-WORD024F30 source-key/text projection after the
/// actual219580 source prefix: refresh, C21 processing,01B4E0 and new-key
/// 213A0/024D40 or existing-key021760 using the SAME string pool. This does
/// not complete273.V68 word augmentation/pruning or0283B0 acceptance.</summary>
public sealed class LegacyFreshMainTextSourceWrapper : IDisposable
{
    private readonly LegacyFreshMainTextGlyphChoices choices;
    private readonly LegacyNativeTextPublicationData properties;
    private readonly LegacyNativeUnicodeNormalizationData normalization;
    private readonly LegacyOwnedMainTextSourceInterner interner;
    private readonly LegacyOwnedFreshTextSourceContext selections;
    public LegacyOwnedMainWordSourceCollection WordSourceCollection {get;}
    private readonly Dictionary<int,LegacyOwnedMainTextSourcePayload> payloads=[];
    private bool disposed;
    public LegacyFreshMainCompletedFirstCandidate Candidate=>choices.Candidate;
    private LegacyFreshMainTextSourceWrapper(LegacyFreshMainTextGlyphChoices choices,
        LegacyNativeTextPublicationData properties,LegacyNativeUnicodeNormalizationData normalization,CancellationToken token)
    {
        this.choices=choices;this.properties=properties;this.normalization=normalization;
        interner=LegacyOwnedMainTextSourceInterner.Prepare(Candidate.RawResult.Owner.SourceDocument);
        selections=LegacyOwnedFreshTextSourceContext.Prepare(interner.Document);
        WordSourceCollection=LegacyOwnedMainWordSourceCollection.ExportBeforeWordAugmentation(Candidate,token);
    }
    public static LegacyFreshMainTextSourceWrapper Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainCompletedFirstCandidate candidate,LegacyCursiveSeparationTable table,
        LegacyNativeTextPublicationData properties,LegacyNativeUnicodeNormalizationData normalization,
        LegacyNativeCircledPayloadData circles,CancellationToken token=default)
        =>new(LegacyFreshMainTextGlyphChoices.Prepare(resource,candidate,table,properties,normalization,circles,token),properties,normalization,token);

    public IReadOnlyList<LegacyOwnedMainTextSourceWord> Publish(ReadOnlySpan<byte> recordAllocation40,
        ReadOnlySpan<byte> selectionAllocation4,CancellationToken token=default)
    {
        ObjectDisposedException.ThrowIf(disposed,this);choices.BeginPublication(recordAllocation40,selectionAllocation4,token);
        var words=new List<LegacyOwnedMainTextSourceWord>();var operations=LegacyOwnedRawInkRangeOperations.Prepare(interner.Document);
        int referenceCount=0;
        foreach(var projected in Candidate.RawWordsInNativeHeadOrder)
        {
            token.ThrowIfCancellationRequested();var word=projected.Publication.PublishedWord!;
            if(word.Word.FlatReferences.Count>4096-referenceCount)throw new NotSupportedException("Wrapper reference budget exceeded.");
            referenceCount+=word.Word.FlatReferences.Count;
            if(!ReferenceEquals(WordSourceCollection.Word,word))throw new ArgumentException("Actual single WORD collection identity changed.");
            var firstPass=choices.RefreshWordSourcesForWrapper(word,recordAllocation40,WordSourceCollection.CopyRanges(),token);
            var processed=new LegacyOwnedMainProcessedTextWordSources(choices,firstPass);
            uint sourceFlags=0x200000,selectedFlags=0;LegacyNativeInkPositionRange[] ordinaryRanges=[];
            for(int i=0;i<processed.Count;i++)
                if(!processed.IsSkipped(i)&&unchecked((short)(word.Word.FlatReferences[i]>>24))>=0)sourceFlags&=~0x200000u;
            var output=new List<LegacyOwnedMainTextSourceChoice>();
            for(int i=0;i<processed.Count;i++)
            {
                token.ThrowIfCancellationRequested();if(processed.IsSkipped(i)){processed.AdvanceSkipped(i);continue;}
                var selected=choices.PrepareGlyph(firstPass,i,recordAllocation40,selectionAllocation4,token);
                var source=processed.Process(selected,token);var key=interner.Intern(source,token);
                LegacyOwnedMainTextSourcePayload payload;int marker;
                if(key.Inserted)
                {
                    var fresh=choices.PublishFreshGlyph(selected,token);marker=fresh.AdjustedMarkerIndex;
                    payload=new(key.Node,source,selections,choices,fresh,properties,normalization,token);
                    payloads.Add(key.Node.Identity,payload);
                }
                else
                {
                    payload=payloads[key.Node.Identity];var reuse=payload.Reuse(selected,token);marker=reuse.AdjustedMarkerIndex;
                    if(!reuse.Succeeded)throw new NotSupportedException("Native021760 retained a failed publication prefix.");
                }
                if(source.IsOrdinarySource)ordinaryRanges=operations.Union(ordinaryRanges,payload.SourceNode.FirstProcessedSource!.CopyRanges(),token);
                var rows=payload.CopyRecords();
                if((uint)marker>=(uint)rows.Count)
                    throw new NotSupportedException("Native selected marker has no active row; no alternate choice is fabricated.");
                selectedFlags|=rows[marker].Flags;
                output.Add(new(selected.Reference,source,payload,marker,!key.Inserted,selected.Selection));
            }
            words.Add(new(word,output.ToArray(),ordinaryRanges,sourceFlags,selectedFlags));
        }
        token.ThrowIfCancellationRequested();return words.AsReadOnly();
    }
    public IReadOnlyList<LegacyOwnedFreshTextSelection> CopyPendingSelections()=>selections.CopyPendingSelections();
    public long NextSelectionId=>selections.NextSelectionId;
    public void Dispose(){if(disposed)return;disposed=true;choices.Dispose();}
}
