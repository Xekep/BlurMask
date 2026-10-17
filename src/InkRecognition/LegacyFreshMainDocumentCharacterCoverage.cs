namespace InkRecognition;

/// <summary>One actual01C740 CHAR subtraction input. Fresh CHAR payload38 is
/// zero, so its original interned key20/28 is used, not a current glyph's
/// possibly equal-but-differently-owned source projection.</summary>
public sealed class LegacyOwnedMainCharacterCoverageInput
{
    private readonly LegacyNativeInkPositionRange[] ranges;
    public LegacyOwnedMainTextSourcePayload Source {get;}
    public int NativeCharacterIndex {get;}
    public bool UsesPriorRanges=>false;
    internal LegacyOwnedMainCharacterCoverageInput(LegacyOwnedMainTextSourcePayload source,int index,
        LegacyNativeInkPositionRange[] actualRanges)
    {Source=source;NativeCharacterIndex=index;ranges=actualRanges;}
    public LegacyNativeInkPositionRange[] CopyRanges()=>ranges.ToArray();
}

/// <summary>A native-order01C740 call over one actual48-byte WORD choice.</summary>
public sealed class LegacyOwnedMainWordCharacterCoverage
{
    private readonly LegacyNativeInkPositionRange[] initial,remaining;
    public LegacyOwnedMainWordSourceChoiceRow WordChoice {get;}
    public int NativeWordChoiceIndex {get;}
    public bool UsesOrdinaryWordRanges {get;}
    public IReadOnlyList<LegacyOwnedMainCharacterCoverageInput> Characters {get;}
    public int RemainingRangeCount=>remaining.Length;
    internal LegacyOwnedMainWordCharacterCoverage(LegacyOwnedMainWordSourceChoiceRow choice,int index,
        bool ordinary,LegacyNativeInkPositionRange[] initial,LegacyNativeInkPositionRange[] remaining,
        LegacyOwnedMainCharacterCoverageInput[] characters)
    {WordChoice=choice;NativeWordChoiceIndex=index;UsesOrdinaryWordRanges=ordinary;
     this.initial=initial;this.remaining=remaining;Characters=Array.AsReadOnly(characters);}
    public LegacyNativeInkPositionRange[] CopyInitialRanges()=>initial.ToArray();
    public LegacyNativeInkPositionRange[] CopyRemainingRanges()=>remaining.ToArray();
}

/// <summary>Owning0283B0 per-WORD01C740 source-minus-CHAR scan after0248D0.
/// The multiple-choice nonempty-remainder024A90 split branch is unsupported.
/// All previous source flags, choice vectors, commands and line snapshots
/// remain unchanged; this is not document acceptance.</summary>
public sealed class LegacyFreshMainDocumentCharacterCoverage
{
    public LegacyFreshMainDocumentSourceCoverage SourceCoverage {get;}
    public LegacyFreshMainDocumentTextSelection Text=>SourceCoverage.Text;
    public IReadOnlyList<LegacyOwnedMainWordCharacterCoverage> Words {get;}
    public int CharacterSubtractionCount {get;}
    private LegacyFreshMainDocumentCharacterCoverage(LegacyFreshMainDocumentSourceCoverage source,
        LegacyOwnedMainWordCharacterCoverage[] words,int characterCount)
    {SourceCoverage=source;Words=Array.AsReadOnly(words);CharacterSubtractionCount=characterCount;}

    public static LegacyFreshMainDocumentCharacterCoverage Prepare(
        LegacyFreshMainDocumentSourceCoverage actualSource,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualSource);token.ThrowIfCancellationRequested();
        var text=actualSource.Text;var choices=text.WordChoices;var source=choices.Source;
        foreach(var word in choices.Owner.WordViews)word.Validate(token);
        var commands=text.CopyPendingSelections();var prefix=choices.PendingSelections;
        if(actualSource.WordSourceNodeCount!=1||!text.IsRegistered||text.IsFaulted||commands.Count!=prefix.Count+1||
            !ReferenceEquals(commands[^1],text.Selection)||!commands.Take(prefix.Count).SequenceEqual(prefix))
            throw new ArgumentException("Exact current post0248D0 shared WORD/CHAR/TEXT source owner required.");
        if((source.Flags&0x200000)!=0)throw new NotSupportedException("Ordinary actual WORD source required.");
        var operations=LegacyOwnedRawInkRangeOperations.Prepare(text.Document);
        //WORD payload30/count38 is the accumulated ordinary-range vector;
        //when empty,01C740 instead copies its original key20/count28.
        var ordinary=source.CopyOrdinaryRanges();bool useOrdinary=ordinary.Length!=0;
        var initial=useOrdinary?ordinary:source.CopySourceKeyRanges();
        var results=new List<LegacyOwnedMainWordCharacterCoverage>();int count=0;
        //0296F9..029783 visits WORD choices backwards, while01C740 visits
        //each selected choice's CHAR vector forwards. Preserve both orders.
        for(int index=source.Choices.Count-1;index>=0;index--)
        {
            token.ThrowIfCancellationRequested();var choice=source.Choices[index];choice.Word.Validate(token);
            if(!ReferenceEquals(choice.Word.Owner,choices.Owner))throw new ArgumentException("Foreign actual WORD choice.");
            //The caller clears temporary count withC21.V78/07F820 first.
            //V88/07FAD0 ->07F830 then appends an exact vector copy, without
            //union/canonicalization before the first subtraction.
            var remaining=initial.ToArray();var characters=new List<LegacyOwnedMainCharacterCoverageInput>();
            for(int i=0;i<choice.CharacterLinks.Count;i++)
            {
                token.ThrowIfCancellationRequested();var character=choice.CharacterLinks[i].Source;
                if(character.PriorRangeCount!=0)
                    throw new NotSupportedException("A prior CHAR range owner is not available in the fresh source scope.");
                var original=character.SourceNode.FirstProcessedSource??
                    throw new NotSupportedException("Actual original interned CHAR source key required.");
                if(!ReferenceEquals(character.SourceNode.Owner.Document,text.Document))
                    throw new ArgumentException("CHAR key belongs to another raw document.");
                var key=original.CopyRanges();
                characters.Add(new(character,i,key));
                remaining=operations.Subtract(remaining,key,token);count++;
            }
            //Native keeps a sole WORD choice even if ranges remain. With
            //multiple choices, a nonempty result requires real024A90 splitting.
            if(remaining.Length!=0&&source.Choices.Count!=1)
                throw new NotSupportedException("Actual nonempty WORD-minus-CHAR ranges require024A90 splitting; no source is silently dropped.");
            results.Add(new(choice,index,useOrdinary,initial.ToArray(),remaining,characters.ToArray()));
        }
        token.ThrowIfCancellationRequested();return new(actualSource,results.ToArray(),count);
    }
}
