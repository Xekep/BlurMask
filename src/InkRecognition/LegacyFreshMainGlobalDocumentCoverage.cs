using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyGlobalDocumentCoverage.Probe")]
namespace InkRecognition;

/// <summary>Bounded actual global post-TEXT025E40/0248D0/01C740 result.
/// No configured-two-alternative owner, geometric238B0 or DOM is fabricated.</summary>
public sealed class LegacyFreshMainGlobalDocumentCoverage
{
    private readonly LegacyNativeInkPositionRange[] word,text,initial,remaining;
    private readonly LegacyFreshMainGlobalWordCharacterLink[] links;
    private readonly LegacyNativeInkPositionRange[][] keys;
    private readonly LegacyOwnedFreshTextSelection[] commands;
    private readonly byte[] lineStorage;
    public LegacyFreshMainGlobalWordTextPublication Publication {get;}
    public int SelectedAlternativeIndex=>0;
    public bool UsesOrdinaryWordRanges {get;}
    public int LineCount=>lineStorage.Length/56;
    public int LastReferencedLine {get;}
    public int WordVisits=>1;
    public int LineHelperCalls=>1;
    public int AdjacentCharacterChecks=>links.Length-1;
    public int CharacterSubtractions=>links.Length;
    private LegacyFreshMainGlobalDocumentCoverage(LegacyFreshMainGlobalWordTextPublication publication,
        LegacyNativeInkPositionRange[] word,LegacyNativeInkPositionRange[] text,
        LegacyNativeInkPositionRange[] initial,LegacyNativeInkPositionRange[] remaining,
        LegacyFreshMainGlobalWordCharacterLink[] links,LegacyNativeInkPositionRange[][] keys,
        LegacyOwnedFreshTextSelection[] commands,byte[] lines,bool ordinary,int last)
    {Publication=publication;this.word=word;this.text=text;this.initial=initial;this.remaining=remaining;
     this.links=links;this.keys=keys;this.commands=commands;lineStorage=lines;
     UsesOrdinaryWordRanges=ordinary;LastReferencedLine=last;}
    public static LegacyFreshMainGlobalDocumentCoverage Prepare(LegacyFreshMainGlobalWordTextPublication actual,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actual);token.ThrowIfCancellationRequested();actual.Scope.Validate();
        if(!actual.IsComplete||actual.IsFaulted||actual.Wrapper.IsFaulted||actual.Wrapper.Choices.IsFaulted||
            actual.AlternativeCount!=1||actual.SelectedWordIndex!=0||!actual.HasDocumentWordLink||
            !actual.HasTextReceipt||actual.Text.Selection is null)
            throw new ArgumentException("Exact successfully completed actual global WORD/TEXT owner required.");
        var payload=actual.WordSource;
        if(payload.ChoiceCount!=1||(payload.Flags&0x201000)!=0||payload.Selection is null||
            !ReferenceEquals(payload.OriginalSource,actual.OriginalWordSource)||
            !ReferenceEquals(payload.Node.FirstGlobalWordSource,actual.OriginalWordSource))
            throw new NotSupportedException("Actual ordinary one-choice global WORD source required.");
        var pending=actual.CopyPendingSelections().ToArray();
        if(pending.Length<3||!ReferenceEquals(pending[^1],actual.Text.Selection)||
            !ReferenceEquals(pending[^2],payload.Selection)||pending[^1].Name!="TEXT"||pending[^2].Name!="WORD")
            throw new ArgumentException("Exact pending CHAR/WORD/TEXT prefix required.");
        var links=payload.CopyCharacterLinks().ToArray();
        if(links.Length is<1 or>4096)throw new NotSupportedException("Bounded nonempty actual WORD links required.");
        byte[][] descriptors=actual.CopyDescriptors();byte[] lines=actual.CopySourceLines56();
        if(descriptors.Length!=1||descriptors[0].Length!=88||(descriptors[0][0]&15)!=0||
            BinaryPrimitives.ReadInt32LittleEndian(descriptors[0].AsSpan(16))!=0||
            actual.LineCount!=1||lines.Length!=56||actual.WrapperLineOffset!=1)
            throw new NotSupportedException("Actual single assigned ordinary source line0 required.");
        int last=links[^1].Source.SourceLineId??-1;
        foreach(var link in links)
        {
            token.ThrowIfCancellationRequested();var source=link.Source;
            if(!source.IsComplete||!ReferenceEquals(source.Owner,actual.Wrapper)||
                !ReferenceEquals(source.Node.Owner,actual.Wrapper.CharacterInterner)||
                !ReferenceEquals(source.Node.FirstGlobalSource,source.Source)||source.SourceLineId!=last||last!=0)
                throw new NotSupportedException("Same actual complete CHAR sources/line required; split/insertion is excluded.");
        }
        //025E40's signed max+1 is1; no tail exists for this one actual ALT.
        //027670 sees a single WORD and returns before its gap-extension loop.
        var word=payload.OriginalSource.CopyOriginalKey();var text=actual.Text.CopyRanges();
        if(!LegacyOwnedRawInkRangeOperations.Equal(word,text))
            throw new NotSupportedException("Unequal actual WORD/TEXT coverage requires geometric238B0; containment is not equality.");
        var ordinary=payload.CopyOrdinaryRanges();bool useOrdinary=ordinary.Length!=0;
        var initial=useOrdinary?ordinary:word.ToArray();
        var keys=new LegacyNativeInkPositionRange[links.Length][];long comparisons=0,retained=initial.Length;
        for(int i=0;i<links.Length;i++)
        {
            token.ThrowIfCancellationRequested();var source=links[i].Source;
            if(source.PriorRangeCount!=0)throw new NotSupportedException("Fresh actual CHAR original key required; prior sources excluded.");
            keys[i]=source.Source.CopyRanges(token);retained+=keys[i].Length;
            //Conservative interval-pair envelope for repeated subtraction.
            comparisons+=(long)(initial.Length+retained)*keys[i].Length;
            if(retained>1_048_576||comparisons>16_777_216)
                throw new NotSupportedException("Whole postcoverage interval work budget exceeded before subtraction.");
        }
        var operations=LegacyOwnedRawInkRangeOperations.Prepare(actual.Scope.Document);
        var remaining=initial.ToArray();
        for(int i=0;i<keys.Length;i++){token.ThrowIfCancellationRequested();remaining=operations.Subtract(remaining,keys[i],token);}
        //Native keeps the sole WORD choice even if this remainder is nonempty.
        var result=new LegacyFreshMainGlobalDocumentCoverage(actual,word,text,initial,remaining,links,keys,pending,lines,useOrdinary,last);
        result.Validate(token);return result;
    }
    public void Validate(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();Publication.Scope.Validate();
        if(!Publication.IsComplete||Publication.IsFaulted||Publication.Wrapper.IsFaulted||
            !Publication.CopyPendingSelections().SequenceEqual(commands)||
            !Publication.WordSource.CopyCharacterLinks().SequenceEqual(links)||
            !Publication.CopySourceLines56().AsSpan().SequenceEqual(lineStorage))
            throw new InvalidOperationException("Actual global post-TEXT prefix changed.");
    }
    public LegacyNativeInkPositionRange[] CopyWordRanges(){Validate();return word.ToArray();}
    public LegacyNativeInkPositionRange[] CopyTextRanges(){Validate();return text.ToArray();}
    public LegacyNativeInkPositionRange[] CopyInitialCharacterCoverage(){Validate();return initial.ToArray();}
    public LegacyNativeInkPositionRange[] CopyRemainingCharacterCoverage(){Validate();return remaining.ToArray();}
    public LegacyNativeInkPositionRange[][] CopyCharacterKeys(){Validate();return keys.Select(k=>k.ToArray()).ToArray();}
    public byte[] CopySourceLines56(){Validate();return lineStorage.ToArray();}
}
