namespace InkRecognition;

/// <summary>Bounded025E40 after actual pending TEXT registration. All actual
/// WORD/CHAR choices belong to one assigned source line, so025940's insertion
/// predicate is false. The returned line storage is a new immutable snapshot;
/// this does not apply selections or complete0283B0 document acceptance.</summary>
public sealed class LegacyFreshMainDocumentLineConsistency
{
    private readonly byte[] storage;
    private readonly LegacyNativePublishedSourceLine[] lines;
    public LegacyFreshMainDocumentTextSelection Text {get;}
    public LegacyFreshMainSourceLinePublication OriginalSourceLines=>Text.SourceLines;
    public LegacyOwnedMainWordSourceChoicesResult WordChoices=>Text.WordChoices;
    public IReadOnlyList<LegacyNativePublishedSourceLine> Lines {get;}
    public int LineCount=>lines.Length;
    public int LastReferencedLine {get;}
    public int WordVisits {get;}
    public int HelperCalls025940 {get;}
    public int AdjacentCharacterChecks {get;}

    private LegacyFreshMainDocumentLineConsistency(LegacyFreshMainDocumentTextSelection text,
        int lastLine,int visits,int helperCalls,int adjacentChecks)
    {
        Text=text;LastReferencedLine=lastLine;WordVisits=visits;
        HelperCalls025940=helperCalls;AdjacentCharacterChecks=adjacentChecks;
        lines=text.SourceLines.Lines.Take(lastLine+1).ToArray();
        Lines=Array.AsReadOnly(lines);storage=LegacyFreshMainSourceLinePublication.Encode(lines);
    }

    public static LegacyFreshMainDocumentLineConsistency Apply(
        LegacyFreshMainDocumentTextSelection actualText,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualText);token.ThrowIfCancellationRequested();
        if(!actualText.IsRegistered||actualText.IsFaulted)
            throw new ArgumentException("Actual successful pending TEXT registration must precede025E40.");
        var words=actualText.WordChoices;var source=words.Source;
        foreach(var word in words.Owner.WordViews)word.Validate(token);
        var commands=actualText.CopyPendingSelections();var prefix=words.PendingSelections;
        if(commands.Count!=prefix.Count+1||!ReferenceEquals(commands[^1],actualText.Selection)||
            !commands.Take(prefix.Count).SequenceEqual(prefix)||actualText.SourceLines.LineCount!=2)
            throw new ArgumentException("Exact current CHAR/WORD/TEXT command prefix and fresh source-line owner required.");
        //The sealed fresh WORD producer initializes payload40 to minus-one;
        //no API changes it.025E40 therefore uses each actual279 selected index.
        if((source.Flags&0x200000)!=0||source.Choices.Count==0)
            throw new NotSupportedException("Ordinary nonempty actual WORD source required.");
        int assigned=words.Owner.Sources.CandidateSource.Sampled.Owner.SourceLineId;
        if(assigned!=0||actualText.SourceLines.Sampled.Owner.SourceLineId!=assigned)
            throw new NotSupportedException("One actual assigned source line0 required; split/insertion is not supported.");
        foreach(var choice in source.Choices)
        {
            token.ThrowIfCancellationRequested();choice.Word.Validate(token);
            if(choice.CharacterLinks.Count==0||!ReferenceEquals(choice.Word.Owner,words.Owner))
                throw new ArgumentException("Actual nonempty WORD/CHAR ownership required.");
            foreach(var link in choice.CharacterLinks)
            {
                token.ThrowIfCancellationRequested();
                if(link.Source.SourceLineId!=assigned||
                    !ReferenceEquals(link.Source.SourceNode.Owner.Document,actualText.Document))
                    throw new NotSupportedException("All actual CHAR source owners must reference the same assigned line;025940 insertion is not implemented.");
            }
        }
        int max=-1,visits=0,calls=0,adjacent=0;
        foreach(var alternative in words.Alternatives)
        {
            token.ThrowIfCancellationRequested();alternative.Candidate.Validate(token);
            if(!ReferenceEquals(alternative.Source,source)||
                !ReferenceEquals(alternative.Candidate.TopOwner,words.Owner.TopOwner))
                throw new ArgumentException("Actual native-order shared WORD alternatives required.");
            var selected=source.Choices[alternative.SelectedWordIndex];
            int last=selected.CharacterLinks[^1].Source.SourceLineId;max=Math.Max(max,last);visits++;
            if((source.Flags&0x1000)!=0)continue;
            //025E40 scans every WORD choice's last CHAR in reverse. The full
            //owner gate above proves all last lines equal:024A90 is unreachable.
            for(int i=source.Choices.Count-1;i>=0;i--)
                if(source.Choices[i].CharacterLinks[^1].Source.SourceLineId!=last)
                    throw new NotSupportedException("Native WORD split branch is not implemented.");
            calls++;
            //025940 compares adjacent CHAR line IDs backwards in every choice.
            //Equality bypasses all range allocations, flag writes and insertions.
            foreach(var choice in source.Choices)
                for(int i=choice.CharacterLinks.Count-1;i>0;i--)
                {
                    token.ThrowIfCancellationRequested();adjacent++;
                    if(choice.CharacterLinks[i-1].Source.SourceLineId!=choice.CharacterLinks[i].Source.SourceLineId)
                        throw new NotSupportedException("Native adjacent-line insertion branch is not implemented.");
                }
        }
        if(max!=assigned)throw new InvalidDataException("Actual selected WORD line did not resolve to the assigned guide.");
        //025FD8..026015 truncates source2DA.F8/count100 after max+1. The
        //previous021AB0 snapshot remains untouched, including its zero tail.
        return new(actualText,max,visits,calls,adjacent);
    }
    public byte[] CopyStorage56()=>storage.ToArray();
}
