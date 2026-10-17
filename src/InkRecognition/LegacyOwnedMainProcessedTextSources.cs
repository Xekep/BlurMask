using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainTextSourceWrapper.Probe")]
[assembly: InternalsVisibleTo("LegacyMainConfiguredTextSourceWrapper.Probe")]

namespace InkRecognition;

/// <summary>One actual024F30 source key after clipping, endpoint downgrade and
/// later-glyph overlap ownership. It is not a caller-prepared positive flag.</summary>
public sealed class LegacyOwnedMainProcessedGlyphSource
{
    private readonly LegacyNativeInkPositionRange[] ranges;
    public LegacyOwnedMainProcessedTextWordSources Owner {get;}
    public LegacyOwnedMainGlyphDocumentPositions OriginalPositions {get;}
    public int WordIndex {get;}
    public ulong GlyphReference {get;}
    public bool IsOrdinarySource {get;}
    public int SourceLineId {get;}
    public int RangeCount=>ranges.Length;
    internal LegacyOwnedMainProcessedGlyphSource(LegacyOwnedMainProcessedTextWordSources owner,
        LegacyOwnedMainGlyphDocumentPositions original,int index,ulong reference,bool ordinary,int line,
        LegacyNativeInkPositionRange[] ranges)
    {Owner=owner;OriginalPositions=original;WordIndex=index;GlyphReference=reference;IsOrdinarySource=ordinary;
     SourceLineId=line;this.ranges=ranges;}
    public LegacyNativeInkPositionRange[] CopyRanges()=>ranges.ToArray();
    public byte[] CopyRanges16()
    {
        byte[] bytes=new byte[ranges.Length*16];
        for(int i=0;i<ranges.Length;i++)
        {BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(i*16),ranges[i].First.Value);
         BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(i*16+8),ranges[i].Last.Value);}
        return bytes;
    }
}

/// <summary>Actual fresh single-TOP024F30 source-vector lifecycle. The native
/// glyph selection must succeed before the next source key is processed.
/// Existing document/cached/external source owners are not substituted.</summary>
public sealed class LegacyOwnedMainProcessedTextWordSources
{
    private readonly LegacyPreparedMainTextWordSources sources;
    private readonly LegacyNativeInkPositionRange[][] ranges;
    private readonly bool[] skipped,positive,specialJoiner;
    private readonly LegacyOwnedRawInkRangeOperations operations;
    private readonly LegacyNativeInkPositionRange[] originalRanges;
    private int next;
    private bool failed;
    public LegacyFreshMainTextGlyphChoices Choices {get;}
    public LegacyNativePublishedWordNode Word=>sources.Word;
    public LegacyOwnedMainSpanWordView? SpanWord=>sources.SpanWord;
    public LegacyOwnedRawInkDocument Document=>Choices.CandidateSource?.SourceDocument??Choices.Candidate.RawResult.Owner.SourceDocument;
    public int Count=>ranges.Length;
    public int NextIndex=>next;
    internal LegacyOwnedMainProcessedTextWordSources(LegacyFreshMainTextGlyphChoices choices,
        LegacyPreparedMainTextWordSources actualSources)
    {
        if(!ReferenceEquals(actualSources.Owner,choices)||!actualSources.WrapperFirstPass)
            throw new ArgumentException("Actual024F30 first-pass owner required.");
        Choices=choices;sources=actualSources;operations=LegacyOwnedRawInkRangeOperations.Prepare(Document);
        originalRanges=sources.CopyOriginalRanges();int count=sources.Record.FlatReferences.Count;
        ranges=new LegacyNativeInkPositionRange[count][];skipped=new bool[count];positive=new bool[count];specialJoiner=new bool[count];
        var alphabet=choices.CandidateSource?.SourcePolicy.Model.AcousticAlphabet??choices.Candidate.Snapshot.SourcePolicy.Model.AcousticAlphabet;
        bool hasJoiner=alphabet.TryGetLabel("\u200d",out ushort joiner);
        for(int i=0;i<count;i++)
        {
            var descriptor=sources.ReadDescriptor(i);skipped[i]=LegacyFreshMainTextGlyphChoices.IsWrapperSkipped(descriptor);
            positive[i]=unchecked((short)(sources.Record.FlatReferences[i]>>24))>=0;
            specialJoiner[i]=hasJoiner&&(descriptor.KindFlags&7)==0&&descriptor.SymbolOrRunLength==joiner;
            ranges[i]=sources.CopyInitialRanges(i);
        }
    }
    internal bool IsSkipped(int index)=>skipped[index];
    internal void AdvanceSkipped(int index)
    {Check(index);if(!skipped[index])throw new InvalidOperationException("Actual source is not native-skipped.");next++;}
    private void Check(int index)
    {if(failed||index!=next||(uint)index>=(uint)Count)throw new InvalidOperationException("Source processing is ordered, single-use, and retains failed prefixes.");}
    internal LegacyOwnedMainProcessedGlyphSource Process(LegacyPreparedMainTextGlyphSelection selected,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(selected);token.ThrowIfCancellationRequested();Check(selected.Index);
        if(!ReferenceEquals(selected.Owner,Choices)||!ReferenceEquals(selected.Sources,sources)||skipped[next])
            throw new ArgumentException("Successful selection from this same next actual WORD reference required.");
        int current=next;
        try
        {
            var result=LegacyPreparedMainTextSourceProcessing.Process(operations,originalRanges,ranges,
                skipped,positive,specialJoiner,current,token);
            bool ordinary=result.Ordinary;var key=result.Key;
            ranges[current]=key;next++;
            var sampled=Choices.CandidateSource?.Sampled??Choices.Candidate.RawResult.Sampled;
            if(sampled.Owner.SourceDescriptorOrdinal!=0)
                throw new NotSupportedException("Only actual single-descriptor raw group0 is supported.");
            int line=positive[current]?sampled.Owner.SourceLineId:-1;
            return new(this,selected.SourcePositions,current,selected.Reference,ordinary,line,key.ToArray());
        }
        catch{failed=true;throw;}
    }
}

//One numeric lifecycle kernel shared with independent prepared controls.
//Only the enclosing private-owner path can construct a processed source.
internal static class LegacyPreparedMainTextSourceProcessing
{
    internal static (bool Ordinary,LegacyNativeInkPositionRange[] Key) Process(
        LegacyOwnedRawInkRangeOperations operations,LegacyNativeInkPositionRange[] originalRanges,
        LegacyNativeInkPositionRange[][] ranges,bool[] skipped,bool[] positive,bool[] specialJoiner,
        int current,CancellationToken token)
    {
        bool ordinary=positive[current];var key=ranges[current];
        if(ordinary)
        {
            if(key.Length==1&&SameCoordinate(key[0])&&!LegacyOwnedRawInkRangeOperations.Contains(originalRanges,key[0]))
            {ordinary=false;key[0]=new(new((key[0].First.Value&~255UL)|4),new((key[0].Last.Value&~255UL)|4));}
            else if(specialJoiner[current])
            {
                if(key.Length==0)throw new NotSupportedException("Native joiner source has no position storage.");
                ulong end=key[0].Last.Value;if((byte)(end>>8)==255)end&=~0xff00UL;end=(end&~255UL)|4;
                key=[new(new(end),new(end))];ordinary=false;
            }
            else
            {
                for(int later=current+1;later<ranges.Length;later++)
                {
                    token.ThrowIfCancellationRequested();if(skipped[later]||!positive[later])continue;
                    var overlap=operations.Intersect(operations.Union([],ranges[later],token),key,token);
                    if(overlap.Length==0)continue;
                    if(!LegacyOwnedRawInkRangeOperations.Equal(key,overlap))key=operations.Subtract(key,overlap,token);
                    else ranges[later]=operations.Subtract(ranges[later],overlap,token);
                    break;
                }
            }
        }
        return(ordinary,key);
    }
    private static bool SameCoordinate(LegacyNativeInkPositionRange range)
        =>range.First.SourceIndex==range.Last.SourceIndex&&range.First.PointIndex==range.Last.PointIndex&&
          (range.First.Fraction==range.Last.Fraction||range.First.Fraction==255||range.Last.Fraction==255);
}
