using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Fresh source-owned post025E40 state: live source metadata disables
///026090, the selected alternative has one WORD so027670 returns early, and
///0248D0 compares its native WORD-range union exactly with the TEXT key.
/// Uncovered-source238B0 processing and document acceptance are not supported.</summary>
public sealed class LegacyFreshMainDocumentSourceCoverage
{
    private readonly LegacyNativeInkPositionRange[] wordRanges,textRanges;
    public LegacyFreshMainDocumentLineConsistency LineConsistency {get;}
    public LegacyFreshMainDocumentTextSelection Text=>LineConsistency.Text;
    public LegacyOwnedRawInkDocument Document=>Text.Document;
    public int NativeSelectedAlternativeIndex=>0;
    /// <summary>Derived NEW2B3.10 ->12D0070 ->218.80 ->secondary14,
    /// read by actual context38.V80/169740 and0283B0; not a2DA ctor default.</summary>
    public bool SourceMetadataEnabled=>false;
    public int WordSourceNodeCount {get;}
    public LegacyOwnedMainWordSourceAlternative SelectedAlternative=>Text.WordChoices.Alternatives[NativeSelectedAlternativeIndex];
    public int LineCount=>LineConsistency.LineCount;
    private LegacyFreshMainDocumentSourceCoverage(LegacyFreshMainDocumentLineConsistency previous,
        LegacyNativeInkPositionRange[] words,LegacyNativeInkPositionRange[] text,int nodeCount)
    {LineConsistency=previous;wordRanges=words;textRanges=text;WordSourceNodeCount=nodeCount;}

    public static LegacyFreshMainDocumentSourceCoverage Prepare(
        LegacyFreshMainDocumentLineConsistency actualLines,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualLines);token.ThrowIfCancellationRequested();
        var text=actualLines.Text;var words=text.WordChoices;var candidate=words.Owner.Sources.CandidateSource;
        foreach(var word in words.Owner.WordViews)word.Validate(token);
        var commands=text.CopyPendingSelections();var prefix=words.PendingSelections;
        if(!text.IsRegistered||text.IsFaulted||commands.Count!=prefix.Count+1||
            !ReferenceEquals(commands[^1],text.Selection)||!commands.Take(prefix.Count).SequenceEqual(prefix)||
            actualLines.LineCount!=1||actualLines.LastReferencedLine!=0)
            throw new ArgumentException("Actual current post025E40 CHAR/WORD/TEXT state required.");
        //The actual immutable fresh main0 input excludes prior sources,
        //external property events and user guidelines.150D00 forwards its
        //NEW2B3.byte10 through12D0070, then1680D0 copies source80 tosecondary14.
        //FrontendV250/1530F0 returns that source; context38.V80/169740 returns
        //its secondary.0283B0 copies secondary14 to22A:026090 is disabled even
        //when input-result count/cache would otherwise make it applicable.
        if(candidate.Sampled.Owner.SourceDescriptorOrdinal!=0||
            candidate.Preprocessed.Lines.Owner.Controller.SourceOffset!=0||
            !ReferenceEquals(candidate.Preprocessed.Lines,candidate.Sampled.Owner.Preparation.Input.Input.Input)||
            !ReferenceEquals(candidate.SourceDocument,text.Document))
            throw new ArgumentException("Identical fresh main0 source/document/property lineage required.");
        if(words.Alternatives.Count!=2)
            throw new NotSupportedException("Actual two-alternative one-WORD source scope required.");
        //0283B0 stores a zero QWORD atB8 before output construction: selected
        //document alternativeBC is0. Each sealed alternative has one24B WORD
        //link.027670 sees wordCount1, then returns before gap-extension logic.
        var selected=words.Alternatives[0];selected.Candidate.Validate(token);
        if(BinaryPrimitives.ReadInt32LittleEndian(selected.CopyScalarStorage32().AsSpan(24))!=1||selected.Source.Choices.Count==0)
            throw new InvalidDataException("Actual selected WORD source missing.");
        var nodes=new List<LegacyOwnedMainWordSourcePayload>();
        foreach(var alternative in words.Alternatives)
        {
            token.ThrowIfCancellationRequested();alternative.Candidate.Validate(token);
            if(!ReferenceEquals(alternative.Source,words.Source)||
                !ReferenceEquals(alternative.Candidate.TopOwner,words.Owner.TopOwner))
                throw new ArgumentException("Actual shared source/TOP alternatives required.");
            if(!nodes.Contains(alternative.Source))nodes.Add(alternative.Source);
        }
        if(nodes.Count!=1)
            throw new NotSupportedException("Fresh single interned WORD source tree required.");
        //0248D0 traverses WORD source nodes, unions their ORIGINAL key38/40,
        //then C21.VE8/081700 compares count and every full16B range exactly.
        var operations=LegacyOwnedRawInkRangeOperations.Prepare(text.Document);
        LegacyNativeInkPositionRange[] union=[];
        foreach(var node in nodes)union=operations.Union(union,node.CopySourceKeyRanges(),token);
        var root=text.CopyRanges();
        if(!LegacyOwnedRawInkRangeOperations.Equal(union,root))
            throw new NotSupportedException("Native0248D0 unequal source-array branch requires238B0 processing; it is not bypassed by containment.");
        token.ThrowIfCancellationRequested();return new(actualLines,union,root,nodes.Count);
    }
    public LegacyNativeInkPositionRange[] CopyWordRanges()=>wordRanges.ToArray();
    public LegacyNativeInkPositionRange[] CopyTextRanges()=>textRanges.ToArray();
    public byte[] CopySourceLineStorage56()=>LineConsistency.CopyStorage56();
}
